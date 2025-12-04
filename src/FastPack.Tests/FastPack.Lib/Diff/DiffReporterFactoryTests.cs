using System;
using FastPack.Lib;
using FastPack.Lib.Diff;
using FastPack.Lib.Logging;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Diff;

[TestFixture]
public class DiffReporterFactoryTests
{
	[Test]
	public void Ensure_GetDiffReporter_Creates_TextDiffReporter()
	{
		// arrange
		IDiffReporterFactory diffReporterFactory = new DiffReporterFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		IDiffReporter diffReporter = diffReporterFactory.GetDiffReporter(OutputFormat.Text, loggerMock.Object);

		// assert
		diffReporter.Should().NotBeNull();
		diffReporter.Should().BeOfType<TextDiffReporter>();
	}

	[Test]
	public void Ensure_GetDiffReporter_Creates_JsonDiffReporter()
	{
		// arrange
		IDiffReporterFactory diffReporterFactory = new DiffReporterFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		IDiffReporter diffReporter = diffReporterFactory.GetDiffReporter(OutputFormat.Json, loggerMock.Object);

		// assert
		diffReporter.Should().NotBeNull();
		diffReporter.Should().BeOfType<JsonDiffReporter>();
	}

	[Test]
	public void Ensure_GetDiffReporter_Creates_XmlDiffReporter()
	{
		// arrange
		IDiffReporterFactory diffReporterFactory = new DiffReporterFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		IDiffReporter diffReporter = diffReporterFactory.GetDiffReporter(OutputFormat.Xml, loggerMock.Object);

		// assert
		diffReporter.Should().NotBeNull();
		diffReporter.Should().BeOfType<XmlDiffReporter>();
	}

	[Test]
	public void Ensure_GetDiffReporter_ThrowsExceptionForUnknownOutputFormat()
	{
		// arrange
		IDiffReporterFactory diffReporterFactory = new DiffReporterFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		Action act = () => diffReporterFactory.GetDiffReporter((OutputFormat)9999, loggerMock.Object);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("There is no diff reporter for output type 9999.");
	}
}