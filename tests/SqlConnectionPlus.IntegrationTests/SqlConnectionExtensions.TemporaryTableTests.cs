using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_TemporaryTableTests : DatabaseTestsBase
{
    [Fact]
    public void TemporaryTable_ComplexObjects_EmptyCollection_ShouldReturnNoRows()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<Entity> entities = [];

        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM {TemporaryTable(entities)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEmpty();
    }

    [Fact]
    public void TemporaryTable_ComplexObjects_ShouldBePassedAsMultiColumnTemporaryTableToSqlStatement()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM {TemporaryTable(entities)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public void TemporaryTable_ScalarValues_EmptyCollection_ShouldReturnNoRows()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<Int32> values = [];

        this.Connection
            .QueryScalars<Int32>(
                $"SELECT Value FROM {TemporaryTable(values)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEmpty();
    }

    [Fact]
    public void TemporaryTable_ScalarValues_ShouldBePassedAsSingleColumnTemporaryTableToSqlStatement()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        this.Connection
            .QueryScalars<Int32>(
                $"SELECT Value FROM {TemporaryTable(entityIds)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entityIds);
    }
}
