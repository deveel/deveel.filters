namespace Deveel.Filters {
	public static class FilterBuildTests {
		[Fact]
		public static void BuildSimpleBinaryFilter() {
			var filter = new FilterModel {
				Equals = new BinaryFilterModel {
					Left = new FilterModel {
						Ref = "foo"
					},
					Right = new FilterModel {
						Value = 42
					}
				}
			};

			var result = filter.BuildFilter();
			Assert.NotNull(result);
			var binary = Assert.IsType<BinaryFilter>(result);
			Assert.Equal(FilterType.Equals, result.FilterType);

			var left = Assert.IsType<VariableFilter>(binary.Left);
			Assert.Equal("foo", left.VariableName);

			var right = Assert.IsType<ConstantFilter>(binary.Right);
			Assert.Equal(42, right.Value);
		}
	}
}
