// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningMachineAssetLifecycleTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_ExportImportRoundTripsLearnedGuideState()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-lifecycle-{Guid.NewGuid():N}");
        try
        {
            var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));
            var checkpointPath = Path.Combine(root, "orchestration-reasoning-checkpoint.json");
            var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");

            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath,
                "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json",
                ["accepted-ref:or-04a"]);

            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
            Assert.True(File.Exists(exportResult.Value!.CheckpointRef));
            Assert.True(File.Exists(exportResult.Value.ManifestRef));
            Assert.False(string.IsNullOrWhiteSpace(exportResult.Value.CheckpointSha256));

            var imported = new OrchestrationReasoningMachine();
            var importResult = await imported.ImportAsync(exportResult.Value);
            var planned = imported.Plan(CreateLearnedRequest("sha256:or-life-import-a"));

            Assert.True(importResult.IsSuccess, importResult.ErrorMessage);
            Assert.True(planned.IsSuccess, planned.ErrorMessage);
            Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, planned.Value!.ContractStatus);
            Assert.Equal("scenario:bounded-implementation-review-loop", planned.Value.ScenarioPlan.ScenarioId);
            Assert.Equal(["or-pattern-bounded-implementation-review"], planned.Value.ScenarioPlan.SelectedLearnedPatterns);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_PersistedManifestUsesPortableRefs()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-portable-{Guid.NewGuid():N}");
        try
        {
            var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));
            var checkpointPath = Path.Combine(root, "nested", "orchestration-reasoning-checkpoint.json");
            var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");

            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath,
                "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json",
                ["accepted-ref:or-04a"]);
            var manifestJson = await File.ReadAllTextAsync(exportResult.Value!.ManifestRef);
            var manifest = JsonSerializer.Deserialize<OrchestrationReasoningCheckpointManifest>(manifestJson, JsonOptions)!;

            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
            Assert.Equal("nested/orchestration-reasoning-checkpoint.json", manifest.CheckpointRef);
            Assert.Equal("orchestration-reasoning-manifest.json", manifest.ManifestRef);
            Assert.DoesNotContain(root, manifestJson, StringComparison.OrdinalIgnoreCase);
            Assert.True(Path.IsPathRooted(exportResult.Value.CheckpointRef));
            Assert.True(Path.IsPathRooted(exportResult.Value.ManifestRef));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_MismatchedManifestSourceRefFailsLoad()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-source-mismatch-{Guid.NewGuid():N}");
        try
        {
            var paths = await SaveValidCheckpointAsync(root);
            var manifest = await ReadManifestAsync(paths.ManifestPath);
            await WriteManifestAsync(paths.ManifestPath, manifest with
            {
                SourceManifestRef = "training-data/orchestration-reasoning/different-source-manifest.json"
            });

            var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(paths.CheckpointPath, paths.ManifestPath);

            Assert.False(loadResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_MANIFEST_SOURCE_REF_MISMATCH", loadResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_MismatchedManifestPatternCountFailsLoad()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-pattern-count-mismatch-{Guid.NewGuid():N}");
        try
        {
            var paths = await SaveValidCheckpointAsync(root);
            var manifest = await ReadManifestAsync(paths.ManifestPath);
            await WriteManifestAsync(paths.ManifestPath, manifest with
            {
                LearnedPatternCount = manifest.LearnedPatternCount + 1
            });

            var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(paths.CheckpointPath, paths.ManifestPath);

            Assert.False(loadResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_MANIFEST_PATTERN_COUNT_MISMATCH", loadResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_FileBackedLoaderFreshLoadsCheckpoint()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-fresh-load-{Guid.NewGuid():N}");
        try
        {
            var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));
            var checkpointPath = Path.Combine(root, "orchestration-reasoning-checkpoint.json");
            var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");
            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath,
                "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json",
                ["accepted-ref:or-04a"]);

            var original = source.Plan(CreateLearnedRequest("sha256:or-life-original"));
            var fresh = new OrchestrationReasoningMachine(new FileBackedOrchestrationReasoningLearnedGuideStateLoader(
                exportResult.Value!.CheckpointRef,
                exportResult.Value.ManifestRef));
            var freshResult = fresh.Plan(CreateLearnedRequest("sha256:or-life-fresh"));

            Assert.True(original.IsSuccess, original.ErrorMessage);
            Assert.True(freshResult.IsSuccess, freshResult.ErrorMessage);
            Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, freshResult.Value!.ContractStatus);
            Assert.Equal(original.Value!.ScenarioPlan.ScenarioId, freshResult.Value.ScenarioPlan.ScenarioId);
            Assert.Equal(original.Value.ScenarioPlan.SelectedLearnedPatterns, freshResult.Value.ScenarioPlan.SelectedLearnedPatterns);
            Assert.Contains("INFERENCE_SOURCE:LOADED_LEARNED_GUIDE_STATE", freshResult.Value.ContractEvidence);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_NoStateAndNoPatternAblationsRemainNoSemanticPlan()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-ablation-{Guid.NewGuid():N}");
        try
        {
            var noState = new OrchestrationReasoningMachine().Plan(CreateLearnedRequest("sha256:or-life-no-state"));
            var noPatterns = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([])))
                .Plan(CreateLearnedRequest("sha256:or-life-no-patterns"));
            var sourceWithoutPatterns = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([])));
            var exportResult = await sourceWithoutPatterns.SaveCheckpointAsync(
                Path.Combine(root, "checkpoint.json"),
                Path.Combine(root, "manifest.json"),
                "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json",
                ["accepted-ref:or-04a"]);

            Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, noState.Value!.ContractStatus);
            Assert.Equal(OrchestrationReasoningMachine.LoadedGuideStateReadyStatus, noPatterns.Value!.ContractStatus);
            Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, noState.Value.ScenarioPlan.ScenarioId);
            Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, noPatterns.Value.ScenarioPlan.ScenarioId);
            Assert.False(exportResult.IsSuccess);
            Assert.Equal("ORCHESTRATION_REASONING_EXPORT_PATTERNS_REQUIRED", exportResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OrchestrationReasoningMachineAssetLifecycle_CheckpointAndManifestDoNotClaimOutOfScopeReadiness()
    {
        var root = Path.Combine(Path.GetTempPath(), $"or-claims-{Guid.NewGuid():N}");
        try
        {
            var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));
            var checkpointPath = Path.Combine(root, "orchestration-reasoning-checkpoint.json");
            var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");
            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath,
                "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json",
                ["accepted-ref:or-04a"]);

            var loaded = await OrchestrationReasoningMachine.LoadCheckpointAsync(
                exportResult.Value!.CheckpointRef,
                exportResult.Value.ManifestRef);
            var checkpointJson = await File.ReadAllTextAsync(exportResult.Value.CheckpointRef);
            var manifestJson = await File.ReadAllTextAsync(exportResult.Value.ManifestRef);

            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.False(loaded.Value!.Checkpoint.SemanticQualityClaimed);
            Assert.False(loaded.Value.Checkpoint.ContinueTrainingClaimed);
            Assert.False(loaded.Value.Checkpoint.HostRuntimeClaimed);
            Assert.False(loaded.Value.Manifest.SemanticQualityClaimed);
            Assert.False(loaded.Value.Manifest.ContinueTrainingClaimed);
            Assert.False(loaded.Value.Manifest.HostRuntimeClaimed);
            Assert.DoesNotContain("sensitive-local-wording", checkpointJson);
            Assert.DoesNotContain("sensitive-local-wording", manifestJson);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static OrchestrationReasoningLearnedGuideStateDescriptor CreateDescriptor(
        IReadOnlyList<OrchestrationReasoningLearnedPattern>? learnedPatterns = null)
        => new(
            StateId: "orchestration-reasoning-state:v1:lifecycle-test",
            GuideStateRef: "guide-state:orchestration-reasoning:common:v1",
            ManifestId: "models/orchestration-reasoning/orchestration-reasoning-v1/manifest",
            ManifestSha256: "sha256:or-life-manifest-test",
            MachineType: nameof(OrchestrationReasoningMachine),
            SchemaVersion: "orchestration-reasoning-state-v1",
            SourceBaseCommit: "commit:or-life-test",
            StateSha256: "sha256:or-life-state-test",
            LearnedPatterns: learnedPatterns);

    private static OrchestrationReasoningLearnedPattern CreatePattern()
        => new(
            PatternId: "or-pattern-bounded-implementation-review",
            ScenarioRef: "scenario:bounded-implementation-review-loop",
            RequiredTaskTypes: ["bounded_implementation_slice"],
            RequiredConstraintSignals: ["no host repos", "no external AI"],
            RequiredRequirementSignals: ["parent review required"],
            RequiredDeliverableSignals: ["focused test evidence"],
            RequiredEvidenceSignals: ["ablation evidence"],
            PhaseFamilies: ["implementation", "review"],
            GateFamilies: ["parent-review", "completion-matrix", "no-hidden-determinism"],
            AssetFamilies: ["llm_mvp_core"],
            HostCheckSets: ["codex"],
            RiskFlags: ["no_hidden_determinism_review_required", "no_training_claim"],
            NextGranularizationActions: ["write completion matrix before parent review", "prepare commit boundary only after acceptance"],
            Confidence: 0.93);

    private static OrchestrationReasoningRequest CreateLearnedRequest(string rawPromptSha256)
        => new(
            RequestId: $"orch-life-request-{rawPromptSha256}",
            Task: new StructuredTaskContract(
                PromptId: "prompt-or-life-001",
                RawPromptSha256: rawPromptSha256,
                UserLanguage: "en",
                TaskType: "bounded_implementation_slice",
                ExplicitUserRequirements: ["parent review required"],
                Constraints: ["no host repos", "no external AI"],
                Deliverables: ["focused test evidence"],
                ForbiddenActions: ["training runs", "commit/push"],
                RequiredHosts: ["claude", "cursor", "codex"],
                EvidenceNeeds: ["ablation evidence"],
                SourceProvenance: new Dictionary<string, string>
                {
                    ["source_root_id"] = "or-04a-temp-fixture",
                    ["fixture_text_sha256"] = "sha256:fixture-only"
                },
                SplitId: "split-or-life-01"),
            GuideDerivedStateRefs: ["guide-state:orchestration-reasoning:common:v1"],
            CorpusBundleIds: ["corpus:orchestration-reasoning:common"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static async Task<(string CheckpointPath, string ManifestPath)> SaveValidCheckpointAsync(string root)
    {
        var source = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));
        var checkpointPath = Path.Combine(root, "orchestration-reasoning-checkpoint.json");
        var manifestPath = Path.Combine(root, "orchestration-reasoning-manifest.json");
        var exportResult = await source.SaveCheckpointAsync(
            checkpointPath,
            manifestPath,
            "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json",
            ["accepted-ref:or-04a"]);

        Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
        return (exportResult.Value!.CheckpointRef, exportResult.Value.ManifestRef);
    }

    private static async Task<OrchestrationReasoningCheckpointManifest> ReadManifestAsync(string manifestPath)
        => JsonSerializer.Deserialize<OrchestrationReasoningCheckpointManifest>(
            await File.ReadAllTextAsync(manifestPath),
            JsonOptions)!;

    private static Task WriteManifestAsync(
        string manifestPath,
        OrchestrationReasoningCheckpointManifest manifest)
        => File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions));

    private sealed class StaticOrchestrationReasoningGuideStateLoader(
        OrchestrationReasoningLearnedGuideStateDescriptor descriptor)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public AgentInfrastructure.OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load()
            => AgentInfrastructure.OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor>.Ok(descriptor);
    }
}
