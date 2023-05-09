namespace Deveel.Filters {
    public class FilterVisitor : IFilterVisitor {
		public IFilter Visit(IFilter filter) {
			if (filter.IsEmpty())
				return Filter.Empty;

			switch (filter.FilterType) {
				case FilterType.Equal:
				case FilterType.NotEqual:
				case FilterType.GreaterThan:
				case FilterType.GreaterThanOrEqual:
				case FilterType.LessThan:
				case FilterType.LessThanOrEqual:
				case FilterType.And:
				case FilterType.Or:
					return VisitBinary((IBinaryFilter) filter);
				case FilterType.Not:
					return VisitUnary((IUnaryFilter) filter);
				case FilterType.Function:
					return VisitFunction((IFunctionFilter)filter);
				case FilterType.Constant:
					return VisitConstant((IConstantFilter) filter);
				case FilterType.Variable:
					return VisitVariable((IVariableFilter)filter);
			}

			throw new NotSupportedException($"The filter type '{filter.FilterType}' is not supported.");
		}

		public virtual IList<IFilter> VisitFunctionArguments(IList<IFilter>? arguments) {
			var list = new List<IFilter>(arguments?.Count ?? 0);
			if (arguments != null) {
				foreach (var argument in arguments) {
					list.Add(Visit(argument));
				}
			}

			return list;
		}

		public virtual IFilter VisitFunction(IFunctionFilter filter) {
			var variable = VisitVariable(filter.Variable);
			var arguments = VisitFunctionArguments(filter.Arguments);

			var args = new Filter[arguments.Count];
			for (var i = 0; i < arguments.Count; i++) {
				args[i] = (Filter)arguments[i];
			}

			if (!(variable is VariableFilter variableFilter))
				throw new InvalidOperationException($"The variable '{variable}' is not a valid function variable.");

			return Filter.Function(variableFilter, filter.FunctionName, args);
		}

		public virtual IFilter VisitConstant(IConstantFilter constant) {
			return new ConstantFilter(constant.Value);
		}

		public virtual IFilter VisitVariable(IVariableFilter variable) {
			return Filter.Variable(variable.VariableName);
		}

		public virtual IFilter VisitUnary(IUnaryFilter filter) {
			var operand = (Filter) Visit(filter.Operand);

			return Filter.Unary(operand, filter.FilterType);
		}

		public virtual IFilter VisitBinary(IBinaryFilter filter) {
			var left = (Filter) Visit(filter.Left);
			var right = (Filter) Visit(filter.Right);

			return Filter.Binary(left, right, filter.FilterType);
		}
	}
}
