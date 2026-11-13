// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;

public sealed class DesignConversionPostEvaluationTrainingEligibilityTests
{
    private static readonly string LegalTeacherModelLayerDigest = new('e', 64);

    [Fact]
    public async Task ExactCommittedMatchingState_AllowsEveryLocalArtifactConsumerCategory()
    {
        var root = NewRoot();
        var record = BuildArtifactRecord(
            "eligibility-positive",
            BuildRuntimeApprovedTrainingRecord("positive-a", "source-a"),
            BuildRuntimeApprovedTrainingRecord("positive-b", "source-b"));
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        record =
            await DesignConversionPostEvaluationEligibilityTestFixture
                .AppendEligibleRecordAsync(db, record);

        var eligibility =
            await db.EvaluatePostEvaluationTrainingEligibilityAsync(record);
        var approved = await db.QueryApprovedTrainingCandidatesAsync();
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            exportedAtUtc: DateTimeOffset.UnixEpoch.AddHours(1));
        var display = await db.QueryDisplayRowsAsync(
            new DesignConversionDpoDisplayQuery(IncludeBlocked: true));
        var measurement =
            await new DesignConversionTrainingMeasurementGate()
                .EvaluateLocalArtifactDbAsync(
                    db,
                    null,
                    MeasurementOptions("eligibility-positive"));

        Assert.True(eligibility.IsEligible, eligibility.Reason);
        Assert.Equal(2, approved.Value!.Count);
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(2, handoff.Value!.ApprovedCandidateCount);
        Assert.Equal(2, display.Value!.Rows.Count);
        Assert.True(measurement.IsSuccess, measurement.ErrorMessage);
        Assert.Equal(2, measurement.Value!.CandidateDecisions.Count);

        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value,
            "reviewer:eligibility",
            "committed matching candidates");
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        Assert.Equal(2, intake.Value!.AcceptedCandidateCount);

        using var index = JsonDocument.Parse(
            await File.ReadAllTextAsync(
                Path.Combine(root, "conversion-results.index.json")));
        Assert.Equal(
            2,
            index.RootElement
                .GetProperty("approvedTrainingCandidateCount")
                .GetInt32());
    }

    [Fact]
    public async Task AppendFailure_LeavesPreparedStateProvisionalAndCentrallyIneligible()
    {
        var root = NewRoot();
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            promotionUploader: null,
            recordsAuthorityMaxBytes: 1);
        var record =
            await DesignConversionPostEvaluationEligibilityTestFixture
                .PrepareProvisionalRecordAsync(
                    db,
                    BuildArtifactRecord(
                        "append-failure-provisional",
                        BuildRuntimeApprovedTrainingRecord(
                            "append-failure-candidate",
                            "append-failure-source")));

        var append = await db.AppendRecordAsync(record);
        var stateRef = record.Metadata[
            JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey];
        var state = await StateStore(root).ReadAsync(stateRef);
        var eligibility =
            await db.EvaluatePostEvaluationTrainingEligibilityAsync(record);

        Assert.False(append.IsSuccess);
        Assert.True(state.IsSuccess, state.ErrorMessage);
        Assert.NotNull(state.Value);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .PersistenceProvisional,
            state.Value!.PersistenceStatus);
        Assert.False(eligibility.IsEligible);
    }

    [Fact]
    public async Task FixtureReplayWithSameIdsButDifferentPayloadCannotCommitOrRebindOriginalDurableRow()
    {
        var root = NewRoot();
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var provisional =
            await DesignConversionPostEvaluationEligibilityTestFixture
                .PrepareProvisionalRecordAsync(
                    db,
                    BuildArtifactRecord(
                        "fixture-conflicting-replay",
                        BuildRuntimeApprovedTrainingRecord(
                            "fixture-conflicting-replay-candidate",
                            "fixture-conflicting-replay-source")));
        var firstAppend = await db.AppendRecordAsync(provisional);
        Assert.True(firstAppend.IsSuccess, firstAppend.ErrorMessage);
        var conflictingReplay = provisional with
        {
            Tier = "different-immutable-tier"
        };

        await Assert.ThrowsAnyAsync<Xunit.Sdk.XunitException>(
            () =>
                DesignConversionPostEvaluationEligibilityTestFixture
                    .CommitEligibleRecordAsync(
                        db,
                        conflictingReplay));

        var durable = await db.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery(
                RequestId: provisional.RequestId));
        Assert.True(durable.IsSuccess, durable.ErrorMessage);
        var original = Assert.Single(durable.Value!);
        Assert.Equal(firstAppend.Value!.Tier, original.Tier);
        Assert.NotEqual(conflictingReplay.Tier, original.Tier);
        Assert.True(
            JsonlDesignConversionLocalArtifactDb
                .HasExactDurableImmutableAppendIdentity(
                    firstAppend.Value,
                    original));

        var stateRef = provisional.Metadata[
            JsonDesignConversionPostEvaluationStateStore
                .StateRefMetadataKey];
        var state = await StateStore(root).ReadAsync(stateRef);
        var eligibility =
            await db.EvaluatePostEvaluationTrainingEligibilityAsync(
                original);
        Assert.True(state.IsSuccess, state.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .PersistenceProvisional,
            state.Value!.PersistenceStatus);
        Assert.False(eligibility.IsEligible);
    }

    [Theory]
    [InlineData(
        "missing",
        DesignConversionPostEvaluationTrainingEligibilityPolicy
            .CompletionBindingMissingReason)]
    [InlineData(
        "unknown-kind",
        DesignConversionPostEvaluationTrainingEligibilityPolicy
            .CompletionBindingKindInvalidReason)]
    [InlineData(
        "wrong-kind",
        DesignConversionPostEvaluationTrainingEligibilityPolicy
            .CompletionBindingKindInvalidReason)]
    [InlineData(
        "wrong-ref",
        DesignConversionPostEvaluationTrainingEligibilityPolicy
            .CompletionBindingRefMismatchReason)]
    public async Task CompletionBindingMustNameTheExactLocalArtifactAuthority(
        string mutation,
        string expectedReason)
    {
        var record = BuildArtifactRecord(
            "completion-binding-" + mutation,
            BuildRuntimeApprovedTrainingRecord(
                "completion-binding-candidate-" + mutation,
                "source-a"));
        var stateRef =
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                "completion-binding-" + mutation);
        record = WithPostEvaluationMarkers(record, stateRef);
        var state = InitialState(stateRef) with
        {
            PersistenceStatus =
                JsonDesignConversionPostEvaluationStateStore
                    .PersistenceCommitted,
            ArtifactRecordId = record.RecordId,
            CompletionBindingKind = mutation switch
            {
                "missing" => string.Empty,
                "unknown-kind" => "unknown_completion_authority",
                "wrong-kind" =>
                    JsonDesignConversionPostEvaluationStateStore
                        .CompletionBindingVisualCostLedgerRecord,
                _ => JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord
            },
            CompletionBindingRef = mutation switch
            {
                "missing" => string.Empty,
                "wrong-ref" => "different-artifact-record",
                _ => record.RecordId
            },
            UpdatedAtUtc = InitialState(stateRef).UpdatedAtUtc.AddSeconds(1),
            Version = 2
        };
        var store = new CountingStateStore([state]);
        var result =
            await new DesignConversionPostEvaluationTrainingEligibilityPolicy(
                    store)
                .EvaluateAsync(record);

        Assert.False(result.IsEligible);
        Assert.Equal(expectedReason, result.Reason);
    }

    [Fact]
    public async Task EvaluateMany_UsesOneStateInventoryAndNoPerRecordReads()
    {
        var first = BuildArtifactRecord(
            "batch-first",
            BuildRuntimeApprovedTrainingRecord(
                "batch-first-candidate",
                "source-a"));
        var second = BuildArtifactRecord(
            "batch-second",
            BuildRuntimeApprovedTrainingRecord(
                "batch-second-candidate",
                "source-b"));
        var firstState = CommittedStateFor(first, "batch-first");
        var secondState = CommittedStateFor(second, "batch-second");
        first = WithPostEvaluationMarkers(first, firstState.StateRef);
        second = WithPostEvaluationMarkers(second, secondState.StateRef);
        var store = new CountingStateStore([firstState, secondState]);

        var results =
            await new DesignConversionPostEvaluationTrainingEligibilityPolicy(
                    store)
                .EvaluateManyAsync([first, second]);

        Assert.All(results, result => Assert.True(result.IsEligible));
        Assert.Equal(1, store.ListCalls);
        Assert.Equal(0, store.ReadCalls);
    }

    [Fact]
    public async Task LocalReadbackSnapshot_UsesOneInventoryAndPreservesRecordEligibilityPairing()
    {
        var root = NewRoot();
        var first = BuildArtifactRecord(
            "snapshot-readback-first",
            BuildRuntimeApprovedTrainingRecord(
                "snapshot-readback-first-candidate",
                "snapshot-readback-source-a"));
        var second = BuildArtifactRecord(
            "snapshot-readback-second",
            BuildRuntimeApprovedTrainingRecord(
                "snapshot-readback-second-candidate",
                "snapshot-readback-source-b"));
        var firstState = CommittedStateFor(
            first,
            "snapshot-readback-first");
        var missingSecondStateRef =
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                "snapshot-readback-second");
        first = WithPostEvaluationMarkers(first, firstState.StateRef);
        second = WithPostEvaluationMarkers(
            second,
            missingSecondStateRef);
        var store = new CountingStateStore([firstState]);
        var db = InstrumentedDb(root, store);
        Assert.True((await db.AppendRecordAsync(first)).IsSuccess);
        Assert.True((await db.AppendRecordAsync(second)).IsSuccess);
        store.ResetCounts();

        var response =
            await new DesignConversionDpoUsefulnessApi(db)
                .QueryLocalArtifactReadbackAsync();

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(2, response.Value!.Rows.Count);
        var firstRow = Assert.Single(
            response.Value.Rows,
            row => row.RecordId == first.RecordId);
        var secondRow = Assert.Single(
            response.Value.Rows,
            row => row.RecordId == second.RecordId);
        Assert.Equal(
            bool.TrueString,
            firstRow.BoundaryMetadata[
                "post_evaluation_training_eligible"]);
        Assert.Equal(
            bool.FalseString,
            secondRow.BoundaryMetadata[
                "post_evaluation_training_eligible"]);
        Assert.Equal(1, store.ListCalls);
        Assert.Equal(0, store.ReadCalls);
    }

    [Fact]
    public async Task MeasurementSnapshot_UsesOneInventoryAndIncludesOnlyPositionallyEligibleRecord()
    {
        var root = NewRoot();
        var first = BuildArtifactRecord(
            "snapshot-measurement-first",
            BuildRuntimeApprovedTrainingRecord(
                "snapshot-measurement-first-candidate",
                "snapshot-measurement-source-a"));
        var second = BuildArtifactRecord(
            "snapshot-measurement-second",
            BuildRuntimeApprovedTrainingRecord(
                "snapshot-measurement-second-candidate",
                "snapshot-measurement-source-b"));
        var firstState = CommittedStateFor(
            first,
            "snapshot-measurement-first");
        first = WithPostEvaluationMarkers(first, firstState.StateRef);
        second = WithPostEvaluationMarkers(
            second,
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                "snapshot-measurement-second"));
        var store = new CountingStateStore([firstState]);
        var db = InstrumentedDb(root, store);
        Assert.True((await db.AppendRecordAsync(first)).IsSuccess);
        Assert.True((await db.AppendRecordAsync(second)).IsSuccess);
        store.ResetCounts();

        var result =
            await new DesignConversionTrainingMeasurementGate()
                .EvaluateLocalArtifactDbAsync(
                    db,
                    null,
                    MeasurementOptions(
                        "snapshot-measurement-pairing"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var decision = Assert.Single(result.Value!.CandidateDecisions);
        Assert.Equal(
            "snapshot-measurement-first-candidate",
            decision.RecordId);
        Assert.Equal(1, store.ListCalls);
        Assert.Equal(0, store.ReadCalls);
    }

    [Fact]
    public async Task TrainingIntakeResolution_BatchesDistinctRecordsOnceForDuplicateAndMissingRefs()
    {
        var root = NewRoot();
        var first = BuildArtifactRecord(
            "snapshot-intake-first",
            BuildRuntimeApprovedTrainingRecord(
                "snapshot-intake-first-candidate",
                "snapshot-intake-source-a"));
        var second = BuildArtifactRecord(
            "snapshot-intake-second",
            BuildRuntimeApprovedTrainingRecord(
                "snapshot-intake-second-candidate",
                "snapshot-intake-source-b"));
        var firstState = CommittedStateFor(first, "snapshot-intake-first");
        var secondState =
            CommittedStateFor(second, "snapshot-intake-second");
        first = WithPostEvaluationMarkers(first, firstState.StateRef);
        second = WithPostEvaluationMarkers(second, secondState.StateRef);
        var store = new CountingStateStore([firstState, secondState]);
        var db = InstrumentedDb(root, store);
        Assert.True((await db.AppendRecordAsync(first)).IsSuccess);
        Assert.True((await db.AppendRecordAsync(second)).IsSuccess);
        var handoff =
            await db.ExportApprovedRuntimeDpoCandidatesAsync(
                exportedAtUtc: DateTimeOffset.UnixEpoch);
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(2, handoff.Value!.Rows.Count);
        var firstRow = Assert.Single(
            handoff.Value.Rows,
            row => row.LocalArtifactRecordId == first.RecordId);
        var duplicateOnly = handoff.Value with
        {
            ApprovedCandidateCount = 2,
            Rows = [firstRow, firstRow]
        };
        store.ResetCounts();

        var duplicateResult = await db.CreateTrainingIntakeAsync(
            duplicateOnly,
            "reviewer:snapshot-intake",
            "duplicate refs share one eligibility snapshot",
            DateTimeOffset.UnixEpoch.AddMinutes(1));

        Assert.True(
            duplicateResult.IsSuccess,
            duplicateResult.ErrorMessage);
        Assert.Equal(2, duplicateResult.Value!.AcceptedCandidateCount);
        Assert.Equal(1, store.ListCalls);
        Assert.Equal(0, store.ReadCalls);

        var missingRow = firstRow with
        {
            LocalArtifactRecordId = "missing-local-artifact-record",
            CandidateId = "missing-candidate",
            TrainingRecordId = "missing-training-record"
        };
        var duplicateAndMissing = handoff.Value with
        {
            ApprovedCandidateCount = 3,
            Rows = [firstRow, firstRow, missingRow]
        };
        store.ResetCounts();

        var missingResult = await db.CreateTrainingIntakeAsync(
            duplicateAndMissing,
            "reviewer:snapshot-intake",
            "missing ref must fail closed",
            DateTimeOffset.UnixEpoch.AddMinutes(2));

        Assert.False(missingResult.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_TRAINING_INTAKE_POST_EVALUATION_INELIGIBLE",
            missingResult.ErrorCode);
        Assert.Contains(
            "local_artifact_record_not_found:missing-training-record",
            missingResult.ErrorMessage,
            StringComparison.Ordinal);
        Assert.Equal(1, store.ListCalls);
        Assert.Equal(0, store.ReadCalls);
    }

    [Fact]
    public async Task ConsumerSnapshots_FailClosedWhenInventoryFailsAndOnCancellation()
    {
        var root = NewRoot();
        var record = BuildArtifactRecord(
            "snapshot-fail-closed",
            BuildRuntimeApprovedTrainingRecord(
                "snapshot-fail-closed-candidate",
                "snapshot-fail-closed-source"));
        var state = CommittedStateFor(record, "snapshot-fail-closed");
        record = WithPostEvaluationMarkers(record, state.StateRef);
        var setupStore = new CountingStateStore([state]);
        var setupDb = InstrumentedDb(root, setupStore);
        Assert.True((await setupDb.AppendRecordAsync(record)).IsSuccess);

        var failureStore = new CountingStateStore(
            [state],
            failInventory: true);
        var failureDb = InstrumentedDb(root, failureStore);
        var readback =
            await new DesignConversionDpoUsefulnessApi(failureDb)
                .QueryLocalArtifactReadbackAsync();
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(
            bool.FalseString,
            Assert.Single(readback.Value!.Rows)
                .BoundaryMetadata[
                    "post_evaluation_training_eligible"]);
        Assert.Equal(1, failureStore.ListCalls);
        Assert.Equal(0, failureStore.ReadCalls);

        failureStore.ResetCounts();
        var measurement =
            await new DesignConversionTrainingMeasurementGate()
                .EvaluateLocalArtifactDbAsync(
                    failureDb,
                    null,
                    MeasurementOptions(
                        "snapshot-inventory-failure"));
        Assert.True(measurement.IsSuccess, measurement.ErrorMessage);
        Assert.Empty(measurement.Value!.CandidateDecisions);
        Assert.Equal(1, failureStore.ListCalls);
        Assert.Equal(0, failureStore.ReadCalls);

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancelledReadback =
            await new DesignConversionDpoUsefulnessApi(failureDb)
                .QueryLocalArtifactReadbackAsync(ct: cts.Token);
        var cancelledMeasurement =
            await new DesignConversionTrainingMeasurementGate()
                .EvaluateLocalArtifactDbAsync(
                    failureDb,
                    null,
                    MeasurementOptions(
                        "snapshot-cancelled"),
                    cts.Token);
        Assert.False(cancelledReadback.IsSuccess);
        Assert.Equal("CANCELLED", cancelledReadback.ErrorCode);
        Assert.False(cancelledMeasurement.IsSuccess);
        Assert.Equal("CANCELLED", cancelledMeasurement.ErrorCode);
    }

    [Fact]
    public async Task EvaluateMany_DoesNotDeadlockWhenCallerHasNonPumpingSyncContext()
    {
        var record = BuildArtifactRecord(
            "sync-context",
            BuildRuntimeApprovedTrainingRecord(
                "sync-context-candidate",
                "source-a"));
        var state = CommittedStateFor(record, "sync-context");
        record = WithPostEvaluationMarkers(record, state.StateRef);
        var store = new CountingStateStore(
            [state],
            completeInventoryAsynchronously: true);
        var policy =
            new DesignConversionPostEvaluationTrainingEligibilityPolicy(store);

        var operation = Task.Run(() =>
        {
            SynchronizationContext.SetSynchronizationContext(
                new NonPumpingSynchronizationContext());
            return policy.EvaluateManyAsync([record]).GetAwaiter().GetResult();
        });
        var completed = await Task.WhenAny(
            operation,
            Task.Delay(TimeSpan.FromSeconds(5)));

        Assert.Same(operation, completed);
        Assert.True((await operation).Single().IsEligible);
    }

    [Fact]
    public async Task DerivedIndexCountRefreshesAfterStateOnlyEligibilityTransition()
    {
        var root = NewRoot();
        var record = BuildArtifactRecord(
            "index-state-transition",
            BuildRuntimeApprovedTrainingRecord(
                "index-state-transition-candidate",
                "source-a"));
        var stateRef =
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                "index-state-transition");
        record = WithPostEvaluationMarkers(record, stateRef);
        var store = StateStore(root);
        var provisional = await AssertWrite(store, InitialState(stateRef));
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        Assert.True((await db.AppendRecordAsync(record)).IsSuccess);
        Assert.True((await db.ListRecordsAsync()).IsSuccess);
        Assert.Equal(0, await ReadApprovedCandidateCountAsync(root));

        var committed = await store.WriteAsync(provisional with
        {
            PersistenceStatus =
                JsonDesignConversionPostEvaluationStateStore
                    .PersistenceCommitted,
            ArtifactRecordId = record.RecordId,
            CompletionBindingKind =
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
            CompletionBindingRef = record.RecordId,
            UpdatedAtUtc = provisional.UpdatedAtUtc.AddSeconds(1),
            Version = provisional.Version
        });
        Assert.True(committed.IsSuccess, committed.ErrorMessage);
        Assert.True((await db.ListRecordsAsync()).IsSuccess);

        Assert.Equal(1, await ReadApprovedCandidateCountAsync(root));
    }

    [Fact]
    public async Task StableEligibilitySnapshot_DoesNotRewriteCurrentDerivedIndex()
    {
        var root = NewRoot();
        var setup = new JsonlDesignConversionLocalArtifactDb(root);
        await DesignConversionPostEvaluationEligibilityTestFixture
            .AppendEligibleRecordAsync(
                setup,
                BuildArtifactRecord(
                    "index-stable-no-rewrite",
                    BuildRuntimeApprovedTrainingRecord(
                        "index-stable-no-rewrite-candidate",
                        "source-a")));
        Assert.True((await setup.ListRecordsAsync()).IsSuccess);
        Assert.Equal(1, await ReadApprovedCandidateCountAsync(root));

        var indexPath =
            Path.Combine(root, "conversion-results.index.json");
        var beforeBytes = await File.ReadAllBytesAsync(indexPath);
        var writeAttempts = 0;
        var reader = new JsonlDesignConversionLocalArtifactDb(
            root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            derivedIndexWriteObserverForTesting:
                () => Interlocked.Increment(ref writeAttempts));

        var first = await reader.ListRecordsAsync();
        var second = await reader.ListRecordsAsync();
        var afterBytes = await File.ReadAllBytesAsync(indexPath);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(0, Volatile.Read(ref writeAttempts));
        Assert.Equal(beforeBytes, afterBytes);
        Assert.Equal(1, await ReadApprovedCandidateCountAsync(root));
    }

    [Theory]
    [InlineData("provisional", DesignConversionPostEvaluationTrainingEligibilityPolicy.StateNotCommittedReason)]
    [InlineData("quarantined", DesignConversionPostEvaluationTrainingEligibilityPolicy.StateNotCommittedReason)]
    [InlineData("missing-state", DesignConversionPostEvaluationTrainingEligibilityPolicy.StateMissingReason)]
    [InlineData("corrupt-state", DesignConversionPostEvaluationTrainingEligibilityPolicy.StateReadFailedReason)]
    [InlineData("noncanonical-ref", DesignConversionPostEvaluationTrainingEligibilityPolicy.StateRefInvalidReason)]
    [InlineData("artifact-id-mismatch", DesignConversionPostEvaluationTrainingEligibilityPolicy.ArtifactRecordIdMismatchReason)]
    [InlineData("incomplete-marker-set", DesignConversionPostEvaluationTrainingEligibilityPolicy.MarkerSetIncompleteReason)]
    [InlineData("misleading-training-eligible", DesignConversionPostEvaluationTrainingEligibilityPolicy.MetadataTrainingEligibilityInvalidReason)]
    [InlineData("missing-metadata", DesignConversionPostEvaluationTrainingEligibilityPolicy.MarkerSetIncompleteReason)]
    public async Task IncompleteOrNonDurableShapes_AreDeniedFailClosed(
        string mutation,
        string expectedReason)
    {
        var root = NewRoot();
        var record = BuildArtifactRecord(
            $"eligibility-{mutation}",
            BuildRuntimeApprovedTrainingRecord($"candidate-{mutation}", "source-a"));
        var stateRef =
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                $"eligibility-{mutation}");
        record = WithPostEvaluationMarkers(record, stateRef);
        var store = StateStore(root);

        switch (mutation)
        {
            case "provisional":
                await AssertWrite(store, InitialState(stateRef));
                break;
            case "quarantined":
            {
                var initial = await AssertWrite(store, InitialState(stateRef));
                await AssertWrite(
                    store,
                    initial with
                    {
                        PersistenceStatus =
                            JsonDesignConversionPostEvaluationStateStore
                                .PersistenceQuarantined,
                        LastErrorCode = "TEST_QUARANTINE",
                        UpdatedAtUtc = initial.UpdatedAtUtc.AddSeconds(1),
                        Version = initial.Version
                    });
                break;
            }
            case "missing-state":
                break;
            case "corrupt-state":
                Directory.CreateDirectory(
                    Path.Combine(
                        root,
                        JsonDesignConversionPostEvaluationStateStore.DirectoryName));
                await File.WriteAllTextAsync(
                    StatePath(root, stateRef),
                    "{\"schemaVersion\":\"wrong\",\"state\":");
                break;
            case "noncanonical-ref":
                record = WithPostEvaluationMarkers(record, "../not-canonical");
                break;
            case "artifact-id-mismatch":
                await WriteCommittedState(
                    root,
                    "different-artifact-record",
                    $"eligibility-{mutation}");
                break;
            case "incomplete-marker-set":
            {
                var metadata = record.Metadata
                    .Where(pair =>
                        !string.Equals(
                            pair.Key,
                            JsonDesignConversionPostEvaluationStateStore
                                .PersistenceStatusMetadataKey,
                            StringComparison.Ordinal))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                record = record with { Metadata = metadata };
                break;
            }
            case "misleading-training-eligible":
            {
                await WriteCommittedState(
                    root,
                    record.RecordId,
                    $"eligibility-{mutation}");
                var metadata = new Dictionary<string, string>(
                    record.Metadata,
                    StringComparer.Ordinal)
                {
                    [JsonDesignConversionPostEvaluationStateStore
                        .TrainingEligibleMetadataKey] = bool.TrueString
                };
                record = record with { Metadata = metadata };
                break;
            }
            case "missing-metadata":
                record = record with
                {
                    Metadata = new Dictionary<string, string>()
                };
                break;
            default:
                throw new InvalidOperationException(mutation);
        }

        var policy =
            new DesignConversionPostEvaluationTrainingEligibilityPolicy(store);
        var result = await policy.EvaluateAsync(record);

        Assert.False(result.IsEligible);
        Assert.Equal(expectedReason, result.Reason);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorCode));
    }

    [Fact]
    public async Task MissingMetadata_IsDeniedAcrossApprovalQueryExportDisplayIndexMeasurementAndReadback()
    {
        var root = NewRoot();
        var approvedRecord = BuildArtifactRecord(
            "missing-metadata-surfaces",
            BuildRuntimeApprovedTrainingRecord("missing-metadata-approved", "source-a"));
        var pendingRecord = BuildArtifactRecord(
            "missing-metadata-approval",
            BuildRuntimePendingTrainingRecord("missing-metadata-pending", "source-b"));
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        Assert.True((await db.AppendRecordAsync(approvedRecord)).IsSuccess);
        Assert.True((await db.AppendRecordAsync(pendingRecord)).IsSuccess);

        var approval = await db.ApproveRuntimeCreatedDpoCandidateAsync(
            pendingRecord.RecordId,
            pendingRecord.DpoCandidates[0].CandidateId,
            "reviewer:eligibility",
            "must remain blocked without durable state");
        var query = await db.QueryApprovedTrainingCandidatesAsync();
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync();
        var display = await db.QueryDisplayRowsAsync(
            new DesignConversionDpoDisplayQuery(IncludeBlocked: true));
        var measurement =
            await new DesignConversionTrainingMeasurementGate()
                .EvaluateLocalArtifactDbAsync(
                    db,
                    null,
                    MeasurementOptions("missing-metadata"));
        var readback =
            await new DesignConversionDpoUsefulnessApi(db)
                .QueryLocalArtifactReadbackAsync();

        Assert.False(approval.IsSuccess);
        Assert.Equal(
            DesignConversionPostEvaluationTrainingEligibilityPolicy
                .MarkerSetIncompleteErrorCode,
            approval.ErrorCode);
        Assert.Empty(query.Value!);
        Assert.Empty(handoff.Value!.Rows);
        Assert.Empty(display.Value!.Rows);
        Assert.True(measurement.IsSuccess, measurement.ErrorMessage);
        Assert.Empty(measurement.Value!.CandidateDecisions);
        Assert.Equal(
            DesignConversionTrainingMeasurementGateStatus.Blocked,
            measurement.Value.Status);
        Assert.All(
            readback.Value!.Rows,
            row =>
            {
                Assert.Equal(
                    bool.FalseString,
                    row.BoundaryMetadata[
                        "post_evaluation_training_eligible"]);
                Assert.Equal(
                    DesignConversionPostEvaluationTrainingEligibilityPolicy
                        .MarkerSetIncompleteReason,
                    row.BoundaryMetadata[
                        "post_evaluation_eligibility_reason"]);
            });

        using var index = JsonDocument.Parse(
            await File.ReadAllTextAsync(
                Path.Combine(root, "conversion-results.index.json")));
        Assert.Equal(
            0,
            index.RootElement
                .GetProperty("approvedTrainingCandidateCount")
                .GetInt32());
    }

    [Fact]
    public async Task RebuiltIndex_DoesNotCountProvisionalOrQuarantinedCandidatesAsApproved()
    {
        var root = NewRoot();
        var store = StateStore(root);
        var provisional = BuildArtifactRecord(
            "index-provisional",
            BuildRuntimeApprovedTrainingRecord(
                "index-provisional-candidate",
                "source-a"));
        var provisionalRef =
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                "index-provisional");
        await AssertWrite(store, InitialState(provisionalRef));
        provisional = WithPostEvaluationMarkers(
            provisional,
            provisionalRef);

        var quarantined = BuildArtifactRecord(
            "index-quarantined",
            BuildRuntimeApprovedTrainingRecord(
                "index-quarantined-candidate",
                "source-b"));
        var quarantinedRef =
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                "index-quarantined");
        var initialQuarantined =
            await AssertWrite(store, InitialState(quarantinedRef));
        await AssertWrite(
            store,
            initialQuarantined with
            {
                PersistenceStatus =
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceQuarantined,
                LastErrorCode = "TEST_QUARANTINE",
                UpdatedAtUtc =
                    initialQuarantined.UpdatedAtUtc.AddSeconds(1),
                Version = initialQuarantined.Version
            });
        quarantined = WithPostEvaluationMarkers(
            quarantined,
            quarantinedRef);

        var db = new JsonlDesignConversionLocalArtifactDb(root);
        Assert.True(
            (await db.AppendRecordAsync(provisional)).IsSuccess);
        Assert.True(
            (await db.AppendRecordAsync(quarantined)).IsSuccess);
        File.Delete(
            Path.Combine(root, "conversion-results.index.json"));

        var rebuilt =
            await new JsonlDesignConversionLocalArtifactDb(root)
                .ListRecordsAsync();
        Assert.True(rebuilt.IsSuccess, rebuilt.ErrorMessage);
        Assert.Equal(2, rebuilt.Value!.Count);

        using var index = JsonDocument.Parse(
            await File.ReadAllTextAsync(
                Path.Combine(root, "conversion-results.index.json")));
        Assert.Equal(
            0,
            index.RootElement
                .GetProperty("approvedTrainingCandidateCount")
                .GetInt32());
    }

    [Fact]
    public async Task QueueAfterExport_DeletedStateFailsBeforeIntakeJournalAppend()
    {
        var root = NewRoot();
        var record = BuildArtifactRecord(
            "queue-delete",
            BuildRuntimeApprovedTrainingRecord("queue-delete-a", "source-a"),
            BuildRuntimeApprovedTrainingRecord("queue-delete-b", "source-b"));
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        record =
            await DesignConversionPostEvaluationEligibilityTestFixture
                .AppendEligibleRecordAsync(db, record);
        var stateRef = record.Metadata[
            JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey];
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync();
        Assert.Equal(2, handoff.Value!.ApprovedCandidateCount);

        File.Delete(StatePath(root, stateRef));
        var intake = await db.CreateTrainingIntakeAsync(
            handoff.Value,
            "reviewer:eligibility",
            "state disappeared before queue");

        Assert.False(intake.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_TRAINING_INTAKE_POST_EVALUATION_INELIGIBLE",
            intake.ErrorCode);
        Assert.False(
            File.Exists(Path.Combine(root, "training-intake-requests.jsonl")));
    }

    [Fact]
    public async Task QueuedWhileCommitted_DeletedBeforeRunBlocksTrainerDependency()
    {
        var root = NewRoot();
        var record = BuildArtifactRecord(
            "run-delete",
            BuildRuntimeApprovedTrainingRecord("run-delete-a", "source-a"),
            BuildRuntimeApprovedTrainingRecord("run-delete-b", "source-b"));
        var setup = new JsonlDesignConversionLocalArtifactDb(root);
        record =
            await DesignConversionPostEvaluationEligibilityTestFixture
                .AppendEligibleRecordAsync(setup, record);
        var stateRef = record.Metadata[
            JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey];
        var handoff = await setup.ExportApprovedRuntimeDpoCandidatesAsync();
        var intake = await setup.CreateTrainingIntakeAsync(
            handoff.Value!,
            "reviewer:eligibility",
            "queue before state mutation");
        Assert.True(intake.IsSuccess, intake.ErrorMessage);

        var trainerInvoked = false;
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerBeforeInvokeObserverForTesting:
                () => File.Delete(StatePath(root, stateRef)),
            runTrainingIntakeRunnerInvokedObserverForTesting:
                () => trainerInvoked = true);

        var run = await db.RunTrainingIntakeAsync(
            intake.Value!.IntakeId,
            PromotionOptions("run-delete"));

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.False(trainerInvoked);
        Assert.False(run.Value!.TrainingRunnerInvoked);
        Assert.Contains(
            run.Value.BlockReasons,
            reason => reason.StartsWith(
                DesignConversionPostEvaluationTrainingEligibilityPolicy
                    .StateMissingReason,
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task EligibilityDeletedImmediatelyBeforePublication_BlocksUploader()
    {
        var fixture = await CreateQueuedPromotionFixture(
            "promotion-publication-delete");
        var terminalKinds = new List<DesignConversionPromotionPublicationKind>();
        var observerReached = false;
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            dpoPolicyBaseProvider: new DesignConversionDpoPolicyBaseTestProvider(),
            runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting:
                () =>
                {
                    observerReached = true;
                    File.Delete(fixture.StatePath);
                },
            promotionBeforeTerminalReturnObserverForTesting:
                terminalKinds.Add,
            legalTeacherAdmissionAuthority:
                CreateEvaluatedLegalTeacherCapability());

        var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
            () => db.RunTrainingIntakeAndPromoteLocalAsync(
            fixture.IntakeId,
            Path.Combine(fixture.Root, "promotion-output"),
            PromotionOptions("promotion-publication-delete")));

        Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
        Assert.True(
            observerReached,
            string.Join("|", promotion.Value!.BlockReasons));
        Assert.Empty(terminalKinds);
        Assert.False(promotion.Value.LocalPromotionReady);
        Assert.False(promotion.Value.ExternalCloudUploadAttempted);
        Assert.Null(promotion.Value!.Publication);
        Assert.Null(promotion.Value.PromotedLocalModel);
        Assert.Contains(
            "local_promotion_blocked_immediately_before_publication",
            promotion.Value.BlockReasons);
    }

    [Fact]
    public async Task EligibilityDeletedImmediatelyBeforeActivation_BlocksLoader()
    {
        var fixture = await CreateQueuedPromotionFixture(
            "promotion-activation-delete");
        var terminalKinds = new List<DesignConversionPromotionPublicationKind>();
        var observerReached = false;
        var activationInvoked = false;
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            dpoPolicyBaseProvider: new DesignConversionDpoPolicyBaseTestProvider(),
            runTrainingIntakeLocalPromotionBeforeActivationObserverForTesting:
                () =>
                {
                    observerReached = true;
                    File.Delete(fixture.StatePath);
                },
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting:
                () => activationInvoked = true,
            promotionBeforeTerminalReturnObserverForTesting:
                terminalKinds.Add,
            legalTeacherAdmissionAuthority:
                CreateEvaluatedLegalTeacherCapability());

        var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
            () => db.RunTrainingIntakeAndPromoteLocalAsync(
            fixture.IntakeId,
            Path.Combine(fixture.Root, "activation-output"),
            PromotionOptions("promotion-activation-delete")));

        Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
        Assert.True(
            observerReached,
            string.Join("|", promotion.Value!.BlockReasons));
        Assert.Equal(
            [DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt],
            terminalKinds);
        Assert.False(activationInvoked);
        Assert.False(promotion.Value.LocalPromotionReady);
        Assert.False(promotion.Value.ExternalCloudUploadAttempted);
        Assert.Null(promotion.Value!.PromotedLocalModel);
        Assert.NotNull(promotion.Value.Publication);
        var publication = promotion.Value.Publication!;
        Assert.Equal(
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            publication.PublicationKind);
        Assert.Equal(
            DesignConversionPromotionPublicationStatus.NotUploaded,
            publication.PublicationStatus);
        Assert.False(publication.Manifest.ModelServerUploadAttempted);
        Assert.False(publication.Manifest.ModelServerPromotionClaimed);
        Assert.True(publication.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal(
            "local_receipt_only",
            publication.Manifest.PromotionReceiptKind);
        Assert.StartsWith(
            DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
            publication.Manifest.PromotionUploadRef);
        Assert.NotNull(publication.UploadReceipt);
        var receipt = publication.UploadReceipt!;
        Assert.Equal(
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            receipt.PublicationKind);
        Assert.False(receipt.UploadAttempted);
        Assert.False(receipt.ExternalCloudUploadAttempted);
        Assert.False(receipt.UploadSucceeded);
        Assert.False(receipt.VerificationSucceeded);
        Assert.Equal(
            DesignConversionPromotionPublicationStatus.NotUploaded,
            receipt.PublicationStatus);
        Assert.Equal(
            new[]
            {
                UploadReceiptEvidenceSha256Ref(
                    "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_ONLY_PROMOTION_UPLOADER_CALLED"),
                UploadReceiptEvidenceSha256Ref(
                    "EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED"),
                UploadReceiptEvidenceSha256Ref(
                    "DESIGN_CONVERSION_TRAINING_INTAKE_EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED")
            }
            .Order(StringComparer.Ordinal)
            .ToArray(),
            receipt.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
            publication.Manifest.EvidenceRefs);
        Assert.Contains(
            "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED_LOCAL_RECEIPT",
            publication.Manifest.EvidenceRefs);
        Assert.Contains(
            "local_activation_blocked_post_evaluation_eligibility_change",
            promotion.Value.BlockReasons);
    }

    [Fact]
    public async Task ConfiguredUploaderCannotSelfMintLocalOnlyBudgetAuthorityOrAttemptRemoteUpload()
    {
        var fixture = await CreateQueuedPromotionFixture(
            "configured-uploader-cannot-self-mint-local-only");
        var uploader = new ConfiguredRemotePromotionUploader();
        var terminalKinds = new List<DesignConversionPromotionPublicationKind>();
        var activationInvoked = false;
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            promotionUploader: uploader,
            dpoPolicyBaseProvider: new DesignConversionDpoPolicyBaseTestProvider(),
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting:
                () => activationInvoked = true,
            promotionBeforeTerminalReturnObserverForTesting:
                terminalKinds.Add,
            legalTeacherAdmissionAuthority:
                CreateEvaluatedLegalTeacherCapability());

        var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
            () => db.RunTrainingIntakeAndPromoteLocalAsync(
                fixture.IntakeId,
                Path.Combine(fixture.Root, "configured-uploader-output"),
                PromotionOptions("configured-uploader-cannot-self-mint-local-only")));

        Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
        Assert.Equal(0, uploader.Count);
        Assert.False(activationInvoked);
        Assert.False(promotion.Value!.LocalPromotionReady);
        Assert.False(promotion.Value.ExternalCloudUploadAttempted);
        Assert.Null(promotion.Value.PromotedLocalModel);
        Assert.NotNull(promotion.Value.Publication);
        var publication = promotion.Value.Publication!;
        Assert.Equal(
            DesignConversionPromotionPublicationKind.Rejected,
            publication.PublicationKind);
        Assert.Equal(
            DesignConversionPromotionPublicationStatus.Rejected,
            publication.PublicationStatus);
        Assert.Contains(
            "BUDGET_TIER_POLICY_FAILED",
            publication.Decision.Reasons);
        Assert.Contains(
            "BUDGET_TIER_POLICY_FAILED",
            promotion.Value.BlockReasons);
        Assert.False(publication.Manifest.ModelServerUploadAttempted);
        Assert.False(publication.Manifest.ModelServerPromotionClaimed);
        Assert.False(publication.Manifest.LocalPromotionReceiptWritten);
        Assert.Null(publication.UploadReceipt);
        Assert.Equal(
            [DesignConversionPromotionPublicationKind.Rejected],
            terminalKinds);
    }

    [Fact]
    public async Task LocalReceiptOnlyCapabilityForDifferentUploaderFailsBeforeEffects()
    {
        var fixture = await CreateQueuedPromotionFixture(
            "local-receipt-capability-mismatch");
        var authorityUploader = new ConfiguredRemotePromotionUploader();
        var currentUploader = new ConfiguredRemotePromotionUploader();
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var terminalKinds = new List<DesignConversionPromotionPublicationKind>();
        var activationInvoked = false;
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            promotionUploader: currentUploader,
            dpoPolicyBaseProvider: provider,
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting:
                () => activationInvoked = true,
            promotionBeforeTerminalReturnObserverForTesting:
                terminalKinds.Add,
            legalTeacherAdmissionAuthority:
                CreateEvaluatedLegalTeacherCapability(),
            localReceiptOnlyPromotionAuthority: authorityUploader.Authority);

        var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
            () => db.RunTrainingIntakeAndPromoteLocalAsync(
                fixture.IntakeId,
                Path.Combine(fixture.Root, "capability-mismatch-output"),
                PromotionOptions("local-receipt-capability-mismatch")));

        Assert.False(promotion.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INVALID",
            promotion.ErrorCode);
        Assert.Equal(0, authorityUploader.Count);
        Assert.Equal(0, currentUploader.Count);
        Assert.Equal(0, provider.InvocationCount);
        Assert.False(activationInvoked);
        Assert.Empty(terminalKinds);
    }

    [Fact]
    public async Task LocalReceiptOnlyCapabilityRejectsRemoteReceiptWithoutModelServerClaim()
    {
        var fixture = await CreateQueuedPromotionFixture(
            "local-receipt-capability-rejects-remote");
        var uploader = new ConfiguredRemotePromotionUploader();
        var terminalKinds = new List<DesignConversionPromotionPublicationKind>();
        var activationInvoked = false;
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            promotionUploader: uploader,
            dpoPolicyBaseProvider: new DesignConversionDpoPolicyBaseTestProvider(),
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting:
                () => activationInvoked = true,
            promotionBeforeTerminalReturnObserverForTesting:
                terminalKinds.Add,
            legalTeacherAdmissionAuthority:
                CreateEvaluatedLegalTeacherCapability(),
            localReceiptOnlyPromotionAuthority: uploader.Authority);

        var promotion = await DesignConversionPromotionAdmissionTestScope.RunAsync(
            () => db.RunTrainingIntakeAndPromoteLocalAsync(
                fixture.IntakeId,
                Path.Combine(fixture.Root, "capability-remote-output"),
                PromotionOptions("local-receipt-capability-rejects-remote")));

        Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
        Assert.Equal(1, uploader.Count);
        Assert.False(activationInvoked);
        Assert.False(promotion.Value!.LocalPromotionReady);
        Assert.Null(promotion.Value.PromotedLocalModel);
        var publication = Assert.IsType<DesignConversionPromotionPublicationResult>(
            promotion.Value.Publication);
        Assert.Equal(
            DesignConversionPromotionPublicationKind.Rejected,
            publication.PublicationKind);
        Assert.Contains(
            "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_AUTHORITY_REJECTED_REMOTE_RECEIPT",
            publication.Decision.Reasons);
        Assert.False(publication.Manifest.ModelServerUploadAttempted);
        Assert.False(publication.Manifest.ModelServerPromotionClaimed);
        Assert.False(publication.Manifest.LocalPromotionReceiptWritten);
        Assert.Null(publication.UploadReceipt);
        Assert.Equal(
            [DesignConversionPromotionPublicationKind.Rejected],
            terminalKinds);
    }

    [Theory]
    [InlineData("DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INTERNAL_ISSUER")]
    [InlineData("sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")]
    public void LocalReceiptOnlyCapabilityStoresOnlyCanonicalIssuerEvidence(
        string issuerEvidenceRef)
    {
        var uploader = new ConfiguredRemotePromotionUploader();

        var issued = DesignConversionLocalReceiptOnlyPromotionUploadAuthority
            .Issue(uploader, issuerEvidenceRef);

        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var fingerprint = uploader.PreflightConfigurationFingerprintSha256();
        Assert.True(fingerprint.IsSuccess, fingerprint.ErrorMessage);
        var validation = issued.Value!.Validate(
            uploader,
            fingerprint.Value!,
            uploader.MaximumSerializedReceiptUtf8Bytes);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        Assert.Equal(issuerEvidenceRef, validation.Value!.IssuerEvidenceRef);
        Assert.Equal(0, uploader.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\r\n\t")]
    public void LocalReceiptOnlyCapabilityBlankIssuerEvidenceUsesDefaultIssuer(
        string? issuerEvidenceRef)
    {
        var uploader = new ConfiguredRemotePromotionUploader();

        var issued = DesignConversionLocalReceiptOnlyPromotionUploadAuthority
            .Issue(uploader, issuerEvidenceRef);

        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var fingerprint = uploader.PreflightConfigurationFingerprintSha256();
        Assert.True(fingerprint.IsSuccess, fingerprint.ErrorMessage);
        var validation = issued.Value!.Validate(
            uploader,
            fingerprint.Value!,
            uploader.MaximumSerializedReceiptUtf8Bytes);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        Assert.Equal(
            "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INTERNAL_ISSUER",
            validation.Value!.IssuerEvidenceRef);
        Assert.Equal(0, uploader.Count);
    }

    [Theory]
    [MemberData(nameof(UnsafeLocalReceiptOnlyIssuerEvidenceRefs))]
    public void LocalReceiptOnlyCapabilityRejectsUnsafeIssuerEvidenceBeforeMintOrPublication(
        string unsafeIssuerEvidenceRef)
    {
        var uploader = new ConfiguredRemotePromotionUploader();

        var issued = DesignConversionLocalReceiptOnlyPromotionUploadAuthority
            .Issue(uploader, unsafeIssuerEvidenceRef);

        Assert.False(issued.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_ISSUER_EVIDENCE_INVALID",
            issued.ErrorCode);
        Assert.Equal(
            "DesignConversion local receipt-only upload authority issuer evidence is invalid.",
            issued.ErrorMessage);
        Assert.DoesNotContain(
            unsafeIssuerEvidenceRef,
            issued.ErrorMessage,
            StringComparison.Ordinal);
        Assert.Null(issued.Value);
        Assert.Equal(0, uploader.Count);
    }

    public static IEnumerable<object[]> UnsafeLocalReceiptOnlyIssuerEvidenceRefs()
    {
        yield return
        [
            " DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INTERNAL_ISSUER"
        ];
        yield return
        [
            "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INTERNAL_ISSUER "
        ];
        yield return
        [
            "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY INTERNAL_ISSUER"
        ];
        yield return
        [
            "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY\nINTERNAL_ISSUER"
        ];
        yield return
        [
            "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY\u0001INTERNAL_ISSUER"
        ];
        yield return ["sk-live-local-receipt-secret"];
        yield return ["C:\\Users\\luba\\secrets\\issuer.txt"];
        yield return ["../secrets/issuer"];
        yield return
        [
            "{\"issuer\":\"DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INTERNAL_ISSUER\"}"
        ];
        yield return
        [
            "<issuer>DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INTERNAL_ISSUER</issuer>"
        ];
        yield return
        [
            "design_conversion_local_receipt_only_upload_authority_internal_issuer"
        ];
        yield return ["sha256:" + "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"];
        yield return ["DESIGN_CONVERSION_" + new string('A', 512)];
        yield return
        [
            "\"DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INTERNAL_ISSUER\""
        ];
    }

    private static DesignConversionTrainingPipelineOptions MeasurementOptions(
        string runId)
        => new(
            runId,
            BatchSize: 2,
            RequireDatasetCountGate: false,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: runId,
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight:
                    DesignConversionDpoDatasetPreflight
                        .LocalFixtureTrainingCandidateOnly()));

    private static DesignConversionTrainingPipelineOptions PromotionOptions(
        string runId)
        => new(
            RunId: runId,
            BatchSize: 1,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: runId,
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight:
                    DesignConversionDpoDatasetPreflight
                        .LocalTrainingIntakeSourceManifestDisjointProof(
                            [$"{runId}:controlled_source_disjoint_intake"])),
            MeasurementGateRequest:
                new DesignConversionMeasurementGateRequest(
                    GateId: $"{runId}:gate",
                    UnitId: DesignConversionDpoUnits.CriticUnit,
                    HeldOutMetrics:
                        new DesignConversionHeldOutMeasurementMetrics(
                            HeldOutRecordCount: 1,
                            HeldOutQualityScore: 0.95,
                            RuntimeMs: 100,
                            CostUsd: 0.01m,
                            PeakRamMb: 64,
                            EvidenceRefs:
                            [
                                $"ref:{runId}:heldout:pass"
                            ]),
                    IntegrityProof:
                        new DesignConversionMeasurementIntegrityProof(
                            SourceDisjoint: true,
                            LeakageDetected: false,
                            HiddenDeterminismDetected: false,
                            AblationPassed: true,
                            EvidenceRefs:
                            [
                                $"ref:{runId}:integrity:pass"
                            ]),
                    Thresholds:
                        new DesignConversionMeasurementGateThresholds(
                            RequiredHeldOutQualityScore: 0.90,
                            MaxRuntimeMs: 1000,
                            MaxCostUsd: 1m,
                            MaxPeakRamMb: 512)),
            DpoPolicyBridgeOptions:
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 1,
                    DpoStepCount: 5)
                {
                    DpoConvergenceThreshold = 0f
                });

    private static string UploadReceiptEvidenceSha256Ref(string evidenceRef)
        => "UPLOAD_RECEIPT_EVIDENCE_SHA256:" +
           Convert.ToHexString(
                   System.Security.Cryptography.SHA256.HashData(
                       System.Text.Encoding.UTF8.GetBytes(evidenceRef)))
               .ToLowerInvariant();

    private static async Task<QueuedPromotionFixture>
        CreateQueuedPromotionFixture(string scope)
    {
        var root = NewRoot();
        var record = BuildArtifactRecord(
            scope,
            BuildPromotionReadyRuntimePendingTrainingRecord(
                $"{scope}-train",
                $"{scope}-source-train",
                DesignConversionDpoUnits.ImproverUnit,
                usefulness: 0.97,
                rejectedHtml: "<main class=\"candidate-r\">R</main>",
                rejectedCss: ".candidate-r{display:block;}"),
            BuildPromotionReadyRuntimePendingTrainingRecord(
                $"{scope}-heldout",
                $"{scope}-source-heldout",
                DesignConversionDpoUnits.ImproverUnit,
                usefulness: 0.96,
                rejectedHtml: "<main class=\"candidate-s\">S</main>",
                rejectedCss: ".candidate-s{display:block;}"));
        var setup = new JsonlDesignConversionLocalArtifactDb(root);
        record =
            await DesignConversionPostEvaluationEligibilityTestFixture
                .AppendEligibleRecordAsync(setup, record);
        var stateRef = record.Metadata[
            JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey];
        foreach (var candidate in record.DpoCandidates)
        {
            var approval = await setup.ApproveRuntimeCreatedDpoCandidateAsync(
                record.RecordId,
                candidate.CandidateId,
                "reviewer:eligibility",
                $"{scope} promotion fixture post-evaluation eligible approval",
                DateTimeOffset.UnixEpoch.AddMinutes(2));
            Assert.True(approval.IsSuccess, approval.ErrorMessage);
        }

        var handoff = await setup.ExportApprovedRuntimeDpoCandidatesAsync();
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        var intake = await setup.CreateTrainingIntakeAsync(
            handoff.Value!,
            "reviewer:eligibility",
            "queue for promotion revalidation");
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        return new QueuedPromotionFixture(
            root,
            intake.Value!.IntakeId,
            StatePath(root, stateRef));
    }

    private static DesignConversionDpoTrainingRecord
        BuildRuntimeApprovedTrainingRecord(
            string id,
            string sourceId)
    {
        var record = WithPersistedRuntimeArtifactRefs(
            BuildTrainingRecord(id, sourceId),
            id);
        return WithRuntimeMetadata(
            record,
            approved: true);
    }

    private static DesignConversionDpoTrainingRecord
        BuildRuntimePendingTrainingRecord(
            string id,
            string sourceId)
    {
        var record = WithPersistedRuntimeArtifactRefs(
            BuildTrainingRecord(id, sourceId),
            id);
        return WithRuntimeMetadata(
            record,
            approved: false);
    }

    private static DesignConversionDpoTrainingRecord
        BuildPromotionReadyRuntimePendingTrainingRecord(
            string id,
        string sourceId,
        string unitId,
        double usefulness,
        string rejectedHtml,
        string rejectedCss)
    {
        var trainingRecord = BuildTrainingRecord(
            id,
            sourceId,
            unitId,
            usefulness) with
        {
            SourceStructurePayload = BuildRuntimeSourceStructureFixturePayload(id),
            TeacherProvenance = CreateQwenOllamaTeacherProvenance(
                $"{id}:teacher-request")
        };
        var record = WithPersistedRuntimeArtifactRefs(
            trainingRecord,
            id);
        record = WithRuntimeMetadata(
            record,
            approved: false);
        return AttachInlineOptimizerPayloadCustody(
            record,
            rejectedHtml,
            rejectedCss);
    }

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string sourceId)
        => BuildTrainingRecord(
            id,
            sourceId,
            DesignConversionDpoUnits.CriticUnit,
            usefulness: 0.9);

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string sourceId,
        string unitId,
        double usefulness)
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(
            id,
            unitId,
            usefulness: usefulness);
        example = example with
        {
            Provenance = example.Provenance with
            {
                SourceRequestId = $"request:{sourceId}",
                SourceManifestRef = $"manifest://eligibility/{sourceId}",
                Metadata = new Dictionary<string, string>
                {
                    ["source_design_id"] = sourceId
                }
            }
        };
        var build = new DesignConversionDpoGenerator()
            .BuildTrainReadyRecords([example]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        return Assert.Single(build.Value!.AcceptedRecords);
    }

    private static DesignConversionDpoTrainingRecord
        AttachInlineOptimizerPayloadCustody(
            DesignConversionDpoTrainingRecord record,
            string rejectedHtml,
            string rejectedCss)
    {
        var custodyMetadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            ["runtime_teacher_authored_candidate"] = bool.TrueString
        };
        var custodyRecord = record with
        {
            Provenance = record.Provenance with { Metadata = custodyMetadata }
        };
        var materialized = custodyRecord with
        {
            Chosen = custodyRecord.Chosen with
            {
                Html = "<main class=\"candidate-c\">C</main>",
                Css = ".candidate-c{display:grid;gap:16px;}"
            },
            Rejected = custodyRecord.Rejected with
            {
                Html = rejectedHtml,
                Css = rejectedCss
            },
            PayloadCustodySecret = null
        };
        var item = DesignConversionTrainingPipeline.ToCanonicalTrainingItem(
            materialized);
        var promptSha256 = DesignConversionDpoGenerator.Sha256Ref(item.Prompt);
        var chosenSha256 = DesignConversionDpoGenerator.Sha256Ref(
            item.ChosenResponse);
        var rejectedSha256 = DesignConversionDpoGenerator.Sha256Ref(
            item.RejectedResponse);
        var governanceSha256 = DesignConversionDpoGenerator
            .BuildPayloadCustodyGovernanceSha256(custodyRecord);
        var bindingSha256 = DesignConversionDpoGenerator.BuildPayloadCustodyBinding(
            custodyRecord,
            promptSha256,
            chosenSha256,
            rejectedSha256,
            governanceSha256);
        var metadata = new Dictionary<string, string>(
            custodyRecord.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            [DesignConversionDpoGenerator.PayloadCustodyRequiredMetadataKey] =
                bool.TrueString,
            [DesignConversionDpoGenerator.PayloadCustodyVersionMetadataKey] =
                DesignConversionDpoGenerator.PayloadCustodyVersion,
            [DesignConversionDpoGenerator.PayloadCustodyPromptSha256MetadataKey] =
                promptSha256,
            [DesignConversionDpoGenerator.PayloadCustodyChosenSha256MetadataKey] =
                chosenSha256,
            [DesignConversionDpoGenerator.PayloadCustodyRejectedSha256MetadataKey] =
                rejectedSha256,
            [DesignConversionDpoGenerator.PayloadCustodyGovernanceSha256MetadataKey] =
                governanceSha256,
            [DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey] =
                bindingSha256,
            ["payload_custody_raw_in_public_record"] = bool.FalseString
        };
        return custodyRecord with
        {
            Provenance = custodyRecord.Provenance with { Metadata = metadata },
            PayloadCustodySecret = new DesignConversionDpoPayloadCustodySecret(
                DesignConversionDpoGenerator.PayloadCustodyVersion,
                item.Prompt,
                item.ChosenResponse,
                item.RejectedResponse,
                promptSha256,
                chosenSha256,
                rejectedSha256,
                governanceSha256,
                bindingSha256)
        };
    }

    private static string BuildRuntimeSourceStructureFixturePayload(string id)
        => JsonSerializer.Serialize(
            new
            {
                schema = "design-conversion-runtime-source-structure-v1",
                tree = new[] { "frame", "candidate" },
                generated_binding =
                    $"artifact://design-conversion/{id}/generated",
                incumbent_binding =
                    $"artifact://design-conversion/{id}/incumbent",
                redacted = true
            },
            DesignConversionCheckpointJsonOptions.Standard);

    private static TrainingTeacherProvenance CreateQwenOllamaTeacherProvenance(
        string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            DesignConversionLegalTeacherCapabilityTestFactory.DefaultModelId,
            DesignConversionLegalTeacherCapabilityTestFactory
                .DefaultExpectedOllamaManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = LegalTeacherModelLayerDigest
        };

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateEvaluatedLegalTeacherCapability()
        => DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
            expectedOllamaModelLayerDigest: LegalTeacherModelLayerDigest);

    private static DesignConversionDpoTrainingRecord WithRuntimeMetadata(
        DesignConversionDpoTrainingRecord record,
        bool approved)
    {
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["split"] = "runtime_training_candidate_review",
            ["review_required"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["approval_status"] = approved ? "approved" : "pending",
            ["review_status"] = approved
                ? JsonlDesignConversionLocalArtifactDb
                    .RuntimeCandidateReviewApprovedStatus
                : JsonlDesignConversionLocalArtifactDb
                    .RuntimeCandidateReviewPendingStatus,
            ["approved_for_training"] = approved.ToString(),
            ["training_candidate_only"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["production_dpo"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString
        };
        if (approved)
        {
            metadata["approved_by_ref"] = "reviewer:eligibility";
            metadata["approval_reason"] = "durable eligibility test approval";
            metadata["approved_at_utc"] =
                DateTimeOffset.UnixEpoch.AddMinutes(1).ToString("O");
        }

        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata }
        };
    }

    private static DesignConversionDpoTrainingRecord
        WithPersistedRuntimeArtifactRefs(
            DesignConversionDpoTrainingRecord record,
            string artifactId)
        => record with
        {
            Chosen = record.Chosen with
            {
                Html =
                    $"artifact://design-conversion/{artifactId}/chosen.html",
                Css =
                    $"artifact://design-conversion/{artifactId}/chosen.css",
                QaResults =
                [
                    CreateRuntimeArtifactQaResult(
                        $"{artifactId}:chosen",
                        0.92,
                        true)
                ]
            },
            Rejected = record.Rejected with
            {
                Html =
                    $"artifact://design-conversion/{artifactId}/rejected.html",
                Css =
                    $"artifact://design-conversion/{artifactId}/rejected.css",
                QaResults =
                [
                    CreateRuntimeArtifactQaResult(
                        $"{artifactId}:rejected",
                        0.42,
                        false)
                ]
            }
        };

    private static RenderQaResultRecord CreateRuntimeArtifactQaResult(
        string artifactId,
        double score,
        bool passed)
    {
        var resolution =
            new RenderQaResolution("desktop", 1440, 900, 1);
        var expected = new RenderQaImageRecord(
            ImageId: $"{artifactId}:expected",
            Label: "expected",
            Resolution: resolution,
            Uri:
                $"artifact://design-conversion/{artifactId}/expected.png",
            Sha256: $"sha256:{artifactId}:expected",
            Metadata: new Dictionary<string, string>());
        var actual = new RenderQaImageRecord(
            ImageId: $"{artifactId}:actual",
            Label: "actual",
            Resolution: resolution,
            Uri:
                $"artifact://design-conversion/{artifactId}/actual.png",
            Sha256: $"sha256:{artifactId}:actual",
            Metadata: new Dictionary<string, string>());
        var verdict = new VisualVerdict(
            VerdictId: $"{artifactId}:verdict",
            Score: score,
            Defects: [],
            EvidenceRefs:
            [
                $"artifact://design-conversion/{artifactId}/comparison-evidence"
            ],
            Passed: passed);
        return new RenderQaResultRecord(
            ResultId: $"{artifactId}:qa-result",
            RequestId: $"{artifactId}:qa-request",
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            Metrics: new Dictionary<string, double>
            {
                ["visual_distance"] = 1.0 - score
            },
            Warnings: []);
    }

    private static DesignConversionLocalArtifactDbRecord BuildArtifactRecord(
        string recordId,
        params DesignConversionDpoTrainingRecord[] trainingRecords)
        => new(
            RecordId: recordId,
            DatabaseKind:
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: $"job:{recordId}",
            RequestId: $"request:{recordId}",
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "local-fixture",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] =
                    $"manifest://eligibility/{recordId}",
                ["design_tree_payload_ref"] =
                    $"artifact://design-conversion/{recordId}/design-tree.json",
                ["original_tree_payload_ref"] =
                    $"artifact://design-conversion/{recordId}/original-tree.json",
                ["source_artifact:design_tree"] =
                    $"artifact://design-conversion/{recordId}/design-tree.json",
                ["source_artifact:original_tree"] =
                    $"artifact://design-conversion/{recordId}/original-tree.json"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion",
                HtmlRef: "sha256:eligibility-html",
                HtmlSha256: "sha256:eligibility-html",
                CssRef: "sha256:eligibility-css",
                CssSha256: "sha256:eligibility-css",
                Metadata: new Dictionary<string, string>()),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render",
                    Kind: "browser_screenshot",
                    UriRef:
                        $"artifact://design-conversion/{recordId}/render.png",
                    Sha256: $"sha256:{recordId}:render",
                    ByteLength: 1024,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>())
            ],
            VisualJudgeStatus:
                new DesignConversionVisualJudgeArtifactDbStatus(
                    Status: "passed",
                    ProviderBoundary: "local-test-provider",
                    ExternalCallExecuted: false,
                    EstimatedCostUsd: 0m,
                    TierDecision: "local",
                    EvidenceRefs: [],
                    Metadata: new Dictionary<string, string>()),
            DpoCandidates: trainingRecords
                .Select(
                    trainingRecord =>
                        DesignConversionLocalArtifactDbDpoCandidate
                            .FromTrainingRecord(trainingRecord))
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "local_artifact_db",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>());

    private static DesignConversionLocalArtifactDbRecord
        WithPostEvaluationMarkers(
            DesignConversionLocalArtifactDbRecord record,
            string stateRef)
        => record with
        {
            Metadata = new Dictionary<string, string>(
                record.Metadata,
                StringComparer.Ordinal)
            {
                [JsonDesignConversionPostEvaluationStateStore
                    .StateRefMetadataKey] = stateRef,
                [JsonDesignConversionPostEvaluationStateStore
                    .PersistenceStatusMetadataKey] =
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceProvisional,
                [JsonDesignConversionPostEvaluationStateStore
                    .TrainingEligibleMetadataKey] = bool.FalseString
            }
        };

    private static async Task<DesignConversionPostEvaluationState>
        WriteCommittedState(
            string root,
            string artifactRecordId,
            string scope)
    {
        var store = StateStore(root);
        var initial = await AssertWrite(
            store,
            InitialState(
                JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                    scope)));
        return await AssertWrite(
            store,
            initial with
            {
                PersistenceStatus =
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceCommitted,
                LedgerRecordId = string.Empty,
                ArtifactRecordId = artifactRecordId,
                CompletionBindingKind =
                    JsonDesignConversionPostEvaluationStateStore
                        .CompletionBindingLocalArtifactDbRecord,
                CompletionBindingRef = artifactRecordId,
                UpdatedAtUtc = initial.UpdatedAtUtc.AddSeconds(1),
                Version = initial.Version
            });
    }

    private static DesignConversionPostEvaluationState InitialState(
        string stateRef)
        => new(
            StateRef: stateRef,
            ReservationRef: stateRef,
            UserRef:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationUserRef,
            PeriodStartUtc:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationPeriodStartUtc,
            PeriodEndExclusiveUtc:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationPeriodEndExclusiveUtc,
            ChargedCostUsd: 0m,
            AccountingStatus:
                JsonDesignConversionPostEvaluationStateStore
                    .AccountingSettled,
            AccountingDesiredAction:
                JsonDesignConversionPostEvaluationStateStore.DesiredNone,
            SettlementAttempted: false,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            PersistenceStatus:
                JsonDesignConversionPostEvaluationStateStore
                    .PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            LastErrorCode: string.Empty,
            UpdatedAtUtc:
                new DateTimeOffset(
                    2026,
                    7,
                    30,
                    0,
                    0,
                    0,
                    TimeSpan.Zero),
            Version: 1);

    private static DesignConversionPostEvaluationState CommittedStateFor(
        DesignConversionLocalArtifactDbRecord record,
        string scope)
    {
        var initial = InitialState(
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                scope));
        return initial with
        {
            PersistenceStatus =
                JsonDesignConversionPostEvaluationStateStore
                    .PersistenceCommitted,
            ArtifactRecordId = record.RecordId,
            CompletionBindingKind =
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
            CompletionBindingRef = record.RecordId,
            UpdatedAtUtc = initial.UpdatedAtUtc.AddSeconds(1),
            Version = 2
        };
    }

    private static async Task<int> ReadApprovedCandidateCountAsync(
        string root)
    {
        using var index = JsonDocument.Parse(
            await File.ReadAllTextAsync(
                Path.Combine(root, "conversion-results.index.json")));
        return index.RootElement
            .GetProperty("approvedTrainingCandidateCount")
            .GetInt32();
    }

    private static JsonDesignConversionPostEvaluationStateStore StateStore(
        string root)
        => new(
            Path.Combine(
                root,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName));

    private static JsonlDesignConversionLocalArtifactDb InstrumentedDb(
        string root,
        IDesignConversionPostEvaluationStateStore stateStore)
        => new(
            root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            postEvaluationStateStoreForTesting: stateStore);

    private static async Task<DesignConversionPostEvaluationState>
        AssertWrite(
            IDesignConversionPostEvaluationStateStore store,
            DesignConversionPostEvaluationState state)
    {
        var write = await store.WriteAsync(state);
        Assert.True(write.IsSuccess, write.ErrorMessage);
        return write.Value!;
    }

    private static string StatePath(string root, string stateRef)
        => Path.Combine(
            root,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName,
            $"{stateRef}.json");

    private static string NewRoot()
        => DesignConversionPostEvaluationEligibilityTestFixture
            .CreateHandleBoundTempRoot(
                "post-evaluation-training-eligibility");

    private sealed record QueuedPromotionFixture(
        string Root,
        string IntakeId,
        string StatePath);

    private sealed class CountingStateStore(
        IReadOnlyList<DesignConversionPostEvaluationState> states,
        bool completeInventoryAsynchronously = false,
        bool failInventory = false)
        : IDesignConversionPostEvaluationStateStore
    {
        public int ListCalls { get; private set; }

        public int ReadCalls { get; private set; }

        public void ResetCounts()
        {
            ListCalls = 0;
            ReadCalls = 0;
        }

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            WriteAsync(
                DesignConversionPostEvaluationState state,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState>.Fail(
                    "Counting state store is read-only.",
                    "COUNTING_STATE_STORE_READ_ONLY"));

        public Task<OperationResult<DesignConversionPostEvaluationState?>>
            ReadAsync(
                string stateRef,
                CancellationToken cancellationToken = default)
        {
            ReadCalls++;
            return Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState?>.Ok(
                    states.SingleOrDefault(state =>
                        string.Equals(
                            state.StateRef,
                            stateRef,
                            StringComparison.Ordinal))));
        }

        public async Task<OperationResult<
            IReadOnlyList<DesignConversionPostEvaluationState>>> ListAsync(
                CancellationToken cancellationToken = default)
        {
            ListCalls++;
            if (completeInventoryAsynchronously)
            {
                await Task.Delay(10, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (failInventory)
            {
                return OperationResult<
                    IReadOnlyList<DesignConversionPostEvaluationState>>.Fail(
                    "Counting state inventory failed.",
                    "COUNTING_STATE_INVENTORY_FAILED");
            }

            return OperationResult<
                IReadOnlyList<DesignConversionPostEvaluationState>>.Ok(
                states);
        }
    }

    private sealed class NonPumpingSynchronizationContext
        : SynchronizationContext
    {
        public override void Post(
            SendOrPostCallback callback,
            object? state)
        {
            // Deliberately do not pump. Production must not capture this
            // context and then block itself waiting for a continuation.
        }
    }

    private sealed class ConfiguredRemotePromotionUploader
        : IDesignConversionBoundedPromotionUploader,
          IDesignConversionPromotionUploaderPreflight
    {
        private int _count;

        public int Count => Interlocked.CompareExchange(
            ref _count,
            0,
            0);

        public int MaximumSerializedReceiptUtf8Bytes => 64 * 1024;
        public DesignConversionLocalReceiptOnlyPromotionUploadAuthority Authority
        {
            get
            {
                var issued =
                    DesignConversionLocalReceiptOnlyPromotionUploadAuthority
                        .Issue(
                            this,
                            "DESIGN_CONVERSION_TEST_REMOTE_RECEIPT_UPLOAD_AUTHORITY");
                if (!issued.IsSuccess)
                    throw new InvalidOperationException(issued.ErrorMessage);
                return issued.Value!;
            }
        }

        public OperationResult<string> PreflightConfigurationFingerprintSha256()
            => OperationResult<string>.Ok("sha256:" + new string('b', 64));

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>>
            UploadAsync(
                DesignConversionTrainingManifest manifest,
                DesignConversionPromotionGateReport gateReport,
                CancellationToken ct = default)
        {
            Interlocked.Increment(ref _count);
            return Task.FromResult(
                OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                    new DesignConversionPromotionUploadReceipt(
                        UploadId: $"{manifest.ManifestId}:remote",
                        RemoteRef:
                            "https://model-server.example/promotions/" +
                            Uri.EscapeDataString(manifest.ManifestId),
                        UploadedAtUtc: DateTimeOffset.UtcNow,
                        EvidenceRefs:
                        [
                            "configured-uploader-remote-receipt"
                        ])
                    {
                        PublicationKind =
                            DesignConversionPromotionPublicationKind
                                .VerifiedModelServerUpload,
                        UploadAttempted = true,
                        ExternalCloudUploadAttempted = true,
                        UploadSucceeded = true,
                        VerificationSucceeded = true,
                        PublicationStatus =
                            DesignConversionPromotionPublicationStatus
                                .UploadedVerified,
                        ProvenanceRef =
                            $"{manifest.ManifestId}:remote-provenance"
                    }));
        }
    }
}
