using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FastPack.Lib;
using FastPack.Lib.Logging;

namespace FastPack.Options.Parsers;

internal class ArgumentsParser
{
	private ILogger Logger { get; }
	public string[] Arguments { get; }
	public int Index { get; set; }
	public bool StrictMode { get; }

	public ArgumentsParser(string[] arguments, ILogger logger, bool strictMode)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
		StrictMode = strictMode;
	}

	public async Task<bool> Parse(Dictionary<string, Func<string, Task<bool>>> parameterProcessingMap)
	{
		while (Index < Arguments.Length)
		{
			string parameterName = Arguments[Index];
			if (parameterProcessingMap.TryGetValue(parameterName, out Func<string, Task<bool>> processor))
			{
				if (!await processor(parameterName))
					return false;
			}
			else if (StrictMode)
			{
				await Logger.ErrorLine($"Unknown parameter: {parameterName}");
				return false;
			}

			Index++;
		}

		return true;
	}

	public async Task<bool> ProcessTextParameterValue(Action<string> parameterAction)
	{
		if (Index + 1 == Arguments.Length)
		{
			await Logger.ErrorLine($"Missing value for for parameter: {Arguments[Index]}.");
			return false;
		}

		Index++;
		parameterAction(Arguments[Index]);
			

		return true;
	}

	public Task<bool> ProcessOptionalTextParameterValue(Action<string> parameterAction, string expectedValue = null, bool ignoreCase = false)
	{
		if (Index + 1 == Arguments.Length ||
		    Arguments[Index + 1].StartsWith("-") ||
		    (expectedValue != null && !Arguments[Index + 1].Equals(expectedValue, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.InvariantCulture)))
			return Task.FromResult(true);

		Index++;
		parameterAction(Arguments[Index]);

		return Task.FromResult(true);
	}

	public async Task<bool> ProcessUInt16ParameterValue(Action<ushort> parameterAction)
	{
		string value = null;
		string parameterName = Arguments[Index];

		if (!await ProcessTextParameterValue(v => value = v))
			return false;

		if (!ushort.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out ushort ushortValue))
		{
			await Logger.ErrorLine($"The value \"{value}\" is not a valid for parameter: {parameterName}.");
			return false;
		}

		parameterAction(ushortValue);
			
		return true;
	}

	public async Task<bool> ProcessEnumParameterValue<TEnum>(Action<TEnum> parameterAction) where TEnum : struct, Enum
	{
		string value = null;
		string parameterName = Arguments[Index];

		if (!await ProcessTextParameterValue(v => value = v))
			return false;

		if (!Enum.TryParse(value, true, out TEnum enumValue))
		{
			await Logger.ErrorLine($"Value '{value}' is not valid for parameter: {parameterName}. Valid values: {string.Join(", ", Enum.GetValues<TEnum>())}");
			return false;
		}

		parameterAction(enumValue);
			
		return true;
	}

	public async Task<bool> ProcessNumberOfProcessors(Action<int?> parameterAction) 
	{
		string value = null;
		string parameterName = Arguments[Index];
			
		if (!await ProcessTextParameterValue(v => value = v))
			return false;

		int? numberOfProcessors = GetNumberOfProcessorsFromTextValue(value);

		if (!numberOfProcessors.HasValue)
		{
			await Logger.ErrorLine($"Value '{value}' is not valid for parameter: {parameterName}.");
			return false;
		}

		parameterAction(numberOfProcessors);

		return true;
	}

	internal async Task<bool> ProcessMaxMemory(Action<long?> parameterAction)
	{
		string value = null;
		string parameterName = Arguments[Index];
			
		if (!await ProcessTextParameterValue(v => value = v))
			return false;

		long? maxMemory = await GetMaxMemoryFromTextValue(value);

		if (!maxMemory.HasValue)
		{
			await Logger.ErrorLine($"Value '{value}' is not valid for parameter: {parameterName}.");
			return false;
		}

		parameterAction(maxMemory);

		return true;
	}

	internal int? GetNumberOfProcessorsFromTextValue(string text)
	{
		if (text == null)
			throw new ArgumentNullException(nameof(text));

		Match match = Regex.Match(text, @"^\s*(?<minus>-)?(?<value>\d+)(?<percent>%)?\s*$", RegexOptions.Singleline);

		if (!match.Success)
			return null;

		int value = int.Parse(match.Groups["value"].Value);
		bool negate = match.Groups["minus"].Success;

		int relativeValue;
		if (match.Groups["percent"].Success)
		{
			relativeValue = (int)Math.Floor(value / 100d * Environment.ProcessorCount);
			if (negate)
				relativeValue = Environment.ProcessorCount - relativeValue;
		}
		else
		{
			relativeValue = negate ? Environment.ProcessorCount - value : value;
		}

		return Math.Min(Environment.ProcessorCount, Math.Max(1, relativeValue));
	}

	public async Task<long?> GetMaxMemoryFromTextValue(string text)
	{
		if (text == null)
			throw new ArgumentNullException(nameof(text));

		long availableMemory = await MemoryInfo.GetAvailableMemoryInBytes(Logger);

		Match match = Regex.Match(text, @"^\s*(?<minus>-)?(?<value>\d+)((?<percent>%)|(?<gigabyte>GB?)|(?<megabyte>MB?)|(?<kilobyte>KB?))?\s*$", RegexOptions.Singleline | RegexOptions.IgnoreCase);

		if (!match.Success)
			return null;

		long value = long.Parse(match.Groups["value"].Value);
		bool negate = match.Groups["minus"].Success;

		long relativeValue;
		if (match.Groups["percent"].Success)
		{
			relativeValue = (long)Math.Floor(value / 100d * availableMemory);
			if (negate)
				relativeValue = availableMemory - relativeValue;
		}
		else
		{
			if (match.Groups["gigabyte"].Success)
				value *= (1024 * 1024 * 1024);
			else if (match.Groups["megabyte"].Success)
				value *= (1024 * 1024);
			else if (match.Groups["kilobyte"].Success)
				value *= 1024;
			relativeValue = negate ? availableMemory - value : value;
		}

		return Math.Min(availableMemory, Math.Max(1, relativeValue));
	}
}