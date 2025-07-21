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

		private static string Serialize(Filter filter)
		{
			return JsonSerializer.Serialize(filter, CreateOptions());
		}

		private static Filter? Deserialize(string json)
		{
			return JsonSerializer.Deserialize<Filter>(json, CreateOptions());
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
			var filter = Filter.Constant(value);
			var json = Serialize(filter);
			Assert.Equal(expectedJson, json);
		}

		[Fact]
		public static void SerializeConstantFilterWithNull()
		{
			var filter = Filter.Constant(null);
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
			var constant = Assert.IsType<ConstantFilter>(filter);
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
			var filter = Filter.Variable(variableName);
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
			var variable = Assert.IsType<VariableFilter>(filter);
			Assert.Equal(expectedVariableName, variable.VariableName);
		}

		#endregion

		#region Binary Filter Tests

		[Theory]
		[InlineData(FilterType.Equal)]
		[InlineData(FilterType.NotEqual)]
		[InlineData(FilterType.GreaterThan)]
		[InlineData(FilterType.GreaterThanOrEqual)]
		[InlineData(FilterType.LessThan)]
		[InlineData(FilterType.LessThanOrEqual)]
		[InlineData(FilterType.And)]
		[InlineData(FilterType.Or)]
		public static void SerializeBinaryFilter(FilterType filterType)
		{
			var left = Filter.Variable("x");
			var right = Filter.Constant(123);
			var filter = Filter.Binary(left, right, filterType);

			var json = Serialize(filter);
			var expectedJson = $"{{\"filterType\":\"{filterType}\",\"left\":{{\"filterType\":\"Variable\",\"variableName\":\"x\"}},\"right\":{{\"filterType\":\"Constant\",\"value\":123}}}}";
			
			Assert.Equal(expectedJson, json);
		}

		[Theory]
		[InlineData(FilterType.Equal)]
		[InlineData(FilterType.NotEqual)]
		[InlineData(FilterType.GreaterThan)]
		[InlineData(FilterType.GreaterThanOrEqual)]
		[InlineData(FilterType.LessThan)]
		[InlineData(FilterType.LessThanOrEqual)]
		[InlineData(FilterType.And)]
		[InlineData(FilterType.Or)]
		public static void DeserializeBinaryFilter(FilterType filterType)
		{
			var json = $"{{\"filterType\":\"{filterType}\",\"left\":{{\"filterType\":\"Variable\",\"variableName\":\"x\"}},\"right\":{{\"filterType\":\"Constant\",\"value\":123}}}}";
			
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var binary = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(filterType, binary.FilterType);

			var left = Assert.IsType<VariableFilter>(binary.Left);
			Assert.Equal("x", left.VariableName);

			var right = Assert.IsType<ConstantFilter>(binary.Right);
			Assert.Equal(123, right.Value);
		}

		[Fact]
		public static void SerializeNestedBinaryFilter()
		{
			var innerLeft = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);
			var innerRight = Filter.Binary(Filter.Variable("y"), Filter.Constant(20), FilterType.LessThan);
			var filter = Filter.Binary(innerLeft, innerRight, FilterType.And);

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
			var binary = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.And, binary.FilterType);

			var leftBinary = Assert.IsType<BinaryFilter>(binary.Left);
			Assert.Equal(FilterType.GreaterThan, leftBinary.FilterType);

			var rightBinary = Assert.IsType<BinaryFilter>(binary.Right);
			Assert.Equal(FilterType.LessThan, rightBinary.FilterType);
		}

		#endregion

		#region Unary Filter Tests

		[Fact]
		public static void SerializeUnaryFilter()
		{
			var operand = Filter.Binary(Filter.Variable("x"), Filter.Constant(123), FilterType.Equal);
			var filter = Filter.Not(operand);

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
			var unary = Assert.IsType<UnaryFilter>(filter);
			Assert.Equal(FilterType.Not, unary.FilterType);

			var operand = Assert.IsType<BinaryFilter>(unary.Operand);
			Assert.Equal(FilterType.Equal, operand.FilterType);
		}

		[Fact]
		public static void SerializeNestedUnaryFilter()
		{
			var innerOperand = Filter.Variable("x");
			var innerUnary = Filter.Not(innerOperand);
			var filter = Filter.Not(innerUnary);

			var json = Serialize(filter);
			var expectedJson = "{\"filterType\":\"Not\",\"operand\":{\"filterType\":\"Not\",\"operand\":{\"filterType\":\"Variable\",\"variableName\":\"x\"}}}";
			
			Assert.Equal(expectedJson, json);
		}

		#endregion

		#region Function Filter Tests

		[Fact]
		public static void SerializeFunctionFilterWithoutArguments()
		{
			var variable = Filter.Variable("user");
			var filter = Filter.Function(variable, "isActive");

			var json = Serialize(filter);
			var expectedJson = "{\"filterType\":\"Function\",\"variable\":{\"filterType\":\"Variable\",\"variableName\":\"user\"},\"functionName\":\"isActive\"}";
			
			Assert.Equal(expectedJson, json);
		}

		[Fact]
		public static void SerializeFunctionFilterWithArguments()
		{
			var variable = Filter.Variable("user");
			var args = new Filter[] { Filter.Constant("admin"), Filter.Variable("role") };
			var filter = Filter.Function(variable, "hasRole", args);

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
			var function = Assert.IsType<FunctionFilter>(filter);
			Assert.Equal(FilterType.Function, function.FilterType);
			Assert.Equal("isActive", function.FunctionName);

			var variable = Assert.IsType<VariableFilter>(function.Variable);
			Assert.Equal("user", variable.VariableName);

			Assert.Empty(function.Arguments);
		}

		[Fact]
		public static void DeserializeFunctionFilterWithArguments()
		{
			var json = "{\"filterType\":\"Function\",\"variable\":{\"filterType\":\"Variable\",\"variableName\":\"user\"},\"functionName\":\"hasRole\",\"arguments\":[{\"filterType\":\"Constant\",\"value\":\"admin\"},{\"filterType\":\"Variable\",\"variableName\":\"role\"}]}";
			
			var filter = Deserialize(json);
			Assert.NotNull(filter);
			var function = Assert.IsType<FunctionFilter>(filter);
			Assert.Equal(FilterType.Function, function.FilterType);
			Assert.Equal("hasRole", function.FunctionName);

			var variable = Assert.IsType<VariableFilter>(function.Variable);
			Assert.Equal("user", variable.VariableName);

			Assert.NotNull(function.Arguments);
			Assert.Equal(2, function.Arguments.Length);

			var firstArg = Assert.IsType<ConstantFilter>(function.Arguments[0]);
			Assert.Equal("admin", firstArg.Value);

			var secondArg = Assert.IsType<VariableFilter>(function.Arguments[1]);
			Assert.Equal("role", secondArg.VariableName);
		}

		#endregion

		#region Round-trip Tests

		[Fact]
		public static void RoundTripConstantFilter()
		{
			var original = Filter.Constant("test value");
			var json = Serialize(original);
			var deserialized = Deserialize(json);

			Assert.NotNull(deserialized);
			var constant = Assert.IsType<ConstantFilter>(deserialized);
			Assert.Equal(original.Value, constant.Value);
		}

		[Fact]
		public static void RoundTripVariableFilter()
		{
			var original = Filter.Variable("test.variable");
			var json = Serialize(original);
			var deserialized = Deserialize(json);

			Assert.NotNull(deserialized);
			var variable = Assert.IsType<VariableFilter>(deserialized);
			Assert.Equal(original.VariableName, variable.VariableName);
		}

		[Fact]
		public static void RoundTripComplexFilter()
		{
			var original = Filter.And(
				Filter.Binary(Filter.Variable("age"), Filter.Constant(18), FilterType.GreaterThanOrEqual),
				Filter.Or(
					Filter.Binary(Filter.Variable("status"), Filter.Constant("active"), FilterType.Equal),
					Filter.Not(Filter.Binary(Filter.Variable("blocked"), Filter.Constant(true), FilterType.Equal))
				)
			);

			var json = Serialize(original);
			var deserialized = Deserialize(json);

			Assert.NotNull(deserialized);
			Assert.IsType<BinaryFilter>(deserialized);
			Assert.Equal(FilterType.And, deserialized.FilterType);
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
			var filter = Filter.Constant(value);
			var json = Serialize(filter);
			Assert.Contains("filterType", json);
			Assert.Contains("Constant", json);
		}

		[Fact]
		public static void SerializeFilterWithDateTime()
		{
			var dateTime = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);
			var filter = Filter.Constant(dateTime);
			var json = Serialize(filter);
			
			Assert.Contains("filterType", json);
			Assert.Contains("Constant", json);
		}

		[Fact]
		public static void RoundTripFilterWithDateTime()
		{
			var original = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);
			var filter = Filter.Constant(original);
			var json = Serialize(filter);
			var deserialized = Deserialize(json);

			Assert.NotNull(deserialized);
			var constant = Assert.IsType<ConstantFilter>(deserialized);
			Assert.IsType<DateTime>(constant.Value);
		}

		[Fact]
		public static void FilterWithDateTimeOffsetConvertsToDateTime()
		{
			var original = new DateTimeOffset(2023, 12, 25, 10, 30, 0, TimeSpan.FromHours(2));
			var filter = Filter.Constant(original);
			var json = Serialize(filter);
			var deserialized = Deserialize(json);
			Assert.NotNull(deserialized);
			var constant = Assert.IsType<ConstantFilter>(deserialized);
			// TODO: Handle DateTimeOffset if needed (this is a limitation of System.Text.Json)
			Assert.IsType<DateTime>(constant.Value);
		}

		[Fact]
		public static void SerializeFilterWithNullValue()
		{
			var filter = Filter.Constant(null);
			var json = Serialize(filter);
			Assert.Equal("{\"filterType\":\"Constant\",\"value\":null}", json);
		}

		#endregion
	}
}
