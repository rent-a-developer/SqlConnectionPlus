// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus;

/// <summary>
/// Specifies how <see cref="Enum" /> values are serialized when sent to a database.
/// </summary>
public enum EnumSerializationMode
{
    /// <summary>
    /// <see cref="Enum" /> values are serialized as integers (<see cref="Int32" /> / SQL Data Type "INT").
    /// </summary>
    Integers = 0,

    /// <summary>
    /// <see cref="Enum" /> values are serialized as strings (<see cref="String" /> / SQL Data Type "NVARCHAR(200)").
    /// </summary>
    Strings = 1
}
