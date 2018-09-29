﻿namespace Backend.Fx.EfCorePersistence
{
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;

    public abstract class SequenceHiLoIdGenerator<TDbContext> : HiLoIdGenerator, IEntityIdGenerator where TDbContext : DbContext
    {
        private readonly ISequence _sequence;
        private readonly DbContextOptions<TDbContext> _dbContextOptions;

        protected SequenceHiLoIdGenerator(ISequence sequence, DbContextOptions<TDbContext> dbContextOptions)
        {
            _sequence = sequence;
            _dbContextOptions = dbContextOptions;
        }

        protected override int GetNextBlockStart()
        {
            using (var dbContext = _dbContextOptions.CreateDbContext())
            {
                return _sequence.GetNextValue(dbContext);
            }
        }

        protected override int Increment => _sequence.Increment;
    }
}