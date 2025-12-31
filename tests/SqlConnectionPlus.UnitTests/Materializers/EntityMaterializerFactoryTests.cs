using System.Data.SqlTypes;
using System.Numerics;
using NSubstitute.ExceptionExtensions;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Materializers;

public class EntityMaterializerFactoryTests : TestsBase
{
    [Fact]
    public void GetMaterializer_DataReaderFieldHasNoName_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.GetName(0).Returns(String.Empty);

        Invoking(() => EntityMaterializerFactory.GetMaterializer<Entity>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                "The 1st column returned by the SQL statement does not have a name. Make sure that all columns the " +
                "SQL statement returns have a name.*"
            );
    }

    [Fact]
    public void
        GetMaterializer_DataReaderFieldTypeNotCompatibleWithEntityPropertyType_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(2);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(Int32));

        Invoking(() => EntityMaterializerFactory.GetMaterializer<EntityWithCharProperty>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(Int32)} of the column 'Char' returned by the SQL statement does not match " +
                $"the property type {typeof(Char)} of the corresponding property of the type " +
                $"{typeof(EntityWithCharProperty)}.*"
            );
    }

    [Fact]
    public void GetMaterializer_DataReaderHasNoFields_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(0);

        Invoking(() => EntityMaterializerFactory.GetMaterializer<Entity>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage("The SQL statement did not return any columns.*");
    }

    [Fact]
    public void
        GetMaterializer_EntityHasNoMatchingPropertyForDataReaderField_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.GetName(0).Returns("NonExistingProperty");

        Invoking(() => EntityMaterializerFactory.GetMaterializer<Entity>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"Could not map the column 'NonExistingProperty' returned by the SQL statement to a property (with a " +
                $"public setter) of the type {typeof(Entity)}. Make sure the type has a corresponding property.*"
            );
    }

    [Fact]
    public void GetMaterializer_UnsupportedDataReaderFieldType_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Id");
        dataReader.GetFieldType(0).Returns(typeof(BigInteger));

        Invoking(() =>
                EntityMaterializerFactory.GetMaterializer<EntityWithUnsupportedPropertyType>(dataReader)
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(BigInteger)} of the column 'Id' returned by the SQL statement is not " +
                $"supported.*"
            );
    }

    [Fact]
    public void Materializer_DataRecordFieldNameMatchesEntityPropertyCaseInsensitively_ShouldMaterialize()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("id"); // lower-case
        dataReader.GetFieldType(0).Returns(typeof(Int64));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetInt64(0).Returns(789);

        var materializer = EntityMaterializerFactory.GetMaterializer<Entity>(dataReader);

        var entity = materializer(dataReader);

        entity.Id
            .Should().Be(789);
    }

    [Fact]
    public void Materializer_EnumEntityProperty_DataRecordFieldContainsInteger_ShouldConvertToEnumMember()
    {
        var dataReader = Substitute.For<IDataReader>();

        var enumValue = Generate.Enum();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Enum");
        dataReader.GetFieldType(0).Returns(typeof(Int32));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetInt32(0).Returns((Int32)enumValue);

        var materializer = EntityMaterializerFactory.GetMaterializer<EntityWithEnumProperty>(dataReader);

        var entity = materializer(dataReader);

        entity.Enum
            .Should().Be(enumValue);
    }

    [Fact]
    public void
        Materializer_EnumEntityProperty_DataRecordFieldContainsIntegerNotMatchingAnyEnumMemberValue_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Enum");
        dataReader.GetFieldType(0).Returns(typeof(Int32));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetInt32(0).Returns(999);

        var materializer = EntityMaterializerFactory.GetMaterializer<EntityWithEnumProperty>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Enum' returned by the SQL statement contains a value that could not be converted to " +
                $"the enum type {typeof(TestEnum)} of the corresponding property of the type " +
                $"{typeof(EntityWithEnumProperty)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. That value does not match any of the values of the enum's members.*"
            );
    }

    [Fact]
    public void Materializer_EnumEntityProperty_DataRecordFieldContainsString_ShouldConvertToEnumMember()
    {
        var dataReader = Substitute.For<IDataReader>();

        var enumValue = Generate.Enum();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Enum");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns(enumValue.ToString());

        var materializer = EntityMaterializerFactory.GetMaterializer<EntityWithEnumProperty>(dataReader);

        var entity = materializer(dataReader);

        entity.Enum
            .Should().Be(enumValue);
    }

    [Fact]
    public void Materializer_EnumEntityProperty_DataRecordFieldContainsStringNotMatchingAnyEnumMemberName_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Enum");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns("NonExistent");

        var materializer = EntityMaterializerFactory.GetMaterializer<EntityWithEnumProperty>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Enum' returned by the SQL statement contains a value that could not be converted to " +
                $"the enum type {typeof(TestEnum)} of the corresponding property of the type " +
                $"{typeof(EntityWithEnumProperty)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'NonExistent' to an enum member of the type {typeof(TestEnum)}. That " +
                $"string does not match any of the names of the enum's members.*"
            );
    }

    [Fact]
    public void
        Materializer_NonNullableCharEntityProperty_DataRecordFieldContainsStringWithLengthNotOne_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns(String.Empty);

        var materializer = EntityMaterializerFactory.GetMaterializer<EntityWithCharProperty>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string '', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding property of the type " +
                $"{typeof(EntityWithCharProperty)}. The string must be exactly one character long.*"
            );

        dataReader.GetString(0).Returns("ab");

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string 'ab', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding property of the type " +
                $"{typeof(EntityWithCharProperty)}. The string must be exactly one character long.*"
            );
    }

    [Fact]
    public void
        Materializer_NonNullableCharEntityProperty_DataRecordFieldContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns("X");

        var materializer = EntityMaterializerFactory.GetMaterializer<EntityWithCharProperty>(dataReader);

        var entity = materializer(dataReader);

        entity.Char
            .Should().Be('X');
    }

    [Fact]
    public void
        Materializer_NonNullableEntityProperty_DataRecordFieldContainsNull_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Id");
        dataReader.GetFieldType(0).Returns(typeof(Int64));
        dataReader.IsDBNull(0).Returns(true);

        var materializer = EntityMaterializerFactory.GetMaterializer<Entity>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Id' returned by the SQL statement contains a " +
                $"NULL value, but the corresponding property of the type {typeof(Entity)} is non-nullable.*"
            );
    }

    [Fact]
    public void
        Materializer_NullableCharEntityProperty_DataRecordFieldContainsStringWithLengthNotOne_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns(String.Empty);

        var materializer = EntityMaterializerFactory.GetMaterializer<EntityWithNullableCharProperty>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string '', which could not be " +
                $"converted to the type {typeof(Char?)} of the corresponding property of the type " +
                $"{typeof(EntityWithNullableCharProperty)}. The string must be exactly one character long.*"
            );

        dataReader.GetString(0).Returns("ab");

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string 'ab', which could not be " +
                $"converted to the type {typeof(Char?)} of the corresponding property of the type " +
                $"{typeof(EntityWithNullableCharProperty)}. The string must be exactly one character long.*"
            );
    }

    [Fact]
    public void
        Materializer_NullableCharEntityProperty_DataRecordFieldContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns("X");

        var materializer = EntityMaterializerFactory
            .GetMaterializer<EntityWithNullableCharProperty>(dataReader);

        var entity = materializer(dataReader);

        entity.Char
            .Should().Be('X');
    }

    [Fact]
    public void Materializer_NullableEntityProperty_DataRecordFieldContainsNull_ShouldMaterializeNull()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Value");
        dataReader.GetFieldType(0).Returns(typeof(Int32));
        dataReader.IsDBNull(0).Returns(true);
        dataReader.GetInt32(0).Throws(new SqlNullValueException());

        var materializer = EntityMaterializerFactory
            .GetMaterializer<EntityWithNullableProperty>(dataReader);

        var entity = Invoking(() => materializer(dataReader))
            .Should().NotThrow().Subject;

        entity.Value
            .Should().BeNull();
    }

    [Fact]
    public void Materializer_ShouldMaterializeBinaryData()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("BinaryData");
        dataReader.GetFieldType(0).Returns(typeof(Byte[]));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetValue(0).Returns(new Byte[] { 1, 2, 3 });

        var materializer = EntityMaterializerFactory.GetMaterializer<EntityWithBinaryProperty>(dataReader);

        var entity = materializer(dataReader);

        entity.BinaryData
            .Should().BeEquivalentTo(new Byte[] { 1, 2, 3 });
    }

    [Fact]
    public void Materializer_ShouldMaterializeDataRecordToEntity()
    {
        var entity = Generate.Entity();

        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(16);

        dataReader.GetName(0).Returns("Id");
        dataReader.GetFieldType(0).Returns(typeof(Int64));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetInt64(0).Returns(entity.Id);

        dataReader.GetName(1).Returns("BooleanValue");
        dataReader.GetFieldType(1).Returns(typeof(Boolean));
        dataReader.IsDBNull(1).Returns(false);
        dataReader.GetBoolean(1).Returns(entity.BooleanValue);

        dataReader.GetName(2).Returns("ByteValue");
        dataReader.GetFieldType(2).Returns(typeof(Byte));
        dataReader.IsDBNull(2).Returns(false);
        dataReader.GetByte(2).Returns(entity.ByteValue);

        dataReader.GetName(3).Returns("CharValue");
        dataReader.GetFieldType(3).Returns(typeof(String));
        dataReader.IsDBNull(3).Returns(false);
        dataReader.GetString(3).Returns(entity.CharValue.ToString());

        dataReader.GetName(4).Returns("DateTimeOffsetValue");
        dataReader.GetFieldType(4).Returns(typeof(DateTimeOffset));
        dataReader.IsDBNull(4).Returns(false);
        dataReader.GetValue(4).Returns(entity.DateTimeOffsetValue);

        dataReader.GetName(5).Returns("DateTimeValue");
        dataReader.GetFieldType(5).Returns(typeof(DateTime));
        dataReader.IsDBNull(5).Returns(false);
        dataReader.GetDateTime(5).Returns(entity.DateTimeValue);

        dataReader.GetName(6).Returns("DecimalValue");
        dataReader.GetFieldType(6).Returns(typeof(Decimal));
        dataReader.IsDBNull(6).Returns(false);
        dataReader.GetDecimal(6).Returns(entity.DecimalValue);

        dataReader.GetName(7).Returns("DoubleValue");
        dataReader.GetFieldType(7).Returns(typeof(Double));
        dataReader.IsDBNull(7).Returns(false);
        dataReader.GetDouble(7).Returns(entity.DoubleValue);

        dataReader.GetName(8).Returns("EnumValue");
        dataReader.GetFieldType(8).Returns(typeof(String));
        dataReader.IsDBNull(8).Returns(false);
        dataReader.GetString(8).Returns(entity.EnumValue.ToString());

        dataReader.GetName(9).Returns("GuidValue");
        dataReader.GetFieldType(9).Returns(typeof(Guid));
        dataReader.IsDBNull(9).Returns(false);
        dataReader.GetGuid(9).Returns(entity.GuidValue);

        dataReader.GetName(10).Returns("Int16Value");
        dataReader.GetFieldType(10).Returns(typeof(Int16));
        dataReader.IsDBNull(10).Returns(false);
        dataReader.GetInt16(10).Returns(entity.Int16Value);

        dataReader.GetName(11).Returns("Int32Value");
        dataReader.GetFieldType(11).Returns(typeof(Int32));
        dataReader.IsDBNull(11).Returns(false);
        dataReader.GetInt32(11).Returns(entity.Int32Value);

        dataReader.GetName(12).Returns("Int64Value");
        dataReader.GetFieldType(12).Returns(typeof(Int64));
        dataReader.IsDBNull(12).Returns(false);
        dataReader.GetInt64(12).Returns(entity.Int64Value);

        dataReader.GetName(13).Returns("SingleValue");
        dataReader.GetFieldType(13).Returns(typeof(Single));
        dataReader.IsDBNull(13).Returns(false);
        dataReader.GetFloat(13).Returns(entity.SingleValue);

        dataReader.GetName(14).Returns("StringValue");
        dataReader.GetFieldType(14).Returns(typeof(String));
        dataReader.IsDBNull(14).Returns(false);
        dataReader.GetString(14).Returns(entity.StringValue);

        dataReader.GetName(15).Returns("TimeSpanValue");
        dataReader.GetFieldType(15).Returns(typeof(TimeSpan));
        dataReader.IsDBNull(15).Returns(false);
        dataReader.GetValue(15).Returns(entity.TimeSpanValue);

        var materializer = EntityMaterializerFactory.GetMaterializer<Entity>(dataReader);

        var materializedEntity = materializer(dataReader);

        materializedEntity
            .Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            EntityMaterializerFactory.GetMaterializer<Entity>(Substitute.For<IDataReader>()));
}
