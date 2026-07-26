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
        Assert.Empty(Directory.EnumerateFiles(
            root,
            $"{JsonlDesignConversionVisualCostBudgetStore.RecordsFileName}.*.append.tmp"));
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
    public async Task TryReserveAsync_ExactJournalCapSucceedsButNextAppendFailsWithoutMutationOrLeakage()
    {
        const string privateMarker = "n25-7c2-private-quota-marker";
        var userRef = UserRef("quota-user");
        var period = Period();
        var firstRequest = Reserve("quota-first", userRef, period, 1.00m, 0.01m);
        var nextRequest = Reserve(privateMarker, userRef, period, 1.00m, 0.01m);
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

        var cappedRoot = NewRoot($"visual-cost-budget-quota-{privateMarker}");
        var cappedStore = new JsonlDesignConversionVisualCostBudgetStore(
            cappedRoot,
            maxJournalBytes: measuredFirstBytes);
        var exact = await cappedStore.TryReserveAsync(firstRequest);
        Assert.True(exact.IsSuccess, exact.ErrorMessage);
        Assert.True(exact.Value!.Reserved);
        Assert.Equal(measuredFirstBytes, new FileInfo(cappedStore.RecordsPath).Length);
        var bytesBeforeRejection = await File.ReadAllBytesAsync(cappedStore.RecordsPath);

        var rejected = await cappedStore.TryReserveAsync(nextRequest);
        var bytesAfterRejection = await File.ReadAllBytesAsync(cappedStore.RecordsPath);
        var freshRead = await new JsonlDesignConversionVisualCostBudgetStore(cappedRoot)
            .ReadPeriodAsync(Query(userRef, period));

        Assert.False(rejected.IsSuccess);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.JournalQuotaExceededErrorCode, rejected.ErrorCode);
        Assert.Equal(
            JsonlDesignConversionVisualCostBudgetStore.JournalQuotaExceededSafeMessage,
            rejected.ErrorMessage);
        Assert.DoesNotContain(cappedRoot, rejected.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, rejected.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(bytesBeforeRejection, bytesAfterRejection);
        Assert.True(freshRead.IsSuccess, freshRead.ErrorMessage);
        Assert.Equal(1, freshRead.Value!.ReservationCount);
        Assert.Equal(1, freshRead.Value.ActiveReservationCount);
        Assert.Equal(0, freshRead.Value.DeniedReservationCount);
        Assert.Empty(Directory.EnumerateFiles(
            cappedRoot,
            $"{JsonlDesignConversionVisualCostBudgetStore.RecordsFileName}.*.append.tmp"));
    }

    [Fact]
    public async Task TryReserveAsync_ReplaySucceedsUnderSmallerJournalCapWithoutAppending()
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
        var smallerCap = Math.Max(1L, bytesBeforeReplay.LongLength - 1L);

        var cappedReader = new JsonlDesignConversionVisualCostBudgetStore(root, smallerCap);
        var replay = await cappedReader.TryReserveAsync(request);
        var bytesAfterReplay = await File.ReadAllBytesAsync(cappedReader.RecordsPath);

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.Replayed);
        Assert.Equal(JsonlDesignConversionVisualCostBudgetStore.StatusReplayed, replay.Value.Status);
        Assert.True(bytesBeforeReplay.LongLength > cappedReader.MaxJournalBytes);
        Assert.Equal(bytesBeforeReplay, bytesAfterReplay);
        Assert.Empty(Directory.EnumerateFiles(
            root,
            $"{JsonlDesignConversionVisualCostBudgetStore.RecordsFileName}.*.append.tmp"));
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
