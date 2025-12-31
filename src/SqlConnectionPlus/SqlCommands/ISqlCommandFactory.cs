// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus.SqlCommands;

/// <summary>
/// Represents a factory that creates instances of <see cref="SqlCommand" />.
/// </summary>
internal interface ISqlCommandFactory
{
    /// <summary>
    /// Creates an instance of <see cref="SqlCommand" /> with the specified settings.
    /// </summary>
    /// <param name="connection">The connection to use to create the <see cref="SqlCommand" />.</param>
    /// <param name="commandText">The command text to assign to the <see cref="SqlCommand" />.</param>
    /// <param name="transaction">The SQL transaction to assign to the <see cref="SqlCommand" />.</param>
    /// <param name="commandTimeout">The command timeout to assign to the <see cref="SqlCommand" />.</param>
    /// <param name="commandType">The <see cref="CommandType" /> to assign to the <see cref="SqlCommand" />.</param>
    /// <returns>An instance of <see cref="SqlCommand" /> with the specified settings.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="connection" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="commandText" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public SqlCommand CreateSqlCommand(
        SqlConnection connection,
        String commandText,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text
    );
}
