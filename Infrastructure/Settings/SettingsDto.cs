namespace MemoryLingo.Infrastructure.Settings;

public class SettingsDto
{
	public BehaviorSettings Behavior { get; set; } = new();
	public LearnSettings Learn { get; set; } = new();
	public Dictionary<string, SpeechLangSettings> Speech { get; set; } = [];

	public static SettingsDto Default => new()
	{
		Behavior = new BehaviorSettings
		{
			MinimizeToTray = true,
			RandomizeQueue = true
		},
		Learn = new LearnSettings
		{
			ExerciseSize = 17,
			CorrectAnswersToLearn = 2,
			DifficultEntriesSession2Percent = 30,
			DifficultEntriesSession3Percent = 50
		},
		Speech = new Dictionary<string, SpeechLangSettings>
		{
			{ "en", new SpeechLangSettings { IsActive = true, Voice = "Microsoft David Desktop", Rate = 0 } },
			{ "bg", new SpeechLangSettings { IsActive = true, Voice = "Microsoft Ivan", Rate = 0 } }
		}
	};
}

public class BehaviorSettings
{
	public bool MinimizeToTray { get; set; } = true;
	/// <summary>
	/// Randomize the order of entries in the session queue
	/// </summary>
	public bool RandomizeQueue { get; set; } = true;
}

public class LearnSettings
{
	/// <summary>
	/// Number of entries per exercise
	/// </summary>
	public int ExerciseSize { get; set; } = 17;
	/// <summary>
	/// Number of correct answers required to learn an entry
	/// </summary>
	public int CorrectAnswersToLearn { get; set; } = 2;
	/// <summary>
	/// Percentage of difficult entries we will repeat in a second session
	/// </summary>
	public int DifficultEntriesSession2Percent { get; set; } = 30;
	/// <summary>
	/// Percentage of difficult entries we will repeat in a third session
	/// </summary>
	public int DifficultEntriesSession3Percent { get; set; } = 50;
}

public class SpeechLangSettings
{
	public required string Voice { get; set; }
	public required int Rate { get; set; }
	public bool IsActive { get; set; }
}
