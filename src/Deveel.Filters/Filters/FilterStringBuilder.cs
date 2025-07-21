using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Filters {
	class FilterStringBuilder : FilterVisitor {
		private readonly StringBuilder builder;

		public FilterStringBuilder(StringBuilder builder) {
			this.builder = builder;
		}

		public override IFilter VisitVariable(IVariableFilter variable) {
			builder.Append(variable.VariableName);
			return Filter.Variable(variable.VariableName);
		}

		public override IFilter VisitConstant(IConstantFilter constant) {
			if (constant.Value == null) {
				builder.Append("null");
			} else if (constant.Value is string s) {
				builder.Append('\"');
				builder.Append(s);
				builder.Append('\"');
			} else if (constant.Value is bool b) {
				if (b) {
					builder.Append("true");
				} else {
					builder.Append("false");
				}
			} else {
				// TODO: format the value
				builder.Append(constant.Value.ToString());
			}

			return Filter.Constant(constant.Value);
		}

		public override IFilter VisitBinary(IBinaryFilter filter) {
			if (filter.Left.IsEmpty() &&
				filter.Right.IsEmpty())
				throw new FilterException("Both left and right operands are empty");

			if (filter.FilterType == FilterType.And ||
				filter.FilterType == FilterType.Or) {
				if (!filter.Left.IsEmpty() && filter.Right.IsEmpty())
					return Visit(filter.Left);
				if (filter.Left.IsEmpty() && !filter.Right.IsEmpty())
					return Visit(filter.Right);
			}

			if (filter.Left.FilterType != FilterType.Constant &&
				filter.Left.FilterType != FilterType.Variable)
				builder.Append('(');

			var left = Visit(filter.Left);

			if (filter.Left.FilterType != FilterType.Constant &&
				filter.Left.FilterType != FilterType.Variable)
				builder.Append(')');

			builder.Append(' ');

			switch (filter.FilterType) {
				case FilterType.Equal:
					builder.Append("==");
					break;
				case FilterType.NotEqual:
					builder.Append("!=");
					break;
				case FilterType.GreaterThan:
					builder.Append(">");
					break;
				case FilterType.GreaterThanOrEqual:
					builder.Append(">=");
					break;
				case FilterType.LessThan:
					builder.Append("<");
					break;
				case FilterType.LessThanOrEqual:
					builder.Append("<=");
					break;
				case FilterType.And:
					builder.Append("&&");
					break;
				case FilterType.Or:
					builder.Append("||");
					break;
			}

			builder.Append(' ');

			if (filter.Right.FilterType != FilterType.Constant &&
				filter.Right.FilterType != FilterType.Variable)
				builder.Append('(');

			var right = Visit(filter.Right);

			if (filter.Right.FilterType != FilterType.Constant &&
				filter.Right.FilterType != FilterType.Variable)
				builder.Append(')');

			return Filter.Binary(left, right, filter.FilterType);
		}

		public override IFilter VisitUnary(IUnaryFilter filter) {
			if (filter.Operand.IsEmpty())
				throw new FilterException("The operand of the unary filter is empty");

			switch (filter.FilterType) {
				case FilterType.Not:
					builder.Append("!");
					break;
			}

			builder.Append("(");

			var operand = Visit(filter.Operand);

			builder.Append(")");

			return Filter.Unary(operand, filter.FilterType);
		}

		public override IList<IFilter> VisitFunctionArguments(IList<IFilter>? arguments) {
			var args = new List<IFilter>(arguments?.Count ?? 0);

			builder.Append('(');

			if (arguments != null) {
				for (int i = 0; i < arguments.Count; i++) {
					args.Add((Filter) Visit(arguments[i]));

					if (i < arguments.Count - 1)
						builder.Append(',').Append(' ');
				}
			}

			builder.Append(')');

			return args;
		}

		public override IFilter VisitFunction(IFunctionFilter filter) {
			var variable = VisitVariable(filter.Variable);

			builder.Append('.');
			builder.Append(filter.FunctionName);
			var args = VisitFunctionArguments(filter.Arguments);

			var arguments = new Filter[args.Count];
			for (int i = 0; i < args.Count; i++) {
				arguments[i] = (Filter) args[i];
			}

			if (!(variable is VariableFilter variableFilter))
				throw new InvalidOperationException($"The variable '{variable}' is not a valid function variable.");

			return Filter.Function(variableFilter, filter.FunctionName, arguments);
		}
	}
}
