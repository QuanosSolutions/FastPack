using System.Data.HashFunction.xxHash;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using HashDepot;
using K4os.Hash.xxHash;

namespace FastPack.Benchmarks.Hashing;

[MemoryDiagnoser]
public class HashComparison
{
	/*
		Intel Core i7-7820X CPU 3.60GHz (Kaby Lake), 1 CPU, 16 logical and 8 physical cores
		.NET SDK=6.0.300
		  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
		  Job-AFVQMW : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT

		InvocationCount=1  UnrollFactor=1

		|      Method |    N |           Mean |         Error |        StdDev |         Median |         Ratio | RatioSD | Allocated |
		|------------ |----- |---------------:|--------------:|--------------:|---------------:|--------------:|--------:|----------:|
		|    XXHash64 | 0.01 |       2.877 us |     0.1592 us |     0.4567 us |       2.700 us |      baseline |         |     816 B |
		|      XXK4os | 0.01 |       5.395 us |     0.3317 us |     0.9463 us |       5.000 us |  1.92x slower |   0.42x |   1,240 B |
		| XXHashDepot | 0.01 |       6.244 us |     0.3251 us |     0.8449 us |       6.100 us |  2.23x slower |   0.48x |  33,272 B |
		|  XXHashFunc | 0.01 |      31.886 us |     0.6268 us |     0.9187 us |      31.900 us | 10.79x slower |   1.76x |   5,056 B |
		|     MD5Hash | 0.01 |      26.021 us |     0.6513 us |     1.8371 us |      24.800 us |  9.23x slower |   1.34x |   1,024 B |
		|    SHA1Hash | 0.01 |      26.859 us |     1.6753 us |     4.3839 us |      26.550 us |  9.54x slower |   1.72x |   1,040 B |
		|  SHA256Hash | 0.01 |      51.665 us |     1.0381 us |     2.8765 us |      50.850 us | 18.40x slower |   2.95x |   1,056 B |
		|             |      |                |               |               |                |               |         |           |
		|    XXHash64 |  0.1 |      15.861 us |     0.4782 us |     1.2847 us |      15.500 us |      baseline |         |     816 B |
		|      XXK4os |  0.1 |      16.909 us |     0.5212 us |     1.4090 us |      16.800 us |  1.07x slower |   0.11x |   1,240 B |
		| XXHashDepot |  0.1 |      29.435 us |     1.1010 us |     2.9954 us |      28.200 us |  1.87x slower |   0.23x |  33,272 B |
		|  XXHashFunc |  0.1 |     299.166 us |    20.6007 us |    54.9874 us |     276.600 us | 18.97x slower |   3.70x |   5,064 B |
		|     MD5Hash |  0.1 |     190.081 us |     1.1066 us |     1.6899 us |     189.300 us | 11.76x slower |   1.10x |   1,024 B |
		|    SHA1Hash |  0.1 |     182.761 us |     3.6863 us |    10.5765 us |     188.000 us | 11.67x slower |   1.21x |   1,040 B |
		|  SHA256Hash |  0.1 |     425.708 us |     1.5355 us |     1.1988 us |     425.300 us | 26.05x slower |   2.56x |   1,056 B |
		|             |      |                |               |               |                |               |         |           |
		|    XXHash64 |    1 |     150.439 us |     3.2704 us |     9.1167 us |     149.100 us |      baseline |         |     816 B |
		|      XXK4os |    1 |     129.701 us |     3.4301 us |     9.6184 us |     131.700 us |  1.16x faster |   0.10x |   1,240 B |
		| XXHashDepot |    1 |     253.189 us |     5.0381 us |    12.7318 us |     250.300 us |  1.67x slower |   0.11x |  33,272 B |
		|  XXHashFunc |    1 |   2,704.107 us |   368.6591 us | 1,051.8050 us |   2,470.400 us | 18.18x slower |   7.03x |   5,024 B |
		|     MD5Hash |    1 |   1,946.879 us |    38.7786 us |   102.1585 us |   1,893.600 us | 12.92x slower |   0.88x |   1,024 B |
		|    SHA1Hash |    1 |   1,684.750 us |    16.9537 us |    13.2363 us |   1,680.500 us | 11.28x slower |   0.53x |   1,040 B |
		|  SHA256Hash |    1 |   4,306.367 us |    85.5507 us |   191.3465 us |   4,261.600 us | 28.31x slower |   2.07x |   1,056 B |
		|             |      |                |               |               |                |               |         |           |
		|    XXHash64 |   10 |   1,866.284 us |    39.6623 us |   107.9041 us |   1,850.450 us |      baseline |         |     816 B |
		|      XXK4os |   10 |   1,765.771 us |    35.0646 us |    86.6711 us |   1,754.350 us |  1.06x faster |   0.08x |   1,240 B |
		| XXHashDepot |   10 |   3,020.948 us |    59.7695 us |   165.6210 us |   2,973.100 us |  1.62x slower |   0.12x |  33,272 B |
		|  XXHashFunc |   10 |  22,439.835 us | 1,725.0611 us | 4,751.3193 us |  20,836.100 us | 12.11x slower |   2.76x |   4,928 B |
		|     MD5Hash |   10 |  23,586.899 us | 1,970.5648 us | 5,653.9199 us |  21,071.200 us | 12.55x slower |   3.19x |   1,024 B |
		|    SHA1Hash |   10 |  20,225.402 us | 1,482.5410 us | 4,083.3486 us |  19,016.000 us | 10.85x slower |   2.32x |     944 B |
		|  SHA256Hash |   10 |  44,583.584 us |   977.2605 us | 2,658.7091 us |  43,389.750 us | 23.97x slower |   2.06x |   1,248 B |
		|             |      |                |               |               |                |               |         |           |
		|    XXHash64 |  100 |  22,162.352 us |   756.5996 us | 2,071.1793 us |  21,761.700 us |      baseline |         |     816 B |
		|      XXK4os |  100 |  21,843.518 us | 1,112.8083 us | 3,102.0648 us |  20,593.600 us |  1.03x faster |   0.16x |   1,144 B |
		| XXHashDepot |  100 |  35,714.303 us | 1,754.1683 us | 4,947.6627 us |  33,912.150 us |  1.63x slower |   0.23x |  33,464 B |
		|  XXHashFunc |  100 | 194,261.502 us | 3,829.4861 us | 7,192.7032 us | 191,578.950 us |  8.49x slower |   0.74x |   4,928 B |
		|     MD5Hash |  100 | 190,332.757 us | 3,298.9498 us | 4,172.1152 us | 188,579.000 us |  8.28x slower |   0.72x |     928 B |
		|    SHA1Hash |  100 | 173,287.614 us | 2,629.9866 us | 3,229.8598 us | 172,496.900 us |  7.51x slower |   0.64x |     944 B |
		|  SHA256Hash |  100 | 429,635.013 us | 6,246.8140 us | 5,843.2737 us | 428,113.500 us | 18.49x slower |   1.55x |   3,144 B |
	 */

#pragma warning disable CS8618
	private System.IO.MemoryStream _sourceStream;

	[Params(0.01, 0.1,1, 10, 100, 1000)] // Megabytes
	public double N;

	[GlobalSetup]
	public void GlobalSetup()
	{
		_sourceStream = new System.IO.MemoryStream();
		byte[] data = new byte[(int)Math.Floor(1024 * 1024 * N)];
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
	public ulong XXHash64()
	{
		return Standart.Hash.xxHash.xxHash64.ComputeHash(_sourceStream);
	}

	[Benchmark]
	public byte[] XXK4os()
	{
		return new XXH64().AsHashAlgorithm().ComputeHash(_sourceStream);
	}

	[Benchmark]
	public ulong XXHashDepot()
	{
		return XXHash.Hash64(_sourceStream);
	}

	[Benchmark]
	public byte[] XXHashFunc()
	{
		return xxHashFactory.Instance.Create().ComputeHash(_sourceStream).Hash;
	}

	[Benchmark()]
	public byte[] MD5Hash()
	{
		return MD5.Create().ComputeHash(_sourceStream);
	}

	[Benchmark]
	public byte[] SHA1Hash()
	{
		return SHA1.Create().ComputeHash(_sourceStream);
	}

	[Benchmark]
	public byte[] SHA256Hash()
	{
		return SHA256.Create().ComputeHash(_sourceStream);
	}
}