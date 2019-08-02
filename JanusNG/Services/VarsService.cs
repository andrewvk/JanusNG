using System.Collections.Generic;
using System.Linq;
using CodeJam.Collections;

namespace Rsdn.JanusNG.Services
{
	public class VarsService
	{
		private readonly LocalDBFactory _dbFactory;
		private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

		public VarsService(LocalDBFactory dbFactory)
		{
			_dbFactory = dbFactory;
		}

		public string GetVar(string name) =>
			_cache.GetOrAdd(
				name,
				nm =>
				{
					using var db = _dbFactory();
					var col = db.GetCollection<Var>("vars");
					col.EnsureIndex(v => v.ID, true);
					return col.Find(v => v.ID == name).FirstOrDefault()?.Value;
				});

		public void SetVar(string name, string value)
		{
			using var db = _dbFactory();
			var col = db.GetCollection<Var>("vars");
			col.Upsert(new Var {ID = name, Value = value});
			_cache[name] = value;
		}

		private class Var
		{
			public string ID { get; set; }
			public string Value { get; set; }
		}
	}
}