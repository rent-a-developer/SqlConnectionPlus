using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_DeleteEntitiesTests : DatabaseTestsBase
{
    [Fact]
    public void DeleteEntities_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entitiesToDelete = this.InsertNewEntities(Generate.SmallNumber());

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() => this.Connection.DeleteEntities(entitiesToDelete, cancellationToken: cancellationToken))
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        foreach (var entity in entitiesToDelete)
        {
            // Since the operation was cancelled, the entities should still exist.
            this.ExistsEntityById(entity.Id)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void DeleteEntities_EntitiesHaveNoKeyProperty_ShouldThrow()
    {
        var entityWithoutKeyProperty = new EntityWithoutKeyProperty();

        Invoking(() => this.Connection.DeleteEntities([entityWithoutKeyProperty]))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"Could not get the key property of the type {typeof(EntityWithoutKeyProperty)}. " +
                $"Make sure that one property (with a public getter) of that type is denoted with " +
                $"a {typeof(KeyAttribute)}."
            );
    }

    [Fact]
    public void DeleteEntities_EntitiesWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entitiesToDelete = this.InsertNewEntities(Generate.SmallNumber());

        this.Connection.DeleteEntities(entitiesToDelete, cancellationToken: TestContext.Current.CancellationToken);

        foreach (var entity in entitiesToDelete)
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeFalse();
        }
    }

    [Fact]
    public void DeleteEntities_EntitiesWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entities = this.InsertNewEntities(Generate.SmallNumber());
        var entitiesWithTableAttribute = Generate.MapToEntitiesWithTableAttribute(entities);

        this.Connection.DeleteEntities(
            entitiesWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entityWithTableAttribute in entitiesWithTableAttribute)
        {
            this.ExistsEntityById(entityWithTableAttribute.Id)
                .Should().BeFalse();
        }
    }

    [Fact]
    public void DeleteEntities_ShouldReturnNumberOfAffectedRows()
    {
        var entitiesToDelete = this.InsertNewEntities(Generate.SmallNumber());

        this.Connection.DeleteEntities(entitiesToDelete, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(entitiesToDelete.Count);

        this.Connection.DeleteEntities(entitiesToDelete, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(0);
    }

    [Fact]
    public void DeleteEntities_Transaction_ShouldUseTransaction()
    {
        var entitiesToDelete = this.InsertNewEntities(Generate.SmallNumber());

        using (var transaction = this.Connection.BeginTransaction())
        {
            this.Connection.DeleteEntities(
                entitiesToDelete,
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            foreach (var entity in entitiesToDelete)
            {
                this.ExistsEntityById(entity.Id, transaction)
                    .Should().BeFalse();
            }

            transaction.Rollback();
        }

        foreach (var entity in entitiesToDelete)
        {
            this.ExistsEntityById(entity.Id)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task DeleteEntitiesAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entitiesToDelete = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.DeleteEntitiesAsync(entitiesToDelete, cancellationToken: cancellationToken)
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        foreach (var entity in entitiesToDelete)
        {
            // Since the operation was cancelled, the entities should still exist.
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeTrue();
        }
    }

    [Fact]
    public Task DeleteEntitiesAsync_EntitiesHaveNoKeyProperty_ShouldThrow()
    {
        var entityWithoutKey = new EntityWithoutKeyProperty();

        return Invoking(() => this.Connection.DeleteEntitiesAsync([entityWithoutKey]))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"Could not get the key property of the type {typeof(EntityWithoutKeyProperty)}. " +
                $"Make sure that one property (with a public getter) of that type is denoted with a " +
                $"{typeof(KeyAttribute)}."
            );
    }

    [Fact]
    public async Task DeleteEntitiesAsync_EntitiesWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entitiesToDelete = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        await this.Connection.DeleteEntitiesAsync(
            entitiesToDelete,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entitiesToDelete)
        {
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteEntitiesAsync_EntitiesWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entities = await this.InsertNewEntitiesAsync(Generate.SmallNumber());
        var entitiesWithTableAttribute = Generate.MapToEntitiesWithTableAttribute(entities);

        await this.Connection.DeleteEntitiesAsync(
            entitiesWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entityWithTableAttribute in entitiesWithTableAttribute)
        {
            (await this.ExistsEntityByIdAsync(entityWithTableAttribute.Id))
                .Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteEntitiesAsync_ShouldReturnNumberOfAffectedRows()
    {
        var entitiesToDelete = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        (await this.Connection.DeleteEntitiesAsync(
                entitiesToDelete,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(entitiesToDelete.Count);

        (await this.Connection.DeleteEntitiesAsync(
                entitiesToDelete,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(0);
    }

    [Fact]
    public async Task DeleteEntitiesAsync_Transaction_ShouldUseTransaction()
    {
        var entitiesToDelete = await this.InsertNewEntitiesAsync(Generate.SmallNumber());

        using (var transaction = this.Connection.BeginTransaction())
        {
            await this.Connection.DeleteEntitiesAsync(
                entitiesToDelete,
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            foreach (var entity in entitiesToDelete)
            {
                (await this.ExistsEntityByIdAsync(entity.Id, transaction))
                    .Should().BeFalse();
            }

            await transaction.RollbackAsync();
        }

        foreach (var entity in entitiesToDelete)
        {
            (await this.ExistsEntityByIdAsync(entity.Id))
                .Should().BeTrue();
        }
    }
}
