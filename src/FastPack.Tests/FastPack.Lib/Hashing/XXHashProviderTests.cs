using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.Hashing;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Hashing;

[TestFixture]
public class XXHashProviderTests
{
	[Test]
	public void Ensure_GetHashSizeInBytes_Works()
	{
		// arrange
		IHashProvider hashProvider = new XXHashProvider();

		// act
		int hashSizeInBytes = hashProvider.GetHashSizeInBytes();

		// assert
		hashSizeInBytes.Should().Be(8);
	}

	[Test]
	public void Ensure_HashStringToBytes_Works()
	{
		// arrange
		IHashProvider hashProvider = new XXHashProvider();
		string test = "12345678";

		// act
		byte[] bytes = hashProvider.HashStringToBytes(test);

		// assert
		bytes.Length.Should().Be(8);
		bytes[0].Should().Be(0x4E);
		bytes[1].Should().Be(0x61);
		bytes[2].Should().Be(0xBC);
		bytes[3].Should().Be(0x00);
		bytes[4].Should().Be(0x00);
		bytes[5].Should().Be(0x00);
		bytes[6].Should().Be(0x00);
		bytes[7].Should().Be(0x00);
	}

	[Test]
	public void Ensure_HashBytesToString_Works()
	{
		// arrange
		IHashProvider hashProvider = new XXHashProvider();
		byte[] bytes = {
			0x4E,
			0x61,
			0xBC,
			0x00,
			0x00,
			0x00,
			0x00,
			0x00,
		};

		// act
		string value = hashProvider.HashBytesToString(bytes);

		// assert
		value.Should().Be("12345678");
	}

	[Test]
	public async Task Ensure_CalculateHash_Works()
	{
		// arrange
		IHashProvider hashProvider = new XXHashProvider();
		byte[] someData = System.Text.Encoding.UTF8.GetBytes("testData");
		await using MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(someData, 0, someData.Length);
		memoryStream.Position = 0;

		// act
		string value = await hashProvider.CalculateHash(memoryStream);

		// assert
		value.Should().Be("11372068393339243384");
	}
}