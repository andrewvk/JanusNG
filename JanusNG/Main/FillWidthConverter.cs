using System;
using System.Globalization;
using System.Windows.Data;

namespace Rsdn.JanusNG.Main
{
	public class FillWidthConverter : IValueConverter
	{
		private const int _maxLevel = 22;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is MessageNode msg))
				return 0;
			return (_maxLevel - msg.Level) * 11;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}