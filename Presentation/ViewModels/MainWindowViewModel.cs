using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using MemoryLingo.Core.Models;
using MemoryLingo.Core.Services;
using MemoryLingo.Infrastructure.Logging;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.SpeechSynthesis;
using MemoryLingo.Infrastructure.VocabularyReference;
using MemoryLingo.Presentation.Commands;
using SW = System.Windows;

namespace MemoryLingo.Presentation.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
	#region logic properties
	readonly ISettingsStore _settingsService;
	readonly SettingsDto _settings;
	readonly EntryValidationService _entryValidationService;
	readonly ILearnService _learnService;
	readonly ILogService _logService;
	readonly ISpeechService _speechService;
	readonly ISynthesisService _synthesisService;
	EntryProgress _current = EntryProgress.Empty;
	EntryProgress _previous = EntryProgress.Empty;
	readonly DispatcherTimer _validationTimer = new();
	string _vocabularyLanguage = string.Empty;
	string _vocabularyPath = string.Empty;
	#endregion logic properties

	#region Application settings properties
	public int RandomizeLevel
	{
		get => _settings.Behavior.RandomizeLevel;
		set
		{
			if (_settings.Behavior.RandomizeLevel != value)
			{
				_settings.Behavior.RandomizeLevel = value;
				SaveSettings();
				OnPropertyChanged();
			}
		}
	}

	public int ExerciseSize
	{
		get => _settings.Learn.ExerciseSize;
		set
		{
			int clamped = Math.Clamp(value, 3, 100);
			if (_settings.Learn.ExerciseSize != clamped)
			{
				_settings.Learn.ExerciseSize = clamped;
				SaveSettings();
				OnPropertyChanged();
			}
		}
	}
	#endregion Application settings properties

	#region UI properties
	public event PropertyChangedEventHandler? PropertyChanged;

	public event Action? FocusAnswerRequested;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public string RuText
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string RuTip
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string Transcription
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string Answer
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();

				if (string.IsNullOrWhiteSpace(field))
					return;
				RestartValidationTimer();
			}
		}
	} = string.Empty;

	public string RuExample
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string EnExample
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string FileName
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string PrevRuText
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string PrevRuTip
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string PrevTranscription
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string PrevEnText
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string PrevRuExample
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string PrevEnExample
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string PrevStatusImage
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = "/Presentation/Assets/Images/question-mark-16.png";

	public string PrevStatus
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string QueueStat
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public string VocabularyStat
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = string.Empty;

	public ObservableCollection<TokenCheckResult> TokensResult
	{
		get => field;
		set
		{
			if (!ReferenceEquals(field, value))
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = [];

	public bool TipsUsedForCurrentEntry
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	}

	public bool HideIncorrectTokens
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsOverlayVisible
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	}

	public int SelectedTabIndex
	{
		get => field;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	}

	public ObservableCollection<VocabularyReferenceModel> VocabulariesCollection
	{
		get => field;
		set
		{
			if (!ReferenceEquals(field, value))
			{
				field = value;
				OnPropertyChanged();
			}
		}
	} = [];

	static string WrapTranscription(string t) => t.StartsWith('[') ? t : $"[{t}]";
	#endregion UI properties

	public MainWindowViewModel(ISettingsStore settingsService, EntryValidationService entryValidationService, ILearnService learnService,
		ILogService logService, ISpeechService speechService, ISynthesisService synthesisService)
	{
		_settingsService = settingsService;
		_settings = settingsService.Get();
		_entryValidationService = entryValidationService;
		_learnService = learnService;
		_logService = logService;
		_speechService = speechService;
		_synthesisService = synthesisService;

		ShowTipsCommand = new RelayCommand(ShowTips);
		AddVocabularyCommand = new RelayCommand(AddVocabulary);
		ReloadVocabulariesCommand = new RelayCommand(ReloadVocabularies);
		ReloadLessonCommand = new RelayCommand(ReloadLesson);
		DeleteVocabularyCommand = new ParameterizedRelayCommand<VocabularyReferenceModel>(DeleteVocabulary);
		SessionClickCommand = new ParameterizedRelayCommand<SessionClickParameter>(OnSessionClick);
		RefreshSessionCommand = new ParameterizedRelayCommand<SessionClickParameter>(OnRefreshSessionClick);
		OpenCurrentFileCommand = new RelayCommand(OnOpenCurrentFileClick);

		_validationTimer.Interval = TimeSpan.FromMilliseconds(300);
		_validationTimer.Tick += (s, e) =>
		{
			_validationTimer.Stop();
			ProcessAnswer(Answer);
		};
	}

	public void Initialize()
	{
		LoadVocabularyList(true);
		SelectedTabIndex = 0;
	}

	#region events
	public ICommand ShowTipsCommand { get; }
	public ICommand AddVocabularyCommand { get; }
	public ICommand ReloadVocabulariesCommand { get; }
	public ICommand DeleteVocabularyCommand { get; }
	public ICommand SessionClickCommand { get; }
	public ICommand RefreshSessionCommand { get; }
	public ICommand OpenCurrentFileCommand { get; }
	public ICommand ReloadLessonCommand { get; }

	void ShowTips()
	{
		_validationTimer.Stop();
		TipsUsedForCurrentEntry = true;
		ShowEntry(isNewEntry: false);
	}

	void RestartValidationTimer()
	{
		_validationTimer.Stop();
		_validationTimer.Start();
	}

	void OnSessionClick(SessionClickParameter? sessionParam)
	{
		if (sessionParam == null)
			return;

		StartVocabularySession(sessionParam.VocabularyFile, sessionParam.SessionIndex, true);
	}

	void OnRefreshSessionClick(SessionClickParameter? sessionParam)
	{
		if (sessionParam == null)
			return;

		var vocabularyFile = sessionParam.VocabularyFile;

		if (SW.MessageBoxResult.Yes != SW.MessageBox.Show(
			$"Restart session {sessionParam.SessionIndex + 1} for '{vocabularyFile.FileName}'?",
			"Confirm Session Restart",
			SW.MessageBoxButton.YesNo,
			SW.MessageBoxImage.Question))
			return;

		StartVocabularySession(vocabularyFile, sessionParam.SessionIndex, false);
	}

	void OnOpenCurrentFileClick()
	{
		if (string.IsNullOrWhiteSpace(_vocabularyPath))
			return;

		try
		{
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_vocabularyPath) { UseShellExecute = true });
		}
		catch (Exception ex)
		{
			SW.MessageBox.Show($"Failed to open file: {ex.Message}", "Error", SW.MessageBoxButton.OK, SW.MessageBoxImage.Error);
		}
	}
	#endregion events

	#region VocabularyList
	void ReloadVocabularies()
	{
		LoadVocabularyList(false);
	}

	void LoadVocabularyList(bool initialLoad)
	{
		var vocabularies = _learnService.LoadVocabularyList(forceReloadSession: !initialLoad);

		if (initialLoad)
		{
			VocabulariesCollection = [.. vocabularies];
			var view = CollectionViewSource.GetDefaultView(VocabulariesCollection);
			view.SortDescriptions.Clear();
			view.SortDescriptions.Add(new SortDescription("LastSessionLocalTime", ListSortDirection.Descending));
		}
		else
		{
			UpdateVocabulariesCollectionPreserveSort(vocabularies);
		}
	}

	void AddVocabulary()
	{
		var openFileDialog = new Microsoft.Win32.OpenFileDialog
		{
			Filter = "Excel files (*.xlsx)|*.xlsx",
			FilterIndex = 1,
			RestoreDirectory = true
		};

		if (openFileDialog.ShowDialog() == true)
		{
			if (VocabulariesCollection.Any(v => v.FilePath.Equals(openFileDialog.FileName, StringComparison.OrdinalIgnoreCase)))
			{
				SW.MessageBox.Show("This vocabulary file is already added.", "Duplicate", SW.MessageBoxButton.OK, SW.MessageBoxImage.Stop);
				return;
			}

			var vocabularyFile = _learnService.AddVocabularyFile(openFileDialog.FileName);
			VocabulariesCollection.Add(vocabularyFile);
			var view = CollectionViewSource.GetDefaultView(VocabulariesCollection);
			view.Refresh();
		}
	}

	void DeleteVocabulary(VocabularyReferenceModel? vocabularyFile)
	{
		if (vocabularyFile == null)
			return;

		_learnService.RemoveVocabularyFile(vocabularyFile.FilePath);
		VocabulariesCollection.Remove(vocabularyFile);
	}

	void UpdateVocabulariesCollectionPreserveSort(IEnumerable<VocabularyReferenceModel> vocabularies)
	{
		var view = CollectionViewSource.GetDefaultView(VocabulariesCollection);
		var savedSort = new SortDescriptionCollection();

		if (view?.SortDescriptions.Count > 0)
		{
			foreach (var sortDesc in view.SortDescriptions)
				savedSort.Add(sortDesc);
		}

		VocabulariesCollection.Clear();
		foreach (var vocab in vocabularies)
			VocabulariesCollection.Add(vocab);

		if (savedSort.Count > 0)
		{
			view?.SortDescriptions.Clear();
			foreach (var sortDesc in savedSort)
				view?.SortDescriptions.Add(sortDesc);
		}
	}
	#endregion VocabularyList

	#region Lesson
	void ReloadLesson()
	{
		try
		{
			var newEntry = _learnService.GetFirstEntryOfNewSession();

			if (newEntry is null)
			{
				_current = EntryProgress.Empty;
				LoadVocabularyList(false);
				SelectedTabIndex = 0;
			}
			else
			{
				_current = newEntry;
			}

			ShowEntry(isNewEntry: true);
			ShowSessionInfo(_current.Session);

			IsOverlayVisible = false;
		}
		catch (Exception ex)
		{
			SW.MessageBox.Show("An error occurred while initializing the next entry.", "Error", SW.MessageBoxButton.OK, SW.MessageBoxImage.Error);
			_logService.LogError(ex, nameof(InitializeNextEntry));
			SelectedTabIndex = 0;
		}
	}
	#endregion Lesson

	#region Application settings
	void SaveSettings()
	{
		try
		{
			_settingsService.Save(_settings);
		}
		catch (Exception ex)
		{
			SW.MessageBox.Show("An error occurred while saving the settings.", "Error", SW.MessageBoxButton.OK, SW.MessageBoxImage.Error);
			_logService.LogError(ex, nameof(SaveSettings));
			SelectedTabIndex = 0;
		}
	}
	#endregion Application settings

	#region Show/Hide UI elements
	void ShowSessionInfo(EntryProgress.CurrentSessionProgress sessionProgress)
	{
		QueueStat = $"{sessionProgress.QueueIndex + 1}/{sessionProgress.QueueCount}";
		VocabularyStat = $"{sessionProgress.VocabularyLearnedCount}/{sessionProgress.VocabularyEntriesCount}";
	}

	void ShowEntry(bool isNewEntry)
	{
		HideIncorrectTokens = false;
		RuText = _current.Entry.RuText;
		RuTip = _current.Entry.RuTip;

		if (isNewEntry)
		{
			Answer = string.Empty;
			TipsUsedForCurrentEntry = false;
		}

		if (TipsUsedForCurrentEntry)
		{
			Transcription = WrapTranscription(_current.Entry.Transcription);
			RuExample = _current.Entry.RuExample;
			EnExample = _current.Entry.EnExample;

			var tokens = EntryValidationService.SplitIntoTokens(_current.Entry.EnText);
			TokensResult = [.. tokens.Select(x=> x with { IsMatch = true })];
		}
		else
		{
			Transcription = string.Empty;
			RuExample = string.Empty;
			EnExample = string.Empty;
			TokensResult = [];
		}
	}

	void HideEntry()
	{
		HideIncorrectTokens = false;
		RuText = string.Empty;
		RuTip = string.Empty;
		Answer = string.Empty;
		Transcription = string.Empty;
		RuExample = string.Empty;
		EnExample = string.Empty;
		TokensResult = [];
	}

	void ShowPreviousEntry()
	{
		PrevRuText = _previous.Entry.RuText;
		PrevRuTip = _previous.Entry.RuTip;
		PrevEnText = _previous.Entry.EnText;
		PrevTranscription = WrapTranscription(_previous.Entry.Transcription);
		PrevRuExample = _previous.Entry.RuExample;
		PrevEnExample = _previous.Entry.EnExample;
		PrevStatusImage = $"/Presentation/Assets/Images/{(_previous.CorrectAnswers == 0 && _previous.TotalAttempts == 0
			? "question-mark-16.png"
			: _previous.IsLearned
				? "check-16.png"
				: _previous.IsLastAttemptSuccess
					? "increase-16.png"
					: "decrease-16.png")}";
		PrevStatus = $"{_previous.CorrectAnswers}/{_previous.TotalAttempts}";
	}
	#endregion Show/Hide UI elements

	void StartVocabularySession(VocabularyReferenceModel vocabularyFile, int sessionIndex, bool continueSession)
	{
		try
		{
			var vocabulary = _learnService.StartVocabularySession(vocabularyFile.FilePath, sessionIndex, continueSession);
			if (vocabulary is null)
				return;

			_vocabularyLanguage = vocabulary.Lang;
			_vocabularyPath = vocabulary.FilePath;
			FileName = vocabulary.FileName;
			ShowPreviousEntry();
			_current = _learnService.GetFirstEntry();
			ShowEntry(isNewEntry: true);
			ShowSessionInfo(_current.Session);
			SelectedTabIndex = 1;
			FocusAnswerRequested?.Invoke();
		}
		catch (Exception ex)
		{
			SW.MessageBox.Show($"Failed to start session: {ex.Message}", "Error", SW.MessageBoxButton.OK, SW.MessageBoxImage.Error);
			_logService.LogError(ex, nameof(StartVocabularySession));
		}
	}

	void ProcessAnswer(string answer)
	{
		EntryCheckResult wordResults;

		try
		{
			ShowEntry(isNewEntry: false);

			wordResults = _entryValidationService.GetEntryCheckResult(answer, _current.Entry.EnText);
		}
		catch (Exception ex)
		{
			SW.MessageBox.Show("An error occurred while processing the answer.", "Error", SW.MessageBoxButton.OK, SW.MessageBoxImage.Error);
			_logService.LogError(ex, nameof(ProcessAnswer));
			SelectedTabIndex = 0;
			return;
		}

		if (wordResults.IsCorrect)
		{
			try
			{
				// the answer is considered correct if no tips were used
				bool isAnswerCorrect = !TipsUsedForCurrentEntry;

				_previous = _learnService.SaveEntryProgress(_current.Entry.RuText, isAnswerCorrect);
				HideEntry();
				ShowPreviousEntry();

				if (isAnswerCorrect)
				{
					ShowSessionInfo(_previous.Session);
					var vocabularies = _learnService.GetVocabularyList();
					UpdateVocabulariesCollectionPreserveSort(vocabularies);
				}
			}
			catch (Exception ex)
			{
				SW.MessageBox.Show("An error occurred while processing the answer.", "Error", SW.MessageBoxButton.OK, SW.MessageBoxImage.Error);
				_logService.LogError(ex, nameof(ProcessAnswer));
				SelectedTabIndex = 0;
				return;
			}

			SpeakPreviousEntryThenInitializeNext();
			return;
		}

		if (wordResults.IsSimilar)
		{
			HideIncorrectTokens = true;
			TokensResult = [.. wordResults.Tokens];
		}
	}

	async void SpeakPreviousEntryThenInitializeNext()
	{
		IsOverlayVisible = true;
		await Task.Delay(TimeSpan.FromMilliseconds(100));
		await SpeakPreviousEntry();
		await Task.Delay(TimeSpan.FromMilliseconds(100));
		InitializeNextEntry();
	}

	async Task SpeakPreviousEntry()
	{
		if (string.IsNullOrWhiteSpace(_vocabularyLanguage))
			return;

		var (text, example) = _speechService.PrepareTextAndExample(_previous.Entry.EnText, _previous.Entry.EnExample);

		if (!string.IsNullOrEmpty(text))
		{
			_synthesisService.Speak(_vocabularyLanguage, text);
		}

		if (!string.IsNullOrEmpty(example))
		{
			await Task.Delay(TimeSpan.FromMilliseconds(300));
			_synthesisService.Speak(_vocabularyLanguage, example);
		}
	}

	void InitializeNextEntry()
	{
		try
		{
			var newEntry = _learnService.GetNextEntry();

			if (newEntry is null)
			{
				_current = EntryProgress.Empty;
				LoadVocabularyList(false);
				SelectedTabIndex = 0;
			}
			else
			{
				_current = newEntry;

				if (newEntry.Session.QueueIndex == 0)
				{
					LoadVocabularyList(false);
				}
			}

			ShowEntry(isNewEntry: true);
			ShowSessionInfo(_current.Session);

			IsOverlayVisible = false;
		}
		catch (Exception ex)
		{
			SW.MessageBox.Show("An error occurred while initializing the next entry.", "Error", SW.MessageBoxButton.OK, SW.MessageBoxImage.Error);
			_logService.LogError(ex, nameof(InitializeNextEntry));
			SelectedTabIndex = 0;
		}
	}
}
