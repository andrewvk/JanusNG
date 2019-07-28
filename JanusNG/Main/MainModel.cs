using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Rsdn.Api.Models.Accounts;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.Main
{
	public class MainModel : INotifyPropertyChanged
	{
		private ForumGroup[] _forums;
		private TopicNode[] _topics;
		private MessageInfo _message;
		private bool _isSignedIn;
		private AccountInfo _me;

		public bool IsSignedIn
		{
			get => _isSignedIn;
			set
			{
				_isSignedIn = value;
				OnPropertyChanged(nameof(IsSignedIn));
				OnPropertyChanged(nameof(IsNotSignedIn));
			}
		}

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

		public MessageInfo Message
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