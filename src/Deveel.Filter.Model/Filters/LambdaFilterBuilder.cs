using System.Linq.Expressions;
using System.Reflection;

namespace Deveel.Filters {
	class LambdaFilterBuilder : FilterVisitor {
		private readonly string parameterName;
		private readonly Type parameterType;
		private readonly ParameterExpression parameter;

		public LambdaFilterBuilder(Type parameterType, string parameterName) {
			this.parameterType = parameterType;
			this.parameterName = parameterName;

			parameter = Expression.Parameter(parameterType, parameterName);
		}

		public LambdaExpression BuildLambda(IFilter filter) {
			var result = Visit(filter);
			if (!(result is FilterExpression filterExp))
				throw new InvalidOperationException("Could not parse the filter");

			var delegateType = typeof(Func<,>).MakeGenericType(parameterType, typeof(bool));

			var body = filterExp.Expression;
			return Expression.Lambda(delegateType, body, parameter);
		}

		public LambdaExpression BuildAsyncLambda(IFilter filter) {
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

		public override IFilter VisitVariable(IVariableFilter variable) {
			var expression = ResolveVariable(variable.VariableName);
			return new FilterExpression(variable.FilterType, expression);
		}

		public override IFilter VisitConstant(IConstantFilter constant) {
			var expression = Expression.Constant(constant.Value);
			return new FilterExpression(constant.FilterType, expression);
		}

		public override IFilter VisitUnary(IUnaryFilter filter) {
			if (filter.Operand.IsEmpty())
				throw new FilterException("The operand of a unary filter cannot be empty");

			var operand = Visit(filter.Operand);
			var operandExp = ((FilterExpression) operand).Expression;
			
			Expression expression;
			switch (filter.FilterType) {
				case FilterType.Not:
					expression = Expression.Not(operandExp);
					break;
				default:
					throw new InvalidOperationException($"The filter of type '{filter.FilterType}' is not a unary");
			}

			return new FilterExpression(filter.FilterType, expression);
		}

		public override IFilter VisitFunction(IFunctionFilter filter) {
			var variable = VisitVariable(filter.Variable);
			var variableExp = ((FilterExpression) variable).Expression;

			var arguments = new Expression[filter.Arguments?.Count ?? 0];
			if (filter.Arguments != null) {
				for (int i = 0; i < filter.Arguments.Count; i++) {
					var arg = Visit(filter.Arguments[i]);
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

			var methodInfo = reflectTye.GetMethod(filter.FunctionName, arguments.Select(x => x.Type).ToArray());

			if (methodInfo == null)
				throw new FilterException($"The method '{filter.FunctionName}' is not found in the type '{reflectTye}'");

			var functionCall = Expression.Call(variableExp, methodInfo,arguments);
			return new FilterExpression(filter.FilterType, functionCall);
		}

		public override IFilter VisitBinary(IBinaryFilter filter) {
			if (filter.Left.IsEmpty() &&
				filter.Right.IsEmpty())
				throw new FilterException("The left and right filter cannot be empty");

			if (filter.FilterType == FilterType.And ||
				filter.FilterType == FilterType.Or) {
				if (filter.Left.IsEmpty() && !filter.Right.IsEmpty())
					return Visit(filter.Right);
				if (!filter.Left.IsEmpty() && filter.Right.IsEmpty())
					return Visit(filter.Left);
			} else if (filter.Left.IsEmpty() || filter.Right.IsEmpty())
				throw new FilterException($"The filter of type '{filter.FilterType}' must have either left or right part not empty");

			var left = Visit(filter.Left);
			var right = Visit(filter.Right);
			var leftExp = ((FilterExpression) left).Expression;
			var rightExp = ((FilterExpression) right).Expression;

			Expression expression;
			switch (filter.FilterType) {
				case FilterType.Equals:
					expression = Expression.Equal(leftExp, rightExp);
					break;
				case FilterType.NotEquals:
					expression = Expression.NotEqual(leftExp, rightExp);
					break;
				case FilterType.GreaterThan:
					expression = Expression.GreaterThan(leftExp, rightExp);
					break;
				case FilterType.GreaterThanOrEqual:
					expression = Expression.GreaterThanOrEqual(leftExp, rightExp);
					break;
				case FilterType.LessThan:
					expression = Expression.LessThan(leftExp, rightExp);
					break;
				case FilterType.LessThanOrEqual:
					expression = Expression.LessThanOrEqual(leftExp, rightExp);
					break;
				case FilterType.And:
					expression = Expression.AndAlso(leftExp, rightExp);
					break;
				case FilterType.Or:
					expression = Expression.OrElse(leftExp, rightExp);
					break;
				default:
					throw new FilterException($"The filter of type '{filter.FilterType}' is not a binary");
			}

			return new FilterExpression(filter.FilterType, expression);
		}

		#region FilterExpression

		class FilterExpression : IFilter {
			public FilterExpression(FilterType filterType, Expression expression) {
				FilterType = filterType;
				Expression = expression;
			}

			public FilterType FilterType { get; }

			public Expression Expression { get; }
		}

		#endregion
	}
}
