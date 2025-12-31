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
    /// of value tuples of the type <typeparamref name="TValueTuple" />.
    /// </summary>
    /// <typeparam name="TValueTuple">
    /// The type of value tuples to materialize the result set to.
    /// Only value tuples with up to 7 fields are supported.
    /// </typeparam>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A sequence of value tuples of the type <typeparamref name="TValueTuple" /> containing the data of the result
    /// set returned by the statement.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <typeparamref name="TValueTuple" /> is not a <see cref="ValueTuple" /> type or a
    /// <see cref="ValueTuple" /> type with more than 7 fields.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The type <typeparamref name="TValueTuple" /> has a different number of fields than the number
    /// of columns returned by the statement.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 A column returned by the statement contains NULL and the corresponding field of the type
    /// <typeparamref name="TValueTuple" /> has a non-nullable field type.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 A column returned by the statement has a data type which is not compatible with the field type
    ///                 of the corresponding field of the type <typeparamref name="TValueTuple" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// Each row in the result set will be materialized into an instance of <typeparamref name="TValueTuple" />,
    /// with the fields of the value tuple being populated from the corresponding columns in the row.
    /// 
    /// The order of the fields in the value tuple must match the order of the columns in the result set.
    /// The data types of the columns must be compatible with the field types of the fields of the value tuple.
    /// 
    /// See <see cref="SqlCommand.ExecuteReader()" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get the returned rows as value tuples.
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var productUnitsInStockInfos = connection.QueryTuples<(Int64 ProductId, Int32 UnitsInStock)>(
    ///     """
    ///     SELECT  Id, UnitsInStock
    ///     FROM    Product
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
    /// var lowStockProductInfos = connection.QueryTuples<(Int64 ProductId, Int32 UnitsInStock)>(
    ///    $"""
    ///     SELECT  Id, UnitsInStock
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
    /// var retiredSupplierProductInfos = connection.QueryTuples<(Int64 ProductId, Int32 UnitsInStock)>(
    ///    $"""
    ///     SELECT  Id, UnitsInStock
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
    /// var productsOrderedInPastSixMonthsInfos = connection.QueryTuples<(Int64 ProductId, Int32 UnitsInStock)>(
    ///     $"""
    ///      SELECT     Id, UnitsInStock
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
    public static IEnumerable<TValueTuple> QueryTuples<TValueTuple>(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default
    ) where TValueTuple : struct, IStructuralEquatable, IStructuralComparable, IComparable
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
                var materializer = ValueTupleMaterializerFactory.GetMaterializer<TValueTuple>(reader);

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
    /// into a sequence of value tuples of the type <typeparamref name="TValueTuple" />.
    /// </summary>
    /// <typeparam name="TValueTuple">
    /// The type of value tuples to materialize the result set to.
    /// Only value tuples with up to 7 fields are supported.
    /// </typeparam>
    /// <param name="connection">The SQL connection to use to execute the statement.</param>
    /// <param name="statement">The SQL statement to execute.</param>
    /// <param name="transaction">The SQL transaction within to execute the statement.</param>
    /// <param name="commandTimeout">The timeout to use for the execution of the statement.</param>
    /// <param name="commandType">A value indicating how <paramref name="statement" /> is to be interpreted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// An async enumerable that represents the sequence of value tuples of the type
    /// <typeparamref name="TValueTuple" /> containing the data of the result set returned by the statement.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <typeparamref name="TValueTuple" /> is not a <see cref="ValueTuple" /> type or a
    /// <see cref="ValueTuple" /> type with more than 7 fields.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The type <typeparamref name="TValueTuple" /> has a different number of fields than the number
    /// of columns returned by the statement.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 A column returned by the statement contains NULL and the corresponding field of the type
    /// <typeparamref name="TValueTuple" /> has a non-nullable field type.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 A column returned by the statement has a data type which is not compatible with the field type
    ///                 of the corresponding field of the type <typeparamref name="TValueTuple" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The statement was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// Each row in the result set will be materialized into an instance of <typeparamref name="TValueTuple" />,
    /// with the fields of the value tuple being populated from the corresponding columns in the row.
    /// 
    /// The order of the fields in the value tuple must match the order of the columns in the result set.
    /// The data types of the columns must be compatible with the field types of the fields of the value tuple.
    /// 
    /// See <see cref="SqlCommand.ExecuteReader()" /> for additional exceptions this method may throw.
    /// </remarks>
    /// <example>
    /// Execute an SQL statement and get the returned rows as value tuples.
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var productUnitsInStockInfos = await connection.QueryTuplesAsync<(Int64 ProductId, Int32 UnitsInStock)>(
    ///     """
    ///     SELECT  Id, UnitsInStock
    ///     FROM    Product
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
    /// var lowStockProductInfos = await connection.QueryTuplesAsync<(Int64 ProductId, Int32 UnitsInStock)>(
    ///    $"""
    ///     SELECT  Id, UnitsInStock
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
    /// var retiredSupplierProductInfos = await connection.QueryTuplesAsync<(Int64 ProductId, Int32 UnitsInStock)>(
    ///    $"""
    ///     SELECT  Id, UnitsInStock
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
    /// var productsOrderedInPastSixMonthsInfos =
    ///     await connection.QueryTuplesAsync<(Int64 ProductId, Int32 UnitsInStock)>(
    ///         $"""
    ///          SELECT     Id, UnitsInStock
    ///          FROM       Product
    ///          WHERE      EXISTS (
    ///                         SELECT  1
    ///                         FROM    {TemporaryTable(orderItems)} TOrderItem
    ///                         WHERE   TOrderItem.ProductId = Product.Id AND
    ///                                 TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
    ///                     )
    ///              """
    ///     );
    /// ]]>
    /// </code>
    /// </example>
    public static async IAsyncEnumerable<TValueTuple> QueryTuplesAsync<TValueTuple>(
        this SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) where TValueTuple : struct, IStructuralEquatable, IStructuralComparable, IComparable
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
                var materializer = ValueTupleMaterializerFactory.GetMaterializer<TValueTuple>(reader);

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
