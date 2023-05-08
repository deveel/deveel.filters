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
    }
}
