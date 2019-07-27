using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JanusNG;
using Rsdn.Api.Models.Forums;
using Rsdn.Api.Models.Messages;
using Rsdn.ApiClient;

namespace Rsdn.JanusNG.Main
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private static readonly Uri _rsdnUri = new Uri("https://api.rsdn.org");

		private RsdnApiClient _rsdnClient;

		public MainWindow()
		{
			InitializeComponent();
			_rsdnClient = RsdnClientHelpers.CreateAnonymousClient(_rsdnUri);
		}

		private async void SignInClick(object sender, RoutedEventArgs e)
		{
			var tokenFactory = await RsdnSignIn.SignInAsync();
			_rsdnClient = RsdnClientHelpers.CreateClient(_rsdnUri, tokenFactory);
			Model.Me = await _rsdnClient.Accounts.GetMeAsync();

			// Bring to front
			Activate();
			Topmost = true;  // important
			Topmost = false; // important
			Focus();

			Model.IsSignedIn = true;
			await ReloadModel();
		}

		private async void WindowLoaded(object sender, RoutedEventArgs e)
		{
			await ReloadModel();
		}

		private async Task ReloadModel()
		{
			Model.Forums = (await _rsdnClient.Forums.GetForumsAsync())
				.OrderBy(f => f.Name)
				.ToArray();
		}

		private async void ForumsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var forum = (ForumDescription)ForumsList.SelectedItem;
			Model.Topics =
				forum != null
				? (await _rsdnClient.Messages.GetMessagesAsync(
					limit: 100,
					forumID: forum.ID,
					onlyTopics: true))
					.Items
				: null;
		}

		private async void MessagesSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var msg = (MessageInfo) MessagesList.SelectedItem;
			Model.Message =
				msg != null
				? await _rsdnClient
					.Messages
					.GetMessageAsync(msg.ID, withRates: true, withBodies: true, formatBody: true)
				: null;
			MessageBrowser.NavigateToString(
				"<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head>"
				+ Model.Message?.Body?.Text ?? " ");
		}
	}
}
