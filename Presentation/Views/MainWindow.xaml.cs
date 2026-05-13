using System.Windows;
using MemoryLingo.Core.Services;
using MemoryLingo.Infrastructure.Logging;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.SpeechSynthesis;
using MemoryLingo.Infrastructure.WindowsIntegration;
using MemoryLingo.Presentation.ViewModels;
using SWI = System.Windows.Input;

namespace MemoryLingo;

public partial class MainWindow : Window
{
	readonly ISettingsStore _settingsService;
	readonly IWindowSettingsStore _windowSettingsStore;
	readonly ITrayService _trayService;
	public MainWindowViewModel ViewModel { get; }

	public MainWindow(ISettingsStore settingsService, IWindowSettingsStore windowSettingsStore, ITrayService trayService, EntryValidationService entryValidationService, ILearnService learnService, ILogService logService, ISpeechService speechService, ISynthesisService synthesisService)
	{
		InitializeComponent();
		_settingsService = settingsService;
		_windowSettingsStore = windowSettingsStore;
		_trayService = trayService;
		ViewModel = new MainWindowViewModel(entryValidationService, learnService, logService, speechService, synthesisService);
	}

	void Window_Loaded(object sender, RoutedEventArgs e)
	{
		var windowSettings = _windowSettingsStore.Get();
		Top = windowSettings.Top;
		Left = windowSettings.Left;
		Height = windowSettings.Height;
		Width = windowSettings.Width;

		_trayService.Initialize(this);

		ViewModel.Initialize();
		DataContext = ViewModel;

		// Add KeyDown event handler for F1 key
		KeyDown += MainWindow_KeyDown;
	}

	void Window_Closed(object sender, EventArgs e)
	{
		_windowSettingsStore.Save(Top, Left, Height, Width);
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
