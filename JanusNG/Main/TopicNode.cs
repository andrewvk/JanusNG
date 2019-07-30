namespace Rsdn.JanusNG.Main
{
	public class TopicNode : MessageNode
	{
		private int _topicUnreadCount;

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
	}
}