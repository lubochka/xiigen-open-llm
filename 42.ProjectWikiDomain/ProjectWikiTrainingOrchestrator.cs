// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectWikiTrainingOrchestrator
{
    private readonly ProjectWikiRequestRouterUnit _router;
    private readonly ProjectWikiRequestRoutingTrainingPipeline _requestRouting;
    private readonly StackAndVersionTrainingPipeline _stackAndVersion;
    private readonly SystemBoundaryMapperTrainingPipeline _systemBoundaryMapper;
    private readonly CrossStackBusinessFlowGraphTrainingPipeline _crossStackBusinessFlowGraph;
    private readonly CodeTraceBuilderTrainingPipeline _codeTrace;
    private readonly EntityImpactMapperTrainingPipeline _entityImpact;
    private readonly DocumentationGeneratorTrainingPipeline _documentationGenerator;
    private readonly ProjectWikiQueryPlanningTrainingPipeline _queryPlanning;
    private readonly ProjectWikiAnswerComposerTrainingPipeline _answerComposer;
    private readonly WikiContextRoutingTrainingPipeline _contextRouting;
    private readonly NegativeDpoGeneratorTrainingPipeline _negativeDpo;
    private readonly ProjectAnswerVerifierTrainingPipeline _answerVerifier;
    private readonly WikiTraceAggregatorTrainingPipeline _traceAggregator;
    private readonly WikiTraceAggregatorUnit _traceAggregatorUnit;

    public ProjectWikiTrainingOrchestrator(
        ProjectWikiRequestRouterUnit? router = null,
        ProjectWikiRequestRoutingTrainingPipeline? requestRouting = null,
        StackAndVersionTrainingPipeline? stackAndVersion = null,
        SystemBoundaryMapperTrainingPipeline? systemBoundaryMapper = null,
        CrossStackBusinessFlowGraphTrainingPipeline? crossStackBusinessFlowGraph = null,
        CodeTraceBuilderTrainingPipeline? codeTrace = null,
        EntityImpactMapperTrainingPipeline? entityImpact = null,
        DocumentationGeneratorTrainingPipeline? documentationGenerator = null,
        ProjectWikiQueryPlanningTrainingPipeline? queryPlanning = null,
        ProjectWikiAnswerComposerTrainingPipeline? answerComposer = null,
        WikiContextRoutingTrainingPipeline? contextRouting = null,
        NegativeDpoGeneratorTrainingPipeline? negativeDpo = null,
        ProjectAnswerVerifierTrainingPipeline? answerVerifier = null,
        WikiTraceAggregatorTrainingPipeline? traceAggregator = null,
        WikiTraceAggregatorUnit? traceAggregatorUnit = null)
    {
        _router = router ?? new ProjectWikiRequestRouterUnit();
        _requestRouting = requestRouting ?? new ProjectWikiRequestRoutingTrainingPipeline();
        _stackAndVersion = stackAndVersion ?? new StackAndVersionTrainingPipeline();
        _systemBoundaryMapper = systemBoundaryMapper ?? new SystemBoundaryMapperTrainingPipeline();
        _crossStackBusinessFlowGraph = crossStackBusinessFlowGraph ?? new CrossStackBusinessFlowGraphTrainingPipeline();
        _codeTrace = codeTrace ?? new CodeTraceBuilderTrainingPipeline();
        _entityImpact = entityImpact ?? new EntityImpactMapperTrainingPipeline();
        _documentationGenerator = documentationGenerator ?? new DocumentationGeneratorTrainingPipeline();
        _queryPlanning = queryPlanning ?? new ProjectWikiQueryPlanningTrainingPipeline();
        _answerComposer = answerComposer ?? new ProjectWikiAnswerComposerTrainingPipeline();
        _contextRouting = contextRouting ?? new WikiContextRoutingTrainingPipeline();
        _negativeDpo = negativeDpo ?? new NegativeDpoGeneratorTrainingPipeline();
        _answerVerifier = answerVerifier ?? new ProjectAnswerVerifierTrainingPipeline();
        _traceAggregator = traceAggregator ?? new WikiTraceAggregatorTrainingPipeline();
        _traceAggregatorUnit = traceAggregatorUnit ?? new WikiTraceAggregatorUnit();
    }

    public OperationResult<ProjectWikiTrainingLoopPlanResult> PlanNextTrainingAction(
        string learningDataRoot,
        WikiTraceAggregationResult? aggregation = null)
    {
        if (string.IsNullOrWhiteSpace(learningDataRoot))
            return OperationResult<ProjectWikiTrainingLoopPlanResult>.Fail("Learning data root is required.", "LEARNING_DATA_ROOT_REQUIRED");

        var registry = ProjectWikiTrainingAssetRegistry.LoadFromDataRoot(learningDataRoot);
        if (!registry.IsSuccess)
            return registry.MapError<ProjectWikiTrainingLoopPlanResult>();

        var continuationGate = registry.Value!.ValidateTrainingMayContinue();
        if (!continuationGate.IsSuccess)
            return continuationGate.MapError<ProjectWikiTrainingLoopPlanResult>();

        var selectedAggregation = aggregation ?? new WikiTraceAggregationResult(
            "registry-plan-only",
            0,
            0,
            1.0,
            [],
            [],
            false,
            0.50,
            ["selected_from_registry_statuses_without_live_trace_artifacts"]);
        var nextAction = _traceAggregatorUnit.SelectNextTrainingAction(selectedAggregation, registry.Value);
        if (!nextAction.IsSuccess)
            return nextAction.MapError<ProjectWikiTrainingLoopPlanResult>();

        var action = nextAction.Value!;
        var canStartNextBatch = !action.StopBlindTraining &&
                                !string.Equals(action.TargetUnitName, "NONE", StringComparison.Ordinal) &&
                                !string.Equals(action.TargetUnitName, "UNKNOWN", StringComparison.Ordinal) &&
                                !string.IsNullOrWhiteSpace(action.TargetUnitName);

        return OperationResult<ProjectWikiTrainingLoopPlanResult>.Ok(new ProjectWikiTrainingLoopPlanResult(
            learningDataRoot,
            registry.Value.Entries.Length,
            registry.Value.StatusCounts,
            action,
            SelectBatchStrategyForUnit(action.TargetUnitName),
            canStartNextBatch,
            selectedAggregation.Warnings));
    }

    public async Task<OperationResult<ProjectWikiTrainingOrchestratorResult>> RunAsync(
        string prompt,
        string artifactRoot,
        string batchId = "",
        string serverBaseUrl = "https://models.local",
        string learningDataRoot = "",
        CancellationToken ct = default)
        => await RunCoreAsync(
            prompt,
            artifactRoot,
            batchId,
            serverBaseUrl,
            learningDataRoot,
            negativeDpoRepositoryOwnedDpo: null,
            negativeDpoRepositoryOwnedModel: null,
            ct);

    public async Task<OperationResult<ProjectWikiTrainingOrchestratorResult>>
        RunWithRepositoryOwnedNegativeDpoAsync(
        string prompt,
        string artifactRoot,
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext?
            negativeDpoRepositoryOwnedDpo,
            string batchId = "",
            string serverBaseUrl = "https://models.local",
            string learningDataRoot = "",
            CancellationToken ct = default)
        // WHY: existing callers that learned the DPO-dataset context overload
        // keep compiling, but the live route still fails closed because a saved
        // model continuation context is now required before any training.
        => await RunWithRepositoryOwnedNegativeDpoAsync(
            prompt,
            artifactRoot,
            negativeDpoRepositoryOwnedDpo,
            NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                .Disabled,
            batchId,
            serverBaseUrl,
            learningDataRoot,
            ct);

    public async Task<OperationResult<ProjectWikiTrainingOrchestratorResult>>
        RunWithRepositoryOwnedNegativeDpoAsync(
        string prompt,
        string artifactRoot,
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext?
            negativeDpoRepositoryOwnedDpo,
        NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext?
            negativeDpoRepositoryOwnedModel,
        string batchId = "",
        string serverBaseUrl = "https://models.local",
        string learningDataRoot = "",
        CancellationToken ct = default)
        => await RunCoreAsync(
            prompt,
            artifactRoot,
            batchId,
            serverBaseUrl,
            learningDataRoot,
            negativeDpoRepositoryOwnedDpo,
            negativeDpoRepositoryOwnedModel,
            ct);

    private async Task<OperationResult<ProjectWikiTrainingOrchestratorResult>> RunCoreAsync(
        string prompt,
        string artifactRoot,
        string batchId,
        string serverBaseUrl,
        string learningDataRoot,
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext?
            negativeDpoRepositoryOwnedDpo,
        NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext?
            negativeDpoRepositoryOwnedModel,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiTrainingOrchestratorResult>.Fail("Training orchestration cancelled.", "TRAINING_ORCHESTRATION_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiTrainingOrchestratorResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        if (!string.IsNullOrWhiteSpace(learningDataRoot))
        {
            var registry = ProjectWikiTrainingAssetRegistry.LoadFromDataRoot(learningDataRoot);
            if (!registry.IsSuccess)
                return registry.MapError<ProjectWikiTrainingOrchestratorResult>();

            var continuationGate = registry.Value!.ValidateTrainingMayContinue();
            if (!continuationGate.IsSuccess)
                return continuationGate.MapError<ProjectWikiTrainingOrchestratorResult>();

            var rejectedContinuations = registry.Value.RejectedContinuationCandidates();
            if (rejectedContinuations.Length > 0)
                return OperationResult<ProjectWikiTrainingOrchestratorResult>.Fail(
                    $"Registry contains {rejectedContinuations.Length} rejected checkpoint continuation candidates.",
                    "REJECTED_CHECKPOINT_CONTINUATION_CANDIDATES");
        }

        var routed = _router.Route(prompt);
        if (!routed.IsSuccess)
            return routed.MapError<ProjectWikiTrainingOrchestratorResult>();

        var decision = routed.Value!;
        return decision.RouteKind switch
        {
            ProjectWikiRequestRouteKind.ProjectWikiRequestRouterTraining => FromRequestRouting(
                decision,
                await _requestRouting.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "project-wiki-request-routing-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.StackVersionTraining => FromStackAndVersion(
                decision,
                await _stackAndVersion.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "stack-version-classifier-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.SystemBoundaryMapperTraining => FromSystemBoundaryMapper(
                decision,
                await _systemBoundaryMapper.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "system-boundary-mapper-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.CrossStackBusinessFlowGraphTraining => FromCrossStackBusinessFlowGraph(
                decision,
                await _crossStackBusinessFlowGraph.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "cross-stack-business-flow-graph-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.CodeTraceTraining => FromCodeTrace(
                decision,
                await _codeTrace.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "code-trace-builder-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.EntityImpactTraining => FromEntityImpact(
                decision,
                await _entityImpact.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "entity-impact-mapper-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.DocumentationGeneratorTraining => FromDocumentationGenerator(
                decision,
                await _documentationGenerator.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "documentation-generator-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.QueryPlanningTraining => FromQueryPlanning(
                decision,
                await _queryPlanning.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "project-wiki-query-planning-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.AnswerComposerTraining => FromAnswerComposer(
                decision,
                await _answerComposer.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "project-wiki-answer-composer-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.ContextRoutingTraining => FromContextRouting(
                decision,
                await _contextRouting.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "wiki-context-routing-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.NegativeDpoTraining => FromNegativeDpo(
                decision,
                negativeDpoRepositoryOwnedDpo is null
                    ? await _negativeDpo.RunOneBatchAsync(
                        artifactRoot,
                        SelectBatchId(batchId, "negative-dpo-generator-batch-001"),
                        serverBaseUrl,
                        ct)
                    : await _negativeDpo.RunRepositoryOwnedBatchAsync(
                        negativeDpoRepositoryOwnedDpo,
                        negativeDpoRepositoryOwnedModel ??
                        NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                            .Disabled,
                        artifactRoot,
                        SelectBatchId(batchId, "negative-dpo-generator-batch-001"),
                        serverBaseUrl,
                        ct)),
            ProjectWikiRequestRouteKind.AnswerVerifierTraining => FromAnswerVerifier(
                decision,
                await _answerVerifier.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "project-answer-verifier-batch-001"), serverBaseUrl, ct)),
            ProjectWikiRequestRouteKind.TraceAggregatorTraining => FromTraceAggregator(
                decision,
                await _traceAggregator.RunOneBatchAsync(artifactRoot, SelectBatchId(batchId, "wiki-trace-aggregator-batch-001"), serverBaseUrl, ct)),
            _ => OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
                decision,
                SelectBatchId(batchId, "no-training-batch"),
                decision.TargetUnitId,
                "REQUEST_ROUTE_NOT_TRAINING_PIPELINE",
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                null,
                "Route the request to the selected documentation, answer, validation, or main orchestrator path; do not run a training batch for this prompt.",
                Combine(decision.Warnings, ["training_orchestrator_rejected_non_training_route"])))
        };
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromRequestRouting(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<ProjectWikiRequestRoutingTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            run.RouteAccuracy,
            run.AverageConfidence,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromStackAndVersion(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<StackAndVersionTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.RuntimeAccuracy, run.TargetFrameworkAccuracy, run.LanguageVersionAccuracy, run.ProjectStyleAccuracy),
            run.AverageConfidence,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? [])));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromCodeTrace(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<CodeTraceBuilderTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.MethodRecall, run.CallEdgeRecall, run.ValueFlowRecall),
            run.AverageConfidence,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromSystemBoundaryMapper(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<SystemBoundaryMapperTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.BoundaryEdgeAccuracy, run.UnknownBoundaryAccuracy, run.EvidenceGroundingRate),
            run.EvidenceGroundingRate,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromCrossStackBusinessFlowGraph(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.ClientToStoredProcedurePathRecall, run.ClientServerEdgeRecall, run.StoredProcedureEdgeRecall, run.EvidenceGroundingRate),
            run.EvidenceGroundingRate,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromEntityImpact(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<EntityImpactMapperTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.EvidenceRecall, run.TouchpointRecall),
            run.AverageConfidence,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromDocumentationGenerator(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<DocumentationGeneratorTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            run.RequiredFieldCoverage,
            run.AverageConfidence,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromQueryPlanning(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<ProjectWikiQueryPlanningTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.QueryPlanAccuracy, run.RequiredTouchpointRecall, run.MultiHopSequenceAccuracy, run.MissingEvidenceDetection),
            run.AverageAnswerReadinessScore,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromAnswerComposer(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<ProjectWikiAnswerComposerTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.ExactEvidenceAccuracy, run.CitationAccuracy, run.AnswerShapeAccuracy, run.MissingEvidenceAbstentionAccuracy),
            run.EvidenceGroundingSecondaryScore(),
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromContextRouting(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<ProjectWikiContextRoutingTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.RouteAccuracy, run.PreCheckAccuracy),
            run.AverageConfidence,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromNegativeDpo(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<NegativeDpoGeneratorTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.ChosenRejectedSeparationRate, run.WrongAnnotationRate, run.EvidenceGroundingRate, run.RejectedDefectAccuracy, run.NegativeTypeCoverageRate),
            run.NegativeTypeCoverageRate,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.RepositoryOwnedModelCheckpointEvidence is not null ||
            value.DpoMetrics is null
                ? null
                : Path.Combine(
                    value.DpoMetrics.ResultRoot,
                    "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath,
            BuildRepositoryOwnedDpoDatasetEvidence(
                value.RepositoryOwnedDpoDataset),
            value.RepositoryOwnedModelCheckpointEvidence));
    }

    private static ProjectWikiRepositoryOwnedDpoDatasetEvidence?
        BuildRepositoryOwnedDpoDatasetEvidence(
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch?
                dataset)
        => dataset is null
            ? null
            : new ProjectWikiRepositoryOwnedDpoDatasetEvidence(
                dataset.Source,
                dataset.ManifestRelativePath,
                dataset.ManifestSha256,
                dataset.ReopenedProjectPairsSha256);

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromAnswerVerifier(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<ProjectAnswerVerifierTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.PassFailAccuracy, run.WarningSetAccuracy),
            run.AverageCitationAccuracy,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static OperationResult<ProjectWikiTrainingOrchestratorResult> FromTraceAggregator(
        ProjectWikiRequestRoutingDecision decision,
        OperationResult<WikiTraceAggregatorTrainingPipelineResult> result)
    {
        if (!result.IsSuccess)
            return result.MapError<ProjectWikiTrainingOrchestratorResult>();
        var value = result.Value!;
        var run = value.TrainingRun;
        return OperationResult<ProjectWikiTrainingOrchestratorResult>.Ok(new ProjectWikiTrainingOrchestratorResult(
            decision,
            value.BatchId,
            decision.TargetUnitId,
            run.Verdict,
            run.TrainingRowCount,
            run.HeldOutRowCount,
            Min(run.FailedUnitAccuracy, run.RemediationAccuracy, run.StopDecisionAccuracy),
            run.AverageConfidence,
            run.WrongAnnotationRate,
            run.CpuMilliseconds,
            run.PeakWorkingSetMb,
            run.ElapsedMilliseconds,
            value.Publication?.GeneratedCapabilityManifestPath,
            value.NextAction,
            Combine(decision.Warnings, value.Warnings, value.Publication?.Warnings ?? []),
            value.DpoMetrics?.FinalLoss,
            value.DpoMetrics?.FinalAccuracy,
            value.DpoMetrics?.FinalRewardMargin,
            value.DpoMetrics?.HeldOutLoss,
            value.DpoMetrics?.HeldOutAccuracy,
            value.DpoMetrics?.HeldOutRewardMargin,
            value.DpoMetrics is null ? null : Path.Combine(value.DpoMetrics.ResultRoot, "project-wiki-dpo-metric-summary.json"),
            value.DpoMetrics?.AverageCpuPercent,
            value.DpoMetrics?.TotalCpuTimeMs,
            value.DpoMetrics?.PeakWorkingSetMb,
            value.DpoMetrics?.TotalElapsedMs,
            value.DpoMetrics?.PairDiagnosticsPath));
    }

    private static string SelectBatchId(string requested, string fallback)
        => string.IsNullOrWhiteSpace(requested) ? fallback : requested;

    private string SelectBatchStrategyForUnit(string unitName)
        => unitName switch
        {
            "ProjectWikiRequestRouterUnit" => "build_50_project_wiki_request_routing_rows",
            "StackAndVersionClassifierUnit" => "build_50_stack_and_version_classifier_rows",
            "SystemBoundaryMapperUnit" => "build_50_system_boundary_mapper_rows",
            "CodeTraceBuilderUnit" => "build_50_code_trace_builder_rows",
            "EntityImpactMapperUnit" => "build_50_entity_impact_mapper_rows",
            "DocumentationGeneratorUnit" => "build_50_documentation_generator_rows",
            "NegativeDpoGenerator" => "build_50_negative_dpo_generator_rows",
            "CrossStackBusinessFlowGraphUnit" => "build_50_cross_stack_business_flow_graph_rows",
            "ProjectWikiQueryPlanningFlow" => "build_50_project_wiki_query_planning_rows",
            "ProjectWikiAnswerComposer" => "build_50_project_wiki_answer_composer_rows",
            "WikiContextRoutingUnit" => "build_50_wiki_context_routing_rows",
            "ProjectAnswerVerifier" => "build_50_project_answer_verifier_rows",
            "WikiTraceAggregator" => "build_50_wiki_trace_aggregator_rows",
            _ => "manual_architect_review_required"
        };

    private static double Min(params double[] values)
        => values.Length == 0 ? 0 : values.Min();

    private static string[] Combine(params string[][] groups)
        => groups.SelectMany(group => group).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
}

file static class ProjectWikiTrainingMetricExtensions
{
    public static double EvidenceGroundingSecondaryScore(this ProjectWikiAnswerComposerTrainingRunResult run)
        => (run.ExactEvidenceAccuracy + run.CitationAccuracy + run.AnswerShapeAccuracy + run.MissingEvidenceAbstentionAccuracy) / 4.0;
}
