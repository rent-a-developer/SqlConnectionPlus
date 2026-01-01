using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using RentADeveloper.SqlConnectionPlus.Readers;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Readers;

public class DisposeSignalingDataReaderDecoratorTests : TestsBase
{
    public DisposeSignalingDataReaderDecoratorTests()
    {
        this.decoratedReader = Substitute.For<DbDataReader>();
        this.decorator = new(this.decoratedReader, CancellationToken.None);
    }

    [Fact]
    public void Close_ShouldForwardToDecoratedReader()
    {
        this.decorator.Close();

        this.decoratedReader.Received().Close();
    }

    [Fact]
    public async Task CloseAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.CloseAsync().Returns(Task.CompletedTask);

        await this.decorator.CloseAsync();

        await this.decoratedReader.Received().CloseAsync();
    }

    [Fact]
    public void Depth_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.Depth.Returns(123);

        this.decorator.Depth
            .Should().Be(123);

        _ = this.decoratedReader.Received().Depth;
    }

    [Fact]
    public void Dispose_ShouldInvokeOnDisposingFunction()
    {
        var onDisposingFunction = Substitute.For<Action>();

        this.decorator.OnDisposing = onDisposingFunction;

        this.decorator.Dispose();

        onDisposingFunction.Received()();
    }

    [Fact]
    public async Task DisposeAsync_ShouldInvokeOnDisposingAsyncFunction()
    {
        var onDisposingAsyncFunction = Substitute.For<Func<Task>>();

        this.decorator.OnDisposingAsync = onDisposingAsyncFunction;

        await this.decorator.DisposeAsync();

        await onDisposingAsyncFunction.Received()();
    }

    [Fact]
    public void FieldCount_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.FieldCount.Returns(123);

        this.decorator.FieldCount
            .Should().Be(123);

        _ = this.decoratedReader.Received().FieldCount;
    }

    [Fact]
    public void GetBoolean_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetBoolean(0).Returns(true);

        this.decorator.GetBoolean(0)
            .Should().BeTrue();

        _ = this.decoratedReader.Received().GetBoolean(0);
    }

    [Fact]
    public void GetByte_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetByte(0).Returns((Byte)123);

        this.decorator.GetByte(0)
            .Should().Be(123);

        _ = this.decoratedReader.Received().GetByte(0);
    }

    [Fact]
    public void GetBytes_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[10];
        this.decoratedReader.GetBytes(0, 1, buffer, 2, 3).Returns(123);

        this.decorator.GetBytes(0, 1, buffer, 2, 3)
            .Should().Be(123);

        this.decoratedReader.Received().GetBytes(0, 1, buffer, 2, 3);
    }

    [Fact]
    public void GetChars_ShouldForwardToDecoratedReader()
    {
        var buffer = new Char[10];
        this.decoratedReader.GetChars(0, 1, buffer, 2, 3).Returns(123);

        this.decorator.GetChars(0, 1, buffer, 2, 3)
            .Should().Be(123);

        this.decoratedReader.Received().GetChars(0, 1, buffer, 2, 3);
    }

    [Fact]
    public async Task GetColumnSchemaAsync_ShouldForwardToDecoratedReader()
    {
        var dbColumns = new ReadOnlyCollection<DbColumn>([]);

        this.decoratedReader.GetColumnSchemaAsync(CancellationToken.None).Returns(Task.FromResult(dbColumns));

        (await this.decorator.GetColumnSchemaAsync(CancellationToken.None))
            .Should().BeEquivalentTo(dbColumns);

        await this.decoratedReader.Received().GetColumnSchemaAsync(CancellationToken.None);
    }

    [Fact]
    public void GetDataTypeName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetDataTypeName(0).Returns("nvarchar");

        this.decorator.GetDataTypeName(0)
            .Should().Be("nvarchar");

        _ = this.decoratedReader.Received().GetDataTypeName(0);
    }

    [Fact]
    public void GetDateTime_ShouldForwardToDecoratedReader()
    {
        var date = new DateTime(2024, 1, 2);
        this.decoratedReader.GetDateTime(0).Returns(date);

        this.decorator.GetDateTime(0)
            .Should().Be(date);

        _ = this.decoratedReader.Received().GetDateTime(0);
    }

    [Fact]
    public void GetDecimal_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetDecimal(0).Returns(123.45m);

        this.decorator.GetDecimal(0)
            .Should().Be(123.45m);

        _ = _ = this.decoratedReader.Received().GetDecimal(0);
    }

    [Fact]
    public void GetDouble_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetDouble(0).Returns(1.23);

        this.decorator.GetDouble(0)
            .Should().Be(1.23);

        _ = this.decoratedReader.Received().GetDouble(0);
    }

    [Fact]
    public void GetEnumerator_ShouldForwardToDecoratedReader()
    {
        var enumerator = Substitute.For<IEnumerator>();
        
        // ReSharper disable once GenericEnumeratorNotDisposed
        this.decoratedReader.GetEnumerator().Returns(enumerator);

        // ReSharper disable once GenericEnumeratorNotDisposed
        this.decorator.GetEnumerator()
            .Should().BeSameAs(enumerator);

        // ReSharper disable once GenericEnumeratorNotDisposed
        _ = this.decoratedReader.Received().GetEnumerator();
    }

    [Fact]
    public void GetFieldType_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetFieldType(0).Returns(typeof(String));

        this.decorator.GetFieldType(0)
            .Should().Be(typeof(String));

        _ = this.decoratedReader.Received().GetFieldType(0);
    }

    [Fact]
    public void GetFieldValue_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetFieldValue<Int32>(0).Returns(42);

        this.decorator.GetFieldValue<Int32>(0)
            .Should().Be(42);

        this.decoratedReader.Received().GetFieldValue<Int32>(0);
    }

    [Fact]
    public async Task GetFieldValueAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetFieldValueAsync<Int32>(0, CancellationToken.None)
            .Returns(Task.FromResult(42));

        (await this.decorator.GetFieldValueAsync<Int32>(0, CancellationToken.None))
            .Should().Be(42);

        await this.decoratedReader.Received().GetFieldValueAsync<Int32>(0, CancellationToken.None);
    }

    [Fact]
    public void GetFloat_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetFloat(0).Returns(1.23f);

        this.decorator.GetFloat(0)
            .Should().Be(1.23f);

        _ = this.decoratedReader.Received().GetFloat(0);
    }

    [Fact]
    public void GetGuid_ShouldForwardToDecoratedReader()
    {
        var guid = Guid.NewGuid();
        this.decoratedReader.GetGuid(0).Returns(guid);

        this.decorator.GetGuid(0)
            .Should().Be(guid);

        _ = this.decoratedReader.Received().GetGuid(0);
    }

    [Fact]
    public void GetInt16_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetInt16(0).Returns((Int16)123);

        this.decorator.GetInt16(0)
            .Should().Be(123);

        _ = this.decoratedReader.Received().GetInt16(0);
    }

    [Fact]
    public void GetInt32_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetInt32(0).Returns(123);

        this.decorator.GetInt32(0)
            .Should().Be(123);

        _ = this.decoratedReader.Received().GetInt32(0);
    }

    [Fact]
    public void GetInt64_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetInt64(0).Returns(123L);

        this.decorator.GetInt64(0)
            .Should().Be(123L);

        _ = this.decoratedReader.Received().GetInt64(0);
    }

    [Fact]
    public void GetName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetName(1).Returns("Column1");

        this.decorator.GetName(1)
            .Should().Be("Column1");

        _ = this.decoratedReader.Received().GetName(1);
    }

    [Fact]
    public void GetOrdinal_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetOrdinal("Column1").Returns(1);

        this.decorator.GetOrdinal("Column1")
            .Should().Be(1);

        _ = this.decoratedReader.Received().GetOrdinal("Column1");
    }

    [Fact]
    public void GetProviderSpecificFieldType_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetProviderSpecificFieldType(0).Returns(typeof(SqlInt32));

        this.decorator.GetProviderSpecificFieldType(0)
            .Should().Be(typeof(SqlInt32));

        _ = this.decoratedReader.Received().GetProviderSpecificFieldType(0);
    }


    [Fact]
    public void GetProviderSpecificValue_ShouldForwardToDecoratedReader()
    {
        var value = new Object();
        this.decoratedReader.GetProviderSpecificValue(0).Returns(value);

        this.decorator.GetProviderSpecificValue(0)
            .Should().BeSameAs(value);

        _ = this.decoratedReader.Received().GetProviderSpecificValue(0);
    }

    [Fact]
    public void GetProviderSpecificValues_ShouldForwardToDecoratedReader()
    {
        var values = new Object[3];
        this.decoratedReader.GetProviderSpecificValues(values).Returns(3);

        this.decorator.GetProviderSpecificValues(values)
            .Should().Be(3);

        this.decoratedReader.Received().GetProviderSpecificValues(values);
    }

    [Fact]
    public void GetSchemaTable_ShouldForwardToDecoratedReader()
    {
        var table = new DataTable();
        this.decoratedReader.GetSchemaTable().Returns(table);

        this.decorator.GetSchemaTable()
            .Should().BeSameAs(table);

        this.decoratedReader.Received().GetSchemaTable();
    }

    [Fact]
    public async Task GetSchemaTableAsync_ShouldForwardToDecoratedReader()
    {
        var table = new DataTable();
        this.decoratedReader.GetSchemaTableAsync(CancellationToken.None).Returns(Task.FromResult<DataTable?>(table));

        (await this.decorator.GetSchemaTableAsync(CancellationToken.None))
            .Should().BeSameAs(table);

        await this.decoratedReader.Received().GetSchemaTableAsync(CancellationToken.None);
    }

    [Fact]
    public void GetStream_ShouldForwardToDecoratedReader()
    {
        var stream = Substitute.For<Stream>();
        
        this.decoratedReader.GetStream(0).Returns(stream);

        this.decorator.GetStream(0)
            .Should().BeSameAs(stream);

        this.decoratedReader.Received().GetStream(0);
    }

    [Fact]
    public void GetString_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetString(0).Returns("ABC");

        this.decorator.GetString(0)
            .Should().Be("ABC");

        _ = this.decoratedReader.Received().GetString(0);
    }

    [Fact]
    public void GetTextReader_ShouldForwardToDecoratedReader()
    {
        var textReader = Substitute.For<TextReader>();
        
        this.decoratedReader.GetTextReader(0).Returns(textReader);

        this.decorator.GetTextReader(0)
            .Should().BeSameAs(textReader);

        this.decoratedReader.Received().GetTextReader(0);
    }

    [Fact]
    public void GetValue_ShouldForwardToDecoratedReader()
    {
        var value = new Object();
        this.decoratedReader.GetValue(0).Returns(value);

        this.decorator.GetValue(0)
            .Should().BeSameAs(value);

        _ = this.decoratedReader.Received().GetValue(0);
    }

    [Fact]
    public void GetValues_ShouldForwardToDecoratedReader()
    {
        var values = new Object[3];
        this.decoratedReader.GetValues(values).Returns(3);

        this.decorator.GetValues(values)
            .Should().Be(3);

        this.decoratedReader.Received().GetValues(values);
    }

    [Fact]
    public void HasRows_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.HasRows.Returns(true);

        this.decorator.HasRows
            .Should().BeTrue();

        _ = this.decoratedReader.Received().HasRows;
    }

    [Fact]
    public void Indexer_Name_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader["A"].Returns("ABC");

        this.decorator["A"]
            .Should().Be("ABC");

        _ = this.decoratedReader.Received()["A"];
    }

    [Fact]
    public void Indexer_Ordinal_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader[0].Returns("ABC");

        this.decorator[0]
            .Should().Be("ABC");

        _ = this.decoratedReader.Received()[0];
    }

    [Fact]
    public void IsClosed_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.IsClosed.Returns(true);

        this.decorator.IsClosed
            .Should().BeTrue();

        _ = this.decoratedReader.Received().IsClosed;
    }

    [Fact]
    public void IsDBNull_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.IsDBNull(0).Returns(true);

        this.decorator.IsDBNull(0)
            .Should().BeTrue();

        _ = this.decoratedReader.Received().IsDBNull(0);
    }

    [Fact]
    public async Task IsDBNullAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.IsDBNullAsync(0, CancellationToken.None).Returns(Task.FromResult(true));

        (await this.decorator.IsDBNullAsync(0, CancellationToken.None))
            .Should().BeTrue();

        await this.decoratedReader.Received().IsDBNullAsync(0, CancellationToken.None);
    }

    [Fact]
    public void NextResult_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.NextResult().Returns(true);

        this.decorator.NextResult()
            .Should().BeTrue();

        this.decoratedReader.Received().NextResult();
    }

    [Fact]
    public async Task NextResultAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.NextResultAsync(CancellationToken.None).Returns(Task.FromResult(true));

        (await this.decorator.NextResultAsync(CancellationToken.None))
            .Should().BeTrue();

        await this.decoratedReader.Received().NextResultAsync(CancellationToken.None);
    }

    [Fact]
    public void RecordsAffected_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.RecordsAffected.Returns(123);

        this.decorator.RecordsAffected
            .Should().Be(123);

        _ = this.decoratedReader.Received().RecordsAffected;
    }

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            new DisposeSignalingDataReaderDecorator(this.decoratedReader, CancellationToken.None)
        );

    [Fact]
    public void VisibleFieldCount_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.VisibleFieldCount.Returns(123);

        this.decorator.VisibleFieldCount
            .Should().Be(123);

        _ = this.decoratedReader.Received().VisibleFieldCount;
    }

    private readonly DbDataReader decoratedReader;
    private readonly DisposeSignalingDataReaderDecorator decorator;
}
