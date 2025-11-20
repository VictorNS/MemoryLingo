using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MemoryLingo.Commands;
using MemoryLingo.Models;
using MemoryLingo.Services;
using Microsoft.Win32;

namespace MemoryLingo.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
	#region logic properties
	private readonly EntryValidationService _entryValidationService;
	private readonly LearnService _learnService;
	private EntryProgress _current = EntryProgress.Empty;
	private EntryProgress _previous = EntryProgress.Empty;
	private bool _tipsUsedForCurrentEntry;
	private int _remainingSeconds;
	private readonly DispatcherTimer _validationTimer = new();
	private readonly DispatcherTimer _nextEntryTimer = new();
	#endregion logic properties

	#region UI properties
	private string _ruText = string.Empty;
	private string _ruTip = string.Empty;
	private string _transcription = string.Empty;
	private string _answer = string.Empty;
	private string _ruExample = string.Empty;
	private string _enExample = string.Empty;
	private string _fileName = string.Empty;
	private string _prevText = string.Empty;
	private string _prevStatusImage = "Images/question-mark-16.png";
	private string _prevStatus = string.Empty;
	private string _queueStat = string.Empty;
	private string _vocabularyStat = string.Empty;
	private ObservableCollection<WordCheckResult> _wordResults = [];
	private bool _hideIncorrectWords = false;
	private bool _isOverlayVisible = false;
	private string _countdownMessage = string.Empty;
	private int _selectedTabIndex = 0;
	private ObservableCollection<VocabularyFile> _vocabulariesCollection = [];

	public string RuText
	{
		get => _ruText;
		set => SetProperty(ref _ruText, value);
	}
	public string RuTip
	{
		get => _ruTip;
		set => SetProperty(ref _ruTip, value);
	}
	public string Transcription
	{
		get => _transcription;
		set => SetProperty(ref _transcription, value);
	}
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
	public string RuExample
	{
		get => _ruExample;
		set => SetProperty(ref _ruExample, value);
	}
	public string EnExample
	{
		get => _enExample;
		set => SetProperty(ref _enExample, value);
	}
	public string FileName
	{
		get => _fileName;
		set => SetProperty(ref _fileName, value);
	}
	public string PrevText
	{
		get => _prevText;
		set => SetProperty(ref _prevText, value);
	}
	public string PrevStatusImage
	{
		get => _prevStatusImage;
		set => SetProperty(ref _prevStatusImage, value);
	}
	public string PrevStatus
	{
		get => _prevStatus;
		set => SetProperty(ref _prevStatus, value);
	}
	public string QueueStat
	{
		get => _queueStat;
		set => SetProperty(ref _queueStat, value);
	}
	public string VocabularyStat
	{
		get => _vocabularyStat;
		set => SetProperty(ref _vocabularyStat, value);
	}
	public ObservableCollection<WordCheckResult> WordResults
	{
		get => _wordResults;
		set => SetProperty(ref _wordResults, value);
	}
	public bool HideIncorrectWords
	{
		get => _hideIncorrectWords;
		set => SetProperty(ref _hideIncorrectWords, value);
	}
	public bool IsOverlayVisible
	{
		get => _isOverlayVisible;
		set => SetProperty(ref _isOverlayVisible, value);
	}
	public string CountdownMessage
	{
		get => _countdownMessage;
		set => SetProperty(ref _countdownMessage, value);
	}
	public int SelectedTabIndex
	{
		get => _selectedTabIndex;
		set => SetProperty(ref _selectedTabIndex, value);
	}
	public ObservableCollection<VocabularyFile> VocabulariesCollection
	{
		get => _vocabulariesCollection;
		set => SetProperty(ref _vocabulariesCollection, value);
	}

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
	#endregion UI properties

	public MainWindowViewModel(EntryValidationService entryValidationService, LearnService learnService)
	{
		_entryValidationService = entryValidationService;
		_learnService = learnService;

		ShowTipsCommand = new RelayCommand(ShowTips);
		SkipToNextEntryCommand = new RelayCommand(InitializeNextEntry);
		AddVocabularyCommand = new RelayCommand(AddVocabulary);
		DeleteVocabularyCommand = new ParameterizedRelayCommand<VocabularyFile>(DeleteVocabulary);
		SessionClickCommand = new ParameterizedRelayCommand<SessionClickParameter>(OnSessionClick);

		_validationTimer.Interval = TimeSpan.FromMilliseconds(300);
		_validationTimer.Tick += (s, e) =>
		{
			_validationTimer.Stop();
			ProcessAnswer(_answer);
		};

		_nextEntryTimer.Interval = TimeSpan.FromSeconds(1);
		_nextEntryTimer.Tick += OnNextEntryTimerTick;
	}

	#region events
	public ICommand ShowTipsCommand { get; }
	public ICommand SkipToNextEntryCommand { get; }
	public ICommand AddVocabularyCommand { get; }
	public ICommand DeleteVocabularyCommand { get; }
	public ICommand SessionClickCommand { get; }

	private void ShowTips()
	{
		_tipsUsedForCurrentEntry = true;
		ShowEntry(isNewEntry: false, showTips: true);
	}

	private void RestartValidationTimer()
	{
		_validationTimer.Stop();
		_validationTimer.Start();
	}

	private void StartNextEntryDelay()
	{
		IsOverlayVisible = true;
		_remainingSeconds = 3; // TODO: Get from settings
		CountdownMessage = $"Next entry in {_remainingSeconds} seconds...";
		_nextEntryTimer.Start();
	}

	private void OnNextEntryTimerTick(object? sender, EventArgs e)
	{
		_remainingSeconds--;

		if (_remainingSeconds <= 0)
		{
			InitializeNextEntry();
		}
		else
		{
			CountdownMessage = $"{_remainingSeconds} seconds...";
		}
	}

	private void OnSessionClick(SessionClickParameter? sessionParam)
	{
		if (sessionParam == null)
			return;

		var vocabularyFile = sessionParam.VocabularyFile;

		if (MessageBoxResult.Yes != MessageBox.Show(
			$"Start session {sessionParam.SessionIndex + 1} for '{vocabularyFile.FileName}'?",
			"Confirm Session Start",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question))
			return;

		StartVocabularySession(vocabularyFile, sessionParam.SessionIndex);
	}
	#endregion events

	public void Initialize()
	{
		var vocabularies = _learnService.LoadVocabularyList();
		VocabulariesCollection = new ObservableCollection<VocabularyFile>(vocabularies);
		SelectedTabIndex = 0;
	}

	public void ShowSessionInfo(EntryProgress.CurrentSessionProgress sessionProgress)
	{
		QueueStat = $"{sessionProgress.QueueIndex + 1}-{sessionProgress.QueueCount}";
		VocabularyStat = $"{sessionProgress.VocabularyLearnedCount}/{sessionProgress.VocabularyEntriesCount}";
	}

	public void ShowEntry(bool isNewEntry, bool showTips)
	{
		HideIncorrectWords = false;
		RuText = _current.Entry.RuText;

		if (isNewEntry)
		{
			Answer = string.Empty;
			_tipsUsedForCurrentEntry = false;
		}

		if (showTips)
		{
			RuTip = _current.Entry.RuTip;
			Transcription = _current.Entry.Transcription;
			RuExample = _current.Entry.RuExample;
			EnExample = _current.Entry.EnExample;

			var wordResults = _entryValidationService.GetWordCheckResults(_current.Entry.EnText, _current.Entry.EnText);
			WordResults = new ObservableCollection<WordCheckResult>(wordResults);
		}
		else
		{
			RuTip = string.Empty;
			Transcription = string.Empty;
			RuExample = string.Empty;
			EnExample = string.Empty;
			WordResults.Clear();
		}
	}

	public void ShowPreviousEntry()
	{
		PrevText = _previous.Entry.RuText;
		PrevStatusImage = $"Images/{(_previous.CorrectAnswers == 0 && _previous.TotalAttempts == 0
			? "question-mark-16.png"
			: _previous.IsLastAttemptSuccess
				? "check-16.png"
				: "decrease-16.png")}";
		PrevStatus = $"{_previous.CorrectAnswers}/{_previous.TotalAttempts}";
	}

	private void AddVocabulary()
	{
		var openFileDialog = new OpenFileDialog
		{
			Filter = "Excel files (*.xlsx)|*.xlsx",
			FilterIndex = 1,
			RestoreDirectory = true
		};

		if (openFileDialog.ShowDialog() == true)
		{
			var vocabularyFile = _learnService.AddVocabularyFile(openFileDialog.FileName);
			VocabulariesCollection.Add(vocabularyFile);
		}
	}

	private void DeleteVocabulary(VocabularyFile? vocabularyFile)
	{
		if (vocabularyFile == null)
			return;

		_learnService.RemoveVocabularyFile(vocabularyFile.FilePath);
		VocabulariesCollection.Remove(vocabularyFile);
	}

	private void StartVocabularySession(VocabularyFile vocabularyFile, int sessionNumber)
	{
		var vocabulary = _learnService.StartVocabularySession(vocabularyFile.FilePath, sessionNumber);
		if (vocabulary is null)
			return;

		FileName = vocabulary.FileName;
		ShowPreviousEntry();
		_current = _learnService.GetFirstEntry();
		ShowEntry(isNewEntry: true, showTips: false);
		ShowSessionInfo(_current.Session);
		SelectedTabIndex = 1;
	}

	public void ProcessAnswer(string answer)
	{
		ShowEntry(isNewEntry: false, showTips: false);

		var wordResults = _entryValidationService.GetWordCheckResults(answer, _current.Entry.EnText);

		if (wordResults.All(w => w.IsMatch))
		{
			// is correct if no tips were used
			bool isCorrect = !_tipsUsedForCurrentEntry;

			_previous = _learnService.SaveEntryProgress(_current.Entry.RuText, isCorrect: isCorrect);
			ShowEntry(isNewEntry: false, showTips: true);
			ShowPreviousEntry();

			if (isCorrect)
			{
				ShowSessionInfo(_previous.Session);
				var vocabularies = _learnService.GetVocabularyList();
				VocabulariesCollection = new ObservableCollection<VocabularyFile>(vocabularies);
			}

			StartNextEntryDelay();
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

	public void InitializeNextEntry()
	{
		IsOverlayVisible = false;
		_nextEntryTimer.Stop();

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
	}
}
