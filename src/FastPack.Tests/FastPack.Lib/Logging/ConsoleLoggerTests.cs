using System;
using System.Globalization;
using System.Threading.Tasks;
using FastPack.Lib.Logging;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Logging;

[TestFixture]
public class ConsoleLoggerTests
{
	[Test]
	public async Task Ensure_Debug_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object, true);
		string testMessage = "test";

		// act
		await logger.Debug(testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteAsync(testMessage), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_DebugLine_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object, true);
		string testMessage = "test";
		((ConsoleLogger)logger).DebugLinePrefix = "DEBUG ";

		// act
		await logger.DebugLine(testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteLineAsync(((ConsoleLogger)logger).DebugLinePrefix + testMessage), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Debug_Disabled_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";

		// act
		await logger.Debug(testMessage);
		await logger.DebugLine(testMessage);

		// assert
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Info_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";

		// act
		await logger.Info(testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteAsync(testMessage), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_InfoLine_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";

		// act
		await logger.InfoLine(testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteLineAsync(testMessage), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Error_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";

		// act
		await logger.Error(testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleErrorWriteAsync(testMessage), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.SetForegroundColor(ConsoleColor.Red), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.ResetColor(), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_ErrorLine_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";
		((ConsoleLogger)logger).ErrorLinePrefix = "ERROR ";

		// act
		await logger.ErrorLine(testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleErrorWriteLineAsync($"{((ConsoleLogger) logger).ErrorLinePrefix}{testMessage}"), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.SetForegroundColor(ConsoleColor.Red), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.ResetColor(), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_StartTextProgress_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";

		// act
		await logger.StartTextProgress(testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteAsync($"\r{testMessage}"), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_ReportTextProgress_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";
		double percentage = 50;
		// This is needed due to the throttling
		await Task.Delay(110);

		// act
		await logger.ReportTextProgress(percentage, testMessage);

		// assert
		FormattableString formattableString = $"{testMessage}{percentage:F}%";
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteAsync($"\r{formattableString.ToString(CultureInfo.InvariantCulture)}"), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.GetWindowWidth(), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_ReportTextProgress_With_100_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";
		double percentage = 100;

		// act
		await logger.ReportTextProgress(percentage, testMessage);

		// assert
		FormattableString formattableString = $"{testMessage}{percentage:F}%";
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteAsync($"\r{formattableString.ToString(CultureInfo.InvariantCulture)}"), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.GetWindowWidth(), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_ReportTextProgress_Throttling_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";
		double percentage = 50;
		// This is needed due to the throttling
		await Task.Delay(110);

		// act
		await logger.ReportTextProgress(percentage, testMessage);
		await logger.ReportTextProgress(percentage, testMessage);

		// assert
		FormattableString formattableString = $"{testMessage}{percentage:F}%";
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteAsync($"\r{formattableString.ToString(CultureInfo.InvariantCulture)}"), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.GetWindowWidth(), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_FinishTextProgress_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";

		// act
		await logger.FinishTextProgress(testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleOutWriteLineAsync($"\r{testMessage}"), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.GetWindowWidth(), Times.Exactly(1));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_SetQuiet_Works()
	{
		// arrange
		Mock<IConsoleAbstraction> consoleAbstractionMock = new Mock<IConsoleAbstraction>();
		ILogger logger = new ConsoleLogger(consoleAbstractionMock.Object);
		string testMessage = "test";

		// act
		logger.SetQuiet();
		await logger.Debug(testMessage);
		await logger.DebugLine(testMessage);
		await logger.Info(testMessage);
		await logger.InfoLine(testMessage);
		await logger.Error(testMessage);
		await logger.ErrorLine(testMessage);
		await logger.StartTextProgress(testMessage);
		await logger.FinishTextProgress(testMessage);
		await logger.ReportTextProgress(100, testMessage);

		// assert
		consoleAbstractionMock.Verify(l => l.ConsoleErrorWriteLineAsync($"{((ConsoleLogger)logger).ErrorLinePrefix}{testMessage}"), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.ConsoleErrorWriteAsync(testMessage), Times.Exactly(1));
		consoleAbstractionMock.Verify(l => l.SetForegroundColor(ConsoleColor.Red), Times.Exactly(2));
		consoleAbstractionMock.Verify(l => l.ResetColor(), Times.Exactly(2));
		consoleAbstractionMock.VerifyNoOtherCalls();
	}
}