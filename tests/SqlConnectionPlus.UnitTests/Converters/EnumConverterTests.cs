using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Converters;

public class EnumConverterTests : TestsBase
{
    [Fact]
    public void ConvertValueToEnumMember_DecimalValue_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>(1.23M))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '1.23' ({typeof(Decimal)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. The value must either be an enum value of that type or a string or an integer.*"
            );

    [Fact]
    public void ConvertValueToEnumMember_EmptyStringValue_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>(String.Empty))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert an empty string or a string that consists only of white-space characters to an " +
                $"enum member of the type {typeof(TestEnum)}.*"
            );

    [Theory]
    [InlineData(1, TestEnum.Value1)]
    [InlineData(2, TestEnum.Value2)]
    [InlineData(3, TestEnum.Value3)]
    [InlineData(1L, TestEnum.Value1)]
    [InlineData(2L, TestEnum.Value2)]
    [InlineData(3L, TestEnum.Value3)]
    [InlineData((Byte)1, TestEnum.Value1)]
    [InlineData((Byte)2, TestEnum.Value2)]
    [InlineData((Byte)3, TestEnum.Value3)]
    public void ConvertValueToEnumMember_IntegerValue_ShouldConvertValueToEnumMember(
        Object integerValue,
        TestEnum expectedResult
    ) =>
        EnumConverter.ConvertValueToEnumMember<TestEnum>(integerValue)
            .Should().Be(expectedResult);

    [Fact]
    public void ConvertValueToEnumMember_IntegerValueNotMatchingAnyEnumMemberValue_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>(999))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. That value does not match any of the values of the enum's members.*"
            );

    [Theory]
    [InlineData(1L, TestEnum.Value1)]
    [InlineData(2L, TestEnum.Value2)]
    [InlineData(3L, TestEnum.Value3)]
    public void
        ConvertValueToEnumMember_IntegerValueWithDifferentSizeThanEnumUnderlyingType_ShouldConvertValueToEnumMember(
            Int64 integerValue,
            TestEnum expectedResult
        ) =>
        EnumConverter.ConvertValueToEnumMember<TestEnum>(integerValue)
            .Should().Be(expectedResult);

    [Fact]
    public void ConvertValueToEnumMember_NonEnumTargetType_ShouldThrow()
    {
        Invoking(() => EnumConverter.ConvertValueToEnumMember<Int32>("ValueA"))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"Could not convert the value 'ValueA' ({typeof(String)}) to an enum member of the type " +
                $"{typeof(Int32)}, because {typeof(Int32)} is not an enum type.*"
            );

        Invoking(() => EnumConverter.ConvertValueToEnumMember<Nullable<Int32>>("ValueA"))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"Could not convert the value 'ValueA' ({typeof(String)}) to an enum member of the type " +
                $"{typeof(Nullable<Int32>)}, because {typeof(Nullable<Int32>)} is not an enum type.*"
            );
    }

    [Fact]
    public void ConvertValueToEnumMember_NonNullableTargetType_DBNullValue_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>(DBNull.Value))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert {{null}} to an enum member of the type {typeof(TestEnum)}."
            );

    [Fact]
    public void ConvertValueToEnumMember_NonNullableTargetType_NullValue_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>(null))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert {{null}} to an enum member of the type {typeof(TestEnum)}."
            );

    [Theory]
    [InlineData(1, TestEnum.Value1)]
    [InlineData(2, TestEnum.Value2)]
    [InlineData(3, TestEnum.Value3)]
    [InlineData(1L, TestEnum.Value1)]
    [InlineData(2L, TestEnum.Value2)]
    [InlineData(3L, TestEnum.Value3)]
    [InlineData((Byte)1, TestEnum.Value1)]
    [InlineData((Byte)2, TestEnum.Value2)]
    [InlineData((Byte)3, TestEnum.Value3)]
    [InlineData("Value1", TestEnum.Value1)]
    [InlineData("Value2", TestEnum.Value2)]
    [InlineData("Value3", TestEnum.Value3)]
    [InlineData("vaLue1", TestEnum.Value1)]
    [InlineData("vaLue2", TestEnum.Value2)]
    [InlineData("vaLue3", TestEnum.Value3)]
    public void ConvertValueToEnumMember_NonNullableTargetType_ShouldConvertValueToEnumMember(
        Object value,
        TestEnum expectedResult
    ) =>
        EnumConverter.ConvertValueToEnumMember<TestEnum>(value)
            .Should().Be(expectedResult);

    [Fact]
    public void ConvertValueToEnumMember_NullableTargetType_DBNullValue_ShouldReturnNull() =>
        EnumConverter.ConvertValueToEnumMember<TestEnum?>(DBNull.Value)
            .Should().BeNull();

    [Fact]
    public void ConvertValueToEnumMember_NullableTargetType_NullValue_ShouldReturnNull() =>
        EnumConverter.ConvertValueToEnumMember<TestEnum?>(null)
            .Should().BeNull();

    [Theory]
    [InlineData(1, TestEnum.Value1)]
    [InlineData(2, TestEnum.Value2)]
    [InlineData(3, TestEnum.Value3)]
    [InlineData(1L, TestEnum.Value1)]
    [InlineData(2L, TestEnum.Value2)]
    [InlineData(3L, TestEnum.Value3)]
    [InlineData((Byte)1, TestEnum.Value1)]
    [InlineData((Byte)2, TestEnum.Value2)]
    [InlineData((Byte)3, TestEnum.Value3)]
    [InlineData("Value1", TestEnum.Value1)]
    [InlineData("Value2", TestEnum.Value2)]
    [InlineData("Value3", TestEnum.Value3)]
    [InlineData("vaLue1", TestEnum.Value1)]
    [InlineData("vaLue2", TestEnum.Value2)]
    [InlineData("vaLue3", TestEnum.Value3)]
    public void ConvertValueToEnumMember_NullableTargetType_ShouldConvertValueToEnumMember(
        Object value,
        TestEnum expectedResult
    ) =>
        EnumConverter.ConvertValueToEnumMember<Nullable<TestEnum>>(value)
            .Should().Be(expectedResult);

    [Theory]
    [InlineData("1", TestEnum.Value1)]
    [InlineData("2", TestEnum.Value2)]
    [InlineData("3", TestEnum.Value3)]
    public void ConvertValueToEnumMember_StringContainingIntegerValue_ShouldConvertValueToEnumMember(
        String stringValue,
        TestEnum expectedResult
    ) =>
        EnumConverter.ConvertValueToEnumMember<TestEnum>(stringValue)
            .Should().Be(expectedResult);

    [Theory]
    [InlineData("Value1", TestEnum.Value1)]
    [InlineData("Value2", TestEnum.Value2)]
    [InlineData("Value3", TestEnum.Value3)]
    [InlineData("vaLue1", TestEnum.Value1)]
    [InlineData("vaLue2", TestEnum.Value2)]
    [InlineData("vaLue3", TestEnum.Value3)]
    public void ConvertValueToEnumMember_StringValue_ShouldConvertValueToEnumMemberCaseInsensitive(
        String stringValue,
        TestEnum expectedResult
    ) =>
        EnumConverter.ConvertValueToEnumMember<TestEnum>(stringValue)
            .Should().Be(expectedResult);

    [Fact]
    public void ConvertValueToEnumMember_StringValueNotMatchingAnyEnumMemberName_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>("NonExistent"))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'NonExistent' to an enum member of the type {typeof(TestEnum)}. " +
                $"That string does not match any of the names of the enum's members.*"
            );

    [Fact]
    public void ConvertValueToEnumMember_ValueAlreadyOfTargetType_ShouldReturnValueAsIs()
    {
        var enumValue = Generate.Enum();

        EnumConverter.ConvertValueToEnumMember<TestEnum>(enumValue)
            .Should().Be(enumValue);

        EnumConverter.ConvertValueToEnumMember<TestEnum?>(enumValue)
            .Should().Be(enumValue);
    }

    [Fact]
    public void ConvertValueToEnumMember_ValueIsNeitherEnumValueNorStringNorInteger_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>(Guid.Empty))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '{Guid.Empty}' ({typeof(Guid)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. The value must either be an enum value of that type or a string or an integer.*"
            );

    [Fact]
    public void ConvertValueToEnumMember_ValueIsOfDifferentEnumType_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>(ConsoleColor.Red))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value 'Red' ({typeof(ConsoleColor)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. The value must either be an enum value of that type or a string or an integer."
            );

    [Fact]
    public void ConvertValueToEnumMember_WhitespaceStringValue_ShouldThrow() =>
        Invoking(() => EnumConverter.ConvertValueToEnumMember<TestEnum>("   "))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert an empty string or a string that consists only of white-space characters to an " +
                $"enum member of the type {typeof(TestEnum)}.*"
            );
}
