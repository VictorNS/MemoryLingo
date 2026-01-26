namespace MemoryLingo.Core.Models;

public record TokenCheckResult(string Text, bool IsMatch, TokenCheckResultType TokenType)
{
	public bool NoSpaceBefore => TokenType == TokenCheckResultType.Punctuation;
	public bool IsWord => TokenType == TokenCheckResultType.Word;
}

public enum TokenCheckResultType
{
	Word,
	Punctuation,
	Tip
}
