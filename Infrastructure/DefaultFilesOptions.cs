using System.IO;
using System.Text.Json;

namespace MemoryLingo.Infrastructure;

public static class DefaultFilesOptions
{
	readonly static string _appFolder;
	readonly static JsonSerializerOptions _serializerOptions;

	static DefaultFilesOptions()
	{
		_appFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"MemoryLingo");

		_serializerOptions = new()
		{
			WriteIndented = true,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
		};
	}

	public static string AppFolder => _appFolder;
	public static JsonSerializerOptions SerializerOptions => _serializerOptions;
}
