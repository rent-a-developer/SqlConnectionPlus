using System.Data.SqlTypes;
using System.Numerics;
using NSubstitute.ExceptionExtensions;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Materializers;

public class ValueTupleMaterializerFactoryTests : TestsBase
{
    [Fact]
    public void GetMaterializer_DataReaderFieldCountDoesNotMatchValueTupleFieldCount_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        Invoking(() => ValueTupleMaterializerFactory.GetMaterializer<(Int32, Int32)>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The SQL statement returned 1 column, but the value tuple type {typeof((Int32, Int32))} has 2 " +
                $"fields. Make sure that the SQL statement returns the same number of columns as the number of " +
                $"fields in the value tuple type.*"
            );
    }

    [Fact]
    public void GetMaterializer_DataReaderHasNoFields_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(0);

        Invoking(() => ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Int32>>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage("The SQL statement did not return any columns.*");
    }

    [Fact]
    public void GetMaterializer_DataReaderHasUnsupportedFieldType_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Value");
        dataReader.GetFieldType(0).Returns(typeof(BigInteger));

        Invoking(() => ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<BigInteger>>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(BigInteger)} of the column 'Value' returned by the SQL statement is not " +
                $"supported.*"
            );
    }

    [Fact]
    public void
        GetMaterializer_DataRecordFieldTypeNotCompatibleWithValueTupleFieldType_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Name");
        dataReader.GetFieldType(0).Returns(typeof(String));

        Invoking(() => ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<DateTime>>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(String)} of the column 'Name' returned by the SQL statement does not " +
                $"match the field type {typeof(DateTime)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<DateTime>)}.*"
            );
    }

    [Fact]
    public void GetMaterializer_ValueTupleTypeHasMoreThan7Fields_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        Invoking(() =>
                ValueTupleMaterializerFactory
                    .GetMaterializer<(Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32)>(dataReader)
            )
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The specified type {typeof((Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32))} is not a " +
                $"value tuple type or it is a value tuple type with more than 7 fields.*"
            );
    }

    [Fact]
    public void GetMaterializer_ValueTupleTypeIsNotAValueTupleType_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        Invoking(() => ValueTupleMaterializerFactory.GetMaterializer<NotAValueTuple>(dataReader))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The specified type {typeof(NotAValueTuple)} is not a value tuple type or it is a value tuple type " +
                $"with more than 7 fields.*"
            );
    }

    [Fact]
    public void Materializer_EnumValueTupleField_DataRecordContainsInteger_ShouldConvertToEnumMember()
    {
        var dataReader = Substitute.For<IDataReader>();

        var enumValue = Generate.Enum();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Enum");
        dataReader.GetFieldType(0).Returns(typeof(Int32));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetInt32(0).Returns((Int32)enumValue);

        var materializer = ValueTupleMaterializerFactory
            .GetMaterializer<ValueTuple<TestEnum>>(dataReader);

        var valueTuple = materializer(dataReader);

        valueTuple.Item1
            .Should().Be(enumValue);
    }

    [Fact]
    public void Materializer_EnumValueTupleField_DataRecordContainsIntegerNotMatchingAnyEnumMemberValue_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Enum");
        dataReader.GetFieldType(0).Returns(typeof(Int32));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetInt32(0).Returns(999);

        var materializer = ValueTupleMaterializerFactory
            .GetMaterializer<ValueTuple<TestEnum>>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Enum' returned by the SQL statement contains a value that could not be converted to " +
                $"the enum type {typeof(TestEnum)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<TestEnum>)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. That value does not match any of the values of the enum's members.*"
            );
    }

    [Fact]
    public void Materializer_EnumValueTupleField_DataRecordContainsString_ShouldConvertToEnumMember()
    {
        var dataReader = Substitute.For<IDataReader>();

        var enumValue = Generate.Enum();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Enum");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns(enumValue.ToString());

        var materializer = ValueTupleMaterializerFactory
            .GetMaterializer<ValueTuple<TestEnum>>(dataReader);

        var valueTuple = materializer(dataReader);

        valueTuple.Item1
            .Should().Be(enumValue);
    }

    [Fact]
    public void Materializer_EnumValueTupleField_DataRecordContainsStringNotMatchingAnyEnumMemberName_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Enum");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns("NonExistent");

        var materializer = ValueTupleMaterializerFactory
            .GetMaterializer<ValueTuple<TestEnum>>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Enum' returned by the SQL statement contains a value that could not be converted to " +
                $"the enum type {typeof(TestEnum)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<TestEnum>)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'NonExistent' to an enum member of the type {typeof(TestEnum)}. That " +
                $"string does not match any of the names of the enum's members.*"
            );
    }

    [Fact]
    public void
        Materializer_NonNullableCharValueTupleField_DataRecordFieldContainsStringWithLengthNotOne_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns(String.Empty);

        var materializer = ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Char>>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string '', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Char>)}. The string must be exactly one character long.*"
            );

        dataReader.GetString(0).Returns("ab");

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string 'ab', which could not be " +
                $"converted to the type {typeof(Char)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Char>)}. The string must be exactly one character long.*"
            );
    }

    [Fact]
    public void
        Materializer_NonNullableCharValueTupleField_DataRecordFieldContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns("X");

        var materializer = ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Char>>(dataReader);

        var valueTuple = materializer(dataReader);

        valueTuple.Item1
            .Should().Be('X');
    }

    [Fact]
    public void
        Materializer_NonNullableValueTupleField_DataRecordFieldContainsNull_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Id");
        dataReader.GetFieldType(0).Returns(typeof(Int64));
        dataReader.IsDBNull(0).Returns(true);

        var materializer = ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Int64>>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Id' returned by the SQL statement contains a NULL value, but the corresponding " +
                $"field of the value tuple type {typeof(ValueTuple<Int64>)} is non-nullable.*"
            );
    }

    [Fact]
    public void
        Materializer_NullableCharValueTupleField_DataRecordFieldContainsStringWithLengthNotOne_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns(String.Empty);

        var materializer = ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Char?>>(dataReader);

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string '', which could not be " +
                $"converted to the type {typeof(Char?)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Char?>)}. The string must be exactly one character long.*"
            );

        dataReader.GetString(0).Returns("ab");

        Invoking(() => materializer(dataReader))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"The column 'Char' returned by the SQL statement contains the string 'ab', which could not be " +
                $"converted to the type {typeof(Char?)} of the corresponding field of the value tuple type " +
                $"{typeof(ValueTuple<Char?>)}. The string must be exactly one character long.*"
            );
    }

    [Fact]
    public void
        Materializer_NullableCharValueTupleField_DataRecordFieldContainsStringWithLengthOne_ShouldGetFirstCharacter()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Char");
        dataReader.GetFieldType(0).Returns(typeof(String));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetString(0).Returns("X");

        var materializer = ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Char?>>(dataReader);

        var valueTuple = materializer(dataReader);

        valueTuple.Item1
            .Should().Be('X');
    }

    [Fact]
    public void Materializer_NullableValueTupleField_DataRecordFieldContainsNull_ShouldMaterializeNull()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetFieldType(0).Returns(typeof(Int64));
        dataReader.GetName(0).Returns("Id");
        dataReader.IsDBNull(0).Returns(true);
        dataReader.GetInt64(0).Throws(new SqlNullValueException());

        var materializer = ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Int64?>>(dataReader);

        var valueTuple = Invoking(() => materializer(dataReader))
            .Should().NotThrow().Subject;

        valueTuple.Item1
            .Should().BeNull();
    }

    [Fact]
    public void Materializer_ShouldMaterializeBinaryData()
    {
        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(1);

        dataReader.GetName(0).Returns("Data");
        dataReader.GetFieldType(0).Returns(typeof(Byte[]));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetValue(0).Returns(new Byte[] { 1, 2, 3 });

        var materializer = ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Byte[]>>(dataReader);

        var valueTuple = materializer(dataReader);

        valueTuple.Item1
            .Should().BeEquivalentTo(new Byte[] { 1, 2, 3 });
    }

    [Fact]
    public void Materializer_ShouldMaterializeDataRecordToValueTuple()
    {
        var entity = Generate.Entity();

        var dataReader = Substitute.For<IDataReader>();

        dataReader.FieldCount.Returns(7);

        dataReader.GetName(0).Returns("Boolean");
        dataReader.GetFieldType(0).Returns(typeof(Boolean));
        dataReader.IsDBNull(0).Returns(false);
        dataReader.GetBoolean(0).Returns(entity.BooleanValue);

        dataReader.GetName(1).Returns("Char");
        dataReader.GetFieldType(1).Returns(typeof(String));
        dataReader.IsDBNull(1).Returns(false);
        dataReader.GetString(1).Returns(entity.CharValue.ToString());

        dataReader.GetName(2).Returns("DateTime");
        dataReader.GetFieldType(2).Returns(typeof(DateTime));
        dataReader.IsDBNull(2).Returns(false);
        dataReader.GetDateTime(2).Returns(entity.DateTimeValue);

        dataReader.GetName(3).Returns("Nullable");
        dataReader.GetFieldType(3).Returns(typeof(Decimal));
        dataReader.IsDBNull(3).Returns(true);

        dataReader.GetName(4).Returns("Enum");
        dataReader.GetFieldType(4).Returns(typeof(String));
        dataReader.IsDBNull(4).Returns(false);
        dataReader.GetString(4).Returns(entity.EnumValue.ToString());

        dataReader.GetName(5).Returns("Guid");
        dataReader.GetFieldType(5).Returns(typeof(Guid));
        dataReader.IsDBNull(5).Returns(false);
        dataReader.GetGuid(5).Returns(entity.GuidValue);

        dataReader.GetName(6).Returns("Int32");
        dataReader.GetFieldType(6).Returns(typeof(Int32));
        dataReader.IsDBNull(6).Returns(false);
        dataReader.GetInt32(6).Returns(entity.Int32Value);

        var materializer = ValueTupleMaterializerFactory
            .GetMaterializer<(Boolean, Char, DateTime, Decimal?, TestEnum, Guid, Int32)>(dataReader);

        var valueTuple = materializer(dataReader);

        valueTuple.Item1
            .Should().Be(entity.BooleanValue);

        valueTuple.Item2
            .Should().Be(entity.CharValue);

        valueTuple.Item3
            .Should().Be(entity.DateTimeValue);

        valueTuple.Item4
            .Should().BeNull();

        valueTuple.Item5
            .Should().Be(entity.EnumValue);

        valueTuple.Item6
            .Should().Be(entity.GuidValue);

        valueTuple.Item7
            .Should().Be(entity.Int32Value);
    }

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            ValueTupleMaterializerFactory.GetMaterializer<ValueTuple<Int32>>(Substitute.For<IDataReader>())
        );
}
