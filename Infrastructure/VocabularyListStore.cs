using System.IO;
using System.Text.Json;
using MemoryLingo.Models;

namespace MemoryLingo.Infrastructure;

public class VocabularyListStore
{
	readonly private string _filePath;
	private List<VocabularyFile> _vocabularies = [];

	public VocabularyListStore()
	{
		_filePath = Path.Combine(DefaultFilesOptions.AppFolder, "vocabulary-list.json");
	}

	public IReadOnlyList<VocabularyFile> Load()
	{
		if (File.Exists(_filePath))
		{
			_vocabularies = JsonSerializer.Deserialize<List<VocabularyFile>>(File.ReadAllText(_filePath), DefaultFilesOptions.SerializerOptions)
				?? [];
		}

		return _vocabularies;
	}

	public IReadOnlyList<VocabularyFile> GetVocabularyList()
	{
		return _vocabularies;
	}

	public void AddAndSave(VocabularyFile file)
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

	public void AddErrorAndSave(string filePath, string ErrorMessage)
	{
		var vocabulary = _vocabularies.FirstOrDefault(x => x.FilePath == filePath);

		if (vocabulary is null)
			return;

		vocabulary.ErrorMessage = ErrorMessage;
		Save();
	}

	public void UpdateSessionAndSave(string filePath, int sessionIndex, int learnedEntries, int totalEntries)
	{
		var vocabulary = _vocabularies.FirstOrDefault(x => x.FilePath == filePath);

		if (vocabulary is null)
			return;

		if (vocabulary.Sessions.Count <= sessionIndex)
		{
			for (int i = vocabulary.Sessions.Count; i <= sessionIndex; i++)
				vocabulary.Sessions.Add(new VocabularyFileSession());
		}

		vocabulary.SessionIndex = sessionIndex;
		var session = vocabulary.Sessions[sessionIndex];
		session.SessionDate = DateTime.UtcNow;
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
