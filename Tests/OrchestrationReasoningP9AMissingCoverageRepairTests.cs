// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningP9AMissingCoverageRepairTests
{
    private static readonly string[] ForbiddenTrainingRows =
    [
        "P5E-DPO-ORCH-003",
        "P5E-DPO-ORCH-005",
        "P4-ORCH-DPO-DRAFT-001",
        "P4-ORCH-DPO-DRAFT-002"
    ];

    [Fact]
    public async Task P9ACandidate_FreshLoadsWithoutOverclaimOrForbiddenTrainingRows()
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

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);

        Assert.False(loadResult.IsSuccess);
        Assert.Equal(
            "ORCHESTRATION_REASONING_CHECKPOINT_SOURCE_PROVENANCE_REBUILD_REQUIRED",
            loadResult.ErrorCode);
    }

    private static OrchestrationReasoningRequest CreateMatchingRequest(
        OrchestrationReasoningLearnedPattern pattern)
        => new(
            RequestId: $"p9a-fresh-load-proof-{pattern.PatternId}",
            Task: new StructuredTaskContract(
                PromptId: $"p9a-fresh-load-proof-{pattern.PatternId}",
                RawPromptSha256: $"sha256:p9a-fresh-load-proof-{pattern.PatternId}",
                UserLanguage: "en",
                TaskType: pattern.RequiredTaskTypes[0],
                ExplicitUserRequirements: pattern.RequiredRequirementSignals,
                Constraints: pattern.RequiredConstraintSignals,
                Deliverables: pattern.RequiredDeliverableSignals,
                ForbiddenActions:
                [
                    "eval_holdout_training",
                    "semantic_quality_claim",
                    "source_marker_authority"
                ],
                RequiredHosts: ["core_only"],
                EvidenceNeeds: pattern.RequiredEvidenceSignals,
                SourceProvenance: new Dictionary<string, string>
                {
                    ["fresh_load_evidence"] = "p9a-focused-test",
                    ["raw_prompt_text"] = "not_stored"
                },
                SplitId: "p9a-fresh-load"),
            GuideDerivedStateRefs: [pattern.PatternId],
            CorpusBundleIds: ["p9a-candidate-artifacts"],
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
