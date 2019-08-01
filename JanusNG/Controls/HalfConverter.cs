using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rsdn.JanusNG.Controls
{
	public class HalfConverter : IValueConverter
	{
		public HalfConverter()
		{}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is int i))
				return 0;
			if (targetType == typeof(double))
				return i / 2f;
			if (targetType == typeof(Point))
				return new Point(i / 2f, i / 2f);
			return 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}