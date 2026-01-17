using System.Windows.Threading;
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
	SingleInstanceGuard? _singleInstanceGuard;

	protected override void OnStartup(SW.StartupEventArgs e)
	{
		_singleInstanceGuard = new SingleInstanceGuard();

		if (!_singleInstanceGuard.IsPrimaryInstance)
		{
			_singleInstanceGuard.SignalPrimaryInstance();
			Shutdown();
			return;
		}

		var services = new ServiceCollection();

		services.AddSingleton<ISettingsStore, SettingsStore>();
		services.AddSingleton<IVocabularyReferenceStore, VocabularyReferenceStore>();
		services.AddSingleton<IVocabularyReferenceService, VocabularyReferenceService>();
		services.AddSingleton<IVocabularyProgressStore, VocabularyProgressStore>();
		services.AddSingleton<IVocabularyExcelReader, VocabularyExcelReader>();
		services.AddSingleton<ITrayService, TrayService>();
		services.AddSingleton<ISpeechService, SpeechService>();
		services.AddSingleton<EntryValidationService>();
		services.AddSingleton<ILearnService, LearnService>();
		services.AddSingleton<MainWindow>();

		_serviceProvider = services.BuildServiceProvider();

		var trayService = _serviceProvider.GetRequiredService<ITrayService>();
		var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		mainWindow.Loaded += (_, _) =>
		{
			mainWindow.Dispatcher.BeginInvoke(() =>
			{
				_singleInstanceGuard?.BeginListening(() => trayService.ShowWindow(), mainWindow.Dispatcher);
			}, DispatcherPriority.Background);
		};
		mainWindow.Show();
		base.OnStartup(e);
	}

	protected override void OnExit(SW.ExitEventArgs e)
	{
		_singleInstanceGuard?.Dispose();
		_serviceProvider?.Dispose();
		base.OnExit(e);
	}
}

sealed class SingleInstanceGuard : IDisposable
{
	const string MUTEX_NAME = "MemoryLingoSingleInstance";
	const string ACTIVATION_EVENT = "MemoryLingoShowWindow";
	readonly Mutex _mutex;
	readonly EventWaitHandle? _activationHandle;
	RegisteredWaitHandle? _registeredWaitHandle;
	bool _disposed;

	public SingleInstanceGuard()
	{
		_mutex = new Mutex(true, MUTEX_NAME, out bool isPrimaryInstance);
		IsPrimaryInstance = isPrimaryInstance;
		if (isPrimaryInstance)
		{
			_activationHandle = new EventWaitHandle(false, EventResetMode.AutoReset, ACTIVATION_EVENT);
		}
		else
		{
			try
			{
				_activationHandle = EventWaitHandle.OpenExisting(ACTIVATION_EVENT);
			}
			catch (WaitHandleCannotBeOpenedException)
			{
				_activationHandle = null;
			}
		}
	}

	public bool IsPrimaryInstance { get; }

	public void BeginListening(Action activationRequested, Dispatcher dispatcher)
	{
		if (!IsPrimaryInstance || _activationHandle == null || _registeredWaitHandle != null)
		{
			return;
		}

		_registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
			_activationHandle,
			(_, timedOut) =>
			{
				if (!timedOut)
				{
					dispatcher.BeginInvoke(activationRequested);
				}
			},
			null,
			-1,
			false);
	}

	public void SignalPrimaryInstance()
	{
		_activationHandle?.Set();
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_registeredWaitHandle?.Unregister(null);
		_activationHandle?.Dispose();
		if (IsPrimaryInstance)
		{
			_mutex.ReleaseMutex();
		}
		_mutex.Dispose();
		_disposed = true;
	}
}
