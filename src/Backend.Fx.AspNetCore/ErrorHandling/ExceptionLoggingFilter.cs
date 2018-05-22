﻿namespace Backend.Fx.AspNetCore.ErrorHandling
{
    using Logging;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class ExceptionLoggingFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null && !context.HttpContext.Items.ContainsKey("ExceptionLogged"))
            {
                ILogger logger = LogManager.Create(context.Controller.GetType().FullName);
                IExceptionLogger exceptionLogger = new ExceptionLogger(logger);
                exceptionLogger.LogException(context.Exception);
                context.HttpContext.Items["ExceptionLogged"] = true;
            }
        }
    }
}
