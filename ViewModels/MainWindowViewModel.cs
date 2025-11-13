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
	readonly EntryValidationService _entryValidationService;
	readonly LearnService _learnService;
	VocabularyEntryProgress _current;
	private DispatcherTimer? _validationTimer;
	private bool _tipsUsedForCurrentEntry;

	#region properties
	private string _ruText = string.Empty;
	private string _ruTip = string.Empty;
	private string _transcription = string.Empty;
	private string _answer = string.Empty;
	private string _enText = string.Empty;
	private string _ruExample = string.Empty;
	private string _enExample = string.Empty;
	private string _fileName = string.Empty;
	private string _filePath = string.Empty;
	private string _status = string.Empty;
	private string _wordStat = string.Empty;
	private string _vocabularyStat = string.Empty;
	private ObservableCollection<WordCheckResult> _wordResults = [];
	private bool _hideIncorrectWords = false;

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
				// Restart timer for debouncing
				_validationTimer?.Stop();
				_validationTimer?.Start();
			}
		}
	}
	public string EnText
	{
		get => _enText;
		set => SetProperty(ref _enText, value);
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
	public string WordStat
	{
		get => _wordStat;
		set => SetProperty(ref _wordStat, value);
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

	public ICommand SelectFileCommand { get; }
	public ICommand ShowTipsCommand { get; }

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
	#endregion properties

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public MainWindowViewModel(EntryValidationService entryValidationService, LearnService learnService)
	{
		_entryValidationService = entryValidationService;
		_learnService = learnService;

		SelectFileCommand = new RelayCommand(SelectFile);
		ShowTipsCommand = new RelayCommand(ShowTips);

		_validationTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(300)
		};
		_validationTimer.Tick += (s, e) =>
		{
			_validationTimer.Stop();
			ShowAnswerProgress(_answer);
		};
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	#region events
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
		}
	}
	#endregion events

	public void Initialize()
	{
		var vocabulary = _learnService.LoadVocabulary(null);
		ShowVocabularyInfo(vocabulary);

		_current = _learnService.GetFirstEntry();
		ShowEntry(isNewEntry: true, showTips: false);
	}

	public void ShowVocabularyInfo(Vocabulary vocabulary)
	{
		FileName = vocabulary.FileName;
		FilePath = vocabulary.FilePath;
		Status = vocabulary.Status;
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
			EnText = _current.Entry.EnText;
			RuExample = _current.Entry.RuExample;
			EnExample = _current.Entry.EnExample;

			var wordResults = _entryValidationService.GetWordCheckResults(_current.Entry.EnText, _current.Entry.EnText);
			WordResults = new ObservableCollection<WordCheckResult>(wordResults);
		}
		else
		{
			RuTip = string.Empty;
			Transcription = string.Empty;
			EnText = string.Empty;
			RuExample = string.Empty;
			EnExample = string.Empty;
			WordResults.Clear();
		}
	}

	public void ShowAnswerProgress(string answer)
	{
		ShowEntry(isNewEntry: false, showTips: false);

		var wordResults = _entryValidationService.GetWordCheckResults(answer, _current.Entry.EnText);

		if (wordResults.All(w => w.IsMatch))
		{
			ShowEntry(isNewEntry: false, showTips: true);
			_current = _learnService.SaveEntryProgress(_current.Entry.RuText, isCorrect: !_tipsUsedForCurrentEntry);
			var icon = _current.IsLearned ? "✓" : "X";
			WordStat = $"{icon} {_current.CorrectAnswers}/{_current.TotalAttempts}";
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
