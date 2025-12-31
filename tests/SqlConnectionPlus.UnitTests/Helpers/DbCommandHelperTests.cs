using RentADeveloper.SqlConnectionPlus.Helpers;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Helpers;

public class DbCommandHelperTests : TestsBase
{
    [Fact]
    public void RegisterDbCommandCancellation_CancellationToken_ShouldRegister()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var command = Substitute.For<DbCommand>();

        var registration = DbCommandHelper.RegisterDbCommandCancellation(command, cancellationToken);

        registration
            .Should().NotBe(default(CancellationTokenRegistration));

        registration.Token
            .Should().Be(cancellationToken);
    }

    [Fact]
    public void RegisterDbCommandCancellation_NoneCancellationToken_ShouldNotRegister()
    {
        var command = Substitute.For<DbCommand>();

        DbCommandHelper.RegisterDbCommandCancellation(command, CancellationToken.None)
            .Should().Be(default(CancellationTokenRegistration));
    }

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            DbCommandHelper.RegisterDbCommandCancellation(Substitute.For<DbCommand>(), CancellationToken.None)
        );
}
