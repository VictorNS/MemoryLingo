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
	VocabularyProgress _vocabularyProgress;
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

		_vocabularyProgress = LoadProgress();
		var result = SynchronizeProgressWithVocabulary();

		if (!string.IsNullOrEmpty(result))
		{
			_vocabulary = _vocabularyService.CreateEmptyVocabulary();
			_vocabulary.Status = result;
			SynchronizeProgressWithVocabulary();
		}

		SaveProgress();
		LoadSession();

		return _vocabulary;
	}

	public string SynchronizeProgressWithVocabulary()
	{
		// check for duplicate RuText entries
		var duplicates = _vocabulary.Entries
			.GroupBy(e => e.RuText)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		if (duplicates.Count > 0)
			return $"Duplicates: {string.Join(", ", duplicates)}";

		// remove progress entries for removed vocabulary entries
		var notExistingEntries = _vocabularyProgress.Entries.Keys.Except(_vocabulary.Entries.Select(e => e.RuText)).ToList();
		foreach (var key in notExistingEntries)
			_vocabularyProgress.Entries.Remove(key);

		// add progress entries for new vocabulary entries
		foreach (var entry in _vocabulary.Entries)
		{
			if (!_vocabularyProgress.Entries.ContainsKey(entry.RuText))
			{
				var newEntry = new VocabularyProgressEntry();

				for (int i = 0; i <= _vocabularyProgress.SessionIndex; i++)
					newEntry.Sessions.Add(new VocabularyProgressEntrySession());

				_vocabularyProgress.Entries[entry.RuText] = newEntry;
			}
		}

		return "";
	}

	public void LoadSession()
	{
		int currentSession = 0;
		Dictionary<string, VocabularyProgressEntry> restEntries = [];

		for (int i = 0; i <= _vocabularyProgress.SessionIndex; i++)
		{
			restEntries = _vocabularyProgress.Entries
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
			Exercise = [.. restEntries.Take(_settings.Learn.ExerciseSize).Select(kv => kv.Key)],
			StudiedEntriesCount = 0,
			TotalEntriesCount = _vocabularyProgress.Entries.Count,
		};
	}

	public EntryProgress GetFirstEntry()
	{
		return GetCurrentEntry();
	}

	public EntryProgress GetNextEntry()
	{
		if (_session.EntryIndex + 1 >= _session.Exercise.Count)
			return EntryProgress.Empty;

		_session.EntryIndex++;
		return GetCurrentEntry();
	}

	private EntryProgress GetCurrentEntry()
	{
		var ruText = _session.Exercise[_session.EntryIndex];
		var entry = _vocabulary.Entries.First(e => e.RuText == ruText);
		var progress = _session.Entries[ruText].Sessions[_session.SessionIndex];

		return new EntryProgress
		{
			Entry = entry,
			IsLearned = progress.IsLearned,
			CorrectAnswers = progress.CorrectAnswers,
			TotalAttempts = progress.TotalAttempts,
			Session = new()
			{
				TotalEntriesCount = _session.TotalEntriesCount,
				SessionEntriesCount = _session.Entries.Count,
				StudiedEntriesCount = _session.StudiedEntriesCount
			}
		};
	}

	public EntryProgress SaveEntryProgress(string ruText, bool isCorrect)
	{
		var entry = _vocabulary.Entries.FirstOrDefault(e => e.RuText == ruText);
		if (entry == null)
		{
			throw new InvalidDataException($"Entry with RuText '{ruText}' not found in vocabulary.");
		}
		var progress = _session.Entries[ruText].Sessions[_session.SessionIndex];
		progress.TotalAttempts++;

		if (isCorrect)
		{
			progress.CorrectAnswers++;

			if (progress.CorrectAnswers >= _settings.Learn.CorrectAnswersToLearn)
				progress.IsLearned = true;
		}

		SaveProgress();

		return new EntryProgress
		{
			Entry = entry,
			IsLearned = progress.IsLearned,
			IsLastAttemptSuccess = isCorrect,
			CorrectAnswers = progress.CorrectAnswers,
			TotalAttempts = progress.TotalAttempts,
			Session = new()
			{
				TotalEntriesCount = _session.TotalEntriesCount,
				SessionEntriesCount = _session.Entries.Count,
				StudiedEntriesCount = _session.StudiedEntriesCount
			}
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
		var json = JsonSerializer.Serialize(_vocabularyProgress, _serializerOptions);
		File.WriteAllText(progressFilePath, json);
	}
}
