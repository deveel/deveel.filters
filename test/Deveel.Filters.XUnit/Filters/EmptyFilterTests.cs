using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public class EmptyFilterTests {
		[Fact]
		public static void BuildEmptyFilter() {
			var filter = Filter.Empty;
			Assert.NotNull(filter);
			Assert.True(filter.IsEmpty());
			Assert.Equal(Filter.Empty, filter);
		}

		[Fact]
		public static void CompareEmptyToNotEmptyFilter() {
			var emptyFilter = Filter.Empty;
			var binary = Filter.Binary(Filter.Variable("x.l"), Filter.Constant(22), FilterType.LessThanOrEqual);

			Assert.NotEqual(emptyFilter, binary);
			Assert.NotEqual(binary, emptyFilter);
			Assert.False(emptyFilter.Equals(binary));
			Assert.False(binary.Equals(emptyFilter));
			Assert.False(binary.IsEmpty());
		}

		[Fact]
		public static void CreateBinaryWithTwoEmpty() {
			var binary = Filter.Binary(Filter.Empty, Filter.Empty, FilterType.LessThanOrEqual);

			Assert.NotNull(binary);
			Assert.True(binary.Left.IsEmpty());
			Assert.Equal(Filter.Empty, binary.Left);

			Assert.True(binary.Right.IsEmpty());
			Assert.Equal(Filter.Empty, binary.Right);
		}
	}
}
