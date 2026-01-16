namespace MemoryLingo.Core.Models;

public record WordCheckResult(string Text, bool IsMatch, WordCheckResultType EntryType)
{
	public bool IsPunctuation => EntryType == WordCheckResultType.Punctuation;
	public bool IsWord => EntryType == WordCheckResultType.Word;
}

public enum WordCheckResultType
{
	Word,
	Punctuation,
	Tip
}
