namespace Backend.Fx.Bootstrapping
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using Environment.MultiTenancy;
    using Logging;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation;
    using SimpleInjector;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    /// <summary>
    ///     Provides a reusable compostion root assuming Simple Injector as container
    /// </summary>
    public sealed class SimpleInjectorCompositionRoot : ICompositionRoot, IScopeManager
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorCompositionRoot>();

        public SimpleInjectorCompositionRoot()
        {
            Logger.Info("Initializing SimpleInjector");
            Container.Options.LifestyleSelectionBehavior = new ScopedLifestyleBehavior();
            Container.Options.DefaultScopedLifestyle = ScopedLifestyle;
        }

        internal Container Container { get; } = new Container();
        internal ScopedLifestyle ScopedLifestyle { get; } = new AsyncScopedLifestyle();

        public void RegisterModules(params IModule[] modules)
        {
            foreach (var module in modules)
            {
                Logger.Info($"Registering {module.GetType().Name}");
                module.Register();
            }
        }

        #region ICompositionRoot implementation

        public void Verify()
        {
            Logger.Info("container is being verified");
            try
            {
                Container.Verify(VerificationOption.VerifyAndDiagnose);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "container configuration invalid");
                throw;
            }
        }


        public object GetInstance(Type serviceType)
        {
            return Container.GetInstance(serviceType);
        }

        public T GetInstance<T>() where T : class
        {
            return Container.GetInstance<T>();
        }

        public IEnumerable<T> GetInstances<T>() where T : class
        {
            return Container.GetAllInstances<T>();
        }
        #endregion

        #region IScopeManager implementation

        /// <inheritdoc />
        public IScope BeginScope(IIdentity identity, TenantId tenantId)
        {
            var scope = new SimpleInjectorScope(AsyncScopedLifestyle.BeginScope(Container), identity, tenantId);
            scope.GetInstance<ICurrentTHolder<IIdentity>>().ReplaceCurrent(identity);
            scope.GetInstance<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);
            return scope;
        }

        /// <summary>
        ///     This should never be used in production code
        /// </summary>
        public Scope GetCurrentScopeForTestsOnly()
        {
            return ScopedLifestyle.GetCurrentScope(Container);
        }
        #endregion

        #region IEventHandlerProvider implementation

        /// <inheritdoc />
        public IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent
        {
            return Container.GetAllInstances<IDomainEventHandler<TDomainEvent>>();
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Container?.Dispose();
        }
        #endregion

        /// <summary>
        ///     A behavior that defaults to scoped life style for injected instances
        /// </summary>
        private class ScopedLifestyleBehavior : ILifestyleSelectionBehavior
        {
            public Lifestyle SelectLifestyle(Type implementationType)
            {
                return Lifestyle.Scoped;
            }
        }
    }
}