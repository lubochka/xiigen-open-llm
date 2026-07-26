// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DeterministicDynamicGraphTopologyBuilderTests
{
    private const string TenantId = "tenant-a";
    private const string DomainTag = "dynamic-graph";
    private const string ContextId = "context-a";
    private const string SignalRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SIGNAL_REQUIRED";
    private const string SignalInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SIGNAL_INVALID";
    private const string BudgetRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_REQUIRED";
    private const string BudgetInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID";
    private const string ScopeMismatchCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SCOPE_MISMATCH";
    private const string BudgetExceededCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_EXCEEDED";
    private const string ExceptionCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_EXCEPTION";

    private static readonly string[] RequiredLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void Contracts_ExposeExactBuilderBoundaryWithoutTrainingSurface()
    {
        Assert.Equal(
            [
                DynamicGraphBuildSourceKind.DeterministicBootstrap,
                DynamicGraphBuildSourceKind.LogicalRegistryProjection,
                DynamicGraphBuildSourceKind.MachineTopologyProposal
            ],
            Enum.GetValues<DynamicGraphBuildSourceKind>());

        AssertRecordShape<DynamicGraphBuildSignal>(
            [
                ("SignalId", typeof(string)),
                ("SourceKind", typeof(DynamicGraphBuildSourceKind)),
                ("ProposedTopology", typeof(DynamicGraphTopology)),
                ("PreviousTopology", typeof(DynamicGraphTopology)),
                ("SourceRef", typeof(string)),
                ("TraceRef", typeof(string))
            ]);
        AssertRecordShape<TopologyBuildBudget>(
            [
                ("MaxNodes", typeof(int)),
                ("MaxEdges", typeof(int)),
                ("MaxDepth", typeof(int)),
                ("MaxParallelBranches", typeof(int))
            ]);
        AssertRecordShape<TopologyBudgetProof>(
            [
                ("MaxNodes", typeof(int)),
                ("ActualNodes", typeof(int)),
                ("MaxEdges", typeof(int)),
                ("ActualEdges", typeof(int)),
                ("MaxDepth", typeof(int)),
                ("ActualDepth", typeof(int)),
                ("MaxParallelBranches", typeof(int)),
                ("ActualParallelBranches", typeof(int)),
                ("WithinBudget", typeof(bool)),
                ("ExceededDimensions", typeof(IReadOnlyList<string>))
            ]);
        AssertRecordShape<TopologyMutationEvidence>(
            [
                ("SignalId", typeof(string)),
                ("SourceKind", typeof(DynamicGraphBuildSourceKind)),
                ("SourceRef", typeof(string)),
                ("TraceRef", typeof(string)),
                ("ProposedAddedNodeIds", typeof(IReadOnlyList<string>)),
                ("ProposedRemovedNodeIds", typeof(IReadOnlyList<string>)),
                ("ProposedUpdatedNodeIds", typeof(IReadOnlyList<string>)),
                ("ProposedAddedEdgeIds", typeof(IReadOnlyList<string>)),
                ("ProposedRemovedEdgeIds", typeof(IReadOnlyList<string>)),
                ("ProposedUpdatedEdgeIds", typeof(IReadOnlyList<string>)),
                ("ProposedReorderedNodeIds", typeof(IReadOnlyList<string>)),
                ("MutationApplied", typeof(bool))
            ]);
        AssertRecordShape<BuildDynamicTopologyResult>(
            [
                ("Topology", typeof(DynamicGraphTopology)),
                ("BudgetProof", typeof(TopologyBudgetProof)),
                ("FailureReason", typeof(string)),
                ("MutationEvidence", typeof(TopologyMutationEvidence))
            ]);

        var method = Assert.Single(typeof(IDynamicGraphTopologyBuilder).GetMethods());
        Assert.Equal(nameof(IDynamicGraphTopologyBuilder.BuildDynamicTopology), method.Name);
        Assert.Equal(typeof(OperationResult<BuildDynamicTopologyResult>), method.ReturnType);
        Assert.Equal(
            [typeof(DynamicGraphBuildSignal), typeof(TopologyBuildBudget)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [typeof(IDynamicGraphTopologyBuilder)],
            typeof(DeterministicDynamicGraphTopologyBuilder).GetInterfaces());
        Assert.DoesNotContain(
            typeof(IDynamicGraphTopologyBuilder).GetMembers(),
            member => member.Name.Contains("Train", StringComparison.OrdinalIgnoreCase));
        var boundedTypes = new[]
        {
            typeof(DynamicGraphBuildSourceKind),
            typeof(DynamicGraphBuildSignal),
            typeof(TopologyBuildBudget),
            typeof(TopologyBudgetProof),
            typeof(TopologyMutationEvidence),
            typeof(BuildDynamicTopologyResult),
            typeof(IDynamicGraphTopologyBuilder),
            typeof(DeterministicDynamicGraphTopologyBuilder)
        };
        Assert.DoesNotContain(
            boundedTypes.SelectMany(type => type.GetMembers()),
            member => member.Name.Contains("Train", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Build_ValidInitialTopologyReturnsFullSortedAddedEvidence()
    {
        var proposed = CreateTopology(
            [CreateNode("node-b"), CreateNode("node-a")],
            [CreateEdge("edge-z", "node-a", ["node-b"])],
            [CreateOrder("node-b", 1), CreateOrder("node-a", 0)]);
        var result = Build(CreateSignal(proposed, DynamicGraphBuildSourceKind.DeterministicBootstrap));

        Assert.Null(result.FailureReason);
        Assert.NotNull(result.Topology);
        Assert.NotSame(proposed, result.Topology);
        Assert.True(result.BudgetProof.WithinBudget);
        Assert.Equal(2, result.BudgetProof.ActualNodes);
        Assert.Equal(1, result.BudgetProof.ActualEdges);
        Assert.Equal(1, result.BudgetProof.ActualDepth);
        Assert.Equal(1, result.BudgetProof.ActualParallelBranches);
        Assert.Equal(["node-a", "node-b"], result.MutationEvidence.ProposedAddedNodeIds);
        Assert.Equal(["edge-z"], result.MutationEvidence.ProposedAddedEdgeIds);
        Assert.Empty(result.MutationEvidence.ProposedRemovedNodeIds);
        Assert.Empty(result.MutationEvidence.ProposedUpdatedNodeIds);
        Assert.Empty(result.MutationEvidence.ProposedRemovedEdgeIds);
        Assert.Empty(result.MutationEvidence.ProposedUpdatedEdgeIds);
        Assert.Empty(result.MutationEvidence.ProposedReorderedNodeIds);
        Assert.True(result.MutationEvidence.MutationApplied);
    }

    [Fact]
    public void Build_AcceptedMutationReportsAddedRemovedUpdatedAndFullOrderChanges()
    {
        var previous = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [
                CreateEdge("edge-one", "node-a", ["node-b"]),
                CreateEdge("edge-two", "node-b", ["node-c"])
            ],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);
        var proposed = CreateTopology(
            [CreateNode("node-d"), CreateNode("node-c"), CreateNode("node-a", outputType: "output-v2")],
            [
                CreateEdge("edge-three", "node-c", ["node-d"]),
                CreateEdge("edge-one", "node-a", ["node-c"])
            ],
            [CreateOrder("node-d", 2), CreateOrder("node-c", 0), CreateOrder("node-a", 1)]);

        var result = Build(CreateSignal(
            proposed,
            DynamicGraphBuildSourceKind.MachineTopologyProposal,
            previous));
        var evidence = result.MutationEvidence;

        Assert.Equal(["node-d"], evidence.ProposedAddedNodeIds);
        Assert.Equal(["node-b"], evidence.ProposedRemovedNodeIds);
        Assert.Equal(["node-a"], evidence.ProposedUpdatedNodeIds);
        Assert.Equal(["edge-three"], evidence.ProposedAddedEdgeIds);
        Assert.Equal(["edge-two"], evidence.ProposedRemovedEdgeIds);
        Assert.Equal(["edge-one"], evidence.ProposedUpdatedEdgeIds);
        Assert.Equal(["node-a", "node-c"], evidence.ProposedReorderedNodeIds);
        Assert.True(evidence.MutationApplied);
        Assert.Null(result.FailureReason);
        Assert.Equal(["node-d", "node-c", "node-a"], result.Topology!.Nodes.Select(node => node.NodeId));
    }

    [Fact]
    public void Build_OrderIdentityChangeAloneProducesReorderEvidence()
    {
        var previous = CreateTopology(
            [CreateNode("node-a")],
            [],
            [CreateOrder("node-a", 0)]);
        var proposed = CreateTopology(
            [CreateNode("node-a")],
            [],
            [new DynamicGraphNodeOrder("order-node-a-v2", "node-a", TenantId, DomainTag, ContextId, 0)]);

        var result = Build(CreateSignal(
            proposed,
            DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            previous));

        Assert.Equal(["node-a"], result.MutationEvidence.ProposedReorderedNodeIds);
        Assert.True(result.MutationEvidence.MutationApplied);
        Assert.Empty(result.MutationEvidence.ProposedUpdatedNodeIds);
        Assert.Empty(result.MutationEvidence.ProposedAddedNodeIds);
        Assert.Empty(result.MutationEvidence.ProposedRemovedNodeIds);
    }

    [Fact]
    public void Build_EveryValidNodeDescriptorFieldChangeIsClassifiedAsUpdated()
    {
        var baseline = CreateNode("node-a");
        var trained = CreateNode(
            "node-a",
            isTrained: true,
            authorityRole: "trained_node",
            checkpointRef: "checkpoint-a",
            manifestRef: "manifest-a");
        var cases = new (string Field, TestNode Previous, TestNode Proposed)[]
        {
            ("DomainTag", baseline, CreateNode("node-a", domainTag: "*")),
            ("TInputTypeName", baseline, CreateNode("node-a", inputType: "input-v2")),
            ("TOutputTypeName", baseline, CreateNode("node-a", outputType: "output-v2")),
            ("AuthorityRole", baseline, CreateNode("node-a", authorityRole: "diagnostics_only")),
            ("IsTrained", baseline, trained),
            ("RuntimeSemanticQualityClaimed", trained, CreateNode(
                "node-a",
                isTrained: true,
                runtimeSemanticQualityClaimed: true,
                authorityRole: "trained_node",
                checkpointRef: "checkpoint-a",
                manifestRef: "manifest-a")),
            ("CheckpointRef", trained, CreateNode(
                "node-a",
                isTrained: true,
                authorityRole: "trained_node",
                checkpointRef: "checkpoint-b",
                manifestRef: "manifest-a")),
            ("ManifestRef", trained, CreateNode(
                "node-a",
                isTrained: true,
                authorityRole: "trained_node",
                checkpointRef: "checkpoint-a",
                manifestRef: "manifest-b"))
        };

        foreach (var testCase in cases)
        {
            var previous = CreateTopology(
                [testCase.Previous],
                [],
                [CreateOrder("node-a", 0)]);
            var proposed = CreateTopology(
                [testCase.Proposed],
                [],
                [CreateOrder("node-a", 0)]);
            var result = Build(CreateSignal(
                proposed,
                DynamicGraphBuildSourceKind.MachineTopologyProposal,
                previous));

            Assert.Equal(["node-a"], result.MutationEvidence.ProposedUpdatedNodeIds);
            Assert.True(result.MutationEvidence.MutationApplied);
            Assert.Empty(result.MutationEvidence.ProposedAddedNodeIds);
            Assert.Empty(result.MutationEvidence.ProposedRemovedNodeIds);
        }
    }

    [Fact]
    public void Build_EveryValidEdgeContractFieldChangeIsClassifiedAsUpdated()
    {
        var nodes = new IDynamicGraphNode[]
        {
            CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")
        };
        var order = new[]
        {
            CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)
        };
        var baseline = CreateEdge("edge-a", "node-a", ["node-c"]);
        var cases = new (string Field, TestEdge Proposed)[]
        {
            ("FromNodeId", CreateEdge("edge-a", "node-b", ["node-c"])),
            ("ToNodeIds", CreateEdge("edge-a", "node-a", ["node-b"])),
            ("Kind", CreateEdge("edge-a", "node-a", ["node-c"], kind: GraphEdgeKind.MultiOutput)),
            ("ParallelEligible", CreateEdge("edge-a", "node-a", ["node-c"], parallelEligible: true)),
            ("AuthorityUnitId", CreateEdge("edge-a", "node-a", ["node-c"], authorityUnitId: "authority-unit-b")),
            ("BaselineUnitId", CreateEdge("edge-a", "node-a", ["node-c"], baselineUnitId: "baseline-unit-b")),
            ("BaselineRole", CreateEdge("edge-a", "node-a", ["node-c"], baselineRole: "ablation_baseline")),
            ("SourceProof", CreateEdge("edge-a", "node-a", ["node-c"], sourceProof: "source-proof-b")),
            ("CutoverProof", CreateEdge("edge-a", "node-a", ["node-c"], cutoverProof: "cutover-proof-b")),
            ("AblationProofRef", CreateEdge("edge-a", "node-a", ["node-c"], ablationProofRef: "ablation-proof-b"))
        };

        foreach (var testCase in cases)
        {
            var previous = CreateTopology(nodes, [baseline], order);
            var proposed = CreateTopology(nodes.Reverse().ToArray(), [testCase.Proposed], order.Reverse().ToArray());
            var result = Build(CreateSignal(
                proposed,
                DynamicGraphBuildSourceKind.MachineTopologyProposal,
                previous));

            Assert.Equal(["edge-a"], result.MutationEvidence.ProposedUpdatedEdgeIds);
            Assert.True(result.MutationEvidence.MutationApplied);
            Assert.Empty(result.MutationEvidence.ProposedAddedEdgeIds);
            Assert.Empty(result.MutationEvidence.ProposedRemovedEdgeIds);
        }
    }

    [Fact]
    public void Build_OrderNodeAssociationAndPositionSwapClassifiesBothRetainedNodesAsReordered()
    {
        var nodes = new IDynamicGraphNode[] { CreateNode("node-a"), CreateNode("node-b") };
        var previous = CreateTopology(
            nodes,
            [],
            [
                new DynamicGraphNodeOrder("order-a", "node-a", TenantId, DomainTag, ContextId, 0),
                new DynamicGraphNodeOrder("order-b", "node-b", TenantId, DomainTag, ContextId, 1)
            ]);
        var proposed = CreateTopology(
            nodes.Reverse().ToArray(),
            [],
            [
                new DynamicGraphNodeOrder("order-a", "node-b", TenantId, DomainTag, ContextId, 0),
                new DynamicGraphNodeOrder("order-b", "node-a", TenantId, DomainTag, ContextId, 1)
            ]);

        var result = Build(CreateSignal(
            proposed,
            DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            previous));

        Assert.Equal(["node-a", "node-b"], result.MutationEvidence.ProposedReorderedNodeIds);
        Assert.True(result.MutationEvidence.MutationApplied);
        Assert.Empty(result.MutationEvidence.ProposedUpdatedNodeIds);
    }

    [Fact]
    public void Build_IdenticalIdBasedSnapshotsAreNoopDespiteEnumerationShuffle()
    {
        var previous = CreateChainTopology(["node-a", "node-b", "node-c"]);
        var proposed = CreateTopology(
            [CreateNode("node-c"), CreateNode("node-a"), CreateNode("node-b")],
            [
                CreateEdge("edge-node-b-node-c", "node-b", ["node-c"]),
                CreateEdge("edge-node-a-node-b", "node-a", ["node-b"])
            ],
            [CreateOrder("node-c", 2), CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);

        var result = Build(CreateSignal(
            proposed,
            DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            previous));

        Assert.Null(result.FailureReason);
        Assert.NotNull(result.Topology);
        Assert.NotSame(previous, result.Topology);
        Assert.Equal(
            previous.Nodes.Select(node => node.NodeId),
            result.Topology!.Nodes.Select(node => node.NodeId));
        Assert.Equal(
            previous.Edges.Select(edge => edge.EdgeId),
            result.Topology.Edges.Select(edge => edge.EdgeId));
        Assert.Equal(
            previous.Order.Select(entry => entry.NodeId),
            result.Topology.Order.Select(entry => entry.NodeId));
        Assert.NotSame(previous.Nodes[0], result.Topology.Nodes[0]);
        Assert.NotSame(previous.Nodes[0].Descriptor, result.Topology.Nodes[0].Descriptor);
        Assert.NotSame(previous.Edges[0], result.Topology.Edges[0]);
        Assert.NotSame(previous.Edges[0].ToNodeIds, result.Topology.Edges[0].ToNodeIds);
        Assert.NotSame(
            previous.Edges[0].ExcludedLeakageSignals,
            result.Topology.Edges[0].ExcludedLeakageSignals);
        Assert.NotSame(previous.Order[0], result.Topology.Order[0]);
        var outputEdges = Assert.IsAssignableFrom<ICollection<IDynamicGraphEdge>>(result.Topology.Edges);
        var outputOrder = Assert.IsAssignableFrom<ICollection<DynamicGraphNodeOrder>>(result.Topology.Order);
        var outputTargets = Assert.IsAssignableFrom<ICollection<string>>(result.Topology.Edges[0].ToNodeIds);
        var outputLeakage = Assert.IsAssignableFrom<ICollection<string>>(
            result.Topology.Edges[0].ExcludedLeakageSignals);
        Assert.True(outputEdges.IsReadOnly);
        Assert.True(outputOrder.IsReadOnly);
        Assert.True(outputTargets.IsReadOnly);
        Assert.True(outputLeakage.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => outputEdges.Add(previous.Edges[0]));
        Assert.Throws<NotSupportedException>(() => outputOrder.Add(previous.Order[0]));
        Assert.Throws<NotSupportedException>(() => outputTargets.Add("node-mutated"));
        Assert.Throws<NotSupportedException>(() => outputLeakage.Add("signal-mutated"));
        Assert.False(result.MutationEvidence.MutationApplied);
        AssertEvidenceListsEmpty(result.MutationEvidence);
    }

    [Fact]
    public void Build_SetLikeTargetCarrierShuffleIsNoopAndRetainsPreviousCarrierOrder()
    {
        var nodes = new IDynamicGraphNode[]
        {
            CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")
        };
        var order = new[]
        {
            CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)
        };
        var previous = CreateTopology(
            nodes,
            [CreateEdge("edge-a", "node-a", ["node-b", "node-c"])],
            order);
        var proposed = CreateTopology(
            nodes.Reverse().ToArray(),
            [CreateEdge(
                "edge-a",
                "node-a",
                ["node-c", "node-b"])],
            order.Reverse().ToArray());

        var result = Build(CreateSignal(
            proposed,
            DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            previous));

        Assert.False(result.MutationEvidence.MutationApplied);
        Assert.Empty(result.MutationEvidence.ProposedUpdatedEdgeIds);
        Assert.Equal(["node-b", "node-c"], result.Topology!.Edges[0].ToNodeIds);
        Assert.Equal(RequiredLeakageSignals, result.Topology.Edges[0].ExcludedLeakageSignals);
        Assert.NotSame(previous.Edges[0], result.Topology.Edges[0]);
        Assert.NotSame(previous.Edges[0].ToNodeIds, result.Topology.Edges[0].ToNodeIds);
        Assert.NotSame(
            previous.Edges[0].ExcludedLeakageSignals,
            result.Topology.Edges[0].ExcludedLeakageSignals);
    }

    [Fact]
    public void EdgeEquality_TreatsTargetsAsSetLikeAndLeakageVocabularyAsExactOrdered()
    {
        var edgeEquals = typeof(DeterministicDynamicGraphTopologyBuilder).GetMethod(
            "EdgeEquals",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(edgeEquals);
        var canonical = CreateEdge("edge-a", "node-a", ["node-b", "node-c"]);
        var reorderedTargets = CreateEdge("edge-a", "node-a", ["node-c", "node-b"]);
        var reorderedLeakage = CreateEdge(
            "edge-a",
            "node-a",
            ["node-b", "node-c"],
            excludedLeakageSignals: RequiredLeakageSignals.Reverse().ToArray());

        var targetResult = edgeEquals.Invoke(null, [canonical, reorderedTargets]);
        var leakageResult = edgeEquals.Invoke(null, [canonical, reorderedLeakage]);

        Assert.True(Assert.IsType<bool>(targetResult));
        Assert.False(Assert.IsType<bool>(leakageResult));
    }

    [Fact]
    public void Build_ExactBudgetBoundaryIsAccepted()
    {
        var topology = CreateChainTopology(["node-a", "node-b", "node-c"]);
        var result = Build(
            CreateSignal(topology, DynamicGraphBuildSourceKind.DeterministicBootstrap),
            new TopologyBuildBudget(3, 2, 2, 1));

        Assert.True(result.BudgetProof.WithinBudget);
        Assert.Empty(result.BudgetProof.ExceededDimensions);
        Assert.Null(result.FailureReason);
        Assert.NotNull(result.Topology);
        Assert.True(result.MutationEvidence.MutationApplied);
    }

    [Fact]
    public void Build_EachExceededDimensionIsReportedSeparatelyInFixedVocabulary()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [CreateEdge("edge-a", "node-a", ["node-b", "node-c"], parallelEligible: true)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);
        var signal = CreateSignal(topology, DynamicGraphBuildSourceKind.DeterministicBootstrap);
        var cases = new (TopologyBuildBudget Budget, string Dimension)[]
        {
            (new TopologyBuildBudget(2, 1, 1, 2), "nodes"),
            (new TopologyBuildBudget(3, 0, 1, 2), "edges"),
            (new TopologyBuildBudget(3, 1, 0, 2), "depth"),
            (new TopologyBuildBudget(3, 1, 1, 1), "parallel_branches")
        };

        foreach (var testCase in cases)
        {
            var result = Build(signal, testCase.Budget);
            Assert.False(result.BudgetProof.WithinBudget);
            Assert.Equal([testCase.Dimension], result.BudgetProof.ExceededDimensions);
            Assert.Equal(BudgetExceededCode, result.FailureReason);
            Assert.Null(result.Topology);
            Assert.False(result.MutationEvidence.MutationApplied);
        }
    }

    [Fact]
    public void Build_AllExceededDimensionsUseFixedNodesEdgesDepthParallelOrder()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [CreateEdge("edge-a", "node-a", ["node-b", "node-c"], parallelEligible: true)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

        var result = Build(
            CreateSignal(topology, DynamicGraphBuildSourceKind.DeterministicBootstrap),
            new TopologyBuildBudget(2, 0, 0, 1));

        Assert.Equal(
            ["nodes", "edges", "depth", "parallel_branches"],
            result.BudgetProof.ExceededDimensions);
        Assert.False(result.BudgetProof.WithinBudget);
        Assert.Equal(BudgetExceededCode, result.FailureReason);
        Assert.Null(result.Topology);
        Assert.False(result.MutationEvidence.MutationApplied);
    }

    [Fact]
    public void Build_BudgetRejectionRetainsPreviousSnapshotButInitialRejectionReturnsNull()
    {
        var previous = CreateChainTopology(["node-old"]);
        var proposed = CreateChainTopology(["node-a", "node-b"]);
        var rejectedMutation = Build(
            CreateSignal(proposed, DynamicGraphBuildSourceKind.MachineTopologyProposal, previous),
            new TopologyBuildBudget(1, 1, 1, 1));

        Assert.Equal(BudgetExceededCode, rejectedMutation.FailureReason);
        Assert.NotNull(rejectedMutation.Topology);
        Assert.NotSame(previous, rejectedMutation.Topology);
        Assert.Equal(["node-old"], rejectedMutation.Topology!.Nodes.Select(node => node.NodeId));
        Assert.False(rejectedMutation.MutationEvidence.MutationApplied);

        var rejectedInitial = Build(
            CreateSignal(proposed, DynamicGraphBuildSourceKind.DeterministicBootstrap),
            new TopologyBuildBudget(1, 1, 1, 1));
        Assert.Equal(BudgetExceededCode, rejectedInitial.FailureReason);
        Assert.Null(rejectedInitial.Topology);
        Assert.False(rejectedInitial.MutationEvidence.MutationApplied);
    }

    [Fact]
    public void Build_RejectsMissingAndInvalidBudgetsWithStableCodes()
    {
        var signal = CreateSignal(
            CreateChainTopology(["node-a"]),
            DynamicGraphBuildSourceKind.DeterministicBootstrap);
        AssertFailure(new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, null!), BudgetRequiredCode);

        var invalid = new[]
        {
            new TopologyBuildBudget(0, 0, 0, 1),
            new TopologyBuildBudget(1, -1, 0, 1),
            new TopologyBuildBudget(1, 0, -1, 1),
            new TopologyBuildBudget(1, 0, 0, 0),
            new TopologyBuildBudget(1, 0, 0, 9)
        };
        foreach (var budget in invalid)
            AssertFailure(new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(signal, budget), BudgetInvalidCode);
    }

    [Fact]
    public void Build_RejectsMissingMalformedEnumAndBootstrapWithPrevious()
    {
        var builder = new DeterministicDynamicGraphTopologyBuilder();
        var topology = CreateChainTopology(["node-a"]);
        var valid = CreateSignal(topology, DynamicGraphBuildSourceKind.DeterministicBootstrap);

        AssertFailure(builder.BuildDynamicTopology(null!, ValidBudget()), SignalRequiredCode);
        var invalid = new[]
        {
            valid with { SignalId = " " },
            valid with { SourceKind = (DynamicGraphBuildSourceKind)99 },
            valid with { ProposedTopology = null! },
            valid with { SourceRef = " " },
            valid with { TraceRef = " " },
            valid with { PreviousTopology = topology }
        };
        foreach (var signal in invalid)
            AssertFailure(builder.BuildDynamicTopology(signal, ValidBudget()), SignalInvalidCode);
    }

    [Fact]
    public void Build_PropagatesTopologyValidationCodesAndValidatesPreviousFirst()
    {
        var cycle = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [
                CreateEdge("edge-a", "node-a", ["node-b"]),
                CreateEdge("edge-b", "node-b", ["node-a"])
            ],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);
        var dangling = CreateTopology(
            [CreateNode("node-a")],
            [CreateEdge("edge-dangling", "node-a", ["node-missing"])],
            [CreateOrder("node-a", 0)]);
        var authorityBypass = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [CreateEdge("edge-authority", "node-a", ["node-b"], deterministicFinalAuthority: true)],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);
        var invalidAuthorityLevel = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [CreateEdge("edge-level", "node-a", ["node-b"], authorityUnitLevel: "L3")],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);
        var duplicateTarget = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b")],
            [CreateEdge("edge-duplicate-target", "node-a", ["node-b", "node-b"])],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1)]);
        var nodeAuthorityBypass = CreateTopology(
            [CreateNode("node-a", deterministicFinalAuthority: true)],
            [],
            [CreateOrder("node-a", 0)]);
        var descriptorMismatch = CreateTopology(
            [new TestNode("node-a", CreateNode("node-b").Descriptor)],
            [],
            [CreateOrder("node-a", 0)]);
        var builder = new DeterministicDynamicGraphTopologyBuilder();

        AssertFailure(
            builder.BuildDynamicTopology(
                CreateSignal(cycle, DynamicGraphBuildSourceKind.LogicalRegistryProjection),
                ValidBudget()),
            "DYNAMIC_GRAPH_CYCLE_DETECTED");
        AssertFailure(
            builder.BuildDynamicTopology(
                CreateSignal(dangling, DynamicGraphBuildSourceKind.LogicalRegistryProjection),
                ValidBudget()),
            "DYNAMIC_GRAPH_DANGLING_TARGET_NODE");
        AssertFailure(
            builder.BuildDynamicTopology(
                CreateSignal(authorityBypass, DynamicGraphBuildSourceKind.LogicalRegistryProjection),
                ValidBudget()),
            "DYNAMIC_GRAPH_EDGE_DETERMINISTIC_AUTHORITY_FORBIDDEN");
        AssertFailure(
            builder.BuildDynamicTopology(
                CreateSignal(invalidAuthorityLevel, DynamicGraphBuildSourceKind.LogicalRegistryProjection),
                ValidBudget()),
            "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_INVALID");
        AssertFailure(
            builder.BuildDynamicTopology(
                CreateSignal(duplicateTarget, DynamicGraphBuildSourceKind.LogicalRegistryProjection),
                ValidBudget()),
            "DYNAMIC_GRAPH_DUPLICATE_EDGE_TARGET");
        AssertFailure(
            builder.BuildDynamicTopology(
                CreateSignal(nodeAuthorityBypass, DynamicGraphBuildSourceKind.LogicalRegistryProjection),
                ValidBudget()),
            "DYNAMIC_GRAPH_NODE_DETERMINISTIC_AUTHORITY_FORBIDDEN");
        AssertFailure(
            builder.BuildDynamicTopology(
                CreateSignal(descriptorMismatch, DynamicGraphBuildSourceKind.LogicalRegistryProjection),
                ValidBudget()),
            "DYNAMIC_GRAPH_NODE_DESCRIPTOR_ID_MISMATCH");
        AssertFailure(
            builder.BuildDynamicTopology(
                CreateSignal(dangling, DynamicGraphBuildSourceKind.MachineTopologyProposal, cycle),
                ValidBudget()),
            "DYNAMIC_GRAPH_CYCLE_DETECTED");
    }

    [Fact]
    public void Build_RejectsTenantDomainAndContextMutationScopeMismatch()
    {
        var previous = CreateScopedSingle(TenantId, DomainTag, ContextId);
        var proposed = new[]
        {
            CreateScopedSingle("tenant-b", DomainTag, ContextId),
            CreateScopedSingle(TenantId, "other-domain", ContextId),
            CreateScopedSingle(TenantId, DomainTag, "context-b")
        };

        foreach (var topology in proposed)
        {
            AssertFailure(
                new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(
                    CreateSignal(topology, DynamicGraphBuildSourceKind.MachineTopologyProposal, previous),
                    ValidBudget()),
                ScopeMismatchCode);
        }
    }

    [Fact]
    public void Build_ReturnLoopDoesNotIncreaseOrdinaryDepth()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c")],
            [
                CreateEdge("edge-a-b", "node-a", ["node-b"]),
                CreateEdge("edge-b-c", "node-b", ["node-c"]),
                CreateEdge("edge-return", "node-c", ["node-a"], kind: GraphEdgeKind.ReturnLoop)
            ],
            [CreateOrder("node-a", 0), CreateOrder("node-b", 1), CreateOrder("node-c", 2)]);

        var result = Build(
            CreateSignal(topology, DynamicGraphBuildSourceKind.DeterministicBootstrap),
            new TopologyBuildBudget(3, 3, 2, 1));

        Assert.Equal(2, result.BudgetProof.ActualDepth);
        Assert.True(result.BudgetProof.WithinBudget);
    }

    [Fact]
    public void Build_LongestDepthUsesMaximumAcrossMultipleRootsAndDisconnectedComponents()
    {
        var topology = CreateTopology(
            [
                CreateNode("node-a"),
                CreateNode("node-b"),
                CreateNode("node-c"),
                CreateNode("node-d"),
                CreateNode("node-e"),
                CreateNode("node-isolated")
            ],
            [
                CreateEdge("edge-a-b", "node-a", ["node-b"]),
                CreateEdge("edge-b-c", "node-b", ["node-c"]),
                CreateEdge("edge-d-e", "node-d", ["node-e"])
            ],
            [
                CreateOrder("node-a", 0),
                CreateOrder("node-b", 1),
                CreateOrder("node-c", 2),
                CreateOrder("node-d", 3),
                CreateOrder("node-e", 4),
                CreateOrder("node-isolated", 5)
            ]);

        var validation = DynamicGraphValidation.Validate(topology);
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        var result = Build(
            CreateSignal(topology, DynamicGraphBuildSourceKind.DeterministicBootstrap),
            new TopologyBuildBudget(6, 3, 2, 1));

        Assert.Equal(2, result.BudgetProof.ActualDepth);
        Assert.True(result.BudgetProof.WithinBudget);
    }

    [Fact]
    public void Build_ParallelWidthCountsDistinctTargetsOnlyOnEligibleOrdinaryEdges()
    {
        var topology = CreateTopology(
            [CreateNode("node-a"), CreateNode("node-b"), CreateNode("node-c"), CreateNode("node-d")],
            [
                CreateEdge("edge-eligible", "node-a", ["node-b"], parallelEligible: true),
                CreateEdge("edge-ineligible", "node-a", ["node-c", "node-d"], parallelEligible: false)
            ],
            [
                CreateOrder("node-a", 0),
                CreateOrder("node-b", 1),
                CreateOrder("node-c", 2),
                CreateOrder("node-d", 3)
            ]);

        var result = Build(
            CreateSignal(topology, DynamicGraphBuildSourceKind.DeterministicBootstrap),
            new TopologyBuildBudget(4, 2, 1, 1));

        Assert.Equal(1, result.BudgetProof.ActualParallelBranches);
        Assert.True(result.BudgetProof.WithinBudget);
    }

    [Fact]
    public void Build_ParallelWidthDeduplicatesOverlappingEligibleTargetsPerSource()
    {
        var topology = CreateTopology(
            [
                CreateNode("node-a"),
                CreateNode("node-b"),
                CreateNode("node-c"),
                CreateNode("node-d")
            ],
            [
                CreateEdge("edge-first", "node-a", ["node-b", "node-c"], parallelEligible: true),
                CreateEdge("edge-second", "node-a", ["node-c", "node-d"], parallelEligible: true)
            ],
            [
                CreateOrder("node-a", 0),
                CreateOrder("node-b", 1),
                CreateOrder("node-c", 2),
                CreateOrder("node-d", 3)
            ]);

        var result = Build(
            CreateSignal(topology, DynamicGraphBuildSourceKind.DeterministicBootstrap),
            new TopologyBuildBudget(4, 2, 1, 3));

        Assert.Equal(3, result.BudgetProof.ActualParallelBranches);
        Assert.True(result.BudgetProof.WithinBudget);
    }

    [Fact]
    public void Build_ShuffledEnumerationProducesStableProofAndEvidence()
    {
        var firstTopology = CreateChainTopology(["node-a", "node-b", "node-c"]);
        var secondTopology = CreateTopology(
            [CreateNode("node-c"), CreateNode("node-b"), CreateNode("node-a")],
            [
                CreateEdge("edge-node-b-node-c", "node-b", ["node-c"]),
                CreateEdge("edge-node-a-node-b", "node-a", ["node-b"])
            ],
            [CreateOrder("node-b", 1), CreateOrder("node-c", 2), CreateOrder("node-a", 0)]);
        var first = Build(CreateSignal(firstTopology, DynamicGraphBuildSourceKind.DeterministicBootstrap));
        var second = Build(CreateSignal(secondTopology, DynamicGraphBuildSourceKind.DeterministicBootstrap));

        Assert.Equal(ProofSnapshot(first.BudgetProof), ProofSnapshot(second.BudgetProof));
        Assert.Equal(EvidenceSnapshot(first.MutationEvidence), EvidenceSnapshot(second.MutationEvidence));
    }

    [Fact]
    public void Build_ReturnsDefensiveTopologyAndReadOnlyProofEvidenceCollections()
    {
        var proposed = CreateChainTopology(["node-a", "node-b"]);
        var result = Build(CreateSignal(proposed, DynamicGraphBuildSourceKind.DeterministicBootstrap));

        Assert.NotSame(proposed, result.Topology);
        Assert.NotSame(proposed.Nodes[0], result.Topology!.Nodes[0]);
        var addedNodes = Assert.IsAssignableFrom<ICollection<string>>(
            result.MutationEvidence.ProposedAddedNodeIds);
        var exceeded = Assert.IsAssignableFrom<ICollection<string>>(
            result.BudgetProof.ExceededDimensions);
        var resultNodes = Assert.IsAssignableFrom<ICollection<IDynamicGraphNode>>(result.Topology.Nodes);
        Assert.True(addedNodes.IsReadOnly);
        Assert.True(exceeded.IsReadOnly);
        Assert.True(resultNodes.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => addedNodes.Add("node-mutated"));
        Assert.Throws<NotSupportedException>(() => exceeded.Add("nodes"));
        Assert.Throws<NotSupportedException>(() => resultNodes.Add(CreateNode("node-mutated")));
        Assert.Equal(["node-a", "node-b"], result.MutationEvidence.ProposedAddedNodeIds);
    }

    [Fact]
    public void Build_SanitizesUnexpectedExceptionsWithoutLeakingDetails()
    {
        var corrupted = CreateChainTopology(["node-a"]);
        var nodesField = typeof(DynamicGraphTopology).GetField(
            "<Nodes>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(nodesField);
        nodesField.SetValue(corrupted, null);

        var result = new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(
            CreateSignal(corrupted, DynamicGraphBuildSourceKind.LogicalRegistryProjection),
            ValidBudget());

        AssertFailure(result, ExceptionCode);
        Assert.DoesNotContain("NullReference", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at ", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static BuildDynamicTopologyResult Build(
        DynamicGraphBuildSignal signal,
        TopologyBuildBudget? budget = null)
    {
        var result = new DeterministicDynamicGraphTopologyBuilder().BuildDynamicTopology(
            signal,
            budget ?? ValidBudget());
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsType<BuildDynamicTopologyResult>(result.Value);
    }

    private static DynamicGraphBuildSignal CreateSignal(
        DynamicGraphTopology proposed,
        DynamicGraphBuildSourceKind sourceKind,
        DynamicGraphTopology? previous = null)
        => new(
            SignalId: "signal-a",
            SourceKind: sourceKind,
            ProposedTopology: proposed,
            PreviousTopology: previous,
            SourceRef: "source-a",
            TraceRef: "trace-a");

    private static TopologyBuildBudget ValidBudget()
        => new(MaxNodes: 32, MaxEdges: 64, MaxDepth: 32, MaxParallelBranches: 8);

    private static DynamicGraphTopology CreateChainTopology(string[] nodeIds)
    {
        var nodes = nodeIds.Select(nodeId => CreateNode(nodeId)).Cast<IDynamicGraphNode>().ToArray();
        var edges = nodeIds.Zip(nodeIds.Skip(1), (from, to) =>
                CreateEdge($"edge-{from}-{to}", from, [to]))
            .Cast<IDynamicGraphEdge>()
            .ToArray();
        var order = nodeIds.Select((nodeId, position) => CreateOrder(nodeId, position)).ToArray();
        return CreateTopology(nodes, edges, order);
    }

    private static DynamicGraphTopology CreateScopedSingle(
        string tenantId,
        string domainTag,
        string contextId)
        => CreateTopology(
            [CreateNode("node-a", domainTag)],
            [],
            [CreateOrder("node-a", 0, tenantId, domainTag, contextId)],
            tenantId,
            domainTag,
            contextId);

    private static DynamicGraphTopology CreateTopology(
        IReadOnlyList<IDynamicGraphNode> nodes,
        IReadOnlyList<IDynamicGraphEdge> edges,
        IReadOnlyList<DynamicGraphNodeOrder> order,
        string tenantId = TenantId,
        string domainTag = DomainTag,
        string contextId = ContextId)
        => new(tenantId, domainTag, contextId, nodes, edges, order);

    private static TestNode CreateNode(
        string nodeId,
        string domainTag = DomainTag,
        string outputType = "output-v1",
        string inputType = "input-v1",
        bool isTrained = false,
        bool deterministicFinalAuthority = false,
        bool runtimeSemanticQualityClaimed = false,
        string authorityRole = "baseline_candidate_generator",
        string? checkpointRef = null,
        string? manifestRef = null)
        => new(
            nodeId,
            new DynamicGraphNodeDescriptor(
                NodeId: nodeId,
                DomainTag: domainTag,
                TInputTypeName: inputType,
                TOutputTypeName: outputType,
                IsTrained: isTrained,
                DeterministicFinalAuthority: deterministicFinalAuthority,
                RuntimeSemanticQualityClaimed: runtimeSemanticQualityClaimed,
                AuthorityRole: authorityRole,
                CheckpointRef: checkpointRef,
                ManifestRef: manifestRef));

    private static TestEdge CreateEdge(
        string edgeId,
        string fromNodeId,
        IReadOnlyList<string> toNodeIds,
        GraphEdgeKind kind = GraphEdgeKind.FanOut,
        bool parallelEligible = false,
        bool deterministicFinalAuthority = false,
        string authorityUnitId = "authority-unit-a",
        string authorityUnitLevel = "L2",
        string? baselineUnitId = "baseline-unit-a",
        string baselineRole = "candidate_generator",
        IReadOnlyList<string>? excludedLeakageSignals = null,
        string sourceProof = "source-proof-a",
        string cutoverProof = "cutover-proof-a",
        string ablationProofRef = "ablation-proof-a")
        => new(
            EdgeId: edgeId,
            FromNodeId: fromNodeId,
            ToNodeIds: toNodeIds,
            Kind: kind,
            ParallelEligible: parallelEligible,
            AuthorityUnitId: authorityUnitId,
            AuthorityUnitLevel: authorityUnitLevel,
            DeterministicFinalAuthority: deterministicFinalAuthority,
            BaselineUnitId: baselineUnitId,
            BaselineRole: baselineRole,
            ExcludedLeakageSignals: excludedLeakageSignals ?? RequiredLeakageSignals,
            SourceProof: sourceProof,
            CutoverProof: cutoverProof,
            AblationProofRef: ablationProofRef);

    private static DynamicGraphNodeOrder CreateOrder(
        string nodeId,
        int position,
        string tenantId = TenantId,
        string domainTag = DomainTag,
        string contextId = ContextId)
        => new(
            OrderId: $"order-{nodeId}",
            NodeId: nodeId,
            TenantId: tenantId,
            DomainTag: domainTag,
            ContextId: contextId,
            Position: position);

    private static void AssertRecordShape<T>((string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        Assert.Equal(
            expected,
            constructor.GetParameters().Select(parameter => (parameter.Name!, parameter.ParameterType)));
        Assert.Equal(
            expected.OrderBy(item => item.Name, StringComparer.Ordinal),
            typeof(T).GetProperties()
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => (property.Name, property.PropertyType)));
    }

    private static void AssertEvidenceListsEmpty(TopologyMutationEvidence evidence)
    {
        Assert.Empty(evidence.ProposedAddedNodeIds);
        Assert.Empty(evidence.ProposedRemovedNodeIds);
        Assert.Empty(evidence.ProposedUpdatedNodeIds);
        Assert.Empty(evidence.ProposedAddedEdgeIds);
        Assert.Empty(evidence.ProposedRemovedEdgeIds);
        Assert.Empty(evidence.ProposedUpdatedEdgeIds);
        Assert.Empty(evidence.ProposedReorderedNodeIds);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string errorCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(errorCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private static string ProofSnapshot(TopologyBudgetProof proof)
        => string.Join(
            "|",
            proof.MaxNodes,
            proof.ActualNodes,
            proof.MaxEdges,
            proof.ActualEdges,
            proof.MaxDepth,
            proof.ActualDepth,
            proof.MaxParallelBranches,
            proof.ActualParallelBranches,
            proof.WithinBudget,
            string.Join(",", proof.ExceededDimensions));

    private static string EvidenceSnapshot(TopologyMutationEvidence evidence)
        => string.Join(
            "|",
            string.Join(",", evidence.ProposedAddedNodeIds),
            string.Join(",", evidence.ProposedRemovedNodeIds),
            string.Join(",", evidence.ProposedUpdatedNodeIds),
            string.Join(",", evidence.ProposedAddedEdgeIds),
            string.Join(",", evidence.ProposedRemovedEdgeIds),
            string.Join(",", evidence.ProposedUpdatedEdgeIds),
            string.Join(",", evidence.ProposedReorderedNodeIds),
            evidence.MutationApplied);

    private sealed record TestNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

    private sealed record TestEdge(
        string EdgeId,
        string FromNodeId,
        IReadOnlyList<string> ToNodeIds,
        GraphEdgeKind Kind,
        bool ParallelEligible,
        string AuthorityUnitId,
        string AuthorityUnitLevel,
        bool DeterministicFinalAuthority,
        string? BaselineUnitId,
        string BaselineRole,
        IReadOnlyList<string> ExcludedLeakageSignals,
        string SourceProof,
        string CutoverProof,
        string AblationProofRef) : IDynamicGraphEdge;
}
