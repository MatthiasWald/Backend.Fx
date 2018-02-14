﻿namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Bootstrapping;
    using Bootstrapping.Modules;
    using BuildingBlocks;
    using ConfigurationSettings;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using FakeItEasy;
    using Patterns.UnitOfWork;
    using SimpleInjector;
    
    public class InMemoryPersistenceModule : SimpleInjectorModule
    {
        public Dictionary<Type, object> Stores { get; }

        public InMemoryPersistenceModule(SimpleInjectorCompositionRoot compositionRoot, params Assembly[] domainAssemblies) : base(compositionRoot)
        {
            Stores = domainAssemblies.SelectMany(ass => ass.GetExportedTypes())
                    .Where(t => !t.GetTypeInfo().IsAbstract && t.GetTypeInfo().IsClass)
                    .Where(t => typeof(AggregateRoot).IsAssignableFrom(t))
                    .Select(t => {
                                var storeType = typeof(InMemoryStore<>).MakeGenericType(t);
                                var store = Activator.CreateInstance(storeType);
                                return new { t, store };
                            })
                    .ToDictionary(arg => arg.t, arg => arg.store);

            // the aggregate root "setting" resides outside the scanned assembly. Adding a repo manually now.
            Stores.Add(typeof(Setting), new InMemoryStore<Setting>());
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            foreach (var store in Stores)
            {
                container.RegisterSingleton(typeof(IInMemoryStore<>).MakeGenericType(store.Key), () => store.Value);
            }

            container.Register(typeof(IRepository<>), typeof(InMemoryRepository<>));
            container.Register(typeof(IQueryable<>), typeof(InMemoryQueryable<>));

            var uowRegistration = Lifestyle.Scoped.CreateRegistration(() => new InMemoryUnitOfWork(new FrozenClock(), new SystemIdentity()), container);
            container.AddRegistration(typeof(IUnitOfWork), uowRegistration);
            container.AddRegistration(typeof(ICanFlush), uowRegistration);
            container.Register(A.Fake<IReadonlyUnitOfWork>);
            container.Register(A.Fake<ICanInterruptTransaction>);
        }

        public Dictionary<Type, TAggregateRoot> GetStore<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            return (Dictionary<Type, TAggregateRoot>)Stores[typeof(TAggregateRoot)];
        }
    }
}
