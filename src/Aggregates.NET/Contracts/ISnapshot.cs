﻿using System;

namespace Aggregates.Contracts
{
    public interface ISnapshot
    {
        string Bucket { get; }
        string EntityType { get; }
        Id StreamId { get; }
        long Version { get; }

        object Payload { get; }

        DateTime Timestamp { get; }
    }
}