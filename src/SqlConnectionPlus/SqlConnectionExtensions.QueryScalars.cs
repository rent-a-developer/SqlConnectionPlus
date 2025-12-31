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
    /// Executes the specified SQL statement and returns the values of the first column of the result set returned by
    /// the statement converted to a sequence of values of the type <typeparamref name="TTarget" />.
    /// </summary>
    /// <typeparam name="TTarget">
    /// The type to convert the values of the first column of the result set of the statement to.
    /// </typeparam>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// The values of the first column of the result set returned by the statement converted to a sequence of values
    /// of the type <typeparamref name="TTarget" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidCastException">
    /// The first column of the result set returned by the statement contains a value that could not be converted to
    /// the type <typeparamref name="TTarget" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteReader()" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get the values of the first column of the result set.
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var discontinuedProductIds = connection.QueryScalars<Int64>(
    ///     """
    ///     SELECT  Id
    ///     FROM    Product
    ///     WHERE   IsDiscontinued = 1
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
    /// var lowStockProductIds = connection.QueryScalars<Int64>(
    ///    $"""
    ///     SELECT  Id
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
    /// var idsOfProductsOfRetiredSuppliers = connection.QueryScalars<Int64>(
    ///   $"""
    ///   SELECT    Id
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
    /// var idsOfProductsOrderedInPastSixMonths = connection.QueryScalars<Int64>(
    ///     $"""
    ///      SELECT     Id
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
    public static IEnumerable<TTarget> QueryScalars<TTarget>(
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
            DbDataReader reader;

            try
            {
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            }
            catch (SqlException exception) when (
                SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
            )
            {
                throw new OperationCanceledException(cancellationToken);
            }

            using (reader)
            {
                // We can't use "while (reader.Read())" here because, the "yield return" statement cannot be placed
                // inside a try/catch block.
                while (true)
                {
                    try
                    {
                        if (!reader.Read())
                        {
                            yield break;
                        }
                    }
                    catch (SqlException exception) when (
                        SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
                    )
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }

                    var value = reader.GetValue(0);

                    if (value is TTarget typedValue)
                    {
                        yield return typedValue;
                    }
                    else
                    {
                        yield return ConvertValueForQueryScalars<TTarget>(value);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Asynchronously executes the specified SQL statement and returns the values of the first column of the result
    /// set returned by the statement converted to a sequence of values of the type <typeparamref name="TTarget" />.
    /// </summary>
    /// <typeparam name="TTarget">
    /// The type to convert the values of the first column of the result set of the statement to.
    /// </typeparam>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// An async enumerable that represents the values of the first column of the result set returned by the statement
    /// converted to a sequence of values of the type <typeparamref name="TTarget" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidCastException">
    /// The first column of the result set returned by the statement contains a value that could not be converted to
    /// the type <typeparamref name="TTarget" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteReaderAsync(System.Threading.CancellationToken)" /> for additional exceptions
    /// this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get the values of the first column of the result set.
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var discontinuedProductIds = await connection.QueryScalarsAsync<Int64>(
    ///     """
    ///     SELECT  Id
    ///     FROM    Product
    ///     WHERE   IsDiscontinued = 1
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
    /// var lowStockProductIds = await connection.QueryScalarsAsync<Int64>(
    ///    $"""
    ///     SELECT  Id
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
    /// var idsOfProductsOfRetiredSuppliers = await connection.QueryScalarsAsync<Int64>(
    ///   $"""
    ///   SELECT    Id
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
    /// var idsOfProductsOrderedInPastSixMonths = await connection.QueryScalarsAsync<Int64>(
    ///     $"""
    ///      SELECT     Id
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
    public static async IAsyncEnumerable<TTarget> QueryScalarsAsync<TTarget>(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
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
            DbDataReader reader;

            try
            {
                reader = await command.ExecuteReaderAsync(
                    CommandBehavior.SequentialAccess,
                    cancellationToken
                ).ConfigureAwait(false);
            }
            catch (SqlException exception) when (
                SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
            )
            {
                throw new OperationCanceledException(cancellationToken);
            }

            await using (reader)
            {
                // We can't use "while (await reader.ReadAsync())" here because, the "yield return" statement cannot
                // be placed inside a try/catch block.
                while (true)
                {
                    try
                    {
                        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            yield break;
                        }
                    }
                    catch (SqlException exception) when (
                        SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
                    )
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }

                    var value = reader.GetValue(0);

                    if (value is TTarget typedValue)
                    {
                        yield return typedValue;
                    }
                    else
                    {
                        yield return ConvertValueForQueryScalars<TTarget>(value);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Converts <paramref name="value" /> to the type <typeparamref name="TTarget" />.
    /// If the conversion fails this method throws exceptions with messages matching the context of the methods
    /// <see cref="QueryScalars{TTarget}" /> and <see cref="QueryScalarsAsync{TTarget}" />.
    /// </summary>
    /// <typeparam name="TTarget">The type to convert <paramref name="value" /> to.</typeparam>
    /// <param name="value">The value to convert to the type <typeparamref name="TTarget" />.</param>
    /// <returns><paramref name="value" /> converted to the type <typeparamref name="TTarget" />.</returns>
    /// <exception cref="InvalidCastException">
    /// <paramref name="value" /> could not be converted to the type <typeparamref name="TTarget" />.
    /// </exception>
    private static TTarget ConvertValueForQueryScalars<TTarget>(Object? value)
    {
        try
        {
            return (TTarget)ValueConverter.ConvertValueToType<TTarget>(value)!;
        }
        catch (Exception exception) when (value is DBNull)
        {
            throw new InvalidCastException(
                $"The first column returned by the SQL statement contains NULL, which could not be converted to the " +
                $"type {typeof(TTarget)}. See inner exception for details.",
                exception
            );
        }
        catch (Exception exception) when (value is not null)
        {
            throw new InvalidCastException(
                $"The first column returned by the SQL statement contains the value {value.ToDebugString()}, which " +
                $"could not be converted to the type {typeof(TTarget)}. See inner exception for details.",
                exception
            );
        }
    }
}
