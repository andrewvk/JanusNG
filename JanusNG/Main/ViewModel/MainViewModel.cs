using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Rsdn.JanusNG.Services;
using Rsdn.JanusNG.Services.Connection;

namespace Rsdn.JanusNG.Main.ViewModel
{
	[UsedImplicitly]
	public partial class MainViewModel : INotifyPropertyChanged
	{
		private const string _curSelectionVar = "MainForm.CurrentSelectionIDs";

		private readonly ApiConnectionService _api;
		private readonly VarsService _varsService;

		public MainViewModel(ApiConnectionService api, VarsService varsService, AccountsService accountsService)
		{
			_api = api;
			_varsService = varsService;
			InitCommands();
			Accounts = accountsService
				.GetAccounts()
				.Select(a => new AccountViewModel(
					a,
					new Command(() =>
					{
						api.UseStoredAccount(a.ID);
						CurrentAccount = a;
					})))
				.ToArray();
			CurrentAccount = accountsService.GetCurrentAccount();
		}

		public async Task Init()
		{
			await LoadForumsAsync();
			var selectedIDs = FullMsgID.TryParse(_varsService.GetVar(_curSelectionVar));
			if (selectedIDs != null)
			{
				var forum = Forums.SelectMany(fg => fg.Forums).FirstOrDefault(f => f.ID == selectedIDs.ForumID);
				SelectedForum = forum;
				if (selectedIDs.MessageID.HasValue)
				{
					await LoadTopicsAsync(selectedIDs.ForumID);
					var topic = Topics.FirstOrDefault(t =>
						t.Message.ID == selectedIDs.TopicID.GetValueOrDefault(selectedIDs.MessageID.Value));
					if (topic != null)
					{
						await LoadRepliesAsync(topic);
						MessageNode FindMessage(int id, MessageNode m) =>
							m.Message.ID == id ? m : m.Children.Select(mc => FindMessage(id, mc)).FirstOrDefault(mc => mc != null);
						Message = FindMessage(selectedIDs.MessageID.Value, topic);
					}
				}
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}