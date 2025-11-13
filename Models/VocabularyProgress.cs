public class VocabularyProgress
{
	public Dictionary<string, VocabularyProgressEntry> Entries { get; set; } = [];
	public int SessionIndex { get; set; }
}

public class VocabularyProgressEntry
{
	public List<VocabularyProgressEntrySession> Sessions { get; set; } = [];
	public int SessionIndex { get; set; }
}

public class VocabularyProgressEntrySession
{
	public bool IsLearned { get; set; }
	public int CorrectAnswers { get; set; }
	public int TotalAttempts { get; set; }
}
