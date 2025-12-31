
namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_ExistsTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.Exists(new(), "SELECT 1")
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExistsAsync(new(), "SELECT 1")
        );
    }
}
