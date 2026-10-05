namespace MemoryLingo.Core.Models;

public class EntryContainer
{
	public bool IsReady { get; }
	public EntryProgress Progress { get; }
	public EntryCurrentSession Session { get; }

	private EntryContainer(bool isReady, EntryProgress progress, EntryCurrentSession session)
	{
		IsReady = isReady;
		Progress = progress;
		Session = session;
	}

	public Entry Entry => Progress.Entry;
	public static EntryContainer Empty { get; } = new(
		isReady: false,
		progress: new()
		{
			Entry = new(),
			IsLearned = false,
			IsLastAttemptSuccess = false,
			CorrectAnswers = 0,
			TotalAttempts = 0,
		},
		session: new()
		{
			SessionIndex = 0,
			QueueIndex = 0,
			QueueCount = 0,
			VocabularyLearnedCount = 0,
			VocabularyEntriesCount = 0,
			TotalEntriesCompletedCount = 0,
		});

	public static EntryContainer Create(EntryProgress entryProgress, LearnSession session)
	{
		return new EntryContainer(
			isReady: true,
			progress: entryProgress,
			session: new EntryCurrentSession
			{
				SessionIndex = session.SessionIndex,
				QueueIndex = session.QueueIndex,
				QueueCount = session.Queue.Count,
				VocabularyLearnedCount = session.VocabularyLearnedCount,
				VocabularyEntriesCount = session.VocabularyEntriesCount,
				TotalEntriesCompletedCount = session.TotalEntriesCompletedCount,
			});
	}
}

public class EntryProgress
{
	public required Entry Entry { get; set; }
	public bool IsLearned { get; set; }
	public bool IsLastAttemptSuccess { get; set; }
	public required int CorrectAnswers { get; set; }
	public required int TotalAttempts { get; set; }
}

public class EntryCurrentSession
{
	public required int SessionIndex { get; set; }
	public required int QueueIndex { get; set; }
	public required int QueueCount { get; set; }
	public required int VocabularyLearnedCount { get; set; }
	public required int VocabularyEntriesCount { get; set; }
	public required int TotalEntriesCompletedCount { get; set; }
}
