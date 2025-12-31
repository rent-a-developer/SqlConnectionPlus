using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_UpdateEntitiesTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var entities = Generate.Entities(5);

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.UpdateEntities(new(), entities)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.UpdateEntitiesAsync(new(), entities)
        );
    }
}
