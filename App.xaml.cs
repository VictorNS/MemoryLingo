using System.Windows;
using MemoryLingo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MemoryLingo;

public partial class App : Application
{
	private ServiceProvider? _serviceProvider;

	protected override void OnStartup(StartupEventArgs e)
	{
		var services = new ServiceCollection();

		services.AddTransient<FileReader>();
		services.AddTransient<SettingsService>();
		services.AddTransient<VocabularyService>();
		services.AddTransient<EntryValidationService>();
		services.AddTransient<LearnService>();
		services.AddTransient<MainWindow>();

		_serviceProvider = services.BuildServiceProvider();

		var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		mainWindow.Show();
		base.OnStartup(e);
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_serviceProvider?.Dispose();
		base.OnExit(e);
	}
}
