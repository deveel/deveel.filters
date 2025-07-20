using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Deveel.Filters
{
	/// <summary>
	/// Provides functionality to parse string expressions into Filter objects using DynamicExpressionParser.
	/// </summary>
	public static class FilterExpressionParser
	{
		public static readonly ParsingConfig DefaultParsingConfig = new ParsingConfig
		{
			// Default configuration can be customized here if needed
			// e.g., CustomTypeProvider, etc.
		};

		/// <summary>
		/// Parses a string expression into a Filter object.
		/// </summary>
		/// <param name="expression">The string expression to parse (e.g., "Name == \"John\" && Age > 25")</param>
		/// <param name="parameterType">The type of the parameter in the expression</param>
		/// <param name="parameterName">The name of the parameter (default: "x")</param>
		/// <param name="config">Optional parsing configuration</param>
		/// <returns>A Filter object representing the parsed expression</returns>
		/// <exception cref="ArgumentException">Thrown when the expression is null or empty</exception>
		/// <exception cref="FilterException">Thrown when the expression cannot be parsed or converted</exception>
		public static Filter Parse(string expression, Type parameterType, string parameterName = "x", ParsingConfig? config = null)
		{
			if (string.IsNullOrWhiteSpace(expression))
				throw new ArgumentException("Expression cannot be null or empty", nameof(expression));

			try
			{
				// Parse the string expression into a LambdaExpression using DynamicExpressionParser
				var parameter = Expression.Parameter(parameterType, parameterName);
				var lambda = DynamicExpressionParser.ParseLambda(config, new[] { parameter }, typeof(bool), expression, parameterName);
				
				// Navigate the expression tree and convert to Filter
				var visitor = new ExpressionToFilterVisitor(parameterName);
				return visitor.ConvertToFilter(lambda.Body);
			}
			catch (Exception ex) when (!(ex is FilterException))
			{
				throw new FilterException($"Unable to parse expression '{expression}' into a Filter", ex);
			}
		}

		/// <summary>
		/// Parses a string expression into a Filter object with a typed parameter.
		/// </summary>
		/// <typeparam name="T">The type of the parameter in the expression</typeparam>
		/// <param name="expression">The string expression to parse</param>
		/// <param name="parameterName">The name of the parameter (default: "x")</param>
		/// <param name="config">Optional parsing configuration</param>
		/// <returns>A Filter object representing the parsed expression</returns>
		public static Filter Parse<T>(string expression, string parameterName = "x", ParsingConfig? config = null)
		{
			return Parse(expression, typeof(T), parameterName, config);
		}
	}

	/// <summary>
	/// Visitor class that navigates Expression trees and converts them to Filter objects.
	/// </summary>
	internal class ExpressionToFilterVisitor
	{
		private readonly string _parameterName;

		public ExpressionToFilterVisitor(string parameterName)
		{
			_parameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
		}

		/// <summary>
		/// Converts an Expression to a Filter object.
		/// </summary>
		public Filter ConvertToFilter(Expression expression)
		{
			return expression switch
			{
				BinaryExpression binary => ConvertBinaryExpression(binary),
				UnaryExpression unary => ConvertUnaryExpression(unary),
				ConstantExpression constant => ConvertConstantExpression(constant),
				MemberExpression member => ConvertMemberExpression(member),
				ParameterExpression parameter => ConvertParameterExpression(parameter),
				MethodCallExpression methodCall => ConvertMethodCallExpression(methodCall),
				NewExpression newExpr => ConvertNewExpression(newExpr),
				_ => throw new FilterException($"Unsupported expression type: {expression.GetType().Name}")
			};
		}

		private Filter ConvertNewExpression(NewExpression newExpr)
		{
			// check if any of the arguments are not constant or variable
			if (newExpr.Arguments.Any(arg => !(arg is ConstantExpression)))
				throw new FilterException("New expressions must only contain constant expressions");
			if (newExpr.Constructor == null)
				throw new FilterException("New expression must have a valid constructor");

			// Create a variable filter for the new expression
			var args = newExpr.Arguments
				.OfType<ConstantExpression>()
				.Select(arg => arg.Value)
				.ToArray();

			// var obj = newExpr.Constructor.Invoke(null, args);
			var obj = Activator.CreateInstance(newExpr.Type, args);
			return Filter.Constant(obj);
		}

		private Filter ConvertBinaryExpression(BinaryExpression binary)
		{
			var left = ConvertToFilter(binary.Left);
			var right = ConvertToFilter(binary.Right);

			var filterType = binary.NodeType switch
			{
				ExpressionType.Equal => FilterType.Equal,
				ExpressionType.NotEqual => FilterType.NotEqual,
				ExpressionType.GreaterThan => FilterType.GreaterThan,
				ExpressionType.GreaterThanOrEqual => FilterType.GreaterThanOrEqual,
				ExpressionType.LessThan => FilterType.LessThan,
				ExpressionType.LessThanOrEqual => FilterType.LessThanOrEqual,
				ExpressionType.AndAlso => FilterType.And,
				ExpressionType.OrElse => FilterType.Or,
				_ => throw new FilterException($"Unsupported binary expression type: {binary.NodeType}")
			};

			return Filter.Binary(left, right, filterType);
		}

		private Filter ConvertUnaryExpression(UnaryExpression unary)
		{
			var operand = ConvertToFilter(unary.Operand);

			var filterType = unary.NodeType switch
			{
				ExpressionType.Not => FilterType.Not,
				_ => throw new FilterException($"Unsupported unary expression type: {unary.NodeType}")
			};

			return Filter.Unary(operand, filterType);
		}

		private Filter ConvertConstantExpression(ConstantExpression constant)
		{
			return Filter.Constant(constant.Value);
		}

		private Filter ConvertMemberExpression(MemberExpression member)
		{
			// Build the variable name by traversing the member access chain
			var variableName = BuildVariableName(member);
			return Filter.Variable(variableName);
		}

		private Filter ConvertParameterExpression(ParameterExpression parameter)
		{
			return Filter.Variable(parameter.Name ?? _parameterName);
		}

		private Filter ConvertMethodCallExpression(MethodCallExpression methodCall)
		{
			// Handle method calls as function filters
			if (methodCall.Object != null)
			{
				var variable = ConvertToFilter(methodCall.Object);
				if (variable is not VariableFilter variableFilter)
					throw new FilterException("Function calls must be made on variable expressions");

				var arguments = methodCall.Arguments.Select(ConvertToFilter).ToArray();
				return Filter.Function(variableFilter, methodCall.Method.Name, arguments);
			}

			throw new FilterException("Static method calls are not supported");
		}

		private string BuildVariableName(MemberExpression member)
		{
			var parts = new List<string>();
			Expression current = member;

			while (current != null)
			{
				switch (current)
				{
					case MemberExpression memberExpr:
						parts.Insert(0, memberExpr.Member.Name);
						current = memberExpr.Expression;
						break;
					case ParameterExpression paramExpr:
						// Don't include the parameter name if it matches our expected parameter name
						if (!String.IsNullOrWhiteSpace(paramExpr.Name) && !String.Equals(paramExpr.Name, _parameterName, StringComparison.Ordinal))
							parts.Insert(0, paramExpr.Name ?? _parameterName);
						current = null;
						break;
					default:
						current = null;
						break;
				}
			}

			if (parts.Count == 0)
				throw new FilterException("Unable to build variable name from member expression");

			return string.Join(".", parts);
		}
	}
}
