using System.Collections.Generic;
using FastPack.Lib.Matching;

namespace FastPack.Lib.Options
{
	internal interface IFilterOptions
	{
		public List<string> IncludeFilters { get; }
		public List<string> ExcludeFilters { get; }
		public TextMatchProviderType FilterType { get; }
		public bool FilterCaseInsensitive { get; set; }
	}
}