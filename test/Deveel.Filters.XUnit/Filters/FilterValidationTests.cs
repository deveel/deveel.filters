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
			Assert.Equal(expected, Filter.IsValidReference(variableName!));
		}

		#endregion

		#region Filter.Binary invalid type

		[Theory]
		[InlineData(FilterType.Not)]
		[InlineData(FilterType.Function)]
		[InlineData(FilterType.Constant)]
		[InlineData(FilterType.Variable)]
		public static void CreateBinaryWithInvalidFilterType(FilterType filterType) {
			Assert.Throws<ArgumentException>(() =>
				Filter.Binary(Filter.Variable("x"), Filter.Constant(1), filterType));
		}

		#endregion

		#region Filter.Unary invalid type

		[Theory]
		[InlineData(FilterType.Equal)]
		[InlineData(FilterType.And)]
		[InlineData(FilterType.Or)]
		[InlineData(FilterType.Function)]
		public static void CreateUnaryWithInvalidFilterType(FilterType filterType) {
			Assert.Throws<ArgumentException>(() =>
				Filter.Unary(Filter.Variable("x"), filterType));
		}

		#endregion

		#region FunctionFilter validation

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public static void CreateFunctionWithInvalidName(string? funcName) {
			Assert.Throws<ArgumentException>(() =>
				Filter.Function(Filter.Variable("x"), funcName!));
		}

		[Fact]
		public static void CreateFunctionWithInvalidArgumentType() {
			// Arguments that are binary filters should not be allowed
			var binaryArg = Filter.Binary(Filter.Variable("a"), Filter.Constant(1), FilterType.Equal);
			Assert.Throws<ArgumentException>(() =>
				Filter.Function(Filter.Variable("x"), "test", new Filter[] { binaryArg }));
		}

		[Fact]
		public static void CreateFunctionWithNoArguments() {
			var func = Filter.Function(Filter.Variable("x"), "Any");
			Assert.NotNull(func);
			Assert.Equal("Any", func.FunctionName);
			Assert.Equal("x", func.Variable.VariableName);
			Assert.Empty(func.Arguments);
		}

		[Fact]
		public static void CreateFunctionWithVariableArgument() {
			var func = Filter.Function(Filter.Variable("x"), "test", new Filter[] { Filter.Variable("y") });
			Assert.NotNull(func);
			Assert.Single(func.Arguments);
			Assert.IsType<VariableFilter>(func.Arguments[0]);
		}

		#endregion

		#region Filter.And / Or / Equal shortcuts

		[Fact]
		public static void CreateAndFilter() {
			var filter = Filter.And(
				Filter.Binary(Filter.Variable("x"), Filter.Constant(1), FilterType.Equal),
				Filter.Binary(Filter.Variable("y"), Filter.Constant(2), FilterType.Equal));
			Assert.Equal(FilterType.And, filter.FilterType);
		}

		[Fact]
		public static void CreateOrFilter() {
			var filter = Filter.Or(
				Filter.Binary(Filter.Variable("x"), Filter.Constant(1), FilterType.Equal),
				Filter.Binary(Filter.Variable("y"), Filter.Constant(2), FilterType.Equal));
			Assert.Equal(FilterType.Or, filter.FilterType);
		}

		#endregion
	}
}

