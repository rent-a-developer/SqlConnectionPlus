using System.Xml;
using System.Xml.Schema;
using RentADeveloper.SqlConnectionPlus.Readers;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.Readers;

public class DisposeSignalingXmlReaderDecoratorTests : TestsBase
{
    [Fact]
    public void AttributeCount_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.AttributeCount.Returns(123);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.AttributeCount
            .Should().Be(123);

        _ = decoratedReader.Received().AttributeCount;
    }

    [Fact]
    public void BaseURI_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.BaseURI.Returns("Base URI");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.BaseURI
            .Should().Be("Base URI");

        _ = decoratedReader.Received().BaseURI;
    }

    [Fact]
    public void CanReadBinaryContent_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.CanReadBinaryContent.Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.CanReadBinaryContent
            .Should().BeTrue();

        _ = decoratedReader.Received().CanReadBinaryContent;
    }

    [Fact]
    public void CanReadValueChunk_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.CanReadValueChunk.Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.CanReadValueChunk
            .Should().BeTrue();

        _ = decoratedReader.Received().CanReadValueChunk;
    }

    [Fact]
    public void CanResolveEntity_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.CanResolveEntity.Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.CanResolveEntity
            .Should().BeTrue();

        _ = decoratedReader.Received().CanResolveEntity;
    }

    [Fact]
    public void Close_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.Close();

        decoratedReader.Received().Close();
    }

    [Fact]
    public void Depth_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.Depth.Returns(5);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.Depth
            .Should().Be(5);

        _ = decoratedReader.Received().Depth;
    }

    [Fact]
    public void Dispose_ShouldInvokeOnDisposingFunction()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var onDisposingFunction = Substitute.For<Action>();

        var reader = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        reader.OnDisposing = onDisposingFunction;

        reader.Dispose();

        onDisposingFunction.Received()();
    }

    [Fact]
    public void EOF_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.EOF.Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.EOF
            .Should().BeTrue();

        _ = decoratedReader.Received().EOF;
    }

    [Fact]
    public void GetAttribute_ByIndex_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.GetAttribute(0).Returns("value");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.GetAttribute(0)
            .Should().Be("value");

        decoratedReader.Received().GetAttribute(0);
    }

    [Fact]
    public void GetAttribute_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.GetAttribute("attr").Returns("value");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.GetAttribute("attr")
            .Should().Be("value");

        decoratedReader.Received().GetAttribute("attr");
    }

    [Fact]
    public void GetAttribute_ByNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.GetAttribute("attr", "http://ns").Returns("value");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.GetAttribute("attr", "http://ns")
            .Should().Be("value");

        decoratedReader.Received().GetAttribute("attr", "http://ns");
    }

    [Fact]
    public async Task GetValueAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.GetValueAsync().Returns(Task.FromResult("async-value"));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.GetValueAsync();

        result.Should().Be("async-value");

        await decoratedReader.Received().GetValueAsync();
    }

    [Fact]
    public void HasAttributes_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.HasAttributes.Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.HasAttributes
            .Should().BeTrue();

        _ = decoratedReader.Received().HasAttributes;
    }

    [Fact]
    public void HasValue_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.HasValue.Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.HasValue
            .Should().BeTrue();

        _ = decoratedReader.Received().HasValue;
    }

    [Fact]
    public void Indexer_ByIndex_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader[1].Returns("value1");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator[1]
            .Should().Be("value1");

        _ = decoratedReader.Received()[1];
    }

    [Fact]
    public void Indexer_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader["attr"].Returns("value");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator["attr"]
            .Should().Be("value");

        _ = decoratedReader.Received()["attr"];
    }

    [Fact]
    public void Indexer_ByNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader["attr", "ns"].Returns("value");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator["attr", "ns"]
            .Should().Be("value");

        _ = decoratedReader.Received()["attr", "ns"];
    }

    [Fact]
    public void IsDefault_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.IsDefault.Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.IsDefault
            .Should().BeTrue();

        _ = decoratedReader.Received().IsDefault;
    }

    [Fact]
    public void IsEmptyElement_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.IsEmptyElement.Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.IsEmptyElement
            .Should().BeTrue();

        _ = decoratedReader.Received().IsEmptyElement;
    }

    [Fact]
    public void IsStartElement_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.IsStartElement("local", "ns").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.IsStartElement("local", "ns")
            .Should().BeTrue();

        decoratedReader.Received().IsStartElement("local", "ns");
    }

    [Fact]
    public void IsStartElement_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.IsStartElement("name").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.IsStartElement("name")
            .Should().BeTrue();

        decoratedReader.Received().IsStartElement("name");
    }

    [Fact]
    public void IsStartElement_Parameterless_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.IsStartElement().Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.IsStartElement()
            .Should().BeTrue();

        decoratedReader.Received().IsStartElement();
    }

    [Fact]
    public void LocalName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.LocalName.Returns("LocalName");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.LocalName
            .Should().Be("LocalName");

        _ = decoratedReader.Received().LocalName;
    }

    [Fact]
    public void LookupNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.LookupNamespace("prefix").Returns("http://ns");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.LookupNamespace("prefix")
            .Should().Be("http://ns");

        decoratedReader.Received().LookupNamespace("prefix");
    }

    [Fact]
    public void MoveToAttribute_ByIndex_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.MoveToAttribute(1);

        decoratedReader.Received().MoveToAttribute(1);
    }

    [Fact]
    public void MoveToAttribute_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.MoveToAttribute("attr").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.MoveToAttribute("attr")
            .Should().BeTrue();

        decoratedReader.Received().MoveToAttribute("attr");
    }

    [Fact]
    public void MoveToAttribute_ByNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.MoveToAttribute("attr", "http://ns").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.MoveToAttribute("attr", "http://ns")
            .Should().BeTrue();

        decoratedReader.Received().MoveToAttribute("attr", "http://ns");
    }

    [Fact]
    public void MoveToContent_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.MoveToContent().Returns(XmlNodeType.Text);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.MoveToContent()
            .Should().Be(XmlNodeType.Text);

        decoratedReader.Received().MoveToContent();
    }

    [Fact]
    public async Task MoveToContentAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.MoveToContentAsync().Returns(Task.FromResult(XmlNodeType.CDATA));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.MoveToContentAsync();

        result.Should().Be(XmlNodeType.CDATA);

        await decoratedReader.Received().MoveToContentAsync();
    }

    [Fact]
    public void MoveToElement_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.MoveToElement().Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.MoveToElement()
            .Should().BeTrue();

        decoratedReader.Received().MoveToElement();
    }

    [Fact]
    public void MoveToFirstAttribute_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.MoveToFirstAttribute().Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.MoveToFirstAttribute()
            .Should().BeTrue();

        decoratedReader.Received().MoveToFirstAttribute();
    }

    [Fact]
    public void MoveToNextAttribute_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.MoveToNextAttribute().Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.MoveToNextAttribute()
            .Should().BeTrue();

        decoratedReader.Received().MoveToNextAttribute();
    }

    [Fact]
    public void Name_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.Name.Returns("Name");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.Name
            .Should().Be("Name");

        _ = decoratedReader.Received().Name;
    }

    [Fact]
    public void NamespaceURI_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.NamespaceURI.Returns("http://namespace.uri");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.NamespaceURI
            .Should().Be("http://namespace.uri");

        _ = decoratedReader.Received().NamespaceURI;
    }

    [Fact]
    public void NameTable_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var nameTable = Substitute.For<XmlNameTable>();

        decoratedReader.NameTable.Returns(nameTable);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.NameTable
            .Should().BeSameAs(nameTable);

        _ = decoratedReader.Received().NameTable;
    }

    [Fact]
    public void NodeType_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.NodeType.Returns(XmlNodeType.Element);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.NodeType
            .Should().Be(XmlNodeType.Element);

        _ = decoratedReader.Received().NodeType;
    }

    [Fact]
    public void Prefix_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.Prefix.Returns("Prefix");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.Prefix
            .Should().Be("Prefix");

        _ = decoratedReader.Received().Prefix;
    }

    [Fact]
    public void QuoteChar_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.QuoteChar.Returns('"');

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.QuoteChar
            .Should().Be('"');

        _ = decoratedReader.Received().QuoteChar;
    }

    [Fact]
    public void Read_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.Read().Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.Read()
            .Should().BeTrue();

        decoratedReader.Received().Read();
    }

    [Fact]
    public async Task ReadAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadAsync().Returns(Task.FromResult(true));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadAsync();

        result.Should().BeTrue();

        await decoratedReader.Received().ReadAsync();
    }

    [Fact]
    public void ReadAttributeValue_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadAttributeValue().Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadAttributeValue()
            .Should().BeTrue();

        decoratedReader.Received().ReadAttributeValue();
    }

    [Fact]
    public void ReadContentAs_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var resolver = Substitute.For<IXmlNamespaceResolver>();

        decoratedReader.ReadContentAs(typeof(Int32), resolver).Returns(42);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAs(typeof(Int32), resolver)
            .Should().Be(42);

        decoratedReader.Received().ReadContentAs(typeof(Int32), resolver);
    }

    [Fact]
    public async Task ReadContentAsAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var resolver = Substitute.For<IXmlNamespaceResolver>();

        decoratedReader.ReadContentAsAsync(typeof(String), resolver).Returns(Task.FromResult<Object>("x"));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadContentAsAsync(typeof(String), resolver);

        result.Should().Be("x");

        await decoratedReader.Received().ReadContentAsAsync(typeof(String), resolver);
    }

    [Fact]
    public void ReadContentAsBase64_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Byte[10];

        decoratedReader.ReadContentAsBase64(buffer, 1, 3).Returns(2);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsBase64(buffer, 1, 3)
            .Should().Be(2);

        decoratedReader.Received().ReadContentAsBase64(buffer, 1, 3);
    }

    [Fact]
    public async Task ReadContentAsBase64Async_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Byte[10];

        decoratedReader.ReadContentAsBase64Async(buffer, 2, 4).Returns(Task.FromResult(3));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadContentAsBase64Async(buffer, 2, 4);

        result.Should().Be(3);

        await decoratedReader.Received().ReadContentAsBase64Async(buffer, 2, 4);
    }

    [Fact]
    public void ReadContentAsBinHex_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Byte[8];

        decoratedReader.ReadContentAsBinHex(buffer, 0, 8).Returns(4);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsBinHex(buffer, 0, 8)
            .Should().Be(4);

        decoratedReader.Received().ReadContentAsBinHex(buffer, 0, 8);
    }

    [Fact]
    public async Task ReadContentAsBinHexAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Byte[8];

        decoratedReader.ReadContentAsBinHexAsync(buffer, 1, 5).Returns(Task.FromResult(2));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadContentAsBinHexAsync(buffer, 1, 5);

        result.Should().Be(2);

        await decoratedReader.Received().ReadContentAsBinHexAsync(buffer, 1, 5);
    }

    [Fact]
    public void ReadContentAsBoolean_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadContentAsBoolean().Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsBoolean()
            .Should().BeTrue();

        decoratedReader.Received().ReadContentAsBoolean();
    }

    [Fact]
    public void ReadContentAsDateTime_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var date = new DateTime(2024, 01, 02);

        decoratedReader.ReadContentAsDateTime().Returns(date);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsDateTime()
            .Should().Be(date);

        decoratedReader.Received().ReadContentAsDateTime();
    }

    [Fact]
    public void ReadContentAsDateTimeOffset_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var value = new DateTimeOffset(2024, 01, 02, 0, 0, 0, TimeSpan.Zero);

        decoratedReader.ReadContentAsDateTimeOffset().Returns(value);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsDateTimeOffset()
            .Should().Be(value);

        decoratedReader.Received().ReadContentAsDateTimeOffset();
    }

    [Fact]
    public void ReadContentAsDecimal_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadContentAsDecimal().Returns(1.23m);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsDecimal()
            .Should().Be(1.23m);

        decoratedReader.Received().ReadContentAsDecimal();
    }

    [Fact]
    public void ReadContentAsDouble_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadContentAsDouble().Returns(1.23);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsDouble()
            .Should().Be(1.23);

        decoratedReader.Received().ReadContentAsDouble();
    }

    [Fact]
    public void ReadContentAsFloat_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadContentAsFloat().Returns(1.23f);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsFloat()
            .Should().Be(1.23f);

        decoratedReader.Received().ReadContentAsFloat();
    }

    [Fact]
    public void ReadContentAsInt_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadContentAsInt().Returns(7);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsInt()
            .Should().Be(7);

        decoratedReader.Received().ReadContentAsInt();
    }

    [Fact]
    public void ReadContentAsLong_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadContentAsLong().Returns(42L);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsLong()
            .Should().Be(42L);

        decoratedReader.Received().ReadContentAsLong();
    }

    [Fact]
    public void ReadContentAsObject_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var obj = new Object();

        decoratedReader.ReadContentAsObject().Returns(obj);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsObject()
            .Should().BeSameAs(obj);

        decoratedReader.Received().ReadContentAsObject();
    }

    [Fact]
    public async Task ReadContentAsObjectAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var obj = new Object();

        decoratedReader.ReadContentAsObjectAsync().Returns(Task.FromResult(obj));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadContentAsObjectAsync();

        result.Should().BeSameAs(obj);

        await decoratedReader.Received().ReadContentAsObjectAsync();
    }

    [Fact]
    public void ReadContentAsString_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadContentAsString().Returns("text");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadContentAsString()
            .Should().Be("text");

        decoratedReader.Received().ReadContentAsString();
    }

    [Fact]
    public async Task ReadContentAsStringAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadContentAsStringAsync().Returns(Task.FromResult("async-text"));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadContentAsStringAsync();

        result.Should().Be("async-text");

        await decoratedReader.Received().ReadContentAsStringAsync();
    }

    [Fact]
    public void ReadElementContentAs_WithResolver_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var resolver = Substitute.For<IXmlNamespaceResolver>();
        var obj = new Object();

        decoratedReader.ReadElementContentAs(typeof(String), resolver).Returns(obj);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAs(typeof(String), resolver)
            .Should().BeSameAs(obj);

        decoratedReader.Received().ReadElementContentAs(typeof(String), resolver);
    }

    [Fact]
    public void ReadElementContentAs_WithResolverAndNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var resolver = Substitute.For<IXmlNamespaceResolver>();
        var obj = new Object();

        decoratedReader.ReadElementContentAs(typeof(Int32), resolver, "l", "ns").Returns(obj);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAs(typeof(Int32), resolver, "l", "ns")
            .Should().BeSameAs(obj);

        decoratedReader.Received().ReadElementContentAs(typeof(Int32), resolver, "l", "ns");
    }

    [Fact]
    public async Task ReadElementContentAsAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var resolver = Substitute.For<IXmlNamespaceResolver>();
        var obj = new Object();

        decoratedReader.ReadElementContentAsAsync(typeof(String), resolver).Returns(Task.FromResult(obj));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadElementContentAsAsync(typeof(String), resolver);

        result.Should().BeSameAs(obj);

        await decoratedReader.Received().ReadElementContentAsAsync(typeof(String), resolver);
    }

    [Fact]
    public void ReadElementContentAsBase64_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Byte[10];

        decoratedReader.ReadElementContentAsBase64(buffer, 1, 3).Returns(2);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsBase64(buffer, 1, 3)
            .Should().Be(2);

        decoratedReader.Received().ReadElementContentAsBase64(buffer, 1, 3);
    }

    [Fact]
    public async Task ReadElementContentAsBase64Async_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Byte[10];

        decoratedReader.ReadElementContentAsBase64Async(buffer, 1, 3).Returns(Task.FromResult(2));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadElementContentAsBase64Async(buffer, 1, 3);

        result.Should().Be(2);

        await decoratedReader.Received().ReadElementContentAsBase64Async(buffer, 1, 3);
    }

    [Fact]
    public void ReadElementContentAsBinHex_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Byte[10];

        decoratedReader.ReadElementContentAsBinHex(buffer, 0, 5).Returns(3);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsBinHex(buffer, 0, 5)
            .Should().Be(3);

        decoratedReader.Received().ReadElementContentAsBinHex(buffer, 0, 5);
    }

    [Fact]
    public async Task ReadElementContentAsBinHexAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Byte[10];

        decoratedReader.ReadElementContentAsBinHexAsync(buffer, 0, 5).Returns(Task.FromResult(3));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadElementContentAsBinHexAsync(buffer, 0, 5);

        result.Should().Be(3);

        await decoratedReader.Received().ReadElementContentAsBinHexAsync(buffer, 0, 5);
    }

    [Fact]
    public void ReadElementContentAsBoolean_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsBoolean().Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsBoolean()
            .Should().BeTrue();

        decoratedReader.Received().ReadElementContentAsBoolean();
    }

    [Fact]
    public void ReadElementContentAsBoolean_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsBoolean("l", "ns").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsBoolean("l", "ns")
            .Should().BeTrue();

        decoratedReader.Received().ReadElementContentAsBoolean("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsDateTime_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var value = new DateTime(2024, 1, 1);

        decoratedReader.ReadElementContentAsDateTime().Returns(value);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsDateTime()
            .Should().Be(value);

        decoratedReader.Received().ReadElementContentAsDateTime();
    }

    [Fact]
    public void ReadElementContentAsDateTime_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var value = new DateTime(2024, 1, 1);

        decoratedReader.ReadElementContentAsDateTime("l", "ns").Returns(value);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsDateTime("l", "ns")
            .Should().Be(value);

        decoratedReader.Received().ReadElementContentAsDateTime("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsDecimal_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsDecimal().Returns(1.23m);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsDecimal()
            .Should().Be(1.23m);

        decoratedReader.Received().ReadElementContentAsDecimal();
    }

    [Fact]
    public void ReadElementContentAsDecimal_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsDecimal("l", "ns").Returns(1.23m);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsDecimal("l", "ns")
            .Should().Be(1.23m);

        decoratedReader.Received().ReadElementContentAsDecimal("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsDouble_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsDouble().Returns(1.23);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsDouble()
            .Should().Be(1.23);

        decoratedReader.Received().ReadElementContentAsDouble();
    }

    [Fact]
    public void ReadElementContentAsDouble_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsDouble("l", "ns").Returns(1.23);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsDouble("l", "ns")
            .Should().Be(1.23);

        decoratedReader.Received().ReadElementContentAsDouble("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsFloat_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsFloat().Returns(1.23f);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsFloat()
            .Should().Be(1.23f);

        decoratedReader.Received().ReadElementContentAsFloat();
    }

    [Fact]
    public void ReadElementContentAsFloat_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsFloat("l", "ns").Returns(1.23f);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsFloat("l", "ns")
            .Should().Be(1.23f);

        decoratedReader.Received().ReadElementContentAsFloat("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsInt_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsInt().Returns(1);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsInt()
            .Should().Be(1);

        decoratedReader.Received().ReadElementContentAsInt();
    }

    [Fact]
    public void ReadElementContentAsInt_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsInt("l", "ns").Returns(1);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsInt("l", "ns")
            .Should().Be(1);

        decoratedReader.Received().ReadElementContentAsInt("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsLong_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsLong().Returns(1L);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsLong()
            .Should().Be(1L);

        decoratedReader.Received().ReadElementContentAsLong();
    }

    [Fact]
    public void ReadElementContentAsLong_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsLong("l", "ns").Returns(1L);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsLong("l", "ns")
            .Should().Be(1L);

        decoratedReader.Received().ReadElementContentAsLong("l", "ns");
    }

    [Fact]
    public void ReadElementContentAsObject_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var obj = new Object();

        decoratedReader.ReadElementContentAsObject().Returns(obj);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsObject()
            .Should().BeSameAs(obj);

        decoratedReader.Received().ReadElementContentAsObject();
    }

    [Fact]
    public void ReadElementContentAsObject_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var obj = new Object();

        decoratedReader.ReadElementContentAsObject("l", "ns").Returns(obj);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsObject("l", "ns")
            .Should().BeSameAs(obj);

        decoratedReader.Received().ReadElementContentAsObject("l", "ns");
    }

    [Fact]
    public async Task ReadElementContentAsObjectAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var obj = new Object();

        decoratedReader.ReadElementContentAsObjectAsync().Returns(Task.FromResult(obj));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadElementContentAsObjectAsync();

        result.Should().BeSameAs(obj);

        await decoratedReader.Received().ReadElementContentAsObjectAsync();
    }

    [Fact]
    public void ReadElementContentAsString_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsString().Returns("x");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsString()
            .Should().Be("x");

        decoratedReader.Received().ReadElementContentAsString();
    }

    [Fact]
    public void ReadElementContentAsString_WithNames_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsString("l", "ns").Returns("x");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementContentAsString("l", "ns")
            .Should().Be("x");

        decoratedReader.Received().ReadElementContentAsString("l", "ns");
    }

    [Fact]
    public async Task ReadElementContentAsStringAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementContentAsStringAsync().Returns(Task.FromResult("x"));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadElementContentAsStringAsync();

        result.Should().Be("x");

        await decoratedReader.Received().ReadElementContentAsStringAsync();
    }

    [Fact]
    public void ReadElementString_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementString("l", "ns").Returns("v");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementString("l", "ns")
            .Should().Be("v");

        decoratedReader.Received().ReadElementString("l", "ns");
    }

    [Fact]
    public void ReadElementString_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementString("n").Returns("v");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementString("n")
            .Should().Be("v");

        decoratedReader.Received().ReadElementString("n");
    }

    [Fact]
    public void ReadElementString_Parameterless_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadElementString().Returns("v");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadElementString()
            .Should().Be("v");

        decoratedReader.Received().ReadElementString();
    }

    [Fact]
    public void ReadEndElement_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadEndElement();

        decoratedReader.Received().ReadEndElement();
    }

    [Fact]
    public void ReadInnerXml_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadInnerXml().Returns("<inner/>");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadInnerXml()
            .Should().Be("<inner/>");

        decoratedReader.Received().ReadInnerXml();
    }

    [Fact]
    public async Task ReadInnerXmlAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadInnerXmlAsync().Returns(Task.FromResult("<inner/>"));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadInnerXmlAsync();

        result.Should().Be("<inner/>");

        await decoratedReader.Received().ReadInnerXmlAsync();
    }

    [Fact]
    public void ReadOuterXml_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadOuterXml().Returns("<outer/>");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadOuterXml()
            .Should().Be("<outer/>");

        decoratedReader.Received().ReadOuterXml();
    }

    [Fact]
    public async Task ReadOuterXmlAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadOuterXmlAsync().Returns(Task.FromResult("<outer/>"));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadOuterXmlAsync();

        result.Should().Be("<outer/>");

        await decoratedReader.Received().ReadOuterXmlAsync();
    }

    [Fact]
    public void ReadStartElement_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadStartElement("l", "ns");

        decoratedReader.Received().ReadStartElement("l", "ns");
    }

    [Fact]
    public void ReadStartElement_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadStartElement("n");

        decoratedReader.Received().ReadStartElement("n");
    }

    [Fact]
    public void ReadStartElement_Parameterless_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadStartElement();

        decoratedReader.Received().ReadStartElement();
    }

    [Fact]
    public void ReadState_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadState.Returns(ReadState.Interactive);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadState
            .Should().Be(ReadState.Interactive);

        _ = decoratedReader.Received().ReadState;
    }

    [Fact]
    public void ReadString_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadString().Returns("v");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadString()
            .Should().Be("v");

        decoratedReader.Received().ReadString();
    }

    [Fact]
    public void ReadSubtree_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var subtree = Substitute.For<XmlReader>();

        decoratedReader.ReadSubtree().Returns(subtree);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadSubtree()
            .Should().BeSameAs(subtree);

        decoratedReader.Received().ReadSubtree();
    }

    [Fact]
    public void ReadToDescendant_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadToDescendant("l", "ns").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadToDescendant("l", "ns")
            .Should().BeTrue();

        decoratedReader.Received().ReadToDescendant("l", "ns");
    }

    [Fact]
    public void ReadToDescendant_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadToDescendant("n").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadToDescendant("n")
            .Should().BeTrue();

        decoratedReader.Received().ReadToDescendant("n");
    }

    [Fact]
    public void ReadToFollowing_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadToFollowing("l", "ns").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadToFollowing("l", "ns")
            .Should().BeTrue();

        decoratedReader.Received().ReadToFollowing("l", "ns");
    }

    [Fact]
    public void ReadToFollowing_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadToFollowing("n").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadToFollowing("n")
            .Should().BeTrue();

        decoratedReader.Received().ReadToFollowing("n");
    }

    [Fact]
    public void ReadToNextSibling_ByLocalNameAndNamespace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadToNextSibling("l", "ns").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadToNextSibling("l", "ns")
            .Should().BeTrue();

        decoratedReader.Received().ReadToNextSibling("l", "ns");
    }

    [Fact]
    public void ReadToNextSibling_ByName_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ReadToNextSibling("n").Returns(true);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadToNextSibling("n")
            .Should().BeTrue();

        decoratedReader.Received().ReadToNextSibling("n");
    }

    [Fact]
    public void ReadValueChunk_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Char[10];

        decoratedReader.ReadValueChunk(buffer, 1, 3).Returns(2);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ReadValueChunk(buffer, 1, 3)
            .Should().Be(2);

        decoratedReader.Received().ReadValueChunk(buffer, 1, 3);
    }

    [Fact]
    public async Task ReadValueChunkAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var buffer = new Char[10];

        decoratedReader.ReadValueChunkAsync(buffer, 1, 3).Returns(Task.FromResult(2));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        var result = await decorator.ReadValueChunkAsync(buffer, 1, 3);

        result.Should().Be(2);

        await decoratedReader.Received().ReadValueChunkAsync(buffer, 1, 3);
    }

    [Fact]
    public void ResolveEntity_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ResolveEntity();

        decoratedReader.Received().ResolveEntity();
    }

    [Fact]
    public void SchemaInfo_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var schemaInfo = Substitute.For<IXmlSchemaInfo>();

        decoratedReader.SchemaInfo.Returns(schemaInfo);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.SchemaInfo
            .Should().BeSameAs(schemaInfo);

        _ = decoratedReader.Received().SchemaInfo;
    }

    [Fact]
    public void Settings_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();
        var settings = new XmlReaderSettings();

        decoratedReader.Settings.Returns(settings);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.Settings
            .Should().BeSameAs(settings);

        _ = decoratedReader.Received().Settings;
    }

    [Fact]
    public void Skip_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.Skip();

        decoratedReader.Received().Skip();
    }

    [Fact]
    public async Task SkipAsync_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.SkipAsync().Returns(Task.CompletedTask);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        await decorator.SkipAsync();

        await decoratedReader.Received().SkipAsync();
    }

    [Fact]
    public void Value_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.Value.Returns("Value");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.Value
            .Should().Be("Value");

        _ = decoratedReader.Received().Value;
    }

    [Fact]
    public void ValueType_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.ValueType.Returns(typeof(Int32));

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.ValueType
            .Should().Be(typeof(Int32));

        _ = decoratedReader.Received().ValueType;
    }

    [Fact]
    public void VerifyNullArgumentGuards() =>
        ArgumentNullGuardVerifier.Verify(() =>
            new DisposeSignalingXmlReaderDecorator(Substitute.For<XmlReader>())
        );

    [Fact]
    public void XmlLang_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.XmlLang.Returns("de-DE");

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.XmlLang
            .Should().Be("de-DE");

        _ = decoratedReader.Received().XmlLang;
    }

    [Fact]
    public void XmlSpace_ShouldForwardToDecoratedReader()
    {
        var decoratedReader = Substitute.For<XmlReader>();

        decoratedReader.XmlSpace.Returns(XmlSpace.Preserve);

        var decorator = new DisposeSignalingXmlReaderDecorator(decoratedReader);

        decorator.XmlSpace
            .Should().Be(XmlSpace.Preserve);

        _ = decoratedReader.Received().XmlSpace;
    }
}
