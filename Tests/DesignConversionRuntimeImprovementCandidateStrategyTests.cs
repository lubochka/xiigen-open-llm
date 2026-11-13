// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionRuntimeImprovementCandidateStrategyTests
{
    [Fact]
    public async Task CreateNextCandidateAsync_UnchangedRuntimeOutputWithIncreaseSpacingFindingReturnsUnchangedWithoutMutation()
    {
        var request = CreateRequest();
        var originalHtml = request.Html;
        var originalCss = request.Css;

        var result = await RunStrategyAsync(request, Finding(
            category: "spacing",
            description: "Not enough padding; elements are too close.",
            suggestedAction: "Increase spacing with bounded fallback targets."));

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode, result.ErrorCode);
        Assert.Equal(originalHtml, request.Html);
        Assert.Equal(originalCss, request.Css);
        Assert.DoesNotContain("dcm-loop-improved", request.Html, StringComparison.Ordinal);
        Assert.DoesNotContain(".dcm-loop-improved", request.Css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateNextCandidateAsync_ChangedRuntimeOutputPassesThroughExactlyWithFallbackDisabledMetadata()
    {
        const string privateMarker = "PRIVATE_RUNTIME_FINDING_TEXT";
        var request = CreateRequest();
        var improvedHtml = "<main class=\"screen\"><section class=\"card card-spaced\">Hello</section></main>";
        var improvedCss = string.Empty;
        var finding = Finding(
            category: "spacing",
            description: $"Not enough padding; elements are too close. {privateMarker}",
            suggestedAction: $"Increase spacing, but do not expose raw text. {privateMarker}");
        var evaluation = CreateEvaluation(request, [finding], improvedHtml, improvedCss);

        var result = await RunStrategyAsync(request, evaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var candidate = result.Value!;
        Assert.Equal("runtime-direction-loop:attempt-2:runtime-improvement-candidate", candidate.CandidateId);
        Assert.Equal("runtime-direction-request:runtime-improvement:2", candidate.RequestId);
        Assert.Equal(improvedHtml, candidate.Html);
        Assert.Equal(improvedCss, candidate.Css);
        Assert.DoesNotContain("dcm-loop-improved", candidate.Html, StringComparison.Ordinal);
        Assert.DoesNotContain(".dcm-loop-improved", candidate.Css, StringComparison.Ordinal);
        Assert.Equal(bool.TrueString, candidate.Metadata["candidate_changed"]);
        Assert.Equal(bool.TrueString, candidate.Metadata["candidate_html_changed"]);
        Assert.Equal(bool.TrueString, candidate.Metadata["candidate_css_changed"]);
        Assert.Equal(bool.TrueString, candidate.Metadata["runtime_flow_candidate_used"]);
        Assert.Equal(bool.FalseString, candidate.Metadata["visual_finding_fallback_applied"]);
        Assert.Equal(string.Empty, candidate.Metadata["visual_finding_fallback_intents"]);
        Assert.Equal(string.Empty, candidate.Metadata["visual_finding_fallback_conflicts"]);
        Assert.Equal(bool.TrueString, candidate.Metadata["deterministic_visual_finding_fallback_disabled"]);
        Assert.Equal(bool.FalseString, candidate.Metadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, candidate.Metadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, candidate.Metadata["learned_improvement_claimed"]);
        Assert.Equal(bool.FalseString, candidate.Metadata["deterministic_logic_claimed_as_learning"]);
        Assert.Equal(bool.FalseString, candidate.Metadata["product_ready"]);
        Assert.Contains(DesignConversionRuntimeImprovementCandidateStrategy.StrategyMarker, candidate.EvidenceRefs);
        Assert.Contains(DesignConversionRuntimeImprovementCandidateStrategy.ChangedCandidateMarker, candidate.EvidenceRefs);
        Assert.Contains(DesignConversionRuntimeImprovementCandidateStrategy.NoExternalProviderMarker, candidate.EvidenceRefs);
        Assert.Contains("DETERMINISTIC_VISUAL_FINDING_FALLBACK_DISABLED_TRUE", candidate.EvidenceRefs);
        Assert.Contains(candidate.ChangeReasons, reason => reason.Contains("changed runtime-flow HTML/CSS bytes", StringComparison.Ordinal));
        Assert.DoesNotContain(candidate.ChangeReasons, reason => reason.Contains("Fallback", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(privateMarker, candidate.Css, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, string.Join("|", candidate.ChangeReasons), StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, string.Join("|", candidate.Metadata.Values), StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, string.Join("|", candidate.EvidenceRefs), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_UnchangedRuntimeOutputStopsAsNoProgressWithoutSynthesizingFallback()
    {
        var finding = Finding(
            category: "spacing",
            description: "Not enough padding; elements are too close.",
            suggestedAction: "Increase spacing with bounded fallback targets.");
        var evaluator = new UnchangedRuntimeEvaluator(finding);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new DesignConversionRuntimeImprovementCandidateStrategy(),
            utcNow: () => DateTimeOffset.UnixEpoch,
            runDeadline: TimeSpan.FromSeconds(30));

        var loop = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "runtime-direction-coordinator-loop",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 5,
            Metadata: new Dictionary<string, string>()));

        Assert.True(loop.IsSuccess, loop.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, loop.Value!.StopReason);
        Assert.False(loop.Value.CandidateStrategyFailed);
        Assert.False(loop.Value.Completed);
        Assert.Equal(1, loop.Value.AttemptCount);
        Assert.Single(evaluator.Requests);
        Assert.DoesNotContain("dcm-loop-improved", evaluator.Requests[0].Html, StringComparison.Ordinal);
        Assert.DoesNotContain(".dcm-loop-improved", evaluator.Requests[0].Css, StringComparison.Ordinal);
        Assert.Contains(
            $"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_NO_PROGRESS:{DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode}",
            loop.Value.Warnings);
        Assert.Equal(bool.TrueString, loop.Value.PublicMetadata["no_progress_stop"]);
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
        => await RunStrategyAsync(request, CreateEvaluation(request, findings));

    private static async Task<OperationResult<DesignConversionImprovementCandidate>> RunStrategyAsync(
        DesignConversionInternalEvaluateRequest request,
        DesignConversionInternalEvaluationResult evaluation,
        int completedAttemptNumber = 1)
    {
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
        IReadOnlyList<DesignConversionVisualCorrectionFinding> findings,
        string? improvedHtml = null,
        string? improvedCss = null)
    {
        var submitted = ToSubmittedRequest(request);
        var candidateHtml = improvedHtml ?? request.Html;
        var candidateCss = improvedCss ?? request.Css;
        var verdict = new VisualVerdict(
            VerdictId: "runtime-direction-verdict",
            Score: 0.72,
            Defects: [],
            EvidenceRefs: ["runtime-direction-verdict"],
            Passed: false);
        var candidate = new DesignConversionImprovementCandidate(
            CandidateId: "runtime-direction-current-candidate",
            RequestId: request.RequestId,
            Html: candidateHtml,
            Css: candidateCss,
            ChangeReasons: [string.Equals(candidateHtml, request.Html, StringComparison.Ordinal) &&
                string.Equals(candidateCss, request.Css, StringComparison.Ordinal)
                    ? "runtime unchanged candidate"
                    : "runtime changed candidate"],
            EvidenceRefs: ["runtime-direction-candidate"],
            Metadata: new Dictionary<string, string>());
        var output = new DesignConversionImprovementOutput(
            OutputId: "runtime-direction-output",
            CandidateId: candidate.CandidateId,
            Html: candidateHtml,
            Css: candidateCss,
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
            ImprovedHtml: candidateHtml,
            ImprovedCss: candidateCss,
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

    private sealed class UnchangedRuntimeEvaluator(
        params DesignConversionVisualCorrectionFinding[] findings) : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                CreateEvaluationWithQaScore(request, findings, score: 0.70)));
        }
    }
}
