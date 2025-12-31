// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.SqlServer;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using SqlCommandBuilder = RentADeveloper.SqlConnectionPlus.SqlCommands.SqlCommandBuilder;

namespace RentADeveloper.SqlConnectionPlus;

/// <summary>
/// Provides extension members for the type <see cref="SqlConnection" />.
/// </summary>
public static partial class SqlConnectionExtensions
{
    /// <summary>
    /// Executes the specified SQL statement and returns the first column of the first row in the result set returned
    /// by the statement converted to the type <typeparamref name="TTarget" />.
    /// Additional columns or rows are ignored.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// The first column of the first row in the result set converted to the type <typeparamref name="TTarget" />,
    /// or <see langword="default" /> of <typeparamref name="TTarget" /> if the result set is empty.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidCastException">
    /// The first column of the first row in the result set returned by the statement could not be converted to the
    /// type <typeparamref name="TTarget" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteScalar()" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get the first column of the first row in the result set.
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var numberOfDiscontinuedProducts = connection.ExecuteScalar<Int32>(
    ///     """
    ///     SELECT  COUNT(*)
    ///     FROM    Product
    ///     WHERE   IsDiccontinued = 1
    ///     """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// Pass a parameter via an interpolated string:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var lowStockThreshold = configuration.Thresholds.LowStock;
    /// 
    /// var numberOfLowStockProducts = connection.ExecuteScalar<Int32>(
    ///    $"""
    ///     SELECT  COUNT(*)
    ///     FROM    Product
    ///     WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    ///     """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// Pass a sequence of scalar values as a temporary table via an interpolated string:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var numberOfProductsOfRetiredSuppliers = connection.ExecuteScalar<Int32>(
    ///   $"""
    ///   SELECT    COUNT(*)
    ///   FROM      Product
    ///   WHERE     SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///   """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// Pass a sequence of complex objects as a temporary table via an interpolated string:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class OrderItem
    /// {
    ///     public Int64 ProductId { get; set; }
    ///     public DateTime OrderDate { get; set; }
    /// }
    /// 
    /// var orderItems = GetOrderItems();
    /// var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
    /// 
    /// var numberOfProductsOrderedInPastSixMonths = connection.ExecuteScalar<Int32>(
    ///     $"""
    ///      SELECT     COUNT(*)
    ///      FROM       Product
    ///      WHERE      EXISTS (
    ///                     SELECT  1
    ///                     FROM    {TemporaryTable(orderItems)} TOrderItem
    ///                     WHERE   TOrderItem.ProductId = Product.Id AND
    ///                             TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
    ///                 )
    ///      """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    public static TTarget ExecuteScalar<TTarget>(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(connection);

        var (command, commandDisposer) = SqlCommandBuilder.BuildSqlCommand(
            connection,
            statement,
            transaction,
            commandTimeout,
            commandType,
            cancellationToken
        );

        using (commandDisposer)
        {
            try
            {
                var value = command.ExecuteScalar();

                return value switch
                {
                    null => default!, // If the result set is empty, we get null and must return default of TTarget.
                    TTarget typedValue => typedValue,
                    _ => ConvertValueForExecuteScalar<TTarget>(value)
                };
            }
            catch (SqlException exception) when (
                SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
            )
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Asynchronously executes the specified SQL statement and returns the first column of the first row in the result
    /// set returned by the statement converted to the type <typeparamref name="TTarget" />.
    /// Additional columns or rows are ignored.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain the first column of the first row in the result set
    /// converted to the type <typeparamref name="TTarget" />, or <see langword="default" /> of
    /// <typeparamref name="TTarget" /> if the result set is empty.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidCastException">
    /// The first column of the first row in the result set returned by the statement could not be converted to the
    /// type <typeparamref name="TTarget" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteScalarAsync(System.Threading.CancellationToken)" /> for additional exceptions
    /// this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get the first column of the first row in the result set.
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var numberOfDiscontinuedProducts = await connection.ExecuteScalarAsync<Int32>(
    ///     """
    ///     SELECT  COUNT(*)
    ///     FROM    Product
    ///     WHERE   IsDiccontinued = 1
    ///     """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// Pass a parameter via an interpolated string:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var lowStockThreshold = configuration.Thresholds.LowStock;
    /// 
    /// var numberOfLowStockProducts = await connection.ExecuteScalarAsync<Int32>(
    ///    $"""
    ///     SELECT  COUNT(*)
    ///     FROM    Product
    ///     WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    ///     """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// Pass a sequence of scalar values as a temporary table via an interpolated string:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var numberOfProductsOfRetiredSuppliers = await connection.ExecuteScalarAsync<Int32>(
    ///   $"""
    ///   SELECT    COUNT(*)
    ///   FROM      Product
    ///   WHERE     SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///   """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// Pass a sequence of complex objects as a temporary table via an interpolated string:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class OrderItem
    /// {
    ///     public Int64 ProductId { get; set; }
    ///     public DateTime OrderDate { get; set; }
    /// }
    /// 
    /// var orderItems = await GetOrderItemsAsync();
    /// var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
    /// 
    /// var numberOfProductsOrderedInPastSixMonths = await connection.ExecuteScalarAsync<Int32>(
    ///     $"""
    ///      SELECT     COUNT(*)
    ///      FROM       Product
    ///      WHERE      EXISTS (
    ///                     SELECT  1
    ///                     FROM    {TemporaryTable(orderItems)} TOrderItem
    ///                     WHERE   TOrderItem.ProductId = Product.Id AND
    ///                             TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
    ///                 )
    ///      """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    public static async Task<TTarget> ExecuteScalarAsync<TTarget>(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(connection);

        var (command, commandDisposer) = await SqlCommandBuilder.BuildSqlCommandAsync(
            connection,
            statement,
            transaction,
            commandTimeout,
            commandType,
            cancellationToken
        ).ConfigureAwait(false);

        await using (commandDisposer)
        {
            try
            {
                var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                return value switch
                {
                    null => default!, // If the result set is empty, we get null and must return default of TTarget.
                    TTarget typedValue => typedValue,
                    _ => ConvertValueForExecuteScalar<TTarget>(value)
                };
            }
            catch (SqlException exception) when (
                SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
            )
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Converts <paramref name="value" /> to the type <typeparamref name="TTarget" />.
    /// If the conversion fails this method throws exceptions with messages matching the context of the methods
    /// <see cref="ExecuteScalar{TTarget}" /> and <see cref="ExecuteScalarAsync{TTarget}" />.
    /// </summary>
    /// <typeparam name="TTarget">The type to convert <paramref name="value" /> to.</typeparam>
    /// <param name="value">The value to convert to the type <typeparamref name="TTarget" />.</param>
    /// <returns><paramref name="value" /> converted to the type <typeparamref name="TTarget" />.</returns>
    /// <exception cref="InvalidCastException">
    /// <paramref name="value" /> could not be converted to the type <typeparamref name="TTarget" />.
    /// </exception>
    private static TTarget ConvertValueForExecuteScalar<TTarget>(Object? value)
    {
        try
        {
            return (TTarget)ValueConverter.ConvertValueToType<TTarget>(value)!;
        }
        catch (Exception exception) when (value is null or DBNull)
        {
            throw new InvalidCastException(
                $"The first column of the first row in the result set returned by the SQL statement contains NULL, " +
                $"which could not be converted to the type {typeof(TTarget)}. See inner exception for details.",
                exception
            );
        }
        catch (Exception exception) when (value is not null)
        {
            throw new InvalidCastException(
                $"The first column of the first row in the result set returned by the SQL statement contains " +
                $"the value {value.ToDebugString()}, which could not be converted to the type {typeof(TTarget)}. " +
                $"See inner exception for details.",
                exception
            );
        }
    }
}
