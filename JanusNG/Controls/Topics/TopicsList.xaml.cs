using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Rsdn.JanusNG.Main.ViewModel;

namespace Rsdn.JanusNG.Controls.Topics
{
	/// <summary>
	/// Interaction logic for TopicsList.xaml
	/// </summary>
	public partial class TopicsList
	{
		public static readonly DependencyProperty TopicsProperty =
			DependencyProperty.Register(
				nameof(Topics),
				typeof(TopicNode[]),
				typeof(TopicsList),
				new UIPropertyMetadata());

		public static readonly DependencyProperty SelectedMessageProperty =
			DependencyProperty.Register(
				nameof(SelectedMessage),
				typeof(MessageNode),
				typeof(TopicsList),
				new UIPropertyMetadata(SelectedMessageChanged));

		public TopicsList()
		{
			InitializeComponent();
		}

		public TopicNode[] Topics
		{
			get => (TopicNode[]) GetValue(TopicsProperty);
			set => SetValue(TopicsProperty, value);
		}

		public MessageNode SelectedMessage
		{
			get => (MessageNode)GetValue(SelectedMessageProperty);
			set => SetValue(SelectedMessageProperty, value);
		}

		private async void MessageSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var msg = (MessageNode) e.NewValue;
			var oldMsg = (MessageNode) e.OldValue;
			if (msg?.Message.ID == oldMsg?.Message.ID)
				return;
			SelectedMessage = msg;

			await MarkMessageReadAsync(msg);
		}

		private async void TopicExpanded(object sender, RoutedEventArgs e)
		{
			if (!(((TreeViewItem)e.OriginalSource).Header is TopicNode topic) || topic.IsLoaded)
				return;
			await topic.LoadAsync();
		}

		private async Task MarkMessageReadAsync(MessageNode msg)
		{
			if (msg == null || msg.IsRead != false)
				return;
			await Task.Delay(TimeSpan.FromSeconds(2));
			await msg.MarkReadAsync();
		}

		private static void SelectedMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var msg = (MessageNode)e.NewValue;
			var oldMsg = (MessageNode) e.OldValue;
			if (msg?.Message.ID == oldMsg?.Message.ID)
				return;
			var path = new List<object>();

			if (msg == null)
				return;
			if (msg.TopicNode != null)
			{
				var curMsg = msg;
				while (curMsg != null)
				{
					path.Add(curMsg);
					curMsg = curMsg.ParentNode;
				}

				path.Reverse();
			}
			else
				path.Add(msg);

			((TopicsList) d).MessagesList.SelectItem(path.ToArray());
		}
	}
}
