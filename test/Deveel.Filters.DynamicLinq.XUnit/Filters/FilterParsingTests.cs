using System.Linq.Dynamic.Core;
using Xunit;

namespace Deveel.Filters
{
	[Trait("Feature", "Filter Expression Parsing")]
	public class FilterParsingTests
	{
		#region Test Models

		public class Person
		{
			public string Name { get; set; } = "";
			public int Age { get; set; }
			public bool IsActive { get; set; }
			public double Salary { get; set; }
			public Address? Address { get; set; }
			public DateTime BirthDate { get; set; }
		}

		public class Address
		{
			public string Street { get; set; } = "";
			public string City { get; set; } = "";
			public string Country { get; set; } = "";
			public int ZipCode { get; set; }
		}

		public class Product
		{
			public string Name { get; set; } = "";
			public decimal Price { get; set; }
			public int CategoryId { get; set; }
			public bool IsAvailable { get; set; }
		}

		#endregion

		#region Simple Equality Tests

		[Theory]
		[InlineData("Name == \"John\"", "John")]
		[InlineData("Name == \"Jane\"", "Jane")]
		[InlineData("Name == \"\"", "")]
		public void ParseSimpleStringEquality(string expression, string expectedValue)
		{
			var filter = FilterExpressionParser.Parse<Person>(expression);

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Name", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(expectedValue, rightConstant.Value);
		}

		[Theory]
		[InlineData("Age == 25", 25)]
		[InlineData("Age == 0", 0)]
		[InlineData("Age == -5", -5)]
		public void ParseSimpleIntegerEquality(string expression, int expectedValue)
		{
			var filter = FilterExpressionParser.Parse<Person>(expression);

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Age", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(expectedValue, rightConstant.Value);
		}

		[Theory]
		[InlineData("IsActive == true", true)]
		[InlineData("IsActive == false", false)]
		public void ParseSimpleBooleanEquality(string expression, bool expectedValue)
		{
			var filter = FilterExpressionParser.Parse<Person>(expression);

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("IsActive", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(expectedValue, rightConstant.Value);
		}

		[Theory]
		[InlineData("Salary == 50000.5", 50000.5)]
		[InlineData("Salary == 0.0", 0.0)]
		[InlineData("Salary == -1000.75", -1000.75)]
		public void ParseSimpleDoubleEquality(string expression, double expectedValue)
		{
			var filter = FilterExpressionParser.Parse<Person>(expression);

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Salary", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(expectedValue, rightConstant.Value);
		}

		#endregion

		#region Comparison Operators Tests

		[Theory]
		[InlineData("Age > 25", FilterType.GreaterThan)]
		[InlineData("Age >= 25", FilterType.GreaterThanOrEqual)]
		[InlineData("Age < 25", FilterType.LessThan)]
		[InlineData("Age <= 25", FilterType.LessThanOrEqual)]
		[InlineData("Age != 25", FilterType.NotEqual)]
		public void ParseComparisonOperators(string expression, FilterType expectedFilterType)
		{
			var filter = FilterExpressionParser.Parse<Person>(expression);

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(expectedFilterType, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Age", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(25, rightConstant.Value);
		}

		#endregion

		#region Logical Operators Tests

		[Fact]
		public void ParseLogicalAnd()
		{
			var filter = FilterExpressionParser.Parse<Person>("Age > 18 && IsActive == true");

			Assert.NotNull(filter);
			var andFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.And, andFilter.FilterType);

			// Left side: Age > 18
			var leftBinary = Assert.IsType<BinaryFilter>(andFilter.Left);
			Assert.Equal(FilterType.GreaterThan, leftBinary.FilterType);
			Assert.Equal("Age", ((VariableFilter)leftBinary.Left).VariableName);
			Assert.Equal(18, ((ConstantFilter)leftBinary.Right).Value);

			// Right side: IsActive == true
			var rightBinary = Assert.IsType<BinaryFilter>(andFilter.Right);
			Assert.Equal(FilterType.Equal, rightBinary.FilterType);
			Assert.Equal("IsActive", ((VariableFilter)rightBinary.Left).VariableName);
			Assert.Equal(true, ((ConstantFilter)rightBinary.Right).Value);
		}

		[Fact]
		public void ParseLogicalOr()
		{
			var filter = FilterExpressionParser.Parse<Person>("Age < 18 || Age > 65");

			Assert.NotNull(filter);
			var orFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Or, orFilter.FilterType);

			// Left side: Age < 18
			var leftBinary = Assert.IsType<BinaryFilter>(orFilter.Left);
			Assert.Equal(FilterType.LessThan, leftBinary.FilterType);
			Assert.Equal("Age", ((VariableFilter)leftBinary.Left).VariableName);
			Assert.Equal(18, ((ConstantFilter)leftBinary.Right).Value);

			// Right side: Age > 65
			var rightBinary = Assert.IsType<BinaryFilter>(orFilter.Right);
			Assert.Equal(FilterType.GreaterThan, rightBinary.FilterType);
			Assert.Equal("Age", ((VariableFilter)rightBinary.Left).VariableName);
			Assert.Equal(65, ((ConstantFilter)rightBinary.Right).Value);
		}

		[Fact]
		public void ParseComplexLogicalExpression()
		{
			var filter = FilterExpressionParser.Parse<Person>("(Age > 18 && Age < 65) || IsActive == false");

			Assert.NotNull(filter);
			var orFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Or, orFilter.FilterType);

			// Left side: (Age > 18 && Age < 65)
			var leftAnd = Assert.IsType<BinaryFilter>(orFilter.Left);
			Assert.Equal(FilterType.And, leftAnd.FilterType);

			// Right side: IsActive == false
			var rightBinary = Assert.IsType<BinaryFilter>(orFilter.Right);
			Assert.Equal(FilterType.Equal, rightBinary.FilterType);
		}

		#endregion

		#region Negation Tests

		[Fact]
		public void ParseSimpleNegation()
		{
			var filter = FilterExpressionParser.Parse<Person>("!(Age > 25)");

			Assert.NotNull(filter);
			var notFilter = Assert.IsType<UnaryFilter>(filter);
			Assert.Equal(FilterType.Not, notFilter.FilterType);

			var innerBinary = Assert.IsType<BinaryFilter>(notFilter.Operand);
			Assert.Equal(FilterType.GreaterThan, innerBinary.FilterType);
			Assert.Equal("Age", ((VariableFilter)innerBinary.Left).VariableName);
			Assert.Equal(25, ((ConstantFilter)innerBinary.Right).Value);
		}

		[Fact]
		public void ParseNegationWithLogical()
		{
			var filter = FilterExpressionParser.Parse<Person>("!(Age > 18 && IsActive == true)");

			Assert.NotNull(filter);
			var notFilter = Assert.IsType<UnaryFilter>(filter);
			Assert.Equal(FilterType.Not, notFilter.FilterType);

			var innerAnd = Assert.IsType<BinaryFilter>(notFilter.Operand);
			Assert.Equal(FilterType.And, innerAnd.FilterType);
		}

		#endregion

		#region Nested Property Tests

		[Fact]
		public void ParseNestedPropertyAccess()
		{
			var filter = FilterExpressionParser.Parse<Person>("Address.City == \"New York\"");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Address.City", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal("New York", rightConstant.Value);
		}

		[Fact]
		public void ParseDeepNestedPropertyAccess()
		{
			var filter = FilterExpressionParser.Parse<Person>("Address.ZipCode > 10000");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.GreaterThan, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Address.ZipCode", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(10000, rightConstant.Value);
		}

		#endregion

		#region Method Call Tests

		[Fact]
		public void ParseStringContainsMethod()
		{
			var filter = FilterExpressionParser.Parse<Person>("Name.Contains(\"John\")");

			Assert.NotNull(filter);
			var functionFilter = Assert.IsType<FunctionFilter>(filter);
			Assert.Equal("Contains", functionFilter.FunctionName);

			var variable = Assert.IsType<VariableFilter>(functionFilter.Variable);
			Assert.Equal("Name", variable.VariableName);

			Assert.Single(functionFilter.Arguments);
			var argument = Assert.IsType<ConstantFilter>(functionFilter.Arguments[0]);
			Assert.Equal("John", argument.Value);
		}

		[Fact]
		public void ParseStringStartsWithMethod()
		{
			var filter = FilterExpressionParser.Parse<Person>("Name.StartsWith(\"J\")");

			Assert.NotNull(filter);
			var functionFilter = Assert.IsType<FunctionFilter>(filter);
			Assert.Equal("StartsWith", functionFilter.FunctionName);

			var variable = Assert.IsType<VariableFilter>(functionFilter.Variable);
			Assert.Equal("Name", variable.VariableName);

			Assert.Single(functionFilter.Arguments);
			var argument = Assert.IsType<ConstantFilter>(functionFilter.Arguments[0]);
			Assert.Equal("J", argument.Value);
		}

		[Fact]
		public void ParseStringEndsWithMethod()
		{
			var filter = FilterExpressionParser.Parse<Person>("Name.EndsWith(\"son\")");

			Assert.NotNull(filter);
			var functionFilter = Assert.IsType<FunctionFilter>(filter);
			Assert.Equal("EndsWith", functionFilter.FunctionName);

			var variable = Assert.IsType<VariableFilter>(functionFilter.Variable);
			Assert.Equal("Name", variable.VariableName);

			Assert.Single(functionFilter.Arguments);
			var argument = Assert.IsType<ConstantFilter>(functionFilter.Arguments[0]);
			Assert.Equal("son", argument.Value);
		}

		#endregion

		#region Different Parameter Names Tests

		[Fact]
		public void ParseWithCustomParameterName()
		{
			var filter = FilterExpressionParser.Parse<Person>("person.Name == \"John\"", "person");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Name", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal("John", rightConstant.Value);
		}

		[Fact]
		public void ParseWithItParameterName()
		{
			var filter = FilterExpressionParser.Parse<Person>("it.Age > 25", "it");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.GreaterThan, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Age", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(25, rightConstant.Value);
		}

		#endregion

		#region Different Types Tests

		[Fact]
		public void ParseProductFilter()
		{
			var filter = FilterExpressionParser.Parse<Product>("Price > 100m && IsAvailable == true");

			Assert.NotNull(filter);
			var andFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.And, andFilter.FilterType);

			// Left side: Price > 100m
			var leftBinary = Assert.IsType<BinaryFilter>(andFilter.Left);
			Assert.Equal(FilterType.GreaterThan, leftBinary.FilterType);
			Assert.Equal("Price", ((VariableFilter)leftBinary.Left).VariableName);

			// Right side: IsAvailable == true
			var rightBinary = Assert.IsType<BinaryFilter>(andFilter.Right);
			Assert.Equal(FilterType.Equal, rightBinary.FilterType);
			Assert.Equal("IsAvailable", ((VariableFilter)rightBinary.Left).VariableName);
		}

		[Fact]
		public void ParseIntegerType()
		{
			var filter = FilterExpressionParser.Parse<int>("x > 5");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.GreaterThan, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("x", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(5, rightConstant.Value);
		}

		[Fact]
		public void ParseStringType()
		{
			var filter = FilterExpressionParser.Parse<string>("x.Contains(\"test\")");

			Assert.NotNull(filter);
			var functionFilter = Assert.IsType<FunctionFilter>(filter);
			Assert.Equal("Contains", functionFilter.FunctionName);

			var variable = Assert.IsType<VariableFilter>(functionFilter.Variable);
			Assert.Equal("x", variable.VariableName);
		}

		#endregion

		#region DateTime Tests

		[Fact]
		public void ParseDateTimeComparison()
		{
			var filter = FilterExpressionParser.Parse<Person>("BirthDate > DateTime(2000, 1, 1)");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.GreaterThan, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("BirthDate", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.IsType<DateTime>(rightConstant.Value);
		}

		#endregion

		#region Null Tests

		[Fact]
		public void ParseNullComparison()
		{
			var filter = FilterExpressionParser.Parse<Person>("Address == null");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Address", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Null(rightConstant.Value);
		}

		[Fact]
		public void ParseNotNullComparison()
		{
			var filter = FilterExpressionParser.Parse<Person>("Address != null");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.NotEqual, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Address", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Null(rightConstant.Value);
		}

		#endregion

		#region Error Cases Tests

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public void ParseThrowsArgumentExceptionForNullOrEmptyExpression(string expression)
		{
			var exception = Assert.Throws<ArgumentException>(() =>
				FilterExpressionParser.Parse<Person>(expression));

			Assert.Contains("Expression cannot be null or empty", exception.Message);
		}

		[Fact]
		public void ParseThrowsFilterExceptionForInvalidExpression()
		{
			var exception = Assert.Throws<FilterException>(() =>
				FilterExpressionParser.Parse<Person>("invalid syntax +++"));

			Assert.Contains("Unable to parse expression", exception.Message);
		}

		[Fact]
		public void ParseThrowsFilterExceptionForInvalidProperty()
		{
			var exception = Assert.Throws<FilterException>(() =>
				FilterExpressionParser.Parse<Person>("NonExistentProperty == \"value\""));

			Assert.Contains("Unable to parse expression", exception.Message);
		}

		#endregion

		#region Parsing Config Tests

		[Fact]
		public void ParseWithCustomConfig()
		{
			var config = new ParsingConfig
			{
				IsCaseSensitive = false
			};

			var filter = FilterExpressionParser.Parse<Person>("name == \"John\"", "x", config);

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);
		}

		#endregion

		#region Non-Generic Parse Tests

		[Fact]
		public void ParseNonGenericWithPersonType()
		{
			var filter = FilterExpressionParser.Parse("Name == \"John\"", typeof(Person));

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Equal, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Name", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal("John", rightConstant.Value);
		}

		[Fact]
		public void ParseNonGenericWithCustomParameterName()
		{
			var filter = FilterExpressionParser.Parse<Person>("person.Age > 25", "person");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.GreaterThan, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("Age", leftVariable.VariableName);

			var rightConstant = Assert.IsType<ConstantFilter>(binaryFilter.Right);
			Assert.Equal(25, rightConstant.Value);
		}

		#endregion

		#region Complex Expression Tests

		[Fact]
		public void ParseVeryComplexExpression()
		{
			var expression = "(Name.StartsWith(\"J\") && Age > 18 && Age < 65) || (IsActive == false && Salary > 50000)";
			var filter = FilterExpressionParser.Parse<Person>(expression);

			Assert.NotNull(filter);
			var orFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.Or, orFilter.FilterType);

			// Left side should be an AND operation
			var leftAnd = Assert.IsType<BinaryFilter>(orFilter.Left);
			Assert.Equal(FilterType.And, leftAnd.FilterType);

			// Right side should be an AND operation
			var rightAnd = Assert.IsType<BinaryFilter>(orFilter.Right);
			Assert.Equal(FilterType.And, rightAnd.FilterType);
		}

		[Fact]
		public void ParseNestedParentheses()
		{
			var expression = "((Age > 18 && Age < 30) || (Age > 50 && Age < 65)) && IsActive == true";
			var filter = FilterExpressionParser.Parse<Person>(expression);

			Assert.NotNull(filter);
			var andFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.And, andFilter.FilterType);

			// Left side should be an OR operation with nested conditions
			var leftOr = Assert.IsType<BinaryFilter>(andFilter.Left);
			Assert.Equal(FilterType.Or, leftOr.FilterType);

			// Right side should be IsActive == true
			var rightBinary = Assert.IsType<BinaryFilter>(andFilter.Right);
			Assert.Equal(FilterType.Equal, rightBinary.FilterType);
		}

		#endregion

		#region Edge Cases

		[Fact]
		public void ParseSingleVariable()
		{
			var filter = FilterExpressionParser.Parse<Person>("IsActive");

			Assert.NotNull(filter);
			var variableFilter = Assert.IsType<VariableFilter>(filter);
			Assert.Equal("IsActive", variableFilter.VariableName);
		}

		[Fact]
		public void ParseSingleConstant()
		{
			var filter = FilterExpressionParser.Parse<bool>("true");

			Assert.NotNull(filter);
			var constantFilter = Assert.IsType<ConstantFilter>(filter);
			Assert.Equal(true, constantFilter.Value);
		}

		[Fact]
		public void ParseParameterReference()
		{
			var filter = FilterExpressionParser.Parse<Person>("x != null", "x");

			Assert.NotNull(filter);
			var binaryFilter = Assert.IsType<BinaryFilter>(filter);
			Assert.Equal(FilterType.NotEqual, binaryFilter.FilterType);

			var leftVariable = Assert.IsType<VariableFilter>(binaryFilter.Left);
			Assert.Equal("x", leftVariable.VariableName);
		}

		#endregion
	}
}
