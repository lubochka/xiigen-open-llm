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

        Assert.True(loadResult.IsSuccess, loadResult.ErrorMessage);
        var checkpoint = loadResult.Value!.Checkpoint;
        Assert.Equal(5, checkpoint.LearnedState.LearnedPatterns!.Count);
        Assert.False(checkpoint.SemanticQualityClaimed);
        Assert.False(checkpoint.ContinueTrainingClaimed);
        Assert.False(checkpoint.HostRuntimeClaimed);
        Assert.False(loadResult.Value.Manifest.SemanticQualityClaimed);

        var learnedPatternJson = JsonSerializer.Serialize(checkpoint.LearnedState.LearnedPatterns);
        foreach (var forbiddenRow in ForbiddenTrainingRows)
        {
            Assert.DoesNotContain(forbiddenRow, checkpoint.AcceptedRefIds);
            Assert.DoesNotContain(forbiddenRow, learnedPatternJson);
        }

        Assert.Contains("P9A-ORCH-TRAIN-001", checkpoint.AcceptedRefIds);
        Assert.Contains("P9A-ORCH-TRAIN-002", checkpoint.AcceptedRefIds);

        var repairPatterns = checkpoint.LearnedState.LearnedPatterns
            .Where(pattern => pattern.PatternId.StartsWith("or-p9a-pattern-", StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(2, repairPatterns.Length);

        foreach (var pattern in repairPatterns)
        {
            var response = loadResult.Value.Machine.Plan(CreateMatchingRequest(pattern));
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, response.Value!.ContractStatus);
            Assert.Equal([pattern.PatternId], response.Value.ScenarioPlan.SelectedLearnedPatterns);
            Assert.Contains("SEMANTIC_QUALITY_CLAIMED_FALSE", response.Value.ContractEvidence);
        }
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
