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
	private ReaderWriterLockSlim _cacheLock = new();
	private Dictionary<string, string> _cache = [];

	public VocabularyProgressDto Load(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return new VocabularyProgressDto();

		_cacheLock.EnterReadLock();
		try
		{
			if (_cache.TryGetValue(filePath, out var cachedProgress))
			{
				return JsonSerializer.Deserialize<VocabularyProgressDto>(cachedProgress, DefaultFilesOptions.SerializerOptions)
					?? new VocabularyProgressDto();
			}
		}
		finally
		{
			_cacheLock.ExitReadLock();
		}

		var progressZipPath = Path.ChangeExtension(filePath, ".progress.zip");

		if (!File.Exists(progressZipPath))
			return new VocabularyProgressDto();

		VocabularyProgressDto vocabularyProgress;
		_cacheLock.EnterWriteLock();
		try
		{
			using var fileStream = File.OpenRead(progressZipPath);
			using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
			using var reader = new StreamReader(zipArchive.Entries[0].Open());
			var json = reader.ReadToEnd();
			_cache[filePath] = json;
			vocabularyProgress = JsonSerializer.Deserialize<VocabularyProgressDto>(json, DefaultFilesOptions.SerializerOptions)
				?? new VocabularyProgressDto();
		}
		finally
		{
			_cacheLock.ExitWriteLock();
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

		var json = JsonSerializer.Serialize(vocabularyProgress, DefaultFilesOptions.SerializerOptions);
		if (json is null)
			return;

		Directory.CreateDirectory(folderPath);
		_cacheLock.EnterWriteLock();
		try
		{
			_cache[filePath] = json;
			var progressZipPath = Path.ChangeExtension(filePath, ".progress.zip");
			var entryName = Path.GetFileNameWithoutExtension(filePath) + ".progress.json";

			FileStream? fileStream = null;
			int attempt;
			for (attempt = 0; attempt < 10; attempt++)
			{
				try
				{
					fileStream = File.Create(progressZipPath);
					break;
				}
				catch (IOException)
				{
					Thread.Sleep(500);
				}
			}
			if (fileStream is null)
				return;
			if (attempt > 0)
				Console.WriteLine($"Warning: Had to retry creating progress file {progressZipPath} {attempt} times.");

			using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);
			using var writer = new StreamWriter(zipArchive.CreateEntry(entryName).Open());
			writer.Write(json);
		}
		finally
		{
			_cacheLock.ExitWriteLock();
		}
	}
}
