using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using MemoryLingo.Core.Models;
using MemoryLingo.Core.Services;
using MemoryLingo.Infrastructure.SpeechSynthesis;
using MemoryLingo.Infrastructure.VocabularyReference;
using MemoryLingo.Presentation.Commands;
using SW = System.Windows;

namespace MemoryLingo.Presentation.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
	#region logic properties
	readonly EntryValidationService _entryValidationService;
	readonly ILearnService _learnService;
	readonly ISpeechService _speechService;
	EntryProgress _current = EntryProgress.Empty;
	EntryProgress _previous = EntryProgress.Empty;
	bool _tipsUsedForCurrentEntry;
	readonly DispatcherTimer _validationTimer = new();
	string _vocabularyLanguage = string.Empty;
	#endregion logic properties

	#region UI properties
	public event PropertyChangedEventHandler? PropertyChanged;

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

	public ObservableCollection<WordCheckResult> WordResults
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

	public bool HideIncorrectWords
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

	public ObservableCollection<VocabularyReferenceDto> VocabulariesCollection
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

	public MainWindowViewModel(EntryValidationService entryValidationService, ILearnService learnService, ISpeechService speechService)
	{
		_entryValidationService = entryValidationService;
		_learnService = learnService;
		_speechService = speechService;

		ShowTipsCommand = new RelayCommand(ShowTips);
		AddVocabularyCommand = new RelayCommand(AddVocabulary);
		ReloadVocabulariesCommand = new RelayCommand(ReloadVocabularies);
		DeleteVocabularyCommand = new ParameterizedRelayCommand<VocabularyReferenceDto>(DeleteVocabulary);
		SessionClickCommand = new ParameterizedRelayCommand<SessionClickParameter>(OnSessionClick);
		RefreshSessionCommand = new ParameterizedRelayCommand<SessionClickParameter>(OnRefreshSessionClick);

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

	void ShowTips()
	{
		_validationTimer.Stop();
		_tipsUsedForCurrentEntry = true;
		ShowEntry(isNewEntry: false, showTips: true);
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
			VocabulariesCollection = new ObservableCollection<VocabularyReferenceDto>(vocabularies);
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
			var vocabularyFile = _learnService.AddVocabularyFile(openFileDialog.FileName);
			VocabulariesCollection.Add(vocabularyFile);
			var view = CollectionViewSource.GetDefaultView(VocabulariesCollection);
			view.Refresh();
		}
	}

	void DeleteVocabulary(VocabularyReferenceDto? vocabularyFile)
	{
		if (vocabularyFile == null)
			return;

		_learnService.RemoveVocabularyFile(vocabularyFile.FilePath);
		VocabulariesCollection.Remove(vocabularyFile);
	}

	void UpdateVocabulariesCollectionPreserveSort(IEnumerable<VocabularyReferenceDto> vocabularies)
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

	#region Show/Hide UI elements
	void ShowSessionInfo(EntryProgress.CurrentSessionProgress sessionProgress)
	{
		QueueStat = $"{sessionProgress.QueueIndex + 1}-{sessionProgress.QueueCount}";
		VocabularyStat = $"{sessionProgress.VocabularyLearnedCount}/{sessionProgress.VocabularyEntriesCount}";
	}

	void ShowEntry(bool isNewEntry, bool showTips)
	{
		HideIncorrectWords = false;
		RuText = _current.Entry.RuText;
		RuTip = _current.Entry.RuTip;

		if (isNewEntry)
		{
			Answer = string.Empty;
			_tipsUsedForCurrentEntry = false;
		}

		if (showTips)
		{
			Transcription = WrapTranscription(_current.Entry.Transcription);
			RuExample = _current.Entry.RuExample;
			EnExample = _current.Entry.EnExample;

			var wordResults = _entryValidationService.GetWordCheckResults(_current.Entry.EnText, _current.Entry.EnText);
			WordResults = new ObservableCollection<WordCheckResult>(wordResults);
		}
		else
		{
			Transcription = string.Empty;
			RuExample = string.Empty;
			EnExample = string.Empty;
			WordResults = [];
		}
	}

	void HideEntry()
	{
		HideIncorrectWords = false;
		RuText = string.Empty;
		RuTip = string.Empty;
		Answer = string.Empty;
		Transcription = string.Empty;
		RuExample = string.Empty;
		EnExample = string.Empty;
		WordResults = [];
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

	void StartVocabularySession(VocabularyReferenceDto vocabularyFile, int sessionIndex, bool continueSession)
	{
		var vocabulary = _learnService.StartVocabularySession(vocabularyFile.FilePath, sessionIndex, continueSession);
		if (vocabulary is null)
			return;

		_vocabularyLanguage = vocabulary.Lang;
		FileName = vocabulary.FileName;
		ShowPreviousEntry();
		_current = _learnService.GetFirstEntry();
		ShowEntry(isNewEntry: true, showTips: false);
		ShowSessionInfo(_current.Session);
		SelectedTabIndex = 1;
	}

	void ProcessAnswer(string answer)
	{
		ShowEntry(isNewEntry: false, showTips: false);

		var wordResults = _entryValidationService.GetWordCheckResults(answer, _current.Entry.EnText);

		if (wordResults.All(w => w.IsMatch))
		{
			// is correct if no tips were used
			bool isCorrect = !_tipsUsedForCurrentEntry;

			_previous = _learnService.SaveEntryProgress(_current.Entry.RuText, isCorrect: isCorrect);
			HideEntry();
			ShowPreviousEntry();

			if (isCorrect)
			{
				ShowSessionInfo(_previous.Session);
				var vocabularies = _learnService.GetVocabularyList();
				UpdateVocabulariesCollectionPreserveSort(vocabularies);
			}

			SpeakPreviousEntryThenInitializeNext();
			return;
		}

		if (wordResults.Count(x => !x.IsNonWord) == 1)
		{
			return;
		}

		// if first word is correct or more then 50% words are correct
		if (wordResults.FirstOrDefault(x => x.IsMatch && !x.IsNonWord) is not null
		 	||
			wordResults.Count(x => x.IsMatch && !x.IsNonWord) >= (wordResults.Count(w => !w.IsNonWord) / 2))
		{
			HideIncorrectWords = true;
			WordResults = new ObservableCollection<WordCheckResult>(wordResults);
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

		var enText = _entryValidationService.RemoveTextInBrackets(_previous.Entry.EnText);
		_speechService.Speak(_vocabularyLanguage, enText);

		var enExample = _entryValidationService.RemoveTextInBrackets(_previous.Entry.EnExample);
		if (!string.IsNullOrWhiteSpace(enExample) && !enExample.Equals(enText, StringComparison.OrdinalIgnoreCase))
		{
			await Task.Delay(TimeSpan.FromMilliseconds(300));
			_speechService.Speak(_vocabularyLanguage, _previous.Entry.EnExample);
		}
	}

	void InitializeNextEntry()
	{
		var newEntry = _learnService.GetNextEntry();

		if (newEntry is null)
		{
			_current = EntryProgress.Empty;
			SelectedTabIndex = 0;
		}
		else
		{
			_current = newEntry;
		}

		ShowEntry(isNewEntry: true, showTips: false);
		ShowSessionInfo(_current.Session);

		IsOverlayVisible = false;
	}
}
