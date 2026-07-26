// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphEdgeCandidateOrderLeakageGuardUnitTests
{
    private const string RunId = "candidate-order-guard-run";
    private const string DomainTag = "candidate-order-guard-domain";
    private const string UnitId = "candidate-order-trained-authority";

    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] IdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] IdentityControlledSignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] IdentityControlValues =
        [
            "edge-authority-source-id-control",
            "identity-control://edge-authority-source-path",
            "edge-authority-correlation-id-control"
        ];

    [Fact]
    public async Task ExecuteAsync_FirstCandidateAuthorityFailsClosedWithStableLeakageCode()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.FirstCandidate);
        var pipeline = CreatePipeline(authority, out var materializer);

        var result = await pipeline.ExecuteAsync(
            CreateEdge(),
            CreateTraversal(),
            DomainTag,
            new LogicalExecutionFlags());

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeCandidateOrderLeakageGuardUnit.LeakageCode, result.ErrorCode);
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(3, authority.CallCount);
        Assert.Equal(
            ["target-a|target-b|target-c", "target-a|target-b|target-c", "target-b|target-c|target-a"],
            authority.CandidateOrders);
    }

    [Fact]
    public async Task ExecuteAsync_SemanticAuthorityRunsFiveDistinctDeterministicProbesAndPreservesFinalOwnership()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var pipeline = CreatePipeline(authority, out var materializer);

        var result = await pipeline.ExecuteAsync(
            CreateEdge(),
            CreateTraversal(),
            DomainTag,
            new LogicalExecutionFlags());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(4, materializer.CallCount);
        Assert.Equal(9, authority.CallCount);
        Assert.Equal("target-c", Assert.Single(result.Value!.Decision.OrderedTargets).TargetSubPhaseId);
        Assert.Equal(RoutingAuthorityVocabulary.TrainedEdgeUnitRole, result.Value.Decision.AuthoritySource);
        Assert.Equal(
            [
                "target-a|target-b|target-c",
                "target-a|target-b|target-c",
                "target-b|target-c|target-a",
                "target-c|target-a|target-b",
                "target-c|target-b|target-a",
                "target-b|target-a|target-c",
                "target-a|target-b|target-c",
                "target-a|target-b|target-c",
                "target-a|target-b|target-c"
            ],
            authority.CandidateOrders);
        Assert.Equal(5, authority.CandidateOrders.Skip(1).Distinct(StringComparer.Ordinal).Count());

        var authorityEvidence = result.Value.AuthorityEvidence;
        Assert.Equal("edge-candidate-order", authorityEvidence.EdgeId);
        Assert.Equal(UnitId, authorityEvidence.AuthorityUnitId);
        Assert.Equal("L2", authorityEvidence.AuthorityUnitLevel);
        Assert.False(authorityEvidence.DeterministicFinalAuthority);
        Assert.Equal(DeterministicRegistryEdgeCandidateProvider.Id, authorityEvidence.BaselineUnitId);
        Assert.Equal("candidate_generator", authorityEvidence.BaselineRole);
        Assert.Equal(LeakageSignals, authorityEvidence.ExcludedLeakageSignals);
        Assert.Equal("source-proof", authorityEvidence.SourceProof);
        Assert.Equal("cutover-proof", authorityEvidence.CutoverProof);
        Assert.Equal("ablation-proof", authorityEvidence.AblationProofRef);

        var report = result.Value.CandidateOrderLeakageReport;
        Assert.Equal("edge-candidate-order", report.EdgeId);
        Assert.Equal(UnitId, report.ResolvedAuthorityUnitId);
        Assert.True(report.IsOrderInvariant);
        Assert.Equal(LeakageSignals, report.ExcludedLeakageSignals);
        Assert.Equal(
            Enumerable.Range(1, 5).Select(index => $"candidate-order-permutation-{index:D2}"),
            report.Probes.Select(probe => probe.ProbeId));
        Assert.Equal(
            [
                "target-a|target-b|target-c",
                "target-b|target-c|target-a",
                "target-c|target-a|target-b",
                "target-c|target-b|target-a",
                "target-b|target-a|target-c"
            ],
            report.Probes.Select(probe => string.Join(
                '|',
                probe.CandidateOrder.Select(candidate => candidate.TargetSubPhaseId))));
        Assert.All(report.Probes, probe =>
        {
            var target = Assert.Single(probe.Decision.OrderedTargets);
            Assert.Equal("target-c", target.TargetSubPhaseId);
            Assert.Equal(0.93d, target.Confidence);
            Assert.Equal(1, probe.Decision.FanOutCount);
            Assert.False(probe.Decision.KnowledgeEdge);
            Assert.Equal(RoutingAuthorityVocabulary.TrainedEdgeUnitRole, probe.Decision.AuthoritySource);
            Assert.Equal(["target-c", "target-b", "target-a"], probe.Decision.AblationBaselineRank);
        });

        var identityReport = result.Value.IdentityLeakageReport;
        Assert.Equal("edge-candidate-order", identityReport.EdgeId);
        Assert.Equal(UnitId, identityReport.ResolvedAuthorityUnitId);
        Assert.True(identityReport.IsIdentityInvariant);
        Assert.Equal(LeakageSignals, identityReport.ExcludedLeakageSignals);
        Assert.Equal(
            IdentityProbeIds,
            identityReport.Probes.Select(probe => probe.ProbeId));
        Assert.Equal(
            IdentityControlledSignals,
            identityReport.Probes.Select(probe => probe.ControlledSignal));
        Assert.Equal(
            IdentityControlValues,
            identityReport.Probes.Select(probe => probe.ControlValue));
        Assert.All(identityReport.Probes, probe => AssertDecisionEquals(result.Value.Decision, probe.Decision));
        Assert.Equal(report.EdgeId, authorityEvidence.EdgeId);
        Assert.Equal(identityReport.EdgeId, authorityEvidence.EdgeId);
        Assert.Equal(report.ResolvedAuthorityUnitId, authorityEvidence.AuthorityUnitId);
        Assert.Equal(identityReport.ResolvedAuthorityUnitId, authorityEvidence.AuthorityUnitId);
        Assert.Equal(report.ExcludedLeakageSignals, authorityEvidence.ExcludedLeakageSignals);
        Assert.Equal(identityReport.ExcludedLeakageSignals, authorityEvidence.ExcludedLeakageSignals);
    }

    [Fact]
    public async Task ExecuteAsync_NullBaselineUnitIdFailsBeforeAnyDependencyRuns()
    {
        var candidateProvider = new FixedCandidateProvider(candidateCount: 3);
        var materializer = new CountingMaterializer();
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var resolver = new FixedResolver(CreateDescriptor(), authority);
        var pipeline = new DynamicGraphEdgeAuthorityExecutionPipeline(
            candidateProvider,
            materializer,
            resolver);

        var result = await pipeline.ExecuteAsync(
            CreateEdge() with { BaselineUnitId = null },
            CreateTraversal(),
            DomainTag,
            new LogicalExecutionFlags());

        Assert.False(result.IsSuccess);
        Assert.Equal("EDGE_AUTHORITY_EDGE_INVALID", result.ErrorCode);
        Assert.Equal(0, candidateProvider.CallCount);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, resolver.CallCount);
        Assert.Equal(0, authority.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulReportGetterReturnsIndependentDeepSnapshots()
    {
        var result = await CreatePipeline(
                new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC),
                out _)
            .ExecuteAsync(
                CreateEdge(),
                CreateTraversal(),
                DomainTag,
                new LogicalExecutionFlags());
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");

        var firstRead = result.Value!.CandidateOrderLeakageReport;
        var secondBeforeMutation = result.Value.CandidateOrderLeakageReport;
        Assert.NotSame(firstRead, secondBeforeMutation);
        Assert.NotSame(firstRead.Probes, secondBeforeMutation.Probes);
        Assert.NotSame(firstRead.Probes[0], secondBeforeMutation.Probes[0]);
        Assert.NotSame(
            firstRead.Probes[0].CandidateOrder[0].EvidencePredicates,
            secondBeforeMutation.Probes[0].CandidateOrder[0].EvidencePredicates);
        Assert.NotSame(
            firstRead.Probes[0].Decision.OrderedTargets,
            secondBeforeMutation.Probes[0].Decision.OrderedTargets);
        Assert.NotSame(firstRead.ExcludedLeakageSignals, secondBeforeMutation.ExcludedLeakageSignals);

        firstRead.Probes[0].CandidateOrder[0].EvidencePredicates[0] = "HOSTILE";
        Assert.Throws<NotSupportedException>(() =>
            ((IList<(string TargetSubPhaseId, double Confidence)>)firstRead.Probes[0].Decision.OrderedTargets)[0]
                = ("hostile-target", 0.01d));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)firstRead.ExcludedLeakageSignals)[0] = "hostile-leakage-signal");

        var secondRead = result.Value.CandidateOrderLeakageReport;
        Assert.Equal("PREDICATE-A", secondRead.Probes[0].CandidateOrder[0].EvidencePredicates[0]);
        Assert.Equal("target-c", Assert.Single(secondRead.Probes[0].Decision.OrderedTargets).TargetSubPhaseId);
        Assert.Equal(LeakageSignals, secondRead.ExcludedLeakageSignals);

        var canonicalDecision = result.Value.Decision;
        var canonicalOrderReport = result.Value.CandidateOrderLeakageReport;
        var firstIdentityRead = result.Value.IdentityLeakageReport;
        var secondIdentityBeforeMutation = result.Value.IdentityLeakageReport;
        Assert.NotSame(firstIdentityRead, secondIdentityBeforeMutation);
        Assert.NotSame(firstIdentityRead.Probes, secondIdentityBeforeMutation.Probes);
        Assert.NotSame(firstIdentityRead.Probes[0], secondIdentityBeforeMutation.Probes[0]);
        Assert.NotSame(
            firstIdentityRead.Probes[0].Decision.OrderedTargets,
            secondIdentityBeforeMutation.Probes[0].Decision.OrderedTargets);
        Assert.NotSame(
            firstIdentityRead.Probes[0].Decision.AblationBaselineRank,
            secondIdentityBeforeMutation.Probes[0].Decision.AblationBaselineRank);
        Assert.NotSame(firstIdentityRead.ExcludedLeakageSignals, secondIdentityBeforeMutation.ExcludedLeakageSignals);

        var probesField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphEdgeIdentityLeakageReport).GetField(
                "_probes",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        var mutableProbes = Assert.IsType<DynamicGraphEdgeIdentityControlProbe[]>(
            probesField.GetValue(firstIdentityRead));
        mutableProbes[0] = new DynamicGraphEdgeIdentityControlProbe(
            "hostile-probe",
            "hostile-signal",
            "hostile-control",
            canonicalDecision with { OrderedTargets = [("target-a", 0.01d)] });
        var exclusionsField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphEdgeIdentityLeakageReport).GetField(
                "_excludedLeakageSignals",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        var mutableExclusions = Assert.IsType<string[]>(exclusionsField.GetValue(firstIdentityRead));
        mutableExclusions[0] = "hostile-exclusion";

        var secondIdentityRead = result.Value.IdentityLeakageReport;
        Assert.Equal(
            IdentityProbeIds,
            secondIdentityRead.Probes.Select(probe => probe.ProbeId));
        Assert.Equal(LeakageSignals, secondIdentityRead.ExcludedLeakageSignals);
        Assert.All(secondIdentityRead.Probes, probe => AssertDecisionEquals(canonicalDecision, probe.Decision));
        AssertDecisionEquals(canonicalDecision, result.Value.Decision);
        Assert.Equal(
            Enumerable.Range(1, 5).Select(index => $"candidate-order-permutation-{index:D2}"),
            result.Value.CandidateOrderLeakageReport.Probes.Select(probe => probe.ProbeId));
        AssertDecisionEquals(canonicalDecision, canonicalOrderReport.Probes[0].Decision);
    }

    [Fact]
    public void EdgeAuthorityExecutionResult_ConstructorOwnsIdentityLeakageReportSnapshot()
    {
        var candidates = CreateNormalizedCandidates();
        var input = CreateNormalizedInput(candidates);
        var canonicalDecision = CreateSemanticDecision(candidates);
        var originalAuthorityEvidence = CreateAuthorityEvidence();
        var candidateOrderReport = CreateCandidateOrderLeakageReport(candidates, canonicalDecision);
        var originalIdentityReport = CreateIdentityLeakageReport(canonicalDecision);
        var result = new EdgeAuthorityExecutionResult(
            input,
            candidates,
            canonicalDecision,
            originalAuthorityEvidence,
            candidateOrderReport,
            originalIdentityReport,
            0.93d,
            ["evidence"],
            []);

        var authorityExclusionsField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphEdgeAuthorityEvidence).GetField(
                "_excludedLeakageSignals",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        var originalAuthorityExclusions = Assert.IsType<string[]>(
            authorityExclusionsField.GetValue(originalAuthorityEvidence));
        originalAuthorityExclusions[0] = "forged-authority-exclusion";

        var probesField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphEdgeIdentityLeakageReport).GetField(
                "_probes",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        var originalProbes = Assert.IsType<DynamicGraphEdgeIdentityControlProbe[]>(
            probesField.GetValue(originalIdentityReport));
        originalProbes[0] = new DynamicGraphEdgeIdentityControlProbe(
            "forged-probe",
            "forged-signal",
            "forged-control",
            canonicalDecision with { OrderedTargets = [("target-a", 0.01d)] });
        var exclusionsField = Assert.IsAssignableFrom<FieldInfo>(
            typeof(DynamicGraphEdgeIdentityLeakageReport).GetField(
                "_excludedLeakageSignals",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        var originalExclusions = Assert.IsType<string[]>(
            exclusionsField.GetValue(originalIdentityReport));
        originalExclusions[0] = "forged-exclusion";

        var identityReport = result.IdentityLeakageReport;
        Assert.Equal("edge-candidate-order", identityReport.EdgeId);
        Assert.Equal(UnitId, identityReport.ResolvedAuthorityUnitId);
        Assert.True(identityReport.IsIdentityInvariant);
        Assert.Equal(IdentityProbeIds, identityReport.Probes.Select(probe => probe.ProbeId));
        Assert.Equal(
            IdentityControlledSignals,
            identityReport.Probes.Select(probe => probe.ControlledSignal));
        Assert.Equal(IdentityControlValues, identityReport.Probes.Select(probe => probe.ControlValue));
        Assert.Equal(LeakageSignals, identityReport.ExcludedLeakageSignals);
        Assert.All(identityReport.Probes, probe => AssertDecisionEquals(canonicalDecision, probe.Decision));
        AssertDecisionEquals(canonicalDecision, result.Decision);

        var preservedOrderReport = result.CandidateOrderLeakageReport;
        Assert.Equal(candidateOrderReport.EdgeId, preservedOrderReport.EdgeId);
        Assert.Equal(candidateOrderReport.ResolvedAuthorityUnitId, preservedOrderReport.ResolvedAuthorityUnitId);
        Assert.Equal(candidateOrderReport.IsOrderInvariant, preservedOrderReport.IsOrderInvariant);
        Assert.Equal(
            candidateOrderReport.Probes.Select(probe => probe.ProbeId),
            preservedOrderReport.Probes.Select(probe => probe.ProbeId));
        Assert.Equal(candidateOrderReport.ExcludedLeakageSignals, preservedOrderReport.ExcludedLeakageSignals);
        Assert.All(preservedOrderReport.Probes, probe => AssertDecisionEquals(canonicalDecision, probe.Decision));

        var firstAuthorityEvidence = result.AuthorityEvidence;
        var secondAuthorityEvidence = result.AuthorityEvidence;
        Assert.NotSame(firstAuthorityEvidence, secondAuthorityEvidence);
        Assert.NotSame(
            firstAuthorityEvidence.ExcludedLeakageSignals,
            secondAuthorityEvidence.ExcludedLeakageSignals);
        Assert.Equal(LeakageSignals, firstAuthorityEvidence.ExcludedLeakageSignals);
        var returnedAuthorityExclusions = Assert.IsType<string[]>(
            authorityExclusionsField.GetValue(firstAuthorityEvidence));
        returnedAuthorityExclusions[0] = "forged-returned-authority-exclusion";
        Assert.Equal(LeakageSignals, result.AuthorityEvidence.ExcludedLeakageSignals);
    }

    [Fact]
    public void DynamicGraphEdgeAuthorityEvidence_ConstructorOwnsExclusionsAndGetterReturnsFreshReadOnlyCopies()
    {
        var callerExclusions = LeakageSignals.ToArray();
        var evidence = CreateAuthorityEvidence(callerExclusions);

        callerExclusions[0] = "forged-caller-exclusion";
        var firstRead = evidence.ExcludedLeakageSignals;
        var secondRead = evidence.ExcludedLeakageSignals;

        Assert.NotSame(firstRead, secondRead);
        Assert.Equal(LeakageSignals, firstRead);
        Assert.Equal(LeakageSignals, secondRead);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)firstRead)[0] = "forged-read-exclusion");
        Assert.Equal(LeakageSignals, evidence.ExcludedLeakageSignals);
    }

    [Fact]
    public void EdgeAuthorityExecutionResult_RejectsNullAuthorityEvidence()
    {
        var candidates = CreateNormalizedCandidates();
        var input = CreateNormalizedInput(candidates);
        var decision = CreateSemanticDecision(candidates);

        Assert.Throws<ArgumentNullException>(() => new EdgeAuthorityExecutionResult(
            input,
            candidates,
            decision,
            null!,
            CreateCandidateOrderLeakageReport(candidates, decision),
            CreateIdentityLeakageReport(decision),
            0.93d,
            ["evidence"],
            []));
    }

    [Fact]
    public void EdgeAuthorityExecutionResult_RejectsNullCandidateOrderLeakageReport()
    {
        var candidates = CreateNormalizedCandidates();
        var input = CreateNormalizedInput(candidates);
        var decision = CreateSemanticDecision(candidates);

        Assert.Throws<ArgumentNullException>(() => new EdgeAuthorityExecutionResult(
            input,
            candidates,
            decision,
            CreateAuthorityEvidence(),
            null!,
            CreateIdentityLeakageReport(decision),
            0.93d,
            ["evidence"],
            []));
    }

    [Fact]
    public void EdgeAuthorityExecutionResult_RejectsNullIdentityLeakageReport()
    {
        var candidates = CreateNormalizedCandidates();
        var input = CreateNormalizedInput(candidates);
        var decision = CreateSemanticDecision(candidates);

        Assert.Throws<ArgumentNullException>(() => new EdgeAuthorityExecutionResult(
            input,
            candidates,
            decision,
            CreateAuthorityEvidence(),
            CreateCandidateOrderLeakageReport(candidates, decision),
            null!,
            0.93d,
            ["evidence"],
            []));
    }

    [Fact]
    public async Task GuardReport_OwnsFiveProbeSnapshotsAndExactLeakageBoundary()
    {
        var candidates = CreateNormalizedCandidates();
        var orderedTargets = new[] { (TargetSubPhaseId: "target-c", Confidence: 0.93d) };
        var ablationRank = new[] { "target-c", "target-b", "target-a" };
        var input = CreateNormalizedInput(candidates);
        var reference = new EdgeAuthorityDecision(
            orderedTargets,
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            ablationRank);
        var probeInput = new DynamicGraphEdgeCandidateOrderProbeInput(input, reference);

        candidates[0].EvidencePredicates[0] = "MUTATED";
        orderedTargets[0] = ("target-a", 0.01d);
        ablationRank[0] = "mutated";

        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var result = await CreateGuard(authority).ExecuteAsync(CreateGuardRequest(probeInput));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var report = result.Value!.Output;
        Assert.True(report.IsOrderInvariant);
        Assert.Equal("edge-candidate-order", report.EdgeId);
        Assert.Equal(UnitId, report.ResolvedAuthorityUnitId);
        Assert.Equal(LeakageSignals, report.ExcludedLeakageSignals);
        Assert.Equal(5, report.Probes.Count);
        Assert.Equal(5, report.Probes.Select(probe => probe.ProbeId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(5, report.Probes
            .Select(probe => string.Join('|', probe.CandidateOrder.Select(candidate => candidate.TargetSubPhaseId)))
            .Distinct(StringComparer.Ordinal)
            .Count());
        Assert.All(report.Probes, probe =>
        {
            Assert.Equal("target-c", Assert.Single(probe.Decision.OrderedTargets).TargetSubPhaseId);
            Assert.Equal(["target-c", "target-b", "target-a"], probe.Decision.AblationBaselineRank);
            Assert.DoesNotContain(
                probe.CandidateOrder.SelectMany(candidate => candidate.EvidencePredicates),
                predicate => StringComparer.Ordinal.Equals(predicate, "MUTATED"));
        });

        var firstRead = report.Probes[0];
        firstRead.CandidateOrder[0].EvidencePredicates[0] = "HOSTILE";
        Assert.DoesNotContain(
            report.Probes[0].CandidateOrder.SelectMany(candidate => candidate.EvidencePredicates),
            predicate => StringComparer.Ordinal.Equals(predicate, "HOSTILE"));
    }

    [Fact]
    public async Task ExecuteAsync_NullOrMalformedProbeFailsClosed()
    {
        var nullRequestResult = await CreateGuard(new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC))
            .ExecuteAsync(null!);
        Assert.False(nullRequestResult.IsSuccess);
        Assert.Equal(DynamicGraphEdgeCandidateOrderLeakageGuardUnit.GuardFailedCode, nullRequestResult.ErrorCode);

        var nullAuthorityResult = await CreateGuard(new RecordingAuthorityUnit(AuthorityBehavior.NullSuccess))
            .ExecuteAsync(CreateGuardRequest(CreateProbeInput()));
        Assert.False(nullAuthorityResult.IsSuccess);
        Assert.Equal(DynamicGraphEdgeCandidateOrderLeakageGuardUnit.GuardFailedCode, nullAuthorityResult.ErrorCode);

        var malformedDecisionResult = await CreateGuard(new RecordingAuthorityUnit(AuthorityBehavior.MalformedDecision))
            .ExecuteAsync(CreateGuardRequest(CreateProbeInput()));
        Assert.False(malformedDecisionResult.IsSuccess);
        Assert.Equal(DynamicGraphEdgeCandidateOrderLeakageGuardUnit.GuardFailedCode, malformedDecisionResult.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledTokenReturnsCancelledWithoutCallingAuthority()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await CreateGuard(authority).ExecuteAsync(
            CreateGuardRequest(CreateProbeInput()),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeCandidateOrderLeakageGuardUnit.CancelledCode, result.ErrorCode);
        Assert.Equal(0, authority.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_UncancelledNestedFatalEscapesTheGuardBoundary()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.NestedFatal);

        var exception = await Assert.ThrowsAsync<AggregateException>(() =>
            CreateGuard(authority).ExecuteAsync(CreateGuardRequest(CreateProbeInput())));

        Assert.Contains(exception.InnerExceptions, inner => inner is BadImageFormatException);
    }

    [Fact]
    public async Task ExecuteAsync_UncancelledOperationCanceledExceptionIsNotConvertedToCancelledCode()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.UncancelledCancellation);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            CreateGuard(authority).ExecuteAsync(CreateGuardRequest(CreateProbeInput())));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public async Task ExecuteAsync_FirstCandidateAuthorityIsDetectedForWiderCandidateSets(int candidateCount)
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.FirstCandidate);
        var pipeline = CreatePipeline(authority, out var materializer, candidateCount);

        var result = await pipeline.ExecuteAsync(
            CreateEdge(candidateCount),
            CreateTraversal(candidateCount),
            DomainTag,
            new LogicalExecutionFlags());

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeCandidateOrderLeakageGuardUnit.LeakageCode, result.ErrorCode);
        Assert.Equal(1, materializer.CallCount);
        Assert.Equal(3, authority.CallCount);
        Assert.Equal("target-a", authority.CandidateOrders[1].Split('|')[0]);
        Assert.Equal("target-b", authority.CandidateOrders[2].Split('|')[0]);
    }

    [Theory]
    [InlineData(1, "target-a,target-a,target-a,target-a,target-a")]
    [InlineData(2, "target-a|target-b,target-b|target-a,target-a|target-b,target-b|target-a,target-a|target-b")]
    public async Task ExecuteAsync_SmallCandidateSetsUseFiveDocumentedDeterministicProbes(
        int candidateCount,
        string expectedOrders)
    {
        var candidates = CreateNormalizedCandidates(candidateCount);
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var result = await CreateGuard(authority).ExecuteAsync(CreateGuardRequest(
            new DynamicGraphEdgeCandidateOrderProbeInput(
                CreateNormalizedInput(candidates),
                CreateSemanticDecision(candidates))));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(5, authority.CallCount);
        Assert.Equal(expectedOrders.Split(','), authority.CandidateOrders);
        Assert.Equal(5, result.Value!.Output.Probes.Count);
        Assert.Equal(
            Enumerable.Range(1, 5).Select(index => $"candidate-order-permutation-{index:D2}"),
            result.Value.Output.Probes.Select(probe => probe.ProbeId));
    }

    [Theory]
    [InlineData("aggregate")]
    [InlineData("ordinary")]
    public async Task Pipeline_CanonicalAuthorityNestedFatalEscapesUnchanged(string mode)
    {
        var behavior = mode == "aggregate"
            ? AuthorityBehavior.NestedFatal
            : AuthorityBehavior.OrdinaryWrappedFatal;
        var authority = new RecordingAuthorityUnit(behavior);
        var pipeline = CreatePipeline(authority, out _);

        var exception = await Assert.ThrowsAnyAsync<Exception>(() => pipeline.ExecuteAsync(
            CreateEdge(),
            CreateTraversal(),
            DomainTag,
            new LogicalExecutionFlags()));

        if (mode == "aggregate")
        {
            var aggregate = Assert.IsType<AggregateException>(exception);
            Assert.Contains(aggregate.InnerExceptions, inner => inner is BadImageFormatException);
        }
        else
        {
            var wrapper = Assert.IsType<InvalidOperationException>(exception);
            Assert.IsType<BadImageFormatException>(wrapper.InnerException);
        }
    }

    [Fact]
    public async Task Pipeline_UncancelledOperationCanceledExceptionEscapes()
    {
        var pipeline = CreatePipeline(
            new RecordingAuthorityUnit(AuthorityBehavior.UncancelledCancellation),
            out _);

        await Assert.ThrowsAsync<OperationCanceledException>(() => pipeline.ExecuteAsync(
            CreateEdge(),
            CreateTraversal(),
            DomainTag,
            new LogicalExecutionFlags()));
    }

    [Fact]
    public async Task Pipeline_CancelledSuppliedTokenReturnsCancelledBeforeAuthority()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var pipeline = CreatePipeline(authority, out _);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await pipeline.ExecuteAsync(
            CreateEdge(),
            CreateTraversal(),
            DomainTag,
            new LogicalExecutionFlags(),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeCandidateOrderLeakageGuardUnit.CancelledCode, result.ErrorCode);
        Assert.Equal(0, authority.CallCount);
    }

    [Fact]
    public async Task Pipeline_IdentityControlsRematerializeOnlyDeclaredFieldsAndUseExactRunIds()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var pipeline = CreatePipeline(authority, out var materializer);
        var traversal = CreateTraversal();

        var result = await pipeline.ExecuteAsync(
            CreateEdge(),
            traversal,
            DomainTag,
            new LogicalExecutionFlags());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(4, materializer.CallCount);
        Assert.Equal(9, authority.CallCount);
        Assert.Equal(
            Enumerable.Repeat(RunId, 8)
                .Append(IdentityControlValue(2)),
            authority.RunIds);

        var inputs = materializer.Inputs;
        AssertTraversalEqualWithControls(
            inputs[0],
            inputs[1],
            IdentityControlValue(0),
            inputs[0].Source.SourcePath,
            RunId);
        AssertTraversalEqualWithControls(
            inputs[0],
            inputs[2],
            inputs[0].Source.DocumentId,
            IdentityControlValue(1),
            RunId);
        AssertTraversalEqualWithControls(
            inputs[0],
            inputs[3],
            inputs[0].Source.DocumentId,
            inputs[0].Source.SourcePath,
            IdentityControlValue(2));
    }

    [Theory]
    [InlineData(MaterializerBehavior.SourceIdSemanticLeakage, 2, 6)]
    [InlineData(MaterializerBehavior.SourcePathSemanticLeakage, 3, 7)]
    public async Task Pipeline_SourceIdentitySemanticLeakageFailsClosed(
        MaterializerBehavior behavior,
        int expectedMaterializerCalls,
        int expectedAuthorityCalls)
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var materializer = new ControlledMaterializer(behavior);
        var pipeline = CreatePipeline(authority, materializer);

        var result = await pipeline.ExecuteAsync(
            CreateEdge(),
            CreateTraversal(),
            DomainTag,
            new LogicalExecutionFlags());

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeIdentityLeakageGuardUnit.LeakageCode, result.ErrorCode);
        Assert.Equal(expectedMaterializerCalls, materializer.CallCount);
        Assert.Equal(expectedAuthorityCalls, authority.CallCount);
    }

    [Fact]
    public async Task Pipeline_RunIdSensitiveAuthorityPassesPriorControlsAndFailsCorrelationControl()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.RunIdSensitive);
        var materializer = new ControlledMaterializer(MaterializerBehavior.Stable);
        var pipeline = CreatePipeline(authority, materializer);

        var result = await pipeline.ExecuteAsync(
            CreateEdge(),
            CreateTraversal(),
            DomainTag,
            new LogicalExecutionFlags());

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeIdentityLeakageGuardUnit.LeakageCode, result.ErrorCode);
        Assert.Equal(4, materializer.CallCount);
        Assert.Equal(9, authority.CallCount);
        Assert.Equal(
            Enumerable.Repeat(RunId, 8)
                .Append(IdentityControlValue(2)),
            authority.RunIds);
    }

    [Theory]
    [InlineData(MaterializerBehavior.NullSuccess)]
    [InlineData(MaterializerBehavior.Failure)]
    public async Task IdentityGuard_MalformedMaterializationFailsClosed(MaterializerBehavior behavior)
    {
        var materializer = new ControlledMaterializer(behavior);
        var result = await CreateIdentityGuard(
                new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC),
                materializer)
            .ExecuteAsync(CreateIdentityGuardRequest(CreateIdentityProbeInput()));

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeIdentityLeakageGuardUnit.GuardFailedCode, result.ErrorCode);
    }

    [Theory]
    [InlineData(AuthorityBehavior.NullSuccess)]
    [InlineData(AuthorityBehavior.MalformedDecision)]
    [InlineData(AuthorityBehavior.MalformedEnvelope)]
    public async Task IdentityGuard_MalformedAuthorityEnvelopeOrDecisionFailsClosed(AuthorityBehavior behavior)
    {
        var result = await CreateIdentityGuard(
                new RecordingAuthorityUnit(behavior),
                new ControlledMaterializer(MaterializerBehavior.Stable))
            .ExecuteAsync(CreateIdentityGuardRequest(CreateIdentityProbeInput()));

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeIdentityLeakageGuardUnit.GuardFailedCode, result.ErrorCode);
    }

    [Fact]
    public async Task IdentityGuard_MissingDependenciesFailClosed()
    {
        var input = CreateIdentityGuardRequest(CreateIdentityProbeInput());
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var materializer = new ControlledMaterializer(MaterializerBehavior.Stable);

        var guards = new DynamicGraphEdgeIdentityLeakageGuardUnit[]
        {
            new(null, materializer, CreateDescriptor(), authority),
            new(CreateEdge(), null, CreateDescriptor(), authority),
            new(CreateEdge(), materializer, null, authority),
            new(CreateEdge(), materializer, CreateDescriptor(), null)
        };

        foreach (var guard in guards)
        {
            var result = await guard.ExecuteAsync(input);
            Assert.False(result.IsSuccess);
            Assert.Equal(DynamicGraphEdgeIdentityLeakageGuardUnit.GuardFailedCode, result.ErrorCode);
        }
    }

    [Fact]
    public async Task IdentityGuard_SuppliedCancellationReturnsCancelledWithoutAnyProbe()
    {
        var authority = new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC);
        var materializer = new ControlledMaterializer(MaterializerBehavior.Stable);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await CreateIdentityGuard(authority, materializer).ExecuteAsync(
            CreateIdentityGuardRequest(CreateIdentityProbeInput()),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeIdentityLeakageGuardUnit.CancelledCode, result.ErrorCode);
        Assert.Equal(0, materializer.CallCount);
        Assert.Equal(0, authority.CallCount);
    }

    [Fact]
    public async Task IdentityGuard_OrdinaryMaterializerExceptionFailsClosed()
    {
        var result = await CreateIdentityGuard(
                new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC),
                new ControlledMaterializer(MaterializerBehavior.OrdinaryException))
            .ExecuteAsync(CreateIdentityGuardRequest(CreateIdentityProbeInput()));

        Assert.False(result.IsSuccess);
        Assert.Equal(DynamicGraphEdgeIdentityLeakageGuardUnit.GuardFailedCode, result.ErrorCode);
    }

    [Fact]
    public async Task IdentityGuard_NestedFatalAndUncancelledCancellationEscape()
    {
        var fatalGuard = CreateIdentityGuard(
            new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC),
            new ControlledMaterializer(MaterializerBehavior.NestedFatal));
        var fatal = await Assert.ThrowsAsync<AggregateException>(() =>
            fatalGuard.ExecuteAsync(CreateIdentityGuardRequest(CreateIdentityProbeInput())));
        Assert.Contains(fatal.InnerExceptions, inner => inner is BadImageFormatException);

        var cancellationGuard = CreateIdentityGuard(
            new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC),
            new ControlledMaterializer(MaterializerBehavior.UncancelledCancellation));
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            cancellationGuard.ExecuteAsync(CreateIdentityGuardRequest(CreateIdentityProbeInput())));
    }

    [Fact]
    public async Task IdentityContractsOwnInputsAndReportGettersReturnDeepSnapshots()
    {
        var traversal = CreateTraversal();
        var normalizedInput = CreateMaterializedIdentityInput(traversal);
        var decision = CreateSemanticDecision(normalizedInput.Candidates);
        var probeInput = new DynamicGraphEdgeIdentityProbeInput(traversal, normalizedInput, decision);

        traversal.Source.CompiledPredicateIds[0] = "MUTATED";
        normalizedInput.Candidates[0].EvidencePredicates[0] = "MUTATED";
        ((string[])decision.AblationBaselineRank)[0] = "MUTATED";

        var result = await CreateIdentityGuard(
                new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC),
                new ControlledMaterializer(MaterializerBehavior.Stable))
            .ExecuteAsync(CreateIdentityGuardRequest(probeInput));

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var first = result.Value!.Output;
        var second = result.Value.Output;
        Assert.NotSame(first.Probes, second.Probes);
        Assert.NotSame(first.Probes[0], second.Probes[0]);
        Assert.NotSame(first.Probes[0].Decision.OrderedTargets, second.Probes[0].Decision.OrderedTargets);
        Assert.NotSame(first.ExcludedLeakageSignals, second.ExcludedLeakageSignals);
        Assert.Equal(LeakageSignals, second.ExcludedLeakageSignals);
        Assert.Equal("target-c", Assert.Single(second.Probes[0].Decision.OrderedTargets).TargetSubPhaseId);
        Assert.DoesNotContain("MUTATED", probeInput.TraversalInput.Source.CompiledPredicateIds);
        Assert.DoesNotContain(
            probeInput.NormalizedInput.Candidates.SelectMany(candidate => candidate.EvidencePredicates),
            value => StringComparer.Ordinal.Equals(value, "MUTATED"));
    }

    [Theory]
    [InlineData("envelope-id")]
    [InlineData("probe-id")]
    [InlineData("probe-order")]
    [InlineData("candidate-omission")]
    [InlineData("candidate-mutation")]
    [InlineData("decision-mismatch")]
    public async Task PipelineGuardValidator_RejectsForgedSuccessfulEnvelopeAndReport(string mutation)
    {
        var candidates = CreateNormalizedCandidates();
        var normalizedInput = CreateNormalizedInput(candidates);
        var canonicalDecision = CreateSemanticDecision(candidates);
        var guardResult = await CreateGuard(new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC))
            .ExecuteAsync(CreateGuardRequest(new DynamicGraphEdgeCandidateOrderProbeInput(
                normalizedInput,
                canonicalDecision)));
        Assert.True(guardResult.IsSuccess, guardResult.ErrorMessage);

        var forged = ForgeGuardResult(guardResult, mutation);

        Assert.True(InvokeGuardValidator(
            guardResult,
            normalizedInput,
            canonicalDecision,
            CreateDescriptor(),
            DomainTag));
        Assert.False(InvokeGuardValidator(
            forged,
            normalizedInput,
            canonicalDecision,
            CreateDescriptor(),
            DomainTag));
    }

    [Theory]
    [InlineData("envelope-id")]
    [InlineData("envelope-role")]
    [InlineData("envelope-domain")]
    [InlineData("envelope-confidence")]
    [InlineData("warning")]
    [InlineData("transition")]
    [InlineData("probe-order")]
    [InlineData("probe-id")]
    [InlineData("probe-signal")]
    [InlineData("probe-control")]
    [InlineData("synthetic-report")]
    [InlineData("invariant")]
    [InlineData("missing-probe")]
    [InlineData("decision")]
    [InlineData("exclusion")]
    public async Task IdentityGuardValidator_RejectsForgedEnvelopeAndReport(string mutation)
    {
        var traversal = CreateTraversal();
        var normalizedInput = CreateMaterializedIdentityInput(traversal);
        var canonicalDecision = CreateSemanticDecision(normalizedInput.Candidates);
        var guardResult = await CreateIdentityGuard(
                new RecordingAuthorityUnit(AuthorityBehavior.SemanticTargetC),
                new ControlledMaterializer(MaterializerBehavior.Stable))
            .ExecuteAsync(CreateIdentityGuardRequest(new DynamicGraphEdgeIdentityProbeInput(
                traversal,
                normalizedInput,
                canonicalDecision)));
        Assert.True(guardResult.IsSuccess, guardResult.ErrorMessage);

        var forged = ForgeIdentityGuardResult(guardResult, mutation);

        Assert.True(InvokeIdentityGuardValidator(
            guardResult,
            traversal,
            normalizedInput,
            canonicalDecision,
            CreateDescriptor(),
            DomainTag));
        Assert.False(InvokeIdentityGuardValidator(
            forged,
            traversal,
            normalizedInput,
            canonicalDecision,
            CreateDescriptor(),
            DomainTag));
    }

    private static DynamicGraphEdgeAuthorityExecutionPipeline CreatePipeline(
        RecordingAuthorityUnit authority,
        out CountingMaterializer materializer,
        int candidateCount = 3)
    {
        materializer = new CountingMaterializer();
        return new DynamicGraphEdgeAuthorityExecutionPipeline(
            new FixedCandidateProvider(candidateCount),
            materializer,
            new FixedResolver(CreateDescriptor(), authority));
    }

    private static DynamicGraphEdgeAuthorityExecutionPipeline CreatePipeline(
        RecordingAuthorityUnit authority,
        IDynamicGraphEdgeAuthorityInputMaterializer materializer,
        int candidateCount = 3)
        => new(
            new FixedCandidateProvider(candidateCount),
            materializer,
            new FixedResolver(CreateDescriptor(), authority));

    private static DynamicGraphEdgeCandidateOrderLeakageGuardUnit CreateGuard(
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision> authority)
        => new(CreateDescriptor(), authority);

    private static DynamicGraphEdgeIdentityLeakageGuardUnit CreateIdentityGuard(
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision> authority,
        IDynamicGraphEdgeAuthorityInputMaterializer materializer)
        => new(CreateEdge(), materializer, CreateDescriptor(), authority);

    private static LogicalUnitDescriptor CreateDescriptor()
        => new(
            UnitId,
            "edge-authority",
            DomainTag,
            "1.0.0",
            LogicalUnitTrustState.Trusted,
            IsLearned: true,
            typeof(EdgeAuthorityInput),
            typeof(EdgeAuthorityDecision),
            RegistrationOrder: 1);

    private static LogicalUnitRequest<DynamicGraphEdgeCandidateOrderProbeInput> CreateGuardRequest(
        DynamicGraphEdgeCandidateOrderProbeInput input)
        => new(
            RunId,
            DynamicGraphEdgeCandidateOrderLeakageGuardUnit.Id,
            DynamicGraphEdgeCandidateOrderLeakageGuardUnit.Role,
            DomainTag,
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>(StringComparer.Ordinal));

    private static DynamicGraphEdgeCandidateOrderProbeInput CreateProbeInput()
    {
        var candidates = CreateNormalizedCandidates();
        return new DynamicGraphEdgeCandidateOrderProbeInput(
            CreateNormalizedInput(candidates),
            CreateSemanticDecision(candidates));
    }

    private static LogicalUnitRequest<DynamicGraphEdgeIdentityProbeInput> CreateIdentityGuardRequest(
        DynamicGraphEdgeIdentityProbeInput input)
        => new(
            RunId,
            DynamicGraphEdgeIdentityLeakageGuardUnit.Id,
            DynamicGraphEdgeIdentityLeakageGuardUnit.Role,
            DomainTag,
            input,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>(StringComparer.Ordinal));

    private static DynamicGraphEdgeIdentityProbeInput CreateIdentityProbeInput()
    {
        var traversal = CreateTraversal();
        var normalizedInput = CreateMaterializedIdentityInput(traversal);
        return new DynamicGraphEdgeIdentityProbeInput(
            traversal,
            normalizedInput,
            CreateSemanticDecision(normalizedInput.Candidates));
    }

    private static EdgeAuthorityInput CreateMaterializedIdentityInput(EdgeTraversalInput traversal)
    {
        var candidates = Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"predicate-{(char)('a' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();
        var result = new DeterministicDynamicGraphEdgeAuthorityInputMaterializer().Materialize(
            CreateEdge(),
            traversal,
            candidates);
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsType<EdgeAuthorityInput>(result.Value);
    }

    private static EdgeAuthorityInput CreateNormalizedInput(IReadOnlyList<EdgeCandidate> candidates)
        => new(
            "edge-candidate-order",
            ["candidate", "edge", "semantic"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["sub-phase-complete"],
                ["knowledge-edge-active"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            candidates,
            LeakageSignals.ToArray());

    private static EdgeCandidate[] CreateNormalizedCandidates(int count = 3)
        => Enumerable.Range(0, count)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"PREDICATE_{(char)('A' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();

    private static EdgeAuthorityDecision CreateSemanticDecision(IReadOnlyList<EdgeCandidate> candidates)
        => new(
            [(candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .First()
                .TargetSubPhaseId, 0.93d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray());

    private static DynamicGraphEdgeCandidateOrderLeakageReport CreateCandidateOrderLeakageReport(
        IReadOnlyList<EdgeCandidate> candidates,
        EdgeAuthorityDecision decision)
        => new(
            "edge-candidate-order",
            UnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreateExpectedPermutation(candidates, index),
                    decision))
                .ToArray(),
            LeakageSignals);

    private static DynamicGraphEdgeIdentityLeakageReport CreateIdentityLeakageReport(
        EdgeAuthorityDecision decision)
        => new(
            "edge-candidate-order",
            UnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    decision))
                .ToArray(),
            LeakageSignals);

    private static DynamicGraphEdgeAuthorityEvidence CreateAuthorityEvidence(
        IReadOnlyList<string>? exclusions = null)
        => new(
            "edge-candidate-order",
            UnitId,
            "L2",
            false,
            DeterministicRegistryEdgeCandidateProvider.Id,
            "candidate_generator",
            exclusions ?? LeakageSignals,
            "source-proof",
            "cutover-proof",
            "ablation-proof");

    private static IReadOnlyList<EdgeCandidate> CreateExpectedPermutation(
        IReadOnlyList<EdgeCandidate> candidates,
        int index)
    {
        int[] order = index switch
        {
            0 => [0, 1, 2],
            1 => [1, 2, 0],
            2 => [2, 0, 1],
            3 => [2, 1, 0],
            4 => [1, 0, 2],
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        return order.Select(candidateIndex => candidates[candidateIndex]).ToArray();
    }

    private static void AssertDecisionEquals(
        EdgeAuthorityDecision expected,
        EdgeAuthorityDecision actual)
    {
        Assert.Equal(expected.OrderedTargets, actual.OrderedTargets);
        Assert.Equal(expected.FanOutCount, actual.FanOutCount);
        Assert.Equal(expected.KnowledgeEdge, actual.KnowledgeEdge);
        Assert.Equal(expected.AuthoritySource, actual.AuthoritySource, StringComparer.Ordinal);
        Assert.Equal(expected.AblationBaselineRank, actual.AblationBaselineRank);
    }

    private static TestEdge CreateEdge(int candidateCount = 3)
        => new(
            "edge-candidate-order",
            "source-node",
            Enumerable.Range(0, candidateCount).Select(index => $"target-{(char)('a' + index)}").ToArray(),
            GraphEdgeKind.FanOut,
            ParallelEligible: true,
            UnitId,
            "L2",
            DeterministicFinalAuthority: false,
            DeterministicRegistryEdgeCandidateProvider.Id,
            "candidate_generator",
            LeakageSignals,
            "source-proof",
            "cutover-proof",
            "ablation-proof");

    private static EdgeTraversalInput CreateTraversal(int candidateCount = 3)
        => new(
            RunId,
            "edge-candidate-order",
            new SubPhaseRouterSourceDocument(
                "document-candidate-order",
                "source://candidate-order",
                "internal",
                "owner-approved",
                "plan",
                "Candidate order guard",
                "Semantic target C requires candidate order invariance.",
                Enumerable.Range(0, candidateCount).Select(index => $"predicate-{(char)('a' + index)}").ToArray(),
                "accepted",
                string.Empty,
                DomainTag,
                []),
            new ContextSignals(
                [Domain.CODE_Q],
                ["sub-phase-complete"],
                ["knowledge-edge-active"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            new NodeOutputToEdgeInputAdapter(
                "source-node",
                Enumerable.Range(0, candidateCount).Select(index => $"candidate-{(char)('a' + index)}").ToArray(),
                "sha256:candidate-order-source",
                "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)"));

    private static void AssertTraversalEqualWithControls(
        EdgeTraversalInput expected,
        EdgeTraversalInput actual,
        string expectedDocumentId,
        string expectedSourcePath,
        string expectedRunId)
    {
        Assert.Equal(expectedRunId, actual.RunId);
        Assert.Equal(expected.EdgeId, actual.EdgeId);

        Assert.Equal(expectedDocumentId, actual.Source.DocumentId);
        Assert.Equal(expectedSourcePath, actual.Source.SourcePath);
        Assert.Equal(expected.Source.SourceTier, actual.Source.SourceTier);
        Assert.Equal(expected.Source.LicenseOrAuthority, actual.Source.LicenseOrAuthority);
        Assert.Equal(expected.Source.DocumentType, actual.Source.DocumentType);
        Assert.Equal(expected.Source.DocumentTitle, actual.Source.DocumentTitle);
        Assert.Equal(expected.Source.CompiledText, actual.Source.CompiledText);
        Assert.Equal(expected.Source.CompiledPredicateIds, actual.Source.CompiledPredicateIds);
        Assert.Equal(expected.Source.SourceStatus, actual.Source.SourceStatus);
        Assert.Equal(expected.Source.KnownFailureMode, actual.Source.KnownFailureMode);
        Assert.Equal(expected.Source.DomainLabel, actual.Source.DomainLabel);
        Assert.Equal(expected.Source.SegmentDocuments, actual.Source.SegmentDocuments);

        Assert.Equal(expected.Context.PriorDomains, actual.Context.PriorDomains);
        Assert.Equal(expected.Context.CompletedSubPhases, actual.Context.CompletedSubPhases);
        Assert.Equal(expected.Context.ActiveKnowledgeEdges, actual.Context.ActiveKnowledgeEdges);
        Assert.Equal(
            expected.Context.PriorConfidenceMap.OrderBy(pair => pair.Key),
            actual.Context.PriorConfidenceMap.OrderBy(pair => pair.Key));

        Assert.Equal(expected.UpstreamNodeOutput.SourceNodeId, actual.UpstreamNodeOutput.SourceNodeId);
        Assert.Equal(
            expected.UpstreamNodeOutput.UpstreamCandidateRefs,
            actual.UpstreamNodeOutput.UpstreamCandidateRefs);
        Assert.Equal(expected.UpstreamNodeOutput.UpstreamOutputHash, actual.UpstreamNodeOutput.UpstreamOutputHash);
        Assert.Equal(expected.UpstreamNodeOutput.ProjectionRule, actual.UpstreamNodeOutput.ProjectionRule);
    }

    private static string IdentityControlValue(int index)
        => index switch
        {
            0 => "edge-authority-source-id-control",
            1 => "identity-control://edge-authority-source-path",
            2 => "edge-authority-correlation-id-control",
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };

    private static EdgeTraversalInput CopyTraversalForTest(EdgeTraversalInput source)
        => new(
            source.RunId,
            source.EdgeId,
            CopySourceForTest(source.Source),
            new ContextSignals(
                source.Context.PriorDomains.ToArray(),
                source.Context.CompletedSubPhases.ToArray(),
                source.Context.ActiveKnowledgeEdges.ToArray(),
                new Dictionary<Domain, double>(source.Context.PriorConfidenceMap)),
            new NodeOutputToEdgeInputAdapter(
                source.UpstreamNodeOutput.SourceNodeId,
                source.UpstreamNodeOutput.UpstreamCandidateRefs.ToArray(),
                source.UpstreamNodeOutput.UpstreamOutputHash,
                source.UpstreamNodeOutput.ProjectionRule));

    private static SubPhaseRouterSourceDocument CopySourceForTest(SubPhaseRouterSourceDocument source)
        => source with
        {
            CompiledPredicateIds = source.CompiledPredicateIds.ToArray(),
            SegmentDocuments = source.SegmentDocuments?.Select(CopySourceForTest).ToArray()
        };

    private sealed class FixedCandidateProvider(int candidateCount) : IEdgeCandidateProvider
    {
        public int CallCount { get; private set; }

        public OperationResult<EdgeCandidate[]> Propose(IDynamicGraphEdge edge, EdgeTraversalInput input)
        {
            CallCount++;
            return OperationResult<EdgeCandidate[]>.Ok(
                Enumerable.Range(0, candidateCount)
                    .Reverse()
                    .Select(index => new EdgeCandidate(
                        $"target-{(char)('a' + index)}",
                        [$"predicate-{(char)('a' + index)}"],
                        0.2d + (index * 0.2d),
                        RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
                    .ToArray());
        }
    }

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>> ForgeGuardResult(
        OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>> source,
        string mutation)
    {
        var envelope = source.Value!;
        if (mutation == "envelope-id")
        {
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>>.Ok(
                envelope with { UnitId = "forged-guard-id" });
        }

        var report = envelope.Output;
        var probes = report.Probes.ToArray();
        var original = probes[0];
        probes[0] = mutation switch
        {
            "probe-id" => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                "forged-probe-id",
                original.CandidateOrder,
                original.Decision),
            "probe-order" => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                original.ProbeId,
                original.CandidateOrder.Reverse().ToArray(),
                original.Decision),
            "candidate-omission" => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                original.ProbeId,
                original.CandidateOrder.SkipLast(1).ToArray(),
                original.Decision),
            "candidate-mutation" => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                original.ProbeId,
                original.CandidateOrder
                    .Select((candidate, index) => index == 0
                        ? candidate with { BaselineScore = candidate.BaselineScore + 1d }
                        : candidate)
                    .ToArray(),
                original.Decision),
            "decision-mismatch" => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                original.ProbeId,
                original.CandidateOrder,
                original.Decision with
                {
                    OrderedTargets = original.Decision.OrderedTargets
                        .Select(target => (target.TargetSubPhaseId, target.Confidence - 0.01d))
                        .ToArray()
                }),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation))
        };

        var forgedReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            report.EdgeId,
            report.ResolvedAuthorityUnitId,
            report.IsOrderInvariant,
            probes,
            report.ExcludedLeakageSignals);
        return OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>>.Ok(
            envelope with { Output = forgedReport });
    }

    private static bool InvokeGuardValidator(
        OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>> result,
        EdgeAuthorityInput normalizedInput,
        EdgeAuthorityDecision canonicalDecision,
        LogicalUnitDescriptor descriptor,
        string domainTag)
    {
        var method = typeof(DynamicGraphEdgeAuthorityExecutionPipeline).GetMethod(
            "TryValidateSuccessfulCandidateOrderGuardResult",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return Assert.IsType<bool>(method.Invoke(
            null,
            [result, normalizedInput, canonicalDecision, descriptor, domainTag]));
    }

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>> ForgeIdentityGuardResult(
        OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>> source,
        string mutation)
    {
        var envelope = source.Value!;
        if (mutation == "envelope-id")
        {
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Ok(
                envelope with { UnitId = "forged-identity-guard-id" });
        }

        if (mutation == "envelope-role")
        {
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Ok(
                envelope with { UnitRole = "forged-identity-guard-role" });
        }

        if (mutation == "envelope-domain")
        {
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Ok(
                envelope with { DomainTag = "forged-domain" });
        }

        if (mutation == "envelope-confidence")
        {
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Ok(
                envelope with { Confidence = 0.99d });
        }

        if (mutation == "warning")
        {
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Ok(
                envelope with { Warnings = ["forged-warning"] });
        }

        if (mutation == "transition")
        {
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Ok(
                envelope with
                {
                    TransitionSignal = new PhaseTransitionSignal(
                        Domain.CODE_Q,
                        "forged-transition",
                        "source-node",
                        "source-sub-phase",
                        ["forged-evidence"])
                });
        }

        var report = envelope.Output;
        var probes = report.Probes.ToArray();
        if (mutation == "probe-order")
            probes = probes.Reverse().ToArray();
        else if (mutation == "missing-probe")
            probes = probes.SkipLast(1).ToArray();
        else if (mutation is "probe-id" or "probe-signal" or "probe-control" or "decision")
        {
            var original = probes[0];
            probes[0] = new DynamicGraphEdgeIdentityControlProbe(
                mutation == "probe-id" ? "forged-probe-id" : original.ProbeId,
                mutation == "probe-signal" ? "forged-signal" : original.ControlledSignal,
                mutation == "probe-control" ? "forged-control" : original.ControlValue,
                mutation == "decision"
                    ? original.Decision with
                    {
                        OrderedTargets = original.Decision.OrderedTargets
                            .Select(target => (target.TargetSubPhaseId, target.Confidence - 0.01d))
                            .ToArray()
                    }
                    : original.Decision);
        }

        var forgedReport = new DynamicGraphEdgeIdentityLeakageReport(
            mutation == "synthetic-report" ? "synthetic-edge" : report.EdgeId,
            mutation == "synthetic-report" ? "synthetic-authority" : report.ResolvedAuthorityUnitId,
            mutation == "invariant" ? false : report.IsIdentityInvariant,
            probes,
            mutation == "exclusion"
                ? report.ExcludedLeakageSignals.Append("forged-exclusion").ToArray()
                : report.ExcludedLeakageSignals);
        return OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Ok(
            envelope with { Output = forgedReport });
    }

    private static bool InvokeIdentityGuardValidator(
        OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>> result,
        EdgeTraversalInput traversalInput,
        EdgeAuthorityInput normalizedInput,
        EdgeAuthorityDecision canonicalDecision,
        LogicalUnitDescriptor descriptor,
        string domainTag)
    {
        var method = typeof(DynamicGraphEdgeAuthorityExecutionPipeline).GetMethod(
            "TryValidateSuccessfulIdentityGuardResult",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return Assert.IsType<bool>(method.Invoke(
            null,
            [result, traversalInput, normalizedInput, canonicalDecision, descriptor, domainTag]));
    }

    private sealed class CountingMaterializer : IDynamicGraphEdgeAuthorityInputMaterializer
    {
        private readonly DeterministicDynamicGraphEdgeAuthorityInputMaterializer _inner = new();
        private readonly List<EdgeTraversalInput> _inputs = [];

        public int CallCount { get; private set; }

        public IReadOnlyList<EdgeTraversalInput> Inputs
            => _inputs
                .Select(CopyTraversalForTest)
                .ToArray();

        public OperationResult<EdgeAuthorityInput> Materialize(
            IDynamicGraphEdge edge,
            EdgeTraversalInput input,
            IReadOnlyList<EdgeCandidate> candidates)
        {
            CallCount++;
            _inputs.Add(CopyTraversalForTest(input));
            return _inner.Materialize(edge, input, candidates);
        }
    }

    private sealed class ControlledMaterializer(MaterializerBehavior behavior)
        : IDynamicGraphEdgeAuthorityInputMaterializer
    {
        private readonly DeterministicDynamicGraphEdgeAuthorityInputMaterializer _inner = new();

        public int CallCount { get; private set; }

        public OperationResult<EdgeAuthorityInput> Materialize(
            IDynamicGraphEdge edge,
            EdgeTraversalInput input,
            IReadOnlyList<EdgeCandidate> candidates)
        {
            CallCount++;
            if (behavior == MaterializerBehavior.OrdinaryException)
                throw new InvalidOperationException("ordinary materializer failure");

            if (behavior == MaterializerBehavior.NestedFatal)
            {
                throw new AggregateException(
                    new InvalidOperationException("ordinary sibling"),
                    new BadImageFormatException("nested fatal"));
            }

            if (behavior == MaterializerBehavior.UncancelledCancellation)
                throw new OperationCanceledException("uncancelled materializer cancellation");

            if (behavior == MaterializerBehavior.NullSuccess)
                return OperationResult<EdgeAuthorityInput>.Ok(null!);

            if (behavior == MaterializerBehavior.Failure)
            {
                return OperationResult<EdgeAuthorityInput>.Fail(
                    "MATERIALIZER_FAILURE",
                    "controlled materializer failure");
            }

            var result = _inner.Materialize(edge, input, candidates);
            if (!result.IsSuccess || result.Value is null)
                return result;

            var hasSourceIdControl = StringComparer.Ordinal.Equals(
                input.Source.DocumentId,
                IdentityControlValue(0));
            var hasSourcePathControl = StringComparer.Ordinal.Equals(
                input.Source.SourcePath,
                IdentityControlValue(1));
            if ((behavior == MaterializerBehavior.SourceIdSemanticLeakage && hasSourceIdControl)
                || (behavior == MaterializerBehavior.SourcePathSemanticLeakage && hasSourcePathControl))
            {
                var normalized = result.Value;
                return OperationResult<EdgeAuthorityInput>.Ok(
                    normalized with
                    {
                        SemanticTokens = normalized.SemanticTokens
                            .Append("identity-leakage-token")
                            .ToArray()
                    });
            }

            return result;
        }
    }

    private sealed class FixedResolver(
        LogicalUnitDescriptor descriptor,
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision> unit)
        : IDynamicGraphEdgeAuthorityResolver
    {
        public int CallCount { get; private set; }

        public OperationResult<ResolvedDynamicGraphEdgeAuthority> Resolve(
            IDynamicGraphEdge edge,
            string domainTag)
        {
            CallCount++;
            return OperationResult<ResolvedDynamicGraphEdgeAuthority>.Ok(new(descriptor, unit));
        }
    }

    private sealed class RecordingAuthorityUnit(AuthorityBehavior behavior)
        : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
    {
        private readonly List<string> _candidateOrders = [];
        private readonly List<string> _runIds = [];

        public int CallCount { get; private set; }

        public IReadOnlyList<string> CandidateOrders => _candidateOrders.ToArray();

        public IReadOnlyList<string> RunIds => _runIds.ToArray();

        public Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>> ExecuteAsync(
            LogicalUnitRequest<EdgeAuthorityInput> request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            _runIds.Add(request.RunId);
            _candidateOrders.Add(string.Join('|', request.Input.Candidates.Select(candidate => candidate.TargetSubPhaseId)));

            if (behavior == AuthorityBehavior.NestedFatal)
            {
                throw new AggregateException(
                    new InvalidOperationException("ordinary sibling"),
                    new BadImageFormatException("nested fatal"));
            }

            if (behavior == AuthorityBehavior.OrdinaryWrappedFatal)
            {
                throw new InvalidOperationException(
                    "ordinary wrapper",
                    new BadImageFormatException("nested fatal"));
            }

            if (behavior == AuthorityBehavior.UncancelledCancellation)
                throw new OperationCanceledException("uncancelled authority cancellation");

            if (behavior == AuthorityBehavior.NullSuccess)
            {
                return Task.FromResult(
                    OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Ok(null!));
            }

            var decision = behavior switch
            {
                AuthorityBehavior.FirstCandidate => new EdgeAuthorityDecision(
                    [(request.Input.Candidates[0].TargetSubPhaseId, 0.93d)],
                    1,
                    false,
                    RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
                    request.Input.Candidates
                        .OrderByDescending(candidate => candidate.BaselineScore)
                        .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                        .Select(candidate => candidate.TargetSubPhaseId)
                        .ToArray()),
                AuthorityBehavior.MalformedDecision => new EdgeAuthorityDecision(
                    [("missing-target", 0.93d)],
                    1,
                    false,
                    RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
                    request.Input.Candidates.Select(candidate => candidate.TargetSubPhaseId).ToArray()),
                AuthorityBehavior.RunIdSensitive when StringComparer.Ordinal.Equals(
                    request.RunId,
                    IdentityControlValue(2)) => new EdgeAuthorityDecision(
                        [(request.Input.Candidates[0].TargetSubPhaseId, 0.91d)],
                        1,
                        false,
                        RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
                        request.Input.Candidates
                            .OrderByDescending(candidate => candidate.BaselineScore)
                            .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                            .Select(candidate => candidate.TargetSubPhaseId)
                            .ToArray()),
                _ => CreateSemanticDecision(request.Input.Candidates)
            };
            var result = new LogicalUnitResult<EdgeAuthorityDecision>(
                request.UnitId,
                request.UnitRole,
                request.DomainTag,
                decision,
                0.93d,
                request.Input.PredicateIds.ToArray(),
                [request.DomainTag],
                ["candidate-order-authority-evidence"],
                []);
            if (behavior == AuthorityBehavior.MalformedEnvelope)
                result = result with { UnitId = "forged-authority-unit-id" };

            return Task.FromResult(OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>.Ok(result));
        }
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

    public enum AuthorityBehavior
    {
        SemanticTargetC,
        FirstCandidate,
        NullSuccess,
        MalformedDecision,
        MalformedEnvelope,
        RunIdSensitive,
        NestedFatal,
        OrdinaryWrappedFatal,
        UncancelledCancellation
    }

    public enum MaterializerBehavior
    {
        Stable,
        SourceIdSemanticLeakage,
        SourcePathSemanticLeakage,
        NullSuccess,
        Failure,
        OrdinaryException,
        NestedFatal,
        UncancelledCancellation
    }
}
