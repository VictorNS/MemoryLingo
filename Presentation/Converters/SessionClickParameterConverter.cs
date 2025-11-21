using System.Globalization;
using System.Windows.Data;
using MemoryLingo.Infrastructure.VocabularyReference;
using MemoryLingo.Presentation.Commands;

namespace MemoryLingo.Presentation.Converters;

public class SessionClickParameterConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length >= 2 && values[0] is VocabularyReferenceDto vocabularyFile)
		{
			// Handle both string and int session index values
			int sessionIndex;
			if (values[1] is int intValue)
			{
				sessionIndex = intValue;
			}
			else if (values[1] is string stringValue && int.TryParse(stringValue, out int parsedValue))
			{
				sessionIndex = parsedValue;
			}
			else
			{
				return Binding.DoNothing;
			}

			return new SessionClickParameter
			{
				VocabularyFile = vocabularyFile,
				SessionIndex = sessionIndex
			};
		}
		return Binding.DoNothing;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
