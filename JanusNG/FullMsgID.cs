using CodeJam.Strings;
using JetBrains.Annotations;

namespace Rsdn.JanusNG
{
	public class FullMsgID
	{
		public FullMsgID(int forumID, int? topicID, int? messageID)
		{
			ForumID = forumID;
			TopicID = topicID;
			MessageID = messageID;
		}

		public int ForumID { get; }
		public int? TopicID { get; }
		public int? MessageID { get; }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override string ToString() => $"{ForumID}:{TopicID}:{MessageID}";

		[CanBeNull]
		public static FullMsgID TryParse(string str)
		{
			if (str.IsNullOrWhiteSpace())
				return null;
			var parts = str.Split(':');
			if (parts.Length != 3)
				return null;
			return new FullMsgID(
				int.Parse(parts[0]),
				int.TryParse(parts[1], out var tid) ? tid : (int?) null,
				int.TryParse(parts[2], out var mid) ? mid : (int?) null);
		}
	}
}