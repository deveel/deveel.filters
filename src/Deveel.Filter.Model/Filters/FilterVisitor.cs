using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	public class FilterVisitor {
		public Filter Visit(Filter filter) {
			switch (filter.FilterType) {
				case FilterType.Equals:
				case FilterType.NotEquals:
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

		public virtual Filter VisitFunction(FunctionFilter filter) {
			var variable = VisitVariable(filter.Variable);
			var arguments = new Filter[filter.Arguments?.Length ?? 0];

			if (filter.Arguments != null) {
				for (int i = 0; i < filter.Arguments.Length; i++) {
					arguments[i] = Visit(filter.Arguments[i]);
				}
			}

			if (!(variable is VariableFilter variableFilter))
				throw new InvalidOperationException($"The variable '{variable}' is not a valid function variable.");

			return Filter.Function(variableFilter, filter.FunctionName, arguments);
		}

		public virtual Filter VisitConstant(ConstantFilter constant) {
			return new ConstantFilter(constant.Value);
		}

		public virtual Filter VisitVariable(VariableFilter variable) {
			return Filter.Variable(variable.VariableName);
		}

		public virtual Filter VisitUnary(UnaryFilter filter) {
			var operand = Visit(filter.Operand);

			return Filter.Unary(operand, filter.FilterType);
		}


		public virtual Filter VisitBinary(BinaryFilter filter) {
			var left = Visit(filter.Left);
			var right = Visit(filter.Right);

			return Filter.Binary(left, right, filter.FilterType);
		}
	}
}
