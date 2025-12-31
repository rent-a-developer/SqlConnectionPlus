using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_DeleteEntityTests : DatabaseTestsBase
{
    [Fact]
    public void DeleteEntity_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entityToDelete = this.InsertNewEntity();

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        Invoking(() => this.Connection.DeleteEntity(entityToDelete, cancellationToken: cancellationToken))
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entity should still exist.
        this.ExistsEntityById(entityToDelete.Id)
            .Should().BeTrue();
    }

    [Fact]
    public void DeleteEntity_EntityHasNoKeyProperty_ShouldThrow()
    {
        var entityWithoutKeyProperty = new EntityWithoutKeyProperty();

        Invoking(() => this.Connection.DeleteEntity(entityWithoutKeyProperty))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"Could not get the key property of the type {typeof(EntityWithoutKeyProperty)}. " +
                $"Make sure that one property (with a public getter) of that type is denoted with a " +
                $"{typeof(KeyAttribute)}."
            );
    }

    [Fact]
    public void DeleteEntity_EntityWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entityToDelete = this.InsertNewEntity();

        this.Connection.DeleteEntity(entityToDelete, cancellationToken: TestContext.Current.CancellationToken);

        this.ExistsEntityById(entityToDelete.Id)
            .Should().BeFalse();
    }

    [Fact]
    public void DeleteEntity_EntityWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entity = this.InsertNewEntity();
        var entityWithTableAttribute = Generate.MapToEntityWithTableAttribute(entity);

        this.Connection.DeleteEntity(
            entityWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.ExistsEntityById(entityWithTableAttribute.Id)
            .Should().BeFalse();
    }

    [Fact]
    public void DeleteEntity_ShouldReturnNumberOfAffectedRows()
    {
        var entityToDelete = this.InsertNewEntity();

        this.Connection.DeleteEntity(entityToDelete, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(1);

        this.Connection.DeleteEntity(entityToDelete, cancellationToken: TestContext.Current.CancellationToken)
            .Should().Be(0);
    }

    [Fact]
    public void DeleteEntity_Transaction_ShouldUseTransaction()
    {
        var entityToDelete = this.InsertNewEntity();

        using (var transaction = this.Connection.BeginTransaction())
        {
            this.Connection.DeleteEntity(
                entityToDelete,
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            this.ExistsEntityById(entityToDelete.Id, transaction)
                .Should().BeFalse();

            transaction.Rollback();
        }

        this.ExistsEntityById(entityToDelete.Id)
            .Should().BeTrue();
    }

    [Fact]
    public async Task DeleteEntityAsync_CancellationToken_ShouldCancelOperationIfCancellationIsRequested()
    {
        var entityToDelete = await this.InsertNewEntityAsync();

        var cancellationToken = CreateCancellationTokenThatIsCancelledAfter100Milliseconds();

        this.SqlCommandFactory.DelayNextSqlCommand = true;

        await Invoking(() =>
                this.Connection.DeleteEntityAsync(entityToDelete, cancellationToken: cancellationToken)
            )
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);

        // Since the operation was cancelled, the entity should still exist.
        (await this.ExistsEntityByIdAsync(entityToDelete.Id))
            .Should().BeTrue();
    }

    [Fact]
    public Task DeleteEntityAsync_EntityHasNoKeyProperty_ShouldThrow()
    {
        var entityWithoutKey = new EntityWithoutKeyProperty();

        return Invoking(() => this.Connection.DeleteEntityAsync(entityWithoutKey))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                $"Could not get the key property of the type {typeof(EntityWithoutKeyProperty)}. " +
                $"Make sure that one property (with a public getter) of that type is denoted with a " +
                $"{typeof(KeyAttribute)}."
            );
    }

    [Fact]
    public async Task DeleteEntityAsync_EntityWithoutTableAttribute_ShouldUseEntityTypeNameAsTableName()
    {
        var entityToDelete = await this.InsertNewEntityAsync();

        await this.Connection.DeleteEntityAsync(
            entityToDelete,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsEntityByIdAsync(entityToDelete.Id))
            .Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEntityAsync_EntityWithTableAttribute_ShouldUseTableNameFromAttribute()
    {
        var entity = await this.InsertNewEntityAsync();
        var entityWithTableAttribute = Generate.MapToEntityWithTableAttribute(entity);

        await this.Connection.DeleteEntityAsync(
            entityWithTableAttribute,
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsEntityByIdAsync(entityWithTableAttribute.Id))
            .Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEntityAsync_ShouldReturnNumberOfAffectedRows()
    {
        var entityToDelete = await this.InsertNewEntityAsync();

        (await this.Connection.DeleteEntityAsync(
                entityToDelete,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(1);

        (await this.Connection.DeleteEntityAsync(
                entityToDelete,
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().Be(0);
    }

    [Fact]
    public async Task DeleteEntityAsync_Transaction_ShouldUseTransaction()
    {
        var entityToDelete = await this.InsertNewEntityAsync();

        using (var transaction = this.Connection.BeginTransaction())
        {
            await this.Connection.DeleteEntityAsync(
                entityToDelete,
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            );

            (await this.ExistsEntityByIdAsync(entityToDelete.Id, transaction))
                .Should().BeFalse();

            await transaction.RollbackAsync();
        }

        (await this.ExistsEntityByIdAsync(entityToDelete.Id))
            .Should().BeTrue();
    }
}
