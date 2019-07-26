using System;
using System.Windows;
using Rsdn.ApiClient;

namespace JanusNG
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{

		public MainWindow()
		{
			InitializeComponent();
		}

		private async void SignInClick(object sender, RoutedEventArgs e)
		{
			var tokenFactory = await RsdnSignIn.SignInAsync();
			var client = RsdnClientHelpers.CreateClient(new Uri("https://api.rsdn.org"), tokenFactory);
			var me = await client.Accounts.GetMeAsync();
			MessageBox.Show(this, $"id = {me.ID}, name = {me.DisplayName}, email = {me.Email}");
		}
	}
}
