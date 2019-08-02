using System;
using System.Globalization;
using System.Windows.Data;

namespace Rsdn.JanusNG
{
	public class DateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is DateTimeOffset date))
				return null;
			return date.ToString("g");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}