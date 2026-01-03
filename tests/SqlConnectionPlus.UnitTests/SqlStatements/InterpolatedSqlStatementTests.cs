using RentADeveloper.SqlConnectionPlus.SqlStatements;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.SqlStatements;

public class InterpolatedSqlStatementTests : TestsBase
{
    [Fact]
    public void
        AppendFormatted_InterpolatedParameter_EnumValue_EnumSerializationModeIsIntegers_ShouldSerializeEnumToIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var enumValue = Generate.Enum();

        InterpolatedSqlStatement statement = $"SELECT {Parameter(enumValue)}";

        statement.Parameters
            .Should().Contain("@EnumValue", (Int32)enumValue);
    }

    [Fact]
    public void
        AppendFormatted_InterpolatedParameter_EnumValue_EnumSerializationModeIsStrings_ShouldSerializeEnumToString()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var enumValue = Generate.Enum();

        InterpolatedSqlStatement statement = $"SELECT {Parameter(enumValue)}";

        statement.Parameters
            .Should().Contain("@EnumValue", enumValue.ToString());
    }

    [Fact]
    public void AppendFormatted_InterpolatedParameter_NameCantBeInferred_ShouldUseGenericName()
    {
        InterpolatedSqlStatement statement = $"SELECT {Parameter(new { })}, {Parameter(new { })}, {Parameter(new { })}";

        statement.Code
            .Should().Be("SELECT @Parameter_1, @Parameter_2, @Parameter_3");

        statement.Parameters
            .Should().ContainKey("@Parameter_1");

        statement.Parameters
            .Should().ContainKey("@Parameter_2");

        statement.Parameters
            .Should().ContainKey("@Parameter_3");
    }

    [Fact]
    public void AppendFormatted_InterpolatedParameter_NullableValueTypes_ShouldHandleNullAndNonNull()
    {
        Int64? id1 = 123L;
        Int64? id2 = null;

        InterpolatedSqlStatement statement = $"SELECT {Parameter(id1)}, {Parameter(id2)}";

        statement.Code
            .Should().Be("SELECT @Id1, @Id2");

        statement.Parameters
            .Should().Contain("@Id1", id1);

        statement.Parameters
            .Should().Contain("@Id2", id2);
    }


    [Fact]
    public void AppendFormatted_InterpolatedParameter_NullValue_ShouldStoreNull()
    {
        const String? nullValue = null;
        InterpolatedSqlStatement statement = $"SELECT {Parameter(nullValue)}";

        statement.Code
            .Should().Be("SELECT @NullValue");

        statement.Parameters
            .Should().Contain("@NullValue", null);
    }

    [Fact]
    public void AppendFormatted_InterpolatedParameter_ReferenceTypes_ShouldHandleNullAndNonNull()
    {
        const String name1 = "Test";
        String? name2 = null;

        InterpolatedSqlStatement statement = $"SELECT {Parameter(name1)}, {Parameter(name2)}";

        statement.Code
            .Should().Be("SELECT @Name1, @Name2");

        statement.Parameters
            .Should().Contain("@Name1", name1);

        statement.Parameters
            .Should().Contain("@Name2", name2);
    }

    [Fact]
    public void AppendFormatted_InterpolatedParameter_ShouldInferNameFromValueExpressionIfPossible()
    {
        const Int64 productId = 101L;
        static Int64 GetProductId() => 102L;

        // ReSharper disable once UnusedParameter.Local
        static Int64 GetProductIdByCategory(String category) => 103L;
        Int64[] productIds = [104L, 105L];

        InterpolatedSqlStatement statement =
            $"""
             SELECT  {Parameter(productId)},
                     {Parameter(GetProductId())},
                     {Parameter(GetProductIdByCategory("Shoes"))},
                     {Parameter(productIds[1])},
                     {Parameter(TestProductId)},
                     {Parameter(new { })}
             """;

        statement.Code
            .Should().Be(
                """
                SELECT  @ProductId,
                        @ProductId2,
                        @ProductIdByCategoryShoes,
                        @ProductIds1,
                        @TestProductId,
                        @Parameter_6
                """
            );

        statement.Parameters
            .Should().Contain("@ProductId", productId);

        statement.Parameters
            .Should().Contain("@ProductId2", GetProductId());

        statement.Parameters
            .Should().Contain("@ProductIdByCategoryShoes", GetProductIdByCategory("Shoes"));

        statement.Parameters
            .Should().Contain("@ProductIds1", productIds[1]);

        statement.Parameters
            .Should().Contain("@TestProductId", TestProductId);
    }

    [Fact]
    public void AppendFormatted_InterpolatedParameter_ShouldStoreParameter()
    {
        var value = Generate.ScalarValue();
        InterpolatedSqlStatement statement = $"SELECT {Parameter(value)}";

        statement.Code
            .Should().Be("SELECT @Value");

        statement.Parameters
            .Should().Contain("@Value", value);
    }

    [Fact]
    public void AppendFormatted_InterpolatedParameter_ShouldSupportComplexExpressions()
    {
        const Double baseDiscount = 0.1;
        var entityIds = Generate.EntityIds(20);

        InterpolatedSqlStatement statement =
            $"""
             SELECT  {Parameter(baseDiscount * 5 / 3)},
                     {Parameter(entityIds.Where(a => a > 5).Select(a => a.ToString()).ToArray()[0])}
             """;

        statement.Code
            .Should().Be(
                """
                SELECT  @BaseDiscount53,
                        @EntityIdsWhereaa5SelectaaToStringToArray0
                """
            );

        statement.Parameters
            .Should().Contain("@BaseDiscount53", baseDiscount * 5 / 3);

        statement.Parameters
            .Should().Contain(
                "@EntityIdsWhereaa5SelectaaToStringToArray0",
                entityIds.Where(a => a > 5).Select(a => a.ToString()).ToArray()[0]
            );
    }

    [Fact]
    public void AppendFormatted_InterpolatedTemporaryTable_ShouldStoreTemporaryTable()
    {
        var entities = Generate.Entities(Generate.SmallNumber());
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        InterpolatedSqlStatement statement =
            $"""
             SELECT Id
             FROM   {TemporaryTable(entities)} Entities
             WHERE  Entities.Id IN (SELECT Value FROM {TemporaryTable(entityIds)})
             """;

        statement.TemporaryTables
            .Should().HaveCount(2);

        var table1 = statement.TemporaryTables[0];

        table1.Name
            .Should().StartWith("#Entities_");

        table1.Values
            .Should().Be(entities);

        table1.ValuesType
            .Should().Be(typeof(Entity));


        var table2 = statement.TemporaryTables[1];

        table2.Name
            .Should().StartWith("#EntityIds_");

        table2.Values
            .Should().Be(entityIds);

        table2.ValuesType
            .Should().Be(typeof(Int64));

        statement.Code
            .Should().Be(
                $"""
                 SELECT Id
                 FROM   {table1.Name} Entities
                 WHERE  Entities.Id IN (SELECT Value FROM {table2.Name})
                 """
            );
    }

    [Fact]
    public void AppendFormatted_InterpolatedTemporaryTable_ValuesExpressionShouldBeUsedToInferTableName()
    {
        List<Int64> entityIds = [101L, 102L];
        static List<Int64> GetEntityIds() => [103L, 104L];

        // ReSharper disable once UnusedParameter.Local
        static List<Int64> GetEntityIdsByCategory(String category) => [105L, 106L];

        InterpolatedSqlStatement statement =
            $"""
             SELECT Value FROM {TemporaryTable(entityIds)}
             UNION
             SELECT Value FROM {TemporaryTable(GetEntityIds())}
             UNION
             SELECT Value FROM {TemporaryTable(GetEntityIdsByCategory("Shoes"))}
             UNION
             SELECT Value FROM {TemporaryTable(this.testEntityIds)}
             """;

        var temporaryTables = statement.TemporaryTables.ToList();

        temporaryTables
            .Should().HaveCount(4);

        temporaryTables[0].Name
            .Should().StartWith("#EntityIds_");

        temporaryTables[0].Values
            .Should().BeEquivalentTo(entityIds);

        temporaryTables[0].ValuesType
            .Should().Be(typeof(Int64));

        temporaryTables[1].Name
            .Should().StartWith("#EntityIds_");

        temporaryTables[1].Values
            .Should().BeEquivalentTo(GetEntityIds());

        temporaryTables[1].ValuesType
            .Should().Be(typeof(Int64));

        temporaryTables[2].Name
            .Should().StartWith("#EntityIdsByCategoryShoes_");

        temporaryTables[2].Values
            .Should().BeEquivalentTo(GetEntityIdsByCategory("Shoes"));

        temporaryTables[2].ValuesType
            .Should().Be(typeof(Int64));

        temporaryTables[3].Name
            .Should().StartWith("#TestEntityIds_");

        temporaryTables[3].Values
            .Should().BeEquivalentTo(this.testEntityIds);

        temporaryTables[3].ValuesType
            .Should().Be(typeof(Int64));

        statement.Code
            .Should().Be(
                $"""
                 SELECT Value FROM {temporaryTables[0].Name}
                 UNION
                 SELECT Value FROM {temporaryTables[1].Name}
                 UNION
                 SELECT Value FROM {temporaryTables[2].Name}
                 UNION
                 SELECT Value FROM {temporaryTables[3].Name}
                 """
            );
    }

    [Fact]
    public void AppendFormatted_MultipleInterpolatedParameters_ShouldStoreParameters()
    {
        const Int64 entityId = 10;
        const String entityName = "Shoes";
        const Boolean isActive = true;

        InterpolatedSqlStatement statement =
            $"""
             SELECT  *
             FROM    Entity
             WHERE   Id = {Parameter(entityId)} AND
                     Name = {Parameter(entityName)} AND
                     IsActive = {Parameter(isActive)}
             """;

        statement.Code
            .Should()
            .Be(
                """
                SELECT  *
                FROM    Entity
                WHERE   Id = @EntityId AND
                        Name = @EntityName AND
                        IsActive = @IsActive
                """
            );

        statement.Parameters
            .Should().Contain("@EntityId", entityId);

        statement.Parameters
            .Should().Contain("@EntityName", entityName);

        statement.Parameters
            .Should().Contain("@IsActive", isActive);
    }

    [Fact]
    public void AppendFormatted_ShouldFormatAndStoreLiteral()
    {
        InterpolatedSqlStatement statement = $"SELECT {123.45,10:N2}, {123.45,-10:N2}";

        statement.Code
            .Should().Be("SELECT     123.45, 123.45    ");
    }

    [Fact]
    public void AppendLiteral_ShouldStoreLiteral()
    {
        InterpolatedSqlStatement statement = $"SELECT 1";

        statement.Code
            .Should().Be("SELECT 1");
    }

    [Fact]
    public void Constructor_Code_Parameters_DuplicateKey_ShouldThrow()
    {
        Invoking(() => new InterpolatedSqlStatement(
                "Code",
                new("Parameter1", "Value1"),
                new("Parameter1", "Value2")
            ))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                "Duplicate parameter name 'Parameter1'. Make sure each parameter name is only used once.*"
            );

        Invoking(() => new InterpolatedSqlStatement(
                "Code",
                new("a", "Value1"),
                new("A", "Value2")
            ))
            .Should().Throw<ArgumentException>()
            .WithMessage(
                "Duplicate parameter name 'A'. Make sure each parameter name is only used once.*"
            );
    }

    [Fact]
    public void Constructor_Code_Parameters_EnumValue_EnumSerializationModeIsIntegers_ShouldSerializeEnumToInteger()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        var enumValue = Generate.Enum();

        var statement = new InterpolatedSqlStatement(
            "Code",
            ("Parameter1", enumValue)
        );

        statement.Parameters
            .Should().Contain("Parameter1", (Int32)enumValue);
    }

    [Fact]
    public void Constructor_Code_Parameters_EnumValue_EnumSerializationModeIsStrings_ShouldSerializeEnumToString()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        var enumValue = Generate.Enum();

        var statement = new InterpolatedSqlStatement(
            "Code",
            ("Parameter1", enumValue)
        );

        statement.Parameters
            .Should().Contain("Parameter1", enumValue.ToString());
    }

    [Fact]
    public void Constructor_Code_Parameters_ShouldStoreCodeAndParameters()
    {
        var statement = new InterpolatedSqlStatement(
            "Code",
            ("Parameter1", "Value1"),
            ("Parameter2", "Value2"),
            ("Parameter3", "Value3")
        );

        statement.Code
            .Should().Be("Code");

        statement.Parameters
            .Should().Contain("Parameter1", "Value1");

        statement.Parameters
            .Should().Contain("Parameter2", "Value2");

        statement.Parameters
            .Should().Contain("Parameter3", "Value3");
    }

    [Fact]
    public void FromString_EmptyString_ShouldCreateEmptyStatement()
    {
        var statement = InterpolatedSqlStatement.FromString(String.Empty);

        statement.Code
            .Should().Be(String.Empty);

        statement.Parameters
            .Should().BeEmpty();

        statement.TemporaryTables
            .Should().BeEmpty();
    }

    [Fact]
    public void FromString_ShouldCreateSqlStatementFromString()
    {
        var statement = InterpolatedSqlStatement.FromString("SELECT 1");

        statement.Code
            .Should().Be("SELECT 1");

        statement.Parameters
            .Should().BeEmpty();

        statement.TemporaryTables
            .Should().BeEmpty();
    }

    [Fact]
    public void ImplicitConversion_Null_ShouldCreateEmptyStatement()
    {
        String? sql = null;

        InterpolatedSqlStatement statement = sql!;

        statement.Code
            .Should().Be("");

        statement.Parameters
            .Should().BeEmpty();

        statement.TemporaryTables
            .Should().BeEmpty();
    }

    [Fact]
    public void ImplicitConversion_ShouldCreateSqlStatementFromString()
    {
        InterpolatedSqlStatement statement = "SELECT 1";

        statement.Code
            .Should().Be("SELECT 1");

        statement.Parameters
            .Should().BeEmpty();

        statement.TemporaryTables
            .Should().BeEmpty();
    }

    [Fact]
    public void Parameters_ShouldGetParameters()
    {
        var value1 = Generate.ScalarValue();
        var value2 = Generate.ScalarValue();
        var value3 = Generate.ScalarValue();

        InterpolatedSqlStatement statement = $"SELECT {Parameter(value1)}, {Parameter(value2)}, {Parameter(value3)}";

        statement.Parameters
            .Should().HaveCount(3);

        statement.Parameters
            .Should().Contain("@Value1", value1);

        statement.Parameters
            .Should().Contain("@Value2", value2);

        statement.Parameters
            .Should().Contain("@Value3", value3);
    }

    [Fact]
    public void TemporaryTables_ShouldGetTemporaryTables()
    {
        var entities = Generate.Entities(Generate.SmallNumber());
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        InterpolatedSqlStatement statement = $"""
                                              SELECT    *
                                              FROM      {TemporaryTable(entities)} TEntity
                                              WHERE     Entity.Id IN (
                                                            SELECT  Value
                                                            FROM    {TemporaryTable(entityIds)}
                                                        )
                                              """;

        statement.TemporaryTables
            .Should().HaveCount(2);

        var temporaryTable1 = statement.TemporaryTables[0];

        temporaryTable1.Name
            .Should().StartWith("#Entities_");

        temporaryTable1.Values
            .Should().Be(entities);

        temporaryTable1.ValuesType
            .Should().Be(typeof(Entity));

        var temporaryTable2 = statement.TemporaryTables[1];

        temporaryTable2.Name
            .Should().StartWith("#EntityIds_");

        temporaryTable2.Values
            .Should().Be(entityIds);

        temporaryTable2.ValuesType
            .Should().Be(typeof(Int64));
    }

    [Fact]
    public void ToString_ShouldReturnStringRepresentationOfStatement()
    {
        var items = new List<Item>
        {
            new(1, "A", TestEnum.Value1),
            new(2, "B", TestEnum.Value2),
            new(3, "C", TestEnum.Value3)
        };

        List<Int32> ids = [1, 2, 3];

        const String name = "B";
        const TestEnum enumValue = TestEnum.Value2;

        InterpolatedSqlStatement statement = $"""
                                              SELECT    *
                                              FROM      {TemporaryTable(items)} TItem
                                              WHERE     TItem.Id IN (
                                                            SELECT  Value
                                                            FROM    {TemporaryTable(ids)}
                                                        )
                                                        AND
                                                        TItem.Name = {Parameter(name)}
                                                        AND
                                                        TItem.Enum = {Parameter(enumValue)}
                                              """;
        statement.TemporaryTables
            .Should().HaveCount(2);

        var itemsTable = statement.TemporaryTables[0];

        itemsTable.Name
            .Should().StartWith("#Items_");

        itemsTable.Values
            .Should().Be(items);

        itemsTable.ValuesType
            .Should().Be(typeof(Item));

        var idsTable = statement.TemporaryTables[1];

        idsTable.Name
            .Should().StartWith("#Ids_");

        idsTable.Values
            .Should().Be(ids);

        idsTable.ValuesType
            .Should().Be(typeof(Int32));

        statement.ToString()
            .Should().Be(
                $$"""
                  SQL Statement

                  Statement Code
                  --------------
                  SELECT    *
                  FROM      {{itemsTable.Name}} TItem
                  WHERE     TItem.Id IN (
                                SELECT  Value
                                FROM    {{idsTable.Name}}
                            )
                            AND
                            TItem.Name = @Name
                            AND
                            TItem.Enum = @EnumValue
                  --------------

                  Statement Parameters
                  --------------------
                  @Name = 'B' (System.String)
                  @EnumValue = 'Value2' (System.String)

                  Statement Temporary Tables
                  --------------------------
                  {{itemsTable.Name}}
                  ----------------------------------------
                  '{"Id":1,"Name":"A","Enum":1}' (RentADeveloper.SqlConnectionPlus.UnitTests.TestData.Item)
                  '{"Id":2,"Name":"B","Enum":2}' (RentADeveloper.SqlConnectionPlus.UnitTests.TestData.Item)
                  '{"Id":3,"Name":"C","Enum":3}' (RentADeveloper.SqlConnectionPlus.UnitTests.TestData.Item)

                  {{idsTable.Name}}
                  --------------------------------------
                  '1' (System.Int32)
                  '2' (System.Int32)
                  '3' (System.Int32)


                  """
            );
    }

    [Fact]
    public void VerifyNullArgumentGuards()
    {
        (String, Object?)[] parameters = [("Parameter1", "Value1")];

        ArgumentNullGuardVerifier.Verify(() => new InterpolatedSqlStatement("SELECT 1", parameters));
    }

    private readonly List<Int64> testEntityIds = [106L, 107L];

    private const Int64 TestProductId = 106L;
}
