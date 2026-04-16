using System.Text;

namespace MemoryLingo.Core.Services;

public interface ISpeechService
{
	string RemoveUnspeakableSymbols(string text);
	(string text, string example) PrepareTextAndExample(string text, string example);
}

public class SpeechService : ISpeechService
{
	public string RemoveUnspeakableSymbols(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return "";

		var result = new StringBuilder();
		int i = 0;

		while (i < text.Length)
		{
			if (text[i] == '(')
			{
				// Skip to the closing bracket
				while (i < text.Length && text[i] != ')')
					i++;
				if (i < text.Length) // Skip the closing bracket
					i++;

				// If removal creates a double space, collapse to one
				if (result.Length > 0 && result[result.Length - 1] == ' '
					&& i < text.Length && text[i] == ' ')
					i++;
			}
			else
			{
				result.Append(text[i]);
				i++;
			}
		}

		return result.ToString()
			.Replace("/", "").Replace("|", "")
			.Trim();
	}

	public (string text, string example) PrepareTextAndExample(string text, string example)
	{
		text = RemoveUnspeakableSymbols(text);
		example = RemoveUnspeakableSymbols(example);

		if (string.IsNullOrEmpty(text)
			|| (!string.IsNullOrEmpty(example) && example.Contains(text, StringComparison.OrdinalIgnoreCase)))
			return ("", example);

		return (text, example);
	}
}
