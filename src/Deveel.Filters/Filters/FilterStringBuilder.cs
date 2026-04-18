// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace Deveel.Filters {
	class FilterStringBuilder : FilterExpressionVisitor {
		private readonly StringBuilder builder;

		public FilterStringBuilder(StringBuilder builder) {
			this.builder = builder;
		}

		public override FilterExpression VisitVariable(VariableFilterExpression variable) {
			builder.Append(variable.VariableName);
			return FilterExpression.Variable(variable.VariableName);
		}

		public override FilterExpression VisitConstant(ConstantFilterExpression constant) {
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

			return FilterExpression.Constant(constant.Value);
		}

		public override FilterExpression VisitBinary(BinaryFilterExpression filterExpression) {
			if (filterExpression.Left.IsEmpty &&
				filterExpression.Right.IsEmpty)
				throw new FilterException("Both left and right operands are empty");

			if (filterExpression.ExpressionType == FilterExpressionType.And ||
				filterExpression.ExpressionType == FilterExpressionType.Or) {
				if (!filterExpression.Left.IsEmpty && filterExpression.Right.IsEmpty)
					return Visit(filterExpression.Left);
				if (filterExpression.Left.IsEmpty && !filterExpression.Right.IsEmpty)
					return Visit(filterExpression.Right);
			}

			if (filterExpression.Left.ExpressionType != FilterExpressionType.Constant &&
				filterExpression.Left.ExpressionType != FilterExpressionType.Variable)
				builder.Append('(');

			var left = Visit(filterExpression.Left);

			if (filterExpression.Left.ExpressionType != FilterExpressionType.Constant &&
				filterExpression.Left.ExpressionType != FilterExpressionType.Variable)
				builder.Append(')');

			builder.Append(' ');

			switch (filterExpression.ExpressionType) {
				case FilterExpressionType.Equal:
					builder.Append("==");
					break;
				case FilterExpressionType.NotEqual:
					builder.Append("!=");
					break;
				case FilterExpressionType.GreaterThan:
					builder.Append(">");
					break;
				case FilterExpressionType.GreaterThanOrEqual:
					builder.Append(">=");
					break;
				case FilterExpressionType.LessThan:
					builder.Append("<");
					break;
				case FilterExpressionType.LessThanOrEqual:
					builder.Append("<=");
					break;
				case FilterExpressionType.And:
					builder.Append("&&");
					break;
				case FilterExpressionType.Or:
					builder.Append("||");
					break;
			}

			builder.Append(' ');

			if (filterExpression.Right.ExpressionType != FilterExpressionType.Constant &&
				filterExpression.Right.ExpressionType != FilterExpressionType.Variable)
				builder.Append('(');

			var right = Visit(filterExpression.Right);

			if (filterExpression.Right.ExpressionType != FilterExpressionType.Constant &&
				filterExpression.Right.ExpressionType != FilterExpressionType.Variable)
				builder.Append(')');

			return FilterExpression.Binary(left, right, filterExpression.ExpressionType);
		}

		public override FilterExpression VisitUnary(UnaryFilterExpression filterExpression) {
			if (filterExpression.Operand.IsEmpty)
				throw new FilterException("The operand of the unary filter is empty");

			switch (filterExpression.ExpressionType) {
				case FilterExpressionType.Not:
					builder.Append("!");
					break;
			}

			builder.Append("(");

			var operand = Visit(filterExpression.Operand);

			builder.Append(")");

			return FilterExpression.Unary(operand, filterExpression.ExpressionType);
		}

		public override IList<FilterExpression> VisitFunctionArguments(IList<FilterExpression>? arguments) {
			var args = new List<FilterExpression>(arguments?.Count ?? 0);

			builder.Append('(');

			if (arguments != null) {
				for (int i = 0; i < arguments.Count; i++) {
					args.Add(Visit(arguments[i]));

					if (i < arguments.Count - 1)
						builder.Append(',').Append(' ');
				}
			}

			builder.Append(')');

			return args;
		}

		public override FilterExpression VisitFunction(FunctionFilterExpression filterExpression) {
			var variable = VisitVariable(filterExpression.Variable);

			builder.Append('.');
			builder.Append(filterExpression.FunctionName);
			var args = VisitFunctionArguments(filterExpression.Arguments);

			var arguments = new FilterExpression[args.Count];
			for (int i = 0; i < args.Count; i++) {
				arguments[i] = args[i];
			}

			if (!(variable is VariableFilterExpression variableFilter))
				throw new InvalidOperationException($"The variable '{variable}' is not a valid function variable.");

			return FilterExpression.Function(variableFilter, filterExpression.FunctionName, arguments);
		}
	}
}
