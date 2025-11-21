using System.Xml.Serialization;

namespace MemoryLingo.Infrastructure.Excel;

/// <summary>
/// (c) 2014 Vienna, Dietmar Schoder
/// https://www.codeproject.com/Tips/801032/Csharp-How-To-Read-xlsx-Excel-File-With-Lines-of
/// Code Project Open License (CPOL) 1.02
/// Deals with an Excel worksheet in an xlsx-file
/// </summary>
[Serializable()]
[XmlType(Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
[XmlRoot("worksheet", Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
#pragma warning disable IDE1006 // Naming Styles
public class worksheet
#pragma warning restore IDE1006 // Naming Styles
{
	[XmlArray("sheetData")]
	[XmlArrayItem("row")]
	public Row[] Rows;
	[XmlIgnore]
	public int NumberOfColumns; // Total number of columns in this worksheet

	public static int MaxColumnIndex = 0; // Temporary variable for import

	public void ExpandRows()
	{
		foreach (var row in Rows)
			row.ExpandCells(NumberOfColumns);
	}
}
