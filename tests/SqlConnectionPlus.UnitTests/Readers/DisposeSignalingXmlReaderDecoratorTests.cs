using System.Xml;
using System.Xml.Schema;
using RentADeveloper.SqlConnectionPlus.Readers;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Readers;

public class DisposeSignalingXmlReaderDecoratorTests : TestsBase
{
    public DisposeSignalingXmlReaderDecoratorTests()
    {
        this.decoratedReader = Substitute.For<XmlReader>();
        this.decorator = new(this.decoratedReader);
    }

    [Fact]
    public void AttributeCount_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.AttributeCount.Returns(123);

        this.decorator.AttributeCount
            .Should().Be(123);

        _ = this.decoratedReader.Received().AttributeCount;
    }

    [Fact]
    public void BaseURI_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.BaseURI.Returns("Base URI");

        this.decorator.BaseURI
            .Should().Be("Base URI");

        _ = this.decoratedReader.Received().BaseURI;
    }

    [Fact]
    public void CanReadBinaryContent_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.CanReadBinaryContent.Returns(true);

        this.decorator.CanReadBinaryContent
            .Should().BeTrue();

        _ = this.decoratedReader.Received().CanReadBinaryContent;
    }

    [Fact]
    public void CanReadValueChunk_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.CanReadValueChunk.Returns(true);

        this.decorator.CanReadValueChunk
            .Should().BeTrue();

        _ = this.decoratedReader.Received().CanReadValueChunk;
    }

    [Fact]
    public void CanResolveEntity_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.CanResolveEntity.Returns(true);

        this.decorator.CanResolveEntity
            .Should().BeTrue();

        _ = this.decoratedReader.Received().CanResolveEntity;
    }

    [Fact]
    public void Close_ShouldForwardToDecoratedReader()
    {
        this.decorator.Close();

        this.decoratedReader.Received().Close();
    }

    [Fact]
    public void Depth_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.Depth.Returns(5);

        this.decorator.Depth
            .Should().Be(5);

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
    public void EOF_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.EOF.Returns(true);

        this.decorator.EOF
            .Should().BeTrue();

        _ = this.decoratedReader.Received().EOF;
    }

    [Fact]
    public void GetAttribute_ByIndex_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetAttribute(0).Returns("value");

        this.decorator.GetAttribute(0)
            .Should().Be("value");

        this.decoratedReader.Received().GetAttribute(0);
    }

    [Fact]
    public void GetAttribute_ByName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetAttribute("attr").Returns("value");

        this.decorator.GetAttribute("attr")
            .Should().Be("value");

        this.decoratedReader.Received().GetAttribute("attr");
    }

    [Fact]
    public void GetAttribute_ByNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetAttribute("attr", "http://ns").Returns("value");

        this.decorator.GetAttribute("attr", "http://ns")
            .Should().Be("value");

        this.decoratedReader.Received().GetAttribute("attr", "http://ns");
    }

    [Fact]
    public async Task GetValueAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.GetValueAsync().Returns(Task.FromResult("async-value"));

        var result = await this.decorator.GetValueAsync();

        result.Should().Be("async-value");

        await this.decoratedReader.Received().GetValueAsync();
    }

    [Fact]
    public void HasAttributes_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.HasAttributes.Returns(true);

        this.decorator.HasAttributes
            .Should().BeTrue();

        _ = this.decoratedReader.Received().HasAttributes;
    }

    [Fact]
    public void HasValue_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.HasValue.Returns(true);

        this.decorator.HasValue
            .Should().BeTrue();

        _ = this.decoratedReader.Received().HasValue;
    }

    [Fact]
    public void Indexer_ByIndex_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader[1].Returns("value1");

        this.decorator[1]
            .Should().Be("value1");

        _ = this.decoratedReader.Received()[1];
    }

    [Fact]
    public void Indexer_ByName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader["attr"].Returns("value");

        this.decorator["attr"]
            .Should().Be("value");

        _ = this.decoratedReader.Received()["attr"];
    }

    [Fact]
    public void Indexer_ByNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader["attr", "ns"].Returns("value");

        this.decorator["attr", "ns"]
            .Should().Be("value");

        _ = this.decoratedReader.Received()["attr", "ns"];
    }

    [Fact]
    public void IsDefault_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.IsDefault.Returns(true);

        this.decorator.IsDefault
            .Should().BeTrue();

        _ = this.decoratedReader.Received().IsDefault;
    }

    [Fact]
    public void IsEmptyElement_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.IsEmptyElement.Returns(true);

        this.decorator.IsEmptyElement
            .Should().BeTrue();

        _ = this.decoratedReader.Received().IsEmptyElement;
    }

    [Fact]
    public void IsStartElement_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.IsStartElement("local", "ns").Returns(true);

        this.decorator.IsStartElement("local", "ns")
            .Should().BeTrue();

        this.decoratedReader.Received().IsStartElement("local", "ns");
    }

    [Fact]
    public void IsStartElement_ByName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.IsStartElement("name").Returns(true);

        this.decorator.IsStartElement("name")
            .Should().BeTrue();

        this.decoratedReader.Received().IsStartElement("name");
    }

    [Fact]
    public void IsStartElement_Parameterless_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.IsStartElement().Returns(true);

        this.decorator.IsStartElement()
            .Should().BeTrue();

        this.decoratedReader.Received().IsStartElement();
    }

    [Fact]
    public void LocalName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.LocalName.Returns("LocalName");

        this.decorator.LocalName
            .Should().Be("LocalName");

        _ = this.decoratedReader.Received().LocalName;
    }

    [Fact]
    public void LookupNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.LookupNamespace("prefix").Returns("http://ns");

        this.decorator.LookupNamespace("prefix")
            .Should().Be("http://ns");

        this.decoratedReader.Received().LookupNamespace("prefix");
    }

    [Fact]
    public void MoveToAttribute_ByIndex_ShouldForwardToDecoratedReader()
    {
        this.decorator.MoveToAttribute(1);

        this.decoratedReader.Received().MoveToAttribute(1);
    }

    [Fact]
    public void MoveToAttribute_ByName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.MoveToAttribute("attr").Returns(true);

        this.decorator.MoveToAttribute("attr")
            .Should().BeTrue();

        this.decoratedReader.Received().MoveToAttribute("attr");
    }

    [Fact]
    public void MoveToAttribute_ByNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.MoveToAttribute("attr", "http://ns").Returns(true);

        this.decorator.MoveToAttribute("attr", "http://ns")
            .Should().BeTrue();

        this.decoratedReader.Received().MoveToAttribute("attr", "http://ns");
    }

    [Fact]
    public void MoveToContent_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.MoveToContent().Returns(XmlNodeType.Text);

        this.decorator.MoveToContent()
            .Should().Be(XmlNodeType.Text);

        this.decoratedReader.Received().MoveToContent();
    }

    [Fact]
    public async Task MoveToContentAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.MoveToContentAsync().Returns(Task.FromResult(XmlNodeType.CDATA));

        var result = await this.decorator.MoveToContentAsync();

        result.Should().Be(XmlNodeType.CDATA);

        await this.decoratedReader.Received().MoveToContentAsync();
    }

    [Fact]
    public void MoveToElement_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.MoveToElement().Returns(true);

        this.decorator.MoveToElement()
            .Should().BeTrue();

        this.decoratedReader.Received().MoveToElement();
    }

    [Fact]
    public void MoveToFirstAttribute_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.MoveToFirstAttribute().Returns(true);

        this.decorator.MoveToFirstAttribute()
            .Should().BeTrue();

        this.decoratedReader.Received().MoveToFirstAttribute();
    }

    [Fact]
    public void MoveToNextAttribute_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.MoveToNextAttribute().Returns(true);

        this.decorator.MoveToNextAttribute()
            .Should().BeTrue();

        this.decoratedReader.Received().MoveToNextAttribute();
    }

    [Fact]
    public void Name_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.Name.Returns("Name");

        this.decorator.Name
            .Should().Be("Name");

        _ = this.decoratedReader.Received().Name;
    }

    [Fact]
    public void NamespaceURI_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.NamespaceURI.Returns("http://namespace.uri");

        this.decorator.NamespaceURI
            .Should().Be("http://namespace.uri");

        _ = this.decoratedReader.Received().NamespaceURI;
    }

    [Fact]
    public void NameTable_ShouldForwardToDecoratedReader()
    {
        var nameTable = Substitute.For<XmlNameTable>();

        this.decoratedReader.NameTable.Returns(nameTable);

        this.decorator.NameTable
            .Should().BeSameAs(nameTable);

        _ = this.decoratedReader.Received().NameTable;
    }

    [Fact]
    public void NodeType_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.NodeType.Returns(XmlNodeType.Element);

        this.decorator.NodeType
            .Should().Be(XmlNodeType.Element);

        _ = this.decoratedReader.Received().NodeType;
    }

    [Fact]
    public void Prefix_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.Prefix.Returns("Prefix");

        this.decorator.Prefix
            .Should().Be("Prefix");

        _ = this.decoratedReader.Received().Prefix;
    }

    [Fact]
    public void QuoteChar_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.QuoteChar.Returns('"');

        this.decorator.QuoteChar
            .Should().Be('"');

        _ = this.decoratedReader.Received().QuoteChar;
    }

    [Fact]
    public void Read_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.Read().Returns(true);

        this.decorator.Read()
            .Should().BeTrue();

        this.decoratedReader.Received().Read();
    }

    [Fact]
    public async Task ReadAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadAsync().Returns(Task.FromResult(true));

        var result = await this.decorator.ReadAsync();

        result.Should().BeTrue();

        await this.decoratedReader.Received().ReadAsync();
    }

    [Fact]
    public void ReadAttributeValue_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadAttributeValue().Returns(true);

        this.decorator.ReadAttributeValue()
            .Should().BeTrue();

        this.decoratedReader.Received().ReadAttributeValue();
    }

    [Fact]
    public void ReadContentAs_ShouldForwardToDecoratedReader()
    {
        var resolver = Substitute.For<IXmlNamespaceResolver>();

        this.decoratedReader.ReadContentAs(typeof(Int32), resolver).Returns(42);

        this.decorator.ReadContentAs(typeof(Int32), resolver)
            .Should().Be(42);

        this.decoratedReader.Received().ReadContentAs(typeof(Int32), resolver);
    }

    [Fact]
    public async Task ReadContentAsAsync_ShouldForwardToDecoratedReader()
    {
        var resolver = Substitute.For<IXmlNamespaceResolver>();

        this.decoratedReader.ReadContentAsAsync(typeof(String), resolver).Returns(Task.FromResult<Object>("x"));

        var result = await this.decorator.ReadContentAsAsync(typeof(String), resolver);

        result.Should().Be("x");

        await this.decoratedReader.Received().ReadContentAsAsync(typeof(String), resolver);
    }

    [Fact]
    public void ReadContentAsBase64_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[10];

        this.decoratedReader.ReadContentAsBase64(buffer, 1, 3).Returns(2);

        this.decorator.ReadContentAsBase64(buffer, 1, 3)
            .Should().Be(2);

        this.decoratedReader.Received().ReadContentAsBase64(buffer, 1, 3);
    }

    [Fact]
    public async Task ReadContentAsBase64Async_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[10];

        this.decoratedReader.ReadContentAsBase64Async(buffer, 2, 4).Returns(Task.FromResult(3));

        var result = await this.decorator.ReadContentAsBase64Async(buffer, 2, 4);

        result.Should().Be(3);

        await this.decoratedReader.Received().ReadContentAsBase64Async(buffer, 2, 4);
    }

    [Fact]
    public void ReadContentAsBinHex_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[8];

        this.decoratedReader.ReadContentAsBinHex(buffer, 0, 8).Returns(4);

        this.decorator.ReadContentAsBinHex(buffer, 0, 8)
            .Should().Be(4);

        this.decoratedReader.Received().ReadContentAsBinHex(buffer, 0, 8);
    }

    [Fact]
    public async Task ReadContentAsBinHexAsync_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[8];

        this.decoratedReader.ReadContentAsBinHexAsync(buffer, 1, 5).Returns(Task.FromResult(2));

        var result = await this.decorator.ReadContentAsBinHexAsync(buffer, 1, 5);

        result.Should().Be(2);

        await this.decoratedReader.Received().ReadContentAsBinHexAsync(buffer, 1, 5);
    }

    [Fact]
    public void ReadContentAsBoolean_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadContentAsBoolean().Returns(true);

        this.decorator.ReadContentAsBoolean()
            .Should().BeTrue();

        this.decoratedReader.Received().ReadContentAsBoolean();
    }

    [Fact]
    public void ReadContentAsDateTime_ShouldForwardToDecoratedReader()
    {
        var date = new DateTime(2024, 01, 02);

        this.decoratedReader.ReadContentAsDateTime().Returns(date);

        this.decorator.ReadContentAsDateTime()
            .Should().Be(date);

        this.decoratedReader.Received().ReadContentAsDateTime();
    }

    [Fact]
    public void ReadContentAsDateTimeOffset_ShouldForwardToDecoratedReader()
    {
        var value = new DateTimeOffset(2024, 01, 02, 0, 0, 0, TimeSpan.Zero);

        this.decoratedReader.ReadContentAsDateTimeOffset().Returns(value);

        this.decorator.ReadContentAsDateTimeOffset()
            .Should().Be(value);

        this.decoratedReader.Received().ReadContentAsDateTimeOffset();
    }

    [Fact]
    public void ReadContentAsDecimal_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadContentAsDecimal().Returns(1.23m);

        this.decorator.ReadContentAsDecimal()
            .Should().Be(1.23m);

        this.decoratedReader.Received().ReadContentAsDecimal();
    }

    [Fact]
    public void ReadContentAsDouble_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadContentAsDouble().Returns(1.23);

        this.decorator.ReadContentAsDouble()
            .Should().Be(1.23);

        this.decoratedReader.Received().ReadContentAsDouble();
    }

    [Fact]
    public void ReadContentAsFloat_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadContentAsFloat().Returns(1.23f);

        this.decorator.ReadContentAsFloat()
            .Should().Be(1.23f);

        this.decoratedReader.Received().ReadContentAsFloat();
    }

    [Fact]
    public void ReadContentAsInt_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadContentAsInt().Returns(7);

        this.decorator.ReadContentAsInt()
            .Should().Be(7);

        this.decoratedReader.Received().ReadContentAsInt();
    }

    [Fact]
    public void ReadContentAsLong_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadContentAsLong().Returns(42L);

        this.decorator.ReadContentAsLong()
            .Should().Be(42L);

        this.decoratedReader.Received().ReadContentAsLong();
    }

    [Fact]
    public void ReadContentAsObject_ShouldForwardToDecoratedReader()
    {
        var obj = new Object();

        this.decoratedReader.ReadContentAsObject().Returns(obj);

        this.decorator.ReadContentAsObject()
            .Should().BeSameAs(obj);

        this.decoratedReader.Received().ReadContentAsObject();
    }

    [Fact]
    public async Task ReadContentAsObjectAsync_ShouldForwardToDecoratedReader()
    {
        var obj = new Object();

        this.decoratedReader.ReadContentAsObjectAsync().Returns(Task.FromResult(obj));

        var result = await this.decorator.ReadContentAsObjectAsync();

        result.Should().BeSameAs(obj);

        await this.decoratedReader.Received().ReadContentAsObjectAsync();
    }

    [Fact]
    public void ReadContentAsString_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadContentAsString().Returns("text");

        this.decorator.ReadContentAsString()
            .Should().Be("text");

        this.decoratedReader.Received().ReadContentAsString();
    }

    [Fact]
    public async Task ReadContentAsStringAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadContentAsStringAsync().Returns(Task.FromResult("async-text"));

        var result = await this.decorator.ReadContentAsStringAsync();

        result.Should().Be("async-text");

        await this.decoratedReader.Received().ReadContentAsStringAsync();
    }

    [Fact]
    public void ReadElementContentAs_WithResolver_ShouldForwardToDecoratedReader()
    {
        var resolver = Substitute.For<IXmlNamespaceResolver>();

        var obj = new Object();

        this.decoratedReader.ReadElementContentAs(typeof(String), resolver).Returns(obj);

        this.decorator.ReadElementContentAs(typeof(String), resolver)
            .Should().BeSameAs(obj);

        this.decoratedReader.Received().ReadElementContentAs(typeof(String), resolver);
    }

    [Fact]
    public void ReadElementContentAs_WithResolverAndNames_ShouldForwardToDecoratedReader()
    {
        var resolver = Substitute.For<IXmlNamespaceResolver>();

        var obj = new Object();

        this.decoratedReader.ReadElementContentAs(typeof(Int32), resolver, "l", "ns").Returns(obj);

        this.decorator.ReadElementContentAs(typeof(Int32), resolver, "l", "ns")
            .Should().BeSameAs(obj);

        this.decoratedReader.Received().ReadElementContentAs(typeof(Int32), resolver, "l", "ns");
    }

    [Fact]
    public async Task ReadElementContentAsAsync_ShouldForwardToDecoratedReader()
    {
        var resolver = Substitute.For<IXmlNamespaceResolver>();

        var obj = new Object();

        this.decoratedReader.ReadElementContentAsAsync(typeof(String), resolver).Returns(Task.FromResult(obj));

        var result = await this.decorator.ReadElementContentAsAsync(typeof(String), resolver);

        result.Should().BeSameAs(obj);

        await this.decoratedReader.Received().ReadElementContentAsAsync(typeof(String), resolver);
    }

    [Fact]
    public void ReadElementContentAsBase64_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[10];

        this.decoratedReader.ReadElementContentAsBase64(buffer, 1, 3).Returns(2);

        this.decorator.ReadElementContentAsBase64(buffer, 1, 3)
            .Should().Be(2);

        this.decoratedReader.Received().ReadElementContentAsBase64(buffer, 1, 3);
    }

    [Fact]
    public async Task ReadElementContentAsBase64Async_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[10];

        this.decoratedReader.ReadElementContentAsBase64Async(buffer, 1, 3).Returns(Task.FromResult(2));

        var result = await this.decorator.ReadElementContentAsBase64Async(buffer, 1, 3);

        result.Should().Be(2);

        await this.decoratedReader.Received().ReadElementContentAsBase64Async(buffer, 1, 3);
    }

    [Fact]
    public void ReadElementContentAsBinHex_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[10];

        this.decoratedReader.ReadElementContentAsBinHex(buffer, 0, 5).Returns(3);

        this.decorator.ReadElementContentAsBinHex(buffer, 0, 5)
            .Should().Be(3);

        this.decoratedReader.Received().ReadElementContentAsBinHex(buffer, 0, 5);
    }

    [Fact]
    public async Task ReadElementContentAsBinHexAsync_ShouldForwardToDecoratedReader()
    {
        var buffer = new Byte[10];

        this.decoratedReader.ReadElementContentAsBinHexAsync(buffer, 0, 5).Returns(Task.FromResult(3));

        var result = await this.decorator.ReadElementContentAsBinHexAsync(buffer, 0, 5);

        result.Should().Be(3);

        await this.decoratedReader.Received().ReadElementContentAsBinHexAsync(buffer, 0, 5);
    }

    [Fact]
    public void ReadElementContentAsBoolean_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsBoolean().Returns(true);

        this.decorator.ReadElementContentAsBoolean()
            .Should().BeTrue();

        this.decoratedReader.Received().ReadElementContentAsBoolean();
    }

    [Fact]
    public void ReadElementContentAsBoolean_WithNames_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsBoolean("l", "ns").Returns(true);

        this.decorator.ReadElementContentAsBoolean("l", "ns")
            .Should().BeTrue();

        this.decoratedReader.Received().ReadElementContentAsBoolean("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsDateTime_ShouldForwardToDecoratedReader()
    {
        var value = new DateTime(2024, 1, 1);

        this.decoratedReader.ReadElementContentAsDateTime().Returns(value);

        this.decorator.ReadElementContentAsDateTime()
            .Should().Be(value);

        this.decoratedReader.Received().ReadElementContentAsDateTime();
    }

    [Fact]
    public void ReadElementContentAsDateTime_WithNames_ShouldForwardToDecoratedReader()
    {
        var value = new DateTime(2024, 1, 1);

        this.decoratedReader.ReadElementContentAsDateTime("l", "ns").Returns(value);

        this.decorator.ReadElementContentAsDateTime("l", "ns")
            .Should().Be(value);

        this.decoratedReader.Received().ReadElementContentAsDateTime("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsDecimal_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsDecimal().Returns(1.23m);

        this.decorator.ReadElementContentAsDecimal()
            .Should().Be(1.23m);

        this.decoratedReader.Received().ReadElementContentAsDecimal();
    }

    [Fact]
    public void ReadElementContentAsDecimal_WithNames_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsDecimal("l", "ns").Returns(1.23m);

        this.decorator.ReadElementContentAsDecimal("l", "ns")
            .Should().Be(1.23m);

        this.decoratedReader.Received().ReadElementContentAsDecimal("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsDouble_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsDouble().Returns(1.23);

        this.decorator.ReadElementContentAsDouble()
            .Should().Be(1.23);

        this.decoratedReader.Received().ReadElementContentAsDouble();
    }

    [Fact]
    public void ReadElementContentAsDouble_WithNames_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsDouble("l", "ns").Returns(1.23);

        this.decorator.ReadElementContentAsDouble("l", "ns")
            .Should().Be(1.23);

        this.decoratedReader.Received().ReadElementContentAsDouble("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsFloat_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsFloat().Returns(1.23f);

        this.decorator.ReadElementContentAsFloat()
            .Should().Be(1.23f);

        this.decoratedReader.Received().ReadElementContentAsFloat();
    }

    [Fact]
    public void ReadElementContentAsFloat_WithNames_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsFloat("l", "ns").Returns(1.23f);

        this.decorator.ReadElementContentAsFloat("l", "ns")
            .Should().Be(1.23f);

        this.decoratedReader.Received().ReadElementContentAsFloat("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsInt_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsInt().Returns(1);

        this.decorator.ReadElementContentAsInt()
            .Should().Be(1);

        this.decoratedReader.Received().ReadElementContentAsInt();
    }

    [Fact]
    public void ReadElementContentAsInt_WithNames_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsInt("l", "ns").Returns(1);

        this.decorator.ReadElementContentAsInt("l", "ns")
            .Should().Be(1);

        this.decoratedReader.Received().ReadElementContentAsInt("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsLong_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsLong().Returns(1L);

        this.decorator.ReadElementContentAsLong()
            .Should().Be(1L);

        this.decoratedReader.Received().ReadElementContentAsLong();
    }

    [Fact]
    public void ReadElementContentAsLong_WithNames_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsLong("l", "ns").Returns(1L);

        this.decorator.ReadElementContentAsLong("l", "ns")
            .Should().Be(1L);

        this.decoratedReader.Received().ReadElementContentAsLong("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsObject_ShouldForwardToDecoratedReader()
    {
        var obj = new Object();

        this.decoratedReader.ReadElementContentAsObject().Returns(obj);

        this.decorator.ReadElementContentAsObject()
            .Should().BeSameAs(obj);

        this.decoratedReader.Received().ReadElementContentAsObject();
    }

    [Fact]
    public void ReadElementContentAsObject_WithNames_ShouldForwardToDecoratedReader()
    {
        var obj = new Object();

        this.decoratedReader.ReadElementContentAsObject("l", "ns").Returns(obj);

        this.decorator.ReadElementContentAsObject("l", "ns")
            .Should().BeSameAs(obj);

        this.decoratedReader.Received().ReadElementContentAsObject("l", "ns");
    }

    [Fact]
    public async Task ReadElementContentAsObjectAsync_ShouldForwardToDecoratedReader()
    {
        var obj = new Object();

        this.decoratedReader.ReadElementContentAsObjectAsync().Returns(Task.FromResult(obj));

        var result = await this.decorator.ReadElementContentAsObjectAsync();

        result.Should().BeSameAs(obj);

        await this.decoratedReader.Received().ReadElementContentAsObjectAsync();
    }

    [Fact]
    public void ReadElementContentAsString_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsString().Returns("x");

        this.decorator.ReadElementContentAsString()
            .Should().Be("x");

        this.decoratedReader.Received().ReadElementContentAsString();
    }

    [Fact]
    public void ReadElementContentAsString_WithNames_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsString("l", "ns").Returns("x");

        this.decorator.ReadElementContentAsString("l", "ns")
            .Should().Be("x");

        this.decoratedReader.Received().ReadElementContentAsString("l", "ns");
    }

    [Fact]
    public async Task ReadElementContentAsStringAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementContentAsStringAsync().Returns(Task.FromResult("x"));

        var result = await this.decorator.ReadElementContentAsStringAsync();

        result.Should().Be("x");

        await this.decoratedReader.Received().ReadElementContentAsStringAsync();
    }

    [Fact]
    public void ReadElementString_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementString("l", "ns").Returns("v");

        this.decorator.ReadElementString("l", "ns")
            .Should().Be("v");

        this.decoratedReader.Received().ReadElementString("l", "ns");
    }

    [Fact]
    public void ReadElementString_ByName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementString("n").Returns("v");

        this.decorator.ReadElementString("n")
            .Should().Be("v");

        this.decoratedReader.Received().ReadElementString("n");
    }

    [Fact]
    public void ReadElementString_Parameterless_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadElementString().Returns("v");

        this.decorator.ReadElementString()
            .Should().Be("v");

        this.decoratedReader.Received().ReadElementString();
    }

    [Fact]
    public void ReadEndElement_ShouldForwardToDecoratedReader()
    {
        this.decorator.ReadEndElement();

        this.decoratedReader.Received().ReadEndElement();
    }

    [Fact]
    public void ReadInnerXml_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadInnerXml().Returns("<inner/>");

        this.decorator.ReadInnerXml()
            .Should().Be("<inner/>");

        this.decoratedReader.Received().ReadInnerXml();
    }

    [Fact]
    public async Task ReadInnerXmlAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadInnerXmlAsync().Returns(Task.FromResult("<inner/>"));

        var result = await this.decorator.ReadInnerXmlAsync();

        result.Should().Be("<inner/>");

        await this.decoratedReader.Received().ReadInnerXmlAsync();
    }

    [Fact]
    public void ReadOuterXml_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadOuterXml().Returns("<outer/>");

        this.decorator.ReadOuterXml()
            .Should().Be("<outer/>");

        this.decoratedReader.Received().ReadOuterXml();
    }

    [Fact]
    public async Task ReadOuterXmlAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadOuterXmlAsync().Returns(Task.FromResult("<outer/>"));

        var result = await this.decorator.ReadOuterXmlAsync();

        result.Should().Be("<outer/>");

        await this.decoratedReader.Received().ReadOuterXmlAsync();
    }

    [Fact]
    public void ReadStartElement_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decorator.ReadStartElement("l", "ns");

        this.decoratedReader.Received().ReadStartElement("l", "ns");
    }

    [Fact]
    public void ReadStartElement_ByName_ShouldForwardToDecoratedReader()
    {
        this.decorator.ReadStartElement("n");

        this.decoratedReader.Received().ReadStartElement("n");
    }

    [Fact]
    public void ReadStartElement_Parameterless_ShouldForwardToDecoratedReader()
    {
        this.decorator.ReadStartElement();

        this.decoratedReader.Received().ReadStartElement();
    }

    [Fact]
    public void ReadState_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadState.Returns(ReadState.Interactive);

        this.decorator.ReadState
            .Should().Be(ReadState.Interactive);

        _ = this.decoratedReader.Received().ReadState;
    }

    [Fact]
    public void ReadString_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadString().Returns("v");

        this.decorator.ReadString()
            .Should().Be("v");

        this.decoratedReader.Received().ReadString();
    }

    [Fact]
    public void ReadSubtree_ShouldForwardToDecoratedReader()
    {
        var subtree = Substitute.For<XmlReader>();

        this.decoratedReader.ReadSubtree().Returns(subtree);

        this.decorator.ReadSubtree()
            .Should().BeSameAs(subtree);

        this.decoratedReader.Received().ReadSubtree();
    }

    [Fact]
    public void ReadToDescendant_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadToDescendant("l", "ns").Returns(true);

        this.decorator.ReadToDescendant("l", "ns")
            .Should().BeTrue();

        this.decoratedReader.Received().ReadToDescendant("l", "ns");
    }

    [Fact]
    public void ReadToDescendant_ByName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadToDescendant("n").Returns(true);

        this.decorator.ReadToDescendant("n")
            .Should().BeTrue();

        this.decoratedReader.Received().ReadToDescendant("n");
    }

    [Fact]
    public void ReadToFollowing_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadToFollowing("l", "ns").Returns(true);

        this.decorator.ReadToFollowing("l", "ns")
            .Should().BeTrue();

        this.decoratedReader.Received().ReadToFollowing("l", "ns");
    }

    [Fact]
    public void ReadToFollowing_ByName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadToFollowing("n").Returns(true);

        this.decorator.ReadToFollowing("n")
            .Should().BeTrue();

        this.decoratedReader.Received().ReadToFollowing("n");
    }

    [Fact]
    public void ReadToNextSibling_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadToNextSibling("l", "ns").Returns(true);

        this.decorator.ReadToNextSibling("l", "ns")
            .Should().BeTrue();

        this.decoratedReader.Received().ReadToNextSibling("l", "ns");
    }

    [Fact]
    public void ReadToNextSibling_ByName_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ReadToNextSibling("n").Returns(true);

        this.decorator.ReadToNextSibling("n")
            .Should().BeTrue();

        this.decoratedReader.Received().ReadToNextSibling("n");
    }

    [Fact]
    public void ReadValueChunk_ShouldForwardToDecoratedReader()
    {
        var buffer = new Char[10];

        this.decoratedReader.ReadValueChunk(buffer, 1, 3).Returns(2);

        this.decorator.ReadValueChunk(buffer, 1, 3)
            .Should().Be(2);

        this.decoratedReader.Received().ReadValueChunk(buffer, 1, 3);
    }

    [Fact]
    public async Task ReadValueChunkAsync_ShouldForwardToDecoratedReader()
    {
        var buffer = new Char[10];

        this.decoratedReader.ReadValueChunkAsync(buffer, 1, 3).Returns(Task.FromResult(2));

        var result = await this.decorator.ReadValueChunkAsync(buffer, 1, 3);

        result.Should().Be(2);

        await this.decoratedReader.Received().ReadValueChunkAsync(buffer, 1, 3);
    }

    [Fact]
    public void ResolveEntity_ShouldForwardToDecoratedReader()
    {
        this.decorator.ResolveEntity();

        this.decoratedReader.Received().ResolveEntity();
    }

    [Fact]
    public void SchemaInfo_ShouldForwardToDecoratedReader()
    {
        var schemaInfo = Substitute.For<IXmlSchemaInfo>();

        this.decoratedReader.SchemaInfo.Returns(schemaInfo);

        this.decorator.SchemaInfo
            .Should().BeSameAs(schemaInfo);

        _ = this.decoratedReader.Received().SchemaInfo;
    }

    [Fact]
    public void Settings_ShouldForwardToDecoratedReader()
    {
        var settings = new XmlReaderSettings();

        this.decoratedReader.Settings.Returns(settings);

        this.decorator.Settings
            .Should().BeSameAs(settings);

        _ = this.decoratedReader.Received().Settings;
    }

    [Fact]
    public void Skip_ShouldForwardToDecoratedReader()
    {
        this.decorator.Skip();

        this.decoratedReader.Received().Skip();
    }

    [Fact]
    public async Task SkipAsync_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.SkipAsync().Returns(Task.CompletedTask);

        await this.decorator.SkipAsync();

        await this.decoratedReader.Received().SkipAsync();
    }

    [Fact]
    public void Value_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.Value.Returns("Value");

        this.decorator.Value
            .Should().Be("Value");

        _ = this.decoratedReader.Received().Value;
    }

    [Fact]
    public void ValueType_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.ValueType.Returns(typeof(Int32));

        this.decorator.ValueType
            .Should().Be(typeof(Int32));

        _ = this.decoratedReader.Received().ValueType;
    }

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            new DisposeSignalingXmlReaderDecorator(this.decoratedReader)
        );

    [Fact]
    public void XmlLang_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.XmlLang.Returns("de-DE");

        this.decorator.XmlLang
            .Should().Be("de-DE");

        _ = this.decoratedReader.Received().XmlLang;
    }

    [Fact]
    public void XmlSpace_ShouldForwardToDecoratedReader()
    {
        this.decoratedReader.XmlSpace.Returns(XmlSpace.Preserve);

        this.decorator.XmlSpace
            .Should().Be(XmlSpace.Preserve);

        _ = this.decoratedReader.Received().XmlSpace;
    }

    private readonly XmlReader decoratedReader;
    private readonly DisposeSignalingXmlReaderDecorator decorator;
}
