// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Deveel.Filters {
	/// <summary>
	/// Represents a binary filter expression that combines two operands
	/// (left and right) with a binary operator (e.g., equality, comparison, logical).
	/// </summary>
	public sealed class BinaryFilterExpression : FilterExpression {
		private readonly FilterExpressionType expressionType;

		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryFilterExpression"/> class
		/// with the specified left and right operands and expression type.
		/// </summary>
		/// <param name="left">The left operand of the binary expression.</param>
		/// <param name="right">The right operand of the binary expression.</param>
		/// <param name="expressionType">The type of the binary expression.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.
		/// </exception>
		internal BinaryFilterExpression(FilterExpression left, FilterExpression right, FilterExpressionType expressionType) {
			Left = left ?? throw new ArgumentNullException(nameof(left));
			Right = right ?? throw new ArgumentNullException(nameof(right));
			this.expressionType = expressionType;
		}

		/// <inheritdoc/>
		public override FilterExpressionType ExpressionType => expressionType;

		/// <summary>
		/// Gets the left operand of the binary expression.
		/// </summary>
		public FilterExpression Left { get; }
		
		/// <summary>
		/// Gets the right operand of the binary expression.
		/// </summary>
		public FilterExpression Right { get; }
	}
}
