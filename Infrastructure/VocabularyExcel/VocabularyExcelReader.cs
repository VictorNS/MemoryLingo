using System.IO;
using MemoryLingo.Core.Models;
using MemoryLingo.Infrastructure.Excel;

namespace MemoryLingo.Infrastructure.VocabularyExcel;

public interface IVocabularyExcelReader
{
	VocabularyExcelDto LoadVocabulary(string filePath);
}

public class VocabularyExcelReader : IVocabularyExcelReader
{
	VocabularyExcelDto? _vocabulary;

	public VocabularyExcelDto LoadVocabulary(string filePath)
	{
		_vocabulary = ReadDataFromFile(filePath);

		return _vocabulary;
	}

	private static VocabularyExcelDto ReadDataFromFile(string filePath)
	{
		if (!File.Exists(filePath))
			return new VocabularyExcelDto
			{
				FileName = Path.GetFileName(filePath),
				FilePath = filePath,
				Entries = [],
				ErrorMessage = "File not found"
			};


		var worksheet = Workbook.Worksheets(filePath).FirstOrDefault();

		if (worksheet == null)
			return new VocabularyExcelDto
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

		return new VocabularyExcelDto
		{
			FileName = Path.GetFileName(filePath),
			FilePath = filePath,
			Entries = entries,
			ErrorMessage = ""
		};
	}
}
