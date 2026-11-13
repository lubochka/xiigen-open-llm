// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;

public sealed class DesignConversionTrainingMeasurementGateTests
{
    [Theory]
    [InlineData("synthetic_negative")]
    [InlineData("scaffold_negative")]
    [InlineData("high_quality_negative_pair_not_proven")]
    public void GetRecordBlockReasons_ApprovedRuntimeCandidateCannotWaiveIrreversiblePoison(
        string marker)
    {
        var record = WithApprovedRuntimeCandidateBoundary(
            BuildTrainingRecord(
                $"approved-runtime-{marker}",
                DesignConversionDpoUnits.CriticUnit,
                $"approved-runtime-source-{marker}"),
            marker,
            bool.TrueString);

        var reasons = DesignConversionTrainingMeasurementGatePolicy
            .GetRecordBlockReasons(record);

        Assert.Contains(marker, reasons);
        Assert.DoesNotContain("not_training_grade", reasons);
    }

    [Theory]
    [InlineData("provenance", "legal_verdict", "EVAL_ONLY", "eval_only_legal_verdict")]
    [InlineData("provenance", "LegalVerdict", "eval-only", "eval_only_legal_verdict")]
    [InlineData("license", "legal-verdict", "eval_only", "eval_only_legal_verdict")]
    [InlineData("license", "legal verdict", "EvalOnly", "eval_only_legal_verdict")]
    [InlineData("provenance", "EvalOnly", "True", "eval_only_legal_verdict")]
    [InlineData("license", "eval_only", "True", "eval_only_legal_verdict")]
    [InlineData("provenance", "legalVerdict", "QUARANTINE", "non_trainable_legal_verdict")]
    [InlineData("license", "legal_verdict", "BLOCK", "non_trainable_legal_verdict")]
    [InlineData("provenance", "legal-verdict", "DATASET_REBUILD_REQUIRED", "non_trainable_legal_verdict")]
    [InlineData("license", "LegalVerdict", "BLOCK_FOR_COMMON", "non_trainable_legal_verdict")]
    [InlineData("provenance", "legal verdict", "BLOCK_FOR_HOSTED", "non_trainable_legal_verdict")]
    public void GetRecordBlockReasons_ApprovedRuntimeCandidateCannotWaiveEvalOnlyLegalVerdict(
        string surface,
        string key,
        string value,
        string expectedReason)
    {
        var record = WithLegalVerdictMetadata(
            WithApprovedRuntimeCandidateBoundary(
                BuildTrainingRecord(
                    $"approved-runtime-legal-verdict-{surface}-{key}-{value}",
                    DesignConversionDpoUnits.CriticUnit,
                    $"approved-runtime-legal-verdict-source-{surface}-{key}-{value}"),
                "approved_for_training",
                bool.TrueString),
            surface,
            key,
            value);

        var reasons = DesignConversionTrainingMeasurementGatePolicy
            .GetRecordBlockReasons(record);

        Assert.Contains(expectedReason, reasons);
        Assert.DoesNotContain("not_training_grade", reasons);
        AssertAllowedLegalVerdictControls();
    }

    [Theory]
    [InlineData("synthetic_negative")]
    [InlineData("scaffold_negative")]
    [InlineData("high_quality_negative_pair_not_proven")]
    public void GetRecordBlockReasons_ApprovedRuntimeCandidateAllowsFalsePoisonMarker(
        string marker)
    {
        var record = WithApprovedRuntimeCandidateBoundary(
            BuildTrainingRecord(
                $"approved-runtime-false-{marker}",
                DesignConversionDpoUnits.CriticUnit,
                $"approved-runtime-false-source-{marker}"),
            marker,
            bool.FalseString);

        var reasons = DesignConversionTrainingMeasurementGatePolicy
            .GetRecordBlockReasons(record);

        Assert.Empty(reasons);
    }

    [Theory]
    [InlineData("forbidden_self_label")]
    [InlineData("runtime_teacher_selected_preference")]
    public void GetRecordBlockReasons_ApprovedRuntimeCandidateCannotWaiveSelfLabelAuthority(
        string marker)
    {
        var record = WithApprovedRuntimeCandidateBoundary(
            BuildTrainingRecord(
                $"approved-runtime-self-label-{marker}",
                DesignConversionDpoUnits.CriticUnit,
                $"approved-runtime-self-label-source-{marker}"),
            marker,
            bool.TrueString);

        var reasons = DesignConversionTrainingMeasurementGatePolicy
            .GetRecordBlockReasons(record);

        Assert.Contains("forbidden_self_label", reasons);
        Assert.DoesNotContain("not_training_grade", reasons);
    }

    [Theory]
    [InlineData("forbidden_self_label")]
    [InlineData("runtime_teacher_selected_preference")]
    public void GetRecordBlockReasons_ApprovedRuntimeCandidateAllowsFalseSelfLabelMarker(
        string marker)
    {
        var record = WithApprovedRuntimeCandidateBoundary(
            BuildTrainingRecord(
                $"approved-runtime-false-self-label-{marker}",
                DesignConversionDpoUnits.CriticUnit,
                $"approved-runtime-false-self-label-source-{marker}"),
            marker,
            bool.FalseString);

        var reasons = DesignConversionTrainingMeasurementGatePolicy
            .GetRecordBlockReasons(record);

        Assert.Empty(reasons);
    }

    [Fact]
    public void SelfLabelledApprovedRecordCannotEnterDatasetOrTrainingBatch()
    {
        var record = WithApprovedRuntimeCandidateBoundary(
            BuildTrainingRecord(
                "approved-runtime-self-label-consumer",
                DesignConversionDpoUnits.ImproverUnit,
                "approved-runtime-self-label-consumer-source"),
            "runtime_teacher_selected_preference",
            bool.TrueString);
        var options = LocalOptions("self-label-consumer-guard");

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            [record],
            options.DatasetOptions);
        var batches = new DesignConversionTrainingPipeline().BuildBatches(
            [record],
            options);

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.Empty(dataset.Value!.TrainRows);
        Assert.Empty(dataset.Value.HeldOutRows);
        Assert.Equal(0, dataset.Value.Manifest.AcceptedRecordCount);
        Assert.Equal(1, dataset.Value.Manifest.ExcludedRecordCount);
        Assert.True(batches.IsSuccess, batches.ErrorMessage);
        Assert.Empty(batches.Value!);
    }

    [Fact]
    public void EvalOnlyApprovedRecordCannotEnterDatasetOrTrainingBatch()
    {
        var accepted = WithLegalVerdictMetadata(
            WithApprovedRuntimeCandidateBoundary(
                BuildTrainingRecord(
                    "approved-runtime-eval-only-control",
                    DesignConversionDpoUnits.CriticUnit,
                    "approved-runtime-eval-only-control-source"),
                "approved_for_training",
                bool.TrueString),
            "provenance",
            "legal_verdict",
            "ACCEPT");
        var evalOnly = WithLegalVerdictMetadata(
            WithApprovedRuntimeCandidateBoundary(
                BuildTrainingRecord(
                    "approved-runtime-eval-only-blocked",
                    DesignConversionDpoUnits.ImproverUnit,
                    "approved-runtime-eval-only-blocked-source"),
                "approved_for_training",
                bool.TrueString),
            "license",
            "legalVerdict",
            "EvalOnly");
        var options = LocalOptions("eval-only-consumer-guard");

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            [accepted, evalOnly],
            options.DatasetOptions);
        var batches = new DesignConversionTrainingPipeline().BuildBatches(
            [accepted, evalOnly],
            options);
        var evalOnlyIdentityHash =
            DesignConversionTrainingMeasurementGatePolicy.ToRecordIdentityHash(evalOnly);

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.Equal(1, dataset.Value!.Manifest.AcceptedRecordCount);
        Assert.Equal(1, dataset.Value.Manifest.ExcludedRecordCount);
        Assert.DoesNotContain(dataset.Value.Rows, row => row.RecordIdentityHash == evalOnlyIdentityHash);
        Assert.Contains(dataset.Value.Manifest.ExcludedRecords, excluded =>
            excluded.RecordIdentityHash == evalOnlyIdentityHash &&
            excluded.Reason == "not_trainable_for_dataset");
        Assert.Contains("eval_only_legal_verdict",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(evalOnly));
        Assert.True(batches.IsSuccess, batches.ErrorMessage);
        Assert.Equal([accepted.RecordId],
            batches.Value!
                .SelectMany(batch => batch.Records)
                .Select(record => record.RecordId)
                .ToArray());
    }

    [Fact]
    public void EvaluateRecords_ReportsExplicitTrainingMeasurementGateBlockReasons()
    {
        var valid = BuildTrainingRecord("approval-missing", DesignConversionDpoUnits.CriticUnit, "blocked-source-a");
        var providerMissing = BuildTrainingRecord("provider-missing", DesignConversionDpoUnits.CriticUnit, "blocked-source-b") with
        {
            Provenance = valid.Provenance with
            {
                SourceManifestRef = "manifest://dcm-source/blocked-source-b",
                Metadata = new Dictionary<string, string>
                {
                    ["visual_judge_status"] = DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
                    ["provider_boundary"] = DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker
                }
            }
        };
        var quarantined = BuildTrainingRecord("quarantined", DesignConversionDpoUnits.ImproverUnit, "blocked-source-c") with
        {
            Quarantine = new DesignConversionQuarantineState(
                IsQuarantined: true,
                Reason: "manual_quarantine",
                EvidenceRefs: ["manual-quarantine:evidence"])
        };
        var leaky = BuildTrainingRecord("leaky", DesignConversionDpoUnits.CriticUnit, "blocked-source-d") with
        {
            SourceStructurePayload = "{\"source_request_id_as_feature\":\"request-123\"}"
        };
        var negativeMissing = BuildTrainingRecord("negative-missing", DesignConversionDpoUnits.ImproverUnit, "blocked-source-e") with
        {
            NegativeQualityReason = ""
        };
        var duplicateA = BuildTrainingRecord("replay-duplicate", DesignConversionDpoUnits.CriticUnit, "blocked-source-f");
        var duplicateB = duplicateA with
        {
            Provenance = duplicateA.Provenance with
            {
                SourceManifestRef = "manifest://dcm-source/blocked-source-g",
                Metadata = new Dictionary<string, string>
                {
                    ["source_design_id"] = "blocked-source-g"
                }
            }
        };

        var result = new DesignConversionTrainingMeasurementGate().EvaluateRecords(
            [valid, providerMissing, quarantined, leaky, negativeMissing, duplicateA, duplicateB],
            new DesignConversionTrainingPipelineOptions(
                "pass82-blocked",
                BatchSize: 2,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    DatasetId: "pass82-blocked",
                    MinimumTrainRecords: 1,
                    MinimumHeldOutRecords: 1,
                    Preflight: DesignConversionDpoDatasetPreflight.MissingApproval()),
                MeasurementGateRequest: BuildNonDisjointMissingHeldOutMeasurement()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingMeasurementGateStatus.Blocked, result.Value!.Status);
        Assert.False(result.Value.ReadyForTraining);
        Assert.True(result.Value.NotRealModelReady);
        Assert.Null(result.Value.TrainingBatchArtifact);
        Assert.Contains("approval_missing", result.Value.Reasons);
        Assert.Contains("provider_missing", result.Value.Reasons);
        Assert.Contains("quarantine_blocked", result.Value.Reasons);
        Assert.Contains("leakage", result.Value.Reasons);
        Assert.Contains("source_not_disjoint", result.Value.Reasons);
        Assert.Contains("heldout_missing", result.Value.Reasons);
        Assert.Contains("negative_quality_missing", result.Value.Reasons);
        Assert.Contains("replay_duplicate", result.Value.Reasons);
        Assert.Contains("provider_missing_blocked", result.Value.EvidenceRefs);
        Assert.Contains("quarantine_blocked", result.Value.EvidenceRefs);
        Assert.Contains("negative_quality_missing", result.Value.EvidenceRefs);
        Assert.Contains("training_measurement_gate", result.Value.EvidenceRefs);
        Assert.Contains("source_disjoint", result.Value.EvidenceRefs);
        Assert.Contains("heldout_required", result.Value.EvidenceRefs);
        Assert.Contains("not_real_model_ready", result.Value.EvidenceRefs);
        Assert.Contains("promotion_blocked", result.Value.EvidenceRefs);
        Assert.Contains("product_not_ready", result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task EvaluateDpoStorageAsync_ConsumesDpoStorageAndBlocksProviderMissingRows()
    {
        var storage = new InMemoryDesignConversionDpoStorage();
        await Save(storage, BuildExample("storage-allowed", DesignConversionDpoUnits.CriticUnit, "storage-source-a"));
        await Save(storage, BuildExample("storage-provider-missing", DesignConversionDpoUnits.ImproverUnit, "storage-source-b") with
        {
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "request:storage-source-b",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://dcm-source/storage-source-b",
                ReviewerRef: "teacher:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["source_design_id"] = "storage-source-b",
                    ["visual_judge_status"] = DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                })
        });

        var result = await new DesignConversionTrainingMeasurementGate().EvaluateDpoStorageAsync(
            storage,
            LocalOptions("pass82-dpo-storage"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains("provider_missing", result.Value!.Reasons);
        Assert.Contains("provider_missing_blocked", result.Value.EvidenceRefs);
        Assert.Contains(result.Value.CandidateDecisions, decision =>
            decision.RecordId == "storage-provider-missing" &&
            decision.Reasons.Contains("provider_missing"));
    }

    [Fact]
    public async Task EvaluateLocalArtifactDbAsync_AllowsApprovedFixtureRowsOnlyAsTrainingCandidateArtifact()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "local_artifact_db");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var trainCandidate = BuildTrainingRecord("fixture-train", DesignConversionDpoUnits.CriticUnit, "fixture-source-a");
        var heldOutCandidate = BuildTrainingRecord("fixture-heldout", DesignConversionDpoUnits.CriticUnit, "fixture-source-b");
        await DesignConversionPostEvaluationEligibilityTestFixture
            .AppendEligibleRecordAsync(
                db,
                BuildArtifactRecord(
                    "fixture-storage-record",
                    [trainCandidate, heldOutCandidate]));

        var result = await new DesignConversionTrainingMeasurementGate().EvaluateLocalArtifactDbAsync(
            db,
            null,
            LocalOptions("pass82-local-fixture"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingMeasurementGateStatus.TrainingCandidateOnly, result.Value!.Status);
        Assert.False(result.Value.ReadyForTraining);
        Assert.True(result.Value.TrainingCandidateOnly);
        Assert.True(result.Value.NotRealModelReady);
        var artifact = Assert.IsType<DesignConversionTrainingBatchArtifact>(result.Value.TrainingBatchArtifact);
        Assert.True(artifact.TrainingCandidateOnly);
        Assert.True(artifact.NotRealModelReady);
        var batch = Assert.Single(artifact.Batches);
        var record = Assert.Single(batch.Records);
        Assert.Equal("fixture-train", record.RecordId);
        Assert.DoesNotContain("fixture-heldout", batch.Records.Select(candidate => candidate.RecordId));
        Assert.Contains("training_candidate_only", result.Value.EvidenceRefs);
        Assert.Contains("not_real_model_ready", result.Value.EvidenceRefs);
        Assert.Contains("heldout_required", result.Value.EvidenceRefs);
        Assert.Contains("promotion_blocked", result.Value.EvidenceRefs);
        Assert.Contains("product_not_ready", result.Value.EvidenceRefs);
        Assert.Contains("ready_for_training:false", result.Value.EvidenceRefs);
    }

    [Fact]
    public void EvaluateRecords_ReportsFailingHeldOutQualityGateWithScoreDeltaAndReason()
    {
        var trainCandidate = BuildTrainingRecord("quality-fail-train", DesignConversionDpoUnits.CriticUnit, "quality-fail-source-a");
        var heldOutCandidate = BuildTrainingRecord("quality-fail-heldout", DesignConversionDpoUnits.CriticUnit, "quality-fail-source-b");

        var result = new DesignConversionTrainingMeasurementGate().EvaluateRecords(
            [trainCandidate, heldOutCandidate],
            LocalOptions(
                "quality-fail-local-fixture",
                BuildHeldOutMeasurement(heldOutQualityScore: 0.81)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingMeasurementGateStatus.TrainingCandidateOnly, result.Value!.Status);
        Assert.False(result.Value.ReadyForTraining);
        Assert.False(result.Value.PassesGate);
        Assert.Equal(0.81, result.Value.HeldOutQualityScore!.Value, precision: 2);
        Assert.Equal(0.90, result.Value.RequiredHeldOutQualityScore!.Value, precision: 2);
        Assert.Equal(-0.09, result.Value.ScoreDelta!.Value, precision: 2);
        Assert.True(result.Value.SourceDisjoint == true);
        Assert.True(result.Value.AblationPassed == true);
        Assert.True(result.Value.BudgetPassed == true);
        Assert.Contains("HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD:0.81<0.9", result.Value.GateReason);
        Assert.Contains("heldout_required", result.Value.Reasons);
        Assert.Contains("passesQualityGate:false", result.Value.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED", result.Value.EvidenceRefs);
    }

    [Fact]
    public void EvaluateRecords_ReportsPassingSourceDisjointHeldOutGateWithoutProductReadyClaim()
    {
        var trainCandidate = BuildTrainingRecord("quality-pass-train", DesignConversionDpoUnits.CriticUnit, "quality-pass-source-a");
        var heldOutCandidate = BuildTrainingRecord("quality-pass-heldout", DesignConversionDpoUnits.CriticUnit, "quality-pass-source-b");

        var result = new DesignConversionTrainingMeasurementGate().EvaluateRecords(
            [trainCandidate, heldOutCandidate],
            LocalOptions(
                "quality-pass-local-fixture",
                BuildHeldOutMeasurement(heldOutQualityScore: 0.94)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionTrainingMeasurementGateStatus.TrainingCandidateOnly, result.Value!.Status);
        Assert.False(result.Value.ReadyForTraining);
        Assert.True(result.Value.TrainingCandidateOnly);
        Assert.True(result.Value.NotRealModelReady);
        Assert.False(result.Value.DatasetManifest.Preflight.ProductReady);
        Assert.True(result.Value.PassesGate, result.Value.GateReason);
        Assert.Equal(0.94, result.Value.HeldOutQualityScore!.Value, precision: 2);
        Assert.Equal(0.90, result.Value.RequiredHeldOutQualityScore!.Value, precision: 2);
        Assert.Equal(0.04, result.Value.ScoreDelta!.Value, precision: 2);
        Assert.True(result.Value.SourceDisjoint == true);
        Assert.True(result.Value.AblationPassed == true);
        Assert.True(result.Value.BudgetPassed == true);
        Assert.Equal("ALL_MEASUREMENT_GATES_PASSED", result.Value.GateReason);
        Assert.Equal(1, result.Value.DatasetCountGate.ActualTrainRecords);
        Assert.Equal(1, result.Value.DatasetCountGate.ActualHeldOutRecords);
        Assert.True(result.Value.DatasetCountGate.SourceDisjoint);
        var trainSource = Assert.Single(result.Value.DatasetManifest.TrainSources);
        var heldOutSource = Assert.Single(result.Value.DatasetManifest.HeldOutSources);
        Assert.NotEqual(trainSource.SourceIdentityHash, heldOutSource.SourceIdentityHash);
        Assert.Contains("passesQualityGate:true", result.Value.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED", result.Value.EvidenceRefs);
        Assert.Contains("product_not_ready", result.Value.EvidenceRefs);
        Assert.Contains("promotion_blocked", result.Value.EvidenceRefs);
        Assert.Contains("not_real_model_ready", result.Value.EvidenceRefs);
    }

    private static DesignConversionTrainingPipelineOptions LocalOptions(string runId)
        => new(
            runId,
            BatchSize: 2,
            RequireDatasetCountGate: false,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: runId,
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly()));

    private static DesignConversionTrainingPipelineOptions LocalOptions(
        string runId,
        DesignConversionMeasurementGateRequest measurementGateRequest)
        => new(
            runId,
            BatchSize: 2,
            RequireDatasetCountGate: false,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: runId,
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly()),
            MeasurementGateRequest: measurementGateRequest);

    private static DesignConversionMeasurementGateRequest BuildHeldOutMeasurement(double heldOutQualityScore)
        => new(
            GateId: "design-conversion-training-heldout-quality-gate",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 1,
                HeldOutQualityScore: heldOutQualityScore,
                RuntimeMs: 320,
                CostUsd: 0.25m,
                PeakRamMb: 256,
                EvidenceRefs:
                ["artifact://caller-measurement/focused/heldout.json"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs:
                ["artifact://caller-measurement/focused/integrity.json"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionMeasurementGateRequest BuildNonDisjointMissingHeldOutMeasurement()
        => new(
            GateId: "pass82-non-disjoint-heldout-missing",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 0,
                HeldOutQualityScore: null,
                RuntimeMs: null,
                CostUsd: null,
                PeakRamMb: null,
                EvidenceRefs: []),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: false,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: false,
                EvidenceRefs: []),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        string sourceId)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords([BuildExample(id, unitId, sourceId)]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        return Assert.Single(build.Value.AcceptedRecords);
    }

    private static DesignConversionDpoTrainingRecord
        WithApprovedRuntimeCandidateBoundary(
            DesignConversionDpoTrainingRecord record,
            string marker,
            string markerValue)
    {
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["approval_status"] = "approved",
            ["approved_for_training"] = bool.TrueString,
            ["training_candidate_only"] = bool.TrueString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["production_dpo"] = bool.FalseString,
            [marker] = markerValue
        };

        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata }
        };
    }

    private static DesignConversionDpoTrainingRecord WithLegalVerdictMetadata(
        DesignConversionDpoTrainingRecord record,
        string surface,
        string key,
        string value)
    {
        if (string.Equals(surface, "provenance", StringComparison.Ordinal))
        {
            var metadata = new Dictionary<string, string>(
                record.Provenance.Metadata,
                StringComparer.Ordinal)
            {
                [key] = value
            };

            return record with
            {
                Provenance = record.Provenance with { Metadata = metadata }
            };
        }

        if (string.Equals(surface, "license", StringComparison.Ordinal))
        {
            var metadata = new Dictionary<string, string>(
                record.License.Metadata,
                StringComparer.Ordinal)
            {
                [key] = value
            };

            return record with
            {
                License = record.License with { Metadata = metadata }
            };
        }

        throw new ArgumentOutOfRangeException(nameof(surface), surface, "Unknown metadata surface.");
    }

    private static void AssertAllowedLegalVerdictControls()
    {
        var accept = WithLegalVerdictMetadata(
            BuildTrainingRecord(
                "legal-verdict-accept-control",
                DesignConversionDpoUnits.CriticUnit,
                "legal-verdict-accept-control-source"),
            "provenance",
            "legal_verdict",
            "ACCEPT");
        var evalOnlyFalse = WithLegalVerdictMetadata(
            BuildTrainingRecord(
                "legal-verdict-eval-only-false-control",
                DesignConversionDpoUnits.CriticUnit,
                "legal-verdict-eval-only-false-control-source"),
            "license",
            "eval_only",
            bool.FalseString);
        var unrelatedEvalKey = WithLegalVerdictMetadata(
            BuildTrainingRecord(
                "legal-verdict-unrelated-eval-control",
                DesignConversionDpoUnits.CriticUnit,
                "legal-verdict-unrelated-eval-control-source"),
            "provenance",
            "evaluation_scope",
            "EVAL_ONLY");
        var dpoTrainReadyFalse = WithLegalVerdictMetadata(
            BuildTrainingRecord(
                "legal-verdict-dpo-train-ready-false-control",
                DesignConversionDpoUnits.CriticUnit,
                "legal-verdict-dpo-train-ready-false-control-source"),
            "license",
            "dpoTrainReady",
            bool.FalseString);

        Assert.Empty(DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(accept));
        Assert.Empty(DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(evalOnlyFalse));
        Assert.Empty(DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(unrelatedEvalKey));
        Assert.Empty(DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(dpoTrainReadyFalse));
    }

    private static DesignConversionStoredDpoExample BuildExample(
        string id,
        string unitId,
        string sourceId)
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(id, unitId, usefulness: 0.88);
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
    }

    private static DesignConversionLocalArtifactDbRecord BuildArtifactRecord(
        string recordId,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records)
        => new(
            RecordId: recordId,
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: "pass82-job",
            RequestId: "pass82-request",
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "local-fixture",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "manifest://dcm-source/pass82-fixture"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion",
                HtmlRef: "sha256:fixture-html",
                HtmlSha256: "sha256:fixture-html",
                CssRef: "sha256:fixture-css",
                CssSha256: "sha256:fixture-css",
                Metadata: new Dictionary<string, string>()),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render",
                    Kind: "browser_screenshot",
                    UriRef: "artifact://pass82/render-desktop.png",
                    Sha256: "sha256:fixture-render",
                    ByteLength: 1024,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>())
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ProviderBoundary: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: 0.01m,
                TierDecision: DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker],
                Metadata: new Dictionary<string, string>()),
            DpoCandidates: records
                .Select(record => DesignConversionLocalArtifactDbDpoCandidate.FromTrainingRecord(record))
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "training_measurement_gate",
                "local_artifact_db",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>());

    private static async Task Save(IDesignConversionDpoStorage storage, DesignConversionStoredDpoExample example)
    {
        var save = await storage.SaveExampleAsync(example);
        Assert.True(save.IsSuccess, save.ErrorMessage);
    }
}
