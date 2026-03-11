using System.Text;
using MemoryLingo.Core.Models;

namespace MemoryLingo.Core.Services;

public class EntryValidationService
{
	public List<WordCheckResult> GetWordCheckResults(string input, string expectedAnswer)
	{
		var inputWords = SplitIntoWords(input);
		var expectedWords = SplitIntoWords(expectedAnswer);
		var results = new List<WordCheckResult>();

		// Filter out punctuation and tips from input words for comparison
		var inputWordsNoPunctuation = inputWords.Where(w => !w.IsPunctuation && !w.IsTip).ToList();
		int inputIndex = 0;

		for (int i = 0; i < expectedWords.Count; i++)
		{
			var expectedWord = expectedWords[i];
			bool isMatch;

			if (expectedWord.IsTip)
			{
				isMatch = true;
			}
			else if (expectedWord.IsPunctuation)
			{
				isMatch = true;
			}
			else
			{
				var inputText = inputIndex < inputWordsNoPunctuation.Count ? inputWordsNoPunctuation[inputIndex].Text : "";
				isMatch = string.Equals(inputText.Trim(), expectedWord.Text.Trim(), StringComparison.OrdinalIgnoreCase);
				inputIndex++;
			}

			results.Add(expectedWord with { IsMatch = isMatch });
		}

		return results;
	}

	internal static List<WordCheckResult> SplitIntoWords(string text)
	{
		if (string.IsNullOrEmpty(text))
			return [];

		var words = new List<WordCheckResult>();
		var currentWord = new StringBuilder();
		var i = 0;

		while (i < text.Length)
		{
			char c = text[i];

			if (char.IsWhiteSpace(c))
			{
				if (currentWord.Length > 0)
				{
					bool IsSpaceBefore = i - currentWord.Length > 0 && char.IsWhiteSpace(text[i - currentWord.Length - 1]);
					words.Add(WordCheckResult.Create(currentWord, IsSpaceBefore));
					currentWord.Clear();
				}
				i++;
			}
			else if (c == '(')
			{
				// Save current word if any
				if (currentWord.Length > 0)
				{
					bool IsSpaceBefore = i - currentWord.Length > 0 && char.IsWhiteSpace(text[i - currentWord.Length - 1]);
					words.Add(WordCheckResult.Create(currentWord, IsSpaceBefore));
					currentWord.Clear();
				}

				// Find matching closing parenthesis
				var tipBuilder = new StringBuilder();
				tipBuilder.Append(c); // Include opening parenthesis
				i++;

				while (i < text.Length && text[i] != ')')
				{
					tipBuilder.Append(text[i]);
					i++;
				}

				if (i < text.Length) // Include closing parenthesis if found
				{
					tipBuilder.Append(text[i]);
					i++;
				}

				bool IsSpaceBeforeTip = i - currentWord.Length > 0 && char.IsWhiteSpace(text[i - currentWord.Length - 1]);
				words.Add(WordCheckResult.Create(tipBuilder, IsSpaceBeforeTip));
			}
			else if (WordCheckResult.CharIsPunctuation(c))
			{
				if (currentWord.Length > 0)
				{
					bool IsSpaceBefore = i - currentWord.Length > 0 && char.IsWhiteSpace(text[i - currentWord.Length - 1]);
					words.Add(WordCheckResult.Create(currentWord, IsSpaceBefore));
					currentWord.Clear();
				}
				// Add punctuation as separate word
				bool isSpaceBeforePunctuation = i > 0 && char.IsWhiteSpace(text[i - 1]);
				words.Add(WordCheckResult.Create(c, isSpaceBeforePunctuation));
				i++;
			}
			else
			{
				currentWord.Append(c);
				i++;
			}
		}

		if (currentWord.Length > 0)
		{
			bool IsSpaceBefore = i - currentWord.Length > 0 && char.IsWhiteSpace(text[i - currentWord.Length - 1]);
			words.Add(WordCheckResult.Create(currentWord, IsSpaceBefore));
		}

		return words;
	}

	public string RemoveTextInBrackets(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return text;

		var result = new StringBuilder();
		int i = 0;

		while (i < text.Length)
		{
			if (text[i] == '(')
			{
				// Skip to the closing bracket
				while (i < text.Length && text[i] != ')')
				{
					i++;
				}
				if (i < text.Length) // Skip the closing bracket
				{
					i++;
				}

				// Remove trailing space after bracket if present
				if (i < text.Length && text[i] == ' ')
				{
					i++;
				}
			}
			else if (text[i] == ' ' && i + 1 < text.Length && text[i + 1] == '(')
			{
				// Skip space before opening bracket
				i++;
			}
			else
			{
				result.Append(text[i]);
				i++;
			}
		}

		return result.ToString().Trim();
	}
}
