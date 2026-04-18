using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;

namespace Deveel.Filters {
	public static class BsonFilterSerializationTests {
		[Fact]
		public static void SerializeVariable() {
			var filter = FilterExpression.Variable("x");

			var bson = filter.AsBsonDocument();

			Assert.NotNull(bson);
			Assert.Equal("variable", bson["type"].AsString);
			Assert.Equal("x", bson["varRef"].AsString);
		}

		[Theory]
		[InlineData(123)]
		[InlineData(123l)]
		[InlineData("test")]
		[InlineData(true)]
		[InlineData(false)]
		[InlineData(123.456)]
		[InlineData(123.456f)]
		[InlineData(null)]
		public static void SerializeConstant(object? value) {
			var valueType = (value?.GetType() ?? typeof(DBNull));
			var valueTypeString = BsonFilter.GetTypeString(valueType);

			var filter = FilterExpression.Constant(value);
			var bson = filter.AsBsonDocument();

			Assert.NotNull(bson);
			Assert.Equal("constant", bson["type"].AsString);
			Assert.Equal(valueTypeString, bson["valueType"].AsString);

			var bsonValue = bson["value"];

			Assert.NotNull(bsonValue);

			var runtimeValue = BsonFilter.ConvertBsonValue(valueType, bsonValue);
			Assert.Equal(value, runtimeValue);
		}

		[Theory]
		[InlineData("2009-12-01T01:23:56")]
		[InlineData("2009-12-01T01:23:56Z")]
		[InlineData("2009-12-01T01:23:56+01:00")]
		public static void SerializeDateTime(string dateTimeString) {
			var dateTime = DateTime.Parse(dateTimeString);

			var filter = FilterExpression.Constant(dateTime);
			var bson = filter.AsBsonDocument();

			Assert.NotNull(bson);
			Assert.Equal("constant", bson["type"].AsString);
			Assert.Equal("datetime", bson["valueType"].AsString);

			var bsonValue = bson["value"];
			Assert.NotNull(bsonValue);

			var runtimeValue = BsonFilter.ConvertBsonValue(typeof(DateTime), bsonValue);

			var convertedDate = Assert.IsType<DateTime>(runtimeValue);
			Assert.Equal(dateTime.ToUniversalTime(), convertedDate.ToUniversalTime());
		}

		[Theory]
		[InlineData("2009-12-01T01:23:56")]
		[InlineData("2009-12-01T01:23:56Z")]
		[InlineData("2009-12-01T01:23:56+01:00")]
		public static void SerializeDateTimeOffset(string dateTimeString) {
            var dateTime = DateTimeOffset.Parse(dateTimeString);
            var filter = FilterExpression.Constant(dateTime);
            var bson = filter.AsBsonDocument();
            Assert.NotNull(bson);
            Assert.Equal("constant", bson["type"].AsString);
            Assert.Equal("datetime2", bson["valueType"].AsString);
            var bsonValue = bson["value"];
            Assert.NotNull(bsonValue);
            var runtimeValue = BsonFilter.ConvertBsonValue(typeof(DateTimeOffset), bsonValue);
            var convertedDate = Assert.IsType<DateTimeOffset>(runtimeValue);
            Assert.Equal(dateTime.ToUniversalTime(), convertedDate.ToUniversalTime());
        }

		[Theory]
		[InlineData("x", 123, FilterExpressionType.Equal)]
		[InlineData("x", "test", FilterExpressionType.Equal)]
		[InlineData("x", 123, FilterExpressionType.NotEqual)]
		[InlineData("x", "test", FilterExpressionType.NotEqual)]
		[InlineData("x", 123, FilterExpressionType.GreaterThan)]
		[InlineData("x", 123, FilterExpressionType.GreaterThanOrEqual)]
		[InlineData("x", 123, FilterExpressionType.LessThan)]
		[InlineData("x", 123, FilterExpressionType.LessThanOrEqual)]
		public static void SerializeBinary(string varName, object value, FilterExpressionType expressionType) {
			var variable = FilterExpression.Variable(varName);
			var filter = FilterExpression.Binary(variable, FilterExpression.Constant(value), expressionType);
			var bson = filter.AsBsonDocument();
			Assert.NotNull(bson);
			Assert.Equal(expressionType.ToString().ToLowerInvariant(), bson["type"].AsString);
			var left = bson["left"].AsBsonDocument;
			Assert.NotNull(left);
			Assert.Equal("variable", left["type"].AsString);
			Assert.Equal(varName, left["varRef"].AsString);
			var right = bson["right"].AsBsonDocument;
			Assert.NotNull(right);
			Assert.Equal("constant", right["type"].AsString);
			Assert.Equal(BsonFilter.GetTypeString(value.GetType()), right["valueType"].AsString);
			var rightValue = BsonFilter.ConvertBsonValue(value.GetType(), right["value"]);
			Assert.Equal(value, rightValue);
		}

		[Fact]
		public static void SerializeNot() {
			var variable = FilterExpression.Variable("x");
			var filter = FilterExpression.Not(variable);
			var bson = filter.AsBsonDocument();
			Assert.NotNull(bson);
			Assert.Equal("not", bson["type"].AsString);
			var operand = bson["operand"].AsBsonDocument;
			Assert.NotNull(operand);
			Assert.Equal("variable", operand["type"].AsString);
			Assert.Equal("x", operand["varRef"].AsString);
		}

		[Theory]
		[InlineData("x", "Contains", 123)]
		public static void SerializeFunction(string varName, string functionName, object arg) {
			var variable = FilterExpression.Variable(varName);
			var filter = FilterExpression.Function(variable, functionName, new[] { FilterExpression.Constant(arg) });
			var bson = filter.AsBsonDocument();
			Assert.NotNull(bson);
			Assert.Equal("function", bson["type"].AsString);
			Assert.Equal(functionName, bson["function"].AsString);

			Assert.NotNull(bson["variable"]);
			Assert.NotNull(bson["variable"]["type"]);
			Assert.Equal("variable", bson["variable"]["type"].AsString);
			Assert.NotNull(bson["variable"]["varRef"]);
			Assert.Equal(varName, bson["variable"]["varRef"].AsString);

			var args = bson["arguments"].AsBsonArray;
			Assert.NotNull(args);
			Assert.Single(args);
			var argBson = args[0].AsBsonDocument;
			Assert.NotNull(argBson);
			Assert.Equal("constant", argBson["type"].AsString);
			Assert.Equal(BsonFilter.GetTypeString(arg.GetType()), argBson["valueType"].AsString);
			var argValue = BsonFilter.ConvertBsonValue(arg.GetType(), argBson["value"]);
			Assert.Equal(arg, argValue);
		}

		[Fact]
		public static void SerializeLogicalAndWithEmpty() {
			var filter = FilterExpression.And(FilterExpression.GreaterThan(FilterExpression.Constant(22), FilterExpression.Variable("x")), FilterExpression.Empty);

			var bson = filter.AsBsonDocument();
			Assert.NotNull(bson);
			Assert.Equal("greaterthan", bson["type"].AsString);
			var left = bson["left"].AsBsonDocument;
			Assert.NotNull(left);
			Assert.Equal("constant", left["type"].AsString);
			Assert.Equal("int", left["valueType"].AsString);
			Assert.Equal(22, left["value"].AsInt32);
			var right = bson["right"].AsBsonDocument;
			Assert.NotNull(right);
			Assert.Equal("variable", right["type"].AsString);
			Assert.Equal("x", right["varRef"].AsString);
		}

		[Fact]
		public static void DeserializeVariable() {
			var bson = new BsonDocument {
				{"type", "variable"},
				{"varRef", "x"}
			};

			var filter = BsonFilter.FromBson(bson);

			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.Variable, filter.ExpressionType);

			var variable = Assert.IsType<VariableFilterExpression>(filter);
			Assert.Equal("x", variable.VariableName);
		}

		[Theory]
		[InlineData(22)]
		[InlineData(Int64.MaxValue - 1)]
		[InlineData("test")]
		[InlineData(true)]
		[InlineData(false)]
		[InlineData(123.456)]
		[InlineData(123.456f)]
		[InlineData(null)]
		public static void DeserializeConstant(object? value) {
			var valueType = (value?.GetType() ?? typeof(DBNull));
			var valueTypeString = valueType.ToString();
			var bson = new BsonDocument {
				{"type", "constant"},
				{"valueType", valueTypeString},
				{"value", BsonValue.Create(value)}
			};
			var filter = BsonFilter.FromBson(bson);
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.Constant, filter.ExpressionType);
			var constant = Assert.IsType<ConstantFilterExpression>(filter);
			Assert.Equal(value, constant.Value);
		}

		[Theory]
		[InlineData("x", 123, FilterExpressionType.Equal)]
		[InlineData("x", "test", FilterExpressionType.Equal)]
		[InlineData("x", 123, FilterExpressionType.NotEqual)]
		[InlineData("x", "test", FilterExpressionType.NotEqual)]
		[InlineData("x", 123, FilterExpressionType.GreaterThan)]
		[InlineData("x", 123, FilterExpressionType.GreaterThanOrEqual)]
		[InlineData("x", 123, FilterExpressionType.LessThan)]
		[InlineData("x", 123, FilterExpressionType.LessThanOrEqual)]
		public static void DeserializeBinary(string varName, object value, FilterExpressionType expressionType) {
			var bson = new BsonDocument {
				{"type", expressionType.ToString().ToLowerInvariant()},
				{"left", new BsonDocument {
					{"type", "variable"},
					{"varRef", varName}
				}},
				{"right", new BsonDocument {
					{"type", "constant"},
					{"valueType", value.GetType().ToString()},
					{"value", BsonValue.Create(value)}
				}}
			};

			var filter = BsonFilter.FromBson(bson);
			Assert.NotNull(filter);
			Assert.Equal(expressionType, filter.ExpressionType);

			var binary = Assert.IsType<BinaryFilterExpression>(filter);

			var left = Assert.IsType<VariableFilterExpression>(binary.Left);
			var right = Assert.IsType<ConstantFilterExpression>(binary.Right);

			Assert.Equal(varName, left.VariableName);
			Assert.Equal(value, right.Value);
		}

		[Theory]
		[InlineData("x", FilterExpressionType.Not)]
		public static void DeserializeNot(string varName, FilterExpressionType expressionType) {
			var bson = new BsonDocument {
				{"type", "not"},
				{"operand", new BsonDocument {
					{"type", "variable"},
					{"varRef", varName}
				}}
			};
			var filter = BsonFilter.FromBson(bson);
			Assert.NotNull(filter);
			Assert.Equal(expressionType, filter.ExpressionType);
			var unary = Assert.IsType<UnaryFilterExpression>(filter);
			var operand = Assert.IsType<VariableFilterExpression>(unary.Operand);
			Assert.Equal(varName, operand.VariableName);
		}

		[Theory]
		[InlineData("x.name", "Contains", "foo")]
		public static void DeserializeFunction(string varName, string functionName, object arg) {
			var bson = new BsonDocument {
				{"type", "function"},
				{"variable", new BsonDocument {
					{ "type", "variable" },
					{ "varRef", varName }
				} },
				{"function", functionName},
				{"arguments", new BsonArray {
					new BsonDocument {
						{"type", "constant"},
						{"valueType", BsonValue.Create(BsonFilter.GetTypeString(arg.GetType()))},
						{"value", BsonValue.Create(arg)}
					}
				}}
			};

			var filter = BsonFilter.FromBson(bson);
			Assert.NotNull(filter);
			Assert.Equal(FilterExpressionType.Function, filter.ExpressionType);
			var function = Assert.IsType<FunctionFilterExpression>(filter);
			Assert.Equal(functionName, function.FunctionName);
			var args = Assert.Single(function.Arguments);
			var argConstant = Assert.IsType<ConstantFilterExpression>(args);
			Assert.Equal(arg, argConstant.Value);
		}
	}
}
