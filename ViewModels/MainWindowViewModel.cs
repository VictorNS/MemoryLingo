using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
	private readonly DispatcherTimer _validationTimer = new();
	private readonly DispatcherTimer _nextEntryTimer = new();
	private EntryProgress _current = EntryProgress.Empty;
	private EntryProgress _previous = EntryProgress.Empty;
	private bool _tipsUsedForCurrentEntry;
	private int _remainingSeconds;
	#endregion logic properties

	#region UI properties
	private string _ruText = string.Empty;
	private string _ruTip = string.Empty;
	private string _transcription = string.Empty;
	private string _answer = string.Empty;
	private string _ruExample = string.Empty;
	private string _enExample = string.Empty;
	private string _fileName = string.Empty;
	private string _filePath = string.Empty;
	private string _status = string.Empty;
	private string _prevText = string.Empty;
	private string _prevStatusImage = string.Empty;
	private string _prevStatus = string.Empty;
	private string _vocabularyStat = string.Empty;
	private ObservableCollection<WordCheckResult> _wordResults = [];
	private bool _hideIncorrectWords = false;
	private bool _isOverlayVisible = false;
	private string _countdownMessage = string.Empty;

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
	public string FilePath
	{
		get => _filePath;
		set => SetProperty(ref _filePath, value);
	}
	public string Status
	{
		get => _status;
		set => SetProperty(ref _status, value);
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

		SelectFileCommand = new RelayCommand(SelectFile);
		ShowTipsCommand = new RelayCommand(ShowTips);
		SkipToNextEntryCommand = new RelayCommand(SkipToNextEntry);

		_validationTimer.Interval = TimeSpan.FromMilliseconds(300);
		_validationTimer.Tick += (s, e) =>
		{
			_validationTimer.Stop();
			ShowAnswerProgress(_answer);
		};

		_nextEntryTimer.Interval = TimeSpan.FromSeconds(1);
		_nextEntryTimer.Tick += OnNextEntryTimerTick;
	}

	#region events
	public ICommand SelectFileCommand { get; }
	public ICommand ShowTipsCommand { get; }
	public ICommand SkipToNextEntryCommand { get; }

	private void ShowTips()
	{
		_tipsUsedForCurrentEntry = true;
		ShowEntry(isNewEntry: false, showTips: true);
	}

	private void SelectFile()
	{
		var openFileDialog = new OpenFileDialog
		{
			Filter = "Excel files (*.xlsx)|*.xlsx",
			FilterIndex = 1,
			RestoreDirectory = true
		};

		if (openFileDialog.ShowDialog() == true)
		{
			var vocabulary = _learnService.LoadVocabulary(openFileDialog.FileName);
			ShowVocabularyInfo(vocabulary);

			_current = _learnService.GetFirstEntry();
			ShowEntry(isNewEntry: true, showTips: false);
			ShowSessionInfo(_current.Session);
		}
	}

	private void RestartValidationTimer()
	{
		_validationTimer.Stop();
		_validationTimer.Start();
	}

	private void StartNextEntryDelay()
	{
		IsOverlayVisible = true;
		_remainingSeconds = 5; // TODO: Get from settings
		CountdownMessage = $"Next entry in {_remainingSeconds} seconds...";
		_nextEntryTimer.Start();
	}

	private void OnNextEntryTimerTick(object? sender, EventArgs e)
	{
		_remainingSeconds--;

		if (_remainingSeconds <= 0)
		{
			SkipToNextEntry();
		}
		else
		{
			CountdownMessage = $"{_remainingSeconds} seconds...";
		}
	}

	public void SkipToNextEntry()
	{
		IsOverlayVisible = false;
		_nextEntryTimer.Stop();
		_current = _learnService.GetNextEntry();
		ShowEntry(isNewEntry: true, showTips: false);
		ShowSessionInfo(_current.Session);
	}
	#endregion events

	public void Initialize()
	{
		var vocabulary = _learnService.LoadVocabulary(null);
		ShowVocabularyInfo(vocabulary);

		ShowPreviousEntry();
		_current = _learnService.GetFirstEntry();
		ShowEntry(isNewEntry: true, showTips: false);
		ShowSessionInfo(_current.Session);
	}

	public void ShowVocabularyInfo(Vocabulary vocabulary)
	{
		FileName = vocabulary.FileName;
		FilePath = vocabulary.FilePath;
		Status = vocabulary.Status;
	}

	public void ShowSessionInfo(EntryProgress.SessionProgress sessionProgress)
	{
		VocabularyStat = $"{sessionProgress.StudiedEntriesCount}/{sessionProgress.SessionEntriesCount}/{sessionProgress.TotalEntriesCount}";
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

	public void ShowAnswerProgress(string answer)
	{
		ShowEntry(isNewEntry: false, showTips: false);

		var wordResults = _entryValidationService.GetWordCheckResults(answer, _current.Entry.EnText);

		if (wordResults.All(w => w.IsMatch))
		{
			_previous = _learnService.SaveEntryProgress(_current.Entry.RuText, isCorrect: !_tipsUsedForCurrentEntry);
			ShowEntry(isNewEntry: false, showTips: true);
			ShowPreviousEntry();
			ShowSessionInfo(_previous.Session);
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
}
