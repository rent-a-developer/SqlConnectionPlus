using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_UpdateEntityTests : DatabaseTestsBase
{
    [Fact]
    public void UpdateEntity_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entity = this.InsertNewEntity();
        var updatedEntity = Generate.Update(entity);

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() => this.Connection.UpdateEntity(updatedEntity, cancellationToken: cancellationToken))
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entity should not have been updated.
        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .First()
            .Should().Be(entity);
    }

    [Fact]
    public void UpdateEntity_EntityHasNoKeyProperty_ShouldThrow() =>
        Invoking(() =>
                this.Connection.UpdateEntity(
                    new EntityWithoutKeyProperty(),
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"Could not get the key property of the type {typeof(EntityWithoutKeyProperty)}. " +
                $"Make sure that one property (with a public getter) of that type is denoted with a " +
                $"{typeof(KeyAttribute)}."
            );

    [Fact]
    public void UpdateEntity_EntityWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entity = this.InsertNewEntity();
        var updatedEntity = Generate.Update(entity);

        this.Connection.UpdateEntity(updatedEntity, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .First()
            .Should().Be(updatedEntity);
    }

    [Fact]
    public void UpdateEntity_EntityWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entity = this.InsertNewEntity();
        var updatedEntity = Generate.Update(entity);
        var updatedEntityWithTableAttribute = Generate.MapToEntityWithTableAttribute(updatedEntity);

        this.Connection.UpdateEntity(
            updatedEntityWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.Connection.QueryEntities<EntityWithTableAttribute>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .First()
            .Should().Be(updatedEntityWithTableAttribute);
    }

    [Fact]
    public void UpdateEntity_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entity = Generate.EntityWithEnumStoredAsInteger();
        var updatedEntity = Generate.Update(entity);

        this.Connection.InsertEntity(entity, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enum is stored as an integer:
        this.Connection.ExecuteScalar<Int32>(
                $"SELECT Enum FROM EntityWithEnumStoredAsInteger WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be((Int32)entity.Enum);

        this.Connection.UpdateEntity(updatedEntity, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enum is stored as an integer:
        this.Connection.ExecuteScalar<Int32>(
                $"SELECT Enum FROM EntityWithEnumStoredAsInteger WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be((Int32)updatedEntity.Enum);
    }

    [Fact]
    public void UpdateEntity_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entity = Generate.EntityWithEnumStoredAsString();
        var updatedEntity = Generate.Update(entity);

        this.Connection.InsertEntity(entity, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enum is stored as a string:
        this.Connection.ExecuteScalar<String>(
                $"SELECT Enum FROM EntityWithEnumStoredAsString WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(entity.Enum.ToString());

        this.Connection.UpdateEntity(updatedEntity, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(1);

        // Make sure the enum is stored as a string:
        this.Connection.ExecuteScalar<String>(
                $"SELECT Enum FROM EntityWithEnumStoredAsString WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(updatedEntity.Enum.ToString());
    }

    [Fact]
    public void UpdateEntity_ShouldIgnorePropertiesDenotedWithNotMappedAttribute()
    {
        var entity = this.InsertNewEntity();
        var updatedEntity = Generate.Update(entity) with { NotMappedProperty = "Not Mapped Value" };

        Invoking(() =>
                this.Connection.UpdateEntity(updatedEntity, cancellationToken: TestContext.Current.CancellationToken)
            )
            // UpdateEntity would throw if it tried to update the property NotMappedProperty, because no such column
            // exists in the Entity table.
            .Should().NotThrow();
    }

    [Fact]
    public void UpdateEntity_ShouldReturnNumberOfAffectedRows()
    {
        var entity = this.InsertNewEntity();
        var updatedEntity = Generate.Update(entity);

        this.Connection.UpdateEntity(updatedEntity, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(1);

        var nonExistentEntity = Generate.Entity();

        this.Connection.UpdateEntity(nonExistentEntity, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(0);
    }

    [Fact]
    public void UpdateEntity_ShouldUpdateDataFromProperties()
    {
        var entity = this.InsertNewEntity();
        var updatedEntity = Generate.Update(entity);

        this.Connection.UpdateEntity(updatedEntity, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(updatedEntity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .First()
            .Should().Be(updatedEntity);
    }

    [Fact]
    public void UpdateEntity_Transaction_ShouldUseTransaction()
    {
        var entity = this.InsertNewEntity();
        var updatedEntity = Generate.Update(entity);

        using (var transaction = this.Connection.BeginTransaction())
        {
            this.Connection.UpdateEntity(
                    updatedEntity,
                    transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                )
                .Should().Be(1);

            this.Connection.QueryEntities<Entity>("SELECT * FROM Entity", transaction)
                .Should().BeEquivalentTo([updatedEntity]);

            transaction.Rollback();
        }

        this.Connection.QueryEntities<Entity>("SELECT * FROM Entity")
            .Should().BeEquivalentTo([entity]);
    }

    [Fact]
    public async Task UpdateEntityAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entity = await this.InsertNewEntityAsync();
        var updatedEntity = Generate.Update(entity);

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() => this.Connection.UpdateEntityAsync(updatedEntity, cancellationToken: cancellationToken))
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entity should not have been updated.
        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).FirstAsync(TestContext.Current.CancellationToken))
            .Should().Be(entity);
    }

    [Fact]
    public Task UpdateEntityAsync_EntityHasNoKeyProperty_ShouldThrow() =>
        Invoking(() =>
                this.Connection.UpdateEntityAsync(
                    new EntityWithoutKeyProperty(),
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"Could not get the key property of the type {typeof(EntityWithoutKeyProperty)}. " +
                $"Make sure that one property (with a public getter) of that type is denoted with a " +
                $"{typeof(KeyAttribute)}."
            );

    [Fact]
    public async Task UpdateEntityAsync_EntityWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entity = await this.InsertNewEntityAsync();
        var updatedEntity = Generate.Update(entity);

        (await this.Connection.UpdateEntityAsync(
                updatedEntity,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(1);

        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).FirstAsync(TestContext.Current.CancellationToken))
            .Should().Be(updatedEntity);
    }

    [Fact]
    public async Task UpdateEntityAsync_EntityWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entity = await this.InsertNewEntityAsync();
        var updatedEntity = Generate.Update(entity);
        var updatedEntityWithTableAttribute = Generate.MapToEntityWithTableAttribute(updatedEntity);

        await this.Connection.UpdateEntityAsync(
            updatedEntityWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryEntitiesAsync<EntityWithTableAttribute>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).FirstAsync(TestContext.Current.CancellationToken))
            .Should().Be(updatedEntityWithTableAttribute);
    }

    [Fact]
    public async Task UpdateEntityAsync_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entity = Generate.EntityWithEnumStoredAsInteger();
        var updatedEntity = Generate.Update(entity);

        await this.Connection.InsertEntityAsync(entity, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enum is stored as an integer:
        (await this.Connection.ExecuteScalarAsync<Int32>(
                $"SELECT Enum FROM EntityWithEnumStoredAsInteger WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be((Int32)entity.Enum);

        await this.Connection.UpdateEntityAsync(
            updatedEntity,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Make sure the enum is stored as an integer:
        (await this.Connection.ExecuteScalarAsync<Int32>(
                $"SELECT Enum FROM EntityWithEnumStoredAsInteger WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be((Int32)updatedEntity.Enum);
    }

    [Fact]
    public async Task UpdateEntityAsync_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entity = Generate.EntityWithEnumStoredAsString();
        var updatedEntity = Generate.Update(entity);

        await this.Connection.InsertEntityAsync(entity, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enum is stored as a string:
        (await this.Connection.ExecuteScalarAsync<String>(
                $"SELECT Enum FROM EntityWithEnumStoredAsString WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entity.Enum.ToString());

        await this.Connection.UpdateEntityAsync(
            updatedEntity,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Make sure the enum is stored as a string:
        (await this.Connection.ExecuteScalarAsync<String>(
                $"SELECT Enum FROM EntityWithEnumStoredAsString WHERE Id = {Parameter(entity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(updatedEntity.Enum.ToString());
    }

    [Fact]
    public async Task UpdateEntityAsync_ShouldIgnorePropertiesDenotedWithNotMappedAttribute()
    {
        var entity = await this.InsertNewEntityAsync();
        var updatedEntity = Generate.Update(entity) with { NotMappedProperty = "Not Mapped Value" };

        await Invoking(() =>
                this.Connection.UpdateEntityAsync(
                    updatedEntity,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            // UpdateAsync would throw if it tried to update the property NotMappedProperty, because no such column
            // exists in the Entity table.
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateEntityAsync_ShouldReturnNumberOfAffectedRows()
    {
        var entity = await this.InsertNewEntityAsync();
        var updatedEntity = Generate.Update(entity);

        (await this.Connection.UpdateEntityAsync(
                updatedEntity,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(1);

        var nonExistentEntity = Generate.Entity();

        (await this.Connection.UpdateEntityAsync(
                nonExistentEntity,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(0);
    }

    [Fact]
    public async Task UpdateEntityAsync_ShouldUpdateDataFromProperties()
    {
        var entity = await this.InsertNewEntityAsync();
        var updatedEntity = Generate.Update(entity);

        await this.Connection.UpdateEntityAsync(
            updatedEntity,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"SELECT * FROM Entity WHERE Id = {Parameter(updatedEntity.Id)}",
                cancellationToken: TestContext.Current.CancellationToken
            ).FirstAsync(TestContext.Current.CancellationToken))
            .Should().Be(updatedEntity);
    }

    [Fact]
    public async Task UpdateEntityAsync_Transaction_ShouldUseTransaction()
    {
        var entity = await this.InsertNewEntityAsync();
        var updatedEntity = Generate.Update(entity);

        using (var transaction = this.Connection.BeginTransaction())
        {
            (await this.Connection.UpdateEntityAsync(
                    updatedEntity,
                    transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ))
                .Should().Be(1);

            (await this.Connection.QueryEntitiesAsync<Entity>("SELECT * FROM Entity", transaction).ToListAsync())
                .Should().BeEquivalentTo([updatedEntity]);

            await transaction.RollbackAsync();
        }

        (await this.Connection.QueryEntitiesAsync<Entity>("SELECT * FROM Entity").ToListAsync())
            .Should().BeEquivalentTo([entity]);
    }
}
