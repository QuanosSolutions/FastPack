using System;
using System.IO;
using System.Threading.Tasks;
using FastPack.Lib;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib;

[TestFixture]
internal class SubStreamTests
{
	[Test]
	public async Task Ensure_SubStream_Works()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);
		await using MemoryStream parentMemoryStream = new MemoryStream();
		parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
		parentMemoryStream.Position = 0;

		// act
		await using Stream subStream = new SubStream(parentMemoryStream, 10, 4);
		MemoryStream memoryStream = new((int)subStream.Length);
		await subStream.CopyToAsync(memoryStream);

		// assert
		System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length).Should().Be("test");
	}

	[Test]
	public async Task Ensure_Position_Get_Works()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);
		await using MemoryStream parentMemoryStream = new MemoryStream();
		parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
		parentMemoryStream.Position = 0;
		await using Stream subStream = new SubStream(parentMemoryStream, 10, 4);

		// act

		// assert
		subStream.Position.Should().Be(0);
	}

	[Test]
	public async Task Ensure_CanRead_Is_True()
	{
		// arrange
		await using MemoryStream parentMemoryStream = new MemoryStream();
		await using Stream subStream = new SubStream(parentMemoryStream, 10, 4);

		// act

		// assert
		subStream.CanRead.Should().Be(true);
	}

	[Test]
	public async Task Ensure_CanSeek_Is_False()
	{
		// arrange
		await using MemoryStream parentMemoryStream = new MemoryStream();
		await using Stream subStream = new SubStream(parentMemoryStream, 10, 4);

		// act

		// assert
		subStream.CanSeek.Should().Be(false);
	}

	[Test]
	public async Task Ensure_CanWrite_Is_False()
	{
		// arrange
		await using MemoryStream parentMemoryStream = new MemoryStream();
		await using Stream subStream = new SubStream(parentMemoryStream, 10, 4);

		// act

		// assert
		subStream.CanWrite.Should().Be(false);
	}

	[Test]
	public void Ensure_Position_Set_Throws_Exception()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);

		// act
		Action act = () =>
		{
			using MemoryStream parentMemoryStream = new MemoryStream();
			parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
			parentMemoryStream.Position = 0;
			using Stream subStream = new SubStream(parentMemoryStream, 10, 4);
			subStream.Position = 1;
		};

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("Seeking is not supported!");
	}

	[Test]
	public void Ensure_Seek_Throws_Exception()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);

		// act
		Action act = () =>
		{
			using MemoryStream parentMemoryStream = new MemoryStream();
			parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
			parentMemoryStream.Position = 0;
			using Stream subStream = new SubStream(parentMemoryStream, 10, 4);
			subStream.Seek(1, SeekOrigin.Begin);
		};

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("Seeking is not supported.");
	}

	[Test]
	public void Ensure_SetLength_Throws_Exception()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);

		// act
		Action act = () =>
		{
			using MemoryStream parentMemoryStream = new MemoryStream();
			parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
			parentMemoryStream.Position = 0;
			using Stream subStream = new SubStream(parentMemoryStream, 10, 4);
			subStream.SetLength(1);
		};

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("SetLength requires seeking and writing, which is not supported.");
	}

	[Test]
	public void Ensure_Write_Throws_Exception()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);

		// act
		Action act = () =>
		{
			using MemoryStream parentMemoryStream = new MemoryStream();
			parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
			parentMemoryStream.Position = 0;
			using Stream subStream = new SubStream(parentMemoryStream, 10, 4);
			subStream.Write(null, 1, 1);
		};

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("Writing is not supported.");
	}

	[Test]
	public void Ensure_Flush_Throws_Exception()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);

		// act
		Action act = () =>
		{
			using MemoryStream parentMemoryStream = new MemoryStream();
			parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
			parentMemoryStream.Position = 0;
			using Stream subStream = new SubStream(parentMemoryStream, 10, 4);
			subStream.Flush();
		};

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("Writing is not supported.");
	}

	[Test]
	public void Ensure_Disposed_Parent_Stream_Throws_Exception()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);

		MemoryStream parentMemoryStream;
		using (parentMemoryStream = new MemoryStream())
		{
			parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
			parentMemoryStream.Position = 0;
		}

		// act
		Action act = () =>
		{
			using Stream subStream = new SubStream(parentMemoryStream, 10, 4);
			byte[] buffer = new byte[4];
			int read = subStream.Read(buffer, 0, 4);
		};

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("Can not read!");
	}

	[Test]
	public void Ensure_Disposed_Stream_Throws_Exception()
	{
		// arrange
		string testString = "This is a test.";
		byte[] stringAsBytes = System.Text.Encoding.UTF8.GetBytes(testString);

		MemoryStream parentMemoryStream = new MemoryStream();
		parentMemoryStream.Write(stringAsBytes, 0, stringAsBytes.Length);
		parentMemoryStream.Position = 0;

		// act
		Action act = () =>
		{
			Stream subStream;
			using (subStream = new SubStream(parentMemoryStream, 10, 4))
			{
				// do nothing for now
			}
			byte[] buffer = new byte[4];
			int read = subStream.Read(buffer, 0, 4);
		};

		// assert
		act.Should().Throw<ObjectDisposedException>().WithMessage("Cannot access a disposed object.\r\nObject name: 'SubStream'.");
	}
}