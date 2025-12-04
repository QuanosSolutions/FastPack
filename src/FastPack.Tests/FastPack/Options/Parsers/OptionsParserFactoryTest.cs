using System;
using System.Collections.Generic;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;
using FastPack.Options.Parsers;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Options.Parsers
{
	[TestFixture]
	public class OptionsParserFactoryTest
	{
		[Test]
		public void EnsureFactoryIsImplementedForEachEnumValue()
		{
			OptionsParserFactory factory = new();

			foreach (ActionType actionType in Enum.GetValues<ActionType>())
			{
				factory.Create(actionType, Mock.Of<ILogger>()).Should().NotBeNull();
			}
		}

		[Test]
		public void EnsureCorrectTypes()
		{
			OptionsParserFactory factory = new();
			Dictionary<ActionType, Type> typeMapping = new() {
				{ActionType.Version, typeof(VersionOptionsParser)},
				{ActionType.About, typeof(AboutOptionsParser)},
				{ActionType.Info, typeof(InfoOptionsParser)},
				{ActionType.Licenses, typeof(LicensesOptionsParser)},
				{ActionType.Diff, typeof(DiffOptionsParser)},
				{ActionType.Pack, typeof(PackOptionsParser)},
				{ActionType.Unpack, typeof(UnpackOptionsParser)},
				{ActionType.Help, typeof(HelpOptionsParser)},
			};

			foreach (ActionType actionType in Enum.GetValues<ActionType>())
			{
				factory.Create(actionType, Mock.Of<ILogger>()).Should().BeOfType(typeMapping[actionType]);
			}
		}

		[Test]
		public void Ensure_Create_ThrowsExceptionForUnknownActionType()
		{
			// arrange
			IOptionsParserFactory optionsParserFactory = new OptionsParserFactory();
			Mock<ILogger> loggerMock = new Mock<ILogger>();

			// act
			Action act = () => optionsParserFactory.Create((ActionType)9999, loggerMock.Object);

			// assert
			act.Should().Throw<NotImplementedException>().WithMessage("OptionsParser for 9999 is not implemented, but should be!");
		}
	}
}