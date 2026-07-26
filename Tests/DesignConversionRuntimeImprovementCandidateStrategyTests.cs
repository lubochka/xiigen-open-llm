// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionRuntimeImprovementCandidateStrategyTests
{
    [Fact]
    public async Task CreateNextCandidateAsync_DecreaseSpacingFindingsEmitDecreasingFallbackDeclarations()
    {
        var result = await RunStrategyAsync(Finding(
            category: "spacing",
            description: "Reduce spacing; too much padding; gap too large.",
            suggestedAction: "Decrease the excessive spacing before the next render."));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var candidate = result.Value!;
        Assert.Empty(candidate.Metadata["visual_finding_fallback_conflicts"]);
        Assert.Contains("gap:0", candidate.Css, StringComparison.Ordinal);
        Assert.Contains("padding:0", candidate.Css, StringComparison.Ordinal);
        Assert.DoesNotContain("gap:12px", candidate.Css, StringComparison.Ordinal);
        Assert.DoesNotContain("padding:16px", candidate.Css, StringComparison.Ordinal);
        Assert.Equal("gap:decrease|padding:decrease", candidate.Metadata["visual_finding_fallback_intents"]);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_IncreaseSpacingFindingsEmitIncreasingFallbackDeclarations()
    {
        var result = await RunStrategyAsync(Finding(
            category: "spacing",
            description: "Not enough padding; elements are too close.",
            suggestedAction: "Increase spacing with bounded fallback targets."));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var candidate = result.Value!;
        Assert.Contains("gap:12px", candidate.Css, StringComparison.Ordinal);
        Assert.Contains("padding:16px", candidate.Css, StringComparison.Ordinal);
        Assert.Equal("gap:increase|padding:increase", candidate.Metadata["visual_finding_fallback_intents"]);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_ContradictorySamePropertyDirectionFailsClosed()
    {
        var result = await RunStrategyAsync(Finding(
            category: "gap",
            description: "Increase gap and reduce gap.",
            suggestedAction: "Conflicting gap direction must not produce fallback CSS."));

        Assert.False(result.IsSuccess);
        Assert.Equal("RUNTIME_IMPROVEMENT_CANDIDATE_UNCHANGED", result.ErrorCode);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_DeduplicatesOrdersAndBoundsFallbackDeclarations()
    {
        var result = await RunStrategyAsync(
            Finding(
                category: "spacing",
                description: "Not enough padding; elements are too close.",
                suggestedAction: "Increase spacing."),
            Finding(
                category: "padding",
                description: "Needs more padding.",
                suggestedAction: "Add padding."));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var candidate = result.Value!;
        var fallbackDeclarations = ExtractLoopImproverDeclarations(candidate.Css);
        Assert.Equal("gap:12px;padding:16px;", fallbackDeclarations);
        Assert.Equal(1, CountPropertyDeclarations(fallbackDeclarations, "gap"));
        Assert.Equal(1, CountPropertyDeclarations(fallbackDeclarations, "padding"));
        Assert.DoesNotContain("padding:32px", fallbackDeclarations, StringComparison.Ordinal);
        Assert.DoesNotContain("gap:24px", fallbackDeclarations, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_ContradictoryDirectionalPropertyKeepsNonDirectionalFallbacks()
    {
        var result = await RunStrategyAsync(Finding(
            category: "contrast",
            description: "Increase gap and reduce gap. Foreground contrast is low.",
            suggestedAction: "Improve contrast without changing conflicting gap direction."));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var candidate = result.Value!;
        var fallbackDeclarations = ExtractLoopImproverDeclarations(candidate.Css);
        Assert.Contains("background-color:#ffffff", fallbackDeclarations, StringComparison.Ordinal);
        Assert.Contains("color:#111827", fallbackDeclarations, StringComparison.Ordinal);
        Assert.Equal(0, CountPropertyDeclarations(fallbackDeclarations, "gap"));
        Assert.Equal("gap", candidate.Metadata["visual_finding_fallback_conflicts"]);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_InjectionShapedIssueCannotInjectRawCss()
    {
        const string injected = "PRIVATE_RAW_CSS_SECRET";
        var result = await RunStrategyAsync(Finding(
            category: "padding",
            description: $"Too much padding; color:red; background:url(http://evil/{injected}); padding:999px;",
            suggestedAction: $"Reduce padding only. {injected}"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var candidate = result.Value!;
        var fallbackDeclarations = ExtractLoopImproverDeclarations(candidate.Css);
        Assert.Equal("padding:0;", fallbackDeclarations);
        Assert.Contains("padding:0", candidate.Css, StringComparison.Ordinal);
        Assert.DoesNotContain("color:red", candidate.Css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("background:url", candidate.Css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("999px", candidate.Css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(injected, candidate.Css, StringComparison.Ordinal);
        Assert.DoesNotContain(injected, string.Join("|", candidate.ChangeReasons), StringComparison.Ordinal);
        Assert.DoesNotContain(injected, string.Join("|", candidate.Metadata.Values), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_EquivalentGeneratedFallbackBlockReturnsUnchanged()
    {
        var request = CreateRequest() with
        {
            Html = "<main class=\"screen dcm-loop-improved\"><section class=\"card\">Hello</section></main>",
            Css = ".screen{display:grid;padding:24px;gap:24px;}\n.dcm-loop-improved{gap:12px;padding:16px;}\n"
        };

        var result = await RunStrategyAsync(
            request,
            Finding(
                category: "spacing",
                description: "Not enough padding; elements are too close.",
                suggestedAction: "Increase spacing with bounded fallback targets."));

        Assert.False(result.IsSuccess);
        Assert.Equal("RUNTIME_IMPROVEMENT_CANDIDATE_UNCHANGED", result.ErrorCode);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_ChangedGeneratedFallbackBlockReplacesWithoutDuplicate()
    {
        var request = CreateRequest() with
        {
            Html = "<main class=\"screen dcm-loop-improved\"><section class=\"card\">Hello</section></main>",
            Css = ".screen{display:grid;padding:24px;gap:24px;}\n.dcm-loop-improved{padding:0;}\n"
        };

        var result = await RunStrategyAsync(
            request,
            Finding(
                category: "spacing",
                description: "Not enough padding; elements are too close.",
                suggestedAction: "Increase spacing with bounded fallback targets."));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var candidate = result.Value!;
        Assert.Equal(1, CountOccurrences(candidate.Css, ".dcm-loop-improved{"));
        Assert.Equal("gap:12px;padding:16px;", ExtractLoopImproverDeclarations(candidate.Css));
        Assert.DoesNotContain(".dcm-loop-improved{padding:0;}", candidate.Css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_RepeatedProducedFallbackStopsWithoutGrowthAndCoordinatorNoProgress()
    {
        const string privateMarker = "PRIVATE_FALLBACK_PAYLOAD";
        var initialRequest = CreateRequest();
        var increaseFinding = Finding(
            category: "spacing",
            description: $"Not enough padding; elements are too close. {privateMarker}",
            suggestedAction: $"Increase spacing with bounded fallback targets. {privateMarker}");

        var first = await RunStrategyAsync(initialRequest, completedAttemptNumber: 1, increaseFinding);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        var firstCandidate = first.Value!;
        Assert.Contains("attempt-2", firstCandidate.CandidateId, StringComparison.Ordinal);
        Assert.Contains("runtime-improvement:2", firstCandidate.RequestId, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(firstCandidate.Css, ".dcm-loop-improved{"));
        Assert.Equal(1, CountOccurrences(firstCandidate.Css, "gap:12px"));
        Assert.Equal(1, CountOccurrences(firstCandidate.Css, "padding:16px"));
        Assert.Equal("gap:12px;padding:16px;", ExtractLoopImproverDeclarations(firstCandidate.Css));
        Assert.DoesNotContain(privateMarker, firstCandidate.Css, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, string.Join("|", firstCandidate.ChangeReasons), StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, string.Join("|", firstCandidate.Metadata.Values), StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, string.Join("|", firstCandidate.EvidenceRefs), StringComparison.Ordinal);

        var producedRequest = ToRequest(initialRequest, firstCandidate, attemptNumber: 2);
        var producedCss = producedRequest.Css;
        var second = await RunStrategyAsync(producedRequest, completedAttemptNumber: 2, increaseFinding);
        var third = await RunStrategyAsync(producedRequest, completedAttemptNumber: 3, increaseFinding);

        Assert.False(second.IsSuccess);
        Assert.Equal(DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode, second.ErrorCode);
        Assert.False(third.IsSuccess);
        Assert.Equal(DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode, third.ErrorCode);
        Assert.Equal(producedCss, producedRequest.Css);
        Assert.Equal(producedCss.Length, producedRequest.Css.Length);
        Assert.Equal(1, CountOccurrences(producedRequest.Css, ".dcm-loop-improved{"));
        Assert.Equal(1, CountOccurrences(producedRequest.Css, "gap:12px"));
        Assert.Equal(1, CountOccurrences(producedRequest.Css, "padding:16px"));

        var decrease = await RunStrategyAsync(
            producedRequest,
            completedAttemptNumber: 4,
            Finding(
                category: "spacing",
                description: "Reduce spacing; too much padding; gap too large.",
                suggestedAction: "Decrease the excessive spacing before the next render."));

        Assert.True(decrease.IsSuccess, decrease.ErrorMessage);
        var decreaseCandidate = decrease.Value!;
        Assert.Contains("attempt-5", decreaseCandidate.CandidateId, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(decreaseCandidate.Css, ".dcm-loop-improved{"));
        Assert.Equal("gap:0;padding:0;", ExtractLoopImproverDeclarations(decreaseCandidate.Css));
        Assert.DoesNotContain(".dcm-loop-improved{gap:12px;padding:16px;}", decreaseCandidate.Css, StringComparison.Ordinal);

        var evaluator = new ScoreImprovingFallbackEvaluator(increaseFinding);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new DesignConversionRuntimeImprovementCandidateStrategy(),
            utcNow: () => DateTimeOffset.UnixEpoch,
            runDeadline: TimeSpan.FromSeconds(30));

        var loop = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "runtime-direction-coordinator-loop",
            InitialEvaluationRequest: initialRequest,
            MaxAttempts: 5,
            Metadata: new Dictionary<string, string>()));

        Assert.True(loop.IsSuccess, loop.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, loop.Value!.StopReason);
        Assert.False(loop.Value.CandidateStrategyFailed);
        Assert.False(loop.Value.Completed);
        Assert.Equal(2, loop.Value.AttemptCount);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Equal(1, CountOccurrences(evaluator.Requests[1].Css, ".dcm-loop-improved{"));
        Assert.Equal("gap:12px;padding:16px;", ExtractLoopImproverDeclarations(evaluator.Requests[1].Css));
        Assert.Contains(
            $"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_NO_PROGRESS:{DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode}",
            loop.Value.Warnings);
        Assert.Equal(bool.TrueString, loop.Value.PublicMetadata["no_progress_stop"]);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_MalformedGeneratedFallbackBlockFailsClosed()
    {
        var request = CreateRequest() with
        {
            Css = ".screen{display:grid;padding:24px;gap:24px;}\n.dcm-loop-improved{padding:0;"
        };

        var result = await RunStrategyAsync(
            request,
            Finding(
                category: "spacing",
                description: "Not enough padding; elements are too close.",
                suggestedAction: "Increase spacing with bounded fallback targets."));

        Assert.False(result.IsSuccess);
        Assert.Equal("RUNTIME_IMPROVEMENT_GENERATED_BLOCK_AMBIGUOUS", result.ErrorCode);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_MultipleGeneratedFallbackBlocksFailClosed()
    {
        var request = CreateRequest() with
        {
            Css = ".screen{display:grid;padding:24px;gap:24px;}\n.dcm-loop-improved{padding:0;}\n.dcm-loop-improved{gap:0;}\n"
        };

        var result = await RunStrategyAsync(
            request,
            Finding(
                category: "spacing",
                description: "Not enough padding; elements are too close.",
                suggestedAction: "Increase spacing with bounded fallback targets."));

        Assert.False(result.IsSuccess);
        Assert.Equal("RUNTIME_IMPROVEMENT_GENERATED_BLOCK_AMBIGUOUS", result.ErrorCode);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_UserAuthoredLoopImproverSelectorFailsClosed()
    {
        var request = CreateRequest() with
        {
            Css = ".screen{display:grid;padding:24px;gap:24px;}\n.dcm-loop-improved { color:red; }\n"
        };

        var result = await RunStrategyAsync(
            request,
            Finding(
                category: "spacing",
                description: "Not enough padding; elements are too close.",
                suggestedAction: "Increase spacing with bounded fallback targets."));

        Assert.False(result.IsSuccess);
        Assert.Equal("RUNTIME_IMPROVEMENT_GENERATED_BLOCK_AMBIGUOUS", result.ErrorCode);
    }

    private static async Task<OperationResult<DesignConversionImprovementCandidate>> RunStrategyAsync(
        params DesignConversionVisualCorrectionFinding[] findings)
    {
        var request = CreateRequest();
        return await RunStrategyAsync(request, findings);
    }

    private static async Task<OperationResult<DesignConversionImprovementCandidate>> RunStrategyAsync(
        DesignConversionInternalEvaluateRequest request,
        params DesignConversionVisualCorrectionFinding[] findings)
        => await RunStrategyAsync(request, completedAttemptNumber: 1, findings);

    private static async Task<OperationResult<DesignConversionImprovementCandidate>> RunStrategyAsync(
        DesignConversionInternalEvaluateRequest request,
        int completedAttemptNumber,
        params DesignConversionVisualCorrectionFinding[] findings)
    {
        var evaluation = CreateEvaluation(request, findings);
        var strategy = new DesignConversionRuntimeImprovementCandidateStrategy();
        return await strategy.CreateNextCandidateAsync(new DesignConversionCandidateUpdateRequest(
            LoopId: "runtime-direction-loop",
            CompletedAttemptNumber: completedAttemptNumber,
            CurrentRequest: request,
            CurrentEvaluation: evaluation,
            TierPolicy: evaluation.VisualQaBoundaryTrace.TierPolicy,
            AttemptHistory: []));
    }

    private static DesignConversionVisualCorrectionFinding Finding(
        string category,
        string description,
        string suggestedAction)
        => new(
            FindingId: $"finding-{Guid.NewGuid():N}",
            Category: category,
            Severity: "minor",
            Description: description,
            SuggestedAction: suggestedAction,
            EvidenceImageIds: [],
            CanDriveRetry: true);

    private static DesignConversionInternalEvaluateRequest CreateRequest()
        => new(
            JobId: "runtime-direction-job",
            RequestId: "runtime-direction-request",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"screen\"><section class=\"card\">Hello</section></main>",
            Css: ".screen{display:grid;padding:24px;gap:24px;}",
            DesignTreePayload: Payload("tree-current"),
            OriginalTreePayload: Payload("tree-original"),
            Screenshots: [Artifact("source-shot", "screenshot")],
            DesignRenders: [Artifact("source-render", "design-render")],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "runtime-direction-user",
                Tier: "internal",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 1,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.9"
                }),
            RenderResolutions: [new RenderQaResolution("source-desktop", 1440, 900, 1.0)],
            AiTeacherEvidenceRefs: [],
            DeterministicEvidenceRefs: new Dictionary<string, string>(),
            Provenance: new Dictionary<string, string>());

    private static DesignConversionInternalEvaluationResult CreateEvaluation(
        DesignConversionInternalEvaluateRequest request,
        IReadOnlyList<DesignConversionVisualCorrectionFinding> findings)
    {
        var submitted = ToSubmittedRequest(request);
        var verdict = new VisualVerdict(
            VerdictId: "runtime-direction-verdict",
            Score: 0.72,
            Defects: [],
            EvidenceRefs: ["runtime-direction-verdict"],
            Passed: false);
        var candidate = new DesignConversionImprovementCandidate(
            CandidateId: "runtime-direction-current-candidate",
            RequestId: request.RequestId,
            Html: request.Html,
            Css: request.Css,
            ChangeReasons: ["runtime unchanged candidate"],
            EvidenceRefs: ["runtime-direction-candidate"],
            Metadata: new Dictionary<string, string>());
        var output = new DesignConversionImprovementOutput(
            OutputId: "runtime-direction-output",
            CandidateId: candidate.CandidateId,
            Html: request.Html,
            Css: request.Css,
            Verdict: verdict,
            QaResults: [],
            EvidenceRefs: ["runtime-direction-output"],
            Accepted: false);
        var runtimeFlow = new DesignConversionRuntimeFlowResult(
            Request: submitted,
            CriticOutput: new DesignConversionCriticUnitOutput(
                OutputId: "runtime-direction-critic",
                Verdict: verdict,
                QaEvidenceRefs: [],
                AiTeacherEvidenceRefs: [],
                DeterministicLogicClaimedAsLearning: false),
            ImproverOutput: new DesignConversionImproverUnitOutput(
                OutputId: "runtime-direction-improver",
                Candidate: candidate,
                Output: output,
                QaEvidenceRefs: [],
                AiTeacherEvidenceRefs: [],
                DeterministicLogicClaimedAsLearning: false),
            Candidate: candidate,
            Output: output,
            UnitExecutionOrder: ["critic", "improver"],
            EvidenceRefs: ["runtime-direction-flow"],
            DeterministicLogicClaimedAsLearning: false);

        return new DesignConversionInternalEvaluationResult(
            JobId: request.JobId,
            SubmittedRequest: submitted,
            RenderResults: [],
            ComparisonArtifacts: [],
            VisualJudgeResults: [],
            QaResults: [],
            RuntimeFlowResult: runtimeFlow,
            Verdict: verdict,
            ImprovedHtml: request.Html,
            ImprovedCss: request.Css,
            EvidenceRefs: ["runtime-direction-evaluation"],
            Warnings: [],
            ScaffoldMarkers: new Dictionary<string, string>(),
            DeterministicLogicClaimedAsLearning: false,
            VisualQaBoundaryTrace: CreateBoundary(findings));
    }

    private static DesignConversionInternalEvaluationResult CreateEvaluationWithQaScore(
        DesignConversionInternalEvaluateRequest request,
        IReadOnlyList<DesignConversionVisualCorrectionFinding> findings,
        double score)
    {
        var evaluation = CreateEvaluation(request, findings);
        var resolution = request.RenderResolutions[0];
        var expected = new RenderQaImageRecord(
            ImageId: $"{request.JobId}:expected",
            Label: "expected",
            Resolution: resolution,
            Uri: $"artifact://runtime-direction/{request.JobId}.expected.png",
            Sha256: $"sha256:{request.JobId}:expected",
            Metadata: new Dictionary<string, string>());
        var actual = new RenderQaImageRecord(
            ImageId: $"{request.JobId}:actual",
            Label: "actual",
            Resolution: resolution,
            Uri: $"artifact://runtime-direction/{request.JobId}.actual.png",
            Sha256: $"sha256:{request.JobId}:actual",
            Metadata: new Dictionary<string, string>());
        var verdict = new VisualVerdict(
            VerdictId: $"{request.JobId}:verdict",
            Score: score,
            Defects: [],
            EvidenceRefs: [$"verdict:{request.JobId}"],
            Passed: false);
        var qa = new RenderQaResultRecord(
            ResultId: $"{request.JobId}:qa",
            RequestId: request.RequestId,
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            Metrics: new Dictionary<string, double>
            {
                ["visual_distance"] = 1.0 - score
            },
            Warnings: []);

        return evaluation with
        {
            QaResults = [qa],
            Verdict = verdict,
            VisualQaBoundaryTrace = CreateBoundary(findings, score)
        };
    }

    private static DesignConversionInternalEvaluateRequest ToRequest(
        DesignConversionInternalEvaluateRequest current,
        DesignConversionImprovementCandidate candidate,
        int attemptNumber)
        => current with
        {
            JobId = $"{current.JobId}:loop-attempt:{attemptNumber}",
            RequestId = candidate.RequestId,
            Html = candidate.Html,
            Css = candidate.Css
        };

    private static int CountOccurrences(string value, string marker)
    {
        var count = 0;
        var start = 0;
        while (start < value.Length)
        {
            var index = value.IndexOf(marker, start, StringComparison.Ordinal);
            if (index < 0)
                return count;

            count++;
            start = index + marker.Length;
        }

        return count;
    }

    private static DesignConversionVisualQaBoundaryTrace CreateBoundary(
        IReadOnlyList<DesignConversionVisualCorrectionFinding> findings,
        double score = 0.72)
    {
        var tier = new DesignConversionTierPolicyDecision(
            Tier: "internal",
            RequiredScore: 0.9,
            BestScore: score,
            EstimatedCostUsd: 0.01m,
            BudgetUsd: 1.00m,
            RemainingBudgetUsd: 0.99m,
            AttemptCount: 1,
            MaxImprovementCandidates: 3,
            StopReason: DesignConversionVisualQaBoundaryPolicy.ContinueImprovement,
            ShouldContinue: true,
            EvidenceRefs: ["tier-policy"],
            Metadata: new Dictionary<string, string>());
        var decision = new DesignConversionVisualComparisonDecision(
            Decision: DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
            QualityScore: score,
            RequiredThreshold: 0.9,
            EstimatedCostUsd: 0.01m,
            BudgetUsd: 1.00m,
            RemainingBudgetUsd: 0.99m,
            CostBudgetMet: true,
            QualityThresholdMet: false,
            CanRetry: true,
            CanPersistTrainingCandidate: true,
            ProviderBoundaryStatus: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            ExternalAiVisualJudgeExecuted: false,
            CorrectionFindings: findings,
            EvidenceRefs: ["visual-decision"],
            Metadata: new Dictionary<string, string>());

        return new DesignConversionVisualQaBoundaryTrace(
            MultiResolutionRender: new DesignConversionViewportCoverage(
                IsMultiResolution: false,
                IsComplete: true,
                RequiredViewportIds: ["source-desktop"],
                ObservedViewportIds: ["source-desktop"],
                MissingViewportIds: [],
                EvidenceRefs: ["coverage"]),
            VisualJudgeStatus: "SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED",
            VisualJudgeProviderBoundary: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            ExternalAiVisualJudgeExecuted: false,
            TierPolicy: tier,
            VisualDecision: decision,
            ProductReady: false,
            TrainedFeatureClaimed: false,
            EvidenceRefs: ["boundary"],
            Warnings: []);
    }

    private static SubmittedDesignConversionRequest ToSubmittedRequest(DesignConversionInternalEvaluateRequest request)
        => new(
            RequestId: request.RequestId,
            SourceKind: request.SourceKind,
            Html: request.Html,
            Css: request.Css,
            DesignTreePayload: request.DesignTreePayload,
            OriginalTreePayload: request.OriginalTreePayload,
            Screenshots: request.Screenshots,
            DesignRenders: request.DesignRenders,
            Budget: request.Budget,
            Provenance: request.Provenance);

    private static DesignTreePayload Payload(string id)
        => new(
            PayloadId: id,
            Format: "figma-json",
            Content: "{\"type\":\"FRAME\"}",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static DesignRenderArtifact Artifact(string id, string kind)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: 1440,
            Height: 900,
            Uri: $"artifact://runtime-direction/{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static string ExtractLoopImproverDeclarations(string css)
    {
        const string blockPrefix = ".dcm-loop-improved{";
        var blockStart = css.IndexOf(blockPrefix, StringComparison.Ordinal);
        Assert.True(blockStart >= 0, "Expected loop improver CSS block.");

        var declarationsStart = blockStart + blockPrefix.Length;
        var declarationsEnd = css.IndexOf('}', declarationsStart);
        Assert.True(declarationsEnd > declarationsStart, "Expected loop improver CSS declarations.");

        return css[declarationsStart..declarationsEnd];
    }

    private static int CountPropertyDeclarations(string declarations, string property)
    {
        return declarations
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Count(declaration => declaration.StartsWith($"{property}:", StringComparison.Ordinal));
    }

    private sealed class ScoreImprovingFallbackEvaluator(
        params DesignConversionVisualCorrectionFinding[] findings) : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var score = Requests.Count == 1 ? 0.70 : 0.72;
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                CreateEvaluationWithQaScore(request, findings, score)));
        }
    }
}
