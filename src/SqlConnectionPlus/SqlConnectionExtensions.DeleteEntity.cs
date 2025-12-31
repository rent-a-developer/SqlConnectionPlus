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
    /// Deletes the specified entity identified by its key property from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to delete.</typeparam>
    /// <param name="connection">The SQL connection to use to delete the entity.</param>
    /// <param name="entity">The entity to delete.</param>
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
    ///                 <paramref name="entity" /> is <see langword="null" />.
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
    /// The table from which the entity will be deleted is determined by the <see cref="TableAttribute" />
    /// applied to the type <typeparamref name="TEntity" />.
    /// If this attribute is not present, the singular name of the type <typeparamref name="TEntity" /> is used.
    /// 
    /// The type <typeparamref name="TEntity" /> must have a property (with a public getter) denoted with a
    /// <see cref="KeyAttribute" />.
    /// </remarks>
    /// <example>
    /// Delete an entity:
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
    /// if (product.IsDiscontinued)
    /// {
    ///     connection.DeleteEntity(product);
    /// }
    /// </code>
    /// </example>
    public static Int32 DeleteEntity<TEntity>(
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

        var command = Dependencies.SqlCommandFactory.CreateSqlCommand(connection, metadata.DeleteSql, transaction);

        var keyParameter = command.CreateParameter();
        keyParameter.ParameterName = "Key";
        command.Parameters.Add(keyParameter);

        var cancellationTokenRegistration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        using (command)
        using (cancellationTokenRegistration)
        {
            keyParameter.Value = metadata.KeyPropertyGetter(entity) ?? DBNull.Value;

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
    /// Asynchronously deletes the specified entity identified by its key property from the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to delete.</typeparam>
    /// <param name="connection">The SQL connection to use to delete the entity.</param>
    /// <param name="entity">The entity to delete.</param>
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
    ///                 <paramref name="entity" /> is <see langword="null" />.
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
    /// The table from which the entity will be deleted is determined by the <see cref="TableAttribute" />
    /// applied to the type <typeparamref name="TEntity" />.
    /// If this attribute is not present, the singular name of the type <typeparamref name="TEntity" /> is used.
    /// 
    /// The type <typeparamref name="TEntity" /> must have a property (with a public getter) denoted with a
    /// <see cref="KeyAttribute" />.
    /// </remarks>
    /// <example>
    /// Delete an entity:
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
    /// if (product.IsDiscontinued)
    /// {
    ///     await connection.DeleteEntityAsync(product);
    /// }
    /// </code>
    /// </example>
    public static async Task<Int32> DeleteEntityAsync<TEntity>(
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

        var command = Dependencies.SqlCommandFactory.CreateSqlCommand(connection, metadata.DeleteSql, transaction);

        var keyParameter = command.CreateParameter();
        keyParameter.ParameterName = "Key";
        command.Parameters.Add(keyParameter);

        var cancellationTokenRegistration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        await using (command)
        await using (cancellationTokenRegistration)
        {
            keyParameter.Value = metadata.KeyPropertyGetter(entity) ?? DBNull.Value;

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
