using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_ExistsTests : DatabaseTestsBase
{
    [Fact]
    public void Exists_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() => this.Connection.Exists("SELECT 1", cancellationToken: cancellationToken))
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public void Exists_CommandType_ShouldUseCommandType()
    {
        this.InsertNewEntities(1);

        this.Connection.Exists(
                "GetFirstEntityId",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeTrue();
    }

    [Fact]
    public void Exists_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     1
                                              FROM       {TemporaryTable(entities)}
                                              WHERE      Id = {entities[0].Id}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        this.Connection.Exists(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeTrue();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void Exists_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(2);

        this.Connection.Exists(
                $"""
                 SELECT     1
                 FROM       {TemporaryTable(entities)}
                 WHERE      Id = {Parameter(entities[0].Id)}
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeTrue();
    }

    [Fact]
    public void Exists_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = this.InsertNewEntity();

        this.Connection.Exists(
                $"SELECT 1 FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeTrue();
    }

    [Fact]
    public void Exists_QueryReturnedAtLeastOneRow_ShouldReturnTrue()
    {
        var entity = this.InsertNewEntity();

        this.Connection.Exists(
                $"SELECT 1 FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeTrue();
    }

    [Fact]
    public void Exists_QueryReturnedNoRows_ShouldReturnFalse() =>
        this.Connection.Exists(
                $"SELECT 1 FROM Entity WHERE Id = -1",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeFalse();

    [Fact]
    public void Exists_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement =
            $"SELECT 1 FROM {TemporaryTable(entityIds)} WHERE Value = {entityIds[0]}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        this.Connection.Exists(statement, cancellationToken: TestContext.Current.CancellationToken)
            .Should().BeTrue();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void Exists_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(2);

        this.Connection.Exists(
                $"SELECT 1 FROM {TemporaryTable(entityIds)} WHERE Value = {Parameter(entityIds[0])}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeTrue();
    }

    [Fact]
    public void Exists_Timeout_ShouldUseTimeout() =>
        Invoking(() => this.Connection.Exists("WAITFOR DELAY '00:00:02';", commandTimeout: TimeSpan.FromSeconds(1)))
            .Should().ThrowTimeoutSqlException();

    [Fact]
    public void Exists_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entity = this.InsertNewEntity(transaction);

            this.Connection.Exists(
                    $"SELECT 1 FROM Entity WHERE Id = {Parameter(entity.Id)}",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                )
                .Should().BeTrue();

            transaction.Rollback();
        }

        this.Connection.Exists(
                $"SELECT 1 FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() => this.Connection.ExistsAsync("SELECT 1", cancellationToken: cancellationToken))
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public async Task ExistsAsync_CommandType_ShouldUseCommandType()
    {
        await this.InsertNewEntitiesAsync(1);

        (await this.Connection.ExistsAsync(
                "GetFirstEntityId",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     1
                                              FROM       {TemporaryTable(entities)}
                                              WHERE      Id = {entities[0].Id}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        (await this.Connection.ExistsAsync(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeTrue();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExistsAsync_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(2);

        (await this.Connection.ExistsAsync(
                $"""
                 SELECT     1
                 FROM       {TemporaryTable(entities)}
                 WHERE      Id = {Parameter(entities[0].Id)}
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = await this.InsertNewEntityAsync();

        (await this.Connection.ExistsAsync(
                $"SELECT 1 FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_QueryReturnedAtLeastOneRow_ShouldReturnTrue()
    {
        var entity = await this.InsertNewEntityAsync();

        (await this.Connection.ExistsAsync(
                $"SELECT 1 FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_QueryReturnedNoRows_ShouldReturnFalse() =>
        (await this.Connection.ExistsAsync(
            "SELECT 1 FROM Entity WHERE Id = -1",
            cancellationToken: TestContext.Current.CancellationToken
        ))
        .Should().BeFalse();

    [Fact]
    public async Task ExistsAsync_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement =
            $"SELECT 1 FROM {TemporaryTable(entityIds)} WHERE Value = {Parameter(entityIds[0])}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        (await this.Connection.ExistsAsync(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeTrue();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(2);

        (await this.Connection.ExistsAsync(
                $"SELECT 1 FROM {TemporaryTable(entityIds)} WHERE Value = {Parameter(entityIds[0])}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeTrue();
    }

    [Fact]
    public Task ExistsAsync_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExistsAsync("WAITFOR DELAY '00:00:02';", commandTimeout: TimeSpan.FromSeconds(1))
            )
            .Should().ThrowTimeoutSqlExceptionAsync();

    [Fact]
    public async Task ExistsAsync_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entity = await this.InsertNewEntityAsync(transaction);

            (await this.Connection.ExistsAsync(
                    $"SELECT 1 FROM Entity WHERE Id = {Parameter(entity.Id)}",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ))
                .Should().BeTrue();

            await transaction.RollbackAsync();
        }

        (await this.Connection.ExistsAsync(
                $"SELECT 1 FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeFalse();
    }
}
