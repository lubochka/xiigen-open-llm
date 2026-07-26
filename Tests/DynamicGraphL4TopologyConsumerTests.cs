// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Collections.Concurrent;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DynamicDomains;
using ExperimentsV2.LogicalFlow;
using OrchestratorModel.Logical;
using ProjectWikiDomain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphL4TopologyConsumerTests
{
    private const string TenantId = "tenant-l4";
    private const string DomainTag = "domain-l4";
    private const string ContextId = "context-l4";
    private const string NodeA = "3.1";
    private const string NodeB = "4.1";
    private const string NodeC = "5.1";
    private const string FallbackPrefix = "DYNAMIC_GRAPH_L4_TOPOLOGY_FALLBACK:";
    private const string ProjectionRule =
        "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)";

    private static readonly string[] RequiredLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void PublicAndPrivateSurface_PinsLegacyAndOptInNodeOutputContracts()
    {
        var type = typeof(AdaptiveLogicalOrchestrator);
        var expectedNames = new[]
        {
            "topology",
            "promptCompiler",
            "router",
            "flows",
            "units",
            "multiRouter",
            "learningEventScheduler",
            "capabilityTrainer",
            "dynamicGraphTopologySource"
        };
        var expectedTypes = new[]
        {
            typeof(ILogicalTopologyRegistry),
            typeof(ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>),
            typeof(ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>),
            typeof(InMemoryLogicalFlowRegistry),
            typeof(InMemoryLogicalUnitRegistry),
            typeof(ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>),
            typeof(LearningEventScheduler),
            typeof(ILogicalCapabilityTrainer),
            typeof(IDynamicGraphTopologySource)
        };
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(constructor => constructor.GetParameters().Length)
            .ThenBy(ConstructorSignature, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(2, constructors.Length);
        var legacy = constructors[0];
        Assert.Equal(9, legacy.GetParameters().Length);
        AssertParameterContract(legacy.GetParameters(), expectedNames, expectedTypes);
        Assert.Equal(
            [false, false, false, false, false, true, true, true, true],
            legacy.GetParameters().Select(parameter => parameter.HasDefaultValue));
        Assert.All(legacy.GetParameters().Skip(5), parameter => Assert.Null(parameter.DefaultValue));

        var optIn = constructors[1];
        Assert.Equal(10, optIn.GetParameters().Length);
        AssertParameterContract(
            optIn.GetParameters(),
            [.. expectedNames, "dynamicGraphNodeOutputSource"],
            [.. expectedTypes, typeof(IDynamicGraphNodeOutputSource)]);
        Assert.All(optIn.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var topologySourceField = Assert.Single(
            fields,
            candidate => candidate.FieldType == typeof(IDynamicGraphTopologySource));
        Assert.Equal("_dynamicGraphTopologySource", topologySourceField.Name);
        Assert.True(topologySourceField.IsPrivate);
        Assert.True(topologySourceField.IsInitOnly);

        var nodeOutputSourceField = Assert.Single(
            fields,
            candidate => candidate.FieldType == typeof(IDynamicGraphNodeOutputSource));
        Assert.Equal("_dynamicGraphNodeOutputSource", nodeOutputSourceField.Name);
        Assert.True(nodeOutputSourceField.IsPrivate);
        Assert.True(nodeOutputSourceField.IsInitOnly);

        var requestedField = Assert.Single(
            fields,
            candidate => candidate.Name == "_dynamicGraphNodeOutputSourceRequested");
        Assert.Equal(typeof(bool), requestedField.FieldType);
        Assert.True(requestedField.IsPrivate);
        Assert.True(requestedField.IsInitOnly);

        var projectorField = Assert.Single(
            fields,
            candidate => candidate.Name == "_dynamicGraphNodeOutputProjector");
        Assert.Equal(typeof(DeterministicDynamicGraphNodeOutputProjector), projectorField.FieldType);
        Assert.True(projectorField.IsPrivate);
        Assert.True(projectorField.IsInitOnly);
        Assert.DoesNotContain(fields, candidate => candidate.FieldType == typeof(IDynamicGraphTopologyRuntimeOwner));

        var factory = Assert.IsAssignableFrom<MethodInfo>(typeof(LogicalOrchestratorAssistant).GetMethod(
            nameof(LogicalOrchestratorAssistant.CreateDefault),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [typeof(ISessionStorage), typeof(ProjectWikiTrainingFlowAdapter), typeof(IDynamicGraphTopologySource)],
            modifiers: null));
        Assert.Single(
            typeof(LogicalOrchestratorAssistant).GetMethods(BindingFlags.Public | BindingFlags.Static),
            method => method.Name == nameof(LogicalOrchestratorAssistant.CreateDefault));
        var factoryLast = factory.GetParameters()[^1];
        Assert.Equal("dynamicGraphTopologySource", factoryLast.Name);
        Assert.Equal(typeof(IDynamicGraphTopologySource), factoryLast.ParameterType);
        Assert.DoesNotContain(
            typeof(LogicalOrchestratorAssistant).GetFields(BindingFlags.NonPublic | BindingFlags.Instance),
            candidate => typeof(IDynamicGraphTopologySource).IsAssignableFrom(candidate.FieldType));

        var forbidden = new[] { "ApplyBuildSignal", "ListRevisions", "Projector", "Builder", "Owner" };
        Assert.DoesNotContain(
            type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            member => forbidden.Any(token => member.Name.Contains(token, StringComparison.OrdinalIgnoreCase)));
        Assert.DoesNotContain(
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType)),
            exposedType => exposedType == typeof(IDynamicGraphTopologyRuntimeOwner)
                           || exposedType == typeof(DynamicGraphBuildSignal)
                           || exposedType == typeof(IDynamicGraphNodeOutputProjector));
    }

    [Fact]
    public async Task AdaptiveRun_ReadsOneSnapshotAndKeepsItConsistentAcrossHelpers()
    {
        var firstTopology = CreateTopology(
            [NodeA, NodeB],
            [CreateEdge("edge-a-b", NodeA, [NodeB], parallelEligible: false)]);
        var laterTopology = CreateTopology([NodeA, NodeB], []);
        var source = new TestTopologySource(call => OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Ok(
            CreateSnapshot(call == 1 ? firstTopology : laterTopology)));
        var rig = BuildRig(source, [Decision(NodeB), Decision(NodeA)]);

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("single-snapshot"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, source.CallCount);
        Assert.Equal(1, rig.Topology.ListSubPhasesCallCount);
        Assert.Equal([NodeA, NodeB], rig.CompletionOrder);
        Assert.True(rig.ContextSeen[NodeB]);
    }

    [Fact]
    public async Task ExplicitScenario_DoesNotReadDynamicTopologySource()
    {
        var source = new TestTopologySource(_ => throw new InvalidOperationException("must not be read"));
        var rig = BuildRig(source, [Decision(NodeA)]);

        var result = await rig.Orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "explicit-no-read",
            TestRig.ExplicitScenarioId,
            "Run the explicit compatibility scenario.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, source.CallCount);
        Assert.Equal(TestRig.ExplicitFlowId, result.Value!.SelectedFlowId);
    }

    [Fact]
    public async Task SuccessfulEmptyDynamicEdges_AreAuthoritativeOverLegacyTransitions()
    {
        var source = SuccessSource(CreateTopology([NodeA, NodeB], []));
        var probe = new ConcurrencyProbe(2);
        var rig = BuildRig(
            source,
            [Decision(NodeA), Decision(NodeB)],
            legacyTransitions: [(NodeA, NodeB)],
            probe: probe);

        var run = rig.Orchestrator.RunAsync(CreateAdaptiveRequest("empty-authoritative"));
        var allStarted = await probe.WaitForAllStartedAsync();
        probe.Release();
        var result = await run;

        Assert.True(allStarted, "Successful empty dynamic edges must not fall back to the sequential legacy transition.");
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.BranchResults.Length);
        Assert.DoesNotContain(result.Value.Warnings, warning => warning.StartsWith(FallbackPrefix, StringComparison.Ordinal));
    }

    [Fact]
    public async Task NonParallelDynamicEdge_OrdersBranchesAndCarriesContext()
    {
        var topology = CreateTopology(
            [NodeA, NodeB],
            [CreateEdge("edge-a-b", NodeA, [NodeB], parallelEligible: false)]);
        var rig = BuildRig(SuccessSource(topology), [Decision(NodeB), Decision(NodeA)]);

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("nonparallel-order"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal([NodeA, NodeB], rig.CompletionOrder);
        Assert.True(rig.ContextSeen[NodeB]);
    }

    [Fact]
    public async Task ParallelEligibleDynamicEdge_RunsBranchesConcurrently()
    {
        var topology = CreateTopology(
            [NodeA, NodeB],
            [CreateEdge("edge-a-b-parallel", NodeA, [NodeB], parallelEligible: true)]);
        var probe = new ConcurrencyProbe(2);
        var rig = BuildRig(SuccessSource(topology), [Decision(NodeA), Decision(NodeB)], probe: probe);

        var run = rig.Orchestrator.RunAsync(CreateAdaptiveRequest("parallel-edge"));
        var allStarted = await probe.WaitForAllStartedAsync();
        probe.Release();
        var result = await run;

        Assert.True(allStarted, "Parallel-eligible edge endpoints should both start before either is released.");
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    [Fact]
    public async Task StructuralParallelPath_AddsBridgeWithoutSequentialAuthority()
    {
        var topology = CreateTopology(
            [NodeA, NodeB, NodeC],
            [
                CreateEdge("edge-a-b-parallel", NodeA, [NodeB], parallelEligible: true),
                CreateEdge("edge-b-c-parallel", NodeB, [NodeC], parallelEligible: true)
            ]);
        var probe = new ConcurrencyProbe(3);
        var rig = BuildRig(SuccessSource(topology), [Decision(NodeA), Decision(NodeC)], probe: probe);

        var run = rig.Orchestrator.RunAsync(CreateAdaptiveRequest("structural-bridge"));
        var allStarted = await probe.WaitForAllStartedAsync();
        probe.Release();
        var result = await run;

        Assert.True(allStarted, "A structural-only bridge must not acquire sequential authority.");
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.BranchResults.Length);
        Assert.Contains(result.Value.BranchResults, branch => branch.Metrics["sub_phase_id"] == NodeB);
    }

    [Fact]
    public async Task ReturnLoop_IsExcludedFromStructuralAndDependencyViews()
    {
        var topology = CreateTopology(
            [NodeA, NodeB],
            [CreateEdge("edge-return", NodeB, [NodeA], parallelEligible: false, kind: GraphEdgeKind.ReturnLoop)]);
        var probe = new ConcurrencyProbe(2);
        var rig = BuildRig(SuccessSource(topology), [Decision(NodeA), Decision(NodeB)], probe: probe);

        var run = rig.Orchestrator.RunAsync(CreateAdaptiveRequest("return-loop-excluded"));
        var allStarted = await probe.WaitForAllStartedAsync();
        probe.Release();
        var result = await run;

        Assert.True(allStarted, "ReturnLoop must not serialize ordinary L4 branch execution.");
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.BranchResults.Length);
    }

    [Fact]
    public async Task SourceFailure_FallsBackToLegacyWithExactNormalizedWarningAndNoWarningLoss()
    {
        var source = new TestTopologySource(_ =>
            OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Fail("source unavailable", "source-down"));
        var rig = BuildRig(
            source,
            [Decision(NodeB), Decision(NodeA)],
            legacyTransitions: [(NodeA, NodeB)]);

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("source-fallback"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal([NodeA, NodeB], rig.CompletionOrder);
        Assert.Equal(1, source.CallCount);
        Assert.Contains("FLOW_WARNING_3.1", result.Value!.Warnings);
        Assert.Equal(1, result.Value.Warnings.Count(warning => warning == FallbackPrefix + "SOURCE_DOWN"));
    }

    [Fact]
    public async Task BlankSourceFailureCode_UsesSourceFailedFallbackWarning()
    {
        var source = new TestTopologySource(_ =>
            OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Fail("source unavailable", " "));
        var rig = BuildRig(source, [Decision(NodeA)]);

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("blank-source-code"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains(FallbackPrefix + "SOURCE_FAILED", result.Value!.Warnings);
    }

    [Theory]
    [InlineData("exception", "SOURCE_EXCEPTION")]
    [InlineData("null-operation", "SOURCE_RESULT_REQUIRED")]
    public async Task ExceptionalAndNullSourceResults_FallBackWithoutLeakingDetails(string mode, string expectedCode)
    {
        var source = new TestTopologySource(_ => mode == "exception"
            ? throw new InvalidOperationException("secret source detail")
            : null!);
        var rig = BuildRig(source, [Decision(NodeA)]);

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("source-sanitized"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains(FallbackPrefix + expectedCode, result.Value!.Warnings);
        Assert.DoesNotContain(result.Value.Warnings, warning => warning.Contains("secret", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("null-snapshot", "DYNAMIC_GRAPH_L4_TOPOLOGY_SNAPSHOT_REQUIRED")]
    [InlineData("invalid-revision", "DYNAMIC_GRAPH_L4_TOPOLOGY_REVISION_INVALID")]
    [InlineData("invalid-topology", "DYNAMIC_GRAPH_DOMAIN_MISMATCH")]
    public async Task CorruptSuccessfulSource_IsAHardFailure(string mode, string expectedCode)
    {
        var valid = CreateTopology([NodeA], []);
        var source = new TestTopologySource(_ => mode switch
        {
            "null-snapshot" => OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Ok(null!),
            "invalid-revision" => OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Ok(CreateSnapshot(valid, revision: 0)),
            "invalid-topology" => OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Ok(CreateSnapshot(
                new DynamicGraphTopology(
                    TenantId,
                    DomainTag,
                    ContextId,
                    [CreateNode(NodeA, descriptorDomain: "foreign-domain")],
                    [],
                    [CreateOrder(NodeA, 0)]))),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        });
        var rig = BuildRig(source, [Decision(NodeA)]);

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("corrupt-success"));

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Empty(rig.CompletionOrder);
    }

    [Fact]
    public async Task SuccessfulTopologyWithUnknownNode_FailsBeforeRouting()
    {
        var topology = CreateTopology([NodeA, "9.9"], []);
        var rig = BuildRig(SuccessSource(topology), [Decision(NodeA)]);

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("unknown-node"));

        Assert.False(result.IsSuccess);
        Assert.Equal("DYNAMIC_GRAPH_L4_TOPOLOGY_NODE_NOT_REGISTERED", result.ErrorCode);
        Assert.Contains("9.9", result.ErrorMessage);
        Assert.Empty(rig.CompletionOrder);
    }

    [Fact]
    public async Task LogicalRegistryListSubPhasesFailure_IsPropagatedExactly()
    {
        var topology = CreateTopology([NodeA], []);
        var rig = BuildRig(SuccessSource(topology), [Decision(NodeA)]);
        rig.Topology.ListSubPhasesFailure = OperationResult<IReadOnlyList<LogicalSubPhaseEntry>>.Fail(
            "sentinel registry failure",
            "SENTINEL_REGISTRY_CODE");

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("registry-failure"));

        Assert.False(result.IsSuccess);
        Assert.Equal("SENTINEL_REGISTRY_CODE", result.ErrorCode);
        Assert.Equal("sentinel registry failure", result.ErrorMessage);
        Assert.Equal(1, rig.Topology.ListSubPhasesCallCount);
    }

    [Fact]
    public async Task AssistantDefaultFactory_PassesReadOnlySourceIntoAdaptiveOrchestrator()
    {
        var source = new TestTopologySource(_ =>
            OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Fail("offline", "SOURCE_OFFLINE"));
        var assistant = LogicalOrchestratorAssistant.CreateDefault(
            new NullSessionStorage(),
            dynamicGraphTopologySource: source);
        Assert.True(assistant.IsSuccess, assistant.ErrorMessage);

        var result = await assistant.Value!.RunAsync(BuildSecurityPrompt());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, source.CallCount);
        Assert.Equal(SecurityVerification34FlowAdapter.FlowId, result.Value!.SelectedFlowId);
    }

    [Fact]
    public async Task NoSource_PreservesLegacyOrderingWithoutFallbackWarning()
    {
        var rig = BuildRig(
            source: null,
            decisions: [Decision(NodeB), Decision(NodeA)],
            legacyTransitions: [(NodeA, NodeB)]);

        var result = await rig.Orchestrator.RunAsync(CreateAdaptiveRequest("legacy-no-source"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal([NodeA, NodeB], rig.CompletionOrder);
        Assert.DoesNotContain(result.Value!.Warnings, warning => warning.StartsWith(FallbackPrefix, StringComparison.Ordinal));
        Assert.Contains("FLOW_WARNING_3.1", result.Value.Warnings);
    }

    [Fact]
    public void DynamicTransitionView_ExactPrivateShapeCarriesTopologyRevisionAndProjectionByIdentity()
    {
        var topology = CreateTopology(
            [NodeB, NodeA, NodeC],
            [
                CreateEdge("edge-a-many", NodeA, [NodeC, NodeB], parallelEligible: true),
                CreateEdge("edge-a-b-sequential", NodeA, [NodeB], parallelEligible: false),
                CreateEdge("edge-b-c", NodeB, [NodeC], parallelEligible: false)
            ]);
        const long revision = 73;
        var projection = new NodeOutputToEdgeInputAdapter(
            NodeA,
            Array.AsReadOnly(new[] { "projection-sentinel" }),
            "sha256:projection-sentinel",
            ProjectionRule);
        var method = AssertExactPrivateStaticMethod(
            "BuildDynamicTransitionView",
            [typeof(DynamicGraphTopology), typeof(long), typeof(NodeOutputToEdgeInputAdapter)]);
        var viewType = method.ReturnType;
        Assert.Equal("DynamicGraphTransitionView", viewType.Name);
        Assert.False(viewType.IsVisible);
        Assert.Equal(
            new Dictionary<string, Type>(StringComparer.Ordinal)
            {
                ["StructuralTransitions"] = typeof(DynamicDomainTransition[]),
                ["DependencyTransitions"] = typeof(DynamicDomainTransition[]),
                ["Warnings"] = typeof(string[]),
                ["Topology"] = typeof(DynamicGraphTopology),
                ["TopologyRevision"] = typeof(long?),
                ["NodeOutputProjection"] = typeof(NodeOutputToEdgeInputAdapter)
            },
            viewType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToDictionary(property => property.Name, property => property.PropertyType, StringComparer.Ordinal));

        var view = method.Invoke(null, [topology, revision, projection]);
        Assert.NotNull(view);
        var structural = Assert.IsType<DynamicDomainTransition[]>(
            ReadViewProperty(view, "StructuralTransitions"));
        var dependencies = Assert.IsType<DynamicDomainTransition[]>(
            ReadViewProperty(view, "DependencyTransitions"));

        Assert.Equal(
            [$"{NodeB}>{NodeC}", $"{NodeA}>{NodeB}", $"{NodeA}>{NodeC}"],
            structural.Select(TransitionLabel));
        Assert.Equal(
            [$"{NodeB}>{NodeC}", $"{NodeA}>{NodeB}"],
            dependencies.Select(TransitionLabel));
        Assert.Same(topology, ReadViewProperty(view, "Topology"));
        Assert.Equal(revision, Assert.IsType<long>(ReadViewProperty(view, "TopologyRevision")));
        Assert.Same(projection, ReadViewProperty(view, "NodeOutputProjection"));

        var withoutProjection = method.Invoke(null, [topology, revision, null]);
        Assert.NotNull(withoutProjection);
        Assert.Equal(viewType, withoutProjection.GetType());
        Assert.Same(topology, ReadViewProperty(withoutProjection, "Topology"));
        Assert.Equal(revision, Assert.IsType<long>(ReadViewProperty(withoutProjection, "TopologyRevision")));
        Assert.Null(ReadViewProperty(withoutProjection, "NodeOutputProjection"));

        var followUpMethod = AssertExactPrivateStaticMethod(
            "BuildInvocationFollowUpTransitionView",
            [viewType]);
        Assert.Equal(viewType, followUpMethod.ReturnType);
        var followUp = followUpMethod.Invoke(null, [view]);
        Assert.NotNull(followUp);
        Assert.Equal(viewType, followUp.GetType());
        Assert.NotSame(structural, ReadViewProperty(followUp, "StructuralTransitions"));
        Assert.NotSame(dependencies, ReadViewProperty(followUp, "DependencyTransitions"));
        Assert.Same(topology, ReadViewProperty(followUp, "Topology"));
        Assert.Equal(revision, Assert.IsType<long>(ReadViewProperty(followUp, "TopologyRevision")));
        Assert.Null(ReadViewProperty(followUp, "NodeOutputProjection"));
    }

    [Fact]
    public void ProvidedNodeOutputs_ProjectDeterministicallyIntoExactTransitionView()
    {
        const string runId = "provided-projection";
        const long revision = 41;
        var topology = CreateTopology([NodeA], []);
        var topologySnapshot = CreateSnapshot(topology, revision);
        var constructorSource = new TestTopologySource(_ => throw new InvalidOperationException("must not be read"));
        var rig = BuildRig(constructorSource, [Decision(NodeA)]);
        var firstSource = new TestNodeOutputSource(requestedRunId =>
            OperationResult<DynamicGraphNodeOutputSnapshot>.Ok(CreateNodeOutputSnapshot(
                requestedRunId,
                revision,
                NodeA,
                [CreateCandidate("candidate-z", NodeA), CreateCandidate("candidate-a", NodeA)])));
        var secondSource = new TestNodeOutputSource(requestedRunId =>
            OperationResult<DynamicGraphNodeOutputSnapshot>.Ok(CreateNodeOutputSnapshot(
                requestedRunId,
                revision,
                NodeA,
                [CreateCandidate("candidate-a", NodeA), CreateCandidate("candidate-z", NodeA)])));
        var method = AssertExactPrivateInstanceMethod(
            "ReadProvidedTransitionView",
            [typeof(string), typeof(DynamicGraphTopologyRuntimeSnapshot), typeof(IDynamicGraphNodeOutputSource)]);

        var firstView = AssertSuccessfulOperationValue(method.Invoke(
            rig.Orchestrator,
            [runId, topologySnapshot, firstSource]));
        var secondView = AssertSuccessfulOperationValue(method.Invoke(
            rig.Orchestrator,
            [runId, topologySnapshot, secondSource]));
        var firstProjection = Assert.IsType<NodeOutputToEdgeInputAdapter>(
            ReadViewProperty(firstView, "NodeOutputProjection"));
        var secondProjection = Assert.IsType<NodeOutputToEdgeInputAdapter>(
            ReadViewProperty(secondView, "NodeOutputProjection"));

        Assert.Equal(NodeA, firstProjection.SourceNodeId);
        Assert.Equal(["candidate-a", "candidate-z"], firstProjection.UpstreamCandidateRefs);
        Assert.StartsWith("sha256:", firstProjection.UpstreamOutputHash, StringComparison.Ordinal);
        Assert.Equal(71, firstProjection.UpstreamOutputHash.Length);
        Assert.Equal(ProjectionRule, firstProjection.ProjectionRule);
        Assert.Equal(firstProjection.SourceNodeId, secondProjection.SourceNodeId);
        Assert.Equal(firstProjection.UpstreamCandidateRefs, secondProjection.UpstreamCandidateRefs);
        Assert.Equal(firstProjection.UpstreamOutputHash, secondProjection.UpstreamOutputHash);
        Assert.Equal(firstProjection.ProjectionRule, secondProjection.ProjectionRule);
        Assert.Equal([runId], firstSource.RequestedRunIds);
        Assert.Equal([runId], secondSource.RequestedRunIds);
        Assert.Equal(0, constructorSource.CallCount);
    }

    [Fact]
    public async Task InvocationNodeOutputs_ReadOnceUseCallerTopologyAndDoNotReplayProjectionOnFollowUp()
    {
        const string runId = "provided-follow-up";
        const long revision = 29;
        var topology = CreateTopology(
            [NodeA, NodeB],
            [CreateEdge("edge-b-a-return", NodeB, [NodeA], parallelEligible: false, kind: GraphEdgeKind.ReturnLoop)]);
        var constructorSource = new TestTopologySource(_ => throw new InvalidOperationException("must not be read"));
        var rig = BuildRig(
            constructorSource,
            [Decision(NodeB)],
            transitionSourceNodeId: NodeA);
        var nodeOutputSource = new TestNodeOutputSource(requestedRunId =>
            OperationResult<DynamicGraphNodeOutputSnapshot>.Ok(CreateNodeOutputSnapshot(
                requestedRunId,
                revision,
                NodeB,
                [CreateCandidate("candidate-z", NodeB), CreateCandidate("candidate-a", NodeB)])));

        var result = await rig.Orchestrator.RunWithInvocationNodeOutputsAsync(
            CreateAdaptiveRequest(runId),
            CreateSnapshot(topology, revision),
            nodeOutputSource);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(0, constructorSource.CallCount);
        Assert.Equal(1, nodeOutputSource.CallCount);
        Assert.Equal([runId], nodeOutputSource.RequestedRunIds);
        Assert.Equal(2, rig.CompletionOrder.Count(nodeId => nodeId == NodeB));
        Assert.Equal(1, rig.CompletionOrder.Count(nodeId => nodeId == NodeA));
        var consumedReturnLoop = Assert.Single(result.Value!.BranchResults, branch =>
            branch.Metrics.TryGetValue("edge_kind", out var kind)
            && StringComparer.Ordinal.Equals(kind, GraphEdgeKind.ReturnLoop.ToString()));
        Assert.Equal("true", consumedReturnLoop.Metrics["handoff_consumed"]);
        Assert.Equal("true", consumedReturnLoop.Metrics["targets_executed"]);
        Assert.Equal(
            "baseline_candidate_generator",
            consumedReturnLoop.Metrics["routing_authority_role"]);
        var returnTarget = Assert.Single(result.Value.BranchResults, branch =>
            branch.Metrics.TryGetValue("routed_sub_phase_id", out var subPhaseId)
            && StringComparer.Ordinal.Equals(subPhaseId, NodeA));
        Assert.Equal(
            SubPhaseRoutingDecision.BaselineAuthorityRole,
            returnTarget.Metrics[SubPhaseRoutingDecision.AuthorityRoleMetadataKey]);
    }

    [Fact]
    public async Task InvocationNodeOutputs_LearnedTargetPublishesTrainedAuthorityRole()
    {
        const string runId = "provided-learned-authority";
        const long revision = 31;
        var topology = CreateTopology(
            [NodeB, NodeC],
            [CreateEdge("edge-b-c-learned", NodeB, [NodeC], parallelEligible: false)]);
        var constructorSource = new TestTopologySource(_ => throw new InvalidOperationException("must not be read"));
        var rig = BuildRig(
            constructorSource,
            [Decision(NodeB)],
            learnedAuthorityTargetNodeId: NodeC);
        var nodeOutputSource = new TestNodeOutputSource(requestedRunId =>
            OperationResult<DynamicGraphNodeOutputSnapshot>.Ok(CreateNodeOutputSnapshot(
                requestedRunId,
                revision,
                NodeB,
                [CreateCandidate("candidate-learned", NodeB)])));

        var result = await rig.Orchestrator.RunWithInvocationNodeOutputsAsync(
            CreateAdaptiveRequest(runId) with
            {
                UserRequest = "Implement target sub-phase 5.1 and verify it with tests."
            },
            CreateSnapshot(topology, revision),
            nodeOutputSource);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var consumedLearnedHandoff = Assert.Single(result.Value!.BranchResults, branch =>
            branch.Metrics.TryGetValue("edge_id", out var edgeId)
            && StringComparer.Ordinal.Equals(edgeId, "edge-b-c-learned")
            && branch.Metrics.TryGetValue("handoff_consumed", out var consumed)
            && StringComparer.Ordinal.Equals(consumed, "true"));
        Assert.Equal(
            "trained_edge_unit",
            consumedLearnedHandoff.Metrics["routing_authority_role"]);
        var learnedTarget = Assert.Single(result.Value!.BranchResults, branch =>
            branch.Metrics.TryGetValue("routed_sub_phase_id", out var subPhaseId)
            && StringComparer.Ordinal.Equals(subPhaseId, NodeC));
        Assert.Equal(
            SubPhaseRoutingDecision.TrainedAuthorityRole,
            learnedTarget.Metrics[SubPhaseRoutingDecision.AuthorityRoleMetadataKey]);
    }

    [Theory]
    [InlineData("null-result", "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_RESULT_REQUIRED")]
    [InlineData("failed-result", "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_FAILED")]
    [InlineData("null-snapshot", "DYNAMIC_GRAPH_NODE_OUTPUT_SNAPSHOT_REQUIRED")]
    [InlineData("run-invalid", "DYNAMIC_GRAPH_NODE_OUTPUT_RUN_ID_INVALID")]
    [InlineData("run-mismatch", "DYNAMIC_GRAPH_NODE_OUTPUT_RUN_ID_MISMATCH")]
    [InlineData("revision-invalid", "DYNAMIC_GRAPH_NODE_OUTPUT_REVISION_INVALID")]
    [InlineData("revision-mismatch", "DYNAMIC_GRAPH_NODE_OUTPUT_REVISION_MISMATCH")]
    [InlineData("source-invalid", "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_NODE_INVALID")]
    [InlineData("source-missing", "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_NODE_NOT_FOUND")]
    [InlineData("candidate-source-mismatch", "DYNAMIC_GRAPH_NODE_OUTPUT_CANDIDATE_SOURCE_NODE_MISMATCH")]
    [InlineData("candidate-id-invalid", "DYNAMIC_GRAPH_NODE_OUTPUT_IDENTITY_INVALID")]
    public async Task InvocationNodeOutputs_InvalidSourceSnapshotFailsWithStableCodeBeforeFlowExecution(
        string mode,
        string expectedCode)
    {
        const string runId = "provided-invalid";
        const long revision = 17;
        var topology = CreateTopology([NodeA], []);
        var constructorSource = new TestTopologySource(_ => throw new InvalidOperationException("must not be read"));
        var rig = BuildRig(constructorSource, [Decision(NodeA)]);
        var source = new TestNodeOutputSource(requestedRunId => mode switch
        {
            "null-result" => null!,
            "failed-result" => OperationResult<DynamicGraphNodeOutputSnapshot>.Fail("source failed", "SOURCE_FAILED"),
            "null-snapshot" => OperationResult<DynamicGraphNodeOutputSnapshot>.Ok(null!),
            "run-invalid" => NodeOutputSuccess(" invalid-run", revision, NodeA, CreateCandidate("candidate-a", NodeA)),
            "run-mismatch" => NodeOutputSuccess("other-run", revision, NodeA, CreateCandidate("candidate-a", NodeA)),
            "revision-invalid" => NodeOutputSuccess(requestedRunId, 0, NodeA, CreateCandidate("candidate-a", NodeA)),
            "revision-mismatch" => NodeOutputSuccess(requestedRunId, revision + 1, NodeA, CreateCandidate("candidate-a", NodeA)),
            "source-invalid" => NodeOutputSuccess(requestedRunId, revision, " invalid-source", CreateCandidate("candidate-a", " invalid-source")),
            "source-missing" => NodeOutputSuccess(requestedRunId, revision, NodeC, CreateCandidate("candidate-a", NodeC)),
            "candidate-source-mismatch" => NodeOutputSuccess(requestedRunId, revision, NodeA, CreateCandidate("candidate-a", NodeB)),
            "candidate-id-invalid" => NodeOutputSuccess(requestedRunId, revision, NodeA, CreateCandidate(" invalid-candidate", NodeA)),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        });

        var result = await rig.Orchestrator.RunWithInvocationNodeOutputsAsync(
            CreateAdaptiveRequest(runId),
            CreateSnapshot(topology, revision),
            source);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(1, source.CallCount);
        Assert.Equal([runId], source.RequestedRunIds);
        Assert.Equal(0, constructorSource.CallCount);
        Assert.Empty(rig.CompletionOrder);
    }

    [Fact]
    public async Task InvocationNodeOutputs_InvalidCallerTopologyOrNullSourceFailsBeforeSourceOrFlowExecution()
    {
        const string runId = "provided-invalid-topology";
        var topology = CreateTopology([NodeA], []);
        var constructorSource = new TestTopologySource(_ => throw new InvalidOperationException("must not be read"));
        var rig = BuildRig(constructorSource, [Decision(NodeA)]);
        var source = new TestNodeOutputSource(_ => throw new InvalidOperationException("must not be read"));

        var missingTopology = await rig.Orchestrator.RunWithInvocationNodeOutputsAsync(
            CreateAdaptiveRequest(runId),
            null!,
            source);
        var invalidRevision = await rig.Orchestrator.RunWithInvocationNodeOutputsAsync(
            CreateAdaptiveRequest(runId),
            CreateSnapshot(topology, revision: 0),
            source);
        var invalidTopology = await rig.Orchestrator.RunWithInvocationNodeOutputsAsync(
            CreateAdaptiveRequest(runId),
            CreateSnapshot(new DynamicGraphTopology(
                TenantId,
                DomainTag,
                ContextId,
                [CreateNode(NodeA, descriptorDomain: "foreign-domain")],
                [],
                [CreateOrder(NodeA, 0)])),
            source);
        var missingSource = await rig.Orchestrator.RunWithInvocationNodeOutputsAsync(
            CreateAdaptiveRequest(runId),
            CreateSnapshot(topology),
            null!);

        Assert.Equal("DYNAMIC_GRAPH_NODE_OUTPUT_TOPOLOGY_SNAPSHOT_REQUIRED", AssertFailureCode(missingTopology));
        Assert.Equal("DYNAMIC_GRAPH_NODE_OUTPUT_TOPOLOGY_REVISION_INVALID", AssertFailureCode(invalidRevision));
        Assert.Equal("DYNAMIC_GRAPH_NODE_OUTPUT_TOPOLOGY_INVALID", AssertFailureCode(invalidTopology));
        Assert.Equal("DYNAMIC_GRAPH_NODE_OUTPUT_INVOCATION_SOURCE_REQUIRED", AssertFailureCode(missingSource));
        Assert.Equal(0, source.CallCount);
        Assert.Equal(0, constructorSource.CallCount);
        Assert.Empty(rig.CompletionOrder);
    }

    private static string TransitionLabel(DynamicDomainTransition transition)
        => $"{transition.FromId}>{transition.ToId}";

    private static string ConstructorSignature(ConstructorInfo constructor)
        => string.Join(
            "|",
            constructor.GetParameters().Select(parameter => parameter.ParameterType.FullName));

    private static void AssertParameterContract(
        ParameterInfo[] parameters,
        IReadOnlyList<string> expectedNames,
        IReadOnlyList<Type> expectedTypes)
    {
        Assert.Equal(expectedNames, parameters.Select(parameter => parameter.Name));
        Assert.Equal(expectedTypes, parameters.Select(parameter => parameter.ParameterType));
    }

    private static MethodInfo AssertExactPrivateStaticMethod(string name, Type[] parameterTypes)
    {
        var methods = typeof(AdaptiveLogicalOrchestrator)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == name)
            .ToArray();
        var method = Assert.Single(methods);
        Assert.True(method.IsPrivate);
        Assert.Equal(parameterTypes, method.GetParameters().Select(parameter => parameter.ParameterType));
        return method;
    }

    private static MethodInfo AssertExactPrivateInstanceMethod(string name, Type[] parameterTypes)
    {
        var methods = typeof(AdaptiveLogicalOrchestrator)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == name)
            .ToArray();
        var method = Assert.Single(methods);
        Assert.True(method.IsPrivate);
        Assert.Equal(parameterTypes, method.GetParameters().Select(parameter => parameter.ParameterType));
        return method;
    }

    private static object? ReadViewProperty(object view, string propertyName)
        => view.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.GetValue(view);

    private static object AssertSuccessfulOperationValue(object? operation)
    {
        Assert.NotNull(operation);
        var operationType = operation.GetType();
        Assert.True(Assert.IsType<bool>(operationType.GetProperty("IsSuccess")!.GetValue(operation)));
        return Assert.IsAssignableFrom<object>(operationType.GetProperty("Value")!.GetValue(operation));
    }

    private static string AssertFailureCode<T>(OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        return Assert.IsType<string>(result.ErrorCode);
    }

    private static TestRig BuildRig(
        IDynamicGraphTopologySource? source,
        SubPhaseRoutingDecision[] decisions,
        (string From, string To)[]? legacyTransitions = null,
        ConcurrencyProbe? probe = null,
        string? transitionSourceNodeId = null,
        string? learnedAuthorityTargetNodeId = null)
    {
        var innerTopology = new InMemoryLogicalTopologyRegistry();
        AddSubPhase(innerTopology, NodeA, "3");
        AddSubPhase(innerTopology, NodeB, "4");
        AddSubPhase(innerTopology, NodeC, "5");
        foreach (var transition in legacyTransitions ?? [])
            AddLegacyTransition(innerTopology, transition.From, transition.To);

        var topology = new CountingTopologyRegistry(innerTopology);
        var flows = new InMemoryLogicalFlowRegistry();
        var completionOrder = new ConcurrentQueue<string>();
        var contextSeen = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);
        RegisterFlow(flows, NodeA, "3", completionOrder, contextSeen, probe, transitionSourceNodeId == NodeA);
        RegisterFlow(flows, NodeB, "4", completionOrder, contextSeen, probe, transitionSourceNodeId == NodeB);
        RegisterFlow(flows, NodeC, "5", completionOrder, contextSeen, probe, transitionSourceNodeId == NodeC);
        var explicitRegistration = flows.Register(
            TestRig.ExplicitFlowId,
            TestRig.ExplicitScenarioId,
            "1.0.0",
            new RecordingFlow(
                TestRig.ExplicitFlowId,
                TestRig.ExplicitScenarioId,
                "explicit",
                completionOrder,
                contextSeen,
                probe: null));
        Assert.True(explicitRegistration.IsSuccess, explicitRegistration.ErrorMessage);

        var units = new InMemoryLogicalUnitRegistry();
        if (learnedAuthorityTargetNodeId is not null)
        {
            var authorityRegistration = units.Register(
                "authority-unit-l4",
                "edge-authority",
                DomainTag,
                "1.0.0",
                LogicalUnitTrustState.Trusted,
                isLearned: true,
                new FixedEdgeAuthorityUnit(learnedAuthorityTargetNodeId));
            Assert.True(authorityRegistration.IsSuccess, authorityRegistration.ErrorMessage);
        }

        var singleRouter = new SubPhaseRouterUnit(topology);
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            singleRouter,
            flows,
            units,
            new FixedMultiRouter(decisions),
            dynamicGraphTopologySource: source);

        return new TestRig(orchestrator, topology, completionOrder, contextSeen);
    }

    private static void AddSubPhase(InMemoryLogicalTopologyRegistry topology, string subPhaseId, string phaseId)
    {
        var added = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            subPhaseId,
            phaseId,
            $"Sub-phase {subPhaseId}",
            $"Deterministic test sub-phase {subPhaseId}.",
            [$"domain-{subPhaseId}"],
            [$"PREDICATE_{subPhaseId.Replace('.', '_')}"],
            [$"source-{subPhaseId}"]));
        Assert.True(added.IsSuccess, added.ErrorMessage);
    }

    private static void AddLegacyTransition(InMemoryLogicalTopologyRegistry topology, string from, string to)
    {
        var added = topology.AddOrUpdateTransition(new LogicalTransition(
            $"legacy-{from}-{to}",
            "sub-phase",
            from,
            "sub-phase",
            to,
            "Legacy transition used only by fallback tests.",
            ["LEGACY"],
            ["PRESERVE_CONTEXT"],
            ["legacy-test"]));
        Assert.True(added.IsSuccess, added.ErrorMessage);
    }

    private static void RegisterFlow(
        InMemoryLogicalFlowRegistry flows,
        string subPhaseId,
        string phaseId,
        ConcurrentQueue<string> completionOrder,
        ConcurrentDictionary<string, bool> contextSeen,
        ConcurrencyProbe? probe,
        bool emitInitialTransition)
    {
        var registration = flows.Register(
            $"flow-{subPhaseId}",
            $"scenario-{subPhaseId}",
            "1.0.0",
            phaseId,
            subPhaseId,
            "l4-topology-consumer-test",
            [$"domain-{subPhaseId}"],
            new RecordingFlow(
                $"flow-{subPhaseId}",
                $"scenario-{subPhaseId}",
                subPhaseId,
                completionOrder,
                contextSeen,
                probe,
                emitInitialTransition));
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
    }

    private static SubPhaseRoutingDecision Decision(string subPhaseId)
    {
        var phaseId = subPhaseId.Split('.')[0];
        return new SubPhaseRoutingDecision(
            $"decision-{subPhaseId}",
            "ready",
            phaseId,
            subPhaseId,
            0.95,
            0.0,
            [$"ROUTE_{subPhaseId.Replace('.', '_')}"],
            false,
            "fixed_l4_test_route");
    }

    private static LogicalOrchestratorRequest CreateAdaptiveRequest(string runId)
        => new(
            runId,
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement the service and verify the result with tests.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

    private static TestTopologySource SuccessSource(DynamicGraphTopology topology)
        => new(_ => OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Ok(CreateSnapshot(topology)));

    private static DynamicGraphTopologyRuntimeSnapshot CreateSnapshot(
        DynamicGraphTopology topology,
        long revision = 1)
        => new(
            Revision: revision,
            Topology: topology,
            SignalId: "signal-l4",
            SourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal,
            SourceRef: "source-l4",
            TraceRef: "trace-l4",
            BudgetProof: new TopologyBudgetProof(
                MaxNodes: 16,
                ActualNodes: topology.Nodes.Count,
                MaxEdges: 32,
                ActualEdges: topology.Edges.Count,
                MaxDepth: 16,
                ActualDepth: topology.Nodes.Count - 1,
                MaxParallelBranches: 8,
                ActualParallelBranches: 1,
                WithinBudget: true,
                ExceededDimensions: []),
            MutationEvidence: new TopologyMutationEvidence(
                SignalId: "signal-l4",
                SourceKind: DynamicGraphBuildSourceKind.MachineTopologyProposal,
                SourceRef: "source-l4",
                TraceRef: "trace-l4",
                ProposedAddedNodeIds: topology.Nodes.Select(node => node.NodeId).ToArray(),
                ProposedRemovedNodeIds: [],
                ProposedUpdatedNodeIds: [],
                ProposedAddedEdgeIds: topology.Edges.Select(edge => edge.EdgeId).ToArray(),
                ProposedRemovedEdgeIds: [],
                ProposedUpdatedEdgeIds: [],
                ProposedReorderedNodeIds: [],
                MutationApplied: true));

    private static DynamicGraphNodeOutputSnapshot CreateNodeOutputSnapshot(
        string runId,
        long topologyRevision,
        string sourceNodeId,
        IReadOnlyList<GraphNodeCandidate> candidates)
        => new(runId, topologyRevision, sourceNodeId, candidates);

    private static OperationResult<DynamicGraphNodeOutputSnapshot> NodeOutputSuccess(
        string runId,
        long topologyRevision,
        string sourceNodeId,
        GraphNodeCandidate candidate)
        => OperationResult<DynamicGraphNodeOutputSnapshot>.Ok(
            CreateNodeOutputSnapshot(runId, topologyRevision, sourceNodeId, [candidate]));

    private static GraphNodeCandidate CreateCandidate(string candidateId, string sourceNodeId)
        => new(
            candidateId,
            sourceNodeId,
            [sourceNodeId],
            $"logical-unit-{sourceNodeId}",
            [$"output-{candidateId}"],
            typeof(string).Name,
            0.9,
            0.8,
            [$"provenance-{candidateId}"],
            $"path-{candidateId}",
            "LEGAL_ACCEPTED",
            "PROJECTED",
            [],
            [$"source-{candidateId}"],
            $"trace-{candidateId}",
            [$"trace-{candidateId}"],
            [$"evidence-{candidateId}"],
            $"b1-{candidateId}",
            $"b7-{candidateId}",
            $"b11-{candidateId}",
            $"b12-{candidateId}",
            $"b6-{candidateId}",
            SemanticQualityClaimed: false);

    private static DynamicGraphTopology CreateTopology(
        string[] orderedNodeIds,
        TestEdge[] edges)
        => new(
            TenantId,
            DomainTag,
            ContextId,
            orderedNodeIds.Select(nodeId => (IDynamicGraphNode)CreateNode(nodeId)).ToArray(),
            edges,
            orderedNodeIds.Select((nodeId, index) => CreateOrder(nodeId, index)).ToArray());

    private static TestNode CreateNode(string nodeId, string descriptorDomain = DomainTag)
        => new(
            nodeId,
            new DynamicGraphNodeDescriptor(
                NodeId: nodeId,
                DomainTag: descriptorDomain,
                TInputTypeName: "logical-orchestrator-request",
                TOutputTypeName: "logical-flow-result",
                IsTrained: false,
                DeterministicFinalAuthority: false,
                RuntimeSemanticQualityClaimed: false,
                AuthorityRole: "baseline_candidate_generator",
                CheckpointRef: null,
                ManifestRef: null));

    private static TestEdge CreateEdge(
        string edgeId,
        string fromNodeId,
        string[] toNodeIds,
        bool parallelEligible,
        GraphEdgeKind kind = GraphEdgeKind.FanOut)
        => new(
            EdgeId: edgeId,
            FromNodeId: fromNodeId,
            ToNodeIds: toNodeIds,
            Kind: kind,
            ParallelEligible: parallelEligible,
            AuthorityUnitId: "authority-unit-l4",
            AuthorityUnitLevel: "L2",
            DeterministicFinalAuthority: false,
            BaselineUnitId: DeterministicRegistryEdgeCandidateProvider.Id,
            BaselineRole: "candidate_generator",
            ExcludedLeakageSignals: RequiredLeakageSignals,
            SourceProof: "source-proof-l4",
            CutoverProof: "cutover-proof-l4",
            AblationProofRef: "ablation-proof-l4");

    private static DynamicGraphNodeOrder CreateOrder(string nodeId, int position)
        => new(
            OrderId: $"order-{nodeId}",
            NodeId: nodeId,
            TenantId: TenantId,
            DomainTag: DomainTag,
            ContextId: ContextId,
            Position: position);

    private static string BuildSecurityPrompt()
        => """
           Our backend authentication service has verified password, session, JWT RS256, RBAC, ownership, and authorization controls. Verify the security and authorization model.
           """;

    private sealed record TestRig(
        AdaptiveLogicalOrchestrator Orchestrator,
        CountingTopologyRegistry Topology,
        ConcurrentQueue<string> CompletionOrder,
        ConcurrentDictionary<string, bool> ContextSeen)
    {
        public const string ExplicitFlowId = "explicit-flow-l4";
        public const string ExplicitScenarioId = "explicit-scenario-l4";
    }

    private sealed class TestTopologySource(
        Func<int, OperationResult<DynamicGraphTopologyRuntimeSnapshot>> handler)
        : IDynamicGraphTopologySource
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public OperationResult<DynamicGraphTopologyRuntimeSnapshot> ReadCurrent()
            => handler(Interlocked.Increment(ref _callCount));
    }

    private sealed class TestNodeOutputSource(
        Func<string, OperationResult<DynamicGraphNodeOutputSnapshot>> handler)
        : IDynamicGraphNodeOutputSource
    {
        private readonly ConcurrentQueue<string> _requestedRunIds = new();
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public string[] RequestedRunIds => _requestedRunIds.ToArray();

        public OperationResult<DynamicGraphNodeOutputSnapshot> ReadForInvocation(string runId)
        {
            Interlocked.Increment(ref _callCount);
            _requestedRunIds.Enqueue(runId);
            return handler(runId);
        }
    }

    private sealed class FixedMultiRouter(SubPhaseRoutingDecision[] decisions)
        : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
    {
        public Task<OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> ExecuteAsync(
            LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
            CancellationToken cancellationToken = default)
        {
            var invocationDecisions = decisions
                .Select(decision => decision with
                {
                    DocumentId = request.Input.DocumentId,
                    SourceStatus = request.Input.SourceStatus
                })
                .ToArray();
            var result = new LogicalUnitResult<SubPhaseRoutingDecision[]>(
                MultiSubPhaseRouterUnit.Id,
                MultiSubPhaseRouterUnit.Role,
                MultiSubPhaseRouterUnit.DomainTag,
                invocationDecisions,
                invocationDecisions.Min(decision => decision.Confidence),
                invocationDecisions.SelectMany(decision => decision.EvidencePredicates).Distinct(StringComparer.Ordinal).ToArray(),
                invocationDecisions.Select(decision => decision.PredictedSubPhaseId).ToArray(),
                [request.Input.DocumentId],
                []);
            return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(result));
        }
    }

    private sealed class FixedEdgeAuthorityUnit(string targetNodeId)
        : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
    {
        public Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>> ExecuteAsync(
            LogicalUnitRequest<EdgeAuthorityInput> request,
            CancellationToken cancellationToken = default)
        {
            var candidate = Assert.Single(
                request.Input.Candidates,
                value => StringComparer.Ordinal.Equals(value.TargetSubPhaseId, targetNodeId));
            var decision = new EdgeAuthorityDecision(
                OrderedTargets: [(targetNodeId, 0.95d)],
                FanOutCount: 1,
                KnowledgeEdge: false,
                AuthoritySource: SubPhaseRoutingDecision.TrainedAuthorityRole,
                AblationBaselineRank: request.Input.Candidates
                    .OrderByDescending(value => value.BaselineScore)
                    .ThenBy(value => value.TargetSubPhaseId, StringComparer.Ordinal)
                    .Select(value => value.TargetSubPhaseId)
                    .ToArray());
            var result = new LogicalUnitResult<EdgeAuthorityDecision>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                decision,
                0.95d,
                candidate.EvidencePredicates.ToArray(),
                [DomainTag],
                ["evidence-l4-edge-authority"],
                []);
            return Task.FromResult(OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Ok(result));
        }
    }

    private sealed class RecordingFlow(
        string flowId,
        string scenarioId,
        string subPhaseId,
        ConcurrentQueue<string> completionOrder,
        ConcurrentDictionary<string, bool> contextSeen,
        ConcurrencyProbe? probe,
        bool emitInitialTransition = false)
        : ILogicalFlow<string>
    {
        public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            if (probe is not null)
                await probe.EnterAsync(cancellationToken);

            contextSeen[subPhaseId] = request.Metadata.ContainsKey("context.branch_results");
            completionOrder.Enqueue(subPhaseId);
            var summary = new LogicalFlowSummary(
                flowId,
                1,
                0,
                1,
                "l4_test_complete",
                false,
                true,
                false,
                "COMPLETE",
                1.0,
                $"sub_phase={subPhaseId}",
                "L4 topology consumer test flow completed.",
                0,
                0);
            return OperationResult<LogicalFlowResult>.Ok(new LogicalFlowResult(
                flowId,
                scenarioId,
                "PASS",
                $"Branch {subPhaseId} completed.",
                summary,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["sub_phase_id"] = subPhaseId
                },
                [$"evidence-{subPhaseId}"],
                [$"FLOW_WARNING_{subPhaseId}"])
            {
                TransitionSignals = emitInitialTransition && !request.Metadata.ContainsKey("transition.depth")
                    ?
                    [
                        new PhaseTransitionSignal(
                            SemanticCompiler29.Domain.CODE_Q,
                            "QA review required after implementation completed.",
                            $"node-{subPhaseId}-transition",
                            subPhaseId,
                            [$"transition-evidence-{subPhaseId}"])
                    ]
                    : []
            });
        }
    }

    private sealed class ConcurrencyProbe(int expectedStarts)
    {
        private readonly TaskCompletionSource<bool> _allStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _started;

        public async Task EnterAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _started) == expectedStarts)
                _allStarted.TrySetResult(true);
            await _release.Task.WaitAsync(cancellationToken);
        }

        public async Task<bool> WaitForAllStartedAsync()
        {
            var completed = await Task.WhenAny(_allStarted.Task, Task.Delay(TimeSpan.FromSeconds(2)));
            return ReferenceEquals(completed, _allStarted.Task);
        }

        public void Release()
            => _release.TrySetResult(true);
    }

    private sealed class CountingTopologyRegistry(InMemoryLogicalTopologyRegistry inner)
        : ILogicalTopologyRegistry
    {
        public int ListSubPhasesCallCount { get; private set; }
        public OperationResult<IReadOnlyList<LogicalSubPhaseEntry>>? ListSubPhasesFailure { get; set; }

        public OperationResult<LogicalDomainEntry> AddOrUpdateDomain(LogicalDomain domain)
            => inner.AddOrUpdateDomain(domain);

        public OperationResult<LogicalDomainEntry> ResolveDomain(string domainId)
            => inner.ResolveDomain(domainId);

        public OperationResult<IReadOnlyList<LogicalDomainEntry>> ListDomains()
            => inner.ListDomains();

        public OperationResult<LogicalPhaseEntry> AddOrUpdatePhase(LogicalPhase phase)
            => inner.AddOrUpdatePhase(phase);

        public OperationResult<LogicalPhaseEntry> ResolvePhase(string phaseId)
            => inner.ResolvePhase(phaseId);

        public OperationResult<IReadOnlyList<LogicalPhaseEntry>> ListPhases()
            => inner.ListPhases();

        public OperationResult<LogicalSubPhaseEntry> AddOrUpdateSubPhase(LogicalSubPhase subPhase)
            => inner.AddOrUpdateSubPhase(subPhase);

        public OperationResult<LogicalSubPhaseEntry> ResolveSubPhase(string subPhaseId)
            => inner.ResolveSubPhase(subPhaseId);

        public OperationResult<IReadOnlyList<LogicalSubPhaseEntry>> ListSubPhases()
        {
            ListSubPhasesCallCount++;
            return ListSubPhasesFailure ?? inner.ListSubPhases();
        }

        public OperationResult<LogicalTransitionEntry> AddOrUpdateTransition(LogicalTransition transition)
            => inner.AddOrUpdateTransition(transition);

        public OperationResult<LogicalTransitionEntry> ResolveTransition(string transitionId)
            => inner.ResolveTransition(transitionId);

        public OperationResult<IReadOnlyList<LogicalTransitionEntry>> ListTransitions()
            => inner.ListTransitions();
    }

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

    private sealed class NullSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NOT_PERSISTED"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
    }
}
