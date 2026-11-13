// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Serializes synchronous registry operations while allowing a publication lease
// to be released by a thread other than the one that acquired it.
internal sealed class CrossThreadRegistryGate
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    [ThreadStatic]
    private static Ownership? CurrentThreadOwnership;

    internal IDisposable Enter()
    {
        var ownership = FindReentrantOwnership();
        if (ownership is not null && ownership.TryRetain())
            return CreateRetainedScope(ownership);

        _semaphore.Wait();
        try
        {
            return CreateOwnership();
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    internal bool TryEnter(TimeSpan timeout, out IDisposable? scope)
    {
        var ownership = FindReentrantOwnership();
        if (ownership is not null && ownership.TryRetain())
        {
            scope = CreateRetainedScope(ownership);
            return true;
        }

        if (!_semaphore.Wait(timeout))
        {
            scope = null;
            return false;
        }

        try
        {
            scope = CreateOwnership();
            return true;
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    private IDisposable CreateOwnership()
    {
        RemoveReleasedOwnershipsFromTop();
        var ownership = new Ownership(
            this,
            _semaphore,
            CurrentThreadOwnership);
        var scope = new Scope(ownership);
        CurrentThreadOwnership = ownership;
        return scope;
    }

    private static IDisposable CreateRetainedScope(Ownership ownership)
    {
        try
        {
            return new Scope(ownership);
        }
        catch
        {
            ownership.Release();
            throw;
        }
    }

    private Ownership? FindReentrantOwnership()
    {
        RemoveReleasedOwnershipsFromTop();
        for (var candidate = CurrentThreadOwnership;
             candidate is not null;
             candidate = candidate.Previous)
        {
            if (ReferenceEquals(candidate.Gate, this) && candidate.IsActive)
                return candidate;
        }

        return null;
    }

    private static void RemoveReleasedOwnershipsFromTop()
    {
        while (CurrentThreadOwnership is { IsActive: false } released)
            CurrentThreadOwnership = released.Previous;
    }

    private sealed class Ownership(
        CrossThreadRegistryGate gate,
        SemaphoreSlim semaphore,
        Ownership? previous)
    {
        private int _referenceCount = 1;

        internal CrossThreadRegistryGate Gate { get; } = gate;
        internal Ownership? Previous { get; } = previous;
        internal bool IsActive => Volatile.Read(ref _referenceCount) > 0;

        internal bool TryRetain()
        {
            while (true)
            {
                var current = Volatile.Read(ref _referenceCount);
                if (current == 0)
                    return false;
                if (current == int.MaxValue)
                    throw new InvalidOperationException(
                        "Registry gate reentrancy is exhausted.");
                if (Interlocked.CompareExchange(
                        ref _referenceCount,
                        current + 1,
                        current) == current)
                {
                    return true;
                }
            }
        }

        internal void Release()
        {
            var remaining = Interlocked.Decrement(ref _referenceCount);
            if (remaining == 0)
                semaphore.Release();
            else if (remaining < 0)
                throw new InvalidOperationException(
                    "Registry gate ownership was released more than once.");
        }
    }

    private sealed class Scope(Ownership ownership) : IDisposable
    {
        private Ownership? _ownership = ownership;

        public void Dispose()
            => Interlocked.Exchange(ref _ownership, null)?.Release();
    }
}

// Owns the only production acquisition protocol for the two capability
// registries. The scope is deliberately synchronous: callers must not await
// while it is held. Unit always precedes flow, and disposal reverses that
// order, so independent publishers cannot construct an AB/BA deadlock.
public sealed class LogicalRegistryPublicationLease : IDisposable
{
    private readonly InMemoryLogicalUnitRegistry _units;
    private readonly InMemoryLogicalFlowRegistry _flows;
    private IDisposable? _unitLease;
    private IDisposable? _flowLease;

    private LogicalRegistryPublicationLease(
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        IDisposable unitLease,
        IDisposable flowLease)
    {
        _units = units;
        _flows = flows;
        _unitLease = unitLease;
        _flowLease = flowLease;
    }

    public static OperationResult<LogicalRegistryPublicationLease>
        TryAcquireUnitThenFlow(
        InMemoryLogicalUnitRegistry units,
        InMemoryLogicalFlowRegistry flows,
        TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(flows);

        var unitResult = units.TryAcquirePublicationLease(timeout);
        if (!unitResult.IsSuccess || unitResult.Value is null)
            return unitResult.MapError<LogicalRegistryPublicationLease>();

        var unitLease = unitResult.Value;
        IDisposable? flowLease = null;
        try
        {
            var flowResult = flows.TryAcquirePublicationLease(timeout);
            if (!flowResult.IsSuccess || flowResult.Value is null)
            {
                unitLease.Dispose();
                return flowResult.MapError<
                    LogicalRegistryPublicationLease>();
            }

            flowLease = flowResult.Value;
            return OperationResult<LogicalRegistryPublicationLease>.Ok(
                new LogicalRegistryPublicationLease(
                    units,
                    flows,
                    unitLease,
                    flowLease));
        }
        catch
        {
            try
            {
                flowLease?.Dispose();
            }
            finally
            {
                unitLease.Dispose();
            }
            throw;
        }
    }

    public OperationResult<(
        long NextUnitRegistrationOrder,
        long NextFlowRegistrationOrder)> PeekNextRegistrationOrders()
    {
        if (_unitLease is null || _flowLease is null)
        {
            return OperationResult<(
                long NextUnitRegistrationOrder,
                long NextFlowRegistrationOrder)>.Fail(
                    "Logical registry publication lease is no longer active.",
                    "LOGICAL_REGISTRY_PUBLICATION_LEASE_INACTIVE");
        }

        var nextUnitRegistrationOrder =
            _units.PeekNextRegistrationOrderForPublication();
        if (!nextUnitRegistrationOrder.IsSuccess)
        {
            return nextUnitRegistrationOrder.MapError<(
                long NextUnitRegistrationOrder,
                long NextFlowRegistrationOrder)>();
        }

        var nextFlowRegistrationOrder =
            _flows.PeekNextRegistrationOrderForPublication();
        if (!nextFlowRegistrationOrder.IsSuccess)
        {
            return nextFlowRegistrationOrder.MapError<(
                long NextUnitRegistrationOrder,
                long NextFlowRegistrationOrder)>();
        }

        return OperationResult<(
            long NextUnitRegistrationOrder,
            long NextFlowRegistrationOrder)>.Ok((
                nextUnitRegistrationOrder.Value,
                nextFlowRegistrationOrder.Value));
    }

    public void Dispose()
    {
        var flowLease = Interlocked.Exchange(ref _flowLease, null);
        var unitLease = Interlocked.Exchange(ref _unitLease, null);
        try
        {
            flowLease?.Dispose();
        }
        finally
        {
            unitLease?.Dispose();
        }
    }
}
