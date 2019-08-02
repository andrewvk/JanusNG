using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CodeJam.Strings;
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
		private const string _curForumVar = "MainForm.CurrentForumID";
		private readonly ApiConnectionService _api;
		private readonly AccountsService _accountsService;
		private readonly VarsService _varsService;

		public MainWindow(ApiConnectionService api, AccountsService accountsService, VarsService varsService)
		{
			_api = api;
			_accountsService = accountsService;
			_varsService = varsService;
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
			var selectedForumID = _varsService.GetVar(_curForumVar);
			await ReloadModel();
			if (selectedForumID.NotNullNorEmpty())
				ForumsList.SelectForum(int.Parse(selectedForumID));
		}

		private async Task ReloadModel()
		{
			await RefreshForumsAsync();
		}

		private async Task RefreshForumsAsync()
		{
			var selectedForumID = ForumsList.SelectedForum?.ID;
			using (ForumsList.ApplyLoader())
				Model.Forums = await LoadForumsAsync();
			if (selectedForumID.HasValue)
				ForumsList.SelectForum(selectedForumID.Value);
		}

		private async void ForumsSelectionChanged(object sender, RoutedEventArgs routedEventArgs)
		{
			if (ForumsList.SelectedForum != null)
			{
				using (MessagesList.ApplyLoader())
					Model.Topics = await LoadTopics(ForumsList.SelectedForum.ID);
				_varsService.SetVar(_curForumVar, ForumsList.SelectedForum.ID.ToString());
			}
			//switch (ForumsList.SelectedItem)
			//{
			//	case ForumDescription f:
			//		using (MessagesList.ApplyLoader())
			//			Model.Topics = await LoadTopics(f.ID);
			//		//var m = Model.Topics.Max(t => t.Message.AnswersCount);
			//		break;
			//	default:
			//		Model.Topics = null;
			//		break;
			//}
		}

		private async Task<ForumGroup[]> LoadForumsAsync() =>
			(await _api.Client.Forums.GetForumsAsync())
			.Where(f => !f.IsService || f.Code == "test")
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
				withReadMarks: true,
				order: MessageOrder.LastAnswerDesc))
			.Items
			.Select(m => new TopicNode
			{
				Message = m,
				IsLoaded = m.AnswersCount == 0,
				Children = m.AnswersCount != 0 ? new MessageNode[]{ new PlaceholderNode() } : Array.Empty<MessageNode>(),
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
