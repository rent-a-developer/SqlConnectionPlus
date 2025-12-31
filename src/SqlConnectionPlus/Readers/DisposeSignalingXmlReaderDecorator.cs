// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

// ReSharper disable InconsistentNaming

using System.Xml;
using System.Xml.Schema;

namespace RentADeveloper.SqlConnectionPlus.Readers;

/// <summary>
/// A decorator for an <see cref="XmlReader" /> that signals when it is being disposed.
/// </summary>
internal sealed class DisposeSignalingXmlReaderDecorator : XmlReader
{
    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="xmlReader">The <see cref="XmlReader" /> to decorate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="xmlReader" /> is <see langword="null" />.</exception>
    public DisposeSignalingXmlReaderDecorator(XmlReader xmlReader)
    {
        ArgumentNullException.ThrowIfNull(xmlReader);

        this.xmlReader = xmlReader;
    }

    /// <inheritdoc />
    public override Int32 AttributeCount =>
        this.xmlReader.AttributeCount;

    /// <inheritdoc />
    public override String BaseURI =>
        this.xmlReader.BaseURI;

    /// <inheritdoc />
    public override Boolean CanReadBinaryContent =>
        this.xmlReader.CanReadBinaryContent;

    /// <inheritdoc />
    public override Boolean CanReadValueChunk =>
        this.xmlReader.CanReadValueChunk;

    /// <inheritdoc />
    public override Boolean CanResolveEntity =>
        this.xmlReader.CanResolveEntity;

    /// <inheritdoc />
    public override Int32 Depth =>
        this.xmlReader.Depth;

    /// <inheritdoc />
    public override Boolean EOF =>
        this.xmlReader.EOF;

    /// <inheritdoc />
    public override Boolean HasAttributes =>
        this.xmlReader.HasAttributes;

    /// <inheritdoc />
    public override Boolean HasValue =>
        this.xmlReader.HasValue;

    /// <inheritdoc />
    public override Boolean IsDefault =>
        this.xmlReader.IsDefault;

    /// <inheritdoc />
    public override Boolean IsEmptyElement =>
        this.xmlReader.IsEmptyElement;

    /// <inheritdoc />
    public override String this[Int32 i] =>
        this.xmlReader[i];

    /// <inheritdoc />
    public override String? this[String name] =>
        this.xmlReader[name];

    /// <inheritdoc />
    public override String? this[String name, String? namespaceURI] =>
        this.xmlReader[name, namespaceURI];

    /// <inheritdoc />
    public override String LocalName =>
        this.xmlReader.LocalName;

    /// <inheritdoc />
    public override String Name =>
        this.xmlReader.Name;

    /// <inheritdoc />
    public override String NamespaceURI =>
        this.xmlReader.NamespaceURI;

    /// <inheritdoc />
    public override XmlNameTable NameTable =>
        this.xmlReader.NameTable;

    /// <inheritdoc />
    public override XmlNodeType NodeType =>
        this.xmlReader.NodeType;

    /// <inheritdoc />
    public override String Prefix =>
        this.xmlReader.Prefix;

    /// <inheritdoc />
    public override Char QuoteChar =>
        this.xmlReader.QuoteChar;

    /// <inheritdoc />
    public override ReadState ReadState =>
        this.xmlReader.ReadState;

    /// <inheritdoc />
    public override IXmlSchemaInfo? SchemaInfo =>
        this.xmlReader.SchemaInfo;

    /// <inheritdoc />
    public override XmlReaderSettings? Settings =>
        this.xmlReader.Settings;

    /// <inheritdoc />
    public override String Value =>
        this.xmlReader.Value;

    /// <inheritdoc />
    public override Type ValueType =>
        this.xmlReader.ValueType;

    /// <inheritdoc />
    public override String XmlLang =>
        this.xmlReader.XmlLang;

    /// <inheritdoc />
    public override XmlSpace XmlSpace =>
        this.xmlReader.XmlSpace;

    /// <inheritdoc />
    public override void Close() =>
        this.xmlReader.Close();

    /// <inheritdoc />
    public override String GetAttribute(Int32 i) =>
        this.xmlReader.GetAttribute(i);

    /// <inheritdoc />
    public override String? GetAttribute(String name) =>
        this.xmlReader.GetAttribute(name);

    /// <inheritdoc />
    public override String? GetAttribute(String name, String? namespaceURI) =>
        this.xmlReader.GetAttribute(name, namespaceURI);

    /// <inheritdoc />
    public override Task<String> GetValueAsync() =>
        this.xmlReader.GetValueAsync();

    /// <inheritdoc />
    public override Boolean IsStartElement() =>
        this.xmlReader.IsStartElement();

    /// <inheritdoc />
    public override Boolean IsStartElement(String name) =>
        this.xmlReader.IsStartElement(name);

    /// <inheritdoc />
    public override Boolean IsStartElement(String localname, String ns) =>
        this.xmlReader.IsStartElement(localname, ns);

    /// <inheritdoc />
    public override String? LookupNamespace(String prefix) =>
        this.xmlReader.LookupNamespace(prefix);

    /// <inheritdoc />
    public override void MoveToAttribute(Int32 i) =>
        this.xmlReader.MoveToAttribute(i);

    /// <inheritdoc />
    public override Boolean MoveToAttribute(String name) =>
        this.xmlReader.MoveToAttribute(name);

    /// <inheritdoc />
    public override Boolean MoveToAttribute(String name, String? ns) =>
        this.xmlReader.MoveToAttribute(name, ns);

    /// <inheritdoc />
    public override XmlNodeType MoveToContent() =>
        this.xmlReader.MoveToContent();

    /// <inheritdoc />
    public override Task<XmlNodeType> MoveToContentAsync() =>
        this.xmlReader.MoveToContentAsync();

    /// <inheritdoc />
    public override Boolean MoveToElement() =>
        this.xmlReader.MoveToElement();

    /// <inheritdoc />
    public override Boolean MoveToFirstAttribute() =>
        this.xmlReader.MoveToFirstAttribute();

    /// <inheritdoc />
    public override Boolean MoveToNextAttribute() =>
        this.xmlReader.MoveToNextAttribute();

    /// <inheritdoc />
    public override Boolean Read() =>
        this.xmlReader.Read();

    /// <inheritdoc />
    public override Task<Boolean> ReadAsync() =>
        this.xmlReader.ReadAsync();

    /// <inheritdoc />
    public override Boolean ReadAttributeValue() =>
        this.xmlReader.ReadAttributeValue();

    /// <inheritdoc />
    public override Object ReadContentAs(Type returnType, IXmlNamespaceResolver? namespaceResolver) =>
        this.xmlReader.ReadContentAs(returnType, namespaceResolver);

    /// <inheritdoc />
    public override Task<Object> ReadContentAsAsync(Type returnType, IXmlNamespaceResolver? namespaceResolver) =>
        this.xmlReader.ReadContentAsAsync(returnType, namespaceResolver);

    /// <inheritdoc />
    public override Int32 ReadContentAsBase64(Byte[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadContentAsBase64(buffer, index, count);

    /// <inheritdoc />
    public override Task<Int32> ReadContentAsBase64Async(Byte[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadContentAsBase64Async(buffer, index, count);

    /// <inheritdoc />
    public override Int32 ReadContentAsBinHex(Byte[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadContentAsBinHex(buffer, index, count);

    /// <inheritdoc />
    public override Task<Int32> ReadContentAsBinHexAsync(Byte[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadContentAsBinHexAsync(buffer, index, count);

    /// <inheritdoc />
    public override Boolean ReadContentAsBoolean() =>
        this.xmlReader.ReadContentAsBoolean();

    /// <inheritdoc />
    public override DateTime ReadContentAsDateTime() =>
        this.xmlReader.ReadContentAsDateTime();

    /// <inheritdoc />
    public override DateTimeOffset ReadContentAsDateTimeOffset() =>
        this.xmlReader.ReadContentAsDateTimeOffset();

    /// <inheritdoc />
    public override Decimal ReadContentAsDecimal() =>
        this.xmlReader.ReadContentAsDecimal();

    /// <inheritdoc />
    public override Double ReadContentAsDouble() =>
        this.xmlReader.ReadContentAsDouble();

    /// <inheritdoc />
    public override Single ReadContentAsFloat() =>
        this.xmlReader.ReadContentAsFloat();

    /// <inheritdoc />
    public override Int32 ReadContentAsInt() =>
        this.xmlReader.ReadContentAsInt();

    /// <inheritdoc />
    public override Int64 ReadContentAsLong() =>
        this.xmlReader.ReadContentAsLong();

    /// <inheritdoc />
    public override Object ReadContentAsObject() =>
        this.xmlReader.ReadContentAsObject();

    /// <inheritdoc />
    public override Task<Object> ReadContentAsObjectAsync() =>
        this.xmlReader.ReadContentAsObjectAsync();

    /// <inheritdoc />
    public override String ReadContentAsString() =>
        this.xmlReader.ReadContentAsString();

    /// <inheritdoc />
    public override Task<String> ReadContentAsStringAsync() =>
        this.xmlReader.ReadContentAsStringAsync();

    /// <inheritdoc />
    public override Object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver) =>
        this.xmlReader.ReadElementContentAs(returnType, namespaceResolver);

    /// <inheritdoc />
    public override Object ReadElementContentAs(
        Type returnType,
        IXmlNamespaceResolver namespaceResolver,
        String localName,
        String namespaceURI
    ) =>
        this.xmlReader.ReadElementContentAs(returnType, namespaceResolver, localName, namespaceURI);

    /// <inheritdoc />
    public override Task<Object> ReadElementContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver) =>
        this.xmlReader.ReadElementContentAsAsync(returnType, namespaceResolver);

    /// <inheritdoc />
    public override Int32 ReadElementContentAsBase64(Byte[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadElementContentAsBase64(buffer, index, count);

    /// <inheritdoc />
    public override Task<Int32> ReadElementContentAsBase64Async(Byte[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadElementContentAsBase64Async(buffer, index, count);

    /// <inheritdoc />
    public override Int32 ReadElementContentAsBinHex(Byte[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadElementContentAsBinHex(buffer, index, count);

    /// <inheritdoc />
    public override Task<Int32> ReadElementContentAsBinHexAsync(Byte[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadElementContentAsBinHexAsync(buffer, index, count);

    /// <inheritdoc />
    public override Boolean ReadElementContentAsBoolean() =>
        this.xmlReader.ReadElementContentAsBoolean();

    /// <inheritdoc />
    public override Boolean ReadElementContentAsBoolean(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsBoolean(localName, namespaceURI);

    /// <inheritdoc />
    public override DateTime ReadElementContentAsDateTime() =>
        this.xmlReader.ReadElementContentAsDateTime();

    /// <inheritdoc />
    public override DateTime ReadElementContentAsDateTime(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsDateTime(localName, namespaceURI);

    /// <inheritdoc />
    public override Decimal ReadElementContentAsDecimal() =>
        this.xmlReader.ReadElementContentAsDecimal();

    /// <inheritdoc />
    public override Decimal ReadElementContentAsDecimal(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsDecimal(localName, namespaceURI);

    /// <inheritdoc />
    public override Double ReadElementContentAsDouble() =>
        this.xmlReader.ReadElementContentAsDouble();

    /// <inheritdoc />
    public override Double ReadElementContentAsDouble(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsDouble(localName, namespaceURI);

    /// <inheritdoc />
    public override Single ReadElementContentAsFloat() =>
        this.xmlReader.ReadElementContentAsFloat();

    /// <inheritdoc />
    public override Single ReadElementContentAsFloat(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsFloat(localName, namespaceURI);

    /// <inheritdoc />
    public override Int32 ReadElementContentAsInt() =>
        this.xmlReader.ReadElementContentAsInt();

    /// <inheritdoc />
    public override Int32 ReadElementContentAsInt(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsInt(localName, namespaceURI);

    /// <inheritdoc />
    public override Int64 ReadElementContentAsLong() =>
        this.xmlReader.ReadElementContentAsLong();

    /// <inheritdoc />
    public override Int64 ReadElementContentAsLong(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsLong(localName, namespaceURI);

    /// <inheritdoc />
    public override Object ReadElementContentAsObject() =>
        this.xmlReader.ReadElementContentAsObject();

    /// <inheritdoc />
    public override Object ReadElementContentAsObject(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsObject(localName, namespaceURI);

    /// <inheritdoc />
    public override Task<Object> ReadElementContentAsObjectAsync() =>
        this.xmlReader.ReadElementContentAsObjectAsync();

    /// <inheritdoc />
    public override String ReadElementContentAsString() =>
        this.xmlReader.ReadElementContentAsString();

    /// <inheritdoc />
    public override String ReadElementContentAsString(String localName, String namespaceURI) =>
        this.xmlReader.ReadElementContentAsString(localName, namespaceURI);

    /// <inheritdoc />
    public override Task<String> ReadElementContentAsStringAsync() =>
        this.xmlReader.ReadElementContentAsStringAsync();

    /// <inheritdoc />
    public override String ReadElementString() =>
        this.xmlReader.ReadElementString();

    /// <inheritdoc />
    public override String ReadElementString(String name) =>
        this.xmlReader.ReadElementString(name);

    /// <inheritdoc />
    public override String ReadElementString(String localname, String ns) =>
        this.xmlReader.ReadElementString(localname, ns);

    /// <inheritdoc />
    public override void ReadEndElement() =>
        this.xmlReader.ReadEndElement();

    /// <inheritdoc />
    public override String ReadInnerXml() =>
        this.xmlReader.ReadInnerXml();

    /// <inheritdoc />
    public override Task<String> ReadInnerXmlAsync() =>
        this.xmlReader.ReadInnerXmlAsync();

    /// <inheritdoc />
    public override String ReadOuterXml() =>
        this.xmlReader.ReadOuterXml();

    /// <inheritdoc />
    public override Task<String> ReadOuterXmlAsync() =>
        this.xmlReader.ReadOuterXmlAsync();

    /// <inheritdoc />
    public override void ReadStartElement() =>
        this.xmlReader.ReadStartElement();

    /// <inheritdoc />
    public override void ReadStartElement(String name) =>
        this.xmlReader.ReadStartElement(name);

    /// <inheritdoc />
    public override void ReadStartElement(String localname, String ns) =>
        this.xmlReader.ReadStartElement(localname, ns);

    /// <inheritdoc />
    public override String ReadString() =>
        this.xmlReader.ReadString();

    /// <inheritdoc />
    public override XmlReader ReadSubtree() =>
        this.xmlReader.ReadSubtree();

    /// <inheritdoc />
    public override Boolean ReadToDescendant(String name) =>
        this.xmlReader.ReadToDescendant(name);

    /// <inheritdoc />
    public override Boolean ReadToDescendant(String localName, String namespaceURI) =>
        this.xmlReader.ReadToDescendant(localName, namespaceURI);

    /// <inheritdoc />
    public override Boolean ReadToFollowing(String name) =>
        this.xmlReader.ReadToFollowing(name);

    /// <inheritdoc />
    public override Boolean ReadToFollowing(String localName, String namespaceURI) =>
        this.xmlReader.ReadToFollowing(localName, namespaceURI);

    /// <inheritdoc />
    public override Boolean ReadToNextSibling(String name) =>
        this.xmlReader.ReadToNextSibling(name);

    /// <inheritdoc />
    public override Boolean ReadToNextSibling(String localName, String namespaceURI) =>
        this.xmlReader.ReadToNextSibling(localName, namespaceURI);

    /// <inheritdoc />
    public override Int32 ReadValueChunk(Char[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadValueChunk(buffer, index, count);

    /// <inheritdoc />
    public override Task<Int32> ReadValueChunkAsync(Char[] buffer, Int32 index, Int32 count) =>
        this.xmlReader.ReadValueChunkAsync(buffer, index, count);

    /// <inheritdoc />
    public override void ResolveEntity() =>
        this.xmlReader.ResolveEntity();

    /// <inheritdoc />
    public override void Skip() =>
        this.xmlReader.Skip();

    /// <inheritdoc />
    public override Task SkipAsync() =>
        this.xmlReader.SkipAsync();

    /// <summary>
    /// A function that is invoked when this instance is being disposed synchronously.
    /// </summary>
    internal Action? OnDisposing { get; set; }

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;

        base.Dispose(disposing);

        if (disposing)
        {
            this.xmlReader.Dispose();

            this.OnDisposing?.Invoke();
        }
    }

    private readonly XmlReader xmlReader;
    private Boolean isDisposed;
}
