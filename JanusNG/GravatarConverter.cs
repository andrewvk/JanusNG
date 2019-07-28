using System;
using System.Globalization;
using System.Windows.Data;

namespace Rsdn.JanusNG
{
	public class GravatarConverter : IValueConverter
	{
		/// <summary>
		/// Retrieves a Gravatar Uri for the current property values.
		/// </summary>
		/// <returns>Returns a new Uri.</returns>
		private string GetUri(string hash)
		{
			// Reference: http://en.gravatar.com/site/implement/url
			return $"http://www.gravatar.com/avatar/{hash ?? ""}.jpg?s={Size}";
		}

		public int Size { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			GetUri(value?.ToString());

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}