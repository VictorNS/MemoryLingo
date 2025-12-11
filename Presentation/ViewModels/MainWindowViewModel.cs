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
	readonly LearnService _learnService;
	readonly ISpeechService _speechService;
	EntryProgress _current = EntryProgress.Empty;
	EntryProgress _previous = EntryProgress.Empty;
	bool _tipsUsedForCurrentEntry;
	readonly DispatcherTimer _validationTimer = new();
	string _vocabularyLanguage = string.Empty;
	#endregion logic properties

	#region UI properties
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
	{
		if (EqualityComparer<T>.Default.Equals(backingStore, value))
			return false;

		backingStore = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	string _ruText = string.Empty;
	public string RuText
	{
		get => _ruText;
		set => SetProperty(ref _ruText, value);
	}

	string _ruTip = string.Empty;
	public string RuTip
	{
		get => _ruTip;
		set => SetProperty(ref _ruTip, value);
	}

	string _transcription = string.Empty;
	public string Transcription
	{
		get => _transcription;
		set => SetProperty(ref _transcription, value);
	}

	string _answer = string.Empty;
	public string Answer
	{
		get => _answer;
		set
		{
			if (SetProperty(ref _answer, value))
			{
				if (string.IsNullOrWhiteSpace(_answer))
					return;
				RestartValidationTimer();
			}
		}
	}

	string _ruExample = string.Empty;
	public string RuExample
	{
		get => _ruExample;
		set => SetProperty(ref _ruExample, value);
	}

	string _enExample = string.Empty;
	public string EnExample
	{
		get => _enExample;
		set => SetProperty(ref _enExample, value);
	}

	string _fileName = string.Empty;
	public string FileName
	{
		get => _fileName;
		set => SetProperty(ref _fileName, value);
	}

	string _prevRuText = string.Empty;
	public string PrevRuText
	{
		get => _prevRuText;
		set => SetProperty(ref _prevRuText, value);
	}

	string _prevRuTip = string.Empty;
	public string PrevRuTip
	{
		get => _prevRuTip;
		set => SetProperty(ref _prevRuTip, value);
	}

	string _prevTranscription = string.Empty;
	public string PrevTranscription
	{
		get => _prevTranscription;
		set => SetProperty(ref _prevTranscription, value);
	}

	string _prevEnText = string.Empty;
	public string PrevEnText
	{
		get => _prevEnText;
		set => SetProperty(ref _prevEnText, value);
	}

	string _prevRuExample = string.Empty;
	public string PrevRuExample
	{
		get => _prevRuExample;
		set => SetProperty(ref _prevRuExample, value);
	}

	string _prevEnExample = string.Empty;
	public string PrevEnExample
	{
		get => _prevEnExample;
		set => SetProperty(ref _prevEnExample, value);
	}

	string _prevStatusImage = "/Presentation/Assets/Images/question-mark-16.png";
	public string PrevStatusImage
	{
		get => _prevStatusImage;
		set => SetProperty(ref _prevStatusImage, value);
	}

	string _prevStatus = string.Empty;
	public string PrevStatus
	{
		get => _prevStatus;
		set => SetProperty(ref _prevStatus, value);
	}

	string _queueStat = string.Empty;
	public string QueueStat
	{
		get => _queueStat;
		set => SetProperty(ref _queueStat, value);
	}

	string _vocabularyStat = string.Empty;
	public string VocabularyStat
	{
		get => _vocabularyStat;
		set => SetProperty(ref _vocabularyStat, value);
	}

	ObservableCollection<WordCheckResult> _wordResults = [];
	public ObservableCollection<WordCheckResult> WordResults
	{
		get => _wordResults;
		set => SetProperty(ref _wordResults, value);
	}

	bool _hideIncorrectWords = false;
	public bool HideIncorrectWords
	{
		get => _hideIncorrectWords;
		set => SetProperty(ref _hideIncorrectWords, value);
	}

	bool _isOverlayVisible = false;
	public bool IsOverlayVisible
	{
		get => _isOverlayVisible;
		set => SetProperty(ref _isOverlayVisible, value);
	}

	int _selectedTabIndex = 0;
	public int SelectedTabIndex
	{
		get => _selectedTabIndex;
		set => SetProperty(ref _selectedTabIndex, value);
	}

	ObservableCollection<VocabularyReferenceDto> _vocabulariesCollection = [];
	public ObservableCollection<VocabularyReferenceDto> VocabulariesCollection
	{
		get => _vocabulariesCollection;
		set => SetProperty(ref _vocabulariesCollection, value);
	}

	static string WrapTranscription(string t) => t.StartsWith('[') ? t : $"[{t}]";
	#endregion UI properties

	public MainWindowViewModel(EntryValidationService entryValidationService, LearnService learnService, ISpeechService speechService)
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
			ProcessAnswer(_answer);
		};
	}

	public void Initialize()
	{
		LoadVocabularyList(forceReloadSession: false);
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
		LoadVocabularyList(forceReloadSession: true);
	}

	void LoadVocabularyList(bool forceReloadSession)
	{
		var vocabularies = _learnService.LoadVocabularyList(forceReloadSession);
		VocabulariesCollection = new ObservableCollection<VocabularyReferenceDto>(vocabularies);
		var view = CollectionViewSource.GetDefaultView(VocabulariesCollection);
		view.SortDescriptions.Clear();
		view.SortDescriptions.Add(new SortDescription("LastSessionLocalTime", ListSortDirection.Descending));
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
			WordResults.Clear();
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
		WordResults.Clear();
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
			: _previous.IsLastAttemptSuccess
				? "check-16.png"
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
				VocabulariesCollection = new ObservableCollection<VocabularyReferenceDto>(vocabularies);
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
