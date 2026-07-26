// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ProjectWikiDomain;

// Returns the caller's exact profile identity together with the constructed runtime facade.
public sealed record DynamicGraphLogicalOrchestratorCompositionResult(
    string ProfileId,
    DynamicGraphLogicalOrchestratorRuntime Runtime);

// Publishes one immutable session identity and its one-shot candidate invocation without runtime mutation authority.
public sealed record DynamicGraphCandidateInvocationCompositionResult(
    DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor,
    DynamicGraphCandidateInvocationOrchestrator Invocation);

// Internal creation evidence preserves the exact caller snapshot for both public result surfaces.
internal sealed record DynamicGraphLogicalOrchestratorCompositionCoreResult(
    DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor,
    DynamicGraphLogicalOrchestratorRuntime Runtime);

// Converts an explicit bootstrap profile into the production-adjacent logical orchestrator runtime.
public static class DynamicGraphLogicalOrchestratorComposition
{
    private const string ProfileRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_REQUIRED";
    private const string ProfileIdRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_ID_REQUIRED";
    private const string NodeDefinitionsRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITIONS_REQUIRED";
    private const string NodeDefinitionRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITION_REQUIRED";
    private const string NodeDescriptorRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DESCRIPTOR_REQUIRED";
    private const string EdgeBindingsRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_EDGE_BINDINGS_REQUIRED";
    private const string CandidateInvocationSourceNodeRequiredCode =
        "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_SOURCE_NODE_REQUIRED";
    private const string CandidateInvocationRequestProjectorRequiredCode =
        "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_REQUEST_PROJECTOR_REQUIRED";
    private const string CandidateInvocationTopologyUnavailableCode =
        "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_TOPOLOGY_UNAVAILABLE";
    private const string CandidateInvocationTopologySnapshotRequiredCode =
        "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_TOPOLOGY_SNAPSHOT_REQUIRED";
    private const string CandidateInvocationTopologyRevisionInvalidCode =
        "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_TOPOLOGY_REVISION_INVALID";
    private const string CandidateInvocationTopologyInvalidCode =
        "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_TOPOLOGY_INVALID";
    private const string CandidateInvocationCompositionSourceNodeMismatchCode =
        "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_COMPOSITION_SOURCE_NODE_MISMATCH";
    private const string CandidateInvocationCompositionResultRequiredCode =
        "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_COMPOSITION_RESULT_REQUIRED";

    // Materialize one caller-selected profile and delegate bootstrap exactly once to the runtime facade.
    public static OperationResult<DynamicGraphLogicalOrchestratorCompositionResult> Create(
        ISessionStorage storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
    {
        var composed = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(
                storage,
                projectWikiTrainingFlowAdapter)
            .Compose(profile);
        if (composed is null || (composed.IsSuccess && composed.Value is null))
        {
            return DynamicGraphLogicalOrchestratorCompositionExecutorAdapter
                .SessionSourceFailure<DynamicGraphLogicalOrchestratorCompositionResult>();
        }

        if (!composed.IsSuccess)
            return composed.MapError<DynamicGraphLogicalOrchestratorCompositionResult>();

        if (composed.Value is not IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor runtimeSession)
        {
            return DynamicGraphLogicalOrchestratorCompositionExecutorAdapter
                .SessionSourceFailure<DynamicGraphLogicalOrchestratorCompositionResult>();
        }

        return OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>.Ok(
            new DynamicGraphLogicalOrchestratorCompositionResult(
                composed.Value.Descriptor.ProfileId,
                runtimeSession.Runtime));
    }

    public static OperationResult<DynamicGraphLogicalOrchestratorCompositionResult> Create(
        ISessionStorage storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        IDynamicGraphNodeOutputSource dynamicGraphNodeOutputSource,
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
    {
        var composed = CreateCore(
            storage,
            profile,
            dynamicGraphNodeOutputSource,
            edgeAuthorityPreload,
            projectWikiTrainingFlowAdapter);
        if (composed is null || (composed.IsSuccess && composed.Value is null))
        {
            return DynamicGraphLogicalOrchestratorCompositionExecutorAdapter
                .SessionSourceFailure<DynamicGraphLogicalOrchestratorCompositionResult>();
        }

        if (!composed.IsSuccess)
            return composed.MapError<DynamicGraphLogicalOrchestratorCompositionResult>();

        return OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>.Ok(
            new DynamicGraphLogicalOrchestratorCompositionResult(
                composed.Value.Descriptor.ProfileId,
                composed.Value.Runtime));
    }

    // Compose one production axis-sweep invocation while keeping runtime and CAS ownership private.
    public static OperationResult<DynamicGraphCandidateInvocationCompositionResult>
        CreateAxisSweepInvocation<TInput, TOutput>(
            ISessionStorage storage,
            DynamicGraphLogicalOrchestratorBootstrapProfile profile,
            IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload,
            LogicalUnitDynamicGraphNode<TInput, TOutput> sourceNode,
            Func<GraphCandidateRunRequest, CombinationPlan, OperationResult<LogicalUnitRequest<TInput>>> requestProjector)
    {
        if (sourceNode is null)
        {
            return OperationResult<DynamicGraphCandidateInvocationCompositionResult>.Fail(
                "Dynamic graph candidate invocation source node is required.",
                CandidateInvocationSourceNodeRequiredCode);
        }

        if (requestProjector is null)
        {
            return OperationResult<DynamicGraphCandidateInvocationCompositionResult>.Fail(
                "Dynamic graph candidate invocation request projector is required.",
                CandidateInvocationRequestProjectorRequiredCode);
        }

        var composed = CreateCore(storage, profile, edgeAuthorityPreload);
        if (composed is null || (composed.IsSuccess && composed.Value is null))
            return CandidateInvocationCompositionResultRequired();

        if (!composed.IsSuccess)
            return composed.MapError<DynamicGraphCandidateInvocationCompositionResult>();

        if (composed.Value!.Descriptor is null || composed.Value.Runtime is null)
            return CandidateInvocationCompositionResultRequired();

        var invocation = DynamicGraphCandidateInvocationOrchestrator.CreateAxisSweep(
            composed.Value.Runtime,
            sourceNode,
            requestProjector);
        if (invocation is null || (invocation.IsSuccess && invocation.Value is null))
            return CandidateInvocationCompositionResultRequired();

        if (!invocation.IsSuccess)
            return invocation.MapError<DynamicGraphCandidateInvocationCompositionResult>();

        var topologyValidation = ValidateCandidateInvocationTopology(
            composed.Value.Runtime.TopologySource,
            sourceNode);
        if (!topologyValidation.IsSuccess)
            return topologyValidation.MapError<DynamicGraphCandidateInvocationCompositionResult>();

        return OperationResult<DynamicGraphCandidateInvocationCompositionResult>.Ok(
            new DynamicGraphCandidateInvocationCompositionResult(
                composed.Value.Descriptor with { },
                invocation.Value!));
    }

    // The single runtime-building algorithm is shared by the host-neutral and legacy result surfaces.
    internal static OperationResult<DynamicGraphLogicalOrchestratorCompositionCoreResult> CreateCore(
        ISessionStorage storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
        => CreateCoreImplementation(
            profile,
            (projectionRequest, budget) => DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
                storage,
                projectionRequest,
                budget,
                projectWikiTrainingFlowAdapter));

    internal static OperationResult<DynamicGraphLogicalOrchestratorCompositionCoreResult> CreateCore(
        ISessionStorage storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        IDynamicGraphNodeOutputSource dynamicGraphNodeOutputSource,
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
        => CreateCoreImplementation(
            profile,
            (projectionRequest, budget) => DynamicGraphLogicalOrchestratorRuntime.CreateDefault(
                storage,
                projectionRequest,
                budget,
                projectWikiTrainingFlowAdapter,
                dynamicGraphNodeOutputSource,
                edgeAuthorityPreload));

    // Build a preload-backed runtime whose node outputs can only be supplied by a later invocation owner.
    internal static OperationResult<DynamicGraphLogicalOrchestratorCompositionCoreResult> CreateCore(
        ISessionStorage storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        IReadOnlyList<ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>> edgeAuthorityPreload,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
        => CreateCoreImplementation(
            profile,
            (projectionRequest, budget) =>
                DynamicGraphLogicalOrchestratorRuntime.CreateDefaultWithEdgeAuthorityPreload(
                    storage,
                    projectionRequest,
                    budget,
                    edgeAuthorityPreload,
                    projectWikiTrainingFlowAdapter));

    private static OperationResult<DynamicGraphLogicalOrchestratorCompositionCoreResult> CreateCoreImplementation(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        Func<DynamicGraphRegistryProjectionRequest, TopologyBuildBudget,
            OperationResult<DynamicGraphLogicalOrchestratorRuntime>> runtimeFactory)
    {
        var validation = ValidateProfile(profile);
        if (!validation.IsSuccess)
            return validation.MapError<DynamicGraphLogicalOrchestratorCompositionCoreResult>();

        var snapshot = SnapshotProfile(profile);
        var projectionRequest = new DynamicGraphRegistryProjectionRequest(
            TenantId: snapshot.TenantId,
            DomainTag: snapshot.DomainTag,
            ContextId: snapshot.ContextId,
            SignalId: snapshot.SignalId,
            SourceRef: snapshot.SourceRef,
            TraceRef: snapshot.TraceRef,
            NodeBindings: snapshot.NodeDefinitions
                .Select(definition => new DynamicGraphRegistryNodeBinding(
                    definition.LogicalSubPhaseId,
                    new DescriptorOnlyNode(definition.Descriptor)))
                .ToArray(),
            EdgeBindings: snapshot.EdgeBindings);

        OperationResult<DynamicGraphLogicalOrchestratorRuntime>? runtime;
        try
        {
            runtime = runtimeFactory(projectionRequest, snapshot.Budget);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return DynamicGraphLogicalOrchestratorCompositionExecutorAdapter
                .SessionSourceFailure<DynamicGraphLogicalOrchestratorCompositionCoreResult>();
        }

        if (runtime is null || (runtime.IsSuccess && runtime.Value is null))
        {
            return DynamicGraphLogicalOrchestratorCompositionExecutorAdapter
                .SessionSourceFailure<DynamicGraphLogicalOrchestratorCompositionCoreResult>();
        }

        if (!runtime.IsSuccess)
            return runtime.MapError<DynamicGraphLogicalOrchestratorCompositionCoreResult>();

        return OperationResult<DynamicGraphLogicalOrchestratorCompositionCoreResult>.Ok(
            new DynamicGraphLogicalOrchestratorCompositionCoreResult(
                new DynamicGraphLogicalOrchestratorSessionDescriptor(
                    ProfileId: snapshot.ProfileId,
                    TenantId: snapshot.TenantId,
                    DomainTag: snapshot.DomainTag,
                    ContextId: snapshot.ContextId),
                runtime.Value!));
    }

    private static OperationResult<bool> ValidateProfile(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        if (profile is null)
        {
            return OperationResult<bool>.Fail(
                "An explicit dynamic graph logical orchestrator bootstrap profile is required.",
                ProfileRequiredCode);
        }

        if (string.IsNullOrWhiteSpace(profile.ProfileId))
        {
            return OperationResult<bool>.Fail(
                "A dynamic graph logical orchestrator profile id is required.",
                ProfileIdRequiredCode);
        }

        if (profile.NodeDefinitions is not { Length: > 0 })
        {
            return OperationResult<bool>.Fail(
                "At least one dynamic graph logical orchestrator node definition is required.",
                NodeDefinitionsRequiredCode);
        }

        foreach (var definition in profile.NodeDefinitions)
        {
            if (definition is null)
            {
                return OperationResult<bool>.Fail(
                    "A dynamic graph logical orchestrator node definition is required.",
                    NodeDefinitionRequiredCode);
            }

            if (definition.Descriptor is null)
            {
                return OperationResult<bool>.Fail(
                    "A dynamic graph logical orchestrator node descriptor is required.",
                    NodeDescriptorRequiredCode);
            }
        }

        if (profile.EdgeBindings is null)
        {
            return OperationResult<bool>.Fail(
                "Dynamic graph logical orchestrator edge bindings are required.",
                EdgeBindingsRequiredCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateCandidateInvocationTopology(
        IDynamicGraphTopologySource topologySource,
        IDynamicGraphNode sourceNode)
    {
        OperationResult<DynamicGraphTopologyRuntimeSnapshot>? topologyRead;
        try
        {
            topologyRead = topologySource?.ReadCurrent();
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return CandidateInvocationTopologyUnavailable();
        }

        if (topologyRead is null || !topologyRead.IsSuccess)
            return CandidateInvocationTopologyUnavailable();

        var topologySnapshot = topologyRead.Value;
        if (topologySnapshot is null || topologySnapshot.Topology is null)
        {
            return OperationResult<bool>.Fail(
                "Dynamic graph topology snapshot is required for candidate invocation composition.",
                CandidateInvocationTopologySnapshotRequiredCode);
        }

        if (topologySnapshot.Revision <= 0)
        {
            return OperationResult<bool>.Fail(
                "Dynamic graph topology revision must be positive for candidate invocation composition.",
                CandidateInvocationTopologyRevisionInvalidCode);
        }

        OperationResult<DynamicGraphTopology>? topologyValidation;
        try
        {
            topologyValidation = DynamicGraphValidation.Validate(topologySnapshot.Topology);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return CandidateInvocationTopologyInvalid();
        }

        if (topologyValidation is null
            || !topologyValidation.IsSuccess
            || topologyValidation.Value is null)
        {
            return CandidateInvocationTopologyInvalid();
        }

        try
        {
            var sourceNodeId = sourceNode.NodeId;
            var sourceDescriptor = sourceNode.Descriptor;
            var matchingNodes = topologySnapshot.Topology.Nodes
                .Where(node => node is not null
                    && StringComparer.Ordinal.Equals(node.NodeId, sourceNodeId))
                .ToArray();
            if (matchingNodes.Length != 1 || sourceDescriptor is null)
                return CandidateInvocationSourceNodeMismatch();

            var topologyNode = matchingNodes[0];
            var topologyNodeId = topologyNode.NodeId;
            var topologyDescriptor = topologyNode.Descriptor;
            if (topologyDescriptor is null
                || !CandidateInvocationSourceNodeEquals(
                    sourceNodeId,
                    sourceDescriptor,
                    topologyNodeId,
                    topologyDescriptor))
            {
                return CandidateInvocationSourceNodeMismatch();
            }
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return CandidateInvocationSourceNodeMismatch();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool CandidateInvocationSourceNodeEquals(
        string sourceNodeId,
        DynamicGraphNodeDescriptor sourceDescriptor,
        string topologyNodeId,
        DynamicGraphNodeDescriptor topologyDescriptor)
        => StringComparer.Ordinal.Equals(sourceNodeId, topologyNodeId)
           && sourceDescriptor == topologyDescriptor;

    private static OperationResult<bool> CandidateInvocationTopologyUnavailable()
        => OperationResult<bool>.Fail(
            "Dynamic graph topology is unavailable for candidate invocation composition.",
            CandidateInvocationTopologyUnavailableCode);

    private static OperationResult<bool> CandidateInvocationTopologyInvalid()
        => OperationResult<bool>.Fail(
            "Dynamic graph topology is invalid for candidate invocation composition.",
            CandidateInvocationTopologyInvalidCode);

    private static OperationResult<bool> CandidateInvocationSourceNodeMismatch()
        => OperationResult<bool>.Fail(
            "Candidate source node does not occur exactly once in the composed topology.",
            CandidateInvocationCompositionSourceNodeMismatchCode);

    private static OperationResult<DynamicGraphCandidateInvocationCompositionResult>
        CandidateInvocationCompositionResultRequired()
        => OperationResult<DynamicGraphCandidateInvocationCompositionResult>.Fail(
            "Dynamic graph candidate invocation composition result is required.",
            CandidateInvocationCompositionResultRequiredCode);

    private static DynamicGraphLogicalOrchestratorBootstrapProfile SnapshotProfile(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
        => new(
            ProfileId: profile.ProfileId,
            TenantId: profile.TenantId,
            DomainTag: profile.DomainTag,
            ContextId: profile.ContextId,
            SignalId: profile.SignalId,
            SourceRef: profile.SourceRef,
            TraceRef: profile.TraceRef,
            NodeDefinitions: profile.NodeDefinitions
                .Select(definition => new DynamicGraphLogicalOrchestratorNodeDefinition(
                    definition.LogicalSubPhaseId,
                    definition.Descriptor with { }))
                .ToArray(),
            EdgeBindings: profile.EdgeBindings
                .Select(SnapshotEdgeBinding)
                .ToArray(),
            Budget: profile.Budget is null ? null! : profile.Budget with { });

    private static DynamicGraphRegistryEdgeBinding SnapshotEdgeBinding(
        DynamicGraphRegistryEdgeBinding binding)
        => binding is null
            ? null!
            : new DynamicGraphRegistryEdgeBinding(
                EdgeId: binding.EdgeId,
                LogicalTransitionIds: binding.LogicalTransitionIds?.ToArray()!,
                Kind: binding.Kind,
                ParallelEligible: binding.ParallelEligible,
                AuthorityUnitId: binding.AuthorityUnitId,
                AuthorityUnitLevel: binding.AuthorityUnitLevel,
                DeterministicFinalAuthority: binding.DeterministicFinalAuthority,
                BaselineUnitId: binding.BaselineUnitId,
                BaselineRole: binding.BaselineRole,
                ExcludedLeakageSignals: binding.ExcludedLeakageSignals?.ToArray()!,
                SourceProof: binding.SourceProof,
                CutoverProof: binding.CutoverProof,
                AblationProofRef: binding.AblationProofRef);

    private static bool IsNonFatalException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    // Descriptor-only bootstrap nodes deliberately expose no executable or trainable behavior.
    private sealed class DescriptorOnlyNode(
        DynamicGraphNodeDescriptor descriptor) : IDynamicGraphNode
    {
        private readonly DynamicGraphNodeDescriptor _descriptor = descriptor with { };

        public string NodeId => _descriptor.NodeId;

        public DynamicGraphNodeDescriptor Descriptor => _descriptor with { };
    }
}
