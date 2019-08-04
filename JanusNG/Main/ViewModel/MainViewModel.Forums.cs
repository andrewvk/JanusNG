using System.Linq;
using System.Threading.Tasks;
using Rsdn.Api.Models.Forums;

namespace Rsdn.JanusNG.Main.ViewModel
{
	public partial class MainViewModel
	{
		private ForumDescription _selectedForum;
		private bool _forumsLoading;

		public bool ForumsLoading
		{
			get => _forumsLoading;
			set
			{
				_forumsLoading = value;
				OnPropertyChanged(nameof(ForumsLoading));
			}
		}

		public ForumGroup[] Forums { get; private set; }

		private async Task LoadForumsAsync()
		{
			ForumsLoading = true;
			try
			{
				Forums =
					(await _api.Client.Forums.GetForumsAsync())
					.Where(f => !f.IsService || f.Code == "test")
					.OrderBy(f => f.Name)
					.GroupBy(
						f => f.ForumGroup.ID,
						f => f,
						(gid, grp) =>
						{
							var res = new ForumGroup
							{
								Forums = grp.ToArray(),
							};
							var fg = res.Forums.First().ForumGroup;
							res.Name = fg.Name;
							res.SortOrder = fg.SortOrder;
							return res;
						})
					.OrderBy(g => g.SortOrder)
					.ToArray();
			}
			finally
			{
				ForumsLoading = false;
			}

			OnPropertyChanged(nameof(Forums));
		}

		public ForumDescription SelectedForum
		{
			get => _selectedForum;
			set => SetSelectedForumAsync(value);
		}

		private async void SetSelectedForumAsync(ForumDescription forum)
		{
			await LoadTopicsAsync(forum?.ID);
			if (forum != null)
				_varsService.SetVar(
					_curSelectionVar,
					new FullMsgID(forum.ID, null, null).ToString());
			_selectedForum = forum;
			OnPropertyChanged(nameof(SelectedForum));
		}
	}
}