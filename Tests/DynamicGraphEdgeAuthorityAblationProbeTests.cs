// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections.ObjectModel;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphEdgeAuthorityAblationProbeTests
{
    private const string EdgeId = "edge-ablation-probe";
    private const string AuthorityUnitId = "edge-ablation-learned-unit";
    private const string DomainTag = "edge-ablation-domain";
    private const string InputMismatchCode =
        "EDGE_AUTHORITY_ABLATION_SUBJECT_BASELINE_INPUT_MISMATCH";
    private const string RankMismatchCode =
        "EDGE_AUTHORITY_ABLATION_BASELINE_RANK_MISMATCH";
    private const string BaselineCandidateFailedCode =
        "EDGE_AUTHORITY_ABLATION_BASELINE_CANDIDATE_FAILED";
    private const string BaselineMaterializationFailedCode =
        "EDGE_AUTHORITY_ABLATION_BASELINE_MATERIALIZATION_FAILED";
    private const string SubjectResultInvalidCode =
        "EDGE_AUTHORITY_ABLATION_SUBJECT_RESULT_INVALID";

    private static readonly string[] LeakageSignals =
        DynamicGraphEdgeAuthorityLeakageSignalVocabulary.CreateSnapshot();
    private static readonly string[] IdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] IdentitySignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] IdentityValues =
        [
            "edge-authority-source-id-control",
            "identity-control://edge-authority-source-path",
            "edge-authority-correlation-id-control"
        ];

    [Fact]
    public void PublicSurface_IsExactOwnedAndDirectInvocationClaimIsNotCallerSettable()
    {
        Assert.True(typeof(DynamicGraphEdgeAuthorityAblationProbeResult).IsSealed);
        var resultConstructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationProbeResult).GetConstructors());
        Assert.Equal(
            [
                ("Subject", typeof(EdgeAuthorityExecutionResult)),
                ("BaselineInput", typeof(EdgeAuthorityInput)),
                ("OrderedBaselineTargetIds", typeof(IReadOnlyList<string>)),
                ("AuthorityUnitExcludedFromDirectBaselineBranchId", typeof(string))
            ],
            resultConstructor.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());

        var resultProperties = typeof(DynamicGraphEdgeAuthorityAblationProbeResult)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        Assert.Equal(
            [
                ("Subject", typeof(EdgeAuthorityExecutionResult), false),
                ("BaselineInput", typeof(EdgeAuthorityInput), false),
                ("OrderedBaselineTargetIds", typeof(IReadOnlyList<string>), false),
                ("AuthorityUnitExcludedFromDirectBaselineBranchId", typeof(string), false),
                ("ProbeDirectlyInvokedLearnedAuthorityForBaseline", typeof(bool), false)
            ],
            resultProperties);
        Assert.DoesNotContain(
            resultConstructor.GetParameters(),
            parameter => parameter.ParameterType == typeof(bool));
        Assert.True(typeof(DynamicGraphEdgeAuthorityAblationProbe).IsSealed);
        var probeConstructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationProbe).GetConstructors());
        Assert.Equal(
            [
                typeof(IDynamicGraphEdgeAuthorityExecutionPipeline),
                typeof(IEdgeCandidateProvider),
                typeof(IDynamicGraphEdgeAuthorityInputMaterializer)
            ],
            probeConstructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());

        var method = Assert.Single(typeof(DynamicGraphEdgeAuthorityAblationProbe)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(nameof(DynamicGraphEdgeAuthorityAblationProbe.RunAsync), method.Name);
        Assert.Equal(
            typeof(Task<OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>>),
            method.ReturnType);
        Assert.Equal(
            [
                typeof(IDynamicGraphEdge),
                typeof(EdgeTraversalInput),
                typeof(string),
                typeof(LogicalExecutionFlags),
                typeof(CancellationToken)
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());

        var dependencyFields = typeof(DynamicGraphEdgeAuthorityAblationProbe)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(field => field.FieldType)
            .ToArray();
        Assert.Equal(
            [
                typeof(IDynamicGraphEdgeAuthorityExecutionPipeline),
                typeof(IEdgeCandidateProvider),
                typeof(IDynamicGraphEdgeAuthorityInputMaterializer)
            ],
            dependencyFields);
        Assert.DoesNotContain(dependencyFields, type => type == typeof(IDynamicGraphEdgeAuthorityResolver));
        Assert.DoesNotContain(
            dependencyFields,
            type => type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(ILogicalUnit<,>));

        var claimSurface = string.Join(
            " ",
            resultProperties.Select(property => property.Name)
                .Concat(typeof(DynamicGraphEdgeAuthorityAblationProbe)
                    .GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(member => member.Name)));
        Assert.DoesNotContain("Accuracy", claimSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("Novelty", claimSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("Training", claimSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("Promotion", claimSurface, StringComparison.Ordinal);
        Assert.DoesNotContain("GraphMutation", claimSurface, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_LearnedSubjectAndIndependentBaselineChooseDifferentKnownTopTargets()
    {
        var harness = CreateHarness();

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var evidence = result.Value!;
        Assert.Equal("target-a", Assert.Single(evidence.Subject.Decision.OrderedTargets).TargetSubPhaseId);
        Assert.Equal(["target-b", "target-c", "target-a"], evidence.OrderedBaselineTargetIds);
        Assert.Equal(
            AuthorityUnitId,
            evidence.AuthorityUnitExcludedFromDirectBaselineBranchId,
            StringComparer.Ordinal);
        Assert.False(evidence.ProbeDirectlyInvokedLearnedAuthorityForBaseline);
        Assert.Equal(1, harness.SubjectPipeline.CallCount);
        Assert.Equal(1, harness.BaselineProvider.CallCount);
        Assert.Equal(1, harness.BaselineMaterializer.CallCount);
        AssertInputsEqual(evidence.Subject.NormalizedInput, evidence.BaselineInput);
    }

    [Fact]
    public async Task RunAsync_ShuffledEqualScoreCandidatesUseScoreThenOrdinalIdNotAnyInputOrder()
    {
        var candidates = new[]
        {
            Candidate("target-c", "PREDICATE_C", 0.40d),
            Candidate("target-b", "PREDICATE_B", 0.90d),
            Candidate("target-a", "PREDICATE_A", 0.90d)
        };
        var harness = CreateHarness(candidates, ["target-a", "target-b", "target-c"]);
        harness.Edge.ToNodeIds = Array.AsReadOnly(new[] { "target-b", "target-c", "target-a" });

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(["target-a", "target-b", "target-c"], result.Value!.OrderedBaselineTargetIds);
        Assert.Equal("target-a", Assert.Single(result.Value.Subject.Decision.OrderedTargets).TargetSubPhaseId);
        Assert.Equal(["target-c", "target-b", "target-a"], result.Value.BaselineInput.Candidates.Select(
            candidate => candidate.TargetSubPhaseId));
    }

    [Theory]
    [InlineData(BaselineMismatch.EdgeId)]
    [InlineData(BaselineMismatch.SemanticTokenCase)]
    [InlineData(BaselineMismatch.SemanticTokenOrder)]
    [InlineData(BaselineMismatch.SemanticTokenValue)]
    [InlineData(BaselineMismatch.PredicateOrder)]
    [InlineData(BaselineMismatch.PredicateValueAndEvidence)]
    [InlineData(BaselineMismatch.PriorDomainAndMap)]
    [InlineData(BaselineMismatch.CompletedCase)]
    [InlineData(BaselineMismatch.CompletedOrder)]
    [InlineData(BaselineMismatch.CompletedValue)]
    [InlineData(BaselineMismatch.ActiveEdgeCase)]
    [InlineData(BaselineMismatch.ActiveEdgeOrder)]
    [InlineData(BaselineMismatch.ActiveEdgeValue)]
    [InlineData(BaselineMismatch.ConfidenceMapValue)]
    [InlineData(BaselineMismatch.CandidateOrder)]
    [InlineData(BaselineMismatch.CandidateScore)]
    public async Task RunAsync_EachCanonicalDeepCorrelationMismatchFailsExactInputMismatch(
        BaselineMismatch mismatch)
    {
        var harness = CreateHarness();
        ApplyBaselineMismatch(harness, mismatch);

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        AssertFailure(
            result,
            InputMismatchCode,
            "Learned subject and independent baseline inputs do not match.");
    }

    [Fact]
    public async Task RunAsync_MaterializedCandidateRoleMismatchFailsMaterializationNotInputComparison()
    {
        var harness = CreateHarness();
        var input = harness.BaselineInput;
        var changed = input.Candidates.Select(CopyCandidate).ToArray();
        changed[0] = changed[0] with { AuthorityRole = "learned-authority" };
        harness.BaselineInput = CopyInput(input, candidates: changed);

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        AssertFailure(
            result,
            BaselineMaterializationFailedCode,
            "Independent baseline input materialization failed.");
    }

    [Fact]
    public async Task RunAsync_SubjectRankMismatchHasItsOwnFailureAfterIndependentCalculation()
    {
        var harness = CreateHarness(subjectBaselineRank: ["target-a", "target-b", "target-c"]);

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        AssertFailure(
            result,
            RankMismatchCode,
            "Learned subject ablation baseline rank does not match the independent baseline.");
        Assert.Equal(1, harness.BaselineProvider.CallCount);
        Assert.Equal(1, harness.BaselineMaterializer.CallCount);
    }

    [Fact]
    public async Task RunAsync_SubjectFailurePassesThroughExactlyAndNeverTraversesBaseline()
    {
        var harness = CreateHarness();
        harness.SubjectPipeline.Handler = (_, _, _, _, _) => Task.FromResult(
            OperationResult<EdgeAuthorityExecutionResult>.Fail(
                "distinctive subject failure",
                "DISTINCTIVE_SUBJECT_FAILURE"));
        harness.BaselineProvider.Handler = (_, _) => throw new InvalidOperationException(
            "baseline must not be traversed");

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        AssertFailure(result, "DISTINCTIVE_SUBJECT_FAILURE", "distinctive subject failure");
        Assert.Equal(1, harness.SubjectPipeline.CallCount);
        Assert.Equal(0, harness.BaselineProvider.CallCount);
        Assert.Equal(0, harness.BaselineMaterializer.CallCount);
    }

    [Theory]
    [InlineData(IdentityOriginal.DocumentId)]
    [InlineData(IdentityOriginal.SourcePath)]
    [InlineData(IdentityOriginal.RunId)]
    public async Task RunAsync_FixedIdentityControlEqualToOriginalFailsClosedBeforeBaseline(
        IdentityOriginal original)
    {
        var traversal = CreateTraversal(
            original == IdentityOriginal.RunId ? IdentityValues[2] : "edge-ablation-run",
            original == IdentityOriginal.DocumentId ? IdentityValues[0] : "edge-ablation-document",
            original == IdentityOriginal.SourcePath
                ? IdentityValues[1]
                : "repo://edge-ablation-source");
        var harness = CreateHarness(traversal: traversal);

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        AssertFailure(
            result,
            SubjectResultInvalidCode,
            "Learned edge authority subject result is invalid.");
        Assert.Equal(0, harness.BaselineProvider.CallCount);
        Assert.Equal(0, harness.BaselineMaterializer.CallCount);
    }

    [Fact]
    public async Task RunAsync_IdentityOriginalAdjacentToButDistinctFromControlRemainsValid()
    {
        var harness = CreateHarness(traversal: CreateTraversal(
            $"{IdentityValues[2]}-adjacent",
            $"{IdentityValues[0]}-adjacent",
            $"{IdentityValues[1]}-adjacent"));

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
    }

    [Theory]
    [InlineData(SubjectCorruption.ProposedVsNormalizedCandidate)]
    [InlineData(SubjectCorruption.NormalizedEdge)]
    [InlineData(SubjectCorruption.DecisionAuthoritySource)]
    [InlineData(SubjectCorruption.DecisionTargetMembership)]
    [InlineData(SubjectCorruption.AuthorityEvidenceEdge)]
    [InlineData(SubjectCorruption.AuthorityEvidenceUnit)]
    [InlineData(SubjectCorruption.OrderReportEdge)]
    [InlineData(SubjectCorruption.OrderReportUnit)]
    [InlineData(SubjectCorruption.OrderProbeId)]
    [InlineData(SubjectCorruption.OrderProbeOrder)]
    [InlineData(SubjectCorruption.OrderProbeDecision)]
    [InlineData(SubjectCorruption.IdentityReportEdge)]
    [InlineData(SubjectCorruption.IdentityReportUnit)]
    [InlineData(SubjectCorruption.IdentityControlledSignal)]
    [InlineData(SubjectCorruption.IdentityControlValue)]
    [InlineData(SubjectCorruption.IdentityProbeDecision)]
    [InlineData(SubjectCorruption.UnitConfidence)]
    [InlineData(SubjectCorruption.EvidenceString)]
    [InlineData(SubjectCorruption.WarningString)]
    public async Task RunAsync_EachSubjectValidatorFamilyFailsClosedBeforeHostileBaseline(
        SubjectCorruption corruption)
    {
        var harness = CreateHarness();
        var corrupted = CorruptSubject(harness.SubjectResult, corruption);
        harness.SubjectPipeline.Handler = (_, _, _, _, _) => Task.FromResult(
            OperationResult<EdgeAuthorityExecutionResult>.Ok(corrupted));
        harness.BaselineProvider.Handler = (_, _) => throw new InvalidOperationException(
            "baseline must not be traversed");

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        AssertFailure(
            result,
            SubjectResultInvalidCode,
            "Learned edge authority subject result is invalid.");
        Assert.Equal(0, harness.BaselineProvider.CallCount);
        Assert.Equal(0, harness.BaselineMaterializer.CallCount);
    }

    [Fact]
    public async Task RunAsync_NullSuccessfulSubjectResultFailsClosedBeforeBaseline()
    {
        var harness = CreateHarness();
        harness.SubjectPipeline.Handler = (_, _, _, _, _) => Task.FromResult(
            OperationResult<EdgeAuthorityExecutionResult>.Ok(null!));

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        AssertFailure(
            result,
            SubjectResultInvalidCode,
            "Learned edge authority subject result is invalid.");
        Assert.Equal(0, harness.BaselineProvider.CallCount);
    }

    [Fact]
    public async Task RunAsync_DirectInvalidEdgeAndTraversalUseExactStableCodes()
    {
        var harness = CreateHarness();
        harness.Edge.EdgeId = " invalid ";
        AssertFailure(
            await harness.Probe.RunAsync(
                harness.Edge,
                harness.Traversal,
                DomainTag,
                new LogicalExecutionFlags()),
            "EDGE_AUTHORITY_ABLATION_EDGE_INVALID",
            "Dynamic graph edge is invalid.");
        Assert.Equal(0, harness.SubjectPipeline.CallCount);

        harness = CreateHarness();
        var invalidTraversal = CreateTraversal(edgeId: "different-edge");
        AssertFailure(
            await harness.Probe.RunAsync(
                harness.Edge,
                invalidTraversal,
                DomainTag,
                new LogicalExecutionFlags()),
            "EDGE_AUTHORITY_ABLATION_TRAVERSAL_INVALID",
            "Edge traversal input is invalid.");
        Assert.Equal(0, harness.SubjectPipeline.CallCount);
    }

    [Theory]
    [InlineData(CancellationBoundary.AfterSubject)]
    [InlineData(CancellationBoundary.AfterProvider)]
    [InlineData(CancellationBoundary.AfterMaterializer)]
    public async Task RunAsync_RequestedCancellationStopsAtCompletedBoundary(
        CancellationBoundary boundary)
    {
        using var cts = new CancellationTokenSource();
        var harness = CreateHarness();
        if (boundary == CancellationBoundary.AfterSubject)
        {
            harness.SubjectPipeline.Handler = (_, _, _, _, _) =>
            {
                cts.Cancel();
                return Task.FromResult(
                    OperationResult<EdgeAuthorityExecutionResult>.Ok(harness.SubjectResult));
            };
        }
        else if (boundary == CancellationBoundary.AfterProvider)
        {
            harness.BaselineProvider.Handler = (_, _) =>
            {
                cts.Cancel();
                return OperationResult<EdgeCandidate[]>.Ok(
                    harness.BaselineInput.Candidates.Select(CopyCandidate).ToArray());
            };
        }
        else
        {
            harness.BaselineMaterializer.Handler = (_, _, _) =>
            {
                cts.Cancel();
                return OperationResult<EdgeAuthorityInput>.Ok(harness.BaselineInput);
            };
        }

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags(),
            cts.Token);

        AssertFailure(result, "CANCELLED", "Cancelled.");
        Assert.Equal(1, harness.SubjectPipeline.CallCount);
        Assert.Equal(boundary == CancellationBoundary.AfterSubject ? 0 : 1, harness.BaselineProvider.CallCount);
        Assert.Equal(boundary == CancellationBoundary.AfterMaterializer ? 1 : 0, harness.BaselineMaterializer.CallCount);
    }

    [Fact]
    public async Task RunAsync_DirectInvocationClaimDoesNotAttestToProviderInternals()
    {
        var harness = CreateHarness();
        var providerInternalLearnedSeamCalls = 0;
        harness.BaselineProvider.Handler = (_, _) =>
        {
            providerInternalLearnedSeamCalls++;
            return OperationResult<EdgeCandidate[]>.Ok(
                harness.BaselineInput.Candidates.Select(CopyCandidate).ToArray());
        };

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, providerInternalLearnedSeamCalls);
        Assert.False(result.Value!.ProbeDirectlyInvokedLearnedAuthorityForBaseline);
        Assert.DoesNotContain(
            typeof(DynamicGraphEdgeAuthorityAblationProbeResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.Name),
            name => name.Contains("Transitive", StringComparison.Ordinal)
                || name.Contains("ProviderInternal", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_CancellationPrecedesDependenciesAndDependenciesPrecedeArguments()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var noDependencies = new DynamicGraphEdgeAuthorityAblationProbe(null, null, null);
        AssertFailure(
            await noDependencies.RunAsync(null, null, null, null, cancelled.Token),
            "CANCELLED",
            "Cancelled.");

        AssertFailure(
            await noDependencies.RunAsync(null, null, null, null),
            "EDGE_AUTHORITY_ABLATION_SUBJECT_PIPELINE_REQUIRED",
            "Learned edge authority subject pipeline is required.");

        var subjectOnly = new DynamicGraphEdgeAuthorityAblationProbe(
            new DelegateSubjectPipeline(),
            null,
            null);
        AssertFailure(
            await subjectOnly.RunAsync(null, null, null, null),
            "EDGE_AUTHORITY_ABLATION_BASELINE_PROVIDER_REQUIRED",
            "Independent baseline candidate provider is required.");

        var twoDependencies = new DynamicGraphEdgeAuthorityAblationProbe(
            new DelegateSubjectPipeline(),
            new DelegateCandidateProvider(),
            null);
        AssertFailure(
            await twoDependencies.RunAsync(null, null, null, null),
            "EDGE_AUTHORITY_ABLATION_BASELINE_MATERIALIZER_REQUIRED",
            "Independent baseline input materializer is required.");
    }

    [Fact]
    public async Task RunAsync_ArgumentPrecedenceIsStableAndLearnedSubjectMustBeEnabled()
    {
        var harness = CreateHarness();
        AssertFailure(
            await harness.Probe.RunAsync(null, null, null, null),
            "EDGE_AUTHORITY_ABLATION_EDGE_REQUIRED",
            "Dynamic graph edge is required.");
        AssertFailure(
            await harness.Probe.RunAsync(harness.Edge, null, null, null),
            "EDGE_AUTHORITY_ABLATION_TRAVERSAL_REQUIRED",
            "Edge traversal input is required.");
        AssertFailure(
            await harness.Probe.RunAsync(harness.Edge, harness.Traversal, DomainTag, null),
            "EDGE_AUTHORITY_ABLATION_FLAGS_REQUIRED",
            "Logical execution flags are required.");
        AssertFailure(
            await harness.Probe.RunAsync(
                harness.Edge,
                harness.Traversal,
                " domain ",
                new LogicalExecutionFlags()),
            "EDGE_AUTHORITY_ABLATION_DOMAIN_INVALID",
            "Edge authority ablation domain is invalid.");
        AssertFailure(
            await harness.Probe.RunAsync(
                harness.Edge,
                harness.Traversal,
                DomainTag,
                new LogicalExecutionFlags { UseLearnedUnitWhenAvailable = false }),
            "EDGE_AUTHORITY_ABLATION_LEARNED_UNIT_DISABLED",
            "Learned edge authority subject must be enabled.");
    }

    [Fact]
    public async Task RunAsync_BaselineFailuresAreStableAndStopAtTheFailingBoundary()
    {
        var harness = CreateHarness();
        harness.BaselineProvider.Handler = (_, _) =>
            OperationResult<EdgeCandidate[]>.Fail("hostile detail", "HOSTILE_DETAIL");
        AssertFailure(
            await harness.Probe.RunAsync(
                harness.Edge,
                harness.Traversal,
                DomainTag,
                new LogicalExecutionFlags()),
            BaselineCandidateFailedCode,
            "Independent baseline candidate proposal failed.");
        Assert.Equal(0, harness.BaselineMaterializer.CallCount);

        harness = CreateHarness();
        harness.BaselineMaterializer.Handler = (_, _, _) =>
            OperationResult<EdgeAuthorityInput>.Fail("hostile detail", "HOSTILE_DETAIL");
        AssertFailure(
            await harness.Probe.RunAsync(
                harness.Edge,
                harness.Traversal,
                DomainTag,
                new LogicalExecutionFlags()),
            BaselineMaterializationFailedCode,
            "Independent baseline input materialization failed.");
    }

    [Theory]
    [InlineData(FailureBoundary.Subject)]
    [InlineData(FailureBoundary.Provider)]
    [InlineData(FailureBoundary.Materializer)]
    public async Task RunAsync_OrdinaryExceptionsAreMappedAtTheExactBoundary(FailureBoundary boundary)
    {
        var harness = CreateHarness();
        ConfigureThrow(harness, boundary, new InvalidOperationException("ordinary"));

        var result = await harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());

        var expected = boundary switch
        {
            FailureBoundary.Subject => (
                "EDGE_AUTHORITY_ABLATION_SUBJECT_EXECUTION_FAILED",
                "Learned edge authority subject execution failed."),
            FailureBoundary.Provider => (
                BaselineCandidateFailedCode,
                "Independent baseline candidate proposal failed."),
            _ => (
                BaselineMaterializationFailedCode,
                "Independent baseline input materialization failed.")
        };
        AssertFailure(result, expected.Item1, expected.Item2);
    }

    [Theory]
    [InlineData(FailureBoundary.Subject, FatalShape.Direct)]
    [InlineData(FailureBoundary.Subject, FatalShape.Nested)]
    [InlineData(FailureBoundary.Provider, FatalShape.Direct)]
    [InlineData(FailureBoundary.Provider, FatalShape.Nested)]
    [InlineData(FailureBoundary.Materializer, FatalShape.Direct)]
    [InlineData(FailureBoundary.Materializer, FatalShape.Nested)]
    public async Task RunAsync_FatalAndNestedFatalExceptionsEscape(
        FailureBoundary boundary,
        FatalShape shape)
    {
        var harness = CreateHarness();
        Exception exception = shape == FatalShape.Direct
            ? new OutOfMemoryException("fatal")
            : new InvalidOperationException("outer", new OutOfMemoryException("fatal"));
        ConfigureThrow(harness, boundary, exception);

        var thrown = await Assert.ThrowsAnyAsync<Exception>(() => harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags()));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task RunAsync_UnrequestedCancellationEscapesButRequestedCancellationMapsToCancelled()
    {
        var harness = CreateHarness();
        var unrequested = new OperationCanceledException("unrequested");
        ConfigureThrow(harness, FailureBoundary.Subject, unrequested);
        Assert.Same(
            unrequested,
            await Assert.ThrowsAsync<OperationCanceledException>(() => harness.Probe.RunAsync(
                harness.Edge,
                harness.Traversal,
                DomainTag,
                new LogicalExecutionFlags())));

        harness = CreateHarness();
        using var cts = new CancellationTokenSource();
        harness.SubjectPipeline.Handler = (_, _, _, _, _) =>
        {
            cts.Cancel();
            throw new OperationCanceledException(cts.Token);
        };
        AssertFailure(
            await harness.Probe.RunAsync(
                harness.Edge,
                harness.Traversal,
                DomainTag,
                new LogicalExecutionFlags(),
                cts.Token),
            "CANCELLED",
            "Cancelled.");
    }

    [Fact]
    public async Task RunAsync_CallerMutationAfterAwaitBoundaryCannotChangeEitherBranch()
    {
        var harness = CreateHarness();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        harness.SubjectPipeline.Handler = async (edge, input, domain, flags, token) =>
        {
            Assert.Equal(EdgeId, edge.EdgeId, StringComparer.Ordinal);
            Assert.Equal(EdgeId, input.EdgeId, StringComparer.Ordinal);
            Assert.Equal(DomainTag, domain, StringComparer.Ordinal);
            Assert.True(flags.UseLearnedUnitWhenAvailable);
            entered.SetResult();
            await release.Task.WaitAsync(token);
            return OperationResult<EdgeAuthorityExecutionResult>.Ok(harness.SubjectResult);
        };

        var run = harness.Probe.RunAsync(
            harness.Edge,
            harness.Traversal,
            DomainTag,
            new LogicalExecutionFlags());
        await entered.Task;

        harness.Edge.EdgeId = "mutated-edge";
        harness.Edge.ToNodeIds = Array.AsReadOnly(new[] { "mutated-target" });
        harness.Traversal.Source.CompiledPredicateIds[0] = "MUTATED_PREDICATE";
        harness.Traversal.Context.CompletedSubPhases[0] = "mutated-completed";
        ((List<string>)harness.Traversal.UpstreamNodeOutput.UpstreamCandidateRefs)[0] =
            "mutated-upstream";
        release.SetResult();

        var result = await run;
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(EdgeId, result.Value!.Subject.NormalizedInput.EdgeId, StringComparer.Ordinal);
        Assert.Equal(EdgeId, result.Value.BaselineInput.EdgeId, StringComparer.Ordinal);
        Assert.Equal(1, harness.BaselineProvider.CallCount);
        Assert.Equal(1, harness.BaselineMaterializer.CallCount);
        Assert.Equal(EdgeId, harness.BaselineProvider.LastEdgeId, StringComparer.Ordinal);
        Assert.Equal("completed-a", harness.BaselineProvider.LastCompletedSubPhase, StringComparer.Ordinal);
    }

    [Fact]
    public async Task RunAsync_ReturnedCarriersAreDeepOwnedAcrossEveryRead()
    {
        var harness = CreateHarness();
        var result = Assert.IsType<DynamicGraphEdgeAuthorityAblationProbeResult>(
            (await harness.Probe.RunAsync(
                harness.Edge,
                harness.Traversal,
                DomainTag,
                new LogicalExecutionFlags())).Value);

        var firstSubject = result.Subject;
        var firstBaseline = result.BaselineInput;
        var firstOrder = result.OrderedBaselineTargetIds;
        Assert.NotSame(firstSubject, result.Subject);
        Assert.NotSame(firstBaseline, result.BaselineInput);
        Assert.NotSame(firstOrder, result.OrderedBaselineTargetIds);
        Assert.IsType<ReadOnlyCollection<string>>(firstOrder);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)firstOrder)[0] = "mutated");

        firstSubject.NormalizedInput.SemanticTokens[0] = "mutated-subject";
        firstBaseline.SemanticTokens[0] = "mutated-baseline";
        firstBaseline.Candidates[0].EvidencePredicates[0] = "MUTATED_EVIDENCE";

        Assert.Equal("semantic-a", result.Subject.NormalizedInput.SemanticTokens[0]);
        Assert.Equal("semantic-a", result.BaselineInput.SemanticTokens[0]);
        Assert.Equal("PREDICATE_A", result.BaselineInput.Candidates[0].EvidencePredicates[0]);
        Assert.Equal(["target-b", "target-c", "target-a"], result.OrderedBaselineTargetIds);
        Assert.False(result.ProbeDirectlyInvokedLearnedAuthorityForBaseline);
    }

    private static Harness CreateHarness(
        EdgeCandidate[]? candidates = null,
        string[]? subjectBaselineRank = null,
        EdgeTraversalInput? traversal = null)
    {
        candidates ??=
        [
            Candidate("target-a", "PREDICATE_A", 0.10d),
            Candidate("target-b", "PREDICATE_B", 0.90d),
            Candidate("target-c", "PREDICATE_C", 0.50d)
        ];
        subjectBaselineRank ??= candidates
            .OrderByDescending(candidate => candidate.BaselineScore)
            .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();
        var edge = CreateEdge();
        traversal ??= CreateTraversal();
        var baselineInput = CreateInput(candidates);
        var subjectResult = CreateSubjectResult(candidates, subjectBaselineRank);
        var subject = new DelegateSubjectPipeline
        {
            Handler = (_, _, _, _, _) => Task.FromResult(
                OperationResult<EdgeAuthorityExecutionResult>.Ok(subjectResult))
        };
        var provider = new DelegateCandidateProvider
        {
            Handler = (_, _) => OperationResult<EdgeCandidate[]>.Ok(
                candidates.Select(CopyCandidate).ToArray())
        };
        var materializer = new DelegateInputMaterializer
        {
            Handler = (_, _, _) => OperationResult<EdgeAuthorityInput>.Ok(baselineInput)
        };
        return new Harness(edge, traversal, subjectResult, baselineInput, subject, provider, materializer);
    }

    private static MutableEdge CreateEdge()
        => new()
        {
            EdgeId = EdgeId,
            FromNodeId = "source-node",
            ToNodeIds = Array.AsReadOnly(new[] { "target-c", "target-a", "target-b" }),
            Kind = GraphEdgeKind.ManyToMany,
            ParallelEligible = true,
            AuthorityUnitId = AuthorityUnitId,
            AuthorityUnitLevel = "L2",
            DeterministicFinalAuthority = false,
            BaselineUnitId = "edge-ablation-baseline",
            BaselineRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
            ExcludedLeakageSignals = Array.AsReadOnly(LeakageSignals.ToArray()),
            SourceProof = "edge-ablation-source-proof",
            CutoverProof = "edge-ablation-cutover-proof",
            AblationProofRef = "edge-ablation-proof-ref"
        };

    private static EdgeTraversalInput CreateTraversal(
        string runId = "edge-ablation-run",
        string documentId = "edge-ablation-document",
        string sourcePath = "repo://edge-ablation-source",
        string edgeId = EdgeId)
        => new(
            runId,
            edgeId,
            new SubPhaseRouterSourceDocument(
                documentId,
                sourcePath,
                "internal",
                "AGPL-3.0-only",
                "design",
                "Edge ablation source",
                "edge authority ablation source text",
                ["PREDICATE_A", "PREDICATE_B", "PREDICATE_C"],
                "compiled",
                "none",
                DomainTag),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-a", "completed-b"],
                ["active-edge-a", "active-edge-b"],
                new ReadOnlyDictionary<Domain, double>(
                    new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.75d })),
            new NodeOutputToEdgeInputAdapter(
                "source-node",
                new List<string> { "candidate-ref-a", "candidate-ref-b" },
                "upstream-output-hash",
                "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)"));

    private static EdgeAuthorityInput CreateInput(IReadOnlyList<EdgeCandidate> candidates)
        => new(
            EdgeId,
            ["semantic-a", "semantic-b"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-a", "completed-b"],
                ["active-edge-a", "active-edge-b"],
                new ReadOnlyDictionary<Domain, double>(
                    new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.75d })),
            Array.AsReadOnly(candidates.Select(CopyCandidate).ToArray()),
            LeakageSignals.ToArray());

    private static EdgeAuthorityExecutionResult CreateSubjectResult(
        IReadOnlyList<EdgeCandidate> candidates,
        IReadOnlyList<string> baselineRank)
    {
        var decision = new EdgeAuthorityDecision(
            [("target-a", 0.97d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            Array.AsReadOnly(baselineRank.ToArray()));
        var input = CreateInput(candidates);
        var orderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            EdgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreatePermutation(input.Candidates, index),
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            EdgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentitySignals[index],
                    IdentityValues[index],
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        return new EdgeAuthorityExecutionResult(
            input,
            candidates.Select(CopyCandidate).ToArray(),
            decision,
            new DynamicGraphEdgeAuthorityEvidence(
                EdgeId,
                AuthorityUnitId,
                "L2",
                false,
                "edge-ablation-baseline",
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
                LeakageSignals,
                "edge-ablation-source-proof",
                "edge-ablation-cutover-proof",
                "edge-ablation-proof-ref"),
            orderReport,
            identityReport,
            0.97d,
            ["edge-ablation-subject-evidence"],
            []);
    }

    private static EdgeCandidate Candidate(string target, string predicate, double score)
        => new(
            target,
            [predicate],
            score,
            RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole);

    private static EdgeAuthorityExecutionResult CorruptSubject(
        EdgeAuthorityExecutionResult source,
        SubjectCorruption corruption)
    {
        var normalized = source.NormalizedInput;
        var proposed = source.ProposedCandidates.Select(CopyCandidate).ToArray();
        var decision = CopyDecision(source.Decision);
        var authorityEvidence = source.AuthorityEvidence;
        var orderReport = source.CandidateOrderLeakageReport;
        var identityReport = source.IdentityLeakageReport;
        var confidence = source.UnitConfidence;
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        switch (corruption)
        {
            case SubjectCorruption.ProposedVsNormalizedCandidate:
                proposed[0] = proposed[0] with { BaselineScore = 0.11d };
                break;
            case SubjectCorruption.NormalizedEdge:
                normalized = normalized with { EdgeId = "different-edge" };
                break;
            case SubjectCorruption.DecisionAuthoritySource:
                decision = new EdgeAuthorityDecision(
                    decision.OrderedTargets,
                    decision.FanOutCount,
                    decision.KnowledgeEdge,
                    "not-trained-authority",
                    decision.AblationBaselineRank);
                break;
            case SubjectCorruption.DecisionTargetMembership:
                decision = new EdgeAuthorityDecision(
                    [("not-an-edge-target", 0.97d)],
                    1,
                    false,
                    decision.AuthoritySource,
                    decision.AblationBaselineRank);
                break;
            case SubjectCorruption.AuthorityEvidenceEdge:
                authorityEvidence = CopyAuthorityEvidence(authorityEvidence, edgeId: "different-edge");
                break;
            case SubjectCorruption.AuthorityEvidenceUnit:
                authorityEvidence = CopyAuthorityEvidence(
                    authorityEvidence,
                    authorityUnitId: "different-unit");
                break;
            case SubjectCorruption.OrderReportEdge:
                orderReport = CopyOrderReport(orderReport, edgeId: "different-edge");
                break;
            case SubjectCorruption.OrderReportUnit:
                orderReport = CopyOrderReport(orderReport, authorityUnitId: "different-unit");
                break;
            case SubjectCorruption.OrderProbeId:
                {
                    var probes = orderReport.Probes.ToArray();
                    probes[0] = new DynamicGraphEdgeCandidateOrderPermutationProbe(
                        "different-probe",
                        probes[0].CandidateOrder,
                        probes[0].Decision);
                    orderReport = CopyOrderReport(orderReport, probes: probes);
                    break;
                }
            case SubjectCorruption.OrderProbeOrder:
                {
                    var probes = orderReport.Probes.ToArray();
                    probes[0] = new DynamicGraphEdgeCandidateOrderPermutationProbe(
                        probes[0].ProbeId,
                        probes[0].CandidateOrder.Reverse().ToArray(),
                        probes[0].Decision);
                    orderReport = CopyOrderReport(orderReport, probes: probes);
                    break;
                }
            case SubjectCorruption.OrderProbeDecision:
                {
                    var probes = orderReport.Probes.ToArray();
                    probes[0] = new DynamicGraphEdgeCandidateOrderPermutationProbe(
                        probes[0].ProbeId,
                        probes[0].CandidateOrder,
                        DecisionWithConfidence(probes[0].Decision, 0.96d));
                    orderReport = CopyOrderReport(orderReport, probes: probes);
                    break;
                }
            case SubjectCorruption.IdentityReportEdge:
                identityReport = CopyIdentityReport(identityReport, edgeId: "different-edge");
                break;
            case SubjectCorruption.IdentityReportUnit:
                identityReport = CopyIdentityReport(
                    identityReport,
                    authorityUnitId: "different-unit");
                break;
            case SubjectCorruption.IdentityControlledSignal:
                {
                    var probes = identityReport.Probes.ToArray();
                    probes[0] = new DynamicGraphEdgeIdentityControlProbe(
                        probes[0].ProbeId,
                        "different_signal",
                        probes[0].ControlValue,
                        probes[0].Decision);
                    identityReport = CopyIdentityReport(identityReport, probes: probes);
                    break;
                }
            case SubjectCorruption.IdentityControlValue:
                {
                    var probes = identityReport.Probes.ToArray();
                    probes[0] = new DynamicGraphEdgeIdentityControlProbe(
                        probes[0].ProbeId,
                        probes[0].ControlledSignal,
                        "different-control",
                        probes[0].Decision);
                    identityReport = CopyIdentityReport(identityReport, probes: probes);
                    break;
                }
            case SubjectCorruption.IdentityProbeDecision:
                {
                    var probes = identityReport.Probes.ToArray();
                    probes[0] = new DynamicGraphEdgeIdentityControlProbe(
                        probes[0].ProbeId,
                        probes[0].ControlledSignal,
                        probes[0].ControlValue,
                        DecisionWithConfidence(probes[0].Decision, 0.96d));
                    identityReport = CopyIdentityReport(identityReport, probes: probes);
                    break;
                }
            case SubjectCorruption.UnitConfidence:
                confidence = double.NaN;
                break;
            case SubjectCorruption.EvidenceString:
                evidenceIds = ["invalid\nevidence"];
                break;
            case SubjectCorruption.WarningString:
                warnings = ["invalid\nwarning"];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(corruption));
        }

        return new EdgeAuthorityExecutionResult(
            normalized,
            proposed,
            decision,
            authorityEvidence,
            orderReport,
            identityReport,
            confidence,
            evidenceIds,
            warnings);
    }

    private static DynamicGraphEdgeAuthorityEvidence CopyAuthorityEvidence(
        DynamicGraphEdgeAuthorityEvidence source,
        string? edgeId = null,
        string? authorityUnitId = null)
        => new(
            edgeId ?? source.EdgeId,
            authorityUnitId ?? source.AuthorityUnitId,
            source.AuthorityUnitLevel,
            source.DeterministicFinalAuthority,
            source.BaselineUnitId,
            source.BaselineRole,
            source.ExcludedLeakageSignals,
            source.SourceProof,
            source.CutoverProof,
            source.AblationProofRef);

    private static DynamicGraphEdgeCandidateOrderLeakageReport CopyOrderReport(
        DynamicGraphEdgeCandidateOrderLeakageReport source,
        string? edgeId = null,
        string? authorityUnitId = null,
        IReadOnlyList<DynamicGraphEdgeCandidateOrderPermutationProbe>? probes = null)
        => new(
            edgeId ?? source.EdgeId,
            authorityUnitId ?? source.ResolvedAuthorityUnitId,
            source.IsOrderInvariant,
            probes ?? source.Probes,
            source.ExcludedLeakageSignals);

    private static DynamicGraphEdgeIdentityLeakageReport CopyIdentityReport(
        DynamicGraphEdgeIdentityLeakageReport source,
        string? edgeId = null,
        string? authorityUnitId = null,
        IReadOnlyList<DynamicGraphEdgeIdentityControlProbe>? probes = null)
        => new(
            edgeId ?? source.EdgeId,
            authorityUnitId ?? source.ResolvedAuthorityUnitId,
            source.IsIdentityInvariant,
            probes ?? source.Probes,
            source.ExcludedLeakageSignals);

    private static EdgeAuthorityDecision DecisionWithConfidence(
        EdgeAuthorityDecision source,
        double confidence)
        => new(
            source.OrderedTargets.Select((target, index) =>
                index == 0
                    ? (target.TargetSubPhaseId, confidence)
                    : target).ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static void ApplyBaselineMismatch(Harness harness, BaselineMismatch mismatch)
    {
        var input = harness.BaselineInput;
        var candidates = input.Candidates.Select(CopyCandidate).ToArray();
        var semantic = input.SemanticTokens.ToArray();
        var predicates = input.PredicateIds.ToArray();
        var priorDomains = input.Context.PriorDomains.ToArray();
        var completed = input.Context.CompletedSubPhases.ToArray();
        var active = input.Context.ActiveKnowledgeEdges.ToArray();
        var confidence = input.Context.PriorConfidenceMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        var edgeId = input.EdgeId;

        switch (mismatch)
        {
            case BaselineMismatch.EdgeId:
                edgeId = "edge-ablation-probe-different";
                break;
            case BaselineMismatch.SemanticTokenCase:
                semantic[0] = "Semantic-a";
                break;
            case BaselineMismatch.SemanticTokenOrder:
                Array.Reverse(semantic);
                break;
            case BaselineMismatch.SemanticTokenValue:
                semantic[0] = "semantic-different";
                break;
            case BaselineMismatch.PredicateOrder:
                Array.Reverse(predicates);
                break;
            case BaselineMismatch.PredicateValueAndEvidence:
                predicates[0] = "PREDICATE_DIFFERENT";
                candidates[0] = candidates[0] with { EvidencePredicates = ["PREDICATE_DIFFERENT"] };
                break;
            case BaselineMismatch.PriorDomainAndMap:
                priorDomains = [Domain.BUG];
                confidence = new Dictionary<Domain, double> { [Domain.BUG] = 0.75d };
                break;
            case BaselineMismatch.CompletedCase:
                completed[0] = "Completed-a";
                break;
            case BaselineMismatch.CompletedOrder:
                Array.Reverse(completed);
                break;
            case BaselineMismatch.CompletedValue:
                completed[0] = "completed-different";
                break;
            case BaselineMismatch.ActiveEdgeCase:
                active[0] = "Active-edge-a";
                break;
            case BaselineMismatch.ActiveEdgeOrder:
                Array.Reverse(active);
                break;
            case BaselineMismatch.ActiveEdgeValue:
                active[0] = "active-edge-different";
                break;
            case BaselineMismatch.ConfidenceMapValue:
                confidence[Domain.CODE_Q] = 0.76d;
                break;
            case BaselineMismatch.CandidateOrder:
                Array.Reverse(candidates);
                break;
            case BaselineMismatch.CandidateScore:
                candidates[0] = candidates[0] with { BaselineScore = 0.11d };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mismatch));
        }

        var changed = new EdgeAuthorityInput(
            edgeId,
            semantic,
            predicates,
            new ContextSignals(
                priorDomains,
                completed,
                active,
                new ReadOnlyDictionary<Domain, double>(confidence)),
            Array.AsReadOnly(candidates.Select(CopyCandidate).ToArray()),
            input.ExcludedLeakageSignals.ToArray());
        harness.BaselineInput = changed;
        harness.BaselineCandidates = candidates;
    }

    private static EdgeAuthorityInput CopyInput(
        EdgeAuthorityInput source,
        IReadOnlyList<EdgeCandidate>? candidates = null)
        => new(
            source.EdgeId,
            source.SemanticTokens.ToArray(),
            source.PredicateIds.ToArray(),
            new ContextSignals(
                source.Context.PriorDomains.ToArray(),
                source.Context.CompletedSubPhases.ToArray(),
                source.Context.ActiveKnowledgeEdges.ToArray(),
                new ReadOnlyDictionary<Domain, double>(
                    source.Context.PriorConfidenceMap.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value))),
            Array.AsReadOnly((candidates ?? source.Candidates).Select(CopyCandidate).ToArray()),
            source.ExcludedLeakageSignals.ToArray());

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static EdgeCandidate[] CreatePermutation(
        IReadOnlyList<EdgeCandidate> candidates,
        int index)
    {
        var source = candidates.Select(CopyCandidate).ToArray();
        var offset = index switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => -1,
            4 => -2,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        if (offset < 0)
        {
            var reversed = source.Reverse().ToArray();
            return RotateLeft(reversed, -offset - 1);
        }

        return RotateLeft(source, offset);
    }

    private static EdgeCandidate[] RotateLeft(IReadOnlyList<EdgeCandidate> source, int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopyCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static void AssertInputsEqual(EdgeAuthorityInput expected, EdgeAuthorityInput actual)
    {
        Assert.Equal(expected.EdgeId, actual.EdgeId, StringComparer.Ordinal);
        Assert.Equal(expected.SemanticTokens, actual.SemanticTokens);
        Assert.Equal(expected.PredicateIds, actual.PredicateIds);
        Assert.Equal(expected.Context.PriorDomains, actual.Context.PriorDomains);
        Assert.Equal(expected.Context.CompletedSubPhases, actual.Context.CompletedSubPhases);
        Assert.Equal(expected.Context.ActiveKnowledgeEdges, actual.Context.ActiveKnowledgeEdges);
        Assert.Equal(expected.Context.PriorConfidenceMap, actual.Context.PriorConfidenceMap);
        Assert.Equal(expected.ExcludedLeakageSignals, actual.ExcludedLeakageSignals);
        Assert.Equal(
            expected.Candidates.Select(CandidateTuple),
            actual.Candidates.Select(CandidateTuple));
    }

    private static (string Target, string Evidence, double Score, string Role) CandidateTuple(
        EdgeCandidate candidate)
        => (
            candidate.TargetSubPhaseId,
            string.Join("|", candidate.EvidencePredicates),
            candidate.BaselineScore,
            candidate.AuthorityRole);

    private static void AssertFailure(
        OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(expectedMessage, result.ErrorMessage, StringComparer.Ordinal);
    }

    private static void ConfigureThrow(Harness harness, FailureBoundary boundary, Exception exception)
    {
        switch (boundary)
        {
            case FailureBoundary.Subject:
                harness.SubjectPipeline.Handler = (_, _, _, _, _) => throw exception;
                break;
            case FailureBoundary.Provider:
                harness.BaselineProvider.Handler = (_, _) => throw exception;
                break;
            case FailureBoundary.Materializer:
                harness.BaselineMaterializer.Handler = (_, _, _) => throw exception;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(boundary));
        }
    }

    public enum BaselineMismatch
    {
        EdgeId,
        SemanticTokenCase,
        SemanticTokenOrder,
        SemanticTokenValue,
        PredicateOrder,
        PredicateValueAndEvidence,
        PriorDomainAndMap,
        CompletedCase,
        CompletedOrder,
        CompletedValue,
        ActiveEdgeCase,
        ActiveEdgeOrder,
        ActiveEdgeValue,
        ConfidenceMapValue,
        CandidateOrder,
        CandidateScore
    }

    public enum FailureBoundary
    {
        Subject,
        Provider,
        Materializer
    }

    public enum FatalShape
    {
        Direct,
        Nested
    }

    public enum IdentityOriginal
    {
        DocumentId,
        SourcePath,
        RunId
    }

    public enum CancellationBoundary
    {
        AfterSubject,
        AfterProvider,
        AfterMaterializer
    }

    public enum SubjectCorruption
    {
        ProposedVsNormalizedCandidate,
        NormalizedEdge,
        DecisionAuthoritySource,
        DecisionTargetMembership,
        AuthorityEvidenceEdge,
        AuthorityEvidenceUnit,
        OrderReportEdge,
        OrderReportUnit,
        OrderProbeId,
        OrderProbeOrder,
        OrderProbeDecision,
        IdentityReportEdge,
        IdentityReportUnit,
        IdentityControlledSignal,
        IdentityControlValue,
        IdentityProbeDecision,
        UnitConfidence,
        EvidenceString,
        WarningString
    }

    private sealed class Harness
    {
        private EdgeCandidate[] _baselineCandidates;
        private EdgeAuthorityInput _baselineInput;

        internal Harness(
            MutableEdge edge,
            EdgeTraversalInput traversal,
            EdgeAuthorityExecutionResult subjectResult,
            EdgeAuthorityInput baselineInput,
            DelegateSubjectPipeline subjectPipeline,
            DelegateCandidateProvider baselineProvider,
            DelegateInputMaterializer baselineMaterializer)
        {
            Edge = edge;
            Traversal = traversal;
            SubjectResult = subjectResult;
            _baselineInput = baselineInput;
            _baselineCandidates = baselineInput.Candidates.Select(CopyCandidate).ToArray();
            SubjectPipeline = subjectPipeline;
            BaselineProvider = baselineProvider;
            BaselineMaterializer = baselineMaterializer;
            BaselineProvider.Handler = (edge, input) =>
            {
                BaselineProvider.LastEdgeId = edge.EdgeId;
                BaselineProvider.LastCompletedSubPhase = input.Context.CompletedSubPhases[0];
                return OperationResult<EdgeCandidate[]>.Ok(
                    _baselineCandidates.Select(CopyCandidate).ToArray());
            };
            BaselineMaterializer.Handler = (_, _, _) =>
                OperationResult<EdgeAuthorityInput>.Ok(_baselineInput);
            Probe = new DynamicGraphEdgeAuthorityAblationProbe(
                SubjectPipeline,
                BaselineProvider,
                BaselineMaterializer);
        }

        internal MutableEdge Edge { get; }

        internal EdgeTraversalInput Traversal { get; }

        internal EdgeAuthorityExecutionResult SubjectResult { get; }

        internal DelegateSubjectPipeline SubjectPipeline { get; }

        internal DelegateCandidateProvider BaselineProvider { get; }

        internal DelegateInputMaterializer BaselineMaterializer { get; }

        internal DynamicGraphEdgeAuthorityAblationProbe Probe { get; }

        internal EdgeAuthorityInput BaselineInput
        {
            get => _baselineInput;
            set => _baselineInput = value;
        }

        internal EdgeCandidate[] BaselineCandidates
        {
            set => _baselineCandidates = value.Select(CopyCandidate).ToArray();
        }
    }

    private sealed class DelegateSubjectPipeline : IDynamicGraphEdgeAuthorityExecutionPipeline
    {
        internal Func<
            IDynamicGraphEdge,
            EdgeTraversalInput,
            string,
            LogicalExecutionFlags,
            CancellationToken,
            Task<OperationResult<EdgeAuthorityExecutionResult>>>
            Handler
        { get; set; }
            = (_, _, _, _, _) => throw new InvalidOperationException("Subject handler is required.");

        internal int CallCount { get; private set; }

        public Task<OperationResult<EdgeAuthorityExecutionResult>> ExecuteAsync(
            IDynamicGraphEdge edge,
            EdgeTraversalInput input,
            string domainTag,
            LogicalExecutionFlags flags,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Handler(edge, input, domainTag, flags, cancellationToken);
        }
    }

    private sealed class DelegateCandidateProvider : IEdgeCandidateProvider
    {
        internal Func<IDynamicGraphEdge, EdgeTraversalInput, OperationResult<EdgeCandidate[]>>
            Handler
        { get; set; }
            = (_, _) => throw new InvalidOperationException("Provider handler is required.");

        internal int CallCount { get; private set; }

        internal string? LastEdgeId { get; set; }

        internal string? LastCompletedSubPhase { get; set; }

        public OperationResult<EdgeCandidate[]> Propose(
            IDynamicGraphEdge edge,
            EdgeTraversalInput input)
        {
            CallCount++;
            return Handler(edge, input);
        }
    }

    private sealed class DelegateInputMaterializer : IDynamicGraphEdgeAuthorityInputMaterializer
    {
        internal Func<
            IDynamicGraphEdge,
            EdgeTraversalInput,
            IReadOnlyList<EdgeCandidate>,
            OperationResult<EdgeAuthorityInput>>
            Handler
        { get; set; }
            = (_, _, _) => throw new InvalidOperationException("Materializer handler is required.");

        internal int CallCount { get; private set; }

        public OperationResult<EdgeAuthorityInput> Materialize(
            IDynamicGraphEdge edge,
            EdgeTraversalInput input,
            IReadOnlyList<EdgeCandidate> candidates)
        {
            CallCount++;
            return Handler(edge, input, candidates);
        }
    }

    private sealed class MutableEdge : IDynamicGraphEdge
    {
        public string EdgeId { get; set; } = "";

        public string FromNodeId { get; set; } = "";

        public IReadOnlyList<string> ToNodeIds { get; set; } = [];

        public GraphEdgeKind Kind { get; set; }

        public bool ParallelEligible { get; set; }

        public string AuthorityUnitId { get; set; } = "";

        public string AuthorityUnitLevel { get; set; } = "";

        public bool DeterministicFinalAuthority { get; set; }

        public string? BaselineUnitId { get; set; }

        public string BaselineRole { get; set; } = "";

        public IReadOnlyList<string> ExcludedLeakageSignals { get; set; } = [];

        public string SourceProof { get; set; } = "";

        public string CutoverProof { get; set; } = "";

        public string AblationProofRef { get; set; } = "";
    }
}
