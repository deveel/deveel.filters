namespace Deveel.Filters {
	public static class FilterExtensionTests {
		#region AsLambda error paths

		[Fact]
		public static void AsLambda_NullParameterType_Throws() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(1), FilterType.Equal);
			Assert.Throws<ArgumentNullException>(() => filter.AsLambda(null!));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public static void AsLambda_InvalidParameterName_Throws(string? paramName) {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(1), FilterType.Equal);
			Assert.Throws<ArgumentException>(() => filter.AsLambda(typeof(int), paramName!));
		}

		[Fact]
		public static void AsLambda_InvalidMember_ThrowsFilterException() {
			var filter = Filter.Binary(Filter.Variable("nonExistent"), Filter.Constant(1), FilterType.Equal);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		#endregion

		#region AsAsyncLambda error paths

		[Fact]
		public static void AsAsyncLambda_NullParameterType_Throws() {
			var filter = Filter.Variable("x");
			Assert.Throws<ArgumentNullException>(() => filter.AsAsyncLambda(null!));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public static void AsAsyncLambda_InvalidParameterName_Throws(string? paramName) {
			var filter = Filter.Variable("x");
			Assert.Throws<ArgumentException>(() => filter.AsAsyncLambda(typeof(int), paramName!));
		}

		#endregion

		#region Evaluate error paths

		[Fact]
		public static void Evaluate_NullParameterType_Throws() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(1), FilterType.Equal);
			Assert.Throws<ArgumentNullException>(() => filter.Evaluate(null!, "x", 1));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public static void Evaluate_InvalidParameterName_Throws(string? paramName) {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(1), FilterType.Equal);
			Assert.Throws<ArgumentException>(() => filter.Evaluate(typeof(int), paramName!, 1));
		}

		[Fact]
		public static void Evaluate_InvalidFilter_ThrowsFilterEvaluationException() {
			var filter = Filter.Binary(Filter.Variable("nonExistent"), Filter.Constant(1), FilterType.Equal);
			Assert.Throws<FilterEvaluationException>(() => filter.Evaluate<int>("x", 1));
		}

		#endregion

		#region Evaluate with Type overloads

		[Fact]
		public static void Evaluate_WithType_DefaultParamName() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);
			Assert.True(filter.Evaluate(typeof(int), 20));
			Assert.False(filter.Evaluate(typeof(int), 5));
		}

		[Fact]
		public static void Evaluate_GenericDefaultParamName() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);
			Assert.True(filter.Evaluate(20));
			Assert.False(filter.Evaluate(5));
		}

		#endregion

		#region EvaluateAsync overloads

		[Fact]
		public static async Task EvaluateAsync_WithTypeAndName() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);
			Assert.True(await filter.EvaluateAsync(typeof(int), "x", 20));
		}

		[Fact]
		public static async Task EvaluateAsync_GenericWithName() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);
			Assert.True(await filter.EvaluateAsync("x", 20));
		}

		#endregion

		#region AsString

		[Fact]
		public static void ToString_Variable() {
			var filter = Filter.Variable("x");
			Assert.Equal("x", filter.ToString());
		}

		[Fact]
		public static void ToString_NullConstant() {
			var filter = Filter.Constant(null);
			Assert.Equal("null", filter.ToString());
		}

		[Fact]
		public static void ToString_BoolConstant() {
			Assert.Equal("true", Filter.Constant(true).ToString());
			Assert.Equal("false", Filter.Constant(false).ToString());
		}

		[Fact]
		public static void AsString_StringConstant() {
			Assert.Equal("\"hello\"", Filter.Constant("hello").ToString());
		}

		#endregion

		#region IsEmpty
		
		[Fact]
		public static void IsEmpty_EmptyFilter() {
			Assert.True(Filter.Empty.IsEmpty);
		}

		[Fact]
		public static void IsEmpty_NonEmptyFilter() {
			Assert.False(Filter.Variable("x").IsEmpty);
		}

		#endregion

		#region FilterVisitor with empty filter

		[Fact]
		public static void Visitor_EmptyFilter_ReturnsEmpty() {
			var visitor = new FilterVisitor();
			var result = visitor.Visit(Filter.Empty);
			Assert.Equal(Filter.Empty, result);
		}

		#endregion
	}
}

