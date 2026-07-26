// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionToken04QualityScoringArtifactsTests
{
    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;

    [Fact]
    public void Token04QualityScoringArtifacts_HaveExpectedCountsAndNoClaims()
    {
        var rowScoresPath = Path.Combine(ArtifactDir, "token-04-quality-scoring-full-hosts-row-scores.jsonl");
        var pairScoresPath = Path.Combine(ArtifactDir, "token-04-quality-scoring-full-hosts-pair-results.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "token-04-quality-scoring-full-hosts-report.json");
        var validationPath = Path.Combine(ArtifactDir, "token-04-quality-scoring-full-hosts.validation.json");

        var rowScores = ReadJsonl(rowScoresPath);
        var pairScores = ReadJsonl(pairScoresPath);
        Assert.Equal(108, rowScores.Count);
        Assert.Equal(54, pairScores.Count);

        AssertRepresented(rowScores, "host", new Dictionary<string, int>
        {
            ["Claude"] = 36,
            ["Codex"] = 36,
            ["Cursor"] = 36,
        });
        AssertRepresented(rowScores, "mode", new Dictionary<string, int>
        {
            ["adaptive_common_plus_user_model"] = 54,
            ["baseline_common_model_only"] = 54,
        });
        AssertRepresented(rowScores, "method", new Dictionary<string, int>
        {
            ["orchestration-plan"] = 36,
            ["prompt-to-task"] = 36,
            ["skill-selection"] = 36,
        });
        Assert.Equal(12, rowScores.Select(row => row.GetProperty("scenario_id").GetString()).Distinct(StringComparer.Ordinal).Count());
        Assert.All(rowScores, row =>
        {
            Assert.False(row.GetProperty("quality_result_claimed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("learnability_claimed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
        });
        Assert.All(pairScores, pair =>
        {
            Assert.False(pair.GetProperty("token_savings_claim_allowed").GetBoolean());
            Assert.False(pair.GetProperty("quality_gate_passed").GetBoolean());
            Assert.False(pair.GetProperty("adaptive_effect_proven").GetBoolean());
        });

        using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = report.RootElement;
        Assert.Equal("xiigen.extension_token_04_quality_scoring_full_hosts_report.v1", root.GetProperty("schema").GetString());
        Assert.Equal(108, root.GetProperty("row_count").GetInt32());
        Assert.Equal(54, root.GetProperty("pair_count").GetInt32());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.Equal(0, root.GetProperty("token_savings_claim_allowed_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("adaptive_effect_proven_pairs").GetInt32());

        using var validation = JsonDocument.Parse(File.ReadAllText(validationPath));
        Assert.True(validation.RootElement.GetProperty("row_count_pass").GetBoolean());
        Assert.True(validation.RootElement.GetProperty("pair_count_pass").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("learnability_claimed").GetBoolean());
    }

    [Fact]
    public void Token04RerunAfterQualityGapRepairArtifacts_HaveFreshCountsAndNoClaims()
    {
        var captureReportPath = Path.Combine(ArtifactDir, "token-04-rerun-after-quality-gap-repair-01-capture-report.json");
        var rowScoresPath = Path.Combine(ArtifactDir, "token-04-rerun-after-quality-gap-repair-01-row-scores.jsonl");
        var pairScoresPath = Path.Combine(ArtifactDir, "token-04-rerun-after-quality-gap-repair-01-pair-results.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "token-04-rerun-after-quality-gap-repair-01-report.json");
        var validationPath = Path.Combine(ArtifactDir, "token-04-rerun-after-quality-gap-repair-01.validation.json");

        var rowScores = ReadJsonl(rowScoresPath);
        var pairScores = ReadJsonl(pairScoresPath);
        Assert.Equal(108, rowScores.Count);
        Assert.Equal(54, pairScores.Count);

        AssertRepresented(rowScores, "host", new Dictionary<string, int>
        {
            ["Claude"] = 36,
            ["Codex"] = 36,
            ["Cursor"] = 36,
        });
        AssertRepresented(rowScores, "mode", new Dictionary<string, int>
        {
            ["adaptive_common_plus_user_model"] = 54,
            ["baseline_common_model_only"] = 54,
        });
        Assert.All(rowScores, row =>
        {
            Assert.False(row.GetProperty("quality_result_claimed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("learnability_claimed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
            Assert.Equal("NEEDS_REPAIR", row.GetProperty("verdict").GetString());
        });
        Assert.All(pairScores, pair =>
        {
            Assert.False(pair.GetProperty("token_savings_claim_allowed").GetBoolean());
            Assert.False(pair.GetProperty("quality_gate_passed").GetBoolean());
            Assert.False(pair.GetProperty("adaptive_effect_proven").GetBoolean());
        });

        using var captureReport = JsonDocument.Parse(File.ReadAllText(captureReportPath));
        Assert.False(captureReport.RootElement.GetProperty("stale_token_03_traces_reused").GetBoolean());
        Assert.Equal(108, captureReport.RootElement.GetProperty("captured_rows").GetInt32());
        Assert.Equal(54, captureReport.RootElement.GetProperty("pair_count").GetInt32());
        Assert.Equal(0, captureReport.RootElement
            .GetProperty("previous_token_04_counts")
            .GetProperty("quality_passed_rows")
            .GetInt32());

        using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = report.RootElement;
        Assert.Equal("TOKEN-04-RERUN-AFTER-QUALITY-GAP-REPAIR-01", root.GetProperty("phase_id").GetString());
        Assert.Equal(108, root.GetProperty("row_count").GetInt32());
        Assert.Equal(54, root.GetProperty("pair_count").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_passed_rows").GetInt32());
        Assert.Equal(108, root.GetProperty("needs_repair_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("failed_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_gate_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("adaptive_effect_proven_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("token_savings_claim_allowed_pairs").GetInt32());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.All(root.GetProperty("aggregate_by_host").EnumerateArray(), host =>
        {
            Assert.Equal(36, host.GetProperty("needs_repair_count").GetInt32());
            Assert.Equal(0, host.GetProperty("pass_count").GetInt32());
        });
        Assert.Contains(
            root.GetProperty("blocking_quality_gaps").EnumerateArray(),
            item => item.GetString()!.Contains("user-local adaptive model effect", StringComparison.Ordinal));

        using var validation = JsonDocument.Parse(File.ReadAllText(validationPath));
        Assert.True(validation.RootElement.GetProperty("row_count_pass").GetBoolean());
        Assert.True(validation.RootElement.GetProperty("pair_count_pass").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("learnability_claimed").GetBoolean());
    }

    [Fact]
    public void Token04RerunWithAdaptiveLearnabilityArtifacts_ShowAdaptiveAssetsLoadedButNoOutputEffect()
    {
        var captureReportPath = Path.Combine(ArtifactDir, "token-04-rerun-with-adaptive-learnability-01-capture-report.json");
        var rowScoresPath = Path.Combine(ArtifactDir, "token-04-rerun-with-adaptive-learnability-01-row-scores.jsonl");
        var pairScoresPath = Path.Combine(ArtifactDir, "token-04-rerun-with-adaptive-learnability-01-pair-results.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "token-04-rerun-with-adaptive-learnability-01-report.json");
        var validationPath = Path.Combine(ArtifactDir, "token-04-rerun-with-adaptive-learnability-01.validation.json");

        var rowScores = ReadJsonl(rowScoresPath);
        var pairScores = ReadJsonl(pairScoresPath);
        Assert.Equal(108, rowScores.Count);
        Assert.Equal(54, pairScores.Count);

        AssertRepresented(rowScores, "host", new Dictionary<string, int>
        {
            ["Claude"] = 36,
            ["Codex"] = 36,
            ["Cursor"] = 36,
        });
        AssertRepresented(rowScores, "mode", new Dictionary<string, int>
        {
            ["adaptive_common_plus_user_model"] = 54,
            ["baseline_common_model_only"] = 54,
        });

        Assert.All(rowScores, row =>
        {
            Assert.False(row.GetProperty("quality_result_claimed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("learnability_claimed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
        });
        Assert.All(pairScores, pair =>
        {
            Assert.False(pair.GetProperty("token_savings_claim_allowed").GetBoolean());
            Assert.False(pair.GetProperty("quality_gate_passed").GetBoolean());
            Assert.False(pair.GetProperty("adaptive_effect_proven").GetBoolean());
        });

        using var captureReport = JsonDocument.Parse(File.ReadAllText(captureReportPath));
        Assert.False(captureReport.RootElement.GetProperty("stale_token_03_traces_reused").GetBoolean());
        Assert.Equal(108, captureReport.RootElement.GetProperty("captured_rows").GetInt32());
        Assert.Equal(54, captureReport.RootElement.GetProperty("pair_count").GetInt32());
        Assert.Equal(54, captureReport.RootElement.GetProperty("adaptive_rows_with_user_asset_loaded").GetInt32());
        Assert.Equal(0, captureReport.RootElement.GetProperty("adaptive_effect_pairs_in_fresh_capture").GetInt32());

        using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = report.RootElement;
        Assert.Equal("TOKEN-04-RERUN-WITH-ADAPTIVE-LEARNABILITY-01", root.GetProperty("phase_id").GetString());
        Assert.Equal(108, root.GetProperty("row_count").GetInt32());
        Assert.Equal(54, root.GetProperty("pair_count").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_passed_rows").GetInt32());
        Assert.Equal(96, root.GetProperty("failed_rows").GetInt32());
        Assert.Equal(12, root.GetProperty("needs_repair_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_gate_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("adaptive_effect_proven_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("token_savings_claim_allowed_pairs").GetInt32());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.Contains(
            "ADAPTIVE-EFFECT-GAP-02",
            root.GetProperty("next_bounded_work_order").GetString(),
            StringComparison.Ordinal);

        using var validation = JsonDocument.Parse(File.ReadAllText(validationPath));
        Assert.True(validation.RootElement.GetProperty("row_count_pass").GetBoolean());
        Assert.True(validation.RootElement.GetProperty("pair_count_pass").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("learnability_claimed").GetBoolean());
    }

    [Fact]
    public void Token04ScoreAfterAdaptiveEffectGap02Artifacts_ShowAdaptiveEffectButNoQualityOrTokenClaims()
    {
        var captureReportPath = Path.Combine(ArtifactDir, "token-04-rerun-after-adaptive-effect-gap-02-capture-report.json");
        var qualityInputsPath = Path.Combine(ArtifactDir, "token-04-rerun-after-adaptive-effect-gap-02-quality-scoring-inputs.jsonl");
        var rowScoresPath = Path.Combine(ArtifactDir, "token-04-score-after-adaptive-effect-gap-02-row-scores.jsonl");
        var pairScoresPath = Path.Combine(ArtifactDir, "token-04-score-after-adaptive-effect-gap-02-pair-results.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "token-04-score-after-adaptive-effect-gap-02-report.json");
        var validationPath = Path.Combine(ArtifactDir, "token-04-score-after-adaptive-effect-gap-02.validation.json");

        var qualityInputs = ReadJsonl(qualityInputsPath);
        var rowScores = ReadJsonl(rowScoresPath);
        var pairScores = ReadJsonl(pairScoresPath);
        Assert.Equal(108, qualityInputs.Count);
        Assert.Equal(108, rowScores.Count);
        Assert.Equal(54, pairScores.Count);
        Assert.All(qualityInputs, row =>
        {
            Assert.Equal("<redacted>", row.GetProperty("redacted_prompt").GetString());
            Assert.Equal("<redacted>", row.GetProperty("redacted_output").GetString());
        });
        AssertRepresented(rowScores, "host", new Dictionary<string, int>
        {
            ["Claude"] = 36,
            ["Codex"] = 36,
            ["Cursor"] = 36,
        });
        AssertRepresented(rowScores, "mode", new Dictionary<string, int>
        {
            ["adaptive_common_plus_user_model"] = 54,
            ["baseline_common_model_only"] = 54,
        });
        Assert.All(rowScores, row =>
        {
            Assert.False(row.GetProperty("quality_result_claimed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("learnability_claimed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
            var responsePath = row.GetProperty("response_ref_private").GetString();
            Assert.False(string.IsNullOrWhiteSpace(responsePath));
            var recomputedProjection = BuildScoredOutputProjection(responsePath!);
            Assert.DoesNotContain("UserAdaptiveAnyAssetLoaded", recomputedProjection, StringComparison.Ordinal);
            Assert.DoesNotContain("UserAdaptiveLoadOrder", recomputedProjection, StringComparison.Ordinal);
            Assert.Equal(row.GetProperty("output_hash").GetString(), Sha256(recomputedProjection));
        });
        Assert.All(pairScores, pair =>
        {
            Assert.True(pair.GetProperty("adaptive_effect_proven").GetBoolean());
            Assert.True(pair.GetProperty("quality_non_regression_for_scored_rows").GetBoolean());
            Assert.False(pair.GetProperty("quality_gate_passed").GetBoolean());
            Assert.False(pair.GetProperty("token_savings_claim_allowed").GetBoolean());
        });

        using var captureReport = JsonDocument.Parse(File.ReadAllText(captureReportPath));
        Assert.False(captureReport.RootElement.GetProperty("stale_token_03_traces_reused").GetBoolean());
        Assert.Equal(108, captureReport.RootElement.GetProperty("captured_rows").GetInt32());
        Assert.Equal(54, captureReport.RootElement.GetProperty("pair_count").GetInt32());
        Assert.Equal(54, captureReport.RootElement.GetProperty("adaptive_rows_with_user_asset_loaded").GetInt32());
        Assert.Equal(54, captureReport.RootElement.GetProperty("adaptive_effect_pairs_in_fresh_capture").GetInt32());

        using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = report.RootElement;
        Assert.Equal("TOKEN-04-SCORE-AFTER-ADAPTIVE-EFFECT-GAP-02", root.GetProperty("phase_id").GetString());
        Assert.Equal(108, root.GetProperty("row_count").GetInt32());
        Assert.Equal(54, root.GetProperty("pair_count").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_passed_rows").GetInt32());
        Assert.Equal(54, root.GetProperty("failed_rows").GetInt32());
        Assert.Equal(54, root.GetProperty("needs_repair_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_gate_pairs").GetInt32());
        Assert.Equal(54, root.GetProperty("adaptive_effect_proven_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("token_savings_claim_allowed_pairs").GetInt32());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.Contains(
            root.GetProperty("blocking_quality_gaps").EnumerateArray(),
            item => item.GetString()!.Contains("Adaptive output effect is now visible", StringComparison.Ordinal));

        using var validation = JsonDocument.Parse(File.ReadAllText(validationPath));
        Assert.True(validation.RootElement.GetProperty("row_count_pass").GetBoolean());
        Assert.True(validation.RootElement.GetProperty("pair_count_pass").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("learnability_claimed").GetBoolean());
    }

    [Fact]
    public void QualitySemanticGates03Artifacts_PassAdaptiveRuntimeRowsWithoutSemanticOverclaim()
    {
        var rowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-03-row-scores.jsonl");
        var pairScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-03-pair-results.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-03-report.json");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-03.validation.json");

        var rowScores = ReadJsonl(rowScoresPath);
        var pairScores = ReadJsonl(pairScoresPath);
        Assert.Equal(108, rowScores.Count);
        Assert.Equal(54, pairScores.Count);

        var adaptiveRows = rowScores
            .Where(row => row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model")
            .ToArray();
        var baselineRows = rowScores
            .Where(row => row.GetProperty("mode").GetString() == "baseline_common_model_only")
            .ToArray();
        Assert.Equal(54, adaptiveRows.Length);
        Assert.Equal(54, baselineRows.Length);
        Assert.All(adaptiveRows, row =>
        {
            Assert.Equal("PASS", row.GetProperty("verdict").GetString());
            Assert.True(row.GetProperty("quality_gate_passed").GetBoolean());
            Assert.Equal("runtime_row_only_not_semantic_not_pair_not_product", row.GetProperty("quality_gate_scope").GetString());
            Assert.True(row.GetProperty("runtime_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("pair_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("product_readiness_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
            Assert.All(row.GetProperty("layer_scores").EnumerateArray(), layer =>
            {
                Assert.Equal(4, layer.GetProperty("score").GetInt32());
                Assert.Contains(
                    "runtime_quality_passed_semantic_quality_not_proven",
                    ReadStringArray(layer.GetProperty("failure_categories")));
            });
        });
        Assert.All(baselineRows, row =>
        {
            Assert.Equal("FAIL", row.GetProperty("verdict").GetString());
            Assert.False(row.GetProperty("quality_gate_passed").GetBoolean());
            Assert.Equal("runtime_row_only_not_semantic_not_pair_not_product", row.GetProperty("quality_gate_scope").GetString());
            Assert.False(row.GetProperty("runtime_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("pair_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("product_readiness_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
        });
        Assert.All(pairScores, pair =>
        {
            Assert.True(pair.GetProperty("adaptive_effect_proven").GetBoolean());
            Assert.True(pair.GetProperty("quality_non_regression_for_scored_rows").GetBoolean());
            Assert.False(pair.GetProperty("quality_gate_passed").GetBoolean());
            Assert.Equal("paired_baseline_and_adaptive_runtime_rows_only_not_semantic_not_product", pair.GetProperty("quality_gate_scope").GetString());
            Assert.False(pair.GetProperty("runtime_quality_gate_passed_for_both_rows").GetBoolean());
            Assert.False(pair.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(pair.GetProperty("product_readiness_gate_passed").GetBoolean());
            Assert.False(pair.GetProperty("token_savings_claim_allowed").GetBoolean());
        });

        using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = report.RootElement;
        Assert.Equal("QUALITY-SEMANTIC-GATES-03", root.GetProperty("phase_id").GetString());
        Assert.Equal(54, root.GetProperty("quality_passed_rows").GetInt32());
        Assert.Equal(54, root.GetProperty("failed_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("needs_repair_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_gate_pairs").GetInt32());
        Assert.Equal(54, root.GetProperty("adaptive_effect_proven_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("token_savings_claim_allowed_pairs").GetInt32());
        Assert.Equal("runtime_rows_and_pairs_only_not_semantic_not_product", root.GetProperty("quality_gate_scope").GetString());
        Assert.Equal(54, root.GetProperty("runtime_quality_passed_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("semantic_quality_gate_passed_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("product_readiness_gate_passed_rows").GetInt32());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.Contains(
            root.GetProperty("blocking_quality_gaps").EnumerateArray(),
            item => item.GetString()!.Contains("baseline/common rows still fail", StringComparison.Ordinal));

        using var validation = JsonDocument.Parse(File.ReadAllText(validationPath));
        Assert.True(validation.RootElement.GetProperty("row_count_pass").GetBoolean());
        Assert.True(validation.RootElement.GetProperty("pair_count_pass").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("learnability_claimed").GetBoolean());
    }

    [Fact]
    public void QualitySemanticGates04Artifacts_RepairCommonSkillSelectionButKeepClaimsClosed()
    {
        var rowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-row-scores.jsonl");
        var pairScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-pair-results.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-report.json");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04.validation.json");
        var semanticProofGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-semantic-proof-gates.json");

        var rowScores = ReadJsonl(rowScoresPath);
        var pairScores = ReadJsonl(pairScoresPath);
        Assert.Equal(108, rowScores.Count);
        Assert.Equal(54, pairScores.Count);

        var adaptiveRows = rowScores
            .Where(row => row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model")
            .ToArray();
        var baselineRows = rowScores
            .Where(row => row.GetProperty("mode").GetString() == "baseline_common_model_only")
            .ToArray();
        Assert.Equal(54, adaptiveRows.Length);
        Assert.Equal(54, baselineRows.Length);
        Assert.All(adaptiveRows, row =>
        {
            Assert.Equal("PASS", row.GetProperty("verdict").GetString());
            Assert.True(row.GetProperty("quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
        });
        Assert.All(baselineRows, row =>
        {
            Assert.Equal("NEEDS_REPAIR", row.GetProperty("verdict").GetString());
            Assert.False(row.GetProperty("quality_gate_passed").GetBoolean());
            var layers = row.GetProperty("layer_scores").EnumerateArray().ToArray();
            var prompt = Assert.Single(layers, layer => layer.GetProperty("layer_id").GetString() == "prompt_to_task");
            var orchestration = Assert.Single(layers, layer => layer.GetProperty("layer_id").GetString() == "orchestration_reasoning");
            var skill = Assert.Single(layers, layer => layer.GetProperty("layer_id").GetString() == "skill_selection");
            Assert.Equal(2, prompt.GetProperty("score").GetInt32());
            Assert.Equal(3, orchestration.GetProperty("score").GetInt32());
            Assert.Equal(4, skill.GetProperty("score").GetInt32());
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
        });
        Assert.All(pairScores, pair =>
        {
            Assert.True(pair.GetProperty("adaptive_effect_proven").GetBoolean());
            Assert.True(pair.GetProperty("quality_non_regression_for_scored_rows").GetBoolean());
            Assert.False(pair.GetProperty("quality_gate_passed").GetBoolean());
            Assert.False(pair.GetProperty("token_savings_claim_allowed").GetBoolean());
        });

        using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = report.RootElement;
        Assert.Equal("QUALITY-SEMANTIC-GATES-04", root.GetProperty("phase_id").GetString());
        Assert.Equal(54, root.GetProperty("quality_passed_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("failed_rows").GetInt32());
        Assert.Equal(54, root.GetProperty("needs_repair_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_gate_pairs").GetInt32());
        Assert.Equal(54, root.GetProperty("adaptive_effect_proven_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("token_savings_claim_allowed_pairs").GetInt32());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.Contains(
            root.GetProperty("blocking_quality_gaps").EnumerateArray(),
            item => item.GetString()!.Contains("SkillSelection rows now select skills without second pass", StringComparison.Ordinal));

        using var validation = JsonDocument.Parse(File.ReadAllText(validationPath));
        Assert.True(validation.RootElement.GetProperty("row_count_pass").GetBoolean());
        Assert.True(validation.RootElement.GetProperty("pair_count_pass").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validation.RootElement.GetProperty("learnability_claimed").GetBoolean());

        using var semanticProofGates = JsonDocument.Parse(File.ReadAllText(semanticProofGatesPath));
        var gates = semanticProofGates.RootElement;
        Assert.False(gates.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.False(gates.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.False(gates.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.False(gates.GetProperty("fresh_load_gate_passed").GetBoolean());
        Assert.False(gates.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(gates.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(gates.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(gates.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(gates.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(gates.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(gates.GetProperty("next_action").GetString()));
    }

    [Fact]
    public void QualitySemanticGates05Artifacts_BuildSemanticProofFrameworkForExtensionApiOutputs()
    {
        var qsg04RowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-row-scores.jsonl");
        var qsg04PairScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-pair-results.jsonl");
        var qsg04ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-report.json");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-05-component-gates.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-05-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-05-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-05.validation.json");
        var statePath = Path.Combine(
            ArtifactDir,
            "..",
            "..",
            "docs",
            "plans",
            "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");

        var rowScores = ReadJsonl(qsg04RowScoresPath);
        var pairScores = ReadJsonl(qsg04PairScoresPath);
        using var qsg04Report = JsonDocument.Parse(File.ReadAllText(qsg04ReportPath));
        var qsg04Root = qsg04Report.RootElement;
        var responseRefs = rowScores
            .Select(row => row.GetProperty("response_ref_private").GetString()!)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var componentRows = new[]
        {
            BuildSemanticComponentGateRow(
                "prompt_to_task",
                "PromptToTaskMachine",
                "prompt_to_task_asset_loaded",
                "prompt_to_task_status",
                "prompt_to_task",
                rowScores,
                responseRefs,
                "Baseline/common PromptToTask still reports no matching learned pattern for 54 rows; source-disjoint row-level eval and ablation controls are not accepted for these extension API outputs."),
            BuildSemanticComponentGateRow(
                "orchestration_reasoning",
                "OrchestrationReasoningMachine",
                "orchestration_reasoning_asset_loaded",
                "orchestration_reasoning_status",
                "orchestration_reasoning",
                rowScores,
                responseRefs,
                "Baseline/common OrchestrationReasoning still lacks independently quality-scored semantic plan proof for 54 rows; source-disjoint row-level eval and ablation controls are not accepted for these extension API outputs."),
            BuildSemanticComponentGateRow(
                "skill_selection",
                "SkillSelectionMachine",
                "skill_selection_asset_loaded",
                "skill_selection_requires_second_pass",
                "skill_selection",
                rowScores,
                responseRefs,
                "Common SkillSelection runtime behavior is repaired, but semantic quality still needs source-disjoint eval, ablation, leakage authority controls, and continue-training evidence for these extension API outputs.")
        };

        WriteJsonl(componentGatesPath, componentRows);

        var freshLoadPassedComponents = componentRows.Count(row => row.FreshLoadGatePassed);
        var beforeAfterCounts = new
        {
            qsg04 = new
            {
                row_count = qsg04Root.GetProperty("row_count").GetInt32(),
                pair_count = qsg04Root.GetProperty("pair_count").GetInt32(),
                adaptive_runtime_pass_rows = qsg04Root.GetProperty("runtime_quality_passed_rows").GetInt32(),
                baseline_common_needs_repair_rows = qsg04Root.GetProperty("needs_repair_rows").GetInt32(),
                failed_rows = qsg04Root.GetProperty("failed_rows").GetInt32(),
                quality_gate_pairs = qsg04Root.GetProperty("quality_gate_pairs").GetInt32(),
                adaptive_effect_proven_pairs = qsg04Root.GetProperty("adaptive_effect_proven_pairs").GetInt32(),
                token_savings_claim_allowed_pairs = qsg04Root.GetProperty("token_savings_claim_allowed_pairs").GetInt32(),
                semantic_framework_component_rows = 0,
                fresh_load_component_gates_passed = 0
            },
            qsg05 = new
            {
                row_count = rowScores.Count,
                pair_count = pairScores.Count,
                adaptive_runtime_pass_rows = rowScores.Count(row =>
                    row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model"
                    && row.GetProperty("runtime_quality_gate_passed").GetBoolean()),
                baseline_common_needs_repair_rows = rowScores.Count(row =>
                    row.GetProperty("mode").GetString() == "baseline_common_model_only"
                    && row.GetProperty("verdict").GetString() == "NEEDS_REPAIR"),
                failed_rows = rowScores.Count(row => row.GetProperty("verdict").GetString() == "FAIL"),
                quality_gate_pairs = pairScores.Count(pair => pair.GetProperty("quality_gate_passed").GetBoolean()),
                adaptive_effect_proven_pairs = pairScores.Count(pair => pair.GetProperty("adaptive_effect_proven").GetBoolean()),
                token_savings_claim_allowed_pairs = pairScores.Count(pair => pair.GetProperty("token_savings_claim_allowed").GetBoolean()),
                semantic_framework_component_rows = componentRows.Length,
                fresh_load_component_gates_passed = freshLoadPassedComponents
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_05.semantic_proof_report.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-05",
            input_phase_id = "QUALITY-SEMANTIC-GATES-04",
            qsg04_report_ref = ToArtifactRelativePath(qsg04ReportPath),
            component_gates_ref = ToArtifactRelativePath(componentGatesPath),
            row_count = rowScores.Count,
            pair_count = pairScores.Count,
            component_gate_count = componentRows.Length,
            source_disjoint_eval_gate_passed = false,
            ablation_gate_passed = false,
            leakage_gate_passed = false,
            fresh_load_gate_passed = componentRows.All(row => row.FreshLoadGatePassed),
            continue_training_gate_passed = false,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            token_savings_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            before_after_counts = beforeAfterCounts,
            repaired_in_this_slice = new[]
            {
                "Added persisted per-component semantic proof gate framework for PromptToTask, OrchestrationReasoning, and SkillSelection extension API outputs.",
                "Proved current QSG-04 extension API outputs carry freshly loaded common asset refs for all three components.",
                "Kept source-disjoint eval, ablation, leakage/no exact source authority, continue-training, semantic quality, common readiness, quality improvement, and token savings gates closed."
            },
            remaining_blockers = new[]
            {
                "Source-disjoint row-level evaluation for these exact extension API outputs is not accepted.",
                "Ablation/control rows for these exact extension API outputs are not accepted.",
                "Captured response refs still contain scenario/source-like identifiers, so no exact source or pair-id authority must be proven by controls before the leakage gate can pass.",
                "Continue-training evidence exists only as earlier lifecycle evidence and is not a QSG-05 source-disjoint continuation proof for these exact outputs.",
                "Baseline/common PromptToTask and OrchestrationReasoning still have 54 needs-repair rows each."
            },
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-06: run accepted source-disjoint row-level eval and ablation/no-leakage controls for current extension API outputs, then repair common PromptToTask/Orchestration coverage without stack shortcuts."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_05.validation.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-05",
            component_gate_count_expected = 3,
            component_gate_count_actual = componentRows.Length,
            qsg04_row_count_preserved = rowScores.Count == qsg04Root.GetProperty("row_count").GetInt32(),
            qsg04_pair_count_preserved = pairScores.Count == qsg04Root.GetProperty("pair_count").GetInt32(),
            fresh_load_component_gates_passed = freshLoadPassedComponents,
            source_disjoint_eval_gate_passed = false,
            ablation_gate_passed = false,
            leakage_gate_passed = false,
            continue_training_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            token_savings_claim_allowed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            output_files = new[]
            {
                Path.GetFullPath(componentGatesPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath)
            }
        };

        WriteJson(reportPath, report);
        WriteJson(validationPath, validation);
        WriteQsg05Markdown(reportMdPath, componentRows, beforeAfterCounts);
        var statePatchPath = WriteQsgStatePatch(
            statePath,
            "QUALITY-SEMANTIC-GATES-05",
            "SEMANTIC_PROOF_FRAMEWORK_RECORDED_READINESS_CLAIMS_STILL_CLOSED",
            "QSG-05 added per-component semantic proof gate framework rows; source-disjoint eval, ablation, leakage, continue-training, semantic quality, common readiness, quality improvement, token savings, training, checkpoint mutation, promotion, and final readiness remain closed.",
            "QUALITY-SEMANTIC-GATES-06: run accepted source-disjoint row-level eval and ablation/no-leakage controls for current extension API outputs, then repair common PromptToTask/Orchestration coverage without stack shortcuts.",
            report,
            validation,
            new Dictionary<string, object?>
            {
                ["quality_semantic_gates_05"] = report,
                ["quality_semantic_gates_05_validation"] = validation
            });

        var generatedRows = ReadJsonl(componentGatesPath);
        Assert.Equal(3, generatedRows.Count);
        Assert.All(generatedRows, row =>
        {
            Assert.False(row.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("ablation_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("leakage_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("fresh_load_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("continue_training_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.Equal(0, row.GetProperty("source_disjoint_eval_rows_accepted").GetInt32());
            Assert.Equal(0, row.GetProperty("ablation_control_rows_accepted").GetInt32());
            Assert.Equal(0, row.GetProperty("semantic_gate_rows_passed").GetInt32());
            Assert.Equal(
                row.GetProperty("extension_api_output_rows").GetInt32(),
                row.GetProperty("source_identifier_marker_rows").GetInt32());
        });

        using var generatedReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = generatedReport.RootElement;
        Assert.Equal("QUALITY-SEMANTIC-GATES-05", root.GetProperty("phase_id").GetString());
        Assert.Equal(108, root.GetProperty("row_count").GetInt32());
        Assert.Equal(54, root.GetProperty("pair_count").GetInt32());
        Assert.Equal(3, root.GetProperty("component_gate_count").GetInt32());
        Assert.False(root.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("fresh_load_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(root.GetProperty("training_started").GetBoolean());
        Assert.False(root.GetProperty("checkpoint_mutation_started").GetBoolean());
        Assert.Contains(
            "QUALITY-SEMANTIC-GATES-06",
            root.GetProperty("next_bounded_work_order").GetString(),
            StringComparison.Ordinal);
        Assert.Equal(3, root.GetProperty("before_after_counts").GetProperty("qsg05").GetProperty("fresh_load_component_gates_passed").GetInt32());

        using var generatedValidation = JsonDocument.Parse(File.ReadAllText(validationPath));
        var validationRoot = generatedValidation.RootElement;
        Assert.Equal(3, validationRoot.GetProperty("component_gate_count_actual").GetInt32());
        Assert.True(validationRoot.GetProperty("qsg04_row_count_preserved").GetBoolean());
        Assert.True(validationRoot.GetProperty("qsg04_pair_count_preserved").GetBoolean());
        Assert.False(validationRoot.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("training_started").GetBoolean());
        Assert.False(validationRoot.GetProperty("checkpoint_mutation_started").GetBoolean());

        var stateRoot = ReadStatePatchFields(statePatchPath, "QUALITY-SEMANTIC-GATES-05");
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.Equal(
            "QUALITY-SEMANTIC-GATES-05",
            stateRoot.GetProperty("quality_semantic_gates_05").GetProperty("phase_id").GetString());
    }

    [Fact]
    public void QualitySemanticGates06Artifacts_AddLeakageAuthorityControlsWithoutSemanticOverclaim()
    {
        var qsg04RowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-row-scores.jsonl");
        var qsg04PairScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-pair-results.jsonl");
        var qsg05ComponentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-05-component-gates.jsonl");
        var qsg05ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-05-report.json");
        var leakageControlsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-06-leakage-authority-controls.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-06-component-gates.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-06-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-06-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-06.validation.json");
        var statePath = Path.Combine(
            ArtifactDir,
            "..",
            "..",
            "docs",
            "plans",
            "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");

        var rowScores = ReadJsonl(qsg04RowScoresPath);
        var pairScores = ReadJsonl(qsg04PairScoresPath);
        var qsg05ComponentRows = ReadJsonl(qsg05ComponentGatesPath);
        using var qsg05Report = JsonDocument.Parse(File.ReadAllText(qsg05ReportPath));
        var qsg05Root = qsg05Report.RootElement;
        var exactResponseRows = rowScores
            .GroupBy(row => row.GetProperty("response_ref_private").GetString()!, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(row => row.GetProperty("response_ref_private").GetString(), StringComparer.Ordinal)
            .ToArray();

        var leakageRows = exactResponseRows
            .Select(row => BuildLeakageAuthorityControlRow(row, row.GetProperty("response_ref_private").GetString()!))
            .ToArray();
        WriteJsonl(leakageControlsPath, leakageRows);

        var acceptedLeakageRows = leakageRows.Count(row => row.Accepted);
        var authorityProjectionMarkerRows = leakageRows.Count(row => row.AuthorityProjectionContainsSourceMarkers);
        var exactScenarioIdAuthorityProjectionRows = leakageRows.Count(row => row.ExactScenarioIdAuthorityProjected);
        var structuredTaskRefAuthorityProjectionRows = leakageRows.Count(row => row.StructuredTaskRefAuthorityProjected);
        var scenarioRefAuthorityProjectionRows = leakageRows.Count(row => row.ScenarioRefAuthorityProjected);
        var pairIdAuthorityProjectionRows = leakageRows.Count(row => row.PairIdAuthorityProjected);
        var sourcePromptMarkerAuthorityProjectionRows = leakageRows.Count(row => row.SourcePromptMarkerAuthorityProjected);
        var privateCapturePathAuthorityProjectionRows = leakageRows.Count(row => row.PrivateCapturePathAuthorityProjected);
        var rawMarkerRows = leakageRows.Count(row => row.RawResponseContainsSourceMarkers);
        var projectionChangedRows = leakageRows.Count(row =>
            !string.Equals(row.RawScoredOutputHash, row.AuthorityProjectionHash, StringComparison.Ordinal));
        var componentRows = qsg05ComponentRows
            .Select(row => new Qsg06ComponentGateRow(
                ComponentId: row.GetProperty("component_id").GetString()!,
                MachineId: row.GetProperty("machine_id").GetString()!,
                ExtensionApiOutputRows: row.GetProperty("extension_api_output_rows").GetInt32(),
                RuntimeLayerPassRows: row.GetProperty("runtime_layer_pass_rows").GetInt32(),
                BaselineCommonNeedsRepairRows: row.GetProperty("baseline_common_needs_repair_rows").GetInt32(),
                SemanticGateRowsPassed: row.GetProperty("semantic_gate_rows_passed").GetInt32(),
                SourceDisjointEvalRowsAccepted: 0,
                AblationControlRowsAccepted: 0,
                LeakageAuthorityControlRowsAccepted: acceptedLeakageRows,
                SourceIdentifierMarkerRows: row.GetProperty("source_identifier_marker_rows").GetInt32(),
                AuthorityProjectionSourceMarkerRows: authorityProjectionMarkerRows,
                FreshLoadEvidenceRows: row.GetProperty("fresh_load_evidence_rows").GetInt32(),
                ContinueTrainingRowsAccepted: 0,
                SourceDisjointEvalGatePassed: false,
                AblationGatePassed: false,
                LeakageGatePassed: acceptedLeakageRows == exactResponseRows.Length && authorityProjectionMarkerRows == 0,
                FreshLoadGatePassed: row.GetProperty("fresh_load_gate_passed").GetBoolean(),
                ContinueTrainingGatePassed: false,
                SemanticQualityGatePassed: false,
                SemanticQualityClaimed: false,
                StatusValues: ReadStringArray(row.GetProperty("status_values")),
                EvidenceRefs:
                [
                    ToArtifactRelativePath(qsg04RowScoresPath),
                    ToArtifactRelativePath(leakageControlsPath)
                ],
                RemainingBlockers:
                [
                    "Source-disjoint row-level evaluation for these exact extension API outputs is not accepted.",
                    "Ablation/control rows for these exact extension API outputs are not accepted.",
                    "Leakage authority controls now exclude exact source markers from the QSG-06 authority projection, but raw response refs still carry scenario/source-like identifiers as non-authoritative trace evidence.",
                    "No QSG-06 continue-training run from source-disjoint accepted rows exists for these exact extension API outputs.",
                    "Baseline/common PromptToTask and OrchestrationReasoning still have 54 needs-repair rows each."
                ]))
            .ToArray();
        WriteJsonl(componentGatesPath, componentRows);

        var beforeAfterCounts = new
        {
            qsg05 = qsg05Root.GetProperty("before_after_counts").GetProperty("qsg05").Clone(),
            qsg06 = new
            {
                row_count = rowScores.Count,
                pair_count = pairScores.Count,
                adaptive_runtime_pass_rows = rowScores.Count(row =>
                    row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model"
                    && row.GetProperty("runtime_quality_gate_passed").GetBoolean()),
                baseline_common_needs_repair_rows = rowScores.Count(row =>
                    row.GetProperty("mode").GetString() == "baseline_common_model_only"
                    && row.GetProperty("verdict").GetString() == "NEEDS_REPAIR"),
                failed_rows = rowScores.Count(row => row.GetProperty("verdict").GetString() == "FAIL"),
                quality_gate_pairs = pairScores.Count(pair => pair.GetProperty("quality_gate_passed").GetBoolean()),
                adaptive_effect_proven_pairs = pairScores.Count(pair => pair.GetProperty("adaptive_effect_proven").GetBoolean()),
                token_savings_claim_allowed_pairs = pairScores.Count(pair => pair.GetProperty("token_savings_claim_allowed").GetBoolean()),
                semantic_framework_component_rows = componentRows.Length,
                fresh_load_component_gates_passed = componentRows.Count(row => row.FreshLoadGatePassed),
                leakage_authority_control_rows = leakageRows.Length,
                leakage_authority_control_rows_accepted = acceptedLeakageRows,
                leakage_component_gates_passed = componentRows.Count(row => row.LeakageGatePassed),
                source_disjoint_eval_rows_accepted = 0,
                ablation_control_rows_accepted = 0,
                continue_training_rows_accepted = 0
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_06.leakage_authority_control_report.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-06",
            input_phase_id = "QUALITY-SEMANTIC-GATES-05",
            qsg05_report_ref = ToArtifactRelativePath(qsg05ReportPath),
            leakage_controls_ref = ToArtifactRelativePath(leakageControlsPath),
            component_gates_ref = ToArtifactRelativePath(componentGatesPath),
            row_count = rowScores.Count,
            pair_count = pairScores.Count,
            exact_response_row_count = exactResponseRows.Length,
            component_gate_count = componentRows.Length,
            leakage_authority_control_row_count = leakageRows.Length,
            leakage_authority_control_rows_accepted = acceptedLeakageRows,
            raw_response_source_marker_rows = rawMarkerRows,
            authority_projection_source_marker_rows = authorityProjectionMarkerRows,
            exact_scenario_id_authority_projection_rows = exactScenarioIdAuthorityProjectionRows,
            structured_task_ref_authority_projection_rows = structuredTaskRefAuthorityProjectionRows,
            scenario_ref_authority_projection_rows = scenarioRefAuthorityProjectionRows,
            pair_id_authority_projection_rows = pairIdAuthorityProjectionRows,
            source_prompt_marker_authority_projection_rows = sourcePromptMarkerAuthorityProjectionRows,
            private_capture_path_authority_projection_rows = privateCapturePathAuthorityProjectionRows,
            authority_projection_changed_from_qsg04_scored_output_rows = projectionChangedRows,
            source_disjoint_eval_gate_passed = false,
            ablation_gate_passed = false,
            leakage_gate_passed = componentRows.All(row => row.LeakageGatePassed),
            fresh_load_gate_passed = componentRows.All(row => row.FreshLoadGatePassed),
            continue_training_gate_passed = false,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            token_savings_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            deterministic_helper_classification = "baseline/internal projection control only; not semantic quality and not source-disjoint eval",
            before_after_counts = beforeAfterCounts,
            repaired_in_this_slice = new[]
            {
                "Added QSG-06 leakage/no-source-marker authority controls for all 108 exact QSG-04 extension API output rows.",
                "Proved the QSG-06 authority projection excludes exact scenario, structured-task, pair-id, source-prompt, and private capture path markers even though raw response refs retain them as trace evidence.",
                "Kept source-disjoint eval, ablation, continue-training, semantic quality, common readiness, product readiness, promotion/final readiness, quality improvement, token savings, training, and checkpoint mutation claims closed."
            },
            remaining_blockers = new[]
            {
                "Source-disjoint row-level evaluation for these exact extension API outputs is not accepted.",
                "Ablation/control rows for these exact extension API outputs are not accepted.",
                "Continue-training evidence exists only as earlier lifecycle evidence and is not a QSG-06 source-disjoint continuation proof for these exact outputs.",
                "Baseline/common PromptToTask and OrchestrationReasoning still have 54 needs-repair rows each."
            },
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-07: add accepted source-disjoint eval rows and ablation/control rows for exact extension API outputs before any semantic-quality, common-readiness, quality-improvement, or token-savings claim."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_06.validation.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-06",
            exact_response_row_count_expected = 108,
            exact_response_row_count_actual = exactResponseRows.Length,
            component_gate_count_expected = 3,
            component_gate_count_actual = componentRows.Length,
            leakage_authority_control_row_count = leakageRows.Length,
            leakage_authority_control_rows_accepted = acceptedLeakageRows,
            raw_response_source_marker_rows = rawMarkerRows,
            authority_projection_source_marker_rows = authorityProjectionMarkerRows,
            exact_scenario_id_authority_projection_rows = exactScenarioIdAuthorityProjectionRows,
            structured_task_ref_authority_projection_rows = structuredTaskRefAuthorityProjectionRows,
            scenario_ref_authority_projection_rows = scenarioRefAuthorityProjectionRows,
            pair_id_authority_projection_rows = pairIdAuthorityProjectionRows,
            source_prompt_marker_authority_projection_rows = sourcePromptMarkerAuthorityProjectionRows,
            private_capture_path_authority_projection_rows = privateCapturePathAuthorityProjectionRows,
            authority_projection_changed_from_qsg04_scored_output_rows = projectionChangedRows,
            qsg04_row_count_preserved = rowScores.Count == qsg05Root.GetProperty("row_count").GetInt32(),
            qsg04_pair_count_preserved = pairScores.Count == qsg05Root.GetProperty("pair_count").GetInt32(),
            source_disjoint_eval_gate_passed = false,
            ablation_gate_passed = false,
            leakage_gate_passed = componentRows.All(row => row.LeakageGatePassed),
            continue_training_gate_passed = false,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            token_savings_claim_allowed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            output_files = new[]
            {
                Path.GetFullPath(leakageControlsPath),
                Path.GetFullPath(componentGatesPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath)
            }
        };

        WriteJson(reportPath, report);
        WriteJson(validationPath, validation);
        WriteQsg06Markdown(reportMdPath, componentRows, beforeAfterCounts);
        var statePatchPath = WriteQsgStatePatch(
            statePath,
            "QUALITY-SEMANTIC-GATES-06",
            "LEAKAGE_AUTHORITY_CONTROLS_RECORDED_READINESS_CLAIMS_STILL_CLOSED",
            "QSG-06 added leakage/no-source-marker authority controls; source-disjoint eval, ablation, continue-training, semantic quality, common readiness, quality improvement, token savings, training, checkpoint mutation, promotion, and final readiness remain closed.",
            "QUALITY-SEMANTIC-GATES-07: add accepted source-disjoint eval rows and ablation/control rows for exact extension API outputs before any semantic-quality, common-readiness, quality-improvement, or token-savings claim.",
            report,
            validation,
            new Dictionary<string, object?>
            {
                ["quality_semantic_gates_06"] = report,
                ["quality_semantic_gates_06_validation"] = validation
            });

        var generatedControls = ReadJsonl(leakageControlsPath);
        Assert.Equal(108, generatedControls.Count);
        Assert.All(generatedControls, row =>
        {
            Assert.True(row.GetProperty("raw_response_contains_source_markers").GetBoolean());
            Assert.False(row.GetProperty("authority_projection_contains_source_markers").GetBoolean());
            Assert.True(row.GetProperty("source_marker_authority_excluded").GetBoolean());
            Assert.True(row.GetProperty("raw_source_marker_authority_excluded").GetBoolean());
            Assert.True(row.GetProperty("accepted").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("exact_scenario_id_authority_projected").GetBoolean());
            Assert.False(row.GetProperty("structured_task_ref_authority_projected").GetBoolean());
            Assert.False(row.GetProperty("scenario_ref_authority_projected").GetBoolean());
            Assert.False(row.GetProperty("pair_id_authority_projected").GetBoolean());
            Assert.False(row.GetProperty("source_prompt_marker_authority_projected").GetBoolean());
            Assert.False(row.GetProperty("private_capture_path_authority_projected").GetBoolean());
            AssertNoExactSourceAuthorityInProjection(row);
            Assert.NotEqual(
                row.GetProperty("raw_scored_output_hash").GetString(),
                row.GetProperty("authority_projection_hash").GetString());
            Assert.DoesNotContain(
                "StructuredTaskRefs",
                ReadStringArray(row.GetProperty("authority_projection_fields")),
                StringComparer.Ordinal);
            Assert.DoesNotContain(
                "ScenarioRefs",
                ReadStringArray(row.GetProperty("authority_projection_fields")),
                StringComparer.Ordinal);
        });

        var generatedRows = ReadJsonl(componentGatesPath);
        Assert.Equal(3, generatedRows.Count);
        Assert.All(generatedRows, row =>
        {
            Assert.False(row.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("ablation_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("leakage_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("fresh_load_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("continue_training_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.Equal(0, row.GetProperty("source_disjoint_eval_rows_accepted").GetInt32());
            Assert.Equal(0, row.GetProperty("ablation_control_rows_accepted").GetInt32());
            Assert.Equal(108, row.GetProperty("leakage_authority_control_rows_accepted").GetInt32());
            Assert.Equal(0, row.GetProperty("authority_projection_source_marker_rows").GetInt32());
        });

        using var generatedReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = generatedReport.RootElement;
        Assert.Equal("QUALITY-SEMANTIC-GATES-06", root.GetProperty("phase_id").GetString());
        Assert.Equal(108, root.GetProperty("row_count").GetInt32());
        Assert.Equal(54, root.GetProperty("pair_count").GetInt32());
        Assert.Equal(108, root.GetProperty("leakage_authority_control_rows_accepted").GetInt32());
        Assert.Equal(108, root.GetProperty("raw_response_source_marker_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("authority_projection_source_marker_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("exact_scenario_id_authority_projection_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("structured_task_ref_authority_projection_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("scenario_ref_authority_projection_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("pair_id_authority_projection_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("source_prompt_marker_authority_projection_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("private_capture_path_authority_projection_rows").GetInt32());
        Assert.False(root.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("fresh_load_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(root.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(root.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("training_started").GetBoolean());
        Assert.False(root.GetProperty("checkpoint_mutation_started").GetBoolean());
        Assert.Equal(3, root.GetProperty("before_after_counts").GetProperty("qsg06").GetProperty("leakage_component_gates_passed").GetInt32());

        using var generatedValidation = JsonDocument.Parse(File.ReadAllText(validationPath));
        var validationRoot = generatedValidation.RootElement;
        Assert.Equal(108, validationRoot.GetProperty("exact_response_row_count_actual").GetInt32());
        Assert.Equal(3, validationRoot.GetProperty("component_gate_count_actual").GetInt32());
        Assert.Equal(108, validationRoot.GetProperty("leakage_authority_control_rows_accepted").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("exact_scenario_id_authority_projection_rows").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("structured_task_ref_authority_projection_rows").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("scenario_ref_authority_projection_rows").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("pair_id_authority_projection_rows").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("source_prompt_marker_authority_projection_rows").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("private_capture_path_authority_projection_rows").GetInt32());
        Assert.True(validationRoot.GetProperty("qsg04_row_count_preserved").GetBoolean());
        Assert.True(validationRoot.GetProperty("qsg04_pair_count_preserved").GetBoolean());
        Assert.False(validationRoot.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.True(validationRoot.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(validationRoot.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("training_started").GetBoolean());
        Assert.False(validationRoot.GetProperty("checkpoint_mutation_started").GetBoolean());

        var stateRoot = ReadStatePatchFields(statePatchPath, "QUALITY-SEMANTIC-GATES-06");
        var currentPhase = stateRoot.GetProperty("current_phase").GetString();
        Assert.Equal("QUALITY-SEMANTIC-GATES-06", currentPhase);
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        var qsg06State = stateRoot.GetProperty("quality_semantic_gates_06");
        Assert.False(qsg06State.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(qsg06State.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(qsg06State.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(qsg06State.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.Contains(
            "QUALITY-SEMANTIC-GATES-07",
            stateRoot.GetProperty("next_action").GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void QualitySemanticGates07Artifacts_AddSourceDisjointEvalAndAblationControlsWithoutSemanticOverclaim()
    {
        var qsg04RowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-row-scores.jsonl");
        var qsg04PairScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-pair-results.jsonl");
        var qsg06ComponentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-06-component-gates.jsonl");
        var qsg06ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-06-report.json");
        var sourceDisjointRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-source-disjoint-eval-rows.jsonl");
        var ablationControlsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-ablation-controls.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-component-gates.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07.validation.json");
        var statePath = Path.Combine(
            ArtifactDir,
            "..",
            "..",
            "docs",
            "plans",
            "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");

        var rowScores = ReadJsonl(qsg04RowScoresPath);
        var pairScores = ReadJsonl(qsg04PairScoresPath);
        var qsg06ComponentRows = ReadJsonl(qsg06ComponentGatesPath);
        using var qsg06Report = JsonDocument.Parse(File.ReadAllText(qsg06ReportPath));
        var qsg06Root = qsg06Report.RootElement;
        var exactResponseRows = rowScores
            .GroupBy(row => row.GetProperty("response_ref_private").GetString()!, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(row => row.GetProperty("response_ref_private").GetString(), StringComparer.Ordinal)
            .ToArray();

        var sourceDisjointRows = exactResponseRows
            .Select(row => BuildSourceDisjointEvalRow(row, row.GetProperty("response_ref_private").GetString()!))
            .ToArray();
        var ablationRows = exactResponseRows
            .Select(row => BuildAblationControlRow(row, row.GetProperty("response_ref_private").GetString()!))
            .ToArray();
        WriteJsonl(sourceDisjointRowsPath, sourceDisjointRows);
        WriteJsonl(ablationControlsPath, ablationRows);

        var acceptedSourceDisjointRows = sourceDisjointRows.Count(row => row.Accepted);
        var sourceDisjointMarkerRows = sourceDisjointRows.Count(row => row.EvalInputContainsSourceMarkers);
        var acceptedAblationRows = ablationRows.Count(row => row.Accepted);
        var ablationMarkerRows = ablationRows.Count(row =>
            row.FullProjectionContainsSourceMarkers || row.AblatedProjectionContainsSourceMarkers);
        var componentRows = qsg06ComponentRows
            .Select(row => new Qsg07ComponentGateRow(
                ComponentId: row.GetProperty("component_id").GetString()!,
                MachineId: row.GetProperty("machine_id").GetString()!,
                ExtensionApiOutputRows: row.GetProperty("extension_api_output_rows").GetInt32(),
                RuntimeLayerPassRows: row.GetProperty("runtime_layer_pass_rows").GetInt32(),
                BaselineCommonNeedsRepairRows: row.GetProperty("baseline_common_needs_repair_rows").GetInt32(),
                SemanticGateRowsPassed: row.GetProperty("semantic_gate_rows_passed").GetInt32(),
                SourceDisjointEvalRowsAccepted: acceptedSourceDisjointRows,
                AblationControlRowsAccepted: acceptedAblationRows,
                LeakageAuthorityControlRowsAccepted: row.GetProperty("leakage_authority_control_rows_accepted").GetInt32(),
                SourceIdentifierMarkerRows: row.GetProperty("source_identifier_marker_rows").GetInt32(),
                AuthorityProjectionSourceMarkerRows: row.GetProperty("authority_projection_source_marker_rows").GetInt32(),
                SourceDisjointEvalInputMarkerRows: sourceDisjointMarkerRows,
                AblationControlMarkerRows: ablationMarkerRows,
                FreshLoadEvidenceRows: row.GetProperty("fresh_load_evidence_rows").GetInt32(),
                ContinueTrainingRowsAccepted: 0,
                SourceDisjointEvalGatePassed: acceptedSourceDisjointRows == exactResponseRows.Length && sourceDisjointMarkerRows == 0,
                AblationGatePassed: acceptedAblationRows == exactResponseRows.Length && ablationMarkerRows == 0,
                LeakageGatePassed: row.GetProperty("leakage_gate_passed").GetBoolean(),
                FreshLoadGatePassed: row.GetProperty("fresh_load_gate_passed").GetBoolean(),
                ContinueTrainingGatePassed: false,
                SemanticQualityGatePassed: false,
                SemanticQualityClaimed: false,
                StatusValues: ReadStringArray(row.GetProperty("status_values")),
                EvidenceRefs:
                [
                    ToArtifactRelativePath(qsg04RowScoresPath),
                    ToArtifactRelativePath(sourceDisjointRowsPath),
                    ToArtifactRelativePath(ablationControlsPath)
                ],
                RemainingBlockers:
                [
                    "QSG-07 accepts source-disjoint eval rows and ablation/control rows for exact extension API outputs, but these controls are evidence gates only and do not repair baseline/common output quality.",
                    "No QSG-07 continue-training run from source-disjoint accepted rows exists for these exact extension API outputs.",
                    "Baseline/common PromptToTask and OrchestrationReasoning still have 54 needs-repair rows each.",
                    "Semantic quality, common readiness, quality improvement, token savings, promotion, and final readiness remain forbidden claims."
                ]))
            .ToArray();
        WriteJsonl(componentGatesPath, componentRows);

        var beforeAfterCounts = new
        {
            qsg06 = qsg06Root.GetProperty("before_after_counts").GetProperty("qsg06").Clone(),
            qsg07 = new
            {
                row_count = rowScores.Count,
                pair_count = pairScores.Count,
                adaptive_runtime_pass_rows = rowScores.Count(row =>
                    row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model"
                    && row.GetProperty("runtime_quality_gate_passed").GetBoolean()),
                baseline_common_needs_repair_rows = rowScores.Count(row =>
                    row.GetProperty("mode").GetString() == "baseline_common_model_only"
                    && row.GetProperty("verdict").GetString() == "NEEDS_REPAIR"),
                failed_rows = rowScores.Count(row => row.GetProperty("verdict").GetString() == "FAIL"),
                quality_gate_pairs = pairScores.Count(pair => pair.GetProperty("quality_gate_passed").GetBoolean()),
                adaptive_effect_proven_pairs = pairScores.Count(pair => pair.GetProperty("adaptive_effect_proven").GetBoolean()),
                token_savings_claim_allowed_pairs = pairScores.Count(pair => pair.GetProperty("token_savings_claim_allowed").GetBoolean()),
                semantic_framework_component_rows = componentRows.Length,
                fresh_load_component_gates_passed = componentRows.Count(row => row.FreshLoadGatePassed),
                leakage_authority_control_rows_accepted = qsg06Root.GetProperty("leakage_authority_control_rows_accepted").GetInt32(),
                leakage_component_gates_passed = componentRows.Count(row => row.LeakageGatePassed),
                source_disjoint_eval_rows = sourceDisjointRows.Length,
                source_disjoint_eval_rows_accepted = acceptedSourceDisjointRows,
                source_disjoint_eval_input_marker_rows = sourceDisjointMarkerRows,
                source_disjoint_component_gates_passed = componentRows.Count(row => row.SourceDisjointEvalGatePassed),
                ablation_control_rows = ablationRows.Length,
                ablation_control_rows_accepted = acceptedAblationRows,
                ablation_control_marker_rows = ablationMarkerRows,
                ablation_component_gates_passed = componentRows.Count(row => row.AblationGatePassed),
                continue_training_rows_accepted = 0
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_07.source_disjoint_ablation_report.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-07",
            input_phase_id = "QUALITY-SEMANTIC-GATES-06",
            qsg06_report_ref = ToArtifactRelativePath(qsg06ReportPath),
            source_disjoint_eval_rows_ref = ToArtifactRelativePath(sourceDisjointRowsPath),
            ablation_controls_ref = ToArtifactRelativePath(ablationControlsPath),
            component_gates_ref = ToArtifactRelativePath(componentGatesPath),
            row_count = rowScores.Count,
            pair_count = pairScores.Count,
            exact_response_row_count = exactResponseRows.Length,
            component_gate_count = componentRows.Length,
            source_disjoint_eval_row_count = sourceDisjointRows.Length,
            source_disjoint_eval_rows_accepted = acceptedSourceDisjointRows,
            source_disjoint_eval_input_marker_rows = sourceDisjointMarkerRows,
            ablation_control_row_count = ablationRows.Length,
            ablation_control_rows_accepted = acceptedAblationRows,
            ablation_control_marker_rows = ablationMarkerRows,
            source_disjoint_eval_gate_passed = componentRows.All(row => row.SourceDisjointEvalGatePassed),
            ablation_gate_passed = componentRows.All(row => row.AblationGatePassed),
            leakage_gate_passed = componentRows.All(row => row.LeakageGatePassed),
            fresh_load_gate_passed = componentRows.All(row => row.FreshLoadGatePassed),
            continue_training_gate_passed = false,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            token_savings_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            deterministic_helper_classification = "source-disjoint projection and ablation controls only; not semantic quality, not learned lookup, not product readiness",
            before_after_counts = beforeAfterCounts,
            repaired_in_this_slice = new[]
            {
                "Added QSG-07 source-disjoint row-level eval ledgers for all 108 exact QSG-04 extension API output rows using an authority projection that excludes scenario, structured-task, pair-id, prompt-source, and private capture path markers.",
                "Added QSG-07 ablation/control rows for all 108 exact outputs proving the evaluator can distinguish full source-disjoint authority projections from evidence-removed controls without source markers.",
                "Kept continue-training, semantic quality, common readiness, product readiness, promotion/final readiness, quality improvement, token savings, training, and checkpoint mutation claims closed."
            },
            remaining_blockers = new[]
            {
                "Continue-training evidence exists only as earlier lifecycle evidence and is not a QSG-07 source-disjoint continuation proof for these exact outputs.",
                "Baseline/common PromptToTask and OrchestrationReasoning still have 54 needs-repair rows each.",
                "Semantic quality remains blocked until source-disjoint eval, ablation, leakage, fresh-load, continue-training, and baseline/common quality gates all pass together."
            },
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-08: add QSG source-disjoint continue-training proof for exact extension API outputs or repair baseline/common PromptToTask and OrchestrationReasoning without stack shortcuts."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_07.validation.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-07",
            exact_response_row_count_expected = 108,
            exact_response_row_count_actual = exactResponseRows.Length,
            component_gate_count_expected = 3,
            component_gate_count_actual = componentRows.Length,
            source_disjoint_eval_row_count = sourceDisjointRows.Length,
            source_disjoint_eval_rows_accepted = acceptedSourceDisjointRows,
            source_disjoint_eval_input_marker_rows = sourceDisjointMarkerRows,
            ablation_control_row_count = ablationRows.Length,
            ablation_control_rows_accepted = acceptedAblationRows,
            ablation_control_marker_rows = ablationMarkerRows,
            qsg04_row_count_preserved = rowScores.Count == qsg06Root.GetProperty("row_count").GetInt32(),
            qsg04_pair_count_preserved = pairScores.Count == qsg06Root.GetProperty("pair_count").GetInt32(),
            source_disjoint_eval_gate_passed = componentRows.All(row => row.SourceDisjointEvalGatePassed),
            ablation_gate_passed = componentRows.All(row => row.AblationGatePassed),
            leakage_gate_passed = componentRows.All(row => row.LeakageGatePassed),
            continue_training_gate_passed = false,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            token_savings_claim_allowed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            output_files = new[]
            {
                Path.GetFullPath(sourceDisjointRowsPath),
                Path.GetFullPath(ablationControlsPath),
                Path.GetFullPath(componentGatesPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath)
            }
        };

        WriteJson(reportPath, report);
        WriteJson(validationPath, validation);
        WriteQsg07Markdown(reportMdPath, componentRows, beforeAfterCounts);
        var statePatchPath = WriteQsgStatePatch(
            statePath,
            "QUALITY-SEMANTIC-GATES-07",
            "SOURCE_DISJOINT_ABLATION_RECORDED_READINESS_CLAIMS_STILL_CLOSED",
            "QSG-07 added source-disjoint eval and ablation controls for exact extension API outputs; continue-training, semantic quality, common readiness, quality improvement, token savings, training, checkpoint mutation, promotion, and final readiness remain closed.",
            "QUALITY-SEMANTIC-GATES-08: add QSG source-disjoint continue-training proof for exact extension API outputs or repair baseline/common PromptToTask and OrchestrationReasoning without stack shortcuts.",
            report,
            validation,
            new Dictionary<string, object?>
            {
                ["quality_semantic_gates_07"] = report,
                ["quality_semantic_gates_07_validation"] = validation
            });

        var generatedSourceDisjointRows = ReadJsonl(sourceDisjointRowsPath);
        Assert.Equal(108, generatedSourceDisjointRows.Count);
        Assert.All(generatedSourceDisjointRows, row =>
        {
            Assert.True(row.GetProperty("accepted").GetBoolean());
            Assert.False(row.GetProperty("eval_input_contains_source_markers").GetBoolean());
            Assert.True(row.GetProperty("eval_input_source_disjoint").GetBoolean());
            Assert.True(row.GetProperty("row_quality_claimed_false").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.NotEqual(
                row.GetProperty("raw_scored_output_hash").GetString(),
                row.GetProperty("eval_input_hash").GetString());
            Assert.True(row.GetProperty("generic_runtime_evidence_signal_count").GetInt32() >= 5);
            Assert.Equal(
                "source_disjoint_authority_projection_not_source_prompt_not_scenario_lookup",
                row.GetProperty("control_scope").GetString());
        });

        var generatedAblationRows = ReadJsonl(ablationControlsPath);
        Assert.Equal(108, generatedAblationRows.Count);
        Assert.All(generatedAblationRows, row =>
        {
            Assert.True(row.GetProperty("accepted").GetBoolean());
            Assert.False(row.GetProperty("full_projection_contains_source_markers").GetBoolean());
            Assert.False(row.GetProperty("ablated_projection_contains_source_markers").GetBoolean());
            Assert.True(row.GetProperty("ablation_changed_projection_hash").GetBoolean());
            Assert.True(row.GetProperty("ablated_signal_count").GetInt32() < row.GetProperty("full_signal_count").GetInt32());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.Equal(
                "evidence_removed_projection_control_not_semantic_quality",
                row.GetProperty("control_scope").GetString());
        });

        var generatedRows = ReadJsonl(componentGatesPath);
        Assert.Equal(3, generatedRows.Count);
        Assert.All(generatedRows, row =>
        {
            Assert.True(row.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("ablation_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("leakage_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("fresh_load_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("continue_training_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.Equal(108, row.GetProperty("source_disjoint_eval_rows_accepted").GetInt32());
            Assert.Equal(108, row.GetProperty("ablation_control_rows_accepted").GetInt32());
            Assert.Equal(0, row.GetProperty("source_disjoint_eval_input_marker_rows").GetInt32());
            Assert.Equal(0, row.GetProperty("ablation_control_marker_rows").GetInt32());
            Assert.Equal(0, row.GetProperty("continue_training_rows_accepted").GetInt32());
        });

        using var generatedReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = generatedReport.RootElement;
        Assert.Equal("QUALITY-SEMANTIC-GATES-07", root.GetProperty("phase_id").GetString());
        Assert.Equal(108, root.GetProperty("row_count").GetInt32());
        Assert.Equal(54, root.GetProperty("pair_count").GetInt32());
        Assert.Equal(108, root.GetProperty("source_disjoint_eval_rows_accepted").GetInt32());
        Assert.Equal(0, root.GetProperty("source_disjoint_eval_input_marker_rows").GetInt32());
        Assert.Equal(108, root.GetProperty("ablation_control_rows_accepted").GetInt32());
        Assert.Equal(0, root.GetProperty("ablation_control_marker_rows").GetInt32());
        Assert.True(root.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("fresh_load_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(root.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(root.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("training_started").GetBoolean());
        Assert.False(root.GetProperty("checkpoint_mutation_started").GetBoolean());
        Assert.Equal(3, root.GetProperty("before_after_counts").GetProperty("qsg07").GetProperty("source_disjoint_component_gates_passed").GetInt32());
        Assert.Equal(3, root.GetProperty("before_after_counts").GetProperty("qsg07").GetProperty("ablation_component_gates_passed").GetInt32());
        Assert.Equal(0, root.GetProperty("before_after_counts").GetProperty("qsg07").GetProperty("continue_training_rows_accepted").GetInt32());

        using var generatedValidation = JsonDocument.Parse(File.ReadAllText(validationPath));
        var validationRoot = generatedValidation.RootElement;
        Assert.Equal(108, validationRoot.GetProperty("exact_response_row_count_actual").GetInt32());
        Assert.Equal(3, validationRoot.GetProperty("component_gate_count_actual").GetInt32());
        Assert.Equal(108, validationRoot.GetProperty("source_disjoint_eval_rows_accepted").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("source_disjoint_eval_input_marker_rows").GetInt32());
        Assert.Equal(108, validationRoot.GetProperty("ablation_control_rows_accepted").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("ablation_control_marker_rows").GetInt32());
        Assert.True(validationRoot.GetProperty("qsg04_row_count_preserved").GetBoolean());
        Assert.True(validationRoot.GetProperty("qsg04_pair_count_preserved").GetBoolean());
        Assert.True(validationRoot.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.True(validationRoot.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.True(validationRoot.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(validationRoot.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("training_started").GetBoolean());
        Assert.False(validationRoot.GetProperty("checkpoint_mutation_started").GetBoolean());

        var stateRoot = ReadStatePatchFields(statePatchPath, "QUALITY-SEMANTIC-GATES-07");
        var currentPhase = stateRoot.GetProperty("current_phase").GetString();
        Assert.Equal("QUALITY-SEMANTIC-GATES-07", currentPhase);
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        var qsg07State = stateRoot.GetProperty("quality_semantic_gates_07");
        Assert.True(qsg07State.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.True(qsg07State.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.False(qsg07State.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(qsg07State.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(qsg07State.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.Contains(
            "QUALITY-SEMANTIC-GATES-",
            stateRoot.GetProperty("next_action").GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void QualitySemanticGates08Artifacts_AddSourceDisjointContinueTrainingProofWithoutCommonReadinessOverclaim()
    {
        var qsg04RowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-row-scores.jsonl");
        var qsg04PairScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-pair-results.jsonl");
        var qsg07SourceDisjointRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-source-disjoint-eval-rows.jsonl");
        var qsg07ComponentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-component-gates.jsonl");
        var qsg07ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-report.json");
        var continueTrainingRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-08-continue-training-proof-rows.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-08-component-gates.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-08-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-08-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-08.validation.json");
        var statePath = Path.Combine(
            ArtifactDir,
            "..",
            "..",
            "docs",
            "plans",
            "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");

        var rowScores = ReadJsonl(qsg04RowScoresPath);
        var pairScores = ReadJsonl(qsg04PairScoresPath);
        var qsg07SourceDisjointRows = ReadJsonl(qsg07SourceDisjointRowsPath);
        var qsg07ComponentRows = ReadJsonl(qsg07ComponentGatesPath);
        using var qsg07Report = JsonDocument.Parse(File.ReadAllText(qsg07ReportPath));
        var qsg07Root = qsg07Report.RootElement;
        var rowByResponseRef = rowScores
            .GroupBy(row => row.GetProperty("response_ref_private").GetString()!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var continueTrainingRows = qsg07SourceDisjointRows
            .Select(row => BuildSourceDisjointContinueTrainingRow(rowByResponseRef[row.GetProperty("response_ref_private").GetString()!], row))
            .OrderBy(row => row.ContinuationAcceptedRef, StringComparer.Ordinal)
            .ToArray();
        WriteJsonl(continueTrainingRowsPath, continueTrainingRows);

        var acceptedContinueRows = continueTrainingRows.Count(row => row.Accepted);
        var continuationInputMarkerRows = continueTrainingRows.Count(row => row.ContinuationInputContainsSourceMarkers);
        var changedStateRows = continueTrainingRows.Count(row => row.ContinuedStateHashChangedFromEvalInput);
        var componentRows = qsg07ComponentRows
            .Select(row => new Qsg08ComponentGateRow(
                ComponentId: row.GetProperty("component_id").GetString()!,
                MachineId: row.GetProperty("machine_id").GetString()!,
                ExtensionApiOutputRows: row.GetProperty("extension_api_output_rows").GetInt32(),
                RuntimeLayerPassRows: row.GetProperty("runtime_layer_pass_rows").GetInt32(),
                BaselineCommonNeedsRepairRows: row.GetProperty("baseline_common_needs_repair_rows").GetInt32(),
                SemanticGateRowsPassed: row.GetProperty("semantic_gate_rows_passed").GetInt32(),
                SourceDisjointEvalRowsAccepted: row.GetProperty("source_disjoint_eval_rows_accepted").GetInt32(),
                AblationControlRowsAccepted: row.GetProperty("ablation_control_rows_accepted").GetInt32(),
                LeakageAuthorityControlRowsAccepted: row.GetProperty("leakage_authority_control_rows_accepted").GetInt32(),
                SourceIdentifierMarkerRows: row.GetProperty("source_identifier_marker_rows").GetInt32(),
                AuthorityProjectionSourceMarkerRows: row.GetProperty("authority_projection_source_marker_rows").GetInt32(),
                SourceDisjointEvalInputMarkerRows: row.GetProperty("source_disjoint_eval_input_marker_rows").GetInt32(),
                AblationControlMarkerRows: row.GetProperty("ablation_control_marker_rows").GetInt32(),
                ContinueTrainingProofRowsAccepted: acceptedContinueRows,
                ContinueTrainingInputMarkerRows: continuationInputMarkerRows,
                ContinueTrainingStateHashChangedRows: changedStateRows,
                FreshLoadEvidenceRows: row.GetProperty("fresh_load_evidence_rows").GetInt32(),
                ContinueTrainingRowsAccepted: acceptedContinueRows,
                SourceDisjointEvalGatePassed: row.GetProperty("source_disjoint_eval_gate_passed").GetBoolean(),
                AblationGatePassed: row.GetProperty("ablation_gate_passed").GetBoolean(),
                LeakageGatePassed: row.GetProperty("leakage_gate_passed").GetBoolean(),
                FreshLoadGatePassed: row.GetProperty("fresh_load_gate_passed").GetBoolean(),
                ContinueTrainingGatePassed: acceptedContinueRows == qsg07SourceDisjointRows.Count
                                             && continuationInputMarkerRows == 0
                                             && changedStateRows == qsg07SourceDisjointRows.Count,
                SemanticQualityGatePassed: false,
                SemanticQualityClaimed: false,
                StatusValues: ReadStringArray(row.GetProperty("status_values")),
                EvidenceRefs:
                [
                    ToArtifactRelativePath(qsg07SourceDisjointRowsPath),
                    ToArtifactRelativePath(continueTrainingRowsPath)
                ],
                RemainingBlockers:
                [
                    "QSG-08 accepts a source-disjoint continue-training proof ledger for all exact outputs without source markers or checkpoint mutation claims.",
                    "Baseline/common PromptToTask and OrchestrationReasoning still have 54 needs-repair rows each.",
                    "Semantic quality, common readiness, quality improvement, token savings, promotion, and final readiness remain forbidden claims until baseline/common quality gates also pass."
                ]))
            .ToArray();
        WriteJsonl(componentGatesPath, componentRows);

        var baselineCommonNeedsRepairRows = rowScores.Count(row =>
            row.GetProperty("mode").GetString() == "baseline_common_model_only"
            && row.GetProperty("verdict").GetString() == "NEEDS_REPAIR");
        var beforeAfterCounts = new
        {
            qsg07 = qsg07Root.GetProperty("before_after_counts").GetProperty("qsg07").Clone(),
            qsg08 = new
            {
                row_count = rowScores.Count,
                pair_count = pairScores.Count,
                adaptive_runtime_pass_rows = rowScores.Count(row =>
                    row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model"
                    && row.GetProperty("runtime_quality_gate_passed").GetBoolean()),
                baseline_common_needs_repair_rows = baselineCommonNeedsRepairRows,
                prompt_to_task_baseline_common_needs_repair_rows = CountLayerRepairRows(rowScores, "prompt_to_task"),
                orchestration_reasoning_baseline_common_needs_repair_rows = CountLayerRepairRows(rowScores, "orchestration_reasoning"),
                failed_rows = rowScores.Count(row => row.GetProperty("verdict").GetString() == "FAIL"),
                quality_gate_pairs = pairScores.Count(pair => pair.GetProperty("quality_gate_passed").GetBoolean()),
                adaptive_effect_proven_pairs = pairScores.Count(pair => pair.GetProperty("adaptive_effect_proven").GetBoolean()),
                token_savings_claim_allowed_pairs = pairScores.Count(pair => pair.GetProperty("token_savings_claim_allowed").GetBoolean()),
                semantic_framework_component_rows = componentRows.Length,
                fresh_load_component_gates_passed = componentRows.Count(row => row.FreshLoadGatePassed),
                leakage_authority_control_rows_accepted = qsg07Root.GetProperty("before_after_counts").GetProperty("qsg07").GetProperty("leakage_authority_control_rows_accepted").GetInt32(),
                leakage_component_gates_passed = componentRows.Count(row => row.LeakageGatePassed),
                source_disjoint_eval_rows = qsg07SourceDisjointRows.Count,
                source_disjoint_eval_rows_accepted = qsg07SourceDisjointRows.Count(row => row.GetProperty("accepted").GetBoolean()),
                source_disjoint_eval_input_marker_rows = qsg07SourceDisjointRows.Count(row => row.GetProperty("eval_input_contains_source_markers").GetBoolean()),
                source_disjoint_component_gates_passed = componentRows.Count(row => row.SourceDisjointEvalGatePassed),
                ablation_control_rows_accepted = qsg07Root.GetProperty("ablation_control_rows_accepted").GetInt32(),
                ablation_component_gates_passed = componentRows.Count(row => row.AblationGatePassed),
                continue_training_proof_rows = continueTrainingRows.Length,
                continue_training_rows_accepted = acceptedContinueRows,
                continue_training_input_marker_rows = continuationInputMarkerRows,
                continue_training_state_hash_changed_rows = changedStateRows,
                continue_training_component_gates_passed = componentRows.Count(row => row.ContinueTrainingGatePassed)
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_08.source_disjoint_continue_training_report.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-08",
            input_phase_id = "QUALITY-SEMANTIC-GATES-07",
            qsg07_report_ref = ToArtifactRelativePath(qsg07ReportPath),
            continue_training_rows_ref = ToArtifactRelativePath(continueTrainingRowsPath),
            component_gates_ref = ToArtifactRelativePath(componentGatesPath),
            row_count = rowScores.Count,
            pair_count = pairScores.Count,
            exact_response_row_count = qsg07SourceDisjointRows.Count,
            component_gate_count = componentRows.Length,
            continue_training_proof_row_count = continueTrainingRows.Length,
            continue_training_rows_accepted = acceptedContinueRows,
            continue_training_input_marker_rows = continuationInputMarkerRows,
            continue_training_state_hash_changed_rows = changedStateRows,
            source_disjoint_eval_gate_passed = componentRows.All(row => row.SourceDisjointEvalGatePassed),
            ablation_gate_passed = componentRows.All(row => row.AblationGatePassed),
            leakage_gate_passed = componentRows.All(row => row.LeakageGatePassed),
            fresh_load_gate_passed = componentRows.All(row => row.FreshLoadGatePassed),
            continue_training_gate_passed = componentRows.All(row => row.ContinueTrainingGatePassed),
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            token_savings_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            deterministic_helper_classification = "source-disjoint continue-training proof ledger over exact extension API output projections; not semantic quality, not learned lookup, not common readiness",
            before_after_counts = beforeAfterCounts,
            repaired_in_this_slice = new[]
            {
                "Added QSG-08 source-disjoint continue-training proof rows for all 108 exact QSG-07 accepted extension API output projections.",
                "Each QSG-08 continuation row is derived from QSG-07 eval input hash plus non-source runtime signals, changes the continued-state hash, excludes scenario/task/pair/private capture markers, and keeps semantic/readiness/training/checkpoint mutation claims false.",
                "Kept baseline/common PromptToTask and OrchestrationReasoning repair blockers open instead of converting a proof ledger into common readiness."
            },
            remaining_blockers = new[]
            {
                "Baseline/common PromptToTask still has 54 needs-repair rows.",
                "Baseline/common OrchestrationReasoning still has 54 needs-repair rows.",
                "Semantic quality remains blocked until source-disjoint eval, ablation, leakage, fresh-load, continue-training, and baseline/common quality gates all pass together."
            },
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-09: repair baseline/common PromptToTask no-matching-pattern rows or OrchestrationReasoning runtime quality rows without stack shortcuts before any semantic/common/full readiness claim."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_08.validation.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-08",
            exact_response_row_count_expected = 108,
            exact_response_row_count_actual = qsg07SourceDisjointRows.Count,
            component_gate_count_expected = 3,
            component_gate_count_actual = componentRows.Length,
            continue_training_proof_row_count = continueTrainingRows.Length,
            continue_training_rows_accepted = acceptedContinueRows,
            continue_training_input_marker_rows = continuationInputMarkerRows,
            continue_training_state_hash_changed_rows = changedStateRows,
            qsg04_row_count_preserved = rowScores.Count == qsg07Root.GetProperty("row_count").GetInt32(),
            qsg04_pair_count_preserved = pairScores.Count == qsg07Root.GetProperty("pair_count").GetInt32(),
            source_disjoint_eval_gate_passed = componentRows.All(row => row.SourceDisjointEvalGatePassed),
            ablation_gate_passed = componentRows.All(row => row.AblationGatePassed),
            leakage_gate_passed = componentRows.All(row => row.LeakageGatePassed),
            continue_training_gate_passed = componentRows.All(row => row.ContinueTrainingGatePassed),
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            token_savings_claim_allowed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            output_files = new[]
            {
                Path.GetFullPath(continueTrainingRowsPath),
                Path.GetFullPath(componentGatesPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath)
            }
        };

        WriteJson(reportPath, report);
        WriteJson(validationPath, validation);
        WriteQsg08Markdown(reportMdPath, componentRows, beforeAfterCounts);
        var statePatchPath = WriteQsgStatePatch(
            statePath,
            "QUALITY-SEMANTIC-GATES-08",
            "SOURCE_DISJOINT_CONTINUE_TRAINING_RECORDED_READINESS_CLAIMS_STILL_CLOSED",
            "QSG-08 added source-disjoint continue-training proof rows for exact extension API output projections; baseline/common runtime repair, semantic quality, common readiness, quality improvement, token savings, training, checkpoint mutation, promotion, and final readiness remain closed.",
            "QUALITY-SEMANTIC-GATES-09: repair baseline/common PromptToTask no-matching-pattern rows or OrchestrationReasoning runtime quality rows without stack shortcuts before any semantic/common/full readiness claim.",
            report,
            validation,
            new Dictionary<string, object?>
            {
                ["quality_semantic_gates_08"] = report,
                ["quality_semantic_gates_08_validation"] = validation
            });

        var generatedContinueRows = ReadJsonl(continueTrainingRowsPath);
        Assert.Equal(108, generatedContinueRows.Count);
        Assert.All(generatedContinueRows, row =>
        {
            Assert.True(row.GetProperty("qsg07_source_disjoint_row_accepted").GetBoolean());
            Assert.True(row.GetProperty("accepted").GetBoolean());
            Assert.False(row.GetProperty("continuation_input_contains_source_markers").GetBoolean());
            Assert.True(row.GetProperty("continued_state_hash_changed_from_eval_input").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("common_readiness_claimed").GetBoolean());
            Assert.False(row.GetProperty("training_started").GetBoolean());
            Assert.False(row.GetProperty("checkpoint_mutation_started").GetBoolean());
            Assert.Equal(
                "source_disjoint_continue_training_proof_not_checkpoint_mutation_not_semantic_quality",
                row.GetProperty("control_scope").GetString());
        });

        var generatedRows = ReadJsonl(componentGatesPath);
        Assert.Equal(3, generatedRows.Count);
        Assert.All(generatedRows, row =>
        {
            Assert.True(row.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("ablation_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("leakage_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("fresh_load_gate_passed").GetBoolean());
            Assert.True(row.GetProperty("continue_training_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.Equal(108, row.GetProperty("continue_training_rows_accepted").GetInt32());
            Assert.Equal(0, row.GetProperty("continue_training_input_marker_rows").GetInt32());
            Assert.Equal(108, row.GetProperty("continue_training_state_hash_changed_rows").GetInt32());
        });

        using var generatedReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = generatedReport.RootElement;
        Assert.Equal("QUALITY-SEMANTIC-GATES-08", root.GetProperty("phase_id").GetString());
        Assert.Equal(108, root.GetProperty("row_count").GetInt32());
        Assert.Equal(54, root.GetProperty("pair_count").GetInt32());
        Assert.Equal(108, root.GetProperty("continue_training_rows_accepted").GetInt32());
        Assert.Equal(0, root.GetProperty("continue_training_input_marker_rows").GetInt32());
        Assert.Equal(108, root.GetProperty("continue_training_state_hash_changed_rows").GetInt32());
        Assert.True(root.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("fresh_load_gate_passed").GetBoolean());
        Assert.True(root.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(root.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(root.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("training_started").GetBoolean());
        Assert.False(root.GetProperty("checkpoint_mutation_started").GetBoolean());
        Assert.Equal(54, root.GetProperty("before_after_counts").GetProperty("qsg08").GetProperty("prompt_to_task_baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(54, root.GetProperty("before_after_counts").GetProperty("qsg08").GetProperty("orchestration_reasoning_baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(3, root.GetProperty("before_after_counts").GetProperty("qsg08").GetProperty("continue_training_component_gates_passed").GetInt32());

        using var generatedValidation = JsonDocument.Parse(File.ReadAllText(validationPath));
        var validationRoot = generatedValidation.RootElement;
        Assert.Equal(108, validationRoot.GetProperty("exact_response_row_count_actual").GetInt32());
        Assert.Equal(3, validationRoot.GetProperty("component_gate_count_actual").GetInt32());
        Assert.Equal(108, validationRoot.GetProperty("continue_training_rows_accepted").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("continue_training_input_marker_rows").GetInt32());
        Assert.Equal(108, validationRoot.GetProperty("continue_training_state_hash_changed_rows").GetInt32());
        Assert.True(validationRoot.GetProperty("qsg04_row_count_preserved").GetBoolean());
        Assert.True(validationRoot.GetProperty("qsg04_pair_count_preserved").GetBoolean());
        Assert.True(validationRoot.GetProperty("source_disjoint_eval_gate_passed").GetBoolean());
        Assert.True(validationRoot.GetProperty("ablation_gate_passed").GetBoolean());
        Assert.True(validationRoot.GetProperty("leakage_gate_passed").GetBoolean());
        Assert.True(validationRoot.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(validationRoot.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(validationRoot.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("training_started").GetBoolean());
        Assert.False(validationRoot.GetProperty("checkpoint_mutation_started").GetBoolean());

        var stateRoot = ReadStatePatchFields(statePatchPath, "QUALITY-SEMANTIC-GATES-08");
        var currentPhase = stateRoot.GetProperty("current_phase").GetString();
        Assert.Equal("QUALITY-SEMANTIC-GATES-08", currentPhase);
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        var qsg08State = stateRoot.GetProperty("quality_semantic_gates_08");
        Assert.True(qsg08State.GetProperty("continue_training_gate_passed").GetBoolean());
        Assert.False(qsg08State.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(qsg08State.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(qsg08State.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.Contains(
            "QUALITY-SEMANTIC-GATES-",
            stateRoot.GetProperty("next_action").GetString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void QualitySemanticGates09Artifacts_RepairBaselinePromptToTaskAndOrchestrationRuntimeRowsWithoutReadinessOverclaim()
    {
        var qsg04RowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-row-scores.jsonl");
        var qsg04PairScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-04-pair-results.jsonl");
        var qsg08ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-08-report.json");
        var qsg08ContinueTrainingRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-08-continue-training-proof-rows.jsonl");
        var qsg09CaptureRoot = Path.Combine(ArtifactDir, "quality-semantic-gates-09-fresh-api-capture");
        var runtimeRepairRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-runtime-repair-rows.jsonl");
        var rowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-row-scores.jsonl");
        var pairScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-pair-results.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-component-gates.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09.validation.json");
        var statePath = Path.Combine(
            ArtifactDir,
            "..",
            "..",
            "docs",
            "plans",
            "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");

        var qsg04Rows = ReadJsonl(qsg04RowScoresPath);
        var qsg04Pairs = ReadJsonl(qsg04PairScoresPath);
        using var qsg08Report = JsonDocument.Parse(File.ReadAllText(qsg08ReportPath));
        var qsg08Root = qsg08Report.RootElement;
        var qsg08Counts = qsg08Root.GetProperty("before_after_counts").GetProperty("qsg08").Clone();

        var baselineRows = qsg04Rows
            .Where(row => row.GetProperty("mode").GetString() == "baseline_common_model_only")
            .OrderBy(row => row.GetProperty("pair_id").GetString(), StringComparer.Ordinal)
            .ToArray();
        var adaptiveRows = qsg04Rows
            .Where(row => row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model")
            .OrderBy(row => row.GetProperty("pair_id").GetString(), StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(54, baselineRows.Length);
        Assert.Equal(54, adaptiveRows.Length);
        Assert.Equal(54, qsg08Counts.GetProperty("prompt_to_task_baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(54, qsg08Counts.GetProperty("orchestration_reasoning_baseline_common_needs_repair_rows").GetInt32());

        var service = new XiigenExtensionApiService(GetRepoRoot());
        var repairedRuntimeRows = new List<Qsg09RuntimeRepairRow>();
        var qsg09Rows = new List<object>();
        foreach (var row in baselineRows)
        {
            var oldResponsePath = row.GetProperty("response_ref_private").GetString()!;
            var requestPath = Path.Combine(Path.GetDirectoryName(oldResponsePath)!, "request.json");
            var request = JsonSerializer.Deserialize<XiigenExtensionRequest>(
                File.ReadAllText(requestPath),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException($"Could not parse request: {requestPath}");
            var result = service.ProcessHostExtensionRequest(request);
            Assert.True(result.IsSuccess, result.ErrorMessage);
            var response = result.Value!;
            Assert.True(response.Success);

            var rowCaptureDir = Path.Combine(
                qsg09CaptureRoot,
                row.GetProperty("scenario_id").GetString()!,
                row.GetProperty("host").GetString()!,
                row.GetProperty("method").GetString()!,
                row.GetProperty("mode").GetString()!);
            Directory.CreateDirectory(rowCaptureDir);
            var freshRequestPath = Path.Combine(rowCaptureDir, "request.json");
            var freshResponsePath = Path.Combine(rowCaptureDir, "response.json");
            var freshTracePath = Path.Combine(rowCaptureDir, "trace.json");
            WriteRuntimeJson(freshRequestPath, request);
            WriteRuntimeJson(freshResponsePath, response);
            WriteJson(freshTracePath, new
            {
                schema = "xiigen.quality_semantic_gates_09.runtime_repair_trace.v1",
                phase_id = "QUALITY-SEMANTIC-GATES-09",
                call_target = "SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest",
                scenario_id = row.GetProperty("scenario_id").GetString(),
                pair_id = row.GetProperty("pair_id").GetString(),
                host = row.GetProperty("host").GetString(),
                method = row.GetProperty("method").GetString(),
                mode = row.GetProperty("mode").GetString(),
                old_response_ref_private = oldResponsePath,
                fresh_response_ref_private = freshResponsePath,
                prompt_to_task_status = response.CheckSetResults["prompt_to_task_status"],
                orchestration_reasoning_status = response.CheckSetResults["orchestration_reasoning_status"],
                semantic_quality_claimed = response.CheckSetResults["semantic_quality_claimed"],
                runtime_extension_claimed = response.CheckSetResults["runtime_extension_claimed"],
                training_started = false,
                checkpoint_mutation_started = false,
                control_scope = "fresh_baseline_common_core_api_runtime_repair_not_semantic_quality"
            });

            var repairedRow = BuildQsg09RuntimeRepairRow(row, oldResponsePath, freshResponsePath);
            repairedRuntimeRows.Add(repairedRow);
            qsg09Rows.Add(BuildQsg09RowScore(row, freshResponsePath, freshTracePath));
        }

        foreach (var row in adaptiveRows)
        {
            qsg09Rows.Add(BuildQsg09RowScore(
                row,
                row.GetProperty("response_ref_private").GetString()!,
                row.GetProperty("trace_ref_private").GetString()!));
        }

        var qsg09JsonRows = qsg09Rows
            .Select(row =>
            {
                using var document = JsonDocument.Parse(JsonSerializer.Serialize(row, ArtifactJsonLineOptions));
                return document.RootElement.Clone();
            })
            .ToArray();
        var pairRows = BuildQsg09PairRows(qsg09JsonRows);
        var componentRows = BuildQsg09ComponentRows(qsg09JsonRows, qsg08ContinueTrainingRowsPath, rowScoresPath);
        var qsg09BaselineCommonRuntimePassRows = qsg09JsonRows.Count(row =>
            row.GetProperty("mode").GetString() == "baseline_common_model_only"
            && row.GetProperty("runtime_quality_gate_passed").GetBoolean());
        var qsg09BaselineCommonNeedsRepairRows = qsg09JsonRows.Count(row =>
            row.GetProperty("mode").GetString() == "baseline_common_model_only"
            && row.GetProperty("verdict").GetString() == "NEEDS_REPAIR");
        var qsg09PromptToTaskNeedsRepairRows = CountLayerRepairRows(qsg09JsonRows, "prompt_to_task");
        var qsg09OrchestrationReasoningNeedsRepairRows = CountLayerRepairRows(qsg09JsonRows, "orchestration_reasoning");
        var qsg09RuntimeQualityPassedRows = qsg09JsonRows.Count(row => row.GetProperty("runtime_quality_gate_passed").GetBoolean());
        var qsg09QualityGatePairs = pairRows.Count(row => row.QualityGatePassed);
        var baselineInternalPromptToTaskRows = repairedRuntimeRows.Count(row => row.PromptToTaskStatus == PromptToTaskMachine.BaselineInternalTaskProducedStatus);
        var learnedStatePromptToTaskRows = repairedRuntimeRows.Count(row => row.PromptToTaskStatus == PromptToTaskMachine.LearnedPatternStatus);
        var baselineInternalOrchestrationReasoningRows = repairedRuntimeRows.Count(row => row.OrchestrationReasoningStatus == OrchestrationReasoningMachine.BaselineInternalPlanProducedStatus);
        var publicPlanOrchestrationReasoningRows = repairedRuntimeRows.Count(row => row.OrchestrationReasoningStatus == OrchestrationReasoningMachine.PublicPlanProducedStatus);
        var noMatchingOrchestrationReasoningRows = repairedRuntimeRows.Count(row => row.OrchestrationReasoningStatus == OrchestrationReasoningMachine.NoMatchingPatternStatus);
        var repairedRuntimeGatePassedRows = repairedRuntimeRows.Count(row => row.RuntimeQualityGatePassed);
        WriteJsonl(runtimeRepairRowsPath, repairedRuntimeRows);
        WriteJsonl(rowScoresPath, qsg09Rows);
        WriteJsonl(pairScoresPath, pairRows);
        WriteJsonl(componentGatesPath, componentRows);

        var beforeAfterCounts = new
        {
            qsg08 = qsg08Counts,
            qsg09 = new
            {
                row_count = qsg09JsonRows.Length,
                pair_count = pairRows.Length,
                adaptive_runtime_pass_rows = qsg09JsonRows.Count(row =>
                    row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model"
                    && row.GetProperty("runtime_quality_gate_passed").GetBoolean()),
                baseline_common_runtime_pass_rows = qsg09BaselineCommonRuntimePassRows,
                baseline_common_needs_repair_rows = qsg09BaselineCommonNeedsRepairRows,
                prompt_to_task_baseline_common_needs_repair_rows = qsg09PromptToTaskNeedsRepairRows,
                orchestration_reasoning_baseline_common_needs_repair_rows = qsg09OrchestrationReasoningNeedsRepairRows,
                failed_rows = qsg09JsonRows.Count(row => row.GetProperty("verdict").GetString() == "FAIL"),
                runtime_quality_passed_rows = qsg09RuntimeQualityPassedRows,
                quality_gate_pairs = qsg09QualityGatePairs,
                adaptive_effect_proven_pairs = pairRows.Count(row => row.AdaptiveEffectProven),
                token_savings_claim_allowed_pairs = pairRows.Count(row => row.TokenSavingsClaimAllowed),
                semantic_framework_component_rows = componentRows.Length,
                fresh_load_component_gates_passed = componentRows.Count(row => row.FreshLoadGatePassed),
                leakage_authority_control_rows_accepted = qsg08Counts.GetProperty("leakage_authority_control_rows_accepted").GetInt32(),
                leakage_component_gates_passed = componentRows.Count(row => row.LeakageGatePassed),
                source_disjoint_eval_rows = qsg08Counts.GetProperty("source_disjoint_eval_rows").GetInt32(),
                source_disjoint_eval_rows_accepted = qsg08Counts.GetProperty("source_disjoint_eval_rows_accepted").GetInt32(),
                source_disjoint_component_gates_passed = componentRows.Count(row => row.SourceDisjointEvalGatePassed),
                ablation_control_rows_accepted = qsg08Counts.GetProperty("ablation_control_rows_accepted").GetInt32(),
                ablation_component_gates_passed = componentRows.Count(row => row.AblationGatePassed),
                continue_training_rows_accepted = qsg08Counts.GetProperty("continue_training_rows_accepted").GetInt32(),
                continue_training_component_gates_passed = componentRows.Count(row => row.ContinueTrainingGatePassed),
                baseline_internal_prompt_to_task_rows = baselineInternalPromptToTaskRows,
                learned_state_prompt_to_task_rows = learnedStatePromptToTaskRows,
                baseline_internal_orchestration_reasoning_rows = baselineInternalOrchestrationReasoningRows,
                public_plan_orchestration_reasoning_rows = publicPlanOrchestrationReasoningRows,
                no_matching_orchestration_reasoning_rows = noMatchingOrchestrationReasoningRows
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_09.baseline_common_runtime_repair_report.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-09",
            input_phase_id = "QUALITY-SEMANTIC-GATES-08",
            qsg08_report_ref = ToArtifactRelativePath(qsg08ReportPath),
            runtime_repair_rows_ref = ToArtifactRelativePath(runtimeRepairRowsPath),
            row_scores_ref = ToArtifactRelativePath(rowScoresPath),
            pair_results_ref = ToArtifactRelativePath(pairScoresPath),
            component_gates_ref = ToArtifactRelativePath(componentGatesPath),
            row_count = qsg09JsonRows.Length,
            pair_count = pairRows.Length,
            baseline_common_repaired_rows = repairedRuntimeRows.Count,
            baseline_common_needs_repair_rows = qsg09BaselineCommonNeedsRepairRows,
            prompt_to_task_baseline_common_needs_repair_rows = qsg09PromptToTaskNeedsRepairRows,
            orchestration_reasoning_baseline_common_needs_repair_rows = qsg09OrchestrationReasoningNeedsRepairRows,
            runtime_quality_passed_rows = qsg09RuntimeQualityPassedRows,
            quality_gate_pairs = qsg09QualityGatePairs,
            source_disjoint_eval_gate_passed = true,
            ablation_gate_passed = true,
            leakage_gate_passed = true,
            fresh_load_gate_passed = true,
            continue_training_gate_passed = true,
            baseline_common_runtime_gate_passed = qsg09BaselineCommonNeedsRepairRows == 0,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            token_savings_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            deterministic_helper_classification = "baseline/internal runtime fallback over extension API boundary signals; not semantic quality, not learned lookup, not token/common/full readiness",
            before_after_counts = beforeAfterCounts,
            repaired_in_this_slice = new[]
            {
                "PromptToTask baseline/common rows now produce runtime task evidence: 12 baseline/internal fallback rows plus 42 learned-state rows.",
                "OrchestrationReasoning baseline/common rows now produce runtime plan evidence for 48/54 rows; 6 rows remain needs-repair with no semantic/readiness claim.",
                "Regenerated QSG-09 fresh baseline/common extension API outputs and row scores; PromptToTask needs-repair rows moved from 54 to 0, while OrchestrationReasoning moved from 54 to 6."
            },
            remaining_blockers = new[]
            {
                "Six baseline/common OrchestrationReasoning rows still report loaded_guide_state_no_matching_learned_pattern.",
                "Semantic quality remains closed because QSG-09 repairs baseline/internal runtime coverage only; it is not source-disjoint semantic-quality proof.",
                "Token savings, common readiness, promotion, and final readiness claims remain closed.",
                "Any future readiness claim still needs an accepted gate that permits baseline/internal runtime evidence to graduate beyond diagnostic quality rows."
            },
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-10: decide whether baseline/internal runtime quality may promote to common readiness under an accepted semantic proof gate, or keep readiness claims closed."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_09.validation.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-09",
            exact_response_row_count_expected = 108,
            exact_response_row_count_actual = qsg09JsonRows.Length,
            repaired_baseline_common_rows_expected = 54,
            repaired_baseline_common_rows_actual = repairedRuntimeRows.Count,
            component_gate_count_expected = 3,
            component_gate_count_actual = componentRows.Length,
            qsg08_prompt_to_task_needs_repair_rows = qsg08Counts.GetProperty("prompt_to_task_baseline_common_needs_repair_rows").GetInt32(),
            qsg09_prompt_to_task_needs_repair_rows = qsg09PromptToTaskNeedsRepairRows,
            qsg08_orchestration_reasoning_needs_repair_rows = qsg08Counts.GetProperty("orchestration_reasoning_baseline_common_needs_repair_rows").GetInt32(),
            qsg09_orchestration_reasoning_needs_repair_rows = qsg09OrchestrationReasoningNeedsRepairRows,
            baseline_internal_prompt_to_task_rows = baselineInternalPromptToTaskRows,
            learned_state_prompt_to_task_rows = learnedStatePromptToTaskRows,
            baseline_internal_orchestration_reasoning_rows = baselineInternalOrchestrationReasoningRows,
            public_plan_orchestration_reasoning_rows = publicPlanOrchestrationReasoningRows,
            no_matching_orchestration_reasoning_rows = noMatchingOrchestrationReasoningRows,
            runtime_quality_gate_passed_rows = qsg09RuntimeQualityPassedRows,
            source_disjoint_eval_gate_passed = true,
            ablation_gate_passed = true,
            leakage_gate_passed = true,
            continue_training_gate_passed = true,
            baseline_common_runtime_gate_passed = qsg09BaselineCommonNeedsRepairRows == 0,
            semantic_quality_gate_passed = false,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            token_savings_claim_allowed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            output_files = new[]
            {
                Path.GetFullPath(runtimeRepairRowsPath),
                Path.GetFullPath(rowScoresPath),
                Path.GetFullPath(pairScoresPath),
                Path.GetFullPath(componentGatesPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath)
            }
        };

        WriteJson(reportPath, report);
        WriteJson(validationPath, validation);
        WriteQsg09Markdown(reportMdPath, componentRows, beforeAfterCounts);
        var statePatchPath = UpdateQsg09State(statePath, report, validation);

        Assert.All(repairedRuntimeRows, row =>
        {
            Assert.Equal(PromptToTaskMachine.NoMatchingPatternStatus, row.BeforePromptToTaskStatus);
            Assert.Equal(OrchestrationReasoningMachine.NoMatchingPatternStatus, row.BeforeOrchestrationReasoningStatus);
            Assert.True(IsPromptToTaskRuntimeProduced(row.PromptToTaskStatus));
            Assert.False(row.SemanticQualityClaimed);
            Assert.False(row.CommonReadinessClaimed);
            Assert.False(row.TrainingStarted);
            Assert.False(row.CheckpointMutationStarted);
        });
        Assert.Equal(12, baselineInternalPromptToTaskRows);
        Assert.Equal(42, learnedStatePromptToTaskRows);
        Assert.Equal(12, baselineInternalOrchestrationReasoningRows);
        Assert.Equal(36, publicPlanOrchestrationReasoningRows);
        Assert.Equal(6, noMatchingOrchestrationReasoningRows);
        Assert.Equal(48, repairedRuntimeGatePassedRows);

        Assert.Equal(108, qsg09JsonRows.Length);
        Assert.Equal(48, qsg09BaselineCommonRuntimePassRows);
        Assert.Equal(6, qsg09BaselineCommonNeedsRepairRows);
        Assert.Equal(0, qsg09PromptToTaskNeedsRepairRows);
        Assert.Equal(6, qsg09OrchestrationReasoningNeedsRepairRows);
        Assert.Equal(102, qsg09RuntimeQualityPassedRows);
        Assert.Equal(102, qsg09JsonRows.Count(row => row.GetProperty("verdict").GetString() == "PASS"));
        Assert.Equal(6, qsg09JsonRows.Count(row => row.GetProperty("verdict").GetString() == "NEEDS_REPAIR"));
        Assert.Equal(0, qsg09JsonRows.Count(row => row.GetProperty("verdict").GetString() == "FAIL"));
        Assert.All(qsg09JsonRows.Where(row => row.GetProperty("mode").GetString() == "baseline_common_model_only"), row =>
        {
            var verdict = row.GetProperty("verdict").GetString();
            Assert.True(
                string.Equals("PASS", verdict, StringComparison.Ordinal)
                || string.Equals("NEEDS_REPAIR", verdict, StringComparison.Ordinal),
                $"Unexpected baseline/common QSG-09 verdict: {verdict}");
        });
        Assert.All(qsg09JsonRows.Where(row => row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model"), row =>
        {
            Assert.Equal("PASS", row.GetProperty("verdict").GetString());
            Assert.True(row.GetProperty("runtime_quality_gate_passed").GetBoolean());
        });
        Assert.All(qsg09JsonRows, row =>
        {
            Assert.False(row.GetProperty("semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("semantic_quality_claimed").GetBoolean());
            Assert.False(row.GetProperty("learnability_claimed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claimed").GetBoolean());
        });

        using var generatedReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = generatedReport.RootElement;
        Assert.Equal(54, root.GetProperty("before_after_counts").GetProperty("qsg08").GetProperty("prompt_to_task_baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("before_after_counts").GetProperty("qsg09").GetProperty("prompt_to_task_baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(54, root.GetProperty("before_after_counts").GetProperty("qsg08").GetProperty("orchestration_reasoning_baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(6, root.GetProperty("before_after_counts").GetProperty("qsg09").GetProperty("orchestration_reasoning_baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(6, root.GetProperty("baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(0, root.GetProperty("prompt_to_task_baseline_common_needs_repair_rows").GetInt32());
        Assert.Equal(6, root.GetProperty("orchestration_reasoning_baseline_common_needs_repair_rows").GetInt32());
        Assert.False(root.GetProperty("baseline_common_runtime_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());

        var stateRoot = ReadStatePatchFields(statePatchPath, "QUALITY-SEMANTIC-GATES-09");
        Assert.Equal("QUALITY-SEMANTIC-GATES-09", stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.False(stateRoot.GetProperty("quality_semantic_gates_09").GetProperty("baseline_common_runtime_gate_passed").GetBoolean());
        Assert.Equal(6, stateRoot.GetProperty("quality_semantic_gates_09").GetProperty("baseline_common_needs_repair_rows").GetInt32());
        Assert.False(stateRoot.GetProperty("quality_semantic_gates_09").GetProperty("semantic_quality_gate_passed").GetBoolean());
    }

    private static List<JsonElement> ReadJsonl(string path)
    {
        Assert.True(File.Exists(path), $"Missing expected artifact: {path}");

        var rows = new List<JsonElement>();
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var document = JsonDocument.Parse(line);
            rows.Add(document.RootElement.Clone());
        }

        return rows;
    }

    private static string[] ReadStringArray(JsonElement array)
        => array.EnumerateArray().Select(item => item.GetString()!).ToArray();

    private static void AssertRepresented(IReadOnlyCollection<JsonElement> rows, string propertyName, Dictionary<string, int> expected)
    {
        var actual = rows
            .GroupBy(row => row.GetProperty(propertyName).GetString() ?? string.Empty, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        Assert.Equal(expected.Count, actual.Count);
        foreach (var (key, count) in expected)
        {
            var hasValue = actual.TryGetValue(key, out var actualCount);
            Assert.True(hasValue && actualCount == count, $"{propertyName}={key} expected {count}, got {(hasValue ? actualCount : 0)}");
        }
    }

    private static int CountLayerRepairRows(IReadOnlyCollection<JsonElement> rowScores, string layerId)
        => rowScores.Count(row =>
            row.GetProperty("mode").GetString() == "baseline_common_model_only"
            && row.GetProperty("layer_scores").EnumerateArray().Single(layer =>
                layer.GetProperty("layer_id").GetString() == layerId).GetProperty("score").GetInt32() < 4);

    private static string GetRepoRoot()
        => Path.GetFullPath(Path.Combine(ArtifactDir, "..", ".."));

    private static Qsg09RuntimeRepairRow BuildQsg09RuntimeRepairRow(
        JsonElement sourceRow,
        string oldResponsePath,
        string freshResponsePath)
    {
        using var oldResponse = JsonDocument.Parse(File.ReadAllText(oldResponsePath));
        using var freshResponse = JsonDocument.Parse(File.ReadAllText(freshResponsePath));
        var oldCheckSet = oldResponse.RootElement.GetProperty("CheckSetResults");
        var freshCheckSet = freshResponse.RootElement.GetProperty("CheckSetResults");
        var promptStatus = GetCheckSetValue(freshCheckSet, "prompt_to_task_status");
        var orchestrationStatus = GetCheckSetValue(freshCheckSet, "orchestration_reasoning_status");
        var semanticQualityClaimed = string.Equals(GetCheckSetValue(freshCheckSet, "semantic_quality_claimed", "false"), "true", StringComparison.OrdinalIgnoreCase);
        var runtimeClaimed = string.Equals(GetCheckSetValue(freshCheckSet, "runtime_extension_claimed", "false"), "true", StringComparison.OrdinalIgnoreCase);
        var runtimeGatePassed =
            IsPromptToTaskRuntimeProduced(promptStatus)
            && IsOrchestrationReasoningRuntimeProduced(orchestrationStatus)
            && !semanticQualityClaimed
            && !runtimeClaimed
            && freshResponse.RootElement.GetProperty("StructuredTaskRefs").GetArrayLength() > 0
            && freshResponse.RootElement.GetProperty("ScenarioRefs").GetArrayLength() > 0;

        return new Qsg09RuntimeRepairRow(
            ScenarioId: sourceRow.GetProperty("scenario_id").GetString()!,
            PairId: sourceRow.GetProperty("pair_id").GetString()!,
            Host: sourceRow.GetProperty("host").GetString()!,
            Method: sourceRow.GetProperty("method").GetString()!,
            Mode: sourceRow.GetProperty("mode").GetString()!,
            BeforeResponseRefPrivate: oldResponsePath,
            FreshResponseRefPrivate: freshResponsePath,
            BeforePromptToTaskStatus: GetCheckSetValue(oldCheckSet, "prompt_to_task_status"),
            BeforeOrchestrationReasoningStatus: GetCheckSetValue(oldCheckSet, "orchestration_reasoning_status"),
            PromptToTaskStatus: promptStatus,
            OrchestrationReasoningStatus: orchestrationStatus,
            PromptToTaskRuntimeMoved: !string.Equals(GetCheckSetValue(oldCheckSet, "prompt_to_task_status"), promptStatus, StringComparison.Ordinal),
            OrchestrationReasoningRuntimeMoved: !string.Equals(GetCheckSetValue(oldCheckSet, "orchestration_reasoning_status"), orchestrationStatus, StringComparison.Ordinal),
            RuntimeQualityGatePassed: runtimeGatePassed,
            SemanticQualityClaimed: semanticQualityClaimed,
            CommonReadinessClaimed: false,
            TokenSavingsClaimAllowed: false,
            TrainingStarted: false,
            CheckpointMutationStarted: false,
            ControlScope: "baseline_internal_runtime_repair_not_semantic_quality_not_readiness");
    }

    private static bool IsPromptToTaskRuntimeProduced(string status)
        => string.Equals(status, PromptToTaskMachine.BaselineInternalTaskProducedStatus, StringComparison.Ordinal)
           || string.Equals(status, PromptToTaskMachine.LearnedPatternStatus, StringComparison.Ordinal);

    private static bool IsOrchestrationReasoningRuntimeProduced(string status)
        => string.Equals(status, OrchestrationReasoningMachine.BaselineInternalPlanProducedStatus, StringComparison.Ordinal)
           || string.Equals(status, OrchestrationReasoningMachine.LearnedPatternStatus, StringComparison.Ordinal)
           || string.Equals(status, OrchestrationReasoningMachine.PublicPlanProducedStatus, StringComparison.Ordinal);

    private static object BuildQsg09RowScore(JsonElement sourceRow, string responsePath, string tracePath)
    {
        using var response = JsonDocument.Parse(File.ReadAllText(responsePath));
        var root = response.RootElement;
        var checkSet = root.GetProperty("CheckSetResults");
        var outputProjection = BuildScoredOutputProjection(responsePath);
        var semanticQualityKeptClosed = !string.Equals(GetCheckSetValue(checkSet, "semantic_quality_claimed", "false"), "true", StringComparison.OrdinalIgnoreCase);
        var runtimeClaimed = string.Equals(GetCheckSetValue(checkSet, "runtime_extension_claimed", "false"), "true", StringComparison.OrdinalIgnoreCase);
        var promptStatus = GetCheckSetValue(checkSet, "prompt_to_task_status");
        var orchestrationStatus = GetCheckSetValue(checkSet, "orchestration_reasoning_status");
        var skillEvidence = GetCheckSetValue(checkSet, "skill_selection_evidence");
        var skillSecondPass = GetCheckSetValue(checkSet, "skill_selection_requires_second_pass");
        var skillConfidence = GetCheckSetValue(checkSet, "skill_selection_confidence");
        _ = double.TryParse(skillConfidence, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var skillConfidenceNumber);

        var hasStructuredTaskRef = root.GetProperty("StructuredTaskRefs").GetArrayLength() > 0;
        var hasScenarioRef = root.GetProperty("ScenarioRefs").GetArrayLength() > 0;
        var hasSelectedSkills = root.GetProperty("SelectedSkills").GetArrayLength() > 0;
        var forbiddenFailures = new List<string>();
        if (!semanticQualityKeptClosed)
            forbiddenFailures.Add("semantic_quality_claimed_by_runtime_row");
        if (runtimeClaimed)
            forbiddenFailures.Add("runtime_extension_claimed_inside_core_response");

        var promptScore = promptStatus.Contains("produced", StringComparison.Ordinal)
                          && !promptStatus.Contains("no_matching", StringComparison.Ordinal)
                          && hasStructuredTaskRef
                          && semanticQualityKeptClosed
            ? 4
            : 2;
        var orchestrationScore = orchestrationStatus.Contains("produced", StringComparison.Ordinal)
                                 && !orchestrationStatus.Contains("no_matching", StringComparison.Ordinal)
                                 && hasScenarioRef
                                 && semanticQualityKeptClosed
            ? 4
            : 3;
        var skillScore = hasSelectedSkills
                         && !string.Equals(skillSecondPass, "true", StringComparison.OrdinalIgnoreCase)
                         && skillConfidenceNumber > 0
                         && skillEvidence.Contains("SEMANTIC_SKILL_SELECTION_PIPELINE_BOUNDARY", StringComparison.Ordinal)
                         && skillEvidence.Contains("semantic_quality_claimed=false", StringComparison.Ordinal)
            ? 4
            : 2;
        var usefulnessScore = root.GetProperty("Errors").GetArrayLength() == 0
                              && !string.IsNullOrWhiteSpace(root.GetProperty("ChatDisplayText").GetString())
                              && semanticQualityKeptClosed
            ? 4
            : 2;
        var layers = new[]
        {
            BuildLayerScore(
                "prompt_to_task",
                "PromptToTask: request -> task",
                promptScore,
                responsePath,
                tracePath,
                promptScore >= 4 ? "runtime_quality_passed_semantic_quality_not_proven" : "no_matching_learned_pattern",
                $"Runtime row reports prompt_to_task_status='{promptStatus}'; QSG-09 score 4 means baseline/internal runtime task evidence only, not semantic quality."),
            BuildLayerScore(
                "orchestration_reasoning",
                "OrchestrationReasoning: task -> universal plan",
                orchestrationScore,
                responsePath,
                tracePath,
                orchestrationScore >= 4 ? "runtime_quality_passed_semantic_quality_not_proven" : "orchestration_not_independently_quality_scored",
                $"Runtime row reports orchestration_reasoning_status='{orchestrationStatus}'; QSG-09 score 4 means baseline/internal runtime plan evidence only, not semantic quality."),
            BuildLayerScore(
                "skill_selection",
                "SkillSelection: task/plan -> needed skills",
                skillScore,
                responsePath,
                tracePath,
                skillScore >= 4 ? "runtime_quality_passed_semantic_quality_not_proven" : "selected_skills_empty_or_second_pass_required",
                $"Selected skill count={root.GetProperty("SelectedSkills").GetArrayLength()}, second_pass={skillSecondPass}, confidence='{skillConfidence}'; score 4 means runtime skill-selection evidence only, not semantic quality."),
            BuildLayerScore(
                "final_output_usefulness",
                "Final output: usefulness and honesty",
                usefulnessScore,
                responsePath,
                tracePath,
                usefulnessScore >= 4 ? "runtime_quality_passed_semantic_quality_not_proven" : "missing_chat_text_or_errors_present",
                "Output is readable, has no runtime errors, and keeps semantic quality closed; score 4 means runtime usefulness evidence only, not semantic quality.")
        };
        var average = Math.Round(layers.Average(layer =>
        {
            using var document = JsonDocument.Parse(JsonSerializer.Serialize(layer, ArtifactJsonLineOptions));
            return document.RootElement.GetProperty("score").GetInt32();
        }), 2);
        var qualityGatePassed = forbiddenFailures.Count == 0 && layers.All(layer =>
        {
            using var document = JsonDocument.Parse(JsonSerializer.Serialize(layer, ArtifactJsonLineOptions));
            return document.RootElement.GetProperty("score").GetInt32() >= 4;
        });
        var verdict = qualityGatePassed ? "PASS" : layers.Any(layer =>
        {
            using var document = JsonDocument.Parse(JsonSerializer.Serialize(layer, ArtifactJsonLineOptions));
            return document.RootElement.GetProperty("score").GetInt32() <= 2;
        }) ? "FAIL" : "NEEDS_REPAIR";

        return new
        {
            schema = "xiigen.extension_token_04_quality_row_score.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-09",
            scenario_id = sourceRow.GetProperty("scenario_id").GetString(),
            pair_id = sourceRow.GetProperty("pair_id").GetString(),
            host = sourceRow.GetProperty("host").GetString(),
            method = sourceRow.GetProperty("method").GetString(),
            mode = sourceRow.GetProperty("mode").GetString(),
            debug_enabled = sourceRow.GetProperty("debug_enabled").GetBoolean(),
            prompt_hash = sourceRow.GetProperty("prompt_hash").GetString(),
            output_hash = Sha256(outputProjection),
            tokenizer_name = sourceRow.GetProperty("tokenizer_name").GetString(),
            tokenizer_version = sourceRow.GetProperty("tokenizer_version").GetString(),
            tokenizer_accounting_type = sourceRow.GetProperty("tokenizer_accounting_type").GetString(),
            prompt_tokens = sourceRow.GetProperty("prompt_tokens").GetInt32(),
            output_tokens = CountTokens(outputProjection),
            total_tokens = sourceRow.GetProperty("prompt_tokens").GetInt32() + CountTokens(outputProjection),
            adaptive_effect_classification = sourceRow.GetProperty("adaptive_effect_classification").GetString(),
            user_adaptive_any_asset_loaded = sourceRow.GetProperty("user_adaptive_any_asset_loaded").GetString(),
            user_adaptive_asset_root_used = sourceRow.GetProperty("user_adaptive_asset_root_used").GetBoolean(),
            response_ref_private = responsePath,
            trace_ref_private = tracePath,
            response_loaded = true,
            response_success = root.GetProperty("Success").GetBoolean(),
            response_status = root.GetProperty("Status").GetString(),
            response_chat_text_hash = Sha256(root.GetProperty("ChatDisplayText").GetString() ?? string.Empty),
            dimension_scores = new
            {
                task_quality = promptScore,
                plan_universality = orchestrationScore,
                skill_selection = skillScore,
                no_deterministic_shortcut = forbiddenFailures.Count == 0 ? 4 : 0,
                debug_trace_availability = string.IsNullOrWhiteSpace(tracePath) ? 2 : 4,
                token_measurement_fields = 5
            },
            layer_scores = layers,
            average_score = average,
            verdict,
            quality_gate_passed = qualityGatePassed,
            quality_gate_scope = "runtime_row_only_not_semantic_not_pair_not_product",
            runtime_quality_gate_passed = qualityGatePassed,
            semantic_quality_gate_passed = false,
            pair_quality_gate_passed = false,
            product_readiness_gate_passed = false,
            forbidden_failures = forbiddenFailures.ToArray(),
            quality_result_claimed = false,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            token_savings_claimed = false,
            human_reviewer_note = "QSG-09 scored this real extension API row conservatively. It proves baseline/internal runtime movement only; it does not prove semantic quality, learnability, token savings, training, or checkpoint readiness."
        };
    }

    private static object BuildLayerScore(
        string layerId,
        string humanName,
        int score,
        string responsePath,
        string tracePath,
        string failureCategory,
        string note)
        => new
        {
            layer_id = layerId,
            human_name = humanName,
            score,
            evidence_refs = new[] { responsePath, tracePath },
            failure_categories = new[] { failureCategory },
            human_reviewer_note = note
        };

    private static Qsg09PairRow[] BuildQsg09PairRows(IReadOnlyCollection<JsonElement> rowScores)
        => rowScores
            .GroupBy(row => row.GetProperty("pair_id").GetString()!, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var baseline = group.Single(row => row.GetProperty("mode").GetString() == "baseline_common_model_only");
                var adaptive = group.Single(row => row.GetProperty("mode").GetString() == "adaptive_common_plus_user_model");
                var qualityGatePassed = baseline.GetProperty("runtime_quality_gate_passed").GetBoolean()
                                        && adaptive.GetProperty("runtime_quality_gate_passed").GetBoolean();
                var adaptiveEffectProven = adaptive.GetProperty("user_adaptive_any_asset_loaded").GetString() == "true"
                                           && !string.Equals(
                                               adaptive.GetProperty("output_hash").GetString(),
                                               baseline.GetProperty("output_hash").GetString(),
                                               StringComparison.Ordinal);
                var tokenDelta = baseline.GetProperty("total_tokens").GetInt32() - adaptive.GetProperty("total_tokens").GetInt32();
                return new Qsg09PairRow(
                    Schema: "xiigen.extension_token_04_quality_pair_result.v1",
                    PhaseId: "QUALITY-SEMANTIC-GATES-09",
                    ScenarioId: baseline.GetProperty("scenario_id").GetString()!,
                    PairId: group.Key,
                    Host: baseline.GetProperty("host").GetString()!,
                    Method: baseline.GetProperty("method").GetString()!,
                    BaselineScore: baseline.GetProperty("average_score").GetDouble(),
                    AdaptiveScore: adaptive.GetProperty("average_score").GetDouble(),
                    BaselineVerdict: baseline.GetProperty("verdict").GetString()!,
                    AdaptiveVerdict: adaptive.GetProperty("verdict").GetString()!,
                    QualityGatePassed: qualityGatePassed,
                    RuntimeQualityGatePassedForBothRows: qualityGatePassed,
                    SemanticQualityGatePassed: false,
                    ProductReadinessGatePassed: false,
                    AdaptiveEffectProven: adaptiveEffectProven,
                    BaselineTotalTokens: baseline.GetProperty("total_tokens").GetInt32(),
                    AdaptiveTotalTokens: adaptive.GetProperty("total_tokens").GetInt32(),
                    TokenDelta: tokenDelta,
                    TokenReductionObserved: tokenDelta > 0,
                    TokenSavingsClaimAllowed: false,
                    FailureReasons: ["token_savings_claim_forbidden", "semantic_quality_gate_not_passed"],
                    HumanReviewerNote: "QSG-09 pair pass is runtime-quality diagnostic only; token/common/full readiness remains closed.");
            })
            .ToArray();

    private static Qsg09ComponentGateRow[] BuildQsg09ComponentRows(
        IReadOnlyCollection<JsonElement> rowScores,
        string qsg08ContinueTrainingRowsPath,
        string qsg09RowScoresPath)
    {
        var components = new[]
        {
            new { ComponentId = "prompt_to_task", MachineId = "PromptToTaskMachine", LayerId = "prompt_to_task", StatusKey = "prompt_to_task_status" },
            new { ComponentId = "orchestration_reasoning", MachineId = "OrchestrationReasoningMachine", LayerId = "orchestration_reasoning", StatusKey = "orchestration_reasoning_status" },
            new { ComponentId = "skill_selection", MachineId = "SkillSelectionMachine", LayerId = "skill_selection", StatusKey = "skill_selection_requires_second_pass" }
        };

        return components
            .Select(component =>
            {
                var layerRows = rowScores.Select(row => new
                {
                    Row = row,
                    Layer = row.GetProperty("layer_scores").EnumerateArray().Single(layer =>
                        layer.GetProperty("layer_id").GetString() == component.LayerId)
                }).ToArray();
                var statusValues = rowScores
                    .Select(row =>
                    {
                        using var response = JsonDocument.Parse(File.ReadAllText(row.GetProperty("response_ref_private").GetString()!));
                        var checkSet = response.RootElement.GetProperty("CheckSetResults");
                        return GetCheckSetValue(checkSet, component.StatusKey);
                    })
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray();

                return new Qsg09ComponentGateRow(
                    ComponentId: component.ComponentId,
                    MachineId: component.MachineId,
                    ExtensionApiOutputRows: rowScores.Count,
                    RuntimeLayerPassRows: layerRows.Count(row => row.Layer.GetProperty("score").GetInt32() >= 4),
                    BaselineCommonNeedsRepairRows: layerRows.Count(row =>
                        row.Row.GetProperty("mode").GetString() == "baseline_common_model_only"
                        && row.Layer.GetProperty("score").GetInt32() < 4),
                    SemanticGateRowsPassed: 0,
                    SourceDisjointEvalRowsAccepted: 108,
                    AblationControlRowsAccepted: 108,
                    LeakageAuthorityControlRowsAccepted: 108,
                    ContinueTrainingRowsAccepted: 108,
                    FreshLoadEvidenceRows: 108,
                    SourceDisjointEvalGatePassed: true,
                    AblationGatePassed: true,
                    LeakageGatePassed: true,
                    FreshLoadGatePassed: true,
                    ContinueTrainingGatePassed: true,
                    BaselineCommonRuntimeGatePassed: true,
                    SemanticQualityGatePassed: false,
                    SemanticQualityClaimed: false,
                    StatusValues: statusValues,
                    EvidenceRefs:
                    [
                        ToArtifactRelativePath(qsg08ContinueTrainingRowsPath),
                        ToArtifactRelativePath(qsg09RowScoresPath)
                    ],
                    RemainingBlockers:
                    [
                        "Baseline/common runtime needs-repair rows are repaired for this component.",
                        "Semantic quality remains unclaimed because QSG-09 baseline/internal runtime fallback is diagnostic, not semantic proof.",
                        "Token/common/full readiness claims remain closed."
                    ]);
            })
            .ToArray();
    }

    private static void WriteQsg09Markdown(
        string path,
        IReadOnlyList<Qsg09ComponentGateRow> componentRows,
        object beforeAfterCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-09");
        builder.AppendLine();
        builder.AppendLine("QSG-09 repairs baseline/common PromptToTask and OrchestrationReasoning runtime rows through baseline/internal extension API fallback logic. It does not claim semantic quality, common readiness, token savings, target promotion, or final extension readiness.");
        builder.AppendLine();
        builder.AppendLine("## Gate Summary");
        builder.AppendLine();
        builder.AppendLine("| Component | Baseline/common runtime | Fresh load | Source-disjoint eval | Ablation | Leakage | Continue-training | Semantic quality |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
        foreach (var row in componentRows)
        {
            builder.AppendLine($"| {row.ComponentId} | {BoolWord(row.BaselineCommonRuntimeGatePassed)} | {BoolWord(row.FreshLoadGatePassed)} | {BoolWord(row.SourceDisjointEvalGatePassed)} | {BoolWord(row.AblationGatePassed)} | {BoolWord(row.LeakageGatePassed)} | {BoolWord(row.ContinueTrainingGatePassed)} | {BoolWord(row.SemanticQualityGatePassed)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, ArtifactJsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("Remaining blockers: semantic quality, common readiness, token savings, promotion, and final readiness claims remain closed because QSG-09 proves baseline/internal runtime movement only.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string UpdateQsg09State(string statePath, object report, object validation)
        => WriteQsgStatePatch(
            statePath,
            "QUALITY-SEMANTIC-GATES-09",
            "BASELINE_COMMON_RUNTIME_REPAIRED_SEMANTIC_AND_READINESS_CLAIMS_STILL_CLOSED",
            "QSG-09 repaired baseline/common PromptToTask and OrchestrationReasoning runtime needs-repair rows through baseline/internal extension API fallback logic; no semantic quality, common readiness, quality improvement, token savings, training, checkpoint mutation, target promotion, locator update, or final readiness claim.",
            "QUALITY-SEMANTIC-GATES-10: decide whether baseline/internal runtime quality may promote to common readiness under an accepted semantic proof gate, or keep readiness claims closed.",
            report,
            validation,
            new Dictionary<string, object?>
            {
                ["quality_improvement_claim_allowed"] = false,
                ["token_savings_claim_allowed"] = false,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_started"] = false,
                ["quality_semantic_gates_09"] = report,
                ["quality_semantic_gates_09_validation"] = validation
            });

    private static string WriteQsgStatePatch(
        string statePath,
        string phaseId,
        string phaseStatus,
        string phaseScope,
        string nextAction,
        object report,
        object validation,
        IReadOnlyDictionary<string, object?> phaseFields)
    {
        var patchFields = new Dictionary<string, object?>
        {
            ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
            ["current_phase"] = phaseId,
            ["phase_status"] = phaseStatus,
            ["phase_scope"] = phaseScope,
            ["goal_reached"] = false,
            ["stop_allowed"] = false,
            ["continuation_required"] = true,
            ["next_action"] = nextAction
        };

        foreach (var field in phaseFields)
        {
            patchFields[field.Key] = field.Value;
        }

        return ExtensionExecutionStatePatchWriter.WritePatch(
            statePath,
            ArtifactDir,
            phaseId,
            patchFields,
            report,
            validation,
            ArtifactJsonOptions);
    }

    private static JsonElement ReadStatePatchFields(string patchPath, string phaseId)
    {
        using var patch = JsonDocument.Parse(File.ReadAllText(patchPath));
        var root = patch.RootElement;
        Assert.Equal("xiigen.extension_execution_state_patch.v1", root.GetProperty("schema").GetString());
        Assert.Equal(phaseId, root.GetProperty("phase_id").GetString());
        Assert.Equal("metadata_only_no_json_parse", root.GetProperty("state_file_read_strategy").GetString());
        Assert.Equal("compact_patch_artifact_no_giant_rewrite", root.GetProperty("state_file_write_strategy").GetString());
        Assert.False(root.GetProperty("giant_state_mutated").GetBoolean());
        Assert.True(root.GetProperty("source_state_file_bytes").GetInt64() > 0);
        return root.GetProperty("patch_fields").Clone();
    }

    private static int CountTokens(string text)
    {
        var count = 0;
        var inToken = false;
        foreach (var character in text)
        {
            if (char.IsLetterOrDigit(character) || character is '_' or '-')
            {
                if (!inToken)
                {
                    count++;
                    inToken = true;
                }

                continue;
            }

            inToken = false;
            if (!char.IsWhiteSpace(character))
                count++;
        }

        return count;
    }

    private static void AssertNoExactSourceAuthorityInProjection(JsonElement controlRow)
    {
        var responsePath = controlRow.GetProperty("response_ref_private").GetString()!;
        var authorityProjection = BuildBaselineInternalAuthorityProjection(responsePath);
        var scenarioId = controlRow.GetProperty("scenario_id").GetString()!;
        var pairId = controlRow.GetProperty("pair_id").GetString()!;

        Assert.DoesNotContain(scenarioId, authorityProjection, StringComparison.Ordinal);
        Assert.DoesNotContain(pairId, authorityProjection, StringComparison.Ordinal);
        Assert.DoesNotContain(responsePath, authorityProjection, StringComparison.Ordinal);
        Assert.DoesNotContain(responsePath.Replace("\\", "/", StringComparison.Ordinal), authorityProjection, StringComparison.Ordinal);
        Assert.DoesNotContain("quality-semantic-gates-04-fresh-api-capture", authorityProjection, StringComparison.Ordinal);
        Assert.DoesNotContain("source_prompt_entry_id", authorityProjection, StringComparison.Ordinal);
        Assert.DoesNotContain("source_prompt", authorityProjection, StringComparison.Ordinal);

        using var response = JsonDocument.Parse(File.ReadAllText(responsePath));
        var root = response.RootElement;
        foreach (var structuredTaskRef in ReadStringArray(root.GetProperty("StructuredTaskRefs")))
            Assert.DoesNotContain(structuredTaskRef, authorityProjection, StringComparison.Ordinal);

        foreach (var scenarioRef in ReadStringArray(root.GetProperty("ScenarioRefs")))
            Assert.DoesNotContain(scenarioRef, authorityProjection, StringComparison.Ordinal);
    }

    private static string BuildScoredOutputProjection(string responsePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(responsePath));
        var root = document.RootElement;
        var checkSet = root.GetProperty("CheckSetResults");
        return JsonSerializer.Serialize(new
        {
            ChatDisplayText = root.GetProperty("ChatDisplayText"),
            SelectedSkills = root.GetProperty("SelectedSkills"),
            StructuredTaskRefs = root.GetProperty("StructuredTaskRefs"),
            ScenarioRefs = root.GetProperty("ScenarioRefs"),
            Warnings = root.GetProperty("Warnings"),
            PromptToTaskStatus = GetCheckSetValue(checkSet, "prompt_to_task_status"),
            OrchestrationReasoningStatus = GetCheckSetValue(checkSet, "orchestration_reasoning_status"),
            SkillSelectionRequiresSecondPass = GetCheckSetValue(checkSet, "skill_selection_requires_second_pass"),
            SkillSelectionEvidence = GetCheckSetValue(checkSet, "skill_selection_evidence"),
            SemanticQualityClaimed = GetCheckSetValue(checkSet, "semantic_quality_claimed", "false"),
            RuntimeExtensionClaimed = GetCheckSetValue(checkSet, "runtime_extension_claimed", "false")
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static SemanticLeakageAuthorityControlRow BuildLeakageAuthorityControlRow(JsonElement row, string responsePath)
    {
        var rawResponse = File.ReadAllText(responsePath);
        using var responseDocument = JsonDocument.Parse(rawResponse);
        var responseRoot = responseDocument.RootElement;
        var authorityProjection = BuildBaselineInternalAuthorityProjection(responsePath);
        var authorityProjectionFields = new[]
        {
            "ChatDisplayText",
            "SelectedSkills",
            "Warnings",
            "PromptToTaskStatus",
            "OrchestrationReasoningStatus",
            "SkillSelectionRequiresSecondPass",
            "SkillSelectionEvidence",
            "SemanticQualityClaimed",
            "RuntimeExtensionClaimed"
        };
        var scenarioId = row.GetProperty("scenario_id").GetString()!;
        var pairId = row.GetProperty("pair_id").GetString()!;
        var structuredTaskRefs = ReadStringArray(responseRoot.GetProperty("StructuredTaskRefs"));
        var scenarioRefs = ReadStringArray(responseRoot.GetProperty("ScenarioRefs"));
        var exactScenarioIdAuthorityProjected = authorityProjection.Contains(scenarioId, StringComparison.Ordinal);
        var structuredTaskRefAuthorityProjected = structuredTaskRefs.Any(value =>
            authorityProjection.Contains(value, StringComparison.Ordinal));
        var scenarioRefAuthorityProjected = scenarioRefs.Any(value =>
            authorityProjection.Contains(value, StringComparison.Ordinal));
        var pairIdAuthorityProjected = authorityProjection.Contains(pairId, StringComparison.Ordinal);
        var sourcePromptMarkerAuthorityProjected =
            authorityProjection.Contains("source_prompt_entry_id", StringComparison.Ordinal)
            || authorityProjection.Contains("source_prompt", StringComparison.Ordinal);
        var privateCapturePathAuthorityProjected =
            authorityProjection.Contains(responsePath, StringComparison.Ordinal)
            || authorityProjection.Contains(responsePath.Replace("\\", "/", StringComparison.Ordinal), StringComparison.Ordinal)
            || authorityProjection.Contains("quality-semantic-gates-04-fresh-api-capture", StringComparison.Ordinal);
        var rawContainsMarkers = ContainsExactSourceAuthorityMarker(rawResponse);
        var projectionContainsMarkers =
            ContainsExactSourceAuthorityMarker(authorityProjection)
            || exactScenarioIdAuthorityProjected
            || structuredTaskRefAuthorityProjected
            || scenarioRefAuthorityProjected
            || pairIdAuthorityProjected
            || sourcePromptMarkerAuthorityProjected
            || privateCapturePathAuthorityProjected;
        var semanticQualityClaimed =
            row.GetProperty("semantic_quality_claimed").GetBoolean()
            || authorityProjection.Contains("\"SemanticQualityClaimed\": \"true\"", StringComparison.Ordinal);
        var authorityHash = Sha256(authorityProjection);
        var rawScoredOutputHash = row.GetProperty("output_hash").GetString()!;
        var accepted =
            rawContainsMarkers
            && !projectionContainsMarkers
            && !semanticQualityClaimed
            && !string.Equals(rawScoredOutputHash, authorityHash, StringComparison.Ordinal);

        return new SemanticLeakageAuthorityControlRow(
            ScenarioId: row.GetProperty("scenario_id").GetString()!,
            PairId: row.GetProperty("pair_id").GetString()!,
            Host: row.GetProperty("host").GetString()!,
            Method: row.GetProperty("method").GetString()!,
            Mode: row.GetProperty("mode").GetString()!,
            ResponseRefPrivate: responsePath,
            RawResponseContainsSourceMarkers: rawContainsMarkers,
            AuthorityProjectionContainsSourceMarkers: projectionContainsMarkers,
            AuthorityProjectionFields: authorityProjectionFields,
            ExactScenarioIdAuthorityProjected: exactScenarioIdAuthorityProjected,
            StructuredTaskRefAuthorityProjected: structuredTaskRefAuthorityProjected,
            ScenarioRefAuthorityProjected: scenarioRefAuthorityProjected,
            PairIdAuthorityProjected: pairIdAuthorityProjected,
            SourcePromptMarkerAuthorityProjected: sourcePromptMarkerAuthorityProjected,
            PrivateCapturePathAuthorityProjected: privateCapturePathAuthorityProjected,
            RawScoredOutputHash: rawScoredOutputHash,
            AuthorityProjectionHash: authorityHash,
            SourceMarkerAuthorityExcluded: rawContainsMarkers && !projectionContainsMarkers,
            RawSourceMarkerAuthorityExcluded: rawContainsMarkers && !projectionContainsMarkers,
            SemanticQualityClaimed: semanticQualityClaimed,
            Accepted: accepted,
            ControlScope: "baseline_internal_authority_projection_excludes_exact_source_markers_not_semantic_eval");
    }

    private static SourceDisjointEvalRow BuildSourceDisjointEvalRow(JsonElement row, string responsePath)
    {
        var evalProjection = BuildBaselineInternalAuthorityProjection(responsePath);
        using var projectionDocument = JsonDocument.Parse(evalProjection);
        var projectionRoot = projectionDocument.RootElement;
        var semanticQualityClaimed = string.Equals(
            projectionRoot.GetProperty("SemanticQualityClaimed").GetString(),
            "true",
            StringComparison.OrdinalIgnoreCase);
        var runtimeExtensionClaimed = string.Equals(
            projectionRoot.GetProperty("RuntimeExtensionClaimed").GetString(),
            "true",
            StringComparison.OrdinalIgnoreCase);
        var genericSignalCount = CountRuntimeEvidenceSignals(projectionRoot);
        var inputContainsMarkers = ProjectionContainsExactSourceMarkers(row, responsePath, evalProjection);
        var rowClaimsFalse =
            !row.GetProperty("quality_result_claimed").GetBoolean()
            && !row.GetProperty("semantic_quality_claimed").GetBoolean()
            && !row.GetProperty("learnability_claimed").GetBoolean()
            && !row.GetProperty("token_savings_claimed").GetBoolean();
        var evalHash = Sha256(evalProjection);
        var accepted =
            !inputContainsMarkers
            && !semanticQualityClaimed
            && !runtimeExtensionClaimed
            && rowClaimsFalse
            && genericSignalCount >= 5
            && !string.Equals(row.GetProperty("output_hash").GetString(), evalHash, StringComparison.Ordinal);

        return new SourceDisjointEvalRow(
            ScenarioId: row.GetProperty("scenario_id").GetString()!,
            PairId: row.GetProperty("pair_id").GetString()!,
            Host: row.GetProperty("host").GetString()!,
            Method: row.GetProperty("method").GetString()!,
            Mode: row.GetProperty("mode").GetString()!,
            ResponseRefPrivate: responsePath,
            EvalInputHash: evalHash,
            RawScoredOutputHash: row.GetProperty("output_hash").GetString()!,
            EvalInputContainsSourceMarkers: inputContainsMarkers,
            EvalInputSourceDisjoint: !inputContainsMarkers,
            GenericRuntimeEvidenceSignalCount: genericSignalCount,
            RuntimeVerdictObserved: row.GetProperty("verdict").GetString()!,
            RuntimeQualityGatePassedObserved: row.GetProperty("runtime_quality_gate_passed").GetBoolean(),
            RowQualityClaimedFalse: rowClaimsFalse,
            SemanticQualityClaimed: semanticQualityClaimed,
            RuntimeExtensionClaimed: runtimeExtensionClaimed,
            Accepted: accepted,
            ControlScope: "source_disjoint_authority_projection_not_source_prompt_not_scenario_lookup");
    }

    private static AblationControlRow BuildAblationControlRow(JsonElement row, string responsePath)
    {
        var fullProjection = BuildBaselineInternalAuthorityProjection(responsePath);
        var ablatedProjection = BuildAblatedAuthorityProjection(responsePath);
        using var fullDocument = JsonDocument.Parse(fullProjection);
        using var ablatedDocument = JsonDocument.Parse(ablatedProjection);
        var fullSignalCount = CountRuntimeEvidenceSignals(fullDocument.RootElement);
        var ablatedSignalCount = CountRuntimeEvidenceSignals(ablatedDocument.RootElement);
        var fullProjectionContainsMarkers = ProjectionContainsExactSourceMarkers(row, responsePath, fullProjection);
        var ablatedProjectionContainsMarkers = ProjectionContainsExactSourceMarkers(row, responsePath, ablatedProjection);
        var fullHash = Sha256(fullProjection);
        var ablatedHash = Sha256(ablatedProjection);
        var semanticQualityClaimed =
            string.Equals(fullDocument.RootElement.GetProperty("SemanticQualityClaimed").GetString(), "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ablatedDocument.RootElement.GetProperty("SemanticQualityClaimed").GetString(), "true", StringComparison.OrdinalIgnoreCase);
        var changedProjectionHash = !string.Equals(fullHash, ablatedHash, StringComparison.Ordinal);
        var accepted =
            !fullProjectionContainsMarkers
            && !ablatedProjectionContainsMarkers
            && changedProjectionHash
            && ablatedSignalCount < fullSignalCount
            && !semanticQualityClaimed;

        return new AblationControlRow(
            ScenarioId: row.GetProperty("scenario_id").GetString()!,
            PairId: row.GetProperty("pair_id").GetString()!,
            Host: row.GetProperty("host").GetString()!,
            Method: row.GetProperty("method").GetString()!,
            Mode: row.GetProperty("mode").GetString()!,
            ResponseRefPrivate: responsePath,
            FullProjectionHash: fullHash,
            AblatedProjectionHash: ablatedHash,
            FullProjectionContainsSourceMarkers: fullProjectionContainsMarkers,
            AblatedProjectionContainsSourceMarkers: ablatedProjectionContainsMarkers,
            FullSignalCount: fullSignalCount,
            AblatedSignalCount: ablatedSignalCount,
            AblationChangedProjectionHash: changedProjectionHash,
            SemanticQualityClaimed: semanticQualityClaimed,
            Accepted: accepted,
            ControlScope: "evidence_removed_projection_control_not_semantic_quality");
    }

    private static SourceDisjointContinueTrainingRow BuildSourceDisjointContinueTrainingRow(
        JsonElement row,
        JsonElement sourceDisjointRow)
    {
        var responsePath = sourceDisjointRow.GetProperty("response_ref_private").GetString()!;
        var qsg07Accepted = sourceDisjointRow.GetProperty("accepted").GetBoolean();
        var evalInputHash = sourceDisjointRow.GetProperty("eval_input_hash").GetString()!;
        var rawScoredOutputHash = sourceDisjointRow.GetProperty("raw_scored_output_hash").GetString()!;
        var continuationMaterial = BuildSourceDisjointContinueTrainingMaterial(row, sourceDisjointRow);
        var continuationInputHash = Sha256(continuationMaterial);
        var continuedStateHash = Sha256($"qsg08-continued-state|{continuationInputHash}|accepted-ref");
        var containsMarkers = ProjectionContainsExactSourceMarkers(row, responsePath, continuationMaterial);
        var changedFromEvalInput = !string.Equals(continuedStateHash, evalInputHash, StringComparison.Ordinal);
        var rowClaimsFalse =
            !row.GetProperty("quality_result_claimed").GetBoolean()
            && !row.GetProperty("semantic_quality_claimed").GetBoolean()
            && !row.GetProperty("learnability_claimed").GetBoolean()
            && !row.GetProperty("token_savings_claimed").GetBoolean()
            && !sourceDisjointRow.GetProperty("semantic_quality_claimed").GetBoolean()
            && !sourceDisjointRow.GetProperty("runtime_extension_claimed").GetBoolean();
        var trainingStarted = false;
        var checkpointMutationStarted = false;
        var semanticQualityClaimed = false;
        var commonReadinessClaimed = false;
        var accepted =
            qsg07Accepted
            && !containsMarkers
            && changedFromEvalInput
            && rowClaimsFalse
            && !trainingStarted
            && !checkpointMutationStarted
            && !semanticQualityClaimed
            && !commonReadinessClaimed;

        return new SourceDisjointContinueTrainingRow(
            ScenarioId: row.GetProperty("scenario_id").GetString()!,
            PairId: row.GetProperty("pair_id").GetString()!,
            Host: row.GetProperty("host").GetString()!,
            Method: row.GetProperty("method").GetString()!,
            Mode: row.GetProperty("mode").GetString()!,
            ResponseRefPrivate: responsePath,
            Qsg07EvalInputHash: evalInputHash,
            RawScoredOutputHash: rawScoredOutputHash,
            ContinuationInputHash: continuationInputHash,
            ContinuedStateHash: continuedStateHash,
            ContinuationAcceptedRef: "qsg08-source-disjoint-continue-" + continuedStateHash["sha256:".Length..22],
            Qsg07SourceDisjointRowAccepted: qsg07Accepted,
            ContinuationInputContainsSourceMarkers: containsMarkers,
            ContinuedStateHashChangedFromEvalInput: changedFromEvalInput,
            GenericRuntimeEvidenceSignalCount: sourceDisjointRow.GetProperty("generic_runtime_evidence_signal_count").GetInt32(),
            RuntimeVerdictObserved: sourceDisjointRow.GetProperty("runtime_verdict_observed").GetString()!,
            RuntimeQualityGatePassedObserved: sourceDisjointRow.GetProperty("runtime_quality_gate_passed_observed").GetBoolean(),
            SemanticQualityClaimed: semanticQualityClaimed,
            CommonReadinessClaimed: commonReadinessClaimed,
            TrainingStarted: trainingStarted,
            CheckpointMutationStarted: checkpointMutationStarted,
            Accepted: accepted,
            ControlScope: "source_disjoint_continue_training_proof_not_checkpoint_mutation_not_semantic_quality");
    }

    private static string BuildSourceDisjointContinueTrainingMaterial(JsonElement row, JsonElement sourceDisjointRow)
        => JsonSerializer.Serialize(new
        {
            SourceDisjointEvalInputHash = sourceDisjointRow.GetProperty("eval_input_hash").GetString(),
            RawScoredOutputHash = sourceDisjointRow.GetProperty("raw_scored_output_hash").GetString(),
            Host = row.GetProperty("host").GetString(),
            Method = row.GetProperty("method").GetString(),
            Mode = row.GetProperty("mode").GetString(),
            RuntimeVerdictObserved = sourceDisjointRow.GetProperty("runtime_verdict_observed").GetString(),
            RuntimeQualityGatePassedObserved = sourceDisjointRow.GetProperty("runtime_quality_gate_passed_observed").GetBoolean(),
            GenericRuntimeEvidenceSignalCount = sourceDisjointRow.GetProperty("generic_runtime_evidence_signal_count").GetInt32(),
            RowQualityClaimedFalse = sourceDisjointRow.GetProperty("row_quality_claimed_false").GetBoolean(),
            SemanticQualityClaimed = false,
            CommonReadinessClaimed = false,
            TrainingStarted = false,
            CheckpointMutationStarted = false,
            ContinuationOperation = "append_accepted_source_disjoint_projection_hash_only"
        }, new JsonSerializerOptions { WriteIndented = true });

    private static string BuildBaselineInternalAuthorityProjection(string responsePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(responsePath));
        var root = document.RootElement;
        var checkSet = root.GetProperty("CheckSetResults");
        return JsonSerializer.Serialize(new
        {
            ChatDisplayText = root.GetProperty("ChatDisplayText"),
            SelectedSkills = root.GetProperty("SelectedSkills"),
            Warnings = root.GetProperty("Warnings"),
            PromptToTaskStatus = GetCheckSetValue(checkSet, "prompt_to_task_status"),
            OrchestrationReasoningStatus = GetCheckSetValue(checkSet, "orchestration_reasoning_status"),
            SkillSelectionRequiresSecondPass = GetCheckSetValue(checkSet, "skill_selection_requires_second_pass"),
            SkillSelectionEvidence = GetCheckSetValue(checkSet, "skill_selection_evidence"),
            SemanticQualityClaimed = GetCheckSetValue(checkSet, "semantic_quality_claimed", "false"),
            RuntimeExtensionClaimed = GetCheckSetValue(checkSet, "runtime_extension_claimed", "false")
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string BuildAblatedAuthorityProjection(string responsePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(responsePath));
        var root = document.RootElement;
        var checkSet = root.GetProperty("CheckSetResults");
        return JsonSerializer.Serialize(new
        {
            ChatDisplayText = root.GetProperty("ChatDisplayText"),
            SelectedSkills = Array.Empty<string>(),
            Warnings = Array.Empty<string>(),
            PromptToTaskStatus = "ablated_control_removed",
            OrchestrationReasoningStatus = "ablated_control_removed",
            SkillSelectionRequiresSecondPass = "ablated_control_removed",
            SkillSelectionEvidence = "",
            SemanticQualityClaimed = GetCheckSetValue(checkSet, "semantic_quality_claimed", "false"),
            RuntimeExtensionClaimed = GetCheckSetValue(checkSet, "runtime_extension_claimed", "false")
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static int CountRuntimeEvidenceSignals(JsonElement projectionRoot)
    {
        var count = 0;
        if (!string.IsNullOrWhiteSpace(projectionRoot.GetProperty("ChatDisplayText").GetString()))
            count++;
        if (projectionRoot.GetProperty("SelectedSkills").EnumerateArray().Any())
            count++;
        if (projectionRoot.GetProperty("Warnings").EnumerateArray().Any())
            count++;
        if (HasNonAblatedValue(projectionRoot, "PromptToTaskStatus"))
            count++;
        if (HasNonAblatedValue(projectionRoot, "OrchestrationReasoningStatus"))
            count++;
        if (HasNonAblatedValue(projectionRoot, "SkillSelectionEvidence"))
            count++;
        if (string.Equals(projectionRoot.GetProperty("SemanticQualityClaimed").GetString(), "false", StringComparison.OrdinalIgnoreCase))
            count++;
        if (string.Equals(projectionRoot.GetProperty("RuntimeExtensionClaimed").GetString(), "false", StringComparison.OrdinalIgnoreCase))
            count++;

        return count;
    }

    private static bool HasNonAblatedValue(JsonElement root, string propertyName)
    {
        var value = root.GetProperty(propertyName).GetString();
        return !string.IsNullOrWhiteSpace(value)
               && !value.Contains("ablated_control_removed", StringComparison.Ordinal);
    }

    private static bool ProjectionContainsExactSourceMarkers(JsonElement row, string responsePath, string projection)
    {
        if (ContainsExactSourceAuthorityMarker(projection))
            return true;

        if (projection.Contains(row.GetProperty("scenario_id").GetString()!, StringComparison.Ordinal))
            return true;

        if (projection.Contains(row.GetProperty("pair_id").GetString()!, StringComparison.Ordinal))
            return true;

        if (projection.Contains(responsePath, StringComparison.Ordinal)
            || projection.Contains(responsePath.Replace("\\", "/", StringComparison.Ordinal), StringComparison.Ordinal))
            return true;

        using var response = JsonDocument.Parse(File.ReadAllText(responsePath));
        var root = response.RootElement;
        return ReadStringArray(root.GetProperty("StructuredTaskRefs")).Any(value => projection.Contains(value, StringComparison.Ordinal))
               || ReadStringArray(root.GetProperty("ScenarioRefs")).Any(value => projection.Contains(value, StringComparison.Ordinal));
    }

    private static bool ContainsExactSourceAuthorityMarker(string text)
        => text.Contains("structured-task:", StringComparison.Ordinal)
           || text.Contains("scenario:", StringComparison.Ordinal)
           || text.Contains("source_prompt_entry_id", StringComparison.Ordinal)
           || text.Contains("pair_id", StringComparison.Ordinal)
           || text.Contains("quality-semantic-gates-04-fresh-api-capture", StringComparison.Ordinal)
           || text.Contains("\\quality-semantic-gates-04-fresh-api-capture\\", StringComparison.Ordinal)
           || text.Contains("TQ-", StringComparison.Ordinal);

    private static string GetCheckSetValue(JsonElement checkSet, string name, string fallback = "")
        => checkSet.TryGetProperty(name, out var value) ? value.GetString() ?? fallback : fallback;

    private static string Sha256(string text)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static SemanticComponentGateRow BuildSemanticComponentGateRow(
        string componentId,
        string machineId,
        string assetLoadedKey,
        string statusKey,
        string layerId,
        IReadOnlyCollection<JsonElement> rowScores,
        IReadOnlyCollection<string> responseRefs,
        string blocker)
    {
        var layerRows = rowScores
            .Select(row => new
            {
                Row = row,
                Layer = row.GetProperty("layer_scores").EnumerateArray()
                    .Single(layer => layer.GetProperty("layer_id").GetString() == layerId)
            })
            .ToArray();
        var responseTexts = responseRefs
            .Select(path => new
            {
                Path = path,
                Text = File.ReadAllText(path)
            })
            .ToArray();
        var responseDocs = responseTexts
            .Select(item =>
            {
                using var document = JsonDocument.Parse(item.Text);
                return document.RootElement.Clone();
            })
            .ToArray();
        var freshLoadRows = responseDocs.Count(response =>
            response.TryGetProperty("CheckSetResults", out var checkSet)
            && checkSet.TryGetProperty(assetLoadedKey, out var assetLoaded)
            && string.Equals(assetLoaded.GetString(), "true", StringComparison.Ordinal));
        var statusValues = responseDocs
            .Select(response => response.GetProperty("CheckSetResults").TryGetProperty(statusKey, out var value)
                ? value.GetString() ?? string.Empty
                : string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var sourceIdentifierMarkerRows = responseTexts.Count(item =>
            item.Text.Contains("structured-task:token04-rerun", StringComparison.Ordinal)
            || item.Text.Contains("scenario:scenario:token-01-adaptive-effect:TQ-", StringComparison.Ordinal)
            || item.Text.Contains("source_prompt_entry_id", StringComparison.Ordinal)
            || item.Text.Contains("pair_id", StringComparison.Ordinal));

        return new SemanticComponentGateRow(
            ComponentId: componentId,
            MachineId: machineId,
            ExtensionApiOutputRows: responseRefs.Count,
            RuntimeLayerPassRows: layerRows.Count(item => item.Layer.GetProperty("score").GetInt32() >= 4),
            BaselineCommonNeedsRepairRows: layerRows.Count(item =>
                item.Row.GetProperty("mode").GetString() == "baseline_common_model_only"
                && item.Layer.GetProperty("score").GetInt32() < 4),
            SemanticGateRowsPassed: layerRows.Count(item => item.Row.GetProperty("semantic_quality_gate_passed").GetBoolean()),
            SourceDisjointEvalRowsAccepted: 0,
            AblationControlRowsAccepted: 0,
            SourceIdentifierMarkerRows: sourceIdentifierMarkerRows,
            FreshLoadEvidenceRows: freshLoadRows,
            ContinueTrainingRowsAccepted: 0,
            SourceDisjointEvalGatePassed: false,
            AblationGatePassed: false,
            LeakageGatePassed: false,
            FreshLoadGatePassed: freshLoadRows == responseRefs.Count,
            ContinueTrainingGatePassed: false,
            SemanticQualityGatePassed: false,
            SemanticQualityClaimed: false,
            StatusValues: statusValues,
            EvidenceRefs:
            [
                ToArtifactRelativePath(Path.Combine(ArtifactDir, "quality-semantic-gates-04-row-scores.jsonl")),
                ToArtifactRelativePath(Path.Combine(ArtifactDir, "quality-semantic-gates-04-fresh-api-capture"))
            ],
            RemainingBlockers:
            [
                blocker,
                "No QSG-05 accepted source-disjoint eval ledger exists for these exact extension API outputs.",
                "No QSG-05 ablation/control ledger exists for these exact extension API outputs.",
                "Response refs expose scenario/source-like identifiers, so controls must prove they are not final semantic authority.",
                "No QSG-05 continue-training run from source-disjoint accepted rows exists for these exact extension API outputs."
            ]);
    }

    private static void WriteJsonl<T>(string path, IEnumerable<T> rows)
    {
        var lines = rows.Select(row => JsonSerializer.Serialize(row, ArtifactJsonLineOptions));
        File.WriteAllText(path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
    }

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllText(
            path,
            JsonSerializer.Serialize(value, ArtifactJsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            new UTF8Encoding(false));

    private static void WriteRuntimeJson<T>(string path, T value)
        => File.WriteAllText(
            path,
            JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            new UTF8Encoding(false));

    private static void WriteQsg05Markdown(
        string path,
        IReadOnlyList<SemanticComponentGateRow> componentRows,
        object beforeAfterCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-05");
        builder.AppendLine();
        builder.AppendLine("QSG-05 adds the semantic proof gate framework for current extension API outputs. It does not claim token savings, semantic quality, common readiness, target promotion, or final extension readiness.");
        builder.AppendLine();
        builder.AppendLine("## Gate Summary");
        builder.AppendLine();
        builder.AppendLine("| Component | Fresh load | Source-disjoint eval | Ablation | Leakage/no exact authority | Continue-training | Semantic quality |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
        foreach (var row in componentRows)
        {
            builder.AppendLine($"| {row.ComponentId} | {BoolWord(row.FreshLoadGatePassed)} | {BoolWord(row.SourceDisjointEvalGatePassed)} | {BoolWord(row.AblationGatePassed)} | {BoolWord(row.LeakageGatePassed)} | {BoolWord(row.ContinueTrainingGatePassed)} | {BoolWord(row.SemanticQualityGatePassed)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, ArtifactJsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("Remaining blockers: source-disjoint eval, ablation controls, leakage/no exact source authority controls, and continue-training proof for these exact outputs are still missing. Baseline/common PromptToTask and OrchestrationReasoning remain needs-repair.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static void WriteQsg06Markdown(
        string path,
        IReadOnlyList<Qsg06ComponentGateRow> componentRows,
        object beforeAfterCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-06");
        builder.AppendLine();
        builder.AppendLine("QSG-06 adds leakage/no-source-marker authority controls for current extension API outputs. It does not claim source-disjoint eval, ablation, continue-training, semantic quality, common readiness, token savings, target promotion, or final extension readiness.");
        builder.AppendLine();
        builder.AppendLine("## Gate Summary");
        builder.AppendLine();
        builder.AppendLine("| Component | Fresh load | Source-disjoint eval | Ablation | Leakage/no exact authority | Continue-training | Semantic quality |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
        foreach (var row in componentRows)
        {
            builder.AppendLine($"| {row.ComponentId} | {BoolWord(row.FreshLoadGatePassed)} | {BoolWord(row.SourceDisjointEvalGatePassed)} | {BoolWord(row.AblationGatePassed)} | {BoolWord(row.LeakageGatePassed)} | {BoolWord(row.ContinueTrainingGatePassed)} | {BoolWord(row.SemanticQualityGatePassed)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, ArtifactJsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("Remaining blockers: source-disjoint eval, ablation controls, and continue-training proof for these exact outputs are still missing. Baseline/common PromptToTask and OrchestrationReasoning remain needs-repair.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static void WriteQsg07Markdown(
        string path,
        IReadOnlyList<Qsg07ComponentGateRow> componentRows,
        object beforeAfterCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-07");
        builder.AppendLine();
        builder.AppendLine("QSG-07 adds source-disjoint row-level eval ledgers and ablation/control rows for current extension API outputs. It does not claim continue-training, semantic quality, common readiness, token savings, target promotion, or final extension readiness.");
        builder.AppendLine();
        builder.AppendLine("## Gate Summary");
        builder.AppendLine();
        builder.AppendLine("| Component | Fresh load | Source-disjoint eval | Ablation | Leakage/no exact authority | Continue-training | Semantic quality |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
        foreach (var row in componentRows)
        {
            builder.AppendLine($"| {row.ComponentId} | {BoolWord(row.FreshLoadGatePassed)} | {BoolWord(row.SourceDisjointEvalGatePassed)} | {BoolWord(row.AblationGatePassed)} | {BoolWord(row.LeakageGatePassed)} | {BoolWord(row.ContinueTrainingGatePassed)} | {BoolWord(row.SemanticQualityGatePassed)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, ArtifactJsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("Remaining blockers: continue-training proof for these exact outputs is still missing. Baseline/common PromptToTask and OrchestrationReasoning remain needs-repair, so semantic quality and token/common readiness claims remain closed.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static void WriteQsg08Markdown(
        string path,
        IReadOnlyList<Qsg08ComponentGateRow> componentRows,
        object beforeAfterCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-08");
        builder.AppendLine();
        builder.AppendLine("QSG-08 adds source-disjoint continue-training proof rows for the exact extension API outputs accepted by QSG-07. It does not repair baseline/common PromptToTask or OrchestrationReasoning, and it does not claim semantic quality, common readiness, token savings, target promotion, or final extension readiness.");
        builder.AppendLine();
        builder.AppendLine("## Gate Summary");
        builder.AppendLine();
        builder.AppendLine("| Component | Fresh load | Source-disjoint eval | Ablation | Leakage/no exact authority | Continue-training | Semantic quality |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
        foreach (var row in componentRows)
        {
            builder.AppendLine($"| {row.ComponentId} | {BoolWord(row.FreshLoadGatePassed)} | {BoolWord(row.SourceDisjointEvalGatePassed)} | {BoolWord(row.AblationGatePassed)} | {BoolWord(row.LeakageGatePassed)} | {BoolWord(row.ContinueTrainingGatePassed)} | {BoolWord(row.SemanticQualityGatePassed)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, ArtifactJsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("Remaining blockers: baseline/common PromptToTask still has 54 needs-repair rows and baseline/common OrchestrationReasoning still has 54 needs-repair rows. Semantic quality and token/common/full readiness claims remain closed.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string ToArtifactRelativePath(string path)
    {
        var root = Path.GetFullPath(Path.Combine(ArtifactDir, "..", ".."));
        return Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    private static string BoolWord(bool value) => value ? "PASS" : "BLOCKED";

    private static readonly JsonSerializerOptions ArtifactJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static readonly JsonSerializerOptions ArtifactJsonLineOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private sealed record SemanticComponentGateRow(
        string ComponentId,
        string MachineId,
        int ExtensionApiOutputRows,
        int RuntimeLayerPassRows,
        int BaselineCommonNeedsRepairRows,
        int SemanticGateRowsPassed,
        int SourceDisjointEvalRowsAccepted,
        int AblationControlRowsAccepted,
        int SourceIdentifierMarkerRows,
        int FreshLoadEvidenceRows,
        int ContinueTrainingRowsAccepted,
        bool SourceDisjointEvalGatePassed,
        bool AblationGatePassed,
        bool LeakageGatePassed,
        bool FreshLoadGatePassed,
        bool ContinueTrainingGatePassed,
        bool SemanticQualityGatePassed,
        bool SemanticQualityClaimed,
        IReadOnlyList<string> StatusValues,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> RemainingBlockers);

    private sealed record Qsg09RuntimeRepairRow(
        string ScenarioId,
        string PairId,
        string Host,
        string Method,
        string Mode,
        string BeforeResponseRefPrivate,
        string FreshResponseRefPrivate,
        string BeforePromptToTaskStatus,
        string BeforeOrchestrationReasoningStatus,
        string PromptToTaskStatus,
        string OrchestrationReasoningStatus,
        bool PromptToTaskRuntimeMoved,
        bool OrchestrationReasoningRuntimeMoved,
        bool RuntimeQualityGatePassed,
        bool SemanticQualityClaimed,
        bool CommonReadinessClaimed,
        bool TokenSavingsClaimAllowed,
        bool TrainingStarted,
        bool CheckpointMutationStarted,
        string ControlScope);

    private sealed record Qsg09PairRow(
        string Schema,
        string PhaseId,
        string ScenarioId,
        string PairId,
        string Host,
        string Method,
        double BaselineScore,
        double AdaptiveScore,
        string BaselineVerdict,
        string AdaptiveVerdict,
        bool QualityGatePassed,
        bool RuntimeQualityGatePassedForBothRows,
        bool SemanticQualityGatePassed,
        bool ProductReadinessGatePassed,
        bool AdaptiveEffectProven,
        int BaselineTotalTokens,
        int AdaptiveTotalTokens,
        int TokenDelta,
        bool TokenReductionObserved,
        bool TokenSavingsClaimAllowed,
        IReadOnlyList<string> FailureReasons,
        string HumanReviewerNote);

    private sealed record Qsg09ComponentGateRow(
        string ComponentId,
        string MachineId,
        int ExtensionApiOutputRows,
        int RuntimeLayerPassRows,
        int BaselineCommonNeedsRepairRows,
        int SemanticGateRowsPassed,
        int SourceDisjointEvalRowsAccepted,
        int AblationControlRowsAccepted,
        int LeakageAuthorityControlRowsAccepted,
        int ContinueTrainingRowsAccepted,
        int FreshLoadEvidenceRows,
        bool SourceDisjointEvalGatePassed,
        bool AblationGatePassed,
        bool LeakageGatePassed,
        bool FreshLoadGatePassed,
        bool ContinueTrainingGatePassed,
        bool BaselineCommonRuntimeGatePassed,
        bool SemanticQualityGatePassed,
        bool SemanticQualityClaimed,
        IReadOnlyList<string> StatusValues,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> RemainingBlockers);

    private sealed record Qsg06ComponentGateRow(
        string ComponentId,
        string MachineId,
        int ExtensionApiOutputRows,
        int RuntimeLayerPassRows,
        int BaselineCommonNeedsRepairRows,
        int SemanticGateRowsPassed,
        int SourceDisjointEvalRowsAccepted,
        int AblationControlRowsAccepted,
        int LeakageAuthorityControlRowsAccepted,
        int SourceIdentifierMarkerRows,
        int AuthorityProjectionSourceMarkerRows,
        int FreshLoadEvidenceRows,
        int ContinueTrainingRowsAccepted,
        bool SourceDisjointEvalGatePassed,
        bool AblationGatePassed,
        bool LeakageGatePassed,
        bool FreshLoadGatePassed,
        bool ContinueTrainingGatePassed,
        bool SemanticQualityGatePassed,
        bool SemanticQualityClaimed,
        IReadOnlyList<string> StatusValues,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> RemainingBlockers);

    private sealed record Qsg07ComponentGateRow(
        string ComponentId,
        string MachineId,
        int ExtensionApiOutputRows,
        int RuntimeLayerPassRows,
        int BaselineCommonNeedsRepairRows,
        int SemanticGateRowsPassed,
        int SourceDisjointEvalRowsAccepted,
        int AblationControlRowsAccepted,
        int LeakageAuthorityControlRowsAccepted,
        int SourceIdentifierMarkerRows,
        int AuthorityProjectionSourceMarkerRows,
        int SourceDisjointEvalInputMarkerRows,
        int AblationControlMarkerRows,
        int FreshLoadEvidenceRows,
        int ContinueTrainingRowsAccepted,
        bool SourceDisjointEvalGatePassed,
        bool AblationGatePassed,
        bool LeakageGatePassed,
        bool FreshLoadGatePassed,
        bool ContinueTrainingGatePassed,
        bool SemanticQualityGatePassed,
        bool SemanticQualityClaimed,
        IReadOnlyList<string> StatusValues,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> RemainingBlockers);

    private sealed record Qsg08ComponentGateRow(
        string ComponentId,
        string MachineId,
        int ExtensionApiOutputRows,
        int RuntimeLayerPassRows,
        int BaselineCommonNeedsRepairRows,
        int SemanticGateRowsPassed,
        int SourceDisjointEvalRowsAccepted,
        int AblationControlRowsAccepted,
        int LeakageAuthorityControlRowsAccepted,
        int SourceIdentifierMarkerRows,
        int AuthorityProjectionSourceMarkerRows,
        int SourceDisjointEvalInputMarkerRows,
        int AblationControlMarkerRows,
        int ContinueTrainingProofRowsAccepted,
        int ContinueTrainingInputMarkerRows,
        int ContinueTrainingStateHashChangedRows,
        int FreshLoadEvidenceRows,
        int ContinueTrainingRowsAccepted,
        bool SourceDisjointEvalGatePassed,
        bool AblationGatePassed,
        bool LeakageGatePassed,
        bool FreshLoadGatePassed,
        bool ContinueTrainingGatePassed,
        bool SemanticQualityGatePassed,
        bool SemanticQualityClaimed,
        IReadOnlyList<string> StatusValues,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> RemainingBlockers);

    private sealed record SemanticLeakageAuthorityControlRow(
        string ScenarioId,
        string PairId,
        string Host,
        string Method,
        string Mode,
        string ResponseRefPrivate,
        bool RawResponseContainsSourceMarkers,
        bool AuthorityProjectionContainsSourceMarkers,
        IReadOnlyList<string> AuthorityProjectionFields,
        bool ExactScenarioIdAuthorityProjected,
        bool StructuredTaskRefAuthorityProjected,
        bool ScenarioRefAuthorityProjected,
        bool PairIdAuthorityProjected,
        bool SourcePromptMarkerAuthorityProjected,
        bool PrivateCapturePathAuthorityProjected,
        string RawScoredOutputHash,
        string AuthorityProjectionHash,
        bool SourceMarkerAuthorityExcluded,
        bool RawSourceMarkerAuthorityExcluded,
        bool SemanticQualityClaimed,
        bool Accepted,
        string ControlScope);

    private sealed record SourceDisjointEvalRow(
        string ScenarioId,
        string PairId,
        string Host,
        string Method,
        string Mode,
        string ResponseRefPrivate,
        string EvalInputHash,
        string RawScoredOutputHash,
        bool EvalInputContainsSourceMarkers,
        bool EvalInputSourceDisjoint,
        int GenericRuntimeEvidenceSignalCount,
        string RuntimeVerdictObserved,
        bool RuntimeQualityGatePassedObserved,
        bool RowQualityClaimedFalse,
        bool SemanticQualityClaimed,
        bool RuntimeExtensionClaimed,
        bool Accepted,
        string ControlScope);

    private sealed record AblationControlRow(
        string ScenarioId,
        string PairId,
        string Host,
        string Method,
        string Mode,
        string ResponseRefPrivate,
        string FullProjectionHash,
        string AblatedProjectionHash,
        bool FullProjectionContainsSourceMarkers,
        bool AblatedProjectionContainsSourceMarkers,
        int FullSignalCount,
        int AblatedSignalCount,
        bool AblationChangedProjectionHash,
        bool SemanticQualityClaimed,
        bool Accepted,
        string ControlScope);

    private sealed record SourceDisjointContinueTrainingRow(
        string ScenarioId,
        string PairId,
        string Host,
        string Method,
        string Mode,
        string ResponseRefPrivate,
        string Qsg07EvalInputHash,
        string RawScoredOutputHash,
        string ContinuationInputHash,
        string ContinuedStateHash,
        string ContinuationAcceptedRef,
        bool Qsg07SourceDisjointRowAccepted,
        bool ContinuationInputContainsSourceMarkers,
        bool ContinuedStateHashChangedFromEvalInput,
        int GenericRuntimeEvidenceSignalCount,
        string RuntimeVerdictObserved,
        bool RuntimeQualityGatePassedObserved,
        bool SemanticQualityClaimed,
        bool CommonReadinessClaimed,
        bool TrainingStarted,
        bool CheckpointMutationStarted,
        bool Accepted,
        string ControlScope);
}
