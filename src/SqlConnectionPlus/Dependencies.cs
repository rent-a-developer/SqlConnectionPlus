// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus;

/// <summary>
/// Provides dependencies for SqlConnectionPlus.
/// </summary>
internal static class Dependencies
{
    /// <summary>
    /// The factory to use to create instances of <see cref="SqlCommand" />.
    /// </summary>
    /// <remarks>
    /// This property is only used in tests to test the cancellation of SQL statements.
    /// </remarks>
    internal static ISqlCommandFactory SqlCommandFactory { get; set; } = new DefaultSqlCommandFactory();
}
