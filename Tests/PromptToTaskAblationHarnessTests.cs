// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

public sealed class PromptToTaskAblationHarnessTests
{
    [Fact]
    public void PromptToTaskAblationHarness_ParsesAllP5IRowsAndJoinsP5HRows()
    {
        var repoRoot = FindRepositoryRoot();
        var invocationCount = 0;
        var harness = new PromptToTaskAblationHarness(request =>
        {
            invocationCount++;
            return new PromptToTaskMachine().Translate(request);
        });

        var result = harness.Run(CreateRequest(repoRoot, CreateTempOutputDirectory()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(36, result.Value!.PlannedProbeCount);
        Assert.Equal(36, result.Value.InvokedProbeCount);
        Assert.Equal(36, invocationCount);
        Assert.Equal(6, result.Value.Results.Select(row => row.SourceRowId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(result.Value.Results, row => Assert.Equal("PromptToTask", row.TargetMachine));
        Assert.All(result.Value.Results, row => Assert.StartsWith("sha256:", row.RawPromptSha256, StringComparison.Ordinal));
    }

    [Fact]
    public void PromptToTaskAblationHarness_InvokesPromptToTaskRuntimeForEveryProbeAndRecordsNotReady()
    {
        var repoRoot = FindRepositoryRoot();
        var harness = new PromptToTaskAblationHarness();

        var result = harness.Run(CreateRequest(repoRoot, CreateTempOutputDirectory()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(36, result.Value!.InvokedProbeCount);
        Assert.Equal(36, result.Value.NotReadyCount);
        Assert.Equal(0, result.Value.RuntimeErrorCount);
        Assert.All(result.Value.Results, row => Assert.Equal("not_ready", row.ResultStatus));
        Assert.All(result.Value.Results, row => Assert.Equal(PromptToTaskMachine.NotTrainedStatus, row.RuntimeContractStatus));
        Assert.All(result.Value.Results, row => Assert.Equal(PromptToTaskMachine.NotReadyTaskType, row.RuntimeTaskType));
        Assert.All(result.Value.Results, row => Assert.Equal("none_loaded", row.RuntimeMachineState));
    }

    [Fact]
    public void PromptToTaskAblationHarness_DoesNotWriteTrainingCheckpointOrModelAssets()
    {
        var repoRoot = FindRepositoryRoot();
        var commonCheckpoint = Path.Combine(
            repoRoot,
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "prompt-to-task-v1-checkpoint.json");
        var commonManifest = Path.Combine(
            repoRoot,
            "models",
            "prompt-to-task",
            "prompt-to-task-v1",
            "manifest.json");
        var checkpointBefore = HashFile(commonCheckpoint);
        var manifestBefore = HashFile(commonManifest);
        var harness = new PromptToTaskAblationHarness();

        var result = harness.Run(CreateRequest(repoRoot, CreateTempOutputDirectory()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.TrainingRun);
        Assert.False(result.Value.EvalRun);
        Assert.False(result.Value.CheckpointOrModelChanged);
        Assert.False(result.Value.SemanticQualityClaimed);
        Assert.False(result.Value.TrainingReady);
        Assert.Equal(checkpointBefore, HashFile(commonCheckpoint));
        Assert.Equal(manifestBefore, HashFile(commonManifest));
    }

    [Fact]
    public void PromptToTaskAblationHarness_WritesUtf8ReadableArtifactsWithoutMojibake()
    {
        var repoRoot = FindRepositoryRoot();
        var outputDirectory = CreateTempOutputDirectory();
        var harness = new PromptToTaskAblationHarness();

        var result = harness.Run(CreateRequest(repoRoot, outputDirectory));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(File.Exists(Path.Combine(outputDirectory, PromptToTaskAblationHarness.ResultsJsonlFileName)));
        Assert.True(File.Exists(Path.Combine(outputDirectory, PromptToTaskAblationHarness.RuntimeReportFileName)));
        Assert.True(File.Exists(Path.Combine(outputDirectory, PromptToTaskAblationHarness.SelfCheckFileName)));
        Assert.True(File.Exists(Path.Combine(outputDirectory, PromptToTaskAblationHarness.EvidenceCardFileName)));
        Assert.All(Directory.GetFiles(outputDirectory), file =>
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            AssertDoesNotContainMojibake(text);
        });
        Assert.Contains("runtime evidence for PromptToTask", File.ReadAllText(
            Path.Combine(outputDirectory, PromptToTaskAblationHarness.RuntimeReportFileName),
            Encoding.UTF8));
        Assert.True(result.Value!.MojibakeScanPassed);
    }

    [Fact]
    public void PromptToTaskAblationHarness_GeneratesCurrentP5KArtifacts()
    {
        var repoRoot = FindRepositoryRoot();
        var outputDirectory = Path.Combine(
            repoRoot,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p5k-prompt-to-task-ablation-harness");
        var harness = new PromptToTaskAblationHarness();

        var result = harness.Run(CreateRequest(repoRoot, outputDirectory));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(36, result.Value!.PlannedProbeCount);
        Assert.Equal(36, result.Value.InvokedProbeCount);
        Assert.Equal(36, result.Value.NotReadyCount);
        Assert.False(result.Value.SemanticQualityClaimed);
        Assert.False(result.Value.TrainingReady);
        Assert.True(result.Value.MojibakeScanPassed);

        var selfCheckPath = Path.Combine(outputDirectory, PromptToTaskAblationHarness.SelfCheckFileName);
        using var document = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));
        Assert.True(document.RootElement.GetProperty("prompt_to_task_runtime_invoked").GetBoolean());
        Assert.False(document.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
    }

    private static PromptToTaskAblationHarnessRequest CreateRequest(string repoRoot, string outputDirectory)
        => new(
            ProbePlanJsonlPath: Path.Combine(
                repoRoot,
                "artifacts",
                "smart-skills-semantic-quality-completion",
                "p5i-prompt-to-task-split-ablation-plan",
                "prompt-to-task-executable-ablation-plan.jsonl"),
            RedactedDpoRowsJsonlPath: Path.Combine(
                repoRoot,
                "artifacts",
                "smart-skills-semantic-quality-completion",
                "p5h-prompt-to-task-dpo-materialization",
                "prompt-to-task-redacted-dpo-rows.jsonl"),
            OutputDirectory: outputDirectory);

    private static string CreateTempOutputDirectory()
        => Path.Combine(Path.GetTempPath(), $"p5k-ptt-ablation-{Guid.NewGuid():N}");

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private static string HashFile(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static void AssertDoesNotContainMojibake(string text)
    {
        Assert.DoesNotContain(((char)0x00D0).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)0x00D1).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotContain(new string('?', 4), text, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)0xFFFD).ToString(), text, StringComparison.Ordinal);
    }
}
