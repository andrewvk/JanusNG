using System;
using System.Globalization;
using System.Windows.Data;

namespace Rsdn.JanusNG
{
	public abstract class ExpressionConverter<T> : IValueConverter
	{
		private readonly Func<T, T> _convertFunc;

		protected ExpressionConverter(Func<T, T> convertFunc)
		{
			_convertFunc = convertFunc;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value is T typed ? _convertFunc(typed) : default;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}