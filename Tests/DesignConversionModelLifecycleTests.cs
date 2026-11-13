// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelLifecycleTests
{
    [Fact]
    public async Task Pass144_LocalLifecycle_TrainsPromotesLoadsAndModelServerUsesPromotedCheckpoint()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", "pass144", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var records = BuildSourceDisjointRecords(100);
            var options = new DesignConversionTrainingPipelineOptions(
                "pass144-local-model-lifecycle",
                BatchSize: 25,
                MeasurementGateRequest: BuildPassingMeasurementGate(),
                DatasetOptions: LocalDatasetOptions());
            var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(records, options.DatasetOptions);
            Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
            Assert.True(dataset.Value!.Manifest.CountGate.Passed);

            var machine = new DesignConversionMachine();
            var pipeline = await DesignConversionLegalTeacherCapabilityTestFactory
                .RunStrictPipelineAsync(
                    new DesignConversionTrainingPipeline(),
                    machine,
                    records,
                    options);
            Assert.True(pipeline.IsSuccess, pipeline.ErrorMessage);
            Assert.True(pipeline.Value!.DatasetCountGate.Passed);
            Assert.True(pipeline.Value.LifecycleEvidence.CheckpointCandidateCreated);
            Assert.True(pipeline.Value.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
            Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, pipeline.Value.LifecycleEvidence.FreshLoadStatus);
            Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, pipeline.Value.LifecycleEvidence.ContinueTrainingStatus);
            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, pipeline.Value.LifecycleEvidence.MeasurementGateStatus);
            Assert.False(pipeline.Value.LifecycleEvidence.PromotionUploadAllowed);
            Assert.Contains(
                "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_NOT_AVAILABLE_IN_TRAINING_PIPELINE",
                pipeline.Value.LifecycleEvidence.FailureReasons);

            var promotionPipeline = BuildSyntheticChildDpoMeasuredPromotionFixture(
                pipeline.Value,
                "pass144-local-model-lifecycle");

            var checkpointPath = Path.Combine(root, "pass144-promoted-checkpoint.json");
            var checkpointSave = await machine.SaveCheckpointAsync(checkpointPath);
            Assert.True(checkpointSave.IsSuccess, checkpointSave.ErrorMessage);
            Assert.True(File.Exists(checkpointPath));

            var freshLoad = await DesignConversionLocalFreshLoadProbe.VerifyCheckpointFileAsync(
                checkpointPath,
                checkpointSave.Value!.ManifestRef);
            Assert.True(freshLoad.IsSuccess, freshLoad.ErrorMessage);
            Assert.True(freshLoad.Value!.MetadataContinuityVerified);
            Assert.True(freshLoad.Value.CheckpointContainsRealTrainedWeights);

            var continuationBatch = pipeline.Value.Batches[0] with
            {
                BatchId = "pass144-local-model-lifecycle:manual-continue-proof"
            };
            var continuedCheckpointPath = Path.Combine(root, "pass144-continued-checkpoint.json");
            var continueProof = await DesignConversionLegalTeacherCapabilityTestFactory.ContinueTrainingStrictAsync(
                freshLoad.Value,
                continuationBatch,
                dataset.Value.HeldOutRows.Select(row => row.RecordIdentityHash).ToArray(),
                continuedCheckpointPath,
                datasetCountGatePassed: true);
            Assert.True(continueProof.IsSuccess, continueProof.ErrorMessage);
            Assert.True(continueProof.Value!.SourceCheckpointUnchanged);
            Assert.True(continueProof.Value.ContinuationBatchAppended);
            Assert.True(continueProof.Value.RealTrainedWeightsContinued);
            Assert.Equal(0, continueProof.Value.HeldOutRecordUsageCount);
            Assert.True(File.Exists(continuedCheckpointPath));

            var artifacts = await WritePromotionArtifactsAsync(root, checkpointPath);
            var publication = await new DesignConversionPromotionPublisher(new LocalOnlyPromotionUploader()).PublishAsync(
                machine,
                promotionPipeline,
                "pass144-local-model-manifest",
                artifacts,
                BuildPassingPromotionGate(artifacts));
            Assert.True(publication.IsSuccess, publication.ErrorMessage);
            Assert.Equal(DesignConversionPromotionDecisionKind.Promote, publication.Value!.Decision.Kind);
            Assert.False(publication.Value.Manifest.ModelServerUploadAttempted);
            Assert.False(publication.Value.Manifest.ModelServerPromotionClaimed);
            Assert.True(publication.Value.Manifest.LocalPromotionReceiptWritten);
            Assert.Equal("local_receipt_only", publication.Value.Manifest.PromotionReceiptKind);
            Assert.Contains("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED_LOCAL_RECEIPT", publication.Value.Manifest.EvidenceRefs);
            Assert.Contains("MODEL_SERVER_PROMOTION_NOT_CLAIMED_LOCAL_RECEIPT", publication.Value.Manifest.EvidenceRefs);
            Assert.StartsWith(
                DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
                publication.Value.Manifest.PromotionUploadRef,
                StringComparison.Ordinal);

            var promoted = await DesignConversionPromotedLocalModelLoader.LoadAsync(
                publication.Value,
                checkpointPath);
            Assert.True(promoted.IsSuccess, promoted.ErrorMessage);
            Assert.True(promoted.Value!.Model.CheckpointFreshLoadVerified);
            Assert.True(promoted.Value.Model.CheckpointContainsRealTrainedWeights);
            Assert.True(promoted.Value.Model.LearnedWeightArtifactCount > 0);
            Assert.True(promoted.Value.Model.LearnedWeightObservationCount > 0);

            var service = DesignConversionPromotedLocalModelLoader.CreateInternalService(
                promoted.Value,
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(CreateVerdict("pass144-modelserver-verdict", 0.90, passed: true)));
            var api = new DesignConversionModelServerApi(
                service,
                () => DateTimeOffset.Parse("2026-07-03T12:00:00Z"),
                promotedLocalModel: promoted.Value.Model,
                postEvaluationStateStore: new JsonDesignConversionPostEvaluationStateStore(
                    Path.Combine(root, "pass144-post-evaluation-state")));

            var response = await api.SubmitAndEvaluateAsync(CreateRequest("pass144-promoted-modelserver-request"));
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.True(response.Value!.LearnedInferenceApplied);
            Assert.False(response.Value.TrainedModelAuthority);
            Assert.True(response.Value.NotTrainedModelAuthority);
            Assert.False(response.Value.ModelTrainingVerified);
            Assert.False(response.Value.ModelUploadReady);
            Assert.False(response.Value.DeploymentVerified);
            Assert.False(response.Value.ProductReady);
            Assert.Equal(
                "promoted_local_model_informational_verification_not_proven",
                response.Value.TrainingReadiness.Status);
            Assert.Equal(
                "local_promoted_model_informational_verification_not_proven",
                response.Value.TrainingReadiness.PromotionStatus);
            Assert.Equal(promoted.Value.Model.ModelId, response.Value.PromotedLocalModel!.ModelId);
            Assert.Equal(promoted.Value.Model.ModelId, response.Value.BoundaryMetadata["local_promoted_model_id"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["local_model_promotion_verified"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["local_promoted_checkpoint_real_weights"]);
            Assert.Equal("not_proven", response.Value.BoundaryMetadata["local_model_verification_authority"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(
                "critic_local_checkpoint_feature_adjusted_score",
                response.Value.BoundaryMetadata["logical_flow_primary_metric_name"]);
            Assert.True(
                int.Parse(
                    response.Value.BoundaryMetadata["LocalCheckpointFeatureContributionCount"],
                    System.Globalization.CultureInfo.InvariantCulture) > 0);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_checkpoint_weights_configured"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["trained_model_authority"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["not_trained_model_authority"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["product_ready"]);
            Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, response.Value.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, response.Value.EvidenceRefs);
            Assert.Contains("logical-flow:design-conversion-routing-flow", response.Value.EvidenceRefs);
            Assert.Contains("logical-orchestrator-selected-flow:design-conversion-routing-flow", response.Value.EvidenceRefs);
            Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, response.Value.Warnings);

            var status = api.GetJobResult(response.Value.JobId);
            Assert.True(status.IsSuccess, status.ErrorMessage);
            Assert.True(status.Value!.LearnedInferenceApplied);
            Assert.False(status.Value.TrainedModelAuthority);
            Assert.True(status.Value.NotTrainedModelAuthority);
            Assert.Equal(promoted.Value.Model.ModelId, status.Value.BoundaryMetadata["local_promoted_model_id"]);
            Assert.Equal(
                "critic_local_checkpoint_feature_adjusted_score",
                status.Value.BoundaryMetadata["logical_flow_primary_metric_name"]);
            Assert.True(
                int.Parse(
                    status.Value.BoundaryMetadata["LocalCheckpointFeatureContributionCount"],
                    System.Globalization.CultureInfo.InvariantCulture) > 0);
            Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["local_checkpoint_weights_configured"]);
            Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["trained_model_authority"]);
            Assert.Equal(bool.TrueString, status.Value.BoundaryMetadata["not_trained_model_authority"]);
            Assert.Equal(bool.FalseString, status.Value.BoundaryMetadata["product_ready"]);
            Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, status.Value.RuntimeStatus.Warnings);
            Assert.False(status.Value.DeploymentVerified);
            Assert.False(status.Value.ProductReady);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Pass145_PromotedLocalModelLoaderRejectsManifestMatchedCheckpointWithoutRealLearnedWeights()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", "pass145", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var records = BuildSourceDisjointRecords(100);
            var options = new DesignConversionTrainingPipelineOptions(
                "pass145-local-promoted-loader-real-weights-required",
                BatchSize: 25,
                MeasurementGateRequest: BuildPassingMeasurementGate(),
                DatasetOptions: LocalDatasetOptions());
            var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(records, options.DatasetOptions);
            Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
            Assert.True(dataset.Value!.Manifest.CountGate.Passed);

            var trainedMachine = new DesignConversionMachine();
            var pipeline = await DesignConversionLegalTeacherCapabilityTestFactory
                .RunStrictPipelineAsync(
                    new DesignConversionTrainingPipeline(),
                    trainedMachine,
                    records,
                    options);
            Assert.True(pipeline.IsSuccess, pipeline.ErrorMessage);
            Assert.True(pipeline.Value!.LifecycleEvidence.CheckpointCandidateCreated);
            Assert.True(pipeline.Value.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, pipeline.Value.LifecycleEvidence.MeasurementGateStatus);
            Assert.False(pipeline.Value.LifecycleEvidence.PromotionUploadAllowed);

            var promotionPipeline = BuildSyntheticChildDpoMeasuredPromotionFixture(
                pipeline.Value,
                "pass145-local-promoted-loader-real-weights-required");

            var checkpointPath = Path.Combine(root, "pass145-promoted-checkpoint.json");
            var checkpointSave = await trainedMachine.SaveCheckpointAsync(checkpointPath);
            Assert.True(checkpointSave.IsSuccess, checkpointSave.ErrorMessage);
            Assert.True(File.Exists(checkpointPath));

            var trainedLoad = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(trainedLoad.IsSuccess, trainedLoad.ErrorMessage);
            Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(trainedLoad.Value!.Checkpoint));

            var artifacts = await WritePromotionArtifactsAsync(root, checkpointPath);
            var publication = await new DesignConversionPromotionPublisher(new LocalOnlyPromotionUploader()).PublishAsync(
                trainedMachine,
                promotionPipeline,
                "pass145-local-model-manifest",
                artifacts,
                BuildPassingPromotionGate(artifacts));
            Assert.True(publication.IsSuccess, publication.ErrorMessage);
            Assert.Equal(DesignConversionPromotionDecisionKind.Promote, publication.Value!.Decision.Kind);
            Assert.True(publication.Value.Manifest.LocalPromotionReceiptWritten);
            Assert.Equal("local_receipt_only", publication.Value.Manifest.PromotionReceiptKind);
            Assert.False(publication.Value.Manifest.ModelServerUploadAttempted);
            Assert.False(publication.Value.Manifest.ModelServerPromotionClaimed);
            Assert.Equal(Path.GetFullPath(checkpointPath), Path.GetFullPath(publication.Value.Manifest.PromotionArtifacts!.CheckpointPath));

            var pristineCheckpointPath = Path.Combine(root, "pass145-pristine-checkpoint.json");
            var pristineSave = await new DesignConversionMachine().SaveCheckpointAsync(pristineCheckpointPath);
            Assert.True(pristineSave.IsSuccess, pristineSave.ErrorMessage);
            Assert.True(File.Exists(pristineCheckpointPath));

            var pristineLoad = await DesignConversionMachine.LoadCheckpointAsync(pristineCheckpointPath);
            Assert.True(pristineLoad.IsSuccess, pristineLoad.ErrorMessage);
            Assert.Equal(publication.Value.Manifest.MachineId, pristineLoad.Value!.Checkpoint.MachineId);
            Assert.Equal(publication.Value.Manifest.DomainTag, pristineLoad.Value.Checkpoint.DomainTag);
            Assert.False(DesignConversionMachine.CheckpointHasRealLearnedWeights(pristineLoad.Value.Checkpoint));

            var manifestMatchedPristineCheckpoint = pristineLoad.Value.Checkpoint with
            {
                Version = publication.Value.Manifest.Version
            };
            Assert.Equal(publication.Value.Manifest.MachineId, manifestMatchedPristineCheckpoint.MachineId);
            Assert.Equal(publication.Value.Manifest.DomainTag, manifestMatchedPristineCheckpoint.DomainTag);
            Assert.Equal(publication.Value.Manifest.Version, manifestMatchedPristineCheckpoint.Version);
            Assert.False(DesignConversionMachine.CheckpointHasRealLearnedWeights(manifestMatchedPristineCheckpoint));
            await File.WriteAllTextAsync(
                pristineCheckpointPath,
                JsonSerializer.Serialize(manifestMatchedPristineCheckpoint, DesignConversionCheckpointJsonOptions.Standard));

            File.Copy(pristineCheckpointPath, checkpointPath, overwrite: true);
            var replacementLoad = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(replacementLoad.IsSuccess, replacementLoad.ErrorMessage);
            Assert.Equal(publication.Value.Manifest.MachineId, replacementLoad.Value!.Checkpoint.MachineId);
            Assert.Equal(publication.Value.Manifest.DomainTag, replacementLoad.Value.Checkpoint.DomainTag);
            Assert.Equal(publication.Value.Manifest.Version, replacementLoad.Value.Checkpoint.Version);
            Assert.False(DesignConversionMachine.CheckpointHasRealLearnedWeights(replacementLoad.Value.Checkpoint));

            var promoted = await DesignConversionPromotedLocalModelLoader.LoadAsync(
                publication.Value,
                checkpointPath);
            Assert.False(promoted.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_PROMOTED_CHECKPOINT_REAL_WEIGHTS_REQUIRED", promoted.ErrorCode);
            Assert.Null(promoted.Value);
            Assert.False(
                (promoted.ErrorMessage ?? string.Empty).Contains(root, StringComparison.OrdinalIgnoreCase),
                promoted.ErrorMessage);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildSourceDisjointRecords(int count)
    {
        var result = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            Enumerable.Range(0, count)
                .Select(index =>
                {
                    var unitId = index % 2 == 0
                        ? DesignConversionDpoUnits.CriticUnit
                        : DesignConversionDpoUnits.ImproverUnit;
                    var sourceId = $"pass144-source-{index:000}";
                    var example = DesignConversionDpoGeneratorTests.CreateExample(
                        $"pass144-record-{index:000}",
                        unitId,
                        usefulness: 0.8 + (index % 10 * 0.01));
                    return example with
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
                })
                .ToArray());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        Assert.Equal(count, result.Value.AcceptedRecords.Count);
        return result.Value.AcceptedRecords;
    }

    private static DesignConversionDpoDatasetOptions LocalDatasetOptions()
        => new(
            DatasetId: "pass144-dpo-dataset",
            MinimumTrainRecords: 50,
            MinimumHeldOutRecords: 50,
            Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly());

    private static DesignConversionMeasurementGateRequest BuildPassingMeasurementGate()
        => new(
            GateId: "pass144-local-measurement-gate",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 50,
                HeldOutQualityScore: 0.94,
                RuntimeMs: 320,
                CostUsd: 0m,
                PeakRamMb: 256,
                EvidenceRefs:
                ["artifact://caller-measurement/pass144/heldout.json"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs:
                ["artifact://caller-measurement/pass144/integrity.json"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionTrainingPipelineResult BuildSyntheticChildDpoMeasuredPromotionFixture(
        DesignConversionTrainingPipelineResult result,
        string scope)
    {
        AssertRawPipelineRequiresChildDpoMeasurement(result);

        var measurementGate = new DesignConversionMeasurementGateResult(
            GateId: $"{scope}:child-dpo-measurement-gate",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            Status: DesignConversionMeasurementGateStatus.Passed,
            Passed: true,
            RequiredHeldOutQualityScore: 0.90,
            HeldOutQualityScore: 0.94,
            SourceDisjoint: true,
            AblationPassed: true,
            BudgetPassed: true,
            Reasons: ["ALL_MEASUREMENT_GATES_PASSED"],
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED",
                DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
                "DESIGN_CONVERSION_TEST_ONLY_CHILD_DPO_MEASUREMENT_FIXTURE",
                $"artifact://child-dpo-measurement/{scope}/heldout.json",
                $"artifact://child-dpo-measurement/{scope}/integrity.json"
            ]);

        var lifecycleEvidenceRefs = result.LifecycleEvidence.EvidenceRefs
            .Where(reference => !IsRawChildDpoMeasurementBlockedEvidence(reference))
            .Concat(measurementGate.EvidenceRefs)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var notProvenFields = result.LifecycleEvidence.NotProvenFields
            .Where(field => !string.Equals(field, "held_out_measurement_gate", StringComparison.Ordinal))
            .Where(field => !string.Equals(field, "promotion_upload", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var synthetic = result with
        {
            LifecycleEvidence = result.LifecycleEvidence with
            {
                MeasurementGate = measurementGate,
                MeasurementGateStatus = DesignConversionMeasurementGateStatus.Passed,
                PromotionStatus = "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
                PromotionUploadAllowed = true,
                NotProvenFields = notProvenFields,
                EvidenceRefs = lifecycleEvidenceRefs,
                FailureReasons = []
            }
        };

        AssertSyntheticChildDpoMeasuredPromotionFixture(synthetic);
        return synthetic;
    }

    private static void AssertRawPipelineRequiresChildDpoMeasurement(
        DesignConversionTrainingPipelineResult result)
    {
        Assert.True(result.LifecycleEvidence.CheckpointCandidateCreated);
        Assert.True(result.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
        Assert.Equal(
            DesignConversionLocalFreshLoadProbe.VerifiedStatus,
            result.LifecycleEvidence.FreshLoadStatus);
        Assert.Equal(
            DesignConversionLocalContinueTrainingProbe.VerifiedStatus,
            result.LifecycleEvidence.ContinueTrainingStatus);
        Assert.NotNull(result.LifecycleEvidence.LocalFreshLoadProbe);
        Assert.NotNull(result.LifecycleEvidence.LocalContinueTrainingProbe);
        Assert.True(result.LifecycleEvidence.LocalFreshLoadProbe!.CheckpointContainsRealTrainedWeights);
        Assert.True(result.LifecycleEvidence.LocalContinueTrainingProbe!.RealTrainedWeightsContinued);
        Assert.Equal(0, result.LifecycleEvidence.LocalContinueTrainingProbe.HeldOutRecordUsageCount);
        Assert.NotNull(result.LifecycleEvidence.MeasurementGate);
        Assert.Equal(
            DesignConversionMeasurementGateStatus.Failed,
            result.LifecycleEvidence.MeasurementGate!.Status);
        Assert.False(result.LifecycleEvidence.MeasurementGate.Passed);
        Assert.Equal(
            DesignConversionMeasurementGateStatus.Failed,
            result.LifecycleEvidence.MeasurementGateStatus);
        Assert.Equal(
            "PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED",
            result.LifecycleEvidence.PromotionStatus);
        Assert.False(result.LifecycleEvidence.PromotionUploadAllowed);
        Assert.Contains("held_out_measurement_gate", result.LifecycleEvidence.NotProvenFields);
        Assert.Contains("promotion_upload", result.LifecycleEvidence.NotProvenFields);
        Assert.Contains("trained_feature_fresh_load_metrics", result.LifecycleEvidence.NotProvenFields);
        Assert.Equal(
            ChildDpoMeasurementAuthorityFailureReasons,
            result.LifecycleEvidence.MeasurementGate.Reasons.Order(StringComparer.Ordinal).ToArray());
        Assert.Equal(
            ChildDpoMeasurementAuthorityFailureReasons,
            result.LifecycleEvidence.FailureReasons.Order(StringComparer.Ordinal).ToArray());
        Assert.Contains("CALLER_ADMISSION_MEASUREMENT_ADVISORY_ONLY", result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains(
            "CALLER_ADMISSION_MEASUREMENT_IS_NOT_CHILD_DPO_ACCURACY",
            result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_CHILD_DPO_PROMOTION_AUTHORITY_NOT_AVAILABLE_IN_PIPELINE",
            result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", result.LifecycleEvidence.EvidenceRefs);
        Assert.DoesNotContain(
            "DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED",
            result.LifecycleEvidence.EvidenceRefs);
        Assert.DoesNotContain(
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
            result.LifecycleEvidence.EvidenceRefs);
    }

    private static void AssertSyntheticChildDpoMeasuredPromotionFixture(
        DesignConversionTrainingPipelineResult result)
    {
        Assert.True(result.LifecycleEvidence.CheckpointCandidateCreated);
        Assert.True(result.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
        Assert.Equal(
            DesignConversionLocalFreshLoadProbe.VerifiedStatus,
            result.LifecycleEvidence.FreshLoadStatus);
        Assert.Equal(
            DesignConversionLocalContinueTrainingProbe.VerifiedStatus,
            result.LifecycleEvidence.ContinueTrainingStatus);
        Assert.True(result.LifecycleEvidence.LocalFreshLoadProbe!.CheckpointContainsRealTrainedWeights);
        Assert.True(result.LifecycleEvidence.LocalContinueTrainingProbe!.RealTrainedWeightsContinued);
        Assert.Equal(0, result.LifecycleEvidence.LocalContinueTrainingProbe.HeldOutRecordUsageCount);
        Assert.NotNull(result.LifecycleEvidence.MeasurementGate);
        Assert.Equal(
            DesignConversionMeasurementGateStatus.Passed,
            result.LifecycleEvidence.MeasurementGate!.Status);
        Assert.True(result.LifecycleEvidence.MeasurementGate.Passed);
        Assert.Equal(0.90, result.LifecycleEvidence.MeasurementGate.RequiredHeldOutQualityScore);
        Assert.Equal(0.94, result.LifecycleEvidence.MeasurementGate.HeldOutQualityScore);
        Assert.True(result.LifecycleEvidence.MeasurementGate.SourceDisjoint);
        Assert.True(result.LifecycleEvidence.MeasurementGate.AblationPassed);
        Assert.True(result.LifecycleEvidence.MeasurementGate.BudgetPassed);
        Assert.Equal(
            DesignConversionMeasurementGateStatus.Passed,
            result.LifecycleEvidence.MeasurementGateStatus);
        Assert.Equal(
            "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
            result.LifecycleEvidence.PromotionStatus);
        Assert.True(result.LifecycleEvidence.PromotionUploadAllowed);
        Assert.Empty(result.LifecycleEvidence.FailureReasons);
        Assert.Contains("trained_feature_fresh_load_metrics", result.LifecycleEvidence.NotProvenFields);
        Assert.DoesNotContain("held_out_measurement_gate", result.LifecycleEvidence.NotProvenFields);
        Assert.DoesNotContain("promotion_upload", result.LifecycleEvidence.NotProvenFields);
        Assert.DoesNotContain("real_trained_weights", result.LifecycleEvidence.NotProvenFields);
        Assert.Contains(
            "DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED",
            result.LifecycleEvidence.MeasurementGate.EvidenceRefs);
        Assert.Contains(
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
            result.LifecycleEvidence.MeasurementGate.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_TEST_ONLY_CHILD_DPO_MEASUREMENT_FIXTURE",
            result.LifecycleEvidence.MeasurementGate.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED",
            result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains(
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
            result.LifecycleEvidence.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_TEST_ONLY_CHILD_DPO_MEASUREMENT_FIXTURE",
            result.LifecycleEvidence.EvidenceRefs);
        Assert.All(
            result.LifecycleEvidence.EvidenceRefs,
            reference => Assert.False(
                IsRawChildDpoMeasurementBlockedEvidence(reference),
                reference));
        Assert.All(
            result.LifecycleEvidence.MeasurementGate.EvidenceRefs,
            reference => Assert.False(
                IsRawChildDpoMeasurementBlockedEvidence(reference),
                reference));
    }

    private static bool IsRawChildDpoMeasurementBlockedEvidence(string reference)
        => string.Equals(reference, "CALLER_ADMISSION_MEASUREMENT_ADVISORY_ONLY", StringComparison.Ordinal) ||
           string.Equals(reference, "CALLER_ADMISSION_MEASUREMENT_IS_NOT_CHILD_DPO_ACCURACY", StringComparison.Ordinal) ||
           string.Equals(reference, "DESIGN_CONVERSION_CHILD_DPO_PROMOTION_AUTHORITY_NOT_AVAILABLE_IN_PIPELINE", StringComparison.Ordinal) ||
           string.Equals(reference, "DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", StringComparison.Ordinal) ||
           string.Equals(
               reference,
               "DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE",
               StringComparison.Ordinal) ||
           string.Equals(
               reference,
               DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGateBlockedEvidence,
               StringComparison.Ordinal);

    private static readonly string[] ChildDpoMeasurementAuthorityFailureReasons =
    [
        "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_NOT_AVAILABLE_IN_TRAINING_PIPELINE",
        "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_BUDGET_AUTHORITY_REQUIRED",
        "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_INTEGRITY_AUTHORITY_REQUIRED"
    ];

    private static async Task<DesignConversionTrainingManifestArtifacts> WritePromotionArtifactsAsync(
        string root,
        string checkpointPath)
    {
        var tokenizerPath = Path.Combine(root, "pass144-tokenizer.json");
        var policyPath = Path.Combine(root, "pass144-policy.json");
        await File.WriteAllTextAsync(tokenizerPath, "{\"kind\":\"pass144-local-tokenizer-boundary\"}");
        await File.WriteAllTextAsync(policyPath, "{\"kind\":\"pass144-local-policy-boundary\"}");
        Assert.True(File.Exists(checkpointPath));
        Assert.True(File.Exists(tokenizerPath));
        Assert.True(File.Exists(policyPath));

        return new DesignConversionTrainingManifestArtifacts(
            CheckpointPath: checkpointPath,
            TokenizerPath: tokenizerPath,
            PolicyPath: policyPath);
    }

    private static DesignConversionPromotionGateReport BuildPassingPromotionGate(
        DesignConversionTrainingManifestArtifacts artifacts)
    {
        var paths = new[]
        {
            artifacts.CheckpointPath,
            artifacts.TokenizerPath,
            artifacts.PolicyPath
        };

        return new DesignConversionPromotionGateReport(
            HeldOutScore: 0.94,
            RequiredHeldOutScore: 0.90,
            HeldOutSourceDisjoint: true,
            DeterministicShortcutDetected: false,
            HiddenShortcutEvidenceDetected: false,
            FreshLoadPassed: true,
            BudgetTierPolicyPassed: true,
            BudgetTier: "pass144-local-only",
            DpoQuarantineCount: 0,
            MaxDpoQuarantineCount: 0,
            RequiredArtifactPaths: paths,
            PresentArtifactPaths: paths,
            EvidenceRefs:
            [
                "pass144-local-promotion-gate:deterministic-fixture",
                "pass144-local-only-no-cloud-upload"
            ]);
    }

    private static DesignConversionModelServerApiRequest CreateRequest(string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "pass144-figma-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass144-figma-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"safe\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "pass144-original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass144-original-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"original\"}",
                Format: "figma-json"),
            Html: "<main class=\"screen\"><h1>Dashboard</h1></main>",
            Css: ".screen{display:grid;gap:16px;}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "pass144-screenshot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://pass144/screenshot-desktop.png",
                    Sha256: "sha256:pass144-screenshot-desktop")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "pass144-design-render-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://pass144/design-render-desktop.png",
                    Sha256: "sha256:pass144-design-render-desktop")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "user-redacted",
                Tier: "local",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0m),
            Metadata: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "pass144-local-source-manifest",
                ["caller"] = "pass144-local-modelserver-proof"
            });

    private static VisualVerdict CreateVerdict(string verdictId, double score, bool passed)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects: passed
                ? []
                :
                [
                    new VisualDefect(
                        DefectId: $"{verdictId}:spacing",
                        Category: "spacing",
                        Severity: "major",
                        Description: "Spacing differs from expected render.",
                        X: 16,
                        Y: 24,
                        Width: 80,
                        Height: 20,
                        EvidenceImageIds: ["expected", "actual"])
                ],
            EvidenceRefs: [$"verdict:{verdictId}"],
            Passed: passed);

    private sealed class LocalOnlyPromotionUploader : IDesignConversionBoundedPromotionUploader,
        IDesignConversionPromotionUploaderPreflight
    {
        public int MaximumSerializedReceiptUtf8Bytes => 64 * 1024;
        public OperationResult<string> PreflightConfigurationFingerprintSha256()
            => OperationResult<string>.Ok("sha256:" + new string('a', 64));

        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    UploadId: "pass144-local-only-promotion",
                    RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                               Uri.EscapeDataString(manifest.ManifestId),
                    UploadedAtUtc: DateTimeOffset.UnixEpoch,
                    EvidenceRefs:
                    [
                        "PASS144_LOCAL_ONLY_PROMOTION_UPLOADER_CALLED",
                        "PASS144_CLOUD_UPLOAD_NOT_ATTEMPTED"
                    ])));
    }

    private sealed class RecordingRenderProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass144/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["pass144-recording-render-provider"])));
        }
    }

    private sealed class RecordingComparisonProvider(VisualVerdict verdict) : IDesignConversionComparisonProvider
    {
        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://pass144/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["pass144-comparison-provider"])));
    }
}
