using MemoryLingo.Core.Models;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.VocabularyExcel;
using MemoryLingo.Infrastructure.VocabularyProgress;
using MemoryLingo.Infrastructure.VocabularyReference;

namespace MemoryLingo.Core.Services;

public interface ILearnService
{
	IReadOnlyList<VocabularyReferenceModel> LoadVocabularyList(bool forceReloadSession);
	IReadOnlyList<VocabularyReferenceModel> GetVocabularyList();
	VocabularyReferenceModel AddVocabularyFile(string filePath);
	void RemoveVocabularyFile(string filePath);

	VocabularyExcelDto? StartVocabularySession(string filePath, int sessionIndex, bool continueSession);
	EntryContainer GetFirstEntry();
	EntryContainer GetNextEntry();
	EntryContainer GetFirstEntryOfNewSession();
	EntryContainer SaveEntryProgress(string ruText, bool isAnswerCorrect);
}

public class LearnService : ILearnService
{
	readonly ISettingsStore _settingsService;
	readonly IVocabularyProgressStore _vocabularyProgressStore;
	readonly IVocabularyReferenceService _vocabularyReferenceService;
	readonly IVocabularyExcelReader _vocabularyService;
	readonly SettingsDto _settings;
	VocabularyExcelDto? _vocabulary;
	VocabularyProgressDto? _vocabularyProgress;
	LearnSession? _session;

	public LearnService(ISettingsStore settingsService, IVocabularyProgressStore vocabularyProgressStore, IVocabularyReferenceService vocabularyReferenceService, IVocabularyExcelReader vocabularyService)
	{
		_settingsService = settingsService;
		_vocabularyProgressStore = vocabularyProgressStore;
		_vocabularyReferenceService = vocabularyReferenceService;
		_vocabularyService = vocabularyService;
		_settings = _settingsService.Get();
	}

	#region VocabularyList
	public IReadOnlyList<VocabularyReferenceModel> LoadVocabularyList(bool forceReloadSession)
	{
		var vocabularieReferences = _vocabularyReferenceService.Load();

		foreach (var vr in vocabularieReferences)
		{
			vr.ErrorMessage = string.Empty;
			var result = LoadAndCheckVocabularyFile(vr.FilePath);

			if (result.CheckResult.HasErrors)
				vr.ErrorMessage = result.CheckResult.ErrorMessage;

			if (forceReloadSession)
			{
				var vocabularyProgress = _vocabularyProgressStore.Load(vr.FilePath);

				for (int sessionIndex = 0; sessionIndex < 3; sessionIndex++)
				{
					var s = vocabularyProgress.Sessions[sessionIndex];
					var learnedEntries = vocabularyProgress.Entries.Values.Where(x => x.Sessions[sessionIndex].IsLearned);
					var avg = learnedEntries.Any()
						? learnedEntries.Average(x => (decimal?)x.Sessions[sessionIndex].TotalAttempts)
						: null;
					vr.Sessions[sessionIndex].Update(s.LastUpdated, s.LearnedEntries, s.TotalEntries, avg);
				}
			}
		}

		_vocabularyReferenceService.Save(vocabularieReferences);
		return _vocabularyReferenceService.GetVocabularyList();
	}

	public VocabularyReferenceModel AddVocabularyFile(string filePath)
	{
		var result = LoadAndCheckVocabularyFile(filePath);
		var vocabularyProgress = _vocabularyProgressStore.Load(filePath);

		var vr = VocabularyReferenceModel.Empty(result.CheckResult.FilePath);
		vr.ErrorMessage = result.CheckResult.ErrorMessage;

		for (int sessionIndex = 0; sessionIndex < 3; sessionIndex++)
		{
			var s = vocabularyProgress.Sessions[sessionIndex];
			var learnedEntries = vocabularyProgress.Entries.Values.Where(x => x.Sessions[sessionIndex].IsLearned);
			var avg = learnedEntries.Any()
				? learnedEntries.Average(x => (decimal)x.Sessions[sessionIndex].TotalAttempts)
				: (decimal?)null;
			vr.Sessions[sessionIndex].Update(s.LastUpdated, s.LearnedEntries, s.TotalEntries, avg);
		}

		_vocabularyReferenceService.AddAndSave(vr);
		return vr;
	}

	public void RemoveVocabularyFile(string filePath)
	{
		_vocabularyReferenceService.RemoveAndSave(filePath);
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

	public IReadOnlyList<VocabularyReferenceModel> GetVocabularyList()
	{
		return _vocabularyReferenceService.GetVocabularyList();
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
			LessonsCompletedCount = 0,
			TotalEntriesCompletedCount = 0,
			Entries = restEntries,
			Queue = [],
			VocabularyEntriesCount = _vocabularyProgress.Entries.Count - skippedCount,
			VocabularyLearnedCount = _vocabularyProgress.Entries.Count - restEntries.Count - skippedCount,
		};
		_session.Queue = BuildQueue(restEntries);

		if (!continueSession)
		{
			// update vocabulary progress
			_vocabularyProgressStore.Save(_vocabulary.FilePath, _session.SessionIndex, _vocabularyProgress);
			var progress = _vocabularyProgress.GetSessionProgress(_session.SessionIndex);
			_vocabularyReferenceService.UpdateSessionAndSave(_vocabulary.FilePath, _session.SessionIndex, progress.LearnedEntries, progress.TotalEntries);
		}

		return _session.Queue.Count > 0;
	}

	internal LearnSession? GetCurrentSession()
	{
		return _session;
	}

	public EntryContainer GetFirstEntry()
	{
		if (_session is null)
			return EntryContainer.Empty;

		var entryProgress = _session.Queue[_session.QueueIndex];
		return EntryContainer.Create(entryProgress, _session);
	}

	public EntryContainer GetNextEntry()
	{
		if (_session is null || _session.Queue.Count == 0) // it's possible when all entries are learned
			return EntryContainer.Empty;

		_session.TotalEntriesCompletedCount++;
		var expectQueueIndex = _session.QueueIndex + 1;

		if (expectQueueIndex >= _session.Queue.Count) // We have completed the session.
		{
			expectQueueIndex = 0;
			_session.LessonsCompletedCount++;

			if (_session.LessonsCompletedCount >= _settings.Learn.LessonRepeatCount)
				return GetFirstEntryOfNewSession(); // Start a new session if the repeat count is reached.
		}

		_session.QueueIndex = expectQueueIndex;
		var entryProgress = _session.Queue[_session.QueueIndex];
		return EntryContainer.Create(entryProgress, _session);
	}

	public EntryContainer GetFirstEntryOfNewSession()
	{
		if (_vocabularyProgress is null || _session is null)
			return EntryContainer.Empty;

		var restEntries = _vocabularyProgress.Entries
			.Where(kv => !kv.Value.Sessions[_session.SessionIndex].IsSkipped && !kv.Value.Sessions[_session.SessionIndex].IsLearned)
			.ToDictionary(kv => kv.Key, kv => kv.Value);

		if (restEntries.Count == 0)
			return EntryContainer.Empty;

		_session.QueueIndex = 0;
		_session.LessonsCompletedCount = 0;
		_session.TotalEntriesCompletedCount = 0;
		_session.Queue = BuildQueue(restEntries);

		var entryProgress = _session.Queue[_session.QueueIndex];
		return EntryContainer.Create(entryProgress, _session);
	}

	internal List<EntryProgress> BuildQueue(Dictionary<string, VocabularyProgressEntry> entries)
	{
		if (_vocabulary is null || _session is null)
			return [];

		List<string> queue;
		var sessionIndex = _session.SessionIndex;
		var exerciseSize = _settings.Learn.LessonSize;

		// Level 0: just take the first N entries (no randomization)
		if (_settings.Behavior.RandomizeLevel == 0)
		{
			queue = [.. entries.Take(exerciseSize).Select(kv => kv.Key)];
		}
		// If entries count is less than or equal to 1.4 times the exercise size, just randomize and take the first N entries
		else if (entries.Count <= exerciseSize * 1.4)
		{
			queue = [.. entries.OrderBy(_ => Random.Shared.Next()).Take(exerciseSize).Select(kv => kv.Key)];
		}
		// Level 3: just randomize all entries
		else if (_settings.Behavior.RandomizeLevel == 3)
		{
			queue = entries
				.Select(kv => kv.Key)
				.OrderBy(_ => Random.Shared.Next())
				.Take(exerciseSize)
				.ToList();
		}
		// Level 4: prioritize entries with fewer attempts
		else if (_settings.Behavior.RandomizeLevel == 4)
		{
			queue = entries
				.OrderBy(kv => kv.Value.Sessions[sessionIndex].TotalAttempts)
				.ThenBy(_ => Random.Shared.Next())
				.Select(kv => kv.Key)
				.Take(exerciseSize)
				.ToList();
		}
		else
		{
			// Split proportionally into 3 groups, where the split ratios depend on RandomizeLevel:
			// Group 1 (startedP): already started entries (TotalAttempts > 0)
			// Group 2 (nearP of the remainder): from the nearest range
			// Group 3 (remainder): from the farther range
			// Higher RandomizeLevel lowers startedP/nearP, shifting weight toward farther, more random entries:
			// Level 1: startedP 0.70, nearP 0.65
			// Level 2: startedP 0.30, nearP 0.30

			// Default: RandomizeLevel == 1
			double startedP = 0.70;
			double nearP = 0.65;

			if (_settings.Behavior.RandomizeLevel == 2)
			{
				startedP = 0.30;
				nearP = 0.30;
			}

			var startedCount = (int)Math.Round(exerciseSize * startedP);
			var startedKeys = entries
				.Where(kv => kv.Value.Sessions[sessionIndex].TotalAttempts > 0)
				.OrderByDescending(kv => kv.Value.Sessions[sessionIndex].TotalAttempts)
				.Select(kv => kv.Key).ToList();
			var started = startedKeys.Take(startedCount).ToList();

			var nearCount = (int)Math.Round((exerciseSize - started.Count) * nearP);
			var restKeys = entries.Select(kv => kv.Key).Except(started).ToList();
			var near = restKeys.Take(exerciseSize * 2).OrderBy(_ => Random.Shared.Next()).Take(nearCount).ToList();

			var farCount = exerciseSize - started.Count - near.Count;
			restKeys = restKeys.Except(near).ToList();
			var far = restKeys.OrderBy(_ => Random.Shared.Next()).Take(farCount).ToList();
			queue = started.Concat(near).Concat(far).OrderBy(_ => Random.Shared.Next()).ToList();
		}

		List<EntryProgress> entryProgressList = [];

		foreach (var text in queue)
		{
			var entry = _vocabulary.Entries.First(e => e.RuText == text);
			var progress = entries[text].Sessions[sessionIndex];

			entryProgressList.Add(new EntryProgress
			{
				Entry = entry,
				IsLearned = progress.IsLearned,
				CorrectAnswers = progress.CorrectAnswers,
				TotalAttempts = progress.TotalAttempts,
			});
		}

		return entryProgressList;
	}

	public EntryContainer SaveEntryProgress(string ruText, bool isAnswerCorrect)
	{
		if (_vocabulary is null || _vocabularyProgress is null || _session is null)
			return EntryContainer.Empty;

		var entryProgress = _session.Queue.First(x => x.Entry.RuText == ruText);
		var progressEntrySession = _session.Entries[ruText].Sessions[_session.SessionIndex];
		progressEntrySession.TotalAttempts++;

		if (isAnswerCorrect)
		{
			progressEntrySession.CorrectAnswers++;

			if (progressEntrySession.CorrectAnswers >= _settings.Learn.CorrectAnswersToLearn)
			{
				progressEntrySession.IsLearned = true;
				_session.VocabularyLearnedCount++;
				// update vocabulary progress
				var progress = _vocabularyProgress.GetSessionProgress(_session.SessionIndex);
				_vocabularyReferenceService.UpdateSessionAndSave(_vocabulary.FilePath, _session.SessionIndex, progress.LearnedEntries, progress.TotalEntries);
			}
		}

		_vocabularyProgressStore.Save(_vocabulary.FilePath, _session.SessionIndex, _vocabularyProgress);

		entryProgress.IsLearned = progressEntrySession.IsLearned;
		entryProgress.IsLastAttemptSuccess = isAnswerCorrect;
		entryProgress.CorrectAnswers = progressEntrySession.CorrectAnswers;
		entryProgress.TotalAttempts = progressEntrySession.TotalAttempts;

		return EntryContainer.Create(entryProgress, _session);
	}
}
