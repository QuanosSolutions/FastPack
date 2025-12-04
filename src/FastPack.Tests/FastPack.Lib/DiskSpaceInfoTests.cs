using System.Reflection;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Logging;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib;

[TestFixture]

public class DiskSpaceInfoTests
{
	[Test]
	public async Task Ensure_GetAvailableSpaceForPathInBytes_Works()
	{
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		long? availableSpaceForPathInBytes = await DiskSpaceInfo.GetAvailableSpaceForPathInBytes(Assembly.GetExecutingAssembly().Location, loggerMock.Object);
		availableSpaceForPathInBytes.Should().NotBeNull();
		availableSpaceForPathInBytes.Should().BeGreaterThan(1000);
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_GetAvailableSpaceForPathInBytes_Returns_Null_For_Null_Works()
	{
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		long? availableSpaceForPathInBytes = await DiskSpaceInfo.GetAvailableSpaceForPathInBytes(null, loggerMock.Object);
		availableSpaceForPathInBytes.Should().BeNull();
		loggerMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_GetAvailableSpaceForPathInBytes_Returns_Null_For_Not_Existing_Path_Works()
	{
		Mock<ILogger> loggerMock = new Mock<ILogger>();
		string path = @"\\notexisting\whtrth45h45h45h4w5h";
		long? availableSpaceForPathInBytes = await DiskSpaceInfo.GetAvailableSpaceForPathInBytes(path, loggerMock.Object);
		availableSpaceForPathInBytes.Should().BeNull();
		loggerMock.VerifyNoOtherCalls();
	}
}