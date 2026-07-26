// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class LogicalUnitDynamicGraphNodeTests
{
    private const string NodeId = "node-alpha";
    private const string UnitRole = "candidate_role";
    private const string DomainTag = "domain-alpha";

    [Fact]
    public async Task ExecuteAsync_DelegatesSameRequestAndTokenOnce_AndPreservesOrderedCandidatesAndMetadata()
    {
        var transition = new PhaseTransitionSignal(
            SemanticCompiler29.Domain.PLAN,
            "continue with a planning branch",
            NodeId,
            "subphase-1",
            ["transition-evidence"]);
        var innerResult = CreateUnitResult() with
        {
            TransitionSignal = transition
        };
        var unit = new RecordingLogicalUnit((_, _) => Success(innerResult));
        var first = CreateCandidate("candidate-1");
        var second = CreateCandidate("candidate-2") with { Confidence = 0.77, Score = 0.5 };
        var projectionCalls = 0;
        LogicalUnitResult<string>? projectedResult = null;
        var node = CreateNode(unit, result =>
        {
            projectionCalls++;
            projectedResult = result;
            return OperationResult<IReadOnlyList<GraphNodeCandidate>>.Ok([first, second]);
        });
        var request = CreateRequest();
        using var source = new CancellationTokenSource();

        var executed = await node.ExecuteAsync(request, source.Token);

        Assert.True(executed.IsSuccess, executed.ErrorMessage);
        Assert.Equal(1, unit.CallCount);
        Assert.Same(request, unit.LastRequest);
        Assert.Equal(source.Token, unit.LastToken);
        Assert.Equal(1, projectionCalls);
        Assert.Same(innerResult, projectedResult);
        Assert.Equal(new[] { "candidate-1", "candidate-2" }, executed.Value!.Output.Select(candidate => candidate.CandidateId));
        Assert.Equal(innerResult.Confidence, executed.Value.Confidence);
        Assert.Equal(innerResult.PredicateIds, executed.Value.PredicateIds);
        Assert.Equal(innerResult.DomainTags, executed.Value.DomainTags);
        Assert.Equal(innerResult.EvidenceIds, executed.Value.EvidenceIds);
        Assert.Equal(innerResult.Warnings, executed.Value.Warnings);
        Assert.NotNull(executed.Value.TransitionSignal);
        Assert.Equal(transition.SuggestedDomain, executed.Value.TransitionSignal!.SuggestedDomain);
        Assert.Equal(transition.TransitionPrompt, executed.Value.TransitionSignal.TransitionPrompt);
        Assert.Equal(transition.SourceNodeId, executed.Value.TransitionSignal.SourceNodeId);
        Assert.Equal(transition.SourceSubPhaseId, executed.Value.TransitionSignal.SourceSubPhaseId);
        Assert.Equal(transition.EvidenceIds, executed.Value.TransitionSignal.EvidenceIds);
    }

    [Fact]
    public async Task ExecuteAsync_PreservesInnerFailureAndSkipsProjection()
    {
        var unit = new RecordingLogicalUnit((_, _) => Task.FromResult(
            OperationResult<LogicalUnitResult<string>>.Fail("inner failure message", "INNER_FAILURE_EXACT")));
        var projectionCalls = 0;
        var node = CreateNode(unit, _ =>
        {
            projectionCalls++;
            return CandidateSuccess(CreateCandidate("candidate-never"));
        });

        var executed = await node.ExecuteAsync(CreateRequest());

        Assert.False(executed.IsSuccess);
        Assert.Equal("INNER_FAILURE_EXACT", executed.ErrorCode);
        Assert.Equal("inner failure message", executed.ErrorMessage);
        Assert.Equal(1, unit.CallCount);
        Assert.Equal(0, projectionCalls);
    }

    [Fact]
    public async Task ExecuteAsync_DomainMismatchIsHonestSuccessfulNoDecisionWithoutDependencies()
    {
        var unit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        var projectionCalls = 0;
        var node = CreateNode(unit, _ =>
        {
            projectionCalls++;
            return CandidateSuccess(CreateCandidate("candidate-never"));
        });
        var request = CreateRequest(domainTag: "domain-foreign");

        var executed = await node.ExecuteAsync(request);

        Assert.True(executed.IsSuccess, executed.ErrorMessage);
        Assert.Equal(0, unit.CallCount);
        Assert.Equal(0, projectionCalls);
        Assert.Equal(request.UnitId, executed.Value!.UnitId);
        Assert.Equal(request.UnitRole, executed.Value.UnitRole);
        Assert.Equal(request.DomainTag, executed.Value.DomainTag);
        Assert.Equal(0.0, executed.Value.Confidence);
        Assert.Empty(executed.Value.Output);
        Assert.Empty(executed.Value.PredicateIds);
        Assert.Equal(new[] { request.DomainTag }, executed.Value.DomainTags);
        Assert.Empty(executed.Value.EvidenceIds);
        Assert.Equal(new[] { "DYNAMIC_GRAPH_DOMAIN_MISMATCH" }, executed.Value.Warnings);
        Assert.Null(executed.Value.TransitionSignal);
    }

    [Fact]
    public async Task ExecuteAsync_PreCancelledRequestReturnsStableFailureWithoutDependencies()
    {
        var unit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        var projectionCalls = 0;
        var node = CreateNode(unit, _ =>
        {
            projectionCalls++;
            return CandidateSuccess(CreateCandidate("candidate-never"));
        });
        using var source = new CancellationTokenSource();
        source.Cancel();

        var executed = await node.ExecuteAsync(CreateRequest(), source.Token);

        AssertFailure(executed, "CANCELLED");
        Assert.Equal(0, unit.CallCount);
        Assert.Equal(0, projectionCalls);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidConstructorDescriptorBeforeDependencies()
    {
        var unit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        var projectionCalls = 0;
        var deterministicAuthorityDescriptor = CreateDescriptor() with { DeterministicFinalAuthority = true };
        var deterministicNode = CreateNode(unit, _ =>
        {
            projectionCalls++;
            return CandidateSuccess(CreateCandidate("candidate-never"));
        }, deterministicAuthorityDescriptor);

        var deterministicFailure = await deterministicNode.ExecuteAsync(CreateRequest());

        AssertFailure(deterministicFailure, "DYNAMIC_GRAPH_NODE_DETERMINISTIC_AUTHORITY_FORBIDDEN");
        Assert.Equal(0, unit.CallCount);
        Assert.Equal(0, projectionCalls);

        var wrongTypeNode = CreateNode(
            unit,
            _ => CandidateSuccess(CreateCandidate("candidate-never")),
            CreateDescriptor() with { TOutputTypeName = typeof(int).Name });

        var typeFailure = await wrongTypeNode.ExecuteAsync(CreateRequest());

        AssertFailure(typeFailure, "DYNAMIC_GRAPH_NODE_OUTPUT_TYPE_MISMATCH");
        Assert.Equal(0, unit.CallCount);

        var blankIdentityNode = CreateNode(
            unit,
            _ => CandidateSuccess(CreateCandidate("candidate-never")),
            CreateDescriptor() with { NodeId = " " });

        var identityFailure = await blankIdentityNode.ExecuteAsync(CreateRequest());

        AssertFailure(identityFailure, "DYNAMIC_GRAPH_NODE_ID_REQUIRED");
        Assert.Equal(0, unit.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsNullConstructorDependenciesWithStableCodes()
    {
        var validUnit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        var validProjection = new Func<LogicalUnitResult<string>, OperationResult<IReadOnlyList<GraphNodeCandidate>>>(
            _ => CandidateSuccess(CreateCandidate("candidate")));

        var missingDescriptor = new LogicalUnitDynamicGraphNode<string, string>(null!, validUnit, validProjection);
        var missingUnit = new LogicalUnitDynamicGraphNode<string, string>(CreateDescriptor(), null!, validProjection);
        var missingProjection = new LogicalUnitDynamicGraphNode<string, string>(CreateDescriptor(), validUnit, null!);

        AssertFailure(await missingDescriptor.ExecuteAsync(CreateRequest()), "DYNAMIC_GRAPH_NODE_DESCRIPTOR_REQUIRED");
        AssertFailure(await missingUnit.ExecuteAsync(CreateRequest()), "DYNAMIC_GRAPH_NODE_UNIT_REQUIRED");
        AssertFailure(await missingProjection.ExecuteAsync(CreateRequest()), "DYNAMIC_GRAPH_NODE_PROJECTION_REQUIRED");
        Assert.Equal(0, validUnit.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsNullAndIdentityInvalidRequestsBeforeDependencies()
    {
        var unit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        var node = CreateNode(unit, _ => CandidateSuccess(CreateCandidate("candidate")));

        AssertFailure(await node.ExecuteAsync(null!), "DYNAMIC_GRAPH_NODE_REQUEST_REQUIRED");
        AssertFailure(await node.ExecuteAsync(CreateRequest(unitId: "another-node")), "DYNAMIC_GRAPH_NODE_REQUEST_UNIT_MISMATCH");
        AssertFailure(
            await node.ExecuteAsync(CreateRequest(input: null!)),
            "DYNAMIC_GRAPH_NODE_INPUT_REQUIRED");
        AssertFailure(
            await node.ExecuteAsync(CreateRequest() with { Flags = null! }),
            "DYNAMIC_GRAPH_NODE_FLAGS_REQUIRED");
        AssertFailure(
            await node.ExecuteAsync(CreateRequest() with { Metadata = null! }),
            "DYNAMIC_GRAPH_NODE_METADATA_REQUIRED");
        Assert.Equal(0, unit.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsResultIdentityDriftAndSkipsProjection()
    {
        var cases = new[]
        {
            (CreateUnitResult() with { UnitId = "wrong-unit" }, "DYNAMIC_GRAPH_NODE_RESULT_UNIT_MISMATCH"),
            (CreateUnitResult() with { UnitRole = "wrong-role" }, "DYNAMIC_GRAPH_NODE_RESULT_ROLE_MISMATCH"),
            (CreateUnitResult() with { DomainTag = "wrong-domain" }, "DYNAMIC_GRAPH_NODE_RESULT_DOMAIN_MISMATCH")
        };

        foreach (var (result, expectedCode) in cases)
        {
            var unit = new RecordingLogicalUnit((_, _) => Success(result));
            var projectionCalls = 0;
            var node = CreateNode(unit, _ =>
            {
                projectionCalls++;
                return CandidateSuccess(CreateCandidate("candidate-never"));
            });

            var executed = await node.ExecuteAsync(CreateRequest());

            AssertFailure(executed, expectedCode);
            Assert.Equal(1, unit.CallCount);
            Assert.Equal(0, projectionCalls);
        }
    }

    [Fact]
    public async Task ExecuteAsync_PreservesExplicitProjectionFailure()
    {
        var unit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        var node = CreateNode(
            unit,
            _ => OperationResult<IReadOnlyList<GraphNodeCandidate>>.Fail(
                "projection failure message",
                "PROJECTION_FAILURE_EXACT"));

        var executed = await node.ExecuteAsync(CreateRequest());

        AssertFailure(executed, "PROJECTION_FAILURE_EXACT");
        Assert.Equal("projection failure message", executed.ErrorMessage);
        Assert.Equal(1, unit.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsWrongCandidateOutputTypeDuplicateIdsAndInvalidConfidence()
    {
        var wrongType = await ExecuteWithCandidates(
            [CreateCandidate("candidate") with { OutputType = typeof(int).Name }]);
        AssertFailure(wrongType, "DYNAMIC_GRAPH_NODE_CANDIDATE_OUTPUT_TYPE_MISMATCH");

        var duplicateIds = await ExecuteWithCandidates(
            [CreateCandidate("duplicate"), CreateCandidate("duplicate") with { OutputRefs = ["output-2"] }]);
        AssertFailure(duplicateIds, "DYNAMIC_GRAPH_NODE_DUPLICATE_CANDIDATE_ID");

        var invalidConfidence = await ExecuteWithCandidates(
            [CreateCandidate("candidate") with { Confidence = double.NaN }]);
        AssertFailure(invalidConfidence, "DYNAMIC_GRAPH_NODE_CANDIDATE_CONFIDENCE_INVALID");
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUntrainedCandidateSemanticClaimAndMalformedRequiredCarrier()
    {
        var semanticClaim = await ExecuteWithCandidates(
            [CreateCandidate("candidate") with { SemanticQualityClaimed = true }]);
        AssertFailure(semanticClaim, "DYNAMIC_GRAPH_NODE_CANDIDATE_SEMANTIC_CLAIM_FORBIDDEN");

        var missingOutputRefs = await ExecuteWithCandidates(
            [CreateCandidate("candidate") with { OutputRefs = null! }]);
        AssertFailure(missingOutputRefs, "DYNAMIC_GRAPH_NODE_CANDIDATE_OUTPUT_REFS_INVALID");
    }

    [Fact]
    public async Task ExecuteAsync_RejectsNullSuccessEnvelopesListsAndCandidates()
    {
        var nullUnitEnvelope = new RecordingLogicalUnit((_, _) =>
            Task.FromResult<OperationResult<LogicalUnitResult<string>>>(null!));
        AssertFailure(
            await CreateNode(nullUnitEnvelope, _ => CandidateSuccess(CreateCandidate("candidate"))).ExecuteAsync(CreateRequest()),
            "DYNAMIC_GRAPH_NODE_UNIT_RESULT_REQUIRED");

        var nullUnitValue = new RecordingLogicalUnit((_, _) =>
            Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(null!)));
        AssertFailure(
            await CreateNode(nullUnitValue, _ => CandidateSuccess(CreateCandidate("candidate"))).ExecuteAsync(CreateRequest()),
            "DYNAMIC_GRAPH_NODE_UNIT_RESULT_REQUIRED");

        var unit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        AssertFailure(
            await CreateNode(unit, _ => null!).ExecuteAsync(CreateRequest()),
            "DYNAMIC_GRAPH_NODE_PROJECTION_RESULT_REQUIRED");
        AssertFailure(
            await CreateNode(
                unit,
                _ => OperationResult<IReadOnlyList<GraphNodeCandidate>>.Ok(null!)).ExecuteAsync(CreateRequest()),
            "DYNAMIC_GRAPH_NODE_CANDIDATE_LIST_REQUIRED");
        AssertFailure(
            await ExecuteWithCandidates([null!]),
            "DYNAMIC_GRAPH_NODE_CANDIDATE_REQUIRED");
    }

    [Fact]
    public async Task ExecuteAsync_ConvertsUnitAndProjectionExceptionsToStableFailures()
    {
        var throwingUnit = new RecordingLogicalUnit((_, _) => throw new InvalidOperationException("private unit detail"));
        var unitFailure = await CreateNode(
            throwingUnit,
            _ => CandidateSuccess(CreateCandidate("candidate-never"))).ExecuteAsync(CreateRequest());

        AssertFailure(unitFailure, "DYNAMIC_GRAPH_NODE_UNIT_EXCEPTION");
        Assert.DoesNotContain("private unit detail", unitFailure.ErrorMessage, StringComparison.Ordinal);

        var unit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        var projectionFailure = await CreateNode(
            unit,
            _ => throw new InvalidOperationException("private projection detail")).ExecuteAsync(CreateRequest());

        AssertFailure(projectionFailure, "DYNAMIC_GRAPH_NODE_PROJECTION_EXCEPTION");
        Assert.DoesNotContain("private projection detail", projectionFailure.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Adapter_PublicSurfaceContainsNoTrainingOrMutationMethods()
    {
        var publicMethods = typeof(LogicalUnitDynamicGraphNode<string, string>)
            .GetMethods(System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.Static
                        | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        Assert.Single(publicMethods);
        Assert.Equal(nameof(LogicalUnitDynamicGraphNode<string, string>.ExecuteAsync), publicMethods[0].Name);
        var forbiddenFragments = new[] { "Train", "Learn", "Evaluate", "Export", "Import", "Save", "Load", "Add", "Remove", "Update", "Delete", "Set" };
        Assert.DoesNotContain(
            publicMethods,
            method => forbiddenFragments.Any(fragment => method.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
    }

    private static async Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>> ExecuteWithCandidates(
        IReadOnlyList<GraphNodeCandidate> candidates)
    {
        var unit = new RecordingLogicalUnit((_, _) => Success(CreateUnitResult()));
        var node = CreateNode(
            unit,
            _ => OperationResult<IReadOnlyList<GraphNodeCandidate>>.Ok(candidates));
        return await node.ExecuteAsync(CreateRequest());
    }

    private static LogicalUnitDynamicGraphNode<string, string> CreateNode(
        ILogicalUnit<string, string> unit,
        Func<LogicalUnitResult<string>, OperationResult<IReadOnlyList<GraphNodeCandidate>>> projection,
        DynamicGraphNodeDescriptor? descriptor = null)
        => new(descriptor ?? CreateDescriptor(), unit, projection);

    private static DynamicGraphNodeDescriptor CreateDescriptor()
        => new(
            NodeId,
            DomainTag,
            typeof(string).Name,
            typeof(string).Name,
            IsTrained: false,
            DeterministicFinalAuthority: false,
            RuntimeSemanticQualityClaimed: false,
            AuthorityRole: "baseline_candidate_generator",
            CheckpointRef: null,
            ManifestRef: null);

    private static LogicalUnitRequest<string> CreateRequest(
        string unitId = NodeId,
        string domainTag = DomainTag,
        string? input = "input",
        LogicalExecutionFlags? flags = null,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new(
            "run-1",
            unitId,
            UnitRole,
            domainTag,
            input!,
            flags ?? new LogicalExecutionFlags(),
            metadata ?? new Dictionary<string, object> { ["request"] = "metadata" });

    private static LogicalUnitResult<string> CreateUnitResult()
        => new(
            NodeId,
            UnitRole,
            DomainTag,
            "typed-output",
            0.82,
            ["predicate-1"],
            [DomainTag, "supporting-domain"],
            ["unit-evidence"],
            ["unit-warning"]);

    private static GraphNodeCandidate CreateCandidate(string candidateId)
        => new(
            candidateId,
            NodeId,
            [NodeId],
            "logical-unit-ref",
            [$"output-ref-{candidateId}"],
            typeof(string).Name,
            0.81,
            0.4,
            [$"provenance-{candidateId}"],
            $"path-order-{candidateId}",
            "LEGAL_ACCEPTED",
            "PROJECTED",
            [],
            [$"source-{candidateId}"],
            $"trace-{candidateId}",
            [$"trace-{candidateId}"],
            [$"evidence-{candidateId}"],
            $"b1-{candidateId}",
            $"b7-{candidateId}",
            $"b11-{candidateId}",
            $"b12-{candidateId}",
            $"b6-{candidateId}",
            SemanticQualityClaimed: false);

    private static Task<OperationResult<LogicalUnitResult<string>>> Success(LogicalUnitResult<string> result)
        => Task.FromResult(OperationResult<LogicalUnitResult<string>>.Ok(result));

    private static OperationResult<IReadOnlyList<GraphNodeCandidate>> CandidateSuccess(GraphNodeCandidate candidate)
        => OperationResult<IReadOnlyList<GraphNodeCandidate>>.Ok([candidate]);

    private static void AssertFailure<T>(OperationResult<T> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private sealed class RecordingLogicalUnit : ILogicalUnit<string, string>
    {
        private readonly Func<
            LogicalUnitRequest<string>,
            CancellationToken,
            Task<OperationResult<LogicalUnitResult<string>>>> _handler;

        public RecordingLogicalUnit(Func<
            LogicalUnitRequest<string>,
            CancellationToken,
            Task<OperationResult<LogicalUnitResult<string>>>> handler)
        {
            _handler = handler;
        }

        public int CallCount { get; private set; }

        public LogicalUnitRequest<string>? LastRequest { get; private set; }

        public CancellationToken LastToken { get; private set; }

        public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastRequest = request;
            LastToken = cancellationToken;
            return _handler(request, cancellationToken);
        }
    }
}
