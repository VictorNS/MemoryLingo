using System.IO;
using MemoryLingo.Excel;
using MemoryLingo.Models;

namespace MemoryLingo.Infrastructure;

public class VocabularyStore
{
	Vocabulary? _vocabulary;

	public Vocabulary LoadVocabulary(string filePath)
	{
		_vocabulary = ReadDataFromFile(filePath);

		return _vocabulary;
	}

	private static Vocabulary ReadDataFromFile(string filePath)
	{
		if (!File.Exists(filePath))
			return new Vocabulary
			{
				FileName = Path.GetFileName(filePath),
				FilePath = filePath,
				Entries = [],
				ErrorMessage = "File not found"
			};


		var worksheet = Workbook.Worksheets(filePath).FirstOrDefault();

		if (worksheet == null)
			return new Vocabulary
			{
				FileName = Path.GetFileName(filePath),
				FilePath = filePath,
				Entries = [],
				ErrorMessage = "No worksheets found in the file"
			};

		var entries = new List<Entry>();

		foreach (var excelRow in worksheet.Rows)
		{
			entries.Add(new Entry
			{
				RuText = excelRow.GetText(0),
				RuTip = excelRow.GetText(1),
				Transcription = excelRow.GetText(2),
				EnText = excelRow.GetText(3),
				RuExample = excelRow.GetText(4),
				EnExample = excelRow.GetText(5)
			});
		}

		return new Vocabulary
		{
			FileName = Path.GetFileName(filePath),
			FilePath = filePath,
			Entries = entries,
			ErrorMessage = ""
		};
	}
}
