using MemoryLingo.Infrastructure.VocabularyReference;

namespace MemoryLingo.Core.Services;

public interface IVocabularyReferenceService
{
	IReadOnlyList<VocabularyReferenceModel> Load();
	IReadOnlyList<VocabularyReferenceModel> GetVocabularyList();
	void AddAndSave(VocabularyReferenceModel file);
	void RemoveAndSave(string filePath);
	void Save();
	void UpdateSessionAndSave(string filePath, int sessionIndex, int learnedEntries, int totalEntries);
}

public class VocabularyReferenceService : IVocabularyReferenceService
{
	readonly IVocabularyReferenceStore _vocabularyReferenceStore;

	public VocabularyReferenceService(IVocabularyReferenceStore vocabularyReferenceStore)
	{
		_vocabularyReferenceStore = vocabularyReferenceStore;
	}

	public IReadOnlyList<VocabularyReferenceModel> Load()
	{
		var vocabularies = _vocabularyReferenceStore.Load();
		return VocabularyReferenceModel.FromDataList(vocabularies);
	}

	public IReadOnlyList<VocabularyReferenceModel> GetVocabularyList()
	{
		var vocabularies = _vocabularyReferenceStore.GetVocabularyList();
		return VocabularyReferenceModel.FromDataList(vocabularies);
	}

	public void Save()
	{
		_vocabularyReferenceStore.Save();
	}

	public void AddAndSave(VocabularyReferenceModel vocabulary)
	{
		_vocabularyReferenceStore.AddAndSave(vocabulary.ToData());
	}

	public void RemoveAndSave(string filePath)
	{
		_vocabularyReferenceStore.RemoveAndSave(filePath);
	}

	public void UpdateSessionAndSave(string filePath, int sessionIndex, int learnedEntries, int totalEntries)
	{
		_vocabularyReferenceStore.UpdateSessionAndSave(filePath, sessionIndex, learnedEntries, totalEntries);
	}
}
