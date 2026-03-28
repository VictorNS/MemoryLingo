using System.IO.Pipes;
using SW = System.Windows;

namespace MemoryLingo.Infrastructure.WindowsIntegration;

public interface ISingleInstanceService : IDisposable
{
	bool TryClaimInstance();
	void StartListening();
	void NotifyFirstInstance();
}

public sealed class SingleInstanceService : ISingleInstanceService
{
	const string MutexName = "MemoryLingo_SingleInstance_Mutex";
	const string PipeName = "MemoryLingo_Activation";

	readonly ITrayService _trayService;
	Mutex? _mutex;
	CancellationTokenSource? _cts;
	bool _disposed;

	public SingleInstanceService(ITrayService trayService)
	{
		_trayService = trayService;
	}

	public bool TryClaimInstance()
	{
		_mutex = new Mutex(initiallyOwned: true, MutexName, out bool createdNew);

        if (!createdNew)
            _mutex.Dispose();

        return createdNew;
	}

	public void StartListening()
	{
		_cts = new CancellationTokenSource();
		_ = ListenAsync(_cts.Token);
	}

	async Task ListenAsync(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			try
			{
				using var pipe = new NamedPipeServerStream(
					PipeName, PipeDirection.In, 1,
					PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				await pipe.WaitForConnectionAsync(ct);
				SW.Application.Current.Dispatcher.Invoke(_trayService.ShowWindow);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch
			{
				// Continue listening on transient errors
			}
		}
	}

	public void NotifyFirstInstance()
	{
		try
		{
			using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
			pipe.Connect(timeout: 500);
			pipe.WriteByte(1);
		}
		catch
		{
			// First instance may not be listening yet or has already exited
		}
	}

	public void Dispose()
	{
        if (_disposed)
            return;

		_disposed = true;
		_cts?.Cancel();
		_cts?.Dispose();
		_mutex?.Dispose();
	}
}
