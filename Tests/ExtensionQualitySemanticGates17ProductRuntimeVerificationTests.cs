// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using SmartSkillsDomain;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates17ProductRuntimeVerificationTests
{
    private const string PhaseId = "QUALITY-SEMANTIC-GATES-17";
    private const string InputPhaseId = "QUALITY-SEMANTIC-GATES-16";
    private const string Command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionQualitySemanticGates17ProductRuntimeVerificationTests --no-restore --nologo";

    private static readonly string RepoRoot = ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot;

    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;

    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates17_ActivatesQsg16LocatorCandidateInTestOnlyProductApiRuntime()
    {
        var qsg16ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-report.json");
        var qsg16ValidationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16.validation.json");
        var qsg16LocatorPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-locator-candidate.json");
        var qsg16TargetManifestPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-manifest-candidate.json");
        var qsg16TargetCheckpointPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-checkpoint-candidate.json");
        var qsg16RollbackCheckpointPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-rollback-checkpoint-candidate.json");
        var qsg16ProductRuntimeProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-product-runtime-semantic-verification-probe.json");

        var activeLocatorPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-test-only-active-locator.json");
        var hostRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-host-target-bound-runtime-rows.jsonl");
        var productRuntimeProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-product-runtime-semantic-verification-probe.json");
        var finalReadinessRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-final-readiness-checks.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17.validation.json");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, PhaseId);

        using var qsg16ReportDocument = JsonDocument.Parse(File.ReadAllText(qsg16ReportPath, Encoding.UTF8));
        using var qsg16ValidationDocument = JsonDocument.Parse(File.ReadAllText(qsg16ValidationPath, Encoding.UTF8));
        using var qsg16LocatorDocument = JsonDocument.Parse(File.ReadAllText(qsg16LocatorPath, Encoding.UTF8));
        using var qsg16TargetManifestDocument = JsonDocument.Parse(File.ReadAllText(qsg16TargetManifestPath, Encoding.UTF8));
        using var qsg16ProductRuntimeProbeDocument = JsonDocument.Parse(File.ReadAllText(qsg16ProductRuntimeProbePath, Encoding.UTF8));

        var qsg16Report = qsg16ReportDocument.RootElement;
        var qsg16Validation = qsg16ValidationDocument.RootElement;
        var qsg16Locator = qsg16LocatorDocument.RootElement;
        var qsg16TargetManifest = qsg16TargetManifestDocument.RootElement;
        var qsg16ProductRuntimeProbe = qsg16ProductRuntimeProbeDocument.RootElement;

        var targetCandidateId = qsg16Locator.GetProperty("target_candidate_id").GetString()!;
        var locatorCandidateId = qsg16Locator.GetProperty("locator_candidate_id").GetString()!;
        var rollbackCandidateId = qsg16Locator.GetProperty("rollback_candidate_id").GetString()!;
        var rawLocatorRefs = ReadLocatorRefs(qsg16Locator).ToArray();
        var locatorShapeValidation = ValidateLocatorRefShape(rawLocatorRefs);
        Assert.True(locatorShapeValidation.IsValid, FormatLocatorEvidenceDiagnostics(locatorShapeValidation.Diagnostics));

        var targetAssetRoot = locatorShapeValidation.TargetAssetRoot;
        var targetAssetRootMaterialized = EnsureTargetAssetRootFromUserCache(targetAssetRoot);
        var rollbackAssetRoot = locatorShapeValidation.RollbackAssetRoot;
        var rollbackAssetRootMaterialized = EnsureRollbackAssetRootFromRepoCommon(rollbackAssetRoot);
        var locatorEvidenceValidation = ValidateLocatorEvidenceForTestOnlyActivation(rawLocatorRefs);
        Assert.True(locatorEvidenceValidation.IsValid, FormatLocatorEvidenceDiagnostics(locatorEvidenceValidation.Diagnostics));

        var locatorRefs = locatorEvidenceValidation.LocatorRefs.ToArray();
        var rollbackHashMatches = locatorEvidenceValidation.RollbackHashMatches;
        var targetHashMatches = locatorEvidenceValidation.TargetHashMatches;
        var targetManifestsExist = locatorEvidenceValidation.TargetManifestsExist;
        var targetLoadCompatibility = ExtensionAdaptiveCandidateAssetCompatibility.VerifyTargetAssetsWithProductionLoaders(targetAssetRoot);
        var targetRootExists = Directory.Exists(targetAssetRoot);
        var safeToActivateTestOnly = targetHashMatches
            && rollbackHashMatches
            && targetManifestsExist
            && targetRootExists
            && targetLoadCompatibility.AllLoadersAccepted
            && !qsg16Locator.GetProperty("locator_update_done").GetBoolean()
            && !qsg16Locator.GetProperty("product_runtime_binding_done").GetBoolean();

        var hostRows = safeToActivateTestOnly
            ? RunHostShapedProductApiProbe(targetAssetRoot, targetCandidateId, locatorCandidateId)
            : Array.Empty<HostRuntimeRow>();

        var targetBoundHostProbeCount = hostRows.Count(row => row.TargetBoundProductApiProbeExecuted);
        var targetBoundHostProbePasses = hostRows.Count(row => row.TargetBoundProductApiProbePassed);
        var activeProductRuntimeTargetVerified = safeToActivateTestOnly
            && hostRows.Length == 3
            && hostRows.All(row => row.TargetBoundProductApiProbePassed);
        var productRuntimeSemanticQualityVerified = activeProductRuntimeTargetVerified
            && hostRows.All(row => row.SemanticQualityClaimed == "true");
        var noOverclaimVerified = hostRows.Length == 3
            && hostRows.All(row => row.SemanticQualityClaimed == "false")
            && hostRows.All(row => row.RuntimeExtensionClaimed == "false");

        var activeLocator = new
        {
            schema = "xiigen.quality_semantic_gates_17.test_only_active_locator.v1",
            phase_id = PhaseId,
            input_phase_id = InputPhaseId,
            source_locator_candidate_ref = Rel(qsg16LocatorPath),
            locator_candidate_id = locatorCandidateId,
            target_candidate_id = targetCandidateId,
            rollback_candidate_id = rollbackCandidateId,
            activation_scope = "test_only_candidate_scoped_xiigen_extension_api_service_user_adaptive_asset_root",
            activated_asset_root = Rel(targetAssetRoot),
            irreversible_production_promotion_done = false,
            production_locator_update_done = false,
            host_side_logic_changed = false,
            rollback_refs_verified_before_activation = rollbackHashMatches,
            target_hashes_verified_before_activation = targetHashMatches,
            target_manifests_exist = targetManifestsExist,
            target_production_loaders_accept = targetLoadCompatibility.AllLoadersAccepted,
            target_production_loader_loaded_count = targetLoadCompatibility.LoadedCount,
            target_production_loader_expected_count = targetLoadCompatibility.ExpectedLoaderCount,
            target_production_loader_diagnostics = targetLoadCompatibility.Diagnostics,
            target_asset_root_materialized_from_user_cache = targetAssetRootMaterialized,
            rollback_asset_root_materialized_from_repo_common = rollbackAssetRootMaterialized,
            safe_to_activate_test_only = safeToActivateTestOnly,
            proposed_locator_refs = locatorRefs
        };

        var productRuntimeProbe = new
        {
            schema = "xiigen.quality_semantic_gates_17.product_runtime_semantic_verification_probe.v1",
            phase_id = PhaseId,
            input_phase_id = InputPhaseId,
            source_qsg16_product_runtime_probe_ref = Rel(qsg16ProductRuntimeProbePath),
            target_candidate_id = targetCandidateId,
            locator_candidate_id = locatorCandidateId,
            artifact_role = "product_runtime_semantic_verification_probe",
            inspected_refs = new[]
            {
                Rel(qsg16LocatorPath),
                Rel(qsg16TargetManifestPath),
                Rel(qsg16TargetCheckpointPath),
                Rel(qsg16RollbackCheckpointPath),
                Rel(activeLocatorPath),
                Rel(hostRowsPath)
            },
            qsg16_probe_status = qsg16ProductRuntimeProbe.GetProperty("probe_status").GetString(),
            locator_applied_to_product_runtime = false,
            test_only_locator_binding_applied = safeToActivateTestOnly,
            target_active_in_test_only_product_api_runtime = activeProductRuntimeTargetVerified,
            target_bound_host_shaped_api_probes_expected = 3,
            target_bound_host_shaped_api_probes_executed = targetBoundHostProbeCount,
            target_bound_host_shaped_api_probe_passes = targetBoundHostProbePasses,
            product_runtime_semantic_verification_executed = activeProductRuntimeTargetVerified,
            product_runtime_semantic_quality_claimed = false,
            product_runtime_semantic_quality_verified = productRuntimeSemanticQualityVerified,
            no_overclaim_verified = noOverclaimVerified,
            exact_probe_passed_for_promotion = false,
            probe_status = activeProductRuntimeTargetVerified
                ? "TARGET_BOUND_TEST_ONLY_PRODUCT_API_RUNTIME_VERIFIED_CLAIMS_CLOSED"
                : "IN_PROGRESS_TARGET_BOUND_PRODUCT_API_RUNTIME_GAP",
            what_passed = activeProductRuntimeTargetVerified
                ? "The QSG-16 locator candidate was activated through a test-only candidate-scoped XiigenExtensionApiService user-adaptive asset root and exercised by Claude/Cursor/Codex-shaped product API requests."
                : "QSG-17 did not safely activate the QSG-16 locator candidate through product API runtime.",
            what_remains = productRuntimeSemanticQualityVerified
                ? "Promotion/readiness are still not performed in this bounded slice."
                : "Product semantic quality remains unverified because the active target-bound API responses correctly keep semantic_quality_claimed=false; live host invocations and full final readiness remain separate gates.",
            supporting_only_limitation = "This is target-bound core product API runtime evidence. It is not irreversible production promotion, not a production locator update, not external AI/live-host proof, and not final readiness."
        };

        var remainingBlockers = BuildRemainingBlockers(activeProductRuntimeTargetVerified, noOverclaimVerified);
        var finalReadinessRows = BuildFinalReadinessRows(
            activeProductRuntimeTargetVerified,
            noOverclaimVerified,
            remainingBlockers,
            [Rel(activeLocatorPath), Rel(productRuntimeProbePath), Rel(hostRowsPath)]);
        var finalReadinessPasses = finalReadinessRows.Count(row => row.CountsAsFinalReadinessPass);
        var finalReadinessInProgress = finalReadinessRows.Count(row => !row.CountsAsFinalReadinessPass);

        var beforeAfterCounts = new
        {
            qsg16 = new
            {
                pair_count = qsg16Report.GetProperty("pair_count").GetInt32(),
                semantic_graduation_accepted_pairs = qsg16Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
                limited_semantic_quality_evidence_accepted_pairs = qsg16Report.GetProperty("limited_semantic_quality_evidence_accepted_pairs").GetInt32(),
                required_evidence_candidates_created = qsg16Report.GetProperty("required_evidence_candidates_created").GetInt32(),
                candidate_artifacts_created = qsg16Report.GetProperty("before_after_counts").GetProperty("qsg16").GetProperty("candidate_artifacts_created").GetInt32(),
                probe_artifacts_created = qsg16Report.GetProperty("before_after_counts").GetProperty("qsg16").GetProperty("probe_artifacts_created").GetInt32(),
                product_runtime_semantic_verification_executed = 0,
                target_active_in_product_runtime = qsg16ProductRuntimeProbe.GetProperty("target_active_in_product_runtime").GetBoolean(),
                target_bound_host_shaped_api_probe_passes = 0,
                product_runtime_semantic_quality_verified = qsg16Report.GetProperty("product_runtime_semantic_quality_verified").GetBoolean(),
                no_overclaim_verified = false,
                target_promotion_done = qsg16Report.GetProperty("target_promotion_done").GetBoolean(),
                locator_update_done = qsg16Report.GetProperty("locator_update_done").GetBoolean(),
                rollback_closure_done = qsg16Report.GetProperty("rollback_closure_done").GetBoolean(),
                final_readiness_checks_run_after_semantic_graduation = qsg16Report.GetProperty("final_readiness_checks_run_after_semantic_graduation").GetInt32(),
                final_readiness_passes = qsg16Report.GetProperty("final_readiness_passes").GetInt32(),
                final_readiness_in_progress = qsg16Report.GetProperty("final_readiness_in_progress").GetInt32(),
                final_readiness_claimed = qsg16Report.GetProperty("final_readiness_claimed").GetBoolean(),
                remaining_blockers = qsg16Report.GetProperty("remaining_blockers").GetArrayLength()
            },
            qsg17 = new
            {
                pair_count = qsg16Report.GetProperty("pair_count").GetInt32(),
                semantic_graduation_accepted_pairs = qsg16Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
                limited_semantic_quality_evidence_accepted_pairs = qsg16Report.GetProperty("limited_semantic_quality_evidence_accepted_pairs").GetInt32(),
                required_evidence_candidates_created = qsg16Report.GetProperty("required_evidence_candidates_created").GetInt32(),
                candidate_artifacts_created = 4,
                probe_artifacts_created = 9,
                test_only_active_locator_artifacts_created = 1,
                product_runtime_semantic_verification_executed = activeProductRuntimeTargetVerified ? 1 : 0,
                target_active_in_test_only_product_api_runtime = activeProductRuntimeTargetVerified,
                target_bound_host_shaped_api_probe_passes = targetBoundHostProbePasses,
                product_runtime_semantic_quality_verified = productRuntimeSemanticQualityVerified,
                no_overclaim_verified = noOverclaimVerified,
                target_promotion_done = false,
                locator_update_done = false,
                rollback_closure_done = false,
                final_readiness_checks_run_after_semantic_graduation = finalReadinessRows.Length,
                final_readiness_passes = finalReadinessPasses,
                final_readiness_in_progress = finalReadinessInProgress,
                final_readiness_claimed = false,
                remaining_blockers = remainingBlockers.Length
            }
        };

        var report = new
        {
            schema = "xiigen.quality_semantic_gates_17.product_runtime_target_verification_report.v1",
            phase_id = PhaseId,
            input_phase_id = InputPhaseId,
            qsg16_report_ref = Rel(qsg16ReportPath),
            qsg16_validation_ref = Rel(qsg16ValidationPath),
            source_locator_candidate_ref = Rel(qsg16LocatorPath),
            test_only_active_locator_ref = Rel(activeLocatorPath),
            host_target_bound_runtime_rows_ref = Rel(hostRowsPath),
            product_runtime_semantic_verification_probe_ref = Rel(productRuntimeProbePath),
            final_readiness_checks_ref = Rel(finalReadinessRowsPath),
            pair_count = qsg16Report.GetProperty("pair_count").GetInt32(),
            semantic_graduation_accepted_pairs = qsg16Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
            limited_semantic_quality_evidence_accepted_pairs = qsg16Report.GetProperty("limited_semantic_quality_evidence_accepted_pairs").GetInt32(),
            qsg16_required_evidence_candidates_created = qsg16Report.GetProperty("required_evidence_candidates_created").GetInt32(),
            test_only_locator_binding_applied = safeToActivateTestOnly,
            target_active_in_test_only_product_api_runtime = activeProductRuntimeTargetVerified,
            target_bound_host_shaped_api_probe_passes = targetBoundHostProbePasses,
            product_runtime_semantic_verification_executed = activeProductRuntimeTargetVerified,
            product_runtime_semantic_quality_verified = productRuntimeSemanticQualityVerified,
            product_runtime_semantic_quality_claimed = false,
            no_overclaim_verified = noOverclaimVerified,
            common_readiness_claim_allowed = false,
            learnability_claimed = false,
            target_promotion_done = false,
            locator_update_done = false,
            rollback_closure_done = false,
            promotion_readiness_claimed = false,
            product_readiness_claim_allowed = false,
            final_readiness_checks_run_after_semantic_graduation = finalReadinessRows.Length,
            final_readiness_passes = finalReadinessPasses,
            final_readiness_in_progress = finalReadinessInProgress,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            host_side_logic_changed = false,
            external_ai_used = false,
            deterministic_helper_classification = "QSG-17 runs target-bound product API runtime verification through an existing candidate-scoped loader and records no-overclaim evidence; it does not hide deterministic artifact checks as semantic quality.",
            before_after_counts = beforeAfterCounts,
            advanced_in_this_slice = new[]
            {
                "Activated the QSG-16 locator candidate only as a test-only XiigenExtensionApiService user-adaptive asset root.",
                "Ran Claude/Cursor/Codex-shaped product API requests against the same target candidate.",
                "Recorded target-bound active-runtime evidence while keeping product semantic quality, promotion, rollback closure, and final readiness claims closed."
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
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-18: run fresh live Claude/Cursor/Codex host invocations against the QSG-16 target-bound locator path or repair the host adapters to pass that locator explicitly, then prove rollback closure before any promotion/readiness claim."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_17.validation.v1",
            phase_id = PhaseId,
            source_qsg16_locator_candidate_seen = true,
            rollback_refs_verified_before_activation = rollbackHashMatches,
            target_hashes_verified_before_activation = targetHashMatches,
            target_production_loaders_accept = targetLoadCompatibility.AllLoadersAccepted,
            target_production_loader_loaded_count = targetLoadCompatibility.LoadedCount,
            target_production_loader_expected_count = targetLoadCompatibility.ExpectedLoaderCount,
            target_production_loader_diagnostics = targetLoadCompatibility.Diagnostics,
            test_only_locator_binding_applied = safeToActivateTestOnly,
            production_locator_update_done = false,
            irreversible_production_promotion_done = false,
            host_side_logic_changed = false,
            target_bound_host_shaped_api_probes_expected = 3,
            target_bound_host_shaped_api_probes_executed = targetBoundHostProbeCount,
            target_bound_host_shaped_api_probe_passes = targetBoundHostProbePasses,
            product_runtime_semantic_verification_executed = activeProductRuntimeTargetVerified,
            product_runtime_semantic_quality_claimed = false,
            product_runtime_semantic_quality_verified = productRuntimeSemanticQualityVerified,
            no_overclaim_verified = noOverclaimVerified,
            final_readiness_checks_expected = 10,
            final_readiness_checks_actual = finalReadinessRows.Length,
            final_readiness_passes_actual = finalReadinessPasses,
            final_readiness_in_progress_actual = finalReadinessInProgress,
            common_readiness_claim_allowed = false,
            promotion_readiness_claimed = false,
            product_readiness_claim_allowed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            remaining_blockers = remainingBlockers,
            output_files = ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRefs(
                "QUALITY-SEMANTIC-GATES-17 output artifact",
                activeLocatorPath,
                hostRowsPath,
                productRuntimeProbePath,
                finalReadinessRowsPath,
                reportPath,
                reportMdPath,
                validationPath,
                statePatchPath)
        };

        WriteJson(activeLocatorPath, activeLocator);
        WriteJsonl(hostRowsPath, hostRows);
        WriteJson(productRuntimeProbePath, productRuntimeProbe);
        WriteJsonl(finalReadinessRowsPath, finalReadinessRows);
        WriteJson(reportPath, report);
        WriteMarkdown(reportMdPath, beforeAfterCounts, remainingBlockers, activeProductRuntimeTargetVerified, noOverclaimVerified);
        WriteJson(validationPath, validation);
        statePatchPath = UpdateState(report, validation);

        Assert.True(targetHashMatches);
        Assert.True(rollbackHashMatches);
        Assert.Equal(ExpectedLocatorRefs().Select(expected => expected.MachineId), locatorRefs.Select(locator => locator.MachineId));
        Assert.True(
            targetLoadCompatibility.AllLoadersAccepted,
            string.Join(Environment.NewLine, targetLoadCompatibility.Diagnostics.Select(diagnostic => diagnostic.HumanDiagnostic)));
        Assert.Equal(3, targetLoadCompatibility.LoadedCount);
        Assert.True(safeToActivateTestOnly);
        Assert.Equal(3, hostRows.Length);
        Assert.Equal(3, targetBoundHostProbePasses);
        Assert.True(activeProductRuntimeTargetVerified);
        Assert.True(noOverclaimVerified);
        Assert.False(productRuntimeSemanticQualityVerified);
        Assert.Equal(10, finalReadinessRows.Length);
        Assert.Equal(0, finalReadinessPasses);

        using var writtenProbe = JsonDocument.Parse(File.ReadAllText(productRuntimeProbePath, Encoding.UTF8));
        var probeRoot = writtenProbe.RootElement;
        Assert.True(probeRoot.GetProperty("test_only_locator_binding_applied").GetBoolean());
        Assert.True(probeRoot.GetProperty("target_active_in_test_only_product_api_runtime").GetBoolean());
        Assert.Equal(3, probeRoot.GetProperty("target_bound_host_shaped_api_probe_passes").GetInt32());
        Assert.False(probeRoot.GetProperty("product_runtime_semantic_quality_claimed").GetBoolean());
        Assert.False(probeRoot.GetProperty("product_runtime_semantic_quality_verified").GetBoolean());
        Assert.True(probeRoot.GetProperty("no_overclaim_verified").GetBoolean());
        Assert.False(probeRoot.GetProperty("locator_applied_to_product_runtime").GetBoolean());
        Assert.False(probeRoot.GetProperty("exact_probe_passed_for_promotion").GetBoolean());

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath, Encoding.UTF8));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal(PhaseId, stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.False(stateRoot.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(stateRoot.GetProperty("locator_update_done").GetBoolean());
        Assert.False(stateRoot.GetProperty("rollback_closure_done").GetBoolean());
        Assert.False(stateRoot.GetProperty("product_runtime_semantic_quality_claimed").GetBoolean());
        Assert.False(stateRoot.GetProperty("product_runtime_semantic_quality_verified").GetBoolean());
        Assert.False(stateRoot.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(stateRoot.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.StartsWith("QUALITY-SEMANTIC-GATES-18:", stateRoot.GetProperty("next_action").GetString());
    }

    [Fact]
    public void QualitySemanticGates17LocatorEvidenceValidation_FailsClosedForMissingHashAndUnsafeRefs()
    {
        var validRefs = WellFormedLocatorRefsForValidationTests();

        AssertInvalidLocatorShape(
            "missing target hash",
            MutateLocatorRef(validRefs, "PromptToTaskMachine", locator => locator with { TargetSha256 = string.Empty }),
            "missing target_sha256");

        AssertInvalidLocatorShape(
            "rooted Windows path",
            MutateLocatorRef(validRefs, "PromptToTaskMachine", locator => locator with
            {
                ProposedTargetRef = @"C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\adaptive-learnability-01\user-cache-continued\prompt-to-task\prompt-to-task-v1\prompt-to-task-v1-checkpoint.json"
            }),
            "rooted paths are rejected");

        AssertInvalidLocatorShape(
            "../ escape",
            MutateLocatorRef(validRefs, "OrchestrationReasoningMachine", locator => locator with
            {
                ProposedManifestRef = "artifacts/extension-runtime-token-quality-learnability/adaptive-learnability-01/user-cache-continued/orchestration-reasoning/../orchestration-reasoning-v1/manifest.json"
            }),
            "must not contain '..' traversal");

        AssertInvalidLocatorShape(
            "wrong machine file",
            MutateLocatorRef(validRefs, "SkillSelectionMachine", locator => locator with
            {
                ProposedTargetRef = ExpectedLocatorRefs().Single(expected => expected.MachineId == "PromptToTaskMachine").ProposedTargetRef
            }),
            "must equal exact QSG-16 BuildMachineAssets ref");
    }

    [Fact]
    public void QualitySemanticGates17HostProbeRow_FailedProductApiResultRecordsDiagnosticWithoutNullDereference()
    {
        var host = new HostCase("codex", "codex-desktop", "synthetic failed product API probe");
        var request = BuildHostProbeRequest(host, "qsg17-target-candidate", "qsg17-locator-candidate");
        var failedResult = OperationResult<XiigenExtensionResponse>.Fail(
            "Synthetic product API result failure for QSG-17 host row proof.",
            "SYNTHETIC_PRODUCT_API_FAILURE");

        var row = BuildHostRuntimeRow(
            ExpectedTargetAssetRootFullPath(),
            "qsg17-target-candidate",
            "qsg17-locator-candidate",
            host,
            request,
            failedResult);

        Assert.True(row.TargetBoundProductApiProbeExecuted);
        Assert.False(row.TargetBoundProductApiProbePassed);
        Assert.False(row.ResponseSuccess);
        Assert.Equal(XiigenExtensionApiService.ErrorStatus, row.ResponseStatus);
        Assert.Equal("SYNTHETIC_PRODUCT_API_FAILURE", row.PromptToTaskUserLoadError);
        Assert.Contains("Synthetic product API result failure", row.WhatRemains, StringComparison.Ordinal);
    }

    private static HostRuntimeRow[] RunHostShapedProductApiProbe(
        string targetAssetRoot,
        string targetCandidateId,
        string locatorCandidateId)
    {
        var service = new XiigenExtensionApiService(RepoRoot, targetAssetRoot);
        return HostCases()
            .Select(host =>
            {
                var request = BuildHostProbeRequest(host, targetCandidateId, locatorCandidateId);
                var result = service.ProcessHostExtensionRequest(request);
                return BuildHostRuntimeRow(targetAssetRoot, targetCandidateId, locatorCandidateId, host, request, result);
            })
            .ToArray();
    }

    private static XiigenExtensionRequest BuildHostProbeRequest(
        HostCase host,
        string targetCandidateId,
        string locatorCandidateId)
        => new(
            HostName: host.HostName,
            HostType: host.HostType,
            Method: XiigenExtensionApiContract.PromptToTaskMethod,
            UserPromptOrTaskText: host.Prompt,
            HostFacts: new Dictionary<string, string>
            {
                ["workspace"] = RepoRoot,
                ["host_client"] = host.HostName,
                ["qsg_phase"] = PhaseId,
                ["locator_candidate_id"] = locatorCandidateId,
                ["target_candidate_id"] = targetCandidateId,
                ["product_runtime_probe_scope"] = "test_only_candidate_scoped"
            },
            DebugEnabled: true,
            RequestedSkills: ["plan-review", "orchestration", "code-review"],
            AvailableSkills: ["plan-review", "orchestration", "code-review", "skill-selection", "prompt-to-task"],
            CheckSetInputs: new Dictionary<string, string>
            {
                ["scope"] = "qsg17-target-bound-product-api",
                ["external_ai"] = "forbidden_without_approval",
                ["locator_candidate_id"] = locatorCandidateId,
                ["target_candidate_id"] = targetCandidateId
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "QUALITY-SEMANTIC-GATES-17",
                ["qsg16_locator_candidate"] = locatorCandidateId
            },
            CorrelationId: $"qsg17-{host.HostName}");

    private static HostRuntimeRow BuildHostRuntimeRow(
        string targetAssetRoot,
        string targetCandidateId,
        string locatorCandidateId,
        HostCase host,
        XiigenExtensionRequest request,
        OperationResult<XiigenExtensionResponse> result)
    {
        if (!result.IsSuccess || result.Value is null)
        {
            var errorCode = string.IsNullOrWhiteSpace(result.ErrorCode)
                ? "PRODUCT_API_RESULT_VALUE_MISSING"
                : result.ErrorCode;
            var errorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage)
                ? "XiigenExtensionApiService returned no response value for the QSG-17 host-shaped product API probe."
                : result.ErrorMessage;

            // Fail closed with an inspectable row. A failed product API result is evidence of a gap,
            // not permission to dereference a null response or claim target-bound runtime success.
            return new HostRuntimeRow(
                Schema: "xiigen.quality_semantic_gates_17.host_target_bound_runtime_row.v1",
                PhaseId: PhaseId,
                HostName: host.HostName,
                HostType: host.HostType,
                CorrelationId: request.CorrelationId,
                TargetCandidateId: targetCandidateId,
                LocatorCandidateId: locatorCandidateId,
                ActivationScope: "test_only_candidate_scoped_xiigen_extension_api_service_user_adaptive_asset_root",
                TargetAssetRoot: Rel(targetAssetRoot),
                TargetBoundProductApiProbeExecuted: true,
                TargetBoundProductApiProbePassed: false,
                ResponseSuccess: false,
                ResponseStatus: XiigenExtensionApiService.ErrorStatus,
                PromptToTaskUserAssetLoadedFirst: false,
                OrchestrationReasoningUserAssetLoadedFirst: false,
                SkillSelectionUserAssetLoadedFirst: false,
                UserAdaptiveAnyAssetLoaded: "false",
                PromptToTaskUserCheckpointRef: string.Empty,
                OrchestrationReasoningUserCheckpointRef: string.Empty,
                SkillSelectionUserCheckpointRef: string.Empty,
                PromptToTaskUserLoadError: errorCode,
                OrchestrationReasoningUserLoadError: errorCode,
                SkillSelectionUserLoadError: errorCode,
                SemanticQualityClaimed: "false",
                RuntimeExtensionClaimed: "false",
                SelectedSkillCount: 0,
                WarningCount: 0,
                ErrorCount: 1,
                HostSideLogicChanged: false,
                ExternalAiUsed: false,
                ProductReadinessClaimed: false,
                ExactReadinessPass: false,
                WhatPassed: "No target-bound product API runtime evidence passed for this host-shaped probe.",
                WhatRemains: $"QSG-17 host-shaped product API probe failed closed before response inspection. ErrorCode={errorCode}; ErrorMessage={errorMessage}");
        }

        var response = result.Value;
        var checkSet = response.CheckSetResults;
        var promptLoaded = ReadCheckSet(checkSet, "prompt_to_task_user_asset_loaded_first") == "true";
        var orchestrationLoaded = ReadCheckSet(checkSet, "orchestration_reasoning_user_asset_loaded_first") == "true";
        var skillLoaded = ReadCheckSet(checkSet, "skill_selection_user_asset_loaded_first") == "true";
        var promptLoadError = ReadCheckSet(checkSet, "prompt_to_task_user_load_error");
        var orchestrationLoadError = ReadCheckSet(checkSet, "orchestration_reasoning_user_load_error");
        var skillLoadError = ReadCheckSet(checkSet, "skill_selection_user_load_error");
        var targetBoundProbePassed = response.Success
            && response.Status == XiigenExtensionApiService.PartialBoundaryStatus
            && promptLoaded
            && orchestrationLoaded
            && skillLoaded
            && ReadCheckSet(checkSet, "user_adaptive_any_asset_loaded") == "true"
            && ReadCheckSet(checkSet, "semantic_quality_claimed") == "false"
            && ReadCheckSet(checkSet, "runtime_extension_claimed") == "false";

        return new HostRuntimeRow(
            Schema: "xiigen.quality_semantic_gates_17.host_target_bound_runtime_row.v1",
            PhaseId: PhaseId,
            HostName: host.HostName,
            HostType: host.HostType,
            CorrelationId: request.CorrelationId,
            TargetCandidateId: targetCandidateId,
            LocatorCandidateId: locatorCandidateId,
            ActivationScope: "test_only_candidate_scoped_xiigen_extension_api_service_user_adaptive_asset_root",
            TargetAssetRoot: Rel(targetAssetRoot),
            TargetBoundProductApiProbeExecuted: true,
            TargetBoundProductApiProbePassed: targetBoundProbePassed,
            ResponseSuccess: response.Success,
            ResponseStatus: response.Status,
            PromptToTaskUserAssetLoadedFirst: promptLoaded,
            OrchestrationReasoningUserAssetLoadedFirst: orchestrationLoaded,
            SkillSelectionUserAssetLoadedFirst: skillLoaded,
            UserAdaptiveAnyAssetLoaded: ReadCheckSet(checkSet, "user_adaptive_any_asset_loaded"),
            PromptToTaskUserCheckpointRef: ReadCheckSet(checkSet, "prompt_to_task_user_checkpoint_ref"),
            OrchestrationReasoningUserCheckpointRef: ReadCheckSet(checkSet, "orchestration_reasoning_user_checkpoint_ref"),
            SkillSelectionUserCheckpointRef: ReadCheckSet(checkSet, "skill_selection_user_checkpoint_ref"),
            PromptToTaskUserLoadError: promptLoadError,
            OrchestrationReasoningUserLoadError: orchestrationLoadError,
            SkillSelectionUserLoadError: skillLoadError,
            SemanticQualityClaimed: ReadCheckSet(checkSet, "semantic_quality_claimed"),
            RuntimeExtensionClaimed: ReadCheckSet(checkSet, "runtime_extension_claimed"),
            SelectedSkillCount: response.SelectedSkills.Count,
            WarningCount: response.Warnings.Count,
            ErrorCount: response.Errors.Count,
            HostSideLogicChanged: false,
            ExternalAiUsed: false,
            ProductReadinessClaimed: false,
            ExactReadinessPass: false,
            WhatPassed: targetBoundProbePassed
                ? "Candidate-scoped product API request loaded the QSG-16 target assets and returned a no-overclaim response."
                : $"Candidate-scoped product API request did not satisfy every target-bound load/no-overclaim check. Loader errors: PromptToTask={promptLoadError}; OrchestrationReasoning={orchestrationLoadError}; SkillSelection={skillLoadError}.",
            WhatRemains: "This is not a fresh live host invocation and does not prove product semantic quality or final readiness.");
    }

    private static FinalReadinessRow[] BuildFinalReadinessRows(
        bool activeProductRuntimeTargetVerified,
        bool noOverclaimVerified,
        IReadOnlyList<string> remainingBlockers,
        IReadOnlyList<string> qsg17Refs)
        => FinalReadinessChecks()
            .Select(check => new FinalReadinessRow(
                Schema: "xiigen.quality_semantic_gates_17.final_readiness_check.v1",
                PhaseId: PhaseId,
                FinalReadinessPass: check.Pass,
                SubagentId: $"QUALITY-SEMANTIC-GATES-17-FINAL-READINESS-{check.Pass:00}",
                TargetOutcomeRow: check.TargetOutcomeRow,
                RequiredCheck: check.RequiredCheck,
                ArtifactsInspected: check.ArtifactsInspected.Concat(qsg17Refs).Distinct(StringComparer.Ordinal).ToArray(),
                Verdict: check.Pass is 4 or 9 or 10 && activeProductRuntimeTargetVerified && noOverclaimVerified
                    ? "TARGET_BOUND_API_EVIDENCE_RECORDED_CLAIM_CLOSED"
                    : "IN_PROGRESS",
                ExactlyWhatPassed: check.Pass switch
                {
                    4 when activeProductRuntimeTargetVerified => "The same QSG-16 target locator candidate was exercised through Claude/Cursor/Codex-shaped product API requests.",
                    9 when activeProductRuntimeTargetVerified => "Core boundary stayed in SmartSkillsDomain; QSG-17 used existing candidate-scoped asset loading and did not change host logic.",
                    10 when noOverclaimVerified => "QSG-17 records active target-bound runtime evidence while semantic quality, production locator, promotion, rollback closure, and readiness claims stay closed.",
                    _ => check.WhatPassed
                },
                ExactlyWhatRemains: check.Pass switch
                {
                    4 when activeProductRuntimeTargetVerified => "Fresh live Claude/Cursor/Codex host invocations against this locator remain missing.",
                    9 when activeProductRuntimeTargetVerified => "Production locator update remains intentionally unapplied.",
                    10 when noOverclaimVerified => "Final readiness still needs live-host target-bound proofs and rollback closure before any ready claim.",
                    _ => check.WhatRemains
                },
                Findings: check.Findings,
                RequiredFixRefs: ["QUALITY-SEMANTIC-GATES-18 live-host target-bound verification and rollback closure"],
                RecheckEvidence: "QSG-17 closes the active product API target-binding gap only. Final readiness rows do not count as passed until live host proofs, rollback closure, and full readiness gates all pass together.",
                CountsAsFinalReadinessPass: false,
                DashboardOrHelperSubstituteUsed: false,
                RemainingBlockers: remainingBlockers.ToArray()))
            .ToArray();

    private static string[] BuildRemainingBlockers(bool activeProductRuntimeTargetVerified, bool noOverclaimVerified)
    {
        var blockers = new List<string>();
        if (!activeProductRuntimeTargetVerified)
            blockers.Add("qsg17_target_bound_product_api_runtime_verification_gap");
        if (!noOverclaimVerified)
            blockers.Add("qsg17_no_overclaim_target_bound_runtime_gap");

        blockers.AddRange(
        [
            "fresh_live_claude_cursor_codex_runtime_not_rerun_against_qsg16_locator",
            "product_runtime_semantic_quality_claim_remains_false_without_positive_semantic_evidence_from_all_components",
            "rollback_closure_not_applied_to_active_locator",
            "production_locator_update_not_done",
            "ten_final_readiness_checks_remain_claim_closed_zero_counted_passes"
        ]);
        return blockers.ToArray();
    }

    private static FinalReadinessCheck[] FinalReadinessChecks() =>
    [
        new(1, "Claude live host runtime readiness", "Claude path must prove the active QSG-16 target locator through a fresh live host invocation.", ["artifacts/extension-runtime-token-quality-learnability/claude-runtime-proof.json"], "Claude historical runtime packet exists.", "Fresh live Claude invocation against the active QSG-16 locator remains missing.", ["QSG-17 used a Claude-shaped product API request, not a fresh live Claude host invocation."]),
        new(2, "Cursor live host runtime readiness", "Cursor path must prove the active QSG-16 target locator through a fresh live host invocation.", ["artifacts/extension-runtime-token-quality-learnability/cursor-runtime-proof-rerun.json"], "Cursor historical runtime packet exists.", "Fresh live Cursor invocation against the active QSG-16 locator remains missing.", ["QSG-17 used a Cursor-shaped product API request, not a fresh live Cursor host invocation."]),
        new(3, "Codex live host runtime readiness", "Codex path must prove direct plugin/app/core target-bound readiness.", ["artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-04f.json"], "Codex 04F supporting packet exists.", "Fresh target-bound Codex plugin/app invocation remains missing.", ["QSG-17 used a Codex-shaped product API request, not a fresh Codex app/plugin proof."]),
        new(4, "Cross-host end-to-end behavior", "The same active target locator must be verified across Claude, Cursor, and Codex.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-17-host-target-bound-runtime-rows.jsonl"], "QSG-16 target binding was previously inactive.", "Cross-host target-bound behavior remains live-host incomplete.", ["QSG-17 can close only the product API target-binding gap."]),
        new(5, "Token before/after", "Final token readiness must be tied to the active target and no quality regression.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-report.json"], "Bounded QSG-14 token evidence remains available.", "No new active target-bound token run was performed.", ["Token evidence is not promoted to final readiness."]),
        new(6, "Quality improvement", "Product runtime semantic quality needs positive semantic evidence from all components.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-report.json"], "Bounded QSG semantic graduation evidence remains available.", "Active API responses keep semantic_quality_claimed=false, so product semantic quality is not verified.", ["No semantic quality claim is opened."]),
        new(7, "Learnability and DPO", "Full machine target probes must close PromptToTask, OrchestrationReasoning, and SkillSelection together.", ["artifacts/extension-runtime-token-quality-learnability/continue-training-report.json", "artifacts/extension-runtime-token-quality-learnability/held-out-ablation-report.json"], "Candidate target assets load through the product API overlay.", "Full-machine promotion/readiness closure remains incomplete.", ["QSG-17 does not run new training."]),
        new(8, "Backup/export/import/share and rollback", "Rollback must be executable for the active locator before promotion.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-16-target-rollback-probe.json"], "Rollback refs were hash-verified before test-only activation.", "Rollback closure was not applied to a production locator.", ["QSG-17 does not apply rollback."]),
        new(9, "Boundary and architecture", "Core boundary must remain preserved during target binding.", ["docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"], "QSG-17 changes no host-side logic.", "Production locator update remains unapplied.", ["Candidate binding is test-only."]),
        new(10, "Documentation and no-overclaim", "Reports must name exact scope and keep final readiness closed.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-17-report.md"], "QSG-17 no-overclaim evidence can be recorded.", "Final docs cannot say ready while live host and rollback gates remain open.", ["QSG-18 is required."])
    ];

    private static HostCase[] HostCases() =>
    [
        new("claude", "claude-code", "Summarize this bounded work order into a task, plan, and skills without claiming semantic readiness."),
        new("cursor", "cursor-extension-host", "Plan a safe target-bound verification slice for the current project."),
        new("codex", "codex-desktop", "Review the locator-bound runtime evidence and keep product readiness closed.")
    ];

    private static IReadOnlyList<LocatorRef> ReadLocatorRefs(JsonElement locator)
        => locator.GetProperty("proposed_locator_refs")
            .EnumerateArray()
            .Select(item => new LocatorRef(
                MachineId: ReadOptionalString(item, "machine_id"),
                ProposedTargetRef: ReadOptionalString(item, "proposed_target_ref"),
                ProposedManifestRef: ReadOptionalString(item, "proposed_manifest_ref"),
                TargetSha256: ReadOptionalString(item, "target_sha256"),
                RollbackRef: ReadOptionalString(item, "rollback_ref"),
                RollbackSha256: ReadOptionalString(item, "rollback_sha256")))
            .ToArray();

    private static string ReadOptionalString(JsonElement item, string propertyName)
        => item.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static LocatorEvidenceValidation ValidateLocatorEvidenceForTestOnlyActivation(IReadOnlyList<LocatorRef> locatorRefs)
    {
        var shapeValidation = ValidateLocatorRefShape(locatorRefs);
        var diagnostics = shapeValidation.Diagnostics.ToList();
        var targetHashMatches = shapeValidation.IsValid;
        var rollbackHashMatches = shapeValidation.IsValid;
        var targetManifestsExist = shapeValidation.IsValid;

        if (shapeValidation.IsValid)
        {
            foreach (var locator in shapeValidation.LocatorRefs)
            {
                if (!File.Exists(RepoPath(locator.ProposedManifestRef)))
                {
                    targetManifestsExist = false;
                    diagnostics.Add($"{locator.MachineId}: proposed_manifest_ref file is missing before QSG-17 activation: {locator.ProposedManifestRef}.");
                }

                if (!File.Exists(RepoPath(locator.ProposedTargetRef)))
                {
                    targetHashMatches = false;
                    diagnostics.Add($"{locator.MachineId}: proposed_target_ref file is missing before QSG-17 activation: {locator.ProposedTargetRef}.");
                }
                else
                {
                    var actualTargetSha256 = Sha256File(RepoPath(locator.ProposedTargetRef));
                    if (!string.Equals(actualTargetSha256, locator.TargetSha256, StringComparison.Ordinal))
                    {
                        targetHashMatches = false;
                        diagnostics.Add($"{locator.MachineId}: persisted target_sha256 does not match the proposed_target_ref file before QSG-17 activation. Persisted={locator.TargetSha256}; actual={actualTargetSha256}; ref={locator.ProposedTargetRef}.");
                    }
                }

                if (!File.Exists(RepoPath(locator.RollbackRef)))
                {
                    rollbackHashMatches = false;
                    diagnostics.Add($"{locator.MachineId}: rollback_ref file is missing before QSG-17 activation: {locator.RollbackRef}.");
                }
                else
                {
                    var actualRollbackSha256 = Sha256File(RepoPath(locator.RollbackRef));
                    if (!string.Equals(actualRollbackSha256, locator.RollbackSha256, StringComparison.Ordinal))
                    {
                        rollbackHashMatches = false;
                        diagnostics.Add($"{locator.MachineId}: persisted rollback_sha256 does not match the rollback_ref file before QSG-17 activation. Persisted={locator.RollbackSha256}; actual={actualRollbackSha256}; ref={locator.RollbackRef}.");
                    }
                }
            }
        }

        return new LocatorEvidenceValidation(
            IsValid: diagnostics.Count == 0,
            Diagnostics: diagnostics.ToArray(),
            LocatorRefs: shapeValidation.LocatorRefs,
            TargetAssetRoot: shapeValidation.TargetAssetRoot,
            RollbackAssetRoot: shapeValidation.RollbackAssetRoot,
            TargetHashMatches: targetHashMatches,
            RollbackHashMatches: rollbackHashMatches,
            TargetManifestsExist: targetManifestsExist);
    }

    private static LocatorRefShapeValidation ValidateLocatorRefShape(IReadOnlyList<LocatorRef> locatorRefs)
    {
        var expectedRefs = ExpectedLocatorRefs();
        var diagnostics = new List<string>();

        if (locatorRefs.Count != expectedRefs.Length)
        {
            diagnostics.Add($"QSG-17 locator evidence must contain exactly {expectedRefs.Length} refs for PromptToTask, OrchestrationReasoning, and SkillSelection; got {locatorRefs.Count}.");
        }

        foreach (var group in locatorRefs.GroupBy(locator => locator.MachineId, StringComparer.Ordinal))
        {
            if (group.Count() > 1)
            {
                var machineId = string.IsNullOrWhiteSpace(group.Key) ? "<missing>" : group.Key;
                diagnostics.Add($"QSG-17 locator evidence contains duplicate machine_id '{machineId}'. Persisted locator refs must be unique.");
            }
        }

        var expectedMachineIds = expectedRefs.Select(expected => expected.MachineId).ToArray();
        var actualMachineIds = locatorRefs.Select(locator => locator.MachineId).Where(machineId => !string.IsNullOrWhiteSpace(machineId)).ToArray();
        foreach (var missingMachineId in expectedMachineIds.Except(actualMachineIds, StringComparer.Ordinal))
        {
            diagnostics.Add($"QSG-17 locator evidence is missing machine_id '{missingMachineId}'.");
        }

        foreach (var unknownMachineId in actualMachineIds.Except(expectedMachineIds, StringComparer.Ordinal))
        {
            diagnostics.Add($"QSG-17 locator evidence contains unknown machine_id '{unknownMachineId}'.");
        }

        var singleRefsByMachine = locatorRefs
            .GroupBy(locator => locator.MachineId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

        foreach (var expected in expectedRefs)
        {
            if (!singleRefsByMachine.TryGetValue(expected.MachineId, out var locator))
                continue;

            ValidateRequiredSha256(locator.MachineId, "target_sha256", locator.TargetSha256, diagnostics);
            ValidateRequiredSha256(locator.MachineId, "rollback_sha256", locator.RollbackSha256, diagnostics);
            ValidateExpectedRelativeRef(locator.MachineId, "proposed_target_ref", locator.ProposedTargetRef, expected.ProposedTargetRef, ExpectedTargetAssetRootRelative(), diagnostics);
            ValidateExpectedRelativeRef(locator.MachineId, "proposed_manifest_ref", locator.ProposedManifestRef, expected.ProposedManifestRef, ExpectedTargetAssetRootRelative(), diagnostics);
            ValidateExpectedRelativeRef(locator.MachineId, "rollback_ref", locator.RollbackRef, expected.RollbackRef, ExpectedRollbackAssetRootRelative(), diagnostics);
        }

        return new LocatorRefShapeValidation(
            IsValid: diagnostics.Count == 0,
            Diagnostics: diagnostics.ToArray(),
            LocatorRefs: expectedRefs
                .Select(expected => singleRefsByMachine.TryGetValue(expected.MachineId, out var locator) ? locator : null)
                .Where(locator => locator is not null)
                .Select(locator => locator!)
                .ToArray(),
            TargetAssetRoot: RepoPath(ExpectedTargetAssetRootRelative()),
            RollbackAssetRoot: RepoPath(ExpectedRollbackAssetRootRelative()));
    }

    private static void ValidateRequiredSha256(
        string machineId,
        string propertyName,
        string value,
        List<string> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add($"{machineId}: missing {propertyName}; QSG-17 refuses to synthesize locator hashes from current files before test-only activation.");
            return;
        }

        if (!IsSha256Hex(value))
        {
            diagnostics.Add($"{machineId}: {propertyName} must be a 64-character lowercase-or-uppercase hex SHA-256 value; got '{value}'.");
        }
    }

    private static void ValidateExpectedRelativeRef(
        string machineId,
        string propertyName,
        string value,
        string expectedRelative,
        string expectedRootRelative,
        List<string> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add($"{machineId}: missing {propertyName}; QSG-17 refuses to infer locator refs before test-only activation.");
            return;
        }

        var trimmed = value.Trim();
        if (!string.Equals(value, trimmed, StringComparison.Ordinal))
        {
            diagnostics.Add($"{machineId}: {propertyName} must not contain leading or trailing whitespace.");
        }

        if (LooksRootedPath(trimmed))
        {
            diagnostics.Add($"{machineId}: {propertyName} must be a repo-relative ref; rooted paths are rejected: {value}.");
        }

        var normalized = trimmed.Replace('\\', '/');
        if (normalized.Split('/').Any(segment => string.Equals(segment, "..", StringComparison.Ordinal)))
        {
            diagnostics.Add($"{machineId}: {propertyName} must not contain '..' traversal: {value}.");
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(Path.Combine(RepoRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            diagnostics.Add($"{machineId}: {propertyName} could not be resolved safely: {ex.Message}");
            return;
        }

        var expectedRoot = RepoPath(expectedRootRelative);
        if (!IsSameOrUnderDirectory(fullPath, expectedRoot))
        {
            diagnostics.Add($"{machineId}: {propertyName} resolves outside expected artifact root '{expectedRootRelative}': {value}.");
        }

        if (!string.Equals(trimmed, expectedRelative, StringComparison.Ordinal))
        {
            diagnostics.Add($"{machineId}: {propertyName} must equal exact QSG-16 BuildMachineAssets ref '{expectedRelative}', got '{value}'.");
        }
    }

    private static bool LooksRootedPath(string value)
        => Path.IsPathRooted(value)
            || value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("\\", StringComparison.Ordinal)
            || value.StartsWith("//", StringComparison.Ordinal)
            || value.StartsWith(@"\\", StringComparison.Ordinal)
            || (value.Length >= 2 && char.IsLetter(value[0]) && value[1] == ':');

    private static bool IsSameOrUnderDirectory(string candidatePath, string rootPath)
    {
        var comparison = Path.DirectorySeparatorChar == '\\'
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var normalizedRoot = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidatePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(normalizedCandidate, normalizedRoot, comparison)
            || normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, comparison);
    }

    private static bool IsSha256Hex(string value)
        => value.Length == 64 && value.All(character =>
            character is >= '0' and <= '9'
                or >= 'a' and <= 'f'
                or >= 'A' and <= 'F');

    private static string FormatLocatorEvidenceDiagnostics(IEnumerable<string> diagnostics)
        => "QSG-17 locator evidence rejected before production loader/host probes:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, diagnostics);

    private static ExpectedLocatorRef[] ExpectedLocatorRefs()
    {
        var targetRoot = Path.Combine(ArtifactDir, "adaptive-learnability-01", "user-cache-continued");
        var rollbackRoot = Path.Combine(ArtifactDir, "adaptive-learnability-01", "rollback");
        return
        [
            new(
                "PromptToTaskMachine",
                Rel(Path.Combine(targetRoot, "prompt-to-task", "prompt-to-task-v1", "prompt-to-task-v1-checkpoint.json")),
                Rel(Path.Combine(targetRoot, "prompt-to-task", "prompt-to-task-v1", "manifest.json")),
                Rel(Path.Combine(rollbackRoot, "prompt-to-task", "prompt-to-task-v1", "prompt-to-task-v1-checkpoint.json"))),
            new(
                "OrchestrationReasoningMachine",
                Rel(Path.Combine(targetRoot, "orchestration-reasoning", "orchestration-reasoning-v1", "checkpoint.json")),
                Rel(Path.Combine(targetRoot, "orchestration-reasoning", "orchestration-reasoning-v1", "manifest.json")),
                Rel(Path.Combine(rollbackRoot, "orchestration-reasoning", "orchestration-reasoning-v1", "checkpoint.json"))),
            new(
                "SkillSelectionMachine",
                Rel(Path.Combine(targetRoot, "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json")),
                Rel(Path.Combine(targetRoot, "skill-selection", "skill-selection-v1", "manifest.json")),
                Rel(Path.Combine(rollbackRoot, "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json")))
        ];
    }

    private static string ExpectedTargetAssetRootRelative()
        => Rel(Path.Combine(ArtifactDir, "adaptive-learnability-01", "user-cache-continued"));

    private static string ExpectedTargetAssetRootFullPath()
        => RepoPath(ExpectedTargetAssetRootRelative());

    private static string ExpectedRollbackAssetRootRelative()
        => Rel(Path.Combine(ArtifactDir, "adaptive-learnability-01", "rollback"));

    private static LocatorRef[] WellFormedLocatorRefsForValidationTests()
        => ExpectedLocatorRefs()
            .Select((expected, index) => new LocatorRef(
                MachineId: expected.MachineId,
                ProposedTargetRef: expected.ProposedTargetRef,
                ProposedManifestRef: expected.ProposedManifestRef,
                TargetSha256: RepeatedHex((char)('a' + index)),
                RollbackRef: expected.RollbackRef,
                RollbackSha256: RepeatedHex((char)('d' + index))))
            .ToArray();

    private static LocatorRef[] MutateLocatorRef(
        IReadOnlyList<LocatorRef> locatorRefs,
        string machineId,
        Func<LocatorRef, LocatorRef> mutate)
        => locatorRefs
            .Select(locator => string.Equals(locator.MachineId, machineId, StringComparison.Ordinal) ? mutate(locator) : locator)
            .ToArray();

    private static void AssertInvalidLocatorShape(
        string scenario,
        IReadOnlyList<LocatorRef> locatorRefs,
        string expectedDiagnosticFragment)
    {
        var validation = ValidateLocatorRefShape(locatorRefs);
        Assert.False(validation.IsValid);
        Assert.Contains(
            validation.Diagnostics,
            diagnostic => diagnostic.Contains(expectedDiagnosticFragment, StringComparison.Ordinal));
    }

    private static string RepeatedHex(char character)
    {
        Assert.True(character is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');
        return new string(character, 64);
    }

    private static bool EnsureTargetAssetRootFromUserCache(string targetAssetRoot)
    {
        var normalizedTargetRoot = Path.GetFullPath(targetAssetRoot);
        if (Directory.Exists(normalizedTargetRoot))
            return false;

        if (!string.Equals(Path.GetFileName(normalizedTargetRoot), "user-cache-continued", StringComparison.OrdinalIgnoreCase))
            return false;

        var parent = Directory.GetParent(normalizedTargetRoot)?.FullName;
        if (parent is null)
            return false;

        var sourceRoot = Path.Combine(parent, "user-cache");
        if (!Directory.Exists(sourceRoot))
            return false;

        CopyDirectory(sourceRoot, normalizedTargetRoot);
        return true;
    }

    private static bool EnsureRollbackAssetRootFromRepoCommon(string rollbackAssetRoot)
    {
        var normalizedRollbackRoot = Path.GetFullPath(rollbackAssetRoot);
        if (Directory.Exists(normalizedRollbackRoot))
            return false;

        if (!string.Equals(Path.GetFileName(normalizedRollbackRoot), "rollback", StringComparison.OrdinalIgnoreCase))
            return false;

        var parent = Directory.GetParent(normalizedRollbackRoot)?.FullName;
        if (parent is null)
            return false;

        var sourceRoot = Path.Combine(parent, "repo-common", "models");
        if (!Directory.Exists(sourceRoot))
            return false;

        CopyDirectory(sourceRoot, normalizedRollbackRoot);
        return true;
    }

    private static void CopyDirectory(string sourceRoot, string targetRoot)
    {
        Directory.CreateDirectory(targetRoot);
        foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, directory);
            Directory.CreateDirectory(Path.Combine(targetRoot, relative));
        }

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, file);
            File.Copy(file, Path.Combine(targetRoot, relative), overwrite: true);
        }
    }

    private static void WriteMarkdown(
        string path,
        object beforeAfterCounts,
        IReadOnlyList<string> remainingBlockers,
        bool activeProductRuntimeTargetVerified,
        bool noOverclaimVerified)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-17");
        builder.AppendLine();
        builder.AppendLine("QSG-17 activates the QSG-16 locator candidate only inside a candidate-scoped product API probe. It does not promote a target, update the production locator, mutate checkpoints, change host-side logic, or claim final readiness.");
        builder.AppendLine();
        builder.AppendLine("## What Was Verified");
        builder.AppendLine();
        builder.AppendLine($"- Test-only target-bound product API runtime: {activeProductRuntimeTargetVerified.ToString().ToLowerInvariant()}.");
        builder.AppendLine($"- No-overclaim through target-bound responses: {noOverclaimVerified.ToString().ToLowerInvariant()}.");
        builder.AppendLine("- Claude/Cursor/Codex-shaped product API requests all used the same QSG-16 candidate-scoped asset root.");
        builder.AppendLine();
        builder.AppendLine("## What Stayed Closed");
        builder.AppendLine();
        builder.AppendLine("- Product semantic quality, common readiness, target promotion, production locator update, rollback closure, live-host readiness, and final readiness.");
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
        builder.AppendLine("Next independent track: QUALITY-SEMANTIC-GATES-18 should run fresh live host invocations against the target-bound locator path and prove rollback closure before any promotion/readiness claim.");

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
                Path.Combine(RepoRoot, "Tests", "ExtensionQualitySemanticGates17ProductRuntimeVerificationTests.cs")
            }
            .Concat(validationOutputFiles)
            .Concat([statePatchPath])
            // Это поле читает родительская сессия как список готовых к публикации файлов.
            // Пишем repo-owned пути одинаково для Windows и WSL, чтобы повторный запуск
            // не менял evidence только из-за среды, где был создан JSON.
            .Select(path => ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRefIfOwned(
                path,
                "QUALITY-SEMANTIC-GATES-17 pending commit files"))
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
                ["phase_status"] = "TEST_ONLY_PRODUCT_API_TARGET_BINDING_VERIFIED_CLAIMS_CLOSED",
                ["phase_scope"] = "QSG-17 activated the QSG-16 locator candidate only as a test-only XiigenExtensionApiService user-adaptive asset root and ran Claude/Cursor/Codex-shaped product API probes. Product semantic quality, production locator update, irreversible promotion, rollback closure, live-host readiness, and final readiness remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-18: run fresh live Claude/Cursor/Codex host invocations against the QSG-16 target-bound locator path or repair the host adapters to pass that locator explicitly, then prove rollback closure before any promotion/readiness claim.",
                ["continuation_required"] = true,
                ["implementation_authorized_scope"] = "QUALITY-SEMANTIC-GATES-17 bounded sub-agent slice: run/repair active product-runtime target verification for the QSG-16 locator candidate through test-only candidate-scoped activation; no commit/push, irreversible promotion, production locator update, host-side logic, external AI, or final readiness claim.",
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
                ["git_authority_seen"] = "commit/push forbidden by current QUALITY-SEMANTIC-GATES-17 work order; pending commit boundary recorded only",
                ["git_write_or_ref_commands_run"] = Array.Empty<string>(),
                ["git_directives_emitted"] = Array.Empty<string>(),
                ["pending_commit_boundary"] = new
                {
                    pending = true,
                    phase_id = PhaseId,
                    commit_push_forbidden_by_current_work_order = true,
                    parent_will_perform_commit_push_after_review = true,
                    exact_commit_message = "phase QUALITY-SEMANTIC-GATES-17: verify candidate scoped product runtime target binding",
                    files_to_stage = filesToStage,
                    phase_commit_accepted_by_worker_evidence = true
                }
            },
            report,
            validation,
            JsonOptions);
    }

    private static string ReadCheckSet(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) ? value : string.Empty;

    private static string RepoPath(string relativePath)
        => Path.GetFullPath(Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

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

    private sealed record HostCase(
        string HostName,
        string HostType,
        string Prompt);

    private sealed record LocatorRef(
        string MachineId,
        string ProposedTargetRef,
        string ProposedManifestRef,
        string TargetSha256,
        string RollbackRef,
        string RollbackSha256);

    private sealed record ExpectedLocatorRef(
        string MachineId,
        string ProposedTargetRef,
        string ProposedManifestRef,
        string RollbackRef);

    private sealed record LocatorRefShapeValidation(
        bool IsValid,
        string[] Diagnostics,
        LocatorRef[] LocatorRefs,
        string TargetAssetRoot,
        string RollbackAssetRoot);

    private sealed record LocatorEvidenceValidation(
        bool IsValid,
        string[] Diagnostics,
        LocatorRef[] LocatorRefs,
        string TargetAssetRoot,
        string RollbackAssetRoot,
        bool TargetHashMatches,
        bool RollbackHashMatches,
        bool TargetManifestsExist);

    private sealed record HostRuntimeRow(
        string Schema,
        string PhaseId,
        string HostName,
        string HostType,
        string CorrelationId,
        string TargetCandidateId,
        string LocatorCandidateId,
        string ActivationScope,
        string TargetAssetRoot,
        bool TargetBoundProductApiProbeExecuted,
        bool TargetBoundProductApiProbePassed,
        bool ResponseSuccess,
        string ResponseStatus,
        bool PromptToTaskUserAssetLoadedFirst,
        bool OrchestrationReasoningUserAssetLoadedFirst,
        bool SkillSelectionUserAssetLoadedFirst,
        string UserAdaptiveAnyAssetLoaded,
        string PromptToTaskUserCheckpointRef,
        string OrchestrationReasoningUserCheckpointRef,
        string SkillSelectionUserCheckpointRef,
        string PromptToTaskUserLoadError,
        string OrchestrationReasoningUserLoadError,
        string SkillSelectionUserLoadError,
        string SemanticQualityClaimed,
        string RuntimeExtensionClaimed,
        int SelectedSkillCount,
        int WarningCount,
        int ErrorCount,
        bool HostSideLogicChanged,
        bool ExternalAiUsed,
        bool ProductReadinessClaimed,
        bool ExactReadinessPass,
        string WhatPassed,
        string WhatRemains);

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
