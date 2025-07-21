using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	class FilterConverter : FilterVisitor {
		public override IFilter VisitConstant(IConstantFilter constant) {
			if (constant is ConstantFilter filter)
				return constant;

			return Filter.Constant(constant.Value);
		}

		public override IFilter VisitVariable(IVariableFilter variable) {
			if (variable is VariableFilter filter)
				return variable;

			return Filter.Variable(variable.VariableName);
		}

		public override IList<IFilter> VisitFunctionArguments(IList<IFilter>? arguments) {
			if (arguments == null)
				return new IFilter[0];

			var list = new List<IFilter>(arguments.Count);
			foreach (var argument in arguments) {
				list.Add(Visit(argument));
			}
			return list;
		}

		public override IFilter VisitFunction(IFunctionFilter filter) {
			var variable = VisitVariable(filter.Variable);
			var arguments = VisitFunctionArguments(filter.Arguments);

			var args = new Filter[arguments.Count];
			for (var i = 0; i < arguments.Count; i++) {
				args[i] = (Filter)arguments[i];
			}

			return Filter.Function((VariableFilter) variable, filter.FunctionName, args);
		}

		public override IFilter VisitUnary(IUnaryFilter filter) {
			if (filter is UnaryFilter unaryFilter)
				return unaryFilter;

			var operand = Visit(filter.Operand);

			return Filter.Unary((Filter) operand, filter.FilterType);
		}

		public override IFilter VisitBinary(IBinaryFilter filter) {
			if (filter is BinaryFilter binaryFilter)
				return binaryFilter;

			var left = Visit(filter.Left);
			var right = Visit(filter.Right);
			return Filter.Binary((Filter) left, (Filter) right, filter.FilterType);
		}
	}
}
