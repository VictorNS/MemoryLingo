namespace MemoryLingo.Infrastructure.VocabularyReference;

public class VocabularyReferenceDto
{
	public required string FilePath { get; set; }
	public List<VocabularyReferenceSession> Sessions { get; set; } = [];

	public void EnsureValid()
	{
		// prevent null after deserialization
		Sessions ??= [];

		for (int i = Sessions.Count; i < 3; i++)
			Sessions.Add(new());
	}
}
