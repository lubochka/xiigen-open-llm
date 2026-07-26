// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using DesignConversionDomain;

public sealed class DesignConversionDpoUsefulnessApiTests
{
    [Fact]
    public async Task QueryAsync_ReturnsUserVisibleUsefulnessRowsWithStatusBucketsAndNoRawPayloads()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-usefulness-api");
        var db = new JsonlDesignConversionLocalArtifactDb(root);

        await Save(db, BuildArtifactRecord(
            "record-train-ready",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("train-ready", DesignConversionDpoUnits.CriticUnit, 0.91)]));
        await Save(db, BuildArtifactRecord(
            "record-heldout",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("heldout-row", DesignConversionDpoUnits.CriticUnit, 0.89, split: "held_out")]));
        await Save(db, BuildArtifactRecord(
            "record-provider",
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            [BuildTrainingRecord("provider-missing", DesignConversionDpoUnits.CriticUnit, 0.97)]));
        await Save(db, BuildArtifactRecord(
            "record-quarantine",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("quarantined", DesignConversionDpoUnits.ImproverUnit, 0.72) with
            {
                Quarantine = new DesignConversionQuarantineState(
                    IsQuarantined: true,
                    Reason: "manual_quarantine",
                    EvidenceRefs: ["quarantine:evidence"])
            }]));
        await Save(db, BuildArtifactRecord(
            "record-negative-quality",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("negative-quality", DesignConversionDpoUnits.CriticUnit, 0.66) with
            {
                NegativeQualityReason = "",
                NegativeReasonTaxonomy = DesignConversionNegativeReasonTaxonomy.TrivialBroken,
                Quarantine = new DesignConversionQuarantineState(
                    IsQuarantined: true,
                    Reason: "negative_quality_reason_required;negative_reason_taxonomy_quarantine_only:trivial_broken",
                    EvidenceRefs: ["design-conversion-dpo-validation:negative_quality_reason_required"])
            }]));

        var response = await new DesignConversionDpoUsefulnessApi(db).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApiShape, response.Value!.ApiShape);
        Assert.False(response.Value.ProductReady);
        Assert.Equal(5, response.Value.Rows.Count);
        Assert.Equal(5, response.Value.RowCount);
        Assert.Equal(1, response.Value.TrainReadyCount);
        Assert.Equal(2, response.Value.QuarantinedCount);
        Assert.Equal(1, response.Value.ProviderMissingCount);
        Assert.Equal(1, response.Value.RejectedNegativeQualityCount);
        Assert.Equal(1, response.Value.HeldOutCount);
        Assert.Equal(3, response.Value.BlockedCount);
        Assert.Equal(0.83, response.Value.AverageUsefulnessScore.GetValueOrDefault());
        Assert.Equal(0.66, response.Value.MinimumUsefulnessScore.GetValueOrDefault());
        Assert.Equal(0.97, response.Value.MaximumUsefulnessScore.GetValueOrDefault());
        Assert.Contains("raw_html_css_tree_private_provider_payloads_not_emitted", response.Value.Warnings);
        Assert.Equal("5", response.Value.BoundaryMetadata["row_count"]);
        Assert.Equal("0.83", response.Value.BoundaryMetadata["average_usefulness_score"]);
        Assert.Equal("0.66", response.Value.BoundaryMetadata["minimum_usefulness_score"]);
        Assert.Equal("0.97", response.Value.BoundaryMetadata["maximum_usefulness_score"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["provider_payload_included"]);
        Assert.Contains("retrain_decision_surface_refs_only", response.Value.Warnings);
        Assert.Equal("row_status_plus_refs_only", response.Value.BoundaryMetadata["retrain_decision_surface"]);

        Assert.All(response.Value.Rows, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(row.RetrainDecisionStatus));
            Assert.NotEmpty(row.RetrainDecisionEvidenceRefs);
            Assert.Equal(row.RetrainDecisionStatus, row.BoundaryMetadata["api_retrain_decision_status"]);
            Assert.Equal("row_status_plus_refs_only", row.BoundaryMetadata["retrain_decision_surface"]);
            Assert.Contains(
                $"design-conversion-dpo-retrain-decision:review_status:{row.ReviewStatus}",
                row.RetrainDecisionEvidenceRefs);
            Assert.Contains(
                $"design-conversion-dpo-retrain-decision:usefulness_status:{row.UsefulnessStatus}",
                row.RetrainDecisionEvidenceRefs);
        });

        var trainReadyRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "train-ready");
        Assert.True(trainReadyRow.TrainReady);
        Assert.Equal("high_usefulness", trainReadyRow.UsefulnessStatus);
        Assert.False(trainReadyRow.ProviderMissing);
        Assert.Equal("ready-for-retrain-intake", trainReadyRow.RetrainDecisionStatus);
        Assert.Contains("design-conversion-dpo-retrain-decision:status:ready-for-retrain-intake", trainReadyRow.RetrainDecisionEvidenceRefs);
        Assert.Contains("design-conversion-dpo-retrain-decision:train_ready:true", trainReadyRow.RetrainDecisionEvidenceRefs);
        Assert.Contains("design-conversion-dpo-display:training_candidate_only", trainReadyRow.RetrainDecisionEvidenceRefs);

        var heldOutRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "heldout");
        Assert.True(heldOutRow.HeldOut);
        Assert.Equal("held_out", heldOutRow.BoundaryMetadata["split"]);
        Assert.Equal("holdout-evidence-not-training-input", heldOutRow.RetrainDecisionStatus);

        var providerMissingRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "provider-missing");
        Assert.True(providerMissingRow.ProviderMissing);
        Assert.Contains("provider_missing_or_blocked", providerMissingRow.BlockReasons);
        Assert.Equal("blocked-provider-evidence-required", providerMissingRow.RetrainDecisionStatus);

        var quarantinedRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "quarantined");
        Assert.True(quarantinedRow.Quarantined);
        Assert.Contains("manual_quarantine", quarantinedRow.BlockReasons);
        Assert.Equal("blocked-quarantined", quarantinedRow.RetrainDecisionStatus);

        var rejectedNegativeQualityRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "rejected-negative-quality");
        Assert.True(rejectedNegativeQualityRow.RejectedNegativeQuality);
        Assert.Contains(rejectedNegativeQualityRow.BlockReasons, reason => reason.Contains("negative_quality_reason_required", StringComparison.Ordinal));
        Assert.Equal("blocked-negative-quality", rejectedNegativeQualityRow.RetrainDecisionStatus);

        var serialized = JsonSerializer.Serialize(response.Value);
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("candidate_order", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QueryAsyncAndLocalReadbackExposeSourceEncoderIndicatorsWithoutRawPayloads()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-source-encoder-readback-api");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        await Save(db, BuildArtifactRecord(
            "record-source-encoder",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("source-encoder-readback", DesignConversionDpoUnits.ImproverUnit, 0.94)]));
        var api = new DesignConversionDpoUsefulnessApi(db);

        var usefulness = await api.QueryAsync(new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        var readback = await api.QueryLocalArtifactReadbackAsync(
            new DesignConversionLocalArtifactReadbackApiRequest(JobId: "record-source-encoder:job"));

        Assert.True(usefulness.IsSuccess, usefulness.ErrorMessage);
        var usefulnessRow = Assert.Single(usefulness.Value!.Rows);
        Assert.StartsWith("dcm-source-encoder-vector-", usefulnessRow.BoundaryMetadata["source_encoder_vector_hash"], StringComparison.Ordinal);
        Assert.True(int.Parse(usefulnessRow.BoundaryMetadata["source_encoder_dimensions"]) > 0);
        Assert.Contains("source_json_node_count", usefulnessRow.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.DoesNotContain("visual_score_margin", usefulnessRow.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.DoesNotContain("chosen_css_rule_count", usefulnessRow.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.Equal("source_kind_source_id_not_vector_features", usefulnessRow.BoundaryMetadata["source_encoder_shortcut_guard"]);
        Assert.Equal(bool.FalseString, usefulnessRow.BoundaryMetadata["source_kind_in_vector"]);
        Assert.Equal(bool.FalseString, usefulnessRow.BoundaryMetadata["source_id_in_vector"]);

        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        var readbackRow = Assert.Single(readback.Value!.Rows);
        Assert.NotEqual("missing", readbackRow.BoundaryMetadata["dpo_source_encoder_vector_hashes"]);
        Assert.NotEqual("missing", readbackRow.BoundaryMetadata["dpo_internal_vector_dimension_count"]);
        Assert.Contains("source_json_node_count", readbackRow.BoundaryMetadata["dpo_source_encoder_feature_names"]);
        Assert.DoesNotContain("visual_score_margin", readbackRow.BoundaryMetadata["dpo_source_encoder_feature_names"]);
        Assert.DoesNotContain("chosen_css_rule_count", readbackRow.BoundaryMetadata["dpo_source_encoder_feature_names"]);
        Assert.Contains("visual_score_margin=", readbackRow.BoundaryMetadata["dpo_result_signal_summary"]);
        Assert.Equal("source_kind_source_id_not_vector_features", readbackRow.BoundaryMetadata["dpo_source_encoder_shortcut_guard"]);
        Assert.Equal(bool.FalseString, readbackRow.BoundaryMetadata["dpo_source_kind_in_vector"]);
        Assert.Equal(bool.FalseString, readbackRow.BoundaryMetadata["dpo_source_id_in_vector"]);

        var serialized = JsonSerializer.Serialize(new { Usefulness = usefulness.Value, Readback = readback.Value });
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt private", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QueryAsync_FiltersRowsThroughExistingDisplayQuery()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-usefulness-api-filter");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        await Save(db, BuildArtifactRecord(
            "record-high",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("high", DesignConversionDpoUnits.CriticUnit, 0.93)]));
        await Save(db, BuildArtifactRecord(
            "record-low",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("low", DesignConversionDpoUnits.CriticUnit, 0.31)]));

        var response = await new DesignConversionDpoUsefulnessApi(db).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(
                UnitId: DesignConversionDpoUnits.CriticUnit,
                IncludeBlocked: false,
                MinimumUsefulnessScore: 0.5,
                Limit: 1));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(1, response.Value!.RowCount);
        Assert.Equal(0.93, response.Value.AverageUsefulnessScore.GetValueOrDefault());
        Assert.Equal(0.93, response.Value.MinimumUsefulnessScore.GetValueOrDefault());
        Assert.Equal(0.93, response.Value.MaximumUsefulnessScore.GetValueOrDefault());
        var row = Assert.Single(response.Value!.Rows);
        Assert.Equal("train-ready", row.ReviewStatus);
        Assert.Equal("high_usefulness", row.UsefulnessStatus);
        Assert.True(row.TrainReady);
    }

    [Fact]
    public async Task QueryAsync_ExposesUsefulnessMetricsForTrainingRecordWithProductReadyFalseBoundary()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-usefulness-api-metrics");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        await Save(db, BuildArtifactRecord(
            "record-metrics",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("metrics-row", DesignConversionDpoUnits.CriticUnit, 0.88)]));

        var response = await new DesignConversionDpoUsefulnessApi(db).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.False(response.Value!.ProductReady);
        var row = Assert.Single(response.Value.Rows);
        Assert.Equal(0.88, row.UsefulnessScore);
        Assert.Equal(0.88, row.UsefulnessMetrics.Score);
        Assert.Equal(0.57, row.UsefulnessMetrics.QualityGain);
        Assert.Equal(DesignConversionNegativeReasonTaxonomy.Spacing, row.UsefulnessMetrics.NegativePairUsefulness);
        Assert.Contains("source_kind=Figma", row.UsefulnessMetrics.SourceCoverage);
        Assert.Contains("source_ref=present", row.UsefulnessMetrics.SourceCoverage);
        Assert.Contains("heldout_quality=not_proven", row.UsefulnessMetrics.SourceCoverage);
        Assert.Equal("not_proven", row.UsefulnessMetrics.SourceDisjointStatus);
        Assert.Equal("not_proven", row.UsefulnessMetrics.HeldOutEvaluationStatus);
        Assert.Equal(
            "retrain_intake_eligible;local_promotion_requires_training_run_measurement_gate_and_policy_checkpoint",
            row.UsefulnessMetrics.PromotionEligibility);
        Assert.Equal(new[] { "policy_checkpoint_evidence:not_present" }, row.UsefulnessMetrics.PolicyCheckpointEvidenceRefs);
        Assert.False(row.UsefulnessMetrics.ProductReady);
        Assert.True(row.ProductNotReady);
        Assert.Equal("product_ready=false", row.UsefulnessMetrics.ProductReadyBoundary);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["api_product_ready"]);
    }

    private static async Task Save(
        IDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord record)
    {
        var result = await db.AppendRecordAsync(record);
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        double usefulness,
        string split = "train")
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(id, unitId, usefulness) with
        {
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "design-request-redacted",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://design-conversion/source-redacted",
                ReviewerRef: "teacher:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["split"] = split,
                    ["source_quality"] = "teacher-reviewed"
                })
        };
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords([example]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        return build.Value!.AllRecords.Single();
    }

    private static DesignConversionLocalArtifactDbRecord BuildArtifactRecord(
        string recordId,
        string visualJudgeStatus,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records)
        => new(
            RecordId: recordId,
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: $"{recordId}:job",
            RequestId: $"{recordId}:request",
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "pro",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "file:///C:/Users/private-user/Documents/private.fig",
                ["source_kind"] = "figma"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion-result",
                HtmlRef: "<main data-user-id=\"luba\">exact_prompt private html</main>",
                HtmlSha256: string.Empty,
                CssRef: ".private{background:url('C:/Users/private-user/private.png');}",
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>()),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render:desktop",
                    Kind: "browser_screenshot",
                    UriRef: "file:///C:/Users/private-user/private/render-desktop.png",
                    Sha256: "sha256:render-desktop",
                    ByteLength: 1234,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>())
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: visualJudgeStatus,
                ProviderBoundary: visualJudgeStatus == DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: null,
                TierDecision: visualJudgeStatus == DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [visualJudgeStatus, "visual_judge_status", "product_not_ready"],
                Metadata: new Dictionary<string, string>
                {
                    ["visual_judge_status"] = visualJudgeStatus
                }),
            DpoCandidates: records
                .Select(record => DesignConversionLocalArtifactDbDpoCandidate.FromTrainingRecord(record))
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "storage_training_artifact_db",
                "local_artifact_db",
                "conversion_result",
                "render_artifact_refs",
                "visual_judge_status",
                "dpo_candidate",
                "usefulness_score",
                "quarantine",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>());
}
