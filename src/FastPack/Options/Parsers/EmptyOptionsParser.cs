using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FastPack.Lib.Logging;
using FastPack.Lib.Options;

namespace FastPack.Options.Parsers;

internal abstract class EmptyOptionsParser : IOptionsParser
{
	protected ILogger Logger { get; }

	protected EmptyOptionsParser(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[ExcludeFromCodeCoverage]
	async Task<IOptions> IOptionsParser.CreateFromArgs(string[] args, bool strictMode, ILogger logger)
	{
		return await CreateFromArgs(args, strictMode, logger);
	}

	public abstract Task PrintHelp();

	[ExcludeFromCodeCoverage]
	public async Task<EmptyOptions> CreateFromArgs(string[] args, bool strictMode, ILogger logger)
	{
		return await Task.FromResult(new EmptyOptions());
	}
}