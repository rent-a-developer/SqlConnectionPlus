using RentADeveloper.SqlConnectionPlus.Entities;
using RentADeveloper.SqlConnectionPlus.SqlCommands;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.SqlCommands;

public class EntitySqlCommandFactoryTests : DatabaseTestsBase
{
    [Fact]
    public void CreateInsertEntityCommand_Transaction_ShouldAssignTransaction()
    {
        var transaction = this.Connection.BeginTransaction();

        var (resultCommand, _) = EntitySqlCommandFactory.CreateInsertEntityCommand(
            this.Connection,
            transaction,
            EntityHelper.GetEntityTypeMetadata<Entity>()
        );

        resultCommand.Transaction
            .Should().BeSameAs(transaction);
    }

    [Fact]
    public void CreateUpdateEntityCommand_Transaction_ShouldAssignTransaction()
    {
        var transaction = this.Connection.BeginTransaction();

        var (resultCommand, _) = EntitySqlCommandFactory.CreateUpdateEntityCommand(
            this.Connection,
            transaction,
            EntityHelper.GetEntityTypeMetadata<Entity>()
        );

        resultCommand.Transaction
            .Should().BeSameAs(transaction);
    }
}
