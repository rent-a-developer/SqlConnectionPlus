using RentADeveloper.SqlConnectionPlus.Helpers;
using RentADeveloper.SqlConnectionPlus.SqlServer;

// ReSharper disable AccessToDisposedClosure

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.SqlServer;

public class SqlExceptionHelperTests : DatabaseTestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        using var command = this.Connection.CreateCommand();
        command.CommandText = "WAITFOR DELAY '00:00:01'";

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        using var registration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        var sqlException = Invoking(() => command.ExecuteNonQuery())
            .Should().Throw<SqlException>().Subject.First();

        ArgumentNullGuardVerifier.Verify(() =>
            SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(sqlException, cancellationToken)
        );
    }

    [Fact]
    public void WasSqlStatementCancelledByCancellationToken_StatementWasCancelled_ShouldReturnTrue()
    {
        using var command = this.Connection.CreateCommand();
        command.CommandText = "WAITFOR DELAY '00:00:01'";

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        using var registration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        var sqlException = Invoking(() => command.ExecuteNonQuery())
            .Should().Throw<SqlException>().Subject.First();

        SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(sqlException, cancellationToken)
            .Should().BeTrue();
    }

    [Fact]
    public void WasSqlStatementCancelledByCancellationToken_StatementWasNotCancelled_ShouldReturnFalse()
    {
        using var command = this.Connection.CreateCommand();
        command.CommandText = "InvalidStatement";

        var sqlException = Invoking(() => command.ExecuteNonQuery())
            .Should().Throw<SqlException>().Subject.First();

        SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(sqlException, CancellationToken.None)
            .Should().BeFalse();
    }
}
