
namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_QueryTuplesTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.QueryTuples<ValueTuple<Int32>>(new(), "SELECT 1")
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.QueryTuplesAsync<ValueTuple<Int32>>(new(), "SELECT 1")
        );
    }
}
