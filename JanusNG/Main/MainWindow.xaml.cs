using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Rsdn.Api.Models.Messages;
using Rsdn.JanusNG.Controls;
using Rsdn.JanusNG.Services;
using Rsdn.JanusNG.Services.Connection;

namespace Rsdn.JanusNG.Main
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private const string _curSelectionVar = "MainForm.CurrentSelectionIDs";
		private readonly ApiConnectionService _api;
		private readonly VarsService _varsService;

		public MainWindow(
			IServiceProvider serviceProvider,
			ApiConnectionService api,
			VarsService varsService)
		{
			_api = api;
			_varsService = varsService;
			ViewModel = ActivatorUtilities.CreateInstance<MainViewModel>(serviceProvider);
			DataContext = ViewModel;
			InitializeComponent();
			ViewModel.SignedIn += SignedIn;
			ViewModel.PropertyChanged += (s, a) =>
			{
				if (a.PropertyName == nameof(ViewModel.Message))
				{
					SelectMessage(ViewModel.Message);
				}
			};
		}

		private MainViewModel ViewModel { get; }

		private void SignedIn(object sender, EventArgs e)
		{
			// Bring to front
			Activate();
			Topmost = true;  // important
			Topmost = false; // important
			Focus();
		}

		private async void WindowLoaded(object sender, RoutedEventArgs e)
		{
			await ViewModel.Init();
		}

		private void SelectMessage(MessageNode msg)
		{
			if (msg == null)
				return;
			var topic = msg.TopicNode;

			var path = new List<object>();

			if (topic != null)
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

			MessagesList.SelectItem(path.ToArray());

			//ViewModel.Message = msg;
			if (ViewModel.IsSignedIn && ViewModel.Message?.IsRead != true)
#pragma warning disable CS4014
				Task.Run(() => MarkMessageRead(msg));
#pragma warning restore CS4014
		}

		private void MessageSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
		{
			ViewModel.Message = (MessageNode) MessagesList.SelectedItem;
		}

		private async void TopicExpanded(object sender, RoutedEventArgs e)
		{
			if (!(((TreeViewItem)e.OriginalSource).Header is TopicNode topic) || topic.IsLoaded)
				return;
			await LoadRepliesAsync(topic);
		}

		private async Task LoadRepliesAsync(TopicNode topic)
		{
			var messageMap =
				(await _api.Client.Messages.GetMessagesAsync(topicID: topic.Message.ID, withRates: true, withReadMarks: true))
				.Items
				.GroupBy(m => m.ParentID)
				.ToDictionary(grp => grp.Key, grp => grp.ToArray());
			topic.Children = BuildTopicTree(messageMap, topic.Message.ID, 0, topic, topic);
			topic.IsLoaded = true;
		}

		private static MessageNode[] BuildTopicTree(
			IReadOnlyDictionary<int, MessageInfo[]> messageMap,
			int parentID,
			int level,
			TopicNode topicNode,
			MessageNode parentNode)
		{
			if (!messageMap.TryGetValue(parentID, out var children))
				return Array.Empty<MessageNode>();
			level += 1;
			return children
				.Select(c =>
				{
					var res = new MessageNode
					{
						Message = c,
						Level = level,
						IsRead = c.IsRead,
						TopicNode = topicNode,
						ParentNode = parentNode
					};
					res.Children = BuildTopicTree(messageMap, c.ID, level, topicNode, res);
					return res;
				})
				.ToArray();
		}

		private async Task MarkMessageRead(MessageNode msg)
		{
			if (msg == null || msg.IsRead != false)
				return;
			await Task.Delay(TimeSpan.FromSeconds(2));
			if (ViewModel.Message.Message.ID != msg.Message.ID) // another message selected
				return;
			await _api
				.Client
				.ReadMarks
				.AddReadMarksAsync(new[] {msg.Message.ID});
			msg.IsRead = true;
			if (msg.TopicNode != null)
				lock (msg.TopicNode)
					msg.TopicNode.TopicUnreadCount = msg.TopicNode.TopicUnreadCount - 1;
		}
	}
}
