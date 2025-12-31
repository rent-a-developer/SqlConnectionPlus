using RentADeveloper.SqlConnectionPlus.SqlCommands;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.SqlCommands;

public class DefaultSqlCommandFactoryTests : DatabaseTestsBase
{
    [Fact]
    public void CreateSqlCommand_NoTimeout_ShouldUseDefaultTimeout()
    {
        var command = this.factory.CreateSqlCommand(this.Connection, "SELECT 1");

        command.CommandTimeout
            .Should().Be(30);
    }

    [Fact]
    public void CreateSqlCommand_ShouldCreateSqlCommandWithSpecifiedSettings()
    {
        var transaction = this.Connection.BeginTransaction();
        var timeout = TimeSpan.FromMilliseconds(123);

        var command = this.factory.CreateSqlCommand(
            this.Connection,
            "SELECT 1",
            transaction,
            timeout,
            CommandType.StoredProcedure
        );

        command.Connection
            .Should().BeSameAs(this.Connection);

        command.CommandText
            .Should().Be("SELECT 1");

        command.Transaction
            .Should().BeSameAs(transaction);

        command.CommandType
            .Should().Be(CommandType.StoredProcedure);

        command.CommandTimeout
            .Should().Be((Int32)timeout.TotalSeconds);
    }

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            this.factory.CreateSqlCommand(this.Connection, "SELECT 1")
        );

    private readonly DefaultSqlCommandFactory factory = new();
}
