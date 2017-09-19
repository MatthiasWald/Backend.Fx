﻿namespace DemoBlog.Domain
{
    using System;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Security.Principal;
    using Backend.Fx.Environment.Authentication;
    using Backend.Fx.Patterns.Authorization;

    public class BloggerAuthorization : AggregateAuthorization<Blogger>
    {
        private readonly IIdentity identity;

        public BloggerAuthorization(IIdentity identity)
        {
            this.identity = identity;
        }

        public override Expression<Func<Blogger, bool>> HasAccessExpression
        {
            get { return blogger => true; }
        }

        public override bool CanCreate(Blogger t)
        {
            if (identity is SystemIdentity)
            {
                return true;
            }
            var claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity != null && claimsIdentity.HasClaim(claim => claim.Type == claimsIdentity.RoleClaimType && claim.Value == "Admin");
        }

        public override bool CanModify(Blogger t)
        {
            return CanCreate(t);
        }
    }
}