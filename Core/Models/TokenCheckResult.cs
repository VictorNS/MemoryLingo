using System.Text;

namespace MemoryLingo.Core.Models;

public record TokenCheckResult(string Text, bool IsMatch, TokenCheckResultType EntryType, bool IsSpaceBefore)
{
	public bool IsPunctuation => EntryType == TokenCheckResultType.Punctuation;
	public bool IsTip => EntryType == TokenCheckResultType.Tip;
	public bool IsWord => EntryType == TokenCheckResultType.Word;

	public static TokenCheckResult Create(char c, bool isSpaceBefore) => BuildTokenCheckResult(c.ToString(), isSpaceBefore);
	public static TokenCheckResult Create(StringBuilder sb, bool isSpaceBefore) => BuildTokenCheckResult(sb.ToString(), isSpaceBefore);

	private static TokenCheckResult BuildTokenCheckResult(string text, bool isSpaceBefore)
	{
		var tokenType = TokenCheckResultType.Word;
		var isMatch = false;

		if (text.StartsWith('(') && text.EndsWith(')'))
		{
			tokenType = TokenCheckResultType.Tip;
			isMatch = true;
		}
		else if (CharIsPunctuation(text[0]))
		{
			tokenType = TokenCheckResultType.Punctuation;
			isMatch = true;
		}

		return new(text, isMatch, tokenType, isSpaceBefore);
	}

	public static bool CharIsPunctuation(char c)
	{
		return c == ',' || c == '.' || c == ';' || c == '!' || c == '?' || c == '-';
	}
}

public enum TokenCheckResultType
{
	Word,
	Punctuation,
	Tip
}
