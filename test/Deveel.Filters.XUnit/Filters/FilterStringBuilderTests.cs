namespace Deveel.Filters {
	public static class FilterStringBuilderTests {
		[Fact]
		public static void ToString_UnaryOfEmptyOperand_Throws() {
			var filter = FilterExpression.Unary(FilterExpression.Empty, FilterExpressionType.Not);
			Assert.Throws<FilterException>(() => filter.ToString());
		}

		[Fact]
		public static void ToString_FunctionWithMultipleArgs() {
			var func = FilterExpression.Function(FilterExpression.Variable("x"), "Substring", new FilterExpression[] { FilterExpression.Constant(1), FilterExpression.Constant(3) });
			var str = func.ToString();
			Assert.Equal("x.Substring(1, 3)", str);
		}

		[Fact]
		public static void ToString_FunctionWithNoArgs() {
			var func = FilterExpression.Function(FilterExpression.Variable("x"), "Any");
			var str = func.ToString();
			Assert.Equal("x.Any()", str);
		}

		[Fact]
		public static void ToString_ConstantNull() {
			var filter = FilterExpression.Constant(null);
			Assert.Equal("null", filter.ToString());
		}

		[Fact]
		public static void ToString_Variable() {
			Assert.Equal("x", FilterExpression.Variable("x").ToString());
			Assert.Equal("x.y", FilterExpression.Variable("x.y").ToString());
		}

		[Fact]
		public static void ToString_LogicalBinaryWithRightEmpty() {
			var filter = FilterExpression.Binary(
				FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), FilterExpressionType.Equal),
				FilterExpression.Empty,
				FilterExpressionType.Or);
			Assert.Equal("x == 1", filter.ToString());
		}

		[Fact]
		public static void ToString_LogicalBinaryWithLeftEmpty() {
			var filter = FilterExpression.Binary(
				FilterExpression.Empty,
				FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(1), FilterExpressionType.Equal),
				FilterExpressionType.And);
			Assert.Equal("x == 1", filter.ToString());
		}

		[Fact]
		public static void ToString_NestedUnary() {
			var filter = FilterExpression.Not(FilterExpression.Not(FilterExpression.Variable("x")));
			Assert.Equal("!(!(x))", filter.ToString());
		}

		[Fact]
		public static void ToString_BinaryWithStringConstant() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("name"), FilterExpression.Constant("hello"), FilterExpressionType.Equal);
			Assert.Equal("name == \"hello\"", filter.ToString());
		}

		[Fact]
		public static void ToString_BinaryWithNullConstant() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("x"), FilterExpression.Constant(null), FilterExpressionType.Equal);
			Assert.Equal("x == null", filter.ToString());
		}

		[Fact]
		public static void ToString_BinaryWithBoolConstant() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("active"), FilterExpression.Constant(true), FilterExpressionType.Equal);
			Assert.Equal("active == true", filter.ToString());
		}

		[Fact]
		public static void ToString_ComplexNested() {
			// (x > 1) && (y < 2)
			var filter = FilterExpression.And(
				FilterExpression.GreaterThan(FilterExpression.Variable("x"), FilterExpression.Constant(1)),
				FilterExpression.LessThan(FilterExpression.Variable("y"), FilterExpression.Constant(2)));
			Assert.Equal("(x > 1) && (y < 2)", filter.ToString());
		}
	}
}

