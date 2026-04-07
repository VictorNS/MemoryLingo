using System.IO;
using System.Text;

namespace MemoryLingo.Infrastructure.Logging;

public interface ILogService
{
	void LogError(Exception ex, string context);
}

public class LogService : ILogService
{
	readonly string _filePath;

	public LogService()
	{
		_filePath = Path.Combine(DefaultFilesOptions.AppFolder, "errors.log");
	}

	public void LogError(Exception ex, string context)
	{
		try
		{
			Directory.CreateDirectory(DefaultFilesOptions.AppFolder);
			var entry = BuildLogEntry(ex, context);
			File.AppendAllText(_filePath, entry, Encoding.UTF8);
		}
		catch
		{
			// swallow to avoid cascading failures
		}
	}

	static string BuildLogEntry(Exception ex, string context)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] {context}");
		AppendException(sb, ex, depth: 0);
		sb.AppendLine();
		return sb.ToString();
	}

	static void AppendException(StringBuilder sb, Exception ex, int depth)
	{
		var indent = depth == 0 ? string.Empty : new string(' ', depth * 2) + "--- Inner: ";
		sb.AppendLine($"{indent}{ex.GetType().FullName}: {ex.Message}");

		if (!string.IsNullOrWhiteSpace(ex.StackTrace))
		{
			foreach (var line in ex.StackTrace.Split('\n'))
				sb.AppendLine($"  {line.TrimEnd()}");
		}

		if (ex.InnerException is not null)
			AppendException(sb, ex.InnerException, depth + 1);

		if (ex is AggregateException agg)
		{
			foreach (var inner in agg.InnerExceptions)
				AppendException(sb, inner, depth + 1);
		}
	}
}
