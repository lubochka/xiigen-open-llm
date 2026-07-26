// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using AgentAlgorithms.Skills;
using DynamicDomains;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

// This orchestrator selects flows from topology-aware routing instead of relying on caller-supplied scenario ids.
public sealed class AdaptiveLogicalOrchestrator : ILogicalOrchestrator
{
    // This scenario id marks normal adaptive prompt routing.
    public const string ScenarioId = "adaptive-logical-orchestration";

    // This flow id labels synthetic knowledge-edge results when no executable flow exists.
    public const string KnowledgeEdgeFlowId = "knowledge-edge-missing-flow";

    // Keep adaptive branch expansion bounded so broad prompts do not train the whole lifecycle.
    public const int MaxAdaptiveBranchCount = 8;

    private const string LearnedEdgeAuthoritySource = RoutingAuthorityVocabulary.TrainedEdgeUnitRole;
    private const string LearnedEdgeProjectionRule = "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)";
    private const string TargetHandoffReadyVerdict = "TARGET_HANDOFF_READY";
    private const string AuthorityGapHandoffVerdict = "EDGE_AUTHORITY_GAP_HANDOFF";
    private const string HandoffToL4Action = "HANDOFF_TO_L4";
    private const string HandoffGapToL4Action = "HANDOFF_GAP_TO_L4";
    private const string HandoffScalingSignalNote = "Inference handoff only; scaling was not evaluated.";
    private const string HandoffSummaryNotes = "Learned edge authority produced a typed handoff; no target was executed and topology was not mutated.";
    private const string AuthorityGapHumanResult = "Learned edge authority emitted a typed gap handoff; target execution was not performed.";
    private const string TargetHandoffHumanResult = "Learned edge authority selected target nodes; target execution was not performed.";
    private const string ReturnLoopAuthoritySource = "topology_return_loop";
    private const string ReturnLoopHandoffReadyVerdict = "RETURN_LOOP_HANDOFF_READY";
    private const string ReturnLoopToL4Action = "HANDOFF_RETURN_LOOP_TO_L4";
    private const string ReturnLoopSummarySuffix = "return-loop-handoff";
    private const string ReturnLoopPrimaryMetricName = "correction_target_count";
    private const string ReturnLoopScalingSignalNote = "Deterministic correction handoff only; scaling was not evaluated.";
    private const string ReturnLoopSummaryNotes = "Topology-defined ReturnLoop produced a single typed correction handoff; no target was executed and topology was not mutated.";
    private const string ReturnLoopHumanResult = "Topology-defined ReturnLoop emitted its single correction target; target execution was not performed.";
    private const string MultipleTargetHandoffsCode = "DYNAMIC_GRAPH_L4_MULTIPLE_TARGET_HANDOFFS_UNSUPPORTED";
    private const string TargetHandoffSourceMismatchCode = "DYNAMIC_GRAPH_L4_TARGET_HANDOFF_SOURCE_MISMATCH";
    private const string TargetTopologyRequiredCode = "DYNAMIC_GRAPH_L4_TARGET_TOPOLOGY_REQUIRED";
    private const string TargetHandoffInvalidCode = "DYNAMIC_GRAPH_L4_TARGET_HANDOFF_INVALID";
    private const string TargetEdgeNotFoundCode = "DYNAMIC_GRAPH_L4_TARGET_EDGE_NOT_FOUND";
    private const string TargetEdgeAmbiguousCode = "DYNAMIC_GRAPH_L4_TARGET_EDGE_AMBIGUOUS";
    private const string TargetEdgeMismatchCode = "DYNAMIC_GRAPH_L4_TARGET_EDGE_MISMATCH";
    private const string TargetEdgeKindUnsupportedCode = "DYNAMIC_GRAPH_L4_TARGET_EDGE_KIND_UNSUPPORTED";
    private const string TargetCardinalityInvalidCode = "DYNAMIC_GRAPH_L4_TARGET_CARDINALITY_INVALID";
    private const string TargetNotRegisteredCode = "DYNAMIC_GRAPH_L4_TARGET_NOT_REGISTERED";
    private const string TargetFlowNotRegisteredCode = "DYNAMIC_GRAPH_L4_TARGET_FLOW_NOT_REGISTERED";
    private const string TargetExecutionFailedCode = "DYNAMIC_GRAPH_L4_TARGET_EXECUTION_FAILED";
    private const string TargetMetadataInvalidCode = "DYNAMIC_GRAPH_L4_TARGET_METADATA_INVALID";
    private const string TargetHandoffDepthExceededCode = "DYNAMIC_GRAPH_L4_TARGET_HANDOFF_DEPTH_EXCEEDED";
    private const string TargetContinuationRequiredCode = "DYNAMIC_GRAPH_L4_NONTERMINAL_TARGET_CONTINUATION_REQUIRED";
    private const string PromptCompilerResultRequiredCode = "DYNAMIC_GRAPH_L4_PROMPT_COMPILER_RESULT_REQUIRED";
    private const string PromptCompilerResultInvalidCode = "DYNAMIC_GRAPH_L4_PROMPT_COMPILER_RESULT_INVALID";
    private const string PromptCompilerResultIdentityMismatchCode = "DYNAMIC_GRAPH_L4_PROMPT_COMPILER_RESULT_IDENTITY_MISMATCH";
    private const string PromptCompilerExecutionFailedCode = "DYNAMIC_GRAPH_L4_PROMPT_COMPILER_EXECUTION_FAILED";
    private const string DependencyMetadataInvalidCode = "DYNAMIC_GRAPH_L4_DEPENDENCY_METADATA_INVALID";
    private const string DomainClassifierResultInvalidCode = "DYNAMIC_GRAPH_L4_DOMAIN_CLASSIFIER_RESULT_INVALID";
    private const string DomainClassifierResultIdentityMismatchCode = "DYNAMIC_GRAPH_L4_DOMAIN_CLASSIFIER_RESULT_IDENTITY_MISMATCH";
    private const string DomainClassifierExecutionFailedCode = "DYNAMIC_GRAPH_L4_DOMAIN_CLASSIFIER_EXECUTION_FAILED";
    private const string RouterExecutionFailedCode = "DYNAMIC_GRAPH_L4_ROUTER_EXECUTION_FAILED";
    private const string RouterResultRequiredCode = "DYNAMIC_GRAPH_L4_ROUTER_RESULT_REQUIRED";
    private const string RouterResultInvalidCode = "DYNAMIC_GRAPH_L4_ROUTER_RESULT_INVALID";
    private const string RouterResultIdentityMismatchCode = "DYNAMIC_GRAPH_L4_ROUTER_RESULT_IDENTITY_MISMATCH";
    private const string FlowExecutionFailedCode = "DYNAMIC_GRAPH_L4_FLOW_EXECUTION_FAILED";
    private const string FlowResultRequiredCode = "DYNAMIC_GRAPH_L4_FLOW_RESULT_REQUIRED";
    private const string FlowResultInvalidCode = "DYNAMIC_GRAPH_L4_FLOW_RESULT_INVALID";
    private const string FlowResultIdentityMismatchCode = "DYNAMIC_GRAPH_L4_FLOW_RESULT_IDENTITY_MISMATCH";
    private const string NodeOutputTopologySourceRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUT_TOPOLOGY_SOURCE_REQUIRED";
    private const string NodeOutputTopologyUnavailableCode = "DYNAMIC_GRAPH_NODE_OUTPUT_TOPOLOGY_UNAVAILABLE";
    private const string NodeOutputTopologySnapshotRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUT_TOPOLOGY_SNAPSHOT_REQUIRED";
    private const string NodeOutputTopologyRevisionInvalidCode = "DYNAMIC_GRAPH_NODE_OUTPUT_TOPOLOGY_REVISION_INVALID";
    private const string NodeOutputTopologyInvalidCode = "DYNAMIC_GRAPH_NODE_OUTPUT_TOPOLOGY_INVALID";
    private const string NodeOutputSourceExceptionCode = "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_EXCEPTION";
    private const string NodeOutputSourceResultRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_RESULT_REQUIRED";
    private const string NodeOutputSourceFailedCode = "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_FAILED";
    private const string NodeOutputInvocationSourceRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUT_INVOCATION_SOURCE_REQUIRED";
    private const string NodeOutputSnapshotRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUT_SNAPSHOT_REQUIRED";
    private const string NodeOutputRunIdInvalidCode = "DYNAMIC_GRAPH_NODE_OUTPUT_RUN_ID_INVALID";
    private const string NodeOutputRunIdMismatchCode = "DYNAMIC_GRAPH_NODE_OUTPUT_RUN_ID_MISMATCH";
    private const string NodeOutputRevisionInvalidCode = "DYNAMIC_GRAPH_NODE_OUTPUT_REVISION_INVALID";
    private const string NodeOutputRevisionMismatchCode = "DYNAMIC_GRAPH_NODE_OUTPUT_REVISION_MISMATCH";
    private const string NodeOutputSourceNodeInvalidCode = "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_NODE_INVALID";
    private const string NodeOutputSourceNodeNotFoundCode = "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_NODE_NOT_FOUND";
    private const string NodeOutputSourceNodeAmbiguousCode = "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_NODE_AMBIGUOUS";
    private const string NodeOutputCandidateSourceNodeMismatchCode = "DYNAMIC_GRAPH_NODE_OUTPUT_CANDIDATE_SOURCE_NODE_MISMATCH";
    private const string NodeOutputProjectionResultRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUT_PROJECTION_RESULT_REQUIRED";
    private const string NodeOutputInvocationScenarioInvalidCode = "DYNAMIC_GRAPH_NODE_OUTPUT_INVOCATION_SCENARIO_INVALID";
    private const string ProjectedEdgeFlowId = "dynamic-graph-projected-edge-flow";
    private const string ProjectedEdgeScenarioId = "dynamic-graph-projected-edge-traversal";
    private const string ProjectedEdgeTopologyRequiredCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_TOPOLOGY_REQUIRED";
    private const string ProjectedEdgeTopologyRevisionInvalidCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_TOPOLOGY_REVISION_INVALID";
    private const string ProjectedEdgeAuthorityConflictCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_AUTHORITY_CONFLICT";
    private const string ProjectedEdgeSourceBranchNotFoundCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_SOURCE_BRANCH_NOT_FOUND";
    private const string ProjectedEdgeSourceBranchAmbiguousCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_SOURCE_BRANCH_AMBIGUOUS";
    private const string ProjectedEdgeSourceBranchInvalidCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_SOURCE_BRANCH_INVALID";
    private const string ProjectedEdgeSourceBranchNotPassedCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_SOURCE_BRANCH_NOT_PASSED";
    private const string ProjectedEdgeAmbiguousCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_AMBIGUOUS";
    private const string ProjectedEdgeInvalidCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_INVALID";
    private const string ProjectedEdgeKindUnsupportedCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_KIND_UNSUPPORTED";
    private const string ProjectedEdgeContextInvalidCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_CONTEXT_INVALID";
    private const string ProjectedEdgeExecutionFailedCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_EXECUTION_FAILED";
    private const string ProjectedEdgeResultRequiredCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_RESULT_REQUIRED";
    private const string ProjectedEdgeHandoffRequiredCode = "DYNAMIC_GRAPH_PROJECTED_EDGE_HANDOFF_REQUIRED";
    // Transition chains are useful, but unbounded chains are cycles in disguise.
    public const int MaxTransitionDepth = 3;

    // The topology registry is the authority for known phases and sub-phases.
    private readonly ILogicalTopologyRegistry? _topology;

    // The prompt compiler converts raw human text into router input.
    private readonly ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>? _promptCompiler;

    // The router predicts one sub-phase in Phase A.
    private readonly ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>? _router;

    // The multi-router can fan out to multiple sub-phases in Phase B.
    private readonly ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>? _multiRouter;

    // The flow registry resolves executable flows for routed sub-phases.
    private readonly InMemoryLogicalFlowRegistry? _flows;

    // The unit registry is introduced in Phase A and read by flow factories in Phase C.
    private readonly InMemoryLogicalUnitRegistry? _units;

    // The scheduler receives knowledge-edge examples when capability learning is enabled.
    private readonly LearningEventScheduler? _learningEventScheduler;

    // The optional trainer can turn knowledge-edge evidence into a registered runtime capability.
    private readonly ILogicalCapabilityTrainer? _capabilityTrainer;

    // The optional read-only L4 source exposes one accepted dynamic-graph topology revision.
    private readonly IDynamicGraphTopologySource? _dynamicGraphTopologySource;

    // The optional invocation-scoped source supplies actual outputs produced by one graph node.
    private readonly IDynamicGraphNodeOutputSource? _dynamicGraphNodeOutputSource;

    // Distinguish the legacy no-source constructor from an explicit source overload invoked with null.
    private readonly bool _dynamicGraphNodeOutputSourceRequested;

    // Projection is fixed at the runtime boundary so callers cannot substitute edge-input identity logic.
    private readonly DeterministicDynamicGraphNodeOutputProjector _dynamicGraphNodeOutputProjector = new();

    // Create the adaptive orchestrator from its runtime registries and routing units.
    public AdaptiveLogicalOrchestrator(
        ILogicalTopologyRegistry? topology,
        ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>? promptCompiler,
        ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>? router,
        InMemoryLogicalFlowRegistry? flows,
        InMemoryLogicalUnitRegistry? units,
        ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>? multiRouter = null,
        LearningEventScheduler? learningEventScheduler = null,
        ILogicalCapabilityTrainer? capabilityTrainer = null,
        IDynamicGraphTopologySource? dynamicGraphTopologySource = null)
    {
        // Store the topology registry for route validation.
        _topology = topology;

        // Store the compiler used before sub-phase routing.
        _promptCompiler = promptCompiler;

        // Store the single-sub-phase router used in Phase A.
        _router = router;

        // Store the multi-sub-phase router when Phase B callers provide it.
        _multiRouter = multiRouter;

        // Store the runtime flow registry.
        _flows = flows;

        // Store the runtime unit registry for the later learned-unit phase.
        _units = units;

        // Store the optional scheduler used by the knowledge-edge learning loop.
        _learningEventScheduler = learningEventScheduler;

        // Store the optional capability trainer used by adaptive retry runs.
        _capabilityTrainer = capabilityTrainer;

        // Store only the read-only topology source; mutation ownership remains outside this consumer.
        _dynamicGraphTopologySource = dynamicGraphTopologySource;
    }

    // Opt-in node-output wiring preserves the original constructor while requiring an explicit source.
    public AdaptiveLogicalOrchestrator(
        ILogicalTopologyRegistry? topology,
        ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>? promptCompiler,
        ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>? router,
        InMemoryLogicalFlowRegistry? flows,
        InMemoryLogicalUnitRegistry? units,
        ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>? multiRouter,
        LearningEventScheduler? learningEventScheduler,
        ILogicalCapabilityTrainer? capabilityTrainer,
        IDynamicGraphTopologySource? dynamicGraphTopologySource,
        IDynamicGraphNodeOutputSource dynamicGraphNodeOutputSource)
        : this(
            topology,
            promptCompiler,
            router,
            flows,
            units,
            multiRouter,
            learningEventScheduler,
            capabilityTrainer,
            dynamicGraphTopologySource)
    {
        _dynamicGraphNodeOutputSourceRequested = true;
        _dynamicGraphNodeOutputSource = dynamicGraphNodeOutputSource;
    }

    // Run one assistant request through either explicit scenario selection or adaptive topology routing.
    public Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
        => RunCoreAsync(
            request,
            useProvidedNodeOutputs: false,
            providedTopologySnapshot: null,
            providedNodeOutputSource: null,
            cancellationToken);

    // Run one adaptive request against an exact caller-owned topology snapshot and invocation node-output source.
    public Task<OperationResult<LogicalOrchestratorResult>> RunWithInvocationNodeOutputsAsync(
        LogicalOrchestratorRequest request,
        DynamicGraphTopologyRuntimeSnapshot topologySnapshot,
        IDynamicGraphNodeOutputSource nodeOutputSource,
        CancellationToken cancellationToken = default)
        => RunCoreAsync(
            request,
            useProvidedNodeOutputs: true,
            topologySnapshot,
            nodeOutputSource,
            cancellationToken);

    // Preserve one execution pipeline while keeping provided invocation state method-local.
    private async Task<OperationResult<LogicalOrchestratorResult>> RunCoreAsync(
        LogicalOrchestratorRequest request,
        bool useProvidedNodeOutputs,
        DynamicGraphTopologyRuntimeSnapshot? providedTopologySnapshot,
        IDynamicGraphNodeOutputSource? providedNodeOutputSource,
        CancellationToken cancellationToken,
        DynamicGraphTransitionView? reusedInvocationTransitionView = null)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalOrchestratorResult>.Fail("Cancelled.", "CANCELLED");

        // Reject missing requests before reading request properties.
        if (request is null)
            return OperationResult<LogicalOrchestratorResult>.Fail("Logical orchestrator request is required.", "REQUEST_REQUIRED");

        // Reject empty user requests before flow selection.
        if (string.IsNullOrWhiteSpace(request.UserRequest))
            return OperationResult<LogicalOrchestratorResult>.Fail("User request cannot be empty.", "EMPTY_REQUEST");

        // Reject invalid construction through the same envelope used by runtime services.
        var constructionError = ValidateConstruction();
        if (constructionError is not null)
            return OperationResult<LogicalOrchestratorResult>.Fail(constructionError, "ORCHESTRATOR_NOT_CONFIGURED");

        if (useProvidedNodeOutputs)
        {
            if (!StringComparer.Ordinal.Equals(request.ScenarioId, ScenarioId))
            {
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "Dynamic graph node-output invocation requires the adaptive scenario.",
                    NodeOutputInvocationScenarioInvalidCode);
            }

            if (providedTopologySnapshot is null)
            {
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "Dynamic graph topology snapshot is required for node-output projection.",
                    NodeOutputTopologySnapshotRequiredCode);
            }

            if (providedNodeOutputSource is null)
            {
                return OperationResult<LogicalOrchestratorResult>.Fail(
                    "Dynamic graph node-output source is required.",
                    NodeOutputInvocationSourceRequiredCode);
            }
        }
        // An explicitly requested legacy source must fail through the result envelope before any injected unit runs.
        else if (_dynamicGraphNodeOutputSourceRequested && _dynamicGraphNodeOutputSource is null)
        {
            return OperationResult<LogicalOrchestratorResult>.Fail(
                "Dynamic graph node-output source is required.",
                NodeOutputInvocationSourceRequiredCode);
        }

        // Apply default flags when the caller did not provide custom flags.
        var flags = request.Flags ?? new LogicalExecutionFlags();

        // Explicit scenarios own their typed metadata contract and do not participate in adaptive prompt routing.
        // Dispatch before compiler/classifier dependency snapshots so opaque domain payloads reach only the selected flow.
        if (!IsAdaptiveScenario(request.ScenarioId))
            return await RunExplicitScenarioAsync(request, flags, cancellationToken);

        // Compile the raw prompt before domain classification so both the classifier and router consume the same semantic source of truth.
        var compiled = await CompilePromptAsync(request, flags, cancellationToken);

        // Preserve compiler failures at the orchestrator boundary.
        if (!compiled.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(compiled.ErrorMessage, compiled.ErrorCode);

        request = request with { Metadata = PromptDomainClassifier.AddCompiledPromptToMetadata(request.Metadata, compiled.Value!.Output) };

        // Classify the business domain from the compiled prompt so domain memory buckets use the same representation as routing.
        var domainClassification = await ClassifyDomainAsync(request, flags, compiled.Value.Output, cancellationToken);
        if (!domainClassification.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(domainClassification.ErrorMessage, domainClassification.ErrorCode);

        request = request with { Metadata = PromptDomainClassifier.AddToMetadata(request.Metadata, domainClassification.Value!) };

        // Snapshot the structural and dependency views exactly once for this adaptive invocation.
        var transitionView = reusedInvocationTransitionView is not null
            ? OperationResult<DynamicGraphTransitionView>.Ok(reusedInvocationTransitionView)
            : useProvidedNodeOutputs
                ? ReadProvidedTransitionView(
                    request.RunId,
                    providedTopologySnapshot!,
                    providedNodeOutputSource!)
                : ReadTransitionView(request.RunId);
        if (!transitionView.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(transitionView.ErrorMessage, transitionView.ErrorCode);

        var invocationFollowUpView = BuildInvocationFollowUpTransitionView(transitionView.Value!);

        // Route to one or more sub-phases.
        var decisions = await RouteAsync(
            request,
            flags,
            compiled.Value!.Output,
            transitionView.Value!.StructuralTransitions,
            cancellationToken);

        // Preserve routing failures at the orchestrator boundary.
        if (!decisions.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(decisions.ErrorMessage, decisions.ErrorCode);

        // Execute every routed branch.
        var branchResult = await ExecuteBranchesAsync(
            request,
            flags,
            decisions.Value!,
            transitionView.Value.DependencyTransitions,
            cancellationToken);

        // Preserve branch failures at the orchestrator boundary.
        if (!branchResult.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(branchResult.ErrorMessage, branchResult.ErrorCode);

        // Execute one invocation-projected L3 edge before the existing L4 handoff consumer runs.
        var projectedEdge = await ExecuteProjectedDynamicGraphEdgeAsync(
            request,
            flags,
            compiled.Value.Output,
            branchResult.Value!,
            transitionView.Value,
            cancellationToken);
        if (!projectedEdge.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(projectedEdge.ErrorMessage, projectedEdge.ErrorCode);

        // Consume one bounded chain of typed edge target handoffs after every initial branch has completed.
        var targetExecution = await ConsumeEdgeTargetHandoffAsync(
            request,
            flags,
            projectedEdge.Value!,
            transitionView.Value.Topology,
            cancellationToken);
        if (!targetExecution.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(targetExecution.ErrorMessage, targetExecution.ErrorCode);

        // Existing scheduling, transition, and aggregation stages consume only normalized flow results.
        var branches = targetExecution.Value!.Select(branch => branch.Result).ToArray();

        // Queue learning events for knowledge-edge branches when enabled.
        ScheduleKnowledgeEdges(flags, branches);

        // Follow any advisory transition signals emitted by completed flow nodes.
        var transitionFollowUp = await FollowTransitionSignalsAsync(
            request,
            flags,
            branches,
            useProvidedNodeOutputs,
            providedTopologySnapshot,
            providedNodeOutputSource,
            invocationFollowUpView,
            cancellationToken);
        if (!transitionFollowUp.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(transitionFollowUp.ErrorMessage, transitionFollowUp.ErrorCode);

        var aggregateBranches = branches.Concat(transitionFollowUp.Value!.Branches).ToArray();

        // Return the aggregate assistant-level result.
        var aggregateWarnings = transitionFollowUp.Value.Warnings
            .Concat(transitionView.Value.Warnings)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return OperationResult<LogicalOrchestratorResult>.Ok(BuildAggregateResult(request, flags, aggregateBranches, aggregateWarnings));
    }

    // Run one request and, when possible, generate the missing capability and retry the same request.
    public async Task<OperationResult<LogicalOrchestratorResult>> RunWithCapabilityGenerationAsync(LogicalOrchestratorRequest request, CancellationToken cancellationToken = default)
    {
        // Run the normal adaptive path first.
        var firstRun = await RunAsync(request, cancellationToken);
        if (!firstRun.IsSuccess)
            return firstRun;

        // Collect knowledge-edge branches from the first run.
        var knowledgeEdges = firstRun.Value!.BranchResults.Where(IsKnowledgeEdgeBranch).ToArray();
        if (knowledgeEdges.Length == 0)
            return firstRun;

        // Without a trainer, the honest result is still the visible knowledge edge.
        if (_capabilityTrainer is null)
            return firstRun;

        // Ask the trainer to register the missing flow or learned unit.
        var trainingDomain = TryReadDomainFromResult(firstRun.Value!, out var resultDomain)
            ? resultDomain
            : PromptDomainClassifier.Classify(request.UserRequest, request.Metadata).Domain;
        var compiledPrompt = new SemanticTeachingPromptCompiler().Compile(request.UserRequest);
        var training = await _capabilityTrainer.TrainAndRegisterAsync(new LogicalCapabilityTrainingRequest(
            request.RunId,
            request,
            knowledgeEdges,
            _topology!,
            _flows!,
            _units!)
        {
            Domain = trainingDomain,
            CompiledPrompt = compiledPrompt.IsSuccess ? compiledPrompt.Value : null
        }, cancellationToken);
        if (!training.IsSuccess || !training.Value!.CapabilityRegistered)
            return firstRun;

        // Retry with training evidence in metadata so downstream flows can inspect the generated capability.
        var retryMetadata = new Dictionary<string, object>(request.Metadata, StringComparer.Ordinal)
        {
            ["capability.training_result"] = training.Value,
            ["capability.retry_after_training"] = true
        };
        var retryRequest = request with { Metadata = retryMetadata };

        // Return the retry result if it succeeds; otherwise preserve the original visible knowledge edge.
        var retry = await RunAsync(retryRequest, cancellationToken);
        return retry.IsSuccess ? retry : firstRun;
    }

    // Resolve the active compiled domain classifier through the unit registry, falling back to the deterministic semantic compiler core.
    private async Task<OperationResult<PromptDomainClassification>> ClassifyDomainAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        SubPhaseRouterSourceDocument compiledPrompt,
        CancellationToken cancellationToken)
    {
        ILogicalUnit<DomainClassifierRequest, DomainClassification> classifier =
            new CompiledDomainClassifierUnit();
        var classifierUnitId = DomainClassifierUnit.Id;
        var classifierUnitRole = DomainClassifierUnit.Role;
        var classifierDomainTag = DomainClassifierUnit.DomainTag;
        if (flags.UseLearnedUnitWhenAvailable && _units is not null)
        {
            var learnedClassifier = _units.ResolveLatestEntry<DomainClassifierRequest, DomainClassification>(
                DomainClassifierUnit.Role,
                DomainClassifierUnit.DomainTag,
                LogicalUnitTrustState.Probationary);
            if (learnedClassifier.IsSuccess)
            {
                var learnedEntry = learnedClassifier.Value!;
                (classifier, classifierUnitId, classifierUnitRole, classifierDomainTag) = (
                    learnedEntry.Unit,
                    learnedEntry.Descriptor.UnitId,
                    learnedEntry.Descriptor.UnitRole,
                    learnedEntry.Descriptor.DomainTag);
            }
        }

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED");

        var hasOwnedClassifierMetadata = LogicalMetadataSnapshot.TryCreate(
            request.Metadata,
            out var classifierMetadataBase);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED");

        if (!hasOwnedClassifierMetadata)
        {
            return OperationResult<PromptDomainClassification>.Fail(
                "Dependency metadata is invalid.",
                DependencyMetadataInvalidCode);
        }

        // Give the classifier its own compiled-metadata view so it cannot mutate the canonical request metadata.
        var classifierMetadata = PromptDomainClassifier.AddCompiledPromptToMetadata(
            classifierMetadataBase,
            compiledPrompt);
        var classifierPredicateIds = new string[compiledPrompt.CompiledPredicateIds.Length];
        Array.Copy(
            compiledPrompt.CompiledPredicateIds,
            classifierPredicateIds,
            classifierPredicateIds.Length);
        var classifierRequest = new DomainClassifierRequest(
            compiledPrompt.CompiledText,
            Predicates: DomainClassifierUnit.EncodePredicateProfile(classifierPredicateIds),
            Metadata: classifierMetadata);

        var logicalUnitRequest = new LogicalUnitRequest<DomainClassifierRequest>(
            request.RunId,
            classifierUnitId,
            classifierUnitRole,
            classifierDomainTag,
            classifierRequest,
            flags,
            classifierMetadata);

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED");

        try
        {
            Task<OperationResult<LogicalUnitResult<DomainClassification>>>? resultTask =
                classifier.ExecuteAsync(logicalUnitRequest, cancellationToken);
            if (resultTask is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<PromptDomainClassification>.Fail(
                    "Domain classifier result is invalid.",
                    DomainClassifierResultInvalidCode);
            }

            OperationResult<LogicalUnitResult<DomainClassification>>? classified = await resultTask;
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED");

            if (classified is null)
            {
                return OperationResult<PromptDomainClassification>.Fail(
                    "Domain classifier result is invalid.",
                    DomainClassifierResultInvalidCode);
            }

            if (!classified.IsSuccess)
                return OperationResult<PromptDomainClassification>.Fail(classified.ErrorMessage, classified.ErrorCode);

            if (classified.Value is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<PromptDomainClassification>.Fail(
                    "Domain classifier result is invalid.",
                    DomainClassifierResultInvalidCode);
            }

            var hasMatchingIdentity =
                StringComparer.Ordinal.Equals(classified.Value.UnitId, logicalUnitRequest.UnitId)
                && StringComparer.Ordinal.Equals(classified.Value.UnitRole, logicalUnitRequest.UnitRole)
                && StringComparer.Ordinal.Equals(classified.Value.DomainTag, logicalUnitRequest.DomainTag);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED");

            if (!hasMatchingIdentity)
            {
                return OperationResult<PromptDomainClassification>.Fail(
                    "Domain classifier result identity does not match the request.",
                    DomainClassifierResultIdentityMismatchCode);
            }

            var hasValidOutput = TryGetValidDomainClassifierOutput(classified.Value, out var output);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED");

            if (!hasValidOutput)
            {
                return OperationResult<PromptDomainClassification>.Fail(
                    "Domain classifier result is invalid.",
                    DomainClassifierResultInvalidCode);
            }

            return OperationResult<PromptDomainClassification>.Ok(new PromptDomainClassification(
                output.Label,
                output.Confidence,
                output.Evidence,
                output.UsedContext));
        }
        catch (OperationCanceledException)
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<PromptDomainClassification>.Fail(
                    "Domain classifier execution failed unexpectedly.",
                    DomainClassifierExecutionFailedCode);
        }
        catch (Exception exception) when (IsNonFatalL4Exception(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<PromptDomainClassification>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<PromptDomainClassification>.Fail(
                    "Domain classifier execution failed unexpectedly.",
                    DomainClassifierExecutionFailedCode);
        }
    }

    private static bool TryGetValidDomainClassifierOutput(
        LogicalUnitResult<DomainClassification>? result,
        out DomainClassification output)
    {
        output = null!;

        if (result?.Output is not DomainClassification candidate)
            return false;

        var evidence = candidate.Evidence;
        if (evidence is null)
            return false;

        var evidenceSnapshot = new string[evidence.Length];
        Array.Copy(evidence, evidenceSnapshot, evidence.Length);

        var isValid = Enum.IsDefined(typeof(Domain), candidate.Label)
            && evidenceSnapshot.All(IsCanonicalL4Value)
            && double.IsFinite(result.Confidence)
            && result.Confidence >= 0.0d
            && result.Confidence <= 1.0d
            && double.IsFinite(candidate.Confidence)
            && candidate.Confidence >= 0.0d
            && candidate.Confidence <= 1.0d
            && result.Confidence.Equals(candidate.Confidence);
        if (!isValid)
        {
            return false;
        }

        output = new DomainClassification(
            candidate.Label,
            candidate.Confidence,
            evidenceSnapshot,
            candidate.UsedContext);
        return true;
    }

    // Resolve the active prompt compiler through the unit registry, falling back to the deterministic semantic compiler wrapper.
    private async Task<OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>> CompilePromptAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        CancellationToken cancellationToken)
    {
        var fallbackCompiler = _promptCompiler is PromptRoutingInputCompilerUnit promptCompiler
            ? promptCompiler.WithLogicalUnits(_units)
            : _promptCompiler!;
        var compiler = fallbackCompiler;
        var compilerUnitId = PromptRoutingInputCompilerUnit.Id;
        var compilerUnitRole = PromptRoutingInputCompilerUnit.Role;
        var compilerDomainTag = PromptRoutingInputCompilerUnit.DomainTag;
        if (flags.UseLearnedUnitWhenAvailable && _units is not null)
        {
            var learnedCompiler = _units.ResolveLatestEntry<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>(
                PromptRoutingInputCompilerUnit.Role,
                PromptRoutingInputCompilerUnit.DomainTag,
                LogicalUnitTrustState.Probationary);
            if (learnedCompiler.IsSuccess)
            {
                compiler = learnedCompiler.Value!.Unit;
                compilerUnitId = learnedCompiler.Value.Descriptor.UnitId;
                compilerUnitRole = learnedCompiler.Value.Descriptor.UnitRole;
                compilerDomainTag = learnedCompiler.Value.Descriptor.DomainTag;
            }
        }

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED");

        var hasOwnedCompilerMetadata = LogicalMetadataSnapshot.TryCreate(
            request.Metadata,
            out var compilerMetadata);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED");

        if (!hasOwnedCompilerMetadata)
        {
            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(
                "Dependency metadata is invalid.",
                DependencyMetadataInvalidCode);
        }

        var compilerInput = request with { Metadata = compilerMetadata };
        var compilerRequest = new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            compilerUnitId,
            compilerUnitRole,
            compilerDomainTag,
            compilerInput,
            flags,
            compilerMetadata);

        try
        {
            Task<OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>>? resultTask =
                compiler.ExecuteAsync(compilerRequest, cancellationToken);
            if (resultTask is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(
                    "Prompt compiler result is required.",
                    PromptCompilerResultRequiredCode);
            }

            var result = await resultTask;
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED");

            if (result is null || (result.IsSuccess && (result.Value is null || result.Value.Output is null)))
            {
                return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(
                    "Prompt compiler result is required.",
                    PromptCompilerResultRequiredCode);
            }

            // Preserve a dependency-declared failure exactly as returned.
            if (!result.IsSuccess)
                return result;

            if (!HasPromptCompilerCarrierIdentity(result.Value!, compilerRequest))
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(
                    "Prompt compiler result identity does not match the request.",
                    PromptCompilerResultIdentityMismatchCode);
            }

            var hasOwnedSnapshot = TrySnapshotPromptCompilerSuccess(result.Value!, compilerRequest, out var ownedResult);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED");

            if (!hasOwnedSnapshot)
            {
                return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(
                    "Prompt compiler result is invalid.",
                    PromptCompilerResultInvalidCode);
            }

            // Recheck cancellation after admission so a successful dependency never outruns caller cancellation.
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED");

            return OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Ok(ownedResult);
        }
        catch (OperationCanceledException)
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(
                    "Prompt compiler execution failed unexpectedly.",
                    PromptCompilerExecutionFailedCode);
        }
        catch (Exception exception) when (IsNonFatalL4Exception(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Fail(
                    "Prompt compiler execution failed unexpectedly.",
                    PromptCompilerExecutionFailedCode);
        }
    }

    private static bool HasPromptCompilerCarrierIdentity(
        LogicalUnitResult<SubPhaseRouterSourceDocument> result,
        LogicalUnitRequest<LogicalOrchestratorRequest> request)
        => StringComparer.Ordinal.Equals(result.UnitId, request.UnitId)
           && StringComparer.Ordinal.Equals(result.UnitRole, request.UnitRole)
           && StringComparer.Ordinal.Equals(result.DomainTag, request.DomainTag);

    private static bool TrySnapshotPromptCompilerSuccess(
        LogicalUnitResult<SubPhaseRouterSourceDocument> result,
        LogicalUnitRequest<LogicalOrchestratorRequest> request,
        out LogicalUnitResult<SubPhaseRouterSourceDocument> snapshot)
    {
        snapshot = null!;
        if (result.PredicateIds is null
            || result.DomainTags is null
            || result.EvidenceIds is null
            || result.Warnings is null
            || result.TransitionSignal is not null)
        {
            return false;
        }

        var predicateIds = new string[result.PredicateIds.Length];
        Array.Copy(result.PredicateIds, predicateIds, predicateIds.Length);
        var domainTags = new string[result.DomainTags.Length];
        Array.Copy(result.DomainTags, domainTags, domainTags.Length);
        var evidenceIds = new string[result.EvidenceIds.Length];
        Array.Copy(result.EvidenceIds, evidenceIds, evidenceIds.Length);
        var warnings = new string[result.Warnings.Length];
        Array.Copy(result.Warnings, warnings, warnings.Length);

        if (!TrySnapshotPromptCompilerDocument(result.Output, out var output)
            || !double.IsFinite(result.Confidence)
            || result.Confidence is < 0.0d or > 1.0d
            || !predicateIds.All(IsCanonicalL4Value)
            || !domainTags.All(IsCanonicalL4Value)
            || !evidenceIds.All(IsCanonicalL4Value)
            || !warnings.All(value => IsSafeL4FreeText(value, requireNonEmpty: true))
            || !predicateIds.SequenceEqual(output.CompiledPredicateIds, StringComparer.Ordinal)
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || !evidenceIds.Contains(output.DocumentId, StringComparer.Ordinal))
        {
            return false;
        }

        snapshot = new LogicalUnitResult<SubPhaseRouterSourceDocument>(
            result.UnitId,
            result.UnitRole,
            result.DomainTag,
            output,
            result.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Snapshot only the admitted top level and its direct flat segments; never recurse into dependency-owned graphs.
    private static bool TrySnapshotPromptCompilerDocument(
        SubPhaseRouterSourceDocument? source,
        out SubPhaseRouterSourceDocument snapshot)
    {
        snapshot = null!;
        if (!TrySnapshotFlatPromptCompilerDocument(source, out var topLevel))
            return false;

        SubPhaseRouterSourceDocument[]? segmentSnapshots = null;
        if (source!.SegmentDocuments is not null)
        {
            var directSegments = new SubPhaseRouterSourceDocument[source.SegmentDocuments.Length];
            Array.Copy(source.SegmentDocuments, directSegments, directSegments.Length);
            segmentSnapshots = new SubPhaseRouterSourceDocument[directSegments.Length];
            for (var index = 0; index < directSegments.Length; index++)
            {
                var segment = directSegments[index];
                if (segment is null
                    || segment.SegmentDocuments is not null
                    || !TrySnapshotFlatPromptCompilerDocument(segment, out segmentSnapshots[index]))
                {
                    return false;
                }
            }
        }

        snapshot = new SubPhaseRouterSourceDocument(
            topLevel.DocumentId,
            topLevel.SourcePath,
            topLevel.SourceTier,
            topLevel.LicenseOrAuthority,
            topLevel.DocumentType,
            topLevel.DocumentTitle,
            topLevel.CompiledText,
            topLevel.CompiledPredicateIds,
            topLevel.SourceStatus,
            topLevel.KnownFailureMode,
            topLevel.DomainLabel,
            segmentSnapshots);
        return true;
    }

    private static bool TrySnapshotFlatPromptCompilerDocument(
        SubPhaseRouterSourceDocument? source,
        out SubPhaseRouterSourceDocument snapshot)
    {
        snapshot = null!;
        if (source?.CompiledPredicateIds is null)
            return false;

        // Copy predicate ids before validating any dependency-owned document contents.
        var predicateIds = new string[source.CompiledPredicateIds.Length];
        Array.Copy(source.CompiledPredicateIds, predicateIds, predicateIds.Length);
        if (!IsCanonicalL4Value(source.DocumentId)
            || !IsCanonicalL4Value(source.SourcePath)
            || !IsCanonicalL4Value(source.SourceTier)
            || !IsCanonicalL4Value(source.LicenseOrAuthority)
            || !IsCanonicalL4Value(source.DocumentType)
            || !IsCanonicalL4Value(source.SourceStatus)
            || !IsSafeL4FreeText(source.DocumentTitle, requireNonEmpty: true)
            || !IsSafeL4FreeText(source.CompiledText, requireNonEmpty: true)
            || !IsSafeL4FreeText(source.KnownFailureMode, requireNonEmpty: true)
            || source.DomainLabel is null
            || (source.DomainLabel.Length > 0
                && !IsSafeL4FreeText(source.DomainLabel, requireNonEmpty: true))
            || !predicateIds.All(IsCanonicalL4Value))
        {
            return false;
        }

        snapshot = new SubPhaseRouterSourceDocument(
            source.DocumentId,
            source.SourcePath,
            source.SourceTier,
            source.LicenseOrAuthority,
            source.DocumentType,
            source.DocumentTitle,
            source.CompiledText,
            predicateIds,
            source.SourceStatus,
            source.KnownFailureMode,
            source.DomainLabel);
        return true;
    }

    // Run the compatibility path for callers that explicitly choose a scenario id.
    private async Task<OperationResult<LogicalOrchestratorResult>> RunExplicitScenarioAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        CancellationToken cancellationToken)
    {
        // Resolve the requested scenario through the runtime flow registry.
        var binding = _flows!.ResolveBindingByScenario(request.ScenarioId, new LogicalFlowFactoryRequest(request.RunId, flags, _units!, request.Metadata, _topology));

        // Preserve missing explicit scenarios as failures because the caller requested a specific scenario.
        if (!binding.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(binding.ErrorMessage, binding.ErrorCode);

        // Execute the explicitly selected flow without a routed sub-phase decision.
        var flowResult = await ExecuteBindingAsync(request, flags, binding.Value!, decision: null, completedBranches: [], cancellationToken);

        // Preserve flow failures at the orchestrator boundary.
        if (!flowResult.IsSuccess)
            return OperationResult<LogicalOrchestratorResult>.Fail(flowResult.ErrorMessage, flowResult.ErrorCode);

        // Explicit scenarios do not carry an authoritative adaptive routing decision for a learned target handoff.
        if (flowResult.Value!.EdgeTargetHandoff is not null)
        {
            return OperationResult<LogicalOrchestratorResult>.Fail(
                "Explicit scenario flows cannot emit dynamic graph target handoffs.",
                "DYNAMIC_GRAPH_L4_EXPLICIT_TARGET_HANDOFF_UNSUPPORTED");
        }

        // Build the assistant-level result from the selected flow result.
        return OperationResult<LogicalOrchestratorResult>.Ok(new LogicalOrchestratorResult(
            request.ScenarioId,
            flowResult.Value!.FlowId,
            flowResult.Value.OverallVerdict,
            flowResult.Value.FinalHumanResult,
            flags,
            flowResult.Value)
        {
            BranchResults = [flowResult.Value]
        });
    }

    // Route a compiled source document through the multi-router when available, otherwise the single router.
    private async Task<OperationResult<SubPhaseRoutingDecision[]>> RouteAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        SubPhaseRouterSourceDocument sourceDocument,
        CancellationToken cancellationToken)
        => await RouteAsync(request, flags, sourceDocument, BuildDynamicDomainTransitions(), cancellationToken);

    // Route against the invocation-local structural view so a dynamic snapshot is never re-read by helpers.
    private async Task<OperationResult<SubPhaseRoutingDecision[]>> RouteAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        SubPhaseRouterSourceDocument sourceDocument,
        DynamicDomainTransition[] structuralTransitions,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        // The router receives neither the compiler master document nor the request's canonical compiled metadata.
        var hasOwnedRouterSource = TrySnapshotPromptCompilerDocument(
            sourceDocument,
            out var routerSourceDocument);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        if (!hasOwnedRouterSource)
        {
            return OperationResult<SubPhaseRoutingDecision[]>.Fail(
                "Sub-phase router source document is invalid.",
                RouterResultInvalidCode);
        }

        var hasOwnedRouterMetadata = LogicalMetadataSnapshot.TryCreate(
            request.Metadata,
            out var routerMetadataBase);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        if (!hasOwnedRouterMetadata)
        {
            return OperationResult<SubPhaseRoutingDecision[]>.Fail(
                "Dependency metadata is invalid.",
                DependencyMetadataInvalidCode);
        }

        var routerMetadata = PromptDomainClassifier.AddCompiledPromptToMetadata(
            routerMetadataBase,
            routerSourceDocument);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        // Use the multi-router when it is configured.
        if (_multiRouter is not null)
        {
            var routerRequest = new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
                request.RunId,
                MultiSubPhaseRouterUnit.Id,
                MultiSubPhaseRouterUnit.Role,
                MultiSubPhaseRouterUnit.DomainTag,
                routerSourceDocument,
                flags,
                routerMetadata);

            var allowedDecisionIdentities = SnapshotAllowedMultiRouterDecisionIdentities(routerSourceDocument);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

            var routed = await ExecuteRouterDependencyAsync(_multiRouter, routerRequest, cancellationToken);

            // Preserve multi-router failures.
            if (!routed.IsSuccess)
                return OperationResult<SubPhaseRoutingDecision[]>.Fail(routed.ErrorMessage, routed.ErrorCode);

            var hasOwnedSnapshot = TrySnapshotRouterDecisions(routed.Value!.Output, out var ownedDecisions);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

            if (!hasOwnedSnapshot)
            {
                return OperationResult<SubPhaseRoutingDecision[]>.Fail(
                    "Sub-phase router result is invalid.",
                    RouterResultInvalidCode);
            }

            var hasMatchingIdentity = HasRouterCarrierIdentity(routed.Value, routerRequest)
                && ownedDecisions.All(decision => HasRouterDecisionIdentity(decision, allowedDecisionIdentities));
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

            if (!hasMatchingIdentity)
            {
                return OperationResult<SubPhaseRoutingDecision[]>.Fail(
                    "Sub-phase router result identity does not match the request.",
                    RouterResultIdentityMismatchCode);
            }

            var hasValidResult = IsValidRouterCarrierConfidence(routed.Value.Confidence)
                && ownedDecisions.All(IsValidRouterDecision)
                && (ownedDecisions.Length == 0
                    || routed.Value.Confidence.Equals(ownedDecisions.Min(decision => decision.Confidence)));
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

            if (!hasValidResult)
            {
                return OperationResult<SubPhaseRoutingDecision[]>.Fail(
                    "Sub-phase router result is invalid.",
                    RouterResultInvalidCode);
            }

            // Return routed decisions plus topology bridge branches when the graph shows a missing expert handoff.
            var expanded = ExpandRoutedDecisions(ownedDecisions, structuralTransitions);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

            return OperationResult<SubPhaseRoutingDecision[]>.Ok(expanded);
        }

        var expectedSingleDecisionIdentity = new RouterDecisionIdentity(
            routerSourceDocument.DocumentId,
            routerSourceDocument.SourceStatus);
        var singleRouterRequest = new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
            request.RunId,
            SubPhaseRouterUnit.Id,
            SubPhaseRouterUnit.Role,
            SubPhaseRouterUnit.DomainTag,
            routerSourceDocument,
            flags,
            routerMetadata);

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        var single = await ExecuteRouterDependencyAsync(_router!, singleRouterRequest, cancellationToken);

        // Preserve single-router failures.
        if (!single.IsSuccess)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail(single.ErrorMessage, single.ErrorCode);

        var hasOwnedDecision = TrySnapshotRouterDecision(single.Value!.Output, out var ownedDecision);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        if (!hasOwnedDecision)
        {
            return OperationResult<SubPhaseRoutingDecision[]>.Fail(
                "Sub-phase router result is invalid.",
                RouterResultInvalidCode);
        }

        var hasMatchingSingleIdentity = HasRouterCarrierIdentity(single.Value, singleRouterRequest)
            && HasRouterDecisionIdentity(ownedDecision, expectedSingleDecisionIdentity);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        if (!hasMatchingSingleIdentity)
        {
            return OperationResult<SubPhaseRoutingDecision[]>.Fail(
                "Sub-phase router result identity does not match the request.",
                RouterResultIdentityMismatchCode);
        }

        var hasValidSingleResult = IsValidRouterCarrierConfidence(single.Value.Confidence)
            && IsValidRouterDecision(ownedDecision)
            && single.Value.Confidence.Equals(ownedDecision.Confidence);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        if (!hasValidSingleResult)
        {
            return OperationResult<SubPhaseRoutingDecision[]>.Fail(
                "Sub-phase router result is invalid.",
                RouterResultInvalidCode);
        }

        // Return the single decision in branch form.
        var singleExpanded = ExpandRoutedDecisions([ownedDecision], structuralTransitions);
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<SubPhaseRoutingDecision[]>.Fail("Cancelled.", "CANCELLED");

        return OperationResult<SubPhaseRoutingDecision[]>.Ok(singleExpanded);
    }

    private static ReadOnlyCollection<RouterDecisionIdentity> SnapshotAllowedMultiRouterDecisionIdentities(
        SubPhaseRouterSourceDocument sourceDocument)
    {
        var identities = new List<RouterDecisionIdentity>
        {
            new(sourceDocument.DocumentId, sourceDocument.SourceStatus)
        };

        var segments = sourceDocument.SegmentDocuments;
        if (segments is { Length: > 1 })
        {
            var segmentSnapshot = new SubPhaseRouterSourceDocument[segments.Length];
            Array.Copy(segments, segmentSnapshot, segments.Length);
            foreach (var segment in segmentSnapshot)
            {
                if (segment is not null)
                    identities.Add(new RouterDecisionIdentity(segment.DocumentId, segment.SourceStatus));
            }
        }

        return Array.AsReadOnly(identities.ToArray());
    }

    private static async Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteRouterDependencyAsync<TOutput>(
        ILogicalUnit<SubPhaseRouterSourceDocument, TOutput> router,
        LogicalUnitRequest<SubPhaseRouterSourceDocument> routerRequest,
        CancellationToken cancellationToken)
        where TOutput : class
    {
        try
        {
            Task<OperationResult<LogicalUnitResult<TOutput>>>? resultTask =
                router.ExecuteAsync(routerRequest, cancellationToken);
            if (resultTask is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<LogicalUnitResult<TOutput>>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    "Sub-phase router result is required.",
                    RouterResultRequiredCode);
            }

            OperationResult<LogicalUnitResult<TOutput>>? result = await resultTask;
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalUnitResult<TOutput>>.Fail("Cancelled.", "CANCELLED");

            if (result is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<LogicalUnitResult<TOutput>>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    "Sub-phase router result is required.",
                    RouterResultRequiredCode);
            }

            if (!result.IsSuccess)
                return OperationResult<LogicalUnitResult<TOutput>>.Fail(result.ErrorMessage, result.ErrorCode);

            if (result.Value is null || result.Value.Output is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<LogicalUnitResult<TOutput>>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    "Sub-phase router result is required.",
                    RouterResultRequiredCode);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<LogicalUnitResult<TOutput>>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    "Sub-phase router execution failed unexpectedly.",
                    RouterExecutionFailedCode);
        }
        catch (Exception exception) when (IsNonFatalL4Exception(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<LogicalUnitResult<TOutput>>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    "Sub-phase router execution failed unexpectedly.",
                    RouterExecutionFailedCode);
        }
    }

    private static bool TrySnapshotRouterDecisions(
        SubPhaseRoutingDecision[]? decisions,
        out SubPhaseRoutingDecision[] ownedDecisions)
    {
        ownedDecisions = [];
        if (decisions is null)
            return false;

        var topLevelSnapshot = new SubPhaseRoutingDecision[decisions.Length];
        Array.Copy(decisions, topLevelSnapshot, decisions.Length);

        var snapshots = new SubPhaseRoutingDecision[topLevelSnapshot.Length];
        for (var index = 0; index < topLevelSnapshot.Length; index++)
        {
            if (!TrySnapshotRouterDecision(topLevelSnapshot[index], out snapshots[index]))
                return false;
        }

        ownedDecisions = snapshots;
        return true;
    }

    private static bool TrySnapshotRouterDecision(
        SubPhaseRoutingDecision? decision,
        out SubPhaseRoutingDecision ownedDecision)
    {
        ownedDecision = null!;
        if (decision is null || decision.EvidencePredicates is null)
            return false;

        var evidenceSnapshot = new string[decision.EvidencePredicates.Length];
        Array.Copy(decision.EvidencePredicates, evidenceSnapshot, evidenceSnapshot.Length);

        ownedDecision = new SubPhaseRoutingDecision(
            decision.DocumentId,
            decision.SourceStatus,
            decision.PredictedPhaseId,
            decision.PredictedSubPhaseId,
            decision.Confidence,
            decision.NearestWrongSubPhaseScore,
            evidenceSnapshot,
            decision.KnowledgeEdge,
            decision.FailureClassification)
        {
            AuthorityRole = decision.AuthorityRole
        };
        return true;
    }

    private static bool HasRouterCarrierIdentity<TOutput>(
        LogicalUnitResult<TOutput> result,
        LogicalUnitRequest<SubPhaseRouterSourceDocument> request)
        => StringComparer.Ordinal.Equals(result.UnitId, request.UnitId)
           && StringComparer.Ordinal.Equals(result.UnitRole, request.UnitRole)
           && StringComparer.Ordinal.Equals(result.DomainTag, request.DomainTag);

    private static bool HasRouterDecisionIdentity(
        SubPhaseRoutingDecision decision,
        RouterDecisionIdentity expectedIdentity)
        => StringComparer.Ordinal.Equals(decision.DocumentId, expectedIdentity.DocumentId)
           && StringComparer.Ordinal.Equals(decision.SourceStatus, expectedIdentity.SourceStatus);

    private static bool HasRouterDecisionIdentity(
        SubPhaseRoutingDecision decision,
        IReadOnlyList<RouterDecisionIdentity> allowedIdentities)
        => allowedIdentities.Any(identity =>
            StringComparer.Ordinal.Equals(decision.DocumentId, identity.DocumentId)
            && StringComparer.Ordinal.Equals(decision.SourceStatus, identity.SourceStatus));

    private static bool IsValidRouterCarrierConfidence(double confidence)
        => double.IsFinite(confidence)
           && confidence is >= 0.0d and <= 1.0d;

    private static bool IsValidRouterDecision(SubPhaseRoutingDecision decision)
        => IsCanonicalL4Value(decision.DocumentId)
           && IsCanonicalL4Value(decision.SourceStatus)
           && IsCanonicalL4Value(decision.PredictedPhaseId)
           && IsCanonicalL4Value(decision.PredictedSubPhaseId)
           && IsCanonicalL4Value(decision.FailureClassification)
           && StringComparer.Ordinal.Equals(
               decision.AuthorityRole,
               RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole)
           && double.IsFinite(decision.Confidence)
           && decision.Confidence is >= 0.0d and <= 1.0d
           && double.IsFinite(decision.NearestWrongSubPhaseScore)
           && decision.NearestWrongSubPhaseScore is >= 0.0d and <= 1.0d
           && decision.EvidencePredicates.All(IsCanonicalL4Value);

    private readonly record struct RouterDecisionIdentity(string DocumentId, string SourceStatus);

    private async Task<OperationResult<TransitionFollowUpResult>> FollowTransitionSignalsAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        LogicalFlowResult[] branches,
        bool useProvidedNodeOutputs,
        DynamicGraphTopologyRuntimeSnapshot? providedTopologySnapshot,
        IDynamicGraphNodeOutputSource? providedNodeOutputSource,
        DynamicGraphTransitionView invocationTransitionView,
        CancellationToken cancellationToken)
    {
        var signals = branches.SelectMany(branch => branch.TransitionSignals).ToArray();
        if (signals.Length == 0)
            return OperationResult<TransitionFollowUpResult>.Ok(new TransitionFollowUpResult([], []));

        var warnings = new List<string>();
        var transitionBranches = new List<LogicalFlowResult>();
        var currentDepth = ReadTransitionDepth(request.Metadata);
        var contextPacket = flags.UseContextCarryForward
            ? BuildContextPacket(request.RunId, branches)
            : null;

        foreach (var signal in signals.Select((value, index) => new { Value = value, Index = index + 1 }))
        {
            if (currentDepth >= MaxTransitionDepth)
            {
                warnings.Add($"Transition depth limit reached. Signal from {signal.Value.SourceNodeId} skipped.");
                continue;
            }

            var transitionMetadata = new Dictionary<string, object>(request.Metadata, StringComparer.Ordinal)
            {
                ["transition.depth"] = currentDepth + 1,
                ["transition.source_node_id"] = signal.Value.SourceNodeId,
                ["transition.source_sub_phase_id"] = signal.Value.SourceSubPhaseId,
                ["transition.evidence_ids"] = signal.Value.EvidenceIds
            };
            if (contextPacket is not null)
            {
                transitionMetadata["context.packet"] = contextPacket;
                transitionMetadata[ContextPacketReader.ContextSignalsMetadataKey] = BuildContextSignals(contextPacket);
            }

            var classificationRequest = new LogicalOrchestratorRequest(
                $"{request.RunId}-transition-{signal.Index}",
                ScenarioId,
                signal.Value.TransitionPrompt,
                flags,
                transitionMetadata);

            var compiledTransition = await CompilePromptAsync(classificationRequest, flags, cancellationToken);
            if (!compiledTransition.IsSuccess)
                return OperationResult<TransitionFollowUpResult>.Fail(compiledTransition.ErrorMessage, compiledTransition.ErrorCode);

            classificationRequest = classificationRequest with
            {
                Metadata = PromptDomainClassifier.AddCompiledPromptToMetadata(classificationRequest.Metadata, compiledTransition.Value!.Output)
            };

            var classified = await ClassifyDomainAsync(classificationRequest, flags, compiledTransition.Value.Output, cancellationToken);
            if (!classified.IsSuccess)
                return OperationResult<TransitionFollowUpResult>.Fail(classified.ErrorMessage, classified.ErrorCode);

            if (classified.Value!.Confidence < 0.40)
            {
                warnings.Add($"Transition signal from {signal.Value.SourceNodeId} classified with low confidence ({classified.Value.Confidence:0.00}). Suggested domain: {signal.Value.SuggestedDomain}. Classified domain: {classified.Value.Domain}. Transition skipped.");
                continue;
            }

            if (classified.Value.Domain != signal.Value.SuggestedDomain)
                warnings.Add($"Domain mismatch on transition from {signal.Value.SourceNodeId}. Suggested: {signal.Value.SuggestedDomain}. Classified: {classified.Value.Domain}. Routing to classified domain {classified.Value.Domain}.");

            transitionMetadata = new Dictionary<string, object>(
                PromptDomainClassifier.AddToMetadata(classificationRequest.Metadata, classified.Value),
                StringComparer.Ordinal);

            var transitionRequest = classificationRequest with { Metadata = transitionMetadata };
            var transitionRun = await RunCoreAsync(
                transitionRequest,
                useProvidedNodeOutputs,
                providedTopologySnapshot,
                providedNodeOutputSource,
                cancellationToken,
                invocationTransitionView);
            if (!transitionRun.IsSuccess)
                return OperationResult<TransitionFollowUpResult>.Fail(transitionRun.ErrorMessage, transitionRun.ErrorCode);

            transitionBranches.AddRange(transitionRun.Value!.BranchResults);
            warnings.AddRange(transitionRun.Value.Warnings);
        }

        return OperationResult<TransitionFollowUpResult>.Ok(new TransitionFollowUpResult(transitionBranches.ToArray(), warnings.Distinct(StringComparer.Ordinal).ToArray()));
    }

    // Execute every branch either concurrently or in topology dependency order.
    private async Task<OperationResult<RoutedLogicalFlowResult[]>> ExecuteBranchesAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        SubPhaseRoutingDecision[] decisions,
        CancellationToken cancellationToken)
        => await ExecuteBranchesAsync(request, flags, decisions, BuildDynamicDomainTransitions(), cancellationToken);

    // Execute against the invocation-local dependency view; structural-only parallel edges cannot serialize branches.
    private async Task<OperationResult<RoutedLogicalFlowResult[]>> ExecuteBranchesAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        SubPhaseRoutingDecision[] decisions,
        DynamicDomainTransition[] dependencyTransitions,
        CancellationToken cancellationToken)
    {
        // Reject an empty route array.
        if (decisions.Length == 0)
            return OperationResult<RoutedLogicalFlowResult[]>.Fail("No routed sub-phases were produced.", "NO_ROUTED_SUB_PHASES");

        // Order decisions by declared topology transitions when any dependency exists.
        var orderedDecisions = OrderByTransitions(decisions, flags, dependencyTransitions);

        // Run independent branches concurrently.
        if (!HasTransitionDependency(decisions, flags, dependencyTransitions))
        {
            // Start every branch together.
            var tasks = orderedDecisions.Select(decision => ExecuteBranchAsync(request, flags, decision, completedBranches: [], cancellationToken)).ToArray();

            // Await all branch results.
            var parallelResults = await Task.WhenAll(tasks);

            // Preserve first branch failure, if any.
            var failure = parallelResults.FirstOrDefault(result => !result.IsSuccess);
            if (failure is not null && !failure.IsSuccess)
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(failure.ErrorMessage, failure.ErrorCode);

            // Return branch values in the deterministic route order.
            return OperationResult<RoutedLogicalFlowResult[]>.Ok(parallelResults.Select(result => result.Value!).ToArray());
        }

        // Run dependent branches sequentially in topology order.
        var sequentialResults = new List<RoutedLogicalFlowResult>();
        foreach (var decision in orderedDecisions)
        {
            // Execute one branch.
            var branch = await ExecuteBranchAsync(
                request,
                flags,
                decision,
                sequentialResults.Select(result => result.Result).ToArray(),
                cancellationToken);

            // Preserve branch failure.
            if (!branch.IsSuccess)
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(branch.ErrorMessage, branch.ErrorCode);

            // Store the completed branch.
            sequentialResults.Add(branch.Value!);
        }

        // Return every branch result.
        return OperationResult<RoutedLogicalFlowResult[]>.Ok(sequentialResults.ToArray());
    }

    // Execute the single outgoing L3 edge selected by an invocation-owned node-output projection.
    private async Task<OperationResult<RoutedLogicalFlowResult[]>> ExecuteProjectedDynamicGraphEdgeAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        SubPhaseRouterSourceDocument compiledSource,
        RoutedLogicalFlowResult[] initialBranches,
        DynamicGraphTransitionView transitionView,
        CancellationToken cancellationToken)
    {
        var projection = transitionView.NodeOutputProjection;
        if (projection is null)
            return OperationResult<RoutedLogicalFlowResult[]>.Ok(initialBranches);

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<RoutedLogicalFlowResult[]>.Fail("Cancelled.", "CANCELLED");

        try
        {
            // Projection and topology are one invocation-local view; this path never rereads or mutates topology.
            var topology = transitionView.Topology;
            if (topology is null)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Dynamic graph topology is required for projected edge execution.",
                    ProjectedEdgeTopologyRequiredCode);
            }

            if (transitionView.TopologyRevision is not > 0)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Dynamic graph topology revision is invalid for projected edge execution.",
                    ProjectedEdgeTopologyRevisionInvalidCode);
            }

            // An initial handoff is a second authority source and cannot be silently preferred or replaced.
            if (initialBranches.Any(branch => branch?.Result?.EdgeTargetHandoff is not null))
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Initial branches already contain a dynamic graph target handoff.",
                    ProjectedEdgeAuthorityConflictCode);
            }

            var sourceMatches = initialBranches
                .Select((branch, index) => new { Branch = branch, Index = index })
                .Where(item => item.Branch?.Decision is not null
                               && StringComparer.Ordinal.Equals(
                                   item.Branch.Decision.PredictedSubPhaseId,
                                   projection.SourceNodeId))
                .ToArray();
            if (sourceMatches.Length == 0)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected edge source branch was not found.",
                    ProjectedEdgeSourceBranchNotFoundCode);
            }

            if (sourceMatches.Length > 1)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected edge source branch is ambiguous.",
                    ProjectedEdgeSourceBranchAmbiguousCode);
            }

            var sourceIndex = sourceMatches[0].Index;
            var sourceBranch = sourceMatches[0].Branch;
            if (!TrySnapshotRoutedLogicalFlowResult(sourceBranch, out var stableSource)
                || !IsValidProjectedEdgeSourceSnapshot(stableSource))
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected edge source branch is invalid.",
                    ProjectedEdgeSourceBranchInvalidCode);
            }

            if (!stableSource.Result.Summary.Passed
                || stableSource.Decision.KnowledgeEdge
                || IsKnowledgeEdgeBranch(stableSource.Result)
                || !StringComparer.Ordinal.Equals(stableSource.Result.OverallVerdict, "PASS"))
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected edge source branch did not pass.",
                    ProjectedEdgeSourceBranchNotPassedCode);
            }

            // Cardinality is authority: no enumeration order or First fallback may select among edges.
            var outgoingEdges = topology.ListEdgesFrom(projection.SourceNodeId);
            if (outgoingEdges.Length == 0)
                return OperationResult<RoutedLogicalFlowResult[]>.Ok(initialBranches);

            if (outgoingEdges.Length > 1)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected edge source has multiple outgoing edges.",
                    ProjectedEdgeAmbiguousCode);
            }

            var edge = outgoingEdges[0];
            if (edge is null
                || !IsCanonicalL4Value(edge.EdgeId)
                || !IsCanonicalL4Value(edge.FromNodeId)
                || !StringComparer.Ordinal.Equals(edge.FromNodeId, projection.SourceNodeId))
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected dynamic graph edge is invalid.",
                    ProjectedEdgeInvalidCode);
            }

            if (edge.Kind is not GraphEdgeKind.FanOut
                and not GraphEdgeKind.ManyToMany
                and not GraphEdgeKind.MultiOutput
                and not GraphEdgeKind.ReturnLoop)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected dynamic graph edge kind is unsupported.",
                    ProjectedEdgeKindUnsupportedCode);
            }

            var context = ContextSignals.Empty;
            if (flags.UseContextCarryForward)
            {
                var initialResults = new LogicalFlowResult[initialBranches.Length];
                for (var index = 0; index < initialBranches.Length; index++)
                {
                    if (initialBranches[index]?.Result is null)
                    {
                        return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                            "Projected edge context is invalid.",
                            ProjectedEdgeContextInvalidCode);
                    }

                    initialResults[index] = initialBranches[index].Result;
                }

                if (!LogicalMetadataSnapshot.TryCreate(initialResults, out var completedSnapshots))
                {
                    return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                        "Projected edge context is invalid.",
                        ProjectedEdgeContextInvalidCode);
                }

                context = BuildContextSignals(BuildContextPacket(request.RunId, completedSnapshots));
            }

            if (!TrySnapshotPromptCompilerDocument(compiledSource, out var projectedEdgeSource))
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected edge source document is invalid.",
                    ProjectedEdgeContextInvalidCode);
            }

            DynamicGraphEdgeFlow flow;
            if (edge.Kind == GraphEdgeKind.ReturnLoop)
            {
                // Structural correction cannot even construct a learned resolver or authority pipeline.
                flow = new DynamicGraphEdgeFlow(topology, null, null);
            }
            else
            {
                // Learned kinds keep the existing concrete L3 authority components and behavior.
                var candidateProvider = new DeterministicRegistryEdgeCandidateProvider(_topology);
                var inputMaterializer = new DeterministicDynamicGraphEdgeAuthorityInputMaterializer();
                var resolver = new RegistryDynamicGraphEdgeAuthorityResolver(_units!);
                var pipeline = new DynamicGraphEdgeAuthorityExecutionPipeline(
                    candidateProvider,
                    inputMaterializer,
                    resolver);
                flow = new DynamicGraphEdgeFlow(topology, null, null, pipeline);
            }

            var traversal = new EdgeTraversalInput(
                request.RunId,
                edge.EdgeId,
                projectedEdgeSource,
                context,
                projection);
            var flowRequest = new LogicalFlowRequest<EdgeTraversalInput>(
                request.RunId,
                ProjectedEdgeFlowId,
                ProjectedEdgeScenarioId,
                traversal,
                flags with { },
                new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(StringComparer.Ordinal)));

            if (cancellationToken.IsCancellationRequested)
                return OperationResult<RoutedLogicalFlowResult[]>.Fail("Cancelled.", "CANCELLED");

            // DynamicGraphEdgeFlow emits, but never consumes, the kind-specific typed handoff.
            var executed = await flow.ExecuteAsync(flowRequest, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<RoutedLogicalFlowResult[]>.Fail("Cancelled.", "CANCELLED");

            if (executed is null)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected dynamic graph edge result is required.",
                    ProjectedEdgeResultRequiredCode);
            }

            if (!executed.IsSuccess)
            {
                return !StringComparer.Ordinal.Equals(executed.ErrorCode, "CANCELLED")
                       && IsValidProjectedEdgeFailureEnvelope(executed.ErrorMessage, executed.ErrorCode)
                    ? OperationResult<RoutedLogicalFlowResult[]>.Fail(executed.ErrorMessage, executed.ErrorCode)
                    : OperationResult<RoutedLogicalFlowResult[]>.Fail(
                        "Projected dynamic graph edge execution failed.",
                        ProjectedEdgeExecutionFailedCode);
            }

            if (executed.Value is null)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected dynamic graph edge result is required.",
                    ProjectedEdgeResultRequiredCode);
            }

            if (executed.Value.EdgeTargetHandoff is null)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Projected dynamic graph edge handoff is required.",
                    ProjectedEdgeHandoffRequiredCode);
            }

            // Associate the handoff-producing branch with the authority that actually produced that handoff.
            var handoffDecision = AssociateDecisionWithHandoffAuthority(stableSource.Decision, edge.Kind);

            // Preserve the original source branch and insert the edge result immediately after it.
            var augmented = new RoutedLogicalFlowResult[initialBranches.Length + 1];
            Array.Copy(initialBranches, 0, augmented, 0, sourceIndex + 1);
            augmented[sourceIndex + 1] = new RoutedLogicalFlowResult(handoffDecision, executed.Value);
            Array.Copy(
                initialBranches,
                sourceIndex + 1,
                augmented,
                sourceIndex + 2,
                initialBranches.Length - sourceIndex - 1);
            return OperationResult<RoutedLogicalFlowResult[]>.Ok(augmented);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<RoutedLogicalFlowResult[]>.Fail("Cancelled.", "CANCELLED");
        }
        catch (Exception exception) when (IsNonFatalL4Exception(exception))
        {
            return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                "Projected dynamic graph edge execution failed.",
                ProjectedEdgeExecutionFailedCode);
        }
    }

    // Validate a completed source envelope before learned edge authority can observe it.
    private static bool IsValidProjectedEdgeSourceSnapshot(RoutedLogicalFlowResult source)
    {
        var decision = source.Decision;
        var result = source.Result;
        var summary = result.Summary;
        return IsCanonicalL4Value(decision.DocumentId)
            && IsCanonicalL4Value(decision.SourceStatus)
            && IsCanonicalL4Value(decision.PredictedPhaseId)
            && IsCanonicalL4Value(decision.PredictedSubPhaseId)
            && IsCanonicalL4Value(decision.FailureClassification)
            && double.IsFinite(decision.Confidence)
            && decision.Confidence is >= 0d and <= 1d
            && double.IsFinite(decision.NearestWrongSubPhaseScore)
            && decision.NearestWrongSubPhaseScore is >= 0d and <= 1d
            && decision.EvidencePredicates.All(IsCanonicalL4Value)
            && result.EdgeTargetHandoff is null
            && IsCanonicalL4Value(result.FlowId)
            && IsCanonicalL4Value(result.ScenarioId)
            && IsCanonicalL4Value(result.OverallVerdict)
            && IsSafeL4FreeText(result.FinalHumanResult, requireNonEmpty: true)
            && IsCanonicalL4Value(summary.SummaryId)
            && summary.Tier >= 0
            && IsCanonicalL4Value(summary.PrimaryMetricName)
            && IsCanonicalL4Value(summary.NextAction)
            && IsSafeL4FreeText(summary.ScalingSignalNote, requireNonEmpty: false)
            && IsSafeL4FreeText(summary.Notes, requireNonEmpty: false)
            && double.IsFinite(summary.BaselinePrimary)
            && double.IsFinite(summary.EnhancedPrimary)
            && double.IsFinite(summary.ScalingSignal)
            && double.IsFinite(summary.BaselineTimeMs)
            && double.IsFinite(summary.EnhancedTimeMs)
            && result.Metrics.All(metric =>
                IsCanonicalL4Value(metric.Key)
                && IsSafeL4FreeText(metric.Value, requireNonEmpty: false))
            && result.EvidenceIds.All(IsCanonicalL4Value)
            && result.Warnings.All(value => IsSafeL4FreeText(value, requireNonEmpty: false))
            && result.TransitionSignals.Length == 0;
    }

    private static bool IsValidProjectedEdgeFailureEnvelope(string message, string code)
        => IsCanonicalL4Value(message)
           && IsCanonicalProjectedEdgeErrorCode(code);

    private static bool IsCanonicalProjectedEdgeErrorCode(string? code)
        => IsCanonicalL4Value(code)
           && code![0] is >= 'A' and <= 'Z'
           && code.All(character => character is >= 'A' and <= 'Z'
                                          or >= '0' and <= '9'
                                          or '_');

    // Execute one routed branch and return a real flow result or a synthetic knowledge edge.
    private async Task<OperationResult<RoutedLogicalFlowResult>> ExecuteBranchAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        SubPhaseRoutingDecision decision,
        IReadOnlyList<LogicalFlowResult> completedBranches,
        CancellationToken cancellationToken)
    {
        // Low-confidence routing becomes a visible knowledge edge instead of invented flow selection.
        if (decision.KnowledgeEdge)
        {
            return OperationResult<RoutedLogicalFlowResult>.Ok(new RoutedLogicalFlowResult(
                decision,
                BuildKnowledgeEdgeFlowResult(request, decision, "routing confidence below floor", missingFlow: false)));
        }

        // Verify the predicted sub-phase exists in the topology registry.
        var topologySubPhase = _topology!.ResolveSubPhase(decision.PredictedSubPhaseId);
        if (!topologySubPhase.IsSuccess)
        {
            return OperationResult<RoutedLogicalFlowResult>.Ok(new RoutedLogicalFlowResult(
                decision,
                BuildKnowledgeEdgeFlowResult(request, decision, "routed sub-phase is not registered in topology", missingFlow: true)));
        }

        // Resolve an executable flow for the routed sub-phase.
        var binding = _flows!.ResolveBindingBySubPhase(decision.PredictedSubPhaseId, new LogicalFlowFactoryRequest(request.RunId, flags, _units!, request.Metadata, _topology));
        if (!binding.IsSuccess)
        {
            return OperationResult<RoutedLogicalFlowResult>.Ok(new RoutedLogicalFlowResult(
                decision,
                BuildKnowledgeEdgeFlowResult(request, decision, "no logical flow registered for routed sub-phase", missingFlow: true)));
        }

        // Execute the selected flow.
        var executed = await ExecuteBindingAsync(request, flags, binding.Value!, decision, completedBranches, cancellationToken);
        return executed.IsSuccess
            ? OperationResult<RoutedLogicalFlowResult>.Ok(new RoutedLogicalFlowResult(decision, executed.Value!))
            : OperationResult<RoutedLogicalFlowResult>.Fail(executed.ErrorMessage, executed.ErrorCode);
    }

    // Consume one typed edge target handoff per layer after the complete current branch set is available.
    private async Task<OperationResult<RoutedLogicalFlowResult[]>> ConsumeEdgeTargetHandoffAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        RoutedLogicalFlowResult[] sourceBranches,
        DynamicGraphTopology? topology,
        CancellationToken cancellationToken,
        int handoffDepth = 0)
    {
        var handoffIndexes = sourceBranches
            .Select((branch, index) => new { Branch = branch, Index = index })
            .Where(item => item.Branch.Result.EdgeTargetHandoff is not null)
            .ToArray();
        if (handoffIndexes.Length == 0)
            return OperationResult<RoutedLogicalFlowResult[]>.Ok(sourceBranches);

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<RoutedLogicalFlowResult[]>.Fail("Cancelled.", "CANCELLED");

        // More than one source handoff has no deterministic cross-branch authority policy in this runtime.
        if (handoffIndexes.Length > 1)
        {
            return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                "Multiple dynamic graph target handoffs are not supported in one adaptive run.",
                MultipleTargetHandoffsCode);
        }

        // A bounded chain may consume at most one handoff per layer; another layer fails before any dependency access.
        if (handoffDepth >= MaxTransitionDepth)
        {
            return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                "Dynamic graph target handoff depth exceeds the configured limit.",
                TargetHandoffDepthExceededCode);
        }

        var sourceIndex = handoffIndexes[0].Index;
        var source = handoffIndexes[0].Branch;
        ValidatedEdgeHandoff validated;
        RoutedLogicalFlowResult[] stableSourceBranches;
        RoutedLogicalFlowResult pendingSource;
        ResolvedEdgeTarget[] targets;
        try
        {
            // Pure validation and deep ownership precede logical registry and factory access.
            var validation = ValidateAndSnapshotEdgeHandoff(request, source, topology);
            if (!validation.IsSuccess)
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(validation.ErrorMessage, validation.ErrorCode);
            validated = validation.Value!;

            if (!TrySnapshotSourceBranches(sourceBranches, sourceIndex, validated.Source, out stableSourceBranches))
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    "Dynamic graph source branch set is invalid.",
                    TargetHandoffInvalidCode);
            }

            if (validated.OrderedTargets.Length == 0)
            {
                var finalGapSource = BuildHandoffSourceSnapshot(validated.Source, selectedTargetCount: 0, executionCompleted: true);
                return OperationResult<RoutedLogicalFlowResult[]>.Ok(
                    InsertTargetResults(stableSourceBranches, sourceIndex, finalGapSource, [], []));
            }

            // Pending context is an independent snapshot and is created before any target flow factory resolution.
            pendingSource = BuildHandoffSourceSnapshot(
                validated.Source,
                validated.OrderedTargets.Length,
                executionCompleted: false);

            // Complete every target-owned metadata graph before any sub-phase lookup or flow factory call.
            var targetMetadata = SnapshotTargetMetadataGraphs(
                request.Metadata,
                validated.OrderedTargets.Length);
            if (!targetMetadata.IsSuccess)
            {
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                    targetMetadata.ErrorMessage,
                    targetMetadata.ErrorCode);
            }

            var subPhases = ResolveEdgeTargetSubPhases(validated, cancellationToken);
            if (!subPhases.IsSuccess)
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(subPhases.ErrorMessage, subPhases.ErrorCode);

            var bindings = ResolveEdgeTargetBindings(
                request,
                flags,
                subPhases.Value!,
                targetMetadata.Value!,
                cancellationToken);
            if (!bindings.IsSuccess)
                return OperationResult<RoutedLogicalFlowResult[]>.Fail(bindings.ErrorMessage, bindings.ErrorCode);
            targets = bindings.Value!;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<RoutedLogicalFlowResult[]>.Fail("Cancelled.", "CANCELLED");
        }
        catch (Exception exception) when (IsNonFatalL4Exception(exception))
        {
            return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                "Dynamic graph target handoff preflight failed.",
                TargetHandoffInvalidCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<RoutedLogicalFlowResult[]>.Fail("Cancelled.", "CANCELLED");

        var targetResults = new LogicalFlowResult[targets.Length];
        try
        {
            if (targets.Length > 1 && validated.Handoff.ParallelEligible)
            {
                // Each parallel target receives its own deep context snapshot; task-array order is target ordinal order.
                var tasks = targets
                    .Select(target => ExecuteResolvedEdgeTargetAsync(
                        flags,
                        target,
                        BuildTargetContext(pendingSource, [], 0),
                        topology,
                        cancellationToken))
                    .ToArray();
                var executed = await Task.WhenAll(tasks);

                // Select failure by target ordinal rather than completion time.
                for (var index = 0; index < executed.Length; index++)
                {
                    if (!executed[index].IsSuccess)
                    {
                        return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                            executed[index].ErrorMessage,
                            executed[index].ErrorCode);
                    }

                    targetResults[index] = executed[index].Value!;
                }
            }
            else
            {
                // Each sequential context is rebuilt from pending source plus only earlier completed target snapshots.
                for (var index = 0; index < targets.Length; index++)
                {
                    var executed = await ExecuteResolvedEdgeTargetAsync(
                        flags,
                        targets[index],
                        BuildTargetContext(pendingSource, targetResults, index),
                        topology,
                        cancellationToken);
                    if (!executed.IsSuccess)
                        return OperationResult<RoutedLogicalFlowResult[]>.Fail(executed.ErrorMessage, executed.ErrorCode);

                    targetResults[index] = executed.Value!;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<RoutedLogicalFlowResult[]>.Fail("Cancelled.", "CANCELLED");
        }
        catch (Exception exception) when (IsNonFatalL4Exception(exception))
        {
            return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                "Dynamic graph target flow execution failed.",
                TargetExecutionFailedCode);
        }

        if (!targetResults.Any(result => result.EdgeTargetHandoff is not null))
        {
            for (var index = 0; index < targets.Length; index++)
            {
                var requiresContinuation = topology!
                    .ListEdgesFrom(targets[index].Selection.TargetNodeId)
                    .Any(edge => edge.Kind != GraphEdgeKind.ReturnLoop);
                if (requiresContinuation)
                {
                    return OperationResult<RoutedLogicalFlowResult[]>.Fail(
                        "Dynamic graph target requires a continuation carrier.",
                        TargetContinuationRequiredCode);
                }
            }
        }

        // Final consumed truth is constructed only after every selected target completed successfully.
        var finalSource = BuildHandoffSourceSnapshot(
            validated.Source,
            targets.Length,
            executionCompleted: true);
        var combinedBranches = InsertTargetResults(
            stableSourceBranches,
            sourceIndex,
            finalSource,
            targets,
            targetResults);
        if (!combinedBranches.Any(branch => branch.Result.EdgeTargetHandoff is not null))
            return OperationResult<RoutedLogicalFlowResult[]>.Ok(combinedBranches);

        // Re-enter the same full validation path with the same invocation-owned request, flags, and topology snapshot.
        return await ConsumeEdgeTargetHandoffAsync(
                request,
                flags,
                combinedBranches,
                topology,
                cancellationToken,
                handoffDepth + 1)
            .ConfigureAwait(false);
    }

    // Purely validate and deep-snapshot the source, handoff, edge, and complete ordered target set.
    private static OperationResult<ValidatedEdgeHandoff> ValidateAndSnapshotEdgeHandoff(
        LogicalOrchestratorRequest request,
        RoutedLogicalFlowResult source,
        DynamicGraphTopology? topology)
    {
        if (topology is null)
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph topology is required to consume a target handoff.",
                TargetTopologyRequiredCode);
        }

        if (!TrySnapshotRoutedLogicalFlowResult(source, out var stableSource)
            || !IsValidHandoffSourceSnapshot(stableSource))
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target handoff source result is invalid.",
                TargetHandoffInvalidCode);
        }

        var handoff = stableSource.Result.EdgeTargetHandoff!;
        if (!StringComparer.Ordinal.Equals(handoff.FromNodeId, stableSource.Decision.PredictedSubPhaseId))
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target handoff source does not match its routed branch.",
                TargetHandoffSourceMismatchCode);
        }

        var identityValues = new[]
        {
            handoff.RunId,
            handoff.EdgeId,
            handoff.FromNodeId,
            handoff.DomainTag,
            handoff.AuthoritySource,
            handoff.UpstreamOutputHash,
            handoff.ProjectionRule
        };
        var isReturnLoop = handoff.Kind == GraphEdgeKind.ReturnLoop;
        if (identityValues.Any(value => !IsCanonicalL4Value(value))
            || handoff.UpstreamCandidateRefs is null
            || !AreCanonicalUniqueL4Refs(handoff.UpstreamCandidateRefs)
            || !StringComparer.Ordinal.Equals(handoff.ProjectionRule, LearnedEdgeProjectionRule))
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target handoff identity or provenance is invalid.",
                TargetHandoffInvalidCode);
        }

        if (!DeterministicDynamicGraphNodeOutputProjector.TryComputeOutputHash(
                handoff.UpstreamCandidateRefs,
                out var expectedUpstreamOutputHash)
            || !StringComparer.Ordinal.Equals(
                handoff.UpstreamOutputHash,
                expectedUpstreamOutputHash))
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target handoff identity or provenance is invalid.",
                TargetHandoffInvalidCode);
        }

        var expectedAuthoritySource = isReturnLoop
            ? ReturnLoopAuthoritySource
            : LearnedEdgeAuthoritySource;
        if (!StringComparer.Ordinal.Equals(handoff.RunId, request.RunId)
            || !StringComparer.Ordinal.Equals(handoff.DomainTag, topology.DomainTag)
            || !StringComparer.Ordinal.Equals(handoff.AuthoritySource, expectedAuthoritySource))
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target handoff scope or authority is invalid.",
                TargetHandoffInvalidCode);
        }

        var edgeMatches = topology.Edges
            .Where(edge => edge is not null && StringComparer.Ordinal.Equals(edge.EdgeId, handoff.EdgeId))
            .ToArray();
        if (edgeMatches.Length == 0)
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target edge is not registered.",
                TargetEdgeNotFoundCode);
        }

        if (edgeMatches.Length != 1)
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target edge identity is ambiguous.",
                TargetEdgeAmbiguousCode);
        }

        var edge = edgeMatches[0];
        if (!StringComparer.Ordinal.Equals(edge.FromNodeId, handoff.FromNodeId)
            || edge.Kind != handoff.Kind
            || edge.ParallelEligible != handoff.ParallelEligible)
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target handoff does not match the registered edge.",
                TargetEdgeMismatchCode);
        }

        if (handoff.Kind is not GraphEdgeKind.FanOut
            and not GraphEdgeKind.ManyToMany
            and not GraphEdgeKind.MultiOutput
            and not GraphEdgeKind.ReturnLoop)
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target edge kind is not executable by L4.",
                TargetEdgeKindUnsupportedCode);
        }

        var orderedTargets = handoff.OrderedTargets.ToArray();
        if (isReturnLoop
            ? handoff.KnowledgeEdge
              || handoff.ParallelEligible
              || orderedTargets.Length != 1
            : handoff.KnowledgeEdge != (orderedTargets.Length == 0)
              || (!handoff.KnowledgeEdge
                  && (orderedTargets.Length < 1 || orderedTargets.Length > MaxAdaptiveBranchCount)))
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target handoff cardinality is invalid.",
                TargetCardinalityInvalidCode);
        }

        if (edge.ToNodeIds is null)
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target edge endpoints are invalid.",
                TargetEdgeMismatchCode);
        }

        var edgeTargetIds = edge.ToNodeIds.ToHashSet(StringComparer.Ordinal);
        var topologyNodeIds = topology.Nodes
            .Where(node => node is not null)
            .Select(node => node.NodeId)
            .ToHashSet(StringComparer.Ordinal);
        if (edgeTargetIds.Count != edge.ToNodeIds.Count
            || edgeTargetIds.Any(target => !IsCanonicalL4Value(target))
            || topologyNodeIds.Count != topology.Nodes.Count)
        {
            return OperationResult<ValidatedEdgeHandoff>.Fail(
                "Dynamic graph target topology identities are invalid.",
                TargetEdgeMismatchCode);
        }

        if (isReturnLoop)
        {
            if (edge.ParallelEligible
                || edge.ToNodeIds.Count != 1
                || orderedTargets[0].Ordinal != 0
                || !orderedTargets[0].Confidence.Equals(1d)
                || !StringComparer.Ordinal.Equals(
                    orderedTargets[0].TargetNodeId,
                    edge.ToNodeIds[0]))
            {
                return OperationResult<ValidatedEdgeHandoff>.Fail(
                    "Dynamic graph ReturnLoop handoff does not match its deterministic correction edge.",
                    TargetEdgeMismatchCode);
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
                return OperationResult<ValidatedEdgeHandoff>.Fail(
                    "Dynamic graph ReturnLoop handoff does not match its deterministic correction edge.",
                    TargetEdgeMismatchCode);
            }
        }

        var selectedIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < orderedTargets.Length; index++)
        {
            var selection = orderedTargets[index];
            if (selection is null
                || selection.Ordinal != index
                || !IsCanonicalL4Value(selection.TargetNodeId)
                || !selectedIds.Add(selection.TargetNodeId)
                || !edgeTargetIds.Contains(selection.TargetNodeId)
                || !topologyNodeIds.Contains(selection.TargetNodeId)
                || !double.IsFinite(selection.Confidence)
                || selection.Confidence is < 0d or > 1d)
            {
                return OperationResult<ValidatedEdgeHandoff>.Fail(
                    "Dynamic graph target selection is invalid.",
                    TargetHandoffInvalidCode);
            }

            if (index > 0)
            {
                var previous = orderedTargets[index - 1];
                if (previous.Confidence < selection.Confidence
                    || (previous.Confidence.Equals(selection.Confidence)
                        && StringComparer.Ordinal.Compare(previous.TargetNodeId, selection.TargetNodeId) >= 0))
                {
                    return OperationResult<ValidatedEdgeHandoff>.Fail(
                        "Dynamic graph target order is invalid.",
                        TargetHandoffInvalidCode);
                }
            }
        }

        return OperationResult<ValidatedEdgeHandoff>.Ok(new ValidatedEdgeHandoff(
            stableSource,
            handoff,
            orderedTargets.Select(selection => selection with { }).ToArray()));
    }

    // Resolve every logical sub-phase in one complete pass before any target flow factory is touched.
    private OperationResult<ResolvedEdgeTargetSubPhase[]> ResolveEdgeTargetSubPhases(
        ValidatedEdgeHandoff validated,
        CancellationToken cancellationToken)
    {
        var resolved = new ResolvedEdgeTargetSubPhase[validated.OrderedTargets.Length];
        for (var index = 0; index < validated.OrderedTargets.Length; index++)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<ResolvedEdgeTargetSubPhase[]>.Fail("Cancelled.", "CANCELLED");

            var selection = validated.OrderedTargets[index];
            var subPhase = _topology!.ResolveSubPhase(selection.TargetNodeId);
            if (!subPhase.IsSuccess
                || subPhase.Value is null
                || subPhase.Value.SubPhase is null
                || !StringComparer.Ordinal.Equals(
                    subPhase.Value.SubPhase.SubPhaseId,
                    selection.TargetNodeId)
                || !IsCanonicalL4Value(subPhase.Value.SubPhase.PhaseId))
            {
                return OperationResult<ResolvedEdgeTargetSubPhase[]>.Fail(
                    "Dynamic graph target sub-phase is not registered.",
                    TargetNotRegisteredCode);
            }

            var decision = new SubPhaseRoutingDecision(
                validated.Source.Decision.DocumentId,
                validated.Source.Decision.SourceStatus,
                subPhase.Value.SubPhase.PhaseId,
                selection.TargetNodeId,
                selection.Confidence,
                0d,
                [validated.Handoff.Kind == GraphEdgeKind.ReturnLoop
                    ? ReturnLoopAuthoritySource
                    : LearnedEdgeAuthoritySource],
                false,
                validated.Handoff.Kind == GraphEdgeKind.ReturnLoop
                    ? "topology_return_loop_target"
                    : "learned_edge_authority_target")
            {
                AuthorityRole = validated.Handoff.Kind == GraphEdgeKind.ReturnLoop
                    ? RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
                    : RoutingAuthorityVocabulary.TrainedEdgeUnitRole
            };
            resolved[index] = new ResolvedEdgeTargetSubPhase(selection, decision);
        }

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<ResolvedEdgeTargetSubPhase[]>.Fail("Cancelled.", "CANCELLED");

        return OperationResult<ResolvedEdgeTargetSubPhase[]>.Ok(resolved);
    }

    // Resolve all string flow bindings only after the complete target/sub-phase set has validated.
    private OperationResult<ResolvedEdgeTarget[]> ResolveEdgeTargetBindings(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        ResolvedEdgeTargetSubPhase[] subPhases,
        IReadOnlyDictionary<string, object>[] targetMetadata,
        CancellationToken cancellationToken)
    {
        if (targetMetadata.Length != subPhases.Length)
        {
            return OperationResult<ResolvedEdgeTarget[]>.Fail(
                "Dynamic graph target metadata cardinality is invalid.",
                TargetMetadataInvalidCode);
        }

        var resolved = new ResolvedEdgeTarget[subPhases.Length];
        for (var index = 0; index < subPhases.Length; index++)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<ResolvedEdgeTarget[]>.Fail("Cancelled.", "CANCELLED");

            var target = subPhases[index];
            var targetRequest = request with { Metadata = targetMetadata[index] };
            var binding = _flows!.ResolveBindingBySubPhase(
                target.Selection.TargetNodeId,
                new LogicalFlowFactoryRequest(targetRequest.RunId, flags, _units!, targetRequest.Metadata, _topology));
            if (!binding.IsSuccess || binding.Value is null)
            {
                return OperationResult<ResolvedEdgeTarget[]>.Fail(
                    "Dynamic graph target flow is not registered.",
                    TargetFlowNotRegisteredCode);
            }

            resolved[index] = new ResolvedEdgeTarget(
                target.Selection,
                target.Decision,
                binding.Value,
                targetRequest);
        }

        if (cancellationToken.IsCancellationRequested)
            return OperationResult<ResolvedEdgeTarget[]>.Fail("Cancelled.", "CANCELLED");

        return OperationResult<ResolvedEdgeTarget[]>.Ok(resolved);
    }

    // Execute one already-resolved target and sanitize all non-fatal dependency failures at the L4 boundary.
    private static async Task<OperationResult<LogicalFlowResult>> ExecuteResolvedEdgeTargetAsync(
        LogicalExecutionFlags flags,
        ResolvedEdgeTarget target,
        IReadOnlyList<LogicalFlowResult> completedBranches,
        DynamicGraphTopology? topology,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

        try
        {
            var executed = await ExecuteBindingAsync(
                target.Request,
                flags,
                target.Binding,
                target.Decision,
                completedBranches,
                cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

            if (!executed.IsSuccess || executed.Value is null)
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Dynamic graph target flow execution failed.",
                    TargetExecutionFailedCode);
            }

            if (!LogicalMetadataSnapshot.TryCreate(executed.Value, out var stableResult)
                || !IsValidL4TargetResultSnapshot(stableResult))
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Dynamic graph target flow result is invalid.",
                    TargetExecutionFailedCode);
            }

            if (stableResult.EdgeTargetHandoff is not null)
            {
                var handoffDecision = AssociateDecisionWithHandoffAuthority(
                    target.Decision,
                    stableResult.EdgeTargetHandoff.Kind);
                var routed = new RoutedLogicalFlowResult(handoffDecision, stableResult);
                var validation = ValidateAndSnapshotEdgeHandoff(target.Request, routed, topology);
                if (!validation.IsSuccess)
                {
                    return OperationResult<LogicalFlowResult>.Fail(
                        validation.ErrorMessage,
                        validation.ErrorCode);
                }

                stableResult = validation.Value!.Source.Result;
            }

            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

            return OperationResult<LogicalFlowResult>.Ok(stableResult);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");
        }
        catch (Exception exception) when (IsNonFatalL4Exception(exception))
        {
            return OperationResult<LogicalFlowResult>.Fail(
                "Dynamic graph target flow execution failed.",
                TargetExecutionFailedCode);
        }
    }

    // Replace the handoff-producing result with the primary consumed result and insert targets immediately after it.
    private static RoutedLogicalFlowResult[] InsertTargetResults(
        RoutedLogicalFlowResult[] sourceBranches,
        int sourceIndex,
        RoutedLogicalFlowResult consumedSource,
        IReadOnlyList<ResolvedEdgeTarget> targets,
        IReadOnlyList<LogicalFlowResult> targetResults)
    {
        var output = new List<RoutedLogicalFlowResult>(sourceBranches.Length + targetResults.Count);
        for (var index = 0; index < sourceBranches.Length; index++)
        {
            if (index != sourceIndex)
            {
                output.Add(sourceBranches[index]);
                continue;
            }

            output.Add(consumedSource);
            for (var targetIndex = 0; targetIndex < targetResults.Count; targetIndex++)
            {
                var targetResult = targetResults[targetIndex];
                var targetDecision = targetResult.EdgeTargetHandoff is { } nestedHandoff
                    ? AssociateDecisionWithHandoffAuthority(targets[targetIndex].Decision, nestedHandoff.Kind)
                    : targets[targetIndex].Decision;
                output.Add(new RoutedLogicalFlowResult(targetDecision, targetResult));
            }
        }

        return output.ToArray();
    }

    // Create an independently owned pending or final source snapshot with honest handoff state.
    private static RoutedLogicalFlowResult BuildHandoffSourceSnapshot(
        RoutedLogicalFlowResult source,
        int selectedTargetCount,
        bool executionCompleted)
    {
        if (!TrySnapshotRoutedLogicalFlowResult(source, out var ownedSource))
            throw new InvalidOperationException("Dynamic graph source snapshot failed.");

        var isReturnLoop = ownedSource.Result.EdgeTargetHandoff?.Kind == GraphEdgeKind.ReturnLoop;
        var isGap = selectedTargetCount == 0;
        var metrics = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var metric in ownedSource.Result.Metrics.OrderBy(item => item.Key, StringComparer.Ordinal))
            metrics[metric.Key] = metric.Value;
        metrics["handoff_consumed"] = "true";
        metrics["handoff_state"] = executionCompleted
            ? isGap ? "consumed_gap" : "target_execution_completed"
            : "target_execution_pending";
        metrics["selected_target_count"] = selectedTargetCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        metrics["target_execution_count"] = (executionCompleted ? selectedTargetCount : 0)
            .ToString(System.Globalization.CultureInfo.InvariantCulture);
        metrics["targets_executed"] = executionCompleted && !isGap ? "true" : "false";
        metrics["topology_mutated"] = "false";
        if (executionCompleted)
            metrics[RoutingAuthorityVocabulary.MetadataKey] = ownedSource.Decision.AuthorityRole;
        var summary = ownedSource.Result.Summary with
        {
            ShouldScaleUp = false,
            NextAction = isReturnLoop
                ? !executionCompleted
                    ? "EXECUTE_RETURN_LOOP_TARGET"
                    : "CONTINUE_AFTER_RETURN_LOOP_CORRECTION"
                : !executionCompleted
                    ? "EXECUTE_RESOLVED_TARGETS"
                    : isGap ? "REVIEW_EDGE_AUTHORITY_GAP" : "CONTINUE_AFTER_TARGET_EXECUTION",
            Notes = isReturnLoop
                ? !executionCompleted
                    ? "L4 consumed the deterministic ReturnLoop handoff into an isolated pending context; one correction target remains pending. Topology was not mutated."
                    : "L4 consumed the deterministic ReturnLoop handoff after its single correction target completed. Topology was not mutated."
                : !executionCompleted
                    ? $"L4 consumed the typed handoff into an isolated pending context; {selectedTargetCount} selected target flow(s) remain pending. Topology was not mutated."
                    : isGap
                        ? "L4 consumed the typed authority-gap handoff; no target flow was executed. Topology was not mutated."
                        : $"L4 consumed the typed target handoff after all {selectedTargetCount} selected target flow(s) completed. Topology was not mutated."
        };
        var consumed = ownedSource.Result with
        {
            OverallVerdict = isReturnLoop
                ? !executionCompleted
                    ? "RETURN_LOOP_EXECUTION_PENDING"
                    : "RETURN_LOOP_CONSUMED"
                : !executionCompleted
                    ? "TARGET_HANDOFF_EXECUTION_PENDING"
                    : isGap ? "EDGE_AUTHORITY_GAP_CONSUMED" : "TARGET_HANDOFF_CONSUMED",
            FinalHumanResult = isReturnLoop
                ? !executionCompleted
                    ? "L4 consumed the deterministic ReturnLoop handoff into pending context; its correction target has not yet executed."
                    : "L4 consumed the deterministic ReturnLoop handoff after its correction target completed and returned a result."
                : !executionCompleted
                    ? $"L4 consumed the learned-edge handoff into pending context; {selectedTargetCount} target logical flow(s) have not yet executed."
                    : isGap
                        ? "L4 consumed the learned-edge authority-gap handoff; no target logical flow was executed."
                        : $"L4 consumed the learned-edge handoff after {selectedTargetCount} target logical flow(s) completed and returned results.",
            Summary = summary,
            Metrics = new ReadOnlyDictionary<string, string>(metrics),
            EdgeTargetHandoff = null
        };

        return new RoutedLogicalFlowResult(ownedSource.Decision, consumed);
    }

    // A handoff-producing branch reports the authority of its outgoing edge, independent of how it was reached.
    private static SubPhaseRoutingDecision AssociateDecisionWithHandoffAuthority(
        SubPhaseRoutingDecision decision,
        GraphEdgeKind handoffKind)
        => decision with
        {
            AuthorityRole = handoffKind == GraphEdgeKind.ReturnLoop
                ? RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
                : RoutingAuthorityVocabulary.TrainedEdgeUnitRole
        };

    // Build a fresh context graph for each target so no mutable arrays or dictionaries cross target boundaries.
    private static LogicalFlowResult[] BuildTargetContext(
        RoutedLogicalFlowResult pendingSource,
        IReadOnlyList<LogicalFlowResult> completedTargets,
        int completedTargetCount)
    {
        var context = new LogicalFlowResult[completedTargetCount + 1];
        if (!LogicalMetadataSnapshot.TryCreate(pendingSource.Result, out context[0]))
            throw new InvalidOperationException("Dynamic graph pending context snapshot failed.");

        for (var index = 0; index < completedTargetCount; index++)
        {
            if (!LogicalMetadataSnapshot.TryCreate(completedTargets[index], out context[index + 1]))
                throw new InvalidOperationException("Dynamic graph completed target context snapshot failed.");
        }

        return context;
    }

    // Snapshot every initial branch on the handoff path while replacing the producer with its validated snapshot.
    private static bool TrySnapshotSourceBranches(
        RoutedLogicalFlowResult[] sourceBranches,
        int sourceIndex,
        RoutedLogicalFlowResult stableSource,
        out RoutedLogicalFlowResult[] snapshots)
    {
        snapshots = new RoutedLogicalFlowResult[sourceBranches.Length];
        for (var index = 0; index < sourceBranches.Length; index++)
        {
            if (index == sourceIndex)
            {
                if (!TrySnapshotRoutedLogicalFlowResult(stableSource, out snapshots[index]))
                    return false;
                continue;
            }

            if (!TrySnapshotRoutedLogicalFlowResult(sourceBranches[index], out snapshots[index]))
                return false;
        }

        return true;
    }

    private static bool TrySnapshotRoutedLogicalFlowResult(
        RoutedLogicalFlowResult source,
        out RoutedLogicalFlowResult snapshot)
    {
        snapshot = null!;
        if (source is null
            || source.Decision is null
            || source.Decision.EvidencePredicates is null
            || !LogicalMetadataSnapshot.TryCreate(source.Result, out var result))
        {
            return false;
        }

        var decision = new SubPhaseRoutingDecision(
            source.Decision.DocumentId,
            source.Decision.SourceStatus,
            source.Decision.PredictedPhaseId,
            source.Decision.PredictedSubPhaseId,
            source.Decision.Confidence,
            source.Decision.NearestWrongSubPhaseScore,
            source.Decision.EvidencePredicates.ToArray(),
            source.Decision.KnowledgeEdge,
            source.Decision.FailureClassification)
        {
            AuthorityRole = source.Decision.AuthorityRole
        };
        snapshot = new RoutedLogicalFlowResult(decision, result);
        return true;
    }

    // Deep-own one base metadata graph, then derive every target graph before any registry or factory access.
    private static OperationResult<IReadOnlyDictionary<string, object>[]> SnapshotTargetMetadataGraphs(
        IReadOnlyDictionary<string, object> source,
        int targetCount)
    {
        return LogicalMetadataSnapshot.TryCreateCopies(source, targetCount, out var targetGraphs)
            ? OperationResult<IReadOnlyDictionary<string, object>[]>.Ok(targetGraphs)
            : OperationResult<IReadOnlyDictionary<string, object>[]>.Fail(
                "Dynamic graph target metadata is invalid.",
                TargetMetadataInvalidCode);
    }

    // Strict source validation dispatches kind-specific invariants before any registry or factory call.
    private static bool IsValidHandoffSourceSnapshot(RoutedLogicalFlowResult source)
        => source.Result.EdgeTargetHandoff?.Kind == GraphEdgeKind.ReturnLoop
            ? IsValidReturnLoopHandoffSourceSnapshot(source)
            : IsValidLearnedHandoffSourceSnapshot(source);

    // ReturnLoop uses exact deterministic literals and cannot borrow learned-edge result semantics.
    private static bool IsValidReturnLoopHandoffSourceSnapshot(RoutedLogicalFlowResult source)
    {
        var decision = source.Decision;
        var result = source.Result;
        var summary = result.Summary;
        var handoff = result.EdgeTargetHandoff;
        if (handoff is null)
            return false;

        return !decision.KnowledgeEdge
            && StringComparer.Ordinal.Equals(
                decision.AuthorityRole,
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole)
            && IsCanonicalL4Value(decision.DocumentId)
            && IsCanonicalL4Value(decision.SourceStatus)
            && IsCanonicalL4Value(decision.PredictedPhaseId)
            && IsCanonicalL4Value(decision.PredictedSubPhaseId)
            && IsCanonicalL4Value(decision.FailureClassification)
            && double.IsFinite(decision.Confidence)
            && decision.Confidence is >= 0d and <= 1d
            && double.IsFinite(decision.NearestWrongSubPhaseScore)
            && decision.NearestWrongSubPhaseScore is >= 0d and <= 1d
            && decision.EvidencePredicates.All(IsCanonicalL4Value)
            && StringComparer.Ordinal.Equals(result.FlowId, ProjectedEdgeFlowId)
            && StringComparer.Ordinal.Equals(result.ScenarioId, ProjectedEdgeScenarioId)
            && StringComparer.Ordinal.Equals(result.OverallVerdict, ReturnLoopHandoffReadyVerdict)
            && StringComparer.Ordinal.Equals(result.FinalHumanResult, ReturnLoopHumanResult)
            && StringComparer.Ordinal.Equals(summary.SummaryId, $"{ProjectedEdgeFlowId}:{ReturnLoopSummarySuffix}")
            && summary.Tier == 3
            && StringComparer.Ordinal.Equals(summary.PrimaryMetricName, ReturnLoopPrimaryMetricName)
            && StringComparer.Ordinal.Equals(summary.NextAction, ReturnLoopToL4Action)
            && StringComparer.Ordinal.Equals(summary.ScalingSignalNote, ReturnLoopScalingSignalNote)
            && StringComparer.Ordinal.Equals(summary.Notes, ReturnLoopSummaryNotes)
            && summary.BaselinePrimary.Equals(0d)
            && summary.EnhancedPrimary.Equals(1d)
            && summary.ScalingSignal.Equals(0d)
            && summary.BaselineTimeMs.Equals(0d)
            && summary.EnhancedTimeMs.Equals(0d)
            && summary.Passed
            && !summary.ShouldScaleUp
            && !summary.LowerIsBetter
            && handoff.Kind == GraphEdgeKind.ReturnLoop
            && !handoff.KnowledgeEdge
            && !handoff.ParallelEligible
            && handoff.OrderedTargets.Count == 1
            && HasExactL4Metric(result.Metrics, "authority_source", ReturnLoopAuthoritySource)
            && HasExactL4Metric(result.Metrics, "authority_gap", "false")
            && HasExactL4Metric(result.Metrics, "authority_warning_count", "0")
            && HasExactL4Metric(result.Metrics, "edge_id", handoff.EdgeId)
            && HasExactL4Metric(result.Metrics, "edge_kind", GraphEdgeKind.ReturnLoop.ToString())
            && HasExactL4Metric(result.Metrics, "parallel_eligible", "false")
            && HasExactL4Metric(result.Metrics, "target_count", "1")
            && HasExactL4Metric(result.Metrics, "targets_executed", "false")
            && HasExactL4Metric(result.Metrics, "topology_mutated", "false")
            && result.Metrics.Count == 9
            && result.EvidenceIds.Length == 0
            && result.Warnings.Length == 0
            && result.TransitionSignals.Length == 0;
    }

    // Learned target handoffs retain their established result and metric invariants.
    private static bool IsValidLearnedHandoffSourceSnapshot(RoutedLogicalFlowResult source)
    {
        var decision = source.Decision;
        var result = source.Result;
        var summary = result.Summary;
        var handoff = result.EdgeTargetHandoff;
        if (handoff is null)
            return false;

        var expectedVerdict = handoff.KnowledgeEdge
            ? AuthorityGapHandoffVerdict
            : TargetHandoffReadyVerdict;
        var expectedNextAction = handoff.KnowledgeEdge
            ? HandoffGapToL4Action
            : HandoffToL4Action;
        var expectedHumanResult = handoff.KnowledgeEdge
            ? AuthorityGapHumanResult
            : TargetHandoffHumanResult;
        var targetCount = handoff.OrderedTargets.Count;
        var expectedTargetCount = targetCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var expectedWarningCount = result.Warnings.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var expectedSummaryId = $"{result.FlowId}:edge-target-handoff";

        return !decision.KnowledgeEdge
            && StringComparer.Ordinal.Equals(
                decision.AuthorityRole,
                RoutingAuthorityVocabulary.TrainedEdgeUnitRole)
            && IsCanonicalL4Value(decision.DocumentId)
            && IsCanonicalL4Value(decision.SourceStatus)
            && IsCanonicalL4Value(decision.PredictedPhaseId)
            && IsCanonicalL4Value(decision.PredictedSubPhaseId)
            && IsCanonicalL4Value(decision.FailureClassification)
            && double.IsFinite(decision.Confidence)
            && decision.Confidence is >= 0d and <= 1d
            && double.IsFinite(decision.NearestWrongSubPhaseScore)
            && decision.NearestWrongSubPhaseScore is >= 0d and <= 1d
            && decision.EvidencePredicates.All(IsCanonicalL4Value)
            && IsCanonicalL4Value(result.FlowId)
            && IsCanonicalL4Value(result.ScenarioId)
            && IsCanonicalL4Value(result.OverallVerdict)
            && StringComparer.Ordinal.Equals(result.OverallVerdict, expectedVerdict)
            && IsSafeL4Text(result.FinalHumanResult, requireNonEmpty: true)
            && StringComparer.Ordinal.Equals(result.FinalHumanResult, expectedHumanResult)
            && IsCanonicalL4Value(summary.SummaryId)
            && StringComparer.Ordinal.Equals(summary.SummaryId, expectedSummaryId)
            && summary.Tier == 3
            && IsCanonicalL4Value(summary.PrimaryMetricName)
            && StringComparer.Ordinal.Equals(summary.PrimaryMetricName, "selected_target_count")
            && IsCanonicalL4Value(summary.NextAction)
            && StringComparer.Ordinal.Equals(summary.NextAction, expectedNextAction)
            && IsSafeL4Text(summary.ScalingSignalNote, requireNonEmpty: false)
            && StringComparer.Ordinal.Equals(summary.ScalingSignalNote, HandoffScalingSignalNote)
            && IsSafeL4Text(summary.Notes, requireNonEmpty: false)
            && StringComparer.Ordinal.Equals(summary.Notes, HandoffSummaryNotes)
            && double.IsFinite(summary.BaselinePrimary)
            && summary.BaselinePrimary.Equals(0d)
            && double.IsFinite(summary.EnhancedPrimary)
            && summary.EnhancedPrimary.Equals((double)targetCount)
            && double.IsFinite(summary.ScalingSignal)
            && summary.ScalingSignal.Equals(0d)
            && double.IsFinite(summary.BaselineTimeMs)
            && summary.BaselineTimeMs.Equals(0d)
            && double.IsFinite(summary.EnhancedTimeMs)
            && summary.EnhancedTimeMs.Equals(0d)
            && summary.Passed
            && !summary.ShouldScaleUp
            && !summary.LowerIsBetter
            && HasExactL4Metric(result.Metrics, "authority_source", LearnedEdgeAuthoritySource)
            && HasExactL4Metric(
                result.Metrics,
                "authority_gap",
                handoff.KnowledgeEdge ? "true" : "false")
            && HasExactL4Metric(result.Metrics, "authority_warning_count", expectedWarningCount)
            && HasExactL4Metric(result.Metrics, "edge_id", handoff.EdgeId)
            && HasExactL4Metric(result.Metrics, "edge_kind", handoff.Kind.ToString())
            && HasExactL4Metric(
                result.Metrics,
                "parallel_eligible",
                handoff.ParallelEligible ? "true" : "false")
            && HasExactL4Metric(result.Metrics, "target_count", expectedTargetCount)
            && HasExactL4Metric(result.Metrics, "targets_executed", "false")
            && HasExactL4Metric(result.Metrics, "topology_mutated", "false")
            && result.Metrics.All(metric =>
                IsAllowedHandoffMetricKey(metric.Key)
                && IsCanonicalL4Value(metric.Key)
                && IsSafeL4Text(metric.Value, requireNonEmpty: false))
            && !result.Metrics.Keys.Any(key =>
                StringComparer.OrdinalIgnoreCase.Equals(key, "knowledge_edge"))
            && result.EvidenceIds.All(IsCanonicalL4Value)
            && result.Warnings.All(value => IsSafeL4Text(value, requireNonEmpty: false))
            && !result.Warnings.Any(value =>
                value.Contains("knowledge_edge", StringComparison.OrdinalIgnoreCase))
            && result.TransitionSignals.Length == 0;
    }

    // Validate an already-deep-snapshotted target result before it can enter shared branch state.
    private static bool IsValidL4TargetResultSnapshot(LogicalFlowResult result)
    {
        var summary = result.Summary;
        return result.Metrics is ReadOnlyDictionary<string, string>
            && IsCanonicalL4Value(result.FlowId)
            && IsCanonicalL4Value(result.ScenarioId)
            && IsCanonicalL4Value(result.OverallVerdict)
            && IsSafeL4FreeText(result.FinalHumanResult, requireNonEmpty: true)
            && IsCanonicalL4Value(summary.SummaryId)
            && IsCanonicalL4Value(summary.PrimaryMetricName)
            && IsCanonicalL4Value(summary.NextAction)
            && IsSafeL4FreeText(summary.ScalingSignalNote, requireNonEmpty: false)
            && IsSafeL4FreeText(summary.Notes, requireNonEmpty: false)
            && double.IsFinite(summary.BaselinePrimary)
            && double.IsFinite(summary.EnhancedPrimary)
            && double.IsFinite(summary.ScalingSignal)
            && double.IsFinite(summary.BaselineTimeMs)
            && double.IsFinite(summary.EnhancedTimeMs)
            && summary.Passed
            && !StringComparer.OrdinalIgnoreCase.Equals(result.OverallVerdict, "REJECT")
            && !StringComparer.OrdinalIgnoreCase.Equals(result.OverallVerdict, "REPROCESS")
            && result.Metrics.All(metric =>
                IsCanonicalL4Value(metric.Key)
                && IsSafeL4FreeText(metric.Value, requireNonEmpty: false))
            && result.EvidenceIds.All(IsCanonicalL4Value)
            && result.Warnings.All(value => IsSafeL4FreeText(value, requireNonEmpty: false))
            && HasValidL4TransitionSignals(result.TransitionSignals);
    }

    private static bool HasValidL4TransitionSignals(
        IReadOnlyList<PhaseTransitionSignal>? signals)
        => signals is not null
           && signals.All(signal =>
               signal is not null
               && IsSafeL4FreeText(signal.TransitionPrompt, requireNonEmpty: true)
               && IsCanonicalL4Value(signal.SourceNodeId)
               && IsCanonicalL4Value(signal.SourceSubPhaseId)
               && signal.EvidenceIds is not null
               && signal.EvidenceIds.All(IsCanonicalL4Value));

    private static bool HasExactL4Metric(
        IReadOnlyDictionary<string, string> metrics,
        string key,
        string expectedValue)
        => metrics.TryGetValue(key, out var actualValue)
           && StringComparer.Ordinal.Equals(actualValue, expectedValue);

    private static bool IsAllowedHandoffMetricKey(string key)
        => (key is "authority_source"
                or "authority_gap"
                or "authority_warning_count"
                or "edge_id"
                or "edge_kind"
                or "parallel_eligible"
                or "target_count"
                or "targets_executed"
                or "topology_mutated")
           || StringComparer.Ordinal.Equals(key, PromptDomainClassifier.DomainLabelMetadataKey)
           || StringComparer.Ordinal.Equals(key, PromptDomainClassifier.DomainConfidenceMetadataKey)
           || StringComparer.Ordinal.Equals(key, PromptDomainClassifier.DomainEvidenceTermsMetadataKey);

    private static bool IsSafeL4Text(string? value, bool requireNonEmpty)
        => value is not null
           && (!requireNonEmpty || !string.IsNullOrWhiteSpace(value))
           && !value.Any(char.IsControl)
           && value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) < 0;

    // Human-facing target text may use ordinary line and tab formatting, but no other controls or separators.
    private static bool IsSafeL4FreeText(string? value, bool requireNonEmpty)
    {
        if (value is null || (requireNonEmpty && string.IsNullOrWhiteSpace(value)))
            return false;

        foreach (var character in value)
        {
            if (character is '\r' or '\n' or '\t')
                continue;
            if (char.IsControl(character) || character is '\u0085' or '\u2028' or '\u2029')
                return false;
        }

        return true;
    }

    private static bool AreCanonicalUniqueL4Refs(IReadOnlyList<string> references)
    {
        if (references.Count == 0)
            return false;

        for (var index = 0; index < references.Count; index++)
        {
            if (!IsCanonicalL4Value(references[index]))
                return false;
            if (index > 0 && StringComparer.Ordinal.Compare(references[index - 1], references[index]) >= 0)
                return false;
        }

        return true;
    }

    private static bool IsCanonicalL4Value(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !value.Any(char.IsControl)
           && value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) < 0;

    private static bool IsCanonicalNodeOutputValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) >= 0)
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character))
                return false;

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static bool IsNonFatalNodeOutputSourceException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    private static bool IsNonFatalL4Exception(Exception exception)
        => exception is not OperationCanceledException
           && exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    // Determine if any routed branches have a declared transition dependency.
    private bool HasTransitionDependency(SubPhaseRoutingDecision[] decisions)
        => HasTransitionDependency(decisions, new LogicalExecutionFlags { UseE35DependencyDetection = true });

    private bool HasTransitionDependency(SubPhaseRoutingDecision[] decisions, LogicalExecutionFlags flags)
        => HasTransitionDependency(decisions, flags, BuildDynamicDomainTransitions());

    // Detect dependencies from the invocation-local non-parallel transition view.
    private bool HasTransitionDependency(
        SubPhaseRoutingDecision[] decisions,
        LogicalExecutionFlags flags,
        DynamicDomainTransition[] dependencyTransitions)
    {
        // Build the routed id set.
        var routedIds = decisions.Select(decision => decision.PredictedSubPhaseId).ToHashSet(StringComparer.Ordinal);

        // Dependency exists when graph closure finds any routed sub-phase reachable from another.
        if (DomainGraphClosure.HasAnyDependency(routedIds, dependencyTransitions))
            return true;

        // E35 adds dynamic dependency detection only when the caller enables the flag and the route carries E35 evidence.
        return flags.UseE35DependencyDetection &&
            HasDependencyGraphEvidence(decisions) &&
            (HasSequentialPairDependency(decisions) || HasDependencyGraphDomainDependency(decisions, requireEvidence: true));
    }

    // Order routed decisions with simple transition precedence.
    private SubPhaseRoutingDecision[] OrderByTransitions(SubPhaseRoutingDecision[] decisions)
        => OrderByTransitions(decisions, new LogicalExecutionFlags { UseE35DependencyDetection = true });

    private SubPhaseRoutingDecision[] OrderByTransitions(SubPhaseRoutingDecision[] decisions, LogicalExecutionFlags flags)
        => OrderByTransitions(decisions, flags, BuildDynamicDomainTransitions());

    // Order with the invocation-local dependency view while retaining the legacy two-argument reflection seam.
    private SubPhaseRoutingDecision[] OrderByTransitions(
        SubPhaseRoutingDecision[] decisions,
        LogicalExecutionFlags flags,
        DynamicDomainTransition[] dependencyTransitions)
    {
        // If no dependency exists, preserve router order.
        if (!HasTransitionDependency(decisions, flags, dependencyTransitions))
            return decisions;

        // Build transitive predecessor counts among routed ids.
        var routedIds = decisions.Select(decision => decision.PredictedSubPhaseId).ToHashSet(StringComparer.Ordinal);
        var predecessorCounts = DomainGraphClosure.BuildPredecessorMap(routedIds, dependencyTransitions);

        // E35 maps each routed sub-phase to the canonical project-29 dependency level only for explicitly marked routes.
        var useE35Evidence = flags.UseE35DependencyDetection && HasDependencyGraphEvidence(decisions);
        var dependencyGraphOrder = useE35Evidence
            ? BuildDependencyGraphOrder(decisions, requireEvidence: true)
            : new Dictionary<string, int>(StringComparer.Ordinal);
        var sequentialPairOrder = useE35Evidence
            ? BuildSequentialPairOrder(decisions)
            : new Dictionary<string, int>(StringComparer.Ordinal);

        // Order first by domain dependency level, then by topology predecessor count, then stable sub-phase id.
        return decisions
            .OrderBy(decision => sequentialPairOrder.GetValueOrDefault(decision.PredictedSubPhaseId, int.MaxValue))
            .ThenBy(decision => dependencyGraphOrder.GetValueOrDefault(decision.PredictedSubPhaseId, int.MaxValue))
            .ThenBy(decision => predecessorCounts[decision.PredictedSubPhaseId])
            .ThenBy(decision => decision.PredictedSubPhaseId, StringComparer.Ordinal)
            .ToArray();
    }

    private bool HasSequentialPairDependency(SubPhaseRoutingDecision[] decisions)
    {
        return BuildSequentialPairOrder(decisions).Count > 0;
    }

    private Dictionary<string, int> BuildSequentialPairOrder(SubPhaseRoutingDecision[] decisions)
    {
        if (decisions.Length <= 1)
            return new Dictionary<string, int>(StringComparer.Ordinal);

        var ids = decisions.Select(decision => decision.PredictedSubPhaseId).Distinct(StringComparer.Ordinal).ToArray();
        var outgoing = ids.ToDictionary(id => id, _ => new HashSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
        var incoming = ids.ToDictionary(id => id, _ => 0, StringComparer.Ordinal);
        for (var left = 0; left < decisions.Length; left++)
            for (var right = left + 1; right < decisions.Length; right++)
            {
                var leftText = BuildSequentialPairText(decisions[left]);
                var rightText = BuildSequentialPairText(decisions[right]);
                if (AgentAlgorithms.DependencyGraph.IsSequentialPair(leftText, rightText))
                    AddSequentialEdge(decisions[left].PredictedSubPhaseId, decisions[right].PredictedSubPhaseId, outgoing, incoming);
                else if (AgentAlgorithms.DependencyGraph.IsSequentialPair(rightText, leftText))
                    AddSequentialEdge(decisions[right].PredictedSubPhaseId, decisions[left].PredictedSubPhaseId, outgoing, incoming);
            }

        if (incoming.Values.All(value => value == 0))
            return new Dictionary<string, int>(StringComparer.Ordinal);

        var queue = new Queue<string>(incoming.Where(item => item.Value == 0).Select(item => item.Key).OrderBy(id => id, StringComparer.Ordinal));
        var levels = new Dictionary<string, int>(StringComparer.Ordinal);
        var level = 0;
        while (queue.Count > 0)
        {
            var levelCount = queue.Count;
            for (var index = 0; index < levelCount; index++)
            {
                var id = queue.Dequeue();
                levels[id] = level;
                foreach (var next in outgoing[id].OrderBy(value => value, StringComparer.Ordinal))
                {
                    incoming[next]--;
                    if (incoming[next] == 0)
                        queue.Enqueue(next);
                }
            }

            level++;
        }

        return levels.Count == ids.Length ? levels : new Dictionary<string, int>(StringComparer.Ordinal);
    }

    private static void AddSequentialEdge(
        string prerequisite,
        string dependent,
        Dictionary<string, HashSet<string>> outgoing,
        Dictionary<string, int> incoming)
    {
        if (string.Equals(prerequisite, dependent, StringComparison.Ordinal))
            return;
        if (outgoing[prerequisite].Add(dependent))
            incoming[dependent]++;
    }

    private string BuildSequentialPairText(SubPhaseRoutingDecision decision)
    {
        var resolved = _topology!.ResolveSubPhase(decision.PredictedSubPhaseId);
        if (!resolved.IsSuccess)
            return string.Join(" ", decision.EvidencePredicates);

        var subPhase = resolved.Value!.SubPhase;
        return string.Join(" ", [subPhase.Name, subPhase.Purpose, .. subPhase.PredicateIds, .. decision.EvidencePredicates]);
    }

    // Detect whether E35's canonical domain graph imposes an order among routed branches.
    private bool HasDependencyGraphDomainDependency(SubPhaseRoutingDecision[] decisions)
        => HasDependencyGraphDomainDependency(decisions, requireEvidence: true);

    // Detect whether E35's canonical domain graph imposes an order among routed branches.
    private bool HasDependencyGraphDomainDependency(SubPhaseRoutingDecision[] decisions, bool requireEvidence)
    {
        // E35 dependency ordering is an explicit routing signal unless the feature flag already opted in.
        if (requireEvidence && !HasDependencyGraphEvidence(decisions))
            return false;

        // Resolve route domains from topology metadata; unresolved routes cannot contribute dependency evidence.
        var domains = decisions
            .Select(TryInferDependencyDomain)
            .Where(domain => domain.HasValue)
            .Select(domain => domain!.Value)
            .Distinct()
            .ToArray();

        // One or zero domains cannot have a cross-domain dependency.
        if (domains.Length <= 1)
            return false;

        // Sort the domains through the production E35 graph.
        var sorted = BuildDependencyGraph().Sort(domains);
        if (!sorted.IsSuccess)
            return false;

        // Multiple dependency levels means at least one routed branch should wait for another.
        return sorted.Value!.Length > 1;
    }

    // Build the project-29 dependency order for each routed sub-phase id.
    private Dictionary<string, int> BuildDependencyGraphOrder(SubPhaseRoutingDecision[] decisions)
        => BuildDependencyGraphOrder(decisions, requireEvidence: true);

    // Build the project-29 dependency order for each routed sub-phase id.
    private Dictionary<string, int> BuildDependencyGraphOrder(SubPhaseRoutingDecision[] decisions, bool requireEvidence)
    {
        // Preserve existing router order unless E35 evidence or a flag opts this branch set into graph ordering.
        if (requireEvidence && !HasDependencyGraphEvidence(decisions))
            return new Dictionary<string, int>(StringComparer.Ordinal);

        // Resolve each decision to the E35 domain graph domain.
        var decisionDomains = decisions
            .Select(decision => new { Decision = decision, Domain = TryInferDependencyDomain(decision) })
            .Where(item => item.Domain.HasValue)
            .ToArray();

        // Return an empty order when no decision can be mapped.
        if (decisionDomains.Length == 0)
            return new Dictionary<string, int>(StringComparer.Ordinal);

        // Sort distinct domains through the canonical project-29 graph.
        var sorted = BuildDependencyGraph().Sort(decisionDomains.Select(item => item.Domain!.Value).Distinct().ToArray());
        if (!sorted.IsSuccess)
            return new Dictionary<string, int>(StringComparer.Ordinal);

        // Convert graph levels to a lookup table.
        var domainLevels = sorted.Value!
            .SelectMany((level, index) => level.Select(domain => new { Domain = domain, Level = index }))
            .ToDictionary(item => item.Domain, item => item.Level);

        // Return the sub-phase id to dependency-level map used by branch ordering.
        return decisionDomains.ToDictionary(
            item => item.Decision.PredictedSubPhaseId,
            item => domainLevels.GetValueOrDefault(item.Domain!.Value, int.MaxValue),
            StringComparer.Ordinal);
    }

    // Detect the explicit E35 evidence markers that opt branch ordering into DependencyGraph.
    private static bool HasDependencyGraphEvidence(SubPhaseRoutingDecision[] decisions)
    {
        // Evidence can arrive from the experiment id or the production graph marker.
        return decisions.Any(decision => decision.EvidencePredicates.Any(predicate =>
            string.Equals(predicate, "E35_DEPENDENCY_GRAPH", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(predicate, "DEPENDENCY_GRAPH", StringComparison.OrdinalIgnoreCase)));
    }

    // Infer the E35 graph domain for one routed sub-phase from topology predicates and lifecycle placement.
    private Domain? TryInferDependencyDomain(SubPhaseRoutingDecision decision)
    {
        // Resolve topology because sub-phase metadata is the source of truth for lifecycle role.
        var resolved = _topology!.ResolveSubPhase(decision.PredictedSubPhaseId);
        if (!resolved.IsSuccess)
            return null;

        // Merge topology predicates, routing evidence, and human labels for robust matching.
        var subPhase = resolved.Value!.SubPhase;
        var labels = subPhase.PredicateIds
            .Concat(decision.EvidencePredicates)
            .Concat([subPhase.PhaseId, subPhase.Name, subPhase.Purpose])
            .Select(label => label.ToUpperInvariant())
            .ToArray();

        // Functional specification and documentation are planning work in the E35 graph.
        if (subPhase.PhaseId == "2" || labels.Any(label => label.Contains("DOCUMENT", StringComparison.Ordinal) || label.Contains("PLAN", StringComparison.Ordinal)))
            return Domain.PLAN;

        // Architecture, contracts, and security all belong before implementation.
        if (subPhase.PhaseId == "3" || labels.Any(label => label.Contains("ARCHITECTURE", StringComparison.Ordinal) || label.Contains("API_CONTRACT", StringComparison.Ordinal) || label.Contains("SECURITY", StringComparison.Ordinal) || label.Contains("AUTHORIZATION", StringComparison.Ordinal)))
            return Domain.ARCH;

        // Refactoring has a distinct domain edge from architecture.
        if (labels.Any(label => label.Contains("REFACTOR", StringComparison.Ordinal)))
            return Domain.REFACTOR;

        // Implementation branches are code-understanding/code-writing work in the E35 graph.
        if (subPhase.PhaseId == "4" || labels.Any(label => label.Contains("IMPLEMENT", StringComparison.Ordinal)))
            return Domain.CODE_Q;

        // Testing and QA validate code changes, so they are downstream of code understanding like BUG work.
        if (subPhase.PhaseId == "5" || subPhase.PhaseId == "6" || labels.Any(label => label.Contains("TEST", StringComparison.Ordinal) || label.Contains("QA", StringComparison.Ordinal) || label.Contains("BUG", StringComparison.Ordinal) || label.Contains("FAIL", StringComparison.Ordinal)))
            return Domain.BUG;

        // Unmapped domains stay out of E35 ordering.
        return null;
    }

    // Create the canonical E35 graph without leaking storage state between orchestrator calls.
    private static SemanticCompiler29.DependencyGraph BuildDependencyGraph()
    {
        // BuildDefault owns the canonical PLAN -> ARCH -> CODE_Q -> BUG and ARCH -> REFACTOR edges.
        return SemanticCompiler29.DependencyGraph.BuildDefault(new DependencyGraphStorage(), new DependencyGraphLogger());
    }

    // Add E49 bridge decisions for sub-phases that sit on topology paths between routed endpoints.
    private SubPhaseRoutingDecision[] ExpandRoutedDecisions(SubPhaseRoutingDecision[] decisions)
        => ExpandRoutedDecisions(decisions, BuildDynamicDomainTransitions());

    // Expand against the invocation-local structural view, including parallel-eligible bridge relations.
    private SubPhaseRoutingDecision[] ExpandRoutedDecisions(
        SubPhaseRoutingDecision[] decisions,
        DynamicDomainTransition[] structuralTransitions)
    {
        if (decisions.Length < 2)
            return decisions;

        var routedIds = decisions.Select(decision => decision.PredictedSubPhaseId).ToHashSet(StringComparer.Ordinal);
        var bridgeIds = OrchestratorExpander.FindBridgeSubPhases(routedIds, structuralTransitions)
            .Where(id => !routedIds.Contains(id))
            .ToArray();
        if (bridgeIds.Length == 0)
            return CapRoutedDecisions(decisions);

        var expanded = decisions.ToList();
        var template = decisions[0];
        foreach (var bridgeId in bridgeIds)
        {
            var subPhase = _topology!.ResolveSubPhase(bridgeId);
            if (!subPhase.IsSuccess)
                continue;

            expanded.Add(new SubPhaseRoutingDecision(
                template.DocumentId,
                template.SourceStatus,
                subPhase.Value!.SubPhase.PhaseId,
                bridgeId,
                0.71,
                0.0,
                ["TOPOLOGY_BRIDGE", .. subPhase.Value.SubPhase.PredicateIds],
                false,
                "source_expanded_by_topology_bridge")
            {
                AuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
            });
        }

        return CapRoutedDecisions(expanded.ToArray());
    }

    // Preserve direct router choices first, then keep only the strongest topology bridge branches.
    private static SubPhaseRoutingDecision[] CapRoutedDecisions(SubPhaseRoutingDecision[] decisions)
    {
        if (decisions.Length <= MaxAdaptiveBranchCount)
            return decisions;

        return decisions
            .Select((decision, index) => new { Decision = decision, Index = index })
            .OrderByDescending(item => !item.Decision.EvidencePredicates.Contains("TOPOLOGY_BRIDGE", StringComparer.Ordinal))
            .ThenByDescending(item => item.Decision.Confidence)
            .ThenBy(item => item.Decision.PredictedSubPhaseId, StringComparer.Ordinal)
            .Take(MaxAdaptiveBranchCount)
            .OrderBy(item => item.Index)
            .Select(item => item.Decision)
            .ToArray();
    }

    // Adapt the topology registry into the project 17 dynamic-domain graph contract.
    private DynamicDomainTransition[] BuildDynamicDomainTransitions()
    {
        var transitions = _topology!.ListTransitions();
        if (!transitions.IsSuccess)
            return [];

        return transitions.Value!
            .Select(entry => entry.Transition)
            .Where(transition =>
                string.Equals(transition.FromTopologyKind, "sub-phase", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(transition.ToTopologyKind, "sub-phase", StringComparison.OrdinalIgnoreCase))
            .Select(transition => new DynamicDomainTransition(transition.FromTopologyId, transition.ToTopologyId))
            .ToArray();
    }

    // Read one authoritative dynamic snapshot or build one invocation-local legacy fallback view.
    private OperationResult<DynamicGraphTransitionView> ReadTransitionView(string runId)
    {
        if (_dynamicGraphNodeOutputSourceRequested)
        {
            if (!IsCanonicalNodeOutputValue(runId))
            {
                return OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph node-output run identity is invalid.",
                    NodeOutputRunIdInvalidCode);
            }

            if (_dynamicGraphNodeOutputSource is null)
            {
                return OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph node-output source is required.",
                    NodeOutputInvocationSourceRequiredCode);
            }
        }

        if (_dynamicGraphTopologySource is null)
        {
            if (_dynamicGraphNodeOutputSourceRequested)
            {
                return OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph topology source is required for node-output projection.",
                    NodeOutputTopologySourceRequiredCode);
            }

            return OperationResult<DynamicGraphTransitionView>.Ok(BuildLegacyTransitionView([]));
        }

        OperationResult<DynamicGraphTopologyRuntimeSnapshot>? read;
        try
        {
            read = _dynamicGraphTopologySource.ReadCurrent();
        }
        catch (Exception exception) when (
            _dynamicGraphNodeOutputSourceRequested
            && IsNonFatalNodeOutputSourceException(exception))
        {
            return OperationResult<DynamicGraphTransitionView>.Fail(
                "Dynamic graph topology is unavailable for node-output projection.",
                NodeOutputTopologyUnavailableCode);
        }
        catch (Exception exception) when (
            !_dynamicGraphNodeOutputSourceRequested
            && IsNonFatalNodeOutputSourceException(exception))
        {
            return OperationResult<DynamicGraphTransitionView>.Ok(
                BuildLegacyTransitionView([BuildFallbackWarning("SOURCE_EXCEPTION")]));
        }

        if (read is null)
        {
            if (_dynamicGraphNodeOutputSourceRequested)
            {
                return OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph topology is unavailable for node-output projection.",
                    NodeOutputTopologyUnavailableCode);
            }

            return OperationResult<DynamicGraphTransitionView>.Ok(
                BuildLegacyTransitionView([BuildFallbackWarning("SOURCE_RESULT_REQUIRED")]));
        }

        if (!read.IsSuccess)
        {
            if (_dynamicGraphNodeOutputSourceRequested)
            {
                return OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph topology is unavailable for node-output projection.",
                    NodeOutputTopologyUnavailableCode);
            }

            return OperationResult<DynamicGraphTransitionView>.Ok(
                BuildLegacyTransitionView([BuildFallbackWarning(NormalizeSourceCode(read.ErrorCode))]));
        }

        return ValidateAndBuildTransitionView(
            runId,
            read.Value,
            _dynamicGraphNodeOutputSource,
            _dynamicGraphNodeOutputSourceRequested);
    }

    // Validate a supplied snapshot without consulting the constructor-injected topology source.
    private OperationResult<DynamicGraphTransitionView> ReadProvidedTransitionView(
        string runId,
        DynamicGraphTopologyRuntimeSnapshot topologySnapshot,
        IDynamicGraphNodeOutputSource nodeOutputSource)
    {
        try
        {
            return ValidateAndBuildTransitionView(
                runId,
                topologySnapshot,
                nodeOutputSource,
                requested: true);
        }
        catch (Exception exception) when (IsNonFatalNodeOutputSourceException(exception))
        {
            return OperationResult<DynamicGraphTransitionView>.Fail(
                "Dynamic graph topology is invalid for node-output projection.",
                NodeOutputTopologyInvalidCode);
        }
    }

    // Build one transition view from an already selected snapshot and exact invocation source.
    private OperationResult<DynamicGraphTransitionView> ValidateAndBuildTransitionView(
        string runId,
        DynamicGraphTopologyRuntimeSnapshot? snapshot,
        IDynamicGraphNodeOutputSource? nodeOutputSource,
        bool requested)
    {
        if (requested)
        {
            if (!IsCanonicalNodeOutputValue(runId))
            {
                return OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph node-output run identity is invalid.",
                    NodeOutputRunIdInvalidCode);
            }

            if (nodeOutputSource is null)
            {
                return OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph node-output source is required.",
                    NodeOutputInvocationSourceRequiredCode);
            }
        }

        if (snapshot is null)
        {
            return OperationResult<DynamicGraphTransitionView>.Fail(
                !requested
                    ? "Dynamic graph L4 topology snapshot is required."
                    : "Dynamic graph topology snapshot is required for node-output projection.",
                !requested
                    ? "DYNAMIC_GRAPH_L4_TOPOLOGY_SNAPSHOT_REQUIRED"
                    : NodeOutputTopologySnapshotRequiredCode);
        }

        if (snapshot.Revision <= 0)
        {
            return OperationResult<DynamicGraphTransitionView>.Fail(
                !requested
                    ? "Dynamic graph L4 topology revision must be positive."
                    : "Dynamic graph topology revision is invalid for node-output projection.",
                !requested
                    ? "DYNAMIC_GRAPH_L4_TOPOLOGY_REVISION_INVALID"
                    : NodeOutputTopologyRevisionInvalidCode);
        }

        if (snapshot.Topology is null && requested)
        {
            return OperationResult<DynamicGraphTransitionView>.Fail(
                "Dynamic graph topology is invalid for node-output projection.",
                NodeOutputTopologyInvalidCode);
        }

        var validation = DynamicGraphValidation.Validate(snapshot.Topology);
        if (!validation.IsSuccess)
        {
            return !requested
                ? OperationResult<DynamicGraphTransitionView>.Fail(validation.ErrorMessage, validation.ErrorCode)
                : OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph topology is invalid for node-output projection.",
                    NodeOutputTopologyInvalidCode);
        }
        var stableTopology = validation.Value!;

        var subPhases = _topology!.ListSubPhases();
        if (subPhases is null)
        {
            return OperationResult<DynamicGraphTransitionView>.Fail(
                !requested
                    ? "Logical topology sub-phase result is required."
                    : "Dynamic graph topology registry validation failed for node-output projection.",
                !requested
                    ? "DYNAMIC_GRAPH_L4_TOPOLOGY_SUB_PHASE_RESULT_REQUIRED"
                    : NodeOutputTopologyInvalidCode);
        }

        if (!subPhases.IsSuccess)
        {
            return !requested
                ? OperationResult<DynamicGraphTransitionView>.Fail(subPhases.ErrorMessage, subPhases.ErrorCode)
                : OperationResult<DynamicGraphTransitionView>.Fail(
                    "Dynamic graph topology registry validation failed for node-output projection.",
                    NodeOutputTopologyInvalidCode);
        }
        if (subPhases.Value is null)
        {
            return OperationResult<DynamicGraphTransitionView>.Fail(
                !requested
                    ? "Logical topology sub-phase result is required."
                    : "Dynamic graph topology registry validation failed for node-output projection.",
                !requested
                    ? "DYNAMIC_GRAPH_L4_TOPOLOGY_SUB_PHASE_RESULT_REQUIRED"
                    : NodeOutputTopologyInvalidCode);
        }

        var registeredSubPhaseIds = subPhases.Value
            .Select(entry => entry.SubPhase.SubPhaseId)
            .ToHashSet(StringComparer.Ordinal);
        var unknownNode = stableTopology.Nodes
            .Select(node => node.NodeId)
            .FirstOrDefault(nodeId => !registeredSubPhaseIds.Contains(nodeId));
        if (unknownNode is not null)
        {
            return OperationResult<DynamicGraphTransitionView>.Fail(
                !requested
                    ? $"Dynamic graph L4 topology node '{unknownNode}' is not registered as a logical sub-phase."
                    : "Dynamic graph topology contains an unregistered node and cannot accept node outputs.",
                !requested
                    ? "DYNAMIC_GRAPH_L4_TOPOLOGY_NODE_NOT_REGISTERED"
                    : NodeOutputTopologyInvalidCode);
        }

        NodeOutputToEdgeInputAdapter? nodeOutputProjection = null;
        if (requested)
        {
            var projected = ReadNodeOutputProjection(runId, snapshot, nodeOutputSource!);
            if (!projected.IsSuccess)
                return OperationResult<DynamicGraphTransitionView>.Fail(projected.ErrorMessage, projected.ErrorCode);

            nodeOutputProjection = projected.Value;
        }

        return OperationResult<DynamicGraphTransitionView>.Ok(
            BuildDynamicTransitionView(
                stableTopology,
                snapshot.Revision,
                nodeOutputProjection));
    }

    // Validate one invocation-scoped node-output snapshot and project it exactly once into the L2-to-L3 carrier.
    private OperationResult<NodeOutputToEdgeInputAdapter> ReadNodeOutputProjection(
        string runId,
        DynamicGraphTopologyRuntimeSnapshot topologySnapshot,
        IDynamicGraphNodeOutputSource nodeOutputSource)
    {
        OperationResult<DynamicGraphNodeOutputSnapshot>? read;
        try
        {
            read = nodeOutputSource.ReadForInvocation(runId);
        }
        catch (Exception exception) when (IsNonFatalNodeOutputSourceException(exception))
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output source failed.",
                NodeOutputSourceExceptionCode);
        }

        if (read is null)
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output source result is required.",
                NodeOutputSourceResultRequiredCode);
        }

        if (!read.IsSuccess)
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output source failed.",
                NodeOutputSourceFailedCode);
        }

        var snapshot = read.Value;
        if (snapshot is null)
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output snapshot is required.",
                NodeOutputSnapshotRequiredCode);
        }

        if (!IsCanonicalNodeOutputValue(runId) || !IsCanonicalNodeOutputValue(snapshot.RunId))
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output run identity is invalid.",
                NodeOutputRunIdInvalidCode);
        }

        if (!StringComparer.Ordinal.Equals(snapshot.RunId, runId))
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output run identity does not match the invocation.",
                NodeOutputRunIdMismatchCode);
        }

        if (snapshot.TopologyRevision <= 0)
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output topology revision must be positive.",
                NodeOutputRevisionInvalidCode);
        }

        if (snapshot.TopologyRevision != topologySnapshot.Revision)
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output topology revision does not match the invocation topology.",
                NodeOutputRevisionMismatchCode);
        }

        if (!IsCanonicalNodeOutputValue(snapshot.SourceNodeId))
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output source-node identity is invalid.",
                NodeOutputSourceNodeInvalidCode);
        }

        var sourceNodeCount = topologySnapshot.Topology.Nodes.Count(node =>
            node is not null
            && StringComparer.Ordinal.Equals(node.NodeId, snapshot.SourceNodeId));
        if (sourceNodeCount == 0)
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output source node is not present in the invocation topology.",
                NodeOutputSourceNodeNotFoundCode);
        }

        if (sourceNodeCount != 1)
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output source node is ambiguous in the invocation topology.",
                NodeOutputSourceNodeAmbiguousCode);
        }

        // The immutable source snapshot is read once and the fixed projector is invoked once.
        var candidates = snapshot.Candidates;
        if (candidates.Any(candidate =>
                !StringComparer.Ordinal.Equals(candidate.UnitId, snapshot.SourceNodeId)))
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output candidate does not match the source node.",
                NodeOutputCandidateSourceNodeMismatchCode);
        }

        var projected = _dynamicGraphNodeOutputProjector.Project(snapshot.SourceNodeId, candidates);
        if (projected is null || (projected.IsSuccess && projected.Value is null))
        {
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(
                "Dynamic graph node-output projection result is required.",
                NodeOutputProjectionResultRequiredCode);
        }

        // The deterministic in-process projector owns these stable validation messages and codes.
        if (!projected.IsSuccess)
            return OperationResult<NodeOutputToEdgeInputAdapter>.Fail(projected.ErrorMessage, projected.ErrorCode);

        return OperationResult<NodeOutputToEdgeInputAdapter>.Ok(projected.Value!);
    }

    // Follow-up prompts reuse the exact invocation topology view but never replay the original node outputs.
    private static DynamicGraphTransitionView BuildInvocationFollowUpTransitionView(
        DynamicGraphTransitionView source)
        => new(
            source.StructuralTransitions.ToArray(),
            source.DependencyTransitions.ToArray(),
            source.Warnings.ToArray(),
            source.Topology,
            source.TopologyRevision,
            null);

    // Snapshot legacy logical transitions once; absence of an L4 source preserves the historical no-warning path.
    private DynamicGraphTransitionView BuildLegacyTransitionView(string[] warnings)
    {
        var transitions = BuildDynamicDomainTransitions();
        return new DynamicGraphTransitionView(
            transitions,
            transitions.ToArray(),
            warnings.ToArray(),
            null,
            null,
            null);
    }

    // Expand multi-target edges, deduplicate endpoint pairs, and sort by topology order then ordinal identity.
    private static DynamicGraphTransitionView BuildDynamicTransitionView(
        DynamicGraphTopology topology,
        long topologyRevision,
        NodeOutputToEdgeInputAdapter? nodeOutputProjection)
    {
        var positions = topology.Order.ToDictionary(entry => entry.NodeId, entry => entry.Position, StringComparer.Ordinal);
        var pairs = topology.Edges
            .Where(edge => edge.Kind != GraphEdgeKind.ReturnLoop)
            .SelectMany(edge => edge.ToNodeIds.Select(target => new DynamicGraphTransitionPair(
                edge.FromNodeId,
                target,
                edge.ParallelEligible)))
            .ToArray();

        DynamicDomainTransition[] Materialize(IEnumerable<DynamicGraphTransitionPair> source)
            => source
                .GroupBy(pair => new DynamicGraphTransitionEndpoint(pair.FromId, pair.ToId))
                .Select(group => group.Key)
                .OrderBy(pair => positions[pair.FromId])
                .ThenBy(pair => positions[pair.ToId])
                .ThenBy(pair => pair.FromId, StringComparer.Ordinal)
                .ThenBy(pair => pair.ToId, StringComparer.Ordinal)
                .Select(pair => new DynamicDomainTransition(pair.FromId, pair.ToId))
                .ToArray();

        var structuralTransitions = Materialize(pairs);
        var dependencyTransitions = Materialize(pairs.Where(pair => !pair.ParallelEligible));
        return new DynamicGraphTransitionView(
            structuralTransitions,
            dependencyTransitions,
            [],
            topology,
            topologyRevision,
            nodeOutputProjection);
    }

    private static string BuildFallbackWarning(string sourceCode)
        => $"DYNAMIC_GRAPH_L4_TOPOLOGY_FALLBACK:{sourceCode}";

    private static string NormalizeSourceCode(string? sourceCode)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            return "SOURCE_FAILED";

        var normalized = new string(sourceCode.Trim().ToUpperInvariant()
            .Select(character => char.IsAsciiLetterOrDigit(character) || character == '_' ? character : '_')
            .ToArray());
        return string.IsNullOrWhiteSpace(normalized) ? "SOURCE_FAILED" : normalized;
    }

    // Build the aggregate assistant-level result from branch results.
    private static LogicalOrchestratorResult BuildAggregateResult(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        LogicalFlowResult[] branches,
        string[]? warnings = null)
    {
        // Use the first branch for backward compatibility.
        var primary = branches[0];

        // Aggregate verdicts across branches.
        var overallVerdict = branches.Select(branch => branch.OverallVerdict).OrderByDescending(VerdictRank).First();

        // Combine human-readable branch results.
        var finalHumanResult = branches.Length == 1
            ? primary.FinalHumanResult
            : string.Join(Environment.NewLine, branches.Select(branch => $"[{branch.ScenarioId}] {branch.FinalHumanResult}"));
        var aggregateWarnings = branches
            .SelectMany(branch => branch.Warnings)
            .Concat(warnings ?? [])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        // Return the aggregate result with all branches attached.
        return new LogicalOrchestratorResult(
            primary.ScenarioId,
            primary.FlowId,
            overallVerdict,
            finalHumanResult,
            flags,
            primary)
        {
            BranchResults = branches,
            Warnings = aggregateWarnings,
            OutputContextPacket = flags.UseContextCarryForward ? BuildContextPacket(request.RunId, branches) : null
        };
    }

    public static LogicalOrchestratorRequest ContinueFrom(
        LogicalOrchestratorResult prior,
        string runId,
        string userRequest,
        string scenarioId = ScenarioId)
    {
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal);
        if (prior.OutputContextPacket is not null)
        {
            metadata["context.packet"] = prior.OutputContextPacket;
            metadata[ContextPacketReader.ContextSignalsMetadataKey] = BuildContextSignals(prior.OutputContextPacket);
        }

        return new LogicalOrchestratorRequest(
            runId,
            scenarioId,
            userRequest,
            prior.AppliedFlags,
            metadata);
    }

    private static int ReadTransitionDepth(IReadOnlyDictionary<string, object> metadata)
    {
        if (!metadata.TryGetValue("transition.depth", out var value))
            return 0;

        return value switch
        {
            int intValue => intValue,
            string text when int.TryParse(text, out var parsed) => parsed,
            _ => 0
        };
    }

    // Assign verdict severity for aggregation.
    private static int VerdictRank(string verdict)
    {
        // Rank known verdicts from strongest failure to clean pass.
        return verdict switch
        {
            "REJECT" => 6,
            "REPROCESS" => 5,
            "KNOWLEDGE_EDGE" => 4,
            "EDGE_AUTHORITY_GAP_CONSUMED" => 3,
            "PASS_WITH_GAPS" => 3,
            "PASS_WITH_CONCERNS" => 2,
            "CONCERN" => 2,
            "PASS" => 1,
            "TARGET_HANDOFF_CONSUMED" => -1,
            _ => 0
        };
    }

    // Queue learning events from knowledge-edge branches.
    private void ScheduleKnowledgeEdges(LogicalExecutionFlags flags, LogicalFlowResult[] branches)
    {
        // Nothing queues when the scheduler is absent or the safety flag is disabled.
        if (_learningEventScheduler is null || !flags.UseKnowledgeEdgeDetection)
            return;

        // Inspect every branch for knowledge-edge evidence.
        foreach (var branch in branches)
        {
            // Detect knowledge-edge branch shape.
            var isKnowledgeEdge = IsKnowledgeEdgeBranch(branch);

            // Skip non-gap branches.
            if (!isKnowledgeEdge)
                continue;

            // Use missing sub-phase as the domain key when present.
            var domain = branch.Metrics.TryGetValue("missing_sub_phase", out var subPhase) && !string.IsNullOrWhiteSpace(subPhase)
                ? subPhase
                : branch.ScenarioId;

            // Queue one pending example; the scheduler threshold decides whether it is ready.
            _learningEventScheduler.QueueIfReady(domain, 1, $"Knowledge edge observed in flow '{branch.FlowId}'.");
        }
    }

    // Detect knowledge-edge branch shape in one place so scheduling and retry agree.
    private static bool IsKnowledgeEdgeBranch(LogicalFlowResult branch)
    {
        return
            string.Equals(branch.OverallVerdict, "KNOWLEDGE_EDGE", StringComparison.Ordinal) ||
            string.Equals(branch.OverallVerdict, "PASS_WITH_GAPS", StringComparison.Ordinal) ||
            branch.Warnings.Any(warning => warning.Contains("knowledge_edge", StringComparison.OrdinalIgnoreCase)) ||
            (branch.Metrics.TryGetValue("knowledge_edge", out var metric) && string.Equals(metric, "True", StringComparison.OrdinalIgnoreCase));
    }

    // Execute one binding using the scenario id supplied by the registry when available.
    private static async Task<OperationResult<LogicalFlowResult>> ExecuteBindingAsync(
        LogicalOrchestratorRequest request,
        LogicalExecutionFlags flags,
        LogicalFlowBinding binding,
        SubPhaseRoutingDecision? decision,
        IReadOnlyList<LogicalFlowResult> completedBranches,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

        try
        {
            // Copy metadata so route evidence can be added without mutating the caller dictionary.
            var metadata = new Dictionary<string, object>(request.Metadata, StringComparer.Ordinal);

            // Attach route decision metadata when adaptive routing produced one.
            if (decision is not null)
            {
                if (!RoutingAuthorityVocabulary.IsCanonicalRole(decision.AuthorityRole))
                {
                    return OperationResult<LogicalFlowResult>.Fail(
                        "Logical flow execution result is invalid.",
                        FlowResultInvalidCode);
                }

                metadata["routed_phase_id"] = decision.PredictedPhaseId;
                metadata["routed_sub_phase_id"] = decision.PredictedSubPhaseId;
                metadata["routing_confidence"] = decision.Confidence;
                metadata[RoutingAuthorityVocabulary.MetadataKey] = decision.AuthorityRole;
            }

            // Carry upstream branch results into dependent downstream branches when enabled.
            if (flags.UseContextCarryForward && completedBranches.Count > 0)
            {
                // Attach the typed context packet first so downstream units have a stable carry-forward contract.
                var contextPacket = BuildContextPacket(request.RunId, completedBranches);
                metadata["context.packet"] = contextPacket;
                metadata[ContextPacketReader.ContextSignalsMetadataKey] = BuildContextSignals(contextPacket);

                // Keep the original keys for existing tests and transitional callers.
                metadata["context.branch_results"] = completedBranches.ToArray();
                metadata["context.source_branch_ids"] = completedBranches.Select(branch => branch.ScenarioId).ToArray();
            }

            // E38V2 exposes skill-retriever evidence to selected flows without changing the default path.
            if (flags.UseSkillRetriever)
            {
                var retrieval = new SkillRetriever().Retrieve(request.UserRequest);
                metadata["skill_retriever.is_fallback"] = retrieval.IsFallback;
                metadata["skill_retriever.selected_skills"] = retrieval.Skills.Select(skill => skill.SkillName).ToArray();
                metadata["skill_retriever.knn_signal"] = retrieval.IsFallback ? 0.0 : 1.0;
            }

            // Use the registry scenario id when it exists because flow adapters validate it.
            var scenarioId = string.IsNullOrWhiteSpace(binding.ScenarioId)
                ? request.ScenarioId
                : binding.ScenarioId;

            // Build the flow request.
            var flowRequest = new LogicalFlowRequest<string>(
                request.RunId,
                binding.FlowId,
                scenarioId,
                request.UserRequest,
                flags,
                metadata);

            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

            // Capture the task separately so a broken flow cannot turn a missing result into a null-await exception.
            Task<OperationResult<LogicalFlowResult>>? executionTask =
                binding.Flow.ExecuteAsync(flowRequest, cancellationToken);

            if (executionTask is null)
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution result is required.",
                    FlowResultRequiredCode);
            }

            var executed = await executionTask;
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

            if (executed is null)
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution result is required.",
                    FlowResultRequiredCode);
            }

            // Preserve declared failures exactly; only malformed successful envelopes are normalized locally.
            if (!executed.IsSuccess)
                return OperationResult<LogicalFlowResult>.Fail(executed.ErrorMessage, executed.ErrorCode);

            if (executed.Value is null)
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution result is required.",
                    FlowResultRequiredCode);
            }

            // Own the dependency result before inspecting or enriching any of its mutable members.
            if (!LogicalMetadataSnapshot.TryCreate(executed.Value, out var stableResult))
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution result is invalid.",
                    FlowResultInvalidCode);
            }

            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

            if (!StringComparer.Ordinal.Equals(stableResult.FlowId, flowRequest.FlowId)
                || !StringComparer.Ordinal.Equals(stableResult.ScenarioId, flowRequest.ScenarioId))
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution result identity does not match the request.",
                    FlowResultIdentityMismatchCode);
            }

            if (!HasValidL4TransitionSignals(stableResult.TransitionSignals))
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution result is invalid.",
                    FlowResultInvalidCode);
            }

            // Attach the prompt domain to the owned result without changing the established metric semantics.
            var enrichedResult = EnrichFlowResultWithDomain(stableResult, metadata);

            // Bind ordinary routed results to the admitted route; typed handoffs own their correlation separately.
            enrichedResult = EnrichFlowResultWithRouteAuthority(enrichedResult, decision);

            // Enrichment creates a mutable metrics dictionary, so snapshot again before crossing the L4 boundary.
            if (!LogicalMetadataSnapshot.TryCreate(enrichedResult, out var finalResult))
            {
                if (cancellationToken.IsCancellationRequested)
                    return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

                return OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution result is invalid.",
                    FlowResultInvalidCode);
            }

            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

            return OperationResult<LogicalFlowResult>.Ok(finalResult);
        }
        catch (OperationCanceledException)
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution failed unexpectedly.",
                    FlowExecutionFailedCode);
        }
        catch (Exception exception) when (IsNonFatalL4Exception(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<LogicalFlowResult>.Fail(
                    "Logical flow execution failed unexpectedly.",
                    FlowExecutionFailedCode);
        }
    }

    // Build a compact context packet from completed upstream branch results.
    private static LogicalContextPacket BuildContextPacket(string runId, IReadOnlyList<LogicalFlowResult> completedBranches)
    {
        // Convert each branch into a durable context fragment.
        var fragments = completedBranches.Select(branch => new LogicalContextFragment(
            branch.FlowId,
            branch.ScenarioId,
            branch.OverallVerdict,
            branch.FinalHumanResult,
            branch.Metrics.TryGetValue("routed_phase_id", out var phaseId) ? phaseId : null,
            branch.Metrics.TryGetValue("routed_sub_phase_id", out var routedSubPhaseId)
                ? routedSubPhaseId
                : branch.Metrics.TryGetValue("sub_phase_id", out var branchSubPhaseId) ? branchSubPhaseId : null,
            branch.Metrics,
            branch.EvidenceIds,
            branch.Warnings)).ToArray();

        // Return the typed packet used by downstream branches and future training examples.
        return new LogicalContextPacket(
            runId,
            fragments.Select(fragment => fragment.FlowId).ToArray(),
            fragments.Select(fragment => fragment.ScenarioId).ToArray(),
            fragments,
            DateTimeOffset.UtcNow);
    }

    // Convert the project-21 context packet into project-23 context signals without reflection.
    private static ContextSignals BuildContextSignals(LogicalContextPacket packet)
    {
        var priorDomains = new List<SemanticCompiler29.Domain>();
        var completedSubPhases = new List<string>();
        var activeKnowledgeEdges = new List<string>();
        var confidenceMap = new Dictionary<SemanticCompiler29.Domain, double>();

        foreach (var fragment in packet.Fragments)
        {
            if ((string.Equals(fragment.OverallVerdict, "PASS", StringComparison.Ordinal) ||
                 string.Equals(fragment.OverallVerdict, "PASS_WITH_CONCERNS", StringComparison.Ordinal) ||
                 string.Equals(fragment.OverallVerdict, "CONCERN", StringComparison.Ordinal)) &&
                ContextPacketReader.TryReadDomain(fragment.Metrics, out var domain))
            {
                priorDomains.Add(domain);
                if (ContextPacketReader.TryReadConfidence(fragment.Metrics, out var confidence))
                    confidenceMap[domain] = Math.Max(confidenceMap.TryGetValue(domain, out var existing) ? existing : 0.0, confidence);
            }

            if (string.Equals(fragment.OverallVerdict, "PASS", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(fragment.SubPhaseId))
                completedSubPhases.Add(fragment.SubPhaseId!);

            foreach (var evidenceId in fragment.EvidenceIds)
            {
                if (!evidenceId.StartsWith("knowledge_edge:", StringComparison.OrdinalIgnoreCase))
                    continue;

                activeKnowledgeEdges.Add(evidenceId);
                var edgeSubPhase = ExtractSubPhaseIdFromEvidence(evidenceId);
                if (!string.IsNullOrWhiteSpace(edgeSubPhase))
                    activeKnowledgeEdges.Add(edgeSubPhase!);
            }

            if (fragment.Metrics.TryGetValue("knowledge_edge", out var knowledgeEdge) &&
                string.Equals(knowledgeEdge, bool.TrueString, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(fragment.SubPhaseId))
                activeKnowledgeEdges.Add(fragment.SubPhaseId!);
        }

        return new ContextSignals(
            priorDomains.Distinct().ToArray(),
            completedSubPhases.Distinct(StringComparer.Ordinal).ToArray(),
            activeKnowledgeEdges.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            confidenceMap);
    }

    private static string? ExtractSubPhaseIdFromEvidence(string evidenceId)
    {
        var tokens = evidenceId.Split([' ', '=', ':', ';', ',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return tokens.FirstOrDefault(token => token.Length == 3 && char.IsDigit(token[0]) && token[1] == '.' && char.IsDigit(token[2]));
    }

    // Build a synthetic knowledge-edge flow result for missing topology or missing flow capability.
    private static LogicalFlowResult BuildKnowledgeEdgeFlowResult(
        LogicalOrchestratorRequest request,
        SubPhaseRoutingDecision decision,
        string reason,
        bool missingFlow)
    {
        // Build the required knowledge-edge metrics.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["knowledge_edge"] = true.ToString(),
            ["missing_sub_phase"] = decision.PredictedSubPhaseId,
            ["missing_flow"] = missingFlow.ToString(),
            ["routed_phase_id"] = decision.PredictedPhaseId,
            ["routed_sub_phase_id"] = decision.PredictedSubPhaseId,
            ["routing_confidence"] = decision.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            ["knowledge_edge_reason"] = reason
        };

        // Build a compact human-visible gap statement.
        var finalHumanResult = $"Knowledge edge: routed to sub-phase '{decision.PredictedSubPhaseId}', but {reason}.";

        // Build the summary for the synthetic result.
        var summary = new LogicalFlowSummary(
            KnowledgeEdgeFlowId,
            1,
            0,
            0,
            "knowledge_edge",
            false,
            true,
            false,
            "SCHEDULE_CAPABILITY_LEARNING",
            decision.Confidence,
            reason,
            $"missing_sub_phase={decision.PredictedSubPhaseId}; missing_flow={missingFlow}",
            0,
            0);

        // Build the synthetic flow result.
        var flowResult = new LogicalFlowResult(
            KnowledgeEdgeFlowId,
            request.ScenarioId,
            "KNOWLEDGE_EDGE",
            finalHumanResult,
            summary,
            metrics,
            decision.EvidencePredicates,
            ["knowledge_edge"]);

        // Return the synthetic flow result with the same domain and routing-authority metrics real branches receive.
        return EnrichFlowResultWithRouteAuthority(
            EnrichFlowResultWithDomain(flowResult, request.Metadata),
            decision);
    }

    // Copy domain classification metadata into flow metrics for result-level traceability.
    private static LogicalFlowResult EnrichFlowResultWithDomain(
        LogicalFlowResult result,
        IReadOnlyDictionary<string, object> metadata)
    {
        if (!metadata.TryGetValue(PromptDomainClassifier.DomainLabelMetadataKey, out var domainLabel))
            return result;

        var metrics = new Dictionary<string, string>(result.Metrics, StringComparer.Ordinal)
        {
            [PromptDomainClassifier.DomainLabelMetadataKey] = Convert.ToString(domainLabel, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
        };

        if (metadata.TryGetValue(PromptDomainClassifier.DomainConfidenceMetadataKey, out var confidence))
            metrics[PromptDomainClassifier.DomainConfidenceMetadataKey] = Convert.ToString(confidence, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

        if (metadata.TryGetValue(PromptDomainClassifier.DomainEvidenceTermsMetadataKey, out var evidenceTerms))
        {
            metrics[PromptDomainClassifier.DomainEvidenceTermsMetadataKey] = evidenceTerms is string[] terms
                ? string.Join(",", terms)
                : Convert.ToString(evidenceTerms, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        }

        return result with { Metrics = metrics };
    }

    // Overwrite ordinary result route metrics from the admitted decision so a flow cannot forge route identity.
    private static LogicalFlowResult EnrichFlowResultWithRouteAuthority(
        LogicalFlowResult result,
        SubPhaseRoutingDecision? decision)
    {
        // Explicit scenarios have no adaptive decision, while typed handoffs use their closed correlation contract.
        if (decision is null || result.EdgeTargetHandoff is not null)
            return result;

        var metrics = new Dictionary<string, string>(result.Metrics, StringComparer.Ordinal)
        {
            ["routed_phase_id"] = decision.PredictedPhaseId,
            ["routed_sub_phase_id"] = decision.PredictedSubPhaseId,
            [RoutingAuthorityVocabulary.MetadataKey] = decision.AuthorityRole
        };

        return result with { Metrics = metrics };
    }

    // Read the selected prompt domain back from result metrics so capability training uses the same classifier decision as routing.
    private static bool TryReadDomainFromResult(LogicalOrchestratorResult result, out SemanticCompiler29.Domain domain)
    {
        if (result.FlowResult.Metrics.TryGetValue(PromptDomainClassifier.DomainLabelMetadataKey, out var primaryDomain) &&
            Enum.TryParse(primaryDomain, ignoreCase: true, out domain))
            return true;

        foreach (var branch in result.BranchResults)
        {
            if (branch.Metrics.TryGetValue(PromptDomainClassifier.DomainLabelMetadataKey, out var branchDomain) &&
                Enum.TryParse(branchDomain, ignoreCase: true, out domain))
                return true;
        }

        domain = SemanticCompiler29.Domain.CODE_Q;
        return false;
    }

    // This storage implementation satisfies the project-29 graph constructor without persisting graph state.
    private sealed class DependencyGraphStorage : SemanticCompiler29.ICompilerStorage
    {
        // DependencyGraph does not read storage during default sorting, but Save keeps the interface honest.
        private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);

        // Store compiler outputs by key for any future graph diagnostics.
        public void Save(string key, SemanticCompiler29.PipelineOutput output)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _items[key] = output;
        }

        // Load compiler outputs by key without throwing when the key is missing.
        public SemanticCompiler29.PipelineOutput? Load(string key)
            => string.IsNullOrWhiteSpace(key) ? null : _items.GetValueOrDefault(key);
    }

    // This logger satisfies the project-29 graph constructor while keeping orchestrator ordering side-effect free.
    private sealed class DependencyGraphLogger : SemanticCompiler29.ICompilerEventLogger
    {
        // Dependency sorting is deterministic and needs no runtime event sink in this adapter.
        public void Log(SemanticCompiler29.StandardEvent evt)
        {
        }
    }

    // Validate constructor dependencies.
    private string? ValidateConstruction()
    {
        // Topology is required for route validation.
        if (_topology is null)
            return "Topology registry is required.";

        // Prompt compiler is required before router execution.
        if (_promptCompiler is null)
            return "Prompt routing input compiler is required.";

        // Router is required for adaptive routing.
        if (_router is null)
            return "Sub-phase router is required.";

        // Flow registry is required for flow selection.
        if (_flows is null)
            return "Logical flow registry is required.";

        // Unit registry is required in the constructor so Phase C can resolve learned units without changing the surface.
        if (_units is null)
            return "Logical unit registry is required.";

        // Construction is valid.
        return null;
    }

    // Determine whether the request should use adaptive routing.
    private static bool IsAdaptiveScenario(string scenarioId)
    {
        // Empty scenario ids and the adaptive scenario id both select adaptive routing.
        return string.IsNullOrWhiteSpace(scenarioId) || string.Equals(scenarioId, ScenarioId, StringComparison.Ordinal);
    }
}

sealed record TransitionFollowUpResult(LogicalFlowResult[] Branches, string[] Warnings);

sealed record DynamicGraphTransitionView(
    DynamicDomainTransition[] StructuralTransitions,
    DynamicDomainTransition[] DependencyTransitions,
    string[] Warnings,
    DynamicGraphTopology? Topology,
    long? TopologyRevision,
    NodeOutputToEdgeInputAdapter? NodeOutputProjection);

sealed record RoutedLogicalFlowResult(
    SubPhaseRoutingDecision Decision,
    LogicalFlowResult Result);

sealed record ValidatedEdgeHandoff(
    RoutedLogicalFlowResult Source,
    DynamicGraphEdgeTargetHandoff Handoff,
    DynamicGraphEdgeTargetSelection[] OrderedTargets);

sealed record ResolvedEdgeTargetSubPhase(
    DynamicGraphEdgeTargetSelection Selection,
    SubPhaseRoutingDecision Decision);

sealed record ResolvedEdgeTarget(
    DynamicGraphEdgeTargetSelection Selection,
    SubPhaseRoutingDecision Decision,
    LogicalFlowBinding Binding,
    LogicalOrchestratorRequest Request);

sealed record DynamicGraphTransitionPair(
    string FromId,
    string ToId,
    bool ParallelEligible);

sealed record DynamicGraphTransitionEndpoint(
    string FromId,
    string ToId);
