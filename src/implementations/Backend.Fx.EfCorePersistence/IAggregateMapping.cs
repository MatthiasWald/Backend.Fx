﻿namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Microsoft.EntityFrameworkCore;

    public interface IAggregateMapping
    {
        void ApplyEfMapping(ModelBuilder modelBuilder);
    }

    public interface IAggregateMapping<T> : IAggregateMapping where T : AggregateRoot
    {
         IEnumerable<Expression<Func<T, object>>> IncludeDefinitions { get; }
    }
}
