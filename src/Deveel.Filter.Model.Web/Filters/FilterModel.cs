using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Deveel.Filters {
	public class FilterModel : IValidatableObject {
		private BinaryFilterModel? binaryFilter;
		private FilterType? filterType;
		private FilterModel? not;
		private object? value;
		private string? variableName;

		private void Reset() {
			binaryFilter = null;
			filterType = null;
			not = null;
			value = null;
		}

		private BinaryFilterModel? GetBinaryIf(FilterType type) {
			return filterType == type ? binaryFilter : null;
		}

		private void SetBinary(FilterType type, BinaryFilterModel? binary) {
			Reset();
			filterType = type;
			binaryFilter = binary;
		}

		[JsonPropertyName("eq")]
		public BinaryFilterModel? Equals {
			get => GetBinaryIf(FilterType.Equals);
			set => SetBinary(FilterType.Equals, value);
		}

		[JsonPropertyName("neq")]
		public BinaryFilterModel? NotEquals {
			get => GetBinaryIf(FilterType.NotEquals);
			set => SetBinary(FilterType.NotEquals, value);
		}

		[JsonPropertyName("gt")]
		public BinaryFilterModel? GreaterThan {
			get => GetBinaryIf(FilterType.GreaterThan);
			set => SetBinary(FilterType.GreaterThan, value);
		}

		[JsonPropertyName("gte")]
		public BinaryFilterModel? GreaterThanOrEqual {
			get => GetBinaryIf(FilterType.GreaterThanOrEqual);
			set => SetBinary(FilterType.GreaterThanOrEqual, value);
		}

		[JsonPropertyName("lt")]
		public BinaryFilterModel? LessThan {
			get => GetBinaryIf(FilterType.LessThan);
			set => SetBinary(FilterType.LessThan, value);
		}

		[JsonPropertyName("lte")]
		public BinaryFilterModel? LessThanOrEqual {
			get => GetBinaryIf(FilterType.LessThanOrEqual);
			set => SetBinary(FilterType.LessThanOrEqual, value);
		}

		[JsonPropertyName("and")]
		public BinaryFilterModel? And {
			set => SetBinary(FilterType.And, value);
			get => GetBinaryIf(FilterType.And);
		}

		[JsonPropertyName("or")]
		public BinaryFilterModel? Or { 
			set => SetBinary(FilterType.Or, value);
			get => GetBinaryIf(FilterType.Or);
		}

		[JsonPropertyName("not")]
		public FilterModel? Not {
			get => not;
			set {
				Reset();
				filterType = FilterType.Not;
				not = value;
			}
		}

		[JsonPropertyName("value")]
		[SimpleValue]
		public object? Value {
			get => value;
			set {
				Reset();
				filterType = FilterType.Constant;
				this.value = value;
			}
		}

		[JsonPropertyName("ref")]
		public string? Ref {
			get => variableName;
			set {
				Reset();
				filterType = FilterType.Variable;
				variableName = value;
			}
		}

		public virtual Filter BuildFilter() {
			if (filterType == null)
				throw new FilterException("The model is invalid");

			switch (filterType) {
				case FilterType.Constant:
					return new ConstantFilter(value);
				case FilterType.Not:
					if (not == null)
						throw new FilterException("The model is invalid");

					return Filter.Not(not.BuildFilter());
				case FilterType.Equals:
				case FilterType.NotEquals:
				case FilterType.GreaterThan:
				case FilterType.GreaterThanOrEqual:
				case FilterType.LessThan:
				case FilterType.LessThanOrEqual:
				case FilterType.And:
				case FilterType.Or:
					if (binaryFilter == null)
						throw new FilterException("The model is invalid");

					return binaryFilter.BuildFilter(filterType.Value);
				case FilterType.Variable:
					if (variableName == null)
						throw new FilterException("The model is invalid");

					return Filter.Variable(variableName);
			}

			throw new FilterException("Not a valid filter model");
		}

		IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext) {
			if (filterType == null)
				yield return new ValidationResult("The filter type is not specified");

			if (filterType == FilterType.Not && not == null)
				yield return new ValidationResult("The NOT filter is not specified", new[] {nameof(Not)});

			if (filterType == FilterType.Variable && String.IsNullOrWhiteSpace(variableName))
				yield return new ValidationResult("The variable name is not specified", new[] {nameof(Ref)});

			if (binaryFilter != null) {
				var binaryValidationResults = new List<ValidationResult>();
				var filterContext = new ValidationContext(binaryFilter);
				if (!Validator.TryValidateObject(binaryFilter, filterContext, binaryValidationResults, true)) {
					foreach (var result in binaryValidationResults)
						yield return result;
				}
			}

			var results = ValidateModel(validationContext);

			foreach (var result in results)
				yield return result;
		}

		protected virtual IEnumerable<ValidationResult> ValidateModel(ValidationContext validationContext) {
			return Enumerable.Empty<ValidationResult>();
		}
	}
}