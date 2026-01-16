namespace MemoryLingo.Infrastructure.VocabularyReference;

public class VocabularyReferenceSession
{
	public DateTime? LastUpdated { get; set; }
	public int LearnedEntries { get; set; }
	public int TotalEntries { get; set; }
	public decimal? AverageAttempts { get; set; }

	public void Update(DateTime? lastUpdated, int learnedEntries, int totalEntries, decimal? averageAttempts)
	{
		LastUpdated = lastUpdated;
		LearnedEntries = learnedEntries;
		TotalEntries = totalEntries;
		AverageAttempts = averageAttempts;
	}
}
