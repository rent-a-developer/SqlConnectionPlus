using Fasterflect;
using RentADeveloper.SqlConnectionPlus.Entities;
using RentADeveloper.SqlConnectionPlus.SqlCommands;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.SqlCommands;

public class EntitySqlCommandFactoryTests : TestsBase
{
    /// <inheritdoc />
    public EntitySqlCommandFactoryTests()
    {
        var keyPropertyGetter = Substitute.For<MemberGetter>();
        var propertyGetters = Enumerable.Range(0, 4).Select(_ => Substitute.For<MemberGetter>()).ToArray();

        this.entityTypeMetadata = new(
            InsertSql: "Insert SQL",
            UpdateSql: "Update SQL",
            DeleteSql: "Delete SQL",
            TableName: "Entity",
            KeyPropertyName: "KeyPropertyName",
            KeyPropertyType: typeof(Int64),
            KeyPropertyGetter: keyPropertyGetter,
            PropertyNames: ["Property1", "Property2", "Property3", "Property4"],
            PropertyGetters: propertyGetters,
            IsPropertyTypeByteArray: [true, false, false, false],
            IsPropertyTypeDateTimeOrNullableDateTime: [false, true, false, false],
            IsPropertyTypeEnumOrNullableEnum: [false, false, true, false]
        );
    }

    [Fact]
    public void CreateInsertEntityCommand_ShouldCreateCommandWithInformationFromMetadata()
    {
        var (command, parameters) =
            EntitySqlCommandFactory.CreateInsertEntityCommand(new(), null, this.entityTypeMetadata);

        command.CommandText
            .Should().Be(this.entityTypeMetadata.InsertSql);

        parameters
            .Should().HaveCount(this.entityTypeMetadata.PropertyNames.Length);

        command.Parameters
            .Should().BeEquivalentTo(parameters);

        parameters.Select(a => a.ParameterName)
            .Should().BeEquivalentTo(this.entityTypeMetadata.PropertyNames);

        parameters[0].DbType
            .Should().Be(DbType.Binary);

        parameters[1].DbType
            .Should().Be(DbType.DateTime2);
    }

    [Fact]
    public void CreateUpdateEntityCommand_ShouldCreateCommandWithInformationFromMetadata()
    {
        var (command, parameters) =
            EntitySqlCommandFactory.CreateUpdateEntityCommand(new(), null, this.entityTypeMetadata);

        command.CommandText
            .Should().Be(this.entityTypeMetadata.UpdateSql);

        parameters
            .Should().HaveCount(this.entityTypeMetadata.PropertyNames.Length);

        command.Parameters
            .Should().BeEquivalentTo(parameters);

        parameters.Select(a => a.ParameterName)
            .Should().BeEquivalentTo(this.entityTypeMetadata.PropertyNames);

        parameters[0].DbType
            .Should().Be(DbType.Binary);

        parameters[1].DbType
            .Should().Be(DbType.DateTime2);
    }

    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() =>
            EntitySqlCommandFactory.CreateInsertEntityCommand(new(), null, this.entityTypeMetadata)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            EntitySqlCommandFactory.CreateUpdateEntityCommand(new(), null, this.entityTypeMetadata)
        );
    }

    private readonly EntityTypeMetadata entityTypeMetadata;
}
