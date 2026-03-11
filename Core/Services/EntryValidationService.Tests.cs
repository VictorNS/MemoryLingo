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
		var expected = actual[0];
		Assert.Equal("Hello", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal("world", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
	}

	[Fact]
	public void SplitIntoWords_WithSpacesAround_ReturnsWordsWithoutPunctuation()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoWords("  Hello    world  ");

		// Assert
		var expected = actual[0];
		Assert.Equal("Hello", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal("world", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
	}

	[Fact]
	public void SplitIntoWords_WithPunctuation_IncludesPunctuationAsTokens()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoWords("Hello, world!");

		// Assert
		var expected = actual[0];
		Assert.Equal("Hello", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal(",", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[2];
		Assert.Equal("world", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[3];
		Assert.Equal("!", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
	}

	[Fact]
	public void SplitIntoWords_WithLeadingDash_IncludesDashAsFirstToken()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoWords("-Hello, world!");

		// Assert
		var expected = actual[0];
		Assert.Equal("-", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal("Hello", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[2];
		Assert.Equal(",", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[3];
		Assert.Equal("world", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[4];
		Assert.Equal("!", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
	}

	[Fact]
	public void SplitIntoWords_WithLeadingDash_IncludesDashes()
	{
		// Arrange

		// Act
		var actual = EntryValidationService.SplitIntoWords(" -Bla blu! -Gla glu!");

		// Assert
		var expected = actual[0];
		Assert.Equal("-", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[1];
		Assert.Equal("Bla", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[2];
		Assert.Equal("blu", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[3];
		Assert.Equal("!", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[4];
		Assert.Equal("-", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[5];
		Assert.Equal("Gla", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
		expected = actual[6];
		Assert.Equal("glu", expected.Text);
		Assert.Equal(WordCheckResultType.Word, expected.EntryType);
		Assert.True(expected.IsSpaceBefore);
		expected = actual[7];
		Assert.Equal("!", expected.Text);
		Assert.Equal(WordCheckResultType.Punctuation, expected.EntryType);
		Assert.False(expected.IsSpaceBefore);
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
		Assert.Equal(2, actual.Count);
		Assert.Equal("Hello", actual[0].Text);
		Assert.True(actual[0].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[0].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[0].EntryType);
		Assert.True(actual[0].IsWord);
		Assert.Equal("world", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[1].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[1].EntryType);
		Assert.True(actual[1].IsWord);
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
		Assert.Equal(4, actual.Count);
		Assert.Equal("Hello", actual[0].Text);
		Assert.True(actual[0].IsMatch);
		Assert.Equal(",", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.Equal("world", actual[2].Text);
		Assert.True(actual[2].IsMatch);
		Assert.Equal("!", actual[3].Text);
		Assert.True(actual[3].IsMatch);
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
		Assert.Equal(4, actual.Count);
		Assert.Equal("Hello", actual[0].Text);
		Assert.True(actual[0].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[0].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[0].EntryType);
		Assert.True(actual[0].IsWord);
		Assert.Equal(",", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.Equal(WordCheckResultType.Punctuation, actual[1].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[1].EntryType);
		Assert.False(actual[1].IsWord);
		Assert.Equal("world", actual[2].Text);
		Assert.True(actual[2].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[2].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[2].EntryType);
		Assert.True(actual[2].IsWord);
		Assert.Equal("!", actual[3].Text);
		Assert.True(actual[3].IsMatch);
		Assert.Equal(WordCheckResultType.Punctuation, actual[3].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[3].EntryType);
		Assert.False(actual[3].IsWord);
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
		Assert.Equal(2, actual.Count);
		Assert.Equal("hello", actual[0].Text);
		Assert.True(actual[0].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[0].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[0].EntryType);
		Assert.True(actual[0].IsWord);
		Assert.Equal("world", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[1].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[1].EntryType);
		Assert.True(actual[1].IsWord);
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
		Assert.Equal(3, actual.Count);
		Assert.Equal("hello", actual[0].Text);
		Assert.True(actual[0].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[0].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[0].EntryType);
		Assert.True(actual[0].IsWord);
		Assert.Equal("world", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[1].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[1].EntryType);
		Assert.True(actual[1].IsWord);
		Assert.Equal("(some tip)", actual[2].Text);
		Assert.True(actual[2].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[2].EntryType);
		Assert.Equal(WordCheckResultType.Tip, actual[2].EntryType);
		Assert.False(actual[2].IsWord);
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
		Assert.Equal(6, actual.Count);
		Assert.Equal("(start)", actual[0].Text);
		Assert.True(actual[0].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[0].EntryType);
		Assert.Equal(WordCheckResultType.Tip, actual[0].EntryType);
		Assert.False(actual[0].IsWord);
		Assert.Equal("Hello", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[1].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[1].EntryType);
		Assert.True(actual[1].IsWord);
		Assert.Equal(",", actual[2].Text);
		Assert.True(actual[2].IsMatch);
		Assert.Equal(WordCheckResultType.Punctuation, actual[2].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[2].EntryType);
		Assert.False(actual[2].IsWord);
		Assert.Equal("world", actual[3].Text);
		Assert.True(actual[3].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[3].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[3].EntryType);
		Assert.True(actual[3].IsWord);
		Assert.Equal("!", actual[4].Text);
		Assert.True(actual[4].IsMatch);
		Assert.Equal(WordCheckResultType.Punctuation, actual[4].EntryType);
		Assert.NotEqual(WordCheckResultType.Tip, actual[4].EntryType);
		Assert.False(actual[4].IsWord);
		Assert.Equal("(end)", actual[5].Text);
		Assert.True(actual[5].IsMatch);
		Assert.NotEqual(WordCheckResultType.Punctuation, actual[5].EntryType);
		Assert.Equal(WordCheckResultType.Tip, actual[5].EntryType);
		Assert.False(actual[5].IsWord);
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
}
