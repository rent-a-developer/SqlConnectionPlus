using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_QueryEntitiesTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.QueryEntities<Entity>(new(), "SELECT * FROM Entity")
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.QueryEntitiesAsync<Entity>(new(), "SELECT * FROM Entity")
        );
    }
}
