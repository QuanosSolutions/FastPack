using System;
using System.Collections.Generic;
using FastPack.Lib;
using FastPack.Lib.Hashing;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using FastPack.Options.Parsers;

namespace FastPack.Tests.FastPack.Options.Parsers;

public class PackOptionsParserTests : OptionsParserTestBase<PackOptionsParser, PackOptions>
{
	// ReSharper disable once UnusedMember.Local
	// Use by base class through reflection
	private static IEnumerable<OptionsParserTestCaseData<PackOptions>> GetTestCaseData(bool strictMode)
	{
		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = Array.Empty<string>(),
			ExpectedOptions = new PackOptions(),
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-i", "test1" },
			ExpectedOptions = new PackOptions { InputDirectoryPath = "test1" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--input", "test1" },
			ExpectedOptions = new PackOptions { InputDirectoryPath = "test1" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-o", "test2" },
			ExpectedOptions = new PackOptions { OutputFilePath = "test2" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--output", "test2" },
			ExpectedOptions = new PackOptions { OutputFilePath = "test2" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-if", "includefilter1" },
			ExpectedOptions = new PackOptions { IncludeFilters = { "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--include-filter", "includefilter1" },
			ExpectedOptions = new PackOptions { IncludeFilters = { "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-if", "includefilter1", "-if", "includefilter2" },
			ExpectedOptions = new PackOptions { IncludeFilters = { "includefilter1", "includefilter2" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-if", "includefilter2", "-if", "includefilter1" },
			ExpectedOptions = new PackOptions { IncludeFilters = { "includefilter2", "includefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-ef", "excludefilter1" },
			ExpectedOptions = new PackOptions { ExcludeFilters = { "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--exclude-filter", "excludefilter1" },
			ExpectedOptions = new PackOptions { ExcludeFilters = { "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-ef", "excludefilter1", "-ef", "excludefilter2" },
			ExpectedOptions = new PackOptions { ExcludeFilters = { "excludefilter1", "excludefilter2" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-ef", "excludefilter2", "-ef", "excludefilter1" },
			ExpectedOptions = new PackOptions { ExcludeFilters = { "excludefilter2", "excludefilter1" } },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-fci"},
			ExpectedOptions = new PackOptions { FilterCaseInsensitive = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--filter-case-insensitive" },
			ExpectedOptions = new PackOptions { FilterCaseInsensitive = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-ft", "Glob" },
			ExpectedOptions = new PackOptions { FilterType = TextMatchProviderType.Glob },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--filter-type", "Regex" },
			ExpectedOptions = new PackOptions { FilterType = TextMatchProviderType.Regex },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-p", "1" },
			ExpectedOptions = new PackOptions { MaxDegreeOfParallelism = 1 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--parallelism", "1%" },
			ExpectedOptions = new PackOptions { MaxDegreeOfParallelism = 1 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-p", "-1" },
			ExpectedOptions = new PackOptions { MaxDegreeOfParallelism = Environment.ProcessorCount-1 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-p", "100%" },
			ExpectedOptions = new PackOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-p", "-25%" },
			ExpectedOptions = new PackOptions { MaxDegreeOfParallelism = Math.Max(1, (Environment.ProcessorCount - (int) Math.Floor(0.25 * Environment.ProcessorCount))) },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-mm", "1G" },
			ExpectedOptions = new PackOptions { MaxMemory = 1024*1024*1024 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-mm", "1gb" },
			ExpectedOptions = new PackOptions { MaxMemory = 1024 * 1024 * 1024 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--maxmemory", "1M" },
			ExpectedOptions = new PackOptions { MaxMemory = 1024 * 1024 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--maxmemory", "1mb" },
			ExpectedOptions = new PackOptions { MaxMemory = 1024 * 1024 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-mm", "1K" },
			ExpectedOptions = new PackOptions { MaxMemory = 1024 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-mm", "1kb" },
			ExpectedOptions = new PackOptions { MaxMemory = 1024 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-mm", "500" },
			ExpectedOptions = new PackOptions { MaxMemory = 500 },
			StrictMode = strictMode
		};

		// can not test negative values for max memory here, because calculating the expected memory is not exact. The memory changes between the call and the calculation

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-c", "hello world" },
			ExpectedOptions = new PackOptions { Comment = "hello world" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--comment", "hello world" },
			ExpectedOptions = new PackOptions { Comment = "hello world" },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-ca", "Deflate" },
			ExpectedOptions = new PackOptions { CompressionAlgorithm = CompressionAlgorithm.Deflate },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--compressionalgorithm", "NoCompression" },
			ExpectedOptions = new PackOptions { CompressionAlgorithm = CompressionAlgorithm.NoCompression },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-cl", "0" },
			ExpectedOptions = new PackOptions { CompressionLevel = 0 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--compressionlevel", "50" },
			ExpectedOptions = new PackOptions { CompressionLevel = 50 },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-ha", "XXHash" },
			ExpectedOptions = new PackOptions { HashAlgorithm = HashAlgorithm.XXHash },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--hashalgorithm", "XXHash" },
			ExpectedOptions = new PackOptions { HashAlgorithm = HashAlgorithm.XXHash },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-dr" },
			ExpectedOptions = new PackOptions { DryRun = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--dryrun", "detailed" },
			ExpectedOptions = new PackOptions { DryRun = true, DetailedDryRun = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-drf", "Text" },
			ExpectedOptions = new PackOptions { DryRunOutputFormat = OutputFormat.Text},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--dryrunformat", "jSon" },
			ExpectedOptions = new PackOptions { DryRunOutputFormat = OutputFormat.Json, QuietMode = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-drf", "XML" },
			ExpectedOptions = new PackOptions { DryRunOutputFormat = OutputFormat.Xml, QuietMode = true },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--pretty"},
			ExpectedOptions = new PackOptions { PrettyPrint = true},
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-np" },
			ExpectedOptions = new PackOptions { ShowProgress = false },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "--no-progress" },
			ExpectedOptions = new PackOptions { ShowProgress = false },
			StrictMode = strictMode
		};

		yield return new OptionsParserTestCaseData<PackOptions> {
			Args = new[] { "-i", "test1", "--output", "test2", "-if", "includefilter1", "-if", "includefilter2", "-ef", "excludefilter2", "-ef", "excludefilter1", "-fci", "--filter-type", "Regex", "--pretty" },
			ExpectedOptions = new PackOptions {
				InputDirectoryPath = "test1",
				OutputFilePath = "test2",
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