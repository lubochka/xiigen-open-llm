// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// Bridges one normalized candidate batch into one invocation-scoped adaptive graph run.
public sealed class DynamicGraphCandidateInvocationOrchestrator
{
    private const string RuntimeRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_RUNTIME_REQUIRED";
    private const string RunnerRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_RUNNER_REQUIRED";
    private const string SourceNodeInvalidCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_SOURCE_NODE_INVALID";
    private const string SourceNodeRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_SOURCE_NODE_REQUIRED";
    private const string RequestProjectorRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_REQUEST_PROJECTOR_REQUIRED";
    private const string SourceNodeRegistrationFailedCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_SOURCE_NODE_REGISTRATION_FAILED";
    private const string SourceNodeRegistrationResultRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_SOURCE_NODE_REGISTRATION_RESULT_REQUIRED";
    private const string AlreadyStartedCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_ALREADY_STARTED";
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_REQUEST_REQUIRED";
    private const string RequestInvalidCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_REQUEST_INVALID";
    private const string CorrelationInvalidCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_CORRELATION_INVALID";
    private const string ScenarioMismatchCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_SCENARIO_MISMATCH";
    private const string QueryMismatchCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_QUERY_MISMATCH";
    private const string TopologyUnavailableCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_TOPOLOGY_UNAVAILABLE";
    private const string TopologySnapshotRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_TOPOLOGY_SNAPSHOT_REQUIRED";
    private const string TopologyRevisionInvalidCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_TOPOLOGY_REVISION_INVALID";
    private const string TopologyInvalidCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_TOPOLOGY_INVALID";
    private const string SourceNodeMismatchCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_SOURCE_NODE_MISMATCH";
    private const string RunnerExceptionCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_RUNNER_EXCEPTION";
    private const string RunnerResultRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_RUNNER_RESULT_REQUIRED";
    private const string RunnerFailedCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_RUNNER_FAILED";
    private const string BatchRequestMismatchCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_BATCH_REQUEST_MISMATCH";
    private const string BatchNotCleanCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_BATCH_NOT_CLEAN";
    private const string OutputInvalidCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_OUTPUT_INVALID";
    private const string OutputSourceNodeMismatchCode = "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_OUTPUT_SOURCE_NODE_MISMATCH";

    private readonly DynamicGraphLogicalOrchestratorRuntime _runtime;
    private readonly IGraphCandidateRunner _runner;
    private readonly string _sourceNodeId;
    private readonly DynamicGraphNodeDescriptor? _expectedSourceNodeDescriptor;
    private int _started;

    private DynamicGraphCandidateInvocationOrchestrator(
        DynamicGraphLogicalOrchestratorRuntime runtime,
        IGraphCandidateRunner runner,
        string sourceNodeId,
        DynamicGraphNodeDescriptor? expectedSourceNodeDescriptor)
    {
        _runtime = runtime;
        _runner = runner;
        _sourceNodeId = sourceNodeId;
        _expectedSourceNodeDescriptor = expectedSourceNodeDescriptor is null
            ? null
            : expectedSourceNodeDescriptor with { };
    }

    // Create a one-shot bridge with one fixed candidate runner and source-node identity.
    public static OperationResult<DynamicGraphCandidateInvocationOrchestrator> Create(
        DynamicGraphLogicalOrchestratorRuntime runtime,
        IGraphCandidateRunner runner,
        string sourceNodeId)
    {
        if (runtime is null)
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Dynamic graph logical orchestrator runtime is required.",
                RuntimeRequiredCode);
        }

        if (runner is null)
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Graph candidate runner is required.",
                RunnerRequiredCode);
        }

        if (!IsCanonicalValue(sourceNodeId))
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Dynamic graph candidate invocation source-node identity is invalid.",
                SourceNodeInvalidCode);
        }

        return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Ok(
            new DynamicGraphCandidateInvocationOrchestrator(
                runtime,
                runner,
                sourceNodeId,
                expectedSourceNodeDescriptor: null));
    }

    // Create the production bounded axis sweep from one freshly registered executable source node.
    public static OperationResult<DynamicGraphCandidateInvocationOrchestrator> CreateAxisSweep<TInput, TOutput>(
        DynamicGraphLogicalOrchestratorRuntime runtime,
        IDynamicGraphNode<TInput, TOutput> sourceNode,
        Func<GraphCandidateRunRequest, CombinationPlan, OperationResult<LogicalUnitRequest<TInput>>> requestProjector)
    {
        if (runtime is null)
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Dynamic graph logical orchestrator runtime is required.",
                RuntimeRequiredCode);
        }

        if (sourceNode is null)
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Dynamic graph candidate invocation source node is required.",
                SourceNodeRequiredCode);
        }

        if (requestProjector is null)
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Dynamic graph candidate invocation request projector is required.",
                RequestProjectorRequiredCode);
        }

        var registry = new InMemoryDynamicGraphNodeRegistry<TInput, TOutput>();
        OperationResult<DynamicGraphNodeDescriptor>? registration;
        try
        {
            registration = registry.Register(sourceNode);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Dynamic graph candidate invocation source-node registration failed.",
                SourceNodeRegistrationFailedCode);
        }

        if (registration is null
            || (registration.IsSuccess && registration.Value is null))
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Dynamic graph candidate invocation source-node registration result is required.",
                SourceNodeRegistrationResultRequiredCode);
        }

        if (!registration.IsSuccess)
            return registration.MapError<DynamicGraphCandidateInvocationOrchestrator>();

        var descriptor = registration.Value!;
        var strategy = new AxisSweepCombinationStrategy(useDeterministicFixtureBaseline: false);
        var runner = new BoundedCombinationGraphCandidateRunner<TInput, TOutput>(
            strategy,
            registry,
            requestProjector);
        if (!IsCanonicalValue(descriptor.NodeId))
        {
            return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Fail(
                "Dynamic graph candidate invocation source-node identity is invalid.",
                SourceNodeInvalidCode);
        }

        return OperationResult<DynamicGraphCandidateInvocationOrchestrator>.Ok(
            new DynamicGraphCandidateInvocationOrchestrator(
                runtime,
                runner,
                descriptor.NodeId,
                descriptor));
    }

    // Run exactly one logical/candidate/topology correlation and retain no invocation state afterward.
    public async Task<OperationResult<LogicalAssistantRunResult>> RunAsync(
        string userRequest,
        GraphCandidateRunRequest? candidateRequest,
        string? scenarioId = null,
        LogicalExecutionFlags? flags = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        // The first attempt owns this instance even when its method token was already cancelled.
        if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Dynamic graph candidate invocation has already started.",
                AlreadyStartedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<LogicalOrchestratorRequest>? preparedResult;
        try
        {
            preparedResult = _runtime.Assistant.PrepareRunRequest(
                userRequest,
                scenarioId,
                flags,
                metadata);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Logical orchestrator request preparation failed.",
                CorrelationInvalidCode);
        }

        if (preparedResult is null)
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Logical orchestrator request preparation result is required.",
                CorrelationInvalidCode);
        }

        if (!preparedResult.IsSuccess)
            return preparedResult.MapError<LogicalAssistantRunResult>();

        var preparedRequest = preparedResult.Value;
        if (preparedRequest is null
            || !IsCanonicalValue(preparedRequest.RunId)
            || preparedRequest.Metadata is null
            || !StringComparer.Ordinal.Equals(preparedRequest.UserRequest, userRequest))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Logical and candidate invocation correlation is invalid.",
                CorrelationInvalidCode);
        }

        // Explicit and ProjectWiki pre-routes remain exact legacy assistant calls with no candidate side effects.
        if (!StringComparer.Ordinal.Equals(
                preparedRequest.ScenarioId,
                AdaptiveLogicalOrchestrator.ScenarioId))
        {
            return await _runtime.Assistant.RunPreparedAsync(preparedRequest, cancellationToken);
        }

        if (candidateRequest is null)
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Graph candidate run request is required for an adaptive invocation.",
                RequestRequiredCode);
        }

        if (!IsCanonicalValue(candidateRequest.ScenarioId)
            || !StringComparer.Ordinal.Equals(candidateRequest.ScenarioId, preparedRequest.ScenarioId)
            || !StringComparer.Ordinal.Equals(candidateRequest.ScenarioId, AdaptiveLogicalOrchestrator.ScenarioId))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Graph candidate scenario does not match the prepared adaptive scenario.",
                ScenarioMismatchCode);
        }

        if (!StringComparer.Ordinal.Equals(candidateRequest.Query, preparedRequest.UserRequest))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Graph candidate query does not match the prepared user request.",
                QueryMismatchCode);
        }

        if (!IsValidAdaptiveCandidateRequest(candidateRequest))
        {
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Graph candidate run request is invalid.",
                RequestInvalidCode);
        }

        CancellationTokenSource? linkedCancellation = null;
        GraphCandidateRunRequest runnerRequest;
        GraphCandidateRunRequest requestExpectation;
        try
        {
            // Link the method token and the caller's original embedded token exactly once.
            linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                candidateRequest.Cancellation);
            runnerRequest = CopyRequest(candidateRequest, CancellationToken.None);
            requestExpectation = CopyRequest(runnerRequest, CancellationToken.None);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            linkedCancellation?.Dispose();
            return OperationResult<LogicalAssistantRunResult>.Fail(
                "Graph candidate run request is invalid.",
                RequestInvalidCode);
        }

        using (linkedCancellation!)
        {
            var linkedToken = linkedCancellation.Token;
            if (linkedToken.IsCancellationRequested)
                return Cancelled();

            OperationResult<DynamicGraphTopologyRuntimeSnapshot>? topologyRead;
            try
            {
                // This is the only topology-source read in the complete candidate invocation.
                topologyRead = _runtime.TopologySource.ReadCurrent();
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                return linkedToken.IsCancellationRequested
                    ? Cancelled()
                    : OperationResult<LogicalAssistantRunResult>.Fail(
                        "Dynamic graph topology is unavailable for candidate invocation.",
                        TopologyUnavailableCode);
            }

            if (linkedToken.IsCancellationRequested)
                return Cancelled();

            if (topologyRead is null)
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Dynamic graph topology source result is required for candidate invocation.",
                    TopologyUnavailableCode);
            }

            if (!topologyRead.IsSuccess)
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Dynamic graph topology is unavailable for candidate invocation.",
                    TopologyUnavailableCode);
            }

            var topologySnapshot = topologyRead.Value;
            if (topologySnapshot is null || topologySnapshot.Topology is null)
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Dynamic graph topology snapshot is required for candidate invocation.",
                    TopologySnapshotRequiredCode);
            }

            if (topologySnapshot.Revision <= 0)
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Dynamic graph topology revision must be positive for candidate invocation.",
                    TopologyRevisionInvalidCode);
            }

            OperationResult<DynamicGraphTopology>? topologyValidation;
            try
            {
                topologyValidation = DynamicGraphValidation.Validate(topologySnapshot.Topology);
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Dynamic graph topology is invalid for candidate invocation.",
                    TopologyInvalidCode);
            }

            if (topologyValidation is null || !topologyValidation.IsSuccess || topologyValidation.Value is null)
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Dynamic graph topology is invalid for candidate invocation.",
                    TopologyInvalidCode);
            }

            try
            {
                var matchingSourceNodes = topologySnapshot.Topology.Nodes
                    .Where(node => node is not null
                        && StringComparer.Ordinal.Equals(node.NodeId, _sourceNodeId))
                    .ToArray();
                if (matchingSourceNodes.Length != 1)
                {
                    return OperationResult<LogicalAssistantRunResult>.Fail(
                        "Configured candidate source node does not exactly match the invocation topology.",
                        SourceNodeMismatchCode);
                }

                var sourceTopologyNode = matchingSourceNodes[0]!;
                if (_expectedSourceNodeDescriptor is not null)
                {
                    var topologyDescriptor = sourceTopologyNode.Descriptor;
                    if (topologyDescriptor is null
                        || topologyDescriptor != _expectedSourceNodeDescriptor)
                    {
                        return OperationResult<LogicalAssistantRunResult>.Fail(
                            "Configured candidate source node does not exactly match the invocation topology.",
                            SourceNodeMismatchCode);
                    }
                }
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Configured candidate source node does not exactly match the invocation topology.",
                    SourceNodeMismatchCode);
            }

            if (linkedToken.IsCancellationRequested)
                return Cancelled();

            OperationResult<AsyncCandidateBatch>? runnerResult;
            try
            {
                Task<OperationResult<AsyncCandidateBatch>>? runnerTask =
                    _runner.RunCandidatesAsync(runnerRequest, linkedToken);
                if (runnerTask is null)
                {
                    return linkedToken.IsCancellationRequested
                        ? Cancelled()
                        : OperationResult<LogicalAssistantRunResult>.Fail(
                            "Graph candidate runner result is required.",
                            RunnerResultRequiredCode);
                }

                runnerResult = await runnerTask;
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                return linkedToken.IsCancellationRequested
                    ? Cancelled()
                    : OperationResult<LogicalAssistantRunResult>.Fail(
                        "Graph candidate runner failed unexpectedly.",
                        RunnerExceptionCode);
            }

            if (linkedToken.IsCancellationRequested)
                return Cancelled();

            if (runnerResult is null
                || (runnerResult.IsSuccess && runnerResult.Value is null))
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Graph candidate runner result is required.",
                    RunnerResultRequiredCode);
            }

            if (!runnerResult.IsSuccess)
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Graph candidate runner failed.",
                    RunnerFailedCode);
            }

            var batch = runnerResult.Value!;
            if (!RequestsMatch(batch.Request, requestExpectation))
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Returned candidate batch request does not match the normalized runner request.",
                    BatchRequestMismatchCode);
            }

            GraphCandidateOutput[] wrappers;
            try
            {
                if (!StringComparer.Ordinal.Equals(batch.Status, "OK")
                    || batch.Candidates is null
                    || batch.Candidates.Count == 0
                    || batch.ElapsedMs < 0
                    || batch.ErrorCodes is null
                    || batch.ErrorCodes.Length != 0
                    || batch.RequestedCombinationCount != requestExpectation.CombinationCount
                    || batch.Candidates.Count != requestExpectation.CombinationCount
                    || batch.ActualCombinationCount != requestExpectation.CombinationCount)
                {
                    return OperationResult<LogicalAssistantRunResult>.Fail(
                        "Returned candidate batch is not a clean complete batch.",
                        BatchNotCleanCode);
                }

                wrappers = batch.Candidates.ToArray();
                var combinationIds = new HashSet<string>(StringComparer.Ordinal);
                if (wrappers.Length != requestExpectation.CombinationCount
                    || wrappers.Any(wrapper => wrapper is null
                        || wrapper.ElapsedMs < 0
                        || wrapper.ErrorCodes is null
                        || wrapper.ErrorCodes.Length != 0
                        || wrapper.OutputRefs is null
                        || wrapper.OutputRefs.Count == 0
                        || !IsCanonicalValue(wrapper.CombinationId)
                        || !combinationIds.Add(wrapper.CombinationId)))
                {
                    return OperationResult<LogicalAssistantRunResult>.Fail(
                        "Returned candidate batch is not a clean complete batch.",
                        BatchNotCleanCode);
                }
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Returned candidate batch is not a clean complete batch.",
                    BatchNotCleanCode);
            }

            GraphNodeCandidate[] candidateOutputs;
            try
            {
                var flattened = wrappers
                    .SelectMany(wrapper => wrapper.OutputRefs)
                    .ToArray();
                if (flattened.Length == 0 || flattened.Any(candidate => candidate is null))
                {
                    return OperationResult<LogicalAssistantRunResult>.Fail(
                        "Candidate batch node outputs are invalid.",
                        OutputInvalidCode);
                }

                if (flattened.Any(candidate =>
                        !StringComparer.Ordinal.Equals(candidate.UnitId, _sourceNodeId)))
                {
                    return OperationResult<LogicalAssistantRunResult>.Fail(
                        "Candidate batch node outputs do not match the configured source node.",
                        OutputSourceNodeMismatchCode);
                }

                candidateOutputs = flattened.Select(CopyCandidate).ToArray();
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                return OperationResult<LogicalAssistantRunResult>.Fail(
                    "Candidate batch node outputs are invalid.",
                    OutputInvalidCode);
            }

            // The source owns only this invocation and labels no retained provenance or evidence as verified.
            var nodeOutputSource = new InvocationNodeOutputSource(
                preparedRequest.RunId,
                topologySnapshot.Revision,
                _sourceNodeId,
                candidateOutputs);

            if (linkedToken.IsCancellationRequested)
                return Cancelled();

            return await _runtime.Assistant.RunPreparedDynamicGraphAsync(
                preparedRequest,
                topologySnapshot,
                nodeOutputSource,
                linkedToken);
        }
    }

    private static GraphCandidateRunRequest CopyRequest(
        GraphCandidateRunRequest source,
        CancellationToken cancellation)
        => new(
            source.ScenarioId,
            source.Query,
            source.InputGraphRefs.ToArray(),
            source.InputUnitRefs.ToArray(),
            new AsyncCombinationBudget(
                source.Budget.MaxCombinations,
                source.Budget.Timeout,
                source.Budget.MaxParallelBranches,
                source.Budget.MaxParallelCombinationRuns),
            source.CombinationCount,
            cancellation,
            source.LegalScope,
            source.ProvenanceRefs.ToArray(),
            source.TraceRef);

    private static bool IsValidAdaptiveCandidateRequest(GraphCandidateRunRequest request)
        => !string.IsNullOrWhiteSpace(request.ScenarioId)
           && !string.IsNullOrWhiteSpace(request.Query)
           && HasRequiredReferences(request.InputGraphRefs)
           && HasRequiredReferences(request.InputUnitRefs)
           && request.Budget is not null
           && request.CombinationCount is >= 2 and <= 8
           && request.Budget.MaxCombinations >= request.CombinationCount
           && request.Budget.MaxCombinations <= 8
           && request.Budget.MaxParallelBranches >= 1
           && request.Budget.MaxParallelCombinationRuns >= 1
           && request.Budget.MaxParallelCombinationRuns <= request.Budget.MaxCombinations
           && (request.Budget.Timeout is null || request.Budget.Timeout > TimeSpan.Zero)
           && !string.IsNullOrWhiteSpace(request.LegalScope)
           && HasRequiredReferences(request.ProvenanceRefs)
           && !string.IsNullOrWhiteSpace(request.TraceRef);

    private static bool HasRequiredReferences(string[]? values)
        => values is { Length: > 0 }
           && values.All(value => !string.IsNullOrWhiteSpace(value));

    private static bool RequestsMatch(
        GraphCandidateRunRequest? actual,
        GraphCandidateRunRequest expected)
        => actual is not null
           && StringComparer.Ordinal.Equals(actual.ScenarioId, expected.ScenarioId)
           && StringComparer.Ordinal.Equals(actual.Query, expected.Query)
           && SequenceEquals(actual.InputGraphRefs, expected.InputGraphRefs)
           && SequenceEquals(actual.InputUnitRefs, expected.InputUnitRefs)
           && BudgetsMatch(actual.Budget, expected.Budget)
           && actual.CombinationCount == expected.CombinationCount
           && actual.Cancellation == CancellationToken.None
           && !actual.Cancellation.CanBeCanceled
           && !actual.Cancellation.IsCancellationRequested
           && StringComparer.Ordinal.Equals(actual.LegalScope, expected.LegalScope)
           && SequenceEquals(actual.ProvenanceRefs, expected.ProvenanceRefs)
           && StringComparer.Ordinal.Equals(actual.TraceRef, expected.TraceRef);

    private static bool BudgetsMatch(AsyncCombinationBudget? actual, AsyncCombinationBudget expected)
        => actual is not null
           && actual.MaxCombinations == expected.MaxCombinations
           && Nullable.Equals(actual.Timeout, expected.Timeout)
           && actual.MaxParallelBranches == expected.MaxParallelBranches
           && actual.MaxParallelCombinationRuns == expected.MaxParallelCombinationRuns;

    private static bool SequenceEquals(string[]? actual, string[] expected)
        => actual is not null
           && actual.SequenceEqual(expected, StringComparer.Ordinal);

    private static GraphNodeCandidate CopyCandidate(GraphNodeCandidate source)
        => new(
            source.CandidateId,
            source.UnitId,
            CopyArray(source.UnitIds),
            source.LogicalUnitRef,
            CopyArray(source.OutputRefs),
            source.OutputType,
            source.Confidence,
            source.Score,
            CopyArray(source.ProvenanceRefs),
            source.PathOrderRef,
            source.LegalStatus,
            source.Status,
            CopyArray(source.ErrorCodes),
            CopyArray(source.SourceIds),
            source.TraceRef,
            CopyArray(source.TraceRefs),
            CopyArray(source.EvidenceIds),
            source.B1TeacherJudgeRef,
            source.B7GraphOrderRef,
            source.B11AsyncBatchRef,
            source.B12WinnerSelectionRef,
            source.B6UnitDpoBuilderRef,
            source.SemanticQualityClaimed);

    private static string[] CopyArray(string[]? source)
        => source is null ? null! : source.ToArray();

    private static bool IsCanonicalValue(string? value)
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

    private static bool IsNonFatalException(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is OutOfMemoryException
                || current is StackOverflowException
                || current is AccessViolationException
                || current is AppDomainUnloadedException
                || current is BadImageFormatException
                || StringComparer.Ordinal.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException"))
            {
                return false;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return true;
    }

    private static OperationResult<LogicalAssistantRunResult> Cancelled()
        => OperationResult<LogicalAssistantRunResult>.Fail("Cancelled.", "CANCELLED");

    // Owns one immutable candidate set and returns a fresh defensive snapshot on every exact run-id read.
    private sealed class InvocationNodeOutputSource : IDynamicGraphNodeOutputSource
    {
        private readonly string _runId;
        private readonly long _topologyRevision;
        private readonly string _sourceNodeId;
        private readonly GraphNodeCandidate[] _candidates;

        public InvocationNodeOutputSource(
            string runId,
            long topologyRevision,
            string sourceNodeId,
            IReadOnlyList<GraphNodeCandidate> candidates)
        {
            _runId = runId;
            _topologyRevision = topologyRevision;
            _sourceNodeId = sourceNodeId;
            _candidates = candidates.Select(CopyCandidate).ToArray();
        }

        public OperationResult<DynamicGraphNodeOutputSnapshot> ReadForInvocation(string runId)
        {
            if (!StringComparer.Ordinal.Equals(runId, _runId))
            {
                return OperationResult<DynamicGraphNodeOutputSnapshot>.Fail(
                    "Dynamic graph node-output run identity does not match the invocation.",
                    "DYNAMIC_GRAPH_NODE_OUTPUT_RUN_ID_MISMATCH");
            }

            return OperationResult<DynamicGraphNodeOutputSnapshot>.Ok(
                new DynamicGraphNodeOutputSnapshot(
                    _runId,
                    _topologyRevision,
                    _sourceNodeId,
                    _candidates.Select(CopyCandidate).ToArray()));
        }
    }
}
