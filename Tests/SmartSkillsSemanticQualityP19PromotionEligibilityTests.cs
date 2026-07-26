// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP19PromotionEligibilityTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void P17EExportCandidateIsNotEligibleForDirectRuntimePromotion()
    {
        var existingManifestPath = RepoPath(
            "models",
            "skill-selection",
            "skill-selection-v1",
            "manifest.json");
        var existingCheckpointPath = RepoPath(
            "models",
            "skill-selection",
            "skill-selection-v1",
            "skill-selection-v1-checkpoint.json");
        var candidateManifestPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "production-export",
            "skill-selection-manifest.json");
        var candidateCheckpointPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "production-export",
            "skill-selection-checkpoint.json");
        var p17eLocatorPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p17e-skillselection-lifecycle-import-export",
            "p17e-manifest-locator-map.json");

        using var existingManifest = JsonDocument.Parse(File.ReadAllText(existingManifestPath, Encoding.UTF8));
        using var existingCheckpoint = JsonDocument.Parse(File.ReadAllText(existingCheckpointPath, Encoding.UTF8));
        using var candidateManifest = JsonDocument.Parse(File.ReadAllText(candidateManifestPath, Encoding.UTF8));
        using var candidateCheckpoint = JsonDocument.Parse(File.ReadAllText(candidateCheckpointPath, Encoding.UTF8));
        using var p17eLocator = JsonDocument.Parse(File.ReadAllText(p17eLocatorPath, Encoding.UTF8));

        Assert.Equal("skill-selection-v1", existingManifest.RootElement.GetProperty("model_id").GetString());
        Assert.Equal(
            "candidate_position_scaffold_not_final_semantic_authority",
            existingManifest.RootElement.GetProperty("route_mode").GetString());
        Assert.False(existingManifest.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());

        Assert.Equal(
            "candidate_position_scaffold_not_final_semantic_authority",
            candidateManifest.RootElement.GetProperty("routeMode").GetString());
        Assert.Equal(0, candidateManifest.RootElement.GetProperty("accuracyAtExport").GetInt32());
        Assert.False(candidateManifest.RootElement.GetProperty("semanticQualityClaimed").GetBoolean());

        var existingPairs = CountArray(existingCheckpoint.RootElement, "trainedPairIds", "trained_pair_ids");
        var candidatePairs = CountArray(candidateCheckpoint.RootElement, "trainedPairIds", "trained_pair_ids");
        var existingPromptKeys = CountArray(existingCheckpoint.RootElement, "trainedPromptKeys", "trained_prompt_keys");
        var candidatePromptKeys = CountArray(candidateCheckpoint.RootElement, "trainedPromptKeys", "trained_prompt_keys");

        Assert.True(existingPairs > candidatePairs);
        Assert.True(existingPromptKeys > candidatePromptKeys);

        var locatorCandidate = p17eLocator.RootElement.GetProperty("production_export_candidate");
        Assert.False(locatorCandidate.GetProperty("runtime_wired").GetBoolean());
        Assert.False(locatorCandidate.GetProperty("final_model_ready_claimed").GetBoolean());

        var locatorEntry = p17eLocator.RootElement.GetProperty("local_locator_entries")[0];
        Assert.False(locatorEntry.GetProperty("common_model_path_updated").GetBoolean());
        Assert.False(locatorEntry.GetProperty("final_model_path_updated").GetBoolean());

        Assert.NotEqual(
            Path.GetFullPath(existingCheckpointPath),
            Path.GetFullPath(candidateCheckpointPath),
            StringComparer.OrdinalIgnoreCase);
    }

    private static int CountArray(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array)
                return value.GetArrayLength();
        }

        return 0;
    }

    private static string RepoPath(params string[] segments)
        => Path.Combine(new[] { RepoRoot }.Concat(segments).ToArray());

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
}
