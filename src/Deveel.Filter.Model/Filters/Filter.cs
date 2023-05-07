using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Deveel.Filters {
	/// <summary>
	/// Represents a filter that can be used to restrict
	/// the result of a query.
	/// </summary>
	[DebuggerDisplay("{ToString(),nq}")]
	public abstract class Filter {
		/// <summary>
		/// Gets the type of filter.
		/// </summary>
		public abstract FilterType FilterType { get; }

		/// <summary>
		/// Gets a value indicating if this filter is empty.
		/// </summary>
		public bool IsEmpty => this is EmptyFilter;

		/// <summary>
		/// An empty filter that has no effect on the result.
		/// </summary>
		public static readonly Filter Empty = new EmptyFilter();

		/// <inheritdoc/>
		public override string ToString() {
			var sb = new StringBuilder();
			var stringVisitor = new FilterStringBuilder(sb);
			stringVisitor.Visit(this);
			return sb.ToString();
		}

		public Expression<Func<T, bool>> AsLambda<T>(string parameterName = "x") {
			return (Expression<Func<T, bool>>)AsLambda(typeof(T), parameterName);
		}

		public LambdaExpression AsLambda(Type parameterType, string parameterName = "x") {
			var builder = new LambdaFilterBuilder(parameterType, parameterName);
			return builder.BuildLambda(this);
		}

		public LambdaExpression AsAsyncLambda(Type parameterType, string parameterName = "x") {
			var builder = new LambdaFilterBuilder(parameterType, parameterName);
			return builder.BuildAsyncLambda(this);
		}

		public Expression<Func<T, Task<bool>>> AsAsyncLambda<T>(string parameterName = "x") {
			return (Expression<Func<T, Task<bool>>>)AsAsyncLambda(typeof(T), parameterName);
		}

		#region Factories

		public static UnaryFilter Unary(Filter operand, FilterType filterType) {
			if (filterType != FilterType.Not)
				throw new ArgumentException($"The filter type '{filterType}' is not a unary filter type.", nameof(filterType));

			return new UnaryFilter(operand, filterType);
		}

		public static UnaryFilter Not(Filter operand)
			=> Unary(operand, FilterType.Not);

		public static BinaryFilter Binary(Filter left, Filter right, FilterType filterType) {
			if (filterType != FilterType.And &&
				filterType != FilterType.Or &&
				filterType != FilterType.Equals &&
				filterType != FilterType.NotEquals &&
				filterType != FilterType.LessThan &&
				filterType != FilterType.LessThanOrEqual &&
				filterType != FilterType.GreaterThan &&
				filterType != FilterType.GreaterThanOrEqual)
				throw new ArgumentException($"The filter type '{filterType}' is not a binary filter type.", nameof(filterType));

			return new BinaryFilter(left, right, filterType);
		}

		public static BinaryFilter And(Filter left, Filter right)
			=> Binary(left, right, FilterType.And);

		public static BinaryFilter Or(Filter left, Filter right)
			=> Binary(left, right, FilterType.Or);

		public static BinaryFilter Equals(Filter left, Filter right)
			=> Binary(left, right, FilterType.Equals);

		public static BinaryFilter NotEquals(Filter left, Filter right)
			=> Binary(left, right, FilterType.NotEquals);

		public static BinaryFilter GreaterThan(Filter left, Filter right)
			=> Binary(left, right, FilterType.GreaterThan);

		public static BinaryFilter GreaterThanOrEqual(Filter left, Filter right)
			=> Binary(left, right, FilterType.GreaterThanOrEqual);

		public static BinaryFilter LessThan(Filter left, Filter right)
			=> Binary(left, right, FilterType.LessThan);

		public static BinaryFilter LessThanOrEqual(Filter left, Filter right)
			=> Binary(left, right, FilterType.LessThanOrEqual);

		public static FunctionFilter Function(VariableFilter variable, string functionName, params Filter[] arguments)
			=> new FunctionFilter(variable, functionName, arguments);

		public static ConstantFilter Constant(object? value)
			=> new ConstantFilter(value);

		public static VariableFilter Variable(string variableName)
			=> new VariableFilter(variableName);

		#endregion

		#region EmptyFilter

		class EmptyFilter : Filter {
			public override FilterType FilterType => new FilterType();
		}

		#endregion
	}
}