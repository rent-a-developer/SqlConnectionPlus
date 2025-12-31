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
    /// Executes the specified SQL statement and returns the number of rows affected by the statement.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The number of rows affected by the statement.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteNonQuery" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var numberOfDeletedProducts = connection.ExecuteNonQuery(
    ///     """
    ///     DELETE FROM Product
    ///     WHERE       IsDiscontinued = 1
    ///     """
    /// );
    /// </code>
    /// </example>
    /// <example>
    /// Pass a parameter via an interpolated string:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// if (supplier.IsRetired)
    /// {
    ///     var numberOfDeletedProducts = connection.ExecuteNonQuery(
    ///        $"""
    ///         DELETE FROM Product
    ///         WHERE       SupplierId = {Parameter(supplier.Id)}
    ///         """
    ///     );
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// Pass a sequence of scalar values as a temporary table via an interpolated string:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var numberOfDeletedProducts = connection.ExecuteNonQuery(
    ///    $"""
    ///     DELETE FROM Product
    ///     WHERE       SupplierId IN (
    ///                     SELECT  Value
    ///                     FROM    {TemporaryTable(retiredSupplierIds)}
    ///                 )
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
    /// // Delete products that have not been ordered in the past six months.
    /// var numberOfDeletedProducts = connection.ExecuteNonQuery(
    ///    $"""
    ///     DELETE FROM Product
    ///     WHERE       NOT EXISTS (
    ///                     SELECT  1
    ///                     FROM    {TemporaryTable(orderItems)} TOrderItem
    ///                     WHERE   TOrderItem.ProductId = Product.Id AND
    ///                             TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
    ///                 )
    ///     """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    public static Int32 ExecuteNonQuery(
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
                return command.ExecuteNonQuery();
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
    /// Asynchronously executes the specified SQL statement and returns the number of rows affected by the statement.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain the number of rows affected by the statement.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteNonQueryAsync(System.Threading.CancellationToken)" /> for additional
    /// exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var numberOfDeletedProducts = await connection.ExecuteNonQueryAsync(
    ///     """
    ///     DELETE FROM Product
    ///     WHERE       IsDiscontinued = 1
    ///     """
    /// );
    /// </code>
    /// </example>
    /// <example>
    /// Pass a parameter via an interpolated string:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// if (supplier.IsRetired)
    /// {
    ///     var numberOfDeletedProducts = await connection.ExecuteNonQueryAsync(
    ///        $"""
    ///         DELETE FROM Product
    ///         WHERE       SupplierId = {Parameter(supplier.Id)}
    ///         """
    ///     );
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// Pass a sequence of scalar values as a temporary table via an interpolated string:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var numberOfDeletedProducts = await connection.ExecuteNonQueryAsync(
    ///    $"""
    ///     DELETE FROM Product
    ///     WHERE       SupplierId IN (
    ///                     SELECT  Value
    ///                     FROM    {TemporaryTable(retiredSupplierIds)}
    ///                 )
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
    /// // Delete products that have not been ordered in the past six months.
    /// var numberOfDeletedProducts = await connection.ExecuteNonQueryAsync(
    ///    $"""
    ///     DELETE FROM Product
    ///     WHERE       NOT EXISTS (
    ///                     SELECT  1
    ///                     FROM    {TemporaryTable(orderItems)} TOrderItem
    ///                     WHERE   TOrderItem.ProductId = Product.Id AND
    ///                             TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
    ///                 )
    ///     """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    public static async Task<Int32> ExecuteNonQueryAsync(
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
                return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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
