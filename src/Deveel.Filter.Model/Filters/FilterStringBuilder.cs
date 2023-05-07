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

		public override Filter VisitVariable(VariableFilter variable) {
			builder.Append(variable.VariableName);
			return base.VisitVariable(variable);
		}

		public override Filter VisitConstant(ConstantFilter constant) {
			if (constant.Value == null) {
				builder.Append("null");
			} else if (constant.Value is string s) {
				builder.Append('\"');
				builder.Append(s);
				builder.Append('\"');
			} else if (constant.Value is char c) {
				builder.Append('\'');
				builder.Append(c);
				builder.Append('\'');
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

			return base.VisitConstant(constant);
		}

		public override Filter VisitBinary(BinaryFilter filter) {
			if (filter.Left.FilterType != FilterType.Constant &&
				filter.Left.FilterType != FilterType.Variable)
				builder.Append('(');

			var left = Visit(filter.Left);

			if (filter.Left.FilterType != FilterType.Constant &&
				filter.Left.FilterType != FilterType.Variable)
				builder.Append(')');

			builder.Append(' ');

			switch (filter.FilterType) {
				case FilterType.Equals:
					builder.Append("==");
					break;
				case FilterType.NotEquals:
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

		public override Filter VisitUnary(UnaryFilter filter) {
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

		public override Filter VisitFunction(FunctionFilter filter) {
			var variable = VisitVariable(filter.Variable);

			builder.Append('.');
			builder.Append(filter.FunctionName);
			builder.Append('(');

			var arguments = new Filter[filter.Arguments?.Length ?? 0];
			if (filter.Arguments != null) {
				for (int i = 0; i < filter.Arguments.Length; i++) {
					arguments[i] = Visit(filter.Arguments[i]);

					if (i < filter.Arguments.Length - 1)
						builder.Append(',').Append(' ');
				}
			}

			builder.Append(')');

			if (!(variable is VariableFilter variableFilter))
				throw new InvalidOperationException($"The variable '{variable}' is not a valid function variable.");

			return Filter.Function(variableFilter, filter.FunctionName, arguments);
		}
	}
}
