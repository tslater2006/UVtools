﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace UVtools.Core.Extensions;

public static class XmlExtensions
{
    public static readonly XmlWriterSettings SettingsIndent = new()
    {
        CloseOutput = false,
        Indent = true,
    };

    public static void Serialize(object toSerialize, Stream stream, bool noNameSpace = false)
    {
        var xmlSerializer = new XmlSerializer(toSerialize.GetType());
        XmlSerializerNamespaces? ns = null;
        if (noNameSpace)
        {
            ns = new();
            ns.Add("", "");
        }
        xmlSerializer.Serialize(stream, toSerialize, ns);
    }

    public static void Serialize(object toSerialize, Stream stream, XmlWriterSettings settings, bool noNameSpace = false, bool standalone = false)
    {
        settings.CloseOutput = false;

        using var xw = XmlWriter.Create(stream, settings);
        xw.WriteStartDocument(standalone); // that bool parameter is called "standalone"

        var s = new XmlSerializer(toSerialize.GetType());
        XmlSerializerNamespaces? ns = null;
        if (noNameSpace)
        {
            ns = new();
            ns.Add("", "");
        }
        s.Serialize(xw, toSerialize, ns);
    }

    public static void Serialize(object toSerialize, Stream stream, Encoding encoding, bool indent = true, bool omitXmlDeclaration = false, bool noNameSpace = false, bool standalone = false)
    {
        var settings = new XmlWriterSettings
        {
            // If set to true XmlWriter would close MemoryStream automatically and using would then do double dispose
            // Code analysis does not understand that. That's why there is a suppress message.
            CloseOutput = false,
            Encoding = encoding,
            OmitXmlDeclaration = omitXmlDeclaration,
            Indent = indent,
        };
        Serialize(toSerialize, stream, settings, noNameSpace, standalone);
    }

    public static void SerializeToFile(object toSerialize, string path, bool noNameSpace = false)
    {
        using var stream = new FileStream(path, FileMode.Create);
        Serialize(toSerialize, stream, noNameSpace);
    }

    public static void SerializeToFile(object toSerialize, string path, XmlWriterSettings settings, bool noNameSpace = false, bool standalone = false)
    {
        using var stream = new FileStream(path, FileMode.Create);
        Serialize(toSerialize, stream, settings, noNameSpace, standalone);
    }

    public static void SerializeToFile(object toSerialize, string path, Encoding encoding, bool indent = true, bool omitXmlDeclaration = false, bool noNameSpace = false, bool standalone = false)
    {
        using var stream = new FileStream(path, FileMode.Create);
        Serialize(toSerialize, stream, encoding, indent, omitXmlDeclaration, noNameSpace, standalone);
    }


    public static string SerializeObject(object toSerialize, bool noNameSpace = false)
    {
        using var stream = new MemoryStream();
        Serialize(toSerialize, stream, noNameSpace);
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static string SerializeObject(object toSerialize, XmlWriterSettings settings, bool noNameSpace = false, bool standalone = false)
    {
        using var stream = new MemoryStream();
        Serialize(toSerialize, stream, settings, noNameSpace, standalone);
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static string SerializeObject(object toSerialize, Encoding encoding, bool indent = true, bool omitXmlDeclaration = false, bool noNameSpace = false, bool standalone = false)
    {
        using var stream = new MemoryStream();
        Serialize(toSerialize, stream, encoding, indent, omitXmlDeclaration, noNameSpace, standalone);
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static T DeserializeFromStream<T>(Stream stream)
    {
        var serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(stream)!;
    }

    public static T DeserializeFromFile<T>(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return DeserializeFromStream<T>(stream);
    }

    public static T DeserializeFromText<T>(string text)
    {
        var serializer = new XmlSerializer(typeof(T));
        using TextReader reader = new StringReader(text);
        return (T)serializer.Deserialize(reader)!;
    }

}