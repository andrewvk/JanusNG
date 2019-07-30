using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
namespace Rsdn.JanusNG.Main
{
	public class MessageRowStyleConverter : IMultiValueConverter
	{
		public Style ReadStyle { get; set; }

		public Style UnreadStyle { get; set; }

		public Style RepliesUnreadStyle { get; set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length != 2)
				return ReadStyle;
			var isRead = values[0] as bool? != false;
			return
				isRead
					? values[1] as int? > 0
						? RepliesUnreadStyle
						: ReadStyle
					: UnreadStyle;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}