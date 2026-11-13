// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using Teaching20RowsCurrentRunner;

public sealed class Teaching20RowsCurrentRunnerContractTests
{
    [Theory]
    [InlineData(SubPhaseRoutingDecision.BaselineAuthorityRole)]
    [InlineData(SubPhaseRoutingDecision.TrainedAuthorityRole)]
    public void BuildKnowledgeEdgeBranches_PreservesCanonicalAuthorityInDeterministicMetrics(
        string authorityRole)
    {
        var decision = CreateDecision(authorityRole);

        var mapped = TeachingKnowledgeEdgeBranchMapper.Build(
            "teaching-row-1",
            "Implement the service.",
            "source-row.jsonl",
            "conversation-1",
            "message-1",
            [decision]);

        Assert.True(mapped.IsSuccess, mapped.ErrorMessage);
        var branch = Assert.Single(mapped.Value!);
        Assert.Equal(authorityRole, branch.Metrics[SubPhaseRoutingDecision.AuthorityRoleMetadataKey]);
        Assert.Equal(
            [
                "missing_sub_phase",
                "routed_phase_id",
                "routed_sub_phase_id",
                SubPhaseRoutingDecision.AuthorityRoleMetadataKey,
                "sub_phase_id",
                "phase_id",
                "routing_confidence",
                "source_row_id",
                "source_conversation_uuid",
                "source_message_uuid",
                "knowledge_edge"
            ],
            branch.Metrics.Keys);
    }

    [Fact]
    public void BuildKnowledgeEdgeBranches_NullDecisionCollectionFailsClosedWithoutBranchInput()
    {
        var mapped = TeachingKnowledgeEdgeBranchMapper.Build(
            "teaching-row-null-decisions",
            "Implement the service.",
            "source-row.jsonl",
            "conversation-1",
            "message-1",
            null);

        Assert.False(mapped.IsSuccess);
        Assert.Null(mapped.Value);
        Assert.Equal("TEACHING_BRANCH_ROUTING_DECISIONS_REQUIRED", mapped.ErrorCode);
        Assert.Equal("Teaching branch routing decisions are required.", mapped.ErrorMessage);
    }

    [Fact]
    public void BuildKnowledgeEdgeBranches_NullAuthorityFailsClosedWithoutDefaultOrBranchInput()
    {
        var decision = CreateDecision(SubPhaseRoutingDecision.BaselineAuthorityRole) with
        {
            AuthorityRole = null!
        };
        Assert.Null(decision.AuthorityRole);

        var mapped = TeachingKnowledgeEdgeBranchMapper.Build(
            "teaching-row-null-authority",
            "Implement the service.",
            "source-row.jsonl",
            "conversation-1",
            "message-1",
            [decision]);

        Assert.False(mapped.IsSuccess);
        Assert.Null(mapped.Value);
        Assert.Equal("TEACHING_BRANCH_AUTHORITY_ROLE_INVALID", mapped.ErrorCode);
        Assert.Equal("Teaching branch routing authority role is invalid.", mapped.ErrorMessage);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("unknown_authority_role")]
    public void BuildKnowledgeEdgeBranches_RejectsNonCanonicalAuthority(string authorityRole)
    {
        var mapped = TeachingKnowledgeEdgeBranchMapper.Build(
            "teaching-row-invalid",
            "Implement the service.",
            "source-row.jsonl",
            "conversation-1",
            "message-1",
            [CreateDecision(authorityRole)]);

        Assert.False(mapped.IsSuccess);
        Assert.Null(mapped.Value);
        Assert.Equal("TEACHING_BRANCH_AUTHORITY_ROLE_INVALID", mapped.ErrorCode);
        Assert.Equal("Teaching branch routing authority role is invalid.", mapped.ErrorMessage);
    }

    [Fact]
    public void ProgramBoundary_SuccessEnvelopeWithNullDecisionCollectionFailsBeforeTrainerInputConstruction()
    {
        var route = AgentInfrastructure.OperationResult<SubPhaseRoutingDecision[]>.Ok(null!);

        var prepared = PrepareProgramBoundary(route);

        Assert.False(prepared.IsSuccess);
        Assert.Null(prepared.Value);
        Assert.Equal("TEACHING_BRANCH_ROUTING_DECISIONS_REQUIRED", prepared.ErrorCode);
        Assert.Equal("Teaching branch routing decisions are required.", prepared.ErrorMessage);
    }

    [Fact]
    public void ProgramBoundary_SuccessEnvelopeWithNullDecisionElementFailsBeforeTrainerInputConstruction()
    {
        var route = AgentInfrastructure.OperationResult<SubPhaseRoutingDecision[]>.Ok([null!]);

        var prepared = PrepareProgramBoundary(route);

        Assert.False(prepared.IsSuccess);
        Assert.Null(prepared.Value);
        Assert.Equal("TEACHING_BRANCH_ROUTING_DECISION_INVALID", prepared.ErrorCode);
        Assert.Equal("Teaching branch routing decision is invalid.", prepared.ErrorMessage);
    }

    [Fact]
    public void ProgramBoundary_NonFiniteDecisionFailsBeforeTrainerInputConstruction()
    {
        var route = AgentInfrastructure.OperationResult<SubPhaseRoutingDecision[]>.Ok(
            [CreateDecision(SubPhaseRoutingDecision.BaselineAuthorityRole, double.NaN)]);

        var prepared = PrepareProgramBoundary(route);

        Assert.False(prepared.IsSuccess);
        Assert.Null(prepared.Value);
        Assert.Equal("TEACHING_BRANCH_ROUTING_DECISION_INVALID", prepared.ErrorCode);
        Assert.Equal("Teaching branch routing decision is invalid.", prepared.ErrorMessage);
    }

    [Theory]
    [InlineData("confidence", -0.01d)]
    [InlineData("confidence", 1.01d)]
    [InlineData("nearest_wrong", -0.01d)]
    [InlineData("nearest_wrong", 1.01d)]
    public void ProgramBoundary_OutOfRangeScoreFailsBeforeBranchesOrTrainerInputConstruction(
        string scoreName,
        double scoreValue)
    {
        var confidence = StringComparer.Ordinal.Equals(scoreName, "confidence") ? scoreValue : 0.91d;
        var nearestWrongScore = StringComparer.Ordinal.Equals(scoreName, "nearest_wrong") ? scoreValue : 0d;
        var route = AgentInfrastructure.OperationResult<SubPhaseRoutingDecision[]>.Ok(
            [CreateDecision(
                SubPhaseRoutingDecision.BaselineAuthorityRole,
                confidence,
                nearestWrongScore)]);

        var prepared = PrepareProgramBoundary(route);

        Assert.False(prepared.IsSuccess);
        Assert.Null(prepared.Value);
        Assert.Equal("TEACHING_BRANCH_ROUTING_DECISION_INVALID", prepared.ErrorCode);
        Assert.Equal("Teaching branch routing decision is invalid.", prepared.ErrorMessage);
    }

    [Fact]
    public void ProgramBoundary_SnapshotsDecisionCollectionAndNestedEvidenceBeforeOrdering()
    {
        var evidence = new[] { "IMPLEMENT" };
        var sourceDecision = CreateDecision(
            SubPhaseRoutingDecision.BaselineAuthorityRole,
            evidencePredicates: evidence);
        var sourceDecisions = new[] { sourceDecision };
        var route = AgentInfrastructure.OperationResult<SubPhaseRoutingDecision[]>.Ok(sourceDecisions);

        var prepared = PrepareProgramBoundary(route);
        sourceDecisions[0] = CreateDecision(SubPhaseRoutingDecision.TrainedAuthorityRole);
        evidence[0] = "MUTATED_AFTER_PREPARATION";

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var input = prepared.Value!;
        var ownedDecision = Assert.Single(input.RoutedDecisions);
        Assert.NotSame(sourceDecision, ownedDecision);
        Assert.NotSame(evidence, ownedDecision.EvidencePredicates);
        Assert.Equal(SubPhaseRoutingDecision.BaselineAuthorityRole, ownedDecision.AuthorityRole);
        Assert.Equal(["IMPLEMENT"], ownedDecision.EvidencePredicates);
        Assert.NotNull(input.TrainingRequest);
        Assert.Single(input.TrainingRequest.KnowledgeEdgeBranches);
    }

    [Fact]
    public void BuildKnowledgeEdgeBranches_AggregateNestedCancellationEscapesUnchanged()
    {
        var exception = new AggregateException(
            "aggregate outer",
            [new InvalidOperationException("nested wrapper", new OperationCanceledException("cancelled"))]);
        var decisions = new ThrowingDecisionCollection(exception);

        var actual = Assert.Throws<AggregateException>(() => BuildWithDecisionList(decisions));

        Assert.Same(exception, actual);
    }

    [Fact]
    public void BuildKnowledgeEdgeBranches_WrappedFatalEscapesUnchanged()
    {
        var exception = new InvalidOperationException(
            "fatal wrapper",
            new OutOfMemoryException("constructed but never thrown"));
        var decisions = new ThrowingDecisionCollection(exception);

        var actual = Assert.Throws<InvalidOperationException>(() => BuildWithDecisionList(decisions));

        Assert.Same(exception, actual);
    }

    [Fact]
    public void BuildKnowledgeEdgeBranches_OrdinaryNestedSnapshotFailureIsNormalized()
    {
        var decisions = new ThrowingDecisionCollection(new InvalidOperationException(
            "ordinary wrapper",
            new ArgumentException("ordinary inner")));

        var mapped = BuildWithDecisionList(decisions);

        Assert.False(mapped.IsSuccess);
        Assert.Null(mapped.Value);
        Assert.Equal("TEACHING_BRANCH_ROUTING_DECISION_INVALID", mapped.ErrorCode);
        Assert.Equal("Teaching branch routing decision is invalid.", mapped.ErrorMessage);
    }

    [Fact]
    public void BuildKnowledgeEdgeBranches_SharedOrdinaryInnerGraphTerminatesAndIsNormalized()
    {
        var shared = new InvalidOperationException("shared ordinary node");
        var decisions = new ThrowingDecisionCollection(
            new AggregateException(shared, new AggregateException(shared)));

        var mapped = BuildWithDecisionList(decisions);

        Assert.False(mapped.IsSuccess);
        Assert.Null(mapped.Value);
        Assert.Equal("TEACHING_BRANCH_ROUTING_DECISION_INVALID", mapped.ErrorCode);
        Assert.Equal("Teaching branch routing decision is invalid.", mapped.ErrorMessage);
    }

    private static SubPhaseRoutingDecision CreateDecision(
        string authorityRole,
        double confidence = 0.91d,
        double nearestWrongSubPhaseScore = 0d,
        string[]? evidencePredicates = null)
        => new(
            "teaching-row-decision",
            "ready",
            "4",
            "4.1",
            confidence,
            nearestWrongSubPhaseScore,
            evidencePredicates ?? ["IMPLEMENT"],
            false,
            "teaching_runner_contract")
        {
            AuthorityRole = authorityRole
        };

    private static AgentInfrastructure.OperationResult<LogicalFlowResult[]> BuildWithDecisionList(
        IReadOnlyList<SubPhaseRoutingDecision> decisions)
        => TeachingKnowledgeEdgeBranchMapper.Build(
            "teaching-row-throwing-list",
            "Implement the service.",
            "source-row.jsonl",
            "conversation-1",
            "message-1",
            decisions);

    private static AgentInfrastructure.OperationResult<TeachingProgramTrainingInput> PrepareProgramBoundary(
        AgentInfrastructure.OperationResult<SubPhaseRoutingDecision[]> route)
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var units = new InMemoryLogicalUnitRegistry();
        var request = new LogicalOrchestratorRequest(
            "teaching-program-boundary",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "Implement the service.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>(StringComparer.Ordinal));
        return TeachingKnowledgeEdgeBranchMapper.PrepareProgramTrainingInput(
            "teaching-program-boundary",
            request.UserRequest,
            "source-row.jsonl",
            "conversation-1",
            "message-1",
            route,
            request,
            topology,
            flows,
            units,
            SemanticCompiler29.Domain.CODE_Q);
    }

    private sealed class ThrowingDecisionCollection(Exception exception)
        : IReadOnlyList<SubPhaseRoutingDecision>, ICollection<SubPhaseRoutingDecision>
    {
        public int Count => 1;

        public bool IsReadOnly => true;

        public SubPhaseRoutingDecision this[int index] => throw exception;

        public void Add(SubPhaseRoutingDecision item) => throw exception;

        public void Clear() => throw exception;

        public bool Contains(SubPhaseRoutingDecision item) => false;

        public void CopyTo(SubPhaseRoutingDecision[] array, int arrayIndex) => throw exception;

        public IEnumerator<SubPhaseRoutingDecision> GetEnumerator() => throw exception;

        public bool Remove(SubPhaseRoutingDecision item) => throw exception;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
