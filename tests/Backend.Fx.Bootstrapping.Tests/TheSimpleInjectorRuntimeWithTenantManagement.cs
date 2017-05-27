﻿namespace Backend.Fx.Bootstrapping.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using BuildingBlocks;
    using DummyImpl;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using FakeItEasy;
    using Patterns.EventAggregation;
    using Xunit;

    public class TheSimpleInjectorRuntimeWithTenantManagement
    {
        private readonly TestRuntime sut;

        public TheSimpleInjectorRuntimeWithTenantManagement()
        {
            sut = new TestRuntime(null, A.Fake<IDatabaseManager>());
        }

        [Fact]
        public void RunsProductiveDataGeneratorsOnTenantInitialization()
        {
            sut.Boot();
            TenantId tenantId = sut.TenantManager.CreateProductionTenant("prod", "unit test created");

            using (var scope = sut.BeginScope(new SystemIdentity(), tenantId))
            {
                IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                AnAggregate[] allAggregates = repository.GetAll();
                Assert.Equal(1, allAggregates.Length);
                Assert.Equal("Productive record", allAggregates[0].Name);
            }
        }

        [Fact]
        public void RunsProductiveAndDemonstrationDataGeneratorsOnDemoTenantInitialization()
        {
            sut.Boot();
            TenantId tenantId = sut.TenantManager.CreateDemonstrationTenant("demo", "unit test created");

            using (var scope = sut.BeginScope(new SystemIdentity(), tenantId))
            {
                IRepository<AnAggregate> repository = scope.GetInstance<IRepository<AnAggregate>>();
                AnAggregate[] allAggregates = repository.GetAll();
                Assert.Equal(2, allAggregates.Length);
                Assert.Equal("Productive record", allAggregates[0].Name);
                Assert.Equal("Demo record", allAggregates[1].Name);
            }
        }

        [Fact]
        public void HandlesIntegrationEvents()
        {
            bool wasHandled = false;
            sut.Boot();
            TenantId tenantId = sut.TenantManager.CreateDemonstrationTenant("for integration event test", "");

            sut.SubscribeToIntegrationEvent<TheIntegrationEvent>(evt =>
            {
                Assert.Equal(tenantId.Value, evt.TenantId);
                Assert.Equal(42, evt.Whatever);
                wasHandled = true;
            });

            Task.WaitAll(sut.GetInstance<IEventAggregator>().PublishIntegrationEvent(new TheIntegrationEvent(tenantId.Value, 42)).ToArray());
            Assert.True(wasHandled);
        }
    }
}
