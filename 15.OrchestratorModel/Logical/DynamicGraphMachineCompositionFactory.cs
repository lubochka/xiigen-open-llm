// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ProjectWikiDomain;

internal delegate OperationResult<DynamicGraphLogicalOrchestratorCompositionCoreResult>
    DynamicGraphLogicalOrchestratorCoreCompositionFunction(
        ISessionStorage storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter);

internal sealed record DynamicGraphMachineCompositionOwnedResult(
    DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor,
    DynamicGraphLogicalOrchestratorRuntime Runtime,
    DynamicGraphMachine Machine);

// Bridges one explicit, fully untrained L4 composition into the honest-untrained L5 scaffold.
public static class DynamicGraphMachineCompositionFactory
{
    public const string CompositionResultRequiredCode =
        "DYNAMIC_GRAPH_MACHINE_COMPOSITION_RESULT_REQUIRED";
    public const string RuntimeRequiredCode =
        "DYNAMIC_GRAPH_MACHINE_COMPOSITION_RUNTIME_REQUIRED";
    public const string ProfileSnapshotFailedCode =
        "DYNAMIC_GRAPH_MACHINE_PROFILE_SNAPSHOT_FAILED";
    public const string TrainedStateForbiddenCode =
        "DYNAMIC_GRAPH_MACHINE_TRAINED_STATE_FORBIDDEN";
    public const string RuntimeSnapshotRequiredCode =
        "DYNAMIC_GRAPH_MACHINE_RUNTIME_SNAPSHOT_REQUIRED";
    public const string OrchestratorRequiredCode =
        "DYNAMIC_GRAPH_MACHINE_COMPOSED_ORCHESTRATOR_REQUIRED";
    public const string CompositionFailedCode =
        "DYNAMIC_GRAPH_MACHINE_COMPOSITION_FAILED";
    internal const string CompositionFunctionRequiredCode =
        "DYNAMIC_GRAPH_MACHINE_COMPOSITION_FUNCTION_REQUIRED";

    // Compose exactly once, admit only an honest-untrained snapshot, and retain one orchestrator identity.
    public static OperationResult<DynamicGraphMachine> CreateHonestUntrained(
        ISessionStorage storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
    {
        var owned = CreateHonestUntrainedCore(
            storage,
            profile,
            projectWikiTrainingFlowAdapter,
            DynamicGraphLogicalOrchestratorComposition.CreateCore);
        if (!owned.IsSuccess)
            return owned.MapError<DynamicGraphMachine>();

        return OperationResult<DynamicGraphMachine>.Ok(owned.Value!.Machine);
    }

    // Internal test seam keeps the public API fixed while allowing exact one-call composition evidence.
    internal static OperationResult<DynamicGraphMachineCompositionOwnedResult>
        CreateHonestUntrainedCore(
        ISessionStorage storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter,
        DynamicGraphLogicalOrchestratorCoreCompositionFunction compositionFunction)
    {
        if (compositionFunction is null)
        {
            return OperationResult<DynamicGraphMachineCompositionOwnedResult>.Fail(
                "A dynamic graph logical orchestrator composition function is required.",
                CompositionFunctionRequiredCode);
        }

        DynamicGraphLogicalOrchestratorBootstrapProfile? ownedProfile = profile;
        if (profile is not null)
        {
            var profileSnapshot = SnapshotProfile(profile);
            if (!profileSnapshot.IsSuccess)
                return profileSnapshot.MapError<DynamicGraphMachineCompositionOwnedResult>();

            ownedProfile = profileSnapshot.Value!;
        }

        OperationResult<DynamicGraphLogicalOrchestratorCompositionCoreResult>? composition;
        try
        {
            composition = compositionFunction(
                storage,
                ownedProfile!,
                projectWikiTrainingFlowAdapter);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DynamicGraphMachineCompositionOwnedResult>.Fail(
                "Dynamic graph logical orchestrator composition failed.",
                CompositionFailedCode);
        }

        if (composition is null || (composition.IsSuccess && composition.Value is null))
        {
            return OperationResult<DynamicGraphMachineCompositionOwnedResult>.Fail(
                "Dynamic graph logical orchestrator composition result is required.",
                CompositionResultRequiredCode);
        }

        // Existing composition failures own their exact error code and message.
        if (!composition.IsSuccess)
            return composition.MapError<DynamicGraphMachineCompositionOwnedResult>();

        var core = composition.Value!;
        if (core.Descriptor is null)
        {
            return OperationResult<DynamicGraphMachineCompositionOwnedResult>.Fail(
                "Dynamic graph logical orchestrator composition result is required.",
                CompositionResultRequiredCode);
        }

        var runtime = core.Runtime;
        if (runtime is null || runtime.Assistant is null || runtime.TopologySource is null)
        {
            return OperationResult<DynamicGraphMachineCompositionOwnedResult>.Fail(
                "A composed dynamic graph logical orchestrator runtime is required.",
                RuntimeRequiredCode);
        }

        if (ownedProfile is null)
            return ProfileSnapshotFailed().MapError<DynamicGraphMachineCompositionOwnedResult>();

        var profileAdmission = ValidateHonestUntrainedProfile(ownedProfile);
        if (!profileAdmission.IsSuccess)
            return profileAdmission.MapError<DynamicGraphMachineCompositionOwnedResult>();

        var runtimeAdmission = ValidateHonestUntrainedRuntime(runtime);
        if (!runtimeAdmission.IsSuccess)
            return runtimeAdmission.MapError<DynamicGraphMachineCompositionOwnedResult>();

        var orchestrator = runtime.Assistant.ReadConfiguredOrchestrator();
        if (orchestrator is null || !orchestrator.IsSuccess || orchestrator.Value is null)
        {
            return OperationResult<DynamicGraphMachineCompositionOwnedResult>.Fail(
                "The composed logical orchestrator is required.",
                OrchestratorRequiredCode);
        }

        var retainedResult = orchestrator;
        var machine = new DynamicGraphMachine(_ => Task.FromResult(retainedResult));
        return OperationResult<DynamicGraphMachineCompositionOwnedResult>.Ok(
            new DynamicGraphMachineCompositionOwnedResult(
                core.Descriptor,
                runtime,
                machine));
    }

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> SnapshotProfile(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        try
        {
            var snapshot = new DynamicGraphLogicalOrchestratorBootstrapProfile(
                ProfileId: profile.ProfileId,
                TenantId: profile.TenantId,
                DomainTag: profile.DomainTag,
                ContextId: profile.ContextId,
                SignalId: profile.SignalId,
                SourceRef: profile.SourceRef,
                TraceRef: profile.TraceRef,
                NodeDefinitions: profile.NodeDefinitions is null
                    ? null!
                    : profile.NodeDefinitions
                        .Select(definition => definition is null
                            ? null!
                            : new DynamicGraphLogicalOrchestratorNodeDefinition(
                                definition.LogicalSubPhaseId,
                                definition.Descriptor is null ? null! : definition.Descriptor with { }))
                        .ToArray(),
                EdgeBindings: profile.EdgeBindings is null
                    ? null!
                    : profile.EdgeBindings
                        .Select(binding => binding is null
                            ? null!
                            : new DynamicGraphRegistryEdgeBinding(
                                binding.EdgeId,
                                binding.LogicalTransitionIds is null
                                    ? null!
                                    : binding.LogicalTransitionIds.ToArray(),
                                binding.Kind,
                                binding.ParallelEligible,
                                binding.AuthorityUnitId,
                                binding.AuthorityUnitLevel,
                                binding.DeterministicFinalAuthority,
                                binding.BaselineUnitId,
                                binding.BaselineRole,
                                binding.ExcludedLeakageSignals is null
                                    ? null!
                                    : binding.ExcludedLeakageSignals.ToArray(),
                                binding.SourceProof,
                                binding.CutoverProof,
                                binding.AblationProofRef))
                        .ToArray(),
                Budget: profile.Budget is null ? null! : profile.Budget with { });

            return OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>.Ok(snapshot);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ProfileSnapshotFailed();
        }
    }

    private static OperationResult<bool> ValidateHonestUntrainedProfile(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
        try
        {
            if (profile.NodeDefinitions is null
                || profile.NodeDefinitions.Any(definition =>
                    definition is null
                    || !IsHonestUntrained(definition.Descriptor))
                || profile.EdgeBindings is null
                || profile.EdgeBindings.Any(binding =>
                    binding is null || binding.DeterministicFinalAuthority))
            {
                return TrainedStateForbidden();
            }

            // CutoverProof and AblationProofRef are required provenance references in the current
            // edge contract; unlike the explicit descriptor/edge flags above, they do not activate
            // trained authority and therefore are preserved for the existing composition validator.
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return TrainedStateForbidden();
        }
    }

    private static OperationResult<bool> ValidateHonestUntrainedRuntime(
        DynamicGraphLogicalOrchestratorRuntime runtime)
    {
        OperationResult<DynamicGraphTopologyRuntimeSnapshot>? current;
        try
        {
            current = runtime.TopologySource.ReadCurrent();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RuntimeSnapshotRequired();
        }

        if (current is null
            || !current.IsSuccess
            || current.Value is null
            || current.Value.Topology is null)
        {
            return RuntimeSnapshotRequired();
        }

        try
        {
            var topology = current.Value.Topology;
            if (topology.Nodes is null
                || topology.Nodes.Any(node =>
                    node is null || !IsHonestUntrained(node.Descriptor))
                || topology.Edges is null
                || topology.Edges.Any(edge =>
                    edge is null || edge.DeterministicFinalAuthority))
            {
                return TrainedStateForbidden();
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return RuntimeSnapshotRequired();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool IsHonestUntrained(DynamicGraphNodeDescriptor? descriptor)
        => descriptor is not null
           && !descriptor.IsTrained
           && !descriptor.DeterministicFinalAuthority
           && !descriptor.RuntimeSemanticQualityClaimed
           && StringComparer.Ordinal.Equals(
               descriptor.AuthorityRole,
               RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole)
           && descriptor.CheckpointRef is null
           && descriptor.ManifestRef is null;

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>
        ProfileSnapshotFailed()
        => OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>.Fail(
            "Dynamic graph machine bootstrap profile snapshot failed.",
            ProfileSnapshotFailedCode);

    private static OperationResult<bool> TrainedStateForbidden()
        => OperationResult<bool>.Fail(
            "Honest-untrained dynamic graph machine composition forbids trained or cutover state.",
            TrainedStateForbiddenCode);

    private static OperationResult<bool> RuntimeSnapshotRequired()
        => OperationResult<bool>.Fail(
            "A valid dynamic graph runtime snapshot is required.",
            RuntimeSnapshotRequiredCode);
}
