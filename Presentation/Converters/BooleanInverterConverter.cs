using System.Globalization;
using System.Windows.Data;

namespace MemoryLingo.Presentation.Converters;

public class BooleanInverterConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool boolValue)
		{
			return !boolValue;
		}
		return true; // Default to enabled if value is not boolean
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool boolValue)
		{
			return !boolValue;
		}
		return false;
	}
}
