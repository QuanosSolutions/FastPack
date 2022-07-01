using BenchmarkDotNet.Attributes;

namespace FastPack.Benchmarks.MemoryStream;

[MemoryDiagnoser]
public class MemoryStreamCopyToComparison
{
	/*
		BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1706 (21H2)
		Intel Core i7-7820X CPU 3.60GHz (Kaby Lake), 1 CPU, 16 logical and 8 physical cores
		.NET SDK=6.0.100
		  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
		  Job-UVGGBL : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT

		InvocationCount=1  UnrollFactor=1

		|       Method |    N |       Mean |     Error |     StdDev |     Median |        Ratio | RatioSD | Allocated |
		|------------- |----- |-----------:|----------:|-----------:|-----------:|-------------:|--------:|----------:|
		|   NoSizeInit |   10 |   2.045 ms | 0.0406 ms |  0.0722 ms |   2.036 ms |     baseline |         |     10 MB |
		| WithSizeInit |   10 |   2.038 ms | 0.0406 ms |  0.0607 ms |   2.039 ms | 1.00x faster |   0.05x |     10 MB |
		|              |      |            |           |            |            |              |         |           |
		|   NoSizeInit |  100 |  17.883 ms | 0.3370 ms |  0.8640 ms |  17.632 ms |     baseline |         |    100 MB |
		| WithSizeInit |  100 |  31.990 ms | 6.1536 ms | 18.1441 ms |  18.225 ms | 2.03x slower |   1.06x |    100 MB |
		|              |      |            |           |            |            |              |         |           |
		|   NoSizeInit | 1000 | 492.510 ms | 9.5928 ms | 15.2153 ms | 486.733 ms |     baseline |         |  1,000 MB |
		| WithSizeInit | 1000 | 488.081 ms | 9.3664 ms | 11.1500 ms | 485.267 ms | 1.01x faster |   0.04x |  1,000 MB |
	 */

#pragma warning disable CS8618
	private System.IO.MemoryStream _sourceStream;

	[Params(10, 100, 1000)] // Megabytes
	public int N;

	[GlobalSetup]
	public void GlobalSetup()
	{
		_sourceStream = new System.IO.MemoryStream();
		byte[] data = new byte[1024 * 1024 * N];
		new Random(42).NextBytes(data);

		_sourceStream.Write(data);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		_sourceStream.Dispose();
	}

	[IterationSetup]
	public void IterationSetup()
	{
		_sourceStream.Seek(0, SeekOrigin.Begin);
	}

	[Benchmark(Baseline = true)]
	public void NoSizeInit()
	{
		System.IO.MemoryStream targetStream = new();
		_sourceStream.CopyTo(targetStream);
	}

	[Benchmark]
	public void WithSizeInit()
	{
		System.IO.MemoryStream targetStream = new((int)_sourceStream.Length);
		_sourceStream.CopyTo(targetStream);
	}
}