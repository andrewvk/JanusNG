using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Rsdn.Api.Models.Forums;
using Rsdn.Api.Models.Messages;
using Rsdn.JanusNG.Services.Connection;

namespace Rsdn.JanusNG.Main
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly ApiConnectionService _api;
		private readonly AccountsService _accountsService;

		public MainWindow(ApiConnectionService api, AccountsService accountsService)
		{
			_api = api;
			_accountsService = accountsService;
			InitializeComponent();
			Model.Accounts = _accountsService.GetAccounts();
			Model.CurrentAccount = _accountsService.GetCurrentAccount();
		}

		private async void SignInClick(object sender, RoutedEventArgs e)
		{
			var acc = await _api.SignInAsync();
			// Bring to front
			Activate();
			Topmost = true;  // important
			Topmost = false; // important
			Focus();

			Model.CurrentAccount = acc;
			Model.Accounts = _accountsService.GetAccounts();
			await ReloadModel();
		}

		private async void WindowLoaded(object sender, RoutedEventArgs e)
		{
			await ReloadModel();
		}

		private async Task ReloadModel()
		{
			var selectedForumID = (ForumsList.SelectedItem as ForumDescription)?.ID;
			await RefreshForumsAsync();
			if (selectedForumID != null)
			{
				var forum = Model.Forums.SelectMany(fg => fg.Forums).FirstOrDefault(f => f.ID == selectedForumID);
				if (forum != null)
					if (ForumsList.ItemContainerGenerator.ContainerFromItem(forum) is TreeViewItem tvi)
						tvi.IsSelected = true;
			}
		}

		private async Task RefreshForumsAsync()
		{
			using (ForumsList.ApplyLoader())
				Model.Forums = await LoadForumsAsync();
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

		private async Task<ForumGroup[]> LoadForumsAsync() =>
			(await _api.Client.Forums.GetForumsAsync())
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
			(await _api.Client.Messages.GetMessagesAsync(
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
						? await _api.Client.Messages.GetMessageAsync(msg.Message.ID, withRates: true, withBodies: true, formatBody: true)
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
			var messageMap = (await _api.Client.Messages.GetMessagesAsync(topicID: topic.Message.ID, withRates: true, withReadMarks: true))
				.Items
				.GroupBy(m => m.ParentID)
				.ToDictionary(grp => grp.Key, grp => grp.ToArray());
			topic.Children = BuildTopicTree(messageMap, topic.Message.ID, 0, topic);
			topic.IsLoaded = true;
		}

		private static MessageNode[] BuildTopicTree(
			IReadOnlyDictionary<int, MessageInfo[]> messageMap,
			int parentID,
			int level,
			TopicNode topicNode)
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
			await _api
				.Client
				.ReadMarks
				.AddReadMarksAsync(new[] {msg.Message.ID});
			msg.IsRead = true;
			if (msg.TopicNode != null)
				lock (msg.TopicNode)
					msg.TopicNode.TopicUnreadCount = msg.TopicNode.TopicUnreadCount - 1;
		}

		private async void SignOutClick(object sender, RoutedEventArgs e)
		{
			_api.SignOut();
			Model.CurrentAccount = null;
			await RefreshForumsAsync();
		}

		private async void AccountClicked(object sender, RoutedEventArgs e)
		{
			var account = (Account) ((MenuItem) sender).DataContext;
			Model.CurrentAccount = _api.UseStoredAccount(account.ID);
			await RefreshForumsAsync();
		}
	}
}
