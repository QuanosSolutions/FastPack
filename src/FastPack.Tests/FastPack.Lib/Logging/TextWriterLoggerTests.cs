using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FastPack.Lib.Logging;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Logging;

[TestFixture]
public class TextWriterLoggerTests
{
	[Test]
	public async Task Ensure_Constructor_Chooses_InfoTextWriter_As_DebugTextWriter()
	{
		// arrange
		Mock<TextWriter> textWriterMock = new();
		ILogger logger = new TextWriterLogger(textWriterMock.Object);
		string testMessage = "test";

		// act
		await logger.Debug(testMessage);

		// assert
		textWriterMock.Verify(l => l.WriteAsync(testMessage), Times.Exactly(1));
		textWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Constructor_Chooses_InfoTextWriter_As_ErrorTextWriter()
	{
		// arrange
		Mock<TextWriter> textWriterMock = new();
		ILogger logger = new TextWriterLogger(textWriterMock.Object);
		string testMessage = "test";

		// act
		await logger.Error(testMessage);

		// assert
		textWriterMock.Verify(l => l.WriteAsync(testMessage), Times.Exactly(1));
		textWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Debug_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";

		// act
		await logger.Debug(testMessage);

		// assert
		debugTextWriterMock.Verify(l => l.WriteAsync(testMessage), Times.Exactly(1));
		debugTextWriterMock.VerifyNoOtherCalls();
		infoTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_DebugLine_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";
		((TextWriterLogger) logger).DebugLinePrefix = "DEBUG ";

		// act
		await logger.DebugLine(testMessage);

		// assert
		debugTextWriterMock.Verify(l => l.WriteLineAsync($"{(logger as TextWriterLogger).DebugLinePrefix}{testMessage}"), Times.Exactly(1));
		debugTextWriterMock.VerifyNoOtherCalls();
		infoTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Info_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";

		// act
		await logger.Info(testMessage);

		// assert
		infoTextWriterMock.Verify(l => l.WriteAsync(testMessage), Times.Exactly(1));
		infoTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_InfoLine_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";

		// act
		await logger.InfoLine(testMessage);

		// assert
		infoTextWriterMock.Verify(l => l.WriteLineAsync(testMessage), Times.Exactly(1));
		infoTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_Error_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";

		// act
		await logger.Error(testMessage);

		// assert
		errorTextWriterMock.Verify(l => l.WriteAsync(testMessage), Times.Exactly(1));
		errorTextWriterMock.VerifyNoOtherCalls();
		infoTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_ErrorLine_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";
		((TextWriterLogger)logger).ErrorLinePrefix = "ERROR ";

		// act
		await logger.ErrorLine(testMessage);

		// assert
		errorTextWriterMock.Verify(l => l.WriteLineAsync($"{(logger as TextWriterLogger).ErrorLinePrefix}{testMessage}"), Times.Exactly(1));
		errorTextWriterMock.VerifyNoOtherCalls();
		infoTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_StartTextProgress_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";

		// act
		await logger.StartTextProgress(testMessage);

		// assert
		infoTextWriterMock.Verify(l => l.WriteLineAsync(testMessage), Times.Exactly(1));
		infoTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_ReportTextProgress_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";
		double percentage = 50;
		// This is needed due to the throttling
		await Task.Delay(110);

		// act
		await logger.ReportTextProgress(percentage, testMessage);

		// assert
		FormattableString formattableString = $"{testMessage}{percentage:F}%";
		infoTextWriterMock.Verify(l => l.WriteLineAsync(formattableString.ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		infoTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_ReportTextProgress_With_100_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";
		double percentage = 100;

		// act
		await logger.ReportTextProgress(percentage, testMessage);

		// assert
		FormattableString formattableString = $"{testMessage}{percentage:F}%";
		infoTextWriterMock.Verify(l => l.WriteLineAsync(formattableString.ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		infoTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_ReportTextProgress_Throttling_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";
		double percentage = 50;
		// This is needed due to the throttling
		await Task.Delay(110);

		// act
		await logger.ReportTextProgress(percentage, testMessage);
		await logger.ReportTextProgress(percentage, testMessage);

		// assert
		FormattableString formattableString = $"{testMessage}{percentage:F}%";
		infoTextWriterMock.Verify(l => l.WriteLineAsync(formattableString.ToString(CultureInfo.InvariantCulture)), Times.Exactly(1));
		infoTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_FinishTextProgress_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
		string testMessage = "test";

		// act
		await logger.FinishTextProgress(testMessage);

		// assert
		infoTextWriterMock.Verify(l => l.WriteLineAsync(testMessage), Times.Exactly(1));
		infoTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task Ensure_SetQuiet_Works()
	{
		// arrange
		Mock<TextWriter> infoTextWriterMock = new();
		Mock<TextWriter> debugTextWriterMock = new();
		Mock<TextWriter> errorTextWriterMock = new();
		ILogger logger = new TextWriterLogger(infoTextWriterMock.Object, debugTextWriterMock.Object, errorTextWriterMock.Object);
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
		((TextWriterLogger) logger).Quiet.Should().Be(true);
		infoTextWriterMock.VerifyNoOtherCalls();
		debugTextWriterMock.VerifyNoOtherCalls();
		errorTextWriterMock.Verify(l => l.WriteLineAsync($"{(logger as TextWriterLogger).ErrorLinePrefix}{testMessage}"), Times.Exactly(1));
		errorTextWriterMock.Verify(l => l.WriteAsync(testMessage), Times.Exactly(1));
	}
}