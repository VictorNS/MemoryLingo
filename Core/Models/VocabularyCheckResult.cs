namespace MemoryLingo.Core.Models;

public class VocabularyCheckResult
{
	public required string FileName { get; set; }
	public required string FilePath { get; set; }
    public required string ErrorMessage { get; set; }
    public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage);
}
