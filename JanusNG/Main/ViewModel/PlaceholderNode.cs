using System.Threading.Tasks;

namespace Rsdn.JanusNG.Main.ViewModel
{
	public class PlaceholderNode : MessageNode
	{
		public PlaceholderNode() : base(null, _ => Task.CompletedTask) 
		{}
	}
}