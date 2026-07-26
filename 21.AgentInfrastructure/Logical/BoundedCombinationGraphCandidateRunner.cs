// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using System.Diagnostics;
using AgentInfrastructure;

// Executes a validated bounded plan set while delegating every individual plan to the accepted single-plan runner.
public sealed class BoundedCombinationGraphCandidateRunner<TInput, TOutput> : IGraphCandidateRunner
{
    private const string DependencyRequiredCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_DEPENDENCY_REQUIRED";
    private const string RequestInvalidCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_REQUEST_INVALID";
    private const string StrategyExceptionCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_STRATEGY_EXCEPTION";
    private const string StrategyResultRequiredCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_STRATEGY_RESULT_REQUIRED";
    private const string PlanInvalidCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_PLAN_INVALID";
    private const string PlanDuplicateCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_PLAN_DUPLICATE";
    private const string PlanExceptionCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_PLAN_EXCEPTION";
    private const string PlanResultInvalidCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_PLAN_RESULT_INVALID";
    private const string NoPlansCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_NO_PLANS";
    private const string AllPlansFailedCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_ALL_PLANS_FAILED";
    private const string CancelledCode = "CANCELLED";
    private const string TimeoutCode = "TIMEOUT";
    private const int MaximumCombinationCount = 8;

    private readonly IGraphCombinationStrategy? _strategy;
    private readonly InMemoryDynamicGraphNodeRegistry<TInput, TOutput>? _registry;
    private readonly Func<
        GraphCandidateRunRequest,
        CombinationPlan,
        OperationResult<LogicalUnitRequest<TInput>>>? _requestProjector;
    private readonly OperationResult<bool> _constructorValidation;

    public BoundedCombinationGraphCandidateRunner(
        IGraphCombinationStrategy strategy,
        InMemoryDynamicGraphNodeRegistry<TInput, TOutput> registry,
        Func<GraphCandidateRunRequest, CombinationPlan,
            OperationResult<LogicalUnitRequest<TInput>>> requestProjector)
    {
        _strategy = strategy;
        _registry = registry;
        _requestProjector = requestProjector;
        _constructorValidation = ValidateConstructor(strategy, registry, requestProjector);
    }

    public async Task<OperationResult<AsyncCandidateBatch>> RunCandidatesAsync(
        GraphCandidateRunRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_constructorValidation.IsSuccess)
            return Failure(_constructorValidation.ErrorMessage, _constructorValidation.ErrorCode);

        var requestValidation = ValidateRequest(request);
        if (!requestValidation.IsSuccess)
            return Failure(requestValidation.ErrorMessage, requestValidation.ErrorCode);

        GraphCandidateRunRequest requestSnapshot;
        try
        {
            requestSnapshot = CopyRequest(request);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure("Bounded graph candidate request snapshot is invalid.", RequestInvalidCode);
        }

        if (cancellationToken.IsCancellationRequested || requestSnapshot.Cancellation.IsCancellationRequested)
            return Cancelled();

        OperationResult<IReadOnlyList<CombinationPlan>>? enumeration;
        try
        {
            enumeration = _strategy!.Enumerate(CopyRequest(requestSnapshot));
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return Failure(
                "Bounded graph candidate strategy failed unexpectedly.",
                StrategyExceptionCode);
        }

        if (enumeration is null || enumeration.IsSuccess && enumeration.Value is null)
        {
            return Failure(
                "Bounded graph candidate strategy result is required.",
                StrategyResultRequiredCode);
        }

        if (!enumeration.IsSuccess)
            return Failure(enumeration.ErrorMessage, enumeration.ErrorCode);

        var planValidation = ValidateAndCopyPlans(enumeration.Value!, requestSnapshot);
        if (!planValidation.IsSuccess)
            return Failure(planValidation.ErrorMessage, planValidation.ErrorCode);

        var plans = planValidation.Value!;
        if (plans.Count == 0)
        {
            return OperationResult<AsyncCandidateBatch>.Ok(
                CreateBatch(
                    requestSnapshot,
                    [],
                    "EMPTY_CANDIDATES",
                    [NoPlansCode]));
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
            return Failure("Bounded graph candidate request budget is invalid.", RequestInvalidCode);
        }

        using (executionCancellation)
        {
            var cancellationFailure = ClassifyCancellation(
                cancellationToken,
                requestSnapshot.Cancellation,
                executionCancellation);
            if (cancellationFailure is not null)
                return cancellationFailure;

            var outcomes = new PlanOutcome?[plans.Count];
            var active = new List<Task<PlanOutcome>>();
            var nextPlanIndex = 0;
            var parallelism = Math.Min(requestSnapshot.Budget.MaxParallelCombinationRuns, plans.Count);
            var cancellationSignalSource = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            using var cancellationRegistration = executionCancellation.Token.Register(
                static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true),
                cancellationSignalSource);
            Task cancellationSignal = cancellationSignalSource.Task;

            var batchStopwatch = Stopwatch.StartNew();
            while (nextPlanIndex < plans.Count && active.Count < parallelism)
            {
                active.Add(RunPlanAsync(requestSnapshot, plans[nextPlanIndex], executionCancellation.Token));
                nextPlanIndex++;
            }

            while (active.Count > 0 && !executionCancellation.Token.IsCancellationRequested)
            {
                var completedTask = await Task.WhenAny(
                    active.Cast<Task>().Append(cancellationSignal)).ConfigureAwait(false);
                if (ReferenceEquals(completedTask, cancellationSignal))
                    break;

                var completedPlanTask = (Task<PlanOutcome>)completedTask;
                active.Remove(completedPlanTask);
                var outcome = await completedPlanTask.ConfigureAwait(false);
                outcomes[outcome.Index] = outcome;

                if (nextPlanIndex < plans.Count && !executionCancellation.Token.IsCancellationRequested)
                {
                    active.Add(RunPlanAsync(
                        requestSnapshot,
                        plans[nextPlanIndex],
                        executionCancellation.Token));
                    nextPlanIndex++;
                }
            }

            if (executionCancellation.Token.IsCancellationRequested)
            {
                executionCancellation.CancelActive();
                await AwaitActiveAfterCancellationAsync(active).ConfigureAwait(false);
                return ClassifyCancellation(
                           cancellationToken,
                           requestSnapshot.Cancellation,
                           executionCancellation)
                       ?? Cancelled();
            }

            cancellationFailure = ClassifyCancellation(
                cancellationToken,
                requestSnapshot.Cancellation,
                executionCancellation);
            if (cancellationFailure is not null)
                return cancellationFailure;

            var orderedOutcomes = outcomes
                .Select((outcome, index) => outcome ?? PlanOutcome.Failed(
                    index,
                    PlanBatchCode(index, plans[index].CombinationId, PlanResultInvalidCode)))
                .ToArray();
            return Aggregate(requestSnapshot, orderedOutcomes, batchStopwatch);
        }
    }

    private async Task<PlanOutcome> RunPlanAsync(
        GraphCandidateRunRequest outerRequest,
        CombinationPlan plan,
        CancellationToken cancellationToken)
    {
        try
        {
            var normalizedPlan = new CombinationPlan(
                Index: 0,
                CombinationId: plan.CombinationId,
                NodeSetIds: plan.NodeSetIds.ToArray(),
                EdgeOrderKey: plan.EdgeOrderKey,
                Flags: plan.Flags with { });
            var planRequest = new GraphCandidateRunRequest(
                ScenarioId: outerRequest.ScenarioId,
                Query: outerRequest.Query,
                InputGraphRefs: [plan.EdgeOrderKey],
                InputUnitRefs: plan.NodeSetIds.ToArray(),
                Budget: new AsyncCombinationBudget(
                    MaxCombinations: 1,
                    Timeout: null,
                    MaxParallelBranches: outerRequest.Budget.MaxParallelBranches,
                    MaxParallelCombinationRuns: 1),
                CombinationCount: 1,
                Cancellation: CancellationToken.None,
                LegalScope: outerRequest.LegalScope,
                ProvenanceRefs: outerRequest.ProvenanceRefs.ToArray(),
                TraceRef: $"{outerRequest.TraceRef}:{plan.Index}:{plan.CombinationId}");
            var runner = new SingleCombinationGraphCandidateRunner<TInput, TOutput>(
                _registry!,
                normalizedPlan,
                _requestProjector!);

            var result = await runner.RunCandidatesAsync(planRequest, cancellationToken).ConfigureAwait(false);
            return ValidateChildResult(plan, result);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return PlanOutcome.Failed(
                plan.Index,
                PlanBatchCode(plan.Index, plan.CombinationId, PlanExceptionCode));
        }
    }

    private static PlanOutcome ValidateChildResult(
        CombinationPlan plan,
        OperationResult<AsyncCandidateBatch>? result)
    {
        if (result is null || result.IsSuccess && result.Value is null)
        {
            return PlanOutcome.Failed(
                plan.Index,
                PlanBatchCode(plan.Index, plan.CombinationId, PlanResultInvalidCode));
        }

        if (!result.IsSuccess)
        {
            return PlanOutcome.Failed(
                plan.Index,
                PlanBatchCode(
                    plan.Index,
                    plan.CombinationId,
                    NormalizeChildCode(result.ErrorCode)));
        }

        try
        {
            var batch = result.Value!;
            if (batch.Request is null
                || batch.Candidates is null
                || batch.ErrorCodes is null
                || batch.ElapsedMs < 0
                || string.IsNullOrWhiteSpace(batch.BatchId)
                || batch.RequestedCombinationCount != 1
                || batch.ActualCombinationCount < 0
                || batch.ActualCombinationCount > 1
                || batch.ActualCombinationCount != batch.Candidates.Count)
            {
                return InvalidChild(plan);
            }

            if (StringComparer.Ordinal.Equals(batch.Status, "EMPTY_CANDIDATES"))
            {
                return batch.Candidates.Count == 0 && batch.ErrorCodes.Length == 0
                    ? PlanOutcome.Empty(plan.Index)
                    : InvalidChild(plan);
            }

            if (!StringComparer.Ordinal.Equals(batch.Status, "OK")
                && !StringComparer.Ordinal.Equals(batch.Status, "PARTIAL_FAILURE"))
            {
                return InvalidChild(plan);
            }

            if (batch.Candidates.Count != 1)
                return InvalidChild(plan);

            var candidate = batch.Candidates[0];
            if (!IsCandidateValid(candidate, plan))
                return InvalidChild(plan);

            var candidateSnapshot = CopyOutput(candidate);
            var isPartial = StringComparer.Ordinal.Equals(batch.Status, "PARTIAL_FAILURE")
                            || batch.ErrorCodes.Length > 0
                            || candidateSnapshot.ErrorCodes.Length > 0;
            if (!isPartial && batch.ErrorCodes.Length != 0)
                return InvalidChild(plan);

            var errorCodes = isPartial
                ? NormalizeBatchCodes(plan, batch.ErrorCodes, candidateSnapshot.ErrorCodes)
                : [];

            return isPartial
                ? PlanOutcome.Partial(plan.Index, candidateSnapshot, errorCodes)
                : PlanOutcome.Clean(plan.Index, candidateSnapshot);
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return InvalidChild(plan);
        }
    }

    private static bool IsCandidateValid(GraphCandidateOutput? candidate, CombinationPlan plan)
        => candidate is not null
           && !string.IsNullOrWhiteSpace(candidate.CandidateId)
           && StringComparer.Ordinal.Equals(candidate.CandidateId, plan.CombinationId)
           && StringComparer.Ordinal.Equals(candidate.CombinationId, plan.CombinationId)
           && StringComparer.Ordinal.Equals(candidate.UnitId, plan.NodeSetIds[0])
           && candidate.UnitIds is { Length: 1 }
           && StringComparer.Ordinal.Equals(candidate.UnitIds[0], plan.NodeSetIds[0])
           && candidate.OutputRefs is not null
           && candidate.ProvenanceRefs is not null
           && candidate.ErrorCodes is not null
           && candidate.ElapsedMs >= 0
           && !string.IsNullOrWhiteSpace(candidate.PathTraceRef)
           && !string.IsNullOrWhiteSpace(candidate.TraceRef);

    private static PlanOutcome InvalidChild(CombinationPlan plan)
        => PlanOutcome.Failed(
            plan.Index,
            PlanBatchCode(plan.Index, plan.CombinationId, PlanResultInvalidCode));

    private static string[] NormalizeBatchCodes(
        CombinationPlan plan,
        IReadOnlyList<string> batchCodes,
        IReadOnlyList<string> candidateCodes)
    {
        var source = batchCodes.Count > 0 ? batchCodes : candidateCodes;
        if (source.Count == 0)
            return [PlanBatchCode(plan.Index, plan.CombinationId, PlanResultInvalidCode)];

        return source
            .Select(code => PlanBatchCode(
                plan.Index,
                plan.CombinationId,
                NormalizeChildCode(code)))
            .ToArray();
    }

    private static OperationResult<AsyncCandidateBatch> Aggregate(
        GraphCandidateRunRequest request,
        IReadOnlyList<PlanOutcome> outcomes,
        Stopwatch batchStopwatch)
    {
        var candidates = outcomes
            .OrderBy(outcome => outcome.Index)
            .SelectMany(outcome => outcome.Candidates)
            .Select(CopyOutput)
            .ToArray();
        var errors = outcomes
            .OrderBy(outcome => outcome.Index)
            .SelectMany(outcome => outcome.ErrorCodes)
            .ToArray();
        var anyFailed = outcomes.Any(outcome => outcome.Kind == PlanOutcomeKind.Failed);

        if (candidates.Length == 0 && anyFailed)
        {
            return Failure(
                "All bounded graph candidate plans failed.",
                AllPlansFailedCode);
        }

        if (candidates.Length == 0)
        {
            return OperationResult<AsyncCandidateBatch>.Ok(
                CreateBatch(request, [], "EMPTY_CANDIDATES", [], batchStopwatch));
        }

        var allClean = outcomes.All(outcome => outcome.Kind == PlanOutcomeKind.Clean);
        var status = allClean
                     && candidates.Length == request.CombinationCount
                     && outcomes.Count == request.CombinationCount
            ? "OK"
            : "PARTIAL_FAILURE";
        return OperationResult<AsyncCandidateBatch>.Ok(
            CreateBatch(request, candidates, status, errors, batchStopwatch));
    }

    private static AsyncCandidateBatch CreateBatch(
        GraphCandidateRunRequest request,
        IReadOnlyList<GraphCandidateOutput> candidates,
        string status,
        string[] errorCodes,
        Stopwatch? batchStopwatch = null)
    {
        var requestSnapshot = CopyRequest(request);
        var candidateSnapshots = Array.AsReadOnly(candidates.Select(CopyOutput).ToArray());
        var errorCodeSnapshots = errorCodes.ToArray();
        var elapsedMs = batchStopwatch?.ElapsedMilliseconds ?? 0;
        batchStopwatch?.Stop();
        return new(
            BatchId: request.TraceRef,
            Request: requestSnapshot,
            Candidates: candidateSnapshots,
            CandidateStream: null,
            RequestedCombinationCount: request.CombinationCount,
            ActualCombinationCount: candidates.Count,
            Status: status,
            ErrorCodes: errorCodeSnapshots,
            ElapsedMs: elapsedMs);
    }

    private static OperationResult<IReadOnlyList<CombinationPlan>> ValidateAndCopyPlans(
        IReadOnlyList<CombinationPlan> source,
        GraphCandidateRunRequest request)
    {
        try
        {
            if (source.Count > request.CombinationCount)
            {
                return OperationResult<IReadOnlyList<CombinationPlan>>.Fail(
                    "Bounded graph candidate strategy returned too many plans.",
                    PlanInvalidCode);
            }

            var unitRefs = new HashSet<string>(request.InputUnitRefs, StringComparer.Ordinal);
            var graphRefs = new HashSet<string>(request.InputGraphRefs, StringComparer.Ordinal);
            var combinationIds = new HashSet<string>(StringComparer.Ordinal);
            var planTuples = new HashSet<PlanTupleKey>();
            var snapshots = new CombinationPlan[source.Count];

            for (var index = 0; index < source.Count; index++)
            {
                var plan = source[index];
                if (plan is null
                    || plan.Index != index
                    || string.IsNullOrWhiteSpace(plan.CombinationId)
                    || string.IsNullOrWhiteSpace(plan.EdgeOrderKey)
                    || plan.NodeSetIds is not { Length: 1 }
                    || string.IsNullOrWhiteSpace(plan.NodeSetIds[0])
                    || plan.Flags is null
                    || !unitRefs.Contains(plan.NodeSetIds[0])
                    || !graphRefs.Contains(plan.EdgeOrderKey))
                {
                    return OperationResult<IReadOnlyList<CombinationPlan>>.Fail(
                        "Bounded graph candidate combination plan is invalid.",
                        PlanInvalidCode);
                }

                var tuple = new PlanTupleKey(
                    plan.NodeSetIds[0],
                    plan.EdgeOrderKey,
                    CreateFlagKey(plan.Flags));
                if (!combinationIds.Add(plan.CombinationId) || !planTuples.Add(tuple))
                {
                    return OperationResult<IReadOnlyList<CombinationPlan>>.Fail(
                        "Bounded graph candidate combination plans must be unique.",
                        PlanDuplicateCode);
                }

                snapshots[index] = new CombinationPlan(
                    plan.Index,
                    plan.CombinationId,
                    plan.NodeSetIds.ToArray(),
                    plan.EdgeOrderKey,
                    plan.Flags with { });
            }

            return OperationResult<IReadOnlyList<CombinationPlan>>.Ok(Array.AsReadOnly(snapshots));
        }
        catch (Exception exception) when (IsNonFatalException(exception))
        {
            return OperationResult<IReadOnlyList<CombinationPlan>>.Fail(
                "Bounded graph candidate combination plans are invalid.",
                PlanInvalidCode);
        }
    }

    private static OperationResult<bool> ValidateConstructor(
        IGraphCombinationStrategy? strategy,
        InMemoryDynamicGraphNodeRegistry<TInput, TOutput>? registry,
        Func<GraphCandidateRunRequest, CombinationPlan,
            OperationResult<LogicalUnitRequest<TInput>>>? requestProjector)
        => strategy is null || registry is null || requestProjector is null
            ? OperationResult<bool>.Fail(
                "Bounded graph candidate runner dependencies are required.",
                DependencyRequiredCode)
            : OperationResult<bool>.Ok(true);

    private static OperationResult<bool> ValidateRequest(GraphCandidateRunRequest? request)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.ScenarioId)
            || string.IsNullOrWhiteSpace(request.Query)
            || !AreRequiredRefsValid(request.InputGraphRefs)
            || !AreRequiredRefsValid(request.InputUnitRefs)
            || request.Budget is null
            || request.CombinationCount < 2
            || request.CombinationCount > MaximumCombinationCount
            || request.Budget.MaxCombinations < request.CombinationCount
            || request.Budget.MaxCombinations > MaximumCombinationCount
            || request.Budget.MaxParallelBranches < 1
            || request.Budget.MaxParallelCombinationRuns < 1
            || request.Budget.MaxParallelCombinationRuns > request.Budget.MaxCombinations
            || request.Budget.Timeout is { } timeout && timeout <= TimeSpan.Zero
            || string.IsNullOrWhiteSpace(request.LegalScope)
            || !AreRequiredRefsValid(request.ProvenanceRefs)
            || string.IsNullOrWhiteSpace(request.TraceRef))
        {
            return OperationResult<bool>.Fail(
                "Bounded graph candidate run request is invalid.",
                RequestInvalidCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool AreRequiredRefsValid(string[]? values)
        => values is { Length: > 0 }
           && values.All(value => !string.IsNullOrWhiteSpace(value));

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

    private static GraphCandidateOutput CopyOutput(GraphCandidateOutput source)
        => new(
            source.CandidateId,
            Array.AsReadOnly(source.OutputRefs.Select(CopyCandidate).ToArray()),
            source.UnitId,
            source.UnitIds.ToArray(),
            source.Confidence,
            source.Score,
            source.CombinationId,
            source.ProvenanceRefs.ToArray(),
            source.PathTraceRef,
            source.TraceRef,
            source.ErrorCodes.ToArray(),
            source.ElapsedMs);

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

    private static string CreateFlagKey(LogicalExecutionFlags flags)
        => string.Concat(new[]
        {
            flags.UseCompiledPredicateRepresentation,
            flags.UseDomainScopedRetrieval,
            flags.UseConfidenceFloor,
            flags.UseLearnedUnitWhenAvailable,
            flags.UseTraceEvidence,
            flags.UseContextCarryForward,
            flags.UseIdleLearningScheduling,
            flags.UseE35DependencyDetection,
            flags.UseSkillRetriever,
            flags.UseReplayFusionRanking,
            flags.UseArbiterAuthority,
            flags.UseKnowledgeEdgeDetection,
            flags.UseInternalFirstExecution,
            flags.RequireExternalAiApproval,
            flags.RejectArbiterContradictions,
            flags.UseDeterministicFixtureBaseline,
            flags.WriteEvidenceArtifacts,
            flags.CompareAgainstOracle,
            flags.FailOnMetricDrift
        }.Select(value => value ? '1' : '0'));

    private static string NormalizeChildCode(string? code)
    {
        if (string.IsNullOrEmpty(code) || code.Length > 128)
            return PlanResultInvalidCode;

        foreach (var character in code)
        {
            if (character != '_' && (character < 'A' || character > 'Z')
                                 && (character < '0' || character > '9'))
            {
                return PlanResultInvalidCode;
            }
        }

        return code;
    }

    private static string PlanBatchCode(int index, string combinationId, string childCode)
        => $"P{index}:{combinationId}:{childCode}";

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
            return Failure("Bounded graph candidate run timed out.", TimeoutCode);

        return null;
    }

    private static async Task AwaitActiveAfterCancellationAsync(
        IEnumerable<Task<PlanOutcome>> active)
    {
        foreach (var task in active.ToArray())
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception exception) when (IsNonFatalException(exception))
            {
                // Cancellation has already determined the public result; child details are intentionally discarded.
            }
        }
    }

    private static OperationResult<AsyncCandidateBatch> Cancelled()
        => Failure("Bounded graph candidate run was cancelled.", CancelledCode);

    private static OperationResult<AsyncCandidateBatch> Failure(string message, string code)
        => OperationResult<AsyncCandidateBatch>.Fail(message, code);

    private enum PlanOutcomeKind
    {
        Clean,
        Partial,
        Empty,
        Failed
    }

    // Structural fields prevent delimiter-containing identities from collapsing into one duplicate key.
    private readonly record struct PlanTupleKey(
        string NodeId,
        string EdgeOrderKey,
        string FlagKey);

    private sealed record PlanOutcome(
        int Index,
        PlanOutcomeKind Kind,
        IReadOnlyList<GraphCandidateOutput> Candidates,
        string[] ErrorCodes)
    {
        public static PlanOutcome Clean(int index, GraphCandidateOutput candidate)
            => new(index, PlanOutcomeKind.Clean, Array.AsReadOnly([candidate]), []);

        public static PlanOutcome Partial(
            int index,
            GraphCandidateOutput candidate,
            string[] errorCodes)
            => new(index, PlanOutcomeKind.Partial, Array.AsReadOnly([candidate]), errorCodes.ToArray());

        public static PlanOutcome Empty(int index)
            => new(index, PlanOutcomeKind.Empty, Array.AsReadOnly(Array.Empty<GraphCandidateOutput>()), []);

        public static PlanOutcome Failed(int index, string errorCode)
            => new(index, PlanOutcomeKind.Failed, Array.AsReadOnly(Array.Empty<GraphCandidateOutput>()), [errorCode]);
    }

    private sealed class ExecutionCancellationScope : IDisposable
    {
        private readonly CancellationTokenSource? _timeoutSource;
        private readonly CancellationTokenSource _runSource;

        private ExecutionCancellationScope(
            CancellationTokenSource? timeoutSource,
            CancellationTokenSource runSource)
        {
            _timeoutSource = timeoutSource;
            _runSource = runSource;
        }

        public CancellationToken Token => _runSource.Token;

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

            var runSource = tokens.Count == 0
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(tokens.ToArray());
            return new ExecutionCancellationScope(timeoutSource, runSource);
        }

        public void CancelActive()
        {
            if (!_runSource.IsCancellationRequested)
                _runSource.Cancel();
        }

        public void Dispose()
        {
            _runSource.Dispose();
            _timeoutSource?.Dispose();
        }

        private static void AddDistinct(List<CancellationToken> tokens, CancellationToken token)
        {
            if (token.CanBeCanceled && !tokens.Contains(token))
                tokens.Add(token);
        }
    }
}
