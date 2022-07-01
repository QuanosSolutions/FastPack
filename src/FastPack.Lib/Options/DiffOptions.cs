using System;
using System.Diagnostics.CodeAnalysis;

namespace FastPack.Lib.Options;

[ExcludeFromCodeCoverage]
public class DiffOptions : FilterOptions, IOptions
{
	[Flags]
	public enum DiffSetting
	{
		StructureOnly = 0,
		Size = 1,
		Date = 2,
		Permission = 4,
	}

	public string FirstFilePath { get; set; }
	public string SecondFilePath { get; set; }
	public DiffSetting DiffSettings { get; set; }
	public bool ExtractionEnabled { get; set; }
	public string ExtractionDirectoryPath { get; set; }
	public OutputFormat OutputFormat { get; set; }
	public bool PrettyPrint { get; set; }
	public bool QuietMode { get; set; }
}