using SqlCommandBuilder = RentADeveloper.SqlConnectionPlus.SqlCommands.SqlCommandBuilder;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.SqlCommands;

public class SqlCommandBuilderTests : TestsBase
{
    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            SqlCommandBuilder.BuildSqlCommand(new(), "", null, null, CommandType.Text)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            SqlCommandBuilder.BuildSqlCommandAsync(new(), "", null, null, CommandType.Text)
        );
    }
}
