// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using SemanticCompiler;
using Domain = SemanticCompiler29.Domain;

public sealed class DefaultLogicalCapabilityTrainerClearCacheTests
{
    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_ClearCache_ForcesNextStepToLoadFromCheckpoint()
    {
        var topology = BuildTopology();
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-clear-cache-{Guid.NewGuid():N}");
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);

        try
        {
            var first = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "clear-cache-row-0001",
                "Fix the Figma export quality issue.",
                topology,
                flows,
                units));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.Single(first.Value!.BranchTimings);
            Assert.False(first.Value.BranchTimings[0].UsedWarmCache);
            Assert.Equal(1, first.Value.BranchTimings[0].TrainingStepCount);

            var second = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "clear-cache-row-0002",
                "Fix another Figma export quality issue.",
                topology,
                flows,
                units));
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.Single(second.Value!.BranchTimings);
            Assert.True(second.Value.BranchTimings[0].UsedWarmCache);
            Assert.Equal(2, second.Value.BranchTimings[0].TrainingStepCount);

            trainer.ClearCache();

            var third = await trainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "clear-cache-row-0003",
                "Fix the next Figma export quality issue.",
                topology,
                flows,
                units));
            Assert.True(third.IsSuccess, third.ErrorMessage);
            Assert.Single(third.Value!.BranchTimings);
            Assert.False(third.Value.BranchTimings[0].UsedWarmCache);
            Assert.Equal(3, third.Value.BranchTimings[0].TrainingStepCount);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DefaultLogicalCapabilityTrainer_FreshRegistryContinuesFromPersistedDomainManifest()
    {
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"default-capability-fresh-registry-{Guid.NewGuid():N}");

        try
        {
            var firstTopology = BuildTopology();
            var firstFlows = new InMemoryLogicalFlowRegistry();
            var firstUnits = new InMemoryLogicalUnitRegistry();
            var firstTrainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);

            var first = await firstTrainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "fresh-registry-row-0001",
                "Fix the Figma export quality issue.",
                firstTopology,
                firstFlows,
                firstUnits));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.Single(first.Value!.BranchTimings);
            Assert.Equal(1, first.Value.BranchTimings[0].TrainingStepCount);

            var secondTopology = BuildTopology();
            var secondFlows = new InMemoryLogicalFlowRegistry();
            var secondUnits = new InMemoryLogicalUnitRegistry();
            var secondTrainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);

            var second = await secondTrainer.TrainAndRegisterAsync(BuildTrainingRequest(
                "fresh-registry-row-0002",
                "Fix another Figma export quality issue.",
                secondTopology,
                secondFlows,
                secondUnits));
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.Single(second.Value!.BranchTimings);
            Assert.False(second.Value.BranchTimings[0].UsedWarmCache);
            Assert.Equal(2, second.Value.BranchTimings[0].TrainingStepCount);

            var snapshot = TeachingRegistryPersistence.Capture("fresh-registry-test", 2, secondUnits, secondFlows, checkpointRoot);
            Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
            var manifests = snapshot.Value!.GeneratedCapabilities
                .Where(manifest => manifest.SubPhaseId == "4.4")
                .OrderBy(manifest => manifest.CreatedAtUtc)
                .ToArray();
            Assert.Equal(2, manifests.Length);
            Assert.Equal(manifests[0].UnitId, manifests[1].ParentUnitId);
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    private static InMemoryLogicalTopologyRegistry BuildTopology()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var subPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.4",
            "4",
            "Refactoring and Code Quality",
            "Owns internal code improvement, simplification, dependency cleanup, and maintainability.",
            ["software-development"],
            ["IMPLEMENT"],
            ["default-capability-trainer-clear-cache-test"]));
        Assert.True(subPhase.IsSuccess, subPhase.ErrorMessage);
        return topology;
    }

    private static LogicalCapabilityTrainingRequest BuildTrainingRequest(
        string runId,
        string userRequest,
        InMemoryLogicalTopologyRegistry topology,
        InMemoryLogicalFlowRegistry flows,
        InMemoryLogicalUnitRegistry units)
    {
        var summary = new LogicalFlowSummary(
            $"summary-{runId}",
            1,
            0,
            0,
            "knowledge_edge",
            false,
            false,
            false,
            "GENERATE_CAPABILITY",
            0.70,
            "missing_sub_phase=4.4",
            "Capability is missing for routed sub-phase.",
            0,
            0);

        var branch = new LogicalFlowResult(
            $"missing-flow-{runId}",
            "missing-capability",
            "KNOWLEDGE_EDGE",
            "knowledge_edge: missing generated capability",
            summary,
            new Dictionary<string, string> { ["missing_sub_phase"] = "4.4" },
            [],
            []);

        return new LogicalCapabilityTrainingRequest(
            runId,
            new LogicalOrchestratorRequest(
                runId,
                AdaptiveLogicalOrchestrator.ScenarioId,
                userRequest,
                new LogicalExecutionFlags(),
                new Dictionary<string, object>()),
            [branch],
            topology,
            flows,
            units)
        {
            Domain = PromptDomainClassifier.Classify(userRequest).Domain
        };
    }
}
