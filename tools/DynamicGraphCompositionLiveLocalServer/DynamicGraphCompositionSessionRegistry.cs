// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer;

public sealed class DynamicGraphCompositionSessionRegistry : IDisposable
{
    private const int SelectedTargetEvidenceCapacity = 16;
    private const int SelectedTargetEvidenceHandleAttempts = 32;
    private const string SelectedTargetEvidenceAdmissionResultRequiredCode =
        "HOST_SELECTED_TARGET_EVIDENCE_ADMISSION_RESULT_REQUIRED";
    private const string SelectedTargetEvidenceAdmissionResultRequiredMessage =
        "A selected-target evidence admission result is required.";
    private const string SelectedTargetEvidenceHandleAllocationFailedCode =
        "HOST_SELECTED_TARGET_EVIDENCE_HANDLE_ALLOCATION_FAILED";
    private const string SelectedTargetEvidenceHandleAllocationFailedMessage =
        "A selected-target evidence handle could not be allocated safely.";
    private static readonly Regex SelectedTargetEvidenceHandlePattern = new(
        @"\Adgeh_[0-9a-f]{32}\z",
        RegexOptions.CultureInvariant);

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
    private readonly IDynamicGraphSelectedTargetEvidenceInvoker _selectedTargetEvidenceInvoker;
    private readonly Func<string> _selectedTargetEvidenceHandleFactory;
    private Action<string>? _persistenceFailure;
    private int _reservedSessions;
    private int _invalidated;
    private int _activeOperations;
    private int _disposed;

    public DynamicGraphCompositionSessionRegistry(
        DynamicGraphCompositionServerOptions options,
        TimeProvider? timeProvider = null)
        : this(
            options,
            new DynamicGraphCompositionSessionPersistence(options),
            timeProvider ?? TimeProvider.System,
            new DynamicGraphSelectedTargetEvidenceInvoker(),
            CreateSelectedTargetEvidenceHandle,
            true)
    {
    }

    public DynamicGraphCompositionSessionRegistry(
        DynamicGraphCompositionServerOptions options,
        DynamicGraphCompositionSessionPersistence persistence,
        TimeProvider? timeProvider = null)
        : this(
            options,
            persistence,
            timeProvider ?? TimeProvider.System,
            new DynamicGraphSelectedTargetEvidenceInvoker(),
            CreateSelectedTargetEvidenceHandle,
            false)
    {
    }

    internal DynamicGraphCompositionSessionRegistry(
        DynamicGraphCompositionServerOptions options,
        DynamicGraphCompositionSessionPersistence persistence,
        TimeProvider timeProvider,
        IDynamicGraphSelectedTargetEvidenceInvoker selectedTargetEvidenceInvoker,
        Func<string> handleFactory)
        : this(
            options,
            persistence,
            timeProvider,
            selectedTargetEvidenceInvoker,
            handleFactory,
            false)
    {
    }

    private DynamicGraphCompositionSessionRegistry(
        DynamicGraphCompositionServerOptions options,
        DynamicGraphCompositionSessionPersistence persistence,
        TimeProvider timeProvider,
        IDynamicGraphSelectedTargetEvidenceInvoker selectedTargetEvidenceInvoker,
        Func<string> handleFactory,
        bool ownsPersistence)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(persistence);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(selectedTargetEvidenceInvoker);
        ArgumentNullException.ThrowIfNull(handleFactory);
        options.Validate();
        _options = options;
        _persistence = persistence;
        _ownsPersistence = ownsPersistence;
        _timeProvider = timeProvider;
        _selectedTargetEvidenceInvoker = selectedTargetEvidenceInvoker;
        _selectedTargetEvidenceHandleFactory = handleFactory;
        _compositionSlots = new SemaphoreSlim(options.MaxConcurrentCompositions, options.MaxConcurrentCompositions);
        _runSlots = new SemaphoreSlim(options.MaxConcurrentRuns, options.MaxConcurrentRuns);
    }

    private static string CreateSelectedTargetEvidenceHandle()
        => "dgeh_" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));

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
            DynamicGraphLogicalOrchestratorCheckpointCapture? capture = null;
            if (_persistence.IsEnabled)
            {
                if (composed.RunSession
                    is not IDynamicGraphLogicalOrchestratorCheckpointSession checkpointSession)
                {
                    throw new DynamicGraphCompositionPersistenceException(
                        "The composed runtime does not expose the private durable checkpoint capability.");
                }
                var captured = checkpointSession.CaptureCheckpoint(profile);
                if (captured is null
                    || !captured.IsSuccess
                    || captured.Value is null)
                {
                    throw new DynamicGraphCompositionPersistenceException(
                        "The composed runtime checkpoint could not be captured.");
                }
                capture = captured.Value;
            }
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
                    entry.Result,
                    capture,
                    AuthenticatedCheckpoint: null));
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
                    || (string.Equals(
                            checkpoint.Schema,
                            DynamicGraphCompositionSessionPersistence.LegacyCheckpointSchema,
                            StringComparison.Ordinal)
                        && !string.Equals(
                            admission.CanonicalDigest,
                            checkpoint.CanonicalDigest,
                            StringComparison.Ordinal))
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
                    || composed.Evidence is null
                    || composed.RunSession is null)
                {
                    throw new DynamicGraphCompositionPersistenceException(
                        "Persisted composition evidence does not match the restored runtime.");
                }

                if (string.Equals(
                        checkpoint.Schema,
                        DynamicGraphCompositionSessionPersistence.LegacyCheckpointSchema,
                        StringComparison.Ordinal))
                {
                    if (!string.Equals(
                            postComposeAdmission.CanonicalDigest,
                            checkpoint.CanonicalDigest,
                            StringComparison.Ordinal)
                        || !EvidenceEquals(checkpoint.Evidence, composed.Evidence)
                        || checkpoint.Evidence.Revision != 1
                        || composed.Evidence.Revision != 1
                        || checkpoint.AuthenticatedCheckpoint is not null)
                    {
                        throw new DynamicGraphCompositionPersistenceException(
                            "Persisted legacy composition evidence does not match the restored runtime.");
                    }
                }
                else
                {
                    if (!string.Equals(
                            checkpoint.Schema,
                            DynamicGraphCompositionSessionPersistence.CheckpointSchema,
                            StringComparison.Ordinal)
                        || checkpoint.AuthenticatedCheckpoint is null
                        || composed.RunSession
                            is not IDynamicGraphLogicalOrchestratorCheckpointSession
                                checkpointSession
                        || composed.RunSession
                            is not IDynamicGraphLogicalOrchestratorCompositionSession
                                compositionSession)
                    {
                        throw new DynamicGraphCompositionPersistenceException(
                            "The persisted runtime checkpoint capability is unavailable.");
                    }

                    var restored = checkpointSession.RestoreCheckpoint(
                        restoreProfile,
                        checkpoint.AuthenticatedCheckpoint);
                    if (restored is null
                        || !restored.IsSuccess
                        || restored.Value is null
                        || !CheckpointCaptureEquals(
                            restored.Value,
                            restoreProfile,
                            checkpoint.AuthenticatedCheckpoint))
                    {
                        throw new DynamicGraphCompositionPersistenceException(
                            "The persisted runtime checkpoint could not be restored.");
                    }

                    var captured = checkpointSession.CaptureCheckpoint(restoreProfile);
                    var publicCurrent = compositionSession.ReadCurrent();
                    if (captured is null
                        || !captured.IsSuccess
                        || captured.Value is null
                        || !CheckpointCaptureEquals(
                            captured.Value,
                            restoreProfile,
                            checkpoint.AuthenticatedCheckpoint)
                        || publicCurrent is null
                        || !publicCurrent.IsSuccess
                        || publicCurrent.Value is null
                        || !CurrentReadEqualsCheckpoint(
                            publicCurrent.Value,
                            restoreProfile,
                            checkpoint.AuthenticatedCheckpoint))
                    {
                        throw new DynamicGraphCompositionPersistenceException(
                            "The restored runtime checkpoint readback is invalid.");
                    }
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

    internal CurrentTopologySessionResult ReadCurrentTopology(
        string tenantId,
        string sessionId,
        Func<bool> readAllowed)
    {
        using var operationLease = EnterOperation();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentNullException.ThrowIfNull(readAllowed);

        if (IsInvalidated || !readAllowed())
            return CurrentTopologySessionResult.ForbiddenAssembly();

        var preLookup = GetCore(tenantId, sessionId);
        var preFailure = MapCurrentTopologyLookup(preLookup);
        if (preFailure is not null)
            return preFailure;

        if (IsInvalidated || !readAllowed())
            return CurrentTopologySessionResult.ForbiddenAssembly();

        var readLookup = GetCore(tenantId, sessionId);
        var readFailure = MapCurrentTopologyLookup(readLookup);
        if (readFailure is not null)
            return readFailure;
        if (!ReferenceEquals(preLookup.Session, readLookup.Session))
            return CurrentTopologySessionResult.Unknown();

        var entry = readLookup.Session!;
        var result = entry.ReadCurrentTopology();

        if (IsInvalidated || !readAllowed())
            return CurrentTopologySessionResult.ForbiddenAssembly();

        var postLookup = GetCore(tenantId, sessionId);
        var postFailure = MapCurrentTopologyLookup(postLookup);
        if (postFailure is not null)
            return postFailure;
        if (!ReferenceEquals(entry, postLookup.Session))
            return CurrentTopologySessionResult.Unknown();

        return CurrentTopologySessionResult.Completed(entry, result);
    }

    private static CurrentTopologySessionResult? MapCurrentTopologyLookup(
        SessionLookupResult lookup)
    {
        if (lookup.Status == SessionLookupStatus.Expired)
            return CurrentTopologySessionResult.Expired();
        if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
            return CurrentTopologySessionResult.Unknown();
        return lookup.Session.SupportsTopologyRead
            ? null
            : CurrentTopologySessionResult.Unavailable();
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

        var execution = await ExecuteSessionOperationAsync<
                DynamicGraphLogicalOrchestratorRunRequest,
                DynamicGraphLogicalOrchestratorRunResult>(
                tenantId,
                sessionId,
                request,
                executionAllowed,
                static _ => true,
                static (entry, runRequest, token) => entry.ExecuteAsync(runRequest, token),
                cancellationToken)
            .ConfigureAwait(false);
        return execution.Status switch
        {
            SessionOperationStatus.Completed => SessionExecutionResult.Completed(execution.Result),
            SessionOperationStatus.Expired => SessionExecutionResult.Expired(),
            SessionOperationStatus.Unknown => SessionExecutionResult.Unknown(),
            SessionOperationStatus.Busy => SessionExecutionResult.Busy(),
            SessionOperationStatus.ForbiddenAssembly => SessionExecutionResult.ForbiddenAssembly(),
            // The legacy capability predicate is always true, so this state is unreachable.
            SessionOperationStatus.Unavailable => SessionExecutionResult.Unknown(),
            _ => throw new InvalidOperationException("The session execution status is invalid.")
        };
    }

    internal async Task<TopologySessionExecutionResult> RunOnCurrentTopologyAsync(
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

        var execution = await ExecuteSessionOperationAsync<
                DynamicGraphLogicalOrchestratorRunRequest,
                DynamicGraphLogicalOrchestratorTopologyRunResult>(
                tenantId,
                sessionId,
                request,
                executionAllowed,
                static entry => entry.SupportsTopologyRun,
                static (entry, runRequest, token) =>
                    entry.ExecuteTopologyRunAsync(runRequest, token),
                cancellationToken)
            .ConfigureAwait(false);
        return execution.Status switch
        {
            SessionOperationStatus.Completed =>
                TopologySessionExecutionResult.Completed(execution.Result),
            SessionOperationStatus.Expired => TopologySessionExecutionResult.Expired(),
            SessionOperationStatus.Unknown => TopologySessionExecutionResult.Unknown(),
            SessionOperationStatus.Busy => TopologySessionExecutionResult.Busy(),
            SessionOperationStatus.ForbiddenAssembly =>
                TopologySessionExecutionResult.ForbiddenAssembly(),
            SessionOperationStatus.Unavailable => TopologySessionExecutionResult.Unavailable(),
            _ => throw new InvalidOperationException(
                "The topology session execution status is invalid.")
        };
    }

    internal async Task<TopologyMutationSessionResult> ApplyTopologyMutationAsync(
        string tenantId,
        string sessionId,
        long expectedRevision,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        Func<bool> executionAllowed,
        CancellationToken cancellationToken)
    {
        using var operationLease = EnterOperation();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(executionAllowed);

        if (_persistence.IsEnabled)
            return TopologyMutationSessionResult.EphemeralModeRequired();
        if (IsInvalidated || !executionAllowed())
            return TopologyMutationSessionResult.ForbiddenAssembly();

        var admission = DynamicGraphCompositionProfilePolicy.Admit(profile, tenantId);
        if (!admission.IsAccepted || admission.Profile is null)
        {
            return TopologyMutationSessionResult.Completed(
                OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                    "The topology mutation profile is invalid.",
                    "HOST_TOPOLOGY_MUTATION_PROFILE_INVALID"));
        }
        var admittedProfile = admission.Profile;

        var lookup = GetCore(tenantId, sessionId);
        if (lookup.Status == SessionLookupStatus.Expired)
            return TopologyMutationSessionResult.Expired();
        if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
            return TopologyMutationSessionResult.Unknown();
        if (!lookup.Session.SupportsTopologyMutation)
            return TopologyMutationSessionResult.Unavailable();

        var scopeFailure = ValidateTopologyMutationScope(lookup.Session, admittedProfile);
        if (scopeFailure is not null)
            return TopologyMutationSessionResult.Completed(scopeFailure);

        if (!await _runSlots.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return TopologyMutationSessionResult.Busy();

        Exception? executionFailure = null;
        try
        {
            if (IsInvalidated || !executionAllowed())
                return TopologyMutationSessionResult.ForbiddenAssembly();

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return TopologyMutationSessionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
                return TopologyMutationSessionResult.Unknown();
            if (!lookup.Session.SupportsTopologyMutation)
                return TopologyMutationSessionResult.Unavailable();

            scopeFailure = ValidateTopologyMutationScope(lookup.Session, admittedProfile);
            if (scopeFailure is not null)
                return TopologyMutationSessionResult.Completed(scopeFailure);

            var projection = CreateOwnedRegistryProjection(admittedProfile);
            var budget = admittedProfile.Budget with { };
            cancellationToken.ThrowIfCancellationRequested();
            var result = lookup.Session.ApplyRegistryProjection(
                expectedRevision,
                projection,
                budget);
            return TopologyMutationSessionResult.Completed(result);
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

    private static OperationResult<DynamicGraphTopologyApplyResult>?
        ValidateTopologyMutationScope(
            SessionEntry entry,
            DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        var descriptor = entry.TopologyMutationDescriptor;
        if (!string.Equals(entry.Result.ProfileId, descriptor.ProfileId, StringComparison.Ordinal)
            || !string.Equals(profile.ProfileId, descriptor.ProfileId, StringComparison.Ordinal))
        {
            return OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                "The topology mutation profile id does not match the session.",
                "HOST_TOPOLOGY_MUTATION_PROFILE_ID_MISMATCH");
        }
        var profileTenant = profile.TenantId;
        var descriptorTenant = descriptor.TenantId;
        if (!string.Equals(profileTenant, descriptorTenant, StringComparison.Ordinal)
            || !string.Equals(profile.DomainTag, descriptor.DomainTag, StringComparison.Ordinal)
            || !string.Equals(profile.ContextId, descriptor.ContextId, StringComparison.Ordinal))
        {
            return OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                "The topology mutation scope does not match the session.",
                "HOST_TOPOLOGY_MUTATION_SCOPE_MISMATCH");
        }
        return null;
    }

    private static DynamicGraphRegistryProjectionRequest CreateOwnedRegistryProjection(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        var tenantId = profile.TenantId;
        return new DynamicGraphRegistryProjectionRequest(
            tenantId,
            profile.DomainTag,
            profile.ContextId,
            profile.SignalId,
            profile.SourceRef,
            profile.TraceRef,
            profile.NodeDefinitions
                .Select(definition => new DynamicGraphRegistryNodeBinding(
                    definition.LogicalSubPhaseId,
                    new DescriptorOnlyDynamicGraphNode(definition.Descriptor with { })))
                .ToArray(),
            profile.EdgeBindings
                .Select(binding => binding with
                {
                    LogicalTransitionIds = binding.LogicalTransitionIds is null
                        ? null!
                        : binding.LogicalTransitionIds.ToArray(),
                    ExcludedLeakageSignals = binding.ExcludedLeakageSignals is null
                        ? null!
                        : binding.ExcludedLeakageSignals.ToArray()
                })
                .ToArray());
    }

    public async Task<SelectedTargetSessionExecutionResult> RunSelectedTargetAsync(
        string tenantId,
        string sessionId,
        DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest request,
        Func<bool> executionAllowed,
        CancellationToken cancellationToken)
    {
        using var operationLease = EnterOperation();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(executionAllowed);

        var execution = await ExecuteSessionOperationAsync<
                DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest,
                DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>(
                tenantId,
                sessionId,
                request,
                executionAllowed,
                static entry => entry.SupportsSelectedTargetExecution,
                static (entry, selectedTargetRequest, token) =>
                    entry.ExecuteSelectedTargetAsync(selectedTargetRequest, token),
                cancellationToken)
            .ConfigureAwait(false);
        return execution.Status switch
        {
            SessionOperationStatus.Completed =>
                SelectedTargetSessionExecutionResult.Completed(execution.Result),
            SessionOperationStatus.Expired => SelectedTargetSessionExecutionResult.Expired(),
            SessionOperationStatus.Unknown => SelectedTargetSessionExecutionResult.Unknown(),
            SessionOperationStatus.Busy => SelectedTargetSessionExecutionResult.Busy(),
            SessionOperationStatus.ForbiddenAssembly =>
                SelectedTargetSessionExecutionResult.ForbiddenAssembly(),
            SessionOperationStatus.Unavailable => SelectedTargetSessionExecutionResult.Unavailable(),
            _ => throw new InvalidOperationException("The selected-target execution status is invalid.")
        };
    }

    internal async Task<SelectedTargetSessionExecutionResult> RunAdmittedSelectedTargetAsync(
        string tenantId,
        string sessionId,
        string handle,
        string runId,
        string scenarioId,
        string userRequest,
        Func<bool> executionAllowed,
        CancellationToken cancellationToken)
    {
        using var operationLease = EnterOperation();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(handle);
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userRequest);
        ArgumentNullException.ThrowIfNull(executionAllowed);

        if (IsInvalidated)
            return SelectedTargetSessionExecutionResult.ForbiddenAssembly();
        if (!executionAllowed() || IsInvalidated)
            return SelectedTargetSessionExecutionResult.ForbiddenAssembly();

        var lookup = GetCore(tenantId, sessionId);
        if (lookup.Status == SessionLookupStatus.Expired)
            return SelectedTargetSessionExecutionResult.Expired();
        if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
            return SelectedTargetSessionExecutionResult.Unknown();

        var entry = lookup.Session;
        if (!entry.SupportsAdmittedSelectedTargetExecution)
            return SelectedTargetSessionExecutionResult.Unavailable();
        if (entry.ResolveSelectedTargetEvidence(handle) is null)
            return SelectedTargetSessionExecutionResult.Unknown();

        if (!await _runSlots.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return SelectedTargetSessionExecutionResult.Busy();

        Exception? executionFailure = null;
        try
        {
            if (IsInvalidated)
                return SelectedTargetSessionExecutionResult.ForbiddenAssembly();
            if (!executionAllowed() || IsInvalidated)
                return SelectedTargetSessionExecutionResult.ForbiddenAssembly();

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return SelectedTargetSessionExecutionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found
                || lookup.Session is null
                || !ReferenceEquals(lookup.Session, entry))
            {
                return SelectedTargetSessionExecutionResult.Unknown();
            }
            if (!entry.SupportsAdmittedSelectedTargetExecution)
                return SelectedTargetSessionExecutionResult.Unavailable();
            if (entry.ResolveSelectedTargetEvidence(handle) is null)
                return SelectedTargetSessionExecutionResult.Unknown();

            if (!executionAllowed())
                return SelectedTargetSessionExecutionResult.ForbiddenAssembly();

            var claim = ClaimSameEntrySelectedTargetEvidence(
                tenantId,
                sessionId,
                handle,
                entry);
            if (claim.Status == SelectedTargetEvidenceResolutionStatus.ForbiddenAssembly)
                return SelectedTargetSessionExecutionResult.ForbiddenAssembly();
            if (claim.Status == SelectedTargetEvidenceResolutionStatus.Expired)
                return SelectedTargetSessionExecutionResult.Expired();
            if (claim.Status != SelectedTargetEvidenceResolutionStatus.Found
                || claim.Evidence is null)
            {
                return SelectedTargetSessionExecutionResult.Unknown();
            }

            var request = new
                DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest(
                    runId,
                    scenarioId,
                    userRequest,
                    claim.Evidence);
            var result = await entry.ExecuteAdmittedSelectedTargetAsync(
                    request,
                    cancellationToken)
                .ConfigureAwait(false);
            return SelectedTargetSessionExecutionResult.Completed(result);
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

    private async Task<SessionOperationResult<TResult>> ExecuteSessionOperationAsync<
        TRequest,
        TResult>(
        string tenantId,
        string sessionId,
        TRequest request,
        Func<bool> executionAllowed,
        Func<SessionEntry, bool> capabilityAvailable,
        Func<SessionEntry, TRequest, CancellationToken, Task<OperationResult<TResult>>> execute,
        CancellationToken cancellationToken)
    {
        if (IsInvalidated || !executionAllowed())
            return new SessionOperationResult<TResult>(SessionOperationStatus.ForbiddenAssembly, null);

        var lookup = GetCore(tenantId, sessionId);
        if (lookup.Status == SessionLookupStatus.Expired)
            return new SessionOperationResult<TResult>(SessionOperationStatus.Expired, null);
        if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
            return new SessionOperationResult<TResult>(SessionOperationStatus.Unknown, null);
        if (!capabilityAvailable(lookup.Session))
            return new SessionOperationResult<TResult>(SessionOperationStatus.Unavailable, null);

        if (!await _runSlots.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return new SessionOperationResult<TResult>(SessionOperationStatus.Busy, null);

        Exception? executionFailure = null;
        try
        {
            if (IsInvalidated || !executionAllowed())
                return new SessionOperationResult<TResult>(
                    SessionOperationStatus.ForbiddenAssembly,
                    null);

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return new SessionOperationResult<TResult>(SessionOperationStatus.Expired, null);
            if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
                return new SessionOperationResult<TResult>(SessionOperationStatus.Unknown, null);
            if (!capabilityAvailable(lookup.Session))
                return new SessionOperationResult<TResult>(SessionOperationStatus.Unavailable, null);

            var result = await execute(lookup.Session, request, cancellationToken)
                .ConfigureAwait(false);
            if (IsInvalidated || !executionAllowed())
            {
                return new SessionOperationResult<TResult>(
                    SessionOperationStatus.ForbiddenAssembly,
                    null);
            }
            return new SessionOperationResult<TResult>(SessionOperationStatus.Completed, result);
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

    internal SelectedTargetEvidenceAdmissionResult AdmitSelectedTargetEvidence(
        string tenantId,
        string sessionId,
        DynamicGraphMeasurementTarget? target,
        EdgeAuthorityExecutionResult? authorityResult,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest,
        Func<bool> executionAllowed,
        CancellationToken cancellationToken)
    {
        using var operationLease = EnterOperation();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentNullException.ThrowIfNull(executionAllowed);

        if (IsInvalidated)
            return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();
        if (!executionAllowed() || IsInvalidated)
            return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();

        var lookup = GetCore(tenantId, sessionId);
        if (lookup.Status == SessionLookupStatus.Expired)
            return SelectedTargetEvidenceAdmissionResult.Expired();
        if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
            return SelectedTargetEvidenceAdmissionResult.Unknown();

        var entry = lookup.Session;
        var reservationAttempt = entry.ReserveSelectedTargetEvidence();
        if (reservationAttempt.Status == EvidenceReservationStatus.Revoked)
            return ResolveAdmissionLifecycle(tenantId, sessionId, entry);
        if (reservationAttempt.Status == EvidenceReservationStatus.CapacityExceeded)
            return SelectedTargetEvidenceAdmissionResult.CapacityExceeded();

        var reservation = reservationAttempt.Reservation!;
        Exception? executionFailure = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsInvalidated)
                return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();
            if (!executionAllowed() || IsInvalidated)
                return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return SelectedTargetEvidenceAdmissionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found
                || lookup.Session is null
                || !ReferenceEquals(lookup.Session, entry))
            {
                return SelectedTargetEvidenceAdmissionResult.Unknown();
            }

            var evidenceResult = _selectedTargetEvidenceInvoker.Invoke(
                target,
                authorityResult,
                noveltyRequest);

            // Cancellation observed during a synchronous invoker must win before its envelope is
            // inspected or any evidence can be published.
            cancellationToken.ThrowIfCancellationRequested();

            if (evidenceResult is null
                || (evidenceResult.IsSuccess && evidenceResult.Value is null))
            {
                return RequiredSelectedTargetEvidenceAdmissionResult();
            }
            if (!evidenceResult.IsSuccess)
            {
                return SelectedTargetEvidenceAdmissionResult.Completed(
                    evidenceResult.MapError<SelectedTargetEvidenceHandle>());
            }

            var ownedAssociation = CopySelectedTargetEvidence(evidenceResult.Value!);

            // No caller callback runs under either registry lock. Reentrant invalidation or
            // rollback performed by the callback is observed immediately and by the following
            // identity-bound lookup.
            if (IsInvalidated)
                return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();
            if (!executionAllowed() || IsInvalidated)
                return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();
            lock (_cleanupGate)
            {
                if (IsInvalidated)
                    return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();

                lookup = GetCore(tenantId, sessionId);
                if (lookup.Status == SessionLookupStatus.Expired)
                    return SelectedTargetEvidenceAdmissionResult.Expired();
                if (lookup.Status != SessionLookupStatus.Found
                    || lookup.Session is null
                    || !ReferenceEquals(lookup.Session, entry))
                {
                    return SelectedTargetEvidenceAdmissionResult.Unknown();
                }
            }

            // This is the final caller callback. It is deliberately before the transactional
            // commit lock, so false, throwing, rollback, and invalidation outcomes cannot leave
            // a published handle behind.
            if (IsInvalidated)
                return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();
            if (!executionAllowed() || IsInvalidated)
                return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();

            lock (_cleanupGate)
            {
                if (IsInvalidated)
                    return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();

                lookup = GetCore(tenantId, sessionId);
                if (lookup.Status == SessionLookupStatus.Expired)
                    return SelectedTargetEvidenceAdmissionResult.Expired();
                if (lookup.Status != SessionLookupStatus.Found
                    || lookup.Session is null
                    || !ReferenceEquals(lookup.Session, entry))
                {
                    return SelectedTargetEvidenceAdmissionResult.Unknown();
                }

                var publication = entry.PublishSelectedTargetEvidence(
                    reservation,
                    ownedAssociation,
                    _selectedTargetEvidenceHandleFactory);
                if (publication.Status == EvidencePublicationStatus.Revoked)
                    return ResolveAdmissionLifecycle(tenantId, sessionId, entry);
                if (publication.Status == EvidencePublicationStatus.AllocationFailed)
                    return FailedSelectedTargetEvidenceHandleAllocation();

                return SelectedTargetEvidenceAdmissionResult.Completed(
                    OperationResult<SelectedTargetEvidenceHandle>.Ok(publication.Handle!));
            }
        }
        catch (Exception exception)
        {
            executionFailure = exception;
            throw;
        }
        finally
        {
            if (executionFailure is null || IsNonFatalException(executionFailure))
                entry.ReleaseSelectedTargetEvidenceReservation(reservation);
        }
    }

    internal SelectedTargetEvidenceResolutionResult ResolveSelectedTargetEvidence(
        string tenantId,
        string sessionId,
        string handle,
        Func<bool> executionAllowed)
    {
        using var operationLease = EnterOperation();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(handle);
        ArgumentNullException.ThrowIfNull(executionAllowed);

        if (IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();
        if (!executionAllowed() || IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

        var lookup = GetCore(tenantId, sessionId);
        if (lookup.Status == SessionLookupStatus.Expired)
            return SelectedTargetEvidenceResolutionResult.Expired();
        if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
            return SelectedTargetEvidenceResolutionResult.Unknown();

        var entry = lookup.Session;
        if (entry.ResolveSelectedTargetEvidence(handle) is null)
            return SelectedTargetEvidenceResolutionResult.Unknown();

        if (IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();
        if (!executionAllowed() || IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

        lock (_cleanupGate)
        {
            if (IsInvalidated)
                return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return SelectedTargetEvidenceResolutionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found
                || lookup.Session is null
                || !ReferenceEquals(lookup.Session, entry))
            {
                return SelectedTargetEvidenceResolutionResult.Unknown();
            }

            if (entry.ResolveSelectedTargetEvidence(handle) is null)
                return SelectedTargetEvidenceResolutionResult.Unknown();
        }

        if (IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();
        if (!executionAllowed() || IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

        lock (_cleanupGate)
        {
            if (IsInvalidated)
                return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return SelectedTargetEvidenceResolutionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found
                || lookup.Session is null
                || !ReferenceEquals(lookup.Session, entry))
            {
                return SelectedTargetEvidenceResolutionResult.Unknown();
            }

            var evidence = entry.ResolveSelectedTargetEvidence(handle);
            return evidence is null
                ? SelectedTargetEvidenceResolutionResult.Unknown()
                : SelectedTargetEvidenceResolutionResult.Found(evidence);
        }
    }

    internal SelectedTargetEvidenceResolutionResult ClaimSelectedTargetEvidence(
        string tenantId,
        string sessionId,
        string handle,
        Func<bool> executionAllowed)
    {
        using var operationLease = EnterOperation();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(handle);
        ArgumentNullException.ThrowIfNull(executionAllowed);

        if (IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();
        if (!executionAllowed() || IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

        var lookup = GetCore(tenantId, sessionId);
        if (lookup.Status == SessionLookupStatus.Expired)
            return SelectedTargetEvidenceResolutionResult.Expired();
        if (lookup.Status != SessionLookupStatus.Found || lookup.Session is null)
            return SelectedTargetEvidenceResolutionResult.Unknown();

        var entry = lookup.Session;
        if (entry.ResolveSelectedTargetEvidence(handle) is null)
            return SelectedTargetEvidenceResolutionResult.Unknown();

        if (IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();
        if (!executionAllowed() || IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

        lock (_cleanupGate)
        {
            if (IsInvalidated)
                return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return SelectedTargetEvidenceResolutionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found
                || lookup.Session is null
                || !ReferenceEquals(lookup.Session, entry))
            {
                return SelectedTargetEvidenceResolutionResult.Unknown();
            }

            if (entry.ResolveSelectedTargetEvidence(handle) is null)
                return SelectedTargetEvidenceResolutionResult.Unknown();
        }

        if (IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();
        if (!executionAllowed() || IsInvalidated)
            return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

        lock (_cleanupGate)
        {
            if (IsInvalidated)
                return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

            lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return SelectedTargetEvidenceResolutionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found
                || lookup.Session is null
                || !ReferenceEquals(lookup.Session, entry))
            {
                return SelectedTargetEvidenceResolutionResult.Unknown();
            }

            var evidence = entry.ClaimSelectedTargetEvidence(handle);
            return evidence is null
                ? SelectedTargetEvidenceResolutionResult.Unknown()
                : SelectedTargetEvidenceResolutionResult.Found(evidence);
        }
    }

    private SelectedTargetEvidenceResolutionResult ClaimSameEntrySelectedTargetEvidence(
        string tenantId,
        string sessionId,
        string handle,
        SessionEntry expectedEntry)
    {
        lock (_cleanupGate)
        {
            if (IsInvalidated)
                return SelectedTargetEvidenceResolutionResult.ForbiddenAssembly();

            var lookup = GetCore(tenantId, sessionId);
            if (lookup.Status == SessionLookupStatus.Expired)
                return SelectedTargetEvidenceResolutionResult.Expired();
            if (lookup.Status != SessionLookupStatus.Found
                || lookup.Session is null
                || !ReferenceEquals(lookup.Session, expectedEntry))
            {
                return SelectedTargetEvidenceResolutionResult.Unknown();
            }

            var evidence = expectedEntry.ClaimSelectedTargetEvidence(handle);
            return evidence is null
                ? SelectedTargetEvidenceResolutionResult.Unknown()
                : SelectedTargetEvidenceResolutionResult.Found(evidence);
        }
    }

    private SelectedTargetEvidenceAdmissionResult ResolveAdmissionLifecycle(
        string tenantId,
        string sessionId,
        SessionEntry expectedEntry)
    {
        if (IsInvalidated)
            return SelectedTargetEvidenceAdmissionResult.ForbiddenAssembly();

        var lookup = GetCore(tenantId, sessionId);
        if (lookup.Status == SessionLookupStatus.Expired)
            return SelectedTargetEvidenceAdmissionResult.Expired();
        if (lookup.Status != SessionLookupStatus.Found
            || lookup.Session is null
            || !ReferenceEquals(lookup.Session, expectedEntry))
        {
            return SelectedTargetEvidenceAdmissionResult.Unknown();
        }
        return SelectedTargetEvidenceAdmissionResult.Unknown();
    }

    private static SelectedTargetEvidenceAdmissionResult
        RequiredSelectedTargetEvidenceAdmissionResult()
        => SelectedTargetEvidenceAdmissionResult.Completed(
            OperationResult<SelectedTargetEvidenceHandle>.Fail(
                SelectedTargetEvidenceAdmissionResultRequiredMessage,
                SelectedTargetEvidenceAdmissionResultRequiredCode));

    private static SelectedTargetEvidenceAdmissionResult
        FailedSelectedTargetEvidenceHandleAllocation()
        => SelectedTargetEvidenceAdmissionResult.Completed(
            OperationResult<SelectedTargetEvidenceHandle>.Fail(
                SelectedTargetEvidenceHandleAllocationFailedMessage,
                SelectedTargetEvidenceHandleAllocationFailedCode));

    private static DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
        CopySelectedTargetEvidence(
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement source)
        => new(source.Target, source.EdgeAuthoritySelectionQualityEvidence);

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

            // A supplied stale entry must lose all of its process-local evidence even when it is
            // no longer the dictionary owner. Persistence deletion is deliberately attempted
            // first so its existing failure semantics remain fail closed.
            entry.RevokeSelectedTargetEvidence();
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
            var entries = _sessions.Values.ToArray();
            foreach (var entry in entries)
                entry.RevokeSelectedTargetEvidence();
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
        entry.RevokeSelectedTargetEvidence();
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

    private static bool CheckpointCaptureEquals(
        DynamicGraphLogicalOrchestratorCheckpointCapture capture,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
        => capture.Revision == checkpoint.Revision
           && capture.SourceKind == checkpoint.SourceKind
           && capture.InitialRevisionAdvanced == checkpoint.InitialRevisionAdvanced
           && string.Equals(capture.SignalId, profile.SignalId, StringComparison.Ordinal)
           && string.Equals(capture.SourceRef, profile.SourceRef, StringComparison.Ordinal)
           && string.Equals(capture.TraceRef, profile.TraceRef, StringComparison.Ordinal)
           && capture.NodeCount == profile.NodeDefinitions.Length
           && capture.EdgeCount == profile.EdgeBindings.Length
           && BudgetProofEquals(capture.BudgetProof, checkpoint.BudgetProof)
           && MutationEvidenceEquals(
               capture.MutationEvidence,
               checkpoint.MutationEvidence);

    private static bool CurrentReadEqualsCheckpoint(
        DynamicGraphLogicalOrchestratorSessionResult current,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
        => current.Revision == checkpoint.Revision
           && current.SourceKind == checkpoint.SourceKind
           && current.InitialRevisionAdvanced == checkpoint.InitialRevisionAdvanced
           && current.NodeCount == profile.NodeDefinitions.Length
           && current.EdgeCount == profile.EdgeBindings.Length
           && string.Equals(current.SignalId, profile.SignalId, StringComparison.Ordinal)
           && string.Equals(
               current.Descriptor.ProfileId,
               profile.ProfileId,
               StringComparison.Ordinal)
           && string.Equals(
               current.Descriptor.TenantId,
               profile.TenantId,
               StringComparison.Ordinal)
           && string.Equals(
               current.Descriptor.DomainTag,
               profile.DomainTag,
               StringComparison.Ordinal)
           && string.Equals(
               current.Descriptor.ContextId,
               profile.ContextId,
               StringComparison.Ordinal);

    private static bool BudgetProofEquals(
        TopologyBudgetProof left,
        TopologyBudgetProof right)
        => left.MaxNodes == right.MaxNodes
           && left.ActualNodes == right.ActualNodes
           && left.MaxEdges == right.MaxEdges
           && left.ActualEdges == right.ActualEdges
           && left.MaxDepth == right.MaxDepth
           && left.ActualDepth == right.ActualDepth
           && left.MaxParallelBranches == right.MaxParallelBranches
           && left.ActualParallelBranches == right.ActualParallelBranches
           && left.WithinBudget == right.WithinBudget
           && left.ExceededDimensions.SequenceEqual(
               right.ExceededDimensions,
               StringComparer.Ordinal);

    private static bool MutationEvidenceEquals(
        TopologyMutationEvidence left,
        TopologyMutationEvidence right)
        => string.Equals(left.SignalId, right.SignalId, StringComparison.Ordinal)
           && left.SourceKind == right.SourceKind
           && string.Equals(left.SourceRef, right.SourceRef, StringComparison.Ordinal)
           && string.Equals(left.TraceRef, right.TraceRef, StringComparison.Ordinal)
           && left.MutationApplied == right.MutationApplied
           && left.ProposedAddedNodeIds.SequenceEqual(
               right.ProposedAddedNodeIds,
               StringComparer.Ordinal)
           && left.ProposedRemovedNodeIds.SequenceEqual(
               right.ProposedRemovedNodeIds,
               StringComparer.Ordinal)
           && left.ProposedUpdatedNodeIds.SequenceEqual(
               right.ProposedUpdatedNodeIds,
               StringComparer.Ordinal)
           && left.ProposedAddedEdgeIds.SequenceEqual(
               right.ProposedAddedEdgeIds,
               StringComparer.Ordinal)
           && left.ProposedRemovedEdgeIds.SequenceEqual(
               right.ProposedRemovedEdgeIds,
               StringComparer.Ordinal)
           && left.ProposedUpdatedEdgeIds.SequenceEqual(
               right.ProposedUpdatedEdgeIds,
               StringComparer.Ordinal)
           && left.ProposedReorderedNodeIds.SequenceEqual(
               right.ProposedReorderedNodeIds,
               StringComparer.Ordinal);

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

    private enum SessionOperationStatus
    {
        Completed,
        Expired,
        Unknown,
        Busy,
        ForbiddenAssembly,
        Unavailable
    }

    private sealed record SessionOperationResult<TResult>(
        SessionOperationStatus Status,
        OperationResult<TResult>? Result);

    internal enum EvidenceReservationStatus
    {
        Reserved,
        CapacityExceeded,
        Revoked
    }

    internal sealed record EvidenceReservationAttempt(
        EvidenceReservationStatus Status,
        EvidenceReservation? Reservation);

    internal enum EvidencePublicationStatus
    {
        Published,
        AllocationFailed,
        Revoked
    }

    internal sealed record EvidencePublicationResult(
        EvidencePublicationStatus Status,
        SelectedTargetEvidenceHandle? Handle);

    internal sealed class EvidenceReservation
    {
        private readonly SessionEntry _owner;
        private int _consumed;

        internal EvidenceReservation(SessionEntry owner)
        {
            _owner = owner;
        }

        internal bool IsLiveFor(SessionEntry owner)
            => ReferenceEquals(_owner, owner) && Volatile.Read(ref _consumed) == 0;

        internal bool TryConsume(SessionEntry owner)
            => ReferenceEquals(_owner, owner)
               && Interlocked.Exchange(ref _consumed, 1) == 0;
    }

    public sealed class SessionEntry
    {
        private readonly IDynamicGraphLogicalOrchestratorRunSession _runSession;
        private readonly object _evidenceGate = new();
        private readonly Dictionary<
            string,
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
            _selectedTargetEvidence = new(StringComparer.Ordinal);
        private readonly HashSet<string> _retiredSelectedTargetEvidenceHandleIds =
            new(StringComparer.Ordinal);
        private int _selectedTargetEvidenceReservationCount;
        private bool _selectedTargetEvidenceRevoked;

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

        internal bool SupportsTopologyRun
            => _runSession is IDynamicGraphLogicalOrchestratorTopologyRunSession;

        internal bool SupportsTopologyRead
            => _runSession is IDynamicGraphLogicalOrchestratorCompositionSession;

        internal DynamicGraphLogicalOrchestratorSessionDescriptor TopologyReadDescriptor
            => ((IDynamicGraphLogicalOrchestratorCompositionSession)_runSession).Descriptor;

        internal OperationResult<DynamicGraphLogicalOrchestratorSessionResult>
            ReadCurrentTopology()
            => ((IDynamicGraphLogicalOrchestratorCompositionSession)_runSession).ReadCurrent();

        internal Task<OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>
            ExecuteTopologyRunAsync(
                DynamicGraphLogicalOrchestratorRunRequest request,
                CancellationToken cancellationToken)
            => ((IDynamicGraphLogicalOrchestratorTopologyRunSession)_runSession)
                .RunOnCurrentTopologyAsync(request, cancellationToken);

        internal bool SupportsTopologyMutation
            => _runSession is IDynamicGraphLogicalOrchestratorCompositionSession
               && _runSession is IDynamicGraphLogicalOrchestratorTopologyMutationSession;

        internal DynamicGraphLogicalOrchestratorSessionDescriptor TopologyMutationDescriptor
            => ((IDynamicGraphLogicalOrchestratorCompositionSession)_runSession).Descriptor;

        internal OperationResult<DynamicGraphTopologyApplyResult> ApplyRegistryProjection(
            long expectedRevision,
            DynamicGraphRegistryProjectionRequest request,
            TopologyBuildBudget budget)
            => ((IDynamicGraphLogicalOrchestratorTopologyMutationSession)_runSession)
                .ApplyRegistryProjection(expectedRevision, request, budget);

        internal bool SupportsSelectedTargetExecution
            => _runSession is IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession;

        internal bool SupportsAdmittedSelectedTargetExecution
            => _runSession is
                IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession;

        internal Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
            ExecuteSelectedTargetAsync(
                DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest request,
                CancellationToken cancellationToken)
            => ((IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession)_runSession)
                .RunAsync(request, cancellationToken);

        internal Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
            ExecuteAdmittedSelectedTargetAsync(
                DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest request,
                CancellationToken cancellationToken)
            => ((IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession)_runSession)
                .RunAdmittedAsync(request, cancellationToken);

        internal EvidenceReservationAttempt ReserveSelectedTargetEvidence()
        {
            lock (_evidenceGate)
            {
                if (_selectedTargetEvidenceRevoked)
                    return new(EvidenceReservationStatus.Revoked, null);
                if (_selectedTargetEvidence.Count + _selectedTargetEvidenceReservationCount
                    >= SelectedTargetEvidenceCapacity)
                {
                    return new(EvidenceReservationStatus.CapacityExceeded, null);
                }

                _selectedTargetEvidenceReservationCount++;
                return new(
                    EvidenceReservationStatus.Reserved,
                    new EvidenceReservation(this));
            }
        }

        internal EvidencePublicationResult PublishSelectedTargetEvidence(
            EvidenceReservation reservation,
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement evidence,
            Func<string> handleFactory)
        {
            lock (_evidenceGate)
            {
                if (!reservation.IsLiveFor(this))
                    throw new InvalidOperationException("The selected-target evidence reservation is invalid.");
                if (_selectedTargetEvidenceRevoked)
                    return new(EvidencePublicationStatus.Revoked, null);

                for (var attempt = 0; attempt < SelectedTargetEvidenceHandleAttempts; attempt++)
                {
                    var handleId = handleFactory();
                    if (handleId is null
                        || !SelectedTargetEvidenceHandlePattern.IsMatch(handleId)
                        || _selectedTargetEvidence.ContainsKey(handleId)
                        || _retiredSelectedTargetEvidenceHandleIds.Contains(handleId))
                    {
                        continue;
                    }

                    var handle = new SelectedTargetEvidenceHandle(handleId, ExpiresAtUtc);
                    _selectedTargetEvidence.Add(handleId, evidence);
                    if (!reservation.TryConsume(this))
                    {
                        _selectedTargetEvidence.Remove(handleId);
                        throw new InvalidOperationException(
                            "The selected-target evidence reservation was already consumed.");
                    }

                    _selectedTargetEvidenceReservationCount--;
                    return new(EvidencePublicationStatus.Published, handle);
                }

                return new(EvidencePublicationStatus.AllocationFailed, null);
            }
        }

        internal void ReleaseSelectedTargetEvidenceReservation(EvidenceReservation reservation)
        {
            lock (_evidenceGate)
            {
                if (!reservation.TryConsume(this))
                    return;
                _selectedTargetEvidenceReservationCount--;
            }
        }

        internal DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement?
            ResolveSelectedTargetEvidence(string handleId)
        {
            lock (_evidenceGate)
            {
                if (_selectedTargetEvidenceRevoked
                    || !_selectedTargetEvidence.TryGetValue(handleId, out var evidence))
                {
                    return null;
                }

                return CopySelectedTargetEvidence(evidence);
            }
        }

        internal DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement?
            ClaimSelectedTargetEvidence(string handleId)
        {
            lock (_evidenceGate)
            {
                if (_selectedTargetEvidenceRevoked
                    || _retiredSelectedTargetEvidenceHandleIds.Contains(handleId)
                    || !_selectedTargetEvidence.ContainsKey(handleId))
                {
                    return null;
                }

                if (!_retiredSelectedTargetEvidenceHandleIds.Add(handleId))
                {
                    throw new InvalidOperationException(
                        "The selected-target evidence handle retirement invariant is invalid.");
                }
                if (!_selectedTargetEvidence.Remove(handleId, out var evidence)
                    || evidence is null)
                {
                    throw new InvalidOperationException(
                        "The selected-target evidence claim invariant is invalid.");
                }

                return evidence;
            }
        }

        internal void RemoveSelectedTargetEvidence(string handleId)
        {
            lock (_evidenceGate)
                _selectedTargetEvidence.Remove(handleId);
        }

        internal void RevokeSelectedTargetEvidence()
        {
            lock (_evidenceGate)
            {
                _selectedTargetEvidenceRevoked = true;
                _selectedTargetEvidence.Clear();
                _retiredSelectedTargetEvidenceHandleIds.Clear();
            }
        }
    }

    private sealed class DescriptorOnlyDynamicGraphNode : IDynamicGraphNode
    {
        internal DescriptorOnlyDynamicGraphNode(DynamicGraphNodeDescriptor descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            Descriptor = descriptor with { };
        }

        public string NodeId => Descriptor.NodeId;

        public DynamicGraphNodeDescriptor Descriptor { get; }
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

internal interface IDynamicGraphSelectedTargetEvidenceInvoker
{
    OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement> Invoke(
        DynamicGraphMeasurementTarget? target,
        EdgeAuthorityExecutionResult? authorityResult,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest);
}

internal sealed class DynamicGraphSelectedTargetEvidenceInvoker
    : IDynamicGraphSelectedTargetEvidenceInvoker
{
    public OperationResult<DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement>
        Invoke(
            DynamicGraphMeasurementTarget? target,
            EdgeAuthorityExecutionResult? authorityResult,
            DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest)
        => DynamicGraphMeasurementCoordinator.MeasureEdgeAuthoritySelectionQualityEvidence(
            target,
            authorityResult,
            noveltyRequest);
}

internal sealed record SelectedTargetEvidenceHandle(
    string HandleId,
    DateTimeOffset ExpiresAtUtc);

internal enum SelectedTargetEvidenceAdmissionStatus
{
    Completed,
    Expired,
    Unknown,
    CapacityExceeded,
    ForbiddenAssembly
}

internal sealed record SelectedTargetEvidenceAdmissionResult(
    SelectedTargetEvidenceAdmissionStatus Status,
    OperationResult<SelectedTargetEvidenceHandle>? Result)
{
    internal static SelectedTargetEvidenceAdmissionResult Completed(
        OperationResult<SelectedTargetEvidenceHandle> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new(SelectedTargetEvidenceAdmissionStatus.Completed, result);
    }

    internal static SelectedTargetEvidenceAdmissionResult Expired()
        => new(SelectedTargetEvidenceAdmissionStatus.Expired, null);

    internal static SelectedTargetEvidenceAdmissionResult Unknown()
        => new(SelectedTargetEvidenceAdmissionStatus.Unknown, null);

    internal static SelectedTargetEvidenceAdmissionResult CapacityExceeded()
        => new(SelectedTargetEvidenceAdmissionStatus.CapacityExceeded, null);

    internal static SelectedTargetEvidenceAdmissionResult ForbiddenAssembly()
        => new(SelectedTargetEvidenceAdmissionStatus.ForbiddenAssembly, null);
}

internal enum SelectedTargetEvidenceResolutionStatus
{
    Found,
    Expired,
    Unknown,
    ForbiddenAssembly
}

internal sealed record SelectedTargetEvidenceResolutionResult(
    SelectedTargetEvidenceResolutionStatus Status,
    DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? Evidence)
{
    internal static SelectedTargetEvidenceResolutionResult Found(
        DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        return new(SelectedTargetEvidenceResolutionStatus.Found, evidence);
    }

    internal static SelectedTargetEvidenceResolutionResult Expired()
        => new(SelectedTargetEvidenceResolutionStatus.Expired, null);

    internal static SelectedTargetEvidenceResolutionResult Unknown()
        => new(SelectedTargetEvidenceResolutionStatus.Unknown, null);

    internal static SelectedTargetEvidenceResolutionResult ForbiddenAssembly()
        => new(SelectedTargetEvidenceResolutionStatus.ForbiddenAssembly, null);
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

internal enum CurrentTopologySessionStatus
{
    Completed,
    Expired,
    Unknown,
    Unavailable,
    ForbiddenAssembly
}

internal sealed record CurrentTopologySessionResult(
    CurrentTopologySessionStatus Status,
    DynamicGraphCompositionSessionRegistry.SessionEntry? Session,
    OperationResult<DynamicGraphLogicalOrchestratorSessionResult>? Result)
{
    internal static CurrentTopologySessionResult Completed(
        DynamicGraphCompositionSessionRegistry.SessionEntry session,
        OperationResult<DynamicGraphLogicalOrchestratorSessionResult>? result)
    {
        ArgumentNullException.ThrowIfNull(session);
        return new(CurrentTopologySessionStatus.Completed, session, result);
    }

    internal static CurrentTopologySessionResult Expired()
        => new(CurrentTopologySessionStatus.Expired, null, null);

    internal static CurrentTopologySessionResult Unknown()
        => new(CurrentTopologySessionStatus.Unknown, null, null);

    internal static CurrentTopologySessionResult Unavailable()
        => new(CurrentTopologySessionStatus.Unavailable, null, null);

    internal static CurrentTopologySessionResult ForbiddenAssembly()
        => new(CurrentTopologySessionStatus.ForbiddenAssembly, null, null);
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

public enum TopologySessionExecutionStatus
{
    Completed,
    Expired,
    Unknown,
    Busy,
    ForbiddenAssembly,
    Unavailable
}

public sealed record TopologySessionExecutionResult(
    TopologySessionExecutionStatus Status,
    OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>? Result)
{
    public static TopologySessionExecutionResult Completed(
        OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>? result)
        => new(TopologySessionExecutionStatus.Completed, result);

    public static TopologySessionExecutionResult Expired()
        => new(TopologySessionExecutionStatus.Expired, null);

    public static TopologySessionExecutionResult Unknown()
        => new(TopologySessionExecutionStatus.Unknown, null);

    public static TopologySessionExecutionResult Busy()
        => new(TopologySessionExecutionStatus.Busy, null);

    public static TopologySessionExecutionResult ForbiddenAssembly()
        => new(TopologySessionExecutionStatus.ForbiddenAssembly, null);

    public static TopologySessionExecutionResult Unavailable()
        => new(TopologySessionExecutionStatus.Unavailable, null);
}

internal enum TopologyMutationSessionStatus
{
    Completed,
    Expired,
    Unknown,
    Busy,
    ForbiddenAssembly,
    Unavailable,
    EphemeralModeRequired
}

internal sealed record TopologyMutationSessionResult(
    TopologyMutationSessionStatus Status,
    OperationResult<DynamicGraphTopologyApplyResult>? Result)
{
    public static TopologyMutationSessionResult Completed(
        OperationResult<DynamicGraphTopologyApplyResult>? result)
        => new(TopologyMutationSessionStatus.Completed, result);

    public static TopologyMutationSessionResult Expired()
        => new(TopologyMutationSessionStatus.Expired, null);

    public static TopologyMutationSessionResult Unknown()
        => new(TopologyMutationSessionStatus.Unknown, null);

    public static TopologyMutationSessionResult Busy()
        => new(TopologyMutationSessionStatus.Busy, null);

    public static TopologyMutationSessionResult ForbiddenAssembly()
        => new(TopologyMutationSessionStatus.ForbiddenAssembly, null);

    public static TopologyMutationSessionResult Unavailable()
        => new(TopologyMutationSessionStatus.Unavailable, null);

    public static TopologyMutationSessionResult EphemeralModeRequired()
        => new(TopologyMutationSessionStatus.EphemeralModeRequired, null);
}

public enum SelectedTargetSessionExecutionStatus
{
    Completed,
    Expired,
    Unknown,
    Busy,
    ForbiddenAssembly,
    Unavailable
}

public sealed record SelectedTargetSessionExecutionResult(
    SelectedTargetSessionExecutionStatus Status,
    OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>? Result)
{
    public static SelectedTargetSessionExecutionResult Completed(
        OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>? result)
        => new(SelectedTargetSessionExecutionStatus.Completed, result);

    public static SelectedTargetSessionExecutionResult Expired()
        => new(SelectedTargetSessionExecutionStatus.Expired, null);

    public static SelectedTargetSessionExecutionResult Unknown()
        => new(SelectedTargetSessionExecutionStatus.Unknown, null);

    public static SelectedTargetSessionExecutionResult Busy()
        => new(SelectedTargetSessionExecutionStatus.Busy, null);

    public static SelectedTargetSessionExecutionResult ForbiddenAssembly()
        => new(SelectedTargetSessionExecutionStatus.ForbiddenAssembly, null);

    public static SelectedTargetSessionExecutionResult Unavailable()
        => new(SelectedTargetSessionExecutionStatus.Unavailable, null);
}
