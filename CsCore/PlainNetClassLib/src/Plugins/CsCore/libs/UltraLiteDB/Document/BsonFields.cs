using System.Collections.Generic;

namespace UltraLiteDB
{
	public class BsonFields
	{
		private string Field;

		public BsonFields(string field)
		{
			Field = field;
		}
		public IEnumerable<BsonValue> Execute(BsonDocument doc, bool includeNullIfEmpty = true)
		{
			var index = 0;
			BsonValue value=null;
			if(doc.TryGetValue(Field, out value))
			{
				index++;
				yield return value;
			}

			if(index == 0 && includeNullIfEmpty) yield return BsonValue.Null;
		}
	}
}