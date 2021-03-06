﻿namespace Backend.Fx.Patterns.Authorization
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;

    public abstract class AggregateAuthorization<TAggregateRoot> : IAggregateAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        /// <inheritdoc />>
        public virtual Expression<Func<TAggregateRoot, bool>> HasAccessExpression
        {
            get { return agg => true; }
        }

        /// <inheritdoc />>
        public virtual IQueryable<TAggregateRoot> Filter(IQueryable<TAggregateRoot> queryable)
        {
            return queryable.Where(HasAccessExpression);
        }

        /// <inheritdoc />>
        public virtual bool CanCreate(TAggregateRoot t)
        {
            return true;
        }

        /// <summary>
        /// Implement a guard that might disallow modifying an existing aggregate.
        /// This overload is called directly before saving modification of an instance, so that you can use the instance's state for deciding.
        /// This default implementation forwards to <see cref="AggregateAuthorization{TAggregateRoot}.CanCreate"/>
        /// </summary>
        public virtual bool CanModify(TAggregateRoot t)
        {
            return CanCreate(t);
        }

        /// <inheritdoc />>
        public virtual bool CanDelete(TAggregateRoot t)
        {
            return CanModify(t);
        }
    }
}