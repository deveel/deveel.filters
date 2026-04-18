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
			var filter = FilterExpression.Binary(FilterExpression.Variable("Name"), FilterExpression.Constant("John"), FilterExpressionType.Equal);
			var str = filter.ToDynamicString();
			Assert.Equal("Name == \"John\"", str);
		}

		[Fact]
		public static void ToDynamicString_NullConstant() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("Name"), FilterExpression.Constant(null), FilterExpressionType.Equal);
			var str = filter.ToDynamicString();
			Assert.Equal("Name == null", str);
		}

		[Fact]
		public static void ToDynamicString_BoolConstant() {
			var filter = FilterExpression.Binary(FilterExpression.Variable("IsActive"), FilterExpression.Constant(true), FilterExpressionType.Equal);
			var str = filter.ToDynamicString();
			Assert.Equal("IsActive == true", str);
		}

		[Fact]
		public static void ToDynamicString_DateTime() {
			var dt = new DateTime(2023, 6, 15);
			var filter = FilterExpression.Binary(FilterExpression.Variable("BirthDate"), FilterExpression.Constant(dt), FilterExpressionType.GreaterThan);
			var str = filter.ToDynamicString();
			Assert.Contains("DateTime(2023, 6, 15)", str);
		}

		[Fact]
		public static void ToDynamicString_DateTimeWithTime() {
			var dt = new DateTime(2023, 6, 15, 10, 30, 45);
			var filter = FilterExpression.Constant(dt);
			var str = filter.ToDynamicString();
			Assert.Contains("DateTime(2023, 6, 15, 10, 30, 45)", str);
		}

		[Fact]
		public static void ToDynamicString_UnaryNot() {
			var filter = FilterExpression.Not(FilterExpression.Binary(FilterExpression.Variable("Age"), FilterExpression.Constant(25), FilterExpressionType.GreaterThan));
			var str = filter.ToDynamicString();
			Assert.Contains("!", str);
		}

		[Fact]
		public static void ToDynamicString_FunctionCall() {
			var filter = FilterExpression.Function(FilterExpression.Variable("Name"), "Contains", new FilterExpression[] { FilterExpression.Constant("test") });
			var str = filter.ToDynamicString();
			Assert.Equal("Name.Contains(\"test\")", str);
		}

		[Fact]
		public static void ToDynamicString_FunctionNoArgs() {
			var filter = FilterExpression.Function(FilterExpression.Variable("Tags"), "Any");
			var str = filter.ToDynamicString();
			Assert.Equal("Tags.Any()", str);
		}

		[Fact]
		public static void ToDynamicString_LogicalAndWithEmptyLeft() {
			var filter = FilterExpression.Binary(
				FilterExpression.Empty,
				FilterExpression.Binary(FilterExpression.Variable("Age"), FilterExpression.Constant(25), FilterExpressionType.GreaterThan),
				FilterExpressionType.And);
			var str = filter.ToDynamicString();
			Assert.Equal("Age > 25", str);
		}

		[Fact]
		public static void ToDynamicString_LogicalOrWithEmptyRight() {
			var filter = FilterExpression.Binary(
				FilterExpression.Binary(FilterExpression.Variable("Age"), FilterExpression.Constant(25), FilterExpressionType.GreaterThan),
				FilterExpression.Empty,
				FilterExpressionType.Or);
			var str = filter.ToDynamicString();
			Assert.Equal("Age > 25", str);
		}

		[Fact]
		public static void ToDynamicString_BothEmptyThrows() {
			var filter = FilterExpression.Binary(FilterExpression.Empty, FilterExpression.Empty, FilterExpressionType.And);
			Assert.Throws<FilterException>(() => filter.ToDynamicString());
		}

		[Fact]
		public static void ToDynamicString_UnaryEmptyOperandThrows() {
			var filter = FilterExpression.Unary(FilterExpression.Empty, FilterExpressionType.Not);
			Assert.Throws<FilterException>(() => filter.ToDynamicString());
		}

		#endregion

		#region AsDynamicLambda null checks

		[Fact]
		public static void AsDynamicLambda_NullFilter_Throws() {
			FilterExpression filter = null!;
			Assert.Throws<ArgumentNullException>(() => filter.AsDynamicLambda(typeof(Person)));
		}

		[Fact]
		public static void AsDynamicLambda_NullType_Throws() {
			var filter = FilterExpression.Variable("x");
			Assert.Throws<ArgumentNullException>(() => filter.AsDynamicLambda(null!));
		}

		#endregion

		#region FilterExpressionParser with empty/null parameter name

		[Fact]
		public static void Parse_WithEmptyParameterName() {
			// The empty parameter name path goes through the else branch
			var filter = FilterExpressionParser.Parse("Age > 25", typeof(Person), "");
			Assert.NotNull(filter);
			var binary = Assert.IsType<BinaryFilterExpression>(filter);
			Assert.Equal(FilterExpressionType.GreaterThan, binary.ExpressionType);
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
			var filter = FilterExpression.Constant('A');
			var str = filter.ToDynamicString();
			Assert.Equal("'A'", str);
		}

		#endregion

		#region Decimal/Double constants

		[Fact]
		public static void ToDynamicString_DecimalConstant() {
			var filter = FilterExpression.Constant(123.45m);
			var str = filter.ToDynamicString();
			Assert.Contains("123", str);
		}

		#endregion
	}
}


