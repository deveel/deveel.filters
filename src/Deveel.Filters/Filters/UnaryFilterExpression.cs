// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Deveel.Filters {
	/// <summary>
	/// Represents a unary filter expression that applies an operator
	/// to a single operand (e.g., logical negation).
	/// </summary>
	public sealed class UnaryFilterExpression : FilterExpression {
		private readonly FilterExpressionType filterExpressionType;

		/// <summary>
		/// Initializes a new instance of the <see cref="UnaryFilterExpression"/> class
		/// with the specified operand and expression type.
		/// </summary>
		/// <param name="operand">The operand of the unary expression.</param>
		/// <param name="expressionType">The type of the unary expression.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="operand"/> is <c>null</c>.
		/// </exception>
		internal UnaryFilterExpression(FilterExpression operand, FilterExpressionType expressionType) {
			Operand = operand ?? throw new ArgumentNullException(nameof(operand));
			filterExpressionType = expressionType;
		}

		/// <inheritdoc/>
		public override FilterExpressionType ExpressionType => filterExpressionType;

		/// <summary>
		/// Gets the operand of the unary expression.
		/// </summary>
		public FilterExpression Operand { get; }
	}
}
