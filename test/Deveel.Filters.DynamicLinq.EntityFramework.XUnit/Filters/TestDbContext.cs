using Microsoft.EntityFrameworkCore;

namespace Deveel.Filters
{
	#region Test Models

	public class Person
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public int Age { get; set; }
		public bool IsActive { get; set; }
		public double Salary { get; set; }
		public DateTime BirthDate { get; set; }
		public int? DepartmentId { get; set; }
		public Department? Department { get; set; }
		public List<Address> Addresses { get; set; } = new();
	}

	public class Address
	{
		public int Id { get; set; }
		public string Street { get; set; } = "";
		public string City { get; set; } = "";
		public string Country { get; set; } = "";
		public int ZipCode { get; set; }
		public int PersonId { get; set; }
		public Person? Person { get; set; }
	}

	public class Department
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public int Budget { get; set; }
		public List<Person> Employees { get; set; } = new();
	}

	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public decimal Price { get; set; }
		public int CategoryId { get; set; }
		public bool IsAvailable { get; set; }
	}

	#endregion

	public class TestDbContext : DbContext
	{
		public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

		public DbSet<Person> People => Set<Person>();
		public DbSet<Address> Addresses => Set<Address>();
		public DbSet<Department> Departments => Set<Department>();
		public DbSet<Product> Products => Set<Product>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Person>(e =>
			{
				e.HasKey(p => p.Id);
				e.HasOne(p => p.Department)
				 .WithMany(d => d.Employees)
				 .HasForeignKey(p => p.DepartmentId);
				e.HasMany(p => p.Addresses)
				 .WithOne(a => a.Person)
				 .HasForeignKey(a => a.PersonId);
			});

			modelBuilder.Entity<Address>(e =>
			{
				e.HasKey(a => a.Id);
			});

			modelBuilder.Entity<Department>(e =>
			{
				e.HasKey(d => d.Id);
			});

			modelBuilder.Entity<Product>(e =>
			{
				e.HasKey(p => p.Id);
			});
		}
	}
}
