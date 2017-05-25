namespace Backend.Fx.Bootstrapping.Tests
{
    using System;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using FakeItEasy;
    using Xunit;

    public class TheMisconfiguredSimpleInjectorRuntime
    {
        [Fact]
        public void ThrowsOnValidation()
        {
            IDatabaseManager databaseManager = A.Fake<IDatabaseManager>();

            ITenantManager tenantManager = A.Fake<ITenantManager>();
            A.CallTo(() => tenantManager.IsActive(A<TenantId>._)).Returns(true);

            var sut = new TestRuntime(tenantManager, databaseManager);
            Assert.Throws<InvalidOperationException>(() => sut.Boot(container => container.Register<UnresolvableService>()));
        }

        public class UnresolvableService
        {
            private readonly Entity e;

            public UnresolvableService(Entity e)
            {
                this.e = e;
            }
        }
    }
}