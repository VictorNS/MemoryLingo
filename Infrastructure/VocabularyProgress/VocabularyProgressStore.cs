using System.IO;
using System.Text.Json;

namespace MemoryLingo.Infrastructure.VocabularyProgress;

public interface IVocabularyProgressStore
{
	VocabularyProgressDto Load(string filePath);
	void Save(string filePath, VocabularyProgressDto vocabularyProgress);
}

public class VocabularyProgressStore : IVocabularyProgressStore
{
	public VocabularyProgressDto Load(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return new VocabularyProgressDto();

		var progressFilePath = Path.ChangeExtension(filePath, ".progress.json");

		if (!File.Exists(progressFilePath))
			return new VocabularyProgressDto();

		return JsonSerializer.Deserialize<VocabularyProgressDto>(File.ReadAllText(progressFilePath), DefaultFilesOptions.SerializerOptions)
			?? new VocabularyProgressDto();
	}

	public void Save(string filePath, VocabularyProgressDto vocabularyProgress)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return;

		var folderPath = Path.GetDirectoryName(filePath);
		if (folderPath == null)
			return;

		Directory.CreateDirectory(folderPath);
		var progressFilePath = Path.ChangeExtension(filePath, ".progress.json");
		var json = JsonSerializer.Serialize(vocabularyProgress, DefaultFilesOptions.SerializerOptions);
		File.WriteAllText(progressFilePath, json);
	}
}
