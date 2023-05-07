using System.Net;

using MongoDB.Bson;

namespace Deveel.Filters {
	class BsonFilterVisitor : FilterVisitor {
		internal BsonDocument BuildDocument(Filter filter) {
			var bsonFilter = Visit(filter);

			if (!(bsonFilter is BsonFilter bson))
				throw new FilterException("Invalid filter type");

			if (!(bson.Value is BsonDocument document))
				throw new FilterException("Invalid filter value");

			return document;
		}

		public override Filter VisitConstant(ConstantFilter constant) {
			var value = constant.Value;
			var valueType = value?.GetType() ?? typeof(DBNull);

			BsonValue bsonValue;

			if (value == null) {
				bsonValue = BsonNull.Value;
			} else if (value is DateTimeOffset date2) {
				bsonValue = BsonDateTime.Create(date2.UtcDateTime);
			} else {
				bsonValue = BsonValue.Create(value);
			}

			var bson = new BsonDocument {
				{ "type", FilterTypeString(constant.FilterType) },
				{ "valueType", BsonValue.Create(valueType.ToString()) },
				{ "value", bsonValue }
			};

			return new BsonFilter(FilterType.Constant, bson);
		}

		public override Filter VisitVariable(VariableFilter variable) {
			var varRef = new BsonDocument {
				{ "type", FilterTypeString(variable.FilterType) },
				{ "varRef", variable.VariableName }
			};

			return new BsonFilter(FilterType.Variable, varRef);
		}

		private static string FilterTypeString(FilterType filterType)
			=> filterType.ToString().ToLowerInvariant();

		public override Filter VisitUnary(UnaryFilter filter) {
			var operand = Visit(filter.Operand);

			if (!(operand is BsonFilter bson))
				throw new FilterException("Invalid filter type");

			var not = new BsonDocument {
				{ "type", FilterTypeString(filter.FilterType) },
				{ "not", bson.Value }
			};

			return new BsonFilter(FilterType.Not, not);
		}

		public override Filter VisitFunction(FunctionFilter filter) {
			var variable = VisitVariable(filter.Variable);
			var bsonVariable = ((BsonFilter) variable);

			var arguments = new BsonArray();
			if (filter.Arguments != null) {
				foreach (var arg in filter.Arguments) {
					var bsonArg = Visit(arg);
					var bsonArgFilter = (BsonFilter) bsonArg;

					arguments.Add(bsonArgFilter.Value);
				}
			}
			var function = new BsonDocument {
				{ "type", FilterTypeString(filter.FilterType) },
				{ "variable", bsonVariable.Value },
				{ "function", filter.FunctionName },
				{ "arguments", arguments }
			};

			return new BsonFilter(FilterType.Function, function);
		}

		public override Filter VisitBinary(BinaryFilter filter) {
			var bsonLeft = (BsonFilter) Visit(filter.Left);
			var bsonRight = (BsonFilter) Visit(filter.Right);

			var bson = new BsonDocument {
				{ "type", filter.FilterType.ToString().ToLowerInvariant() },
				{ "left", bsonLeft.Value },
				{ "right", bsonRight.Value }
			};

			switch (filter.FilterType) {
				case FilterType.And:
				case FilterType.Or:
				case FilterType.Equals:
				case FilterType.NotEquals:
				case FilterType.GreaterThan:
				case FilterType.GreaterThanOrEqual:
				case FilterType.LessThan:
				case FilterType.LessThanOrEqual:
					return new BsonFilter(filter.FilterType, bson);
			}

			throw new FilterException("Invalid filter type");
		}

		#region BsonFilter

		class BsonFilter : Filter {
			public BsonFilter(FilterType filterType, BsonValue value) {
				FilterType = filterType;
				Value = value;
			}

			public override FilterType FilterType { get; }

			public BsonValue Value { get; }
		}

		#endregion
	}
}
