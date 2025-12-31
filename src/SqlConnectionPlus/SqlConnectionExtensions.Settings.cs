// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus;

/// <summary>
/// Provides extension members for the type <see cref="SqlConnection" />.
/// </summary>
public static partial class SqlConnectionExtensions
{
    /// <summary>
    /// Controls how <see cref="Enum" /> values are serialized when they are sent to a database using one of the
    /// following methods:
    /// 
    /// 1. When an entity containing an enum property is inserted via
    /// <see cref="InsertEntities{TEntity}" />, <see cref="InsertEntitiesAsync{TEntity}" />,
    /// <see cref="InsertEntity{TEntity}" /> or <see cref="InsertEntityAsync{TEntity}" />.
    /// 
    /// 2. When an entity containing an enum property is updated via
    /// <see cref="UpdateEntities{TEntity}" />, <see cref="UpdateEntitiesAsync{TEntity}" />,
    /// <see cref="UpdateEntity{TEntity}" /> or <see cref="UpdateEntityAsync{TEntity}" />.
    /// 
    /// 3. When an enum value is passed as a parameter to an SQL statement via <see cref="Parameter" />.
    /// 
    /// 4. When a sequence of enum values is passed as a temporary table to an SQL statement via
    /// <see cref="TemporaryTable{T}" />.
    /// 
    /// 5. When objects containing an enum property are passed as a temporary table to an SQL statement via
    /// <see cref="TemporaryTable{T}" />.
    /// 
    /// The default is <see cref="SqlConnectionPlus.EnumSerializationMode.Strings" />.
    /// </summary>
    /// <remarks>
    /// <strong>Thread Safety:</strong> This is a static mutable property. To avoid race conditions in multi-threaded
    /// applications, set this property during application initialization before any database operations are performed,
    /// and do not change it afterward. Changing this value while database operations are in progress from multiple
    /// threads may lead to inconsistent enum serialization behavior.
    /// </remarks>
    public static EnumSerializationMode EnumSerializationMode { get; set; } = EnumSerializationMode.Strings;
}
