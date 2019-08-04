using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rsdn.JanusNG.Controls
{
	public class BusyStyleConverter : IValueConverter
	{
		public Style NormalStyle { get; set; }
		public Style BusyStyle { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool b && b ? BusyStyle : NormalStyle;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}