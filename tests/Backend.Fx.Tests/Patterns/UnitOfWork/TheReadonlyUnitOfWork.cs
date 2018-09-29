﻿using Xunit;

namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    public class TheReadonlyUnitOfWork
    {
        [Fact]
        public void RollsBackOnComplete()
        {
            TestReadonlyUnitOfWork sut = new TestReadonlyUnitOfWork();
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            Assert.Equal(1, sut.RollbackCount);
        }

        [Fact]
        public void RollsBackOnDispose()
        {
            TestReadonlyUnitOfWork sut = new TestReadonlyUnitOfWork();
            sut.Begin();
            sut.Dispose();
            Assert.Equal(1, sut.RollbackCount);
        }
    }
}
