using MemoryLingo.Infrastructure.VocabularyReference;

namespace MemoryLingo.Presentation.Commands;

public class SessionClickParameter
{
	public required VocabularyReferenceModel VocabularyFile { get; set; }
	public required int SessionIndex { get; set; }
}
