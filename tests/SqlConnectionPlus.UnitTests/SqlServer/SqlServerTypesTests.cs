using RentADeveloper.SqlConnectionPlus.SqlServer;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.SqlServer;

public class SqlServerTypesTests : TestsBase
{
    [Fact]
    public void GetSqlServerDataType_EnumType_EnumSerializationModeIsInteger_ShouldReturnInt() =>
        SqlServerTypes.GetSqlServerDataType(typeof(TestEnum), EnumSerializationMode.Integers)
            .Should().Be("INT");

    [Fact]
    public void GetSqlServerDataType_EnumType_EnumSerializationModeIsNotSupported_ShouldThrow() =>
        Invoking(() => SqlServerTypes.GetSqlServerDataType(typeof(TestEnum), (EnumSerializationMode)999))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage(
                $"The {nameof(EnumSerializationMode)} '999' ({typeof(EnumSerializationMode)}) is not supported.*"
            );

    [Fact]
    public void GetSqlServerDataType_EnumType_EnumSerializationModeIsString_ShouldReturnNVarchar() =>
        SqlServerTypes.GetSqlServerDataType(typeof(TestEnum), EnumSerializationMode.Strings)
            .Should().Be("NVARCHAR(200)");

    [Fact]
    public void GetSqlServerDataType_NullableEnumType_SerializationModeIsInteger_ShouldReturnInt() =>
        SqlServerTypes.GetSqlServerDataType(typeof(Nullable<TestEnum>), EnumSerializationMode.Integers)
            .Should().Be("INT");

    [Fact]
    public void GetSqlServerDataType_NullableEnumType_SerializationModeIsString_ShouldReturnNVarchar() =>
        SqlServerTypes.GetSqlServerDataType(typeof(Nullable<TestEnum>), EnumSerializationMode.Strings)
            .Should().Be("NVARCHAR(200)");

    [Theory]
    [InlineData(typeof(Boolean?), "BIT")]
    [InlineData(typeof(Boolean), "BIT")]
    [InlineData(typeof(Byte), "TINYINT")]
    [InlineData(typeof(Byte?), "TINYINT")]
    [InlineData(typeof(Byte[]), "VARBINARY(MAX)")]
    [InlineData(typeof(Char?), "CHAR(1)")]
    [InlineData(typeof(Char), "CHAR(1)")]
    [InlineData(typeof(DateTimeOffset?), "DATETIMEOFFSET")]
    [InlineData(typeof(DateTimeOffset), "DATETIMEOFFSET")]
    [InlineData(typeof(DateTime?), "DATETIME2")]
    [InlineData(typeof(DateTime), "DATETIME2")]
    [InlineData(typeof(Decimal?), "DECIMAL(28,10)")]
    [InlineData(typeof(Decimal), "DECIMAL(28,10)")]
    [InlineData(typeof(Double?), "FLOAT")]
    [InlineData(typeof(Double), "FLOAT")]
    [InlineData(typeof(Guid?), "UNIQUEIDENTIFIER")]
    [InlineData(typeof(Guid), "UNIQUEIDENTIFIER")]
    [InlineData(typeof(Int16?), "SMALLINT")]
    [InlineData(typeof(Int16), "SMALLINT")]
    [InlineData(typeof(Int32?), "INT")]
    [InlineData(typeof(Int32), "INT")]
    [InlineData(typeof(Int64?), "BIGINT")]
    [InlineData(typeof(Int64), "BIGINT")]
    [InlineData(typeof(Object), "sql_variant")]
    [InlineData(typeof(Single?), "REAL")]
    [InlineData(typeof(Single), "REAL")]
    [InlineData(typeof(String), "NVARCHAR(MAX)")]
    [InlineData(typeof(TimeSpan?), "TIME")]
    [InlineData(typeof(TimeSpan), "TIME")]
    public void GetSqlServerDataType_SupportedTypeType_ShouldReturnSqlDataType(Type type, String expectedResult) =>
        SqlServerTypes.GetSqlServerDataType(type, EnumSerializationMode.Strings)
            .Should().Be(expectedResult);

    [Fact]
    public void GetSqlServerDataType_UnsupportedType_ShouldThrow() =>
        Invoking(() => SqlServerTypes.GetSqlServerDataType(typeof(Entity), EnumSerializationMode.Strings))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage($"Could not map the type {typeof(Entity)} to an SQL Server data type.*");

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            SqlServerTypes.GetSqlServerDataType(typeof(Int32), EnumSerializationMode.Strings)
        );
}
