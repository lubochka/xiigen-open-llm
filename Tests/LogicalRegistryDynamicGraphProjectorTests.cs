// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class LogicalRegistryDynamicGraphProjectorTests
{
    private const string DependencyRequiredCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DEPENDENCY_REQUIRED";
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_REQUEST_REQUIRED";
    private const string ScopeInvalidCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SCOPE_INVALID";
    private const string SignalReferencesInvalidCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SIGNAL_REFERENCES_INVALID";
    private const string NodeBindingsRequiredCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_NODE_BINDINGS_REQUIRED";
    private const string EdgeBindingsInvalidCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_BINDINGS_INVALID";
    private const string DuplicateNodeBindingCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_NODE_BINDING";
    private const string SubPhaseNotFoundCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SUB_PHASE_NOT_FOUND";
    private const string NodeMismatchCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_NODE_MISMATCH";
    private const string DuplicateEdgeBindingCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_EDGE_BINDING";
    private const string TransitionsRequiredCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITIONS_REQUIRED";
    private const string NonProjectableTransitionCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITION_NOT_PROJECTABLE";
    private const string DuplicateTransitionCoverageCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_TRANSITION_COVERAGE";
    private const string IncompleteTransitionCoverageCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_INCOMPLETE_TRANSITION_COVERAGE";
    private const string EdgeSourceMismatchCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_SOURCE_MISMATCH";
    private const string ExceptionCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EXCEPTION";

    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void Contracts_ExposeExactProjectionBoundaryWithoutRuntimeOrLearningAuthority()
    {
        AssertRecordShape<DynamicGraphRegistryNodeBinding>(
            [("LogicalSubPhaseId", typeof(string)), ("Node", typeof(IDynamicGraphNode))]);
        AssertRecordShape<DynamicGraphRegistryEdgeBinding>(
            [
                ("EdgeId", typeof(string)),
                ("LogicalTransitionIds", typeof(string[])),
                ("Kind", typeof(GraphEdgeKind)),
                ("ParallelEligible", typeof(bool)),
                ("AuthorityUnitId", typeof(string)),
                ("AuthorityUnitLevel", typeof(string)),
                ("DeterministicFinalAuthority", typeof(bool)),
                ("BaselineUnitId", typeof(string)),
                ("BaselineRole", typeof(string)),
                ("ExcludedLeakageSignals", typeof(string[])),
                ("SourceProof", typeof(string)),
                ("CutoverProof", typeof(string)),
                ("AblationProofRef", typeof(string))
            ]);
        AssertRecordShape<DynamicGraphRegistryProjectionRequest>(
            [
                ("TenantId", typeof(string)),
                ("DomainTag", typeof(string)),
                ("ContextId", typeof(string)),
                ("SignalId", typeof(string)),
                ("SourceRef", typeof(string)),
                ("TraceRef", typeof(string)),
                ("NodeBindings", typeof(DynamicGraphRegistryNodeBinding[])),
                ("EdgeBindings", typeof(DynamicGraphRegistryEdgeBinding[]))
            ]);

        var interfaceMethod = Assert.Single(typeof(ILogicalRegistryDynamicGraphProjector).GetMethods());
        Assert.Equal(nameof(ILogicalRegistryDynamicGraphProjector.Project), interfaceMethod.Name);
        Assert.Equal(typeof(OperationResult<DynamicGraphBuildSignal>), interfaceMethod.ReturnType);
        Assert.Equal(
            [typeof(DynamicGraphRegistryProjectionRequest)],
            interfaceMethod.GetParameters().Select(parameter => parameter.ParameterType));

        Assert.Equal(
            [typeof(ILogicalRegistryDynamicGraphProjector)],
            typeof(LogicalRegistryDynamicGraphProjector).GetInterfaces());
        var publicMethods = typeof(LogicalRegistryDynamicGraphProjector)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(["Project"], publicMethods.Select(method => method.Name));
        Assert.DoesNotContain(publicMethods, method =>
            method.Name.Contains("Apply", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Train", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Dpo", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Learn", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Assistant", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("L4", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Project_ValidForkCreatesStrictSignalWithCanonicalTargetsAndOrder()
    {
        var registry = CreateForkRegistry();
        var result = new LogicalRegistryDynamicGraphProjector(registry).Project(CreateForkRequest());

        var signal = AssertSuccess(result);
        Assert.Equal("signal-a", signal.SignalId);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, signal.SourceKind);
        Assert.Null(signal.PreviousTopology);
        Assert.Equal("registry:source", signal.SourceRef);
        Assert.Equal("trace:projection", signal.TraceRef);
        Assert.Equal(["A", "B", "C"], signal.ProposedTopology.Nodes.Select(node => node.NodeId));
        Assert.Equal(["A", "B", "C"], signal.ProposedTopology.Order.Select(order => order.NodeId));
        Assert.Equal([0, 1, 2], signal.ProposedTopology.Order.Select(order => order.Position));
        Assert.Equal(
            "registry-order-f4b06b7db9d8c29a97e994dce116b512cb89a6966395353875a9f9c6dc4147de",
            signal.ProposedTopology.Order[0].OrderId);

        var edge = Assert.Single(signal.ProposedTopology.Edges);
        Assert.Equal("edge-a-fork", edge.EdgeId);
        Assert.Equal("A", edge.FromNodeId);
        Assert.Equal(["B", "C"], edge.ToNodeIds);
        Assert.Equal(GraphEdgeKind.FanOut, edge.Kind);
        Assert.True(edge.ParallelEligible);
        Assert.Equal("explicit-authority", edge.AuthorityUnitId);
        Assert.Equal("L2", edge.AuthorityUnitLevel);
        Assert.False(edge.DeterministicFinalAuthority);
        Assert.Equal("explicit-baseline", edge.BaselineUnitId);
        Assert.Equal("candidate_generator", edge.BaselineRole);
        Assert.Equal(LeakageSignals, edge.ExcludedLeakageSignals);
        Assert.Equal("explicit-source-proof", edge.SourceProof);
        Assert.Equal("explicit-cutover-proof", edge.CutoverProof);
        Assert.Equal("explicit-ablation-proof", edge.AblationProofRef);
        Assert.Equal(1, registry.ListSubPhasesCallCount);
        Assert.Equal(1, registry.ListTransitionsCallCount);
    }

    [Fact]
    public void Project_PermutationsOfRegistryAndRequestProduceIdenticalSnapshot()
    {
        var firstRegistry = CreateForkRegistry();
        var secondRegistry = CreateForkRegistry(
            reverseSubPhases: true,
            reverseTransitions: true);
        var firstRequest = CreateForkRequest();
        var secondRequest = CreateForkRequest() with
        {
            NodeBindings = CreateForkRequest().NodeBindings.Reverse().ToArray(),
            EdgeBindings =
            [
                CreateForkRequest().EdgeBindings[0] with
                {
                    LogicalTransitionIds = CreateForkRequest().EdgeBindings[0].LogicalTransitionIds.Reverse().ToArray()
                }
            ]
        };

        var first = AssertSuccess(new LogicalRegistryDynamicGraphProjector(firstRegistry).Project(firstRequest));
        var second = AssertSuccess(new LogicalRegistryDynamicGraphProjector(secondRegistry).Project(secondRequest));
        var changedSignalRefs = AssertSuccess(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(firstRequest with
            {
                SignalId = "signal-z",
                SourceRef = "registry:other-source",
                TraceRef = "trace:other-projection"
            }));

        Assert.Equal(Snapshot(first), Snapshot(second));
        Assert.Equal(
            first.ProposedTopology.Order.Select(entry => entry.OrderId),
            changedSignalRefs.ProposedTopology.Order.Select(entry => entry.OrderId));
    }

    [Fact]
    public void Project_RejectsMissingDependencyRequestScopeSignalAndBindingSurfaces()
    {
        var valid = CreateForkRequest();
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(null!).Project(valid),
            DependencyRequiredCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(null!),
            RequestRequiredCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with { TenantId = " " }),
            ScopeInvalidCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with { SignalId = "" }),
            SignalReferencesInvalidCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with { NodeBindings = [] }),
            NodeBindingsRequiredCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with { EdgeBindings = null! }),
            EdgeBindingsInvalidCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with
            {
                EdgeBindings = [valid.EdgeBindings[0] with { LogicalTransitionIds = [] }]
            }),
            TransitionsRequiredCode);
    }

    [Fact]
    public void Project_RejectsDuplicateNodeOrEdgeBindingsAndNodeIdentityMismatch()
    {
        var registry = CreateForkRegistry();
        var valid = CreateForkRequest();

        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(registry).Project(valid with
            {
                NodeBindings = [valid.NodeBindings[0], valid.NodeBindings[0]]
            }),
            DuplicateNodeBindingCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(registry).Project(valid with
            {
                EdgeBindings = [valid.EdgeBindings[0], valid.EdgeBindings[0]]
            }),
            DuplicateEdgeBindingCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(registry).Project(valid with
            {
                NodeBindings =
                [
                    new DynamicGraphRegistryNodeBinding("A", CreateNode("wrong")),
                    valid.NodeBindings[1],
                    valid.NodeBindings[2]
                ]
            }),
            NodeMismatchCode);
    }

    [Fact]
    public void Project_RejectsUnknownSubPhaseAndForeignOrNonSubPhaseTransition()
    {
        var valid = CreateForkRequest();
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with
            {
                NodeBindings =
                [
                    new DynamicGraphRegistryNodeBinding("missing", CreateNode("missing")),
                    valid.NodeBindings[1],
                    valid.NodeBindings[2]
                ]
            }),
            SubPhaseNotFoundCode);

        var registry = CreateForkRegistry();
        registry.TransitionEntries =
        [
            .. registry.TransitionEntries,
            Entry(CreateTransition("phase-only", "A", "B", fromKind: "phase", toKind: "phase"), 30)
        ];
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(registry).Project(valid with
            {
                EdgeBindings =
                [valid.EdgeBindings[0] with { LogicalTransitionIds = ["transition-a-b", "transition-a-c", "phase-only"] }]
            }),
            NonProjectableTransitionCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with
            {
                EdgeBindings = [valid.EdgeBindings[0] with { LogicalTransitionIds = ["missing-transition"] }]
            }),
            NonProjectableTransitionCode);
    }

    [Fact]
    public void Project_RequiresExactSingleCoverageAndOneSourcePerEdge()
    {
        var valid = CreateForkRequest();
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with { EdgeBindings = [] }),
            IncompleteTransitionCoverageCode);
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(valid with
            {
                EdgeBindings =
                [
                    valid.EdgeBindings[0] with { EdgeId = "edge-one", LogicalTransitionIds = ["transition-a-b"] },
                    valid.EdgeBindings[0] with { EdgeId = "edge-two", LogicalTransitionIds = ["transition-a-b", "transition-a-c"] }
                ]
            }),
            DuplicateTransitionCoverageCode);

        var mixedRegistry = CreateForkRegistry();
        mixedRegistry.TransitionEntries =
        [
            Entry(CreateTransition("transition-a-b", "A", "B"), 10),
            Entry(CreateTransition("transition-c-b", "C", "B"), 20)
        ];
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(mixedRegistry).Project(valid with
            {
                EdgeBindings =
                [valid.EdgeBindings[0] with { LogicalTransitionIds = ["transition-a-b", "transition-c-b"] }]
            }),
            EdgeSourceMismatchCode);
    }

    [Fact]
    public void Project_DuplicateTransitionTargetsRemainVisibleToCanonicalValidator()
    {
        var registry = CreateForkRegistry();
        registry.TransitionEntries =
        [
            Entry(CreateTransition("transition-a-b-one", "A", "B"), 10),
            Entry(CreateTransition("transition-a-b-two", "A", "B"), 20)
        ];
        var request = CreateForkRequest() with
        {
            EdgeBindings =
            [CreateForkRequest().EdgeBindings[0] with
            {
                LogicalTransitionIds = ["transition-a-b-one", "transition-a-b-two"]
            }]
        };

        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(registry).Project(request),
            "DYNAMIC_GRAPH_DUPLICATE_EDGE_TARGET");
    }

    [Fact]
    public void Project_OrdinaryCycleReturnsCanonicalDynamicGraphCycleCode()
    {
        var registry = new StubRegistry(
            [Entry(CreateSubPhase("A"), 1), Entry(CreateSubPhase("B"), 2)],
            [
                Entry(CreateTransition("transition-a-b", "A", "B"), 10),
                Entry(CreateTransition("transition-b-a", "B", "A"), 20)
            ]);
        var validEdge = CreateEdgeBinding("edge-a-b", ["transition-a-b"]);
        var request = CreateForkRequest() with
        {
            NodeBindings =
            [
                new DynamicGraphRegistryNodeBinding("A", CreateNode("A")),
                new DynamicGraphRegistryNodeBinding("B", CreateNode("B"))
            ],
            EdgeBindings =
            [
                validEdge,
                CreateEdgeBinding("edge-b-a", ["transition-b-a"])
            ]
        };

        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(registry).Project(request),
            "DYNAMIC_GRAPH_CYCLE_DETECTED");
    }

    [Fact]
    public void Project_OrdinarySelfTransitionPreservesCanonicalSelfEdgeCodeBeforeKahn()
    {
        var registry = new StubRegistry(
            [Entry(CreateSubPhase("A"), 1)],
            [Entry(CreateTransition("transition-a-self", "A", "A"), 10)]);
        var request = CreateForkRequest() with
        {
            NodeBindings = [new DynamicGraphRegistryNodeBinding("A", CreateNode("A"))],
            EdgeBindings = [CreateEdgeBinding("edge-a-self", ["transition-a-self"])]
        };

        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(registry).Project(request),
            "DYNAMIC_GRAPH_SELF_EDGE");
    }

    [Fact]
    public void Project_ReturnLoopIsExcludedFromKahnIndegreeAndRetainsEarlierTargetDirection()
    {
        var registry = new StubRegistry(
            [Entry(CreateSubPhase("B"), 2), Entry(CreateSubPhase("A"), 1)],
            [
                Entry(CreateTransition("transition-b-a-return", "B", "A"), 20),
                Entry(CreateTransition("transition-a-b", "A", "B"), 10)
            ]);
        var returnEdge = CreateEdgeBinding("edge-b-a-return", ["transition-b-a-return"]) with
        {
            Kind = GraphEdgeKind.ReturnLoop,
            ParallelEligible = false
        };
        var request = CreateForkRequest() with
        {
            NodeBindings =
            [
                new DynamicGraphRegistryNodeBinding("B", CreateNode("B")),
                new DynamicGraphRegistryNodeBinding("A", CreateNode("A"))
            ],
            EdgeBindings =
            [
                returnEdge,
                CreateEdgeBinding("edge-a-b", ["transition-a-b"])
            ]
        };

        var signal = AssertSuccess(new LogicalRegistryDynamicGraphProjector(registry).Project(request));

        Assert.Equal(["A", "B"], signal.ProposedTopology.Order.Select(entry => entry.NodeId));
        var projectedReturn = Assert.Single(
            signal.ProposedTopology.Edges,
            edge => edge.Kind == GraphEdgeKind.ReturnLoop);
        Assert.Equal("B", projectedReturn.FromNodeId);
        Assert.Equal(["A"], projectedReturn.ToNodeIds);
        Assert.False(projectedReturn.ParallelEligible);
    }

    [Fact]
    public void Project_WhenInducedSubgraphHasNoTransitionsAcceptsEmptyEdgesAndUsesOrdinalTies()
    {
        var registry = new StubRegistry(
            [Entry(CreateSubPhase("C"), 2), Entry(CreateSubPhase("A"), 1)],
            []);
        var request = CreateForkRequest() with
        {
            NodeBindings =
            [
                new DynamicGraphRegistryNodeBinding("C", CreateNode("C")),
                new DynamicGraphRegistryNodeBinding("A", CreateNode("A"))
            ],
            EdgeBindings = []
        };

        var signal = AssertSuccess(new LogicalRegistryDynamicGraphProjector(registry).Project(request));

        Assert.Empty(signal.ProposedTopology.Edges);
        Assert.Equal(["A", "C"], signal.ProposedTopology.Order.Select(entry => entry.NodeId));
    }

    [Fact]
    public void Project_UnicodeScopeAndNodeUseUtf8ByteLengthsInHardcodedOrderIdentityVector()
    {
        const string tenantId = "tenant-😀";
        const string domainTag = "домен-雪";
        const string contextId = "ctx-é";
        const string nodeId = "узел-🧠";
        // Independently precomputed canonical signature (UTF-8 byte lengths, not UTF-16 string lengths):
        // Signature "registry-order-v1" + "\ntenant:11:...\ndomain:14:...\ncontext:6:...\nnode:13:..."; the two Russian strings on lines 429/431 are 14 and 13 UTF-8 bytes respectively.
        const string expectedOrderId =
            "registry-order-136ac1c50eabd94f3680c5c2e7694b18fc66833eb62e84896fdfa257dc51e91a";
        var registry = new StubRegistry(
            [Entry(CreateSubPhase(nodeId), 1)],
            []);
        var request = CreateForkRequest() with
        {
            TenantId = tenantId,
            DomainTag = domainTag,
            ContextId = contextId,
            NodeBindings =
            [new DynamicGraphRegistryNodeBinding(nodeId, CreateNode(nodeId, domainTag))],
            EdgeBindings = []
        };

        Assert.Equal(11, Encoding.UTF8.GetByteCount(tenantId));
        Assert.Equal(14, Encoding.UTF8.GetByteCount(domainTag));
        Assert.Equal(6, Encoding.UTF8.GetByteCount(contextId));
        Assert.Equal(13, Encoding.UTF8.GetByteCount(nodeId));
        var signal = AssertSuccess(new LogicalRegistryDynamicGraphProjector(registry).Project(request));

        Assert.Equal(expectedOrderId, Assert.Single(signal.ProposedTopology.Order).OrderId);
    }

    [Fact]
    public void Project_PropagatesRegistryAndDynamicGraphValidationFailuresExactly()
    {
        var subPhaseFailure = CreateForkRegistry();
        subPhaseFailure.SubPhaseResult = OperationResult<IReadOnlyList<LogicalSubPhaseEntry>>.Fail(
            "sub-phase registry failed",
            "REGISTRY_SUB_PHASE_FAILURE");
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(subPhaseFailure).Project(CreateForkRequest()),
            "REGISTRY_SUB_PHASE_FAILURE");
        Assert.Equal(1, subPhaseFailure.ListSubPhasesCallCount);
        Assert.Equal(1, subPhaseFailure.ListTransitionsCallCount);

        var transitionFailure = CreateForkRegistry();
        transitionFailure.TransitionResult = OperationResult<IReadOnlyList<LogicalTransitionEntry>>.Fail(
            "transition registry failed",
            "REGISTRY_TRANSITION_FAILURE");
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(transitionFailure).Project(CreateForkRequest()),
            "REGISTRY_TRANSITION_FAILURE");

        var invalidAuthority = CreateForkRequest();
        invalidAuthority.EdgeBindings[0] = invalidAuthority.EdgeBindings[0] with { AuthorityUnitLevel = "L3" };
        AssertFailure(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(invalidAuthority),
            "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_INVALID");
    }

    [Fact]
    public void Project_UnexpectedRegistryExceptionIsSanitized()
    {
        var registry = CreateForkRegistry();
        registry.ThrowOnListSubPhases = true;

        var result = new LogicalRegistryDynamicGraphProjector(registry).Project(CreateForkRequest());

        AssertFailure(result, ExceptionCode);
        Assert.DoesNotContain("InvalidOperationException", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("registry secret", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at ", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Project_DoesNotInferEdgeAuthorityOrProofsFromPoisonedLegacyRegistryStrings()
    {
        var registry = CreateForkRegistry();
        registry.TransitionEntries = registry.TransitionEntries
            .Select((entry, index) => entry with
            {
                Transition = entry.Transition with
                {
                    Purpose = "L4 final authority trained DPO explicit-authority-poison",
                    PredicateIds = ["TRAINED_AUTHORITY", "WINNER_SELECTION"],
                    ConstraintIds = ["DETERMINISTIC_FINAL_AUTHORITY"],
                    SourceIds = ["teacher:poison", "checkpoint:poison"]
                },
                Revision = index + 100
            })
            .ToArray();

        var signal = AssertSuccess(
            new LogicalRegistryDynamicGraphProjector(registry).Project(CreateForkRequest()));
        var edge = Assert.Single(signal.ProposedTopology.Edges);

        Assert.Equal("explicit-authority", edge.AuthorityUnitId);
        Assert.False(edge.DeterministicFinalAuthority);
        Assert.Equal("explicit-baseline", edge.BaselineUnitId);
        Assert.Equal("explicit-source-proof", edge.SourceProof);
        Assert.Equal("explicit-cutover-proof", edge.CutoverProof);
        Assert.Equal("explicit-ablation-proof", edge.AblationProofRef);
        Assert.DoesNotContain("poison", string.Join('|',
            edge.AuthorityUnitId,
            edge.BaselineUnitId,
            edge.SourceProof,
            edge.CutoverProof,
            edge.AblationProofRef), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Project_OwnsRequestAndRegistrySnapshotsAfterReturn()
    {
        var registry = CreateForkRegistry();
        var request = CreateForkRequest();
        var signal = AssertSuccess(new LogicalRegistryDynamicGraphProjector(registry).Project(request));

        request.NodeBindings[0] = new DynamicGraphRegistryNodeBinding("mutated", CreateNode("mutated"));
        request.EdgeBindings[0].LogicalTransitionIds[0] = "mutated-transition";
        request.EdgeBindings[0].ExcludedLeakageSignals[0] = "mutated-leakage";
        registry.SubPhaseEntries[0].SubPhase.DomainIds[0] = "mutated-domain";
        registry.TransitionEntries[0].Transition.PredicateIds[0] = "MUTATED_PREDICATE";

        Assert.Equal(["A", "B", "C"], signal.ProposedTopology.Nodes.Select(node => node.NodeId));
        var edge = Assert.Single(signal.ProposedTopology.Edges);
        Assert.Equal(["B", "C"], edge.ToNodeIds);
        Assert.Equal("candidate_index", edge.ExcludedLeakageSignals[0]);
    }

    [Fact]
    public void Project_PublishedBuilderAndRuntimeAcceptRevisionZeroAndExposeRevisionOne()
    {
        var signal = AssertSuccess(
            new LogicalRegistryDynamicGraphProjector(CreateForkRegistry()).Project(CreateForkRequest()));
        var builder = new DeterministicDynamicGraphTopologyBuilder();
        var built = builder.BuildDynamicTopology(signal, ValidBudget());
        Assert.True(built.IsSuccess, $"{built.ErrorCode}: {built.ErrorMessage}");
        Assert.NotNull(built.Value?.Topology);

        var runtime = new InMemoryDynamicGraphTopologyRuntime(builder);
        var applied = runtime.ApplyBuildSignal(new DynamicGraphTopologyApplyRequest(0, signal, ValidBudget()));
        Assert.True(applied.IsSuccess, $"{applied.ErrorCode}: {applied.ErrorMessage}");
        Assert.Equal(0, applied.Value!.PreviousRevision);
        Assert.Equal(1, applied.Value.CurrentRevision);
        Assert.True(applied.Value.RevisionAdvanced);

        var current = runtime.ReadCurrent();
        Assert.True(current.IsSuccess, $"{current.ErrorCode}: {current.ErrorMessage}");
        Assert.Equal(1, current.Value!.Revision);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, current.Value.SourceKind);
        Assert.Equal(Snapshot(signal), Snapshot(current.Value));
    }

    private static StubRegistry CreateForkRegistry(
        bool reverseSubPhases = false,
        bool reverseTransitions = false)
    {
        var subPhases = new[]
        {
            Entry(CreateSubPhase("A"), 1),
            Entry(CreateSubPhase("B"), 2),
            Entry(CreateSubPhase("C"), 3)
        };
        var transitions = new[]
        {
            Entry(CreateTransition("transition-a-b", "A", "B"), 10),
            Entry(CreateTransition("transition-a-c", "A", "C"), 20),
            Entry(CreateTransition("foreign-phase", "phase-a", "phase-b", "phase", "phase"), 30)
        };
        return new StubRegistry(
            reverseSubPhases ? subPhases.Reverse().ToArray() : subPhases,
            reverseTransitions ? transitions.Reverse().ToArray() : transitions);
    }

    private static DynamicGraphRegistryProjectionRequest CreateForkRequest()
        => new(
            TenantId: "tenant-a",
            DomainTag: "domain-a",
            ContextId: "context-a",
            SignalId: "signal-a",
            SourceRef: "registry:source",
            TraceRef: "trace:projection",
            NodeBindings:
            [
                new DynamicGraphRegistryNodeBinding("A", CreateNode("A")),
                new DynamicGraphRegistryNodeBinding("B", CreateNode("B")),
                new DynamicGraphRegistryNodeBinding("C", CreateNode("C"))
            ],
            EdgeBindings:
            [
                CreateEdgeBinding("edge-a-fork", ["transition-a-c", "transition-a-b"])
            ]);

    private static DynamicGraphRegistryEdgeBinding CreateEdgeBinding(
        string edgeId,
        string[] transitionIds)
        => new(
            EdgeId: edgeId,
            LogicalTransitionIds: transitionIds,
            Kind: GraphEdgeKind.FanOut,
            ParallelEligible: true,
            AuthorityUnitId: "explicit-authority",
            AuthorityUnitLevel: "L2",
            DeterministicFinalAuthority: false,
            BaselineUnitId: "explicit-baseline",
            BaselineRole: "candidate_generator",
            ExcludedLeakageSignals: LeakageSignals.ToArray(),
            SourceProof: "explicit-source-proof",
            CutoverProof: "explicit-cutover-proof",
            AblationProofRef: "explicit-ablation-proof");

    private static LogicalSubPhase CreateSubPhase(string id)
        => new(
            SubPhaseId: id,
            PhaseId: "phase-a",
            Name: $"Sub-phase {id}",
            Purpose: $"Purpose for {id}",
            DomainIds: ["domain-a"],
            PredicateIds: ["PROJECT"],
            SourceIds: ["registry-source"]);

    private static LogicalTransition CreateTransition(
        string id,
        string from,
        string to,
        string fromKind = "sub-phase",
        string toKind = "sub-phase")
        => new(
            TransitionId: id,
            FromTopologyKind: fromKind,
            FromTopologyId: from,
            ToTopologyKind: toKind,
            ToTopologyId: to,
            Purpose: "Registry transition metadata is not graph authority.",
            PredicateIds: ["HANDOFF"],
            ConstraintIds: ["PRESERVE_CONTEXT"],
            SourceIds: ["registry-source"]);

    private static TestNode CreateNode(string id, string domainTag = "domain-a")
        => new(
            id,
            new DynamicGraphNodeDescriptor(
                NodeId: id,
                DomainTag: domainTag,
                TInputTypeName: "System.String",
                TOutputTypeName: "System.String",
                IsTrained: false,
                DeterministicFinalAuthority: false,
                RuntimeSemanticQualityClaimed: false,
                AuthorityRole: "baseline_candidate_generator",
                CheckpointRef: null,
                ManifestRef: null));

    private static LogicalSubPhaseEntry Entry(LogicalSubPhase subPhase, long revision)
        => new(subPhase, revision, DateTimeOffset.UnixEpoch.AddSeconds(revision));

    private static LogicalTransitionEntry Entry(LogicalTransition transition, long revision)
        => new(transition, revision, DateTimeOffset.UnixEpoch.AddSeconds(revision));

    private static TopologyBuildBudget ValidBudget()
        => new(MaxNodes: 8, MaxEdges: 8, MaxDepth: 8, MaxParallelBranches: 8);

    private static DynamicGraphBuildSignal AssertSuccess(
        OperationResult<DynamicGraphBuildSignal> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsType<DynamicGraphBuildSignal>(result.Value);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private static void AssertRecordShape<T>((string Name, Type Type)[] expected)
    {
        var type = typeof(T);
        Assert.True(type.IsClass && type.IsSealed);
        Assert.Equal(
            expected,
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => (property.Name, property.PropertyType)));
    }

    private static string Snapshot(DynamicGraphBuildSignal signal)
        => Snapshot(
            signal.SignalId,
            signal.SourceKind,
            signal.SourceRef,
            signal.TraceRef,
            signal.ProposedTopology);

    private static string Snapshot(DynamicGraphTopologyRuntimeSnapshot snapshot)
        => Snapshot(
            snapshot.SignalId,
            snapshot.SourceKind,
            snapshot.SourceRef,
            snapshot.TraceRef,
            snapshot.Topology);

    private static string Snapshot(
        string signalId,
        DynamicGraphBuildSourceKind sourceKind,
        string sourceRef,
        string traceRef,
        DynamicGraphTopology topology)
        => string.Join(
            "||",
            signalId,
            sourceKind,
            sourceRef,
            traceRef,
            topology.TenantId,
            topology.DomainTag,
            topology.ContextId,
            string.Join(';', topology.Nodes.Select(node => $"{node.NodeId}:{node.Descriptor.DomainTag}")),
            string.Join(';', topology.Edges.Select(edge =>
                $"{edge.EdgeId}:{edge.FromNodeId}:{string.Join(',', edge.ToNodeIds)}:{edge.Kind}:{edge.ParallelEligible}:"
                + $"{edge.AuthorityUnitId}:{edge.AuthorityUnitLevel}:{edge.DeterministicFinalAuthority}:"
                + $"{edge.BaselineUnitId}:{edge.BaselineRole}:{string.Join(',', edge.ExcludedLeakageSignals)}:"
                + $"{edge.SourceProof}:{edge.CutoverProof}:{edge.AblationProofRef}")),
            string.Join(';', topology.Order.Select(order =>
                $"{order.OrderId}:{order.NodeId}:{order.TenantId}:{order.DomainTag}:{order.ContextId}:{order.Position}")));

    private sealed record TestNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

    private sealed class StubRegistry(
        LogicalSubPhaseEntry[] subPhaseEntries,
        LogicalTransitionEntry[] transitionEntries) : ILogicalTopologyRegistry
    {
        public LogicalSubPhaseEntry[] SubPhaseEntries { get; set; } = subPhaseEntries;

        public LogicalTransitionEntry[] TransitionEntries { get; set; } = transitionEntries;

        public OperationResult<IReadOnlyList<LogicalSubPhaseEntry>>? SubPhaseResult { get; set; }

        public OperationResult<IReadOnlyList<LogicalTransitionEntry>>? TransitionResult { get; set; }

        public bool ThrowOnListSubPhases { get; set; }

        public int ListSubPhasesCallCount { get; private set; }

        public int ListTransitionsCallCount { get; private set; }

        public OperationResult<IReadOnlyList<LogicalSubPhaseEntry>> ListSubPhases()
        {
            ListSubPhasesCallCount++;
            if (ThrowOnListSubPhases)
                throw new InvalidOperationException("registry secret");

            return SubPhaseResult
                   ?? OperationResult<IReadOnlyList<LogicalSubPhaseEntry>>.Ok(SubPhaseEntries);
        }

        public OperationResult<IReadOnlyList<LogicalTransitionEntry>> ListTransitions()
        {
            ListTransitionsCallCount++;
            return TransitionResult
                   ?? OperationResult<IReadOnlyList<LogicalTransitionEntry>>.Ok(TransitionEntries);
        }

        public OperationResult<LogicalDomainEntry> AddOrUpdateDomain(LogicalDomain domain)
            => throw new NotSupportedException();

        public OperationResult<LogicalDomainEntry> ResolveDomain(string domainId)
            => throw new NotSupportedException();

        public OperationResult<IReadOnlyList<LogicalDomainEntry>> ListDomains()
            => throw new NotSupportedException();

        public OperationResult<LogicalPhaseEntry> AddOrUpdatePhase(LogicalPhase phase)
            => throw new NotSupportedException();

        public OperationResult<LogicalPhaseEntry> ResolvePhase(string phaseId)
            => throw new NotSupportedException();

        public OperationResult<IReadOnlyList<LogicalPhaseEntry>> ListPhases()
            => throw new NotSupportedException();

        public OperationResult<LogicalSubPhaseEntry> AddOrUpdateSubPhase(LogicalSubPhase subPhase)
            => throw new NotSupportedException();

        public OperationResult<LogicalSubPhaseEntry> ResolveSubPhase(string subPhaseId)
            => throw new NotSupportedException();

        public OperationResult<LogicalTransitionEntry> AddOrUpdateTransition(LogicalTransition transition)
            => throw new NotSupportedException();

        public OperationResult<LogicalTransitionEntry> ResolveTransition(string transitionId)
            => throw new NotSupportedException();
    }
}
