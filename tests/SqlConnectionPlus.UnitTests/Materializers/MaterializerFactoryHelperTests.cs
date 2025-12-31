using System.Linq.Expressions;
using System.Numerics;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Materializers;

public class MaterializerFactoryHelperTests : TestsBase
{
    [Fact]
    public void CreateGetRecordFieldValueExpression_BytesFieldType_ShouldCallGetValueAndConvert()
    {
        var dataReader = Substitute.For<IDataReader>();

        var expression = MaterializerFactoryHelper.CreateGetRecordFieldValueExpression(
            Expression.Constant(dataReader),
            Expression.Constant(1),
            1,
            "FieldA",
            typeof(Byte[])
        );

        expression.ToString()
            .Should().Match("Convert(*IDataReader*.GetValue(1), Byte[])");
    }

    [Fact]
    public void CreateGetRecordFieldValueExpression_DateTimeOffsetFieldType_ShouldCallGetValueAndConvert()
    {
        var dataReader = Substitute.For<IDataReader>();

        var expression = MaterializerFactoryHelper.CreateGetRecordFieldValueExpression(
            Expression.Constant(dataReader),
            Expression.Constant(1),
            1,
            "FieldA",
            typeof(DateTimeOffset)
        );

        expression.ToString()
            .Should().Match("Convert(*IDataReader*.GetValue(1), DateTimeOffset)");
    }

    [Theory]
    [InlineData(typeof(Boolean), "*IDataReader*.GetBoolean(1)")]
    [InlineData(typeof(Byte), "*IDataReader*.GetByte(1)")]
    [InlineData(typeof(Char), "*IDataReader*.GetString(1)")]
    [InlineData(typeof(DateTime), "*IDataReader*.GetDateTime(1)")]
    [InlineData(typeof(Decimal), "*IDataReader*.GetDecimal(1)")]
    [InlineData(typeof(Double), "*IDataReader*.GetDouble(1)")]
    [InlineData(typeof(Single), "*IDataReader*.GetFloat(1)")]
    [InlineData(typeof(Guid), "*IDataReader*.GetGuid(1)")]
    [InlineData(typeof(Int16), "*IDataReader*.GetInt16(1)")]
    [InlineData(typeof(Int32), "*IDataReader*.GetInt32(1)")]
    [InlineData(typeof(Int64), "*IDataReader*.GetInt64(1)")]
    [InlineData(typeof(String), "*IDataReader*.GetString(1)")]
    public void CreateGetRecordFieldValueExpression_ShouldCallTypedGetMethod(Type fieldType, String expectedExpression)
    {
        var dataReader = Substitute.For<IDataReader>();

        var expression = MaterializerFactoryHelper.CreateGetRecordFieldValueExpression(
            Expression.Constant(dataReader),
            Expression.Constant(1),
            1,
            "FieldA",
            fieldType
        );

        expression.ToString()
            .Should().Match(expectedExpression);
    }

    [Fact]
    public void CreateGetRecordFieldValueExpression_TimeSpanFieldType_ShouldCallGetValueAndConvert()
    {
        var dataReader = Substitute.For<IDataReader>();

        var expression = MaterializerFactoryHelper.CreateGetRecordFieldValueExpression(
            Expression.Constant(dataReader),
            Expression.Constant(1),
            1,
            "FieldA",
            typeof(TimeSpan)
        );

        expression.ToString()
            .Should().Match("Convert(*IDataReader*.GetValue(1), TimeSpan)");
    }

    [Fact]
    public void CreateGetRecordFieldValueExpression_UnsupportedFieldType_ShouldThrow()
    {
        var dataReader = Substitute.For<IDataReader>();

        Invoking(() => MaterializerFactoryHelper.CreateGetRecordFieldValueExpression(
                Expression.Constant(dataReader),
                Expression.Constant(1),
                1,
                "FieldA",
                typeof(BigInteger)
            ))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                $"The data type {typeof(BigInteger)} of the column 'FieldA' returned by the SQL statement is not " +
                $"supported.*"
            );
    }

    [Theory]
    [InlineData(typeof(Boolean), true)]
    [InlineData(typeof(Byte), true)]
    [InlineData(typeof(Char), true)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(Decimal), true)]
    [InlineData(typeof(Double), true)]
    [InlineData(typeof(Single), true)]
    [InlineData(typeof(Guid), true)]
    [InlineData(typeof(Int16), true)]
    [InlineData(typeof(Int32), true)]
    [InlineData(typeof(Int64), true)]
    [InlineData(typeof(String), true)]
    [InlineData(typeof(Byte[]), true)]
    [InlineData(typeof(TimeSpan), true)]
    [InlineData(typeof(DateTimeOffset), true)]
    [InlineData(typeof(BigInteger), false)]
    public void IsDataRecordTypedGetMethodAvailable_ShouldReturnWhetherTypedGetMethodIsAvailable(
        Type fieldType,
        Boolean expectedResult
    ) =>
        MaterializerFactoryHelper.IsDataRecordTypedGetMethodAvailable(fieldType)
            .Should().Be(expectedResult);

    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var dataReader = Substitute.For<IDataReader>();

        ArgumentNullGuardVerifier.Verify(() =>
            MaterializerFactoryHelper.CreateGetRecordFieldValueExpression(
                Expression.Constant(dataReader),
                Expression.Constant(1),
                1,
                "FieldA",
                typeof(Int32)
            )
        );
    }
}
