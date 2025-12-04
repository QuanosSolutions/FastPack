using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Actions
{
	[TestFixture]
	public class AboutActionTests
	{
		const int ConsoleWidth = 200;

		[Test]
		public async Task Test_With_ShowOwnLicenseText_UpperCase()
		{
			// arrange
			const string aboutText = "About FastPack";
			ConsoleKeyInfo userPressedCode = new('L', ConsoleKey.L, true, false, false);

			Mock<IConsoleAbstraction> consoleAbstractionMock = new();
			consoleAbstractionMock.Setup(c => c.GetWindowWidth()).Returns(ConsoleWidth);
			consoleAbstractionMock.Setup(c => c.ReadKey(true)).Returns(userPressedCode);

			
			List<string> outputList = new();
			Mock<ILogger> loggerMock = new();
			loggerMock.Setup(l => l.InfoLine(It.IsAny<string>())).Callback<string>(l => outputList.Add(l));
			
			Mock<AboutAction> aboutActionMock = new(loggerMock.Object) {
				Object = {
					Console = consoleAbstractionMock.Object
				}
			};
			aboutActionMock.Setup(a => a.GetAboutText()).CallBase();
			aboutActionMock.Setup(a => a.ShowOwnLicenseText()).CallBase();

			// act
			int exitCode = await aboutActionMock.Object.Run();

			// assert
			exitCode.Should().Be(0);

			// first line is the about text
			outputList[0].Length.Should().Be((ConsoleWidth + aboutText.Length)/2);
			// second line is fully filled with equal-signs
			outputList[1].Length.Should().Be(ConsoleWidth);

			// verify function calls
			aboutActionMock.Verify(a => a.GetAboutText(), Times.Once);
			aboutActionMock.Verify(a => a.ShowOwnLicenseText(), Times.Once);
			aboutActionMock.VerifyNoOtherCalls();
		}

		[Test]
		public async Task Test_With_ShowOwnLicenseText_LowerCase()
		{
			// arrange
			ConsoleKeyInfo userPressedCode = new('l', ConsoleKey.L, false, false, false);

			Mock<IConsoleAbstraction> consoleAbstractionMock = new();
			consoleAbstractionMock.Setup(c => c.GetWindowWidth()).Returns(ConsoleWidth);
			consoleAbstractionMock.Setup(c => c.ReadKey(true)).Returns(userPressedCode);
			
			Mock<ILogger> loggerMock = new();

			Mock<AboutAction> aboutActionMock = new(loggerMock.Object) {
				Object = {
					Console = consoleAbstractionMock.Object
				}
			};
			aboutActionMock.Setup(a => a.GetAboutText()).CallBase();
			aboutActionMock.Setup(a => a.ShowOwnLicenseText()).CallBase();

			// act
			int exitCode = await aboutActionMock.Object.Run();

			// assert
			exitCode.Should().Be(0);

			// verify function calls
			aboutActionMock.Verify(a => a.GetAboutText(), Times.Once);
			aboutActionMock.Verify(a => a.ShowOwnLicenseText(), Times.Once);
			aboutActionMock.VerifyNoOtherCalls();
		}

		[Test]
		public async Task Test_With_Show3rdPartyLicenseTexts_ConsoleKey_D3()
		{
			// arrange
			ConsoleKeyInfo userPressedCode = new('3', ConsoleKey.D3, false, false, false);

			Mock<IConsoleAbstraction> consoleAbstractionMock = new();
			consoleAbstractionMock.Setup(c => c.GetWindowWidth()).Returns(ConsoleWidth);
			consoleAbstractionMock.Setup(c => c.ReadKey(true)).Returns(userPressedCode);
			
			Mock<ILogger> loggerMock = new();

			Mock<AboutAction> aboutActionMock = new(loggerMock.Object) {
				Object = {
					Console = consoleAbstractionMock.Object
				}
			};
			aboutActionMock.Setup(a => a.GetAboutText()).CallBase();
			aboutActionMock.Setup(a => a.Show3rdPartyLicenseTexts()).CallBase();

			// act
			int exitCode = await aboutActionMock.Object.Run();

			// assert
			exitCode.Should().Be(0);
			
			// verify function calls
			aboutActionMock.Verify(a => a.GetAboutText(), Times.Once);
			aboutActionMock.Verify(a => a.Show3rdPartyLicenseTexts(), Times.Once);
			aboutActionMock.VerifyNoOtherCalls();
		}

		[Test]
		public async Task Test_With_Show3rdPartyLicenseTexts_ConsoleKey_NumPad3()
		{
			// arrange
			ConsoleKeyInfo userPressedCode = new('3', ConsoleKey.NumPad3, false, false, false);

			Mock<IConsoleAbstraction> consoleAbstractionMock = new();
			consoleAbstractionMock.Setup(c => c.GetWindowWidth()).Returns(ConsoleWidth);
			consoleAbstractionMock.Setup(c => c.ReadKey(true)).Returns(userPressedCode);

			Mock<ILogger> loggerMock = new();

			Mock<AboutAction> aboutActionMock = new(loggerMock.Object) {
				Object = {
					Console = consoleAbstractionMock.Object
				}
			};
			aboutActionMock.Setup(a => a.GetAboutText()).CallBase();
			aboutActionMock.Setup(a => a.Show3rdPartyLicenseTexts()).CallBase();

			// act
			int exitCode = await aboutActionMock.Object.Run();

			// assert
			exitCode.Should().Be(0);

			// verify function calls
			aboutActionMock.Verify(a => a.GetAboutText(), Times.Once);
			aboutActionMock.Verify(a => a.Show3rdPartyLicenseTexts(), Times.Once);
			aboutActionMock.VerifyNoOtherCalls();
		}
	}
}
