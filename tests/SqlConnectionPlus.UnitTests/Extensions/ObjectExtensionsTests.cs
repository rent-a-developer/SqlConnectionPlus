// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable RedundantCast

using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Extensions;

public class ObjectExtensionsTests : TestsBase
{
    [Fact]
    public void ToDebugString_ShouldHandleObjectsWithCyclicReferences()
    {
        var itemA = new Item(Id: "A");
        var itemB = new Item(Id: "B");

        itemA.Reference = itemB;
        itemB.Reference = itemA;

        itemA.ToDebugString()
            .Should().Be(
                """'{"Id":"A","Reference":{"Id":"B","Reference":null}}' (RentADeveloper.SqlConnectionPlus.UnitTests.Extensions.ObjectExtensionsTests+Item)"""
            );
    }

    [Fact]
    public void ToDebugString_ShouldReturnStringRepresentationOfValue()
    {
        (null as Object).ToDebugString()
            .Should().Be("{null}");

        DBNull.Value.ToDebugString()
            .Should().Be("{DBNull}");

        "A String".ToDebugString()
            .Should().Be("'A String' (System.String)");

        'X'.ToDebugString()
            .Should().Be("'X' (System.Char)");

        true.ToDebugString()
            .Should().Be("'True' (System.Boolean)");

        123.45M.ToDebugString()
            .Should().Be("'123.45' (System.Decimal)");

        ((Single)123.45).ToDebugString()
            .Should().Be("'123.449997' (System.Single)");

        123.45.ToDebugString()
            .Should().Be("'123.45' (System.Double)");

        ((Byte)123).ToDebugString()
            .Should().Be("'123' (System.Byte)");

        ((SByte)123).ToDebugString()
            .Should().Be("'123' (System.SByte)");

        ((Int16)123).ToDebugString()
            .Should().Be("'123' (System.Int16)");

        ((UInt16)123).ToDebugString()
            .Should().Be("'123' (System.UInt16)");

        123.ToDebugString()
            .Should().Be("'123' (System.Int32)");

        ((UInt32)123).ToDebugString()
            .Should().Be("'123' (System.UInt32)");

        ((Int64)123).ToDebugString()
            .Should().Be("'123' (System.Int64)");

        ((UInt64)123).ToDebugString()
            .Should().Be("'123' (System.UInt64)");

        new Byte[] { 1, 2, 3 }.ToDebugString()
            .Should().Be("'AQID' (System.Byte[])");

        new Guid("889a8be0-f0ff-4555-86d8-8490434b7def").ToDebugString()
            .Should().Be("'889a8be0-f0ff-4555-86d8-8490434b7def' (System.Guid)");

        new DateTime(2025, 12, 31, 23, 59, 59, 999).ToDebugString()
            .Should().Be("'2025-12-31T23:59:59.9990000' (System.DateTime)");

        new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.FromHours(1)).ToDebugString()
            .Should().Be("'2025-12-31T23:59:59.0000000+01:00' (System.DateTimeOffset)");

        new TimeSpan(1, 2, 3, 4).ToDebugString()
            .Should().Be("'1.02:03:04' (System.TimeSpan)");

        TestEnum.Value3.ToDebugString()
            .Should().Be("'Value3' (RentADeveloper.SqlConnectionPlus.UnitTests.TestData.TestEnum)");

        new Int32[] { 1, 2, 3 }.ToDebugString()
            .Should().Be("'[1,2,3]' (System.Int32[])");

        new Object().ToDebugString()
            .Should().Be("'{}' (System.Object)");

        new EntityWithStringProperty { String = "A String" }.ToDebugString()
            .Should().Be(
                """'{"String":"A String"}' (RentADeveloper.SqlConnectionPlus.UnitTests.TestData.EntityWithStringProperty)"""
            );
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record Item(String Id)
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Item? Reference { get; set; }
    }
}
