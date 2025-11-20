namespace MemoryLingo.Models;

public class EntryProgress
{
	public required Entry Entry { get; set; }
	public bool IsLearned { get; set; }
	public bool IsLastAttemptSuccess { get; set; }
	public required int CorrectAnswers { get; set; }
	public required int TotalAttempts { get; set; }
	public required CurrentSessionProgress Session { get; set; }

	public static EntryProgress Empty { get; } = new()
	{
		Entry = new(),
		IsLearned = false,
		IsLastAttemptSuccess = false,
		CorrectAnswers = 0,
		TotalAttempts = 0,
		Session = new()
		{
			QueueIndex = 0,
			QueueCount = 0,
			VocabularyLearnedCount = 0,
			VocabularyEntriesCount = 0,
		}
	};

	public class CurrentSessionProgress
	{
		public required int QueueIndex { get; set; }
		public required int QueueCount { get; set; }
		public required int VocabularyLearnedCount { get; set; }
		public required int VocabularyEntriesCount { get; set; }
	}
}
