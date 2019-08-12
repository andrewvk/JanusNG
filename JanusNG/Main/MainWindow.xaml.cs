using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Rsdn.JanusNG.Main.ViewModel;

namespace Rsdn.JanusNG.Main
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow(IServiceProvider serviceProvider)
		{
			ViewModel = ActivatorUtilities.CreateInstance<MainViewModel>(serviceProvider);
			DataContext = ViewModel;
			InitializeComponent();
			ViewModel.SignedIn += SignedIn;
		}

		private MainViewModel ViewModel { get; }

		private void SignedIn(object sender, EventArgs e)
		{
			// Bring to front
			Activate();
			Topmost = true;  // important
			Topmost = false; // important
			Focus();
		}

		private async void WindowLoaded(object sender, RoutedEventArgs e)
		{
			await ViewModel.Init();
		}
	}
}
