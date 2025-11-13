namespace MemoryLingo.Models;

public class Vocabulary
{
	public required string FileName { get; set; }
	public required string FilePath { get; set; }
	public required List<VocabularyEntry> Entries { get; set; }
	public required string Status { get; set; }
}
