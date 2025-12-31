using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_QueryTuplesTests : DatabaseTestsBase
{
    [Fact]
    public void QueryTuples_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() =>
                this.Connection.QueryTuples<ValueTuple<Int32>>(
                    "SELECT 1",
                    cancellationToken: cancellationToken
                ).ToList()
            )
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public void QueryTuples_CharValueTupleField_ColumnContainsStringWithLengthNotOne_ShouldThrow()
    {
        Invoking(() =>
                this.Connection.QueryTuples<ValueTuple<Char>>(
                    "SELECT ''",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains the string '', which could not be converted " +
                $"to the type {typeof(Char)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Char>)}. The string must be exactly one character long.*"
            );

        Invoking(() =>
                this.Connection.QueryTuples<ValueTuple<Char>>(
                    "SELECT 'ab'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains the string 'ab', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Char>)}. The string must be exactly one character long.*"
            );
    }

    [Fact]
    public void
        QueryTuples_CharValueTupleField_ColumnContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        this.Connection.QueryTuples<ValueTuple<Char>>(
                $"SELECT '{character}'",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([ValueTuple.Create(character)]);
    }

    [Fact]
    public void QueryTuples_ColumnDataTypeDoesNotMatchValueTupleFieldType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuples<ValueTuple<Int32>>(
                    "SELECT 'A'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(String)} of the 1st column returned by the SQL statement does not match the " +
                $"field type {typeof(Int32)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Int32>)}.*"
            );

    [Fact]
    public void QueryTuples_CommandType_ShouldUseCommandType()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        this.Connection.QueryTuples<(Int64, String)>(
                "GetEntityIdsAndStringValues",
                commandType: CommandType.StoredProcedure, cancellationToken: TestContext.Current.CancellationToken)
            .Should().BeEquivalentTo(entities.Select(a => (a.Id, a.StringValue)));
    }

    [Fact]
    public void QueryTuples_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     Id, StringValue
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var enumerator = this.Connection.QueryTuples<(Int64, String)>(
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
    public void QueryTuples_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        this.Connection.QueryTuples<(Int64, String)>(
                $"""
                 SELECT     Id, StringValue
                 FROM       {TemporaryTable(entities)}
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities.Select(a => (a.Id, String: a.StringValue)));
    }

    [Fact]
    public void QueryTuples_EnumValueTupleField_ColumnContainsInvalidInteger_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuples<ValueTuple<TestEnum>>(
                    "SELECT 999",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains a value that could not be converted to the " +
                $"enum type {typeof(TestEnum)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<TestEnum>)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. That value does not match any of the values of the enum's members.*"
            );

    [Fact]
    public void QueryTuples_EnumValueTupleField_ColumnContainsInvalidString_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuples<ValueTuple<TestEnum>>(
                    "SELECT 'DoesNotExist'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains a value that could not be converted to the " +
                $"enum type {typeof(TestEnum)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<TestEnum>)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'DoesNotExist' to an enum member of the type {typeof(TestEnum)}. " +
                $"That string does not match any of the names of the enum's members.*"
            );

    [Fact]
    public void QueryTuples_EnumValueTupleField_ShouldConvertIntegerToEnum()
    {
        var enumValue = Generate.Enum();

        this.Connection.QueryTuples<ValueTuple<TestEnum>>(
                $"SELECT {(Int32)enumValue}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([ValueTuple.Create(enumValue)]);
    }

    [Fact]
    public void QueryTuples_EnumValueTupleField_ShouldConvertStringToEnum()
    {
        var enumValue = Generate.Enum();

        this.Connection.QueryTuples<ValueTuple<TestEnum>>(
                $"SELECT '{enumValue}'",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([ValueTuple.Create(enumValue)]);
    }

    [Fact]
    public void QueryTuples_NonNullableValueTupleField_ColumnContainsNull_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuples<ValueTuple<Int32>>(
                    "SELECT NULL",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains a NULL value, but the corresponding field " +
                $"of the value tuple type {typeof(ValueTuple<Int32>)} is non-nullable.*"
            );

    [Fact]
    public void QueryTuples_NullableValueTupleField_ColumnContainsNull_ShouldReturnNull() =>
        this.Connection.QueryTuples<ValueTuple<Int32?>>(
                "SELECT NULL",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([new ValueTuple<Int32?>(null)]);

    [Fact]
    public void QueryTuples_NumberOfColumnsDoesNotMatchNumberOfValueTupleFields_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuples<(Int32, Int32)>(
                    "SELECT 1",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The SQL statement returned 1 column, but the value tuple type {typeof((Int32, Int32))} has 2 " +
                $"fields. Make sure that the SQL statement returns the same number of columns as the number of " +
                $"fields in the value tuple type.*"
            );

    [Fact]
    public void QueryTuples_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = this.InsertNewEntity();

        this.Connection.QueryTuples<(Int64, String)>(
                $"SELECT Id, StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([(entity.Id, entity.StringValue)]);
    }

    [Fact]
    public void QueryTuples_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var enumerator = this.Connection.QueryTuples<ValueTuple<Int64>>(
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
    public void QueryTuples_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        this.Connection.QueryTuples<ValueTuple<Int64>>(
                $"SELECT Value FROM {TemporaryTable(entityIds)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entityIds.Select(a => ValueTuple.Create(a)));
    }

    [Fact]
    public void QueryTuples_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.QueryTuples<ValueTuple<Int32>>(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                ).ToList()
            )
            .Should().ThrowTimeoutSqlException();

    [Fact]
    public void QueryTuples_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = this.InsertNewEntities(Generate.SmallNumber(), transaction);

            this.Connection.QueryTuples<(Int64, String)>(
                    "SELECT Id, StringValue FROM Entity",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                )
                .Should().BeEquivalentTo(entities.Select(a => (a.Id, a.StringValue)));

            transaction.Rollback();
        }

        this.Connection.QueryTuples<(Int64, String)>(
                "SELECT Id, StringValue FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEmpty();
    }

    [Fact]
    public void QueryTuples_TypeIsNotValueTupleType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuples<NotAValueTuple>(
                    "SELECT 1",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The specified type {typeof(NotAValueTuple)} is not a value tuple type or it is a value tuple type " +
                $"with more than 7 fields.*"
            );

    [Fact]
    public void QueryTuples_TypeIsValueTupleWithMoreThan7Fields_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuples<(Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32)>(
                    "SELECT 1, 1, 1, 1, 1, 1, 1, 1",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The specified type {typeof((Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32))} is not a " +
                $"value tuple type or it is a value tuple type with more than 7 fields.*"
            );

    [Fact]
    public async Task QueryTuplesAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.QueryTuplesAsync<ValueTuple<Int32>>(
                    "SELECT 1",
                    cancellationToken: cancellationToken
                ).ToListAsync(cancellationToken: cancellationToken).AsTask()
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public async Task QueryTuplesAsync_CharValueTupleField_ColumnContainsStringWithLengthNotOne_ShouldThrow()
    {
        await Invoking(() =>
                this.Connection.QueryTuplesAsync<ValueTuple<Char>>(
                    "SELECT ''",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains the string '', which could not be converted " +
                $"to the type {typeof(Char)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Char>)}. The string must be exactly one character long.*"
            );

        await Invoking(() =>
                this.Connection.QueryTuplesAsync<ValueTuple<Char>>(
                    "SELECT 'ab'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains the string 'ab', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Char>)}. The string must be exactly one character long.*"
            );
    }

    [Fact]
    public async Task
        QueryTuplesAsync_CharValueTupleField_ColumnContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        (await this.Connection.QueryTuplesAsync<ValueTuple<Char>>(
                $"SELECT '{character}'",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo([ValueTuple.Create(character)]);
    }

    [Fact]
    public async Task QueryTuplesAsync_ColumnDataTypeDoesNotMatchValueTupleFieldType_ShouldThrow() =>
        await Invoking(() =>
                this.Connection.QueryTuplesAsync<ValueTuple<Int32>>(
                    "SELECT 'A'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(String)} of the 1st column returned by the SQL statement does not match the " +
                $"field type {typeof(Int32)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Int32>)}.*"
            );

    [Fact]
    public async Task QueryTuplesAsync_CommandType_ShouldUseCommandType()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        (await this.Connection.QueryTuplesAsync<(Int64, String)>(
                "GetEntityIdsAndStringValues",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo(entities.Select(a => (a.Id, a.StringValue)));
    }

    [Fact]
    public async Task QueryTuplesAsync_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     Id, StringValue
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var enumerator = this.Connection.QueryTuplesAsync<(Int64, String)>(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        ).GetAsyncEnumerator();

        (await enumerator.MoveNextAsync())
            .Should().BeTrue();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        (await enumerator.MoveNextAsync())
            .Should().BeTrue();

        (await enumerator.MoveNextAsync())
            .Should().BeFalse();

        await enumerator.DisposeAsync();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        QueryTuplesAsync_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        (await this.Connection.QueryTuplesAsync<(Int64, String)>(
                $"""
                 SELECT     Id, StringValue
                 FROM       {TemporaryTable(entities)}
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo(entities.Select(a => (a.Id, String: a.StringValue)));
    }

    [Fact]
    public Task QueryTuplesAsync_EnumValueTupleField_ColumnContainsInvalidInteger_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuplesAsync<ValueTuple<TestEnum>>(
                    "SELECT 999",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains a value that could not be converted to the " +
                $"enum type {typeof(TestEnum)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<TestEnum>)}. See inner exception for details.*"
            )
            .WithInnerException(typeof(InvalidCastException))
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. That value does not match any of the values of the enum's members.*"
            );

    [Fact]
    public Task QueryTuplesAsync_EnumValueTupleField_ColumnContainsInvalidString_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuplesAsync<ValueTuple<TestEnum>>(
                    "SELECT 'DoesNotExist'",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains a value that could not be converted to the " +
                $"enum type {typeof(TestEnum)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<TestEnum>)}. See inner exception for details.*"
            )
            .WithInnerException(typeof(InvalidCastException))
            .WithMessage(
                $"Could not convert the string 'DoesNotExist' to an enum member of the type {typeof(TestEnum)}. " +
                $"That string does not match any of the names of the enum's members.*"
            );

    [Fact]
    public async Task QueryTuplesAsync_EnumValueTupleField_ShouldConvertIntegerToEnum()
    {
        var enumValue = Generate.Enum();

        (await this.Connection.QueryTuplesAsync<ValueTuple<TestEnum>>(
                $"SELECT {(Int32)enumValue}",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo([ValueTuple.Create(enumValue)]);
    }

    [Fact]
    public async Task QueryTuplesAsync_EnumValueTupleField_ShouldConvertStringToEnum()
    {
        var enumValue = Generate.Enum();

        (await this.Connection.QueryTuplesAsync<ValueTuple<TestEnum>>(
                $"SELECT '{enumValue}'",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo([ValueTuple.Create(enumValue)]);
    }

    [Fact]
    public Task QueryTuplesAsync_NonNullableValueTupleField_ColumnContainsNull_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuplesAsync<ValueTuple<Int32>>(
                    "SELECT NULL",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The 1st column returned by the SQL statement contains a NULL value, but the corresponding field " +
                $"of the value tuple type {typeof(ValueTuple<Int32>)} is non-nullable.*"
            );

    [Fact]
    public async Task QueryTuplesAsync_NullableValueTupleField_ColumnContainsNull_ShouldReturnNull() =>
        (await this.Connection.QueryTuplesAsync<ValueTuple<Int32?>>(
            "SELECT NULL",
            cancellationToken: TestContext.Current.CancellationToken
        ).ToListAsync())
        .Should().BeEquivalentTo([new ValueTuple<Int32?>(null)]);

    [Fact]
    public Task QueryTuplesAsync_NumberOfColumnsDoesNotMatchNumberOfValueTupleFields_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuplesAsync<(Int32, Int32)>(
                    "SELECT 1",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"The SQL statement returned 1 column, but the value tuple type {typeof((Int32, Int32))} has 2 " +
                $"fields. Make sure that the SQL statement returns the same number of columns as the number of " +
                $"fields in the value tuple type.*"
            );

    [Fact]
    public async Task QueryTuplesAsync_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = await this.InsertNewEntityAsync();

        (await this.Connection.QueryTuplesAsync<(Int64, String)>(
                $"SELECT Id, StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo([(entity.Id, entity.StringValue)]);
    }

    [Fact]
    public async Task QueryTuplesAsync_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var enumerator = this.Connection.QueryTuplesAsync<ValueTuple<Int64>>(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        ).GetAsyncEnumerator();

        (await enumerator.MoveNextAsync())
            .Should().BeTrue();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeTrue();

        (await enumerator.MoveNextAsync())
            .Should().BeTrue();

        (await enumerator.MoveNextAsync())
            .Should().BeFalse();

        await enumerator.DisposeAsync();

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        QueryTuplesAsync_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        (await this.Connection.QueryTuplesAsync<ValueTuple<Int64>>(
                $"SELECT Value FROM {TemporaryTable(entityIds)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo(entityIds.Select(a => ValueTuple.Create(a)));
    }

    [Fact]
    public Task QueryTuplesAsync_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.QueryTuplesAsync<ValueTuple<Int32>>(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                ).ToListAsync().AsTask()
            )
            .Should().ThrowTimeoutSqlExceptionAsync();

    [Fact]
    public async Task QueryTuplesAsync_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber(), transaction);

            (await this.Connection.QueryTuplesAsync<(Int64, String)>(
                    "SELECT Id, StringValue FROM Entity",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync())
                .Should().BeEquivalentTo(entities.Select(a => (a.Id, a.StringValue)));

            transaction.Rollback();
        }

        (await this.Connection.QueryTuplesAsync<(Int64, String)>(
                "SELECT Id, StringValue FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEmpty();
    }

    [Fact]
    public Task QueryTuplesAsync_TypeIsNotValueTupleType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuplesAsync<NotAValueTuple>(
                    "SELECT 1",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"The specified type {typeof(NotAValueTuple)} is not a value tuple type or it is a value tuple type " +
                $"with more than 7 fields.*"
            );

    [Fact]
    public Task QueryTuplesAsync_TypeIsValueTupleWithMoreThan7Fields_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryTuplesAsync<(Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32)>(
                    "SELECT 1, 1, 1, 1, 1, 1, 1, 1",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"The specified type {typeof((Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32))} is not a " +
                $"value tuple type or it is a value tuple type with more than 7 fields.*"
            );
}
