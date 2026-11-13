// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;
using SemanticCompilerAdapter;

public sealed class DynamicGraphCandidateInvocationOrchestratorTests
{
    private const string SourceNodeId = "4.1";
    private const string TargetNodeId = "4.2";
    private const string AuthorityUnitId = "candidate-invocation-edge-authority";
    private const string WorkPrompt =
        "Implement backend service services code and frontend client UI integration.";

    private static readonly string[] ExactLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    [Fact]
    public async Task RunAsync_UsesOneCallerOwnedTopologyOutputPairAndExecutesItsExactTargetAfterRuntimeAdvances()
    {
        var authority = new RecordingEdgeAuthorityUnit(TargetNodeId);
        var runtime = CreateRuntime(authority);
        var runner = new TestCandidateRunner((request, _) =>
        {
            var advanced = runtime.ApplyRegistryProjection(
                expectedRevision: 1,
                CreateProjection(
                    signalId: "candidate-invocation-revision-two",
                    sourceNodeId: TargetNodeId,
                    targetNodeId: "4.3",
                    transitionId: "transition-sub-phase-4.2-to-4.3"),
                ValidTopologyBudget());
            Assert.True(advanced.IsSuccess, $"{advanced.ErrorCode}: {advanced.ErrorMessage}");
            Assert.Equal(2, advanced.Value!.CurrentRevision);
            return Task.FromResult<OperationResult<AsyncCandidateBatch>?>(
                OperationResult<AsyncCandidateBatch>.Ok(CreateCleanBatch(request, SourceNodeId)));
        });
        var invocation = AssertSuccess(
            DynamicGraphCandidateInvocationOrchestrator.Create(runtime, runner, SourceNodeId));
        var request = CreateCandidateRequest();

        var result = await invocation.RunAsync(WorkPrompt, request);

        var answer = AssertSuccess(result);
        Assert.Equal("PASS", answer.OverallVerdict);
        Assert.Contains(
            $"[dynamic-graph-projected-edge-traversal] L4 consumed the learned-edge handoff after 1 target logical flow(s) completed and returned results.{Environment.NewLine}" +
            "[universal-nine-node-development-4.2] Universal nine-node development flow completed for sub-phase 4.2",
            answer.FinalHumanResult,
            StringComparison.Ordinal);
        Assert.Equal(1, runner.CallCount);
        Assert.NotSame(request, runner.ReceivedRequest);
        Assert.NotSame(request.InputGraphRefs, runner.ReceivedRequest!.InputGraphRefs);
        Assert.NotSame(request.InputUnitRefs, runner.ReceivedRequest.InputUnitRefs);
        Assert.NotSame(request.ProvenanceRefs, runner.ReceivedRequest.ProvenanceRefs);
        Assert.NotSame(request.Budget, runner.ReceivedRequest.Budget);
        Assert.Equal(request.InputGraphRefs, runner.ReceivedRequest.InputGraphRefs);
        Assert.Equal(request.InputUnitRefs, runner.ReceivedRequest.InputUnitRefs);
        Assert.Equal(request.ProvenanceRefs, runner.ReceivedRequest.ProvenanceRefs);
        Assert.Equal(2, runner.ReceivedRequest.Budget.MaxCombinations);
        Assert.Equal(TimeSpan.FromMinutes(1), runner.ReceivedRequest.Budget.Timeout);
        Assert.Equal(1, runner.ReceivedRequest.Budget.MaxParallelBranches);
        Assert.Equal(1, runner.ReceivedRequest.Budget.MaxParallelCombinationRuns);
        Assert.Equal(WorkPrompt, runner.ReceivedRequest!.Query);
        Assert.Equal(AdaptiveLogicalOrchestrator.ScenarioId, runner.ReceivedRequest.ScenarioId);
        Assert.Equal(CancellationToken.None, runner.ReceivedRequest.Cancellation);
        Assert.False(runner.ReceivedRequest.Cancellation.CanBeCanceled);
        var canonicalAuthorityInput = AssertAuthorityProbeSequence(authority);
        Assert.Equal("edge-4.1-to-4.2", canonicalAuthorityInput.EdgeId);
        Assert.Equal(
            [TargetNodeId],
            canonicalAuthorityInput.Candidates.Select(candidate => candidate.TargetSubPhaseId));

        var current = AssertSuccess(runtime.TopologySource.ReadCurrent());
        Assert.Equal(2, current.Revision);
        Assert.DoesNotContain(current.Topology.Nodes, node => node.NodeId == SourceNodeId);
        Assert.Equal(["4.2", "4.3"], current.Topology.Order.Select(entry => entry.NodeId));
    }

    [Theory]
    [InlineData("null-result", "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_RUNNER_RESULT_REQUIRED")]
    [InlineData("batch-request-mismatch", "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_BATCH_REQUEST_MISMATCH")]
    [InlineData("candidate-source-mismatch", "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_OUTPUT_SOURCE_NODE_MISMATCH")]
    [InlineData("null-candidate", "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_OUTPUT_INVALID")]
    [InlineData("negative-batch-timing", "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_BATCH_NOT_CLEAN")]
    [InlineData("negative-candidate-timing", "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_BATCH_NOT_CLEAN")]
    public async Task RunAsync_InvalidRunnerBoundaryFailsBeforeProjectedEdgeOrTargetExecution(
        string mode,
        string expectedCode)
    {
        var authority = new RecordingEdgeAuthorityUnit(TargetNodeId);
        var runtime = CreateRuntime(authority);
        var runner = new TestCandidateRunner((request, _) =>
        {
            OperationResult<AsyncCandidateBatch>? result = mode switch
            {
                "null-result" => null,
                "batch-request-mismatch" => OperationResult<AsyncCandidateBatch>.Ok(
                    CreateCleanBatch(request with { Query = "different-query" }, SourceNodeId)),
                "candidate-source-mismatch" => OperationResult<AsyncCandidateBatch>.Ok(
                    CreateCleanBatch(request, TargetNodeId)),
                "null-candidate" => OperationResult<AsyncCandidateBatch>.Ok(
                    CreateCleanBatch(request, SourceNodeId, nullFirstCandidate: true)),
                "negative-batch-timing" => OperationResult<AsyncCandidateBatch>.Ok(
                    CreateCleanBatch(request, SourceNodeId, batchElapsedMs: -1)),
                "negative-candidate-timing" => OperationResult<AsyncCandidateBatch>.Ok(
                    CreateCleanBatch(request, SourceNodeId, firstCandidateElapsedMs: -1)),
                _ => throw new InvalidOperationException("Unknown test mode.")
            };
            return Task.FromResult(result);
        });
        var invocation = AssertSuccess(
            DynamicGraphCandidateInvocationOrchestrator.Create(runtime, runner, SourceNodeId));

        var result = await invocation.RunAsync(WorkPrompt, CreateCandidateRequest());

        AssertFailure(result, expectedCode);
        Assert.Equal(1, runner.CallCount);
        Assert.Equal(0, authority.CallCount);
        Assert.Equal(1, AssertSuccess(runtime.TopologySource.ReadCurrent()).Revision);
    }

    [Theory]
    [InlineData(0, 0, 999999)]
    [InlineData(999999, 999999, 0)]
    public async Task RunAsync_TimingIsDiagnosticAndDoesNotChangeAuthorityTarget(
        long batchElapsedMs,
        long firstCandidateElapsedMs,
        long secondCandidateElapsedMs)
    {
        var authority = new RecordingEdgeAuthorityUnit(TargetNodeId);
        var runtime = CreateRuntime(authority);
        var runner = new TestCandidateRunner((request, _) =>
            Task.FromResult<OperationResult<AsyncCandidateBatch>?>(
                OperationResult<AsyncCandidateBatch>.Ok(CreateCleanBatch(
                    request,
                    SourceNodeId,
                    batchElapsedMs: batchElapsedMs,
                    firstCandidateElapsedMs: firstCandidateElapsedMs,
                    secondCandidateElapsedMs: secondCandidateElapsedMs))));
        var invocation = AssertSuccess(
            DynamicGraphCandidateInvocationOrchestrator.Create(runtime, runner, SourceNodeId));

        var result = await invocation.RunAsync(WorkPrompt, CreateCandidateRequest());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var canonicalAuthorityInput = AssertAuthorityProbeSequence(authority);
        Assert.Equal(
            [TargetNodeId],
            canonicalAuthorityInput.Candidates.Select(candidate => candidate.TargetSubPhaseId));
    }

    [Fact]
    public async Task RunAsync_RunnerWrappedFatalExceptionEscapesWithoutGraphExecution()
    {
        var authority = new RecordingEdgeAuthorityUnit(TargetNodeId);
        var runtime = CreateRuntime(authority);
        var fatal = new OutOfMemoryException("fatal-candidate-runner");
        var wrapper = new TypeInitializationException("candidate-runner", fatal);
        var runner = new TestCandidateRunner((_, _) =>
            Task.FromException<OperationResult<AsyncCandidateBatch>?>(wrapper));
        var invocation = AssertSuccess(
            DynamicGraphCandidateInvocationOrchestrator.Create(runtime, runner, SourceNodeId));

        var observed = await Assert.ThrowsAsync<TypeInitializationException>(() =>
            invocation.RunAsync(WorkPrompt, CreateCandidateRequest()));

        Assert.Same(wrapper, observed);
        Assert.Same(fatal, observed.InnerException);
        Assert.Equal(1, runner.CallCount);
        Assert.Equal(0, authority.CallCount);
        Assert.Equal(1, AssertSuccess(runtime.TopologySource.ReadCurrent()).Revision);
    }

    [Fact]
    public async Task RunAsync_RunnerAggregateFatalExceptionEscapesWhenFatalIsNotFirstChild()
    {
        var authority = new RecordingEdgeAuthorityUnit(TargetNodeId);
        var runtime = CreateRuntime(authority);
        var fatal = new OutOfMemoryException("fatal-candidate-runner-child");
        var aggregate = new AggregateException(
            new InvalidOperationException("ordinary-candidate-runner-child"),
            fatal);
        var runner = new TestCandidateRunner((_, _) =>
            Task.FromException<OperationResult<AsyncCandidateBatch>?>(aggregate));
        var invocation = AssertSuccess(
            DynamicGraphCandidateInvocationOrchestrator.Create(runtime, runner, SourceNodeId));

        var observed = await Assert.ThrowsAsync<AggregateException>(() =>
            invocation.RunAsync(WorkPrompt, CreateCandidateRequest()));

        Assert.Same(aggregate, observed);
        Assert.Same(fatal, observed.InnerExceptions[1]);
        Assert.Equal(1, runner.CallCount);
        Assert.Equal(0, authority.CallCount);
        Assert.Equal(1, AssertSuccess(runtime.TopologySource.ReadCurrent()).Revision);
    }

    [Fact]
    public async Task RunAsync_RunnerWrappedNonFatalExceptionIsContainedWithoutGraphExecution()
    {
        var authority = new RecordingEdgeAuthorityUnit(TargetNodeId);
        var runtime = CreateRuntime(authority);
        var wrapper = new TypeInitializationException(
            "candidate-runner",
            new InvalidOperationException("ordinary-candidate-runner"));
        var runner = new TestCandidateRunner((_, _) =>
            Task.FromException<OperationResult<AsyncCandidateBatch>?>(wrapper));
        var invocation = AssertSuccess(
            DynamicGraphCandidateInvocationOrchestrator.Create(runtime, runner, SourceNodeId));

        var result = await invocation.RunAsync(WorkPrompt, CreateCandidateRequest());

        AssertFailure(result, "DYNAMIC_GRAPH_CANDIDATE_INVOCATION_RUNNER_EXCEPTION");
        Assert.Equal(1, runner.CallCount);
        Assert.Equal(0, authority.CallCount);
        Assert.Equal(1, AssertSuccess(runtime.TopologySource.ReadCurrent()).Revision);
    }

    private static DynamicGraphLogicalOrchestratorRuntime CreateRuntime(
        RecordingEdgeAuthorityUnit authority)
    {
        var descriptor = new LogicalUnitDescriptor(
            UnitId: AuthorityUnitId,
            UnitRole: "edge-authority",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            Version: "1.0.0",
            TrustState: LogicalUnitTrustState.Trusted,
            IsLearned: true,
            InputType: typeof(EdgeAuthorityInput),
            OutputType: typeof(EdgeAuthorityDecision),
            RegistrationOrder: 3);
        var preload = new ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>(
            descriptor,
            authority);
        return AssertSuccess(DynamicGraphLogicalOrchestratorRuntime.CreateDefaultWithEdgeAuthorityPreload(
            new NullSessionStorage(),
            CreateProjection(
                signalId: "candidate-invocation-revision-one",
                sourceNodeId: SourceNodeId,
                targetNodeId: TargetNodeId,
                transitionId: "transition-sub-phase-4.1-to-4.2"),
            ValidTopologyBudget(),
            [preload]));
    }

    private static DynamicGraphRegistryProjectionRequest CreateProjection(
        string signalId,
        string sourceNodeId,
        string targetNodeId,
        string transitionId)
        => new(
            TenantId: "tenant-candidate-invocation",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: "context-candidate-invocation",
            SignalId: signalId,
            SourceRef: $"logical-registry:{signalId}",
            TraceRef: $"trace:{signalId}",
            NodeBindings:
            [
                new DynamicGraphRegistryNodeBinding(sourceNodeId, CreateNode(sourceNodeId)),
                new DynamicGraphRegistryNodeBinding(targetNodeId, CreateNode(targetNodeId))
            ],
            EdgeBindings:
            [
                new DynamicGraphRegistryEdgeBinding(
                    EdgeId: $"edge-{sourceNodeId}-to-{targetNodeId}",
                    LogicalTransitionIds: [transitionId],
                    Kind: GraphEdgeKind.FanOut,
                    ParallelEligible: false,
                    AuthorityUnitId: AuthorityUnitId,
                    AuthorityUnitLevel: "L2",
                    DeterministicFinalAuthority: false,
                    BaselineUnitId: DeterministicRegistryEdgeCandidateProvider.Id,
                    BaselineRole: DeterministicRegistryEdgeCandidateProvider.BaselineRole,
                    ExcludedLeakageSignals: ExactLeakageSignals.ToArray(),
                    SourceProof: $"source-proof:{signalId}",
                    CutoverProof: $"cutover-proof:{signalId}",
                    AblationProofRef: $"ablation-proof:{signalId}")
            ]);

    private static TestNode CreateNode(string nodeId)
        => new(
            nodeId,
            new DynamicGraphNodeDescriptor(
                NodeId: nodeId,
                DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
                TInputTypeName: typeof(string).FullName!,
                TOutputTypeName: typeof(string).FullName!,
                IsTrained: false,
                DeterministicFinalAuthority: false,
                RuntimeSemanticQualityClaimed: false,
                AuthorityRole: "baseline_candidate_generator",
                CheckpointRef: null,
                ManifestRef: null));

    private static GraphCandidateRunRequest CreateCandidateRequest()
        => new(
            ScenarioId: AdaptiveLogicalOrchestrator.ScenarioId,
            Query: WorkPrompt,
            InputGraphRefs: ["graph:candidate-invocation"],
            InputUnitRefs: [SourceNodeId],
            Budget: new AsyncCombinationBudget(
                MaxCombinations: 2,
                Timeout: TimeSpan.FromMinutes(1),
                MaxParallelBranches: 1,
                MaxParallelCombinationRuns: 1),
            CombinationCount: 2,
            Cancellation: CancellationToken.None,
            LegalScope: "hosted-open-weight-permissive",
            ProvenanceRefs: ["provenance:candidate-invocation"],
            TraceRef: "trace:candidate-invocation");

    private static AsyncCandidateBatch CreateCleanBatch(
        GraphCandidateRunRequest request,
        string candidateSourceNodeId,
        bool nullFirstCandidate = false,
        long batchElapsedMs = 23,
        long firstCandidateElapsedMs = 11,
        long secondCandidateElapsedMs = 12)
    {
        var first = nullFirstCandidate
            ? null
            : CreateCandidate("candidate-a", candidateSourceNodeId);
        var second = CreateCandidate("candidate-b", candidateSourceNodeId);
        return new AsyncCandidateBatch(
            BatchId: "batch:candidate-invocation",
            Request: request,
            Candidates:
            [
                CreateWrapper("combination-a", first, firstCandidateElapsedMs),
                CreateWrapper("combination-b", second, secondCandidateElapsedMs)
            ],
            CandidateStream: null,
            RequestedCombinationCount: 2,
            ActualCombinationCount: 2,
            Status: "OK",
            ErrorCodes: [],
            ElapsedMs: batchElapsedMs);
    }

    private static GraphCandidateOutput CreateWrapper(
        string combinationId,
        GraphNodeCandidate? candidate,
        long elapsedMs)
        => new(
            CandidateId: $"wrapper-{combinationId}",
            OutputRefs: [candidate!],
            UnitId: SourceNodeId,
            UnitIds: [SourceNodeId],
            Confidence: 0.9,
            Score: 0.8,
            CombinationId: combinationId,
            ProvenanceRefs: [$"provenance-{combinationId}"],
            PathTraceRef: $"path-{combinationId}",
            TraceRef: $"trace-{combinationId}",
            ErrorCodes: [],
            ElapsedMs: elapsedMs);

    private static GraphNodeCandidate CreateCandidate(string candidateId, string sourceNodeId)
        => new(
            CandidateId: candidateId,
            UnitId: sourceNodeId,
            UnitIds: [sourceNodeId],
            LogicalUnitRef: $"logical-unit-{sourceNodeId}",
            OutputRefs: [$"output-{candidateId}"],
            OutputType: typeof(string).Name,
            Confidence: 0.9,
            Score: 0.8,
            ProvenanceRefs: [$"provenance-{candidateId}"],
            PathOrderRef: $"path-{candidateId}",
            LegalStatus: "LEGAL_ACCEPTED",
            Status: "OK",
            ErrorCodes: [],
            SourceIds: [$"source-{candidateId}"],
            TraceRef: $"trace-{candidateId}",
            TraceRefs: [$"trace-{candidateId}"],
            EvidenceIds: [$"evidence-{candidateId}"],
            B1TeacherJudgeRef: $"b1-{candidateId}",
            B7GraphOrderRef: $"b7-{candidateId}",
            B11AsyncBatchRef: $"b11-{candidateId}",
            B12WinnerSelectionRef: $"b12-{candidateId}",
            B6UnitDpoBuilderRef: $"b6-{candidateId}",
            SemanticQualityClaimed: false);

    private static TopologyBuildBudget ValidTopologyBudget()
        => new(MaxNodes: 8, MaxEdges: 8, MaxDepth: 8, MaxParallelBranches: 8);

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsAssignableFrom<T>(result.Value);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private static EdgeAuthorityInput AssertAuthorityProbeSequence(RecordingEdgeAuthorityUnit authority)
    {
        Assert.Equal(9, authority.CallCount);

        var authorityInputs = authority.Inputs;
        var secondInputRead = authority.Inputs;
        Assert.Equal(9, authorityInputs.Count);
        Assert.NotSame(authorityInputs, secondInputRead);
        Assert.NotSame(authorityInputs[0], secondInputRead[0]);

        var canonicalInput = authorityInputs[0];
        var orderProbeInputs = authorityInputs.Skip(1).Take(5).ToArray();
        var identityProbeInputs = authorityInputs.Skip(6).Take(3).ToArray();
        Assert.Equal(5, orderProbeInputs.Length);
        Assert.Equal(3, identityProbeInputs.Length);
        Assert.Equal(ExactLeakageSignals, canonicalInput.ExcludedLeakageSignals);

        Assert.All(orderProbeInputs, probeInput =>
        {
            Assert.NotSame(canonicalInput, probeInput);
            AssertAuthorityInputEquivalent(canonicalInput, probeInput);
        });
        Assert.All(identityProbeInputs, probeInput =>
        {
            Assert.NotSame(canonicalInput, probeInput);
            AssertAuthorityInputEquivalent(canonicalInput, probeInput);
        });

        var runIds = authority.RunIds;
        Assert.NotSame(runIds, authority.RunIds);
        Assert.Equal(9, runIds.Count);
        Assert.StartsWith("assistant-", runIds[0], StringComparison.Ordinal);
        Assert.All(runIds.Take(8), runId => Assert.Equal(runIds[0], runId));
        Assert.Equal("edge-authority-correlation-id-control", runIds[8]);
        return canonicalInput;
    }

    private static void AssertAuthorityInputEquivalent(EdgeAuthorityInput expected, EdgeAuthorityInput actual)
    {
        Assert.Equal(expected.EdgeId, actual.EdgeId);
        Assert.Equal(expected.SemanticTokens, actual.SemanticTokens);
        Assert.Equal(expected.PredicateIds, actual.PredicateIds);
        Assert.Equal(expected.ExcludedLeakageSignals, actual.ExcludedLeakageSignals);
        Assert.Equal(expected.Context.PriorDomains, actual.Context.PriorDomains);
        Assert.Equal(expected.Context.CompletedSubPhases, actual.Context.CompletedSubPhases);
        Assert.Equal(expected.Context.ActiveKnowledgeEdges, actual.Context.ActiveKnowledgeEdges);
        Assert.Equal(
            expected.Context.PriorConfidenceMap.OrderBy(pair => pair.Key),
            actual.Context.PriorConfidenceMap.OrderBy(pair => pair.Key));
        Assert.NotSame(expected.Candidates, actual.Candidates);
        Assert.Equal(expected.Candidates.Count, actual.Candidates.Count);

        for (var index = 0; index < expected.Candidates.Count; index++)
        {
            var expectedCandidate = expected.Candidates[index];
            var actualCandidate = actual.Candidates[index];
            Assert.NotSame(expectedCandidate, actualCandidate);
            Assert.Equal(expectedCandidate.TargetSubPhaseId, actualCandidate.TargetSubPhaseId);
            Assert.Equal(expectedCandidate.EvidencePredicates, actualCandidate.EvidencePredicates);
            Assert.Equal(expectedCandidate.BaselineScore, actualCandidate.BaselineScore);
            Assert.Equal(expectedCandidate.AuthorityRole, actualCandidate.AuthorityRole);
        }
    }

    private sealed record TestNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

    private sealed class TestCandidateRunner(
        Func<GraphCandidateRunRequest, CancellationToken, Task<OperationResult<AsyncCandidateBatch>?>> handler)
        : IGraphCandidateRunner
    {
        public int CallCount { get; private set; }

        public GraphCandidateRunRequest? ReceivedRequest { get; private set; }

        public async Task<OperationResult<AsyncCandidateBatch>> RunCandidatesAsync(
            GraphCandidateRunRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            ReceivedRequest = request;
            return (await handler(request, cancellationToken))!;
        }
    }

    private sealed class RecordingEdgeAuthorityUnit(string targetNodeId)
        : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
    {
        private readonly List<EdgeAuthorityInput> _inputs = [];
        private readonly List<string> _runIds = [];

        public int CallCount { get; private set; }

        public IReadOnlyList<EdgeAuthorityInput> Inputs => _inputs.Select(CopyInput).ToArray();

        public IReadOnlyList<string> RunIds => _runIds.ToArray();

        public Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>> ExecuteAsync(
            LogicalUnitRequest<EdgeAuthorityInput> request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            _runIds.Add(request.RunId);
            _inputs.Add(CopyInput(request.Input));
            var candidate = Assert.Single(request.Input.Candidates, value => value.TargetSubPhaseId == targetNodeId);
            var decision = new EdgeAuthorityDecision(
                OrderedTargets: [(targetNodeId, 0.95)],
                FanOutCount: 1,
                KnowledgeEdge: false,
                AuthoritySource: "trained_edge_unit",
                AblationBaselineRank: request.Input.Candidates
                    .OrderByDescending(value => value.BaselineScore)
                    .ThenBy(value => value.TargetSubPhaseId, StringComparer.Ordinal)
                    .Select(value => value.TargetSubPhaseId)
                    .ToArray());
            var result = new LogicalUnitResult<EdgeAuthorityDecision>(
                UnitId: AuthorityUnitId,
                UnitRole: "edge-authority",
                DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
                Output: decision,
                Confidence: 0.95,
                PredicateIds: candidate.EvidencePredicates.ToArray(),
                DomainTags: [DevelopmentLifecycleTopologySeeder.DomainId],
                EvidenceIds: ["evidence:candidate-invocation-authority"],
                Warnings: []);
            return Task.FromResult(
                OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Ok(result));
        }

        private static EdgeAuthorityInput CopyInput(EdgeAuthorityInput source)
            => new(
                source.EdgeId,
                source.SemanticTokens.ToArray(),
                source.PredicateIds.ToArray(),
                new ContextSignals(
                    source.Context.PriorDomains.ToArray(),
                    source.Context.CompletedSubPhases.ToArray(),
                    source.Context.ActiveKnowledgeEdges.ToArray(),
                    source.Context.PriorConfidenceMap.ToDictionary(pair => pair.Key, pair => pair.Value)),
                source.Candidates
                    .Select(candidate => candidate with
                    {
                        EvidencePredicates = candidate.EvidencePredicates.ToArray()
                    })
                    .ToArray(),
                source.ExcludedLeakageSignals.ToArray());

    }

    private sealed class NullSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
    }
}
