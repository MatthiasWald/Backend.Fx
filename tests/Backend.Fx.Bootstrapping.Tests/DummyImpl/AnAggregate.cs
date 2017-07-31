﻿namespace Backend.Fx.Bootstrapping.Tests.DummyImpl
{
    using BuildingBlocks;
    using Patterns.Authorization;
    using Patterns.DataGeneration;

    public class AnAggregate : AggregateRoot
    {
        public AnAggregate(int id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public class ProductiveGenerator : InitialDataGenerator, IProductiveDataGenerator
    {
        private readonly IRepository<AnAggregate> repository;
        public override int Priority { get { return 1; } }

        public ProductiveGenerator(IRepository<AnAggregate> repository)
        {
            this.repository = repository;
        }

        protected override void GenerateCore()
        {
            repository.Add(new AnAggregate(234, "Productive record"));
        }

        protected override void Initialize()
        { }

        protected override bool ShouldRun()
        {
            return true;
        }
    }

    public class AnAggregateAuthorization : AllowAll<AnAggregate> { }

    public class DemonstrationGenerator : InitialDataGenerator, IDemoDataGenerator
    {
        private readonly IRepository<AnAggregate> repository;
        public override int Priority { get { return 1; } }

        public DemonstrationGenerator(IRepository<AnAggregate> repository)
        {
            this.repository = repository;
        }

        protected override void GenerateCore()
        {
            repository.Add(new AnAggregate(456, "Demo record"));
        }

        protected override void Initialize()
        { }

        protected override bool ShouldRun()
        {
            return true;
        }
    }
}
