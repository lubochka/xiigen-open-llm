// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningP8BCheckpointCandidateTests
{
    private static readonly string[] AllowedTrainRows =
    [
        "P5E-DPO-ORCH-001",
        "P5E-DPO-ORCH-002",
        "P5E-DPO-ORCH-004"
    ];

    private static readonly string[] ForbiddenTrainingRows =
    [
        "P5E-DPO-ORCH-003",
        "P5E-DPO-ORCH-005",
        "P4-ORCH-DPO-DRAFT-001",
        "P4-ORCH-DPO-DRAFT-002"
    ];

    [Fact]
    public async Task OrchestrationReasoningP8BCheckpointCandidate_FreshLoadsWithoutOverclaimOrForbiddenTrainingRows()
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
        var sourceManifestPath = RepoPath(
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p8b-orchestration-reasoning-checkpoint-candidate",
            "orchestration-reasoning-p8b-source-manifest.json");

        var loadResult = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);

        Assert.False(loadResult.IsSuccess);
        Assert.Equal(
            "ORCHESTRATION_REASONING_CHECKPOINT_SOURCE_PROVENANCE_REBUILD_REQUIRED",
            loadResult.ErrorCode);
    }

    private static OrchestrationReasoningRequest CreateMatchingRequest(
        OrchestrationReasoningLearnedPattern pattern)
        => new(
            RequestId: "p8b-fresh-load-proof",
            Task: new StructuredTaskContract(
                PromptId: "p8b-fresh-load-proof",
                RawPromptSha256: "sha256:p8b-fresh-load-proof",
                UserLanguage: "en",
                TaskType: pattern.RequiredTaskTypes[0],
                ExplicitUserRequirements: pattern.RequiredRequirementSignals,
                Constraints: pattern.RequiredConstraintSignals,
                Deliverables: pattern.RequiredDeliverableSignals,
                ForbiddenActions:
                [
                    "eval_holdout_training",
                    "semantic_quality_claim"
                ],
                RequiredHosts: ["core_only"],
                EvidenceNeeds: pattern.RequiredEvidenceSignals,
                SourceProvenance: new Dictionary<string, string>
                {
                    ["fresh_load_evidence"] = "p8b-focused-test",
                    ["raw_prompt_text"] = "not_stored"
                },
                SplitId: "p8b-fresh-load"),
            GuideDerivedStateRefs: [pattern.PatternId],
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
