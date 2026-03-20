using System.IO;
using System.IO.Compression;
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

		VocabularyProgressDto vocabularyProgress;
		var progressZipPath = Path.ChangeExtension(filePath, ".progress.zip");

		if (File.Exists(progressZipPath))
		{
			using var fileStream = File.OpenRead(progressZipPath);
			using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
			using var reader = new StreamReader(zipArchive.Entries[0].Open());
			var jsonContent = reader.ReadToEnd();
			vocabularyProgress = JsonSerializer.Deserialize<VocabularyProgressDto>(jsonContent, DefaultFilesOptions.SerializerOptions)
				?? new VocabularyProgressDto();
		}
		else
		{
			var progressFilePath = Path.ChangeExtension(filePath, ".progress.json");

			if (!File.Exists(progressFilePath))
				return new VocabularyProgressDto();

			vocabularyProgress = JsonSerializer.Deserialize<VocabularyProgressDto>(File.ReadAllText(progressFilePath), DefaultFilesOptions.SerializerOptions)
				?? new VocabularyProgressDto();
		}

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

		Directory.CreateDirectory(folderPath);
		var json = JsonSerializer.Serialize(vocabularyProgress, DefaultFilesOptions.SerializerOptions);
		var progressZipPath = Path.ChangeExtension(filePath, ".progress.zip");
		var entryName = Path.GetFileNameWithoutExtension(filePath) + ".progress.json";
		using var fileStream = File.Create(progressZipPath);
		using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);
		using var writer = new StreamWriter(zipArchive.CreateEntry(entryName).Open());
		writer.Write(json);
	}
}
