// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Linq.Expressions;
using Humanizer;

namespace RentADeveloper.SqlConnectionPlus.Materializers;

/// <summary>
/// Creates functions to materialize instances of <see cref="IDataRecord" /> to instances of <see cref="ValueTuple" />.
/// </summary>
internal static class ValueTupleMaterializerFactory
{
    /// <summary>
    /// Gets a materializer function that materializes the data of an <see cref="IDataRecord" /> to an instance of the
    /// value tuple type <typeparamref name="TValueTuple" />.
    /// </summary>
    /// <typeparam name="TValueTuple">
    /// The type of value tuple the materializer function should materialize.
    /// </typeparam>
    /// <param name="dataReader">The <see cref="IDataReader" /> for which to create the materializer.</param>
    /// <returns>
    /// A function that materializes an <see cref="IDataRecord" /> to an instance of
    /// <typeparamref name="TValueTuple" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataReader" /> is <see langword="null" />.</exception>
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
    ///                 <paramref name="dataReader" /> has no fields.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 A field of <paramref name="dataReader" /> has a field type which does not match the field
    ///                 type of the corresponding field of the value tuple type <typeparamref name="TValueTuple" />.
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
    /// The returned function will materialize the passed <see cref="IDataRecord" /> into an instance of
    /// <typeparamref name="TValueTuple" />, with the fields of the value tuple being populated from the corresponding
    /// fields of <see cref="IDataRecord" />.
    /// 
    /// The order of the fields in the value tuple must match the order of the fields in <see cref="IDataRecord" />.
    /// The data types of the fields in <see cref="IDataRecord" /> must be compatible with the data types of the fields
    /// of the value tuple type <typeparamref name="TValueTuple" />.
    /// </remarks>
    internal static Func<IDataRecord, TValueTuple> GetMaterializer<TValueTuple>(IDataReader dataReader)
        where TValueTuple : struct, IStructuralEquatable, IStructuralComparable, IComparable
    {
        ArgumentNullException.ThrowIfNull(dataReader);

        var valueTupleType = typeof(TValueTuple);

        if (!valueTupleType.IsValueTupleTypeWithUpTo7Fields())
        {
            throw new ArgumentException(
                $"The specified type {typeof(TValueTuple)} is not a value tuple type or it is a value tuple type " +
                $"with more than 7 fields."
            );
        }

        var valueTupleTypeArguments = valueTupleType.GenericTypeArguments;

        var dataReaderFieldNames = dataReader.GetFieldNames();
        var dataReaderFieldTypes = dataReader.GetFieldTypes();

        ValidateDataReader(
            valueTupleType,
            valueTupleTypeArguments,
            dataReader,
            dataReaderFieldNames,
            dataReaderFieldTypes
        );

        // We can only re-use a cached materializer if the value tuple type and the data reader field names and field
        // types are the same:
        var cacheKey = new MaterializerCacheKey(valueTupleType, dataReaderFieldNames, dataReaderFieldTypes);

        return (Func<IDataRecord, TValueTuple>)materializerCache.GetOrAdd(
            cacheKey,
            static (cacheKey2, args) =>
                CreateMaterializer(
                    cacheKey2.ValueTupleType,
                    args.valueTupleTypeArguments,
                    args.dataReader,
                    args.dataReaderFieldNames,
                    args.dataReaderFieldTypes
                ),
            (valueTupleTypeArguments, dataReader, dataReaderFieldNames, dataReaderFieldTypes)
        );
    }

    /// <summary>
    /// Creates a materializer function that materializes the data of an <see cref="IDataRecord" /> to an instance of
    /// the value tuple type <paramref name="valueTupleType" />.
    /// </summary>
    /// <param name="valueTupleType">
    /// The type of value tuple the materializer function should materialize.
    /// </param>
    /// <param name="valueTupleTypeArguments">
    /// The type arguments of the value tuple type <paramref name="valueTupleType" />.
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
    /// A function that materializes an <see cref="IDataRecord" /> to an instance of the value tuple type
    /// <paramref name="valueTupleType" />.
    /// </returns>
    private static Delegate CreateMaterializer(
        Type valueTupleType,
        Type[] valueTupleTypeArguments,
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
         * TValueTuple MaterializeToValueTuple<TValueTuple>(IDataRecord dataRecord)
         * {
         *   TValueTuple valueTuple;
         *
         *   ValueTupleField1Type valueTupleFieldValue1;
         *   ValueTupleField2Type valueTupleFieldValue2;
         *   ...
         *
         *   String stringValue1;
         *   String stringValue1;
         *   ...
         *
         *   Boolean isFieldValueDbNull;
         *
         *   // Field 1:
         *   isFieldValueDbNull = dataRecord.IsDBNull(0);
         *
         *   if (isFieldValueDbNull && !ValueTupleField1Type.IsReferenceTypeOrNullableType())
         *   {
         *     throw new InvalidCastException(...);
         *   }
         *
         *   if (!isFieldValueDbNull)
         *   {
         *     if (ValueTupleField1Type.IsEnumOrNullableEnumType())
         *     {
         *       valueTupleFieldValue1 = EnumConverter
         *          .ConvertValueToEnumMember<ValueTupleField1Type>(dataRecord.GetXXX(0));
         *     }
         *     else
         *     {
         *       if (ValueTupleField1Type.IsCharOrNullableCharType())
         *       {
         *         stringValue1 = dataRecord.GetString(0);
         *
         *         if (stringValue1.Length != 1)
         *         {
         *           throw new InvalidCastException(...);
         *         }
         *         else
         *         {
         *           valueTupleFieldValue1 = stringValue1[0];
         *         }
         *       }
         *       else
         *       {
         *         valueTupleFieldValue1 = (ValueTupleField1Type) dataRecord.GetXXX(0);
         *       }
         *     }
         *   }
         *
         *   // Field 2:
         *   isFieldValueDbNull = dataRecord.IsDBNull(1);
         *
         *   if (isFieldValueDbNull && !ValueTupleField2Type.IsReferenceTypeOrNullableType())
         *   {
         *     throw new InvalidCastException(...);
         *   }
         *
         *   if (!isFieldValueDbNull)
         *   {
         *     if (ValueTupleField2Type.IsEnumOrNullableEnumType())
         *     {
         *       valueTupleFieldValue2 = EnumConverter
         *          .ConvertValueToEnumMember<ValueTupleField2Type>(dataRecord.GetXXX(1));
         *     }
         *     else
         *     {
         *       if (ValueTupleField2Type.IsCharOrNullableCharType())
         *       {
         *         stringValue2 = dataRecord.GetString(1);
         *
         *         if (stringValue2.Length != 0)
         *         {
         *           throw new InvalidCastException(...);
         *         }
         *         else
         *         {
         *           valueTupleFieldValue2 = stringValue2[0];
         *         }
         *       }
         *       else
         *       {
         *         valueTupleFieldValue2 = (ValueTupleField2Type) dataRecord.GetXXX(1);
         *       }
         *     }
         *   }
         *
         *   ...
         *
         *   valueTuple = new TValueTuple(valueTupleFieldValue1, valueTupleFieldValue2, ...);
         *   return valueTuple;
         * }
         */

        var dataRecordParameter = Expression.Parameter(typeof(IDataRecord), "record");

        // TValueTuple valueTuple;
        var valueTupleVariable = Expression.Variable(valueTupleType, "valueTuple");

        // Boolean isFieldValueDbNull;
        var isFieldValueDbNullVariable = Expression.Variable(typeof(Boolean), "isFieldValueDbNull");

        // ValueTupleFieldType1 valueTupleFieldValue1;
        // ValueTupleFieldType2 valueTupleFieldValue2;
        // ...
        var valueTupleFieldValueVariables = new ParameterExpression[dataReader.FieldCount];

        for (var i = 0; i < valueTupleFieldValueVariables.Length; i++)
        {
            valueTupleFieldValueVariables[i] = Expression.Variable(
                valueTupleTypeArguments[i],
                $"valueTupleFieldValue{i + 1}"
            );
        }

        // String stringValue1;
        // String stringValue2;
        // ...
        var stringValueVariables = new ParameterExpression[dataReader.FieldCount];

        for (var i = 0; i < stringValueVariables.Length; i++)
        {
            stringValueVariables[i] = Expression.Variable(typeof(String), $"stringValue{i + 1}");
        }

        var expressions = new List<Expression>();

        for (var fieldOrdinal = 0; fieldOrdinal < dataReader.FieldCount; fieldOrdinal++)
        {
            var fieldOrdinalExpression = Expression.Constant(fieldOrdinal);

            var valueTupleFieldType = valueTupleTypeArguments[fieldOrdinal];

            var dataReaderFieldName = dataReaderFieldNames[fieldOrdinal];
            var columnNameOrPosition = !String.IsNullOrWhiteSpace(dataReaderFieldName)
                ? $"column '{dataReaderFieldName}'"
                : $"{(fieldOrdinal + 1).OrdinalizeEnglish()} column";
            var dataReaderFieldType = dataReaderFieldTypes[fieldOrdinal];

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

            var getFieldValue =
                // getFieldValue = record.GetXXX(fieldOrdinal);
                MaterializerFactoryHelper.CreateGetRecordFieldValueExpression(
                    dataRecordParameter,
                    fieldOrdinalExpression,
                    fieldOrdinal,
                    dataReaderFieldName,
                    dataReaderFieldType
                );

            if (valueTupleFieldType.IsEnumOrNullableEnumType())
            {
                // getFieldValue =
                // try
                // {
                //   EnumConverter.ConvertValueToEnumMember<ValueTupleFieldType>(getFieldValue);
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
                                .MakeGenericMethod(valueTupleFieldType),
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
                                        $"The {columnNameOrPosition} returned by the SQL statement contains a " +
                                        $"value that could not be converted to the enum type {valueTupleFieldType} " +
                                        $"of the corresponding field of the value tuple type {valueTupleType}. See " +
                                        $"inner exception for details."
                                    ),
                                    exceptionParameter
                                ),
                                valueTupleFieldType
                            )
                        )
                    );
            }

            // IDataReader returns a value of the SQL data type CHAR(1) as a string.
            // If valueTupleFieldType is Char or Char?, we need to get the first character of the string.
            if (valueTupleFieldType.IsCharOrNullableCharType())
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
                                    $"The {columnNameOrPosition} returned by the SQL statement contains the " +
                                    $"string '"
                                ),
                                stringValueVariable,
                                Expression.Constant(
                                    $"', which could not be converted to the type {valueTupleFieldType} of the " +
                                    $"corresponding field of the value tuple type {valueTupleType}. The string must " +
                                    $"be exactly one character long."
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

            // valueTupleFieldValueX = (ValueTupleFieldType) getFieldValue;
            var assignFieldValueToVariable = Expression.Assign(
                valueTupleFieldValueVariables[fieldOrdinal],
                Expression.Convert(getFieldValue, valueTupleFieldType)
            );

            if (valueTupleFieldType.IsReferenceTypeOrNullableType())
            {
                // if (!isFieldValueDbNull) { valueTupleFieldValueX = (ValueTupleFieldType) getFieldValue; }
                expressions.Add(
                    Expression.IfThen(
                        Expression.IsFalse(isFieldValueDbNullVariable),
                        assignFieldValueToVariable
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
                //   valueTupleFieldValueX = (ValueTupleFieldType) getFieldValue;
                // }
                //
                // If the field value is DBNull and the field type of the value tuple field is non-nullable, we must
                // throw an exception.
                expressions.Add(
                    Expression.IfThenElse(
                        isFieldValueDbNullVariable,
                        Expression.Throw(
                            Expression.New(
                                typeof(InvalidCastException).GetConstructor([typeof(String)])!,
                                Expression.Constant(
                                    $"The {columnNameOrPosition} returned by the SQL statement contains a NULL " +
                                    $"value, but the corresponding field of the value tuple type {valueTupleType} " +
                                    $"is non-nullable."
                                )
                            ),
                            typeof(void)
                        ),
                        assignFieldValueToVariable
                    )
                );
            }
        }

        var valueTupleConstructor = valueTupleType.GetConstructor(valueTupleTypeArguments)!;

        // valueTuple = new TValueTuple(valueTupleFieldValue1, valueTupleFieldValue2, ...);
        expressions.Add(
            Expression.Assign(
                valueTupleVariable,
                Expression.New(valueTupleConstructor, valueTupleFieldValueVariables.Cast<Expression>())
            )
        );

        // return valueTuple;
        var returnTarget = Expression.Label(valueTupleType);
        var returnExpression = Expression.Return(returnTarget, valueTupleVariable);
        var returnLabel = Expression.Label(returnTarget, Expression.Default(valueTupleType));

        expressions.Add(returnExpression);
        expressions.Add(returnLabel);

        return Expression
            .Lambda(
                Expression.Block(
                    [
                        valueTupleVariable,
                        isFieldValueDbNullVariable,
                        .. valueTupleFieldValueVariables,
                        .. stringValueVariables
                    ],
                    expressions
                ),
                dataRecordParameter
            )
            .Compile();
    }

    /// <summary>
    /// Validates that instances of the value tuple type <paramref name="valueTupleType" /> can be materialized from
    /// the data in <paramref name="dataReader" />.
    /// </summary>
    /// <param name="valueTupleType">The type of value tuple to materialize.</param>
    /// <param name="valueTupleTypeArguments">
    /// The type arguments of the value tuple type <paramref name="valueTupleType" />.
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
    ///                 The value tuple type <paramref name="valueTupleType" /> does not have the same number of
    ///                 fields as <paramref name="dataReader" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///             A field of <paramref name="dataReader" /> has a field type which does not match the field type of
    ///             the corresponding field of the value tuple type <paramref name="valueTupleType" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataReader" /> contains a field with a field type that is not supported.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    private static void ValidateDataReader(
        Type valueTupleType,
        Type[] valueTupleTypeArguments,
        IDataReader dataReader,
        String[] dataReaderFieldNames,
        Type[] dataReaderFieldTypes
    )
    {
        if (dataReader.FieldCount == 0)
        {
            throw new ArgumentException("The SQL statement did not return any columns.", nameof(dataReader));
        }

        if (dataReader.FieldCount != valueTupleTypeArguments.Length)
        {
            throw new ArgumentException(
                $"The SQL statement returned {"column".ToQuantity(dataReader.FieldCount)}, but the value tuple type " +
                $"{valueTupleType} has {"field".ToQuantity(valueTupleTypeArguments.Length)}. Make sure that the SQL " +
                $"statement returns the same number of columns as the number of fields in the value tuple type.",
                nameof(dataReader)
            );
        }

        for (var fieldOrdinal = 0; fieldOrdinal < dataReader.FieldCount; fieldOrdinal++)
        {
            var dataReaderFieldName = dataReaderFieldNames[fieldOrdinal];

            var columnNameOrPosition = !String.IsNullOrWhiteSpace(dataReaderFieldName)
                ? $"column '{dataReaderFieldName}'"
                : $"{(fieldOrdinal + 1).OrdinalizeEnglish()} column";

            var dataReaderFieldType = dataReaderFieldTypes[fieldOrdinal];

            var valueTupleFieldType = valueTupleTypeArguments[fieldOrdinal];
            var underlyingValueTupleFieldType = Nullable.GetUnderlyingType(valueTupleFieldType);

            // Check whether the field type of the data reader is compatible with the field type of the value tuple
            // field:
            var isDataReaderFieldTypeCompatibleWithValueTupleFieldType =
                // If the value tuple field type is Object, we can assign any type of field value to it:
                valueTupleFieldType == typeof(Object)
                ||
                // Direct match:
                dataReaderFieldType == valueTupleFieldType
                ||
                // Match for Nullable<T>:
                (underlyingValueTupleFieldType is not null && dataReaderFieldType == underlyingValueTupleFieldType)
                ||
                // Special case: Char value tuple fields can be populated from String fields:
                (valueTupleFieldType.IsCharOrNullableCharType() && dataReaderFieldType == typeof(String))
                ||
                // Enums are also supported:
                valueTupleFieldType.IsEnumOrNullableEnumType();

            if (!isDataReaderFieldTypeCompatibleWithValueTupleFieldType)
            {
                throw new ArgumentException(
                    $"The data type {dataReaderFieldType} of the {columnNameOrPosition} returned by the SQL " +
                    $"statement does not match the field type {valueTupleFieldType} of the corresponding " +
                    $"field of the value tuple type {valueTupleType}.",
                    nameof(dataReader)
                );
            }

            if (!MaterializerFactoryHelper.IsDataRecordTypedGetMethodAvailable(dataReaderFieldType))
            {
                throw new ArgumentException(
                    $"The data type {dataReaderFieldType} of the {columnNameOrPosition} returned by the SQL " +
                    $"statement is not supported.",
                    nameof(dataReader)
                );
            }
        }
    }

    private static readonly ConcurrentDictionary<MaterializerCacheKey, Delegate> materializerCache = [];

    /// <summary>
    /// A cache key used to uniquely identify a value tuple materializer.
    /// </summary>
    /// <param name="valueTupleType">The type of value tuple the materializer materializes.</param>
    /// <param name="dataReaderFieldNames">
    /// The field names of the <see cref="IDataReader" /> from which to materialize.
    /// The order of the names must match the order of the fields in the data reader.
    /// </param>
    /// <param name="dataReaderFieldTypes">
    /// The field types of the <see cref="IDataReader" /> from which to materialize.
    /// The order of the types must match the order of the fields in the data reader.
    /// </param>
    private readonly struct MaterializerCacheKey(
        Type valueTupleType,
        String[] dataReaderFieldNames,
        Type[] dataReaderFieldTypes
    )
        : IEquatable<MaterializerCacheKey>
    {
        /// <summary>
        /// The type of value tuple the materializer materializes.
        /// </summary>
        public Type ValueTupleType { get; } = valueTupleType;

        /// <inheritdoc />
        public Boolean Equals(MaterializerCacheKey other) =>
            this.ValueTupleType == other.ValueTupleType &&
            this.DataReaderFieldNames.SequenceEqual(other.DataReaderFieldNames) &&
            this.DataReaderFieldTypes.SequenceEqual(other.DataReaderFieldTypes);

        /// <inheritdoc />
        public override Boolean Equals(Object? obj) =>
            obj is MaterializerCacheKey other && this.Equals(other);

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(this.ValueTupleType);

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
