using System.Text.Json.Serialization;

namespace MemoryLingo.Models;

public class VocabularyFile
{
	public required string FilePath { get; set; }
	public required string FileName { get; set; }
	public string ErrorMessage { get; set; } = string.Empty;
	public List<VocabularyFileSession> Sessions { get; set; } = [];
	public int SessionIndex { get; set; }
	[JsonIgnore]
	public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage);
	[JsonIgnore]
	public string Session1 => Sessions.Count > 0 ? $"{Sessions[0].LearnedEntries}/{Sessions[0].TotalEntries}" : "-/-";
	[JsonIgnore]
	public string Session2 => Sessions.Count > 1 ? $"{Sessions[1].LearnedEntries}/{Sessions[1].TotalEntries}" : "-/-";
	[JsonIgnore]
	public string Session3 => Sessions.Count > 2 ? $"{Sessions[2].LearnedEntries}/{Sessions[2].TotalEntries}" : "-/-";
	[JsonIgnore]
	public DateTime? LastSessionLocalTime =>
		Sessions.Count == 0
		? null
		: Sessions.Max(s => s.SessionDate)?.ToLocalTime();
}

public class VocabularyFileSession
{
	public DateTime? SessionDate { get; set; }
	public int LearnedEntries { get; set; }
	public int TotalEntries { get; set; }
}
