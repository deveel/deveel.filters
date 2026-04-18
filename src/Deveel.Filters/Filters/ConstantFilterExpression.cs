// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Deveel.Filters {
	/// <summary>
	/// Represents a constant value expression within a filter tree.
	/// </summary>
	public sealed class ConstantFilterExpression : FilterExpression {
		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantFilterExpression"/> class
		/// with the specified constant value.
		/// </summary>
		/// <param name="value">The constant value of the expression, or <c>null</c>.</param>
		public ConstantFilterExpression(object? value) {
			Value = value;
		}

		/// <summary>
		/// Gets the constant value of this expression.
		/// </summary>
		public object? Value { get; }

		/// <inheritdoc/>
		public override FilterExpressionType ExpressionType => FilterExpressionType.Constant;
	}
}
