using System;
using FastPack.Lib;
using FastPack.Lib.Logging;
using FastPack.Lib.ManifestReporting;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.ManifestReporting;

[TestFixture]
internal class ManifestReporterFactoryTests
{
	[Test]
	public void Ensure_GetManifestReporter_Creates_TextManifestReporter()
	{
		// arrange
		IManifestReporterFactory manifestReporterFactory = new ManifestReporterFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		IManifestReporter manifestReporter = manifestReporterFactory.GetManifestReporter(OutputFormat.Text, loggerMock.Object);

		// assert
		manifestReporter.Should().NotBeNull();
		manifestReporter.Should().BeOfType<TextManifestReporter>();
	}

	[Test]
	public void Ensure_GetManifestReporter_Creates_JsonManifestReporter()
	{
		// arrange
		IManifestReporterFactory manifestReporterFactory = new ManifestReporterFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		IManifestReporter manifestReporter = manifestReporterFactory.GetManifestReporter(OutputFormat.Json, loggerMock.Object);

		// assert
		manifestReporter.Should().NotBeNull();
		manifestReporter.Should().BeOfType<JsonManifestReporter>();
	}

	[Test]
	public void Ensure_GetManifestReporter_Creates_XmlManifestReporter()
	{
		// arrange
		IManifestReporterFactory manifestReporterFactory = new ManifestReporterFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		IManifestReporter manifestReporter = manifestReporterFactory.GetManifestReporter(OutputFormat.Xml, loggerMock.Object);

		// assert
		manifestReporter.Should().NotBeNull();
		manifestReporter.Should().BeOfType<XmlManifestReporter>();
	}

	[Test]
	public void Ensure_GetManifestReporter_ThrowsExceptionForUnknownOutputFormat()
	{
		// arrange
		IManifestReporterFactory manifestReporterFactory = new ManifestReporterFactory();
		Mock<ILogger> loggerMock = new Mock<ILogger>();

		// act
		Action act = () => manifestReporterFactory.GetManifestReporter((OutputFormat)9999, loggerMock.Object);

		// assert
		act.Should().Throw<NotSupportedException>().WithMessage("There is no manifest reporter for output type 9999.");
	}
}