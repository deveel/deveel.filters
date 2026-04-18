// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Text;

namespace Deveel.Filters {
	/// <summary>
	/// Represents a default implementation of a filter that 
	/// can be used to restrict the result of a query.
	/// </summary>
	[DebuggerDisplay("{ToString(),nq}")]
	public abstract class FilterExpression {
		/// <summary>
		/// Gets the type of filter.
		/// </summary>
		public abstract FilterExpressionType ExpressionType { get; }

		/// <summary>
		/// An empty filter that has no effect on the result.
		/// </summary>
		public static readonly FilterExpression Empty = new EmptyFilter();
		
		/// <summary>
		/// Gets a value indicating whether this filter expression is empty.
		/// </summary>
		public bool IsEmpty => Empty.Equals(this);

		/// <inheritdoc/>
		public override string ToString() {
			var builder = new StringBuilder();
			var visitor = new FilterStringBuilder(builder);
			visitor.Visit(this);
			return builder.ToString();
		}

		
		/// <summary>
		/// Determines whether the specified variable name is a valid filter variable reference.
		/// </summary>
		/// <param name="variableName">The variable name to validate.</param>
		/// <returns>
		/// Returns <c>true</c> if the variable name is not null or whitespace and contains
		/// only letters, digits, underscores, or dots; otherwise <c>false</c>.
		/// </returns>
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
		/// <param name="expressionType">
		/// The type of the unary filter.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="UnaryFilterExpression"/> with the given operand and
		/// of the given type.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="expressionType"/> is not a unary filter type.
		/// </exception>
		public static UnaryFilterExpression Unary(FilterExpression operand, FilterExpressionType expressionType) {
			if (expressionType != FilterExpressionType.Not)
				throw new ArgumentException($"The filter type '{expressionType}' is not a unary filter type.", nameof(expressionType));

			return new UnaryFilterExpression(operand, expressionType);
		}
		
		/// <summary>
		/// Creates an unary filter that negates the given operand.
		/// </summary>
		/// <param name="operand">
		/// The filter operand to negate.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="UnaryFilterExpression"/> that negates the given
		/// filter operand.
		/// </returns>

		public static UnaryFilterExpression Not(FilterExpression operand)
			=> Unary(operand, FilterExpressionType.Not);
		
		/// <summary>
		/// Creates a new binary filter with the given left and right operands
		/// </summary>
		/// <param name="left">
		/// The left operand of the binary filter.
		/// </param>
		/// <param name="right">
		/// The right operand of the binary filter.
		/// </param>
		/// <param name="expressionType">
		/// The type of the binary filter.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> of the given type with 
		/// the given left and right operands.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="expressionType"/> is not a valid binary filter type.
		/// </exception>
		public static BinaryFilterExpression Binary(FilterExpression left, FilterExpression right, FilterExpressionType expressionType) {
			if (expressionType != FilterExpressionType.And &&
				expressionType != FilterExpressionType.Or &&
				expressionType != FilterExpressionType.Equal &&
				expressionType != FilterExpressionType.NotEqual &&
				expressionType != FilterExpressionType.LessThan &&
				expressionType != FilterExpressionType.LessThanOrEqual &&
				expressionType != FilterExpressionType.GreaterThan &&
				expressionType != FilterExpressionType.GreaterThanOrEqual)
				throw new ArgumentException($"The filter type '{expressionType}' is not a binary filter type.", nameof(expressionType));

			return new BinaryFilterExpression(left, right, expressionType);
		}
		
		/// <summary>
		/// Creates a binary filter that combines two operands with a logical AND.
		/// </summary>
		/// <param name="left">The left operand.</param>
		/// <param name="right">The right operand.</param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> with <see cref="FilterExpressionType.And"/>.
		/// </returns>
		public static BinaryFilterExpression And(FilterExpression left, FilterExpression right)
			=> Binary(left, right, FilterExpressionType.And);
		
		/// <summary>
		/// Creates a binary filter that combines two operands with a logical OR.
		/// </summary>
		/// <param name="left">The left operand.</param>
		/// <param name="right">The right operand.</param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> with <see cref="FilterExpressionType.Or"/>.
		/// </returns>
		public static BinaryFilterExpression Or(FilterExpression left, FilterExpression right)
			=> Binary(left, right, FilterExpressionType.Or);
		
		/// <summary>
		/// Creates a binary filter that compares two operands for equality.
		/// </summary>
		/// <param name="left">The left operand.</param>
		/// <param name="right">The right operand.</param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> with <see cref="FilterExpressionType.Equal"/>.
		/// </returns>
		public static BinaryFilterExpression Equal(FilterExpression left, FilterExpression right)
			=> Binary(left, right, FilterExpressionType.Equal);
		
		/// <summary>
		/// Creates a binary filter that compares two operands for inequality.
		/// </summary>
		/// <param name="left">The left operand.</param>
		/// <param name="right">The right operand.</param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> with <see cref="FilterExpressionType.NotEqual"/>.
		/// </returns>
		public static BinaryFilterExpression NotEquals(FilterExpression left, FilterExpression right)
			=> Binary(left, right, FilterExpressionType.NotEqual);
		
		/// <summary>
		/// Creates a binary filter that checks if the left operand is greater than the right.
		/// </summary>
		/// <param name="left">The left operand.</param>
		/// <param name="right">The right operand.</param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> with <see cref="FilterExpressionType.GreaterThan"/>.
		/// </returns>
		public static BinaryFilterExpression GreaterThan(FilterExpression left, FilterExpression right)
			=> Binary(left, right, FilterExpressionType.GreaterThan);
		
		/// <summary>
		/// Creates a binary filter that checks if the left operand is greater than or equal to the right.
		/// </summary>
		/// <param name="left">The left operand.</param>
		/// <param name="right">The right operand.</param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> with <see cref="FilterExpressionType.GreaterThanOrEqual"/>.
		/// </returns>
		public static BinaryFilterExpression GreaterThanOrEqual(FilterExpression left, FilterExpression right)
			=> Binary(left, right, FilterExpressionType.GreaterThanOrEqual);
		
		/// <summary>
		/// Creates a binary filter that checks if the left operand is less than the right.
		/// </summary>
		/// <param name="left">The left operand.</param>
		/// <param name="right">The right operand.</param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> with <see cref="FilterExpressionType.LessThan"/>.
		/// </returns>
		public static BinaryFilterExpression LessThan(FilterExpression left, FilterExpression right)
			=> Binary(left, right, FilterExpressionType.LessThan);
		
		/// <summary>
		/// Creates a binary filter that checks if the left operand is less than or equal to the right.
		/// </summary>
		/// <param name="left">The left operand.</param>
		/// <param name="right">The right operand.</param>
		/// <returns>
		/// Returns a new <see cref="BinaryFilterExpression"/> with <see cref="FilterExpressionType.LessThanOrEqual"/>.
		/// </returns>
		public static BinaryFilterExpression LessThanOrEqual(FilterExpression left, FilterExpression right)
			=> Binary(left, right, FilterExpressionType.LessThanOrEqual);
		
		/// <summary>
		/// Creates a function filter expression that invokes the specified function
		/// on the given variable with the provided arguments.
		/// </summary>
		/// <param name="variable">The variable on which the function is invoked.</param>
		/// <param name="functionName">The name of the function to invoke.</param>
		/// <param name="arguments">The arguments to pass to the function.</param>
		/// <returns>
		/// Returns a new <see cref="FunctionFilterExpression"/>.
		/// </returns>
		public static FunctionFilterExpression Function(VariableFilterExpression variable, string functionName, FilterExpression[] arguments)
			=> new FunctionFilterExpression(variable, functionName, arguments);
		
		/// <summary>
		/// Creates a function filter expression that invokes the specified function
		/// on the given variable with no arguments.
		/// </summary>
		/// <param name="variable">The variable on which the function is invoked.</param>
		/// <param name="functionName">The name of the function to invoke.</param>
		/// <returns>
		/// Returns a new <see cref="FunctionFilterExpression"/> with no arguments.
		/// </returns>
		public static FunctionFilterExpression Function(VariableFilterExpression variable, string functionName)
			=> new FunctionFilterExpression(variable, functionName, Array.Empty<FilterExpression>());

		/// <summary>
		/// Creates a constant filter expression with the specified value.
		/// </summary>
		/// <param name="value">The constant value, or <c>null</c>.</param>
		/// <returns>
		/// Returns a new <see cref="ConstantFilterExpression"/> wrapping the given value.
		/// </returns>
		public static ConstantFilterExpression Constant(object? value)
			=> new ConstantFilterExpression(value);

		/// <summary>
		/// Creates a variable filter expression referencing the specified variable name.
		/// </summary>
		/// <param name="variableName">The name of the variable to reference.</param>
		/// <returns>
		/// Returns a new <see cref="VariableFilterExpression"/> for the given variable name.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="variableName"/> is <c>null</c>, whitespace,
		/// or not a valid variable reference.
		/// </exception>
		public static VariableFilterExpression Variable(string variableName) {
			if (String.IsNullOrWhiteSpace(variableName))
				throw new ArgumentException("The given variable name is null or whitespace.", nameof(variableName));

			if (!IsValidReference(variableName))
				throw new ArgumentException($"The given variable name '{variableName}' is not a valid reference.", nameof(variableName));

			return new VariableFilterExpression(variableName);
		}

#endregion

		#region EmptyFilter

		class EmptyFilter : FilterExpression {
			public override FilterExpressionType ExpressionType => new FilterExpressionType();
		}

		#endregion
	}
}