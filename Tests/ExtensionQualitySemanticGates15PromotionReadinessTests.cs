// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates15PromotionReadinessTests
{
    private static readonly string RepoRoot = ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot;

    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;

    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates15Artifacts_RecordPromotionReviewAndKeepClaimsClosed()
    {
        var qsg14ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-report.json");
        var qsg14ValidationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14.validation.json");
        var qsg14SemanticRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-semantic-graduation-rows.jsonl");

        var targetPromotionReviewPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15-target-promotion-review.json");
        var targetPromotionReviewMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15-target-promotion-review.md");
        var locatorRollbackRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15-locator-rollback-review-rows.jsonl");
        var finalReadinessRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15-final-readiness-checks.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15.validation.json");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, "QUALITY-SEMANTIC-GATES-15");

        using var qsg14ReportDocument = JsonDocument.Parse(File.ReadAllText(qsg14ReportPath));
        using var qsg14ValidationDocument = JsonDocument.Parse(File.ReadAllText(qsg14ValidationPath));
        var qsg14Report = qsg14ReportDocument.RootElement;
        var qsg14Validation = qsg14ValidationDocument.RootElement;
        var qsg14SemanticRows = ReadJsonl(qsg14SemanticRowsPath);

        var qsg14SemanticGraduationPairs = qsg14Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32();
        var qsg14LimitedSemanticEvidencePairs = qsg14Report.GetProperty("limited_semantic_quality_evidence_accepted_pairs").GetInt32();
        var qsg14CommonReadinessPairs = qsg14Report.GetProperty("common_readiness_graduated_pairs").GetInt32();

        var requiredPromotionArtifacts = RequiredPromotionArtifacts();
        var missingPromotionArtifacts = requiredPromotionArtifacts
            .Where(artifact => !File.Exists(Path.Combine(RepoRoot, artifact.RelativePath)))
            .ToArray();
        var presentPromotionArtifacts = requiredPromotionArtifacts
            .Where(artifact => File.Exists(Path.Combine(RepoRoot, artifact.RelativePath)))
            .ToArray();

        var exactTargetArtifactsReady = missingPromotionArtifacts.Length == 0;
        var qsg14ReadyForBoundedSemanticOnly = qsg14Report.GetProperty("semantic_quality_gate_passed").GetBoolean()
            && qsg14Report.GetProperty("bounded_semantic_quality_claim_allowed").GetBoolean()
            && !qsg14Report.GetProperty("common_readiness_claim_allowed").GetBoolean()
            && !qsg14Report.GetProperty("target_promotion_done").GetBoolean()
            && !qsg14Report.GetProperty("locator_update_done").GetBoolean()
            && !qsg14Report.GetProperty("final_readiness_claimed").GetBoolean();
        var targetPromotionAllowed = qsg14ReadyForBoundedSemanticOnly && exactTargetArtifactsReady;

        var targetPromotionReview = new
        {
            schema = "xiigen.quality_semantic_gates_15.target_promotion_review.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-15",
            input_phase_id = "QUALITY-SEMANTIC-GATES-14",
            qsg14_report_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-report.json",
            reviewed_target_type = "common_readiness_promotion_candidate",
            bounded_semantic_graduation_pairs = qsg14SemanticGraduationPairs,
            bounded_semantic_quality_claim_allowed = qsg14Report.GetProperty("bounded_semantic_quality_claim_allowed").GetBoolean(),
            required_artifact_count = requiredPromotionArtifacts.Length,
            present_artifact_count = presentPromotionArtifacts.Length,
            missing_artifact_count = missingPromotionArtifacts.Length,
            required_artifacts = requiredPromotionArtifacts,
            missing_artifacts = missingPromotionArtifacts,
            target_promotion_review_packet_created = true,
            target_promotion_decision = targetPromotionAllowed ? "PROMOTE_ALLOWED" : "KEEP_CLOSED",
            target_promotion_done = false,
            locator_update_done = false,
            rollback_closure_done = false,
            final_readiness_claimed = false,
            product_runtime_semantic_quality_claimed = false,
            product_runtime_semantic_quality_verified = false,
            common_readiness_claim_allowed = false,
            decision_reason = targetPromotionAllowed
                ? "All exact target/rollback/locator/probe artifacts are present, but this test is not authorized to mutate locators or enable runtime claims."
                : qsg14ReadyForBoundedSemanticOnly
                    ? "QSG-14 opened bounded scenario-pack semantic evidence only. Exact QSG-15 target manifest/checkpoint, locator candidate, target fresh-load, continue-training, held-out, ablation, and rollback probe artifacts are missing, so promotion and runtime semantic claims stay closed."
                    : "Current QSG-14 evidence keeps bounded semantic graduation closed. Promotion and runtime semantic claims stay closed.",
            no_overclaim = new
            {
                semantic_quality_claimed_in_product_runtime = false,
                common_readiness_claimed = false,
                target_promoted = false,
                locator_updated = false,
                rollback_safety_claimed = false,
                final_readiness_claimed = false,
                deterministic_review_logic_is_not_runtime_semantic_logic = true
            }
        };

        var locatorRollbackRows = new object[]
        {
            new
            {
                schema = "xiigen.quality_semantic_gates_15.locator_rollback_review_row.v1",
                phase_id = "QUALITY-SEMANTIC-GATES-15",
                review_row_id = "locator-update",
                required_artifact_refs = new[]
                {
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-locator-candidate.json",
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-manifest-candidate.json",
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-fresh-load-probe.json",
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-rollback-probe.json"
                },
                reviewed_artifacts_exist = false,
                decision = "KEEP_LOCATOR_INACTIVE",
                locator_update_done = false,
                reason = "Locator update is inactive until exact target, rollback, and probe artifacts all pass and promotion is separately approved."
            },
            new
            {
                schema = "xiigen.quality_semantic_gates_15.locator_rollback_review_row.v1",
                phase_id = "QUALITY-SEMANTIC-GATES-15",
                review_row_id = "rollback-closure",
                required_artifact_refs = new[]
                {
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-checkpoint-candidate.json",
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-rollback-checkpoint-candidate.json",
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-rollback-probe.json"
                },
                reviewed_artifacts_exist = false,
                decision = "KEEP_ROLLBACK_OPEN",
                rollback_closure_done = false,
                reason = "Rollback safety cannot be claimed from QSG-14 semantic rows or the presence of historical rollback artifacts; QSG-15 needs a named target/rollback probe pair."
            }
        };

        var finalReadinessRows = FinalReadinessChecks()
            .Select(check =>
            {
                var inspected = ArtifactsForFinalReadinessCheck(check.Pass);
                return new
                {
                    schema = "xiigen.quality_semantic_gates_15.final_readiness_check.v1",
                    phase_id = "QUALITY-SEMANTIC-GATES-15",
                    final_readiness_pass = check.Pass,
                    subagent_id = $"QUALITY-SEMANTIC-GATES-15-FINAL-READINESS-{check.Pass:00}",
                    target_outcome_row = check.TargetOutcomeRow,
                    required_check = check.RequiredCheck,
                    artifacts_inspected = inspected,
                    verdict = "IN_PROGRESS",
                    findings = check.Findings,
                    required_fix_refs = check.RequiredFixRefs,
                    recheck_evidence = "Not run as PASS because exact target/rollback/locator/product-runtime evidence is absent in QSG-15.",
                    counts_as_final_readiness_pass = false,
                    dashboard_or_helper_substitute_used = false
                };
            })
            .ToArray();

        var finalReadinessPasses = finalReadinessRows.Count(row => row.counts_as_final_readiness_pass);
        var finalReadinessInProgress = finalReadinessRows.Count(row => row.verdict == "IN_PROGRESS");
        var remainingBlockers = new[]
        {
            "common_readiness_target_manifest_or_checkpoint_candidate_not_promoted",
            "locator_update_not_done",
            "product_runtime_semantic_quality_claim_not_enabled_or_verified",
            "target_promotion_decision_keep_closed_due_missing_exact_artifacts",
            "rollback_probe_missing_for_named_qsg15_target_and_rollback",
            "ten_final_readiness_checks_ran_but_zero_passed_after_semantic_graduation"
        };

        var beforeAfterCounts = new
        {
            qsg14 = new
            {
                pair_count = qsg14Report.GetProperty("pair_count").GetInt32(),
                semantic_graduation_accepted_pairs = qsg14SemanticGraduationPairs,
                limited_semantic_quality_evidence_accepted_pairs = qsg14LimitedSemanticEvidencePairs,
                common_readiness_graduated_pairs = qsg14CommonReadinessPairs,
                target_promotion_review_packets = 0,
                target_promotion_done = qsg14Report.GetProperty("target_promotion_done").GetBoolean(),
                locator_update_done = qsg14Report.GetProperty("locator_update_done").GetBoolean(),
                rollback_review_rows = 0,
                final_readiness_checks_run_after_semantic_graduation = 0,
                final_readiness_passes = 0,
                final_readiness_claimed = qsg14Report.GetProperty("final_readiness_claimed").GetBoolean(),
                remaining_blockers = qsg14Validation.GetProperty("missing_evidence_for_common_readiness").GetArrayLength()
            },
            qsg15 = new
            {
                pair_count = qsg14SemanticRows.Count,
                semantic_graduation_accepted_pairs = qsg14SemanticGraduationPairs,
                limited_semantic_quality_evidence_accepted_pairs = qsg14LimitedSemanticEvidencePairs,
                common_readiness_graduated_pairs = 0,
                target_promotion_review_packets = 1,
                target_promotion_done = false,
                locator_update_done = false,
                locator_review_rows = 1,
                rollback_review_rows = 1,
                final_readiness_checks_run_after_semantic_graduation = finalReadinessRows.Length,
                final_readiness_passes = finalReadinessPasses,
                final_readiness_in_progress = finalReadinessInProgress,
                final_readiness_claimed = false,
                remaining_blockers = remainingBlockers.Length
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_15.promotion_locator_rollback_final_readiness_report.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-15",
            input_phase_id = "QUALITY-SEMANTIC-GATES-14",
            qsg14_report_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-report.json",
            target_promotion_review_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-promotion-review.json",
            locator_rollback_rows_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-locator-rollback-review-rows.jsonl",
            final_readiness_checks_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-final-readiness-checks.jsonl",
            pair_count = qsg14SemanticRows.Count,
            semantic_graduation_accepted_pairs = qsg14SemanticGraduationPairs,
            limited_semantic_quality_evidence_accepted_pairs = qsg14LimitedSemanticEvidencePairs,
            bounded_semantic_quality_claim_allowed = qsg14Report.GetProperty("bounded_semantic_quality_claim_allowed").GetBoolean(),
            quality_improvement_claim_allowed = qsg14Report.GetProperty("quality_improvement_claim_allowed").GetBoolean(),
            token_savings_claim_allowed = qsg14Report.GetProperty("token_savings_claim_allowed").GetBoolean(),
            common_readiness_graduated_pairs = 0,
            common_readiness_claim_allowed = false,
            target_promotion_review_packet_created = true,
            target_promotion_done = false,
            locator_update_done = false,
            rollback_closure_done = false,
            product_runtime_semantic_quality_claimed = false,
            product_runtime_semantic_quality_verified = false,
            promotion_readiness_claimed = false,
            product_readiness_claim_allowed = false,
            final_readiness_checks_run_after_semantic_graduation = finalReadinessRows.Length,
            final_readiness_passes = finalReadinessPasses,
            final_readiness_in_progress = finalReadinessInProgress,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            deterministic_helper_classification = "QSG-15 is an artifact-level promotion/readiness review over QSG-14 evidence and exact required target artifacts; it is not runtime semantic logic and does not hide deterministic review logic as product quality.",
            before_after_counts = beforeAfterCounts,
            advanced_in_this_slice = new[]
            {
                "Created an explicit target-promotion review packet after QSG-14 evidence evaluation.",
                "Converted QSG-14's target-review and final-readiness blockers into concrete QSG-15 review artifacts.",
                "Ran the ten final readiness checks as recorded IN_PROGRESS rows with exact missing evidence instead of claiming readiness."
            },
            claims_opened_in_this_slice = Array.Empty<string>(),
            claims_kept_closed = new[]
            {
                "common_readiness_claim_allowed",
                "product_runtime_semantic_quality_claimed",
                "learnability_claimed",
                "target_promotion_done",
                "locator_update_done",
                "rollback_closure_done",
                "promotion_readiness_claimed",
                "product_readiness_claim_allowed",
                "final_readiness_claimed"
            },
            remaining_blockers = remainingBlockers,
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-16: create a named common-readiness target/rollback/locator candidate set and run exact target fresh-load, continue-training, held-out, ablation, rollback, and product-runtime semantic verification before any promotion or final readiness claim."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_15.validation.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-15",
            target_promotion_review_packet_created = true,
            target_promotion_review_packet_expected = true,
            target_promotion_review_packet_actual = true,
            required_promotion_artifacts_expected = requiredPromotionArtifacts.Length,
            required_promotion_artifacts_present = presentPromotionArtifacts.Length,
            required_promotion_artifacts_missing = missingPromotionArtifacts.Length,
            final_readiness_checks_expected = 10,
            final_readiness_checks_actual = finalReadinessRows.Length,
            final_readiness_passes_expected = 0,
            final_readiness_passes_actual = finalReadinessPasses,
            final_readiness_in_progress_actual = finalReadinessInProgress,
            common_readiness_graduated_pairs_expected = 0,
            common_readiness_graduated_pairs_actual = 0,
            target_promotion_done = false,
            locator_update_done = false,
            rollback_closure_done = false,
            product_runtime_semantic_quality_claimed = false,
            product_runtime_semantic_quality_verified = false,
            common_readiness_claim_allowed = false,
            promotion_readiness_claimed = false,
            product_readiness_claim_allowed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            missing_evidence_for_promotion = missingPromotionArtifacts.Select(artifact => artifact.MissingReason).ToArray(),
            output_files = new[]
            {
                Path.GetFullPath(targetPromotionReviewPath),
                Path.GetFullPath(targetPromotionReviewMdPath),
                Path.GetFullPath(locatorRollbackRowsPath),
                Path.GetFullPath(finalReadinessRowsPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath),
                Path.GetFullPath(statePatchPath)
            }
        };

        WriteJson(targetPromotionReviewPath, targetPromotionReview);
        WriteQsg15PromotionReviewMarkdown(targetPromotionReviewMdPath, missingPromotionArtifacts);
        WriteJsonl(locatorRollbackRowsPath, locatorRollbackRows);
        WriteJsonl(finalReadinessRowsPath, finalReadinessRows);
        WriteJson(reportPath, report);
        WriteQsg15Markdown(reportMdPath, beforeAfterCounts, remainingBlockers);
        WriteJson(validationPath, validation);
        statePatchPath = UpdateState(report, validation);

        Assert.Equal(54, qsg14SemanticRows.Count);
        Assert.Equal(qsg14Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(), qsg14SemanticGraduationPairs);
        Assert.Equal(
            qsg14Report.GetProperty("semantic_quality_gate_passed").GetBoolean()
            && qsg14Report.GetProperty("bounded_semantic_quality_claim_allowed").GetBoolean(),
            qsg14ReadyForBoundedSemanticOnly);
        Assert.False(targetPromotionAllowed);
        Assert.True(missingPromotionArtifacts.Length > 0);
        Assert.Equal(10, finalReadinessRows.Length);
        Assert.Equal(0, finalReadinessPasses);
        Assert.Equal(10, finalReadinessInProgress);

        using var writtenReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = writtenReport.RootElement;
        Assert.True(root.GetProperty("target_promotion_review_packet_created").GetBoolean());
        Assert.Equal(10, root.GetProperty("final_readiness_checks_run_after_semantic_graduation").GetInt32());
        Assert.Equal(0, root.GetProperty("final_readiness_passes").GetInt32());
        Assert.Equal(10, root.GetProperty("final_readiness_in_progress").GetInt32());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(root.GetProperty("locator_update_done").GetBoolean());
        Assert.False(root.GetProperty("rollback_closure_done").GetBoolean());
        Assert.False(root.GetProperty("product_runtime_semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("product_runtime_semantic_quality_verified").GetBoolean());
        Assert.False(root.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("training_started").GetBoolean());
        Assert.False(root.GetProperty("checkpoint_mutation_started").GetBoolean());
        AssertJsonStringArray(root.GetProperty("remaining_blockers"), remainingBlockers);

        using var writtenPromotionReview = JsonDocument.Parse(File.ReadAllText(targetPromotionReviewPath));
        var promotionRoot = writtenPromotionReview.RootElement;
        Assert.Equal("KEEP_CLOSED", promotionRoot.GetProperty("target_promotion_decision").GetString());
        Assert.True(promotionRoot.GetProperty("target_promotion_review_packet_created").GetBoolean());
        Assert.False(promotionRoot.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(promotionRoot.GetProperty("locator_update_done").GetBoolean());
        Assert.False(promotionRoot.GetProperty("rollback_closure_done").GetBoolean());
        Assert.False(promotionRoot.GetProperty("final_readiness_claimed").GetBoolean());

        using var writtenValidation = JsonDocument.Parse(File.ReadAllText(validationPath));
        var validationRoot = writtenValidation.RootElement;
        Assert.True(validationRoot.GetProperty("target_promotion_review_packet_created").GetBoolean());
        Assert.Equal(10, validationRoot.GetProperty("final_readiness_checks_actual").GetInt32());
        Assert.Equal(0, validationRoot.GetProperty("final_readiness_passes_actual").GetInt32());
        Assert.Equal(10, validationRoot.GetProperty("final_readiness_in_progress_actual").GetInt32());
        Assert.False(validationRoot.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(validationRoot.GetProperty("locator_update_done").GetBoolean());
        Assert.False(validationRoot.GetProperty("rollback_closure_done").GetBoolean());
        Assert.False(validationRoot.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("final_readiness_claimed").GetBoolean());

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal("QUALITY-SEMANTIC-GATES-15", stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.True(stateRoot.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(stateRoot.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(stateRoot.GetProperty("learnability_claimed").GetBoolean());
        var stateReport = writtenStatePatch.RootElement.GetProperty("report");
        Assert.True(stateReport.GetProperty("target_promotion_review_packet_created").GetBoolean());
        Assert.Equal(10, stateReport.GetProperty("final_readiness_checks_run_after_semantic_graduation").GetInt32());
        Assert.Equal(0, stateReport.GetProperty("final_readiness_passes").GetInt32());
        Assert.False(stateReport.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(stateReport.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(stateReport.GetProperty("locator_update_done").GetBoolean());
        Assert.False(stateReport.GetProperty("rollback_closure_done").GetBoolean());
        Assert.False(stateReport.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(stateReport.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.StartsWith(
            "QUALITY-SEMANTIC-GATES-16:",
            stateRoot.GetProperty("next_action").GetString());
    }

    private static IReadOnlyList<JsonElement> ReadJsonl(string path)
        => File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToArray();

    private static PromotionArtifact[] RequiredPromotionArtifacts() =>
    [
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-manifest-candidate.json",
            ArtifactRole: "target_manifest_candidate",
            MissingReason: "exact_qsg15_common_readiness_target_manifest_candidate_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-checkpoint-candidate.json",
            ArtifactRole: "target_checkpoint_candidate",
            MissingReason: "exact_qsg15_common_readiness_target_checkpoint_candidate_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-rollback-checkpoint-candidate.json",
            ArtifactRole: "rollback_checkpoint_candidate",
            MissingReason: "exact_qsg15_rollback_checkpoint_candidate_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-locator-candidate.json",
            ArtifactRole: "locator_candidate",
            MissingReason: "exact_qsg15_locator_candidate_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-fresh-load-probe.json",
            ArtifactRole: "fresh_load_probe_against_named_target",
            MissingReason: "fresh_load_probe_against_named_qsg15_target_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-continue-training-probe.json",
            ArtifactRole: "continue_training_probe_against_named_target",
            MissingReason: "continue_training_probe_against_named_qsg15_target_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-held-out-source-disjoint-probe.json",
            ArtifactRole: "held_out_source_disjoint_probe_against_named_target",
            MissingReason: "held_out_source_disjoint_probe_against_named_qsg15_target_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-ablation-probe.json",
            ArtifactRole: "ablation_probe_against_named_target",
            MissingReason: "ablation_probe_against_named_qsg15_target_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-rollback-probe.json",
            ArtifactRole: "rollback_probe_against_named_target_and_rollback",
            MissingReason: "rollback_probe_against_named_qsg15_target_and_rollback_missing"),
        new(
            RelativePath: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-product-runtime-semantic-claim-verification.json",
            ArtifactRole: "product_runtime_semantic_claim_verification",
            MissingReason: "product_runtime_semantic_quality_claim_verification_missing")
    ];

    private static FinalReadinessCheck[] FinalReadinessChecks() =>
    [
        new(1, "Claude live host runtime readiness", "Claude path still lacks final product semantic claim plus target/rollback-backed runtime verification.", ["RUNTIME-02 Claude runtime packet exists, but QSG-15 target promotion/runtime semantic verification is missing."], ["QUALITY-SEMANTIC-GATES-16 target/runtime semantic verification for Claude"]),
        new(2, "Cursor live host runtime readiness", "Cursor path still lacks target/rollback-backed product semantic verification.", ["RUNTIME-03 rerun evidence exists, but no QSG-15 promoted target/locator/runtime semantic packet exists."], ["QUALITY-SEMANTIC-GATES-16 target/runtime semantic verification for Cursor"]),
        new(3, "Codex live host runtime readiness", "Codex path still lacks final app/chat/core-boundary readiness tied to promoted target and semantic claim.", ["Codex runtime packets exist with limitations; no QSG-15 promoted locator or product semantic verification exists."], ["QUALITY-SEMANTIC-GATES-16 target/runtime semantic verification for Codex"]),
        new(4, "Cross-host end-to-end behavior", "Cross-host scenario pack does not yet prove the promoted target through Claude, Cursor, and Codex together.", ["QSG-14 is scenario-pack semantic evidence; QSG-15 has no promoted target cross-host runtime packet."], ["QUALITY-SEMANTIC-GATES-16 cross-host target runtime packet"]),
        new(5, "Token before/after", "Bounded QSG token savings exists, but final token readiness must be tied to the exact promoted target and no quality regression.", ["QSG-14 token claim is bounded to QSG13 paired traces, not a promoted product target."], ["QUALITY-SEMANTIC-GATES-16 target token before/after packet"]),
        new(6, "Quality improvement", "Quality improvement is bounded to QSG scenario evidence and not product runtime semantic readiness.", ["QSG-14 accepted bounded semantic evidence; QSG-15 product runtime semantic verification is missing."], ["QUALITY-SEMANTIC-GATES-16 product runtime quality packet"]),
        new(7, "Learnability and DPO", "Full learnability remains unproven for promoted common readiness because target/rollback/DPO lifecycle packets are absent.", ["Narrow user-local learnability exists historically; common readiness promotion artifacts are missing."], ["QUALITY-SEMANTIC-GATES-16 machine-specific DPO/target lifecycle packet"]),
        new(8, "Backup/export/import/share and rollback", "Rollback cannot pass without named QSG-15 target and rollback probes.", ["Historical rollback/backup artifacts are not a QSG-15 target/rollback proof pair."], ["QUALITY-SEMANTIC-GATES-16 named rollback probe"]),
        new(9, "Boundary and architecture", "Core boundary remains preserved in this review slice, but final target readiness is open until product runtime and locator artifacts exist.", ["QSG-15 did not move logic into hosts or mutate checkpoints; promotion is closed."], ["QUALITY-SEMANTIC-GATES-16 locator/runtime architecture review"]),
        new(10, "Documentation and no-overclaim", "No-overclaim passes for this slice, but final docs cannot claim done while target/locator/rollback/readiness are open.", ["QSG-15 report keeps completion false and names next work."], ["QUALITY-SEMANTIC-GATES-16 docs/readiness update after target proof"])
    ];

    private static string[] ArtifactsForFinalReadinessCheck(int pass)
        => pass switch
        {
            1 => ["artifacts/extension-runtime-token-quality-learnability/claude-runtime-proof.json", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-promotion-review.json"],
            2 => ["artifacts/extension-runtime-token-quality-learnability/cursor-runtime-proof-rerun.json", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-promotion-review.json"],
            3 => ["artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-04f.json", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-promotion-review.json"],
            4 => ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-13-pair-results.jsonl", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-final-readiness-checks.jsonl"],
            5 => ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-report.json", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-promotion-review.json"],
            6 => ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-report.json", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-promotion-review.json"],
            7 => ["artifacts/extension-runtime-token-quality-learnability/adaptive-learnability-01/adaptive-learnability-01-report.json", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-target-promotion-review.json"],
            8 => ["artifacts/extension-runtime-token-quality-learnability/backup-export-import-share-report.json", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-locator-rollback-review-rows.jsonl"],
            9 => ["docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-report.json"],
            10 => ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15-report.md", "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-15.validation.json"],
            _ => []
        };

    private static void AssertJsonStringArray(JsonElement element, IReadOnlyList<string> expected)
    {
        var actual = element.EnumerateArray()
            .Select(item => item.GetString() ?? throw new InvalidOperationException("Expected a JSON string."))
            .ToArray();
        Assert.Equal(expected, actual);
    }

    private static void WriteJsonl<T>(string path, IEnumerable<T> rows)
    {
        var lines = rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions));
        File.WriteAllText(path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
    }

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllText(
            path,
            JsonSerializer.Serialize(value, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            new UTF8Encoding(false));

    private static void WriteQsg15PromotionReviewMarkdown(string path, IReadOnlyList<PromotionArtifact> missingArtifacts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-15 Target Promotion Review");
        builder.AppendLine();
        builder.AppendLine("Decision: KEEP_CLOSED.");
        builder.AppendLine();
        builder.AppendLine("QSG-14 bounded semantic evidence is accepted for the evaluated scenario pack, but QSG-15 cannot promote a common-readiness target or update a locator because exact target, rollback, locator, and product runtime verification artifacts are missing.");
        builder.AppendLine();
        builder.AppendLine("## Missing Artifacts");
        builder.AppendLine();
        foreach (var artifact in missingArtifacts)
            builder.AppendLine($"- {artifact.RelativePath}: {artifact.MissingReason}");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static void WriteQsg15Markdown(string path, object beforeAfterCounts, IReadOnlyList<string> remainingBlockers)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-15");
        builder.AppendLine();
        builder.AppendLine("QSG-15 records the target-promotion, locator, rollback, and final-readiness review packet after QSG-14 evidence evaluation. No product/runtime semantic claim is enabled.");
        builder.AppendLine();
        builder.AppendLine("## What Advanced");
        builder.AppendLine();
        builder.AppendLine("- Target-promotion review packet now exists.");
        builder.AppendLine("- Locator and rollback closure review rows now exist.");
        builder.AppendLine("- Ten final readiness checks were recorded after QSG-14 evidence evaluation; all remain IN_PROGRESS with exact missing evidence.");
        builder.AppendLine();
        builder.AppendLine("## What Stayed Closed");
        builder.AppendLine();
        builder.AppendLine("- Common readiness, product runtime semantic quality, target promotion, locator update, rollback closure, promotion readiness, product readiness, and final readiness.");
        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("## Remaining Blockers");
        builder.AppendLine();
        foreach (var blocker in remainingBlockers)
            builder.AppendLine($"- {blocker}");
        builder.AppendLine();
        builder.AppendLine("Next independent track: QUALITY-SEMANTIC-GATES-16 should create the named target/rollback/locator candidate set and run exact probes before any promotion or final readiness claim.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string UpdateState(object report, object validation)
        => ExtensionExecutionStatePatchWriter.WritePatch(
            StatePath,
            ArtifactDir,
            "QUALITY-SEMANTIC-GATES-15",
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
                ["current_phase"] = "QUALITY-SEMANTIC-GATES-15",
                ["phase_status"] = "TARGET_PROMOTION_REVIEW_RECORDED_CLAIMS_CLOSED",
                ["phase_scope"] = "QSG-15 created explicit promotion, locator, rollback, and ten-pass final-readiness review artifacts after QSG-14 evidence evaluation. Exact target/rollback/locator/product-runtime verification artifacts are missing or QSG-14 has not opened bounded semantic graduation, so common readiness, product semantic runtime claims, target promotion, locator update, rollback closure, and final readiness remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-16: create a named common-readiness target/rollback/locator candidate set and run exact target fresh-load, continue-training, held-out, ablation, rollback, and product-runtime semantic verification before any promotion or final readiness claim.",
                ["continuation_required"] = true,
                ["quality_improvement_claim_allowed"] = true,
                ["token_savings_claim_allowed"] = true,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_started"] = false
            },
            report,
            validation,
            JsonOptions);

    private sealed record PromotionArtifact(
        string RelativePath,
        string ArtifactRole,
        string MissingReason);

    private sealed record FinalReadinessCheck(
        int Pass,
        string RequiredCheck,
        string TargetOutcomeRow,
        string[] Findings,
        string[] RequiredFixRefs);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}
