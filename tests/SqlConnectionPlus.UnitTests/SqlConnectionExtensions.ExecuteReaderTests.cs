
namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_ExecuteReaderTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExecuteReader(new(), "SELECT * FROM Entity")
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExecuteReaderAsync(new(), "SELECT * FROM Entity")
        );
    }
}
