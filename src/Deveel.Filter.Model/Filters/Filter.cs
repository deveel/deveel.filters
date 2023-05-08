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

        /// <summary>
        /// Produces a lambda expression that represents the filter
        /// </summary>
        /// <typeparam name="T">
        /// The type of the parameter of the expression.
        /// </typeparam>
        /// <param name="parameterName">
        /// The name of the parameter to use in the expression. 
        /// <strong>Note</strong>: this name must match the variable references
        /// in the filter.
        /// </param>
        /// <returns>
        /// Returns a <see cref="Expression{TDelegate}"/> that represents the
        /// expression tree of the filter, that can be used to compile a delegate.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="parameterName"/> is <c>null</c> or empty.
        /// </exception>
        public Expression<Func<T, bool>> AsLambda<T>(string parameterName = "x") {
            return (Expression<Func<T, bool>>)AsLambda(typeof(T), parameterName);
		}

        /// <summary>
        /// Produces a lambda expression that represents the filter
        /// </summary>
        /// <param name="parameterType">
        /// The type of the parameter of the expression.
        /// </param>
        /// <param name="parameterName">
        /// The name of the parameter to use in the expression. 
        /// <strong>Note</strong>: this name must match the variable references
        /// in the filter.
        /// </param>
        /// <returns>
		/// Returns a <see cref="LambdaExpression"/> that represents the
		/// expression tree of the filter, that can be used to compile a delegate.
        /// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="parameterName"/> is <c>null</c> or empty.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="parameterType"/> is <c>null</c>.
		/// </exception>
        public LambdaExpression AsLambda(Type parameterType, string parameterName = "x") {
            if (parameterType is null)
                throw new ArgumentNullException(nameof(parameterType));

            if (String.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentException($"'{nameof(parameterName)}' cannot be null or whitespace.", nameof(parameterName));

			try {
                var builder = new LambdaFilterBuilder(parameterType, parameterName);
                return builder.BuildLambda(this);
            } catch (Exception ex) {
				throw new FilterException("Unable to compile the filter to a lambda expression", ex);
			}
		}

        /// <summary>
        /// Produces an asynchrounous lambda expression that represents the filter
        /// </summary>
        /// <param name="parameterType">
        /// The type of the parameter of the expression.
        /// </param>
        /// <param name="parameterName">
        /// The name of the parameter to use in the expression. 
        /// <strong>Note</strong>: this name must match the variable references
        /// in the filter.
        /// </param>
        /// <returns>
		/// Returns a <see cref="LambdaExpression"/> that represents the
		/// expression tree of the filter, that can be used to compile an asynchrouns delegate.
		/// </returns>
        public LambdaExpression AsAsyncLambda(Type parameterType, string parameterName = "x") {
            if (parameterType is null)
                throw new ArgumentNullException(nameof(parameterType));
            if (String.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentException($"'{nameof(parameterName)}' cannot be null or whitespace.", nameof(parameterName));

			try {
                var builder = new LambdaFilterBuilder(parameterType, parameterName);
                return builder.BuildAsyncLambda(this);
            } catch (Exception ex) {
				throw new FilterException("Unable to compile the filter to a lambda expression", ex);
			}
		}

		/// <summary>
		/// Produces an asynchrounous lambda expression that represents the filter
		/// </summary>
		/// <typeparam name="T">
		/// The type of the parameter of the expression.
		/// </typeparam>
		/// <param name="parameterName">
		/// The name of the parameter to use in the expression. 
		/// <strong>Note</strong>: this name must match the variable references
		/// in the filter.
		/// </param>
		/// <returns>
		/// Returns a <see cref="Expression{TDelegate}"/> that represents the
		/// expression tree of the filter, that can be used to compile an asynchrouns delegate.
		/// </returns>
		public Expression<Func<T, Task<bool>>> AsAsyncLambda<T>(string parameterName = "x") {
			return (Expression<Func<T, Task<bool>>>)AsAsyncLambda(typeof(T), parameterName);
		}

		/// <summary>
		/// Compiles and evaluates the filter against the given parameter value.
		/// </summary>
		/// <param name="parameterType">
		/// The type of the parameter to use in the evaluation.
		/// </param>
		/// <param name="parameterName">
		/// The name of the parameter to use in the evaluation.
		/// </param>
		/// <param name="parameterValue">
		/// The value of the parameter to use in the evaluation.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the given value matches the filter, otherwise
		/// it returns <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="parameterType"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="parameterName"/> is <c>null</c> or empty.
		/// </exception>
		/// <exception cref="FilterEvaluationException">
		/// Thrown when the filter cannot be evaluated because of an
		/// unhandled error.
		/// </exception>
		public bool Evaluate(Type parameterType, string parameterName, object parameterValue) {
            if (parameterType is null)
                throw new ArgumentNullException(nameof(parameterType));
			if (String.IsNullOrWhiteSpace(parameterName))
				throw new ArgumentException("The parameter name cannot be null or empty", nameof(parameterName));

			try {
                var lambda = AsLambda(parameterType, parameterName);
                var compiled = lambda.Compile();

                return (bool)compiled.DynamicInvoke(parameterValue);
            } catch(Exception ex) {
				throw new FilterEvaluationException("Unable to evaluate the filter", ex);
			}
        }

		/// <summary>
		/// Compiles and evaluates the filter against the given parameter value.
		/// </summary>
		/// <param name="parameterType">
		/// The type of the parameter to use in the evaluation.
		/// </param>
		/// <param name="parameterValue">
		/// The value of the parameter to use in the evaluation.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the given value matches the filter, otherwise
		/// </returns>
		public bool Evaluate(Type parameterType, object parameterValue)
			=> Evaluate(parameterType, "x", parameterValue);

		public bool Evaluate<T>(string parameterName, T parameterValue) {
			try {
				var lambda = AsLambda<T>(parameterName);
				var compiled = lambda.Compile();

				return compiled(parameterValue);
			} catch (Exception ex) {
				throw new FilterEvaluationException("Unable to evaluate the filter", ex);
			}
		}

		public bool Evaluate<T>(T parameterValue)
			=> Evaluate("x", parameterValue);

		public Task<bool> EvaluateAsync(Type parameterType, string parameterName, object parameterValue) {
			var asyncLambda = AsAsyncLambda(parameterType, parameterName);
			var compiled = asyncLambda.Compile();

			var task = compiled.DynamicInvoke(parameterValue);

			return (Task<bool>)task;
		}

		public Task<bool> EvaluateAsync<T>(string parameterName, T parameterValue) {
            var asyncLambda = AsAsyncLambda<T>(parameterName);
            var compiled = asyncLambda.Compile();

			return compiled.Invoke(parameterValue);
        }

		public Task<bool> EvaluateAsync<T>(T parameterValue)
			=> EvaluateAsync("x", parameterValue);

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