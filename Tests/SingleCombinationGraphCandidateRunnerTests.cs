// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class SingleCombinationGraphCandidateRunnerTests
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

    [Fact]
    public void PublicSurface_IsOneAsyncCandidateMethodWithoutMultiPlanOrchestrationOrTrainingAuthority()
    {
        var type = typeof(SingleCombinationGraphCandidateRunner<string, string>);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var method = Assert.Single(methods);

        Assert.Equal(nameof(IGraphCandidateRunner.RunCandidatesAsync), method.Name);
        Assert.Equal(typeof(Task<OperationResult<AsyncCandidateBatch>>), method.ReturnType);
        Assert.Equal(
            [typeof(GraphCandidateRunRequest), typeof(CancellationToken)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal([typeof(IGraphCandidateRunner)], type.GetInterfaces());
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.DoesNotContain(
            methods,
            candidate => new[] { "Plans", "Orchestr", "Train", "Dpo", "Judge", "Winner", "Select" }
                .Any(fragment => candidate.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase)));

        var constructor = Assert.Single(type.GetConstructors());
        Assert.Equal(
            1,
            constructor.GetParameters().Count(parameter => parameter.ParameterType == typeof(CombinationPlan)));
        Assert.DoesNotContain(
            constructor.GetParameters(),
            parameter => parameter.ParameterType.IsArray
                         && parameter.ParameterType.GetElementType() == typeof(CombinationPlan));
    }

    [Fact]
    public async Task HappyPath_MapsAndExecutesExactlyOncePreservesOrderAndOwnsSnapshots()
    {
        var first = CreateCandidate("candidate-2", errorCodes: []);
        var second = CreateCandidate("candidate-1", errorCodes: []);
        var source = new CountingEnumerable<GraphNodeCandidate>([first, second]);
        var node = new TestNode((_, _) => Task.FromResult(Success(source.AsQueryable(), 0.83)));
        var plan = CreatePlan();
        var originalPlanNodeIds = plan.NodeSetIds;
        GraphCandidateRunRequest? projectedRunRequest = null;
        CombinationPlan? projectedPlan = null;
        LogicalUnitRequest<string>? projectedLogicalRequest = null;
        var projectorCalls = 0;
        var runner = CreateRunner(
            node,
            plan,
            (request, planSnapshot) =>
            {
                projectorCalls++;
                projectedRunRequest = request;
                projectedPlan = planSnapshot;
                projectedLogicalRequest = CreateProjectedRequest(request, planSnapshot);
                request.InputGraphRefs[0] = "projector-mutated-graph";
                planSnapshot.NodeSetIds[0] = "projector-mutated-node";
                return OperationResult<LogicalUnitRequest<string>>.Ok(projectedLogicalRequest);
            });

        originalPlanNodeIds[0] = "caller-mutated-node";
        var request = CreateRequest();
        var result = await runner.RunCandidatesAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, projectorCalls);
        Assert.Equal(1, node.ExecuteCount);
        Assert.Equal(1, source.EnumerationCount);
        Assert.NotNull(projectedRunRequest);
        Assert.NotNull(projectedPlan);
        Assert.NotNull(projectedLogicalRequest);
        Assert.NotSame(request, projectedRunRequest);
        Assert.NotSame(request.InputGraphRefs, projectedRunRequest.InputGraphRefs);
        Assert.NotSame(plan, projectedPlan);
        Assert.NotSame(plan.Flags, projectedPlan.Flags);
        Assert.NotSame(projectedLogicalRequest, node.LastRequest);
        Assert.NotSame(projectedLogicalRequest.Metadata, node.LastRequest!.Metadata);
        Assert.Equal("node-a", node.LastRequest.UnitId);
        Assert.Equal("metadata-value", node.LastRequest.Metadata["metadata-key"]);

        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("trace-a", batch.BatchId);
        Assert.Equal(1, batch.RequestedCombinationCount);
        Assert.Equal(1, batch.ActualCombinationCount);
        Assert.Equal("OK", batch.Status);
        Assert.Empty(batch.ErrorCodes);
        Assert.Null(batch.CandidateStream);
        Assert.Equal("graph-a", batch.Request.InputGraphRefs[0]);
        Assert.Equal("node-a", batch.Request.InputUnitRefs[0]);
        Assert.Equal("provenance-a", batch.Request.ProvenanceRefs[0]);
        Assert.NotSame(request, batch.Request);
        Assert.NotSame(request.InputGraphRefs, batch.Request.InputGraphRefs);

        var output = Assert.Single(batch.Candidates);
        Assert.Equal("combination-a", output.CandidateId);
        Assert.Equal("combination-a", output.CombinationId);
        Assert.Equal("node-a", output.UnitId);
        Assert.Equal(["node-a"], output.UnitIds);
        Assert.Equal(0.83, output.Confidence);
        Assert.Null(output.Score);
        Assert.Equal(["provenance-a"], output.ProvenanceRefs);
        Assert.Equal("edge-order-a", output.PathTraceRef);
        Assert.Equal("trace-a", output.TraceRef);
        Assert.Empty(output.ErrorCodes);
        Assert.Equal(["candidate-2", "candidate-1"], output.OutputRefs.Select(candidate => candidate.CandidateId));
        Assert.True(batch.ElapsedMs >= 0);
        Assert.Equal(batch.ElapsedMs, output.ElapsedMs);
        Assert.NotSame(first, output.OutputRefs[0]);
        Assert.NotSame(second, output.OutputRefs[1]);

        request.InputGraphRefs[0] = "caller-mutated-graph";
        request.InputUnitRefs[0] = "caller-mutated-unit";
        request.ProvenanceRefs[0] = "caller-mutated-provenance";
        ((Dictionary<string, object>)projectedLogicalRequest.Metadata)["metadata-key"] = "mutated-metadata";
        first.OutputRefs[0] = "mutated-output";

        Assert.Equal("graph-a", batch.Request.InputGraphRefs[0]);
        Assert.Equal("node-a", batch.Request.InputUnitRefs[0]);
        Assert.Equal("provenance-a", output.ProvenanceRefs[0]);
        Assert.Equal("metadata-value", node.LastRequest.Metadata["metadata-key"]);
        Assert.Equal("output-candidate-2", output.OutputRefs[0].OutputRefs[0]);
        Assert.Empty(output.OutputRefs[0].ErrorCodes);
    }

    [Fact]
    public async Task SuccessfulTiming_IncludesAsyncNodeExecution()
    {
        var node = new TestNode(async (_, cancellationToken) =>
        {
            await Task.Delay(80, cancellationToken);
            return Success(new[] { CreateCandidate("candidate-delayed-node") }.AsQueryable());
        });
        var runner = CreateRunner(node);

        var result = await runner.RunCandidatesAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        var candidate = Assert.Single(batch.Candidates);
        Assert.True(batch.ElapsedMs >= 50, $"Observed only {batch.ElapsedMs} ms.");
        Assert.Equal(batch.ElapsedMs, candidate.ElapsedMs);
    }

    [Fact]
    public async Task SuccessfulTiming_IncludesDelayedQueryableMaterializationAndSnapshot()
    {
        var source = new DelayedEnumerable<GraphNodeCandidate>(
            [CreateCandidate("candidate-delayed-materialization")],
            TimeSpan.FromMilliseconds(80));
        var runner = CreateRunner(new TestNode(source.AsQueryable()));

        var result = await runner.RunCandidatesAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        var candidate = Assert.Single(batch.Candidates);
        Assert.Equal(1, source.EnumerationCount);
        Assert.True(batch.ElapsedMs >= 50, $"Observed only {batch.ElapsedMs} ms.");
        Assert.Equal(batch.ElapsedMs, candidate.ElapsedMs);
    }

    [Fact]
    public async Task ConstructorValidation_RejectsInvalidPlanDependenciesBeforeProjectionOrExecution()
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var projectionCalls = 0;
        OperationResult<LogicalUnitRequest<string>> Project(
            GraphCandidateRunRequest request,
            CombinationPlan plan)
        {
            projectionCalls++;
            return OperationResult<LogicalUnitRequest<string>>.Ok(CreateProjectedRequest(request, plan));
        }

        var cases = new (SingleCombinationGraphCandidateRunner<string, string> Runner, string Code)[]
        {
            (new(null!, CreatePlan(), Project), PlanInvalidCode),
            (new(registry, null!, Project), PlanInvalidCode),
            (new(registry, CreatePlan(), null!), ProjectionRequiredCode),
            (new(registry, CreatePlan() with { Index = 1 }, Project), PlanInvalidCode),
            (new(registry, CreatePlan() with { CombinationId = " " }, Project), PlanInvalidCode),
            (new(registry, CreatePlan() with { EdgeOrderKey = " " }, Project), PlanInvalidCode),
            (new(registry, CreatePlan() with { Flags = null! }, Project), PlanInvalidCode),
            (new(registry, CreatePlan() with { NodeSetIds = null! }, Project), PlanInvalidCode),
            (new(registry, CreatePlan() with { NodeSetIds = [] }, Project), NodeCountUnsupportedCode),
            (new(registry, CreatePlan() with { NodeSetIds = ["node-a", "node-b"] }, Project), NodeCountUnsupportedCode),
            (new(registry, CreatePlan() with { NodeSetIds = [" "] }, Project), PlanInvalidCode)
        };

        foreach (var testCase in cases)
        {
            var result = await testCase.Runner.RunCandidatesAsync(CreateRequest());
            AssertFailure(result, testCase.Code);
        }

        Assert.Equal(0, projectionCalls);
    }

    [Fact]
    public async Task RequestAndBudgetValidation_RejectsBeforeProjectionOrExecutionWithStableCodes()
    {
        var node = new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable());
        var projectionCalls = 0;
        var runner = CreateRunner(
            node,
            CreatePlan(),
            (request, plan) =>
            {
                projectionCalls++;
                return OperationResult<LogicalUnitRequest<string>>.Ok(CreateProjectedRequest(request, plan));
            });
        var valid = CreateRequest();
        var cases = new (GraphCandidateRunRequest Request, string Code)[]
        {
            (null!, RequestInvalidCode),
            (valid with { ScenarioId = " " }, RequestInvalidCode),
            (valid with { Query = " " }, RequestInvalidCode),
            (valid with { LegalScope = " " }, RequestInvalidCode),
            (valid with { TraceRef = " " }, RequestInvalidCode),
            (valid with { InputGraphRefs = null! }, RequestInvalidCode),
            (valid with { InputGraphRefs = [] }, RequestInvalidCode),
            (valid with { InputGraphRefs = [" "] }, RequestInvalidCode),
            (valid with { InputUnitRefs = null! }, RequestInvalidCode),
            (valid with { InputUnitRefs = [] }, RequestInvalidCode),
            (valid with { ProvenanceRefs = null! }, RequestInvalidCode),
            (valid with { ProvenanceRefs = [] }, RequestInvalidCode),
            (valid with { Budget = null! }, RequestInvalidCode),
            (valid with { CombinationCount = 0 }, CombinationCountUnsupportedCode),
            (valid with { CombinationCount = 2 }, CombinationCountUnsupportedCode),
            (valid with { Budget = valid.Budget with { MaxParallelCombinationRuns = 2 } }, ParallelismUnsupportedCode),
            (valid with { Budget = valid.Budget with { MaxCombinations = 0 } }, RequestInvalidCode),
            (valid with { Budget = valid.Budget with { MaxParallelBranches = 0 } }, RequestInvalidCode),
            (valid with { Budget = valid.Budget with { Timeout = TimeSpan.Zero } }, RequestInvalidCode),
            (valid with { Budget = valid.Budget with { Timeout = TimeSpan.FromMilliseconds(-1) } }, RequestInvalidCode),
            (valid with { InputUnitRefs = ["node-a", "node-b"] }, RequestInvalidCode),
            (valid with { InputUnitRefs = ["NODE-A"] }, RequestInvalidCode)
        };

        foreach (var testCase in cases)
        {
            var result = await runner.RunCandidatesAsync(testCase.Request);
            AssertFailure(result, testCase.Code);
        }

        Assert.Equal(0, projectionCalls);
        Assert.Equal(0, node.ExecuteCount);
    }

    [Fact]
    public async Task MissingNode_StopsBeforeProjectionAndPropagatesRegistryCode()
    {
        var projectionCalls = 0;
        var runner = new SingleCombinationGraphCandidateRunner<string, string>(
            new InMemoryDynamicGraphNodeRegistry<string, string>(),
            CreatePlan(),
            (request, plan) =>
            {
                projectionCalls++;
                return OperationResult<LogicalUnitRequest<string>>.Ok(CreateProjectedRequest(request, plan));
            });

        var result = await runner.RunCandidatesAsync(CreateRequest());

        AssertFailure(result, "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_NOT_FOUND");
        Assert.Equal(0, projectionCalls);
    }

    [Fact]
    public async Task ProjectorFailureNullExceptionAndInvalidValue_AreClosedWithStableCodes()
    {
        var cases = new (Projector Projector, string Code)[]
        {
            ((_, _) => OperationResult<LogicalUnitRequest<string>>.Fail("projector failed", "PROJECTOR_FAILED"), "PROJECTOR_FAILED"),
            ((_, _) => null!, ProjectionResultRequiredCode),
            ((_, _) => OperationResult<LogicalUnitRequest<string>>.Ok(null!), ProjectionResultRequiredCode),
            ((_, _) => throw new InvalidOperationException("projector exception"), ProjectionExceptionCode),
            ((request, plan) => OperationResult<LogicalUnitRequest<string>>.Ok(
                CreateProjectedRequest(request, plan) with { UnitId = "node-b" }), RequestInvalidCode),
            ((request, plan) => OperationResult<LogicalUnitRequest<string>>.Ok(
                CreateProjectedRequest(request, plan) with { Metadata = null! }), RequestInvalidCode),
            ((request, plan) => OperationResult<LogicalUnitRequest<string>>.Ok(
                CreateProjectedRequest(request, plan) with { Flags = new LogicalExecutionFlags() }), RequestInvalidCode)
        };

        foreach (var testCase in cases)
        {
            var node = new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable());
            var runner = CreateRunner(node, CreatePlan(), testCase.Projector.Invoke);

            var result = await runner.RunCandidatesAsync(CreateRequest());

            AssertFailure(result, testCase.Code);
            Assert.Equal(0, node.ExecuteCount);
        }
    }

    [Fact]
    public async Task NodeFailureNullTaskNullEnvelopeNullValueNullOutputAndExceptions_AreClosedWithStableCodes()
    {
        var cases = new (NodeExecution Execution, string Code)[]
        {
            ((_, _) => Task.FromResult(OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Fail(
                "node failed", "NODE_FAILED")), "NODE_FAILED"),
            ((_, _) => null!, NodeTaskRequiredCode),
            ((_, _) => Task.FromResult<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>>(null!), NodeResultRequiredCode),
            ((_, _) => Task.FromResult(OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Ok(null!)), NodeResultRequiredCode),
            ((_, _) => Task.FromResult(Success(null!)), NodeOutputRequiredCode),
            ((_, _) => throw new InvalidOperationException("sync node exception"), NodeExceptionCode),
            ((_, _) => Task.FromException<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>>(
                new InvalidOperationException("async node exception")), NodeExceptionCode)
        };

        foreach (var testCase in cases)
        {
            var node = new TestNode(testCase.Execution);
            var runner = CreateRunner(node);

            var result = await runner.RunCandidatesAsync(CreateRequest());

            AssertFailure(result, testCase.Code);
            Assert.Equal(1, node.ExecuteCount);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MethodAndEmbeddedCancellation_PreCancelledStopBeforeProjectionOrExecution(bool embedded)
    {
        using var methodSource = new CancellationTokenSource();
        using var embeddedSource = new CancellationTokenSource();
        if (embedded)
            embeddedSource.Cancel();
        else
            methodSource.Cancel();

        var node = new TestNode(Array.Empty<GraphNodeCandidate>().AsQueryable());
        var projectionCalls = 0;
        var runner = CreateRunner(
            node,
            CreatePlan(),
            (request, plan) =>
            {
                projectionCalls++;
                return OperationResult<LogicalUnitRequest<string>>.Ok(CreateProjectedRequest(request, plan));
            });
        var request = CreateRequest(cancellation: embeddedSource.Token);

        var result = await runner.RunCandidatesAsync(request, methodSource.Token);

        AssertFailure(result, "CANCELLED");
        Assert.Equal(0, projectionCalls);
        Assert.Equal(0, node.ExecuteCount);
    }

    [Theory(Timeout = 10000)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MethodAndEmbeddedCancellation_DuringExecutionReturnCancelled(bool embedded)
    {
        using var methodSource = new CancellationTokenSource();
        using var embeddedSource = new CancellationTokenSource();
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var node = new TestNode(async (_, cancellationToken) =>
        {
            entered.TrySetResult(true);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return Success(Array.Empty<GraphNodeCandidate>().AsQueryable());
        });
        var runner = CreateRunner(node);
        var request = CreateRequest(cancellation: embeddedSource.Token);

        var run = runner.RunCandidatesAsync(request, methodSource.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        if (embedded)
            embeddedSource.Cancel();
        else
            methodSource.Cancel();
        var result = await run.WaitAsync(TimeSpan.FromSeconds(2));

        AssertFailure(result, "CANCELLED");
        Assert.Equal(1, node.ExecuteCount);
        Assert.True(node.LastCancellation.CanBeCanceled);
    }

    [Fact(Timeout = 10000)]
    public async Task Timeout_DuringExecutionReturnsTimeoutRatherThanNodeException()
    {
        var node = new TestNode(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return Success(Array.Empty<GraphNodeCandidate>().AsQueryable());
        });
        var runner = CreateRunner(node);
        var request = CreateRequest(timeout: TimeSpan.FromMilliseconds(40));

        var result = await runner.RunCandidatesAsync(request).WaitAsync(TimeSpan.FromSeconds(3));

        AssertFailure(result, "TIMEOUT");
        Assert.Equal(1, node.ExecuteCount);
    }

    [Fact]
    public async Task EmptyQuery_EnumeratesOnceAndReturnsExplicitEmptyBatch()
    {
        var source = new DelayedEnumerable<GraphNodeCandidate>([], TimeSpan.FromMilliseconds(80));
        var node = new TestNode(source.AsQueryable());
        var runner = CreateRunner(node);

        var result = await runner.RunCandidatesAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, source.EnumerationCount);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Empty(batch.Candidates);
        Assert.Equal(1, batch.RequestedCombinationCount);
        Assert.Equal(0, batch.ActualCombinationCount);
        Assert.Equal("EMPTY_CANDIDATES", batch.Status);
        Assert.Empty(batch.ErrorCodes);
        Assert.Null(batch.CandidateStream);
        Assert.True(batch.ElapsedMs >= 50, $"Observed only {batch.ElapsedMs} ms.");
    }

    [Fact]
    public async Task QueryEnumerationException_IsEnumeratedOnceAndFailsClosed()
    {
        var source = new ThrowingEnumerable<GraphNodeCandidate>();
        var node = new TestNode(source.AsQueryable());
        var runner = CreateRunner(node);

        var result = await runner.RunCandidatesAsync(CreateRequest());

        AssertFailure(result, MaterializationExceptionCode);
        Assert.Equal(1, source.EnumerationCount);
        Assert.Equal(1, node.ExecuteCount);
    }

    [Fact]
    public async Task InvalidCandidates_AreRejectedWithStableIdentityAndMaterializationCodes()
    {
        var valid = CreateCandidate("candidate-a");
        var cases = new (GraphNodeCandidate[] Candidates, string Code)[]
        {
            ([null!], MaterializationExceptionCode),
            ([valid with { CandidateId = " " }], MaterializationExceptionCode),
            ([valid, CreateCandidate("candidate-a")], DuplicateCandidateCode),
            ([valid with { UnitId = "NODE-A" }], CandidateNodeMismatchCode),
            ([valid with { ErrorCodes = null! }], MaterializationExceptionCode),
            ([valid with { OutputRefs = null! }], MaterializationExceptionCode)
        };

        foreach (var testCase in cases)
        {
            var node = new TestNode(testCase.Candidates.AsQueryable());
            var runner = CreateRunner(node);

            var result = await runner.RunCandidatesAsync(CreateRequest());

            AssertFailure(result, testCase.Code);
            Assert.Equal(1, node.ExecuteCount);
        }
    }

    [Fact]
    public async Task CandidateErrors_AggregateInMaterializationOrderAndMarkPartialFailure()
    {
        var candidates = new[]
        {
            CreateCandidate("candidate-a", ["A-1", "A-2"]),
            CreateCandidate("candidate-b", ["B-1", "A-1"])
        };
        var node = new TestNode((_, _) => Task.FromResult(Success(candidates.AsQueryable(), 0.61)));
        var runner = CreateRunner(node);

        var result = await runner.RunCandidatesAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var batch = Assert.IsType<AsyncCandidateBatch>(result.Value);
        Assert.Equal("PARTIAL_FAILURE", batch.Status);
        Assert.Equal(["A-1", "A-2", "B-1", "A-1"], batch.ErrorCodes);
        var output = Assert.Single(batch.Candidates);
        Assert.Equal(["A-1", "A-2", "B-1", "A-1"], output.ErrorCodes);
        Assert.Equal(["A-1", "A-2"], output.OutputRefs[0].ErrorCodes);
        Assert.Equal(["B-1", "A-1"], output.OutputRefs[1].ErrorCodes);
        Assert.Equal(0.61, output.Confidence);
    }

    private static SingleCombinationGraphCandidateRunner<string, string> CreateRunner(
        TestNode node,
        CombinationPlan? plan = null,
        Func<GraphCandidateRunRequest, CombinationPlan, OperationResult<LogicalUnitRequest<string>>>? projector = null)
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var registration = registry.Register(node);
        if (!registration.IsSuccess)
            throw new InvalidOperationException($"Test node registration failed: {registration.ErrorCode} {registration.ErrorMessage}");

        return new SingleCombinationGraphCandidateRunner<string, string>(
            registry,
            plan ?? CreatePlan(),
            projector ?? ProjectRequest);
    }

    private static OperationResult<LogicalUnitRequest<string>> ProjectRequest(
        GraphCandidateRunRequest request,
        CombinationPlan plan)
        => OperationResult<LogicalUnitRequest<string>>.Ok(CreateProjectedRequest(request, plan));

    private static LogicalUnitRequest<string> CreateProjectedRequest(
        GraphCandidateRunRequest request,
        CombinationPlan plan)
        => new(
            RunId: request.TraceRef,
            UnitId: plan.NodeSetIds[0],
            UnitRole: "baseline_candidate_generator",
            DomainTag: "domain-a",
            Input: request.Query,
            Flags: plan.Flags,
            Metadata: new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["metadata-key"] = "metadata-value"
            });

    private static CombinationPlan CreatePlan()
        => new(
            Index: 0,
            CombinationId: "combination-a",
            NodeSetIds: ["node-a"],
            EdgeOrderKey: "edge-order-a",
            Flags: new LogicalExecutionFlags
            {
                UseDeterministicFixtureBaseline = true,
                UseE35DependencyDetection = true
            });

    private static GraphCandidateRunRequest CreateRequest(
        CancellationToken cancellation = default,
        TimeSpan? timeout = null)
        => new(
            ScenarioId: "scenario-a",
            Query: "query-a",
            InputGraphRefs: ["graph-a"],
            InputUnitRefs: ["node-a"],
            Budget: new AsyncCombinationBudget(
                MaxCombinations: 1,
                Timeout: timeout,
                MaxParallelBranches: 1,
                MaxParallelCombinationRuns: 1),
            CombinationCount: 1,
            Cancellation: cancellation,
            LegalScope: "legal-scope-a",
            ProvenanceRefs: ["provenance-a"],
            TraceRef: "trace-a");

    private static DynamicGraphNodeDescriptor CreateDescriptor()
        => new(
            NodeId: "node-a",
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
        string[]? errorCodes = null)
        => new(
            CandidateId: candidateId,
            UnitId: "node-a",
            UnitIds: ["node-a"],
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

    private static OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>> Success(
        IQueryable<GraphNodeCandidate> output,
        double confidence = 0.5)
        => OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Ok(
            new LogicalUnitResult<IQueryable<GraphNodeCandidate>>(
                UnitId: "node-a",
                UnitRole: "baseline_candidate_generator",
                DomainTag: "domain-a",
                Output: output,
                Confidence: confidence,
                PredicateIds: [],
                DomainTags: ["domain-a"],
                EvidenceIds: [],
                Warnings: []));

    private static void AssertFailure<T>(OperationResult<T> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private delegate OperationResult<LogicalUnitRequest<string>> Projector(
        GraphCandidateRunRequest request,
        CombinationPlan plan);

    private delegate Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>> NodeExecution(
        LogicalUnitRequest<string> request,
        CancellationToken cancellationToken);

    private sealed class TestNode : IDynamicGraphNode<string, string>
    {
        private readonly NodeExecution _execution;

        public TestNode(IQueryable<GraphNodeCandidate> output)
            : this((_, _) => Task.FromResult(Success(output)))
        {
        }

        public TestNode(NodeExecution execution)
        {
            _execution = execution;
        }

        public string NodeId => "node-a";

        public DynamicGraphNodeDescriptor Descriptor { get; } = CreateDescriptor();

        public int ExecuteCount { get; private set; }

        public LogicalUnitRequest<string>? LastRequest { get; private set; }

        public CancellationToken LastCancellation { get; private set; }

        public Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            ExecuteCount++;
            LastRequest = request;
            LastCancellation = cancellationToken;
            return _execution(request, cancellationToken);
        }
    }

    private sealed class CountingEnumerable<T> : IEnumerable<T>
    {
        private readonly IReadOnlyList<T> _items;

        public CountingEnumerable(IReadOnlyList<T> items)
        {
            _items = items;
        }

        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed class DelayedEnumerable<T> : IEnumerable<T>
    {
        private readonly IReadOnlyList<T> _items;
        private readonly TimeSpan _delay;

        public DelayedEnumerable(IReadOnlyList<T> items, TimeSpan delay)
        {
            _items = items;
            _delay = delay;
        }

        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            Thread.Sleep(_delay);
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed class ThrowingEnumerable<T> : IEnumerable<T>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            throw new InvalidOperationException("enumeration failed");
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
