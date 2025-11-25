using MemoryLingo.Core.Services;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.VocabularyExcel;
using MemoryLingo.Infrastructure.VocabularyProgress;
using MemoryLingo.Infrastructure.VocabularyReference;
using Microsoft.Extensions.DependencyInjection;
using SW = System.Windows;

namespace MemoryLingo;

public partial class App : SW.Application
{
	ServiceProvider? _serviceProvider;

	protected override void OnStartup(SW.StartupEventArgs e)
	{
		var services = new ServiceCollection();

		services.AddTransient<ISettingsStore, SettingsStore>();
		services.AddTransient<IVocabularyReferenceStore, VocabularyReferenceStore>();
		services.AddTransient<IVocabularyProgressStore, VocabularyProgressStore>();
		services.AddTransient<IVocabularyExcelReader, VocabularyExcelReader>();
		services.AddTransient<ITrayService, TrayService>();
		services.AddTransient<EntryValidationService>();
		services.AddTransient<LearnService>();
		services.AddTransient<MainWindow>();

		_serviceProvider = services.BuildServiceProvider();

		var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		mainWindow.Show();
		base.OnStartup(e);
	}

	protected override void OnExit(SW.ExitEventArgs e)
	{
		_serviceProvider?.Dispose();
		base.OnExit(e);
	}
}
