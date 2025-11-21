using Xunit;

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
		Assert.Equal(2, actual.Count);
		Assert.Equal("Hello", actual[0].Text);
		Assert.True(actual[0].IsMatch);
		Assert.False(actual[0].IsPunctuation);
		Assert.False(actual[0].IsTip);
		Assert.False(actual[0].IsNonWord);
		Assert.Equal("world", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.False(actual[1].IsPunctuation);
		Assert.False(actual[1].IsTip);
		Assert.False(actual[1].IsNonWord);
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
		Assert.False(actual[0].IsPunctuation);
		Assert.False(actual[0].IsTip);
		Assert.False(actual[0].IsNonWord);
		Assert.Equal(",", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.True(actual[1].IsPunctuation);
		Assert.False(actual[1].IsTip);
		Assert.True(actual[1].IsNonWord);
		Assert.Equal("world", actual[2].Text);
		Assert.True(actual[2].IsMatch);
		Assert.False(actual[2].IsPunctuation);
		Assert.False(actual[2].IsTip);
		Assert.False(actual[2].IsNonWord);
		Assert.Equal("!", actual[3].Text);
		Assert.True(actual[3].IsMatch);
		Assert.True(actual[3].IsPunctuation);
		Assert.False(actual[3].IsTip);
		Assert.True(actual[3].IsNonWord);
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
		Assert.False(actual[0].IsPunctuation);
		Assert.False(actual[0].IsTip);
		Assert.False(actual[0].IsNonWord);
		Assert.Equal("world", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.False(actual[1].IsPunctuation);
		Assert.False(actual[1].IsTip);
		Assert.False(actual[1].IsNonWord);
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
		Assert.False(actual[0].IsPunctuation);
		Assert.False(actual[0].IsTip);
		Assert.False(actual[0].IsNonWord);
		Assert.Equal("world", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.False(actual[1].IsPunctuation);
		Assert.False(actual[1].IsTip);
		Assert.False(actual[1].IsNonWord);
		Assert.Equal("(some tip)", actual[2].Text);
		Assert.True(actual[2].IsMatch);
		Assert.False(actual[2].IsPunctuation);
		Assert.True(actual[2].IsTip);
		Assert.True(actual[2].IsNonWord);
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
		Assert.False(actual[0].IsPunctuation);
		Assert.True(actual[0].IsTip);
		Assert.True(actual[0].IsNonWord);
		Assert.Equal("Hello", actual[1].Text);
		Assert.True(actual[1].IsMatch);
		Assert.False(actual[1].IsPunctuation);
		Assert.False(actual[1].IsTip);
		Assert.False(actual[1].IsNonWord);
		Assert.Equal(",", actual[2].Text);
		Assert.True(actual[2].IsMatch);
		Assert.True(actual[2].IsPunctuation);
		Assert.False(actual[2].IsTip);
		Assert.True(actual[2].IsNonWord);
		Assert.Equal("world", actual[3].Text);
		Assert.True(actual[3].IsMatch);
		Assert.False(actual[3].IsPunctuation);
		Assert.False(actual[3].IsTip);
		Assert.False(actual[3].IsNonWord);
		Assert.Equal("!", actual[4].Text);
		Assert.True(actual[4].IsMatch);
		Assert.True(actual[4].IsPunctuation);
		Assert.False(actual[4].IsTip);
		Assert.True(actual[4].IsNonWord);
		Assert.Equal("(end)", actual[5].Text);
		Assert.True(actual[5].IsMatch);
		Assert.False(actual[5].IsPunctuation);
		Assert.True(actual[5].IsTip);
		Assert.True(actual[5].IsNonWord);
	}
}
