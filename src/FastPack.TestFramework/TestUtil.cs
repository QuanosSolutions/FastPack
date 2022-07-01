using System;
using System.IO;
using System.Threading.Tasks;
using FastPack.TestFramework.Common;

namespace FastPack.TestFramework;

internal static class TestUtil
{
	public static void DeleteFileCatchingExceptions(string filePath, bool writeWarningOnException = false)
	{
		try
		{
			if (File.Exists(filePath))
				File.Delete(filePath);
		}
		catch (Exception exception)
		{
			if (writeWarningOnException)
				WriteWarning(exception.ToString());
		}
	}

	public static string GetRandomFilePath(string directoryPath, string extension=null)
	{
		extension = extension == null ? string.Empty : $".{extension}";

		return Path.Combine(directoryPath, $"{Path.GetRandomFileName()}{extension}");
	}

	public static string GetRandomTempDirectoryPath()
	{
		return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
	}

	public static string CreateRandomTempDirectory()
	{
		return Directory.CreateDirectory(GetRandomTempDirectoryPath()).FullName;
	}

	public static async Task<string> CreateFileWithRandomNameFromTestData(string pathRelativeToTestDataDirectory, string targetDirectoryPath, string extension = null)
	{
		if (!Directory.Exists(targetDirectoryPath))
			Directory.CreateDirectory(targetDirectoryPath!);

		string randomFilePath = GetRandomFilePath(targetDirectoryPath, extension);
		await TestData.WriteToFile(pathRelativeToTestDataDirectory, randomFilePath);

		return randomFilePath;
	}

	public static async Task UseRandomTempDirectory(Func<string, Task> directoryAction)
	{
		string randomTempDirectory = CreateRandomTempDirectory();

		try
		{
			await directoryAction(randomTempDirectory);
		}
		finally
		{
			DeleteDirectoryCatchingExceptions(randomTempDirectory);
		}
	}

	public static async Task UseFileWithRandomNameFromTestData(string pathRelativeToTestDataDirectory, Func<string, Task> fileAction, string extension = null)
	{
		await UseRandomTempDirectory(async randomTempDirectory => {
			string filePath = await CreateFileWithRandomNameFromTestData(pathRelativeToTestDataDirectory, randomTempDirectory, extension);
			await fileAction(filePath);
		});
	}

	public static void DeleteDirectoryCatchingExceptions(string directoryPath, bool writeWarningOnException = false)
	{
		try
		{
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}
		catch (Exception exception)
		{
			if (writeWarningOnException)
				WriteWarning(exception.ToString());
		}
	}

	public static void WriteWarning(string message)
	{
		Console.WriteLine($"[Warning] {message}");
	}
}