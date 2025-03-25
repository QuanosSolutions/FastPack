using System;
using System.Collections.Generic;
using FastPack.Lib;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Options.Parsers;

namespace FastPack.Tests.FastPack.Options.Parsers;

public class UnpackOptionsParserTests : OptionsParserTestBase<UnpackOptionsParser, UnpackOptions>
{
	// ReSharper disable once UnusedMember.Local
	// Use by base class through reflection
	private static IEnumerable<OptionsParserTestCaseData<UnpackOptions>> GetTestCaseData(bool strictMode)
	{
		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = Array.Empty<string>(),
			ExpectedOptions = new UnpackOptions(),
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-i", "test1" },
			ExpectedOptions = new UnpackOptions { InputFilePath = "test1" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--input", "test1" },
			ExpectedOptions = new UnpackOptions { InputFilePath = "test1" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-o", "test2" },
			ExpectedOptions = new UnpackOptions { OutputDirectoryPath = "test2" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--output", "test2" },
			ExpectedOptions = new UnpackOptions { OutputDirectoryPath = "test2" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-if", "includefilter1" },
			ExpectedOptions = new UnpackOptions { IncludeFilters = { "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--include-filter", "includefilter1" },
			ExpectedOptions = new UnpackOptions { IncludeFilters = { "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-if", "includefilter1", "-if", "includefilter2" },
			ExpectedOptions = new UnpackOptions { IncludeFilters = { "includefilter1", "includefilter2" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-if", "includefilter2", "-if", "includefilter1" },
			ExpectedOptions = new UnpackOptions { IncludeFilters = { "includefilter2", "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-ef", "excludefilter1" },
			ExpectedOptions = new UnpackOptions { ExcludeFilters = { "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--exclude-filter", "excludefilter1" },
			ExpectedOptions = new UnpackOptions { ExcludeFilters = { "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-ef", "excludefilter1", "-ef", "excludefilter2" },
			ExpectedOptions = new UnpackOptions { ExcludeFilters = { "excludefilter1", "excludefilter2" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-ef", "excludefilter2", "-ef", "excludefilter1" },
			ExpectedOptions = new UnpackOptions { ExcludeFilters = { "excludefilter2", "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-fci"},
			ExpectedOptions = new UnpackOptions { FilterCaseInsensitive = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--filter-case-insensitive" },
			ExpectedOptions = new UnpackOptions { FilterCaseInsensitive = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-ft", "Glob" },
			ExpectedOptions = new UnpackOptions { FilterType = TextMatchProviderType.Glob },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--filter-type", "Regex" },
			ExpectedOptions = new UnpackOptions { FilterType = TextMatchProviderType.Regex },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-p", "1" },
			ExpectedOptions = new UnpackOptions { MaxDegreeOfParallelism = 1 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--parallelism", "1%" },
			ExpectedOptions = new UnpackOptions { MaxDegreeOfParallelism = 1 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-p", "-1" },
			ExpectedOptions = new UnpackOptions { MaxDegreeOfParallelism = Environment.ProcessorCount-1 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-p", "100%" },
			ExpectedOptions = new UnpackOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-p", "-25%" },
			ExpectedOptions = new UnpackOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - (int)Math.Floor(0.25 * Environment.ProcessorCount)) },
			StrictMode = strictMode
		};

		// can not test negative values for max memory here, because calculating the expected memory is not exact. The memory changes between the call and the calculation

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-dr" },
			ExpectedOptions = new UnpackOptions { DryRun = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--dryrun", "detailed" },
			ExpectedOptions = new UnpackOptions { DryRun = true, DetailedDryRun = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-drf", "Text" },
			ExpectedOptions = new UnpackOptions { DryRunOutputFormat = OutputFormat.Text},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--dryrunformat", "jSon" },
			ExpectedOptions = new UnpackOptions { DryRunOutputFormat = OutputFormat.Json, QuietMode = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-drf", "XML" },
			ExpectedOptions = new UnpackOptions { DryRunOutputFormat = OutputFormat.Xml, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--pretty"},
			ExpectedOptions = new UnpackOptions { PrettyPrint = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-np" },
			ExpectedOptions = new UnpackOptions { ShowProgress = false },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--no-progress" },
			ExpectedOptions = new UnpackOptions { ShowProgress = false },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-nd" },
			ExpectedOptions = new UnpackOptions { RestoreDates = false },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--no-dates" },
			ExpectedOptions = new UnpackOptions { RestoreDates = false },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-nfp" },
			ExpectedOptions = new UnpackOptions { RestorePermissions = false },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--no-permissions" },
			ExpectedOptions = new UnpackOptions { RestorePermissions = false },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-isc" },
			ExpectedOptions = new UnpackOptions { IgnoreDiskSpaceCheck = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "--ignore-space-check" },
			ExpectedOptions = new UnpackOptions { IgnoreDiskSpaceCheck = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<UnpackOptions> {
			Args = new[] { "-i", "test1", "--output", "test2", "-if", "includefilter1", "-if", "includefilter2", "-ef", "excludefilter2", "-ef", "excludefilter1", "-fci", "--filter-type", "Regex", "--pretty" },
			ExpectedOptions = new UnpackOptions {
				InputFilePath = "test1",
				OutputDirectoryPath = "test2",
				IncludeFilters = { "includefilter1", "includefilter2" },
				ExcludeFilters = { "excludefilter2", "excludefilter1" },
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Regex,
				PrettyPrint = true
			},
			StrictMode = strictMode
		};
	}
}