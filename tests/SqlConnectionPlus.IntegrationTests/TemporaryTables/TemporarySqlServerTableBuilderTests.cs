using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.TemporaryTables;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.TemporaryTables;

public class TemporarySqlServerTableBuilderTests : DatabaseTestsBase
{
    [Fact]
    public void BuildTemporaryTable_ComplexObjects_EmptyList_ShouldCreateEmptyTable()
    {
        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Objects",
            new List<Entity>(),
            typeof(Entity),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.Connection.Exists(
                "SELECT 1 FROM #Objects",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeFalse();
    }

    [Fact]
    public void BuildTemporaryTable_ComplexObjects_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Objects",
            this.entitiesWithEnumProperty,
            typeof(EntityWithEnumProperty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.GetDataTypeAndMaxLengthOfColumnOfTemporaryTable("#Objects", "Enum")
            .Should().Be(("int", 4));

        using var reader = this.Connection.ExecuteReader(
            "SELECT Enum FROM #Objects",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.GetFieldType(0)
            .Should().Be(typeof(Int32));

        reader.Read();
        reader.GetInt32(0)
            .Should().Be((Int32)this.entitiesWithEnumProperty[0].Enum);

        reader.Read();
        reader.GetInt32(0)
            .Should().Be((Int32)this.entitiesWithEnumProperty[1].Enum);

        reader.Read();
        reader.GetInt32(0)
            .Should().Be((Int32)this.entitiesWithEnumProperty[2].Enum);
    }

    [Fact]
    public void BuildTemporaryTable_ComplexObjects_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Objects",
            this.entitiesWithEnumProperty,
            typeof(EntityWithEnumProperty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.GetDataTypeAndMaxLengthOfColumnOfTemporaryTable("#Objects", "Enum")
            .Should().Be(("nvarchar", 400));

        using var reader = this.Connection.ExecuteReader(
            "SELECT Enum FROM #Objects",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.GetFieldType(0)
            .Should().Be(typeof(String));

        reader.Read();
        reader.GetString(0)
            .Should().Be(this.entitiesWithEnumProperty[0].Enum.ToString());

        reader.Read();
        reader.GetString(0)
            .Should().Be(this.entitiesWithEnumProperty[1].Enum.ToString());

        reader.Read();
        reader.GetString(0)
            .Should().Be(this.entitiesWithEnumProperty[2].Enum.ToString());
    }

    [Fact]
    public void
        BuildTemporaryTable_ComplexObjects_EnumSerializationModeIsStrings_ShouldUseCollationOfDatabaseForEnumColumns()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Objects",
            this.entitiesWithEnumProperty,
            typeof(EntityWithEnumProperty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var columnCollation = this.GetCollationOfTemporaryTableColumn("#Objects", "Enum");

        columnCollation
            .Should().Be(TestDatabaseManager.TestDatabaseCollation);
    }

    [Fact]
    public void BuildTemporaryTable_ComplexObjects_ShouldCreateMultiColumnTable()
    {
        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Objects",
            this.temporaryTableTestItems,
            typeof(TemporaryTableTestItem),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.Connection.QueryEntities<TemporaryTableTestItem>(
                "SELECT * FROM #Objects",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(this.temporaryTableTestItems);
    }

    [Fact]
    public void BuildTemporaryTable_ComplexObjects_ShouldUseCollationOfDatabaseForTextColumns()
    {
        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Objects",
            this.entitiesWithStringProperty,
            typeof(EntityWithStringProperty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var columnCollation = this.GetCollationOfTemporaryTableColumn("#Objects", "String");

        columnCollation
            .Should().Be(TestDatabaseManager.TestDatabaseCollation);
    }

    [Fact]
    public void BuildTemporaryTable_ComplexObjects_WithNullables_ShouldHandleNullValues()
    {
        var itemsWithNulls = new List<TemporaryTableTestItem>
        {
            new()
            {
                Boolean = null,
                Bytes = null,
                Char = null,
                Decimal = null,
                Double = null,
                Single = null,
                Int16 = null,
                Int32 = null,
                Int64 = null,
                DateTime = null,
                DateTimeOffset = null,
                String = null,
                TimeSpan = null,
                Guid = null
            }
        };

        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Objects",
            itemsWithNulls,
            typeof(TemporaryTableTestItem),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.Connection.QueryEntities<TemporaryTableTestItem>(
                "SELECT * FROM #Objects",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(itemsWithNulls);
    }

    [Fact]
    public void BuildTemporaryTable_ScalarValues_EmptyList_ShouldCreateEmptyTable()
    {
        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Values",
            new List<Int32>(),
            typeof(Int32),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.Connection.Exists(
                "SELECT 1 FROM #Values",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeFalse();
    }

    [Fact]
    public void BuildTemporaryTable_ScalarValues_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Values",
            this.enumValues,
            typeof(TestEnum),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.GetDataTypeAndMaxLengthOfColumnOfTemporaryTable("#Values", "Value")
            .Should().Be(("int", 4));

        using var reader = this.Connection.ExecuteReader(
            "SELECT Value FROM #Values",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.GetFieldType(0)
            .Should().Be(typeof(Int32));

        reader.Read();
        reader.GetInt32(0)
            .Should().Be((Int32)this.enumValues[0]);

        reader.Read();
        reader.GetInt32(0)
            .Should().Be((Int32)this.enumValues[1]);

        reader.Read();
        reader.GetInt32(0)
            .Should().Be((Int32)this.enumValues[2]);
    }

    [Fact]
    public void BuildTemporaryTable_ScalarValues_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Values",
            this.enumValues,
            typeof(TestEnum),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.GetDataTypeAndMaxLengthOfColumnOfTemporaryTable("#Values", "Value")
            .Should().Be(("nvarchar", 400));

        using var reader = this.Connection.ExecuteReader(
            "SELECT Value FROM #Values",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.GetFieldType(0)
            .Should().Be(typeof(String));

        reader.Read();
        reader.GetString(0)
            .Should().Be(this.enumValues[0].ToString());

        reader.Read();
        reader.GetString(0)
            .Should().Be(this.enumValues[1].ToString());

        reader.Read();
        reader.GetString(0)
            .Should().Be(this.enumValues[2].ToString());
    }

    [Fact]
    public void
        BuildTemporaryTable_ScalarValues_EnumSerializationModeIsStrings_ShouldUseCollationOfDatabaseForEnumColumns()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Values",
            this.enumValues,
            typeof(TestEnum),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var columnCollation = this.GetCollationOfTemporaryTableColumn("#Values", "Value");

        columnCollation
            .Should().Be(TestDatabaseManager.TestDatabaseCollation);
    }

    [Fact]
    public void BuildTemporaryTable_ScalarValues_ShouldCreateSingleColumnTable()
    {
        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Values",
            this.integerValues,
            typeof(Int32),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.Connection.QueryScalars<Int32>(
                "SELECT Value FROM #Values",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(this.integerValues);
    }

    [Fact]
    public void BuildTemporaryTable_ScalarValues_ShouldUseCollationOfDatabaseForTextColumns()
    {
        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Values",
            this.stringValues,
            typeof(String),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var columnCollation = this.GetCollationOfTemporaryTableColumn("#Values", "Value");

        columnCollation
            .Should().Be(TestDatabaseManager.TestDatabaseCollation);
    }

    [Fact]
    public void BuildTemporaryTable_ScalarValuesWithNullValues_ShouldHandleNullValues()
    {
        using var table = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#NullValues",
            this.nullValues,
            typeof(Int32?),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.Connection.QueryScalars<Int32?>(
                "SELECT Value FROM #NullValues",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo(this.nullValues);
    }

    [Fact]
    public void BuildTemporaryTable_ShouldReturnDisposerThatDropsTable()
    {
        var disposer = TemporarySqlServerTableBuilder.BuildTemporaryTable(
            this.Connection,
            null,
            "#Values",
            this.integerValues,
            typeof(Int32),
            cancellationToken: TestContext.Current.CancellationToken
        );

        this.ExistsTemporaryTable("#Values")
            .Should().BeTrue();

        disposer.Dispose();

        this.ExistsTemporaryTable("#Values")
            .Should().BeFalse();
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ComplexObjects_EmptyList_ShouldCreateEmptyTable()
    {
        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Objects",
            new List<Entity>(),
            typeof(Entity),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.ExistsAsync(
                "SELECT 1 FROM #Objects",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        BuildTemporaryTableAsync_ComplexObjects_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Objects",
            this.entitiesWithEnumProperty,
            typeof(EntityWithEnumProperty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.GetDataTypeAndMaxLengthOfColumnOfTemporaryTableAsync("#Objects", "Enum"))
            .Should().Be(("int", 4));

        using var reader = await this.Connection.ExecuteReaderAsync(
            "SELECT Enum FROM #Objects",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.GetFieldType(0)
            .Should().Be(typeof(Int32));

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetInt32(0)
            .Should().Be((Int32)this.entitiesWithEnumProperty[0].Enum);

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetInt32(0)
            .Should().Be((Int32)this.entitiesWithEnumProperty[1].Enum);

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetInt32(0)
            .Should().Be((Int32)this.entitiesWithEnumProperty[2].Enum);
    }

    [Fact]
    public async Task
        BuildTemporaryTableAsync_ComplexObjects_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Objects",
            this.entitiesWithEnumProperty,
            typeof(EntityWithEnumProperty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.GetDataTypeAndMaxLengthOfColumnOfTemporaryTableAsync("#Objects", "Enum"))
            .Should().Be(("nvarchar", 400));

        using var reader = await this.Connection.ExecuteReaderAsync(
            "SELECT Enum FROM #Objects",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.GetFieldType(0)
            .Should().Be(typeof(String));

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetString(0)
            .Should().Be(this.entitiesWithEnumProperty[0].Enum.ToString());

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetString(0)
            .Should().Be(this.entitiesWithEnumProperty[1].Enum.ToString());

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetString(0)
            .Should().Be(this.entitiesWithEnumProperty[2].Enum.ToString());
    }

    [Fact]
    public async Task
        BuildTemporaryTableAsync_ComplexObjects_EnumSerializationModeIsStrings_ShouldUseCollationOfDatabaseForEnumColumns()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Objects",
            this.entitiesWithEnumProperty,
            typeof(EntityWithEnumProperty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var columnCollation = await this.GetCollationOfTemporaryTableColumnAsync("#Objects", "Enum");

        columnCollation
            .Should().Be(TestDatabaseManager.TestDatabaseCollation);
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ComplexObjects_ShouldCreateMultiColumnTable()
    {
        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Objects",
            this.temporaryTableTestItems,
            typeof(TemporaryTableTestItem),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryEntitiesAsync<TemporaryTableTestItem>(
                "SELECT * FROM #Objects", cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(this.temporaryTableTestItems);
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ComplexObjects_ShouldUseCollationOfDatabaseForTextColumns()
    {
        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Objects",
            this.entitiesWithStringProperty,
            typeof(EntityWithStringProperty),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var columnCollation = await this.GetCollationOfTemporaryTableColumnAsync("#Objects", "String");

        columnCollation
            .Should().Be(TestDatabaseManager.TestDatabaseCollation);
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ComplexObjects_WithNullables_ShouldHandleNullValues()
    {
        var itemsWithNulls = new List<TemporaryTableTestItem>
        {
            new()
            {
                Boolean = null,
                Bytes = null,
                Char = null,
                Decimal = null,
                Double = null,
                Single = null,
                Int16 = null,
                Int32 = null,
                Int64 = null,
                DateTime = null,
                DateTimeOffset = null,
                String = null,
                TimeSpan = null,
                Guid = null
            }
        };

        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Objects",
            itemsWithNulls,
            typeof(TemporaryTableTestItem),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryEntitiesAsync<TemporaryTableTestItem>(
                "SELECT * FROM #Objects",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(itemsWithNulls);
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ScalarValues_EmptyList_ShouldCreateEmptyTable()
    {
        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Values",
            new List<Int32>(),
            typeof(Int32),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.ExistsAsync(
                "SELECT 1 FROM #Values",
                cancellationToken: TestContext.Current.CancellationToken
            ))
            .Should().BeFalse();
    }

    [Fact]
    public async Task
        BuildTemporaryTableAsync_ScalarValues_EnumSerializationModeIsIntegers_ShouldStoreEnumValuesAsIntegers()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;

        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Values",
            this.enumValues,
            typeof(TestEnum),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.GetDataTypeAndMaxLengthOfColumnOfTemporaryTableAsync("#Values", "Value"))
            .Should().Be(("int", 4));

        using var reader = await this.Connection.ExecuteReaderAsync(
            "SELECT Value FROM #Values",
            cancellationToken: TestContext.Current.CancellationToken
        );


        reader.GetFieldType(0)
            .Should().Be(typeof(Int32));

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetInt32(0)
            .Should().Be((Int32)this.enumValues[0]);

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetInt32(0)
            .Should().Be((Int32)this.enumValues[1]);

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetInt32(0)
            .Should().Be((Int32)this.enumValues[2]);
    }

    [Fact]
    public async Task
        BuildTemporaryTableAsync_ScalarValues_EnumSerializationModeIsStrings_ShouldStoreEnumValuesAsStrings()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Values",
            this.enumValues,
            typeof(TestEnum),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.GetDataTypeAndMaxLengthOfColumnOfTemporaryTableAsync("#Values", "Value"))
            .Should().Be(("nvarchar", 400));

        using var reader = await this.Connection.ExecuteReaderAsync(
            "SELECT Value FROM #Values",
            cancellationToken: TestContext.Current.CancellationToken
        );

        reader.GetFieldType(0)
            .Should().Be(typeof(String));

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetString(0)
            .Should().Be(this.enumValues[0].ToString());

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetString(0)
            .Should().Be(this.enumValues[1].ToString());

        await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.GetString(0)
            .Should().Be(this.enumValues[2].ToString());
    }

    [Fact]
    public async Task
        BuildTemporaryTableAsync_ScalarValues_EnumSerializationModeIsStrings_ShouldUseCollationOfDatabaseForEnumColumns()
    {
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;

        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Values",
            this.enumValues,
            typeof(TestEnum),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var columnCollation = await this.GetCollationOfTemporaryTableColumnAsync("#Values", "Value");

        columnCollation
            .Should().Be(TestDatabaseManager.TestDatabaseCollation);
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ScalarValues_ShouldCreateSingleColumnTable()
    {
        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Values",
            this.integerValues,
            typeof(Int32),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryScalarsAsync<Int32>(
                "SELECT Value FROM #Values",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(this.integerValues);
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ScalarValues_ShouldUseCollationOfDatabaseForTextColumns()
    {
        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Values",
            this.stringValues,
            typeof(String),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var columnCollation = await this.GetCollationOfTemporaryTableColumnAsync("#Values", "Value");

        columnCollation
            .Should().Be(TestDatabaseManager.TestDatabaseCollation);
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ScalarValuesWithNullValues_ShouldHandleNullValues()
    {
        using var table = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#NullValues",
            this.nullValues,
            typeof(Int32?),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.Connection.QueryScalarsAsync<Int32?>(
                "SELECT Value FROM #NullValues",
                cancellationToken: TestContext.Current.CancellationToken
            ).ToListAsync(TestContext.Current.CancellationToken))
            .Should().BeEquivalentTo(this.nullValues);
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_ShouldReturnDisposerThatDropsTableAsync()
    {
        var disposer = await TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(
            this.Connection,
            null,
            "#Values",
            this.integerValues,
            typeof(Int32),
            cancellationToken: TestContext.Current.CancellationToken
        );

        (await this.ExistsTemporaryTableAsync("#Values"))
            .Should().BeTrue();

        await disposer.DisposeAsync();

        (await this.ExistsTemporaryTableAsync("#Values"))
            .Should().BeFalse();
    }

    private readonly List<EntityWithEnumProperty> entitiesWithEnumProperty =
    [
        new() { Enum = TestEnum.Value1 },
        new() { Enum = TestEnum.Value2 },
        new() { Enum = TestEnum.Value3 }
    ];

    private readonly List<EntityWithStringProperty> entitiesWithStringProperty =
    [
        new() { String = "A" }
    ];

    private readonly List<TestEnum> enumValues =
    [
        TestEnum.Value1,
        TestEnum.Value2,
        TestEnum.Value3
    ];

    private readonly List<Int32> integerValues =
    [
        1, 2, 3, 4, 5
    ];

    private readonly List<Int32?> nullValues = [null, null, null];

    private readonly List<String> stringValues =
    [
        "A", "B", "C"
    ];

    private readonly List<TemporaryTableTestItem> temporaryTableTestItems =
    [
        new()
        {
            Boolean = true,
            Bytes = [1, 2, 3, 4, 5],
            Char = 'A',
            Decimal = 123.45M,
            Double = 123.45,
            Single = 123.45F,
            Int16 = 123,
            Int32 = 123456,
            Int64 = 1234567890,
            DateTime = new(2025, 12, 30, 23, 59, 59),
            DateTimeOffset = new(2025, 12, 30, 23, 59, 59, TimeSpan.FromHours(1)),
            String = "Test String 1",
            TimeSpan = new(1, 2, 3),
            Guid = Guid.NewGuid()
        },

        new()
        {
            Boolean = false,
            Bytes = [6, 7, 8],
            Char = 'B',
            Decimal = 223.45M,
            Double = 223.45,
            Single = 223.45F,
            Int16 = 223,
            Int32 = 223456,
            Int64 = 2234567890,
            DateTime = new(2025, 12, 31, 23, 59, 59),
            DateTimeOffset = new(2025, 12, 31, 23, 59, 59, TimeSpan.FromHours(2)),
            String = "Test String 2",
            TimeSpan = new(4, 5, 6),
            Guid = Guid.NewGuid()
        }
    ];
}
