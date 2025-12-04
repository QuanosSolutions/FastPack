using System;
using System.Collections.Generic;
using System.Linq;
using FastPack.Lib;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib;

[TestFixture]
public class FilterTests
{
	private const char PathSeparator = '/';

	[Test, TestCaseSource(nameof(GetTestCaseData))]
	public void Test(FilterTestCaseData testData)
	{
		// act
		List<FilterFileSystemRecord> filteredRecords = Filter(testData.Input, testData.Options, testData.TextMatchProviderFactory);

		// assert
		filteredRecords.OrderBy(e => e.Path).Select(e => e.Path).Should().BeEquivalentTo(testData.ExpectedOutputDiff != null
			? testData.Input.Except(testData.ExpectedOutputDiff, new FilterFileSystemRecordEqualityComparer()).OrderBy(e => e.Path).Select(e => e.Path)
			: testData.ExpectedOutput.OrderBy(e => e.Path).Select(e => e.Path));
	}

	private static IEnumerable<FilterTestCaseData> GetTestCaseData()
	{
		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = false,
				FilterType = TextMatchProviderType.Glob,
			},
			Input = new FilterFileSystemRecord[] { },
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Empty_Filter_Empty_Input_Mocked_TextMatchProviderFactory",
			TextMatchProviderFactory = Mock.Of<ITextMatchProviderFactory>()
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = false,
				FilterType = TextMatchProviderType.Glob,
			},
			Input = new FilterFileSystemRecord[] { },
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Empty_Filter_Empty_Input"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = false,
				FilterType = TextMatchProviderType.Glob,
				ExcludeFilters = { "File1" },
			},
			Input = new FilterFileSystemRecord[] { },
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Single_Exclude_Empty_Input"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = false,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "File1" },
				ExcludeFilters = { "File2" },
			},
			Input = new FilterFileSystemRecord[] { },
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Single_Exclude_And_Include_Empty_Input"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = false,
				FilterType = TextMatchProviderType.Glob,
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/"
			},
			ExpectedOutputDiff = new FilterFileSystemRecord[] {},
			TestName = "Empty_Filter_Filled_Input"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = false,
				FilterType = TextMatchProviderType.Glob,
				ExcludeFilters = { "File1" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/"
			},
			ExpectedOutputDiff = new FilterFileSystemRecord[] {
				"File1"
			},
			TestName = "Single_Matching_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				ExcludeFilters = { "file1" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/"
			},
			ExpectedOutputDiff = new FilterFileSystemRecord[] {
				"File1"
			},
			TestName = "Single_Matching_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = false,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "File1" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/"
			},
			ExpectedOutput = new FilterFileSystemRecord[] {
				"File1"
			},
			TestName = "Single_Matching_Include"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "file1" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/"
			},
			ExpectedOutput = new FilterFileSystemRecord[] {
				"File1"
			},
			TestName = "Single_Matching_Include"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = false,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "File1" },
				ExcludeFilters = { "File1" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/"
			},
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Single_Same_Include_And_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "file1" },
				ExcludeFilters = { "File1" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/"
			},
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Single_Same_Include_And_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "Dir1" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			ExpectedOutput = new FilterFileSystemRecord[] {
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			TestName = "Single_Matching_Dir_Include"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "Dir1/**" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			ExpectedOutput = new FilterFileSystemRecord[] {
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			TestName = "Single_Matching_Dir_Recursive_Include"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				ExcludeFilters = { "Dir1" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			ExpectedOutputDiff = new FilterFileSystemRecord[] {
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			TestName = "Single_Matching_Dir_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				ExcludeFilters = { "Dir1/**" },
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			ExpectedOutputDiff = new FilterFileSystemRecord[] {
				"Dir1/File1",
				"Dir1/File2"
			},
			TestName = "Single_Matching_Dir_Recursive_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "Dir1/**" },
				ExcludeFilters = { "Dir1" }
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Recursive_Dir_Include_With_Same_Dir_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "Dir1/**" },
				ExcludeFilters = { "Dir1/**" }
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Recursive_Dir_Include_With_Same_Dir_Recursive_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "Dir1" },
				ExcludeFilters = { "Dir1/**" }
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/",
				"Dir1/File1",
				"Dir1/File2"
			},
			ExpectedOutput = new FilterFileSystemRecord[] {
				"Dir1/"
			},
			TestName = "Dir_Include_With_Same_Dir_Recursive_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "File1", "Dir1", "Dir2/**", "Dir3" },
				ExcludeFilters = { "Dir2/Dir2/**", "**/*.pdb", "**/Dir1/*.exe", "**/Dir2/**/*.exe", "**/Dir3/*.dll" }
			},
			Input = new FilterFileSystemRecord[] {
				"File1",
				"File2",
				"Dir1/",
				"Dir1/file1.exe",
				"Dir1/file2.dll",
				"Dir1/file3.pdb",
				"Dir1/Dir1/",
				"Dir1/Dir1/file1.exe",
				"Dir1/Dir1/file2.dll",
				"Dir1/Dir1/file3.pdb",
				"Dir1/Dir2/",
				"Dir1/Dir2/file1.exe",
				"Dir1/Dir2/file2.dll",
				"Dir1/Dir2/file3.pdb",
				"Dir1/Dir2/Dir1",
				"Dir1/Dir2/Dir1/file1.exe",
				"Dir1/Dir2/Dir1/file2.dll",
				"Dir1/Dir2/Dir1/file3.pdb",
				"Dir1/Dir3/",
				"Dir1/Dir3/file1.exe",
				"Dir1/Dir3/file2.dll",
				"Dir1/Dir3/file3.pdb",
				"Dir2/",
				"Dir2/file1.exe",
				"Dir2/file2.dll",
				"Dir2/file3.pdb",
				"Dir2/Dir1/",
				"Dir2/Dir1/file1.exe",
				"Dir2/Dir1/file2.dll",
				"Dir2/Dir1/file3.pdb",
				"Dir2/Dir2/",
				"Dir2/Dir2/file1.exe",
				"Dir2/Dir2/file2.dll",
				"Dir2/Dir2/file3.pdb",
				"Dir2/Dir3/",
				"Dir2/Dir3/file1.exe",
				"Dir2/Dir3/file2.dll",
				"Dir2/Dir3/file3.pdb",
				"Dir3/",
				"Dir3/file1.exe",
				"Dir3/file2.dll",
				"Dir3/file3.pdb",
			},
			ExpectedOutput = new FilterFileSystemRecord[] {
				"File1",
				"Dir1/",
				"Dir1/file2.dll",
				"Dir1/Dir1/",
				"Dir1/Dir1/file2.dll",
				"Dir1/Dir2/",
				"Dir1/Dir2/file2.dll",
				"Dir1/Dir2/Dir1",
				"Dir1/Dir2/Dir1/file2.dll",
				"Dir1/Dir3/",
				"Dir1/Dir3/file1.exe",
				"Dir2/",
				"Dir2/Dir2/",
				"Dir2/file2.dll",
				"Dir2/Dir1/",
				"Dir2/Dir1/file2.dll",
				"Dir2/Dir3/",
				"Dir3/",
				"Dir3/file1.exe",
			},
			TestName = "Complex_Include_Exclude"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "Dir1/**" },
				ExcludeFilters = { "Dir1/*.exe", "Dir1/*.dll" }
			},
			Input = new FilterFileSystemRecord[] {
				"Dir1/",
				"Dir1/file1.exe",
				"Dir1/file2.dll",
			},
			ExpectedOutput = new FilterFileSystemRecord[] { },
			TestName = "Recursive_Dir_Include_With_Multiple_Children_Exclude_Resulting_In_No_Output"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				IncludeFilters = { "**/*.exe" },
			},
			Input = new FilterFileSystemRecord[] {
				"Dir1/",
				"Dir1/Dir2/",
				"Dir1/Dir2/file1.exe",
			},
			ExpectedOutputDiff = new FilterFileSystemRecord[] { },
			TestName = "Single_Deep_File_Include"
		};

		yield return new FilterTestCaseData {
			Options = new() {
				FilterCaseInsensitive = true,
				FilterType = TextMatchProviderType.Glob,
				ExcludeFilters = { "**/*.exe" },
			},
			Input = new FilterFileSystemRecord[] {
				"Dir1/",
				"Dir1/Dir2/",
				"Dir1/Dir2/file1.exe",
			},
			ExpectedOutput = new FilterFileSystemRecord[] {
				"Dir1/",
				"Dir1/Dir2/",
			},
			TestName = "Single_Deep_File_Exclude"
		};
	}

	private List<FilterFileSystemRecord> Filter(IEnumerable<FilterFileSystemRecord> element, IFilterOptions options, ITextMatchProviderFactory textMatchProviderFactory)
	{
		Filter filter = new();
		if (textMatchProviderFactory != null)
			filter.TextMatchProviderFactory = textMatchProviderFactory;

		return filter.Apply(element, options, PathSeparator, r => r.Path, r => r.IsDirectory);
	}

	public class FilterTestCaseData
	{
		public FilterOptions Options { get; set; } = new FilterOptions();
		public FilterFileSystemRecord[] Input { get; set; }
		public FilterFileSystemRecord[] ExpectedOutput { get; set; }
		public FilterFileSystemRecord[] ExpectedOutputDiff { get; set; }
		public string TestName { get; set; }

		internal ITextMatchProviderFactory TextMatchProviderFactory { get; set; }

		public override string ToString()
		{
			string prefix = $"[{Options.FilterType}] [CaseInsensitive: {Options.FilterCaseInsensitive}]";

			return string.IsNullOrEmpty(TestName) ? $"{prefix} [Inc: {string.Join(',', Options.IncludeFilters)}] [Exc: {string.Join(',', Options.ExcludeFilters)}] [I:{string.Join(',', Input.Select(i => i.Path))} --> O:{string.Join(',', ExpectedOutput.Select(o => o.Path))}]" : $"{prefix} {TestName}";
		}
	}

	public class FilterFileSystemRecord
	{
		public string Path { get; set; }
		public bool IsDirectory { get; set; }

		public FilterFileSystemRecord(string path, bool isDirectory = false)
		{
			IsDirectory = isDirectory || path.EndsWith(@"\") || path.EndsWith(@"/");
			Path = path.TrimEnd('/', '\\');
		}

		public static implicit operator FilterFileSystemRecord(string text) => new FilterFileSystemRecord(text);

		public override string ToString()
		{
			return $"{Path} [IsDirectory: {IsDirectory}]";
		}
	}

	public class FilterFileSystemRecordEqualityComparer : IEqualityComparer<FilterFileSystemRecord>
	{
		public bool Equals(FilterFileSystemRecord x, FilterFileSystemRecord y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (ReferenceEquals(x, null)) return false;
			if (ReferenceEquals(y, null)) return false;
			if (x.GetType() != y.GetType()) return false;
			return x.Path == y.Path && x.IsDirectory == y.IsDirectory;
		}

		public int GetHashCode(FilterFileSystemRecord obj)
		{
			return HashCode.Combine(obj.Path, obj.IsDirectory);
		}
	}
}