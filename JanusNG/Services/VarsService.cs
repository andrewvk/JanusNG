using System.Linq;

namespace Rsdn.JanusNG.Services
{
	public class VarsService
	{
		private readonly LocalDBFactory _dbFactory;

		public VarsService(LocalDBFactory dbFactory)
		{
			_dbFactory = dbFactory;
		}

		public string GetVar(string name)
		{
			using var db = _dbFactory();
			var col = db.GetCollection<Var>("vars");
			col.EnsureIndex(v => v.ID, true);
			return col.Find(v => v.ID == name).FirstOrDefault()?.Value;
		}

		public void SetVar(string name, string value)
		{
			using var db = _dbFactory();
			var col = db.GetCollection<Var>("vars");
			col.Upsert(new Var {ID = name, Value = value});
		}

		private class Var
		{
			public string ID { get; set; }
			public string Value { get; set; }
		}
	}
}