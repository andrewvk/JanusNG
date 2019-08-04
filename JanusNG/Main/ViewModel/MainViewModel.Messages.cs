using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.Main.ViewModel
{
	public partial class MainViewModel
	{
		private MessageNode _message;

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

		private bool _topicsLoading;

		public bool TopicsLoading
		{
			get => _topicsLoading;
			set
			{
				_topicsLoading = value;
				OnPropertyChanged(nameof(TopicsLoading));
			}
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
				_varsService.SetVar(
					_curSelectionVar,
					 SelectedForum == null ? null : new FullMsgID(SelectedForum.ID, value?.TopicNode?.Message.ID, value?.Message.ID).ToString());
				LoadMessage(value);
			}
		}

		private async void LoadMessage(MessageNode msg)
		{
			if (msg != null)
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
			}

			OnPropertyChanged(nameof(Message));
		}
	}
}