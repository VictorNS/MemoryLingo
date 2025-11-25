using System.Windows;
using MemoryLingo.Core.Services;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Presentation.ViewModels;
using SWI = System.Windows.Input;

namespace MemoryLingo;

public partial class MainWindow : Window
{
	readonly ISettingsStore _settingsService;
	readonly ITrayService _trayService;
	public MainWindowViewModel ViewModel { get; }

	public MainWindow(ISettingsStore settingsService, ITrayService trayService, EntryValidationService entryValidationService, LearnService learnService)
	{
		InitializeComponent();
		_settingsService = settingsService;
		_trayService = trayService;
		ViewModel = new MainWindowViewModel(entryValidationService, learnService);
	}

	void Window_Loaded(object sender, RoutedEventArgs e)
	{
		var settings = _settingsService.Load();
		Top = settings.Window.Top;
		Left = settings.Window.Left;
		Height = settings.Window.Height;
		Width = settings.Window.Width;

		_trayService.Initialize(this);

		ViewModel.Initialize(settings);
		DataContext = ViewModel;

		// Add KeyDown event handler for F1 key
		KeyDown += MainWindow_KeyDown;
	}

	void Window_Closed(object sender, EventArgs e)
	{
		_trayService.Dispose();
		var settings = _settingsService.Load();
		settings.Window.Top = Top;
		settings.Window.Left = Left;
		settings.Window.Height = Height;
		settings.Window.Width = Width;
		_settingsService.Save(settings);
	}

	void Window_StateChanged(object sender, EventArgs e)
	{
		var settings = _settingsService.Load();

		if (settings.Behavior.MinimizeToTray && WindowState == WindowState.Minimized)
			_trayService.HideWindow();
	}

	void MainWindow_KeyDown(object sender, SWI.KeyEventArgs e)
	{
		if (ViewModel.IsOverlayVisible && (e.Key == SWI.Key.Space || e.Key == SWI.Key.Enter))
		{
			ViewModel.InitializeNextEntry();
			e.Handled = true;
		}
		else if (e.Key == SWI.Key.F1)
		{
			ViewModel.ShowTipsCommand.Execute(null);
			e.Handled = true;
		}
	}
}
