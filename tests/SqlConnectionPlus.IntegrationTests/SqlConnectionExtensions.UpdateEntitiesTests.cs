using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_UpdateEntitiesTests : DatabaseTestsBase
{
    [Fact]
    public void UpdateEntities_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();
        var updatedEntities = Generate.Updates(entities);

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() => this.Connection.UpdateEntities(updatedEntities, cancellationToken: cancellationToken))
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entities should not have been updated.
        this.Connection.QueryEntities<Entity>(
                $"SELECT * FROM Entity WHERE Id IN (SELECT Value FROM {TemporaryTable(entityIds)})",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public void UpdateEntities_EntitiesHaveNoKeyProperty_ShouldThrow() =>
        Invoking(() =>
                this.Connection.UpdateEntities(
                    [new EntityWithoutKeyProperty()],
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
    public void UpdateEntities_EntitiesWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();
        var updatedEntities = Generate.Updates(entities);

        this.Connection.UpdateEntities(updatedEntities, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.QueryEntities<Entity>(
                $"""
                 SELECT *
                 FROM   Entity
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(updatedEntities);
    }

    [Fact]
    public void UpdateEntities_EntitiesWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();
        var updatedEntities = Generate.Updates(entities);
        var updatedEntitiesWithTableAttribute = Generate.MapToEntitiesWithTableAttribute(updatedEntities);

        this.Connection.UpdateEntities(
            updatedEntitiesWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.Connection.QueryEntities<EntityWithTableAttribute>(
                $"""
                 SELECT *
                 FROM   Entity
                 WHERE  Id IN
                        (
                            SELECT  Value
                            FROM    {TemporaryTable(entityIds)}
                        )
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(updatedEntitiesWithTableAttribute);
    }

    [Fact]
    public void UpdateEntities_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entities = Generate.EntitiesWithEnumStoredAsInteger(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();

        this.Connection.InsertEntities(entities, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enums are stored as integers:
        this.Connection.QueryScalars<Int32>(
                $"""
                 SELECT Enum
                 FROM   EntityWithEnumStoredAsInteger
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities.Select(a => (Int32)a.Enum));

        var updatedEntities = Generate.Updates(entities);

        this.Connection.UpdateEntities(updatedEntities, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enums are stored as integers:
        this.Connection.QueryScalars<Int32>(
                $"""
                 SELECT Enum
                 FROM   EntityWithEnumStoredAsInteger
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(updatedEntities.Select(a => (Int32)a.Enum));
    }

    [Fact]
    public void UpdateEntities_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entities = Generate.EntitiesWithEnumStoredAsString(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();

        this.Connection.InsertEntities(entities, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enums are stored as strings:
        this.Connection.QueryScalars<String>(
                $"""
                 SELECT Enum
                 FROM   EntityWithEnumStoredAsString
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities.Select(a => a.Enum.ToString()));

        var updatedEntities = Generate.Updates(entities);

        this.Connection.UpdateEntities(updatedEntities, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enums are stored as strings:
        this.Connection.QueryScalars<String>(
                $"""
                 SELECT Enum
                 FROM   EntityWithEnumStoredAsString
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(updatedEntities.Select(a => a.Enum.ToString()));
    }

    [Fact]
    public void UpdateEntities_ShouldIgnorePropertiesDenotedWithNotMappedAttribute()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());
        var updatedEntities = Generate.Updates(entities);
        updatedEntities.ForEach(a => a.NotMappedProperty = "Not Mapped Value");

        Invoking(() =>
                this.Connection.UpdateEntities(
                    updatedEntities,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            // UpdateEntities would throw if it tried to update the property NotMappedProperty, because no such column
            // exists in the Entity table.
            .Should().NotThrow();
    }

    [Fact]
    public void UpdateEntities_ShouldReturnNumberOfAffectedRows()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());
        var updatedEntities = Generate.Updates(entities);

        this.Connection.UpdateEntities(updatedEntities, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(entities.Count);

        var nonExistentEntities = Generate.Entities(Generate.SmallNumber());

        this.Connection.UpdateEntities(nonExistentEntities, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(0);
    }

    [Fact]
    public void UpdateEntities_ShouldUpdateDataFromProperties()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();
        var updatedEntities = Generate.Updates(entities);

        this.Connection.UpdateEntities(updatedEntities, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.QueryEntities<Entity>(
                $"""
                 SELECT *
                 FROM   Entity
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(updatedEntities);
    }

    [Fact]
    public void UpdateEntities_Transaction_ShouldUseTransaction()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());

        using (var transaction = this.Connection.BeginTransaction())
        {
            var updatedEntities = Generate.Updates(entities);

            this.Connection.UpdateEntities(
                    updatedEntities,
                    transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                )
                .Should().Be(entities.Count);

            this.Connection.QueryEntities<Entity>("SELECT * FROM Entity", transaction)
                .Should().BeEquivalentTo(updatedEntities);

            transaction.Rollback();
        }

        this.Connection.QueryEntities<Entity>("SELECT * FROM Entity")
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task UpdateEntitiesAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());
        var updatedEntities = Generate.Updates(entities);
        var entityIds = entities.Select(a => a.Id).ToList();

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.UpdateEntitiesAsync(updatedEntities, cancellationToken: cancellationToken)
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entities should not have been updated.
        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"SELECT * FROM Entity WHERE Id IN (SELECT Value FROM {TemporaryTable(entityIds)})",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public Task UpdateEntitiesAsync_EntitiesHaveNoKeyProperty_ShouldThrow() =>
        Invoking(() =>
                this.Connection.UpdateEntitiesAsync(
                    [new EntityWithoutKeyProperty()],
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
    public async Task UpdateEntitiesAsync_EntitiesWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();
        var updatedEntities = Generate.Updates(entities);

        await this.Connection.UpdateEntitiesAsync(
            updatedEntities,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"""
                 SELECT *
                 FROM   Entity
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(updatedEntities);
    }

    [Fact]
    public async Task UpdateEntitiesAsync_EntitiesWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();
        var updatedEntities = Generate.Updates(entities);
        var updatedEntitiesWithTableAttribute = Generate.MapToEntitiesWithTableAttribute(updatedEntities);

        await this.Connection.UpdateEntitiesAsync(
            updatedEntitiesWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryEntitiesAsync<EntityWithTableAttribute>(
                $"""
                 SELECT *
                 FROM   Entity
                 WHERE  Id IN
                        (
                            SELECT  Value
                            FROM    {TemporaryTable(entityIds)}
                        )
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(updatedEntitiesWithTableAttribute);
    }

    [Fact]
    public async Task UpdateEntitiesAsync_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entities = Generate.EntitiesWithEnumStoredAsInteger(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();

        await this.Connection.InsertEntitiesAsync(entities, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enums are stored as integers:
        (await this.Connection.QueryScalarsAsync<Int32>(
                $"""
                 SELECT Enum
                 FROM   EntityWithEnumStoredAsInteger
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities.Select(a => (Int32)a.Enum));

        var updatedEntities = Generate.Updates(entities);

        await this.Connection.UpdateEntitiesAsync(updatedEntities,
            cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enums are stored as integers:
        (await this.Connection.QueryScalarsAsync<Int32>(
                $"""
                 SELECT Enum
                 FROM   EntityWithEnumStoredAsInteger
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(updatedEntities.Select(a => (Int32)a.Enum));
    }

    [Fact]
    public async Task UpdateEntitiesAsync_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entities = Generate.EntitiesWithEnumStoredAsString(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();

        await this.Connection.InsertEntitiesAsync(entities, cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enums are stored as strings:
        (await this.Connection.QueryScalarsAsync<String>(
                $"""
                 SELECT Enum
                 FROM   EntityWithEnumStoredAsString
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities.Select(a => a.Enum.ToString()));

        var updatedEntities = Generate.Updates(entities);

        await this.Connection.UpdateEntitiesAsync(updatedEntities,
            cancellationToken: TestContext.Current.CancellationToken);

        // Make sure the enums are stored as strings:
        (await this.Connection.QueryScalarsAsync<String>(
                $"""
                 SELECT Enum
                 FROM   EntityWithEnumStoredAsString
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(updatedEntities.Select(a => a.Enum.ToString()));
    }

    [Fact]
    public async Task UpdateEntitiesAsync_ShouldIgnorePropertiesDenotedWithNotMappedAttribute()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());
        var updatedEntities = Generate.Updates(entities);
        updatedEntities.ForEach(a => a.NotMappedProperty = "Not Mapped Value");

        await Invoking(() =>
                this.Connection.UpdateEntitiesAsync(
                    updatedEntities,
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            // UpdateEntitiesAsync would throw if it tried to update the property NotMappedProperty, because no such
            // column exists in the Entity table.
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateEntitiesAsync_ShouldReturnNumberOfAffectedRows()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());
        var updatedEntities = Generate.Updates(entities);

        (await this.Connection.UpdateEntitiesAsync(updatedEntities,
                cancellationToken: TestContext.Current.CancellationToken))
            .Should().Be(entities.Count);

        var nonExistentEntities = Generate.Entities(Generate.SmallNumber());

        (await this.Connection.UpdateEntitiesAsync(
                nonExistentEntities,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(0);
    }

    [Fact]
    public async Task UpdateEntitiesAsync_ShouldUpdateDataFromProperties()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());
        var entityIds = entities.Select(a => a.Id).ToList();
        var updatedEntities = Generate.Updates(entities);

        (await this.Connection.UpdateEntitiesAsync(
                updatedEntities,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(updatedEntities.Count);

        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"""
                 SELECT *
                 FROM   Entity
                 WHERE  Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(updatedEntities);
    }

    [Fact]
    public async Task UpdateEntitiesAsync_Transaction_ShouldUseTransaction()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        using (var transaction = this.Connection.BeginTransaction())
        {
            var updatedEntities = Generate.Updates(entities);

            (await this.Connection.UpdateEntitiesAsync(
                    updatedEntities,
                    transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ))
                .Should().Be(entities.Count);

            (await this.Connection.QueryEntitiesAsync<Entity>("SELECT * FROM Entity", transaction).ToListAsync())
                .Should().BeEquivalentTo(updatedEntities);

            await transaction.RollbackAsync();
        }

        (await this.Connection.QueryEntitiesAsync<Entity>("SELECT * FROM Entity").ToListAsync())
            .Should().BeEquivalentTo(entities);
    }
}
