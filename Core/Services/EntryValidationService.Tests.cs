using Xunit;
using MemoryLingo.Core.Models;

namespace MemoryLingo.Core.Services;

public class EntryValidationServiceTests
{
	[Fact]
	public void SplitIntoTokens_WithSpacesOnly_ReturnsTokensWithoutPunctuation()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoTokens("Hello world");

		// Assert
		var expected = actual[0];
		Assert.Equal("Hello", expected.Text);
		Assert.True(expected.IsWord);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal("world", expected.Text);
		Assert.True(expected.IsWord);
		Assert.True(expected.IsSpaceBefore);
	}

	[Fact]
	public void SplitIntoTokens_WithSpacesAround_ReturnsTokensWithoutPunctuation()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoTokens("  Hello    world  ");

		// Assert
		var expected = actual[0];
		Assert.Equal("Hello", expected.Text);
		Assert.True(expected.IsWord);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal("world", expected.Text);
		Assert.True(expected.IsWord);
		Assert.True(expected.IsSpaceBefore);
	}

	[Fact]
	public void SplitIntoTokens_WithPunctuation_IncludesPunctuationAsTokens()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoTokens("Hello, world!");

		// Assert
		var expected = actual[0];
		Assert.Equal("Hello", expected.Text);
		Assert.True(expected.IsWord);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal(",", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[2];
		Assert.Equal("world", expected.Text);
		Assert.True(expected.IsWord);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[3];
		Assert.Equal("!", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
	}

	[Fact]
	public void SplitIntoTokens_WithLeadingDash_IncludesDashAsFirstToken()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoTokens("-Hello, world!");

		// Assert
		var expected = actual[0];
		Assert.Equal("-", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal("Hello", expected.Text);
		Assert.True(expected.IsWord);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[2];
		Assert.Equal(",", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[3];
		Assert.Equal("world", expected.Text);
		Assert.True(expected.IsWord);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[4];
		Assert.Equal("!", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
	}

	[Fact]
	public void SplitIntoTokens_WithLeadingDash_IncludesDashes()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoTokens(" -Bla blu! -Gla glu!");

		// Assert
		var expected = actual[0];
		Assert.Equal("-", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal("Bla", expected.Text);
		Assert.True(expected.IsWord);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[2];
		Assert.Equal("blu", expected.Text);
		Assert.True(expected.IsWord);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[3];
		Assert.Equal("!", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[4];
		Assert.Equal("-", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[5];
		Assert.Equal("Gla", expected.Text);
		Assert.True(expected.IsWord);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[6];
		Assert.Equal("glu", expected.Text);
		Assert.True(expected.IsWord);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[7];
		Assert.Equal("!", expected.Text);
		Assert.Equal(TokenCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
	}

	[Fact]
	public void GetEntryCheckResult_ReturnsWrong()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "Hello world";
		var input = "hi folks";

		// Act
		var actual = service.GetEntryCheckResult(input, expectedAnswer);
		var tokens = actual.Tokens;

		// Assert
		Assert.False(actual.IsCorrect);
		Assert.False(actual.IsSimilar);
		Assert.Equal(EntryCheckResultType.Wrong, actual.EntryType);
		Assert.Equal(2, tokens.Count);
		Assert.Equal("Hello", tokens[0].Text);
		Assert.False(tokens[0].IsMatch);
		Assert.False(tokens[0].IsPunctuation);
		Assert.False(tokens[0].IsTip);
		Assert.True(tokens[0].IsWord);
		Assert.Equal("world", tokens[1].Text);
		Assert.False(tokens[1].IsMatch);
		Assert.False(tokens[0].IsPunctuation);
		Assert.False(tokens[0].IsTip);
		Assert.True(tokens[1].IsWord);
	}

	[Fact]
	public void GetEntryCheckResult_ReturnsSimilar()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "Hello world";
		var input = "hi world";

		// Act
		var actual = service.GetEntryCheckResult(input, expectedAnswer);
		var tokens = actual.Tokens;

		// Assert
		Assert.False(actual.IsCorrect);
		Assert.True(actual.IsSimilar);
		Assert.Equal(2, tokens.Count);
		Assert.Equal("Hello", tokens[0].Text);
		Assert.False(tokens[0].IsMatch);
		Assert.False(tokens[0].IsPunctuation);
		Assert.False(tokens[0].IsTip);
		Assert.True(tokens[0].IsWord);
		Assert.Equal("world", tokens[1].Text);
		Assert.True(tokens[1].IsMatch);
		Assert.False(tokens[0].IsPunctuation);
		Assert.False(tokens[0].IsTip);
		Assert.True(tokens[1].IsWord);
	}

	[Fact]
	public void GetEntryCheckResult_CaseInsensitiveMatch_ReturnsCorrect()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "Hello world";
		var input = "hello world";

		// Act
		var actual = service.GetEntryCheckResult(input, expectedAnswer);
		var tokens = actual.Tokens;

		// Assert
		Assert.True(actual.IsCorrect);
		Assert.Equal(2, tokens.Count);
		Assert.Equal("Hello", tokens[0].Text);
		Assert.True(tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[0].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[0].EntryType);
		Assert.True(tokens[0].IsWord);
		Assert.Equal("world", tokens[1].Text);
		Assert.True(tokens[1].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[1].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[1].EntryType);
		Assert.True(tokens[1].IsWord);
	}

	[Fact]
	public void GetEntryCheckResult_MissingPunctuationInInput_ReturnsCorrect()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "Hello, world!";
		var input = "hello world";

		// Act
		var actual = service.GetEntryCheckResult(input, expectedAnswer);
		var tokens = actual.Tokens;

		// Assert
		Assert.True(actual.IsCorrect);
		Assert.Equal(4, tokens.Count);
		Assert.Equal("Hello", tokens[0].Text);
		Assert.True(tokens[0].IsMatch);
		Assert.Equal(",", tokens[1].Text);
		Assert.True(tokens[1].IsMatch);
		Assert.Equal("world", tokens[2].Text);
		Assert.True(tokens[2].IsMatch);
		Assert.Equal("!", tokens[3].Text);
		Assert.True(tokens[3].IsMatch);
	}

	[Fact]
	public void GetEntryCheckResult_DifferentPunctuation_ReturnsCorrect()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "Hello, world!";
		var input = "hello-world?";

		// Act
		var actual = service.GetEntryCheckResult(input, expectedAnswer);
		var tokens = actual.Tokens;

		// Assert
		Assert.True(actual.IsCorrect);
		Assert.Equal(4, tokens.Count);
		Assert.Equal("Hello", tokens[0].Text);
		Assert.True(tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[0].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[0].EntryType);
		Assert.True(tokens[0].IsWord);
		Assert.Equal(",", tokens[1].Text);
		Assert.True(tokens[1].IsMatch);
		Assert.Equal(TokenCheckResultType.Punctuation, tokens[1].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[1].EntryType);
		Assert.False(tokens[1].IsWord);
		Assert.Equal("world", tokens[2].Text);
		Assert.True(tokens[2].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[2].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[2].EntryType);
		Assert.True(tokens[2].IsWord);
		Assert.Equal("!", tokens[3].Text);
		Assert.True(tokens[3].IsMatch);
		Assert.Equal(TokenCheckResultType.Punctuation, tokens[3].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[3].EntryType);
		Assert.False(tokens[3].IsWord);
	}

	[Fact]
	public void GetEntryCheckResult_ExtraPunctuationInInput_ReturnsCorrect()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "hello world";
		var input = "hello-world?";

		// Act
		var actual = service.GetEntryCheckResult(input, expectedAnswer);
		var tokens = actual.Tokens;

		// Assert
		Assert.True(actual.IsCorrect);
		Assert.Equal(2, tokens.Count);
		Assert.Equal("hello", tokens[0].Text);
		Assert.True(tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[0].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[0].EntryType);
		Assert.True(tokens[0].IsWord);
		Assert.Equal("world", tokens[1].Text);
		Assert.True(tokens[1].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[1].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[1].EntryType);
		Assert.True(tokens[1].IsWord);
	}

	[Fact]
	public void GetEntryCheckResult_WithTipInParentheses_ReturnsCorrect()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "hello world (some tip)";
		var input = "hello world";

		// Act
		var actual = service.GetEntryCheckResult(input, expectedAnswer);
		var tokens = actual.Tokens;

		// Assert
		Assert.True(actual.IsCorrect);
		Assert.Equal(3, tokens.Count);
		Assert.Equal("hello", tokens[0].Text);
		Assert.True(tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[0].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[0].EntryType);
		Assert.True(tokens[0].IsWord);
		Assert.Equal("world", tokens[1].Text);
		Assert.True(tokens[1].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[1].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[1].EntryType);
		Assert.True(tokens[1].IsWord);
		Assert.Equal("(some tip)", tokens[2].Text);
		Assert.True(tokens[2].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[2].EntryType);
		Assert.Equal(TokenCheckResultType.Tip, tokens[2].EntryType);
		Assert.False(tokens[2].IsWord);
	}

	[Fact]
	public void GetEntryCheckResult_WithMultipleTipsAndPunctuation_ReturnsCorrect()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "(start) Hello, world! (end)";
		var input = "hello world";

		// Act
		var actual = service.GetEntryCheckResult(input, expectedAnswer);
		var tokens = actual.Tokens;

		// Assert
		Assert.True(actual.IsCorrect);
		Assert.Equal(6, tokens.Count);
		Assert.Equal("(start)", tokens[0].Text);
		Assert.True(tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[0].EntryType);
		Assert.Equal(TokenCheckResultType.Tip, tokens[0].EntryType);
		Assert.False(tokens[0].IsWord);
		Assert.Equal("Hello", tokens[1].Text);
		Assert.True(tokens[1].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[1].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[1].EntryType);
		Assert.True(tokens[1].IsWord);
		Assert.Equal(",", tokens[2].Text);
		Assert.True(tokens[2].IsMatch);
		Assert.Equal(TokenCheckResultType.Punctuation, tokens[2].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[2].EntryType);
		Assert.False(tokens[2].IsWord);
		Assert.Equal("world", tokens[3].Text);
		Assert.True(tokens[3].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[3].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[3].EntryType);
		Assert.True(tokens[3].IsWord);
		Assert.Equal("!", tokens[4].Text);
		Assert.True(tokens[4].IsMatch);
		Assert.Equal(TokenCheckResultType.Punctuation, tokens[4].EntryType);
		Assert.NotEqual(TokenCheckResultType.Tip, tokens[4].EntryType);
		Assert.False(tokens[4].IsWord);
		Assert.Equal("(end)", tokens[5].Text);
		Assert.True(tokens[5].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, tokens[5].EntryType);
		Assert.Equal(TokenCheckResultType.Tip, tokens[5].EntryType);
		Assert.False(tokens[5].IsWord);
	}

	[Fact]
	public void RemoveTextInBrackets_WithBracketsAtStartAndEnd_RemovesBothWithSpaces()
	{
		// Arrange
		var service = new EntryValidationService();
		var input = "(asd) fgh jkl (qwert).";

		// Act
		var actual = service.RemoveUnspeakableSymbols(input);

		// Assert
		Assert.Equal("fgh jkl.", actual);
	}

	[Fact]
	public void RemoveTextInBrackets_WithBracketsAtEnd_RemovesBracketsAndSpace()
	{
		// Arrange
		var service = new EntryValidationService();
		var input = "fgh jkl (qwert).";

		// Act
		var actual = service.RemoveUnspeakableSymbols(input);

		// Assert
		Assert.Equal("fgh jkl.", actual);
	}

	[Fact]
	public void RemoveTextInBrackets_WithBracketsAtStart_RemovesBracketsAndSpace()
	{
		// Arrange
		var service = new EntryValidationService();
		var input = "(asd) fgh jkl.";

		// Act
		var actual = service.RemoveUnspeakableSymbols(input);

		// Assert
		Assert.Equal("fgh jkl.", actual);
	}

	[Fact]
	public void RemoveTextInBrackets_WithBracketsNoSpaces_RemovesBracketsOnly()
	{
		// Arrange
		var service = new EntryValidationService();
		var input = "(asd)fgh jkl!(qwert)";

		// Act
		var actual = service.RemoveUnspeakableSymbols(input);

		// Assert
		Assert.Equal("fgh jkl!", actual);
	}
}
