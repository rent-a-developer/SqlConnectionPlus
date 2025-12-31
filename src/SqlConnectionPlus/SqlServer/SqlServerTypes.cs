// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus.SqlServer;

/// <summary>
/// Provides functions related to SQL Server data types.
/// </summary>
internal static class SqlServerTypes
{
    /// <summary>
    /// Gets the corresponding SQL Server data type for the specified type.
    /// </summary>
    /// <param name="type">The type to get the SQL Server data type for.</param>
    /// <param name="enumSerializationMode">The mode to use to serialize <see cref="Enum" /> values.</param>
    /// <returns>
    /// The corresponding SQL Server data type for the specified type.
    /// If <paramref name="type" /> is an <see cref="Enum" /> or a nullable <see cref="Enum" />, the returned SQL
    /// Server data type will be either 'NVARCHAR(200)' or 'INT', depending on the value of
    /// <paramref name="enumSerializationMode" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="enumSerializationMode" /> is not a valid <see cref="EnumSerializationMode" />
    /// value.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The type <paramref name="type" /> could not be mapped to an SQL Server data type.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    internal static String GetSqlServerDataType(
        Type type,
        EnumSerializationMode enumSerializationMode
    )
    {
        ArgumentNullException.ThrowIfNull(type);

        // Unwrap Nullable<T> types:
        var effectiveType = Nullable.GetUnderlyingType(type) ?? type;

        if (effectiveType.IsEnum)
        {
            return enumSerializationMode switch
            {
                EnumSerializationMode.Strings => "NVARCHAR(200)",

                EnumSerializationMode.Integers => "INT",

                _ => throw new ArgumentOutOfRangeException(
                    nameof(enumSerializationMode),
                    enumSerializationMode,
                    $"The {nameof(EnumSerializationMode)} {enumSerializationMode.ToDebugString()} is not supported."
                )
            };
        }

        if (!typeToSqlDataType.TryGetValue(effectiveType, out var result))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                $"Could not map the type {type} to an SQL Server data type."
            );
        }

        return result;
    }

    private static readonly Dictionary<Type, String> typeToSqlDataType = new()
    {
        { typeof(Boolean), "BIT" },
        { typeof(Byte), "TINYINT" },
        { typeof(Byte[]), "VARBINARY(MAX)" },
        { typeof(Char), "CHAR(1)" },
        { typeof(DateTime), "DATETIME2" },
        { typeof(DateTimeOffset), "DATETIMEOFFSET" },
        { typeof(Decimal), "DECIMAL(28,10)" },
        { typeof(Double), "FLOAT" },
        { typeof(Guid), "UNIQUEIDENTIFIER" },
        { typeof(Int16), "SMALLINT" },
        { typeof(Int32), "INT" },
        { typeof(Int64), "BIGINT" },
        { typeof(Object), "sql_variant" },
        { typeof(Single), "REAL" },
        { typeof(String), "NVARCHAR(MAX)" },
        { typeof(TimeSpan), "TIME" }
    };
}
