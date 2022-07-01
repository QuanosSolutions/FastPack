using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FastPack.Lib.Matching;

namespace FastPack.Lib.Options;

[ExcludeFromCodeCoverage]
public class FilterOptions : IFilterOptions
{
	public List<string> IncludeFilters { get; } = new();
	public List<string> ExcludeFilters { get; } = new();
	public TextMatchProviderType FilterType { get; set; } = TextMatchProviderType.Glob;
	public bool FilterCaseInsensitive { get; set; }
}