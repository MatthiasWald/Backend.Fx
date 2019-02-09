﻿namespace Backend.Fx.Patterns.UnitOfWork
{
    using System;
    using System.Security.Principal;
    using DependencyInjection;
    using Environment.DateAndTime;
    using EventAggregation.Domain;
    using EventAggregation.Integration;
    using Extensions;
    using Logging;

    /// <summary>
    /// All-or-nothing operation wrapper, typically implemented by a surrounding database transaction
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        void Begin();
        void Complete();
        ICurrentTHolder<IIdentity> IdentityHolder { get; }
    }

    public abstract class UnitOfWork : IUnitOfWork, ICanFlush
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWork>();
        private static int _index;
        private readonly int _instanceId = _index++;
        private readonly IClock _clock;
        private readonly IDomainEventAggregator _eventAggregator;
        private readonly IEventBusScope _eventBusScope;
        private bool? _isCompleted;
        private IDisposable _lifetimeLogger;

        protected UnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder,
                             IDomainEventAggregator eventAggregator, IEventBusScope eventBusScope)
        {
            _clock = clock;
            _eventAggregator = eventAggregator;
            _eventBusScope = eventBusScope;
            IdentityHolder = identityHolder;
        }

        public ICurrentTHolder<IIdentity> IdentityHolder { get; }

        public virtual void Flush()
        {
            Logger.Debug("Flushing unit of work #" + _instanceId);
            UpdateTrackingProperties(IdentityHolder.Current.Name, _clock.UtcNow);
        }
    
        public virtual void Begin()
        {
            _lifetimeLogger = Logger.DebugDuration($"Beginning unit of work #{_instanceId}", $"Disposing unit of work #{_instanceId}");
            _isCompleted = false;
        }

        public void Complete()
        {
            Logger.Debug("Completing unit of work #" + _instanceId);
            Flush();
            _eventAggregator.RaiseEvents();
            Commit();
            AsyncHelper.RunSync(()=>_eventBusScope.RaiseEvents());
            _isCompleted = true;
        }

        

        protected abstract void UpdateTrackingProperties(string userId, DateTime utcNow);
        protected abstract void Commit();
        protected abstract void Rollback();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_isCompleted == false)
                {
                    Logger.Info($"Canceling unit of work #{_instanceId} because the instance is being disposed although it did not complete before. This should only occur during cleanup after errors.");
                    Rollback();
                }
                _lifetimeLogger?.Dispose();
                _lifetimeLogger = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
