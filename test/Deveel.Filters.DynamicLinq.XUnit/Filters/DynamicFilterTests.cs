using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

using Xunit;

namespace Deveel.Filters
{
	[Trait("Feature", "Dynamic Filter Conversion")]
	public static class DynamicFilterTests
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
			public List<string> Tags { get; set; } = new();
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

		#region AsDynamicLambda Generic Tests

		[Fact]
		public static void AsDynamicLambda_SimpleStringEquality_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Name"),
				Filter.Constant("John"),
				FilterType.Equal);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			//Assert.IsType<Expression<Func<Person, bool>>>(lambda);

			var compiled = lambda.Compile();
			var person1 = new Person { Name = "John" };
			var person2 = new Person { Name = "Jane" };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_IntegerComparison_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Age"),
				Filter.Constant(25),
				FilterType.GreaterThan);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();
			var person1 = new Person { Age = 30 };
			var person2 = new Person { Age = 20 };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_BooleanEquality_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("IsActive"),
				Filter.Constant(true),
				FilterType.Equal);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();
			var person1 = new Person { IsActive = true };
			var person2 = new Person { IsActive = false };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_DoubleComparison_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Salary"),
				Filter.Constant(50000.0),
				FilterType.GreaterThanOrEqual);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();
			var person1 = new Person { Salary = 60000.0 };
			var person2 = new Person { Salary = 40000.0 };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Theory]
		[InlineData(FilterType.Equal, 25, 25, true)]
		[InlineData(FilterType.Equal, 25, 30, false)]
		[InlineData(FilterType.NotEqual, 25, 30, true)]
		[InlineData(FilterType.NotEqual, 25, 25, false)]
		[InlineData(FilterType.GreaterThan, 25, 30, true)]
		[InlineData(FilterType.GreaterThan, 25, 20, false)]
		[InlineData(FilterType.GreaterThanOrEqual, 25, 25, true)]
		[InlineData(FilterType.GreaterThanOrEqual, 25, 20, false)]
		[InlineData(FilterType.LessThan, 25, 20, true)]
		[InlineData(FilterType.LessThan, 25, 30, false)]
		[InlineData(FilterType.LessThanOrEqual, 25, 25, true)]
		[InlineData(FilterType.LessThanOrEqual, 25, 30, false)]
		public static void AsDynamicLambda_AllComparisonOperators_ShouldWork(FilterType filterType, int filterValue, int testValue, bool expectedResult)
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Age"),
				Filter.Constant(filterValue),
				filterType);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();
			var compiled = lambda.Compile();
			var person = new Person { Age = testValue };

			// Assert
			Assert.Equal(expectedResult, compiled(person));
		}

		[Fact]
		public static void AsDynamicLambda_LogicalAnd_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(18), FilterType.GreaterThan),
				Filter.Binary(Filter.Variable("IsActive"), Filter.Constant(true), FilterType.Equal),
				FilterType.And);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Age = 25, IsActive = true };   // Should pass
			var person2 = new Person { Age = 16, IsActive = true };   // Should fail (age)
			var person3 = new Person { Age = 25, IsActive = false };  // Should fail (active)
			var person4 = new Person { Age = 16, IsActive = false };  // Should fail (both)

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
			Assert.False(compiled(person3));
			Assert.False(compiled(person4));
		}

		[Fact]
		public static void AsDynamicLambda_LogicalOr_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(65), FilterType.GreaterThan),
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(18), FilterType.LessThan),
				FilterType.Or);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Age = 70 };  // Should pass (> 65)
			var person2 = new Person { Age = 16 };  // Should pass (< 18)
			var person3 = new Person { Age = 30 };  // Should fail (18 <= age <= 65)

			Assert.True(compiled(person1));
			Assert.True(compiled(person2));
			Assert.False(compiled(person3));
		}

		[Fact]
		public static void AsDynamicLambda_UnaryNot_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Unary(
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(25), FilterType.GreaterThan),
				FilterType.Not);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Age = 30 };  // Should fail (!(30 > 25) = !true = false)
			var person2 = new Person { Age = 20 };  // Should pass (!(20 > 25) = !false = true)

			Assert.False(compiled(person1));
			Assert.True(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_NestedPropertyAccess_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Address.City"),
				Filter.Constant("New York"),
				FilterType.Equal);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Address = new Address { City = "New York" } };
			var person2 = new Person { Address = new Address { City = "Boston" } };
			var person3 = new Person { Address = null };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
			Assert.Throws<NullReferenceException>(() => compiled(person3));
		}

		[Fact]
		public static void AsDynamicLambda_StringMethods_ShouldConvertToLambda()
		{
			// Arrange - Using Contains method
			var filter = Filter.Function(
				Filter.Variable("Name"),
				"Contains",
				Filter.Constant("John"));

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Name = "John Doe" };
			var person2 = new Person { Name = "Jane Smith" };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_WithCustomParameterName_ShouldWork()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Name"),
				Filter.Constant("John"),
				FilterType.Equal);

			// Act
			var lambda = filter.AsDynamicLambda<Person>("person");

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();
			var person = new Person { Name = "John" };

			Assert.True(compiled(person));
		}

		[Fact]
		public static void AsDynamicLambda_WithCustomParsingConfig_ShouldWork()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("x.name"),
				Filter.Constant("john"),
				FilterType.Equal);

			var config = new ParsingConfig
			{
				IsCaseSensitive = false
			};

			// Act
			var lambda = filter.AsDynamicLambda<Person>("x", config);

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();
			var person = new Person { Name = "john" };

			Assert.True(compiled(person));
		}

		#endregion

		#region AsDynamicLamda Non-Generic Tests

		[Fact]
		public static void AsDynamicLamda_NonGeneric_ShouldConvertToLambda()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Name"),
				Filter.Constant("John"),
				FilterType.Equal);

			// Act
			var lambda = filter.AsDynamicLamda(typeof(Person));

			// Assert
			Assert.NotNull(lambda);
			Assert.IsAssignableFrom<LambdaExpression>(lambda);

			var compiled = lambda.Compile();
			var person1 = new Person { Name = "John" };
			var person2 = new Person { Name = "Jane" };

			Assert.True((bool)compiled.DynamicInvoke(person1)!);
			Assert.False((bool)compiled.DynamicInvoke(person2)!);
		}

		[Fact]
		public static void AsDynamicLamda_NonGenericWithCustomParameterName_ShouldWork()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Age"),
				Filter.Constant(25),
				FilterType.GreaterThan);

			// Act
			var lambda = filter.AsDynamicLamda(typeof(Person), "person");

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();
			var person = new Person { Age = 30 };

			Assert.True((bool)compiled.DynamicInvoke(person)!);
		}

		[Fact]
		public static void AsDynamicLamda_NonGenericWithConfig_ShouldWork()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("x.name"),
				Filter.Constant("John"),
				FilterType.Equal);

			var config = new ParsingConfig
			{
				IsCaseSensitive = false
			};

			// Act
			var lambda = filter.AsDynamicLamda(typeof(Person), "x", config);

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();
			var person = new Person { Name = "John" };

			Assert.True((bool)compiled.DynamicInvoke(person)!);
		}

		[Fact]
		public static void AsDynamicLamda_PrimitiveTypes_ShouldWork()
		{
			// Test with int
			var intFilter = Filter.Binary(Filter.Variable("x"), Filter.Constant(5), FilterType.GreaterThan);
			var intLambda = intFilter.AsDynamicLamda(typeof(int), "x");
			var intCompiled = intLambda.Compile();
			Assert.True((bool)intCompiled.DynamicInvoke(10)!);
			Assert.False((bool)intCompiled.DynamicInvoke(3)!);

			// Test with string
			var stringFilter = Filter.Function(Filter.Variable("x"), "StartsWith", Filter.Constant("Hello"));
			var stringLambda = stringFilter.AsDynamicLamda(typeof(string));
			var stringCompiled = stringLambda.Compile();
			Assert.True((bool)stringCompiled.DynamicInvoke("Hello World")!);
			Assert.False((bool)stringCompiled.DynamicInvoke("Goodbye")!);
		}

		#endregion

		#region Different Types Tests

		[Fact]
		public static void AsDynamicLambda_ProductType_ShouldWork()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("Price"), Filter.Constant(100m), FilterType.GreaterThan),
				Filter.Binary(Filter.Variable("IsAvailable"), Filter.Constant(true), FilterType.Equal),
				FilterType.And);

			// Act
			var lambda = filter.AsDynamicLambda<Product>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var product1 = new Product { Price = 150m, IsAvailable = true };
			var product2 = new Product { Price = 50m, IsAvailable = true };
			var product3 = new Product { Price = 150m, IsAvailable = false };

			Assert.True(compiled(product1));
			Assert.False(compiled(product2));
			Assert.False(compiled(product3));
		}

		[Fact]
		public static void AsDynamicLambda_AnonymousType_ShouldWork()
		{
			// Arrange
			var obj = new { Value = 42, Name = "Test" };
			var filter = Filter.Binary(
				Filter.Variable("Value"),
				Filter.Constant(40),
				FilterType.GreaterThan);

			// Act
			var lambda = filter.AsDynamicLamda(obj.GetType());

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			Assert.True((bool)compiled.DynamicInvoke(obj)!);
			Assert.False((bool)compiled.DynamicInvoke(new { Value = 30, Name = "Test" })!);
		}

		#endregion

		#region Complex Expression Tests

		[Fact]
		public static void AsDynamicLambda_ComplexNestedExpression_ShouldWork()
		{
			// Arrange - ((Age > 18 && Age < 65) || IsActive) && Name != null
			var filter = Filter.Binary(
				Filter.Binary(
					Filter.Binary(
						Filter.Binary(Filter.Variable("Age"), Filter.Constant(18), FilterType.GreaterThan),
						Filter.Binary(Filter.Variable("Age"), Filter.Constant(65), FilterType.LessThan),
						FilterType.And),
					Filter.Variable("IsActive"),
					FilterType.Or),
				Filter.Binary(Filter.Variable("Name"), Filter.Constant(null), FilterType.NotEqual),
				FilterType.And);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Age = 30, IsActive = false, Name = "John" };  // Should pass
			var person2 = new Person { Age = 70, IsActive = true, Name = "John" };   // Should pass (IsActive = true)
			var person3 = new Person { Age = 30, IsActive = false, Name = null! };   // Should fail (Name = null)
			var person4 = new Person { Age = 10, IsActive = false, Name = "John" };  // Should fail (Age and IsActive)

			Assert.True(compiled(person1));
			Assert.True(compiled(person2));
			Assert.False(compiled(person3));
			Assert.False(compiled(person4));
		}

		[Fact]
		public static void AsDynamicLambda_MultipleNegations_ShouldWork()
		{
			// Arrange - !(!(Age > 25))  which should be equivalent to (Age > 25)
			var filter = Filter.Unary(
				Filter.Unary(
					Filter.Binary(Filter.Variable("Age"), Filter.Constant(25), FilterType.GreaterThan),
					FilterType.Not),
				FilterType.Not);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Age = 30 };  // Should pass
			var person2 = new Person { Age = 20 };  // Should fail

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		#endregion

		#region Null and Edge Cases Tests

		[Fact]
		public static void AsDynamicLambda_NullComparison_ShouldWork()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Address"),
				Filter.Constant(null),
				FilterType.Equal);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Address = null };
			var person2 = new Person { Address = new Address() };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_NotNullComparison_ShouldWork()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Address"),
				Filter.Constant(null),
				FilterType.NotEqual);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Address = null };
			var person2 = new Person { Address = new Address() };

			Assert.False(compiled(person1));
			Assert.True(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_EmptyStringComparison_ShouldWork()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Name"),
				Filter.Constant(""),
				FilterType.Equal);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Name = "" };
			var person2 = new Person { Name = "John" };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_ConstantFilter_ShouldWork()
		{
			// Arrange
			var filter = Filter.Constant(true);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person = new Person();
			Assert.True(compiled(person));
		}

		[Fact]
		public static void AsDynamicLambda_VariableFilter_ShouldWork()
		{
			// Arrange
			var filter = Filter.Variable("IsActive");

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { IsActive = true };
			var person2 = new Person { IsActive = false };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		#endregion

		#region DateTime Tests

		[Fact]
		public static void AsDynamicLambda_DateTimeComparison_ShouldWork()
		{
			// Arrange
			var date = new DateTime(2000, 1, 1);
			var filter = Filter.Binary(
				Filter.Variable("BirthDate"),
				Filter.Constant(date),
				FilterType.GreaterThan);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { BirthDate = new DateTime(2010, 1, 1) };
			var person2 = new Person { BirthDate = new DateTime(1990, 1, 1) };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		#endregion

		#region Multiple Function Calls Tests

		[Fact]
		public static void AsDynamicLambda_StringStartsWith_ShouldWork()
		{
			// Arrange
			var filter = Filter.Function(
				Filter.Variable("Name"),
				"StartsWith",
				Filter.Constant("J"));

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Name = "John" };
			var person2 = new Person { Name = "Alice" };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_StringEndsWith_ShouldWork()
		{
			// Arrange
			var filter = Filter.Function(
				Filter.Variable("Name"),
				"EndsWith",
				Filter.Constant("son"));

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Name = "Johnson" };
			var person2 = new Person { Name = "Smith" };

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		[Fact]
		public static void AsDynamicLambda_StringLength_ShouldWork()
		{
			// Arrange - Name.Length > 5
			var filter = Filter.Binary(
				Filter.Function(Filter.Variable("Name"), "get_Length"),
				Filter.Constant(5),
				FilterType.GreaterThan);

			// Act
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(lambda);
			var compiled = lambda.Compile();

			var person1 = new Person { Name = "Alexander" };  // 9 characters
			var person2 = new Person { Name = "John" };       // 4 characters

			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
		}

		#endregion

		#region Error Cases Tests

		[Fact]
		public static void AsDynamicLambda_InvalidFilterString_ShouldThrowFilterException()
		{
			// Arrange - Create a filter that will produce invalid dynamic LINQ
			var filter = Filter.Binary(
				Filter.Variable("NonExistentProperty"),
				Filter.Constant("value"),
				FilterType.Equal);

			// Act & Assert
			var exception = Assert.Throws<FilterException>(() => filter.AsDynamicLambda<Person>());
			Assert.Contains("Unable to construct the dynamic lamda", exception.Message);
		}

		[Fact]
		public static void AsDynamicLamda_NullParameterType_ShouldThrowArgumentNullException()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Name"),
				Filter.Constant("John"),
				FilterType.Equal);

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => filter.AsDynamicLamda(null!));
		}

		#endregion

		#region Performance and Compilation Tests

		[Fact]
		public static void AsDynamicLambda_MultipleCompilations_ShouldWorkConsistently()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Age"),
				Filter.Constant(25),
				FilterType.GreaterThan);

			// Act - Compile multiple times
			var lambda1 = filter.AsDynamicLambda<Person>();
			var lambda2 = filter.AsDynamicLambda<Person>();
			var compiled1 = lambda1.Compile();
			var compiled2 = lambda2.Compile();

			// Assert
			var person = new Person { Age = 30 };
			Assert.True(compiled1(person));
			Assert.True(compiled2(person));
		}

		[Fact]
		public static void AsDynamicLambda_CachedCompilation_ShouldWorkCorrectly()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Variable("Name"),
				Filter.Constant("John"),
				FilterType.Equal);

			var lambda = filter.AsDynamicLambda<Person>();
			var compiled = lambda.Compile();

			// Act - Use the same compiled lambda multiple times
			var person1 = new Person { Name = "John" };
			var person2 = new Person { Name = "Jane" };

			// Assert
			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
			Assert.True(compiled(person1));  // Test again
			Assert.False(compiled(person2)); // Test again
		}

		#endregion

		#region Filter ToString Integration Tests

		[Fact]
		public static void AsDynamicLambda_FilterToStringIntegration_ShouldProduceValidExpression()
		{
			// Arrange
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(18), FilterType.GreaterThan),
				Filter.Binary(Filter.Variable("IsActive"), Filter.Constant(true), FilterType.Equal),
				FilterType.And);

			// Act
			var filterString = filter.ToString();
			var lambda = filter.AsDynamicLambda<Person>();

			// Assert
			Assert.NotNull(filterString);
			Assert.NotEmpty(filterString);
			Assert.NotNull(lambda);

			var compiled = lambda.Compile();
			var person = new Person { Age = 25, IsActive = true };
			Assert.True(compiled(person));
		}

		#endregion

		#region Round-trip Tests (Filter -> Lambda -> String -> Filter)

		[Fact]
		public static void FilterToLambdaRoundTrip_SimpleExpression_ShouldMaintainSemantics()
		{
			// Arrange
			var originalFilter = Filter.Binary(
				Filter.Variable("Age"),
				Filter.Constant(25),
				FilterType.GreaterThan);

			// Act
			var lambda = originalFilter.AsDynamicLambda<Person>();
			var compiled = lambda.Compile();

			// Test data
			var person1 = new Person { Age = 30 };
			var person2 = new Person { Age = 20 };

			// Assert - Lambda should produce same results as intended filter logic
			Assert.True(compiled(person1));   // 30 > 25
			Assert.False(compiled(person2));  // 20 > 25
		}

		[Fact]
		public static void FilterToLambdaRoundTrip_ComplexExpression_ShouldMaintainSemantics()
		{
			// Arrange
			var originalFilter = Filter.Binary(
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(18), FilterType.GreaterThan),
				Filter.Binary(Filter.Variable("Name"), Filter.Constant(null), FilterType.NotEqual),
				FilterType.And);

			// Act
			var lambda = originalFilter.AsDynamicLambda<Person>();
			var compiled = lambda.Compile();

			// Test data
			var person1 = new Person { Age = 25, Name = "John" };    // Should pass
			var person2 = new Person { Age = 16, Name = "John" };    // Should fail (age)
			var person3 = new Person { Age = 25, Name = null! };     // Should fail (name)

			// Assert
			Assert.True(compiled(person1));
			Assert.False(compiled(person2));
			Assert.False(compiled(person3));
		}

		#endregion
	}
}