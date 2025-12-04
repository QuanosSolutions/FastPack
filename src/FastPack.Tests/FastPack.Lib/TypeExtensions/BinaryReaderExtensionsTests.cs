using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FastPack.Lib.TypeExtensions;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.TypeExtensions;

[TestFixture]
public class BinaryReaderExtensionsTests
{
	[Test]
	public async Task Ensure_ReadUtf8String_Works()
	{
		// arrange
		string testValue = " - this is a test - ";
		byte[] valueBytes = Encoding.UTF8.GetBytes(testValue);
		await using MemoryStream memoryStream = new MemoryStream();
		await using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true))
		{
			binaryWriter.Write(valueBytes.Length);
			binaryWriter.Write(valueBytes);
		}
		memoryStream.Position = 0;
		using BinaryReader binaryReader = new BinaryReader(memoryStream);

		// act
		string readStream = binaryReader.ReadUtf8String();

		// assert
		readStream.Should().Be(testValue);
	}

	[Test]
	public async Task Ensure_ReadUtf8String_With_Null_Works()
	{
		// arrange
		await using MemoryStream memoryStream = new MemoryStream();
		await using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true))
		{
			binaryWriter.Write(0);
		}
		memoryStream.Position = 0;
		using BinaryReader binaryReader = new BinaryReader(memoryStream);

		// act
		string readStream = binaryReader.ReadUtf8String();

		// assert
		readStream.Should().BeNull();
	}

	[Test]
	public async Task Ensure_ReadUtf8String_Detects_Invalid_Data()
	{
		// arrange
		string testValue = " - this is a test - ";
		byte[] valueBytes = Encoding.UTF8.GetBytes(testValue);
		await using MemoryStream memoryStream = new MemoryStream();
		await using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true))
		{
			binaryWriter.Write(valueBytes.Length + 10);
			binaryWriter.Write(valueBytes);
		}
		memoryStream.Position = 0;
		using BinaryReader binaryReader = new BinaryReader(memoryStream);

		// act
		Action act = () => binaryReader.ReadUtf8String();

		// assert
		act.Should().Throw<InvalidDataException>().WithMessage("Tried to read 30 string bytes, but got 20 bytes.");
	}
}