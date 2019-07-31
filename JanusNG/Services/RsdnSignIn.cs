using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rsdn.ApiClient;

namespace Rsdn.JanusNG.Services
{
	internal static class RsdnSignIn
	{
		public static async Task<TokenFactory> SignInAsync(CancellationToken cancellation = default)
		{
			var port = GetFreeTcpPort();
			var redirectUri = $"http://127.0.0.1:{port}/";
			var authenticator = RsdnClientHelpers.CreateAuthenticator(
				//new Uri("https://localhost:44389"),
				new Uri("https://api.rsdn.org"),
				"test_public_client",
				"",
				"offline_access");
			{
				using var httpListener = new HttpListener();
				httpListener.Prefixes.Add(redirectUri);
				httpListener.Start();

				var flowData = authenticator.GetCodeFlowData(redirectUri);
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
					cancellation);

				await responseOutput.FlushAsync(cancellation);
				responseOutput.Close();
				await Task.Delay(500, cancellation); // Wait for browser to get all data
				httpListener.Stop();

				var qs = context.Request.QueryString;
				var redirectParams = qs.AllKeys.ToDictionary(key => key, key => qs[key]);

				// Store token in closure.
				// Actual implementation should use ProtectedData or Windows Credentials Manager to store token on
				// persistent storage
				var token = await authenticator.GetTokenByCodeAsync(flowData, redirectParams, cancellation);
				return authenticator.GetAccessTokenFactory(() => token, t => token = t);
			}
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
	}
}