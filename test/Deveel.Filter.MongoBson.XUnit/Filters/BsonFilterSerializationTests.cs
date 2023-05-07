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
			var filter = Filter.Variable("x");

			var bson = filter.AsBsonDocument();

			Assert.NotNull(bson);
			Assert.Equal("variable", bson["type"].AsString);
			Assert.Equal("x", bson["varRef"].AsString);
		}

		[Theory]
		[InlineData(123)]
		[InlineData("test")]
		[InlineData(true)]
		[InlineData(false)]
		[InlineData(123.456)]
		[InlineData(123.456f)]
		[InlineData(null)]
		public static void SerializeConstant(object? value) {
			var valueType = (value?.GetType() ?? typeof(DBNull));
			var valueTypeString = valueType.ToString();

			var filter = Filter.Constant(value);
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
		[InlineData("x", 123, FilterType.Equals)]
		[InlineData("x", "test", FilterType.Equals)]
		[InlineData("x", 123, FilterType.NotEquals)]
		[InlineData("x", "test", FilterType.NotEquals)]
		[InlineData("x", 123, FilterType.GreaterThan)]
		[InlineData("x", 123, FilterType.GreaterThanOrEqual)]
		[InlineData("x", 123, FilterType.LessThan)]
		[InlineData("x", 123, FilterType.LessThanOrEqual)]
		public static void SerializeBinary(string varName, object value, FilterType filterType) {
			var variable = Filter.Variable(varName);
			var filter = Filter.Binary(variable, Filter.Constant(value), filterType);
			var bson = filter.AsBsonDocument();
			Assert.NotNull(bson);
			Assert.Equal(filterType.ToString().ToLowerInvariant(), bson["type"].AsString);
			var left = bson["left"].AsBsonDocument;
			Assert.NotNull(left);
			Assert.Equal("variable", left["type"].AsString);
			Assert.Equal(varName, left["varRef"].AsString);
			var right = bson["right"].AsBsonDocument;
			Assert.NotNull(right);
			Assert.Equal("constant", right["type"].AsString);
			Assert.Equal(value.GetType().ToString(), right["valueType"].AsString);
			var rightValue = BsonFilter.ConvertBsonValue(value.GetType(), right["value"]);
			Assert.Equal(value, rightValue);
		}

		[Theory]
		[InlineData("x", "Contains", 123)]
		public static void SerializeFunction(string varName, string functionName, object arg) {
			var variable = Filter.Variable(varName);
			var filter = Filter.Function(variable, functionName, Filter.Constant(arg));
			var bson = filter.AsBsonDocument();
			Assert.NotNull(bson);
			Assert.Equal("function", bson["type"].AsString);
			Assert.Equal(functionName, bson["function"].AsString);
			var args = bson["arguments"].AsBsonArray;
			Assert.NotNull(args);
			Assert.Single(args);
			var argBson = args[0].AsBsonDocument;
			Assert.NotNull(argBson);
			Assert.Equal("constant", argBson["type"].AsString);
			Assert.Equal(arg.GetType().ToString(), argBson["valueType"].AsString);
			var argValue = BsonFilter.ConvertBsonValue(arg.GetType(), argBson["value"]);
			Assert.Equal(arg, argValue);
		}

		[Fact]
		public static void DeserializeVariable() {
			var bson = new BsonDocument {
				{"type", "variable"},
				{"varRef", "x"}
			};

			var filter = BsonFilter.FromBson(bson);

			Assert.NotNull(filter);
			Assert.Equal(FilterType.Variable, filter.FilterType);

			var variable = Assert.IsType<VariableFilter>(filter);
			Assert.Equal("x", variable.VariableName);
		}

		[Theory]
		[InlineData(22)]
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
			Assert.Equal(FilterType.Constant, filter.FilterType);
			var constant = Assert.IsType<ConstantFilter>(filter);
			Assert.Equal(value, constant.Value);
		}

		[Theory]
		[InlineData("x", 123, FilterType.Equals)]
		[InlineData("x", "test", FilterType.Equals)]
		[InlineData("x", 123, FilterType.NotEquals)]
		[InlineData("x", "test", FilterType.NotEquals)]
		[InlineData("x", 123, FilterType.GreaterThan)]
		[InlineData("x", 123, FilterType.GreaterThanOrEqual)]
		[InlineData("x", 123, FilterType.LessThan)]
		[InlineData("x", 123, FilterType.LessThanOrEqual)]
		public static void DeserializeBinary(string varName, object value, FilterType filterType) {
			var bson = new BsonDocument {
				{"type", filterType.ToString().ToLowerInvariant()},
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
			Assert.Equal(filterType, filter.FilterType);

			var binary = Assert.IsType<BinaryFilter>(filter);

			var left = Assert.IsType<VariableFilter>(binary.Left);
			var right = Assert.IsType<ConstantFilter>(binary.Right);

			Assert.Equal(varName, left.VariableName);
			Assert.Equal(value, right.Value);
		}
	}
}
