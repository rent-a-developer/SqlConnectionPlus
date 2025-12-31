using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_UpdateEntityTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var entity = Generate.Entity();

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.UpdateEntity(new(), entity)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.UpdateEntityAsync(new(), entity)
        );
    }
}
