// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates18LiveHostTargetRollbackClosureTests
{
    private const string PhaseId = "QUALITY-SEMANTIC-GATES-18";
    private const string InputPhaseId = "QUALITY-SEMANTIC-GATES-17";
    private const string NextPhaseId = "QUALITY-SEMANTIC-GATES-19";
    private const string Command = "dotnet test Tests\\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionQualitySemanticGates18LiveHostTargetRollbackClosureTests --no-restore --nologo";

    private static readonly string RepoRoot = ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot;

    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;

    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates18_RecordsLiveHostTargetBlockersAndKeepsRollbackClosed()
    {
        var planPath = Path.Combine(RepoRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md");
        var stateSourcePath = StatePath;
        var qsg17ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-report.json");
        var qsg17ValidationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17.validation.json");
        var qsg17RowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-host-target-bound-runtime-rows.jsonl");
        var qsg17ActiveLocatorPath = Path.Combine(ArtifactDir, "quality-semantic-gates-17-test-only-active-locator.json");
        var qsg16LocatorPath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-locator-candidate.json");
        var qsg16RollbackProbePath = Path.Combine(ArtifactDir, "quality-semantic-gates-16-target-rollback-probe.json");
        var rollbackReportPath = Path.Combine(ArtifactDir, "rollback-report.json");
        var claudeProofPath = Path.Combine(ArtifactDir, "claude-runtime-proof.json");
        var cursorProofPath = Path.Combine(ArtifactDir, "cursor-runtime-proof-rerun.json");
        var codexProofPath = Path.Combine(ArtifactDir, "codex-runtime-proof-04g.json");

        using var qsg17ReportDocument = JsonDocument.Parse(File.ReadAllText(qsg17ReportPath, Encoding.UTF8));
        using var qsg17ValidationDocument = JsonDocument.Parse(File.ReadAllText(qsg17ValidationPath, Encoding.UTF8));
        using var activeLocatorDocument = JsonDocument.Parse(File.ReadAllText(qsg17ActiveLocatorPath, Encoding.UTF8));
        using var locatorDocument = JsonDocument.Parse(File.ReadAllText(qsg16LocatorPath, Encoding.UTF8));
        using var rollbackProbeDocument = JsonDocument.Parse(File.ReadAllText(qsg16RollbackProbePath, Encoding.UTF8));

        var qsg17Report = qsg17ReportDocument.RootElement;
        var qsg17Validation = qsg17ValidationDocument.RootElement;
        var activeLocator = activeLocatorDocument.RootElement;
        var locator = locatorDocument.RootElement;
        var rollbackProbe = rollbackProbeDocument.RootElement;

        var hostRows = BuildHostRows(
            locator,
            qsg17RowsPath,
            qsg17ActiveLocatorPath,
            claudeProofPath,
            cursorProofPath,
            codexProofPath);
        var rollbackClosure = BuildRollbackClosure(
            locator,
            rollbackProbe,
            qsg16RollbackProbePath,
            rollbackReportPath,
            qsg17ActiveLocatorPath);
        var finalReadinessRows = BuildFinalReadinessRows(hostRows, rollbackClosure, qsg17RowsPath, qsg16RollbackProbePath);
        var outputPaths = BuildOutputPaths();

        WriteJsonl(outputPaths.HostRowsPath, hostRows);
        WriteJson(outputPaths.RollbackClosurePath, rollbackClosure);
        WriteJsonl(outputPaths.FinalReadinessRowsPath, finalReadinessRows);

        var report = BuildReport(
            planPath,
            stateSourcePath,
            qsg17Report,
            qsg17Validation,
            activeLocator,
            locator,
            rollbackClosure,
            hostRows,
            finalReadinessRows,
            outputPaths);
        var validation = BuildValidation(hostRows, rollbackClosure, finalReadinessRows, outputPaths);

        WriteJson(outputPaths.ReportPath, report);
        WriteMarkdown(outputPaths.ReportMdPath, report, hostRows, rollbackClosure);
        WriteJson(outputPaths.ValidationPath, validation);
        var statePatchPath = UpdateState(report, validation, outputPaths);

        Assert.All(hostRows, row => Assert.False(row.TrueLiveTargetBoundInvocationPassed));
        Assert.False(rollbackClosure.RollbackClosureDone);
        Assert.False(report.ProductReadinessClaimAllowed);
        Assert.False(report.FinalReadinessClaimed);
        Assert.Equal(10, finalReadinessRows.Length);
        Assert.Equal(0, finalReadinessRows.Count(row => row.CountsAsFinalReadinessPass));

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath, Encoding.UTF8));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal(PhaseId, stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.False(stateRoot.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(stateRoot.GetProperty("final_readiness_claimed").GetBoolean());
    }

    private static HostLiveTargetRow[] BuildHostRows(
        JsonElement locator,
        string qsg17RowsPath,
        string activeLocatorPath,
        string claudeProofPath,
        string cursorProofPath,
        string codexProofPath)
    {
        var targetCandidateId = locator.GetProperty("target_candidate_id").GetString()!;
        var locatorCandidateId = locator.GetProperty("locator_candidate_id").GetString()!;
        var targetAssetRoot = "artifacts/extension-runtime-token-quality-learnability/adaptive-learnability-01/user-cache-continued";
        return
        [
            new HostLiveTargetRow(
                Schema: "xiigen.quality_semantic_gates_18.live_host_target_bound_row.v1",
                PhaseId: PhaseId,
                HostName: "Claude",
                HostType: "claude-code",
                InstalledRuntimeDetected: File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Claude", "claude-code", "2.1.170", "claude.exe")),
                HistoricalLiveRuntimePacketRef: Rel(claudeProofPath),
                HistoricalRuntimeScope: "RUNTIME-02 Claude Code SessionStart hook smoke; not QSG-16 target-bound.",
                TargetCandidateId: targetCandidateId,
                LocatorCandidateId: locatorCandidateId,
                TargetAssetRoot: targetAssetRoot,
                TrueLiveTargetBoundInvocationAttempted: false,
                TrueLiveTargetBoundInvocationPassed: false,
                ExactBlockerCode: "CLAUDE_ADAPTER_TARGET_LOCATOR_CONTRACT_MISSING",
                ExactBlocker: "Claude entrypoint has no QSG-16 locator or user-adaptive asset-root parameter in the live host request path, so rerunning the normal Claude smoke would spend runtime without proving target-bound loading.",
                NarrowFallbackRef: Rel(qsg17RowsPath),
                NarrowFallbackScope: "QSG-17 Claude-shaped product API request loaded the target through a test-only XiigenExtensionApiService constructor; it is not live Claude host evidence.",
                RequiredQsg19Repair: "Add a host-to-core target-locator contract or phase-scoped environment contract, then run Claude Code SessionStart with visible answer, redacted input/output, correlation id, negative path, and core trace proving the QSG-16 target asset root loaded."),
            new HostLiveTargetRow(
                Schema: "xiigen.quality_semantic_gates_18.live_host_target_bound_row.v1",
                PhaseId: PhaseId,
                HostName: "Cursor",
                HostType: "cursor-extension-host",
                InstalledRuntimeDetected: File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "cursor", "Cursor.exe")),
                HistoricalLiveRuntimePacketRef: Rel(cursorProofPath),
                HistoricalRuntimeScope: "RUNTIME-03-RERUN Cursor Extension Host command path; not QSG-16 target-bound.",
                TargetCandidateId: targetCandidateId,
                LocatorCandidateId: locatorCandidateId,
                TargetAssetRoot: targetAssetRoot,
                TrueLiveTargetBoundInvocationAttempted: false,
                TrueLiveTargetBoundInvocationPassed: false,
                ExactBlockerCode: "CURSOR_EXTENSION_TARGET_LOCATOR_CONTRACT_MISSING",
                ExactBlocker: "Cursor command path can launch noninteractively, but extension.ts does not pass an active QSG-16 locator or asset-root into XiigenExtensionApiService; a rerun would prove only the old command smoke.",
                NarrowFallbackRef: Rel(qsg17RowsPath),
                NarrowFallbackScope: "QSG-17 Cursor-shaped product API request loaded the target through a test-only XiigenExtensionApiService constructor; it is not live Cursor host evidence.",
                RequiredQsg19Repair: "Add a Cursor host command/test harness setting that passes the QSG-16 locator target into the core boundary, then run Extension Host positive and controlled negative commands with trace refs."),
            new HostLiveTargetRow(
                Schema: "xiigen.quality_semantic_gates_18.live_host_target_bound_row.v1",
                PhaseId: PhaseId,
                HostName: "Codex",
                HostType: "codex-app-server",
                InstalledRuntimeDetected: File.Exists(@"C:\Program Files\WindowsApps\OpenAI.Codex_26.609.4994.0_x64__2p2nqsd0c76g0\app\resources\codex.exe")
                    || File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin", "codex.exe")),
                HistoricalLiveRuntimePacketRef: Rel(codexProofPath),
                HistoricalRuntimeScope: "RUNTIME-04G Codex app-server/chat adapter core-boundary proof; not QSG-16 target-bound.",
                TargetCandidateId: targetCandidateId,
                LocatorCandidateId: locatorCandidateId,
                TargetAssetRoot: targetAssetRoot,
                TrueLiveTargetBoundInvocationAttempted: false,
                TrueLiveTargetBoundInvocationPassed: false,
                ExactBlockerCode: "CODEX_APP_SERVER_TARGET_LOCATOR_CONTRACT_MISSING",
                ExactBlocker: "Codex adapter can carry HostFacts metadata, but the live app-server adapter/core path does not bind that metadata to the XiigenExtensionApiService user-adaptive asset-root constructor; running app-server now would not prove target-bound loading.",
                NarrowFallbackRef: Rel(activeLocatorPath),
                NarrowFallbackScope: "QSG-17 test-only active locator proves candidate asset existence and API-level loading only; it is not Codex app/chat UI target-bound proof.",
                RequiredQsg19Repair: "Add Codex installed-skill/app-server adapter support for an approved target-locator contract, then rerun app-server/chat positive and controlled negative turns with request/response/trace proving target asset loading.")
        ];
    }

    private static RollbackClosureReport BuildRollbackClosure(
        JsonElement locator,
        JsonElement rollbackProbe,
        string rollbackProbePath,
        string rollbackReportPath,
        string activeLocatorPath)
    {
        var targetPromotionDone = locator.GetProperty("locator_update_done").GetBoolean();
        var locatorUpdateDone = locator.GetProperty("product_runtime_binding_done").GetBoolean();
        var probePassedForPromotion = rollbackProbe.GetProperty("exact_probe_passed_for_promotion").GetBoolean();
        var rollbackClosureDone = targetPromotionDone && locatorUpdateDone && probePassedForPromotion;
        return new RollbackClosureReport(
            Schema: "xiigen.quality_semantic_gates_18.rollback_closure.v1",
            PhaseId: PhaseId,
            InputPhaseId: InputPhaseId,
            InspectedRefs:
            [
                Rel(rollbackProbePath),
                Rel(rollbackReportPath),
                Rel(activeLocatorPath)
            ],
            RollbackCandidateId: locator.GetProperty("rollback_candidate_id").GetString()!,
            TargetCandidateId: locator.GetProperty("target_candidate_id").GetString()!,
            LocatorCandidateId: locator.GetProperty("locator_candidate_id").GetString()!,
            Qsg16RollbackProbeStatus: rollbackProbe.GetProperty("probe_status").GetString()!,
            ExactProbePassedForPromotion: probePassedForPromotion,
            TargetPromotionDone: targetPromotionDone,
            LocatorUpdateDone: locatorUpdateDone,
            RollbackClosureDone: rollbackClosureDone,
            TargetPromotionAllowed: false,
            WhyRollbackRemainsInactive: "QSG-16 rollback evidence is linked but not promotion-grade, QSG-17 activation was test-only, and no production locator rollback/load probe was applied.",
            RequiredQsg19Action: "After live target-bound host proof exists, run a production-locator rollback/load probe or keep target promotion disabled; do not promote target checkpoint or locator until rollback passes.")
        ;
    }

    private static FinalReadinessRow[] BuildFinalReadinessRows(
        IReadOnlyList<HostLiveTargetRow> hostRows,
        RollbackClosureReport rollbackClosure,
        string qsg17RowsPath,
        string rollbackProbePath)
    {
        var hostBlockers = hostRows.Select(row => row.ExactBlockerCode).ToArray();
        return
        [
            Row(1, "Claude live host runtime readiness", "Claude target-bound host invocation is still blocked.", [hostRows[0].HistoricalLiveRuntimePacketRef, Rel(qsg17RowsPath)], hostRows[0].ExactBlocker, hostBlockers),
            Row(2, "Cursor live host runtime readiness", "Cursor target-bound host invocation is still blocked.", [hostRows[1].HistoricalLiveRuntimePacketRef, Rel(qsg17RowsPath)], hostRows[1].ExactBlocker, hostBlockers),
            Row(3, "Codex live host runtime readiness", "Codex target-bound app-server/plugin invocation is still blocked.", [hostRows[2].HistoricalLiveRuntimePacketRef, Rel(qsg17RowsPath)], hostRows[2].ExactBlocker, hostBlockers),
            Row(4, "Cross-host end-to-end behavior", "No host has fresh target-bound live evidence.", [Rel(qsg17RowsPath)], "QSG-17 fallback is API-level only; cross-host live target-bound behavior remains open.", hostBlockers),
            Row(5, "Token before/after", "Token savings remain closed.", ["artifacts/extension-runtime-token-quality-learnability/token-04-quality-scoring-full-hosts-report.json"], "No new target-bound live host token run was performed.", ["TOKEN_SAVINGS_NOT_PROVEN"]),
            Row(6, "Quality improvement", "Quality improvement remains closed.", ["artifacts/extension-runtime-token-quality-learnability/token-04-quality-scoring-full-hosts-report.json"], "No new target-bound live host quality run was performed, and semantic_quality_claimed remains false.", ["QUALITY_IMPROVEMENT_NOT_PROVEN"]),
            Row(7, "Learnability and DPO", "Learnability remains closed.", ["artifacts/extension-runtime-token-quality-learnability/continue-training-report.json"], "QSG-18 did not train, mutate checkpoints, or prove new DPO learning.", ["LEARNABILITY_NOT_PROVEN"]),
            Row(8, "Backup/export/import/share and rollback", "Rollback closure remains false.", [Rel(rollbackProbePath)], rollbackClosure.WhyRollbackRemainsInactive, ["ROLLBACK_CLOSURE_FALSE"]),
            Row(9, "Boundary and architecture", "Core boundary remains protected but target locator plumbing is missing.", ["43.SmartSkillsDomain/XiigenExtensionApiService.cs"], "No host-side semantic logic was added; QSG-19 must add explicit thin target-locator plumbing rather than hiding logic in hosts.", ["TARGET_LOCATOR_CONTRACT_MISSING"]),
            Row(10, "Documentation and no-overclaim", "No readiness claim is allowed.", ["artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-18-report.md"], "QSG-18 records blockers and fallback only; final readiness stays IN_PROGRESS.", ["FINAL_READINESS_CLOSED"])
        ];
    }

    private static FinalReadinessRow Row(
        int pass,
        string requiredCheck,
        string targetOutcomeRow,
        string[] artifactsInspected,
        string whatRemains,
        string[] blockers)
        => new(
            Schema: "xiigen.quality_semantic_gates_18.final_readiness_check.v1",
            PhaseId: PhaseId,
            FinalReadinessPass: pass,
            SubagentId: $"QUALITY-SEMANTIC-GATES-18-FINAL-READINESS-{pass:00}",
            TargetOutcomeRow: targetOutcomeRow,
            RequiredCheck: requiredCheck,
            ArtifactsInspected: artifactsInspected,
            Verdict: "IN_PROGRESS",
            ExactlyWhatPassed: "QSG-18 preserved no-overclaim boundaries and recorded exact blockers/fallback scope.",
            ExactlyWhatRemains: whatRemains,
            RequiredFixRefs: [NextPhaseId],
            RecheckEvidence: "Counts as zero final readiness passes because target-bound live host proof and rollback closure are still missing.",
            CountsAsFinalReadinessPass: false,
            DashboardOrHelperSubstituteUsed: false,
            RemainingBlockers: blockers);

    private static ReportPaths BuildOutputPaths()
        => new(
            HostRowsPath: Path.Combine(ArtifactDir, "quality-semantic-gates-18-live-host-target-bound-rows.jsonl"),
            RollbackClosurePath: Path.Combine(ArtifactDir, "quality-semantic-gates-18-rollback-closure.json"),
            FinalReadinessRowsPath: Path.Combine(ArtifactDir, "quality-semantic-gates-18-final-readiness-checks.jsonl"),
            ReportPath: Path.Combine(ArtifactDir, "quality-semantic-gates-18-report.json"),
            ReportMdPath: Path.Combine(ArtifactDir, "quality-semantic-gates-18-report.md"),
            ValidationPath: Path.Combine(ArtifactDir, "quality-semantic-gates-18.validation.json"),
            StatePatchPath: ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, PhaseId));

    private static Qsg18Report BuildReport(
        string planPath,
        string statePath,
        JsonElement qsg17Report,
        JsonElement qsg17Validation,
        JsonElement activeLocator,
        JsonElement locator,
        RollbackClosureReport rollbackClosure,
        IReadOnlyList<HostLiveTargetRow> hostRows,
        IReadOnlyList<FinalReadinessRow> finalReadinessRows,
        ReportPaths outputPaths)
        => new(
            Schema: "xiigen.quality_semantic_gates_18.live_host_target_rollback_report.v1",
            PhaseId: PhaseId,
            InputPhaseId: InputPhaseId,
            PlanSourceUsed: Rel(planPath),
            StateSourceUsed: Rel(statePath),
            Qsg17ReportRef: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-17-report.json",
            Qsg17ValidationRef: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-17.validation.json",
            Qsg17PhaseStatus: qsg17Report.GetProperty("next_bounded_work_order").GetString()!,
            Qsg17TargetBoundHostShapedApiProbePasses: qsg17Report.GetProperty("target_bound_host_shaped_api_probe_passes").GetInt32(),
            Qsg17FinalReadinessPasses: qsg17Report.GetProperty("final_readiness_passes").GetInt32(),
            Qsg17RollbackClosureDone: qsg17Report.GetProperty("rollback_closure_done").GetBoolean(),
            SourceLocatorCandidateRef: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-16-locator-candidate.json",
            TestOnlyActiveLocatorRef: "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-17-test-only-active-locator.json",
            LocatorCandidateId: locator.GetProperty("locator_candidate_id").GetString()!,
            TargetCandidateId: locator.GetProperty("target_candidate_id").GetString()!,
            RollbackCandidateId: locator.GetProperty("rollback_candidate_id").GetString()!,
            ActivatedAssetRoot: activeLocator.GetProperty("activated_asset_root").GetString()!,
            LiveHostTargetBoundRowsRef: Rel(outputPaths.HostRowsPath),
            RollbackClosureRef: Rel(outputPaths.RollbackClosurePath),
            FinalReadinessChecksRef: Rel(outputPaths.FinalReadinessRowsPath),
            TrueLiveHostTargetBoundInvocationsAttempted: hostRows.Count(row => row.TrueLiveTargetBoundInvocationAttempted),
            TrueLiveHostTargetBoundInvocationPasses: hostRows.Count(row => row.TrueLiveTargetBoundInvocationPassed),
            HostSpecificBlockers: hostRows.ToDictionary(row => row.HostName, row => row.ExactBlockerCode),
            NarrowFallbackEvidence: "QSG-17 target-bound product API rows are retained as scoped intermediate evidence only.",
            RollbackClosureDone: rollbackClosure.RollbackClosureDone,
            TargetPromotionDone: false,
            LocatorUpdateDone: false,
            ProductRuntimeSemanticQualityClaimed: false,
            ProductRuntimeSemanticQualityVerified: false,
            TokenSavingsClaimAllowed: false,
            QualityImprovementClaimAllowed: false,
            LearnabilityClaimed: false,
            CommonReadinessClaimAllowed: false,
            PromotionReadinessClaimed: false,
            ProductReadinessClaimAllowed: false,
            FinalReadinessClaimed: false,
            FinalReadinessChecksRunAfterQsg18: finalReadinessRows.Count,
            FinalReadinessPasses: finalReadinessRows.Count(row => row.CountsAsFinalReadinessPass),
            FinalReadinessInProgress: finalReadinessRows.Count(row => !row.CountsAsFinalReadinessPass),
            TrainingStarted: false,
            EvalStarted: false,
            CheckpointMutationStarted: false,
            ExternalAiUsedInQsg18: false,
            HostSideLogicChanged: false,
            Qsg17ValidationSeen: qsg17Validation.GetProperty("no_overclaim_verified").GetBoolean(),
            ClaimsKeptClosed:
            [
                "token_savings_claim_allowed",
                "quality_improvement_claim_allowed",
                "product_runtime_semantic_quality_claimed",
                "learnability_claimed",
                "common_readiness_claim_allowed",
                "target_promotion_done",
                "locator_update_done",
                "rollback_closure_done",
                "promotion_readiness_claimed",
                "product_readiness_claim_allowed",
                "final_readiness_claimed"
            ],
            RemainingBlockers:
            [
                "fresh_live_claude_cursor_codex_runtime_target_bound_not_executed",
                "host_adapter_target_locator_contract_missing",
                "rollback_closure_not_applied_to_active_locator",
                "production_locator_update_not_done",
                "ten_final_readiness_checks_remain_claim_closed_zero_counted_passes"
            ],
            NextBoundedWorkOrder: "QUALITY-SEMANTIC-GATES-19: add/verify thin host-to-core target-locator plumbing for Claude/Cursor/Codex, rerun live target-bound host probes with visible redacted traces and controlled negative paths, then run production-locator rollback/load closure before any promotion/readiness claim.");

    private static Qsg18Validation BuildValidation(
        IReadOnlyList<HostLiveTargetRow> hostRows,
        RollbackClosureReport rollbackClosure,
        IReadOnlyList<FinalReadinessRow> finalReadinessRows,
        ReportPaths outputPaths)
        => new(
            Schema: "xiigen.quality_semantic_gates_18.validation.v1",
            PhaseId: PhaseId,
            HostRowsExpected: 3,
            HostRowsActual: hostRows.Count,
            InstalledRuntimeDetectedHosts: hostRows.Count(row => row.InstalledRuntimeDetected),
            TrueLiveTargetBoundInvocationsAttempted: hostRows.Count(row => row.TrueLiveTargetBoundInvocationAttempted),
            TrueLiveTargetBoundInvocationPasses: hostRows.Count(row => row.TrueLiveTargetBoundInvocationPassed),
            HostSpecificBlockersRecorded: hostRows.All(row => !string.IsNullOrWhiteSpace(row.ExactBlockerCode)),
            NarrowFallbackRecorded: hostRows.All(row => !string.IsNullOrWhiteSpace(row.NarrowFallbackScope)),
            RollbackClosureDone: rollbackClosure.RollbackClosureDone,
            TargetPromotionAllowed: rollbackClosure.TargetPromotionAllowed,
            FinalReadinessChecksExpected: 10,
            FinalReadinessChecksActual: finalReadinessRows.Count,
            FinalReadinessPassesActual: finalReadinessRows.Count(row => row.CountsAsFinalReadinessPass),
            FinalReadinessInProgressActual: finalReadinessRows.Count(row => !row.CountsAsFinalReadinessPass),
            NoOverclaimVerified: true,
            OutputFiles:
            [
                Path.GetFullPath(outputPaths.HostRowsPath),
                Path.GetFullPath(outputPaths.RollbackClosurePath),
                Path.GetFullPath(outputPaths.FinalReadinessRowsPath),
                Path.GetFullPath(outputPaths.ReportPath),
                Path.GetFullPath(outputPaths.ReportMdPath),
                Path.GetFullPath(outputPaths.ValidationPath),
                Path.GetFullPath(outputPaths.StatePatchPath)
            ]);

    private static void WriteMarkdown(
        string path,
        Qsg18Report report,
        IReadOnlyList<HostLiveTargetRow> hostRows,
        RollbackClosureReport rollbackClosure)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-18");
        builder.AppendLine();
        builder.AppendLine("QSG-18 checked whether the already installed Claude, Cursor, and Codex runtimes can prove the active QSG-16 locator target through a fresh live host invocation. The answer is no for this slice: the host adapters do not yet have a target-locator or user-adaptive asset-root contract that the live host path binds into XiigenExtensionApiService.");
        builder.AppendLine();
        builder.AppendLine("## Live Host Evidence");
        builder.AppendLine();
        foreach (var row in hostRows)
        {
            builder.AppendLine($"- {row.HostName}: installed_runtime_detected={row.InstalledRuntimeDetected.ToString().ToLowerInvariant()}, live_target_bound_pass=false, blocker={row.ExactBlockerCode}.");
        }
        builder.AppendLine();
        builder.AppendLine("## Rollback Closure");
        builder.AppendLine();
        builder.AppendLine($"- rollback_closure_done={rollbackClosure.RollbackClosureDone.ToString().ToLowerInvariant()}.");
        builder.AppendLine($"- why: {rollbackClosure.WhyRollbackRemainsInactive}");
        builder.AppendLine();
        builder.AppendLine("## Claims Kept Closed");
        builder.AppendLine();
        foreach (var claim in report.ClaimsKeptClosed)
            builder.AppendLine($"- {claim}");
        builder.AppendLine();
        builder.AppendLine("## Next");
        builder.AppendLine();
        builder.AppendLine(report.NextBoundedWorkOrder);

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string UpdateState(Qsg18Report report, Qsg18Validation validation, ReportPaths outputPaths)
    {
        var filesToStage = new[]
        {
            Path.Combine(RepoRoot, "Tests", "ExtensionQualitySemanticGates18LiveHostTargetRollbackClosureTests.cs"),
            outputPaths.HostRowsPath,
            outputPaths.RollbackClosurePath,
            outputPaths.FinalReadinessRowsPath,
            outputPaths.ReportPath,
            outputPaths.ReportMdPath,
            outputPaths.ValidationPath,
            outputPaths.StatePatchPath
        };

        return ExtensionExecutionStatePatchWriter.WritePatch(
            StatePath,
            ArtifactDir,
            PhaseId,
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
                ["current_phase"] = PhaseId,
                ["phase_status"] = "LIVE_HOST_TARGET_BOUND_BLOCKED_FALLBACK_RECORDED_ROLLBACK_CLOSED_FALSE",
                ["phase_scope"] = "QSG-18 records that true live Claude/Cursor/Codex target-bound host verification cannot be proven without host-to-core target-locator plumbing. QSG-17 test-only API fallback remains supporting evidence only. Rollback closure, production locator update, promotion, semantic quality, token savings, learnability, and final readiness remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = report.NextBoundedWorkOrder,
                ["continuation_required"] = true,
                ["implementation_authorized_scope"] = "QUALITY-SEMANTIC-GATES-18 bounded sub-agent slice: inspect QSG-17/QSG-16 sources, determine live host target-bound feasibility, record blockers/fallback evidence, and verify rollback closure stays false; no commit/push, host-side logic rewrite, production locator update, irreversible promotion, external AI run, training, checkpoint mutation, or readiness claim.",
                ["common_readiness_claim_allowed"] = false,
                ["product_readiness_claim_allowed"] = false,
                ["target_promotion_done"] = false,
                ["locator_update_done"] = false,
                ["rollback_closure_done"] = false,
                ["product_runtime_semantic_quality_claimed"] = false,
                ["product_runtime_semantic_quality_verified"] = false,
                ["token_savings_claim_allowed"] = false,
                ["quality_improvement_claim_allowed"] = false,
                ["learnability_claimed"] = false,
                ["promotion_readiness_claimed"] = false,
                ["final_readiness_claimed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_started"] = false,
                ["runtime_claimed"] = false,
                ["runtime_launched"] = false,
                ["eval_started"] = false,
                ["external_ai_used"] = false,
                ["host_side_logic_changed"] = false,
                ["git_authority_seen"] = "commit/push forbidden by current QUALITY-SEMANTIC-GATES-18 work order; parent owns commit/push after review",
                ["git_write_or_ref_commands_run"] = Array.Empty<string>(),
                ["git_directives_emitted"] = Array.Empty<string>(),
                ["pending_commit_boundary"] = new
                {
                    pending = true,
                    phase_id = PhaseId,
                    commit_push_forbidden_by_current_work_order = true,
                    parent_will_perform_commit_push_after_review = true,
                    exact_commit_message = "phase QUALITY-SEMANTIC-GATES-18: record live host target blockers and rollback closure",
                    files_to_stage = filesToStage.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                    phase_commit_accepted_by_worker_evidence = true
                }
            },
            report,
            validation,
            JsonOptions);
    }

    private static string Rel(string path)
        => Path.GetRelativePath(RepoRoot, path).Replace('\\', '/');

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

    private sealed record ReportPaths(
        string HostRowsPath,
        string RollbackClosurePath,
        string FinalReadinessRowsPath,
        string ReportPath,
        string ReportMdPath,
        string ValidationPath,
        string StatePatchPath);

    private sealed record HostLiveTargetRow(
        string Schema,
        string PhaseId,
        string HostName,
        string HostType,
        bool InstalledRuntimeDetected,
        string HistoricalLiveRuntimePacketRef,
        string HistoricalRuntimeScope,
        string TargetCandidateId,
        string LocatorCandidateId,
        string TargetAssetRoot,
        bool TrueLiveTargetBoundInvocationAttempted,
        bool TrueLiveTargetBoundInvocationPassed,
        string ExactBlockerCode,
        string ExactBlocker,
        string NarrowFallbackRef,
        string NarrowFallbackScope,
        string RequiredQsg19Repair);

    private sealed record RollbackClosureReport(
        string Schema,
        string PhaseId,
        string InputPhaseId,
        string[] InspectedRefs,
        string RollbackCandidateId,
        string TargetCandidateId,
        string LocatorCandidateId,
        string Qsg16RollbackProbeStatus,
        bool ExactProbePassedForPromotion,
        bool TargetPromotionDone,
        bool LocatorUpdateDone,
        bool RollbackClosureDone,
        bool TargetPromotionAllowed,
        string WhyRollbackRemainsInactive,
        string RequiredQsg19Action);

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
        string[] RequiredFixRefs,
        string RecheckEvidence,
        bool CountsAsFinalReadinessPass,
        bool DashboardOrHelperSubstituteUsed,
        string[] RemainingBlockers);

    private sealed record Qsg18Report(
        string Schema,
        string PhaseId,
        string InputPhaseId,
        string PlanSourceUsed,
        string StateSourceUsed,
        string Qsg17ReportRef,
        string Qsg17ValidationRef,
        string Qsg17PhaseStatus,
        int Qsg17TargetBoundHostShapedApiProbePasses,
        int Qsg17FinalReadinessPasses,
        bool Qsg17RollbackClosureDone,
        string SourceLocatorCandidateRef,
        string TestOnlyActiveLocatorRef,
        string LocatorCandidateId,
        string TargetCandidateId,
        string RollbackCandidateId,
        string ActivatedAssetRoot,
        string LiveHostTargetBoundRowsRef,
        string RollbackClosureRef,
        string FinalReadinessChecksRef,
        int TrueLiveHostTargetBoundInvocationsAttempted,
        int TrueLiveHostTargetBoundInvocationPasses,
        IReadOnlyDictionary<string, string> HostSpecificBlockers,
        string NarrowFallbackEvidence,
        bool RollbackClosureDone,
        bool TargetPromotionDone,
        bool LocatorUpdateDone,
        bool ProductRuntimeSemanticQualityClaimed,
        bool ProductRuntimeSemanticQualityVerified,
        bool TokenSavingsClaimAllowed,
        bool QualityImprovementClaimAllowed,
        bool LearnabilityClaimed,
        bool CommonReadinessClaimAllowed,
        bool PromotionReadinessClaimed,
        bool ProductReadinessClaimAllowed,
        bool FinalReadinessClaimed,
        int FinalReadinessChecksRunAfterQsg18,
        int FinalReadinessPasses,
        int FinalReadinessInProgress,
        bool TrainingStarted,
        bool EvalStarted,
        bool CheckpointMutationStarted,
        bool ExternalAiUsedInQsg18,
        bool HostSideLogicChanged,
        bool Qsg17ValidationSeen,
        string[] ClaimsKeptClosed,
        string[] RemainingBlockers,
        string NextBoundedWorkOrder);

    private sealed record Qsg18Validation(
        string Schema,
        string PhaseId,
        int HostRowsExpected,
        int HostRowsActual,
        int InstalledRuntimeDetectedHosts,
        int TrueLiveTargetBoundInvocationsAttempted,
        int TrueLiveTargetBoundInvocationPasses,
        bool HostSpecificBlockersRecorded,
        bool NarrowFallbackRecorded,
        bool RollbackClosureDone,
        bool TargetPromotionAllowed,
        int FinalReadinessChecksExpected,
        int FinalReadinessChecksActual,
        int FinalReadinessPassesActual,
        int FinalReadinessInProgressActual,
        bool NoOverclaimVerified,
        string[] OutputFiles);

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
