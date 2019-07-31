using System;
using System.IO;
using System.Windows;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Rsdn.JanusNG.Main;
using Rsdn.JanusNG.Services;
using Rsdn.JanusNG.Services.Connection;

namespace Rsdn.JanusNG
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		private IServiceProvider _serviceProvider;

		protected override void OnStartup(StartupEventArgs e)
		{
			var services = new ServiceCollection();

			ConfigureServices(services);

			_serviceProvider = services.BuildServiceProvider();

			ActivatorUtilities
				.CreateInstance<MainWindow>(_serviceProvider)
				.Show();
		}

		private void ConfigureServices(ServiceCollection services)
		{
			var localFolder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"RSDN/JanusNG/");
			Directory.CreateDirectory(localFolder);
			var localDBPath = Path.Combine(localFolder, "JanusNGData.db");
			services.AddSingleton<LocalDBFactory>(() => new LiteDatabase(localDBPath));
			services.AddSingleton<VarsService>();
			services.AddSingleton<AccountsService>();
			services.AddSingleton<ApiConnectionService>();
		}
	}
}
