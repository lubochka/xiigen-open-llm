// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;
using System.Diagnostics;
using System.Text;

public sealed class DesignConversionAccountingReconciliationLifecycleTests
{
    private const string LeaseChildModeEnvironmentName =
        "XIIGEN_ACCOUNTING_RECONCILIATION_LEASE_CHILD";
    private const string LeaseChildStateRootEnvironmentName =
        "XIIGEN_ACCOUNTING_RECONCILIATION_LEASE_STATE_ROOT";
    private const string LeaseChildBudgetRootEnvironmentName =
        "XIIGEN_ACCOUNTING_RECONCILIATION_LEASE_BUDGET_ROOT";
    private const string LeaseChildReadyEnvironmentName =
        "XIIGEN_ACCOUNTING_RECONCILIATION_LEASE_READY";
    private const string LeaseChildReleaseEnvironmentName =
        "XIIGEN_ACCOUNTING_RECONCILIATION_LEASE_RELEASE";
    private const string LeaseChildResultEnvironmentName =
        "XIIGEN_ACCOUNTING_RECONCILIATION_LEASE_RESULT";

    [Fact]
    public async Task BatchBounds_RejectInvalidAcceptMaximumAndConvenienceIsFinite()
    {
        var root = NewRoot("finite-authority");
        var emptyStates = new MemoryStateStore([]);
        var budget = new SettledSnapshotBudgetStore([]);
        var emptyRuntime = CreateRuntime(root, budget, emptyStates);

        foreach (var invalid in new[]
                 {
                     -1,
                     0,
                     DesignConversionThinClientJobRuntime
                         .AccountingReconciliationMaximumAllowedItems + 1
                 })
        {
            var rejected =
                await emptyRuntime.ReconcilePendingAccountingBatchAsync(invalid);
            Assert.False(rejected.IsSuccess);
            Assert.False(rejected.Completed);
            Assert.Equal(
                DesignConversionThinClientJobRuntime
                    .AccountingReconciliationBatchSizeInvalidErrorCode,
                rejected.TerminalErrorCode);
            Assert.Equal(0, rejected.ExaminedCount);
        }

        var accepted =
            await emptyRuntime.ReconcilePendingAccountingBatchAsync(
                DesignConversionThinClientJobRuntime
                    .AccountingReconciliationMaximumAllowedItems);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.True(accepted.Completed);

        var pending = Enumerable.Range(
                0,
                DesignConversionThinClientJobRuntime
                    .AccountingReconciliationDefaultMaximumItems + 1)
            .Select(index => CreateMemoryPendingState($"finite-{index:D4}"))
            .ToArray();
        var finiteStates = new MemoryStateStore(pending);
        var finiteBudget = new SettledSnapshotBudgetStore(pending);
        var finiteRuntime = CreateRuntime(
            NewRoot("finite-convenience"),
            finiteBudget,
            finiteStates);

        var convenience =
            await finiteRuntime.ReconcilePendingAccountingAsync();

        Assert.True(convenience.IsSuccess, convenience.ErrorMessage);
        Assert.Equal(
            DesignConversionThinClientJobRuntime
                .AccountingReconciliationDefaultMaximumItems,
            convenience.Value);
        Assert.Equal(
            1,
            finiteStates.States.Count(state =>
                state.AccountingStatus ==
                JsonDesignConversionPostEvaluationStateStore
                    .AccountingReconciliationRequired));
        Assert.Equal(
            DesignConversionThinClientJobRuntime
                .AccountingReconciliationDefaultMaximumItems,
            finiteStates.TerminalWriteCount);
    }

    [Fact]
    public async Task Batch_MidBatchCancellationPublishesTruthfulDurablePartialOutcome()
    {
        var root = NewRoot("mid-batch-cancel");
        var budget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(
                root,
                "artifact-db",
                JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        await CreatePendingSettlementAsync(budget, states, "partial-a");
        await CreatePendingSettlementAsync(budget, states, "partial-b");
        using var cancellation = new CancellationTokenSource();
        var runtime = CreateRuntime(
            root,
            budget,
            states,
            (_, reconciledCount) =>
            {
                if (reconciledCount == 1)
                    cancellation.Cancel();
            });
        var service = new DesignConversionAccountingReconciliationHostedService(
            runtime,
            states.RootDirectory,
            budget.RootDirectory,
            maximumItems: 2);

        var result = await service.RunOnceAsync(cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.False(result.Completed);
        Assert.Equal("CANCELLED", result.TerminalErrorCode);
        Assert.Equal(1, result.ExaminedCount);
        Assert.Equal(1, result.ReconciledCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(1, result.RemainingPendingCount);
        Assert.False(service.Status.CompletedBatchResult);
        Assert.Equal("cancelled", service.Status.Status);
        Assert.Equal(1, service.Status.ExaminedCount);
        Assert.Equal(1, service.Status.ReconciledCount);
        Assert.Equal(0, service.Status.FailedCount);
        Assert.Equal(1, service.Status.RemainingPendingCount);
        Assert.Equal("CANCELLED", service.Status.TerminalErrorCode);
        Assert.False(service.Status.TrainingEnabledByReconciliation);

        var inventory = await states.ListAsync();
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        Assert.Equal(
            1,
            inventory.Value!.Count(state =>
                state.AccountingStatus ==
                JsonDesignConversionPostEvaluationStateStore.AccountingSettled));
        Assert.Equal(
            1,
            inventory.Value!.Count(state =>
                state.AccountingStatus ==
                JsonDesignConversionPostEvaluationStateStore
                    .AccountingReconciliationRequired));
    }

    [Fact]
    public async Task Batch_IsFiniteRestartIdempotentAndDoesNotEnableTraining()
    {
        var root = NewRoot("finite-restart");
        var budget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "artifact-db", JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        await CreatePendingSettlementAsync(budget, states, "finite-a");
        await CreatePendingSettlementAsync(budget, states, "finite-b");
        var runtime = CreateRuntime(root, budget, states);
        var service = new DesignConversionAccountingReconciliationHostedService(
            runtime,
            states.RootDirectory,
            budget.RootDirectory,
            maximumItems: 1);

        var first = await service.RunOnceAsync();
        var second = await service.RunOnceAsync();
        var replay = await service.RunOnceAsync();

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(1, first.ExaminedCount);
        Assert.Equal(1, first.ReconciledCount);
        Assert.Equal(1, first.RemainingPendingCount);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(1, second.ExaminedCount);
        Assert.Equal(1, second.ReconciledCount);
        Assert.Equal(0, second.RemainingPendingCount);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Equal(0, replay.ExaminedCount);
        Assert.Equal(0, replay.ReconciledCount);
        Assert.Equal(0, replay.RemainingPendingCount);
        Assert.False(service.Status.TrainingEnabledByReconciliation);
        Assert.Equal("completed", service.Status.Status);

        var inventory = await states.ListAsync();
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        Assert.Equal(
            2,
            inventory.Value!.Count(state =>
                state.AccountingStatus ==
                JsonDesignConversionPostEvaluationStateStore.AccountingSettled));
    }

    [Fact]
    public async Task Batch_CancellationLeavesPendingStateUntouched()
    {
        var root = NewRoot("cancel");
        var budget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "state"));
        var pending = await CreatePendingSettlementAsync(budget, states, "cancel-a");
        var runtime = CreateRuntime(root, budget, states);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await runtime.ReconcilePendingAccountingBatchAsync(
            maximumItems: 1,
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("CANCELLED", result.ErrorCode);
        var readback = await states.ReadAsync(pending.StateRef);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
            readback.Value!.AccountingStatus);
    }

    [Fact]
    public async Task Batch_ItemFailureStaysPendingAndLaterItemStillReconciles()
    {
        var root = NewRoot("continue-after-item-failure");
        var innerBudget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "state"));
        var first = await CreatePendingSettlementAsync(innerBudget, states, "failure-a");
        var second = await CreatePendingSettlementAsync(innerBudget, states, "failure-b");
        var failRef = new[] { first, second }
            .OrderBy(state => state.StateRef, StringComparer.Ordinal)
            .First()
            .ReservationRef;
        var budget = new SelectiveSettlementFailureBudgetStore(innerBudget, failRef);
        var runtime = CreateRuntime(root, budget, states);

        var result = await runtime.ReconcilePendingAccountingBatchAsync(2);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.ExaminedCount);
        Assert.Equal(1, result.ReconciledCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(1, result.RemainingPendingCount);
        var inventory = await states.ListAsync();
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        var failed = Assert.Single(
            inventory.Value!,
            state => state.ReservationRef == failRef);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
            failed.AccountingStatus);
        Assert.Equal(
            SelectiveSettlementFailureBudgetStore.FailureCode,
            failed.LastErrorCode);
        Assert.Single(
            inventory.Value!,
            state => state.AccountingStatus ==
                     JsonDesignConversionPostEvaluationStateStore.AccountingSettled);
    }

    [Fact]
    public async Task Batch_BudgetCorruptionIsFatalExactAndStopsLaterItems()
    {
        var root = NewRoot("budget-corruption");
        var innerBudget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "state"));
        var created = new[]
        {
            await CreatePendingSettlementAsync(
                innerBudget,
                states,
                "budget-corrupt-a"),
            await CreatePendingSettlementAsync(
                innerBudget,
                states,
                "budget-corrupt-b")
        };
        var ordered = created
            .OrderBy(state => state.StateRef, StringComparer.Ordinal)
            .ToArray();
        var budget = new SelectiveReadFailureBudgetStore(
            innerBudget,
            ordered[0].UserRef,
            JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode);
        var runtime = CreateRuntime(root, budget, states);

        var result = await runtime.ReconcilePendingAccountingBatchAsync(2);

        Assert.False(result.IsSuccess);
        Assert.False(result.Completed);
        Assert.Equal(
            JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode,
            result.TerminalErrorCode);
        Assert.Equal(1, result.ExaminedCount);
        Assert.Equal(0, result.ReconciledCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(2, result.RemainingPendingCount);
        Assert.Equal(1, budget.ReadCount);
        var inventory = await states.ListAsync();
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        Assert.All(
            inventory.Value!,
            state => Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore
                    .AccountingReconciliationRequired,
                state.AccountingStatus));
    }

    [Fact]
    public async Task Batch_MarkerWriteFailureAbortsBeforeLaterItemWithExactCode()
    {
        var root = NewRoot("marker-write-failure");
        var innerBudget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var innerStates = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "state"));
        var created = new[]
        {
            await CreatePendingSettlementAsync(
                innerBudget,
                innerStates,
                "marker-fail-a"),
            await CreatePendingSettlementAsync(
                innerBudget,
                innerStates,
                "marker-fail-b")
        };
        var ordered = created
            .OrderBy(state => state.StateRef, StringComparer.Ordinal)
            .ToArray();
        var budget = new SelectiveSettlementFailureBudgetStore(
            innerBudget,
            ordered[0].ReservationRef);
        var states = new SelectiveStateWriteFailureStore(
            innerStates,
            ordered[0].ReservationRef,
            state => string.Equals(
                state.LastErrorCode,
                SelectiveSettlementFailureBudgetStore.FailureCode,
                StringComparison.Ordinal),
            JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode);
        var runtime = CreateRuntime(root, budget, states);

        var result = await runtime.ReconcilePendingAccountingBatchAsync(2);

        Assert.False(result.IsSuccess);
        Assert.False(result.Completed);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode,
            result.TerminalErrorCode);
        Assert.Equal(
            SelectiveSettlementFailureBudgetStore.FailureCode,
            result.LastErrorCode);
        Assert.Equal(1, result.ExaminedCount);
        Assert.Equal(0, result.ReconciledCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(2, result.RemainingPendingCount);
        Assert.Equal(1, budget.SettlementAttemptCount);
        var inventory = await innerStates.ListAsync();
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        Assert.All(
            inventory.Value!,
            state => Assert.Equal(
                DesignConversionThinClientJobRuntime
                    .AccountingReconciliationRequiredErrorCode,
                state.LastErrorCode));
    }

    [Fact]
    public async Task HostedLifecycle_MarkerWriteFailurePublishesDistinctUnderlyingAndTerminalCodes()
    {
        var root = NewRoot("hosted-marker-write-failure");
        var innerBudget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "runtime-data", "budget"));
        var innerStates = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "artifact-db", "state"));
        var created = await CreatePendingSettlementAsync(
            innerBudget,
            innerStates,
            "hosted-marker-fail");
        var budget = new SelectiveSettlementFailureBudgetStore(
            innerBudget,
            created.ReservationRef);
        var states = new SelectiveStateWriteFailureStore(
            innerStates,
            created.ReservationRef,
            state => string.Equals(
                state.LastErrorCode,
                SelectiveSettlementFailureBudgetStore.FailureCode,
                StringComparison.Ordinal),
            JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode);
        var runtime = CreateRuntime(root, budget, states);
        var service = new DesignConversionAccountingReconciliationHostedService(
            runtime,
            innerStates.RootDirectory,
            innerBudget.RootDirectory,
            maximumItems: 1,
            beforeBatchForTesting: null,
            lateWorkShutdownTimeoutForTesting: null,
            allowStoreTestDoublesForTesting: true);

        var result = await service.RunOnceAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(
            SelectiveSettlementFailureBudgetStore.FailureCode,
            result.LastErrorCode);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode,
            result.TerminalErrorCode);
        Assert.Equal(
            SelectiveSettlementFailureBudgetStore.FailureCode,
            service.Status.LastErrorCode);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode,
            service.Status.TerminalErrorCode);
        Assert.NotEqual(
            service.Status.LastErrorCode,
            service.Status.TerminalErrorCode);
        Assert.True(service.Status.Degraded);
        Assert.False(service.Status.TrainingEnabledByReconciliation);
    }

    [Fact]
    public async Task Batch_TerminalStateTrustFailureAbortsBeforeLaterItem()
    {
        var root = NewRoot("terminal-state-trust-failure");
        var budget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var innerStates = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "state"));
        var created = new[]
        {
            await CreatePendingSettlementAsync(
                budget,
                innerStates,
                "terminal-trust-a"),
            await CreatePendingSettlementAsync(
                budget,
                innerStates,
                "terminal-trust-b")
        };
        var ordered = created
            .OrderBy(state => state.StateRef, StringComparer.Ordinal)
            .ToArray();
        var settled = await budget.SettleAsync(
            new DesignConversionVisualCostSettlementRequest(
                ordered[0].ReservationRef,
                ordered[0].UserRef,
                ordered[0].PeriodStartUtc,
                ordered[0].PeriodEndExclusiveUtc,
                ordered[0].ChargedCostUsd,
                DateTimeOffset.UtcNow));
        Assert.True(settled.IsSuccess, settled.ErrorMessage);
        var states = new SelectiveStateWriteFailureStore(
            innerStates,
            ordered[0].ReservationRef,
            state => state.AccountingStatus ==
                     JsonDesignConversionPostEvaluationStateStore
                         .AccountingSettled,
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode);
        var runtime = CreateRuntime(root, budget, states);

        var result = await runtime.ReconcilePendingAccountingBatchAsync(2);

        Assert.False(result.IsSuccess);
        Assert.False(result.Completed);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            result.TerminalErrorCode);
        Assert.Equal(1, result.ExaminedCount);
        Assert.Equal(0, result.ReconciledCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(2, result.RemainingPendingCount);
        Assert.Equal(1, states.FailedWriteCount);
        var inventory = await innerStates.ListAsync();
        Assert.True(inventory.IsSuccess, inventory.ErrorMessage);
        Assert.All(
            inventory.Value!,
            state => Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore
                    .AccountingReconciliationRequired,
                state.AccountingStatus));
    }

    [Fact]
    public async Task Lease_IsExclusiveWithinProcessAndRecoversAfterDispose()
    {
        var root = NewRoot("lease");
        var artifactRoot = Path.Combine(root, "artifact-db");
        var stateRoot = Path.Combine(
            artifactRoot,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        var budgetRoot = Path.Combine(root, "runtime-data", "budget");
        var first = await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
            stateRoot,
            budgetRoot,
            DesignConversionAccountingReconciliationHostedService.ProductionHostScope,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(10));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.False(IsSameOrDescendant(first.Value!.LeaseRootDirectory, artifactRoot));
        Assert.False(IsSameOrDescendant(first.Value.LeaseRootDirectory, stateRoot));
        Assert.False(IsSameOrDescendant(first.Value.LeaseRootDirectory, budgetRoot));
        Assert.DoesNotContain(
            artifactRoot,
            first.Value.LeaseRootDirectory,
            StringComparison.OrdinalIgnoreCase);
        try
        {
            var second = await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                stateRoot,
                budgetRoot,
                DesignConversionAccountingReconciliationHostedService.ProductionHostScope,
                TimeSpan.FromMilliseconds(75),
                TimeSpan.FromMilliseconds(10));
            Assert.False(second.IsSuccess);
            Assert.Equal(
                DesignConversionAccountingReconciliationLease.LeaseHeldErrorCode,
                second.ErrorCode);
        }
        finally
        {
            first.Value!.Dispose();
        }

        var recovered =
            await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                stateRoot,
                budgetRoot,
                DesignConversionAccountingReconciliationHostedService.ProductionHostScope,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(10));
        Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
        using var recoveredLease = recovered.Value!;
    }

    [Fact(Timeout = 120000)]
    public async Task Lease_IsExclusiveAcrossGenuineChildProcessAndRecoversAfterChildExit()
    {
        if (string.Equals(
                Environment.GetEnvironmentVariable(LeaseChildModeEnvironmentName),
                "1",
                StringComparison.Ordinal))
        {
            await RunLeaseHolderChildProcessAsync();
            return;
        }

        var root = NewRoot("lease-child-process");
        var stateRoot = Path.Combine(root, "artifact-db", "state");
        var budgetRoot = Path.Combine(root, "runtime-data", "budget");
        var readyPath = Path.Combine(root, "child-ready.txt");
        var releasePath = Path.Combine(root, "parent-release.txt");
        var resultPath = Path.Combine(root, "child-result.txt");
        Directory.CreateDirectory(root);

        using var process = StartLeaseHolderChildProcess(
            stateRoot,
            budgetRoot,
            readyPath,
            releasePath,
            resultPath);
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        try
        {
            await WaitForChildSignalAsync(
                process,
                readyPath,
                TimeSpan.FromSeconds(30),
                stdout,
                stderr);

            var blocked =
                await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                    stateRoot,
                    budgetRoot,
                    DesignConversionAccountingReconciliationHostedService
                        .ProductionHostScope,
                    TimeSpan.FromMilliseconds(150),
                    TimeSpan.FromMilliseconds(10));
            Assert.False(blocked.IsSuccess);
            Assert.Equal(
                DesignConversionAccountingReconciliationLease.LeaseHeldErrorCode,
                blocked.ErrorCode);

            await File.WriteAllTextAsync(
                releasePath,
                "release",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            var standardOutput = await stdout;
            var standardError = await stderr;
            Assert.True(
                process.HasExited,
                $"Lease child did not exit. stdout={standardOutput}; stderr={standardError}");
            Assert.Equal(0, process.ExitCode);
            Assert.Equal("released", await File.ReadAllTextAsync(resultPath));

            var recovered =
                await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                    stateRoot,
                    budgetRoot,
                    DesignConversionAccountingReconciliationHostedService
                        .ProductionHostScope,
                    TimeSpan.Zero,
                    TimeSpan.FromMilliseconds(10));
            Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
            using var recoveredLease = recovered.Value!;
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            }

            Assert.True(process.HasExited, "Lease-holder child process leaked.");
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Lease_PreCancelledAndBoundedHeldWaitAreStableAndScopeAliasesCollide()
    {
        var root = NewRoot("lease-bounded");
        var stateRoot = Path.Combine(root, "artifact-db", "state");
        var budgetRoot = Path.Combine(root, "runtime-data", "budget");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var preCancelled =
            await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                stateRoot,
                budgetRoot,
                "  MODELServer_Design_Conversion_Live_Local  ",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromMilliseconds(10),
                cancellation.Token);

        Assert.False(preCancelled.IsSuccess);
        Assert.Equal(
            DesignConversionAccountingReconciliationLease.CancelledErrorCode,
            preCancelled.ErrorCode);

        var first =
            await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                stateRoot,
                budgetRoot,
                "  MODELServer_Design_Conversion_Live_Local  ",
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(10));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        using (first.Value!)
        {
            var started = DateTimeOffset.UtcNow;
            var alias =
                await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                    stateRoot,
                    budgetRoot,
                    "modelserver_design_conversion_live_local",
                    TimeSpan.FromMilliseconds(80),
                    TimeSpan.FromMilliseconds(10));
            var elapsed = DateTimeOffset.UtcNow - started;

            Assert.False(alias.IsSuccess);
            Assert.Equal(
                DesignConversionAccountingReconciliationLease.LeaseHeldErrorCode,
                alias.ErrorCode);
            Assert.InRange(
                elapsed,
                TimeSpan.FromMilliseconds(60),
                TimeSpan.FromSeconds(2));
        }
    }

    [AccountingReconciliationReparseFact]
    public async Task Lease_ReparseRootFailsClosedWithoutWritingOutside()
    {
        var root = NewRoot("lease-reparse");
        var artifactRoot = Path.Combine(root, "artifact-db");
        var stateRoot = Path.Combine(artifactRoot, "state");
        var budgetRoot = Path.Combine(root, "runtime-data", "budget");
        var outsideRoot = Path.Combine(root, "outside");
        Directory.CreateDirectory(outsideRoot);
        var initial =
            await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                stateRoot,
                budgetRoot,
                DesignConversionAccountingReconciliationHostedService.ProductionHostScope,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(10));
        Assert.True(initial.IsSuccess, initial.ErrorMessage);
        var leaseRoot = initial.Value!.LeaseRootDirectory;
        initial.Value.Dispose();
        Directory.Delete(leaseRoot, recursive: true);
        Directory.CreateSymbolicLink(leaseRoot, outsideRoot);

        var denied =
            await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                stateRoot,
                budgetRoot,
                DesignConversionAccountingReconciliationHostedService.ProductionHostScope,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(10));

        Assert.False(denied.IsSuccess);
        Assert.Equal(
            DesignConversionAccountingReconciliationLease.LeaseUnavailableErrorCode,
            denied.ErrorCode);
        Assert.False(
            File.Exists(
                Path.Combine(
                    outsideRoot,
                    DesignConversionAccountingReconciliationLease.LeaseFileName)));
    }

    [Fact]
    public async Task HostedLifecycle_CorruptInventoryIsTruthfullyDegradedAndFailClosed()
    {
        var root = NewRoot("corrupt");
        var budget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "artifact-db", JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        Directory.CreateDirectory(states.RootDirectory);
        await File.WriteAllTextAsync(
            Path.Combine(states.RootDirectory, "unexpected-private-payload.txt"),
            "private-payload-must-not-surface");
        var runtime = CreateRuntime(root, budget, states);
        var service = new DesignConversionAccountingReconciliationHostedService(
            runtime,
            states.RootDirectory,
            budget.RootDirectory);

        var result = await service.RunOnceAsync();

        Assert.False(result.IsSuccess);
        Assert.True(service.Status.Degraded);
        Assert.Equal("failed", service.Status.Status);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode,
            service.Status.LastErrorCode);
        Assert.False(service.Status.TrainingEnabledByReconciliation);
        Assert.DoesNotContain("private-payload", service.Status.LastErrorCode, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HostedLifecycle_RejectsOverlappingInHostInvocation()
    {
        var root = NewRoot("in-host-overlap");
        var budget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "artifact-db", JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        await CreatePendingSettlementAsync(budget, states, "overlap-a");
        var runtime = CreateRuntime(root, budget, states);
        var entered = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new DesignConversionAccountingReconciliationHostedService(
            runtime,
            states.RootDirectory,
            budget.RootDirectory,
            maximumItems: 1,
            async cancellationToken =>
            {
                entered.TrySetResult(true);
                await release.Task.WaitAsync(cancellationToken);
            });

        var first = service.RunOnceAsync();
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var overlapping = await service.RunOnceAsync();
        release.TrySetResult(true);
        var completed = await first.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(overlapping.IsSuccess);
        Assert.Equal(
            DesignConversionAccountingReconciliationHostedService.OverlapRejectedErrorCode,
            overlapping.ErrorCode);
        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        Assert.Equal(1, completed.ReconciledCount);
    }

    [Fact]
    public async Task HostedLifecycle_StopRejectsWorkRegistrationRaceAndSucceedsOnlyAtVerifiedZero()
    {
        var root = NewRoot("stop-registration-race");
        var budget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "artifact-db", JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        var drainSnapshot = new TaskCompletionSource<int>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var allowDrainToReturn = new ManualResetEventSlim(false);
        var runtime = CreateRuntime(
            root,
            budget,
            states,
            afterAccountingReconciliationDrainSnapshotForTesting:
                count =>
                {
                    drainSnapshot.TrySetResult(count);
                    Assert.True(
                        allowDrainToReturn.Wait(TimeSpan.FromSeconds(5)),
                        "Test did not release the bounded drain snapshot hook.");
                });
        var service = new DesignConversionAccountingReconciliationHostedService(
            runtime,
            states.RootDirectory,
            budget.RootDirectory,
            maximumItems: 1,
            beforeBatchForTesting: null,
            lateWorkShutdownTimeoutForTesting: TimeSpan.FromSeconds(1));
        var candidate = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var stop = Task.Run(() => service.StopAsync(CancellationToken.None));
        Assert.Equal(
            0,
            await drainSnapshot.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.False(
            runtime.TryTrackAccountingReconciliationWorkForTesting(
                candidate.Task));
        Assert.Equal(0, runtime.AccountingReconciliationTrackedWorkCount);
        allowDrainToReturn.Set();
        await stop.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(0, runtime.AccountingReconciliationTrackedWorkCount);
        Assert.False(service.Status.Degraded);
        Assert.NotEqual(
            "shutdown_late_work_incomplete",
            service.Status.Status);
        candidate.TrySetResult(true);
    }

    [Fact]
    public async Task HostedLifecycle_StopReportsTimeoutAndCancellationTruthfullyForTrackedWork()
    {
        var timeoutRoot = NewRoot("stop-timeout");
        var timeoutBudget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(timeoutRoot, "budget"));
        var timeoutStates = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(
                timeoutRoot,
                "artifact-db",
                JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        var timeoutRuntime = CreateRuntime(
            timeoutRoot,
            timeoutBudget,
            timeoutStates);
        var timeoutService =
            new DesignConversionAccountingReconciliationHostedService(
                timeoutRuntime,
                timeoutStates.RootDirectory,
                timeoutBudget.RootDirectory,
                maximumItems: 1,
                beforeBatchForTesting: null,
                lateWorkShutdownTimeoutForTesting:
                    TimeSpan.FromMilliseconds(100));
        var timeoutWork = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.True(
            timeoutRuntime.TryTrackAccountingReconciliationWorkForTesting(
                timeoutWork.Task));

        await timeoutService.StopAsync(CancellationToken.None);

        Assert.True(timeoutService.Status.Degraded);
        Assert.Equal(
            "shutdown_late_work_incomplete",
            timeoutService.Status.Status);
        Assert.Equal(
            DesignConversionThinClientJobRuntime
                .AccountingReconciliationLateWorkDrainTimeoutErrorCode,
            timeoutService.Status.TerminalErrorCode);
        Assert.Equal(1, timeoutRuntime.AccountingReconciliationTrackedWorkCount);
        timeoutWork.TrySetResult(true);
        await WaitForTrackedWorkCountAsync(timeoutRuntime, expected: 0);
        var timeoutDrained =
            await timeoutRuntime.DrainAccountingReconciliationLateWorkAsync(
                TimeSpan.FromSeconds(1));
        Assert.True(timeoutDrained.IsSuccess, timeoutDrained.ErrorMessage);
        Assert.Equal(0, timeoutDrained.Value);

        var cancellationRoot = NewRoot("stop-cancelled");
        var cancellationBudget =
            new JsonlDesignConversionVisualCostBudgetStore(
                Path.Combine(cancellationRoot, "budget"));
        var cancellationStates =
            new JsonDesignConversionPostEvaluationStateStore(
                Path.Combine(
                    cancellationRoot,
                    "artifact-db",
                    JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        var cancellationRuntime = CreateRuntime(
            cancellationRoot,
            cancellationBudget,
            cancellationStates);
        var cancellationService =
            new DesignConversionAccountingReconciliationHostedService(
                cancellationRuntime,
                cancellationStates.RootDirectory,
                cancellationBudget.RootDirectory,
                maximumItems: 1,
                beforeBatchForTesting: null,
                lateWorkShutdownTimeoutForTesting: TimeSpan.FromSeconds(1));
        var cancelledWork = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.True(
            cancellationRuntime.TryTrackAccountingReconciliationWorkForTesting(
                cancelledWork.Task));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await cancellationService.StopAsync(cancellation.Token);

        Assert.True(cancellationService.Status.Degraded);
        Assert.Equal(
            "shutdown_late_work_incomplete",
            cancellationService.Status.Status);
        Assert.Equal(
            "CANCELLED",
            cancellationService.Status.TerminalErrorCode);
        Assert.Equal(
            1,
            cancellationRuntime.AccountingReconciliationTrackedWorkCount);
        cancelledWork.TrySetResult(true);
        await WaitForTrackedWorkCountAsync(cancellationRuntime, expected: 0);
    }

    [Fact]
    public void HostedLifecycle_RejectsAnyConfiguredStoreRootMismatchBeforeStartup()
    {
        var root = NewRoot("root-mismatch");
        var budget = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(root, "budget"));
        var states = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(root, "state"));
        var runtime = CreateRuntime(root, budget, states);

        var stateMismatch = Assert.Throws<InvalidOperationException>(
            () => new DesignConversionAccountingReconciliationHostedService(
                runtime,
                Path.Combine(root, "different-state"),
                budget.RootDirectory));
        var budgetMismatch = Assert.Throws<InvalidOperationException>(
            () => new DesignConversionAccountingReconciliationHostedService(
                runtime,
                states.RootDirectory,
                Path.Combine(root, "different-budget")));

        Assert.Contains(
            DesignConversionAccountingReconciliationHostedService.ConfigurationInvalidErrorCode,
            stateMismatch.Message,
            StringComparison.Ordinal);
        Assert.Contains(
            DesignConversionAccountingReconciliationHostedService.ConfigurationInvalidErrorCode,
            budgetMismatch.Message,
            StringComparison.Ordinal);
    }

    private static async Task RunLeaseHolderChildProcessAsync()
    {
        var stateRoot = RequiredChildEnvironment(
            LeaseChildStateRootEnvironmentName);
        var budgetRoot = RequiredChildEnvironment(
            LeaseChildBudgetRootEnvironmentName);
        var readyPath = RequiredChildEnvironment(
            LeaseChildReadyEnvironmentName);
        var releasePath = RequiredChildEnvironment(
            LeaseChildReleaseEnvironmentName);
        var resultPath = RequiredChildEnvironment(
            LeaseChildResultEnvironmentName);
        var acquired =
            await DesignConversionAccountingReconciliationLease.TryAcquireAsync(
                stateRoot,
                budgetRoot,
                DesignConversionAccountingReconciliationHostedService
                    .ProductionHostScope,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(10));
        Assert.True(acquired.IsSuccess, acquired.ErrorMessage);
        using (acquired.Value!)
        {
            await File.WriteAllTextAsync(
                readyPath,
                "held",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(30);
            while (!File.Exists(releasePath) && DateTimeOffset.UtcNow < deadline)
                await Task.Delay(25);
            Assert.True(
                File.Exists(releasePath),
                "Parent did not release the child-held reconciliation lease within 30 seconds.");
        }

        await File.WriteAllTextAsync(
            resultPath,
            "released",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static Process StartLeaseHolderChildProcess(
        string stateRoot,
        string budgetRoot,
        string readyPath,
        string releasePath,
        string resultPath)
    {
        var dotnetHost = ResolveDotnetHostForChildTest();
        var projectPath = FindTestProjectPath();
        var repositoryRoot = Directory.GetParent(
            Path.GetDirectoryName(projectPath)!)!.FullName;
        var startInfo = new ProcessStartInfo(dotnetHost)
        {
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add(projectPath);
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add(
            AppContext.BaseDirectory.Contains(
                $"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase)
                ? "Release"
                : "Debug");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add(
            "FullyQualifiedName~Lease_IsExclusiveAcrossGenuineChildProcessAndRecoversAfterChildExit");
        startInfo.ArgumentList.Add("--verbosity");
        startInfo.ArgumentList.Add("quiet");
        startInfo.Environment[LeaseChildModeEnvironmentName] = "1";
        startInfo.Environment[LeaseChildStateRootEnvironmentName] = stateRoot;
        startInfo.Environment[LeaseChildBudgetRootEnvironmentName] = budgetRoot;
        startInfo.Environment[LeaseChildReadyEnvironmentName] = readyPath;
        startInfo.Environment[LeaseChildReleaseEnvironmentName] = releasePath;
        startInfo.Environment[LeaseChildResultEnvironmentName] = resultPath;
        var process = Process.Start(startInfo);
        Assert.NotNull(process);
        return process;
    }

    private static string ResolveDotnetHostForChildTest()
    {
        // Сначала доверяем host, который передал сам dotnet/test runner:
        // это удерживает ребёнка в той же совместимой runtime-семье.
        foreach (var environmentName in new[] { "DOTNET_HOST_PATH", "DOTNET_EXE" })
        {
            var configuredHost = Environment.GetEnvironmentVariable(environmentName);
            if (!string.IsNullOrWhiteSpace(configuredHost) && File.Exists(configuredHost))
                return configuredHost;
        }

        // Дочерний процесс должен идти через тот же совместимый dotnet,
        // что и родительский прогон тестов. Жёсткий путь в профиль
        // пользователя может выбрать другой patch-runtime и упасть раньше,
        // чем тест доберётся до проверки межпроцессного lease.
        return OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
    }

    private static async Task WaitForChildSignalAsync(
        Process process,
        string readyPath,
        TimeSpan timeout,
        Task<string> stdout,
        Task<string> stderr)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    $"Lease-holder child exited before acquiring lease. Exit={process.ExitCode}; stdout={await stdout}; stderr={await stderr}");
            }

            if (File.Exists(readyPath))
            {
                try
                {
                    if (string.Equals(
                            "held",
                            await File.ReadAllTextAsync(readyPath),
                            StringComparison.Ordinal))
                    {
                        return;
                    }
                }
                catch (IOException)
                {
                    // File.Exists can become true before the child closes its
                    // asynchronous signal write. Retry within the same bound.
                }
            }

            await Task.Delay(25);
        }

        Assert.Fail(
            "Lease-holder child did not publish a complete acquisition signal within the bounded wait.");
    }

    private static string RequiredChildEnvironment(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        Assert.False(string.IsNullOrWhiteSpace(value), $"Missing child environment {name}.");
        return value!;
    }

    private static async Task WaitForTrackedWorkCountAsync(
        DesignConversionThinClientJobRuntime runtime,
        int expected)
    {
        var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (runtime.AccountingReconciliationTrackedWorkCount != expected &&
               DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.Equal(
            expected,
            runtime.AccountingReconciliationTrackedWorkCount);
    }

    private static string FindTestProjectPath()
    {
        foreach (var start in new[]
                 {
                     Directory.GetCurrentDirectory(),
                     AppContext.BaseDirectory
                 })
        {
            var current = new DirectoryInfo(start);
            while (current is not null)
            {
                var nested = Path.Combine(
                    current.FullName,
                    "Tests",
                    "LlmFromScratch.Tests.csproj");
                if (File.Exists(nested))
                    return nested;
                var direct = Path.Combine(
                    current.FullName,
                    "LlmFromScratch.Tests.csproj");
                if (File.Exists(direct))
                    return direct;
                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not locate Tests/LlmFromScratch.Tests.csproj.");
    }

    private static DesignConversionThinClientJobRuntime CreateRuntime(
        string root,
        IDesignConversionVisualCostBudgetStore budget,
        IDesignConversionPostEvaluationStateStore states,
        Action<DesignConversionPostEvaluationState, int>?
            afterAccountingReconciliationItemForTesting = null,
        Action<int>?
            afterAccountingReconciliationDrainSnapshotForTesting = null)
        => new DesignConversionThinClientJobRuntime(
            new NeverInvokedEvaluator(),
            utcNow: null,
            artifactDb: new JsonlDesignConversionLocalArtifactDb(
                Path.Combine(root, "artifact-db")),
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(
                Path.Combine(root, "ledger")),
            visualCostBudgetStore: budget,
            staleRunningTimeout: null,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout: null,
            postEvaluationStateStore: states,
            afterAccountingReconciliationItemForTesting:
                afterAccountingReconciliationItemForTesting,
            afterAccountingReconciliationDrainSnapshotForTesting:
                afterAccountingReconciliationDrainSnapshotForTesting);

    private static async Task<DesignConversionPostEvaluationState>
        CreatePendingSettlementAsync(
            JsonlDesignConversionVisualCostBudgetStore budget,
            JsonDesignConversionPostEvaluationStateStore states,
            string seed)
    {
        var userRef = JsonlDesignConversionVisualCostLedger
            .BuildPseudonymousUserRef($"reconciliation-user-{seed}")
            .Value!;
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(
            new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero));
        var reservationRef =
            JsonlDesignConversionVisualCostBudgetStore.BuildCanonicalReservationRef(seed);
        var reserved = await budget.TryReserveAsync(
            new DesignConversionVisualCostReservationRequest(
                reservationRef,
                userRef,
                "paid",
                period.StartUtc,
                period.EndExclusiveUtc,
                10m,
                0.10m,
                new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero)));
        Assert.True(reserved.IsSuccess, reserved.ErrorMessage);
        Assert.True(reserved.Value!.Reserved);

        var initial = new DesignConversionPostEvaluationState(
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(reservationRef),
            reservationRef,
            userRef,
            period.StartUtc,
            period.EndExclusiveUtc,
            0.08m,
            JsonDesignConversionPostEvaluationStateStore.AccountingPending,
            JsonDesignConversionPostEvaluationStateStore.DesiredSettle,
            SettlementAttempted: false,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            LastErrorCode: string.Empty,
            new DateTimeOffset(2026, 7, 15, 12, 0, 1, TimeSpan.Zero),
            Version: 1);
        var created = await states.WriteAsync(initial);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var pending = await states.WriteAsync(
            created.Value! with
            {
                AccountingStatus =
                    JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
                SettlementAttempted = true,
                LastErrorCode =
                    DesignConversionThinClientJobRuntime.AccountingReconciliationRequiredErrorCode,
                UpdatedAtUtc = initial.UpdatedAtUtc.AddSeconds(1)
            });
        Assert.True(pending.IsSuccess, pending.ErrorMessage);
        return pending.Value!;
    }

    private static DesignConversionPostEvaluationState CreateMemoryPendingState(
        string seed)
    {
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(
            new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero));
        var reservationRef =
            JsonlDesignConversionVisualCostBudgetStore.BuildCanonicalReservationRef(seed);
        return new DesignConversionPostEvaluationState(
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(reservationRef),
            reservationRef,
            JsonlDesignConversionVisualCostLedger
                .BuildPseudonymousUserRef($"memory-user-{seed}")
                .Value!,
            period.StartUtc,
            period.EndExclusiveUtc,
            0.08m,
            JsonDesignConversionPostEvaluationStateStore
                .AccountingReconciliationRequired,
            JsonDesignConversionPostEvaluationStateStore.DesiredSettle,
            SettlementAttempted: true,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            DesignConversionThinClientJobRuntime
                .AccountingReconciliationRequiredErrorCode,
            new DateTimeOffset(2026, 7, 15, 12, 0, 1, TimeSpan.Zero),
            Version: 1);
    }

    private static string NewRoot(string scenario)
        => Path.Combine(
            Path.GetTempPath(),
            "design-conversion-accounting-reconciliation-tests",
            scenario,
            Guid.NewGuid().ToString("N"));

    private static bool IsSameOrDescendant(string candidate, string root)
    {
        var relative = Path.GetRelativePath(root, candidate);
        return !Path.IsPathRooted(relative) &&
               relative != ".." &&
               !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
               !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);
    }

    private sealed class NeverInvokedEvaluator : IDesignConversionThinClientJobEvaluator
    {
        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(
                "Accounting reconciliation must not invoke evaluation or training.");
    }

    private sealed class SelectiveSettlementFailureBudgetStore(
        JsonlDesignConversionVisualCostBudgetStore inner,
        string failedReservationRef) : IDesignConversionVisualCostBudgetStore
    {
        public const string FailureCode = "SCRIPTED_RECONCILIATION_ITEM_FAILURE";
        public int SettlementAttemptCount { get; private set; }

        public Task<OperationResult<DesignConversionVisualCostReservationResult>> TryReserveAsync(
            DesignConversionVisualCostReservationRequest request,
            CancellationToken cancellationToken = default)
            => inner.TryReserveAsync(request, cancellationToken);

        public Task<OperationResult<DesignConversionVisualCostSettlementResult>> SettleAsync(
            DesignConversionVisualCostSettlementRequest request,
            CancellationToken cancellationToken = default)
        {
            SettlementAttemptCount++;
            return request.ReservationRef == failedReservationRef
                ? Task.FromResult(
                    OperationResult<DesignConversionVisualCostSettlementResult>.Fail(
                        "scripted-item-failure",
                        FailureCode))
                : inner.SettleAsync(request, cancellationToken);
        }

        public Task<OperationResult<DesignConversionVisualCostReleaseResult>> ReleaseAsync(
            DesignConversionVisualCostReleaseRequest request,
            CancellationToken cancellationToken = default)
            => inner.ReleaseAsync(request, cancellationToken);

        public Task<OperationResult<DesignConversionVisualCostBudgetPeriodState>> ReadPeriodAsync(
            DesignConversionVisualCostBudgetPeriodQuery query,
            CancellationToken cancellationToken = default)
            => inner.ReadPeriodAsync(query, cancellationToken);
    }

    private sealed class SelectiveReadFailureBudgetStore(
        JsonlDesignConversionVisualCostBudgetStore inner,
        string failedUserRef,
        string failureCode) : IDesignConversionVisualCostBudgetStore
    {
        public int ReadCount { get; private set; }

        public Task<OperationResult<DesignConversionVisualCostReservationResult>>
            TryReserveAsync(
                DesignConversionVisualCostReservationRequest request,
                CancellationToken cancellationToken = default)
            => inner.TryReserveAsync(request, cancellationToken);

        public Task<OperationResult<DesignConversionVisualCostSettlementResult>>
            SettleAsync(
                DesignConversionVisualCostSettlementRequest request,
                CancellationToken cancellationToken = default)
            => inner.SettleAsync(request, cancellationToken);

        public Task<OperationResult<DesignConversionVisualCostReleaseResult>>
            ReleaseAsync(
                DesignConversionVisualCostReleaseRequest request,
                CancellationToken cancellationToken = default)
            => inner.ReleaseAsync(request, cancellationToken);

        public Task<OperationResult<DesignConversionVisualCostBudgetPeriodState>>
            ReadPeriodAsync(
                DesignConversionVisualCostBudgetPeriodQuery query,
                CancellationToken cancellationToken = default)
        {
            ReadCount++;
            return string.Equals(
                query.UserRef,
                failedUserRef,
                StringComparison.Ordinal)
                ? Task.FromResult(
                    OperationResult<DesignConversionVisualCostBudgetPeriodState>
                        .Fail("scripted-budget-corruption", failureCode))
                : inner.ReadPeriodAsync(query, cancellationToken);
        }
    }

    private sealed class SelectiveStateWriteFailureStore(
        JsonDesignConversionPostEvaluationStateStore inner,
        string failedReservationRef,
        Func<DesignConversionPostEvaluationState, bool> shouldFail,
        string failureCode) : IDesignConversionPostEvaluationStateStore
    {
        public int FailedWriteCount { get; private set; }

        public Task<OperationResult<DesignConversionPostEvaluationState>> WriteAsync(
            DesignConversionPostEvaluationState state,
            CancellationToken cancellationToken = default)
        {
            if (string.Equals(
                    state.ReservationRef,
                    failedReservationRef,
                    StringComparison.Ordinal) &&
                shouldFail(state))
            {
                FailedWriteCount++;
                return Task.FromResult(
                    OperationResult<DesignConversionPostEvaluationState>.Fail(
                        "scripted-state-write-failure",
                        failureCode));
            }

            return inner.WriteAsync(state, cancellationToken);
        }

        public Task<OperationResult<DesignConversionPostEvaluationState?>> ReadAsync(
            string stateRef,
            CancellationToken cancellationToken = default)
            => inner.ReadAsync(stateRef, cancellationToken);

        public Task<OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>>
            ListAsync(CancellationToken cancellationToken = default)
            => inner.ListAsync(cancellationToken);
    }

    private sealed class MemoryStateStore(
        IEnumerable<DesignConversionPostEvaluationState> states)
        : IDesignConversionPostEvaluationStateStore
    {
        private readonly Dictionary<string, DesignConversionPostEvaluationState>
            _states = states.ToDictionary(
                state => state.StateRef,
                state => state,
                StringComparer.Ordinal);

        public IReadOnlyList<DesignConversionPostEvaluationState> States =>
            _states.Values.ToArray();

        public int TerminalWriteCount { get; private set; }

        public Task<OperationResult<DesignConversionPostEvaluationState>> WriteAsync(
            DesignConversionPostEvaluationState state,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(
                    OperationResult<DesignConversionPostEvaluationState>.Fail(
                        "cancelled",
                        "CANCELLED"));
            }

            var updated = state with { Version = state.Version + 1 };
            _states[state.StateRef] = updated;
            if (updated.AccountingStatus is
                JsonDesignConversionPostEvaluationStateStore.AccountingSettled or
                JsonDesignConversionPostEvaluationStateStore.AccountingReleased)
            {
                TerminalWriteCount++;
            }

            return Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState>.Ok(updated));
        }

        public Task<OperationResult<DesignConversionPostEvaluationState?>> ReadAsync(
            string stateRef,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState?>.Ok(
                    _states.TryGetValue(stateRef, out var state)
                        ? state
                        : null));

        public Task<OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>>
            ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>.Ok(
                    _states.Values
                        .OrderBy(state => state.StateRef, StringComparer.Ordinal)
                        .ToArray()));
    }

    private sealed class SettledSnapshotBudgetStore(
        IEnumerable<DesignConversionPostEvaluationState> states)
        : IDesignConversionVisualCostBudgetStore
    {
        private readonly DesignConversionPostEvaluationState[] _states =
            states.ToArray();

        public Task<OperationResult<DesignConversionVisualCostReservationResult>>
            TryReserveAsync(
                DesignConversionVisualCostReservationRequest request,
                CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Reserve must not be called.");

        public Task<OperationResult<DesignConversionVisualCostSettlementResult>>
            SettleAsync(
                DesignConversionVisualCostSettlementRequest request,
                CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(
                "Settled snapshots must not be settled again.");

        public Task<OperationResult<DesignConversionVisualCostReleaseResult>>
            ReleaseAsync(
                DesignConversionVisualCostReleaseRequest request,
                CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Release must not be called.");

        public Task<OperationResult<DesignConversionVisualCostBudgetPeriodState>>
            ReadPeriodAsync(
                DesignConversionVisualCostBudgetPeriodQuery query,
                CancellationToken cancellationToken = default)
        {
            var matches = _states
                .Where(state =>
                    state.UserRef == query.UserRef &&
                    state.PeriodStartUtc == query.PeriodStartUtc &&
                    state.PeriodEndExclusiveUtc == query.PeriodEndExclusiveUtc)
                .Select(state => new DesignConversionVisualCostReservationSnapshot(
                    state.ReservationRef,
                    JsonlDesignConversionVisualCostBudgetStore.StatusSettled,
                    state.UserRef,
                    "reconciliation",
                    state.PeriodStartUtc,
                    state.PeriodEndExclusiveUtc,
                    1m,
                    0.1m,
                    state.ChargedCostUsd,
                    state.UpdatedAtUtc,
                    state.UpdatedAtUtc,
                    ReleasedAtUtc: null,
                    JsonlDesignConversionVisualCostBudgetStore.NoDenialReason))
                .ToArray();
            return Task.FromResult(
                OperationResult<DesignConversionVisualCostBudgetPeriodState>.Ok(
                    new DesignConversionVisualCostBudgetPeriodState(
                        query.UserRef,
                        JsonlDesignConversionVisualCostLedger
                            .UtcCalendarMonthPeriodKind,
                        query.PeriodStartUtc,
                        query.PeriodEndExclusiveUtc,
                        1m,
                        matches.Sum(match => match.ActualCostUsd),
                        ActiveReservedUsd: 0m,
                        RemainingBudgetUsd: 1m,
                        matches.Length,
                        ActiveReservationCount: 0,
                        SettledReservationCount: matches.Length,
                        ReleasedReservationCount: 0,
                        DeniedReservationCount: 0,
                        matches)));
        }
    }
}

file sealed class AccountingReconciliationReparseFactAttribute : FactAttribute
{
    public AccountingReconciliationReparseFactAttribute()
        => Skip =
            AccountingReconciliationReparseTestSupport.GetCapabilitySkipReason();
}

file static class AccountingReconciliationReparseTestSupport
{
    private const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);

    public static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows())
        {
            return "REPARSE_TEST_PLATFORM_LIMITATION: handle-bound lease storage is Windows-specific.";
        }

        var root = Path.Combine(
            Path.GetTempPath(),
            $"accounting-reconciliation-reparse-capability-{Guid.NewGuid():N}");
        var target = Path.Combine(root, "target");
        var link = Path.Combine(root, "link");
        try
        {
            Directory.CreateDirectory(target);
            try
            {
                Directory.CreateSymbolicLink(link, target);
                return null;
            }
            catch (Exception exception)
                when ((exception is IOException or UnauthorizedAccessException) &&
                      exception.HResult == HResultPrivilegeNotHeld)
            {
                return "REPARSE_TEST_PLATFORM_LIMITATION: " +
                       $"{exception.GetType().FullName}, HResult=0x{exception.HResult:X8}: " +
                       "creating symbolic links requires SeCreateSymbolicLinkPrivilege.";
            }
        }
        finally
        {
            try
            {
                if (Directory.Exists(link))
                    Directory.Delete(link);
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch
            {
                // Capability probe cleanup cannot alter the product verdict.
            }
        }
    }
}
