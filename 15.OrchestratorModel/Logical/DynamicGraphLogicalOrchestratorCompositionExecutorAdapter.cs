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

    private static readonly LogicalExecutionFlags NonLearningExecutionFlags = new()
    {
        UseLearnedUnitWhenAvailable = false,
        UseIdleLearningScheduling = false,
        UseKnowledgeEdgeDetection = false,
        UseSkillRetriever = false,
        UseReplayFusionRanking = false,
        WriteEvidenceArtifacts = false
    };

    private readonly ISessionStorage _storage;
    private readonly ProjectWikiTrainingFlowAdapter? _projectWikiTrainingFlowAdapter;

    // The host-neutral path uses the repository's real no-I/O storage singleton.
    public DynamicGraphLogicalOrchestratorCompositionExecutorAdapter()
        : this(NullSessionStorage.Instance)
    {
    }

    // Existing in-assembly callers and tests may retain an explicit storage/adapter seam.
    internal DynamicGraphLogicalOrchestratorCompositionExecutorAdapter(
        ISessionStorage storage,
        ProjectWikiTrainingFlowAdapter? projectWikiTrainingFlowAdapter = null)
    {
        _storage = storage;
        _projectWikiTrainingFlowAdapter = projectWikiTrainingFlowAdapter;
    }

    public OperationResult<IDynamicGraphLogicalOrchestratorCompositionSession> Compose(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
    {
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
            core.Runtime);
        return OperationResult<IDynamicGraphLogicalOrchestratorCompositionSession>.Ok(session);
    }

    internal static OperationResult<T> SessionSourceFailure<T>()
        => OperationResult<T>.Fail(
            "Dynamic graph logical orchestrator session source result is required.",
            SessionSourceResultRequiredCode);

    // Retains the runtime privately while exposing only immutable descriptor and bounded read evidence.
    private sealed class RuntimeCompositionSession
        : IDynamicGraphLogicalOrchestratorCompositionSession,
          IDynamicGraphLogicalOrchestratorRunSession,
          IDynamicGraphLogicalOrchestratorRuntimeSessionAccessor
    {
        private readonly DynamicGraphLogicalOrchestratorSessionDescriptor _descriptor;
        private readonly DynamicGraphLogicalOrchestratorRuntime _runtime;
        private readonly bool _initialRevisionAdvanced;

        internal RuntimeCompositionSession(
            DynamicGraphLogicalOrchestratorSessionDescriptor descriptor,
            DynamicGraphLogicalOrchestratorRuntime runtime)
        {
            _descriptor = descriptor with { };
            _runtime = runtime;
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
                    flags: NonLearningExecutionFlags,
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

            var result = run.Value!;
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
    }
}
