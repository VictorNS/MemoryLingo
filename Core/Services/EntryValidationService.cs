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
		var inputWordsNoPunctuation = inputWords.Where(w => w.Length > 0 && !IsPunctuation(w[0]) && !IsTip(w)).ToList();
		int inputIndex = 0;

		for (int i = 0; i < expectedWords.Count; i++)
		{
			var expectedWord = expectedWords[i];
			bool isMatch;
			WordCheckResultType entryType;

			if (IsTip(expectedWord))
			{
				isMatch = true;
				entryType = WordCheckResultType.Tip;
			}
			else if (expectedWord.Length > 0 && IsPunctuation(expectedWord[0]))
			{
				isMatch = true;
				entryType = WordCheckResultType.Punctuation;
			}
			else
			{
				var inputWord = inputIndex < inputWordsNoPunctuation.Count ? inputWordsNoPunctuation[inputIndex] : "";
				isMatch = string.Equals(inputWord.Trim(), expectedWord.Trim(), StringComparison.OrdinalIgnoreCase);
				entryType = WordCheckResultType.Word;
				inputIndex++;
			}

			results.Add(new WordCheckResult(expectedWord, isMatch, entryType));
		}

		return results;
	}

	internal static List<string> SplitIntoWords(string text)
	{
		if (string.IsNullOrEmpty(text))
			return [];

		var words = new List<string>();
		var currentWord = new StringBuilder();
		var i = 0;

		while (i < text.Length)
		{
			char c = text[i];

			if (char.IsWhiteSpace(c))
			{
				if (currentWord.Length > 0)
				{
					words.Add(currentWord.ToString());
					currentWord.Clear();
				}
				i++;
			}
			else if (c == '(')
			{
				// Save current word if any
				if (currentWord.Length > 0)
				{
					words.Add(currentWord.ToString());
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

				words.Add(tipBuilder.ToString());
			}
			else if (IsPunctuation(c))
			{
				if (currentWord.Length > 0)
				{
					words.Add(currentWord.ToString());
					currentWord.Clear();
				}
				// Add punctuation as separate word
				words.Add(c.ToString());
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
			words.Add(currentWord.ToString());
		}

		return words;
	}

	static bool IsPunctuation(char c)
	{
		return c == ',' || c == '.' || c == ';' || c == '!' || c == '?' || c == '-';
	}

	static bool IsTip(string word)
	{
		return word.StartsWith('(') && word.EndsWith(')');
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
