// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningP9EvalAblationTests
{
    private static readonly string[] ForbiddenTrainingRows =
    [
        "P5E-DPO-ORCH-003",
        "P5E-DPO-ORCH-005",
        "P4-ORCH-DPO-DRAFT-001",
        "P4-ORCH-DPO-DRAFT-002"
    ];

    [Fact]
    public async Task P8BCheckpointCandidate_FreshLoadsAndProducesP9FailEvidenceWithoutShortcutLeaks()
    {
        var checkpointPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p8b-orchestration-reasoning-checkpoint-candidate",
            "orchestration-reasoning-p8b-candidate-checkpoint.json");
        var manifestPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p8b-orchestration-reasoning-checkpoint-candidate",
            "orchestration-reasoning-p8b-candidate-manifest.json");

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);
        Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);
        var machine = loadResult.Value!.Machine;
        var checkpoint = loadResult.Value.Checkpoint;

        Assert.False(checkpoint.SemanticQualityClaimed);
        Assert.False(checkpoint.ContinueTrainingClaimed);
        Assert.False(checkpoint.HostRuntimeClaimed);
        Assert.Equal(3, checkpoint.LearnedState.LearnedPatterns!.Count);

        var learnedPatternJson = JsonSerializer.Serialize(checkpoint.LearnedState.LearnedPatterns);
        foreach (var forbiddenRow in ForbiddenTrainingRows)
        {
            Assert.DoesNotContain(forbiddenRow, checkpoint.AcceptedRefIds);
            Assert.DoesNotContain(forbiddenRow, learnedPatternJson);
        }

        var reservedPositiveRequests = new[]
        {
            CreateRequest(
                "p9-reserved-positive-P5E-DPO-ORCH-003",
                ["separate_branches", "risk_review_before_repair", "targeted_repair_only"],
                ["choose_branch_after_review", "preserve_approved_fragments"],
                ["branch_review_decision", "targeted_repair_plan"],
                ["risk_review_evidence", "failed_fragment_evidence"]),
            CreateRequest(
                "p9-reserved-positive-P5E-DPO-ORCH-005",
                ["isolate_independent_flows", "prevent_state_bleed", "no_cross_flow_contamination"],
                ["keep_flow_state_separate", "deduplicate_without_merging_context"],
                ["isolated_flow_plan", "state_boundary_report"],
                ["flow_isolation_evidence", "deduplication_evidence"])
        };

        foreach (var request in reservedPositiveRequests)
        {
            var response = machine.Plan(request);
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.Equal(OrchestrationReasoningMachine.NoMatchingPatternStatus, response.Value!.ContractStatus);
            Assert.Empty(response.Value.ScenarioPlan.SelectedLearnedPatterns);
        }

        foreach (var request in BuildAblationRequests())
        {
            var response = machine.Plan(request);
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.NotEqual(OrchestrationReasoningMachine.LearnedPatternStatus, response.Value!.ContractStatus);
            Assert.Empty(response.Value.ScenarioPlan.SelectedLearnedPatterns);
        }
    }

    private static IEnumerable<OrchestrationReasoningRequest> BuildAblationRequests()
    {
        var rows = new[] { "P5E-DPO-ORCH-003", "P5E-DPO-ORCH-005" };
        var controlSignals = new (string Kind, string[] Constraints, string[] Requirements, string[] Deliverables, string[] Evidence)[]
        {
            ("exact_prompt", ["exact_prompt_marker_only"], ["verbatim_prompt_lookup"], ["copied_surface_answer"], ["exact_prompt_evidence"]),
            ("filename_path_title", ["path_title_marker_only"], ["route_by_filename"], ["path_based_decision"], ["filename_marker_evidence"]),
            ("source_family_id", ["source_family_marker_only"], ["route_by_source_family"], ["family_based_decision"], ["source_family_evidence"]),
            ("row_id_marker", ["row_id_marker_only"], ["route_by_row_id"], ["row_id_based_decision"], ["row_id_evidence"]),
            ("nearest_only_retrieval", ["nearest_neighbor_only"], ["reuse_nearest_memory"], ["retrieval_only_decision"], ["nearest_only_evidence"]),
            ("stack_project_specific_marker", ["stack_marker_only"], ["route_by_project_marker"], ["project_marker_decision"], ["stack_marker_evidence"]),
            ("dependency_order_perturbation", ["order_surface_changed_only"], ["ignore_prerequisite_relationship"], ["surface_order_decision"], ["perturbation_evidence"])
        };

        foreach (var row in rows)
        {
            foreach (var control in controlSignals)
            {
                yield return CreateRequest(
                    $"p9-control-{row}-{control.Kind}",
                    control.Constraints,
                    control.Requirements,
                    control.Deliverables,
                    control.Evidence);
            }
        }
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
                UserLanguage: "ru",
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
                    ["p9_boundary"] = "reserved-positive-or-ablation-control",
                    ["raw_prompt_text"] = "not_stored"
                },
                SplitId: "p9-source-disjoint-eval"),
            GuideDerivedStateRefs: [],
            CorpusBundleIds: ["p8b-candidate-artifacts"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static string RepoPath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. parts]);
    }
}
