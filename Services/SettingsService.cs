using System.IO;
using System.Text.Json;
using MemoryLingo.Models;

namespace MemoryLingo.Services;

public class SettingsService
{
	readonly string _appFolder;
	readonly string _settingsPath;
	readonly JsonSerializerOptions _serializerOptions = new()
	{
		WriteIndented = true,
		Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
	};

	public SettingsService()
	{
		_appFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"MemoryLingo");
		_settingsPath = Path.Combine(_appFolder, "settings.json");
	}

	public Settings LoadSettings()
	{
		if (!File.Exists(_settingsPath))
			return new Settings();

		return JsonSerializer.Deserialize<Settings>(File.ReadAllText(_settingsPath), _serializerOptions)
			?? new Settings();
	}

	public void SaveSettings(Settings settings)
	{
		Directory.CreateDirectory(_appFolder);
		var json = JsonSerializer.Serialize(settings, _serializerOptions);
		File.WriteAllText(_settingsPath, json);
	}
}
