using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.UnitTests;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

/// <summary>
/// Base class for integration tests that need an actual SQL Server database connection.
/// </summary>
public abstract class DatabaseTestsBase : TestsBase, IDisposable, IAsyncDisposable
{
    /// <inheritdoc />
    protected DatabaseTestsBase()
    {
        this.SqlCommandFactory = new();
        Dependencies.SqlCommandFactory = this.SqlCommandFactory;

        TestDatabaseManager.ResetDatabase();
        this.Connection = TestDatabaseManager.CreateConnection();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Connection.Close();
        this.Connection.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await this.Connection.CloseAsync();
        await this.Connection.DisposeAsync();
    }

    /// <summary>
    /// The connection to the test database.
    /// </summary>
    protected SqlConnection Connection { get; }

    /// <summary>
    /// Determines whether an entity with the specified ID exists in the test database.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>
    /// <see langword="true" /> if an entity with the specified ID exists in the test database;
    /// otherwise, <see langword="false" />.
    /// </returns>
    protected Boolean ExistsEntityById(Int64 id, SqlTransaction? transaction = null) =>
        this.Connection.Exists(
            $"SELECT 1 FROM Entity WHERE Id = {Parameter(id)}",
            transaction: transaction,
            cancellationToken: TestContext.Current.CancellationToken
        );

    /// <summary>
    /// Asynchronously determines whether an entity with the specified ID exists in the test database.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain <see langword="true" /> if an entity with the specified ID
    /// exists in the test database; otherwise, <see langword="false" />.
    /// </returns>
    protected Task<Boolean> ExistsEntityByIdAsync(Int64 id, SqlTransaction? transaction = null) =>
        this.Connection.ExistsAsync(
            $"SELECT 1 FROM Entity WHERE Id = {Parameter(id)}",
            transaction: transaction,
            cancellationToken: TestContext.Current.CancellationToken
        );

    /// <summary>
    /// Determines whether a temporary table with the specified name exists in the test database.
    /// </summary>
    /// <param name="tableName">The name of the temporary table to check for existence.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>
    /// <see langword="true" /> if a temporary table with the specified name exists in the test database;
    /// otherwise, <see langword="false" />.
    /// </returns>
    protected Boolean ExistsTemporaryTable(String tableName, SqlTransaction? transaction = null) =>
        this.Connection.ExecuteScalar<Boolean>(
            $"IF OBJECT_ID('tempdb..{tableName}', 'U') IS NOT NULL SELECT 1 ELSE SELECT 0",
            transaction,
            cancellationToken: TestContext.Current.CancellationToken
        );

    /// <summary>
    /// Asynchronously determines whether a temporary table with the specified name exists in the test database.
    /// </summary>
    /// <param name="tableName">The name of the temporary table to check for existence.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain <see langword="true" /> if the temporary table exists in the
    /// test database; otherwise, <see langword="false" />.
    /// </returns>
    protected Task<Boolean> ExistsTemporaryTableAsync(String tableName, SqlTransaction? transaction = null) =>
        this.Connection.ExecuteScalarAsync<Boolean>(
            $"IF OBJECT_ID('tempdb..{tableName}', 'U') IS NOT NULL SELECT 1 ELSE SELECT 0",
            transaction,
            cancellationToken: TestContext.Current.CancellationToken
        );

    /// <summary>
    /// Gets the collation of the specified column of the specified temporary table.
    /// </summary>
    /// <param name="temporaryTableName">The name of the temporary table that contains the specified column.</param>
    /// <param name="columnName">The name of the column of which to get the collation.</param>
    /// <returns>The collation of the specified column of the specified temporary table.</returns>
    protected String GetCollationOfTemporaryTableColumn(String temporaryTableName, String columnName) =>
        this.Connection.ExecuteScalar<String>(
            $"""
             SELECT	C.collation_name AS CollationName
             FROM	tempdb.sys.columns C
             WHERE	c.object_id = OBJECT_ID('tempdb..{temporaryTableName}') AND C.name = '{columnName}'
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

    /// <summary>
    /// Asynchronously gets the collation of the specified column of the specified temporary table.
    /// </summary>
    /// <param name="temporaryTableName">The name of the temporary table that contains the specified column.</param>
    /// <param name="columnName">The name of the column of which to get the collation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will be the collation of the specified column of the specified
    /// temporary table.
    /// </returns>
    protected Task<String> GetCollationOfTemporaryTableColumnAsync(String temporaryTableName, String columnName) =>
        this.Connection.ExecuteScalarAsync<String>(
            $"""
             SELECT	C.collation_name AS CollationName
             FROM	tempdb.sys.columns C
             WHERE	c.object_id = OBJECT_ID('tempdb..{temporaryTableName}') AND C.name = '{columnName}'
             """,
            cancellationToken: TestContext.Current.CancellationToken
        );

    /// <summary>
    /// Gets the data type and the maximum length of the specified column of the specified temporary table.
    /// </summary>
    /// <param name="temporaryTableName">The name of the temporary table that contains the specified column.</param>
    /// <param name="columnName">The name of the column of which to get the data type and maximum length.</param>
    /// <returns>
    /// The data type and the maximum length of the specified column of the specified temporary table.
    /// </returns>
    protected (String DataType, Int16 MaxLength) GetDataTypeAndMaxLengthOfColumnOfTemporaryTable(
        String temporaryTableName,
        String columnName
    ) =>
        this.Connection.QueryTuples<(String DataType, Int16 MaxLength)>(
            $"""
             SELECT  t.name AS DataType,c.max_length AS MaxLength
             FROM    tempdb.sys.columns c
             JOIN    tempdb.sys.types t ON c.user_type_id = t.user_type_id
             WHERE   c.object_id = OBJECT_ID('tempdb..{temporaryTableName}') AND c.name = '{columnName}'
             """,
            cancellationToken: TestContext.Current.CancellationToken
        ).First();

    /// <summary>
    /// Asynchronously gets the data type and the maximum length of the specified column of the specified temporary
    /// table.
    /// </summary>
    /// <param name="temporaryTableName">The name of the temporary table that contains the specified column.</param>
    /// <param name="columnName">The name of the column of which to get the data type and maximum length.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="ValueTask{TResult}.Result" /> will contain the data type and the maximum length of the specified
    /// column of the specified temporary table.
    /// </returns>
    protected ValueTask<(String DataType, Int16 MaxLength)> GetDataTypeAndMaxLengthOfColumnOfTemporaryTableAsync(
        String temporaryTableName,
        String columnName
    ) =>
        this.Connection.QueryTuplesAsync<(String DataType, Int16 MaxLength)>(
            $"""
             SELECT  t.name AS DataType,c.max_length AS MaxLength
             FROM    tempdb.sys.columns c
             JOIN    tempdb.sys.types t ON c.user_type_id = t.user_type_id
             WHERE   c.object_id = OBJECT_ID('tempdb..{temporaryTableName}') AND c.name = '{columnName}'
             """,
            cancellationToken: TestContext.Current.CancellationToken
        ).FirstAsync(TestContext.Current.CancellationToken);

    /// <summary>
    /// Creates the specified number of entities and inserts them into the test database.
    /// </summary>
    /// <param name="numberOfEntities">The number of entities to create and insert.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>The entities that were created and inserted.</returns>
    protected List<Entity> InsertNewEntities(Int32 numberOfEntities, SqlTransaction? transaction = null)
    {
        var entities = Generate.Entities(numberOfEntities);

        this.Connection.InsertEntities(entities, transaction, cancellationToken: TestContext.Current.CancellationToken);

        foreach (var entity in entities)
        {
            // Verify that the entity has been inserted:
            this.ExistsEntityById(entity.Id, transaction)
                .Should().BeTrue();
        }

        return entities;
    }

    /// <summary>
    /// Asynchronously creates the specified number of entities and inserts them into the test database.
    /// </summary>
    /// <param name="numberOfEntities">The number of entities to create and insert.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain the entities that were created and inserted.
    /// </returns>
    protected async Task<List<Entity>> InsertNewEntitiesAsync(
        Int32 numberOfEntities,
        SqlTransaction? transaction = null
    )
    {
        var entities = Generate.Entities(numberOfEntities);

        await this.Connection.InsertEntitiesAsync(
            entities,
            transaction,
            cancellationToken: TestContext.Current.CancellationToken
        );

        foreach (var entity in entities)
        {
            // Verify that the entity has been inserted:
            (await this.ExistsEntityByIdAsync(entity.Id, transaction))
                .Should().BeTrue();
        }

        return entities;
    }

    /// <summary>
    /// Creates an entity and inserts it into the test database.
    /// </summary>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>The entity that was created and inserted.</returns>
    protected Entity InsertNewEntity(SqlTransaction? transaction = null)
    {
        var entity = Generate.Entity();

        this.Connection.InsertEntity(entity, transaction, cancellationToken: TestContext.Current.CancellationToken);

        // Verify that the entity has been inserted:
        this.ExistsEntityById(entity.Id, transaction)
            .Should().BeTrue();

        return entity;
    }

    /// <summary>
    /// Asynchronously creates an entity and inserts it into the test database.
    /// </summary>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain the entity that was created and inserted.
    /// </returns>
    protected async Task<Entity> InsertNewEntityAsync(SqlTransaction? transaction = null)
    {
        var entity = Generate.Entity();

        await this.Connection.InsertEntityAsync(
            entity,
            transaction,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Verify that the entity has been inserted:
        (await this.ExistsEntityByIdAsync(entity.Id, transaction))
            .Should().BeTrue();

        return entity;
    }

    /// <summary>
    /// Creates a <see cref="CancellationToken" /> that is cancelled after 100 milliseconds.
    /// </summary>
    /// <returns>The created <see cref="CancellationToken" />.</returns>
    protected static CancellationToken CreateCancellationTokenThatIsCancelledAfter100Milliseconds()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(100);
        return cancellationTokenSource.Token;
    }

    /// <summary>
    /// The SQL command factory used for testing cancellation of SQL statements.
    /// </summary>
    protected readonly DelaySqlCommandFactory SqlCommandFactory;
}
