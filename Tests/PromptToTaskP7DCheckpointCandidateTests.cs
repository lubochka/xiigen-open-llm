// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

public sealed class PromptToTaskP7DCheckpointCandidateTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task PromptToTaskP7DCheckpointCandidate_LoadsThroughRuntimeAndKeepsProvenanceOutOfSignals()
    {
        var repoRoot = FindRepositoryRoot();
        var artifactDir = Path.Combine(
            repoRoot,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p7d-prompt-to-task-checkpoint-candidate");
        var checkpointPath = Path.Combine(artifactDir, "prompt-to-task-p7d-candidate-checkpoint.json");
        var manifestPath = Path.Combine(artifactDir, "prompt-to-task-p7d-candidate-manifest.json");

        var loaded = await PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath);

        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(
            "artifacts/smart-skills-semantic-quality-completion/p7d-prompt-to-task-checkpoint-candidate/prompt-to-task-p7d-source-manifest.json",
            loaded.Value!.Checkpoint.LearnedState.ManifestId);
        Assert.Equal(
            "artifacts/smart-skills-semantic-quality-completion/p7d-prompt-to-task-checkpoint-candidate/prompt-to-task-p7d-source-manifest.json",
            loaded.Value.Checkpoint.SourceManifestRef);
        Assert.Equal(Sha256File(checkpointPath), loaded.Value.Manifest.CheckpointSha256);
        Assert.False(loaded.Value!.Checkpoint.SemanticQualityClaimed);
        Assert.False(loaded.Value.Manifest.SemanticQualityClaimed);
        Assert.Equal(5, loaded.Value.Checkpoint.LearnedState.LearnedPatterns!.Count);
        Assert.Contains("p7c:P7C-PTT-TRAIN-001", loaded.Value.Checkpoint.AcceptedRefIds);
        Assert.DoesNotContain(loaded.Value.Checkpoint.AcceptedRefIds, value => value.Contains("EVAL", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(loaded.Value.Checkpoint.AcceptedRefIds, value => value.Contains("HOLDOUT", StringComparison.OrdinalIgnoreCase));

        var forbiddenSignalMarkers = new[]
        {
            "P7C-PTT-TRAIN",
            "source_family",
            "source_ref",
            "source_hash",
            "row_id",
            ".md",
            ".json",
            "\\",
            "/"
        };

        foreach (var pattern in loaded.Value.Checkpoint.LearnedState.LearnedPatterns)
        {
            foreach (var signal in pattern.LearnedSignals)
            {
                Assert.DoesNotContain(forbiddenSignalMarkers, marker =>
                    signal.Contains(marker, StringComparison.OrdinalIgnoreCase));
            }
        }

        AssertNoPrivateAbsolutePaths(
            checkpointPath,
            manifestPath,
            Path.Combine(artifactDir, "prompt-to-task-p7d-lifecycle-export.json"),
            Path.Combine(artifactDir, "prompt-to-task-p7d-dataset-merge-report.json"),
            Path.Combine(artifactDir, "prompt-to-task-p7d-dataset-merge-report.md"));

        var request = new PromptToTaskRequest(
            PromptId: "p7d-focused-load-check",
            RawPrompt: "redacted surrogate: broad product request requires task contract before execution",
            RawPromptSha256: Sha256("redacted surrogate: broad product request requires task contract before execution"),
            PromptProvenance: "p7d-focused-test-redacted-surrogate",
            UserLanguage: "ru",
            ExplicitConstraints:
            [
                "broad_product_request",
                "task_contract_before_execution",
                "phased_plan",
                "coverage_verification",
                "positive_negative_examples"
            ],
            ContextFields: new Dictionary<string, string>
            {
                ["state_recovery_plan"] = "required",
                ["no_code_before_contract"] = "required"
            });

        var translated = loaded.Value.Machine.Translate(request);

        Assert.True(translated.IsSuccess, translated.ErrorMessage);
        Assert.Equal(PromptToTaskMachine.LearnedPatternStatus, translated.Value!.ContractStatus);
        Assert.Equal("phased_architecture_plan_contract", translated.Value.StructuredTask.TaskType);
        Assert.Contains("SEMANTIC_QUALITY_CLAIMED_FALSE", translated.Value.ContractEvidence);
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LlmFromScratch.sln")))
                return current;

            current = Directory.GetParent(current)?.FullName;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void AssertNoPrivateAbsolutePaths(params string[] paths)
    {
        foreach (var path in paths)
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            Assert.DoesNotContain("C:\\Projects", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("C:/Projects", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("llm_mvp_core-DpoImplementation", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("llm_mvp_core\\", text, StringComparison.OrdinalIgnoreCase);
        }
    }
}
