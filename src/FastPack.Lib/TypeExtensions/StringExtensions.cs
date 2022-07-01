using System;
using System.Text;

namespace FastPack.Lib.TypeExtensions;

internal static class StringExtensions
{
	public static byte[] GetUtf8ByteArray(this string value)
	{
		return string.IsNullOrEmpty(value) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(value);
	}
}