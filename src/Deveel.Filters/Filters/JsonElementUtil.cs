// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Deveel.Filters {
	/// <summary>
	/// Provides utility methods for converting values to and from <see cref="JsonElement"/>
	/// and for building filter expressions from JSON data.
	/// </summary>
    public static class JsonElementUtil {
		/// <summary>
		/// Converts the specified value to a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="value">The value to convert, or <c>null</c>.</param>
		/// <returns>A <see cref="JsonElement"/> representing the given value.</returns>
        public static JsonElement ToElement(object? value) {
            string valueString;
            if (value == null) {
                valueString = "null";
            } else if (value is string s) { 
                valueString = $"\"{s}\"";
            } else if (value is bool b) {
                valueString = b ? "true" : "false";
            } else {
                valueString = Convert.ToString(value);
            }

            return JsonDocument.Parse(valueString).RootElement;
        }

		/// <summary>
		/// Builds a composite <see cref="BinaryFilterExpression"/> from a dictionary of
		/// JSON key-value pairs, combining them with the specified logical operator.
		/// </summary>
		/// <param name="jsonData">A dictionary of field names to JSON values.</param>
		/// <param name="expressionType">The comparison type to apply to each key-value pair.</param>
		/// <param name="defaultLogical">
		/// The logical operator used to combine multiple comparisons (default is <see cref="FilterExpressionType.And"/>).
		/// </param>
		/// <returns>A <see cref="BinaryFilterExpression"/> representing the combined filter.</returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="defaultLogical"/> is not a logical type, or
		/// <paramref name="expressionType"/> is not a comparison type.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="jsonData"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="FilterException">
		/// Thrown when no filter can be built from the given data.
		/// </exception>
        public static BinaryFilterExpression BuildFilter(IDictionary<string, JsonElement>? jsonData, FilterExpressionType expressionType, FilterExpressionType defaultLogical = FilterExpressionType.And) {
            if (defaultLogical != FilterExpressionType.And &&
                defaultLogical != FilterExpressionType.Or)
                throw new ArgumentException($"The type '{defaultLogical}' is not a logical filter type", nameof(defaultLogical));

            if (expressionType != FilterExpressionType.Equal &&
                expressionType != FilterExpressionType.NotEqual &&
                expressionType != FilterExpressionType.GreaterThan &&
                expressionType != FilterExpressionType.GreaterThanOrEqual &&
                expressionType != FilterExpressionType.LessThan &&
                expressionType != FilterExpressionType.LessThanOrEqual)
                throw new ArgumentException($"The filter type '{expressionType}' is not a binary filter type", nameof(expressionType));

            if (jsonData == null)
                throw new ArgumentNullException(nameof(jsonData));

            BinaryFilterExpression? result = null;

            foreach (var item in jsonData) {
                var value = InferValue(item.Value);
                var filter = FilterExpression.Binary(FilterExpression.Variable(item.Key), FilterExpression.Constant(value), expressionType);

                if (result == null) {
                    result = filter;
                } else {
                    result = FilterExpression.Binary(result, filter, defaultLogical);
                }
            }

            if (result == null)
                throw new FilterException("It was not possible to build a binary filter from the given data");

            return result;
        }

		/// <summary>
		/// Infers a CLR value from the specified <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="json">The JSON element to infer the value from, or <c>null</c>.</param>
		/// <returns>
		/// The inferred CLR value, or <c>null</c> if the element is <c>null</c>,
		/// <see cref="JsonValueKind.Null"/>, or <see cref="JsonValueKind.Undefined"/>.
		/// </returns>
		/// <exception cref="FilterException">
		/// Thrown when the JSON element contains an unsupported value kind.
		/// </exception>
        public static object? InferValue(JsonElement? json) {
            if (json == null)
                return null;

            switch (json.Value.ValueKind) {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.String: {
						if (json.Value.TryGetDateTime(out var dateTime))
							return dateTime;
						if (json.Value.TryGetDateTimeOffset(out var dateTimeOffset))
                            return dateTimeOffset;

                        return json.Value.GetString();
                    }
                case JsonValueKind.Number:
                    if (json.Value.TryGetInt32(out var i))
                        return i;
                    if (json.Value.TryGetInt64(out var l))
                        return l;
                    if (json.Value.TryGetDouble(out var d))
                        return d;
                    if (json.Value.TryGetSingle(out var f))
                        return f;

                    throw new FilterException("The number is not supported");
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                default:
                    throw new FilterException("Complex data are not supported for this filter type");
            }
        }
    }
}
