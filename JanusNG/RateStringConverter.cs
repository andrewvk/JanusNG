using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using CodeJam.Strings;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG
{
	public class RateStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(string))
				throw new NotSupportedException();
			if (!(value is MessageRates rates))
				return "";

			IEnumerable<string> GetRateStrings()
			{
				if (rates.LikeCount > 0)
					yield return Enumerable.Range(0, rates.LikeCount).Select(n => ":)").Join();
				if (rates.LikeCount > 0)
					yield return $"{rates.LikeCount}+";
				if (rates.DislikeCount > 0)
					yield return $"{rates.DislikeCount}-";
				if (rates.TotalRate > 0)
					yield return rates.TotalRate.ToString();
			}

			return GetRateStrings().Join("/");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}