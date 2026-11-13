// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using SmartSkillsDomain;

public sealed class ExtensionQualityGapRepairTests
{
    [Fact]
    public void XiigenExtensionApiService_PassesUserPromptTextIntoTrainableMachines()
    {
        var service = CreateScenarioService();
        var request = CreateRequest(
            "TQ-001-prompt-task-documentation",
            "Turn this request into a universal task: create human-readable documentation for an extension API with a simple diagram, no-code algorithms, short code quotations, and clear boundaries about what the evidence proves.",
            XiigenExtensionApiContract.PromptToTaskMethod);

        var result = service.ProcessHostExtensionRequest(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.True(response.Success);
        Assert.NotEqual(PromptToTaskMachine.NoMatchingPatternStatus, response.CheckSetResults["prompt_to_task_status"]);
        Assert.NotEqual(PromptToTaskMachine.NotTrainedStatus, response.CheckSetResults["prompt_to_task_status"]);
        Assert.DoesNotContain("NO_MATCHING_LEARNED_PATTERN", response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Contains("[redacted]", response.DebugTrace!.InputPayloadText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.UserPromptOrTaskText, response.DebugTrace.InputPayloadText, StringComparison.Ordinal);
    }

    [Fact]
    public void XiigenExtensionApiService_ReportsLegacyOrchestrationReasoningAssetNotLoaded()
    {
        var service = new XiigenExtensionApiService(FindRepoRoot());
        var request = CreateRequest(
            "TQ-004-orchestration-runtime-proof",
            "Build a universal orchestration plan to prove an extension works in Claude, Cursor, and Codex host runtime paths, with visible answer, debug trace, correlation id, negative error path, and no hidden logic inside host adapters.",
            XiigenExtensionApiContract.OrchestrationPlanMethod);

        var result = service.ProcessHostExtensionRequest(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.True(response.Success);
        Assert.Equal("false", response.CheckSetResults["orchestration_reasoning_asset_loaded"]);
        Assert.Contains(
            response.Warnings,
            warning => warning.Contains(
                "ORCHESTRATION_REASONING_CHECKPOINT_SOURCE_PROVENANCE_REBUILD_REQUIRED",
                StringComparison.Ordinal));
        Assert.Equal(
            OrchestrationReasoningMachine.NotTrainedStatus,
            response.CheckSetResults["orchestration_reasoning_status"]);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
    }

    [Fact]
    public void XiigenExtensionApiService_SkillSelectionUsesPromptTextAndNotCandidatePositionAuthority()
    {
        var service = CreateScenarioService();
        var request = CreateRequest(
            "TQ-008-skill-debug-runtime",
            "Select skills for a task that asks the extension to show programmer debug input/output, redacted shared traces, controlled error handling, and host runtime proof for Claude, Cursor, and Codex.",
            XiigenExtensionApiContract.SkillSelectionMethod);

        var result = service.ProcessHostExtensionRequest(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.True(response.Success);
        Assert.NotEmpty(response.SelectedSkills);
        Assert.Equal("false", response.CheckSetResults["skill_selection_requires_second_pass"]);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_position_suggestion=", response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
    }

    [Fact]
    public void XiigenExtensionApiService_LoadsRepoCommonSkillSelectionAssetWithoutSemanticOverclaim()
    {
        var service = new XiigenExtensionApiService(FindRepoRoot());
        var request = CreateRequest(
            "TQ-008-skill-debug-runtime",
            "Select skills for a task that asks the extension to show programmer debug input/output, redacted shared traces, controlled error handling, and host runtime proof for Claude, Cursor, and Codex.",
            XiigenExtensionApiContract.SkillSelectionMethod);

        var result = service.ProcessHostExtensionRequest(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.True(response.Success);
        Assert.True(
            response.CheckSetResults["skill_selection_asset_loaded"] == "true",
            string.Join(Environment.NewLine, response.Warnings));
        Assert.Equal("false", response.CheckSetResults["all_common_assets_loaded"]);
        Assert.NotEmpty(response.SelectedSkills);
        Assert.Equal("false", response.CheckSetResults["skill_selection_requires_second_pass"]);
        Assert.DoesNotContain("SKILL_SELECTION_COMMON_CHECKPOINT_READ_FAILED", response.Warnings);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, response.CheckSetResults["skill_selection_evidence"], StringComparison.Ordinal);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
        Assert.Contains("semantic_quality_claimed=false", response.Warnings);
    }

    [Fact]
    public void XiigenExtensionApiService_LoadsRepoCommonSkillSelectionCheckpointFromManifestFilename()
    {
        using var tempRoot = CreateTempCommonRootWithRenamedSkillSelectionCheckpoint();
        var service = new XiigenExtensionApiService(tempRoot.RootPath);
        var request = CreateRequest(
            "TQ-008-skill-debug-runtime",
            "Select skills for a task that asks the extension to show programmer debug input/output, redacted shared traces, controlled error handling, and host runtime proof for Claude, Cursor, and Codex.",
            XiigenExtensionApiContract.SkillSelectionMethod);

        var result = service.ProcessHostExtensionRequest(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var response = result.Value!;
        Assert.True(response.Success);
        Assert.Equal("true", response.CheckSetResults["skill_selection_asset_loaded"]);
        Assert.EndsWith("renamed-skill-selection-checkpoint.json", response.CheckSetResults["skill_selection_checkpoint_ref"], StringComparison.Ordinal);
        Assert.NotEmpty(response.SelectedSkills);
        Assert.Equal("false", response.CheckSetResults["skill_selection_requires_second_pass"]);
        Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
    }

    [Fact]
    public void XiigenExtensionApiService_Token01ScenarioSampleHasUsableOutputsWithoutQualityOverclaim()
    {
        var service = CreateScenarioService();
        var rows = ReadScenarioRows()
            .Where(row => row.ScenarioId is
                "TQ-001-prompt-task-documentation" or
                "TQ-004-orchestration-runtime-proof" or
                "TQ-008-skill-debug-runtime" or
                "TQ-012-mixed-final-readiness")
            .ToArray();

        Assert.Equal(4, rows.Length);
        foreach (var row in rows)
        {
            var request = CreateRequest(row.ScenarioId, row.PromptText, row.MethodsToRun.First());
            var result = service.ProcessHostExtensionRequest(request);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var response = result.Value!;
            Assert.True(response.Success);
            Assert.NotEqual(PromptToTaskMachine.NotTrainedStatus, response.CheckSetResults["prompt_to_task_status"]);
            Assert.NotEqual(PromptToTaskMachine.NoMatchingPatternStatus, response.CheckSetResults["prompt_to_task_status"]);
            Assert.NotEqual(OrchestrationReasoningMachine.NotTrainedStatus, response.CheckSetResults["orchestration_reasoning_status"]);
            Assert.NotEmpty(response.SelectedSkills);
            Assert.Equal("false", response.CheckSetResults["skill_selection_requires_second_pass"]);
            Assert.Equal("false", response.CheckSetResults["semantic_quality_claimed"]);
            Assert.Contains("semantic_quality_claimed=false", response.Warnings);
        }
    }

    private static XiigenExtensionApiService CreateScenarioService()
    {
        var promptToTask = CreatePromptToTaskMachineWithScenarioBaseline();
        var orchestration = CreateOrchestrationReasoningMachineWithScenarioBaseline();
        var skillSelection = CreateSkillSelectionMachineWithScenarioBaseline();
        return new XiigenExtensionApiService(promptToTask, orchestration, skillSelection);
    }

    private static PromptToTaskMachine CreatePromptToTaskMachineWithScenarioBaseline()
    {
        var machine = new PromptToTaskMachine();
        var checkpoint = new PromptToTaskCheckpoint(
            SchemaVersion: 1,
            LearnedState: new PromptToTaskLearnedStateDescriptor(
                StateId: "quality-gap-repair-01-ptt-state",
                ManifestId: "quality-gap-repair-01-ptt-manifest",
                ManifestSha256: "sha256:quality-gap-repair-01-ptt-manifest",
                MachineType: "PromptToTaskMachine",
                SchemaVersion: "prompt-to-task-state-v1",
                SourceBaseCommit: "commit:test-local-quality-gap-repair-01",
                StateSha256: "sha256:quality-gap-repair-01-ptt-state",
                LearnedPatterns:
                [
                    new PromptToTaskLearnedPattern(
                        PatternId: "quality-gap-repair-01-extension-readiness",
                        TaskType: "universal_extension_readiness",
                        LearnedSignals:
                        [
                            "prompt:documentation",
                            "prompt:extension",
                            "prompt:runtime",
                            "prompt:debug",
                            "prompt:readiness",
                            "prompt:automation"
                        ],
                        ExplicitUserRequirements:
                        [
                            "turn the user prompt into a universal extension-readiness task",
                            "keep stack-specific implementation details out of the universal task"
                        ],
                        Constraints:
                        [
                            "universal_only",
                            "host_runtime_evidence_required",
                            "debug_trace_required",
                            "quality_claims_must_remain_false_until_gates_pass"
                        ],
                        Deliverables:
                        [
                            "human-readable task contract",
                            "extension runtime proof plan",
                            "debug trace and negative-path evidence"
                        ],
                        ForbiddenActions:
                        [
                            "claim semantic quality from baseline overlap",
                            "use candidate position as skill authority"
                        ],
                        RequiredHosts: ["claude", "cursor", "codex"],
                        EvidenceNeeds:
                        [
                            "visible answer evidence",
                            "redacted input output trace",
                            "quality gate result"
                        ],
                        SplitId: "quality-gap-repair-01-test-local",
                        Confidence: 0.9)
                ]),
            AcceptedRefIds: ["quality-gap-repair-01-ptt-accepted"],
            SourceManifestRef: "quality-gap-repair-01-test-local-source",
            SemanticQualityClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        var imported = machine.ImportLearnedState(checkpoint);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return machine;
    }

    private static OrchestrationReasoningMachine CreateOrchestrationReasoningMachineWithScenarioBaseline()
    {
        var machine = new OrchestrationReasoningMachine();
        var checkpoint = new OrchestrationReasoningCheckpoint(
            SchemaVersion: 1,
            LearnedState: new OrchestrationReasoningLearnedGuideStateDescriptor(
                StateId: "quality-gap-repair-01-or-state",
                GuideStateRef: "quality-gap-repair-01-or-guide",
                ManifestId: "quality-gap-repair-01-or-manifest",
                ManifestSha256: "sha256:quality-gap-repair-01-or-manifest",
                MachineType: "OrchestrationReasoningMachine",
                SchemaVersion: "orchestration-reasoning-state-v1",
                SourceBaseCommit: "commit:test-local-quality-gap-repair-01",
                StateSha256: "sha256:quality-gap-repair-01-or-state",
                LearnedPatterns:
                [
                    new OrchestrationReasoningLearnedPattern(
                        PatternId: "quality-gap-repair-01-extension-readiness-plan",
                        ScenarioRef: "scenario:quality-gap-repair-01-extension-readiness",
                        RequiredTaskTypes: ["universal_extension_readiness"],
                        RequiredConstraintSignals:
                        [
                            "universal_only",
                            "host_runtime_evidence_required",
                            "debug_trace_required",
                            "quality_claims_must_remain_false_until_gates_pass"
                        ],
                        RequiredRequirementSignals:
                        [
                            "turn the user prompt into a universal extension-readiness task",
                            "keep stack-specific implementation details out of the universal task"
                        ],
                        RequiredDeliverableSignals:
                        [
                            "human-readable task contract",
                            "extension runtime proof plan",
                            "debug trace and negative-path evidence"
                        ],
                        RequiredEvidenceSignals:
                        [
                            "visible answer evidence",
                            "redacted input output trace",
                            "quality gate result"
                        ],
                        PhaseFamilies: ["extension-runtime-quality"],
                        GateFamilies: ["no-overclaim", "host-runtime-proof-required"],
                        AssetFamilies: ["llm_mvp_core-test-local"],
                        HostCheckSets: ["host-boundary-from-request"],
                        RiskFlags: ["baseline_not_semantic_quality"],
                        NextGranularizationActions: ["rerun TOKEN-04 scoring before any readiness claim"],
                        Confidence: 0.9)
                ]),
            AcceptedRefIds: ["quality-gap-repair-01-or-accepted"],
            SourceManifestRef: "quality-gap-repair-01-test-local-source",
            SemanticQualityClaimed: false,
            ContinueTrainingClaimed: false,
            HostRuntimeClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        var admitted = OrchestrationReasoningSourceAdmissionTestFixture.Admit(
            checkpoint.LearnedState.LearnedPatterns!,
            checkpoint.LearnedState.StateId,
            checkpoint.LearnedState.StateId);
        var imported = machine.ImportAdmittedState(admitted);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return machine;
    }

    private static SkillSelectionMachine CreateSkillSelectionMachineWithScenarioBaseline()
    {
        var machine = new SkillSelectionMachine();
        var prompts = new[]
        {
            "Turn this request into a universal task: create human-readable documentation for an extension API with a simple diagram, no-code algorithms, short code quotations, and clear boundaries about what the evidence proves.",
            "Build a universal orchestration plan to prove an extension works in Claude, Cursor, and Codex host runtime paths, with visible answer, debug trace, correlation id, negative error path, and no hidden logic inside host adapters.",
            "Select skills for a task that asks the extension to show programmer debug input/output, redacted shared traces, controlled error handling, and host runtime proof for Claude, Cursor, and Codex.",
            "Check final readiness for extension runtime, token comparison, quality scoring, local adaptive learning, backup/export/import, fresh-load, continue-training, and no overclaim."
        };
        var pairs = prompts
            .Select((prompt, index) => CreateSkillSelectionPair(prompt, $"quality-gap-repair-01-skill-{index}"))
            .ToArray();
        var trained = machine.TrainMachineBatchAsync(pairs, CancellationToken.None).GetAwaiter().GetResult();
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        return machine;
    }

    private static SkillSelectionMachinePair CreateSkillSelectionPair(string prompt, string pairId)
    {
        var request = new SkillSelectionRequest(
            Prompt: prompt,
            ProjectRoot: FindRepoRoot(),
            AvailableSkillIds:
            [
                "documentation-sync",
                "extension-runtime",
                "runtime-QA",
                "debug-session",
                "quality-review",
                "privacy-redaction",
                "plan-review",
                "skill-selection-invariance"
            ],
            HostFacts: new Dictionary<string, string>
            {
                ["host"] = "codex",
                ["method"] = "quality-gap-repair"
            },
            UserPromptSourceEntryId: pairId);
        var chosen = new SkillSelectionDecision(
            SelectedSkillIds:
            [
                "extension-runtime",
                "runtime-QA",
                "debug-session",
                "quality-review",
                "documentation-sync"
            ],
            RejectedSkillIds:
            [
                "privacy-redaction",
                "plan-review",
                "skill-selection-invariance"
            ],
            EvidenceFeatures: ["test-local-quality-gap-repair-01"],
            Confidence: 0.8,
            RequiresSecondPass: false,
            DecisionText: "test-local prompt_context baseline chosen skills");
        var rejected = chosen with
        {
            SelectedSkillIds = ["skill-selection-invariance"],
            RejectedSkillIds = ["extension-runtime", "runtime-QA", "debug-session", "quality-review", "documentation-sync"],
            Confidence = 0.2,
            RequiresSecondPass = true,
            DecisionText = "test-local rejected empty/second-pass boundary"
        };
        return new SkillSelectionMachinePair(
            PairId: pairId,
            Request: request,
            Chosen: chosen,
            Rejected: rejected,
            RejectedType: "empty_or_second_pass_skill_selection",
            SourcePromptEntryId: pairId,
            TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter);
    }

    private static XiigenExtensionRequest CreateRequest(string scenarioId, string promptText, string method)
        => new(
            HostName: "Codex",
            HostType: "codex",
            Method: method,
            UserPromptOrTaskText: promptText,
            HostFacts: new Dictionary<string, string>
            {
                ["workspace"] = FindRepoRoot(),
                ["user_language"] = "en",
                ["host"] = "codex",
                ["source"] = "token-01-quality-gap-repair-test"
            },
            DebugEnabled: true,
            RequestedSkills: [],
            AvailableSkills:
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
            ],
            CheckSetInputs: new Dictionary<string, string>
            {
                ["semantic_quality_claimed"] = "false",
                ["deterministic_shortcut"] = "forbidden"
            },
            Provenance: new Dictionary<string, string>
            {
                ["scenario_id"] = scenarioId,
                ["source_prompt_entry_id"] = $"quality-gap-repair:{scenarioId}"
            },
            CorrelationId: $"quality-gap-repair-{scenarioId}");

    private static IReadOnlyList<ScenarioRow> ReadScenarioRows()
    {
        var path = Path.Combine(
            FindRepoRoot(),
            "artifacts",
            "extension-runtime-token-quality-learnability",
            "scenario-pack-token-quality-v1.jsonl");
        var rows = new List<ScenarioRow>();
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            rows.Add(new ScenarioRow(
                root.GetProperty("scenario_id").GetString()!,
                root.GetProperty("prompt_text").GetString()!,
                root.GetProperty("methods_to_run").EnumerateArray().Select(item => item.GetString()!).ToArray()));
        }

        return rows;
    }

    private static TempCommonRoot CreateTempCommonRootWithRenamedSkillSelectionCheckpoint()
    {
        var tempRootPath = Path.Combine(Path.GetTempPath(), $"xiigen-skill-selection-manifest-ref-{Guid.NewGuid():N}");
        var skillDir = Path.Combine(tempRootPath, "models", "skill-selection", "skill-selection-v1");
        Directory.CreateDirectory(skillDir);

        var sourceDir = Path.Combine(FindRepoRoot(), "models", "skill-selection", "skill-selection-v1");
        var sourceCheckpoint = Path.Combine(sourceDir, "skill-selection-v1-checkpoint.json");
        var targetCheckpoint = Path.Combine(skillDir, "renamed-skill-selection-checkpoint.json");
        File.Copy(sourceCheckpoint, targetCheckpoint);

        using var manifestDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(sourceDir, "manifest.json")));
        using var stream = File.Create(Path.Combine(skillDir, "manifest.json"));
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            foreach (var property in manifestDocument.RootElement.EnumerateObject())
            {
                if (property.NameEquals("checkpoint_filename"))
                {
                    writer.WriteString(property.Name, "renamed-skill-selection-checkpoint.json");
                }
                else
                {
                    property.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }

        return new TempCommonRoot(tempRootPath);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }

    private sealed class TempCommonRoot(string rootPath) : IDisposable
    {
        public string RootPath { get; } = rootPath;

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
                Directory.Delete(RootPath, recursive: true);
        }
    }

    private sealed record ScenarioRow(string ScenarioId, string PromptText, IReadOnlyList<string> MethodsToRun);
}
