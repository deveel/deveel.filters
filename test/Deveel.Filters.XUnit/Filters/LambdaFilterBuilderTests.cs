namespace Deveel.Filters {
	public static class LambdaFilterBuilderTests {
		public class TestObj {
			public string Name { get; set; } = "";
			public int Value { get; set; }
			public Inner? Inner { get; set; }
		}

		public class Inner {
			public string Label { get; set; } = "";
		}

		#region Nested property access

		[Fact]
		public static void AsLambda_NestedProperty() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x.Inner.Label"), FilterExpression.Constant("test"), FilterExpressionType.Equal);
			var lambda = filter.AsLambda<TestObj>();
			var compiled = lambda.Compile();
			Assert.True(compiled(new TestObj { Inner = new Inner { Label = "test" } }));
			Assert.False(compiled(new TestObj { Inner = new Inner { Label = "other" } }));
		}

		#endregion

		#region Function on parameter expression

		[Fact]
		public static void AsLambda_FunctionOnParameter() {
			// x.StartsWith("foo") where x is string
			var filter = FilterExpression.Function(FilterExpression.Variable("x"), "StartsWith", new FilterExpression[] { FilterExpression.Constant("foo") });
			var lambda = filter.AsLambda<string>();
			var compiled = lambda.Compile();
			Assert.True(compiled("foobar"));
			Assert.False(compiled("barfoo"));
		}

		#endregion

		#region Function on member expression

		[Fact]
		public static void AsLambda_FunctionOnMember() {
			var filter = FilterExpression.Function(FilterExpression.Variable("x.Name"), "Contains", new FilterExpression[] { FilterExpression.Constant("oo") });
			var lambda = filter.AsLambda<TestObj>();
			var compiled = lambda.Compile();
			Assert.True(compiled(new TestObj { Name = "foobar" }));
			Assert.False(compiled(new TestObj { Name = "bar" }));
		}

		#endregion

		#region Function not found

		[Fact]
		public static void AsLambda_FunctionNotFound_Throws() {
			var filter = FilterExpression.Function(FilterExpression.Variable("x"), "NonExistentMethod", new FilterExpression[] { FilterExpression.Constant("a") });
			Assert.Throws<FilterException>(() => filter.AsLambda<string>());
		}

		#endregion

		#region Binary with one empty side (non-logical)

		[Fact]
		public static void AsLambda_NonLogicalBinaryWithEmptyLeft_Throws() {
			var filter = FilterExpression.Binary(FilterExpression.Empty, FilterExpression.Constant(1), FilterExpressionType.Equal);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		[Fact]
		public static void AsLambda_NonLogicalBinaryWithEmptyRight_Throws() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Empty, FilterExpressionType.Equal);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		#endregion

		#region Unary with empty operand

		[Fact]
		public static void AsLambda_UnaryWithEmptyOperand_Throws() {
			var filter = FilterExpression.Unary(FilterExpression.Empty, FilterExpressionType.Not);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		#endregion

		#region Async lambda

		[Fact]
		public static async Task BuildAsyncLambda_Simple() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(10), FilterExpressionType.GreaterThan);
			var lambda = filter.AsAsyncLambda<int>();
			var compiled = lambda.Compile();
			Assert.True(await compiled(20));
			Assert.False(await compiled(5));
		}

		#endregion

		#region Logical binary with empty sides

		[Fact]
		public static void AsLambda_AndWithLeftEmpty() {
			var filter = FilterExpression.Binary(FilterExpression.Empty, FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(5), FilterExpressionType.GreaterThan), FilterExpressionType.And);
			var lambda = filter.AsLambda<int>();
			var compiled = lambda.Compile();
			Assert.True(compiled(10));
			Assert.False(compiled(3));
		}

		[Fact]
		public static void AsLambda_OrWithRightEmpty() {
			var filter = FilterExpression.Binary(FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(5), FilterExpressionType.GreaterThan), FilterExpression.Empty, FilterExpressionType.Or);
			var lambda = filter.AsLambda<int>();
			var compiled = lambda.Compile();
			Assert.True(compiled(10));
			Assert.False(compiled(3));
		}

		[Fact]
		public static void AsLambda_AndWithBothEmpty_Throws() {
			var filter = FilterExpression.Binary(FilterExpression.Empty, FilterExpression.Empty, FilterExpressionType.And);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		#endregion
	}
}

