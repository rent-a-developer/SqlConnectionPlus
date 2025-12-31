
namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_QueryScalarsTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.QueryScalars<Int32>(new(), "SELECT 1")
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.QueryScalarsAsync<Int32>(new(), "SELECT 1")
        );
    }
}
