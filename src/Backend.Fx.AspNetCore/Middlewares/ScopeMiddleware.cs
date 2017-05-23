﻿namespace Backend.Fx.AspNetCore.Middlewares
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Environment.MultiTenancy;
    using Exceptions;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Patterns.DependencyInjection;

    /// <summary>
    ///     This middleware enables use of an application runtime for each request. It makes sure that every request
    ///     is handled inside a unique execution scope resulting in a specific resolution root throughout the request.
    ///     The Middleware handles exceptions and is responsible for beginning and completing (or disposing) the unit
    ///     of work for each request.
    /// </summary>
    public class ScopeMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<ScopeMiddleware>();
        private readonly IHostingEnvironment env;
        private readonly RequestDelegate next;
        private readonly IScopeManager scopeManager;
        public const string TenantIdClaimType = "urn:metropoliplan:tenantid";

        /// <summary>
        ///     This constructor is being called by the framework DI container
        /// </summary>
        [UsedImplicitly]
        public ScopeMiddleware(RequestDelegate next, IScopeManager scopeManager, IHostingEnvironment env)
        {
            this.next = next;
            this.scopeManager = scopeManager;
            this.env = env;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            try
            {
                TenantId tenantId = GetTenantId(context.User.Identity);
                
                var asReadonly = context.Request.Method.ToUpperInvariant() == "GET";
                using (var scope = scopeManager.BeginScope(context.User.Identity, tenantId))
                {
                    scope.BeginUnitOfWork(asReadonly);
                    try
                    {
                        await next.Invoke(context);
                        scope.CompleteUnitOfWork();
                    }
                    catch (UnprocessableException uex)
                    {
                        Logger.Warn(uex);
                        context.Response.StatusCode = 422;
                        var responseContent = JsonConvert.SerializeObject(new { uex.Message });
                        await context.Response.WriteAsync(responseContent);
                    }
                    catch (ClientException cex)
                    {
                        Logger.Warn(cex);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var responseContent = JsonConvert.SerializeObject(new { cex.Message });
                        await context.Response.WriteAsync(responseContent);
                    }
                    catch (System.Security.SecurityException secex)
                    {
                        Logger.Warn(secex);
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsync("");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var responseContent = env.IsDevelopment()
                            ? JsonConvert.SerializeObject(new { ex.Message, ex.StackTrace })
                            : JsonConvert.SerializeObject(new { Message = "An internal error occured" });
                        await context.Response.WriteAsync(responseContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                var responseContent = env.IsDevelopment()
                    ? JsonConvert.SerializeObject(new { ex.Message, ex.StackTrace })
                    : JsonConvert.SerializeObject(new { Message = "An internal error occured" });
                await context.Response.WriteAsync(responseContent);
            }
        }

        private static TenantId GetTenantId(IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.Claims.SingleOrDefault(cl => cl.Type == TenantIdClaimType);
            int parsed;
            if (claim != null && int.TryParse(claim.Value, out parsed))
            {
                return new TenantId(parsed);
            }

            return new TenantId(null);
        }
    }
}
