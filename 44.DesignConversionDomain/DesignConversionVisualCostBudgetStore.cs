// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Collections.Concurrent;
using System.Globalization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public interface IDesignConversionVisualCostBudgetStore
{
    Task<OperationResult<DesignConversionVisualCostReservationResult>> TryReserveAsync(
        DesignConversionVisualCostReservationRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionVisualCostSettlementResult>> SettleAsync(
        DesignConversionVisualCostSettlementRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionVisualCostReleaseResult>> ReleaseAsync(
        DesignConversionVisualCostReleaseRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionVisualCostBudgetPeriodState>> ReadPeriodAsync(
        DesignConversionVisualCostBudgetPeriodQuery query,
        CancellationToken cancellationToken = default);
}

public sealed record DesignConversionVisualCostReservationRequest(
    string ReservationRef,
    string UserRef,
    string SubmittedTier,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal MonthlyLimitUsd,
    decimal RequestedReserveUsd,
    DateTimeOffset RequestedAtUtc);

public sealed record DesignConversionVisualCostSettlementRequest(
    string ReservationRef,
    string UserRef,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal ActualCostUsd,
    DateTimeOffset SettledAtUtc);

public sealed record DesignConversionVisualCostReleaseRequest(
    string ReservationRef,
    string UserRef,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    DateTimeOffset ReleasedAtUtc);

public sealed record DesignConversionVisualCostBudgetPeriodQuery(
    string UserRef,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc);

public sealed record DesignConversionVisualCostReservationResult(
    string Status,
    bool Reserved,
    bool Replayed,
    string ReservationRef,
    string UserRef,
    string SubmittedTier,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal MonthlyLimitUsd,
    decimal RequestedReserveUsd,
    decimal FinalizedSpendUsd,
    decimal ActiveReservedUsd,
    decimal RemainingBudgetUsd,
    string DenialReasonCode,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionVisualCostSettlementResult(
    string Status,
    bool Settled,
    bool Replayed,
    string ReservationRef,
    string UserRef,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal ReservedUsd,
    decimal ActualCostUsd,
    decimal FinalizedSpendUsd,
    decimal ActiveReservedUsd,
    decimal RemainingBudgetUsd,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionVisualCostReleaseResult(
    string Status,
    bool Released,
    bool Replayed,
    string ReservationRef,
    string UserRef,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal ReleasedReserveUsd,
    decimal FinalizedSpendUsd,
    decimal ActiveReservedUsd,
    decimal RemainingBudgetUsd,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionVisualCostBudgetPeriodState(
    string UserRef,
    string PeriodKind,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal ObservedMonthlyLimitUsd,
    decimal FinalizedSpendUsd,
    decimal ActiveReservedUsd,
    decimal RemainingBudgetUsd,
    int ReservationCount,
    int ActiveReservationCount,
    int SettledReservationCount,
    int ReleasedReservationCount,
    int DeniedReservationCount,
    IReadOnlyList<DesignConversionVisualCostReservationSnapshot> Reservations);

public sealed record DesignConversionVisualCostReservationSnapshot(
    string ReservationRef,
    string Status,
    string UserRef,
    string SubmittedTier,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndExclusiveUtc,
    decimal MonthlyLimitUsd,
    decimal RequestedReserveUsd,
    decimal ActualCostUsd,
    DateTimeOffset ReservedAtUtc,
    DateTimeOffset? SettledAtUtc,
    DateTimeOffset? ReleasedAtUtc,
    string DenialReasonCode);

public sealed class JsonlDesignConversionVisualCostBudgetStore : IDesignConversionVisualCostBudgetStore
{
    public const string LedgerKind = "design_conversion_visual_cost_budget_store";
    public const string SchemaVersion = "design-conversion-visual-cost-budget-store-v1";
    public const string RecordsFileName = "visual-cost-budget-reservations.jsonl";
    public const string ReservationRefPrefix = "dcm-visual-cost-budget-reservation";
    public const string EventRefPrefix = "dcm-visual-cost-budget-event";
    public const long DefaultMaxJournalBytes = 1L * 1024L * 1024L * 1024L;
    public const string VisualCostBudgetStoreRootEnvironmentVariable =
        "DESIGN_CONVERSION_VISUAL_COST_BUDGET_STORE_ROOT";

    public const string StatusReserved = "reserved";
    public const string StatusReplayed = "replayed";
    public const string StatusDenied = "denied";
    public const string StatusSettled = "settled";
    public const string StatusReleased = "released";

    public const string EventTypeReserveDecision = "reserve_decision";
    public const string EventTypeSettled = "settled";
    public const string EventTypeReleased = "released";

    public const string DenialReasonMonthlyLimitExceeded = "DESIGN_CONVERSION_VISUAL_COST_MONTHLY_LIMIT_EXCEEDED";
    public const string NoDenialReason = "none";
    public const string InvalidRequestErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_REQUEST_INVALID";
    public const string IdempotencyConflictErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_IDEMPOTENCY_CONFLICT";
    public const string PeriodAuthorityConflictErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_PERIOD_AUTHORITY_CONFLICT";
    public const string ReservationNotFoundErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_RESERVATION_NOT_FOUND";
    public const string InvalidTransitionErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_TRANSITION_INVALID";
    public const string SettlementConflictErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_SETTLEMENT_CONFLICT";
    public const string SettlementExceedsReservationErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_SETTLEMENT_EXCEEDS_RESERVATION";
    public const string ReadFailedErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_STORE_READ_FAILED";
    public const string WriteFailedErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_STORE_WRITE_FAILED";
    public const string JournalQuotaExceededErrorCode = "DESIGN_CONVERSION_VISUAL_COST_BUDGET_STORE_JOURNAL_QUOTA_EXCEEDED";
    public const string JournalQuotaExceededSafeMessage = "DesignConversion visual-cost budget store journal quota was exceeded.";
    public const string CancelledErrorCode = "CANCELLED";

    private const string PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind;

    private static readonly JsonSerializerOptions JsonOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    private static readonly Encoding StrictUtf8Encoding = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> MutationGates = new(GetPathKeyComparer());

    private readonly string _rootDirectory;
    private readonly string _recordsPath;
    private readonly long _maxJournalBytes;
    private readonly SemaphoreSlim _gate;

    public JsonlDesignConversionVisualCostBudgetStore(
        string rootDirectory,
        long maxJournalBytes = DefaultMaxJournalBytes)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A visual-cost budget store root is required.", nameof(rootDirectory));
        if (maxJournalBytes <= 0L)
            throw new ArgumentOutOfRangeException(
                nameof(maxJournalBytes),
                "A visual-cost budget store journal size limit must be positive.");

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _recordsPath = Path.Combine(_rootDirectory, RecordsFileName);
        _maxJournalBytes = maxJournalBytes;
        _gate = MutationGates.GetOrAdd(NormalizeMutationGateKey(_recordsPath), _ => new SemaphoreSlim(1, 1));
    }

    public string RootDirectory => _rootDirectory;

    public string RecordsPath => _recordsPath;

    public long MaxJournalBytes => _maxJournalBytes;

    public static JsonlDesignConversionVisualCostBudgetStore CreateDefault()
        => new(ResolveDefaultRoot(
            Environment.GetEnvironmentVariable(VisualCostBudgetStoreRootEnvironmentVariable),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)));

    internal static string ResolveDefaultRoot(
        string? configuredRoot,
        string? localApplicationDataRoot)
    {
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            if (!Path.IsPathFullyQualified(configuredRoot))
            {
                throw new InvalidOperationException(
                    "DesignConversion visual-cost budget store configured root must be fully qualified.");
            }

            return Path.GetFullPath(configuredRoot);
        }

        if (string.IsNullOrWhiteSpace(localApplicationDataRoot) ||
            !Path.IsPathFullyQualified(localApplicationDataRoot))
        {
            throw new InvalidOperationException(
                "DesignConversion visual-cost budget store default root requires a fully qualified LocalApplicationData directory.");
        }

        return Path.Combine(
            Path.GetFullPath(localApplicationDataRoot),
            "llm_mvp_core",
            "design-conversion",
            "visual-cost-budget-store");
    }

    public async Task<OperationResult<DesignConversionVisualCostReservationResult>> TryReserveAsync(
        DesignConversionVisualCostReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionVisualCostReservationResult>();

        var normalized = TryNormalizeReserveRequest(request);
        if (!normalized.IsSuccess)
            return normalized.MapError<DesignConversionVisualCostReservationResult>();

        try
        {
            await _gate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionVisualCostReservationResult>();
        }

        try
        {
            var journal = await ReadJournalNoLockAsync(cancellationToken);
            if (!journal.IsSuccess)
                return journal.MapError<DesignConversionVisualCostReservationResult>();

            var state = ReplayJournal(journal.Value!);
            if (!state.IsSuccess)
                return state.MapError<DesignConversionVisualCostReservationResult>();

            var candidate = normalized.Value!;
            var replayedState = state.Value!;
            if (replayedState.ReservationsByRef.TryGetValue(candidate.ReservationRef, out var existing))
                return ReplayOrConflictReservation(existing, candidate, replayedState);

            var authority = replayedState.TryGetPeriodAuthority(
                candidate.UserRef,
                candidate.PeriodStartUtc,
                candidate.PeriodEndExclusiveUtc);
            if (authority is not null && !MatchesPeriodAuthority(authority, candidate))
                return FailPeriodAuthorityConflict<DesignConversionVisualCostReservationResult>();

            var periodStateBefore = BuildPeriodState(
                replayedState,
                candidate.UserRef,
                candidate.PeriodStartUtc,
                candidate.PeriodEndExclusiveUtc,
                authority?.MonthlyLimitUsd ?? candidate.MonthlyLimitUsd);
            var decisionStatus = HasRemainingCapacity(
                    periodStateBefore.FinalizedSpendUsd,
                    periodStateBefore.ActiveReservedUsd,
                    candidate.RequestedReserveUsd,
                    authority?.MonthlyLimitUsd ?? candidate.MonthlyLimitUsd)
                ? StatusReserved
                : StatusDenied;
            var denialReasonCode = decisionStatus == StatusDenied
                ? DenialReasonMonthlyLimitExceeded
                : NoDenialReason;
            var record = BuildReserveDecisionRecord(candidate, decisionStatus, denialReasonCode);
            var append = await AppendJournalRecordNoLockAsync(record, cancellationToken);
            if (!append.IsSuccess)
                return append.MapError<DesignConversionVisualCostReservationResult>();

            state = ReplayJournal(journal.Value!.Concat([record]).ToArray());
            if (!state.IsSuccess)
                return state.MapError<DesignConversionVisualCostReservationResult>();

            replayedState = state.Value!;
            var periodStateAfter = BuildPeriodState(
                replayedState,
                candidate.UserRef,
                candidate.PeriodStartUtc,
                candidate.PeriodEndExclusiveUtc,
                candidate.MonthlyLimitUsd);
            return OperationResult<DesignConversionVisualCostReservationResult>.Ok(
                BuildReservationResult(
                    status: decisionStatus,
                    reserved: decisionStatus == StatusReserved,
                    replayed: false,
                    reservation: replayedState.ReservationsByRef[candidate.ReservationRef],
                    periodState: periodStateAfter,
                    monthlyLimitUsd: candidate.MonthlyLimitUsd));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionVisualCostReservationResult>();
        }
        catch (JsonException)
        {
            return WriteFailed<DesignConversionVisualCostReservationResult>();
        }
        catch (IOException)
        {
            return WriteFailed<DesignConversionVisualCostReservationResult>();
        }
        catch (UnauthorizedAccessException)
        {
            return WriteFailed<DesignConversionVisualCostReservationResult>();
        }
        catch (SecurityException)
        {
            return WriteFailed<DesignConversionVisualCostReservationResult>();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<OperationResult<DesignConversionVisualCostSettlementResult>> SettleAsync(
        DesignConversionVisualCostSettlementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionVisualCostSettlementResult>();

        var normalized = TryNormalizeSettlementRequest(request);
        if (!normalized.IsSuccess)
            return normalized.MapError<DesignConversionVisualCostSettlementResult>();

        try
        {
            await _gate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionVisualCostSettlementResult>();
        }

        try
        {
            var journal = await ReadJournalNoLockAsync(cancellationToken);
            if (!journal.IsSuccess)
                return journal.MapError<DesignConversionVisualCostSettlementResult>();

            var state = ReplayJournal(journal.Value!);
            if (!state.IsSuccess)
                return state.MapError<DesignConversionVisualCostSettlementResult>();

            var candidate = normalized.Value!;
            if (!state.Value!.ReservationsByRef.TryGetValue(candidate.ReservationRef, out var existing))
                return FailReservationNotFound<DesignConversionVisualCostSettlementResult>();
            if (!MatchesUserPeriod(existing, candidate.UserRef, candidate.PeriodStartUtc, candidate.PeriodEndExclusiveUtc))
                return FailIdempotencyConflict<DesignConversionVisualCostSettlementResult>();
            if (existing.DecisionStatus == StatusDenied)
                return FailInvalidTransition<DesignConversionVisualCostSettlementResult>();
            if (existing.ReleasedAtUtc.HasValue)
                return FailInvalidTransition<DesignConversionVisualCostSettlementResult>();
            if (existing.SettledAtUtc.HasValue)
            {
                if (existing.ActualCostUsd == candidate.ActualCostUsd)
                {
                    var periodState = BuildPeriodState(
                        state.Value,
                        existing.UserRef,
                        existing.PeriodStartUtc,
                        existing.PeriodEndExclusiveUtc,
                        existing.MonthlyLimitUsd);
                    return OperationResult<DesignConversionVisualCostSettlementResult>.Ok(
                        BuildSettlementResult(StatusReplayed, replayed: true, existing, periodState));
                }

                return OperationResult<DesignConversionVisualCostSettlementResult>.Fail(
                    "DesignConversion visual-cost budget settlement conflicts with an existing durable settlement.",
                    SettlementConflictErrorCode);
            }
            if (candidate.ActualCostUsd > existing.RequestedReserveUsd)
            {
                return OperationResult<DesignConversionVisualCostSettlementResult>.Fail(
                    "DesignConversion visual-cost budget settlement exceeds the reserved amount.",
                    SettlementExceedsReservationErrorCode);
            }

            var record = BuildSettlementRecord(existing, candidate);
            var append = await AppendJournalRecordNoLockAsync(record, cancellationToken);
            if (!append.IsSuccess)
                return append.MapError<DesignConversionVisualCostSettlementResult>();

            state = ReplayJournal(journal.Value!.Concat([record]).ToArray());
            if (!state.IsSuccess)
                return state.MapError<DesignConversionVisualCostSettlementResult>();

            var settled = state.Value!.ReservationsByRef[candidate.ReservationRef];
            var settledPeriod = BuildPeriodState(
                state.Value,
                settled.UserRef,
                settled.PeriodStartUtc,
                settled.PeriodEndExclusiveUtc,
                settled.MonthlyLimitUsd);
            return OperationResult<DesignConversionVisualCostSettlementResult>.Ok(
                BuildSettlementResult(StatusSettled, replayed: false, settled, settledPeriod));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionVisualCostSettlementResult>();
        }
        catch (JsonException)
        {
            return WriteFailed<DesignConversionVisualCostSettlementResult>();
        }
        catch (IOException)
        {
            return WriteFailed<DesignConversionVisualCostSettlementResult>();
        }
        catch (UnauthorizedAccessException)
        {
            return WriteFailed<DesignConversionVisualCostSettlementResult>();
        }
        catch (SecurityException)
        {
            return WriteFailed<DesignConversionVisualCostSettlementResult>();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<OperationResult<DesignConversionVisualCostReleaseResult>> ReleaseAsync(
        DesignConversionVisualCostReleaseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionVisualCostReleaseResult>();

        var normalized = TryNormalizeReleaseRequest(request);
        if (!normalized.IsSuccess)
            return normalized.MapError<DesignConversionVisualCostReleaseResult>();

        try
        {
            await _gate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionVisualCostReleaseResult>();
        }

        try
        {
            var journal = await ReadJournalNoLockAsync(cancellationToken);
            if (!journal.IsSuccess)
                return journal.MapError<DesignConversionVisualCostReleaseResult>();

            var state = ReplayJournal(journal.Value!);
            if (!state.IsSuccess)
                return state.MapError<DesignConversionVisualCostReleaseResult>();

            var candidate = normalized.Value!;
            if (!state.Value!.ReservationsByRef.TryGetValue(candidate.ReservationRef, out var existing))
                return FailReservationNotFound<DesignConversionVisualCostReleaseResult>();
            if (!MatchesUserPeriod(existing, candidate.UserRef, candidate.PeriodStartUtc, candidate.PeriodEndExclusiveUtc))
                return FailIdempotencyConflict<DesignConversionVisualCostReleaseResult>();
            if (existing.DecisionStatus == StatusDenied)
                return FailInvalidTransition<DesignConversionVisualCostReleaseResult>();
            if (existing.SettledAtUtc.HasValue)
                return FailInvalidTransition<DesignConversionVisualCostReleaseResult>();
            if (existing.ReleasedAtUtc.HasValue)
            {
                var periodState = BuildPeriodState(
                    state.Value,
                    existing.UserRef,
                    existing.PeriodStartUtc,
                    existing.PeriodEndExclusiveUtc,
                    existing.MonthlyLimitUsd);
                return OperationResult<DesignConversionVisualCostReleaseResult>.Ok(
                    BuildReleaseResult(StatusReplayed, replayed: true, existing, periodState));
            }

            var record = BuildReleaseRecord(existing, candidate);
            var append = await AppendJournalRecordNoLockAsync(record, cancellationToken);
            if (!append.IsSuccess)
                return append.MapError<DesignConversionVisualCostReleaseResult>();

            state = ReplayJournal(journal.Value!.Concat([record]).ToArray());
            if (!state.IsSuccess)
                return state.MapError<DesignConversionVisualCostReleaseResult>();

            var released = state.Value!.ReservationsByRef[candidate.ReservationRef];
            var releasedPeriod = BuildPeriodState(
                state.Value,
                released.UserRef,
                released.PeriodStartUtc,
                released.PeriodEndExclusiveUtc,
                released.MonthlyLimitUsd);
            return OperationResult<DesignConversionVisualCostReleaseResult>.Ok(
                BuildReleaseResult(StatusReleased, replayed: false, released, releasedPeriod));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionVisualCostReleaseResult>();
        }
        catch (JsonException)
        {
            return WriteFailed<DesignConversionVisualCostReleaseResult>();
        }
        catch (IOException)
        {
            return WriteFailed<DesignConversionVisualCostReleaseResult>();
        }
        catch (UnauthorizedAccessException)
        {
            return WriteFailed<DesignConversionVisualCostReleaseResult>();
        }
        catch (SecurityException)
        {
            return WriteFailed<DesignConversionVisualCostReleaseResult>();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<OperationResult<DesignConversionVisualCostBudgetPeriodState>> ReadPeriodAsync(
        DesignConversionVisualCostBudgetPeriodQuery query,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionVisualCostBudgetPeriodState>();

        var normalized = TryNormalizePeriodQuery(query);
        if (!normalized.IsSuccess)
            return normalized.MapError<DesignConversionVisualCostBudgetPeriodState>();

        try
        {
            await _gate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionVisualCostBudgetPeriodState>();
        }

        try
        {
            var journal = await ReadJournalNoLockAsync(cancellationToken);
            if (!journal.IsSuccess)
                return journal.MapError<DesignConversionVisualCostBudgetPeriodState>();
            var state = ReplayJournal(journal.Value!);
            if (!state.IsSuccess)
                return state.MapError<DesignConversionVisualCostBudgetPeriodState>();

            var candidate = normalized.Value!;
            return OperationResult<DesignConversionVisualCostBudgetPeriodState>.Ok(
                BuildPeriodState(
                    state.Value!,
                    candidate.UserRef,
                    candidate.PeriodStartUtc,
                    candidate.PeriodEndExclusiveUtc,
                    monthlyLimitUsd: null));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionVisualCostBudgetPeriodState>();
        }
        catch (IOException)
        {
            return ReadFailed<DesignConversionVisualCostBudgetPeriodState>();
        }
        catch (UnauthorizedAccessException)
        {
            return ReadFailed<DesignConversionVisualCostBudgetPeriodState>();
        }
        catch (SecurityException)
        {
            return ReadFailed<DesignConversionVisualCostBudgetPeriodState>();
        }
        finally
        {
            _gate.Release();
        }
    }

    internal static string BuildCanonicalReservationRef(string source)
        => BuildCanonicalRef(ReservationRefPrefix, source);

    internal static bool IsCanonicalReservationRef(string? value)
        => IsCanonicalRef(value, ReservationRefPrefix);

    private OperationResult<DesignConversionVisualCostReservationResult> ReplayOrConflictReservation(
        MutableReservationState existing,
        DesignConversionVisualCostReservationRequest candidate,
        JournalReplayState replayState)
    {
        if (!MatchesReservationDimensions(existing, candidate))
            return FailIdempotencyConflict<DesignConversionVisualCostReservationResult>();

        var periodState = BuildPeriodState(
            replayState,
            existing.UserRef,
            existing.PeriodStartUtc,
            existing.PeriodEndExclusiveUtc,
            existing.MonthlyLimitUsd);
        return OperationResult<DesignConversionVisualCostReservationResult>.Ok(
            BuildReservationResult(
                status: StatusReplayed,
                reserved: existing.DecisionStatus == StatusReserved &&
                    !existing.SettledAtUtc.HasValue &&
                    !existing.ReleasedAtUtc.HasValue,
                replayed: true,
                reservation: existing,
                periodState: periodState,
                monthlyLimitUsd: existing.MonthlyLimitUsd));
    }

    private static OperationResult<DesignConversionVisualCostReservationRequest> TryNormalizeReserveRequest(
        DesignConversionVisualCostReservationRequest? request)
    {
        if (request is null)
            return InvalidRequest<DesignConversionVisualCostReservationRequest>();
        if (!IsCanonicalReservationRef(request.ReservationRef) ||
            !JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(request.UserRef) ||
            !IsExactUtcCalendarMonthPeriod(request.PeriodStartUtc, request.PeriodEndExclusiveUtc) ||
            !IsUtcTimestamp(request.RequestedAtUtc) ||
            request.MonthlyLimitUsd <= 0m ||
            request.RequestedReserveUsd <= 0m)
        {
            return InvalidRequest<DesignConversionVisualCostReservationRequest>();
        }

        return OperationResult<DesignConversionVisualCostReservationRequest>.Ok(
            request with
            {
                SubmittedTier = JsonlDesignConversionVisualCostLedger.NormalizeSubmittedTier(request.SubmittedTier),
                RequestedAtUtc = request.RequestedAtUtc.ToUniversalTime()
            });
    }

    private static OperationResult<DesignConversionVisualCostSettlementRequest> TryNormalizeSettlementRequest(
        DesignConversionVisualCostSettlementRequest? request)
    {
        if (request is null)
            return InvalidRequest<DesignConversionVisualCostSettlementRequest>();
        if (!IsCanonicalReservationRef(request.ReservationRef) ||
            !JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(request.UserRef) ||
            !IsExactUtcCalendarMonthPeriod(request.PeriodStartUtc, request.PeriodEndExclusiveUtc) ||
            !IsUtcTimestamp(request.SettledAtUtc) ||
            request.ActualCostUsd < 0m)
        {
            return InvalidRequest<DesignConversionVisualCostSettlementRequest>();
        }

        return OperationResult<DesignConversionVisualCostSettlementRequest>.Ok(
            request with { SettledAtUtc = request.SettledAtUtc.ToUniversalTime() });
    }

    private static OperationResult<DesignConversionVisualCostReleaseRequest> TryNormalizeReleaseRequest(
        DesignConversionVisualCostReleaseRequest? request)
    {
        if (request is null)
            return InvalidRequest<DesignConversionVisualCostReleaseRequest>();
        if (!IsCanonicalReservationRef(request.ReservationRef) ||
            !JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(request.UserRef) ||
            !IsExactUtcCalendarMonthPeriod(request.PeriodStartUtc, request.PeriodEndExclusiveUtc) ||
            !IsUtcTimestamp(request.ReleasedAtUtc))
        {
            return InvalidRequest<DesignConversionVisualCostReleaseRequest>();
        }

        return OperationResult<DesignConversionVisualCostReleaseRequest>.Ok(
            request with { ReleasedAtUtc = request.ReleasedAtUtc.ToUniversalTime() });
    }

    private static OperationResult<DesignConversionVisualCostBudgetPeriodQuery> TryNormalizePeriodQuery(
        DesignConversionVisualCostBudgetPeriodQuery? query)
    {
        if (query is null)
            return InvalidRequest<DesignConversionVisualCostBudgetPeriodQuery>();
        if (!JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(query.UserRef) ||
            !IsExactUtcCalendarMonthPeriod(query.PeriodStartUtc, query.PeriodEndExclusiveUtc))
        {
            return InvalidRequest<DesignConversionVisualCostBudgetPeriodQuery>();
        }

        return OperationResult<DesignConversionVisualCostBudgetPeriodQuery>.Ok(query);
    }

    private async Task<OperationResult<IReadOnlyList<BudgetJournalRecord>>> ReadJournalNoLockAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_recordsPath))
            return OperationResult<IReadOnlyList<BudgetJournalRecord>>.Ok([]);

        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(_recordsPath, StrictUtf8Encoding, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<IReadOnlyList<BudgetJournalRecord>>();
        }
        catch (DecoderFallbackException)
        {
            return ReadFailed<IReadOnlyList<BudgetJournalRecord>>();
        }
        catch (IOException)
        {
            return ReadFailed<IReadOnlyList<BudgetJournalRecord>>();
        }
        catch (UnauthorizedAccessException)
        {
            return ReadFailed<IReadOnlyList<BudgetJournalRecord>>();
        }
        catch (SecurityException)
        {
            return ReadFailed<IReadOnlyList<BudgetJournalRecord>>();
        }

        var records = new List<BudgetJournalRecord>(lines.Length);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                return ReadFailed<IReadOnlyList<BudgetJournalRecord>>();

            BudgetJournalRecord? record;
            try
            {
                record = JsonSerializer.Deserialize<BudgetJournalRecord>(line, JsonOptions);
            }
            catch (JsonException)
            {
                return ReadFailed<IReadOnlyList<BudgetJournalRecord>>();
            }

            var normalized = TryNormalizeJournalRecord(record);
            if (!normalized.IsSuccess)
                return ReadFailed<IReadOnlyList<BudgetJournalRecord>>();
            records.Add(normalized.Value!);
        }

        return OperationResult<IReadOnlyList<BudgetJournalRecord>>.Ok(records);
    }

    private async Task<OperationResult<bool>> AppendJournalRecordNoLockAsync(
        BudgetJournalRecord record,
        CancellationToken cancellationToken)
    {
        string line;
        try
        {
            line = JsonSerializer.Serialize(record, JsonOptions);
        }
        catch (JsonException)
        {
            return WriteFailed<bool>();
        }

        var appendedBytes = StrictUtf8Encoding.GetBytes(line + Environment.NewLine);
        var quota = EnsureAppendWithinJournalQuotaNoLock(appendedBytes);
        if (!quota.IsSuccess)
            return quota;

        try
        {
            Directory.CreateDirectory(_rootDirectory);
            await CommitCopyOnWriteAppendAsync(appendedBytes, cancellationToken);
            return OperationResult<bool>.Ok(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<bool>();
        }
        catch (IOException)
        {
            return WriteFailed<bool>();
        }
        catch (UnauthorizedAccessException)
        {
            return WriteFailed<bool>();
        }
        catch (SecurityException)
        {
            return WriteFailed<bool>();
        }
    }

    private OperationResult<bool> EnsureAppendWithinJournalQuotaNoLock(byte[] appendedBytes)
    {
        var length = TryReadExistingJournalLengthNoLock();
        if (!length.IsSuccess)
            return length.MapError<bool>();

        try
        {
            var projectedLength = checked(length.Value + appendedBytes.LongLength);
            return projectedLength <= _maxJournalBytes
                ? OperationResult<bool>.Ok(true)
                : JournalQuotaExceeded<bool>();
        }
        catch (OverflowException)
        {
            return JournalQuotaExceeded<bool>();
        }
    }

    private OperationResult<long> TryReadExistingJournalLengthNoLock()
    {
        try
        {
            var attributes = File.GetAttributes(_recordsPath);
            if ((attributes & FileAttributes.Directory) != 0)
                return WriteFailed<long>();

            return OperationResult<long>.Ok(new FileInfo(_recordsPath).Length);
        }
        catch (FileNotFoundException)
        {
            return OperationResult<long>.Ok(0L);
        }
        catch (DirectoryNotFoundException)
        {
            return OperationResult<long>.Ok(0L);
        }
        catch (IOException)
        {
            return WriteFailed<long>();
        }
        catch (UnauthorizedAccessException)
        {
            return WriteFailed<long>();
        }
        catch (SecurityException)
        {
            return WriteFailed<long>();
        }
    }

    private async Task CommitCopyOnWriteAppendAsync(byte[] appendedBytes, CancellationToken cancellationToken)
    {
        var tempPath = Path.Combine(_rootDirectory, $"{RecordsFileName}.{Guid.NewGuid():N}.append.tmp");
        var commitSucceeded = false;
        try
        {
            await using (var temp = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                if (File.Exists(_recordsPath))
                {
                    await using var source = new FileStream(
                        _recordsPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read | FileShare.Delete,
                        bufferSize: 81920,
                        FileOptions.Asynchronous | FileOptions.SequentialScan);
                    await source.CopyToAsync(temp, cancellationToken);
                }

                await temp.WriteAsync(appendedBytes, cancellationToken);
                await temp.FlushAsync(cancellationToken);
                temp.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(_recordsPath))
                File.Replace(tempPath, _recordsPath, destinationBackupFileName: null);
            else
                File.Move(tempPath, _recordsPath);
            commitSucceeded = true;
        }
        finally
        {
            if (!commitSucceeded)
                DeleteTempBestEffort(tempPath);
        }
    }

    private static OperationResult<BudgetJournalRecord> TryNormalizeJournalRecord(BudgetJournalRecord? record)
    {
        if (record is null ||
            !string.Equals(record.LedgerKind, LedgerKind, StringComparison.Ordinal) ||
            !string.Equals(record.SchemaVersion, SchemaVersion, StringComparison.Ordinal) ||
            !IsCanonicalRef(record.EventId, EventRefPrefix) ||
            !IsCanonicalReservationRef(record.ReservationRef) ||
            !JsonlDesignConversionVisualCostLedger.IsTechnicalUserRef(record.UserRef) ||
            !string.Equals(record.PeriodKind, PeriodKind, StringComparison.Ordinal) ||
            !IsExactUtcCalendarMonthPeriod(record.PeriodStartUtc, record.PeriodEndExclusiveUtc) ||
            !IsUtcTimestamp(record.RecordedAtUtc) ||
            record.MonthlyLimitUsd <= 0m ||
            record.RequestedReserveUsd <= 0m ||
            !HasExpectedMetadata(record.Metadata))
        {
            return ReadFailed<BudgetJournalRecord>();
        }

        var normalized = record with
        {
            SubmittedTier = JsonlDesignConversionVisualCostLedger.NormalizeSubmittedTier(record.SubmittedTier),
            RecordedAtUtc = record.RecordedAtUtc.ToUniversalTime()
        };

        if (!string.Equals(record.SubmittedTier, normalized.SubmittedTier, StringComparison.Ordinal))
            return ReadFailed<BudgetJournalRecord>();

        if (normalized.EventType == EventTypeReserveDecision)
        {
            if (normalized.ActualCostUsd != 0m ||
                (normalized.DecisionStatus != StatusReserved && normalized.DecisionStatus != StatusDenied) ||
                (normalized.DecisionStatus == StatusReserved &&
                 !string.Equals(normalized.DenialReasonCode, NoDenialReason, StringComparison.Ordinal)) ||
                (normalized.DecisionStatus == StatusDenied &&
                 !string.Equals(normalized.DenialReasonCode, DenialReasonMonthlyLimitExceeded, StringComparison.Ordinal)))
            {
                return ReadFailed<BudgetJournalRecord>();
            }
        }
        else if (normalized.EventType == EventTypeSettled)
        {
            if (normalized.ActualCostUsd < 0m ||
                normalized.ActualCostUsd > normalized.RequestedReserveUsd ||
                normalized.DecisionStatus != StatusSettled ||
                !string.Equals(normalized.DenialReasonCode, NoDenialReason, StringComparison.Ordinal))
            {
                return ReadFailed<BudgetJournalRecord>();
            }
        }
        else if (normalized.EventType == EventTypeReleased)
        {
            if (normalized.ActualCostUsd != 0m ||
                normalized.DecisionStatus != StatusReleased ||
                !string.Equals(normalized.DenialReasonCode, NoDenialReason, StringComparison.Ordinal))
            {
                return ReadFailed<BudgetJournalRecord>();
            }
        }
        else
        {
            return ReadFailed<BudgetJournalRecord>();
        }

        var expectedEventId = BuildEventId(normalized);
        return string.Equals(normalized.EventId, expectedEventId, StringComparison.Ordinal)
            ? OperationResult<BudgetJournalRecord>.Ok(normalized)
            : ReadFailed<BudgetJournalRecord>();
    }

    private static OperationResult<JournalReplayState> ReplayJournal(
        IReadOnlyList<BudgetJournalRecord> records)
    {
        var state = new JournalReplayState();
        var eventIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var record in records)
        {
            if (!eventIds.Add(record.EventId))
                return ReadFailed<JournalReplayState>();

            if (record.EventType == EventTypeReserveDecision)
            {
                if (state.ReservationsByRef.ContainsKey(record.ReservationRef))
                    return ReadFailed<JournalReplayState>();

                var authority = state.TryGetPeriodAuthority(
                    record.UserRef,
                    record.PeriodStartUtc,
                    record.PeriodEndExclusiveUtc);
                if (authority is not null && !MatchesPeriodAuthority(authority, record))
                    return ReadFailed<JournalReplayState>();

                var periodStateBefore = BuildPeriodState(
                    state,
                    record.UserRef,
                    record.PeriodStartUtc,
                    record.PeriodEndExclusiveUtc,
                    authority?.MonthlyLimitUsd ?? record.MonthlyLimitUsd);
                var expectedDecisionStatus = HasRemainingCapacity(
                        periodStateBefore.FinalizedSpendUsd,
                        periodStateBefore.ActiveReservedUsd,
                        record.RequestedReserveUsd,
                        authority?.MonthlyLimitUsd ?? record.MonthlyLimitUsd)
                    ? StatusReserved
                    : StatusDenied;
                if (!string.Equals(record.DecisionStatus, expectedDecisionStatus, StringComparison.Ordinal))
                    return ReadFailed<JournalReplayState>();

                state.ReservationsByRef[record.ReservationRef] = new MutableReservationState(
                    ReservationRef: record.ReservationRef,
                    UserRef: record.UserRef,
                    SubmittedTier: record.SubmittedTier,
                    PeriodStartUtc: record.PeriodStartUtc,
                    PeriodEndExclusiveUtc: record.PeriodEndExclusiveUtc,
                    MonthlyLimitUsd: record.MonthlyLimitUsd,
                    RequestedReserveUsd: record.RequestedReserveUsd,
                    ReservedAtUtc: record.RecordedAtUtc,
                    DecisionStatus: record.DecisionStatus,
                    DenialReasonCode: record.DenialReasonCode);
                state.SetPeriodAuthority(record);
                continue;
            }

            if (!state.ReservationsByRef.TryGetValue(record.ReservationRef, out var reservation) ||
                !MatchesRecordDimensions(reservation, record) ||
                reservation.DecisionStatus != StatusReserved)
            {
                return ReadFailed<JournalReplayState>();
            }

            if (record.EventType == EventTypeSettled)
            {
                if (reservation.SettledAtUtc.HasValue ||
                    reservation.ReleasedAtUtc.HasValue ||
                    record.RecordedAtUtc < reservation.ReservedAtUtc)
                    return ReadFailed<JournalReplayState>();

                reservation.SettledAtUtc = record.RecordedAtUtc;
                reservation.ActualCostUsd = record.ActualCostUsd;
                continue;
            }

            if (record.EventType == EventTypeReleased)
            {
                if (reservation.SettledAtUtc.HasValue ||
                    reservation.ReleasedAtUtc.HasValue ||
                    record.RecordedAtUtc < reservation.ReservedAtUtc)
                    return ReadFailed<JournalReplayState>();

                reservation.ReleasedAtUtc = record.RecordedAtUtc;
                continue;
            }
        }

        return OperationResult<JournalReplayState>.Ok(state);
    }

    private static DesignConversionVisualCostBudgetPeriodState BuildPeriodState(
        JournalReplayState replayState,
        string userRef,
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndExclusiveUtc,
        decimal? monthlyLimitUsd)
    {
        var reservations = replayState.ReservationsByRef.Values
            .Where(reservation =>
                string.Equals(reservation.UserRef, userRef, StringComparison.Ordinal) &&
                reservation.PeriodStartUtc == periodStartUtc &&
                reservation.PeriodEndExclusiveUtc == periodEndExclusiveUtc)
            .OrderBy(reservation => reservation.ReservedAtUtc)
            .ThenBy(reservation => reservation.ReservationRef, StringComparer.Ordinal)
            .ToArray();
        var observedMonthlyLimit = monthlyLimitUsd ??
            (reservations.Length == 0 ? 0m : reservations[0].MonthlyLimitUsd);
        var finalized = reservations
            .Where(reservation => reservation.SettledAtUtc.HasValue)
            .Sum(reservation => reservation.ActualCostUsd);
        var active = reservations
            .Where(reservation =>
                reservation.DecisionStatus == StatusReserved &&
                !reservation.SettledAtUtc.HasValue &&
                !reservation.ReleasedAtUtc.HasValue)
            .Sum(reservation => reservation.RequestedReserveUsd);

        return new DesignConversionVisualCostBudgetPeriodState(
            UserRef: userRef,
            PeriodKind: PeriodKind,
            PeriodStartUtc: periodStartUtc,
            PeriodEndExclusiveUtc: periodEndExclusiveUtc,
            ObservedMonthlyLimitUsd: observedMonthlyLimit,
            FinalizedSpendUsd: finalized,
            ActiveReservedUsd: active,
            RemainingBudgetUsd: Math.Max(0m, observedMonthlyLimit - finalized - active),
            ReservationCount: reservations.Length,
            ActiveReservationCount: reservations.Count(reservation =>
                reservation.DecisionStatus == StatusReserved &&
                !reservation.SettledAtUtc.HasValue &&
                !reservation.ReleasedAtUtc.HasValue),
            SettledReservationCount: reservations.Count(reservation => reservation.SettledAtUtc.HasValue),
            ReleasedReservationCount: reservations.Count(reservation => reservation.ReleasedAtUtc.HasValue),
            DeniedReservationCount: reservations.Count(reservation => reservation.DecisionStatus == StatusDenied),
            Reservations: reservations.Select(ToSnapshot).ToArray());
    }

    private static DesignConversionVisualCostReservationResult BuildReservationResult(
        string status,
        bool reserved,
        bool replayed,
        MutableReservationState reservation,
        DesignConversionVisualCostBudgetPeriodState periodState,
        decimal monthlyLimitUsd)
        => new(
            Status: status,
            Reserved: reserved,
            Replayed: replayed,
            ReservationRef: reservation.ReservationRef,
            UserRef: reservation.UserRef,
            SubmittedTier: reservation.SubmittedTier,
            PeriodStartUtc: reservation.PeriodStartUtc,
            PeriodEndExclusiveUtc: reservation.PeriodEndExclusiveUtc,
            MonthlyLimitUsd: monthlyLimitUsd,
            RequestedReserveUsd: reservation.RequestedReserveUsd,
            FinalizedSpendUsd: periodState.FinalizedSpendUsd,
            ActiveReservedUsd: periodState.ActiveReservedUsd,
            RemainingBudgetUsd: periodState.RemainingBudgetUsd,
            DenialReasonCode: reservation.DenialReasonCode,
            Metadata: BuildResultMetadata());

    private static DesignConversionVisualCostSettlementResult BuildSettlementResult(
        string status,
        bool replayed,
        MutableReservationState reservation,
        DesignConversionVisualCostBudgetPeriodState periodState)
        => new(
            Status: status,
            Settled: true,
            Replayed: replayed,
            ReservationRef: reservation.ReservationRef,
            UserRef: reservation.UserRef,
            PeriodStartUtc: reservation.PeriodStartUtc,
            PeriodEndExclusiveUtc: reservation.PeriodEndExclusiveUtc,
            ReservedUsd: reservation.RequestedReserveUsd,
            ActualCostUsd: reservation.ActualCostUsd,
            FinalizedSpendUsd: periodState.FinalizedSpendUsd,
            ActiveReservedUsd: periodState.ActiveReservedUsd,
            RemainingBudgetUsd: periodState.RemainingBudgetUsd,
            Metadata: BuildResultMetadata());

    private static DesignConversionVisualCostReleaseResult BuildReleaseResult(
        string status,
        bool replayed,
        MutableReservationState reservation,
        DesignConversionVisualCostBudgetPeriodState periodState)
        => new(
            Status: status,
            Released: true,
            Replayed: replayed,
            ReservationRef: reservation.ReservationRef,
            UserRef: reservation.UserRef,
            PeriodStartUtc: reservation.PeriodStartUtc,
            PeriodEndExclusiveUtc: reservation.PeriodEndExclusiveUtc,
            ReleasedReserveUsd: reservation.RequestedReserveUsd,
            FinalizedSpendUsd: periodState.FinalizedSpendUsd,
            ActiveReservedUsd: periodState.ActiveReservedUsd,
            RemainingBudgetUsd: periodState.RemainingBudgetUsd,
            Metadata: BuildResultMetadata());

    private static BudgetJournalRecord BuildReserveDecisionRecord(
        DesignConversionVisualCostReservationRequest request,
        string decisionStatus,
        string denialReasonCode)
    {
        var candidate = new BudgetJournalRecord(
            LedgerKind: LedgerKind,
            SchemaVersion: SchemaVersion,
            EventId: string.Empty,
            EventType: EventTypeReserveDecision,
            ReservationRef: request.ReservationRef,
            UserRef: request.UserRef,
            SubmittedTier: request.SubmittedTier,
            PeriodKind: PeriodKind,
            PeriodStartUtc: request.PeriodStartUtc,
            PeriodEndExclusiveUtc: request.PeriodEndExclusiveUtc,
            MonthlyLimitUsd: request.MonthlyLimitUsd,
            RequestedReserveUsd: request.RequestedReserveUsd,
            ActualCostUsd: 0m,
            RecordedAtUtc: request.RequestedAtUtc,
            DecisionStatus: decisionStatus,
            DenialReasonCode: denialReasonCode,
            Metadata: BuildFixedMetadata());
        return candidate with { EventId = BuildEventId(candidate) };
    }

    private static BudgetJournalRecord BuildSettlementRecord(
        MutableReservationState reservation,
        DesignConversionVisualCostSettlementRequest request)
    {
        var candidate = new BudgetJournalRecord(
            LedgerKind: LedgerKind,
            SchemaVersion: SchemaVersion,
            EventId: string.Empty,
            EventType: EventTypeSettled,
            ReservationRef: reservation.ReservationRef,
            UserRef: reservation.UserRef,
            SubmittedTier: reservation.SubmittedTier,
            PeriodKind: PeriodKind,
            PeriodStartUtc: reservation.PeriodStartUtc,
            PeriodEndExclusiveUtc: reservation.PeriodEndExclusiveUtc,
            MonthlyLimitUsd: reservation.MonthlyLimitUsd,
            RequestedReserveUsd: reservation.RequestedReserveUsd,
            ActualCostUsd: request.ActualCostUsd,
            RecordedAtUtc: request.SettledAtUtc,
            DecisionStatus: StatusSettled,
            DenialReasonCode: NoDenialReason,
            Metadata: BuildFixedMetadata());
        return candidate with { EventId = BuildEventId(candidate) };
    }

    private static BudgetJournalRecord BuildReleaseRecord(
        MutableReservationState reservation,
        DesignConversionVisualCostReleaseRequest request)
    {
        var candidate = new BudgetJournalRecord(
            LedgerKind: LedgerKind,
            SchemaVersion: SchemaVersion,
            EventId: string.Empty,
            EventType: EventTypeReleased,
            ReservationRef: reservation.ReservationRef,
            UserRef: reservation.UserRef,
            SubmittedTier: reservation.SubmittedTier,
            PeriodKind: PeriodKind,
            PeriodStartUtc: reservation.PeriodStartUtc,
            PeriodEndExclusiveUtc: reservation.PeriodEndExclusiveUtc,
            MonthlyLimitUsd: reservation.MonthlyLimitUsd,
            RequestedReserveUsd: reservation.RequestedReserveUsd,
            ActualCostUsd: 0m,
            RecordedAtUtc: request.ReleasedAtUtc,
            DecisionStatus: StatusReleased,
            DenialReasonCode: NoDenialReason,
            Metadata: BuildFixedMetadata());
        return candidate with { EventId = BuildEventId(candidate) };
    }

    private static DesignConversionVisualCostReservationSnapshot ToSnapshot(
        MutableReservationState reservation)
        => new(
            ReservationRef: reservation.ReservationRef,
            Status: ReservationStatus(reservation),
            UserRef: reservation.UserRef,
            SubmittedTier: reservation.SubmittedTier,
            PeriodStartUtc: reservation.PeriodStartUtc,
            PeriodEndExclusiveUtc: reservation.PeriodEndExclusiveUtc,
            MonthlyLimitUsd: reservation.MonthlyLimitUsd,
            RequestedReserveUsd: reservation.RequestedReserveUsd,
            ActualCostUsd: reservation.ActualCostUsd,
            ReservedAtUtc: reservation.ReservedAtUtc,
            SettledAtUtc: reservation.SettledAtUtc,
            ReleasedAtUtc: reservation.ReleasedAtUtc,
            DenialReasonCode: reservation.DenialReasonCode);

    private static string ReservationStatus(MutableReservationState reservation)
    {
        if (reservation.DecisionStatus == StatusDenied)
            return StatusDenied;
        if (reservation.SettledAtUtc.HasValue)
            return StatusSettled;
        if (reservation.ReleasedAtUtc.HasValue)
            return StatusReleased;
        return StatusReserved;
    }

    private static bool MatchesReservationDimensions(
        MutableReservationState existing,
        DesignConversionVisualCostReservationRequest candidate)
        => string.Equals(existing.ReservationRef, candidate.ReservationRef, StringComparison.Ordinal) &&
           string.Equals(existing.UserRef, candidate.UserRef, StringComparison.Ordinal) &&
           string.Equals(existing.SubmittedTier, candidate.SubmittedTier, StringComparison.Ordinal) &&
           existing.PeriodStartUtc == candidate.PeriodStartUtc &&
           existing.PeriodEndExclusiveUtc == candidate.PeriodEndExclusiveUtc &&
           existing.MonthlyLimitUsd == candidate.MonthlyLimitUsd &&
           existing.RequestedReserveUsd == candidate.RequestedReserveUsd;

    private static bool MatchesUserPeriod(
        MutableReservationState existing,
        string userRef,
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndExclusiveUtc)
        => string.Equals(existing.UserRef, userRef, StringComparison.Ordinal) &&
           existing.PeriodStartUtc == periodStartUtc &&
           existing.PeriodEndExclusiveUtc == periodEndExclusiveUtc;

    private static bool MatchesRecordDimensions(MutableReservationState existing, BudgetJournalRecord record)
        => string.Equals(existing.ReservationRef, record.ReservationRef, StringComparison.Ordinal) &&
           string.Equals(existing.UserRef, record.UserRef, StringComparison.Ordinal) &&
           string.Equals(existing.SubmittedTier, record.SubmittedTier, StringComparison.Ordinal) &&
           existing.PeriodStartUtc == record.PeriodStartUtc &&
           existing.PeriodEndExclusiveUtc == record.PeriodEndExclusiveUtc &&
           existing.MonthlyLimitUsd == record.MonthlyLimitUsd &&
           existing.RequestedReserveUsd == record.RequestedReserveUsd;

    private static IReadOnlyDictionary<string, string> BuildFixedMetadata()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["store_purpose"] = "monthly_visual_cost_budget_reservation",
            ["atomicity_scope"] = "single_process_per_path_lock_copy_on_write",
            ["cross_process_atomicity"] = bool.FalseString,
            ["raw_payloads_persisted"] = bool.FalseString,
            ["raw_user_identity_persisted"] = bool.FalseString,
            ["runtime_enforcement_wired"] = bool.FalseString,
            ["training_artifact_db"] = bool.FalseString
        };

    private static IReadOnlyDictionary<string, string> BuildResultMetadata()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["store_kind"] = LedgerKind,
            ["schema_version"] = SchemaVersion,
            ["atomicity_scope"] = "single_process_per_path_lock_copy_on_write",
            ["cross_process_atomicity"] = bool.FalseString,
            ["raw_payloads_persisted"] = bool.FalseString,
            ["raw_user_identity_persisted"] = bool.FalseString,
            ["runtime_enforcement_wired"] = bool.FalseString
        };

    private static bool HasExpectedMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        var expected = BuildFixedMetadata();
        var actual = metadata ?? new Dictionary<string, string>();
        return actual.Count == expected.Count &&
               expected.All(pair =>
                   actual.TryGetValue(pair.Key, out var value) &&
                   string.Equals(value, pair.Value, StringComparison.Ordinal));
    }

    private static string BuildEventId(BudgetJournalRecord record)
    {
        var identity = string.Join("|",
        [
            LedgerKind,
            SchemaVersion,
            record.EventType,
            record.ReservationRef,
            record.UserRef,
            record.SubmittedTier,
            record.PeriodKind,
            record.PeriodStartUtc.ToString("O", CultureInfo.InvariantCulture),
            record.PeriodEndExclusiveUtc.ToString("O", CultureInfo.InvariantCulture),
            record.MonthlyLimitUsd.ToString("G29", CultureInfo.InvariantCulture),
            record.RequestedReserveUsd.ToString("G29", CultureInfo.InvariantCulture),
            record.ActualCostUsd.ToString("G29", CultureInfo.InvariantCulture),
            record.RecordedAtUtc.ToString("O", CultureInfo.InvariantCulture),
            record.DecisionStatus,
            record.DenialReasonCode
        ]);

        return BuildCanonicalRef(EventRefPrefix, identity);
    }

    private static bool IsExactUtcCalendarMonthPeriod(
        DateTimeOffset periodStartUtc,
        DateTimeOffset periodEndExclusiveUtc)
    {
        return periodStartUtc.Offset == TimeSpan.Zero &&
               periodEndExclusiveUtc.Offset == TimeSpan.Zero &&
               periodStartUtc.Day == 1 &&
               periodStartUtc.Hour == 0 &&
               periodStartUtc.Minute == 0 &&
               periodStartUtc.Second == 0 &&
               periodStartUtc.Millisecond == 0 &&
               periodStartUtc.Microsecond == 0 &&
               periodStartUtc.Nanosecond == 0 &&
               periodEndExclusiveUtc == periodStartUtc.AddMonths(1);
    }

    private static bool IsUtcTimestamp(DateTimeOffset value)
        => value.Offset == TimeSpan.Zero;

    private static OperationResult<T> InvalidRequest<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost budget store request is invalid.",
            InvalidRequestErrorCode);

    private static OperationResult<T> FailIdempotencyConflict<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost budget idempotency key conflicts with durable dimensions.",
            IdempotencyConflictErrorCode);

    private static OperationResult<T> FailPeriodAuthorityConflict<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost budget period authority conflicts with durable dimensions.",
            PeriodAuthorityConflictErrorCode);

    private static OperationResult<T> FailReservationNotFound<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost budget reservation was not found.",
            ReservationNotFoundErrorCode);

    private static OperationResult<T> FailInvalidTransition<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost budget reservation transition is invalid.",
            InvalidTransitionErrorCode);

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost budget store operation cancelled.",
            CancelledErrorCode);

    private static OperationResult<T> ReadFailed<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost budget store records could not be read.",
            ReadFailedErrorCode);

    private static OperationResult<T> WriteFailed<T>()
        => OperationResult<T>.Fail(
            "DesignConversion visual-cost budget store record could not be written.",
            WriteFailedErrorCode);

    private static OperationResult<T> JournalQuotaExceeded<T>()
        => OperationResult<T>.Fail(
            JournalQuotaExceededSafeMessage,
            JournalQuotaExceededErrorCode);

    private static StringComparer GetPathKeyComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static string NormalizeMutationGateKey(string recordsPath)
        => Path.GetFullPath(recordsPath);

    private static void DeleteTempBestEffort(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
    }

    private static string BuildCanonicalRef(string prefix, string value)
        => $"{prefix}-{Convert.ToHexString(SHA256.HashData(StrictUtf8Encoding.GetBytes(value ?? string.Empty))).ToLowerInvariant()}";

    private static bool IsCanonicalRef(string? value, string prefix)
    {
        var expectedLength = prefix.Length + 1 + 64;
        return !string.IsNullOrWhiteSpace(value) &&
               value.Length == expectedLength &&
               value.StartsWith(prefix + "-", StringComparison.Ordinal) &&
               value.Skip(prefix.Length + 1).All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');
    }

    private static bool HasRemainingCapacity(
        decimal finalizedSpendUsd,
        decimal activeReservedUsd,
        decimal requestedReserveUsd,
        decimal monthlyLimitUsd)
    {
        var remaining = monthlyLimitUsd;
        if (finalizedSpendUsd > remaining)
            return false;
        remaining -= finalizedSpendUsd;
        if (activeReservedUsd > remaining)
            return false;
        remaining -= activeReservedUsd;
        return requestedReserveUsd <= remaining;
    }

    private static bool MatchesPeriodAuthority(
        PeriodAuthorityState authority,
        DesignConversionVisualCostReservationRequest request)
        => string.Equals(authority.SubmittedTier, request.SubmittedTier, StringComparison.Ordinal) &&
           authority.MonthlyLimitUsd == request.MonthlyLimitUsd;

    private static bool MatchesPeriodAuthority(
        PeriodAuthorityState authority,
        BudgetJournalRecord record)
        => string.Equals(authority.SubmittedTier, record.SubmittedTier, StringComparison.Ordinal) &&
           authority.MonthlyLimitUsd == record.MonthlyLimitUsd;

    private sealed record BudgetJournalRecord(
        string LedgerKind,
        string SchemaVersion,
        string EventId,
        string EventType,
        string ReservationRef,
        string UserRef,
        string SubmittedTier,
        string PeriodKind,
        DateTimeOffset PeriodStartUtc,
        DateTimeOffset PeriodEndExclusiveUtc,
        decimal MonthlyLimitUsd,
        decimal RequestedReserveUsd,
        decimal ActualCostUsd,
        DateTimeOffset RecordedAtUtc,
        string DecisionStatus,
        string DenialReasonCode,
        IReadOnlyDictionary<string, string> Metadata);

    private sealed class JournalReplayState
    {
        public Dictionary<string, MutableReservationState> ReservationsByRef { get; } =
            new(StringComparer.Ordinal);

        public Dictionary<string, PeriodAuthorityState> PeriodAuthorities { get; } =
            new(StringComparer.Ordinal);

        public PeriodAuthorityState? TryGetPeriodAuthority(
            string userRef,
            DateTimeOffset periodStartUtc,
            DateTimeOffset periodEndExclusiveUtc)
            => PeriodAuthorities.TryGetValue(
                BuildPeriodAuthorityKey(userRef, periodStartUtc, periodEndExclusiveUtc),
                out var authority)
                ? authority
                : null;

        public void SetPeriodAuthority(BudgetJournalRecord record)
        {
            PeriodAuthorities[BuildPeriodAuthorityKey(
                record.UserRef,
                record.PeriodStartUtc,
                record.PeriodEndExclusiveUtc)] = new PeriodAuthorityState(
                record.SubmittedTier,
                record.MonthlyLimitUsd);
        }

        private static string BuildPeriodAuthorityKey(
            string userRef,
            DateTimeOffset periodStartUtc,
            DateTimeOffset periodEndExclusiveUtc)
            => string.Join("|",
            [
                userRef,
                periodStartUtc.ToString("O", CultureInfo.InvariantCulture),
                periodEndExclusiveUtc.ToString("O", CultureInfo.InvariantCulture)
            ]);
    }

    private sealed record PeriodAuthorityState(
        string SubmittedTier,
        decimal MonthlyLimitUsd);

    private sealed record MutableReservationState(
        string ReservationRef,
        string UserRef,
        string SubmittedTier,
        DateTimeOffset PeriodStartUtc,
        DateTimeOffset PeriodEndExclusiveUtc,
        decimal MonthlyLimitUsd,
        decimal RequestedReserveUsd,
        DateTimeOffset ReservedAtUtc,
        string DecisionStatus,
        string DenialReasonCode)
    {
        public decimal ActualCostUsd { get; set; }
        public DateTimeOffset? SettledAtUtc { get; set; }
        public DateTimeOffset? ReleasedAtUtc { get; set; }
    }
}
