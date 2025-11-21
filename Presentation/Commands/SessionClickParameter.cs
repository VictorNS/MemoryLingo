using MemoryLingo.Infrastructure.VocabularyReference;

namespace MemoryLingo.Presentation.Commands;

public class SessionClickParameter
{
	public required VocabularyReferenceDto VocabularyFile { get; set; }
	public required int SessionIndex { get; set; }
}
