using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastPack.Lib.ManifestManagement;
using FastPack.Lib.Options;

namespace FastPack.Lib.Unpackers;

public abstract class Unpacker : IUnpacker
{
	internal class EntryWithGroup
	{
		public ManifestEntry Group { get; set; }
		public ManifestFileSystemEntry FileSystemEntry { get; set; }
	}

	internal IFilter Filter { get; set; } = new Filter();

	protected void FilterEntries(List<ManifestEntry> entries, UnpackOptions options)
	{
		if (!options.IncludeFilters.Any() && !options.ExcludeFilters.Any())
			return;

		IEnumerable<EntryWithGroup> entriesWithGroups = entries.SelectMany(e => e.FileSystemEntries, (group, entry) => new EntryWithGroup { Group = group, FileSystemEntry = entry });
		List<EntryWithGroup> remainingEntriesWithGroups = Filter.Apply(entriesWithGroups, options, Constants.FastpackManifestDirectorySeparatorChar, e => e.FileSystemEntry.RelativePath, e => e.Group.Type == EntryType.Directory);

		const string directoryGroupHash = "Directories";
		Dictionary<string, HashSet<string>> remainingHashesToPathsMapping = remainingEntriesWithGroups.GroupBy(e => e.Group.Hash).ToDictionary(g => g.Key ?? directoryGroupHash, g => new HashSet<string>(g.Select(g => g.FileSystemEntry.RelativePath)));

		for (int i = entries.Count - 1; i >= 0; i--)
		{
			ManifestEntry manifestEntry = entries[i];
				
			if (!remainingHashesToPathsMapping.TryGetValue(manifestEntry.Hash ?? directoryGroupHash, out HashSet<string> paths))
			{
				entries.RemoveAt(i);
				continue;
			}

			for (int j = manifestEntry.FileSystemEntries.Count-1; j >= 0; j--)
			{
				string relativePath = manifestEntry.FileSystemEntries[j].RelativePath;

				if (!paths.Contains(relativePath))
					manifestEntry.FileSystemEntries.RemoveAt(j);
			}
		}
	}

	public abstract Task<Manifest> GetManifestFromStream(Stream inputStream);

	public abstract Task<int> Extract(string inputFile, UnpackOptions options);
}