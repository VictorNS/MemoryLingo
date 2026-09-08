using MemoryLingo.Infrastructure.VocabularyProgress;

namespace MemoryLingo.Core.Models;

public class LearnSession
{
	public required int VocabularyLearnedCount { get; set; }
	public required int VocabularyEntriesCount { get; set; }
	public required Dictionary<string, VocabularyProgressEntry> Entries { get; set; }
	public required int SessionIndex { get; set; }
	public required int QueueIndex { get; set; }
	public required int LessonsCompletedCount { get; set; }
	public required int TotalEntriesCompletedCount { get; set; }
	public required List<EntryProgress> Queue { get; set; }
}
