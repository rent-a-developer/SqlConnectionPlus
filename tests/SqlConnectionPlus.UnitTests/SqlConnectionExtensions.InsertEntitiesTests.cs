using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_InsertEntitiesTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var entities = Generate.Entities(5);

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.InsertEntities(new(), entities)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.InsertEntitiesAsync(new(), entities)
        );
    }
}
