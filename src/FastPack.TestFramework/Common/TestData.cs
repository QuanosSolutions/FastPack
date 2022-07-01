using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FastPack.TestFramework.Common;

internal class TestData
{
	private static Assembly ThisAssembly { get; } = typeof(TestData).Assembly;
	//private static string TestDataNamespace { get; } = $"{typeof(TestUtil).Namespace}.TestData";

	public static Stream GetFileStream(string pathRelativeToTestDataDirectory, Assembly testDataAssembly = null)
	{
		testDataAssembly ??= ThisAssembly;
		string testDataNamespace = $"{testDataAssembly.GetName().Name}.TestData";

		return testDataAssembly.GetManifestResourceStream($"{testDataNamespace}.{pathRelativeToTestDataDirectory.Replace('\\', '.').Replace('/', '.')}") ?? throw new ArgumentException(
			$"'{pathRelativeToTestDataDirectory}' is not a valid embedded resource. Maybe you forgot to set the build action for this file to 'Embedded resource'?",
			nameof(pathRelativeToTestDataDirectory));
	}

	public static async Task UseFileStream(string pathRelativeToTestDataDirectory, Func<Stream, Task> streamAction, Assembly testDataAssembly = null)
	{
		await using Stream stream = GetFileStream(pathRelativeToTestDataDirectory, testDataAssembly);
		await streamAction(stream);
	}

	public static async Task WriteToFile(string pathRelativeToTestDataDirectory, string targetFilePath, Assembly testDataAssembly = null)
	{
		if (Directory.Exists(targetFilePath))
			Directory.CreateDirectory(targetFilePath);

		await UseFileStream(pathRelativeToTestDataDirectory, async testDataStream =>
		{
			await using FileStream fileStream = File.Create(targetFilePath!);
			await testDataStream.CopyToAsync(fileStream);
		}, testDataAssembly);
	}
}