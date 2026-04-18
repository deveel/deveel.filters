// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Deveel.Filters;

/// <summary>
/// A fluent builder for creating <see cref="FilterExpression"/> objects.
/// </summary>
public sealed class FilterExpressionBuilder {
	private FilterExpression? _expression;

	/// <summary>
	/// Starts a filter expression with a reference to a variable.
	/// </summary>
	/// <param name="variableName">The name of the variable to reference.</param>
	/// <returns>
	/// Returns a <see cref="FieldFilterExpressionBuilder"/> to continue building
	/// the filter for the specified variable.
	/// </returns>
	public FieldFilterExpressionBuilder Where(string variableName) {
		return new FieldFilterExpressionBuilder(this, FilterExpression.Variable(variableName));
	}

	/// <summary>
	/// Combines the current expression with another using a logical AND,
	/// starting a new condition on the specified variable.
	/// </summary>
	/// <param name="variableName">The name of the variable to reference.</param>
	/// <returns>
	/// Returns a <see cref="FieldFilterExpressionBuilder"/> to continue building
	/// the filter for the specified variable.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when no previous expression has been set.
	/// </exception>
	public FieldFilterExpressionBuilder And(string variableName) {
		if (_expression == null)
			throw new InvalidOperationException("No previous expression to combine with AND.");

		return new FieldFilterExpressionBuilder(this, FilterExpression.Variable(variableName), FilterExpressionType.And);
	}

	/// <summary>
	/// Combines the current expression with another using a logical OR,
	/// starting a new condition on the specified variable.
	/// </summary>
	/// <param name="variableName">The name of the variable to reference.</param>
	/// <returns>
	/// Returns a <see cref="FieldFilterExpressionBuilder"/> to continue building
	/// the filter for the specified variable.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when no previous expression has been set.
	/// </exception>
	public FieldFilterExpressionBuilder Or(string variableName) {
		if (_expression == null)
			throw new InvalidOperationException("No previous expression to combine with OR.");

		return new FieldFilterExpressionBuilder(this, FilterExpression.Variable(variableName), FilterExpressionType.Or);
	}

	/// <summary>
	/// Negates the current expression.
	/// </summary>
	/// <returns>
	/// Returns this builder instance for further chaining.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when no previous expression has been set.
	/// </exception>
	public FilterExpressionBuilder Not() {
		if (_expression == null)
			throw new InvalidOperationException("No previous expression to negate.");

		_expression = FilterExpression.Not(_expression);
		return this;
	}

	internal void SetExpression(FilterExpression expression, FilterExpressionType? combineWith) {
		if (_expression == null || combineWith == null) {
			_expression = expression;
		} else {
			_expression = FilterExpression.Binary(_expression, expression, combineWith.Value);
		}
	}

	/// <summary>
	/// Builds the final <see cref="FilterExpression"/>.
	/// </summary>
	/// <returns>
	/// Returns the constructed <see cref="FilterExpression"/>, or
	/// <see cref="FilterExpression.Empty"/> if no conditions were added.
	/// </returns>
	public FilterExpression Build() {
		return _expression ?? FilterExpression.Empty;
	}
}