// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.Materializers;
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
    /// Executes the specified SQL statement and materializes the result set returned by the statement into a sequence
    /// of instances of the type <typeparamref name="TEntity" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of objects to materialize the result set to.</typeparam>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A sequence of instances of the type <typeparamref name="TEntity" /> containing the data of the result set
    /// returned by the statement.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    /// The statement returned a column for which no matching property (with a public setter) exists in the type
    /// <typeparamref name="TEntity" />.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// A column value returned by the statement could not be converted to the property type of the corresponding
    /// property of the type <typeparamref name="TEntity" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// The type <typeparamref name="TEntity" /> must have properties (with public setters) that match the names
    /// (case-sensitive) and data types of the columns returned by the statement.
    /// 
    /// Each row in the result set will be materialized into an instance of <typeparamref name="TEntity" />,
    /// with the properties being populated from the corresponding columns in the row.
    /// The data types of the columns must be compatible with the property types of the properties.
    /// 
    /// If the statement returns a column that does not have a corresponding property in the type
    /// <typeparamref name="TEntity" />, an <see cref="ArgumentException" /> will be thrown.
    /// 
    /// If a column value returned by the statement could not be converted to the property type of the corresponding
    /// property of the type <typeparamref name="TEntity" />, an <see cref="InvalidCastException" /> will be thrown.
    /// 
    /// See <see cref="SqlCommand.ExecuteReader()" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get the result set as a sequence of entities:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// public enum OrderStatus : Int32
    /// {
    ///     Pending = 1,
    ///     Processing = 2,
    ///     Shipped = 3,
    ///     Delivered = 4,
    ///     Cancelled = 5
    /// }
    /// 
    /// public class Order
    /// {
    ///     [Key]
    ///     public Int64 Id { get; set; }
    ///     public DateTime OrderDate { get; set; }
    ///     public Decimal TotalAmount { get; set; }
    ///     public OrderStatus Status { get; set; }
    /// }
    /// 
    /// var orders = connection.QueryEntities<Order>(
    ///     """
    ///     SELECT  *
    ///     FROM    [Order]
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
    /// var lowStockProducts = connection.QueryEntities<Product>(
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
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var retiredSupplierProducts = connection.QueryEntities<Product>(
    ///    $"""
    ///     SELECT  *
    ///     FROM    Product
    ///     WHERE   SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///     """
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
    /// var productsOrderedInPastSixMonths = connection.QueryEntities<Product>(
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
    public static IEnumerable<TEntity> QueryEntities<TEntity>(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default
    ) where TEntity : new()
    {
        ArgumentNullException.ThrowIfNull(connection);

        var (command, commandDisposer) = SqlCommandBuilder.BuildSqlCommand(connection,
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
                var materializer = EntityMaterializerFactory.GetMaterializer<TEntity>(reader);

                // We can't use "while (reader.Read())" here because, the "yield return" statement cannot be placed
                // inside a try/catch block.
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

                    yield return materializer(reader);
                }
            }
        }
    }

    /// <summary>
    /// Asynchronously executes the specified SQL statement and materializes the result set returned by the statement
    /// into a sequence of instances of the type <typeparamref name="TEntity" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of objects to materialize the result set to.</typeparam>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// An async enumerable that represents the sequence of instances of the type <typeparamref name="TEntity" />
    /// containing the data of the result set returned by the statement.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    /// The statement returned a column for which no matching property (with a public setter) exists in the type
    /// <typeparamref name="TEntity" />.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// A column value returned by the statement could not be converted to the property type of the corresponding
    /// property of the type <typeparamref name="TEntity" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// The type <typeparamref name="TEntity" /> must have properties (with public setters) that match the names
    /// (case-sensitive) and data types of the columns returned by the statement.
    /// 
    /// Each row in the result set will be materialized into an instance of <typeparamref name="TEntity" />,
    /// with the properties being populated from the corresponding columns in the row.
    /// The data types of the columns must be compatible with the property types of the properties.
    /// 
    /// If the statement returns a column that does not have a corresponding property in the type
    /// <typeparamref name="TEntity" />, an <see cref="ArgumentException" /> will be thrown.
    /// 
    /// If a column value returned by the statement could not be converted to the property type of the corresponding
    /// property of the type <typeparamref name="TEntity" />, an <see cref="InvalidCastException" /> will be thrown.
    /// 
    /// See <see cref="SqlCommand.ExecuteReaderAsync(System.Threading.CancellationToken)" /> for additional exceptions
    /// this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get the result set as a sequence of entities:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// public enum OrderStatus : Int32
    /// {
    ///     Pending = 1,
    ///     Processing = 2,
    ///     Shipped = 3,
    ///     Delivered = 4,
    ///     Cancelled = 5
    /// }
    /// 
    /// public class Order
    /// {
    ///     [Key]
    ///     public Int64 Id { get; set; }
    ///     public DateTime OrderDate { get; set; }
    ///     public Decimal TotalAmount { get; set; }
    ///     public OrderStatus Status { get; set; }
    /// }
    /// 
    /// var products = await connection.QueryEntitiesAsync<Order>(
    ///     """
    ///     SELECT  *
    ///     FROM    [Order]
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
    /// var lowStockProducts = await connection.QueryEntitiesAsync<Product>(
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
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var retiredSupplierProducts = await connection.QueryEntitiesAsync<Product>(
    ///    $"""
    ///     SELECT  *
    ///     FROM    Product
    ///     WHERE   SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///     """
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
    /// var productsOrderedInPastSixMonths = await connection.QueryEntitiesAsync<Product>(
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
    public static async IAsyncEnumerable<TEntity> QueryEntitiesAsync<TEntity>(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) where TEntity : new()
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
                var materializer = EntityMaterializerFactory.GetMaterializer<TEntity>(reader);

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

                    yield return materializer(reader);
                }
            }
        }
    }
}
