namespace Deveel.Filters {
	public interface IFunctionFilter : IFilter {
		IVariableFilter Variable { get; }

		string FunctionName { get; }

		IList<IFilter> Arguments { get; }
	}
}
