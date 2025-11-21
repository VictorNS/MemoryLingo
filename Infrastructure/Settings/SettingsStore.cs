using System.IO;
using System.Text.Json;

namespace MemoryLingo.Infrastructure.Settings;

public interface ISettingsStore
{
	SettingsDto Load();
	void Save(SettingsDto settings);
}

public class SettingsStore : ISettingsStore
{
	readonly string _filePath;

	public SettingsStore()
	{
		_filePath = Path.Combine(DefaultFilesOptions.AppFolder, "settings.json");
	}

	public SettingsDto Load()
	{
		if (!File.Exists(_filePath))
			return new SettingsDto();

		return JsonSerializer.Deserialize<SettingsDto>(File.ReadAllText(_filePath), DefaultFilesOptions.SerializerOptions)
			?? new SettingsDto();
	}

	public void Save(SettingsDto settings)
	{
		Directory.CreateDirectory(DefaultFilesOptions.AppFolder);
		var json = JsonSerializer.Serialize(settings, DefaultFilesOptions.SerializerOptions);
		File.WriteAllText(_filePath, json);
	}
}
