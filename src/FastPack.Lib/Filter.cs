using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastPack.Lib.Matching;
using FastPack.Lib.Options;

namespace FastPack.Lib;

internal class Filter : IFilter
{
	private class ElementState<T>
	{
		public T Element { get; init; }
		public bool RemoveIfChildrenMissing { get; init; }
	}

	internal ITextMatchProviderFactory TextMatchProviderFactory { get; set; } = new TextMatchProviderFactory();

	public virtual List<T> Apply<T>(IEnumerable<T> elements, IFilterOptions options, char pathSeparator, Func<T, string> getRelativePathFunc, Func<T, bool> isDirectoryFunc)
	{
		if (!options.IncludeFilters.Any() && !options.ExcludeFilters.Any())
			return elements.ToList();

		ITextMatchProvider textMatchProvider = TextMatchProviderFactory.GetProvider(options.FilterType);
		List<string> includeFilters = textMatchProvider.NormalizePathFilters(options.IncludeFilters, pathSeparator).ToList();
		List<string> excludeFilters = textMatchProvider.NormalizePathFilters(options.ExcludeFilters, pathSeparator).ToList();
		
		List<ElementState<T>> includedElementsSortedByRelativePath = ProcessIncludes(elements, options, pathSeparator, getRelativePathFunc, isDirectoryFunc, includeFilters, textMatchProvider);
		ProcessExcludes(options, pathSeparator, getRelativePathFunc, isDirectoryFunc, excludeFilters, includedElementsSortedByRelativePath, textMatchProvider);
		RemoveOrphanedDirectories(pathSeparator, getRelativePathFunc, includedElementsSortedByRelativePath);

		return includedElementsSortedByRelativePath.ConvertAll(e => e.Element);
	}

	private static List<ElementState<T>> ProcessIncludes<T>(IEnumerable<T> elements, IFilterOptions options, char pathSeparator, Func<T, string> getRelativePathFunc, Func<T, bool> isDirectoryFunc, List<string> includeFilters, ITextMatchProvider textMatchProvider)
	{
		List<T> elementsSortedByRelativePath = elements.OrderBy(getRelativePathFunc).ToList();

		if (!includeFilters.Any())
			return elementsSortedByRelativePath.ConvertAll(e => new ElementState<T> {Element = e});

		Dictionary<string, T> pathToElementMap = new();
		Dictionary<string, ElementState<T>> filteredElementStates = new();

		for (int i = 0; i <= elementsSortedByRelativePath.Count - 1; i++)
		{
			T element = elementsSortedByRelativePath[i];
			string relativePath = getRelativePathFunc(element);
			pathToElementMap[relativePath] = element;

			bool included = includeFilters.Any(f => textMatchProvider.IsMatch(relativePath, f, !options.FilterCaseInsensitive));

			if (!included)
				continue;

			filteredElementStates[relativePath] = new ElementState<T> {Element = element};

			if (isDirectoryFunc(element))
			{
				string directoryWithSeparator = relativePath + pathSeparator;

				// find all children and add them
				for (int c = i + 1; c <= elementsSortedByRelativePath.Count - 1; c++)
				{
					T childElement = elementsSortedByRelativePath[c];

					// we check if the child entry starts with the directory including the separator, if so we include the directory
					string childRelativePath = getRelativePathFunc(childElement);
					if (!childRelativePath.StartsWith(directoryWithSeparator, StringComparison.InvariantCulture))
						break;

					i = c;
					filteredElementStates[childRelativePath] = new ElementState<T> {Element = childElement};
				}
			}
			else
			{
				// check if include filter applies to any parent folder, if so add the parent directory to the filtered elements

				StringBuilder includePathBuilder = new();
				foreach (string segment in relativePath.Split(pathSeparator)[..^1])
				{
					if (includePathBuilder.Length > 0)
						includePathBuilder.Append(pathSeparator);

					includePathBuilder.Append(segment);

					string segmentPath = includePathBuilder.ToString();

					if (!filteredElementStates.ContainsKey(segmentPath))
						filteredElementStates[segmentPath] = new ElementState<T> {Element = pathToElementMap[segmentPath], RemoveIfChildrenMissing = true};
				}
			}
		}

		return filteredElementStates.OrderBy(e => e.Key).Select(kv => kv.Value).ToList();
	}

	private static void ProcessExcludes<T>(IFilterOptions options, char pathSeparator, Func<T, string> getRelativePathFunc,
		Func<T, bool> isDirectoryFunc, List<string> excludeFilters, List<ElementState<T>> includedElementsSortedByRelativePath,
		ITextMatchProvider textMatchProvider)
	{
		if (!excludeFilters.Any())
			return;

		for (int i = 0; i <= includedElementsSortedByRelativePath.Count - 1; i++)
		{
			T element = includedElementsSortedByRelativePath[i].Element;
			string relativePath = getRelativePathFunc(element);

			bool excluded = excludeFilters.Any(f => textMatchProvider.IsMatch(relativePath, f, !options.FilterCaseInsensitive));

			if (!excluded)
				continue;

			includedElementsSortedByRelativePath.RemoveAt(i);
			i--;

			if (!isDirectoryFunc(element))
				continue;

			string directoryWithSeparator = relativePath + pathSeparator;

			// find all children and remove them
			for (int c = i + 1; c <= includedElementsSortedByRelativePath.Count - 1; c++)
			{
				T childElement = includedElementsSortedByRelativePath[c].Element;

				// we check if the child entry starts with the directory including the separator, if so we include the directory
				string childRelativePath = getRelativePathFunc(childElement);
				if (!childRelativePath.StartsWith(directoryWithSeparator, StringComparison.InvariantCulture))
					break;

				includedElementsSortedByRelativePath.RemoveAt(c);
				c--;
			}
		}
	}

	private static void RemoveOrphanedDirectories<T>(char pathSeparator, Func<T, string> getRelativePathFunc, List<ElementState<T>> includedElementsSortedByRelativePath)
	{
		// handle directories that were included only because of their children. If no children are left, they have to be removed
		for (int i = includedElementsSortedByRelativePath.Count - 1; i >= 0; i--)
		{
			ElementState<T> state = includedElementsSortedByRelativePath[i];
			if (!state.RemoveIfChildrenMissing)
				continue;

			string directoryWithSeparator = getRelativePathFunc(state.Element) + pathSeparator;

			// we check if the next entry starts with the directory including the separator, if so we include the directory
			if (i + 1 < includedElementsSortedByRelativePath.Count && getRelativePathFunc(includedElementsSortedByRelativePath[i + 1].Element).StartsWith(directoryWithSeparator, StringComparison.InvariantCulture))
				continue;

			includedElementsSortedByRelativePath.RemoveAt(i);
		}
	}
}