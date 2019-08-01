using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CodeJam.Collections;

namespace Rsdn.JanusNG.Controls
{
	public class GravatarConverter : IMultiValueConverter
	{
		// TODO: Add local DB cache with expiration
		private static readonly LazyDictionary<Uri, BitmapImage> _imageCache = new LazyDictionary<Uri, BitmapImage>(
			uri =>
			{
				var bm = new BitmapImage();
				bm.BeginInit();
				bm.UriSource = uri;
				bm.EndInit();
				return bm;
			});

		/// <summary>
		/// Retrieves a Gravatar Uri for the current property values.
		/// </summary>
		/// <returns>Returns a new Uri.</returns>
		private static Uri GetUri(string hash, int size) =>
			new Uri($"http://www.gravatar.com/avatar/{hash ?? ""}?s={size}&d=robohash");

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
			!(values[0] is string hash) || !(values[1] is int size) || size == 0
				? null
				: _imageCache[GetUri(hash, size)];

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => 
			throw new NotImplementedException();
	}
}