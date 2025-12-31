// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus.SqlCommands;

/// <summary>
/// The default implementation of <see cref="ISqlCommandFactory" />.
/// </summary>
internal sealed class DefaultSqlCommandFactory : ISqlCommandFactory
{
    /// <inheritdoc />
    public SqlCommand CreateSqlCommand(
        SqlConnection connection,
        String commandText,
        SqlTransaction? transaction = null,
        TimeSpan? commandTimeout = null,
        CommandType commandType = CommandType.Text
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(commandText);

        var command = connection.CreateCommand();

#pragma warning disable CA2100
        command.CommandText = commandText;
#pragma warning restore CA2100

        command.Transaction = transaction;
        command.CommandType = commandType;

        if (commandTimeout is not null)
        {
            command.CommandTimeout = (Int32)commandTimeout.Value.TotalSeconds;
        }

        return command;
    }
}
