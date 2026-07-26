// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP10CheckpointRewriteTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string P10Dir = Path.Combine(
        RepoRoot,
        "artifacts",
        "smart-skills-semantic-quality-completion",
        "p10-checkpoint-rewrite");

    [Fact]
    public void P10ArtifactsResolveHashesAndKeepLaterGatesPending()
    {
        var reportPath = Path.Combine(P10Dir, "smart-skills-p10-checkpoint-rewrite-report.json");
        var manifestsPath = Path.Combine(P10Dir, "smart-skills-p10-component-manifests.json");
        var indexPath = Path.Combine(P10Dir, "smart-skills-p10-component-index.json");
        var locatorPath = Path.Combine(P10Dir, "smart-skills-p10-locator.json");
        var ledgerPath = Path.Combine(P10Dir, "smart-skills-p10-lineage-ledger.jsonl");
        var selfCheckPath = Path.Combine(P10Dir, "p10-self-check.json");

        using var reportDoc = JsonDocument.Parse(File.ReadAllText(reportPath, Encoding.UTF8));
        using var manifestsDoc = JsonDocument.Parse(File.ReadAllText(manifestsPath, Encoding.UTF8));
        using var indexDoc = JsonDocument.Parse(File.ReadAllText(indexPath, Encoding.UTF8));
        using var locatorDoc = JsonDocument.Parse(File.ReadAllText(locatorPath, Encoding.UTF8));
        using var selfCheckDoc = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));

        Assert.False(GetBool(reportDoc.RootElement.GetProperty("safety_flags"), "semanticQualityClaimed"));
        Assert.False(GetBool(reportDoc.RootElement.GetProperty("safety_flags"), "semantic_quality_claimed"));
        Assert.False(GetBool(reportDoc.RootElement.GetProperty("safety_flags"), "fullApiSemanticQualityClaimed"));
        Assert.False(GetBool(reportDoc.RootElement.GetProperty("safety_flags"), "full_api_semantic_quality_claimed"));
        Assert.False(GetBool(reportDoc.RootElement.GetProperty("safety_flags"), "finalModelReadyClaimed"));
        Assert.False(GetBool(reportDoc.RootElement.GetProperty("safety_flags"), "final_model_ready_claimed"));

        var components = manifestsDoc.RootElement.GetProperty("components").EnumerateArray().ToArray();
        Assert.Equal(2, components.Length);

        foreach (var component in components)
        {
            var checkpointRef = component.GetProperty("candidate_checkpoint_ref").GetString()!;
            var checkpointPath = ResolveRepoPath(checkpointRef);
            Assert.True(File.Exists(checkpointPath), checkpointRef);

            var expectedHash = component.GetProperty("candidate_checkpoint_sha256").GetString()!;
            Assert.Equal(expectedHash, Sha256File(checkpointPath));

            var manifestRef = component.GetProperty("candidate_manifest_ref").GetString()!;
            Assert.True(File.Exists(ResolveRepoPath(manifestRef)), manifestRef);

            foreach (var evalRef in component.GetProperty("eval_ablation_report_refs").EnumerateArray())
            {
                var evalPath = ResolveRepoPath(evalRef.GetString()!);
                Assert.True(File.Exists(evalPath), evalPath);
            }

            Assert.Equal("pending_p11_fresh_load_gate", component.GetProperty("fresh_load_status").GetString());
            Assert.Equal("pending_p12_continue_training_gate", component.GetProperty("continue_training_status").GetString());
            Assert.Empty(component.GetProperty("fresh_load_report_refs").EnumerateArray());
            Assert.Empty(component.GetProperty("continue_training_report_refs").EnumerateArray());
            Assert.False(GetBool(component, "semanticQualityClaimed"));
            Assert.False(GetBool(component, "semantic_quality_claimed"));
            Assert.False(GetBool(component, "fullApiSemanticQualityClaimed"));
            Assert.False(GetBool(component, "full_api_semantic_quality_claimed"));
            Assert.False(GetBool(component, "finalModelReadyClaimed"));
            Assert.False(GetBool(component, "final_model_ready_claimed"));

            if (component.TryGetProperty("previous_checkpoint_ref", out var previousRefElement)
                && previousRefElement.ValueKind == JsonValueKind.String)
            {
                var previousRef = previousRefElement.GetString()!;
                var previousPath = ResolveRepoPath(previousRef);
                Assert.True(File.Exists(previousPath), previousRef);
                Assert.Equal(
                    component.GetProperty("previous_checkpoint_sha256").GetString(),
                    Sha256File(previousPath));
            }
        }

        foreach (var entry in indexDoc.RootElement.GetProperty("components").EnumerateArray())
        {
            Assert.True(File.Exists(ResolveRepoPath(entry.GetProperty("candidate_checkpoint_ref").GetString()!)));
            Assert.True(File.Exists(ResolveRepoPath(entry.GetProperty("candidate_manifest_ref").GetString()!)));
        }

        foreach (var entry in locatorDoc.RootElement.GetProperty("entries").EnumerateArray())
        {
            Assert.True(File.Exists(ResolveRepoPath(entry.GetProperty("checkpoint_ref").GetString()!)));
            Assert.True(File.Exists(ResolveRepoPath(entry.GetProperty("manifest_ref").GetString()!)));
            Assert.True(entry.GetProperty("resolves_under_repo_artifacts").GetBoolean());
        }

        var ledgerRows = File.ReadAllLines(ledgerPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();
        Assert.Equal(2, ledgerRows.Length);
        foreach (var row in ledgerRows)
        {
            Assert.Equal("pending_p11_fresh_load_gate", row.RootElement.GetProperty("fresh_load_status").GetString());
            Assert.Equal("pending_p12_continue_training_gate", row.RootElement.GetProperty("continue_training_status").GetString());
            Assert.False(GetBool(row.RootElement, "semantic_quality_claimed"));
            Assert.False(GetBool(row.RootElement, "full_api_semantic_quality_claimed"));
            Assert.False(GetBool(row.RootElement, "final_model_ready_claimed"));
        }

        Assert.True(GetBool(selfCheckDoc.RootElement, "checkpoint_hashes_computed_from_bytes"));
        Assert.True(GetBool(selfCheckDoc.RootElement, "p11_fresh_load_pending"));
        Assert.True(GetBool(selfCheckDoc.RootElement, "p12_continue_training_pending"));
        AssertNoMojibake(P10Dir);
    }

    private static string ResolveRepoPath(string repoRelativeOrAbsolute)
    {
        if (Path.IsPathRooted(repoRelativeOrAbsolute))
        {
            return repoRelativeOrAbsolute;
        }

        return Path.GetFullPath(Path.Combine(RepoRoot, repoRelativeOrAbsolute.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool GetBool(JsonElement element, string propertyName)
    {
        return element.GetProperty(propertyName).GetBoolean();
    }

    private static void AssertNoMojibake(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            Assert.DoesNotContain('\u00D0', text);
            Assert.DoesNotContain('\u00D1', text);
            Assert.DoesNotContain('\uFFFD', text);
            Assert.DoesNotContain('\u00E2', text);
            Assert.DoesNotContain('\u00C3', text);
            Assert.DoesNotContain(new string('?', 4), text);
        }
    }

    private static string FindRepoRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "AGENTS.md"))
                && Directory.Exists(Path.Combine(current, "artifacts")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
