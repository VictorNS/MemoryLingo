namespace MemoryLingo.Models;

public class VocabularyEntryProgress
{
	public required VocabularyEntry Entry { get; set; }
	public bool IsLearned { get; set; }
	public required int CorrectAnswers { get; set; }
	public required int TotalAttempts { get; set; }
}
