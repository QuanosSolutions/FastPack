using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using FastPack.Lib;
using FastPack.Lib.TypeExtensions;
using FastPack.TestFramework.Common;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib;

[TestFixture]
public class ParallelProducerConsumerTests
{
	[Test]
	public async Task TestProcessData()
	{
		int sum = 0;
		int amountOfNumbers = 100;
		List<int> numbers = new(amountOfNumbers);
		for (int i = 0; i < amountOfNumbers; i++)
			numbers.Add(i);
		int expected = numbers.Sum();
		await ParallelProducerConsumer.ProcessData<int, int>(numbers, async i =>
			{
				await Task.Delay(i);
				//Console.WriteLine($"Produce: {i}");
				return i.ToIEnumerable();
			}, async i =>
			{
				await Task.Delay(i);
				//Console.WriteLine($"Consume: {i}");
				sum += i;
			},
			Environment.ProcessorCount,
			1);
		sum.Should().Be(expected);
	}

	[Test]
	public async Task TestProcessDataWithParallelLimit()
	{
		int sum = 0;
		int amountOfNumbers = 21;
		int limit = 200;
		List<ParallelProducerConsumerTestItem> numbers = new(amountOfNumbers);
		for (int i = 1; i < amountOfNumbers; i++)
			numbers.Add(new ParallelProducerConsumerTestItem { Value = i, Size = 10 * i });
		int currentSize = 0;
		int currentCountInQueue = 0;
		int expected = numbers.Sum(x => x.Value);
		SemaphoreSlim semaphoreSlim = new(1, 1);
		SemaphoreSlim signal = new(0, 1);
		await ParallelProducerConsumer.ProcessData<ParallelProducerConsumerTestItem, ParallelProducerConsumerTestItem>(numbers, async i =>
			{
				await semaphoreSlim.WaitAsync();
				try
				{
					while (currentSize + i.Size > limit)
						await signal.WaitAsync();
					Interlocked.Add(ref currentSize, i.Size);
					Interlocked.Increment(ref currentCountInQueue);
				}
				finally
				{
					semaphoreSlim.Release();
				}
				//Console.WriteLine($"Produce: {i.Value}");
				return i.ToIEnumerable();
			}, async i =>
			{
				Interlocked.Decrement(ref currentCountInQueue);
				//Console.WriteLine($"Size: {currentSize}, Count: {currentCountInQueue}");
				limit.Should().BeGreaterThanOrEqualTo(currentSize);
				await Task.Delay(i.Size * 10);
				sum += i.Value;
				Interlocked.Add(ref currentSize, -1 * i.Size);
				signal.Release();
			},
			Environment.ProcessorCount,
			1);
		sum.Should().Be(expected);
		currentSize.Should().Be(0);
	}
}