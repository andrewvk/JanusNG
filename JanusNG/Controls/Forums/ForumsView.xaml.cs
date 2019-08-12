using System.Linq;
using System.Windows;
using Rsdn.Api.Models.Forums;
using Rsdn.JanusNG.Main.ViewModel;

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

		public static readonly DependencyProperty SelectedForumProperty =
			DependencyProperty.Register(
				"SelectedForum",
				typeof(ForumDescription),
				typeof(ForumsView),
				new UIPropertyMetadata(SelectedForumChanged));

		public ForumsView()
		{
			InitializeComponent();
		}

		public ForumGroup[] Forums
		{
			get => (ForumGroup[]) GetValue(ForumsProperty);
			set => SetValue(ForumsProperty, value);
		}

		public ForumDescription SelectedForum
		{
			get => (ForumDescription) GetValue(SelectedForumProperty);
			set => SetValue(SelectedForumProperty, value);
		}

		private static void SelectedForumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == e.OldValue
			    || !(e.NewValue is ForumDescription newForum)
			    || e.OldValue != null && (!(e.OldValue is ForumDescription oldForum) || oldForum.ID == newForum.ID))
				return;
			var frmView = ((ForumsView)d);
			var forum = frmView
				.Forums
				.SelectMany(fg => fg.Forums, (fg, f) => new {Group = fg, Forum = f})
				.FirstOrDefault(f => f.Forum.ID == newForum.ID);
			if (forum == null)
				return;
			frmView.ForumsList.SelectItem(forum.Group, forum.Forum);
		}

		private void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			SelectedForum = ForumsList.SelectedItem as ForumDescription;
		}
	}
}
