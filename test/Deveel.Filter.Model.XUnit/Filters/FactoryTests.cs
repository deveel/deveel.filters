namespace Deveel.Filters {
	public static class FactoryTests {
		[Theory]
		[InlineData("foo", 42, FilterType.Equals)]
		[InlineData("foo", 42, FilterType.NotEquals)]
		[InlineData("foo", 42, FilterType.GreaterThan)]
		[InlineData("foo", 42, FilterType.GreaterThanOrEqual)]
		[InlineData("foo", 42, FilterType.LessThan)]
		[InlineData("foo", 42, FilterType.LessThanOrEqual)]
		public static void CreateBinaryFilter(string varName, object value, FilterType filterType) { 
			var filter = Filter.Binary(Filter.Variable(varName), Filter.Constant(value), filterType);

			Assert.NotNull(filter);
			Assert.Equal(filterType, filter.FilterType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateEquals(string varName, object value) {
			var filter = Filter.Equals(Filter.Variable(varName), Filter.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterType.Equals, filter.FilterType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateNotEquals(string varName, object value) {
			var filter = Filter.NotEquals(Filter.Variable(varName), Filter.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterType.NotEquals, filter.FilterType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateGreaterThan(string varName, object value) {
			var filter = Filter.GreaterThan(Filter.Variable(varName), Filter.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterType.GreaterThan, filter.FilterType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateGreaterThanOrEqual(string varName, object value) {
			var filter = Filter.GreaterThanOrEqual(Filter.Variable(varName), Filter.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterType.GreaterThanOrEqual, filter.FilterType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateLessThan(string varName, object value) {
			var filter = Filter.LessThan(Filter.Variable(varName), Filter.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterType.LessThan, filter.FilterType);
		}

		[Theory]
		[InlineData("foo", 42)]
		[InlineData("foo", "bar")]
		[InlineData("foo", true)]
		public static void CreateLessThanOrEqual(string varName, object value) {
			var filter = Filter.LessThanOrEqual(Filter.Variable(varName), Filter.Constant(value));
			Assert.NotNull(filter);
			Assert.Equal(FilterType.LessThanOrEqual, filter.FilterType);
		}
	}
}
