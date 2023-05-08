using System.Linq.Expressions;
using System.Text;

namespace Deveel.Filters {
	public static class FilterExtensions {
		public static bool IsEmpty(this IFilter filter) {
			return filter is null || Filter.Empty.Equals(filter);
		}

		public static string AsString(this IFilter filter) {
			var builder = new StringBuilder();
			var visitor = new FilterStringBuilder(builder);
			visitor.Visit(filter);
			return builder.ToString();
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
		public static Expression<Func<T, bool>> AsLambda<T>(this IFilter filter, string parameterName = "x") {
			return (Expression<Func<T, bool>>)filter.AsLambda(typeof(T), parameterName);
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
		public static LambdaExpression AsLambda(this IFilter filter, Type parameterType, string parameterName = "x") {
			if (parameterType is null)
				throw new ArgumentNullException(nameof(parameterType));

			if (String.IsNullOrWhiteSpace(parameterName))
				throw new ArgumentException($"'{nameof(parameterName)}' cannot be null or whitespace.", nameof(parameterName));

			try {
				var builder = new LambdaFilterBuilder(parameterType, parameterName);
				return builder.BuildLambda(filter);
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
		public static LambdaExpression AsAsyncLambda(this IFilter filter, Type parameterType, string parameterName = "x") {
			if (parameterType is null)
				throw new ArgumentNullException(nameof(parameterType));
			if (String.IsNullOrWhiteSpace(parameterName))
				throw new ArgumentException($"'{nameof(parameterName)}' cannot be null or whitespace.", nameof(parameterName));

			try {
				var builder = new LambdaFilterBuilder(parameterType, parameterName);
				return builder.BuildAsyncLambda(filter);
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
		public static Expression<Func<T, Task<bool>>> AsAsyncLambda<T>(this IFilter filter, string parameterName = "x") {
			return (Expression<Func<T, Task<bool>>>)filter.AsAsyncLambda(typeof(T), parameterName);
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
		public static bool Evaluate(this IFilter filter, Type parameterType, string parameterName, object parameterValue) {
			if (parameterType is null)
				throw new ArgumentNullException(nameof(parameterType));
			if (String.IsNullOrWhiteSpace(parameterName))
				throw new ArgumentException("The parameter name cannot be null or empty", nameof(parameterName));

			try {
				var lambda = filter.AsLambda(parameterType, parameterName);
				var compiled = lambda.Compile();

				return (bool)compiled.DynamicInvoke(parameterValue);
			} catch (Exception ex) {
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
		public static bool Evaluate(this IFilter filter, Type parameterType, object parameterValue)
			=> filter.Evaluate(parameterType, "x", parameterValue);

		public static bool Evaluate<T>(this IFilter filter, string parameterName, T parameterValue) {
			try {
				var lambda = filter.AsLambda<T>(parameterName);
				var compiled = lambda.Compile();

				return compiled(parameterValue);
			} catch (Exception ex) {
				throw new FilterEvaluationException("Unable to evaluate the filter", ex);
			}
		}

		public static bool Evaluate<T>(this IFilter filter, T parameterValue)
			=> filter.Evaluate("x", parameterValue);

		public static Task<bool> EvaluateAsync(this IFilter filter, Type parameterType, string parameterName, object parameterValue) {
			var asyncLambda = filter.AsAsyncLambda(parameterType, parameterName);
			var compiled = asyncLambda.Compile();

			var task = compiled.DynamicInvoke(parameterValue);

			return (Task<bool>)task;
		}

		public static Task<bool> EvaluateAsync<T>(this IFilter filter, string parameterName, T parameterValue) {
			var asyncLambda = filter.AsAsyncLambda<T>(parameterName);
			var compiled = asyncLambda.Compile();

			return compiled.Invoke(parameterValue);
		}

		public static Task<bool> EvaluateAsync<T>(this IFilter filter, T parameterValue)
			=> filter.EvaluateAsync("x", parameterValue);

	}
}
