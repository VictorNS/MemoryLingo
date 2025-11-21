namespace MemoryLingo.Infrastructure.VocabularyProgress;

public class VocabularyProgressDto
{
	public Dictionary<string, VocabularyProgressEntry> Entries { get; set; } = [];
	public int SessionIndex { get; set; }

	public (int LearnedEntries, int TotalEntries) GetSessionProgress(int sessionIndex)
	{
		if (sessionIndex < 0 || sessionIndex > 2)
			return (0, 0);

		int totalEntries = Entries.Values.Count(e => e.Sessions.Count > sessionIndex && !e.Sessions[sessionIndex].IsSkipped);

		if (totalEntries == 0)
			return (0, 0);

		int learnedEntries = Entries.Values.Count(e => e.Sessions.Count > sessionIndex && !e.Sessions[sessionIndex].IsSkipped && e.Sessions[sessionIndex].IsLearned);

		return (learnedEntries, totalEntries);
	}
}

public class VocabularyProgressEntry
{
	public List<VocabularyProgressEntrySession> Sessions { get; set; } = [];
}

public class VocabularyProgressEntrySession
{
	public bool IsSkipped { get; set; }
	public bool IsLearned { get; set; }
	public int CorrectAnswers { get; set; }
	public int TotalAttempts { get; set; }
}
