namespace DemoBlog.Domain
{
    using System;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Security.Principal;
    using Backend.Fx.Environment.Authentication;
    using Backend.Fx.Patterns.Authorization;

    public class PostAuthorization : AggregateAuthorization<Post>
    {
        private readonly IIdentity identity;

        public PostAuthorization(IIdentity identity)
        {
            this.identity = identity;
        }

        public override Expression<Func<Post, bool>> HasAccessExpression
        {
            get { return blogger => true; }
        }

        public override bool CanCreate(Post post)
        {
            if (identity is SystemIdentity)
            {
                return true;
            }
            //TODO: die Blog Id muss hier irgendwie gepr�ft werden
            var claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity != null && claimsIdentity.HasClaim(claim => claim.Type == "urn:demoblog:blogadmin" && claim.Value == "blogId");
        }

        public override bool CanModify(Post t)
        {
            return CanCreate(t);
        }
    }
}