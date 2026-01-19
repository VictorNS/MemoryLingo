using System.Windows;
using MemoryLingo.Core.Services;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.SpeechSynthesis;
using MemoryLingo.Infrastructure.WindowsIntegration;
using MemoryLingo.Presentation.ViewModels;
using SWI = System.Windows.Input;

namespace MemoryLingo;

public partial class MainWindow : Window
{
	readonly ISettingsStore _settingsService;
	readonly ITrayService _trayService;
	public MainWindowViewModel ViewModel { get; }

	public MainWindow(ISettingsStore settingsService, ITrayService trayService, EntryValidationService entryValidationService, ILearnService learnService, ISpeechService speechService)
	{
		InitializeComponent();
		_settingsService = settingsService;
		_trayService = trayService;
		ViewModel = new MainWindowViewModel(entryValidationService, learnService, speechService);
	}

	void Window_Loaded(object sender, RoutedEventArgs e)
	{
		var settings = _settingsService.Get();
		Top = settings.Window.Top;
		Left = settings.Window.Left;
		Height = settings.Window.Height;
		Width = settings.Window.Width;

		if (settings.Speech.Count == 0)
		{
			settings.Speech.Add("en", new SpeechLangSettings { IsActive = true, Voice = "Microsoft David Desktop", Rate = -2 });
			settings.Speech.Add("bg", new SpeechLangSettings { IsActive = true, Voice = "Microsoft Ivan", Rate = -2 });
			_settingsService.Save(settings);
		}

		_trayService.Initialize(this);

		ViewModel.Initialize();
		DataContext = ViewModel;

		// Add KeyDown event handler for F1 key
		KeyDown += MainWindow_KeyDown;
	}

	void Window_Closed(object sender, EventArgs e)
	{
		var settings = _settingsService.Get();
		settings.Window.Top = Top;
		settings.Window.Left = Left;
		settings.Window.Height = Height;
		settings.Window.Width = Width;
		_settingsService.Save(settings);
	}

	void Window_StateChanged(object sender, EventArgs e)
	{
		var settings = _settingsService.Get();

		if (settings.Behavior.MinimizeToTray && WindowState == WindowState.Minimized)
			_trayService.HideWindow();
	}

	void MainWindow_KeyDown(object sender, SWI.KeyEventArgs e)
	{
		if (e.Key == SWI.Key.F1)
		{
			ViewModel.ShowTipsCommand.Execute(null);
			e.Handled = true;
		}
	}
}
