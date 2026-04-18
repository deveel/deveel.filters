// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Deveel.Filters {
	/// <summary>
	/// Provides a base visitor for traversing and transforming a <see cref="FilterExpression"/> tree.
	/// </summary>
	/// <remarks>
	/// Subclasses can override individual <c>Visit*</c> methods to customize behavior
	/// for specific expression types.
	/// </remarks>
    public class FilterExpressionVisitor {
		/// <summary>
		/// Visits the specified filter expression and dispatches to the
		/// appropriate typed visit method based on its <see cref="FilterExpression.ExpressionType"/>.
		/// </summary>
		/// <param name="filter">The filter expression to visit.</param>
		/// <returns>The resulting <see cref="FilterExpression"/> after visiting.</returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the expression type is not supported by this visitor.
		/// </exception>
		public FilterExpression Visit(FilterExpression filter) {
			if (filter.IsEmpty)
				return FilterExpression.Empty;

			switch (filter.ExpressionType) {
				case FilterExpressionType.Equal:
				case FilterExpressionType.NotEqual:
				case FilterExpressionType.GreaterThan:
				case FilterExpressionType.GreaterThanOrEqual:
				case FilterExpressionType.LessThan:
				case FilterExpressionType.LessThanOrEqual:
				case FilterExpressionType.And:
				case FilterExpressionType.Or:
					return VisitBinary((BinaryFilterExpression) filter);
				case FilterExpressionType.Not:
					return VisitUnary((UnaryFilterExpression) filter);
				case FilterExpressionType.Function:
					return VisitFunction((FunctionFilterExpression)filter);
				case FilterExpressionType.Constant:
					return VisitConstant((ConstantFilterExpression) filter);
				case FilterExpressionType.Variable:
					return VisitVariable((VariableFilterExpression)filter);
			}

			throw new NotSupportedException($"The filter type '{filter.ExpressionType}' is not supported.");
		}

		/// <summary>
		/// Visits the arguments of a function filter expression.
		/// </summary>
		/// <param name="arguments">The list of argument expressions, or <c>null</c>.</param>
		/// <returns>A new list of visited argument expressions.</returns>
		public virtual IList<FilterExpression> VisitFunctionArguments(IList<FilterExpression>? arguments) {
			var list = new List<FilterExpression>(arguments?.Count ?? 0);
			if (arguments != null) {
				foreach (var argument in arguments) {
					list.Add(Visit(argument));
				}
			}

			return list;
		}

		/// <summary>
		/// Visits a function filter expression.
		/// </summary>
		/// <param name="filterExpression">The function filter expression to visit.</param>
		/// <returns>The resulting <see cref="FilterExpression"/> after visiting.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the visited variable is not a <see cref="VariableFilterExpression"/>.
		/// </exception>
		public virtual FilterExpression VisitFunction(FunctionFilterExpression filterExpression) {
			var variable = VisitVariable(filterExpression.Variable);
			var arguments = VisitFunctionArguments(filterExpression.Arguments);

			var args = new FilterExpression[arguments.Count];
			for (var i = 0; i < arguments.Count; i++) {
				args[i] = (FilterExpression)arguments[i];
			}

			if (!(variable is VariableFilterExpression variableFilter))
				throw new InvalidOperationException($"The variable '{variable}' is not a valid function variable.");

			return FilterExpression.Function(variableFilter, filterExpression.FunctionName, args);
		}

		/// <summary>
		/// Visits a constant filter expression.
		/// </summary>
		/// <param name="constant">The constant filter expression to visit.</param>
		/// <returns>A new <see cref="ConstantFilterExpression"/> with the same value.</returns>
		public virtual FilterExpression VisitConstant(ConstantFilterExpression constant) {
			return new ConstantFilterExpression(constant.Value);
		}

		/// <summary>
		/// Visits a variable filter expression.
		/// </summary>
		/// <param name="variable">The variable filter expression to visit.</param>
		/// <returns>A new <see cref="VariableFilterExpression"/> with the same variable name.</returns>
		public virtual FilterExpression VisitVariable(VariableFilterExpression variable) {
			return FilterExpression.Variable(variable.VariableName);
		}

		/// <summary>
		/// Visits a unary filter expression.
		/// </summary>
		/// <param name="filterExpression">The unary filter expression to visit.</param>
		/// <returns>The resulting <see cref="FilterExpression"/> after visiting the operand.</returns>
		public virtual FilterExpression VisitUnary(UnaryFilterExpression filterExpression) {
			var operand = (FilterExpression) Visit(filterExpression.Operand);

			return FilterExpression.Unary(operand, filterExpression.ExpressionType);
		}

		/// <summary>
		/// Visits a binary filter expression.
		/// </summary>
		/// <param name="filterExpression">The binary filter expression to visit.</param>
		/// <returns>The resulting <see cref="FilterExpression"/> after visiting both operands.</returns>
		public virtual FilterExpression VisitBinary(BinaryFilterExpression filterExpression) {
			var left = (FilterExpression) Visit(filterExpression.Left);
			var right = (FilterExpression) Visit(filterExpression.Right);

			return FilterExpression.Binary(left, right, filterExpression.ExpressionType);
		}
	}
}
