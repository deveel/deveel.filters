using System.Text.Json.Nodes;

using MongoDB.Bson;

namespace Deveel.Filters {
    internal static class BsonFilterUtil {
		public static string GetValueTypeString(Type valueType) {
			if (valueType == typeof(DBNull))
				return "null";
			if (valueType == typeof(string))
				return "string";
			if (valueType == typeof(int))
				return "int";
			if (valueType == typeof(long))
				return "long";
			if (valueType == typeof(float))
				return "float";
			if (valueType == typeof(double))
				return "double";
			if (valueType == typeof(bool))
				return "bool";
			if (valueType == typeof(DateTime))
				return "datetime";
			if (valueType == typeof(DateTimeOffset))
				return "datetime2";

			return valueType.FullName!;
		}

		public static DateTimeOffset CreateDateTimeOffset(BsonValue bsonDateTime2) {
            var bsonDateTime = bsonDateTime2.AsBsonDateTime;
            return new DateTimeOffset(bsonDateTime.ToUniversalTime());
        }

		public static BsonValue CreateBsonDateTime2(DateTimeOffset dateTimeOffset) {
			return new BsonDateTime(dateTimeOffset.UtcDateTime);
        }

		public static Type GetTypeFromString(string s) {
			if (s == "null")
				return typeof(DBNull);
			if (s == "string")
				return typeof(string);
			if (s == "int")
				return typeof(int);
			if (s == "long")
				return typeof(long);
			if (s == "float")
				return typeof(float);
			if (s == "double")
				return typeof(double);
			if (s == "bool")
				return typeof(bool);
			if (s == "datetime")
				return typeof(DateTime);
			if (s == "datetime2")
				return typeof(DateTimeOffset);

			var type = Type.GetType(s, false, true);
			if (type == null)
				throw new FilterException($"Unable to load the type '{s}' from the current context");

			return type;
		}
	}
}
