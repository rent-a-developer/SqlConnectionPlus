using Bogus;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

/// <summary>
/// Generates test data.
/// </summary>
public static class Generate
{
    /// <summary>
    /// Initializes this type.
    /// </summary>
    static Generate()
    {
        entityFaker = new Faker<Entity>()
            .StrictMode(true)
            .RuleFor(a => a.Id, _ => entityId++)
            .RuleFor(a => a.BooleanValue, a => a.Random.Bool())
            .RuleFor(a => a.ByteValue, a => a.Random.Byte())
            .RuleFor(a => a.CharValue, a => a.Random.Char('A', 'Z'))
            .RuleFor(a => a.DateTimeOffsetValue, a => a.Date.PastOffset())
            .RuleFor(a => a.DateTimeValue, a => a.Date.Past())

            // Limit DecimalValue to 10 digits to the right of the decimal point, because the corresponding column
            // is of the data type DECIMAL(28,10).
            .RuleFor(a => a.DecimalValue, a => Math.Round(a.Random.Decimal(0, 999), 10))
            .RuleFor(a => a.DoubleValue, a => a.Random.Double(0, 999))
            .RuleFor(a => a.EnumValue, a => a.Random.Enum<TestEnum>())
            .RuleFor(a => a.GuidValue, a => a.Random.Guid())
            .RuleFor(a => a.Int16Value, a => a.Random.Short())
            .RuleFor(a => a.Int32Value, a => a.Random.Int())
            .RuleFor(a => a.Int64Value, a => a.Random.Long())
            .RuleFor(a => a.NotMappedProperty, _ => null)
            .RuleFor(a => a.SingleValue, a => a.Random.Float())
            .RuleFor(a => a.StringValue, a => a.Lorem.Sentence())
            .RuleFor(a => a.TimeSpanValue, a => a.Date.Timespan(new TimeSpan(0, 23, 59, 59, 999)));

        entityWithEnumStoredAsIntegerFaker = new Faker<EntityWithEnumStoredAsInteger>()
            .StrictMode(true)
            .RuleFor(a => a.Id, _ => entityId++)
            .RuleFor(a => a.Enum, a => a.Random.Enum<TestEnum>());

        entityWithEnumStoredAsStringFaker = new Faker<EntityWithEnumStoredAsString>()
            .StrictMode(true)
            .RuleFor(a => a.Id, _ => entityId++)
            .RuleFor(a => a.Enum, a => a.Random.Enum<TestEnum>());

        faker = new();
    }

    /// <summary>
    /// Generates the specified number of <see cref="TestData.Entity" /> objects populated with test data.
    /// </summary>
    /// <param name="numberOfEntities">The number of entities to generate.</param>
    /// <returns>
    /// A list containing the specified number of <see cref="TestData.Entity" /> objects populated with test data.
    /// </returns>
    public static List<Entity> Entities(Int32 numberOfEntities) =>
        entityFaker.Generate(numberOfEntities);

    /// <summary>
    /// Generates the specified number of <see cref="TestData.EntityWithEnumStoredAsInteger" /> objects populated
    /// with test data.
    /// </summary>
    /// <param name="numberOfEntities">The number of entities to generate.</param>
    /// <returns>
    /// A list containing the specified number of <see cref="TestData.EntityWithEnumStoredAsInteger" /> objects
    /// populated with test data.
    /// </returns>
    public static List<EntityWithEnumStoredAsInteger> EntitiesWithEnumStoredAsInteger(Int32 numberOfEntities) =>
        entityWithEnumStoredAsIntegerFaker.Generate(numberOfEntities);

    /// <summary>
    /// Generates the specified number of <see cref="TestData.EntityWithEnumStoredAsString" /> objects populated with
    /// test data.
    /// </summary>
    /// <param name="numberOfEntities">The number of entities to generate.</param>
    /// <returns>
    /// A list containing the specified number of <see cref="TestData.EntityWithEnumStoredAsString" /> objects
    /// populated with test data.
    /// </returns>
    public static List<EntityWithEnumStoredAsString> EntitiesWithEnumStoredAsString(Int32 numberOfEntities) =>
        entityWithEnumStoredAsStringFaker.Generate(numberOfEntities);

    /// <summary>
    /// Generates an <see cref="TestData.Entity" /> object populated with test data.
    /// </summary>
    /// <returns>An <see cref="TestData.Entity" /> object populated with test data.</returns>
    public static Entity Entity() =>
        entityFaker.Generate();

    /// <summary>
    /// Generates an entity ID.
    /// </summary>
    /// <returns>An entity ID.</returns>
    public static Int64 EntityId() =>
        entityId++;

    /// <summary>
    /// Generates the specified number of entity IDs.
    /// </summary>
    /// <param name="numberOfIds">The number of entity IDs to generate.</param>
    /// <returns>A list of entity IDs.</returns>
    public static List<Int64> EntityIds(Int32 numberOfIds) =>
        [.. Enumerable.Range(0, numberOfIds).Select(_ => entityId++)];

    /// <summary>
    /// Generates an <see cref="TestData.EntityWithEnumStoredAsInteger" /> object populated with test data.
    /// </summary>
    /// <returns>An <see cref="TestData.EntityWithEnumStoredAsInteger" /> object populated with test data.</returns>
    public static EntityWithEnumStoredAsInteger EntityWithEnumStoredAsInteger() =>
        entityWithEnumStoredAsIntegerFaker.Generate();

    /// <summary>
    /// Generates an <see cref="TestData.EntityWithEnumStoredAsString" /> object populated with test data.
    /// </summary>
    /// <returns>An <see cref="TestData.EntityWithEnumStoredAsString" /> object populated with test data.</returns>
    public static EntityWithEnumStoredAsString EntityWithEnumStoredAsString() =>
        entityWithEnumStoredAsStringFaker.Generate();

    /// <summary>
    /// Generates a random value of the type <see cref="TestEnum" />.
    /// </summary>
    /// <returns>A random value of the type <see cref="TestEnum" />.</returns>
    public static TestEnum Enum() =>
        faker.Random.Enum<TestEnum>();

    /// <summary>
    /// Generates a random character.
    /// </summary>
    /// <returns>A random character.</returns>
    public static Char GenerateCharacter() =>
        faker.Random.Char('A', 'z');

    /// <summary>
    /// Maps <paramref name="entities" /> to a list of <see cref="EntityWithTableAttribute" /> objects containing the
    /// same data.
    /// </summary>
    /// <param name="entities">The entities to map.</param>
    /// <returns>
    /// A list of <see cref="EntityWithTableAttribute" /> containing the same data as <paramref name="entities" />.
    /// </returns>
    public static List<EntityWithTableAttribute> MapToEntitiesWithTableAttribute(IEnumerable<Entity> entities) =>
        [.. entities.Select(MapToEntityWithTableAttribute)];

    /// <summary>
    /// Maps <paramref name="entity" /> to an instance of <see cref="EntityWithTableAttribute" /> containing the same
    /// data.
    /// </summary>
    /// <param name="entity">The entity to map.</param>
    /// <returns>
    /// An instance of <see cref="EntityWithTableAttribute" /> containing the same data as <paramref name="entity" />.
    /// </returns>
    public static EntityWithTableAttribute MapToEntityWithTableAttribute(Entity entity) =>
        new()
        {
            Id = entity.Id,
            BooleanValue = entity.BooleanValue,
            ByteValue = entity.ByteValue,
            CharValue = entity.CharValue,
            DateTimeOffsetValue = entity.DateTimeOffsetValue,
            DateTimeValue = entity.DateTimeValue,
            DecimalValue = entity.DecimalValue,
            DoubleValue = entity.DoubleValue,
            EnumValue = entity.EnumValue,
            GuidValue = entity.GuidValue,
            Int16Value = entity.Int16Value,
            Int32Value = entity.Int32Value,
            Int64Value = entity.Int64Value,
            NotMappedProperty = entity.NotMappedProperty,
            SingleValue = entity.SingleValue,
            StringValue = entity.StringValue,
            TimeSpanValue = entity.TimeSpanValue
        };

    /// <summary>
    /// Generates an array containing the specified number of random integer and <see langword="null" /> values.
    /// </summary>
    /// <param name="numberOfNumbers">
    /// The number of random integer and <see langword="null" /> values to generate.
    /// </param>
    /// <returns>
    /// An array of random integer and <see langword="null" /> values with a length equal to
    /// <paramref name="numberOfNumbers" />.
    /// The array is guaranteed to contain at least one <see langword="null" /> value.
    /// </returns>
    public static Int32?[] NullableNumbers(Int32 numberOfNumbers)
    {
        var result = new Int32?[numberOfNumbers];

        for (int i = 0; i < numberOfNumbers; i++)
        {
            result[i] = faker.Random.Bool() ? faker.Random.Int() : null;
        }

        if (result.All(a => a is not null))
        {
            result[faker.Random.Int(0, numberOfNumbers - 1)] = null;
        }

        return result;
    }

    /// <summary>
    /// Generates an array containing the specified number of random integers.
    /// </summary>
    /// <param name="numberOfNumbers">The number of random integers to generate.</param>
    /// <returns>
    /// An array of random integers with a length equal to <paramref name="numberOfNumbers" />.
    /// </returns>
    public static Int32[] Numbers(Int32 numberOfNumbers) =>
        [.. Enumerable.Range(0, numberOfNumbers).Select(_ => faker.Random.Int())];

    /// <summary>
    /// Generates a random scalar value of one of the following types:
    /// <list type="bullet">
    ///     <item>
    /// <see cref="Boolean" />
    /// </item>
    ///     <item>
    /// <see cref="Byte" />
    /// </item>
    ///     <item>
    /// <see cref="Char" />
    /// </item>
    ///     <item>
    /// <see cref="DateTimeOffset" />
    /// </item>
    ///     <item>
    /// <see cref="DateTime" />
    /// </item>
    ///     <item>
    /// <see cref="Decimal" />
    /// </item>
    ///     <item>
    /// <see cref="Double" />
    /// </item>
    ///     <item>
    /// <see cref="Guid" />
    /// </item>
    ///     <item>
    /// <see cref="Int16" />
    /// </item>
    ///     <item>
    /// <see cref="Int32" />
    /// </item>
    ///     <item>
    /// <see cref="Int64" />
    /// </item>
    ///     <item>
    /// <see cref="Single" />
    /// </item>
    ///     <item>
    /// <see cref="String" />
    /// </item>
    ///     <item>
    /// <see cref="TimeSpan" />
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>A random scalar value.</returns>
    public static Object ScalarValue() =>
        faker.Random.Int(0, 14) switch
        {
            0 => faker.Random.Bool(),
            1 => faker.Random.Byte(),
            2 => faker.Random.Char('A', 'Z'),
            3 => faker.Date.PastOffset(),
            4 => faker.Date.Past(),
            5 => Math.Round(faker.Random.Decimal(0, 999), 10),
            6 => faker.Random.Double(0, 999),
            7 => faker.Random.Guid(),
            8 => faker.Random.Short(),
            9 => faker.Random.Int(),
            10 => faker.Random.Long(),
            11 => faker.Random.Float(),
            12 => faker.Lorem.Sentence(),
            13 => faker.Date.Timespan(new TimeSpan(0, 23, 59, 59, 999)),
            _ => faker.Random.Int()
        };

    /// <summary>
    /// Generates a random number between 3 and 10.
    /// </summary>
    /// <returns>A random number between 3 and 10.</returns>
    public static Int32 SmallNumber() =>
        faker.Random.Int(3, 10);

    /// <summary>
    /// Generates a random string.
    /// </summary>
    /// <returns>A random string.</returns>
    public static String String() =>
        faker.Lorem.Sentence();

    /// <summary>
    /// Creates a copy of <paramref name="entity" /> where all properties except <see cref="Entity.Id" /> have new
    /// values.
    /// </summary>
    /// <param name="entity">The entity for which to create an updated one.</param>
    /// <returns>
    /// A copy of <paramref name="entity" /> where all properties except <see cref="Entity.Id" /> have new values.
    /// </returns>
    public static Entity Update(Entity entity)
    {
        var updatedEntity = Entity() with { Id = entity.Id };

        // For the rare case that all generated values are the same as in the original entity,
        // regenerate until at least one value is different.
        while (entity.Equals(updatedEntity))
        {
            updatedEntity = Entity() with { Id = entity.Id };
        }

        return updatedEntity;
    }

    /// <summary>
    /// Creates a copy of <paramref name="entity" /> where all properties except
    /// <see cref="EntityWithEnumStoredAsInteger.Id" /> have new values.
    /// </summary>
    /// <param name="entity">The entity for which to create an updated one.</param>
    /// <returns>
    /// A copy of <paramref name="entity" /> where all properties except
    /// <see cref="EntityWithEnumStoredAsInteger.Id" /> have new values.
    /// </returns>
    public static EntityWithEnumStoredAsInteger Update(EntityWithEnumStoredAsInteger entity)
    {
        var updatedEntity = new EntityWithEnumStoredAsInteger { Id = entity.Id };

        while (updatedEntity.Enum == entity.Enum)
        {
            updatedEntity.Enum = faker.Random.Enum<TestEnum>();
        }

        return updatedEntity;
    }

    /// <summary>
    /// Creates a copy of <paramref name="entity" /> where all properties except
    /// <see cref="EntityWithEnumStoredAsString.Id" /> have new values.
    /// </summary>
    /// <param name="entity">The entity for which to create an updated one.</param>
    /// <returns>
    /// A copy of <paramref name="entity" /> where all properties except
    /// <see cref="EntityWithEnumStoredAsString.Id" /> have new values.
    /// </returns>
    public static EntityWithEnumStoredAsString Update(EntityWithEnumStoredAsString entity)
    {
        var updatedEntity = new EntityWithEnumStoredAsString { Id = entity.Id };

        while (updatedEntity.Enum == entity.Enum)
        {
            updatedEntity.Enum = faker.Random.Enum<TestEnum>();
        }

        return updatedEntity;
    }

    /// <summary>
    /// Creates a list with copies of <paramref name="entities" /> where all properties except <see cref="Entity.Id" />
    /// have new values.
    /// </summary>
    /// <param name="entities">The entities for which to create updated ones.</param>
    /// <returns>
    /// A list with copies of <paramref name="entities" /> where all properties except <see cref="Entity.Id" /> have
    /// new values.
    /// </returns>
    public static List<Entity> Updates(List<Entity> entities) =>
        [.. entities.Select(Update)];

    /// <summary>
    /// Creates a list with copies of <paramref name="entities" /> where all properties except
    /// <see cref="EntityWithEnumStoredAsInteger.Id" /> have new values.
    /// </summary>
    /// <param name="entities">The entities for which to create updated ones.</param>
    /// <returns>
    /// A list with copies of <paramref name="entities" /> where all properties except
    /// <see cref="EntityWithEnumStoredAsInteger.Id" /> have new values.
    /// </returns>
    public static List<EntityWithEnumStoredAsInteger> Updates(List<EntityWithEnumStoredAsInteger> entities) =>
        [.. entities.Select(Update)];

    /// <summary>
    /// Creates a list with copies of <paramref name="entities" /> where all properties except
    /// <see cref="EntityWithEnumStoredAsString.Id" /> have new values.
    /// </summary>
    /// <param name="entities">The entities for which to create updated ones.</param>
    /// <returns>
    /// A list with copies of <paramref name="entities" /> where all properties except
    /// <see cref="EntityWithEnumStoredAsString.Id" /> have new values.
    /// </returns>
    public static List<EntityWithEnumStoredAsString> Updates(List<EntityWithEnumStoredAsString> entities) =>
        [.. entities.Select(Update)];

    private static readonly Faker<Entity> entityFaker;
    private static readonly Faker<EntityWithEnumStoredAsInteger> entityWithEnumStoredAsIntegerFaker;
    private static readonly Faker<EntityWithEnumStoredAsString> entityWithEnumStoredAsStringFaker;
    private static readonly Faker faker;
    private static Int64 entityId = 1;
}
