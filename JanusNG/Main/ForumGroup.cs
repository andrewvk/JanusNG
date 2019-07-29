using Rsdn.Api.Models.Forums;

namespace Rsdn.JanusNG.Main
{
	public class ForumGroup
	{
		public string Name { get; set; }
		public ForumDescription[] Forums { get; set; }
		public int SortOrder { get; set; }
	}
}