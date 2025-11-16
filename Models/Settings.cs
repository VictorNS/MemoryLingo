namespace MemoryLingo.Models;

public class Settings
{
	public WindowSettings Window { get; set; } = new();
	public LearnSettings Learn { get; set; } = new();
}

public class WindowSettings
{
	public double Top { get; set; } = 16;
	public double Left { get; set; } = 16;
	public double Height { get; set; } = 240;
	public double Width { get; set; } = 600;
}

public class LearnSettings
{
	/// <summary>
	/// Number of entries per exercise
	/// </summary>
	public int ExerciseSize { get; set; } = 13;
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
	/// <summary>
	/// Delay in seconds before automatically advancing to the next entry
	/// </summary>
	public int NextEntryDelaySeconds { get; set; } = 2;
}
