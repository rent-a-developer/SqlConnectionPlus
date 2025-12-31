using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_QueryScalarsTests : DatabaseTestsBase
{
    [Fact]
    public void QueryScalars_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() =>
                this.Connection.QueryScalars<Int32>(
                    "SELECT 1",
                    cancellationToken: cancellationToken
                ).ToList()
            )
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public void QueryScalars_CharTargetType_ColumnContainsStringWithLengthNotOne_ShouldThrow()
    {
        Invoking(() =>
                this.Connection.QueryScalars<Char>(
                    "SELECT ''",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains the value '' ({typeof(String)}), which " +
                $"could not be converted to the type {typeof(Char)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string '' to the target type {typeof(Char)}. The string must be exactly one " +
                $"character long."
            );

        Invoking(() =>
                this.Connection.QueryScalars<Char>(
                    "SELECT 'ab'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains the value 'ab' ({typeof(String)}), which " +
                $"could not be converted to the type {typeof(Char)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'ab' to the target type {typeof(Char)}. The string must be exactly " +
                $"one character long."
            );
    }

    [Fact]
    public void QueryScalars_CharTargetType_ColumnContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        this.Connection.QueryScalars<Char>(
                $"SELECT '{character}'",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([character]);
    }

    [Fact]
    public void QueryScalars_ColumnValueCannotBeConvertedToTargetType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryScalars<Int32>(
                    "SELECT 'A'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains the value 'A' ({typeof(String)}), which " +
                $"could not be converted to the type {typeof(Int32)}. See inner exception for details.*"
            );

    [Fact]
    public void QueryScalars_CommandType_ShouldUseCommandType()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        this.Connection.QueryScalars<Int64>(
                "GetEntityIds",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities.Select(a => a.Id));
    }

    [Fact]
    public void QueryScalars_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     StringValue
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var enumerator = this.Connection.QueryScalars<String>(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        ).GetEnumerator();

        enumerator.MoveNext()
            .Should().BeTrue();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeTrue();

        enumerator.MoveNext()
            .Should().BeTrue();

        enumerator.MoveNext()
            .Should().BeFalse();

        enumerator.Dispose();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void QueryScalars_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        this.Connection.QueryScalars<String>(
                $"""
                 SELECT     StringValue
                 FROM       {TemporaryTable(entities)}
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities.Select(a => a.StringValue));
    }

    [Fact]
    public void QueryScalars_EnumTargetType_ColumnContainsInvalidInteger_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryScalars<TestEnum>(
                    "SELECT 999",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains the value '999' ({typeof(Int32)}), which " +
                $"could not be converted to the type {typeof(TestEnum)}. See inner exception for details.*"
            );

    [Fact]
    public void QueryScalars_EnumTargetType_ColumnContainsInvalidString_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryScalars<TestEnum>(
                    "SELECT 'DoesNotExist'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains the value 'DoesNotExist' " +
                $"({typeof(String)}), which could not be converted to the type {typeof(TestEnum)}. See inner " +
                $"exception for details.*"
            );

    [Fact]
    public void QueryScalars_EnumTargetType_ShouldConvertIntegerToEnum()
    {
        var enumValue = Generate.Enum();

        this.Connection.QueryScalars<TestEnum>(
                $"SELECT {(Int32)enumValue}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([enumValue]);
    }

    [Fact]
    public void QueryScalars_EnumTargetType_ShouldConvertStringToEnum()
    {
        var enumValue = Generate.Enum();

        this.Connection.QueryScalars<TestEnum>(
                $"SELECT '{enumValue.ToString()}'",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([enumValue]);
    }

    [Fact]
    public void QueryScalars_NonNullableTargetType_ColumnContainsNull_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryScalars<Int32>(
                    "SELECT NULL",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains NULL, which could not be converted to the " +
                $"type {typeof(Int32)}. See inner exception for details.*"
            );

    [Fact]
    public void QueryScalars_NullableTargetType_Column_ShouldReturnNull() =>
        this.Connection.QueryScalars<Int32?>(
                "SELECT NULL",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(new Int32?[] { null });

    [Fact]
    public void QueryScalars_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = this.InsertNewEntity();

        this.Connection.QueryScalars<String>(
                $"SELECT StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entity.StringValue);
    }

    [Fact]
    public void QueryScalars_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var enumerator = this.Connection.QueryScalars<Int64>(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        ).GetEnumerator();

        enumerator.MoveNext()
            .Should().BeTrue();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeTrue();

        enumerator.MoveNext()
            .Should().BeTrue();

        enumerator.MoveNext()
            .Should().BeFalse();

        enumerator.Dispose();

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void QueryScalars_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        this.Connection.QueryScalars<Int64>(
                $"SELECT Value FROM {TemporaryTable(entityIds)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entityIds);
    }

    [Fact]
    public void QueryScalars_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.QueryScalars<Int32>(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                ).ToList()
            )
            .Should().ThrowTimeoutSqlException();

    [Fact]
    public void QueryScalars_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = this.InsertNewEntities(Generate.SmallNumber(), transaction);

            this.Connection.QueryScalars<String>(
                    "SELECT StringValue FROM Entity",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                )
                .Should().BeEquivalentTo(entities.Select(a => a.StringValue));

            transaction.Rollback();
        }

        this.Connection.QueryScalars<String>(
                "SELECT StringValue FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEmpty();
    }

    [Fact]
    public async Task QueryScalarsAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.QueryScalarsAsync<Int32>(
                    "SELECT 1",
                    cancellationToken: cancellationToken
                ).ToListAsync(cancellationToken).AsTask()
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public async Task QueryScalarsAsync_CharTargetType_ColumnContainsStringWithLengthNotOne_ShouldThrow()
    {
        (await Invoking(() =>
                    this.Connection.QueryScalarsAsync<Char>(
                        "SELECT ''",
                        cancellationToken: TestContext.Current.CancellationToken
                    ).ToListAsync().AsTask()
                )
                .Should().ThrowAsync<InvalidCastException>()
                .WithMessage(
                    $"The first column returned by the SQL statement contains the value '' ({typeof(String)}), which " +
                    $"could not be converted to the type {typeof(Char)}. See inner exception for details.*"
                ))
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string '' to the target type {typeof(Char)}. The string must be exactly one " +
                $"character long."
            );

        (await Invoking(() =>
                    this.Connection.QueryScalarsAsync<Char>(
                        "SELECT 'ab'",
                        cancellationToken: TestContext.Current.CancellationToken
                    ).ToListAsync().AsTask()
                )
                .Should().ThrowAsync<InvalidCastException>()
                .WithMessage(
                    $"The first column returned by the SQL statement contains the value 'ab' ({typeof(String)}), " +
                    $"which could not be converted to the type {typeof(Char)}. See inner exception for details.*"
                ))
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'ab' to the target type {typeof(Char)}. The string must be exactly " +
                $"one character long."
            );
    }

    [Fact]
    public async Task QueryScalarsAsync_CharTargetType_ColumnContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        (await this.Connection.QueryScalarsAsync<Char>(
                $"SELECT '{character}'",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo([character]);
    }

    [Fact]
    public Task QueryScalarsAsync_ColumnValueCannotBeConvertedToTargetType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryScalarsAsync<Int32>(
                    "SELECT 'A'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains the value 'A' ({typeof(String)}), which " +
                $"could not be converted to the type {typeof(Int32)}. See inner exception for details.*"
            );

    [Fact]
    public async Task QueryScalarsAsync_CommandType_ShouldUseCommandType()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        (await this.Connection.QueryScalarsAsync<Int64>(
                "GetEntityIds",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities.Select(a => a.Id));
    }

    [Fact]
    public async Task QueryScalarsAsync_ComplexObjectsTemporaryTable_ShouldDropTemporaryAfterEnumerationIsFinished()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     StringValue
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var asyncEnumerator = this.Connection.QueryScalarsAsync<String>(
            statement
            , cancellationToken: TestContext.Current.CancellationToken).GetAsyncEnumerator();

        (await asyncEnumerator.MoveNextAsync())
            .Should().BeTrue();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        (await asyncEnumerator.MoveNextAsync())
            .Should().BeTrue();

        (await asyncEnumerator.MoveNextAsync())
            .Should().BeFalse();

        await asyncEnumerator.DisposeAsync();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        QueryScalarsAsync_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        (await this.Connection.QueryScalarsAsync<String>(
                $"""
                 SELECT     StringValue
                 FROM       {TemporaryTable(entities)}
                 """
                , cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities.Select(a => a.StringValue));
    }

    [Fact]
    public Task QueryScalarsAsync_EnumTargetType_ColumnContainsInvalidInteger_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryScalarsAsync<TestEnum>(
                    "SELECT 999",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains the value '999' ({typeof(Int32)}), which " +
                $"could not be converted to the type {typeof(TestEnum)}. See inner exception for details.*"
            );

    [Fact]
    public Task QueryScalarsAsync_EnumTargetType_ColumnContainsInvalidString_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryScalarsAsync<TestEnum>(
                    "SELECT 'DoesNotExist'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains the value 'DoesNotExist' " +
                $"({typeof(String)}), which could not be converted to the type {typeof(TestEnum)}. See inner " +
                $"exception for details.*"
            );

    [Fact]
    public async Task QueryScalarsAsync_EnumTargetType_ShouldConvertIntegerToEnum()
    {
        var enumValue = Generate.Enum();

        (await this.Connection.QueryScalarsAsync<TestEnum>(
                $"SELECT {(Int32)enumValue}",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo([enumValue]);
    }

    [Fact]
    public async Task QueryScalarsAsync_EnumTargetType_ShouldConvertStringToEnum()
    {
        var enumValue = Generate.Enum();

        (await this.Connection.QueryScalarsAsync<TestEnum>(
                $"SELECT '{enumValue.ToString()}'",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo([enumValue]);
    }

    [Fact]
    public Task QueryScalarsAsync_NonNullableTargetType_ColumnContainsNull_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryScalarsAsync<Int32>(
                    "SELECT NULL",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The first column returned by the SQL statement contains NULL, which could not be converted to the " +
                $"type {typeof(Int32)}. See inner exception for details.*"
            );

    [Fact]
    public async Task QueryScalarsAsync_NullableTargetType_ColumnContainsNull_ShouldReturnNull() =>
        (await this.Connection.QueryScalarsAsync<Int32?>(
            "SELECT NULL",
            cancellationToken: TestContext.Current.CancellationToken
        ).ToListAsync(TestContext.Current.CancellationToken).AsTask())
        .Should().BeEquivalentTo(new Int32?[] { null });

    [Fact]
    public async Task QueryScalarsAsync_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = await this.InsertNewEntityAsync();

        (await this.Connection.QueryScalarsAsync<String>(
                $"SELECT StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entity.StringValue);
    }

    [Fact]
    public async Task QueryScalarsAsync_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var asyncEnumerator = this.Connection.QueryScalarsAsync<Int64>(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .GetAsyncEnumerator();

        (await asyncEnumerator.MoveNextAsync())
            .Should().BeTrue();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        (await asyncEnumerator.MoveNextAsync())
            .Should().BeTrue();

        (await asyncEnumerator.MoveNextAsync())
            .Should().BeFalse();

        await asyncEnumerator.DisposeAsync();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        QueryScalarsAsync_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        (await this.Connection.QueryScalarsAsync<Int64>(
                $"SELECT Value FROM {TemporaryTable(entityIds)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entityIds);
    }

    [Fact]
    public Task QueryScalarsAsync_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.QueryScalarsAsync<Int32>(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowTimeoutSqlExceptionAsync();

    [Fact]
    public async Task QueryScalarsAsync_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber(), transaction);

            (await this.Connection.QueryScalarsAsync<String>(
                    "SELECT StringValue FROM Entity",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync())
                .Should().BeEquivalentTo(entities.Select(a => a.StringValue));

            await transaction.RollbackAsync();
        }

        (await this.Connection.QueryScalarsAsync<String>(
                "SELECT StringValue FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEmpty();
    }
}
