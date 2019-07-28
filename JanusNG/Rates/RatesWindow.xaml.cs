using System.Windows;
using Rsdn.Api.Models.Messages;

namespace Rsdn.JanusNG.Rates
{
	/// <summary>
	/// Interaction logic for RatesWindow.xaml
	/// </summary>
	public partial class RatesWindow
	{
		public RatesWindow(MessageRates rates)
		{
			InitializeComponent();
			DataContext = rates;
		}

		private void OKClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
