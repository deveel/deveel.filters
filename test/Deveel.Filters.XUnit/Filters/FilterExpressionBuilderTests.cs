namespace Deveel.Filters {
	public static class FilterExpressionBuilderTests {
		[Fact]
		public static void BuildEmpty_ReturnsEmptyExpression() {
			var result = new FilterExpressionBuilder().Build();

			Assert.True(result.IsEmpty);
		}

		[Theory]
		[InlineData("name", "John")]
		[InlineData("age", 30)]
		[InlineData("active", true)]
		public static void Where_IsEqualTo(string field, object value) {
			var result = new FilterExpressionBuilder()
				.Where(field).IsEqualTo(value)
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Equal, binary.ExpressionType);
			Assert.IsType<VariableFilterExpression>(binary.Left);
			Assert.IsType<ConstantFilterExpression>(binary.Right);
		}

		[Fact]
		public static void Where_IsNotEqualTo() {
			var result = new FilterExpressionBuilder()
				.Where("status").IsNotEqualTo("inactive")
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.NotEqual, binary.ExpressionType);
		}

		[Fact]
		public static void Where_IsGreaterThan() {
			var result = new FilterExpressionBuilder()
				.Where("age").IsGreaterThan(18)
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.GreaterThan, binary.ExpressionType);
		}

		[Fact]
		public static void Where_IsGreaterThanOrEqualTo() {
			var result = new FilterExpressionBuilder()
				.Where("age").IsGreaterThanOrEqualTo(18)
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.GreaterThanOrEqual, binary.ExpressionType);
		}

		[Fact]
		public static void Where_IsLessThan() {
			var result = new FilterExpressionBuilder()
				.Where("age").IsLessThan(65)
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.LessThan, binary.ExpressionType);
		}

		[Fact]
		public static void Where_IsLessThanOrEqualTo() {
			var result = new FilterExpressionBuilder()
				.Where("age").IsLessThanOrEqualTo(65)
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.LessThanOrEqual, binary.ExpressionType);
		}

		[Fact]
		public static void Where_IsNull() {
			var result = new FilterExpressionBuilder()
				.Where("email").IsNull()
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Equal, binary.ExpressionType);
			var constant = Assert.IsType<ConstantFilterExpression>(binary.Right);
			Assert.Null(constant.Value);
		}

		[Fact]
		public static void Where_IsNotNull() {
			var result = new FilterExpressionBuilder()
				.Where("email").IsNotNull()
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.NotEqual, binary.ExpressionType);
			var constant = Assert.IsType<ConstantFilterExpression>(binary.Right);
			Assert.Null(constant.Value);
		}

		[Fact]
		public static void Where_And_CombinesWithAnd() {
			var result = new FilterExpressionBuilder()
				.Where("age").IsGreaterThan(18)
				.And("name").IsEqualTo("John")
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.And, binary.ExpressionType);
			Assert.IsType<BinaryFilterExpression>(binary.Left);
			Assert.IsType<BinaryFilterExpression>(binary.Right);
		}

		[Fact]
		public static void Where_Or_CombinesWithOr() {
			var result = new FilterExpressionBuilder()
				.Where("status").IsEqualTo("active")
				.Or("status").IsEqualTo("pending")
				.Build();

			var binary = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Or, binary.ExpressionType);
		}

		[Fact]
		public static void Not_NegatesExpression() {
			var result = new FilterExpressionBuilder()
				.Where("active").IsEqualTo(true)
				.Not()
				.Build();

			var unary = Assert.IsType<UnaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Not, unary.ExpressionType);
			Assert.IsType<BinaryFilterExpression>(unary.Operand);
		}

		[Fact]
		public static void And_WithoutPreviousExpression_Throws() {
			var builder = new FilterExpressionBuilder();

			Assert.Throws<InvalidOperationException>(() => builder.And("foo"));
		}

		[Fact]
		public static void Or_WithoutPreviousExpression_Throws() {
			var builder = new FilterExpressionBuilder();

			Assert.Throws<InvalidOperationException>(() => builder.Or("foo"));
		}

		[Fact]
		public static void Not_WithoutPreviousExpression_Throws() {
			var builder = new FilterExpressionBuilder();

			Assert.Throws<InvalidOperationException>(() => builder.Not());
		}

		[Fact]
		public static void HasFunction_CreatesFunction() {
			var result = new FilterExpressionBuilder()
				.Where("name").HasFunction("contains", "oh")
				.Build();

			var func = Assert.IsType<FunctionFilterExpression>(result);
			Assert.Equal("contains", func.FunctionName);
			Assert.NotNull(func.Arguments);
			Assert.Single(func.Arguments);
		}

		[Fact]
		public static void HasFunction_WithNoArgs() {
			var result = new FilterExpressionBuilder()
				.Where("list").HasFunction("any")
				.Build();

			var func = Assert.IsType<FunctionFilterExpression>(result);
			Assert.Equal("any", func.FunctionName);
			Assert.NotNull(func.Arguments);
			Assert.Empty(func.Arguments);
		}

		[Fact]
		public static void ComplexChain_MultipleAndOr() {
			var result = new FilterExpressionBuilder()
				.Where("age").IsGreaterThan(18)
				.And("age").IsLessThan(65)
				.Or("vip").IsEqualTo(true)
				.Build();

			// ((age > 18 AND age < 65) OR vip == true)
			var root = Assert.IsType<BinaryFilterExpression>(result);
			Assert.Equal(FilterExpressionType.Or, root.ExpressionType);

			var left = Assert.IsType<BinaryFilterExpression>(root.Left);
			Assert.Equal(FilterExpressionType.And, left.ExpressionType);
		}

		[Fact]
		public static void Where_InvalidVariable_Throws() {
			var builder = new FilterExpressionBuilder();

			Assert.Throws<ArgumentException>(() => builder.Where("invalid name!"));
		}
	}
}

