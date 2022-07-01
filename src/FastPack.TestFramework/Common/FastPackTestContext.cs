using System;
using System.Collections.Generic;
using System.IO;

namespace FastPack.TestFramework.Common;

class FastPackTestContext : IDisposable
{
	public List<string> DirectoriesToRemove { get; } = new();
	public List<string> FilesToRemove { get; } = new();
	public string TestRootDirectoryPath { get; private set; }

	private FastPackTestContext()
	{

	}

	public static FastPackTestContext Create(bool createTestRootDirectory = false)
	{
		FastPackTestContext fastPackTestContext = new();

		if (createTestRootDirectory)
			fastPackTestContext.TestRootDirectoryPath = fastPackTestContext.AddDirectoryToRemove(TestUtil.CreateRandomTempDirectory());

		return fastPackTestContext;
	}

	public string GetTempFileName(string extension = null, bool addTestRootDirectoryPath = false, bool createFile = false, bool addToFilesToRemove = false)
	{
		string tempFileName = Guid.NewGuid().ToString("N") + extension;

		string filePath = addTestRootDirectoryPath ? Path.Combine(TestRootDirectoryPath, tempFileName) : tempFileName;

		if (createFile)
			File.WriteAllText(filePath, string.Empty);

		if (addToFilesToRemove)
			FilesToRemove.Add(filePath);

		return filePath;
	}

	public string GetTempFileNameUnderCurrentDir(string extension = null, bool addCurrentDirectoryPath = false, bool createFile = false, bool addToFilesToRemove = false)
	{
		string tempFileName = Guid.NewGuid().ToString("N") + extension;

		string filePath = addCurrentDirectoryPath ? Path.Combine(Environment.CurrentDirectory, tempFileName) : tempFileName;

		if (createFile)
			File.WriteAllText(filePath, string.Empty);

		if (addToFilesToRemove)
			FilesToRemove.Add(filePath);

		return filePath;
	}

	public string AddDirectoryToRemove(string directoryPathToRemove)
	{
		DirectoriesToRemove.Add(directoryPathToRemove);

		return directoryPathToRemove;
	}

	public void Dispose()
	{
		FilesToRemove.ForEach(f =>TestUtil.DeleteFileCatchingExceptions(f));
		DirectoriesToRemove.ForEach(d => TestUtil.DeleteDirectoryCatchingExceptions(d));
	}
}