// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class ExtensionAdaptiveEffectGap02Tests
{
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
        var repoRoot = FindRepoRoot();
        var artifactRoot = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability", "adaptive-effect-gap-02");
        if (Directory.Exists(artifactRoot))
            Directory.Delete(artifactRoot, recursive: true);
        Directory.CreateDirectory(artifactRoot);

        var scenarioPath = Path.Combine(repoRoot, "artifacts", "extension-runtime-token-quality-learnability", "scenario-pack-token-quality-v1.jsonl");
        var scenarios = ReadScenarioRows(scenarioPath);
        Assert.Equal(12, scenarios.Count);

        var userRoot = Path.Combine(artifactRoot, "user-cache");
        var backupRoot = Path.Combine(artifactRoot, "backup");
        var shareRoot = Path.Combine(artifactRoot, "share-import");
        var rollbackRoot = Path.Combine(artifactRoot, "rollback");

        var promptToTask = BuildScenarioPromptToTaskMachine(scenarios);
        var orchestration = BuildScenarioOrchestrationMachine(scenarios);
        var skillSelection = BuildScenarioSkillSelectionMachine(scenarios);
        await WriteAssetSetAsync(userRoot, promptToTask, orchestration, skillSelection);

        var dpoRowsPath = Path.Combine(artifactRoot, "scenario-aligned-adaptive-dpo-rows.jsonl");
        await WriteScenarioAlignedDpoRowsAsync(dpoRowsPath, scenarios);

        CopyDirectory(userRoot, backupRoot);
        CopyDirectory(userRoot, shareRoot);
        CopyDirectory(userRoot, rollbackRoot);

        var commonRoot = repoRoot;
        var scenario = scenarios.First(row => row.Id == "TQ-007-skill-architecture-audit");
        var baseline = new XiigenExtensionApiService(commonRoot).ProcessHostExtensionRequest(CreateRequest(scenario, "baseline_common_model_only", "aeg02-baseline"));
        var adaptive = new XiigenExtensionApiService(commonRoot, userRoot).ProcessHostExtensionRequest(CreateRequest(scenario, "adaptive_common_plus_user_model", "aeg02-adaptive"));
        var shareImport = new XiigenExtensionApiService(commonRoot, shareRoot).ProcessHostExtensionRequest(CreateRequest(scenario, "adaptive_common_plus_user_model", "aeg02-share"));

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
            scenario_count = scenarios.Count,
            dpo_rows_ref = dpoRowsPath,
            user_root = userRoot,
            backup_root = backupRoot,
            share_import_root = shareRoot,
            rollback_root = rollbackRoot,
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
        var reportPath = Path.Combine(artifactRoot, "adaptive-effect-gap-02-report.json");
        var reportMdPath = Path.Combine(artifactRoot, "adaptive-effect-gap-02-report.md");
        var validationPath = Path.Combine(artifactRoot, "adaptive-effect-gap-02.validation.json");
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, JsonOptions));
        await File.WriteAllTextAsync(reportMdPath, BuildMarkdownReport(report.root_cause, scenario.Id));
        await File.WriteAllTextAsync(validationPath, JsonSerializer.Serialize(new
        {
            schema = "xiigen.adaptive_effect_gap_02.validation.v1",
            passed = true,
            scenario_count = scenarios.Count,
            dpo_row_count = File.ReadLines(dpoRowsPath).Count(line => !string.IsNullOrWhiteSpace(line)),
            user_adaptive_any_asset_loaded = true,
            adaptive_projection_differs_from_baseline = true,
            backup_export_import_share = true,
            semantic_quality_claimed = false,
            token_savings_claim_allowed = false
        }, JsonOptions));
    }

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

    private static OrchestrationReasoningMachine BuildScenarioOrchestrationMachine(IReadOnlyList<ScenarioRow> scenarios)
    {
        var machine = new OrchestrationReasoningMachine();
        var checkpoint = new OrchestrationReasoningCheckpoint(
            SchemaVersion: 1,
            LearnedState: new OrchestrationReasoningLearnedGuideStateDescriptor(
                StateId: "adaptive-effect-gap-02-orchestration",
                GuideStateRef: "adaptive-effect-gap-02-orchestration-guide",
                ManifestId: "adaptive-effect-gap-02-orchestration-manifest",
                ManifestSha256: "sha256:adaptive-effect-gap-02-orchestration-manifest",
                MachineType: nameof(OrchestrationReasoningMachine),
                SchemaVersion: "orchestration-reasoning-state-v1",
                SourceBaseCommit: "token-01-scenario-pack",
                StateSha256: "sha256:adaptive-effect-gap-02-orchestration-state",
                LearnedPatterns: scenarios.Select(BuildOrchestrationPattern).ToArray()),
            AcceptedRefIds: scenarios.Select(row => $"aeg02-or-accepted:{row.Id}").ToArray(),
            SourceManifestRef: "scenario-pack-token-quality-v1",
            SemanticQualityClaimed: false,
            ContinueTrainingClaimed: false,
            HostRuntimeClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);
        var import = machine.ImportLearnedState(checkpoint);
        Assert.True(import.IsSuccess, import.ErrorMessage);
        return machine;
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
            HostCheckSets: row.Hosts.Select(host => $"host:{host.ToLowerInvariant()}").ToArray(),
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
        string root,
        PromptToTaskMachine promptToTask,
        OrchestrationReasoningMachine orchestration,
        SkillSelectionMachine skillSelection)
    {
        var pttDir = Path.Combine(root, "prompt-to-task", "prompt-to-task-v1");
        var orDir = Path.Combine(root, "orchestration-reasoning", "orchestration-reasoning-v1");
        var skillDir = Path.Combine(root, "skill-selection", "skill-selection-v1");

        Assert.True((await promptToTask.SaveCheckpointAsync(
            Path.Combine(pttDir, "prompt-to-task-v1-checkpoint.json"),
            Path.Combine(pttDir, "manifest.json"),
            "scenario-pack-token-quality-v1",
            ["aeg02-ptt-accepted"])).IsSuccess);
        Assert.True((await orchestration.SaveCheckpointAsync(
            Path.Combine(orDir, "checkpoint.json"),
            Path.Combine(orDir, "manifest.json"),
            "scenario-pack-token-quality-v1",
            ["aeg02-or-accepted"])).IsSuccess);
        Assert.True((await skillSelection.SaveCheckpointAsync(
            Path.Combine(skillDir, "skill-selection-v1-checkpoint.json"),
            Path.Combine(skillDir, "manifest.json"))).IsSuccess);
    }

    private static async Task WriteScenarioAlignedDpoRowsAsync(string path, IReadOnlyList<ScenarioRow> scenarios)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
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

    private static XiigenExtensionRequest CreateRequest(ScenarioRow row, string mode, string correlationId)
        => new(
            HostName: "Codex",
            HostType: "codex",
            Method: row.Methods.First(),
            UserPromptOrTaskText: row.PromptText,
            HostFacts: new Dictionary<string, string>
            {
                ["workspace"] = FindRepoRoot(),
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

    private static string Sha256(string text)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static string NormalizeId(string value)
        => new(value.Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '_').ToArray());

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "43.SmartSkillsDomain"))
                && Directory.Exists(Path.Combine(current.FullName, "artifacts")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private sealed record ScenarioRow(
        string Id,
        string Group,
        string PromptText,
        string UniversalizationRule,
        IReadOnlyList<string> Hosts,
        IReadOnlyList<string> Methods,
        IReadOnlyDictionary<string, IReadOnlyList<string>> ExpectedOutputs);
}
