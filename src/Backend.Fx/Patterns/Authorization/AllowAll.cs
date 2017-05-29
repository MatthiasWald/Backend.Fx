﻿namespace Backend.Fx.Patterns.Authorization
{
    using BuildingBlocks;

    public class AllowAll<TAggregateRoot> : IAggregateRootAuthorization<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        public System.Linq.Expressions.Expression<System.Func<TAggregateRoot, bool>> HasAccessExpression
        {
            get { return agg => true; }
        }

        public bool CanCreate()
        {
            return true;
        }
    }
}