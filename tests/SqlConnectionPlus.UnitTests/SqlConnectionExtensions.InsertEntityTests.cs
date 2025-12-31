using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_InsertEntityTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var entity = Generate.Entity();

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.InsertEntity(new(), entity)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.InsertEntityAsync(new(), entity)
        );
    }
}
