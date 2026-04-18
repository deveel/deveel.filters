// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
    public class FilterModel : IValidatableObject {
		private BinaryFilterModel? binaryFilter;
		private FilterExpressionType? filterType;
		private FilterModel? not;
		private object? value;
		private string? variableName;
		private FunctionFilterModel? functionFilter;
		private IDictionary<string, JsonElement>? binaryData;

		internal FilterExpressionType? GetFilterType() => filterType;
		
		private void Reset() {
			binaryFilter = null;
			filterType = null;
			not = null;
			value = null;
			variableName = null;
			functionFilter = null;
			binaryData = null;
		}

		private BinaryFilterModel? GetBinaryIf(FilterExpressionType expressionType) {
			return filterType == expressionType ? binaryFilter : null;
		}

		private void SetBinary(FilterExpressionType expressionType, BinaryFilterModel? binary) {
			Reset();
			filterType = expressionType;
			binaryFilter = binary;
		}

		/// <summary>
		/// Gets or sets the equality binary filter model (JSON: "eq").
		/// </summary>
		[JsonPropertyName("eq")]
		public BinaryFilterModel? Equal {
			get => GetBinaryIf(FilterExpressionType.Equal);
			set => SetBinary(FilterExpressionType.Equal, value);
		}

		/// <summary>
		/// Gets or sets the inequality binary filter model (JSON: "neq").
		/// </summary>
		[JsonPropertyName("neq")]
		public BinaryFilterModel? NotEqual {
			get => GetBinaryIf(FilterExpressionType.NotEqual);
			set => SetBinary(FilterExpressionType.NotEqual, value);
		}

		/// <summary>
		/// Gets or sets the greater-than binary filter model (JSON: "gt").
		/// </summary>
		[JsonPropertyName("gt")]
		public BinaryFilterModel? GreaterThan {
			get => GetBinaryIf(FilterExpressionType.GreaterThan);
			set => SetBinary(FilterExpressionType.GreaterThan, value);
		}

		/// <summary>
		/// Gets or sets the greater-than-or-equal binary filter model (JSON: "gte").
		/// </summary>
		[JsonPropertyName("gte")]
		public BinaryFilterModel? GreaterThanOrEqual {
			get => GetBinaryIf(FilterExpressionType.GreaterThanOrEqual);
			set => SetBinary(FilterExpressionType.GreaterThanOrEqual, value);
		}

		/// <summary>
		/// Gets or sets the less-than binary filter model (JSON: "lt").
		/// </summary>
		[JsonPropertyName("lt")]
		public BinaryFilterModel? LessThan {
			get => GetBinaryIf(FilterExpressionType.LessThan);
			set => SetBinary(FilterExpressionType.LessThan, value);
		}

		/// <summary>
		/// Gets or sets the less-than-or-equal binary filter model (JSON: "lte").
		/// </summary>
		[JsonPropertyName("lte")]
		public BinaryFilterModel? LessThanOrEqual {
			get => GetBinaryIf(FilterExpressionType.LessThanOrEqual);
			set => SetBinary(FilterExpressionType.LessThanOrEqual, value);
		}

		/// <summary>
		/// Gets or sets the logical AND binary filter model (JSON: "and").
		/// </summary>
		[JsonPropertyName("and")]
		public BinaryFilterModel? And {
			set => SetBinary(FilterExpressionType.And, value);
			get => GetBinaryIf(FilterExpressionType.And);
		}

		/// <summary>
		/// Gets or sets the logical OR binary filter model (JSON: "or").
		/// </summary>
		[JsonPropertyName("or")]
		public BinaryFilterModel? Or { 
			set => SetBinary(FilterExpressionType.Or, value);
			get => GetBinaryIf(FilterExpressionType.Or);
		}

		/// <summary>
		/// Gets or sets the extension data containing simple key-value pairs
		/// that represent an equality filter in a compact form.
		/// </summary>
		[JsonExtensionData, SimpleValue]
		public IDictionary<string, JsonElement>? BinaryData {
			get => binaryData;
			set {
				Reset();
				filterType = FilterExpressionType.Equal;
				binaryData = value;
			}
		}

		/// <summary>
		/// Gets or sets the negation (NOT) filter model (JSON: "not").
		/// </summary>
		[JsonPropertyName("not")]
		public FilterModel? Not {
			get => not;
			set {
				Reset();
				filterType = FilterExpressionType.Not;
				not = value;
			}
		}

		/// <summary>
		/// Gets or sets a constant value for this filter (JSON: "value").
		/// </summary>
		[JsonPropertyName("value")]
		[SimpleValue]
		public object? Value {
			get => value;
			set {
				Reset();
				filterType = FilterExpressionType.Constant;
				this.value = value;
			}
		}

		/// <summary>
		/// Gets or sets the variable reference name for this filter (JSON: "ref").
		/// </summary>
		[JsonPropertyName("ref")]
		public string? Ref {
			get => variableName;
			set {
				Reset();
				filterType = FilterExpressionType.Variable;
				variableName = value;
			}
		}

		/// <summary>
		/// Gets or sets the function filter model for this filter (JSON: "func").
		/// </summary>
		[JsonPropertyName("func")]
		public FunctionFilterModel? Function {
			get => functionFilter;
			set {
				Reset();
				filterType = FilterExpressionType.Function;
				functionFilter = value;
			}
		}

		/// <summary>
		/// Builds a <see cref="FilterExpression"/> from this model.
		/// </summary>
		/// <returns>The constructed <see cref="FilterExpression"/>.</returns>
		/// <exception cref="FilterException">
		/// Thrown when the model is invalid or incomplete.
		/// </exception>
		public virtual FilterExpression BuildFilter() {
			if (filterType == null)
				throw new FilterException("The model is invalid - no type was set");

			switch (filterType) {
				case FilterExpressionType.Constant: {
						if (value is JsonElement json)
							value = JsonElementUtil.InferValue(json);

						return new ConstantFilterExpression(value);
					}
				case FilterExpressionType.Not:
					if (not == null)
						throw new FilterException("The model is invalid - no unary filter was set");

					return FilterExpression.Not(not.BuildFilter());
				case FilterExpressionType.Equal: {
                        if (binaryData != null)
                            return JsonElementUtil.BuildFilter(binaryData, FilterExpressionType.Equal);
                        if (binaryFilter != null)
                            return binaryFilter.BuildFilter(FilterExpressionType.Equal);

                    throw new FilterException("The model is invalid - no binary filter was set");
                    }
                case FilterExpressionType.NotEqual:
				case FilterExpressionType.GreaterThan:
				case FilterExpressionType.GreaterThanOrEqual:
				case FilterExpressionType.LessThan:
				case FilterExpressionType.LessThanOrEqual:
				case FilterExpressionType.And:
				case FilterExpressionType.Or:
                    if (binaryFilter != null)
                        return binaryFilter.BuildFilter(filterType.Value);

                    throw new FilterException("The model is invalid - no binary filter was set");
				case FilterExpressionType.Variable:
					if (variableName == null)
						throw new FilterException("The model is invalid - no variable was set");

					return FilterExpression.Variable(variableName);
				case FilterExpressionType.Function:
					if (functionFilter == null)
						throw new FilterException("The model is invalid - no function was set");

					return functionFilter.BuildFilter();
			}

			throw new FilterException("Not a valid filter model");
		}

		private FilterExpression BuildBinaryFilterFromJson(IDictionary<string, JsonElement>? jsonData, FilterExpressionType expressionType) {
			if (jsonData == null)
				return FilterExpression.Empty;

			FilterExpression? filter = null;

			foreach (var item in jsonData) {
                var value = JsonElementUtil.InferValue(item.Value);
                var equalFilter = FilterExpression.Binary(FilterExpression.Variable(item.Key), FilterExpression.Constant(value), expressionType);

				if (filter == null) {
					filter = equalFilter;
				} else {
					filter = FilterExpression.And(filter, equalFilter);
				}
			}

			if (filter == null)
				return FilterExpression.Empty;

			return filter;
        }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext) {
			if (filterType == null)
				yield return new ValidationResult("The filter type is not specified");

			if (filterType == FilterExpressionType.Not && not == null)
				yield return new ValidationResult("The NOT filter is not specified", new[] {nameof(Not)});

			if (filterType == FilterExpressionType.Variable && String.IsNullOrWhiteSpace(variableName))
				yield return new ValidationResult("The variable name is not specified", new[] {nameof(Ref)});

			if (filterType == FilterExpressionType.Equal && binaryData != null) {
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

		/// <summary>
		/// Provides a hook for subclasses to add additional validation logic.
		/// </summary>
		/// <param name="validationContext">The validation context.</param>
		/// <returns>A collection of validation results, empty if valid.</returns>
		protected virtual IEnumerable<ValidationResult> ValidateModel(ValidationContext validationContext) {
			return Enumerable.Empty<ValidationResult>();
		}
	}
}