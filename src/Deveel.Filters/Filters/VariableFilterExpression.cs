// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Deveel.Filters {
	/// <summary>
	/// Represents a variable reference expression within a filter tree,
	/// pointing to a named field or property of the filtered object.
	/// </summary>
	public sealed class VariableFilterExpression : FilterExpression {
		/// <summary>
		/// Initializes a new instance of the <see cref="VariableFilterExpression"/> class
		/// with the specified variable name.
		/// </summary>
		/// <param name="variableName">The name of the variable referenced by this expression.</param>
		internal VariableFilterExpression(string variableName) {
			VariableName = variableName;
		}

		/// <inheritdoc/>
		public override FilterExpressionType ExpressionType => FilterExpressionType.Variable;

		/// <summary>
		/// Gets the name of the variable referenced by this expression.
		/// </summary>
		public string VariableName { get; }
	}
}
