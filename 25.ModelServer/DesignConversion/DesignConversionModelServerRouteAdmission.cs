// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionModelServerRouteAdmissionOptions
{
    public DesignConversionModelServerRouteAdmissionOptions(
        bool enabled = false,
        string? currentToken = null,
        string? previousToken = null,
        DateTimeOffset? previousTokenExpiresAtUtc = null,
        int idempotencyCapacity = DesignConversionModelServerRouteAdmission.DefaultIdempotencyCapacity,
        TimeSpan? idempotencyTtl = null,
        string? routeScope = null,
        string? tenantId = null,
        string? durableIdempotencyStoreRoot = null,
        int maxConcurrentWorkloads = DesignConversionModelServerRouteAdmission.DefaultMaxConcurrentWorkloads,
        int requestQuotaLimit = DesignConversionModelServerRouteAdmission.DefaultRequestQuotaLimit,
        TimeSpan? requestQuotaWindow = null)
    {
        Enabled = enabled;
        CurrentToken = currentToken;
        PreviousToken = previousToken;
        PreviousTokenExpiresAtUtc = previousTokenExpiresAtUtc;
        IdempotencyCapacity = idempotencyCapacity;
        IdempotencyTtl = idempotencyTtl ?? DesignConversionModelServerRouteAdmission.DefaultIdempotencyTtl;
        RouteScope = routeScope;
        TenantId = tenantId;
        DurableIdempotencyStoreRoot = durableIdempotencyStoreRoot;
        MaxConcurrentWorkloads = maxConcurrentWorkloads;
        RequestQuotaLimit = requestQuotaLimit;
        RequestQuotaWindow = requestQuotaWindow ?? DesignConversionModelServerRouteAdmission.DefaultRequestQuotaWindow;
    }

    public bool Enabled { get; }

    public string? CurrentToken { get; }

    public string? PreviousToken { get; }

    public DateTimeOffset? PreviousTokenExpiresAtUtc { get; }

    public int IdempotencyCapacity { get; }

    public TimeSpan IdempotencyTtl { get; }

    public string? RouteScope { get; }

    public string? TenantId { get; }

    public string? DurableIdempotencyStoreRoot { get; }

    public int MaxConcurrentWorkloads { get; }

    public int RequestQuotaLimit { get; }

    public TimeSpan RequestQuotaWindow { get; }
}

public sealed record DesignConversionModelServerRouteAdmissionContext(
    bool TenantBound,
    string? TenantId,
    string TenantIdRef,
    string Scope,
    string ScopeRef);

public sealed record DesignConversionModelServerRouteAdmissionDecision(
    bool IsAccepted,
    string Scope,
    string? IdempotencyKey,
    int StatusCode,
    string ErrorCode,
    string ErrorMessage,
    DesignConversionModelServerRouteAdmissionContext AdmissionContext,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerRouteAdmission
{
    public const string AuthorizationHeaderName = "Authorization";
    public const string ProjectAuthorizationHeaderName = "x-design-conversion-live-route-token";
    public const string IdempotencyHeaderName = "Idempotency-Key";
    public const string RouteScopeDefault = "design-conversion-live-route";
    public const string IdempotencyFailureReplayPolicy =
        "cache_and_replay_first_completed_response_for_same_scope_key_and_canonical_body";
    public const int DefaultIdempotencyCapacity = 256;
    public const int DefaultMaxConcurrentWorkloads = 2;
    public const int MaximumConcurrentWorkloads = 16;
    public const int MinimumRequestQuotaLimit = 1;
    public const int DefaultRequestQuotaLimit = 256;
    public const int MaximumRequestQuotaLimit = 4096;
    public const int MinimumRequestQuotaWindowSeconds = 1;
    public const int DefaultRequestQuotaWindowSeconds = 60;
    public const int MaximumRequestQuotaWindowSeconds = 3600;
    public static readonly TimeSpan DefaultIdempotencyTtl = TimeSpan.FromHours(6);
    public static readonly TimeSpan MinimumRequestQuotaWindow = TimeSpan.FromSeconds(MinimumRequestQuotaWindowSeconds);
    public static readonly TimeSpan DefaultRequestQuotaWindow = TimeSpan.FromSeconds(DefaultRequestQuotaWindowSeconds);
    public static readonly TimeSpan MaximumRequestQuotaWindow = TimeSpan.FromSeconds(MaximumRequestQuotaWindowSeconds);

    private const int MinimumTokenLength = 16;
    private const int MaximumTokenLength = 4096;
    private const int MaximumIdempotencyKeyLength = 128;
    private const int WorkloadRetryAfterSeconds = 1;
    private const int HandlerExceptionStatusCode = 500;
    private const int HandlerCancellationStatusCode = 499;
    private const string HandlerExceptionFailedClosedErrorRef = "handler_exception_failed_closed";
    private const string HandlerCancellationFailedClosedErrorRef = "handler_cancellation_failed_closed";
    private readonly byte[]? _currentTokenHash;
    private readonly byte[]? _previousTokenHash;
    private readonly DateTimeOffset? _previousTokenExpiresAtUtc;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly object _lifecycleLock = new();
    private readonly ConcurrentDictionary<string, IdempotencyEntry> _idempotencyEntries = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, JobScopeEntry> _jobScopes = new(StringComparer.Ordinal);
    private readonly int _idempotencyCapacity;
    private readonly int _maxConcurrentWorkloads;
    private readonly int _requestQuotaLimit;
    private readonly TimeSpan _requestQuotaWindow;
    private readonly SemaphoreSlim _workloadGate;
    private int _activeWorkloadCount;
    private DateTimeOffset _requestQuotaWindowStartedAtUtc;
    private int _requestQuotaCount;
    private readonly TimeSpan _idempotencyTtl;
    private readonly string _routeScope;
    private readonly string _tenantBoundScope;
    private readonly string _tenantScopeRef;
    private readonly string? _tenantId;
    private readonly string _tenantIdRef;
    private readonly string? _notReadyErrorCode;
    private readonly string? _notReadyReason;
    private readonly DesignConversionModelServerRouteAdmissionDurableStore? _durableReplayStore;

    private DesignConversionModelServerRouteAdmission(
        bool enabled,
        bool ready,
        byte[]? currentTokenHash,
        byte[]? previousTokenHash,
        DateTimeOffset? previousTokenExpiresAtUtc,
        string routeScope,
        string? tenantId,
        string? tenantIdRef,
        int idempotencyCapacity,
        TimeSpan idempotencyTtl,
        int maxConcurrentWorkloads,
        int requestQuotaLimit,
        TimeSpan requestQuotaWindow,
        string? durableIdempotencyStoreRoot,
        Func<DateTimeOffset>? utcNow,
        string? notReadyErrorCode,
        string? notReadyReason)
    {
        Enabled = enabled;
        Ready = ready;
        _currentTokenHash = currentTokenHash;
        _previousTokenHash = previousTokenHash;
        _previousTokenExpiresAtUtc = previousTokenExpiresAtUtc;
        _routeScope = string.IsNullOrWhiteSpace(routeScope) ? RouteScopeDefault : routeScope.Trim();
        _tenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
        _tenantIdRef = string.IsNullOrWhiteSpace(tenantIdRef) ? string.Empty : tenantIdRef.Trim();
        _tenantBoundScope = TenantBound
            ? _routeScope + "\n" + _tenantIdRef
            : _routeScope;
        _tenantScopeRef = ToHashRef("design-conversion-route-admission-scope", _tenantBoundScope);
        _idempotencyCapacity = Math.Clamp(idempotencyCapacity, 1, 4096);
        _maxConcurrentWorkloads = NormalizeMaxConcurrentWorkloads(maxConcurrentWorkloads);
        _requestQuotaLimit = NormalizeRequestQuotaLimit(requestQuotaLimit);
        _requestQuotaWindow = NormalizeRequestQuotaWindow(requestQuotaWindow);
        _workloadGate = new SemaphoreSlim(_maxConcurrentWorkloads, _maxConcurrentWorkloads);
        _idempotencyTtl = idempotencyTtl <= TimeSpan.Zero ? DefaultIdempotencyTtl : idempotencyTtl;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _notReadyErrorCode = notReadyErrorCode;
        _notReadyReason = notReadyReason;
        _durableReplayStore = Enabled && Ready
            ? DesignConversionModelServerRouteAdmissionDurableStore.CreateIfConfigured(durableIdempotencyStoreRoot)
            : null;
    }

    public bool Enabled { get; }

    public bool Ready { get; }

    public int MaxConcurrentWorkloads => _maxConcurrentWorkloads;

    public int RequestQuotaLimit => _requestQuotaLimit;

    public TimeSpan RequestQuotaWindow => _requestQuotaWindow;

    public bool TenantBound =>
        Enabled &&
        Ready &&
        !string.IsNullOrWhiteSpace(_tenantId) &&
        !string.IsNullOrWhiteSpace(_tenantIdRef);

    public bool PreviousTokenConfigured => _previousTokenHash is not null;

    public bool PreviousTokenActive =>
        _previousTokenHash is not null &&
        _previousTokenExpiresAtUtc is not null &&
        _utcNow() < _previousTokenExpiresAtUtc.Value;

    public int AcceptedKeyCount => Ready
        ? 1 + (PreviousTokenActive ? 1 : 0)
        : 0;

    public string ConfigurationStatus => !Enabled
        ? "disabled"
        : Ready
            ? "enabled"
            : "bounded_non_ready";

    public string RotationState => !Enabled || !Ready
        ? "not_active"
        : PreviousTokenConfigured
            ? PreviousTokenActive
                ? "previous_token_active"
                : "previous_token_retired"
            : "current_only";

    public static DesignConversionModelServerRouteAdmission Create(
        DesignConversionModelServerRouteAdmissionOptions? options,
        Func<DateTimeOffset>? utcNow = null)
    {
        if (options is null || !options.Enabled)
        {
            return new DesignConversionModelServerRouteAdmission(
                enabled: false,
                ready: false,
                currentTokenHash: null,
                previousTokenHash: null,
                previousTokenExpiresAtUtc: null,
                routeScope: RouteScopeDefault,
                tenantId: null,
                tenantIdRef: null,
                idempotencyCapacity: options?.IdempotencyCapacity ?? DefaultIdempotencyCapacity,
                idempotencyTtl: options?.IdempotencyTtl ?? DefaultIdempotencyTtl,
                maxConcurrentWorkloads: options?.MaxConcurrentWorkloads ?? DefaultMaxConcurrentWorkloads,
                requestQuotaLimit: options?.RequestQuotaLimit ?? DefaultRequestQuotaLimit,
                requestQuotaWindow: options?.RequestQuotaWindow ?? DefaultRequestQuotaWindow,
                durableIdempotencyStoreRoot: null,
                utcNow: utcNow,
                notReadyErrorCode: null,
                notReadyReason: null);
        }

        var current = BuildTokenHash(
            options.CurrentToken,
            "DESIGN_CONVERSION_ROUTE_ADMISSION_CURRENT_TOKEN_REQUIRED",
            "DESIGN_CONVERSION_ROUTE_ADMISSION_CURRENT_TOKEN_INVALID");
        if (!current.IsSuccess)
            return NotReady(options, utcNow, current.ErrorCode, StartupReason(current.ErrorCode));

        byte[]? previousHash = null;
        var hasPrevious = !string.IsNullOrWhiteSpace(options.PreviousToken);
        if (!hasPrevious && options.PreviousTokenExpiresAtUtc is not null)
        {
            CryptographicOperations.ZeroMemory(current.Value!);
            return NotReady(
                options,
                utcNow,
                "DESIGN_CONVERSION_ROUTE_ADMISSION_PREVIOUS_TOKEN_REQUIRED",
                "previous_token_missing");
        }

        if (hasPrevious)
        {
            if (options.PreviousTokenExpiresAtUtc is null)
            {
                CryptographicOperations.ZeroMemory(current.Value!);
                return NotReady(
                    options,
                    utcNow,
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_PREVIOUS_TOKEN_EXPIRATION_REQUIRED",
                    "previous_token_expiration_missing");
            }

            var previous = BuildTokenHash(
                options.PreviousToken,
                "DESIGN_CONVERSION_ROUTE_ADMISSION_PREVIOUS_TOKEN_REQUIRED",
                "DESIGN_CONVERSION_ROUTE_ADMISSION_PREVIOUS_TOKEN_INVALID");
            if (!previous.IsSuccess)
            {
                CryptographicOperations.ZeroMemory(current.Value!);
                return NotReady(options, utcNow, previous.ErrorCode, StartupReason(previous.ErrorCode));
            }

            if (CryptographicOperations.FixedTimeEquals(current.Value!, previous.Value!))
            {
                CryptographicOperations.ZeroMemory(current.Value!);
                CryptographicOperations.ZeroMemory(previous.Value!);
                return NotReady(
                    options,
                    utcNow,
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_CURRENT_PREVIOUS_TOKEN_MATCH",
                    "current_previous_token_match");
            }

            previousHash = previous.Value!;
        }

        var tenant = BuildTenantBinding(options.TenantId);
        if (!tenant.IsSuccess)
        {
            CryptographicOperations.ZeroMemory(current.Value!);
            if (previousHash is not null)
                CryptographicOperations.ZeroMemory(previousHash);

            return NotReady(options, utcNow, tenant.ErrorCode, StartupReason(tenant.ErrorCode));
        }

        return new DesignConversionModelServerRouteAdmission(
            enabled: true,
            ready: true,
            currentTokenHash: current.Value!,
            previousTokenHash: previousHash,
            previousTokenExpiresAtUtc: options.PreviousTokenExpiresAtUtc,
            routeScope: options.RouteScope ?? RouteScopeDefault,
            tenantId: tenant.Value!.TenantId,
            tenantIdRef: tenant.Value.TenantIdRef,
            idempotencyCapacity: options.IdempotencyCapacity,
            idempotencyTtl: options.IdempotencyTtl,
            maxConcurrentWorkloads: options.MaxConcurrentWorkloads,
            requestQuotaLimit: options.RequestQuotaLimit,
            requestQuotaWindow: options.RequestQuotaWindow,
            durableIdempotencyStoreRoot: options.DurableIdempotencyStoreRoot,
            utcNow: utcNow,
            notReadyErrorCode: null,
            notReadyReason: null);
    }

    public DesignConversionModelServerRouteAdmissionDecision AdmitEvaluate(
        DesignConversionModelServerHttpRequest request)
        => Admit(request, requireIdempotencyKey: true);

    public DesignConversionModelServerRouteAdmissionDecision AdmitIdempotentMutation(
        DesignConversionModelServerHttpRequest request)
        => Admit(request, requireIdempotencyKey: true, routeScopedIdempotency: true);

    public DesignConversionModelServerRouteAdmissionDecision AdmitStatus(
        DesignConversionModelServerHttpRequest request)
        => Admit(request, requireIdempotencyKey: false);

    public Task<DesignConversionModelServerHttpResponse> ExecuteEvaluateOnceAsync(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string bodyJson,
        Func<CancellationToken, Task<DesignConversionModelServerHttpResponse>> execute,
        Func<DesignConversionModelServerRouteAdmissionDecision, DesignConversionModelServerHttpResponse> buildConflict,
        Func<string, DesignConversionModelServerHttpResponse> buildInvalidJson,
        CancellationToken cancellationToken)
        => ExecuteIdempotentRequestOnceAsync(
            admission,
            bodyJson,
            execute,
            buildConflict,
            buildInvalidJson,
            cancellationToken,
            operationName: "evaluate");

    public Task<DesignConversionModelServerHttpResponse> ExecuteIdempotentMutationOnceAsync(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string bodyJson,
        Func<CancellationToken, Task<DesignConversionModelServerHttpResponse>> execute,
        Func<DesignConversionModelServerRouteAdmissionDecision, DesignConversionModelServerHttpResponse> buildConflict,
        Func<string, DesignConversionModelServerHttpResponse> buildInvalidJson,
        CancellationToken cancellationToken)
        => ExecuteIdempotentRequestOnceAsync(
            admission,
            bodyJson,
            execute,
            buildConflict,
            buildInvalidJson,
            cancellationToken,
            operationName: "idempotent mutation");

    private async Task<DesignConversionModelServerHttpResponse> ExecuteIdempotentRequestOnceAsync(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string bodyJson,
        Func<CancellationToken, Task<DesignConversionModelServerHttpResponse>> execute,
        Func<DesignConversionModelServerRouteAdmissionDecision, DesignConversionModelServerHttpResponse> buildConflict,
        Func<string, DesignConversionModelServerHttpResponse> buildInvalidJson,
        CancellationToken cancellationToken,
        string operationName)
    {
        if (!Enabled)
            return await ExecuteDisabledWithWorkloadGateAsync(
                admission,
                execute,
                cancellationToken,
                operationName);

        var digest = TryComputeCanonicalDigest(bodyJson);
        if (!digest.IsSuccess)
            return buildInvalidJson(BuildInvalidJsonMessage(digest.ErrorMessage, operationName));

        WorkloadLease? workloadLease = null;
        try
        {
            var quota = TryConsumeRequestQuota();
            if (!quota.IsAccepted)
                return BuildRequestQuotaRejectedResponse(admission, operationName, quota);

            var scope = string.IsNullOrWhiteSpace(admission.Scope) ? _routeScope : admission.Scope;
            var idempotencyKey = admission.IdempotencyKey ?? string.Empty;
            var cacheKey = scope + "\n" + idempotencyKey;
            if (_durableReplayStore is not null)
            {
                var completed = _durableReplayStore.TryReadCompleted(
                    scope,
                    idempotencyKey,
                    digest.Value!,
                    _utcNow(),
                    _idempotencyTtl,
                    _idempotencyCapacity);
                switch (completed.Kind)
                {
                    case DesignConversionModelServerRouteAdmissionDurableReplayKind.Replayed:
                        BindJobFromResponse(scope, completed.Response!);
                        return completed.Response!;
                    case DesignConversionModelServerRouteAdmissionDurableReplayKind.Conflict:
                        return buildConflict(admission);
                    case DesignConversionModelServerRouteAdmissionDurableReplayKind.FailedClosed:
                        return BuildDurableIdempotencyStoreFailureResponse(
                            admission,
                            completed.ErrorRef,
                            operationName);
                }
            }

            IdempotencyEntry entry;
            bool shouldStartExecution;
            while (true)
            {
                var now = _utcNow();
                lock (_lifecycleLock)
                {
                    PruneExpiredCompletedIdempotencyEntriesLocked(now);
                    PruneExpiredJobScopesLocked(now);

                    if (_idempotencyEntries.TryGetValue(cacheKey, out entry!))
                    {
                        if (entry.ResponseTask.IsFaulted || entry.ResponseTask.IsCanceled)
                        {
                            RemoveIdempotencyEntryLocked(cacheKey);
                            continue;
                        }

                        if (entry.IsExpired(now, _idempotencyTtl) &&
                            entry.ResponseTask.IsCompleted)
                        {
                            RemoveIdempotencyEntryLocked(cacheKey);
                            continue;
                        }

                        if (!CryptographicOperations.FixedTimeEquals(digest.Value!, entry.Digest))
                            return buildConflict(admission);

                        shouldStartExecution = false;
                        break;
                    }

                    if (!TryMakeRoomForNewIdempotencyEntryLocked(now))
                        return BuildIdempotencyCapacityRejectedResponse(admission, operationName);

                    workloadLease = TryAcquireWorkload();
                    if (workloadLease is null)
                        return BuildWorkloadCapacityRejectedResponse(admission, operationName);

                    entry = new IdempotencyEntry(
                        scope,
                        idempotencyKey,
                        digest.Value!.ToArray(),
                        now);
                    _idempotencyEntries[cacheKey] = entry;
                    shouldStartExecution = true;
                    break;
                }
            }

            if (shouldStartExecution)
            {
                var leaseForExecution = workloadLease!;
                workloadLease = null;
                _ = CompleteServerOwnedExecutionAsync(
                    cacheKey,
                    entry,
                    admission,
                    scope,
                    execute,
                    buildConflict,
                    operationName,
                    leaseForExecution);
            }

            try
            {
                var response = await entry.ResponseTask.WaitAsync(cancellationToken);
                return response;
            }
            catch
            {
                if (entry.ResponseTask.IsFaulted || entry.ResponseTask.IsCanceled)
                    RemoveFailedIdempotencyEntry(cacheKey, entry);

                throw;
            }
        }
        finally
        {
            workloadLease?.Dispose();
            CryptographicOperations.ZeroMemory(digest.Value!);
        }
    }

    private async Task<DesignConversionModelServerHttpResponse> ExecuteDisabledWithWorkloadGateAsync(
        DesignConversionModelServerRouteAdmissionDecision admission,
        Func<CancellationToken, Task<DesignConversionModelServerHttpResponse>> execute,
        CancellationToken cancellationToken,
        string operationName)
    {
        var quota = TryConsumeRequestQuota();
        if (!quota.IsAccepted)
            return BuildRequestQuotaRejectedResponse(admission, operationName, quota);

        var workloadLease = TryAcquireWorkload();
        if (workloadLease is null)
            return BuildWorkloadCapacityRejectedResponse(admission, operationName);

        try
        {
            return await execute(cancellationToken);
        }
        catch (OperationCanceledException ex) when (IsSuppliedCancellation(ex, cancellationToken))
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return BuildHandlerFailedClosedResponse(
                admission,
                operationName,
                HandlerExceptionStatusCode,
                DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode,
                HandlerExceptionFailedClosedErrorRef,
                DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorMessage);
        }
        finally
        {
            workloadLease.Dispose();
        }
    }

    private async Task CompleteServerOwnedExecutionAsync(
        string cacheKey,
        IdempotencyEntry entry,
        DesignConversionModelServerRouteAdmissionDecision admission,
        string scope,
        Func<CancellationToken, Task<DesignConversionModelServerHttpResponse>> execute,
        Func<DesignConversionModelServerRouteAdmissionDecision, DesignConversionModelServerHttpResponse> buildConflict,
        string operationName,
        WorkloadLease workloadLease)
    {
        DesignConversionModelServerRouteAdmissionDurableReplayLease? durableLease = null;
        DesignConversionModelServerHttpResponse? durableFailureResponse = null;
        DesignConversionModelServerHttpResponse? handlerFailureResponse = null;
        try
        {
            if (_durableReplayStore is not null)
            {
                var reservation = await _durableReplayStore.TryReserveForExecutionAsync(
                    scope,
                    entry.Key,
                    entry.Digest,
                    _utcNow,
                    _idempotencyTtl,
                    _idempotencyCapacity,
                    CancellationToken.None).ConfigureAwait(false);
                switch (reservation.Kind)
                {
                    case DesignConversionModelServerRouteAdmissionDurableReplayKind.Replayed:
                        BindJobFromResponse(scope, reservation.Response!);
                        entry.Completion.TrySetResult(reservation.Response!);
                        return;
                    case DesignConversionModelServerRouteAdmissionDurableReplayKind.Conflict:
                        entry.Completion.TrySetResult(buildConflict(admission));
                        return;
                    case DesignConversionModelServerRouteAdmissionDurableReplayKind.FailedClosed:
                        durableFailureResponse = BuildDurableIdempotencyStoreFailureResponse(
                            admission,
                            reservation.ErrorRef,
                            operationName);
                        break;
                    case DesignConversionModelServerRouteAdmissionDurableReplayKind.Reserved:
                        durableLease = reservation.Lease;
                        break;
                }
            }

            if (durableFailureResponse is null)
            {
                var response = await execute(CancellationToken.None).ConfigureAwait(false);
                if (_durableReplayStore is not null)
                {
                    var persisted = _durableReplayStore.TryPersistCompleted(
                        durableLease,
                        scope,
                        entry.Key,
                        entry.Digest,
                        response,
                        _utcNow(),
                        _idempotencyTtl,
                        _idempotencyCapacity);
                    if (!persisted.IsSuccess)
                    {
                        durableFailureResponse = BuildDurableIdempotencyStoreFailureResponse(
                            admission,
                            persisted.ErrorRef,
                            operationName);
                    }
                }

                if (durableFailureResponse is null)
                {
                    BindJobFromResponse(scope, response);
                    entry.Completion.TrySetResult(response);
                }
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            handlerFailureResponse = BuildHandlerFailedClosedResponse(
                admission,
                operationName,
                HandlerExceptionStatusCode,
                DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode,
                HandlerExceptionFailedClosedErrorRef,
                DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorMessage);
        }
        catch (Exception exception)
        {
            RemoveFailedIdempotencyEntry(cacheKey, entry);
            entry.Completion.TrySetException(exception);
        }
        finally
        {
            try
            {
                durableLease?.Dispose();
            }
            finally
            {
                workloadLease.Dispose();
            }
        }

        if (durableFailureResponse is not null)
            CompleteWithDurableFailure(cacheKey, entry, durableFailureResponse);

        if (handlerFailureResponse is not null)
            CompleteWithHandlerFailure(cacheKey, entry, handlerFailureResponse);
    }

    private static bool IsSuppliedCancellation(
        OperationCanceledException exception,
        CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested &&
           (!exception.CancellationToken.CanBeCanceled ||
            exception.CancellationToken == cancellationToken);

    private void CompleteWithDurableFailure(
        string cacheKey,
        IdempotencyEntry entry,
        DesignConversionModelServerHttpResponse response)
    {
        RemoveFailedIdempotencyEntry(cacheKey, entry);
        entry.Completion.TrySetResult(response);
    }

    private void CompleteWithHandlerFailure(
        string cacheKey,
        IdempotencyEntry entry,
        DesignConversionModelServerHttpResponse response)
    {
        RemoveFailedIdempotencyEntry(cacheKey, entry);
        entry.Completion.TrySetResult(response);
    }

    private DesignConversionModelServerHttpResponse BuildHandlerFailedClosedResponse(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string operationName,
        int statusCode,
        string errorCode,
        string errorRef,
        string errorMessage)
    {
        var failureKind = statusCode == HandlerCancellationStatusCode
            ? "caller_independent_cancellation"
            : "exception";
        var metadata = new Dictionary<string, string>(admission.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["route_admission_path"] = "ref_only",
            ["route_admission_raw_path_exposed"] = bool.FalseString,
            ["route_admission_error_ref"] = errorRef,
            ["route_admission_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["handler_execution_failed_closed"] = bool.TrueString,
            ["handler_failure_kind"] = failureKind,
            ["handler_operation"] = string.IsNullOrWhiteSpace(operationName) ? "unknown" : operationName,
            ["handler_failure_cleanup_completed"] = bool.TrueString,
            ["handler_failure_response_published_after_cleanup"] = bool.TrueString,
            ["handler_exception_message_exposed"] = bool.FalseString,
            ["handler_exception_type_exposed"] = bool.FalseString,
            ["handler_failure_details_exposed"] = bool.FalseString,
            ["handler_failure_request_payload_exposed"] = bool.FalseString,
            ["raw_body_included"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["runtime_invoked"] = bool.FalseString,
            ["persistence_invoked"] = bool.FalseString,
            ["browser_runtime_invoked"] = bool.FalseString,
            ["model_provider_invoked"] = bool.FalseString,
            ["training_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        var bodyJson = JsonSerializer.Serialize(new
        {
            errorCode,
            errorRef,
            statusCode,
            errorMessage,
            boundaryMetadata = metadata
        });

        return new DesignConversionModelServerHttpResponse(
            statusCode,
            "application/json; charset=utf-8",
            bodyJson,
            new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private DesignConversionModelServerHttpResponse BuildDurableIdempotencyStoreFailureResponse(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string? errorRef,
        string operationName)
    {
        var metadata = new Dictionary<string, string>(admission.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["route_admission_authorized"] = bool.FalseString,
            ["route_admission_path"] = "ref_only",
            ["route_admission_raw_path_exposed"] = bool.FalseString,
            ["route_admission_error_ref"] = string.IsNullOrWhiteSpace(errorRef)
                ? "durable_idempotency_store_failed_closed"
                : errorRef,
            ["route_admission_status_code"] = "503",
            ["idempotency_durable_replay_result"] = "failed_closed",
            ["raw_body_included"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["idempotency_payload_digest_exposed"] = bool.FalseString,
            ["idempotency_key_exposed"] = bool.FalseString,
            ["token_value_exposed"] = bool.FalseString,
            ["token_hash_exposed"] = bool.FalseString,
            ["token_fingerprint_exposed"] = bool.FalseString,
            ["runtime_invoked"] = bool.FalseString,
            ["persistence_invoked"] = bool.FalseString,
            ["browser_runtime_invoked"] = bool.FalseString,
            ["model_provider_invoked"] = bool.FalseString,
            ["training_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        var bodyJson = JsonSerializer.Serialize(new
        {
            errorCode = "DESIGN_CONVERSION_IDEMPOTENCY_DURABLE_REPLAY_FAILED_CLOSED",
            errorRef = metadata["route_admission_error_ref"],
            errorMessage = $"DesignConversion {operationName} durable idempotency replay could not be verified safely; the request was not executed.",
            boundaryMetadata = metadata
        });

        return new DesignConversionModelServerHttpResponse(
            503,
            "application/json; charset=utf-8",
            bodyJson,
            new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private DesignConversionModelServerHttpResponse BuildIdempotencyCapacityRejectedResponse(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string operationName)
    {
        var metadata = new Dictionary<string, string>(admission.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["route_admission_authorized"] = bool.FalseString,
            ["route_admission_error_ref"] = "idempotency_cache_capacity_exhausted",
            ["route_admission_status_code"] = "429",
            ["runtime_invoked"] = bool.FalseString,
            ["persistence_invoked"] = bool.FalseString,
            ["browser_runtime_invoked"] = bool.FalseString,
            ["model_provider_invoked"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };
        ApplyRejectedResponsePrivacyMetadata(metadata);

        var bodyJson = JsonSerializer.Serialize(new
        {
            errorCode = "DESIGN_CONVERSION_IDEMPOTENCY_CAPACITY_EXHAUSTED",
            errorRef = "idempotency_cache_capacity_exhausted",
            errorMessage = $"DesignConversion {operationName} idempotency admission is temporarily at capacity.",
            boundaryMetadata = metadata
        });

        return new DesignConversionModelServerHttpResponse(
            429,
            "application/json; charset=utf-8",
            bodyJson,
            new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private DesignConversionModelServerHttpResponse BuildWorkloadCapacityRejectedResponse(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string operationName)
    {
        var retryAfterSeconds = WorkloadRetryAfterSeconds.ToString(CultureInfo.InvariantCulture);
        var metadata = new Dictionary<string, string>(admission.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["workload_admitted"] = bool.FalseString,
            ["workload_admission_rejection_reason"] = "capacity_exhausted",
            ["workload_admission_status_code"] = "429",
            ["workload_limit"] = _maxConcurrentWorkloads.ToString(CultureInfo.InvariantCulture),
            ["workload_active_count"] = ActiveWorkloadCount().ToString(CultureInfo.InvariantCulture),
            ["workload_available_count"] = AvailableWorkloadCount().ToString(CultureInfo.InvariantCulture),
            ["workload_retry_after_seconds"] = retryAfterSeconds,
            ["runtime_invoked"] = bool.FalseString,
            ["persistence_invoked"] = bool.FalseString,
            ["browser_runtime_invoked"] = bool.FalseString,
            ["model_provider_invoked"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };
        ApplyRejectedResponsePrivacyMetadata(metadata);

        var bodyJson = JsonSerializer.Serialize(new
        {
            errorCode = "DESIGN_CONVERSION_WORKLOAD_CAPACITY_EXHAUSTED",
            errorRef = "workload_capacity_exhausted",
            errorMessage = $"DesignConversion {operationName} workload admission is temporarily at capacity.",
            retryAfterSeconds = WorkloadRetryAfterSeconds,
            boundaryMetadata = metadata
        });

        return new DesignConversionModelServerHttpResponse(
            429,
            "application/json; charset=utf-8",
            bodyJson,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Retry-After"] = retryAfterSeconds
            });
    }

    private DesignConversionModelServerHttpResponse BuildRequestQuotaRejectedResponse(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string operationName,
        RequestQuotaDecision quota)
    {
        var retryAfterSeconds = quota.RetryAfterSeconds.ToString(CultureInfo.InvariantCulture);
        var metadata = new Dictionary<string, string>(admission.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["route_admission_method"] = "ref_only",
            ["route_admission_path"] = "ref_only",
            ["request_quota_admitted"] = bool.FalseString,
            ["request_quota_admission_rejection_reason"] = "quota_exhausted",
            ["request_quota_admission_status_code"] = "429",
            ["request_quota_limit"] = quota.Limit.ToString(CultureInfo.InvariantCulture),
            ["request_quota_window_seconds"] = quota.WindowSeconds.ToString(CultureInfo.InvariantCulture),
            ["request_quota_remaining"] = quota.Remaining.ToString(CultureInfo.InvariantCulture),
            ["request_quota_retry_after_seconds"] = retryAfterSeconds,
            ["request_quota_partition_authority"] = "server_route_admission_instance_tenant_binding",
            ["request_quota_partition_ref"] = _tenantScopeRef,
            ["request_quota_refs_only"] = bool.TrueString,
            ["request_quota_raw_partition_exposed"] = bool.FalseString,
            ["idempotency_payload_digest_exposed"] = bool.FalseString,
            ["idempotency_key_exposed"] = bool.FalseString,
            ["runtime_invoked"] = bool.FalseString,
            ["persistence_invoked"] = bool.FalseString,
            ["browser_runtime_invoked"] = bool.FalseString,
            ["model_provider_invoked"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };
        ApplyRejectedResponsePrivacyMetadata(metadata);

        var bodyJson = JsonSerializer.Serialize(new
        {
            errorCode = "DESIGN_CONVERSION_REQUEST_QUOTA_EXHAUSTED",
            errorRef = "request_quota_exhausted",
            errorMessage = $"DesignConversion {operationName} request quota is temporarily exhausted.",
            retryAfterSeconds = quota.RetryAfterSeconds,
            boundaryMetadata = metadata
        });

        return new DesignConversionModelServerHttpResponse(
            429,
            "application/json; charset=utf-8",
            bodyJson,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Retry-After"] = retryAfterSeconds
            });
    }

    private static void ApplyRejectedResponsePrivacyMetadata(IDictionary<string, string> metadata)
    {
        metadata["route_admission_method"] = "ref_only";
        metadata["route_admission_path"] = "ref_only";
        metadata["route_admission_raw_path_exposed"] = bool.FalseString;
        metadata["raw_body_included"] = bool.FalseString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["raw_user_private_data_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["idempotency_payload_digest_exposed"] = bool.FalseString;
        metadata["idempotency_key_exposed"] = bool.FalseString;
        metadata["token_value_exposed"] = bool.FalseString;
        metadata["token_hash_exposed"] = bool.FalseString;
        metadata["token_fingerprint_exposed"] = bool.FalseString;
        metadata["runtime_invoked"] = bool.FalseString;
        metadata["persistence_invoked"] = bool.FalseString;
        metadata["browser_runtime_invoked"] = bool.FalseString;
        metadata["model_provider_invoked"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        metadata["training_ready"] = bool.FalseString;
    }

    private void RemoveFailedIdempotencyEntry(string cacheKey, IdempotencyEntry entry)
    {
        lock (_lifecycleLock)
        {
            if (_idempotencyEntries.TryGetValue(cacheKey, out var current) &&
                ReferenceEquals(current, entry))
            {
                RemoveIdempotencyEntryLocked(cacheKey);
            }
        }
    }

    public bool IsStatusJobAllowed(
        DesignConversionModelServerRouteAdmissionDecision admission,
        string? jobId)
    {
        if (!Enabled)
            return true;
        if (!admission.IsAccepted || string.IsNullOrWhiteSpace(jobId))
            return false;

        var trimmedJobId = jobId.Trim();
        var now = _utcNow();
        lock (_lifecycleLock)
        {
            PruneExpiredJobScopesLocked(now);
            return _jobScopes.TryGetValue(trimmedJobId, out var entry) &&
                   !entry.IsExpired(now, _idempotencyTtl) &&
                   string.Equals(entry.Scope, admission.Scope, StringComparison.Ordinal);
        }
    }

    public IReadOnlyDictionary<string, object?> PublicMetadata()
        => new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["routeAdmissionEnabled"] = Enabled,
            ["routeAdmissionConfigurationStatus"] = ConfigurationStatus,
            ["routeAdmissionHeaderName"] = AuthorizationHeaderName,
            ["routeAdmissionProjectHeaderName"] = ProjectAuthorizationHeaderName,
            ["routeAdmissionAcceptedKeyCount"] = AcceptedKeyCount,
            ["routeAdmissionPreviousTokenConfigured"] = PreviousTokenConfigured,
            ["routeAdmissionPreviousTokenActive"] = PreviousTokenActive,
            ["routeAdmissionRotationState"] = RotationState,
            ["routeAdmissionScope"] = _routeScope,
            ["routeAdmissionScopeRef"] = _tenantScopeRef,
            ["routeAdmissionTenantBound"] = TenantBound,
            ["routeAdmissionTenantIdRef"] = TenantBound ? _tenantIdRef : string.Empty,
            ["routeAdmissionRawTenantExposed"] = false,
            ["idempotencyHeaderName"] = IdempotencyHeaderName,
            ["idempotencyRequired"] = Enabled && Ready,
            ["idempotencyMaxKeyLength"] = MaximumIdempotencyKeyLength,
            ["idempotencyCapacity"] = _idempotencyCapacity,
            ["idempotencyTtlSeconds"] = (long)_idempotencyTtl.TotalSeconds,
            ["idempotencyFailureReplayPolicy"] = IdempotencyFailureReplayPolicy,
            ["idempotencyDurableReplayEnabled"] = _durableReplayStore is not null,
            ["idempotencyDurableReplayStoreState"] = _durableReplayStore?.StoreState ?? "memory_only",
            ["routeAdmissionMaxConcurrentWorkloads"] = _maxConcurrentWorkloads,
            ["routeAdmissionRequestQuotaActive"] = true,
            ["routeAdmissionRequestQuotaLimit"] = _requestQuotaLimit,
            ["routeAdmissionRequestQuotaWindowSeconds"] = (int)_requestQuotaWindow.TotalSeconds,
            ["routeAdmissionRequestQuotaPartitionAuthority"] = "server_route_admission_instance_tenant_binding",
            ["routeAdmissionRequestQuotaPartitionRef"] = _tenantScopeRef,
            ["idempotencyDurableReplayCompletedRecordCount"] =
                _durableReplayStore?.CountCompletedRecordsBounded(_idempotencyCapacity) ?? 0
        };

    private DesignConversionModelServerRouteAdmissionDecision Admit(
        DesignConversionModelServerHttpRequest request,
        bool requireIdempotencyKey,
        bool routeScopedIdempotency = false)
    {
        var method = string.IsNullOrWhiteSpace(request.Method) ? "UNKNOWN" : request.Method.Trim().ToUpperInvariant();
        var path = string.IsNullOrWhiteSpace(request.Path) ? "/" : request.Path.Trim();
        if (!Enabled)
        {
            return Accepted(
                method,
                path,
                scope: "disabled",
                idempotencyKey: null,
                requireIdempotencyKey);
        }

        if (!Ready)
        {
            return Rejected(
                method,
                path,
                503,
                _notReadyErrorCode ?? "DESIGN_CONVERSION_ROUTE_ADMISSION_NOT_READY",
                "DesignConversion live route admission is enabled but its token key ring is not ready.",
                tokenSupplied: false,
                tokenAccepted: false,
                idempotencyKeyAccepted: false,
                requireIdempotencyKey,
                _notReadyReason ?? "route_admission_not_ready");
        }

        var token = ReadSuppliedToken(request.Headers);
        if (!token.IsSuccess)
        {
            return Rejected(
                method,
                path,
                token.StatusCode,
                token.ErrorCode,
                token.ErrorMessage,
                token.TokenSupplied,
                tokenAccepted: false,
                idempotencyKeyAccepted: false,
                requireIdempotencyKey,
                token.Reason);
        }

        if (!IsAcceptedToken(token.Token))
        {
            return Rejected(
                method,
                path,
                403,
                "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_INVALID",
                "DesignConversion live route admission rejected the supplied token.",
                tokenSupplied: true,
                tokenAccepted: false,
                idempotencyKeyAccepted: false,
                requireIdempotencyKey,
                "token_invalid_or_retired");
        }

        string? idempotencyKey = null;
        if (requireIdempotencyKey)
        {
            var key = ReadIdempotencyKey(request.Headers);
            if (!key.IsSuccess)
            {
                return Rejected(
                    method,
                    path,
                    400,
                    key.ErrorCode,
                    key.ErrorMessage,
                    tokenSupplied: true,
                    tokenAccepted: true,
                    idempotencyKeyAccepted: false,
                    requireIdempotencyKey,
                    key.Reason);
            }

            idempotencyKey = key.Key;
        }

        var routeScope = routeScopedIdempotency
            ? BuildRouteScopedIdempotencyScope(method, NormalizeRouteScopePath(request.Path))
            : null;
        return Accepted(
            method,
            path,
            routeScope?.EffectiveScope ?? _tenantBoundScope,
            idempotencyKey,
            requireIdempotencyKey,
            routeScope);
    }

    private DesignConversionModelServerRouteAdmissionDecision Accepted(
        string method,
        string path,
        string scope,
        string? idempotencyKey,
        bool requireIdempotencyKey,
        RouteScopedIdempotencyScope? routeScopedIdempotency = null)
        => new(
            IsAccepted: true,
            Scope: scope,
            IdempotencyKey: idempotencyKey,
            StatusCode: 200,
            ErrorCode: string.Empty,
            ErrorMessage: string.Empty,
            AdmissionContext: BuildAdmissionContext(scope),
            BoundaryMetadata: BuildMetadata(
                method,
                path,
                statusCode: 200,
                authorized: true,
                tokenSupplied: Enabled,
                tokenAccepted: Enabled && Ready,
                idempotencyKeyAccepted: !requireIdempotencyKey || !Enabled || !Ready || !string.IsNullOrWhiteSpace(idempotencyKey),
                requireIdempotencyKey,
                errorRef: "none",
                routeScopedIdempotency));

    private DesignConversionModelServerRouteAdmissionDecision Rejected(
        string method,
        string path,
        int statusCode,
        string errorCode,
        string errorMessage,
        bool tokenSupplied,
        bool tokenAccepted,
        bool idempotencyKeyAccepted,
        bool requireIdempotencyKey,
        string reason)
        => new(
            IsAccepted: false,
            Scope: _routeScope,
            IdempotencyKey: null,
            StatusCode: statusCode,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            AdmissionContext: BuildAdmissionContext(_tenantBoundScope),
            BoundaryMetadata: BuildMetadata(
                method,
                path,
                statusCode,
                authorized: false,
                tokenSupplied,
                tokenAccepted,
                idempotencyKeyAccepted,
                requireIdempotencyKey,
                reason));

    private IReadOnlyDictionary<string, string> BuildMetadata(
        string method,
        string path,
        int statusCode,
        bool authorized,
        bool tokenSupplied,
        bool tokenAccepted,
        bool idempotencyKeyAccepted,
        bool requireIdempotencyKey,
        string errorRef,
        RouteScopedIdempotencyScope? routeScopedIdempotency = null)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["route_admission_boundary"] = "design_conversion_live_route_admission_current_previous_token_and_idempotency",
            ["route_admission_enabled"] = Enabled.ToString(),
            ["route_admission_configuration_status"] = ConfigurationStatus,
            ["route_admission_authorized"] = authorized.ToString(),
            ["route_admission_scope"] = _routeScope,
            ["route_admission_scope_ref"] = routeScopedIdempotency?.EffectiveScopeRef ?? _tenantScopeRef,
            ["route_admission_tenant_bound"] = TenantBound.ToString(),
            ["route_admission_tenant_id_ref"] = TenantBound ? _tenantIdRef : string.Empty,
            ["route_admission_raw_tenant_id_exposed"] = bool.FalseString,
            ["route_admission_header_name"] = AuthorizationHeaderName,
            ["route_admission_project_header_name"] = ProjectAuthorizationHeaderName,
            ["route_admission_token_supplied"] = tokenSupplied.ToString(),
            ["route_admission_token_accepted"] = tokenAccepted.ToString(),
            ["route_admission_previous_token_configured"] = PreviousTokenConfigured.ToString(),
            ["route_admission_previous_token_active"] = PreviousTokenActive.ToString(),
            ["route_admission_accepted_key_count"] = AcceptedKeyCount.ToString(CultureInfo.InvariantCulture),
            ["route_admission_rotation_state"] = RotationState,
            ["route_admission_error_ref"] = errorRef,
            ["route_admission_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["route_admission_method"] = routeScopedIdempotency is null
                ? string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method
                : "ref_only",
            ["route_admission_path"] = routeScopedIdempotency is null
                ? string.IsNullOrWhiteSpace(path) ? "/" : path
                : "ref_only",
            ["idempotency_header_name"] = IdempotencyHeaderName,
            ["idempotency_required"] = (Enabled && Ready && requireIdempotencyKey).ToString(),
            ["idempotency_key_accepted"] = idempotencyKeyAccepted.ToString(),
            ["idempotency_cache_capacity"] = _idempotencyCapacity.ToString(CultureInfo.InvariantCulture),
            ["idempotency_cache_ttl_seconds"] = ((long)_idempotencyTtl.TotalSeconds).ToString(CultureInfo.InvariantCulture),
            ["idempotency_failure_replay_policy"] = IdempotencyFailureReplayPolicy,
            ["idempotency_durable_replay_enabled"] = (_durableReplayStore is not null).ToString(),
            ["idempotency_durable_replay_store_state"] = _durableReplayStore?.StoreState ?? "memory_only",
            ["workload_limit"] = _maxConcurrentWorkloads.ToString(CultureInfo.InvariantCulture),
            ["workload_active_count"] = ActiveWorkloadCount().ToString(CultureInfo.InvariantCulture),
            ["workload_available_count"] = AvailableWorkloadCount().ToString(CultureInfo.InvariantCulture),
            ["request_quota_active"] = bool.TrueString,
            ["request_quota_limit"] = _requestQuotaLimit.ToString(CultureInfo.InvariantCulture),
            ["request_quota_window_seconds"] = ((int)_requestQuotaWindow.TotalSeconds).ToString(CultureInfo.InvariantCulture),
            ["request_quota_partition_authority"] = "server_route_admission_instance_tenant_binding",
            ["request_quota_partition_ref"] = _tenantScopeRef,
            ["request_quota_raw_partition_exposed"] = bool.FalseString,
            ["idempotency_payload_digest_exposed"] = bool.FalseString,
            ["idempotency_key_exposed"] = bool.FalseString,
            ["token_value_exposed"] = bool.FalseString,
            ["token_hash_exposed"] = bool.FalseString,
            ["token_fingerprint_exposed"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["runtime_invoked"] = bool.FalseString,
            ["persistence_invoked"] = bool.FalseString,
            ["browser_runtime_invoked"] = bool.FalseString,
            ["model_provider_invoked"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        if (routeScopedIdempotency is not null)
        {
            metadata["idempotency_route_scoped"] = bool.TrueString;
            metadata["idempotency_effective_scope_ref"] = routeScopedIdempotency.EffectiveScopeRef;
            metadata["idempotency_route_scope_ref"] = routeScopedIdempotency.RouteScopeRef;
            metadata["idempotency_route_method_ref"] = routeScopedIdempotency.MethodRef;
            metadata["idempotency_route_path_ref"] = routeScopedIdempotency.PathRef;
            metadata["idempotency_route_method_path_exposed"] = bool.FalseString;
            metadata["idempotency_effective_scope_contains_raw_route"] = bool.FalseString;
        }

        return metadata;
    }

    private WorkloadLease? TryAcquireWorkload()
    {
        if (!_workloadGate.Wait(0))
            return null;

        Interlocked.Increment(ref _activeWorkloadCount);
        return new WorkloadLease(this);
    }

    private void ReleaseWorkload()
    {
        Interlocked.Decrement(ref _activeWorkloadCount);
        _workloadGate.Release();
    }

    private int ActiveWorkloadCount()
        => Math.Clamp(Volatile.Read(ref _activeWorkloadCount), 0, _maxConcurrentWorkloads);

    private int AvailableWorkloadCount()
        => Math.Clamp(_workloadGate.CurrentCount, 0, _maxConcurrentWorkloads);

    private static int NormalizeMaxConcurrentWorkloads(int value)
        => Math.Clamp(value, 1, MaximumConcurrentWorkloads);

    private static int NormalizeRequestQuotaLimit(int value)
        => Math.Clamp(value, MinimumRequestQuotaLimit, MaximumRequestQuotaLimit);

    private static TimeSpan NormalizeRequestQuotaWindow(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
            return MinimumRequestQuotaWindow;

        if (value < MinimumRequestQuotaWindow)
            return MinimumRequestQuotaWindow;

        if (value > MaximumRequestQuotaWindow)
            return MaximumRequestQuotaWindow;

        return TimeSpan.FromSeconds(Math.Ceiling(value.TotalSeconds));
    }

    private RequestQuotaDecision TryConsumeRequestQuota()
    {
        var now = _utcNow();
        lock (_lifecycleLock)
        {
            EnsureRequestQuotaWindowLocked(now);
            if (_requestQuotaCount >= _requestQuotaLimit)
            {
                return RequestQuotaDecision.Rejected(
                    _requestQuotaLimit,
                    RequestQuotaWindowSeconds(),
                    remaining: 0,
                    retryAfterSeconds: RequestQuotaRetryAfterSecondsLocked(now));
            }

            _requestQuotaCount++;
            return RequestQuotaDecision.Accepted(
                _requestQuotaLimit,
                RequestQuotaWindowSeconds(),
                Math.Max(0, _requestQuotaLimit - _requestQuotaCount));
        }
    }

    private void EnsureRequestQuotaWindowLocked(DateTimeOffset now)
    {
        if (_requestQuotaWindowStartedAtUtc == default ||
            now < _requestQuotaWindowStartedAtUtc ||
            now - _requestQuotaWindowStartedAtUtc >= _requestQuotaWindow)
        {
            _requestQuotaWindowStartedAtUtc = now;
            _requestQuotaCount = 0;
        }
    }

    private int RequestQuotaRetryAfterSecondsLocked(DateTimeOffset now)
    {
        var elapsed = now - _requestQuotaWindowStartedAtUtc;
        var retryAfter = _requestQuotaWindow - elapsed;
        if (retryAfter <= TimeSpan.Zero)
            return 1;

        return Math.Clamp(
            (int)Math.Ceiling(retryAfter.TotalSeconds),
            1,
            MaximumRequestQuotaWindowSeconds);
    }

    private int RequestQuotaWindowSeconds()
        => (int)_requestQuotaWindow.TotalSeconds;

    private bool IsAcceptedToken(string? suppliedToken)
    {
        if (string.IsNullOrWhiteSpace(suppliedToken) || _currentTokenHash is null)
            return false;

        var normalized = suppliedToken.Trim();
        if (normalized.Length > MaximumTokenLength)
            return false;

        var suppliedHash = HashToken(normalized);
        try
        {
            var currentMatched = CryptographicOperations.FixedTimeEquals(suppliedHash, _currentTokenHash);
            var previousMatched = _previousTokenHash is not null &&
                                  CryptographicOperations.FixedTimeEquals(suppliedHash, _previousTokenHash);
            var previousWithinGrace = _previousTokenHash is not null &&
                                      _previousTokenExpiresAtUtc is not null &&
                                      _utcNow() < _previousTokenExpiresAtUtc.Value;

            return currentMatched | (previousMatched & previousWithinGrace);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(suppliedHash);
        }
    }

    private SuppliedToken ReadSuppliedToken(IReadOnlyDictionary<string, string>? headers)
    {
        var authorization = ReadHeader(headers, AuthorizationHeaderName);
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            var trimmed = authorization.Trim();
            if (!trimmed.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return SuppliedToken.Rejected(
                    tokenSupplied: true,
                    statusCode: 401,
                    errorCode: "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_MALFORMED",
                    errorMessage: "DesignConversion live route admission requires Authorization: Bearer.",
                    reason: "authorization_header_malformed");
            }

            var token = trimmed["Bearer ".Length..].Trim();
            if (!IsPlausibleSuppliedToken(token))
            {
                return SuppliedToken.Rejected(
                    tokenSupplied: true,
                    statusCode: 401,
                    errorCode: "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_MALFORMED",
                    errorMessage: "DesignConversion live route admission received a malformed bearer token.",
                    reason: "bearer_token_malformed");
            }

            return SuppliedToken.Accepted(token);
        }

        var projectHeader = ReadHeader(headers, ProjectAuthorizationHeaderName);
        if (!string.IsNullOrWhiteSpace(projectHeader))
        {
            var token = projectHeader.Trim();
            if (!IsPlausibleSuppliedToken(token))
            {
                return SuppliedToken.Rejected(
                    tokenSupplied: true,
                    statusCode: 401,
                    errorCode: "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_MALFORMED",
                    errorMessage: "DesignConversion live route admission received a malformed project token.",
                    reason: "project_token_malformed");
            }

            return SuppliedToken.Accepted(token);
        }

        return SuppliedToken.Rejected(
            tokenSupplied: false,
            statusCode: 401,
            errorCode: "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED",
            errorMessage: "DesignConversion live route admission requires a bearer token.",
            reason: "token_missing");
    }

    private static SuppliedIdempotencyKey ReadIdempotencyKey(IReadOnlyDictionary<string, string>? headers)
    {
        var value = ReadHeader(headers, IdempotencyHeaderName);
        if (string.IsNullOrWhiteSpace(value))
        {
            return SuppliedIdempotencyKey.Rejected(
                "DESIGN_CONVERSION_IDEMPOTENCY_KEY_REQUIRED",
                "DesignConversion evaluate endpoint requires an Idempotency-Key header.",
                "idempotency_key_missing");
        }

        var key = value.Trim();
        if (key.Length > MaximumIdempotencyKeyLength)
        {
            return SuppliedIdempotencyKey.Rejected(
                "DESIGN_CONVERSION_IDEMPOTENCY_KEY_TOO_LONG",
                "DesignConversion evaluate idempotency key is too long.",
                "idempotency_key_too_long");
        }

        if (!key.All(IsAllowedIdempotencyKeyChar))
        {
            return SuppliedIdempotencyKey.Rejected(
                "DESIGN_CONVERSION_IDEMPOTENCY_KEY_INVALID",
                "DesignConversion evaluate idempotency key must be visible bounded ASCII.",
                "idempotency_key_invalid");
        }

        return SuppliedIdempotencyKey.Accepted(key);
    }

    private void BindJobFromResponse(string scope, DesignConversionModelServerHttpResponse response)
    {
        if (!Enabled || response.StatusCode is < 200 or >= 300)
            return;

        try
        {
            using var document = JsonDocument.Parse(response.BodyJson);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("jobId", out var jobIdElement) &&
                jobIdElement.ValueKind == JsonValueKind.String)
            {
                var jobId = jobIdElement.GetString();
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    var now = _utcNow();
                    lock (_lifecycleLock)
                    {
                        PruneExpiredJobScopesLocked(now);
                        EnsureJobScopeCapacityLocked();
                        _jobScopes[jobId.Trim()] = new JobScopeEntry(scope, now);
                    }
                }
            }
        }
        catch (JsonException)
        {
        }
    }

    private bool TryMakeRoomForNewIdempotencyEntryLocked(DateTimeOffset now)
    {
        PruneExpiredCompletedIdempotencyEntriesLocked(now);
        if (_idempotencyEntries.Count < _idempotencyCapacity)
            return true;

        foreach (var pair in _idempotencyEntries
                     .Where(pair => pair.Value.ResponseTask.IsCompleted)
                     .OrderBy(pair => pair.Value.CreatedAtUtc)
                     .ToArray())
        {
            RemoveIdempotencyEntryLocked(pair.Key);
            if (_idempotencyEntries.Count < _idempotencyCapacity)
                return true;
        }

        return _idempotencyEntries.Count < _idempotencyCapacity;
    }

    private void PruneExpiredCompletedIdempotencyEntriesLocked(DateTimeOffset now)
    {
        foreach (var pair in _idempotencyEntries.ToArray())
        {
            if ((pair.Value.IsExpired(now, _idempotencyTtl) &&
                 pair.Value.ResponseTask.IsCompleted) ||
                pair.Value.ResponseTask.IsFaulted ||
                pair.Value.ResponseTask.IsCanceled)
            {
                RemoveIdempotencyEntryLocked(pair.Key);
            }
        }
    }

    private void RemoveIdempotencyEntryLocked(string cacheKey)
    {
        if (_idempotencyEntries.TryRemove(cacheKey, out var removed))
            CryptographicOperations.ZeroMemory(removed.Digest);
    }

    private void PruneExpiredJobScopesLocked(DateTimeOffset now)
    {
        foreach (var pair in _jobScopes.ToArray())
        {
            if (pair.Value.IsExpired(now, _idempotencyTtl))
                _jobScopes.TryRemove(pair.Key, out _);
        }
    }

    private void EnsureJobScopeCapacityLocked()
    {
        var overflow = _jobScopes.Count - _idempotencyCapacity + 1;
        if (overflow <= 0)
            return;

        foreach (var pair in _jobScopes
                     .OrderBy(pair => pair.Value.CreatedAtUtc)
                     .Take(overflow)
                     .ToArray())
        {
            _jobScopes.TryRemove(pair.Key, out _);
        }
    }

    private static AgentInfrastructure.OperationResult<byte[]> TryComputeCanonicalDigest(string? bodyJson)
    {
        if (string.IsNullOrWhiteSpace(bodyJson))
        {
            return AgentInfrastructure.OperationResult<byte[]>.Fail(
                "DesignConversion evaluate endpoint requires a JSON request body.",
                "REQUEST_BODY_REQUIRED");
        }

        try
        {
            using var document = JsonDocument.Parse(bodyJson);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
            {
                WriteCanonical(document.RootElement, writer);
            }

            var canonicalBytes = stream.ToArray();
            try
            {
                return AgentInfrastructure.OperationResult<byte[]>.Ok(SHA256.HashData(canonicalBytes));
            }
            finally
            {
                CryptographicOperations.ZeroMemory(canonicalBytes);
            }
        }
        catch (JsonException)
        {
            return AgentInfrastructure.OperationResult<byte[]>.Fail(
                "DesignConversion evaluate endpoint received invalid JSON.",
                "REQUEST_JSON_INVALID");
        }
    }

    private static string BuildInvalidJsonMessage(string? errorMessage, string operationName)
    {
        if (string.Equals(operationName, "evaluate", StringComparison.Ordinal))
            return errorMessage ?? "DesignConversion evaluate endpoint received invalid JSON.";

        return errorMessage switch
        {
            "DesignConversion evaluate endpoint requires a JSON request body." =>
                $"DesignConversion {operationName} endpoint requires a JSON request body.",
            "DesignConversion evaluate endpoint received invalid JSON." =>
                $"DesignConversion {operationName} endpoint received invalid JSON.",
            _ => errorMessage ?? $"DesignConversion {operationName} endpoint received invalid JSON."
        };
    }

    private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject()
                             .Select((property, index) => new { property, index })
                             .OrderBy(item => item.property.Name, StringComparer.Ordinal)
                             .ThenBy(item => item.index))
                {
                    writer.WritePropertyName(property.property.Name);
                    WriteCanonical(property.property.Value, writer);
                }

                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteCanonical(item, writer);
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteRawValue(element.GetRawText(), skipInputValidation: true);
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                writer.WriteNullValue();
                break;
        }
    }

    private static AgentInfrastructure.OperationResult<byte[]> BuildTokenHash(
        string? token,
        string requiredErrorCode,
        string invalidErrorCode)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return AgentInfrastructure.OperationResult<byte[]>.Fail(
                "DesignConversion live route token is required.",
                requiredErrorCode);
        }

        var normalized = token.Trim();
        if (!IsPlausibleConfiguredToken(normalized))
        {
            return AgentInfrastructure.OperationResult<byte[]>.Fail(
                "DesignConversion live route token configuration is invalid.",
                invalidErrorCode);
        }

        return AgentInfrastructure.OperationResult<byte[]>.Ok(HashToken(normalized));
    }

    private static AgentInfrastructure.OperationResult<TenantBinding> BuildTenantBinding(string? tenantId)
    {
        var tenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(tenantId);
        if (!tenant.IsSuccess)
            return AgentInfrastructure.OperationResult<TenantBinding>.Fail(
                string.Equals(tenant.ErrorCode, "DESIGN_CONVERSION_TENANT_ID_REQUIRED", StringComparison.Ordinal)
                    ? "DesignConversion live route tenant id is required when route admission is enabled."
                    : "DesignConversion live route tenant id configuration is invalid.",
                string.Equals(tenant.ErrorCode, "DESIGN_CONVERSION_TENANT_ID_REQUIRED", StringComparison.Ordinal)
                    ? "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_REQUIRED"
                    : "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID");

        return AgentInfrastructure.OperationResult<TenantBinding>.Ok(new TenantBinding(
            tenant.Value!.TenantId,
            tenant.Value.TenantIdRef));
    }

    private static bool IsPlausibleConfiguredToken(string token)
        => token.Length is >= MinimumTokenLength and <= MaximumTokenLength &&
           !token.Any(char.IsControl);

    private static bool IsPlausibleSuppliedToken(string token)
        => token.Length is >= 1 and <= MaximumTokenLength &&
           !token.Any(char.IsControl);

    private static bool IsAllowedIdempotencyKeyChar(char ch)
        => ch is >= '!' and <= '~';

    private static string? ReadHeader(IReadOnlyDictionary<string, string>? headers, string name)
    {
        foreach (var pair in headers ?? new Dictionary<string, string>())
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }

        return null;
    }

    private RouteScopedIdempotencyScope BuildRouteScopedIdempotencyScope(
        string normalizedMethod,
        string normalizedRoutePath)
    {
        var method = string.IsNullOrWhiteSpace(normalizedMethod) ? "UNKNOWN" : normalizedMethod.Trim().ToUpperInvariant();
        var path = NormalizeRouteScopePath(normalizedRoutePath);
        var routeMaterial = method + "\n" + path;
        var routeScopeRef = ToHashRef(
            "design-conversion-route-admission-idempotency-route",
            routeMaterial);
        var effectiveScope = _tenantBoundScope + "\n" + routeScopeRef;
        return new RouteScopedIdempotencyScope(
            effectiveScope,
            ToHashRef("design-conversion-route-admission-scope", effectiveScope),
            routeScopeRef,
            ToHashRef("design-conversion-route-admission-idempotency-route-method", method),
            ToHashRef("design-conversion-route-admission-idempotency-route-path", path));
    }

    private static string NormalizeRouteScopePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }

    private static byte[] HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        try
        {
            return SHA256.HashData(bytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
        }
    }

    private DesignConversionModelServerRouteAdmissionContext BuildAdmissionContext(string scope)
        => new(
            TenantBound,
            TenantBound ? _tenantId : null,
            TenantBound ? _tenantIdRef : string.Empty,
            scope,
            TenantBound ? ToHashRef("design-conversion-route-admission-scope", scope) : string.Empty);

    private static string ToHashRef(string scope, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        try
        {
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            return $"sha256:{scope}:{hash}";
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
        }
    }

    private static DesignConversionModelServerRouteAdmission NotReady(
        DesignConversionModelServerRouteAdmissionOptions options,
        Func<DateTimeOffset>? utcNow,
        string? errorCode,
        string reason)
        => new(
            enabled: true,
            ready: false,
            currentTokenHash: null,
            previousTokenHash: null,
            previousTokenExpiresAtUtc: options.PreviousTokenExpiresAtUtc,
            routeScope: options.RouteScope ?? RouteScopeDefault,
            tenantId: null,
            tenantIdRef: null,
            idempotencyCapacity: options.IdempotencyCapacity,
            idempotencyTtl: options.IdempotencyTtl,
            maxConcurrentWorkloads: options.MaxConcurrentWorkloads,
            requestQuotaLimit: options.RequestQuotaLimit,
            requestQuotaWindow: options.RequestQuotaWindow,
            durableIdempotencyStoreRoot: options.DurableIdempotencyStoreRoot,
            utcNow: utcNow,
            notReadyErrorCode: string.IsNullOrWhiteSpace(errorCode)
                ? "DESIGN_CONVERSION_ROUTE_ADMISSION_NOT_READY"
                : errorCode,
            notReadyReason: reason);

    private static string StartupReason(string? errorCode)
        => errorCode switch
        {
            "DESIGN_CONVERSION_ROUTE_ADMISSION_CURRENT_TOKEN_REQUIRED" => "current_token_missing",
            "DESIGN_CONVERSION_ROUTE_ADMISSION_CURRENT_TOKEN_INVALID" => "current_token_invalid",
            "DESIGN_CONVERSION_ROUTE_ADMISSION_PREVIOUS_TOKEN_REQUIRED" => "previous_token_missing",
            "DESIGN_CONVERSION_ROUTE_ADMISSION_PREVIOUS_TOKEN_INVALID" => "previous_token_invalid",
            "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_REQUIRED" => "tenant_id_missing",
            "DESIGN_CONVERSION_ROUTE_ADMISSION_TENANT_ID_INVALID" => "tenant_id_invalid",
            _ => "route_admission_key_ring_invalid"
        };

    private sealed record TenantBinding(
        string TenantId,
        string TenantIdRef);

    private sealed record RouteScopedIdempotencyScope(
        string EffectiveScope,
        string EffectiveScopeRef,
        string RouteScopeRef,
        string MethodRef,
        string PathRef);

    private sealed record IdempotencyEntry(
        string Scope,
        string Key,
        byte[] Digest,
        DateTimeOffset CreatedAtUtc)
    {
        public TaskCompletionSource<DesignConversionModelServerHttpResponse> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<DesignConversionModelServerHttpResponse> ResponseTask => Completion.Task;

        public bool IsExpired(DateTimeOffset now, TimeSpan ttl)
            => now - CreatedAtUtc >= ttl;
    }

    private sealed record JobScopeEntry(
        string Scope,
        DateTimeOffset CreatedAtUtc)
    {
        public bool IsExpired(DateTimeOffset now, TimeSpan ttl)
            => now - CreatedAtUtc >= ttl;
    }

    private sealed record RequestQuotaDecision(
        bool IsAccepted,
        int Limit,
        int WindowSeconds,
        int Remaining,
        int RetryAfterSeconds)
    {
        public static RequestQuotaDecision Accepted(
            int limit,
            int windowSeconds,
            int remaining)
            => new(true, limit, windowSeconds, remaining, 0);

        public static RequestQuotaDecision Rejected(
            int limit,
            int windowSeconds,
            int remaining,
            int retryAfterSeconds)
            => new(false, limit, windowSeconds, remaining, retryAfterSeconds);
    }

    private sealed class WorkloadLease : IDisposable
    {
        private DesignConversionModelServerRouteAdmission? _owner;

        public WorkloadLease(DesignConversionModelServerRouteAdmission owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.ReleaseWorkload();
        }
    }

    private sealed record SuppliedToken(
        bool IsSuccess,
        string? Token,
        bool TokenSupplied,
        int StatusCode,
        string ErrorCode,
        string ErrorMessage,
        string Reason)
    {
        public static SuppliedToken Accepted(string token)
            => new(true, token, true, 200, string.Empty, string.Empty, "none");

        public static SuppliedToken Rejected(
            bool tokenSupplied,
            int statusCode,
            string errorCode,
            string errorMessage,
            string reason)
            => new(false, null, tokenSupplied, statusCode, errorCode, errorMessage, reason);
    }

    private sealed record SuppliedIdempotencyKey(
        bool IsSuccess,
        string? Key,
        string ErrorCode,
        string ErrorMessage,
        string Reason)
    {
        public static SuppliedIdempotencyKey Accepted(string key)
            => new(true, key, string.Empty, string.Empty, "none");

        public static SuppliedIdempotencyKey Rejected(
            string errorCode,
            string errorMessage,
            string reason)
            => new(false, null, errorCode, errorMessage, reason);
    }
}
