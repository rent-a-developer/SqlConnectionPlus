namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;

/// <summary>
/// Manages the test database.
/// </summary>
public static class TestDatabaseManager
{
    static TestDatabaseManager()
    {
        var connectionStringFromEnvironmentVariable = Environment.GetEnvironmentVariable("ConnectionString");

        if (String.IsNullOrWhiteSpace(connectionStringFromEnvironmentVariable))
        {
            throw new InvalidOperationException("The environment variable 'ConnectionString' is not set!");
        }

        Console.Out.WriteLine("Using the following connection string for database tests:");
        Console.Out.WriteLine(connectionStringFromEnvironmentVariable);

        connectionString = connectionStringFromEnvironmentVariable;
    }

    /// <summary>
    /// Creates a new connection to the test database.
    /// </summary>
    /// <returns>A new connection to the test database.</returns>
    public static SqlConnection CreateConnection()
    {
        var connection = new SqlConnection(connectionString);
        connection.Open();
        connection.ChangeDatabase(TestDatabaseName);
        return connection;
    }

    /// <summary>
    /// Resets the test database to a clean state.
    /// If the test database does not exist already, it will be created.
    /// </summary>
    public static void ResetDatabase()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var existsDatabase = connection.ExecuteScalar<Boolean>(
            $"SELECT CASE WHEN DB_ID('{TestDatabaseName}') IS NOT NULL THEN 1 ELSE 0 END"
        );

        if (!existsDatabase)
        {
            // Make sure the test database has a different collation than tempDB,
            // so we can test whether text columns of temporary tables for SQL statements are created with the correct
            // collation.
            connection.ExecuteNonQuery($"CREATE DATABASE [{TestDatabaseName}] COLLATE {TestDatabaseCollation}");

            // Wait for the database to be created.
            Thread.Sleep(1000);

            connection.ExecuteNonQuery($"ALTER DATABASE [{TestDatabaseName}] SET READ_COMMITTED_SNAPSHOT ON");

            connection.ChangeDatabase(TestDatabaseName);

            connection.ExecuteNonQuery(
                """
                CREATE TABLE Entity
                (
                    Id BIGINT NOT NULL PRIMARY KEY,
                    BooleanValue BIT,
                    ByteValue TINYINT,
                    CharValue CHAR(1),
                    DateTimeOffsetValue DATETIMEOFFSET,
                    DateTimeValue DATETIME2,
                    DecimalValue DECIMAL(28,10),
                    DoubleValue FLOAT,
                    EnumValue NVARCHAR(200),
                    GuidValue UNIQUEIDENTIFIER,
                    Int16Value SMALLINT,
                    Int32Value INT,
                    Int64Value BIGINT,
                    SingleValue REAL,
                    StringValue NVARCHAR(MAX),
                    TimeSpanValue TIME
                );
                """
            );

            connection.ExecuteNonQuery(
                """
                CREATE TABLE EntityWithEnumStoredAsString
                (
                    Id BIGINT NOT NULL PRIMARY KEY,
                    Enum NVARCHAR(200) NULL
                );
                """
            );

            connection.ExecuteNonQuery(
                """
                CREATE TABLE EntityWithEnumStoredAsInteger
                (
                    Id BIGINT NOT NULL PRIMARY KEY,
                    Enum INT NULL
                );
                """
            );

            connection.ExecuteNonQuery(
                """
                CREATE PROCEDURE GetEntities
                AS
                BEGIN
                	SELECT * FROM Entity
                END
                """
            );

            connection.ExecuteNonQuery(
                """
                CREATE PROCEDURE GetEntityIds
                AS
                BEGIN
                	SELECT Id FROM Entity
                END
                """
            );

            connection.ExecuteNonQuery(
                """
                CREATE PROCEDURE GetEntityIdsAndStringValues
                AS
                BEGIN
                	SELECT Id, StringValue FROM Entity
                END
                """
            );

            connection.ExecuteNonQuery(
                """
                CREATE PROCEDURE GetEntityIdsAndStringValuesAsXml
                AS
                BEGIN
                	SELECT Id, StringValue FROM Entity FOR XML PATH
                END
                """
            );

            connection.ExecuteNonQuery(
                """
                CREATE PROCEDURE GetFirstEntityId
                AS
                BEGIN
                	SELECT TOP 1 Id FROM Entity
                END
                """
            );

            connection.ExecuteNonQuery(
                """
                CREATE PROCEDURE DeleteAllEntities
                AS
                BEGIN
                	DELETE FROM Entity
                END
                """
            );
        }
        else
        {
            connection.ChangeDatabase(TestDatabaseName);
        }

        connection.ExecuteNonQuery("TRUNCATE TABLE Entity");
        connection.ExecuteNonQuery("TRUNCATE TABLE EntityWithEnumStoredAsString");
        connection.ExecuteNonQuery("TRUNCATE TABLE EntityWithEnumStoredAsInteger");
    }

    /// <summary>
    /// The collation of the test database.
    /// </summary>
    public const String TestDatabaseCollation = "Latin1_General_CI_AS";

    /// <summary>
    /// The name of the test database.
    /// </summary>
    public const String TestDatabaseName = "SqlConnectionPlusTests";

    private static readonly String connectionString;
}
