using System.Collections.Generic;
using FastPack.Lib.TypeExtensions;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.TypeExtensions;

[TestFixture]
public class DictionaryExtensionsTests
{
	[Test]
	public void Ensure_AddMultiple_Works()
	{
		// arrange
		Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
		string testString = " - this is a test - ";

		// act
		dictionary.AddMultiple(new []{ "1", "2" }, testString);

		// assert
		dictionary.Count.Should().Be(2);
		dictionary["1"].Should().Be(testString);
		dictionary["2"].Should().Be(testString);
	}
}