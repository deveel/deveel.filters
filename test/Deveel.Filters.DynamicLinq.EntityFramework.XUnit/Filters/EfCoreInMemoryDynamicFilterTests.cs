using Microsoft.EntityFrameworkCore;

namespace Deveel.Filters
{
	[Trait("Feature", "EF Core Dynamic Filter (InMemory)")]
	public class EfCoreInMemoryDynamicFilterTests : EfCoreDynamicFilterTestsBase
	{
		protected override DbContextOptions<TestDbContext> CreateOptions()
		{
			return new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
		}
	}
}

