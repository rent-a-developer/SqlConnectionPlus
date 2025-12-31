using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_InsertEntitiesTests : DatabaseTestsBase
{
    [Fact]
    public void InsertEntities_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() => this.Connection.InsertEntities(entities, cancellationToken: cancellationToken))
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entities should not have been inserted.
        foreach (var entity in entities)
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeFalse();
        }
    }

    [Fact]
    public void InsertEntities_EntitiesWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        this.Connection.InsertEntities(entities, cancellationToken: TestContext.Current.CancellationToken);

        foreach (var entity in entities)
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void InsertEntities_EntitiesWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entities = Generate.Entities(Generate.SmallNumber());
        var entitiesWithTableAttribute = Generate.MapToEntitiesWithTableAttribute(entities);

        this.Connection.InsertEntities(
            entitiesWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entityWithTableAttribute in entitiesWithTableAttribute)
        {
            this.ExistsEntityById(entityWithTableAttribute.Id)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void InsertEntities_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entities = Generate.EntitiesWithEnumStoredAsInteger(Generate.SmallNumber());

        this.Connection.InsertEntities(entities, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.QueryScalars<Int32>(
                $"SELECT Enum FROM EntityWithEnumStoredAsInteger",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities.Select(a => (Int32)a.Enum));
    }

    [Fact]
    public void InsertEntities_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entities = Generate.EntitiesWithEnumStoredAsString(Generate.SmallNumber());

        this.Connection.InsertEntities(entities, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.QueryScalars<String>(
                $"SELECT Enum FROM EntityWithEnumStoredAsString",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities.Select(a => a.Enum.ToString()));
    }

    [Fact]
    public void InsertEntities_ShouldIgnorePropertiesDenotedWithNotMappedAttribute()
    {
        var entity = Generate.Entity();
        entity.NotMappedProperty = "Not Mapped Value";

        Invoking(() =>
                this.Connection.InsertEntities(
                    [entity],
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            // InsertEntities would throw if it tried to insert the property NotMappedProperty, because no such column
            // exists in the Entity table.
            .Should().NotThrow();
    }

    [Fact]
    public void InsertEntities_ShouldInsertDataFromProperties()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        this.Connection.InsertEntities(entities, cancellationToken: TestContext.Current.CancellationToken);

        this.Connection.QueryEntities<Entity>(
                "SELECT * FROM Entity",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public void InsertEntities_ShouldReturnNumberOfAffectedRows()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        this.Connection.InsertEntities(entities, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(entities.Count);

        this.Connection.InsertEntities(Array.Empty<Entity>(), cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(0);
    }

    [Fact]
    public void InsertEntities_Transaction_ShouldUseTransaction()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        using (var transaction = this.Connection.BeginTransaction())
        {
            this.Connection.InsertEntities(
                    entities,
                    transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                )
                .Should().Be(entities.Count);

            foreach (var entity in entities)
            {
                this.ExistsEntityById(entity.Id, transaction)
                    .Should().BeTrue();
            }

            transaction.Rollback();
        }

        foreach (var entity in entities)
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeFalse();
        }
    }

    [Fact]
    public async Task InsertEntitiesAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.InsertEntitiesAsync(entities, cancellationToken: cancellationToken)
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entities should not have been inserted.
        foreach (var entityToInsert in entities)
        {
            (await this.ExistsEntityByIdAsync(entityToInsert.Id))
                .Should().BeFalse();
        }
    }

    [Fact]
    public async Task InsertEntitiesAsync_EntitiesWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        await this.Connection.InsertEntitiesAsync(
            entities,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entities)
        {
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task InsertEntitiesAsync_EntitiesWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entities = Generate.Entities(Generate.SmallNumber());
        var entitiesWithTableAttribute = Generate.MapToEntitiesWithTableAttribute(entities);

        await this.Connection.InsertEntitiesAsync(
            entitiesWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entityWithTableAttribute in entitiesWithTableAttribute)
        {
            (await this.ExistsEntityByIdAsync(entityWithTableAttribute.Id))
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task InsertEntitiesAsync_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entities = Generate.EntitiesWithEnumStoredAsInteger(Generate.SmallNumber());

        await this.Connection.InsertEntitiesAsync(entities, cancellationToken: TestContext.Current.CancellationToken);

        (await this.Connection.QueryScalarsAsync<Int32>(
                $"SELECT Enum FROM EntityWithEnumStoredAsInteger",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities.Select(a => (Int32)a.Enum));
    }

    [Fact]
    public async Task InsertEntitiesAsync_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entities = Generate.EntitiesWithEnumStoredAsString(Generate.SmallNumber());

        await this.Connection.InsertEntitiesAsync(entities, cancellationToken: TestContext.Current.CancellationToken);

        (await this.Connection.QueryScalarsAsync<String>(
                $"SELECT Enum FROM EntityWithEnumStoredAsString",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities.Select(a => a.Enum.ToString()));
    }

    [Fact]
    public async Task InsertEntitiesAsync_ShouldIgnorePropertiesDenotedWithNotMappedAttribute()
    {
        var entity = Generate.Entity();
        entity.NotMappedProperty = "Not Mapped Value";

        await Invoking(() =>
                this.Connection.InsertEntitiesAsync(
                    [entity],
                    cancellationToken: TestContext.Current.CancellationToken
                )
            )
            // InsertAsync would throw if it tried to insert the property NotMappedProperty, because no such column
            // exists in the Entity table.
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task InsertEntitiesAsync_ShouldInsertDataFromProperties()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        (await this.Connection.InsertEntitiesAsync(
                entities,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entities.Count);

        (await this.Connection.QueryEntitiesAsync<Entity>(
                $"""
                 SELECT     *
                 FROM       Entity
                 WHERE      Id IN (SELECT Value FROM {TemporaryTable(entities.Select(a => a.Id).ToList())})
                 """,
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task InsertEntitiesAsync_ShouldReturnNumberOfAffectedRows()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        (await this.Connection.InsertEntitiesAsync(
                entities,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entities.Count);

        (await this.Connection.InsertEntitiesAsync(
                Array.Empty<Entity>(),
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(0);
    }

    [Fact]
    public async Task InsertEntitiesAsync_Transaction_ShouldUseTransaction()
    {
        var entities = Generate.Entities(Generate.SmallNumber());

        using (var transaction = this.Connection.BeginTransaction())
        {
            (await this.Connection.InsertEntitiesAsync(
                    entities,
                    transaction,
                    cancellationToken: TestContext.Current.CancellationToken
                ))
                .Should().Be(entities.Count);

            foreach (var entity in entities)
            {
                (await this.ExistsEntityByIdAsync(entity.Id, transaction))
                    .Should().BeTrue();
            }

            await transaction.RollbackAsync();
        }

        foreach (var entity in entities)
        {
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeFalse();
        }
    }
}
