using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FastPack.Lib.ManifestManagement.Serialization;

public class BinaryArchiveFileWriterV1 : IArchiveFileWriter
{
	private IArchiveManifestWriter BinaryManifestWriter { get; }

	public BinaryArchiveFileWriterV1(IArchiveManifestWriter binaryManifestWriter)
	{
		BinaryManifestWriter = binaryManifestWriter;
	}

	public async Task WriteHeader(Stream outputStream, Manifest manifest)
	{
		await using BinaryWriter writer = new(outputStream, Encoding.UTF8, true);
		writer.Write(Constants.FastpackSignature);
		writer.Write(manifest.Version);
		writer.Write(0L); // Temporary until the index of the manifest is known
	}

	public async Task WriteFileData(Stream outputStream, ManifestEntry manifestEntry, Func<Stream, Task> writeStreamAction)
	{
		manifestEntry.DataIndex = outputStream.Position;
		await writeStreamAction(outputStream);
		manifestEntry.DataSize = outputStream.Position - manifestEntry.DataIndex;
	}

	public async Task WriteManifest(Stream outputStream, Manifest manifest)
	{
		await BinaryManifestWriter.WriteManifest(outputStream, manifest);
	}
}