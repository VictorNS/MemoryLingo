using Xunit;
using MemoryLingo.Core.Models;

namespace MemoryLingo.Core.Services;

public class EntryValidationServiceTests
{
	[Fact]
	public void SplitIntoWords_WithSpacesOnly_ReturnsWordsWithoutPunctuation()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoWords("Hello world");

		// Assert
		var expected = new List<string> { "Hello", "world" };
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void SplitIntoWords_WithPunctuation_IncludesPunctuationAsTokens()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoWords("Hello, world!");

		// Assert
		var expected = new List<string> { "Hello", ",", "world", "!" };
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void GetWordCheckResults_CaseInsensitiveMatch_ReturnsAllWordsAsMatching()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "Hello world";
		var input = "hello world";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(2, actual.Tokens.Count);
		Assert.Equal("Hello", actual.Tokens[0].Text);
		Assert.True(actual.Tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[0].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[0].TokenType);
		Assert.True(actual.Tokens[0].IsWord);
		Assert.Equal("world", actual.Tokens[1].Text);
		Assert.True(actual.Tokens[1].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[1].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[1].TokenType);
		Assert.True(actual.Tokens[1].IsWord);
		Assert.Equal(EntryCheckResultType.Correct, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_MissingPunctuationInInput_ShowsExpectedTokensAsNonMatching()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "Hello, world!";
		var input = "hello world";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(4, actual.Tokens.Count);
		Assert.Equal("Hello", actual.Tokens[0].Text);
		Assert.True(actual.Tokens[0].IsMatch);
		Assert.Equal(",", actual.Tokens[1].Text);
		Assert.True(actual.Tokens[1].IsMatch);
		Assert.Equal("world", actual.Tokens[2].Text);
		Assert.True(actual.Tokens[2].IsMatch);
		Assert.Equal("!", actual.Tokens[3].Text);
		Assert.True(actual.Tokens[3].IsMatch);
		Assert.Equal(EntryCheckResultType.Correct, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_DifferentPunctuation_WordsMatchPunctuationAlwaysMatches()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "Hello, world!";
		var input = "hello-world?";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(4, actual.Tokens.Count);
		Assert.Equal("Hello", actual.Tokens[0].Text);
		Assert.True(actual.Tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[0].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[0].TokenType);
		Assert.True(actual.Tokens[0].IsWord);
		Assert.Equal(",", actual.Tokens[1].Text);
		Assert.True(actual.Tokens[1].IsMatch);
		Assert.Equal(TokenCheckResultType.Punctuation, actual.Tokens[1].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[1].TokenType);
		Assert.False(actual.Tokens[1].IsWord);
		Assert.Equal("world", actual.Tokens[2].Text);
		Assert.True(actual.Tokens[2].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[2].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[2].TokenType);
		Assert.True(actual.Tokens[2].IsWord);
		Assert.Equal("!", actual.Tokens[3].Text);
		Assert.True(actual.Tokens[3].IsMatch);
		Assert.Equal(TokenCheckResultType.Punctuation, actual.Tokens[3].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[3].TokenType);
		Assert.False(actual.Tokens[3].IsWord);
		Assert.Equal(EntryCheckResultType.Correct, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_ExtraPunctuationInInput_OnlyExpectedTokensReturned()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "hello world";
		var input = "hello-world?";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(2, actual.Tokens.Count);
		Assert.Equal("hello", actual.Tokens[0].Text);
		Assert.True(actual.Tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[0].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[0].TokenType);
		Assert.True(actual.Tokens[0].IsWord);
		Assert.Equal("world", actual.Tokens[1].Text);
		Assert.True(actual.Tokens[1].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[1].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[1].TokenType);
		Assert.True(actual.Tokens[1].IsWord);
		Assert.Equal(EntryCheckResultType.Correct, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_WithTipInParentheses_TipShouldBeNonWordAndMatch()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "hello world (some tip)";
		var input = "hello world";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert - This test will fail until service is updated to handle tips
		Assert.Equal(3, actual.Tokens.Count);
		Assert.Equal("hello", actual.Tokens[0].Text);
		Assert.True(actual.Tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[0].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[0].TokenType);
		Assert.True(actual.Tokens[0].IsWord);
		Assert.Equal("world", actual.Tokens[1].Text);
		Assert.True(actual.Tokens[1].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[1].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[1].TokenType);
		Assert.True(actual.Tokens[1].IsWord);
		Assert.Equal("(some tip)", actual.Tokens[2].Text);
		Assert.True(actual.Tokens[2].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[2].TokenType);
		Assert.Equal(TokenCheckResultType.Tip, actual.Tokens[2].TokenType);
		Assert.False(actual.Tokens[2].IsWord);
		Assert.Equal(EntryCheckResultType.Correct, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_WithMultipleTipsAndPunctuation_AllTipsAndPunctuationMatch()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "(start) Hello, world! (end)";
		var input = "hello world";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(6, actual.Tokens.Count);
		Assert.Equal("(start)", actual.Tokens[0].Text);
		Assert.True(actual.Tokens[0].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[0].TokenType);
		Assert.Equal(TokenCheckResultType.Tip, actual.Tokens[0].TokenType);
		Assert.False(actual.Tokens[0].IsWord);
		Assert.Equal("Hello", actual.Tokens[1].Text);
		Assert.True(actual.Tokens[1].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[1].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[1].TokenType);
		Assert.True(actual.Tokens[1].IsWord);
		Assert.Equal(",", actual.Tokens[2].Text);
		Assert.True(actual.Tokens[2].IsMatch);
		Assert.Equal(TokenCheckResultType.Punctuation, actual.Tokens[2].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[2].TokenType);
		Assert.False(actual.Tokens[2].IsWord);
		Assert.Equal("world", actual.Tokens[3].Text);
		Assert.True(actual.Tokens[3].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[3].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[3].TokenType);
		Assert.True(actual.Tokens[3].IsWord);
		Assert.Equal("!", actual.Tokens[4].Text);
		Assert.True(actual.Tokens[4].IsMatch);
		Assert.Equal(TokenCheckResultType.Punctuation, actual.Tokens[4].TokenType);
		Assert.NotEqual(TokenCheckResultType.Tip, actual.Tokens[4].TokenType);
		Assert.False(actual.Tokens[4].IsWord);
		Assert.Equal("(end)", actual.Tokens[5].Text);
		Assert.True(actual.Tokens[5].IsMatch);
		Assert.NotEqual(TokenCheckResultType.Punctuation, actual.Tokens[5].TokenType);
		Assert.Equal(TokenCheckResultType.Tip, actual.Tokens[5].TokenType);
		Assert.False(actual.Tokens[5].IsWord);
		Assert.Equal(EntryCheckResultType.Correct, actual.EntryType);
	}

	[Fact]
	public void RemoveTextInBrackets_WithBracketsAtStartAndEnd_RemovesBothWithSpaces()
	{
		// Arrange
		var service = new EntryValidationService();
		var input = "(asd) fgh jkl (qwert).";

		// Act
		var actual = service.RemoveTextInBrackets(input);

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
		var actual = service.RemoveTextInBrackets(input);

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
		var actual = service.RemoveTextInBrackets(input);

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
		var actual = service.RemoveTextInBrackets(input);

		// Assert
		Assert.Equal("fgh jkl!", actual);
	}

	[Fact]
	public void GetWordCheckResults_AllFiveWordsCorrect_ReturnsCorrect()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "(start) one, two three four five (end)";
		var input = "one, two three four five";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(EntryCheckResultType.Correct, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_FourWordsCorrect_ReturnsSimilar()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "(start) one, two three four five (end)";
		var input = "bla, two three four five";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(EntryCheckResultType.Similar, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_ThreeWordsCorrect_ReturnsSimilar()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "(start) one, two three four five (end)";
		var input = "bla, two three four";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(EntryCheckResultType.Similar, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_TwoWordsCorrect_ReturnsWrong()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "(start) one, two three four five (end)";
		var input = "bla, two three";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(EntryCheckResultType.Wrong, actual.EntryType);
	}

	[Fact]
	public void GetWordCheckResults_WordsReordered_ReturnsWrong()
	{
		// Arrange
		var service = new EntryValidationService();
		var expectedAnswer = "(start) one, two three four five (end)";
		var input = "five, one two three four";

		// Act
		var actual = service.GetWordCheckResults(input, expectedAnswer);

		// Assert
		Assert.Equal(EntryCheckResultType.Wrong, actual.EntryType);
	}
}
