// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace LlmFromScratch.Tests;

public sealed class SmartSkillsSemanticQualityP20PromotionBlockerRepairTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void P20ArtifactsEncodePromotionBlockersWithoutClaimingReadiness()
    {
        var artifactDir = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p20-skillselection-promotion-blocker-repair");
        var planPath = Path.Combine(artifactDir, "p20-promotion-blocker-repair-plan.json");
        var checklistPath = Path.Combine(artifactDir, "p20-gate-checklist.jsonl");
        var selfCheckPath = Path.Combine(artifactDir, "p20-self-check.json");
        var humanReportPath = Path.Combine(artifactDir, "p20-promotion-blocker-repair-plan.md");

        using var plan = JsonDocument.Parse(File.ReadAllText(planPath, Encoding.UTF8));
        using var selfCheck = JsonDocument.Parse(File.ReadAllText(selfCheckPath, Encoding.UTF8));

        Assert.False(plan.RootElement.GetProperty("promotion_performed").GetBoolean());
        Assert.False(plan.RootElement.GetProperty("runtime_behavior_changed").GetBoolean());
        Assert.False(plan.RootElement.GetProperty("common_model_files_modified").GetBoolean());
        Assert.False(plan.RootElement.GetProperty("readiness_claim_after_repair").GetBoolean());

        var blockers = plan.RootElement.GetProperty("blockers").EnumerateArray().ToArray();
        Assert.Contains(blockers, row => row.GetProperty("blocker_id").GetString() == "candidate_manifest_accuracy_zero");
        Assert.Contains(blockers, row => row.GetProperty("blocker_id").GetString() == "candidate_semantic_quality_false");
        Assert.Contains(blockers, row => row.GetProperty("blocker_id").GetString() == "locator_artifact_only_not_runtime_wired");
        Assert.Contains(blockers, row => row.GetProperty("blocker_id").GetString() == "candidate_would_overwrite_larger_common_asset");
        Assert.Contains(blockers, row => row.GetProperty("blocker_id").GetString() == "missing_fresh_load_through_runtime");
        Assert.All(blockers, row =>
        {
            Assert.False(row.GetProperty("readiness_claim_after_repair").GetBoolean());
            Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("current_evidence_ref").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("required_repair").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("forbidden_shortcut").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("required_test_or_probe").GetString()));
        });

        var requiredGates = new[]
        {
            "compatibility_non_regression",
            "rollback_load_proof",
            "fresh_load_through_xiigen_extension_api_service",
            "truthful_manifest_locator_rewrite",
            "api_firewall_rerun",
            "no_overclaim",
            "state_doc_sync"
        };
        var checklistRows = File.ReadLines(checklistPath, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();
        var checklistIds = checklistRows
            .Select(row => row.RootElement.GetProperty("gate_id").GetString())
            .ToHashSet(StringComparer.Ordinal);

        foreach (var gate in requiredGates)
            Assert.Contains(gate, checklistIds);

        Assert.All(checklistRows, row =>
        {
            Assert.False(row.RootElement.GetProperty("readiness_claim_after_gate").GetBoolean());
            Assert.False(string.IsNullOrWhiteSpace(row.RootElement.GetProperty("forbidden_shortcut").GetString()));
        });

        Assert.False(selfCheck.RootElement.GetProperty("promotion_performed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("skill_selection_readiness_claimed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("final_model_ready_claimed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("dashboard_or_host_readiness_claimed").GetBoolean());

        var humanReport = File.ReadAllText(humanReportPath, Encoding.UTF8);
        Assert.Contains("слепое копирование P17E candidate запрещено", humanReport, StringComparison.Ordinal);
        Assert.Contains("P20 не доказывает, что SkillSelection semantic quality готова", humanReport, StringComparison.Ordinal);

        foreach (var path in Directory.EnumerateFiles(artifactDir))
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            AssertNoEncodingDamage(text);
        }

        foreach (var row in checklistRows)
            row.Dispose();
    }

    private static void AssertNoEncodingDamage(string text)
    {
        Assert.DoesNotContain(((char)0x00D0).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)0x00D1).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)0xFFFD).ToString(), text, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\?{3,}"), text);
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
