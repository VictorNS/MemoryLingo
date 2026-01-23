using System.IO;
using System.Text.Json;
using MemoryLingo.Infrastructure.Settings;

namespace MemoryLingo.Infrastructure.VocabularyReference;

public interface IVocabularyReferenceStore
{
	IReadOnlyList<VocabularyReferenceDto> Load();
	IReadOnlyList<VocabularyReferenceDto> GetVocabularyList();
	void Save(IReadOnlyList<VocabularyReferenceDto> vocabularies);
	void AddAndSave(VocabularyReferenceDto file);
	void RemoveAndSave(string filePath);
	void UpdateSessionAndSave(string filePath, int sessionIndex, int learnedEntries, int totalEntries);
}

public class VocabularyReferenceStore : IVocabularyReferenceStore
{
	readonly string _filePath;
	List<VocabularyReferenceDto> _vocabularies = [];

	public VocabularyReferenceStore(ISettingsStore settingsService)
	{
		var settings = settingsService.Get();
		_filePath = string.IsNullOrWhiteSpace(settings.VocabularyListPath)
			? Path.Combine(DefaultFilesOptions.AppFolder, "vocabulary-list.json")
			: settings.VocabularyListPath;
	}

	public IReadOnlyList<VocabularyReferenceDto> Load()
	{
		if (File.Exists(_filePath))
		{
			_vocabularies = JsonSerializer.Deserialize<List<VocabularyReferenceDto>>(File.ReadAllText(_filePath), DefaultFilesOptions.SerializerOptions)
				?? [];
		}

		foreach (var vocabularyFile in _vocabularies)
		{
			vocabularyFile.EnsureValid();
		}

		return _vocabularies;
	}

	public IReadOnlyList<VocabularyReferenceDto> GetVocabularyList()
	{
		return _vocabularies;
	}

	public void AddAndSave(VocabularyReferenceDto file)
	{
		_vocabularies.Add(file);
		Save();
	}

	public void RemoveAndSave(string filePath)
	{
		var vocabulary = _vocabularies.FirstOrDefault(x => x.FilePath == filePath);

		if (vocabulary is null)
			return;

		_vocabularies.Remove(vocabulary);
		Save();
	}

	public void UpdateSessionAndSave(string filePath, int sessionIndex, int learnedEntries, int totalEntries)
	{
		var vocabulary = _vocabularies.FirstOrDefault(x => x.FilePath == filePath);

		if (vocabulary is null)
			return;

		vocabulary.EnsureValid();
		var session = vocabulary.Sessions[sessionIndex];
		session.LastUpdated = DateTime.UtcNow;
		session.LearnedEntries = learnedEntries;
		session.TotalEntries = totalEntries;

		Save();
	}

	public void Save(IReadOnlyList<VocabularyReferenceDto> vocabularies)
	{
		_vocabularies = vocabularies.ToList();
		Save();
	}

	public void Save()
	{
		Directory.CreateDirectory(DefaultFilesOptions.AppFolder);
		var json = JsonSerializer.Serialize(_vocabularies, DefaultFilesOptions.SerializerOptions);
		File.WriteAllText(_filePath, json);
	}
}
