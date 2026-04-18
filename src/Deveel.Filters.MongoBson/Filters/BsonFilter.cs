// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Deveel.Filters {
	/// <summary>
	/// Provides methods to convert <see cref="FilterExpression"/> instances to and from
	/// MongoDB <see cref="BsonDocument"/> representations.
	/// </summary>
	public static class BsonFilter {
		/// <summary>
		/// Gets the string representation of the specified CLR type for use in BSON filter documents.
		/// </summary>
		/// <param name="type">The CLR type to convert.</param>
		/// <returns>A string identifier for the type.</returns>
		public static string GetTypeString(Type type)
			=> BsonFilterUtil.GetValueTypeString(type);

		/// <summary>
		/// Gets the CLR <see cref="Type"/> corresponding to the specified type string.
		/// </summary>
		/// <param name="typeString">The type string to resolve.</param>
		/// <returns>The corresponding CLR <see cref="Type"/>.</returns>
		/// <exception cref="FilterException">
		/// Thrown when the type string cannot be resolved.
		/// </exception>
		public static Type GetTypeFromString(string typeString)
			=> BsonFilterUtil.GetTypeFromString(typeString);

		/// <summary>
		/// Deserializes a <see cref="BsonDocument"/> into a <see cref="FilterExpression"/>.
		/// </summary>
		/// <param name="document">The BSON document to deserialize.</param>
		/// <returns>The deserialized <see cref="FilterExpression"/>.</returns>
		/// <exception cref="FilterException">
		/// Thrown when the document is missing required elements or contains an unsupported filter type.
		/// </exception>
		public static FilterExpression FromBson(BsonDocument document) {
			if (!document.TryGetElement("type", out var typeElement))
				throw new FilterException("The type of the filter is not specified");

			if (!Enum.TryParse<FilterExpressionType>(typeElement.Value.AsString, true, out var filterType))
				throw new FilterException($"The filter type '{typeElement.Value.AsString}' is not valid");

			switch (filterType) {
				case FilterExpressionType.Equal:
				case FilterExpressionType.NotEqual:
				case FilterExpressionType.GreaterThan:
				case FilterExpressionType.GreaterThanOrEqual:
				case FilterExpressionType.LessThan:
				case FilterExpressionType.LessThanOrEqual:
				case FilterExpressionType.And:
				case FilterExpressionType.Or:
					return FromBinaryBsonDocument(filterType, document);
				case FilterExpressionType.Not:
					return FromNotBsonDocument(document);
				case FilterExpressionType.Function:
					return FromFunctionBsonDocument(document);
				case FilterExpressionType.Constant:
					return FromConstantBsonDocument(document);
				case FilterExpressionType.Variable:
					return FromVariableBsonDocument(document);
				default:
					throw new FilterException($"The filter type '{filterType}' is not supported");
			}
		}

		private static FilterExpression FromFunctionBsonDocument(BsonDocument document) {
			if (!document.TryGetElement("function", out var functionElement))
				throw new FilterException("The function name is not specified");

			if (!document.TryGetElement("variable", out var variableElement))
				throw new FilterException("The variable name is not specified");

			if (!variableElement.Value.IsBsonDocument)
				throw new FilterException("The variable is not a valid BSON document");

			var variable = FromVariableBsonDocument(variableElement.Value.AsBsonDocument);
			var functionName = functionElement.Value.AsString;
			var arguments = new FilterExpression[0];
			if (document.TryGetElement("arguments", out var argumentsElement)) {
				var argumentsArray = argumentsElement.Value.AsBsonArray;
				arguments = new FilterExpression[argumentsArray.Count];
				for (var i = 0; i < argumentsArray.Count; i++) {
					var argument = argumentsArray[i].AsBsonDocument;
					arguments[i] = FromBson(argument);
				}
			}
			return FilterExpression.Function(variable, functionName, arguments);
		}

		private static VariableFilterExpression FromVariableBsonDocument(BsonDocument document) {
			if (!document.TryGetElement("varRef", out var refElement))
				throw new FilterException("The variable name is not specified");

			var variableName = refElement.Value.AsString;
			return FilterExpression.Variable(variableName);
		}

		private static ConstantFilterExpression FromConstantBsonDocument(BsonDocument document) {
			if (!document.TryGetElement("value", out var valueElement))
				throw new FilterException("The value of the constant filter is not specified");
			if (!document.TryGetElement("valueType", out var valueTypeElement))
				throw new FilterException("The value type of the constant filter is not specified");

			var valueType = BsonFilterUtil.GetTypeFromString(valueTypeElement.Value.AsString);
			if (valueType == null)
				throw new FilterException($"The value type '{valueTypeElement.Value.AsString}' is not valid");

			var value = ConvertBsonValue(valueType, valueElement.Value);

			return FilterExpression.Constant(value);
		}

		/// <summary>
		/// Converts a <see cref="BsonValue"/> to a CLR object of the specified type.
		/// </summary>
		/// <param name="valueType">The expected CLR type of the value.</param>
		/// <param name="value">The BSON value to convert.</param>
		/// <returns>The converted CLR value, or <c>null</c> if the value is <see cref="BsonNull"/>.</returns>
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
			if (valueType == typeof(DateTime))
				return value.AsDateTime;
			if (valueType == typeof(DateTimeOffset)) {
				// var doc = value.AsBsonDocument;
				return BsonFilterUtil.CreateDateTimeOffset(value.AsBsonDateTime);
			}
			if (valueType == typeof(TimeSpan))
				return TimeSpan.FromMilliseconds(value.AsInt64);
			if (valueType == typeof(Guid))
				return value.AsGuid;
			if (valueType == typeof(byte[]))
				return value.AsByteArray;

			return BsonSerializer.Deserialize(value.ToBsonDocument(), valueType);
		}

		private static UnaryFilterExpression FromNotBsonDocument(BsonDocument document) {
			if (!document.TryGetElement("operand", out var notElement))
				throw new FilterException("The NOT filter is not specified");

			var not = FromBson(notElement.Value.AsBsonDocument);
			return FilterExpression.Not(not);
		}

		private static BinaryFilterExpression FromBinaryBsonDocument(FilterExpressionType expressionType, BsonDocument document) {
			if (!document.TryGetElement("left", out var leftElement))
				throw new FilterException("The left operand of the binary filter is not specified");
			if (!document.TryGetElement("right", out var rightElement))
				throw new FilterException("The right operand of the binary filter is not specified");

			var left = FromBson(leftElement.Value.AsBsonDocument);
			var right = FromBson(rightElement.Value.AsBsonDocument);
			return FilterExpression.Binary(left, right, expressionType);
		}
	}
}
