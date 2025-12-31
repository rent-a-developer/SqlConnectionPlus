// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.Entities;

namespace RentADeveloper.SqlConnectionPlus.SqlCommands;

/// <summary>
/// Provides functions to create SQL commands related to entities.
/// </summary>
internal static class EntitySqlCommandFactory
{
    /// <summary>
    /// Creates a new <see cref="SqlCommand" /> to insert an entity.
    /// </summary>
    /// <param name="connection">The SQL connection to use to create the command.</param>
    /// <param name="transaction">The SQL transaction to assign to the command.</param>
    /// <param name="entityTypeMetadata">The metadata of the type of entity to insert.</param>
    /// <returns>
    /// A tuple containing the created command and the parameters for the property values of the entity.
    /// The order of the returned parameters matches the order of the property names in
    /// <see cref="EntityTypeMetadata.PropertyNames" />.
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
    ///                 <paramref name="entityTypeMetadata" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    internal static (SqlCommand Command, SqlParameter[] Parameters) CreateInsertEntityCommand(
        SqlConnection connection,
        SqlTransaction? transaction,
        EntityTypeMetadata entityTypeMetadata
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(entityTypeMetadata);

        var insertSql = entityTypeMetadata.InsertSql;
        var propertyNames = entityTypeMetadata.PropertyNames;
        var isPropertyTypeByteArray = entityTypeMetadata.IsPropertyTypeByteArray;
        var isPropertyTypeDateTimeOrNullableDateTime = entityTypeMetadata.IsPropertyTypeDateTimeOrNullableDateTime;

        var command = Dependencies.SqlCommandFactory.CreateSqlCommand(connection, insertSql, transaction);

        var parameters = new SqlParameter[propertyNames.Length];

        for (var i = 0; i < propertyNames.Length; i++)
        {
            var propertyName = propertyNames[i];

            var parameter = command.CreateParameter();

            parameter.ParameterName = propertyName;

            if (isPropertyTypeDateTimeOrNullableDateTime[i])
            {
                parameter.DbType = DbType.DateTime2;
            }
            else if (isPropertyTypeByteArray[i])
            {
                parameter.DbType = DbType.Binary;
            }

            command.Parameters.Add(parameter);

            parameters[i] = parameter;
        }

        return (command, parameters);
    }

    /// <summary>
    /// Creates a new <see cref="SqlCommand" /> to update an entity.
    /// </summary>
    /// <param name="connection">The SQL connection to use to create the command.</param>
    /// <param name="transaction">The SQL transaction to assign to the command.</param>
    /// <param name="entityTypeMetadata">The metadata of the type of entity to update.</param>
    /// <returns>
    /// A tuple containing the created command and the parameters for the property values of the entity.
    /// The order of the returned parameters matches the order of the property names in
    /// <see cref="EntityTypeMetadata.PropertyNames" />.
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
    ///                 <paramref name="entityTypeMetadata" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    internal static (SqlCommand Command, SqlParameter[] Parameters) CreateUpdateEntityCommand(
        SqlConnection connection,
        SqlTransaction? transaction,
        EntityTypeMetadata entityTypeMetadata
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(entityTypeMetadata);

        var updateSql = entityTypeMetadata.UpdateSql;
        var propertyNames = entityTypeMetadata.PropertyNames;
        var isPropertyTypeByteArray = entityTypeMetadata.IsPropertyTypeByteArray;
        var isPropertyTypeDateTimeOrNullableDateTime = entityTypeMetadata.IsPropertyTypeDateTimeOrNullableDateTime;

        var command = Dependencies.SqlCommandFactory.CreateSqlCommand(connection, updateSql, transaction);

        var parameters = new SqlParameter[propertyNames.Length];

        for (var i = 0; i < propertyNames.Length; i++)
        {
            var propertyName = propertyNames[i];

            var parameter = command.CreateParameter();

            parameter.ParameterName = propertyName;

            if (isPropertyTypeDateTimeOrNullableDateTime[i])
            {
                parameter.DbType = DbType.DateTime2;
            }
            else if (isPropertyTypeByteArray[i])
            {
                parameter.DbType = DbType.Binary;
            }

            command.Parameters.Add(parameter);

            parameters[i] = parameter;
        }

        return (command, parameters);
    }
}
