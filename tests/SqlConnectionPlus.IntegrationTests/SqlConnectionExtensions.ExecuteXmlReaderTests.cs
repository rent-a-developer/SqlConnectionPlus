using System.Xml.Linq;
using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_ExecuteXmlReaderTests : DatabaseTestsBase
{
    [Fact]
    public void ExecuteXmlReader_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() =>
                this.Connection.ExecuteXmlReader(
                    "SELECT * FROM Entity FOR XML AUTO",
                    cancellationToken: cancellationToken
                )
            )
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public void ExecuteXmlReader_CommandType_ShouldUseCommandType()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        using var reader = this.Connection.ExecuteXmlReader(
            "GetEntityIdsAndStringValuesAsXml",
            cancellationToken: TestContext.Current.CancellationToken,
            commandType: CommandType.StoredProcedure
        );

        reader.Read();

        foreach (var entity in entities)
        {
            XNode.ReadFrom(reader).ToString()
                .Should().Be(
                    $"""
                     <row>
                       <Id>{entity.Id}</Id>
                       <StringValue>{entity.StringValue}</StringValue>
                     </row>
                     """);
        }
    }

    [Fact]
    public void ExecuteXmlReader_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterDisposalOfDataReader()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT    Id
                                              FROM      {TemporaryTable(entities)}
                                              FOR       XML PATH
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var reader = this.Connection.ExecuteXmlReader(
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
    public void
        ExecuteXmlReader_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        using var reader = this.Connection.ExecuteXmlReader(
            $"""
             SELECT     Id, Int32Value, StringValue
             FROM       {TemporaryTable(entities)}
             FOR        XML PATH
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.Read();

        foreach (var entity in entities)
        {
            XNode.ReadFrom(reader).ToString()
                .Should().Be(
                    $"""
                     <row>
                       <Id>{entity.Id}</Id>
                       <Int32Value>{entity.Int32Value}</Int32Value>
                       <StringValue>{entity.StringValue}</StringValue>
                     </row>
                     """);
        }
    }

    [Fact]
    public void ExecuteXmlReader_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = this.InsertNewEntity();

        using var reader = this.Connection.ExecuteXmlReader(
            $"""
             SELECT     Id, Int32Value, StringValue
             FROM       Entity
             WHERE      Id = {Parameter(entity.Id)}
             FOR        XML PATH
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.Read();

        XNode.ReadFrom(reader).ToString()
            .Should().Be(
                $"""
                 <row>
                   <Id>{entity.Id}</Id>
                   <Int32Value>{entity.Int32Value}</Int32Value>
                   <StringValue>{entity.StringValue}</StringValue>
                 </row>
                 """);
    }

    [Fact]
    public void ExecuteXmlReader_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterDisposalOfDataReader()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)} FOR XML PATH";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var reader = this.Connection.ExecuteXmlReader(
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
    public void ExecuteXmlReader_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        using var reader = this.Connection.ExecuteXmlReader(
            $"SELECT Value FROM {TemporaryTable(entityIds)} FOR XML PATH",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.Read();

        foreach (var entityId in entityIds)
        {
            XNode.ReadFrom(reader).ToString()
                .Should().Be(
                    $"""
                     <row>
                       <Value>{entityId}</Value>
                     </row>
                     """);
        }
    }

    [Fact]
    public void ExecuteXmlReader_ShouldReturnXmlReaderForResult()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        using var reader = this.Connection.ExecuteXmlReader(
            $"""
             SELECT     Id, Int32Value, StringValue
             FROM       Entity
             FOR        XML PATH
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.Read()
            .Should().BeTrue();

        foreach (var entity in entities)
        {
            XNode.ReadFrom(reader).ToString()
                .Should().Be(
                    $"""
                     <row>
                       <Id>{entity.Id}</Id>
                       <Int32Value>{entity.Int32Value}</Int32Value>
                       <StringValue>{entity.StringValue}</StringValue>
                     </row>
                     """);
        }
    }

    [Fact]
    public void ExecuteXmlReader_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExecuteXmlReader("WAITFOR DELAY '00:00:02';", commandTimeout: TimeSpan.FromSeconds(1))
            )
            .Should().ThrowTimeoutSqlException();

    [Fact]
    public void ExecuteXmlReader_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = this.InsertNewEntities(Generate.SmallNumber(), transaction);

            var reader = this.Connection.ExecuteXmlReader(
                "SELECT Id, Int32Value, StringValue FROM Entity FOR XML PATH",
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            reader.Read()
                .Should().BeTrue();

            foreach (var entity in entities)
            {
                XNode.ReadFrom(reader).ToString()
                    .Should().Be(
                        $"""
                         <row>
                           <Id>{entity.Id}</Id>
                           <Int32Value>{entity.Int32Value}</Int32Value>
                           <StringValue>{entity.StringValue}</StringValue>
                         </row>
                         """);
            }

            transaction.Rollback();
        }

        this.Connection.ExecuteXmlReader(
                "SELECT * FROM Entity FOR XML PATH",
                cancellationToken: TestContext.Current.CancellationToken
            ).Read()
            .Should().BeFalse();
    }

    [Fact]
    public Task ExecuteXmlReaderAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        return Invoking(() =>
                this.Connection.ExecuteXmlReaderAsync(
                    "SELECT * FROM Entity FOR XML AUTO",
                    cancellationToken: cancellationToken
                )
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public async Task ExecuteXmlReaderAsync_CommandType_ShouldUseCommandType()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        using var reader = await this.Connection.ExecuteXmlReaderAsync(
            "GetEntityIdsAndStringValuesAsXml",
            cancellationToken: TestContext.Current.CancellationToken,
            commandType: CommandType.StoredProcedure
        );

        await reader.ReadAsync();

        foreach (var entity in entities)
        {
            XNode.ReadFrom(reader).ToString()
                .Should().Be(
                    $"""
                     <row>
                       <Id>{entity.Id}</Id>
                       <StringValue>{entity.StringValue}</StringValue>
                     </row>
                     """);
        }
    }

    [Fact]
    public async Task
        ExecuteXmlReaderAsync_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterDisposalOfDataReader()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT    Id
                                              FROM      {TemporaryTable(entities)}
                                              FOR       XML PATH
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var reader = await this.Connection.ExecuteXmlReaderAsync(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        reader.Dispose();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExecuteXmlReaderAsync_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        using var reader = await this.Connection.ExecuteXmlReaderAsync(
            $"""
             SELECT     Id, Int32Value, StringValue
             FROM       {TemporaryTable(entities)}
             FOR        XML PATH
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        await reader.ReadAsync();

        foreach (var entity in entities)
        {
            XNode.ReadFrom(reader).ToString()
                .Should().Be(
                    $"""
                     <row>
                       <Id>{entity.Id}</Id>
                       <Int32Value>{entity.Int32Value}</Int32Value>
                       <StringValue>{entity.StringValue}</StringValue>
                     </row>
                     """);
        }
    }

    [Fact]
    public async Task ExecuteXmlReaderAsync_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = await this.InsertNewEntityAsync();

        using var reader = await this.Connection.ExecuteXmlReaderAsync(
            $"""
             SELECT     Id, Int32Value, StringValue
             FROM       Entity
             WHERE      Id = {Parameter(entity.Id)}
             FOR        XML PATH
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        await reader.ReadAsync();

        XNode.ReadFrom(reader).ToString()
            .Should().Be(
                $"""
                 <row>
                   <Id>{entity.Id}</Id>
                   <Int32Value>{entity.Int32Value}</Int32Value>
                   <StringValue>{entity.StringValue}</StringValue>
                 </row>
                 """);
    }

    [Fact]
    public async Task
        ExecuteXmlReaderAsync_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterDisposalOfDataReader()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)} FOR XML PATH";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var reader = await this.Connection.ExecuteXmlReaderAsync(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        reader.Dispose();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExecuteXmlReaderAsync_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        using var reader = await this.Connection.ExecuteXmlReaderAsync(
            $"SELECT Value FROM {TemporaryTable(entityIds)} FOR XML PATH",
            cancellationToken: TestContext.Current.CancellationToken
        );

        await reader.ReadAsync();

        foreach (var entityId in entityIds)
        {
            XNode.ReadFrom(reader).ToString()
                .Should().Be(
                    $"""
                     <row>
                       <Value>{entityId}</Value>
                     </row>
                     """);
        }
    }

    [Fact]
    public async Task ExecuteXmlReaderAsync_ShouldReturnXmlReaderForResult()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        using var reader = await this.Connection.ExecuteXmlReaderAsync(
            $"""
             SELECT     Id, Int32Value, StringValue
             FROM       Entity
             FOR        XML PATH
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await reader.ReadAsync())
            .Should().BeTrue();

        foreach (var entity in entities)
        {
            XNode.ReadFrom(reader).ToString()
                .Should().Be(
                    $"""
                     <row>
                       <Id>{entity.Id}</Id>
                       <Int32Value>{entity.Int32Value}</Int32Value>
                       <StringValue>{entity.StringValue}</StringValue>
                     </row>
                     """);
        }
    }

    [Fact]
    public Task ExecuteXmlReaderAsync_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExecuteXmlReaderAsync(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                )
            )
            .Should().ThrowTimeoutSqlExceptionAsync();

    [Fact]
    public async Task ExecuteXmlReaderAsync_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber(), transaction);

            var reader = await this.Connection.ExecuteXmlReaderAsync(
                "SELECT Id, Int32Value, StringValue FROM Entity FOR XML PATH",
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            (await reader.ReadAsync())
                .Should().BeTrue();

            foreach (var entity in entities)
            {
                XNode.ReadFrom(reader).ToString()
                    .Should().Be(
                        $"""
                         <row>
                           <Id>{entity.Id}</Id>
                           <Int32Value>{entity.Int32Value}</Int32Value>
                           <StringValue>{entity.StringValue}</StringValue>
                         </row>
                         """);
            }

            await transaction.RollbackAsync();
        }

        (await (await this.Connection.ExecuteXmlReaderAsync(
                "SELECT * FROM Entity FOR XML PATH",
                cancellationToken: TestContext.Current.CancellationToken
            )).ReadAsync())
            .Should().BeFalse();
    }
}
