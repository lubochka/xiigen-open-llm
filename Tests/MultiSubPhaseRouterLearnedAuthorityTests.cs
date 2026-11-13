// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;

public sealed class MultiSubPhaseRouterLearnedAuthorityTests
{
    private static readonly string[] ExactLeakageSignals =
        DynamicGraphEdgeAuthorityLeakageSignalVocabulary.CreateSnapshot();

    [Fact]
    public async Task LearnedAuthority_RunnerUpOverrideOwnsExactFinalOrderConfidenceAndRawBaselineInput()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend service implementation.", new[] { "IMPLEMENT" }));
        EdgeAuthorityInput? observed = null;
        var authority = CreateAuthority((request, _) =>
        {
            observed = request.Input;
            var baselineRank = BaselineRank(request.Input);
            Assert.Equal(2, baselineRank.Length);
            return Success(
                request,
                Decision(
                    [(baselineRank[1], 0.93d), (baselineRank[0], 0.81d)],
                    baselineRank));
        });
        var router = new MultiSubPhaseRouterUnit(
            topology,
            new SubPhaseRouterUnit(topology),
            authority.Resolved);

        var result = await router.ExecuteAsync(CreateRequest(
            CreateSource(
                "Design REST API endpoints and OpenAPI schema, then implement the backend service.",
                ["API_CONTRACT", "IMPLEMENT"])));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, authority.CallCount);
        Assert.NotNull(observed);
        Assert.All(observed!.Candidates, candidate =>
        {
            Assert.True(double.IsFinite(candidate.BaselineScore));
            Assert.True(candidate.BaselineScore > 0d);
            Assert.Equal(RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole, candidate.AuthorityRole);
        });
        var rawRank = BaselineRank(observed);
        Assert.Equal([rawRank[1], rawRank[0]], result.Value!.Output.Select(value => value.PredictedSubPhaseId));
        Assert.Equal([0.93d, 0.81d], result.Value.Output.Select(value => value.Confidence));
        Assert.All(
            result.Value.Output,
            value => Assert.Equal(RoutingAuthorityVocabulary.TrainedEdgeUnitRole, value.AuthorityRole));
        Assert.Equal(0.81d, result.Value.Output[0].NearestWrongSubPhaseScore);
        Assert.Equal(0d, result.Value.Output[1].NearestWrongSubPhaseScore);
    }

    [Fact]
    public async Task LearnedAuthority_OpenApiPromptCanSelectPositive33Route()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend service implementation.", new[] { "IMPLEMENT" }));
        var authority = CreateAuthority((request, _) =>
        {
            var rank = BaselineRank(request.Input);
            Assert.Contains("3.3", rank);
            return Success(request, Decision([("3.3", 0.97d)], rank));
        });
        var result = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(
            CreateSource(
                "Design the REST API endpoints and OpenAPI schema for the checkout service.",
                ["API_CONTRACT"])));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var route = Assert.Single(result.Value!.Output);
        Assert.Equal("3.3", route.PredictedSubPhaseId);
        Assert.Equal(0.97d, route.Confidence);
        Assert.Equal(RoutingAuthorityVocabulary.TrainedEdgeUnitRole, route.AuthorityRole);
    }

    [Fact]
    public async Task LearnedAuthority_InputIsPathFreeDeepOwnedAndRetainedSnapshotSurvivesDependencyMutation()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var callerContext = new ContextSignals(
            [SemanticCompiler29.Domain.ARCH],
            ["2.4"],
            ["knowledge:3.3"],
            new Dictionary<SemanticCompiler29.Domain, double>
            {
                [SemanticCompiler29.Domain.ARCH] = 0.72d
            });
        EdgeAuthorityInput? dependencyInput = null;
        string[]? receivedLeakageSignals = null;
        var authority = CreateAuthority((request, _) =>
        {
            dependencyInput = request.Input;
            receivedLeakageSignals = request.Input.ExcludedLeakageSignals.ToArray();
            var rank = BaselineRank(request.Input);
            var response = Success(request, Decision([("3.3", 0.94d)], rank));
            request.Input.SemanticTokens[0] = "mutated-semantic-token";
            request.Input.PredicateIds[0] = "MUTATED_PREDICATE";
            request.Input.Context.CompletedSubPhases[0] = "mutated-context";
            request.Input.Candidates[0].EvidencePredicates[0] = "MUTATED_EVIDENCE";
            request.Input.ExcludedLeakageSignals[0] = "mutated-leakage";
            return response;
        });
        var metadata = new Dictionary<string, object>
        {
            [ContextPacketReader.ContextSignalsMetadataKey] = callerContext
        };
        var source = CreateSource(
            "Design REST API endpoints and OpenAPI schema.",
            ["API_CONTRACT"]) with
        {
            DocumentId = "secret-document-id",
            SourcePath = "C:/private/source-path-only/secret-file.cs"
        };

        var result = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(source, metadata: metadata));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(dependencyInput);
        Assert.Equal("edge-multi-sub-phase-router-v1", dependencyInput!.EdgeId);
        Assert.DoesNotContain(dependencyInput.SemanticTokens, token => token.Contains("private", StringComparison.Ordinal));
        Assert.DoesNotContain(dependencyInput.SemanticTokens, token => token.Contains("secret", StringComparison.Ordinal));
        Assert.Equal(ExactLeakageSignals, receivedLeakageSignals);
        Assert.Equal(["API_CONTRACT"], result.Value!.Output[0].EvidencePredicates);
        Assert.Equal("2.4", callerContext.CompletedSubPhases[0]);
        Assert.Equal("knowledge:3.3", callerContext.ActiveKnowledgeEdges[0]);
        Assert.NotSame(callerContext.CompletedSubPhases, dependencyInput.Context.CompletedSubPhases);

        dependencyInput.Candidates[0].EvidencePredicates[0] = "MUTATED_AFTER_RETURN";
        Assert.Equal(["API_CONTRACT"], result.Value.Output[0].EvidencePredicates);
    }

    [Fact]
    public async Task NoAuthorityAndDisabledFlagAreBaselineEquivalentAndDisabledNeverCallsLearnedUnit()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend service implementation.", new[] { "IMPLEMENT" }));
        var source = CreateSource(
            "Design API endpoints and implement backend services.",
            ["API_CONTRACT", "IMPLEMENT"]);
        var baseline = await new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology))
            .ExecuteAsync(CreateRequest(source));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([(BaselineRank(request.Input)[0], 0.99d)], BaselineRank(request.Input))));
        var disabled = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(
            source,
            flags: new LogicalExecutionFlags { UseLearnedUnitWhenAvailable = false }));

        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        Assert.True(disabled.IsSuccess, disabled.ErrorMessage);
        Assert.Equal(0, authority.CallCount);
        AssertRoutingEquivalent(baseline.Value!.Output, disabled.Value!.Output);
        Assert.All(disabled.Value.Output, value =>
            Assert.Equal(RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole, value.AuthorityRole));
    }

    [Fact]
    public async Task EmptyPrimaryShortlistUsesSingleFallbackWithoutCallingLearnedAuthority()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([("3.3", 0.99d)], BaselineRank(request.Input))));
        var fallback = new FixedSingleRouter(
            CreateSingleDecision("document-learned-router", "3", "3.3"));
        var router = new MultiSubPhaseRouterUnit(topology, fallback, authority.Resolved);

        var result = await router.ExecuteAsync(CreateRequest(CreateSource(string.Empty, [])));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, authority.CallCount);
        Assert.Equal(
            RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
            Assert.Single(result.Value!.Output).AuthorityRole);
    }

    [Fact]
    public async Task EmptyPrimaryShortlistStillRejectsLearnedRoleFromLegacySingleFallback()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([("3.3", 0.99d)], BaselineRank(request.Input))));
        var learnedClaim = CreateSingleDecision("document-learned-router", "3", "3.3") with
        {
            AuthorityRole = RoutingAuthorityVocabulary.TrainedEdgeUnitRole
        };
        var router = new MultiSubPhaseRouterUnit(
            topology,
            new FixedSingleRouter(learnedClaim),
            authority.Resolved);

        var result = await router.ExecuteAsync(CreateRequest(CreateSource(string.Empty, [])));

        Assert.False(result.IsSuccess);
        Assert.Equal("SINGLE_ROUTER_AUTHORITY_ROLE_INVALID", result.ErrorCode);
        Assert.Equal(0, authority.CallCount);
    }

    [Theory]
    [InlineData("unit-id")]
    [InlineData("unit-role")]
    [InlineData("domain")]
    [InlineData("version")]
    [InlineData("registration-order")]
    [InlineData("reserved-registration-order")]
    [InlineData("learned")]
    [InlineData("candidate-trust")]
    [InlineData("rejected-trust")]
    [InlineData("input-type")]
    [InlineData("output-type")]
    [InlineData("missing-unit")]
    public async Task MalformedDescriptorMatrixFailsBeforeLearnedInvocation(string mutation)
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([("3.3", 0.95d)], BaselineRank(request.Input))));
        var descriptor = authority.Resolved.Descriptor;
        descriptor = mutation switch
        {
            "unit-id" => descriptor with { UnitId = " bad " },
            "unit-role" => descriptor with { UnitRole = "other-authority" },
            "domain" => descriptor with { DomainTag = "other-domain" },
            "version" => descriptor with { Version = "bad\nversion" },
            "registration-order" => descriptor with { RegistrationOrder = 0 },
            "reserved-registration-order" => descriptor with { RegistrationOrder = long.MaxValue },
            "learned" => descriptor with { IsLearned = false },
            "candidate-trust" => descriptor with { TrustState = LogicalUnitTrustState.Candidate },
            "rejected-trust" => descriptor with { TrustState = LogicalUnitTrustState.Rejected },
            "input-type" => descriptor with { InputType = typeof(string) },
            "output-type" => descriptor with { OutputType = typeof(string) },
            "missing-unit" => descriptor,
            _ => throw new ArgumentOutOfRangeException(nameof(mutation))
        };
        var unit = mutation == "missing-unit" ? null! : authority.Resolved.Unit;
        var malformed = new ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>(
            descriptor,
            unit);
        var router = new MultiSubPhaseRouterUnit(
            topology,
            new SubPhaseRouterUnit(topology),
            malformed);

        var result = await router.ExecuteAsync(CreateRequest(
            CreateSource("Design an OpenAPI contract.", ["API_CONTRACT"])));

        Assert.False(result.IsSuccess);
        Assert.Equal("MULTI_ROUTER_LEARNED_AUTHORITY_INVALID", result.ErrorCode);
        Assert.Equal(0, authority.CallCount);
    }

    [Theory]
    [InlineData("null-task", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_REQUIRED")]
    [InlineData("null-operation", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_REQUIRED")]
    [InlineData("null-carrier", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_REQUIRED")]
    [InlineData("null-decision", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_REQUIRED")]
    [InlineData("failed", "MULTI_ROUTER_LEARNED_AUTHORITY_EXECUTION_FAILED")]
    [InlineData("foreign-envelope", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("transition", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("unknown-target", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("duplicate-target", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("bad-confidence", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("bad-order", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("bad-fanout", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("bad-source", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("bad-ablation", "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID")]
    [InlineData("nonfatal", "MULTI_ROUTER_LEARNED_AUTHORITY_EXECUTION_FAILED")]
    public async Task LearnedResultAndFailureMatrixFailsClosed(string mutation, string expectedCode)
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend implementation.", new[] { "IMPLEMENT" }));
        var authority = CreateAuthorityTask((request, _) =>
        {
            var rank = BaselineRank(request.Input);
            var validTargets = rank.Length > 1
                ? new[] { (rank[0], 0.92d), (rank[1], 0.81d) }
                : new[] { (rank[0], 0.92d) };
            var decision = Decision(validTargets, rank);
            return mutation switch
            {
                "null-task" => null!,
                "null-operation" => Task.FromResult<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>>(null!),
                "null-carrier" => Task.FromResult(
                    OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Ok(null!)),
                "null-decision" => Task.FromResult(
                    OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Ok(
                        new LogicalUnitResult<EdgeAuthorityDecision>(
                            request.UnitId,
                            request.UnitRole,
                            request.DomainTag,
                            null!,
                            0.5d,
                            [],
                            [],
                            [],
                            []))),
                "failed" => Task.FromResult(
                    OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Fail("dependency failed", "DEPENDENCY_FAILED")),
                "foreign-envelope" => Task.FromResult(
                    WithEnvelopeUnitId(Success(request, decision), "foreign")),
                "transition" => Task.FromResult(
                    Success(request, decision, withTransition: true)),
                "unknown-target" => Task.FromResult(
                    Success(request, Decision([("9.9", 0.92d)], rank))),
                "duplicate-target" => Task.FromResult(
                    Success(request, Decision([(rank[0], 0.92d), (rank[0], 0.81d)], rank))),
                "bad-confidence" => Task.FromResult(
                    Success(request, Decision([(rank[0], double.NaN)], rank))),
                "bad-order" => Task.FromResult(
                    Success(request, Decision([(rank[0], 0.80d), (rank[^1], 0.90d)], rank))),
                "bad-fanout" => Task.FromResult(
                    Success(request, decision with { FanOutCount = decision.FanOutCount + 1 })),
                "bad-source" => Task.FromResult(
                    Success(request, decision with { AuthoritySource = "baseline_candidate_generator" })),
                "bad-ablation" => Task.FromResult(
                    Success(request, decision with { AblationBaselineRank = rank.Reverse().ToArray() })),
                "nonfatal" => throw new InvalidOperationException("nonfatal dependency failure"),
                _ => throw new ArgumentOutOfRangeException(nameof(mutation))
            };
        });
        var result = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(
            CreateSource(
                "Design API endpoints and implement backend services.",
                ["API_CONTRACT", "IMPLEMENT"])));

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
    }

    [Fact]
    public async Task LearnedKnowledgeEdgeReturnsHonestEmptyCarrierWithoutBaselineFallback()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var fallback = new FixedSingleRouter(CreateSingleDecision("document-learned-router", "3", "3.3"));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([], BaselineRank(request.Input), knowledgeEdge: true)));
        var router = new MultiSubPhaseRouterUnit(topology, fallback, authority.Resolved);

        var result = await router.ExecuteAsync(CreateRequest(
            CreateSource("Design an OpenAPI contract.", ["API_CONTRACT"])));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.Output);
        Assert.Equal(1, authority.CallCount);
        Assert.Equal(0, fallback.CallCount);
        Assert.Contains("learned_authority_knowledge_edge", result.Value.Warnings);
    }

    [Fact]
    public async Task TwoSegmentsInvokeLearnedAuthorityExactlyTwiceAndNeverInvokeParent()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend implementation.", new[] { "IMPLEMENT" }));
        var authority = CreateAuthority((request, _) =>
        {
            var rank = BaselineRank(request.Input);
            var confidence = request.Input.PredicateIds.Contains("IMPLEMENT", StringComparer.Ordinal)
                ? 0.99d
                : 0.81d;
            return Success(request, Decision([(rank[0], confidence)], rank));
        });
        var segments = new[]
        {
            CreateSource("Design an OpenAPI contract.", ["API_CONTRACT"]) with { DocumentId = "segment-api" },
            CreateSource("Implement backend services.", ["IMPLEMENT"]) with { DocumentId = "segment-code" }
        };
        var parent = CreateSource(
            "Parent content must not be independently routed.",
            ["API_CONTRACT", "IMPLEMENT"]) with
        {
            DocumentId = "segment-parent",
            SegmentDocuments = segments
        };

        var result = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(parent));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, authority.CallCount);
        Assert.Equal(2, result.Value!.Output.Length);
        Assert.Equal(["segment-api", "segment-code"], authority.SeenDocumentIds);
        Assert.Equal(["3.3", "4.1"], result.Value.Output.Select(decision => decision.PredictedSubPhaseId));
        Assert.Equal([0.81d, 0.99d], result.Value.Output.Select(decision => decision.Confidence));
    }

    [Fact]
    public async Task LearnedSegmentsRetainFirstExactDuplicateInStableOrder()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var authority = CreateAuthority((request, _) =>
        {
            var rank = BaselineRank(request.Input);
            return Success(request, Decision([("3.3", 0.93d)], rank));
        });
        var sharedSegment = CreateSource("Design an OpenAPI contract.", ["API_CONTRACT"]) with
        {
            DocumentId = "segment-shared"
        };
        var parent = CreateSource("parent", ["API_CONTRACT"]) with
        {
            SegmentDocuments =
            [
                sharedSegment,
                sharedSegment with { CompiledPredicateIds = sharedSegment.CompiledPredicateIds.ToArray() }
            ]
        };

        var result = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(parent));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var retained = Assert.Single(result.Value!.Output);
        Assert.Equal("segment-shared", retained.DocumentId);
        Assert.Equal("3.3", retained.PredictedSubPhaseId);
        Assert.Equal(0.93d, retained.Confidence);
        Assert.Equal(2, authority.CallCount);
    }

    [Fact]
    public async Task LearnedSegmentsRejectConflictingDuplicateTarget()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var invocation = 0;
        var authority = CreateAuthority((request, _) =>
        {
            invocation++;
            var rank = BaselineRank(request.Input);
            return Success(
                request,
                Decision([("3.3", invocation == 1 ? 0.93d : 0.99d)], rank));
        });
        var sharedSegment = CreateSource("Design an OpenAPI contract.", ["API_CONTRACT"]) with
        {
            DocumentId = "segment-shared"
        };
        var parent = CreateSource("parent", ["API_CONTRACT"]) with
        {
            SegmentDocuments =
            [
                sharedSegment,
                sharedSegment with { CompiledPredicateIds = sharedSegment.CompiledPredicateIds.ToArray() }
            ]
        };

        var result = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(parent));

        Assert.False(result.IsSuccess);
        Assert.Equal("MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID", result.ErrorCode);
        Assert.Equal(2, authority.CallCount);
    }

    [Fact]
    public async Task EnabledAuthorityWithEmptySegmentShortlistsPreservesLegacyBaselineConfidenceOrder()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend implementation.", new[] { "IMPLEMENT" }));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([("3.3", 0.99d)], BaselineRank(request.Input))));
        var enabledFallback = new FixedSingleRouter(source =>
            StringComparer.Ordinal.Equals(source.DocumentId, "segment-api")
                ? CreateSingleDecision(source.DocumentId, "3", "3.3") with { Confidence = 0.20d }
                : CreateSingleDecision(source.DocumentId, "4", "4.1") with { Confidence = 0.90d });
        var absentFallback = new FixedSingleRouter(source =>
            StringComparer.Ordinal.Equals(source.DocumentId, "segment-api")
                ? CreateSingleDecision(source.DocumentId, "3", "3.3") with { Confidence = 0.20d }
                : CreateSingleDecision(source.DocumentId, "4", "4.1") with { Confidence = 0.90d });
        var parent = CreateEmptyShortlistSegmentParent(
            ("segment-api", "first"),
            ("segment-code", "second"));

        var enabledResult = await new MultiSubPhaseRouterUnit(
                topology,
                enabledFallback,
                authority.Resolved)
            .ExecuteAsync(CreateRequest(parent));
        var absentResult = await new MultiSubPhaseRouterUnit(topology, absentFallback)
            .ExecuteAsync(CreateRequest(parent));

        Assert.True(enabledResult.IsSuccess, enabledResult.ErrorMessage);
        Assert.True(absentResult.IsSuccess, absentResult.ErrorMessage);
        AssertRouterCarriersEquivalent(absentResult.Value!, enabledResult.Value!);
        Assert.Equal(["4.1", "3.3"], enabledResult.Value!.Output.Select(
            decision => decision.PredictedSubPhaseId));
        Assert.Equal(0, authority.CallCount);
        Assert.Equal(2, enabledFallback.CallCount);
        Assert.Equal(2, absentFallback.CallCount);
    }

    [Fact]
    public async Task EnabledAuthorityWithEmptySegmentShortlistsUsesLegacyDuplicateWinner()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint design.", new[] { "API_CONTRACT" }));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([("3.3", 0.99d)], BaselineRank(request.Input))));
        var enabledFallback = new FixedSingleRouter(source =>
            CreateSingleDecision(source.DocumentId, "3", "3.3") with
            {
                Confidence = StringComparer.Ordinal.Equals(source.DocumentId, "segment-low")
                    ? 0.20d
                    : 0.90d
            });
        var absentFallback = new FixedSingleRouter(source =>
            CreateSingleDecision(source.DocumentId, "3", "3.3") with
            {
                Confidence = StringComparer.Ordinal.Equals(source.DocumentId, "segment-low")
                    ? 0.20d
                    : 0.90d
            });
        var parent = CreateEmptyShortlistSegmentParent(
            ("segment-low", "first"),
            ("segment-high", "second"));

        var enabledResult = await new MultiSubPhaseRouterUnit(
                topology,
                enabledFallback,
                authority.Resolved)
            .ExecuteAsync(CreateRequest(parent));
        var absentResult = await new MultiSubPhaseRouterUnit(topology, absentFallback)
            .ExecuteAsync(CreateRequest(parent));

        Assert.True(enabledResult.IsSuccess, enabledResult.ErrorMessage);
        Assert.True(absentResult.IsSuccess, absentResult.ErrorMessage);
        AssertRouterCarriersEquivalent(absentResult.Value!, enabledResult.Value!);
        var retained = Assert.Single(enabledResult.Value!.Output);
        Assert.Equal("segment-high", retained.DocumentId);
        Assert.Equal(0, authority.CallCount);
        Assert.Equal(2, enabledFallback.CallCount);
        Assert.Equal(2, absentFallback.CallCount);
    }

    [Fact]
    public async Task LearnedSegmentFailureFailsWholeAggregate()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend implementation.", new[] { "IMPLEMENT" }));
        var authority = CreateAuthority((request, _) =>
        {
            if (request.Input.PredicateIds.Contains("IMPLEMENT", StringComparer.Ordinal))
            {
                return OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Fail(
                    "second segment failed",
                    "SECOND_SEGMENT_FAILED");
            }

            var rank = BaselineRank(request.Input);
            return Success(request, Decision([(rank[0], 0.93d)], rank));
        });
        var parent = CreateSource("parent", ["API_CONTRACT", "IMPLEMENT"]) with
        {
            SegmentDocuments =
            [
                CreateSource("Design OpenAPI.", ["API_CONTRACT"]) with { DocumentId = "segment-api" },
                CreateSource("Implement backend.", ["IMPLEMENT"]) with { DocumentId = "segment-code" }
            ]
        };

        var result = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(parent));

        Assert.False(result.IsSuccess);
        Assert.Equal("MULTI_ROUTER_LEARNED_AUTHORITY_EXECUTION_FAILED", result.ErrorCode);
        Assert.Equal(2, authority.CallCount);
    }

    [Fact]
    public async Task AllKnowledgeEdgeSegmentsStayEmptyWithoutThirdParentInvocation()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend implementation.", new[] { "IMPLEMENT" }));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([], BaselineRank(request.Input), knowledgeEdge: true)));
        var parent = CreateSource("parent", ["API_CONTRACT", "IMPLEMENT"]) with
        {
            SegmentDocuments =
            [
                CreateSource("Design OpenAPI.", ["API_CONTRACT"]) with { DocumentId = "segment-api" },
                CreateSource("Implement backend.", ["IMPLEMENT"]) with { DocumentId = "segment-code" }
            ]
        };

        var result = await CreateRouter(topology, authority).ExecuteAsync(CreateRequest(parent));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.Output);
        Assert.Equal(2, authority.CallCount);
    }

    [Fact]
    public async Task CancellationBeforeInvocationReturnsCancelledWithoutPublication()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([("3.3", 0.95d)], BaselineRank(request.Input))));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await CreateRouter(topology, authority).ExecuteAsync(
            CreateRequest(CreateSource("Design OpenAPI.", ["API_CONTRACT"])),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.Equal(0, authority.CallCount);
    }

    [Fact]
    public async Task CancellationDuringInvocationReturnsCancelledWithoutPublication()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        using var cancellation = new CancellationTokenSource();
        var authority = CreateAuthorityTask((_, token) =>
        {
            cancellation.Cancel();
            return Task.FromException<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>>(
                new OperationCanceledException(token));
        });

        var result = await CreateRouter(topology, authority).ExecuteAsync(
            CreateRequest(CreateSource("Design OpenAPI.", ["API_CONTRACT"])),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.Equal(1, authority.CallCount);
    }

    [Fact]
    public async Task CancellationAfterDependencyResultReturnsCancelledBeforeRoutePublication()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        using var cancellation = new CancellationTokenSource();
        var authority = CreateAuthority((request, _) =>
        {
            var response = Success(request, Decision([("3.3", 0.95d)], BaselineRank(request.Input)));
            cancellation.Cancel();
            return response;
        });

        var result = await CreateRouter(topology, authority).ExecuteAsync(
            CreateRequest(CreateSource("Design OpenAPI.", ["API_CONTRACT"])),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.Equal(1, authority.CallCount);
    }

    [Fact]
    public async Task UnrequestedCancellationAndWrappedFatalEscapeTheRouterBoundary()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var unrequested = CreateAuthorityTask((_, _) =>
            Task.FromException<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>>(
                new OperationCanceledException("not requested")));
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            CreateRouter(topology, unrequested).ExecuteAsync(CreateRequest(
                CreateSource("Design OpenAPI.", ["API_CONTRACT"]))));

        var wrappedFatal = CreateAuthorityTask((_, _) =>
            Task.FromException<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>>(
                new AggregateException(new InvalidOperationException(
                    "wrapper",
                    new OutOfMemoryException("fatal")))));
        await Assert.ThrowsAsync<AggregateException>(() =>
            CreateRouter(topology, wrappedFatal).ExecuteAsync(CreateRequest(
                CreateSource("Design OpenAPI.", ["API_CONTRACT"]))));
    }

    [Fact]
    public async Task AdaptiveOrchestratorPreservesLearnedTargetSetMetadataAndSkipsTopologyBridge()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }),
            ("4.1", "4", "Backend Services", "Owns backend implementation.", new[] { "IMPLEMENT" }),
            ("5.1", "5", "Unit Tests", "Owns unit test verification.", new[] { "TEST" }));
        AddTransition(topology, "transition-api-code", "3.3", "4.1");
        AddTransition(topology, "transition-code-test", "4.1", "5.1");
        var authority = CreateAuthority((request, _) =>
        {
            var rank = BaselineRank(request.Input);
            return Success(request, Decision([("5.1", 0.96d), ("3.3", 0.88d)], rank));
        });
        var flowObservations = new List<(string Target, string Role, double Confidence)>();
        var flows = new InMemoryLogicalFlowRegistry();
        RegisterCaptureFlow(flows, "3", "3.3", flowObservations);
        RegisterCaptureFlow(flows, "4", "4.1", flowObservations);
        RegisterCaptureFlow(flows, "5", "5.1", flowObservations);
        var single = new SubPhaseRouterUnit(topology);
        var compiledSource = CreateSource(
            "For sub-phase 3.3 design API endpoints and OpenAPI schema, and for sub-phase 5.1 verify unit tests.",
            ["API_CONTRACT", "TEST"]);
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new FixedPromptCompiler(compiledSource),
            single,
            flows,
            new InMemoryLogicalUnitRegistry(),
            new MultiSubPhaseRouterUnit(topology, single, authority.Resolved));

        var result = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            "adaptive-learned-router",
            AdaptiveLogicalOrchestrator.ScenarioId,
            "For sub-phase 3.3 design API endpoints and OpenAPI schema, then for sub-phase 5.1 verify with unit tests.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.BranchResults.Length);
        Assert.Equal(["5.1", "3.3"], flowObservations.Select(value => value.Target));
        Assert.Equal(
            ["capture-flow-5.1", "capture-flow-3.3"],
            result.Value.BranchResults.Select(branch => branch.FlowId));
        Assert.DoesNotContain(flowObservations, value => value.Target == "4.1");
        Assert.All(flowObservations, value =>
            Assert.Equal(RoutingAuthorityVocabulary.TrainedEdgeUnitRole, value.Role));
        var observedConfidence = flowObservations.ToDictionary(
            value => value.Target,
            value => value.Confidence,
            StringComparer.Ordinal);
        Assert.Equal(0.96d, observedConfidence["5.1"]);
        Assert.Equal(0.88d, observedConfidence["3.3"]);
    }

    [Fact]
    public async Task AdaptiveOrchestratorRejectsUnknownRoleAndOversizedLearnedFanout()
    {
        var oneTopology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint design.", new[] { "API_CONTRACT" }));
        var unknownRoleRouter = new FixedMultiRouter((request, _) =>
            SuccessfulRouterCarrier(
                request,
                [CreateRoute(request.Input, "3", "3.3", 0.9d, "unknown-role")]));
        var unknownRoleResult = await CreateAdaptiveOrchestrator(oneTopology, unknownRoleRouter)
            .RunAsync(CreateAdaptiveRequest("unknown-role-run", "Design an API endpoint."));
        Assert.False(unknownRoleResult.IsSuccess);
        Assert.Equal("DYNAMIC_GRAPH_L4_ROUTER_RESULT_INVALID", unknownRoleResult.ErrorCode);

        var definitions = Enumerable.Range(1, AdaptiveLogicalOrchestrator.MaxAdaptiveBranchCount + 1)
            .Select(index => (
                $"{index}.1",
                index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                $"Sub-phase {index}",
                $"Owns target {index}.",
                new[] { $"TARGET_{index}" }))
            .ToArray();
        var largeTopology = CreateTopology(definitions);
        var oversizedAuthority = CreateAuthority((request, _) =>
        {
            var rank = BaselineRank(request.Input);
            Assert.Equal(definitions.Length, rank.Length);
            var targets = rank.Select((target, index) =>
                (target, 0.99d - (index * 0.01d))).ToArray();
            return Success(request, Decision(targets, rank));
        });
        var single = new SubPhaseRouterUnit(largeTopology);
        var oversizedRouter = new MultiSubPhaseRouterUnit(
            largeTopology,
            single,
            oversizedAuthority.Resolved);
        var oversizedPrompt = string.Join(
            " and ",
            definitions.Select(definition =>
                $"route sub-phase {definition.Item1} {definition.Item5[0]}"));
        var oversizedSource = CreateSource(
            oversizedPrompt,
            definitions.Select(definition => definition.Item5[0]).ToArray());
        var oversizedOrchestrator = new AdaptiveLogicalOrchestrator(
            largeTopology,
            new FixedPromptCompiler(oversizedSource),
            single,
            new InMemoryLogicalFlowRegistry(),
            new InMemoryLogicalUnitRegistry(),
            oversizedRouter);
        var oversizedResult = await oversizedOrchestrator.RunAsync(
            CreateAdaptiveRequest("oversized-learned-run", oversizedPrompt));
        Assert.False(oversizedResult.IsSuccess);
        Assert.Equal("DYNAMIC_GRAPH_L4_ROUTER_RESULT_INVALID", oversizedResult.ErrorCode);
    }

    [Fact]
    public async Task AdaptiveSingleRouterCannotClaimLearnedAuthority()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint design.", new[] { "API_CONTRACT" }));
        var observations = new List<(string Target, string Role, double Confidence)>();
        var flows = new InMemoryLogicalFlowRegistry();
        RegisterCaptureFlow(flows, "3", "3.3", observations);
        var spoofedDecision = CreateSingleDecision(
            "document-learned-router",
            "3",
            "3.3") with
        {
            AuthorityRole = RoutingAuthorityVocabulary.TrainedEdgeUnitRole
        };
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new FixedSingleRouter(spoofedDecision),
            flows,
            new InMemoryLogicalUnitRegistry());

        var result = await orchestrator.RunAsync(
            CreateAdaptiveRequest("single-router-learned-spoof", "Design an API endpoint."));

        Assert.False(result.IsSuccess);
        Assert.Equal("DYNAMIC_GRAPH_L4_ROUTER_RESULT_INVALID", result.ErrorCode);
        Assert.Empty(observations);
    }

    [Fact]
    public async Task AdaptiveKnowledgeEdgeEmptyRouteProducesNoFallbackBranch()
    {
        var topology = CreateTopology(
            ("3.3", "3", "API Contract", "Owns API endpoint and OpenAPI schema design.", new[] { "API_CONTRACT" }));
        var authority = CreateAuthority((request, _) =>
            Success(request, Decision([], BaselineRank(request.Input), knowledgeEdge: true)));
        var observations = new List<(string Target, string Role, double Confidence)>();
        var flows = new InMemoryLogicalFlowRegistry();
        RegisterCaptureFlow(flows, "3", "3.3", observations);
        var single = new SubPhaseRouterUnit(topology);
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            single,
            flows,
            new InMemoryLogicalUnitRegistry(),
            new MultiSubPhaseRouterUnit(topology, single, authority.Resolved));

        var result = await orchestrator.RunAsync(CreateAdaptiveRequest(
            "adaptive-empty-learned",
            "Design API endpoints and OpenAPI schema."));

        Assert.False(result.IsSuccess);
        Assert.Equal("NO_ROUTED_SUB_PHASES", result.ErrorCode);
        Assert.Empty(observations);
        Assert.Equal(1, authority.CallCount);
    }

    private static MultiSubPhaseRouterUnit CreateRouter(
        InMemoryLogicalTopologyRegistry topology,
        RecordingAuthority authority)
        => new(topology, new SubPhaseRouterUnit(topology), authority.Resolved);

    private static RecordingAuthority CreateAuthority(
        Func<LogicalUnitRequest<EdgeAuthorityInput>, CancellationToken,
            OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>> handler)
        => CreateAuthorityTask((request, cancellationToken) =>
            Task.FromResult(handler(request, cancellationToken)));

    private static RecordingAuthority CreateAuthorityTask(
        Func<LogicalUnitRequest<EdgeAuthorityInput>, CancellationToken,
            Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>>> handler)
    {
        var unit = new RecordingAuthorityUnit(handler);
        var descriptor = new LogicalUnitDescriptor(
            "unit-learned-router-authority-v1",
            "edge-authority",
            MultiSubPhaseRouterUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            true,
            typeof(EdgeAuthorityInput),
            typeof(EdgeAuthorityDecision),
            7);
        return new RecordingAuthority(
            new ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision>(
                descriptor,
                unit),
            unit);
    }

    private static OperationResult<LogicalUnitResult<EdgeAuthorityDecision>> Success(
        LogicalUnitRequest<EdgeAuthorityInput> request,
        EdgeAuthorityDecision decision,
        bool withTransition = false)
    {
        var result = new LogicalUnitResult<EdgeAuthorityDecision>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            decision,
            decision.OrderedTargets.Count == 0 ? 0.5d : decision.OrderedTargets.Min(value => value.Confidence),
            request.Input.PredicateIds.ToArray(),
            [request.DomainTag],
            ["learned-router-evidence"],
            [])
        {
            TransitionSignal = withTransition
                ? new PhaseTransitionSignal(
                    SemanticCompiler29.Domain.CODE_Q,
                    "foreign transition",
                    "foreign-node",
                    "3.3",
                    ["foreign-evidence"])
                : null
        };
        return OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Ok(result);
    }

    private static OperationResult<LogicalUnitResult<EdgeAuthorityDecision>> WithEnvelopeUnitId(
        OperationResult<LogicalUnitResult<EdgeAuthorityDecision>> source,
        string unitId)
    {
        var value = source.Value!;
        return OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Ok(
            value with { UnitId = unitId });
    }

    private static EdgeAuthorityDecision Decision(
        IReadOnlyList<(string TargetSubPhaseId, double Confidence)> targets,
        IReadOnlyList<string> baselineRank,
        bool knowledgeEdge = false)
        => new(
            targets.ToArray(),
            targets.Count,
            knowledgeEdge,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            baselineRank.ToArray());

    private static string[] BaselineRank(EdgeAuthorityInput input)
        => input.Candidates
            .OrderByDescending(candidate => candidate.BaselineScore)
            .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();

    private static LogicalUnitRequest<SubPhaseRouterSourceDocument> CreateRequest(
        SubPhaseRouterSourceDocument source,
        LogicalExecutionFlags? flags = null,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new(
            "run-learned-router",
            MultiSubPhaseRouterUnit.Id,
            MultiSubPhaseRouterUnit.Role,
            MultiSubPhaseRouterUnit.DomainTag,
            source,
            flags ?? new LogicalExecutionFlags(),
            metadata ?? new Dictionary<string, object>());

    private static SubPhaseRouterSourceDocument CreateEmptyShortlistSegmentParent(
        params (string DocumentId, string Title)[] segments)
        => CreateSource("parent", []) with
        {
            SegmentDocuments = segments
                .Select(segment => CreateSource(string.Empty, []) with
                {
                    DocumentId = segment.DocumentId,
                    DocumentTitle = segment.Title
                })
                .ToArray()
        };

    private static SubPhaseRouterSourceDocument CreateSource(
        string compiledText,
        string[] predicateIds)
        => new(
            "document-learned-router",
            "C:/path/firewall/never-score-this.cs",
            "internal",
            "owner-approved",
            "design",
            "Learned router source",
            compiledText,
            predicateIds,
            "ready",
            string.Empty,
            "whole-development-area",
            []);

    private static InMemoryLogicalTopologyRegistry CreateTopology(
        params (string SubPhaseId, string PhaseId, string Name, string Purpose, string[] Predicates)[] definitions)
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        foreach (var definition in definitions)
        {
            var result = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
                definition.SubPhaseId,
                definition.PhaseId,
                definition.Name,
                definition.Purpose,
                [$"domain-{definition.SubPhaseId}"],
                definition.Predicates,
                [$"source-{definition.SubPhaseId}"]));
            Assert.True(result.IsSuccess, result.ErrorMessage);
        }

        return topology;
    }

    private static void AddTransition(
        InMemoryLogicalTopologyRegistry topology,
        string transitionId,
        string from,
        string to)
    {
        var result = topology.AddOrUpdateTransition(new LogicalTransition(
            transitionId,
            "sub-phase",
            from,
            "sub-phase",
            to,
            $"Transition from {from} to {to}.",
            ["TOPOLOGY"],
            ["PRESERVE_CONTEXT"],
            ["learned-router-test"]));
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private static void AssertRoutingEquivalent(
        IReadOnlyList<SubPhaseRoutingDecision> expected,
        IReadOnlyList<SubPhaseRoutingDecision> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].DocumentId, actual[index].DocumentId);
            Assert.Equal(expected[index].SourceStatus, actual[index].SourceStatus);
            Assert.Equal(expected[index].PredictedPhaseId, actual[index].PredictedPhaseId);
            Assert.Equal(expected[index].PredictedSubPhaseId, actual[index].PredictedSubPhaseId);
            Assert.Equal(expected[index].Confidence, actual[index].Confidence);
            Assert.Equal(expected[index].NearestWrongSubPhaseScore, actual[index].NearestWrongSubPhaseScore);
            Assert.Equal(expected[index].EvidencePredicates, actual[index].EvidencePredicates);
            Assert.Equal(expected[index].KnowledgeEdge, actual[index].KnowledgeEdge);
            Assert.Equal(expected[index].FailureClassification, actual[index].FailureClassification);
            Assert.Equal(expected[index].AuthorityRole, actual[index].AuthorityRole);
        }
    }

    private static void AssertRouterCarriersEquivalent(
        LogicalUnitResult<SubPhaseRoutingDecision[]> expected,
        LogicalUnitResult<SubPhaseRoutingDecision[]> actual)
    {
        Assert.Equal(expected.UnitId, actual.UnitId);
        Assert.Equal(expected.UnitRole, actual.UnitRole);
        Assert.Equal(expected.DomainTag, actual.DomainTag);
        Assert.Equal(expected.Confidence, actual.Confidence);
        Assert.Equal(expected.PredicateIds, actual.PredicateIds);
        Assert.Equal(expected.DomainTags, actual.DomainTags);
        Assert.Equal(expected.EvidenceIds, actual.EvidenceIds);
        Assert.Equal(expected.Warnings, actual.Warnings);
        Assert.Null(expected.TransitionSignal);
        Assert.Null(actual.TransitionSignal);
        AssertRoutingEquivalent(expected.Output, actual.Output);
    }

    private static SubPhaseRoutingDecision CreateSingleDecision(
        string documentId,
        string phaseId,
        string subPhaseId)
        => new(
            documentId,
            "ready",
            phaseId,
            subPhaseId,
            0.91d,
            0d,
            ["FALLBACK"],
            false,
            "single_router_fallback")
        {
            AuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
        };

    private static AdaptiveLogicalOrchestrator CreateAdaptiveOrchestrator(
        InMemoryLogicalTopologyRegistry topology,
        ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]> multiRouter)
    {
        var single = new SubPhaseRouterUnit(topology);
        return new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            single,
            new InMemoryLogicalFlowRegistry(),
            new InMemoryLogicalUnitRegistry(),
            multiRouter);
    }

    private static LogicalOrchestratorRequest CreateAdaptiveRequest(string runId, string prompt)
        => new(
            runId,
            AdaptiveLogicalOrchestrator.ScenarioId,
            prompt,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());

    private static void RegisterCaptureFlow(
        InMemoryLogicalFlowRegistry flows,
        string phaseId,
        string subPhaseId,
        List<(string Target, string Role, double Confidence)> observations)
    {
        var result = flows.Register(
            $"capture-flow-{subPhaseId}",
            $"capture-scenario-{subPhaseId}",
            "1.0.0",
            phaseId,
            subPhaseId,
            "learned-router-capture",
            [$"domain-{subPhaseId}"],
            new CaptureFlow($"capture-flow-{subPhaseId}", $"capture-scenario-{subPhaseId}", observations));
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    private static OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>> SuccessfulRouterCarrier(
        LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
        SubPhaseRoutingDecision[] routes)
        => OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>.Ok(
            new LogicalUnitResult<SubPhaseRoutingDecision[]>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                routes,
                routes.Length == 0 ? 0d : routes.Min(route => route.Confidence),
                routes.SelectMany(route => route.EvidencePredicates).Distinct(StringComparer.Ordinal).ToArray(),
                routes.Select(route => route.PredictedSubPhaseId).ToArray(),
                [request.Input.DocumentId],
                []));

    private static SubPhaseRoutingDecision CreateRoute(
        SubPhaseRouterSourceDocument source,
        string phaseId,
        string subPhaseId,
        double confidence,
        string role)
        => new(
            source.DocumentId,
            source.SourceStatus,
            phaseId,
            subPhaseId,
            confidence,
            0d,
            ["FIXED_ROUTE"],
            false,
            "fixed_learned_route")
        {
            AuthorityRole = role
        };

    private sealed record RecordingAuthority(
        ResolvedLogicalUnitRegistryEntry<EdgeAuthorityInput, EdgeAuthorityDecision> Resolved,
        RecordingAuthorityUnit Unit)
    {
        public int CallCount => Unit.CallCount;

        public IReadOnlyList<string> SeenDocumentIds => Unit.SeenDocumentIds;
    }

    private sealed class RecordingAuthorityUnit : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
    {
        private readonly Func<LogicalUnitRequest<EdgeAuthorityInput>, CancellationToken,
            Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>>> _handler;
        private readonly List<string> _seenDocumentIds = [];

        public RecordingAuthorityUnit(
            Func<LogicalUnitRequest<EdgeAuthorityInput>, CancellationToken,
                Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>>> handler)
        {
            _handler = handler;
        }

        public int CallCount { get; private set; }

        public IReadOnlyList<string> SeenDocumentIds => _seenDocumentIds;

        public Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>> ExecuteAsync(
            LogicalUnitRequest<EdgeAuthorityInput> request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            _seenDocumentIds.Add(request.Input.PredicateIds.Contains("API_CONTRACT", StringComparer.Ordinal)
                ? "segment-api"
                : request.Input.PredicateIds.Contains("IMPLEMENT", StringComparer.Ordinal)
                    ? "segment-code"
                    : "unknown");
            return _handler(request, cancellationToken);
        }
    }

    private sealed class FixedSingleRouter : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
    {
        private readonly Func<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision> _selector;

        public FixedSingleRouter(SubPhaseRoutingDecision decision)
            : this(_ => decision)
        {
        }

        public FixedSingleRouter(
            Func<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision> selector)
        {
            _selector = selector;
        }

        public int CallCount { get; private set; }

        public Task<OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>> ExecuteAsync(
            LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            var selected = _selector(request.Input);
            var decision = selected with
            {
                DocumentId = request.Input.DocumentId,
                SourceStatus = request.Input.SourceStatus,
                EvidencePredicates = selected.EvidencePredicates.ToArray()
            };
            return Task.FromResult(OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Ok(
                new LogicalUnitResult<SubPhaseRoutingDecision>(
                    request.UnitId,
                    request.UnitRole,
                    request.DomainTag,
                    decision,
                    decision.Confidence,
                    decision.EvidencePredicates.ToArray(),
                    [decision.PredictedSubPhaseId],
                    [decision.DocumentId],
                    [])));
        }
    }

    private sealed class FixedMultiRouter
        : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
    {
        private readonly Func<LogicalUnitRequest<SubPhaseRouterSourceDocument>, CancellationToken,
            OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> _handler;

        public FixedMultiRouter(
            Func<LogicalUnitRequest<SubPhaseRouterSourceDocument>, CancellationToken,
                OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> handler)
        {
            _handler = handler;
        }

        public Task<OperationResult<LogicalUnitResult<SubPhaseRoutingDecision[]>>> ExecuteAsync(
            LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_handler(request, cancellationToken));
    }

    private sealed class FixedPromptCompiler
        : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
    {
        private readonly SubPhaseRouterSourceDocument _source;

        public FixedPromptCompiler(SubPhaseRouterSourceDocument source)
        {
            _source = source;
        }

        public Task<OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>> ExecuteAsync(
            LogicalUnitRequest<LogicalOrchestratorRequest> request,
            CancellationToken cancellationToken = default)
        {
            var output = _source with
            {
                CompiledPredicateIds = _source.CompiledPredicateIds.ToArray(),
                SegmentDocuments = _source.SegmentDocuments?.ToArray(),
                KnownFailureMode = string.IsNullOrEmpty(_source.KnownFailureMode)
                    ? "none"
                    : _source.KnownFailureMode
            };
            return Task.FromResult(
                OperationResult<LogicalUnitResult<SubPhaseRouterSourceDocument>>.Ok(
                    new LogicalUnitResult<SubPhaseRouterSourceDocument>(
                        request.UnitId,
                        request.UnitRole,
                        request.DomainTag,
                        output,
                        0.92d,
                        output.CompiledPredicateIds.ToArray(),
                        [request.DomainTag],
                        [output.DocumentId],
                        [])));
        }
    }

    private sealed class CaptureFlow : ILogicalFlow<string>
    {
        private readonly string _flowId;
        private readonly string _scenarioId;
        private readonly List<(string Target, string Role, double Confidence)> _observations;

        public CaptureFlow(
            string flowId,
            string scenarioId,
            List<(string Target, string Role, double Confidence)> observations)
        {
            _flowId = flowId;
            _scenarioId = scenarioId;
            _observations = observations;
        }

        public Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
            LogicalFlowRequest<string> request,
            CancellationToken cancellationToken = default)
        {
            var target = Assert.IsType<string>(request.Metadata["routed_sub_phase_id"]);
            var role = Assert.IsType<string>(
                request.Metadata[RoutingAuthorityVocabulary.MetadataKey]);
            var confidence = Assert.IsType<double>(request.Metadata["routing_confidence"]);
            lock (_observations)
                _observations.Add((target, role, confidence));

            var summary = new LogicalFlowSummary(
                $"{_flowId}-summary",
                1,
                0d,
                1d,
                "branch_success",
                false,
                true,
                false,
                "BRANCH_COMPLETE",
                1d,
                "learned router integration",
                "learned router integration",
                0d,
                0d);
            var result = new LogicalFlowResult(
                _flowId,
                _scenarioId,
                "PASS",
                "Capture flow completed.",
                summary,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["branch_success"] = "true",
                    ["routed_sub_phase_id"] = target,
                    [RoutingAuthorityVocabulary.MetadataKey] = role
                },
                [$"capture-{target}"],
                []);
            return Task.FromResult(OperationResult<LogicalFlowResult>.Ok(result));
        }
    }
}
