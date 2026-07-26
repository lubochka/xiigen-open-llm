// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Collections.Concurrent;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using OrchestratorModel.Logical;
using ProjectWikiDomain;

public sealed class DynamicGraphLogicalOrchestratorRuntimeTests
{
    private const string BootstrapRequestRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BOOTSTRAP_REQUEST_REQUIRED";
    private const string BudgetRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BUDGET_REQUIRED";
    private const string NotAppliedCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NOT_APPLIED";
    private const string SourceFactoryExceptionCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SOURCE_FACTORY_EXCEPTION";
    private const string SourceResultRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SOURCE_RESULT_REQUIRED";
    private const string RevisionConflictCode =
        "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT";
    private const string BudgetExceededCode =
        "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_EXCEEDED";
    private const string NodeOutputSourceRequiredCode =
        "DYNAMIC_GRAPH_NODE_OUTPUT_INVOCATION_SOURCE_REQUIRED";

    private static readonly string[] ExactLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public void CreateDefault_ExplicitSeeded34To41PathPublishesExactRevisionOneAndSameReadOnlySource()
    {
        var result = DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
            new NullSessionStorage(),
            CreateProjectionRequest(include42: false, signalId: "seed-34-to-41"),
            ValidBudget());

        var facade = AssertSuccess(result);
        Assert.Equal(0, facade.InitialApply.PreviousRevision);
        Assert.Equal(1, facade.InitialApply.CurrentRevision);
        Assert.True(facade.InitialApply.RevisionAdvanced);
        Assert.Null(facade.InitialApply.BuildResult.FailureReason);
        Assert.True(facade.InitialApply.BuildResult.MutationEvidence.MutationApplied);

        var initialCurrent = Assert.IsType<DynamicGraphTopologyRuntimeSnapshot>(facade.InitialApply.Current);
        Assert.Equal(1, initialCurrent.Revision);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, initialCurrent.SourceKind);
        Assert.Equal(["3.4", "3.5", "4.1"], initialCurrent.Topology.Order.Select(entry => entry.NodeId));
        Assert.Equal(
            ["3.4>3.5", "3.5>4.1"],
            initialCurrent.Topology.Edges.Select(edge => $"{edge.FromNodeId}>{Assert.Single(edge.ToNodeIds)}"));
        Assert.All(initialCurrent.Topology.Edges, edge =>
        {
            Assert.Equal(ExactLeakageSignals, edge.ExcludedLeakageSignals);
            Assert.StartsWith("authority-metadata-", edge.AuthorityUnitId, StringComparison.Ordinal);
            Assert.False(edge.DeterministicFinalAuthority);
        });

        var sourceCurrent = AssertSuccess(facade.TopologySource.ReadCurrent());
        Assert.Equal(Snapshot(initialCurrent), Snapshot(sourceCurrent));
        Assert.Same(facade.TopologySource, GetAdaptiveSource(facade.Assistant));
        Assert.False(facade.TopologySource is IDynamicGraphTopologyRuntimeOwner);
        Assert.Null(GetAdaptiveNodeOutputSource(facade.Assistant));
        Assert.False(GetAdaptiveNodeOutputSourceRequested(facade.Assistant));
    }

    [Fact]
    public async Task CreateDefault_NodeOutputFactoryWiresOptInOrchestratorAndReadsExactInvocationOnce()
    {
        var source = new TestNodeOutputSource(runId =>
            OperationResult<DynamicGraphNodeOutputSnapshot>.Ok(new DynamicGraphNodeOutputSnapshot(
                runId,
                topologyRevision: 1,
                sourceNodeId: "3.4",
                [CreateCandidate("candidate-runtime", "3.4")])));
        var result = DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
            new NullSessionStorage(),
            CreateSingleNodeProjectionRequest(),
            ValidBudget(),
            projectWikiTrainingFlowAdapter: null,
            dynamicGraphNodeOutputSource: source);

        var facade = AssertSuccess(result);
        Assert.Same(source, GetAdaptiveNodeOutputSource(facade.Assistant));
        Assert.True(GetAdaptiveNodeOutputSourceRequested(facade.Assistant));

        var run = await facade.Assistant.RunAsync(BuildSecurityPrompt());

        Assert.True(run.IsSuccess, $"{run.ErrorCode}: {run.ErrorMessage}");
        Assert.Equal(SecurityVerification34FlowAdapter.FlowId, run.Value!.SelectedFlowId);
        Assert.Equal(1, source.CallCount);
        var requestedRunId = Assert.Single(source.RequestedRunIds);
        Assert.False(string.IsNullOrWhiteSpace(requestedRunId));
    }

    [Fact]
    public void CreateDefault_NullNodeOutputSourceReturnsStableFailureWithoutThrowing()
    {
        var result = DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
            new NullSessionStorage(),
            CreateSingleNodeProjectionRequest(),
            ValidBudget(),
            projectWikiTrainingFlowAdapter: null,
            dynamicGraphNodeOutputSource: null!);

        AssertFailure(result, NodeOutputSourceRequiredCode);
    }

    [Fact]
    public void CreateDefault_InitialBudgetRejectionIsHardBootstrapFailureAndPublishesNoFacade()
    {
        var result = DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
            new NullSessionStorage(),
            CreateProjectionRequest(include42: false),
            new TopologyBuildBudget(MaxNodes: 2, MaxEdges: 8, MaxDepth: 8, MaxParallelBranches: 8));

        AssertFailure(result, NotAppliedCode);
    }

    [Fact]
    public void CreateAndApply_NullRequestOrBudgetFailWithStableFacadeCodes()
    {
        AssertFailure(
            DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
                new NullSessionStorage(),
                null!,
                ValidBudget()),
            BootstrapRequestRequiredCode);
        AssertFailure(
            DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
                new NullSessionStorage(),
                CreateProjectionRequest(false),
                null!),
            BudgetRequiredCode);

        var facade = CreateFacade();
        AssertFailure(
            facade.ApplyRegistryProjection(1, null!, ValidBudget()),
            BootstrapRequestRequiredCode);
        AssertFailure(
            facade.ApplyRegistryProjection(1, CreateProjectionRequest(false), null!),
            BudgetRequiredCode);
    }

    [Fact]
    public void ApplyRegistryProjection_ChangedExplicitProjectionAdvancesRevisionOneToTwo()
    {
        var facade = CreateFacade();

        var applied = AssertSuccess(facade.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: true, signalId: "seed-through-42"),
            ValidBudget()));

        Assert.Equal(1, applied.PreviousRevision);
        Assert.Equal(2, applied.CurrentRevision);
        Assert.True(applied.RevisionAdvanced);
        Assert.Equal(["3.4", "3.5", "4.1", "4.2"], applied.Current!.Topology.Order.Select(entry => entry.NodeId));
        Assert.Equal(2, AssertSuccess(facade.TopologySource.ReadCurrent()).Revision);
    }

    [Fact]
    public void ApplyRegistryProjection_StaleExpectedRevisionPropagatesConflictWithoutMutation()
    {
        var facade = CreateFacade();
        AssertSuccess(facade.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: true, signalId: "advance-to-two"),
            ValidBudget()));
        var before = AssertSuccess(facade.TopologySource.ReadCurrent());

        var stale = facade.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: false, signalId: "stale"),
            ValidBudget());

        AssertFailure(stale, RevisionConflictCode);
        Assert.Equal(Snapshot(before), Snapshot(AssertSuccess(facade.TopologySource.ReadCurrent())));
    }

    [Fact]
    public void ApplyRegistryProjection_ProjectionFailurePropagatesExactlyAndDoesNotMutateRuntime()
    {
        var facade = CreateFacade();
        var before = AssertSuccess(facade.TopologySource.ReadCurrent());
        var invalid = CreateProjectionRequest(false) with
        {
            NodeBindings =
            [
                new DynamicGraphRegistryNodeBinding("missing", CreateNode("missing")),
                .. CreateProjectionRequest(false).NodeBindings.Skip(1)
            ]
        };

        var result = facade.ApplyRegistryProjection(1, invalid, ValidBudget());

        AssertFailure(result, "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SUB_PHASE_NOT_FOUND");
        Assert.Equal(Snapshot(before), Snapshot(AssertSuccess(facade.TopologySource.ReadCurrent())));
    }

    [Fact]
    public void ApplyRegistryProjection_IdenticalTopologyIsAllowedNoOpWithoutRevisionAdvance()
    {
        var facade = CreateFacade();

        var noOp = AssertSuccess(facade.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: false, signalId: "no-op-projection"),
            ValidBudget()));

        Assert.Equal(1, noOp.PreviousRevision);
        Assert.Equal(1, noOp.CurrentRevision);
        Assert.False(noOp.RevisionAdvanced);
        Assert.False(noOp.BuildResult.MutationEvidence.MutationApplied);
        Assert.Equal(1, AssertSuccess(facade.TopologySource.ReadCurrent()).Revision);
    }

    [Fact]
    public void ApplyRegistryProjection_BudgetRejectionIsAuditableNoOpAndRetainsCurrentRevision()
    {
        var facade = CreateFacade();
        var before = AssertSuccess(facade.TopologySource.ReadCurrent());

        var rejected = AssertSuccess(facade.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: true, signalId: "budget-rejected"),
            new TopologyBuildBudget(MaxNodes: 3, MaxEdges: 8, MaxDepth: 8, MaxParallelBranches: 8)));

        Assert.Equal(BudgetExceededCode, rejected.BuildResult.FailureReason);
        Assert.False(rejected.RevisionAdvanced);
        Assert.Equal(1, rejected.CurrentRevision);
        Assert.Equal(Snapshot(before), Snapshot(AssertSuccess(facade.TopologySource.ReadCurrent())));
    }

    [Fact]
    public void AssistantCreateDefaultCore_NullFactoryPreservesLegacyTopologyPath()
    {
        var result = LogicalOrchestratorAssistant.CreateDefaultCore(
            new NullSessionStorage(),
            projectWikiTrainingFlowAdapter: null,
            dynamicGraphTopologySourceFactory: null);

        var assistant = AssertSuccess(result);
        Assert.Null(GetAdaptiveSource(assistant));
    }

    [Fact]
    public void AssistantCreateDefaultCore_FactoryRunsExactlyOnceAfterFullRegistrySeed()
    {
        var callCount = 0;
        var source = new StaticTopologySource();

        var result = LogicalOrchestratorAssistant.CreateDefaultCore(
            new NullSessionStorage(),
            projectWikiTrainingFlowAdapter: null,
            dynamicGraphTopologySourceFactory: registry =>
            {
                callCount++;
                var subPhases = AssertSuccess(registry.ListSubPhases());
                var transitions = AssertSuccess(registry.ListTransitions());
                Assert.Equal(31, subPhases.Count);
                Assert.Equal(36, transitions.Count);
                Assert.Contains(subPhases, entry => entry.SubPhase.SubPhaseId == "3.4");
                Assert.Contains(subPhases, entry => entry.SubPhase.SubPhaseId == "4.1");
                return OperationResult<IDynamicGraphTopologySource>.Ok(source);
            });

        var assistant = AssertSuccess(result);
        Assert.Equal(1, callCount);
        Assert.Same(source, GetAdaptiveSource(assistant));
    }

    [Fact]
    public void AssistantCreateDefaultCore_FactoryFailurePropagatesAndDoesNotPublishAssistant()
    {
        var callCount = 0;

        var result = LogicalOrchestratorAssistant.CreateDefaultCore(
            new NullSessionStorage(),
            null,
            _ =>
            {
                callCount++;
                return OperationResult<IDynamicGraphTopologySource>.Fail(
                    "sentinel source failure",
                    "SENTINEL_SOURCE_FAILURE");
            });

        AssertFailure(result, "SENTINEL_SOURCE_FAILURE");
        Assert.Equal("sentinel source failure", result.ErrorMessage);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void AssistantCreateDefaultCore_FactoryExceptionIsSanitizedAndDoesNotPublishAssistant()
    {
        var result = LogicalOrchestratorAssistant.CreateDefaultCore(
            new NullSessionStorage(),
            null,
            _ => throw new InvalidOperationException("factory secret detail"));

        AssertFailure(result, SourceFactoryExceptionCode);
        Assert.DoesNotContain("secret", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InvalidOperationException", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(" at ", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AssistantCreateDefaultCore_NullOperationOrSuccessfulNullSourceFailsClosed()
    {
        var nullOperation = LogicalOrchestratorAssistant.CreateDefaultCore(
            new NullSessionStorage(),
            null,
            _ => null!);
        var successfulNull = LogicalOrchestratorAssistant.CreateDefaultCore(
            new NullSessionStorage(),
            null,
            _ => OperationResult<IDynamicGraphTopologySource>.Ok(null!));

        AssertFailure(nullOperation, SourceResultRequiredCode);
        AssertFailure(successfulNull, SourceResultRequiredCode);
    }

    [Fact]
    public void PublicSurface_PinsAllFactoryAndApplyOverloadSignaturesWithoutMutationAuthority()
    {
        var type = typeof(DynamicGraphLogicalOrchestratorRuntime);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(
            ["Assistant", "InitialApply", "TopologySource"],
            properties.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(typeof(LogicalOrchestratorAssistant), properties.Single(property => property.Name == "Assistant").PropertyType);
        Assert.Equal(typeof(IDynamicGraphTopologySource), properties.Single(property => property.Name == "TopologySource").PropertyType);
        Assert.Equal(typeof(DynamicGraphTopologyApplyResult), properties.Single(property => property.Name == "InitialApply").PropertyType);

        var methods = type.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        var preloadType = typeof(IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>>);
        var legacyFactory = AssertExactPublicMethod(
            type,
            nameof(DynamicGraphLogicalOrchestratorRuntime.CreateDefault),
            isStatic: true,
            [
                typeof(ISessionStorage),
                typeof(DynamicGraphRegistryProjectionRequest),
                typeof(TopologyBuildBudget),
                typeof(ProjectWikiTrainingFlowAdapter)
            ]);
        var nodeOutputFactory = AssertExactPublicMethod(
            type,
            nameof(DynamicGraphLogicalOrchestratorRuntime.CreateDefault),
            isStatic: true,
            [
                typeof(ISessionStorage),
                typeof(DynamicGraphRegistryProjectionRequest),
                typeof(TopologyBuildBudget),
                typeof(ProjectWikiTrainingFlowAdapter),
                typeof(IDynamicGraphNodeOutputSource)
            ]);
        var nodeOutputPreloadFactory = AssertExactPublicMethod(
            type,
            nameof(DynamicGraphLogicalOrchestratorRuntime.CreateDefault),
            isStatic: true,
            [
                typeof(ISessionStorage),
                typeof(DynamicGraphRegistryProjectionRequest),
                typeof(TopologyBuildBudget),
                typeof(ProjectWikiTrainingFlowAdapter),
                typeof(IDynamicGraphNodeOutputSource),
                preloadType
            ]);
        var preloadOnlyFactory = AssertExactPublicMethod(
            type,
            nameof(DynamicGraphLogicalOrchestratorRuntime.CreateDefaultWithEdgeAuthorityPreload),
            isStatic: true,
            [
                typeof(ISessionStorage),
                typeof(DynamicGraphRegistryProjectionRequest),
                typeof(TopologyBuildBudget),
                preloadType,
                typeof(ProjectWikiTrainingFlowAdapter)
            ]);
        var apply = AssertExactPublicMethod(
            type,
            nameof(DynamicGraphLogicalOrchestratorRuntime.ApplyRegistryProjection),
            isStatic: false,
            [typeof(long), typeof(DynamicGraphRegistryProjectionRequest), typeof(TopologyBuildBudget)]);

        Assert.Equal(5, methods.Length);
        Assert.Equal(3, methods.Count(method => method.Name == nameof(DynamicGraphLogicalOrchestratorRuntime.CreateDefault)));
        Assert.Equal(
            new[] { legacyFactory, nodeOutputFactory, nodeOutputPreloadFactory, preloadOnlyFactory, apply }
                .OrderBy(MethodSignature, StringComparer.Ordinal),
            methods.OrderBy(MethodSignature, StringComparer.Ordinal));

        var runtimeResultType = typeof(OperationResult<DynamicGraphLogicalOrchestratorRuntime>);
        Assert.All(
            new[] { legacyFactory, nodeOutputFactory, nodeOutputPreloadFactory, preloadOnlyFactory },
            method => Assert.Equal(runtimeResultType, method.ReturnType));
        Assert.Equal(typeof(OperationResult<DynamicGraphTopologyApplyResult>), apply.ReturnType);
        AssertParameterContract(
            legacyFactory,
            ["storage", "projectionRequest", "budget", "projectWikiTrainingFlowAdapter"],
            [false, false, false, true]);
        Assert.Null(legacyFactory.GetParameters()[^1].DefaultValue);
        AssertParameterContract(
            nodeOutputFactory,
            ["storage", "projectionRequest", "budget", "projectWikiTrainingFlowAdapter", "dynamicGraphNodeOutputSource"],
            [false, false, false, false, false]);
        AssertParameterContract(
            nodeOutputPreloadFactory,
            [
                "storage",
                "projectionRequest",
                "budget",
                "projectWikiTrainingFlowAdapter",
                "dynamicGraphNodeOutputSource",
                "edgeAuthorityPreload"
            ],
            [false, false, false, false, false, false]);
        AssertParameterContract(
            preloadOnlyFactory,
            ["storage", "projectionRequest", "budget", "edgeAuthorityPreload", "projectWikiTrainingFlowAdapter"],
            [false, false, false, false, true]);
        Assert.Null(preloadOnlyFactory.GetParameters()[^1].DefaultValue);
        AssertParameterContract(
            apply,
            ["expectedRevision", "request", "budget"],
            [false, false, false]);

        Assert.DoesNotContain(methods, method =>
            method.Name.Contains("ApplyBuildSignal", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Resolve", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Execute", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Train", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Dpo", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Teacher", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Judge", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Persist", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Plan", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            methods.SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType)),
            exposedType => exposedType == typeof(IDynamicGraphTopologyRuntimeOwner)
                           || exposedType == typeof(ILogicalRegistryDynamicGraphProjector)
                           || exposedType == typeof(DynamicGraphBuildSignal));
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly));
    }

    private static MethodInfo AssertExactPublicMethod(
        Type declaringType,
        string name,
        bool isStatic,
        Type[] parameterTypes)
    {
        var method = Assert.Single(
            declaringType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == name
                         && candidate.IsStatic == isStatic
                         && candidate.GetParameters().Select(parameter => parameter.ParameterType)
                             .SequenceEqual(parameterTypes));
        return method;
    }

    private static void AssertParameterContract(
        MethodInfo method,
        IReadOnlyList<string> expectedNames,
        IReadOnlyList<bool> expectedOptionality)
    {
        var parameters = method.GetParameters();
        Assert.Equal(expectedNames, parameters.Select(parameter => parameter.Name));
        Assert.Equal(expectedOptionality, parameters.Select(parameter => parameter.HasDefaultValue));
    }

    private static string MethodSignature(MethodInfo method)
        => $"{method.Name}({string.Join(",", method.GetParameters().Select(parameter => parameter.ParameterType.FullName))})";

    private static DynamicGraphLogicalOrchestratorRuntime CreateFacade()
        => AssertSuccess(DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
            new NullSessionStorage(),
            CreateProjectionRequest(include42: false),
            ValidBudget()));

    private static DynamicGraphRegistryProjectionRequest CreateProjectionRequest(
        bool include42,
        string signalId = "registry-projection-initial")
    {
        var nodeIds = include42
            ? new[] { "3.4", "3.5", "4.1", "4.2" }
            : new[] { "3.4", "3.5", "4.1" };
        var edgeBindings = new List<DynamicGraphRegistryEdgeBinding>
        {
            CreateEdgeBinding(
                "edge-3.4-to-3.5",
                "transition-sub-phase-3.4-to-3.5",
                "authority-metadata-3.4"),
            CreateEdgeBinding(
                "edge-3.5-to-4.1",
                "transition-sub-phase-3.5-to-4.1",
                "authority-metadata-3.5")
        };
        if (include42)
        {
            edgeBindings.Add(CreateEdgeBinding(
                "edge-4.1-to-4.2",
                "transition-sub-phase-4.1-to-4.2",
                "authority-metadata-4.1"));
        }

        return new DynamicGraphRegistryProjectionRequest(
            TenantId: "tenant-production-bootstrap",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: "context-production-bootstrap",
            SignalId: signalId,
            SourceRef: "logical-registry:development-lifecycle",
            TraceRef: $"trace:{signalId}",
            NodeBindings: nodeIds
                .Select(nodeId => new DynamicGraphRegistryNodeBinding(nodeId, CreateNode(nodeId)))
                .ToArray(),
            EdgeBindings: edgeBindings.ToArray());
    }

    private static DynamicGraphRegistryProjectionRequest CreateSingleNodeProjectionRequest()
        => new(
            TenantId: "tenant-node-output-runtime",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: "context-node-output-runtime",
            SignalId: "single-node-output-runtime",
            SourceRef: "logical-registry:single-node-output-runtime",
            TraceRef: "trace:single-node-output-runtime",
            NodeBindings: [new DynamicGraphRegistryNodeBinding("3.4", CreateNode("3.4"))],
            EdgeBindings: []);

    private static DynamicGraphRegistryEdgeBinding CreateEdgeBinding(
        string edgeId,
        string transitionId,
        string authorityUnitId)
        => new(
            EdgeId: edgeId,
            LogicalTransitionIds: [transitionId],
            Kind: GraphEdgeKind.FanOut,
            ParallelEligible: false,
            AuthorityUnitId: authorityUnitId,
            AuthorityUnitLevel: "L2",
            DeterministicFinalAuthority: false,
            BaselineUnitId: $"baseline-{edgeId}",
            BaselineRole: "candidate_generator",
            ExcludedLeakageSignals: ExactLeakageSignals.ToArray(),
            SourceProof: $"registry-proof:{transitionId}",
            CutoverProof: $"cutover-proof:{edgeId}",
            AblationProofRef: $"ablation-proof:{edgeId}");

    private static TestNode CreateNode(string nodeId)
        => new(
            nodeId,
            new DynamicGraphNodeDescriptor(
                NodeId: nodeId,
                DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
                TInputTypeName: "System.String",
                TOutputTypeName: "System.String",
                IsTrained: false,
                DeterministicFinalAuthority: false,
                RuntimeSemanticQualityClaimed: false,
                AuthorityRole: "baseline_candidate_generator",
                CheckpointRef: null,
                ManifestRef: null));

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

    private static TopologyBuildBudget ValidBudget()
        => new(MaxNodes: 8, MaxEdges: 8, MaxDepth: 8, MaxParallelBranches: 8);

    private static IDynamicGraphTopologySource? GetAdaptiveSource(LogicalOrchestratorAssistant assistant)
    {
        var orchestrator = GetAdaptiveOrchestrator(assistant);
        var sourceField = typeof(AdaptiveLogicalOrchestrator).GetField(
            "_dynamicGraphTopologySource",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var source = sourceField!.GetValue(orchestrator);
        return source is null
            ? null
            : Assert.IsAssignableFrom<IDynamicGraphTopologySource>(source);
    }

    private static AdaptiveLogicalOrchestrator GetAdaptiveOrchestrator(LogicalOrchestratorAssistant assistant)
    {
        var orchestratorField = typeof(LogicalOrchestratorAssistant).GetField(
            "_orchestrator",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return Assert.IsType<AdaptiveLogicalOrchestrator>(orchestratorField!.GetValue(assistant));
    }

    private static IDynamicGraphNodeOutputSource? GetAdaptiveNodeOutputSource(LogicalOrchestratorAssistant assistant)
    {
        var field = typeof(AdaptiveLogicalOrchestrator).GetField(
            "_dynamicGraphNodeOutputSource",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return field!.GetValue(GetAdaptiveOrchestrator(assistant)) as IDynamicGraphNodeOutputSource;
    }

    private static bool GetAdaptiveNodeOutputSourceRequested(LogicalOrchestratorAssistant assistant)
    {
        var field = typeof(AdaptiveLogicalOrchestrator).GetField(
            "_dynamicGraphNodeOutputSourceRequested",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return Assert.IsType<bool>(field!.GetValue(GetAdaptiveOrchestrator(assistant)));
    }

    private static string BuildSecurityPrompt()
        => "Our backend authentication service has verified password, session, JWT RS256, RBAC, ownership, and authorization controls. Verify the security and authorization model.";

    private static string Snapshot(DynamicGraphTopologyRuntimeSnapshot snapshot)
        => string.Join(
            "|",
            snapshot.Revision,
            snapshot.SignalId,
            snapshot.SourceKind,
            snapshot.SourceRef,
            snapshot.TraceRef,
            string.Join(",", snapshot.Topology.Nodes.Select(node => node.NodeId)),
            string.Join(",", snapshot.Topology.Edges.Select(edge =>
                $"{edge.EdgeId}:{edge.FromNodeId}:{string.Join('+', edge.ToNodeIds)}:{edge.AuthorityUnitId}")),
            string.Join(",", snapshot.Topology.Order.Select(entry => $"{entry.NodeId}:{entry.Position}")));

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsAssignableFrom<T>(result.Value);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private sealed record TestNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

    private sealed class StaticTopologySource : IDynamicGraphTopologySource
    {
        public OperationResult<DynamicGraphTopologyRuntimeSnapshot> ReadCurrent()
            => OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Fail(
                "No topology is published by this source stub.",
                "STATIC_SOURCE_EMPTY");
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

    private sealed class NullSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
    }
}
