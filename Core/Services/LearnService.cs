using System.IO;
using MemoryLingo.Core.Models;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.VocabularyExcel;
using MemoryLingo.Infrastructure.VocabularyProgress;
using MemoryLingo.Infrastructure.VocabularyReference;

namespace MemoryLingo.Core.Services;

public class LearnService
{
	readonly ISettingsStore _settingsService;
	readonly IVocabularyProgressStore _vocabularyProgressStore;
	readonly IVocabularyReferenceStore _vocabularyListService;
	readonly IVocabularyExcelReader _vocabularyService;
	readonly SettingsDto _settings;
	VocabularyExcelDto _vocabulary;
	VocabularyProgressDto _vocabularyProgress;
	LearnSession _session;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public LearnService(ISettingsStore settingsService, IVocabularyProgressStore vocabularyProgressStore, IVocabularyReferenceStore vocabularyListService, IVocabularyExcelReader vocabularyService)
	{
		_settingsService = settingsService;
		_vocabularyProgressStore = vocabularyProgressStore;
		_vocabularyListService = vocabularyListService;
		_vocabularyService = vocabularyService;
		_settings = _settingsService.Load();
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	#region VocabularyList
	public IReadOnlyList<VocabularyReferenceDto> LoadVocabularyList()
	{
		var vocabularies = _vocabularyListService.Load();

		foreach (var vocabularyFile in vocabularies)
		{
			var checkResult = LoadAndCheckVocabularyFile(vocabularyFile.FilePath);

			if (checkResult.vocabularyFile.HasErrors)
				vocabularyFile.ErrorMessage = checkResult.vocabularyFile.ErrorMessage;
		}

		return vocabularies;
	}

	public VocabularyReferenceDto AddVocabularyFile(string filePath)
	{
		var checkResult = LoadAndCheckVocabularyFile(filePath);
		_vocabularyListService.AddAndSave(checkResult.vocabularyFile);
		return checkResult.vocabularyFile;
	}

	public void RemoveVocabularyFile(string filePath)
	{
		_vocabularyListService.RemoveAndSave(filePath);
	}

	private (VocabularyExcelDto vocabulary, VocabularyReferenceDto vocabularyFile) LoadAndCheckVocabularyFile(string filePath)
	{
		var vocabulary = _vocabularyService.LoadVocabulary(filePath);
		var vocabularyFile = new VocabularyReferenceDto
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

	public IReadOnlyList<VocabularyReferenceDto> GetVocabularyList()
	{
		return _vocabularyListService.Load();
	}
	#endregion VocabularyList

	public VocabularyExcelDto? StartVocabularySession(string filePath, int sessionNumber, bool continueSession)
	{
		var checkResult = LoadAndCheckVocabularyFile(filePath);

		if (checkResult.vocabularyFile.HasErrors)
		{
			return null;
		}

		_vocabulary = checkResult.vocabulary;
		_vocabularyProgress = _vocabularyProgressStore.Load(_vocabulary.FilePath);
		SynchronizeProgressWithVocabulary();
		_vocabularyProgressStore.Save(_vocabulary.FilePath, _vocabularyProgress);

		if (!LoadSession(sessionNumber, continueSession))
			return null;

		return _vocabulary;
	}

	public void SynchronizeProgressWithVocabulary()
	{
		// Create a new dictionary (based on vocabulary entries order)
		var newEntries = new Dictionary<string, VocabularyProgressEntry>();

		foreach (var entry in _vocabulary.Entries)
		{
			var progressEntry = _vocabularyProgress.Entries.TryGetValue(entry.RuText, out var existing)
				? existing
				: new VocabularyProgressEntry();
			newEntries.Add(entry.RuText, progressEntry);

			// Ensure all entries have 3 sessions
			for (int i = progressEntry.Sessions.Count; i < 3; i++)
				progressEntry.Sessions.Add(new VocabularyProgressEntrySession());
		}

		// Replace the old dictionary with the new
		_vocabularyProgress.Entries = newEntries;
	}

	internal bool LoadSession(int currentSession, bool continueSession)
	{

		if (currentSession < 0 || currentSession > 2)
			return false;

		Dictionary<string, VocabularyProgressEntry> restEntries;

		if (continueSession)
		{
			restEntries = _vocabularyProgress.Entries
				.Where(kv => !kv.Value.Sessions[currentSession].IsSkipped && !kv.Value.Sessions[currentSession].IsLearned)
				.ToDictionary(kv => kv.Key, kv => kv.Value);
		}
		else
		{
			foreach (var valuePair in _vocabularyProgress.Entries.Values)
			{
				valuePair.Sessions[currentSession].IsSkipped = false;
				valuePair.Sessions[currentSession].IsLearned = false;
			}

			if (currentSession > 0)
			{
				var prevSession = currentSession - 1;

				var prevSkippedEntries = _vocabularyProgress.Entries
					.Where(kv => kv.Value.Sessions[prevSession].IsSkipped)
					.Select(kv => kv.Key)
					.ToList();

				for (int i = 0; i < prevSkippedEntries.Count; i++)
					_vocabularyProgress.Entries[prevSkippedEntries[i]].Sessions[currentSession].IsSkipped = true;

				var prevEntries = _vocabularyProgress.Entries
					.Where(kv => !kv.Value.Sessions[prevSession].IsSkipped)
					.OrderByDescending(kv => kv.Value.Sessions[prevSession].TotalAttempts)
					.Select(kv => kv.Key)
					.ToList();
				var percent = currentSession == 1
					? _settings.Learn.DifficultEntriesSession2Percent
					: _settings.Learn.DifficultEntriesSession3Percent;
				var takeCount = (int)(prevEntries.Count * percent / 100.0);

				for (int i = 0; i < prevEntries.Count; i++)
					_vocabularyProgress.Entries[prevEntries[i]].Sessions[currentSession].IsSkipped = (i + 1 > takeCount);
			}

			restEntries = _vocabularyProgress.Entries
				.Where(kv => !kv.Value.Sessions[currentSession].IsSkipped)
				.ToDictionary(kv => kv.Key, kv => kv.Value);
		}

		_session = new LearnSession
		{
			SessionIndex = currentSession,
			QueueIndex = 0,
			IsLastLearned = false,
			Entries = restEntries,
			Queue = [.. restEntries.Take(_settings.Learn.ExerciseSize).Select(kv => kv.Key)],
			VocabularyEntriesCount = _vocabularyProgress.Entries.Count,
			VocabularyLearnedCount = _vocabularyProgress.Entries.Count - restEntries.Count
		};

		if (!continueSession)
		{
			// update vocabulary progress
			_vocabularyProgressStore.Save(_vocabulary.FilePath, _vocabularyProgress);
			var progress = _vocabularyProgress.GetSessionProgress(_session.SessionIndex);
			_vocabularyListService.UpdateSessionAndSave(_vocabulary.FilePath, _session.SessionIndex, progress.LearnedEntries, progress.TotalEntries);
		}

		return true;
	}

	internal LearnSession GetCurrentSession()
	{
		return _session;
	}

	public EntryProgress GetFirstEntry()
	{
		return GetEntryByQueueIndex();
	}

	public EntryProgress? GetNextEntry()
	{
		var expectQueueIndex = _session.QueueIndex + (_session.IsLastLearned ? 0 : 1);
		_session.IsLastLearned = false;

		if (expectQueueIndex < _session.Queue.Count)
		{
			_session.QueueIndex = expectQueueIndex;
			return GetEntryByQueueIndex();
		}

		if (_session.Queue.Count > 2)
		{
			_session.QueueIndex = 0;
			return GetEntryByQueueIndex();
		}

		var restEntries = _vocabularyProgress.Entries
			.Where(kv => !kv.Value.Sessions[_session.SessionIndex].IsLearned)
			.ToDictionary(kv => kv.Key, kv => kv.Value);

		if (restEntries.Count == 0)
			return null;

		_session.Queue = [.. restEntries.Take(_settings.Learn.ExerciseSize).Select(kv => kv.Key)];
		_session.QueueIndex = 0;
		return GetEntryByQueueIndex();
	}

	private EntryProgress GetEntryByQueueIndex()
	{
		var ruText = _session.Queue[_session.QueueIndex];
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
				QueueIndex = _session.QueueIndex,
				QueueCount = _session.Queue.Count,
				VocabularyLearnedCount = _session.VocabularyLearnedCount,
				VocabularyEntriesCount = _session.VocabularyEntriesCount,
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
				_session.VocabularyLearnedCount++;
				_session.IsLastLearned = true;
				_session.Queue.Remove(ruText);
				// update vocabulary progress
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
				QueueIndex = _session.QueueIndex,
				QueueCount = _session.Queue.Count,
				VocabularyLearnedCount = _session.VocabularyLearnedCount,
				VocabularyEntriesCount = _session.VocabularyEntriesCount,
			}
		};
	}
}
