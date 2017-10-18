﻿namespace Backend.Fx.Bootstrapping.Modules
{
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using BuildingBlocks;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Patterns.Authorization;
    using Patterns.DataGeneration;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation;
    using SimpleInjector;

    public class DomainModule : SimpleInjectorModule
    {
        private readonly Assembly[] assemblies;

        public DomainModule(SimpleInjectorCompositionRoot compositionRoot, params Assembly[] domainAssemblies) : base(compositionRoot)
        {
            assemblies = domainAssemblies.Concat(new[] {
                typeof(Entity).GetTypeInfo().Assembly,
            }).ToArray();
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            // the current IIdentity is resolved using the scoped CurrentIdentityHolder that is maintained when opening a scope
            container.Register<ICurrentTHolder<IIdentity>, CurrentIdentityHolder>();
            container.Register(() => container.GetInstance<ICurrentTHolder<IIdentity>>().Current);

            // same for the current TenantId
            container.Register<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>();
            container.Register(() => container.GetInstance<ICurrentTHolder<TenantId>>().Current);

            RegisterDomainAndApplicationServices(container);

            RegisterAuthorization(container);

            // domain event subsystem
            container.RegisterCollection(typeof(IDomainEventHandler<>), assemblies);

            // initial data generation subsystem
            container.RegisterCollection<InitialDataGenerator>(assemblies);
        }

        /// <summary>
        ///     Auto registering all implementors of <see cref="IApplicationService" /> and <see cref="IDomainService" /> with
        ///     their implementations as scoped instances
        /// </summary>
        private void RegisterDomainAndApplicationServices(Container container)
        {
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
                container.Register(reg.Service, reg.Implementation);
            }
        }

        /// <summary>
        ///     Auto registering all aggregate authorization classes
        /// </summary>
        private void RegisterAuthorization(Container container)
        {
            var aggregateRootAuthorizationTypes = container.GetTypesToRegister(typeof(IAggregateAuthorization<>), assemblies).ToArray();
            foreach (var aggregateRootAuthorizationType in aggregateRootAuthorizationTypes)
            {
                var serviceTypes = aggregateRootAuthorizationType
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .Where(impif => impif.GetTypeInfo().IsGenericType
                                        && impif.GenericTypeArguments.Length == 1
                                        && typeof(AggregateRoot).GetTypeInfo().IsAssignableFrom(impif.GenericTypeArguments[0].GetTypeInfo()));

                foreach (var serviceType in serviceTypes)
                {
                    container.Register(serviceType, aggregateRootAuthorizationType);
                }
            }
        }
    }
}
