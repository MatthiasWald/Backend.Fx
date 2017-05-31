﻿namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    using Fx.Environment.MultiTenancy;
    using NLogLogging;
    using Xunit;

    public class TheCurrentTenantIdHolder : IClassFixture<NLogLoggingFixture>
    {
        [Fact]
        public void InitializesWithNullTenantIdIdentity()
        {
            var currentTenantIdHolder = new CurrentTenantIdHolder();
            Assert.False(currentTenantIdHolder.Current.HasValue);
        }

        [Fact]
        public void ReplacesCurrentTenantId()
        {
            var currentTenantIdHolder = new CurrentTenantIdHolder();
            currentTenantIdHolder.ReplaceCurrent(new TenantId(345));
            Assert.Equal(345, currentTenantIdHolder.Current.Value);
        }
    }
}
