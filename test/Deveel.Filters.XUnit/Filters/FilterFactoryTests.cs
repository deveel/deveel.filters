using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public static class FilterFactoryTests {
		[Fact]
		public static void CreateVariable() {
			var filter = Filter.Variable("x");
			Assert.NotNull(filter);
			Assert.Equal(FilterType.Variable, filter.FilterType);
			Assert.Equal("x", filter.VariableName);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\t")]
		[InlineData("\n")]
		[InlineData("\r")]
		[InlineData("x-2")]
		[InlineData("x 2")]
		public static void CreateVariableWithBadName(string? varName) {
			Assert.Throws<ArgumentException>(() => Filter.Variable(varName));
		}

		[Fact]
		public static void CreateConstant() {
			var filter = Filter.Constant(123);
			Assert.NotNull(filter);
			Assert.Equal(FilterType.Constant, filter.FilterType);
			Assert.Equal(123, filter.Value);
		}
	}
}
