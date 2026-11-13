// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;

// This object keeps one cumulative teaching session open across a whole bundle.
public sealed class TeachingSession
{
    public TeachingSession(
        LogicalOrchestratorAssistant assistant,
        InMemoryLogicalTopologyRegistry topology,
        InMemoryLogicalFlowRegistry flows,
        InMemoryLogicalUnitRegistry units,
        string bundleId,
        string checkpointRoot,
        string snapshotPath)
    {
        Assistant = assistant;
        Topology = topology;
        Flows = flows;
        Units = units;
        BundleId = bundleId;
        CheckpointRoot = checkpointRoot;
        SnapshotPath = snapshotPath;
    }

    public LogicalOrchestratorAssistant Assistant { get; }
    public InMemoryLogicalTopologyRegistry Topology { get; }
    public InMemoryLogicalFlowRegistry Flows { get; }
    public InMemoryLogicalUnitRegistry Units { get; }
    public string BundleId { get; }
    public string CheckpointRoot { get; }
    public string SnapshotPath { get; }

    // Save the current live registries after one row completes.
    public OperationResult<string> SaveSnapshot(int rowIndex)
    {
        var snapshot = TeachingRegistryPersistence.Capture(BundleId, rowIndex, Units, Flows, CheckpointRoot);
        if (!snapshot.IsSuccess)
            return OperationResult<string>.Fail(snapshot.ErrorMessage, snapshot.ErrorCode);

        return TeachingRegistryPersistence.Save(snapshot.Value!, SnapshotPath);
    }
}

// This builder creates teaching sessions that load prior learned registry state before the next row.
public static class TeachingSessionBuilder
{
    public static OperationResult<TeachingSession> Open(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath)
        => OpenCore(
            storage,
            bundleId,
            checkpointRoot,
            snapshotPath,
            enableCapabilityTraining: true,
            preloadedSnapshot: null,
            heldArtifacts: null,
            admissionAuthority: null,
            replayVerificationCapability: null);

    public static OperationResult<TeachingSession> Open(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath,
        ILogicalCapabilityTrainingAdmissionAuthority admissionAuthority)
        => admissionAuthority is null
            ? OperationResult<TeachingSession>.Fail(
                "Logical capability training admission authority is required.",
                DefaultLogicalCapabilityTrainer.AdmissionAuthorityRequiredErrorCode)
            : OpenCore(
                storage,
                bundleId,
                checkpointRoot,
                snapshotPath,
                enableCapabilityTraining: true,
                preloadedSnapshot: null,
                heldArtifacts: null,
                admissionAuthority: admissionAuthority,
                replayVerificationCapability: null);

    public static OperationResult<TeachingSession> Open(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath,
        ILogicalCapabilityTrainingAdmissionAuthority admissionAuthority,
        LogicalCapabilityReplayVerificationCapability
            replayVerificationCapability)
        => admissionAuthority is null
            ? OperationResult<TeachingSession>.Fail(
                "Logical capability training admission authority is required.",
                DefaultLogicalCapabilityTrainer.AdmissionAuthorityRequiredErrorCode)
            : replayVerificationCapability is null
                ? OperationResult<TeachingSession>.Fail(
                    "A server-issued replay verification capability is required.",
                    LogicalCapabilityReplayVerificationCapability.RequiredErrorCode)
                : OpenCore(
                    storage,
                    bundleId,
                    checkpointRoot,
                    snapshotPath,
                    enableCapabilityTraining: true,
                    preloadedSnapshot: null,
                    heldArtifacts: null,
                    admissionAuthority,
                    replayVerificationCapability);

    // Open a persisted capability registry for serving without installing any capability trainer.
    public static OperationResult<TeachingSession> OpenReadOnlyServing(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath)
        => OpenCore(
            storage,
            bundleId,
            checkpointRoot,
            snapshotPath,
            enableCapabilityTraining: false,
            preloadedSnapshot: null,
            heldArtifacts: null,
            admissionAuthority: null,
            replayVerificationCapability: null);

    public static OperationResult<TeachingSession> OpenReadOnlyServing(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath,
        LogicalCapabilityReplayVerificationCapability
            replayVerificationCapability)
        => replayVerificationCapability is null
            ? OperationResult<TeachingSession>.Fail(
                "A server-issued replay verification capability is required.",
                LogicalCapabilityReplayVerificationCapability.RequiredErrorCode)
            : OpenCore(
                storage,
                bundleId,
                checkpointRoot,
                snapshotPath,
                enableCapabilityTraining: false,
                preloadedSnapshot: null,
                heldArtifacts: null,
                admissionAuthority: null,
                replayVerificationCapability);

    // Serving can validate and restore the exact same in-memory snapshot object
    // obtained from one held, bounded file handle.
    public static OperationResult<TeachingSession> OpenReadOnlyServing(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath,
        TeachingRegistrySnapshot snapshot)
        => snapshot is null
            ? OperationResult<TeachingSession>.Fail(
                "Snapshot is required.",
                "SNAPSHOT_REQUIRED")
            : OpenCore(
                storage,
                bundleId,
                checkpointRoot,
                snapshotPath,
                enableCapabilityTraining: false,
                preloadedSnapshot: snapshot,
                heldArtifacts: null,
                admissionAuthority: null,
                replayVerificationCapability: null);

    public static OperationResult<TeachingSession> OpenReadOnlyServing(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath,
        TeachingRegistrySnapshot snapshot,
        LogicalCapabilityReplayVerificationCapability
            replayVerificationCapability)
        => snapshot is null
            ? OperationResult<TeachingSession>.Fail(
                "Snapshot is required.",
                "SNAPSHOT_REQUIRED")
            : replayVerificationCapability is null
                ? OperationResult<TeachingSession>.Fail(
                    "A server-issued replay verification capability is required.",
                    LogicalCapabilityReplayVerificationCapability.RequiredErrorCode)
                : OpenCore(
                    storage,
                    bundleId,
                    checkpointRoot,
                    snapshotPath,
                    enableCapabilityTraining: false,
                    preloadedSnapshot: snapshot,
                    heldArtifacts: null,
                    admissionAuthority: null,
                    replayVerificationCapability);

    public static OperationResult<TeachingSession> OpenReadOnlyServing(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath,
        TeachingRegistrySnapshot snapshot,
        IReadOnlyDictionary<string, TeachingRegistryArtifactSource> heldArtifacts)
        => snapshot is null
            ? OperationResult<TeachingSession>.Fail(
                "Snapshot is required.",
                "SNAPSHOT_REQUIRED")
            : heldArtifacts is null
                ? OperationResult<TeachingSession>.Fail(
                    "Held artifacts are required.",
                    "HELD_ARTIFACTS_REQUIRED")
                : OpenCore(
                    storage,
                    bundleId,
                    checkpointRoot,
                    snapshotPath,
                    enableCapabilityTraining: false,
                    preloadedSnapshot: snapshot,
                    heldArtifacts,
                    admissionAuthority: null,
                    replayVerificationCapability: null);

    public static OperationResult<TeachingSession> OpenReadOnlyServing(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath,
        TeachingRegistrySnapshot snapshot,
        IReadOnlyDictionary<string, TeachingRegistryArtifactSource> heldArtifacts,
        LogicalCapabilityReplayVerificationCapability
            replayVerificationCapability)
        => snapshot is null
            ? OperationResult<TeachingSession>.Fail(
                "Snapshot is required.",
                "SNAPSHOT_REQUIRED")
            : heldArtifacts is null
                ? OperationResult<TeachingSession>.Fail(
                    "Held artifacts are required.",
                    "HELD_ARTIFACTS_REQUIRED")
                : replayVerificationCapability is null
                    ? OperationResult<TeachingSession>.Fail(
                        "A server-issued replay verification capability is required.",
                        LogicalCapabilityReplayVerificationCapability.RequiredErrorCode)
                    : OpenCore(
                        storage,
                        bundleId,
                        checkpointRoot,
                        snapshotPath,
                        enableCapabilityTraining: false,
                        preloadedSnapshot: snapshot,
                        heldArtifacts,
                        admissionAuthority: null,
                        replayVerificationCapability);

    private static OperationResult<TeachingSession> OpenCore(
        ISessionStorage storage,
        string bundleId,
        string checkpointRoot,
        string snapshotPath,
        bool enableCapabilityTraining,
        TeachingRegistrySnapshot? preloadedSnapshot,
        IReadOnlyDictionary<string, TeachingRegistryArtifactSource>? heldArtifacts,
        ILogicalCapabilityTrainingAdmissionAuthority? admissionAuthority,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability)
    {
        if (storage is null)
            return OperationResult<TeachingSession>.Fail("Session storage is required.", "STORAGE_REQUIRED");
        if (string.IsNullOrWhiteSpace(bundleId))
            return OperationResult<TeachingSession>.Fail("Bundle id is required.", "BUNDLE_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(checkpointRoot))
            return OperationResult<TeachingSession>.Fail("Checkpoint root is required.", "CHECKPOINT_ROOT_REQUIRED");
        if (string.IsNullOrWhiteSpace(snapshotPath))
            return OperationResult<TeachingSession>.Fail("Snapshot path is required.", "SNAPSHOT_PATH_REQUIRED");

        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();

        var baseFlows = RegisterBaseFlows(flows);
        if (!baseFlows.IsSuccess)
            return OperationResult<TeachingSession>.Fail(baseFlows.ErrorMessage, baseFlows.ErrorCode);

        var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows, LogicalOrchestratorAssistant.DefaultFlowVersion);
        if (!seed.IsSuccess)
            return OperationResult<TeachingSession>.Fail(seed.ErrorMessage, seed.ErrorCode);

        TeachingRegistrySnapshot? snapshot;
        if (preloadedSnapshot is null)
        {
            var loadedSnapshot = TeachingRegistryPersistence.Load(snapshotPath);
            if (!loadedSnapshot.IsSuccess)
            {
                return OperationResult<TeachingSession>.Fail(
                    loadedSnapshot.ErrorMessage,
                    loadedSnapshot.ErrorCode);
            }
            snapshot = loadedSnapshot.Value;
        }
        else
        {
            snapshot = preloadedSnapshot;
        }

        if (snapshot is not null)
        {
            var load = heldArtifacts is null
                ? replayVerificationCapability is null
                    ? TeachingRegistryPersistence.LoadIntoRegistries(
                        snapshot,
                        topology,
                        units,
                        flows)
                    : TeachingRegistryPersistence.LoadIntoRegistries(
                        snapshot,
                        topology,
                        units,
                        flows,
                        replayVerificationCapability)
                : replayVerificationCapability is null
                    ? TeachingRegistryPersistence
                        .LoadIntoRegistriesFromHeldArtifacts(
                            snapshot,
                            topology,
                            units,
                            flows,
                            heldArtifacts)
                    : TeachingRegistryPersistence
                        .LoadIntoRegistriesFromHeldArtifacts(
                            snapshot,
                            topology,
                            units,
                            flows,
                            heldArtifacts,
                            replayVerificationCapability);
            if (!load.IsSuccess)
                return OperationResult<TeachingSession>.Fail(load.ErrorMessage, load.ErrorCode);
        }

        var promptCompiler = new PromptRoutingInputCompilerUnit();
        var router = new SubPhaseRouterUnit(topology);
        var multiRouter = new MultiSubPhaseRouterUnit(topology, router);
        ILogicalCapabilityTrainer? capabilityTrainer = enableCapabilityTraining
            ? new DefaultLogicalCapabilityTrainer(
                checkpointRoot,
                admissionAuthority: admissionAuthority)
            : null;
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            promptCompiler,
            router,
            flows,
            units,
            multiRouter,
            capabilityTrainer: capabilityTrainer);

        var topologyMutationFlow = new TopologyMutationFlow(
            new AddDomainTopologyMutationUnit(topology),
            new AddPhaseTopologyMutationUnit(topology),
            new AddSubPhaseTopologyMutationUnit(topology),
            new AddTransitionTopologyMutationUnit(topology));

        var assistant = new LogicalOrchestratorAssistant(
            storage,
            orchestrator,
            topologyMutationFlow: topologyMutationFlow);

        return OperationResult<TeachingSession>.Ok(new TeachingSession(
            assistant,
            topology,
            flows,
            units,
            bundleId.Trim(),
            checkpointRoot,
            snapshotPath));
    }

    private static OperationResult<bool> RegisterBaseFlows(InMemoryLogicalFlowRegistry flows)
    {
        var t34 = flows.Register(
            T34LogicalFlowAdapter.FlowId,
            T34LogicalFlowAdapter.ScenarioId,
            LogicalOrchestratorAssistant.DefaultFlowVersion,
            new T34LogicalFlowAdapter());
        if (!t34.IsSuccess)
            return OperationResult<bool>.Fail(t34.ErrorMessage, t34.ErrorCode);

        var security = flows.RegisterFactory(
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            LogicalOrchestratorAssistant.DefaultFlowVersion,
            "3",
            "3.4",
            "security-verification",
            ["security-and-authorization-model"],
            new SecurityVerification34FlowFactory());
        if (!security.IsSuccess)
            return OperationResult<bool>.Fail(security.ErrorMessage, security.ErrorCode);

        return OperationResult<bool>.Ok(true);
    }
}
