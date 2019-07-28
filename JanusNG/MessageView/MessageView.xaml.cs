using System.Windows;
using System.Windows.Input;
using Rsdn.Api.Models.Messages;
using Rsdn.Framework.Formatting.Resources;
using Rsdn.JanusNG.Rates;

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

		private static readonly string _css =
			(string) ResourceProvider.ReadResource("Formatter.css").Read();

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
				$"<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'><style>{_css}</style></head>" +
				$"<body><div class='m'>{((MessageInfo) e.NewValue)?.Body?.Text ?? " "}</div></body>");
		}

		private void RatesClick(object sender, MouseButtonEventArgs e)
		{
			var wnd = new RatesWindow(Message.Rates);
			wnd.Show();
		}
	}
}
