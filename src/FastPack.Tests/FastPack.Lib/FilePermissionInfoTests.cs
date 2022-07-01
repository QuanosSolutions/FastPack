using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.TestFramework;
using FluentAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib;

[TestFixture]
internal class FilePermissionInfoTests
{
	[Test, Platform("MacOSX,Linux")]
	public async Task Ensure_GetFilePermissions_Works_LinuxAndOSX()
	{
		// arrange
		FilePermissionInfo filePermissionInfo = new();

		// act

		await TestUtil.UseRandomTempDirectory(tempDirectory =>
		{
			// act
			uint? filePermissions = filePermissionInfo.GetFilePermissions(tempDirectory);

			// assert
			filePermissions.Should().NotBeNull();

			return Task.CompletedTask;
		});
	}

	[Test, Platform("MacOSX,Linux")]
	public async Task Ensure_SetFilePermissions_Works_LinuxAndOSX()
	{
		// arrange
		FilePermissionInfo filePermissionInfo = new();

		// act

		await TestUtil.UseRandomTempDirectory(tempDirectory =>
		{
			// act
			uint? filePermissions = filePermissionInfo.GetFilePermissions(tempDirectory);
			filePermissionInfo.SetFilePermissions(tempDirectory, filePermissions.Value);

			// assert

			return Task.CompletedTask;
		});
	}

	[Test, Platform("Win")]
	public async Task Ensure_GetFilePermissions_Works_Windows()
	{
		// arrange
		FilePermissionInfo filePermissionInfo = new();

		// act

		await TestUtil.UseRandomTempDirectory(tempDirectory => {
			// act
			uint? filePermissions = filePermissionInfo.GetFilePermissions(tempDirectory);

			// assert
			filePermissions.Should().BeNull();

			return Task.CompletedTask;
		});
	}

	[Test, Platform("Win")]
	public async Task Ensure_SetFilePermissions_Works_Windows()
	{
		// arrange
		FilePermissionInfo filePermissionInfo = new();

		// act

		await TestUtil.UseRandomTempDirectory(tempDirectory => {
			// act
			filePermissionInfo.SetFilePermissions(tempDirectory, 55);

			// assert

			return Task.CompletedTask;
		});
	}
}