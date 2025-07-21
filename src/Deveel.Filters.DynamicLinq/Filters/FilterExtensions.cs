using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;

namespace Deveel.Filters {
	public static class FilterExtensions {
		public static LambdaExpression AsDynamicLamda(this Filter filter, Type parameterType, string parameterName = "x", ParsingConfig? config = null) {
			ArgumentNullException.ThrowIfNull(filter, nameof(filter));
			ArgumentNullException.ThrowIfNull(parameterType, nameof(parameterType));

			var filterString = filter.ToDynamicString();

			try {
				LambdaExpression lambda;
				if (!String.IsNullOrWhiteSpace(parameterName))
				{
					var parameter = Expression.Parameter(parameterType, parameterName);
					lambda = DynamicExpressionParser.ParseLambda(config, new[] { parameter }, typeof(bool), filterString);
				} else
				{
					lambda = DynamicExpressionParser.ParseLambda(config, parameterType, typeof(bool), filterString);
				}

				return lambda;
			} catch (Exception ex) {
				throw new FilterException("Unable to construct the dynamic lamda", ex);
			}
		}

		public static Expression<Func<T, bool>> AsDynamicLambda<T>(this Filter filter, string parameterName = "x", ParsingConfig? config = null) 
			=>	(Expression<Func<T, bool>>)AsDynamicLamda(filter, typeof(T), parameterName, config);

		/// <summary>
		/// Converts a Filter object into a string that is compatible with DynamicLinq parsing.
		/// </summary>
		/// <param name="filter">The filter to convert to a DynamicLinq-compatible string.</param>
		/// <returns>A string representation of the filter that can be parsed by DynamicLinq.</returns>
		public static string ToDynamicString(this Filter filter) {
			var builder = new StringBuilder();
			var visitor = new DynamicLinqFilterStringBuilder(builder);
			visitor.Visit(filter);
			return builder.ToString();
		}
	}

	/// <summary>
	/// A specialized filter visitor that formats filters in a way compatible with DynamicLinq parsing.
	/// </summary>
	internal class DynamicLinqFilterStringBuilder : FilterVisitor {
		private readonly StringBuilder builder;

		public DynamicLinqFilterStringBuilder(StringBuilder builder) {
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
				// Escape quotes in strings for DynamicLinq
				builder.Append('"');
				builder.Append(s.Replace("\"", "\\\""));
				builder.Append('"');
			} else if (constant.Value is bool b) {
				builder.Append(b ? "true" : "false");
			} else if (constant.Value is char c) {
				builder.Append('\'');
				builder.Append(c.ToString().Replace("'", "\\'"));
				builder.Append('\'');
			} else if (constant.Value is DateTime dt) {
				// Format DateTime for DynamicLinq compatibility
				builder.Append("DateTime(");
				builder.Append(dt.Year);
				builder.Append(", ");
				builder.Append(dt.Month);
				builder.Append(", ");
				builder.Append(dt.Day);
				if (dt.TimeOfDay != TimeSpan.Zero) {
					builder.Append(", ");
					builder.Append(dt.Hour);
					builder.Append(", ");
					builder.Append(dt.Minute);
					builder.Append(", ");
					builder.Append(dt.Second);
					if (dt.Millisecond > 0) {
						builder.Append(", ");
						builder.Append(dt.Millisecond);
					}
				}
				builder.Append(")");
			} else if (constant.Value is decimal || constant.Value is double || constant.Value is float) {
				// Use invariant culture for numeric values to ensure proper parsing
				builder.Append(constant.Value.ToString()?.Replace(',', '.'));
			} else {
				builder.Append(constant.Value.ToString());
			}

			return Filter.Constant(constant.Value);
		}

		public override IFilter VisitBinary(IBinaryFilter filter) {
			if (filter.Left.IsEmpty() && filter.Right.IsEmpty())
				throw new FilterException("Both left and right operands are empty");

			if (filter.FilterType == FilterType.And || filter.FilterType == FilterType.Or) {
				if (!filter.Left.IsEmpty() && filter.Right.IsEmpty())
					return Visit(filter.Left);
				if (filter.Left.IsEmpty() && !filter.Right.IsEmpty())
					return Visit(filter.Right);
			}

			// Add parentheses for complex expressions
			bool needsLeftParens = NeedsParentheses(filter.Left);
			bool needsRightParens = NeedsParentheses(filter.Right);

			if (needsLeftParens)
				builder.Append('(');

			var left = Visit(filter.Left);

			if (needsLeftParens)
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

			if (needsRightParens)
				builder.Append('(');

			var right = Visit(filter.Right);

			if (needsRightParens)
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

			bool needsParens = NeedsParentheses(filter.Operand);
			if (needsParens)
				builder.Append("(");

			var operand = Visit(filter.Operand);

			if (needsParens)
				builder.Append(")");

			return Filter.Unary(operand, filter.FilterType);
		}

		public override IList<IFilter> VisitFunctionArguments(IList<IFilter>? arguments) {
			var args = new List<IFilter>(arguments?.Count ?? 0);

			builder.Append('(');

			if (arguments != null) {
				for (int i = 0; i < arguments.Count; i++) {
					args.Add((Filter)Visit(arguments[i]));

					if (i < arguments.Count - 1)
						builder.Append(", ");
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
				arguments[i] = (Filter)args[i];
			}

			if (!(variable is VariableFilter variableFilter))
				throw new InvalidOperationException($"The variable '{variable}' is not a valid function variable.");

			return Filter.Function(variableFilter, filter.FunctionName, arguments);
		}

		private static bool NeedsParentheses(IFilter filter) {
			return filter.FilterType != FilterType.Constant && 
			       filter.FilterType != FilterType.Variable &&
			       filter.FilterType != FilterType.Function;
		}
	}
}