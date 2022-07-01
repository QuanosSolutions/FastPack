﻿using System.Threading.Tasks;
using FastPack.CmdLine;
using FastPack.TestFramework.Common;
using FluentAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.CmdLine
{
	[TestFixture]
	public class TextFileToCommandLineConverterTests
	{
		[Test]
		public async Task Ensure_Deserialize_Works()
		{
			// arrange
			using FastPackTestContext testContext = FastPackTestContext.Create(true);
			string filePath = testContext.GetTempFileName(".anyextension", true);
			await TestData.WriteToFile("FastPack.CmdLine/FileParams.txt", filePath, GetType().Assembly);

			string[] expectedArgs = {"-i", @"C:\input", "-o", @"C:\out.fup"};

			// act
			string[] convertedArgs = await new TextFileToCommandLineConverter().ConvertToArgs(filePath);

			// assert
			convertedArgs.Should().Equal(expectedArgs);
		}
	}
}