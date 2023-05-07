using System.Linq.Expressions;

namespace Deveel.Filters {
	[Trait("Feature", "Lambda")]
	public static class LambdaFunctionTests {
		[Fact]
		public static void SimpleBinaryAsLambda() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);
			var lambda = filter.AsLambda<int>("x");
			var compiled = lambda.Compile();

			var result = compiled(20);

			Assert.True(result);
		}

		[Fact]
		public static void SimpleUnaryAsLambda() {
			var filter = Filter.Unary(Filter.Variable("x"), FilterType.Not);
			var lambda = filter.AsLambda<bool>("x");
			var compiled = lambda.Compile();
			var result = compiled(true);
			Assert.False(result);
		}

		[Fact]
		public static void ComplexBinaryWithFunctionCallAsLabda() {
			var filter = Filter.Binary(
				Filter.Function(Filter.Variable("x"), "Substring", Filter.Constant(1)),
				Filter.Constant("oobar"),
				FilterType.Equals);

			var lambda = filter.AsLambda<string>("x");
			var compiled = lambda.Compile();

			var result = compiled("foobar");
			Assert.True(result);
		}

		[Fact]
		public static void SimpleFunctionCallAslambda() {
			var filter = Filter.Function(Filter.Variable("x"), "StartsWith", Filter.Constant("foo"));
			var lambda = filter.AsLambda<string>("x");
			var compiled = lambda.Compile();
			var result = compiled("foobar");
			Assert.True(result);
		}

		[Fact]
		public static void BinaryWithLogicalAsLambda() {
			var filter = Filter.Binary(
					Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan),
					Filter.Binary(Filter.Variable("x"), Filter.Constant(20), FilterType.LessThan),
					FilterType.And);

			var lambda = filter.AsLambda<int>("x");
			var compiled = lambda.Compile();
			var result = compiled(15);
			Assert.True(result);
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
			var lambda = filter.AsLambda(value.GetType(), varName);
			var compiled = lambda.Compile();

			var result = compiled.DynamicInvoke(value);

			var b = Assert.IsType<bool>(result);
			Assert.Equal(expected, b);
		}

		[Fact]
		public static async Task SimpleBinaryAsAsyncLambda() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(10), FilterType.GreaterThan);
			var lambda = filter.AsAsyncLambda<int>("x");
			var compiled = lambda.Compile();
			var result = await compiled(20);
			Assert.True(result);
		}
	}
}
