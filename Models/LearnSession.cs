namespace MemoryLingo.Models;

public class LearnSession
{
	public required int SessionIndex { get; set; }
	public required int EntryIndex { get; set; }
	public required Dictionary<string, VocabularyProgressEntry> Entries { get; set; }
	public required List<string> Exercise { get; set; }
	public required int TotalEntriesCount { get; set; }
	public required int StudiedEntriesCount { get; set; }
}
