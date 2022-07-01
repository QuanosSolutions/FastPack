using System;
using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.Logging;

namespace FastPack.Lib.Unpackers;

public class ArchiveUnpackerFactory : IArchiveUnpackerFactory
{
	public async Task<IUnpacker> GetUnpacker(string filePath, ILogger logger)
	{
		if (filePath == null)
			throw new ArgumentNullException(nameof(filePath));

		await using Stream fupStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BufferSize, Constants.OpenFileStreamsAsync);
		return await GetUnpacker(fupStream, logger);
	}

	public async Task<IUnpacker> GetUnpacker(Stream stream, ILogger logger)
	{
		byte[] signatureBuffer = new byte[Constants.FastpackFileDataIndex];

		if (await stream.ReadAsync(signatureBuffer, 0, signatureBuffer.Length) != signatureBuffer.Length)
			throw new NotSupportedException($"Stream does not contain valid FastPack data.");
			
		stream.Seek(0, SeekOrigin.Begin);

		if (IsSignatureMatch(signatureBuffer))
			return GetUnpacker(BitConverter.ToUInt16(signatureBuffer, Constants.FastpackSignatureSize), logger);

		throw new NotSupportedException($"Stream does not contain valid FastPack data.");
	}

	public IUnpacker GetUnpacker(ushort version, ILogger logger)
	{
		return version switch {
			1 => new ArchiveUnpackerV1(logger),
			_ => throw new NotSupportedException($"There is no unpacker for version {version}.")
		};
	}
	private static bool IsSignatureMatch(Span<byte> signature)
	{
		return BitConverter.ToUInt64(signature).Equals(Constants.FastpackSignature);
	}
}