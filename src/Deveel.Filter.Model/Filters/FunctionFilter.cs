using System;

namespace Deveel.Filters {
	/// <summary>
	/// The invocation to a function that returns a
	/// boolean for filtering
	/// </summary>
	public sealed class FunctionFilter : Filter {
		internal FunctionFilter(VariableFilter variable, string functionName, Filter[]? arguments) {
			if (string.IsNullOrEmpty(functionName)) 
				throw new ArgumentException($"'{nameof(functionName)}' cannot be null or empty.", nameof(functionName));

			if (arguments != null) {
				if (arguments.Any(x => x.FilterType != FilterType.Constant && x.FilterType != FilterType.Variable))
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
		public VariableFilter Variable { get; }

		/// <summary>
		/// Gets the arguments to pass to the function.
		/// </summary>
		public Filter[]? Arguments { get; }

		/// <inheritdoc/>
		public override FilterType FilterType => FilterType.Function;
	}
}
