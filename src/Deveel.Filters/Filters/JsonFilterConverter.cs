using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deveel.Filters
{
	public class JsonFilterConverter : JsonConverter<Filter>
	{
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(Filter).IsAssignableFrom(typeToConvert);
		}

		public override Filter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected start of object");

			using var jsonDoc = JsonDocument.ParseValue(ref reader);
			var root = jsonDoc.RootElement;

			if (!root.TryGetProperty("filterType", out var filterTypeElement))
				throw new JsonException("Missing 'filterType' property");

			if (!Enum.TryParse<FilterType>(filterTypeElement.GetString(), out var filterType))
				throw new JsonException("Invalid 'filterType' value");

			return filterType switch
			{
				FilterType.Constant => DeserializeConstant(root),
				FilterType.Variable => DeserializeVariable(root),
				FilterType.Equal or FilterType.NotEqual or FilterType.GreaterThan or 
				FilterType.GreaterThanOrEqual or FilterType.LessThan or FilterType.LessThanOrEqual or
				FilterType.And or FilterType.Or => DeserializeBinary(root, filterType, options),
				FilterType.Not => DeserializeUnary(root, filterType, options),
				FilterType.Function => DeserializeFunction(root, options),
				_ => throw new JsonException($"Unsupported filter type: {filterType}")
			};
		}

		public override void Write(Utf8JsonWriter writer, Filter value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteString("filterType", value.FilterType.ToString());

			switch (value)
			{
				case ConstantFilter constant:
					WriteConstant(writer, constant, options);
					break;
				case VariableFilter variable:
					WriteVariable(writer, variable);
					break;
				case BinaryFilter binary:
					WriteBinary(writer, binary, options);
					break;
				case UnaryFilter unary:
					WriteUnary(writer, unary, options);
					break;
				case FunctionFilter function:
					WriteFunction(writer, function, options);
					break;
				default:
					throw new JsonException($"Unsupported filter type: {value.GetType()}");
			}

			writer.WriteEndObject();
		}

		private static ConstantFilter DeserializeConstant(JsonElement root)
		{
			if (!root.TryGetProperty("value", out var valueElement))
				throw new JsonException("Missing 'value' property for constant filter");

			var value = DeserializeValue(valueElement);
			return new ConstantFilter(value);
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

		private static VariableFilter DeserializeVariable(JsonElement root)
		{
			if (!root.TryGetProperty("variableName", out var nameElement))
				throw new JsonException("Missing 'variableName' property for variable filter");

			var variableName = nameElement.GetString();
			if (string.IsNullOrEmpty(variableName))
				throw new JsonException("Variable name cannot be null or empty");

			return Filter.Variable(variableName);
		}

		private static BinaryFilter DeserializeBinary(JsonElement root, FilterType filterType, JsonSerializerOptions options)
		{
			if (!root.TryGetProperty("left", out var leftElement))
				throw new JsonException("Missing 'left' property for binary filter");

			if (!root.TryGetProperty("right", out var rightElement))
				throw new JsonException("Missing 'right' property for binary filter");

			var left = JsonSerializer.Deserialize<Filter>(leftElement.GetRawText(), options);
			var right = JsonSerializer.Deserialize<Filter>(rightElement.GetRawText(), options);

			if (left == null || right == null)
				throw new JsonException("Left and right operands cannot be null");

			return Filter.Binary(left, right, filterType);
		}

		private static UnaryFilter DeserializeUnary(JsonElement root, FilterType filterType, JsonSerializerOptions options)
		{
			if (!root.TryGetProperty("operand", out var operandElement))
				throw new JsonException("Missing 'operand' property for unary filter");

			var operand = JsonSerializer.Deserialize<Filter>(operandElement.GetRawText(), options);
			if (operand == null)
				throw new JsonException("Operand cannot be null");

			return Filter.Unary(operand, filterType);
		}

		private static FunctionFilter DeserializeFunction(JsonElement root, JsonSerializerOptions options)
		{
			if (!root.TryGetProperty("variable", out var variableElement))
				throw new JsonException("Missing 'variable' property for function filter");

			if (!root.TryGetProperty("functionName", out var functionNameElement))
				throw new JsonException("Missing 'functionName' property for function filter");

			var variable = JsonSerializer.Deserialize<VariableFilter>(variableElement.GetRawText(), options);
			var functionName = functionNameElement.GetString();

			if (variable == null)
				throw new JsonException("Variable cannot be null");

			if (string.IsNullOrEmpty(functionName))
				throw new JsonException("Function name cannot be null or empty");

			Filter[]? arguments = null;
			if (root.TryGetProperty("arguments", out var argumentsElement) && argumentsElement.ValueKind == JsonValueKind.Array)
			{
				var argList = new List<Filter>();
				foreach (var argElement in argumentsElement.EnumerateArray())
				{
					var arg = JsonSerializer.Deserialize<Filter>(argElement.GetRawText(), options);
					if (arg != null)
						argList.Add(arg);
				}
				arguments = argList.ToArray();
			}

			return Filter.Function(variable, functionName, arguments ?? Array.Empty<Filter>());
		}

		private static void WriteConstant(Utf8JsonWriter writer, ConstantFilter constant, JsonSerializerOptions options)
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

		private static void WriteVariable(Utf8JsonWriter writer, VariableFilter variable)
		{
			writer.WriteString("variableName", variable.VariableName);
		}

		private static void WriteBinary(Utf8JsonWriter writer, BinaryFilter binary, JsonSerializerOptions options)
		{
			writer.WritePropertyName("left");
			JsonSerializer.Serialize(writer, binary.Left, options);

			writer.WritePropertyName("right");
			JsonSerializer.Serialize(writer, binary.Right, options);
		}

		private static void WriteUnary(Utf8JsonWriter writer, UnaryFilter unary, JsonSerializerOptions options)
		{
			writer.WritePropertyName("operand");
			JsonSerializer.Serialize(writer, unary.Operand, options);
		}

		private static void WriteFunction(Utf8JsonWriter writer, FunctionFilter function, JsonSerializerOptions options)
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
