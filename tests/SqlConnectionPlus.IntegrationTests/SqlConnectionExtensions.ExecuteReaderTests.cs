using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_ExecuteReaderTests : DatabaseTestsBase
{
    [Fact]
    public void ExecuteReader_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() =>
                this.Connection.ExecuteReader("SELECT * FROM Entity", cancellationToken: cancellationToken)
            )
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public void ExecuteReader_CommandBehavior_ShouldPassUseCommandBehavior()
    {
        var reader = this.Connection.ExecuteReader(
            $"SELECT * FROM Entity",
            commandBehavior: CommandBehavior.CloseConnection,
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.Dispose();

        this.Connection.State
            .Should().Be(ConnectionState.Closed);
    }

    [Fact]
    public void ExecuteReader_CommandType_ShouldUseCommandType()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        using var reader = this.Connection.ExecuteReader(
            $"GetEntityIdsAndStringValues",
            commandType: CommandType.StoredProcedure,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entities)
        {
            reader.Read()
                .Should().BeTrue();

            reader.GetInt64(0)
                .Should().Be(entity.Id);

            reader.GetString(1)
                .Should().Be(entity.StringValue);
        }
    }

    [Fact]
    public void ExecuteReader_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterDisposalOfDataReader()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     Id
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var reader = this.Connection.ExecuteReader(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeTrue();

        reader.Dispose();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteReader_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        using var reader = this.Connection.ExecuteReader(
            $"""
             SELECT     Id, StringValue, DecimalValue
             FROM       {TemporaryTable(entities)}
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entities)
        {
            reader.Read()
                .Should().BeTrue();

            reader.GetInt64(0)
                .Should().Be(entity.Id);

            reader.GetString(1)
                .Should().Be(entity.StringValue);

            reader.GetDecimal(2)
                .Should().Be(entity.DecimalValue);
        }
    }

    [Fact]
    public void ExecuteReader_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = this.InsertNewEntity();

        using var reader = this.Connection.ExecuteReader(
            $"SELECT StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.Read()
            .Should().BeTrue();

        reader.GetString(0)
            .Should().Be(entity.StringValue);
    }

    [Fact]
    public void ExecuteReader_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterDisposalOfDataReader()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var reader = this.Connection.ExecuteReader(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeTrue();

        reader.Dispose();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteReader_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        using var reader = this.Connection.ExecuteReader(
            $"SELECT Value FROM {TemporaryTable(entityIds)}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entityId in entityIds)
        {
            reader.Read()
                .Should().BeTrue();

            reader.GetInt64(0)
                .Should().Be(entityId);
        }
    }

    [Fact]
    public void ExecuteReader_ShouldReturnDataReaderForQueryResult()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        using var reader = this.Connection.ExecuteReader(
            $"SELECT Id, StringValue FROM Entity",
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entities)
        {
            reader.Read()
                .Should().BeTrue();

            reader.GetInt64(0)
                .Should().Be(entity.Id);

            reader.GetString(1)
                .Should().Be(entity.StringValue);
        }

        reader.Read()
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteReader_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExecuteReader("WAITFOR DELAY '00:00:02';", commandTimeout: TimeSpan.FromSeconds(1))
            )
            .Should().ThrowTimeoutSqlException();

    [Fact]
    public void ExecuteReader_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = this.InsertNewEntities(Generate.SmallNumber(), transaction);

            var reader = this.Connection.ExecuteReader(
                "SELECT Id, StringValue FROM Entity",
                transaction: transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            reader.HasRows
                .Should().BeTrue();

            foreach (var entity in entities)
            {
                reader.Read()
                    .Should().BeTrue();

                reader.GetInt64(0)
                    .Should().Be(entity.Id);

                reader.GetString(1)
                    .Should().Be(entity.StringValue);
            }

            transaction.Rollback();
        }

        this.Connection.ExecuteReader(
                "SELECT Id, StringValue FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            ).HasRows
            .Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteReaderAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.ExecuteReaderAsync(
                    "SELECT * FROM Entity",
                    cancellationToken: cancellationToken
                )
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public async Task ExecuteReaderAsync_CommandBehavior_ShouldUseCommandBehavior()
    {
        var reader = await this.Connection.ExecuteReaderAsync(
            $"SELECT * FROM Entity",
            commandBehavior: CommandBehavior.CloseConnection,
            cancellationToken: TestContext.Current.CancellationToken
        );

        await reader.DisposeAsync();

        this.Connection.State
            .Should().Be(ConnectionState.Closed);
    }

    [Fact]
    public async Task ExecuteReaderAsync_CommandType_ShouldUseCommandType()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        using var reader = await this.Connection.ExecuteReaderAsync(
            "GetEntityIdsAndStringValues",
            commandType: CommandType.StoredProcedure, cancellationToken: TestContext.Current.CancellationToken);

        foreach (var entity in entities)
        {
            (await reader.ReadAsync())
                .Should().BeTrue();

            reader.GetInt64(0)
                .Should().Be(entity.Id);

            reader.GetString(1)
                .Should().Be(entity.StringValue);
        }
    }

    [Fact]
    public async Task
        ExecuteReaderAsync_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterDisposalOfDataReader()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     Id
                                              FROM       {TemporaryTable(entities)}
                                              """;
        var temporaryTableName = statement.TemporaryTables[0].Name;

        var reader = await this.Connection.ExecuteReaderAsync(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        await reader.DisposeAsync();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExecuteReaderAsync_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        using var reader = await this.Connection.ExecuteReaderAsync(
            $"""
             SELECT     Id, StringValue, DecimalValue
             FROM       {TemporaryTable(entities)}
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entities)
        {
            (await reader.ReadAsync())
                .Should().BeTrue();

            reader.GetInt64(0)
                .Should().Be(entity.Id);

            reader.GetString(1)
                .Should().Be(entity.StringValue);

            reader.GetDecimal(2)
                .Should().Be(entity.DecimalValue);
        }
    }

    [Fact]
    public async Task ExecuteReaderAsync_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = await this.InsertNewEntityAsync();

        using var reader = await this.Connection.ExecuteReaderAsync(
            $"SELECT StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await reader.ReadAsync(TestContext.Current.CancellationToken))
            .Should().BeTrue();

        reader.GetString(0)
            .Should().Be(entity.StringValue);
    }

    [Fact]
    public async Task ExecuteReaderAsync_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterDisposalOfDataReader()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var reader = await this.Connection.ExecuteReaderAsync(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        await reader.DisposeAsync();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExecuteReaderAsync_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        using var reader = await this.Connection.ExecuteReaderAsync(
            $"SELECT Value FROM {TemporaryTable(entityIds)}",
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entityId in entityIds)
        {
            (await reader.ReadAsync(TestContext.Current.CancellationToken))
                .Should().BeTrue();

            reader.GetInt64(0)
                .Should().Be(entityId);
        }
    }

    [Fact]
    public async Task ExecuteReaderAsync_ShouldReturnDataReaderForQueryResult()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        using var reader = await this.Connection.ExecuteReaderAsync(
            $"SELECT Id, StringValue FROM Entity",
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entities)
        {
            (await reader.ReadAsync())
                .Should().BeTrue();

            reader.GetInt64(0)
                .Should().Be(entity.Id);

            reader.GetString(1)
                .Should().Be(entity.StringValue);
        }

        (await reader.ReadAsync())
            .Should().BeFalse();
    }

    [Fact]
    public Task ExecuteReaderAsync_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExecuteReaderAsync("WAITFOR DELAY '00:00:02';", commandTimeout: TimeSpan.FromSeconds(1))
            )
            .Should().ThrowTimeoutSqlExceptionAsync();

    [Fact]
    public async Task ExecuteReaderAsync_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber(), transaction);

            var reader = await this.Connection.ExecuteReaderAsync(
                "SELECT Id, StringValue FROM Entity",
                transaction: transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            reader.HasRows
                .Should().BeTrue();

            foreach (var entity in entities)
            {
                (await reader.ReadAsync())
                    .Should().BeTrue();

                reader.GetInt64(0)
                    .Should().Be(entity.Id);

                reader.GetString(1)
                    .Should().Be(entity.StringValue);
            }

            await transaction.RollbackAsync();
        }

        (await this.Connection.ExecuteReaderAsync(
                "SELECT Id, StringValue FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            )).HasRows
            .Should().BeFalse();
    }
}
