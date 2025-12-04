using FastPack.Lib.TypeExtensions;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.TypeExtensions;

[TestFixture]
public class LongExtensionsTests
{
	[Test]
	public void Ensure_GetBytesReadable_With_0_Works()
	{
		// arrange
		long size = 0;

		// act
		string bytesReadable = size.GetBytesReadable();

		// assert
		bytesReadable.Should().Be("0 B");
	}

	[Test]
	public void Ensure_GetBytesReadable_Byte_Works()
	{
		// arrange
		long size = 100;

		// act
		string bytesReadable = size.GetBytesReadable();

		// assert
		bytesReadable.Should().Be("100 B");
	}

	[Test]
	public void Ensure_GetBytesReadable_KiloByte_Works()
	{
		// arrange
		long size = 1234;

		// act
		string bytesReadable = size.GetBytesReadable();

		// assert
		bytesReadable.Should().Be("1.205 KB");
	}

	[Test]
	public void Ensure_GetBytesReadable_MegaByte_Works()
	{
		// arrange
		long size = 12345678;

		// act
		string bytesReadable = size.GetBytesReadable();

		// assert
		bytesReadable.Should().Be("11.773 MB");
	}

	[Test]
	public void Ensure_GetBytesReadable_GigaByte_Works()
	{
		// arrange
		long size = 123456789123;

		// act
		string bytesReadable = size.GetBytesReadable();

		// assert
		bytesReadable.Should().Be("114.978 GB");
	}

	[Test]
	public void Ensure_GetBytesReadable_TeraByte_Works()
	{
		// arrange
		long size = 123456789123456;

		// act
		string bytesReadable = size.GetBytesReadable();

		// assert
		bytesReadable.Should().Be("112.283 TB");
	}

	[Test]
	public void Ensure_GetBytesReadable_PetaByte_Works()
	{
		// arrange
		long size = 12345678912345678;

		// act
		string bytesReadable = size.GetBytesReadable();

		// assert
		bytesReadable.Should().Be("10.965 PB");
	}

	[Test]
	public void Ensure_GetBytesReadable_ExaByte_Works()
	{
		// arrange
		long size = 1234567891234567891;

		// act
		string bytesReadable = size.GetBytesReadable();

		// assert
		bytesReadable.Should().Be("1.07 EB");
	}
}