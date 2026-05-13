using System.IO;
using System.Text.Json;

namespace MemoryLingo.Infrastructure.Settings;

public interface IPathSettingsStore
{
	PathSettings Get();
}

public class PathSettingsStore : IPathSettingsStore
{
	readonly string _filePath;

	public PathSettingsStore()
	{
		_filePath = Path.Combine(DefaultFilesOptions.AppFolder, "pathSettings.json");
	}

	public PathSettings Get()
	{
		if (!File.Exists(_filePath))
			return new PathSettings { VocabularyListPath = DefaultFilesOptions.AppFolder };

		var json = File.ReadAllText(_filePath);
		return JsonSerializer.Deserialize<PathSettings>(json)
			?? new PathSettings();
	}
}
