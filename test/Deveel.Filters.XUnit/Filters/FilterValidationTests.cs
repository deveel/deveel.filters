namespace Deveel.Filters {
	public static class FilterValidationTests {
		#region Filter.IsValidReference

		[Theory]
		[InlineData("x", true)]
		[InlineData("x.y", true)]
		[InlineData("x.y.z", true)]
		[InlineData("_private", true)]
		[InlineData("x2", true)]
		[InlineData("", false)]
		[InlineData(" ", false)]
		[InlineData(null, false)]
		[InlineData("x-y", false)]
		[InlineData("x y", false)]
		[InlineData("x+y", false)]
		public static void IsValidReference(string? variableName, bool expected) {
			Assert.Equal(expected, FilterExpression.IsValidReference(variableName!));
		}

		#endregion

		#region Filter.Binary invalid type

		[Theory]
		[InlineData(FilterExpressionType.Not)]
		[InlineData(FilterExpressionType.Function)]
		[InlineData(FilterExpressionType.Constant)]
		[InlineData(FilterExpressionType.Variable)]
		public static void CreateBinaryWithInvalidFilterType(FilterExpressionType expressionType) {
			Assert.Throws<ArgumentException>(() =>
				FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), expressionType));
		}

		#endregion

		#region Filter.Unary invalid type

		[Theory]
		[InlineData(FilterExpressionType.Equal)]
		[InlineData(FilterExpressionType.And)]
		[InlineData(FilterExpressionType.Or)]
		[InlineData(FilterExpressionType.Function)]
		public static void CreateUnaryWithInvalidFilterType(FilterExpressionType expressionType) {
			Assert.Throws<ArgumentException>(() =>
				FilterExpression.Unary(FilterExpression.Variable("x"), expressionType));
		}

		#endregion

		#region FunctionFilter validation

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public static void CreateFunctionWithInvalidName(string? funcName) {
			Assert.Throws<ArgumentException>(() =>
				FilterExpression.Function(FilterExpression.Variable("x"), funcName!));
		}

		[Fact]
		public static void CreateFunctionWithInvalidArgumentType() {
			// Arguments that are binary filters should not be allowed
			var binaryArg = FilterExpression.Binary(FilterExpression.Variable("a"), FilterExpression.Constant(1), FilterExpressionType.Equal);
			Assert.Throws<ArgumentException>(() =>
				FilterExpression.Function(FilterExpression.Variable("x"), "test", new FilterExpression[] { binaryArg }));
		}

		[Fact]
		public static void CreateFunctionWithNoArguments() {
			var func = FilterExpression.Function(FilterExpression.Variable("x"), "Any");
			Assert.NotNull(func);
			Assert.Equal("Any", func.FunctionName);
			Assert.Equal("x", func.Variable.VariableName);
			Assert.Empty(func.Arguments);
		}

		[Fact]
		public static void CreateFunctionWithVariableArgument() {
			var func = FilterExpression.Function(FilterExpression.Variable("x"), "test", new FilterExpression[] { FilterExpression.Variable("y") });
			Assert.NotNull(func);
			Assert.Single(func.Arguments);
			Assert.IsType<VariableFilterExpression>(func.Arguments[0]);
		}

		#endregion

		#region Filter.And / Or / Equal shortcuts

		[Fact]
		public static void CreateAndFilter() {
			var filter = FilterExpression.And(
				FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), FilterExpressionType.Equal),
				FilterExpression.Binary(FilterExpression.Variable("y"), FilterExpression.Constant(2), FilterExpressionType.Equal));
			Assert.Equal(FilterExpressionType.And, filter.ExpressionType);
		}

		[Fact]
		public static void CreateOrFilter() {
			var filter = FilterExpression.Or(
				FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), FilterExpressionType.Equal),
				FilterExpression.Binary(FilterExpression.Variable("y"), FilterExpression.Constant(2), FilterExpressionType.Equal));
			Assert.Equal(FilterExpressionType.Or, filter.ExpressionType);
		}

		#endregion
	}
}

