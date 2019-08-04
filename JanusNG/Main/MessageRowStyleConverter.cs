using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Rsdn.JanusNG.Main.ViewModel;

namespace Rsdn.JanusNG.Main
{
	public class MessageRowStyleConverter : IMultiValueConverter
	{
		public Style ReadStyle { get; set; }

		public Style UnreadStyle { get; set; }

		public Style RepliesUnreadStyle { get; set; }

		public Style PlaceholderStyle { get; set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length == 0 || !(values[0] is MessageNode msg))
				return ReadStyle;

			if (values[0] is PlaceholderNode)
				return PlaceholderStyle;

			if (msg.IsRead == false)
				return UnreadStyle;

			if (msg is TopicNode topic)
				return topic.TopicUnreadCount > 0 ? RepliesUnreadStyle : ReadStyle;

			bool HasUnreadReplies(MessageNode m) =>
				m.Children.Any(cm => cm.IsRead == false || HasUnreadReplies(cm));

			return
				HasUnreadReplies(msg)
					? RepliesUnreadStyle
					: ReadStyle;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}