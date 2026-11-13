// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Text.Json;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class ExtensionAdaptiveLearnability01Tests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public async Task ExtensionApi_LoadsUserAdaptiveAssetsFirst_AndProvesPortableContinueTrainingLifecycle()
    {
        var artifactRoot = Path.Combine(
            Path.GetTempPath(),
            $"xiigen-adaptive-learnability-01-{Guid.NewGuid():N}");
        Directory.CreateDirectory(artifactRoot);

        try
        {
            var commonRoot = Path.Combine(artifactRoot, "repo-common");
            var userRoot = Path.Combine(artifactRoot, "user-cache");
            var shareRoot = Path.Combine(artifactRoot, "share-import");
            var backupRoot = Path.Combine(artifactRoot, "backup");
            var rollbackRoot = Path.Combine(artifactRoot, "rollback");

            await WriteAssetSetAsync(Path.Combine(commonRoot, "models"), BuildCommonPromptToTaskMachine(), BuildCommonOrchestrationMachine(), BuildCommonSkillSelectionMachine());
            await WriteAssetSetAsync(userRoot, BuildUserPromptToTaskMachine(), BuildUserOrchestrationMachine(), BuildUserSkillSelectionMachine());

        var adaptivePrompt = "Please create portable backup adaptive learnability workflow for local model continuation.";
        var baseline = new XiigenExtensionApiService(commonRoot).ProcessHostExtensionRequest(CreateRequest(adaptivePrompt, "adl-001"));
        var freshLoad = new XiigenExtensionApiService(commonRoot, userRoot).ProcessHostExtensionRequest(CreateRequest(adaptivePrompt, "adl-002"));

        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        Assert.True(freshLoad.IsSuccess, freshLoad.ErrorMessage);
        Assert.Equal("true", baseline.Value!.CheckSetResults["all_common_assets_loaded"]);
        Assert.NotEqual(baseline.Value!.StructuredTaskRefs, freshLoad.Value!.StructuredTaskRefs);
        Assert.NotEqual(baseline.Value.SelectedSkills, freshLoad.Value.SelectedSkills);
        Assert.Contains("adaptive-model", freshLoad.Value.SelectedSkills);
        Assert.Equal("true", freshLoad.Value.CheckSetResults["user_adaptive_any_asset_loaded"]);
        Assert.Equal("user_cache_first_repo_common_fallback_second", freshLoad.Value.CheckSetResults["user_adaptive_load_order"]);
        Assert.Equal("user_adaptive_cache", freshLoad.Value.CheckSetResults["prompt_to_task_asset_source"]);
        Assert.Equal("user_adaptive_cache", freshLoad.Value.CheckSetResults["orchestration_reasoning_asset_source"]);
        Assert.Equal("user_adaptive_cache", freshLoad.Value.CheckSetResults["skill_selection_asset_source"]);
        AssertNoOverclaim(freshLoad.Value);

        CopyDirectory(userRoot, shareRoot);
        CopyDirectory(userRoot, backupRoot);
        CopyDirectory(userRoot, rollbackRoot);

        var shareLoad = new XiigenExtensionApiService(commonRoot, shareRoot).ProcessHostExtensionRequest(CreateRequest(adaptivePrompt, "adl-003"));
        Assert.True(shareLoad.IsSuccess, shareLoad.ErrorMessage);
        Assert.Equal(freshLoad.Value.SelectedSkills, shareLoad.Value!.SelectedSkills);
        Assert.Contains(freshLoad.Value.StructuredTaskRefs, value => value.Contains("user-adaptive", StringComparison.Ordinal));
        Assert.Contains(shareLoad.Value.StructuredTaskRefs, value => value.Contains("user-adaptive", StringComparison.Ordinal));
        Assert.Equal(freshLoad.Value.ScenarioRefs, shareLoad.Value.ScenarioRefs);
        AssertNoOverclaim(shareLoad.Value);

        var continuedRoot = Path.Combine(artifactRoot, "user-cache-continued");
        await CopyDirectoryAsync(userRoot, continuedRoot);
        await ContinueTrainingAsync(continuedRoot);

        var newScenarioPrompt = "Please build local model correction DPO backup share continue training.";
        var beforeContinue = new XiigenExtensionApiService(commonRoot, userRoot).ProcessHostExtensionRequest(CreateRequest(newScenarioPrompt, "adl-004"));
        var afterContinue = new XiigenExtensionApiService(commonRoot, continuedRoot).ProcessHostExtensionRequest(CreateRequest(newScenarioPrompt, "adl-005"));

        Assert.True(beforeContinue.IsSuccess, beforeContinue.ErrorMessage);
        Assert.True(afterContinue.IsSuccess, afterContinue.ErrorMessage);
        Assert.DoesNotContain("dpo-authoring", beforeContinue.Value!.SelectedSkills);
        Assert.Contains("dpo-authoring", afterContinue.Value!.SelectedSkills);
        Assert.Contains(afterContinue.Value.StructuredTaskRefs, value => value.Contains("continued", StringComparison.Ordinal));
        AssertNoOverclaim(afterContinue.Value);

        var report = new AdaptiveLearnabilityReport(
            Schema: "xiigen.adaptive_learnability_01.report.v1",
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            Scope: "test-local user adaptive assets for extension API path; no common promotion and no private user data mutation",
            CommonRoot: commonRoot,
            UserRoot: userRoot,
            BackupRoot: backupRoot,
            ShareRoot: shareRoot,
            RollbackRoot: rollbackRoot,
            ContinuedRoot: continuedRoot,
            UserCacheLoadedFirst: freshLoad.Value.CheckSetResults["user_adaptive_any_asset_loaded"] == "true",
            RepoCommonFallbackAvailable: baseline.Value.CheckSetResults.TryGetValue("all_common_assets_loaded", out var allCommon) && allCommon == "true",
            FreshLoadChangedBehaviorVsBaseline: !baseline.Value.SelectedSkills.SequenceEqual(freshLoad.Value.SelectedSkills),
            ShareImportPreservedBehavior: shareLoad.Value.SelectedSkills.SequenceEqual(freshLoad.Value.SelectedSkills)
                && shareLoad.Value.ScenarioRefs.SequenceEqual(freshLoad.Value.ScenarioRefs),
            ContinueTrainingChangedNewScenario: afterContinue.Value.SelectedSkills.Contains("dpo-authoring")
                && !beforeContinue.Value.SelectedSkills.Contains("dpo-authoring"),
            BackupArtifactExists: Directory.Exists(backupRoot) && Directory.EnumerateFiles(backupRoot, "*", SearchOption.AllDirectories).Any(),
            RollbackArtifactExists: Directory.Exists(rollbackRoot) && Directory.EnumerateFiles(rollbackRoot, "*", SearchOption.AllDirectories).Any(),
            DpoSemantics:
            [
                "PromptToTask/Orchestration checkpoints carry accepted refs for controlled chosen patterns.",
                "SkillSelection uses SkillSelectionMachinePair chosen/rejected rows with TargetTrainingAdapter=IPromptContextTraining.",
                "No exact source-row id, candidate position, or hardcoded prompt-to-answer lookup is final authority."
            ],
            Claims: new AdaptiveLearnabilityClaims(
                NarrowUserLocalLearnabilityAllowed: true,
                SemanticQualityClaimed: false,
                TokenSavingsClaimAllowed: false,
                CommonModelReadinessClaimed: false,
                TargetPromotionDone: false,
                LocatorUpdated: false));

        var reportJson = Path.Combine(artifactRoot, "adaptive-learnability-01-report.json");
        var reportMd = Path.Combine(artifactRoot, "adaptive-learnability-01-report.md");
        var validationJson = Path.Combine(artifactRoot, "adaptive-learnability-01.validation.json");
        await File.WriteAllTextAsync(reportJson, JsonSerializer.Serialize(report, JsonOptions));
        await File.WriteAllTextAsync(reportMd, BuildMarkdownReport(report));
            await File.WriteAllTextAsync(validationJson, JsonSerializer.Serialize(new
            {
                schema = "xiigen.adaptive_learnability_01.validation.v1",
                passed = true,
                row_count = 1,
                user_cache_load_first = true,
                repo_common_fallback_available = true,
                backup_export_import_share = true,
                fresh_load_behavior_changed = true,
                continue_training_behavior_changed = true,
                semantic_quality_claimed = false,
                token_savings_claim_allowed = false
            }, JsonOptions));
        }
        finally
        {
            if (Directory.Exists(artifactRoot))
                Directory.Delete(artifactRoot, recursive: true);
        }
    }

    private static async Task WriteAssetSetAsync(
        string root,
        PromptToTaskMachine promptToTask,
        OrchestrationReasoningMachine orchestration,
        SkillSelectionMachine skillSelection)
    {
        var pttDir = Path.Combine(root, "prompt-to-task", "prompt-to-task-v1");
        var orDir = Path.Combine(root, "orchestration-reasoning", "orchestration-reasoning-v1");
        var skillDir = Path.Combine(root, "skill-selection", "skill-selection-v1");

        var pttSave = await promptToTask.SaveCheckpointAsync(
            Path.Combine(pttDir, "prompt-to-task-v1-checkpoint.json"),
            Path.Combine(pttDir, "manifest.json"),
            "adaptive-learnability-01-source-manifest",
            ["adl-ptt-accepted"]);
        Assert.True(pttSave.IsSuccess, pttSave.ErrorMessage);

        var orSave = await orchestration.SaveCheckpointAsync(
            Path.Combine(orDir, "checkpoint.json"),
            Path.Combine(orDir, "manifest.json"));
        Assert.True(orSave.IsSuccess, orSave.ErrorMessage);

        var skillSave = await skillSelection.SaveCheckpointAsync(
            Path.Combine(skillDir, "skill-selection-v1-checkpoint.json"),
            Path.Combine(skillDir, "manifest.json"));
        Assert.True(skillSave.IsSuccess, skillSave.ErrorMessage);
    }

    private static PromptToTaskMachine BuildCommonPromptToTaskMachine()
        => ImportPromptToTask(
            "common-ptt-state",
            new PromptToTaskLearnedPattern(
                PatternId: "common-generic-review",
                TaskType: "generic_review",
                LearnedSignals: ["prompt:review", "prompt:generic"],
                ExplicitUserRequirements: ["review a generic plan"],
                Constraints: ["common_generic_review"],
                Deliverables: ["generic review output"],
                ForbiddenActions: ["semantic quality claim"],
                RequiredHosts: ["codex"],
                EvidenceNeeds: ["common_only"],
                SplitId: "common",
                Confidence: 0.9));

    private static PromptToTaskMachine BuildUserPromptToTaskMachine()
        => ImportPromptToTask(
            "user-ptt-state",
            new PromptToTaskLearnedPattern(
                PatternId: "user-adaptive-portability",
                TaskType: "adaptive_learnability",
                LearnedSignals: ["prompt:portable", "prompt:backup", "prompt:adaptive", "prompt:learnability"],
                ExplicitUserRequirements: ["prove user-local adaptive model lifecycle"],
                Constraints: ["user_local_model", "backup_export_import_share", "fresh_load_required"],
                Deliverables: ["portable adaptive lifecycle proof"],
                ForbiddenActions: ["semantic quality claim", "token savings claim"],
                RequiredHosts: ["codex", "cursor", "claude"],
                EvidenceNeeds: ["backup_exists", "share_import_roundtrip", "continue_training_probe"],
                SplitId: "user-adaptive",
                Confidence: 0.95));

    private static PromptToTaskMachine ImportPromptToTask(string stateId, PromptToTaskLearnedPattern pattern)
    {
        var machine = new PromptToTaskMachine();
        var imported = machine.ImportLearnedState(new PromptToTaskCheckpoint(
            SchemaVersion: 1,
            LearnedState: new PromptToTaskLearnedStateDescriptor(
                StateId: stateId,
                ManifestId: $"{stateId}-manifest",
                ManifestSha256: $"sha256:{stateId}-manifest",
                MachineType: nameof(PromptToTaskMachine),
                SchemaVersion: "prompt-to-task-state-v1",
                SourceBaseCommit: "test-local",
                StateSha256: $"sha256:{stateId}",
                LearnedPatterns: [pattern]),
            AcceptedRefIds: [$"{stateId}-accepted"],
            SourceManifestRef: $"{stateId}-source",
            SemanticQualityClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow));
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return machine;
    }

    private static OrchestrationReasoningMachine BuildCommonOrchestrationMachine()
        => ImportOrchestration(
            "common-or-state",
            new OrchestrationReasoningLearnedPattern(
                PatternId: "common-generic-review-orchestration",
                ScenarioRef: "scenario:common-generic-review",
                RequiredTaskTypes: ["generic_review"],
                RequiredConstraintSignals: ["common_generic_review"],
                RequiredRequirementSignals: ["review a generic plan"],
                RequiredDeliverableSignals: ["generic review output"],
                RequiredEvidenceSignals: ["common_only"],
                PhaseFamilies: ["review"],
                GateFamilies: ["common-baseline"],
                AssetFamilies: ["repo-common"],
                HostCheckSets: ["core-only"],
                RiskFlags: ["no_semantic_claim"],
                NextGranularizationActions: ["keep claims false"],
                Confidence: 0.9));

    private static OrchestrationReasoningMachine BuildUserOrchestrationMachine()
        => ImportOrchestration(
            "user-or-state",
            new OrchestrationReasoningLearnedPattern(
                PatternId: "user-adaptive-portability-orchestration",
                ScenarioRef: "scenario:user-adaptive-portability",
                RequiredTaskTypes: ["adaptive_learnability"],
                RequiredConstraintSignals: ["user_local_model", "backup_export_import_share", "fresh_load_required"],
                RequiredRequirementSignals: ["prove user-local adaptive model lifecycle"],
                RequiredDeliverableSignals: ["portable adaptive lifecycle proof"],
                RequiredEvidenceSignals: ["backup_exists", "share_import_roundtrip", "continue_training_probe"],
                PhaseFamilies: ["adaptive-learnability"],
                GateFamilies: ["backup-export-import-share", "fresh-load", "continue-training"],
                AssetFamilies: ["user-cache", "repo-common-fallback"],
                HostCheckSets: ["extension-api-core"],
                RiskFlags: ["baseline_not_semantic_quality"],
                NextGranularizationActions: ["prove adaptive effect before token claims"],
                Confidence: 0.95));

    private static OrchestrationReasoningMachine ImportOrchestration(string stateId, OrchestrationReasoningLearnedPattern pattern)
    {
        var machine = new OrchestrationReasoningMachine();
        var admitted = OrchestrationReasoningSourceAdmissionTestFixture.Admit(
            [pattern],
            stateId,
            stateId);
        var imported = machine.ImportAdmittedState(admitted);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return machine;
    }

    private static SkillSelectionMachine BuildCommonSkillSelectionMachine()
        => TrainSkillSelection("common skill selection review generic", "plan-review", "common-skill-pair");

    private static SkillSelectionMachine BuildUserSkillSelectionMachine()
        => TrainSkillSelection("portable backup adaptive learnability local model continuation", "adaptive-model", "user-skill-pair");

    private static SkillSelectionMachine TrainSkillSelection(string prompt, string selectedSkill, string pairId)
    {
        var machine = new SkillSelectionMachine();
        var available = new[] { "adaptive-model", "dpo-authoring", "plan-review", "runtime-qa" };
        var training = machine.TrainMachineBatchAsync(
        [
            new SkillSelectionMachinePair(
                PairId: pairId,
                Request: new SkillSelectionRequest(
                    Prompt: prompt,
                    ProjectRoot: "C:/Projects/llm_mvp_core",
                    AvailableSkillIds: available,
                    HostFacts: new Dictionary<string, string> { ["host"] = "codex", ["method"] = "skill-selection" },
                    UserPromptSourceEntryId: pairId),
                Chosen: new SkillSelectionDecision(
                    SelectedSkillIds: [selectedSkill],
                    RejectedSkillIds: available.Where(skill => skill != selectedSkill).ToArray(),
                    EvidenceFeatures: ["CONTROLLED_USER_LOCAL_DPO_CHOSEN"],
                    Confidence: 0.9,
                    RequiresSecondPass: false,
                    DecisionText: "Controlled local adaptive DPO chosen row."),
                Rejected: new SkillSelectionDecision(
                    SelectedSkillIds: ["plan-review"],
                    RejectedSkillIds: [selectedSkill],
                    EvidenceFeatures: ["CONTROLLED_USER_LOCAL_DPO_REJECTED"],
                    Confidence: 0.1,
                    RequiresSecondPass: true,
                    DecisionText: "Rejected row misses the requested adaptive lifecycle skill."),
                RejectedType: "missing_user_local_adaptive_skill",
                SourcePromptEntryId: pairId,
                TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter)
        ]).GetAwaiter().GetResult();
        Assert.True(training.IsSuccess, training.ErrorMessage);
        return machine;
    }

    private static async Task ContinueTrainingAsync(string continuedRoot)
    {
        var pttPath = Path.Combine(continuedRoot, "prompt-to-task", "prompt-to-task-v1", "prompt-to-task-v1-checkpoint.json");
        var pttManifest = Path.Combine(continuedRoot, "prompt-to-task", "prompt-to-task-v1", "manifest.json");
        var pttLoad = await PromptToTaskMachine.LoadCheckpointAsync(pttPath, pttManifest);
        Assert.True(pttLoad.IsSuccess, pttLoad.ErrorMessage);
        var pttContinue = pttLoad.Value!.Machine.ContinueWithAcceptedPattern(
            new PromptToTaskLearnedPattern(
                PatternId: "continued-dpo-authoring",
                TaskType: "adaptive_learnability_continued",
                LearnedSignals: ["prompt:correction", "prompt:dpo", "prompt:continue", "prompt:training"],
                ExplicitUserRequirements: ["build local DPO from correction"],
                Constraints: ["user_local_model", "dpo_correction_row", "continue_training_required"],
                Deliverables: ["continued adaptive DPO proof"],
                ForbiddenActions: ["semantic quality claim", "token savings claim"],
                RequiredHosts: ["codex", "cursor", "claude"],
                EvidenceNeeds: ["previous_checkpoint_loaded", "new_pair_added", "fresh_load_after_continue"],
                SplitId: "continued",
                Confidence: 0.95),
            "adl-ptt-continued-accepted");
        Assert.True(pttContinue.IsSuccess, pttContinue.ErrorMessage);
        var pttSave = await pttLoad.Value.Machine.SaveCheckpointAsync(pttPath, pttManifest, "adaptive-learnability-01-continued-source", ["adl-ptt-continued-accepted"]);
        Assert.True(pttSave.IsSuccess, pttSave.ErrorMessage);

        var orPath = Path.Combine(continuedRoot, "orchestration-reasoning", "orchestration-reasoning-v1", "checkpoint.json");
        var orManifest = Path.Combine(continuedRoot, "orchestration-reasoning", "orchestration-reasoning-v1", "manifest.json");
        var orLoad = await OrchestrationReasoningMachine.LoadCheckpointAsync(orPath, orManifest);
        Assert.True(orLoad.IsSuccess, orLoad.ErrorMessage);
        var orDelta = global::OrchestrationReasoningSourceAdmissionTestFixture.Admit(
            new OrchestrationReasoningLearnedGuideStateDescriptor(
                StateId: "continued-or-dpo-authoring",
                GuideStateRef: "continued-or-dpo-authoring-guide",
                ManifestId: "continued-or-dpo-authoring-manifest",
                ManifestSha256: "sha256:continued-or-dpo-authoring",
                MachineType: nameof(OrchestrationReasoningMachine),
                SchemaVersion: "orchestration-reasoning-state-v1",
                SourceBaseCommit: "test-local-delta",
                StateSha256: "sha256:continued-or-dpo-authoring-state",
                LearnedPatterns:
                [
                    new OrchestrationReasoningLearnedPattern(
                        PatternId: "continued-dpo-authoring-orchestration",
                        ScenarioRef: "scenario:continued-dpo-authoring",
                        RequiredTaskTypes: ["adaptive_learnability_continued"],
                        RequiredConstraintSignals: ["user_local_model", "dpo_correction_row", "continue_training_required"],
                        RequiredRequirementSignals: ["build local DPO from correction"],
                        RequiredDeliverableSignals: ["continued adaptive DPO proof"],
                        RequiredEvidenceSignals: ["previous_checkpoint_loaded", "new_pair_added", "fresh_load_after_continue"],
                        PhaseFamilies: ["adaptive-continue-training"],
                        GateFamilies: ["previous-checkpoint-loaded", "fresh-load-after-continue"],
                        AssetFamilies: ["user-cache-continued"],
                        HostCheckSets: ["extension-api-core"],
                        RiskFlags: ["baseline_not_semantic_quality"],
                        NextGranularizationActions: ["keep semantic quality false"],
                        Confidence: 0.95)
                ]),
            "continued-or-dpo-authoring");
        var orContinue = orLoad.Value!.Machine.ContinueTrainingFromLearnedState(
            orDelta,
            orDelta.Receipt.PairIds);
        Assert.True(orContinue.IsSuccess, orContinue.ErrorMessage);
        var continuedOrPath = Path.Combine(
            continuedRoot,
            "orchestration-reasoning",
            "orchestration-reasoning-v1",
            "continued-checkpoint.json");
        var orSave = await orLoad.Value.Machine.SaveCheckpointAsync(
            continuedOrPath,
            orManifest);
        Assert.True(orSave.IsSuccess, orSave.ErrorMessage);

        var skillPath = Path.Combine(continuedRoot, "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var skillManifest = Path.Combine(continuedRoot, "skill-selection", "skill-selection-v1", "manifest.json");
        var skillMachine = new SkillSelectionMachine();
        var skillImport = await skillMachine.ImportAsync(new LearningMachineExport(
            SkillSelectionMachine.MachineId,
            SkillSelectionMachine.DomainTag,
            SkillSelectionMachine.Version,
            skillPath,
            skillManifest,
            0.0,
            DateTimeOffset.UtcNow));
        Assert.True(skillImport.IsSuccess, skillImport.ErrorMessage);
        var available = new[] { "adaptive-model", "dpo-authoring", "plan-review", "runtime-qa" };
        var train = await skillMachine.TrainMachineBatchAsync(
        [
            new SkillSelectionMachinePair(
                PairId: "continued-skill-pair",
                Request: new SkillSelectionRequest(
                    Prompt: "local model correction DPO backup share continue training",
                    ProjectRoot: "C:/Projects/llm_mvp_core",
                    AvailableSkillIds: available,
                    HostFacts: new Dictionary<string, string> { ["host"] = "codex", ["method"] = "skill-selection" },
                    UserPromptSourceEntryId: "continued-skill-pair"),
                Chosen: new SkillSelectionDecision(
                    SelectedSkillIds: ["dpo-authoring"],
                    RejectedSkillIds: ["adaptive-model", "plan-review", "runtime-qa"],
                    EvidenceFeatures: ["CONTROLLED_CONTINUE_TRAINING_DPO_CHOSEN"],
                    Confidence: 0.9,
                    RequiresSecondPass: false,
                    DecisionText: "Controlled continue-training DPO chosen row."),
                Rejected: new SkillSelectionDecision(
                    SelectedSkillIds: ["plan-review"],
                    RejectedSkillIds: ["dpo-authoring"],
                    EvidenceFeatures: ["CONTROLLED_CONTINUE_TRAINING_DPO_REJECTED"],
                    Confidence: 0.1,
                    RequiresSecondPass: true,
                    DecisionText: "Rejected row misses DPO authoring for correction learning."),
                RejectedType: "missing_dpo_authoring_skill",
                SourcePromptEntryId: "continued-skill-pair",
                TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter)
        ]);
        Assert.True(train.IsSuccess, train.ErrorMessage);
        var skillSave = await skillMachine.SaveCheckpointAsync(skillPath, skillManifest);
        Assert.True(skillSave.IsSuccess, skillSave.ErrorMessage);
    }

    private static XiigenExtensionRequest CreateRequest(string prompt, string correlationId)
        => new(
            HostName: "codex",
            HostType: "cli-agent",
            Method: XiigenExtensionApiContract.SkillSelectionMethod,
            UserPromptOrTaskText: prompt,
            HostFacts: new Dictionary<string, string> { ["workspace"] = "workspace:C:/Projects/llm_mvp_core", ["host_client"] = "thin" },
            DebugEnabled: true,
            RequestedSkills: ["adaptive-model", "dpo-authoring"],
            AvailableSkills: ["adaptive-model", "dpo-authoring", "plan-review", "runtime-qa"],
            CheckSetInputs: new Dictionary<string, string> { ["constraint.user_local_model"] = "required" },
            Provenance: new Dictionary<string, string> { ["source"] = "adaptive-learnability-01", ["user_adaptive_ref"] = "test-local" },
            CorrelationId: correlationId);

    private static void AssertNoOverclaim(XiigenExtensionResponse response)
    {
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Contains(response.Warnings, warning => warning.Contains("semantic_quality_claimed=false", StringComparison.Ordinal));
        Assert.DoesNotContain("SEMANTIC_QUALITY_CLAIMED_TRUE", string.Join("|", response.CheckSetResults.Values));
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (Directory.Exists(destinationDir))
            Directory.Delete(destinationDir, recursive: true);
        Directory.CreateDirectory(destinationDir);
        foreach (var directory in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destinationDir, Path.GetRelativePath(sourceDir, directory)));
        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destinationDir, Path.GetRelativePath(sourceDir, file)), overwrite: true);
    }

    private static Task CopyDirectoryAsync(string sourceDir, string destinationDir)
    {
        CopyDirectory(sourceDir, destinationDir);
        return Task.CompletedTask;
    }

    private static string BuildMarkdownReport(AdaptiveLearnabilityReport report)
        => $"""
           # ADAPTIVE-LEARNABILITY-01

           Scope: {report.Scope}

           - User/cache load first: {report.UserCacheLoadedFirst}
           - Repo/common fallback available: {report.RepoCommonFallbackAvailable}
           - Fresh-load changed behavior vs baseline: {report.FreshLoadChangedBehaviorVsBaseline}
           - Share import preserved behavior: {report.ShareImportPreservedBehavior}
           - Continue-training changed new scenario: {report.ContinueTrainingChangedNewScenario}
           - Backup artifact exists: {report.BackupArtifactExists}
           - Rollback artifact exists: {report.RollbackArtifactExists}

           Claims:
           - narrow user-local learnability allowed: {report.Claims.NarrowUserLocalLearnabilityAllowed}
           - semantic quality claimed: {report.Claims.SemanticQualityClaimed}
           - token savings claim allowed: {report.Claims.TokenSavingsClaimAllowed}
           - common model readiness claimed: {report.Claims.CommonModelReadinessClaimed}
           - target promotion done: {report.Claims.TargetPromotionDone}
           - locator updated: {report.Claims.LocatorUpdated}
           """;

    private sealed record AdaptiveLearnabilityReport(
        string Schema,
        DateTimeOffset GeneratedAtUtc,
        string Scope,
        string CommonRoot,
        string UserRoot,
        string BackupRoot,
        string ShareRoot,
        string RollbackRoot,
        string ContinuedRoot,
        bool UserCacheLoadedFirst,
        bool RepoCommonFallbackAvailable,
        bool FreshLoadChangedBehaviorVsBaseline,
        bool ShareImportPreservedBehavior,
        bool ContinueTrainingChangedNewScenario,
        bool BackupArtifactExists,
        bool RollbackArtifactExists,
        IReadOnlyList<string> DpoSemantics,
        AdaptiveLearnabilityClaims Claims);

    private sealed record AdaptiveLearnabilityClaims(
        bool NarrowUserLocalLearnabilityAllowed,
        bool SemanticQualityClaimed,
        bool TokenSavingsClaimAllowed,
        bool CommonModelReadinessClaimed,
        bool TargetPromotionDone,
        bool LocatorUpdated);
}
