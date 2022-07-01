using System;
using System.Collections.Generic;
using FastPack.Lib;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Options.Parsers;

namespace FastPack.Tests.FastPack.Options.Parsers;

public class DiffOptionsParserTests : OptionsParserTestBase<DiffOptionsParser, DiffOptions>
{
	// ReSharper disable once UnusedMember.Local
	// Use by base class through reflection
	private static IEnumerable<OptionsParserTestCaseData<DiffOptions>> GetTestCaseData(bool strictMode)
	{
		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = Array.Empty<string>(),
			ExpectedOptions = new DiffOptions(),
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-1", "test1" },
			ExpectedOptions = new DiffOptions { FirstFilePath = "test1" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--first", "test1" },
			ExpectedOptions = new DiffOptions { FirstFilePath = "test1" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-2", "test2" },
			ExpectedOptions = new DiffOptions { SecondFilePath = "test2" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--second", "test2" },
			ExpectedOptions = new DiffOptions { SecondFilePath = "test2" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-s", "Size" },
			ExpectedOptions = new DiffOptions { DiffSettings = DiffOptions.DiffSetting.Size | DiffOptions.DiffSetting.StructureOnly },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--settings", "Size" },
			ExpectedOptions = new DiffOptions { DiffSettings = DiffOptions.DiffSetting.Size | DiffOptions.DiffSetting.StructureOnly },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-s", "Size", "--settings", "Date" },
			ExpectedOptions = new DiffOptions { DiffSettings = DiffOptions.DiffSetting.Size | DiffOptions.DiffSetting.Date | DiffOptions.DiffSetting.StructureOnly },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-s", "Size", "--settings", "Date", "-s", "Permission" },
			ExpectedOptions = new DiffOptions { DiffSettings = DiffOptions.DiffSetting.Size | DiffOptions.DiffSetting.Date | DiffOptions.DiffSetting.Permission | DiffOptions.DiffSetting.StructureOnly },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-if", "includefilter1" },
			ExpectedOptions = new DiffOptions { IncludeFilters = { "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--include-filter", "includefilter1" },
			ExpectedOptions = new DiffOptions { IncludeFilters = { "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-if", "includefilter1", "-if", "includefilter2" },
			ExpectedOptions = new DiffOptions { IncludeFilters = { "includefilter1", "includefilter2" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-if", "includefilter2", "-if", "includefilter1" },
			ExpectedOptions = new DiffOptions { IncludeFilters = { "includefilter2", "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-ef", "excludefilter1" },
			ExpectedOptions = new DiffOptions { ExcludeFilters = { "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--exclude-filter", "excludefilter1" },
			ExpectedOptions = new DiffOptions { ExcludeFilters = { "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-ef", "excludefilter1", "-ef", "excludefilter2" },
			ExpectedOptions = new DiffOptions { ExcludeFilters = { "excludefilter1", "excludefilter2" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-ef", "excludefilter2", "-ef", "excludefilter1" },
			ExpectedOptions = new DiffOptions { ExcludeFilters = { "excludefilter2", "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-fci"},
			ExpectedOptions = new DiffOptions { FilterCaseInsensitive = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--filter-case-insensitive" },
			ExpectedOptions = new DiffOptions { FilterCaseInsensitive = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-ft", "Glob" },
			ExpectedOptions = new DiffOptions { FilterType = TextMatchProviderType.Glob },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--filter-type", "Regex" },
			ExpectedOptions = new DiffOptions { FilterType = TextMatchProviderType.Regex },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-x" },
			ExpectedOptions = new DiffOptions { ExtractionEnabled = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--extract" },
			ExpectedOptions = new DiffOptions { ExtractionEnabled = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-x", "ExtractDirectory" },
			ExpectedOptions = new DiffOptions { ExtractionEnabled = true, ExtractionDirectoryPath = "ExtractDirectory" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--extract", "ExtractDirectory" },
			ExpectedOptions = new DiffOptions { ExtractionEnabled = true, ExtractionDirectoryPath = "ExtractDirectory" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-f", "Text" },
			ExpectedOptions = new DiffOptions { OutputFormat = OutputFormat.Text},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--format", "json" },
			ExpectedOptions = new DiffOptions { OutputFormat = OutputFormat.Json, QuietMode = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--format", "XmL" },
			ExpectedOptions = new DiffOptions { OutputFormat = OutputFormat.Xml, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "--pretty"},
			ExpectedOptions = new DiffOptions { PrettyPrint = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<DiffOptions> {
			Args = new[] { "-1", "test1", "--second", "test2", "-s", "Size", "--settings", "Date", "-s", "Permission", "-if", "includefilter1", "-if", "includefilter2", "-ef", "excludefilter2", "-ef", "excludefilter1", "-fci", "--filter-type", "Regex", "--format", "XmL", "--pretty" },
			ExpectedOptions = new DiffOptions {
				FirstFilePath = "test1",
				SecondFilePath = "test2",
				DiffSettings = DiffOptions.DiffSetting.Size | DiffOptions.DiffSetting.Date | DiffOptions.DiffSetting.Permission | DiffOptions.DiffSetting.StructureOnly,
				IncludeFilters = { "includefilter1", "includefilter2" },
				ExcludeFilters = { "excludefilter2", "excludefilter1" },
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Regex,
				OutputFormat = OutputFormat.Xml,
				QuietMode = true,
				PrettyPrint = true
			},
			StrictMode = strictMode
		};
	}
}