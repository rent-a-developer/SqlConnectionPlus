using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_QueryEntitiesTests : DatabaseTestsBase
{
    [Fact]
    public void QueryEntities_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() =>
                this.Connection.QueryEntities<Entity>(
                        "SELECT * FROM Entity",
                        cancellationToken: cancellationToken
                    )
                    .ToList()
            )
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public void QueryEntities_CharEntityProperty_ColumnContainsStringWithLengthNotOne_ShouldThrow()
    {
        Invoking(() =>
                this.Connection.QueryEntities<EntityWithCharProperty>(
                    "SELECT '' AS Char",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string '', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding property of the type " +
                $"{typeof(EntityWithCharProperty)}. The string must be exactly one character long.*"
            );

        Invoking(() =>
                this.Connection.QueryEntities<EntityWithCharProperty>(
                    "SELECT 'ab' AS Char",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string 'ab', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding property of the type " +
                $"{typeof(EntityWithCharProperty)}. The string must be exactly one character long.*"
            );
    }

    [Fact]
    public void QueryEntities_CharEntityProperty_ColumnContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        this.Connection.QueryEntities<EntityWithCharProperty>(
                $"SELECT '{character}' AS Char",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([new EntityWithCharProperty { Char = character }]);
    }

    [Fact]
    public void QueryEntities_ColumnDataTypeDoesNotMatchEntityPropertyType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryEntities<Entity>(
                    "SELECT 'NotAnId' AS Id FROM Entity",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(String)} of the column 'Id' returned by the SQL statement does not match " +
                $"the property type {typeof(Int64)} of the corresponding property of the type {typeof(Entity)}.*"
            );

    [Fact]
    public void QueryEntities_ColumnHasNoName_ShouldThrow()
    {
        var entity = this.InsertNewEntity();

        var transaction = this.Connection.BeginTransaction();

        Invoking(() =>
                this.Connection.QueryEntities<Entity>(
                    $"SELECT 1, * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                "The 1st column returned by the SQL statement does not have a name. Make sure that all columns the " +
                "SQL statement returns have a name.*"
            );
    }

    [Fact]
    public void QueryEntities_CommandType_ShouldUseCommandType()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        this.Connection.QueryEntities<Entity>(
                "GetEntities",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public void QueryEntities_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     *
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var enumerator = this.Connection.QueryEntities<Entity>(
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
    public void QueryEntities_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        this.Connection.QueryEntities<Entity>(
                $"""
                 SELECT     *
                 FROM       {TemporaryTable(entities)}
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public void QueryEntities_EntityTypeHasNoCorrespondingPropertyForColumn_ShouldThrow()
    {
        var entity = this.InsertNewEntity();

        Invoking(() =>
                this.Connection.QueryEntities<Entity>(
                    $"SELECT 1 AS NonExistingProperty, * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"Could not map the column 'NonExistingProperty' returned by the SQL statement to a property (with a " +
                $"public setter) of the type {typeof(Entity)}. Make sure the type has a corresponding property.*"
            );
    }

    [Fact]
    public void QueryEntities_EnumEntityProperty_ColumnContainsInvalidInteger_ShouldThrow() =>
        Invoking(() => this.Connection.QueryEntities<EntityWithEnumStoredAsInteger>(
                $"SELECT CAST(1 AS BIGINT) AS Id, 999 AS Enum",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToList())
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Enum' returned by the SQL statement contains a value that could not be converted to " +
                $"the enum type {typeof(TestEnum)} of the corresponding property of the type " +
                $"{typeof(EntityWithEnumStoredAsInteger)}. See inner exception for details.*"
            )
            .WithInnerException(typeof(InvalidCastException))
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. That value does not match any of the values of the enum's members.*"
            );

    [Fact]
    public void QueryEntities_EnumEntityProperty_ColumnContainsInvalidString_ShouldThrow() =>
        Invoking(() => this.Connection.QueryEntities<EntityWithEnumStoredAsString>(
                $"SELECT CAST(1 AS BIGINT) AS Id, 'DoesNotExist' AS Enum",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToList())
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Enum' returned by the SQL statement contains a value that could not be converted to " +
                $"the enum type {typeof(TestEnum)} of the corresponding property of the type " +
                $"{typeof(EntityWithEnumStoredAsString)}. See inner exception for details.*"
            )
            .WithInnerException(typeof(InvalidCastException))
            .WithMessage(
                $"Could not convert the string 'DoesNotExist' to an enum member of the type {typeof(TestEnum)}. " +
                $"That string does not match any of the names of the enum's members.*"
            );

    [Fact]
    public void QueryEntities_EnumEntityProperty_ShouldConvertIntegerToEnum()
    {
        var enumValue = Generate.Enum();

        this.Connection.QueryEntities<EntityWithEnumStoredAsInteger>(
                $"SELECT CAST(1 AS BIGINT) AS Id, {(Int32)enumValue} AS Enum",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .First().Enum
            .Should().Be(enumValue);
    }

    [Fact]
    public void QueryEntities_EnumEntityProperty_ShouldConvertStringToEnum()
    {
        var enumValue = Generate.Enum();

        this.Connection.QueryEntities<EntityWithEnumStoredAsInteger>(
                $"SELECT CAST(1 AS BIGINT) AS Id, '{enumValue.ToString()}' AS Enum",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .First().Enum
            .Should().Be(enumValue);
    }

    [Fact]
    public void QueryEntities_NonNullableEntityProperty_ColumnContainsNull_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryEntities<EntityWithNonNullableProperty>(
                    "SELECT NULL AS Value",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Value' returned by the SQL statement contains a " +
                $"NULL value, but the corresponding property of the type {typeof(EntityWithNonNullableProperty)} " +
                $"is non-nullable.*"
            );

    [Fact]
    public void QueryEntities_NullableEntityProperty_ColumnContainsNull_ShouldReturnNull() =>
        this.Connection.QueryEntities<EntityWithNullableProperty>(
                "SELECT NULL AS Value",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([new EntityWithNullableProperty { Value = null }]);

    [Fact]
    public void QueryEntities_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = this.InsertNewEntity();

        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([entity]);
    }

    [Fact]
    public void QueryEntities_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value AS Id FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var enumerator = this.Connection.QueryEntities<Entity>(
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
    public void QueryEntities_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entities = this.InsertNewEntities(5).ToList();
        var entityIds = entities.Take(2).Select(a => a.Id).ToList();

        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM Entity WHERE Id IN (SELECT Value FROM {TemporaryTable(entityIds)})",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities.Take(2));
    }

    [Fact]
    public void QueryEntities_ShouldFillPropertiesOfEntityWithRowData()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public void QueryEntities_ShouldMaterializeBinaryData() =>
        this.Connection.QueryEntities<EntityWithBinaryProperty>(
                $"SELECT 0x250 AS BinaryData",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([new EntityWithBinaryProperty { BinaryData = [0x02, 0x50] }]);

    [Fact]
    public void QueryEntities_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.QueryEntities<Entity>(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                ).ToList()
            )
            .Should().ThrowTimeoutSqlException();

    [Fact]
    public void QueryEntities_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = this.InsertNewEntities(Generate.SmallNumber(), transaction);

            this.Connection.QueryEntities<Entity>(
                    "SELECT * FROM Entity",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                )
                .Should().BeEquivalentTo(entities);

            transaction.Rollback();
        }

        this.Connection.QueryEntities<Entity>(
                "SELECT * FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEmpty();
    }

    [Fact]
    public void QueryEntities_UnsupportedFieldType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryEntities<EntityWithObjectProperty>(
                    "SELECT CONVERT(SQL_VARIANT, 123) AS Value",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToList()
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(Object)} of the column 'Value' returned by the SQL statement is not " +
                $"supported.*"
            );

    [Fact]
    public async Task QueryEntitiesAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.QueryEntitiesAsync<Entity>(
                    "SELECT * FROM Entity",
                    cancellationToken: cancellationToken
                ).ToListAsync(cancellationToken).AsTask()
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public async Task
        QueryEntitiesAsync_CharEntityProperty_ColumnContainsStringWithLengthNotOne_ShouldThrow()
    {
        await Invoking(() =>
                this.Connection.QueryEntitiesAsync<EntityWithCharProperty>(
                    "SELECT '' AS Char",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string '', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding property of the type " +
                $"{typeof(EntityWithCharProperty)}. The string must be exactly one character long.*"
            );

        await Invoking(() =>
                this.Connection.QueryEntitiesAsync<EntityWithCharProperty>(
                    "SELECT 'ab' AS Char",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync().AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string 'ab', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding property of the type " +
                $"{typeof(EntityWithCharProperty)}. The string must be exactly one character long.*"
            );
    }

    [Fact]
    public async Task
        QueryEntitiesAsync_CharEntityProperty_ColumnContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        (await this.Connection.QueryEntitiesAsync<EntityWithCharProperty>(
                $"SELECT '{character}' AS Char",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEquivalentTo([new EntityWithCharProperty { Char = character }]);
    }

    [Fact]
    public Task QueryEntitiesAsync_ColumnDataTypeDoesNotMatchEntityPropertyType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryEntitiesAsync<Entity>(
                    "SELECT 'NotAnId' AS Id FROM Entity",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(String)} of the column 'Id' returned by the SQL statement does not match " +
                $"the property type {typeof(Int64)} of the corresponding property of the type {typeof(Entity)}.*"
            );

    [Fact]
    public async Task QueryEntitiesAsync_ColumnHasNoName_ShouldThrow()
    {
        var entity = await this.InsertNewEntityAsync();

        var transaction = this.Connection.BeginTransaction();

        await Invoking(() =>
                this.Connection.QueryEntitiesAsync<Entity>(
                    $"SELECT 1, * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                "The 1st column returned by the SQL statement does not have a name. Make sure that all columns the " +
                "SQL statement returns have a name.*"
            );
    }

    [Fact]
    public async Task QueryEntitiesAsync_CommandType_ShouldUseCommandType()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        (await this.Connection.QueryEntitiesAsync<Entity>(
                "GetEntities",
                commandType: CommandType.StoredProcedure,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task
        QueryEntitiesAsync_ComplexObjectsTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entities = Generate.Entities(2);

        InterpolatedSqlStatement statement = $"""
                                              SELECT     *
                                              FROM       {TemporaryTable(entities)}
                                              """;

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var asyncEnumerator = this.Connection.QueryEntitiesAsync<Entity>(
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
        QueryEntitiesAsync_ComplexObjectsTemporaryTable_ShouldPassInterpolatedObjectsAsMultiColumnTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        (await this.Connection.QueryEntitiesAsync<Entity>(
                    $"""
                     SELECT     *
                     FROM       {TemporaryTable(entities)}
                     """
                    , cancellationToken: TestContext.Current.CancellationToken)
                .ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task QueryEntitiesAsync_EntityTypeHasNoCorrespondingPropertyForColumn_ShouldThrow()
    {
        var entity = await this.InsertNewEntityAsync();

        await Invoking(() =>
                this.Connection.QueryEntitiesAsync<Entity>(
                    $"SELECT 1 AS NonExistingProperty, * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"Could not map the column 'NonExistingProperty' returned by the SQL statement to a property (with a " +
                $"public setter) of the type {typeof(Entity)}. Make sure the type has a corresponding property.*"
            );
    }

    [Fact]
    public async Task QueryEntitiesAsync_EnumEntityProperty_InvalidInteger_ShouldThrow() =>
        await Invoking(() => this.Connection.QueryEntitiesAsync<EntityWithEnumStoredAsInteger>(
                $"SELECT CAST(1 AS BIGINT) AS Id, 999 AS Enum",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken).AsTask())
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The column 'Enum' returned by the SQL statement contains a value that could not be converted to " +
                $"the enum type {typeof(TestEnum)} of the corresponding property of the type " +
                $"{typeof(EntityWithEnumStoredAsInteger)}. See inner exception for details.*"
            )
            .WithInnerException(typeof(InvalidCastException))
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. That value does not match any of the values of the enum's members.*"
            );

    [Fact]
    public async Task QueryEntitiesAsync_EnumEntityProperty_InvalidString_ShouldThrow() =>
        await Invoking(() => this.Connection.QueryEntitiesAsync<EntityWithEnumStoredAsString>(
                $"SELECT CAST(1 AS BIGINT) AS Id, 'DoesNotExist' AS Enum",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken).AsTask())
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The column 'Enum' returned by the SQL statement contains a value that could not be converted to " +
                $"the enum type {typeof(TestEnum)} of the corresponding property of the type " +
                $"{typeof(EntityWithEnumStoredAsString)}. See inner exception for details.*"
            )
            .WithInnerException(typeof(InvalidCastException))
            .WithMessage(
                $"Could not convert the string 'DoesNotExist' to an enum member of the type {typeof(TestEnum)}. " +
                $"That string does not match any of the names of the enum's members.*"
            );

    [Fact]
    public async Task QueryEntitiesAsync_EnumEntityProperty_ShouldConvertIntegerToEnum()
    {
        var enumValue = Generate.Enum();

        (await this.Connection.QueryEntitiesAsync<EntityWithEnumStoredAsInteger>(
                $"SELECT CAST(1 AS BIGINT) AS Id, {(Int32)enumValue} AS Enum",
                cancellationToken: TestContext.Current.CancellationToken
            ).FirstAsync())
            .Enum
            .Should().Be(enumValue);
    }

    [Fact]
    public async Task QueryEntitiesAsync_EnumEntityProperty_ShouldConvertStringToEnum()
    {
        var enumValue = Generate.Enum();

        (await this.Connection.QueryEntitiesAsync<EntityWithEnumStoredAsInteger>(
                $"SELECT CAST(1 AS BIGINT) AS Id, '{enumValue.ToString()}' AS Enum",
                cancellationToken: TestContext.Current.CancellationToken
            ).FirstAsync())
            .Enum
            .Should().Be(enumValue);
    }

    [Fact]
    public Task QueryEntitiesAsync_NonNullableEntityProperty_ColumnValueIsNull_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryEntitiesAsync<EntityWithNonNullableProperty>(
                    "SELECT NULL AS Value",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<InvalidCastException>()
            .WithMessage(
                $"The column 'Value' returned by the SQL statement contains a " +
                $"NULL value, but the corresponding property of the type {typeof(EntityWithNonNullableProperty)} " +
                $"is non-nullable.*"
            );

    [Fact]
    public async Task QueryEntitiesAsync_NullableEntityProperty_ColumnValueIsNull_ShouldReturnNull() =>
        (await this.Connection.QueryEntitiesAsync<EntityWithNullableProperty>(
            "SELECT NULL AS Value",
            cancellationToken: TestContext.Current.CancellationToken
        ).ToListAsync(TestContext.Current.CancellationToken))
        .Should().BeEquivalentTo([new EntityWithNullableProperty { Value = null }]);

    [Fact]
    public async Task QueryEntitiesAsync_Parameter_ShouldPassInterpolatedParameter()
    {
        var entity = await this.InsertNewEntityAsync();

        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo([entity]);
    }

    [Fact]
    public async Task
        QueryEntitiesAsync_ScalarValuesTemporaryTable_ShouldDropTemporaryTableAfterEnumerationIsFinished()
    {
        var entityIds = Generate.EntityIds(2);

        InterpolatedSqlStatement statement = $"SELECT Value AS Id FROM {TemporaryTable(entityIds)}";

        var temporaryTableName = statement.TemporaryTables[0].Name;

        var asyncEnumerator = this.Connection.QueryEntitiesAsync<Entity>(
            statement,
            cancellationToken: TestContext.Current.CancellationToken
        ).GetAsyncEnumerator();

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
        QueryEntitiesAsync_ScalarValuesTemporaryTable_ShouldPassInterpolatedValuesAsSingleColumnTemporaryTable()
    {
        var entities = await this.InsertNewEntitiesAsync(5);
        var entityIds = entities.Take(2).Select(a => a.Id).ToList();

        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"SELECT * FROM Entity WHERE Id IN (SELECT Value FROM {TemporaryTable(entityIds)})",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities.Take(2));
    }

    [Fact]
    public async Task QueryEntitiesAsync_ShouldFillPropertiesOfEntityWithRowData()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"SELECT * FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task QueryEntitiesAsync_ShouldMaterializeBinaryData() =>
        (await this.Connection.QueryEntitiesAsync<EntityWithBinaryProperty>(
            $"SELECT 0x250 AS BinaryData",
            cancellationToken: TestContext.Current.CancellationToken
        ).ToListAsync(TestContext.Current.CancellationToken))
        .Should().BeEquivalentTo([new EntityWithBinaryProperty { BinaryData = [0x02, 0x50] }]);

    [Fact]
    public Task QueryEntitiesAsync_Timeout_ShouldUseTimeout() =>
        Invoking(() =>
                this.Connection.QueryEntitiesAsync<Entity>(
                    "WAITFOR DELAY '00:00:02';",
                    commandTimeout: TimeSpan.FromSeconds(1)
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowTimeoutSqlExceptionAsync();

    [Fact]
    public async Task QueryEntitiesAsync_Transaction_ShouldUseTransaction()
    {
        using (var transaction = this.Connection.BeginTransaction())
        {
            var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber(), transaction);

            (await this.Connection.QueryEntitiesAsync<Entity>(
                    "SELECT * FROM Entity",
                    transaction: transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync())
                .Should().BeEquivalentTo(entities);

            await transaction.RollbackAsync();
        }

        (await this.Connection.QueryEntitiesAsync<Entity>(
                "SELECT * FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync())
            .Should().BeEmpty();
    }

    [Fact]
    public Task QueryEntitiesAsync_UnsupportedFieldType_ShouldThrow() =>
        Invoking(() =>
                this.Connection.QueryEntitiesAsync<EntityWithObjectProperty>(
                    "SELECT CONVERT(SQL_VARIANT, 123) AS Value",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken).AsTask()
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(Object)} of the column 'Value' returned by the SQL statement is not " +
                $"supported.*"
            );
}
