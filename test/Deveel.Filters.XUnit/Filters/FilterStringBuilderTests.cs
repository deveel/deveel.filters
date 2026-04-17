namespace Deveel.Filters {
	public static class FilterStringBuilderTests {
		[Fact]
		public static void ToString_UnaryOfEmptyOperand_Throws() {
			var filter = Filter.Unary(Filter.Empty, FilterType.Not);
			Assert.Throws<FilterException>(() => filter.ToString());
		}

		[Fact]
		public static void ToString_FunctionWithMultipleArgs() {
			var func = Filter.Function(Filter.Variable("x"), "Substring", new Filter[] { Filter.Constant(1), Filter.Constant(3) });
			var str = func.ToString();
			Assert.Equal("x.Substring(1, 3)", str);
		}

		[Fact]
		public static void ToString_FunctionWithNoArgs() {
			var func = Filter.Function(Filter.Variable("x"), "Any");
			var str = func.ToString();
			Assert.Equal("x.Any()", str);
		}

		[Fact]
		public static void ToString_ConstantNull() {
			var filter = Filter.Constant(null);
			Assert.Equal("null", filter.ToString());
		}

		[Fact]
		public static void ToString_Variable() {
			Assert.Equal("x", Filter.Variable("x").ToString());
			Assert.Equal("x.y", Filter.Variable("x.y").ToString());
		}

		[Fact]
		public static void ToString_LogicalBinaryWithRightEmpty() {
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("x"), Filter.Constant(1), FilterType.Equal),
				Filter.Empty,
				FilterType.Or);
			Assert.Equal("x == 1", filter.ToString());
		}

		[Fact]
		public static void ToString_LogicalBinaryWithLeftEmpty() {
			var filter = Filter.Binary(
				Filter.Empty,
				Filter.Binary(Filter.Variable("x"), Filter.Constant(1), FilterType.Equal),
				FilterType.And);
			Assert.Equal("x == 1", filter.ToString());
		}

		[Fact]
		public static void ToString_NestedUnary() {
			var filter = Filter.Not(Filter.Not(Filter.Variable("x")));
			Assert.Equal("!(!(x))", filter.ToString());
		}

		[Fact]
		public static void ToString_BinaryWithStringConstant() {
			var filter = Filter.Binary(Filter.Variable("name"), Filter.Constant("hello"), FilterType.Equal);
			Assert.Equal("name == \"hello\"", filter.ToString());
		}

		[Fact]
		public static void ToString_BinaryWithNullConstant() {
			var filter = Filter.Binary(Filter.Variable("x"), Filter.Constant(null), FilterType.Equal);
			Assert.Equal("x == null", filter.ToString());
		}

		[Fact]
		public static void ToString_BinaryWithBoolConstant() {
			var filter = Filter.Binary(Filter.Variable("active"), Filter.Constant(true), FilterType.Equal);
			Assert.Equal("active == true", filter.ToString());
		}

		[Fact]
		public static void ToString_ComplexNested() {
			// (x > 1) && (y < 2)
			var filter = Filter.And(
				Filter.GreaterThan(Filter.Variable("x"), Filter.Constant(1)),
				Filter.LessThan(Filter.Variable("y"), Filter.Constant(2)));
			Assert.Equal("(x > 1) && (y < 2)", filter.ToString());
		}
	}
}

