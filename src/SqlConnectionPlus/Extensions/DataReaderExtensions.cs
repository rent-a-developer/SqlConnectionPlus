// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus.Extensions;

/// <summary>
/// Provides extension methods for the type <see cref="IDataReader" />
/// </summary>
internal static class DataReaderExtensions
{
    /// <summary>
    /// Gets the names of all fields in this data reader.
    /// </summary>
    /// <param name="dataReader">The data reader from which to retrieve field names.</param>
    /// <returns>
    /// An array containing the names of the fields in <paramref name="dataReader" />.
    /// The order of names in the array corresponds to the order of the fields in <paramref name="dataReader" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataReader" /> is <see langword="null" />.</exception>
    internal static String[] GetFieldNames(this IDataReader dataReader)
    {
        ArgumentNullException.ThrowIfNull(dataReader);

        var result = new String[dataReader.FieldCount];

        for (var i = 0; i < dataReader.FieldCount; i++)
        {
            result[i] = dataReader.GetName(i);
        }

        return result;
    }

    /// <summary>
    /// Gets the data types of all fields in this data reader.
    /// </summary>
    /// <param name="dataReader">The data reader from which to retrieve field types.</param>
    /// <returns>
    /// An array containing the data types of the fields in <paramref name="dataReader" />.
    /// The order of types in the array corresponds to the order of the fields in <paramref name="dataReader" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataReader" /> is <see langword="null" />.</exception>
    internal static Type[] GetFieldTypes(this IDataReader dataReader)
    {
        ArgumentNullException.ThrowIfNull(dataReader);

        var result = new Type[dataReader.FieldCount];

        for (var i = 0; i < dataReader.FieldCount; i++)
        {
            result[i] = dataReader.GetFieldType(i);
        }

        return result;
    }
}
