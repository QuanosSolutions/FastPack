using System.IO;

namespace FastPack.Lib.TypeExtensions;

internal static class BinaryWriterExtensions
{
	public static void WriteUtf8String(this BinaryWriter binaryWriter, string value)
	{
		byte[] valueBytes = value.GetUtf8ByteArray();
		binaryWriter.Write(valueBytes.Length);
		binaryWriter.Write(valueBytes);
	}
}