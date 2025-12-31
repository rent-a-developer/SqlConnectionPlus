// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.SqlServer;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.TemporaryTables;

namespace RentADeveloper.SqlConnectionPlus.SqlCommands;

/// <summary>
/// Builds <see cref="SqlCommand" /> instances from instances of <see cref="InterpolatedSqlStatement" />.
/// </summary>
internal static class SqlCommandBuilder
{
    /// <summary>
    /// Builds a new <see cref="SqlCommand" /> for the specified SQL statement.
    /// Adds the parameters of the specified SQL statement to the command, creates the temporary tables for the
    /// specified SQL statement and registers a callback to cancel the specified SQL command if the specified
    /// cancellation token is triggered.
    /// </summary>
    /// <param name="connection">The SQL connection to use to create the command.</param>
    /// <param name="statement">The SQL statement for which to create the command.</param>
    /// <param name="transaction">The SQL transaction to assign to the command.</param>
    /// <param name="commandTimeout">The command timeout to assign to the command.</param>
    /// <param name="commandType">The <see cref="CommandType" /> to assign to the command.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the command.</param>
    /// <returns>
    /// A tuple containing the created <see cref="SqlCommand" /> and a disposer for the command and its associated
    /// resources.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    internal static (SqlCommand, SqlCommandDisposer) BuildSqlCommand(
        SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(connection);

        var (command, cancellationTokenRegistration) = BuildSqlCommandCore(
            connection,
            statement,
            transaction,
            commandType,
            commandTimeout,
            cancellationToken
        );

        var temporaryTableDisposers = BuildTemporaryTables(statement, connection, transaction, cancellationToken);

        return (command, new(command, temporaryTableDisposers, cancellationTokenRegistration));
    }

    /// <summary>
    /// Asynchronously builds a new <see cref="SqlCommand" /> for the specified SQL statement.
    /// Adds the parameters of the specified SQL statement to the command, creates the temporary tables for the
    /// specified SQL statement and registers a callback to cancel the specified SQL command if the specified
    /// cancellation token is triggered.
    /// </summary>
    /// <param name="connection">The SQL connection to use to create the command.</param>
    /// <param name="statement">The SQL statement for which to create the command.</param>
    /// <param name="transaction">The SQL transaction to assign to the command.</param>
    /// <param name="commandTimeout">The command timeout to assign to the command.</param>
    /// <param name="commandType">The <see cref="CommandType" /> to assign to the command.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the command.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain a tuple containing the created <see cref="SqlCommand" /> and
    /// a disposer for the command and its associated resources.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    internal static async Task<(SqlCommand, SqlCommandDisposer)> BuildSqlCommandAsync(
        SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(connection);

        var (command, cancellationTokenRegistration) = BuildSqlCommandCore(
            connection,
            statement,
            transaction,
            commandType,
            commandTimeout,
            cancellationToken
        );

        var temporaryTableDisposers = await CreateTemporaryTablesAsync(
            statement,
            connection,
            transaction,
            cancellationToken
        ).ConfigureAwait(false);

        return (command, new(command, temporaryTableDisposers, cancellationTokenRegistration));
    }

    /// <summary>
    /// Builds a new <see cref="SqlCommand" /> for the specified SQL statement.
    /// Adds the parameters of the specified SQL statement to the command and registers a callback to cancel the
    /// specified SQL command if the specified cancellation token is triggered.
    /// </summary>
    /// <param name="connection">The SQL connection to use to create the command.</param>
    /// <param name="statement">The SQL statement for which to create the command.</param>
    /// <param name="transaction">The SQL transaction to assign to the command.</param>
    /// <param name="commandType">The <see cref="CommandType" /> to assign to the command.</param>
    /// <param name="commandTimeout">The command timeout to assign to the command.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the command.</param>
    /// <returns>
    /// A tuple containing the created <see cref="SqlCommand" /> and the cancellation token registration for the
    /// command.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is <see langword="null" />.</exception>
    private static (SqlCommand, CancellationTokenRegistration) BuildSqlCommandCore(
        SqlConnection connection,
        InterpolatedSqlStatement statement,
        SqlTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        TimeSpan? commandTimeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var command = Dependencies.SqlCommandFactory.CreateSqlCommand(
            connection,
            statement.Code,
            transaction,
            commandTimeout,
            commandType
        );

        var cancellationTokenRegistration = DbCommandHelper.RegisterDbCommandCancellation(
            command,
            cancellationToken
        );

        foreach (var (name, value) in statement.Parameters)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            command.Parameters.Add(parameter);
        }

        return (command, cancellationTokenRegistration);
    }

    /// <summary>
    /// Builds the temporary tables for the specified SQL statement.
    /// </summary>
    /// <param name="statement">The SQL statement for which to build the temporary tables.</param>
    /// <param name="connection">The SQL connection to use to build the temporary tables.</param>
    /// <param name="transaction">The SQL transaction within to build the temporary tables.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// An array of <see cref="TemporarySqlServerTableDisposer" /> instances that can be used to dispose the built
    /// temporary tables.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    private static TemporarySqlServerTableDisposer[] BuildTemporaryTables(
        InterpolatedSqlStatement statement,
        SqlConnection connection,
        SqlTransaction? transaction,
        CancellationToken cancellationToken
    )
    {
        var temporaryTableDisposers = new TemporarySqlServerTableDisposer[statement.TemporaryTables.Count];

        try
        {
            for (var i = 0; i < statement.TemporaryTables.Count; i++)
            {
                var interpolatedTemporaryTable = statement.TemporaryTables[i];

                temporaryTableDisposers[i] = TemporarySqlServerTableBuilder.BuildTemporaryTable(
                    connection,
                    transaction,
                    interpolatedTemporaryTable.Name,
                    interpolatedTemporaryTable.Values,
                    interpolatedTemporaryTable.ValuesType,
                    cancellationToken
                );
            }
        }
        catch (SqlException exception) when (
            SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
        )
        {
            throw new OperationCanceledException(cancellationToken);
        }

        return temporaryTableDisposers;
    }

    /// <summary>
    /// Asynchronously builds the temporary tables for the specified SQL statement.
    /// </summary>
    /// <param name="statement">The SQL statement for which to build the temporary tables.</param>
    /// <param name="connection">The SQL connection to use to build the temporary tables.</param>
    /// <param name="transaction">The SQL transaction within to build the temporary tables.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain an array of <see cref="TemporarySqlServerTableDisposer" />
    /// instances that can be used to dispose the built temporary tables.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled via <paramref name="cancellationToken" />.
    /// </exception>
    private static async Task<TemporarySqlServerTableDisposer[]> CreateTemporaryTablesAsync(
        InterpolatedSqlStatement statement,
        SqlConnection connection,
        SqlTransaction? transaction,
        CancellationToken cancellationToken
    )
    {
        var temporaryTableDisposers = new TemporarySqlServerTableDisposer[statement.TemporaryTables.Count];

        try
        {
            for (var i = 0; i < statement.TemporaryTables.Count; i++)
            {
                var interpolatedTemporaryTable = statement.TemporaryTables[i];

                temporaryTableDisposers[i] = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
                    connection,
                    transaction,
                    interpolatedTemporaryTable.Name,
                    interpolatedTemporaryTable.Values,
                    interpolatedTemporaryTable.ValuesType,
                    cancellationToken
                ).ConfigureAwait(false);
            }
        }
        catch (SqlException exception) when (
            SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
        )
        {
            throw new OperationCanceledException(cancellationToken);
        }

        return temporaryTableDisposers;
    }
}
