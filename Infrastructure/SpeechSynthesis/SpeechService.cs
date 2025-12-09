using System.Speech.Synthesis;
using MemoryLingo.Infrastructure.Settings;

namespace MemoryLingo.Infrastructure.SpeechSynthesis;

public interface ISpeechService
{
	void Dispose();
	void Speak(string lang, string text);
}

public class SpeechService : IDisposable, ISpeechService
{
	readonly ISettingsStore _settingsService;
	readonly SettingsDto _settings;
	SpeechSynthesizer _synthesizer = new();
	string _currentLang = string.Empty;
	private bool disposedValue;

	public SpeechService(ISettingsStore settingsService)
	{
		_settingsService = settingsService;
		_settings = _settingsService.Load();
	}

	public void Speak(string lang, string text)
	{
		if (_currentLang != lang)
		{
			if (_settings.Speech.TryGetValue(lang, out var langSettings) && langSettings.IsActive)
			{
				_synthesizer.SelectVoice(langSettings.Voice);
				_synthesizer.Rate = langSettings.Rate;
				_synthesizer.SetOutputToDefaultAudioDevice();
				_currentLang = lang;
			}
		}

		if (_currentLang == lang)
		{
			_synthesizer.Speak(text);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_synthesizer.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
