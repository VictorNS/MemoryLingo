using MemoryLingo.Core.Models;

namespace MemoryLingo.Infrastructure.VocabularyExcel;

public class VocabularyExcelDto
{
	public required string FileName { get; set; }
	public required string FilePath { get; set; }
	public required List<Entry> Entries { get; set; }
	public required string ErrorMessage { get; set; }
	public string Lang
	{
		get
		{
			var dotIndex = FileName.IndexOf('.');

			if (dotIndex > 0)
				return FileName.Substring(0, dotIndex);

			return string.Empty;
		}
	}
}
