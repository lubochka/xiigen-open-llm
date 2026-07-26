// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates16CandidateProbeTests
{
    private const string PhaseId = "QUALITY-SEMANTIC-GATES-16";
    private const string Command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionQualitySemanticGates16CandidateProbeTests --no-restore --nologo";

    private static readonly string RepoRoot = ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot;

    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;

    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates16Artifacts_CreateCandidateSetAndExactProbeRecordsWithoutPromotion()
    {
        var qsg15ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15-report.json");
        var qsg15ValidationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-15.validation.json");
        var qsg14ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-report.json");
        var qsg14SemanticRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-semantic-graduation-rows.jsonl");
        var qsg13PairRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-pair-results.jsonl");

        using var qsg15ReportDocument = JsonDocument.Parse(File.ReadAllText(qsg15ReportPath));
        using var qsg15ValidationDocument = JsonDocument.Parse(File.ReadAllText(qsg15ValidationPath));
        using var qsg14ReportDocument = JsonDocument.Parse(File.ReadAllText(qsg14ReportPath));
        var qsg15Report = qsg15ReportDocument.RootElement;
        var qsg15Validation = qsg15ValidationDocument.RootElement;
        var qsg14Report = qsg14ReportDocument.RootElement;
        var qsg14SemanticRows = ReadJsonl(qsg14SemanticRowsPath);
        var qsg13PairRows = ReadJsonl(qsg13PairRowsPath);

        var targetAssets = BuildMachineAssets("target", Path.Combine("adaptive-learnability-01", "user-cache-continued"));
        var rollbackAssets = BuildMachineAssets("rollback", Path.Combine("adaptive-learnability-01", "rollback"));
        var candidateId = "qsg16-common-readiness-target-candidate-v1";
        var rollbackId = "qsg16-rollback-candidate-v1";
        var locatorCandidateId = "qsg16-locator-update-proposal-v1";

        var targetManifestCandidatePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-manifest-candidate.json");
        var targetCheckpointCandidatePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-checkpoint-candidate.json");
        var rollbackCheckpointCandidatePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-rollback-checkpoint-candidate.json");
        var locatorCandidatePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-locator-candidate.json");
        var candidateSetPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-candidate-set.json");
        var freshLoadProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-fresh-load-probe.json");
        var continueTrainingProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-continue-training-probe.json");
        var heldOutProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-held-out-source-disjoint-probe.json");
        var ablationProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-ablation-probe.json");
        var rollbackProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-rollback-probe.json");
        var productRuntimeProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-product-runtime-semantic-verification-probe.json");
        var finalReadinessRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-final-readiness-checks.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16.validation.json");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, PhaseId);

        var targetManifestCandidate = new
        {
            schema = "xiigen.quality_semantic_gates_16.target_manifest_candidate.v1",
            phase_id = PhaseId,
            candidate_id = candidateId,
            candidate_status = "STAGED_INACTIVE",
            source_phase_ids = new[] { "QUALITY-SEMANTIC-GATES-14", "QUALITY-SEMANTIC-GATES-15", "LEARN-04", "LEARN-05", "LEARN-06", "LEARN-07" },
            qsg14_report_ref = Rel(qsg14ReportPath),
            qsg15_report_ref = Rel(qsg15ReportPath),
            semantic_graduation_accepted_pairs = qsg14Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
            limited_semantic_quality_evidence_accepted_pairs = qsg14Report.GetProperty("limited_semantic_quality_evidence_accepted_pairs").GetInt32(),
            common_readiness_graduated_pairs = 0,
            target_assets = targetAssets,
            promotion_state = "NOT_PROMOTED",
            locator_update_allowed = false,
            product_runtime_semantic_quality_claim_allowed = false,
            no_overclaim = CandidateNoOverclaim()
        };

        var targetCheckpointCandidate = new
        {
            schema = "xiigen.quality_semantic_gates_16.target_checkpoint_candidate.v1",
            phase_id = PhaseId,
            candidate_id = candidateId,
            candidate_status = "STAGED_INACTIVE",
            asset_count = targetAssets.Length,
            assets = targetAssets,
            all_candidate_files_exist = targetAssets.All(asset => asset.FileExists),
            all_candidate_json_parses = targetAssets.All(asset => asset.JsonParses),
            target_checkpoint_candidate_created = true,
            checkpoint_mutation_started = false,
            common_model_mutated = false,
            semantic_quality_claimed = false
        };

        var rollbackCheckpointCandidate = new
        {
            schema = "xiigen.quality_semantic_gates_16.rollback_checkpoint_candidate.v1",
            phase_id = PhaseId,
            rollback_id = rollbackId,
            candidate_status = "STAGED_INACTIVE",
            target_candidate_id = candidateId,
            asset_count = rollbackAssets.Length,
            assets = rollbackAssets,
            all_rollback_files_exist = rollbackAssets.All(asset => asset.FileExists),
            all_rollback_json_parses = rollbackAssets.All(asset => asset.JsonParses),
            rollback_checkpoint_candidate_created = true,
            rollback_closure_done = false,
            rollback_safety_claimed = false,
            semantic_quality_claimed = false
        };

        var locatorCandidate = new
        {
            schema = "xiigen.quality_semantic_gates_16.locator_candidate.v1",
            phase_id = PhaseId,
            locator_candidate_id = locatorCandidateId,
            locator_status = "PROPOSAL_ONLY_NOT_APPLIED",
            target_candidate_id = candidateId,
            rollback_candidate_id = rollbackId,
            proposed_locator_refs = targetAssets
                .Select(asset => new
                {
                    machine_id = asset.MachineId,
                    proposed_target_ref = asset.CheckpointRelativePath,
                    proposed_manifest_ref = asset.ManifestRelativePath,
                    target_sha256 = asset.CheckpointSha256,
                    rollback_ref = rollbackAssets.Single(rollback => rollback.MachineId == asset.MachineId).CheckpointRelativePath,
                    rollback_sha256 = rollbackAssets.Single(rollback => rollback.MachineId == asset.MachineId).CheckpointSha256
                })
                .ToArray(),
            locator_update_done = false,
            product_runtime_binding_done = false,
            no_overclaim = CandidateNoOverclaim()
        };

        var candidateSet = new
        {
            schema = "xiigen.quality_semantic_gates_16.candidate_set.v1",
            phase_id = PhaseId,
            candidate_set_id = "qsg16-target-rollback-locator-candidate-set-v1",
            target_manifest_candidate_ref = Rel(targetManifestCandidatePath),
            target_checkpoint_candidate_ref = Rel(targetCheckpointCandidatePath),
            rollback_checkpoint_candidate_ref = Rel(rollbackCheckpointCandidatePath),
            locator_candidate_ref = Rel(locatorCandidatePath),
            required_evidence_candidates = RequiredEvidenceCandidates(),
            candidate_artifacts_created = 4,
            probe_artifacts_expected = 6,
            promotion_allowed_now = false,
            promotion_decision = "KEEP_CLOSED",
            reason = "QSG-16 creates named inactive candidates and exact probe records only. Locator/product-runtime binding is not applied and final readiness remains closed."
        };

        var freshLoadProbe = BuildProbe(
            probeSchema: "xiigen.quality_semantic_gates_16.fresh_load_probe.v1",
            probeId: "qsg16-target-fresh-load",
            artifactRole: "fresh_load_probe",
            targetCandidateId: candidateId,
            inspectedRefs: [Rel(targetCheckpointCandidatePath), Rel(Path.Combine(ArtifactDir, "fresh-load-report.json"))],
            status: targetAssets.All(asset => asset.FileExists && asset.JsonParses)
                ? "PASS_STATIC_LOADABILITY_FOR_NAMED_CANDIDATE"
                : "FAIL_CANDIDATE_FILE_OR_JSON_PARSE",
            exactProbePassedForPromotion: false,
            whatPassed: "All named in-repo target candidate JSON assets exist and parse.",
            whatRemains: "This is static candidate loadability, not a new public machine fresh-load of the inactive QSG-16 locator target through product runtime.",
            supportingOnlyLimitation: "Fresh-load file parse/loadability alone does not prove model readiness or promotion safety.");

        var continueTrainingProbe = BuildProbe(
            probeSchema: "xiigen.quality_semantic_gates_16.continue_training_probe.v1",
            probeId: "qsg16-target-continue-training",
            artifactRole: "continue_training_probe",
            targetCandidateId: candidateId,
            inspectedRefs: [Rel(targetCheckpointCandidatePath), Rel(Path.Combine(ArtifactDir, "continue-training-report.json"))],
            status: File.Exists(Path.Combine(ArtifactDir, "continue-training-report.json"))
                ? "PASS_EXISTING_CONTINUE_TRAINING_EVIDENCE_LINKED_TO_CANDIDATE"
                : "FAIL_MISSING_CONTINUE_TRAINING_REPORT",
            exactProbePassedForPromotion: false,
            whatPassed: "Existing LEARN-04 continue-training evidence is linked to the named inactive target candidate.",
            whatRemains: "QSG-16 did not run new training and did not activate or promote the target.",
            supportingOnlyLimitation: "Continue-training evidence remains local/evidence-bound and cannot by itself prove common readiness.");

        var heldOutProbe = BuildProbe(
            probeSchema: "xiigen.quality_semantic_gates_16.held_out_source_disjoint_probe.v1",
            probeId: "qsg16-target-held-out-source-disjoint",
            artifactRole: "held_out_source_disjoint_probe",
            targetCandidateId: candidateId,
            inspectedRefs: [Rel(targetCheckpointCandidatePath), Rel(Path.Combine(ArtifactDir, "held-out-ablation-report.json"))],
            status: File.Exists(Path.Combine(ArtifactDir, "held-out-ablation-report.json"))
                ? "PASS_WITH_LIMITATIONS_EXISTING_HELD_OUT_EVIDENCE_LINKED"
                : "FAIL_MISSING_HELD_OUT_REPORT",
            exactProbePassedForPromotion: false,
            whatPassed: "Existing held-out/source-disjoint evidence is linked to the named inactive target candidate.",
            whatRemains: "PromptToTask and SkillSelection remain partial overlay checks; this is not full machine held-out readiness for all target components.",
            supportingOnlyLimitation: "Held-out evidence is target-scoped support, not host runtime, source/license, or promotion evidence.");

        var ablationProbe = BuildProbe(
            probeSchema: "xiigen.quality_semantic_gates_16.ablation_probe.v1",
            probeId: "qsg16-target-ablation",
            artifactRole: "ablation_probe",
            targetCandidateId: candidateId,
            inspectedRefs: [Rel(targetCheckpointCandidatePath), Rel(Path.Combine(ArtifactDir, "held-out-ablation-report.json"))],
            status: File.Exists(Path.Combine(ArtifactDir, "held-out-ablation-report.json"))
                ? "PASS_WITH_LIMITATIONS_EXISTING_ABLATION_EVIDENCE_LINKED"
                : "FAIL_MISSING_ABLATION_REPORT",
            exactProbePassedForPromotion: false,
            whatPassed: "Existing ablation/no-memorization evidence is linked to the named inactive target candidate.",
            whatRemains: "Ablation remains limited for overlay-only PromptToTask and SkillSelection candidates.",
            supportingOnlyLimitation: "Ablation support cannot substitute for product runtime semantic verification.");

        var rollbackProbe = BuildProbe(
            probeSchema: "xiigen.quality_semantic_gates_16.rollback_probe.v1",
            probeId: "qsg16-target-rollback",
            artifactRole: "rollback_probe",
            targetCandidateId: candidateId,
            inspectedRefs: [Rel(targetCheckpointCandidatePath), Rel(rollbackCheckpointCandidatePath), Rel(Path.Combine(ArtifactDir, "rollback-report.json"))],
            status: rollbackAssets.All(asset => asset.FileExists && asset.JsonParses) && File.Exists(Path.Combine(ArtifactDir, "rollback-report.json"))
                ? "PASS_WITH_LIMITATIONS_EXISTING_ROLLBACK_EVIDENCE_LINKED"
                : "FAIL_ROLLBACK_CANDIDATE_OR_REPORT_MISSING",
            exactProbePassedForPromotion: false,
            whatPassed: "Named rollback candidate assets exist and existing rollback evidence is linked to the QSG-16 target/rollback pair.",
            whatRemains: "Rollback closure remains closed because PromptToTask and SkillSelection rollback evidence is overlay-only and product locator rollback was not applied.",
            supportingOnlyLimitation: "Rollback proves recovery support only; it does not prove the target should be promoted.");

        var runtimeProofRefs = new[]
        {
            Rel(Path.Combine(ArtifactDir, "claude-runtime-proof.json")),
            Rel(Path.Combine(ArtifactDir, "cursor-runtime-proof-rerun.json")),
            Rel(Path.Combine(ArtifactDir, "codex-runtime-proof-04f.json"))
        };
        var productRuntimeProbe = new
        {
            schema = "xiigen.quality_semantic_gates_16.product_runtime_semantic_verification_probe.v1",
            phase_id = PhaseId,
            probe_id = "qsg16-product-runtime-semantic-verification",
            target_candidate_id = candidateId,
            locator_candidate_id = locatorCandidateId,
            artifact_role = "product_runtime_semantic_verification_probe",
            inspected_refs = runtimeProofRefs.Concat([Rel(locatorCandidatePath), Rel(targetManifestCandidatePath)]).ToArray(),
            runtime_proof_artifacts_exist = runtimeProofRefs.All(relative => File.Exists(Path.Combine(RepoRoot, relative))),
            locator_applied_to_product_runtime = false,
            target_active_in_product_runtime = false,
            product_runtime_semantic_quality_claimed = false,
            product_runtime_semantic_quality_verified = false,
            probe_status = "IN_PROGRESS_TARGET_NOT_ACTIVE_IN_PRODUCT_RUNTIME",
            exact_probe_passed_for_promotion = false,
            what_passed = "Existing host runtime packets remain available as supporting evidence and do not claim semantic quality.",
            what_remains = "No fresh Claude/Cursor/Codex product runtime invocation used the QSG-16 locator candidate as an active target, and Codex 04F remains partial app-server/chat-surface evidence.",
            supporting_only_limitation = "Product runtime semantic verification cannot pass until the named target/locator is active in the product runtime and all host paths verify the same candidate without overclaim."
        };

        var requiredEvidenceCandidates = RequiredEvidenceCandidates();
        var createdEvidenceCandidateRefs = new[]
        {
            Rel(targetManifestCandidatePath),
            Rel(targetCheckpointCandidatePath),
            Rel(rollbackCheckpointCandidatePath),
            Rel(locatorCandidatePath),
            Rel(freshLoadProbePath),
            Rel(continueTrainingProbePath),
            Rel(heldOutProbePath),
            Rel(ablationProbePath),
            Rel(rollbackProbePath),
            Rel(productRuntimeProbePath)
        };

        var remainingBlockers = new[]
        {
            "qsg16_candidates_created_but_not_promoted_or_locator_applied",
            "product_runtime_semantic_verification_in_progress_target_not_active",
            "prompt_to_task_and_skill_selection_full_machine_target_probes_remain_partial_overlay_only",
            "fresh_claude_cursor_codex_runtime_not_rerun_against_active_qsg16_locator",
            "ten_final_readiness_checks_remain_in_progress_zero_passed_after_qsg16"
        };
        var finalReadinessRows = FinalReadinessRows(createdEvidenceCandidateRefs, remainingBlockers);
        var finalReadinessPasses = finalReadinessRows.Count(row => row.CountsAsFinalReadinessPass);

        var beforeAfterCounts = new
        {
            qsg15 = new
            {
                pair_count = qsg15Report.GetProperty("pair_count").GetInt32(),
                semantic_graduation_accepted_pairs = qsg15Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
                limited_semantic_quality_evidence_accepted_pairs = qsg15Report.GetProperty("limited_semantic_quality_evidence_accepted_pairs").GetInt32(),
                required_promotion_artifacts_present = qsg15Validation.GetProperty("required_promotion_artifacts_present").GetInt32(),
                required_promotion_artifacts_missing = qsg15Validation.GetProperty("required_promotion_artifacts_missing").GetInt32(),
                named_candidate_set_created = false,
                candidate_artifacts_created = 0,
                probe_artifacts_created = 0,
                safe_probe_artifacts_executed = 0,
                full_promotion_probe_passes = 0,
                product_runtime_semantic_quality_verified = qsg15Report.GetProperty("product_runtime_semantic_quality_verified").GetBoolean(),
                target_promotion_done = qsg15Report.GetProperty("target_promotion_done").GetBoolean(),
                locator_update_done = qsg15Report.GetProperty("locator_update_done").GetBoolean(),
                rollback_closure_done = qsg15Report.GetProperty("rollback_closure_done").GetBoolean(),
                final_readiness_checks_run_after_semantic_graduation = qsg15Report.GetProperty("final_readiness_checks_run_after_semantic_graduation").GetInt32(),
                final_readiness_passes = qsg15Report.GetProperty("final_readiness_passes").GetInt32(),
                final_readiness_in_progress = qsg15Report.GetProperty("final_readiness_in_progress").GetInt32(),
                final_readiness_claimed = qsg15Report.GetProperty("final_readiness_claimed").GetBoolean(),
                remaining_blockers = qsg15Report.GetProperty("remaining_blockers").GetArrayLength()
            },
            qsg16 = new
            {
                pair_count = qsg14SemanticRows.Count,
                semantic_graduation_accepted_pairs = qsg14Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
                limited_semantic_quality_evidence_accepted_pairs = qsg14Report.GetProperty("limited_semantic_quality_evidence_accepted_pairs").GetInt32(),
                required_evidence_candidates_expected = requiredEvidenceCandidates.Length,
                required_evidence_candidates_created = createdEvidenceCandidateRefs.Length,
                named_candidate_set_created = true,
                candidate_artifacts_created = 4,
                probe_artifacts_created = 6,
                safe_probe_artifacts_executed = 6,
                safe_probe_artifacts_accepted_with_limitations = 5,
                full_promotion_probe_passes = 0,
                product_runtime_semantic_quality_verified = false,
                target_promotion_done = false,
                locator_update_done = false,
                rollback_closure_done = false,
                final_readiness_checks_run_after_semantic_graduation = finalReadinessRows.Length,
                final_readiness_passes = finalReadinessPasses,
                final_readiness_in_progress = finalReadinessRows.Count(row => row.Verdict == "IN_PROGRESS"),
                final_readiness_claimed = false,
                remaining_blockers = remainingBlockers.Length
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_16.target_candidate_probe_report.v1",
            phase_id = PhaseId,
            input_phase_id = "QUALITY-SEMANTIC-GATES-15",
            qsg15_report_ref = Rel(qsg15ReportPath),
            qsg14_report_ref = Rel(qsg14ReportPath),
            candidate_set_ref = Rel(candidateSetPath),
            target_manifest_candidate_ref = Rel(targetManifestCandidatePath),
            target_checkpoint_candidate_ref = Rel(targetCheckpointCandidatePath),
            rollback_checkpoint_candidate_ref = Rel(rollbackCheckpointCandidatePath),
            locator_candidate_ref = Rel(locatorCandidatePath),
            probe_refs = new[]
            {
                Rel(freshLoadProbePath),
                Rel(continueTrainingProbePath),
                Rel(heldOutProbePath),
                Rel(ablationProbePath),
                Rel(rollbackProbePath),
                Rel(productRuntimeProbePath)
            },
            final_readiness_checks_ref = Rel(finalReadinessRowsPath),
            pair_count = qsg14SemanticRows.Count,
            qsg13_pair_count = qsg13PairRows.Count,
            semantic_graduation_accepted_pairs = qsg14Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
            limited_semantic_quality_evidence_accepted_pairs = qsg14Report.GetProperty("limited_semantic_quality_evidence_accepted_pairs").GetInt32(),
            common_readiness_graduated_pairs = 0,
            required_evidence_candidates_created = createdEvidenceCandidateRefs.Length,
            safe_probe_artifacts_executed = 6,
            full_promotion_probe_passes = 0,
            target_manifest_candidate_created = true,
            target_checkpoint_candidate_created = true,
            rollback_checkpoint_candidate_created = true,
            locator_candidate_created = true,
            fresh_load_probe_created = true,
            continue_training_probe_created = true,
            held_out_probe_created = true,
            ablation_probe_created = true,
            rollback_probe_created = true,
            product_runtime_semantic_verification_probe_created = true,
            product_runtime_semantic_quality_verified = false,
            common_readiness_claim_allowed = false,
            product_runtime_semantic_quality_claimed = false,
            learnability_claimed = false,
            target_promotion_done = false,
            locator_update_done = false,
            rollback_closure_done = false,
            promotion_readiness_claimed = false,
            product_readiness_claim_allowed = false,
            final_readiness_checks_run_after_semantic_graduation = finalReadinessRows.Length,
            final_readiness_passes = finalReadinessPasses,
            final_readiness_in_progress = finalReadinessRows.Count(row => row.Verdict == "IN_PROGRESS"),
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            deterministic_helper_classification = "QSG-16 creates inactive candidate/probe artifacts and performs file/readback verification only. It does not hide deterministic artifact checks as product semantic quality.",
            before_after_counts = beforeAfterCounts,
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
            advanced_in_this_slice = new[]
            {
                "Created a named inactive target manifest/checkpoint candidate.",
                "Created a named rollback checkpoint candidate.",
                "Created a locator update proposal without applying it.",
                "Recorded exact QSG-16 fresh-load, continue-training, held-out, ablation, rollback, and product-runtime semantic verification probe artifacts.",
                "Converted QSG-15's 0/10 exact artifact gap into 10/10 created evidence candidate artifacts while keeping promotion/readiness closed."
            },
            remaining_blockers = remainingBlockers,
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-17: run or repair the active-product-runtime target verification path for the QSG-16 locator candidate, including fresh Claude/Cursor/Codex target-bound runtime semantic probes and full-machine PromptToTask/SkillSelection closure if promotion/readiness is still desired."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_16.validation.v1",
            phase_id = PhaseId,
            required_evidence_candidates_expected = requiredEvidenceCandidates.Length,
            required_evidence_candidates_created = createdEvidenceCandidateRefs.Length,
            required_evidence_candidates_missing = requiredEvidenceCandidates.Length - createdEvidenceCandidateRefs.Length,
            candidate_artifacts_expected = 4,
            candidate_artifacts_created = 4,
            probe_artifacts_expected = 6,
            probe_artifacts_created = 6,
            safe_probe_artifacts_executed = 6,
            safe_probe_artifacts_accepted_with_limitations = 5,
            full_promotion_probe_passes = 0,
            final_readiness_checks_expected = 10,
            final_readiness_checks_actual = finalReadinessRows.Length,
            final_readiness_passes_actual = finalReadinessPasses,
            final_readiness_in_progress_actual = finalReadinessRows.Count(row => row.Verdict == "IN_PROGRESS"),
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
            remaining_blockers = remainingBlockers,
            output_files = createdEvidenceCandidateRefs
                .Concat([
                    Rel(candidateSetPath),
                    Rel(finalReadinessRowsPath),
                    Rel(reportPath),
                    Rel(reportMdPath),
                    Rel(validationPath),
                    Rel(statePatchPath)
                ])
                .Select(relative => Path.GetFullPath(Path.Combine(RepoRoot, relative)))
                .ToArray()
        };

        WriteJson(targetManifestCandidatePath, targetManifestCandidate);
        WriteJson(targetCheckpointCandidatePath, targetCheckpointCandidate);
        WriteJson(rollbackCheckpointCandidatePath, rollbackCheckpointCandidate);
        WriteJson(locatorCandidatePath, locatorCandidate);
        WriteJson(candidateSetPath, candidateSet);
        WriteJson(freshLoadProbePath, freshLoadProbe);
        WriteJson(continueTrainingProbePath, continueTrainingProbe);
        WriteJson(heldOutProbePath, heldOutProbe);
        WriteJson(ablationProbePath, ablationProbe);
        WriteJson(rollbackProbePath, rollbackProbe);
        WriteJson(productRuntimeProbePath, productRuntimeProbe);
        WriteJsonl(finalReadinessRowsPath, finalReadinessRows);
        WriteJson(reportPath, report);
        WriteMarkdown(reportMdPath, beforeAfterCounts, remainingBlockers, createdEvidenceCandidateRefs);
        WriteJson(validationPath, validation);
        statePatchPath = UpdateState(report, validation);

        Assert.Equal(54, qsg14SemanticRows.Count);
        Assert.Equal(54, qsg13PairRows.Count);
        Assert.Equal(10, qsg15Validation.GetProperty("required_promotion_artifacts_missing").GetInt32());
        Assert.Equal(10, createdEvidenceCandidateRefs.Length);
        Assert.All(createdEvidenceCandidateRefs, relative => Assert.True(File.Exists(Path.Combine(RepoRoot, relative)), relative));
        Assert.Equal(10, finalReadinessRows.Length);
        Assert.Equal(0, finalReadinessPasses);

        using var writtenReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = writtenReport.RootElement;
        Assert.Equal(10, root.GetProperty("required_evidence_candidates_created").GetInt32());
        Assert.Equal(6, root.GetProperty("safe_probe_artifacts_executed").GetInt32());
        Assert.Equal(0, root.GetProperty("full_promotion_probe_passes").GetInt32());
        Assert.False(root.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(root.GetProperty("locator_update_done").GetBoolean());
        Assert.False(root.GetProperty("rollback_closure_done").GetBoolean());
        Assert.False(root.GetProperty("product_runtime_semantic_quality_verified").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());

        using var writtenRuntimeProbe = JsonDocument.Parse(File.ReadAllText(productRuntimeProbePath));
        Assert.Equal(
            "IN_PROGRESS_TARGET_NOT_ACTIVE_IN_PRODUCT_RUNTIME",
            writtenRuntimeProbe.RootElement.GetProperty("probe_status").GetString());
        Assert.False(writtenRuntimeProbe.RootElement.GetProperty("product_runtime_semantic_quality_verified").GetBoolean());

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal(PhaseId, stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.False(stateRoot.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(stateRoot.GetProperty("locator_update_done").GetBoolean());
        Assert.False(stateRoot.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(stateRoot.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.StartsWith("QUALITY-SEMANTIC-GATES-17:", stateRoot.GetProperty("next_action").GetString());
    }

    private static CandidateAsset[] BuildMachineAssets(string role, string rootRelative)
    {
        var root = Path.Combine(ArtifactDir, rootRelative);
        return
        [
            BuildAsset(role, "PromptToTaskMachine", root, Path.Combine("prompt-to-task", "prompt-to-task-v1"), "prompt-to-task-v1-checkpoint.json"),
            BuildAsset(role, "OrchestrationReasoningMachine", root, Path.Combine("orchestration-reasoning", "orchestration-reasoning-v1"), "checkpoint.json"),
            BuildAsset(role, "SkillSelectionMachine", root, Path.Combine("skill-selection", "skill-selection-v1"), "skill-selection-v1-checkpoint.json")
        ];
    }

    private static CandidateAsset BuildAsset(
        string role,
        string machineId,
        string root,
        string slot,
        string checkpointFileName)
    {
        var checkpointPath = Path.Combine(root, slot, checkpointFileName);
        var manifestPath = Path.Combine(root, slot, "manifest.json");
        return new CandidateAsset(
            Role: role,
            MachineId: machineId,
            CheckpointRelativePath: Rel(checkpointPath),
            ManifestRelativePath: File.Exists(manifestPath) ? Rel(manifestPath) : null,
            CheckpointSha256: File.Exists(checkpointPath) ? Sha256File(checkpointPath) : null,
            ManifestSha256: File.Exists(manifestPath) ? Sha256File(manifestPath) : null,
            FileExists: File.Exists(checkpointPath),
            ManifestExists: File.Exists(manifestPath),
            JsonParses: File.Exists(checkpointPath) && JsonParses(checkpointPath) && (!File.Exists(manifestPath) || JsonParses(manifestPath)));
    }

    private static object BuildProbe(
        string probeSchema,
        string probeId,
        string artifactRole,
        string targetCandidateId,
        string[] inspectedRefs,
        string status,
        bool exactProbePassedForPromotion,
        string whatPassed,
        string whatRemains,
        string supportingOnlyLimitation)
        => new
        {
            schema = probeSchema,
            phase_id = PhaseId,
            probe_id = probeId,
            target_candidate_id = targetCandidateId,
            artifact_role = artifactRole,
            inspected_refs = inspectedRefs,
            inspected_artifacts_exist = inspectedRefs.All(relative => File.Exists(Path.Combine(RepoRoot, relative))),
            probe_status = status,
            exact_probe_passed_for_promotion = exactProbePassedForPromotion,
            semantic_quality_claimed = false,
            target_promotion_done = false,
            locator_update_done = false,
            what_passed = whatPassed,
            what_remains = whatRemains,
            supporting_only_limitation = supportingOnlyLimitation
        };

    private static FinalReadinessRow[] FinalReadinessRows(
        IReadOnlyList<string> createdEvidenceCandidateRefs,
        IReadOnlyList<string> remainingBlockers)
        => FinalReadinessChecks()
            .Select(check => new FinalReadinessRow(
                Schema: "xiigen.quality_semantic_gates_16.final_readiness_check.v1",
                PhaseId: PhaseId,
                FinalReadinessPass: check.Pass,
                SubagentId: $"QUALITY-SEMANTIC-GATES-16-FINAL-READINESS-{check.Pass:00}",
                TargetOutcomeRow: check.TargetOutcomeRow,
                RequiredCheck: check.RequiredCheck,
                ArtifactsInspected: check.ArtifactsInspected.Concat(createdEvidenceCandidateRefs).Distinct(StringComparer.Ordinal).ToArray(),
                Verdict: "IN_PROGRESS",
                ExactlyWhatPassed: check.WhatPassed,
                ExactlyWhatRemains: check.WhatRemains,
                Findings: check.Findings,
                RequiredFixRefs: ["QUALITY-SEMANTIC-GATES-17 active target/product-runtime verification"],
                RecheckEvidence: "QSG-16 created candidate/probe artifacts, but no final readiness row can PASS until active product runtime, full target, rollback, locator, and no-overclaim gates pass together.",
                CountsAsFinalReadinessPass: false,
                DashboardOrHelperSubstituteUsed: false,
                RemainingBlockers: remainingBlockers.ToArray()))
            .ToArray();

    private static FinalReadinessCheck[] FinalReadinessChecks() =>
    [
        new(1, "Claude live host runtime readiness", "Claude path has historical runtime evidence, but not with the active QSG-16 locator target.", ["artifacts/extension-runtime-token-quality-learnability/claude-runtime-proof.json"], "Claude runtime packet exists as supporting evidence.", "Fresh Claude product runtime invocation against the active QSG-16 target/locator remains missing.", ["QSG-16 did not apply the locator or run a fresh Claude target-bound semantic probe."]),
        new(2, "Cursor live host runtime readiness", "Cursor path has command runtime evidence, but not with the active QSG-16 locator target.", ["artifacts/extension-runtime-token-quality-learnability/cursor-runtime-proof-rerun.json"], "Cursor runtime rerun packet exists as supporting evidence.", "Fresh Cursor product runtime invocation against the active QSG-16 target/locator remains missing.", ["QSG-16 did not apply the locator or run a fresh Cursor target-bound semantic probe."]),
        new(3, "Codex live host runtime readiness", "Codex path remains partial and is not target-bound.", ["artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-04f.json"], "Codex 04F app-server/chat-message surface evidence exists.", "Direct plugin/app/core boundary target-bound Codex product semantic verification remains missing.", ["Codex 04F explicitly records adapter/core execution as unproven."]),
        new(4, "Cross-host end-to-end behavior", "Cross-host target-bound behavior remains unproven.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-16-product-runtime-semantic-verification-probe.json"], "QSG-16 names the candidate set for future cross-host verification.", "No same active target/locator scenario pack was verified across Claude, Cursor, and Codex.", ["Product runtime probe is IN_PROGRESS."]),
        new(5, "Token before/after", "Bounded token evidence exists but is not tied to an active promoted target.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-report.json"], "QSG-14 bounded token savings evidence remains available.", "Final token readiness for active product target remains missing.", ["No active QSG-16 target-bound token run was performed."]),
        new(6, "Quality improvement", "Bounded quality evidence exists but product runtime semantic verification is still in progress.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-report.json"], "QSG-14 bounded semantic graduation remains available.", "Product runtime semantic quality is not verified or claimed.", ["QSG-16 keeps product runtime semantic quality closed."]),
        new(7, "Learnability and DPO", "Learnability support exists but full common readiness remains partial.", ["artifacts/extension-runtime-token-quality-learnability/continue-training-report.json", "artifacts/extension-runtime-token-quality-learnability/held-out-ablation-report.json"], "Continue-training and held-out/ablation support are linked to the candidate.", "PromptToTask and SkillSelection full-machine target probes remain partial overlay-only.", ["Full-machine closure is still missing for two components."]),
        new(8, "Backup/export/import/share and rollback", "Rollback candidate and probe exist, but rollback closure is not applied.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-16-target-rollback-probe.json"], "Named rollback candidate/probe artifacts exist.", "Locator rollback was not applied and overlay-only rollback limitations remain.", ["Rollback closure remains false."]),
        new(9, "Boundary and architecture", "Core boundary is preserved; locator/product binding remains inactive.", ["docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"], "QSG-16 did not move logic into host adapters and did not mutate checkpoints.", "Active product runtime target binding remains missing.", ["No product semantic claim is enabled."]),
        new(10, "Documentation and no-overclaim", "No-overclaim passes for QSG-16, but final readiness remains closed.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-16-report.md"], "QSG-16 report names what is proved and keeps readiness closed.", "Final docs/report cannot say ready while active target/runtime rows remain unproven.", ["QSG-17 is required."])
    ];

    private static string[] RequiredEvidenceCandidates() =>
    [
        "named_target_manifest_checkpoint_candidate",
        "named_target_checkpoint_candidate",
        "rollback_checkpoint_candidate",
        "locator_candidate_update_proposal",
        "fresh_load_probe",
        "continue_training_probe",
        "held_out_source_disjoint_probe",
        "ablation_probe",
        "rollback_probe",
        "product_runtime_semantic_verification_probe"
    ];

    private static object CandidateNoOverclaim()
        => new
        {
            semantic_quality_claimed_in_product_runtime = false,
            common_readiness_claimed = false,
            target_promoted = false,
            locator_updated = false,
            rollback_safety_claimed = false,
            final_readiness_claimed = false,
            deterministic_review_logic_is_not_runtime_semantic_logic = true
        };

    private static IReadOnlyList<JsonElement> ReadJsonl(string path)
        => File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToArray();

    private static void WriteMarkdown(
        string path,
        object beforeAfterCounts,
        IReadOnlyList<string> remainingBlockers,
        IReadOnlyList<string> createdEvidenceCandidateRefs)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-16");
        builder.AppendLine();
        builder.AppendLine("QSG-16 creates the named target, rollback, and locator candidate set requested by QSG-15 and records exact probe artifacts where safe. It does not promote the target, apply a locator, or claim product runtime semantic quality.");
        builder.AppendLine();
        builder.AppendLine("## Candidate And Probe Artifacts");
        builder.AppendLine();
        foreach (var relative in createdEvidenceCandidateRefs)
            builder.AppendLine($"- {relative}");
        builder.AppendLine();
        builder.AppendLine("## What Stayed Closed");
        builder.AppendLine();
        builder.AppendLine("- Common readiness, product runtime semantic quality, learnability, target promotion, locator update, rollback closure, promotion readiness, product readiness, and final readiness.");
        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("## Remaining Blockers");
        foreach (var blocker in remainingBlockers)
            builder.AppendLine($"- {blocker}");
        builder.AppendLine();
        builder.AppendLine("What this proves: QSG-16 has a named inactive candidate/probe set with exact artifact refs.");
        builder.AppendLine();
        builder.AppendLine("What this does not prove: active product runtime semantic quality, full common readiness, promotion safety, applied locator update, rollback closure, or final readiness.");
        builder.AppendLine();
        builder.AppendLine("Next independent track: QUALITY-SEMANTIC-GATES-17 should run or repair active target-bound product runtime verification before any promotion/readiness claim.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string UpdateState(object report, object validation)
    {
        var validationOutputFiles = JsonSerializer.SerializeToNode(validation, JsonOptions)!["output_files"]!
            .AsArray()
            .Select(node => node!.GetValue<string>())
            .ToArray();
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, PhaseId);
        var filesToStage = new[]
            {
                Path.Combine(RepoRoot, "Tests", "ExtensionQualitySemanticGates16CandidateProbeTests.cs")
            }
            .Concat(validationOutputFiles)
            .Concat([statePatchPath])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ExtensionExecutionStatePatchWriter.WritePatch(
            StatePath,
            ArtifactDir,
            PhaseId,
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
                ["current_phase"] = PhaseId,
                ["phase_status"] = "TARGET_ROLLBACK_LOCATOR_CANDIDATE_SET_CREATED_EXACT_PROBES_RECORDED_CLAIMS_CLOSED",
                ["phase_scope"] = "QSG-16 created named inactive target/rollback/locator candidates and exact fresh-load, continue-training, held-out, ablation, rollback, and product-runtime semantic verification probe records. Product runtime target binding is still inactive, so promotion/readiness claims remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-17: run or repair the active-product-runtime target verification path for the QSG-16 locator candidate, including fresh Claude/Cursor/Codex target-bound runtime semantic probes and full-machine PromptToTask/SkillSelection closure if promotion/readiness is still desired.",
                ["continuation_required"] = true,
                ["implementation_authorized_scope"] = "QUALITY-SEMANTIC-GATES-16 bounded sub-agent slice: create named inactive target/rollback/locator candidates and exact probe artifacts; no commit/push, irreversible promotion, locator application, host-side logic, external AI, or product readiness claim.",
                ["quality_improvement_claim_allowed"] = true,
                ["token_savings_claim_allowed"] = true,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["common_readiness_claim_allowed"] = false,
                ["product_readiness_claim_allowed"] = false,
                ["target_promotion_done"] = false,
                ["locator_update_done"] = false,
                ["rollback_closure_done"] = false,
                ["product_runtime_semantic_quality_claimed"] = false,
                ["product_runtime_semantic_quality_verified"] = false,
                ["promotion_readiness_claimed"] = false,
                ["final_readiness_claimed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_started"] = false,
                ["runtime_claimed"] = false,
                ["eval_started"] = true,
                ["external_ai_used"] = false,
                ["git_authority_seen"] = "commit/push forbidden by current QUALITY-SEMANTIC-GATES-16 work order; pending commit boundary recorded only",
                ["git_write_or_ref_commands_run"] = Array.Empty<string>(),
                ["git_directives_emitted"] = Array.Empty<string>(),
                ["pending_commit_boundary"] = new
                {
                    pending = true,
                    phase_id = PhaseId,
                    commit_push_forbidden_by_current_work_order = true,
                    parent_will_perform_commit_push_after_review = true,
                    exact_commit_message = "phase QUALITY-SEMANTIC-GATES-16: create target rollback locator candidates",
                    files_to_stage = filesToStage,
                    phase_commit_accepted_by_worker_evidence = true
                }
            },
            report,
            validation,
            JsonOptions);
    }

    private static bool JsonParses(string path)
    {
        try
        {
            using var _ = JsonDocument.Parse(File.ReadAllText(path));
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string Rel(string path)
        => Path.GetRelativePath(RepoRoot, path).Replace('\\', '/');

    private static string Sha256File(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

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

    private sealed record CandidateAsset(
        string Role,
        string MachineId,
        string CheckpointRelativePath,
        string? ManifestRelativePath,
        string? CheckpointSha256,
        string? ManifestSha256,
        bool FileExists,
        bool ManifestExists,
        bool JsonParses);

    private sealed record FinalReadinessCheck(
        int Pass,
        string RequiredCheck,
        string TargetOutcomeRow,
        string[] ArtifactsInspected,
        string WhatPassed,
        string WhatRemains,
        string[] Findings);

    private sealed record FinalReadinessRow(
        string Schema,
        string PhaseId,
        int FinalReadinessPass,
        string SubagentId,
        string TargetOutcomeRow,
        string RequiredCheck,
        string[] ArtifactsInspected,
        string Verdict,
        string ExactlyWhatPassed,
        string ExactlyWhatRemains,
        string[] Findings,
        string[] RequiredFixRefs,
        string RecheckEvidence,
        bool CountsAsFinalReadinessPass,
        bool DashboardOrHelperSubstituteUsed,
        string[] RemainingBlockers);

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
