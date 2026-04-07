using System.Text;
using MemoryLingo.Core.Models;

namespace MemoryLingo.Core.Services;

public class EntryValidationService
{
	public EntryCheckResult GetEntryCheckResult(string input, string expectedAnswer)
	{
		var inputTokens = SplitIntoTokens(input);
		var expectedTokens = SplitIntoTokens(expectedAnswer);
		var results = new List<TokenCheckResult>();

		// Filter out punctuation and tips from input tokens for comparison
		var inputTokensNoPunctuation = inputTokens.Where(w => !w.IsPunctuation && !w.IsTip).ToList();
		int inputIndex = 0;

		for (int i = 0; i < expectedTokens.Count; i++)
		{
			var expectedToken = expectedTokens[i];
			bool isMatch;

			if (expectedToken.IsTip)
			{
				isMatch = true;
			}
			else if (expectedToken.IsPunctuation)
			{
				isMatch = true;
			}
			else
			{
				var inputText = inputIndex < inputTokensNoPunctuation.Count ? inputTokensNoPunctuation[inputIndex].Text : "";
				isMatch = string.Equals(inputText.Trim(), expectedToken.Text.Trim(), StringComparison.OrdinalIgnoreCase);
				inputIndex++;
			}

			results.Add(expectedToken with { IsMatch = isMatch });
		}

		// Determine entry type based on token matches
		if (results.All(w => w.IsMatch))
			return new EntryCheckResult(results, EntryCheckResultType.Correct);

		if (results.Count(x => x.IsWord) == 1)
			return new EntryCheckResult(results, EntryCheckResultType.Wrong);

		var firstTokenMatch = results.First(x => x.IsWord).IsMatch;
		var halfOrMore = results.Count(x => x.IsMatch && x.IsWord) >= (results.Count(x => x.IsWord) / 2);
		var checkResultType = (firstTokenMatch || halfOrMore)
			? EntryCheckResultType.Similar : EntryCheckResultType.Wrong;
		return new EntryCheckResult(results, checkResultType);
	}

	internal static List<TokenCheckResult> SplitIntoTokens(string text)
	{
		if (string.IsNullOrEmpty(text))
			return [];

		var tokens = new List<TokenCheckResult>();
		var currentToken = new StringBuilder();
		var i = 0;

		while (i < text.Length)
		{
			char c = text[i];

			if (char.IsWhiteSpace(c))
			{
				if (currentToken.Length > 0)
				{
					bool IsSpaceBefore = i - currentToken.Length > 0 && char.IsWhiteSpace(text[i - currentToken.Length - 1]);
					tokens.Add(TokenCheckResult.Create(currentToken, IsSpaceBefore));
					currentToken.Clear();
				}
				i++;
			}
			else if (c == '(')
			{
				// Save current word if any
				if (currentToken.Length > 0)
				{
					bool IsSpaceBefore = i - currentToken.Length > 0 && char.IsWhiteSpace(text[i - currentToken.Length - 1]);
					tokens.Add(TokenCheckResult.Create(currentToken, IsSpaceBefore));
					currentToken.Clear();
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

				bool IsSpaceBeforeTip = i - currentToken.Length > 0 && char.IsWhiteSpace(text[i - currentToken.Length - 1]);
				tokens.Add(TokenCheckResult.Create(tipBuilder, IsSpaceBeforeTip));
			}
			else if (TokenCheckResult.CharIsPunctuation(c))
			{
				if (currentToken.Length > 0)
				{
					bool IsSpaceBefore = i - currentToken.Length > 0 && char.IsWhiteSpace(text[i - currentToken.Length - 1]);
					tokens.Add(TokenCheckResult.Create(currentToken, IsSpaceBefore));
					currentToken.Clear();
				}
				// Add punctuation as separate word
				bool isSpaceBeforePunctuation = i > 0 && char.IsWhiteSpace(text[i - 1]);
				tokens.Add(TokenCheckResult.Create(c, isSpaceBeforePunctuation));
				i++;
			}
			else
			{
				currentToken.Append(c);
				i++;
			}
		}

		if (currentToken.Length > 0)
		{
			bool IsSpaceBefore = i - currentToken.Length > 0 && char.IsWhiteSpace(text[i - currentToken.Length - 1]);
			tokens.Add(TokenCheckResult.Create(currentToken, IsSpaceBefore));
		}

		return tokens;
	}

	public string RemoveUnspeakableSymbols(string text)
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

		return result.ToString()
			.Replace("/", "").Replace("|", "")
			.Trim();
	}
}
