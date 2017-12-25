﻿namespace Backend.Fx.Environment.MultiTenancy
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using JetBrains.Annotations;

    public class Tenant
    {
        public const string DemonstrationTenantName = "Demonstration";
        public const string DemonstrationTenantDescription = "Default Demonstration Tenant";

        [UsedImplicitly]
        private Tenant()
        { }

        public Tenant([NotNull] string name, string description, bool isDemoTenant)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDemoTenant { get; set; }
        
        public TenantState State { get; set; }

        public bool IsDefault { get; set; }
    }

    public enum TenantState
    {
        Created = 0,
        Initializing = 1,
        Active = 2,
        Inactive = int.MaxValue
    }
}
