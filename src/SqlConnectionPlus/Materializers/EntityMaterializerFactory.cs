// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Linq.Expressions;
using System.Reflection;
using RentADeveloper.SqlConnectionPlus.Entities;

namespace RentADeveloper.SqlConnectionPlus.Materializers;

/// <summary>
/// A factory that creates functions to materialize instances of <see cref="IDataRecord" /> to instances of entities.
/// </summary>
internal static class EntityMaterializerFactory
{
    /// <summary>
    /// Gets a materializer function that materializes the data of an <see cref="IDataRecord" /> to an instance of the
    /// type <typeparamref name="TEntity" />.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of entity the materializer function should materialize.
    /// </typeparam>
    /// <param name="dataReader">The <see cref="IDataReader" /> for which to create the materializer function.</param>
    /// <returns>
    /// A function that materializes an <see cref="IDataRecord" /> to an instance of <typeparamref name="TEntity" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataReader" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> has no fields.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> contains a field that has no field name.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> contains a field for which no corresponding property of the type
    /// <typeparamref name="TEntity" /> could be found.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> contains a field with a field type that is not supported.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <remarks>
    /// The type <typeparamref name="TEntity" /> must have properties (with public setters) that match the names and
    /// data types of the fields of <paramref name="dataReader" />.
    /// 
    /// The returned function materialized the passed <see cref="IDataRecord" /> into an instance of
    /// the type <typeparamref name="TEntity" />, with the properties being populated from the corresponding fields of
    /// <see cref="IDataRecord" />.
    /// The data types of the fields of <see cref="IDataRecord" /> must be compatible with the property types of
    /// the properties of the type <typeparamref name="TEntity" />.
    /// 
    /// If <paramref name="dataReader" /> contains a field that does not have a corresponding property in the type
    /// <typeparamref name="TEntity" />, an <see cref="ArgumentException" /> will be thrown.
    /// 
    /// If a property cannot be set to the value of the corresponding field of <see cref="IDataRecord" />
    /// (for example, due to a type mismatch), an <see cref="InvalidCastException" /> will also be thrown.
    /// </remarks>
    internal static Func<IDataRecord, TEntity> GetMaterializer<TEntity>(IDataReader dataReader)
        where TEntity : new()
    {
        ArgumentNullException.ThrowIfNull(dataReader);

        var entityType = typeof(TEntity);

        var entityPropertiesByName = EntityHelper
            .GetEntityWriteableProperties(entityType)
            .ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

        var dataReaderFieldNames = dataReader.GetFieldNames();
        var dataReaderFieldTypes = dataReader.GetFieldTypes();

        ValidateDataReader(entityType, entityPropertiesByName, dataReader, dataReaderFieldNames, dataReaderFieldTypes);

        // We can only re-use a cached materializer if the entity type and the data reader field names and field types
        // are the same:
        var cacheKey = new MaterializerCacheKey(entityType, dataReaderFieldNames, dataReaderFieldTypes);

        return (Func<IDataRecord, TEntity>)materializerCache.GetOrAdd(
            cacheKey,
            static (cacheKey2, args) =>
                CreateMaterializer(
                    cacheKey2.EntityType,
                    args.entityPropertiesByName,
                    args.dataReader,
                    args.dataReaderFieldNames,
                    args.dataReaderFieldTypes
                ),
            (entityPropertiesByName, dataReader, dataReaderFieldNames, dataReaderFieldTypes)
        );
    }

    /// <summary>
    /// Creates a materializer function that materializes the data of an <see cref="IDataRecord" /> to an instance of
    /// the type <paramref name="entityType" />.
    /// </summary>
    /// <param name="entityType">The type of entity the materializer function should materialize.</param>
    /// <param name="entityPropertiesByName">
    /// A dictionary containing the properties of the type <paramref name="entityType" />.
    /// The keys are the property names. The values are the corresponding <see cref="PropertyInfo" /> instances.
    /// </param>
    /// <param name="dataReader">The <see cref="IDataReader" /> for which to create the materializer function.</param>
    /// <param name="dataReaderFieldNames">
    /// The names of the fields of <paramref name="dataReader" />.
    /// The order of the names must match the order of the fields in <paramref name="dataReader" />.
    /// </param>
    /// <param name="dataReaderFieldTypes">
    /// The field types of the fields of <paramref name="dataReader" />.
    /// The order of the types must match the order of the fields in <paramref name="dataReader" />.
    /// </param>
    /// <returns>
    /// A function that materializes an <see cref="IDataRecord" /> to an instance of the type
    /// <paramref name="entityType" />.
    /// </returns>
    /// <remarks>
    /// The type <paramref name="entityType" /> must have properties (with public setters) that match the names and
    /// data types of the fields of <paramref name="dataReader" />.
    /// 
    /// The returned function materializes the passed <see cref="IDataRecord" /> into an instance of
    /// the type <paramref name="entityType" />, with the properties being populated from the corresponding fields of
    /// the <see cref="IDataRecord" />.
    /// The data types of the fields of the <see cref="IDataRecord" /> must be compatible with the property types of
    /// the properties of the type <paramref name="entityType" />.
    /// </remarks>
    private static Delegate CreateMaterializer(
        Type entityType,
        Dictionary<String, PropertyInfo> entityPropertiesByName,
        IDataReader dataReader,
        String[] dataReaderFieldNames,
        Type[] dataReaderFieldTypes
    )
    {
        /*
         * This method creates an expression tree to generate a materializer function instead of using reflection for
         * the materialization, because using reflection would be significantly slower.
         * Using an expression tree also allows us to use the typed GetXXX methods of IDataRecord, which avoids boxing
         * in many cases.
         *
         * The code of the generated materializer function looks like this:
         *
         * TEntity MaterializeToEntity<TEntity>(IDataRecord dataRecord)
         * {
         *   var entity = new TEntity();
         *   Boolean isFieldValueDbNull;
         *
         *   // Field 1:
         *   isFieldValueDbNull = dataRecord.IsDBNull(0);
         *
         *   if (isFieldValueDbNull && !EntityProperty1.PropertyType.IsReferenceTypeOrNullableType())
         *   {
         *     throw new InvalidCastException(...);
         *   }
         *
         *   if (!isFieldValueDbNull)
         *   {
         *     if (EntityProperty1.PropertyType.IsEnumOrNullableEnumType())
         *     {
         *       entity.Property1 = EnumConverter
         *          .ConvertValueToEnumMember<EntityProperty1.PropertyType>(dataRecord.GetXXX(0));
         *     }
         *     else
         *     {
         *       if (EntityProperty1.PropertyType.IsCharOrNullableCharType())
         *       {
         *         stringValue1 = dataRecord.GetString(0);
         *
         *         if (stringValue1.Length == 1)
         *         {
         *           entity.Property1 = stringValue1[0];
         *         }
         *         else
         *         {
         *           throw new InvalidCastException(...);
         *         }
         *       }
         *       else
         *       {
         *         entity.Property1 = (EntityProperty1.PropertyType) dataRecord.GetXXX(0);
         *       }
         *     }
         *   }
         *
         *   // Field 2:
         *   isFieldValueDbNull = dataRecord.IsDBNull(1);
         *
         *   if (isFieldValueDbNull && !EntityProperty2.PropertyType.IsReferenceTypeOrNullableType())
         *   {
         *     throw new InvalidCastException(...);
         *   }
         *
         *   if (!isFieldValueDbNull)
         *   {
         *     if (EntityProperty2.PropertyType.IsEnumOrNullableEnumType())
         *     {
         *       entity.Property2 = EnumConverter
         *          .ConvertValueToEnumMember<EntityProperty2.PropertyType>(dataRecord.GetXXX(1));
         *     }
         *     else
         *     {
         *       if (EntityProperty2.PropertyType.IsCharOrNullableCharType())
         *       {
         *         stringValue2 = dataRecord.GetString(1);
         *
         *         if (stringValue2.Length == 1)
         *         {
         *           entity.Property2 = stringValue2[0];
         *         }
         *         else
         *         {
         *           throw new InvalidCastException(...);
         *         }
         *       }
         *       else
         *       {
         *         entity.Property2 = (EntityProperty2.PropertyType) dataRecord.GetXXX(1);
         *       }
         *     }
         *   }
         *
         *   ...
         *
         *   return entity;
         * }
         */

        var dataRecordParameter = Expression.Parameter(typeof(IDataRecord), "dataRecord");

        // TEntity entity;
        var entityVariable = Expression.Variable(entityType, "entity");

        // Boolean isFieldValueDbNull;
        var isFieldValueDbNullVariable = Expression.Variable(typeof(Boolean), "isFieldValueDbNull");

        // String stringValue1;
        // String stringValue2;
        // ...
        var stringValueVariables = new ParameterExpression[dataReader.FieldCount];

        for (var i = 0; i < stringValueVariables.Length; i++)
        {
            stringValueVariables[i] = Expression.Variable(typeof(String), $"stringValue{i + 1}");
        }

#pragma warning disable IDE0028
        var expressions = new List<Expression>();
#pragma warning restore IDE0028

        // entity = new TEntity();
        expressions.Add(Expression.Assign(entityVariable, Expression.New(entityType)));

        for (var fieldOrdinal = 0; fieldOrdinal < dataReader.FieldCount; fieldOrdinal++)
        {
            var fieldOrdinalExpression = Expression.Constant(fieldOrdinal);

            var dataReaderFieldName = dataReaderFieldNames[fieldOrdinal];
            var dataReaderFieldType = dataReaderFieldTypes[fieldOrdinal];

            var entityProperty = entityPropertiesByName[dataReaderFieldName];
            var entityPropertyType = entityProperty.PropertyType;

            // isFieldValueDbNull = record.IsDBNull(fieldOrdinal);
            expressions.Add(
                Expression.Assign(
                    isFieldValueDbNullVariable,
                    Expression.Call(
                        dataRecordParameter,
                        MaterializerFactoryHelper.DataRecordIsDBNullMethod,
                        fieldOrdinalExpression
                    )
                )
            );

            // getFieldValue = record.GetXXX(fieldOrdinal);
            var getFieldValue =
                MaterializerFactoryHelper.CreateGetRecordFieldValueExpression(
                    dataRecordParameter,
                    fieldOrdinalExpression,
                    fieldOrdinal,
                    dataReaderFieldName,
                    dataReaderFieldType
                );

            if (entityPropertyType.IsEnumOrNullableEnumType())
            {
                // getFieldValue =
                // try
                // {
                //   EnumConverter.ConvertValueToEnumMember<EntityPropertyType>(getFieldValue);
                // }
                // catch (Exception ex)
                // {
                //   throw new InvalidCastException(..., ex);
                // }
                //
                var exceptionParameter = Expression.Parameter(typeof(Exception));

                getFieldValue =
                    Expression.TryCatch(
                        Expression.Call(
                            null,
                            MaterializerFactoryHelper
                                .EnumConverterConvertValueToEnumMemberMethod
                                .MakeGenericMethod(entityPropertyType),
                            Expression.Convert(getFieldValue, typeof(Object))
                        ),
                        Expression.Catch(
                            exceptionParameter,
                            Expression.Throw(
                                Expression.New(
                                    typeof(InvalidCastException).GetConstructor(
                                        [typeof(String), typeof(Exception)]
                                    )!,
                                    Expression.Constant(
                                        $"The column '{dataReaderFieldName}' returned by the SQL statement contains " +
                                        $"a value that could not be converted to the enum type {entityPropertyType} " +
                                        $"of the corresponding property of the type {entityType}. See inner " +
                                        $"exception for details."
                                    ),
                                    exceptionParameter
                                ),
                                entityPropertyType
                            )
                        )
                    );
            }

            // IDataReader returns a value of the SQL data type CHAR(1) as a string.
            // If entityPropertyType is Char or Char?, we need to get the first character of the string.
            if (entityPropertyType.IsCharOrNullableCharType())
            {
                var stringValueVariable = stringValueVariables[fieldOrdinal];

                // if (!isFieldValueDbNull) { stringValueX = getFieldValue; }
                // Because calling IDataRecord.GetString on a column that contains NULL would throw an exception.
                expressions.Add(
                    Expression.IfThen(
                        Expression.IsFalse(
                            isFieldValueDbNullVariable
                        ),
                        Expression.Assign(
                            stringValueVariable,
                            getFieldValue
                        )
                    )
                );

                // getFieldValue = stringValueX.Length != 1 ? throw new InvalidCastException() : string[0];
                getFieldValue = Expression.Condition(
                    Expression.NotEqual(
                        Expression.Property(stringValueVariable, MaterializerFactoryHelper.StringLengthProperty),
                        Expression.Constant(1)
                    ),
                    Expression.Throw(
                        Expression.New(
                            typeof(InvalidCastException).GetConstructor([typeof(String)])!,
                            Expression.Call(
                                null,
                                MaterializerFactoryHelper.StringConcatMethod,
                                Expression.Constant(
                                    $"The column '{dataReaderFieldName}' returned by the SQL statement contains the " +
                                    $"string '"
                                ),
                                stringValueVariable,
                                Expression.Constant(
                                    $"', which could not be converted to the type {entityPropertyType} of the " +
                                    $"corresponding property of the type {entityType}. The string must be exactly " +
                                    $"one character long."
                                )
                            )
                        ),
                        typeof(Char)
                    ),
                    Expression.MakeIndex(
                        stringValueVariable,
                        MaterializerFactoryHelper.StringCharsProperty,
                        [Expression.Constant(0)]
                    )
                );
            }

            // entity.Property = (EntityPropertyType) getFieldValue;
            var assignFieldValueToProperty = Expression.Assign(
                Expression.Property(entityVariable, entityProperty),
                Expression.Convert(getFieldValue, entityPropertyType)
            );

            if (entityPropertyType.IsReferenceTypeOrNullableType())
            {
                // if (!isFieldValueDbNull) { entity.Property = (PropertyType) getFieldValue; }
                expressions.Add(
                    Expression.IfThen(
                        Expression.IsFalse(isFieldValueDbNullVariable),
                        assignFieldValueToProperty
                    )
                );
            }
            else
            {
                // if (isFieldValueDbNull)
                // {
                //   throw new InvalidCastException(...);
                // }
                // else
                // {
                //   entity.Property = (EntityPropertyType) getFieldValue;
                // }
                //
                // If the field value is DBNull and the property type is non-nullable, we must throw an exception.
                expressions.Add(
                    Expression.IfThenElse(
                        isFieldValueDbNullVariable,
                        Expression.Throw(
                            Expression.New(
                                typeof(InvalidCastException).GetConstructor([typeof(String)])!,
                                Expression.Constant(
                                    $"The column '{dataReaderFieldName}' returned by the SQL statement contains a " +
                                    $"NULL value, but the corresponding property of the type {entityType} is " +
                                    $"non-nullable."
                                )
                            ),
                            typeof(void)
                        ),
                        assignFieldValueToProperty
                    )
                );
            }
        }

        // return entity;
        var returnTarget = Expression.Label(entityType);
        var returnExpression = Expression.Return(returnTarget, entityVariable);
        var returnLabel = Expression.Label(returnTarget, Expression.Default(entityType));

        expressions.Add(returnExpression);
        expressions.Add(returnLabel);

        return Expression
            .Lambda(
                Expression.Block(
                    [
                        entityVariable,
                        isFieldValueDbNullVariable,
                        .. stringValueVariables
                    ],
                    expressions
                ),
                dataRecordParameter
            )
            .Compile();
    }

    /// <summary>
    /// Validates that instances of the type <paramref name="entityType" /> can be materialized from the data in
    /// <paramref name="dataReader" />.
    /// </summary>
    /// <param name="entityType">The type of entity to materialize.</param>
    /// <param name="entityPropertiesByName">
    /// A dictionary containing the properties of the type <paramref name="entityType" />.
    /// The keys are the property names. The values are the corresponding <see cref="PropertyInfo" /> instances.
    /// </param>
    /// <param name="dataReader">The <see cref="IDataReader" /> to validate.</param>
    /// <param name="dataReaderFieldNames">
    /// The names of the fields of <paramref name="dataReader" />.
    /// The order of the names must match the order of the fields in <paramref name="dataReader" />.
    /// </param>
    /// <param name="dataReaderFieldTypes">
    /// The field types of the fields of <paramref name="dataReader" />.
    /// The order of the types must match the order of the fields in <paramref name="dataReader" />.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> has no fields.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> contains a field that has no field name.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> contains a field for which no corresponding property of the type
    /// <paramref name="entityType" /> could be found.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> contains a field with an unsupported field type.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    private static void ValidateDataReader(
        Type entityType,
        Dictionary<String, PropertyInfo> entityPropertiesByName,
        IDataReader dataReader,
        String[] dataReaderFieldNames,
        Type[] dataReaderFieldTypes
    )
    {
        if (dataReader.FieldCount == 0)
        {
            throw new ArgumentException("The SQL statement did not return any columns.", nameof(dataReader));
        }

        for (var fieldOrdinal = 0; fieldOrdinal < dataReader.FieldCount; fieldOrdinal++)
        {
            var dataReaderFieldName = dataReaderFieldNames[fieldOrdinal];
            var dataReaderFieldType = dataReaderFieldTypes[fieldOrdinal];

            if (String.IsNullOrWhiteSpace(dataReaderFieldName))
            {
                throw new ArgumentException(
                    $"The {(fieldOrdinal + 1).OrdinalizeEnglish()} column returned by the SQL statement does not " +
                    $"have a name. Make sure that all columns the SQL statement returns have a name.",
                    nameof(dataReader)
                );
            }

            if (!entityPropertiesByName.TryGetValue(dataReaderFieldName, out var entityProperty))
            {
                throw new ArgumentException(
                    $"Could not map the column '{dataReaderFieldName}' returned by the SQL statement to a property " +
                    $"(with a public setter) of the type {entityType}. Make sure the type has a corresponding " +
                    $"property.",
                    nameof(dataReader)
                );
            }

            var entityPropertyType = entityProperty.PropertyType;
            var underlyingEntityPropertyType = Nullable.GetUnderlyingType(entityPropertyType);

            // Check whether the field type of the data reader is compatible with the property type of the entity
            // property:
            var isDataReaderFieldTypeCompatibleWithEntityPropertyType =
                // If the property type is Object, we can assign any type of field value to it:
                entityPropertyType == typeof(Object)
                ||
                // Direct match:
                dataReaderFieldType == entityPropertyType
                ||
                // Match for Nullable<T>:
                (underlyingEntityPropertyType is not null && dataReaderFieldType == underlyingEntityPropertyType)
                ||
                // Special case: Char properties can be populated from String fields:
                (entityPropertyType.IsCharOrNullableCharType() && dataReaderFieldType == typeof(String))
                ||
                // Enums are also supported:
                entityPropertyType.IsEnumOrNullableEnumType();

            if (!isDataReaderFieldTypeCompatibleWithEntityPropertyType)
            {
                throw new ArgumentException(
                    $"The data type {dataReaderFieldType} of the column '{dataReaderFieldName}' returned by the " +
                    $"SQL statement does not match the property type {entityPropertyType} of the corresponding " +
                    $"property of the type {entityType}.",
                    nameof(dataReader)
                );
            }

            if (!MaterializerFactoryHelper.IsDataRecordTypedGetMethodAvailable(dataReaderFieldType))
            {
                throw new ArgumentException(
                    $"The data type {dataReaderFieldType} of the column '{dataReaderFieldName}' returned by the " +
                    $"SQL statement is not supported.",
                    nameof(dataReader)
                );
            }
        }
    }

    private static readonly ConcurrentDictionary<MaterializerCacheKey, Delegate> materializerCache = [];

    /// <summary>
    /// A cache key used to uniquely identify an entity materializer.
    /// </summary>
    /// <param name="entityType">The type of entity the materializer materializes.</param>
    /// <param name="dataReaderFieldNames">
    /// The field names of the <see cref="IDataReader" /> from which to materialize.
    /// The order of the names must match the order of the fields in the data reader.
    /// </param>
    /// <param name="dataReaderFieldTypes">
    /// The field types of the <see cref="IDataReader" /> from which to materialize.
    /// The order of the types must match the order of the fields in the data reader.
    /// </param>
    private readonly struct MaterializerCacheKey(
        Type entityType,
        String[] dataReaderFieldNames,
        Type[] dataReaderFieldTypes
    )
        : IEquatable<MaterializerCacheKey>
    {
        /// <summary>
        /// The type of entity the materializer materializes.
        /// </summary>
        public Type EntityType { get; } = entityType;

        /// <inheritdoc />
        public Boolean Equals(MaterializerCacheKey other) =>
            this.EntityType == other.EntityType &&
            this.DataReaderFieldNames.SequenceEqual(other.DataReaderFieldNames) &&
            this.DataReaderFieldTypes.SequenceEqual(other.DataReaderFieldTypes);

        /// <inheritdoc />
        public override Boolean Equals(Object? obj) =>
            obj is MaterializerCacheKey other && this.Equals(other);

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(this.EntityType);

            foreach (var fieldName in this.DataReaderFieldNames)
            {
                hashCode.Add(fieldName);
            }

            foreach (var fieldType in this.DataReaderFieldTypes)
            {
                hashCode.Add(fieldType);
            }

            return hashCode.ToHashCode();
        }

        private String[] DataReaderFieldNames { get; } = dataReaderFieldNames;
        private Type[] DataReaderFieldTypes { get; } = dataReaderFieldTypes;
    }
}
