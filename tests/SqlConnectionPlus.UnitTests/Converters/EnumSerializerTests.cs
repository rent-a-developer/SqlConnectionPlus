using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Converters;

public class EnumSerializerTests : TestsBase
{
    [Fact]
    public void SerializeEnum_InvalidEnumSerializationMode_ShouldThrow() =>
        Invoking(() => EnumSerializer.SerializeEnum(TestEnum.Value3, (EnumSerializationMode)999))
            .Should().Throw<NotSupportedException>()
            .WithMessage(
                $"The {nameof(EnumSerializationMode)} '999' ({typeof(EnumSerializationMode)}) is not supported."
            );

    [Theory]
    [InlineData(EnumSerializationMode.Strings, TestEnum.Value1, "Value1")]
    [InlineData(EnumSerializationMode.Strings, TestEnum.Value2, "Value2")]
    [InlineData(EnumSerializationMode.Strings, TestEnum.Value3, "Value3")]
    [InlineData(EnumSerializationMode.Integers, TestEnum.Value1, 1)]
    [InlineData(EnumSerializationMode.Integers, TestEnum.Value2, 2)]
    [InlineData(EnumSerializationMode.Integers, TestEnum.Value3, 3)]
    public void SerializeEnum_ShouldSerializeEnumValueAccordingToSerializationMode(
        EnumSerializationMode enumSerializationMode,
        TestEnum enumValue,
        Object expectedSerializedValue
    ) =>
        EnumSerializer.SerializeEnum(enumValue, enumSerializationMode)
            .Should().Be(expectedSerializedValue);

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            EnumSerializer.SerializeEnum(TestEnum.Value3, EnumSerializationMode.Strings));
}
