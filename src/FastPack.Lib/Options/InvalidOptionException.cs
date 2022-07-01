using System;
using System.Diagnostics.CodeAnalysis;

namespace FastPack.Lib.Options;

internal class InvalidOptionException : Exception
{
	public int ExceptionCode { get; }
	public string PropertyName { get; }

	public InvalidOptionException(string message, string propertyName, int exceptionCode = -1) : base(message)
	{
		PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
		ExceptionCode = exceptionCode;
	}

	[ExcludeFromCodeCoverage]
	public InvalidOptionException(string message, Exception innerException, string propertyName, int exceptionCode = -1) : base(message, innerException)
	{
		PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
		ExceptionCode = exceptionCode;
	}
}