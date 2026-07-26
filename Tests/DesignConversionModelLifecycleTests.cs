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
            var pipeline = await new DesignConversionTrainingPipeline().RunAsync(machine, records, options);
            Assert.True(pipeline.IsSuccess, pipeline.ErrorMessage);
            Assert.True(pipeline.Value!.DatasetCountGate.Passed);
            Assert.True(pipeline.Value.LifecycleEvidence.CheckpointCandidateCreated);
            Assert.True(pipeline.Value.LifecycleEvidence.CheckpointContainsRealTrainedWeights);
            Assert.Equal(DesignConversionLocalFreshLoadProbe.VerifiedStatus, pipeline.Value.LifecycleEvidence.FreshLoadStatus);
            Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, pipeline.Value.LifecycleEvidence.ContinueTrainingStatus);
            Assert.Equal(DesignConversionMeasurementGateStatus.Passed, pipeline.Value.LifecycleEvidence.MeasurementGateStatus);
            Assert.True(pipeline.Value.LifecycleEvidence.PromotionUploadAllowed);

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
            var continueProof = await DesignConversionLocalContinueTrainingProbe.VerifyFreshLoadedCheckpointAsync(
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
                pipeline.Value,
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
                promotedLocalModel: promoted.Value.Model);

            var response = await api.SubmitAndEvaluateAsync(CreateRequest("pass144-promoted-modelserver-request"));
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.True(response.Value!.LearnedInferenceApplied);
            Assert.False(response.Value.TrainedModelAuthority);
            Assert.True(response.Value.NotTrainedModelAuthority);
            Assert.True(response.Value.ModelTrainingVerified);
            Assert.False(response.Value.ModelUploadReady);
            Assert.False(response.Value.DeploymentVerified);
            Assert.False(response.Value.ProductReady);
            Assert.Equal("promoted_local_checkpoint_loaded", response.Value.TrainingReadiness.Status);
            Assert.Equal("local_promoted_checkpoint_loaded", response.Value.TrainingReadiness.PromotionStatus);
            Assert.Equal(promoted.Value.Model.ModelId, response.Value.PromotedLocalModel!.ModelId);
            Assert.Equal(promoted.Value.Model.ModelId, response.Value.BoundaryMetadata["local_promoted_model_id"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_model_promotion_verified"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_real_weights"]);
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
                EvidenceRefs: ["pass144-local-heldout-metric:deterministic-fixture"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs: ["pass144-local-ablation:deterministic-fixture"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

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

    private sealed class LocalOnlyPromotionUploader : IDesignConversionPromotionUploader
    {
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
