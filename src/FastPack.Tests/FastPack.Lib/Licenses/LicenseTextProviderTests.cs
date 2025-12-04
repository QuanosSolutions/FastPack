using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FastPack.Lib.Licenses;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Licenses;

[TestFixture]
public class LicenseTextProviderTests
{
	[Test]
	public async Task Ensure_GetFastPackLicenseText_Without_LinePrefix_Works()
	{
		// arrange

		// act
		string fastPackLicenseText = await LicenseTextProvider.GetFastPackLicenseText();

		// assert
		fastPackLicenseText.Should().NotBeNull();
		fastPackLicenseText.Should().StartWith("MIT License");
	}

	[Test]
	public async Task Ensure_GetFastPackLicenseText_With_LinePrefix_Works()
	{
		// arrange
		string linePrefix = " - linePrefix - ";

		// act
		string fastPackLicenseText = await LicenseTextProvider.GetFastPackLicenseText(linePrefix);

		// assert
		fastPackLicenseText.Should().NotBeNull();
		fastPackLicenseText.Should().StartWith($"{linePrefix}MIT License");
		foreach (string line in fastPackLicenseText.Split("\n", StringSplitOptions.RemoveEmptyEntries))
			line.Should().StartWith(linePrefix);
	}

	[Test]
	public async Task Ensure_GetThirdPartyLicenseTexts_Works()
	{
		// arrange

		// act
		List<string> thirdPartyLicenseTexts = (await LicenseTextProvider.GetThirdPartyLicenseTexts()).ToList();

		// assert
		thirdPartyLicenseTexts.Should().NotBeEmpty();
		thirdPartyLicenseTexts.First().Should().Contain("MIT License");
	}
}