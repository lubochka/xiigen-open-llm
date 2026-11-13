// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Runtime.CompilerServices;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

public sealed class DynamicGraphLogicalOrchestratorCompositionExecutorAdapterTests
{
    private const string RevisionConflictCode =
        "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT";
    private const string BudgetExceededCode =
        "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_EXCEEDED";

    private static readonly string[] ExactLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public async Task ParameterlessAdapter_ComposesOneOwnedRuntimeMachineAndOrchestratorIdentity()
    {
        IDynamicGraphLogicalOrchestratorCompositionExecutor executor =
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter();

        var session = AssertSuccess(executor.Compose(CreateProfile()));
        var runtimeAccessor =
            Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor>(session);
        var machineAccessor =
            Assert.IsAssignableFrom<IDynamicGraphMachineCompositionSessionAccessor>(session);
        var current = AssertSuccess(session.ReadCurrent());
        var configured =
            AssertSuccess(runtimeAccessor.Runtime.Assistant.ReadConfiguredOrchestrator());
        var firstMachineOrchestrator =
            AssertSuccess(await machineAccessor.Machine.GetOrchestratorAsync());
        var secondMachineOrchestrator =
            AssertSuccess(await machineAccessor.Machine.GetOrchestratorAsync());

        Assert.Equal(
            new DynamicGraphLogicalOrchestratorSessionDescriptor(
                "executor-profile-34-to-41",
                "executor-tenant",
                DevelopmentLifecycleTopologySeeder.DomainId,
                "executor-context"),
            session.Descriptor);
        Assert.Equal(session.Descriptor, current.Descriptor);
        Assert.Equal(1, current.Revision);
        Assert.Equal("executor-signal", current.SignalId);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, current.SourceKind);
        Assert.Equal(3, current.NodeCount);
        Assert.Equal(2, current.EdgeCount);
        Assert.True(current.InitialRevisionAdvanced);
        Assert.Same(configured, firstMachineOrchestrator);
        Assert.Same(firstMachineOrchestrator, secondMachineOrchestrator);
    }

    [Fact]
    public async Task ParameterlessAdapter_TwoCompositionsOwnDistinctStableSessionsRuntimesAndMachines()
    {
        var adapter = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter();
        var profileA = CreateProfile();
        var profileB = CreateProfile() with
        {
            ProfileId = "executor-profile-b",
            ContextId = "executor-context-b",
            SignalId = "executor-signal-b",
            TraceRef = "trace:executor-profile-b"
        };

        var sessionA = AssertSuccess(adapter.Compose(profileA));
        var runtimeA =
            Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor>(sessionA)
                .Runtime;
        var machineA =
            Assert.IsAssignableFrom<IDynamicGraphMachineCompositionSessionAccessor>(sessionA)
                .Machine;
        var orchestratorA = AssertSuccess(await machineA.GetOrchestratorAsync());
        var sessionB = AssertSuccess(adapter.Compose(profileB));
        var runtimeB =
            Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor>(sessionB)
                .Runtime;
        var machineB =
            Assert.IsAssignableFrom<IDynamicGraphMachineCompositionSessionAccessor>(sessionB)
                .Machine;
        var orchestratorB = AssertSuccess(await machineB.GetOrchestratorAsync());

        Assert.NotSame(sessionA, sessionB);
        Assert.NotSame(runtimeA, runtimeB);
        Assert.NotSame(machineA, machineB);
        Assert.NotSame(orchestratorA, orchestratorB);

        var pressure = Enumerable.Range(0, 64).Select(_ => new byte[16 * 1024]).ToArray();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.Equal("executor-profile-34-to-41", sessionA.Descriptor.ProfileId);
        Assert.Equal("executor-profile-b", sessionB.Descriptor.ProfileId);
        Assert.Same(
            runtimeA,
            Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor>(sessionA)
                .Runtime);
        Assert.Same(
            machineA,
            Assert.IsAssignableFrom<IDynamicGraphMachineCompositionSessionAccessor>(sessionA)
                .Machine);
        Assert.Same(orchestratorA, AssertSuccess(await machineA.GetOrchestratorAsync()));
        Assert.Same(orchestratorB, AssertSuccess(await machineB.GetOrchestratorAsync()));
        GC.KeepAlive(pressure);
    }

    [Fact]
    public async Task ParameterlessAdapter_OwnedMachineKeepsAllTrainingLifecycleOperationsInactive()
    {
        var session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));
        var machine =
            Assert.IsAssignableFrom<IDynamicGraphMachineCompositionSessionAccessor>(session).Machine;

        var train = await machine.TrainBatchAsync(null!);
        var evaluate = await machine.EvaluateAsync(null!);
        var export = await machine.ExportAsync();
        var import = await machine.ImportAsync(null!);

        Assert.Equal(DynamicGraphMachine.TrainingNotActivatedCode, train.ErrorCode);
        Assert.Equal(DynamicGraphMachine.EvaluationNotActivatedCode, evaluate.ErrorCode);
        Assert.Equal(DynamicGraphMachine.ExportNotActivatedCode, export.ErrorCode);
        Assert.Equal(DynamicGraphMachine.ImportNotActivatedCode, import.ErrorCode);
    }

    [Fact]
    public void LegacyCreateAndExecutor_PublishEquivalentInitialTopologyEvidence()
    {
        var profile = CreateProfile();
        var executorSession = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(profile));
        var executorCurrent = AssertSuccess(executorSession.ReadCurrent());
        var legacy = AssertSuccess(DynamicGraphLogicalOrchestratorComposition.Create(
            new CountingSessionStorage(),
            profile));
        var legacyCurrent = AssertSuccess(legacy.Runtime.TopologySource.ReadCurrent());

        Assert.Equal(legacy.ProfileId, executorCurrent.Descriptor.ProfileId);
        Assert.Equal(legacyCurrent.Topology.TenantId, executorCurrent.Descriptor.TenantId);
        Assert.Equal(legacyCurrent.Topology.DomainTag, executorCurrent.Descriptor.DomainTag);
        Assert.Equal(legacyCurrent.Topology.ContextId, executorCurrent.Descriptor.ContextId);
        Assert.Equal(legacyCurrent.Revision, executorCurrent.Revision);
        Assert.Equal(legacyCurrent.SignalId, executorCurrent.SignalId);
        Assert.Equal(legacyCurrent.SourceKind, executorCurrent.SourceKind);
        Assert.Equal(legacyCurrent.Topology.Nodes.Count, executorCurrent.NodeCount);
        Assert.Equal(legacyCurrent.Topology.Edges.Count, executorCurrent.EdgeCount);
        Assert.Equal(
            legacy.Runtime.InitialApply.RevisionAdvanced
            && legacy.Runtime.InitialApply.CurrentRevision > legacy.Runtime.InitialApply.PreviousRevision,
            executorCurrent.InitialRevisionAdvanced);
    }

    [Fact]
    public void LegacyStorageAdapterAndPublicCompositionRemainGenericForTrainedProfiles()
    {
        var profile = CreateTrainedProfile();
        var legacySession = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(
                new CountingSessionStorage()).Compose(profile));
        var publicLegacy = AssertSuccess(DynamicGraphLogicalOrchestratorComposition.Create(
            new CountingSessionStorage(),
            profile));
        var honest = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter()
            .Compose(profile);

        Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor>(
            legacySession);
        Assert.False(legacySession is IDynamicGraphMachineCompositionSessionAccessor);
        Assert.NotNull(publicLegacy.Runtime);
        AssertFailure(
            honest,
            DynamicGraphMachineCompositionFactory.TrainedStateForbiddenCode,
            "Honest-untrained dynamic graph machine composition forbids trained or cutover state.");
    }

    [Fact]
    public void Session_RetainsRuntimeAfterComposeScopeAndGcPressure()
    {
        var session = CreateRetainedSession();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var current = AssertSuccess(session.ReadCurrent());
        Assert.Equal(1, current.Revision);
        Assert.Equal(3, current.NodeCount);
        Assert.Equal(2, current.EdgeCount);
    }

    [Fact]
    public void Compose_ProfilesAndNestedArrayMutationsCannotChangeRetainedSessionEvidence()
    {
        var profile = CreateProfile();
        var session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(profile));
        var beforeDescriptor = session.Descriptor;
        var before = AssertSuccess(session.ReadCurrent());

        profile.NodeDefinitions[0] = new DynamicGraphLogicalOrchestratorNodeDefinition(
            "mutated-sub-phase",
            profile.NodeDefinitions[0].Descriptor with
            {
                NodeId = "mutated-node",
                DomainTag = "mutated-domain",
                AuthorityRole = "trained",
                CheckpointRef = "mutated-checkpoint",
                ManifestRef = "mutated-manifest"
            });
        profile.EdgeBindings[0].LogicalTransitionIds[0] = "mutated-transition";
        profile.EdgeBindings[0].ExcludedLeakageSignals[0] = "mutated-leakage";
        profile.EdgeBindings[0] = profile.EdgeBindings[0] with
        {
            AuthorityUnitId = "mutated-authority",
            SourceProof = "mutated-source-proof",
            CutoverProof = "mutated-cutover-proof",
            AblationProofRef = "mutated-ablation-proof"
        };
        profile.EdgeBindings[1] = null!;

        Assert.Equal(beforeDescriptor, session.Descriptor);
        Assert.Equal(before, AssertSuccess(session.ReadCurrent()));
    }

    [Fact]
    public void InternalStorageSeam_PerformsNoStorageOperationsDuringComposeOrRead()
    {
        var storage = new CountingSessionStorage();
        var executor = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage);

        var session = AssertSuccess(executor.Compose(CreateProfile()));
        AssertSuccess(session.ReadCurrent());

        Assert.Equal(0, storage.SaveCount);
        Assert.Equal(0, storage.LoadCount);
        Assert.Equal(0, storage.ListCount);
    }

    [Fact]
    public void ComposedSession_ExposesSeparateTopologyMutationCapability()
    {
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));

        Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyMutationSession>(session);
        Assert.False(
            typeof(IDynamicGraphLogicalOrchestratorTopologyMutationSession)
                .IsAssignableFrom(typeof(IDynamicGraphLogicalOrchestratorCompositionSession)));
    }

    [Fact]
    public async Task TopologyRunCapability_ReportsExactPinnedRevisionBeforeAndAfterAcceptedMutation()
    {
        var storage = new CountingSessionStorage();
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage).Compose(CreateProfile()));
        var topologyRun = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyRunSession>(session);
        var mutation = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyMutationSession>(session);
        var request = new DynamicGraphLogicalOrchestratorRunRequest(
            "Check the technical-design to implementation transition.");

        var first = AssertSuccess(await topologyRun.RunOnCurrentTopologyAsync(request));
        Assert.Equal(1, first.TopologyRevision);
        Assert.Equal("executor-signal", first.TopologySignalId);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, first.TopologySourceKind);
        Assert.Equal("universal-nine-node-development-5.1", first.Run.ScenarioId);
        Assert.Equal("universal-nine-node-development-flow-5.1", first.Run.SelectedFlowId);

        AssertSuccess(mutation.ApplyRegistryProjection(
            expectedRevision: 1,
            CreateProjectionRequest(include42: true, signalId: "executor-run-v2"),
            ValidBudget()));

        var second = AssertSuccess(await topologyRun.RunOnCurrentTopologyAsync(request));
        Assert.Equal(2, second.TopologyRevision);
        Assert.Equal("executor-run-v2", second.TopologySignalId);
        Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, second.TopologySourceKind);
        Assert.Equal(first.Run.ScenarioId, second.Run.ScenarioId);
        Assert.Equal(first.Run.SelectedFlowId, second.Run.SelectedFlowId);
        AssertStorageUnused(storage);
    }

    [Fact]
    public async Task TopologyRunCapability_ReturnedCollectionsAreDetachedAcrossRuns()
    {
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));
        var topologyRun = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyRunSession>(session);
        var request = new DynamicGraphLogicalOrchestratorRunRequest(
            "Check the technical-design to implementation transition.");
        var first = AssertSuccess(await topologyRun.RunOnCurrentTopologyAsync(request));
        var mutableMetrics = Assert.IsAssignableFrom<IDictionary<string, string>>(first.Run.Metrics);

        mutableMetrics["caller-injected"] = "must-not-leak";
        first.Run.EvidenceIds[0] = "caller-mutated-evidence";
        var second = AssertSuccess(await topologyRun.RunOnCurrentTopologyAsync(request));

        Assert.NotSame(first.Run.Metrics, second.Run.Metrics);
        Assert.NotSame(first.Run.EvidenceIds, second.Run.EvidenceIds);
        Assert.DoesNotContain("caller-injected", second.Run.Metrics.Keys);
        Assert.DoesNotContain("caller-mutated-evidence", second.Run.EvidenceIds);
    }

    [Fact]
    public async Task TopologyRunCapability_InvalidAndPreCancelledRequestsFailWithoutMutationOrStorage()
    {
        var storage = new CountingSessionStorage();
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage).Compose(CreateProfile()));
        var topologyRun = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyRunSession>(session);
        var before = AssertSuccess(session.ReadCurrent());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        AssertFailure(
            await topologyRun.RunOnCurrentTopologyAsync(null!),
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_RUN_REQUEST_REQUIRED",
            "A dynamic graph logical orchestrator run request is required.");
        AssertFailure(
            await topologyRun.RunOnCurrentTopologyAsync(
                new DynamicGraphLogicalOrchestratorRunRequest("  ")),
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_USER_REQUEST_REQUIRED",
            "A user request is required.");
        AssertFailure(
            await topologyRun.RunOnCurrentTopologyAsync(
                new DynamicGraphLogicalOrchestratorRunRequest(
                    "Check the technical-design to implementation transition."),
                cancellation.Token),
            "CANCELLED",
            "Cancelled.");

        Assert.Equal(before, AssertSuccess(session.ReadCurrent()));
        AssertStorageUnused(storage);
    }

    [Fact]
    public void TopologyMutation_ChangedProjectionAdvancesOneToTwoAndSameReadSessionObservesIt()
    {
        var storage = new CountingSessionStorage();
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage).Compose(CreateProfile()));
        var mutation = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyMutationSession>(session);
        var before = AssertSuccess(session.ReadCurrent());

        var applied = AssertSuccess(mutation.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: true, signalId: "executor-signal-through-42"),
            ValidBudget()));

        Assert.Equal(1, applied.PreviousRevision);
        Assert.Equal(2, applied.CurrentRevision);
        Assert.True(applied.RevisionAdvanced);
        Assert.Null(applied.BuildResult.FailureReason);
        Assert.True(applied.BuildResult.BudgetProof.WithinBudget);
        Assert.True(applied.BuildResult.MutationEvidence.MutationApplied);
        Assert.Equal(["4.2"], applied.BuildResult.MutationEvidence.ProposedAddedNodeIds);
        Assert.Equal(["executor-edge-41-to-42"], applied.BuildResult.MutationEvidence.ProposedAddedEdgeIds);
        Assert.Equal("logical-registry:executor-profile", applied.BuildResult.MutationEvidence.SourceRef);
        Assert.Equal("trace:executor-signal-through-42", applied.BuildResult.MutationEvidence.TraceRef);

        var accepted = Assert.IsType<DynamicGraphTopologyRuntimeSnapshot>(applied.Current);
        Assert.Equal(session.Descriptor.TenantId, accepted.Topology.TenantId);
        Assert.Equal(session.Descriptor.DomainTag, accepted.Topology.DomainTag);
        Assert.Equal(session.Descriptor.ContextId, accepted.Topology.ContextId);
        Assert.Equal(["3.4", "3.5", "4.1", "4.2"], accepted.Topology.Order.Select(entry => entry.NodeId));
        Assert.Equal(3, accepted.Topology.Edges.Count);
        Assert.All(accepted.Topology.Edges, AssertExpectedEdgeEvidence);

        var after = AssertSuccess(session.ReadCurrent());
        Assert.Equal(before.Descriptor, after.Descriptor);
        Assert.Equal(2, after.Revision);
        Assert.Equal("executor-signal-through-42", after.SignalId);
        Assert.Equal(4, after.NodeCount);
        Assert.Equal(3, after.EdgeCount);
        Assert.Equal(before.InitialRevisionAdvanced, after.InitialRevisionAdvanced);
        AssertStorageUnused(storage);
    }

    [Fact]
    public void TopologyMutation_StaleExpectedRevisionPreservesExactConflictAndCurrentTopology()
    {
        var storage = new CountingSessionStorage();
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage).Compose(CreateProfile()));
        var mutation = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyMutationSession>(session);
        AssertSuccess(mutation.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: true, signalId: "executor-advance-to-two"),
            ValidBudget()));
        var beforeStale = AssertSuccess(session.ReadCurrent());

        var stale = mutation.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: false, signalId: "executor-stale"),
            ValidBudget());

        AssertFailure(
            stale,
            RevisionConflictCode,
            "Expected dynamic graph topology revision does not match the current revision.");
        Assert.Equal(beforeStale, AssertSuccess(session.ReadCurrent()));
        AssertStorageUnused(storage);
    }

    [Fact]
    public void TopologyMutation_BudgetExceededIsAuditableNoOp()
    {
        var storage = new CountingSessionStorage();
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage).Compose(CreateProfile()));
        var mutation = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyMutationSession>(session);
        var before = AssertSuccess(session.ReadCurrent());

        var rejected = AssertSuccess(mutation.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: true, signalId: "executor-budget-rejected"),
            new TopologyBuildBudget(MaxNodes: 3, MaxEdges: 8, MaxDepth: 8, MaxParallelBranches: 8)));

        Assert.Equal(BudgetExceededCode, rejected.BuildResult.FailureReason);
        Assert.False(rejected.BuildResult.BudgetProof.WithinBudget);
        Assert.Equal(["nodes"], rejected.BuildResult.BudgetProof.ExceededDimensions);
        Assert.False(rejected.RevisionAdvanced);
        Assert.Equal(1, rejected.PreviousRevision);
        Assert.Equal(1, rejected.CurrentRevision);
        Assert.False(rejected.BuildResult.MutationEvidence.MutationApplied);
        Assert.Equal(before, AssertSuccess(session.ReadCurrent()));
        AssertStorageUnused(storage);
    }

    [Fact]
    public void TopologyMutation_IdenticalProjectionIsNoOp()
    {
        var storage = new CountingSessionStorage();
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage).Compose(CreateProfile()));
        var mutation = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyMutationSession>(session);
        var before = AssertSuccess(session.ReadCurrent());

        var noOp = AssertSuccess(mutation.ApplyRegistryProjection(
            1,
            CreateProjectionRequest(include42: false, signalId: "executor-identical"),
            ValidBudget()));

        Assert.Equal(1, noOp.PreviousRevision);
        Assert.Equal(1, noOp.CurrentRevision);
        Assert.False(noOp.RevisionAdvanced);
        Assert.False(noOp.BuildResult.MutationEvidence.MutationApplied);
        Assert.Empty(noOp.BuildResult.MutationEvidence.ProposedAddedNodeIds);
        Assert.Empty(noOp.BuildResult.MutationEvidence.ProposedRemovedNodeIds);
        Assert.Empty(noOp.BuildResult.MutationEvidence.ProposedUpdatedNodeIds);
        Assert.Equal(before, AssertSuccess(session.ReadCurrent()));
        AssertStorageUnused(storage);
    }

    [Fact]
    public void TopologyMutation_InvalidInputsPreserveExactDownstreamFailuresWithoutMutation()
    {
        var storage = new CountingSessionStorage();
        IDynamicGraphLogicalOrchestratorCompositionSession session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(storage).Compose(CreateProfile()));
        var mutation = Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyMutationSession>(session);
        var before = AssertSuccess(session.ReadCurrent());
        var invalidBinding = CreateProjectionRequest(include42: false, signalId: "executor-invalid-binding");
        invalidBinding.NodeBindings[0] =
            new DynamicGraphRegistryNodeBinding("missing-sub-phase", CreateNode("missing-sub-phase"));

        AssertFailure(
            mutation.ApplyRegistryProjection(1, null!, ValidBudget()),
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BOOTSTRAP_REQUEST_REQUIRED",
            "An explicit dynamic graph bootstrap projection request is required.");
        AssertFailure(
            mutation.ApplyRegistryProjection(
                1,
                CreateProjectionRequest(include42: false, signalId: "executor-null-budget"),
                null!),
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BUDGET_REQUIRED",
            "A dynamic graph topology budget is required.");
        AssertFailure(
            mutation.ApplyRegistryProjection(1, invalidBinding, ValidBudget()),
            "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SUB_PHASE_NOT_FOUND",
            "A selected logical sub-phase was not found in the registry.");
        Assert.Equal(before, AssertSuccess(session.ReadCurrent()));
        AssertStorageUnused(storage);
    }

    [Fact]
    public void CheckpointSession_CapturesMutatedProfileAndRestoresAuthenticatedRevisionFailClosed()
    {
        var adapter = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter();
        var bootstrapProfile = CreateProfile();
        var sourceSession = AssertSuccess(adapter.Compose(bootstrapProfile));
        var sourceMutation =
            Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorTopologyMutationSession>(
                sourceSession);
        var sourceCheckpoint =
            Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorCheckpointSession>(
                sourceSession);
        var projection = CreateProjectionRequest(
            include42: true,
            signalId: "executor-checkpoint-v2");
        var currentProfile = CreateProfileFromProjection(projection, ValidBudget());

        var applied = AssertSuccess(sourceMutation.ApplyRegistryProjection(
            expectedRevision: 1,
            projection,
            ValidBudget()));
        Assert.Equal(2, applied.CurrentRevision);
        AssertFailure(
            sourceCheckpoint.CaptureCheckpoint(bootstrapProfile),
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_CHECKPOINT_PROFILE_MISMATCH",
            "Dynamic graph logical orchestrator checkpoint profile does not match the retained runtime.");
        var captured = AssertSuccess(sourceCheckpoint.CaptureCheckpoint(currentProfile));
        Assert.Equal(2, captured.Revision);
        Assert.Equal(currentProfile.SignalId, captured.SignalId);
        Assert.Equal(4, captured.NodeCount);
        Assert.Equal(3, captured.EdgeCount);
        Assert.Equal(["4.2"], captured.MutationEvidence.ProposedAddedNodeIds);
        Assert.Equal(
            ["executor-edge-41-to-42"],
            captured.MutationEvidence.ProposedAddedEdgeIds);

        var authenticated = new DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint(
            captured.Revision,
            captured.SourceKind,
            captured.BudgetProof,
            captured.MutationEvidence,
            captured.InitialRevisionAdvanced);
        var restoredSession = AssertSuccess(adapter.Compose(currentProfile));
        var restoredCheckpoint =
            Assert.IsAssignableFrom<IDynamicGraphLogicalOrchestratorCheckpointSession>(
                restoredSession);
        var tampered = authenticated with
        {
            BudgetProof = authenticated.BudgetProof with
            {
                ActualNodes = authenticated.BudgetProof.ActualNodes + 1
            }
        };

        AssertFailure(
            restoredCheckpoint.RestoreCheckpoint(currentProfile, tampered),
            "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_RESTORE_STATE_INVALID",
            "Dynamic graph topology restore state is invalid.");
        Assert.Equal(1, AssertSuccess(restoredSession.ReadCurrent()).Revision);

        var restored = AssertSuccess(
            restoredCheckpoint.RestoreCheckpoint(currentProfile, authenticated));
        Assert.Equal(2, restored.Revision);
        Assert.Equal(captured.SignalId, restored.SignalId);
        Assert.Equal(
            captured.MutationEvidence.ProposedAddedNodeIds,
            restored.MutationEvidence.ProposedAddedNodeIds);
        var current = AssertSuccess(restoredSession.ReadCurrent());
        Assert.Equal(2, current.Revision);
        Assert.Equal(currentProfile.SignalId, current.SignalId);
        AssertFailure(
            restoredCheckpoint.RestoreCheckpoint(currentProfile, authenticated),
            "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_RESTORE_ALREADY_CONSUMED",
            "Dynamic graph topology restore authority was already consumed.");
        Assert.Equal(2, AssertSuccess(restoredSession.ReadCurrent()).Revision);
    }

    [Fact]
    public void PublicAdapterAndSessionSurface_ExposeNoRuntimeOwnerOrOperationalMethods()
    {
        var adapterType = typeof(DynamicGraphLogicalOrchestratorCompositionExecutorAdapter);
        var publicConstructor = Assert.Single(adapterType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Empty(publicConstructor.GetParameters());

        var adapterMethods = adapterType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();
        var composeMethod = Assert.Single(adapterMethods);
        Assert.Equal("Compose", composeMethod.Name);
        Assert.Equal(typeof(DynamicGraphLogicalOrchestratorBootstrapProfile), Assert.Single(composeMethod.GetParameters()).ParameterType);
        Assert.Equal(
            typeof(OperationResult<IDynamicGraphLogicalOrchestratorCompositionSession>),
            composeMethod.ReturnType);

        var session = AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));
        Assert.False(session.GetType().IsPublic);
        Assert.False(session.GetType().IsNestedPublic);
        Assert.False(typeof(DynamicGraphLogicalOrchestratorRuntime).IsAssignableFrom(session.GetType()));
        Assert.False(typeof(IDynamicGraphTopologyRuntimeOwner).IsAssignableFrom(session.GetType()));

        var sessionInterface = typeof(IDynamicGraphLogicalOrchestratorCompositionSession);
        Assert.Equal(["Descriptor"], sessionInterface.GetProperties().Select(property => property.Name));
        Assert.Equal(
            ["ReadCurrent"],
            sessionInterface.GetMethods().Where(method => !method.IsSpecialName).Select(method => method.Name));
        Assert.DoesNotContain(
            adapterMethods.Concat(sessionInterface.GetMethods()),
            method => ContainsForbiddenOperationName(method.Name));
        Assert.DoesNotContain(
            PublicSignatureTypes(adapterType)
                .Concat(PublicSignatureTypes(sessionInterface)),
            IsForbiddenPublicType);

        var machineAccessor = typeof(IDynamicGraphMachineCompositionSessionAccessor);
        Assert.True(machineAccessor.IsNotPublic);
        Assert.Equal(
            ["Machine"],
            machineAccessor.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name));
        Assert.DoesNotContain(
            adapterType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
            field => typeof(IDynamicGraphLogicalOrchestratorCompositionSession)
                         .IsAssignableFrom(field.FieldType)
                     || field.FieldType == typeof(DynamicGraphMachine)
                     || StringComparer.Ordinal.Equals(field.Name, "_session")
                     || StringComparer.Ordinal.Equals(field.Name, "_machine")
                     || field.Name.StartsWith("_last", StringComparison.Ordinal));
    }

    [Fact]
    public void SessionResult_IsBoundedDtoWithoutReceiptProvenanceOrMutableCollections()
    {
        Assert.Equal(
            [
                "Descriptor",
                "Revision",
                "SignalId",
                "SourceKind",
                "NodeCount",
                "EdgeCount",
                "InitialRevisionAdvanced"
            ],
            typeof(DynamicGraphLogicalOrchestratorSessionResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .OrderBy(property => property.MetadataToken)
                .Select(property => property.Name));
        Assert.DoesNotContain(
            typeof(DynamicGraphLogicalOrchestratorSessionResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance),
            property => property.PropertyType.IsArray
                        || typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType)
                           && property.PropertyType != typeof(string));
    }

    [Fact]
    public void Compose_NullAndBlankProfilesPreserveExactCarrierFailures()
    {
        var executor = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter();

        var missing = executor.Compose(null!);
        var blank = executor.Compose(CreateProfile() with { ProfileId = "  " });

        AssertFailure(
            missing,
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_REQUIRED",
            "An explicit dynamic graph logical orchestrator bootstrap profile is required.");
        AssertFailure(
            blank,
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_ID_REQUIRED",
            "A dynamic graph logical orchestrator profile id is required.");
    }

    [Fact]
    public void Compose_InvalidBudgetPreservesExactDownstreamFailure()
    {
        var profile = CreateProfile() with
        {
            Budget = new TopologyBuildBudget(0, 8, 8, 8)
        };

        var result = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(profile);

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID",
            "Dynamic graph topology build budget is invalid.");
    }

    [Fact]
    public void Compose_InvalidAuthorityPreservesExactDownstreamFailure()
    {
        var profile = CreateProfile();
        profile.EdgeBindings[0] = profile.EdgeBindings[0] with { AuthorityUnitId = " " };

        var result = new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(profile);

        AssertFailure(
            result,
            "DYNAMIC_GRAPH_EDGE_AUTHORITY_REQUIRED",
            "Dynamic graph edge 'executor-edge-34-to-35' authority unit id is required.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IDynamicGraphLogicalOrchestratorCompositionSession CreateRetainedSession()
        => AssertSuccess(
            new DynamicGraphLogicalOrchestratorCompositionExecutorAdapter().Compose(CreateProfile()));

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfile()
        => new(
            ProfileId: "executor-profile-34-to-41",
            TenantId: "executor-tenant",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: "executor-context",
            SignalId: "executor-signal",
            SourceRef: "logical-registry:executor-profile",
            TraceRef: "trace:executor-profile",
            NodeDefinitions:
            [
                CreateNodeDefinition("3.4"),
                CreateNodeDefinition("3.5"),
                CreateNodeDefinition("4.1")
            ],
            EdgeBindings:
            [
                CreateEdgeBinding(
                    "executor-edge-34-to-35",
                    "transition-sub-phase-3.4-to-3.5",
                    "executor-authority-34",
                    "34-35"),
                CreateEdgeBinding(
                    "executor-edge-35-to-41",
                    "transition-sub-phase-3.5-to-4.1",
                    "executor-authority-35",
                    "35-41")
            ],
            Budget: new TopologyBuildBudget(8, 8, 8, 8));

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateTrainedProfile()
    {
        var profile = CreateProfile();
        profile.NodeDefinitions[0] = profile.NodeDefinitions[0] with
        {
            Descriptor = profile.NodeDefinitions[0].Descriptor with
            {
                IsTrained = true,
                RuntimeSemanticQualityClaimed = true,
                AuthorityRole = "trained_node",
                CheckpointRef = "checkpoint:executor-trained",
                ManifestRef = "manifest:executor-trained"
            }
        };
        return profile;
    }

    private static DynamicGraphRegistryProjectionRequest CreateProjectionRequest(
        bool include42,
        string signalId)
    {
        var nodeIds = include42
            ? new[] { "3.4", "3.5", "4.1", "4.2" }
            : new[] { "3.4", "3.5", "4.1" };
        var edges = new List<DynamicGraphRegistryEdgeBinding>
        {
            CreateEdgeBinding(
                "executor-edge-34-to-35",
                "transition-sub-phase-3.4-to-3.5",
                "executor-authority-34",
                "34-35"),
            CreateEdgeBinding(
                "executor-edge-35-to-41",
                "transition-sub-phase-3.5-to-4.1",
                "executor-authority-35",
                "35-41")
        };
        if (include42)
        {
            edges.Add(CreateEdgeBinding(
                "executor-edge-41-to-42",
                "transition-sub-phase-4.1-to-4.2",
                "executor-authority-41",
                "41-42"));
        }

        return new DynamicGraphRegistryProjectionRequest(
            TenantId: "executor-tenant",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: "executor-context",
            SignalId: signalId,
            SourceRef: "logical-registry:executor-profile",
            TraceRef: $"trace:{signalId}",
            NodeBindings: nodeIds
                .Select(nodeId => new DynamicGraphRegistryNodeBinding(nodeId, CreateNode(nodeId)))
                .ToArray(),
            EdgeBindings: edges.ToArray());
    }

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfileFromProjection(
        DynamicGraphRegistryProjectionRequest projection,
        TopologyBuildBudget budget)
        => new(
            ProfileId: CreateProfile().ProfileId,
            projection.TenantId,
            projection.DomainTag,
            projection.ContextId,
            projection.SignalId,
            projection.SourceRef,
            projection.TraceRef,
            NodeDefinitions: projection.NodeBindings
                .Select(binding => new DynamicGraphLogicalOrchestratorNodeDefinition(
                    binding.LogicalSubPhaseId,
                    binding.Node.Descriptor with { }))
                .ToArray(),
            EdgeBindings: projection.EdgeBindings
                .Select(binding => binding with
                {
                    LogicalTransitionIds = binding.LogicalTransitionIds.ToArray(),
                    ExcludedLeakageSignals = binding.ExcludedLeakageSignals.ToArray()
                })
                .ToArray(),
            Budget: budget with { });

    private static TopologyBuildBudget ValidBudget()
        => new(MaxNodes: 8, MaxEdges: 8, MaxDepth: 8, MaxParallelBranches: 8);

    private static TestNode CreateNode(string nodeId)
        => new(nodeId, CreateNodeDefinition(nodeId).Descriptor);

    private static DynamicGraphLogicalOrchestratorNodeDefinition CreateNodeDefinition(string nodeId)
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

    private static DynamicGraphRegistryEdgeBinding CreateEdgeBinding(
        string edgeId,
        string transitionId,
        string authorityUnitId,
        string proofSuffix)
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
            SourceProof: $"source-proof:{proofSuffix}",
            CutoverProof: $"cutover-proof:{proofSuffix}",
            AblationProofRef: $"ablation-proof:{proofSuffix}");

    private static void AssertExpectedEdgeEvidence(IDynamicGraphEdge edge)
    {
        var expected = edge.EdgeId switch
        {
            "executor-edge-34-to-35" => CreateEdgeBinding(
                edge.EdgeId,
                "transition-sub-phase-3.4-to-3.5",
                "executor-authority-34",
                "34-35"),
            "executor-edge-35-to-41" => CreateEdgeBinding(
                edge.EdgeId,
                "transition-sub-phase-3.5-to-4.1",
                "executor-authority-35",
                "35-41"),
            "executor-edge-41-to-42" => CreateEdgeBinding(
                edge.EdgeId,
                "transition-sub-phase-4.1-to-4.2",
                "executor-authority-41",
                "41-42"),
            _ => throw new Xunit.Sdk.XunitException($"Unexpected edge '{edge.EdgeId}'.")
        };

        Assert.Equal(expected.Kind, edge.Kind);
        Assert.Equal(expected.ParallelEligible, edge.ParallelEligible);
        Assert.Equal(expected.AuthorityUnitId, edge.AuthorityUnitId);
        Assert.Equal(expected.AuthorityUnitLevel, edge.AuthorityUnitLevel);
        Assert.Equal(expected.DeterministicFinalAuthority, edge.DeterministicFinalAuthority);
        Assert.Equal(expected.BaselineUnitId, edge.BaselineUnitId);
        Assert.Equal(expected.BaselineRole, edge.BaselineRole);
        Assert.Equal(expected.ExcludedLeakageSignals, edge.ExcludedLeakageSignals);
        Assert.Equal(expected.SourceProof, edge.SourceProof);
        Assert.Equal(expected.CutoverProof, edge.CutoverProof);
        Assert.Equal(expected.AblationProofRef, edge.AblationProofRef);
    }

    private static void AssertStorageUnused(CountingSessionStorage storage)
    {
        Assert.Equal(0, storage.SaveCount);
        Assert.Equal(0, storage.LoadCount);
        Assert.Equal(0, storage.ListCount);
    }

    private static IEnumerable<Type> PublicSignatureTypes(Type type)
        => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .SelectMany(constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType))
            .Concat(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType)))
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(property => property.PropertyType))
            .SelectMany(FlattenType);

    private static IEnumerable<Type> FlattenType(Type type)
    {
        yield return type;
        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments().SelectMany(FlattenType))
                yield return argument;
        }
    }

    private static bool IsForbiddenPublicType(Type type)
        => type == typeof(DynamicGraphLogicalOrchestratorRuntime)
           || type == typeof(IDynamicGraphTopologyRuntimeOwner)
           || type == typeof(ISessionStorage)
           || type.FullName?.Contains("LogicalOrchestratorAssistant", StringComparison.Ordinal) == true
           || type.FullName?.Contains("ProjectWikiTrainingFlowAdapter", StringComparison.Ordinal) == true;

    private static bool ContainsForbiddenOperationName(string name)
        => new[] { "Run", "Train", "Dpo", "Apply", "Save", "Persist" }
            .Any(token => name.Contains(token, StringComparison.OrdinalIgnoreCase));

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsAssignableFrom<T>(result.Value);
    }

    private static void AssertFailure<T>(
        OperationResult<T> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    private sealed class CountingSessionStorage : ISessionStorage
    {
        public int SaveCount { get; private set; }

        public int LoadCount { get; private set; }

        public int ListCount { get; private set; }

        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        {
            SaveCount++;
            return Task.FromResult(OperationResult<bool>.Ok(true));
        }

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        {
            LoadCount++;
            return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "COUNTING_STORAGE_EMPTY"));
        }

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        {
            ListCount++;
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
        }
    }

    private sealed record TestNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;
}
