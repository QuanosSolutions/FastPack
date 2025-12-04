using FastPack.CmdLine;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.CmdLine
{
	[TestFixture]
	public class FileToCommandLineConverterFactoryTests
	{
		private FileToCommandLineConverterFactory Factory { get; set; }

		[SetUp]
		public void Setup()
		{
			Factory = new FileToCommandLineConverterFactory();
		}

		[Test]
		public void Ensure_Json_Extension_Returns_Json_Converter()
		{
			Factory.GetConverter("expected.json").Should().BeOfType<JsonFileToCommandLineConverter>("Json file was provided");
			Factory.GetConverter("expected.JsOn").Should().BeOfType<JsonFileToCommandLineConverter>("Json file was provided");
			Factory.GetConverter("C:\\expected.JsOn").Should().BeOfType<JsonFileToCommandLineConverter>("Json file was provided");
		}

		[Test]
		public void Ensure_Xml_Extension_Returns_Json_Converter()
		{
			Factory.GetConverter("expected.xml").Should().BeOfType<XmlFileToCommandLineConverter>("XML file was provided");
			Factory.GetConverter("expected.xMl").Should().BeOfType<XmlFileToCommandLineConverter>("XML file was provided");
			Factory.GetConverter("C:\\expected.XmL").Should().BeOfType<XmlFileToCommandLineConverter>("XML file was provided");
		}

		[Test]
		public void Ensure_Fallback_Is_Text_Converter()
		{
			Factory.GetConverter("expected.somethingelse").Should().BeOfType<TextFileToCommandLineConverter>("No XML or json file was provided");
			Factory.GetConverter("no_extension").Should().BeOfType<TextFileToCommandLineConverter>("No XML or json file was provided");
			Factory.GetConverter("C:\\no_extension_with_full_path").Should().BeOfType<TextFileToCommandLineConverter>("XML file was provided");
		}
	}
}
