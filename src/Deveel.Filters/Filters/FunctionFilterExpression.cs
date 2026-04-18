// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;

namespace Deveel.Filters {
	/// <summary>
	/// The invocation to a function that returns a
	/// boolean for filtering
	/// </summary>
	public sealed class FunctionFilterExpression : FilterExpression {
		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionFilterExpression"/> class.
		/// </summary>
		/// <param name="variable">The variable that defines the function to invoke.</param>
		/// <param name="functionName">The name of the function to invoke.</param>
		/// <param name="arguments">
		/// An optional array of arguments to pass to the function. Each argument must be
		/// either a <see cref="ConstantFilterExpression"/> or a <see cref="VariableFilterExpression"/>.
		/// </param>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="functionName"/> is <c>null</c> or empty, or when
		/// any argument is not a constant or variable expression.
		/// </exception>
		internal FunctionFilterExpression(VariableFilterExpression variable, string functionName, FilterExpression[]? arguments) {
			if (string.IsNullOrEmpty(functionName)) 
				throw new ArgumentException($"'{nameof(functionName)}' cannot be null or empty.", nameof(functionName));

			if (arguments != null) {
				if (arguments.Any(x => x.ExpressionType != FilterExpressionType.Constant && x.ExpressionType != FilterExpressionType.Variable))
					throw new ArgumentException("The arguments of a function must be either a constant or a variable.");
			}
			
			Variable = variable;
			FunctionName = functionName;
			Arguments = arguments;
		}

		/// <summary>
		/// Gets the name of the function to invoke, that is
		/// defined by the object refereced by <see cref="Variable"/>
		/// </summary>
		public string FunctionName { get; }

		/// <summary>
		/// Gets the variable that references the object that
		/// defines the function to invoke
		/// </summary>
		public VariableFilterExpression Variable { get; }
		
		/// <summary>
		/// Gets the arguments to pass to the function.
		/// </summary>
		public FilterExpression[]? Arguments { get; }
		
		/// <inheritdoc/>
		public override FilterExpressionType ExpressionType => FilterExpressionType.Function;
	}
}
