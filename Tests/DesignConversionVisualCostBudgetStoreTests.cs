// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;
using AgentInfrastructure;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

public sealed class DesignConversionVisualCostBudgetStoreTests
{
    private const string CurrentBudgetAtomicityScope = "single_process_per_path_lock_in_place_append_stable_intent";
    private const string LegacyBudgetAtomicityScope = "single_process_per_path_lock_copy_on_write";

    public static IEnumerable<object[]> InvalidUtf8JournalOperationCases()
    {
        yield return new object[] { "ReadPeriodAsync" };
        yield return new object[] { "TryReserveAsync" };
        yield return new object[] { "SettleAsync" };
        yield return new object[] { "ReleaseAsync" };
    }

    [Fact]
    public async Task TryReserveAsync_ReservesUnderCapDeniesOverCapAndReplaysWithoutDoubleCounting()
    {
        var root = NewRoot("visual-cost-budget-reserve-replay");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("reserve-replay-user");
        var period = Period();

        var accepted = await store.TryReserveAsync(Reserve("reserve-a", userRef, period, 0.05m, 0.03m));
        var denied = await store.TryReserveAsync(Reserve("reserve-b", userRef, period, 0.05m, 0.03m));
        var replay = await store.TryReserveAsync(Reserve("reserve-a", userRef, period, 0.05m, 0.03m));
        var readback = await store.ReadPeriodAsync(new DesignConversionVisualCostBudgetPeriodQuery(
            userRef,
            period.StartUtc,
            period.EndExclusiveUtc));

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.True(accepted.Value!.Reserved);
        Assert.False(accepted.Value.Replayed);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusReserved, accepted.Value.Status);
        Assert.Equal(0.03m, accepted.Value.ActiveReservedUsd);
        Assert.Equal(0.02m, accepted.Value.RemainingBudgetUsd);

        Assert.True(denied.IsSuccess, denied.ErrorMessage);
        Assert.False(denied.Value!.Reserved);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusDenied, denied.Value.Status);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded, denied.Value.DenialReasonCode);
        Assert.Equal(0.03m, denied.Value.ActiveReservedUsd);
        Assert.Equal(0.02m, denied.Value.RemainingBudgetUsd);

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.Reserved);
        Assert.True(replay.Value.Replayed);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusReplayed, replay.Value.Status);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(0.03m, readback.Value!.ActiveReservedUsd);
        Assert.Equal(0m, readback.Value.FinalizedSpendUsd);
        Assert.Equal(2, readback.Value.ReservationCount);
        Assert.Equal(1, readback.Value.ActiveReservationCount);
        Assert.Equal(1, readback.Value.DeniedReservationCount);
        Assert.Equal(2, File.ReadAllLines(store.RecordsPath).Length);
    }

    [Fact]
    public async Task TryReserveAsync_ExactDeniedReplayReturnsPriorDecisionWithoutAppending()
    {
        var root = NewRoot("visual-cost-budget-denied-replay");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("denied-replay-user");
        var period = Period();

        var accepted = await store.TryReserveAsync(Reserve("denied-replay-a", userRef, period, 0.05m, 0.03m));
        var denied = await store.TryReserveAsync(Reserve("denied-replay-b", userRef, period, 0.05m, 0.03m));
        var linesBefore = File.ReadAllLines(store.RecordsPath).Length;
        var replay = await store.TryReserveAsync(Reserve("denied-replay-b", userRef, period, 0.05m, 0.03m));
        var readback = await store.ReadPeriodAsync(Query(userRef, period));

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.True(denied.IsSuccess, denied.ErrorMessage);
        Assert.False(denied.Value!.Reserved);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.False(replay.Value!.Reserved);
        Assert.True(replay.Value.Replayed);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusReplayed, replay.Value.Status);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded, replay.Value.DenialReasonCode);
        Assert.Equal(linesBefore, File.ReadAllLines(store.RecordsPath).Length);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(0.03m, readback.Value!.ActiveReservedUsd);
        Assert.Equal(1, readback.Value.DeniedReservationCount);
    }

    [Fact]
    public async Task TryReserveAsync_NewRefCannotChangeEstablishedPeriodTierOrMonthlyLimit()
    {
        var root = NewRoot("visual-cost-budget-period-authority");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("period-authority-user");
        var period = Period();

        var first = await store.TryReserveAsync(Reserve("period-authority-a", userRef, period, 0.05m, 0.01m, submittedTier: "paid"));
        var raisedCap = await store.TryReserveAsync(Reserve("period-authority-b", userRef, period, 1.00m, 0.01m, submittedTier: "paid"));
        var changedTier = await store.TryReserveAsync(Reserve("period-authority-c", userRef, period, 0.05m, 0.01m, submittedTier: "pro"));
        var readback = await store.ReadPeriodAsync(Query(userRef, period));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(first.Value!.Reserved);
        Assert.False(raisedCap.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.PeriodAuthorityConflictErrorCode, raisedCap.ErrorCode);
        Assert.False(changedTier.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.PeriodAuthorityConflictErrorCode, changedTier.ErrorCode);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(0.05m, readback.Value!.ObservedMonthlyLimitUsd);
        Assert.Equal(1, readback.Value.ReservationCount);
        Assert.Single(File.ReadAllLines(store.RecordsPath));
    }

    [Fact]
    public async Task TryReserveAsync_IncludesSettledSpendAndActiveReservationsBeforeAdmittingMoreBudget()
    {
        var root = NewRoot("visual-cost-budget-spend-active");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("spend-active-user");
        var period = Period();

        var first = await store.TryReserveAsync(Reserve("spend-active-a", userRef, period, 0.05m, 0.04m));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var settled = await store.SettleAsync(Settle("spend-active-a", userRef, period, 0.03m));
        Assert.True(settled.IsSuccess, settled.ErrorMessage);
        var active = await store.TryReserveAsync(Reserve("spend-active-b", userRef, period, 0.05m, 0.02m));
        var denied = await store.TryReserveAsync(Reserve("spend-active-c", userRef, period, 0.05m, 0.01m));
        var readback = await store.ReadPeriodAsync(Query(userRef, period));

        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.True(active.Value!.Reserved);
        Assert.True(denied.IsSuccess, denied.ErrorMessage);
        Assert.False(denied.Value!.Reserved);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(0.03m, readback.Value!.FinalizedSpendUsd);
        Assert.Equal(0.02m, readback.Value.ActiveReservedUsd);
        Assert.Equal(0m, readback.Value.RemainingBudgetUsd);
        Assert.Equal(1, readback.Value.SettledReservationCount);
        Assert.Equal(1, readback.Value.ActiveReservationCount);
        Assert.Equal(1, readback.Value.DeniedReservationCount);
    }

    [Fact]
    public async Task TryReserveAsync_ConflictingReservationRefReuseFailsClosedWithoutAppending()
    {
        var root = NewRoot("visual-cost-budget-idempotency-conflict");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("idempotency-conflict-user");
        var otherUserRef = UserRef("idempotency-conflict-other-user");
        var period = Period();

        var first = await store.TryReserveAsync(Reserve("idempotency-conflict", userRef, period, 0.10m, 0.02m));
        var changedAmount = await store.TryReserveAsync(Reserve("idempotency-conflict", userRef, period, 0.10m, 0.03m));
        var changedUser = await store.TryReserveAsync(Reserve("idempotency-conflict", otherUserRef, period, 0.10m, 0.02m));
        var readback = await store.ReadPeriodAsync(Query(userRef, period));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.False(changedAmount.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.IdempotencyConflictErrorCode, changedAmount.ErrorCode);
        Assert.False(changedUser.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.IdempotencyConflictErrorCode, changedUser.ErrorCode);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(1, readback.Value!.ReservationCount);
        Assert.Equal(0.02m, readback.Value.ActiveReservedUsd);
        Assert.Single(File.ReadAllLines(store.RecordsPath));
    }

    [Fact]
    public async Task SettleAsync_IsIdempotentAndRejectsChangedSettlementOrOverspend()
    {
        var root = NewRoot("visual-cost-budget-settle");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("settle-user");
        var period = Period();

        var firstReserve = await store.TryReserveAsync(Reserve("settle-a", userRef, period, 0.10m, 0.04m));
        Assert.True(firstReserve.IsSuccess, firstReserve.ErrorMessage);
        var settled = await store.SettleAsync(Settle("settle-a", userRef, period, 0.03m));
        var replay = await store.SettleAsync(Settle("settle-a", userRef, period, 0.03m));
        var changedSettlement = await store.SettleAsync(Settle("settle-a", userRef, period, 0.02m));
        var secondReserve = await store.TryReserveAsync(Reserve("settle-b", userRef, period, 0.10m, 0.02m));
        Assert.True(secondReserve.IsSuccess, secondReserve.ErrorMessage);
        var overspend = await store.SettleAsync(Settle("settle-b", userRef, period, 0.03m));
        var readback = await store.ReadPeriodAsync(Query(userRef, period));

        Assert.True(settled.IsSuccess, settled.ErrorMessage);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusSettled, settled.Value!.Status);
        Assert.False(settled.Value.Replayed);
        Assert.Equal(0.03m, settled.Value.FinalizedSpendUsd);
        Assert.Equal(0m, settled.Value.ActiveReservedUsd);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.Replayed);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusReplayed, replay.Value.Status);
        Assert.False(changedSettlement.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.SettlementConflictErrorCode, changedSettlement.ErrorCode);
        Assert.False(overspend.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.SettlementExceedsReservationErrorCode, overspend.ErrorCode);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(0.03m, readback.Value!.FinalizedSpendUsd);
        Assert.Equal(0.02m, readback.Value.ActiveReservedUsd);
        Assert.Equal(2, readback.Value.ReservationCount);
        Assert.Equal(3, File.ReadAllLines(store.RecordsPath).Length);
    }

    [Fact]
    public async Task ReleaseAsync_IsIdempotentFreesActiveBudgetAndRejectsSettledOrReleasedTransitions()
    {
        var root = NewRoot("visual-cost-budget-release");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("release-user");
        var period = Period();

        var firstReserve = await store.TryReserveAsync(Reserve("release-a", userRef, period, 0.10m, 0.04m));
        Assert.True(firstReserve.IsSuccess, firstReserve.ErrorMessage);
        var released = await store.ReleaseAsync(Release("release-a", userRef, period));
        var replay = await store.ReleaseAsync(Release("release-a", userRef, period));
        var settleAfterRelease = await store.SettleAsync(Settle("release-a", userRef, period, 0.01m));
        var secondReserve = await store.TryReserveAsync(Reserve("release-b", userRef, period, 0.10m, 0.03m));
        Assert.True(secondReserve.IsSuccess, secondReserve.ErrorMessage);
        var settled = await store.SettleAsync(Settle("release-b", userRef, period, 0.02m));
        Assert.True(settled.IsSuccess, settled.ErrorMessage);
        var releaseAfterSettle = await store.ReleaseAsync(Release("release-b", userRef, period));
        var readback = await store.ReadPeriodAsync(Query(userRef, period));

        Assert.True(released.IsSuccess, released.ErrorMessage);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusReleased, released.Value!.Status);
        Assert.False(released.Value.Replayed);
        Assert.Equal(0m, released.Value.ActiveReservedUsd);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.Replayed);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusReplayed, replay.Value.Status);
        Assert.False(settleAfterRelease.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.InvalidTransitionErrorCode, settleAfterRelease.ErrorCode);
        Assert.False(releaseAfterSettle.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.InvalidTransitionErrorCode, releaseAfterSettle.ErrorCode);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(0.02m, readback.Value!.FinalizedSpendUsd);
        Assert.Equal(0m, readback.Value.ActiveReservedUsd);
        Assert.Equal(1, readback.Value.ReleasedReservationCount);
        Assert.Equal(1, readback.Value.SettledReservationCount);
    }

    [Fact]
    public async Task ReadPeriodAsync_FreshInstanceReconstructsDurableState()
    {
        var root = NewRoot("visual-cost-budget-fresh-instance");
        var userRef = UserRef("fresh-instance-user");
        var period = Period();
        var writer = new JsonlDesignConversionVisualCostBudgetStore(root);
        var reserve = await writer.TryReserveAsync(Reserve("fresh-instance-a", userRef, period, 0.10m, 0.05m));
        Assert.True(reserve.IsSuccess, reserve.ErrorMessage);
        var settle = await writer.SettleAsync(Settle("fresh-instance-a", userRef, period, 0.04m));
        Assert.True(settle.IsSuccess, settle.ErrorMessage);

        var reader = new JsonlDesignConversionVisualCostBudgetStore(root);
        var readback = await reader.ReadPeriodAsync(Query(userRef, period));

        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(1, readback.Value!.ReservationCount);
        Assert.Equal(1, readback.Value.SettledReservationCount);
        Assert.Equal(0.04m, readback.Value.FinalizedSpendUsd);
        Assert.Equal(0m, readback.Value.ActiveReservedUsd);
        var snapshot = Assert.Single(readback.Value.Reservations);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusSettled, snapshot.Status);
        Assert.Equal(0.05m, snapshot.RequestedReserveUsd);
        Assert.Equal(0.04m, snapshot.ActualCostUsd);
    }

    [Fact]
    public async Task MalformedPersistedRecordFailsClosedForReadAndMutation()
    {
        var root = NewRoot("visual-cost-budget-corrupt");
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(
            Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName),
            "{ not-json");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("corrupt-user");
        var period = Period();

        var read = await store.ReadPeriodAsync(Query(userRef, period));
        var reserve = await store.TryReserveAsync(Reserve("corrupt-reserve", userRef, period, 0.10m, 0.01m));

        Assert.False(read.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, read.ErrorCode);
        Assert.False(reserve.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, reserve.ErrorCode);
        Assert.Single(File.ReadAllLines(store.RecordsPath));
    }

    [Theory]
    [MemberData(nameof(InvalidUtf8JournalOperationCases))]
    public async Task InvalidUtf8PersistedJournalBytesFailClosedForPublicReadsAndMutations(string operation)
    {
        const string privateMarker = "invalid-utf8-private-budget-marker";
        var root = NewRoot($"visual-cost-budget-invalid-utf8-{operation}");
        Directory.CreateDirectory(root);
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        await File.WriteAllBytesAsync(recordsPath, BuildInvalidUtf8JournalBytes(privateMarker));
        var bytesBefore = await File.ReadAllBytesAsync(recordsPath);
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("invalid-utf8-user");
        var period = Period();
        var expectedSafeReadMessage = await ExistingReadFailedMessageAsync(operation, userRef, period);

        var outcome = await InvokeBudgetOperationAsync(store, operation, userRef, period);
        var bytesAfter = await File.ReadAllBytesAsync(recordsPath);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, outcome.ErrorCode);
        Assert.Equal(expectedSafeReadMessage, outcome.ErrorMessage);
        Assert.DoesNotContain(root, outcome.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(recordsPath, outcome.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(JsonlDesignConversionVisualCostBudgetStore.RecordsFileName, outcome.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, outcome.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(bytesBefore, bytesAfter);
        AssertNoBudgetAppendArtifacts(root);
    }

    [Fact]
    public async Task BlankPersistedJournalLineFailsClosedForReadAndMutation()
    {
        var root = NewRoot("visual-cost-budget-blank-line");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("blank-line-user");
        var period = Period();
        var reserve = await store.TryReserveAsync(Reserve("blank-line-a", userRef, period, 0.10m, 0.01m));
        Assert.True(reserve.IsSuccess, reserve.ErrorMessage);
        var originalLine = Assert.Single(File.ReadAllLines(store.RecordsPath));
        await File.WriteAllTextAsync(store.RecordsPath, originalLine + Environment.NewLine + Environment.NewLine);

        var read = await store.ReadPeriodAsync(Query(userRef, period));
        var mutation = await store.TryReserveAsync(Reserve("blank-line-b", userRef, period, 0.10m, 0.01m));

        Assert.False(read.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, read.ErrorCode);
        Assert.False(mutation.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, mutation.ErrorCode);
        Assert.Equal(2, File.ReadAllLines(store.RecordsPath).Length);
    }

    [Fact]
    public async Task ReplayRejectsSemanticallyInconsistentReservedDecisionEvenWithValidEventId()
    {
        var root = NewRoot("visual-cost-budget-semantic-corrupt");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("semantic-corrupt-user");
        var period = Period();
        var first = await store.TryReserveAsync(Reserve("semantic-corrupt-a", userRef, period, 0.05m, 0.03m));
        var denied = await store.TryReserveAsync(Reserve("semantic-corrupt-b", userRef, period, 0.05m, 0.03m));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(denied.IsSuccess, denied.ErrorMessage);
        Assert.False(denied.Value!.Reserved);

        var lines = File.ReadAllLines(store.RecordsPath);
        var tampered = RewriteBudgetEvent(lines[1], json =>
        {
            json["decisionStatus"] = JsonlDesignConversionVisualCostBudgetStore.StatusReserved;
            json["denialReasonCode"] = JsonlDesignConversionVisualCostBudgetStore.NoDenialReason;
        });
        await File.WriteAllLinesAsync(store.RecordsPath, [lines[0], tampered]);

        var read = await store.ReadPeriodAsync(Query(userRef, period));
        var mutation = await store.TryReserveAsync(Reserve("semantic-corrupt-c", userRef, period, 0.05m, 0.01m));

        Assert.False(read.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, read.ErrorCode);
        Assert.False(mutation.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, mutation.ErrorCode);
        Assert.Equal(2, File.ReadAllLines(store.RecordsPath).Length);
    }

    [Fact]
    public async Task ReplayRejectsSettlementChronologyEarlierThanReservationEvenWithValidEventId()
    {
        var root = NewRoot("visual-cost-budget-chronology-corrupt");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("chronology-corrupt-user");
        var period = Period();
        var reserve = await store.TryReserveAsync(Reserve("chronology-corrupt-a", userRef, period, 0.10m, 0.04m));
        Assert.True(reserve.IsSuccess, reserve.ErrorMessage);
        var settled = await store.SettleAsync(Settle("chronology-corrupt-a", userRef, period, 0.02m));
        Assert.True(settled.IsSuccess, settled.ErrorMessage);

        var lines = File.ReadAllLines(store.RecordsPath);
        var tamperedSettlement = RewriteBudgetEvent(lines[1], json =>
        {
            json["recordedAtUtc"] = Timestamp(0).AddSeconds(-10);
        });
        await File.WriteAllLinesAsync(store.RecordsPath, [lines[0], tamperedSettlement]);

        var read = await store.ReadPeriodAsync(Query(userRef, period));
        var mutation = await store.TryReserveAsync(Reserve("chronology-corrupt-b", userRef, period, 0.10m, 0.01m));

        Assert.False(read.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, read.ErrorCode);
        Assert.False(mutation.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, mutation.ErrorCode);
        Assert.Equal(2, File.ReadAllLines(store.RecordsPath).Length);
    }

    [Fact]
    public async Task ConcurrentReservationAttemptsAcrossTwoInstancesCannotReserveAboveCapInSameProcess()
    {
        var root = NewRoot("visual-cost-budget-concurrency");
        var firstStore = new JsonlDesignConversionVisualCostBudgetStore(root);
        var secondStore = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("concurrency-user");
        var period = Period();

        var tasks = Enumerable.Range(0, 10)
            .Select(index =>
            {
                var store = index % 2 == 0 ? firstStore : secondStore;
                return store.TryReserveAsync(Reserve($"concurrency-{index}", userRef, period, 0.05m, 0.01m));
            })
            .ToArray();
        var results = await Task.WhenAll(tasks);
        var readback = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Equal(5, results.Count(result => result.Value!.Reserved));
        Assert.Equal(5, results.Count(result => !result.Value!.Reserved));
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(0.05m, readback.Value!.ActiveReservedUsd);
        Assert.Equal(0m, readback.Value.FinalizedSpendUsd);
        Assert.Equal(5, readback.Value.ActiveReservationCount);
        Assert.Equal(5, readback.Value.DeniedReservationCount);
        Assert.Equal(0m, readback.Value.RemainingBudgetUsd);
    }

    [Fact]
    public async Task CancelledReservationDoesNotPersist()
    {
        var root = NewRoot("visual-cost-budget-cancelled");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("cancelled-user");
        var period = Period();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var cancelled = await store.TryReserveAsync(
            Reserve("cancelled-reserve", userRef, period, 0.10m, 0.01m),
            cts.Token);

        Assert.False(cancelled.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.CancelledErrorCode, cancelled.ErrorCode);
        Assert.False(File.Exists(store.RecordsPath));
    }

    [Fact]
    public void Constructor_RejectsNonpositiveJournalLimitsAndExposesConfiguredLimit()
    {
        var defaultStore = new JsonlDesignConversionVisualCostBudgetStore(NewRoot("visual-cost-budget-default-quota"));
        var customStore = new JsonlDesignConversionVisualCostBudgetStore(
            NewRoot("visual-cost-budget-custom-quota"),
            maxJournalBytes: 1234L);

        Assert.Equal(1L * 1024L * 1024L * 1024L, JsonlDesignConversionVisualCostBudgetStore.DefaultMaxJournalBytes);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.DefaultMaxJournalBytes, defaultStore.MaxJournalBytes);
        Assert.Equal(1234L, customStore.MaxJournalBytes);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JsonlDesignConversionVisualCostBudgetStore(
                NewRoot("visual-cost-budget-zero-quota"),
                maxJournalBytes: 0L));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JsonlDesignConversionVisualCostBudgetStore(
                NewRoot("visual-cost-budget-negative-quota"),
                maxJournalBytes: -1L));
    }

    [Fact]
    public async Task TryReserveAsync_DistinctEventAtConfiguredLimit_RollsActiveSegmentAndPreservesBudgetState()
    {
        var userRef = UserRef("quota-roll-user");
        var period = Period();
        var firstRequest = Reserve("quota-roll-a", userRef, period, 1.00m, 0.01m);
        var nextRequest = Reserve("quota-roll-b", userRef, period, 1.00m, 0.01m);
        var measuredRoot = NewRoot("visual-cost-budget-quota-measure");
        var measuredStore = new JsonlDesignConversionVisualCostBudgetStore(measuredRoot);
        var measuredFirst = await measuredStore.TryReserveAsync(firstRequest);
        Assert.True(measuredFirst.IsSuccess, measuredFirst.ErrorMessage);
        var measuredFirstBytes = new FileInfo(measuredStore.RecordsPath).Length;
        Assert.True(measuredFirstBytes > 0L);

        var controlRoot = NewRoot("visual-cost-budget-quota-control");
        var controlStore = new JsonlDesignConversionVisualCostBudgetStore(controlRoot);
        var controlFirst = await controlStore.TryReserveAsync(firstRequest);
        var controlNext = await controlStore.TryReserveAsync(nextRequest);
        Assert.True(controlFirst.IsSuccess, controlFirst.ErrorMessage);
        Assert.True(controlFirst.Value!.Reserved);
        Assert.True(controlNext.IsSuccess, controlNext.ErrorMessage);
        Assert.True(controlNext.Value!.Reserved);

        var cappedRoot = NewRoot("visual-cost-budget-quota-roll");
        var cappedStore = new JsonlDesignConversionVisualCostBudgetStore(
            cappedRoot,
            maxJournalBytes: measuredFirstBytes);
        var exact = await cappedStore.TryReserveAsync(firstRequest);
        Assert.True(exact.IsSuccess, exact.ErrorMessage);
        Assert.True(exact.Value!.Reserved);
        Assert.Equal(measuredFirstBytes, new FileInfo(cappedStore.RecordsPath).Length);
        var archivedBytes = await File.ReadAllBytesAsync(cappedStore.RecordsPath);

        var rolled = await cappedStore.TryReserveAsync(nextRequest);
        var activeBytes = await File.ReadAllBytesAsync(cappedStore.RecordsPath);
        var segmentFiles = Directory
            .EnumerateFiles(cappedRoot, JsonlDesignConversionVisualCostBudgetStore.SegmentFileNamePrefix + "*")
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        var freshRead = await new JsonlDesignConversionVisualCostBudgetStore(cappedRoot, measuredFirstBytes)
            .ReadPeriodAsync(Query(userRef, period));

        Assert.True(rolled.IsSuccess, rolled.ErrorMessage);
        Assert.True(rolled.Value!.Reserved);
        var segmentPath = Assert.Single(segmentFiles);
        Assert.Equal(
            JsonlDesignConversionVisualCostBudgetStore.RecordsFileName + ".segment-000001",
            Path.GetFileName(segmentPath));
        Assert.Equal(archivedBytes, await File.ReadAllBytesAsync(segmentPath));
        Assert.Single(File.ReadAllLines(cappedStore.RecordsPath));
        Assert.NotEqual(archivedBytes, activeBytes);
        Assert.True(freshRead.IsSuccess, freshRead.ErrorMessage);
        Assert.Equal(2, freshRead.Value!.ReservationCount);
        Assert.Equal(2, freshRead.Value.ActiveReservationCount);
        Assert.Equal(0, freshRead.Value.DeniedReservationCount);
        Assert.Equal(0.02m, freshRead.Value.ActiveReservedUsd);
        Assert.Equal(0.98m, freshRead.Value.RemainingBudgetUsd);

        var settledFirst = await new JsonlDesignConversionVisualCostBudgetStore(cappedRoot)
            .SettleAsync(Settle("quota-roll-a", userRef, period, 0.01m));
        var releasedSecond = await new JsonlDesignConversionVisualCostBudgetStore(cappedRoot)
            .ReleaseAsync(Release("quota-roll-b", userRef, period));
        var finalRead = await new JsonlDesignConversionVisualCostBudgetStore(cappedRoot)
            .ReadPeriodAsync(Query(userRef, period));

        Assert.True(settledFirst.IsSuccess, settledFirst.ErrorMessage);
        Assert.True(releasedSecond.IsSuccess, releasedSecond.ErrorMessage);
        Assert.True(finalRead.IsSuccess, finalRead.ErrorMessage);
        Assert.Equal(2, finalRead.Value!.ReservationCount);
        Assert.Equal(1, finalRead.Value.SettledReservationCount);
        Assert.Equal(1, finalRead.Value.ReleasedReservationCount);
        Assert.Equal(0m, finalRead.Value.ActiveReservedUsd);
        Assert.Equal(0.01m, finalRead.Value.FinalizedSpendUsd);
        AssertNoBudgetAppendArtifacts(cappedRoot);
    }

    [Fact]
    public async Task TryReserveAsync_IdenticalReplayAtConfiguredLimit_SucceedsAndPreservesBytes()
    {
        var root = NewRoot("visual-cost-budget-quota-replay");
        var userRef = UserRef("quota-replay-user");
        var period = Period();
        var request = Reserve("quota-replay", userRef, period, 0.10m, 0.01m);
        var writer = new JsonlDesignConversionVisualCostBudgetStore(root);
        var first = await writer.TryReserveAsync(request);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(first.Value!.Reserved);
        var bytesBeforeReplay = await File.ReadAllBytesAsync(writer.RecordsPath);

        var cappedReader = new JsonlDesignConversionVisualCostBudgetStore(root, bytesBeforeReplay.LongLength);
        var replay = await cappedReader.TryReserveAsync(request);
        var bytesAfterReplay = await File.ReadAllBytesAsync(cappedReader.RecordsPath);

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.Replayed);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusReplayed, replay.Value.Status);
        Assert.Equal(bytesBeforeReplay.LongLength, cappedReader.MaxJournalBytes);
        Assert.Equal(bytesBeforeReplay, bytesAfterReplay);
        Assert.Empty(Directory.EnumerateFiles(root, JsonlDesignConversionVisualCostBudgetStore.SegmentFileNamePrefix + "*"));
        AssertNoBudgetAppendArtifacts(root);
    }

    [Fact]
    public async Task TryReserveAsync_SingleEventLargerThanConfiguredLimit_FailsQuotaWithoutPartialFiles()
    {
        var root = NewRoot("visual-cost-budget-single-line-quota");
        var sourceRoot = NewRoot("visual-cost-budget-single-line-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var userRef = UserRef("single-line-quota-user");
        var period = Period();
        var request = Reserve("single-line-quota", userRef, period, 0.10m, 0.01m);

        try
        {
            var candidate = await new JsonlDesignConversionVisualCostBudgetStore(sourceRoot)
                .TryReserveAsync(request);
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var candidateLineBytes = await File.ReadAllBytesAsync(
                Path.Combine(sourceRoot, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName));

            var result = await new JsonlDesignConversionVisualCostBudgetStore(root, candidateLineBytes.LongLength - 1L)
                .TryReserveAsync(request);

            AssertBudgetQuotaFailure(result, root);
            Assert.False(File.Exists(recordsPath));
            Assert.Empty(Directory.EnumerateFiles(root, JsonlDesignConversionVisualCostBudgetStore.SegmentFileNamePrefix + "*"));
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task ReadPeriodAsync_ActiveJournalOverConfiguredLimit_FailsQuotaWithoutReadingOrMutation()
    {
        var root = NewRoot("visual-cost-budget-active-over-quota");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef("active-over-quota-user");
        var period = Period();
        var first = await store.TryReserveAsync(Reserve("active-over-quota", userRef, period, 0.10m, 0.01m));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var bytesBefore = await File.ReadAllBytesAsync(store.RecordsPath);

        var result = await new JsonlDesignConversionVisualCostBudgetStore(root, bytesBefore.LongLength - 1L)
            .ReadPeriodAsync(Query(userRef, period));
        var bytesAfter = await File.ReadAllBytesAsync(store.RecordsPath);

        AssertBudgetQuotaFailure(result, root);
        Assert.Equal(bytesBefore, bytesAfter);
        AssertNoBudgetAppendArtifacts(root);
    }

    [Fact]
    public async Task ReadPeriodAsync_DuplicateEventIdAcrossSegmentAndActiveFailsClosed()
    {
        var root = NewRoot("visual-cost-budget-segment-duplicate");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var userRef = UserRef("segment-duplicate-user");
        var period = Period();

        try
        {
            var seed = new JsonlDesignConversionVisualCostBudgetStore(root);
            var first = await seed.TryReserveAsync(Reserve("segment-duplicate-a", userRef, period, 0.10m, 0.01m));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var firstBytes = await File.ReadAllBytesAsync(recordsPath);
            var second = await new JsonlDesignConversionVisualCostBudgetStore(root, firstBytes.LongLength)
                .TryReserveAsync(Reserve("segment-duplicate-b", userRef, period, 0.10m, 0.02m));
            Assert.True(second.IsSuccess, second.ErrorMessage);
            var segmentPath = Assert.Single(Directory.EnumerateFiles(
                root,
                JsonlDesignConversionVisualCostBudgetStore.SegmentFileNamePrefix + "*"));
            var segmentBytes = await File.ReadAllBytesAsync(segmentPath);
            await File.WriteAllBytesAsync(recordsPath, segmentBytes);

            var result = await new JsonlDesignConversionVisualCostBudgetStore(root, firstBytes.LongLength)
                .ReadPeriodAsync(Query(userRef, period));
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertBudgetReadFailure(
                result,
                root,
                recordsPath,
                "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-budget-reservations.jsonl#duplicate");
            Assert.Equal(segmentBytes, activeBytes);
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ReadPeriodAsync_MalformedSegmentFailsClosedAndPreservesActiveJournal()
    {
        var root = NewRoot("visual-cost-budget-malformed-segment");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var segmentPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.SegmentFileNamePrefix + "000001");
        var userRef = UserRef("malformed-segment-user");
        var period = Period();

        try
        {
            var store = new JsonlDesignConversionVisualCostBudgetStore(root);
            var active = await store.TryReserveAsync(Reserve("malformed-segment-active", userRef, period, 0.10m, 0.01m));
            Assert.True(active.IsSuccess, active.ErrorMessage);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);
            await File.WriteAllTextAsync(segmentPath, "{\"not\":\"a valid budget event\"}" + Environment.NewLine);

            var result = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertBudgetReadFailure(
                result,
                root,
                recordsPath,
                "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-budget-reservations.jsonl#malformed-segment");
            Assert.Equal(activeBytes, afterBytes);
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ReadPeriodAsync_SegmentDirectoryFailsClosedAndPreservesActiveJournal()
    {
        var root = NewRoot("visual-cost-budget-segment-directory");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var segmentDirectoryPath = Path.Combine(
            root,
            JsonlDesignConversionVisualCostBudgetStore.SegmentFileNamePrefix + "000001");
        var userRef = UserRef("segment-directory-user");
        var period = Period();

        try
        {
            var store = new JsonlDesignConversionVisualCostBudgetStore(root);
            var active = await store.TryReserveAsync(Reserve("segment-directory-active", userRef, period, 0.10m, 0.01m));
            Assert.True(active.IsSuccess, active.ErrorMessage);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);
            Directory.CreateDirectory(segmentDirectoryPath);

            var result = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertBudgetReadFailure(
                result,
                root,
                recordsPath,
                "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-budget-reservations.jsonl#segment-directory");
            Assert.Equal(activeBytes, afterBytes);
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ReadPeriodAsync_NonCanonicalSegmentAliasFailsClosedAndPreservesActiveJournal()
    {
        var root = NewRoot("visual-cost-budget-segment-1-alias");
        var sourceRoot = NewRoot("visual-cost-budget-segment-1-alias-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var sourceRecordsPath = Path.Combine(sourceRoot, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var segmentAliasPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.SegmentFileNamePrefix + "1");
        var userRef = UserRef("segment-alias-user");
        var sourceUserRef = UserRef("segment-alias-source-user");
        var period = Period();

        try
        {
            var store = new JsonlDesignConversionVisualCostBudgetStore(root);
            var active = await store.TryReserveAsync(Reserve("segment-alias-active", userRef, period, 0.10m, 0.01m));
            Assert.True(active.IsSuccess, active.ErrorMessage);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);

            var source = await new JsonlDesignConversionVisualCostBudgetStore(sourceRoot)
                .TryReserveAsync(Reserve("segment-alias-source", sourceUserRef, period, 0.10m, 0.01m));
            Assert.True(source.IsSuccess, source.ErrorMessage);
            await File.WriteAllBytesAsync(segmentAliasPath, await File.ReadAllBytesAsync(sourceRecordsPath));

            var result = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            AssertBudgetReadFailure(
                result,
                root,
                recordsPath,
                "PRIVATE_MARKER_C:/Users/lubao/private/visual-cost-budget-reservations.jsonl#segment-1-alias");
            Assert.Equal(activeBytes, afterBytes);
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [DesignConversionJournalReparsePointFact]
    public async Task ReadPeriodAsync_RootDirectoryReparsePointFailsClosedWithoutFollowingExternalJournal()
    {
        var targetRoot = NewRoot("visual-cost-budget-root-reparse-target");
        var linkRoot = NewRoot("visual-cost-budget-root-reparse-link");
        var targetRecordsPath = Path.Combine(
            targetRoot,
            JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var userRef = UserRef("budget-root-reparse-user");
        var period = Period();

        try
        {
            var target = await new JsonlDesignConversionVisualCostBudgetStore(targetRoot)
                .TryReserveAsync(Reserve("budget-root-reparse", userRef, period, 0.10m, 0.01m));
            Assert.True(target.IsSuccess, target.ErrorMessage);
            var targetBytes = await File.ReadAllBytesAsync(targetRecordsPath);
            Directory.CreateDirectory(Path.GetDirectoryName(linkRoot)!);
            Directory.CreateSymbolicLink(linkRoot, targetRoot);
            Assert.NotEqual(FileAttributes.None, File.GetAttributes(linkRoot) & FileAttributes.ReparsePoint);

            var result = await new JsonlDesignConversionVisualCostBudgetStore(linkRoot)
                .ReadPeriodAsync(Query(userRef, period));

            AssertBudgetReadFailure(
                result,
                linkRoot,
                Path.Combine(linkRoot, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName),
                targetRoot);
            Assert.Equal(targetBytes, await File.ReadAllBytesAsync(targetRecordsPath));
            AssertNoBudgetAppendArtifacts(targetRoot);
        }
        finally
        {
            if (Directory.Exists(linkRoot))
                Directory.Delete(linkRoot);
            DeleteTempRoot(Path.GetDirectoryName(linkRoot)!);
            DeleteTempRoot(targetRoot);
        }
    }

    [DesignConversionJournalReparsePointFact]
    public async Task ReadPeriodAsync_ActiveJournalFileReparsePointFailsClosedWithoutFollowingExternalJournal()
    {
        var root = NewRoot("visual-cost-budget-active-reparse");
        var sourceRoot = NewRoot("visual-cost-budget-active-reparse-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var sourceRecordsPath = Path.Combine(
            sourceRoot,
            JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var userRef = UserRef("budget-active-reparse-user");
        var period = Period();

        try
        {
            var source = await new JsonlDesignConversionVisualCostBudgetStore(sourceRoot)
                .TryReserveAsync(Reserve("budget-active-reparse", userRef, period, 0.10m, 0.01m));
            Assert.True(source.IsSuccess, source.ErrorMessage);
            var sourceBytes = await File.ReadAllBytesAsync(sourceRecordsPath);
            Directory.CreateDirectory(root);
            File.CreateSymbolicLink(recordsPath, sourceRecordsPath);
            Assert.NotEqual(FileAttributes.None, File.GetAttributes(recordsPath) & FileAttributes.ReparsePoint);

            var result = await new JsonlDesignConversionVisualCostBudgetStore(root)
                .ReadPeriodAsync(Query(userRef, period));

            AssertBudgetReadFailure(result, root, recordsPath, sourceRecordsPath);
            Assert.Equal(sourceBytes, await File.ReadAllBytesAsync(sourceRecordsPath));
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            if (File.Exists(recordsPath))
                File.Delete(recordsPath);
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [DesignConversionJournalReparsePointFact]
    public async Task ReadPeriodAsync_CanonicalSegmentFileReparsePointFailsClosedWithoutFollowingExternalJournal()
    {
        var root = NewRoot("visual-cost-budget-segment-reparse");
        var sourceRoot = NewRoot("visual-cost-budget-segment-reparse-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var sourceRecordsPath = Path.Combine(
            sourceRoot,
            JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var segmentPath = Path.Combine(
            root,
            JsonlDesignConversionVisualCostBudgetStore.SegmentFileNamePrefix + "000001");
        var userRef = UserRef("budget-segment-reparse-user");
        var period = Period();

        try
        {
            var active = await new JsonlDesignConversionVisualCostBudgetStore(root)
                .TryReserveAsync(Reserve("budget-segment-reparse-active", userRef, period, 0.10m, 0.01m));
            var source = await new JsonlDesignConversionVisualCostBudgetStore(sourceRoot)
                .TryReserveAsync(Reserve("budget-segment-reparse-source", userRef, period, 0.10m, 0.02m));
            Assert.True(active.IsSuccess, active.ErrorMessage);
            Assert.True(source.IsSuccess, source.ErrorMessage);
            var activeBytes = await File.ReadAllBytesAsync(recordsPath);
            var sourceBytes = await File.ReadAllBytesAsync(sourceRecordsPath);
            File.CreateSymbolicLink(segmentPath, sourceRecordsPath);
            Assert.NotEqual(FileAttributes.None, File.GetAttributes(segmentPath) & FileAttributes.ReparsePoint);

            var result = await new JsonlDesignConversionVisualCostBudgetStore(root)
                .ReadPeriodAsync(Query(userRef, period));

            AssertBudgetReadFailure(result, root, recordsPath, sourceRecordsPath);
            Assert.Equal(activeBytes, await File.ReadAllBytesAsync(recordsPath));
            Assert.Equal(sourceBytes, await File.ReadAllBytesAsync(sourceRecordsPath));
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            if (File.Exists(segmentPath))
                File.Delete(segmentPath);
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task ReadPeriodAsync_LegacyAtomicityScopeReplaysAndMixedJournalKeepsOriginalEventId()
    {
        var root = NewRoot("visual-cost-budget-legacy-mixed-replay");
        try
        {
            var store = new JsonlDesignConversionVisualCostBudgetStore(root);
            var userRef = UserRef("legacy-mixed-user");
            var period = Period();
            var first = await store.TryReserveAsync(Reserve("legacy-mixed-a", userRef, period, 0.10m, 0.01m));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(first.Value!.Reserved);

            var originalLine = Assert.Single(File.ReadAllLines(store.RecordsPath));
            var originalEventId = RequiredString(JsonNode.Parse(originalLine)!.AsObject(), "eventId");
            var legacyLine = RewriteBudgetMetadataPreservingEventId(originalLine, metadata =>
            {
                metadata["atomicity_scope"] = LegacyBudgetAtomicityScope;
            });
            Assert.Equal(originalEventId, RequiredString(JsonNode.Parse(legacyLine)!.AsObject(), "eventId"));
            await File.WriteAllTextAsync(store.RecordsPath, legacyLine + Environment.NewLine, Encoding.UTF8);

            var legacyRead = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));
            var second = await new JsonlDesignConversionVisualCostBudgetStore(root)
                .TryReserveAsync(Reserve("legacy-mixed-b", userRef, period, 0.10m, 0.02m));
            var lines = File.ReadAllLines(store.RecordsPath);
            var mixedRead = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));

            Assert.True(legacyRead.IsSuccess, legacyRead.ErrorMessage);
            Assert.Equal(1, legacyRead.Value!.ReservationCount);
            Assert.Equal(1, legacyRead.Value.ActiveReservationCount);
            Assert.Equal(0.01m, legacyRead.Value.ActiveReservedUsd);
            Assert.Equal(0.09m, legacyRead.Value.RemainingBudgetUsd);

            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.True(second.Value!.Reserved);
            Assert.Equal(2, lines.Length);
            Assert.Equal(originalEventId, RequiredString(JsonNode.Parse(lines[0])!.AsObject(), "eventId"));
            Assert.Equal(
                [LegacyBudgetAtomicityScope, CurrentBudgetAtomicityScope],
                lines.Select(ReadBudgetAtomicityScope).ToArray());
            Assert.All(lines, line => Assert.Equal(7, ReadBudgetMetadataCount(line)));

            Assert.True(mixedRead.IsSuccess, mixedRead.ErrorMessage);
            Assert.Equal(2, mixedRead.Value!.ReservationCount);
            Assert.Equal(2, mixedRead.Value.ActiveReservationCount);
            Assert.Equal(0, mixedRead.Value.DeniedReservationCount);
            Assert.Equal(0.03m, mixedRead.Value.ActiveReservedUsd);
            Assert.Equal(0.07m, mixedRead.Value.RemainingBudgetUsd);
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Theory]
    [InlineData("unknown_atomicity_scope")]
    [InlineData("extra_metadata_key")]
    public async Task ReadPeriodAsync_UnexpectedJournalMetadataFailsClosedAndPreservesBytes(string tamperKind)
    {
        var root = NewRoot($"visual-cost-budget-metadata-{tamperKind}");
        try
        {
            var store = new JsonlDesignConversionVisualCostBudgetStore(root);
            var userRef = UserRef($"metadata-{tamperKind}-user");
            var period = Period();
            var first = await store.TryReserveAsync(Reserve($"metadata-{tamperKind}-a", userRef, period, 0.10m, 0.01m));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(first.Value!.Reserved);
            var originalLine = Assert.Single(File.ReadAllLines(store.RecordsPath));
            var originalEventId = RequiredString(JsonNode.Parse(originalLine)!.AsObject(), "eventId");
            var tamperedLine = RewriteBudgetMetadataPreservingEventId(originalLine, metadata =>
            {
                if (string.Equals(tamperKind, "unknown_atomicity_scope", StringComparison.Ordinal))
                {
                    metadata["atomicity_scope"] = "single_process_per_path_lock_unknown";
                }
                else
                {
                    metadata["unexpected_metadata_key"] = "unexpected";
                }
            });
            Assert.Equal(originalEventId, RequiredString(JsonNode.Parse(tamperedLine)!.AsObject(), "eventId"));
            Assert.NotEqual(originalLine, tamperedLine);
            await File.WriteAllTextAsync(store.RecordsPath, tamperedLine + Environment.NewLine, Encoding.UTF8);
            var bytesBefore = await File.ReadAllBytesAsync(store.RecordsPath);
            var expectedReadFailedMessage = await ExistingReadFailedMessageAsync("ReadPeriodAsync", userRef, period);

            var read = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));
            var mutation = await new JsonlDesignConversionVisualCostBudgetStore(root)
                .TryReserveAsync(Reserve($"metadata-{tamperKind}-b", userRef, period, 0.10m, 0.01m));
            var bytesAfter = await File.ReadAllBytesAsync(store.RecordsPath);

            Assert.False(read.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, read.ErrorCode);
            Assert.Equal(expectedReadFailedMessage, read.ErrorMessage);
            Assert.False(mutation.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, mutation.ErrorCode);
            Assert.Equal(bytesBefore, bytesAfter);
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task TryReserveAsync_JournalOpenWithoutDeleteShare_AppendsInPlaceAndPreservesPrefix()
    {
        var root = NewRoot("visual-cost-budget-shared-open-append");
        try
        {
            var store = new JsonlDesignConversionVisualCostBudgetStore(root);
            var userRef = UserRef("shared-open-budget-user");
            var period = Period();
            var first = await store.TryReserveAsync(Reserve("shared-open-budget-prefix", userRef, period, 0.10m, 0.01m));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var prefixBytes = await File.ReadAllBytesAsync(store.RecordsPath);
            byte[] appendedViaHeldOpen;

            await using (var heldOpen = new FileStream(
                store.RecordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                var initialLength = heldOpen.Length;
                var initialPosition = heldOpen.Position;
                var append = await new JsonlDesignConversionVisualCostBudgetStore(root)
                    .TryReserveAsync(Reserve("shared-open-budget-appended", userRef, period, 0.10m, 0.02m));

                Assert.True(append.IsSuccess, append.ErrorMessage);
                Assert.True(append.Value!.Reserved);
                Assert.Equal(prefixBytes.LongLength, initialLength);
                Assert.Equal(0L, initialPosition);
                Assert.True(heldOpen.Length > initialLength);
                heldOpen.Position = initialLength;
                appendedViaHeldOpen = new byte[checked((int)(heldOpen.Length - initialLength))];
                await heldOpen.ReadExactlyAsync(appendedViaHeldOpen);
            }

            var afterBytes = await File.ReadAllBytesAsync(store.RecordsPath);
            var readback = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));

            Assert.Equal(prefixBytes, afterBytes.Take(prefixBytes.Length).ToArray());
            Assert.Equal(appendedViaHeldOpen, afterBytes.Skip(prefixBytes.Length).ToArray());
            Assert.Equal(2, File.ReadAllLines(store.RecordsPath).Length);
            Assert.True(readback.IsSuccess, readback.ErrorMessage);
            Assert.Equal(2, readback.Value!.ReservationCount);
            Assert.Equal(2, readback.Value.ActiveReservationCount);
            Assert.Equal(0.03m, readback.Value.ActiveReservedUsd);
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task ReadPeriodAsync_ProtocolOwnedInterruptedFinalAppend_RecoversExactlyOneBudgetRecord()
    {
        var root = NewRoot("visual-cost-budget-recover-owned-partial");
        var sourceRoot = NewRoot("visual-cost-budget-recover-owned-partial-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var userRef = UserRef("recover-owned-budget-user");
        var period = Period();

        try
        {
            var seed = new JsonlDesignConversionVisualCostBudgetStore(root);
            var first = await seed.TryReserveAsync(Reserve("recover-owned-budget-existing", userRef, period, 0.10m, 0.01m));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var prefixBytes = await File.ReadAllBytesAsync(recordsPath);

            var source = new JsonlDesignConversionVisualCostBudgetStore(sourceRoot);
            var candidate = await source.TryReserveAsync(Reserve("recover-owned-budget-appended", userRef, period, 0.10m, 0.02m));
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var candidateLineBytes = await File.ReadAllBytesAsync(source.RecordsPath);
            var interruptedByteCount = Math.Max(1, candidateLineBytes.Length / 2);
            await DesignConversionBoundedJsonlAppendJournal.PublishInterruptedAppendForTestingAsync(
                root,
                recordsPath,
                JsonlDesignConversionVisualCostBudgetStore.RecordsFileName,
                candidateLineBytes,
                prefixBytes.LongLength,
                interruptedByteCount);

            var recovered = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));
            var secondRead = await new JsonlDesignConversionVisualCostBudgetStore(root).ReadPeriodAsync(Query(userRef, period));
            var afterBytes = await File.ReadAllBytesAsync(recordsPath);

            Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
            Assert.Equal(2, recovered.Value!.ReservationCount);
            Assert.Equal(2, recovered.Value.ActiveReservationCount);
            Assert.Equal(0.03m, recovered.Value.ActiveReservedUsd);
            Assert.True(secondRead.IsSuccess, secondRead.ErrorMessage);
            Assert.Equal(2, secondRead.Value!.ReservationCount);
            Assert.Equal(2, File.ReadAllLines(recordsPath).Length);
            Assert.Equal(prefixBytes, afterBytes.Take(prefixBytes.Length).ToArray());
            Assert.Equal(candidateLineBytes, afterBytes.Skip(prefixBytes.Length).ToArray());
            AssertNoBudgetAppendArtifacts(root);
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task ReadPeriodAsync_ProtocolOwnedAppendBeyondConfiguredQuota_FailsClosedAndPreservesIntent()
    {
        var root = NewRoot("visual-cost-budget-recover-over-quota");
        var sourceRoot = NewRoot("visual-cost-budget-recover-over-quota-source");
        var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
        var intentPath = recordsPath + ".bounded-append.intent";
        var userRef = UserRef("recover-over-quota-budget-user");
        var period = Period();

        try
        {
            var seed = new JsonlDesignConversionVisualCostBudgetStore(root);
            var first = await seed.TryReserveAsync(Reserve("recover-over-quota-budget-existing", userRef, period, 0.10m, 0.01m));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var prefixBytes = await File.ReadAllBytesAsync(recordsPath);

            var source = new JsonlDesignConversionVisualCostBudgetStore(sourceRoot);
            var candidate = await source.TryReserveAsync(Reserve("recover-over-quota-budget-appended", userRef, period, 0.10m, 0.02m));
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var candidateLineBytes = await File.ReadAllBytesAsync(source.RecordsPath);
            var maxJournalBytes = checked(prefixBytes.LongLength + candidateLineBytes.LongLength - 1L);
            await DesignConversionBoundedJsonlAppendJournal.PublishInterruptedAppendForTestingAsync(
                root,
                recordsPath,
                JsonlDesignConversionVisualCostBudgetStore.RecordsFileName,
                candidateLineBytes,
                prefixBytes.LongLength,
                appendedByteCount: 1);
            var expectedJournalBytes = await File.ReadAllBytesAsync(recordsPath);
            var expectedIntentBytes = await File.ReadAllBytesAsync(intentPath);
            var expectedReadFailedMessage = await ExistingReadFailedMessageAsync("ReadPeriodAsync", userRef, period);

            var result = await new JsonlDesignConversionVisualCostBudgetStore(root, maxJournalBytes)
                .ReadPeriodAsync(Query(userRef, period));
            var afterJournalBytes = await File.ReadAllBytesAsync(recordsPath);
            var afterIntentBytes = await File.ReadAllBytesAsync(intentPath);

            Assert.False(result.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, result.ErrorCode);
            Assert.Equal(expectedReadFailedMessage, result.ErrorMessage);
            Assert.Equal(expectedJournalBytes, afterJournalBytes);
            Assert.Equal(expectedIntentBytes, afterIntentBytes);
            Assert.True(File.Exists(intentPath));
        }
        finally
        {
            DeleteTempRoot(root);
            DeleteTempRoot(sourceRoot);
        }
    }

    [Fact]
    public async Task PersistenceResultsAndErrorsDoNotExposeRawIdentityOrPayload()
    {
        const string rawMarker = "raw-user-identity-budget-store-42";
        const string rawPayload = "<main>private design payload</main>";
        var root = NewRoot("visual-cost-budget-privacy");
        var store = new JsonlDesignConversionVisualCostBudgetStore(root);
        var userRef = UserRef(rawMarker);
        var period = Period();

        var reserve = await store.TryReserveAsync(Reserve(rawMarker, userRef, period, 0.10m, 0.01m));
        var settle = await store.SettleAsync(Settle(rawMarker, userRef, period, 0.01m));
        var invalid = await store.TryReserveAsync(new DesignConversionVisualCostReservationRequest(
            ReservationRef: rawMarker,
            UserRef: rawMarker,
            SubmittedTier: rawPayload,
            PeriodStartUtc: period.StartUtc,
            PeriodEndExclusiveUtc: period.EndExclusiveUtc,
            MonthlyLimitUsd: 0.10m,
            RequestedReserveUsd: 0.01m,
            RequestedAtUtc: Timestamp(99)));

        Assert.True(reserve.IsSuccess, reserve.ErrorMessage);
        Assert.True(settle.IsSuccess, settle.ErrorMessage);
        Assert.False(invalid.IsSuccess);
        Assert.DoesNotContain(rawMarker, reserve.Value!.ReservationRef, StringComparison.Ordinal);
        Assert.DoesNotContain(rawMarker, reserve.Value.UserRef, StringComparison.Ordinal);
        Assert.DoesNotContain(rawMarker, invalid.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPayload, invalid.ErrorMessage, StringComparison.Ordinal);
        var persisted = await File.ReadAllTextAsync(store.RecordsPath);
        Assert.DoesNotContain(rawMarker, persisted, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPayload, persisted, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", persisted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw-user", persisted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private design", persisted, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(bool.FalseString, reserve.Value.Metadata["raw_payloads_persisted"]);
        Assert.Equal(bool.FalseString, reserve.Value.Metadata["raw_user_identity_persisted"]);
    }

    private static string NewRoot(string suffix)
        => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), suffix);

    private static void DeleteTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static void AssertNoBudgetAppendArtifacts(string root)
    {
        Assert.Empty(Directory.EnumerateFiles(
            root,
            $"{JsonlDesignConversionVisualCostBudgetStore.RecordsFileName}.*.append.tmp"));
        Assert.Empty(Directory.EnumerateFiles(
            root,
            $"{JsonlDesignConversionVisualCostBudgetStore.RecordsFileName}*.bounded-append.intent*"));
    }

    private static void AssertBudgetQuotaFailure<T>(OperationResult<T> result, string root)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.JournalQuotaExceededErrorCode, result.ErrorCode);
        Assert.Equal(
            JsonlDesignConversionVisualCostBudgetStore.JournalQuotaExceededSafeMessage,
            result.ErrorMessage);
        Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(
            JsonlDesignConversionVisualCostBudgetStore.RecordsFileName,
            result.ErrorMessage,
            StringComparison.Ordinal);
    }

    private static void AssertBudgetReadFailure<T>(
        OperationResult<T> result,
        string root,
        string recordsPath,
        string privateMarker)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, result.ErrorCode);
        Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(recordsPath, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(
            JsonlDesignConversionVisualCostBudgetStore.RecordsFileName,
            result.ErrorMessage,
            StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
    }

    private static string UserRef(string source)
        => JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef(source).Value!;

    private static string ReservationRef(string source)
        => JsonlDesignConversionVisualCostBudgetStore.BuildCanonicalReservationRef(source);

    private static async Task<BudgetOperationOutcome> InvokeBudgetOperationAsync(
        JsonlDesignConversionVisualCostBudgetStore store,
        string operation,
        string userRef,
        (DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc) period)
    {
        return operation switch
        {
            "ReadPeriodAsync" => ToOutcome(await store.ReadPeriodAsync(Query(userRef, period))),
            "TryReserveAsync" => ToOutcome(await store.TryReserveAsync(
                Reserve("invalid-utf8-reserve", userRef, period, 0.10m, 0.01m))),
            "SettleAsync" => ToOutcome(await store.SettleAsync(
                Settle("invalid-utf8-reserve", userRef, period, 0.01m))),
            "ReleaseAsync" => ToOutcome(await store.ReleaseAsync(
                Release("invalid-utf8-reserve", userRef, period))),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown budget-store operation.")
        };
    }

    private static async Task<string> ExistingReadFailedMessageAsync(
        string operation,
        string userRef,
        (DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc) period)
    {
        var root = NewRoot($"visual-cost-budget-existing-read-failed-message-{operation}");
        try
        {
            Directory.CreateDirectory(root);
            var recordsPath = Path.Combine(root, JsonlDesignConversionVisualCostBudgetStore.RecordsFileName);
            await File.WriteAllTextAsync(recordsPath, "{ not-json", Encoding.UTF8);
            var outcome = await InvokeBudgetOperationAsync(
                new JsonlDesignConversionVisualCostBudgetStore(root),
                operation,
                userRef,
                period);

            Assert.False(outcome.IsSuccess);
            Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode, outcome.ErrorCode);
            Assert.DoesNotContain(root, outcome.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(recordsPath, outcome.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(JsonlDesignConversionVisualCostBudgetStore.RecordsFileName, outcome.ErrorMessage, StringComparison.Ordinal);
            return outcome.ErrorMessage;
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    private static BudgetOperationOutcome ToOutcome<T>(OperationResult<T> result)
        => new(result.IsSuccess, result.ErrorMessage, result.ErrorCode);

    private static byte[] BuildInvalidUtf8JournalBytes(string privateMarker)
        => Encoding.UTF8.GetBytes("{\"nearMalformed\":\"")
            .Concat(new byte[] { 0xC3, 0x28 })
            .Concat(Encoding.UTF8.GetBytes($"{privateMarker}\"}}{Environment.NewLine}"))
            .ToArray();

    private static (DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc) Period()
        => JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(
            new DateTimeOffset(2026, 4, 18, 12, 0, 0, TimeSpan.Zero));

    private static DesignConversionVisualCostBudgetPeriodQuery Query(
        string userRef,
        (DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc) period)
        => new(userRef, period.StartUtc, period.EndExclusiveUtc);

    private static DesignConversionVisualCostReservationRequest Reserve(
        string reservationSeed,
        string userRef,
        (DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc) period,
        decimal monthlyLimitUsd,
        decimal requestedReserveUsd,
        string submittedTier = "paid")
        => new(
            ReservationRef: ReservationRef(reservationSeed),
            UserRef: userRef,
            SubmittedTier: submittedTier,
            PeriodStartUtc: period.StartUtc,
            PeriodEndExclusiveUtc: period.EndExclusiveUtc,
            MonthlyLimitUsd: monthlyLimitUsd,
            RequestedReserveUsd: requestedReserveUsd,
            RequestedAtUtc: Timestamp(StableOffset(reservationSeed, 0)));

    private static DesignConversionVisualCostSettlementRequest Settle(
        string reservationSeed,
        string userRef,
        (DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc) period,
        decimal actualCostUsd)
        => new(
            ReservationRef: ReservationRef(reservationSeed),
            UserRef: userRef,
            PeriodStartUtc: period.StartUtc,
            PeriodEndExclusiveUtc: period.EndExclusiveUtc,
            ActualCostUsd: actualCostUsd,
            SettledAtUtc: Timestamp(StableOffset(reservationSeed, 2000)));

    private static DesignConversionVisualCostReleaseRequest Release(
        string reservationSeed,
        string userRef,
        (DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc) period)
        => new(
            ReservationRef: ReservationRef(reservationSeed),
            UserRef: userRef,
            PeriodStartUtc: period.StartUtc,
            PeriodEndExclusiveUtc: period.EndExclusiveUtc,
            ReleasedAtUtc: Timestamp(StableOffset(reservationSeed, 3000)));

    private static int StableOffset(string source, int baseSeconds)
    {
        uint hash = 2166136261;
        foreach (var ch in source)
        {
            hash ^= ch;
            hash *= 16777619;
        }

        return baseSeconds + (int)(hash % 1000);
    }

    private static string RewriteBudgetEvent(string jsonLine, Action<JsonObject> mutate)
    {
        var json = JsonNode.Parse(jsonLine)!.AsObject();
        mutate(json);
        json["eventId"] = BuildBudgetEventId(json);
        return json.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    private static string RewriteBudgetMetadataPreservingEventId(
        string jsonLine,
        Action<JsonObject> mutateMetadata)
    {
        var json = JsonNode.Parse(jsonLine)!.AsObject();
        var originalEventId = RequiredString(json, "eventId");
        mutateMetadata(json["metadata"]!.AsObject());
        Assert.Equal(originalEventId, RequiredString(json, "eventId"));
        return json.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    private static string ReadBudgetAtomicityScope(string jsonLine)
        => JsonNode.Parse(jsonLine)!.AsObject()["metadata"]!.AsObject()["atomicity_scope"]!.GetValue<string>();

    private static int ReadBudgetMetadataCount(string jsonLine)
        => JsonNode.Parse(jsonLine)!.AsObject()["metadata"]!.AsObject().Count;

    private static string BuildBudgetEventId(JsonObject json)
    {
        var identity = string.Join("|",
        [
            JsonlDesignConversionVisualCostBudgetStore.LedgerKind,
            JsonlDesignConversionVisualCostBudgetStore.SchemaVersion,
            RequiredString(json, "eventType"),
            RequiredString(json, "reservationRef"),
            RequiredString(json, "userRef"),
            RequiredString(json, "submittedTier"),
            RequiredString(json, "periodKind"),
            RequiredDateTimeOffset(json, "periodStartUtc").ToString("O", CultureInfo.InvariantCulture),
            RequiredDateTimeOffset(json, "periodEndExclusiveUtc").ToString("O", CultureInfo.InvariantCulture),
            RequiredDecimal(json, "monthlyLimitUsd").ToString("G29", CultureInfo.InvariantCulture),
            RequiredDecimal(json, "requestedReserveUsd").ToString("G29", CultureInfo.InvariantCulture),
            RequiredDecimal(json, "actualCostUsd").ToString("G29", CultureInfo.InvariantCulture),
            RequiredDateTimeOffset(json, "recordedAtUtc").ToString("O", CultureInfo.InvariantCulture),
            RequiredString(json, "decisionStatus"),
            RequiredString(json, "denialReasonCode")
        ]);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
        return $"{JsonlDesignConversionVisualCostBudgetStore.EventRefPrefix}-{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }

    private static string RequiredString(JsonObject json, string name)
        => json[name]!.GetValue<string>();

    private static decimal RequiredDecimal(JsonObject json, string name)
        => json[name]!.GetValue<decimal>();

    private static DateTimeOffset RequiredDateTimeOffset(JsonObject json, string name)
        => json[name]!.GetValue<DateTimeOffset>();

    private static DateTimeOffset Timestamp(int seconds)
        => new DateTimeOffset(2026, 4, 18, 12, 0, 0, TimeSpan.Zero).AddSeconds(seconds);

    private sealed record BudgetOperationOutcome(
        bool IsSuccess,
        string ErrorMessage,
        string ErrorCode);
}
