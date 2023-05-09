using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deveel.Filters {
    /// <summary>
    /// A model that describes a filter that is used to
    /// restrict a set of data.
    /// </summary>
    /// <remarks>
    /// This object defines a model that can be exchanged between
    /// services and clients and be serialized and deserialized safely.
    /// </remarks>
    public class FilterModel : IFilter, IValidatableObject {
		private BinaryFilterModel? binaryFilter;
		private FilterType? filterType;
		private FilterModel? not;
		private object? value;
		private string? variableName;
		private FunctionFilterModel? functionFilter;
		private IDictionary<string, JsonElement>? binaryData;

		FilterType IFilter.FilterType => filterType ?? new FilterType();

		private void Reset() {
			binaryFilter = null;
			filterType = null;
			not = null;
			value = null;
			variableName = null;
			functionFilter = null;
			binaryData = null;
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
		public BinaryFilterModel? Equal {
			get => GetBinaryIf(FilterType.Equal);
			set => SetBinary(FilterType.Equal, value);
		}

		[JsonPropertyName("neq")]
		public BinaryFilterModel? NotEqual {
			get => GetBinaryIf(FilterType.NotEqual);
			set => SetBinary(FilterType.NotEqual, value);
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

		[JsonExtensionData, SimpleValue]
		public IDictionary<string, JsonElement>? BinaryData {
			get => binaryData;
			set {
				Reset();
				filterType = FilterType.Equal;
				binaryData = value;
			}
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

		[JsonPropertyName("func")]
		public FunctionFilterModel? Function {
			get => functionFilter;
			set {
				Reset();
				filterType = FilterType.Function;
				functionFilter = value;
			}
		}

		public virtual IFilter BuildFilter() {
			if (filterType == null)
				throw new FilterException("The model is invalid - no type was set");

			switch (filterType) {
				case FilterType.Constant:
					return new ConstantFilter(value);
				case FilterType.Not:
					if (not == null)
						throw new FilterException("The model is invalid - no unary filter was set");

					return Filter.Not(not.BuildFilter());
				case FilterType.Equal: {
                        if (binaryData != null)
                            return JsonElementUtil.BuildFilter(binaryData, FilterType.Equal);
                        if (binaryFilter != null)
                            return binaryFilter.BuildFilter(FilterType.Equal);

                    throw new FilterException("The model is invalid - no binary filter was set");
                    }
                case FilterType.NotEqual:
				case FilterType.GreaterThan:
				case FilterType.GreaterThanOrEqual:
				case FilterType.LessThan:
				case FilterType.LessThanOrEqual:
				case FilterType.And:
				case FilterType.Or:
                    if (binaryFilter != null)
                        return binaryFilter.BuildFilter(filterType.Value);

                    throw new FilterException("The model is invalid - no binary filter was set");
				case FilterType.Variable:
					if (variableName == null)
						throw new FilterException("The model is invalid - no variable was set");

					return Filter.Variable(variableName);
				case FilterType.Function:
					if (functionFilter == null)
						throw new FilterException("The model is invalid - no function was set");

					return functionFilter.BuildFilter();
			}

			throw new FilterException("Not a valid filter model");
		}

		private IFilter BuildBinaryFilterFromJson(IDictionary<string, JsonElement>? jsonData, FilterType filterType) {
			if (jsonData == null)
				return Filter.Empty;

			Filter? filter = null;

			foreach (var item in jsonData) {
                var value = JsonElementUtil.InferValue(item.Value);
                var equalFilter = Filter.Binary(Filter.Variable(item.Key), Filter.Constant(value), filterType);

				if (filter == null) {
					filter = equalFilter;
				} else {
					filter = Filter.And(filter, equalFilter);
				}
			}

			if (filter == null)
				return Filter.Empty;

			return filter;
        }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext) {
			if (filterType == null)
				yield return new ValidationResult("The filter type is not specified");

			if (filterType == FilterType.Not && not == null)
				yield return new ValidationResult("The NOT filter is not specified", new[] {nameof(Not)});

			if (filterType == FilterType.Variable && String.IsNullOrWhiteSpace(variableName))
				yield return new ValidationResult("The variable name is not specified", new[] {nameof(Ref)});

			if (filterType == FilterType.Equal && binaryData != null) {
				if (binaryData.Count > 1)
                    yield return new ValidationResult("The equals filter can only have one value", new[] {nameof(BinaryData)});

				var simpleValue = new SimpleValueAttribute();
				if (!simpleValue.IsValid(binaryData.Values.First()))
                    yield return new ValidationResult("The value is not valid", new[] {nameof(BinaryData)});
			}

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