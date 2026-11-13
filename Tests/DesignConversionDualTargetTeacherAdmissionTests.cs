// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace Tests;

using System.Security.Cryptography;
using System.Text;
using ContinuousLearning;
using DesignConversionDomain;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;
using Xunit;

public sealed class DesignConversionDualTargetTeacherAdmissionTests
{
    [Fact]
    public void ManualAggregateCustodyAllowsNonSelfLabelButRejectsSelfLabel()
    {
        using var fixture = CreateFixture();
        var manual = fixture.Records[0];
        Assert.Equal(
            bool.FalseString,
            manual.Provenance.Metadata["runtime_teacher_selected_preference"]);
        Assert.Equal(
            bool.TrueString,
            manual.Provenance.Metadata[
                "test_only_manual_independent_teacher_aggregate"]);
        Assert.DoesNotContain(
            "forbidden_self_label",
            DesignConversionTrainingMeasurementGatePolicy
                .GetRecordBlockReasons(manual));
        Assert.True(
            JsonlDesignConversionLocalArtifactDb
                .ValidateAndMaterializePayloadCustody(manual).IsSuccess,
            "manual non-self-labelled fixture custody must validate");

        var selfLabelMetadata = new Dictionary<string, string>(
            manual.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            ["runtime_teacher_selected_preference"] = bool.TrueString
        };
        var selfLabelled = AttachCustody(manual with
        {
            Provenance = manual.Provenance with { Metadata = selfLabelMetadata }
        });

        Assert.Contains(
            "forbidden_self_label",
            DesignConversionTrainingMeasurementGatePolicy
                .GetRecordBlockReasons(selfLabelled));
        Assert.False(
            JsonlDesignConversionLocalArtifactDb
                .ValidateAndMaterializePayloadCustody(selfLabelled).IsSuccess);
    }

    [Fact]
    public async Task DualTargetTeacherAggregate_AdmitsOneParentAndMutatesBothUnitsAtomically()
    {
        using var fixture = CreateFixture();
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = pipeline.PrepareExecution(fixture.Records, fixture.Options);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        Assert.Equal(2, prepared.Value!.FinalOptimizer!.Inputs.Count);
        Assert.All(fixture.Records.Take(2), record => Assert.True(
            JsonlDesignConversionLocalArtifactDb
                .ValidateAndMaterializePayloadCustody(record).IsSuccess,
            "fixture custody must validate"));
        Assert.True(
            DesignConversionDpoTrainingContentAuthority
                .Project(fixture.Records.Take(2).ToArray()).IsSuccess,
            "fixture record content must project");
        Assert.True(
            DesignConversionDpoTrainingContentAuthority
                .MatchesExactPipelineFinalOptimizer(
                    prepared.Value.FinalOptimizer),
            "prepared optimizer must be exact");
        var projectedAggregate = DesignConversionDpoTrainingContentAuthority
            .ProjectDualTargetTeacherAggregate(
                fixture.Records,
                prepared.Value.Batches,
                prepared.Value.FinalOptimizer);
        Assert.True(projectedAggregate.IsSuccess,
            projectedAggregate.ErrorMessage);
        Assert.All(fixture.Records.Take(2), record => Assert.True(
            DesignConversionCandidatePayloadCustodyStore.Authorizes(
                fixture.Capability,
                record),
            "fixture capability must authorize the exact pair record"));

        ServerOwnedLegalTrainingTeacherAdmissionLease? observedLease = null;
        var leaseObservationCount = 0;
        var observedParentContent = false;
        var observedChildContent = false;
        var mutationObservations = new List<
            DesignConversionTrainingPipeline
                .DesignConversionOptimizerMutationSinkObservation>();
        var mutationBoundaries = new List<string>();
        var machineTrainingEntries = new List<string>();
        var previousLeaseObserver = DesignConversionTrainingPipeline
            .TrainingLeaseObserverForTesting;
        var previousSinkObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        var previousBoundaryObserver = DesignConversionTrainingPipeline
            .MutationBoundaryObserverForTesting;
        var previousMachineEntryObserver = DesignConversionMachine
            .TrainingEntryObserverForTesting;
        DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
            lease =>
            {
                leaseObservationCount++;
                observedLease = lease;
                observedParentContent = lease.MatchesExactTrainingContent(
                    projectedAggregate.Value!.ParentContent);
                observedChildContent = lease.MatchesExactTrainingContent(
                    prepared.Value.FinalOptimizer.Content);
            };
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            mutationObservations.Add;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            mutationBoundaries.Add;
        DesignConversionMachine.TrainingEntryObserverForTesting =
            machineTrainingEntries.Add;
        try
        {
            var result = await pipeline
                .RunDualTargetTeacherAggregateValidatedAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    fixture.Records,
                    fixture.Options,
                    fixture.Capability,
                    prepared.Value);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(3, result.Value!.AcceptedRecordCount);
            var batchResult = Assert.Single(result.Value.BatchResults);
            Assert.Equal(2, batchResult.RowsTrained);
            var observation = Assert.Single(mutationObservations);
            Assert.Equal("primary", observation.Stage);
            Assert.Equal(
                fixture.Records.Take(2)
                    .Select(static record => record.RecordId),
                observation.TrainingItems.Select(static item => item.ItemId));
            Assert.Equal(["admitted"], machineTrainingEntries);
            Assert.DoesNotContain("pipeline_continue_probe", mutationBoundaries);
            Assert.DoesNotContain(
                "pipeline_continue_probe_result",
                mutationBoundaries);
            Assert.Equal(
                DesignConversionTrainingPipeline
                    .DualTargetAggregateContinuationSkippedStatus,
                result.Value.LifecycleEvidence.ContinueTrainingStatus);
            Assert.Null(result.Value.LifecycleEvidence.LocalContinueTrainingProbe);
            Assert.Contains(
                DesignConversionTrainingPipeline
                    .DualTargetAggregateContinuationSkippedEvidence,
                result.Value.LifecycleEvidence.EvidenceRefs);
            Assert.Contains(
                "continue_training",
                result.Value.LifecycleEvidence.NotProvenFields);
            Assert.DoesNotContain(
                result.Value.LifecycleEvidence.EvidenceRefs,
                static evidence => evidence.StartsWith(
                    "DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_",
                    StringComparison.Ordinal));
            Assert.NotNull(observedLease);
            Assert.Equal(1, leaseObservationCount);
            Assert.True(observedParentContent);
            Assert.False(observedChildContent);
            var receipt = Assert.Single(observedLease!.Admission.Receipts);
            Assert.Equal(1, observedLease.Admission.ExternalTeacherInvocationCount);
            Assert.Equal(1, receipt.InvocationCount);
            Assert.NotEmpty(receipt.TrainingContentRef);
            Assert.NotEmpty(observedLease.Admission.TrainingContentBatchRef);
            Assert.NotEqual(
                prepared.Value.FinalOptimizer.Content.Sha256,
                observedLease.Admission.TrainingContentBatchRef);
            Assert.NotEqual(
                prepared.Value.FinalOptimizer.Content.Rows[0].Sha256,
                receipt.TrainingContentRef);
            var disposedPermit = observedLease.AcquireMutationPermit();
            Assert.False(disposedPermit.IsSuccess);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority
                    .AdmissionRevokedErrorCode,
                disposedPermit.ErrorCode);
        }
        finally
        {
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
                previousLeaseObserver;
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previousSinkObserver;
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundaryObserver;
            DesignConversionMachine.TrainingEntryObserverForTesting =
                previousMachineEntryObserver;
        }

        var rawDuplicate = DesignConversionLegalTeacherCapabilityTestFactory
            .CreateAdmitted()
            .AcquireTrainingLease(
                prepared.Value.FinalOptimizer.Content,
                prepared.Value.FinalOptimizer.Provenance);
        Assert.False(rawDuplicate.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
            rawDuplicate.ErrorCode);
    }

    [Fact]
    public async Task DualTargetTeacherAggregate_InvalidCustodyCapabilityFailsBeforeLegalLease()
    {
        using var fixture = CreateFixture();
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = pipeline.PrepareExecution(fixture.Records, fixture.Options);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);

        var leaseObservationCount = 0;
        var sinkCount = 0;
        var previousLeaseObserver = DesignConversionTrainingPipeline
            .TrainingLeaseObserverForTesting;
        var previousSinkObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
            _ => leaseObservationCount++;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => sinkCount++;
        try
        {
            var result = await pipeline
                .RunDualTargetTeacherAggregateValidatedAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    fixture.Records,
                    fixture.Options,
                    custodyCapability: null,
                    prepared.Value!);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_CUSTODY_AUTHORITY_REQUIRED",
                result.ErrorCode);
            Assert.Equal(0, leaseObservationCount);
            Assert.Equal(0, sinkCount);
        }
        finally
        {
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
                previousLeaseObserver;
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previousSinkObserver;
        }
    }

    [Fact]
    public void DualTargetTeacherAggregate_ParentContextCommitsBothChildrenWithoutRawSides()
    {
        using var fixture = CreateFixture();
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = pipeline.PrepareExecution(fixture.Records, fixture.Options);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var aggregate = DesignConversionDpoTrainingContentAuthority
            .ProjectDualTargetTeacherAggregate(
                fixture.Records,
                prepared.Value!.Batches,
                prepared.Value.FinalOptimizer);
        Assert.True(aggregate.IsSuccess, aggregate.ErrorMessage);

        var parentRow = Assert.Single(
            aggregate.Value!.ParentContent.Rows);
        var context = parentRow.Context;
        Assert.Equal(
            fixture.Records[0].TeacherProvenance!.RequestId,
            aggregate.Value.ParentProvenance.RequestId);
        Assert.StartsWith("sha256:", parentRow.Prompt, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", parentRow.Chosen, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", parentRow.Rejected, StringComparison.Ordinal);
        Assert.Equal(
            2,
            context.Split(
                "custody_binding_sha256",
                StringSplitOptions.None).Length - 1);
        Assert.Equal(
            2,
            context.Split(
                "optimizer_row_sha256",
                StringSplitOptions.None).Length - 1);
        Assert.Equal(
            2,
            context.Split(
                "record_row_sha256",
                StringSplitOptions.None).Length - 1);
        for (var index = 0; index < 2; index++)
        {
            var secret = fixture.Records[index].PayloadCustodySecret!;
            Assert.Contains(
                secret.TrainingRecordBindingSha256,
                context,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                prepared.Value.FinalOptimizer.Inputs[index].Prompt,
                context,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                prepared.Value.FinalOptimizer.Inputs[index].ChosenResponse,
                context,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                prepared.Value.FinalOptimizer.Inputs[index].RejectedResponse,
                context,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                prepared.Value.FinalOptimizer.Inputs[index].ChosenResponse,
                parentRow.Chosen,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                prepared.Value.FinalOptimizer.Inputs[index].RejectedResponse,
                parentRow.Rejected,
                StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("same-target")]
    [InlineData("partial")]
    [InlineData("third-child")]
    [InlineData("reordered")]
    [InlineData("copied-record")]
    [InlineData("mixed-request")]
    [InlineData("mixed-model")]
    [InlineData("mixed-attestation")]
    [InlineData("replayed")]
    [InlineData("content")]
    [InlineData("custody")]
    [InlineData("custody-binding")]
    public void DualTargetTeacherAggregate_InvalidGroupFailsBeforeLegalAdmission(
        string mutation)
    {
        using var fixture = CreateFixture();
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = pipeline.PrepareExecution(fixture.Records, fixture.Options);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);

        IReadOnlyList<DesignConversionDpoTrainingRecord> records =
            fixture.Records;
        IReadOnlyList<DesignConversionTrainingBatch> batches =
            prepared.Value!.Batches;
        var finalOptimizer = prepared.Value.FinalOptimizer;
        switch (mutation)
        {
            case "same-target":
                var sameTarget = AttachCustody(
                    fixture.Records[1] with
                    {
                        UnitId = DesignConversionDpoUnits.CriticUnit,
                        InternalUnitStatePayload =
                            "{\"schema\":\"design-conversion-unit-state-v1\",\"unit_id\":\"CriticUnit\"}"
                    });
                records =
                [
                    fixture.Records[0],
                    sameTarget,
                    fixture.Records[2]
                ];
                var sameTargetPrepared = pipeline.PrepareExecution(
                    records,
                    fixture.Options);
                Assert.True(sameTargetPrepared.IsSuccess,
                    sameTargetPrepared.ErrorMessage);
                batches = sameTargetPrepared.Value!.Batches;
                finalOptimizer = sameTargetPrepared.Value.FinalOptimizer;
                break;
            case "partial":
                records = [fixture.Records[0], fixture.Records[2]];
                break;
            case "third-child":
                records =
                [
                    fixture.Records[0],
                    fixture.Records[1],
                    fixture.Records[0] with
                    {
                        RecordId = Sha("third-child"),
                        TeacherProvenance = fixture.Records[0]
                            .TeacherProvenance! with
                            {
                                RequestId = "teacher:third-child"
                            }
                    },
                    fixture.Records[2]
                ];
                break;
            case "reordered":
                batches = prepared.Value.Batches.Reverse().ToArray();
                break;
            case "copied-record":
                records =
                [
                    fixture.Records[0] with { },
                    fixture.Records[1],
                    fixture.Records[2]
                ];
                break;
            case "mixed-request":
                records =
                [
                    fixture.Records[0],
                    fixture.Records[1] with
                    {
                        TeacherProvenance = fixture.Records[1]
                            .TeacherProvenance! with
                            {
                                RequestId = "teacher:other-request"
                            }
                    },
                    fixture.Records[2]
                ];
                break;
            case "mixed-model":
                var mixedModelProvenance = prepared.Value.FinalOptimizer
                    .Provenance.ToArray();
                mixedModelProvenance[1] = mixedModelProvenance[1]! with
                {
                    ModelId = "qwen:substituted"
                };
                var mixedModel = DesignConversionDpoTrainingContentAuthority
                    .ProjectFinalOptimizerInputs(
                        prepared.Value.FinalOptimizer.Inputs,
                        mixedModelProvenance,
                        static item => item.Prompt,
                        static item => item.ChosenResponse,
                        static item => item.RejectedResponse,
                        static _ => null,
                        static _ => null,
                        static item => "dual-target-test|" + item.ItemId);
                Assert.True(mixedModel.IsSuccess, mixedModel.ErrorMessage);
                finalOptimizer = mixedModel.Value!;
                break;
            case "mixed-attestation":
                var mutableMetadata = Assert.IsType<Dictionary<string, string>>(
                    fixture.Records[1].Provenance.Metadata);
                Assert.NotSame(
                    fixture.Records[0].Provenance.Metadata,
                    mutableMetadata);
                mutableMetadata["teacher_authoring_generated_ref"] =
                    Sha("substituted-generated");
                break;
            case "replayed":
                records =
                [
                    fixture.Records[0],
                    fixture.Records[1] with
                    {
                        TeacherProvenance = fixture.Records[1]
                            .TeacherProvenance! with { InvocationCount = 2 }
                    },
                    fixture.Records[2]
                ];
                break;
            case "content":
                var itemCopy = prepared.Value.FinalOptimizer.Inputs.ToArray();
                itemCopy[1] = itemCopy[1] with { Prompt = "substituted" };
                var projected = DesignConversionDpoTrainingContentAuthority
                    .ProjectFinalOptimizerInputs(
                        itemCopy,
                        prepared.Value.FinalOptimizer.Provenance,
                        static item => item.Prompt,
                        static item => item.ChosenResponse,
                        static item => item.RejectedResponse,
                        static _ => null,
                        static _ => null,
                        static item => "dual-target-test|" + item.ItemId);
                Assert.True(projected.IsSuccess, projected.ErrorMessage);
                var contentResult = DesignConversionDpoTrainingContentAuthority
                    .ProjectDualTargetTeacherAggregate(
                        records,
                        batches,
                        projected.Value);
                Assert.False(contentResult.IsSuccess);
                Assert.Equal(
                    LegalTrainingTeacherAdmissionAuthority
                        .TrainingContentInvalidErrorCode,
                    contentResult.ErrorCode);
                return;
            case "custody":
                records =
                [
                    fixture.Records[0],
                    fixture.Records[1] with { PayloadCustodySecret = null },
                    fixture.Records[2]
                ];
                break;
            case "custody-binding":
                var custodyMetadata = Assert.IsType<Dictionary<string, string>>(
                    fixture.Records[1].Provenance.Metadata);
                custodyMetadata[
                    DesignConversionDpoGenerator
                        .PayloadCustodyBindingSha256MetadataKey] =
                    Sha("substituted-binding");
                break;
        }

        var result = DesignConversionDpoTrainingContentAuthority
            .ProjectDualTargetTeacherAggregate(
                records,
                batches,
                finalOptimizer);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void DualTargetTeacherAggregate_PostMintChildDriftAndRevocationFailClosed()
    {
        using var fixture = CreateFixture();
        var pipeline = new DesignConversionTrainingPipeline();
        var prepared = pipeline.PrepareExecution(fixture.Records, fixture.Options);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var admission = DesignConversionDpoTrainingContentAuthority
            .AcquireDualTargetTeacherAggregateAdmission(
                fixture.Authority,
                fixture.Records,
                prepared.Value!.Batches,
                prepared.Value.FinalOptimizer);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);

        var atomicBatch = admission.Value!.Binding
            .BuildAtomicDualTargetTeacherBatch("h2a-snapshot");
        Assert.True(atomicBatch.IsSuccess, atomicBatch.ErrorMessage);
        var machineAdmission = DesignConversionMachineTrainingAdmission.Mint(
            prepared.Value.FinalOptimizer,
            atomicBatch.Value,
            admission.Value.Binding);
        Assert.True(machineAdmission.IsSuccess, machineAdmission.ErrorMessage);
        Assert.All(
            machineAdmission.Value!.Batch.TrainingItems,
            captured => Assert.DoesNotContain(
                prepared.Value.FinalOptimizer.Inputs,
                original => ReferenceEquals(original, captured)));

        var mutableMetadata = Assert.IsType<Dictionary<string, string>>(
            fixture.Records[1].Provenance.Metadata);
        mutableMetadata["teacher_authoring_output_hash"] = Sha("drift");
        var driftPermit = admission.Value!.Binding.AcquireMutationPermit();
        Assert.False(driftPermit.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode,
            driftPermit.ErrorCode);

        mutableMetadata["teacher_authoring_output_hash"] =
            Sha("one teacher output");
        admission.Value.Dispose();
        var revokedPermit = admission.Value.Binding.AcquireMutationPermit();
        Assert.False(revokedPermit.IsSuccess);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            revokedPermit.ErrorCode);
    }

    internal static Fixture CreateFixture()
    {
        var teacher = new TrainingTeacherProvenance(
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
            RequestId: "teacher:one-real-invocation",
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = new string('c', 64)
        };
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["split"] = "train",
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["runtime_teacher_authored_candidate"] = bool.TrueString,
            ["runtime_teacher_selected_preference"] = bool.FalseString,
            ["forbidden_self_label"] = bool.FalseString,
            ["runtime_rejected_independently_evaluated"] = bool.TrueString,
            ["test_only_manual_independent_teacher_aggregate"] = bool.TrueString,
            ["test_only_manual_authority_kind"] =
                "explicit_fixture_construction_not_runtime_scorer_issuance",
            ["teacher_authoring_schema"] = "design-conversion-dpo-teacher-authoring-v1",
            ["teacher_authoring_output_hash"] = Sha("one teacher output"),
            ["teacher_authoring_incumbent_ref"] = Sha("incumbent"),
            ["teacher_authoring_generated_ref"] = Sha("generated"),
            ["teacher_inference_profile_id"] = "qwen-local-profile",
            ["teacher_inference_profile_configuration_sha256"] = Sha("profile"),
            ["approval_status"] = "approved",
            ["review_status"] = JsonlDesignConversionLocalArtifactDb
                .RuntimeCandidateReviewApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["approved_by_ref"] = Sha("reviewer"),
            ["approval_reason"] = "measured_teacher_pair_approved",
            ["approved_at_utc"] = "2026-08-14T00:00:00.0000000+00:00",
            ["training_candidate_only"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["production_dpo"] = bool.FalseString
        };
        var criticExample = DesignConversionDpoGeneratorTests.CreateExample(
            Sha("critic-record"),
            DesignConversionDpoUnits.CriticUnit) with
        {
            InternalUnitStatePayload =
                "{\"schema\":\"design-conversion-unit-state-v1\",\"unit_id\":\"CriticUnit\"}",
            Provenance = DesignConversionDpoGeneratorTests.CreateExample(
                    Sha("critic-record"),
                    DesignConversionDpoUnits.CriticUnit)
                .Provenance with { Metadata = metadata },
            TeacherProvenance = teacher
        } with
        {
            // Ручной fixture заявляет teacher-authored generated side, поэтому
            // typed author-kind обязан совпадать с source metadata.
            ChosenAuthorKind = DpoPairSideAuthorKind.OpenTeacher,
            RejectedAuthorKind = DpoPairSideAuthorKind.LocalModel
        };
        var improverExample = criticExample with
        {
            ExampleId = Sha("improver-record"),
            UnitId = DesignConversionDpoUnits.ImproverUnit,
            InternalUnitStatePayload =
                "{\"schema\":\"design-conversion-unit-state-v1\",\"unit_id\":\"ImproverUnit\"}",
            TeacherProvenance = teacher with { }
        };
        var built = new DesignConversionDpoGenerator()
            .BuildTrainReadyRecords([criticExample, improverExample]);
        Assert.True(built.IsSuccess, built.ErrorMessage);
        Assert.Empty(built.Value!.QuarantinedRecords);
        var pairRecords = built.Value.AcceptedRecords
            .Select(AttachCustody)
            .ToArray();
        Assert.Equal(
            new[]
            {
                DesignConversionDpoUnits.CriticUnit,
                DesignConversionDpoUnits.ImproverUnit
            },
            pairRecords.Select(static record => record.UnitId));

        var pairSourceHash = DesignConversionDpoDatasetBuilder
            .ComputeSourceIdentityHash(pairRecords[0]);
        DesignConversionDpoTrainingRecord? heldOut = null;
        for (var attempt = 0; attempt < 10_000 && heldOut is null; attempt++)
        {
            var example = DesignConversionDpoGeneratorTests.CreateExample(
                Sha("heldout-record"),
                DesignConversionDpoUnits.CriticUnit) with
            {
                Provenance = criticExample.Provenance with
                {
                    SourceManifestRef = $"manifest://h2a-heldout/{attempt}",
                    Metadata = new Dictionary<string, string>
                    {
                        ["split"] = "heldout"
                    }
                },
                TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                    "h2a:heldout")
            };
            var candidate = new DesignConversionDpoGenerator()
                .BuildTrainReadyRecords([example]);
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var record = Assert.Single(candidate.Value!.AcceptedRecords);
            if (string.CompareOrdinal(
                    DesignConversionDpoDatasetBuilder
                        .ComputeSourceIdentityHash(record),
                    pairSourceHash) < 0)
            {
                heldOut = record;
            }
        }
        Assert.NotNull(heldOut);
        var records = pairRecords.Append(heldOut!).ToArray();

        var root = Path.Combine(
            Path.GetTempPath(),
            "dcm-h2a-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var entries = pairRecords.Select(static record =>
        {
            var secret = record.PayloadCustodySecret!;
            return new DesignConversionCandidatePayloadCustodyEntry(
                record.RecordId,
                secret.TrainingRecordBindingSha256,
                secret.GovernanceSha256,
                secret.PromptSha256,
                secret.ChosenResponseSha256,
                secret.RejectedResponseSha256,
                secret);
        }).ToArray();
        var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
            "authority:h2a",
            Sha("authority identity"),
            entries);
        Assert.True(draft.IsSuccess, draft.ErrorMessage);
        var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
            draft.Value!,
            Sha("authority fingerprint"));
        Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
        var store = new DesignConversionCandidatePayloadCustodyStore(root);
        var prepared = store.Prepare(draft.Value!, commitment.Value!);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var evidence = store.Commit(prepared.Value!);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);
        var envelope = store.ResolveSecretEnvelope(evidence.Value!);
        Assert.True(envelope.IsSuccess, envelope.ErrorMessage);
        pairRecords = pairRecords.Select(record =>
        {
            var entry = DesignConversionCandidatePayloadCustodyStore
                .FindExactEntry(
                    envelope.Value!,
                    evidence.Value!,
                    record.PayloadCustodySecret!
                        .TrainingRecordBindingSha256);
            Assert.NotNull(entry);
            return record with { PayloadCustodySecret = entry!.Secret };
        }).ToArray();
        records = pairRecords.Append(heldOut!).ToArray();
        var capability = DesignConversionCandidatePayloadCustodyStore
            .MintTrainingCapability([evidence.Value!]);
        Assert.True(capability.IsSuccess, capability.ErrorMessage);

        return new Fixture(
            root,
            records,
            new DesignConversionTrainingPipelineOptions(
                "h2a-atomic",
                BatchSize: 1,
                CallMachineLifecycleHook: true,
                RequireDatasetCountGate: true,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    DatasetId: "h2a-atomic",
                    MinimumTrainRecords: 2,
                    MinimumHeldOutRecords: 0,
                    HeldOutRecordFraction: 0,
                    Preflight: DesignConversionDpoDatasetPreflight
                        .LocalFixtureTrainingCandidateOnly())),
            capability.Value!,
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
    }

    private static DesignConversionDpoTrainingRecord AttachCustody(
        DesignConversionDpoTrainingRecord record)
    {
        var item = DesignConversionTrainingPipeline.ToCanonicalTrainingItem(record);
        var prompt = DesignConversionDpoGenerator.Sha256Ref(item.Prompt);
        var chosen = DesignConversionDpoGenerator.Sha256Ref(item.ChosenResponse);
        var rejected = DesignConversionDpoGenerator.Sha256Ref(item.RejectedResponse);
        var governance = DesignConversionDpoGenerator
            .BuildPayloadCustodyGovernanceSha256(record);
        var binding = DesignConversionDpoGenerator.BuildPayloadCustodyBinding(
            record,
            prompt,
            chosen,
            rejected,
            governance);
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            [DesignConversionDpoGenerator.PayloadCustodyRequiredMetadataKey] =
                bool.TrueString,
            [DesignConversionDpoGenerator.PayloadCustodyVersionMetadataKey] =
                DesignConversionDpoGenerator.PayloadCustodyVersion,
            [DesignConversionDpoGenerator.PayloadCustodyPromptSha256MetadataKey] =
                prompt,
            [DesignConversionDpoGenerator.PayloadCustodyChosenSha256MetadataKey] =
                chosen,
            [DesignConversionDpoGenerator.PayloadCustodyRejectedSha256MetadataKey] =
                rejected,
            [DesignConversionDpoGenerator.PayloadCustodyGovernanceSha256MetadataKey] =
                governance,
            [DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey] =
                binding,
            ["payload_custody_raw_in_public_record"] = bool.FalseString
        };
        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata },
            PayloadCustodySecret = new DesignConversionDpoPayloadCustodySecret(
                DesignConversionDpoGenerator.PayloadCustodyVersion,
                item.Prompt,
                item.ChosenResponse,
                item.RejectedResponse,
                prompt,
                chosen,
                rejected,
                governance,
                binding)
        };
    }

    private static string Sha(string value)
        => "sha256:" + Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    internal sealed record Fixture(
        string Root,
        IReadOnlyList<DesignConversionDpoTrainingRecord> Records,
        DesignConversionTrainingPipelineOptions Options,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability Capability,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority Authority) : IDisposable
    {
        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
