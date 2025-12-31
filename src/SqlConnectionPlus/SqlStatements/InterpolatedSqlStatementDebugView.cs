// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus.SqlStatements;

/// <summary>
/// A debug view proxy for the type <see cref="InterpolatedSqlStatement" />.
/// </summary>
/// <param name="statement">The SQL statement to view in the debugger.</param>
internal sealed class InterpolatedSqlStatementDebugView(InterpolatedSqlStatement statement)
{
    /// <summary>
    /// The code of the SQL statement.
    /// </summary>
    public String Code =>
        statement.Code;

    /// <summary>
    /// The debug view of the SQL statement.
    /// </summary>
    public String DebugView =>
        statement.ToString();

    /// <summary>
    /// The parameters of the SQL statement.
    /// The keys are the parameter names, and the values are the parameter values.
    /// </summary>
    public IReadOnlyDictionary<String, Object?> Parameters =>
        statement.Parameters;

    /// <summary>
    /// The temporary tables used in the SQL statement.
    /// </summary>
    public IEnumerable<InterpolatedTemporaryTable> TemporaryTables =>
        statement.TemporaryTables;
}
