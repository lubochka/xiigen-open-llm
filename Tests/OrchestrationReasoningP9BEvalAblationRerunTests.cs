// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningP9BEvalAblationRerunTests
{
    private static readonly string[] ForbiddenTrainingRows =
    [
        "P5E-DPO-ORCH-003",
        "P5E-DPO-ORCH-005",
        "P4-ORCH-DPO-DRAFT-001",
        "P4-ORCH-DPO-DRAFT-002"
    ];

    [Fact]
    public async Task P9ACandidate_FreshLoadsAndP9BRerunArtifactsMatchRuntimeEvidence()
    {
        var checkpointPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p9a-orchestration-reasoning-missing-coverage-repair",
            "orchestration-reasoning-p9a-candidate-checkpoint.json");
        var manifestPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p9a-orchestration-reasoning-missing-coverage-repair",
            "orchestration-reasoning-p9a-candidate-manifest.json");
        var p9bDirectory = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p9b-orchestration-reasoning-eval-ablation-rerun");

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);
        var machine = loadResult.Value!.Machine;
        var checkpoint = loadResult.Value.Checkpoint;

        Assert.False(checkpoint.SemanticQualityClaimed);
        Assert.False(checkpoint.ContinueTrainingClaimed);
        Assert.False(checkpoint.HostRuntimeClaimed);
        Assert.False(loadResult.Value.Manifest.SemanticQualityClaimed);
        Assert.Equal(5, checkpoint.LearnedState.LearnedPatterns!.Count);

        var learnedPatternJson = JsonSerializer.Serialize(checkpoint.LearnedState.LearnedPatterns);
        foreach (var forbiddenRow in ForbiddenTrainingRows)
        {
            Assert.DoesNotContain(forbiddenRow, checkpoint.AcceptedRefIds);
            Assert.DoesNotContain(forbiddenRow, learnedPatternJson);
        }

        var positiveRows = ReadJsonLines(Path.Combine(p9bDirectory, "orchestration-reasoning-p9b-reserved-positive-results.jsonl"));
        Assert.Equal(2, positiveRows.Count);
        foreach (var row in positiveRows)
        {
            var rowId = row.RootElement.GetProperty("row_id").GetString();
            var expectedPatternId = row.RootElement.GetProperty("expected_pattern_id").GetString();
            var response = machine.Plan(CreateReservedPositiveRequest(rowId!, expectedPatternId!));

            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, response.Value!.ContractStatus);
            Assert.Equal(expectedPatternId, response.Value.ScenarioPlan.SelectedLearnedPatterns.Single());
            Assert.True(row.RootElement.GetProperty("passed").GetBoolean());
            var observedStatus = row.RootElement.GetProperty("observed_status").GetString();
            Assert.Equal("loaded_checkpoint_baseline_plan_produced", observedStatus);
            Assert.Equal(expectedPatternId, row.RootElement.GetProperty("matched_pattern_id").GetString());
            Assert.DoesNotContain("semantic", observedStatus, StringComparison.OrdinalIgnoreCase);
        }

        var ablationRows = ReadJsonLines(Path.Combine(p9bDirectory, "orchestration-reasoning-p9b-ablation-control-results.jsonl"));
        Assert.Equal(16, ablationRows.Count);
        foreach (var row in ablationRows)
        {
            var rowId = row.RootElement.GetProperty("row_id").GetString();
            var kind = row.RootElement.GetProperty("ablation_kind").GetString();
            var response = machine.Plan(CreateAblationRequest(rowId!, kind!));

            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.NotEqual(OrchestrationReasoningMachine.LearnedPatternStatus, response.Value!.ContractStatus);
            Assert.Empty(response.Value.ScenarioPlan.SelectedLearnedPatterns);
            Assert.False(row.RootElement.GetProperty("shortcut_leak").GetBoolean());
            Assert.True(row.RootElement.GetProperty("passed_as_blocked").GetBoolean());
        }

        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(p9bDirectory, "orchestration-reasoning-p9b-eval-ablation-report.json"), Encoding.UTF8));
        Assert.Equal("PASS_CANDIDATE_READY_FOR_PARENT_REVIEW", report.RootElement.GetProperty("verdict").GetString());
        Assert.Equal(2, report.RootElement.GetProperty("reserved_positive_counts").GetProperty("passed").GetInt32());
        Assert.Equal(16, report.RootElement.GetProperty("ablation_control_counts").GetProperty("total").GetInt32());
        Assert.Equal(0, report.RootElement.GetProperty("ablation_control_counts").GetProperty("shortcut_leaks").GetInt32());
        AssertSafetyFlagsFalse(report.RootElement.GetProperty("safety_flags"));

        using var selfCheck = JsonDocument.Parse(File.ReadAllText(Path.Combine(p9bDirectory, "p9b-self-check.json"), Encoding.UTF8));
        Assert.False(selfCheck.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("p8_pass_claimed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("full_api_semantic_quality_claimed").GetBoolean());
        Assert.False(selfCheck.RootElement.GetProperty("final_model_ready_claimed").GetBoolean());

        AssertDirectoryHasNoMojibake(p9bDirectory);
    }

    private static void AssertSafetyFlagsFalse(JsonElement safetyFlags)
    {
        Assert.False(safetyFlags.GetProperty("training_run").GetBoolean());
        Assert.False(safetyFlags.GetProperty("eval_holdout_rows_used_as_training").GetBoolean());
        Assert.False(safetyFlags.GetProperty("checkpoint_rewritten").GetBoolean());
        Assert.False(safetyFlags.GetProperty("final_common_model_overwritten").GetBoolean());
        Assert.False(safetyFlags.GetProperty("production_code_changed").GetBoolean());
        Assert.False(safetyFlags.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(safetyFlags.GetProperty("p8_pass_claimed").GetBoolean());
        Assert.False(safetyFlags.GetProperty("full_api_semantic_quality_claimed").GetBoolean());
        Assert.False(safetyFlags.GetProperty("final_model_ready_claimed").GetBoolean());
    }

    private static IReadOnlyList<JsonDocument> ReadJsonLines(string path)
        => File.ReadAllLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line))
            .ToArray();

    private static OrchestrationReasoningRequest CreateReservedPositiveRequest(
        string rowId,
        string expectedPatternId)
        => expectedPatternId switch
        {
            "or-p9a-pattern-branch-review-targeted-repair" => CreateRequest(
                $"p9b-reserved-positive-{rowId}",
                ["diagnose_failure_before_repair", "choose_one_repair_strategy", "targeted_repair_only"],
                ["compare_actual_to_expected", "preserve_unaffected_work"],
                ["repair_strategy_decision", "bounded_repair_plan"],
                ["failure_evidence", "done_criteria_evidence"]),
            "or-p9a-pattern-flow-isolation-no-state-bleed" => CreateRequest(
                $"p9b-reserved-positive-{rowId}",
                ["isolate_scope_on_reads", "prevent_cross_context_contamination", "no_state_bleed"],
                ["apply_scope_filter", "separate_context_boundaries"],
                ["isolation_verdict", "scope_boundary_review"],
                ["scope_filter_evidence", "contamination_risk_evidence"]),
            _ => throw new InvalidOperationException($"Unexpected P9B expected pattern id: {expectedPatternId}")
        };

    private static OrchestrationReasoningRequest CreateAblationRequest(
        string rowId,
        string kind)
    {
        (IReadOnlyList<string> Constraints, IReadOnlyList<string> Requirements, IReadOnlyList<string> Deliverables, IReadOnlyList<string> Evidence) signals = kind switch
        {
            "exact_prompt" => (["exact_prompt_marker_only"], ["verbatim_prompt_lookup"], ["copied_surface_answer"], ["exact_prompt_evidence"]),
            "filename_path_title" => (["path_title_marker_only"], ["route_by_filename"], ["path_based_decision"], ["filename_marker_evidence"]),
            "source_family_id" => (["source_family_marker_only"], ["route_by_source_family"], ["family_based_decision"], ["source_family_evidence"]),
            "row_id_marker" => (["row_id_marker_only"], ["route_by_row_id"], ["row_id_based_decision"], ["row_id_evidence"]),
            "nearest_only_retrieval" => (["nearest_neighbor_only"], ["reuse_nearest_memory"], ["retrieval_only_decision"], ["nearest_only_evidence"]),
            "stack_project_specific_marker" => (["stack_marker_only"], ["route_by_project_marker"], ["project_marker_decision"], ["stack_marker_evidence"]),
            "dependency_order_perturbation" => (["order_surface_changed_only"], ["ignore_prerequisite_relationship"], ["surface_order_decision"], ["perturbation_evidence"]),
            "repair_label_only" => (["repair_label_marker_only"], ["route_by_repair_label"], ["repair_label_decision"], ["repair_label_evidence"]),
            "scope_marker_only" => (["scope_marker_only"], ["route_by_scope_marker"], ["scope_marker_decision"], ["scope_marker_evidence"]),
            _ => throw new InvalidOperationException($"Unexpected P9B ablation kind: {kind}")
        };

        return CreateRequest(
            $"p9b-control-{rowId}-{kind}",
            signals.Item1,
            signals.Item2,
            signals.Item3,
            signals.Item4);
    }

    private static OrchestrationReasoningRequest CreateRequest(
        string requestId,
        IReadOnlyList<string> constraints,
        IReadOnlyList<string> requirements,
        IReadOnlyList<string> deliverables,
        IReadOnlyList<string> evidence)
        => new(
            RequestId: requestId,
            Task: new StructuredTaskContract(
                PromptId: requestId,
                RawPromptSha256: $"sha256:{requestId}",
                UserLanguage: "en",
                TaskType: "orchestration_reasoning",
                ExplicitUserRequirements: requirements,
                Constraints: constraints,
                Deliverables: deliverables,
                ForbiddenActions:
                [
                    "eval_holdout_training",
                    "semantic_quality_claim",
                    "shortcut_authority"
                ],
                RequiredHosts: ["core_only"],
                EvidenceNeeds: evidence,
                SourceProvenance: new Dictionary<string, string>
                {
                    ["p9b_boundary"] = "reserved-positive-or-ablation-control",
                    ["raw_prompt_text"] = "not_stored"
                },
                SplitId: "p9b-source-disjoint-eval"),
            GuideDerivedStateRefs: [],
            CorpusBundleIds: ["p9a-candidate-artifacts"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static void AssertDirectoryHasNoMojibake(string directory)
    {
        var forbiddenMarkers = new[]
        {
            ((char)0x00D0).ToString(),
            ((char)0x00D1).ToString(),
            ((char)0xFFFD).ToString(),
            ((char)0x00E2).ToString(),
            ((char)0x00C3).ToString(),
            new string('?', 4)
        };

        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            foreach (var marker in forbiddenMarkers)
                Assert.DoesNotContain(marker, text);
        }
    }

    private static string RepoPath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. parts]);
    }
}
