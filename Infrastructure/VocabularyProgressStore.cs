using System.IO;
using System.Text.Json;
using MemoryLingo.Models;

namespace MemoryLingo.Infrastructure;

public class VocabularyProgressStore
{
	public VocabularyProgress Load(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return new VocabularyProgress();

		var progressFilePath = Path.ChangeExtension(filePath, ".progress.json");

		if (!File.Exists(progressFilePath))
			return new VocabularyProgress();

		return JsonSerializer.Deserialize<VocabularyProgress>(File.ReadAllText(progressFilePath), DefaultFilesOptions.SerializerOptions)
			?? new VocabularyProgress();
	}

	public void Save(string filePath, VocabularyProgress vocabularyProgress)
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
