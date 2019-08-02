using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Rsdn.Api.Models.Accounts;
using Rsdn.Api.Models.Messages;
using Rsdn.JanusNG.Services.Connection;

namespace Rsdn.JanusNG.Main
{
	public class MainModel : INotifyPropertyChanged
	{
		private ForumGroup[] _forums;
		private TopicNode[] _topics;
		private MessageNode _message;
		private AccountInfo _me;
		private Account _currentAccount;
		private Account[] _accounts;

		public Account CurrentAccount
		{
			get => _currentAccount;
			set
			{
				_currentAccount = value;
				OnPropertyChanged(nameof(CurrentAccount));
				OnPropertyChanged(nameof(IsSignedIn));
				OnPropertyChanged(nameof(IsNotSignedIn));
			}
		}

		public Account[] Accounts
		{
			get => _accounts;
			set
			{
				_accounts = value;
				OnPropertyChanged(nameof(Accounts));
			}
		}

		public bool IsSignedIn => CurrentAccount != null;

		public bool IsNotSignedIn => !IsSignedIn;

		public AccountInfo Me
		{
			get => _me;
			set
			{
				_me = value;
				OnPropertyChanged(nameof(Me));
			}
		}

		public ForumGroup[] Forums
		{
			[UsedImplicitly] get => _forums;
			set
			{
				_forums = value;
				OnPropertyChanged(nameof(Forums));
			}
		}

		public TopicNode[] Topics
		{
			get => _topics;
			set
			{
				_topics = value;
				OnPropertyChanged(nameof(Topics));
			}
		}

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