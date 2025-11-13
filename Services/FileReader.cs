using System.IO;
using MemoryLingo.Excel;
using MemoryLingo.Models;

namespace MemoryLingo.Services;

public class FileReader
{
	public Vocabulary ReadDataFromFile(string filePath)
	{
		if (!File.Exists(filePath))
			return new Vocabulary
			{
				FileName = Path.GetFileName(filePath),
				FilePath = filePath,
				Entries = [],
				Status = "File not found"
			};


		var worksheet = Workbook.Worksheets(filePath).FirstOrDefault();

		if (worksheet == null)
			return new Vocabulary
			{
				FileName = Path.GetFileName(filePath),
				FilePath = filePath,
				Entries = [],
				Status = "No worksheets found in the file"
			};

		var entries = new List<VocabularyEntry>();

		foreach (var excelRow in worksheet.Rows)
		{
			entries.Add(new VocabularyEntry
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
			Status = ""
		};
	}
}
