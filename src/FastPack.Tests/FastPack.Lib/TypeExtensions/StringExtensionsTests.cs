using FastPack.Lib.TypeExtensions;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.TypeExtensions;

[TestFixture]
public class StringExtensionsTests
{
	[Test]
	public void Ensure_GetUtf8ByteArray_Works()
	{
		// arrange
		string test = "test";

		// act
		byte[] utf8ByteArray = test.GetUtf8ByteArray();

		// assert
		utf8ByteArray.Length.Should().Be(4);
		utf8ByteArray[0].Should().Be(0x74);
		utf8ByteArray[1].Should().Be(0x65);
		utf8ByteArray[2].Should().Be(0x73);
		utf8ByteArray[3].Should().Be(0x74);
	}

	[Test]
	public void Ensure_GetUtf8ByteArray_EmptyString_Works()
	{
		// arrange
		string test = string.Empty;

		// act
		byte[] utf8ByteArray = test.GetUtf8ByteArray();

		// assert
		utf8ByteArray.Length.Should().Be(0);
	}

	[Test]
	public void Ensure_GetUtf8ByteArray_NullString_Works()
	{
		// arrange
		string test = null;

		// act
		byte[] utf8ByteArray = test.GetUtf8ByteArray();

		// assert
		utf8ByteArray.Length.Should().Be(0);
	}
}