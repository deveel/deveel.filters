namespace Deveel.Filters {
	public static class FactoryTests {
		[Theory]
		[InlineData("foo", 42, FilterExpressionType.Equal)]
		[InlineData("foo", 42, FilterExpressionType.NotEqual)]
		[InlineData("foo", 42, FilterExpressionType.GreaterThan)]
		[InlineData("foo", 42, FilterExpressionType.GreaterThanOrEqual)]
		[InlineData("foo", 42, FilterExpressionType.LessThan)]
		[InlineData("foo", 42, FilterExpressionType.LessThanOrEqual)]
		public static void CreateBinaryFilter(string varName, object value, FilterExpressionType expressionType) { 
			var filter = FilterExpression.Binary(FilterExpression.Variable(varName), FilterExpression.Constant(value), expressionType);

			Assert.NotNull(filter);
			Assert.Equal(expressionType, filter.ExpressionType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateEquals(string varName, object value) {
			var filter = FilterExpression.Equal(FilterExpression.Variable(varName), FilterExpression.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.Equal, filter.ExpressionType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateNotEquals(string varName, object value) {
			var filter = FilterExpression.NotEquals(FilterExpression.Variable(varName), FilterExpression.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.NotEqual, filter.ExpressionType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateGreaterThan(string varName, object value) {
			var filter = FilterExpression.GreaterThan(FilterExpression.Variable(varName), FilterExpression.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.GreaterThan, filter.ExpressionType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateGreaterThanOrEqual(string varName, object value) {
			var filter = FilterExpression.GreaterThanOrEqual(FilterExpression.Variable(varName), FilterExpression.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.GreaterThanOrEqual, filter.ExpressionType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateLessThan(string varName, object value) {
			var filter = FilterExpression.LessThan(FilterExpression.Variable(varName), FilterExpression.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.LessThan, filter.ExpressionType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateLessThanOrEqual(string varName, object value) {
			var filter = FilterExpression.LessThanOrEqual(FilterExpression.Variable(varName), FilterExpression.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.LessThanOrEqual, filter.ExpressionType);
		}

		[Theory]
		[InlineData("foo")]
		[InlineData(22)]
		[InlineData(123.456)]
		[InlineData(true)]
		[InlineData(false)]
		[InlineData(null)]
		public static void CreateConstant(object? value) {
			var filter = FilterExpression.Constant(value);
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.Constant, filter.ExpressionType);
		}

		[Theory]
		[InlineData("foo")]
		[InlineData("bar")]
		[InlineData("baz")]
		public static void CreateVariable(string varName) {
			var filter = FilterExpression.Variable(varName);
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.Variable, filter.ExpressionType);
		}
	}
}
