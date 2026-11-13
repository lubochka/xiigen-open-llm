// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

public sealed class PromptToTaskMachineAssetLifecycleTests
{
    [Fact]
    public async Task PromptToTaskMachineAssetLifecycle_CommittedRepoAssetFreshLoadsAndImports()
    {
        var repoRoot = FindRepositoryRoot();
        var checkpointPath = Path.Combine(
            repoRoot,
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "prompt-to-task-v1-checkpoint.json");
        var manifestPath = Path.Combine(
            repoRoot,
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "manifest.json");

        var loaded = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var learnedPatterns = loaded.Value!.Checkpoint.LearnedState.LearnedPatterns!;
        var committedPattern = learnedPatterns.First(pattern => pattern.LearnedSignals.Count > 0);

        var fresh = new PromptToTaskMachine(new FileBackedPromptToTaskLearnedStateLoader(checkpointPath, manifestPath));
        var freshResult = fresh.Translate(CreateRequestFromCommittedPattern(
            "ptt-life-repo-001",
            "Private wording is not the learned key.",
            committedPattern));
        var imported = new PromptToTaskMachine();
        var importResult = await imported.ImportAsync(new PromptToTaskLifecycleExport(
            PromptToTaskMachine.MachineId,
            PromptToTaskMachine.DomainTag,
            PromptToTaskMachine.Version,
            checkpointPath,
            manifestPath,
            loaded.Value!.Manifest.CheckpointSha256,
            loaded.Value.Manifest.ExportedAtUtc));
        var importedResult = imported.Translate(CreateRequestFromCommittedPattern(
            "ptt-life-repo-002",
            "Another private wording is not the learned key.",
            committedPattern));

        Assert.True(importResult.IsSuccess, importResult.ErrorMessage);
        Assert.True(freshResult.IsSuccess, freshResult.ErrorMessage);
        Assert.True(importedResult.IsSuccess, importedResult.ErrorMessage);
        Assert.False(loaded.Value!.Checkpoint.SemanticQualityClaimed);
        Assert.False(loaded.Value.Manifest.SemanticQualityClaimed);
        Assert.True(learnedPatterns.Count > 0);
        Assert.Equal(learnedPatterns.Count, loaded.Value.Manifest.LearnedPatternCount);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, freshResult.Value!.ContractStatus);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, importedResult.Value!.ContractStatus);
        Assert.Equal(committedPattern.TaskType, freshResult.Value.StructuredTask.TaskType);
        Assert.Equal(committedPattern.TaskType, importedResult.Value.StructuredTask.TaskType);
    }

    [Fact]
    public async Task PromptToTaskMachineAssetLifecycle_CommittedRepoAssetUsesHonestHashes()
    {
        var repoRoot = FindRepositoryRoot();
        var checkpointPath = Path.Combine(
            repoRoot,
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "prompt-to-task-v1-checkpoint.json");
        var sourceManifestPath = Path.Combine(repoRoot, "artifacts", "prompt-to-task-corpus-summary.json");
        var checkpointJson = await File.ReadAllTextAsync(checkpointPath);
        var loaded = await PromptToTaskMachine.LoadCheckpointAsync(
            checkpointPath,
            Path.Combine(repoRoot, "models", "prompt-to-task", "prompt-to-task-v1", "manifest.json"));

        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var learnedState = loaded.Value!.Checkpoint.LearnedState;
        var learnedPatterns = learnedState.LearnedPatterns!;
        var physicalCheckpointHash = HashBytes(await File.ReadAllBytesAsync(checkpointPath));

        Assert.Equal(physicalCheckpointHash, loaded.Value.Manifest.CheckpointSha256);
        Assert.True(IsLowerHexSha256(loaded.Value.Manifest.CheckpointSha256), loaded.Value.Manifest.CheckpointSha256);
        Assert.True(learnedPatterns.Count > 0);
        Assert.Equal(learnedPatterns.Count, loaded.Value.Manifest.LearnedPatternCount);
        Assert.Equal("artifacts/prompt-to-task-corpus-summary.json", loaded.Value.Manifest.SourceManifestRef);
        Assert.Equal(loaded.Value.Manifest.SourceManifestRef, loaded.Value.Checkpoint.SourceManifestRef);
        Assert.True(File.Exists(sourceManifestPath), sourceManifestPath);
        Assert.False(loaded.Value.Manifest.SemanticQualityClaimed);
        Assert.False(loaded.Value.Checkpoint.SemanticQualityClaimed);
        Assert.DoesNotContain("manifest-recorded-in-git-diff", checkpointJson);
        Assert.DoesNotContain("common-seed-functional-lifecycle", checkpointJson);
        Assert.All(CollectSha256PrefixedValues(checkpointJson), value => Assert.True(IsValidSha256PrefixedValue(value), value));
        Assert.True(IsLowerHexSha256(learnedState.StateSha256), learnedState.StateSha256);
        Assert.Equal(HashLearnedState(learnedState), learnedState.StateSha256);
    }

    [Fact]
    public async Task PromptToTaskMachineAssetLifecycle_ExportImportRoundTripsLearnedState()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ptt-lifecycle-{Guid.NewGuid():N}");
        try
        {
            var source = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(CreateDescriptor([CreatePattern()])));
            var checkpointPath = Path.Combine(root, "prompt-to-task-checkpoint.json");
            var manifestPath = Path.Combine(root, "prompt-to-task-manifest.json");

            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath,
                "artifacts/prompt-to-task-corpus-summary.json",
                ["accepted-ref:ptt-04"]);

            Assert.True(exportResult.IsSuccess, exportResult.ErrorMessage);
            Assert.True(File.Exists(exportResult.Value!.CheckpointRef));
            Assert.True(File.Exists(exportResult.Value.ManifestRef));
            Assert.False(string.IsNullOrWhiteSpace(exportResult.Value.CheckpointSha256));

            var imported = new PromptToTaskMachine();
            var importResult = await imported.ImportAsync(exportResult.Value);
            var translated = imported.Translate(CreateLearnedRequest("ptt-life-001", "Private wording A."));

            Assert.True(importResult.IsSuccess, importResult.ErrorMessage);
            Assert.True(translated.IsSuccess, translated.ErrorMessage);
            Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, translated.Value!.ContractStatus);
            Assert.Equal("bounded_implementation_slice", translated.Value.StructuredTask.TaskType);
            Assert.Equal("ptt-pattern-bounded-implementation", translated.Value.StructuredTask.SourceProvenance["learned_pattern_id"]);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task PromptToTaskMachineAssetLifecycle_FileBackedLoaderFreshLoadsCheckpoint()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ptt-fresh-load-{Guid.NewGuid():N}");
        try
        {
            var source = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(CreateDescriptor([CreatePattern()])));
            var checkpointPath = Path.Combine(root, "prompt-to-task-checkpoint.json");
            var manifestPath = Path.Combine(root, "prompt-to-task-manifest.json");
            var exportResult = await source.SaveCheckpointAsync(
                checkpointPath,
                manifestPath,
                "artifacts/prompt-to-task-corpus-summary.json",
                ["accepted-ref:ptt-04"]);

            var fresh = new PromptToTaskMachine(new FileBackedPromptToTaskLearnedStateLoader(
                exportResult.Value!.CheckpointRef,
                exportResult.Value.ManifestRef));
            var translated = fresh.Translate(CreateLearnedRequest("ptt-life-002", "Private wording B."));

            Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, translated.Value!.ContractStatus);
            Assert.Equal("learned_pattern_state_evaluation", translated.Value.StructuredTask.SourceProvenance["semantic_translation"]);
            Assert.Equal("2", translated.Value.StructuredTask.SourceProvenance["matched_signal_count"]);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PromptToTaskMachineAssetLifecycle_ContinueAddsAcceptedPatternDelta()
    {
        var machine = new PromptToTaskMachine(new StaticPromptToTaskLearnedStateLoader(CreateDescriptor([CreatePattern()])));
        var delta = new PromptToTaskLearnedPattern(
            PatternId: "ptt-pattern-review",
            TaskType: "bounded_review_slice",
            LearnedSignals: ["bounded_review", "work_order:PTT-04"],
            ExplicitUserRequirements: ["review scoped lifecycle evidence"],
            Constraints: ["no host repos"],
            Deliverables: ["parent review packet"],
            ForbiddenActions: ["training quality claim"],
            RequiredHosts: ["llm_mvp_core"],
            EvidenceNeeds: ["checkpoint hash evidence"],
            SplitId: "ptt-04-delta-split",
            Confidence: 0.88);

        var continueResult = machine.ContinueWithAcceptedPattern(delta, "accepted-ref:ptt-04-delta");
        var original = machine.Translate(CreateLearnedRequest("ptt-life-003", "Private wording C."));
        var added = machine.Translate(new PromptToTaskRequest(
            PromptId: "ptt-life-004",
            RawPrompt: "Different private wording.",
            RawPromptSha256: "sha256:ptt-life-004",
            PromptProvenance: "conversation:ptt-04",
            UserLanguage: "en",
            ExplicitConstraints: ["bounded_review", "no host repos"],
            ContextFields: new Dictionary<string, string>
            {
                ["work_order"] = "PTT-04"
            }));

        Assert.True(continueResult.IsSuccess, continueResult.ErrorMessage);
        Assert.False(continueResult.Value!.SemanticQualityClaimed);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, original.Value!.ContractStatus);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, added.Value!.ContractStatus);
        Assert.Equal("bounded_implementation_slice", original.Value.StructuredTask.TaskType);
        Assert.Equal("bounded_review_slice", added.Value.StructuredTask.TaskType);
        Assert.Contains("accepted-ref:ptt-04-delta", continueResult.Value.AcceptedRefIds);
    }

    private static PromptToTaskLearnedStateDescriptor CreateDescriptor(
        IReadOnlyList<PromptToTaskLearnedPattern>? learnedPatterns = null)
        => new(
            StateId: "prompt-to-task-state:v1:test-descriptor",
            ManifestId: "models/prompt-to-task/prompt-to-task-v1/manifest",
            ManifestSha256: "sha256:manifest-test",
            MachineType: nameof(PromptToTaskMachine),
            SchemaVersion: "prompt-to-task-state-v1",
            SourceBaseCommit: "commit:test",
            StateSha256: "sha256:state-test",
            LearnedPatterns: learnedPatterns);

    private static PromptToTaskLearnedPattern CreatePattern()
        => new(
            PatternId: "ptt-pattern-bounded-implementation",
            TaskType: "bounded_implementation_slice",
            LearnedSignals: ["bounded_subphase", "work_order:PTT-03"],
            ExplicitUserRequirements: ["implement scoped production behavior"],
            Constraints: ["no host repos", "no external AI"],
            Deliverables: ["focused test evidence", "implementation integrity artifact"],
            ForbiddenActions: ["training runs", "commit/push"],
            RequiredHosts: ["llm_mvp_core"],
            EvidenceNeeds: ["ablation evidence", "no hidden deterministic shortcut sweep"],
            SplitId: "ptt-03-learned-split",
            Confidence: 0.91);

    private static PromptToTaskRequest CreateLearnedRequest(string promptId, string rawPrompt)
        => new(
            PromptId: promptId,
            RawPrompt: rawPrompt,
            RawPromptSha256: $"sha256:{promptId}",
            PromptProvenance: "conversation:ptt-04",
            UserLanguage: "en",
            ExplicitConstraints: ["bounded_subphase", "no host repos"],
            ContextFields: new Dictionary<string, string>
            {
                ["source_root_id"] = "claude-user-prompts",
                ["work_order"] = "PTT-03"
            });

    private static PromptToTaskRequest CreateRequestFromCommittedPattern(
        string promptId,
        string rawPrompt,
        PromptToTaskLearnedPattern pattern)
        => new(
            PromptId: promptId,
            RawPrompt: rawPrompt,
            RawPromptSha256: $"sha256:{promptId}",
            PromptProvenance: $"committed-pattern:{pattern.PatternId}",
            UserLanguage: "en",
            ExplicitConstraints: pattern.LearnedSignals,
            ContextFields: new Dictionary<string, string>
            {
                ["committed_pattern_id"] = pattern.PatternId,
                ["committed_split_id"] = pattern.SplitId
            });

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private static string HashLearnedState(PromptToTaskLearnedStateDescriptor learnedState)
    {
        var canonical = JsonSerializer.Serialize(
            learnedState with { StateSha256 = string.Empty },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return HashBytes(Encoding.UTF8.GetBytes(canonical));
    }

    private static IReadOnlyList<string> CollectSha256PrefixedValues(string json)
    {
        using var document = JsonDocument.Parse(json);
        var values = new List<string>();
        CollectSha256PrefixedValues(document.RootElement, values);
        return values;
    }

    private static void CollectSha256PrefixedValues(JsonElement element, List<string> values)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            if (value is not null && value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
                values.Add(value);
            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                CollectSha256PrefixedValues(item, values);
            return;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
                CollectSha256PrefixedValues(property.Value, values);
        }
    }

    private static bool IsValidSha256PrefixedValue(string value)
        => value.Length == 71 && value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) && IsHex(value[7..]);

    private static bool IsLowerHexSha256(string value)
        => value.Length == 64 && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsHex(string value)
        => value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');

    private static string HashBytes(byte[] bytes)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(bytes)).ToLowerInvariant();
    }

    private sealed class StaticPromptToTaskLearnedStateLoader(PromptToTaskLearnedStateDescriptor descriptor)
        : IPromptToTaskLearnedStateLoader
    {
        public AgentInfrastructure.OperationResult<PromptToTaskLearnedStateDescriptor> Load()
            => AgentInfrastructure.OperationResult<PromptToTaskLearnedStateDescriptor>.Ok(descriptor);
    }
}
