﻿using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    using Environment.Authentication;

    public class TheReadonlyEfUnitOfWork : TestWithInMemorySqliteDbContext
    {
        public TheReadonlyEfUnitOfWork()
        {
            CreateDatabase();
        }

        [Fact]
        public void OpensTransaction()
        {
            using(var dbContext = new TestDbContext(DbContextOptions))
            {
                var sut = new ReadonlyEfUnitOfWork(dbContext, CurrentIdentityHolder.CreateSystem());
                
                Assert.Null(dbContext.Database.CurrentTransaction);
                sut.Begin();
                Assert.NotNull(dbContext.Database.CurrentTransaction);
                sut.Complete();
                Assert.Null(dbContext.Database.CurrentTransaction);
            }
        }

        [Fact]
        public void RollbacksTransactionOnComplete()
        {
            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                var sut = new ReadonlyEfUnitOfWork(dbContext, CurrentIdentityHolder.CreateSystem());
                sut.Begin();
                dbContext.Add(new Blogger(334, "Bratislav", "Metulsky"));
                sut.Complete();
                Assert.Null(dbContext.Database.CurrentTransaction);
                Assert.Empty(dbContext.Bloggers);
            }
        }

        [Fact]
        public void RollbacksTransactionOnDisposal()
        {
            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                var sut = new ReadonlyEfUnitOfWork(dbContext, CurrentIdentityHolder.CreateSystem());
                sut.Begin();
                dbContext.Add(new Blogger(335, "Bratislav", "Metulsky"));
                sut.Dispose();
                Assert.Null(dbContext.Database.CurrentTransaction);
                Assert.Empty(dbContext.Bloggers);
            }
        }
    }
}
