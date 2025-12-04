using System.IO;
using System.Text;
using System.Threading.Tasks;
using FastPack.Lib.TypeExtensions;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.TypeExtensions;

[TestFixture]
public class BinaryWriterExtensionsTests
{
	[Test]
	public async Task Ensure_WriteUtf8String_Works()
	{
		// arrange
		string testValue = " - this is a test - ";

		await using MemoryStream memoryStream = new MemoryStream();

		// act
		await using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true))
		{
			binaryWriter.WriteUtf8String(testValue);
		}

		memoryStream.Position = 0;
		using BinaryReader binaryReader = new BinaryReader(memoryStream);
		int length = binaryReader.ReadInt32();
		byte[] stringBytes = binaryReader.ReadBytes(length);
		string readStream = Encoding.UTF8.GetString(stringBytes);


		// assert
		readStream.Should().Be(testValue);
	}
}