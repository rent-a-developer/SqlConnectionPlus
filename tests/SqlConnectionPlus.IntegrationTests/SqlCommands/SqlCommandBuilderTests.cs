using RentADeveloper.SqlConnectionPlus.SqlServer;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;
using SqlCommandBuilder = RentADeveloper.SqlConnectionPlus.SqlCommands.SqlCommandBuilder;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.SqlCommands;

public class SqlCommandBuilderTests : DatabaseTestsBase
{
    [Fact]
    public void BuildSqlCommand_ShouldCreateTemporaryTables()
    {
        var entityIds = Generate.EntityIds(5);
        var entities = Generate.Entities(5);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     Value
                                              FROM       {TemporaryTable(entityIds)} AS Ids
                                              INNER JOIN {TemporaryTable(entities)} AS Entities
                                              ON         Entities.Id = Ids.Value
                                              """;
        var (command, _) = SqlCommandBuilder.BuildSqlCommand(this.Connection, statement);

        var table1Name = statement.TemporaryTables[0].Name;
        var table2Name = statement.TemporaryTables[1].Name;

        command.CommandText
            .Should().Be($"""
                          SELECT     Value
                          FROM       {table1Name} AS Ids
                          INNER JOIN {table2Name} AS Entities
                          ON         Entities.Id = Ids.Value
                          """);

        this.ExistsTemporaryTable(table1Name)
            .Should().BeTrue();

        this.ExistsTemporaryTable(table2Name)
            .Should().BeTrue();

        this.Connection.QueryScalars<Int64>($"SELECT Value FROM {table1Name}")
            .Should().BeEquivalentTo(entityIds);

        this.Connection.QueryEntities<Entity>($"SELECT * FROM {table2Name}")
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public void BuildSqlCommand_ShouldReturnDisposerForCommandWhichDisposesTemporaryTables()
    {
        var entityIds = Generate.EntityIds(5);
        var entities = Generate.Entities(5);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     Value
                                              FROM       {TemporaryTable(entityIds)} AS Ids
                                              INNER JOIN {TemporaryTable(entities)} AS Entities
                                              ON         Entities.Id = Ids.Value
                                              """;
        var (_, commandDisposer) = SqlCommandBuilder.BuildSqlCommand(this.Connection, statement);

        var table1Name = statement.TemporaryTables[0].Name;
        var table2Name = statement.TemporaryTables[1].Name;

        this.ExistsTemporaryTable(table1Name)
            .Should().BeTrue();

        this.ExistsTemporaryTable(table2Name)
            .Should().BeTrue();

        commandDisposer.Dispose();

        this.ExistsTemporaryTable(table1Name)
            .Should().BeFalse();

        this.ExistsTemporaryTable(table2Name)
            .Should().BeFalse();
    }

    [Fact]
    public void BuildSqlCommand_ShouldSetCommandTimeout()
    {
        var timeout = TimeSpan.FromMilliseconds(123);

        var (command, _) = SqlCommandBuilder.BuildSqlCommand(
            this.Connection,
            "SELECT 1",
            null,
            timeout
        );

        command.CommandTimeout
            .Should().Be((Int32)timeout.TotalSeconds);
    }

    [Fact]
    public void BuildSqlCommand_ShouldSetCommandType()
    {
        var (command, _) = SqlCommandBuilder.BuildSqlCommand(
            this.Connection,
            "GetEntities",
            commandType: CommandType.StoredProcedure
        );

        command.CommandType
            .Should().Be(CommandType.StoredProcedure);
    }

    [Fact]
    public void BuildSqlCommand_ShouldSetConnection()
    {
        var (command, _) = SqlCommandBuilder.BuildSqlCommand(this.Connection, "SELECT 1");

        command.Connection
            .Should().BeSameAs(this.Connection);
    }

    [Fact]
    public void BuildSqlCommand_ShouldSetParameters()
    {
        var entityId = Generate.EntityId();
        var dateTimeValue = DateTime.UtcNow;
        var stringValue = Generate.String();

        var (command, _) = SqlCommandBuilder.BuildSqlCommand(
            this.Connection,
            $"""
             SELECT *
             FROM   Entity
             WHERE  Id = {Parameter(entityId)} AND
                    DateTimeValue = {Parameter(dateTimeValue)} AND
                    StringValue = {Parameter(stringValue)}
             """
        );

        command.CommandText
            .Should().Be(
                """
                SELECT *
                FROM   Entity
                WHERE  Id = @EntityId AND
                       DateTimeValue = @DateTimeValue AND
                       StringValue = @StringValue
                """
            );

        command.Parameters.Count
            .Should().Be(3);

        command.Parameters["@EntityId"]
            .Value.Should().Be(entityId);

        command.Parameters["@DateTimeValue"]
            .Value.Should().Be(dateTimeValue);

        command.Parameters["@StringValue"]
            .Value.Should().Be(stringValue);
    }

    [Fact]
    public void BuildSqlCommand_ShouldSetTransaction()
    {
        var transaction = this.Connection.BeginTransaction();

        var (command, _) = SqlCommandBuilder.BuildSqlCommand(
            this.Connection,
            "SELECT 1",
            transaction
        );

        command.Transaction
            .Should().BeSameAs(transaction);
    }

    [Fact]
    public void BuildSqlCommand_ShouldUseCancellationToken()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        var (command, _) = SqlCommandBuilder.BuildSqlCommand(
            this.Connection,
            "WAITFOR DELAY '00:00:01';",
            null,
            null,
            CommandType.Text, cancellationToken);

        var exception = Invoking(() => command.ExecuteNonQuery())
            .Should().Throw<SqlException>().Subject.First();

        SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
            .Should().BeTrue();
    }

    [Fact]
    public async Task BuildSqlCommandAsync_ShouldCreateTemporaryTables()
    {
        var entityIds = Generate.EntityIds(5);
        var entities = Generate.Entities(5);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     Value
                                              FROM       {TemporaryTable(entityIds)} AS Ids
                                              INNER JOIN {TemporaryTable(entities)} AS Entities
                                              ON         Entities.Id = Ids.Value
                                              """;
        var (command, _) = await SqlCommandBuilder.BuildSqlCommandAsync(this.Connection, statement);

        var table1Name = statement.TemporaryTables[0].Name;
        var table2Name = statement.TemporaryTables[1].Name;

        command.CommandText
            .Should().Be($"""
                          SELECT     Value
                          FROM       {table1Name} AS Ids
                          INNER JOIN {table2Name} AS Entities
                          ON         Entities.Id = Ids.Value
                          """);

        (await this.ExistsTemporaryTableAsync(table1Name))
            .Should().BeTrue();

        (await this.ExistsTemporaryTableAsync(table2Name))
            .Should().BeTrue();

        (await this.Connection.QueryScalarsAsync<Int64>($"SELECT Value FROM {table1Name}").ToListAsync())
            .Should().BeEquivalentTo(entityIds);

        (await this.Connection.QueryEntitiesAsync<Entity>($"SELECT * FROM {table2Name}").ToListAsync())
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task BuildSqlCommandAsync_ShouldReturnDisposerForCommand()
    {
        var entityIds = Generate.EntityIds(5);
        var entities = Generate.Entities(5);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     Value
                                              FROM       {TemporaryTable(entityIds)} AS Ids
                                              INNER JOIN {TemporaryTable(entities)} AS Entities
                                              ON         Entities.Id = Ids.Value
                                              """;
        var (_, commandDisposer) = await SqlCommandBuilder.BuildSqlCommandAsync(this.Connection, statement);

        var table1Name = statement.TemporaryTables[0].Name;
        var table2Name = statement.TemporaryTables[1].Name;

        (await this.ExistsTemporaryTableAsync(table1Name))
            .Should().BeTrue();

        (await this.ExistsTemporaryTableAsync(table2Name))
            .Should().BeTrue();

        await commandDisposer.DisposeAsync();

        (await this.ExistsTemporaryTableAsync(table1Name))
            .Should().BeFalse();

        (await this.ExistsTemporaryTableAsync(table2Name))
            .Should().BeFalse();
    }

    [Fact]
    public async Task BuildSqlCommandAsync_ShouldSetCommandTimeout()
    {
        var timeout = TimeSpan.FromMilliseconds(123);

        var (command, _) = await SqlCommandBuilder.BuildSqlCommandAsync(
            this.Connection,
            "SELECT 1",
            null,
            timeout
        );

        command.CommandTimeout
            .Should().Be((Int32)timeout.TotalSeconds);
    }

    [Fact]
    public async Task BuildSqlCommandAsync_ShouldSetCommandType()
    {
        var (command, _) = await SqlCommandBuilder.BuildSqlCommandAsync(
            this.Connection,
            "GetEntities",
            commandType: CommandType.StoredProcedure
        );

        command.CommandType
            .Should().Be(CommandType.StoredProcedure);
    }

    [Fact]
    public async Task BuildSqlCommandAsync_ShouldSetConnection()
    {
        var (command, _) = await SqlCommandBuilder.BuildSqlCommandAsync(this.Connection, "SELECT 1");

        command.Connection
            .Should().BeSameAs(this.Connection);
    }

    [Fact]
    public async Task BuildSqlCommandAsync_ShouldSetParameters()
    {
        var entityId = Generate.EntityId();
        var dateTimeValue = DateTime.UtcNow;
        var stringValue = Generate.String();

        var (command, _) = await SqlCommandBuilder.BuildSqlCommandAsync(
            this.Connection,
            $"""
             SELECT *
             FROM   Entity
             WHERE  Id = {Parameter(entityId)} AND
                    DateTimeValue = {Parameter(dateTimeValue)} AND
                    StringValue = {Parameter(stringValue)}
             """
        );

        command.CommandText
            .Should().Be(
                """
                SELECT *
                FROM   Entity
                WHERE  Id = @EntityId AND
                       DateTimeValue = @DateTimeValue AND
                       StringValue = @StringValue
                """
            );

        command.Parameters.Count
            .Should().Be(3);

        command.Parameters["@EntityId"]
            .Value.Should().Be(entityId);

        command.Parameters["@DateTimeValue"]
            .Value.Should().Be(dateTimeValue);

        command.Parameters["@StringValue"]
            .Value.Should().Be(stringValue);
    }

    [Fact]
    public async Task BuildSqlCommandAsync_ShouldSetTransaction()
    {
        var transaction = this.Connection.BeginTransaction();

        var (command, _) = await SqlCommandBuilder.BuildSqlCommandAsync(
            this.Connection,
            "SELECT 1",
            transaction
        );

        command.Transaction
            .Should().BeSameAs(transaction);
    }

    [Fact]
    public async Task BuildSqlCommandAsync_ShouldUseCancellationToken()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        var (command, _) = await SqlCommandBuilder.BuildSqlCommandAsync(
            this.Connection,
            "WAITFOR DELAY '00:00:01';",
            null,
            null,
            CommandType.Text, cancellationToken);

        var exception = (await Invoking(() => command.ExecuteNonQueryAsync(cancellationToken))
            .Should().ThrowAsync<SqlException>()).Subject.First();

        SqlExceptionHelper.WasSqlStatementCancelledByCancellationToken(exception, cancellationToken)
            .Should().BeTrue();
    }
}
