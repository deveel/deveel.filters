using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Deveel.Filters
{
	[Trait("Feature", "EF Core Dynamic Filter")]
	public class EfCoreDynamicFilterTests : IAsyncLifetime
	{
		private TestDbContext _db = null!;

		public async Task InitializeAsync()
		{
			var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_db = new TestDbContext(options);

			await SeedDataAsync();
		}

		public Task DisposeAsync()
		{
			_db.Dispose();
			return Task.CompletedTask;
		}

		private async Task SeedDataAsync()
		{
			var engineering = new Department { Id = 1, Name = "Engineering", Budget = 500000 };
			var marketing = new Department { Id = 2, Name = "Marketing", Budget = 200000 };

			_db.Departments.AddRange(engineering, marketing);

			var people = new[]
			{
				new Person { Id = 1, Name = "John Doe", Age = 30, IsActive = true, Salary = 75000, BirthDate = new DateTime(1994, 5, 15), DepartmentId = 1 },
				new Person { Id = 2, Name = "Jane Smith", Age = 25, IsActive = true, Salary = 60000, BirthDate = new DateTime(1999, 3, 22), DepartmentId = 1 },
				new Person { Id = 3, Name = "Bob Johnson", Age = 45, IsActive = false, Salary = 90000, BirthDate = new DateTime(1979, 11, 8), DepartmentId = 2 },
				new Person { Id = 4, Name = "Alice Brown", Age = 35, IsActive = true, Salary = 80000, BirthDate = new DateTime(1989, 7, 1), DepartmentId = 2 },
				new Person { Id = 5, Name = "Charlie Wilson", Age = 28, IsActive = false, Salary = 55000, BirthDate = new DateTime(1996, 1, 30), DepartmentId = null },
			};

			_db.People.AddRange(people);

			_db.Addresses.AddRange(
				new Address { Id = 1, Street = "123 Main St", City = "New York", Country = "USA", ZipCode = 10001, PersonId = 1 },
				new Address { Id = 2, Street = "456 Oak Ave", City = "Boston", Country = "USA", ZipCode = 02101, PersonId = 1 },
				new Address { Id = 3, Street = "789 Pine Rd", City = "Chicago", Country = "USA", ZipCode = 60601, PersonId = 2 },
				new Address { Id = 4, Street = "10 Downing Street", City = "London", Country = "UK", ZipCode = 0, PersonId = 3 }
			);

			_db.Products.AddRange(
				new Product { Id = 1, Name = "Laptop", Price = 999.99m, CategoryId = 1, IsAvailable = true },
				new Product { Id = 2, Name = "Mouse", Price = 29.99m, CategoryId = 1, IsAvailable = true },
				new Product { Id = 3, Name = "Keyboard", Price = 79.99m, CategoryId = 1, IsAvailable = false },
				new Product { Id = 4, Name = "Desk", Price = 299.99m, CategoryId = 2, IsAvailable = true }
			);

			await _db.SaveChangesAsync();
		}

		#region Simple Equality

		[Fact]
		public async Task Filter_StringEquality_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("Name"),
				Filter.Constant("John Doe"),
				FilterType.Equal);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Single(results);
			Assert.Equal("John Doe", results[0].Name);
		}

		[Fact]
		public async Task Filter_BooleanEquality_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("IsActive"),
				Filter.Constant(true),
				FilterType.Equal);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(3, results.Count);
			Assert.All(results, p => Assert.True(p.IsActive));
		}

		#endregion

		#region Comparison Operators

		[Theory]
		[InlineData(FilterType.GreaterThan, 30, 2)]
		[InlineData(FilterType.GreaterThanOrEqual, 30, 3)]
		[InlineData(FilterType.LessThan, 30, 2)]
		[InlineData(FilterType.LessThanOrEqual, 30, 3)]
		[InlineData(FilterType.Equal, 30, 1)]
		[InlineData(FilterType.NotEqual, 30, 4)]
		public async Task Filter_IntComparison_ShouldQueryDbSet(FilterType filterType, int value, int expectedCount)
		{
			var filter = Filter.Binary(
				Filter.Variable("Age"),
				Filter.Constant(value),
				filterType);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(expectedCount, results.Count);
		}

		[Fact]
		public async Task Filter_DoubleGreaterThan_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("Salary"),
				Filter.Constant(70000.0),
				FilterType.GreaterThan);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(3, results.Count);
			Assert.All(results, p => Assert.True(p.Salary > 70000));
		}

		#endregion

		#region Logical Operators

		[Fact]
		public async Task Filter_And_ShouldQueryDbSet()
		{
			// IsActive == true && Age > 30
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("IsActive"), Filter.Constant(true), FilterType.Equal),
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(30), FilterType.GreaterThan),
				FilterType.And);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Single(results);
			Assert.Equal("Alice Brown", results[0].Name);
		}

		[Fact]
		public async Task Filter_Or_ShouldQueryDbSet()
		{
			// Age < 26 || Age > 40
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(26), FilterType.LessThan),
				Filter.Binary(Filter.Variable("Age"), Filter.Constant(40), FilterType.GreaterThan),
				FilterType.Or);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(2, results.Count);
		}

		[Fact]
		public async Task Filter_Not_ShouldQueryDbSet()
		{
			// !(IsActive == true)
			var filter = Filter.Unary(
				Filter.Binary(Filter.Variable("IsActive"), Filter.Constant(true), FilterType.Equal),
				FilterType.Not);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(2, results.Count);
			Assert.All(results, p => Assert.False(p.IsActive));
		}

		#endregion

		#region Complex Filters

		[Fact]
		public async Task Filter_ComplexNested_ShouldQueryDbSet()
		{
			// (Age >= 25 && Age <= 35) && IsActive == true
			var filter = Filter.Binary(
				Filter.Binary(
					Filter.Binary(Filter.Variable("Age"), Filter.Constant(25), FilterType.GreaterThanOrEqual),
					Filter.Binary(Filter.Variable("Age"), Filter.Constant(35), FilterType.LessThanOrEqual),
					FilterType.And),
				Filter.Binary(Filter.Variable("IsActive"), Filter.Constant(true), FilterType.Equal),
				FilterType.And);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(3, results.Count);
			Assert.All(results, p =>
			{
				Assert.True(p.Age >= 25 && p.Age <= 35);
				Assert.True(p.IsActive);
			});
		}

		#endregion

		#region Navigation Properties

		[Fact]
		public async Task Filter_NavigationProperty_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("Department.Name"),
				Filter.Constant("Engineering"),
				FilterType.Equal);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People
				.Include(p => p.Department)
				.Where(lambda)
				.ToListAsync();

			Assert.Equal(2, results.Count);
			Assert.All(results, p => Assert.Equal("Engineering", p.Department!.Name));
		}

		[Fact]
		public async Task Filter_NavigationPropertyIntComparison_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("Department.Budget"),
				Filter.Constant(300000),
				FilterType.GreaterThan);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People
				.Include(p => p.Department)
				.Where(lambda)
				.ToListAsync();

			Assert.Equal(2, results.Count);
			Assert.All(results, p => Assert.True(p.Department!.Budget > 300000));
		}

		#endregion

		#region Null Comparisons

		[Fact]
		public async Task Filter_NullComparison_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("DepartmentId"),
				Filter.Constant(null),
				FilterType.Equal);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Single(results);
			Assert.Equal("Charlie Wilson", results[0].Name);
		}

		[Fact]
		public async Task Filter_NotNullComparison_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("DepartmentId"),
				Filter.Constant(null),
				FilterType.NotEqual);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(4, results.Count);
		}

		#endregion

		#region String Functions

		[Fact]
		public async Task Filter_StringContains_ShouldQueryDbSet()
		{
			var filter = Filter.Function(
				Filter.Variable("Name"),
				"Contains",
				new[] { Filter.Constant("John") });

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(2, results.Count); // John Doe, Bob Johnson
		}

		[Fact]
		public async Task Filter_StringStartsWith_ShouldQueryDbSet()
		{
			var filter = Filter.Function(
				Filter.Variable("Name"),
				"StartsWith",
				new[] { Filter.Constant("J") });

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(2, results.Count); // John Doe, Jane Smith
		}

		[Fact]
		public async Task Filter_StringEndsWith_ShouldQueryDbSet()
		{
			var filter = Filter.Function(
				Filter.Variable("Name"),
				"EndsWith",
				new[] { Filter.Constant("son") });

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(2, results.Count); // Bob Johnson, Charlie Wilson
			Assert.All(results, p => Assert.EndsWith("son", p.Name));
		}

		#endregion

		#region DateTime Comparisons

		[Fact]
		public async Task Filter_DateTimeComparison_ShouldQueryDbSet()
		{
			var cutoff = new DateTime(1995, 1, 1);
			var filter = Filter.Binary(
				Filter.Variable("BirthDate"),
				Filter.Constant(cutoff),
				FilterType.GreaterThan);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(2, results.Count); // Jane Smith (1999), Charlie Wilson (1996)
			Assert.All(results, p => Assert.True(p.BirthDate > cutoff));
		}

		#endregion

		#region Product Queries

		[Fact]
		public async Task Filter_DecimalComparison_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Binary(Filter.Variable("Price"), Filter.Constant(100m), FilterType.GreaterThan),
				Filter.Binary(Filter.Variable("IsAvailable"), Filter.Constant(true), FilterType.Equal),
				FilterType.And);

			var lambda = filter.AsDynamicLambda<Product>();
			var results = await _db.Products.Where(lambda).ToListAsync();

			Assert.Equal(2, results.Count); // Laptop, Desk
			Assert.All(results, p =>
			{
				Assert.True(p.Price > 100m);
				Assert.True(p.IsAvailable);
			});
		}

		[Fact]
		public async Task Filter_ProductByCategory_ShouldQueryDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("CategoryId"),
				Filter.Constant(1),
				FilterType.Equal);

			var lambda = filter.AsDynamicLambda<Product>();
			var results = await _db.Products.Where(lambda).ToListAsync();

			Assert.Equal(3, results.Count);
		}

		#endregion

		#region Combined Filter and OrderBy

		[Fact]
		public async Task Filter_WithOrderBy_ShouldQueryAndSortDbSet()
		{
			var filter = Filter.Binary(
				Filter.Variable("IsActive"),
				Filter.Constant(true),
				FilterType.Equal);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People
				.Where(lambda)
				.OrderBy(p => p.Age)
				.ToListAsync();

			Assert.Equal(3, results.Count);
			Assert.Equal("Jane Smith", results[0].Name);
			Assert.Equal("John Doe", results[1].Name);
			Assert.Equal("Alice Brown", results[2].Name);
		}

		#endregion

		#region Empty Results

		[Fact]
		public async Task Filter_NoMatches_ShouldReturnEmpty()
		{
			var filter = Filter.Binary(
				Filter.Variable("Age"),
				Filter.Constant(100),
				FilterType.GreaterThan);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Empty(results);
		}

		#endregion

		#region Constant Filter

		[Fact]
		public async Task Filter_ConstantTrue_ShouldReturnAll()
		{
			var filter = Filter.Constant(true);

			var lambda = filter.AsDynamicLambda<Person>();
			var results = await _db.People.Where(lambda).ToListAsync();

			Assert.Equal(5, results.Count);
		}

		#endregion
	}
}



