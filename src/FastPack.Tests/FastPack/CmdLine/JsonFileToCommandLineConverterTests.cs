using System.Threading.Tasks;
using FastPack.CmdLine;
using FastPack.TestFramework.Common;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.CmdLine
{
	[TestFixture]
	public class JsonFileToCommandLineConverterTests
	{
		[Test]
		public async Task Ensure_Deserialize_Works()
		{
			// arrange
			using FastPackTestContext testContext = FastPackTestContext.Create(true);
			string filePath = testContext.GetTempFileName(".json", true);
			await TestData.WriteToFile("FastPack.CmdLine/FileParams.json", filePath, GetType().Assembly);

			string[] expectedArgs = {"-i", @"C:\input", "-o", @"C:\out.fup"};

			// act
			string[] convertedArgs = await new JsonFileToCommandLineConverter().ConvertToArgs(filePath);

			// assert
			convertedArgs.Should().Equal(expectedArgs);
		}
	}
}