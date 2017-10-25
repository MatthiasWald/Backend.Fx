﻿namespace DemoBlog.Mvc.Infrastructure
{
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Bootstrapping.Modules;
    using Backend.Fx.Patterns.IdGeneration;
    using Persistence;
    using SimpleInjector;

    public class DemoBlogModule : SimpleInjectorModule
    {
        public DemoBlogModule(SimpleInjectorCompositionRoot compositionRoot) : base(compositionRoot)
        { }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterSingleton<IEntityIdGenerator, BlogEntityIdGenerator>();
        }
    }
}