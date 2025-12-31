// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Reflection;
using Fasterflect;
using LinkDotNet.StringBuilder;

namespace RentADeveloper.SqlConnectionPlus.Entities;

/// <summary>
/// Provides helper methods for dealing with entities.
/// </summary>
internal static class EntityHelper
{
    /// <summary>
    /// Gets the first public instance property (with a public getter) of the type <typeparamref name="TEntity" />
    /// that is denoted with a <see cref="KeyAttribute" />.
    /// </summary>
    /// <typeparam name="TEntity">The entity type of which to get the key property.</typeparam>
    /// <returns>
    /// The key property of <see cref="KeyAttribute" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="TEntity" /> does not have a property (with a public getter) that is denoted with
    /// a <see cref="KeyAttribute" />.
    /// </exception>
    internal static PropertyInfo GetEntityKeyProperty<TEntity>() =>
        entityKeyPropertyPerEntityType.GetOrAdd(
            typeof(TEntity),
            static entityType => entityType
                                     .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .FirstOrDefault(p =>
                                         p.GetMethod?.IsPublic == true &&
                                         p.GetCustomAttribute<KeyAttribute>() is not null
                                     )
                                 ?? throw new ArgumentException(
                                     $"Could not get the key property of the type {typeof(TEntity)}. " +
                                     $"Make sure that one property (with a public getter) of that type is denoted " +
                                     $"with a {typeof(KeyAttribute)}."
                                 )
        );

    /// <summary>
    /// Gets the public instance properties (with public getters) of the type <paramref name="entityType" />.
    /// </summary>
    /// <param name="entityType">The entity type of which to get the properties.</param>
    /// <returns>
    /// The public instance properties (with public getters) of the type <paramref name="entityType" />.
    /// Properties denoted with the <see cref="NotMappedAttribute" /> are not included.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="entityType" /> is <see langword="null" />.</exception>
    internal static PropertyInfo[] GetEntityReadableProperties(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        return entityReadablePropertiesPerEntityType.GetOrAdd(
            entityType,
            static entityType2 => entityType2
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(a => a.GetMethod?.IsPublic == true && a.GetCustomAttribute<NotMappedAttribute>() is null)
                .OrderBy(a => a.Name)
                .ToArray()
        );
    }

    /// <summary>
    /// Gets the names of the public instance properties (with public getters) of the type
    /// <paramref name="entityType" />.
    /// </summary>
    /// <param name="entityType">The entity type of which to get the property names.</param>
    /// <returns>
    /// The names of the public instance properties (with public getters) of the type <paramref name="entityType" />.
    /// Properties denoted with the <see cref="NotMappedAttribute" /> are not included.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="entityType" /> is <see langword="null" />.</exception>
    internal static String[] GetEntityReadablePropertyNames(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        return entityReadablePropertyNamesPerEntityType.GetOrAdd(
            entityType,
            static entityType2 => GetEntityReadableProperties(entityType2)
                .Select(a => a.Name)
                .ToArray()
        );
    }

    /// <summary>
    /// Gets the name of the table where entities of the type <typeparamref name="TEntity" /> are stored.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to get the table name of.</typeparam>
    /// <returns>
    /// The name of the table where entities of type <typeparamref name="TEntity" /> are stored.
    /// </returns>
    /// <remarks>
    /// The table name is obtained from the <see cref="TableAttribute" /> applied to the type
    /// <typeparamref name="TEntity" />.
    /// If the type <typeparamref name="TEntity" /> is not denoted with a <see cref="TableAttribute" />,
    /// the singular name of the type <typeparamref name="TEntity" /> is returned.
    /// </remarks>
    internal static String GetEntityTableName<TEntity>() =>
        tableNamePerEntityType.GetOrAdd(
            typeof(TEntity),
            static entityType => entityType.GetCustomAttribute<TableAttribute>()?.Name ?? entityType.Name
        );

    /// <summary>
    /// Gets the metadata required to perform INSERT, UPDATE and DELETE operations for the entity type
    /// <typeparamref name="TEntity" />.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which to get the metadata.</typeparam>
    /// <returns>
    /// An instance of <see cref="EntityTypeMetadata" /> containing the metadata for the entity type
    /// <typeparamref name="TEntity" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="TEntity" /> does not have a property (with a public getter) that is denoted with
    /// a <see cref="KeyAttribute" />.
    /// </exception>
    internal static EntityTypeMetadata GetEntityTypeMetadata<TEntity>() =>
        entityTypeMetadataPerEntityType.GetOrAdd(
            typeof(TEntity),
            static _ => CreateEntityTypeMetadata<TEntity>()
        );

    /// <summary>
    /// Gets the public instance properties (with public setters) of the type <paramref name="entityType" />.
    /// </summary>
    /// <param name="entityType">The entity type of which to get the properties.</param>
    /// <returns>
    /// The public instance properties (with public setters) of the type <paramref name="entityType" />.
    /// Properties denoted with the <see cref="NotMappedAttribute" /> are not included.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="entityType" /> is <see langword="null" />.</exception>
    internal static PropertyInfo[] GetEntityWriteableProperties(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        return entityWritablePropertiesPerEntityType.GetOrAdd(
            entityType,
            static entityType2 => entityType2
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(a => a.SetMethod?.IsPublic == true && a.GetCustomAttribute<NotMappedAttribute>() is null)
                .OrderBy(a => a.Name)
                .ToArray()
        );
    }

    /// <summary>
    /// Populates the specified array of SQL parameters with values extracted from the properties of the specified
    /// entity, using the provided entity type metadata.
    /// </summary>
    /// <param name="entityTypeMetadata">
    /// The metadata of the entity type for which to populate the parameters.
    /// </param>
    /// <param name="sqlParameters">
    /// The array of SQL parameters to be populated with the values from the specified entity's properties.
    /// The length and order of the array must correspond to the property names in
    /// <see cref="EntityTypeMetadata.PropertyNames" />.
    /// </param>
    /// <param name="entity">
    /// The entity from which property values are read to populate the specified SQL parameters.
    /// </param>
    /// <remarks>
    /// Enum values read from <paramref name="entity" /> are serialized according to
    /// <see cref="SqlConnectionExtensions.EnumSerializationMode" />.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="sqlParameters" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="entity" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    internal static void PopulateSqlParametersFromEntityProperties(
        EntityTypeMetadata entityTypeMetadata,
        SqlParameter[] sqlParameters,
        Object entity
    )
    {
        ArgumentNullException.ThrowIfNull(entityTypeMetadata);
        ArgumentNullException.ThrowIfNull(sqlParameters);
        ArgumentNullException.ThrowIfNull(entity);

        var propertyGetters = entityTypeMetadata.PropertyGetters;
        var isPropertyTypeEnumOrNullableEnum = entityTypeMetadata.IsPropertyTypeEnumOrNullableEnum;

        for (var i = 0; i < propertyGetters.Length; i++)
        {
            var parameter = sqlParameters[i];
            var propertyValue = propertyGetters[i](entity);

            if (propertyValue is Enum enumValue && isPropertyTypeEnumOrNullableEnum[i])
            {
                propertyValue = EnumSerializer.SerializeEnum(enumValue, SqlConnectionExtensions.EnumSerializationMode);
            }

            parameter.Value = propertyValue ?? DBNull.Value;
        }
    }

    /// <summary>
    /// Creates the metadata required to perform INSERT, UPDATE and DELETE operations for the entity type
    /// <typeparamref name="TEntity" />.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which to create the metadata.</typeparam>
    /// <returns>
    /// An instance of <see cref="EntityTypeMetadata" /> containing the created metadata.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="TEntity" /> does not have a property (with a public getter) that is denoted with
    /// a <see cref="KeyAttribute" />.
    /// </exception>
    private static EntityTypeMetadata CreateEntityTypeMetadata<TEntity>()
    {
        var entityType = typeof(TEntity);
        var tableName = GetEntityTableName<TEntity>();
        var properties = GetEntityReadableProperties(entityType);
        var keyProperty = GetEntityKeyProperty<TEntity>();
        var keyPropertyGetter = Reflect.PropertyGetter(keyProperty);

        using var insertSqlBuilder = new ValueStringBuilder(stackalloc Char[500]);
        using var updateSqlBuilder = new ValueStringBuilder(stackalloc Char[500]);

        insertSqlBuilder.Append("INSERT INTO [");
        insertSqlBuilder.Append((ReadOnlySpan<Char>)tableName);
        insertSqlBuilder.Append(']');
        insertSqlBuilder.AppendLine();
        insertSqlBuilder.Append('(');

        updateSqlBuilder.Append("UPDATE [");
        updateSqlBuilder.Append((ReadOnlySpan<Char>)tableName);
        updateSqlBuilder.Append(']');
        updateSqlBuilder.AppendLine();
        updateSqlBuilder.Append("SET ");

        var prependComma = false;

        foreach (var property in properties)
        {
            if (prependComma)
            {
                insertSqlBuilder.Append(", ");
                updateSqlBuilder.Append(", ");
            }

            insertSqlBuilder.Append('[');
            insertSqlBuilder.Append(property.Name);
            insertSqlBuilder.Append(']');

            updateSqlBuilder.Append('[');
            updateSqlBuilder.Append(property.Name);
            updateSqlBuilder.Append("] = @");
            updateSqlBuilder.Append(property.Name);

            prependComma = true;
        }

        insertSqlBuilder.Append(')');
        insertSqlBuilder.AppendLine();
        insertSqlBuilder.AppendLine("VALUES");
        insertSqlBuilder.Append('(');

        updateSqlBuilder.AppendLine();
        updateSqlBuilder.Append("WHERE [");
        updateSqlBuilder.Append((ReadOnlySpan<Char>)keyProperty.Name);
        updateSqlBuilder.Append("] = @");
        updateSqlBuilder.AppendLine(keyProperty.Name);

        var propertyNames = new String[properties.Length];
        var propertyGetters = new MemberGetter[properties.Length];
        var isPropertyTypeDateTimeOrNullableDateTime = new Boolean[properties.Length];
        var isPropertyTypeEnumOrNullableEnum = new Boolean[properties.Length];
        var isPropertyTypeBinary = new Boolean[properties.Length];

        prependComma = false;

        for (var i = 0; i < properties.Length; i++)
        {
            if (prependComma)
            {
                insertSqlBuilder.Append(", ");
            }

            var property = properties[i];
            var propertyType = property.PropertyType;

            propertyNames[i] = property.Name;
            propertyGetters[i] = Reflect.PropertyGetter(property);
            isPropertyTypeDateTimeOrNullableDateTime[i] =
                propertyType == typeof(DateTime) || propertyType == typeof(DateTime?);
            isPropertyTypeEnumOrNullableEnum[i] = propertyType.IsEnumOrNullableEnumType();
            isPropertyTypeBinary[i] = propertyType == typeof(Byte[]);

            insertSqlBuilder.Append('@');
            insertSqlBuilder.Append(property.Name);

            prependComma = true;
        }

        insertSqlBuilder.Append(')');
        insertSqlBuilder.AppendLine();

        var deleteSql = $"DELETE FROM [{tableName}] WHERE [{keyProperty.Name}] = @Key";

        return new(
            insertSqlBuilder.ToString(),
            updateSqlBuilder.ToString(),
            deleteSql,
            tableName,
            keyProperty.Name,
            keyProperty.PropertyType,
            keyPropertyGetter,
            propertyNames,
            propertyGetters,
            isPropertyTypeBinary,
            isPropertyTypeDateTimeOrNullableDateTime,
            isPropertyTypeEnumOrNullableEnum
        );
    }

    private static readonly ConcurrentDictionary<Type, PropertyInfo> entityKeyPropertyPerEntityType = [];

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]>
        entityReadablePropertiesPerEntityType = [];

    private static readonly ConcurrentDictionary<Type, String[]>
        entityReadablePropertyNamesPerEntityType = [];

    private static readonly ConcurrentDictionary<Type, EntityTypeMetadata> entityTypeMetadataPerEntityType = [];

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]>
        entityWritablePropertiesPerEntityType = [];

    private static readonly ConcurrentDictionary<Type, String> tableNamePerEntityType = [];
}
