using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_InsertEntityTests : DatabaseTestsBase
{
    [Fact]
    public void InsertEntity_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entity = Generate.Entity();

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() => this.Connection.InsertEntity(entity, cancellationToken: cancellationToken))
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entity should not have been inserted.
        this.ExistsEntityById(entity.Id)
            .Should().BeFalse();
    }

    [Fact]
    public void InsertEntity_EntityWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entity = Generate.Entity();

        this.Connection.InsertEntity(entity, cancellationToken: TestContext.Current.CancellationToken);

        this.ExistsEntityById(entity.Id)
            .Should().BeTrue();
    }

    [Fact]
    public void InsertEntity_EntityWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entity = Generate.Entity();
        var entityWithTableAttribute = Generate.MapToEntityWithTableAttribute(entity);

        this.Connection.InsertEntity(
            entityWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.ExistsEntityById(entityWithTableAttribute.Id)
            .Should().BeTrue();
    }

    [Fact]
    public void InsertEntity_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entity = Generate.EntityWithEnumStoredAsInteger();

        this.Connection.InsertEntity(entity, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.ExecuteScalar<Int32>(
                $"SELECT Enum FROM EntityWithEnumStoredAsInteger WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be((Int32)entity.Enum);
    }

    [Fact]
    public void InsertEntity_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entity = Generate.EntityWithEnumStoredAsString();

        this.Connection.InsertEntity(entity, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.ExecuteScalar<String>(
                $"SELECT Enum FROM EntityWithEnumStoredAsString WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entity.Enum.ToString());
    }

    [Fact]
    public void InsertEntity_ShouldIgnorePropertiesDenotedWithNotMappedAttribute()
    {
        var entity = Generate.Entity();
        entity.NotMappedProperty = "Not Mapped Value";

        Invoking(() =>
                this.Connection.InsertEntity(entity, cancellationToken: TestContext.Current.CancellationToken)
            )
            // InsertEntity would throw if it tried to insert the property NotMappedProperty, because no such column
            // exists in the Entity table.
            .Should().NotThrow();
    }

    [Fact]
    public void InsertEntity_ShouldInsertDataFromProperties()
    {
        var entity = Generate.Entity();

        this.Connection.InsertEntity(entity, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().HaveCount(1)
            .And.BeEquivalentTo([entity]);
    }

    [Fact]
    public void InsertEntity_ShouldReturnNumberOfAffectedRows()
    {
        var entity = Generate.Entity();

        this.Connection.InsertEntity(entity, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(1);
    }

    [Fact]
    public void InsertEntity_Transaction_ShouldUseTransaction()
    {
        var entity = Generate.Entity();

        using (var transaction = this.Connection.BeginTransaction())
        {
            this.Connection.InsertEntity(
                entity,
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            ).Should().Be(1);

            this.ExistsEntityById(entity.Id, transaction)
                .Should().BeTrue();

            transaction.Rollback();
        }

        this.ExistsEntityById(entity.Id)
            .Should().BeFalse();
    }

    [Fact]
    public async Task InsertEntityAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entity = Generate.Entity();

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.InsertEntityAsync(entity, cancellationToken: cancellationToken)
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entity should not have been inserted.
        (await this.ExistsEntityByIdAsync(entity.Id))
            .Should().BeFalse();
    }

    [Fact]
    public async Task InsertEntityAsync_EntityWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entity = Generate.Entity();

        await this.Connection.InsertEntityAsync(
            entity,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsEntityByIdAsync(entity.Id))
            .Should().BeTrue();
    }

    [Fact]
    public async Task InsertEntityAsync_EntityWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entity = Generate.Entity();
        var entityWithTableAttribute = Generate.MapToEntityWithTableAttribute(entity);

        await this.Connection.InsertEntityAsync(
            entityWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsEntityByIdAsync(entityWithTableAttribute.Id))
            .Should().BeTrue();
    }

    [Fact]
    public async Task InsertEntityAsync_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entity = Generate.EntityWithEnumStoredAsInteger();

        await this.Connection.InsertEntityAsync(entity, cancellationToken: TestContext.Current.CancellationToken);

        (await this.Connection.ExecuteScalarAsync<Int32>(
                $"SELECT Enum FROM EntityWithEnumStoredAsInteger WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be((Int32)entity.Enum);
    }

    [Fact]
    public async Task InsertEntityAsync_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entity = Generate.EntityWithEnumStoredAsString();

        await this.Connection.InsertEntityAsync(entity, cancellationToken: TestContext.Current.CancellationToken);

        (await this.Connection.ExecuteScalarAsync<String>(
                $"SELECT Enum FROM EntityWithEnumStoredAsString WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entity.Enum.ToString());
    }

    [Fact]
    public async Task InsertEntityAsync_ShouldIgnorePropertiesDenotedWithNotMappedAttribute()
    {
        var entity = Generate.Entity();
        entity.NotMappedProperty = "Not Mapped Value";

        await Invoking(() =>
                this.Connection.InsertEntityAsync(
                    entity,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            // InsertAsync would throw if it tried to insert the property NotMappedProperty, because no such column
            // exists in the Entity table.
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertEntityAsync_ShouldInsertDataFromProperties()
    {
        var entity = Generate.Entity();

        await this.Connection.InsertEntityAsync(
            entity,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryEntitiesAsync<Entity>(
                    $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                    cancellationToken: TestContext.Current.CancellationToken
                ).ToListAsync(TestContext.Current.CancellationToken)
            ).Should().BeEquivalentTo([entity]);
    }

    [Fact]
    public async Task InsertEntityAsync_ShouldReturnNumberOfAffectedRows()
    {
        var entity = Generate.Entity();

        (await this.Connection.InsertEntityAsync(
                entity,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(1);
    }

    [Fact]
    public async Task InsertEntityAsync_Transaction_ShouldUseTransaction()
    {
        var entity = Generate.Entity();

        using (var transaction = this.Connection.BeginTransaction())
        {
            (await this.Connection.InsertEntityAsync(
                entity,
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            )).Should().Be(1);

            (await this.ExistsEntityByIdAsync(entity.Id, transaction))
                .Should().BeTrue();

            await transaction.RollbackAsync();
        }

        (await this.ExistsEntityByIdAsync(entity.Id))
            .Should().BeFalse();
    }
}
