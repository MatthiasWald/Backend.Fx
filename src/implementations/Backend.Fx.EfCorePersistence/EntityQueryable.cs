﻿namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Microsoft.EntityFrameworkCore;
    
    public class EntityQueryable<TEntity> : IQueryable<TEntity> where TEntity : Entity
    {
        private readonly DbContext _dbContext;
        
        public EntityQueryable(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return InnerQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)InnerQueryable).GetEnumerator();
        }

        public Type ElementType => InnerQueryable.ElementType;

        public Expression Expression => InnerQueryable.Expression;

        public IQueryProvider Provider => InnerQueryable.Provider;

        private IQueryable<TEntity> InnerQueryable => _dbContext.Set<TEntity>();
    }
}
