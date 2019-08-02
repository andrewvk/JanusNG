using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Rsdn.Api.Models.Forums;
using Rsdn.JanusNG.Main;

namespace Rsdn.JanusNG.Controls.Forums
{
	/// <summary>
	/// Interaction logic for ForumsView.xaml
	/// </summary>
	public partial class ForumsView
	{
		public static readonly DependencyProperty ForumsProperty =
			DependencyProperty.Register(
				"Forums",
				typeof(ForumGroup[]),
				typeof(ForumsView),
				new UIPropertyMetadata());

		//public static readonly DependencyProperty SelectedForumProperty =
		//	DependencyProperty.Register(
		//		"SelectedForum",
		//		typeof(ForumDescription),
		//		typeof(ForumsView),
		//		new UIPropertyMetadata());

		public ForumsView()
		{
			InitializeComponent();
		}

		public ForumGroup[] Forums
		{
			get => (ForumGroup[]) GetValue(ForumsProperty);
			set => SetValue(ForumsProperty, value);
		}

		public ForumDescription SelectedForum => ForumsList.SelectedItem as ForumDescription;

		public event RoutedEventHandler SelectedForumChanged;

		public void SelectForum(int forumID)
		{
			var forum = Forums
				.SelectMany(fg => fg.Forums, (fg, f) => new {Group = fg, Forum = f})
				.FirstOrDefault(f => f.Forum.ID == forumID);
			if (forum == null)
				return;
			var groupTvi = (TreeViewItem)ForumsList.ItemContainerGenerator.ContainerFromItem(forum.Group);
			groupTvi.IsExpanded = true;
			if (groupTvi.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
				ForumsList.UpdateLayout(); // Generate TreeViewItems
			var forumTvi = (TreeViewItem)groupTvi.ItemContainerGenerator.ContainerFromItem(forum.Forum);
			forumTvi.IsSelected = true;
		}

		private void ForumsSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			SelectedForumChanged?.Invoke(this, e);
		}
	}
}
