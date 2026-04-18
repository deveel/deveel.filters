namespace Deveel.Filters {
    public class FilterVisitor : IFilterVisitor {
		public Filter Visit(Filter filter) {
			if (filter.IsEmpty)
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
					return VisitBinary((BinaryFilter) filter);
				case FilterType.Not:
					return VisitUnary((UnaryFilter) filter);
				case FilterType.Function:
					return VisitFunction((FunctionFilter)filter);
				case FilterType.Constant:
					return VisitConstant((ConstantFilter) filter);
				case FilterType.Variable:
					return VisitVariable((VariableFilter)filter);
			}

			throw new NotSupportedException($"The filter type '{filter.FilterType}' is not supported.");
		}

		public virtual IList<Filter> VisitFunctionArguments(IList<Filter>? arguments) {
			var list = new List<Filter>(arguments?.Count ?? 0);
			if (arguments != null) {
				foreach (var argument in arguments) {
					list.Add(Visit(argument));
				}
			}

			return list;
		}

		public virtual Filter VisitFunction(FunctionFilter filter) {
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

		public virtual Filter VisitConstant(ConstantFilter constant) {
			return new ConstantFilter(constant.Value);
		}

		public virtual Filter VisitVariable(VariableFilter variable) {
			return Filter.Variable(variable.VariableName);
		}

		public virtual Filter VisitUnary(UnaryFilter filter) {
			var operand = (Filter) Visit(filter.Operand);

			return Filter.Unary(operand, filter.FilterType);
		}

		public virtual Filter VisitBinary(BinaryFilter filter) {
			var left = (Filter) Visit(filter.Left);
			var right = (Filter) Visit(filter.Right);

			return Filter.Binary(left, right, filter.FilterType);
		}
	}
}
