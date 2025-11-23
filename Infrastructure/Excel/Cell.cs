using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace MemoryLingo.Infrastructure.Excel;

/// <summary>
/// (c) 2014 Vienna, Dietmar Schoder
/// https://www.codeproject.com/Tips/801032/Csharp-How-To-Read-xlsx-Excel-File-With-Lines-of
/// Code Project Open License (CPOL) 1.02
/// Deals with an Excel cell
/// </summary>
public partial class Cell
{
	/// <summary>
	/// Used for converting from Excel column/row to column index starting at 0
	/// </summary>
	[XmlAttribute("r")]
	public string CellReference
	{
		get
		{
			return ColumnIndex.ToString();
		}
		set
		{
			ColumnIndex = GetColumnIndex(value);

			if (ColumnIndex > WorksheetData.MaxColumnIndex)
				WorksheetData.MaxColumnIndex = ColumnIndex;
		}
	}
	[XmlAttribute("t")]
	public string tType = "";
	/// <summary>
	/// Original value of the Excel cell
	/// </summary>
	[XmlElement("v")]
	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			if (tType.Equals("s"))
			{
				if (Workbook.SharedStringTable is null)
				{
					Text = string.Empty;
					return;
				}

				Text = Workbook.SharedStringTable.SharedStrings[Convert.ToInt32(_value)].Text;
				return;
			}
			if (tType.Equals("str"))
			{
				Text = _value;
				return;
			}
			try
			{
				Amount = Convert.ToDouble(_value, CultureInfo.InvariantCulture);
				Text = Amount.ToString("#,##0.##");
				IsAmount = true;
			}
			catch (Exception ex)
			{
				Amount = 0;
				Text = string.Format("Cell Value '{0}': {1}", _value, ex.Message);
			}
		}
	}
	/// <summary>
	/// Index of the orignal Excel cell column starting at 0
	/// </summary>
	[XmlIgnore]
	public int ColumnIndex;
	/// <summary>
	/// Text of the Excel cell (if it was a string)
	/// </summary>
	[XmlIgnore]
	public string Text = "";
	[XmlIgnore]
	/// <summary>
	/// Amount of the Excel cell (if it was a number)
	/// </summary>
	public double Amount;
	[XmlIgnore]
	public bool IsAmount;

	string _value = "";

	static int GetColumnIndex(string cellReference)
	{
		string colLetter = ColumnLetterRegex().Match(cellReference).Value.ToUpper();
		int colIndex = 0;

		for (int i = 0; i < colLetter.Length; i++)
		{
			colIndex *= 26;
			colIndex += colLetter[i] - 'A' + 1;
		}
		return colIndex - 1;
	}

	[GeneratedRegex("[A-Za-z]+")]
	private static partial Regex ColumnLetterRegex();
}
