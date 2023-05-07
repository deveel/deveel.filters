using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Deveel.Filters {
	public static class BsonFilter {
		public static Filter FromBson(BsonDocument document) {
			if (!document.TryGetElement("type", out var typeElement))
				throw new FilterException("The type of the filter is not specified");

			if (!Enum.TryParse<FilterType>(typeElement.Value.AsString, true, out var filterType))
				throw new FilterException($"The filter type '{typeElement.Value.AsString}' is not valid");

			switch (filterType) {
				case FilterType.Equals:
				case FilterType.NotEquals:
				case FilterType.GreaterThan:
				case FilterType.GreaterThanOrEqual:
				case FilterType.LessThan:
				case FilterType.LessThanOrEqual:
				case FilterType.And:
				case FilterType.Or:
					return FromBinaryBsonDocument(filterType, document);
				case FilterType.Not:
					return FromNotBsonDocument(document);
				case FilterType.Function:
					return FromFunctionBsonDocument(document);
				case FilterType.Constant:
					return FromConstantBsonDocument(document);
				case FilterType.Variable:
					return FromVariableBsonDocument(document);
				default:
					throw new FilterException($"The filter type '{filterType}' is not supported");
			}
		}

		private static Filter FromFunctionBsonDocument(BsonDocument document) {
			if (!document.TryGetElement("function", out var functionElement))
				throw new FilterException("The function name is not specified");

			if (!document.TryGetElement("variable", out var variableElement))
				throw new FilterException("The variable name is not specified");

			if (!variableElement.Value.IsBsonDocument)
				throw new FilterException("The variable is not a valid BSON document");

			var variable = FromVariableBsonDocument(variableElement.Value.AsBsonDocument);
			var functionName = functionElement.Value.AsString;
			var arguments = new Filter[0];
			if (document.TryGetElement("arguments", out var argumentsElement)) {
				var argumentsArray = argumentsElement.Value.AsBsonArray;
				arguments = new Filter[argumentsArray.Count];
				for (var i = 0; i < argumentsArray.Count; i++) {
					var argument = argumentsArray[i].AsBsonDocument;
					arguments[i] = FromBson(argument);
				}
			}
			return Filter.Function(variable, functionName, arguments);
		}

		private static VariableFilter FromVariableBsonDocument(BsonDocument document) {
			if (!document.TryGetElement("varRef", out var refElement))
				throw new FilterException("The variable name is not specified");

			var variableName = refElement.Value.AsString;
			return Filter.Variable(variableName);
		}

		private static ConstantFilter FromConstantBsonDocument(BsonDocument document) {
			if (!document.TryGetElement("value", out var valueElement))
				throw new FilterException("The value of the constant filter is not specified");
			if (!document.TryGetElement("valueType", out var valueTypeElement))
				throw new FilterException("The value type of the constant filter is not specified");

			var valueType = Type.GetType(valueTypeElement.Value.AsString, true, true);
			if (valueType == null)
				throw new FilterException($"The value type '{valueTypeElement.Value.AsString}' is not valid");

			var value = ConvertBsonValue(valueType, valueElement.Value);

			return Filter.Constant(value);
		}

		public static object? ConvertBsonValue(Type valueType, BsonValue value) {
			if (valueType == typeof(BsonNull) ||
				value == BsonNull.Value)
				return null;

			if (valueType == typeof(string))
				return value.AsString;
			if (valueType == typeof(int))
				return value.AsInt32;
			if (valueType == typeof(long))
				return value.AsInt64;
			if (valueType == typeof(float))
				return (float) value.AsDouble;
			if (valueType == typeof(double))
				return value.AsDouble;
			if (valueType == typeof(bool))
				return value.AsBoolean;
			if (valueType == typeof(DateTime) ||
				valueType == typeof(DateTimeOffset))
				return value.ToUniversalTime();
			if (valueType == typeof(TimeSpan))
				return TimeSpan.FromMilliseconds(value.AsInt64);
			if (valueType == typeof(Guid))
				return value.AsGuid;
			if (valueType == typeof(byte[]))
				return value.AsByteArray;

			return BsonSerializer.Deserialize(value.ToBsonDocument(), valueType);
		}

		private static UnaryFilter FromNotBsonDocument(BsonDocument document) {
			if (!document.TryGetElement("not", out var notElement))
				throw new FilterException("The NOT filter is not specified");

			var not = FromBson(notElement.Value.AsBsonDocument);
			return Filter.Not(not);
		}

		private static BinaryFilter FromBinaryBsonDocument(FilterType filterType, BsonDocument document) {
			if (!document.TryGetElement("left", out var leftElement))
				throw new FilterException("The left operand of the binary filter is not specified");
			if (!document.TryGetElement("right", out var rightElement))
				throw new FilterException("The right operand of the binary filter is not specified");

			var left = FromBson(leftElement.Value.AsBsonDocument);
			var right = FromBson(rightElement.Value.AsBsonDocument);
			return Filter.Binary(left, right, filterType);
		}
	}
}
