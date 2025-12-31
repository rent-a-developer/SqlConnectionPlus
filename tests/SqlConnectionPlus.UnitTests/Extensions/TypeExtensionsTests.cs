using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;
using TypeExtensions = RentADeveloper.SqlConnectionPlus.Extensions.TypeExtensions;

// ReSharper disable InvokeAsExtensionMethod

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Extensions;

public class TypeExtensionsTests : TestsBase
{
    [Theory]
    [InlineData(typeof(String), typeof(List<String>))]
    [InlineData(typeof(Int32), typeof(List<Int32>))]
    [InlineData(typeof(Int32?), typeof(List<Int32?>))]
    [InlineData(typeof(DateTime), typeof(List<DateTime>))]
    [InlineData(typeof(DateTime?), typeof(List<DateTime?>))]
    [InlineData(typeof(Entity), typeof(List<Entity>))]
    [InlineData(typeof(TestEnum), typeof(List<TestEnum>))]
    public void CreateListForType_ShouldReturnEmptyListOfSpecifiedType(
        Type elementType,
        Type expectedListType
    )
    {
        var list = elementType.CreateListForType();

        list
            .Should().BeOfType(expectedListType);

        list.Count
            .Should().Be(0);

        var enumerator = list.GetEnumerator();

        enumerator.MoveNext()
            .Should().BeFalse();

        ((IDisposable)enumerator).Dispose();
    }

    [Theory]
    [InlineData(typeof(Boolean), true)]
    [InlineData(typeof(Boolean?), true)]
    [InlineData(typeof(Byte), true)]
    [InlineData(typeof(Byte?), true)]
    [InlineData(typeof(Char), true)]
    [InlineData(typeof(Char?), true)]
    [InlineData(typeof(DateOnly), true)]
    [InlineData(typeof(DateOnly?), true)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(DateTime?), true)]
    [InlineData(typeof(DateTimeOffset), true)]
    [InlineData(typeof(DateTimeOffset?), true)]
    [InlineData(typeof(Decimal), true)]
    [InlineData(typeof(Decimal?), true)]
    [InlineData(typeof(Double), true)]
    [InlineData(typeof(Double?), true)]
    [InlineData(typeof(Guid), true)]
    [InlineData(typeof(Guid?), true)]
    [InlineData(typeof(Int16), true)]
    [InlineData(typeof(Int16?), true)]
    [InlineData(typeof(Int32), true)]
    [InlineData(typeof(Int32?), true)]
    [InlineData(typeof(Int64), true)]
    [InlineData(typeof(Int64?), true)]
    [InlineData(typeof(IntPtr), true)]
    [InlineData(typeof(IntPtr?), true)]
    [InlineData(typeof(SByte), true)]
    [InlineData(typeof(SByte?), true)]
    [InlineData(typeof(Single), true)]
    [InlineData(typeof(Single?), true)]
    [InlineData(typeof(String), true)]
    [InlineData(typeof(TimeOnly), true)]
    [InlineData(typeof(TimeOnly?), true)]
    [InlineData(typeof(TimeSpan), true)]
    [InlineData(typeof(TimeSpan?), true)]
    [InlineData(typeof(UInt16), true)]
    [InlineData(typeof(UInt16?), true)]
    [InlineData(typeof(UInt32), true)]
    [InlineData(typeof(UInt32?), true)]
    [InlineData(typeof(UInt64), true)]
    [InlineData(typeof(UInt64?), true)]
    [InlineData(typeof(UIntPtr), true)]
    [InlineData(typeof(UIntPtr?), true)]
    [InlineData(typeof(Entity), false)]
    [InlineData(typeof(TestEnum), false)]
    public void IsBuiltInTypeOrNullableBuildInType_ShouldDetermineWhetherTypeIsBuildInTypeOrNullableBuildInType(
        Type type,
        Boolean expectedResult
    ) =>
        type.IsBuiltInTypeOrNullableBuildInType()
            .Should().Be(expectedResult);

    [Theory]
    [InlineData(typeof(Char), true)]
    [InlineData(typeof(Char?), true)]
    [InlineData(typeof(String), false)]
    [InlineData(typeof(DateTime), false)]
    [InlineData(typeof(Entity), false)]
    public void IsCharOrNullableCharType_ShouldDetermineWhetherTypeIsCharOrNullableCharType(
        Type type,
        Boolean expectedResult
    ) =>
        type.IsCharOrNullableCharType()
            .Should().Be(expectedResult);

    [Theory]
    [InlineData(typeof(TestEnum), true)]
    [InlineData(typeof(TestEnum?), true)]
    [InlineData(typeof(ConsoleColor), true)]
    [InlineData(typeof(ConsoleColor?), true)]
    [InlineData(typeof(String), false)]
    [InlineData(typeof(DateTime), false)]
    [InlineData(typeof(Entity), false)]
    public void IsEnumOrNullableEnumType_ShouldDetermineWhetherTypeIsEnumTypeOrNullableEnumType(
        Type type,
        Boolean expectedResult
    ) =>
        type.IsEnumOrNullableEnumType()
            .Should().Be(expectedResult);

    [Theory]
    [InlineData(typeof(Nullable<Int32>), true)]
    [InlineData(typeof(Nullable<DateTime>), true)]
    [InlineData(typeof(Object), true)]
    [InlineData(typeof(Entity), true)]
    [InlineData(typeof(Int32[]), true)]
    [InlineData(typeof(String[]), true)]
    [InlineData(typeof(Int32), false)]
    [InlineData(typeof(DateTime), false)]
    public void IsReferenceTypeOrNullableType_ShouldDetermineWhetherTypeIsReferenceTypeOrNullableType(
        Type type,
        Boolean expectedResult
    ) =>
        type.IsReferenceTypeOrNullableType()
            .Should().Be(expectedResult);

    [Theory]
    [InlineData(typeof(ValueTuple<Int32>), true)]
    [InlineData(typeof(ValueTuple<Int32, Int32>), true)]
    [InlineData(typeof(ValueTuple<Int32, Int32, Int32>), true)]
    [InlineData(typeof(ValueTuple<Int32, Int32, Int32, Int32>), true)]
    [InlineData(typeof(ValueTuple<Int32, Int32, Int32, Int32, Int32>), true)]
    [InlineData(typeof(ValueTuple<Int32, Int32, Int32, Int32, Int32, Int32>), true)]
    [InlineData(typeof(ValueTuple<Int32, Int32, Int32, Int32, Int32, Int32, Int32>), true)]
    // Only value tuples with up to 7 fields are supported.
    [InlineData(typeof(ValueTuple<Int32, Int32, Int32, Int32, Int32, Int32, Int32, Int32>), false)]
    [InlineData(typeof(DateTime), false)]
    [InlineData(typeof(Entity), false)]
    [InlineData(typeof(Tuple<Int32>), false)]
    [InlineData(typeof(Tuple<Int32, Int32>), false)]
    public void IsValueTupleTypeWithUpTo7Fields_ShouldDetermineWhetherTypeIsValueTupleTypeWithUpTo7Fields(
        Type type,
        Boolean expectedResult
    ) =>
        type.IsValueTupleTypeWithUpTo7Fields()
            .Should().Be(expectedResult);

    [Fact]
    public void VerifyNullArgumentGuards()
    {
        ArgumentNullGuardVerifier.Verify(() => TypeExtensions.CreateListForType(typeof(String)));
        ArgumentNullGuardVerifier.Verify(() =>
            TypeExtensions.IsBuiltInTypeOrNullableBuildInType(typeof(DateTime))
        );
        ArgumentNullGuardVerifier.Verify(() => TypeExtensions.IsCharOrNullableCharType(typeof(Char)));
        ArgumentNullGuardVerifier.Verify(() => TypeExtensions.IsEnumOrNullableEnumType(typeof(TestEnum)));
        ArgumentNullGuardVerifier.Verify(() => TypeExtensions.IsReferenceTypeOrNullableType(typeof(Entity)));
        ArgumentNullGuardVerifier.Verify(() =>
            TypeExtensions.IsValueTupleTypeWithUpTo7Fields(typeof(ValueTuple<Int32>))
        );
    }
}
