// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Collections.Concurrent;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class BoundedCombinationGraphCandidateRunnerTests
{
    private const string DependencyRequiredCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_DEPENDENCY_REQUIRED";
    private const string RequestInvalidCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_REQUEST_INVALID";
    private const string StrategyExceptionCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_STRATEGY_EXCEPTION";
    private const string StrategyResultRequiredCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_STRATEGY_RESULT_REQUIRED";
    private const string PlanInvalidCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_PLAN_INVALID";
    private const string PlanDuplicateCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_PLAN_DUPLICATE";
    private const string PlanResultInvalidCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_PLAN_RESULT_INVALID";
    private const string NoPlansCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_NO_PLANS";
    private const string AllPlansFailedCode = "DYNAMIC_GRAPH_BOUNDED_CANDIDATE_RUNNER_ALL_PLANS_FAILED";

    [Fact]
    public void PublicSurface_IsOneAsyncCandidateMethodAndExactDependenciesWithoutFactoryOrAuthority()
    {
        var type = typeof(BoundedCombinationGraphCandidateRunner<string, string>);
        var method = Assert.Single(type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var constructor = Assert.Single(type.GetConstructors());

        Assert.Equal(nameof(IGraphCandidateRunner.RunCandidatesAsync), method.Name);
        Assert.Equal(typeof(Task<OperationResult<AsyncCandidateBatch>>), method.ReturnType);
        Assert.Equal(
            [typeof(GraphCandidateRunRequest), typeof(CancellationToken)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal([typeof(IGraphCandidateRunner)], type.GetInterfaces());
        Assert.Equal(
            [
                typeof(IGraphCombinationStrategy),
                typeof(InMemoryDynamicGraphNodeRegistry<string, string>),
                typeof(Func<GraphCandidateRunRequest, CombinationPlan,
                    OperationResult<LogicalUnitRequest<string>>>)
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.DoesNotContain(
            type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            member => new[] { "Factory", "Train", "Dpo", "Teacher", "Judge", "Winner", "Select" }
                .Any(fragment => member.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task RunCandidatesAsync_BoundsConcurrencyAndReturnsOutOfOrderCompletionsInPlanOrder()
    {
        var projected = new ConcurrentBag<(GraphCandidateRunRequest Request, CombinationPlan Plan)>();
        var node = new TestNode(async (request, cancellationToken) =>
        {
            var index = ParsePlanIndex(request.RunId);
            await Task.Delay(index == 0 ? 90 : 10 + index * 5, cancellationToken);
            return Success([CreateCandidate($"node-candidate-{index}")]);
        });
        var runner = CreateRunner(
            node,
            new StaticStrategy(CreatePlans(4)),
            (request, plan) =>
            {
                projected.Add((request, plan));
                return Project(request, plan);
            });
        var request = CreateRequest(count: 4, parallelism: 2);

        var result = await runner.RunCandidatesAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("OK", batch.Status);
        Assert.Equal(4, batch.RequestedCombinationCount);
        Assert.Equal(4, batch.ActualCombinationCount);
        Assert.Equal(
            ["combination-0", "combination-1", "combination-2", "combination-3"],
            batch.Candidates.Select(candidate => candidate.CombinationId));
        Assert.Equal(4, projected.Count);
        Assert.Equal(
            ["combination-0", "combination-1", "combination-2", "combination-3"],
            projected
                .GroupBy(call => call.Plan.CombinationId, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group =>
                {
                    Assert.Single(group);
                    return group.Key;
                }));
        Assert.Equal(4, node.ExecuteCount);
        Assert.Equal(2, node.MaximumConcurrency);
        Assert.All(projected, call =>
        {
            Assert.Equal(0, call.Plan.Index);
            Assert.Equal(1, call.Request.CombinationCount);
            Assert.Equal(1, call.Request.Budget.MaxCombinations);
            Assert.Equal(1, call.Request.Budget.MaxParallelCombinationRuns);
            Assert.Null(call.Request.Budget.Timeout);
            Assert.False(call.Request.Cancellation.CanBeCanceled);
            Assert.Equal([call.Plan.NodeSetIds[0]], call.Request.InputUnitRefs);
            Assert.Equal([call.Plan.EdgeOrderKey], call.Request.InputGraphRefs);
            Assert.StartsWith("trace-a:", call.Request.TraceRef, StringComparison.Ordinal);
            Assert.Equal("scenario-a", call.Request.ScenarioId);
            Assert.Equal("query-a", call.Request.Query);
            Assert.Equal("legal-scope-a", call.Request.LegalScope);
            Assert.Equal(["provenance-a"], call.Request.ProvenanceRefs);
        });
    }

    [Fact(Timeout = 10000)]
    public async Task RunCandidatesAsync_ParallelBatchUsesOuterWallClockAndPreservesDistinctChildTimingsInPlanOrder()
    {
        const int planCount = 2;
        var bothStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = 0;
        var node = new TestNode(async (request, cancellationToken) =>
        {
            var index = ParsePlanIndex(request.RunId);
            if (Interlocked.Increment(ref started) == planCount)
                bothStarted.TrySetResult(true);
            await bothStarted.Task.WaitAsync(cancellationToken);
            await Task.Delay(index == 0 ? 90 : 180, cancellationToken);
            return Success([CreateCandidate($"parallel-candidate-{index}")]);
        });
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(planCount)));

        var result = await runner.RunCandidatesAsync(CreateRequest(planCount, planCount));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("OK", batch.Status);
        Assert.Equal(
            ["combination-0", "combination-1"],
            batch.Candidates.Select(candidate => candidate.CombinationId));
        Assert.All(batch.Candidates, candidate => Assert.True(candidate.ElapsedMs >= 50));
        Assert.NotEqual(batch.Candidates[0].ElapsedMs, batch.Candidates[1].ElapsedMs);
        var childSum = batch.Candidates.Sum(candidate => candidate.ElapsedMs);
        Assert.True(
            batch.ElapsedMs + 40 < childSum,
            $"Expected outer wall clock {batch.ElapsedMs} ms to be materially below child sum {childSum} ms.");
        Assert.True(batch.ElapsedMs >= batch.Candidates.Max(candidate => candidate.ElapsedMs));
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    public void ValidateChildResult_RejectsNegativeBatchOrCandidateTimingWithStablePlanResultCode(
        long batchElapsedMs,
        long candidateElapsedMs)
    {
        var plan = CreatePlans(2)[0];
        var output = CreateTimedOutput(plan, candidateElapsedMs);
        var request = CreateRequest(2, 1) with
        {
            CombinationCount = 1,
            Budget = new AsyncCombinationBudget(1, null, 2, 1),
            InputGraphRefs = [plan.EdgeOrderKey],
            InputUnitRefs = plan.NodeSetIds.ToArray(),
            TraceRef = "trace-child-timing"
        };
        var batch = new AsyncCandidateBatch(
            BatchId: request.TraceRef,
            Request: request,
            Candidates: [output],
            CandidateStream: null,
            RequestedCombinationCount: 1,
            ActualCombinationCount: 1,
            Status: "OK",
            ErrorCodes: [],
            ElapsedMs: batchElapsedMs);
        var method = typeof(BoundedCombinationGraphCandidateRunner<string, string>)
            .GetMethod("ValidateChildResult", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var outcome = method.Invoke(
            null,
            [plan, OperationResult<AsyncCandidateBatch>.Ok(batch)]);

        Assert.NotNull(outcome);
        var outcomeType = outcome.GetType();
        Assert.Equal("Failed", outcomeType.GetProperty("Kind")!.GetValue(outcome)!.ToString());
        Assert.Equal(
            [$"P0:combination-0:{PlanResultInvalidCode}"],
            Assert.IsType<string[]>(outcomeType.GetProperty("ErrorCodes")!.GetValue(outcome)));
    }

    [Fact]
    public async Task RunCandidatesAsync_StrategyGetsOwnedSnapshotAndBatchPreservesOuterRequest()
    {
        GraphCandidateRunRequest? strategyRequest = null;
        var sourceRequest = CreateRequest(2, 1);
        var strategy = new DelegateStrategy(request =>
        {
            strategyRequest = request;
            request.InputGraphRefs[0] = "strategy-mutated-edge";
            request.InputUnitRefs[0] = "strategy-mutated-node";
            request.ProvenanceRefs[0] = "strategy-mutated-provenance";
            return OperationResult<IReadOnlyList<CombinationPlan>>.Ok(CreatePlans(2));
        });
        var node = new TestNode((_, _) => Task.FromResult(Success([CreateCandidate("candidate-a")])));
        var runner = CreateRunner(node, strategy);

        var result = await runner.RunCandidatesAsync(sourceRequest);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.NotNull(strategyRequest);
        Assert.NotSame(sourceRequest, strategyRequest);
        Assert.NotSame(sourceRequest.InputGraphRefs, strategyRequest.InputGraphRefs);
        Assert.Equal("edge-a", sourceRequest.InputGraphRefs[0]);
        Assert.Equal("node-a", sourceRequest.InputUnitRefs[0]);
        Assert.Equal("edge-a", batch.Request.InputGraphRefs[0]);
        Assert.Equal("node-a", batch.Request.InputUnitRefs[0]);
        Assert.Equal("provenance-a", batch.Request.ProvenanceRefs[0]);
        Assert.NotSame(sourceRequest, batch.Request);
    }

    [Fact]
    public async Task RunCandidatesAsync_NoPlansReturnsTypedEmptyBatchWithStableEvidenceCode()
    {
        var node = new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable());
        var runner = CreateRunner(node, new StaticStrategy([]));

        var result = await runner.RunCandidatesAsync(CreateRequest(3, 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("EMPTY_CANDIDATES", batch.Status);
        Assert.Equal(3, batch.RequestedCombinationCount);
        Assert.Equal(0, batch.ActualCombinationCount);
        Assert.Empty(batch.Candidates);
        Assert.Equal([NoPlansCode], batch.ErrorCodes);
        Assert.Equal(0, node.ExecuteCount);
    }

    [Fact]
    public async Task RunCandidatesAsync_FewerPlansThanRequestedDoesNotPadAndMarksPartial()
    {
        var node = new TestNode((request, _) => Task.FromResult(
            Success([CreateCandidate($"candidate-{ParsePlanIndex(request.RunId)}")])));
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(2)));

        var result = await runner.RunCandidatesAsync(CreateRequest(4, 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("PARTIAL_FAILURE", batch.Status);
        Assert.Equal(4, batch.RequestedCombinationCount);
        Assert.Equal(2, batch.ActualCombinationCount);
        Assert.Equal(2, batch.Candidates.Count);
        Assert.Equal(2, node.ExecuteCount);
    }

    [Fact]
    public async Task RunCandidatesAsync_MixedCandidateEmptyAndFailureReturnsOrderedPartialBatch()
    {
        var plans = new[]
        {
            CreatePlan(0, new LogicalExecutionFlags { UseConfidenceFloor = false, UseContextCarryForward = false }),
            CreatePlan(1, new LogicalExecutionFlags { UseConfidenceFloor = false, UseContextCarryForward = true }),
            CreatePlan(2, new LogicalExecutionFlags { UseConfidenceFloor = true, UseContextCarryForward = false })
        };
        var node = new TestNode((request, _) =>
        {
            if (request.Flags.UseConfidenceFloor)
                return Task.FromResult(FailedNode("NODE_FAILED"));
            if (request.Flags.UseContextCarryForward)
                return Task.FromResult(Success([]));
            return Task.FromResult(Success([CreateCandidate("candidate-ok")]));
        });
        var runner = CreateRunner(node, new StaticStrategy(plans));

        var result = await runner.RunCandidatesAsync(CreateRequest(3, 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("PARTIAL_FAILURE", batch.Status);
        Assert.Equal(1, batch.ActualCombinationCount);
        Assert.Equal("combination-0", Assert.Single(batch.Candidates).CombinationId);
        Assert.Equal(["P2:combination-2:NODE_FAILED"], batch.ErrorCodes);
    }

    [Fact]
    public async Task RunCandidatesAsync_AllSuccessfulEmptyPlansReturnEmptyBatch()
    {
        var node = new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable());
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(3)));

        var result = await runner.RunCandidatesAsync(CreateRequest(3, 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("EMPTY_CANDIDATES", batch.Status);
        Assert.Equal(0, batch.ActualCombinationCount);
        Assert.Empty(batch.Candidates);
        Assert.Empty(batch.ErrorCodes);
    }

    [Fact]
    public async Task RunCandidatesAsync_AllFailedPlansReturnOneStableTopLevelFailure()
    {
        var node = new TestNode((_, _) => Task.FromResult(FailedNode("NODE_FAILED")));
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(3)));

        var result = await runner.RunCandidatesAsync(CreateRequest(3, 2));

        AssertFailure(result, AllPlansFailedCode);
        Assert.Equal("All bounded graph candidate plans failed.", result.ErrorMessage);
    }

    [Fact]
    public async Task RunCandidatesAsync_ChildPartialCodesAreNormalizedAndPrefixedByPlan()
    {
        var plans = CreatePlans(2);
        var node = new TestNode((request, _) =>
        {
            var index = ParsePlanIndex(request.RunId);
            return Task.FromResult(Success([
                CreateCandidate($"candidate-{index}", index == 0 ? ["NODE_WARNING"] : [])
            ]));
        });
        var runner = CreateRunner(node, new StaticStrategy(plans));

        var result = await runner.RunCandidatesAsync(CreateRequest(2, 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("PARTIAL_FAILURE", batch.Status);
        Assert.Equal(2, batch.ActualCombinationCount);
        Assert.Equal(["P0:combination-0:NODE_WARNING"], batch.ErrorCodes);
    }

    [Fact]
    public async Task RunCandidatesAsync_InvalidChildCodeUsesGenericStablePlanResultCode()
    {
        var node = new TestNode((request, _) =>
        {
            var index = ParsePlanIndex(request.RunId);
            return Task.FromResult(index == 0
                ? FailedNode("bad-code")
                : Success([CreateCandidate("candidate-ok")]));
        });
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(2)));

        var result = await runner.RunCandidatesAsync(CreateRequest(2, 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("PARTIAL_FAILURE", batch.Status);
        Assert.Equal(
            [$"P0:combination-0:{PlanResultInvalidCode}"],
            batch.ErrorCodes);
    }

    [Fact]
    public async Task RunCandidatesAsync_PartialSuccessPreservesSuccessfulChildTiming()
    {
        var node = new TestNode(async (request, cancellationToken) =>
        {
            var index = ParsePlanIndex(request.RunId);
            if (index == 1)
                return FailedNode("NODE_FAILED");
            await Task.Delay(80, cancellationToken);
            return Success([CreateCandidate("timed-partial-candidate")]);
        });
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(2)));

        var result = await runner.RunCandidatesAsync(CreateRequest(2, 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("PARTIAL_FAILURE", batch.Status);
        var candidate = Assert.Single(batch.Candidates);
        Assert.Equal("combination-0", candidate.CombinationId);
        Assert.True(candidate.ElapsedMs >= 50, $"Observed only {candidate.ElapsedMs} ms.");
        Assert.True(batch.ElapsedMs >= candidate.ElapsedMs);
        Assert.Equal(["P1:combination-1:NODE_FAILED"], batch.ErrorCodes);
    }

    [Fact]
    public async Task RunCandidatesAsync_MalformedCandidateIdentityBecomesPlanFailureWithoutLeakingDetails()
    {
        var node = new TestNode((request, _) =>
        {
            var index = ParsePlanIndex(request.RunId);
            return Task.FromResult(index == 0
                ? Success([CreateCandidate(" ")])
                : Success([CreateCandidate("candidate-ok")]));
        });
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(2)));

        var result = await runner.RunCandidatesAsync(CreateRequest(2, 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("PARTIAL_FAILURE", batch.Status);
        Assert.Equal(1, batch.ActualCombinationCount);
        Assert.Equal(
            ["P0:combination-0:DYNAMIC_GRAPH_CANDIDATE_RUNNER_MATERIALIZATION_EXCEPTION"],
            batch.ErrorCodes);
    }

    [Fact]
    public async Task RunCandidatesAsync_MethodOrEmbeddedCancellationReturnsCancelledWithoutBatch()
    {
        foreach (var embedded in new[] { false, true })
        {
            const int planCount = 5;
            const int parallelism = 2;
            using var source = new CancellationTokenSource();
            var initialWaveStarted = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var startedCount = 0;
            var completedCount = 0;
            var node = new TestNode(async (_, cancellationToken) =>
            {
                if (Interlocked.Increment(ref startedCount) == parallelism)
                    initialWaveStarted.TrySetResult(true);
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    return Success([]);
                }
                finally
                {
                    Interlocked.Increment(ref completedCount);
                }
            });
            var runner = CreateRunner(node, new StaticStrategy(CreatePlans(planCount)));
            var request = CreateRequest(planCount, parallelism) with
            {
                Cancellation = embedded ? source.Token : CancellationToken.None
            };
            var runTask = runner.RunCandidatesAsync(
                request,
                embedded ? CancellationToken.None : source.Token);

            await initialWaveStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
            source.Cancel();
            var result = await runTask.WaitAsync(TimeSpan.FromSeconds(2));

            AssertFailure(result, "CANCELLED");
            Assert.Equal(parallelism, node.ExecuteCount);
            Assert.Equal(parallelism, Volatile.Read(ref startedCount));
            Assert.Equal(parallelism, Volatile.Read(ref completedCount));
            Assert.Equal(0, node.ActiveCount);
        }
    }

    [Fact]
    public async Task RunCandidatesAsync_GlobalTimeoutReturnsTimeoutWithoutBatch()
    {
        const int planCount = 5;
        const int parallelism = 2;
        var initialWaveStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var startedCount = 0;
        var completedCount = 0;
        var node = new TestNode(async (_, cancellationToken) =>
        {
            if (Interlocked.Increment(ref startedCount) == parallelism)
                initialWaveStarted.TrySetResult(true);
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return Success([]);
            }
            finally
            {
                Interlocked.Increment(ref completedCount);
            }
        });
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(planCount)));
        var request = CreateRequest(planCount, parallelism);
        request = request with
        {
            Budget = request.Budget with { Timeout = TimeSpan.FromMilliseconds(400) }
        };
        var runTask = runner.RunCandidatesAsync(request);

        await initialWaveStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var result = await runTask.WaitAsync(TimeSpan.FromSeconds(2));

        AssertFailure(result, "TIMEOUT");
        Assert.Equal(parallelism, node.ExecuteCount);
        Assert.Equal(parallelism, Volatile.Read(ref startedCount));
        Assert.Equal(parallelism, Volatile.Read(ref completedCount));
        Assert.Equal(0, node.ActiveCount);
    }

    [Fact]
    public async Task RunCandidatesAsync_UserCancellationTakesPrecedenceOverTimeout()
    {
        const int planCount = 4;
        using var source = new CancellationTokenSource();
        var firstPlanStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var completedCount = 0;
        var node = new TestNode(async (_, cancellationToken) =>
        {
            firstPlanStarted.TrySetResult(true);
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return Success([]);
            }
            finally
            {
                Interlocked.Increment(ref completedCount);
            }
        });
        var runner = CreateRunner(node, new StaticStrategy(CreatePlans(planCount)));
        var baseRequest = CreateRequest(planCount, 1);
        var request = baseRequest with
        {
            Cancellation = source.Token,
            Budget = baseRequest.Budget with { Timeout = TimeSpan.FromSeconds(5) }
        };
        var runTask = runner.RunCandidatesAsync(request);

        await firstPlanStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        source.Cancel();
        var result = await runTask.WaitAsync(TimeSpan.FromSeconds(2));

        AssertFailure(result, "CANCELLED");
        Assert.Equal(1, node.ExecuteCount);
        Assert.Equal(1, Volatile.Read(ref completedCount));
        Assert.Equal(0, node.ActiveCount);
    }

    [Fact]
    public async Task RunCandidatesAsync_StrategyFailuresAndExceptionsAreStableAndSanitized()
    {
        var node = new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable());
        var cases = new (IGraphCombinationStrategy Strategy, string Code)[]
        {
            (new DelegateStrategy(_ => throw new InvalidOperationException("secret-strategy-detail")), StrategyExceptionCode),
            (new NullStrategy(returnSuccessfulNull: false), StrategyResultRequiredCode),
            (new NullStrategy(returnSuccessfulNull: true), StrategyResultRequiredCode),
            (new DelegateStrategy(_ => OperationResult<IReadOnlyList<CombinationPlan>>.Fail(
                "strategy rejected request", "STRATEGY_REJECTED")), "STRATEGY_REJECTED")
        };

        foreach (var testCase in cases)
        {
            var result = await CreateRunner(node, testCase.Strategy)
                .RunCandidatesAsync(CreateRequest(2, 1));
            AssertFailure(result, testCase.Code);
            Assert.DoesNotContain("secret-strategy-detail", result.ErrorMessage, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task RunCandidatesAsync_DelimiterContainingPlanFieldsRemainStructurallyDistinct()
    {
        const string firstNodeId = "node-a";
        const string firstEdgeKey = "edge-a\nsegment-c";
        const string secondNodeId = "node-b";
        const string secondEdgeKey = "edge-b\nsegment-c";
        var flags = new LogicalExecutionFlags { UseDeterministicFixtureBaseline = true };
        var plans = new[]
        {
            new CombinationPlan(0, "combination-newline-a", [firstNodeId], firstEdgeKey, flags),
            new CombinationPlan(1, "combination-newline-b", [secondNodeId], secondEdgeKey, flags with { })
        };
        var firstNode = new TestNode(
            (request, _) => Task.FromResult(Success(
                [CreateCandidate("candidate-newline-a", nodeId: firstNodeId)],
                request.UnitId)),
            firstNodeId);
        var secondNode = new TestNode(
            (request, _) => Task.FromResult(Success(
                [CreateCandidate("candidate-newline-b", nodeId: secondNodeId)],
                request.UnitId)),
            secondNodeId);
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        Assert.True(registry.Register(firstNode).IsSuccess);
        Assert.True(registry.Register(secondNode).IsSuccess);
        var runner = new BoundedCombinationGraphCandidateRunner<string, string>(
            new StaticStrategy(plans),
            registry,
            Project);
        var request = CreateRequest(2, 2) with
        {
            InputUnitRefs = [firstNodeId, secondNodeId],
            InputGraphRefs = [firstEdgeKey, secondEdgeKey]
        };

        var result = await runner.RunCandidatesAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("OK", batch.Status);
        Assert.Equal(2, batch.ActualCombinationCount);
        Assert.Equal(
            ["combination-newline-a", "combination-newline-b"],
            batch.Candidates.Select(candidate => candidate.CombinationId));
        Assert.Equal([firstNodeId, secondNodeId], batch.Candidates.Select(candidate => candidate.UnitId));
        Assert.Equal(1, firstNode.ExecuteCount);
        Assert.Equal(1, secondNode.ExecuteCount);
    }

    [Fact]
    public async Task RunCandidatesAsync_RejectsMalformedOrDuplicatePlansBeforeProjectionAndExecution()
    {
        var projectionCount = 0;
        var node = new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable());
        var valid = CreatePlans(2);
        var duplicateId = new[]
        {
            valid[0],
            valid[1] with { CombinationId = valid[0].CombinationId }
        };
        var duplicateTuple = new[]
        {
            valid[0],
            valid[0] with { Index = 1, CombinationId = "different-id" }
        };
        var cases = new (IReadOnlyList<CombinationPlan> Plans, string Code)[]
        {
            (new[] { valid[0] with { Index = 1 }, valid[1] }, PlanInvalidCode),
            (new[] { valid[0] with { NodeSetIds = [] }, valid[1] }, PlanInvalidCode),
            (new[] { valid[0] with { NodeSetIds = ["outside-node"] }, valid[1] }, PlanInvalidCode),
            (new[] { valid[0] with { EdgeOrderKey = "outside-edge" }, valid[1] }, PlanInvalidCode),
            (new[] { valid[0] with { Flags = null! }, valid[1] }, PlanInvalidCode),
            (duplicateId, PlanDuplicateCode),
            (duplicateTuple, PlanDuplicateCode)
        };

        foreach (var testCase in cases)
        {
            var runner = CreateRunner(
                node,
                new StaticStrategy(testCase.Plans),
                (request, plan) =>
                {
                    projectionCount++;
                    return Project(request, plan);
                });
            var result = await runner.RunCandidatesAsync(CreateRequest(2, 1));
            AssertFailure(result, testCase.Code);
        }

        Assert.Equal(0, projectionCount);
        Assert.Equal(0, node.ExecuteCount);
    }

    [Fact]
    public async Task RunCandidatesAsync_ConstructorAndRequestValidationUseStableCodes()
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var strategy = new StaticStrategy(CreatePlans(2));
        var runners = new[]
        {
            new BoundedCombinationGraphCandidateRunner<string, string>(null!, registry, Project),
            new BoundedCombinationGraphCandidateRunner<string, string>(strategy, null!, Project),
            new BoundedCombinationGraphCandidateRunner<string, string>(strategy, registry, null!)
        };
        foreach (var runner in runners)
            AssertFailure(await runner.RunCandidatesAsync(CreateRequest(2, 1)), DependencyRequiredCode);

        var validRunner = CreateRunner(
            new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable()),
            strategy);
        var valid = CreateRequest(2, 1);
        var invalidRequests = new[]
        {
            null!,
            valid with { ScenarioId = " " },
            valid with { Query = " " },
            valid with { InputGraphRefs = [] },
            valid with { InputUnitRefs = [] },
            valid with { CombinationCount = 1 },
            valid with { CombinationCount = 9 },
            valid with { Budget = valid.Budget with { MaxCombinations = 1 } },
            valid with { Budget = valid.Budget with { MaxCombinations = 9 } },
            valid with { Budget = valid.Budget with { MaxParallelCombinationRuns = 0 } },
            valid with { Budget = valid.Budget with { Timeout = TimeSpan.Zero } },
            valid with { LegalScope = " " },
            valid with { ProvenanceRefs = [] },
            valid with { TraceRef = " " }
        };
        foreach (var request in invalidRequests)
            AssertFailure(await validRunner.RunCandidatesAsync(request), RequestInvalidCode);
    }

    [Fact]
    public async Task RunCandidatesAsync_ProjectorExceptionIsSanitizedAndBecomesAllPlansFailed()
    {
        var node = new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable());
        var runner = CreateRunner(
            node,
            new StaticStrategy(CreatePlans(2)),
            (_, _) => throw new InvalidOperationException("secret-projector-detail"));

        var result = await runner.RunCandidatesAsync(CreateRequest(2, 2));

        AssertFailure(result, AllPlansFailedCode);
        Assert.DoesNotContain("secret-projector-detail", result.ErrorMessage, StringComparison.Ordinal);
    }

    private static BoundedCombinationGraphCandidateRunner<string, string> CreateRunner(
        TestNode node,
        IGraphCombinationStrategy strategy,
        Func<GraphCandidateRunRequest, CombinationPlan,
            OperationResult<LogicalUnitRequest<string>>>? projector = null)
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var registration = registry.Register(node);
        if (!registration.IsSuccess)
            throw new InvalidOperationException($"Test node registration failed: {registration.ErrorCode}");

        return new BoundedCombinationGraphCandidateRunner<string, string>(
            strategy,
            registry,
            projector ?? Project);
    }

    private static OperationResult<LogicalUnitRequest<string>> Project(
        GraphCandidateRunRequest request,
        CombinationPlan plan)
        => OperationResult<LogicalUnitRequest<string>>.Ok(
            new LogicalUnitRequest<string>(
                RunId: request.TraceRef,
                UnitId: plan.NodeSetIds[0],
                UnitRole: "baseline_candidate_generator",
                DomainTag: "domain-a",
                Input: request.Query,
                Flags: plan.Flags,
                Metadata: new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["trace"] = request.TraceRef
                }));

    private static GraphCandidateRunRequest CreateRequest(int count, int parallelism)
        => new(
            ScenarioId: "scenario-a",
            Query: "query-a",
            InputGraphRefs: ["edge-a"],
            InputUnitRefs: ["node-a"],
            Budget: new AsyncCombinationBudget(
                MaxCombinations: count,
                Timeout: null,
                MaxParallelBranches: 2,
                MaxParallelCombinationRuns: parallelism),
            CombinationCount: count,
            Cancellation: CancellationToken.None,
            LegalScope: "legal-scope-a",
            ProvenanceRefs: ["provenance-a"],
            TraceRef: "trace-a");

    private static CombinationPlan[] CreatePlans(int count)
        => Enumerable.Range(0, count)
            .Select(index => CreatePlan(
                index,
                new LogicalExecutionFlags
                {
                    UseDeterministicFixtureBaseline = true,
                    UseConfidenceFloor = (index & 1) != 0,
                    UseContextCarryForward = (index & 2) != 0,
                    UseE35DependencyDetection = (index & 4) != 0
                }))
            .ToArray();

    private static CombinationPlan CreatePlan(int index, LogicalExecutionFlags flags)
        => new(
            Index: index,
            CombinationId: $"combination-{index}",
            NodeSetIds: ["node-a"],
            EdgeOrderKey: "edge-a",
            Flags: flags);

    private static int ParsePlanIndex(string traceRef)
        => int.Parse(traceRef.Split(':', StringSplitOptions.None)[1]);

    private static DynamicGraphNodeDescriptor CreateDescriptor(string nodeId = "node-a")
        => new(
            NodeId: nodeId,
            DomainTag: "domain-a",
            TInputTypeName: nameof(String),
            TOutputTypeName: nameof(String),
            IsTrained: false,
            DeterministicFinalAuthority: false,
            RuntimeSemanticQualityClaimed: false,
            AuthorityRole: "baseline_candidate_generator",
            CheckpointRef: null,
            ManifestRef: null);

    private static GraphNodeCandidate CreateCandidate(
        string candidateId,
        string[]? errorCodes = null,
        string nodeId = "node-a")
        => new(
            CandidateId: candidateId,
            UnitId: nodeId,
            UnitIds: [nodeId],
            LogicalUnitRef: "logical-unit-a",
            OutputRefs: [$"output-{candidateId}"],
            OutputType: nameof(String),
            Confidence: 0.7,
            Score: null,
            ProvenanceRefs: ["candidate-provenance-a"],
            PathOrderRef: "candidate-path-a",
            LegalStatus: "ADMITTED",
            Status: errorCodes is { Length: > 0 } ? "PARTIAL_FAILURE" : "OK",
            ErrorCodes: errorCodes ?? [],
            SourceIds: ["source-a"],
            TraceRef: $"trace-{candidateId}",
            TraceRefs: [$"trace-{candidateId}"],
            EvidenceIds: [$"evidence-{candidateId}"],
            B1TeacherJudgeRef: "not-invoked",
            B7GraphOrderRef: "graph-order-a",
            B11AsyncBatchRef: "batch-a",
            B12WinnerSelectionRef: "not-invoked",
            B6UnitDpoBuilderRef: "not-invoked",
            SemanticQualityClaimed: false);

    private static GraphCandidateOutput CreateTimedOutput(CombinationPlan plan, long elapsedMs)
        => new(
            CandidateId: plan.CombinationId,
            OutputRefs: [CreateCandidate("timed-candidate", nodeId: plan.NodeSetIds[0])],
            UnitId: plan.NodeSetIds[0],
            UnitIds: plan.NodeSetIds.ToArray(),
            Confidence: 0.7,
            Score: null,
            CombinationId: plan.CombinationId,
            ProvenanceRefs: ["provenance-a"],
            PathTraceRef: plan.EdgeOrderKey,
            TraceRef: "trace-child-timing",
            ErrorCodes: [],
            ElapsedMs: elapsedMs);

    private static OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>> Success(
        IEnumerable<GraphNodeCandidate> candidates,
        string nodeId = "node-a")
        => OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Ok(
            new LogicalUnitResult<IQueryable<GraphNodeCandidate>>(
                UnitId: nodeId,
                UnitRole: "baseline_candidate_generator",
                DomainTag: "domain-a",
                Output: candidates.AsQueryable(),
                Confidence: 0.75,
                PredicateIds: [],
                DomainTags: ["domain-a"],
                EvidenceIds: [],
                Warnings: []));

    private static OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>> FailedNode(
        string code)
        => OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Fail(
            "Node execution failed.",
            code);

    private static void AssertFailure<T>(OperationResult<T> result, string code)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(code, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private sealed class StaticStrategy : IGraphCombinationStrategy
    {
        private readonly IReadOnlyList<CombinationPlan> _plans;

        public StaticStrategy(IReadOnlyList<CombinationPlan> plans)
        {
            _plans = plans;
        }

        public OperationResult<IReadOnlyList<CombinationPlan>> Enumerate(GraphCandidateRunRequest request)
            => OperationResult<IReadOnlyList<CombinationPlan>>.Ok(_plans);
    }

    private sealed class DelegateStrategy : IGraphCombinationStrategy
    {
        private readonly Func<GraphCandidateRunRequest,
            OperationResult<IReadOnlyList<CombinationPlan>>> _enumerate;

        public DelegateStrategy(Func<GraphCandidateRunRequest,
            OperationResult<IReadOnlyList<CombinationPlan>>> enumerate)
        {
            _enumerate = enumerate;
        }

        public OperationResult<IReadOnlyList<CombinationPlan>> Enumerate(GraphCandidateRunRequest request)
            => _enumerate(request);
    }

    private sealed class NullStrategy : IGraphCombinationStrategy
    {
        private readonly bool _returnSuccessfulNull;

        public NullStrategy(bool returnSuccessfulNull)
        {
            _returnSuccessfulNull = returnSuccessfulNull;
        }

        public OperationResult<IReadOnlyList<CombinationPlan>> Enumerate(GraphCandidateRunRequest request)
            => _returnSuccessfulNull
                ? OperationResult<IReadOnlyList<CombinationPlan>>.Ok(null!)
                : null!;
    }

    private sealed class TestNode : IDynamicGraphNode<string, string>
    {
        private readonly Func<LogicalUnitRequest<string>, CancellationToken,
            Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>>> _execute;
        private int _executeCount;
        private int _activeCount;
        private int _maximumConcurrency;
        private readonly string _nodeId;

        public TestNode(IQueryable<GraphNodeCandidate> output, string nodeId = "node-a")
            : this((_, _) => Task.FromResult(Success(output, nodeId)), nodeId)
        {
        }

        public TestNode(Func<LogicalUnitRequest<string>, CancellationToken,
            Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>>> execute,
            string nodeId = "node-a")
        {
            _execute = execute;
            _nodeId = nodeId;
            Descriptor = CreateDescriptor(nodeId);
        }

        public string NodeId => _nodeId;

        public DynamicGraphNodeDescriptor Descriptor { get; }

        public int ExecuteCount => Volatile.Read(ref _executeCount);

        public int MaximumConcurrency => Volatile.Read(ref _maximumConcurrency);

        public int ActiveCount => Volatile.Read(ref _activeCount);

        public async Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _executeCount);
            var active = Interlocked.Increment(ref _activeCount);
            UpdateMaximum(active);
            try
            {
                return await _execute(request, cancellationToken);
            }
            finally
            {
                Interlocked.Decrement(ref _activeCount);
            }
        }

        private void UpdateMaximum(int value)
        {
            var observed = Volatile.Read(ref _maximumConcurrency);
            while (value > observed)
            {
                var previous = Interlocked.CompareExchange(ref _maximumConcurrency, value, observed);
                if (previous == observed)
                    return;
                observed = previous;
            }
        }
    }
}
