using System;
using System.Collections.Generic;
using FastPack.Lib.Actions;
using FastPack.Lib.Logging;
using FastPack.Lib.Options;
using AwesomeAssertions;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Actions;

[TestFixture]
public class ActionFactoryTests
{
	[Test]
	public void Ensure_Factory_Throw_On_Unsupported_ActionType()
	{
		ILogger logger = Mock.Of<ILogger>();
		ActionFactory factory = new();
		Action createAction = () => factory.CreateAction((ActionType) 9999, null, logger);
		createAction.Should().Throw<NotSupportedException>().WithMessage("Action type is not supported: 9999");
	}

	[Test]
	public void EnsureCorrectTypes()
	{
		ILogger logger = Mock.Of<ILogger>();
		ActionFactory factory = new();
		Dictionary<ActionType, (Type type, IOptions Options)> typeMapping = new() {
			{ActionType.Pack, (typeof(PackAction), new PackOptions())},
			{ActionType.Version, (typeof(VersionAction), null)},
			{ActionType.Help, (null, null)},
			{ActionType.Unpack, (typeof(UnpackAction), new UnpackOptions())},
			{ActionType.About, (typeof(AboutAction), null)},
			{ActionType.Diff, (typeof(DiffAction), new DiffOptions())},
			{ActionType.Licenses, (typeof(LicensesAction), null)},
			{ActionType.Info, (typeof(InfoAction), new InfoOptions())},
		};

		foreach (ActionType actionType in Enum.GetValues<ActionType>())
		{
			(Type type, IOptions options) = typeMapping[actionType];
			IAction action = factory.CreateAction(actionType, options, logger);

			if (type == null)
				action.Should().BeNull();
			else
				action.Should().BeOfType(type);
		}
	}
}