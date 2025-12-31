
namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class SqlConnectionExtensions_ExecuteXmlReaderTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExecuteXmlReader(new(), "SELECT * FROM Entity FOR XML PATH")
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlConnectionExtensions.ExecuteXmlReaderAsync(new(), "SELECT * FROM Entity FOR XML PATH")
        );
    }
}
