using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deveel.Filters
{
	public static class JsonConvertTests
	{
		private static JsonSerializerOptions CreateOptions()
		{
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
				WriteIndented = false
			};
			options.Converters.Add(new JsonFilterConverter());
			return options;
		}

		private static string Serialize(FilterExpression filter)
		{
			return JsonSerializer.Serialize(filter, CreateOptions());
		}

		private static FilterExpression? Deserialize(string json)
		{
			return JsonSerializer.Deserialize<FilterExpression>(json, CreateOptions());
		}

		#region Constant Filter Tests

		[Theory]
		[InlineData(123, "{\"filterType\":\"Constant\",\"value\":123}")]
		[InlineData("test", "{\"filterType\":\"Constant\",\"value\":\"test\"}")]
		[InlineData(true, "{\"filterType\":\"Constant\",\"value\":true}")]
		[InlineData(false, "{\"filterType\":\"Constant\",\"value\":false}")]
		[InlineData(123.456, "{\"filterType\":\"Constant\",\"value\":123.456}")]
		[InlineData(123.456f, "{\"filterType\":\"Constant\",\"value\":123.456}")]
		public static void SerializeConstantFilter(object value, string expectedJson)
		{
			var filter = FilterExpression.Constant(value);
			var json = Serialize(filter);
			Assert.Equal(expectedJson, json);
		}

		[Fact]
		public static void SerializeConstantFilterWithNull()
		{
			var filter = FilterExpression.Constant(null);
			var json = Serialize(filter);
			Assert.Equal("{\"filterType\":\"Constant\",\"value\":null}", json);
		}

		[Theory]
		[InlineData("{\"filterType\":\"Constant\",\"value\":123}", 123)]
		[InlineData("{\"filterType\":\"Constant\",\"value\":\"test\"}", "test")]
		[InlineData("{\"filterType\":\"Constant\",\"value\":true}", true)]
		[InlineData("{\"filterType\":\"Constant\",\"value\":false}", false)]
		[InlineData("{\"filterType\":\"Constant\",\"value\":123.456}", 123.456)]
		[InlineData("{\"filterType\":\"Constant\",\"value\":null}", null)]
		public static void DeserializeConstantFilter(string json, object? expectedValue)
		{
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var constant = Assert.IsType<ConstantFilterExpression>(filter);
			Assert.Equal(expectedValue, constant.Value);
		}

		#endregion

		#region Variable Filter Tests

		[Theory]
		[InlineData("x", "{\"filterType\":\"Variable\",\"variableName\":\"x\"}")]
		[InlineData("user.name", "{\"filterType\":\"Variable\",\"variableName\":\"user.name\"}")]
		[InlineData("_private", "{\"filterType\":\"Variable\",\"variableName\":\"_private\"}")]
		public static void SerializeVariableFilter(string variableName, string expectedJson)
		{
			var filter = FilterExpression.Variable(variableName);
			var json = Serialize(filter);
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData("{\"filterType\":\"Variable\",\"variableName\":\"x\"}", "x")]
		[InlineData("{\"filterType\":\"Variable\",\"variableName\":\"user.name\"}", "user.name")]
		[InlineData("{\"filterType\":\"Variable\",\"variableName\":\"_private\"}", "_private")]
		public static void DeserializeVariableFilter(string json, string expectedVariableName)
		{
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var variable = Assert.IsType<VariableFilterExpression>(filter);
			Assert.Equal(expectedVariableName, variable.VariableName);
		}

		#endregion

		#region Binary Filter Tests

		[Theory]
		[InlineData(FilterExpressionType.Equal)]
		[InlineData(FilterExpressionType.NotEqual)]
		[InlineData(FilterExpressionType.GreaterThan)]
		[InlineData(FilterExpressionType.GreaterThanOrEqual)]
		[InlineData(FilterExpressionType.LessThan)]
		[InlineData(FilterExpressionType.LessThanOrEqual)]
		[InlineData(FilterExpressionType.And)]
		[InlineData(FilterExpressionType.Or)]
		public static void SerializeBinaryFilter(FilterExpressionType expressionType)
		{
			var left = FilterExpression.Variable("x");
			var right = FilterExpression.Constant(123);
			var filter = FilterExpression.Binary(left, right, expressionType);

			var json = Serialize(filter);
			var expectedJson = $"{{\"filterType\":\"{expressionType}\",\"left\":{{\"filterType\":\"Variable\",\"variableName\":\"x\"}},\"right\":{{\"filterType\":\"Constant\",\"value\":123}}}}";
			
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData(FilterExpressionType.Equal)]
		[InlineData(FilterExpressionType.NotEqual)]
		[InlineData(FilterExpressionType.GreaterThan)]
		[InlineData(FilterExpressionType.GreaterThanOrEqual)]
		[InlineData(FilterExpressionType.LessThan)]
		[InlineData(FilterExpressionType.LessThanOrEqual)]
		[InlineData(FilterExpressionType.And)]
		[InlineData(FilterExpressionType.Or)]
		public static void DeserializeBinaryFilter(FilterExpressionType expressionType)
		{
			var json = $"{{\"filterType\":\"{expressionType}\",\"left\":{{\"filterType\":\"Variable\",\"variableName\":\"x\"}},\"right\":{{\"filterType\":\"Constant\",\"value\":123}}}}";
			
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var binary = Assert.IsType<BinaryFilterExpression>(filter);
			Assert.Equal(expressionType, binary.ExpressionType);

			var left = Assert.IsType<VariableFilterExpression>(binary.Left);
			Assert.Equal("x", left.VariableName);

			var right = Assert.IsType<ConstantFilterExpression>(binary.Right);
			Assert.Equal(123, right.Value);
		}

		[Fact]
		public static void SerializeNestedBinaryFilter()
		{
			var innerLeft = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan);
			var innerRight = FilterExpression.Binary(FilterExpression.Variable("y"), FilterExpression.Constant(20), FilterExpressionType.LessThan);
			var filter = FilterExpression.Binary(innerLeft, innerRight, FilterExpressionType.And);

			var json = Serialize(filter);
			var expectedJson = "{\"filterType\":\"And\",\"left\":{\"filterType\":\"GreaterThan\",\"left\":{\"filterType\":\"Variable\",\"variableName\":\"x\"},\"right\":{\"filterType\":\"Constant\",\"value\":10}},\"right\":{\"filterType\":\"LessThan\",\"left\":{\"filterType\":\"Variable\",\"variableName\":\"y\"},\"right\":{\"filterType\":\"Constant\",\"value\":20}}}";
			
			Assert.Equal(expectedJson, json);
		}

		[Fact]
		public static void DeserializeNestedBinaryFilter()
		{
			var json = "{\"filterType\":\"And\",\"left\":{\"filterType\":\"GreaterThan\",\"left\":{\"filterType\":\"Variable\",\"variableName\":\"x\"},\"right\":{\"filterType\":\"Constant\",\"value\":10}},\"right\":{\"filterType\":\"LessThan\",\"left\":{\"filterType\":\"Variable\",\"variableName\":\"y\"},\"right\":{\"filterType\":\"Constant\",\"value\":20}}}";
			
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var binary = Assert.IsType<BinaryFilterExpression>(filter);
			Assert.Equal(FilterExpressionType.And, binary.ExpressionType);

			var leftBinary = Assert.IsType<BinaryFilterExpression>(binary.Left);
			Assert.Equal(FilterExpressionType.GreaterThan, leftBinary.ExpressionType);

			var rightBinary = Assert.IsType<BinaryFilterExpression>(binary.Right);
			Assert.Equal(FilterExpressionType.LessThan, rightBinary.ExpressionType);
		}

		#endregion

		#region Unary Filter Tests

		[Fact]
		public static void SerializeUnaryFilter()
		{
			var operand = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(123), FilterExpressionType.Equal);
			var filter = FilterExpression.Not(operand);

			var json = Serialize(filter);
			var expectedJson = "{\"filterType\":\"Not\",\"operand\":{\"filterType\":\"Equal\",\"left\":{\"filterType\":\"Variable\",\"variableName\":\"x\"},\"right\":{\"filterType\":\"Constant\",\"value\":123}}}";
			
			Assert.Equal(expectedJson, json);
		}

		[Fact]
		public static void DeserializeUnaryFilter()
		{
			var json = "{\"filterType\":\"Not\",\"operand\":{\"filterType\":\"Equal\",\"left\":{\"filterType\":\"Variable\",\"variableName\":\"x\"},\"right\":{\"filterType\":\"Constant\",\"value\":123}}}";
			
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var unary = Assert.IsType<UnaryFilterExpression>(filter);
			Assert.Equal(FilterExpressionType.Not, unary.ExpressionType);

			var operand = Assert.IsType<BinaryFilterExpression>(unary.Operand);
			Assert.Equal(FilterExpressionType.Equal, operand.ExpressionType);
		}

		[Fact]
		public static void SerializeNestedUnaryFilter()
		{
			var innerOperand = FilterExpression.Variable("x");
			var innerUnary = FilterExpression.Not(innerOperand);
			var filter = FilterExpression.Not(innerUnary);

			var json = Serialize(filter);
			var expectedJson = "{\"filterType\":\"Not\",\"operand\":{\"filterType\":\"Not\",\"operand\":{\"filterType\":\"Variable\",\"variableName\":\"x\"}}}";
			
			Assert.Equal(expectedJson, json);
		}

		#endregion

		#region Function Filter Tests

		[Fact]
		public static void SerializeFunctionFilterWithoutArguments()
		{
			var variable = FilterExpression.Variable("user");
			var filter = FilterExpression.Function(variable, "isActive");

			var json = Serialize(filter);
			var expectedJson = "{\"filterType\":\"Function\",\"variable\":{\"filterType\":\"Variable\",\"variableName\":\"user\"},\"functionName\":\"isActive\"}";
			
			Assert.Equal(expectedJson, json);
		}

		[Fact]
		public static void SerializeFunctionFilterWithArguments()
		{
			var variable = FilterExpression.Variable("user");
			var args = new FilterExpression[] { FilterExpression.Constant("admin"), FilterExpression.Variable("role") };
			var filter = FilterExpression.Function(variable, "hasRole", args);

			var json = Serialize(filter);
			var expectedJson = "{\"filterType\":\"Function\",\"variable\":{\"filterType\":\"Variable\",\"variableName\":\"user\"},\"functionName\":\"hasRole\",\"arguments\":[{\"filterType\":\"Constant\",\"value\":\"admin\"},{\"filterType\":\"Variable\",\"variableName\":\"role\"}]}";
			
			Assert.Equal(expectedJson, json);
		}

		[Fact]
		public static void DeserializeFunctionFilterWithoutArguments()
		{
			var json = "{\"filterType\":\"Function\",\"variable\":{\"filterType\":\"Variable\",\"variableName\":\"user\"},\"functionName\":\"isActive\"}";
			
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var function = Assert.IsType<FunctionFilterExpression>(filter);
			Assert.Equal(FilterExpressionType.Function, function.ExpressionType);
			Assert.Equal("isActive", function.FunctionName);

			var variable = Assert.IsType<VariableFilterExpression>(function.Variable);
			Assert.Equal("user", variable.VariableName);

			Assert.Empty(function.Arguments);
		}

		[Fact]
		public static void DeserializeFunctionFilterWithArguments()
		{
			var json = "{\"filterType\":\"Function\",\"variable\":{\"filterType\":\"Variable\",\"variableName\":\"user\"},\"functionName\":\"hasRole\",\"arguments\":[{\"filterType\":\"Constant\",\"value\":\"admin\"},{\"filterType\":\"Variable\",\"variableName\":\"role\"}]}";
			
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var function = Assert.IsType<FunctionFilterExpression>(filter);
			Assert.Equal(FilterExpressionType.Function, function.ExpressionType);
			Assert.Equal("hasRole", function.FunctionName);

			var variable = Assert.IsType<VariableFilterExpression>(function.Variable);
			Assert.Equal("user", variable.VariableName);

			Assert.NotNull(function.Arguments);
			Assert.Equal(2, function.Arguments.Length);

			var firstArg = Assert.IsType<ConstantFilterExpression>(function.Arguments[0]);
			Assert.Equal("admin", firstArg.Value);

			var secondArg = Assert.IsType<VariableFilterExpression>(function.Arguments[1]);
			Assert.Equal("role", secondArg.VariableName);
		}

		#endregion

		#region Round-trip Tests

		[Fact]
		public static void RoundTripConstantFilter()
		{
			var original = FilterExpression.Constant("test value");
			var json = Serialize(original);
			var deserialized = Deserialize(json);

			Assert.NotNull(deserialized);
			var constant = Assert.IsType<ConstantFilterExpression>(deserialized);
			Assert.Equal(original.Value, constant.Value);
		}

		[Fact]
		public static void RoundTripVariableFilter()
		{
			var original = FilterExpression.Variable("test.variable");
			var json = Serialize(original);
			var deserialized = Deserialize(json);

			Assert.NotNull(deserialized);
			var variable = Assert.IsType<VariableFilterExpression>(deserialized);
			Assert.Equal(original.VariableName, variable.VariableName);
		}

		[Fact]
		public static void RoundTripComplexFilter()
		{
			var original = FilterExpression.And(
				FilterExpression.Binary(FilterExpression.Variable("age"), FilterExpression.Constant(18), FilterExpressionType.GreaterThanOrEqual),
				FilterExpression.Or(
					FilterExpression.Binary(FilterExpression.Variable("status"), FilterExpression.Constant("active"), FilterExpressionType.Equal),
					FilterExpression.Not(FilterExpression.Binary(FilterExpression.Variable("blocked"), FilterExpression.Constant(true), FilterExpressionType.Equal))
				)
			);

			var json = Serialize(original);
			var deserialized = Deserialize(json);

			Assert.NotNull(deserialized);
			Assert.IsType<BinaryFilterExpression>(deserialized);
			Assert.Equal(FilterExpressionType.And, deserialized.ExpressionType);
		}

		#endregion

		#region Error Handling Tests

		[Fact]
		public static void DeserializeThrowsOnMissingFilterType()
		{
			var json = "{\"value\":123}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnInvalidFilterType()
		{
			var json = "{\"filterType\":\"InvalidType\",\"value\":123}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnMissingConstantValue()
		{
			var json = "{\"filterType\":\"Constant\"}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnMissingVariableName()
		{
			var json = "{\"filterType\":\"Variable\"}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnEmptyVariableName()
		{
			var json = "{\"filterType\":\"Variable\",\"variableName\":\"\"}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnMissingBinaryLeft()
		{
			var json = "{\"filterType\":\"Equal\",\"right\":{\"filterType\":\"Constant\",\"value\":123}}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnMissingBinaryRight()
		{
			var json = "{\"filterType\":\"Equal\",\"left\":{\"filterType\":\"Variable\",\"variableName\":\"x\"}}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnNullBinaryOperands()
		{
			var json = "{\"filterType\":\"Equal\",\"left\":null,\"right\":null}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnMissingUnaryOperand()
		{
			var json = "{\"filterType\":\"Not\"}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnNullUnaryOperand()
		{
			var json = "{\"filterType\":\"Not\",\"operand\":null}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnMissingFunctionVariable()
		{
			var json = "{\"filterType\":\"Function\",\"functionName\":\"test\"}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnMissingFunctionName()
		{
			var json = "{\"filterType\":\"Function\",\"variable\":{\"filterType\":\"Variable\",\"variableName\":\"x\"}}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnEmptyFunctionName()
		{
			var json = "{\"filterType\":\"Function\",\"variable\":{\"filterType\":\"Variable\",\"variableName\":\"x\"},\"functionName\":\"\"}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnNullFunctionVariable()
		{
			var json = "{\"filterType\":\"Function\",\"variable\":null,\"functionName\":\"test\"}";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnInvalidJsonStructure()
		{
			var json = "invalid json";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		[Fact]
		public static void DeserializeThrowsOnNonObjectJson()
		{
			var json = "\"string value\"";
			Assert.Throws<JsonException>(() => Deserialize(json));
		}

		#endregion

		#region Special Value Tests

		[Theory]
		[InlineData(double.PositiveInfinity)]
		[InlineData(double.NegativeInfinity)]
		[InlineData(double.NaN)]
		[InlineData(float.PositiveInfinity)]
		[InlineData(float.NegativeInfinity)]
		[InlineData(float.NaN)]
		public static void SerializeSpecialNumericValues(object value)
		{
			var filter = FilterExpression.Constant(value);
			var json = Serialize(filter);
			Assert.Contains("filterType", json);
			Assert.Contains("Constant", json);
		}

		[Fact]
		public static void SerializeFilterWithDateTime()
		{
			var dateTime = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);
			var filter = FilterExpression.Constant(dateTime);
			var json = Serialize(filter);
			
			Assert.Contains("filterType", json);
			Assert.Contains("Constant", json);
		}

		[Fact]
		public static void RoundTripFilterWithDateTime()
		{
			var original = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);
			var filter = FilterExpression.Constant(original);
			var json = Serialize(filter);
			var deserialized = Deserialize(json);

			Assert.NotNull(deserialized);
			var constant = Assert.IsType<ConstantFilterExpression>(deserialized);
			Assert.IsType<DateTime>(constant.Value);
		}

		[Fact]
		public static void FilterWithDateTimeOffsetConvertsToDateTime()
		{
			var original = new DateTimeOffset(2023, 12, 25, 10, 30, 0, TimeSpan.FromHours(2));
			var filter = FilterExpression.Constant(original);
			var json = Serialize(filter);
			var deserialized = Deserialize(json);
			Assert.NotNull(deserialized);
			var constant = Assert.IsType<ConstantFilterExpression>(deserialized);
			// TODO: Handle DateTimeOffset if needed (this is a limitation of System.Text.Json)
			Assert.IsType<DateTime>(constant.Value);
		}

		[Fact]
		public static void SerializeFilterWithNullValue()
		{
			var filter = FilterExpression.Constant(null);
			var json = Serialize(filter);
			Assert.Equal("{\"filterType\":\"Constant\",\"value\":null}", json);
		}

		#endregion
	}
}
