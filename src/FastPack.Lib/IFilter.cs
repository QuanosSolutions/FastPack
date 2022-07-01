using System;
using System.Collections.Generic;
using FastPack.Lib.Options;

namespace FastPack.Lib;

internal interface IFilter
{
	List<T> Apply<T>(IEnumerable<T> elements, IFilterOptions options, char pathSeparator, Func<T, string> getRelativePathFunc, Func<T, bool> isDirectoryFunc);
}