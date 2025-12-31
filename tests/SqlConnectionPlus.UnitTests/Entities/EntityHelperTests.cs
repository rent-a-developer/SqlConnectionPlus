using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using RentADeveloper.SqlConnectionPlus.Entities;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

#pragma warning disable IDE0200

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Entities;

public class EntityHelperTests : TestsBase
{
    [Fact]
    public void GetEntityKeyProperty_NoKeyAttributePresent_ShouldThrow() =>
        Invoking(() => EntityHelper.GetEntityKeyProperty<EntityWithCharProperty>())
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"Could not get the key property of the type {typeof(EntityWithCharProperty)}. " +
                $"Make sure that one property (with a public getter) of that type is denoted with a " +
                $"{typeof(KeyAttribute)}."
            );

    [Fact]
    public void GetEntityKeyProperty_ShouldGetPropertyDenotedWithKeyAttribute() =>
        EntityHelper.GetEntityKeyProperty<Entity>()
            .Should().BeSameAs(typeof(Entity).GetProperty(nameof(Entity.Id)));

    [Fact]
    public void GetEntityReadableProperties_ShouldGetReadableProperties() =>
        EntityHelper.GetEntityReadableProperties(typeof(Entity))
            .Should().BeEquivalentTo(
                typeof(Entity)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(a => a.GetMethod?.IsPublic == true && a.GetCustomAttribute<NotMappedAttribute>() is null)
            );

    [Fact]
    public void GetEntityReadablePropertyNames_ShouldGetNamesOfReadableProperties() =>
        EntityHelper.GetEntityReadablePropertyNames(typeof(Entity))
            .Should().BeEquivalentTo(
                typeof(Entity)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(a => a.GetMethod?.IsPublic == true && a.GetCustomAttribute<NotMappedAttribute>() is null)
                    .Select(a => a.Name)
            );

    [Fact]
    public void GetEntityTypeMetadata_ShouldGetMetadataForEntityType()
    {
        var entity = Generate.Entity();

        var entityReadableProperties = EntityHelper.GetEntityReadableProperties(typeof(Entity));

        var metadata = EntityHelper.GetEntityTypeMetadata<Entity>();

        metadata.InsertSql
            .Should().Be(
                """
                INSERT INTO [Entity]
                ([BooleanValue], [ByteValue], [CharValue], [DateTimeOffsetValue], [DateTimeValue], [DecimalValue], [DoubleValue], [EnumValue], [GuidValue], [Id], [Int16Value], [Int32Value], [Int64Value], [SingleValue], [StringValue], [TimeSpanValue])
                VALUES
                (@BooleanValue, @ByteValue, @CharValue, @DateTimeOffsetValue, @DateTimeValue, @DecimalValue, @DoubleValue, @EnumValue, @GuidValue, @Id, @Int16Value, @Int32Value, @Int64Value, @SingleValue, @StringValue, @TimeSpanValue)

                """
            );

        metadata.UpdateSql
            .Should().Be(
                """
                UPDATE [Entity]
                SET [BooleanValue] = @BooleanValue, [ByteValue] = @ByteValue, [CharValue] = @CharValue, [DateTimeOffsetValue] = @DateTimeOffsetValue, [DateTimeValue] = @DateTimeValue, [DecimalValue] = @DecimalValue, [DoubleValue] = @DoubleValue, [EnumValue] = @EnumValue, [GuidValue] = @GuidValue, [Id] = @Id, [Int16Value] = @Int16Value, [Int32Value] = @Int32Value, [Int64Value] = @Int64Value, [SingleValue] = @SingleValue, [StringValue] = @StringValue, [TimeSpanValue] = @TimeSpanValue
                WHERE [Id] = @Id

                """);

        metadata.DeleteSql
            .Should().Be("DELETE FROM [Entity] WHERE [Id] = @Key");

        metadata.TableName
            .Should().Be(nameof(Entity));

        metadata.KeyPropertyName
            .Should().Be(nameof(Entity.Id));

        metadata.KeyPropertyType
            .Should().Be(typeof(Int64));

        metadata.KeyPropertyGetter
            .Should().NotBeNull();

        metadata.KeyPropertyGetter(entity)
            .Should().Be(entity.Id);

        metadata.PropertyNames
            .Should().BeEquivalentTo(entityReadableProperties.Select(a => a.Name));

        metadata.PropertyGetters
            .Should().HaveSameCount(metadata.PropertyNames);

        for (var i = 0; i < metadata.PropertyNames.Length; i++)
        {
            var propertyGetter = metadata.PropertyGetters[i];
            var propertyName = metadata.PropertyNames[i];

            propertyGetter(entity)
                .Should().Be(typeof(Entity).GetProperty(propertyName)!.GetValue(entity));
        }

        metadata.IsPropertyTypeByteArray
            .Should().HaveSameCount(metadata.PropertyNames);

        metadata.IsPropertyTypeByteArray
            .Should().BeEquivalentTo(
                entityReadableProperties
                    .Select(a => a.PropertyType == typeof(Byte[]))
            );

        metadata.IsPropertyTypeDateTimeOrNullableDateTime
            .Should().HaveSameCount(metadata.PropertyNames);

        metadata.IsPropertyTypeDateTimeOrNullableDateTime
            .Should().BeEquivalentTo(
                entityReadableProperties
                    .Select(a => a.PropertyType == typeof(DateTime) || a.PropertyType == typeof(DateTime?))
            );

        metadata.IsPropertyTypeEnumOrNullableEnum
            .Should().HaveSameCount(metadata.PropertyNames);

        metadata.IsPropertyTypeEnumOrNullableEnum
            .Should().BeEquivalentTo(
                entityReadableProperties
                    .Select(a =>
                        a.PropertyType.IsEnum || Nullable.GetUnderlyingType(a.PropertyType) is { IsEnum: true })
            );
    }

    [Fact]
    public void GetEntityWriteableProperties_ShouldGetWriteableProperties() =>
        EntityHelper.GetEntityWriteableProperties(typeof(Entity))
            .Should().BeEquivalentTo(
                typeof(Entity)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(a => a.SetMethod?.IsPublic == true && a.GetCustomAttribute<NotMappedAttribute>() is null)
            );

    [Fact]
    public void GetTableName_EntityHasNoTableAttribute_ShouldReturnTypeName() =>
        EntityHelper.GetEntityTableName<Entity>()
            .Should().Be("Entity");

    [Fact]
    public void GetTableName_EntityHasTableAttribute_ShouldReturnTableNameFromAttribute() =>
        EntityHelper.GetEntityTableName<EntityWithTableAttribute>()
            .Should().Be("Entity");

    [Fact]
    public void
        PopulateSqlParametersFromEntityProperties_EnumProperty_EnumSerializationModeIsIntegers_ShouldUseSerializeEnumToInteger()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var entity = Generate.EntityWithEnumStoredAsInteger();
        var metadata = EntityHelper.GetEntityTypeMetadata<EntityWithEnumStoredAsInteger>();

        SqlParameter[] parameters = [new(), new()];

        EntityHelper.PopulateSqlParametersFromEntityProperties(metadata, parameters, entity);

        parameters[0].Value
            .Should().Be((Int32)entity.Enum);

        parameters[1].Value
            .Should().Be(entity.Id);
    }

    [Fact]
    public void
        PopulateSqlParametersFromEntityProperties_EnumProperty_EnumSerializationModeIsStrings_ShouldUseSerializeEnumToString()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entity = Generate.EntityWithEnumStoredAsString();
        var metadata = EntityHelper.GetEntityTypeMetadata<EntityWithEnumStoredAsString>();

        SqlParameter[] parameters = [new(), new()];

        EntityHelper.PopulateSqlParametersFromEntityProperties(metadata, parameters, entity);

        parameters[0].Value
            .Should().Be(entity.Enum.ToString());

        parameters[1].Value
            .Should().Be(entity.Id);
    }

    [Fact]
    public void PopulateSqlParametersFromEntityProperties_ShouldPopulateParameters()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var entity = Generate.Entity();
        var metadata = EntityHelper.GetEntityTypeMetadata<Entity>();

        var parameters =
            EntityHelper
                .GetEntityReadableProperties(typeof(Entity))
                .Select(_ => new SqlParameter())
                .ToArray();

        EntityHelper.PopulateSqlParametersFromEntityProperties(metadata, parameters, entity);

        for (var i = 0; i < metadata.PropertyNames.Length; i++)
        {
            var propertyName = metadata.PropertyNames[i];
            var propertyValue = typeof(Entity).GetProperty(propertyName)!.GetValue(entity)!;

            if (propertyName == nameof(Entity.EnumValue))
            {
                parameters[i].Value
                    .Should().Be(propertyValue.ToString());
            }
            else
            {
                parameters[i].Value
                    .Should().Be(propertyValue);
            }
        }
    }

    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var metadata = EntityHelper.GetEntityTypeMetadata<Entity>();

        ArgumentNullGuardVerifier.Verify(() => EntityHelper.GetEntityReadableProperties(typeof(Entity)));
        ArgumentNullGuardVerifier.Verify(() => EntityHelper.GetEntityReadablePropertyNames(typeof(Entity)));
        ArgumentNullGuardVerifier.Verify(() => EntityHelper.GetEntityWriteableProperties(typeof(Entity)));
        ArgumentNullGuardVerifier.Verify(() =>
            EntityHelper.PopulateSqlParametersFromEntityProperties(
                metadata,
                Array.Empty<SqlParameter>(),
                new Entity()
            )
        );
    }
}
