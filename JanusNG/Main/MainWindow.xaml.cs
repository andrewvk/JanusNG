﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JanusNG;
using Rsdn.Api.Models.Forums;
using Rsdn.Api.Models.Messages;
using Rsdn.ApiClient;

namespace Rsdn.JanusNG.Main
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private static readonly Uri _rsdnUri = new Uri("https://api.rsdn.org");

		private RsdnApiClient _rsdnClient;

		public MainWindow()
		{
			InitializeComponent();
			_rsdnClient = RsdnClientHelpers.CreateAnonymousClient(_rsdnUri);
		}

		private async void SignInClick(object sender, RoutedEventArgs e)
		{
			var tokenFactory = await RsdnSignIn.SignInAsync();
			_rsdnClient = RsdnClientHelpers.CreateClient(_rsdnUri, tokenFactory);
			Model.Me = await _rsdnClient.Accounts.GetMeAsync();

			// Bring to front
			Activate();
			Topmost = true;  // important
			Topmost = false; // important
			Focus();

			Model.IsSignedIn = true;
			await ReloadModel();
		}

		private async void WindowLoaded(object sender, RoutedEventArgs e)
		{
			await ReloadModel();
		}

		private async Task ReloadModel()
		{
			using (ForumsList.ApplyLoader())
				Model.Forums = await LoadForums();
		}

		private async void ForumsSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
		{
			switch (ForumsList.SelectedItem)
			{
				case ForumDescription f:
					using (MessagesList.ApplyLoader())
						Model.Topics = await LoadTopics(f.ID);
					//var m = Model.Topics.Max(t => t.Message.AnswersCount);
					break;
				default:
					Model.Topics = null;
					break;
			}
		}

		private async Task<ForumGroup[]> LoadForums() =>
			(await _rsdnClient.Forums.GetForumsAsync())
			.OrderBy(f => f.Name)
			.GroupBy(
				f => f.ForumGroup.ID,
				f => f,
				(gid, grp) =>
				{
					var res = new ForumGroup
					{
						Forums = grp.ToArray(),
					};
					var fg = res.Forums.First().ForumGroup;
					res.Name = fg.Name;
					res.SortOrder = fg.SortOrder;
					return res;
				})
			.OrderBy(g => g.SortOrder)
			.ToArray();

		private async Task<TopicNode[]> LoadTopics(int forumID) =>
			(await _rsdnClient.Messages.GetMessagesAsync(
				limit: 50,
				forumID: forumID,
				onlyTopics: true,
				withRates: true,
				withReadMarks: true))
			.Items
			.Select(m => new TopicNode
			{
				Message = m,
				IsLoaded = m.AnswersCount == 0,
				Children = m.AnswersCount != 0 ? new []{new MessageNode()} : Array.Empty<MessageNode>(),
				IsRead = m.IsRead,
				TopicUnreadCount = m.TopicUnreadCount.GetValueOrDefault()
			})
			.ToArray();

		private async void MessageSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
		{
			var msg = (MessageNode)MessagesList.SelectedItem;
			using (MessageView.ApplyLoader())
				Model.Message =
					msg?.Message != null
						? await _rsdnClient.Messages.GetMessageAsync(msg.Message.ID, withRates: true, withBodies: true, formatBody: true)
						: null;
			if (Model.IsSignedIn && Model.Message?.IsRead != true)
#pragma warning disable CS4014
				Task.Run(() => MarkMessageRead(msg));
#pragma warning restore CS4014
		}

		private async void TopicExpanded(object sender, RoutedEventArgs e)
		{
			if (!(((TreeViewItem)e.OriginalSource).Header is TopicNode topic) || topic.IsLoaded)
				return;
			var messageMap = (await _rsdnClient.Messages.GetMessagesAsync(topicID: topic.Message.ID, withRates: true, withReadMarks: true))
				.Items
				.GroupBy(m => m.ParentID)
				.ToDictionary(grp => grp.Key, grp => grp.ToArray());
			topic.Children = BuildTopicTree(messageMap, topic.Message.ID, 0, topic);
			topic.IsLoaded = true;
		}

		private MessageNode[] BuildTopicTree(Dictionary<int, MessageInfo[]> messageMap, int parentID, int level, TopicNode topicNode)
		{
			if (!messageMap.TryGetValue(parentID, out var children))
				return Array.Empty<MessageNode>();
			level += 1;
			return children
				.Select(c => new MessageNode
				{
					Message = c,
					Children = BuildTopicTree(messageMap, c.ID, level, topicNode),
					Level = level,
					IsRead = c.IsRead,
					TopicNode = topicNode
				})
				.ToArray();
		}

		private async Task MarkMessageRead(MessageNode msg)
		{
			if (msg.IsRead != false)
				return;
			await Task.Delay(TimeSpan.FromSeconds(3));
			if (Model.Message.ID != msg.Message.ID) // another message selected
				return;
			await _rsdnClient
				.ReadMarks
				.AddReadMarksAsync(new[] {msg.Message.ID});
			msg.IsRead = true;
			if (msg.TopicNode != null)
				lock (msg.TopicNode)
					msg.TopicNode.TopicUnreadCount = msg.TopicNode.TopicUnreadCount - 1;
		}
	}
}
