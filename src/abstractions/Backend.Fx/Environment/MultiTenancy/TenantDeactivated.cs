﻿using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantDeactivated : IntegrationEvent
    {
        public TenantDeactivated(int tenantId, string name, string description, bool isDemoTenant, string defaultCultureName) : base(tenantId)
        {
            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
            DefaultCultureName = defaultCultureName;
        }

        public string Name { get; }

        public string Description { get; }

        public bool IsDemoTenant { get; }

        public string DefaultCultureName { get; }
    }
}