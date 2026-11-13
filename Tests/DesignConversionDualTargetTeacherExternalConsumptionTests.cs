// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace Tests;

using System.Reflection;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;
using Xunit;

public sealed class DesignConversionDualTargetTeacherExternalConsumptionTests
{
    [Fact]
    public async Task ExternalAggregate_CommitsExactParentAndOneAtomicMachineMutation()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(fixture.Root, "pairs-happy"));
        var transaction = StoreAndPrepare(store, fixture.Records.Take(2).ToArray());
        var reserved = AssertSuccess(transaction.Read());
        var runner = new DesignConversionTrainingRunner();
        var prepared = Prepared(fixture);
        var projectedParent = AssertSuccess(
            DesignConversionDpoTrainingContentAuthority
                .ProjectDualTargetTeacherAggregate(
                    fixture.Records,
                    prepared.Pipeline.Batches,
                    prepared.Pipeline.FinalOptimizer));
        var projectedGroup = AssertSuccess(
            projectedParent.ProjectExternalAtomicGroup());
        var projectedChildren = projectedGroup.Children
            .OrderBy(static child => child.Ordinal)
            .ToArray();
        Assert.Equal(projectedGroup.GroupCommitmentSha256,
            reserved.GroupCommitmentSha256);
        Assert.Equal(projectedGroup.RequestId, reserved.RequestId);
        Assert.Equal(projectedChildren.Select(static child => child.PairId),
            reserved.OrderedPairIds);
        Assert.Equal(
            projectedChildren.Select(static child => child.ContentSha256),
            reserved.OrderedContentSha256s);
        var sinks = new List<DesignConversionTrainingPipeline
            .DesignConversionOptimizerMutationSinkObservation>();
        var boundaries = new List<string>();
        var leaseCount = 0;
        var previousSink = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        var previousBoundary = DesignConversionTrainingPipeline
            .MutationBoundaryObserverForTesting;
        var previousLease = DesignConversionTrainingPipeline
            .TrainingLeaseObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            sinks.Add;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            boundaries.Add;
        DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
            lease =>
            {
                leaseCount++;
                Assert.True(lease.MatchesExactTrainingContent(
                    projectedParent.ParentContent));
            };
        try
        {
            var result = await runner
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    prepared,
                    transaction);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var value = result.Value!;
            var batch = Assert.Single(value.Run.PipelineResult.BatchResults);
            Assert.Equal(2, batch.RowsTrained);
            Assert.Equal(1, leaseCount);
            var sink = Assert.Single(sinks);
            Assert.Equal("primary", sink.Stage);
            Assert.Equal(
                new[]
                {
                    DesignConversionDpoUnits.CriticUnit,
                    DesignConversionDpoUnits.ImproverUnit
                },
                sink.TrainingItems.Select(item => fixture.Records
                    .Single(record => record.RecordId == item.ItemId).UnitId));
            Assert.DoesNotContain(
                sinks,
                static observation => observation.Stage == "continue_probe");
            Assert.Equal(1, boundaries.Count(boundary => boundary ==
                "pipeline_external_after_begin_before_machine"));
            Assert.Equal(1, boundaries.Count(boundary => boundary ==
                "pipeline_external_after_fresh_load_before_complete"));

            var replay = AssertSuccess(transaction.Read());
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.Committed,
                replay.State);
            Assert.Equal(value.ExternalConsumption.TransactionId, replay.TransactionId);
            Assert.Equal(value.ExternalConsumption.GroupCommitmentSha256,
                replay.GroupCommitmentSha256);
            Assert.Equal(value.ExternalConsumption.RequestId, replay.RequestId);
            Assert.Equal(value.ExternalConsumption.ParentBatchSha256,
                replay.ParentBatchSha256);
            Assert.Equal(value.ExternalConsumption.ParentContentSha256,
                replay.ParentContentSha256);
            Assert.Equal(value.ExternalConsumption.ParentProvenanceSha256,
                replay.ParentProvenanceSha256);
            Assert.Equal(value.ExternalConsumption.OrderedPairIds,
                replay.OrderedPairIds);
            Assert.Equal(value.ExternalConsumption.OrderedContentSha256s,
                replay.OrderedContentSha256s);
            Assert.Equal(value.ExternalConsumption.ResultSha256,
                replay.ResultSha256);
            Assert.Equal(value.ExternalConsumption.CheckpointSha256,
                replay.CheckpointSha256);
            Assert.Equal(value.ExternalConsumption.ManifestSha256,
                replay.ManifestSha256);
            Assert.Equal(projectedParent.ParentContent.Sha256,
                value.ExternalConsumption.ParentBatchSha256);
            Assert.Equal(projectedParent.ParentContent.Rows.Single().Sha256,
                value.ExternalConsumption.ParentContentSha256);
            Assert.Equal(projectedParent.ParentProvenance.RequestId,
                value.ExternalConsumption.RequestId);
            Assert.Equal(reserved.OrderedPairIds,
                value.ExternalConsumption.OrderedPairIds);
            Assert.Equal(reserved.OrderedContentSha256s,
                value.ExternalConsumption.OrderedContentSha256s);
            Assert.Empty(AssertSuccess(new FileDpoPairStore(
                Path.Combine(fixture.Root, "pairs-happy")).GetAll()));
        }
        finally
        {
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previousSink;
            DesignConversionTrainingPipeline
                .MutationBoundaryObserverForTesting = previousBoundary;
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
                previousLease;
        }
    }

    [Fact]
    public async Task CustodyRejection_PrecedesBeginAndTransactionRemainsReleasable()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(fixture.Root, "pairs-custody"));
        var transaction = StoreAndPrepare(store, fixture.Records.Take(2).ToArray());
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = AssertSuccess(pipeline.PrepareExecution(
            fixture.Records,
            fixture.Options));
        var sinkCount = 0;
        var previousSink = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => sinkCount++;
        try
        {
            var result = await pipeline
                .RunDualTargetTeacherExternalConsumptionValidatedAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    fixture.Records,
                    fixture.Options,
                    custodyCapability: null,
                    prepared,
                    transaction);

            Assert.False(result.IsSuccess);
            Assert.Equal(0, sinkCount);
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.Reserved,
                AssertSuccess(transaction.Read()).State);
            Assert.True(AssertSuccess(transaction.ReleaseIfUnmutated()));
            Assert.Equal(2, AssertSuccess(store.GetAll()).Count);
        }
        finally
        {
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previousSink;
        }
    }

    [Fact]
    public async Task LegalParentRejection_PrecedesBeginAndTransactionRemainsReleasable()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(fixture.Root, "pairs-legal"));
        var transaction = StoreAndPrepare(store, fixture.Records.Take(2).ToArray());
        var revoked = DesignConversionLegalTeacherCapabilityTestFactory
            .CreateRevocable();
        revoked.LoseAdmissionLive();
        var sinkCount = 0;
        var previousSink = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => sinkCount++;
        try
        {
            var result = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    revoked.Authority,
                    new DesignConversionMachine(),
                    Prepared(fixture),
                    transaction);

            Assert.False(result.IsSuccess);
            Assert.Equal(0, sinkCount);
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.Reserved,
                AssertSuccess(transaction.Read()).State);
            Assert.True(AssertSuccess(transaction.ReleaseIfUnmutated()));
            Assert.Equal(2, AssertSuccess(store.GetAll()).Count);
        }
        finally
        {
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previousSink;
        }
    }

    [Fact]
    public void ReorderedOrCopiedReceiptCannotMintTheAcceptedHandle()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(fixture.Root, "pairs-order"));
        var pairs = fixture.Records.Take(2).Select(ToBoundPair).ToArray();
        var group = AssertSuccess(DpoPairAtomicGroupEnvelope.Create(
            pairs[0],
            pairs[1]));
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var coordinator = Assert.IsAssignableFrom<
            IDpoPairExternalConsumptionTransactionCoordinator>(store);
        var reordered = receipt with
        {
            OrderedPairIds = receipt.OrderedPairIds.Reverse().ToArray()
        };
        var copied = receipt with
        {
            OrderedPairIds = [receipt.OrderedPairIds[0], receipt.OrderedPairIds[0]]
        };

        Assert.False(coordinator.PrepareExternalConsumptionTransaction(
            reordered,
            DesignConversionMachine.DomainTag).IsSuccess);
        Assert.False(coordinator.PrepareExternalConsumptionTransaction(
            copied,
            DesignConversionMachine.DomainTag).IsSuccess);
        Assert.Equal(2, AssertSuccess(store.GetAll()).Count);
    }

    [Fact]
    public async Task SwappedGroupHandle_FailsBeforeBeginOrSink()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var unrelatedRecords = fixture.Records.Take(2)
            .Select(record => record with
            {
                TeacherProvenance = record.TeacherProvenance! with
                {
                    RequestId = "teacher:unrelated-invocation"
                }
            }).ToArray();
        var store = new FileDpoPairStore(Path.Combine(fixture.Root, "pairs-swapped"));
        var unrelated = StoreAndPrepare(store, unrelatedRecords);
        var sinkCount = 0;
        var previousSink = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => sinkCount++;
        try
        {
            var result = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    Prepared(fixture),
                    unrelated);

            Assert.False(result.IsSuccess);
            Assert.Equal(0, sinkCount);
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.Reserved,
                AssertSuccess(unrelated.Read()).State);
            Assert.True(AssertSuccess(unrelated.ReleaseIfUnmutated()));
            Assert.Equal(2, AssertSuccess(store.GetAll()).Count);
        }
        finally
        {
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previousSink;
        }
    }

    [Theory]
    [InlineData("prompt")]
    [InlineData("chosen")]
    [InlineData("rejected")]
    [InlineData("governance")]
    [InlineData("custody_evidence")]
    [InlineData("teacher_attestation")]
    [InlineData("teacher_model")]
    public async Task SameRequestDifferentValidGroup_FailsBeforeLeaseBeginOrSink(
        string difference)
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(
            fixture.Root,
            "pairs-same-request-" + difference));
        var differentRecords = DifferentSameRequestRecords(
            fixture.Records.Take(2).ToArray(),
            difference);
        Assert.All(differentRecords, record => Assert.Equal(
            fixture.Records[0].TeacherProvenance!.RequestId,
            record.TeacherProvenance!.RequestId));
        var transaction = StoreAndPrepare(store, differentRecords);
        var sinkCount = 0;
        var leaseCount = 0;
        var previousSink = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        var previousLease = DesignConversionTrainingPipeline
            .TrainingLeaseObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => sinkCount++;
        DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
            _ => leaseCount++;
        try
        {
            var result = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    Prepared(fixture),
                    transaction);

            Assert.False(result.IsSuccess);
            Assert.Equal(0, leaseCount);
            Assert.Equal(0, sinkCount);
            var live = AssertSuccess(transaction.Read());
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.Reserved,
                live.State);
            Assert.True(AssertSuccess(transaction.ReleaseIfUnmutated()));
            Assert.Equal(2, AssertSuccess(store.GetAll()).Count);
        }
        finally
        {
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previousSink;
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
                previousLease;
        }
    }

    [Fact]
    public void ExpectedGroupIdentity_RejectsChangedCommitmentWithExactChildren()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = AssertSuccess(pipeline.PrepareExecution(
            fixture.Records,
            fixture.Options));
        var aggregate = AssertSuccess(
            DesignConversionDpoTrainingContentAuthority
                .ProjectDualTargetTeacherAggregate(
                    fixture.Records,
                    prepared.Batches,
                    prepared.FinalOptimizer));
        var expected = AssertSuccess(
            DesignConversionExpectedExternalGroupIdentity.Create(
                AssertSuccess(aggregate.ProjectExternalAtomicGroup())));
        var exact = new FileDpoPairStore.ExternalConsumptionSnapshot(
            "transaction",
            expected.GroupCommitmentSha256,
            expected.RequestId,
            DesignConversionMachine.DomainTag,
            FileDpoPairStore.DpoExternalConsumptionState.Reserved,
            "",
            "",
            "",
            expected.OrderedPairIds,
            expected.OrderedContentSha256s,
            DpoExternalConsumerKind
                .DesignConversionDualTargetTeacherAggregate,
            "",
            "",
            "");

        Assert.True(expected.Matches(
            exact,
            allowReserved: true,
            allowAggregateBound: false));
        Assert.False(expected.Matches(
            exact with { GroupCommitmentSha256 = new string('f', 64) },
            allowReserved: true,
            allowAggregateBound: false));
    }

    [Fact]
    public void ExpectedGroupIdentity_RejectsDifferentCanonicalChildren()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = AssertSuccess(pipeline.PrepareExecution(
            fixture.Records,
            fixture.Options));
        var aggregate = AssertSuccess(
            DesignConversionDpoTrainingContentAuthority
                .ProjectDualTargetTeacherAggregate(
                    fixture.Records,
                    prepared.Batches,
                    prepared.FinalOptimizer));
        var expected = AssertSuccess(
            DesignConversionExpectedExternalGroupIdentity.Create(
                AssertSuccess(aggregate.ProjectExternalAtomicGroup())));
        var exact = new FileDpoPairStore.ExternalConsumptionSnapshot(
            "transaction",
            expected.GroupCommitmentSha256,
            expected.RequestId,
            DesignConversionMachine.DomainTag,
            FileDpoPairStore.DpoExternalConsumptionState.Reserved,
            "",
            "",
            "",
            expected.OrderedPairIds,
            expected.OrderedContentSha256s,
            DpoExternalConsumerKind
                .DesignConversionDualTargetTeacherAggregate,
            "",
            "",
            "");
        var otherPairIds = expected.OrderedPairIds.ToArray();
        var otherContent = expected.OrderedContentSha256s.ToArray();
        otherPairIds[0] = new string('e', 64);
        otherContent[1] = new string('d', 64);

        Assert.False(expected.Matches(
            exact with
            {
                OrderedPairIds = otherPairIds,
                OrderedContentSha256s = otherContent
            },
            allowReserved: true,
            allowAggregateBound: false));
    }

    [Fact]
    public void LocalArtifactPairProjection_HasSharedOwnerIdentityParity()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var record = fixture.Records[0];
        var expected = AssertSuccess(
            DesignConversionTrainingIntakeDpoPairAuthority.Project(record));
        var actual = Assert.Single(AssertSuccess(
            JsonlDesignConversionLocalArtifactDb
                .PrepareFinalTrainingIntakeDpoPairsFromApprovedCustodyRecords(
                    [record])));

        Assert.Equal(expected.Prompt, actual.Prompt);
        Assert.Equal(expected.Chosen, actual.Chosen);
        Assert.Equal(expected.Rejected, actual.Rejected);
        Assert.Equal(expected.Domain, actual.Domain);
        Assert.Equal(expected.CapturedAt, actual.CapturedAt);
        Assert.Equal(expected.TeacherProvenance, actual.TeacherProvenance);
        Assert.Equal(
            expected.GovernanceDecision?.LicenseAccepted,
            actual.GovernanceDecision?.LicenseAccepted);
        Assert.Equal(
            expected.GovernanceDecision?.IsQuarantined,
            actual.GovernanceDecision?.IsQuarantined);
        Assert.Equal(
            expected.GovernanceDecision?.EvidenceRefs,
            actual.GovernanceDecision?.EvidenceRefs);
        Assert.Equal(expected.ChosenAuthorship, actual.ChosenAuthorship);
        Assert.Equal(expected.RejectedAuthorship, actual.RejectedAuthorship);
        Assert.Equal(
            FileDpoPairStore.ComputeIdentity(expected, schemaVersion: 5),
            FileDpoPairStore.ComputeIdentity(actual, schemaVersion: 5));
    }

    [Fact]
    public void LocalArtifactPairProjection_RejectsAuthorshipTamperBeforeFileAuthority()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var record = fixture.Records[0];
        var projected = DesignConversionTrainingIntakeDpoPairAuthority
            .Project(record);
        var tampered = record with
        {
            ChosenAuthorKind = DpoPairSideAuthorKind.LocalModel,
            RejectedAuthorKind = DpoPairSideAuthorKind.User
        };

        var rejected = DesignConversionTrainingIntakeDpoPairAuthority
            .Project(tampered);

        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        Assert.Equal(
            record.ChosenAuthorKind,
            projected.Value!.ChosenAuthorship!.AuthorKind);
        Assert.Equal(
            record.RejectedAuthorKind,
            projected.Value.RejectedAuthorship!.AuthorKind);
        var teacherSide = projected.Value.ChosenAuthorship.AuthorKind is
            DpoPairSideAuthorKind.OpenTeacher
            ? projected.Value.ChosenAuthorship
            : projected.Value.RejectedAuthorship;
        Assert.Equal(
            projected.Value.TeacherProvenance,
            teacherSide.OpenTeacherPassport);
        Assert.False(rejected.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_TRAINING_INTAKE_DPO_PAIR_NOT_TRAINABLE",
            rejected.ErrorCode);
    }

    [Fact]
    public void SharedPairProjection_PreservesHistoricalGovernanceAndSchemaV4Identity()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var record = fixture.Records[0];
        var pair = AssertSuccess(
            DesignConversionTrainingIntakeDpoPairAuthority.Project(
                record));
        var historicalPreimage = BuildHistoricalGovernancePreimage(record);
        var historicalDigest = "sha256:" + Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(historicalPreimage)))
            .ToLowerInvariant();
        var historicalEvidence = record.Quarantine.EvidenceRefs
            .Where(IsHistoricalSha256Ref)
            .Distinct(StringComparer.Ordinal)
            .Take(31)
            .Append(historicalDigest)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.DoesNotContain("captured_at_utc", historicalPreimage,
            StringComparison.Ordinal);
        using (var historicalDocument =
               System.Text.Json.JsonDocument.Parse(historicalPreimage))
        {
            Assert.Equal(
                new[]
                {
                    "record_id",
                    "license_id",
                    "rights_scope",
                    "allows_training",
                    "contains_user_private_data",
                    "quarantine",
                    "source_manifest_ref",
                    "reviewer_ref"
                },
                historicalDocument.RootElement.EnumerateObject()
                    .Select(static property => property.Name));
        }
        Assert.Equal(
            "sha256:1643a60d408025e1e973dd43cbd99ac621a30f987b11fd485e0685d13fb0fcff",
            historicalDigest);
        Assert.Equal(
            historicalEvidence,
            pair.GovernanceDecision!.EvidenceRefs);
        var independentPairId = ComputeHistoricalSchemaV4PairId(pair);
        Assert.Equal(
            "07e5551322539849210d6987293502a8b425d2ee5f8f8894278976ab76aba21f",
            independentPairId);
        Assert.Equal(
            independentPairId,
            FileDpoPairStore.ComputeIdentity(pair, schemaVersion: 4));
    }

    [Fact]
    public async Task ClosedExecutionMode_DroppedExternalScopeFailsBeforeMachine()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(
            fixture.Root,
            "pairs-mode-drop"));
        var transaction = StoreAndPrepare(
            store,
            fixture.Records.Take(2).ToArray());
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = AssertSuccess(pipeline.PrepareExecution(
            fixture.Records,
            fixture.Options));
        var scope = AssertSuccess(DesignConversionDpoTrainingContentAuthority
            .AcquireDualTargetTeacherExternalConsumptionScope(
                fixture.Authority,
                fixture.Records,
                prepared.Batches,
                prepared.FinalOptimizer,
                transaction));
        var sinkCount = 0;
        var previousSink = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => sinkCount++;
        try
        {
            using (scope)
            {
                var result = await pipeline.RunValidatedAsync(
                    new DesignConversionMachine(),
                    fixture.Records,
                    fixture.Options,
                    fixture.Capability,
                    scope.Binding,
                    prepared,
                    CancellationToken.None);
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    LegalTrainingTeacherAdmissionAuthority
                        .TrainingContentInvalidErrorCode,
                    result.ErrorCode);
                Assert.Equal(0, sinkCount);
                Assert.Equal(
                    FileDpoPairStore.DpoExternalConsumptionState.AggregateBound,
                    AssertSuccess(transaction.Read()).State);
            }

            Assert.True(AssertSuccess(transaction.ReleaseIfUnmutated()));
            Assert.Equal(2, AssertSuccess(store.GetAll()).Count);
        }
        finally
        {
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previousSink;
        }
    }

    [Fact]
    public async Task FailureAfterDurableBegin_StaysStartedAndCannotRemutateOrRelease()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(fixture.Root, "pairs-begin"));
        var transaction = StoreAndPrepare(store, fixture.Records.Take(2).ToArray());
        var prepared = Prepared(fixture);
        var machineEntries = 0;
        var previousBoundary = DesignConversionTrainingPipeline
            .MutationBoundaryObserverForTesting;
        var previousMachine = DesignConversionMachine
            .TrainingEntryObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            boundary =>
            {
                if (boundary == "pipeline_external_after_begin_before_machine")
                    throw new IOException("injected-after-begin");
            };
        DesignConversionMachine.TrainingEntryObserverForTesting = _ => machineEntries++;
        try
        {
            var failed = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    prepared,
                    transaction);
            Assert.False(failed.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_EXTERNAL_BOUNDARY_FAILED",
                failed.ErrorCode);
            Assert.Equal(0, machineEntries);
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.MutationStarted,
                AssertSuccess(transaction.Read()).State);
            Assert.False(AssertSuccess(transaction.ReleaseIfUnmutated()));

            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            var rerun = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    prepared,
                    transaction);
            Assert.False(rerun.IsSuccess);
            Assert.Equal(0, machineEntries);
            Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", rerun.ErrorCode);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DesignConversionMachine.TrainingEntryObserverForTesting = previousMachine;
        }
    }

    [Fact]
    public async Task FailureAfterFreshLoad_PreventsCompleteAndRerunCannotRemutate()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(fixture.Root, "pairs-fresh"));
        var transaction = StoreAndPrepare(store, fixture.Records.Take(2).ToArray());
        var prepared = Prepared(fixture);
        var machineEntries = 0;
        var previousBoundary = DesignConversionTrainingPipeline
            .MutationBoundaryObserverForTesting;
        var previousMachine = DesignConversionMachine
            .TrainingEntryObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            boundary =>
            {
                if (boundary == "pipeline_external_after_fresh_load_before_complete")
                    throw new IOException("injected-before-complete");
            };
        DesignConversionMachine.TrainingEntryObserverForTesting = _ => machineEntries++;
        try
        {
            var failed = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    prepared,
                    transaction);
            Assert.False(failed.IsSuccess);
            Assert.Equal(1, machineEntries);
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.MutationStarted,
                AssertSuccess(transaction.Read()).State);

            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            var rerun = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    prepared,
                    transaction);
            Assert.False(rerun.IsSuccess);
            Assert.Equal(1, machineEntries);
            Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", rerun.ErrorCode);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DesignConversionMachine.TrainingEntryObserverForTesting = previousMachine;
        }
    }

    [Fact]
    public async Task CancellationBeforeExternalComplete_LeavesStartedAndCannotRemutate()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(
            fixture.Root,
            "pairs-cancel-before-complete"));
        var transaction = StoreAndPrepare(
            store,
            fixture.Records.Take(2).ToArray());
        var prepared = Prepared(fixture);
        var machineEntries = 0;
        using var cancellation = new CancellationTokenSource();
        var previousBoundary = DesignConversionTrainingPipeline
            .MutationBoundaryObserverForTesting;
        var previousMachine = DesignConversionMachine
            .TrainingEntryObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            boundary =>
            {
                if (boundary ==
                    "pipeline_external_after_fresh_load_before_complete")
                {
                    cancellation.Cancel();
                }
            };
        DesignConversionMachine.TrainingEntryObserverForTesting =
            _ => machineEntries++;
        try
        {
            var cancelled = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    prepared,
                    transaction,
                    cancellation.Token);
            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.Equal(1, machineEntries);
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.MutationStarted,
                AssertSuccess(transaction.Read()).State);

            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            var rerun = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    prepared,
                    transaction);
            Assert.False(rerun.IsSuccess);
            Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", rerun.ErrorCode);
            Assert.Equal(1, machineEntries);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DesignConversionMachine.TrainingEntryObserverForTesting =
                previousMachine;
        }
    }

    [Fact]
    public void CancellationDuringExternalComplete_CannotOverwriteCommittedEvidence()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(
            fixture.Root,
            "pairs-cancel-during-complete"));
        var transaction = StoreAndPrepare(
            store,
            fixture.Records.Take(2).ToArray());
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = AssertSuccess(pipeline.PrepareExecution(
            fixture.Records,
            fixture.Options));
        using var scope = AssertSuccess(
            DesignConversionDpoTrainingContentAuthority
                .AcquireDualTargetTeacherExternalConsumptionScope(
                    fixture.Authority,
                    fixture.Records,
                    prepared.Batches,
                    prepared.FinalOptimizer,
                    transaction));
        Assert.True(AssertSuccess(scope.BeginMutation()));
        using var cancellation = new CancellationTokenSource();
        SetPrivateField(
            store,
            "_afterExternalReadyWrittenBeforeCommitForTesting",
            (Action)cancellation.Cancel);
        try
        {
            var completed = DesignConversionTrainingPipeline
                .CompleteExternalConsumptionAfterVerifiedArtifact(
                    scope,
                    new string('a', 64),
                    new string('b', 64),
                    cancellation.Token);

            Assert.True(cancellation.IsCancellationRequested);
            var evidence = AssertSuccess(completed);
            Assert.Equal(new string('a', 64), evidence.ResultSha256);
            Assert.Equal(new string('b', 64), evidence.CheckpointSha256);
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.Committed,
                AssertSuccess(transaction.Read()).State);
        }
        finally
        {
            SetPrivateField(
                store,
                "_afterExternalReadyWrittenBeforeCommitForTesting",
                null);
        }
    }

    [Fact]
    public async Task PreCancelledExternalComplete_LeavesStartedWithoutCommitOrRemutation()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(
            fixture.Root,
            "pairs-pre-cancelled-complete"));
        var transaction = StoreAndPrepare(
            store,
            fixture.Records.Take(2).ToArray());
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = AssertSuccess(pipeline.PrepareExecution(
            fixture.Records,
            fixture.Options));
        using var scope = AssertSuccess(
            DesignConversionDpoTrainingContentAuthority
                .AcquireDualTargetTeacherExternalConsumptionScope(
                    fixture.Authority,
                    fixture.Records,
                    prepared.Batches,
                    prepared.FinalOptimizer,
                    transaction));
        Assert.True(AssertSuccess(scope.BeginMutation()));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var cancelled = DesignConversionTrainingPipeline
            .CompleteExternalConsumptionAfterVerifiedArtifact(
                scope,
                new string('a', 64),
                new string('b', 64),
                cancellation.Token);
        Assert.False(cancelled.IsSuccess);
        Assert.Equal("CANCELLED", cancelled.ErrorCode);
        Assert.Null(scope.CommitEvidence);
        Assert.Equal(
            FileDpoPairStore.DpoExternalConsumptionState.MutationStarted,
            AssertSuccess(transaction.Read()).State);
        Assert.False(AssertSuccess(transaction.ReleaseIfUnmutated()));
        Assert.False(scope.BeginMutation().IsSuccess);

        var machineEntries = 0;
        var previousMachine = DesignConversionMachine
            .TrainingEntryObserverForTesting;
        DesignConversionMachine.TrainingEntryObserverForTesting =
            _ => machineEntries++;
        try
        {
            var rerun = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    Prepared(fixture),
                    transaction);
            Assert.False(rerun.IsSuccess);
            Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", rerun.ErrorCode);
            Assert.Equal(0, machineEntries);
        }
        finally
        {
            DesignConversionMachine.TrainingEntryObserverForTesting =
                previousMachine;
        }
    }

    [Fact]
    public async Task ExternalCompleteFailure_PropagatesWithoutFalseSuccess()
    {
        using var fixture = DesignConversionDualTargetTeacherAdmissionTests
            .CreateFixture();
        var store = new FileDpoPairStore(Path.Combine(fixture.Root, "pairs-complete"));
        var transaction = StoreAndPrepare(store, fixture.Records.Take(2).ToArray());
        SetPrivateField(
            store,
            "_afterExternalManifestWrittenBeforeReadyForTesting",
            (Action)(() => throw new IOException("injected-complete-failure")));
        try
        {
            var result = await new DesignConversionTrainingRunner()
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    Prepared(fixture),
                    transaction);

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PAIR_STORE_UNAVAILABLE", result.ErrorCode);
            Assert.Equal(
                FileDpoPairStore.DpoExternalConsumptionState.MutationStarted,
                AssertSuccess(transaction.Read()).State);
            Assert.False(AssertSuccess(transaction.ReleaseIfUnmutated()));
        }
        finally
        {
            SetPrivateField(
                store,
                "_afterExternalManifestWrittenBeforeReadyForTesting",
                null);
        }
    }

    private static DesignConversionPreparedTrainingRecordExecution Prepared(
        DesignConversionDualTargetTeacherAdmissionTests.Fixture fixture)
    {
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = AssertSuccess(pipeline.PrepareExecution(
            fixture.Records,
            fixture.Options));
        return new DesignConversionPreparedTrainingRecordExecution(
            new DesignConversionDpoBuildResult(
                fixture.Records,
                [],
                []),
            fixture.Options,
            fixture.Capability,
            prepared);
    }

    private static FileDpoPairStore.ExternalConsumptionTransactionHandle
        StoreAndPrepare(
            FileDpoPairStore store,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records)
    {
        Assert.Equal(2, records.Count);
        var pairs = records.Select(ToBoundPair).ToArray();
        var group = AssertSuccess(DpoPairAtomicGroupEnvelope.Create(
            pairs[0],
            pairs[1]));
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var coordinator = Assert.IsAssignableFrom<
            IDpoPairExternalConsumptionTransactionCoordinator>(store);
        return Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
                AssertSuccess(coordinator
                    .PrepareExternalConsumptionTransaction(
                        receipt,
                        DesignConversionMachine.DomainTag)));
    }

    private static DpoPair ToBoundPair(
        DesignConversionDpoTrainingRecord record)
        => AssertSuccess(
            DesignConversionTrainingIntakeDpoPairAuthority.Project(record));

    private static DesignConversionDpoTrainingRecord[]
        DifferentSameRequestRecords(
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            string difference)
    {
        var changed = records.Select(static record => record with { }).ToArray();
        changed[0] = difference switch
        {
            "prompt" => changed[0] with
            {
                InternalVectorPayload = changed[0].InternalVectorPayload
                    .Select((value, index) => index == 0
                        ? value + 0.125
                        : value)
                    .ToArray()
            },
            "chosen" => changed[0] with
            {
                Chosen = changed[0].Chosen with
                {
                    Html = changed[0].Chosen.Html + "<!-- different chosen -->"
                }
            },
            "rejected" => changed[0] with
            {
                Rejected = changed[0].Rejected with
                {
                    Html = changed[0].Rejected.Html +
                        "<!-- different rejected -->"
                }
            },
            "governance" => changed[0] with
            {
                License = changed[0].License with
                {
                    LicenseId = changed[0].License.LicenseId + ":different"
                }
            },
            "custody_evidence" => changed[0] with
            {
                Quarantine = changed[0].Quarantine with
                {
                    EvidenceRefs = changed[0].Quarantine.EvidenceRefs
                        .Append("sha256:" + new string('e', 64))
                        .ToArray()
                }
            },
            _ => changed[0]
        };

        if (difference is "teacher_attestation" or "teacher_model")
        {
            changed = changed.Select(record => record with
            {
                TeacherProvenance = difference == "teacher_attestation"
                    ? record.TeacherProvenance! with
                    {
                        ModelArtifactDigest = new string('d', 64)
                    }
                    : record.TeacherProvenance! with
                    {
                        ModelId = "qwen3.5:27b"
                    }
            }).ToArray();
        }

        return changed;
    }

    private static void SetPrivateField(
        object target,
        string name,
        object? value)
    {
        var field = target.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static string BuildHistoricalGovernancePreimage(
        DesignConversionDpoTrainingRecord record)
        => System.Text.Json.JsonSerializer.Serialize(new
        {
            record_id = record.RecordId,
            license_id = record.License.LicenseId,
            rights_scope = record.License.RightsScope,
            allows_training = record.License.AllowsTraining,
            contains_user_private_data = record.License.ContainsUserPrivateData,
            quarantine = new
            {
                is_quarantined = record.Quarantine.IsQuarantined,
                reason = record.Quarantine.Reason,
                evidence_refs = record.Quarantine.EvidenceRefs
            },
            source_manifest_ref = record.Provenance.SourceManifestRef,
            reviewer_ref = record.Provenance.ReviewerRef
        }, new System.Text.Json.JsonSerializerOptions(
            DesignConversionCheckpointJsonOptions.Standard)
        {
            WriteIndented = false
        });

    private static bool IsHistoricalSha256Ref(string? value)
        => value is { Length: 71 }
           && value.StartsWith("sha256:", StringComparison.Ordinal)
           && value.AsSpan(7).ToArray().All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static string ComputeHistoricalSchemaV4PairId(DpoPair pair)
    {
        using var hash = System.Security.Cryptography.IncrementalHash
            .CreateHash(System.Security.Cryptography.HashAlgorithmName.SHA256);
        AppendHistoricalIdentityField(hash, "xiigen.dpo-pair.v4");
        AppendHistoricalIdentityField(hash, pair.Domain);
        AppendHistoricalIdentityField(hash, pair.Prompt);
        AppendHistoricalIdentityField(hash, pair.Chosen);
        AppendHistoricalIdentityField(hash, pair.Rejected);
        AppendHistoricalIdentityField(
            hash,
            pair.TeacherProvenance is null ? "0" : "1");
        if (pair.TeacherProvenance is { } provenance)
        {
            AppendHistoricalIdentityField(hash, ((int)provenance.SourceKind)
                .ToString(System.Globalization.CultureInfo.InvariantCulture));
            AppendHistoricalIdentityField(hash, HistoricalNullableEnum(provenance.Family));
            AppendHistoricalIdentityField(hash, HistoricalNullableEnum(provenance.AccessMode));
            AppendHistoricalIdentityField(hash, HistoricalNullableEnum(provenance.HostingProvider));
            AppendHistoricalIdentityField(hash, provenance.ModelId);
            AppendHistoricalIdentityField(hash, provenance.ModelVersion);
            AppendHistoricalIdentityField(hash, HistoricalNullableEnum(provenance.ModelLicense));
            AppendHistoricalIdentityField(hash, provenance.ProviderTermsChecked ? "1" : "0");
            AppendHistoricalIdentityField(hash, provenance.ProviderTermsAllowTraining ? "1" : "0");
            AppendHistoricalIdentityField(hash, provenance.ClosedProprietaryModel ? "1" : "0");
            AppendHistoricalIdentityField(hash, provenance.TeacherOutputUsedForTraining ? "1" : "0");
            AppendHistoricalIdentityField(hash, provenance.RequestId);
            AppendHistoricalIdentityField(hash, provenance.InvocationCount
                .ToString(System.Globalization.CultureInfo.InvariantCulture));
            AppendHistoricalIdentityField(hash, provenance.ModelArtifactDigestKind);
            AppendHistoricalIdentityField(hash, provenance.ModelArtifactDigest);
            AppendHistoricalIdentityField(hash, provenance.TrainingContentSha256);
        }

        AppendHistoricalIdentityField(
            hash,
            pair.GovernanceDecision is null ? "0" : "1");
        if (pair.GovernanceDecision is { } governance)
        {
            AppendHistoricalIdentityField(hash, governance.LicenseAccepted ? "1" : "0");
            AppendHistoricalIdentityField(hash, governance.IsQuarantined ? "1" : "0");
            AppendHistoricalIdentityField(hash, governance.EvidenceRefs.Count
                .ToString(System.Globalization.CultureInfo.InvariantCulture));
            foreach (var evidenceRef in governance.EvidenceRefs)
                AppendHistoricalIdentityField(hash, evidenceRef);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static string HistoricalNullableEnum<TEnum>(TEnum? value)
        where TEnum : struct, Enum
        => value is null
            ? "null"
            : Convert.ToInt32(
                    value.Value,
                    System.Globalization.CultureInfo.InvariantCulture)
                .ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static void AppendHistoricalIdentityField(
        System.Security.Cryptography.IncrementalHash hash,
        string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        Span<byte> length = stackalloc byte[sizeof(int)];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(
            length,
            bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }
}
