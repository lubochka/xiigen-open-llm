// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure.Logical;
using LlmFromScratch.Tests;
using SmartSkillsDomain;

[Collection(global::ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionAdaptiveEffectGap02Tests
{
    private const string FailpointMessage = "ADAPTIVE_EFFECT_GAP_02_AFTER_OWNED_ASSETS";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions CompactJsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] RunnerAvailableSkills =
    [
        "architecture-audit",
        "authority-governance",
        "backup-export-import-share",
        "checkpoint-safety",
        "code-reference-grounding",
        "debug-session",
        "documentation-sync",
        "dpo-triple-format",
        "extension-runtime",
        "final-readiness-review",
        "held-out-ablation",
        "model-lifecycle",
        "plan-review",
        "planning-dpo-authoring",
        "privacy-redaction",
        "quality-review",
        "runtime-QA",
        "shortcut-audit",
        "skill-selection-invariance",
        "source-license-review",
        "test-integrity",
        "token-measurement",
        "trainable-unit-design"
    ];

    [Fact]
    public async Task ExtensionApi_UsesScenarioAlignedUserAdaptiveAssets_AndChangesToken01OutputsWithoutClaims()
    {
        var protectedRepoRoot = ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot;
        var before = CaptureProtectedRepositorySnapshot(protectedRepoRoot);
        string? ownedRoot = null;

        await TestOwnedOutputRoot.RunAsync(async output =>
        {
            ownedRoot = output.RootPath;
            var repoRoot = output.RepositoryRootPath;
            Assert.Equal(Path.GetFullPath(protectedRepoRoot), Path.GetFullPath(repoRoot));
            var staged = await StageOwnedScenarioAssetsAsync(output);
            var scenario = staged.Scenarios.First(row => row.Id == "TQ-007-skill-architecture-audit");
            var baseline = new XiigenExtensionApiService(repoRoot).ProcessHostExtensionRequest(
                CreateRequest(repoRoot, scenario, "baseline_common_model_only", "aeg02-baseline"));
            var adaptive = new XiigenExtensionApiService(repoRoot, staged.UserRoot).ProcessHostExtensionRequest(
                CreateRequest(repoRoot, scenario, "adaptive_common_plus_user_model", "aeg02-adaptive"));
            var shareImport = new XiigenExtensionApiService(repoRoot, staged.ShareRoot).ProcessHostExtensionRequest(
                CreateRequest(repoRoot, scenario, "adaptive_common_plus_user_model", "aeg02-share"));

            Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
            Assert.True(adaptive.IsSuccess, adaptive.ErrorMessage);
            Assert.True(shareImport.IsSuccess, shareImport.ErrorMessage);
            Assert.Equal("true", adaptive.Value!.CheckSetResults["user_adaptive_any_asset_loaded"]);
            Assert.Equal("user_cache_first_repo_common_fallback_second", adaptive.Value.CheckSetResults["user_adaptive_load_order"]);
            Assert.NotEqual(StableOutputProjection(baseline.Value!), StableOutputProjection(adaptive.Value));
            Assert.Equal(StableOutputProjection(adaptive.Value), StableOutputProjection(shareImport.Value!));
            Assert.Contains("architecture-audit", adaptive.Value.SelectedSkills);
            Assert.Contains("trainable-unit-design", adaptive.Value.SelectedSkills);
            Assert.Contains(adaptive.Value.StructuredTaskRefs, item => item.Contains("token-01-adaptive-effect", StringComparison.Ordinal));
            Assert.Contains(adaptive.Value.ScenarioRefs, item => item.Contains("token-01-adaptive-effect", StringComparison.Ordinal));
            AssertNoOverclaim(baseline.Value!);
            AssertNoOverclaim(adaptive.Value);

            var report = new
            {
                schema = "xiigen.adaptive_effect_gap_02.report.v1",
                generated_at_utc = DateTimeOffset.UtcNow,
                root_cause = "Previous user adaptive assets proved lifecycle on test-local prompts, but they were not aligned to TOKEN-01 scenario prompts; the capture also compared only ChatDisplayText, hiding API field changes.",
                scenario_count = staged.Scenarios.Count,
                dpo_rows_ref = output.ToStableReference(staged.DpoRowsPath),
                user_root = output.ToStableReference(staged.UserRoot),
                backup_root = output.ToStableReference(staged.BackupRoot),
                share_import_root = output.ToStableReference(staged.ShareRoot),
                rollback_root = output.ToStableReference(staged.RollbackRoot),
                adaptive_effect_probe = new
                {
                    scenario_id = scenario.Id,
                    baseline_projection_hash = Sha256(StableOutputProjection(baseline.Value!)),
                    adaptive_projection_hash = Sha256(StableOutputProjection(adaptive.Value)),
                    share_import_projection_hash = Sha256(StableOutputProjection(shareImport.Value!)),
                    adaptive_differs_from_baseline = StableOutputProjection(baseline.Value!) != StableOutputProjection(adaptive.Value),
                    share_import_preserves_adaptive_behavior = StableOutputProjection(adaptive.Value) == StableOutputProjection(shareImport.Value!)
                },
                claims = new
                {
                    token_savings_claim_allowed = false,
                    quality_improvement_claim_allowed = false,
                    semantic_quality_claimed = false,
                    full_learnability_claimed = false,
                    common_model_readiness_claimed = false,
                    target_promotion_done = false,
                    locator_updated = false
                },
                dpo_semantics = new[]
                {
                    "Rows store source ids, hashes, and chosen/rejected refs only; raw private prompt text is not stored in DPO rows.",
                    "Training uses universal TOKEN-01 scenario semantics and selected skill ids, not source-row id, path, candidate position, or exact prompt-to-answer lookup.",
                    "KNN/nearest overlap remains labelled baseline and cannot claim semantic quality."
                }
            };
            Assert.Equal("test-owned-output/adaptive-effect-gap-02/scenario-aligned-adaptive-dpo-rows.jsonl", report.dpo_rows_ref);
            Assert.Equal("test-owned-output/adaptive-effect-gap-02/user-cache", report.user_root);
            Assert.Equal("test-owned-output/adaptive-effect-gap-02/backup", report.backup_root);
            Assert.Equal("test-owned-output/adaptive-effect-gap-02/share-import", report.share_import_root);
            Assert.Equal("test-owned-output/adaptive-effect-gap-02/rollback", report.rollback_root);

            var reportPath = output.RequireOwnedPath(Path.Combine(staged.ArtifactRoot, "adaptive-effect-gap-02-report.json"));
            var reportMdPath = output.RequireOwnedPath(Path.Combine(staged.ArtifactRoot, "adaptive-effect-gap-02-report.md"));
            var validationPath = output.RequireOwnedPath(Path.Combine(staged.ArtifactRoot, "adaptive-effect-gap-02.validation.json"));
            var reportJson = JsonSerializer.Serialize(report, JsonOptions);
            var reportMarkdown = BuildMarkdownReport(report.root_cause, scenario.Id);
            var validationJson = JsonSerializer.Serialize(new
            {
                schema = "xiigen.adaptive_effect_gap_02.validation.v1",
                passed = true,
                scenario_count = staged.Scenarios.Count,
                dpo_row_count = File.ReadLines(staged.DpoRowsPath).Count(line => !string.IsNullOrWhiteSpace(line)),
                user_adaptive_any_asset_loaded = true,
                adaptive_projection_differs_from_baseline = true,
                backup_export_import_share = true,
                semantic_quality_claimed = false,
                token_savings_claim_allowed = false
            }, JsonOptions);
            AssertStableReportText(output, reportJson);
            AssertStableReportText(output, reportMarkdown);
            AssertStableReportText(output, validationJson);
            await WriteStableTextArtifactAsync(reportPath, reportJson);
            await WriteStableTextArtifactAsync(reportMdPath, reportMarkdown);
            await WriteStableTextArtifactAsync(validationPath, validationJson);
        });

        Assert.NotNull(ownedRoot);
        Assert.False(Directory.Exists(ownedRoot));
        Assert.Equal(before, CaptureProtectedRepositorySnapshot(protectedRepoRoot)); // This is the protected-repository immutability invariant for regular test runs.
    }

    [Fact]
    public async Task StagingFailure_CleansOwnedAssets_AndLeavesProtectedRepositoryInputsUnchanged()
    {
        var protectedRepoRoot = ExtensionRuntimeTokenQualityArtifactPaths.RepoRoot;
        var before = CaptureProtectedRepositorySnapshot(protectedRepoRoot);
        string? ownedRoot = null;
        var failpointCalls = 0;

        var failure = await Assert.ThrowsAsync<AdaptiveEffectGap02StagingFailpointException>(() =>
            TestOwnedOutputRoot.RunAsync(async output =>
            {
                ownedRoot = output.RootPath;
                var repoRoot = output.RepositoryRootPath;
                Assert.Equal(Path.GetFullPath(protectedRepoRoot), Path.GetFullPath(repoRoot));
                var missingSource = output.RequireOwnedPath(Path.Combine(output.RootPath, "copy-control-missing"));
                var protectedDestination = output.RequireOwnedPath(Path.Combine(output.RootPath, "copy-control-destination"));
                Directory.CreateDirectory(protectedDestination);
                var sentinel = output.RequireOwnedPath(Path.Combine(protectedDestination, "sentinel.txt"));
                File.WriteAllText(sentinel, "must-survive-missing-source");

                Assert.Throws<DirectoryNotFoundException>(() => CopyDirectory(output, missingSource, protectedDestination));
                Assert.Equal("must-survive-missing-source", File.ReadAllText(sentinel));

                var committedFixture = Path.Combine(
                    repoRoot,
                    "artifacts",
                    "extension-runtime-token-quality-learnability",
                    "adaptive-effect-gap-02");
                var immediatelyBeforeRejectedCopy = CaptureProtectedRepositorySnapshot(repoRoot);
                Assert.Throws<InvalidOperationException>(() => CopyDirectory(output, protectedDestination, committedFixture));
                Assert.Equal(immediatelyBeforeRejectedCopy, CaptureProtectedRepositorySnapshot(repoRoot));

                await StageOwnedScenarioAssetsAsync(output, () =>
                {
                    failpointCalls++;
                    ThrowAfterOwnedAssetsExistFailpoint();
                });
            }));

        Assert.Equal(FailpointMessage, failure.Message);
        Assert.Contains(nameof(ThrowAfterOwnedAssetsExistFailpoint), failure.StackTrace, StringComparison.Ordinal);
        Assert.Equal(1, failpointCalls);
        Assert.NotNull(ownedRoot);
        Assert.False(Directory.Exists(ownedRoot));
        Assert.Equal(before, CaptureProtectedRepositorySnapshot(protectedRepoRoot));
    }

    private static async Task<StagedAdaptiveEffectAssets> StageOwnedScenarioAssetsAsync(
        TestOwnedOutputRoot output,
        Action? afterOwnedAssetsExist = null)
    {
        var repoRoot = output.RepositoryRootPath;
        var scenarioPath = Path.Combine(
            repoRoot,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "scenario-pack-token-quality-v1.jsonl");
        var scenarios = ReadScenarioRows(scenarioPath);
        Assert.Equal(12, scenarios.Count);

        var artifactRoot = output.RequireOwnedPath(Path.Combine(output.RootPath, "adaptive-effect-gap-02"));
        var userRoot = output.RequireOwnedPath(Path.Combine(artifactRoot, "user-cache"));
        var backupRoot = output.RequireOwnedPath(Path.Combine(artifactRoot, "backup"));
        var shareRoot = output.RequireOwnedPath(Path.Combine(artifactRoot, "share-import"));
        var rollbackRoot = output.RequireOwnedPath(Path.Combine(artifactRoot, "rollback"));
        var dpoRowsPath = output.RequireOwnedPath(Path.Combine(artifactRoot, "scenario-aligned-adaptive-dpo-rows.jsonl"));
        Directory.CreateDirectory(artifactRoot);

        var promptToTask = BuildScenarioPromptToTaskMachine(scenarios);
        var orchestrationSourceRoot = output.RequireOwnedPath(Path.Combine(userRoot, "orchestration-reasoning", "orchestration-reasoning-v1", "source-admission")); // Orchestration proof sources must live under the checkpoint bundle so exported refs stay portable.
        var orchestration = BuildScenarioOrchestrationMachine(output, scenarios, orchestrationSourceRoot); // The adaptive scenario builder now feeds the exporter bundle-local source material.
        var skillSelection = BuildScenarioSkillSelectionMachine(scenarios);
        await WriteAssetSetAsync(output, userRoot, promptToTask, orchestration, skillSelection);
        await WriteScenarioAlignedDpoRowsAsync(output, dpoRowsPath, scenarios);
        CopyDirectory(output, userRoot, backupRoot);
        CopyDirectory(output, userRoot, shareRoot);
        CopyDirectory(output, userRoot, rollbackRoot);

        Assert.True(File.Exists(output.RequireOwnedPath(Path.Combine(
            userRoot,
            "prompt-to-task",
            "prompt-to-task-v1",
            "prompt-to-task-v1-checkpoint.json"))));
        Assert.True(File.Exists(output.RequireOwnedPath(Path.Combine(
            userRoot,
            "orchestration-reasoning",
            "orchestration-reasoning-v1",
            "checkpoint.json"))));
        Assert.True(File.Exists(output.RequireOwnedPath(Path.Combine(
            userRoot,
            "skill-selection",
            "skill-selection-v1",
            "skill-selection-v1-checkpoint.json"))));
        Assert.True(File.Exists(dpoRowsPath));
        Assert.True(Directory.Exists(backupRoot));
        Assert.True(Directory.Exists(shareRoot));
        Assert.True(Directory.Exists(rollbackRoot));
        afterOwnedAssetsExist?.Invoke();

        return new StagedAdaptiveEffectAssets(
            artifactRoot,
            userRoot,
            backupRoot,
            shareRoot,
            rollbackRoot,
            dpoRowsPath,
            scenarios);
    }

    private static void ThrowAfterOwnedAssetsExistFailpoint()
        => throw new AdaptiveEffectGap02StagingFailpointException(FailpointMessage);

    private static PromptToTaskMachine BuildScenarioPromptToTaskMachine(IReadOnlyList<ScenarioRow> scenarios)
    {
        var machine = new PromptToTaskMachine();
        var checkpoint = new PromptToTaskCheckpoint(
            SchemaVersion: 1,
            LearnedState: new PromptToTaskLearnedStateDescriptor(
                StateId: "adaptive-effect-gap-02-prompt-to-task",
                ManifestId: "adaptive-effect-gap-02-prompt-to-task-manifest",
                ManifestSha256: "sha256:adaptive-effect-gap-02-prompt-to-task-manifest",
                MachineType: nameof(PromptToTaskMachine),
                SchemaVersion: "prompt-to-task-state-v1",
                SourceBaseCommit: "token-01-scenario-pack",
                StateSha256: "sha256:adaptive-effect-gap-02-prompt-to-task-state",
                LearnedPatterns: scenarios.Select(BuildPromptPattern).ToArray()),
            AcceptedRefIds: scenarios.Select(row => $"aeg02-ptt-accepted:{row.Id}").ToArray(),
            SourceManifestRef: "scenario-pack-token-quality-v1",
            SemanticQualityClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);
        var import = machine.ImportLearnedState(checkpoint);
        Assert.True(import.IsSuccess, import.ErrorMessage);
        return machine;
    }

    private static PromptToTaskLearnedPattern BuildPromptPattern(ScenarioRow row)
        => new(
            PatternId: $"aeg02-ptt-{row.Id}",
            TaskType: $"token_01_{NormalizeId(row.Group)}",
            LearnedSignals: ExtractPromptSignals(row.PromptText),
            ExplicitUserRequirements: row.ExpectedOutputs.SelectMany(item => item.Value).Distinct(StringComparer.Ordinal).Take(6).ToArray(),
            Constraints:
            [
                "universal_extension_api_boundary",
                "no_semantic_quality_claim",
                "no_token_savings_claim",
                "token-01-adaptive-effect"
            ],
            Deliverables: [$"token-01 adaptive output for {row.Group}", "selected skills and scenario refs differ from baseline"],
            ForbiddenActions: ["semantic quality claim", "token savings claim", "common checkpoint promotion"],
            RequiredHosts: row.Hosts,
            EvidenceNeeds: ["user_adaptive_asset_loaded", "baseline_adaptive_projection_diff", "no_overclaim_scan"],
            SplitId: "token-01-adaptive-effect",
            Confidence: 0.95);

    private static AdmittedOrchestrationMachine BuildScenarioOrchestrationMachine(
        TestOwnedOutputRoot output, // The owned-output guard still validates every generated source-material path.
        IReadOnlyList<ScenarioRow> scenarios, // The twelve scenario rows remain the only learned-pattern source for this cache.
        string sourceAdmissionRoot) // The caller passes the future checkpoint bundle's source-admission directory.
    {
        OrchestrationReasoningAdmittedLearnedState? admitted = null;
        for (var index = 0; index < scenarios.Count; index++)
        {
            var row = scenarios[index];
            var corpusRoot = output.RequireOwnedPath(Path.Combine( // Each learned pattern receives its own portable proof-source directory.
                sourceAdmissionRoot, // Source material stays inside the orchestration checkpoint bundle expected by SaveCheckpointAsync.
                $"pattern-{index + 1:D2}")); // Stable pattern folders make the proof layout human-reviewable.
            output.RequireOwnedPath(Path.Combine(corpusRoot, "payload.jsonl"));
            output.RequireOwnedPath(Path.Combine(corpusRoot, "manifest.json"));
            output.RequireOwnedPath(Path.Combine(corpusRoot, "source-admission-ledger.json"));
            output.RequireOwnedPath(Path.Combine(corpusRoot, "source-admission-evidence.json"));
            var corpus = OrchestrationReasoningSourceAdmissionTestFixture.CreateCorpus(
                root: corpusRoot,
                pairId: $"aeg02-or-pair:{row.Id}",
                sourceRowId: $"aeg02-or-row:{row.Id}",
                sourceId: $"aeg02-or-source:{row.Id}",
                sourceRef: $"fixture://adaptive-effect-gap-02/{row.Id}",
                sourceSha256: OrchestrationReasoningSourceAdmissionTestFixture.Hash($"source:{row.Id}"),
                learnedPattern: BuildOrchestrationPattern(row));
            var bundleManifestRef = Path.Combine( // The learned-state manifest id must be portable after the committed fixture copy.
                "source-admission", // The source material root is inside the orchestration checkpoint bundle.
                $"pattern-{index + 1:D2}", // The pattern folder matches the staged source material directory.
                "manifest.json").Replace('\\', '/'); // The persisted reference is repo-platform-neutral.
            var materialized = new OrchestrationReasoningTrainer()
                .MaterializeAsync(
                    corpus.PayloadPath,
                    corpus.ManifestPath,
                    corpus.LedgerPath,
                    corpus.EvidencePath,
                    learnedStateManifestRef: bundleManifestRef)
                .GetAwaiter()
                .GetResult();
            Assert.True(materialized.IsSuccess, materialized.ErrorMessage);
            if (admitted is null)
            {
                admitted = materialized.Value!.AdmittedState; // The first pattern starts the merged state with portable provenance.
                continue;
            }

            var continuationMachine = new OrchestrationReasoningMachine();
            var imported = continuationMachine.ImportAdmittedState(admitted);
            Assert.True(imported.IsSuccess, imported.ErrorMessage);
            var continued = continuationMachine.ContinueTrainingFromLearnedState(
                materialized.Value!.AdmittedState, // Each delta pattern carries a bundle-local manifest id before merge.
                materialized.Value.AdmittedState.Receipt.PairIds); // The accepted refs stay bound to the same verified source-admission receipt.
            Assert.True(continued.IsSuccess, continued.ErrorMessage);
            admitted = continued.Value!.AdmittedState;
        }

        Assert.NotNull(admitted);
        var machine = new OrchestrationReasoningMachine();
        var import = machine.ImportAdmittedState(admitted);
        Assert.True(import.IsSuccess, import.ErrorMessage);
        return new AdmittedOrchestrationMachine(machine, admitted.State.ManifestId, admitted.Receipt.PairIds);
    }

    private static OrchestrationReasoningLearnedPattern BuildOrchestrationPattern(ScenarioRow row)
        => new(
            PatternId: $"aeg02-or-{row.Id}",
            ScenarioRef: $"scenario:token-01-adaptive-effect:{row.Id}",
            RequiredTaskTypes: [$"token_01_{NormalizeId(row.Group)}"],
            RequiredConstraintSignals: ["universal_extension_api_boundary", "no_semantic_quality_claim", "no_token_savings_claim", "token-01-adaptive-effect"],
            RequiredRequirementSignals: row.ExpectedOutputs.SelectMany(item => item.Value).Distinct(StringComparer.Ordinal).Take(6).ToArray(),
            RequiredDeliverableSignals: [$"token-01 adaptive output for {row.Group}", "selected skills and scenario refs differ from baseline"],
            RequiredEvidenceSignals: ["user_adaptive_asset_loaded", "baseline_adaptive_projection_diff", "no_overclaim_scan"],
            PhaseFamilies: [$"adaptive-effect-{NormalizeId(row.Group)}"],
            GateFamilies: ["user-cache-first", "projection-diff", "no-overclaim"],
            AssetFamilies: ["user-local-adaptive-cache", "repo-common-fallback"],
            HostCheckSets: ["host-boundary-from-request"],
            RiskFlags: ["baseline_not_semantic_quality", "no_token_savings_claim"],
            NextGranularizationActions: ["rerun token quality scoring with adaptive assets"],
            Confidence: 0.95);

    private static SkillSelectionMachine BuildScenarioSkillSelectionMachine(IReadOnlyList<ScenarioRow> scenarios)
    {
        var machine = new SkillSelectionMachine();
        var pairs = scenarios.Select(row => new SkillSelectionMachinePair(
            PairId: $"aeg02-skill-pair:{row.Id}",
            Request: new SkillSelectionRequest(
                Prompt: row.PromptText,
                ProjectRoot: "repo:llm_mvp_core",
                AvailableSkillIds: RunnerAvailableSkills,
                HostFacts: new Dictionary<string, string> { ["scenario_group"] = row.Group },
                UserPromptSourceEntryId: $"aeg02-source:{row.Id}"),
            Chosen: new SkillSelectionDecision(
                SelectedSkillIds: SelectScenarioSkills(row),
                RejectedSkillIds: RunnerAvailableSkills.Except(SelectScenarioSkills(row), StringComparer.Ordinal).ToArray(),
                EvidenceFeatures: ["CONTROLLED_SCENARIO_ALIGNED_USER_LOCAL_DPO_CHOSEN"],
                Confidence: 0.9,
                RequiresSecondPass: false,
                DecisionText: "Controlled TOKEN-01 user-local adaptive DPO chosen row."),
            Rejected: new SkillSelectionDecision(
                SelectedSkillIds: ["plan-review"],
                RejectedSkillIds: SelectScenarioSkills(row),
                EvidenceFeatures: ["CONTROLLED_SCENARIO_ALIGNED_USER_LOCAL_DPO_REJECTED"],
                Confidence: 0.1,
                RequiresSecondPass: true,
                DecisionText: "Rejected row omits required scenario-aligned skills."),
            RejectedType: "missing_token_01_scenario_skill_coverage",
            SourcePromptEntryId: $"aeg02-source:{row.Id}",
            TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter)).ToArray();
        var train = machine.TrainMachineBatchAsync(pairs).GetAwaiter().GetResult();
        Assert.True(train.IsSuccess, train.ErrorMessage);
        return machine;
    }

    private static string[] SelectScenarioSkills(ScenarioRow row)
    {
        var text = $"{row.Group} {row.PromptText}".ToLowerInvariant();
        var selected = new HashSet<string>(StringComparer.Ordinal);
        if (text.Contains("documentation") || text.Contains("quotes")) selected.Add("documentation-sync");
        if (text.Contains("runtime") || text.Contains("claude") || text.Contains("cursor") || text.Contains("codex")) selected.Add("extension-runtime");
        if (text.Contains("debug")) selected.Add("debug-session");
        if (text.Contains("privacy") || text.Contains("redacted")) selected.Add("privacy-redaction");
        if (text.Contains("token")) selected.Add("token-measurement");
        if (text.Contains("quality") || text.Contains("review")) selected.Add("quality-review");
        if (text.Contains("dpo") || text.Contains("learning") || text.Contains("corrected")) selected.Add("planning-dpo-authoring");
        if (text.Contains("checkpoint") || text.Contains("rollback")) selected.Add("checkpoint-safety");
        if (text.Contains("backup") || text.Contains("share") || text.Contains("import")) selected.Add("backup-export-import-share");
        if (text.Contains("architecture") || text.Contains("dictionary") || text.Contains("semantic learning")) selected.Add("architecture-audit");
        if (text.Contains("trainable")) selected.Add("trainable-unit-design");
        if (text.Contains("shortcut") || text.Contains("candidate order")) selected.Add("shortcut-audit");
        if (text.Contains("skill")) selected.Add("skill-selection-invariance");
        if (text.Contains("final readiness")) selected.Add("final-readiness-review");
        if (selected.Count == 0) selected.Add("plan-review");
        selected.Add("test-integrity");
        return selected.Order(StringComparer.Ordinal).ToArray();
    }

    private static async Task WriteAssetSetAsync(
        TestOwnedOutputRoot output,
        string root,
        PromptToTaskMachine promptToTask,
        AdmittedOrchestrationMachine orchestration,
        SkillSelectionMachine skillSelection)
    {
        root = output.RequireOwnedPath(root);
        var pttDir = output.RequireOwnedPath(Path.Combine(root, "prompt-to-task", "prompt-to-task-v1"));
        var orDir = output.RequireOwnedPath(Path.Combine(root, "orchestration-reasoning", "orchestration-reasoning-v1"));
        var skillDir = output.RequireOwnedPath(Path.Combine(root, "skill-selection", "skill-selection-v1"));
        var promptCheckpointPath = output.RequireOwnedPath(Path.Combine(pttDir, "prompt-to-task-v1-checkpoint.json"));
        var promptManifestPath = output.RequireOwnedPath(Path.Combine(pttDir, "manifest.json"));
        var orchestrationCheckpointPath = output.RequireOwnedPath(Path.Combine(orDir, "checkpoint.json"));
        var orchestrationManifestPath = output.RequireOwnedPath(Path.Combine(orDir, "manifest.json"));
        var skillCheckpointPath = output.RequireOwnedPath(Path.Combine(skillDir, "skill-selection-v1-checkpoint.json"));
        var skillManifestPath = output.RequireOwnedPath(Path.Combine(skillDir, "manifest.json"));

        Assert.True((await promptToTask.SaveCheckpointAsync(
            promptCheckpointPath,
            promptManifestPath,
            "scenario-pack-token-quality-v1",
            ["aeg02-ptt-accepted"])).IsSuccess);
        var orchestrationExport = await orchestration.Machine.SaveCheckpointAsync( // The production exporter writes checkpoint, manifest, and source-admission proof together.
            orchestrationCheckpointPath, // The checkpoint is the user-cache orchestration bundle target.
            orchestrationManifestPath, // The manifest sits beside the checkpoint so refs can remain relative.
            orchestration.SourceManifestRef, // The source manifest label is preserved from the admitted learned state.
            orchestration.AcceptedRefIds); // Accepted refs stay bound to the twelve scenario-derived orchestration patterns.
        Assert.True( // If exporter admission fails again, the test must show the exact exporter reason.
            orchestrationExport.IsSuccess, // Success means the bundle-local source materials were accepted by the exporter.
            $"{orchestrationExport.ErrorCode}: {orchestrationExport.ErrorMessage}"); // The assertion message carries the production error code for diagnosis.
        Assert.True((await skillSelection.SaveCheckpointAsync(
            skillCheckpointPath,
            skillManifestPath)).IsSuccess);
    }

    private static async Task WriteScenarioAlignedDpoRowsAsync(
        TestOwnedOutputRoot output,
        string path,
        IReadOnlyList<ScenarioRow> scenarios)
    {
        path = output.RequireOwnedPath(path);
        var parent = output.RequireOwnedPath(Path.GetDirectoryName(path)!);
        Directory.CreateDirectory(parent);
        var rows = scenarios.Select(row => new
        {
            schema = "xiigen.adaptive_effect_gap_02.dpo_row.v1",
            source_row_id = row.Id,
            source_prompt_sha256 = Sha256(row.PromptText),
            approved_pair_ref = $"token-01:{row.Id}:universal",
            chosen_ref = $"aeg02:chosen:{row.Id}",
            rejected_ref = $"aeg02:rejected:{row.Id}",
            error_code = "missing_scenario_aligned_user_adaptive_coverage",
            target_units = new[] { "PromptToTaskTaskPatternUnit", "OrchestrationReasoningScenarioUnit", "PromptContextSkillSelectionUnit" },
            selected_skill_ids = SelectScenarioSkills(row),
            quarantine = false,
            contains_raw_prompt_text = false,
            semantic_quality_claimed = false,
            common_checkpoint_mutation_claimed = false,
            universalization_note = row.UniversalizationRule
        });
        await File.WriteAllTextAsync(path, string.Join(Environment.NewLine, rows.Select(row => JsonSerializer.Serialize(row, CompactJsonOptions))) + Environment.NewLine);
    }

    private static XiigenExtensionRequest CreateRequest(
        string repoRoot,
        ScenarioRow row,
        string mode,
        string correlationId)
        => new(
            HostName: "Codex",
            HostType: "codex",
            Method: row.Methods.First(),
            UserPromptOrTaskText: row.PromptText,
            HostFacts: new Dictionary<string, string>
            {
                ["workspace"] = repoRoot,
                ["user_language"] = "en",
                ["host"] = "codex",
                ["source"] = "adaptive-effect-gap-02"
            },
            DebugEnabled: false,
            RequestedSkills: [],
            AvailableSkills: RunnerAvailableSkills,
            CheckSetInputs: new Dictionary<string, string>
            {
                ["semantic_quality_claimed"] = "false",
                ["deterministic_shortcut"] = "forbidden"
            },
            Provenance: new Dictionary<string, string>
            {
                ["scenario_id"] = row.Id,
                ["source_prompt_entry_id"] = $"adaptive-effect-gap-02:{row.Id}",
                ["mode"] = mode,
                ["user_adaptive_ref"] = "adaptive-effect-gap-02"
            },
            CorrelationId: correlationId);

    private static string OutputProjection(XiigenExtensionResponse response)
        => JsonSerializer.Serialize(new
        {
            response.ChatDisplayText,
            response.SelectedSkills,
            response.StructuredTaskRefs,
            response.ScenarioRefs,
            response.Warnings,
            response.CheckSetResults
        }, JsonOptions);

    private static string StableOutputProjection(XiigenExtensionResponse response)
        => JsonSerializer.Serialize(new
        {
            response.SelectedSkills,
            ScenarioRefs = response.ScenarioRefs.Select(NormalizeRunScopedRef).ToArray(),
            StructuredTaskRefs = response.StructuredTaskRefs.Select(NormalizeRunScopedRef).ToArray(),
            PromptToTaskStatus = response.CheckSetResults.GetValueOrDefault("prompt_to_task_status"),
            OrchestrationReasoningStatus = response.CheckSetResults.GetValueOrDefault("orchestration_reasoning_status"),
            SkillSelectionRequiresSecondPass = response.CheckSetResults.GetValueOrDefault("skill_selection_requires_second_pass"),
            SkillSelectionEvidence = response.CheckSetResults.GetValueOrDefault("skill_selection_evidence"),
            SemanticQualityClaimed = response.CheckSetResults.GetValueOrDefault("semantic_quality_claimed"),
            RuntimeExtensionClaimed = response.CheckSetResults.GetValueOrDefault("runtime_extension_claimed")
        }, JsonOptions);

    private static string NormalizeRunScopedRef(string value)
    {
        var parts = value.Split(':');
        return parts.Length >= 4 && string.Equals(parts[0], "structured-task", StringComparison.Ordinal)
            ? string.Join(':', parts[0], "<correlation>", parts[2], parts[3])
            : value;
    }

    private static IReadOnlyList<string> ExtractPromptSignals(string prompt)
        => prompt.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => new string(token.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray()))
            .Where(token => token.Length is >= 4 and <= 40)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(token => $"prompt:{token}")
            .ToArray();

    private static IReadOnlyList<ScenarioRow> ReadScenarioRows(string path)
        => File.ReadLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .Select(node => new ScenarioRow(
                Id: node["scenario_id"]!.GetValue<string>(),
                Group: node["scenario_group"]!.GetValue<string>(),
                PromptText: node["prompt_text"]!.GetValue<string>(),
                UniversalizationRule: node["universalization_rule"]!.GetValue<string>(),
                Hosts: node["hosts"]!.AsArray().Select(item => item!.GetValue<string>().ToLowerInvariant()).ToArray(),
                Methods: node["methods_to_run"]!.AsArray().Select(item => item!.GetValue<string>()).ToArray(),
                ExpectedOutputs: node["expected_outputs"]!.AsObject()
                    .ToDictionary(
                        item => item.Key,
                        item => (IReadOnlyList<string>)item.Value!.AsArray().Select(value => value!.GetValue<string>()).ToArray(),
                        StringComparer.Ordinal)))
            .ToArray();

    private static void AssertNoOverclaim(XiigenExtensionResponse response)
    {
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Equal("false", response.CheckSetResults["runtime_extension_claimed"]);
        Assert.DoesNotContain("SEMANTIC_QUALITY_CLAIMED_TRUE", string.Join("|", response.CheckSetResults.Values));
    }

    private static string BuildMarkdownReport(string rootCause, string probeScenarioId)
        => $"""
           # ADAPTIVE-EFFECT-GAP-02

           Root cause: {rootCause}

           Probe scenario: `{probeScenarioId}`.

           What changed:
           - built scenario-aligned user-local PromptToTask, OrchestrationReasoning, and SkillSelection adaptive assets from TOKEN-01 universal rows;
           - wrote ref-only DPO rows with chosen/rejected refs and no raw prompt text;
           - proved backup/share/import/rollback directories exist;
           - proved adaptive API output projection differs from baseline and share-import preserves adaptive behavior.

           What this does not prove:
           - token savings;
           - quality improvement;
           - semantic quality;
           - common model readiness;
           - target promotion or locator update.
           """;

    private static void AssertStableReportText(TestOwnedOutputRoot output, string text)
    {
        Assert.DoesNotContain(output.RootPath, text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFileName(output.RootPath), text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(output.RepositoryRootPath, text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFileName(output.RepositoryRootPath), text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetTempPath(), text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@":\", text, StringComparison.Ordinal);
        Assert.DoesNotContain(":/", text, StringComparison.Ordinal);
    }

    private static Task WriteStableTextArtifactAsync(string path, string text)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        return File.WriteAllTextAsync(path, normalized, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)); // Review artifacts stay byte-stable across Windows and Linux checkouts.
    }

    private static void CopyDirectory(
        TestOwnedOutputRoot output,
        string sourceDir,
        string destinationDir)
    {
        sourceDir = output.RequireOwnedPath(sourceDir);
        destinationDir = output.RequireOwnedPath(destinationDir);
        if (IsSameOrDescendant(sourceDir, destinationDir) || IsSameOrDescendant(destinationDir, sourceDir))
        {
            throw new InvalidOperationException(
                $"Copy source and destination must be disjoint test-owned directories: '{sourceDir}' -> '{destinationDir}'.");
        }

        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Copy source directory does not exist: {sourceDir}");

        var directories = Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories)
            .Select(source => new OwnedCopyEntry(
                output.RequireOwnedPath(source),
                output.RequireOwnedPath(Path.Combine(destinationDir, Path.GetRelativePath(sourceDir, source)))))
            .ToArray();
        var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)
            .Select(source => new OwnedCopyEntry(
                output.RequireOwnedPath(source),
                output.RequireOwnedPath(Path.Combine(destinationDir, Path.GetRelativePath(sourceDir, source)))))
            .ToArray();

        if (Directory.Exists(destinationDir))
            Directory.Delete(destinationDir, recursive: true);
        Directory.CreateDirectory(destinationDir);
        foreach (var directory in directories)
            Directory.CreateDirectory(directory.DestinationPath);
        foreach (var file in files)
            File.Copy(file.SourcePath, file.DestinationPath, overwrite: true);
    }

    private static bool IsSameOrDescendant(string candidate, string root)
    {
        var fullCandidate = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate));
        var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(fullCandidate, fullRoot, comparison)
            || fullCandidate.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison);
    }

    private static ProtectedRepositoryEntry[] CaptureProtectedRepositorySnapshot(string repoRoot)
    {
        repoRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(repoRoot));
        var fixtureRoot = Path.Combine(
            repoRoot,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "adaptive-effect-gap-02");
        var scenarioPack = Path.Combine(
            repoRoot,
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "scenario-pack-token-quality-v1.jsonl");
        var modelRoots = new[]
        {
            Path.Combine(repoRoot, "models", "prompt-to-task"),
            Path.Combine(repoRoot, "models", "orchestration-reasoning"),
            Path.Combine(repoRoot, "models", "skill-selection")
        };

        Assert.True(Directory.Exists(fixtureRoot), $"Missing committed fixture: {fixtureRoot}");
        Assert.Equal(224, Directory.GetFiles(fixtureRoot, "*", SearchOption.AllDirectories).Length); // The committed fixture count includes schema-v2 orchestration proofs in user, backup, share, and rollback caches.
        Assert.True(File.Exists(scenarioPack), $"Missing committed scenario pack: {scenarioPack}");
        Assert.All(modelRoots, root => Assert.True(Directory.Exists(root), $"Missing committed model tree: {root}"));

        var entries = new List<ProtectedRepositoryEntry>();
        AddDirectorySnapshot(repoRoot, fixtureRoot, entries);
        AddFileSnapshot(repoRoot, scenarioPack, entries);
        foreach (var modelRoot in modelRoots)
            AddDirectorySnapshot(repoRoot, modelRoot, entries);

        return entries
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ThenBy(entry => entry.EntryKind, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddDirectorySnapshot(
        string repoRoot,
        string directory,
        ICollection<ProtectedRepositoryEntry> entries)
    {
        AddDirectoryEntry(repoRoot, directory, entries);
        foreach (var childDirectory in Directory.GetDirectories(directory, "*", SearchOption.AllDirectories))
            AddDirectoryEntry(repoRoot, childDirectory, entries);
        foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            AddFileSnapshot(repoRoot, file, entries);
    }

    private static void AddDirectoryEntry(
        string repoRoot,
        string directory,
        ICollection<ProtectedRepositoryEntry> entries)
    {
        Assert.Equal(0, (File.GetAttributes(directory) & FileAttributes.ReparsePoint) == 0 ? 0 : 1);
        entries.Add(new ProtectedRepositoryEntry(
            RepositoryRelativePath(repoRoot, directory),
            "directory",
            0,
            string.Empty));
    }

    private static void AddFileSnapshot(
        string repoRoot,
        string file,
        ICollection<ProtectedRepositoryEntry> entries)
    {
        Assert.Equal(0, (File.GetAttributes(file) & FileAttributes.ReparsePoint) == 0 ? 0 : 1);
        using var stream = File.OpenRead(file);
        var hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        entries.Add(new ProtectedRepositoryEntry(
            RepositoryRelativePath(repoRoot, file),
            "file",
            stream.Length,
            hash));
    }

    private static string RepositoryRelativePath(string repoRoot, string path)
    {
        var relative = Path.GetRelativePath(repoRoot, Path.GetFullPath(path)).Replace('\\', '/');
        Assert.False(relative == ".." || relative.StartsWith("../", StringComparison.Ordinal));
        return relative;
    }

    private static string Sha256(string text)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static string NormalizeId(string value)
        => new(value.Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '_').ToArray());

    private sealed record ScenarioRow(
        string Id,
        string Group,
        string PromptText,
        string UniversalizationRule,
        IReadOnlyList<string> Hosts,
        IReadOnlyList<string> Methods,
        IReadOnlyDictionary<string, IReadOnlyList<string>> ExpectedOutputs);

    private sealed record StagedAdaptiveEffectAssets(
        string ArtifactRoot,
        string UserRoot,
        string BackupRoot,
        string ShareRoot,
        string RollbackRoot,
        string DpoRowsPath,
        IReadOnlyList<ScenarioRow> Scenarios);

    private sealed record AdmittedOrchestrationMachine(
        OrchestrationReasoningMachine Machine,
        string SourceManifestRef,
        IReadOnlyList<string> AcceptedRefIds);

    private sealed record OwnedCopyEntry(string SourcePath, string DestinationPath);

    private sealed record ProtectedRepositoryEntry(
        string RelativePath,
        string EntryKind,
        long FileLength,
        string Sha256);

    private sealed class AdaptiveEffectGap02StagingFailpointException(string message) : Exception(message);
}
