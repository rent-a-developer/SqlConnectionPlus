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
    /// Updates the specified entities identified by their key property in the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities to update.</typeparam>
    /// <param name="connection">The SQL connection to use to update the entities.</param>
    /// <param name="entities">The entities to update.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The number of rows that were affected by the update operation.</returns>
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
    /// The table where the entities will be updated is determined by the <see cref="TableAttribute" />
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
    /// Update a sequence of entities:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class User
    /// {
    ///     [Key]
    ///     public Int64 Id { get; set; }
    ///     public DateTime LastLoginDate { get; set; }
    ///     public UserState State { get; set; }
    /// }
    /// 
    /// var usersWithoutLoginInPastYear = connection.QueryEntities<User>(
    ///     """
    ///     SELECT  *
    ///     FROM    Users
    ///     WHERE   LastLoginDate < DATEADD(YEAR, -1, GETUTCDATE())
    ///     """
    /// ).ToList();
    /// 
    /// foreach (var user in usersWithoutLoginInPastYear)
    /// {
    ///     user.State = UserState.Inactive;
    /// }
    /// 
    /// connection.UpdateEntities(usersWithoutLoginInPastYear);
    /// ]]>
    /// </code>
    /// </example>
    public static Int32 UpdateEntities<TEntity>(
        this SqlConnection connection,
        IEnumerable<TEntity> entities,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(entities);

        var metadata = EntityHelper.GetEntityTypeMetadata<TEntity>();

        var (command, parameters) = EntitySqlCommandFactory.CreateUpdateEntityCommand(
            connection,
            transaction,
            metadata
        );
        var cancellationTokenRegistration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        using (command)
        using (cancellationTokenRegistration)
        {
            var totalNumberOfAffectedRows = 0;

            try
            {
                foreach (var entity in entities)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (entity is null)
                    {
                        continue;
                    }

                    EntityHelper.PopulateSqlParametersFromEntityProperties(metadata, parameters, entity);

                    totalNumberOfAffectedRows += command.ExecuteNonQuery();
                }

                return totalNumberOfAffectedRows;
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
    /// Asynchronously updates the specified entities identified by their key property in the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities to update.</typeparam>
    /// <param name="connection">The SQL connection to use to update the entities.</param>
    /// <param name="entities">The entities to update.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain the number of rows that were affected by the update operation.
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
    /// The table where the entities will be updated is determined by the <see cref="TableAttribute" />
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
    /// Update a sequence of entities:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class User
    /// {
    ///     [Key]
    ///     public Int64 Id { get; set; }
    ///     public DateTime LastLoginDate { get; set; }
    ///     public UserState State { get; set; }
    /// }
    /// 
    /// var usersWithoutLoginInPastYear = await connection.QueryEntitiesAsync<User>(
    ///     """
    ///     SELECT  *
    ///     FROM    Users
    ///     WHERE   LastLoginDate < DATEADD(YEAR, -1, GETUTCDATE())
    ///     """
    /// ).ToListAsync();
    /// 
    /// foreach (var user in usersWithoutLoginInPastYear)
    /// {
    ///     user.State = UserState.Inactive;
    /// }
    /// 
    /// await connection.UpdateEntitiesAsync(usersWithoutLoginInPastYear);
    /// ]]>
    /// </code>
    /// </example>
    public static async Task<Int32> UpdateEntitiesAsync<TEntity>(
        this SqlConnection connection,
        IEnumerable<TEntity> entities,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(entities);

        var metadata = EntityHelper.GetEntityTypeMetadata<TEntity>();

        var (command, parameters) = EntitySqlCommandFactory.CreateUpdateEntityCommand(
            connection,
            transaction,
            metadata
        );
        var cancellationTokenRegistration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        await using (command)
        await using (cancellationTokenRegistration)
        {
            var totalNumberOfAffectedRows = 0;

            try
            {
                foreach (var entity in entities)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (entity is null)
                    {
                        continue;
                    }

                    EntityHelper.PopulateSqlParametersFromEntityProperties(metadata, parameters, entity);

                    totalNumberOfAffectedRows += await command
                        .ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                return totalNumberOfAffectedRows;
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
