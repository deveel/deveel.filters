namespace Deveel.Filters {
    [Trait("Feature", "Lambda")]
	public static class LambdaFunctionTests {
		[Fact]
		public static void SimpleBinaryAsLambda() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan);

			var result = filter.Evaluate("x", 20);

			Assert.True(result);
		}

		[Fact]
		public static void SimpleUnaryAsLambda() {
			var filter = FilterExpression.Unary(FilterExpression.Variable("x"), FilterExpressionType.Not);

			var result = filter.Evaluate("x", true);
			Assert.False(result);
		}

		[Fact]
		public static void ComplexBinaryWithFunctionCallAsLabda() {
			var filter = FilterExpression.Binary(
				FilterExpression.Function(FilterExpression.Variable("x"), "Substring", new[] { FilterExpression.Constant(1) }),
				FilterExpression.Constant("oobar"),
				FilterExpressionType.Equal);

			var result = filter.Evaluate("x", "foobar");
			Assert.True(result);
		}

		[Fact]
		public static void SimpleFunctionCallAslambda() {
			var filter = FilterExpression.Function(FilterExpression.Variable("x"), "StartsWith", new[] { FilterExpression.Constant("foo") });

			var result = filter.Evaluate("x", "foobar");
			Assert.True(result);
		}

		[Fact]
		public static void BinaryWithLogicalAsLambda() {
			var filter = FilterExpression.Binary(
					FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan),
					FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(20), FilterExpressionType.LessThan),
					FilterExpressionType.And);

			var result = filter.Evaluate(15);
			Assert.True(result);
		}

		[Fact]
		public static void LogicalBinaryWithLeftEmpty() {
			var filter = FilterExpression.Binary(FilterExpression.Empty, FilterExpression.GreaterThan(FilterExpression.Variable("x"), FilterExpression.Constant(10)), FilterExpressionType.And);
			var result = filter.Evaluate(15);
			Assert.True(result);
		}

		[Fact]
		public static void LogicalBinaryWithRightEmpty() {
			var filter = FilterExpression.Binary(FilterExpression.GreaterThan(FilterExpression.Variable("x"), FilterExpression.Constant(10)), FilterExpression.Empty, FilterExpressionType.And);
			var result = filter.Evaluate(15);
			Assert.True(result);
		}

		[Fact]
		public static void LogicalBinaryWithBothEmpty() {
			var filter = FilterExpression.Binary(FilterExpression.Empty, FilterExpression.Empty, FilterExpressionType.And);

			Assert.Throws<FilterEvaluationException>(() => filter.Evaluate(15));
		}

		[Theory]
		[InlineData("x", 10, FilterExpressionType.Equal, true)]
		[InlineData("x", 10, FilterExpressionType.NotEqual, false)]
		[InlineData("x", 10, FilterExpressionType.GreaterThan, false)]
		[InlineData("x", 10, FilterExpressionType.GreaterThanOrEqual, true)]
		[InlineData("x", 10, FilterExpressionType.LessThan, false)]
		[InlineData("x", 10, FilterExpressionType.LessThanOrEqual, true)]
		public static void FilterByBinaryAsLabda(string varName, object value, FilterExpressionType expressionType, bool expected) {
			var filter = FilterExpression.Binary(FilterExpression.Variable(varName), FilterExpression.Constant(value), expressionType);
			
			var result = filter.Evaluate(value.GetType(), value);

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("x", "2015-01-01", FilterExpressionType.Equal, true)]
		[InlineData("x", "2015-01-01", FilterExpressionType.NotEqual, false)]
		[InlineData("x", "2015-01-01", FilterExpressionType.GreaterThan, false)]
		[InlineData("x", "2015-01-01", FilterExpressionType.GreaterThanOrEqual, true)]
		[InlineData("x", "2015-01-01", FilterExpressionType.LessThan, false)]
		[InlineData("x", "2015-01-01", FilterExpressionType.LessThanOrEqual, true)]
		public static void FilterByDateTimeAsLambda(string varName, string dateTimeString, FilterExpressionType expressionType, bool expected) {
			var filter = FilterExpression.Binary(FilterExpression.Variable(varName), FilterExpression.Constant(DateTime.Parse(dateTimeString)), expressionType);

			var result = filter.Evaluate(DateTime.Parse(dateTimeString));

			Assert.Equal(expected, result);
		}

		[Fact]
		public static async Task SimpleBinaryAsAsyncLambda() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan);

			var result = await filter.EvaluateAsync(20);
			Assert.True(result);
		}

		[Fact]
		public static void SimpleBinaryLambdaOnComplexObject() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x.name"), FilterExpression.Constant("antonello"), FilterExpressionType.Equal);
			var obj = new {
				name = "antonello"
			};

			var result = filter.Evaluate(obj.GetType(), obj);

			Assert.True(result);
		}
	}
}
