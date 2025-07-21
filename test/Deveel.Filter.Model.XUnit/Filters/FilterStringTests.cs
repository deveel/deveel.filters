namespace Deveel.Filters {
	[Trait("Feature", "String Building")]
	public static class FilterStringTests {
		[Theory]
		[InlineData("a", FilterType.Equal, 1, "a == 1")]
		[InlineData("a", FilterType.NotEqual, 1, "a != 1")]
		[InlineData("a", FilterType.GreaterThan, 1, "a > 1")]
		[InlineData("a", FilterType.GreaterThanOrEqual, 1, "a >= 1")]
		[InlineData("a", FilterType.LessThan, 1, "a < 1")]
		[InlineData("a", FilterType.LessThanOrEqual, 1, "a <= 1")]
		public static void SimpleBinaryToString(string variable, FilterType filterType, object value, string expected) {
			var variableFilter = Filter.Variable(variable);
			var constantFilter = Filter.Constant(value);
			var binaryFilter = Filter.Binary(variableFilter, constantFilter, filterType);
			var actual = binaryFilter.ToString();

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("a", FilterType.Equal, 1, "b", FilterType.Equal, 2, FilterType.And, "(a == 1) && (b == 2)")]
		[InlineData("a", FilterType.Equal, 1, "b", FilterType.Equal, 2, FilterType.Or, "(a == 1) || (b == 2)")]
		public static void BinaryWithLogicalToString(string leftVar, FilterType leftType, object leftValue, string rightVar, FilterType rightType, object rightValue, FilterType logicalType, string expected) {
			var left = Filter.Binary(Filter.Variable(leftVar), Filter.Constant(leftValue), leftType);
			var right = Filter.Binary(Filter.Variable(rightVar), Filter.Constant(rightValue), rightType);

			var logicalFilter = Filter.Binary(left, right, logicalType);
			var actual = logicalFilter.ToString();
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("a", FilterType.Equal, 1, FilterType.And, "a == 1")]
		[InlineData("a", FilterType.NotEqual, 1, FilterType.And, "a != 1")]
		[InlineData("a", FilterType.GreaterThan, 1, FilterType.And, "a > 1")]
		[InlineData("a", FilterType.GreaterThanOrEqual, 1, FilterType.And, "a >= 1")]
		[InlineData("a", FilterType.LessThan, 1, FilterType.And, "a < 1")]
		[InlineData("a", FilterType.LessThanOrEqual, 1, FilterType.And, "a <= 1")]
		[InlineData("a", FilterType.Equal, 1, FilterType.Or, "a == 1")]
		[InlineData("a", FilterType.NotEqual, 1, FilterType.Or, "a != 1")]
		[InlineData("a", FilterType.GreaterThan, 1, FilterType.Or, "a > 1")]
		[InlineData("a", FilterType.GreaterThanOrEqual, 1, FilterType.Or, "a >= 1")]
		[InlineData("a", FilterType.LessThan, 1, FilterType.Or, "a < 1")]
		[InlineData("a", FilterType.LessThanOrEqual, 1, FilterType.Or, "a <= 1")]
		public static void LogicalBinaryOfEmptyWithNotEmpty(string leftVar, FilterType leftType, object leftValue, FilterType logicalType, string expected) {
			var left = Filter.Binary(Filter.Variable(leftVar), Filter.Constant(leftValue), leftType);
			var right = Filter.Empty;

			var logicalFilter = Filter.Binary(left, right, logicalType);
			var actual = logicalFilter.ToString();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public static void LogicalBinaryOfEmptyParts() {
			var logicalFilter = Filter.Binary(Filter.Empty, Filter.Empty, FilterType.And);

			Assert.Throws<FilterException>(() => logicalFilter.ToString());
		}

		[Theory]
		[InlineData("a", FilterType.Equal, 1, FilterType.Not, "!(a == 1)")]
		public static void UnaryOfBinaryToString(string variable, FilterType filterType, object value, FilterType unaryType, string expected) {
			var variableFilter = Filter.Variable(variable);
			var constantFilter = Filter.Constant(value);
			var binaryFilter = Filter.Binary(variableFilter, constantFilter, filterType);
			var unaryFilter = Filter.Unary(binaryFilter, unaryType);
			var actual = unaryFilter.ToString();
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(false, FilterType.Not, "!(false)")]
		public static void UnaryOfConstantToString(object value, FilterType filterType, string expected) {
			var constantFilter = Filter.Constant(value);
			var unaryFilter = Filter.Unary(constantFilter, filterType);
			var actual = unaryFilter.ToString();
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("a", "func", 1, "a.func(1)")]
		[InlineData("a", "StartsWith", "foo", "a.StartsWith(\"foo\")")]
		public static void FunctionToString(string variable, string functionName, object arg, string expected) {
			var variableFilter = Filter.Variable(variable);
			var constantFilter = Filter.Constant(arg);
			var functionFilter = Filter.Function(variableFilter, functionName, new[] { constantFilter });
			var actual = functionFilter.ToString();
			Assert.Equal(expected, actual);
		}
	}
}
