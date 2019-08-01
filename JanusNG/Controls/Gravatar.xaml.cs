using System.Windows;

namespace Rsdn.JanusNG.Controls
{
	/// <summary>
	/// Interaction logic for Gravatar.xaml
	/// </summary>
	public partial class Gravatar
	{
		public static readonly DependencyProperty SizeProperty =
			DependencyProperty.Register(
				"Size",
				typeof(int),
				typeof(Gravatar),
				new UIPropertyMetadata());

		public static readonly DependencyProperty HashProperty =
			DependencyProperty.Register(
				"Hash",
				typeof(string),
				typeof(Gravatar),
				new UIPropertyMetadata());

		public int Size { get; set; }

		public string Hash { get; set; }

		public Gravatar()
		{
			InitializeComponent();
		}
	}
}
