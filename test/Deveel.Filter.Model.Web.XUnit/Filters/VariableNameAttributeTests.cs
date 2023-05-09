namespace Deveel.Filters {
    public static class VariableNameAttributeTests {
        [Theory]
        [InlineData("x", true)]
        [InlineData("x.name", true)]
        [InlineData("x_name", true)]
        [InlineData("x-name", false)]
        [InlineData("x name", false)]
        public static void TestVariableNameIsValid(string name, bool expected) {
            var attr = new VariableNameAttribute();
            var isValid = attr.IsValid(name);
            Assert.Equal(expected, isValid);
        }
    }
}
