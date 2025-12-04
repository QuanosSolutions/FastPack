using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Logging;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib;

[TestFixture]
internal class MemoryInfoTests
{
	[Test]
	public async Task Ensure_GetAvailableMemoryInBytes_Works()
	{
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		long availableMemory = await MemoryInfo.GetAvailableMemoryInBytes(loggerMock.Object);
		availableMemory.Should().BeGreaterThan(1000);
		loggerMock.VerifyNoOtherCalls();
	}
}