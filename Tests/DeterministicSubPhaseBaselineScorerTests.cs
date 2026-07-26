// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

// These tests pin the deterministic baseline through both production consumers without learned state.
public sealed class DeterministicSubPhaseBaselineScorerTests
{
    private static readonly string[] RequiredLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];

    public static TheoryData<string, string[]> CanonicalAnchorCases => new()
    {
        { "typography", ["1.1"] },
        { "responsive", ["1.2"] },
        { "navigation", ["1.3"] },
        { "wcag", ["1.4"] },
        { "business", ["2.1"] },
        { "stories", ["2.2"] },
        { "validation", ["2.3"] },
        { "ambiguity", ["2.4"] },
        { "topology", ["3.1"] },
        { "database", ["3.2"] },
        { "openapi", ["3.3"] },
        { "oauth", ["3.4"] },
        { "observability", ["3.5"] },
        { "backend", ["4.1"] },
        { "frontend", ["4.2"] },
        { "aws", ["4.3"] },
        { "quality", ["4.4"] },
        { "connector", ["4.5"] },
        { "unit", ["5.1"] },
        { "integration", ["4.5", "5.2"] },
        { "e2e", ["5.3"] },
        { "regression", ["5.4"] },
        { "exploratory", ["6.1"] },
        { "readiness", ["6.2"] },
        { "vulnerability", ["6.3"] },
        { "latency", ["6.4"] },
        { "harness", ["7.1"] },
        { "pipeline", ["7.2"] },
        { "signal", ["7.3"] },
        { "dashboard", ["7.4"] },
        { "teach", ["7.5"] }
    };

    [Fact]
    public async Task ExactSubPhaseIdBoundary_DoesNotCrossSelectNumericSuffix_InEitherConsumer()
    {
        var registry = CreateRegistry(
            ("4.4", Array.Empty<string>()),
            ("14.4", Array.Empty<string>()));
        var source = CreateSource("14.4", []);
        var edge = CreateEdge(["4.4", "14.4"]);

        var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
            .Propose(edge, CreateTraversal(source, ContextSignals.Empty));
        var routed = await RouteAsync(registry, source, ContextSignals.Empty);

        Assert.True(proposed.IsSuccess, proposed.ErrorMessage);
        var candidate = Assert.Single(proposed.Value!);
        Assert.Equal("14.4", candidate.TargetSubPhaseId);
        Assert.Equal(0.25d, candidate.BaselineScore, precision: 12);
        Assert.Equal("baseline_candidate_generator", candidate.AuthorityRole);
        Assert.Empty(candidate.EvidencePredicates);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var decision = Assert.Single(routed.Value!.Output);
        Assert.Equal("14.4", decision.PredictedSubPhaseId);
        Assert.Equal(0.77d, decision.Confidence, precision: 12);
        Assert.Equal(0.0d, decision.NearestWrongSubPhaseScore, precision: 12);
        Assert.False(decision.KnowledgeEdge);
    }

    [Theory]
    [InlineData("phase 4.4,")]
    [InlineData("knowledge:4.4")]
    public async Task DelimitedExactSubPhaseId_RemainsAnExactMention_InEitherConsumer(string sourceText)
    {
        var registry = CreateRegistry(("4.4", Array.Empty<string>()));
        var source = CreateSource(sourceText, []);
        var edge = CreateEdge(["4.4"]);

        var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
            .Propose(edge, CreateTraversal(source, ContextSignals.Empty));
        var routed = await RouteAsync(registry, source, ContextSignals.Empty);

        Assert.True(proposed.IsSuccess, proposed.ErrorMessage);
        var candidate = Assert.Single(proposed.Value!);
        Assert.Equal("4.4", candidate.TargetSubPhaseId);
        Assert.Equal(0.254d, candidate.BaselineScore, precision: 12);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var decision = Assert.Single(routed.Value!.Output);
        Assert.Equal("4.4", decision.PredictedSubPhaseId);
        Assert.Equal(0.77d, decision.Confidence, precision: 12);
    }

    [Theory]
    [InlineData("14.4")]
    [InlineData("4.40")]
    [InlineData("4.4.1")]
    [InlineData("x4.4")]
    public void EmbeddedOrExtendedSubPhaseId_DoesNotCountAsAnExactMention(string sourceText)
    {
        var registry = CreateRegistry(("4.4", Array.Empty<string>()));
        var source = CreateSource(sourceText, []);

        var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
            .Propose(CreateEdge(["4.4"]), CreateTraversal(source, ContextSignals.Empty));

        Assert.False(proposed.IsSuccess);
        Assert.Equal("EDGE_CANDIDATE_NO_MATCH", proposed.ErrorCode);
        Assert.Null(proposed.Value);
    }

    [Theory]
    [InlineData("implementation_planning", "implementation", "planning")]
    [InlineData("missing-subphase-id", "subphase", "id")]
    public async Task CallerOwnedExtendedId_DoesNotCrossMatchItsShortParts_InEitherConsumer(
        string fullId,
        string prefixPart,
        string suffixPart)
    {
        var registry = CreateRegistry(
            (prefixPart, new[] { "COMMON" }),
            (suffixPart, new[] { "COMMON" }),
            (fullId, new[] { "COMMON" }));
        var source = CreateSource(fullId, ["COMMON"]);
        var edge = CreateEdge([prefixPart, suffixPart, fullId]);

        var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
            .Propose(edge, CreateTraversal(source, ContextSignals.Empty));
        var routed = await RouteAsync(registry, source, ContextSignals.Empty);

        Assert.True(proposed.IsSuccess, proposed.ErrorMessage);
        var candidate = Assert.Single(proposed.Value!);
        Assert.Equal(fullId, candidate.TargetSubPhaseId);
        Assert.Equal(0.90d, candidate.BaselineScore, precision: 12);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var decision = Assert.Single(routed.Value!.Output);
        Assert.Equal(fullId, decision.PredictedSubPhaseId);
        Assert.Equal(0.94d, decision.Confidence, precision: 12);
        Assert.Equal(0.0d, decision.NearestWrongSubPhaseScore, precision: 12);
    }

    [Theory]
    [InlineData("implementation_planning")]
    [InlineData("missing-subphase-id")]
    public async Task CallerOwnedExtendedId_MatchesExactlyAtStartEndAndPunctuation_InEitherConsumer(string subPhaseId)
    {
        var registry = CreateRegistry((subPhaseId, new[] { "COMMON" }));
        var edge = CreateEdge([subPhaseId]);

        foreach (var sourceText in new[] { $"{subPhaseId},", $"phase:{subPhaseId}" })
        {
            var source = CreateSource(sourceText, ["COMMON"]);
            var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
                .Propose(edge, CreateTraversal(source, ContextSignals.Empty));
            var routed = await RouteAsync(registry, source, ContextSignals.Empty);

            Assert.True(proposed.IsSuccess, proposed.ErrorMessage);
            var candidate = Assert.Single(proposed.Value!);
            Assert.Equal(subPhaseId, candidate.TargetSubPhaseId);
            Assert.Equal(0.90d, candidate.BaselineScore, precision: 12);

            Assert.True(routed.IsSuccess, routed.ErrorMessage);
            var decision = Assert.Single(routed.Value!.Output);
            Assert.Equal(subPhaseId, decision.PredictedSubPhaseId);
            Assert.Equal(0.94d, decision.Confidence, precision: 12);
        }
    }

    [Fact]
    public async Task KnowledgeEdgeIdBoundary_BoostsOnlyTheExactTarget_InBothConsumers()
    {
        var registry = CreateRegistry(
            ("4.4", new[] { "COMMON" }),
            ("14.4", new[] { "COMMON" }));
        var source = CreateSource("quality", ["COMMON"]);
        var context = new ContextSignals([], [], ["knowledge:14.4"], new Dictionary<Domain, double>());
        var edge = CreateEdge(["4.4", "14.4"]);

        var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
            .Propose(edge, CreateTraversal(source, context));
        var routed = await RouteAsync(registry, source, context);

        Assert.True(proposed.IsSuccess, proposed.ErrorMessage);
        var proposedCandidates = Assert.IsType<EdgeCandidate[]>(proposed.Value);
        Assert.Equal(["14.4", "4.4"], proposedCandidates.Select(candidate => candidate.TargetSubPhaseId).ToArray());
        Assert.Equal(0.65d, proposedCandidates[0].BaselineScore, precision: 12);
        Assert.Equal(0.55d, proposedCandidates[1].BaselineScore, precision: 12);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(["14.4", "4.4"], routed.Value!.Output.Select(decision => decision.PredictedSubPhaseId).ToArray());
        Assert.Equal(0.88d, routed.Value.Output[0].Confidence, precision: 12);
        Assert.Equal(0.85d, routed.Value.Output[1].Confidence, precision: 12);
    }

    [Fact]
    public async Task EqualRawScores_UseOrdinalSubPhaseTieBreak_AndRemainStableAcrossConsumers()
    {
        // Register in reverse order so registry revision order cannot accidentally become the tie-break.
        var registry = CreateRegistry(
            ("5.1", new[] { "COMMON" }),
            ("4.4", new[] { "COMMON" }));
        var source = CreateSource("bug test", ["COMMON"]);
        var edge = CreateEdge(["5.1", "4.4"]);

        var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
            .Propose(edge, CreateTraversal(source, ContextSignals.Empty));
        var routed = await RouteAsync(registry, source, ContextSignals.Empty);

        Assert.True(proposed.IsSuccess, proposed.ErrorMessage);
        var proposedCandidates = Assert.IsType<EdgeCandidate[]>(proposed.Value);
        Assert.Equal(["4.4", "5.1"], proposedCandidates.Select(candidate => candidate.TargetSubPhaseId).ToArray());
        Assert.All(proposedCandidates, candidate => Assert.Equal(0.57d, candidate.BaselineScore, precision: 12));
        Assert.All(proposedCandidates, candidate => Assert.Equal(["COMMON"], candidate.EvidencePredicates));

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(["4.4", "5.1"], routed.Value!.Output.Select(decision => decision.PredictedSubPhaseId).ToArray());
        Assert.All(routed.Value.Output, decision => Assert.Equal(0.85d, decision.Confidence, precision: 12));
        Assert.Equal(0.28d, routed.Value.Output[0].NearestWrongSubPhaseScore, precision: 12);
        Assert.Equal(0.0d, routed.Value.Output[1].NearestWrongSubPhaseScore, precision: 12);
    }

    [Fact]
    public void RelevanceFloor_RemovesAWeakPositiveEchoBelowEightyPercentOfTheBestScore()
    {
        var registry = CreateRegistry(
            ("4.4", new[] { "COMMON" }),
            ("5.1", Array.Empty<string>()));
        var source = CreateSource("bug test", ["COMMON"]);

        var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
            .Propose(CreateEdge(["5.1", "4.4"]), CreateTraversal(source, ContextSignals.Empty));

        Assert.True(proposed.IsSuccess, proposed.ErrorMessage);
        var candidate = Assert.Single(proposed.Value!);
        Assert.Equal("4.4", candidate.TargetSubPhaseId);
        Assert.Equal(0.57d, candidate.BaselineScore, precision: 12);
        Assert.Equal(["COMMON"], candidate.EvidencePredicates);
    }

    [Fact]
    public void NonFiniteContextConfidence_FailsBeforeScoringWithStableCode()
    {
        var registry = CreateRegistry(("4.4", new[] { "COMMON" }));
        var source = CreateSource("refactoring", ["COMMON"]);
        var provider = new DeterministicRegistryEdgeCandidateProvider(registry);
        var edge = CreateEdge(["4.4"]);

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            var context = new ContextSignals(
                [Domain.REFACTOR],
                [],
                [],
                new Dictionary<Domain, double> { [Domain.REFACTOR] = invalid });

            var proposed = provider.Propose(edge, CreateTraversal(source, context));

            Assert.False(proposed.IsSuccess);
            Assert.Equal("EDGE_CANDIDATE_CONTEXT_INVALID", proposed.ErrorCode);
            Assert.Null(proposed.Value);
        }
    }

    [Theory]
    [MemberData(nameof(CanonicalAnchorCases))]
    public void CanonicalAnchorVocabulary_SelectsOnlyItsFrozenExpectedTargets(
        string promptAnchor,
        string[] expectedTargetIds)
    {
        var canonicalIds = DevelopmentLifecycleTopologySeeder.SubPhaseDefinitions()
            .Select(definition => definition.SubPhaseId)
            .ToArray();
        Assert.Equal(31, canonicalIds.Length);
        Assert.Equal(31, canonicalIds.Distinct(StringComparer.Ordinal).Count());

        var registry = CreateRegistry(canonicalIds.Select(id => (id, new[] { "COMMON" })).ToArray());
        var source = CreateSource(promptAnchor, ["COMMON"]);

        var proposed = new DeterministicRegistryEdgeCandidateProvider(registry)
            .Propose(CreateEdge(canonicalIds), CreateTraversal(source, ContextSignals.Empty));

        Assert.True(proposed.IsSuccess, proposed.ErrorMessage);
        var proposedCandidates = Assert.IsType<EdgeCandidate[]>(proposed.Value);
        Assert.Equal(expectedTargetIds, proposedCandidates.Select(candidate => candidate.TargetSubPhaseId).ToArray());
        Assert.All(proposedCandidates, candidate => Assert.Equal(0.57d, candidate.BaselineScore, precision: 12));
        Assert.All(proposedCandidates, candidate => Assert.True(double.IsFinite(candidate.BaselineScore)));
        Assert.All(proposedCandidates, candidate => Assert.Equal("baseline_candidate_generator", candidate.AuthorityRole));
    }

    private static InMemoryLogicalTopologyRegistry CreateRegistry(
        params (string SubPhaseId, string[] PredicateIds)[] definitions)
    {
        var registry = new InMemoryLogicalTopologyRegistry();
        foreach (var definition in definitions)
        {
            var registered = registry.AddOrUpdateSubPhase(new LogicalSubPhase(
                definition.SubPhaseId,
                definition.SubPhaseId.Split('.')[0],
                "Synthetic candidate",
                "Synthetic baseline candidate",
                ["fixture-domain"],
                definition.PredicateIds,
                ["fixture-source"]));
            Assert.True(registered.IsSuccess, registered.ErrorMessage);
        }

        return registry;
    }

    private static SubPhaseRouterSourceDocument CreateSource(
        string compiledText,
        string[] predicateIds)
        => new(
            DocumentId: "document-baseline-scorer",
            SourcePath: "source://provenance-only",
            SourceTier: "internal",
            LicenseOrAuthority: "owner-approved",
            DocumentType: string.Empty,
            DocumentTitle: string.Empty,
            CompiledText: compiledText,
            CompiledPredicateIds: predicateIds,
            SourceStatus: "ready",
            KnownFailureMode: string.Empty,
            DomainLabel: "fixture-domain",
            SegmentDocuments: []);

    private static TestEdge CreateEdge(string[] targetIds)
        => new(
            EdgeId: "edge-baseline-scorer",
            FromNodeId: "node-source",
            ToNodeIds: targetIds,
            Kind: GraphEdgeKind.FanOut,
            ParallelEligible: true,
            AuthorityUnitId: "unit-learned-edge-authority",
            AuthorityUnitLevel: "L2",
            DeterministicFinalAuthority: false,
            BaselineUnitId: DeterministicRegistryEdgeCandidateProvider.Id,
            BaselineRole: DeterministicRegistryEdgeCandidateProvider.BaselineRole,
            ExcludedLeakageSignals: RequiredLeakageSignals,
            SourceProof: "fixture-source-proof",
            CutoverProof: "fixture-cutover-proof",
            AblationProofRef: "fixture-ablation-proof");

    private static EdgeTraversalInput CreateTraversal(
        SubPhaseRouterSourceDocument source,
        ContextSignals context)
        => new(
            RunId: "run-baseline-scorer",
            EdgeId: "edge-baseline-scorer",
            Source: source,
            Context: context,
            UpstreamNodeOutput: new NodeOutputToEdgeInputAdapter(
                SourceNodeId: "node-source",
                UpstreamCandidateRefs: ["candidate-source"],
                UpstreamOutputHash: "sha256:source",
                ProjectionRule: "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)"));

    private static Task<OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> RouteAsync(
        InMemoryLogicalTopologyRegistry registry,
        SubPhaseRouterSourceDocument source,
        ContextSignals context)
    {
        IReadOnlyDictionary<string, object> metadata = new Dictionary<string, object>
        {
            [ContextPacketReader.ContextSignalsMetadataKey] = context
        };
        var request = new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
            "run-baseline-scorer",
            MultiSubPhaseRouterUnit.Id,
            MultiSubPhaseRouterUnit.Role,
            MultiSubPhaseRouterUnit.DomainTag,
            source,
            new LogicalExecutionFlags { WriteEvidenceArtifacts = false },
            metadata);

        return new MultiSubPhaseRouterUnit(registry, new SubPhaseRouterUnit(registry))
            .ExecuteAsync(request);
    }

    private sealed record TestEdge(
        string EdgeId,
        string FromNodeId,
        IReadOnlyList<string> ToNodeIds,
        GraphEdgeKind Kind,
        bool ParallelEligible,
        string AuthorityUnitId,
        string AuthorityUnitLevel,
        bool DeterministicFinalAuthority,
        string? BaselineUnitId,
        string BaselineRole,
        IReadOnlyList<string> ExcludedLeakageSignals,
        string SourceProof,
        string CutoverProof,
        string AblationProofRef) : IDynamicGraphEdge;
}
