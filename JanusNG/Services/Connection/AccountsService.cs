using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CodeJam;
using JetBrains.Annotations;
using LiteDB;
using Rsdn.Api.Models.Auth;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Rsdn.JanusNG.Services.Connection
{
	public class AccountsService
	{
		private const string _accountsCollection = "accounts";
		private readonly LocalDBFactory _dbFactory;
		private readonly VarsService _varsService;
		private const string _currentAccountVar = "CurrentAccount";

		public AccountsService(LocalDBFactory dbFactory, VarsService varsService)
		{
			_dbFactory = dbFactory;
			_varsService = varsService;
		}

		public void AddOrUpdateAccount(Account account, AuthTokenResponse token, bool asCurrent = true)
		{
			Code.NotNull(account, nameof(account));

			using var db = _dbFactory();
			var col = GetAccountsCol(db);
			//col.EnsureIndex(a => a.ID, true);
			var salt = GenerateSalt();
			col.Upsert(new AccountData
			{
				ID = account.ID,
				Account = account,
				EncryptedToken = EncryptToken(token, salt),
				Salt = Convert.ToBase64String(salt)
			});
			if (asCurrent)
				_varsService.SetVar(_currentAccountVar, account.ID.ToString());
		}

		[CanBeNull]
		public Account GetCurrentAccount() => GetCurrentAccountData()?.Account;

		public (Account account, AuthTokenResponse token) SetCurrentAccount(int id)
		{
			ResetCurrentAccount();
			using var db = _dbFactory();
			var col = GetAccountsCol(db);
			var newAccount = col.Find(a => a.ID == id).FirstOrDefault();
			if (newAccount == null)
				return (null, null);
			_varsService.SetVar(_currentAccountVar, id.ToString());
			return (newAccount.Account, DecryptToken(newAccount.EncryptedToken, newAccount.Salt));
		}

		[CanBeNull]
		public AuthTokenResponse GetCurrentToken()
		{
			var cur = GetCurrentAccountData();
			return cur?.EncryptedToken == null ? null : DecryptToken(cur.EncryptedToken, cur.Salt);
		}

		public void DeleteAccount(int id)
		{
			using var db = _dbFactory();
			GetAccountsCol(db)
				.Delete(a => a.ID == id);
		}

		public void ResetCurrentAccount()
		{
			var id = _varsService.GetVar(_currentAccountVar);
			if (id == null)
				return;
			_varsService.SetVar(_currentAccountVar, null);
		}

		public Account[] GetAccounts()
		{
			using var db = _dbFactory();
			return GetAccountsCol(db)
				.FindAll()
				.Select(ad =>
					new Account
					{
						ID = ad.ID,
						DisplayName = ad.Account.DisplayName,
						GravatarHash = ad.Account.GravatarHash
					})
				.ToArray();
		}

		private AccountData GetCurrentAccountData()
		{
			var id = _varsService.GetVar(_currentAccountVar);
			if (id == null)
				return null;
			using var db = _dbFactory();
			var col = GetAccountsCol(db);
			col.EnsureIndex(a => a.ID, true);
			return col.Find(a => a.ID == int.Parse(id)).FirstOrDefault();
		}

		private byte[] GenerateSalt()
		{
			var rng = RandomNumberGenerator.Create();
			var res = new byte[256/8];
			rng.GetBytes(res);
			return res;
		}

		private string EncryptToken(AuthTokenResponse token, byte[] salt) =>
			Convert.ToBase64String(
				ProtectedData.Protect(
					JsonSerializer.SerializeToUtf8Bytes(token),
					salt,
					DataProtectionScope.CurrentUser));

		private AuthTokenResponse DecryptToken(string encryptedToken, string salt) =>
			JsonSerializer.Deserialize<AuthTokenResponse>(
				Encoding.UTF8.GetString(
					ProtectedData.Unprotect(
						Convert.FromBase64String(encryptedToken),
						Convert.FromBase64String(salt),
						DataProtectionScope.CurrentUser)));

		public void SetCurrentToken(AuthTokenResponse token)
		{
			var id = _varsService.GetVar(_currentAccountVar);
			if (id == null)
				return;
			using var db = _dbFactory();
			var salt = GenerateSalt();
			GetAccountsCol(db)
				.Update(new AccountData
				{
					ID = int.Parse(id),
					EncryptedToken = EncryptToken(token, salt),
					Salt = Convert.ToBase64String(salt)
				});
		}

		private static LiteCollection<AccountData> GetAccountsCol(LiteDatabase db)
		{
			return db.GetCollection<AccountData>(_accountsCollection);
		}
	}
}