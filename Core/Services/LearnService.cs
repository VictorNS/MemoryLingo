using System.IO;
using MemoryLingo.Core.Models;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.VocabularyExcel;
using MemoryLingo.Infrastructure.VocabularyProgress;
using MemoryLingo.Infrastructure.VocabularyReference;

namespace MemoryLingo.Core.Services;

public interface ILearnService
{
	IReadOnlyList<VocabularyReferenceDto> LoadVocabularyList(bool forceReloadSession);
	IReadOnlyList<VocabularyReferenceDto> GetVocabularyList();
	VocabularyReferenceDto AddVocabularyFile(string filePath);
	void RemoveVocabularyFile(string filePath);

	VocabularyExcelDto? StartVocabularySession(string filePath, int sessionIndex, bool continueSession);
	EntryProgress GetFirstEntry();
	EntryProgress? GetNextEntry();
	EntryProgress SaveEntryProgress(string ruText, bool isCorrect);
}

public class LearnService : ILearnService
{
	readonly ISettingsStore _settingsService;
	readonly IVocabularyProgressStore _vocabularyProgressStore;
	readonly IVocabularyReferenceStore _vocabularyListService;
	readonly IVocabularyExcelReader _vocabularyService;
	readonly SettingsDto _settings;
	VocabularyExcelDto? _vocabulary;
	VocabularyProgressDto? _vocabularyProgress;
	LearnSession? _session;

	public LearnService(ISettingsStore settingsService, IVocabularyProgressStore vocabularyProgressStore, IVocabularyReferenceStore vocabularyListService, IVocabularyExcelReader vocabularyService)
	{
		_settingsService = settingsService;
		_vocabularyProgressStore = vocabularyProgressStore;
		_vocabularyListService = vocabularyListService;
		_vocabularyService = vocabularyService;
		_settings = _settingsService.Load();
	}

	#region VocabularyList
	public IReadOnlyList<VocabularyReferenceDto> LoadVocabularyList(bool forceReloadSession)
	{
		var vocabularieReferences = _vocabularyListService.Load();

		foreach (var vr in vocabularieReferences)
		{
			var result = LoadAndCheckVocabularyFile(vr.FilePath);

			if (result.CheckResult.HasErrors)
				vr.ErrorMessage = result.CheckResult.ErrorMessage;

			if (forceReloadSession)
			{
				var vocabularyProgress = _vocabularyProgressStore.Load(vr.FilePath);

				for (int sessionIndex = 0; sessionIndex < 3; sessionIndex++)
				{
					var s = vocabularyProgress.Sessions[sessionIndex];
					var avg = vocabularyProgress.Entries.Values
						.Where(x=> x.Sessions[sessionIndex].IsLearned)
						.Average(x => (decimal?)x.Sessions[sessionIndex].TotalAttempts);
					vr.Sessions[sessionIndex].Update(s.LastUpdated, s.LearnedEntries, s.TotalEntries, avg);
				}
			}
		}

		_vocabularyListService.Save();
		return _vocabularyListService.GetVocabularyList();
	}

	public VocabularyReferenceDto AddVocabularyFile(string filePath)
	{
		var result = LoadAndCheckVocabularyFile(filePath);
		var vocabularyProgress = _vocabularyProgressStore.Load(filePath);

		var vr = new VocabularyReferenceDto
		{
			FilePath = result.CheckResult.FilePath,
			FileName = result.CheckResult.FileName,
			ErrorMessage = result.CheckResult.ErrorMessage,
		};

		for (int sessionIndex = 0; sessionIndex < 3; sessionIndex++)
		{
			var s = vocabularyProgress.Sessions[sessionIndex];
			var avg = vocabularyProgress.Entries.Values
				.Where(x => x.Sessions[sessionIndex].IsLearned)
				.Average(x => (decimal)x.Sessions[sessionIndex].TotalAttempts);
			vr.Sessions[sessionIndex].Update(s.LastUpdated, s.LearnedEntries, s.TotalEntries, avg);
		}

		_vocabularyListService.AddAndSave(vr);
		return vr;
	}

	public void RemoveVocabularyFile(string filePath)
	{
		_vocabularyListService.RemoveAndSave(filePath);
	}

	(VocabularyExcelDto Vocabulary, VocabularyCheckResult CheckResult) LoadAndCheckVocabularyFile(string filePath)
	{
		var vocabulary = _vocabularyService.LoadVocabulary(filePath);
		var vocabularyReference = new VocabularyCheckResult
		{
			FileName = vocabulary.FileName,
			FilePath = vocabulary.FilePath,
			ErrorMessage = vocabulary.ErrorMessage
		};

		if (!string.IsNullOrEmpty(vocabularyReference.ErrorMessage))
		{
			return (vocabulary, vocabularyReference);
		}

		if (vocabulary.Entries.Count == 0)
		{
			vocabularyReference.ErrorMessage = "Contains no entries.";
			return (vocabulary, vocabularyReference);
		}

		var duplicates = vocabulary.Entries.GroupBy(x => x.RuText).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
		if (duplicates.Count > 0)
		{
			vocabularyReference.ErrorMessage = $"Duplicates: {string.Join(", ", duplicates)}";
			return (vocabulary, vocabularyReference);
		}

		return (vocabulary, vocabularyReference);
	}

	public IReadOnlyList<VocabularyReferenceDto> GetVocabularyList()
	{
		return _vocabularyListService.Load();
	}
	#endregion VocabularyList

	public VocabularyExcelDto? StartVocabularySession(string filePath, int sessionIndex, bool continueSession)
	{
		var result = LoadAndCheckVocabularyFile(filePath);

		if (result.CheckResult.HasErrors)
		{
			return null;
		}

		_vocabulary = result.Vocabulary;
		_vocabularyProgress = _vocabularyProgressStore.Load(_vocabulary.FilePath);
		SynchronizeProgressWithVocabulary();
		_vocabularyProgressStore.Save(_vocabulary.FilePath, sessionIndex, _vocabularyProgress);

		if (!LoadSession(sessionIndex, continueSession))
			return null;

		return _vocabulary;
	}

	void SynchronizeProgressWithVocabulary()
	{
		if (_vocabulary is null || _vocabularyProgress is null)
			return;

		// Create a new dictionary (based on vocabulary entries order)
		var newEntries = new Dictionary<string, VocabularyProgressEntry>();

		foreach (var entry in _vocabulary.Entries)
		{
			var progressEntry = _vocabularyProgress.Entries.TryGetValue(entry.RuText, out var existing)
				? existing
				: new VocabularyProgressEntry();
			newEntries.Add(entry.RuText, progressEntry);
		}

		// Replace the old dictionary with the new
		_vocabularyProgress.Entries = newEntries;
	}

	internal bool LoadSession(int sessionIndex, bool continueSession)
	{
		if (_vocabulary is null || _vocabularyProgress is null)
			return false;

		if (sessionIndex < 0 || sessionIndex > 2)
			return false;

		Dictionary<string, VocabularyProgressEntry> restEntries;

		if (continueSession)
		{
			restEntries = _vocabularyProgress.Entries
				.Where(kv => !kv.Value.Sessions[sessionIndex].IsSkipped && !kv.Value.Sessions[sessionIndex].IsLearned)
				.ToDictionary(kv => kv.Key, kv => kv.Value);
		}
		else
		{
			foreach (var valuePair in _vocabularyProgress.Entries.Values)
				valuePair.Sessions[sessionIndex].Reset();

			if (sessionIndex > 0)
			{
				var prevSession = sessionIndex - 1;

				var prevSkippedEntries = _vocabularyProgress.Entries
					.Where(kv => kv.Value.Sessions[prevSession].IsSkipped)
					.Select(kv => kv.Key)
					.ToList();

				for (int i = 0; i < prevSkippedEntries.Count; i++)
					_vocabularyProgress.Entries[prevSkippedEntries[i]].Sessions[sessionIndex].IsSkipped = true;

				var prevEntries = _vocabularyProgress.Entries
					.Where(kv => !kv.Value.Sessions[prevSession].IsSkipped)
					.OrderByDescending(kv => kv.Value.Sessions[prevSession].TotalAttempts)
					.Select(kv => kv.Key)
					.ToList();
				var percent = sessionIndex == 1
					? _settings.Learn.DifficultEntriesSession2Percent
					: _settings.Learn.DifficultEntriesSession3Percent;
				var takeCount = (int)(prevEntries.Count * percent / 100.0);

				for (int i = 0; i < prevEntries.Count; i++)
					_vocabularyProgress.Entries[prevEntries[i]].Sessions[sessionIndex].IsSkipped = (i + 1 > takeCount);
			}

			restEntries = _vocabularyProgress.Entries
				.Where(kv => !kv.Value.Sessions[sessionIndex].IsSkipped)
				.ToDictionary(kv => kv.Key, kv => kv.Value);
		}

		if (restEntries.Count == 0)
			return false;

		var skippedCount = _vocabularyProgress.Entries.Count(kv => kv.Value.Sessions[sessionIndex].IsSkipped);

		_session = new LearnSession
		{
			SessionIndex = sessionIndex,
			QueueIndex = 0,
			IsLastLearned = false,
			Entries = restEntries,
			Queue = BuildQueue(restEntries),
			VocabularyEntriesCount = _vocabularyProgress.Entries.Count - skippedCount,
			VocabularyLearnedCount = _vocabularyProgress.Entries.Count - restEntries.Count - skippedCount,
		};

		if (!continueSession)
		{
			// update vocabulary progress
			_vocabularyProgressStore.Save(_vocabulary.FilePath, _session.SessionIndex, _vocabularyProgress);
			var progress = _vocabularyProgress.GetSessionProgress(_session.SessionIndex);
			_vocabularyListService.UpdateSessionAndSave(_vocabulary.FilePath, _session.SessionIndex, progress.LearnedEntries, progress.TotalEntries);
		}

		return true;
	}

	internal LearnSession? GetCurrentSession()
	{
		return _session;
	}

	public EntryProgress GetFirstEntry()
	{
		return GetEntryByQueueIndex();
	}

	public EntryProgress? GetNextEntry()
	{
		if (_vocabularyProgress is null || _session is null)
			return null;

		if (_session.Queue.Count == 0) // it's possible when all entries are learned
			return null;

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
			.Where(kv => !kv.Value.Sessions[_session.SessionIndex].IsSkipped && !kv.Value.Sessions[_session.SessionIndex].IsLearned)
			.ToDictionary(kv => kv.Key, kv => kv.Value);

		if (restEntries.Count == 0)
			return null;

		_session.Queue = BuildQueue(restEntries);
		_session.QueueIndex = 0;
		return GetEntryByQueueIndex();
	}

	EntryProgress GetEntryByQueueIndex()
	{
		if (_vocabulary is null || _session is null)
			return EntryProgress.Empty;

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

	List<string> BuildQueue(Dictionary<string, VocabularyProgressEntry> entries)
	{
		if (_settings.Behavior.RandomizeQueue)
			return [.. entries.OrderBy(x => Random.Shared.Next()).Take(_settings.Learn.ExerciseSize).Select(kv => kv.Key)];

		return [.. entries.Take(_settings.Learn.ExerciseSize).Select(kv => kv.Key)];
	}

	public EntryProgress SaveEntryProgress(string ruText, bool isCorrect)
	{
		if (_vocabulary is null || _vocabularyProgress is null || _session is null)
			return EntryProgress.Empty;

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

		_vocabularyProgressStore.Save(_vocabulary.FilePath, _session.SessionIndex, _vocabularyProgress);

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
