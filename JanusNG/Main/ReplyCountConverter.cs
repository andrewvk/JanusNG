using System;
using System.Globalization;
using System.Windows.Data;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.Main
{
	public class ReplyCountConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is MessageInfo msg))
				return "";
			return $"{(msg.AnswersCount > 0 ? msg.AnswersCount.ToString() : "")}" +
			       $"{(msg.TopicUnreadCount > 0 ? $"({msg.TopicUnreadCount})" : "")}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}