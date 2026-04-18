using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Deveel.Filters
{
    [Trait("Feature", "EF Core Dynamic Filter (SQLite)")]
    public class EfCoreSqliteDynamicFilterTests : EfCoreDynamicFilterTestsBase, IDisposable
    {
        private readonly SqliteConnection _connection;

        public EfCoreSqliteDynamicFilterTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override DbContextOptions<TestDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(_connection)
                .Options;
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
