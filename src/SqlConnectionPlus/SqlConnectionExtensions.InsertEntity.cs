// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.Entities;
using RentADeveloper.SqlConnectionPlus.SqlServer;

namespace RentADeveloper.SqlConnectionPlus;

/// <summary>
/// Provides extension members for the type <see cref="SqlConnection" />.
/// </summary>
public static partial class SqlConnectionExtensions
{
    /// <summary>
    /// Inserts the specified entity into the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to insert.</typeparam>
    /// <param name="connection">The SQL connection to use to insert the entity.</param>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The number of rows that were affected by the insert operation.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="connection" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="entity" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// The table into which the entity will be inserted is determined by the <see cref="TableAttribute" />
    /// applied to the type <typeparamref name="TEntity" />.
    /// If this attribute is not present, the singular name of the type <typeparamref name="TEntity" /> is used.
    /// 
    /// The type <typeparamref name="TEntity" /> must have a property (with a public getter) denoted with a
    /// <see cref="KeyAttribute" />.
    /// 
    /// Each property (with a public getter) of the type <typeparamref name="TEntity" /> is mapped to a column with the
    /// same name (case-sensitive) in the table.
    /// The columns must have data types that are compatible with the property types of the corresponding properties.
    /// Properties denoted with the <see cref="NotMappedAttribute" /> are ignored.
    /// </remarks>
    /// <example>
    /// Insert an entity:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class Product
    /// {
    ///     [Key]
    ///     public Int64 Id { get; set; }
    ///     public Int64 SupplierId { get; set; }
    ///     public String Name { get; set; }
    ///     public Decimal UnitPrice { get; set; }
    ///     public Int32 UnitsInStock { get; set; }
    /// }
    /// 
    /// var newProduct = GetNewProduct();
    /// 
    /// connection.InsertEntity(newProduct);
    /// </code>
    /// </example>
    public static Int32 InsertEntity<TEntity>(
        this SqlConnection connection,
        TEntity entity,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(entity);

        var metadata = EntityHelper.GetEntityTypeMetadata<TEntity>();

        var (command, parameters) = EntitySqlCommandFactory.CreateInsertEntityCommand(
            connection,
            transaction,
            metadata
        );
        var cancellationTokenRegistration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        using (command)
        using (cancellationTokenRegistration)
        {
            EntityHelper.PopulateSqlParametersFromEntityProperties(metadata, parameters, entity);

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
    /// Asynchronously inserts the specified entity into the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to insert.</typeparam>
    /// <param name="connection">The SQL connection to use to insert the entity.</param>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain the number of rows that were affected by the insert operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="connection" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="entity" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// The table into which the entity will be inserted is determined by the <see cref="TableAttribute" />
    /// applied to the type <typeparamref name="TEntity" />.
    /// If this attribute is not present, the singular name of the type <typeparamref name="TEntity" /> is used.
    /// 
    /// Each property (with a public getter) of the type <typeparamref name="TEntity" /> is mapped to a column with the
    /// same name (case-sensitive) in the table.
    /// The columns must have data types that are compatible with the property types of the corresponding properties.
    /// Properties denoted with the <see cref="NotMappedAttribute" /> are ignored.
    /// </remarks>
    /// <example>
    /// Insert an entity:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class Product
    /// {
    ///     [Key]
    ///     public Int64 Id { get; set; }
    ///     public Int64 SupplierId { get; set; }
    ///     public String Name { get; set; }
    ///     public Decimal UnitPrice { get; set; }
    ///     public Int32 UnitsInStock { get; set; }
    /// }
    /// 
    /// var newProduct = await GetNewProductAsync();
    /// 
    /// await connection.InsertEntityAsync(newProduct);
    /// </code>
    /// </example>
    public static async Task<Int32> InsertEntityAsync<TEntity>(
        this SqlConnection connection,
        TEntity entity,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(entity);

        var metadata = EntityHelper.GetEntityTypeMetadata<TEntity>();

        var (command, parameters) = EntitySqlCommandFactory.CreateInsertEntityCommand(
            connection,
            transaction,
            metadata
        );
        var cancellationTokenRegistration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        await using (command)
        await using (cancellationTokenRegistration)
        {
            EntityHelper.PopulateSqlParametersFromEntityProperties(metadata, parameters, entity);

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
