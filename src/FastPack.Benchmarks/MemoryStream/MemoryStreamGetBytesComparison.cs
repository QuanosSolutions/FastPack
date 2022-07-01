using BenchmarkDotNet.Attributes;

namespace FastPack.Benchmarks.MemoryStream;

[MemoryDiagnoser]
public class MemoryStreamGetBytesComparison
{
	/*
		BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1706 (21H2)
		Intel Core i7-7820X CPU 3.60GHz (Kaby Lake), 1 CPU, 16 logical and 8 physical cores
		.NET SDK=6.0.100
		  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
		  Job-LVBFVY : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT

		InvocationCount=1  UnrollFactor=1

		|    Method |    N |         Mean |      Error |    StdDev |       Median |        Ratio | RatioSD | Allocated |
		|---------- |----- |-------------:|-----------:|----------:|-------------:|-------------:|--------:|----------:|
		|   ToArray |   10 |     5.352 ms |  0.6425 ms |  1.884 ms |     6.063 ms |     baseline |         |     20 MB |
		| GetBuffer |   10 |     3.356 ms |  0.4700 ms |  1.386 ms |     3.165 ms | 1.90x faster |   1.06x |     10 MB |
		|    Resize |   10 |     3.420 ms |  0.5177 ms |  1.518 ms |     2.135 ms | 2.06x faster |   1.32x |     10 MB |
		|           |      |              |            |           |              |              |         |           |
		|   ToArray |  100 |    78.618 ms |  6.4755 ms | 19.093 ms |    68.287 ms |     baseline |         |    200 MB |
		| GetBuffer |  100 |    42.515 ms |  5.4890 ms | 16.185 ms |    49.285 ms | 2.19x faster |   1.01x |    100 MB |
		|    Resize |  100 |    41.929 ms |  5.5725 ms | 16.431 ms |    47.876 ms | 2.23x faster |   1.00x |    100 MB |
		|           |      |              |            |           |              |              |         |           |
		|   ToArray | 1000 | 1,039.355 ms | 20.6185 ms | 42.118 ms | 1,036.217 ms |     baseline |         |  2,000 MB |
		| GetBuffer | 1000 |   485.289 ms |  5.6776 ms |  4.741 ms |   485.117 ms | 2.13x faster |   0.10x |  1,000 MB |
		|    Resize | 1000 |   498.910 ms |  9.8926 ms | 18.337 ms |   493.021 ms | 2.09x faster |   0.12x |  1,000 MB |
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
	public byte[] ToArray()
	{
		System.IO.MemoryStream targetStream = new((int)_sourceStream.Length);
		_sourceStream.CopyTo(targetStream);
		return targetStream.ToArray();
	}

	[Benchmark]
	public byte[] GetBuffer()
	{
		System.IO.MemoryStream targetStream = new((int)_sourceStream.Length);
		_sourceStream.CopyTo(targetStream);

		return targetStream.GetBuffer();
	}

	[Benchmark]
	public byte[] Resize()
	{
		System.IO.MemoryStream targetStream = new((int)_sourceStream.Length);
		_sourceStream.CopyTo(targetStream);

		byte[] bytes = targetStream.GetBuffer();
		Array.Resize(ref bytes, (int)targetStream.Length);
		return bytes;
	}
}