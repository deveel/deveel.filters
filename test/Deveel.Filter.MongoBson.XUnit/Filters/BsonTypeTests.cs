using MongoDB.Bson;

namespace Deveel.Filters {
    public static class BsonTypeTests {
        [Theory]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(string), "string")]
        [InlineData(typeof(double), "double")]
        [InlineData(typeof(float), "float")]
        [InlineData(typeof(long), "long")]
        [InlineData(typeof(bool), "bool")]
        [InlineData(typeof(DateTime), "datetime")]
        [InlineData(typeof(DateTimeOffset), "datetime2")]
        [InlineData(typeof(Guid), "System.Guid")]
        public static void GetTypeString(Type type, string expected) {
            var typeString = BsonFilter.GetTypeString(type);

            Assert.Equal(expected, typeString);
        }

        [Theory]
        [InlineData("int", typeof(int))]
        [InlineData("string", typeof(string))]
        [InlineData("double", typeof(double))]
        [InlineData("float", typeof(float))]
        [InlineData("long", typeof(long))]
        [InlineData("bool", typeof(bool))]
        [InlineData("datetime", typeof(DateTime))]
        [InlineData("datetime2", typeof(DateTimeOffset))]
        [InlineData("System.Guid", typeof(Guid))]
        public static void GetTypeFromString(string typeString, Type expected) {
            var type = BsonFilter.GetTypeFromString(typeString);
            Assert.Equal(expected, type);
        }

        [Fact]
        public static void UnkownTypeFromDocument() {
            var document = new BsonDocument {
                { "type", "System.UnknownType" }
            };

            var exception = Assert.Throws<FilterException>(() => BsonFilter.FromBson(document));
        }

        [Fact]
        public static void UnknownValueTypeFromConstant() {
            var document = new BsonDocument {
                { "type", "constant" },
                { "value", "Hello World" },
                { "valueType", "System.UnknownType" }
            };

            var exception = Assert.Throws<FilterException>(() => BsonFilter.FromBson(document));
        }

        [Fact]
        public static void MalformedFunctionFilter_MissingFunctionName() {
            var document = new BsonDocument {
                { "type", "function" },
                { "variable", "test" },
                { "arguments", new BsonArray() }
            };

            var exception = Assert.Throws<FilterException>(() => BsonFilter.FromBson(document));
        }

        [Fact]
        public static void MalformedFunctionFilter_MissingVariableName() {
            var document = new BsonDocument {
                { "type", "function" },
                { "function", "test" },
                { "arguments", new BsonArray() }
            };
            var exception = Assert.Throws<FilterException>(() => BsonFilter.FromBson(document));
        }
    }
}
