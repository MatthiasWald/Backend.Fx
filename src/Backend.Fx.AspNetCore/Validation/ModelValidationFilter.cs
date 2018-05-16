﻿namespace Backend.Fx.AspNetCore.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
    using Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Net.Http.Headers;

    public abstract class ModelValidationFilter : IActionFilter
    {
        public abstract void OnActionExecuting(ActionExecutingContext context);
        public abstract void OnActionExecuted(ActionExecutedContext context);
        
        protected void LogErrors(FilterContext context, string controllerName, Errors errors)
        {
            ILogger logger = LogManager.Create(controllerName);
            logger.Warn($"Model validation failed during {context.HttpContext.Request.Method} {context.HttpContext.Request.PathBase}: " +
                        string.Join(Environment.NewLine, errors.SelectMany(err => err.Value.Select(er => $"    {er.Code}:{er.Message}"))));
        }

        protected bool AcceptsJson(FilterContext context)
        {
            IList<MediaTypeHeaderValue> accept = context.HttpContext.Request.GetTypedHeaders().Accept;
            return accept?.Any(mth => mth.Type == "application" && mth.SubType == "json") == true;
        }

        protected bool AcceptsHtml(FilterContext context)
        {
            IList<MediaTypeHeaderValue> accept = context.HttpContext.Request.GetTypedHeaders().Accept;
            return accept?.Any(mth => mth.Type == "text" && mth.SubType == "html") == true;
        }
    }
}