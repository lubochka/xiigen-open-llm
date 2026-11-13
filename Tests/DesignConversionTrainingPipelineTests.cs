// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

[Collection(RepoArtifactSnapshotCollectionNames.CollectionName)]
public sealed class DesignConversionTrainingPipelineTests
{
    [Fact]
    public async Task AuthoritylessRunnerAndPipeline_FailClosedWithoutTouchingInputs()
    {
        var records = new ThrowingReadOnlyList<DesignConversionDpoTrainingRecord>();
        var examples = new ThrowingReadOnlyList<DesignConversionStoredDpoExample>();
        var options = new DesignConversionTrainingPipelineOptions("legacy-zero-touch", 1);

#pragma warning disable CS0618
        var pipeline = await new DesignConversionTrainingPipeline().RunAsync(
            null!,
            records,
            options);
        var runner = await new DesignConversionTrainingRunner().RunAsync(
            null!,
            examples,
            options);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var cancelledPipeline = await new DesignConversionTrainingPipeline().RunAsync(
            null!,
            records,
            options,
            cancelled.Token);
#pragma warning restore CS0618

        Assert.False(pipeline.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            pipeline.ErrorCode);
        Assert.False(runner.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            runner.ErrorCode);
        Assert.False(cancelledPipeline.IsSuccess);
        Assert.Equal("CANCELLED", cancelledPipeline.ErrorCode);
    }

    [Fact]
    public async Task StrictRunner_RevocationAtFinalBoundaryPreventsFalseSuccess()
    {
        var records = BuildSyntheticSourceDisjointRecords(2);
        var boundaries = new List<string>();
        var previousObserver =
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting = boundaries.Add;
        try
        {
            var admission = DesignConversionLegalTeacherCapabilityTestFactory.CreateRevocable();
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting = boundary =>
            {
                boundaries.Add(boundary);
                if (boundary == "pipeline_final")
                    admission.LoseAdmissionLive();
            };
            var result = await new DesignConversionTrainingPipeline().RunAsync(
                admission.Authority,
                new DesignConversionMachine(),
                records,
                new DesignConversionTrainingPipelineOptions(
                    "revoked-final",
                    BatchSize: 1,
                    DatasetOptions: LocalDatasetOptions(
                        "revoked-final",
                        MinimumTrainRecords: 1,
                        MinimumHeldOutRecords: 1)));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
                result.ErrorCode);
            Assert.Equal("pipeline_final", boundaries[^1]);
            Assert.Equal(1, boundaries.Count(boundary => boundary == "pipeline_final"));
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousObserver;
        }
    }

    [Fact]
    public async Task AuthorityFirstPipeline_ExactDeterministicProvenanceSucceedsWithoutCustody()
    {
        var records = BuildRecords(
        [
            DesignConversionDpoGeneratorTests.CreateExample(
                "authority-deterministic",
                DesignConversionDpoUnits.CriticUnit)
        ]);
        var record = Assert.Single(records);
        Assert.Equal(
            TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
            record.TeacherProvenance!.SourceKind);
        Assert.False(record.TeacherProvenance.TeacherOutputUsedForTraining);

        var result = await new DesignConversionTrainingPipeline().RunAsync(
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "authority-deterministic",
                BatchSize: 1,
                CallMachineLifecycleHook: false,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.AcceptedRecordCount);
        Assert.Equal(
            DesignConversionTrainingBoundaryDecisionKind.CountGateBypassedForScaffold,
            result.Value.BoundaryDecision.Kind);
    }

    [Fact]
    public async Task AuthorityFirstPipeline_FailingAdmissionMeasurementBlocksBeforeMutation()
    {
        var mutationBoundaries = new List<string>();
        var optimizerSinks = new List<string>();
        var previousBoundary =
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        var previousOptimizer =
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            mutationBoundaries.Add;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            observation => optimizerSinks.Add(observation.Stage);

        try
        {
            var result = await new DesignConversionTrainingPipeline().RunAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                new DesignConversionMachine(),
                BuildSyntheticSourceDisjointRecords(100),
                new DesignConversionTrainingPipelineOptions(
                    "authority-failing-admission-measurement",
                    BatchSize: 25,
                    DatasetOptions: LocalDatasetOptions(),
                    MeasurementGateRequest: FailingAdmissionMeasurement(
                        "below_score")));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.False(result.Value!.MachineLifecycleHookAttempted);
            Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);
            Assert.Equal(
                DesignConversionMeasurementGateStatus.Failed,
                result.Value.LifecycleEvidence.MeasurementGateStatus);
            Assert.Contains(
                "CALLER_ADMISSION_MEASUREMENT_BLOCKED_BEFORE_MACHINE_MUTATION",
                result.Value.LifecycleEvidence.EvidenceRefs);
            Assert.Empty(mutationBoundaries);
            Assert.Empty(optimizerSinks);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousOptimizer;
        }
    }

    [Fact]
    public async Task AuthorityFirstPipeline_ExternalTeacherWithoutCustodyFailsBeforeMutation()
    {
        var records = BuildRecords(
        [
            DesignConversionDpoGeneratorTests.CreateExample(
                "authority-external-no-custody",
                DesignConversionDpoUnits.CriticUnit)
        ]);
        var externalRecord = Assert.Single(records) with
        {
            TeacherProvenance = new TrainingTeacherProvenance(
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
                RequestId: "authority-external-no-custody",
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = new string('c', 64)
            }
        };
        var machine = new DesignConversionMachine();
        var versionBefore = ExportMachineVersion(machine);
        var trainBatchCallCount = 0;
        var pipeline = new DesignConversionTrainingPipeline(
            trainBatchObserver: _ => trainBatchCallCount++);

        var result = await pipeline.RunAsync(
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            machine,
            [externalRecord],
            new DesignConversionTrainingPipelineOptions(
                "authority-external-no-custody",
                BatchSize: 1,
                CallMachineLifecycleHook: true,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_TRAINING_CUSTODY_AUTHORITY_REQUIRED",
            result.ErrorCode);
        Assert.Equal(0, trainBatchCallCount);
        Assert.Equal(versionBefore, ExportMachineVersion(machine));
    }

    [Fact]
    public async Task AuthorityFirstPipeline_DeepSnapshotsRecordsAndNestedOptionsBeforeLeaseObserver()
    {
        var source = Assert.Single(BuildRecords(
        [
            DesignConversionDpoGeneratorTests.CreateExample(
                "authority-deep-snapshot",
                DesignConversionDpoUnits.CriticUnit)
        ]));
        var vectorAlias = new[] { 0.125d, 0.5d };
        var chosenEvidenceAlias = new[] { "chosen-evidence-original" };
        var provenanceMetadataAlias = new Dictionary<string, string>(
            source.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            ["snapshot_alias"] = "record-original"
        };
        var record = source with
        {
            InternalVectorPayload = vectorAlias,
            Chosen = source.Chosen with
            {
                EvidenceRefs = chosenEvidenceAlias
            },
            Provenance = source.Provenance with
            {
                Metadata = provenanceMetadataAlias
            }
        };
        var preflight = LocalPreflight();
        var preflightReasonsAlias = new[] { "preflight-reason-original" };
        var preflightEvidenceAlias = new[] { "preflight-evidence-original" };
        var options = new DesignConversionTrainingPipelineOptions(
            "authority-deep-snapshot",
            BatchSize: 1,
            CallMachineLifecycleHook: false,
            RequireDatasetCountGate: false,
            DatasetOptions: LocalDatasetOptions() with
            {
                Preflight = preflight with
                {
                    Reasons = preflightReasonsAlias,
                    EvidenceRefs = preflightEvidenceAlias
                }
            });
        var previousObserver =
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting;
        DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting = _ =>
        {
            vectorAlias[0] = 99d;
            chosenEvidenceAlias[0] = "chosen-evidence-mutated";
            provenanceMetadataAlias["snapshot_alias"] = "record-mutated";
            preflightReasonsAlias[0] = "preflight-reason-mutated";
            preflightEvidenceAlias[0] = "preflight-evidence-mutated";
        };

        OperationResult<DesignConversionTrainingPipelineResult> result;
        try
        {
            result = await new DesignConversionTrainingPipeline().RunAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                new DesignConversionMachine(),
                [record],
                options);
        }
        finally
        {
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
                previousObserver;
        }

        Assert.Same(
            previousObserver,
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var consumed = Assert.Single(Assert.Single(result.Value!.Batches).Records);
        Assert.Equal(0.125d, consumed.InternalVectorPayload[0]);
        Assert.Equal("chosen-evidence-original", consumed.Chosen.EvidenceRefs[0]);
        Assert.Equal("record-original", consumed.Provenance.Metadata["snapshot_alias"]);
        Assert.Equal(
            "preflight-reason-original",
            result.Value.DatasetManifest.Preflight.Reasons[0]);
        Assert.Equal(
            "preflight-evidence-original",
            result.Value.DatasetManifest.Preflight.EvidenceRefs[0]);
    }

    [Fact]
    public async Task StrictPipeline_SuccessAcquiresMandatoryMutationBoundariesInOrder()
    {
        var records = BuildSyntheticSourceDisjointRecords(2);
        var boundaries = new List<string>();
        var mutationSinks = new List<
            DesignConversionTrainingPipeline
                .DesignConversionOptimizerMutationSinkObservation>();
        var previousObserver =
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        var previousSinkObserver =
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting = boundaries.Add;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            mutationSinks.Add;
        try
        {
            var result = await RunStrictPipelineAsync(
                new DesignConversionTrainingPipeline(),
                new DesignConversionMachine(),
                records,
                new DesignConversionTrainingPipelineOptions(
                    "strict-boundary-order",
                    BatchSize: 1,
                    DatasetOptions: LocalDatasetOptions(
                        "strict-boundary-order",
                        MinimumTrainRecords: 1,
                        MinimumHeldOutRecords: 1)));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(
            [
                "pipeline_train_batch:0",
                "pipeline_train_batch_result:0",
                "pipeline_checkpoint_import",
                "pipeline_checkpoint_import_result",
                "pipeline_fresh_load_probe",
                "pipeline_fresh_load_probe_result",
                "pipeline_checkpoint_import_async",
                "pipeline_checkpoint_import_async_result",
                "pipeline_continue_probe",
                "pipeline_continue_probe_result",
                "pipeline_final"
            ], boundaries);
            var primary = Assert.Single(
                mutationSinks,
                static sink => sink.Stage == "primary");
            var continueProbe = Assert.Single(
                mutationSinks,
                static sink => sink.Stage == "continue_probe");
            Assert.Same(
                Assert.Single(primary.TrainingItems),
                Assert.Single(continueProbe.TrainingItems));
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousObserver;
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting =
                previousSinkObserver;
        }
    }

    [Fact]
    public async Task PreparedPipeline_RejectsDifferentExactBindingBeforeMutationSink()
    {
        var pipeline = new DesignConversionTrainingPipeline();
        var firstRecords = BuildSyntheticSourceDisjointRecords(2);
        var secondRecords = BuildSyntheticSourceDisjointRecords(2)
            .Select(static record => record with
            {
                RecordId = record.RecordId + ":other"
            })
            .ToArray();
        var firstOptions = new DesignConversionTrainingPipelineOptions(
            "binding-first",
            BatchSize: 1,
            RequireDatasetCountGate: false,
            DatasetOptions: LocalDatasetOptions("binding-first", 1, 1),
            MeasurementGateRequest: PassingAdmissionMeasurement(
                "binding-first"));
        var secondOptions = firstOptions with { RunId = "binding-second" };
        var first = pipeline.PrepareExecution(firstRecords, firstOptions);
        var second = pipeline.PrepareExecution(secondRecords, secondOptions);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.NotNull(first.Value!.FinalOptimizer);
        Assert.NotNull(second.Value!.FinalOptimizer);
        var leaseResult = DesignConversionLegalTeacherCapabilityTestFactory
            .CreateAdmitted()
            .AcquireTrainingLease(
                first.Value.FinalOptimizer!.Content,
                first.Value.FinalOptimizer.Provenance);
        Assert.True(leaseResult.IsSuccess, leaseResult.ErrorMessage);
        using var lease = leaseResult.Value!;
        var binding = DesignConversionPipelineTrainingBinding.MintExact(
            first.Value.FinalOptimizer,
            lease);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        var sinkCount = 0;
        var mutationBoundaries = new List<string>();
        var previous = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        var previousBoundary = DesignConversionTrainingPipeline
            .MutationBoundaryObserverForTesting;
        DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting = _ => sinkCount++;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            mutationBoundaries.Add;
        try
        {
            var result = await pipeline.RunValidatedAsync(
                new DesignConversionMachine(),
                secondRecords,
                secondOptions,
                custodyCapability: null,
                binding.Value,
                second.Value,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode,
                result.ErrorCode);
            Assert.Empty(mutationBoundaries);
            Assert.Equal(0, sinkCount);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting = previous;
        }
    }

    [Fact]
    public async Task AuthorityFirstPipeline_ReleasesSingleLeaseOnSuccessFailureAndObserverThrow()
    {
        var records = BuildSyntheticSourceDisjointRecords(2);
        var options = new DesignConversionTrainingPipelineOptions(
            "lease-release",
            BatchSize: 1,
            DatasetOptions: LocalDatasetOptions(
                "lease-release",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1));
        var previousObserver =
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting;

        try
        {
            ServerOwnedLegalTrainingTeacherAdmissionLease? successLease = null;
            var successLeaseCount = 0;
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting = lease =>
            {
                successLease = lease;
                successLeaseCount++;
            };
            var success = await new DesignConversionTrainingPipeline().RunAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                new DesignConversionMachine(),
                records,
                options);
            Assert.True(success.IsSuccess, success.ErrorMessage);
            Assert.Equal(1, successLeaseCount);
            AssertLeaseDisposed(successLease);

            ServerOwnedLegalTrainingTeacherAdmissionLease? failureLease = null;
            var failureLeaseCount = 0;
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting = lease =>
            {
                failureLease = lease;
                failureLeaseCount++;
            };
            var failure = await new DesignConversionTrainingPipeline().RunAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                null!,
                records,
                options);
            Assert.False(failure.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_MACHINE_REQUIRED", failure.ErrorCode);
            Assert.Equal(1, failureLeaseCount);
            AssertLeaseDisposed(failureLease);

            ServerOwnedLegalTrainingTeacherAdmissionLease? observerThrowLease = null;
            var observerThrowLeaseCount = 0;
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting = lease =>
            {
                observerThrowLease = lease;
                observerThrowLeaseCount++;
                throw new InvalidOperationException("observer failure");
            };
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new DesignConversionTrainingPipeline().RunAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    new DesignConversionMachine(),
                    records,
                    options));
            Assert.Equal(1, observerThrowLeaseCount);
            AssertLeaseDisposed(observerThrowLease);
        }
        finally
        {
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting =
                previousObserver;
        }

        Assert.Same(
            previousObserver,
            DesignConversionTrainingPipeline.TrainingLeaseObserverForTesting);
    }

    [Fact]
    public async Task RunAsync_GroupsAcceptedPairsIntoUnitBatchesWithoutLifecycleWhenCountGateBypassed()
    {
        var records = BuildRecords([
            DesignConversionDpoGeneratorTests.CreateExample("critic-1", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
            DesignConversionDpoGeneratorTests.CreateExample("critic-2", DesignConversionDpoUnits.CriticUnit, usefulness: 0.80),
            DesignConversionDpoGeneratorTests.CreateExample("critic-3", DesignConversionDpoUnits.CriticUnit, usefulness: 0.70),
            DesignConversionDpoGeneratorTests.CreateExample("improver-1", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.90),
            DesignConversionDpoGeneratorTests.CreateExample("improver-2", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.60)
        ]);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await RunStrictPipelineAsync(
            pipeline,
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "slice-4",
                BatchSize: 2,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(5, result.Value!.AcceptedRecordCount);
        Assert.Equal(0, result.Value.QuarantinedRecordCount);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGateBypassedForScaffold,
            result.Value.BoundaryDecision.Kind);
        Assert.False(result.Value.BoundaryDecision.AllowsMachineLifecycleHook);
        Assert.Equal(3, result.Value.Batches.Count);
        Assert.Equal([2, 1, 2], result.Value.Batches.Select(batch => batch.Records.Count).ToArray());
        Assert.Empty(result.Value.BatchResults);
        Assert.Equal(5, result.Value.TrainingItems.Count);
        Assert.False(result.Value.MachineLifecycleHookAttempted);
        Assert.False(result.Value.LifecycleEvidence.MachineLifecycleHookCalled);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.CheckpointCandidateStatus);
        Assert.Contains("DESIGN_CONVERSION_COUNT_GATE_BYPASS_DOES_NOT_ALLOW_MACHINE_LIFECYCLE_HOOK",
            result.Value.BoundaryDecision.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_PIPELINE_SCAFFOLD_NO_REAL_MODEL_MATH", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_ExcludesQuarantinedRecordsFromTrainingItems()
    {
        var valid = BuildRecords([DesignConversionDpoGeneratorTests.CreateExample("valid", DesignConversionDpoUnits.CriticUnit)]).Single();
        var quarantined = valid with
        {
            RecordId = "quarantined",
            Quarantine = new DesignConversionQuarantineState(
                IsQuarantined: true,
                Reason: "test_quarantine",
                EvidenceRefs: ["test"])
        };
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await RunStrictPipelineAsync(
            pipeline,
            new DesignConversionMachine(),
            [valid, quarantined],
            new DesignConversionTrainingPipelineOptions(
                "slice-4-quarantine",
                BatchSize: 4,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.AcceptedRecordCount);
        Assert.Equal(1, result.Value.QuarantinedRecordCount);
        Assert.Single(result.Value.TrainingItems);
        Assert.Equal("valid", result.Value.TrainingItems[0].ItemId);
        Assert.Empty(result.Value.BatchResults);
        Assert.False(result.Value.MachineLifecycleHookAttempted);
        Assert.Contains("checkpoint_candidate", result.Value.LifecycleEvidence.NotProvenFields);
    }

    [Fact]
    public async Task RunTrainingRecordsAsync_ExcludesPendingStatusTrainingGradeAndNegativeMetadata()
    {
        var records = BuildRecords(
        [
            DesignConversionDpoGeneratorTests.CreateExample("metadata-allowed", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-status-pending", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-status-pending-not-training-grade", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-status-pending-negative-pair-not-proven", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-pending", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-approved-false", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-not-training-grade", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-training-grade-false", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-synthetic-negative", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-scaffold-negative", DesignConversionDpoUnits.CriticUnit),
            DesignConversionDpoGeneratorTests.CreateExample("metadata-negative-pair-not-proven", DesignConversionDpoUnits.CriticUnit)
        ]).ToArray();
        var allowed = records.Single(record => record.RecordId == "metadata-allowed");
        var blocked = new[]
        {
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-status-pending"),
                ("status", "pending")),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-status-pending-not-training-grade"),
                ("status", "pending"),
                ("not_training_grade", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-status-pending-negative-pair-not-proven"),
                ("status", "pending"),
                ("high_quality_negative_pair_not_proven", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-pending"),
                ("approval_status", "pending"),
                ("review_status", "unapproved")),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-approved-false"),
                ("approved_for_training", bool.FalseString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-not-training-grade"),
                ("not_training_grade", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-training-grade-false"),
                ("training_grade", bool.FalseString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-synthetic-negative"),
                ("synthetic_negative", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-scaffold-negative"),
                ("scaffold_negative", bool.TrueString)),
            WithProvenanceMetadata(
                records.Single(record => record.RecordId == "metadata-negative-pair-not-proven"),
                ("high_quality_negative_pair_not_proven", bool.TrueString))
        };

        var result = await DesignConversionLegalTeacherCapabilityTestFactory.RunStrictRunnerAsync(
            new DesignConversionTrainingRunner(),
            new DesignConversionMachine(),
            blocked.Prepend(allowed).ToArray(),
            new DesignConversionTrainingPipelineOptions(
                "hd3-explicit-metadata-exclusion",
                BatchSize: 4,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal([allowed.RecordId], result.Value!.DpoBuild.AcceptedRecords.Select(record => record.RecordId).ToArray());
        Assert.Equal(blocked.Select(record => record.RecordId).Order(StringComparer.Ordinal),
            result.Value.DpoBuild.QuarantinedRecords.Select(record => record.RecordId).Order(StringComparer.Ordinal));
        Assert.Equal(1, result.Value.PipelineResult.AcceptedRecordCount);
        var item = Assert.Single(result.Value.PipelineResult.TrainingItems);
        Assert.Equal("metadata-allowed", item.ItemId);
        Assert.Empty(result.Value.PipelineResult.BatchResults);
        Assert.False(result.Value.PipelineResult.MachineLifecycleHookAttempted);
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[0]));
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[1]));
        Assert.Contains("not_training_grade",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[1]));
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[2]));
        Assert.Contains("high_quality_negative_pair_not_proven",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[2]));
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[3]));
        Assert.Contains("approval_missing",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[4]));
        Assert.Contains("not_training_grade",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[5]));
        Assert.Contains("not_training_grade",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[6]));
        Assert.Contains("synthetic_negative",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[7]));
        Assert.Contains("scaffold_negative",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[8]));
        Assert.Contains("high_quality_negative_pair_not_proven",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked[9]));
    }

    [Fact]
    public async Task RunTrainingRecordsAsync_ExcludesEvalOnlyLegalVerdictRowsEvenIfApprovedForTraining()
    {
        var records = BuildRecords(
        [
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "legal-verdict-runner-accepted-train",
                    DesignConversionDpoUnits.CriticUnit),
                "legal-verdict-runner-source-a"),
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "legal-verdict-runner-accepted-heldout",
                    DesignConversionDpoUnits.ImproverUnit),
                "legal-verdict-runner-source-b"),
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "legal-verdict-runner-eval-only",
                    DesignConversionDpoUnits.CriticUnit),
                "legal-verdict-runner-source-c")
        ]).ToArray();
        var accepted = records
            .Where(record => record.RecordId != "legal-verdict-runner-eval-only")
            .ToArray();
        var evalOnly = WithProvenanceMetadata(
            records.Single(record => record.RecordId == "legal-verdict-runner-eval-only"),
            ("runtime_created_dpo_candidate", bool.TrueString),
            ("approval_status", "approved"),
            ("approved_for_training", bool.TrueString),
            ("training_candidate_only", bool.TrueString),
            ("product_not_ready", bool.TrueString),
            ("model_readiness_claimed", bool.FalseString),
            ("trained_model_ready", bool.FalseString),
            ("production_dpo", bool.FalseString),
            ("legalVerdict", "EvalOnly"));
        var observations = new List<DesignConversionTrainBatchObservation>();
        var runner = new DesignConversionTrainingRunner(
            pipeline: new DesignConversionTrainingPipeline(
                trainBatchObserver: observations.Add));

        var result = await DesignConversionLegalTeacherCapabilityTestFactory.RunStrictRunnerAsync(
            runner,
            new DesignConversionMachine(),
            accepted.Append(evalOnly).ToArray(),
            new DesignConversionTrainingPipelineOptions(
                "legal-verdict-runner-eval-only",
                BatchSize: 4,
                RequireDatasetCountGate: true,
                DatasetOptions: LocalDatasetOptions(
                    "legal-verdict-runner-eval-only",
                    MinimumTrainRecords: 1,
                    MinimumHeldOutRecords: 1)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            accepted.Select(record => record.RecordId).Order(StringComparer.Ordinal),
            result.Value!.DpoBuild.AcceptedRecords.Select(record => record.RecordId).Order(StringComparer.Ordinal));
        var blocked = Assert.Single(result.Value.DpoBuild.QuarantinedRecords);
        Assert.Equal(evalOnly.RecordId, blocked.RecordId);
        Assert.Contains("eval_only_legal_verdict",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(blocked));
        Assert.DoesNotContain(evalOnly.RecordId,
            result.Value.PipelineResult.TrainingItems.Select(item => item.ItemId));
        Assert.DoesNotContain(evalOnly.RecordId,
            observations.SelectMany(observation => observation.TrainingItemIds));
    }

    [Fact]
    public async Task RunAsync_DoesNotLeakProvenanceLicenseQuarantineOrDeterministicEvidenceIntoTrainingItems()
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample("no-leakage", DesignConversionDpoUnits.CriticUnit) with
        {
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "source-request-should-not-be-feature",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://source-redacted",
                ReviewerRef: "reviewer-redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["deterministicEvidenceRefs"] = "deterministic-input-evidence:fixture=qa",
                    ["license"] = "project-local"
                }),
            TeacherEvidenceRefs = ["deterministic-input-evidence:fixture=qa"]
        };
        var records = BuildRecords([example]);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await RunStrictPipelineAsync(
            pipeline,
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "slice-4-no-leakage",
                BatchSize: 2,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var item = Assert.Single(result.Value!.TrainingItems);
        var payload = string.Join("\n", item.Prompt, item.ChosenResponse, item.RejectedResponse);
        Assert.DoesNotContain("source-request-should-not-be-feature", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("deterministic-input-evidence", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("license", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("quarantine", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("reviewer-redacted", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_CarriesSourceEncoderFeatureMapAndVectorIntoTrainingItems()
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample("source-encoder-training", DesignConversionDpoUnits.ImproverUnit) with
        {
            InternalVectorPayload = [],
            SourceStructurePayload = """
                {
                  "root":{"type":"FRAME","layoutMode":"VERTICAL","width":1280,"height":720},
                  "children":[{"type":"TEXT"},{"type":"COMPONENT","name":"cta_button"}],
                  "breakpoints":["desktop"]
                }
                """,
            Chosen = DesignConversionDpoGeneratorTests.CreateOutput(
                "source-encoder-training-chosen",
                accepted: true,
                score: 0.91,
                htmlSuffix: "source-encoder-chosen") with
            {
                Html = "<main class=\"screen grid\"><button class=\"cta\">Save</button></main>",
                Css = ".screen.grid{display:grid;gap:24px}.cta{padding:12px;border-radius:8px}"
            },
            Rejected = DesignConversionDpoGeneratorTests.CreateOutput(
                "source-encoder-training-rejected",
                accepted: false,
                score: 0.38,
                htmlSuffix: "source-encoder-rejected") with
            {
                Html = "<main class=\"screen\"><button>Save</button></main>",
                Css = ".screen{display:block}.cta{padding:0}"
            }
        };
        var records = BuildRecords([example]);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await RunStrictPipelineAsync(
            pipeline,
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "source-encoder-training-intake",
                BatchSize: 1,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var item = Assert.Single(result.Value!.TrainingItems);
        using var promptDocument = JsonDocument.Parse(item.Prompt);
        var promptRoot = promptDocument.RootElement;
        var sourceEncoder = promptRoot.GetProperty("source_encoder");
        Assert.True(sourceEncoder.TryGetProperty("vector_prefix", out var internalVector));
        Assert.True(internalVector.GetArrayLength() > 0);
        Assert.NotEmpty(sourceEncoder.GetProperty("feature_names").EnumerateArray());
        Assert.Equal(
            "source_kind_source_id_not_vector_features",
            sourceEncoder.GetProperty("shortcut_guard").GetString());
        Assert.Contains("source_json_node_count", item.Prompt, StringComparison.Ordinal);
        Assert.False(sourceEncoder.TryGetProperty("vector_hash", out _));
        Assert.False(promptRoot.GetProperty("source").TryGetProperty("manifest_hash", out _));
        Assert.False(promptRoot.GetProperty("source").TryGetProperty("identity_hash", out _));
        Assert.False(promptRoot.GetProperty("source").TryGetProperty("structure_hash", out _));
        Assert.False(promptRoot.TryGetProperty("pair_family", out _));
        Assert.False(promptRoot.TryGetProperty("internal_unit_state", out _));
        Assert.False(promptRoot.TryGetProperty("result_structure_features", out _));
        Assert.DoesNotContain("accepted_flag_delta", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("visual_score_margin", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("chosen_visual_score", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("rejected_visual_score", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("usefulness_score", item.Prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("layoutMode\":\"VERTICAL", item.ChosenResponse, StringComparison.OrdinalIgnoreCase);
        using var chosenDocument = JsonDocument.Parse(item.ChosenResponse);
        Assert.Equal(2, chosenDocument.RootElement.EnumerateObject().Count());
        Assert.True(chosenDocument.RootElement.TryGetProperty("html", out _));
        Assert.True(chosenDocument.RootElement.TryGetProperty("css", out _));
        Assert.False(chosenDocument.RootElement.TryGetProperty("candidate_id", out _));
        Assert.False(chosenDocument.RootElement.TryGetProperty("output_id", out _));
        Assert.False(chosenDocument.RootElement.TryGetProperty("visual_score", out _));
        Assert.False(chosenDocument.RootElement.TryGetProperty("accepted", out _));

        var build = new DesignConversionDpoBuildResult(records, [], []);
        var runResult = new DesignConversionTrainingRunResult(
            build,
            result.Value.DatasetManifest,
            result.Value.DatasetCountGate,
            result.Value.BoundaryDecision,
            result.Value,
            []);
        object[] publicSerializationSurfaces =
        [
            build,
            result.Value,
            Assert.Single(result.Value.Batches),
            runResult
        ];
        Assert.All(publicSerializationSurfaces, surface =>
        {
            var serialized = JsonSerializer.Serialize(
                surface,
                surface.GetType(),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.DoesNotContain(
                "<main class=\"screen grid\"",
                serialized,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "gap:24px",
                serialized,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "<main class=\"screen\"><button>Save</button></main>",
                serialized,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                ".screen{display:block}",
                serialized,
                StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task ManifestPublisher_ExportsPromotionMetadataWithoutModelServerUpload()
    {
        var records = BuildRecords([DesignConversionDpoGeneratorTests.CreateExample("manifest-record", DesignConversionDpoUnits.ImproverUnit)]);
        var pipeline = new DesignConversionTrainingPipeline();
        var machine = new DesignConversionMachine();
        var pipelineResult = await RunStrictPipelineAsync(
            pipeline,
            machine,
            records,
            new DesignConversionTrainingPipelineOptions(
                "slice-4-manifest",
                BatchSize: 2,
                RequireDatasetCountGate: false,
                DatasetOptions: LocalDatasetOptions()));
        Assert.True(pipelineResult.IsSuccess, pipelineResult.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"design-conversion-manifest-publisher-{Guid.NewGuid():N}");
        var checkpointPath = Path.Combine(root, "checkpoint.json");

        try
        {
            var save = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            var load = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(load.IsSuccess, load.ErrorMessage);

            var manifest = await new DesignConversionManifestPublisher().PublishManifestMetadataAsync(
                load.Value!.Machine,
                pipelineResult.Value!,
                "slice-4-manifest-metadata");

            Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
            Assert.Equal("slice-4-manifest-metadata", manifest.Value!.ManifestId);
            Assert.Equal(DesignConversionMachine.MachineId, manifest.Value.MachineId);
            Assert.Equal(DesignConversionMachine.DomainTag, manifest.Value.DomainTag);
            Assert.Equal(Path.GetFullPath(checkpointPath), manifest.Value.CheckpointRef);
            Assert.Equal(save.Value!.ManifestRef, manifest.Value.ManifestRef);
            Assert.StartsWith("manifest://design-conversion-machine/", manifest.Value.ManifestRef, StringComparison.Ordinal);
            Assert.False(manifest.Value.ModelServerUploadAttempted);
            Assert.False(manifest.Value.ModelServerPromotionClaimed);
            Assert.False(manifest.Value.DeterministicLogicClaimedAsLearning);
            Assert.Contains("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED", manifest.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_MANIFEST_DATASET_COUNT_GATE_NOT_PASSED", manifest.Value.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LIFECYCLE_EVIDENCE_NOT_ATTEMPTED", manifest.Value.EvidenceRefs);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_RefusesLifecycleHookWhenDatasetCountGateLacksRequiredTrainAndHeldOutRecords()
    {
        var records = BuildSyntheticSourceDisjointRecords(3);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await RunStrictPipelineAsync(
            pipeline,
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-count-gate-fail",
                BatchSize: 2,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGateFailed, result.Value!.BoundaryDecision.Kind);
        Assert.True(result.Value.BoundaryDecision.CountGateRequired);
        Assert.False(result.Value.BoundaryDecision.CountGatePassed);
        Assert.False(result.Value.BoundaryDecision.AllowsMachineLifecycleHook);
        Assert.False(result.Value.BoundaryDecision.RealTrainingRunClaimed);
        Assert.False(result.Value.BoundaryDecision.CheckpointClaimed);
        Assert.False(result.Value.BoundaryDecision.PromotionClaimed);
        Assert.False(result.Value.MachineLifecycleHookAttempted);
        Assert.False(result.Value.LifecycleEvidence.MachineLifecycleHookCalled);
        Assert.False(result.Value.LifecycleEvidence.DatasetCountGatePassed);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.CheckpointCandidateStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.FreshLoadStatus);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", result.Value.LifecycleEvidence.PromotionStatus);
        Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);
        Assert.Contains("DATASET_COUNT_GATE_NOT_PASSED", result.Value.LifecycleEvidence.FailureReasons);
        Assert.False(result.Value.DatasetCountGate.Passed);
        Assert.Equal(50, result.Value.DatasetCountGate.RequiredTrainRecords);
        Assert.Equal(50, result.Value.DatasetCountGate.RequiredHeldOutRecords);
        Assert.True(result.Value.DatasetCountGate.ActualTrainRecords < 50);
        Assert.True(result.Value.DatasetCountGate.ActualHeldOutRecords < 50);
        Assert.Empty(result.Value.Batches);
        Assert.Empty(result.Value.BatchResults);
        Assert.Empty(result.Value.TrainingItems);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_BOUNDARY_REFUSED_BY_DATASET_COUNT_GATE",
            result.Value.BoundaryDecision.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MACHINE_TRAINBATCH_LIFECYCLE_HOOK_NOT_CALLED", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_SourceDisjointHundredAcceptedRecordsPassCountGateWithoutCallingTraining()
    {
        var records = BuildSyntheticSourceDisjointRecords(100);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await RunStrictPipelineAsync(
            pipeline,
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-count-gate-pass-no-training",
                BatchSize: 25,
                CallMachineLifecycleHook: false,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGatePassed, result.Value!.BoundaryDecision.Kind);
        Assert.True(result.Value.BoundaryDecision.CountGateRequired);
        Assert.True(result.Value.BoundaryDecision.CountGatePassed);
        Assert.True(result.Value.BoundaryDecision.AllowsMachineLifecycleHook);
        Assert.False(result.Value.BoundaryDecision.RealTrainingRunClaimed);
        Assert.False(result.Value.BoundaryDecision.CheckpointClaimed);
        Assert.False(result.Value.BoundaryDecision.PromotionClaimed);
        Assert.True(result.Value.DatasetCountGate.Passed);
        Assert.Equal(50, result.Value.DatasetCountGate.RequiredTrainRecords);
        Assert.Equal(50, result.Value.DatasetCountGate.RequiredHeldOutRecords);
        Assert.True(result.Value.DatasetCountGate.ActualTrainRecords >= 50);
        Assert.True(result.Value.DatasetCountGate.ActualHeldOutRecords >= 50);
        Assert.True(result.Value.DatasetCountGate.SourceDisjoint);
        Assert.Equal(100, result.Value.DatasetManifest.AcceptedRecordCount);
        Assert.False(result.Value.MachineLifecycleHookAttempted);
        Assert.Empty(result.Value.BatchResults);
        Assert.Equal(50, result.Value.TrainingItems.Count);
        Assert.True(result.Value.LifecycleEvidence.DatasetCountGatePassed);
        Assert.False(result.Value.LifecycleEvidence.MachineLifecycleHookCalled);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.CheckpointCandidateStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.ExportStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.ImportStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.FreshLoadStatus);
        Assert.Equal("not_proven", result.Value.LifecycleEvidence.ContinueTrainingStatus);
        Assert.Equal("not_run", result.Value.LifecycleEvidence.MeasurementGateStatus);
        Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);
        Assert.Contains("LIFECYCLE_HOOK_DISABLED_BY_OPTIONS", result.Value.LifecycleEvidence.FailureReasons);
        Assert.Contains("COUNT_GATE_PASSED", result.Value.DatasetCountGate.Reasons);
        Assert.Contains("DESIGN_CONVERSION_DPO_DATASET_MANIFEST_ATTACHED_TO_TRAINING_BOUNDARY",
            result.Value.BoundaryDecision.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MACHINE_TRAINBATCH_LIFECYCLE_HOOK_NOT_CALLED", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_SourceDisjointHundredAcceptedRecordsPassCountGateAndCallsLifecycleHookWithHonestEvidence()
    {
        var records = BuildSyntheticSourceDisjointRecords(100);
        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(records, LocalDatasetOptions());
        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.Equal(50, dataset.Value!.TrainRows.Count);
        Assert.Equal(50, dataset.Value.HeldOutRows.Count);

        var trainRowHashes = dataset.Value.TrainRows
            .Select(row => row.RecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        var heldOutRowHashes = dataset.Value.HeldOutRows
            .Select(row => row.RecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        var trainRecordIds = records
            .Where(record => trainRowHashes.Contains(ToRecordIdentityHash(record)))
            .Select(record => record.RecordId)
            .ToHashSet(StringComparer.Ordinal);
        var heldOutRecordIds = records
            .Where(record => heldOutRowHashes.Contains(ToRecordIdentityHash(record)))
            .Select(record => record.RecordId)
            .ToHashSet(StringComparer.Ordinal);
        var observations = new List<DesignConversionTrainBatchObservation>();
        var observerAttemptedReplacementRowsTrained = -999;
        var pipeline = new DesignConversionTrainingPipeline(
            trainBatchObserver: observation =>
            {
                observations.Add(observation);
                observerAttemptedReplacementRowsTrained = int.MaxValue;
            });
        var constructorParameters = typeof(DesignConversionTrainingPipeline).GetConstructors()
            .SelectMany(ctor => ctor.GetParameters())
            .ToArray();
        Assert.DoesNotContain(constructorParameters, parameter => parameter.Name == "trainBatchAsync");
        Assert.Contains(constructorParameters, parameter =>
            parameter.Name == "trainBatchObserver" &&
            parameter.ParameterType == typeof(Action<DesignConversionTrainBatchObservation>));

        var result = await RunStrictPipelineAsync(
            pipeline,
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-local-lifecycle-pass",
                BatchSize: 25,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGatePassed, result.Value!.BoundaryDecision.Kind);
        Assert.True(result.Value.DatasetCountGate.Passed);
        Assert.True(result.Value.BoundaryDecision.AllowsMachineLifecycleHook);
        Assert.True(result.Value.MachineLifecycleHookAttempted);
        Assert.Equal(result.Value.Batches.Count, result.Value.BatchResults.Count);
        Assert.Equal(50, result.Value.BatchResults.Sum(batch => batch.RowsTrained));
        Assert.All(result.Value.BatchResults, batch => Assert.Empty(batch.PromotedUnitIds));
        Assert.Equal(result.Value.Batches.Count, observations.Count);
        Assert.Equal(50, observations.Sum(observation => observation.TrainingItemCount));
        Assert.Equal(
            result.Value.BatchResults.Select(batch => batch.BatchId).Order(StringComparer.Ordinal),
            observations.Select(observation => observation.BatchId).Order(StringComparer.Ordinal));
        Assert.All(observations, observation => Assert.Equal(DesignConversionMachine.DomainTag, observation.DomainTag));
        Assert.Equal(50, result.Value.BatchResults.Sum(batch => batch.RowsTrained));
        Assert.NotEqual(observerAttemptedReplacementRowsTrained, result.Value.BatchResults.Sum(batch => batch.RowsTrained));

        var observedTrainingItemIds = observations
            .SelectMany(observation => observation.TrainingItemIds)
            .ToArray();
        Assert.Equal(trainRecordIds.Order(StringComparer.Ordinal), observedTrainingItemIds.Order(StringComparer.Ordinal));
        Assert.All(observedTrainingItemIds, id => Assert.DoesNotContain(id, heldOutRecordIds));

        var trainedRecordHashes = result.Value.Batches
            .SelectMany(batch => batch.Records)
            .Select(ToRecordIdentityHash)
            .ToArray();
        Assert.Equal(50, trainedRecordHashes.Length);
        Assert.Equal(trainRowHashes.Order(StringComparer.Ordinal), trainedRecordHashes.Order(StringComparer.Ordinal));
        Assert.All(trainedRecordHashes, hash => Assert.DoesNotContain(hash, heldOutRowHashes));
        Assert.Equal(50, result.Value.TrainingItems.Count);

        var lifecycle = result.Value.LifecycleEvidence;
        Assert.True(lifecycle.DatasetCountGatePassed);
        Assert.True(lifecycle.MachineLifecycleHookCalled);
        Assert.Equal(result.Value.Batches.Count, lifecycle.LocalLifecycleBatchResultCount);
        Assert.Equal("local_checkpoint_candidate_with_real_learned_weights", lifecycle.CheckpointCandidateStatus);
        Assert.True(lifecycle.CheckpointCandidateCreated);
        Assert.True(lifecycle.CheckpointContainsRealTrainedWeights);
        Assert.Equal("in_process_checkpoint_import_verified", lifecycle.CheckpointImportStatus);
        Assert.Equal("checkpoint_backed_export_verified", lifecycle.ExportStatus);
        Assert.Equal("checkpoint_backed_import_verified", lifecycle.ImportStatus);
        Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, lifecycle.FreshLoadStatus);
        Assert.NotNull(lifecycle.LocalFreshLoadProbe);
        Assert.True(lifecycle.LocalFreshLoadProbe!.MetadataContinuityVerified);
        Assert.Equal(DesignConversionMachine.MachineId, lifecycle.LocalFreshLoadProbe.MachineId);
        Assert.Equal(DesignConversionMachine.DomainTag, lifecycle.LocalFreshLoadProbe.DomainTag);
        Assert.True(lifecycle.LocalFreshLoadProbe.CheckpointContainsRealTrainedWeights);
        Assert.False(lifecycle.LocalFreshLoadProbe.ContinueTrainingProven);
        Assert.False(lifecycle.LocalFreshLoadProbe.MeasurementGateRun);
        Assert.False(lifecycle.LocalFreshLoadProbe.PromotionUploadAllowed);
        Assert.False(File.Exists(lifecycle.LocalFreshLoadProbe.CheckpointRef));
        Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, lifecycle.ContinueTrainingStatus);
        Assert.NotNull(lifecycle.LocalContinueTrainingProbe);
        Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, lifecycle.LocalContinueTrainingProbe!.Status);
        Assert.Equal(lifecycle.LocalFreshLoadProbe.CheckpointRef, lifecycle.LocalContinueTrainingProbe.SourceCheckpointRef);
        Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), lifecycle.LocalContinueTrainingProbe.ContinuedCheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(lifecycle.LocalContinueTrainingProbe.ContinuedCheckpointRef));
        Assert.True(lifecycle.LocalContinueTrainingProbe.FreshLoadMetadataContinuityVerified);
        Assert.True(lifecycle.LocalContinueTrainingProbe.DatasetCountGatePassed);
        Assert.True(lifecycle.LocalContinueTrainingProbe.TrainSplitOnly);
        Assert.True(lifecycle.LocalContinueTrainingProbe.SourceCheckpointUnchanged);
        Assert.Equal(
            lifecycle.LocalContinueTrainingProbe.SourceCheckpointSha256Before,
            lifecycle.LocalContinueTrainingProbe.SourceCheckpointSha256After);
        Assert.True(lifecycle.LocalContinueTrainingProbe.ContinuationBatchAppended);
        Assert.True(lifecycle.LocalContinueTrainingProbe.NoReplayBatchIdVerified);
        Assert.False(lifecycle.LocalContinueTrainingProbe.ContinueTrainingMetadataLocalProbeOnly);
        Assert.True(lifecycle.LocalContinueTrainingProbe.RealTrainedWeightsContinued);
        Assert.False(lifecycle.LocalContinueTrainingProbe.MeasurementGateRun);
        Assert.False(lifecycle.LocalContinueTrainingProbe.PromotionUploadAllowed);
        Assert.Equal(0, lifecycle.LocalContinueTrainingProbe.HeldOutRecordUsageCount);
        Assert.Equal(10, lifecycle.LocalContinueTrainingProbe.ContinueTrainRecordCount);
        Assert.DoesNotContain(
            lifecycle.LocalContinueTrainingProbe.ContinueTrainingRecordIds,
            id => heldOutRecordIds.Contains(id));
        Assert.True(lifecycle.LocalContinueTrainingProbe.ContinuedTrainingHistoryCount >
            lifecycle.LocalContinueTrainingProbe.StartingTrainingHistoryCount);
        Assert.Equal("not_run", lifecycle.MeasurementGateStatus);
        Assert.Equal("PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED", lifecycle.PromotionStatus);
        Assert.False(lifecycle.PromotionUploadAllowed);
        Assert.DoesNotContain("fresh_load", lifecycle.NotProvenFields);
        Assert.Contains("trained_feature_fresh_load_metrics", lifecycle.NotProvenFields);
        Assert.DoesNotContain("continue_training", lifecycle.NotProvenFields);
        Assert.DoesNotContain("continue_training_real_trained_weights", lifecycle.NotProvenFields);
        Assert.Contains("held_out_measurement_gate", lifecycle.NotProvenFields);
        Assert.DoesNotContain("real_trained_weights", lifecycle.NotProvenFields);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_LIFECYCLE_HOOK_CALLED_AFTER_COUNT_GATE_PASS",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_EXPORT_CHECKPOINT_BACKED_VERIFIED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_IMPORT_CHECKPOINT_BACKED_VERIFIED",
            lifecycle.EvidenceRefs);
        Assert.DoesNotContain("DESIGN_CONVERSION_EXPORT_METADATA_ONLY_VERIFIED",
            lifecycle.EvidenceRefs);
        Assert.DoesNotContain("DESIGN_CONVERSION_IMPORT_METADATA_ONLY_VERIFIED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_FRESH_LOAD_METADATA_CONTINUITY_VERIFIED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_AFTER_FRESH_LOAD_METADATA_PASS",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_HELD_OUT_ROWS_USED_0",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_SOURCE_CHECKPOINT_UNCHANGED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_APPENDED_ONE_BATCH",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_NO_REPLAY_BATCH_ID_VERIFIED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LOCAL_CONTINUE_TRAINING_REAL_TRAINED_WEIGHTS_CONTINUED",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_LEARNED_WEIGHTS_PRESENT_TRUE_AFTER_FRESH_LOAD",
            lifecycle.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE",
            lifecycle.EvidenceRefs);
        Assert.Empty(lifecycle.FailureReasons);
    }

    [Fact]
    public async Task RunAsync_RepositoryOwnedModelCheckpointLoadsParentBeforeMutationAndPublishesChildAfterMutation()
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-pipeline-repository-model-" +
            Guid.NewGuid().ToString("N"));
        var boundaries = new List<string>();
        var previousBoundary =
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            boundaries.Add;

        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var modelContext =
                CreateRepositoryOwnedModelCheckpointContext(repositoryRoot);
            var dpoContext =
                CreateRepositoryOwnedDpoTransactionContext(repositoryRoot);
            var dpoIdentity = OpenDpoIdentity(dpoContext);
            var coldStart = PrepareColdStartForPublication(
                modelContext,
                "first DesignConversion parent fixture");
            var parentMachine = coldStart.Start.Machine;
            var parentTraining =
                await DesignConversionLegalTeacherCapabilityTestFactory
                    .TrainMachineStrictAsync(
                        parentMachine,
                        CreateRepositoryModelBatch(
                            "parent-repository-design-conversion"));
            Assert.True(parentTraining.IsSuccess, parentTraining.ErrorMessage);

            var parentPublication =
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PublishNext(
                        new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                            coldStart.Target,
                            coldStart.Start,
                            parentMachine,
                            dpoIdentity,
                            DateTimeOffset.UnixEpoch,
                            ["test:parent-before-pipeline-run"],
                            ColdStartReason:
                            coldStart.Target.ColdStartReason));
            Assert.True(parentPublication.IsSuccess, parentPublication.ErrorMessage);
            var parent = parentPublication.Value!;
            var parentWeightsPath = Path.Combine(
                repositoryRoot,
                parent.Manifest.RepositoryRelativeWeightsPath);
            var parentWeightsBefore = await ReadFileSha256Async(parentWeightsPath);

            var result = await RunStrictPipelineAsync(
                new DesignConversionTrainingPipeline(),
                new DesignConversionMachine(),
                BuildSyntheticSourceDisjointRecords(2),
                new DesignConversionTrainingPipelineOptions(
                    "repo-owned-model-pipeline-continuation",
                    BatchSize: 1,
                    DatasetOptions: LocalDatasetOptions(
                        "repo-owned-model-pipeline-continuation",
                        MinimumTrainRecords: 1,
                        MinimumHeldOutRecords: 1),
                    RepositoryOwnedDpoTransactionContext: dpoContext,
                    RepositoryOwnedModelCheckpointContext: modelContext));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(result.Value!.MachineLifecycleHookAttempted);
            Assert.Contains("pipeline_repository_owned_model_checkpoint_load", boundaries);
            Assert.Contains("pipeline_train_batch:0", boundaries);
            Assert.True(
                boundaries.IndexOf("pipeline_repository_owned_model_checkpoint_load") <
                boundaries.IndexOf("pipeline_train_batch:0"));

            var evidence = Assert.IsType<
                DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence>(
                result.Value.LifecycleEvidence.RepositoryOwnedModelCheckpointEvidence);
            Assert.Equal("loaded_parent_checkpoint", evidence.StartStatus);
            Assert.Equal(parent.Manifest.RepositoryRelativeManifestPath,
                evidence.ParentManifestRelativePath);
            Assert.Equal(2, evidence.PublishedStep);
            Assert.Contains(
                "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_BOUND_TO_DPO_MANIFEST",
                evidence.EvidenceRefs);
            Assert.Equal(
                parentWeightsBefore,
                await ReadFileSha256Async(parentWeightsPath));

            var childAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    repositoryRoot,
                    modelContext.Request!.DomainSlug,
                    modelContext.Request.DesignPatternSlug,
                    modelContext.Request.ModelIdSlug,
                    modelContext.Request.ModelVersion,
                    evidence.PublishedStep);
            Assert.True(childAddress.IsSuccess, childAddress.ErrorMessage);
            var reopenedChild =
                RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
                    childAddress.Value!);
            Assert.True(reopenedChild.IsSuccess, reopenedChild.ErrorMessage);
            Assert.NotNull(reopenedChild.Value!.Manifest.ParentCheckpoint);
            Assert.Equal(
                parent.Manifest.RepositoryRelativeManifestPath,
                reopenedChild.Value.Manifest.ParentCheckpoint!
                    .RepositoryRelativeManifestPath);
            Assert.Equal(
                dpoIdentity,
                reopenedChild.Value.Manifest.DpoDatasetManifest);

            var loadedChild =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .LoadLatestCompatibleAsync(modelContext.Request!);
            Assert.True(loadedChild.IsSuccess, loadedChild.ErrorMessage);
            var childCheckpoint =
                loadedChild.Value!.Machine.ExportCheckpoint().Value!;
            var childHistory = childCheckpoint.TrainingHistory
                .Select(entry => entry.BatchId)
                .ToArray();
            Assert.Contains("parent-repository-design-conversion", childHistory);
            Assert.Contains(
                childHistory,
                batchId => batchId.StartsWith(
                    "repo-owned-model-pipeline-continuation",
                    StringComparison.Ordinal));
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task RunAsync_RepositoryOwnedModelCheckpointDoesNotPublishChildWhenExternalCompletionFailsAfterMutation()
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-pipeline-repository-model-external-fail-" +
            Guid.NewGuid().ToString("N"));
        var boundaries = new List<string>();
        var previousBoundary =
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            boundary =>
            {
                boundaries.Add(boundary);
                if (boundary ==
                    "pipeline_external_after_fresh_load_before_complete")
                {
                    throw new IOException("injected-before-external-complete");
                }
            };

        try
        {
            CreateRepositorySentinels(repositoryRoot);
            using var fixture =
                Tests.DesignConversionDualTargetTeacherAdmissionTests
                    .CreateFixture();
            var modelContext =
                CreateRepositoryOwnedModelCheckpointContext(repositoryRoot);
            var dpoContext =
                CreateRepositoryOwnedDpoTransactionContext(repositoryRoot);
            var parent = PublishParentCheckpoint(
                modelContext,
                dpoContext,
                "external-fail-parent");
            var parentWeightsPath = Path.Combine(
                repositoryRoot,
                parent.Manifest.RepositoryRelativeWeightsPath);
            var parentWeightsBefore = await ReadFileSha256Async(parentWeightsPath);

            var options = fixture.Options with
            {
                RunId = "repo-owned-model-external-fail",
                RepositoryOwnedDpoTransactionContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            };
            var pipeline = new DesignConversionTrainingPipeline();
            var preparedPipeline = pipeline.PrepareExecution(
                fixture.Records,
                options);
            Assert.True(preparedPipeline.IsSuccess, preparedPipeline.ErrorMessage);
            var prepared =
                new DesignConversionPreparedTrainingRecordExecution(
                    new DesignConversionDpoBuildResult(fixture.Records, [], []),
                    options,
                    fixture.Capability,
                    preparedPipeline.Value!);

            var externalStore = new FileDpoPairStore(Path.Combine(
                repositoryRoot,
                "external-consumption-pairs"));
            var transaction = StoreAndPrepareExternalConsumption(
                externalStore,
                prepared.Build.AllRecords,
                prepared.Pipeline.Batches,
                prepared.Pipeline.FinalOptimizer);

            var result = await new DesignConversionTrainingRunner(
                    pipeline: pipeline)
                .RunPreparedDualTargetTeacherExternalConsumptionAsync(
                    fixture.Authority,
                    new DesignConversionMachine(),
                    prepared,
                    transaction);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_EXTERNAL_BOUNDARY_FAILED",
                result.ErrorCode);
            Assert.Contains("pipeline_train_batch:0", boundaries);
            Assert.Contains(
                "pipeline_external_after_fresh_load_before_complete",
                boundaries);
            Assert.Equal(
                parentWeightsBefore,
                await ReadFileSha256Async(parentWeightsPath));

            var childAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    repositoryRoot,
                    modelContext.Request!.DomainSlug,
                    modelContext.Request.DesignPatternSlug,
                    modelContext.Request.ModelIdSlug,
                    modelContext.Request.ModelVersion,
                    step: 2);
            Assert.True(childAddress.IsSuccess, childAddress.ErrorMessage);
            Assert.False(File.Exists(childAddress.Value!.ManifestPath));
            Assert.False(File.Exists(childAddress.Value.WeightsPath));

            var latest =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .LoadLatestCompatibleAsync(modelContext.Request);
            Assert.True(latest.IsSuccess, latest.ErrorMessage);
            Assert.Equal(1, latest.Value!.Manifest.Step);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("corrupt")]
    [InlineData("incompatible")]
    public async Task RunAsync_RepositoryOwnedModelCheckpointFailsClosedBeforeMutationWithoutUsableParent(
        string parentState)
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-pipeline-repository-model-fail-" +
            parentState +
            "-" +
            Guid.NewGuid().ToString("N"));
        var boundaries = new List<string>();
        var previousBoundary =
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            boundaries.Add;

        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var modelContext =
                CreateRepositoryOwnedModelCheckpointContext(repositoryRoot);
            var dpoContext =
                CreateRepositoryOwnedDpoTransactionContext(repositoryRoot);

            if (parentState == "corrupt")
            {
                var published = PublishParentCheckpoint(
                    modelContext,
                    dpoContext,
                    "corrupt-parent");
                var weightsPath = Path.Combine(
                    repositoryRoot,
                    published.Manifest.RepositoryRelativeWeightsPath);
                await File.AppendAllTextAsync(
                    weightsPath,
                    "corrupt after seal");
            }
            else if (parentState == "incompatible")
            {
                var incompatibleContext =
                    CreateRepositoryOwnedModelCheckpointContext(
                        repositoryRoot,
                        compatibilityLabel: "incompatible-parent");
                _ = PublishParentCheckpoint(
                    incompatibleContext,
                    dpoContext,
                    "incompatible-parent");
            }

            var result = await RunStrictPipelineAsync(
                new DesignConversionTrainingPipeline(),
                new DesignConversionMachine(),
                BuildSyntheticSourceDisjointRecords(2),
                new DesignConversionTrainingPipelineOptions(
                    "repo-owned-model-pipeline-fail-" + parentState,
                    BatchSize: 1,
                    DatasetOptions: LocalDatasetOptions(
                        "repo-owned-model-pipeline-fail-" + parentState,
                        MinimumTrainRecords: 1,
                        MinimumHeldOutRecords: 1),
                    RepositoryOwnedDpoTransactionContext: dpoContext,
                    RepositoryOwnedModelCheckpointContext: modelContext));

            Assert.False(result.IsSuccess);
            Assert.Contains("pipeline_repository_owned_model_checkpoint_load", boundaries);
            Assert.DoesNotContain("pipeline_train_batch:0", boundaries);
            Assert.Equal(
                parentState switch
                {
                    "missing" =>
                        DesignConversionRepositoryOwnedModelCheckpointLifecycle
                            .ColdStartNotAuthorizedErrorCode,
                    "corrupt" =>
                        RepositoryOwnedModelCheckpointLoader
                            .CorruptCandidateErrorCode,
                    _ =>
                        RepositoryOwnedModelCheckpointLoader
                            .IncompatibleCandidateErrorCode
                },
                result.ErrorCode);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task RunAsync_RepositoryOwnedModelCheckpointRefusesCorruptDpoPairsBeforeMutation()
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-pipeline-repository-model-dpo-corrupt-" +
            Guid.NewGuid().ToString("N"));
        var boundaries = new List<string>();
        var previousBoundary =
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
            boundaries.Add;

        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var modelContext =
                CreateRepositoryOwnedModelCheckpointContext(repositoryRoot);
            var dpoContext =
                CreateRepositoryOwnedDpoTransactionContext(repositoryRoot);
            _ = PublishParentCheckpoint(
                modelContext,
                dpoContext,
                "dpo-corrupt-parent");

            CorruptFirstDpoPairFile(repositoryRoot, dpoContext);

            var result = await RunStrictPipelineAsync(
                new DesignConversionTrainingPipeline(),
                new DesignConversionMachine(),
                BuildSyntheticSourceDisjointRecords(2),
                new DesignConversionTrainingPipelineOptions(
                    "repo-owned-model-pipeline-dpo-corrupt",
                    BatchSize: 1,
                    DatasetOptions: LocalDatasetOptions(
                        "repo-owned-model-pipeline-dpo-corrupt",
                        MinimumTrainRecords: 1,
                        MinimumHeldOutRecords: 1),
                    RepositoryOwnedDpoTransactionContext: dpoContext,
                    RepositoryOwnedModelCheckpointContext: modelContext));

            Assert.False(result.IsSuccess);
            Assert.Contains(
                "pipeline_repository_owned_model_checkpoint_load",
                boundaries);
            Assert.DoesNotContain("pipeline_train_batch:0", boundaries);

            var childAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    repositoryRoot,
                    modelContext.Request!.DomainSlug,
                    modelContext.Request.DesignPatternSlug,
                    modelContext.Request.ModelIdSlug,
                    modelContext.Request.ModelVersion,
                    step: 2);
            Assert.True(childAddress.IsSuccess, childAddress.ErrorMessage);
            Assert.False(File.Exists(childAddress.Value!.ManifestPath));
            Assert.False(File.Exists(childAddress.Value.WeightsPath));
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting =
                previousBoundary;
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task RunAsync_SourceDisjointHundredAcceptedRecordsPersistsRealLearnedWeightsWithoutRawPayloads()
    {
        var records = BuildSyntheticSourceDisjointRecords(100);
        var machine = new DesignConversionMachine();
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await RunStrictPipelineAsync(
            pipeline,
            machine,
            records,
            new DesignConversionTrainingPipelineOptions(
                "pass114-real-weight-checkpoint",
                BatchSize: 25,
                DatasetOptions: LocalDatasetOptions()));
        var checkpoint = machine.ExportCheckpoint();
        var serializedCheckpoint = System.Text.Json.JsonSerializer.Serialize(
            checkpoint.Value,
            DesignConversionCheckpointJsonOptions.Standard);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint.Value));
        Assert.True(DesignConversionMachine.CountRealLearnedWeightArtifacts(checkpoint.Value) > 0);
        Assert.True(DesignConversionMachine.CountRealLearnedWeightObservations(checkpoint.Value) > 0);
        Assert.True(result.Value!.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
        Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);

        var learnedWeights = new[]
            {
                checkpoint.Value!.CriticUnit,
                checkpoint.Value.ImproverUnit
            }
            .SelectMany(unit => unit.LearnedWeights ?? [])
            .ToArray();
        Assert.NotEmpty(learnedWeights);
        Assert.All(learnedWeights, weight =>
        {
            Assert.True(weight.ObservationCount > 0);
            Assert.True(double.IsFinite(weight.Weight));
            Assert.Contains(DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence, weight.EvidenceRefs);
            Assert.Contains("DESIGN_CONVERSION_LEARNED_WEIGHT_NO_RAW_TREE_HTML_SCREENSHOT", weight.EvidenceRefs);
            Assert.All(weight.SourceIdentityHashes, hash =>
            {
                Assert.Equal(64, hash.Length);
                Assert.All(hash, ch => Assert.True(Uri.IsHexDigit(ch)));
            });
        });

        foreach (var record in result.Value.Batches.SelectMany(batch => batch.Records))
        {
            Assert.DoesNotContain(record.RecordId, serializedCheckpoint, StringComparison.Ordinal);
            Assert.DoesNotContain(record.Provenance.SourceManifestRef, serializedCheckpoint, StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(record.SourceStructurePayload))
                Assert.DoesNotContain(record.SourceStructurePayload, serializedCheckpoint, StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(record.Chosen.Html))
                Assert.DoesNotContain(record.Chosen.Html, serializedCheckpoint, StringComparison.Ordinal);
            if (!string.IsNullOrWhiteSpace(record.Rejected.Html))
                Assert.DoesNotContain(record.Rejected.Html, serializedCheckpoint, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task LocalContinueTrainingProbe_AppendsRealWeightsWithoutMutatingSourceCheckpoint()
    {
        var records = BuildSyntheticSourceDisjointRecords(4);
        var batches = new DesignConversionTrainingPipeline()
            .BuildBatches(
                records,
                new DesignConversionTrainingPipelineOptions(
                    "pass114-direct-continue",
                    BatchSize: 1,
                    DatasetOptions: LocalDatasetOptions(
                        "pass114-direct-continue",
                        MinimumTrainRecords: 1,
                        MinimumHeldOutRecords: 1)));
        Assert.True(batches.IsSuccess, batches.ErrorMessage);
        Assert.True(batches.Value!.Count >= 2);
        var sourceBatch = batches.Value[0];
        var continuationBatch = batches.Value[1];
        var machine = new DesignConversionMachine();
        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
                    BatchId: sourceBatch.BatchId,
                    DomainTag: DesignConversionMachine.DomainTag,
                    TrainingItems: sourceBatch.TrainingItems,
                    SourceIds: sourceBatch.SourceIds));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"pass114-real-weights-{Guid.NewGuid():N}");
        var sourcePath = Path.Combine(root, "source-checkpoint.json");
        var continuedPath = Path.Combine(root, "continued-checkpoint.json");

        try
        {
            var fresh = await DesignConversionLocalFreshLoadProbe.VerifySavedCheckpointAsync(machine, sourcePath);
            Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
            Assert.True(fresh.Value!.CheckpointContainsRealTrainedWeights);
            var sourceShaBefore = await ReadFileSha256Async(sourcePath);
            var sourceLoadBefore = await DesignConversionMachine.LoadCheckpointAsync(sourcePath);
            Assert.True(sourceLoadBefore.IsSuccess, sourceLoadBefore.ErrorMessage);
            var sourceObservationCount = DesignConversionMachine.CountRealLearnedWeightObservations(
                sourceLoadBefore.Value!.Checkpoint);

            var continued = await DesignConversionLegalTeacherCapabilityTestFactory.ContinueTrainingStrictAsync(
                fresh.Value,
                continuationBatch,
                heldOutRecordIdentityHashes: [],
                continuedPath,
                datasetCountGatePassed: true);

            Assert.True(continued.IsSuccess, continued.ErrorMessage);
            Assert.True(continued.Value!.SourceCheckpointUnchanged);
            Assert.Equal(sourceShaBefore, continued.Value.SourceCheckpointSha256Before);
            Assert.Equal(sourceShaBefore, continued.Value.SourceCheckpointSha256After);
            Assert.True(continued.Value.ContinuationBatchAppended);
            Assert.False(continued.Value.ContinueTrainingMetadataLocalProbeOnly);
            Assert.True(continued.Value.RealTrainedWeightsContinued);
            Assert.Equal(sourceShaBefore, await ReadFileSha256Async(sourcePath));

            var sourceLoadAfter = await DesignConversionMachine.LoadCheckpointAsync(sourcePath);
            var continuedLoad = await DesignConversionMachine.LoadCheckpointAsync(continuedPath);
            Assert.True(sourceLoadAfter.IsSuccess, sourceLoadAfter.ErrorMessage);
            Assert.True(continuedLoad.IsSuccess, continuedLoad.ErrorMessage);
            Assert.Equal(
                sourceObservationCount,
                DesignConversionMachine.CountRealLearnedWeightObservations(sourceLoadAfter.Value!.Checkpoint));
            Assert.True(
                DesignConversionMachine.CountRealLearnedWeightObservations(continuedLoad.Value!.Checkpoint) >
                sourceObservationCount);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_LocalFreshLoadProbeUsesTempPathAndDoesNotWriteRepoArtifacts()
    {
        var beforeArtifacts = SnapshotRepoArtifacts();
        var records = BuildSyntheticSourceDisjointRecords(100);
        var pipeline = new DesignConversionTrainingPipeline();

        var result = await RunStrictPipelineAsync(
            pipeline,
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-local-fresh-load-no-artifacts",
                BatchSize: 25,
                DatasetOptions: LocalDatasetOptions()));

        var afterArtifacts = SnapshotRepoArtifacts();
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(beforeArtifacts, afterArtifacts);
        var probe = Assert.IsType<DesignConversionLocalFreshLoadProbeReport>(
            result.Value!.LifecycleEvidence.LocalFreshLoadProbe);
        Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), probe.CheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.Combine("llm_mvp_core", "artifacts"), probe.CheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(probe.CheckpointRef));
        var continueProbe = Assert.IsType<DesignConversionLocalContinueTrainingProbeReport>(
            result.Value.LifecycleEvidence.LocalContinueTrainingProbe);
        Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), continueProbe.ContinuedCheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.Combine("llm_mvp_core", "artifacts"), continueProbe.ContinuedCheckpointRef, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(continueProbe.ContinuedCheckpointRef));
        Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, result.Value.LifecycleEvidence.FreshLoadStatus);
        Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, result.Value.LifecycleEvidence.ContinueTrainingStatus);
        Assert.Equal("not_run", result.Value.LifecycleEvidence.MeasurementGateStatus);
        Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);
    }

    [Fact]
    public async Task RunAsync_RefusesBeforeTrainingWhenTrainAndHeldOutIdentitiesOverlap()
    {
        var records = BuildRecords(
        [
            WithSource(DesignConversionDpoGeneratorTests.CreateExample(
                "overlap-record",
                DesignConversionDpoUnits.CriticUnit,
                usefulness: 0.95), "overlap-source-a"),
            WithSource(DesignConversionDpoGeneratorTests.CreateExample(
                "overlap-record",
                DesignConversionDpoUnits.CriticUnit,
                usefulness: 0.85), "overlap-source-b")
        ]);
        var machine = new DesignConversionMachine();
        var versionBefore = ExportMachineVersion(machine);
        var trainBatchCallCount = 0;
        var pipeline = new DesignConversionTrainingPipeline(
            trainBatchObserver: _ =>
            {
                trainBatchCallCount++;
            });

        var result = await RunStrictPipelineAsync(
            pipeline,
            machine,
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-overlap-refuses-before-training",
                BatchSize: 1,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    DatasetId: "s3-overlap-refuses-before-training",
                    MinimumTrainRecords: 1,
                    MinimumHeldOutRecords: 1,
                    Preflight: LocalPreflight())));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_DPO_TRAIN_HELDOUT_IDENTITY_OVERLAP", result.ErrorCode);
        Assert.Equal(0, trainBatchCallCount);
        Assert.Equal(versionBefore, ExportMachineVersion(machine));
    }

    [Fact]
    public async Task AuthorityFirstPipeline_SnapshotsRecordsBeforeTrainSplitMapping()
    {
        var originalRecords = BuildSyntheticSourceDisjointRecords(4);
        var mutatedRecords = originalRecords
            .Select(record => record with { RecordId = $"mapping-mismatch-{record.RecordId}" })
            .ToArray();
        var records = new MutatingAfterDatasetBuildRecords(originalRecords, mutatedRecords);
        var machine = new DesignConversionMachine();
        var versionBefore = ExportMachineVersion(machine);
        var trainBatchCallCount = 0;
        var pipeline = new DesignConversionTrainingPipeline(
            trainBatchObserver: _ =>
            {
                trainBatchCallCount++;
            });

        var result = await RunStrictPipelineAsync(
            pipeline,
            machine,
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-train-split-mapping-failed",
                BatchSize: 2,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    DatasetId: "s3-train-split-mapping-failed",
                    MinimumTrainRecords: 1,
                    MinimumHeldOutRecords: 1,
                    Preflight: LocalPreflight())));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(trainBatchCallCount > 0);
        Assert.NotEqual(versionBefore, ExportMachineVersion(machine));
        Assert.DoesNotContain(
            result.Value!.Batches.SelectMany(batch => batch.Records),
            record => record.RecordId.StartsWith(
                "mapping-mismatch-",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task Runner_StrictRoute_ReturnsDatasetManifestAndCountGateDecisionAtLocalBoundary()
    {
        var examples = BuildSyntheticSourceDisjointExamples(3);
        var records = BuildRecords(examples);
        var runner = new DesignConversionTrainingRunner();

        var result = await DesignConversionLegalTeacherCapabilityTestFactory.RunStrictRunnerAsync(
            runner,
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-runner-count-gate-fail",
                BatchSize: 2,
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingBoundaryDecisionKind.CountGateFailed, result.Value!.BoundaryDecision.Kind);
        Assert.False(result.Value.DatasetCountGate.Passed);
        Assert.Equal(result.Value.DatasetManifest, result.Value.PipelineResult.DatasetManifest);
        Assert.Equal(result.Value.DatasetCountGate, result.Value.PipelineResult.DatasetCountGate);
        Assert.False(result.Value.BoundaryDecision.RealTrainingRunClaimed);
        Assert.False(result.Value.BoundaryDecision.CheckpointClaimed);
        Assert.False(result.Value.BoundaryDecision.PromotionClaimed);
        Assert.False(result.Value.PipelineResult.MachineLifecycleHookAttempted);
        Assert.False(result.Value.PipelineResult.LifecycleEvidence.MachineLifecycleHookCalled);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PRODUCE_REAL_LEARNED_WEIGHTS",
            result.Value.Warnings);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PERFORM_EXTERNAL_MODEL_SERVER_TRAINING",
            result.Value.Warnings);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_CREATE_CHECKPOINT",
            result.Value.Warnings);
        Assert.Contains("DESIGN_CONVERSION_TRAINING_RUNNER_DID_NOT_PROMOTE_MODEL",
            result.Value.Warnings);
    }

    private static Task<OperationResult<DesignConversionTrainingPipelineResult>>
        RunStrictPipelineAsync(
            DesignConversionTrainingPipeline pipeline,
            DesignConversionMachine machine,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            DesignConversionTrainingPipelineOptions options,
            CancellationToken ct = default)
        => DesignConversionLegalTeacherCapabilityTestFactory.RunStrictPipelineAsync(
            pipeline,
            machine,
            records,
            options,
            ct);

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildRecords(
        IReadOnlyList<DesignConversionStoredDpoExample> examples)
    {
        var result = new DesignConversionDpoGenerator().BuildTrainReadyRecords(examples);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        return result.Value.AcceptedRecords;
    }

    private static DesignConversionRepositoryOwnedModelCheckpointContext
        CreateRepositoryOwnedModelCheckpointContext(
            string repositoryRoot,
            string compatibilityLabel = "pipeline-compatible")
        => new(
            Enabled: true,
            Request: new DesignConversionRepositoryOwnedModelCheckpointRequest(
                repositoryRoot,
                DomainSlug: "figma-html-css",
                DesignPatternSlug: "design-to-code",
                ModelIdSlug: "qwen3-5-9b",
                ModelVersion: "v0001",
                RuntimeCompatibility:
                    CreateRepositoryModelCompatibility(compatibilityLabel),
                ProvenanceRefs:
                [
                    "test:design-conversion-pipeline-repository-model"
                ]));

    private static DesignConversionRepositoryOwnedDpoTransactionContext
        CreateRepositoryOwnedDpoTransactionContext(string repositoryRoot)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                "figma-html-css",
                "design-to-code",
                "v0001");
        Assert.True(address.IsSuccess, address.ErrorMessage);

        var requestIdentity =
            CreateRepositoryOwnedDpoRequestIdentity(
                "design-conversion-pipeline-model-checkpoint");
        var pair =
            CreateRepositoryOwnedDpoPair(
                requestIdentity,
                "design-conversion-pipeline-model-checkpoint");
        var manifest = RepositoryOwnedDpoDatasetManifestSealer
            .PersistAndSeal(
                address.Value!,
                requestIdentity,
                [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);

        return new DesignConversionRepositoryOwnedDpoTransactionContext(
            Enabled: true,
            address.Value!,
            requestIdentity);
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity OpenDpoIdentity(
        DesignConversionRepositoryOwnedDpoTransactionContext context)
    {
        var opened =
            RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                context.Address!,
                context.RequestIdentity!);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        return new RepositoryOwnedDpoDatasetManifestIdentity(
            context.Address!.ManifestRelativePath,
            opened.Value!.Manifest.ManifestSha256);
    }

    private static void CorruptFirstDpoPairFile(
        string repositoryRoot,
        DesignConversionRepositoryOwnedDpoTransactionContext context)
    {
        var opened =
            RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                context.Address!,
                context.RequestIdentity!);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);

        var pair = Assert.Single(opened.Value!.Manifest.OrderedPairs);
        var pairPath = Path.Combine(
            repositoryRoot,
            pair.RepositoryRelativePairFilePath);
        File.AppendAllText(
            pairPath,
            "corrupt pair bytes after seal");
    }

    private static DesignConversionRepositoryOwnedModelCheckpointPublicationReport
        PublishParentCheckpoint(
            DesignConversionRepositoryOwnedModelCheckpointContext modelContext,
            DesignConversionRepositoryOwnedDpoTransactionContext dpoContext,
            string batchId)
    {
        var published =
            PrepareColdStartForPublication(
                modelContext,
                "first DesignConversion parent fixture");
        var machine = published.Start.Machine;
        var training = DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                CreateRepositoryModelBatch(batchId))
            .GetAwaiter()
            .GetResult();
        Assert.True(training.IsSuccess, training.ErrorMessage);

        var publication =
            DesignConversionRepositoryOwnedModelCheckpointLifecycle.PublishNext(
                new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                    published.Target,
                    published.Start,
                    machine,
                    OpenDpoIdentity(dpoContext),
                    DateTimeOffset.UnixEpoch,
                    ["test:" + batchId],
                    ColdStartReason:
                    published.Target.ColdStartReason));
        Assert.True(publication.IsSuccess, publication.ErrorMessage);
        return publication.Value!;
    }

    private static (
        DesignConversionRepositoryOwnedModelCheckpointRequest Target,
        DesignConversionRepositoryOwnedModelCheckpointStart Start)
        PrepareColdStartForPublication(
            DesignConversionRepositoryOwnedModelCheckpointContext modelContext,
            string reason)
    {
        var target = modelContext.Request! with
        {
            AllowColdStart = true,
            ColdStartReason = reason
        };
        var prepared =
            DesignConversionRepositoryOwnedModelCheckpointLifecycle
                .PrepareStartAsync(
                    new DesignConversionRepositoryOwnedModelCheckpointContext(
                        true,
                        target))
                .GetAwaiter()
                .GetResult();
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        Assert.NotNull(prepared.Value);
        Assert.Null(prepared.Value!.ParentManifest);
        Assert.Equal(reason, prepared.Value.ColdStartReason);
        return (target, prepared.Value);
    }

    private static FileDpoPairStore.ExternalConsumptionTransactionHandle
        StoreAndPrepareExternalConsumption(
            FileDpoPairStore store,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            IReadOnlyList<DesignConversionTrainingBatch> preparedBatches,
            DesignConversionFinalOptimizerBatch<LearningTrainingItem>?
                finalOptimizer)
    {
        Assert.True(records.Count >= 2);
        var projected = DesignConversionDpoTrainingContentAuthority
            .ProjectDualTargetTeacherAggregate(
                records,
                preparedBatches,
                finalOptimizer);
        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        var externalGroup = projected.Value!.ProjectExternalAtomicGroup();
        Assert.True(externalGroup.IsSuccess, externalGroup.ErrorMessage);
        var receipt = store.AddAtomicGroup(externalGroup.Value!);
        Assert.True(receipt.IsSuccess, receipt.ErrorMessage);
        var coordinator = Assert.IsAssignableFrom<
            IDpoPairExternalConsumptionTransactionCoordinator>(store);
        var transaction = coordinator.PrepareExternalConsumptionTransaction(
            receipt.Value!,
            DesignConversionMachine.DomainTag);
        Assert.True(transaction.IsSuccess, transaction.ErrorMessage);
        return Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            transaction.Value);
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain>
        CreateRepositoryModelBatch(string batchId)
        => new(
            BatchId: batchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: batchId + "-item-001",
                    Prompt: "figma frame for repository-owned " + batchId,
                    ChosenResponse: SerializeRepositoryModelSide(
                        "<main class=\"screen\"><section>" +
                        batchId +
                        " chosen</section></main>",
                        ".screen{display:grid;gap:16px;}"),
                    RejectedResponse: SerializeRepositoryModelSide(
                        "<main class=\"screen\"><section>" +
                        batchId +
                        " rejected</section></main>",
                        ".screen{display:block;margin:0;}"))
            ],
            SourceIds: ["source:" + batchId]);

    private static RepositoryOwnedModelRuntimeCompatibility
        CreateRepositoryModelCompatibility(string label)
        => new(
            ConfigSha256: HexForTest('a'),
            TopologySha256: HexForTest('b'),
            TokenizerSha256: HexForTest('c'),
            TrainerSha256: HexForTest('d'),
            SchemaSha256: HexForTest('e'),
            CompatibilityKey: "design-conversion-" + label);

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryOwnedDpoRequestIdentity(string scope)
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [HashIdentity("paid DesignConversion reference " + scope)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen-test:9b",
                    HexForTest('c'),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    HexForTest('d')),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "design-conversion-pipeline-model-checkpoint",
                    "v1",
                    HashIdentity("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "design-conversion-pipeline-model-checkpoint-tests",
                    "v1",
                    HashIdentity("creator policy " + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static DpoPair CreateRepositoryOwnedDpoPair(
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        string scope)
    {
        var passport = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId,
            identity.OpenTeacher.ModelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            "repository-owned-pipeline-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

        var bound = DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "convert Figma tree to HTML and CSS for " + scope,
            SerializeRepositoryModelSide(
                "<main class=\"card\"><h1>open teacher</h1></main>",
                ".card{display:grid;gap:1rem;}"),
            SerializeRepositoryModelSide(
                "<main><h1>local candidate</h1></main>",
                "main{display:block;}"),
            "figma-html-css",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs: ["sha256:" + HashIdentity("governance " + scope)]),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel)
        });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private static string SerializeRepositoryModelSide(string html, string css)
        => JsonSerializer.Serialize(new { html, css });

    private static string HexForTest(char value) => new(value, 64);

    private static void CreateRepositorySentinels(string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "DesignConversion repository-owned model fixture");
        Directory.CreateDirectory(
            Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Per-test repository roots are disposable evidence sandboxes; a
            // cleanup race must not hide the actual assertion result above.
        }
    }

    private static DesignConversionDpoTrainingRecord WithProvenanceMetadata(
        DesignConversionDpoTrainingRecord record,
        params (string Key, string Value)[] values)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal);
        foreach (var (key, value) in values)
            metadata[key] = value;

        return record with
        {
            Provenance = record.Provenance with
            {
                Metadata = metadata
            }
        };
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildSyntheticSourceDisjointRecords(int count)
    {
        var result = new DesignConversionDpoGenerator().BuildTrainReadyRecords(BuildSyntheticSourceDisjointExamples(count));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        Assert.Equal(count, result.Value.AcceptedRecords.Count);
        return result.Value.AcceptedRecords;
    }

    private static IReadOnlyList<DesignConversionStoredDpoExample> BuildSyntheticSourceDisjointExamples(int count)
        => Enumerable.Range(0, count)
            .Select(index =>
            {
                var unitId = index % 2 == 0
                    ? DesignConversionDpoUnits.CriticUnit
                    : DesignConversionDpoUnits.ImproverUnit;
                var example = DesignConversionDpoGeneratorTests.CreateExample(
                    $"s3-source-disjoint-{index:000}",
                    unitId,
                    usefulness: 0.8 + (index % 10 * 0.01));
                return WithSource(example, $"s3-source-{index:000}");
            })
            .ToArray();

    private static DesignConversionStoredDpoExample WithSource(
        DesignConversionStoredDpoExample example,
        string sourceId)
        => example with
        {
            Provenance = example.Provenance with
            {
                SourceRequestId = $"request:{sourceId}",
                SourceManifestRef = $"manifest://dcm-source/{sourceId}",
                Metadata = new Dictionary<string, string>
                {
                    ["source_design_id"] = sourceId
                }
            }
        };

    private static DesignConversionDpoDatasetOptions LocalDatasetOptions(
        string DatasetId = "dcm-dpo-dataset",
        int MinimumTrainRecords = 50,
        int MinimumHeldOutRecords = 50)
        => new(
            DatasetId: DatasetId,
            MinimumTrainRecords: MinimumTrainRecords,
            MinimumHeldOutRecords: MinimumHeldOutRecords,
            Preflight: LocalPreflight());

    private static DesignConversionDpoDatasetPreflight LocalPreflight()
        => DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly();

    private static string ExportMachineVersion(DesignConversionMachine machine)
    {
        var checkpoint = machine.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        return checkpoint.Value!.Version;
    }

    private static void AssertLeaseDisposed(
        ServerOwnedLegalTrainingTeacherAdmissionLease? lease)
    {
        Assert.NotNull(lease);
        var permit = lease!.AcquireMutationPermit();
        Assert.False(permit.IsSuccess);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.AdmissionRevokedErrorCode,
            permit.ErrorCode);
    }

    private static string ToRecordIdentityHash(DesignConversionDpoTrainingRecord record)
        => HashIdentity($"record|{record.RecordId}|{record.UnitId}");

    private static string HashIdentity(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static async Task<string> ReadFileSha256Async(string path)
        => Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).ToLowerInvariant();

    private static IReadOnlyList<string> SnapshotRepoArtifacts()
    {
        var root = FindRepoRoot();
        var artifactsRoot = Path.Combine(root, "artifacts");
        if (!Directory.Exists(artifactsRoot))
            return [];

        return Directory.EnumerateFiles(artifactsRoot, "*", SearchOption.AllDirectories)
            .Select(path =>
            {
                var info = new FileInfo(path);
                return $"{Path.GetRelativePath(root, path)}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
            })
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private sealed class MutatingAfterDatasetBuildRecords : IReadOnlyList<DesignConversionDpoTrainingRecord>
    {
        private readonly IReadOnlyList<DesignConversionDpoTrainingRecord> _datasetBuildRecords;
        private readonly IReadOnlyList<DesignConversionDpoTrainingRecord> _mappingRecords;
        private int _enumerationCount;

        public MutatingAfterDatasetBuildRecords(
            IReadOnlyList<DesignConversionDpoTrainingRecord> datasetBuildRecords,
            IReadOnlyList<DesignConversionDpoTrainingRecord> mappingRecords)
        {
            _datasetBuildRecords = datasetBuildRecords;
            _mappingRecords = mappingRecords;
        }

        public int Count => _datasetBuildRecords.Count;

        public DesignConversionDpoTrainingRecord this[int index] => _datasetBuildRecords[index];

        public IEnumerator<DesignConversionDpoTrainingRecord> GetEnumerator()
        {
            _enumerationCount++;
            return (_enumerationCount <= 3 ? _datasetBuildRecords : _mappingRecords).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed class ThrowingReadOnlyList<T> : IReadOnlyList<T>
    {
        public int Count => throw new InvalidOperationException("legacy input touched");
        public T this[int index] => throw new InvalidOperationException("legacy input touched");
        public IEnumerator<T> GetEnumerator()
            => throw new InvalidOperationException("legacy input touched");
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed class TestPermit : IDisposable
    {
        public void Dispose()
        {
        }
    }


    [Fact]
    public void PrepareExecution_RefusesBeforeTrainingWhenTrainSplitCannotMapBackToRecords()
    {
        var originalRecords = BuildSyntheticSourceDisjointRecords(4);
        var mutatedRecords = originalRecords
            .Select(record => record with { RecordId = $"mapping-mismatch-{record.RecordId}" })
            .ToArray();
        var records = new MutatingAfterDatasetBuildRecords(originalRecords, mutatedRecords);
        var machine = new DesignConversionMachine();
        var versionBefore = ExportMachineVersion(machine);
        var trainBatchCallCount = 0;
        var pipeline = new DesignConversionTrainingPipeline(
            trainBatchObserver: _ =>
            {
                trainBatchCallCount++;
            });

        var result = pipeline.PrepareExecution(
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-train-split-mapping-failed",
                BatchSize: 2,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    DatasetId: "s3-train-split-mapping-failed",
                    MinimumTrainRecords: 1,
                    MinimumHeldOutRecords: 1,
                    Preflight: LocalPreflight())));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_TRAIN_SPLIT_MAPPING_FAILED", result.ErrorCode);
        Assert.Equal(0, trainBatchCallCount);
        Assert.Equal(versionBefore, ExportMachineVersion(machine));
    }


    [Fact]
    public async Task RunValidatedAsync_MissingAdmissionMeasurementBlocksAllMutationAndContinuationProbes()
    {
        var records = BuildSyntheticSourceDisjointRecords(100);
        var mutationBoundaries = new List<string>();
        var optimizerSinks = new List<string>();
        var previousBoundary = DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        var previousOptimizer = DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting = mutationBoundaries.Add;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            observation => optimizerSinks.Add(observation.Stage);
        try
        {
            var result = await new DesignConversionTrainingPipeline().RunValidatedAsync(
                new DesignConversionMachine(),
                records,
                new DesignConversionTrainingPipelineOptions(
                    "missing-admission-measurement",
                    BatchSize: 25,
                    DatasetOptions: LocalDatasetOptions()),
                custodyCapability: null,
                trainingBinding: null,
                ct: CancellationToken.None);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.False(result.Value!.MachineLifecycleHookAttempted);
            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.Value.LifecycleEvidence.MeasurementGateStatus);
            Assert.Contains("CALLER_ADMISSION_MEASUREMENT_BLOCKED_BEFORE_MACHINE_MUTATION", result.Value.LifecycleEvidence.EvidenceRefs);
            Assert.Empty(mutationBoundaries);
            Assert.Empty(optimizerSinks);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting = previousBoundary;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting = previousOptimizer;
        }
    }


    [Theory]
    [InlineData("below_score")]
    [InlineData("runtime")]
    [InlineData("cost")]
    [InlineData("ram")]
    [InlineData("leakage")]
    [InlineData("hidden_determinism")]
    [InlineData("ablation")]
    [InlineData("source_disjoint")]
    public async Task RunValidatedAsync_FailingPresentAdmissionMeasurementBlocksEveryMutationBoundary(
        string failure)
    {
        var mutationBoundaries = new List<string>();
        var optimizerSinks = new List<string>();
        var previousBoundary = DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting;
        var previousOptimizer = DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting = mutationBoundaries.Add;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            observation => optimizerSinks.Add(observation.Stage);
        try
        {
            var result = await new DesignConversionTrainingPipeline().RunValidatedAsync(
                new DesignConversionMachine(),
                BuildSyntheticSourceDisjointRecords(100),
                new DesignConversionTrainingPipelineOptions(
                    $"failing-admission-{failure}",
                    BatchSize: 25,
                    DatasetOptions: LocalDatasetOptions(),
                    MeasurementGateRequest: FailingAdmissionMeasurement(failure)),
                custodyCapability: null,
                trainingBinding: null,
                ct: CancellationToken.None);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.False(result.Value!.MachineLifecycleHookAttempted);
            Assert.False(result.Value.LifecycleEvidence.PromotionUploadAllowed);
            Assert.Empty(mutationBoundaries);
            Assert.Empty(optimizerSinks);
        }
        finally
        {
            DesignConversionTrainingPipeline.MutationBoundaryObserverForTesting = previousBoundary;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting = previousOptimizer;
        }
    }


    [Fact]
    public void SnapshotOptions_NullNestedPreflightCollectionsFailStablyWithoutThrowing()
    {
        var options = new DesignConversionTrainingPipelineOptions(
            "null-preflight-collections",
            BatchSize: 1,
            DatasetOptions: LocalDatasetOptions() with
            {
                Preflight = LocalDatasetOptions().Preflight! with
                {
                    Reasons = null!,
                    EvidenceRefs = null!
                }
            },
            MeasurementGateRequest: PassingAdmissionMeasurement("null-preflight"));

        var captured = DesignConversionTrainingPipeline.SnapshotOptions(options);

        Assert.False(captured.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_TRAINING_OPTIONS_INVALID", captured.ErrorCode);
    }


    private static DesignConversionMeasurementGateRequest PassingAdmissionMeasurement(
        string scope)
        => new(
            GateId: $"admission:{scope}",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                50,
                0.95,
                10,
                0.01m,
                64,
                [$"artifact://measurement/{scope}/heldout.json"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                true,
                false,
                false,
                true,
                [$"artifact://measurement/{scope}/integrity.json"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                0.90,
                100,
                1m,
                512));


    private static DesignConversionMeasurementGateRequest FailingAdmissionMeasurement(
        string failure)
    {
        var passing = PassingAdmissionMeasurement($"failure-{failure}");
        return failure switch
        {
            "below_score" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with
                {
                    HeldOutQualityScore = 0.5
                }
            },
            "runtime" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { RuntimeMs = 101 }
            },
            "cost" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { CostUsd = 1.01m }
            },
            "ram" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { PeakRamMb = 513 }
            },
            "leakage" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { LeakageDetected = true }
            },
            "hidden_determinism" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with
                {
                    HiddenDeterminismDetected = true
                }
            },
            "ablation" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { AblationPassed = false }
            },
            "source_disjoint" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { SourceDisjoint = false }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(failure))
        };
    }

}
