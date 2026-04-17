using Xunit;

namespace Deveel.Filters
{
	[Trait("Feature", "Dynamic Filter String")]
	public static class DynamicStringTests
	{
		public class Person
		{
			public string Name { get; set; } = "";
			public int Age { get; set; }
			public bool IsActive { get; set; }
			public DateTime BirthDate { get; set; }
			public decimal Price { get; set; }
		}

		#region ToDynamicString

		[Fact]
		public static void ToDynamicString_SimpleEquality() {
			var filter = Filter.Binary(Filter.Variable("Name"), Filter.Constant("John"), FilterType.Equal);
			var str = filter.ToDynamicString();
			Assert.Equal("Name == \"John\"", str);
		}

		[Fact]
		public static void ToDynamicString_NullConstant() {
			var filter = Filter.Binary(Filter.Variable("Name"), Filter.Constant(null), FilterType.Equal);
			var str = filter.ToDynamicString();
			Assert.Equal("Name == null", str);
		}

		[Fact]
		public static void ToDynamicString_BoolConstant() {
			var filter = Filter.Binary(Filter.Variable("IsActive"), Filter.Constant(true), FilterType.Equal);
			var str = filter.ToDynamicString();
			Assert.Equal("IsActive == true", str);
		}

		[Fact]
		public static void ToDynamicString_DateTime() {
			var dt = new DateTime(2023, 6, 15);
			var filter = Filter.Binary(Filter.Variable("BirthDate"), Filter.Constant(dt), FilterType.GreaterThan);
			var str = filter.ToDynamicString();
			Assert.Contains("DateTime(2023, 6, 15)", str);
		}

		[Fact]
		public static void ToDynamicString_DateTimeWithTime() {
			var dt = new DateTime(2023, 6, 15, 10, 30, 45);
			var filter = Filter.Constant(dt);
			var str = filter.ToDynamicString();
			Assert.Contains("DateTime(2023, 6, 15, 10, 30, 45)", str);
		}

		[Fact]
		public static void ToDynamicString_UnaryNot() {
			var filter = Filter.Not(Filter.Binary(Filter.Variable("Age"), Filter.Constant(25), FilterType.GreaterThan));
			var str = filter.ToDynamicString();
			Assert.Contains("!", str);
		}

		[Fact]
		public static void ToDynamicString_FunctionCall() {
			var filter = Filter.Function(Filter.Variable("Name"), "Contains", new Filter[] { Filter.Constant("test") });
			var str = filter.ToDynamicString();
			Assert.Equal("Name.Contains(\"test\")", str);
		}

		[Fact]
		public static void ToDynamicString_FunctionNoArgs() {
			var filter = Filter.Function(Filter.Variable("Tags"), "Any");
			var str = filter.ToDynamicString();
			Assert.Equal("Tags.Any()", str);
		}

		[Fact]
		public static void ToDynamicString_LogicalAndWithEmptyLeft() {
			var filter = Filter.Binary(
				Filter.Empty,
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(25), FilterType.GreaterThan),
				FilterType.And);
			var str = filter.ToDynamicString();
			Assert.Equal("Age > 25", str);
		}

		[Fact]
		public static void ToDynamicString_LogicalOrWithEmptyRight() {
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(25), FilterType.GreaterThan),
				Filter.Empty,
				FilterType.Or);
			var str = filter.ToDynamicString();
			Assert.Equal("Age > 25", str);
		}

		[Fact]
		public static void ToDynamicString_BothEmptyThrows() {
			var filter = Filter.Binary(Filter.Empty, Filter.Empty, FilterType.And);
			Assert.Throws<FilterException>(() => filter.ToDynamicString());
		}

		[Fact]
		public static void ToDynamicString_UnaryEmptyOperandThrows() {
			var filter = Filter.Unary(Filter.Empty, FilterType.Not);
			Assert.Throws<FilterException>(() => filter.ToDynamicString());
		}

		#endregion

		#region AsDynamicLambda null checks

		[Fact]
		public static void AsDynamicLambda_NullFilter_Throws() {
			Filter filter = null!;
			Assert.Throws<ArgumentNullException>(() => filter.AsDynamicLambda(typeof(Person)));
		}

		[Fact]
		public static void AsDynamicLambda_NullType_Throws() {
			var filter = Filter.Variable("x");
			Assert.Throws<ArgumentNullException>(() => filter.AsDynamicLambda(null!));
		}

		#endregion

		#region FilterExpressionParser with empty/null parameter name

		[Fact]
		public static void Parse_WithEmptyParameterName() {
			// The empty parameter name path goes through the else branch
			var filter = FilterExpressionParser.Parse("Age > 25", typeof(Person), "");
			Assert.NotNull(filter);
			var binary = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.GreaterThan, binary.FilterType);
		}

		[Fact]
		public static void Parse_WithNullParameterName_Throws() {
			Assert.Throws<FilterException>(() =>
				FilterExpressionParser.Parse("Age > 25", typeof(Person), null!));
		}

		#endregion

		#region Char constant in dynamic string

		[Fact]
		public static void ToDynamicString_CharConstant() {
			var filter = Filter.Constant('A');
			var str = filter.ToDynamicString();
			Assert.Equal("'A'", str);
		}

		#endregion

		#region Decimal/Double constants

		[Fact]
		public static void ToDynamicString_DecimalConstant() {
			var filter = Filter.Constant(123.45m);
			var str = filter.ToDynamicString();
			Assert.Contains("123", str);
		}

		#endregion
	}
}


