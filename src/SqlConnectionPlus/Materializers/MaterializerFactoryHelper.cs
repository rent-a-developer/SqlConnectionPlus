// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Linq.Expressions;
using System.Reflection;

namespace RentADeveloper.SqlConnectionPlus.Materializers;

/// <summary>
/// Provides helper functions for materializer factories.
/// </summary>
internal static class MaterializerFactoryHelper
{
    /// <summary>
    /// The <see cref="IDataRecord.GetValue(Int32)" /> method.
    /// </summary>
    internal static MethodInfo DataRecordGetValueMethod { get; } = typeof(IDataRecord)
        .GetMethod(nameof(IDataRecord.GetValue))!;

    /// <summary>
    /// The <see cref="IDataRecord.IsDBNull(Int32)" /> method.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal static MethodInfo DataRecordIsDBNullMethod { get; } = typeof(IDataRecord)
        .GetMethod(nameof(IDataRecord.IsDBNull))!;

    /// <summary>
    /// The <see cref="EnumConverter.ConvertValueToEnumMember{TTarget}" /> method.
    /// </summary>
    internal static MethodInfo EnumConverterConvertValueToEnumMemberMethod { get; } = typeof(EnumConverter)
        .GetMethod(nameof(EnumConverter.ConvertValueToEnumMember), BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>
    /// The 'Chars' property of the <see cref="String" /> type.
    /// </summary>
    internal static PropertyInfo StringCharsProperty { get; } = typeof(String)
        .GetProperty("Chars", BindingFlags.Instance | BindingFlags.Public)!;

    /// <summary>
    /// The <see cref="String.Concat(String, String, String)" /> method.
    /// </summary>
    internal static MethodInfo StringConcatMethod { get; } = typeof(String)
        .GetMethod(nameof(String.Concat), [typeof(String), typeof(String), typeof(String)])!;

    /// <summary>
    /// The <see cref="String.Length" /> property.
    /// </summary>
    internal static PropertyInfo StringLengthProperty { get; } = typeof(String)
        .GetProperty(nameof(String.Length), BindingFlags.Instance | BindingFlags.Public)!;

    /// <summary>
    /// Creates an <see cref="Expression" /> that gets the value of a field of the specified field type from an
    /// <see cref="IDataRecord" /> using one of the typed <see cref="IDataRecord" />.GetXXX methods.
    /// </summary>
    /// <param name="dataRecordExpression">
    /// The expression of the <see cref="IDataRecord" /> to get the field value from.
    /// </param>
    /// <param name="fieldOrdinalExpression">
    /// The expression of the field ordinal of the field to get the value from.
    /// </param>
    /// <param name="fieldOrdinal">The field ordinal of the field to get the value from.</param>
    /// <param name="fieldName">The field name of the field to get the value from.</param>
    /// <param name="fieldType">The field type of the field to get the value from.</param>
    /// <returns>
    /// An <see cref="Expression" /> that gets the value of a field of the specified field type from an
    /// <see cref="IDataRecord" /> using one of the typed <see cref="IDataRecord" />.GetXXX methods.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The specified type <paramref name="fieldType" /> is not supported.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="dataRecordExpression" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="fieldOrdinalExpression" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="fieldType" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    internal static Expression CreateGetRecordFieldValueExpression(
        Expression dataRecordExpression,
        Expression fieldOrdinalExpression,
        Int32 fieldOrdinal,
        String? fieldName,
        Type fieldType
    )
    {
        ArgumentNullException.ThrowIfNull(dataRecordExpression);
        ArgumentNullException.ThrowIfNull(fieldOrdinalExpression);
        ArgumentNullException.ThrowIfNull(fieldType);

        if (fieldType == typeof(Byte[]))
        {
            // Special handling for byte arrays since IDataRecord does not have a GetBytes method that returns
            // a byte array directly.
            return
                Expression.Convert(
                    Expression.Call(
                        dataRecordExpression,
                        DataRecordGetValueMethod,
                        fieldOrdinalExpression
                    ),
                    typeof(Byte[])
                );
        }

        if (fieldType == typeof(TimeSpan))
        {
            // Special handling for the type TimeSpan since IDataRecord does not have a GetTimeSpan method that
            // returns a TimeSpan directly.
            return
                Expression.Convert(
                    Expression.Call(
                        dataRecordExpression,
                        DataRecordGetValueMethod,
                        fieldOrdinalExpression
                    ),
                    typeof(TimeSpan)
                );
        }

        if (fieldType == typeof(DateTimeOffset))
        {
            // Special handling for the type DateTimeOffset since IDataRecord does not have a GetDateTimeOffset method
            // that returns a DateTimeOffset directly.
            return
                Expression.Convert(
                    Expression.Call(
                        dataRecordExpression,
                        DataRecordGetValueMethod,
                        fieldOrdinalExpression
                    ),
                    typeof(DateTimeOffset)
                );
        }

        if (!dataRecordTypedGetMethods.TryGetValue(fieldType, out var recordGetMethod))
        {
            if (!String.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentException(
                    $"The data type {fieldType} of the column '{fieldName}' returned by the SQL statement is not " +
                    $"supported.",
                    nameof(fieldType)
                );
            }

            throw new ArgumentException(
                $"The data type {fieldType} of the {(fieldOrdinal + 1).OrdinalizeEnglish()} column returned by the " +
                $"SQL statement is not supported.",
                nameof(fieldType)
            );
        }

        return Expression.Call(
            dataRecordExpression,
            recordGetMethod,
            fieldOrdinalExpression
        );
    }

    /// <summary>
    /// Determines whether a typed <see cref="IDataRecord" />.GetXXX method is available for the field type
    /// <paramref name="fieldType" />.
    /// </summary>
    /// <param name="fieldType">The field type to check.</param>
    /// <returns>
    /// <see langword="true" /> if a typed <see cref="IDataRecord" />.GetXXX method is available for the field type
    /// <paramref name="fieldType" />; otherwise, <see langword="false" />.
    /// </returns>
    internal static Boolean IsDataRecordTypedGetMethodAvailable(Type fieldType) =>
        dataRecordTypedGetMethods.ContainsKey(fieldType)
        ||
        // The method CreateGetRecordFieldValueExpression has special handling for Byte[], TimeSpan, and DateTimeOffset.
        fieldType == typeof(Byte[])
        ||
        fieldType == typeof(TimeSpan)
        ||
        fieldType == typeof(DateTimeOffset);

    private static readonly Dictionary<Type, MethodInfo> dataRecordTypedGetMethods = new()
    {
        { typeof(Boolean), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetBoolean))! },
        { typeof(Byte), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetByte))! },
        { typeof(Char), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetString))! },
        { typeof(DateTime), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDateTime))! },
        { typeof(Decimal), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDecimal))! },
        { typeof(Double), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDouble))! },
        { typeof(Single), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetFloat))! },
        { typeof(Guid), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetGuid))! },
        { typeof(Int16), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt16))! },
        { typeof(Int32), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt32))! },
        { typeof(Int64), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt64))! },
        { typeof(String), typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetString))! }
    };
}
