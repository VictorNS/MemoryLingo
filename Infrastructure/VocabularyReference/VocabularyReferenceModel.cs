using System.IO;

namespace MemoryLingo.Infrastructure.VocabularyReference;

public class VocabularyReferenceModel
{
	public required string FilePath { get; set; } = string.Empty;
	public required List<VocabularyReferenceSession> Sessions { get; set; } = [];
	public required string FileName { get; set; } = string.Empty;
	public required string ErrorMessage { get; set; } = string.Empty;

	public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage);
	public string Session1 => Sessions[0].LastUpdated is null ? "-/-" : $"{Sessions[0].LearnedEntries}/{Sessions[0].TotalEntries}";
	public string Session2 => Sessions[1].LastUpdated is null ? "-/-" : $"{Sessions[1].LearnedEntries}/{Sessions[1].TotalEntries}";
	public string Session3 => Sessions[2].LastUpdated is null ? "-/-" : $"{Sessions[2].LearnedEntries}/{Sessions[2].TotalEntries}";
	public DateTime? Session1LocalTime => Sessions[0].LastUpdated?.ToLocalTime();
	public DateTime? Session2LocalTime => Sessions[1].LastUpdated?.ToLocalTime();
	public DateTime? Session3LocalTime => Sessions[2].LastUpdated?.ToLocalTime();
	public DateTime? LastSessionLocalTime => Sessions.Max(s => s.LastUpdated)?.ToLocalTime();
	public string Session1AverageAttempts => Sessions[0].AverageAttempts.GetValueOrDefault() == 0
		? ""
		: Sessions[0].AverageAttempts.GetValueOrDefault().ToString("0.##");
	public string Session2AverageAttempts => Sessions[1].AverageAttempts.GetValueOrDefault() == 0
		? ""
		: Sessions[1].AverageAttempts.GetValueOrDefault().ToString("0.##");
	public string Session3AverageAttempts => Sessions[2].AverageAttempts.GetValueOrDefault() == 0
		? ""
		: Sessions[2].AverageAttempts.GetValueOrDefault().ToString("0.##");
	public bool Session2Outdated => (Sessions[0].LastUpdated ?? DateTime.MinValue) > (Sessions[1].LastUpdated ?? DateTime.MinValue);
	public bool Session3Outdated => (Sessions[1].LastUpdated ?? DateTime.MinValue) > (Sessions[2].LastUpdated ?? DateTime.MinValue);

	public static VocabularyReferenceModel Empty(string filePath)
	{
		return new VocabularyReferenceModel
		{
			FilePath = filePath,
			FileName = Path.GetFileName(filePath),
			ErrorMessage = string.Empty,
			Sessions = [new(), new(), new()]
		};
	}
	
	public static VocabularyReferenceModel FromData(VocabularyReferenceDto data)
	{
		data.EnsureValid();

		return new VocabularyReferenceModel
		{
			FilePath = data.FilePath,
			FileName = Path.GetFileName(data.FilePath),
			Sessions = data.Sessions,
			ErrorMessage = data.ErrorMessage
		};
	}

	public static IReadOnlyList<VocabularyReferenceModel> FromDataList(IReadOnlyList<VocabularyReferenceDto> dataList)
	{
		return [.. dataList.Select(FromData)];
	}

	public static IReadOnlyList<VocabularyReferenceDto> ToDataList(IReadOnlyList<VocabularyReferenceModel> models)
	{
		return [.. models.Select(m => m.ToData())];
	}

	public VocabularyReferenceDto ToData()
	{
		return new VocabularyReferenceDto
		{
			FilePath = FilePath,
			Sessions = Sessions,
			ErrorMessage = ErrorMessage
		};
	}
}
