// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using Fasterflect;

namespace RentADeveloper.SqlConnectionPlus.Entities;

/// <summary>
/// Metadata of an entity type required to perform DELETE, INSERT and UPDATE operations.
/// </summary>
/// <param name="DeleteSql">The SQL DELETE statement for the entity type.</param>
/// <param name="InsertSql">The SQL INSERT statement for the entity type.</param>
/// <param name="UpdateSql">The SQL UPDATE statement for the entity type.</param>
/// <param name="TableName">The name of the database table where entities of the entity type are stored.</param>
/// <param name="KeyPropertyName">The name of the key property of the entity type.</param>
/// <param name="KeyPropertyType">The property type of the key property of the entity type.</param>
/// <param name="KeyPropertyGetter">The getter function for the key property of the entity type.</param>
/// <param name="PropertyNames">The names of the properties of the entity type.</param>
/// <param name="PropertyGetters">
/// The getter functions for the properties of the entity type.
/// Each element corresponds to the property at the same index in <paramref name="PropertyNames" />.
/// </param>
/// <param name="IsPropertyTypeDateTimeOrNullableDateTime">
/// An array indicating, for each property of the entity type, whether its type is <see cref="DateTime" /> or
/// <see cref="Nullable{DateTime}" />.
/// Each element corresponds to the property at the same index in <paramref name="PropertyNames" />.
/// </param>
/// <param name="IsPropertyTypeEnumOrNullableEnum">
/// An array indicating, for each property of the entity type, whether its type is <see cref="Enum" /> or
/// nullable <see cref="Enum" />.
/// Each element corresponds to the property at the same index in <paramref name="PropertyNames" />.
/// </param>
/// <param name="IsPropertyTypeByteArray">
/// An array indicating, for each property of the entity type, whether its type is Byte[].
/// Each element corresponds to the property at the same index in <paramref name="PropertyNames" />.
/// </param>
internal sealed record EntityTypeMetadata(
    String InsertSql,
    String UpdateSql,
    String DeleteSql,
    String TableName,
    String KeyPropertyName,
    Type KeyPropertyType,
    MemberGetter KeyPropertyGetter,
    String[] PropertyNames,
    MemberGetter[] PropertyGetters,
    Boolean[] IsPropertyTypeByteArray,
    Boolean[] IsPropertyTypeDateTimeOrNullableDateTime,
    Boolean[] IsPropertyTypeEnumOrNullableEnum
);
