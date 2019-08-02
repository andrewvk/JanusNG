using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.Main
{
	public class MessageNode : INotifyPropertyChanged
	{
		private MessageNode[] _children;
		private bool? _isRead;
		public MessageInfo Message { get; set; }

		public MessageNode[] Children
		{
			get => _children;
			set
			{
				_children = value;
				OnPropertyChanged(nameof(Children));
			}
		}

		public int Level { get; set; }

		public bool? IsRead
		{
			get => _isRead;
			set
			{
				_isRead = value;
				OnPropertyChanged(nameof(IsRead));
				// Notify all parents
				var parent = ParentNode;
				while (parent != null)
				{
					parent.OnPropertyChanged(nameof(IsRead));
					parent = parent.ParentNode;
				}
			}
		}

		public TopicNode TopicNode { get; set; }

		public MessageNode ParentNode { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}