namespace Deveel.Filters {
	public sealed class VariableFilter : Filter {
		internal VariableFilter(string variableName) {
			VariableName = variableName;
		}

		public override FilterType FilterType => FilterType.Variable;

		public string VariableName { get; }
	}
}
