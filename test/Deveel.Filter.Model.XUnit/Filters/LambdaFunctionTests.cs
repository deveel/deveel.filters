namespace Deveel.Filters {
    [Trait("Feature", "Lambda")]
	public static class LambdaFunctionTests {
		[Fact]
		public static void SimpleBinaryAsLambda() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);

			var result = filter.Evaluate("x", 20);

			Assert.True(result);
		}

		[Fact]
		public static void SimpleUnaryAsLambda() {
			var filter = Filter.Unary(Filter.Variable("x"), FilterType.Not);

			var result = filter.Evaluate("x", true);
			Assert.False(result);
		}

		[Fact]
		public static void ComplexBinaryWithFunctionCallAsLabda() {
			var filter = Filter.Binary(
				Filter.Function(Filter.Variable("x"), "Substring", Filter.Constant(1)),
				Filter.Constant("oobar"),
				FilterType.Equals);

			var result = filter.Evaluate("x", "foobar");
			Assert.True(result);
		}

		[Fact]
		public static void SimpleFunctionCallAslambda() {
			var filter = Filter.Function(Filter.Variable("x"), "StartsWith", Filter.Constant("foo"));

			var result = filter.Evaluate("x", "foobar");
			Assert.True(result);
		}

		[Fact]
		public static void BinaryWithLogicalAsLambda() {
			var filter = Filter.Binary(
					Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan),
					Filter.Binary(Filter.Variable("x"), Filter.Constant(20), FilterType.LessThan),
					FilterType.And);

			var result = filter.Evaluate(15);
			Assert.True(result);
		}

		[Fact]
		public static void LogicalBinaryWithLeftEmpty() {
			var filter = Filter.Binary(Filter.Empty, Filter.GreaterThan(Filter.Variable("x"), Filter.Constant(10)), FilterType.And);
			var result = filter.Evaluate(15);
			Assert.True(result);
		}

		[Fact]
		public static void LogicalBinaryWithRightEmpty() {
			var filter = Filter.Binary(Filter.GreaterThan(Filter.Variable("x"), Filter.Constant(10)), Filter.Empty, FilterType.And);
			var result = filter.Evaluate(15);
			Assert.True(result);
		}

		[Fact]
		public static void LogicalBinaryWithBothEmpty() {
			var filter = Filter.Binary(Filter.Empty, Filter.Empty, FilterType.And);

			Assert.Throws<FilterEvaluationException>(() => filter.Evaluate(15));
		}

		[Theory]
		[InlineData("x", 10, FilterType.Equals, true)]
		[InlineData("x", 10, FilterType.NotEquals, false)]
		[InlineData("x", 10, FilterType.GreaterThan, false)]
		[InlineData("x", 10, FilterType.GreaterThanOrEqual, true)]
		[InlineData("x", 10, FilterType.LessThan, false)]
		[InlineData("x", 10, FilterType.LessThanOrEqual, true)]
		public static void FilterByBinaryAsLabda(string varName, object value, FilterType filterType, bool expected) {
			var filter = Filter.Binary(Filter.Variable(varName), Filter.Constant(value), filterType);
			
			var result = filter.Evaluate(value.GetType(), value);

			Assert.Equal(expected, result);
		}

		[Fact]
		public static async Task SimpleBinaryAsAsyncLambda() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);

			var result = await filter.EvaluateAsync(20);
			Assert.True(result);
		}

		[Fact]
		public static void SimpleBinaryLambdaOnComplexObject() {
			var filter = Filter.Binary(Filter.Variable("x.name"), Filter.Constant("antonello"), FilterType.Equals);
			var obj = new {
				name = "antonello"
			};

			var result = filter.Evaluate(obj.GetType(), obj);

			Assert.True(result);
		}
	}
}
