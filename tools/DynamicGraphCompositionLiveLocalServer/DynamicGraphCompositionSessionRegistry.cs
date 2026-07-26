// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Concurrent;
using System.Security.Cryptography;
using AgentInfrastructure;
using OrchestratorModel.Logical;

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer;

public sealed class DynamicGraphCompositionSessionRegistry : IDisposable
{
    private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ExpiredSessionEntry> _expiredSessions = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, IdempotencyState> _idempotency = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _compositionSlots;
    private readonly SemaphoreSlim _runSlots;
    private readonly object _cleanupGate = new();
    private readonly object _lifecycleGate = new();
    private readonly DynamicGraphCompositionServerOptions _options;
    private readonly DynamicGraphCompositionSessionPersistence _persistence;
    private readonly bool _ownsPersistence;
    private readonly TimeProvider _timeProvider;
    private Action<string>? _persistenceFailure;
    private int _reservedSessions;
    private int _invalidated;
    private int _activeOperations;
    private int _disposed;

    public DynamicGraphCompositionSessionRegistry(
        DynamicGraphCompositionServerOptions options,
        TimeProvider? timeProvider = null)
        : this(options, new DynamicGraphCompositionSessionPersistence(options), timeProvider, true)
    {
    }

    public DynamicGraphCompositionSessionRegistry(
        DynamicGraphCompositionServerOptions options,
        DynamicGraphCompositionSessionPersistence persistence,
        TimeProvider? timeProvider = null)
        : this(options, persistence, timeProvider, false)
    {
    }

    private DynamicGraphCompositionSessionRegistry(
        DynamicGraphCompositionServerOptions options,
        DynamicGraphCompositionSessionPersistence persistence,
        TimeProvider? timeProvider,
        bool ownsPersistence)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(persistence);
        options.Validate();
        _options = options;
        _persistence = persistence;
        _ownsPersistence = ownsPersistence;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _compositionSlots = new SemaphoreSlim(options.MaxConcurrentCompositions, options.MaxConcurrentCompositions);
        _runSlots = new SemaphoreSlim(options.MaxConcurrentRuns, options.MaxConcurrentRuns);
    }

    internal void RegisterPersistenceFailure(Action<string> callback)
    {
        using var operationLease = EnterOperation();
        ArgumentNullException.ThrowIfNull(callback);
        _persistenceFailure = callback;
    }

    public async Task<SessionCreationResult> CreateAsync(
        string tenantId,
        string idempotencyKey,
        string canonicalDigest,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        Func<CancellationToken, Task<(
            CompositionEvidenceHttpResponse Evidence,
            IDynamicGraphLogicalOrchestratorRunSession RunSession)>> compose,
        Func<bool> publicationAllowed,
        CancellationToken cancellationToken)
    {
        using var operationLease = EnterOperation();
        return await CreateAsyncCore(
            tenantId,
            idempotencyKey,
            canonicalDigest,
            profile,
            compose,
            publicationAllowed,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<SessionCreationResult> CreateAsyncCore(
        string tenantId,
        string idempotencyKey,
        string canonicalDigest,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        Func<CancellationToken, Task<(
            CompositionEvidenceHttpResponse Evidence,
            IDynamicGraphLogicalOrchestratorRunSession RunSession)>> compose,
        Func<bool> publicationAllowed,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalDigest);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(compose);
        ArgumentNullException.ThrowIfNull(publicationAllowed);

        try
        {
            RemoveExpired();
        }
        catch (DynamicGraphCompositionPersistenceException exception) when (
            IsNonFatalException(exception))
        {
            FailPersistence();
            return SessionCreationResult.PersistenceFailure();
        }
        if (IsInvalidated || !publicationAllowed())
            return SessionCreationResult.ForbiddenAssembly();
        var tenantIndex = DeriveIndex("tenant", tenantId);
        var idempotencyIndex = DeriveIndex("idempotency", tenantId, idempotencyKey);
        while (_idempotency.TryGetValue(idempotencyIndex, out var known))
        {
            if (!DigestEquals(known.CanonicalDigest, canonicalDigest))
                return SessionCreationResult.Conflict();
            if (!known.Session.IsValueCreated || !known.Session.Value.IsCompleted)
                return SessionCreationResult.Pending();
            try
            {
                var replay = await known.Session.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
                if (replay.ExpiresAtUtc > _timeProvider.GetUtcNow())
                    return SessionCreationResult.Replayed(replay);
                lock (_cleanupGate)
                    ExpireSession(replay, _timeProvider.GetUtcNow());
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                RemoveState(idempotencyIndex, known);
                throw;
            }
        }

        if (Interlocked.Increment(ref _reservedSessions) > _options.MaxSessions)
        {
            Interlocked.Decrement(ref _reservedSessions);
            return SessionCreationResult.CapacityExceeded();
        }

        IdempotencyState? candidate = null;
        try
        {
            if (IsInvalidated || !publicationAllowed())
            {
                Interlocked.Decrement(ref _reservedSessions);
                return SessionCreationResult.ForbiddenAssembly();
            }
            var candidateState = new IdempotencyState(canonicalDigest);
            candidate = candidateState;
            candidateState.Session = new Lazy<Task<SessionEntry>>(
                () => ComposeAndStoreAsync(
                    tenantIndex,
                    idempotencyIndex,
                    canonicalDigest,
                    profile,
                    candidateState,
                    compose,
                    publicationAllowed,
                    cancellationToken),
                LazyThreadSafetyMode.ExecutionAndPublication);
            if (IsInvalidated || !publicationAllowed())
            {
                Interlocked.Decrement(ref _reservedSessions);
                return SessionCreationResult.ForbiddenAssembly();
            }
            if (!_idempotency.TryAdd(idempotencyIndex, candidate))
            {
                Interlocked.Decrement(ref _reservedSessions);
                return await CreateAsyncCore(
                    tenantId, idempotencyKey, canonicalDigest, profile, compose, publicationAllowed, cancellationToken).ConfigureAwait(false);
            }

            var entry = await candidate.Session.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (IsInvalidated || !publicationAllowed())
            {
                RollbackPublishedCore(entry);
                return SessionCreationResult.ForbiddenAssembly();
            }
            return SessionCreationResult.Created(entry);
        }
        catch (CompositionConcurrencyException)
        {
            if (candidate is not null)
                RemoveState(idempotencyIndex, candidate);
            return SessionCreationResult.Busy();
        }
        catch (ForbiddenAssemblyPublicationException)
        {
            if (candidate is not null)
                RemoveState(idempotencyIndex, candidate);
            return SessionCreationResult.ForbiddenAssembly();
        }
        catch (DynamicGraphCompositionPersistenceException exception) when (
            IsNonFatalException(exception))
        {
            if (candidate is not null)
                RemoveState(idempotencyIndex, candidate);
            FailPersistence();
            return SessionCreationResult.PersistenceFailure();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            if (candidate is not null)
                RemoveStateAfterUnsuccessfulCompletion(idempotencyIndex, candidate);
            throw;
        }
    }

    private async Task<SessionEntry> ComposeAndStoreAsync(
        string tenantIndex,
        string idempotencyIndex,
        string canonicalDigest,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        IdempotencyState idempotencyState,
        Func<CancellationToken, Task<(
            CompositionEvidenceHttpResponse Evidence,
            IDynamicGraphLogicalOrchestratorRunSession RunSession)>> compose,
        Func<bool> publicationAllowed,
        CancellationToken cancellationToken)
    {
        using var operationLease = EnterDerivedOperation();
        if (!await _compositionSlots.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            throw new CompositionConcurrencyException();

        Exception? compositionFailure = null;
        try
        {
            var composed = await compose(cancellationToken).ConfigureAwait(false);
            if (composed.Evidence is null || composed.RunSession is null)
                throw new InvalidOperationException("A composed runtime session and its evidence are required.");
            if (IsInvalidated || !publicationAllowed())
                throw new ForbiddenAssemblyPublicationException();
            var createdAt = _timeProvider.GetUtcNow();
            while (true)
            {
                if (IsInvalidated || !publicationAllowed())
                    throw new ForbiddenAssemblyPublicationException();
                var entry = new SessionEntry(
                    CreateOpaqueId(),
                    tenantIndex,
                    createdAt,
                    createdAt.Add(_options.SessionTtl),
                    composed.Evidence,
                    composed.RunSession,
                    idempotencyIndex,
                    canonicalDigest,
                    idempotencyState);
                _persistence.PersistNew(new PersistedCompositionSession(
                    DynamicGraphCompositionSessionPersistence.CheckpointSchema,
                    entry.SessionId,
                    entry.TenantIndex,
                    entry.OpaqueIdempotencyIndex,
                    entry.CanonicalDigest,
                    entry.CreatedAtUtc,
                    entry.ExpiresAtUtc,
                    profile,
                    entry.Result));
                if (IsInvalidated || !publicationAllowed())
                {
                    _persistence.Delete(entry.SessionId);
                    throw new ForbiddenAssemblyPublicationException();
                }
                if (_sessions.TryAdd(entry.SessionId, entry))
                {
                    if (IsInvalidated || !publicationAllowed())
                    {
                        RollbackPublishedCore(entry);
                        throw new ForbiddenAssemblyPublicationException();
                    }
                    return entry;
                }
                _persistence.Delete(entry.SessionId);
            }
        }
        catch (Exception exception)
        {
            compositionFailure = exception;
            throw;
        }
        finally
        {
            if (compositionFailure is null || IsNonFatalException(compositionFailure))
                _compositionSlots.Release();
        }
    }

    internal void Restore(
        Func<DynamicGraphLogicalOrchestratorBootstrapProfile, (
            CompositionEvidenceHttpResponse Evidence,
            IDynamicGraphLogicalOrchestratorRunSession RunSession)> compose)
    {
        using var operationLease = EnterOperation();
        ArgumentNullException.ThrowIfNull(compose);
        if (!_persistence.IsEnabled)
            return;

        try
        {
            var checkpoints = _persistence.ReadAll(_options.MaxSessions);
            var now = _timeProvider.GetUtcNow();
            foreach (var checkpoint in checkpoints)
            {
                if (checkpoint.ExpiresAtUtc <= now)
                {
                    _persistence.Delete(checkpoint.SessionId);
                    continue;
                }
                if (_reservedSessions >= _options.MaxSessions)
                    throw new DynamicGraphCompositionPersistenceException(
                        "Persisted session state exceeds the configured capacity.");

                var restoreProfile = DynamicGraphCompositionSessionPersistence.SnapshotProfile(
                    checkpoint.Profile);
                var admission = DynamicGraphCompositionProfilePolicy.ValidateOwnedSnapshot(
                    restoreProfile,
                    _options.TenantId);
                if (!admission.IsAccepted
                    || !string.Equals(
                        admission.CanonicalDigest,
                        checkpoint.CanonicalDigest,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        DeriveIndex("tenant", _options.TenantId),
                        checkpoint.TenantIndex,
                        StringComparison.Ordinal))
                {
                    throw new DynamicGraphCompositionPersistenceException(
                        "A persisted composition profile failed deterministic admission.");
                }

                var composed = compose(restoreProfile);
                var postComposeAdmission = DynamicGraphCompositionProfilePolicy.ValidateOwnedSnapshot(
                    restoreProfile,
                    _options.TenantId);
                if (!postComposeAdmission.IsAccepted
                    || !string.Equals(
                        postComposeAdmission.CanonicalDigest,
                        checkpoint.CanonicalDigest,
                        StringComparison.Ordinal)
                    || !EvidenceEquals(checkpoint.Evidence, composed.Evidence)
                    || checkpoint.Evidence.Revision != 1
                    || composed.Evidence.Revision != 1)
                {
                    throw new DynamicGraphCompositionPersistenceException(
                        "Persisted composition evidence does not match the restored runtime.");
                }

                var state = new IdempotencyState(checkpoint.CanonicalDigest);
                var entry = new SessionEntry(
                    checkpoint.SessionId,
                    checkpoint.TenantIndex,
                    checkpoint.CreatedAtUtc,
                    checkpoint.ExpiresAtUtc,
                    checkpoint.Evidence,
                    composed.RunSession,
                    checkpoint.OpaqueIdempotencyIndex,
                    checkpoint.CanonicalDigest,
                    state);
                state.Session = new Lazy<Task<SessionEntry>>(
                    () => Task.FromResult(entry),
                    LazyThreadSafetyMode.ExecutionAndPublication);
                _ = state.Session.Value;

                if (!_sessions.TryAdd(entry.SessionId, entry)
                    || !_idempotency.TryAdd(entry.OpaqueIdempotencyIndex, state))
                {
                    throw new DynamicGraphCompositionPersistenceException(
                        "Duplicate persisted session state was found.");
                }
                Interlocked.Increment(ref _reservedSessions);
            }
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            InvalidateAllCore();
            throw;
        }
    }

    public SessionLookupResult Get(string tenantId, string sessionId)
    {
        using var operationLease = EnterOperation();
        return GetCore(tenantId, sessionId);
    }

    private SessionLookupResult GetCore(string tenantId, string sessionId)
    {
        var now = _timeProvider.GetUtcNow();
        var tenantIndex = DeriveIndex("tenant", tenantId);
        lock (_cleanupGate)
        {
            CleanupExpiredTombstones(now);
            if (_sessions.TryGetValue(sessionId, out var entry)
                && DigestEquals(entry.TenantIndex, tenantIndex))
            {
                if (entry.ExpiresAtUtc > now)
                    return SessionLookupResult.Found(entry);
                var retainedUntil = entry.ExpiresAtUtc.Add(_options.SessionTtl);
                ExpireSession(entry, now);
                return retainedUntil > now
                    ? SessionLookupResult.Expired()
                    : SessionLookupResult.Unknown();
            }

            if (_expiredSessions.TryGetValue(sessionId, out var expired)
                && DigestEquals(expired.TenantIndex, tenantIndex))
                return SessionLookupResult.Expired();
            return SessionLookupResult.Unknown();
        }
    }

    public async Task<SessionExecutionResult> RunAsync(
        string tenantId,
        string sessionId,
        DynamicGraphLogicalOrchestratorRunRequest request,
        Func<bool> executionAllowed,
        CancellationToken cancellationToken)
    {
        using var operationLease = EnterOperation();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(executionAllowed);

        if (IsInvalidated || !executionAllowed())
            return SessionExecutionResult.ForbiddenAssembly();

        var lookup = GetCore(tenantId, sessionId);
        if (lookup.Status == SessionLookupStatus.Expired)
            return SessionExecutionResult.Expired();
        if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
            return SessionExecutionResult.Unknown();

        if (!await _runSlots.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return SessionExecutionResult.Busy();

        Exception? executionFailure = null;
        try
        {
            if (IsInvalidated || !executionAllowed())
                return SessionExecutionResult.ForbiddenAssembly();

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return SessionExecutionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
                return SessionExecutionResult.Unknown();

            var result = await lookup.Session.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            if (IsInvalidated || !executionAllowed())
                return SessionExecutionResult.ForbiddenAssembly();
            return SessionExecutionResult.Completed(result);
        }
        catch (Exception exception)
        {
            executionFailure = exception;
            throw;
        }
        finally
        {
            if (executionFailure is null || IsNonFatalException(executionFailure))
                _runSlots.Release();
        }
    }

    internal bool RollbackPublished(SessionEntry? entry)
    {
        using var operationLease = EnterOperation();
        return RollbackPublishedCore(entry);
    }

    private bool RollbackPublishedCore(SessionEntry? entry)
    {
        if (entry is null)
            return false;

        var removed = false;
        lock (_cleanupGate)
        {
            try
            {
                _persistence.Delete(entry.SessionId);
            }
            catch (DynamicGraphCompositionPersistenceException exception) when (
                IsNonFatalException(exception))
            {
                FailPersistence();
                return false;
            }

            removed = _sessions.TryRemove(
                new KeyValuePair<string, SessionEntry>(entry.SessionId, entry));
            _expiredSessions.TryRemove(entry.SessionId, out _);

            if (_idempotency.TryGetValue(entry.OpaqueIdempotencyIndex, out var state)
                && DigestEquals(state.CanonicalDigest, entry.CanonicalDigest)
                && ReferenceEquals(state, entry.IdempotencyStateIdentity))
            {
                RemoveState(entry.OpaqueIdempotencyIndex, state);
                removed = true;
            }
        }
        return removed;
    }

    internal void InvalidateAll()
    {
        using var operationLease = EnterOperation();
        InvalidateAllCore();
    }

    private void InvalidateAllCore()
    {
        Interlocked.Exchange(ref _invalidated, 1);
        lock (_cleanupGate)
        {
            _sessions.Clear();
            _expiredSessions.Clear();
            _idempotency.Clear();
            Interlocked.Exchange(ref _reservedSessions, 0);
        }
    }

    private bool IsInvalidated => Volatile.Read(ref _invalidated) == 1;

    private void RemoveExpired()
    {
        var now = _timeProvider.GetUtcNow();
        lock (_cleanupGate)
        {
            foreach (var pair in _sessions)
            {
                if (pair.Value.ExpiresAtUtc > now)
                    continue;
                ExpireSession(pair.Value, now);
                if (IsInvalidated)
                    return;
            }

            CleanupExpiredTombstones(now);
        }
    }

    private void CleanupExpiredTombstones(DateTimeOffset now)
    {
        foreach (var tombstone in _expiredSessions)
        {
            if (tombstone.Value.RemoveAfterUtc <= now)
                _expiredSessions.TryRemove(tombstone.Key, out _);
        }
    }

    private void ExpireSession(SessionEntry entry, DateTimeOffset now)
    {
        if (!_sessions.TryGetValue(entry.SessionId, out var current)
            || !ReferenceEquals(current, entry))
            return;
        try
        {
            _persistence.Delete(entry.SessionId);
        }
        catch (DynamicGraphCompositionPersistenceException exception) when (
            IsNonFatalException(exception))
        {
            FailPersistence();
            return;
        }
        if (!_sessions.TryRemove(new KeyValuePair<string, SessionEntry>(entry.SessionId, entry)))
            return;
        var removeAfter = entry.ExpiresAtUtc.Add(_options.SessionTtl);
        if (removeAfter > now)
            _expiredSessions[entry.SessionId] = new ExpiredSessionEntry(entry.TenantIndex, removeAfter);
        foreach (var idempotency in _idempotency)
        {
            if (!idempotency.Value.Session.IsValueCreated
                || !idempotency.Value.Session.Value.IsCompletedSuccessfully
                || !ReferenceEquals(idempotency.Value.Session.Value.Result, entry))
                continue;
            RemoveState(idempotency.Key, idempotency.Value);
        }
    }

    private void RemoveState(string idempotencyIndex, IdempotencyState state)
    {
        if (_idempotency.TryRemove(new KeyValuePair<string, IdempotencyState>(idempotencyIndex, state)))
            Interlocked.Decrement(ref _reservedSessions);
    }

    private void RemoveStateAfterUnsuccessfulCompletion(
        string idempotencyIndex,
        IdempotencyState state)
    {
        if (!state.Session.IsValueCreated)
        {
            RemoveState(idempotencyIndex, state);
            return;
        }

        var sessionTask = state.Session.Value;
        if (sessionTask.IsCompleted)
        {
            if (!sessionTask.IsCompletedSuccessfully)
            {
                _ = sessionTask.Exception;
                RemoveState(idempotencyIndex, state);
            }
            return;
        }

        // WaitAsync can be cancelled independently from the shared composition task. Keep the
        // reservation/idempotency owner while that task can still publish, and clean it only if
        // the shared task ultimately fails or is cancelled.
        _ = sessionTask.ContinueWith(
            completedTask =>
            {
                _ = completedTask.Exception;
                RemoveState(idempotencyIndex, state);
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnRanToCompletion,
            TaskScheduler.Default);
    }

    private static bool DigestEquals(string left, string right)
    {
        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(left),
                Convert.FromHexString(right));
        }
        catch (FormatException exception) when (IsNonFatalException(exception))
        {
            return false;
        }
    }

    private static string CreateOpaqueId()
        => "dgs_" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));

    private string DeriveIndex(params string[] components)
        => _persistence.DeriveIndex(components);

    private void FailPersistence()
    {
        InvalidateAllCore();
        try { _persistenceFailure?.Invoke("HOST_SESSION_PERSISTENCE_FAILED"); }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            /* The registry remains invalidated even if the host callback fails. */
        }
    }

    private static bool IsNonFatalException(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is OutOfMemoryException
                || current is StackOverflowException
                || current is AccessViolationException
                || current is AppDomainUnloadedException
                || current is BadImageFormatException
                || StringComparer.Ordinal.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException"))
            {
                return false;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return true;
    }

    private static bool EvidenceEquals(
        CompositionEvidenceHttpResponse left,
        CompositionEvidenceHttpResponse right)
        => string.Equals(left.ProfileId, right.ProfileId, StringComparison.Ordinal)
           && left.Revision == right.Revision
           && string.Equals(left.SourceKind, right.SourceKind, StringComparison.Ordinal)
           && left.NodeCount == right.NodeCount
           && left.EdgeCount == right.EdgeCount
           && left.InitialRevisionAdvanced == right.InitialRevisionAdvanced;

    private OperationLease EnterOperation()
    {
        lock (_lifecycleGate)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            return AdmitOperation();
        }
    }

    private OperationLease EnterDerivedOperation()
    {
        lock (_lifecycleGate)
        {
            if (_activeOperations <= 0)
                throw new InvalidOperationException("Derived registry work requires an admitted operation.");
            return AdmitOperation();
        }
    }

    private OperationLease AdmitOperation()
    {
        var operationLease = new OperationLease(this);
        var nextCount = checked(_activeOperations + 1);
        _activeOperations = nextCount;
        return operationLease;
    }

    private void CompleteOperation()
    {
        lock (_lifecycleGate)
        {
            if (_activeOperations <= 0)
                throw new InvalidOperationException("The session registry operation count is invalid.");

            _activeOperations--;
            if (_activeOperations == 0)
                Monitor.PulseAll(_lifecycleGate);
        }
    }

    private void WaitForActiveOperations()
    {
        lock (_lifecycleGate)
        {
            while (_activeOperations != 0)
                Monitor.Wait(_lifecycleGate);
        }
    }

    private sealed class OperationLease : IDisposable
    {
        private DynamicGraphCompositionSessionRegistry? _owner;

        internal OperationLease(DynamicGraphCompositionSessionRegistry owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.CompleteOperation();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        WaitForActiveOperations();
        _persistenceFailure = null;

        Exception? invalidationFailure = null;
        try
        {
            InvalidateAllCore();
        }
        catch (Exception exception)
        {
            invalidationFailure = exception;
            throw;
        }
        finally
        {
            if (invalidationFailure is null || IsNonFatalException(invalidationFailure))
            {
                Exception? runSlotsFailure = null;
                try
                {
                    _runSlots.Dispose();
                }
                catch (Exception exception)
                {
                    runSlotsFailure = exception;
                    throw;
                }
                finally
                {
                    if (runSlotsFailure is null || IsNonFatalException(runSlotsFailure))
                    {
                        Exception? compositionSlotsFailure = null;
                        try
                        {
                            _compositionSlots.Dispose();
                        }
                        catch (Exception exception)
                        {
                            compositionSlotsFailure = exception;
                            throw;
                        }
                        finally
                        {
                            if ((compositionSlotsFailure is null
                                 || IsNonFatalException(compositionSlotsFailure))
                                && _ownsPersistence)
                            {
                                _persistence.Dispose();
                            }
                        }
                    }
                }
            }
        }
    }

    public sealed class SessionEntry
    {
        private readonly IDynamicGraphLogicalOrchestratorRunSession _runSession;

        internal SessionEntry(
            string sessionId,
            string tenantIndex,
            DateTimeOffset createdAtUtc,
            DateTimeOffset expiresAtUtc,
            CompositionEvidenceHttpResponse result,
            IDynamicGraphLogicalOrchestratorRunSession runSession,
            string opaqueIdempotencyIndex,
            string canonicalDigest,
            object idempotencyStateIdentity)
        {
            SessionId = sessionId;
            TenantIndex = tenantIndex;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
            Result = result;
            _runSession = runSession;
            OpaqueIdempotencyIndex = opaqueIdempotencyIndex;
            CanonicalDigest = canonicalDigest;
            IdempotencyStateIdentity = idempotencyStateIdentity;
        }

        public string SessionId { get; }
        internal string TenantIndex { get; }
        public DateTimeOffset CreatedAtUtc { get; }
        public DateTimeOffset ExpiresAtUtc { get; }
        public CompositionEvidenceHttpResponse Result { get; }
        internal string OpaqueIdempotencyIndex { get; }
        internal string CanonicalDigest { get; }
        internal object IdempotencyStateIdentity { get; }

        internal Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> ExecuteAsync(
            DynamicGraphLogicalOrchestratorRunRequest request,
            CancellationToken cancellationToken)
            => _runSession.RunAsync(request, cancellationToken);
    }

    private sealed class IdempotencyState(string canonicalDigest)
    {
        internal string CanonicalDigest { get; } = canonicalDigest;
        internal Lazy<Task<SessionEntry>> Session { get; set; } = null!;
    }
    private sealed record ExpiredSessionEntry(string TenantIndex, DateTimeOffset RemoveAfterUtc);
    private sealed class CompositionConcurrencyException : Exception;
    private sealed class ForbiddenAssemblyPublicationException : Exception;
}

public enum SessionCreationStatus
{
    Created,
    Replayed,
    Conflict,
    Pending,
    CapacityExceeded,
    Busy,
    ForbiddenAssembly,
    PersistenceFailure
}

public sealed record SessionCreationResult(SessionCreationStatus Status, DynamicGraphCompositionSessionRegistry.SessionEntry? Session)
{
    public static SessionCreationResult Created(DynamicGraphCompositionSessionRegistry.SessionEntry session) => new(SessionCreationStatus.Created, session);
    public static SessionCreationResult Replayed(DynamicGraphCompositionSessionRegistry.SessionEntry session) => new(SessionCreationStatus.Replayed, session);
    public static SessionCreationResult Conflict() => new(SessionCreationStatus.Conflict, null);
    public static SessionCreationResult Pending() => new(SessionCreationStatus.Pending, null);
    public static SessionCreationResult CapacityExceeded() => new(SessionCreationStatus.CapacityExceeded, null);
    public static SessionCreationResult Busy() => new(SessionCreationStatus.Busy, null);
    public static SessionCreationResult ForbiddenAssembly() => new(SessionCreationStatus.ForbiddenAssembly, null);
    public static SessionCreationResult PersistenceFailure() => new(SessionCreationStatus.PersistenceFailure, null);
}

public enum SessionLookupStatus { Found, Expired, Unknown }

public sealed record SessionLookupResult(SessionLookupStatus Status, DynamicGraphCompositionSessionRegistry.SessionEntry? Session)
{
    public static SessionLookupResult Found(DynamicGraphCompositionSessionRegistry.SessionEntry session) => new(SessionLookupStatus.Found, session);
    public static SessionLookupResult Expired() => new(SessionLookupStatus.Expired, null);
    public static SessionLookupResult Unknown() => new(SessionLookupStatus.Unknown, null);
}

public enum SessionExecutionStatus { Completed, Expired, Unknown, Busy, ForbiddenAssembly }

public sealed record SessionExecutionResult(
    SessionExecutionStatus Status,
    OperationResult<DynamicGraphLogicalOrchestratorRunResult>? Result)
{
    public static SessionExecutionResult Completed(
        OperationResult<DynamicGraphLogicalOrchestratorRunResult>? result)
        => new(SessionExecutionStatus.Completed, result);

    public static SessionExecutionResult Expired() => new(SessionExecutionStatus.Expired, null);
    public static SessionExecutionResult Unknown() => new(SessionExecutionStatus.Unknown, null);
    public static SessionExecutionResult Busy() => new(SessionExecutionStatus.Busy, null);
    public static SessionExecutionResult ForbiddenAssembly() => new(SessionExecutionStatus.ForbiddenAssembly, null);
}
