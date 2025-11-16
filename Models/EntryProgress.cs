namespace MemoryLingo.Models;

public class EntryProgress
{
	public required Entry Entry { get; set; }
	public bool IsLearned { get; set; }
	public bool IsLastAttemptSuccess { get; set; }
	public required int CorrectAnswers { get; set; }
	public required int TotalAttempts { get; set; }
	public required SessionProgress Session { get; set; }

	public static EntryProgress Empty { get; } = new()
	{
		Entry = new(),
		IsLearned = false,
		IsLastAttemptSuccess = false,
		CorrectAnswers = 0,
		TotalAttempts = 0,
		Session = new()
		{
			TotalEntriesCount = 0,
			SessionEntriesCount = 0,
			StudiedEntriesCount = 0
		}
	};

	public class SessionProgress
	{
		public required int TotalEntriesCount { get; set; }
		public required int SessionEntriesCount { get; set; }
		public required int StudiedEntriesCount { get; set; }
	}
}
