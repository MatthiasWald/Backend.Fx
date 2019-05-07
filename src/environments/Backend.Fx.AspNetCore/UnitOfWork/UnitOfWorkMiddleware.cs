﻿using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.UnitOfWork;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.UnitOfWork
{
    /// <summary>
    ///     The Middleware is responsible for beginning and completing (or disposing) the unit of work for each request.
    /// </summary>
    public class UnitOfWorkMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWorkMiddleware>();
        private readonly RequestDelegate _next;
        private readonly IBackendFxApplication _application;

        [UsedImplicitly]
        public UnitOfWorkMiddleware(RequestDelegate next, IBackendFxApplication application)
        {
            _next = next;
            _application = application;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            while (!await _application.WaitForBootAsync(3000))
            {
                Logger.Info("Queuing Request while application is booting...");
            }

            IUnitOfWork unitOfWork = _application.CompositionRoot.GetInstance<IUnitOfWork>();
            try
            {
                if (context.Request.Method.ToUpperInvariant() == "GET")
                {
                    unitOfWork = new ReadonlyDecorator(unitOfWork);
                }

                unitOfWork.Begin();
                await _next.Invoke(context);
                unitOfWork.Complete();
            }
            finally
            {
                unitOfWork.Dispose();
            }
        }
    }
}
