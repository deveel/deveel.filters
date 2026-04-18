// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;

namespace Deveel.Filters {
	/// <summary>
	/// Provides extension methods for converting Filter objects into dynamic LINQ expressions.
	/// </summary>
	public static class FilterExtensions {
		/// <summary>
		/// Converts a Filter object into a LambdaExpression that can be used with DynamicLinq for filtering.
		/// </summary>
		/// <param name="filter">
		/// The filter to convert into a LambdaExpression. This filter will be converted into a string representation
		/// that is compatible with DynamicLinq parsing, and then parsed back into a LambdaExpression.
		/// </param>
		/// <param name="parameterType">
		/// The type of the parameter that will be used in the LambdaExpression. This is typically the type of the entity
		/// being filtered. The parameter name in the expression will be determined by the <paramref name="parameterName"/> argument.
		/// </param>
		/// <param name="parameterName">
		/// The name of the parameter to be used in the LambdaExpression. This is the name that will be used in the string representation
		/// of the filter when it is converted for DynamicLinq parsing. If this parameter is null or whitespace, the parameter will be defined
		/// without a name in the LambdaExpression, and the string representation of the filter will not include a parameter name.
		/// This can be useful in cases where the parameter name is not relevant or should be inferred from context.
		/// </param>
		/// <param name="config">
	/// An optional <see cref="ParsingConfig"/> to configure the DynamicLinq parser behavior.
	/// </param>
		/// <returns>
		/// Returns a <see cref="LambdaExpression"/> that represents the filter, which can be used to filter an
		/// <see cref="IQueryable"/> using DynamicLinq. The resulting LambdaExpression will have a single parameter
		/// of the specified type, and its body will represent the logic defined in the original Filter object.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if either <paramref name="filter"/> or <paramref name="parameterType"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="FilterException">
		/// Thrown if the filter cannot be converted into a valid LambdaExpression, which can occur if the filter contains
		/// unsupported operations or if the resulting string representation is not compatible with DynamicLinq parsing.
		/// </exception>
		public static LambdaExpression AsDynamicLambda(this FilterExpression filter, Type parameterType, string parameterName = "x", ParsingConfig? config = null) {
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

		/// <summary>
		/// Converts a Filter object into a strongly-typed <see cref="Expression{TDelegate}"/>
		/// that can be used with DynamicLinq for filtering.
		/// </summary>
		/// <typeparam name="T">The type of the parameter in the expression.</typeparam>
		/// <param name="filter">The filter to convert.</param>
		/// <param name="parameterName">The name of the parameter (default: "x").</param>
		/// <param name="config">Optional parsing configuration.</param>
		/// <returns>
		/// Returns an <see cref="Expression{TDelegate}"/> representing the filter.
		/// </returns>
		public static Expression<Func<T, bool>> AsDynamicLambda<T>(this FilterExpression filter, string parameterName = "x", ParsingConfig? config = null) 
			=>	(Expression<Func<T, bool>>)AsDynamicLambda(filter, typeof(T), parameterName, config);

		/// <summary>
		/// Converts a Filter object into a string that is compatible with DynamicLinq parsing.
		/// </summary>
		/// <param name="filter">The filter to convert to a DynamicLinq-compatible string.</param>
		/// <returns>A string representation of the filter that can be parsed by DynamicLinq.</returns>
		public static string ToDynamicString(this FilterExpression filter) {
			var builder = new StringBuilder();
			var visitor = new DynamicLinqFilterStringBuilder(builder);
			visitor.Visit(filter);
			return builder.ToString();
		}
	}

	/// <summary>
	/// A specialized filter visitor that formats filters in a way compatible with DynamicLinq parsing.
	/// </summary>
	internal class DynamicLinqFilterStringBuilder : FilterExpressionVisitor {
		private readonly StringBuilder builder;

		public DynamicLinqFilterStringBuilder(StringBuilder builder) {
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

			return FilterExpression.Constant(constant.Value);
		}

		public override FilterExpression VisitBinary(BinaryFilterExpression filterExpression) {
			if (filterExpression.Left.IsEmpty && filterExpression.Right.IsEmpty)
				throw new FilterException("Both left and right operands are empty");

			if (filterExpression.ExpressionType == FilterExpressionType.And || filterExpression.ExpressionType == FilterExpressionType.Or) {
				if (!filterExpression.Left.IsEmpty && filterExpression.Right.IsEmpty)
					return Visit(filterExpression.Left);
				if (filterExpression.Left.IsEmpty && !filterExpression.Right.IsEmpty)
					return Visit(filterExpression.Right);
			}

			// Add parentheses for complex expressions
			bool needsLeftParens = NeedsParentheses(filterExpression.Left);
			bool needsRightParens = NeedsParentheses(filterExpression.Right);

			if (needsLeftParens)
				builder.Append('(');

			var left = Visit(filterExpression.Left);

			if (needsLeftParens)
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

			if (needsRightParens)
				builder.Append('(');

			var right = Visit(filterExpression.Right);

			if (needsRightParens)
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

			bool needsParens = NeedsParentheses(filterExpression.Operand);
			if (needsParens)
				builder.Append("(");

			var operand = Visit(filterExpression.Operand);

			if (needsParens)
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
						builder.Append(", ");
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

		private static bool NeedsParentheses(FilterExpression filter) {
			return filter.ExpressionType != FilterExpressionType.Constant && 
			       filter.ExpressionType != FilterExpressionType.Variable &&
			       filter.ExpressionType != FilterExpressionType.Function;
		}
	}
}