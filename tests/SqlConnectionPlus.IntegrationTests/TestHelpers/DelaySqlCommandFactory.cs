using RentADeveloper.SqlConnectionPlus.SqlCommands;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;

/// <summary>
/// An implementation of <see cref="ISqlCommandFactory" /> that supports delaying created commands.
/// </summary>
public class DelaySqlCommandFactory : ISqlCommandFactory
{
    /// <summary>
    /// Determines whether the next SQL command created throw this instance will be delayed by 1 second.
    /// If set to <see langword="true" />, a 1 second delay will be injected into the next SQL command created by this
    /// factory. Subsequent commands will not be delayed unless this property is set to <see langword="true" /> again.
    /// </summary>
    public Boolean DelayNextSqlCommand { get; set; }

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
        if (this.DelayNextSqlCommand)
        {
            command.CommandText = "WAITFOR DELAY '00:00:01'; " + commandText;
            this.DelayNextSqlCommand = false;
        }
        else
        {
            command.CommandText = commandText;
        }
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
