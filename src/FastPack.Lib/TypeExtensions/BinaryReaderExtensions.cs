using System.IO;
using System.Text;

namespace FastPack.Lib.TypeExtensions;

internal static class BinaryReaderExtensions
{
	public static string ReadUtf8String(this BinaryReader binaryReader)
	{
		int length = binaryReader.ReadInt32();
		if (length == 0)
			return null;
		byte[] stringBytes = binaryReader.ReadBytes(length);
		if (stringBytes.Length != length)
			throw new InvalidDataException($"Tried to read {length} string bytes, but got {stringBytes.Length} bytes.");
		return Encoding.UTF8.GetString(stringBytes);
	}
}