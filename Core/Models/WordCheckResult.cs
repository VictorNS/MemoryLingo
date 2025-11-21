namespace MemoryLingo.Core.Models;

public record WordCheckResult(string Text, bool IsMatch, bool IsPunctuation, bool IsTip)
{
	public bool IsNonWord => IsPunctuation || IsTip;
}
