using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Rsdn.Api.Models.Forums;
using Rsdn.Api.Models.Messages;
using Rsdn.JanusNG.Services;
using Rsdn.JanusNG.Services.Connection;

namespace Rsdn.JanusNG.Main
{
	[UsedImplicitly]
	public partial class MainViewModel : INotifyPropertyChanged
	{
		private const string _curSelectionVar = "MainForm.CurrentSelectionIDs";

		private readonly ApiConnectionService _api;
		private readonly VarsService _varsService;
		private MessageNode _message;
		private ForumDescription _selectedForum;

		public MainViewModel(ApiConnectionService api, VarsService varsService, AccountsService accountsService)
		{
			_api = api;
			_varsService = varsService;
			InitCommands();
			Accounts = accountsService
				.GetAccounts()
				.Select(a => new AccountViewModel(
					a,
					new Command(() =>
					{
						api.UseStoredAccount(a.ID);
						CurrentAccount = a;
					})))
				.ToArray();
			CurrentAccount = accountsService.GetCurrentAccount();
		}

		public async Task Init()
		{
			await LoadForumsAsync();
			var selectedIDs = FullMsgID.TryParse(_varsService.GetVar(_curSelectionVar));
			if (selectedIDs != null)
			{
				var forum = Forums.SelectMany(fg => fg.Forums).FirstOrDefault(f => f.ID == selectedIDs.ForumID);
				SelectedForum = forum;
				if (selectedIDs.MessageID.HasValue)
				{
					await LoadTopicsAsync(selectedIDs.ForumID);
					var topic = Topics.FirstOrDefault(t =>
						t.Message.ID == selectedIDs.TopicID.GetValueOrDefault(selectedIDs.MessageID.Value));
					if (topic != null)
					{
						await LoadRepliesAsync(topic);
						MessageNode FindMessage(int id, MessageNode m) =>
							m.Message.ID == id ? m : m.Children.Select(mc => FindMessage(id, mc)).FirstOrDefault();
						Message = FindMessage(selectedIDs.MessageID.Value, topic);
					}
				}
			}
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

		private bool _forumsLoading;
		private bool _topicsLoading;

		public bool ForumsLoading
		{
			get => _forumsLoading;
			set
			{
				_forumsLoading = value;
				OnPropertyChanged(nameof(ForumsLoading));
			}
		}


		public ForumGroup[] Forums {get; private set; }

		private async Task LoadForumsAsync()
		{
			ForumsLoading = true;
			try
			{
				Forums =
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
			}
			finally
			{
				ForumsLoading = false;
			}
			OnPropertyChanged(nameof(Forums));
		}

		public bool TopicsLoading
		{
			get => _topicsLoading;
			set
			{
				_topicsLoading = value;
				OnPropertyChanged(nameof(TopicsLoading));
			}
		}

		public ForumDescription SelectedForum
		{
			get => _selectedForum;
			set => SetSelectedForumAsync(value);
		}

		private async void SetSelectedForumAsync(ForumDescription forum)
		{
			await LoadTopicsAsync(forum?.ID);
			if (forum != null)
				_varsService.SetVar(
					_curSelectionVar,
					new FullMsgID(forum.ID, null, null).ToString());
			_selectedForum = forum;
			OnPropertyChanged(nameof(SelectedForum));
		}

		private async Task LoadTopicsAsync(int? forumID)
		{
			if (forumID.HasValue && forumID != SelectedForum?.ID)
			{
				TopicsLoading = true;
				try
				{
					Topics = (await _api.Client.Messages.GetMessagesAsync(
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
							Children = m.AnswersCount != 0 ? new MessageNode[] {new PlaceholderNode()} : Array.Empty<MessageNode>(),
							IsRead = m.IsRead,
							TopicUnreadCount = m.TopicUnreadCount.GetValueOrDefault()
						})
						.ToArray();
				}
				finally
				{
					TopicsLoading = false;
				}
			}
			else
				Topics = null;

			OnPropertyChanged(nameof(Topics));
		}

		public TopicNode[] Topics { get; private set; }
		
		private bool _messageLoading;

		public bool MessageLoading
		{
			get => _messageLoading;
			set
			{
				_messageLoading = value;
				OnPropertyChanged(nameof(MessageLoading));
			}
		}

		public MessageNode Message
		{
			get => _message;
			set
			{
				_message = value;
				if (value != null)
				{
					_varsService.SetVar(
						_curSelectionVar,
						new FullMsgID(SelectedForum.ID, value.TopicNode?.Message.ID, value.Message.ID).ToString());
					LoadMessage(value);
				}
			}
		}

		private async void LoadMessage(MessageNode msg)
		{
			MessageLoading = true;
			try
			{
				msg.Message =
					await _api.Client.Messages.GetMessageAsync(
						msg.Message.ID,
						withRates: true,
						withBodies: true,
						formatBody: true);
			}
			finally
			{
				MessageLoading = false;
			}
			OnPropertyChanged(nameof(Message));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}