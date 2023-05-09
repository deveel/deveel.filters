using System.Text.Json;

namespace Deveel.Filters {
    public static class SimpleValueAttributeTests {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData("test")]
        [InlineData(null)]
        [InlineData(234)]
        [InlineData(1234.56)]
        public static void TestValueIsValid(object? value) {
            var attr = new SimpleValueAttribute();
            var isValid = attr.IsValid(value);

            Assert.True(isValid);
        }

        [Theory]
        [InlineData("test", true)]
        [InlineData("test", false)]
        [InlineData("test", "test")]
        [InlineData("test", null)]
        [InlineData("test", 234)]
        [InlineData("test", 1234.56)]
        [InlineData("test", 1234.56f)]
        public static void TestDictionaryOfPrimitivesIsValid(string key, object value) {
            var dictionary = new Dictionary<string, object> {
                { key, value  }
            };

            var attr = new SimpleValueAttribute();
            var isValid = attr.IsValid(dictionary);
            Assert.True(isValid);
        }

        [Theory]
        [InlineData("test", true)]
        [InlineData("test", false)]
        [InlineData("test", "test")]
        [InlineData("test", null)]
        [InlineData("test", 234)]
        [InlineData("test", 1234.56)]
        public static void TestDictionaryOfJsonElementsIsValid(string key, object value) {
            var valueString = (value == null ? "null" : (value is string s ? $"\"{s}\"" : (value is bool b ? (b == true ? "true" : "false") : Convert.ToString(value))));
            var dictionary = new Dictionary<string, JsonElement> {
                { key, JsonDocument.Parse(valueString).RootElement  }
            };

            var attr = new SimpleValueAttribute();
            var isValid = attr.IsValid(dictionary);
            Assert.True(isValid);
        }
    }
}
