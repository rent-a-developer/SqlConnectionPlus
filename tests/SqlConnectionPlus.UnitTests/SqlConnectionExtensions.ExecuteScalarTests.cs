
namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_ExecuteScalarTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExecuteScalar<Int32>(new(), "SELECT 1")
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExecuteScalarAsync<Int32>(new(), "SELECT 1")
        );
    }
}
