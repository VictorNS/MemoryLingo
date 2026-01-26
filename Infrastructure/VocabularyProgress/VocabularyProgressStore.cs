using System.IO;
using System.Text.Json;

namespace MemoryLingo.Infrastructure.VocabularyProgress;

public interface IVocabularyProgressStore
{
	VocabularyProgressDto Load(string filePath);
	void Save(string filePath, int sessionIndex, VocabularyProgressDto vocabularyProgress);
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

		var vocabularyProgress = JsonSerializer.Deserialize<VocabularyProgressDto>(File.ReadAllText(progressFilePath), DefaultFilesOptions.SerializerOptions)
			?? new VocabularyProgressDto();

		vocabularyProgress.EnsureValid();

		return vocabularyProgress;
	}

	public void Save(string filePath, int sessionIndex, VocabularyProgressDto vocabularyProgress)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return;

		var folderPath = Path.GetDirectoryName(filePath);
		if (folderPath == null)
			return;

		Directory.CreateDirectory(folderPath);
		var progressFilePath = Path.ChangeExtension(filePath, ".progress.json");

		vocabularyProgress.EnsureValid();

		for (int i = 0; i < 3; i++)
		{
			var progress = vocabularyProgress.GetSessionProgress(i);
			var session = vocabularyProgress.Sessions[i];
			session.LearnedEntries = progress.LearnedEntries;
			session.TotalEntries = progress.TotalEntries;

			if (i == sessionIndex)
				session.LastUpdated = DateTime.UtcNow;
		}

		var json = JsonSerializer.Serialize(vocabularyProgress, DefaultFilesOptions.SerializerOptions);
		File.WriteAllText(progressFilePath, json);
		/*
using System.IO.Compression;
...
using var fileStream = File.OpenRead(progressFilePath);
using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
using var reader = new StreamReader(gzipStream);
var jsonContent = reader.ReadToEnd();
var vocabularyProgress = JsonSerializer.Deserialize<VocabularyProgressDto>(jsonContent, DefaultFilesOptions.SerializerOptions);
...
using var fileStream = File.Create(progressFilePath);
using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
using var writer = new StreamWriter(gzipStream);
writer.Write(json);
		*/
	}
}
