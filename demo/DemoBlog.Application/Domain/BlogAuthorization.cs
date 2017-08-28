﻿namespace DemoBlog.Domain
{
    using System;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Security.Principal;
    using Backend.Fx.Environment.Authentication;
    using Backend.Fx.Patterns.Authorization;

    public class BlogAuthorization : IAggregateAuthorization<Blog>
    {
        private readonly IIdentity identity;

        public BlogAuthorization(IIdentity identity)
        {
            this.identity = identity;
        }

        public Expression<Func<Blog, bool>> HasAccessExpression
        {
            get { return blog => true; }
        }
        
        public bool CanCreate(Blog blog)
        {
            if (identity is SystemIdentity)
            {
                return true;
            }
            var claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity != null && claimsIdentity.HasClaim(claim => claim.Type == claimsIdentity.RoleClaimType && claim.Value == "Admin");
        }

        public bool CanModify(Blog t)
        {
            return CanCreate(t);
        }
    }
}
