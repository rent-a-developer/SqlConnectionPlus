using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;
using SqlCommandBuilder = RentADeveloper.SqlConnectionPlus.SqlCommands.SqlCommandBuilder;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.SqlCommands;

public class SqlCommandDisposerTests : DatabaseTestsBase
{
    [Fact]
    public void Dispose_ShouldDisposeTemporaryTables()
    {
        var entityIds = Generate.EntityIds(5);
        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var (_, commandDisposer) = SqlCommandBuilder.BuildSqlCommand(this.Connection, statement);

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeTrue();

        commandDisposer.Dispose();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeTemporaryTables()
    {
        var entityIds = Generate.EntityIds(5);
        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var (_, commandDisposer) = await SqlCommandBuilder.BuildSqlCommandAsync(this.Connection, statement);

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        await commandDisposer.DisposeAsync();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }
}
