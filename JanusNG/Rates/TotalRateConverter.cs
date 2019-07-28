using System;
using System.Globalization;
using System.Windows.Data;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.Rates
{
	public class TotalRateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is MessageRate rate))
				throw new NotSupportedException();
			return rate.Rate.HasValue ? $"{rate.Rate} * {rate.RateBase} = {rate.Rate * rate.RateBase}" : "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}