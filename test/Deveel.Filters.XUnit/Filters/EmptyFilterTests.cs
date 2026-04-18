namespace Deveel.Filters {
	public class EmptyFilterTests {
		[Fact]
		public static void BuildEmptyFilter() {
			var filter = FilterExpression.Empty;
			Assert.NotNull(filter);
			Assert.True(filter.IsEmpty);
			Assert.Equal(FilterExpression.Empty, filter);
		}

		[Fact]
		public static void CompareEmptyToNotEmptyFilter() {
			var emptyFilter = FilterExpression.Empty;
			var binary = FilterExpression.Binary(FilterExpression.Variable("x.l"), FilterExpression.Constant(22), FilterExpressionType.LessThanOrEqual);

			Assert.NotEqual(emptyFilter, binary);
			Assert.NotEqual(binary, emptyFilter);
			Assert.False(emptyFilter.Equals(binary));
			Assert.False(binary.Equals(emptyFilter));
			Assert.False(binary.IsEmpty);
		}

		[Fact]
		public static void CreateBinaryWithTwoEmpty() {
			var binary = FilterExpression.Binary(FilterExpression.Empty, FilterExpression.Empty, FilterExpressionType.LessThanOrEqual);

			Assert.NotNull(binary);
			Assert.True(binary.Left.IsEmpty);
			Assert.Equal(FilterExpression.Empty, binary.Left);

			Assert.True(binary.Right.IsEmpty);
			Assert.Equal(FilterExpression.Empty, binary.Right);
		}
	}
}
