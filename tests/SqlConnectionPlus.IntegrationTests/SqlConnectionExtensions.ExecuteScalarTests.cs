using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_ExecuteScalarTests : DatabaseTestsBase
{
    [Fact]
    public void ExecuteScalar_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() =>
                this.Connection.ExecuteScalar<Int32>("SELECT 1", cancellationToken: cancellationToken)
            )
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public void ExecuteScalar_ColumnValueCannotBeConvertedToTargetType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.ExecuteScalar<Int32>(
                    "SELECT 'A'",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains the " +
                $"value 'A' ({typeof(String)}), which could not be converted to the type {typeof(Int32)}.*"
            );

    [Fact]
    public void ExecuteScalar_CommandType_ShouldUseCommandType()
    {
        var entity = this.InsertNewEntity();

        this.Connection.ExecuteScalar<Int64>(
                $"GetFirstEntityId",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entity.Id);
    }

    [Fact]
    public void ExecuteScalar_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entities = Generate.Entities(1);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     TOP 1 StringValue
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        this.Connection.ExecuteScalar<String>(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entities[0].StringValue);

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteScalar_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(1);

        this.Connection.ExecuteScalar<String>(
                $"""
                 SELECT     TOP 1 StringValue
                 FROM       {TemporaryTable(entities)}
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entities[0].StringValue);
    }

    [Fact]
    public void ExecuteScalar_NoResultSet_ShouldReturnDefault()
    {
        this.Connection.ExecuteScalar<Object>(
                "SET NOCOUNT ON;",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeNull();

        this.Connection.ExecuteScalar<Int32>(
                "SET NOCOUNT ON;",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(0);
    }

    [Fact]
    public void ExecuteScalar_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = this.InsertNewEntity();

        this.Connection.ExecuteScalar<String>(
                $"SELECT TOP 1 StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entity.StringValue);
    }

    [Fact]
    public void ExecuteScalar_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entityIds = Generate.EntityIds(1);

        InterpolatedSqlStatement statement = $"SELECT TOP 1 Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        this.Connection.ExecuteScalar<Int64>(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entityIds[0]);

        this.ExistsTemporaryTable(temporaryTableName)
            .Should().BeFalse();
    }

    [Fact]
    public void ExecuteScalar_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(1);

        this.Connection.ExecuteScalar<Int64>(
                $"SELECT TOP 1 Value FROM {TemporaryTable(entityIds)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entityIds[0]);
    }

    [Fact]
    public void ExecuteScalar_TargetTypeIsChar_ColumnValueIsStringWithLengthNotOne_ShouldThrow()
    {
        Invoking(() =>
                this.Connection.ExecuteScalar<Char>(
                    "SELECT ''",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains the " +
                $"value '' ({typeof(String)}), which could not be converted to the type {typeof(Char)}. See inner " +
                $"exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string '' to the target type {typeof(Char)}. The string must be " +
                $"exactly one character long."
            );

        Invoking(() =>
                this.Connection.ExecuteScalar<Char>(
                    "SELECT 'ab'",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains the " +
                $"value 'ab' ({typeof(String)}), which could not be converted to the type {typeof(Char)}. See inner " +
                $"exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'ab' to the target type {typeof(Char)}. The string must be " +
                $"exactly one character long."
            );
    }

    [Fact]
    public void ExecuteScalar_TargetTypeIsChar_ColumnValueIsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        this.Connection.ExecuteScalar<Char>(
                $"SELECT '{character}'",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(character);
    }

    [Fact]
    public void ExecuteScalar_TargetTypeIsEnum_ColumnValueIsInteger_ShouldConvertIntegerToEnum()
    {
        var enumValue = Generate.Enum();

        this.Connection.ExecuteScalar<TestEnum>(
                $"SELECT {(Int32)enumValue}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(enumValue);
    }

    [Fact]
    public void ExecuteScalar_TargetTypeIsEnum_ColumnValueIsInvalidInteger_ShouldThrow() =>
        Invoking(() =>
                this.Connection.ExecuteScalar<TestEnum>(
                    "SELECT 999",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains the " +
                $"value '999' ({typeof(Int32)}), which could not be converted to the type {typeof(TestEnum)}.*"
            );

    [Fact]
    public void ExecuteScalar_TargetTypeIsEnum_ColumnValueIsInvalidString_ShouldThrow() =>
        Invoking(() =>
                this.Connection.ExecuteScalar<TestEnum>(
                    "SELECT 'DoesNotExist'",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains the " +
                $"value 'DoesNotExist' ({typeof(String)}), which could not be converted to the type " +
                $"{typeof(TestEnum)}.*"
            );

    [Fact]
    public void ExecuteScalar_TargetTypeIsEnum_ColumnValueIsString_ShouldConvertStringToEnum()
    {
        var enumValue = Generate.Enum();

        this.Connection.ExecuteScalar<TestEnum>(
                $"SELECT '{enumValue.ToString()}'",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(enumValue);
    }

    [Fact]
    public void ExecuteScalar_TargetTypeIsNonNullable_ColumnValueIsNull_ShouldThrow() =>
        Invoking(() =>
                this.Connection.ExecuteScalar<Int32>(
                    "SELECT NULL",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains NULL, " +
                $"which could not be converted to the type {typeof(Int32)}.*"
            );

    [Fact]
    public void ExecuteScalar_TargetTypeIsNullable_ColumnValueIsNull_ShouldReturnNull() =>
        this.Connection.ExecuteScalar<Int32?>(
                "SELECT NULL",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeNull();

    [Fact]
    public void ExecuteScalar_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExecuteScalar<Int32>(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                )
            )
            .Should().ThrowTimeoutSqlException();

    [Fact]
    public void ExecuteScalar_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entity = this.InsertNewEntity(transaction);

            this.Connection.ExecuteScalar<String>(
                    "SELECT TOP 1 StringValue FROM Entity",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                )
                .Should().Be(entity.StringValue);

            transaction.Rollback();
        }

        this.Connection.ExecuteScalar<String>(
                "SELECT TOP 1 StringValue FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeNull();
    }

    [Fact]
    public async Task ExecuteScalarAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.ExecuteScalarAsync<Int32>(
                    "SELECT 1",
                    cancellationToken: cancellationToken
                )
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public Task ExecuteScalarAsync_ColumnValueCannotBeConvertedToTargetType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.ExecuteScalarAsync<Int32>(
                    "SELECT 'A'",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains the " +
                $"value 'A' ({typeof(String)}), which could not be converted to the type {typeof(Int32)}.*"
            );

    [Fact]
    public async Task ExecuteScalarAsync_CommandType_ShouldUseCommandType()
    {
        var entity = await this.InsertNewEntityAsync();

        (await this.Connection.ExecuteScalarAsync<Int64>(
                $"GetFirstEntityId",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entity.Id);
    }

    [Fact]
    public async Task ExecuteScalarAsync_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entities = Generate.Entities(1);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     TOP 1 StringValue
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        (await this.Connection.ExecuteScalarAsync<String>(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entities[0].StringValue);

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExecuteScalarAsync_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(1);

        (await this.Connection.ExecuteScalarAsync<String>(
                $"""
                 SELECT     TOP 1 StringValue
                 FROM       {TemporaryTable(entities)}
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entities[0].StringValue);
    }

    [Fact]
    public async Task ExecuteScalarAsync_NoResultSet_ShouldReturnDefault()
    {
        (await this.Connection.ExecuteScalarAsync<Object>(
                "SET NOCOUNT ON;",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeNull();

        (await this.Connection.ExecuteScalarAsync<Int32>(
                "SET NOCOUNT ON;",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(0);
    }

    [Fact]
    public async Task ExecuteScalarAsync_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = await this.InsertNewEntityAsync();

        (await this.Connection.ExecuteScalarAsync<String>(
                $"SELECT TOP 1 StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entity.StringValue);
    }

    [Fact]
    public async Task ExecuteScalarAsync_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterExecution()
    {
        var entityIds = Generate.EntityIds(1);

        InterpolatedSqlStatement statement = $"SELECT TOP 1 Value FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        (await this.Connection.ExecuteScalarAsync<Int64>(
                statement,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entityIds[0]);

        (await this.ExistsTemporaryTableAsync(temporaryTableName))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        ExecuteScalarAsync_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entityIds = Generate.EntityIds(1);

        (await this.Connection.ExecuteScalarAsync<Int64>(
                $"SELECT TOP 1 Value FROM {TemporaryTable(entityIds)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entityIds[0]);
    }

    [Fact]
    public async Task ExecuteScalarAsync_TargetTypeIsChar_ColumnValueIsStringWithLengthNotOne_ShouldThrow()
    {
        (await Invoking(() =>
                    this.Connection.ExecuteScalarAsync<Char>(
                        "SELECT ''",
                        cancellationToken: TestContext.Current.CancellationToken
                    )
                )
                .Should().ThrowAsync<InvalidCastException>()
                .WithMessage(
                    $"The first column of the first row in the result set returned by the SQL statement contains the " +
                    $"value '' ({typeof(String)}), which could not be converted to the type {typeof(Char)}. See " +
                    $"inner exception for details.*"
                ))
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string '' to the target type {typeof(Char)}. The string must be " +
                $"exactly one character long."
            );

        (await Invoking(() =>
                    this.Connection.ExecuteScalarAsync<Char>(
                        "SELECT 'ab'",
                        cancellationToken: TestContext.Current.CancellationToken
                    )
                )
                .Should().ThrowAsync<InvalidCastException>()
                .WithMessage(
                    $"The first column of the first row in the result set returned by the SQL statement contains the " +
                    $"value 'ab' ({typeof(String)}), which could not be converted to the type {typeof(Char)}. See " +
                    $"inner exception for details.*"
                ))
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'ab' to the target type {typeof(Char)}. The string must be " +
                $"exactly one character long."
            );
    }

    [Fact]
    public async Task
        ExecuteScalarAsync_TargetTypeIsChar_ColumnValueIsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        (await this.Connection.ExecuteScalarAsync<Char>(
                $"SELECT '{character}'",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(character);
    }

    [Fact]
    public async Task ExecuteScalarAsync_TargetTypeIsEnum_ColumnValueIsInteger_ShouldConvertIntegerToEnum()
    {
        var enumValue = Generate.Enum();

        (await this.Connection.ExecuteScalarAsync<TestEnum>(
                $"SELECT {(Int32)enumValue}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(enumValue);
    }

    [Fact]
    public Task ExecuteScalarAsync_TargetTypeIsEnum_ColumnValueIsInvalidInteger_ShouldThrow() =>
        Invoking(() =>
                this.Connection.ExecuteScalarAsync<TestEnum>(
                    "SELECT 999",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains the " +
                $"value '999' ({typeof(Int32)}), which could not be converted to the type {typeof(TestEnum)}.*"
            );

    [Fact]
    public Task ExecuteScalarAsync_TargetTypeIsEnum_ColumnValueIsInvalidString_ShouldThrow() =>
        Invoking(() =>
                this.Connection.ExecuteScalarAsync<TestEnum>(
                    "SELECT 'DoesNotExist'",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains the " +
                $"value 'DoesNotExist' ({typeof(String)}), which could not be converted to the type " +
                $"{typeof(TestEnum)}.*"
            );

    [Fact]
    public async Task ExecuteScalarAsync_TargetTypeIsEnum_ColumnValueIsString_ShouldConvertStringToEnum()
    {
        var enumValue = Generate.Enum();

        (await this.Connection.ExecuteScalarAsync<TestEnum>(
                $"SELECT '{enumValue}'",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(enumValue);
    }

    [Fact]
    public Task ExecuteScalarAsync_TargetTypeIsNonNullable_ColumnValueIsNull_ShouldThrow() =>
        Invoking(() =>
                this.Connection.ExecuteScalarAsync<Int32>(
                    "SELECT NULL",
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The first column of the first row in the result set returned by the SQL statement contains NULL, " +
                $"which could not be converted to the type {typeof(Int32)}.*"
            );

    [Fact]
    public async Task ExecuteScalarAsync_TargetTypeIsNullable_ColumnValueIsNull_ShouldReturnNull() =>
        (await this.Connection.ExecuteScalarAsync<Int32?>(
            "SELECT NULL",
            cancellationToken: TestContext.Current.CancellationToken
        ))
        .Should().BeNull();

    [Fact]
    public Task ExecuteScalarAsync_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.ExecuteScalarAsync<Int32>(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                )
            )
            .Should().ThrowTimeoutSqlExceptionAsync();

    [Fact]
    public async Task ExecuteScalarAsync_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entity = await this.InsertNewEntityAsync(transaction);

            (await this.Connection.ExecuteScalarAsync<String>(
                    "SELECT TOP 1 StringValue FROM Entity",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ))
                .Should().Be(entity.StringValue);

            await transaction.RollbackAsync();
        }

        (await this.Connection.ExecuteScalarAsync<String>(
                "SELECT TOP 1 StringValue FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeNull();
    }
}
