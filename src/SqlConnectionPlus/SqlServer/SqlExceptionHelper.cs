// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus.SqlServer;

/// <summary>
/// Provides helper methods for working with <see cref="SqlException" /> objects.
/// </summary>
internal static class SqlExceptionHelper
{
    /// <summary>
    /// Determines whether <paramref name="exception" /> was caused by an SQL statement being cancelled via
    /// <paramref name="cancellationToken" />.
    /// </summary>
    /// <param name="exception">The SQL exception to inspect.</param>
    /// <param name="cancellationToken">The token via the SQL statement may have been cancelled.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="exception" /> was caused by an SQL statement being cancelled via
    /// <paramref name="cancellationToken" />; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception" /> is <see langword="null" />.</exception>
    internal static Boolean WasSqlStatementCancelledByCancellationToken(
        SqlException exception,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Unfortunately SQL Server does not raise a specific error when a statement is being cancelled by the user.
        // However, if a cancellation was requested via the specified cancellation token and
        // SQL Server raised an error with class 11, number 0 and state 0 we can be pretty sure the error was raised
        // because of the cancellation.

        if (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        foreach (SqlError error in exception.Errors)
        {
            if (error is { Class: 11, Number: 0, State: 0 })
            {
                return true;
            }
        }

        return false;
    }
}
