using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Deveel.Filters {
	/// <summary>
	/// Represents a default implementation of a filter that 
	/// can be used to restrict the result of a query.
	/// </summary>
	[DebuggerDisplay("{ToString(),nq}")]
	public abstract class Filter : IFilter {
		/// <summary>
		/// Gets the type of filter.
		/// </summary>
		public abstract FilterType FilterType { get; }

		/// <summary>
		/// An empty filter that has no effect on the result.
		/// </summary>
		public static readonly Filter Empty = new EmptyFilter();

		/// <inheritdoc/>
		public override string ToString() {
			return this.AsString();
		}


		public static Filter Convert(IFilter filter) {
			var converter = new FilterConverter();
			return (Filter) converter.Visit(filter);
		}

		public static bool IsValidReference(string variableName) {
			return !String.IsNullOrWhiteSpace(variableName) &&
			       variableName.All(c => Char.IsLetterOrDigit(c) || c == '_' || c == '.');
		}

		#region Factories

		/// <summary>
		/// Creates a new unary filter with the given operand and filter type.
		/// </summary>
		/// <param name="operand">
		/// The operand of the unary filter.
		/// </param>
		/// <param name="filterType">
		/// The type of the unary filter.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="UnaryFilter"/> with the given operand and
		/// of the given type.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="filterType"/> is not a unary filter type.
		/// </exception>
		public static UnaryFilter Unary(Filter operand, FilterType filterType) {
			if (filterType != FilterType.Not)
				throw new ArgumentException($"The filter type '{filterType}' is not a unary filter type.", nameof(filterType));

			return new UnaryFilter(operand, filterType);
		}

		public static UnaryFilter Unary(IFilter operand, FilterType filterType)
			=> Unary(Convert(operand), filterType);

		/// <summary>
		/// Creates an unary filter that negates the given operand.
		/// </summary>
		/// <param name="operand">
		/// The filter operand to negate.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="UnaryFilter"/> that negates the given
		/// filter operand.
		/// </returns>

		public static UnaryFilter Not(Filter operand)
			=> Unary(operand, FilterType.Not);

		public static UnaryFilter Not(IFilter operand)
			=> Not(Convert(operand));

		/// <summary>
		/// Creates a new binary filter with the given left and right operands
		/// </summary>
		/// <param name="left">
		/// The left operand of the binary filter.
		/// </param>
		/// <param name="right">
		/// The right operand of the binary filter.
		/// </param>
		/// <param name="filterType">
		/// The type of the binary filter.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilter"/> of the given type with 
		/// the given left and right operands.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="filterType"/> is not a valid binary filter type.
		/// </exception>
		public static BinaryFilter Binary(Filter left, Filter right, FilterType filterType) {
			if (filterType != FilterType.And &&
				filterType != FilterType.Or &&
				filterType != FilterType.Equal &&
				filterType != FilterType.NotEqual &&
				filterType != FilterType.LessThan &&
				filterType != FilterType.LessThanOrEqual &&
				filterType != FilterType.GreaterThan &&
				filterType != FilterType.GreaterThanOrEqual)
				throw new ArgumentException($"The filter type '{filterType}' is not a binary filter type.", nameof(filterType));

			return new BinaryFilter(left, right, filterType);
		}

		public static BinaryFilter Binary(IFilter left, IFilter right, FilterType filterType)
			=> Binary(Convert(left), Convert(right), filterType);

		public static BinaryFilter And(Filter left, Filter right)
			=> Binary(left, right, FilterType.And);

		public static BinaryFilter And(IFilter left, IFilter right)
			=> Binary(Convert(left), Convert(right), FilterType.And);

		public static BinaryFilter Or(Filter left, Filter right)
			=> Binary(left, right, FilterType.Or);

		public static BinaryFilter Or(IFilter left, IFilter right)
			=> Binary(Convert(left), Convert(right), FilterType.Or);

		public static BinaryFilter Equal(Filter left, Filter right)
			=> Binary(left, right, FilterType.Equal);

		public static BinaryFilter Equal(IFilter left, IFilter right)
			=> Binary(Convert(left), Convert(right), FilterType.Equal);

		public static BinaryFilter NotEquals(Filter left, Filter right)
			=> Binary(left, right, FilterType.NotEqual);

		public static BinaryFilter NotEqual(IFilter left, IFilter right)
			=> Binary(Convert(left), Convert(right), FilterType.NotEqual);

		public static BinaryFilter GreaterThan(Filter left, Filter right)
			=> Binary(left, right, FilterType.GreaterThan);

		public static BinaryFilter GreaterThan(IFilter left, IFilter right)
			=> Binary(Convert(left), Convert(right), FilterType.GreaterThan);

		public static BinaryFilter GreaterThanOrEqual(Filter left, Filter right)
			=> Binary(left, right, FilterType.GreaterThanOrEqual);

		public static BinaryFilter GreaterThanOrEqual(IFilter left, IFilter right)
			=> Binary(Convert(left), Convert(right), FilterType.GreaterThanOrEqual);

		public static BinaryFilter LessThan(Filter left, Filter right)
			=> Binary(left, right, FilterType.LessThan);

		public static BinaryFilter LessThan(IFilter left, IFilter right)
			=> Binary(Convert(left), Convert(right), FilterType.LessThan);

		public static BinaryFilter LessThanOrEqual(Filter left, Filter right)
			=> Binary(left, right, FilterType.LessThanOrEqual);

		public static BinaryFilter LessThanOrEqual(IFilter left, IFilter right)
			=> Binary(Convert(left), Convert(right), FilterType.LessThanOrEqual);

		public static FunctionFilter Function(VariableFilter variable, string functionName, params Filter[] arguments)
			=> new FunctionFilter(variable, functionName, arguments);

		public static FunctionFilter Function(VariableFilter variable, string functionName, params IFilter[] arguments)
			=> new FunctionFilter(variable, functionName, arguments?.Select(Convert).ToArray());

		public static ConstantFilter Constant(object? value)
			=> new ConstantFilter(value);

		public static VariableFilter Variable(string variableName) {
			if (String.IsNullOrWhiteSpace(variableName))
				throw new ArgumentException("The given variable name is null or whitespace.", nameof(variableName));

			if (!IsValidReference(variableName))
				throw new ArgumentException($"The given variable name '{variableName}' is not a valid reference.", nameof(variableName));

			return new VariableFilter(variableName);
		}

		#endregion

		#region EmptyFilter

		class EmptyFilter : Filter {
			public override FilterType FilterType => new FilterType();
		}

		#endregion
	}
}