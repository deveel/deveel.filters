namespace Deveel.Filters {
	public static class FilterExtensionTests {
		#region AsLambda error paths

		[Fact]
		public static void AsLambda_NullParameterType_Throws() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), FilterExpressionType.Equal);
			Assert.Throws<ArgumentNullException>(() => filter.AsLambda(null!));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public static void AsLambda_InvalidParameterName_Throws(string? paramName) {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), FilterExpressionType.Equal);
			Assert.Throws<ArgumentException>(() => filter.AsLambda(typeof(int), paramName!));
		}

		[Fact]
		public static void AsLambda_InvalidMember_ThrowsFilterException() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("nonExistent"), FilterExpression.Constant(1), FilterExpressionType.Equal);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		#endregion

		#region AsAsyncLambda error paths

		[Fact]
		public static void AsAsyncLambda_NullParameterType_Throws() {
			var filter = FilterExpression.Variable("x");
			Assert.Throws<ArgumentNullException>(() => filter.AsAsyncLambda(null!));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public static void AsAsyncLambda_InvalidParameterName_Throws(string? paramName) {
			var filter = FilterExpression.Variable("x");
			Assert.Throws<ArgumentException>(() => filter.AsAsyncLambda(typeof(int), paramName!));
		}

		#endregion

		#region Evaluate error paths

		[Fact]
		public static void Evaluate_NullParameterType_Throws() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), FilterExpressionType.Equal);
			Assert.Throws<ArgumentNullException>(() => filter.Evaluate(null!, "x", 1));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("  ")]
		public static void Evaluate_InvalidParameterName_Throws(string? paramName) {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), FilterExpressionType.Equal);
			Assert.Throws<ArgumentException>(() => filter.Evaluate(typeof(int), paramName!, 1));
		}

		[Fact]
		public static void Evaluate_InvalidFilter_ThrowsFilterEvaluationException() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("nonExistent"), FilterExpression.Constant(1), FilterExpressionType.Equal);
			Assert.Throws<FilterEvaluationException>(() => filter.Evaluate<int>("x", 1));
		}

		#endregion

		#region Evaluate with Type overloads

		[Fact]
		public static void Evaluate_WithType_DefaultParamName() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan);
			Assert.True(filter.Evaluate(typeof(int), 20));
			Assert.False(filter.Evaluate(typeof(int), 5));
		}

		[Fact]
		public static void Evaluate_GenericDefaultParamName() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan);
			Assert.True(filter.Evaluate(20));
			Assert.False(filter.Evaluate(5));
		}

		#endregion

		#region EvaluateAsync overloads

		[Fact]
		public static async Task EvaluateAsync_WithTypeAndName() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan);
			Assert.True(await filter.EvaluateAsync(typeof(int), "x", 20));
		}

		[Fact]
		public static async Task EvaluateAsync_GenericWithName() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan);
			Assert.True(await filter.EvaluateAsync("x", 20));
		}

		#endregion

		#region AsString

		[Fact]
		public static void ToString_Variable() {
			var filter = FilterExpression.Variable("x");
			Assert.Equal("x", filter.ToString());
		}

		[Fact]
		public static void ToString_NullConstant() {
			var filter = FilterExpression.Constant(null);
			Assert.Equal("null", filter.ToString());
		}

		[Fact]
		public static void ToString_BoolConstant() {
			Assert.Equal("true", FilterExpression.Constant(true).ToString());
			Assert.Equal("false", FilterExpression.Constant(false).ToString());
		}

		[Fact]
		public static void AsString_StringConstant() {
			Assert.Equal("\"hello\"", FilterExpression.Constant("hello").ToString());
		}

		#endregion

		#region IsEmpty
		
		[Fact]
		public static void IsEmpty_EmptyFilter() {
			Assert.True(FilterExpression.Empty.IsEmpty);
		}

		[Fact]
		public static void IsEmpty_NonEmptyFilter() {
			Assert.False(FilterExpression.Variable("x").IsEmpty);
		}

		#endregion

		#region FilterVisitor with empty filter

		[Fact]
		public static void Visitor_EmptyFilter_ReturnsEmpty() {
			var visitor = new FilterExpressionVisitor();
			var result = visitor.Visit(FilterExpression.Empty);
			Assert.Equal(FilterExpression.Empty, result);
		}

		#endregion
	}
}

