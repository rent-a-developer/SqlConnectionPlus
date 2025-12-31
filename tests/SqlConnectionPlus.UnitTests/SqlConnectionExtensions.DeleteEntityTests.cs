using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_DeleteEntityTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var entity = Generate.Entity();

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.DeleteEntity(new(), entity)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.DeleteEntityAsync(new(), entity)
        );
    }
}
