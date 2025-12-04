using System.IO;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Hashing;
using FastPack.TestFramework;
using Moq;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib
{
	[TestFixture]
	internal class CryptoUtilTests
	{
		[Test]
		public async Task Ensure_CalculateFileHash_Works()
		{
			// arrange
			string testFileName = "test.txt";
			string stringInFileToHash = "test test test test test test test test test test";
			Mock<IHashProvider> hashProvider = new Mock<IHashProvider>();

			await TestUtil.UseRandomTempDirectory(async tempDirectory =>
			{
				string fileToCompress = Path.Combine(tempDirectory, testFileName);
				await File.WriteAllTextAsync(fileToCompress, stringInFileToHash);

				// act
				await CryptoUtil.CalculateFileHash(hashProvider.Object, fileToCompress);
			});

			// assert
			hashProvider.Verify(x => x.CalculateHash(It.IsAny<Stream>()), Times.Exactly(1));
		}
	}
}
