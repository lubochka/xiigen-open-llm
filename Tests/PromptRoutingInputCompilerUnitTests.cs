// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using SemanticCompiler;
using SemanticCompilerAdapter;
using DevelopmentPromptCompiler = SemanticCompiler29.DevelopmentPromptCompiler;
using Domain = SemanticCompiler29.Domain;

public sealed class PromptRoutingInputCompilerUnitTests
{
    [Fact]
    public void PromptDomainClassifier_DetectsAllD04BusinessDomains()
    {
        // D-04 domain labels are the memory buckets used before sub-phase routing.
        Assert.Equal(Domain.BUG, PromptDomainClassifier.Classify("Fix the null exception in the Figma export flow.").Domain);
        Assert.Equal(Domain.REFACTOR, PromptDomainClassifier.Classify("Refactor and simplify the generated capability registry.").Domain);
        Assert.Equal(Domain.PLAN, PromptDomainClassifier.Classify("Plan the teaching workflow phases for the orchestrator.").Domain);
        Assert.Equal(Domain.ARCH, PromptDomainClassifier.Classify("Design the domain adapter topology and registry structure.").Domain);
        Assert.Equal(Domain.CODE_Q, PromptDomainClassifier.Classify("I need documentation and skills for this old codebase.").Domain);
    }

    [Fact]
    public void DomainClassifierUnit_KeywordPath_ReturnsCodeQuestionByDefault()
    {
        var result = new DomainClassifierUnit().Classify(new DomainClassifierRequest("what does this method do"));
        Assert.Equal(Domain.CODE_Q, result.Label);
        Assert.InRange(result.Confidence, 0.20, 0.99);
    }

    [Fact]
    public void DomainClassifierUnit_KeywordPath_ReturnsBugForRepairPrompt()
    {
        var result = new DomainClassifierUnit().Classify(new DomainClassifierRequest("fix the null reference crash"));
        Assert.Equal(Domain.BUG, result.Label);
        Assert.Contains("fix", result.Evidence);
    }

    [Fact]
    public void DomainClassifierUnit_KeywordPath_ReturnsPlanForWorkflowPrompt()
    {
        var result = new DomainClassifierUnit().Classify(new DomainClassifierRequest("plan the deployment workflow sequence"));
        Assert.Equal(Domain.PLAN, result.Label);
        Assert.Contains("plan", result.Evidence);
    }

    [Fact]
    public void DomainClassifierUnit_KnnPathFallsBackToKeywordsWhenUntrained()
    {
        var result = new DomainClassifierUnit().Classify(new DomainClassifierRequest(
            "fix the null reference",
            Predicates: [LogicExtractor.NULL_GUARD, LogicExtractor.FAIL]));

        Assert.Equal(Domain.BUG, result.Label);
    }

    [Fact]
    public void DomainClassifierUnit_TrainedPredicatePathOverridesRawKeywordFallback()
    {
        var classifier = new DomainClassifierUnit();
        var implementSlot = DomainClassifierUnit.EncodePredicateProfile(["IMPLEMENT"]);
        classifier.Train([(implementSlot, Domain.BUG)]);

        var result = classifier.Classify(new DomainClassifierRequest(
            "what does this method do",
            Predicates: implementSlot));

        Assert.Equal(Domain.BUG, result.Label);
        Assert.Contains("compiled_predicate_profile", result.Evidence);
    }

    [Fact]
    public void PromptDomainClassifier_UsesCompiledPromptMetadataOverRawPrompt()
    {
        var compiled = new SubPhaseRouterSourceDocument(
            "compiled-domain-test",
            "test://compiled",
            "runtime_prompt",
            "user_request",
            "user_prompt",
            "Compiled prompt",
            "ARCHITECTURE semantic_domain:ARCH semantic_stack:Generic",
            ["ARCHITECTURE"],
            "ready",
            "compiled_by_test",
            Domain.ARCH.ToString());
        var metadata = PromptDomainClassifier.AddCompiledPromptToMetadata(new Dictionary<string, object>(), compiled);

        var result = PromptDomainClassifier.Classify("what does this method do", metadata);

        Assert.Equal(Domain.ARCH, result.Domain);
        Assert.Contains("compiled_predicate_profile", result.EvidenceTerms);
    }

    [Fact]
    public void DomainClassifierUnit_ContextMatchBoostsConfidence()
    {
        var classifier = new DomainClassifierUnit();
        var baseline = classifier.Classify(new DomainClassifierRequest("fix the null reference crash"));
        var withContext = classifier.Classify(new DomainClassifierRequest(
            "fix the null reference crash",
            PriorContext: BuildContextSignals(Domain.BUG, confidence: "0.93")));

        Assert.Equal(Domain.BUG, withContext.Label);
        Assert.True(withContext.UsedContext);
        Assert.True(withContext.Confidence > baseline.Confidence);
        Assert.InRange(withContext.Confidence, 0.20, 0.99);
    }

    [Fact]
    public void DomainClassifierUnit_ContextShiftPenalizesConfidence()
    {
        var classifier = new DomainClassifierUnit();
        var baseline = classifier.Classify(new DomainClassifierRequest("fix the null reference crash"));
        var withContext = classifier.Classify(new DomainClassifierRequest(
            "fix the null reference crash",
            PriorContext: BuildContextSignals(Domain.PLAN, confidence: "0.91")));

        Assert.Equal(Domain.BUG, withContext.Label);
        Assert.True(withContext.UsedContext);
        Assert.True(withContext.Confidence < baseline.Confidence);
        Assert.InRange(withContext.Confidence, 0.20, 0.99);
    }

    [Fact]
    public void DomainClassifierUnit_ContextClampKeepsConfidenceInRange()
    {
        var high = new DomainClassifierUnit().Classify(new DomainClassifierRequest(
            "fix the null reference crash",
            PriorContext: BuildContextSignals(Domain.BUG, confidence: "0.99")));
        var low = new DomainClassifierUnit().Classify(new DomainClassifierRequest(
            "unmatched vocabulary",
            PriorContext: BuildContextSignals(Domain.BUG, confidence: "0.21")));

        Assert.InRange(high.Confidence, 0.20, 0.99);
        Assert.InRange(low.Confidence, 0.20, 0.99);
    }

    [Fact]
    public void ContextPacketReader_MissingContextReturnsEmptySignals()
    {
        var signals = ContextPacketReader.Read(new Dictionary<string, object>());
        Assert.True(signals.IsEmpty);
    }

    [Fact]
    public void ContextPacketReader_ExtractsPriorDomainCompletedSubPhaseAndKnowledgeEdges()
    {
        var packet = BuildContextPacket(
            Domain.BUG,
            confidence: "0.84",
            evidenceIds: ["knowledge_edge:4.4"]);
        var signals = ContextPacketReader.Read(new Dictionary<string, object>
        {
            [ContextPacketReader.ContextPacketMetadataKey] = packet,
            [ContextPacketReader.ContextSignalsMetadataKey] = BuildContextSignals(packet)
        });

        Assert.Contains(Domain.BUG, signals.PriorDomains);
        Assert.Contains("4.4", signals.CompletedSubPhases);
        Assert.Contains("knowledge_edge:4.4", signals.ActiveKnowledgeEdges);
        Assert.Equal(0.84, signals.PriorConfidenceMap[Domain.BUG], precision: 2);
    }

    [Fact]
    public void ContextPacketReader_DoesNotMarkConcernFragmentsAsCompleted()
    {
        var packet = BuildContextPacket(
            Domain.PLAN,
            confidence: "0.76",
            verdict: "CONCERN",
            subPhaseId: "2.4");
        var signals = ContextPacketReader.Read(BuildContextSignals(packet));

        Assert.Contains(Domain.PLAN, signals.PriorDomains);
        Assert.DoesNotContain("2.4", signals.CompletedSubPhases);
    }

    [Fact]
    public void ContextPacketReader_ReadsTypedSignalsWithoutReflection()
    {
        var packet = BuildContextPacket(
            Domain.BUG,
            confidence: "0.92",
            evidenceIds: ["knowledge_edge:3.4"]);
        var signals = BuildContextSignals(packet);

        var read = ContextPacketReader.Read(new Dictionary<string, object>
        {
            [ContextPacketReader.ContextSignalsMetadataKey] = signals
        });

        Assert.Contains(Domain.BUG, read.PriorDomains);
        Assert.Contains("4.4", read.CompletedSubPhases);
        Assert.Contains("knowledge_edge:3.4", read.ActiveKnowledgeEdges);
        Assert.Contains("3.4", read.ActiveKnowledgeEdges);
        Assert.Equal(0.92, read.PriorConfidenceMap[Domain.BUG], precision: 2);
    }

    [Fact]
    public void AttributedSegmentRouter_ClassifiesEachSegmentIndependently()
    {
        var routed = new AttributedSegmentRouter().Route(
        [
            new AttributedSegmentInput("s1", "fix the null export crash", ["IMPLEMENT"]),
            new AttributedSegmentInput("s2", "plan the workflow sequence", ["DOCUMENT"])
        ]);

        Assert.Collection(
            routed,
            first =>
            {
                Assert.Equal("s1", first.SegmentId);
                Assert.Equal(Domain.BUG, first.Domain);
                Assert.Contains("fix", first.Evidence);
            },
            second =>
            {
                Assert.Equal("s2", second.SegmentId);
                Assert.Equal(Domain.PLAN, second.Domain);
                Assert.Contains("plan", second.Evidence);
            });
    }

    [Fact]
    public void AttributedSegmentRouter_ExistingDomainOverridesClassifierWhenKnown()
    {
        var routed = new AttributedSegmentRouter().Route(
        [
            new AttributedSegmentInput("s1", "fix the null export crash", ["IMPLEMENT"], ExistingDomain: Domain.REFACTOR)
        ]);

        var segment = Assert.Single(routed);
        Assert.Equal(Domain.REFACTOR, segment.Domain);
        Assert.Equal("fix the null export crash", segment.Text);
    }

    [Fact]
    public async Task PromptRoutingInputCompilerUnit_LegacyDocsSkillsPrompt_EmitsMultiPhasePredicates()
    {
        // Build the real first prompt from the Claude export legacy documentation bundle.
        var request = BuildRequest(
            "legacy-docs-skills-compiler-test",
            "I need a documentation on this project and skills for claude code");

        // Compile the prompt into predicate evidence.
        var result = await new PromptRoutingInputCompilerUnit().ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));

        // The compiler must not collapse this common old-code task into DOCUMENT only.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains("DOCUMENT", result.Value!.Output.CompiledPredicateIds);
        Assert.Contains("ARCHITECTURE", result.Value.Output.CompiledPredicateIds);
        Assert.Contains("IMPLEMENT", result.Value.Output.CompiledPredicateIds);
        Assert.Contains("LEARNING", result.Value.Output.CompiledPredicateIds);
    }

    [Fact]
    public void DevelopmentPromptCompiler_MultiTaskPrompt_ProducesCanonicalSegmentsAndPredicates()
    {
        var result = new DevelopmentPromptCompiler().Compile(
            "Fix the null error in the Figma export and then write regression tests");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(Domain.BUG, result.Value!.Domain);
        Assert.Contains("IMPLEMENT", result.Value.DevelopmentPredicateIds);
        Assert.Contains("TEST", result.Value.DevelopmentPredicateIds);
        Assert.Contains("UI", result.Value.DevelopmentPredicateIds);
        Assert.True(result.Value.Segments.Length >= 2);
    }

    [Fact]
    public void DevelopmentPromptCompiler_AutomationPrompt_EmitsAutomationPredicate()
    {
        var result = new DevelopmentPromptCompiler().Compile(
            "Create a CI pipeline with quality gates and automated coverage reporting");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains("AUTOMATION", result.Value!.DevelopmentPredicateIds);
        Assert.Contains("TEST", result.Value.DevelopmentPredicateIds);
    }

    [Fact]
    public async Task PromptRoutingInputCompilerUnit_UsesCanonicalSemanticCompilerSegments()
    {
        var request = BuildRequest(
            "canonical-semantic-compiler-segment-test",
            "Fix the null error in the Figma export and then write regression tests");

        var result = await new PromptRoutingInputCompilerUnit().ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(Domain.BUG.ToString(), result.Value!.Output.DomainLabel);
        Assert.NotNull(result.Value.Output.SegmentDocuments);
        Assert.True(result.Value.Output.SegmentDocuments!.Length >= 2);
    }

    [Fact]
    public async Task PromptRoutingInputCompilerUnit_ResolvesAttributedSegmentRouterFromRegistry()
    {
        var units = new InMemoryLogicalUnitRegistry();
        var registered = units.Register<SemanticCompiler29.DevelopmentPromptCompilation, AttributedSegmentDocument[]>(
            "registry-attributed-segment-router-test",
            AttributedSegmentRouterUnit.Role,
            AttributedSegmentRouterUnit.DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Probationary,
            isLearned: true,
            new FixedAttributedSegmentRouter());
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var request = BuildRequest(
            "registry-attributed-segment-router-run",
            "Fix the null error in the Figma export and then write regression tests");

        var result = await new PromptRoutingInputCompilerUnit(logicalUnits: units).ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value!.Output.SegmentDocuments);
        Assert.All(result.Value.Output.SegmentDocuments!, segment =>
        {
            Assert.Equal(Domain.REFACTOR.ToString(), segment.DomainLabel);
            Assert.Contains("registry-attributed-router", segment.CompiledText);
        });
    }

    [Fact]
    public async Task MultiSubPhaseRouter_LegacyDocsSkillsPrompt_ReachesArchitectureImplementationAndLearning()
    {
        // Build the full development topology without relying on a hardcoded scenario id.
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
        Assert.True(seed.IsSuccess, seed.ErrorMessage);

        // Compile and route the first real legacy documentation prompt.
        var request = BuildRequest(
            "legacy-docs-skills-router-test",
            "I need a documentation on this project and skills for claude code");
        var compiler = new PromptRoutingInputCompilerUnit();
        var compiled = await compiler.ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);

        var routed = await new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology)).ExecuteAsync(new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
            request.RunId,
            MultiSubPhaseRouterUnit.Id,
            MultiSubPhaseRouterUnit.Role,
            MultiSubPhaseRouterUnit.DomainTag,
            compiled.Value!.Output,
            new LogicalExecutionFlags(),
            request.Metadata));

        // The route must cover the multi-phase meaning of old-code documentation and skill extraction.
        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var subPhaseIds = routed.Value!.Output.Select(decision => decision.PredictedSubPhaseId).ToArray();
        Assert.Contains("2.1", subPhaseIds);
        Assert.Contains("3.1", subPhaseIds);
        Assert.Contains("4.1", subPhaseIds);
        Assert.Contains("7.5", subPhaseIds);

        // Broad predicate evidence must not fan out into sibling sub-phases with no prompt anchor.
        Assert.DoesNotContain("3.2", subPhaseIds);
        Assert.DoesNotContain("3.5", subPhaseIds);
        Assert.DoesNotContain("4.3", subPhaseIds);
    }

    [Fact]
    public async Task MultiSubPhaseRouter_ReportsBaselineRoleAndNeverClaimsTrainedAuthority()
    {
        var topology = BuildPathLeakageTopology();
        var router = new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology));
        var source = BuildPathLeakageDocument(
            "baseline-authority-role",
            @"C:\neutral\implementation-notes.md",
            "Backend service implementation",
            "Implement backend service runtime logic.",
            ["IMPLEMENT"]);

        var routed = await router.ExecuteAsync(BuildMultiRouterRequest("baseline-authority-role", source));

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.NotEmpty(routed.Value!.Output);
        Assert.Equal("baseline_candidate_generator", RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole);
        Assert.Equal("trained_edge_unit", RoutingAuthorityVocabulary.TrainedEdgeUnitRole);
        Assert.Equal("routing_authority_role", RoutingAuthorityVocabulary.MetadataKey);
        Assert.Equal("baseline_candidate_generator", SubPhaseRoutingDecision.BaselineAuthorityRole);
        Assert.Equal("trained_edge_unit", SubPhaseRoutingDecision.TrainedAuthorityRole);
        Assert.Equal("routing_authority_role", SubPhaseRoutingDecision.AuthorityRoleMetadataKey);
        Assert.All(routed.Value.Output, decision =>
        {
            Assert.Equal(SubPhaseRoutingDecision.BaselineAuthorityRole, decision.AuthorityRole);
            Assert.NotEqual("trained_edge_unit", decision.AuthorityRole);
        });
    }

    [Theory]
    [InlineData("baseline_candidate_generator")]
    [InlineData("trained_edge_unit")]
    [InlineData("routing_authority_role")]
    public void RoutingAuthorityVocabulary_ExactLiteralHasOneProductionSourceOwner(string literal)
    {
        var repositoryRoot = FindRepositoryRoot();
        var quotedLiteral = $"\"{literal}\"";
        var excludedSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git", ".tmp", ".clone", "artifacts", "bin", "docs", "evidence", "obj",
            "Results", "TestResults", "Tests", "Tests.ApiDashboardWpf"
        };
        var owners = Directory
            .EnumerateFiles(repositoryRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(repositoryRoot, path)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(excludedSegments.Contains))
            .Where(path => File.ReadAllText(path).Contains(quotedLiteral, StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/'))
            .ToArray();

        Assert.Equal(["00.Core/RoutingAuthorityVocabulary.cs"], owners);
    }

    [Fact]
    public void SubPhaseRoutingDecision_LegacyJsonDefaultsBaselineAndExplicitTrainedRoleRoundTrips()
    {
        var baseline = new SubPhaseRoutingDecision(
            "authority-role-json",
            "ready",
            "4",
            "4.1",
            0.91d,
            0d,
            ["IMPLEMENT"],
            false,
            "json_compatibility");

        var baselineJson = System.Text.Json.JsonSerializer.Serialize(baseline);
        var legacyJson = Assert.IsType<System.Text.Json.Nodes.JsonObject>(
            System.Text.Json.Nodes.JsonNode.Parse(baselineJson));
        Assert.True(legacyJson.Remove(nameof(SubPhaseRoutingDecision.AuthorityRole)));

        var legacyRoundTrip = Assert.IsType<SubPhaseRoutingDecision>(
            System.Text.Json.JsonSerializer.Deserialize<SubPhaseRoutingDecision>(legacyJson.ToJsonString()));
        Assert.Equal(SubPhaseRoutingDecision.BaselineAuthorityRole, legacyRoundTrip.AuthorityRole);

        var trained = baseline with { AuthorityRole = SubPhaseRoutingDecision.TrainedAuthorityRole };
        var trainedRoundTrip = Assert.IsType<SubPhaseRoutingDecision>(
            System.Text.Json.JsonSerializer.Deserialize<SubPhaseRoutingDecision>(
                System.Text.Json.JsonSerializer.Serialize(trained)));
        Assert.Equal(SubPhaseRoutingDecision.TrainedAuthorityRole, trainedRoundTrip.AuthorityRole);
    }

    [Theory]
    [InlineData("trained_edge_unit")]
    [InlineData(" ")]
    [InlineData("unknown_authority_role")]
    public async Task MultiSubPhaseRouterUnit_RegisteredSingleFallbackRejectsNonBaselineAuthority(
        string authorityRole)
    {
        var topology = BuildPathLeakageTopology();
        var fallback = new InjectedFallbackSingleRouter("4.1", authorityRole);
        var router = new MultiSubPhaseRouterUnit(topology, fallback);
        var source = BuildNoScoreFallbackDocument("fallback-authority-rejected");

        var routed = await router.ExecuteAsync(BuildMultiRouterRequest("fallback-authority-rejected", source));

        Assert.False(routed.IsSuccess);
        Assert.Null(routed.Value);
        Assert.Equal("SINGLE_ROUTER_AUTHORITY_ROLE_INVALID", routed.ErrorCode);
        Assert.Equal(
            "Single sub-phase router authority role must be baseline_candidate_generator.",
            routed.ErrorMessage);
    }

    [Fact]
    public async Task MultiSubPhaseRouterUnit_RegisteredBaselineSingleFallbackReturnsOwnedDecision()
    {
        var topology = BuildPathLeakageTopology();
        var fallback = new InjectedFallbackSingleRouter(
            "4.1",
            SubPhaseRoutingDecision.BaselineAuthorityRole);
        var router = new MultiSubPhaseRouterUnit(topology, fallback);
        var source = BuildNoScoreFallbackDocument("fallback-baseline-owned");

        var routed = await router.ExecuteAsync(BuildMultiRouterRequest("fallback-baseline-owned", source));
        fallback.MutateDecisionEvidence();

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var decision = Assert.Single(routed.Value!.Output);
        Assert.Equal("4.1", decision.PredictedSubPhaseId);
        Assert.Equal(SubPhaseRoutingDecision.BaselineAuthorityRole, decision.AuthorityRole);
        Assert.Equal(["SINGLE_ROUTER_EVIDENCE"], decision.EvidencePredicates);
    }

    [Theory]
    [InlineData(FallbackEnvelopeMutation.NullSuccessfulValue, "SINGLE_ROUTER_RESULT_INVALID", "Single sub-phase router result is invalid.")]
    [InlineData(FallbackEnvelopeMutation.CarrierIdentityMismatch, "SINGLE_ROUTER_RESULT_IDENTITY_MISMATCH", "Single sub-phase router result identity does not match the request.")]
    [InlineData(FallbackEnvelopeMutation.DecisionIdentityMismatch, "SINGLE_ROUTER_RESULT_IDENTITY_MISMATCH", "Single sub-phase router result identity does not match the request.")]
    [InlineData(FallbackEnvelopeMutation.NullDecisionEvidence, "SINGLE_ROUTER_RESULT_INVALID", "Single sub-phase router result is invalid.")]
    [InlineData(FallbackEnvelopeMutation.NullCarrierEvidence, "SINGLE_ROUTER_RESULT_INVALID", "Single sub-phase router result is invalid.")]
    [InlineData(FallbackEnvelopeMutation.NonFiniteConfidence, "SINGLE_ROUTER_RESULT_INVALID", "Single sub-phase router result is invalid.")]
    [InlineData(FallbackEnvelopeMutation.NullTransitionEvidence, "SINGLE_ROUTER_RESULT_INVALID", "Single sub-phase router result is invalid.")]
    [InlineData(FallbackEnvelopeMutation.InvalidTransitionEvidence, "SINGLE_ROUTER_RESULT_INVALID", "Single sub-phase router result is invalid.")]
    public async Task MultiSubPhaseRouterUnit_MalformedSuccessfulSingleFallbackFailsClosed(
        FallbackEnvelopeMutation mutation,
        string expectedCode,
        string expectedMessage)
    {
        var topology = BuildPathLeakageTopology();
        var fallback = new InjectedFallbackSingleRouter(
            "4.1",
            SubPhaseRoutingDecision.BaselineAuthorityRole,
            mutation);
        var router = new MultiSubPhaseRouterUnit(topology, fallback);
        var source = BuildNoScoreFallbackDocument($"fallback-malformed-{mutation}");

        var routed = await router.ExecuteAsync(BuildMultiRouterRequest(source.DocumentId, source));

        Assert.False(routed.IsSuccess);
        Assert.Null(routed.Value);
        Assert.Equal(expectedCode, routed.ErrorCode);
        Assert.Equal(expectedMessage, routed.ErrorMessage);
    }

    [Theory]
    [InlineData("decision_evidence")]
    [InlineData("predicate_ids")]
    [InlineData("domain_tags")]
    [InlineData("evidence_ids")]
    [InlineData("warnings")]
    [InlineData("transition_evidence")]
    public void MultiSubPhaseRouterUnit_SingleFallbackSnapshotOwnsEveryMutableCarrierBeforeValidation(
        string carrierName)
    {
        var source = BuildNoScoreFallbackDocument("fallback-snapshot-all-carriers");
        var decisionEvidence = new[] { "SINGLE_ROUTER_EVIDENCE" };
        var predicateIds = new[] { "SINGLE_ROUTER_EVIDENCE" };
        var domainTags = new[] { "implementation" };
        var evidenceIds = new[] { "single-router-evidence" };
        var warnings = new[] { "warning" };
        var transitionEvidence = new[] { "transition-evidence" };
        var decision = new SubPhaseRoutingDecision(
            source.DocumentId,
            source.SourceStatus,
            "4",
            "4.1",
            0.93d,
            0.1d,
            decisionEvidence,
            false,
            "single_router_registered_fallback")
        {
            AuthorityRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole
        };
        var transition = new PhaseTransitionSignal(
            Domain.BUG,
            "Fix the admitted target.",
            "source-node",
            "4.1",
            transitionEvidence);
        var carrier = new LogicalUnitResult<SubPhaseRoutingDecision>(
            SubPhaseRouterUnit.Id,
            SubPhaseRouterUnit.Role,
            SubPhaseRouterUnit.DomainTag,
            decision,
            decision.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings)
        {
            TransitionSignal = transition
        };
        var method = typeof(MultiSubPhaseRouterUnit).GetMethod(
            "SnapshotSingleRouterResult",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var invoked = method.Invoke(null, [source, carrier]);
        var snapshot = Assert.IsType<AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>>(invoked);
        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);

        decisionEvidence[0] = "MUTATED_DECISION";
        predicateIds[0] = "MUTATED_PREDICATE";
        domainTags[0] = "MUTATED_DOMAIN";
        evidenceIds[0] = "MUTATED_EVIDENCE";
        warnings[0] = "MUTATED_WARNING";
        transitionEvidence[0] = "MUTATED_TRANSITION";

        var owned = snapshot.Value!;
        Assert.NotSame(decision, owned.Output);
        Assert.NotSame(transition, owned.TransitionSignal);
        switch (carrierName)
        {
            case "decision_evidence":
                Assert.NotSame(decisionEvidence, owned.Output.EvidencePredicates);
                Assert.Equal(["SINGLE_ROUTER_EVIDENCE"], owned.Output.EvidencePredicates);
                break;
            case "predicate_ids":
                Assert.NotSame(predicateIds, owned.PredicateIds);
                Assert.Equal(["SINGLE_ROUTER_EVIDENCE"], owned.PredicateIds);
                break;
            case "domain_tags":
                Assert.NotSame(domainTags, owned.DomainTags);
                Assert.Equal(["implementation"], owned.DomainTags);
                break;
            case "evidence_ids":
                Assert.NotSame(evidenceIds, owned.EvidenceIds);
                Assert.Equal(["single-router-evidence"], owned.EvidenceIds);
                break;
            case "warnings":
                Assert.NotSame(warnings, owned.Warnings);
                Assert.Equal(["warning"], owned.Warnings);
                break;
            case "transition_evidence":
                Assert.NotSame(transitionEvidence, owned.TransitionSignal!.EvidenceIds);
                Assert.Equal(["transition-evidence"], owned.TransitionSignal.EvidenceIds);
                break;
            default:
                throw new InvalidOperationException($"Unknown carrier '{carrierName}'.");
        }
    }

    [Theory]
    [InlineData("ordinary_nested_nonfatal", true)]
    [InlineData("aggregate_nested_cancellation", false)]
    [InlineData("nested_fatal", false)]
    [InlineData("shared_inner_graph", true)]
    public void MultiSubPhaseRouterUnit_SharedSnapshotExceptionClassifierUsesCompleteFatalTruthTable(
        string scenario,
        bool expected)
    {
        Exception exception;
        if (StringComparer.Ordinal.Equals(scenario, "ordinary_nested_nonfatal"))
        {
            exception = new InvalidOperationException(
                "ordinary outer",
                new ArgumentException("ordinary inner"));
        }
        else if (StringComparer.Ordinal.Equals(scenario, "aggregate_nested_cancellation"))
        {
            exception = new AggregateException(
                "aggregate outer",
                [new InvalidOperationException("nested wrapper", new OperationCanceledException("cancelled"))]);
        }
        else if (StringComparer.Ordinal.Equals(scenario, "nested_fatal"))
        {
            exception = new InvalidOperationException(
                "fatal wrapper",
                new OutOfMemoryException("constructed but never thrown"));
        }
        else if (StringComparer.Ordinal.Equals(scenario, "shared_inner_graph"))
        {
            var shared = new InvalidOperationException("shared nonfatal node");
            exception = new AggregateException(shared, new AggregateException(shared));
        }
        else
        {
            throw new InvalidOperationException($"Unknown exception scenario '{scenario}'.");
        }

        var actual = SnapshotExceptionGraphClassifier.IsNonFatal(exception);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task MultiSubPhaseRouter_AutomationPrompt_RoutesToPhaseSeven()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
        Assert.True(seed.IsSuccess, seed.ErrorMessage);

        var request = BuildRequest(
            "automation-routing-test",
            "Create a CI pipeline with quality gates and automated coverage reporting");
        var compiled = await new PromptRoutingInputCompilerUnit().ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);
        Assert.Contains("AUTOMATION", compiled.Value!.Output.CompiledPredicateIds);

        var routed = await new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology)).ExecuteAsync(new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
            request.RunId,
            MultiSubPhaseRouterUnit.Id,
            MultiSubPhaseRouterUnit.Role,
            MultiSubPhaseRouterUnit.DomainTag,
            compiled.Value.Output,
            new LogicalExecutionFlags(),
            request.Metadata));

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var subPhaseIds = routed.Value!.Output.Select(decision => decision.PredictedSubPhaseId).ToArray();
        Assert.Contains("7.2", subPhaseIds);
    }

    [Fact]
    public async Task AdaptiveLogicalOrchestrator_LegacyDocsSkillsPrompt_CapsExpandedBranches()
    {
        // Build the full development topology and universal flow registrations.
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
        Assert.True(seed.IsSuccess, seed.ErrorMessage);

        // Run the same broad legacy-docs prompt through the full adaptive orchestrator path.
        var request = BuildRequest(
            "legacy-docs-skills-adaptive-cap-test",
            "I need a documentation on this project and skills for claude code");
        var orchestrator = new AdaptiveLogicalOrchestrator(
            topology,
            new PromptRoutingInputCompilerUnit(),
            new SubPhaseRouterUnit(topology),
            flows,
            new InMemoryLogicalUnitRegistry(),
            new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology)));

        var result = await orchestrator.RunAsync(request);

        // E49 bridge expansion can add useful handoff branches, but it must not train the whole lifecycle.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.InRange(result.Value!.BranchResults.Length, 1, AdaptiveLogicalOrchestrator.MaxAdaptiveBranchCount);
        Assert.Equal(Domain.PLAN.ToString(), result.Value.FlowResult.Metrics[PromptDomainClassifier.DomainLabelMetadataKey]);
    }

    [Fact]
    public async Task PromptRoutingInputCompilerUnit_FigmaAsyncIssue_EmitsImplementationUiAndTestPredicates()
    {
        // Build a second real prompt from the same bundle that names UI code, an async issue, and a required fix.
        var request = BuildRequest(
            "figma-async-issue-compiler-test",
            "I have some issue with my figma code it uses figmaBase64 of just generated frame. Since we use the dynamic document, the moment we need to use it, it is still null. We are missing some async call which wasn't really finished and do not know where a fix is required.");

        // Compile the prompt into predicate evidence.
        var result = await new PromptRoutingInputCompilerUnit().ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));

        // The compiler must route this as implementation + UI + testing work, not as documentation just because "dynamic document" is a code entity.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.DoesNotContain("DOCUMENT", result.Value!.Output.CompiledPredicateIds);
        Assert.Contains("IMPLEMENT", result.Value.Output.CompiledPredicateIds);
        Assert.Contains("TEST", result.Value.Output.CompiledPredicateIds);
        Assert.Contains("UI", result.Value.Output.CompiledPredicateIds);
    }

    [Fact]
    public async Task MultiSubPhaseRouter_FigmaAsyncIssue_DoesNotRouteCodeEntityToDocumentationWork()
    {
        // Build the full development topology for the second real legacy-code prompt.
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
        Assert.True(seed.IsSuccess, seed.ErrorMessage);

        // Compile and route the prompt that uses "dynamic document" as a runtime data object.
        var request = BuildRequest(
            "figma-async-issue-router-test",
            "I have some issue with my figma code it uses figmaBase64 of just generated frame. Since we use the dynamic document, the moment we need to use it, it is still null. We are missing some async call which wasn't really finished and do not know where a fix is required.");
        var compiled = await new PromptRoutingInputCompilerUnit().ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);

        var routed = await new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology)).ExecuteAsync(new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
            request.RunId,
            MultiSubPhaseRouterUnit.Id,
            MultiSubPhaseRouterUnit.Role,
            MultiSubPhaseRouterUnit.DomainTag,
            compiled.Value!.Output,
            new LogicalExecutionFlags(),
            request.Metadata));

        // This is a UI implementation/debugging task; documentation/coverage branches are over-routing.
        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var subPhaseIds = routed.Value!.Output.Select(decision => decision.PredictedSubPhaseId).ToArray();
        Assert.Contains("4.1", subPhaseIds);
        Assert.Contains("4.2", subPhaseIds);
        Assert.Contains("4.4", subPhaseIds);
        Assert.Contains("5.1", subPhaseIds);
        Assert.Contains("5.4", subPhaseIds);
        Assert.DoesNotContain("2.1", subPhaseIds);
        Assert.DoesNotContain("7.4", subPhaseIds);
    }

    [Fact]
    public async Task MultiSubPhaseRouter_FigmaExportNoData_RoutesToDebuggingAndNotDesignSystemOnly()
    {
        // Build the full development topology for the fourth real legacy-code prompt.
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();
        var seed = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
        Assert.True(seed.IsSuccess, seed.ErrorMessage);

        // Compile and route the prompt whose "AI Design Canvas" product name previously over-routed to design-system work only.
        var request = BuildRequest(
            "figma-export-no-data-router-test",
            "To my regret this didn't help Error — see log below Figma frame export returned no data. Make sure the AI Design Canvas frame exists and try again.");
        var compiled = await new PromptRoutingInputCompilerUnit().ExecuteAsync(new LogicalUnitRequest<LogicalOrchestratorRequest>(
            request.RunId,
            PromptRoutingInputCompilerUnit.Id,
            PromptRoutingInputCompilerUnit.Role,
            PromptRoutingInputCompilerUnit.DomainTag,
            request,
            new LogicalExecutionFlags(),
            request.Metadata));
        Assert.True(compiled.IsSuccess, compiled.ErrorMessage);

        Assert.Contains("IMPLEMENT", compiled.Value!.Output.CompiledPredicateIds);
        Assert.Contains("TEST", compiled.Value.Output.CompiledPredicateIds);
        Assert.Contains("UI", compiled.Value.Output.CompiledPredicateIds);

        var routed = await new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology)).ExecuteAsync(new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
            request.RunId,
            MultiSubPhaseRouterUnit.Id,
            MultiSubPhaseRouterUnit.Role,
            MultiSubPhaseRouterUnit.DomainTag,
            compiled.Value.Output,
            new LogicalExecutionFlags(),
            request.Metadata));

        // This is a runtime UI export failure; "Design Canvas" is a product surface, not a design-system task.
        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var subPhaseIds = routed.Value!.Output.Select(decision => decision.PredictedSubPhaseId).ToArray();
        Assert.Contains("4.2", subPhaseIds);
        Assert.Contains("4.4", subPhaseIds);
        Assert.Contains("5.1", subPhaseIds);
        Assert.DoesNotContain("1.1", subPhaseIds);
    }

    [Fact]
    public async Task SubPhaseRouterUnit_DoesNotClassifyFromSourcePathOnly()
    {
        var topology = BuildPathLeakageTopology();
        var router = new SubPhaseRouterUnit(topology);
        var source = BuildPathLeakageDocument(
            "single-path-only",
            @"C:\leaky\phase-7-skills-learning-training-model.md",
            "General notes",
            "general notes",
            []);

        var routed = await router.ExecuteAsync(BuildSingleRouterRequest("single-path-only", source));

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.NotEqual("7.5", routed.Value!.Output.PredictedSubPhaseId);
        Assert.Contains("source_path_used_for_scoring=false", routed.Value.Warnings);
        Assert.Contains(routed.Value.Warnings, warning => warning.StartsWith("source_path_semantic_conflict:7.5->", StringComparison.Ordinal));
    }

    [Fact]
    public async Task MultiSubPhaseRouterUnit_DoesNotUseSourcePathInHybridRank()
    {
        var topology = BuildPathLeakageTopology();
        var router = new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology));
        var neutralPathSource = BuildPathLeakageDocument(
            "multi-neutral-path",
            @"C:\neutral\notes.md",
            "Backend service implementation",
            "Implement backend service runtime logic.",
            ["IMPLEMENT"]);
        var conflictingPathSource = neutralPathSource with
        {
            DocumentId = "multi-conflicting-path",
            SourcePath = @"C:\leaky\3.4-security-authorization-model.md"
        };

        var neutral = await router.ExecuteAsync(BuildMultiRouterRequest("multi-neutral-path", neutralPathSource));
        var conflicting = await router.ExecuteAsync(BuildMultiRouterRequest("multi-conflicting-path", conflictingPathSource));

        Assert.True(neutral.IsSuccess, neutral.ErrorMessage);
        Assert.True(conflicting.IsSuccess, conflicting.ErrorMessage);
        Assert.Equal(neutral.Value!.Output.Select(decision => decision.PredictedSubPhaseId), conflicting.Value!.Output.Select(decision => decision.PredictedSubPhaseId));
        Assert.Equal("4.1", conflicting.Value.Output[0].PredictedSubPhaseId);
        Assert.Contains("source_path_used_for_scoring=false", conflicting.Value.Warnings);
    }

    [Fact]
    public async Task SubPhaseRouterUnit_SourcePathStaysInProvenanceOnly()
    {
        var topology = BuildPathLeakageTopology();
        var router = new SubPhaseRouterUnit(topology);
        var neutralPathSource = BuildPathLeakageDocument(
            "single-neutral-path",
            @"C:\neutral\implementation-notes.md",
            "Backend service implementation",
            "Implement backend service runtime logic.",
            ["IMPLEMENT"]);
        var conflictingPathSource = neutralPathSource with
        {
            DocumentId = "single-conflicting-path",
            SourcePath = @"C:\leaky\3.4-security-authorization-model.md"
        };

        var neutral = await router.ExecuteAsync(BuildSingleRouterRequest("single-neutral-path", neutralPathSource));
        var conflicting = await router.ExecuteAsync(BuildSingleRouterRequest("single-conflicting-path", conflictingPathSource));

        Assert.True(neutral.IsSuccess, neutral.ErrorMessage);
        Assert.True(conflicting.IsSuccess, conflicting.ErrorMessage);
        Assert.Equal(neutral.Value!.Output.PredictedSubPhaseId, conflicting.Value!.Output.PredictedSubPhaseId);
        Assert.Equal(neutral.Value.Output.Confidence, conflicting.Value.Output.Confidence);
        Assert.Contains(@"source_path_provenance:C:\leaky\3.4-security-authorization-model.md", conflicting.Value.EvidenceIds);
        Assert.Contains("source_path_used_for_scoring=false", conflicting.Value.Warnings);
    }

    [Fact]
    public async Task MultiSubPhaseRouterUnit_PathLeakageWarningDoesNotRaiseConfidence()
    {
        var topology = BuildPathLeakageTopology();
        var router = new MultiSubPhaseRouterUnit(topology, new SubPhaseRouterUnit(topology));
        var neutralPathSource = BuildPathLeakageDocument(
            "multi-confidence-neutral-path",
            @"C:\neutral\implementation-notes.md",
            "Backend service implementation",
            "Implement backend service runtime logic.",
            ["IMPLEMENT"]);
        var conflictingPathSource = neutralPathSource with
        {
            DocumentId = "multi-confidence-conflicting-path",
            SourcePath = @"C:\leaky\3.4-security-authorization-model.md"
        };

        var neutral = await router.ExecuteAsync(BuildMultiRouterRequest("multi-confidence-neutral-path", neutralPathSource));
        var conflicting = await router.ExecuteAsync(BuildMultiRouterRequest("multi-confidence-conflicting-path", conflictingPathSource));

        Assert.True(neutral.IsSuccess, neutral.ErrorMessage);
        Assert.True(conflicting.IsSuccess, conflicting.ErrorMessage);
        Assert.Equal("4.1", neutral.Value!.Output[0].PredictedSubPhaseId);
        Assert.Equal("4.1", conflicting.Value!.Output[0].PredictedSubPhaseId);
        Assert.True(conflicting.Value.Output[0].Confidence <= neutral.Value.Output[0].Confidence);
        Assert.Contains("source_path_semantic_conflict:3.4->4.1", conflicting.Value.Warnings);
        Assert.Contains(@"source_path_provenance:C:\leaky\3.4-security-authorization-model.md", conflicting.Value.EvidenceIds);
    }

    [Fact]
    public async Task MultiSubPhaseRouterUnit_UnknownSingleFallbackIdDoesNotRouteToFirstEntry()
    {
        var topology = BuildPathLeakageTopology();
        var missingSubPhaseId = "missing-subphase-id";
        var router = new MultiSubPhaseRouterUnit(topology, new MissingFallbackSingleRouter(missingSubPhaseId));
        var source = BuildPathLeakageDocument(
            "unknown-fallback-no-first-route",
            @"C:\neutral\unknown-fallback.md",
            "Opaque fallback probe",
            "opaque vocabulary without lifecycle anchors",
            []);

        var routed = await router.ExecuteAsync(BuildMultiRouterRequest("unknown-fallback-no-first-route", source));

        Assert.False(routed.IsSuccess);
        Assert.Equal("FALLBACK_SUBPHASE_ID_NOT_REGISTERED", routed.ErrorCode);
        Assert.Contains(missingSubPhaseId, routed.ErrorMessage);
        Assert.Contains("not registered in active topology", routed.ErrorMessage);
        Assert.Null(routed.Value);
    }

    [Fact]
    public async Task MultiSubPhaseRouterUnit_FailsWhenFallbackIdMissing()
    {
        var topology = BuildPathLeakageTopology();
        var missingSubPhaseId = "missing-subphase-id";
        var router = new MultiSubPhaseRouterUnit(topology, new MissingFallbackSingleRouter(missingSubPhaseId));
        var source = BuildPathLeakageDocument(
            "unknown-fallback-knowledge-edge",
            @"C:\neutral\unknown-fallback.md",
            "Opaque fallback probe",
            "opaque vocabulary without lifecycle anchors",
            []);

        var routed = await router.ExecuteAsync(BuildMultiRouterRequest("unknown-fallback-knowledge-edge", source));

        Assert.False(routed.IsSuccess);
        Assert.Equal("FALLBACK_SUBPHASE_ID_NOT_REGISTERED", routed.ErrorCode);
        Assert.Contains(missingSubPhaseId, routed.ErrorMessage);
        Assert.Contains("Single-router fallback id", routed.ErrorMessage);
        Assert.Null(routed.Value);
    }

    [Fact]
    public async Task MultiSubPhaseRouterUnit_RequiresTopologyRegistryConsistency()
    {
        var topology = BuildPathLeakageTopology();
        var missingSubPhaseId = "missing-subphase-id";
        var router = new MultiSubPhaseRouterUnit(topology, new MissingFallbackSingleRouter(missingSubPhaseId));
        var source = BuildPathLeakageDocument(
            "unknown-fallback-topology-consistency",
            @"C:\neutral\unknown-fallback.md",
            "Opaque fallback probe",
            "opaque vocabulary without lifecycle anchors",
            []);

        var routed = await router.ExecuteAsync(BuildMultiRouterRequest("unknown-fallback-topology-consistency", source));

        Assert.False(routed.IsSuccess);
        Assert.Equal("FALLBACK_SUBPHASE_ID_NOT_REGISTERED", routed.ErrorCode);
        Assert.Contains(missingSubPhaseId, routed.ErrorMessage);
        Assert.Contains("active topology", routed.ErrorMessage);
        Assert.Null(routed.Value);
    }

    private static LogicalOrchestratorRequest BuildRequest(string runId, string prompt)
    {
        return new LogicalOrchestratorRequest(
            runId,
            AdaptiveLogicalOrchestrator.ScenarioId,
            prompt,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
    }

    private static InMemoryLogicalTopologyRegistry BuildPathLeakageTopology()
    {
        var topology = new InMemoryLogicalTopologyRegistry();
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "3.4",
            "3",
            "Security and Authorization Model",
            "Owns security authorization model decisions.",
            ["security"],
            ["SECURITY"],
            ["security-source"])).IsSuccess);
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "4.1",
            "4",
            "Backend Services",
            "Owns backend service implementation runtime work.",
            ["implementation"],
            ["IMPLEMENT"],
            ["implementation-source"])).IsSuccess);
        Assert.True(topology.AddOrUpdateSubPhase(new LogicalSubPhase(
            "7.5",
            "7",
            "Skills Learning",
            "Owns skills learning training model feedback.",
            ["learning"],
            ["LEARNING"],
            ["learning-source"])).IsSuccess);

        return topology;
    }

    private static SubPhaseRouterSourceDocument BuildPathLeakageDocument(
        string documentId,
        string sourcePath,
        string documentTitle,
        string compiledText,
        string[] predicateIds)
    {
        return new SubPhaseRouterSourceDocument(
            documentId,
            sourcePath,
            "test_source",
            "test_authority",
            "test_document",
            documentTitle,
            compiledText,
            predicateIds,
            "ready",
            "path_leakage_regression_guard");
    }

    private static SubPhaseRouterSourceDocument BuildNoScoreFallbackDocument(string documentId)
        => BuildPathLeakageDocument(
            documentId,
            @"C:\neutral\opaque-fallback.md",
            "Opaque fallback probe",
            "opaque vocabulary without lifecycle anchors",
            []);

    private static LogicalUnitRequest<SubPhaseRouterSourceDocument> BuildSingleRouterRequest(
        string runId,
        SubPhaseRouterSourceDocument source)
    {
        return new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
            runId,
            SubPhaseRouterUnit.Id,
            SubPhaseRouterUnit.Role,
            SubPhaseRouterUnit.DomainTag,
            source,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
    }

    private static LogicalUnitRequest<SubPhaseRouterSourceDocument> BuildMultiRouterRequest(
        string runId,
        SubPhaseRouterSourceDocument source)
    {
        return new LogicalUnitRequest<SubPhaseRouterSourceDocument>(
            runId,
            MultiSubPhaseRouterUnit.Id,
            MultiSubPhaseRouterUnit.Role,
            MultiSubPhaseRouterUnit.DomainTag,
            source,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>());
    }

    private static LogicalContextPacket BuildContextPacket(
        Domain domain,
        string confidence,
        string verdict = "PASS",
        string subPhaseId = "4.4",
        string[]? evidenceIds = null)
    {
        return new LogicalContextPacket(
            "domain-classifier-context-test",
            [$"flow-{subPhaseId}"],
            [$"scenario-{subPhaseId}"],
            [
                new LogicalContextFragment(
                    $"flow-{subPhaseId}",
                    $"scenario-{subPhaseId}",
                    verdict,
                    "completed branch",
                    subPhaseId.Split('.')[0],
                    subPhaseId,
                    new Dictionary<string, string>
                    {
                        [ContextPacketReader.DomainLabelMetadataKey] = domain.ToString(),
                        [ContextPacketReader.DomainConfidenceMetadataKey] = confidence
                    },
                    evidenceIds ?? [],
                    [])
            ],
            DateTimeOffset.UtcNow);
    }

    private static ContextSignals BuildContextSignals(
        Domain domain,
        string confidence,
        string verdict = "PASS",
        string subPhaseId = "4.4",
        string[]? evidenceIds = null)
        => BuildContextSignals(BuildContextPacket(domain, confidence, verdict, subPhaseId, evidenceIds));

    private static ContextSignals BuildContextSignals(LogicalContextPacket packet)
    {
        var priorDomains = new List<Domain>();
        var completedSubPhases = new List<string>();
        var activeKnowledgeEdges = new List<string>();
        var confidenceMap = new Dictionary<Domain, double>();

        foreach (var fragment in packet.Fragments)
        {
            if ((fragment.OverallVerdict is "PASS" or "PASS_WITH_CONCERNS" or "CONCERN") &&
                ContextPacketReader.TryReadDomain(fragment.Metrics, out var domain))
            {
                priorDomains.Add(domain);
                if (ContextPacketReader.TryReadConfidence(fragment.Metrics, out var parsedConfidence))
                    confidenceMap[domain] = parsedConfidence;
            }

            if (fragment.OverallVerdict == "PASS" && !string.IsNullOrWhiteSpace(fragment.SubPhaseId))
                completedSubPhases.Add(fragment.SubPhaseId!);

            foreach (var evidenceId in fragment.EvidenceIds)
            {
                if (!evidenceId.StartsWith("knowledge_edge:", StringComparison.OrdinalIgnoreCase))
                    continue;

                activeKnowledgeEdges.Add(evidenceId);
                var tokens = evidenceId.Split([' ', '=', ':', ';', ',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var subPhase = tokens.FirstOrDefault(token => token.Length == 3 && char.IsDigit(token[0]) && token[1] == '.' && char.IsDigit(token[2]));
                if (!string.IsNullOrWhiteSpace(subPhase))
                    activeKnowledgeEdges.Add(subPhase!);
            }
        }

        return new ContextSignals(
            priorDomains.Distinct().ToArray(),
            completedSubPhases.Distinct(StringComparer.Ordinal).ToArray(),
            activeKnowledgeEdges.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            confidenceMap);
    }

    private sealed class FixedAttributedSegmentRouter : ILogicalUnit<SemanticCompiler29.DevelopmentPromptCompilation, AttributedSegmentDocument[]>
    {
        public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<AttributedSegmentDocument[]>>> ExecuteAsync(
            LogicalUnitRequest<SemanticCompiler29.DevelopmentPromptCompilation> request,
            CancellationToken cancellationToken = default)
        {
            var segments = request.Input.Segments.Length <= 1
                ? [new SemanticCompiler29.DevelopmentPromptSegment(request.Input.Text, Domain.REFACTOR, [], request.Input.DevelopmentPredicateIds, 1.0)]
                : request.Input.Segments;
            var routed = segments
                .Select((segment, index) => new AttributedSegmentDocument(
                    $"{request.RunId}-registry-segment-{index + 1}",
                    segment.Text,
                    segment.DevelopmentPredicateIds,
                    Domain.REFACTOR,
                    0.99,
                    ["registry-attributed-router"]))
                .ToArray();
            var result = new LogicalUnitResult<AttributedSegmentDocument[]>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                routed,
                0.99,
                routed.SelectMany(segment => segment.PredicateIds).Distinct(StringComparer.Ordinal).ToArray(),
                [Domain.REFACTOR.ToString()],
                routed.Select(segment => segment.SegmentId).ToArray(),
                []);
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<AttributedSegmentDocument[]>>.Ok(result));
        }
    }

    private sealed class MissingFallbackSingleRouter : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
    {
        private readonly string _missingSubPhaseId;

        public MissingFallbackSingleRouter(string missingSubPhaseId)
        {
            _missingSubPhaseId = missingSubPhaseId;
        }

        public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>> ExecuteAsync(
            LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Fail("Cancelled.", "CANCELLED"));

            var decision = new SubPhaseRoutingDecision(
                request.Input.DocumentId,
                request.Input.SourceStatus,
                "missing-phase",
                _missingSubPhaseId,
                0.93,
                0.0,
                ["SINGLE_ROUTER_EVIDENCE"],
                false,
                "single_router_returned_unregistered_sub_phase");
            var result = new LogicalUnitResult<SubPhaseRoutingDecision>(
                SubPhaseRouterUnit.Id,
                SubPhaseRouterUnit.Role,
                SubPhaseRouterUnit.DomainTag,
                decision,
                decision.Confidence,
                decision.EvidencePredicates,
                [_missingSubPhaseId],
                [$"single-router-evidence:{_missingSubPhaseId}"],
                []);

            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Ok(result));
        }
    }

    public enum FallbackEnvelopeMutation
    {
        None,
        NullSuccessfulValue,
        CarrierIdentityMismatch,
        DecisionIdentityMismatch,
        NullDecisionEvidence,
        NullCarrierEvidence,
        NonFiniteConfidence,
        NullTransitionEvidence,
        InvalidTransitionEvidence
    }

    private sealed class InjectedFallbackSingleRouter(
        string targetSubPhaseId,
        string authorityRole,
        FallbackEnvelopeMutation mutation = FallbackEnvelopeMutation.None)
        : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
    {
        private readonly string[] _decisionEvidence = ["SINGLE_ROUTER_EVIDENCE"];

        public void MutateDecisionEvidence()
            => _decisionEvidence[0] = "MUTATED_AFTER_RETURN";

        public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>> ExecuteAsync(
            LogicalUnitRequest<SubPhaseRouterSourceDocument> request,
            CancellationToken cancellationToken = default)
        {
            if (mutation == FallbackEnvelopeMutation.NullSuccessfulValue)
            {
                return Task.FromResult(
                    AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Ok(null!));
            }

            var confidence = mutation == FallbackEnvelopeMutation.NonFiniteConfidence
                ? double.NaN
                : 0.93d;
            var decision = new SubPhaseRoutingDecision(
                mutation == FallbackEnvelopeMutation.DecisionIdentityMismatch
                    ? $"{request.Input.DocumentId}-foreign"
                    : request.Input.DocumentId,
                request.Input.SourceStatus,
                "4",
                targetSubPhaseId,
                confidence,
                0.1d,
                mutation == FallbackEnvelopeMutation.NullDecisionEvidence ? null! : _decisionEvidence,
                false,
                "single_router_registered_fallback")
            {
                AuthorityRole = authorityRole
            };
            var result = new LogicalUnitResult<SubPhaseRoutingDecision>(
                mutation == FallbackEnvelopeMutation.CarrierIdentityMismatch
                    ? "foreign-single-router"
                    : SubPhaseRouterUnit.Id,
                SubPhaseRouterUnit.Role,
                SubPhaseRouterUnit.DomainTag,
                decision,
                confidence,
                mutation == FallbackEnvelopeMutation.NullDecisionEvidence ? [] : _decisionEvidence,
                ["implementation"],
                mutation == FallbackEnvelopeMutation.NullCarrierEvidence ? null! : ["single-router-evidence"],
                [])
            {
                TransitionSignal = mutation is FallbackEnvelopeMutation.NullTransitionEvidence
                    or FallbackEnvelopeMutation.InvalidTransitionEvidence
                        ? new PhaseTransitionSignal(
                        Domain.BUG,
                        "Fix the admitted target.",
                        "source-node",
                        targetSubPhaseId,
                        mutation == FallbackEnvelopeMutation.NullTransitionEvidence
                            ? null!
                            : [" invalid-transition-evidence "])
                    : null
            };

            return Task.FromResult(
                AgentInfrastructure.OperationResult<LogicalUnitResult<SubPhaseRoutingDecision>>.Ok(result));
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
