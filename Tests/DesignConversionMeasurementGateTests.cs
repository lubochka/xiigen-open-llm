// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;

public sealed class DesignConversionMeasurementGateTests
{
    [Fact]
    public void Evaluate_PassesSyntheticHeldOutMetricsWithRequiredProofs()
    {
        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(BuildPassingRequest());

        Assert.True(result.Passed);
        Assert.Equal(DesignConversionMeasurementGateStatus.Passed, result.Status);
        Assert.Equal("ALL_MEASUREMENT_GATES_PASSED", Assert.Single(result.Reasons));
        Assert.True(result.SourceDisjoint);
        Assert.True(result.AblationPassed);
        Assert.True(result.BudgetPassed);
        Assert.Contains("synthetic-held-out-metrics:pass", result.EvidenceRefs);
        Assert.Contains("synthetic-anti-leakage-ablation:pass", result.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED", result.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_FailsWhenHeldOutEvidenceIsMissing()
    {
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with
            {
                EvidenceRefs = []
            }
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.Status);
        Assert.Contains("HELD_OUT_MEASUREMENT_EVIDENCE_MISSING", result.Reasons);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", result.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_FailsWhenThresholdsAreMissing()
    {
        var request = BuildPassingRequest() with
        {
            Thresholds = null!
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.Status);
        Assert.Contains("MEASUREMENT_GATE_THRESHOLDS_REQUIRED", result.Reasons);
        Assert.False(result.BudgetPassed);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", result.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_FailsWhenQualityScoreIsNotFinite()
    {
        var invalidCases = new[]
        {
            (Score: double.NaN, Reason: "HELD_OUT_QUALITY_SCORE_INVALID:NaN"),
            (Score: double.PositiveInfinity, Reason: "HELD_OUT_QUALITY_SCORE_INVALID:Infinity"),
            (Score: double.NegativeInfinity, Reason: "HELD_OUT_QUALITY_SCORE_INVALID:-Infinity")
        };

        foreach (var invalidCase in invalidCases)
        {
            var request = BuildPassingRequest() with
            {
                HeldOutMetrics = BuildPassingMetrics() with
                {
                    HeldOutQualityScore = invalidCase.Score
                }
            };

            var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

            Assert.False(result.Passed);
            Assert.Equal(DesignConversionMeasurementGateStatus.Failed, result.Status);
            Assert.Contains(invalidCase.Reason, result.Reasons);
            Assert.DoesNotContain(
                result.Reasons,
                reason => reason.StartsWith("HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Evaluate_FailsWhenBudgetMetricsOrThresholdsAreInvalid()
    {
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with
            {
                RuntimeMs = -1,
                CostUsd = -0.01m,
                PeakRamMb = -1
            },
            Thresholds = new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: double.PositiveInfinity,
                MaxRuntimeMs: 0,
                MaxCostUsd: -0.01m,
                MaxPeakRamMb: 0)
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.False(result.BudgetPassed);
        Assert.Contains("REQUIRED_HELD_OUT_QUALITY_SCORE_THRESHOLD_INVALID:Infinity", result.Reasons);
        Assert.Contains("MAX_RUNTIME_MS_THRESHOLD_INVALID:0", result.Reasons);
        Assert.Contains("MAX_COST_USD_THRESHOLD_INVALID:-0.01", result.Reasons);
        Assert.Contains("MAX_PEAK_RAM_MB_THRESHOLD_INVALID:0", result.Reasons);
        Assert.Contains("RUNTIME_MS_INVALID:-1", result.Reasons);
        Assert.Contains("COST_USD_INVALID:-0.01", result.Reasons);
        Assert.Contains("PEAK_RAM_MB_INVALID:-1", result.Reasons);
        Assert.DoesNotContain(
            result.Reasons,
            reason => reason.Contains("_BUDGET_EXCEEDED", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_FailsWhenLeakageOrAblationShortcutIsFlagged()
    {
        var request = BuildPassingRequest() with
        {
            IntegrityProof = BuildPassingProof() with
            {
                SourceDisjoint = false,
                LeakageDetected = true,
                HiddenDeterminismDetected = true,
                AblationPassed = false
            }
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.Contains("SOURCE_DISJOINT_PROOF_FAILED", result.Reasons);
        Assert.Contains("LEAKAGE_DETECTED", result.Reasons);
        Assert.Contains("HIDDEN_DETERMINISM_DETECTED", result.Reasons);
        Assert.Contains("ABLATION_PROOF_FAILED", result.Reasons);
    }

    [Fact]
    public void Evaluate_FailsWhenQualityIsLowOrBudgetIsOverrun()
    {
        var request = BuildPassingRequest() with
        {
            HeldOutMetrics = BuildPassingMetrics() with
            {
                HeldOutQualityScore = 0.81,
                RuntimeMs = 901,
                CostUsd = 2.50m,
                PeakRamMb = 1024
            }
        };

        var result = new DesignConversionMeasurementGateEvaluator().Evaluate(request);

        Assert.False(result.Passed);
        Assert.False(result.BudgetPassed);
        Assert.Contains("HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD:0.81<0.9", result.Reasons);
        Assert.Contains("RUNTIME_MS_BUDGET_EXCEEDED:901>800", result.Reasons);
        Assert.Contains("COST_USD_BUDGET_EXCEEDED:2.50>1.00", result.Reasons);
        Assert.Contains("PEAK_RAM_MB_BUDGET_EXCEEDED:1024>512", result.Reasons);
    }

    [Fact]
    public async Task RunAsync_CarriesPassedMeasurementGateAndRealWeightsThenAllowsLocalPromotionPath()
    {
        var records = BuildSyntheticSourceDisjointRecords(100);
        var pipeline = new DesignConversionTrainingPipeline();
        var pipelineResult = await pipeline.RunAsync(
            new DesignConversionMachine(),
            records,
            new DesignConversionTrainingPipelineOptions(
                "s3-local-measurement-gate-pass",
                BatchSize: 25,
                MeasurementGateRequest: BuildPassingRequest(),
                DatasetOptions: LocalDatasetOptions()));

        Assert.True(pipelineResult.IsSuccess, pipelineResult.ErrorMessage);
        var lifecycle = pipelineResult.Value!.LifecycleEvidence;
        Assert.Equal(DesignConversionLocalContinueTrainingProbe.VerifiedStatus, lifecycle.ContinueTrainingStatus);
        Assert.Equal(DesignConversionMeasurementGateStatus.Passed, lifecycle.MeasurementGateStatus);
        Assert.NotNull(lifecycle.MeasurementGate);
        Assert.True(lifecycle.MeasurementGate!.Passed);
        Assert.True(lifecycle.CheckpointContainsRealTrainedWeights);
        Assert.Equal("PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS", lifecycle.PromotionStatus);
        Assert.True(lifecycle.PromotionUploadAllowed);
        Assert.DoesNotContain("held_out_measurement_gate", lifecycle.NotProvenFields);
        Assert.DoesNotContain("real_trained_weights", lifecycle.NotProvenFields);
        Assert.DoesNotContain("promotion_upload", lifecycle.NotProvenFields);

        var uploader = new FakePromotionUploader();
        var publication = await new DesignConversionPromotionPublisher(uploader).PublishAsync(
            new DesignConversionMachine(),
            pipelineResult.Value,
            "s3-local-measurement-promotion-blocked",
            BuildArtifacts(),
            BuildPromotionGate());

        Assert.True(publication.IsSuccess, publication.ErrorMessage);
        Assert.Equal(DesignConversionPromotionDecisionKind.Promote, publication.Value!.Decision.Kind);
        Assert.Contains("ALL_PROMOTION_GATES_PASSED", publication.Value.Decision.Reasons);
        Assert.Equal(1, uploader.UploadCallCount);
        Assert.NotNull(publication.Value.UploadReceipt);
        Assert.False(publication.Value.Manifest.ModelServerUploadAttempted);
        Assert.False(publication.Value.Manifest.ModelServerPromotionClaimed);
        Assert.True(publication.Value.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal("local_receipt_only", publication.Value.Manifest.PromotionReceiptKind);
        Assert.StartsWith(
            DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
            publication.Value.Manifest.PromotionUploadRef,
            StringComparison.Ordinal);
    }

    private static DesignConversionMeasurementGateRequest BuildPassingRequest()
        => new(
            GateId: "s3-local-measurement-gate-synthetic",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: BuildPassingMetrics(),
            IntegrityProof: BuildPassingProof(),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionHeldOutMeasurementMetrics BuildPassingMetrics()
        => new(
            HeldOutRecordCount: 50,
            HeldOutQualityScore: 0.94,
            RuntimeMs: 320,
            CostUsd: 0.25m,
            PeakRamMb: 256,
            EvidenceRefs: ["synthetic-held-out-metrics:pass"]);

    private static DesignConversionMeasurementIntegrityProof BuildPassingProof()
        => new(
            SourceDisjoint: true,
            LeakageDetected: false,
            HiddenDeterminismDetected: false,
            AblationPassed: true,
            EvidenceRefs: ["synthetic-anti-leakage-ablation:pass"]);

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildSyntheticSourceDisjointRecords(int count)
    {
        var result = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            Enumerable.Range(0, count)
                .Select(index =>
                {
                    var unitId = index % 2 == 0
                        ? DesignConversionDpoUnits.CriticUnit
                        : DesignConversionDpoUnits.ImproverUnit;
                    var example = DesignConversionDpoGeneratorTests.CreateExample(
                        $"s3-measurement-source-disjoint-{index:000}",
                        unitId,
                        usefulness: 0.8 + (index % 10 * 0.01));
                    return example with
                    {
                        Provenance = example.Provenance with
                        {
                            SourceRequestId = $"request:s3-measurement-source-{index:000}",
                            SourceManifestRef = $"manifest://dcm-source/s3-measurement-source-{index:000}",
                            Metadata = new Dictionary<string, string>
                            {
                                ["source_design_id"] = $"s3-measurement-source-{index:000}"
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

    private static DesignConversionDpoDatasetOptions LocalDatasetOptions(
        string DatasetId = "dcm-dpo-measurement-dataset",
        int MinimumTrainRecords = 50,
        int MinimumHeldOutRecords = 50)
        => new(
            DatasetId: DatasetId,
            MinimumTrainRecords: MinimumTrainRecords,
            MinimumHeldOutRecords: MinimumHeldOutRecords,
            Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly());

    private static DesignConversionTrainingManifestArtifacts BuildArtifacts()
        => new(
            CheckpointPath: "artifacts/design-conversion/checkpoint.json",
            TokenizerPath: "artifacts/design-conversion/tokenizer.json",
            PolicyPath: "artifacts/design-conversion/policy.bin");

    private static DesignConversionPromotionGateReport BuildPromotionGate()
    {
        var artifacts = BuildArtifacts();
        var artifactPaths = new[]
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
            BudgetTier: "bounded-test-tier",
            DpoQuarantineCount: 0,
            MaxDpoQuarantineCount: 1,
            RequiredArtifactPaths: artifactPaths,
            PresentArtifactPaths: artifactPaths,
            EvidenceRefs: ["synthetic-promotion-gate:pass"]);
    }

    private sealed class FakePromotionUploader : IDesignConversionPromotionUploader
    {
        public int UploadCallCount { get; private set; }

        public Task<AgentInfrastructure.OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
        {
            UploadCallCount++;
            return Task.FromResult(AgentInfrastructure.OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    UploadId: "local-only-upload-receipt",
                    RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                               Uri.EscapeDataString(manifest.ManifestId),
                    UploadedAtUtc: DateTimeOffset.UnixEpoch,
                    EvidenceRefs: ["LOCAL_ONLY_PROMOTION_UPLOADER_CALLED"])));
        }
    }
}
