// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Xml;
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
    /// Executes the specified SQL statement and returns an <see cref="XmlReader" /> to read the statement result
    /// set as XML.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// An instance of <see cref="XmlReader" /> that can be used to read the statement result set as XML.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteXmlReader()" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get an <see cref="XmlReader" /> to read the result set as XML:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var productsXmlReader = connection.ExecuteXmlReader(
    ///     """
    ///     SELECT  *
    ///     FROM    Product
    ///     FOR     XML AUTO
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
    /// var lowStockProductsXmlReader = connection.ExecuteXmlReader(
    ///    $"""
    ///     SELECT  *
    ///     FROM    Product
    ///     WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    ///     FOR     XML AUTO
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
    /// var retiredSupplierProductsXmlReader = connection.ExecuteXmlReader(
    ///    $"""
    ///     SELECT  *
    ///     FROM    Product
    ///     WHERE   SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///     FOR     XML AUTO
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
    /// var productsOrderedInPastSixMonthsXmlReader = connection.ExecuteXmlReader(
    ///     $"""
    ///      SELECT     *
    ///      FROM       Product
    ///      WHERE      EXISTS (
    ///                     SELECT  1
    ///                     FROM    {TemporaryTable(orderItems)} TOrderItem
    ///                     WHERE   TOrderItem.ProductId = Product.Id AND
    ///                             TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
    ///                 )
    ///      FOR        XML AUTO
    ///      """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    public static XmlReader ExecuteXmlReader(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        TimeSpan? commandTimeout = null,
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

        try
        {
            var xmlReader = command.ExecuteXmlReader();

            var disposeSignalingDecorator = new DisposeSignalingXmlReaderDecorator(xmlReader);

            // ReSharper disable AccessToDisposedClosure
            disposeSignalingDecorator.OnDisposing = () => commandDisposer.Dispose();
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
    /// Asynchronously executes the specified SQL statement and returns an <see cref="XmlReader" /> to read the
    /// statement result set as XML.
    /// </summary>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain an instance of <see cref="XmlReader" /> that can be used to
    /// read the statement result set as XML.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// See <see cref="SqlCommand.ExecuteXmlReaderAsync(System.Threading.CancellationToken)" /> for additional
    /// exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get an XML reader to read the result set as XML:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var productsXmlReader = await connection.ExecuteXmlReaderAsync(
    ///     """
    ///     SELECT  *
    ///     FROM    Product
    ///     FOR     XML AUTO
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
    /// var lowStockProductsXmlReader = await connection.ExecuteXmlReaderAsync(
    ///    $"""
    ///     SELECT  *
    ///     FROM    Product
    ///     WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    ///     FOR     XML AUTO
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
    /// var retiredSupplierProductsXmlReader = await connection.ExecuteXmlReaderAsync(
    ///    $"""
    ///     SELECT  *
    ///     FROM    Product
    ///     WHERE   SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///     FOR     XML AUTO
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
    /// var productsOrderedInPastSixMonthsXmlReader = await connection.ExecuteXmlReaderAsync(
    ///     $"""
    ///      SELECT     *
    ///      FROM       Product
    ///      WHERE      EXISTS (
    ///                     SELECT  1
    ///                     FROM    {TemporaryTable(orderItems)} TOrderItem
    ///                     WHERE   TOrderItem.ProductId = Product.Id AND
    ///                             TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
    ///                 )
    ///      FOR        XML AUTO
    ///      """
    /// );
    /// ]]>
    /// </code>
    /// </example>
    public static async Task<XmlReader> ExecuteXmlReaderAsync(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        TimeSpan? commandTimeout = null,
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
            var xmlReader = await command.ExecuteXmlReaderAsync(cancellationToken).ConfigureAwait(false);

            var disposeSignalingDecorator = new DisposeSignalingXmlReaderDecorator(xmlReader);

            disposeSignalingDecorator.OnDisposing = () => commandDisposer.Dispose();

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
