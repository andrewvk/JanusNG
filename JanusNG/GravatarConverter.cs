using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Data;

namespace Rsdn.JanusNG
{
	public class GravatarConverter : IValueConverter
	{
		public int Size { get; set; }

		/// <summary>
		/// Retrieves a Gravatar Uri for the current property values.
		/// </summary>
		/// <returns>Returns a new Uri.</returns>
		private Uri GetGravatarUri(string email)
		{
			// Reference: http://en.gravatar.com/site/implement/url
			StringBuilder sb = new StringBuilder();

			sb.Append("http://www.gravatar.com/avatar/");
			sb.Append(Md5EncodeText(email ?? string.Empty));
			sb.Append(".jpg");

			// Size
			sb.Append("?s=");
			int width = Size;
			sb.Append(width);

			return new Uri(sb.ToString());
		}

		/// <summary>
		/// Retrieves the MD5 encoded hash string of the provided input.
		/// </summary>
		/// <param name="text">The input text.</param>
		/// <returns>Returns the MD5 hash string of the input string.</returns>
		private string Md5EncodeText(string text)
		{
			StringBuilder sb = new StringBuilder();

			byte[] ss = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
			foreach (byte b in ss)
				sb.Append(b.ToString("X2"));
			return sb.ToString().ToLower();
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return GetGravatarUri(value?.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}