using System;
using System.Threading.Tasks;

namespace Rsdn.JanusNG.Main.ViewModel
{
	public class TopicNode : MessageNode
	{
		private readonly Func<TopicNode, Task> _loader;
		private int _topicUnreadCount;

		public TopicNode(bool? isRead, Func<TopicNode, Task> loader, Func<MessageNode, Task> readMarker) : base(isRead, readMarker)
		{
			_loader = loader;
		}

		public bool IsLoaded { get; set; }

		public int TopicUnreadCount
		{
			get => _topicUnreadCount;
			set
			{
				_topicUnreadCount = value;
				OnPropertyChanged(nameof(TopicUnreadCount));
			}
		}

		public async Task LoadAsync() => await _loader(this);
	}
}