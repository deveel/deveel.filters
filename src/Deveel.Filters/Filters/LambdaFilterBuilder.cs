// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Reflection;

namespace Deveel.Filters {
	class LambdaFilterBuilder : FilterExpressionVisitor {
		private readonly string parameterName;
		private readonly Type parameterType;
		private readonly ParameterExpression parameter;

		public LambdaFilterBuilder(Type parameterType, string parameterName) {
			this.parameterType = parameterType;
			this.parameterName = parameterName;

			parameter = Expression.Parameter(parameterType, parameterName);
		}

		public LambdaExpression BuildLambda(Filters.FilterExpression filter) {
			var result = Visit(filter);
			if (!(result is FilterExpression filterExp))
				throw new InvalidOperationException("Could not parse the filter");

			var delegateType = typeof(Func<,>).MakeGenericType(parameterType, typeof(bool));

			var body = filterExp.Expression;
			return Expression.Lambda(delegateType, body, parameter);
		}

		public LambdaExpression BuildAsyncLambda(Filters.FilterExpression filter) {
			var result = Visit(filter);
			if (!(result is FilterExpression filterExp))
				throw new InvalidOperationException("Could not parse the filter");


			var labelTarget = Expression.Label(typeof(Task<bool>));
			var resultExpression = Expression.Call(typeof(Task), nameof(Task.FromResult), new Type[] { typeof(bool) }, filterExp.Expression);
			var returnExpression = Expression.Return(labelTarget, resultExpression, typeof(Task<bool>));
			var blockExpression = Expression.Block(returnExpression, Expression.Label(labelTarget, Expression.Constant(Task.FromResult(false))));
			var delegateType = typeof(Func<,>).MakeGenericType(parameterType, typeof(Task<bool>));
			return Expression.Lambda(delegateType, blockExpression, parameter);
		}

		private Expression ResolveVariable(string variableName) {
			if (variableName == parameterName)
				return parameter;

			Expression instance = parameter;
			var declareType = parameter.Type;
			var parts = variableName.Split('.');
			foreach (var part in parts) {
				if (part == parameterName)
					continue;

				var members = declareType.GetMember(part, BindingFlags.Public | BindingFlags.Instance);
				if (members == null || members.Length == 0)
					throw new FilterException($"Unable to find the member {part} in the type '{declareType}'");
				if (members.Length > 1)
					throw new FilterException($"Ambiguous reference to the member '{part}' in the type '{declareType}'");

				if (members[0] is PropertyInfo property) {
					declareType = property.PropertyType;
				} else if (members[0] is FieldInfo field) {
					declareType = field.FieldType;
				}

				instance = Expression.MakeMemberAccess(instance, members[0]);
			}

			return instance;
		}

		public override Filters.FilterExpression VisitVariable(VariableFilterExpression variable) {
			var expression = ResolveVariable(variable.VariableName);
			return new FilterExpression(variable.ExpressionType, expression);
		}

		public override Filters.FilterExpression VisitConstant(ConstantFilterExpression constant) {
			var expression = Expression.Constant(constant.Value);
			return new FilterExpression(constant.ExpressionType, expression);
		}

		public override Filters.FilterExpression VisitUnary(UnaryFilterExpression filterExpression) {
			if (filterExpression.Operand.IsEmpty)
				throw new FilterException("The operand of a unary filter cannot be empty");

			var operand = Visit(filterExpression.Operand);
			var operandExp = ((FilterExpression) operand).Expression;
			
			Expression expression;
			switch (filterExpression.ExpressionType) {
				case FilterExpressionType.Not:
					expression = Expression.Not(operandExp);
					break;
				default:
					throw new InvalidOperationException($"The filter of type '{filterExpression.ExpressionType}' is not a unary");
			}

			return new FilterExpression(filterExpression.ExpressionType, expression);
		}

		public override Filters.FilterExpression VisitFunction(FunctionFilterExpression filterExpression) {
			var variable = VisitVariable(filterExpression.Variable);
			var variableExp = ((FilterExpression) variable).Expression;

			var arguments = new Expression[filterExpression.Arguments?.Length ?? 0];
			if (filterExpression.Arguments != null) {
				for (int i = 0; i < filterExpression.Arguments.Length; i++) {
					var arg = Visit(filterExpression.Arguments[i]);
					arguments[i] = ((FilterExpression) arg).Expression;
				}
			}

			Type reflectTye;
			if (variableExp is ParameterExpression param) {
				reflectTye = param.Type;
			} else if (variableExp is MemberExpression member) {
				reflectTye = member.Type;
			} else {
				throw new NotSupportedException($"The variable expression '{variableExp}' is not supported");
			}

			var methodInfo = reflectTye.GetMethod(filterExpression.FunctionName, arguments.Select(x => x.Type).ToArray());

			if (methodInfo == null)
				throw new FilterException($"The method '{filterExpression.FunctionName}' is not found in the type '{reflectTye}'");

			var functionCall = Expression.Call(variableExp, methodInfo,arguments);
			return new FilterExpression(filterExpression.ExpressionType, functionCall);
		}

		public override Filters.FilterExpression VisitBinary(BinaryFilterExpression filterExpression) {
			if (filterExpression.Left.IsEmpty &&
				filterExpression.Right.IsEmpty)
				throw new FilterException("The left and right filter cannot be empty");

			if (filterExpression.ExpressionType == FilterExpressionType.And ||
				filterExpression.ExpressionType == FilterExpressionType.Or) {
				if (filterExpression.Left.IsEmpty && !filterExpression.Right.IsEmpty)
					return Visit(filterExpression.Right);
				if (!filterExpression.Left.IsEmpty && filterExpression.Right.IsEmpty)
					return Visit(filterExpression.Left);
			} else if (filterExpression.Left.IsEmpty || filterExpression.Right.IsEmpty)
				throw new FilterException($"The filter of type '{filterExpression.ExpressionType}' must have either left or right part not empty");

			var left = Visit(filterExpression.Left);
			var right = Visit(filterExpression.Right);
			var leftExp = ((FilterExpression) left).Expression;
			var rightExp = ((FilterExpression) right).Expression;

			Expression expression;
			switch (filterExpression.ExpressionType) {
				case FilterExpressionType.Equal:
					expression = Expression.Equal(leftExp, rightExp);
					break;
				case FilterExpressionType.NotEqual:
					expression = Expression.NotEqual(leftExp, rightExp);
					break;
				case FilterExpressionType.GreaterThan:
					expression = Expression.GreaterThan(leftExp, rightExp);
					break;
				case FilterExpressionType.GreaterThanOrEqual:
					expression = Expression.GreaterThanOrEqual(leftExp, rightExp);
					break;
				case FilterExpressionType.LessThan:
					expression = Expression.LessThan(leftExp, rightExp);
					break;
				case FilterExpressionType.LessThanOrEqual:
					expression = Expression.LessThanOrEqual(leftExp, rightExp);
					break;
				case FilterExpressionType.And:
					expression = Expression.AndAlso(leftExp, rightExp);
					break;
				case FilterExpressionType.Or:
					expression = Expression.OrElse(leftExp, rightExp);
					break;
				default:
					throw new FilterException($"The filter of type '{filterExpression.ExpressionType}' is not a binary");
			}

			return new FilterExpression(filterExpression.ExpressionType, expression);
		}

		#region FilterExpression

		class FilterExpression : Filters.FilterExpression {
			public FilterExpression(FilterExpressionType expressionType, Expression expression) {
				ExpressionType = expressionType;
				Expression = expression;
			}

			public override FilterExpressionType ExpressionType { get; }

			public Expression Expression { get; }
		}

		#endregion
	}
}
