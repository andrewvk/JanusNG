using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.Main.ViewModel
{
	public class MessageNode : INotifyPropertyChanged
	{
		private readonly Func<MessageNode, Task> _readMarker;
		private MessageNode[] _children;
		private bool? _isRead;
		public MessageInfo Message { get; set; }

		public MessageNode(bool? isRead, Func<MessageNode, Task> readMarker)
		{
			_readMarker = readMarker;
			_isRead = isRead;
		}

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

		public async Task MarkReadAsync() => await _readMarker(this);
	}
}