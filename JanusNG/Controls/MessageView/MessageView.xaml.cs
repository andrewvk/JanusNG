using System.Windows;
using System.Windows.Input;
using Rsdn.Framework.Formatting.Resources;
using Rsdn.JanusNG.Main;
using Rsdn.JanusNG.Rates;

namespace Rsdn.JanusNG.Controls.MessageView
{
	/// <summary>
	/// Interaction logic for MessageView.xaml
	/// </summary>
	public partial class MessageView
	{
		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register(
				"Message",
				typeof(MessageNode),
				typeof(MessageView),
				new UIPropertyMetadata(null, ChangeCallback));

		private static readonly string _css =
			(string) ResourceProvider.ReadResource("Formatter.css").Read();

		public MessageView()
		{
			InitializeComponent();
		}

		public MessageNode Message
		{
			get => (MessageNode) GetValue(MessageProperty);
			set => SetValue(MessageProperty, value);
		}

		private static void ChangeCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var msgView = (MessageView)sender;
			msgView.MessageBrowser.NavigateToString(
				$"<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'><style>{_css}</style></head>" +
				$"<body><div class='m'>{((MessageNode) e.NewValue)?.Message?.Body?.Text ?? " "}</div></body>");
		}

		private void RatesClick(object sender, MouseButtonEventArgs e)
		{
			var wnd = new RatesWindow(Message.Message.Rates) {Owner = Window.GetWindow(this)};
			wnd.ShowDialog();
		}
	}
}
