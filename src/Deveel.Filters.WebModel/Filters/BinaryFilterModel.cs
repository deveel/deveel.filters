using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deveel.Filters {
	/// <summary>
	/// A model that represents a binary filter
	/// </summary>
	public sealed class BinaryFilterModel : IValidatableObject {
		/// <summary>
		/// The left side of the binary filter
		/// </summary>
		[JsonPropertyName("left")]
		public FilterModel? Left { get; set; }

		/// <summary>
		/// The right side of the binary filter
		/// </summary>
		[JsonPropertyName("right")]
		public FilterModel? Right { get; set; }

		/// <summary>
		/// Extension data for the dynamic properties of the filter.
		/// </summary>
		[JsonExtensionData, SimpleValue]
		public IDictionary<string, JsonElement>? BinaryData { get; set; }

		internal BinaryFilter BuildFilter(FilterType filterType) {
			if (BinaryData != null)
				return JsonElementUtil.BuildFilter(BinaryData, filterType);

			if (Left == null || Right == null)
				throw new FilterException("The left and right side of the binary filter must be specified");

			var left = Left.BuildFilter();
			var right = Right.BuildFilter();

			return Filter.Binary(left, right, filterType);
		}

		IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext) {
			if (BinaryData == null && Left == null && Right == null)
				yield return new ValidationResult("The left and right side of the binary filter must be specified", new[] {nameof(Left), nameof(Right)});

			if (BinaryData != null && (Left != null || Right != null))
				yield return new ValidationResult("The left and right side of the binary filter must not be present if the data are set", new[] {nameof(Left), nameof(Right)});

			if (BinaryData != null && BinaryData.Count == 0)
				yield return new ValidationResult("The binary data must not be empty", new[] {nameof(BinaryData)});

			if (Left != null && Right != null && BinaryData != null)
				yield return new ValidationResult("The left and right side of the binary filter must not be present if the data are set", new[] {nameof(Left), nameof(Right)});

			if (BinaryData == null && (Left == null || Right == null))
				yield return new ValidationResult("The left and right side of the binary filter must be specified", new[] {nameof(Left), nameof(Right)});
		}
	}
}
