
namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_ExecuteNonQueryTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExecuteNonQuery(new(), "DELETE FROM Entity")
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExecuteNonQueryAsync(new(), "DELETE FROM Entity")
        );
    }
}
