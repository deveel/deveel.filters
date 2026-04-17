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
			var filter = Filter.Binary(Filter.Variable("x.Inner.Label"), Filter.Constant("test"), FilterType.Equal);
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
			var filter = Filter.Function(Filter.Variable("x"), "StartsWith", new Filter[] { Filter.Constant("foo") });
			var lambda = filter.AsLambda<string>();
			var compiled = lambda.Compile();
			Assert.True(compiled("foobar"));
			Assert.False(compiled("barfoo"));
		}

		#endregion

		#region Function on member expression

		[Fact]
		public static void AsLambda_FunctionOnMember() {
			var filter = Filter.Function(Filter.Variable("x.Name"), "Contains", new Filter[] { Filter.Constant("oo") });
			var lambda = filter.AsLambda<TestObj>();
			var compiled = lambda.Compile();
			Assert.True(compiled(new TestObj { Name = "foobar" }));
			Assert.False(compiled(new TestObj { Name = "bar" }));
		}

		#endregion

		#region Function not found

		[Fact]
		public static void AsLambda_FunctionNotFound_Throws() {
			var filter = Filter.Function(Filter.Variable("x"), "NonExistentMethod", new Filter[] { Filter.Constant("a") });
			Assert.Throws<FilterException>(() => filter.AsLambda<string>());
		}

		#endregion

		#region Binary with one empty side (non-logical)

		[Fact]
		public static void AsLambda_NonLogicalBinaryWithEmptyLeft_Throws() {
			var filter = Filter.Binary(Filter.Empty, Filter.Constant(1), FilterType.Equal);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		[Fact]
		public static void AsLambda_NonLogicalBinaryWithEmptyRight_Throws() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Empty, FilterType.Equal);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		#endregion

		#region Unary with empty operand

		[Fact]
		public static void AsLambda_UnaryWithEmptyOperand_Throws() {
			var filter = Filter.Unary(Filter.Empty, FilterType.Not);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		#endregion

		#region Async lambda

		[Fact]
		public static async Task BuildAsyncLambda_Simple() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);
			var lambda = filter.AsAsyncLambda<int>();
			var compiled = lambda.Compile();
			Assert.True(await compiled(20));
			Assert.False(await compiled(5));
		}

		#endregion

		#region Logical binary with empty sides

		[Fact]
		public static void AsLambda_AndWithLeftEmpty() {
			var filter = Filter.Binary(Filter.Empty, Filter.Binary(Filter.Variable("x"), Filter.Constant(5), FilterType.GreaterThan), FilterType.And);
			var lambda = filter.AsLambda<int>();
			var compiled = lambda.Compile();
			Assert.True(compiled(10));
			Assert.False(compiled(3));
		}

		[Fact]
		public static void AsLambda_OrWithRightEmpty() {
			var filter = Filter.Binary(Filter.Binary(Filter.Variable("x"), Filter.Constant(5), FilterType.GreaterThan), Filter.Empty, FilterType.Or);
			var lambda = filter.AsLambda<int>();
			var compiled = lambda.Compile();
			Assert.True(compiled(10));
			Assert.False(compiled(3));
		}

		[Fact]
		public static void AsLambda_AndWithBothEmpty_Throws() {
			var filter = Filter.Binary(Filter.Empty, Filter.Empty, FilterType.And);
			Assert.Throws<FilterException>(() => filter.AsLambda<int>());
		}

		#endregion
	}
}

