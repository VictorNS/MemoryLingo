using System.Text;

namespace MemoryLingo.Core.Models;

public record WordCheckResult(string Text, bool IsMatch, WordCheckResultType EntryType, bool IsSpaceBefore)
{
	public bool IsPunctuation => EntryType == WordCheckResultType.Punctuation;
	public bool IsTip => EntryType == WordCheckResultType.Tip;
	public bool IsWord => EntryType == WordCheckResultType.Word;

	public static WordCheckResult Create(char c, bool isSpaceBefore) => BuildWordCheckResult(c.ToString(), isSpaceBefore);
	public static WordCheckResult Create(StringBuilder sb, bool isSpaceBefore) => BuildWordCheckResult(sb.ToString(), isSpaceBefore);

	private static WordCheckResult BuildWordCheckResult(string text, bool isSpaceBefore)
	{
		var wordType = WordCheckResultType.Word;
		var isMatch = false;

		if (text.StartsWith('(') && text.EndsWith(')'))
		{
			wordType = WordCheckResultType.Tip;
			isMatch = true;
		}
		else if (CharIsPunctuation(text[0]))
		{
			wordType = WordCheckResultType.Punctuation;
			isMatch = true;
		}

		return new(text, isMatch, wordType, isSpaceBefore);
	}

	public static bool CharIsPunctuation(char c)
	{
		return c == ',' || c == '.' || c == ';' || c == '!' || c == '?' || c == '-';
	}
}

public enum WordCheckResultType
{
	Word,
	Punctuation,
	Tip
}
