using System;
using FastPack.Lib.Options;
using AwesomeAssertions;
using NUnit.Framework;

namespace FastPack.Tests.FastPack.Lib.Options
{
	[TestFixture]
	public class InvalidOptionExceptionTests
	{
		[Test]
		public void Ensure_Properties_Are_Set_By_Constructor_1()
		{
			// arrange
			const int exceptionCode = 12345;
			const string propertyName = "RandomPropertyName";
			const string message = "Random Message";

			// Act
			InvalidOptionException exception = new(message, propertyName, exceptionCode);

			exception.ExceptionCode.Should().Be(exceptionCode);
			exception.PropertyName.Should().Be(propertyName);
			exception.Message.Should().Be(message);
		}

		[Test]
		public void Ensure_Properties_Are_Set_By_Constructor_2()
		{
			// arrange
			const int exceptionCode = -1;
			const string propertyName = "RandomPropertyName";
			const string message = "Random Message";

			// Act
			InvalidOptionException exception = new(message, propertyName);

			exception.ExceptionCode.Should().Be(exceptionCode);
			exception.PropertyName.Should().Be(propertyName);
			exception.Message.Should().Be(message);
		}

		[Test]
		public void Ensure_Properties_Are_Set_By_Constructor_3()
		{
			// arrange
			const int exceptionCode = 12345;
			const string propertyName = "RandomPropertyName";
			const string message = "Random Message";
			Exception innerException = new Exception("I am an inner exception");

			// Act
			InvalidOptionException exception = new(message, innerException, propertyName, exceptionCode);

			exception.ExceptionCode.Should().Be(exceptionCode);
			exception.PropertyName.Should().Be(propertyName);
			exception.Message.Should().Be(message);
			exception.InnerException.Should().Be(innerException);
		}

		[Test]
		public void Ensure_Properties_Are_Set_By_Constructor_4()
		{
			// arrange
			const int exceptionCode = -1;
			const string propertyName = "RandomPropertyName";
			const string message = "Random Message";
			Exception innerException = new Exception("I am an inner exception");

			// Act
			InvalidOptionException exception = new(message, innerException, propertyName);

			exception.ExceptionCode.Should().Be(exceptionCode);
			exception.PropertyName.Should().Be(propertyName);
			exception.Message.Should().Be(message);
			exception.InnerException.Should().Be(innerException);
		}
	}
}
