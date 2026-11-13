// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using DesignConversionDomain;
using Microsoft.Extensions.Hosting;

public sealed record DesignConversionAccountingReconciliationStatus(
    bool Enabled,
    string Schedule,
    string Status,
    bool CompletedBatchResult,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? LastCompletionAtUtc,
    int ExaminedCount,
    int ReconciledCount,
    int FailedCount,
    int RemainingPendingCount,
    string LastErrorCode,
    string TerminalErrorCode,
    DateTimeOffset? NextAttemptAtUtc,
    bool TrainingEnabledByReconciliation,
    bool Degraded);

/// <summary>
/// Runs one finite accounting-reconciliation batch as part of production host
/// startup. It never schedules training and never runs outside the exact stores
/// already owned by the evaluate route runtime.
/// </summary>
public sealed class DesignConversionAccountingReconciliationHostedService : IHostedService
{
    public const int DefaultMaximumItems =
        DesignConversionThinClientJobRuntime
            .AccountingReconciliationDefaultMaximumItems;
    public const int MaximumAllowedItems =
        DesignConversionThinClientJobRuntime
            .AccountingReconciliationMaximumAllowedItems;
    public const string ScheduleName = "startup_one_shot";
    public const string ConfigurationInvalidErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_CONFIGURATION_INVALID";
    public const string OverlapRejectedErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_OVERLAP_REJECTED";
    public const string ProductionHostScope =
        "modelserver_design_conversion_live_local";
    public static readonly TimeSpan DefaultLeaseAcquireTimeout =
        TimeSpan.FromSeconds(2);
    public static readonly TimeSpan DefaultLeaseRetryDelay =
        TimeSpan.FromMilliseconds(25);
    public static readonly TimeSpan DefaultLateWorkShutdownTimeout =
        TimeSpan.FromSeconds(5);

    private readonly object _statusGate = new();
    private readonly SemaphoreSlim _runGate = new(1, 1);
    private readonly DesignConversionThinClientJobRuntime? _runtime;
    private readonly string? _postEvaluationStateRootDirectory;
    private readonly string? _visualCostBudgetStoreRootDirectory;
    private readonly int _maximumItems;
    private readonly Func<CancellationToken, Task>? _beforeBatchForTesting;
    private readonly TimeSpan _lateWorkShutdownTimeout;
    private DesignConversionAccountingReconciliationStatus _status;

    private DesignConversionAccountingReconciliationHostedService()
    {
        _lateWorkShutdownTimeout = DefaultLateWorkShutdownTimeout;
        _status = CreateStatus(
            enabled: false,
            status: "disabled_non_durable_route",
            degraded: false);
    }

    public DesignConversionAccountingReconciliationHostedService(
        DesignConversionThinClientJobRuntime runtime,
        string expectedPostEvaluationStateRootDirectory,
        string expectedVisualCostBudgetStoreRootDirectory,
        int maximumItems = DefaultMaximumItems)
        : this(
            runtime,
            expectedPostEvaluationStateRootDirectory,
            expectedVisualCostBudgetStoreRootDirectory,
            maximumItems,
            beforeBatchForTesting: null)
    {
    }

    internal DesignConversionAccountingReconciliationHostedService(
        DesignConversionThinClientJobRuntime runtime,
        string expectedPostEvaluationStateRootDirectory,
        string expectedVisualCostBudgetStoreRootDirectory,
        int maximumItems,
        Func<CancellationToken, Task>? beforeBatchForTesting,
        TimeSpan? lateWorkShutdownTimeoutForTesting = null,
        bool allowStoreTestDoublesForTesting = false)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        if (maximumItems is <= 0 or > MaximumAllowedItems)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumItems),
                $"Accounting reconciliation maximum items must be between 1 and {MaximumAllowedItems}.");
        }

        var expectedStateRoot = NormalizeRequiredRoot(
            expectedPostEvaluationStateRootDirectory,
            nameof(expectedPostEvaluationStateRootDirectory));
        var expectedBudgetRoot = NormalizeRequiredRoot(
            expectedVisualCostBudgetStoreRootDirectory,
            nameof(expectedVisualCostBudgetStoreRootDirectory));
        if (!allowStoreTestDoublesForTesting &&
            (runtime.PostEvaluationStateStore is not
                JsonDesignConversionPostEvaluationStateStore stateStore ||
             !PathsEqual(stateStore.RootDirectory, expectedStateRoot)))
        {
            throw new InvalidOperationException(
                ConfigurationInvalidErrorCode +
                ": the production reconciliation lifecycle is not bound to the exact configured post-evaluation state store.");
        }

        if (!allowStoreTestDoublesForTesting &&
            (runtime.VisualCostBudgetStore is not
                JsonlDesignConversionVisualCostBudgetStore budgetStore ||
             !PathsEqual(budgetStore.RootDirectory, expectedBudgetRoot)))
        {
            throw new InvalidOperationException(
                ConfigurationInvalidErrorCode +
                ": the production reconciliation lifecycle is not bound to the exact configured visual-cost budget store.");
        }

        _postEvaluationStateRootDirectory = expectedStateRoot;
        _visualCostBudgetStoreRootDirectory = expectedBudgetRoot;
        _maximumItems = maximumItems;
        _beforeBatchForTesting = beforeBatchForTesting;
        _lateWorkShutdownTimeout =
            lateWorkShutdownTimeoutForTesting ?? DefaultLateWorkShutdownTimeout;
        if (_lateWorkShutdownTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lateWorkShutdownTimeoutForTesting),
                "Accounting reconciliation late-work shutdown timeout must be positive.");
        }
        _status = CreateStatus(
            enabled: true,
            status: "pending_startup",
            degraded: false);
    }

    public static DesignConversionAccountingReconciliationHostedService Disabled()
        => new();

    public DesignConversionAccountingReconciliationStatus Status
    {
        get
        {
            lock (_statusGate)
                return _status;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_runtime is null)
            return;

        _ = await RunOnceAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_runtime is null)
            return;

        var drained = await _runtime
            .DrainAccountingReconciliationLateWorkAsync(
                _lateWorkShutdownTimeout,
                cancellationToken)
            .ConfigureAwait(false);
        if (!drained.IsSuccess)
        {
            var current = Status;
            UpdateFailure(
                "shutdown_late_work_incomplete",
                new DesignConversionAccountingReconciliationBatchResult(
                    Completed: false,
                    current.ExaminedCount,
                    current.ReconciledCount,
                    current.FailedCount,
                    current.RemainingPendingCount,
                    LastErrorCode: drained.ErrorCode,
                    TerminalErrorCode: drained.ErrorCode),
                degraded: true);
        }
    }

    public async Task<DesignConversionAccountingReconciliationBatchResult>
        RunOnceAsync(CancellationToken cancellationToken = default)
    {
        if (_runtime is null ||
            _postEvaluationStateRootDirectory is null ||
            _visualCostBudgetStoreRootDirectory is null)
        {
            return new DesignConversionAccountingReconciliationBatchResult(
                Completed: true,
                ExaminedCount: 0,
                ReconciledCount: 0,
                FailedCount: 0,
                RemainingPendingCount: 0,
                LastErrorCode: string.Empty,
                TerminalErrorCode: string.Empty);
        }

        bool entered;
        try
        {
            entered = await _runGate.WaitAsync(0, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            var cancelled = IncompleteResult("CANCELLED");
            UpdateFailure("cancelled", cancelled, degraded: true);
            return cancelled;
        }

        if (!entered)
        {
            return IncompleteResult(OverlapRejectedErrorCode);
        }

        try
        {
            var attemptAtUtc = DateTimeOffset.UtcNow;
            SetStatus(Status with
            {
                Status = "running",
                CompletedBatchResult = false,
                LastAttemptAtUtc = attemptAtUtc,
                LastCompletionAtUtc = null,
                ExaminedCount = 0,
                ReconciledCount = 0,
                FailedCount = 0,
                RemainingPendingCount = 0,
                LastErrorCode = string.Empty,
                TerminalErrorCode = string.Empty,
                NextAttemptAtUtc = null,
                TrainingEnabledByReconciliation = false,
                Degraded = false
            });

            var lease = await DesignConversionAccountingReconciliationLease
                .TryAcquireAsync(
                    _postEvaluationStateRootDirectory,
                    _visualCostBudgetStoreRootDirectory,
                    ProductionHostScope,
                    DefaultLeaseAcquireTimeout,
                    DefaultLeaseRetryDelay,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!lease.IsSuccess)
            {
                var leaseStatus = string.Equals(
                    lease.ErrorCode,
                    DesignConversionAccountingReconciliationLease.LeaseHeldErrorCode,
                    StringComparison.Ordinal)
                    ? "skipped_cross_process_lease_held"
                    : "failed_lease_unavailable";
                var leaseFailure = IncompleteResult(
                    lease.ErrorCode ??
                    DesignConversionAccountingReconciliationLease.LeaseUnavailableErrorCode);
                UpdateFailure(leaseStatus, leaseFailure, degraded: true);
                return leaseFailure;
            }

            using (lease.Value!)
            {
                if (_beforeBatchForTesting is not null)
                    await _beforeBatchForTesting(cancellationToken).ConfigureAwait(false);

                var result = await _runtime.ReconcilePendingAccountingBatchAsync(
                        _maximumItems,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (!result.IsSuccess)
                {
                    UpdateFailure(
                        string.Equals(
                            result.TerminalErrorCode,
                            "CANCELLED",
                            StringComparison.Ordinal)
                            ? "cancelled"
                            : "failed",
                        result,
                        degraded: true);
                    return result;
                }

                SetStatus(Status with
                {
                    Status = result.FailedCount > 0
                        ? "completed_with_item_failures"
                        : result.RemainingPendingCount > 0
                            ? "completed_batch_limit_reached"
                            : "completed",
                    CompletedBatchResult = true,
                    LastCompletionAtUtc = DateTimeOffset.UtcNow,
                    ExaminedCount = result.ExaminedCount,
                    ReconciledCount = result.ReconciledCount,
                    FailedCount = result.FailedCount,
                    RemainingPendingCount = result.RemainingPendingCount,
                    LastErrorCode = result.LastErrorCode,
                    TerminalErrorCode = string.Empty,
                    NextAttemptAtUtc = null,
                    TrainingEnabledByReconciliation = false,
                    Degraded =
                        result.FailedCount > 0 ||
                        result.RemainingPendingCount > 0
                });
                return result;
            }
        }
        finally
        {
            _runGate.Release();
        }
    }

    private void UpdateFailure(
        string status,
        DesignConversionAccountingReconciliationBatchResult result,
        bool degraded)
    {
        SetStatus(Status with
        {
            Status = status,
            CompletedBatchResult = false,
            LastCompletionAtUtc = DateTimeOffset.UtcNow,
            ExaminedCount = result.ExaminedCount,
            ReconciledCount = result.ReconciledCount,
            FailedCount = result.FailedCount,
            RemainingPendingCount = result.RemainingPendingCount,
            LastErrorCode = string.IsNullOrWhiteSpace(result.LastErrorCode)
                ? DesignConversionThinClientJobRuntime.AccountingReconciliationItemFailedErrorCode
                : result.LastErrorCode,
            TerminalErrorCode = result.TerminalErrorCode,
            NextAttemptAtUtc = null,
            TrainingEnabledByReconciliation = false,
            Degraded = degraded
        });
    }

    private static DesignConversionAccountingReconciliationBatchResult
        IncompleteResult(string errorCode)
        => new(
            Completed: false,
            ExaminedCount: 0,
            ReconciledCount: 0,
            FailedCount: 0,
            RemainingPendingCount: 0,
            LastErrorCode: errorCode,
            TerminalErrorCode: errorCode);

    private void SetStatus(DesignConversionAccountingReconciliationStatus status)
    {
        lock (_statusGate)
            _status = status;
    }

    private static DesignConversionAccountingReconciliationStatus CreateStatus(
        bool enabled,
        string status,
        bool degraded)
        => new(
            enabled,
            ScheduleName,
            status,
            CompletedBatchResult: false,
            LastAttemptAtUtc: null,
            LastCompletionAtUtc: null,
            ExaminedCount: 0,
            ReconciledCount: 0,
            FailedCount: 0,
            RemainingPendingCount: 0,
            LastErrorCode: string.Empty,
            TerminalErrorCode: string.Empty,
            NextAttemptAtUtc: null,
            TrainingEnabledByReconciliation: false,
            Degraded: degraded);

    private static string NormalizeRequiredRoot(string root, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(root) || !Path.IsPathFullyQualified(root))
        {
            throw new ArgumentException(
                ConfigurationInvalidErrorCode + ": a fully qualified durable root is required.",
                parameterName);
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
    }

    private static bool PathsEqual(string left, string right)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
}
