using System.Windows;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.MessageView
{
	/// <summary>
	/// Interaction logic for MessageView.xaml
	/// </summary>
	public partial class MessageView
	{
		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register(
				"Message",
				typeof(MessageInfo),
				typeof(MessageView),
				new UIPropertyMetadata(null, ChangeCallback));

		public MessageView()
		{
			InitializeComponent();
		}

		public MessageInfo Message
		{
			get => (MessageInfo) GetValue(MessageProperty);
			set => SetValue(MessageProperty, value);
		}

		private static void ChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var msgView = (MessageView)d;
			msgView.MessageBrowser.NavigateToString(
				"<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head>"
				+ ((MessageInfo) e.NewValue)?.Body?.Text ?? " ");
		}
	}
}
