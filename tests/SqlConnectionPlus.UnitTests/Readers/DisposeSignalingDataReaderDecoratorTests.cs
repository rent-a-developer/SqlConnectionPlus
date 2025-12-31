using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using RentADeveloper.SqlConnectionPlus.Readers;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Readers;

public class DisposeSignalingDataReaderDecoratorTests : TestsBase
{
    [Fact]
    public void Close_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.Close();
        decoratedReader.Received().Close();
    }

    [Fact]
    public async Task CloseAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.CloseAsync().Returns(Task.CompletedTask);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        await decorator.CloseAsync();

        await decoratedReader.Received().CloseAsync();
    }

    [Fact]
    public void Depth_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.Depth.Returns(123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.Depth
            .Should().Be(123);

        _ = decoratedReader.Received().Depth;
    }

    [Fact]
    public void Dispose_ShouldInvokeOnDisposingFunction()
    {
        var innerReader = Substitute.For<DbDataReader>();
        var onDisposingFunction = Substitute.For<Action>();

        var decoratedReader = new DisposeSignalingDataReaderDecorator(innerReader, CancellationToken.None);

        decoratedReader.OnDisposing = onDisposingFunction;

        decoratedReader.Dispose();

        onDisposingFunction.Received()();
    }

    [Fact]
    public async Task DisposeAsync_ShouldInvokeOnDisposingAsyncFunction()
    {
        var innerReader = Substitute.For<DbDataReader>();
        var onDisposingAsyncFunction = Substitute.For<Func<Task>>();

        var decoratedReader = new DisposeSignalingDataReaderDecorator(innerReader, CancellationToken.None);

        decoratedReader.OnDisposingAsync = onDisposingAsyncFunction;

        await decoratedReader.DisposeAsync();

        await onDisposingAsyncFunction.Received()();
    }

    [Fact]
    public void FieldCount_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.FieldCount.Returns(123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.FieldCount
            .Should().Be(123);

        _ = decoratedReader.Received().FieldCount;
    }

    [Fact]
    public void GetBoolean_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetBoolean(0).Returns(true);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetBoolean(0)
            .Should().BeTrue();

        _ = decoratedReader.Received().GetBoolean(0);
    }

    [Fact]
    public void GetByte_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetByte(0).Returns((Byte)123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetByte(0)
            .Should().Be(123);

        _ = decoratedReader.Received().GetByte(0);
    }

    [Fact]
    public void GetBytes_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var buffer = new Byte[10];
        decoratedReader.GetBytes(0, 1, buffer, 2, 3).Returns(123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetBytes(0, 1, buffer, 2, 3)
            .Should().Be(123);

        decoratedReader.Received().GetBytes(0, 1, buffer, 2, 3);
    }

    [Fact]
    public void GetChars_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var buffer = new Char[10];
        decoratedReader.GetChars(0, 1, buffer, 2, 3).Returns(123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetChars(0, 1, buffer, 2, 3)
            .Should().Be(123);

        decoratedReader.Received().GetChars(0, 1, buffer, 2, 3);
    }

    [Fact]
    public async Task GetColumnSchemaAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var dbColumns = new ReadOnlyCollection<DbColumn>([]);
        decoratedReader.GetColumnSchemaAsync(CancellationToken.None).Returns(Task.FromResult(dbColumns));

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        (await decorator.GetColumnSchemaAsync(CancellationToken.None))
            .Should().BeEquivalentTo(dbColumns);

        await decoratedReader.Received().GetColumnSchemaAsync(CancellationToken.None);
    }

    [Fact]
    public void GetDataTypeName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetDataTypeName(0).Returns("nvarchar");

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetDataTypeName(0)
            .Should().Be("nvarchar");

        _ = decoratedReader.Received().GetDataTypeName(0);
    }

    [Fact]
    public void GetDateTime_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var date = new DateTime(2024, 1, 2);
        decoratedReader.GetDateTime(0).Returns(date);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetDateTime(0)
            .Should().Be(date);

        _ = decoratedReader.Received().GetDateTime(0);
    }

    [Fact]
    public void GetDecimal_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetDecimal(0).Returns(123.45m);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetDecimal(0)
            .Should().Be(123.45m);

        _ = _ = decoratedReader.Received().GetDecimal(0);
    }

    [Fact]
    public void GetDouble_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetDouble(0).Returns(1.23);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetDouble(0)
            .Should().Be(1.23);

        _ = decoratedReader.Received().GetDouble(0);
    }

    [Fact]
    public void GetEnumerator_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var enumerator = Substitute.For<IEnumerator>();
        // ReSharper disable once GenericEnumeratorNotDisposed
        decoratedReader.GetEnumerator().Returns(enumerator);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        // ReSharper disable once GenericEnumeratorNotDisposed
        decorator.GetEnumerator()
            .Should().BeSameAs(enumerator);

        // ReSharper disable once GenericEnumeratorNotDisposed
        _ = decoratedReader.Received().GetEnumerator();
    }

    [Fact]
    public void GetFieldType_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetFieldType(0).Returns(typeof(String));

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetFieldType(0)
            .Should().Be(typeof(String));

        _ = decoratedReader.Received().GetFieldType(0);
    }

    [Fact]
    public void GetFieldValue_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetFieldValue<Int32>(0).Returns(42);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetFieldValue<Int32>(0)
            .Should().Be(42);

        decoratedReader.Received().GetFieldValue<Int32>(0);
    }

    [Fact]
    public async Task GetFieldValueAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetFieldValueAsync<Int32>(0, CancellationToken.None)
            .Returns(Task.FromResult(42));

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        (await decorator.GetFieldValueAsync<Int32>(0, CancellationToken.None))
            .Should().Be(42);

        await decoratedReader.Received().GetFieldValueAsync<Int32>(0, CancellationToken.None);
    }

    [Fact]
    public void GetFloat_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetFloat(0).Returns(1.23f);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetFloat(0)
            .Should().Be(1.23f);

        _ = decoratedReader.Received().GetFloat(0);
    }

    [Fact]
    public void GetGuid_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var guid = Guid.NewGuid();
        decoratedReader.GetGuid(0).Returns(guid);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetGuid(0)
            .Should().Be(guid);

        _ = decoratedReader.Received().GetGuid(0);
    }

    [Fact]
    public void GetInt16_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetInt16(0).Returns((Int16)123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetInt16(0)
            .Should().Be(123);

        _ = decoratedReader.Received().GetInt16(0);
    }

    [Fact]
    public void GetInt32_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetInt32(0).Returns(123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetInt32(0)
            .Should().Be(123);

        _ = decoratedReader.Received().GetInt32(0);
    }

    [Fact]
    public void GetInt64_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetInt64(0).Returns(123L);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetInt64(0)
            .Should().Be(123L);

        _ = decoratedReader.Received().GetInt64(0);
    }

    [Fact]
    public void GetName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetName(1).Returns("Column1");

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetName(1)
            .Should().Be("Column1");

        _ = decoratedReader.Received().GetName(1);
    }

    [Fact]
    public void GetOrdinal_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetOrdinal("Column1").Returns(1);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetOrdinal("Column1")
            .Should().Be(1);

        _ = decoratedReader.Received().GetOrdinal("Column1");
    }

    [Fact]
    public void GetProviderSpecificFieldType_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetProviderSpecificFieldType(0).Returns(typeof(SqlInt32));

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetProviderSpecificFieldType(0)
            .Should().Be(typeof(SqlInt32));

        _ = decoratedReader.Received().GetProviderSpecificFieldType(0);
    }


    [Fact]
    public void GetProviderSpecificValue_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var value = new Object();
        decoratedReader.GetProviderSpecificValue(0).Returns(value);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetProviderSpecificValue(0)
            .Should().BeSameAs(value);

        _ = decoratedReader.Received().GetProviderSpecificValue(0);
    }

    [Fact]
    public void GetProviderSpecificValues_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var values = new Object[3];
        decoratedReader.GetProviderSpecificValues(values).Returns(3);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetProviderSpecificValues(values)
            .Should().Be(3);

        decoratedReader.Received().GetProviderSpecificValues(values);
    }

    [Fact]
    public void GetSchemaTable_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var table = new DataTable();
        decoratedReader.GetSchemaTable().Returns(table);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetSchemaTable()
            .Should().BeSameAs(table);

        decoratedReader.Received().GetSchemaTable();
    }

    [Fact]
    public async Task GetSchemaTableAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var table = new DataTable();
        decoratedReader.GetSchemaTableAsync(CancellationToken.None).Returns(Task.FromResult<DataTable?>(table));

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        (await decorator.GetSchemaTableAsync(CancellationToken.None))
            .Should().BeSameAs(table);

        await decoratedReader.Received().GetSchemaTableAsync(CancellationToken.None);
    }

    [Fact]
    public void GetStream_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var stream = Substitute.For<Stream>();
        decoratedReader.GetStream(0).Returns(stream);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetStream(0)
            .Should().BeSameAs(stream);

        decoratedReader.Received().GetStream(0);
    }

    [Fact]
    public void GetString_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.GetString(0).Returns("ABC");

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetString(0)
            .Should().Be("ABC");

        _ = decoratedReader.Received().GetString(0);
    }

    [Fact]
    public void GetTextReader_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var textReader = Substitute.For<TextReader>();
        decoratedReader.GetTextReader(0).Returns(textReader);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetTextReader(0)
            .Should().BeSameAs(textReader);

        decoratedReader.Received().GetTextReader(0);
    }

    [Fact]
    public void GetValue_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var value = new Object();
        decoratedReader.GetValue(0).Returns(value);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetValue(0)
            .Should().BeSameAs(value);

        _ = decoratedReader.Received().GetValue(0);
    }

    [Fact]
    public void GetValues_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        var values = new Object[3];
        decoratedReader.GetValues(values).Returns(3);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.GetValues(values)
            .Should().Be(3);

        decoratedReader.Received().GetValues(values);
    }

    [Fact]
    public void HasRows_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.HasRows.Returns(true);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.HasRows
            .Should().BeTrue();

        _ = decoratedReader.Received().HasRows;
    }

    [Fact]
    public void Indexer_Name_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader["A"].Returns("ABC");

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator["A"]
            .Should().Be("ABC");

        _ = decoratedReader.Received()["A"];
    }

    [Fact]
    public void Indexer_Ordinal_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader[0].Returns("ABC");

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator[0]
            .Should().Be("ABC");

        _ = decoratedReader.Received()[0];
    }

    [Fact]
    public void IsClosed_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.IsClosed.Returns(true);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.IsClosed
            .Should().BeTrue();

        _ = decoratedReader.Received().IsClosed;
    }

    [Fact]
    public void IsDBNull_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.IsDBNull(0).Returns(true);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.IsDBNull(0)
            .Should().BeTrue();

        _ = decoratedReader.Received().IsDBNull(0);
    }

    [Fact]
    public async Task IsDBNullAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.IsDBNullAsync(0, CancellationToken.None).Returns(Task.FromResult(true));

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        (await decorator.IsDBNullAsync(0, CancellationToken.None))
            .Should().BeTrue();

        await decoratedReader.Received().IsDBNullAsync(0, CancellationToken.None);
    }

    [Fact]
    public void NextResult_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.NextResult().Returns(true);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.NextResult()
            .Should().BeTrue();

        decoratedReader.Received().NextResult();
    }

    [Fact]
    public async Task NextResultAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.NextResultAsync(CancellationToken.None).Returns(Task.FromResult(true));

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        (await decorator.NextResultAsync(CancellationToken.None))
            .Should().BeTrue();

        await decoratedReader.Received().NextResultAsync(CancellationToken.None);
    }

    [Fact]
    public void RecordsAffected_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.RecordsAffected.Returns(123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.RecordsAffected
            .Should().Be(123);

        _ = decoratedReader.Received().RecordsAffected;
    }

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            new DisposeSignalingDataReaderDecorator(Substitute.For<DbDataReader>(), CancellationToken.None)
        );

    [Fact]
    public void VisibleFieldCount_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<DbDataReader>();

        decoratedReader.VisibleFieldCount.Returns(123);

        var decorator = new DisposeSignalingDataReaderDecorator(decoratedReader, CancellationToken.None);

        decorator.VisibleFieldCount
            .Should().Be(123);

        _ = decoratedReader.Received().VisibleFieldCount;
    }
}
