using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rsdn.Api.Models.Auth;
using Rsdn.ApiClient;
using Rsdn.ApiClient.Auth;

namespace Rsdn.JanusNG.Services.Connection
{
	public class ApiConnectionService
	{
		private readonly AccountsService _accountsService;
		private static readonly Uri _rsdnUri = new Uri("https://api.rsdn.org");
		private readonly RsdnApiAuthenticator _authenticator;
		private AuthTokenResponse _token;

		public RsdnApiClient Client { get; }

		public ApiConnectionService(AccountsService accountsService)
		{
			_accountsService = accountsService;

			_authenticator = RsdnClientHelpers.CreateAuthenticator(
				//new Uri("https://localhost:44389"),
				_rsdnUri,
				"test_public_client",
				"",
				"offline_access");
			_token = _accountsService.GetCurrentToken();
			Client = RsdnClientHelpers.CreateClient(
				_rsdnUri,
				_authenticator.GetAccessTokenFactory(
					() => _token,
					token =>
					{
						accountsService.SetCurrentToken(token);
						_token = token;
					}));
		}

		#region SignIn

		public async Task<Account> SignInAsync(CancellationToken cancellation = default)
		{
			var port = GetFreeTcpPort();
			var redirectUri = $"http://127.0.0.1:{port}/";
			{
				using var httpListener = new HttpListener();
				httpListener.Prefixes.Add(redirectUri);
				httpListener.Start();

				var flowData = _authenticator.GetCodeFlowData(redirectUri);
				OpenBrowser(flowData.AuthUri);

				var context = await httpListener.GetContextAsync();
				var response = context.Response;
				var responseString = "<html><head></head><body>Please return to the app.</body></html>";
				var buffer = Encoding.UTF8.GetBytes(responseString);
				response.ContentLength64 = buffer.Length;
				var responseOutput = response.OutputStream;
				await responseOutput.WriteAsync(
					buffer,
					0,
					buffer.Length,
					default);

				await responseOutput.FlushAsync(default);
				responseOutput.Close();
				await Task.Delay(500, default); // Wait for browser to get all data
				httpListener.Stop();

				var qs = context.Request.QueryString;
				var redirectParams = qs.AllKeys.ToDictionary(key => key, key => qs[key]);

				_token = await _authenticator.GetTokenByCodeAsync(flowData, redirectParams, cancellation);

				var account = await Client.Accounts.GetMeAsync(cancellation);
				var localAccount = new Account
				{
					ID = account.ID,
					DisplayName = account.DisplayName,
					GravatarHash = account.GravatarHash
				};
				_accountsService.AddOrUpdateAccount(localAccount, _token);
				return localAccount;
			}
		}

		public void SignOut()
		{
			_token = null;
			_accountsService.ResetCurrentAccount();
		}

		public Account UseStoredAccount(int id)
		{
			_token = null;
			var (newAcc, newToken) = _accountsService.SetCurrentAccount(id);
			if (newAcc != null)
				_token = newToken;
			return newAcc;
		}

		private static int GetFreeTcpPort()
		{
			var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			var port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}

		private static void OpenBrowser(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					Process.Start("xdg-open", url);
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					Process.Start("open", url);
				else
					throw;
			}
		}
		#endregion
	}
}