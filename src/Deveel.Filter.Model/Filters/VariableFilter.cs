using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public sealed class VariableFilter : Filter, IVariableFilter {
		internal VariableFilter(string variableName) {
			if (string.IsNullOrWhiteSpace(variableName))
				throw new ArgumentException($"'{nameof(variableName)}' cannot be null or whitespace.", nameof(variableName));

			VariableName = variableName;
		}

		public override FilterType FilterType => FilterType.Variable;

		public string VariableName { get; }
	}
}
