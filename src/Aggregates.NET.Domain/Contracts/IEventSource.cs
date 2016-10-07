﻿using NServiceBus;
using System;
using System.Collections.Generic;

namespace Aggregates.Contracts
{
    public interface IEventSource
    {
        String Bucket { get; }
        String StreamId { get; }
        Int32 Version { get; }

        IEventStream Stream { get; }
        void Hydrate(IEnumerable<IEvent> events);
        void Conflict(IEvent @event);
        void Apply(IEvent @event);
        void Raise(IEvent @event);
    }

    public interface IEventSource<TId> : IEventSource
    {
        TId Id { get; set; }
    }
}