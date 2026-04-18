// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deveel.Filters
{
	/// <summary>
	/// A <see cref="JsonConverter{T}"/> that serializes and deserializes
	/// <see cref="FilterExpression"/> instances to and from JSON.
	/// </summary>
	public class JsonFilterConverter : JsonConverter<FilterExpression>
	{
		/// <inheritdoc/>
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(FilterExpression).IsAssignableFrom(typeToConvert);
		}

		/// <inheritdoc/>
		public override FilterExpression? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected start of object");

			using var jsonDoc = JsonDocument.ParseValue(ref reader);
			var root = jsonDoc.RootElement;

			if (!root.TryGetProperty("filterType", out var filterTypeElement))
				throw new JsonException("Missing 'filterType' property");

			if (!Enum.TryParse<FilterExpressionType>(filterTypeElement.GetString(), out var filterType))
				throw new JsonException("Invalid 'filterType' value");

			return filterType switch
			{
				FilterExpressionType.Constant => DeserializeConstant(root),
				FilterExpressionType.Variable => DeserializeVariable(root),
				FilterExpressionType.Equal 
					or FilterExpressionType.NotEqual 
					or FilterExpressionType.GreaterThan 
					or FilterExpressionType.GreaterThanOrEqual 
					or FilterExpressionType.LessThan 
					or FilterExpressionType.LessThanOrEqual 
					or FilterExpressionType.And 
					or FilterExpressionType.Or => DeserializeBinary(root, filterType, options),
				FilterExpressionType.Not => DeserializeUnary(root, filterType, options),
				FilterExpressionType.Function => DeserializeFunction(root, options),
				_ => throw new JsonException($"Unsupported filter type: {filterType}")
			};
		}

		/// <inheritdoc/>
		public override void Write(Utf8JsonWriter writer, FilterExpression value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("filterType", value.ExpressionType.ToString());

			switch (value)
			{
				case ConstantFilterExpression constant:
					WriteConstant(writer, constant, options);
					break;
				case VariableFilterExpression variable:
					WriteVariable(writer, variable);
					break;
				case BinaryFilterExpression binary:
					WriteBinary(writer, binary, options);
					break;
				case UnaryFilterExpression unary:
					WriteUnary(writer, unary, options);
					break;
				case FunctionFilterExpression function:
					WriteFunction(writer, function, options);
					break;
				default:
					throw new JsonException($"Unsupported filter type: {value.GetType()}");
			}

			writer.WriteEndObject();
		}

		private static ConstantFilterExpression DeserializeConstant(JsonElement root)
		{
			if (!root.TryGetProperty("value", out var valueElement))
				throw new JsonException("Missing 'value' property for constant filter");

			var value = DeserializeValue(valueElement);
			return new ConstantFilterExpression(value);
		}

		private static object? DeserializeValue(JsonElement valueElement)
		{
			// Handle special numeric values
			if (valueElement.ValueKind == JsonValueKind.String)
			{
				var stringValue = valueElement.GetString();
				return stringValue switch
				{
					"__POSITIVE_INFINITY_DOUBLE__" => double.PositiveInfinity,
					"__NEGATIVE_INFINITY_DOUBLE__" => double.NegativeInfinity,
					"__NAN_DOUBLE__" => double.NaN,
					"__POSITIVE_INFINITY_FLOAT__" => float.PositiveInfinity,
					"__NEGATIVE_INFINITY_FLOAT__" => float.NegativeInfinity,
					"__NAN_FLOAT__" => float.NaN,
					_ => JsonElementUtil.InferValue(valueElement)
				};
			}

			return JsonElementUtil.InferValue(valueElement);
		}

		private static VariableFilterExpression DeserializeVariable(JsonElement root)
		{
			if (!root.TryGetProperty("variableName", out var nameElement))
				throw new JsonException("Missing 'variableName' property for variable filter");

			var variableName = nameElement.GetString();
			if (string.IsNullOrEmpty(variableName))
				throw new JsonException("Variable name cannot be null or empty");

			return FilterExpression.Variable(variableName);
		}

		private static BinaryFilterExpression DeserializeBinary(JsonElement root, FilterExpressionType expressionType, JsonSerializerOptions options)
		{
			if (!root.TryGetProperty("left", out var leftElement))
				throw new JsonException("Missing 'left' property for binary filter");

			if (!root.TryGetProperty("right", out var rightElement))
				throw new JsonException("Missing 'right' property for binary filter");

			var left = JsonSerializer.Deserialize<FilterExpression>(leftElement.GetRawText(), options);
			var right = JsonSerializer.Deserialize<FilterExpression>(rightElement.GetRawText(), options);

			if (left == null || right == null)
				throw new JsonException("Left and right operands cannot be null");

			return FilterExpression.Binary(left, right, expressionType);
		}

		private static UnaryFilterExpression DeserializeUnary(JsonElement root, FilterExpressionType expressionType, JsonSerializerOptions options)
		{
			if (!root.TryGetProperty("operand", out var operandElement))
				throw new JsonException("Missing 'operand' property for unary filter");

			var operand = JsonSerializer.Deserialize<FilterExpression>(operandElement.GetRawText(), options);
			if (operand == null)
				throw new JsonException("Operand cannot be null");

			return FilterExpression.Unary(operand, expressionType);
		}

		private static FunctionFilterExpression DeserializeFunction(JsonElement root, JsonSerializerOptions options)
		{
			if (!root.TryGetProperty("variable", out var variableElement))
				throw new JsonException("Missing 'variable' property for function filter");

			if (!root.TryGetProperty("functionName", out var functionNameElement))
				throw new JsonException("Missing 'functionName' property for function filter");

			var variable = JsonSerializer.Deserialize<VariableFilterExpression>(variableElement.GetRawText(), options);
			var functionName = functionNameElement.GetString();

			if (variable == null)
				throw new JsonException("Variable cannot be null");

			if (string.IsNullOrEmpty(functionName))
				throw new JsonException("Function name cannot be null or empty");

			FilterExpression[]? arguments = null;
			if (root.TryGetProperty("arguments", out var argumentsElement) && argumentsElement.ValueKind == JsonValueKind.Array)
			{
				var argList = new List<FilterExpression>();
				foreach (var argElement in argumentsElement.EnumerateArray())
				{
					var arg = JsonSerializer.Deserialize<FilterExpression>(argElement.GetRawText(), options);
					if (arg != null)
						argList.Add(arg);
				}
				arguments = argList.ToArray();
			}

			return FilterExpression.Function(variable, functionName, arguments ?? Array.Empty<FilterExpression>());
		}

		private static void WriteConstant(Utf8JsonWriter writer, ConstantFilterExpression constant, JsonSerializerOptions options)
		{
			writer.WritePropertyName("value");
			WriteValue(writer, constant.Value, options);
		}

		private static void WriteValue(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
		{
			// Handle special numeric values that JSON doesn't natively support
			switch (value)
			{
				case double d when double.IsPositiveInfinity(d):
					writer.WriteStringValue("__POSITIVE_INFINITY_DOUBLE__");
					break;
				case double d when double.IsNegativeInfinity(d):
					writer.WriteStringValue("__NEGATIVE_INFINITY_DOUBLE__");
					break;
				case double d when double.IsNaN(d):
					writer.WriteStringValue("__NAN_DOUBLE__");
					break;
				case float f when float.IsPositiveInfinity(f):
					writer.WriteStringValue("__POSITIVE_INFINITY_FLOAT__");
					break;
				case float f when float.IsNegativeInfinity(f):
					writer.WriteStringValue("__NEGATIVE_INFINITY_FLOAT__");
					break;
				case float f when float.IsNaN(f):
					writer.WriteStringValue("__NAN_FLOAT__");
					break;
				default:
					JsonSerializer.Serialize(writer, value, options);
					break;
			}
		}

		private static void WriteVariable(Utf8JsonWriter writer, VariableFilterExpression variable)
		{
			writer.WriteString("variableName", variable.VariableName);
		}

		private static void WriteBinary(Utf8JsonWriter writer, BinaryFilterExpression binary, JsonSerializerOptions options)
		{
			writer.WritePropertyName("left");
			JsonSerializer.Serialize(writer, binary.Left, options);

			writer.WritePropertyName("right");
			JsonSerializer.Serialize(writer, binary.Right, options);
		}

		private static void WriteUnary(Utf8JsonWriter writer, UnaryFilterExpression unary, JsonSerializerOptions options)
		{
			writer.WritePropertyName("operand");
			JsonSerializer.Serialize(writer, unary.Operand, options);
		}

		private static void WriteFunction(Utf8JsonWriter writer, FunctionFilterExpression function, JsonSerializerOptions options)
		{
			writer.WritePropertyName("variable");
			JsonSerializer.Serialize(writer, function.Variable, options);

			writer.WriteString("functionName", function.FunctionName);

			if (function.Arguments != null && function.Arguments.Length > 0)
			{
				writer.WritePropertyName("arguments");
				writer.WriteStartArray();
				foreach (var arg in function.Arguments)
				{
					JsonSerializer.Serialize(writer, arg, options);
				}
				writer.WriteEndArray();
			}
		}
	}
}
