using System.IO;

namespace FastPack.Lib.ManifestManagement;

internal class ManifestEntryWithCompressedStream : ManifestEntry
{
	public Stream CompressedStream { get; set; }
}