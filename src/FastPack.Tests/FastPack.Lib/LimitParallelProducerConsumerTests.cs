using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.TypeExtensions;
using FastPack.TestFramework.Common;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib;

[TestFixture]
public class LimitParallelProducerConsumerTests
{
	[Test]
	public async Task TestProcessData()
	{
		int sum = 0;
		int amountOfNumbers = 21;
		int limit = 200;
		List<ParallelProducerConsumerTestItem> numbers = new(amountOfNumbers);
		for (int i = 1; i < amountOfNumbers; i++)
			numbers.Add(new ParallelProducerConsumerTestItem { Value = i, Size = 10 * i });
		int currentSize = 0;
		int expected = numbers.Sum(x => x.Value);
		await LimitParallelProducerConsumer.ProcessData<ParallelProducerConsumerTestItem, ParallelProducerConsumerTestItem, int>(numbers,
			async i =>
			{
				await Task.Delay(i.Value);
				// Console.WriteLine($"Produce: {i.Value}");
				return i.ToIEnumerable();
			},
			async i =>
			{
				await Task.Delay(i.Size);
				// Console.WriteLine($"Consume: {i.Value}");
				sum += i.Value;
			},
			(items, current) =>
			{
				int size = items.Sum(x => x.Size);
				// Console.WriteLine(size);
				return size + current.Size <= limit;
			},
			source => source.Value.ToIEnumerable(),
			produced => produced.Value,
			Environment.ProcessorCount,
			1);
		Assert.AreEqual(expected, sum);
		Assert.AreEqual(0, currentSize);
	}

	[Test]
	public async Task TestProcessData_ManyConsumersOneProducer()
	{
		int sum = 0;
		int amountOfNumbers = 21;
		int limit = 200;
		List<ParallelProducerConsumerTestItem> numbers = new(amountOfNumbers);
		for (int i = 1; i < amountOfNumbers; i++)
			numbers.Add(new ParallelProducerConsumerTestItem { Value = i, Size = 10 * i });
		int currentSize = 0;
		int expected = numbers.Sum(x => x.Value);
		await LimitParallelProducerConsumer.ProcessData<ParallelProducerConsumerTestItem, ParallelProducerConsumerTestItem, int>(numbers,
			async i =>
			{
				await Task.Delay(i.Value);
				// Console.WriteLine($"Produce: {i.Value}");
				return i.ToIEnumerable();
			},
			async i =>
			{
				await Task.Delay(i.Size);
				// Console.WriteLine($"Consume: {i.Value}");
				Interlocked.Add(ref sum, i.Value);
			},
			(items, current) =>
			{
				int size = items.Sum(x => x.Size);
				// Console.WriteLine(size);
				return size + current.Size <= limit;
			},
			source => source.Value.ToIEnumerable(),
			produced => produced.Value,
			1,
			Environment.ProcessorCount);
		Assert.AreEqual(expected, sum);
		Assert.AreEqual(0, currentSize);
	}
}