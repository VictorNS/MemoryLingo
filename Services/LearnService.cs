using System.IO;
using System.Text.Json;
using MemoryLingo.Models;

namespace MemoryLingo.Services;

public class LearnService
{
	readonly SettingsService _settingsService;
	readonly VocabularyService _vocabularyService;
	Vocabulary _vocabulary;
	Settings _settings;
	VocabularyProgress _progress;
	LearnSession _session;
	readonly JsonSerializerOptions _serializerOptions = new()
	{
		WriteIndented = true,
		Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
	};

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public LearnService(SettingsService settingsService, VocabularyService vocabularyService)
	{
		_settingsService = settingsService;
		_vocabularyService = vocabularyService;
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	public Vocabulary LoadVocabulary(string? filePath)
	{
		_settings = _settingsService.LoadSettings();

		if (string.IsNullOrWhiteSpace(filePath))
			filePath = _settings.FilePath;

		_vocabulary = string.IsNullOrWhiteSpace(filePath)
			? _vocabularyService.CreateEmptyVocabulary()
			: _vocabularyService.LoadVocabulary(filePath);

		_progress = LoadProgress();
		SynchronizeProgressWithVocabulary();
		SaveProgress();
		LoadSession();

		return _vocabulary;
	}

	public void SynchronizeProgressWithVocabulary()
	{
		// remove progress entries for removed vocabulary entries
		var notExistingEntries = _progress.Entries.Keys.Except(_vocabulary.Entries.Select(e => e.RuText)).ToList();
		foreach (var key in notExistingEntries)
			_progress.Entries.Remove(key);

		// add progress entries for new vocabulary entries
		foreach (var entry in _vocabulary.Entries)
		{
			if (!_progress.Entries.ContainsKey(entry.RuText))
			{
				var newEntry = new VocabularyProgressEntry();

				for (int i = 0; i <= _progress.SessionIndex; i++)
					newEntry.Sessions.Add(new VocabularyProgressEntrySession());

				_progress.Entries[entry.RuText] = newEntry;
			}
		}
	}

	public void LoadSession()
	{
		int currentSession = 0;
		Dictionary<string, VocabularyProgressEntry> restEntries = [];

		for (int i = 0; i <= _progress.SessionIndex; i++)
		{
			restEntries = _progress.Entries
				.Where(kv => kv.Value.SessionIndex >= i && !kv.Value.Sessions[i].IsLearned)
				.ToDictionary(kv => kv.Key, kv => kv.Value);

			if (restEntries.Count > 0)
			{
				currentSession = i;
				break;
			}
		}

		_session = new LearnSession
		{
			SessionIndex = currentSession,
			EntryIndex = 0,
			Entries = restEntries,
			Exercise = [.. restEntries.Take(_settings.Learn.ExerciseSize).Select(kv => kv.Key)]
		};
	}

	public VocabularyEntryProgress GetFirstEntry()
	{
		var ruText = _session.Exercise[_session.EntryIndex];
		var entry = _vocabulary.Entries.First(e => e.RuText == ruText);
		var progress = _session.Entries[ruText].Sessions[_session.SessionIndex];

		return new VocabularyEntryProgress
		{
			Entry = entry,
			IsLearned = progress.IsLearned,
			CorrectAnswers = progress.CorrectAnswers,
			TotalAttempts = progress.TotalAttempts
		};
	}

	public VocabularyEntryProgress SaveEntryProgress(string ruText, bool isCorrect)
	{
		var entry = _vocabulary.Entries.First(e => e.RuText == ruText);
		var progress = _session.Entries[ruText].Sessions[_session.SessionIndex];
		progress.TotalAttempts++;

		if (isCorrect)
		{
			progress.CorrectAnswers++;
			
			if (progress.CorrectAnswers >= _settings.Learn.CorrectAnswersToLearn)
				progress.IsLearned = true;
		}

		SaveProgress();

		return new VocabularyEntryProgress
		{
			Entry = entry,
			IsLearned = progress.IsLearned,
			CorrectAnswers = progress.CorrectAnswers,
			TotalAttempts = progress.TotalAttempts
		};
	}

	public VocabularyProgress LoadProgress()
	{
		if (string.IsNullOrWhiteSpace(_vocabulary.FilePath))
			return new VocabularyProgress();

		var progressFilePath = Path.ChangeExtension(_vocabulary.FilePath, ".progress.json");

		if (!File.Exists(progressFilePath))
			return new VocabularyProgress();

		return JsonSerializer.Deserialize<VocabularyProgress>(File.ReadAllText(progressFilePath), _serializerOptions)
			?? new VocabularyProgress();
	}

	public void SaveProgress()
	{
		if (string.IsNullOrWhiteSpace(_vocabulary.FilePath))
			return;

		var folderPath = Path.GetDirectoryName(_vocabulary.FilePath);
		if (folderPath == null)
			return;

		Directory.CreateDirectory(folderPath);
		var progressFilePath = Path.ChangeExtension(_vocabulary.FilePath, ".progress.json");
		var json = JsonSerializer.Serialize(_progress, _serializerOptions);
		File.WriteAllText(progressFilePath, json);
	}
}
