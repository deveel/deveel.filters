using MongoDB.Bson;

namespace Deveel.Filters {
	public static class FilterExtensions {
		public static BsonDocument AsBsonDocument(this Filter filter) {
			var visitor = new BsonFilterVisitor();
			return visitor.BuildDocument(filter);
		}
	}
}
