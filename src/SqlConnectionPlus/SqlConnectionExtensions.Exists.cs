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
    /// Executes the specified SQL statement and returns a <see cref="Boolean" /> indicating whether the result set
    /// returned by the statement contains at least one row.
    /// This method is intended to check for the existence of rows matching certain criteria, e.g. checking whether a
    /// Product with a specific Id exists.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// <see langword="true" /> if the result set returned by the statement contains at least one row; otherwise,
    /// <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteReader()" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and check if it returned at least one row.
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var existDiscontinuedProducts = connection.Exists(
    ///     """
    ///     SELECT  1
    ///     FROM    Product
    ///     WHERE   IsDiscontinued = 1
    ///     """
    /// );
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
    /// var existLowStockProducts = connection.Exists(
    ///    $"""
    ///     SELECT  1
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
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var existProductsOfRetiredSuppliers = connection.Exists(
    ///    $"""
    ///     SELECT  1
    ///     FROM    Product
    ///     WHERE   SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///     """
    /// );
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
    /// var existProductsOrderedInPastSixMonths = connection.Exists(
    ///     $"""
    ///      SELECT     1
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
    public static Boolean Exists(
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
                using var reader = command.ExecuteReader(CommandBehavior.Default);
                return reader.HasRows;
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
    /// Asynchronously executes the specified SQL statement and returns a <see cref="Boolean" /> indicating whether
    /// the result set returned by the statement contains at least one row.
    /// This method is intended to check for the existence of rows matching certain criteria,
    /// e.g. checking whether a Product with a specific Id exists.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain <see langword="true" /> if the result set returned by the
    /// statement contains at least one row; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteReaderAsync(System.Threading.CancellationToken)" /> for additional exceptions
    /// this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and check if it returned at least one row.
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var existDiscontinuedProducts = await connection.ExistsAsync(
    ///     """
    ///     SELECT  1
    ///     FROM    Product
    ///     WHERE   IsDiscontinued = 1
    ///     """
    /// );
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
    /// var existLowStockProducts = await connection.ExistsAsync(
    ///    $"""
    ///     SELECT  1
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
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var existProductsOfRetiredSuppliers = await connection.ExistsAsync(
    ///    $"""
    ///     SELECT  1
    ///     FROM    Product
    ///     WHERE   SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///     """
    /// );
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
    /// var existProductsOrderedInPastSixMonths = await connection.ExistsAsync(
    ///     $"""
    ///      SELECT     1
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
    public static async Task<Boolean> ExistsAsync(
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
#pragma warning disable CA2007
                await using var reader = await command.ExecuteReaderAsync(
                    CommandBehavior.Default,
                    cancellationToken
                ).ConfigureAwait(false);
#pragma warning restore CA2007

                return reader.HasRows;
            }
            catch (SqlException exception) when (
                SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
            )
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }
}
