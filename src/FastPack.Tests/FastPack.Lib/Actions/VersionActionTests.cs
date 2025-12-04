using System;
using System.Threading.Tasks;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Actions
{
	[TestFixture]
	public class VersionActionTests
	{
		[Test]
		public void Constructor_Throws_On_Null_Logger()
		{
			Action constructionAction = () => new VersionAction(null);
			constructionAction.Should().Throw<ArgumentException>().Where(e => e.ParamName.Equals("logger", StringComparison.InvariantCulture));
		}

		[Test]
		public async Task Test_Run()
		{
			// arrange
			Mock<ILogger> loggerMock = new();
			
			// act
			int exitCode = await new VersionAction(loggerMock.Object).Run();

			// assert
			exitCode.Should().Be(0);
			
			// verify function calls
			loggerMock.Verify(l => l.InfoLine(It.IsRegex(@"\d+\.\d+\.\d+")), Times.Once);
		}
	}
}
