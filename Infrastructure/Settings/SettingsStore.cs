using System.IO;
using System.Text.Json;

namespace MemoryLingo.Infrastructure.Settings;

public interface ISettingsStore
{
	SettingsDto Get();
	void Save(SettingsDto settings);
}

public class SettingsStore : ISettingsStore
{
	readonly string _filePath;
	SettingsDto? _settings;

	public SettingsStore()
	{
		_filePath = Path.Combine(DefaultFilesOptions.AppFolder, "settings.json");
	}

	public SettingsDto Get()
	{
		if (_settings == null)
		{
			if (File.Exists(_filePath))
			{
				_settings = JsonSerializer.Deserialize<SettingsDto>(File.ReadAllText(_filePath), DefaultFilesOptions.SerializerOptions)
					?? new SettingsDto();
			}
			else
			{
				_settings = new SettingsDto();
			}
		}

		return _settings;
	}

	public void Save(SettingsDto settings)
	{
		_settings = settings;
		Directory.CreateDirectory(DefaultFilesOptions.AppFolder);
		var json = JsonSerializer.Serialize(_settings, DefaultFilesOptions.SerializerOptions);
		File.WriteAllText(_filePath, json);
	}
}
