using System.Globalization;
using MemoryLingo.Infrastructure.VocabularyReference;
using MemoryLingo.Presentation.Commands;
using SWD = System.Windows.Data;

namespace MemoryLingo.Presentation.Converters;

public class SessionClickParameterConverter : SWD.IMultiValueConverter
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
				return SWD.Binding.DoNothing;
			}

			return new SessionClickParameter
			{
				VocabularyFile = vocabularyFile,
				SessionIndex = sessionIndex
			};
		}
		return SWD.Binding.DoNothing;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
