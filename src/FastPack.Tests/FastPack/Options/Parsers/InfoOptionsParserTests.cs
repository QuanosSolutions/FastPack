using System;
using System.Collections.Generic;
using FastPack.Lib;
using FastPack.Lib.Options;
using FastPack.Options.Parsers;

namespace FastPack.Tests.FastPack.Options.Parsers;

public class InfoOptionsParserTests : OptionsParserTestBase<InfoOptionsParser, InfoOptions>
{
	// ReSharper disable once UnusedMember.Local
	// Use by base class through reflection
	private static IEnumerable<OptionsParserTestCaseData<InfoOptions>> GetTestCaseData(bool strictMode)
	{
		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = Array.Empty<string>(),
			ExpectedOptions = new InfoOptions(),
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "-i", "test" },
			ExpectedOptions = new InfoOptions { InputFilePath = "test" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "--input", "test" },
			ExpectedOptions = new InfoOptions { InputFilePath = "test" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "-d", },
			ExpectedOptions = new InfoOptions { Detailed = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "--detailed", },
			ExpectedOptions = new InfoOptions { Detailed = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "-f", "Text" },
			ExpectedOptions = new InfoOptions { OutputFormat = OutputFormat.Text },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "--format", "Text" },
			ExpectedOptions = new InfoOptions { OutputFormat = OutputFormat.Text },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "-f", "Json" },
			ExpectedOptions = new InfoOptions { OutputFormat = OutputFormat.Json, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "--format", "Json" },
			ExpectedOptions = new InfoOptions { OutputFormat = OutputFormat.Json, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "-f", "Xml" },
			ExpectedOptions = new InfoOptions { OutputFormat = OutputFormat.Xml, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "--format", "Xml" },
			ExpectedOptions = new InfoOptions { OutputFormat = OutputFormat.Xml, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "-f", "xML" },
			ExpectedOptions = new InfoOptions { OutputFormat = OutputFormat.Xml, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "--format", "xMl" },
			ExpectedOptions = new InfoOptions { OutputFormat = OutputFormat.Xml, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "-i", "test", "--detailed", "-f", "Xml", "--pretty" },
			ExpectedOptions = new InfoOptions { InputFilePath = "test", Detailed = true, OutputFormat = OutputFormat.Xml, PrettyPrint = true, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "--detailed", "-i", "test", "--pretty", "-f", "Xml" },
			ExpectedOptions = new InfoOptions { InputFilePath = "test", Detailed = true, OutputFormat = OutputFormat.Xml, PrettyPrint = true, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<InfoOptions> {
			Args = new[] { "--pretty", "--detailed", "-f", "Xml", "-i", "test" },
			ExpectedOptions = new InfoOptions { InputFilePath = "test", Detailed = true, OutputFormat = OutputFormat.Xml, PrettyPrint = true, QuietMode = true },
			StrictMode = strictMode
		};
	}
}