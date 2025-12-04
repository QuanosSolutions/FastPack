using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using FastPack.Lib.TypeExtensions;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.TypeExtensions;

[TestFixture]
public class ObjectExtensionsTests
{
	[Test]
	public void Ensure_ToIEnumerable_Works()
	{
		// arrange
		object someObject = new object();

		// act
		List<object> objects = someObject.ToIEnumerable().ToList();

		// assert
		objects.Count.Should().Be(1);
		objects[0].Should().Be(someObject);
	}

	[Test]
	public void Ensure_SerializeToJson_Works()
	{
		// arrange
		string expected = "{\"testInteger\":42,\"testString\":\"test\"}";
		TestSerializableClass test = new TestSerializableClass { TestInteger = 42, TestString = "test" };
		TestSerializableClassJsonContext jsonContext = new TestSerializableClassJsonContext();

		// act
		string json = test.SerializeToJson(jsonContext.TestSerializableClass);

		// assert
		json.Should().Be(expected);
	}

	[Test]
	public void Ensure_SerializeToJson_PrettyPrint_Works()
	{
		// arrange
		string expected = "{\r\n  \"testInteger\": 42,\r\n  \"testString\": \"test\"\r\n}";
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			expected = expected.Replace("\r\n", "\n");
		}
		TestSerializableClass test = new TestSerializableClass { TestInteger = 42, TestString = "test" };
		TestSerializableClassJsonContext jsonContext = new TestSerializableClassJsonContext(new JsonSerializerOptions { WriteIndented = true });

		// act
		string json = test.SerializeToJson(jsonContext.TestSerializableClass);

		// assert
		json.Should().Be(expected);
	}

	[Test]
	public void Ensure_SerializeToXml_Works()
	{
		// arrange
		string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestSerializableClass><TestInteger>42</TestInteger><TestString>test</TestString></TestSerializableClass>";
		TestSerializableClass test = new TestSerializableClass { TestInteger = 42, TestString = "test" };

		// act
		string json = test.SerializeToXml(false);

		// assert
		json.Should().Be(expected);
	}

	[Test]
	public void Ensure_SerializeToXml_PrettyPrint_Works()
	{
		// arrange
		string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<TestSerializableClass>\r\n  <TestInteger>42</TestInteger>\r\n  <TestString>test</TestString>\r\n</TestSerializableClass>";
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			expected = expected.Replace("\r\n", "\n");
		}
		TestSerializableClass test = new TestSerializableClass { TestInteger = 42, TestString = "test" };

		// act
		string json = test.SerializeToXml(true);

		// assert
		json.Should().Be(expected);
	}
}

public class TestSerializableClass
{
	[JsonPropertyName("testInteger")]
	[XmlElement("TestInteger")]
	public int TestInteger { get; set; }
	[JsonPropertyName("testString")]
	[XmlElement("TestString")]
	public string TestString { get; set; }
}

[JsonSerializable(typeof(TestSerializableClass))]
internal partial class TestSerializableClassJsonContext : JsonSerializerContext
{
}