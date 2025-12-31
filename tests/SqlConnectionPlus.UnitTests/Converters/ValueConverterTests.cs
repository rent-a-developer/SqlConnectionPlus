using System.Reflection;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Converters;

public class ValueConverterTests : TestsBase
{
    [Fact]
    public void ConvertValueToType_NonNullableCharTargetType_StringWithLengthOneValue_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        ValueConverter.ConvertValueToType<Char>(character.ToString())
            .Should().Be(character);
    }

    [Fact]
    public void ConvertValueToType_NonNullableCharTargetType_ValueIsStringWithLengthNotOne_ShouldThrow()
    {
        Invoking(() => ValueConverter.ConvertValueToType<Char>(String.Empty))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string '' to the target type {typeof(Char)}. The string must be " +
                $"exactly one character long."
            );

        Invoking(() => ValueConverter.ConvertValueToType<Char>("ab"))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'ab' to the target type {typeof(Char)}. The string must be " +
                $"exactly one character long."
            );
    }

    [Fact]
    public void
        ConvertValueToType_NonNullableEnumTargetType_IntegerValueNotMatchingAnyEnumMemberValue_ShouldThrow() =>
        Invoking(() => ValueConverter.ConvertValueToType<TestEnum>(999))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to the target type {typeof(TestEnum)}. " +
                $"See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum)}. That value does not match any of the values of the enum's members.*"
            );

    [Fact]
    public void ConvertValueToType_NonNullableEnumTargetType_ShouldConvertToEnumMember()
    {
        var enumValue = Generate.Enum();

        ValueConverter.ConvertValueToType<TestEnum>((Int32)enumValue)
            .Should().Be(enumValue);
    }

    [Fact]
    public void
        ConvertValueToType_NonNullableEnumTargetType_StringValueNotMatchingAnyEnumMemberName_ShouldThrow() =>
        Invoking(() => ValueConverter.ConvertValueToType<TestEnum>("NonExistent"))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value 'NonExistent' ({typeof(String)}) to the target type " +
                $"{typeof(TestEnum)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'NonExistent' to an enum member of the type {typeof(TestEnum)}. " +
                $"That string does not match any of the names of the enum's members.*"
            );

    [Fact]
    public void ConvertValueToType_NonNullableTargetType_DBNullValue_ShouldThrow() =>
        Invoking(() => ValueConverter.ConvertValueToType<DateTime>(DBNull.Value))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value {{DBNull}} to the target type {typeof(DateTime)}, because the " +
                $"target type is non-nullable.*"
            );

    [Fact]
    public void ConvertValueToType_NonNullableTargetType_NullValue_ShouldThrow() =>
        Invoking(() => ValueConverter.ConvertValueToType<DateTime>(null))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value {{null}} to the target type {typeof(DateTime)}, because the target " +
                $"type is non-nullable.*"
            );

    [Fact]
    public void ConvertValueToType_NullableCharTargetType_StringValueWithLengthNotOne_ShouldThrow()
    {
        Invoking(() => ValueConverter.ConvertValueToType<Char?>(String.Empty))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string '' to the target type {typeof(Char?)}. The string must be " +
                $"exactly one character long."
            );

        Invoking(() => ValueConverter.ConvertValueToType<Char?>("ab"))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'ab' to the target type {typeof(Char?)}. The string must be " +
                $"exactly one character long."
            );
    }

    [Fact]
    public void ConvertValueToType_NullableCharTargetType_StringValueWithLengthOne_ShouldGetFirstCharacter()
    {
        var character = Generate.GenerateCharacter();

        ValueConverter.ConvertValueToType<Char?>(character.ToString())
            .Should().Be(character);
    }

    [Fact]
    public void
        ConvertValueToType_NullableEnumTargetType_IntegerValueNotMatchingAnyEnumMemberValue_ShouldThrow() =>
        Invoking(() => ValueConverter.ConvertValueToType<TestEnum?>(999))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to the target type {typeof(TestEnum?)}. " +
                $"See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value '999' ({typeof(Int32)}) to an enum member of the type " +
                $"{typeof(TestEnum?)}. That value does not match any of the values of the enum's members.*"
            );

    [Fact]
    public void ConvertValueToType_NullableEnumTargetType_ShouldConvertToEnumMember()
    {
        var enumValue = Generate.Enum();

        ValueConverter.ConvertValueToType<TestEnum?>((Int32)enumValue)
            .Should().Be(enumValue);
    }

    [Fact]
    public void
        ConvertValueToType_NullableEnumTargetType_StringValueNotMatchingAnyEnumMemberName_ShouldThrow() =>
        Invoking(() => ValueConverter.ConvertValueToType<TestEnum?>("NonExistent"))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value 'NonExistent' ({typeof(String)}) to the target type " +
                $"{typeof(TestEnum?)}. See inner exception for details.*"
            )
            .WithInnerException<InvalidCastException>()
            .WithMessage(
                $"Could not convert the string 'NonExistent' to an enum member of the type {typeof(TestEnum?)}. " +
                $"That string does not match any of the names of the enum's members.*"
            );

    [Fact]
    public void ConvertValueToType_NullableTargetType_DBNullValue_ShouldReturnNull()
    {
        ValueConverter.ConvertValueToType<Object>(DBNull.Value)
            .Should().BeNull();

        ValueConverter.ConvertValueToType<Int32?>(DBNull.Value)
            .Should().BeNull();
    }


    [Fact]
    public void ConvertValueToType_NullableTargetType_NullValue_ShouldReturnNull()
    {
        ValueConverter.ConvertValueToType<Object>(null)
            .Should().BeNull();

        ValueConverter.ConvertValueToType<Int32?>(null)
            .Should().BeNull();
    }

    [Fact]
    public void ConvertValueToType_TargetTypeIsObject_ShouldReturnValueAsIs() =>
        ValueConverter.ConvertValueToType<Object>(Guid.Empty)
            .Should().Be(Guid.Empty);

    [Fact]
    public void ConvertValueToType_ValueAlreadyOfTargetType_ShouldReturnValueAsIs()
    {
        ValueConverter.ConvertValueToType<Int64>(123L)
            .Should().Be(123L);

        ValueConverter.ConvertValueToType<Int64?>(123L)
            .Should().Be(123L);
    }

    [Theory]
    [InlineData(123, typeof(Int16), (Int16)123)]
    [InlineData(123, typeof(Int64), 123L)]
    [InlineData(123, typeof(Double), 123D)]
    [InlineData("123", typeof(Int16), (Int16)123)]
    [InlineData("123", typeof(Int32), 123)]
    [InlineData("123", typeof(Int64), 123L)]
    [InlineData(TestEnum.Value3, typeof(Int64), 3)]
    [InlineData(TestEnum.Value3, typeof(String), nameof(TestEnum.Value3))]
    [InlineData('X', typeof(String), "X")]
    public void ConvertValueToType_ValueCanBeConvertedToTargetType_ShouldConvertValue(
        Object value,
        Type targetType,
        Object expectedValue
    ) =>
        typeof(ValueConverter).GetMethod(
                nameof(ValueConverter.ConvertValueToType),
                BindingFlags.Static | BindingFlags.NonPublic
            )!
            .MakeGenericMethod(targetType)
            .Invoke(null, [value])
            .Should().Be(expectedValue);

    [Fact]
    public void ConvertValueToType_ValueCannotBeConvertedToTargetType_ShouldThrow() =>
        Invoking(() => ValueConverter.ConvertValueToType<DateTime>("NotADate"))
            .Should().Throw<InvalidCastException>()
            .WithMessage(
                $"Could not convert the value 'NotADate' ({typeof(String)}) to the target type " +
                $"{typeof(DateTime)}. See inner exception for details.*"
            );
}
