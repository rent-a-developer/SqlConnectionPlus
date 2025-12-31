// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.Entities;
using RentADeveloper.SqlConnectionPlus.SqlStatements;

namespace RentADeveloper.SqlConnectionPlus;

/// <summary>
/// Provides extension members for the type <see cref="SqlConnection" />.
/// </summary>
public static partial class SqlConnectionExtensions
{
    /// <summary>
    /// Deletes the specified entities identified by their key property from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities to delete.</typeparam>
    /// <param name="connection">The SQL connection to use to delete the entities.</param>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The number of rows affected by the delete operation.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="connection" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="entities" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// No property (with a public getter) of the type <typeparamref name="TEntity" /> is denoted with a
    /// <see cref="KeyAttribute" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// The table from which the entities will be deleted is determined by the <see cref="TableAttribute" />
    /// applied to the type <typeparamref name="TEntity" />.
    /// If this attribute is not present, the singular name of the type <typeparamref name="TEntity" /> is used.
    /// 
    /// The type <typeparamref name="TEntity" /> must have a property (with a public getter) denoted with a
    /// <see cref="KeyAttribute" />.
    /// </remarks>
    /// <example>
    /// Delete a sequence of entities:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class Product
    /// {
    ///     [Key]
    ///     public Int64 Id { get; set; }
    ///     public Boolean IsDiscontinued { get; set; }
    /// }
    /// 
    /// connection.DeleteEntities(products.Where(a => a.IsDiscontinued));
    /// </code>
    /// </example>
    public static Int32 DeleteEntities<TEntity>(
        this SqlConnection connection,
        IEnumerable<TEntity> entities,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(entities);

        // ReSharper disable once UseDeconstruction
        var metadata = EntityHelper.GetEntityTypeMetadata<TEntity>();
        var keyPropertyName = metadata.KeyPropertyName;
        var keyPropertyType = metadata.KeyPropertyType;
        var keyPropertyGetter = metadata.KeyPropertyGetter;
        var tableName = metadata.TableName;

        var keyValues = keyPropertyType.CreateListForType();

        foreach (var entity in entities)
        {
            var keyValue = keyPropertyGetter(entity);

            if (keyValue is null)
            {
                continue;
            }

            keyValues.Add(keyValue);
        }

        var keyValuesTable = new InterpolatedTemporaryTable(
            "#Keys_" + Guid.NewGuid().ToString("N"),
            keyValues,
            keyPropertyType
        );

        return connection.ExecuteNonQuery(
            $"""
             DELETE FROM    [{tableName}]
             WHERE          [{keyPropertyName}] IN 
                            (SELECT Value FROM {keyValuesTable})
             """,
            transaction: transaction,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Asynchronously deletes the specified entities identified by their key property from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities to delete.</typeparam>
    /// <param name="connection">The SQL connection to use to delete the entities.</param>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain the number of rows affected by the delete operation.
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
    ///                 <paramref name="entities" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// No property (with a public getter) of the type <typeparamref name="TEntity" /> is denoted with a
    /// <see cref="KeyAttribute" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    /// <remarks>
    /// The table from which the entities will be deleted is determined by the <see cref="TableAttribute" />
    /// applied to the type <typeparamref name="TEntity" />.
    /// If this attribute is not present, the singular name of the type <typeparamref name="TEntity" /> is used.
    /// 
    /// The type <typeparamref name="TEntity" /> must have a property (with a public getter) denoted with a
    /// <see cref="KeyAttribute" />.
    /// </remarks>
    /// <example>
    /// Delete a sequence of entities:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class Product
    /// {
    ///     [Key]
    ///     public Int64 Id { get; set; }
    ///     public Boolean IsDiscontinued { get; set; }
    /// }
    /// 
    /// await connection.DeleteEntitiesAsync(products.Where(a => a.IsDiscontinued));
    /// </code>
    /// </example>
    public static Task<Int32> DeleteEntitiesAsync<TEntity>(
        this SqlConnection connection,
        IEnumerable<TEntity> entities,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(entities);

        // ReSharper disable once UseDeconstruction
        var metadata = EntityHelper.GetEntityTypeMetadata<TEntity>();
        var keyPropertyName = metadata.KeyPropertyName;
        var keyPropertyType = metadata.KeyPropertyType;
        var keyPropertyGetter = metadata.KeyPropertyGetter;
        var tableName = metadata.TableName;

        var keyValues = keyPropertyType.CreateListForType();

        foreach (var entity in entities)
        {
            var keyValue = keyPropertyGetter(entity);

            if (keyValue is null)
            {
                continue;
            }

            keyValues.Add(keyValue);
        }

        var keyValuesTable = new InterpolatedTemporaryTable(
            "#Keys_" + Guid.NewGuid().ToString("N"),
            keyValues,
            keyPropertyType
        );

        return connection.ExecuteNonQueryAsync(
            $"""
             DELETE FROM    [{tableName}]
             WHERE          [{keyPropertyName}] IN 
                            (SELECT Value FROM {keyValuesTable})
             """,
            transaction: transaction,
            cancellationToken: cancellationToken
        );
    }
}
