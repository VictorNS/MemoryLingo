using System.IO;
using MemoryLingo.Infrastructure;
using MemoryLingo.Models;

namespace MemoryLingo.Services;

public class LearnService
{
	readonly SettingsStore _settingsService;
	readonly VocabularyProgressStore _vocabularyProgressStore;
	readonly VocabularyListStore _vocabularyListService;
	readonly VocabularyStore _vocabularyService;
	readonly Settings _settings;
	Vocabulary _vocabulary;
	VocabularyProgress _vocabularyProgress;
	LearnSession _session;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public LearnService(SettingsStore settingsService, VocabularyProgressStore vocabularyProgressStore, VocabularyListStore vocabularyListService, VocabularyStore vocabularyService)
	{
		_settingsService = settingsService;
		_vocabularyProgressStore = vocabularyProgressStore;
		_vocabularyListService = vocabularyListService;
		_vocabularyService = vocabularyService;
		_settings = _settingsService.Load();
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	#region VocabularyList
	public IReadOnlyList<VocabularyFile> LoadVocabularyList()
	{
		var vocabularies = _vocabularyListService.Load();
		bool hasChanges = false;

		foreach (var vocabularyFile in vocabularies)
		{
			var checkResult = LoadAndCheckVocabularyFile(vocabularyFile.FilePath);

			if (checkResult.vocabularyFile.HasErrors)
			{
				hasChanges = true;
				vocabularyFile.ErrorMessage = checkResult.vocabularyFile.ErrorMessage;
			}
		}

		if (hasChanges)
			_vocabularyListService.Save();

		return vocabularies;
	}

	public VocabularyFile AddVocabularyFile(string filePath)
	{
		var checkResult = LoadAndCheckVocabularyFile(filePath);
		_vocabularyListService.AddAndSave(checkResult.vocabularyFile);
		return checkResult.vocabularyFile;
	}

	public void RemoveVocabularyFile(string filePath)
	{
		_vocabularyListService.RemoveAndSave(filePath);
	}

	private (Vocabulary vocabulary, VocabularyFile vocabularyFile) LoadAndCheckVocabularyFile(string filePath)
	{
		var vocabulary = _vocabularyService.LoadVocabulary(filePath);
		var vocabularyFile = new VocabularyFile
		{
			FileName = vocabulary.FileName,
			FilePath = vocabulary.FilePath,
		};

		if (!string.IsNullOrEmpty(vocabulary.ErrorMessage))
		{
			vocabularyFile.ErrorMessage = vocabulary.ErrorMessage;
			return (vocabulary, vocabularyFile);
		}

		if (vocabulary.Entries.Count == 0)
		{
			vocabularyFile.ErrorMessage = "Contains no entries.";
			return (vocabulary, vocabularyFile);
		}

		var existNotEmptyEntries = vocabulary.Entries.Any(x => !string.IsNullOrWhiteSpace(x.RuText) && !string.IsNullOrWhiteSpace(x.EnText));
		if (!existNotEmptyEntries)
		{
			vocabularyFile.ErrorMessage = "Contains no valid entries.";
			return (vocabulary, vocabularyFile);
		}

		var duplicates = vocabulary.Entries.GroupBy(x => x.RuText).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
		if (duplicates.Count > 0)
		{
			vocabularyFile.ErrorMessage = $"Duplicates: {string.Join(", ", duplicates)}";
			return (vocabulary, vocabularyFile);
		}

		return (vocabulary, vocabularyFile);
	}

	public IReadOnlyList<VocabularyFile> GetVocabularyList()
	{
		return _vocabularyListService.Load();
	}
	#endregion VocabularyList

	public Vocabulary? StartVocabularySession(string filePath, int? sessionNumber)
	{
		var checkResult = LoadAndCheckVocabularyFile(filePath);

		if (checkResult.vocabularyFile.HasErrors)
		{
			_vocabularyListService.AddErrorAndSave(checkResult.vocabularyFile.FilePath, checkResult.vocabularyFile.ErrorMessage);
			return null;
		}

		_vocabulary = checkResult.vocabulary;
		_vocabularyProgress = _vocabularyProgressStore.Load(_vocabulary.FilePath);
		SynchronizeProgressWithVocabulary();
		_vocabularyProgressStore.Save(_vocabulary.FilePath, _vocabularyProgress);

		if (!LoadSession(sessionNumber))
			return null;

		return _vocabulary;
	}

	public void SynchronizeProgressWithVocabulary()
	{
		// remove progress entries for removed vocabulary entries
		var notExistingEntries = _vocabularyProgress.Entries.Keys.Except(_vocabulary.Entries.Select(e => e.RuText)).ToList();
		foreach (var key in notExistingEntries)
			_vocabularyProgress.Entries.Remove(key);

		// add progress entries for vocabulary entries
		foreach (var entry in _vocabulary.Entries)
		{

			if (!_vocabularyProgress.Entries.TryGetValue(entry.RuText, out var progressEntry))
				_vocabularyProgress.Entries.Add(entry.RuText, progressEntry = new VocabularyProgressEntry());

			for (int i = progressEntry.Sessions.Count; i < 3; i++)
				progressEntry.Sessions.Add(new VocabularyProgressEntrySession());
		}
	}

	public bool LoadSession(int? sessionNumber)
	{
		var currentSession = sessionNumber ?? FindNotCompletedSession();

		if (currentSession > 2)
			return false;

		var restEntries = _vocabularyProgress.Entries
			.Where(kv => !kv.Value.Sessions[currentSession].IsLearned)
			.ToDictionary(kv => kv.Key, kv => kv.Value);

		if (restEntries.Count == 0 || restEntries.Count == _vocabularyProgress.Entries.Count)
		{
			if (currentSession == 0)
			{
				foreach (var valuePair in _vocabularyProgress.Entries.Values)
					valuePair.Sessions[currentSession].IsLearned = false;

				restEntries = _vocabularyProgress.Entries
					.ToDictionary(kv => kv.Key, kv => kv.Value);
			}
			else
			{
				var percent = currentSession == 1
					? _settings.Learn.DifficultEntriesSession2Percent
					: _settings.Learn.DifficultEntriesSession3Percent;
				var prevIndex = currentSession - 1;
				var prevEntries = _vocabularyProgress.Entries
					.OrderByDescending(kv => kv.Value.Sessions[prevIndex].TotalAttempts)
					.Select(kv => kv.Key)
					.ToList();
				var takeCount = (int)(prevEntries.Count * percent / 100.0);

				for (int i = 0; i < prevEntries.Count; i++)
				{
					_vocabularyProgress.Entries[prevEntries[i]].Sessions[currentSession].IsLearned = (i > takeCount);
				}

				restEntries = _vocabularyProgress.Entries
					.Where(kv => !kv.Value.Sessions[currentSession].IsLearned)
					.ToDictionary(kv => kv.Key, kv => kv.Value);
			}

			_vocabularyProgressStore.Save(_vocabulary.FilePath, _vocabularyProgress);
			var progress = _vocabularyProgress.GetSessionProgress(_session.SessionIndex);
			_vocabularyListService.UpdateSessionAndSave(_vocabulary.FilePath, _session.SessionIndex, progress.LearnedEntries, progress.TotalEntries);
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

		return true;
	}

	private int FindNotCompletedSession()
	{
		int currentSession = 0;
		Dictionary<string, VocabularyProgressEntry> restEntries = [];

		for (int i = 0; i <= _vocabularyProgress.SessionIndex; i++)
		{
			restEntries = _vocabularyProgress.Entries
				.Where(kv => kv.Value.Sessions.Count > i && !kv.Value.Sessions[i].IsLearned)
				.ToDictionary(kv => kv.Key, kv => kv.Value);

			if (restEntries.Count > 0)
			{
				currentSession = i;
				break;
			}
		}

		return currentSession;
	}

	public EntryProgress GetFirstEntry()
	{
		return GetCurrentEntry();
	}

	public EntryProgress? GetNextEntry()
	{
		if (_session.EntryIndex + 1 < _session.Exercise.Count)
		{
			_session.EntryIndex++;
			return GetCurrentEntry();
		}

		if (_session.Exercise.Count > 2)
		{
			_session.EntryIndex = 0;
			return GetCurrentEntry();
		}

		var restEntries = _vocabularyProgress.Entries
			.Where(kv => !kv.Value.Sessions[_session.SessionIndex].IsLearned)
			.ToDictionary(kv => kv.Key, kv => kv.Value);

		if (restEntries.Count == 0)
			return null;

		_session.Exercise = [.. restEntries.Take(_settings.Learn.ExerciseSize).Select(kv => kv.Key)];
		_session.EntryIndex = 0;
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
		var entry = _vocabulary.Entries.FirstOrDefault(e => e.RuText == ruText)
			?? throw new InvalidDataException($"Entry with RuText '{ruText}' not found in vocabulary.");
		var progressEntrySession = _session.Entries[ruText].Sessions[_session.SessionIndex];
		progressEntrySession.TotalAttempts++;

		if (isCorrect)
		{
			progressEntrySession.CorrectAnswers++;

			if (progressEntrySession.CorrectAnswers >= _settings.Learn.CorrectAnswersToLearn)
			{
				// mark as learned
				progressEntrySession.IsLearned = true;
				// update session
				_session.Exercise.Remove(ruText);
				// update studied entries count
				var progress = _vocabularyProgress.GetSessionProgress(_session.SessionIndex);
				_vocabularyListService.UpdateSessionAndSave(_vocabulary.FilePath, _session.SessionIndex, progress.LearnedEntries, progress.TotalEntries);
			}
		}

		_vocabularyProgressStore.Save(_vocabulary.FilePath, _vocabularyProgress);

		return new EntryProgress
		{
			Entry = entry,
			IsLearned = progressEntrySession.IsLearned,
			IsLastAttemptSuccess = isCorrect,
			CorrectAnswers = progressEntrySession.CorrectAnswers,
			TotalAttempts = progressEntrySession.TotalAttempts,
			Session = new()
			{
				TotalEntriesCount = _session.TotalEntriesCount,
				SessionEntriesCount = _session.Entries.Count,
				StudiedEntriesCount = _session.StudiedEntriesCount
			}
		};
	}
}
