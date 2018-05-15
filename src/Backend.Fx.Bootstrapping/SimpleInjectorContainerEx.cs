﻿namespace Backend.Fx.Bootstrapping
{
    using System;
    using System.Linq;
    using System.Reflection;
    using BuildingBlocks;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using SimpleInjector;

    public static class SimpleInjectorContainerEx
    {
        private static readonly ILogger Logger = LogManager.Create(typeof(SimpleInjectorContainerEx));

        /// <summary>
        ///     Auto registering all implementors of <see cref="IApplicationService" /> and <see cref="IDomainService" /> with
        ///     their implementations as scoped instances
        /// </summary>
        public static void RegisterDomainAndApplicationServices(this Container container, Assembly[] assemblies)
        {
            Logger.Debug($"Registering domain and application services from {string.Join(",", assemblies.Select(ass => ass.GetName().Name))}");
            var serviceRegistrations = container
                                       .GetTypesToRegister(typeof(IDomainService), assemblies)
                                       .Concat(container.GetTypesToRegister(typeof(IApplicationService), assemblies))
                                       .SelectMany(type =>
                                                       type.GetTypeInfo()
                                                           .ImplementedInterfaces
                                                           .Where(i => typeof(IDomainService) != i && typeof(IApplicationService) != i && (i.Namespace.StartsWith("Backend") || assemblies.Contains(i.GetTypeInfo().Assembly)))
                                                           .Select(service => new
                                                           {
                                                                   Service = service,
                                                                   Implementation = type
                                                           })
                                       );
            foreach (var reg in serviceRegistrations)
            {
                Logger.Debug($"Registering scoped service {reg.Service.Name} with implementation {reg.Implementation.Name}");
                container.Register(reg.Service, reg.Implementation);
            }
        }

        public static void Configure<TOptions>(this Container container, Action<TOptions> configure) where TOptions : class
        {
            container.RegisterSingleton<IConfigureOptions<TOptions>>(new ConfigureOptions<TOptions>(configure));
        }

        public static void Configure<TOptions>(this Container container, IConfiguration configuration) where TOptions : class
        {
            container.RegisterSingleton<IOptionsChangeTokenSource<TOptions>>(new ConfigurationChangeTokenSource<TOptions>(Options.DefaultName, configuration));
            container.RegisterSingleton<IConfigureOptions<TOptions>>(new NamedConfigureFromConfigurationOptions<TOptions>(Options.DefaultName, configuration));
        }

    }
}
