// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.TemporaryTables;

namespace RentADeveloper.SqlConnectionPlus.SqlCommands;

/// <summary>
/// Handles the disposal of an <see cref="SqlCommand" /> and its associated resources.
/// When disposed, disposes the command, any temporary tables created for the command, and the cancellation token
/// registration associated with the command.
/// </summary>
/// <param name="command">The SQL command to dispose.</param>
/// <param name="temporaryTableDisposers">The disposers of the temporary tables created for the command.</param>
/// <param name="cancellationTokenRegistration">
/// The registration of the cancellation token associated with the command.
/// </param>
internal sealed class SqlCommandDisposer(
    SqlCommand command,
    TemporarySqlServerTableDisposer[] temporaryTableDisposers,
    CancellationTokenRegistration cancellationTokenRegistration
) : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Disposes the SQL command and its associated resources.
    /// </summary>
    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;

        cancellationTokenRegistration.Dispose();

        foreach (var tableDisposer in temporaryTableDisposers)
        {
            tableDisposer.Dispose();
        }

        command.Dispose();
    }

    /// <summary>
    /// Asynchronously disposes the SQL command and its associated resources.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;

        await cancellationTokenRegistration.DisposeAsync().ConfigureAwait(false);

        foreach (var tableDisposer in temporaryTableDisposers)
        {
            await tableDisposer.DisposeAsync().ConfigureAwait(false);
        }

        await command.DisposeAsync().ConfigureAwait(false);
    }

    private Boolean isDisposed;
}
