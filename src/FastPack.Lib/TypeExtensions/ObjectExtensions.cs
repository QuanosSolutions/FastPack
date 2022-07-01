using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Xml;
using System.Xml.Serialization;

namespace FastPack.Lib.TypeExtensions;

public static class ObjectExtensions
{
	public static IEnumerable<T> ToIEnumerable<T>(this T singleObject)
	{
		yield return singleObject;
	}

	public static string SerializeToJson<T>(this T singleObject, JsonTypeInfo<T> jsonTypeInfo)
	{
		return JsonSerializer.Serialize(singleObject, jsonTypeInfo);
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "XmlSerializer has currently no possibility like JsonSerializer to fix the Trimming warnings, but since we already made sure with JsonSerializer that necessary classes don't get trimmed, we can ignore these warnings here.")]
	public static string SerializeToXml<T>(this T singleObject, bool prettyPrint)
	{
		XmlSerializer serializer = new(typeof(T));
		using Utf8StringWriter stringWriter = new();
		XmlWriterSettings settings = new();
		if (prettyPrint)
			settings.Indent = true;
		XmlSerializerNamespaces xmlSerializerNamespaces = new();
		xmlSerializerNamespaces.Add(string.Empty, string.Empty);
		using XmlWriter writer = XmlWriter.Create(stringWriter, settings);
		serializer.Serialize(writer, singleObject, xmlSerializerNamespaces);
		return stringWriter.ToString();
	}
}