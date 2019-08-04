using System.Windows.Input;
using Rsdn.JanusNG.Services.Connection;

namespace Rsdn.JanusNG.Main
{
	public class AccountViewModel
	{
		public AccountViewModel(Account account, ICommand selectAccountCommand)
		{
			Account = account;
			SelectAccountCommand = selectAccountCommand;
		}

		public Account Account { get; }

		public ICommand SelectAccountCommand { get; }
	}
}