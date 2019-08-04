using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Rsdn.JanusNG.Main.ViewModel;

namespace Rsdn.JanusNG.Main
{
	public class ReplyCountConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(values[0] is MessageNode msg))
				return null;

			if (values[0] is PlaceholderNode)
				return null;

			int? CalcUnread(MessageNode m) => m.Children?.Sum(cm => (cm.IsRead != true ? 1 : 0) + CalcUnread(cm));

			var replyCount = msg is TopicNode topic
				? topic.TopicUnreadCount
				: CalcUnread(msg);
			return $"{(msg.Message.AnswersCount > 0 ? msg.Message.AnswersCount.ToString() : "")}" +
			       $"{(replyCount > 0 ? $"({replyCount})" : "")}";
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}