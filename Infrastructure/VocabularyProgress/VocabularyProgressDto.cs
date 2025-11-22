namespace MemoryLingo.Infrastructure.VocabularyProgress;

public class VocabularyProgressDto
{
	public List<VocabularyProgressSession> Sessions { get; set; } = [];
	public Dictionary<string, VocabularyProgressEntry> Entries { get; set; } = [];

	public VocabularyProgressDto()
	{
		EnsureValid();
	}

	public void EnsureValid()
	{
		// prevent null after desialization
		Entries ??= [];
		Sessions ??= [];

		for (int i = Sessions.Count; i < 3; i++)
			Sessions.Add(new());
	}

	public (int LearnedEntries, int TotalEntries) GetSessionProgress(int sessionIndex)
	{
		if (sessionIndex < 0 || sessionIndex > 2)
			return (0, 0);

		int totalEntries = Entries.Values.Count(e => !e.Sessions[sessionIndex].IsSkipped);

		if (totalEntries == 0)
			return (0, 0);

		int learnedEntries = Entries.Values.Count(e => !e.Sessions[sessionIndex].IsSkipped && e.Sessions[sessionIndex].IsLearned);

		return (learnedEntries, totalEntries);
	}
}

public class VocabularyProgressSession
{
	public DateTime? LastUpdated { get; set; }
	public int LearnedEntries { get; set; }
	public int TotalEntries { get; set; }
}

public class VocabularyProgressEntry
{
	public List<VocabularyProgressEntrySession> Sessions { get; set; } = [];

	public VocabularyProgressEntry()
	{
		for (int i = Sessions.Count; i < 3; i++)
			Sessions.Add(new());
	}
}

public class VocabularyProgressEntrySession
{
	public bool IsSkipped { get; set; }
	public bool IsLearned { get; set; }
	public int CorrectAnswers { get; set; }
	public int TotalAttempts { get; set; }

	public void Reset()
	{
		IsSkipped = false;
		IsLearned = false;
		CorrectAnswers = 0;
		TotalAttempts = 0;
	}
}
