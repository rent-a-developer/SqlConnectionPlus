// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.Readers;
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
    /// Executes the specified SQL statement and returns a <see cref="DbDataReader" /> to read the statement result
    /// set.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandBehavior">
    /// The command behavior to be passed to <see cref="SqlCommand.ExecuteReader(System.Data.CommandBehavior)" />.
    /// </param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// An instance of <see cref="DbDataReader" /> that can be used to read the statement result set.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteReader()" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get a <see cref="DbDataReader" /> to read the result set:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var productsReader = connection.ExecuteReader(
    ///     """
    ///     SELECT  *
    ///     FROM    Product
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
    /// var lowStockProductsReader = connection.ExecuteReader(
    ///    $"""
    ///     SELECT  *
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
    /// var retiredSupplierProductsReader = connection.ExecuteReader(
    ///    $"""
    ///     SELECT  *
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
    /// var productsOrderedInPastSixMonthsReader = connection.ExecuteReader(
    ///     $"""
    ///      SELECT     *
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
    public static DbDataReader ExecuteReader(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandBehavior commandBehavior = CommandBehavior.Default,
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
            commandType, cancellationToken
        );

        try
        {
            var dataReader = command.ExecuteReader(commandBehavior);

            var disposeSignalingDecorator = new DisposeSignalingDataReaderDecorator(dataReader, cancellationToken);

            // ReSharper disable AccessToDisposedClosure
            disposeSignalingDecorator.OnDisposing = () => commandDisposer.Dispose();

            disposeSignalingDecorator.OnDisposingAsync = async () => 
                await commandDisposer.DisposeAsync().ConfigureAwait(false);
            // ReSharper restore AccessToDisposedClosure

            return disposeSignalingDecorator;
        }
        catch (SqlException exception) when (
            SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
        )
        {
            commandDisposer.Dispose();

            throw new OperationCanceledException(cancellationToken);
        }
    }

    /// <summary>
    /// Asynchronously executes the specified SQL statement and returns a <see cref="DbDataReader" /> to read the
    /// statement result set.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandBehavior">
    /// The command behavior to be passed to
    /// <see cref="SqlCommand.ExecuteReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" />.
    /// </param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain an instance of <see cref="DbDataReader" /> that can be used
    /// to read the statement result set.
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
    /// Execute an SQL statement and get a <see cref="DbDataReader" /> to read the result set:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var productsReader = await connection.ExecuteReaderAsync(
    ///     """
    ///     SELECT  *
    ///     FROM    Product
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
    /// var lowStockProductsReader = await connection.ExecuteReaderAsync(
    ///    $"""
    ///     SELECT  *
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
    /// var retiredSupplierProductsReader = await connection.ExecuteReaderAsync(
    ///    $"""
    ///     SELECT  *
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
    /// var productsOrderedInPastSixMonthsReader = await connection.ExecuteReaderAsync(
    ///     $"""
    ///      SELECT     *
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
    public static async Task<DbDataReader> ExecuteReaderAsync(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandBehavior commandBehavior = CommandBehavior.Default,
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

        try
        {
            var dataReader = await command.ExecuteReaderAsync(commandBehavior, cancellationToken).ConfigureAwait(false);

            var disposeSignalingDecorator = new DisposeSignalingDataReaderDecorator(dataReader, cancellationToken);

            disposeSignalingDecorator.OnDisposing = () => commandDisposer.Dispose();

            disposeSignalingDecorator.OnDisposingAsync = async () => 
                await commandDisposer.DisposeAsync().ConfigureAwait(false);

            return disposeSignalingDecorator;
        }
        catch (SqlException exception) when (
            SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
        )
        {
            await commandDisposer.DisposeAsync().ConfigureAwait(false);

            throw new OperationCanceledException(cancellationToken);
        }
    }
}
