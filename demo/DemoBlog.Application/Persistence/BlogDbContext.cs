﻿namespace DemoBlog.Persistence
{
    using System;
    using System.Linq;
    using Backend.Fx.BuildingBlocks;
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.MultiTenancy;
    using Backend.Fx.Logging;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    public class BlogDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<BlogDbContext>();

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Blogger> Bloggers { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        public BlogDbContext(DbContextOptions options) : base(options)
        {
            IServiceProvider serviceProvider = this.GetInfrastructure<IServiceProvider>();
            var loggerFactory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            loggerFactory.AddProvider(new BackendFxLoggerProvider());
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            this.ApplyAggregateRootMappings(builder);
            builder.RegisterRowVersionProperty();
            builder.RegisterEntityIdAsNeverGenerated();
        }

        public override int SaveChanges()
        {
            AggregateRoot[] aggregatesWithoutTenantId = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .OfType<AggregateRoot>()
                .Where(ent => ent.TenantId == 0)
                .ToArray();
            if (aggregatesWithoutTenantId.Length > 0)
            {
                throw new InvalidOperationException($"Attempt to save aggregate root entities without tenant id: {string.Join(",", aggregatesWithoutTenantId.Select(agg => agg.DebuggerDisplay))}");
            }
            this.TraceChangeTrackerState();
            using (Logger.DebugDuration("Saving Changes"))
            {
                return base.SaveChanges();
            }
        }
    }
}
