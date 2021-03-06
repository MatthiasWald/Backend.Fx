﻿using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain
{
    public interface ITestDomainService : IDomainService
    { }

    public interface IAnotherTestDomainService : IDomainService
    { }

    public class ADomainService : ITestDomainService, IAnotherTestDomainService
    { }
}
