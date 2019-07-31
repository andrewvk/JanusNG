namespace Rsdn.JanusNG.Services.Connection
{
	internal class AccountData
	{
		public Account Account { get; set; }
		public string EncryptedToken { get; set; }
		public string Salt { get; set; }
		public int ID { get; set; }
	}
}