using Xunit;

namespace MemoryLingo.Core.Services;

public class SpeechServiceTests
{
	[Fact]
	public void RemoveUnspeakableSymbols_WithBracketsAtStartAndEnd_RemovesBothWithSpaces()
	{
		// Arrange
		var service = new SpeechService();
		var input = "(asd) fgh jkl (qwert).";

		// Act
		var actual = service.RemoveUnspeakableSymbols(input);

		// Assert
		Assert.Equal("fgh jkl .", actual);
	}

	[Fact]
	public void RemoveUnspeakableSymbols_WithBracketsAtEnd_RemovesBracketsAndSpace()
	{
		// Arrange
		var service = new SpeechService();
		var input = "fgh jkl (qwert).";

		// Act
		var actual = service.RemoveUnspeakableSymbols(input);

		// Assert
		Assert.Equal("fgh jkl .", actual);
	}

	[Fact]
	public void RemoveUnspeakableSymbols_WithBracketsAtStart_RemovesBracketsAndSpace()
	{
		// Arrange
		var service = new SpeechService();
		var input = "(asd) fgh jkl.";

		// Act
		var actual = service.RemoveUnspeakableSymbols(input);

		// Assert
		Assert.Equal("fgh jkl.", actual);
	}

	[Fact]
	public void RemoveUnspeakableSymbols_WithBracketsNoSpaces_RemovesBracketsOnly()
	{
		// Arrange
		var service = new SpeechService();
		var input = "(asd)fgh jkl!(qwert)";

		// Act
		var actual = service.RemoveUnspeakableSymbols(input);

		// Assert
		Assert.Equal("fgh jkl!", actual);
	}

	[Fact]
	public void RemoveUnspeakableSymbols_RemovesBracketsAndSlashes()
	{
		var service = new SpeechService();

		var input = "Hello (tip) /world |my friend";
		var expected = "Hello world my friend";

		var result = service.RemoveUnspeakableSymbols(input);

		Assert.Equal(expected, result);
	}
}
