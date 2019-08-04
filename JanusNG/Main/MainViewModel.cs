using System;
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
			LoadForumsAsync();
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

		private async void LoadForumsAsync()
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
			_varsService.SetVar(
				_curSelectionVar,
				forum != null ? new FullMsgID(forum.ID, null, null).ToString() : null);
			_selectedForum = forum;
			OnPropertyChanged(nameof(SelectedForum));
		}

		private async Task LoadTopicsAsync(int? forumID)
		{
			if (forumID.HasValue)
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

		public MessageNode Message
		{
			get => _message;
			set
			{
				_message = value;
				OnPropertyChanged(nameof(Message));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}