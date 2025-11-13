using MemoryLingo.Models;

namespace MemoryLingo.Services;

public class VocabularyService
{
	private readonly FileReader _fileReader;
	Vocabulary? _vocabulary;

	public VocabularyService(FileReader fileReader)
	{
		_fileReader = fileReader;
	}

	public Vocabulary CreateEmptyVocabulary()
	{
		_vocabulary = new Vocabulary
		{
			FileName = "In Memory",
			FilePath = string.Empty,
			Entries = [new VocabularyEntry
			{
				RuText = "Слоёное тесто",
				RuTip = "тесто с маслом",
				EnText = "Puff pastry",
				RuExample = "Я могу сделать пиццу на слоеном тесте.",
				EnExample = "I can make a puff pastry pizza.",
			}],
			Status = "Fake Vocabulary"
		};

		return _vocabulary;
	}

	public Vocabulary LoadVocabulary(string filePath)
	{
		_vocabulary = _fileReader.ReadDataFromFile(filePath);

		return _vocabulary;
	}
}
