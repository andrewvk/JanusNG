using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Rsdn.Api.Models.Messages;
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
			var selectedIDs = FullMsgID.TryParse(_varsService.GetVar(_curSelectionVar));
			await SelectMessage(selectedIDs);
		}

		private async Task SelectMessage(FullMsgID ids)
		{
			if (ids == null)
				return;
			ForumsList.SelectForum(ids.ForumID);
			if (!ids.TopicID.HasValue || !ids.MessageID.HasValue)
				return;
			//if (ViewModel.Topics == null)
			//	await LoadTopicsAsync(ids.ForumID);
			var topic = ViewModel.Topics?.FirstOrDefault(t => t.Message.ID == ids.TopicID);
			if (topic == null)
				return;
			var path = new List<MessageNode> { topic };

			if (ids.TopicID != ids.MessageID)
			{
				await LoadRepliesAsync(topic);
				MessageNode FindMsg(int targetId, MessageNode node) =>
					node.Message.ID == targetId
						? node
						: node.Children.Select(cn => FindMsg(targetId, cn)).FirstOrDefault(fn => fn != null);

				var msg = FindMsg(ids.MessageID.Value, topic);
				if (msg != null)
				{
					path.Clear();
					while (msg != null)
					{
						path.Add(msg);
						msg = msg.ParentNode;
					}
					path.Reverse();
				}
			}

			ItemsControl container = MessagesList;
			TreeViewItem tvi = null;
			foreach (var node in path)
			{
				if (container.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
					container.UpdateLayout();
				tvi = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(node);
				if (tvi == null)
					return;
				tvi.IsExpanded = true;
				container = tvi;
			}

			if (tvi != null)
				tvi.IsSelected = true;
		}

		private async void MessageSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
		{
			var msg = (MessageNode)MessagesList.SelectedItem;
			if (msg != null)
				using (MessageView.ApplyLoader())
				{
					msg.Message =
						await _api.Client.Messages.GetMessageAsync(
							msg.Message.ID,
							withRates: true,
							withBodies: true,
							formatBody: true);
					_varsService.SetVar(
						_curSelectionVar,
						new FullMsgID(ViewModel.SelectedForum.ID, msg.TopicNode?.Message.ID, msg.Message.ID).ToString());
				}
			ViewModel.Message = msg;
			if (ViewModel.IsSignedIn && ViewModel.Message?.IsRead != true)
#pragma warning disable CS4014
				Task.Run(() => MarkMessageRead(msg));
#pragma warning restore CS4014
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
