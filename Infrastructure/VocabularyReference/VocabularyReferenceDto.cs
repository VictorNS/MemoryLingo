using System.Text.Json.Serialization;

namespace MemoryLingo.Infrastructure.VocabularyReference;

public class VocabularyReferenceDto
{
	public required string FilePath { get; set; }
	public List<VocabularyFileSession> Sessions { get; set; } = [];

	[JsonIgnore]
	public string FileName { get; set; } = string.Empty;
	[JsonIgnore]
	public string ErrorMessage { get; set; } = string.Empty;
	[JsonIgnore]
	public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage);
	[JsonIgnore]
	public string Session1 => $"{Sessions[0].LearnedEntries}/{Sessions[0].TotalEntries}";
	[JsonIgnore]
	public string Session2 => $"{Sessions[1].LearnedEntries}/{Sessions[1].TotalEntries}";
	[JsonIgnore]
	public string Session3 => $"{Sessions[2].LearnedEntries}/{Sessions[2].TotalEntries}";
	[JsonIgnore]
	public DateTime? LastSessionLocalTime => Sessions.Max(s => s.LastUpdated)?.ToLocalTime();

	public VocabularyReferenceDto()
	{
		EnsureValid();
	}

	public void EnsureValid()
	{
		// prevent null after desialization
		Sessions ??= [];

		for (int i = Sessions.Count; i < 3; i++)
			Sessions.Add(new());
	}
}

public class VocabularyFileSession
{
	public DateTime? LastUpdated { get; set; }
	public int LearnedEntries { get; set; }
	public int TotalEntries { get; set; }

	public void Update(DateTime? lastUpdated, int learnedEntries, int totalEntries)
	{
		LastUpdated = lastUpdated;
		LearnedEntries = learnedEntries;
		TotalEntries = totalEntries;
	}
}
