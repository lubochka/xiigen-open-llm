// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel;
using ProjectWikiDomain;

// Internal-only bridge used by the legacy composition result to retain its runtime facade.
internal interface IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor
{
    DynamicGraphLogicalOrchestratorRuntime Runtime { get; }
}

// Internal-only bridge for sessions that own the honest-untrained machine for their composition.
internal interface IDynamicGraphMachineCompositionSessionAccessor
{
    DynamicGraphMachine Machine { get; }
}

// Internal host-only durable checkpoint boundary. The public composition/run/mutation contracts
// intentionally do not expose capture or restore authority.
internal interface IDynamicGraphLogicalOrchestratorCheckpointSession
{
    OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture> CaptureCheckpoint(
        DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile);

    OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture> RestoreCheckpoint(
        DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
        DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint);
}

internal sealed record DynamicGraphLogicalOrchestratorCheckpointCapture(
    DynamicGraphLogicalOrchestratorBootstrapProfile CurrentProfile,
    DynamicGraphTopology Topology,
    long Revision,
    string SignalId,
    DynamicGraphBuildSourceKind SourceKind,
    string SourceRef,
    string TraceRef,
    int NodeCount,
    int EdgeCount,
    TopologyBudgetProof BudgetProof,
    TopologyMutationEvidence MutationEvidence,
    bool InitialRevisionAdvanced);

internal sealed record DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint(
    long Revision,
    DynamicGraphBuildSourceKind SourceKind,
    TopologyBudgetProof BudgetProof,
    TopologyMutationEvidence MutationEvidence,
    bool InitialRevisionAdvanced);

// Internal one-shot seam for invoking the retained assistant's exact selected-target boundary.
internal interface IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker
{
    Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>> InvokeAsync(
        LogicalOrchestratorAssistant assistant,
        LogicalOrchestratorRequest executionContext,
        DynamicGraphMeasurementTarget? target,
        EdgeAuthorityExecutionResult? authorityResult,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest,
        LogicalUnitTrustState minimumTrustState,
        CancellationToken cancellationToken);

    Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        InvokeAdmittedAsync(
            LogicalOrchestratorAssistant assistant,
            LogicalOrchestratorRequest executionContext,
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? association,
            LogicalUnitTrustState minimumTrustState,
            CancellationToken cancellationToken);
}

// Production invoker delegates exactly once to the assistant's existing factory-selected path.
internal sealed class DynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker
    : IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker
{
    public Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>> InvokeAsync(
        LogicalOrchestratorAssistant assistant,
        LogicalOrchestratorRequest executionContext,
        DynamicGraphMeasurementTarget? target,
        EdgeAuthorityExecutionResult? authorityResult,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest,
        LogicalUnitTrustState minimumTrustState,
        CancellationToken cancellationToken)
        => assistant.RunFactorySelectedTargetExecutionAsync(
            executionContext,
            target,
            authorityResult,
            noveltyRequest,
            minimumTrustState,
            cancellationToken);

    public Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        InvokeAdmittedAsync(
            LogicalOrchestratorAssistant assistant,
            LogicalOrchestratorRequest executionContext,
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? association,
            LogicalUnitTrustState minimumTrustState,
            CancellationToken cancellationToken)
        => assistant.RunFactoryAdmittedSelectedTargetExecutionAsync(
            executionContext,
            association,
            minimumTrustState,
            cancellationToken);
}

// Adapts the existing composition runtime to a host-neutral contracts-only public boundary.
public sealed class DynamicGraphLogicalOrchestratorCompositionExecutorAdapter
    : IDynamicGraphLogicalOrchestratorCompositionExecutor
{
    internal const string SessionSourceResultRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SESSION_SOURCE_RESULT_REQUIRED";
    internal const string RunRequestRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_RUN_REQUEST_REQUIRED";
    internal const string UserRequestRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_USER_REQUEST_REQUIRED";
    internal const string RunResultRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_RUN_RESULT_REQUIRED";
    internal const string NonLearningBoundaryViolatedCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NON_LEARNING_BOUNDARY_VIOLATED";
    internal const string SelectedTargetExecutionRequestRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SELECTED_TARGET_EXECUTION_REQUEST_REQUIRED";
    internal const string SelectedTargetExecutionRequestRequiredMessage =
        "A dynamic graph logical orchestrator selected-target execution request is required.";
    internal const string SelectedTargetExecutionResultRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SELECTED_TARGET_EXECUTION_RESULT_REQUIRED";
    internal const string SelectedTargetExecutionResultRequiredMessage =
        "A dynamic graph logical orchestrator selected-target execution result is required.";

    private readonly ISessionStorage _storage;
    private readonly ProjectWikiTrainingFlowAdapter? _projectWikiTrainingFlowAdapter;
    private readonly IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker
        _selectedTargetExecutionInvoker;
    private readonly bool _useHonestMachineComposition;

    // The host-neutral path uses the repository's real no-I/O storage singleton.
    public DynamicGraphLogicalOrchestratorCompositionExecutorAdapter()
        : this(
            NullSessionStorage.Instance,
            projectWikiTrainingFlowAdapter: null,
            selectedTargetExecutionInvoker: null,
            useHonestMachineComposition: true)
    {
    }

    // Existing in-assembly callers and tests may retain an explicit storage/adapter seam.
    internal DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null,
        IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker?
            selectedTargetExecutionInvoker = null)
        : this(
            storage,
            projectWikiTrainingFlowAdapter,
            selectedTargetExecutionInvoker,
            useHonestMachineComposition: false)
    {
    }

    private DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker?
            selectedTargetExecutionInvoker,
        bool useHonestMachineComposition)
    {
        _storage = storage;
        _projectWikiTrainingFlowAdapter = projectWikiTrainingFlowAdapter;
        _selectedTargetExecutionInvoker = selectedTargetExecutionInvoker
            ?? new DynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker();
        _useHonestMachineComposition = useHonestMachineComposition;
    }

    public OperationResult<IDynamicGraphLogicalOrchestratorCompositionSession> Compose(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        if (_useHonestMachineComposition)
        {
            var owned = DynamicGraphMachineCompositionFactory.CreateHonestUntrainedCore(
                _storage,
                profile,
                _projectWikiTrainingFlowAdapter,
                DynamicGraphLogicalOrchestratorComposition.CreateCore);
            if (owned is null || (owned.IsSuccess && owned.Value is null))
                return SessionSourceFailure<IDynamicGraphLogicalOrchestratorCompositionSession>();

            if (!owned.IsSuccess)
                return owned.MapError<IDynamicGraphLogicalOrchestratorCompositionSession>();

            var exact = owned.Value!;
            IDynamicGraphLogicalOrchestratorCompositionSession machineSession =
                new MachineCompositionSession(
                    exact.Descriptor,
                    exact.Runtime,
                    profile,
                    _selectedTargetExecutionInvoker,
                    exact.Machine);
            return OperationResult<IDynamicGraphLogicalOrchestratorCompositionSession>.Ok(
                machineSession);
        }

        var composed = DynamicGraphLogicalOrchestratorComposition.CreateCore(
            _storage,
            profile,
            _projectWikiTrainingFlowAdapter);
        if (composed is null || (composed.IsSuccess && composed.Value is null))
            return SessionSourceFailure<IDynamicGraphLogicalOrchestratorCompositionSession>();

        if (!composed.IsSuccess)
            return composed.MapError<IDynamicGraphLogicalOrchestratorCompositionSession>();

        var core = composed.Value!;
        IDynamicGraphLogicalOrchestratorCompositionSession session = new RuntimeCompositionSession(
            core.Descriptor,
            core.Runtime,
            profile,
            _selectedTargetExecutionInvoker);
        return OperationResult<IDynamicGraphLogicalOrchestratorCompositionSession>.Ok(session);
    }

    internal static OperationResult<T> SessionSourceFailure<T>()
        => OperationResult<T>.Fail(
            "Dynamic graph logical orchestrator session source result is required.",
            SessionSourceResultRequiredCode);

    private static LogicalExecutionFlags CreateNonLearningExecutionFlags()
        => new()
        {
            UseLearnedUnitWhenAvailable = false,
            UseIdleLearningScheduling = false,
            UseKnowledgeEdgeDetection = false,
            UseSkillRetriever = false,
            UseReplayFusionRanking = false,
            WriteEvidenceArtifacts = false
        };

    // Retains the runtime privately while exposing bounded read and separate bounded topology-mutation capabilities.
    private class RuntimeCompositionSession
        : IDynamicGraphLogicalOrchestratorCompositionSession,
          IDynamicGraphLogicalOrchestratorTopologyMutationSession,
          IDynamicGraphLogicalOrchestratorRunSession,
          IDynamicGraphLogicalOrchestratorTopologyRunSession,
          IDynamicGraphLogicalOrchestratorSelectedTargetExecutionSession,
          IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession,
          IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor,
          IDynamicGraphLogicalOrchestratorCheckpointSession
    {
        private readonly DynamicGraphLogicalOrchestratorSessionDescriptor _descriptor;
        private readonly DynamicGraphLogicalOrchestratorRuntime _runtime;
        private readonly object _profileGate = new();
        private readonly IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker
            _selectedTargetExecutionInvoker;
        private readonly bool _initialRevisionAdvanced;
        private DynamicGraphLogicalOrchestratorBootstrapProfile _currentProfile;

        protected internal RuntimeCompositionSession(
            DynamicGraphLogicalOrchestratorSessionDescriptor descriptor,
            DynamicGraphLogicalOrchestratorRuntime runtime,
            DynamicGraphLogicalOrchestratorBootstrapProfile profile,
            IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker
                selectedTargetExecutionInvoker)
        {
            _descriptor = descriptor with { };
            _runtime = runtime;
            _currentProfile = CopyProfile(profile);
            _selectedTargetExecutionInvoker = selectedTargetExecutionInvoker;
            var initialApply = runtime.InitialApply;
            _initialRevisionAdvanced = initialApply.RevisionAdvanced
                                       && initialApply.CurrentRevision > initialApply.PreviousRevision;
        }

        public DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor
            => _descriptor with { };

        DynamicGraphLogicalOrchestratorRuntime
            IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor.Runtime => _runtime;

        public OperationResult<DynamicGraphLogicalOrchestratorSessionResult> ReadCurrent()
        {
            var current = _runtime.TopologySource.ReadCurrent();
            if (current is null
                || (current.IsSuccess && current.Value is null)
                || (current.IsSuccess && current.Value!.Topology is null))
            {
                return SessionSourceFailure<DynamicGraphLogicalOrchestratorSessionResult>();
            }

            if (!current.IsSuccess)
                return current.MapError<DynamicGraphLogicalOrchestratorSessionResult>();

            var snapshot = current.Value!;
            return OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                new DynamicGraphLogicalOrchestratorSessionResult(
                    Descriptor: _descriptor with { },
                    Revision: snapshot.Revision,
                    SignalId: snapshot.SignalId,
                    SourceKind: snapshot.SourceKind,
                    NodeCount: snapshot.Topology.Nodes.Count,
                    EdgeCount: snapshot.Topology.Edges.Count,
                    InitialRevisionAdvanced: _initialRevisionAdvanced));
        }

        public OperationResult<DynamicGraphTopologyApplyResult> ApplyRegistryProjection(
            long expectedRevision,
            DynamicGraphRegistryProjectionRequest request,
            TopologyBuildBudget budget)
        {
            DynamicGraphLogicalOrchestratorBootstrapProfile? candidate = null;
            if (request is not null && budget is not null)
                candidate = CreateProfileFromProjection(request, budget);

            var applied = _runtime.ApplyRegistryProjection(expectedRevision, request!, budget!);
            if (applied is null)
                return SessionSourceFailure<DynamicGraphTopologyApplyResult>();
            if (applied.IsSuccess
                && applied.Value is not null
                && applied.Value.RevisionAdvanced
                && candidate is not null)
            {
                lock (_profileGate)
                    _currentProfile = candidate;
            }
            return applied;
        }

        public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            CaptureCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile)
        {
            DynamicGraphLogicalOrchestratorBootstrapProfile retained;
            lock (_profileGate)
                retained = CopyProfile(_currentProfile);
            if (!ProfilesEqual(retained, expectedCurrentProfile))
                return CheckpointProfileMismatch();

            var current = _runtime.TopologySource.ReadCurrent();
            if (current is null
                || (current.IsSuccess && current.Value is null)
                || (current.IsSuccess && current.Value!.Topology is null)
                || (current.IsSuccess && current.Value!.BudgetProof is null)
                || (current.IsSuccess && current.Value!.MutationEvidence is null))
            {
                return SessionSourceFailure<DynamicGraphLogicalOrchestratorCheckpointCapture>();
            }
            if (!current.IsSuccess)
                return current.MapError<DynamicGraphLogicalOrchestratorCheckpointCapture>();

            var snapshot = current.Value!;
            if (!SnapshotMatchesProfile(snapshot, retained))
                return CheckpointProfileMismatch();
            return OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>.Ok(
                CaptureOwned(retained, snapshot, _initialRevisionAdvanced));
        }

        public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            RestoreCheckpoint(
                DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
                DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
        {
            if (checkpoint is null)
            {
                return OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>.Fail(
                    "Dynamic graph logical orchestrator checkpoint is required.",
                    "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_CHECKPOINT_REQUIRED");
            }

            DynamicGraphLogicalOrchestratorBootstrapProfile retained;
            lock (_profileGate)
                retained = CopyProfile(_currentProfile);
            if (!ProfilesEqual(retained, expectedCurrentProfile))
                return CheckpointProfileMismatch();

            var current = _runtime.TopologySource.ReadCurrent();
            if (current is null
                || (current.IsSuccess && current.Value is null)
                || (current.IsSuccess && current.Value!.Topology is null))
            {
                return SessionSourceFailure<DynamicGraphLogicalOrchestratorCheckpointCapture>();
            }
            if (!current.IsSuccess)
                return current.MapError<DynamicGraphLogicalOrchestratorCheckpointCapture>();
            if (!SnapshotMatchesProfile(current.Value!, retained)
                || checkpoint.SourceKind
                    != DynamicGraphBuildSourceKind.LogicalRegistryProjection
                || checkpoint.MutationEvidence is null
                || checkpoint.BudgetProof is null)
            {
                return CheckpointProfileMismatch();
            }

            var restoredSnapshot = new DynamicGraphTopologyRuntimeSnapshot(
                checkpoint.Revision,
                CopyTopology(current.Value!.Topology),
                retained.SignalId,
                checkpoint.SourceKind,
                retained.SourceRef,
                retained.TraceRef,
                CopyBudgetProof(checkpoint.BudgetProof),
                CopyMutationEvidence(checkpoint.MutationEvidence));
            var restored = _runtime.RestoreCheckpoint(
                restoredSnapshot,
                checkpoint.InitialRevisionAdvanced);
            if (restored is null || (restored.IsSuccess && restored.Value is null))
                return SessionSourceFailure<DynamicGraphLogicalOrchestratorCheckpointCapture>();
            if (!restored.IsSuccess)
                return restored.MapError<DynamicGraphLogicalOrchestratorCheckpointCapture>();
            if (!SnapshotMatchesProfile(restored.Value!, retained))
                return CheckpointProfileMismatch();

            return OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>.Ok(
                CaptureOwned(
                    retained,
                    restored.Value!,
                    checkpoint.InitialRevisionAdvanced));
        }

        private DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfileFromProjection(
            DynamicGraphRegistryProjectionRequest request,
            TopologyBuildBudget budget)
            => CopyProfile(new DynamicGraphLogicalOrchestratorBootstrapProfile(
                _descriptor.ProfileId,
                request.TenantId,
                request.DomainTag,
                request.ContextId,
                request.SignalId,
                request.SourceRef,
                request.TraceRef,
                request.NodeBindings.Select(binding =>
                    new DynamicGraphLogicalOrchestratorNodeDefinition(
                        binding.LogicalSubPhaseId,
                        binding.Node.Descriptor with { })).ToArray(),
                request.EdgeBindings.Select(CopyEdgeBinding).ToArray(),
                budget with { }));

        private static DynamicGraphLogicalOrchestratorCheckpointCapture CaptureOwned(
            DynamicGraphLogicalOrchestratorBootstrapProfile profile,
            DynamicGraphTopologyRuntimeSnapshot snapshot,
            bool initialRevisionAdvanced)
            => new(
                CopyProfile(profile),
                CopyTopology(snapshot.Topology),
                snapshot.Revision,
                snapshot.SignalId,
                snapshot.SourceKind,
                snapshot.SourceRef,
                snapshot.TraceRef,
                snapshot.Topology.Nodes.Count,
                snapshot.Topology.Edges.Count,
                CopyBudgetProof(snapshot.BudgetProof),
                CopyMutationEvidence(snapshot.MutationEvidence),
                initialRevisionAdvanced);

        private static OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
            CheckpointProfileMismatch()
            => OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>.Fail(
                "Dynamic graph logical orchestrator checkpoint profile does not match the retained runtime.",
                "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_CHECKPOINT_PROFILE_MISMATCH");

        private static bool SnapshotMatchesProfile(
            DynamicGraphTopologyRuntimeSnapshot snapshot,
            DynamicGraphLogicalOrchestratorBootstrapProfile profile)
        {
            if (snapshot.SourceKind != DynamicGraphBuildSourceKind.LogicalRegistryProjection
                || !StringComparer.Ordinal.Equals(snapshot.SignalId, profile.SignalId)
                || !StringComparer.Ordinal.Equals(snapshot.SourceRef, profile.SourceRef)
                || !StringComparer.Ordinal.Equals(snapshot.TraceRef, profile.TraceRef)
                || !StringComparer.Ordinal.Equals(snapshot.Topology.TenantId, profile.TenantId)
                || !StringComparer.Ordinal.Equals(snapshot.Topology.DomainTag, profile.DomainTag)
                || !StringComparer.Ordinal.Equals(snapshot.Topology.ContextId, profile.ContextId)
                || snapshot.Topology.Nodes.Count != profile.NodeDefinitions.Length
                || snapshot.Topology.Edges.Count != profile.EdgeBindings.Length
                || snapshot.BudgetProof.MaxNodes != profile.Budget.MaxNodes
                || snapshot.BudgetProof.MaxEdges != profile.Budget.MaxEdges
                || snapshot.BudgetProof.MaxDepth != profile.Budget.MaxDepth
                || snapshot.BudgetProof.MaxParallelBranches
                    != profile.Budget.MaxParallelBranches)
            {
                return false;
            }

            var profileNodes = profile.NodeDefinitions.ToDictionary(
                definition => definition.Descriptor.NodeId,
                definition => definition.Descriptor,
                StringComparer.Ordinal);
            if (snapshot.Topology.Nodes.Any(node =>
                    !profileNodes.TryGetValue(node.NodeId, out var descriptor)
                    || descriptor != node.Descriptor))
            {
                return false;
            }

            var profileEdges = profile.EdgeBindings.ToDictionary(
                binding => binding.EdgeId,
                StringComparer.Ordinal);
            return snapshot.Topology.Edges.All(edge =>
                profileEdges.TryGetValue(edge.EdgeId, out var binding)
                && edge.Kind == binding.Kind
                && edge.ParallelEligible == binding.ParallelEligible
                && StringComparer.Ordinal.Equals(
                    edge.AuthorityUnitId,
                    binding.AuthorityUnitId)
                && StringComparer.Ordinal.Equals(
                    edge.AuthorityUnitLevel,
                    binding.AuthorityUnitLevel)
                && edge.DeterministicFinalAuthority
                    == binding.DeterministicFinalAuthority
                && StringComparer.Ordinal.Equals(
                    edge.BaselineUnitId,
                    binding.BaselineUnitId)
                && StringComparer.Ordinal.Equals(edge.BaselineRole, binding.BaselineRole)
                && edge.ExcludedLeakageSignals.SequenceEqual(
                    binding.ExcludedLeakageSignals,
                    StringComparer.Ordinal)
                && StringComparer.Ordinal.Equals(edge.SourceProof, binding.SourceProof)
                && StringComparer.Ordinal.Equals(edge.CutoverProof, binding.CutoverProof)
                && StringComparer.Ordinal.Equals(
                    edge.AblationProofRef,
                    binding.AblationProofRef));
        }

        private static bool ProfilesEqual(
            DynamicGraphLogicalOrchestratorBootstrapProfile left,
            DynamicGraphLogicalOrchestratorBootstrapProfile? right)
        {
            if (right is null
                || !StringComparer.Ordinal.Equals(left.ProfileId, right.ProfileId)
                || !StringComparer.Ordinal.Equals(left.TenantId, right.TenantId)
                || !StringComparer.Ordinal.Equals(left.DomainTag, right.DomainTag)
                || !StringComparer.Ordinal.Equals(left.ContextId, right.ContextId)
                || !StringComparer.Ordinal.Equals(left.SignalId, right.SignalId)
                || !StringComparer.Ordinal.Equals(left.SourceRef, right.SourceRef)
                || !StringComparer.Ordinal.Equals(left.TraceRef, right.TraceRef)
                || left.Budget != right.Budget
                || left.NodeDefinitions.Length != right.NodeDefinitions.Length
                || left.EdgeBindings.Length != right.EdgeBindings.Length)
            {
                return false;
            }

            return left.NodeDefinitions.Zip(right.NodeDefinitions).All(pair =>
                       StringComparer.Ordinal.Equals(
                           pair.First.LogicalSubPhaseId,
                           pair.Second.LogicalSubPhaseId)
                       && pair.First.Descriptor == pair.Second.Descriptor)
                   && left.EdgeBindings.Zip(right.EdgeBindings).All(pair =>
                       EdgeBindingsEqual(pair.First, pair.Second));
        }

        private static bool EdgeBindingsEqual(
            DynamicGraphRegistryEdgeBinding left,
            DynamicGraphRegistryEdgeBinding right)
            => left.EdgeId == right.EdgeId
               && left.Kind == right.Kind
               && left.ParallelEligible == right.ParallelEligible
               && left.AuthorityUnitId == right.AuthorityUnitId
               && left.AuthorityUnitLevel == right.AuthorityUnitLevel
               && left.DeterministicFinalAuthority == right.DeterministicFinalAuthority
               && left.BaselineUnitId == right.BaselineUnitId
               && left.BaselineRole == right.BaselineRole
               && left.SourceProof == right.SourceProof
               && left.CutoverProof == right.CutoverProof
               && left.AblationProofRef == right.AblationProofRef
               && left.LogicalTransitionIds.SequenceEqual(
                   right.LogicalTransitionIds,
                   StringComparer.Ordinal)
               && left.ExcludedLeakageSignals.SequenceEqual(
                   right.ExcludedLeakageSignals,
                   StringComparer.Ordinal);

        private static DynamicGraphLogicalOrchestratorBootstrapProfile CopyProfile(
            DynamicGraphLogicalOrchestratorBootstrapProfile profile)
            => profile with
            {
                NodeDefinitions = profile.NodeDefinitions.Select(definition =>
                    definition with { Descriptor = definition.Descriptor with { } }).ToArray(),
                EdgeBindings = profile.EdgeBindings.Select(CopyEdgeBinding).ToArray(),
                Budget = profile.Budget with { }
            };

        private static DynamicGraphRegistryEdgeBinding CopyEdgeBinding(
            DynamicGraphRegistryEdgeBinding binding)
            => binding with
            {
                LogicalTransitionIds = binding.LogicalTransitionIds.ToArray(),
                ExcludedLeakageSignals = binding.ExcludedLeakageSignals.ToArray()
            };

        private static DynamicGraphTopology CopyTopology(DynamicGraphTopology topology)
            => new(
                topology.TenantId,
                topology.DomainTag,
                topology.ContextId,
                topology.Nodes,
                topology.Edges,
                topology.Order);

        private static TopologyBudgetProof CopyBudgetProof(TopologyBudgetProof proof)
            => proof with
            {
                ExceededDimensions = Array.AsReadOnly(
                    proof.ExceededDimensions.ToArray())
            };

        private static TopologyMutationEvidence CopyMutationEvidence(
            TopologyMutationEvidence evidence)
            => evidence with
            {
                ProposedAddedNodeIds = Array.AsReadOnly(
                    evidence.ProposedAddedNodeIds.ToArray()),
                ProposedRemovedNodeIds = Array.AsReadOnly(
                    evidence.ProposedRemovedNodeIds.ToArray()),
                ProposedUpdatedNodeIds = Array.AsReadOnly(
                    evidence.ProposedUpdatedNodeIds.ToArray()),
                ProposedAddedEdgeIds = Array.AsReadOnly(
                    evidence.ProposedAddedEdgeIds.ToArray()),
                ProposedRemovedEdgeIds = Array.AsReadOnly(
                    evidence.ProposedRemovedEdgeIds.ToArray()),
                ProposedUpdatedEdgeIds = Array.AsReadOnly(
                    evidence.ProposedUpdatedEdgeIds.ToArray()),
                ProposedReorderedNodeIds = Array.AsReadOnly(
                    evidence.ProposedReorderedNodeIds.ToArray())
            };

        public async Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
            DynamicGraphLogicalOrchestratorRunRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Fail(
                    "A dynamic graph logical orchestrator run request is required.",
                    RunRequestRequiredCode);
            }

            if (string.IsNullOrWhiteSpace(request.UserRequest))
            {
                return OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Fail(
                    "A user request is required.",
                    UserRequestRequiredCode);
            }

            var run = await _runtime.Assistant.RunAsync(
                    request.UserRequest,
                    scenarioId: _runtime.Assistant.DefaultScenarioId,
                    flags: CreateNonLearningExecutionFlags(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (run is null || (run.IsSuccess && run.Value is null))
            {
                return OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Fail(
                    "A dynamic graph logical orchestrator run result is required.",
                    RunResultRequiredCode);
            }

            if (!run.IsSuccess)
                return run.MapError<DynamicGraphLogicalOrchestratorRunResult>();

            return MapOwnedRunResult(run.Value!);
        }

        public async Task<OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>
            RunOnCurrentTopologyAsync(
                DynamicGraphLogicalOrchestratorRunRequest request,
                CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Fail(
                    "A dynamic graph logical orchestrator run request is required.",
                    RunRequestRequiredCode);
            }

            if (string.IsNullOrWhiteSpace(request.UserRequest))
            {
                return OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Fail(
                    "A user request is required.",
                    UserRequestRequiredCode);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Fail(
                    "Cancelled.",
                    "CANCELLED");
            }

            var prepared = _runtime.Assistant.PrepareRunRequest(
                request.UserRequest,
                scenarioId: AdaptiveLogicalOrchestrator.ScenarioId,
                flags: CreateNonLearningExecutionFlags());
            if (prepared is null || (prepared.IsSuccess && prepared.Value is null))
            {
                return OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Fail(
                    "A dynamic graph logical orchestrator run result is required.",
                    RunResultRequiredCode);
            }

            if (!prepared.IsSuccess)
                return prepared.MapError<DynamicGraphLogicalOrchestratorTopologyRunResult>();

            var current = _runtime.TopologySource.ReadCurrent();
            if (current is null
                || (current.IsSuccess && current.Value is null)
                || (current.IsSuccess && current.Value!.Topology is null))
            {
                return SessionSourceFailure<DynamicGraphLogicalOrchestratorTopologyRunResult>();
            }

            if (!current.IsSuccess)
                return current.MapError<DynamicGraphLogicalOrchestratorTopologyRunResult>();

            var snapshot = current.Value!;
            var run = await _runtime.Assistant.RunPreparedTopologyAsync(
                    prepared.Value!,
                    snapshot,
                    cancellationToken)
                .ConfigureAwait(false);
            if (run is null || (run.IsSuccess && run.Value is null))
            {
                return OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Fail(
                    "A dynamic graph logical orchestrator run result is required.",
                    RunResultRequiredCode);
            }

            if (!run.IsSuccess)
                return run.MapError<DynamicGraphLogicalOrchestratorTopologyRunResult>();

            var mapped = MapOwnedRunResult(run.Value!);
            if (!mapped.IsSuccess)
                return mapped.MapError<DynamicGraphLogicalOrchestratorTopologyRunResult>();

            return OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Ok(
                new DynamicGraphLogicalOrchestratorTopologyRunResult(
                    snapshot.Revision,
                    snapshot.SignalId,
                    snapshot.SourceKind,
                    mapped.Value!));
        }

        private static OperationResult<DynamicGraphLogicalOrchestratorRunResult> MapOwnedRunResult(
            LogicalAssistantRunResult result)
        {
            if (result.AppliedFlags.UseLearnedUnitWhenAvailable
                || result.AppliedFlags.UseIdleLearningScheduling
                || result.AppliedFlags.UseKnowledgeEdgeDetection
                || result.AppliedFlags.UseSkillRetriever
                || result.AppliedFlags.UseReplayFusionRanking
                || result.AppliedFlags.WriteEvidenceArtifacts)
            {
                return OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Fail(
                    "The non-learning execution boundary was not preserved.",
                    NonLearningBoundaryViolatedCode);
            }

            return OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                new DynamicGraphLogicalOrchestratorRunResult(
                    result.ScenarioId,
                    result.SelectedFlowId,
                    result.OverallVerdict,
                    result.FinalHumanResult,
                    result.Metrics.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.Ordinal),
                    [.. result.EvidenceIds]));
        }

        public async Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
            RunAsync(
                DynamicGraphLogicalOrchestratorSelectedTargetExecutionRequest request,
                CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    SelectedTargetExecutionRequestRequiredMessage,
                    SelectedTargetExecutionRequestRequiredCode);
            }

            var executionContext = new LogicalOrchestratorRequest(
                request.RunId,
                request.ScenarioId,
                request.UserRequest,
                CreateNonLearningExecutionFlags(),
                new Dictionary<string, object>(StringComparer.Ordinal));
            var executionTask = _selectedTargetExecutionInvoker.InvokeAsync(
                _runtime.Assistant,
                executionContext,
                request.Target,
                request.AuthorityResult,
                request.NoveltyRequest,
                LogicalUnitTrustState.Trusted,
                cancellationToken);
            if (executionTask is null)
            {
                return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    SelectedTargetExecutionResultRequiredMessage,
                    SelectedTargetExecutionResultRequiredCode);
            }

            var execution = await executionTask.ConfigureAwait(false);
            if (execution is null || (execution.IsSuccess && execution.Value is null))
            {
                return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    SelectedTargetExecutionResultRequiredMessage,
                    SelectedTargetExecutionResultRequiredCode);
            }

            if (!execution.IsSuccess)
                return execution.MapError<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>();

            var result = execution.Value!;
            return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(
                new DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt(
                    result.Association,
                    result.ExecutionResult,
                    result.RunSpeed));
        }

        public async Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
            RunAdmittedAsync(
                DynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionRequest request,
                CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    SelectedTargetExecutionRequestRequiredMessage,
                    SelectedTargetExecutionRequestRequiredCode);
            }

            var executionContext = new LogicalOrchestratorRequest(
                request.RunId,
                request.ScenarioId,
                request.UserRequest,
                CreateNonLearningExecutionFlags(),
                new Dictionary<string, object>(StringComparer.Ordinal));
            var executionTask = _selectedTargetExecutionInvoker.InvokeAdmittedAsync(
                _runtime.Assistant,
                executionContext,
                request.Association,
                LogicalUnitTrustState.Trusted,
                cancellationToken);
            if (executionTask is null)
            {
                return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    SelectedTargetExecutionResultRequiredMessage,
                    SelectedTargetExecutionResultRequiredCode);
            }

            var execution = await executionTask.ConfigureAwait(false);
            if (execution is null || (execution.IsSuccess && execution.Value is null))
            {
                return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Fail(
                    SelectedTargetExecutionResultRequiredMessage,
                    SelectedTargetExecutionResultRequiredCode);
            }

            if (!execution.IsSuccess)
                return execution.MapError<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>();

            var result = execution.Value!;
            return OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>.Ok(
                new DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt(
                    result.Association,
                    result.ExecutionResult,
                    result.RunSpeed));
        }
    }

    private sealed class MachineCompositionSession
        : RuntimeCompositionSession,
          IDynamicGraphMachineCompositionSessionAccessor
    {
        private readonly DynamicGraphMachine _machine;

        internal MachineCompositionSession(
            DynamicGraphLogicalOrchestratorSessionDescriptor descriptor,
            DynamicGraphLogicalOrchestratorRuntime runtime,
            DynamicGraphLogicalOrchestratorBootstrapProfile profile,
            IDynamicGraphLogicalOrchestratorSelectedTargetExecutionInvoker
                selectedTargetExecutionInvoker,
            DynamicGraphMachine machine)
            : base(descriptor, runtime, profile, selectedTargetExecutionInvoker)
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
        }

        DynamicGraphMachine IDynamicGraphMachineCompositionSessionAccessor.Machine => _machine;
    }
}
