using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public sealed class VariableFilter : Filter, IVariableFilter {
		internal VariableFilter(string variableName) {
			VariableName = variableName;
		}

		public override FilterType FilterType => FilterType.Variable;

		public string VariableName { get; }
	}
}
