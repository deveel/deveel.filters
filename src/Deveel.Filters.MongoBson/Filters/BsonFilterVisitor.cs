// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Net;

using MongoDB.Bson;

namespace Deveel.Filters {
	class BsonFilterVisitor : FilterExpressionVisitor {
		internal BsonDocument BuildDocument(FilterExpression filter) {
			var bsonFilter = Visit(filter);

			if (!(bsonFilter is BsonFilter bson))
				throw new FilterException("Invalid filter type");

			if (!(bson.Value is BsonDocument document))
				throw new FilterException("Invalid filter value");

			return document;
		}

		public override FilterExpression VisitConstant(ConstantFilterExpression constant) {
			var value = constant.Value;
			var valueType = value?.GetType() ?? typeof(DBNull);

			BsonValue bsonValue;

			if (value == null) {
				bsonValue = BsonNull.Value;
			} else if (value is DateTimeOffset date2) {
				bsonValue = BsonFilterUtil.CreateBsonDateTime2(date2);
			} else {
				bsonValue = BsonValue.Create(value);
			}

			var bson = new BsonDocument {
				{ "type", FilterTypeString(constant.ExpressionType) },
				{ "valueType", BsonValue.Create(GetValueTypeString(valueType)) },
				{ "value", bsonValue }
			};

			return new BsonFilter(FilterExpressionType.Constant, bson);
		}

		private static string GetValueTypeString(Type valueType) {
			return BsonFilterUtil.GetValueTypeString(valueType);
		}

		public override FilterExpression VisitVariable(VariableFilterExpression variable) {
			var varRef = new BsonDocument {
				{ "type", FilterTypeString(variable.ExpressionType) },
				{ "varRef", variable.VariableName }
			};

			return new BsonFilter(FilterExpressionType.Variable, varRef);
		}

		private static string FilterTypeString(FilterExpressionType expressionType)
			=> expressionType.ToString().ToLowerInvariant();

		public override FilterExpression VisitUnary(UnaryFilterExpression filterExpression) {
			if (filterExpression.Operand.IsEmpty)
				throw new FilterException("Invalid filter operand: it cannot be empty");

			var operand = Visit(filterExpression.Operand);

			if (!(operand is BsonFilter bson))
				throw new FilterException("Invalid filter type");

			var not = new BsonDocument {
				{ "type", FilterTypeString(filterExpression.ExpressionType) },
				{ "operand", bson.Value }
			};

			return new BsonFilter(FilterExpressionType.Not, not);
		}

		public override FilterExpression VisitFunction(FunctionFilterExpression filterExpression) {
			var variable = VisitVariable(filterExpression.Variable);
			var bsonVariable = ((BsonFilter) variable);

			var arguments = new BsonArray();
			if (filterExpression.Arguments != null) {
				foreach (var arg in filterExpression.Arguments) {
					var bsonArg = Visit(arg);
					var bsonArgFilter = (BsonFilter) bsonArg;

					arguments.Add(bsonArgFilter.Value);
				}
			}
			var function = new BsonDocument {
				{ "type", FilterTypeString(filterExpression.ExpressionType) },
				{ "variable", bsonVariable.Value },
				{ "function", filterExpression.FunctionName },
				{ "arguments", arguments }
			};

			return new BsonFilter(FilterExpressionType.Function, function);
		}

		public override FilterExpression VisitBinary(BinaryFilterExpression filterExpression) {
			if (filterExpression.Left.IsEmpty && filterExpression.Right.IsEmpty)
				throw new FilterException("The operands of a binary filter cannot be both empty");

			if (filterExpression.ExpressionType == FilterExpressionType.And ||
				filterExpression.ExpressionType == FilterExpressionType.Or) {
				if (filterExpression.Left.IsEmpty && !filterExpression.Right.IsEmpty)
					return Visit(filterExpression.Right);
				if (!filterExpression.Left.IsEmpty && filterExpression.Right.IsEmpty)
					return Visit(filterExpression.Left);
			}

			if (filterExpression.Left.IsEmpty || filterExpression.Right.IsEmpty)
				throw new FilterException($"The operands of a {filterExpression.ExpressionType} filter cannot be empty");

			var bsonLeft = (BsonFilter) Visit(filterExpression.Left);
			var bsonRight = (BsonFilter) Visit(filterExpression.Right);

			var bson = new BsonDocument {
				{ "type", filterExpression.ExpressionType.ToString().ToLowerInvariant() },
				{ "left", bsonLeft.Value },
				{ "right", bsonRight.Value }
			};

			switch (filterExpression.ExpressionType) {
				case FilterExpressionType.And:
				case FilterExpressionType.Or:
				case FilterExpressionType.Equal:
				case FilterExpressionType.NotEqual:
				case FilterExpressionType.GreaterThan:
				case FilterExpressionType.GreaterThanOrEqual:
				case FilterExpressionType.LessThan:
				case FilterExpressionType.LessThanOrEqual:
					return new BsonFilter(filterExpression.ExpressionType, bson);
			}

			throw new FilterException("Invalid filter type");
		}

		#region BsonFilter

		class BsonFilter : FilterExpression {
			public BsonFilter(FilterExpressionType expressionType, BsonValue value) {
				ExpressionType = expressionType;
				Value = value;
			}

			public override FilterExpressionType ExpressionType { get; }

			public BsonValue Value { get; }
		}

		#endregion
	}
}
