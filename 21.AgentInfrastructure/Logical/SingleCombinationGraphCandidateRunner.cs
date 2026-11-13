// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using System.Diagnostics;
using AgentInfrastructure;

// This runner is the deliberately bounded one-plan, one-node, one-combination candidate execution path.
public sealed class SingleCombinationGraphCandidateRunner<TInput, TOutput> : IGraphCandidateRunner
{
    private const string PlanInvalidCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_PLAN_INVALID";
    private const string RequestInvalidCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_REQUEST_INVALID";
    private const string CombinationCountUnsupportedCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_COMBINATION_COUNT_UNSUPPORTED";
    private const string NodeCountUnsupportedCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_NODE_COUNT_UNSUPPORTED";
    private const string ParallelismUnsupportedCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_PARALLELISM_UNSUPPORTED";
    private const string ProjectionRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_REQUEST_PROJECTION_REQUIRED";
    private const string ProjectionExceptionCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_REQUEST_PROJECTION_EXCEPTION";
    private const string ProjectionResultRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_REQUEST_PROJECTION_RESULT_REQUIRED";
    private const string NodeTaskRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_NODE_TASK_REQUIRED";
    private const string NodeResultRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_NODE_RESULT_REQUIRED";
    private const string NodeOutputRequiredCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_NODE_OUTPUT_REQUIRED";
    private const string NodeExceptionCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_NODE_EXCEPTION";
    private const string MaterializationExceptionCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_MATERIALIZATION_EXCEPTION";
    private const string DuplicateCandidateCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_DUPLICATE_CANDIDATE";
    private const string CandidateNodeMismatchCode = "DYNAMIC_GRAPH_CANDIDATE_RUNNER_CANDIDATE_NODE_MISMATCH";
    private const string CancelledCode = "CANCELLED";
    private const string TimeoutCode = "TIMEOUT";

    private readonly InMemoryDynamicGraphNodeRegistry<TInput, TOutput>? _registry;
    private readonly CombinationPlan? _plan;
    private readonly Func<
        GraphCandidateRunRequest,
        CombinationPlan,
        OperationResult<LogicalUnitRequest<TInput>>>? _requestProjector;
    private readonly OperationResult<bool> _constructorValidation;

    public SingleCombinationGraphCandidateRunner(
        InMemoryDynamicGraphNodeRegistry<TInput, TOutput> registry,
        CombinationPlan plan,
        Func<
            GraphCandidateRunRequest,
            CombinationPlan,
            OperationResult<LogicalUnitRequest<TInput>>> requestProjector)
    {
        _registry = registry;
        _requestProjector = requestProjector;
        _constructorValidation = ValidateConstructor(registry, plan, requestProjector);
        _plan = _constructorValidation.IsSuccess ? CopyPlan(plan) : null;
    }

    public async Task<OperationResult<AsyncCandidateBatch>> RunCandidatesAsync(
        GraphCandidateRunRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested
            || (request is not null && request.Cancellation.IsCancellationRequested))
        {
            return Cancelled();
        }

        if (!_constructorValidation.IsSuccess)
            return Failure(_constructorValidation.ErrorMessage, _constructorValidation.ErrorCode);

        var plan = _plan!;
        var requestValidation = ValidateRequest(request, plan);
        if (!requestValidation.IsSuccess)
            return Failure(requestValidation.ErrorMessage, requestValidation.ErrorCode);

        GraphCandidateRunRequest requestSnapshot;
        try
        {
            requestSnapshot = CopyRequest(request!);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Graph candidate request snapshot is invalid.", RequestInvalidCode);
        }

        var resolution = _registry!.Resolve(plan.NodeSetIds[0]);
        if (!resolution.IsSuccess)
            return Failure(resolution.ErrorMessage, resolution.ErrorCode);

        var node = resolution.Value;
        if (node is null)
            return Failure("Resolved dynamic graph node is required.", NodeResultRequiredCode);

        OperationResult<LogicalUnitRequest<TInput>>? projection;
        try
        {
            projection = _requestProjector!(CopyRequest(requestSnapshot), CopyPlan(plan));
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Graph candidate request projection failed unexpectedly.", ProjectionExceptionCode);
        }

        if (projection is null || (projection.IsSuccess && projection.Value is null))
        {
            return Failure(
                "Graph candidate request projection result is required.",
                ProjectionResultRequiredCode);
        }

        if (!projection.IsSuccess)
            return Failure(projection.ErrorMessage, projection.ErrorCode);

        var projectedRequest = projection.Value!;
        var projectedValidation = ValidateProjectedRequest(projectedRequest, plan);
        if (!projectedValidation.IsSuccess)
            return Failure(projectedValidation.ErrorMessage, projectedValidation.ErrorCode);

        LogicalUnitRequest<TInput> projectedSnapshot;
        try
        {
            projectedSnapshot = CopyProjectedRequest(projectedRequest);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Projected graph candidate request is invalid.", RequestInvalidCode);
        }

        ExecutionCancellationScope executionCancellation;
        try
        {
            executionCancellation = ExecutionCancellationScope.Create(
                cancellationToken,
                requestSnapshot.Cancellation,
                requestSnapshot.Budget.Timeout);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Graph candidate request budget is invalid.", RequestInvalidCode);
        }

        using (executionCancellation)
        {
            var cancellationFailure = ClassifyCancellation(
                cancellationToken,
                requestSnapshot.Cancellation,
                executionCancellation);
            if (cancellationFailure is not null)
                return cancellationFailure;

            var executionStopwatch = Stopwatch.StartNew();
            Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>>? nodeTask;
            try
            {
                nodeTask = node.ExecuteAsync(projectedSnapshot, executionCancellation.Token);
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                cancellationFailure = ClassifyCancellation(
                    cancellationToken,
                    requestSnapshot.Cancellation,
                    executionCancellation);
                return cancellationFailure
                    ?? Failure("Dynamic graph node execution failed unexpectedly.", NodeExceptionCode);
            }

            if (nodeTask is null)
            {
                cancellationFailure = ClassifyCancellation(
                    cancellationToken,
                    requestSnapshot.Cancellation,
                    executionCancellation);
                return cancellationFailure
                    ?? Failure("Dynamic graph node execution task is required.", NodeTaskRequiredCode);
            }

            OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>? nodeExecution;
            try
            {
                nodeExecution = await nodeTask.ConfigureAwait(false);
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                cancellationFailure = ClassifyCancellation(
                    cancellationToken,
                    requestSnapshot.Cancellation,
                    executionCancellation);
                return cancellationFailure
                    ?? Failure("Dynamic graph node execution failed unexpectedly.", NodeExceptionCode);
            }

            cancellationFailure = ClassifyCancellation(
                cancellationToken,
                requestSnapshot.Cancellation,
                executionCancellation);
            if (cancellationFailure is not null)
                return cancellationFailure;

            if (nodeExecution is null || (nodeExecution.IsSuccess && nodeExecution.Value is null))
                return Failure("Dynamic graph node execution result is required.", NodeResultRequiredCode);

            if (!nodeExecution.IsSuccess)
                return Failure(nodeExecution.ErrorMessage, nodeExecution.ErrorCode);

            var nodeResult = nodeExecution.Value!;
            if (nodeResult.Output is null)
                return Failure("Dynamic graph node query output is required.", NodeOutputRequiredCode);

            GraphNodeCandidate[] materialized;
            try
            {
                // This is the runner's single IQueryable enumeration boundary.
                materialized = nodeResult.Output.ToArray();
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                cancellationFailure = ClassifyCancellation(
                    cancellationToken,
                    requestSnapshot.Cancellation,
                    executionCancellation);
                return cancellationFailure
                    ?? Failure("Dynamic graph candidate materialization failed unexpectedly.", MaterializationExceptionCode);
            }

            cancellationFailure = ClassifyCancellation(
                cancellationToken,
                requestSnapshot.Cancellation,
                executionCancellation);
            if (cancellationFailure is not null)
                return cancellationFailure;

            if (materialized.Length == 0)
            {
                executionStopwatch.Stop();
                return OperationResult<AsyncCandidateBatch>.Ok(
                    CreateEmptyBatch(requestSnapshot, executionStopwatch.ElapsedMilliseconds));
            }

            GraphNodeCandidate[] candidateSnapshots;
            string[] aggregatedErrorCodes;
            try
            {
                var ids = new HashSet<string>(StringComparer.Ordinal);
                candidateSnapshots = new GraphNodeCandidate[materialized.Length];
                var errors = new List<string>();
                for (var index = 0; index < materialized.Length; index++)
                {
                    var candidate = materialized[index];
                    if (candidate is null || string.IsNullOrWhiteSpace(candidate.CandidateId))
                    {
                        return Failure(
                            "Materialized dynamic graph candidate identity is required.",
                            MaterializationExceptionCode);
                    }

                    if (!ids.Add(candidate.CandidateId))
                    {
                        return Failure(
                            "Materialized dynamic graph candidate identities must be unique.",
                            DuplicateCandidateCode);
                    }

                    if (!StringComparer.Ordinal.Equals(candidate.UnitId, plan.NodeSetIds[0]))
                    {
                        return Failure(
                            "Materialized dynamic graph candidate node identity does not match the plan.",
                            CandidateNodeMismatchCode);
                    }

                    var snapshot = CopyCandidate(candidate);
                    candidateSnapshots[index] = snapshot;
                    errors.AddRange(snapshot.ErrorCodes);
                }

                aggregatedErrorCodes = errors.ToArray();
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                return Failure(
                    "Dynamic graph candidate materialization failed unexpectedly.",
                    MaterializationExceptionCode);
            }

            var outputSnapshots = Array.AsReadOnly(candidateSnapshots);
            var unitIdSnapshots = plan.NodeSetIds.ToArray();
            var provenanceSnapshots = requestSnapshot.ProvenanceRefs.ToArray();
            var errorCodeSnapshots = aggregatedErrorCodes.ToArray();
            executionStopwatch.Stop();
            var elapsedMs = executionStopwatch.ElapsedMilliseconds;

            var candidateOutput = new GraphCandidateOutput(
                CandidateId: plan.CombinationId,
                OutputRefs: outputSnapshots,
                UnitId: plan.NodeSetIds[0],
                UnitIds: unitIdSnapshots,
                Confidence: nodeResult.Confidence,
                Score: null,
                CombinationId: plan.CombinationId,
                ProvenanceRefs: provenanceSnapshots,
                PathTraceRef: plan.EdgeOrderKey,
                TraceRef: requestSnapshot.TraceRef,
                ErrorCodes: errorCodeSnapshots,
                ElapsedMs: elapsedMs);
            var status = aggregatedErrorCodes.Length == 0 ? "OK" : "PARTIAL_FAILURE";
            var batch = new AsyncCandidateBatch(
                BatchId: requestSnapshot.TraceRef,
                Request: CopyRequest(requestSnapshot),
                Candidates: Array.AsReadOnly([candidateOutput]),
                CandidateStream: null,
                RequestedCombinationCount: 1,
                ActualCombinationCount: 1,
                Status: status,
                ErrorCodes: errorCodeSnapshots.ToArray(),
                ElapsedMs: elapsedMs);

            return OperationResult<AsyncCandidateBatch>.Ok(batch);
        }
    }

    private static OperationResult<bool> ValidateConstructor(
        InMemoryDynamicGraphNodeRegistry<TInput, TOutput>? registry,
        CombinationPlan? plan,
        Func<
            GraphCandidateRunRequest,
            CombinationPlan,
            OperationResult<LogicalUnitRequest<TInput>>>? requestProjector)
    {
        if (registry is null)
            return OperationResult<bool>.Fail("Dynamic graph node registry is required.", PlanInvalidCode);
        if (requestProjector is null)
            return OperationResult<bool>.Fail("Graph candidate request projection is required.", ProjectionRequiredCode);
        if (plan is null)
            return OperationResult<bool>.Fail("Graph candidate combination plan is required.", PlanInvalidCode);
        if (plan.Index != 0
            || string.IsNullOrWhiteSpace(plan.CombinationId)
            || string.IsNullOrWhiteSpace(plan.EdgeOrderKey)
            || plan.Flags is null
            || plan.NodeSetIds is null)
        {
            return OperationResult<bool>.Fail("Graph candidate combination plan is invalid.", PlanInvalidCode);
        }

        if (plan.NodeSetIds.Length != 1)
        {
            return OperationResult<bool>.Fail(
                "Single-combination graph candidate runner requires exactly one node.",
                NodeCountUnsupportedCode);
        }

        if (string.IsNullOrWhiteSpace(plan.NodeSetIds[0]))
            return OperationResult<bool>.Fail("Graph candidate combination plan is invalid.", PlanInvalidCode);

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateRequest(
        GraphCandidateRunRequest? request,
        CombinationPlan plan)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.ScenarioId)
            || string.IsNullOrWhiteSpace(request.Query)
            || string.IsNullOrWhiteSpace(request.LegalScope)
            || string.IsNullOrWhiteSpace(request.TraceRef)
            || !AreRequiredRefsValid(request.InputGraphRefs)
            || !AreRequiredRefsValid(request.InputUnitRefs)
            || !AreRequiredRefsValid(request.ProvenanceRefs)
            || request.Budget is null)
        {
            return OperationResult<bool>.Fail("Graph candidate run request is invalid.", RequestInvalidCode);
        }

        if (plan.NodeSetIds is null || plan.NodeSetIds.Length != 1)
        {
            return OperationResult<bool>.Fail(
                "Single-combination graph candidate runner requires exactly one node.",
                NodeCountUnsupportedCode);
        }

        if (request.CombinationCount != 1)
        {
            return OperationResult<bool>.Fail(
                "Single-combination graph candidate runner supports exactly one combination.",
                CombinationCountUnsupportedCode);
        }

        if (request.Budget.MaxParallelCombinationRuns != 1)
        {
            return OperationResult<bool>.Fail(
                "Single-combination graph candidate runner supports one parallel combination run.",
                ParallelismUnsupportedCode);
        }

        if (request.Budget.MaxCombinations < 1
            || request.CombinationCount > request.Budget.MaxCombinations
            || request.Budget.MaxParallelBranches < 1
            || request.Budget.Timeout is { } timeout && timeout <= TimeSpan.Zero
            || request.InputUnitRefs.Length != 1
            || !StringComparer.Ordinal.Equals(request.InputUnitRefs[0], plan.NodeSetIds[0]))
        {
            return OperationResult<bool>.Fail("Graph candidate run request is invalid.", RequestInvalidCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateProjectedRequest(
        LogicalUnitRequest<TInput>? request,
        CombinationPlan plan)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.RunId)
            || string.IsNullOrWhiteSpace(request.UnitId)
            || string.IsNullOrWhiteSpace(request.UnitRole)
            || string.IsNullOrWhiteSpace(request.DomainTag)
            || request.Input is null
            || request.Flags is null
            || request.Metadata is null
            || !StringComparer.Ordinal.Equals(request.UnitId, plan.NodeSetIds[0])
            || request.Flags != plan.Flags)
        {
            return OperationResult<bool>.Fail("Projected graph candidate request is invalid.", RequestInvalidCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool AreRequiredRefsValid(string[]? values)
        => values is { Length: > 0 }
           && values.All(value => !string.IsNullOrWhiteSpace(value));

    private static CombinationPlan CopyPlan(CombinationPlan source)
        => new(
            source.Index,
            source.CombinationId,
            source.NodeSetIds.ToArray(),
            source.EdgeOrderKey,
            source.Flags with { });

    private static GraphCandidateRunRequest CopyRequest(GraphCandidateRunRequest source)
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
            source.Cancellation,
            source.LegalScope,
            source.ProvenanceRefs.ToArray(),
            source.TraceRef);

    private static LogicalUnitRequest<TInput> CopyProjectedRequest(LogicalUnitRequest<TInput> source)
    {
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var pair in source.Metadata)
            metadata.Add(pair.Key, pair.Value);

        return new LogicalUnitRequest<TInput>(
            source.RunId,
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            source.Input,
            source.Flags with { },
            new ReadOnlyDictionary<string, object>(metadata));
    }

    private static GraphNodeCandidate CopyCandidate(GraphNodeCandidate source)
        => new(
            source.CandidateId,
            source.UnitId,
            source.UnitIds.ToArray(),
            source.LogicalUnitRef,
            source.OutputRefs.ToArray(),
            source.OutputType,
            source.Confidence,
            source.Score,
            source.ProvenanceRefs.ToArray(),
            source.PathOrderRef,
            source.LegalStatus,
            source.Status,
            source.ErrorCodes.ToArray(),
            source.SourceIds.ToArray(),
            source.TraceRef,
            source.TraceRefs.ToArray(),
            source.EvidenceIds.ToArray(),
            source.B1TeacherJudgeRef,
            source.B7GraphOrderRef,
            source.B11AsyncBatchRef,
            source.B12WinnerSelectionRef,
            source.B6UnitDpoBuilderRef,
            source.SemanticQualityClaimed);

    private static AsyncCandidateBatch CreateEmptyBatch(GraphCandidateRunRequest request, long elapsedMs)
        => new(
            BatchId: request.TraceRef,
            Request: CopyRequest(request),
            Candidates: Array.AsReadOnly(Array.Empty<GraphCandidateOutput>()),
            CandidateStream: null,
            RequestedCombinationCount: 1,
            ActualCombinationCount: 0,
            Status: "EMPTY_CANDIDATES",
            ErrorCodes: [],
            ElapsedMs: elapsedMs);

    private static bool IsNonFatalException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    private static OperationResult<AsyncCandidateBatch>? ClassifyCancellation(
        CancellationToken methodCancellation,
        CancellationToken embeddedCancellation,
        ExecutionCancellationScope scope)
    {
        if (methodCancellation.IsCancellationRequested || embeddedCancellation.IsCancellationRequested)
            return Cancelled();
        if (scope.TimeoutCancellationRequested)
            return Failure("Graph candidate run timed out.", TimeoutCode);

        return null;
    }

    private static OperationResult<AsyncCandidateBatch> Cancelled()
        => Failure("Graph candidate run was cancelled.", CancelledCode);

    private static OperationResult<AsyncCandidateBatch> Failure(string message, string code)
        => OperationResult<AsyncCandidateBatch>.Fail(message, code);

    private sealed class ExecutionCancellationScope : IDisposable
    {
        private readonly CancellationTokenSource? _timeoutSource;
        private readonly CancellationTokenSource? _linkedSource;

        private ExecutionCancellationScope(
            CancellationToken token,
            CancellationTokenSource? timeoutSource,
            CancellationTokenSource? linkedSource)
        {
            Token = token;
            _timeoutSource = timeoutSource;
            _linkedSource = linkedSource;
        }

        public CancellationToken Token { get; }

        public bool TimeoutCancellationRequested => _timeoutSource?.IsCancellationRequested == true;

        public static ExecutionCancellationScope Create(
            CancellationToken methodCancellation,
            CancellationToken embeddedCancellation,
            TimeSpan? timeout)
        {
            CancellationTokenSource? timeoutSource = null;
            if (timeout is { } timeoutValue)
                timeoutSource = new CancellationTokenSource(timeoutValue);

            var tokens = new List<CancellationToken>();
            AddDistinct(tokens, methodCancellation);
            AddDistinct(tokens, embeddedCancellation);
            if (timeoutSource is not null)
                AddDistinct(tokens, timeoutSource.Token);

            if (tokens.Count == 0)
                return new ExecutionCancellationScope(CancellationToken.None, timeoutSource, null);
            if (tokens.Count == 1)
                return new ExecutionCancellationScope(tokens[0], timeoutSource, null);

            var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(tokens.ToArray());
            return new ExecutionCancellationScope(linkedSource.Token, timeoutSource, linkedSource);
        }

        public void Dispose()
        {
            _linkedSource?.Dispose();
            _timeoutSource?.Dispose();
        }

        private static void AddDistinct(List<CancellationToken> tokens, CancellationToken token)
        {
            if (token.CanBeCanceled && !tokens.Contains(token))
                tokens.Add(token);
        }
    }
}
