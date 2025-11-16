using System.IO;
using System.Text.Json;
using MemoryLingo.Models;

namespace MemoryLingo.Infrastructure;

public class SettingsStore
{
	readonly string _filePath;

	public SettingsStore()
	{
		_filePath = Path.Combine(DefaultFilesOptions.AppFolder, "settings.json");
	}

	public Settings Load()
	{
		if (!File.Exists(_filePath))
			return new Settings();

		return JsonSerializer.Deserialize<Settings>(File.ReadAllText(_filePath), DefaultFilesOptions.SerializerOptions)
			?? new Settings();
	}

	public void Save(Settings settings)
	{
		Directory.CreateDirectory(DefaultFilesOptions.AppFolder);
		var json = JsonSerializer.Serialize(settings, DefaultFilesOptions.SerializerOptions);
		File.WriteAllText(_filePath, json);
	}
}
