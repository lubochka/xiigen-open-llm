// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;

public sealed class SingletonIdentityTests
{
    [Fact]
    public void Seeder_ReturnsSameFlowInstanceAcrossIndependentSeederCalls()
    {
        var firstFlows = SeedFlows();
        var secondFlows = SeedFlows();

        var first = ResolveUniversalFlow(firstFlows, "3.4");
        var second = ResolveUniversalFlow(secondFlows, "3.4");

        Assert.Same(first, second);
    }

    [Fact]
    public void Seeder_ReturnsSameNodeInstancesAcrossFlowLookups()
    {
        var first = ResolveUniversalFlow(SeedFlows(), "3.4");
        var second = ResolveUniversalFlow(SeedFlows(), "3.4");

        Assert.Equal(UniversalNineNodeDevelopmentFlow.NodeDefinitions.Length, first.Units.Length);
        for (var index = 0; index < first.Units.Length; index++)
            Assert.Same(first.Units[index], second.Units[index]);
    }

    [Fact]
    public void Seeder_AllSubPhaseFlowsShareTheSameNodeInstances()
    {
        var flows = SeedFlows();
        var universalFlows = DevelopmentLifecycleTopologySeeder.SubPhaseDefinitions()
            .Select(definition => ResolveUniversalFlow(flows, definition.SubPhaseId))
            .ToArray();

        Assert.Equal(31, universalFlows.Length);
        for (var nodeIndex = 0; nodeIndex < UniversalNineNodeDevelopmentFlow.NodeDefinitions.Length; nodeIndex++)
        {
            var firstNode = universalFlows[0].Units[nodeIndex];
            Assert.All(universalFlows, flow => Assert.Same(firstNode, flow.Units[nodeIndex]));
        }
    }

    [Fact]
    public void Seeder_DoubleSeedDoesNotRegisterDuplicateFlowsForSameSubPhase()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        var first = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
        var second = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.All(
            flows.ListDescriptors().GroupBy(descriptor => descriptor.SubPhaseId, StringComparer.Ordinal),
            group => Assert.Single(group));
    }

    internal static InMemoryLogicalFlowRegistry SeedFlows()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
        Assert.True(seed.IsSuccess, seed.ErrorMessage);
        return flows;
    }

    internal static UniversalNineNodeDevelopmentFlow ResolveUniversalFlow(InMemoryLogicalFlowRegistry flows, string subPhaseId)
    {
        var binding = flows.ResolveBindingBySubPhase(subPhaseId);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        return Assert.IsType<UniversalNineNodeDevelopmentFlow>(binding.Value!.Flow);
    }
}

public sealed class ConcurrentExecutionIsolationTests
{
    [Fact]
    public async Task SameFlow_ConcurrentCallsProduceIndependentOutputs()
    {
        var flow = SingletonIdentityTests.ResolveUniversalFlow(SingletonIdentityTests.SeedFlows(), "4.1");

        var alpha = flow.ExecuteAsync(BuildFlowRequest(flow, "run-alpha", "prompt-alpha"));
        var beta = flow.ExecuteAsync(BuildFlowRequest(flow, "run-beta", "prompt-beta"));
        var results = await Task.WhenAll(alpha, beta);

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Contains("prompt-alpha", results[0].Value!.FinalHumanResult);
        Assert.DoesNotContain("prompt-beta", results[0].Value.FinalHumanResult);
        Assert.Contains("prompt-beta", results[1].Value!.FinalHumanResult);
        Assert.DoesNotContain("prompt-alpha", results[1].Value.FinalHumanResult);
        Assert.All(results, result => Assert.Equal("PASS", result.Value!.OverallVerdict));
    }

    [Fact]
    public async Task SameNode_ConcurrentCallsProduceIndependentStates()
    {
        var node = (UniversalDevelopmentNodeUnit)UniversalNineNodeDevelopmentFlow.SharedNodeUnits[2];
        var alpha = node.ExecuteAsync(BuildNodeRequest("node-alpha-run", "3.1", "alpha"));
        var beta = node.ExecuteAsync(BuildNodeRequest("node-beta-run", "3.2", "beta"));
        var results = await Task.WhenAll(alpha, beta);

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Equal("3.1", results[0].Value!.Output.SubPhaseId);
        Assert.Equal("3.2", results[1].Value!.Output.SubPhaseId);
        Assert.Equal(["node-03-scheme-planner"], results[0].Value.Output.CompletedNodeIds);
        Assert.Equal(["node-03-scheme-planner"], results[1].Value.Output.CompletedNodeIds);
        Assert.Empty(results[0].Value.Output.EvidenceIds.Intersect(results[1].Value.Output.EvidenceIds, StringComparer.Ordinal));
    }

    [Fact]
    public async Task SameFlow_EightConcurrentBranchesDoNotCrossContaminate()
    {
        var flow = SingletonIdentityTests.ResolveUniversalFlow(SingletonIdentityTests.SeedFlows(), "3.4");
        var tasks = Enumerable.Range(0, 8)
            .Select(index => flow.ExecuteAsync(BuildFlowRequest(flow, $"branch-run-{index}", $"branch-{index}")))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        for (var index = 0; index < results.Length; index++)
        {
            var text = results[index].Value!.FinalHumanResult;
            Assert.Contains($"branch-{index}", text);
            foreach (var other in Enumerable.Range(0, results.Length).Where(other => other != index))
                Assert.DoesNotContain($"branch-{other}", text);
        }

        var evidenceIds = results.SelectMany(result => result.Value!.EvidenceIds).ToArray();
        Assert.Equal(evidenceIds.Length, evidenceIds.Distinct(StringComparer.Ordinal).Count());
    }

    private static LogicalFlowRequest<string> BuildFlowRequest(
        UniversalNineNodeDevelopmentFlow flow,
        string runId,
        string input)
        => new(
            runId,
            $"{UniversalNineNodeDevelopmentFlow.FlowIdPrefix}-{flow.SubPhaseId}",
            $"{UniversalNineNodeDevelopmentFlow.ScenarioIdPrefix}-{flow.SubPhaseId}",
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

    private static LogicalUnitRequest<UniversalNodeState> BuildNodeRequest(string runId, string subPhaseId, string prompt)
        => new(
            runId,
            "node-03-scheme-planner",
            "scheme-planner",
            "software-development",
            new UniversalNodeState(
                subPhaseId,
                subPhaseId.Split('.')[0],
                "test-task",
                prompt,
                prompt,
                [],
                ["ARCHITECTURE"],
                [],
                []),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
}

public sealed class CrossBatchIsolationTests
{
    [Fact]
    public async Task FlowExecutionFromPriorBatchLeavesNoTraceInNextBatch()
    {
        var flow = SingletonIdentityTests.ResolveUniversalFlow(SingletonIdentityTests.SeedFlows(), "4.2");

        var first = await flow.ExecuteAsync(BuildFlowRequest(flow, "batch-one-run", "batch-one-prompt"));
        var second = await flow.ExecuteAsync(BuildFlowRequest(flow, "batch-two-run", "batch-two-prompt"));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Contains("batch-two-prompt", second.Value!.FinalHumanResult);
        Assert.DoesNotContain("batch-one-prompt", second.Value.FinalHumanResult);
        Assert.Empty(first.Value!.EvidenceIds.Intersect(second.Value.EvidenceIds, StringComparer.Ordinal));
        Assert.Equal(
            UniversalNineNodeDevelopmentFlow.NodeDefinitions.Select(definition => definition.NodeId),
            second.Value.Metrics["completed_node_ids"].Split(';', StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public async Task NodeStateDoesNotAccumulateAcrossRepeatedCalls()
    {
        var node = (UniversalDevelopmentNodeUnit)UniversalNineNodeDevelopmentFlow.SharedNodeUnits[8];

        for (var index = 0; index < 10; index++)
        {
            var result = await node.ExecuteAsync(BuildNodeRequest($"formatter-run-{index}", $"7.{index}", $"prompt-{index}"));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(["node-09-formatter"], result.Value!.Output.CompletedNodeIds);
            Assert.All(result.Value.Output.EvidenceIds, evidenceId => Assert.Contains($"formatter-run-{index}", evidenceId));
        }
    }

    [Fact]
    public async Task FlowInstanceDoesNotDependOnRegistryThatRegisteredIt()
    {
        var batchOneFlow = SingletonIdentityTests.ResolveUniversalFlow(SingletonIdentityTests.SeedFlows(), "3.3");
        _ = SingletonIdentityTests.SeedFlows();

        var result = await batchOneFlow.ExecuteAsync(BuildFlowRequest(batchOneFlow, "stale-reference-run", "execute contract design"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("PASS", result.Value!.OverallVerdict);
        Assert.Equal("3.3", result.Value.Metrics["sub_phase_id"]);
    }

    private static LogicalFlowRequest<string> BuildFlowRequest(
        UniversalNineNodeDevelopmentFlow flow,
        string runId,
        string input)
        => new(
            runId,
            $"{UniversalNineNodeDevelopmentFlow.FlowIdPrefix}-{flow.SubPhaseId}",
            $"{UniversalNineNodeDevelopmentFlow.ScenarioIdPrefix}-{flow.SubPhaseId}",
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

    private static LogicalUnitRequest<UniversalNodeState> BuildNodeRequest(string runId, string subPhaseId, string prompt)
        => new(
            runId,
            "node-09-formatter",
            "formatter",
            "software-development",
            new UniversalNodeState(
                subPhaseId,
                subPhaseId.Split('.')[0],
                "test-task",
                prompt,
                prompt,
                [],
                ["DOCUMENT"],
                [],
                []),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
}

public sealed class DeduplicationTests
{
    [Fact]
    public void MultipleRowsWithSameSubPhaseResolveSameFlow()
    {
        var flows = SingletonIdentityTests.SeedFlows();
        var resolved = Enumerable.Range(0, 5)
            .Select(_ => SingletonIdentityTests.ResolveUniversalFlow(flows, "3.4"))
            .ToArray();

        Assert.All(resolved.Skip(1), flow => Assert.Same(resolved[0], flow));
    }

    [Fact]
    public async Task TrainerRegistersOnlyOneGeneratedFlowPerDuplicateSubPhaseInBatch()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        AddSubPhase(topology, "3.1", "Architecture and Component Topology", ["ARCHITECTURE"]);
        AddSubPhase(topology, "3.4", "Security and Authorization Model", ["SECURITY", "AUTHORIZATION"]);
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var checkpointRoot = Path.Combine(Path.GetTempPath(), $"singleton-dedup-{Guid.NewGuid():N}");
        var trainer = new DefaultLogicalCapabilityTrainer(checkpointRoot);

        try
        {
            var branches = Enumerable.Range(0, 3)
                .SelectMany(index => new[] { BuildKnowledgeEdgeBranch("3.1", index), BuildKnowledgeEdgeBranch("3.4", index) })
                .ToArray();
            var request = new LogicalCapabilityTrainingRequest(
                "singleton-dedup-run",
                new LogicalOrchestratorRequest(
                    "singleton-dedup-run",
                    AdaptiveLogicalOrchestrator.ScenarioId,
                    "Design security architecture and authorization topology.",
                    new LogicalExecutionFlags(),
                    new Dictionary<string, object>()),
                branches,
                topology,
                flows,
                units)
            {
                Domain = Domain.ARCH
            };

            var result = await trainer.TrainAndRegisterAsync(request);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var generated = flows.ListDescriptors()
                .Where(descriptor => descriptor.FlowId.StartsWith(DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix, StringComparison.Ordinal))
                .ToArray();
            Assert.Single(generated.Where(descriptor => descriptor.SubPhaseId == "3.1"));
            Assert.Single(generated.Where(descriptor => descriptor.SubPhaseId == "3.4"));
            Assert.Equal(generated.Length, generated.Select(descriptor => descriptor.FlowId).Distinct(StringComparer.Ordinal).Count());
        }
        finally
        {
            if (Directory.Exists(checkpointRoot))
                Directory.Delete(checkpointRoot, recursive: true);
        }
    }

    [Fact]
    public void UniversalFlowsAlwaysHaveExactlyNineCanonicalNodes()
    {
        var flows = SingletonIdentityTests.SeedFlows();
        var universalFlows = DevelopmentLifecycleTopologySeeder.SubPhaseDefinitions()
            .Select(definition => SingletonIdentityTests.ResolveUniversalFlow(flows, definition.SubPhaseId))
            .ToArray();

        Assert.All(universalFlows, flow =>
        {
            Assert.Equal(9, flow.Units.Length);
            for (var index = 0; index < flow.Units.Length; index++)
            {
                var node = Assert.IsType<UniversalDevelopmentNodeUnit>(flow.Units[index]);
                Assert.Equal(UniversalNineNodeDevelopmentFlow.NodeDefinitions[index].NodeId, node.NodeId);
            }
        });
    }

    private static void AddSubPhase(InMemoryLogicalTopologyRegistry topology, string subPhaseId, string name, string[] predicates)
    {
        var result = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            subPhaseId,
            subPhaseId.Split('.')[0],
            name,
            $"Test sub-phase {subPhaseId}.",
            ["software-development"],
            predicates,
            ["singleton-dedup-test"]));
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private static LogicalFlowResult BuildKnowledgeEdgeBranch(string subPhaseId, int index)
    {
        var summary = new LogicalFlowSummary(
            $"summary-{subPhaseId}-{index}",
            1,
            0,
            0,
            "knowledge_edge",
            false,
            false,
            false,
            "GENERATE_CAPABILITY",
            0.70,
            $"missing_sub_phase={subPhaseId}",
            "Capability is missing for routed sub-phase.",
            0,
            0);

        return new LogicalFlowResult(
            $"missing-flow-{subPhaseId}-{index}",
            "missing-capability",
            "KNOWLEDGE_EDGE",
            $"knowledge_edge: missing generated capability for {subPhaseId}",
            summary,
            new Dictionary<string, string> { ["missing_sub_phase"] = subPhaseId },
            [],
            []);
    }
}
