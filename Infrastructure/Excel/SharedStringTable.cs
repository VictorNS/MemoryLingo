using System.Xml;
using System.Xml.Serialization;

namespace MemoryLingo.Infrastructure.Excel;

/// <summary>
/// (c) 2014 Vienna, Dietmar Schoder
/// https://www.codeproject.com/Tips/801032/Csharp-How-To-Read-xlsx-Excel-File-With-Lines-of
/// Code Project Open License (CPOL) 1.02
/// Handles a "shared strings XML-file" in an Excel xlsx-file
/// </summary>
[Serializable()]
[XmlType(Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
[XmlRoot("sst", Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main")]
public class SharedStringTable
{
	[XmlAttribute("uniqueCount")]
	public string UniqueCount = string.Empty;
	[XmlAttribute("count")]
	public string Count = string.Empty;
	[XmlElement("si")]
	public SharedString[] SharedStrings = [];
}

public class SharedString
{
	[XmlElement("t")]
	public string Text = string.Empty;
}
