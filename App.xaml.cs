using MemoryLingo.Core.Services;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.SpeechSynthesis;
using MemoryLingo.Infrastructure.VocabularyExcel;
using MemoryLingo.Infrastructure.VocabularyProgress;
using MemoryLingo.Infrastructure.VocabularyReference;
using MemoryLingo.Infrastructure.WindowsIntegration;
using Microsoft.Extensions.DependencyInjection;
using SW = System.Windows;

namespace MemoryLingo;

public partial class App : SW.Application
{
	ServiceProvider? _serviceProvider;

	protected override void OnStartup(SW.StartupEventArgs e)
	{
		var services = new ServiceCollection();

		services.AddSingleton<ISettingsStore, SettingsStore>();
		services.AddSingleton<IVocabularyReferenceStore, VocabularyReferenceStore>();
		services.AddSingleton<IVocabularyProgressStore, VocabularyProgressStore>();
		services.AddSingleton<IVocabularyExcelReader, VocabularyExcelReader>();
		services.AddSingleton<ITrayService, TrayService>();
		services.AddSingleton<ISpeechService, SpeechService>();
		services.AddSingleton<EntryValidationService>();
		services.AddSingleton<LearnService>();
		services.AddSingleton<MainWindow>();

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
