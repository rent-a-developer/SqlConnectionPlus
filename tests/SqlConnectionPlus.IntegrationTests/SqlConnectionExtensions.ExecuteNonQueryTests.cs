using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.SqlStatements;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_ExecuteNonQueryTests : DatabaseTestsBase
{
    [Fact]
    public void ExecuteNonQuery_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entity = this.InsertNewEntity();

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() =>
                this.Connection.ExecuteNonQuery(
                    "DELETE FROM Entity",
                    cancellationToken: cancellationToken
                )
            )
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entity should still exist.
        this.ExistsEntityById(entity.Id)
            .Should().BeTrue();
    }

    [Fact]
    public void ExecuteNonQuery_CommandType_ShouldPassUseCommandType()
    {
        var entity = this.InsertNewEntity();

        this.Connection.ExecuteNonQuery(
            "DeleteAllEntities",
            commandType: CommandType.StoredProcedure,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.ExistsEntityById(entity.Id)
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteNonQuery_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entities = this.InsertNewEntities(5);
        var entitiesToDelete = entities.Take(2).ToList();

        InterpolatedSqlStatement statement = $"""
                                              DELETE     Entity
                                              FROM       Entity
                                              INNER JOIN {TemporaryTable(entitiesToDelete)} AS TEntitiesToDelete
                                              ON         Entity.Id = TEntitiesToDelete.Id AND
                                                         Entity.StringValue = TEntitiesToDelete.StringValue AND
                                                         Entity.Int32Value = TEntitiesToDelete.Int32Value
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        this.Connection.ExecuteNonQuery(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entitiesToDelete.Count);

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteNonQuery_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = this.InsertNewEntities(5);
        var entitiesToDelete = entities.Take(2).ToList();

        this.Connection.ExecuteNonQuery(
                $"""
                 DELETE     Entity
                 FROM       Entity
                 INNER JOIN {TemporaryTable(entitiesToDelete)} AS TEntitiesToDelete
                 ON         Entity.Id = TEntitiesToDelete.Id AND
                            Entity.StringValue = TEntitiesToDelete.StringValue AND
                            Entity.Int32Value = TEntitiesToDelete.Int32Value
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entitiesToDelete.Count);

        foreach (var entity in entitiesToDelete)
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeFalse();
        }

        foreach (var entity in entities.Except(entitiesToDelete))
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void ExecuteNonQuery_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = this.InsertNewEntity();

        this.Connection.ExecuteNonQuery(
            $"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.ExistsEntityById(entity.Id)
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteNonQuery_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entities = this.InsertNewEntities(5);
        var idsOfEntitiesToDelete = entities.Select(a => a.Id).Take(2).ToList();

        InterpolatedSqlStatement statement =
            $"""
             DELETE FROM    Entity
             WHERE          Id IN (SELECT Value FROM {TemporaryTable(idsOfEntitiesToDelete)})
             """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        this.Connection.ExecuteNonQuery(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(idsOfEntitiesToDelete.Count);

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteNonQuery_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entities = this.InsertNewEntities(5);
        var entitiesToDelete = entities.Take(2).ToList();
        var idsOfEntitiesToDelete = entitiesToDelete.Select(a => a.Id).ToList();

        this.Connection.ExecuteNonQuery(
            $"""
             DELETE FROM    Entity
             WHERE          Id IN (SELECT Value FROM {TemporaryTable(idsOfEntitiesToDelete)})
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entitiesToDelete)
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeFalse();
        }

        foreach (var entity in entities.Except(entitiesToDelete))
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void ExecuteNonQuery_ShouldReturnNumberOfAffectedRows()
    {
        var entity = this.InsertNewEntity();

        this.Connection.ExecuteNonQuery(
                $"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(1);

        this.Connection.ExecuteNonQuery(
                $"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(0);
    }

    [Fact]
    public void ExecuteNonQuery_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExecuteNonQuery("WAITFOR DELAY '00:00:02';", commandTimeout: TimeSpan.FromSeconds(1))
            )
            .Should().ThrowTimeoutSqlException();

    [Fact]
    public void ExecuteNonQuery_Transaction_ShouldUseTransaction()
    {
        var entity = this.InsertNewEntity();

        using (var transaction = this.Connection.BeginTransaction())
        {
            this.Connection.ExecuteNonQuery(
                $"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}",
                transaction: transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            this.ExistsEntityById(entity.Id, transaction)
                .Should().BeFalse();

            transaction.Rollback();
        }

        this.ExistsEntityById(entity.Id)
            .Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entity = await this.InsertNewEntityAsync();

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.ExecuteNonQueryAsync(
                    "DELETE FROM Entity",
                    cancellationToken: cancellationToken
                )
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entity should still exist.
        (await this.ExistsEntityByIdAsync(entity.Id))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_CommandType_ShouldPassUseCommandType()
    {
        var entity = await this.InsertNewEntityAsync();

        await this.Connection.ExecuteNonQueryAsync(
            "DeleteAllEntities",
            commandType: CommandType.StoredProcedure,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsEntityByIdAsync(entity.Id))
            .Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entities = await this.InsertNewEntitiesAsync(5);
        var entitiesToDelete = entities.Take(2).ToList();

        InterpolatedSqlStatement statement = $"""
                                              DELETE     Entity
                                              FROM       Entity
                                              INNER JOIN {TemporaryTable(entitiesToDelete)} AS TEntitiesToDelete
                                              ON         Entity.Id = TEntitiesToDelete.Id AND
                                                         Entity.StringValue = TEntitiesToDelete.StringValue AND
                                                         Entity.Int32Value = TEntitiesToDelete.Int32Value
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        (await this.Connection.ExecuteNonQueryAsync(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entitiesToDelete.Count);

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExecuteNonQueryAsync_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = await this.InsertNewEntitiesAsync(5);
        var entitiesToDelete = entities.Take(2).ToList();

        await this.Connection.ExecuteNonQueryAsync(
            $"""
             DELETE     Entity
             FROM       Entity
             INNER JOIN {TemporaryTable(entitiesToDelete)} AS TEntitiesToDelete
             ON         Entity.Id = TEntitiesToDelete.Id AND
                        Entity.StringValue = TEntitiesToDelete.StringValue AND
                        Entity.Int32Value = TEntitiesToDelete.Int32Value
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entitiesToDelete)
        {
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeFalse();
        }

        foreach (var entity in entities.Except(entitiesToDelete))
        {
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = await this.InsertNewEntityAsync();

        await this.Connection.ExecuteNonQueryAsync(
            $"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsEntityByIdAsync(entity.Id))
            .Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entities = await this.InsertNewEntitiesAsync(5);
        var idsOfEntitiesToDelete = entities.Select(a => a.Id).Take(2).ToList();

        InterpolatedSqlStatement statement =
            $"""
             DELETE FROM    Entity
             WHERE          Id IN (SELECT Value FROM {TemporaryTable(idsOfEntitiesToDelete)})
             """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        (await this.Connection.ExecuteNonQueryAsync(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(idsOfEntitiesToDelete.Count);

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExecuteNonQueryAsync_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entities = await this.InsertNewEntitiesAsync(5);
        var entitiesToDelete = entities.Take(2).ToList();
        var idsOfEntitiesToDelete = entitiesToDelete.Select(a => a.Id).ToList();

        await this.Connection.ExecuteNonQueryAsync(
            $"""
             DELETE FROM    Entity
             WHERE          Id IN (SELECT Value FROM {TemporaryTable(idsOfEntitiesToDelete)})
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entitiesToDelete)
        {
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeFalse();
        }

        foreach (var entity in entities.Except(entitiesToDelete))
        {
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_ShouldReturnNumberOfAffectedRows()
    {
        var entity = await this.InsertNewEntityAsync();

        (await this.Connection.ExecuteNonQueryAsync(
                $"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(1);

        (await this.Connection.ExecuteNonQueryAsync(
                $"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(0);
    }

    [Fact]
    public Task ExecuteNonQueryAsync_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExecuteNonQueryAsync(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                )
            )
            .Should().ThrowTimeoutSqlExceptionAsync();

    [Fact]
    public async Task ExecuteNonQueryAsync_Transaction_ShouldUseTransaction()
    {
        var entity = await this.InsertNewEntityAsync();

        using (var transaction = this.Connection.BeginTransaction())
        {
            await this.Connection.ExecuteNonQueryAsync(
                $"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}",
                transaction: transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            (await this.ExistsEntityByIdAsync(entity.Id, transaction))
                .Should().BeFalse();

            await transaction.RollbackAsync();
        }

        (await this.ExistsEntityByIdAsync(entity.Id))
            .Should().BeTrue();
    }
}
