namespace MemoryLingo.Core.Models;

public record EntryCheckResult(List<TokenCheckResult> Tokens, EntryCheckResultType EntryType)
{
	public bool IsCorrect => EntryType == EntryCheckResultType.Correct;
	public bool IsSimilar => EntryType == EntryCheckResultType.Similar;
}

public enum EntryCheckResultType
{
	Wrong,
	Correct,
	Similar
}
