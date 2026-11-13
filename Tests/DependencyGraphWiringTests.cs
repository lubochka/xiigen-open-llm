// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;

public sealed class DependencyGraphWiringTests
{
    [Fact]
    public void SemanticCompiler29DependencyGraph_OrdersPlanBeforeArchitecture()
    {
        var graph = SemanticCompiler29.DependencyGraph.BuildDefault(new TestCompilerStorage(), new TestCompilerLogger());

        var sorted = graph.Sort([Domain.ARCH, Domain.PLAN]);

        Assert.True(sorted.IsSuccess, sorted.ErrorMessage);
        Assert.Equal(Domain.PLAN, sorted.Value![0][0]);
        Assert.Equal(Domain.ARCH, sorted.Value[1][0]);
    }

    [Fact]
    public void SemanticCompiler29DependencyGraph_OrdersCodeBeforeBug()
    {
        var graph = SemanticCompiler29.DependencyGraph.BuildDefault(new TestCompilerStorage(), new TestCompilerLogger());

        var sorted = graph.Sort([Domain.BUG, Domain.CODE_Q]);

        Assert.True(sorted.IsSuccess, sorted.ErrorMessage);
        Assert.Equal(Domain.CODE_Q, sorted.Value![0][0]);
        Assert.Equal(Domain.BUG, sorted.Value[1][0]);
    }

    [Fact]
    public void AdaptiveOrchestrator_OrderByTransitionsUsesE35DependencyGraphWithoutTopologyEdges()
    {
        var topology = BuildE35OnlyTopology();
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            new InMemoryLogicalFlowRegistry(),
            new InMemoryLogicalUnitRegistry());
        var decisions = new[]
        {
            Decision("4.1", "4", ["IMPLEMENT", "E35_DEPENDENCY_GRAPH"]),
            Decision("3.1", "3", ["ARCHITECTURE", "E35_DEPENDENCY_GRAPH"]),
            Decision("2.1", "2", ["DOCUMENT", "E35_DEPENDENCY_GRAPH"])
        };

        var ordered = InvokeOrderByTransitions(orchestrator, decisions);

        Assert.Equal(["2.1", "3.1", "4.1"], ordered.Select(decision => decision.PredictedSubPhaseId).ToArray());
    }

    [Fact]
    public void AdaptiveOrchestrator_OrderByTransitionsPreservesParallelOrderWithoutE35Evidence()
    {
        var topology = BuildE35OnlyTopology();
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            new InMemoryLogicalFlowRegistry(),
            new InMemoryLogicalUnitRegistry());
        var decisions = new[]
        {
            Decision("4.1", "4", ["IMPLEMENT"]),
            Decision("3.1", "3", ["ARCHITECTURE"]),
            Decision("2.1", "2", ["DOCUMENT"])
        };

        var ordered = InvokeOrderByTransitions(orchestrator, decisions);

        Assert.Equal(["4.1", "3.1", "2.1"], ordered.Select(decision => decision.PredictedSubPhaseId).ToArray());
    }

    private static InMemoryLogicalTopologyRegistry BuildE35OnlyTopology()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase("2.1", "2", "Plan", "Business planning", ["software-development"], ["DOCUMENT"], ["test"])).IsSuccess);
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase("3.1", "3", "Architecture", "Architecture design", ["software-development"], ["ARCHITECTURE"], ["test"])).IsSuccess);
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase("4.1", "4", "Backend Services", "Implementation work", ["software-development"], ["IMPLEMENT"], ["test"])).IsSuccess);
        return topology;
    }

    private static SubPhaseRoutingDecision Decision(string subPhaseId, string phaseId, string[] predicates)
        => new("dependency-graph-doc", "ready", phaseId, subPhaseId, 0.9, 0.0, predicates, false, "test");

    private static SubPhaseRoutingDecision[] InvokeOrderByTransitions(
        AdaptiveLogicalOrchestrator orchestrator,
        SubPhaseRoutingDecision[] decisions)
    {
        var method = typeof(AdaptiveLogicalOrchestrator).GetMethod(
            "OrderByTransitions",
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: [typeof(SubPhaseRoutingDecision[]), typeof(LogicalExecutionFlags)],
            modifiers: null);
        Assert.NotNull(method);
        return Assert.IsType<SubPhaseRoutingDecision[]>(method.Invoke(orchestrator, [decisions, new LogicalExecutionFlags { UseE35DependencyDetection = true }]));
    }

    private sealed class TestCompilerStorage : SemanticCompiler29.ICompilerStorage
    {
        private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);

        public void Save(string key, SemanticCompiler29.PipelineOutput output)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _items[key] = output;
        }

        public SemanticCompiler29.PipelineOutput? Load(string key)
            => string.IsNullOrWhiteSpace(key) ? null : _items.GetValueOrDefault(key);
    }

    private sealed class TestCompilerLogger : SemanticCompiler29.ICompilerEventLogger
    {
        public void Log(SemanticCompiler29.StandardEvent evt)
        {
        }
    }
}
