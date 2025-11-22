using System.IO;
using System.Text.Json;

namespace MemoryLingo.Infrastructure.VocabularyReference;

public interface IVocabularyReferenceStore
{
	void AddAndSave(VocabularyReferenceDto file);
	IReadOnlyList<VocabularyReferenceDto> GetVocabularyList();
	IReadOnlyList<VocabularyReferenceDto> Load();
	void RemoveAndSave(string filePath);
	void Save();
	void UpdateSessionAndSave(string filePath, int sessionIndex, int learnedEntries, int totalEntries);
}

public class VocabularyReferenceStore : IVocabularyReferenceStore
{
	readonly private string _filePath;
	private List<VocabularyReferenceDto> _vocabularies = [];

	public VocabularyReferenceStore()
	{
		_filePath = Path.Combine(DefaultFilesOptions.AppFolder, "vocabulary-list.json");
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
			vocabularyFile.FileName = Path.GetFileName(vocabularyFile.FilePath);
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

	public void Save()
	{
		Directory.CreateDirectory(DefaultFilesOptions.AppFolder);
		var json = JsonSerializer.Serialize(_vocabularies, DefaultFilesOptions.SerializerOptions);
		File.WriteAllText(_filePath, json);
	}
}
