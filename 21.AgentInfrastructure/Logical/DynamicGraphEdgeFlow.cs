// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using System.Globalization;
using AgentInfrastructure;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

// This exact Plan-02 carrier supplies one L3 edge traversal with existing source/context evidence.
public sealed record EdgeTraversalInput(
    string RunId,
    string EdgeId,
    SubPhaseRouterSourceDocument Source,
    ContextSignals Context,
    NodeOutputToEdgeInputAdapter UpstreamNodeOutput);

// This exact Plan-02 adapter carries an order-free node-output reference snapshot into an edge.
public sealed record NodeOutputToEdgeInputAdapter(
    string SourceNodeId,
    IReadOnlyList<string> UpstreamCandidateRefs,
    string UpstreamOutputHash,
    string ProjectionRule);

// This L3 flow performs deterministic FanIn/ReturnLoop work or emits an inference-only learned target handoff.
public sealed class DynamicGraphEdgeFlow : ILogicalFlow<EdgeTraversalInput>
{
    private const string ExpectedAuthoritySource = RoutingAuthorityVocabulary.TrainedEdgeUnitRole;
    private const string TargetHandoffReadyVerdict = "TARGET_HANDOFF_READY";
    private const string AuthorityGapHandoffVerdict = "EDGE_AUTHORITY_GAP_HANDOFF";
    private const string HandoffToL4Action = "HANDOFF_TO_L4";
    private const string HandoffGapToL4Action = "HANDOFF_GAP_TO_L4";
    private const string ReservedWarningRedacted = "edge_authority_reserved_warning_redacted";
    private const string AuthorityGapWarning = "edge_authority_gap_requires_l4";
    private const string LegacyKnowledgeEdgeWarningMarker = "knowledge_edge";
    private const string ReturnLoopAuthoritySource = "topology_return_loop";
    private const string ExpectedNodeOutputProjectionRule = "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)";
    private const string ReturnLoopHandoffReadyVerdict = "RETURN_LOOP_HANDOFF_READY";
    private const string ReturnLoopToL4Action = "HANDOFF_RETURN_LOOP_TO_L4";
    private const string ReturnLoopSummarySuffix = "return-loop-handoff";
    private const string ReturnLoopPrimaryMetricName = "correction_target_count";
    private const string ReturnLoopScalingSignalNote = "Deterministic correction handoff only; scaling was not evaluated.";
    private const string ReturnLoopSummaryNotes = "Topology-defined ReturnLoop produced a single typed correction handoff; no target was executed and topology was not mutated.";
    private const string ReturnLoopHumanResult = "Topology-defined ReturnLoop emitted its single correction target; target execution was not performed.";

    private const string CancelledCode = "CANCELLED";
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_EDGE_REQUEST_REQUIRED";
    private const string InputRequiredCode = "DYNAMIC_GRAPH_EDGE_INPUT_REQUIRED";
    private const string RunIdRequiredCode = "DYNAMIC_GRAPH_EDGE_RUN_ID_REQUIRED";
    private const string FlowIdRequiredCode = "DYNAMIC_GRAPH_EDGE_FLOW_ID_REQUIRED";
    private const string ScenarioIdRequiredCode = "DYNAMIC_GRAPH_EDGE_SCENARIO_ID_REQUIRED";
    private const string RunIdMismatchCode = "DYNAMIC_GRAPH_EDGE_RUN_ID_MISMATCH";
    private const string EdgeIdRequiredCode = "DYNAMIC_GRAPH_EDGE_ID_REQUIRED";
    private const string SourceDocumentRequiredCode = "DYNAMIC_GRAPH_EDGE_SOURCE_DOCUMENT_REQUIRED";
    private const string ContextRequiredCode = "DYNAMIC_GRAPH_EDGE_CONTEXT_REQUIRED";
    private const string UpstreamOutputRequiredCode = "DYNAMIC_GRAPH_UPSTREAM_OUTPUT_REQUIRED";
    private const string UpstreamSourceRequiredCode = "DYNAMIC_GRAPH_UPSTREAM_SOURCE_REQUIRED";
    private const string UpstreamRefsInvalidCode = "DYNAMIC_GRAPH_UPSTREAM_REFS_INVALID";
    private const string UpstreamRefsNotCanonicalCode = "DYNAMIC_GRAPH_UPSTREAM_REFS_NOT_CANONICAL";
    private const string UpstreamHashRequiredCode = "DYNAMIC_GRAPH_UPSTREAM_HASH_REQUIRED";
    private const string UpstreamProjectionRuleRequiredCode = "DYNAMIC_GRAPH_UPSTREAM_PROJECTION_RULE_REQUIRED";
    private const string TraversalSnapshotFailedCode = "DYNAMIC_GRAPH_EDGE_TRAVERSAL_SNAPSHOT_FAILED";
    private const string MergerRequiredCode = "EDGE_FAN_IN_MERGER_REQUIRED";
    private const string MaterializerRequiredCode = "EDGE_FAN_IN_INPUT_MATERIALIZER_REQUIRED";
    private const string AuthorityPipelineRequiredCode = "EDGE_AUTHORITY_PIPELINE_REQUIRED";
    private const string AuthorityPipelineFailedCode = "EDGE_AUTHORITY_PIPELINE_FAILED";
    private const string AuthorityResultRequiredCode = "EDGE_AUTHORITY_RESULT_REQUIRED";
    private const string AuthorityResultInvalidCode = "EDGE_AUTHORITY_RESULT_INVALID";
    private const string EdgeNotFoundCode = "DYNAMIC_GRAPH_EDGE_NOT_FOUND";
    private const string EdgeKindUnsupportedCode = "DYNAMIC_GRAPH_EDGE_KIND_UNSUPPORTED";
    private const string EdgeSourceMismatchCode = "DYNAMIC_GRAPH_EDGE_SOURCE_MISMATCH";
    private const string MaterializerFailedCode = "EDGE_FAN_IN_INPUT_MATERIALIZER_FAILED";
    private const string MergeInputRequiredCode = "EDGE_FAN_IN_MERGE_INPUT_REQUIRED";
    private const string FanInRunIdRequiredCode = "EDGE_FAN_IN_RUN_ID_REQUIRED";
    private const string FanInRunIdMismatchCode = "EDGE_FAN_IN_RUN_ID_MISMATCH";
    private const string MergerFailedCode = "EDGE_FAN_IN_MERGER_FAILED";
    private const string MergeResultRequiredCode = "EDGE_FAN_IN_MERGE_RESULT_REQUIRED";
    private const string MergeResultRunIdRequiredCode = "EDGE_FAN_IN_MERGE_RESULT_RUN_ID_REQUIRED";
    private const string MergeResultRunIdMismatchCode = "EDGE_FAN_IN_MERGE_RESULT_RUN_ID_MISMATCH";
    private const string BestOfForbiddenCode = "BEST_OF_MERGE_FORBIDDEN";
    private const string MergeOrderAuthorityCode = "MERGE_ORDER_USED_AS_AUTHORITY";
    private const string BranchCardinalityInvalidCode = "DYNAMIC_GRAPH_FAN_IN_BRANCH_CARDINALITY_INVALID";
    private const string MergeResultBranchCountMismatchCode = "DYNAMIC_GRAPH_FAN_IN_MERGE_RESULT_BRANCH_COUNT_MISMATCH";
    private const string UnconsumedTargetHandoffCode = "EDGE_FAN_IN_UNCONSUMED_TARGET_HANDOFF";
    private const string ReturnLoopInvalidCode = "DYNAMIC_GRAPH_RETURN_LOOP_INVALID";

    private readonly DynamicGraphTopology? _topology;
    private readonly IEdgeFanInMerger? _merger;
    private readonly Func<
        DynamicGraphTopology,
        IDynamicGraphEdge,
        EdgeTraversalInput,
        OperationResult<GraphFanInMergeInput>>? _fanInInputMaterializer;
    private readonly IDynamicGraphEdgeAuthorityExecutionPipeline? _edgeAuthorityPipeline;

    // The Func remains the provisional materialization seam until its owner defines a durable contract.
    public DynamicGraphEdgeFlow(
        DynamicGraphTopology? topology,
        IEdgeFanInMerger? merger,
        Func<DynamicGraphTopology, IDynamicGraphEdge, EdgeTraversalInput, OperationResult<GraphFanInMergeInput>>? fanInInputMaterializer)
        : this(topology, merger, fanInInputMaterializer, null)
    {
    }

    // New learned-edge wiring uses a distinct metadata signature while preserving the original constructor binary ABI.
    public DynamicGraphEdgeFlow(
        DynamicGraphTopology? topology,
        IEdgeFanInMerger? merger,
        Func<DynamicGraphTopology, IDynamicGraphEdge, EdgeTraversalInput, OperationResult<GraphFanInMergeInput>>? fanInInputMaterializer,
        IDynamicGraphEdgeAuthorityExecutionPipeline? edgeAuthorityPipeline)
    {
        _topology = topology;
        _merger = merger;
        _fanInInputMaterializer = fanInInputMaterializer;
        _edgeAuthorityPipeline = edgeAuthorityPipeline;
    }

    // ExecuteAsync validates one stable traversal boundary before dispatching to exactly one edge-kind path.
    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<EdgeTraversalInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        if (request is null)
            return Failure("Dynamic graph edge flow request is required.", RequestRequiredCode);

        if (request.Input is null)
            return Failure("Dynamic graph edge traversal input is required.", InputRequiredCode);

        if (string.IsNullOrWhiteSpace(request.RunId))
            return Failure("Dynamic graph edge RunId is required.", RunIdRequiredCode);

        if (string.IsNullOrWhiteSpace(request.FlowId))
            return Failure("Dynamic graph edge FlowId is required.", FlowIdRequiredCode);

        if (string.IsNullOrWhiteSpace(request.ScenarioId))
            return Failure("Dynamic graph edge ScenarioId is required.", ScenarioIdRequiredCode);

        var input = request.Input;
        if (string.IsNullOrWhiteSpace(input.RunId))
            return Failure("Dynamic graph edge RunId is required.", RunIdRequiredCode);

        if (!StringComparer.Ordinal.Equals(request.RunId, input.RunId))
            return Failure("Flow request RunId does not match edge traversal RunId.", RunIdMismatchCode);

        if (string.IsNullOrWhiteSpace(input.EdgeId))
            return Failure("Dynamic graph edge traversal EdgeId is required.", EdgeIdRequiredCode);

        if (input.Source is null)
            return Failure("Dynamic graph edge source document is required.", SourceDocumentRequiredCode);

        if (input.Context is null)
            return Failure("Dynamic graph edge context is required.", ContextRequiredCode);

        if (input.UpstreamNodeOutput is null)
            return Failure("Upstream node output is required.", UpstreamOutputRequiredCode);

        if (input.UpstreamNodeOutput.UpstreamCandidateRefs is null)
        {
            return Failure(
                "Upstream candidate references must be materialized and non-empty.",
                UpstreamRefsInvalidCode);
        }

        EdgeTraversalInput stableInput;
        try
        {
            if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotTraversalInput(
                    input,
                    out stableInput))
            {
                return Failure(
                    "Dynamic graph edge traversal snapshot failed.",
                    TraversalSnapshotFailedCode);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure("Cancelled.", CancelledCode);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure(
                "Dynamic graph edge traversal snapshot failed.",
                TraversalSnapshotFailedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        var stableUpstream = stableInput.UpstreamNodeOutput;
        if (!IsCanonicalValue(stableUpstream.SourceNodeId))
            return Failure("Upstream source node id is required.", UpstreamSourceRequiredCode);

        if (stableUpstream.UpstreamCandidateRefs is null
            || stableUpstream.UpstreamCandidateRefs.Count == 0
            || stableUpstream.UpstreamCandidateRefs.Any(reference => !IsCanonicalValue(reference)))
        {
            return Failure(
                "Upstream candidate references must be materialized and non-empty.",
                UpstreamRefsInvalidCode);
        }

        if (!AreCanonicalRefs(stableUpstream.UpstreamCandidateRefs))
        {
            return Failure(
                "Upstream candidate references must be distinct and ordinal-sorted.",
                UpstreamRefsNotCanonicalCode);
        }

        if (!IsCanonicalValue(stableUpstream.UpstreamOutputHash))
            return Failure("Upstream output hash is required.", UpstreamHashRequiredCode);

        if (!IsCanonicalValue(stableUpstream.ProjectionRule))
        {
            return Failure(
                "Upstream projection rule is required.",
                UpstreamProjectionRuleRequiredCode);
        }

        var topologyValidation = DynamicGraphValidation.Validate(_topology);
        if (!topologyValidation.IsSuccess)
            return Failure(topologyValidation.ErrorMessage, topologyValidation.ErrorCode);

        var topology = topologyValidation.Value!;
        var edge = topology.Edges.FirstOrDefault(candidate =>
            StringComparer.Ordinal.Equals(candidate.EdgeId, stableInput.EdgeId));
        if (edge is null)
            return Failure($"Dynamic graph edge '{stableInput.EdgeId}' was not found.", EdgeNotFoundCode);

        var sourceEdge = topology.ListEdgesFrom(stableUpstream.SourceNodeId).FirstOrDefault(candidate =>
            StringComparer.Ordinal.Equals(candidate.EdgeId, stableInput.EdgeId));
        if (sourceEdge is null)
        {
            return Failure(
                "Upstream source node does not match the selected edge source.",
                EdgeSourceMismatchCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        switch (sourceEdge.Kind)
        {
            case GraphEdgeKind.FanIn:
                return ExecuteFanIn(topology, sourceEdge, stableInput, cancellationToken);

            case GraphEdgeKind.FanOut:
            case GraphEdgeKind.ManyToMany:
            case GraphEdgeKind.MultiOutput:
                return await ExecuteLearnedTargetHandoffAsync(
                        sourceEdge,
                        stableInput,
                        topology.DomainTag,
                        request.FlowId,
                        request.ScenarioId,
                        request.Flags,
                        cancellationToken)
                    .ConfigureAwait(false);

            case GraphEdgeKind.ReturnLoop:
                return ExecuteReturnLoop(
                    topology,
                    sourceEdge,
                    stableInput,
                    topology.DomainTag,
                    request.FlowId,
                    request.ScenarioId,
                    cancellationToken);

            default:
                return Failure(
                    $"Dynamic graph edge '{stableInput.EdgeId}' kind is unsupported by this flow.",
                    EdgeKindUnsupportedCode);
        }
    }

    // FanIn remains a deterministic aggregation path and never invokes learned edge authority.
    private OperationResult<LogicalFlowResult> ExecuteFanIn(
        DynamicGraphTopology topology,
        IDynamicGraphEdge sourceEdge,
        EdgeTraversalInput stableInput,
        CancellationToken cancellationToken)
    {
        var merger = _merger;
        if (merger is null)
            return Failure("Edge fan-in merger is required.", MergerRequiredCode);

        var fanInInputMaterializer = _fanInInputMaterializer;
        if (fanInInputMaterializer is null)
            return Failure("Edge fan-in input materializer is required.", MaterializerRequiredCode);

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        OperationResult<GraphFanInMergeInput>? materialized;
        try
        {
            materialized = fanInInputMaterializer(topology, sourceEdge, stableInput);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure("Cancelled.", CancelledCode);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Edge fan-in input materialization failed.", MaterializerFailedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        if (materialized is null)
            return Failure("Edge fan-in input materialization failed.", MaterializerFailedCode);

        if (!materialized.IsSuccess)
            return Failure(materialized.ErrorMessage, materialized.ErrorCode);

        var mergeInput = materialized.Value;
        if (mergeInput is null)
            return Failure("Materialized edge fan-in merge input is required.", MergeInputRequiredCode);

        if (string.IsNullOrWhiteSpace(mergeInput.RunId))
            return Failure("Materialized edge fan-in RunId is required.", FanInRunIdRequiredCode);

        if (!StringComparer.Ordinal.Equals(mergeInput.RunId, stableInput.RunId))
            return Failure("Materialized edge fan-in RunId does not match traversal RunId.", FanInRunIdMismatchCode);

        var expectedMergedBranchCount = mergeInput.Branches?.Length;

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        OperationResult<GraphFanInMergeResult>? merged;
        try
        {
            merged = merger.Merge(mergeInput);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure("Cancelled.", CancelledCode);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Edge fan-in merge failed.", MergerFailedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        if (merged is null)
            return Failure("Edge fan-in merge failed.", MergerFailedCode);

        if (!merged.IsSuccess)
            return Failure(merged.ErrorMessage, merged.ErrorCode);

        var mergeResult = merged.Value;
        if (mergeResult is null)
            return Failure("Edge fan-in merge result is required.", MergeResultRequiredCode);
        var mergeContext = mergeResult.Context;
        if (string.IsNullOrWhiteSpace(mergeContext.RunId))
            return Failure("Edge fan-in merge result RunId is required.", MergeResultRunIdRequiredCode);

        if (!StringComparer.Ordinal.Equals(mergeContext.RunId, mergeInput.RunId))
        {
            return Failure(
                "Edge fan-in merge result RunId does not match merge input RunId.",
                MergeResultRunIdMismatchCode);
        }

        if (mergeResult.MergeRole != FanInMergeRole.DeterministicFanInBaseline)
            return Failure("Fan-in merge must use the deterministic baseline role.", BestOfForbiddenCode);

        if (mergeResult.MergeOrderIsAuthority)
            return Failure("Fan-in merge order cannot be semantic authority.", MergeOrderAuthorityCode);

        if (mergeResult.MergedBranchCount < 2)
        {
            return Failure(
                "Dynamic graph FanIn traversal requires at least two merged branches.",
                BranchCardinalityInvalidCode);
        }

        if (expectedMergedBranchCount is null
            || mergeResult.MergedBranchCount != expectedMergedBranchCount.Value)
        {
            return Failure(
                "Edge fan-in merge result branch count does not match materialized input.",
                MergeResultBranchCountMismatchCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        var mergedOutput = mergeResult.Merged;
        if (mergedOutput.EdgeTargetHandoff is not null)
        {
            return Failure(
                "Edge fan-in merge result contains an unconsumed target handoff.",
                UnconsumedTargetHandoffCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        return OperationResult<LogicalFlowResult>.Ok(mergedOutput);
    }

    // ReturnLoop is a deterministic structural correction handoff and never invokes learned edge authority.
    private static OperationResult<LogicalFlowResult> ExecuteReturnLoop(
        DynamicGraphTopology topology,
        IDynamicGraphEdge edge,
        EdgeTraversalInput stableInput,
        string domainTag,
        string flowId,
        string scenarioId,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        if (edge.ToNodeIds is null
            || edge.ToNodeIds.Count != 1
            || edge.ParallelEligible
            || !IsCanonicalValue(edge.ToNodeIds[0]))
        {
            return Failure(
                "Dynamic graph ReturnLoop edge must contain one canonical, non-parallel correction target.",
                ReturnLoopInvalidCode);
        }

        var sourceOrders = topology.Order
            .Where(entry => entry is not null
                            && StringComparer.Ordinal.Equals(entry.NodeId, edge.FromNodeId))
            .ToArray();
        var targetOrders = topology.Order
            .Where(entry => entry is not null
                            && StringComparer.Ordinal.Equals(entry.NodeId, edge.ToNodeIds[0]))
            .ToArray();
        if (sourceOrders.Length != 1
            || targetOrders.Length != 1
            || targetOrders[0].Position >= sourceOrders[0].Position)
        {
            return Failure(
                "Dynamic graph ReturnLoop target must be an earlier topology node.",
                ReturnLoopInvalidCode);
        }

        var upstream = stableInput.UpstreamNodeOutput;
        if (upstream is null
            || upstream.UpstreamCandidateRefs is null
            || upstream.UpstreamCandidateRefs.Count == 0
            || !AreCanonicalRefs(upstream.UpstreamCandidateRefs)
            || upstream.UpstreamCandidateRefs.Any(reference => !IsCanonicalValue(reference))
            || !IsCanonicalValue(upstream.UpstreamOutputHash)
            || !StringComparer.Ordinal.Equals(
                upstream.ProjectionRule,
                ExpectedNodeOutputProjectionRule))
        {
            return Failure(
                "Dynamic graph ReturnLoop provenance is invalid.",
                ReturnLoopInvalidCode);
        }

        var selection = new DynamicGraphEdgeTargetSelection(
            edge.ToNodeIds[0],
            Confidence: 1d,
            Ordinal: 0);
        var handoff = new DynamicGraphEdgeTargetHandoff(
            stableInput.RunId,
            edge.EdgeId,
            edge.FromNodeId,
            domainTag,
            GraphEdgeKind.ReturnLoop,
            ParallelEligible: false,
            Array.AsReadOnly(new[] { selection }),
            KnowledgeEdge: false,
            ReturnLoopAuthoritySource,
            upstream.UpstreamCandidateRefs,
            upstream.UpstreamOutputHash,
            upstream.ProjectionRule);
        var summary = new LogicalFlowSummary(
            SummaryId: $"{flowId}:{ReturnLoopSummarySuffix}",
            Tier: 3,
            BaselinePrimary: 0d,
            EnhancedPrimary: 1d,
            PrimaryMetricName: ReturnLoopPrimaryMetricName,
            LowerIsBetter: false,
            Passed: true,
            ShouldScaleUp: false,
            NextAction: ReturnLoopToL4Action,
            ScalingSignal: 0d,
            ScalingSignalNote: ReturnLoopScalingSignalNote,
            Notes: ReturnLoopSummaryNotes,
            BaselineTimeMs: 0d,
            EnhancedTimeMs: 0d);
        var metrics = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["authority_source"] = ReturnLoopAuthoritySource,
            ["authority_gap"] = "false",
            ["authority_warning_count"] = "0",
            ["edge_id"] = edge.EdgeId,
            ["edge_kind"] = GraphEdgeKind.ReturnLoop.ToString(),
            ["parallel_eligible"] = "false",
            ["target_count"] = "1",
            ["targets_executed"] = "false",
            ["topology_mutated"] = "false"
        };
        var result = new LogicalFlowResult(
            FlowId: flowId,
            ScenarioId: scenarioId,
            OverallVerdict: ReturnLoopHandoffReadyVerdict,
            FinalHumanResult: ReturnLoopHumanResult,
            Summary: summary,
            Metrics: new ReadOnlyDictionary<string, string>(metrics),
            EvidenceIds: [],
            Warnings: [])
        {
            TransitionSignals = [],
            EdgeTargetHandoff = handoff
        };

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    // The learned path invokes the published 17E pipeline once and emits only a typed target handoff.
    private async Task<OperationResult<LogicalFlowResult>> ExecuteLearnedTargetHandoffAsync(
        IDynamicGraphEdge edge,
        EdgeTraversalInput stableInput,
        string domainTag,
        string flowId,
        string scenarioId,
        LogicalExecutionFlags flags,
        CancellationToken cancellationToken)
    {
        var pipeline = _edgeAuthorityPipeline;
        if (pipeline is null)
            return Failure("Learned edge authority pipeline is required.", AuthorityPipelineRequiredCode);

        OperationResult<EdgeAuthorityExecutionResult>? executed;
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return Failure("Cancelled.", CancelledCode);

            executed = await pipeline.ExecuteAsync(
                    edge,
                    stableInput,
                    domainTag,
                    flags,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure("Cancelled.", CancelledCode);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Learned edge authority pipeline failed.", AuthorityPipelineFailedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Failure("Cancelled.", CancelledCode);

        if (executed is null || (executed.IsSuccess && executed.Value is null))
            return Failure("Learned edge authority result is required.", AuthorityResultRequiredCode);

        if (!executed.IsSuccess)
        {
            return IsValidFailureEnvelope(executed.ErrorMessage, executed.ErrorCode)
                ? Failure(executed.ErrorMessage, executed.ErrorCode)
                : Failure("Learned edge authority pipeline failed.", AuthorityPipelineFailedCode);
        }

        try
        {
            if (!TryBuildTargetHandoffResult(
                    executed.Value!,
                    edge,
                    stableInput,
                    domainTag,
                    flowId,
                    scenarioId,
                    out var flowResult))
            {
                return Failure("Learned edge authority result is invalid.", AuthorityResultInvalidCode);
            }

            if (cancellationToken.IsCancellationRequested)
                return Failure("Cancelled.", CancelledCode);

            return OperationResult<LogicalFlowResult>.Ok(flowResult);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure("Cancelled.", CancelledCode);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Learned edge authority result is invalid.", AuthorityResultInvalidCode);
        }
    }

    private static bool TryBuildTargetHandoffResult(
        EdgeAuthorityExecutionResult execution,
        IDynamicGraphEdge edge,
        EdgeTraversalInput stableInput,
        string domainTag,
        string flowId,
        string scenarioId,
        out LogicalFlowResult result)
    {
        result = null!;

        var normalizedInput = execution.NormalizedInput;
        var proposedCandidates = execution.ProposedCandidates?.ToArray();
        var normalizedCandidates = normalizedInput?.Candidates?.ToArray();
        var decision = execution.Decision;
        var unitConfidence = execution.UnitConfidence;
        var evidenceIds = execution.EvidenceIds.ToArray();
        var warnings = execution.Warnings.ToArray();

        if (normalizedInput is null
            || decision is null
            || !StringComparer.Ordinal.Equals(normalizedInput.EdgeId, edge.EdgeId)
            || !StringComparer.Ordinal.Equals(normalizedInput.EdgeId, stableInput.EdgeId)
            || !StringComparer.Ordinal.Equals(decision.AuthoritySource, ExpectedAuthoritySource)
            || !double.IsFinite(unitConfidence)
            || unitConfidence is < 0d or > 1d
            || decision.OrderedTargets is null
            || edge.ToNodeIds is null
            || evidenceIds.Any(value => value is null || ContainsLineBreak(value))
            || warnings.Any(value => value is null
                                             || value.Any(char.IsControl)
                                             || ContainsLineBreak(value)))
        {
            return false;
        }

        var orderedTargets = decision.OrderedTargets.ToArray();
        if (decision.FanOutCount != orderedTargets.Length
            || (decision.KnowledgeEdge && orderedTargets.Length != 0)
            || (!decision.KnowledgeEdge && orderedTargets.Length == 0))
        {
            return false;
        }

        var edgeTargets = edge.ToNodeIds.ToHashSet(StringComparer.Ordinal);
        if (edgeTargets.Count != edge.ToNodeIds.Count
            || edgeTargets.Any(target => !IsCanonicalValue(target)))
        {
            return false;
        }

        if (!HasExactPriorConfidenceMap(stableInput.Context, normalizedInput.Context))
            return false;

        if (!HasExactContextMembership(stableInput.Context, normalizedInput.Context))
            return false;

        if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotNormalizedSemanticEvidence(
                normalizedInput.SemanticTokens,
                normalizedInput.PredicateIds,
                out _,
                out _))
        {
            return false;
        }

        if (!DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(normalizedInput.ExcludedLeakageSignals))
            return false;

        if (!TryGetCorrelatedCandidateTargetIds(
                proposedCandidates,
                normalizedCandidates,
                edgeTargets,
                out var candidateTargetIds))
        {
            return false;
        }

        var selectedIds = new HashSet<string>(StringComparer.Ordinal);
        var selections = new DynamicGraphEdgeTargetSelection[orderedTargets.Length];
        // Target selection is sourced only from the learned decision; baseline and ablation ranks are not consulted.
        for (var index = 0; index < orderedTargets.Length; index++)
        {
            var target = orderedTargets[index];
            if (!IsCanonicalValue(target.TargetSubPhaseId)
                || !selectedIds.Add(target.TargetSubPhaseId)
                || !edgeTargets.Contains(target.TargetSubPhaseId)
                || !candidateTargetIds.Contains(target.TargetSubPhaseId)
                || !double.IsFinite(target.Confidence)
                || target.Confidence is < 0d or > 1d)
            {
                return false;
            }

            if (index > 0)
            {
                var previous = orderedTargets[index - 1];
                if (previous.Confidence < target.Confidence
                    || (previous.Confidence.Equals(target.Confidence)
                        && StringComparer.Ordinal.Compare(
                            previous.TargetSubPhaseId,
                            target.TargetSubPhaseId) >= 0))
                {
                    return false;
                }
            }

            selections[index] = new DynamicGraphEdgeTargetSelection(
                target.TargetSubPhaseId,
                target.Confidence,
                index);
        }

        var upstream = stableInput.UpstreamNodeOutput;
        if (upstream is null
            || upstream.UpstreamCandidateRefs is null
            || upstream.UpstreamCandidateRefs.Count == 0
            || !AreCanonicalRefs(upstream.UpstreamCandidateRefs)
            || upstream.UpstreamCandidateRefs.Any(reference => !IsCanonicalValue(reference))
            || !IsCanonicalValue(upstream.UpstreamOutputHash)
            || !IsCanonicalValue(upstream.ProjectionRule))
        {
            return false;
        }

        var handoff = new DynamicGraphEdgeTargetHandoff(
            stableInput.RunId,
            edge.EdgeId,
            edge.FromNodeId,
            domainTag,
            edge.Kind,
            edge.ParallelEligible,
            Array.AsReadOnly(selections.ToArray()),
            decision.KnowledgeEdge,
            ExpectedAuthoritySource,
            upstream.UpstreamCandidateRefs,
            upstream.UpstreamOutputHash,
            upstream.ProjectionRule);

        var outwardWarnings = BuildOutwardWarnings(warnings, decision.KnowledgeEdge);
        var verdict = decision.KnowledgeEdge ? AuthorityGapHandoffVerdict : TargetHandoffReadyVerdict;
        var nextAction = decision.KnowledgeEdge ? HandoffGapToL4Action : HandoffToL4Action;
        var humanResult = decision.KnowledgeEdge
            ? "Learned edge authority emitted a typed gap handoff; target execution was not performed."
            : "Learned edge authority selected target nodes; target execution was not performed.";

        var summary = new LogicalFlowSummary(
            SummaryId: $"{flowId}:edge-target-handoff",
            Tier: 3,
            BaselinePrimary: 0d,
            EnhancedPrimary: selections.Length,
            PrimaryMetricName: "selected_target_count",
            LowerIsBetter: false,
            Passed: true,
            ShouldScaleUp: false,
            NextAction: nextAction,
            ScalingSignal: 0d,
            ScalingSignalNote: "Inference handoff only; scaling was not evaluated.",
            Notes: "Learned edge authority produced a typed handoff; no target was executed and topology was not mutated.",
            BaselineTimeMs: 0d,
            EnhancedTimeMs: 0d);

        var metrics = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["authority_source"] = ExpectedAuthoritySource,
            ["authority_gap"] = decision.KnowledgeEdge.ToString().ToLowerInvariant(),
            ["authority_warning_count"] = outwardWarnings.Length.ToString(CultureInfo.InvariantCulture),
            ["edge_id"] = edge.EdgeId,
            ["edge_kind"] = edge.Kind.ToString(),
            ["parallel_eligible"] = edge.ParallelEligible.ToString().ToLowerInvariant(),
            ["target_count"] = selections.Length.ToString(CultureInfo.InvariantCulture),
            ["targets_executed"] = "false",
            ["topology_mutated"] = "false"
        };

        result = new LogicalFlowResult(
            FlowId: flowId,
            ScenarioId: scenarioId,
            OverallVerdict: verdict,
            FinalHumanResult: humanResult,
            Summary: summary,
            Metrics: new ReadOnlyDictionary<string, string>(metrics),
            EvidenceIds: evidenceIds.ToArray(),
            Warnings: outwardWarnings.ToArray())
        {
            TransitionSignals = [],
            EdgeTargetHandoff = handoff
        };

        return true;
    }

    private static bool HasExactContextMembership(
        ContextSignals? expectedContext,
        ContextSignals? actualContext)
    {
        if (expectedContext is null || actualContext is null)
            return false;

        var expectedPriorDomainSource = expectedContext.PriorDomains;
        var actualPriorDomainSource = actualContext.PriorDomains;
        var expectedCompletedSource = expectedContext.CompletedSubPhases;
        var actualCompletedSource = actualContext.CompletedSubPhases;
        var expectedActiveEdgeSource = expectedContext.ActiveKnowledgeEdges;
        var actualActiveEdgeSource = actualContext.ActiveKnowledgeEdges;
        if (expectedPriorDomainSource is null
            || actualPriorDomainSource is null
            || expectedCompletedSource is null
            || actualCompletedSource is null
            || expectedActiveEdgeSource is null
            || actualActiveEdgeSource is null)
        {
            return false;
        }

        var expectedPriorDomains = expectedPriorDomainSource.ToArray();
        var actualPriorDomains = actualPriorDomainSource.ToArray();
        var expectedCompleted = expectedCompletedSource.ToArray();
        var actualCompleted = actualCompletedSource.ToArray();
        var expectedActiveEdges = expectedActiveEdgeSource.ToArray();
        var actualActiveEdges = actualActiveEdgeSource.ToArray();

        var expectedDomainSet = expectedPriorDomains.ToHashSet();
        var actualDomainSet = actualPriorDomains.ToHashSet();
        var expectedCompletedSet = expectedCompleted.ToHashSet(StringComparer.Ordinal);
        var actualCompletedSet = actualCompleted.ToHashSet(StringComparer.Ordinal);
        var expectedActiveEdgeSet = expectedActiveEdges.ToHashSet(StringComparer.Ordinal);
        var actualActiveEdgeSet = actualActiveEdges.ToHashSet(StringComparer.Ordinal);

        return expectedPriorDomains.All(domain => Enum.IsDefined(typeof(Domain), domain))
               && actualPriorDomains.All(domain => Enum.IsDefined(typeof(Domain), domain))
               && expectedCompleted.All(IsCanonicalValue)
               && actualCompleted.All(IsCanonicalValue)
               && expectedActiveEdges.All(IsCanonicalValue)
               && actualActiveEdges.All(IsCanonicalValue)
               && expectedDomainSet.Count == expectedPriorDomains.Length
               && actualDomainSet.Count == actualPriorDomains.Length
               && expectedCompletedSet.Count == expectedCompleted.Length
               && actualCompletedSet.Count == actualCompleted.Length
               && expectedActiveEdgeSet.Count == expectedActiveEdges.Length
               && actualActiveEdgeSet.Count == actualActiveEdges.Length
               && expectedDomainSet.SetEquals(actualDomainSet)
               && expectedCompletedSet.SetEquals(actualCompletedSet)
               && expectedActiveEdgeSet.SetEquals(actualActiveEdgeSet);
    }

    private static bool HasExactPriorConfidenceMap(
        ContextSignals? expectedContext,
        ContextSignals? actualContext)
    {
        var expectedMap = expectedContext?.PriorConfidenceMap;
        var actualMap = actualContext?.PriorConfidenceMap;
        if (expectedMap is null
            || actualMap is null
            || expectedMap.Count != actualMap.Count)
        {
            return false;
        }

        foreach (var pair in expectedMap)
        {
            if (!actualMap.TryGetValue(pair.Key, out var actualValue)
                || !pair.Value.Equals(actualValue))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryGetCorrelatedCandidateTargetIds(
        IReadOnlyList<EdgeCandidate>? proposedCandidates,
        IReadOnlyList<EdgeCandidate>? normalizedCandidates,
        IReadOnlySet<string> edgeTargets,
        out HashSet<string> candidateTargetIds)
    {
        candidateTargetIds = new HashSet<string>(StringComparer.Ordinal);
        if (proposedCandidates is null
            || proposedCandidates.Count == 0
            || normalizedCandidates is null
            || normalizedCandidates.Count == 0)
        {
            return false;
        }

        foreach (var candidate in proposedCandidates)
        {
            if (candidate is null
                || !IsCanonicalValue(candidate.TargetSubPhaseId)
                || !edgeTargets.Contains(candidate.TargetSubPhaseId)
                || !candidateTargetIds.Add(candidate.TargetSubPhaseId))
            {
                return false;
            }
        }

        var normalizedTargetIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in normalizedCandidates)
        {
            if (candidate is null
                || !IsCanonicalValue(candidate.TargetSubPhaseId)
                || !edgeTargets.Contains(candidate.TargetSubPhaseId)
                || !normalizedTargetIds.Add(candidate.TargetSubPhaseId))
            {
                return false;
            }
        }

        return candidateTargetIds.SetEquals(normalizedTargetIds);
    }

    private static string[] BuildOutwardWarnings(
        IReadOnlyList<string> source,
        bool authorityGap)
    {
        var warnings = new List<string>(source.Count + (authorityGap ? 1 : 0));
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var warning in source)
        {
            var outwardWarning = warning.Contains(
                LegacyKnowledgeEdgeWarningMarker,
                StringComparison.OrdinalIgnoreCase)
                    ? ReservedWarningRedacted
                    : warning;

            if (seen.Add(outwardWarning))
                warnings.Add(outwardWarning);
        }

        if (authorityGap && seen.Add(AuthorityGapWarning))
            warnings.Add(AuthorityGapWarning);

        return warnings.ToArray();
    }

    // AreCanonicalRefs observes snapshot order without rewriting the learned authority input.
    private static bool AreCanonicalRefs(IReadOnlyList<string> refs)
    {
        for (var index = 1; index < refs.Count; index++)
        {
            if (StringComparer.Ordinal.Compare(refs[index - 1], refs[index]) >= 0)
                return false;
        }

        return true;
    }

    private static bool IsValidFailureEnvelope(string message, string code)
        => IsCanonicalValue(message)
           && IsCanonicalErrorCode(code);

    private static bool IsCanonicalErrorCode(string? code)
        => IsCanonicalValue(code)
           && code![0] is >= 'A' and <= 'Z'
           && code!.All(character => character is >= 'A' and <= 'Z'
                                             or >= '0' and <= '9'
                                             or '_');

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !value.Any(char.IsControl)
           && !ContainsLineBreak(value);

    private static bool ContainsLineBreak(string value)
        => value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) >= 0;

    private static bool IsNonFatalException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    // Failure keeps every guard and sanitized dependency error on the shared flow result envelope.
    private static OperationResult<LogicalFlowResult> Failure(string message, string code)
        => OperationResult<LogicalFlowResult>.Fail(message, code);
}
