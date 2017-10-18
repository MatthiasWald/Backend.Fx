namespace Backend.Fx.EfCorePersistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using DummyImpl;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Logging;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    
    public abstract class TestWithInMemorySqliteDbContext : IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<TestWithInMemorySqliteDbContext>();
        public SqliteConnection Connection { get; }
        public DbContextOptions<TestDbContext> DbContextOptions { get; }
        public TenantId TenantId { get; }
        public IClock Clock { get; } = new FrozenClock();

        protected TestWithInMemorySqliteDbContext()
        {
            TenantId = new TenantId(12);
            Connection = new SqliteConnection("DataSource=:memory:");
            Connection.Open();
            DbContextOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(Connection).Options;
        }

        protected void CreateDatabase()
        {
            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                dbContext.Database.EnsureCreated();
            }
        }

        protected void ExecuteNonQuery(string cmd)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = cmd;
                command.ExecuteNonQuery();
            }
        }

        protected T ExecuteScalar<T>(string cmd)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = cmd;
                return (T)command.ExecuteScalar();
            }
        }

        protected IEnumerable<T> ExecuteScalar<T>(string cmd, Func<IDataReader, T> forEachResultFunc)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = cmd;
                IDataReader reader = command.ExecuteReader();
                while (reader.NextResult())
                {
                    yield return forEachResultFunc(reader);
                }
            }
        }

        public void Dispose()
        {
            Connection?.Close();
            Connection?.Dispose();
        }
    }
}