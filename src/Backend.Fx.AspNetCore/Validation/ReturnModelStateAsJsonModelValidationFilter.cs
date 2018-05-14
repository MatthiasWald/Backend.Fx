﻿namespace Backend.Fx.AspNetCore.Validation
{
    using Exceptions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class ReturnModelStateAsJsonModelValidationFilter : ModelValidationFilter
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid && AcceptsJson(context))
            {
                Errors errors = context.ModelState.ToErrorsDictionary();
                LogErrors(context, context.Controller.ToString(), errors);
                context.Result = new BadRequestObjectResult(new { errors });
            }
        }
        
        public override void OnActionExecuted(ActionExecutedContext context)
        {}
    }
}