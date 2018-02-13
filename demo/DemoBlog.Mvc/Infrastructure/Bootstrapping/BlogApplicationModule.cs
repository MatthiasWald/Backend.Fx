﻿namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
{
    using System.Reflection;
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Bootstrapping.Modules;
    using Backend.Fx.Environment.DateAndTime;
    using Backend.Fx.Patterns.IdGeneration;
    using Domain;
    using Persistence;
    using SimpleInjector;

    public class BlogApplicationModule : ApplicationModule
    {
        public BlogApplicationModule(SimpleInjectorCompositionRoot compositionRoot) : base(compositionRoot, typeof(Blog).GetTypeInfo().Assembly)
        { }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            base.Register(container, scopedLifestyle);
            container.Register<IClock, FrozenClock>();
            container.RegisterSingleton<IEntityIdGenerator, BlogEntityIdGenerator>();
        }
    }
}