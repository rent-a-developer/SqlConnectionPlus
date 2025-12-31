// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace RentADeveloper.SqlConnectionPlus.Extensions;

/// <summary>
/// Provides extension methods for the type <see cref="Object" />.
/// </summary>
internal static class ObjectExtensions
{
    /// <summary>
    /// Converts this value to its string representation suffixed by the value's type fullname.
    /// </summary>
    /// <param name="value">The value to convert to its string representation.</param>
    /// <returns>A string representation of <paramref name="value" /> suffixed by the value's type fullname.</returns>
    internal static String ToDebugString(this Object? value) =>
        value switch
        {
            null => "{null}",
            DBNull => "{DBNull}",
            _ =>
                $"'{
                    value switch
                    {
                        String stringValue =>
                            stringValue,

                        Char charValue =>
                            charValue.ToString(),

                        Boolean booleanValue =>
                            booleanValue ? "True" : "False",

                        Decimal decimalValue =>
                            decimalValue.ToString("N", CultureInfo.InvariantCulture),

                        Single singleValue =>
                            singleValue.ToString("G9", CultureInfo.InvariantCulture),

                        Double doubleValue =>
                            doubleValue.ToString("G17", CultureInfo.InvariantCulture),

                        Byte byteValue =>
                            byteValue.ToString("G", CultureInfo.InvariantCulture),

                        SByte sbyteValue =>
                            sbyteValue.ToString("G", CultureInfo.InvariantCulture),

                        Int16 int16Value =>
                            int16Value.ToString("G", CultureInfo.InvariantCulture),

                        UInt16 uint16Value =>
                            uint16Value.ToString("G", CultureInfo.InvariantCulture),

                        Int32 int32Value =>
                            int32Value.ToString("G", CultureInfo.InvariantCulture),

                        UInt32 uint32Value =>
                            uint32Value.ToString("G", CultureInfo.InvariantCulture),

                        Int64 int64Value =>
                            int64Value.ToString("G", CultureInfo.InvariantCulture),

                        UInt64 uint64Value =>
                            uint64Value.ToString("G", CultureInfo.InvariantCulture),

                        Byte[] bytesValue =>
                            Convert.ToBase64String(bytesValue),

                        Guid guidValue =>
                            guidValue.ToString("D", CultureInfo.InvariantCulture),

                        DateTime dateTimeValue =>
                            dateTimeValue.ToString("O", CultureInfo.InvariantCulture),

                        DateTimeOffset dateTimeOffsetValue =>
                            dateTimeOffsetValue.ToString("O", CultureInfo.InvariantCulture),

                        TimeSpan timeSpanValue =>
                            timeSpanValue.ToString("c", CultureInfo.InvariantCulture),

                        Enum enumValue =>
                            enumValue.ToString(),

                        _ =>
                            JsonSerializer.Serialize(value, jsonSerializerOptions)
                    }
                }' ({value.GetType()})"
        };

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = false,
        MaxDepth = 10,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}
