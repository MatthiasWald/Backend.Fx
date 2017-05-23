﻿namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using BuildingBlocks;
    using Extensions;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public static class DbContextExtensions
    {
        private static readonly ILogger Logger = LogManager.Create(typeof(DbContextExtensions));

        public static void ApplyAggregateRootMappings(this DbContext dbContext, ModelBuilder modelBuilder)
        {
            //CAVE: IAggregateRootMapping implementations must reside in the same assembly as the Applications DbContext-type
            var aggregateDefinitionTypeInfos = dbContext
                .GetType()
                .GetTypeInfo()
                .Assembly
                .ExportedTypes
                .Select(t => t.GetTypeInfo())
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(IAggregateDefinition).GetTypeInfo().IsAssignableFrom(t));
            foreach (var typeInfo in aggregateDefinitionTypeInfos)
            {
                IAggregateDefinition aggregateDefinition = (IAggregateDefinition)Activator.CreateInstance(typeInfo.AsType());
                aggregateDefinition.ApplyEfMapping(modelBuilder);
            }
        }

        public static void UpdateTrackingProperties(this DbContext dbContext, string userId, DateTime utcNow)
        {
            userId = userId ?? "anonymous";
            var isTraceEnabled = Logger.IsTraceEnabled();
            int count = 0;
            dbContext.ChangeTracker
                .Entries<Entity>()
                .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified)
                .ForAll(entry =>
                {
                    try
                    {
                        count++;
                        Entity entity = entry.Entity;

                        if (entry.State == EntityState.Added)
                        {
                            if (isTraceEnabled)
                            {
                                Logger.Trace("tracking that {0}[{1}] was created by {2} at {3:T} UTC", entity.GetType().Name, entity.Id, userId, utcNow);
                            }
                            entity.SetCreatedProperties(userId, utcNow);
                        }
                        else
                        {
                            if (isTraceEnabled)
                            {
                                Logger.Trace("tracking that {0}[{1}] was modified by {2} at {3:T} UTC", entity.GetType().Name, entity.Id, userId, utcNow);
                            }
                            entity.SetModifiedProperties(userId, utcNow);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Updating tracking properties failed");
                        throw;
                    }
                });
            if (count > 0)
            {
                Logger.Debug($"Tracked {count} entities as created/changed on {utcNow:u} by {userId}");
            }
        }

        public static void TraceChangeTrackerState(this DbContext dbContext)
        {
            if (Logger.IsTraceEnabled())
            {
                try
                {
                    var added = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added).ToArray();
                    var modified = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified).ToArray();
                    var deleted = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Deleted).ToArray();
                    var unchanged = dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Unchanged).ToArray();

                    var stateDumpBuilder = new StringBuilder();
                    stateDumpBuilder.AppendFormat("{0} entities added{1}{2}", added.Length, deleted.Length == 0 ? "." : ":", Environment.NewLine);
                    foreach (var entry in added)
                    {
                        stateDumpBuilder.AppendFormat("added: {0}[{1}]{2}", entry.Entity.GetType().Name, GetPrimaryKeyValue(entry), Environment.NewLine);
                    }
                    stateDumpBuilder.AppendFormat("{0} entities modified{1}{2}", modified.Length, deleted.Length == 0 ? "." : ":", Environment.NewLine);
                    foreach (var entry in modified)
                    {
                        stateDumpBuilder.AppendFormat("modified: {0}[{1}]{2}", entry.Entity.GetType().Name, GetPrimaryKeyValue(entry), Environment.NewLine);
                    }
                    stateDumpBuilder.AppendFormat("{0} entities deleted{1}{2}", deleted.Length, deleted.Length == 0 ? "." : ":", Environment.NewLine);
                    foreach (var entry in deleted)
                    {
                        stateDumpBuilder.AppendFormat("deleted: {0}[{1}]{2}", entry.Entity.GetType().Name, GetPrimaryKeyValue(entry), Environment.NewLine);
                    }
                    stateDumpBuilder.AppendFormat("{0} entities unchanged{1}{2}", unchanged.Length, deleted.Length == 0 ? "." : ":", Environment.NewLine);
                    foreach (var entry in unchanged)
                    {
                        stateDumpBuilder.AppendFormat("unchanged: {0}[{1}]{2}", entry.Entity.GetType().Name, GetPrimaryKeyValue(entry), Environment.NewLine);
                    }
                    Logger.Trace(stateDumpBuilder.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Change tracker state could not be dumped");
                }
            }
        }

        private static string GetPrimaryKeyValue(EntityEntry entry)
        {
            return (entry.Entity as Entity)?.Id.ToString(CultureInfo.InvariantCulture) ?? "?";
        }
    }
}
