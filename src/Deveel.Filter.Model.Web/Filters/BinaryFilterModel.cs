using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Deveel.Filters {
	public sealed class BinaryFilterModel {
		[JsonPropertyName("left"), Required]
		public FilterModel Left { get; set; }

		[JsonPropertyName("right"), Required]
		public FilterModel Right { get; set; }

		internal BinaryFilter BuildFilter(FilterType filterType) {
			var left = Left.BuildFilter();
			var right = Right.BuildFilter();

			return Filter.Binary(left, right, filterType);
		}
	}
}
