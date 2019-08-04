using System;
using System.Windows.Input;
using Rsdn.JanusNG.Services.Connection;

namespace Rsdn.JanusNG.Main
{
	public partial class MainViewModel
	{
		private Account _currentAccount;
		private AccountViewModel[] _accounts;
		private Command _signInCommand;
		private Command _signOutCommand;

		public Account CurrentAccount
		{
			get => _currentAccount;
			private set
			{
				_currentAccount = value;
				LoadForumsAsync();
				OnPropertyChanged(nameof(CurrentAccount));
				OnPropertyChanged(nameof(IsSignedIn));
				OnPropertyChanged(nameof(IsNotSignedIn));
				_signOutCommand.OnCanExecuteChanged();
			}
		}

		public AccountViewModel[] Accounts
		{
			get => _accounts;
			private set
			{
				_accounts = value;
				OnPropertyChanged(nameof(Accounts));
			}
		}

		public bool IsSignedIn => CurrentAccount != null;

		public bool IsNotSignedIn => !IsSignedIn;

		public ICommand SignInCommand => _signInCommand;

		public event EventHandler SignedIn;

		public ICommand SignOutCommand => _signOutCommand;

		private void InitCommands()
		{
			_signInCommand = new Command(async () =>
			{
				var acc = await _api.SignInAsync();
				CurrentAccount = acc;
				SignedIn?.Invoke(this, EventArgs.Empty);
			});
			_signOutCommand = new Command(
				() =>
				{
					_api.SignOut();
					CurrentAccount = null;
				},
				() => IsSignedIn);
		}
	}
}