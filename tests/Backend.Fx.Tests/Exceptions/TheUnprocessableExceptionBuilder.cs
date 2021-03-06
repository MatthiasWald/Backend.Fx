﻿namespace Backend.Fx.Tests.Exceptions
{
    using BuildingBlocks;
    using Fx.Exceptions;
    using Xunit;

    public class TheUnprocessableExceptionBuilder
    {
        [Fact]
        public void AddsExceptionWhenAggregateIsNull()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.AddNotFoundWhenNull<TheAggregateRoot.TestAggregateRoot>(1111, null);
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void AddsNoExceptionWhenAggregateIsNotNull()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.AddNotFoundWhenNull(1111, new TheAggregateRoot.TestAggregateRoot(12345, "gaga"));
            sut.Dispose();
        }

        [Fact]
        public void ThrowsExceptionWhenAddingError()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.Add("something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void ThrowsExceptionWhenAddingConditionalError()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.AddIf(true, "something is broken");
            Assert.Throws<UnprocessableException>(() => sut.Dispose());
        }

        [Fact]
        public void DoesNotThrowExceptionWhenNotAddingConditionalError()
        {
            var sut = UnprocessableException.UseBuilder();
            sut.AddIf(false, "something is broken");
            sut.Dispose();
        }
    }
}
