using System.Windows;
using System.Windows.Input;
using MemoryLingo.Services;
using MemoryLingo.ViewModels;

namespace MemoryLingo;

public partial class MainWindow : Window
{
	readonly SettingsService _settingsService;
	public MainWindowViewModel ViewModel { get; }

	public MainWindow(SettingsService settingsService, EntryValidationService entryValidationService, LearnService learnService)
	{
		InitializeComponent();
		_settingsService = settingsService;
		ViewModel = new MainWindowViewModel(entryValidationService, learnService);
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		var settings = _settingsService.LoadSettings();
		Top = settings.Window.Top;
		Left = settings.Window.Left;
		Height = settings.Window.Height;
		Width = settings.Window.Width;

		ViewModel.Initialize();
		DataContext = ViewModel;

		// Add KeyDown event handler for F1 key
		KeyDown += MainWindow_KeyDown;
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		var settings = _settingsService.LoadSettings();
		settings.Window.Top = Top;
		settings.Window.Left = Left;
		settings.Window.Height = Height;
		settings.Window.Width = Width;
		settings.FilePath = ViewModel.FilePath;
		_settingsService.SaveSettings(settings);
	}

	private void MainWindow_KeyDown(object sender, KeyEventArgs e)
	{
		if (ViewModel.IsOverlayVisible && (e.Key == Key.LeftCtrl || e.Key == Key.Space || e.Key == Key.Enter))
		{
			ViewModel.SkipToNextEntry();
			e.Handled = true;
		}
		else if (e.Key == Key.F1 || e.Key == Key.LeftCtrl)
		{
			ViewModel.ShowTipsCommand.Execute(null);
			e.Handled = true;
		}
	}
}
