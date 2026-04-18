namespace Deveel.Filters {
	[Trait("Feature", "String Building")]
	public static class FilterStringTests {
		[Theory]
		[InlineData("a", FilterExpressionType.Equal, 1, "a == 1")]
		[InlineData("a", FilterExpressionType.NotEqual, 1, "a != 1")]
		[InlineData("a", FilterExpressionType.GreaterThan, 1, "a > 1")]
		[InlineData("a", FilterExpressionType.GreaterThanOrEqual, 1, "a >= 1")]
		[InlineData("a", FilterExpressionType.LessThan, 1, "a < 1")]
		[InlineData("a", FilterExpressionType.LessThanOrEqual, 1, "a <= 1")]
		public static void SimpleBinaryToString(string variable, FilterExpressionType expressionType, object value, string expected) {
			var variableFilter = FilterExpression.Variable(variable);
			var constantFilter = FilterExpression.Constant(value);
			var binaryFilter = FilterExpression.Binary(variableFilter, constantFilter, expressionType);
			var actual = binaryFilter.ToString();

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("a", FilterExpressionType.Equal, 1, "b", FilterExpressionType.Equal, 2, FilterExpressionType.And, "(a == 1) && (b == 2)")]
		[InlineData("a", FilterExpressionType.Equal, 1, "b", FilterExpressionType.Equal, 2, FilterExpressionType.Or, "(a == 1) || (b == 2)")]
		public static void BinaryWithLogicalToString(string leftVar, FilterExpressionType leftType, object leftValue, string rightVar, FilterExpressionType rightType, object rightValue, FilterExpressionType logicalType, string expected) {
			var left = FilterExpression.Binary(FilterExpression.Variable(leftVar), FilterExpression.Constant(leftValue), leftType);
			var right = FilterExpression.Binary(FilterExpression.Variable(rightVar), FilterExpression.Constant(rightValue), rightType);

			var logicalFilter = FilterExpression.Binary(left, right, logicalType);
			var actual = logicalFilter.ToString();
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("a", FilterExpressionType.Equal, 1, FilterExpressionType.And, "a == 1")]
		[InlineData("a", FilterExpressionType.NotEqual, 1, FilterExpressionType.And, "a != 1")]
		[InlineData("a", FilterExpressionType.GreaterThan, 1, FilterExpressionType.And, "a > 1")]
		[InlineData("a", FilterExpressionType.GreaterThanOrEqual, 1, FilterExpressionType.And, "a >= 1")]
		[InlineData("a", FilterExpressionType.LessThan, 1, FilterExpressionType.And, "a < 1")]
		[InlineData("a", FilterExpressionType.LessThanOrEqual, 1, FilterExpressionType.And, "a <= 1")]
		[InlineData("a", FilterExpressionType.Equal, 1, FilterExpressionType.Or, "a == 1")]
		[InlineData("a", FilterExpressionType.NotEqual, 1, FilterExpressionType.Or, "a != 1")]
		[InlineData("a", FilterExpressionType.GreaterThan, 1, FilterExpressionType.Or, "a > 1")]
		[InlineData("a", FilterExpressionType.GreaterThanOrEqual, 1, FilterExpressionType.Or, "a >= 1")]
		[InlineData("a", FilterExpressionType.LessThan, 1, FilterExpressionType.Or, "a < 1")]
		[InlineData("a", FilterExpressionType.LessThanOrEqual, 1, FilterExpressionType.Or, "a <= 1")]
		public static void LogicalBinaryOfEmptyWithNotEmpty(string leftVar, FilterExpressionType leftType, object leftValue, FilterExpressionType logicalType, string expected) {
			var left = FilterExpression.Binary(FilterExpression.Variable(leftVar), FilterExpression.Constant(leftValue), leftType);
			var right = FilterExpression.Empty;

			var logicalFilter = FilterExpression.Binary(left, right, logicalType);
			var actual = logicalFilter.ToString();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public static void LogicalBinaryOfEmptyParts() {
			var logicalFilter = FilterExpression.Binary(FilterExpression.Empty, FilterExpression.Empty, FilterExpressionType.And);

			Assert.Throws<FilterException>(() => logicalFilter.ToString());
		}

		[Theory]
		[InlineData("a", FilterExpressionType.Equal, 1, FilterExpressionType.Not, "!(a == 1)")]
		public static void UnaryOfBinaryToString(string variable, FilterExpressionType expressionType, object value, FilterExpressionType unaryType, string expected) {
			var variableFilter = FilterExpression.Variable(variable);
			var constantFilter = FilterExpression.Constant(value);
			var binaryFilter = FilterExpression.Binary(variableFilter, constantFilter, expressionType);
			var unaryFilter = FilterExpression.Unary(binaryFilter, unaryType);
			var actual = unaryFilter.ToString();
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(false, FilterExpressionType.Not, "!(false)")]
		public static void UnaryOfConstantToString(object value, FilterExpressionType expressionType, string expected) {
			var constantFilter = FilterExpression.Constant(value);
			var unaryFilter = FilterExpression.Unary(constantFilter, expressionType);
			var actual = unaryFilter.ToString();
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("a", "func", 1, "a.func(1)")]
		[InlineData("a", "StartsWith", "foo", "a.StartsWith(\"foo\")")]
		public static void FunctionToString(string variable, string functionName, object arg, string expected) {
			var variableFilter = FilterExpression.Variable(variable);
			var constantFilter = FilterExpression.Constant(arg);
			var functionFilter = FilterExpression.Function(variableFilter, functionName, new[] { constantFilter });
			var actual = functionFilter.ToString();
			Assert.Equal(expected, actual);
		}
	}
}
