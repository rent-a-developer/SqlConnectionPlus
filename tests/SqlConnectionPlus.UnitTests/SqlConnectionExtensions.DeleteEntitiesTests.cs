using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_DeleteEntitiesTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var entities = Generate.Entities(5);

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.DeleteEntities(new(), entities)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.DeleteEntitiesAsync(new(), entities)
        );
    }
}
