using System.IO;
using System.Text.Json;

namespace MemoryLingo.Infrastructure.Settings;

public interface IWindowSettingsStore
{
	WindowSettings Get();
	void Save(double top, double left, double height, double width);
}

public class WindowSettingsStore : IWindowSettingsStore
{
	readonly string _filePath;

	public WindowSettingsStore()
	{
		_filePath = Path.Combine(DefaultFilesOptions.AppFolder, "windowSettings.json");
	}

	public WindowSettings Get()
	{
		if (!File.Exists(_filePath))
			return new WindowSettings();

		var json = File.ReadAllText(_filePath);
		return JsonSerializer.Deserialize<WindowSettings>(json)
			?? new WindowSettings();
	}

	public void Save(double top, double left, double height, double width)
	{
		Directory.CreateDirectory(DefaultFilesOptions.AppFolder);
		var settings = new WindowSettings
		{
			Top = top,
			Left = left,
			Height = height,
			Width = width
		};
		var json = JsonSerializer.Serialize(settings, DefaultFilesOptions.SerializerOptions);
		File.WriteAllText(_filePath, json);
	}
}
