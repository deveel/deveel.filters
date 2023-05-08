using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Deveel.Filters {
	/// <summary>
	/// A model that represents a binary filter
	/// </summary>
	public sealed class BinaryFilterModel {
		/// <summary>
		/// The left side of the binary filter
		/// </summary>
		[JsonPropertyName("left"), Required]
		public FilterModel Left { get; set; }

		/// <summary>
		/// The right side of the binary filter
		/// </summary>
		[JsonPropertyName("right"), Required]
		public FilterModel Right { get; set; }

		internal BinaryFilter BuildFilter(FilterType filterType) {
			var left = Left.BuildFilter();
			var right = Right.BuildFilter();

			return Filter.Binary(left, right, filterType);
		}
	}
}
