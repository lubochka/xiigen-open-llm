// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionImprovementLoopSemanticsTests
{
    private const string SecretMarker = "PRIVATE_LOOP_SEMANTICS_SECRET";
    private const string OwnedImageArtifactRootPrefix =
        "xiigen-design-conversion-improvement-loop-semantics-";

    [Fact]
    public async Task RunAsync_MaxAttemptsAboveHardLimitFailsBeforeEvaluatorOrStrategy()
    {
        var evaluator = new ScriptedEvaluator();
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("max-attempts-should-not-run"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-max-attempts-exceeded",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts + 1,
            Metadata: new Dictionary<string, string>()));

        Assert.False(result.IsSuccess);
        Assert.Equal("IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED", result.ErrorCode);
        Assert.Equal(
            "DesignConversion improvement loop max attempts must be less than or equal to 50.",
            result.ErrorMessage);
        Assert.Empty(evaluator.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_MaxAllowedAttemptsAcceptedAndQualityReachedStopsAfterFirstEvaluation()
    {
        var evaluator = new ScriptedEvaluator(Success(0.93m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("max-attempts-boundary-should-not-run"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-max-attempts-boundary",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Single(result.Value.Attempts);
        Assert.Single(evaluator.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_AcceptedImprovementReplacesIncumbent()
    {
        var initialRequest = CreateRequest();
        var evaluator = new ScriptedEvaluator(
            Success(0.70m),
            Success(0.82m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("accepted-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-accepted-replaces-incumbent",
            InitialEvaluationRequest: initialRequest,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal("accepted-002-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("<main data-candidate=\"accepted-002\">Improved</main>", result.Value.FinalEvaluation.SubmittedRequest.Html);
        Assert.Equal("accepted_measured_improvement", result.Value.Attempts[1].PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal(bool.TrueString, result.Value.Attempts[1].PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("0.82", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_AttemptHistorySnapshotsRemainImmutableAndChronologicalAcrossStrategyCalls()
    {
        var evaluator = new ScriptedEvaluator(
            Success(0.70m),
            Success(0.80m),
            Success(0.85m));
        var strategy = new ScriptedCandidateStrategy(
            CreateCandidate("history-002"),
            CreateCandidate("history-003"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-immutable-attempt-history",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal(3, evaluator.Requests.Count);
        Assert.Equal(2, strategy.Requests.Count);

        var firstSnapshot = strategy.Requests[0].AttemptHistory;
        var secondSnapshot = strategy.Requests[1].AttemptHistory;
        var finalAttempts = result.Value.Attempts;

        Assert.IsType<ImmutableList<DesignConversionImprovementLoopAttemptTelemetry>>(firstSnapshot);
        Assert.IsType<ImmutableList<DesignConversionImprovementLoopAttemptTelemetry>>(secondSnapshot);
        Assert.IsType<ImmutableList<DesignConversionImprovementLoopAttemptTelemetry>>(finalAttempts);
        Assert.IsNotType<DesignConversionImprovementLoopAttemptTelemetry[]>(firstSnapshot);
        Assert.IsNotType<List<DesignConversionImprovementLoopAttemptTelemetry>>(firstSnapshot);
        Assert.IsNotType<DesignConversionImprovementLoopAttemptTelemetry[]>(finalAttempts);
        Assert.IsNotType<List<DesignConversionImprovementLoopAttemptTelemetry>>(finalAttempts);

        Assert.Collection(
            firstSnapshot,
            attempt => AssertAttemptSnapshot(attempt, 1, "initial"));
        Assert.Collection(
            secondSnapshot,
            attempt => AssertAttemptSnapshot(attempt, 1, "initial"),
            attempt => AssertAttemptSnapshot(attempt, 2, "history-002"));
        Assert.Collection(
            finalAttempts,
            attempt => AssertAttemptSnapshot(attempt, 1, "initial"),
            attempt => AssertAttemptSnapshot(attempt, 2, "history-002"),
            attempt => AssertAttemptSnapshot(attempt, 3, "history-003"));
    }

    [Fact]
    public async Task RunAsync_MetadataOnlyQaUsesWorstFiniteVerdictInsteadOfPolicyBestScore()
    {
        var evaluator = new ScriptedEvaluator(
            MetadataOnlySuccess(policyBestScore: 0.96, qaVerdictScores: [0.74, 0.61]));
        var strategy = new ScriptedCandidateStrategy();
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-metadata-only-worst-verdict",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("0.96", result.Value!.PublicMetadata["final_best_score"]);
        Assert.Equal("0.61", result.Value.PublicMetadata["final_measured_quality_score"]);
        var attempt = Assert.Single(result.Value.Attempts);
        Assert.Equal("0.96", attempt.PublicMetadata["best_score"]);
        Assert.Equal("0.61", attempt.PublicMetadata["measured_quality_score"]);
        Assert.All(result.Value.FinalEvaluation!.QaResults, result => Assert.Null(result.Actual.Resolution));
        Assert.Contains(result.Value.FinalEvaluation.QaResults, result => result.Verdict.Score == 0.74);
        Assert.Contains(result.Value.FinalEvaluation.QaResults, result => result.Verdict.Score == 0.61);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_MetadataOnlyQaDistributionImprovementAcceptedAsIncumbent()
    {
        var evaluator = new ScriptedEvaluator(
            MetadataOnlySuccess(policyBestScore: 0.86, qaVerdictScores: [0.66, 0.61]),
            MetadataOnlySuccess(policyBestScore: 0.88, qaVerdictScores: [0.76, 0.72]));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("metadata-improved-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-metadata-only-distribution-improved",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal("metadata-improved-002-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("0.72", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal("unscoped_qa_distribution", result.Value.PublicMetadata["quality_non_regression_evidence_mode"]);
        var initialAttempt = result.Value.Attempts[0];
        Assert.Equal("unavailable", initialAttempt.PublicMetadata["quality_non_regression_evidence_mode"]);
        Assert.Equal(bool.FalseString, initialAttempt.PublicMetadata["quality_non_regression"]);
        Assert.Equal(bool.FalseString, initialAttempt.PublicMetadata["viewport_non_regression"]);
        var attempt = result.Value.Attempts[1];
        Assert.Equal("accepted_measured_improvement", attempt.PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["quality_non_regression"]);
        Assert.Equal("unscoped_qa_distribution", attempt.PublicMetadata["quality_non_regression_evidence_mode"]);
        Assert.Equal("0.88", attempt.PublicMetadata["best_score"]);
        Assert.Equal("0.72", attempt.PublicMetadata["measured_quality_score"]);
        Assert.All(result.Value.FinalEvaluation.QaResults, qa => Assert.Null(qa.Actual.Resolution));
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_MetadataOnlyQaMismatchedDistributionCountFailsClosed()
    {
        var evaluator = new ScriptedEvaluator(
            MetadataOnlySuccess(policyBestScore: 0.84, qaVerdictScores: [0.58, 0.52]),
            MetadataOnlySuccess(policyBestScore: 0.88, qaVerdictScores: [0.70, double.NaN]));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("metadata-mismatch-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-metadata-only-distribution-mismatch",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("0.52", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal("unavailable", result.Value.PublicMetadata["quality_non_regression_evidence_mode"]);
        var attempt = result.Value.Attempts[1];
        Assert.Equal("rejected_candidate", attempt.PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("quality_evidence_unavailable", attempt.PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["quality_non_regression"]);
        Assert.Equal("unavailable", attempt.PublicMetadata["quality_non_regression_evidence_mode"]);
        Assert.Equal("0.88", attempt.PublicMetadata["best_score"]);
        Assert.Equal("0.7", attempt.PublicMetadata["measured_quality_score"]);
        Assert.Contains("IMPROVEMENT_LOOP_CANDIDATE_REJECTED:quality_evidence_unavailable", result.Value.Warnings);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_MetadataOnlyQaWithoutFiniteVerdictsFailsClosedDespiteHighPolicyBestScore()
    {
        var evaluator = new ScriptedEvaluator(
            MetadataOnlySuccess(policyBestScore: 0.97, qaVerdictScores: [double.NaN, double.PositiveInfinity]));
        var coordinator = CreateCoordinator(evaluator, new ScriptedCandidateStrategy());

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-metadata-only-no-finite-verdicts",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("0.97", result.Value!.PublicMetadata["final_best_score"]);
        Assert.Equal("0", result.Value.PublicMetadata["final_measured_quality_score"]);
        var attempt = Assert.Single(result.Value.Attempts);
        Assert.Equal("0.97", attempt.PublicMetadata["best_score"]);
        Assert.Equal("0", attempt.PublicMetadata["measured_quality_score"]);
        Assert.Contains(result.Value.FinalEvaluation!.QaResults, result => double.IsNaN(result.Verdict.Score));
        Assert.Contains(result.Value.FinalEvaluation.QaResults, result => double.IsPositiveInfinity(result.Verdict.Score));
    }

    [Fact]
    public async Task RunAsync_UnchangedRuntimeOutputWithRetryFindingStopsBeforeSecondEvaluationWithoutFallback()
    {
        var initialRequest = CreateRequest();
        var retryFinding = UnchangedRuntimeRetryFinding();
        var evaluator = new ScriptedEvaluator(
            SuccessWithFindings(0.72m, retryFinding),
            SuccessWithFindings(0.82m, retryFinding));
        var coordinator = CreateCoordinator(
            evaluator,
            new DesignConversionRuntimeImprovementCandidateStrategy());

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-unchanged-runtime-no-fallback-stop",
            InitialEvaluationRequest: initialRequest,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.CandidateStrategyFailed);
        Assert.Equal(1, result.Value.AttemptCount);
        var evaluatedRequest = Assert.Single(evaluator.Requests);
        Assert.Equal("0.01", result.Value.PublicMetadata["spent_budget_usd"]);
        Assert.Equal("0.72", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal(0.72, result.Value.FinalEvaluation!.Verdict.Score, precision: 3);
        Assert.Equal(initialRequest.RequestId, evaluatedRequest.RequestId);
        Assert.Equal(initialRequest.Html, evaluatedRequest.Html);
        Assert.Equal(initialRequest.Css, evaluatedRequest.Css);
        Assert.Equal(initialRequest.RequestId, result.Value.FinalEvaluation.SubmittedRequest.RequestId);
        Assert.Equal(initialRequest.Html, result.Value.FinalEvaluation.SubmittedRequest.Html);
        Assert.Equal(initialRequest.Css, result.Value.FinalEvaluation.SubmittedRequest.Css);
        Assert.DoesNotContain("dcm-loop-improved", result.Value.FinalEvaluation.SubmittedRequest.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("dcm-loop-improved", result.Value.FinalEvaluation.SubmittedRequest.Css, StringComparison.Ordinal);
        Assert.Contains(
            $"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_NO_PROGRESS:{DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode}",
            result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_LowerQualityRegressionRejectedAndBestRetained()
    {
        var evaluator = new ScriptedEvaluator(
            Success(0.80m),
            Success(0.72m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("regressed-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-regression-rejected",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("rejected_candidate", result.Value.Attempts[1].PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal("quality_regression", result.Value.Attempts[1].PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal(bool.FalseString, result.Value.Attempts[1].PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("0.8", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal(2, evaluator.Requests.Count);
    }

    [Fact]
    public async Task RunAsync_RetainedBestDuplicateAfterRejectedCandidateStopsBeforeEvaluation()
    {
        var initialRequest = CreateRequest();
        var regressedCandidate = CreateCandidate("regressed-002");
        var retainedBestDuplicate = new DesignConversionImprovementCandidate(
            CandidateId: "retained-best-duplicate-003",
            RequestId: "retained-best-duplicate-003-request",
            Html: $"  {initialRequest.Html}\r\n",
            Css: $"\n{initialRequest.Css}  ",
            ChangeReasons: ["scripted retained best duplicate"],
            EvidenceRefs: ["candidate:retained-best-duplicate-003"],
            Metadata: new Dictionary<string, string>
            {
                ["candidate_kind"] = "scripted"
            });
        var evaluator = new ScriptedEvaluator(
            Success(0.80m),
            Success(0.70m),
            Success(0.95m, costUsd: 0.99m));
        var strategy = new ScriptedCandidateStrategy(
            regressedCandidate,
            retainedBestDuplicate);
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-retained-best-duplicate",
            InitialEvaluationRequest: initialRequest,
            MaxAttempts: 4,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.CandidateStrategyFailed);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.DoesNotContain(evaluator.Requests, request => request.RequestId == "retained-best-duplicate-003-request");
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal("semantics-request", strategy.Requests[0].CurrentRequest.RequestId);
        Assert.Equal("semantics-request", strategy.Requests[1].CurrentRequest.RequestId);
        Assert.Equal("regressed-002-request", evaluator.Requests[1].RequestId);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("0.8", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal("0.02", result.Value.PublicMetadata["spent_budget_usd"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["candidate_strategy_failed"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata["no_progress_stop"]);
        Assert.Contains(
            "IMPROVEMENT_LOOP_CANDIDATE_VALIDATION_NO_PROGRESS:IMPROVEMENT_CANDIDATE_NO_OP",
            result.Value.Warnings);
        Assert.DoesNotContain(result.Value.Warnings, warning => warning.Contains(SecretMarker, StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_InvalidCandidateValidationStillStrategyFailure()
    {
        var regressedCandidate = CreateCandidate("regressed-002");
        var invalidCandidate = new DesignConversionImprovementCandidate(
            CandidateId: "invalid-empty-css-003",
            RequestId: "invalid-empty-css-003-request",
            Html: "<main data-candidate=\"invalid-empty-css-003\">Invalid</main>",
            Css: " ",
            ChangeReasons: ["scripted invalid candidate"],
            EvidenceRefs: ["candidate:invalid-empty-css-003"],
            Metadata: new Dictionary<string, string>
            {
                ["candidate_kind"] = "scripted"
            });
        var evaluator = new ScriptedEvaluator(
            Success(0.80m),
            Success(0.70m),
            Success(0.95m, costUsd: 0.99m));
        var strategy = new ScriptedCandidateStrategy(
            regressedCandidate,
            invalidCandidate);
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-invalid-candidate-validation",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 4,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed, result.Value!.StopReason);
        Assert.True(result.Value.CandidateStrategyFailed);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.DoesNotContain(evaluator.Requests, request => request.RequestId == "invalid-empty-css-003-request");
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("0.8", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal("0.02", result.Value.PublicMetadata["spent_budget_usd"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata["candidate_strategy_failed"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["no_progress_stop"]);
        Assert.Contains(
            "IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:IMPROVEMENT_CANDIDATE_CSS_REQUIRED",
            result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_WithinEpsilonImprovementRejectedWithNoProgressStop()
    {
        var evaluator = new ScriptedEvaluator(
            Success(0.80m),
            Success(0.805m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("epsilon-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-epsilon-no-progress",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["quality_improvement_epsilon"] = "0.01",
                ["max_consecutive_no_improvement"] = "1"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.False(result.Value.QualityReached);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("no_measured_improvement", result.Value.Attempts[1].PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal("1", result.Value.PublicMetadata["consecutive_no_improvement"]);
        Assert.Equal("0.8", result.Value.PublicMetadata["final_measured_quality_score"]);
    }

    [Fact]
    public async Task RunAsync_AcceptedTargetScoreWithCoverageReachesQuality()
    {
        var evaluator = new ScriptedEvaluator(
            Success(0.78m),
            Success(0.94m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("quality-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-quality-reached",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.True(result.Value.Completed);
        Assert.True(result.Value.QualityReached);
        Assert.Equal("quality-002-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("accepted_measured_improvement", result.Value.Attempts[1].PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal(bool.TrueString, result.Value.Attempts[1].PublicMetadata["required_viewport_coverage_complete"]);
        Assert.Equal("0.94", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_BudgetExhaustedStopsBoundedlyWithBestRetained()
    {
        var initialRequest = CreateRequest(budgetUsd: 0.02m);
        var evaluator = new ScriptedEvaluator(
            Success(0.80m, costUsd: 0.01m),
            Success(0.73m, costUsd: 0.01m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("budget-lower-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-budget-retains-best",
            InitialEvaluationRequest: initialRequest,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, result.Value!.StopReason);
        Assert.True(result.Value.BudgetCapReached);
        Assert.False(result.Value.Completed);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("quality_regression", result.Value.Attempts[1].PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal("0.02", result.Value.PublicMetadata["spent_budget_usd"]);
        Assert.Equal("0.8", result.Value.PublicMetadata["final_measured_quality_score"]);
    }

    [Fact]
    public async Task RunAsync_MaxConsecutiveNoImprovementStopsWithBestRetained()
    {
        var evaluator = new ScriptedEvaluator(
            Success(0.80m),
            Success(0.79m),
            Success(0.78m));
        var strategy = new ScriptedCandidateStrategy(
            CreateCandidate("no-progress-002"),
            CreateCandidate("no-progress-003"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-max-no-progress",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 4,
            Metadata: new Dictionary<string, string>
            {
                ["max_consecutive_no_improvement"] = "2"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.Equal(3, result.Value.AttemptCount);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.All(result.Value.Attempts.Skip(1), attempt =>
            Assert.Equal("quality_regression", attempt.PublicMetadata["candidate_rejection_reason"]));
        Assert.Equal("2", result.Value.PublicMetadata["consecutive_no_improvement"]);
        Assert.Equal("0.8", result.Value.PublicMetadata["final_measured_quality_score"]);
    }

    [Fact]
    public async Task RunAsync_AcceptedImprovementResetsCounterThenEqualScoresStopAtConfiguredNoProgressLimit()
    {
        var evaluator = new ScriptedEvaluator(
            Success(0.80m),
            Success(0.79m),
            Success(0.82m),
            Success(0.82m),
            Success(0.82m),
            Success(0.99m));
        var strategy = new ScriptedCandidateStrategy(
            CreateCandidate("regressed-002"),
            CreateCandidate("accepted-003"),
            CreateCandidate("equal-004"),
            CreateCandidate("equal-005"),
            CreateCandidate("should-not-run-006"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-equal-score-no-progress-after-reset",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 6,
            Metadata: new Dictionary<string, string>
            {
                ["caller_secret"] = SecretMarker,
                ["max_consecutive_no_improvement"] = "2"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.False(result.Value.MaxAttemptsReached);
        Assert.Equal(5, result.Value.AttemptCount);
        Assert.Equal(5, evaluator.Requests.Count);
        Assert.Equal(4, strategy.Requests.Count);
        Assert.DoesNotContain(evaluator.Requests, request => request.RequestId == "should-not-run-006-request");
        Assert.Equal("accepted-003-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("<main data-candidate=\"accepted-003\">Improved</main>", result.Value.FinalEvaluation.SubmittedRequest.Html);

        Assert.Equal("quality_regression", result.Value.Attempts[1].PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal("1", result.Value.Attempts[1].PublicMetadata["consecutive_no_improvement"]);
        Assert.Equal("accepted_measured_improvement", result.Value.Attempts[2].PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal(bool.TrueString, result.Value.Attempts[2].PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("0", result.Value.Attempts[2].PublicMetadata["consecutive_no_improvement"]);
        Assert.Equal("no_measured_improvement", result.Value.Attempts[3].PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal("1", result.Value.Attempts[3].PublicMetadata["consecutive_no_improvement"]);
        Assert.Equal("no_measured_improvement", result.Value.Attempts[4].PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal("2", result.Value.Attempts[4].PublicMetadata["consecutive_no_improvement"]);
        Assert.Equal("0", result.Value.Attempts[4].PublicMetadata["measured_improvement"]);

        Assert.Equal("no_progress_stop", result.Value.PublicMetadata["stop_reason"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata["no_progress_stop"]);
        Assert.Equal("0.0001", result.Value.PublicMetadata["improvement_epsilon"]);
        Assert.Equal("2", result.Value.PublicMetadata["max_consecutive_no_improvement"]);
        Assert.Equal("2", result.Value.PublicMetadata["consecutive_no_improvement"]);
        Assert.Equal("0.82", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.DoesNotContain(SecretMarker, string.Join("|", result.Value.PublicMetadata.Values), StringComparison.Ordinal);
        Assert.DoesNotContain(SecretMarker, string.Join("|", result.Value.EvidenceRefs), StringComparison.Ordinal);
        Assert.Contains("IMPROVEMENT_LOOP_MAX_CONSECUTIVE_NO_IMPROVEMENT_REACHED", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_MissingCoverageRetriesSameCandidateThenQualityReachedWithoutStrategy()
    {
        var evaluator = new ScriptedEvaluator(
            Success(
                score: 0.96m,
                coverageComplete: false,
                observedViewportCount: 1),
            Success(
                score: 0.96m,
                coverageComplete: true,
                observedViewportCount: 2));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("coverage-should-not-run"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-missing-coverage-recovers",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.True(result.Value.Completed);
        Assert.True(result.Value.QualityReached);
        Assert.False(result.Value.RenderManifestIncomplete);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Empty(strategy.Requests);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Equal(evaluator.Requests[0].RequestId, evaluator.Requests[1].RequestId);
        Assert.Equal(evaluator.Requests[0].Html, evaluator.Requests[1].Html);
        Assert.Equal(evaluator.Requests[0].Css, evaluator.Requests[1].Css);
        Assert.False(result.Value.FinalEvaluation!.VisualQaBoundaryTrace.MultiResolutionRender.MissingViewportIds.Any());
        Assert.True(result.Value.FinalEvaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);
        Assert.Equal(bool.TrueString, result.Value.Attempts[1].PublicMetadata["coverage_retry"]);
        Assert.Equal("1", result.Value.Attempts[1].PublicMetadata["coverage_retry_count"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata["coverage_retry"]);
        Assert.Equal("1", result.Value.PublicMetadata["coverage_retry_count"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker, result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task RunAsync_CoverageCompletionWithMixedQualityEvidenceFailsClosed()
    {
        var evaluator = new ScriptedEvaluator(
            ScriptedEvaluation.Success(
                score: 0.70,
                costUsd: 0.01m,
                coverageComplete: false,
                observedViewportCount: 1,
                qaVerdictScores: [0.60],
                metadataOnlyQaResults: true),
            ScriptedEvaluation.Success(
                score: 0.85,
                costUsd: 0.01m,
                coverageComplete: true,
                observedViewportCount: 2,
                qaVerdictScores: [0.82, 0.80]));
        var strategy = new ScriptedCandidateStrategy();
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-coverage-completion-mixed-quality-evidence",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Equal("0.6", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal("unavailable", result.Value.PublicMetadata["quality_non_regression_evidence_mode"]);
        Assert.False(result.Value.FinalEvaluation!.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);
        Assert.Single(result.Value.FinalEvaluation.QaResults);
        Assert.Null(result.Value.FinalEvaluation.QaResults[0].Actual.Resolution);

        var attempt = result.Value.Attempts[1];
        Assert.Equal("rejected_candidate", attempt.PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("quality_evidence_unavailable", attempt.PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["quality_non_regression"]);
        Assert.Equal("unavailable", attempt.PublicMetadata["quality_non_regression_evidence_mode"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["viewport_non_regression"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["required_viewport_coverage_complete"]);
        Assert.Equal("0.85", attempt.PublicMetadata["best_score"]);
        Assert.Equal("0.8", attempt.PublicMetadata["measured_quality_score"]);
        Assert.Equal("0.6", attempt.PublicMetadata["incumbent_score_before"]);
        Assert.Equal("0.2", attempt.PublicMetadata["measured_improvement"]);
        Assert.Contains("IMPROVEMENT_LOOP_CANDIDATE_REJECTED:quality_evidence_unavailable", result.Value.Warnings);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_MetadataOnlyCoverageCompletionBelowEpsilonUsesGeneralAcceptanceGate()
    {
        var evaluator = new ScriptedEvaluator(
            ScriptedEvaluation.Success(
                score: 0.70,
                costUsd: 0.01m,
                coverageComplete: false,
                observedViewportCount: 1,
                qaVerdictScores: [0.60],
                metadataOnlyQaResults: true),
            ScriptedEvaluation.Success(
                score: 0.85,
                costUsd: 0.01m,
                coverageComplete: true,
                observedViewportCount: 2,
                qaVerdictScores: [0.61, double.NaN],
                metadataOnlyQaResults: true));
        var strategy = new ScriptedCandidateStrategy();
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-metadata-only-coverage-completion-below-epsilon",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["quality_improvement_epsilon"] = "0.05"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Equal("0.6", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Equal("unscoped_qa_distribution", result.Value.PublicMetadata["quality_non_regression_evidence_mode"]);
        Assert.False(result.Value.FinalEvaluation!.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);
        Assert.Single(result.Value.FinalEvaluation.QaResults);
        Assert.Null(result.Value.FinalEvaluation.QaResults[0].Actual.Resolution);

        var attempt = result.Value.Attempts[1];
        Assert.Equal("rejected_candidate", attempt.PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("no_measured_improvement", attempt.PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["quality_non_regression"]);
        Assert.Equal("unscoped_qa_distribution", attempt.PublicMetadata["quality_non_regression_evidence_mode"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["viewport_non_regression"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["required_viewport_coverage_complete"]);
        Assert.Equal("0.61", attempt.PublicMetadata["measured_quality_score"]);
        Assert.Equal("0.6", attempt.PublicMetadata["incumbent_score_before"]);
        Assert.Equal("0.01", attempt.PublicMetadata["measured_improvement"]);
        Assert.Equal("0.05", attempt.PublicMetadata["improvement_epsilon"]);
        Assert.Contains("IMPROVEMENT_LOOP_CANDIDATE_REJECTED:no_measured_improvement", result.Value.Warnings);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_PersistentMissingRequiredViewportCoverageRetriesBoundedlyWithoutStrategy()
    {
        var evaluator = new ScriptedEvaluator(
            Success(
                score: 0.96m,
                coverageComplete: false,
                observedViewportCount: 1),
            Success(
                score: 0.96m,
                coverageComplete: false,
                observedViewportCount: 1),
            Success(
                score: 0.96m,
                coverageComplete: false,
                observedViewportCount: 1));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("coverage-should-not-run"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-persistent-missing-coverage",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, result.Value!.StopReason);
        Assert.True(result.Value.RenderManifestIncomplete);
        Assert.False(result.Value.Completed);
        Assert.False(result.Value.QualityReached);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["product_ready"]);
        Assert.Empty(strategy.Requests);
        Assert.Equal(3, evaluator.Requests.Count);
        Assert.Equal(3, result.Value.AttemptCount);
        Assert.All(evaluator.Requests, request =>
        {
            Assert.Equal(evaluator.Requests[0].RequestId, request.RequestId);
            Assert.Equal(evaluator.Requests[0].Html, request.Html);
            Assert.Equal(evaluator.Requests[0].Css, request.Css);
        });
        Assert.Single(result.Value.FinalEvaluation!.QaResults);
        Assert.Equal("source-mobile", result.Value.FinalEvaluation.VisualQaBoundaryTrace.MultiResolutionRender.MissingViewportIds.Single());
        Assert.Equal(bool.TrueString, result.Value.Attempts[1].PublicMetadata["coverage_retry"]);
        Assert.Equal(bool.TrueString, result.Value.Attempts[2].PublicMetadata["coverage_retry"]);
        Assert.Equal("2", result.Value.PublicMetadata["coverage_retry_count"]);
        Assert.Equal("configured_max_attempts", result.Value.PublicMetadata["coverage_retry_stop_source"]);
    }

    [Fact]
    public async Task RunAsync_MissingCoverageStopsBeforeRetryWhenCostBudgetExhausted()
    {
        var evaluator = new ScriptedEvaluator(Success(
            score: 0.96m,
            costUsd: 0.01m,
            coverageComplete: false,
            observedViewportCount: 1));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("coverage-budget-should-not-run"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-missing-coverage-budget-stop",
            InitialEvaluationRequest: CreateRequest(budgetUsd: 0.01m),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, result.Value!.StopReason);
        Assert.True(result.Value.BudgetCapReached);
        Assert.True(result.Value.RenderManifestIncomplete);
        Assert.False(result.Value.QualityReached);
        Assert.Single(evaluator.Requests);
        Assert.Empty(strategy.Requests);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Equal("source-mobile", result.Value.FinalEvaluation!.VisualQaBoundaryTrace.MultiResolutionRender.MissingViewportIds.Single());
        Assert.Equal("cost_budget", result.Value.PublicMetadata["coverage_retry_stop_source"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["coverage_retry"]);
        Assert.Equal("0", result.Value.PublicMetadata["coverage_retry_count"]);
    }

    [Fact]
    public async Task RunAsync_ProviderRequiredStopsTerminalWithoutCoverageRetry()
    {
        var evaluator = new ScriptedEvaluator(ProviderBlocked());
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("provider-should-not-run"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-provider-required-terminal",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value!.StopReason);
        Assert.True(result.Value.ProviderRequiredOrBlocked);
        Assert.False(result.Value.RenderManifestIncomplete);
        Assert.False(result.Value.QualityReached);
        Assert.Single(evaluator.Requests);
        Assert.Empty(strategy.Requests);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["coverage_retry"]);
        Assert.Equal("0", result.Value.PublicMetadata["coverage_retry_count"]);
    }

    [Fact]
    public async Task RunAsync_RenderJudgeOrImproverFailureStopsBoundedlyAndRetainsIncumbent()
    {
        var evaluator = new ScriptedEvaluator(
            Success(0.81m),
            Failure("SCRIPTED_RENDER_OR_JUDGE_OR_IMPROVER_FAILURE"));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("failure-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-evaluation-failure-bounded",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.EvaluationFailed, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Equal("evaluation_failed", result.Value.Attempts[1].PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal("0.81", result.Value.PublicMetadata["final_measured_quality_score"]);
        Assert.Contains("IMPROVEMENT_LOOP_EVALUATION_FAILED:SCRIPTED_RENDER_OR_JUDGE_OR_IMPROVER_FAILURE", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_NonFatalEvaluatorExceptionAfterIncumbentStopsBoundedlyWithoutLeak()
    {
        var exceptionMarker = "PRIVATE_EVALUATOR_EXCEPTION_" + Guid.NewGuid().ToString("N");
        var evaluator = new ScriptedEvaluator(
            Success(0.81m),
            new ScriptedThrow(new InvalidOperationException(exceptionMarker)));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("evaluator-exception-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-nonfatal-evaluator-exception",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            "IMPROVEMENT_LOOP_EVALUATOR_BOUNDARY_EXCEPTION",
            DesignConversionImprovementLoopCoordinator.EvaluatorBoundaryExceptionCode);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.EvaluationFailed, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Single(strategy.Requests);
        var failedAttempt = result.Value.Attempts[1];
        Assert.Equal("evaluation_failed", failedAttempt.PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal(
            DesignConversionDpoLeakageGuard.HashRef(
                "dcm-loop-evaluation-error",
                DesignConversionImprovementLoopCoordinator.EvaluatorBoundaryExceptionCode),
            failedAttempt.PublicMetadata["evaluation_error_code"]);
        Assert.Contains(
            $"IMPROVEMENT_LOOP_EVALUATION_FAILED:{DesignConversionImprovementLoopCoordinator.EvaluatorBoundaryExceptionCode}",
            result.Value.Warnings);
        var serialized = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(exceptionMarker, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_NonFatalStrategyExceptionStopsBoundedlyAndRetainsIncumbentWithoutLeak()
    {
        var exceptionMarker = "PRIVATE_STRATEGY_EXCEPTION_" + Guid.NewGuid().ToString("N");
        var evaluator = new ScriptedEvaluator(Success(0.81m));
        var strategy = new ScriptedCandidateStrategy(
            new ScriptedThrow(new InvalidOperationException(exceptionMarker)));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-nonfatal-strategy-exception",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            "IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_BOUNDARY_EXCEPTION",
            DesignConversionImprovementLoopCoordinator.CandidateStrategyBoundaryExceptionCode);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed, result.Value!.StopReason);
        Assert.True(result.Value.CandidateStrategyFailed);
        Assert.False(result.Value.Completed);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Single(evaluator.Requests);
        Assert.Single(strategy.Requests);
        Assert.Contains(
            $"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:{DesignConversionImprovementLoopCoordinator.CandidateStrategyBoundaryExceptionCode}",
            result.Value.Warnings);
        var serialized = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(exceptionMarker, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_NonFatalEvaluatorExceptionReturnsBoundedNonReadyInsteadOfGeneric500()
    {
        var exceptionMarker = "PRIVATE_HANDLER_EVALUATOR_EXCEPTION_" + Guid.NewGuid().ToString("N");
        var evaluator = new ScriptedEvaluator(
            Success(0.81m),
            new ScriptedThrow(new IOException(exceptionMarker)));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("handler-exception-002"));
        var handler = new DesignConversionModelServerImprovementLoopHandler(
            CreateCoordinator(evaluator, strategy));

        var response = await handler.HandleAsync(CreateImprovementLoopHttpRequest(
            "application/json; charset=utf-8",
            loopId: "semantics-handler-nonfatal-evaluator-exception"));

        Assert.Equal(200, response.StatusCode);
        Assert.DoesNotContain(exceptionMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(IOException), response.BodyJson, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.EvaluationFailed,
            root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => string.Equals(
                warning.GetString(),
                $"IMPROVEMENT_LOOP_EVALUATION_FAILED:{DesignConversionImprovementLoopCoordinator.EvaluatorBoundaryExceptionCode}",
                StringComparison.Ordinal));
        Assert.Equal(
            bool.FalseString,
            root.GetProperty("boundaryMetadata").GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task RunAsync_FatalEvaluatorExceptionGraphPropagatesUntouched()
    {
        var fatal = new AggregateException(
            "fatal evaluator graph",
            new IOException("ordinary inner"),
            new OutOfMemoryException("fatal inner"));
        var evaluator = new ScriptedEvaluator(
            Success(0.81m),
            new ScriptedThrow(fatal));
        var coordinator = CreateCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(CreateCandidate("fatal-evaluator-002")));

        var thrown = await Assert.ThrowsAsync<AggregateException>(() => coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "semantics-fatal-evaluator-exception",
                InitialEvaluationRequest: CreateRequest(),
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>())));

        Assert.Same(fatal, thrown);
    }

    [Fact]
    public async Task RunAsync_DirectFatalEvaluatorExceptionPropagatesUntouched()
    {
        var fatal = new OutOfMemoryException("direct fatal evaluator");
        var coordinator = CreateCoordinator(
            new ScriptedEvaluator(new ScriptedThrow(fatal)),
            new ScriptedCandidateStrategy());

        var thrown = await Assert.ThrowsAsync<OutOfMemoryException>(() => coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "semantics-direct-fatal-evaluator-exception",
                InitialEvaluationRequest: CreateRequest(),
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>())));

        Assert.Same(fatal, thrown);
    }

    [Fact]
    public async Task RunAsync_FatalStrategyExceptionGraphPropagatesUntouched()
    {
        var fatal = new AggregateException(
            "fatal strategy graph",
            new IOException("ordinary inner"),
            new AccessViolationException("fatal inner"));
        var coordinator = CreateCoordinator(
            new ScriptedEvaluator(Success(0.81m)),
            new ScriptedCandidateStrategy(new ScriptedThrow(fatal)));

        var thrown = await Assert.ThrowsAsync<AggregateException>(() => coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "semantics-fatal-strategy-exception",
                InitialEvaluationRequest: CreateRequest(),
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>())));

        Assert.Same(fatal, thrown);
    }

    [Fact]
    public async Task RunAsync_CallerCancellationWinsRaceWithNonFatalEvaluatorException()
    {
        using var cancellation = new CancellationTokenSource();
        var evaluator = new ScriptedEvaluator(
            Success(0.81m),
            new ScriptedThrow(
                new InvalidOperationException("must be cancellation, not evaluation failure"),
                cancellation.Cancel));
        var coordinator = CreateCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(CreateCandidate("cancel-evaluator-002")));

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "semantics-cancel-race-evaluator-exception",
                InitialEvaluationRequest: CreateRequest(),
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()),
            cancellation.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.Cancelled, result.Value!.StopReason);
        Assert.True(result.Value.Cancelled);
        Assert.False(result.Value.DeadlineExceeded);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.DoesNotContain(
            result.Value.Warnings,
            warning => warning.Contains(
                DesignConversionImprovementLoopCoordinator.EvaluatorBoundaryExceptionCode,
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_CallerCancellationWinsRaceWithNonFatalStrategyException()
    {
        using var cancellation = new CancellationTokenSource();
        var strategy = new ScriptedCandidateStrategy(
            new ScriptedThrow(
                new InvalidOperationException("must be cancellation, not strategy failure"),
                cancellation.Cancel));
        var coordinator = CreateCoordinator(new ScriptedEvaluator(Success(0.81m)), strategy);

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "semantics-cancel-race-strategy-exception",
                InitialEvaluationRequest: CreateRequest(),
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()),
            cancellation.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.Cancelled, result.Value!.StopReason);
        Assert.True(result.Value.Cancelled);
        Assert.False(result.Value.DeadlineExceeded);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.DoesNotContain(
            result.Value.Warnings,
            warning => warning.Contains(
                DesignConversionImprovementLoopCoordinator.CandidateStrategyBoundaryExceptionCode,
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_InternalDeadlineWinsRaceWithNonFatalEvaluatorException()
    {
        var evaluator = new ScriptedEvaluator(
            new ScriptedThrowAfterCancellation(
                new IOException("must be deadline, not evaluation failure")));
        var coordinator = CreateCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(),
            runDeadline: TimeSpan.FromMilliseconds(50));

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-deadline-race-evaluator-exception",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, result.Value!.StopReason);
        Assert.True(result.Value.DeadlineExceeded);
        Assert.False(result.Value.Cancelled);
        Assert.Equal(0, result.Value.AttemptCount);
        Assert.Single(evaluator.Requests);
        Assert.DoesNotContain(
            result.Value.Warnings,
            warning => warning.Contains(
                DesignConversionImprovementLoopCoordinator.EvaluatorBoundaryExceptionCode,
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_InternalDeadlineWinsRaceWithNonFatalStrategyException()
    {
        var strategy = new ScriptedCandidateStrategy(
            new ScriptedThrowAfterCancellation(
                new IOException("must be deadline, not strategy failure")));
        var coordinator = CreateCoordinator(
            new ScriptedEvaluator(Success(0.81m)),
            strategy,
            runDeadline: TimeSpan.FromMilliseconds(50));

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-deadline-race-strategy-exception",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, result.Value!.StopReason);
        Assert.True(result.Value.DeadlineExceeded);
        Assert.False(result.Value.Cancelled);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Single(strategy.Requests);
        Assert.DoesNotContain(
            result.Value.Warnings,
            warning => warning.Contains(
                DesignConversionImprovementLoopCoordinator.CandidateStrategyBoundaryExceptionCode,
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_CostOnlyCheaperLowerQualityCandidateRejected()
    {
        var evaluator = new ScriptedEvaluator(
            Success(0.84m, costUsd: 0.20m),
            Success(0.82m, costUsd: 0.01m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("cheap-but-lower-002"));
        var coordinator = CreateCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "semantics-cost-only-rejected",
            InitialEvaluationRequest: CreateRequest(budgetUsd: 1.00m),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal("semantics-request", result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal("quality_regression", result.Value.Attempts[1].PublicMetadata["candidate_rejection_reason"]);
        Assert.Equal(bool.FalseString, result.Value.Attempts[1].PublicMetadata["cost_savings_accept_lower_quality"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["cost_savings_accept_lower_quality"]);
        Assert.Equal("0.84", result.Value.PublicMetadata["final_measured_quality_score"]);
    }

    [Fact]
    public async Task ModelServerResponse_ExposesSafeTerminationSpendBestScoreRefsWithoutRawPayloadsOrProductReady()
    {
        var evaluator = new ScriptedEvaluator(Success(0.93m, costUsd: 0.03m));
        var coordinator = CreateCoordinator(evaluator, new ScriptedCandidateStrategy());
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopForLiveHostTesting(
            new DesignConversionModelServerImprovementLoopHandler(coordinator));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);

        var envelope = await adapter.TryRunImprovementLoopAsync(new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "semantics-modelserver-safe-response",
            InitialPackage: CreateModelServerPackage(),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_secret"] = SecretMarker
            }));

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["screenshot_bytes_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal("0.03", envelope.BoundaryMetadata["spent_budget_usd"]);
        Assert.Equal("0.93", envelope.BoundaryMetadata["final_measured_quality_score"]);

        Assert.DoesNotContain(SecretMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".secret", envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SCREENSHOT_BYTES", envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProductReady", envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("\"productReady\":true", envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("qualityReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.NotEmpty(root.GetProperty("evidenceRefs").EnumerateArray());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal("0.03", metadata.GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0.93", metadata.GetProperty("final_measured_quality_score").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("refs_or_hashes_only").GetString());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        Assert.Equal(0.93, attempt.GetProperty("bestScore").GetDouble(), precision: 3);
        Assert.NotEmpty(attempt.GetProperty("renderResultRefs").EnumerateArray());
        Assert.NotEmpty(attempt.GetProperty("comparisonArtifactRefs").EnumerateArray());
        Assert.NotEmpty(attempt.GetProperty("visualJudgeResultRefs").EnumerateArray());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_NullEmptyOrWhitespaceContentTypeFailsClosedBeforeCoordinator(string? contentType)
    {
        var evaluator = new ScriptedEvaluator(Success(0.93m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("content-type-should-not-run"));
        var handler = new DesignConversionModelServerImprovementLoopHandler(CreateCoordinator(evaluator, strategy));

        var response = await handler.HandleAsync(CreateImprovementLoopHttpRequest(
            contentType,
            loopId: "semantics-content-type-fail-closed"));

        Assert.Equal(415, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.Empty(evaluator.Requests);
        Assert.Empty(strategy.Requests);
        Assert.DoesNotContain(SecretMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".secret", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SCREENSHOT_BYTES", response.BodyJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerImprovementLoopHandler.EndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal("UNSUPPORTED_MEDIA_TYPE", root.GetProperty("errorCode").GetString());
        Assert.Equal(415, root.GetProperty("statusCode").GetInt32());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("refs_or_hashes_only").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
    }

    [Fact]
    public async Task HandleAsync_ApplicationJsonWithCharsetReachesCoordinatorProcessing()
    {
        var evaluator = new ScriptedEvaluator(Success(0.93m, costUsd: 0.03m));
        var strategy = new ScriptedCandidateStrategy(CreateCandidate("content-type-positive-control"));
        var handler = new DesignConversionModelServerImprovementLoopHandler(CreateCoordinator(evaluator, strategy));

        var response = await handler.HandleAsync(CreateImprovementLoopHttpRequest(
            "application/json; charset=utf-8",
            loopId: "semantics-content-type-positive"));

        Assert.NotEqual(415, response.StatusCode);
        Assert.Equal(200, response.StatusCode);
        Assert.Single(evaluator.Requests);
        Assert.Empty(strategy.Requests);
        Assert.DoesNotContain(SecretMarker, response.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("refs_or_hashes_only").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
    }


    private static DesignConversionImprovementLoopCoordinator CreateCoordinator(
        ScriptedEvaluator evaluator,
        IDesignConversionImprovementCandidateStrategy strategy,
        TimeSpan? runDeadline = null)
        => new(evaluator, strategy, runDeadline: runDeadline);

    private static ScriptedEvaluation Success(
        decimal score,
        decimal costUsd = 0.01m,
        bool coverageComplete = true,
        int observedViewportCount = 2)
        => ScriptedEvaluation.Success((double)score, costUsd, coverageComplete, observedViewportCount);

    private static ScriptedEvaluation SuccessWithFindings(
        decimal score,
        params DesignConversionVisualCorrectionFinding[] retryFindings)
        => ScriptedEvaluation.Success(
            (double)score,
            costUsd: 0.01m,
            coverageComplete: true,
            observedViewportCount: 2,
            retryFindings);

    private static ScriptedEvaluation MetadataOnlySuccess(
        double policyBestScore,
        params double[] qaVerdictScores)
        => ScriptedEvaluation.Success(
            policyBestScore,
            costUsd: 0.01m,
            coverageComplete: true,
            observedViewportCount: 2,
            qaVerdictScores: qaVerdictScores,
            metadataOnlyQaResults: true);

    private static ScriptedEvaluation ProviderBlocked()
        => ScriptedEvaluation.BlockedProvider();

    private static OperationResult<DesignConversionInternalEvaluationResult> Failure(string errorCode)
        => OperationResult<DesignConversionInternalEvaluationResult>.Fail(
            "Scripted bounded evaluation failure.",
            errorCode);

    private static void AssertAttemptSnapshot(
        DesignConversionImprovementLoopAttemptTelemetry attempt,
        int expectedAttemptNumber,
        string expectedCandidateRefSource)
    {
        Assert.Equal(expectedAttemptNumber, attempt.AttemptNumber);
        Assert.Equal(
            DesignConversionDpoLeakageGuard.HashRef("dcm-loop-candidate", expectedCandidateRefSource),
            attempt.CandidateRef);
    }

    private static DesignConversionInternalEvaluateRequest CreateRequest(
        decimal budgetUsd = 1.00m,
        IReadOnlyDictionary<string, string>? budgetMetadata = null)
        => new(
            JobId: "semantics-job",
            RequestId: "semantics-request",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: $"<main data-secret=\"{SecretMarker}\">Original</main>",
            Css: $".secret{{display:grid;}}/* {SecretMarker} */",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "semantics-tree-current",
                Format: "figma-json",
                Content: $"{{\"secret\":\"{SecretMarker}\"}}",
                Sha256: "sha256:semantics-tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "semantics-tree-original",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:semantics-tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots:
            [
                RenderArtifact("source-desktop-shot", "screenshot", 1440, 900),
                RenderArtifact("source-mobile-shot", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                RenderArtifact("source-desktop-render", "design-render", 1440, 900),
                RenderArtifact("source-mobile-render", "design-render", 390, 844)
            ],
            Budget: new DesignConversionBudgetMetadata(
                UserId: $"user-{SecretMarker}",
                Tier: "internal",
                MaxImprovementCandidates: 10,
                MaxRenderQaRuns: 2,
                BudgetUsd: budgetUsd,
                Metadata: new Dictionary<string, string>(budgetMetadata ?? new Dictionary<string, string>(), StringComparer.Ordinal)
                {
                    ["quality_threshold"] = "0.90"
                }),
            RenderResolutions: RequiredResolutions(),
            AiTeacherEvidenceRefs: ["teacher:none"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["fixture"] = "improvement-loop-semantics"
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "isolated-semantics-test"
            });

    private static IReadOnlyList<RenderQaResolution> RequiredResolutions()
        =>
        [
            new("source-desktop", 1440, 900, 1.0),
            new("source-mobile", 390, 844, 1.0)
        ];

    private static DesignRenderArtifact RenderArtifact(string id, string kind, int width, int height)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: width,
            Height: height,
            Uri: $"artifact://semantics/{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static DesignConversionImprovementCandidate CreateCandidate(string candidateId)
        => new(
            CandidateId: candidateId,
            RequestId: $"{candidateId}-request",
            Html: $"<main data-candidate=\"{candidateId}\">Improved</main>",
            Css: $".candidate-{candidateId}{{display:grid;gap:16px;}}",
            ChangeReasons: ["scripted semantic improvement candidate"],
            EvidenceRefs: [$"candidate:{candidateId}"],
            Metadata: new Dictionary<string, string>
            {
                ["candidate_kind"] = "scripted"
            });

    private static DesignConversionModelServerLocalCallerPackage CreateModelServerPackage()
        => new(
            RequestId: "semantics-modelserver-request",
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:tree-ref",
                Payload: $"{{\"private\":\"{SecretMarker}\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["secret"] = SecretMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:original-tree-ref",
                Payload: "{\"type\":\"FRAME\"}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>()),
            Html: $"<main data-secret=\"{SecretMarker}\">Original</main>",
            Css: $".secret{{color:red;}}/* {SecretMarker} */",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "shot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://SCREENSHOT_BYTES/desktop.png",
                    Sha256: "sha256:shot-desktop",
                    Metadata: new Dictionary<string, string>
                    {
                        ["secret"] = SecretMarker
                    }),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "shot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://SCREENSHOT_BYTES/mobile.png",
                    Sha256: "sha256:shot-mobile",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders: null,
            Budget: new DesignConversionModelServerBudget(
                UserId: $"user-{SecretMarker}",
                Tier: "internal",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90"
                }),
            Metadata: new Dictionary<string, string>
            {
                ["private_marker"] = SecretMarker
            });

    private static DesignConversionModelServerHttpRequest CreateImprovementLoopHttpRequest(
        string? contentType,
        string loopId)
    {
        var body = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: loopId,
            InitialPackage: CreateModelServerPackage(),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = SecretMarker
            });

        return new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerImprovementLoopHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: contentType);
    }

    private static string CreateServerOwnedImageArtifactRoot()
    {
        var normalizedTempRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.GetTempPath()));
        var root = Path.Combine(
            normalizedTempRoot,
            OwnedImageArtifactRootPrefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteOwnedServerOwnedImageArtifactRoot(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            return;

        string normalizedRoot;
        string normalizedTempRoot;
        try
        {
            normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
            normalizedTempRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(Path.GetTempPath()));
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return;
        }

        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!string.Equals(
                Path.GetDirectoryName(normalizedRoot),
                normalizedTempRoot,
                pathComparison))
        {
            return;
        }

        var directoryName = Path.GetFileName(normalizedRoot);
        if (!directoryName.StartsWith(OwnedImageArtifactRootPrefix, StringComparison.Ordinal))
            return;

        var uniqueSuffix = directoryName[OwnedImageArtifactRootPrefix.Length..];
        if (uniqueSuffix.Length != 32 ||
            !Guid.TryParseExact(uniqueSuffix, "N", out _))
        {
            return;
        }

        if (!Directory.Exists(normalizedRoot))
            return;

        FileAttributes attributes;
        try
        {
            attributes = File.GetAttributes(normalizedRoot);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return;
        }

        if ((attributes & FileAttributes.ReparsePoint) != 0)
            return;

        Directory.Delete(normalizedRoot, recursive: true);
    }

    private sealed record ScriptedEvaluation(
        double Score,
        decimal CostUsd,
        bool CoverageComplete,
        int ObservedViewportCount,
        bool ProviderBlockedResult,
        IReadOnlyList<DesignConversionVisualCorrectionFinding> RetryFindings,
        IReadOnlyList<double> QaVerdictScores,
        bool MetadataOnlyQaResults)
    {
        public static ScriptedEvaluation Success(
            double score,
            decimal costUsd,
            bool coverageComplete,
            int observedViewportCount,
            IReadOnlyList<DesignConversionVisualCorrectionFinding>? retryFindings = null,
            IReadOnlyList<double>? qaVerdictScores = null,
            bool metadataOnlyQaResults = false)
            => new(
                score,
                costUsd,
                coverageComplete,
                observedViewportCount,
                ProviderBlockedResult: false,
                RetryFindings: retryFindings ?? [],
                QaVerdictScores: qaVerdictScores ?? [],
                MetadataOnlyQaResults: metadataOnlyQaResults);

        public static ScriptedEvaluation BlockedProvider()
            => new(
                0.0,
                0m,
                CoverageComplete: true,
                ObservedViewportCount: 2,
                ProviderBlockedResult: true,
                RetryFindings: [],
                QaVerdictScores: [],
                MetadataOnlyQaResults: false);
    }

    private sealed record ScriptedThrow(
        Exception Exception,
        Action? BeforeThrow = null);

    private sealed record ScriptedThrowAfterCancellation(Exception Exception);

    private static async Task<OperationResult<T>> ThrowAfterCancellationAsync<T>(
        CancellationToken cancellationToken,
        Exception exception)
    {
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Convert the raced boundary failure to its scripted nonfatal exception.
        }

        throw exception;
    }

    private sealed class ScriptedEvaluator(params object[] outcomes) : IDesignConversionThinClientJobEvaluator
    {
        private readonly Queue<object> _outcomes = new(outcomes);

        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (_outcomes.Count == 0)
            {
                return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                    "No scripted evaluation outcome was configured.",
                    "SCRIPTED_EVALUATION_MISSING"));
            }

            var outcome = _outcomes.Dequeue();
            if (outcome is ScriptedThrow scriptedThrow)
            {
                scriptedThrow.BeforeThrow?.Invoke();
                return Task.FromException<OperationResult<DesignConversionInternalEvaluationResult>>(
                    scriptedThrow.Exception);
            }

            if (outcome is ScriptedThrowAfterCancellation delayedThrow)
            {
                return ThrowAfterCancellationAsync<DesignConversionInternalEvaluationResult>(
                    cancellationToken,
                    delayedThrow.Exception);
            }

            if (outcome is OperationResult<DesignConversionInternalEvaluationResult> failure)
                return Task.FromResult(failure);

            var script = Assert.IsType<ScriptedEvaluation>(outcome);
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                CreateEvaluation(request, script)));
        }
    }

    private sealed class ScriptedCandidateStrategy(params object[] outcomes)
        : IDesignConversionImprovementCandidateStrategy
    {
        private readonly Queue<object> _outcomes = new(outcomes);

        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (_outcomes.Count == 0)
            {
                return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "No scripted candidate was configured.",
                    "SCRIPTED_CANDIDATE_MISSING"));
            }

            var outcome = _outcomes.Dequeue();
            if (outcome is ScriptedThrow scriptedThrow)
            {
                scriptedThrow.BeforeThrow?.Invoke();
                return Task.FromException<OperationResult<DesignConversionImprovementCandidate>>(
                    scriptedThrow.Exception);
            }

            if (outcome is ScriptedThrowAfterCancellation delayedThrow)
            {
                return ThrowAfterCancellationAsync<DesignConversionImprovementCandidate>(
                    cancellationToken,
                    delayedThrow.Exception);
            }

            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Ok(
                Assert.IsType<DesignConversionImprovementCandidate>(outcome)));
        }
    }

    private static DesignConversionInternalEvaluationResult CreateEvaluation(
        DesignConversionInternalEvaluateRequest request,
        ScriptedEvaluation script)
    {
        var submitted = ToSubmittedRequest(request);
        var required = request.RenderResolutions.Count == 0
            ? RequiredResolutions()
            : request.RenderResolutions;
        var observedCount = script.CoverageComplete
            ? required.Count
            : Math.Clamp(script.ObservedViewportCount, 0, Math.Max(0, required.Count - 1));
        var observed = required.Take(observedCount).ToArray();
        var renderResults = new List<DesignConversionRenderResult>();
        var comparisons = new List<DesignConversionQaComparisonArtifact>();
        var judges = new List<DesignConversionVisualJudgeResult>();
        var qaResults = new List<RenderQaResultRecord>();
        var passed = !script.ProviderBlockedResult && script.Score >= 0.90;

        for (var index = 0; index < observed.Length; index++)
        {
            var resolution = observed[index];
            var qaScore = ResolveQaScore(script, index);
            var expected = Image($"expected-{request.RequestId}-{index}", "expected", resolution);
            var actual = Image(
                $"actual-{request.RequestId}-{index}",
                "actual",
                script.MetadataOnlyQaResults ? null : resolution);
            var verdict = Verdict(
                $"verdict-{request.RequestId}-{index}",
                qaScore,
                IsFiniteScore(qaScore) && qaScore >= 0.90,
                expected.ImageId,
                actual.ImageId);
            var render = new DesignConversionRenderResult(
                RenderResultId: $"{request.JobId}:render:{index + 1}",
                RenderRequestId: $"{request.JobId}:render-request:{index + 1}",
                Image: actual,
                ElapsedMs: 1,
                Warnings: []);
            var comparison = new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.JobId}:comparison:{index + 1}",
                ComparisonRequestId: $"{request.JobId}:comparison-request:{index + 1}",
                Expected: expected,
                Actual: actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://semantics/{request.JobId}/diff-{index + 1}.png",
                DiffImageSha256: $"sha256:{request.JobId}:diff:{index + 1}",
                Metrics: new Dictionary<string, double>
                {
                    ["visual_distance"] = IsFiniteScore(qaScore) ? 1.0 - qaScore : 1.0
                },
                EvidenceRefs: [$"comparison:{request.JobId}:{index + 1}"]);
            var judge = new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JobId}:judge:{index + 1}",
                JudgeRequestId: $"{request.JobId}:judge-request:{index + 1}",
                Status: script.ProviderBlockedResult
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : "SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED",
                IsBlocked: script.ProviderBlockedResult,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: script.ProviderBlockedResult ? "none" : "scripted-local-judge",
                    EndpointHost: "none",
                    ModelId: script.ProviderBlockedResult ? "none" : "scripted-v1",
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = (!script.ProviderBlockedResult).ToString(),
                        ["external_ai_visual_judge_executed"] = bool.FalseString
                    }),
                EstimatedCostUsd: index == 0 ? script.CostUsd : 0m,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: [$"judge:{request.JobId}:{index + 1}"],
                Metadata: new Dictionary<string, string>
                {
                    ["score"] = qaScore.ToString("R", CultureInfo.InvariantCulture),
                    ["external_ai_visual_judge_executed"] = bool.FalseString
                });
            var qa = new RenderQaResultRecord(
                ResultId: $"{request.JobId}:qa:{index + 1}",
                RequestId: request.RequestId,
                Expected: expected,
                Actual: actual,
                Verdict: verdict,
                Metrics: new Dictionary<string, double>
                {
                    ["visual_judge_score"] = qaScore
                },
                Warnings: []);

            renderResults.Add(render);
            comparisons.Add(comparison);
            judges.Add(judge);
            qaResults.Add(qa);
        }

        var boundary = CreateBoundary(request, script, required, observed);
        var finalVerdict = Verdict($"final-{request.RequestId}", script.Score, passed, "expected-final", "actual-final");
        var candidate = new DesignConversionImprovementCandidate(
            CandidateId: $"{request.RequestId}:runtime-candidate",
            RequestId: request.RequestId,
            Html: request.Html,
            Css: request.Css,
            ChangeReasons: ["scripted-runtime-candidate"],
            EvidenceRefs: [$"runtime-candidate:{request.RequestId}"],
            Metadata: new Dictionary<string, string>());
        var output = new DesignConversionImprovementOutput(
            OutputId: $"{request.RequestId}:runtime-output",
            CandidateId: candidate.CandidateId,
            Html: request.Html,
            Css: request.Css,
            Verdict: finalVerdict,
            QaResults: qaResults,
            EvidenceRefs: [$"runtime-output:{request.RequestId}"],
            Accepted: passed);
        var runtimeFlow = new DesignConversionRuntimeFlowResult(
            Request: submitted,
            CriticOutput: new DesignConversionCriticUnitOutput(
                OutputId: $"{request.RequestId}:critic",
                Verdict: finalVerdict,
                QaEvidenceRefs: qaResults.Select(result => result.ResultId).ToArray(),
                AiTeacherEvidenceRefs: request.AiTeacherEvidenceRefs,
                DeterministicLogicClaimedAsLearning: false),
            ImproverOutput: new DesignConversionImproverUnitOutput(
                OutputId: $"{request.RequestId}:improver",
                Candidate: candidate,
                Output: output,
                QaEvidenceRefs: qaResults.Select(result => result.ResultId).ToArray(),
                AiTeacherEvidenceRefs: request.AiTeacherEvidenceRefs,
                DeterministicLogicClaimedAsLearning: false),
            Candidate: candidate,
            Output: output,
            UnitExecutionOrder: ["scripted-critic", "scripted-improver"],
            EvidenceRefs: [$"runtime-flow:{request.RequestId}"],
            DeterministicLogicClaimedAsLearning: false);

        return new DesignConversionInternalEvaluationResult(
            JobId: request.JobId,
            SubmittedRequest: submitted,
            RenderResults: renderResults,
            ComparisonArtifacts: comparisons,
            VisualJudgeResults: judges,
            QaResults: qaResults,
            RuntimeFlowResult: runtimeFlow,
            Verdict: finalVerdict,
            ImprovedHtml: request.Html,
            ImprovedCss: request.Css,
            EvidenceRefs:
            [
                $"evaluation:{request.RequestId}",
                DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker
            ],
            Warnings: boundary.Warnings,
            ScaffoldMarkers: new Dictionary<string, string>(),
            DeterministicLogicClaimedAsLearning: false,
            VisualQaBoundaryTrace: boundary);
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

    private static RenderQaImageRecord Image(string id, string label, RenderQaResolution? resolution)
        => new(
            ImageId: id,
            Label: label,
            Resolution: resolution!,
            Uri: $"artifact://semantics/{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static VisualVerdict Verdict(
        string id,
        double score,
        bool passed,
        string expectedImageId,
        string actualImageId)
        => new(
            VerdictId: id,
            Score: score,
            Defects: passed
                ? []
                :
                [
                    new VisualDefect(
                        DefectId: $"{id}:defect",
                        Category: "quality",
                        Severity: "minor",
                        Description: "Scripted deterministic visual quality gap.",
                        X: 0,
                        Y: 0,
                        Width: 1,
                        Height: 1,
                        EvidenceImageIds: [expectedImageId, actualImageId])
                ],
            EvidenceRefs: [$"verdict:{id}"],
            Passed: passed);

    private static DesignConversionVisualQaBoundaryTrace CreateBoundary(
        DesignConversionInternalEvaluateRequest request,
        ScriptedEvaluation script,
        IReadOnlyList<RenderQaResolution> required,
        IReadOnlyList<RenderQaResolution> observed)
    {
        var requiredIds = required.Select(NormalizeResolutionId).ToArray();
        var observedIds = observed.Select(NormalizeResolutionId).ToArray();
        var missing = requiredIds.Except(observedIds, StringComparer.Ordinal).ToArray();
        var coverage = new DesignConversionViewportCoverage(
            IsMultiResolution: requiredIds.Length >= 2,
            IsComplete: script.CoverageComplete && missing.Length == 0,
            RequiredViewportIds: requiredIds,
            ObservedViewportIds: observedIds,
            MissingViewportIds: missing,
            EvidenceRefs:
            [
                DesignConversionVisualQaBoundaryPolicy.MultiResolutionRenderMarker,
                script.CoverageComplete ? "multi_resolution_render:complete" : "multi_resolution_render:incomplete"
            ]);
        var threshold = ReadThreshold(request.Budget);
        var stopReason = script.ProviderBlockedResult
            ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
            : !coverage.IsComplete
            ? DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete
            : script.Score >= threshold
                ? DesignConversionVisualQaBoundaryPolicy.QualityReached
                : script.CostUsd >= request.Budget.BudgetUsd
                    ? DesignConversionVisualQaBoundaryPolicy.BudgetCapReached
                    : DesignConversionVisualQaBoundaryPolicy.ContinueImprovement;
        var shouldContinue = string.Equals(stopReason, DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, StringComparison.Ordinal);
        var retryMode = stopReason switch
        {
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete => DesignConversionVisualQaBoundaryPolicy.RetryModeRetrySameCandidate,
            DesignConversionVisualQaBoundaryPolicy.ContinueImprovement => DesignConversionVisualQaBoundaryPolicy.RetryModeImproveCandidate,
            _ => DesignConversionVisualQaBoundaryPolicy.RetryModeNone
        };
        var tierPolicy = new DesignConversionTierPolicyDecision(
            Tier: request.Budget.Tier,
            RequiredScore: threshold,
            BestScore: script.Score,
            EstimatedCostUsd: script.CostUsd,
            BudgetUsd: request.Budget.BudgetUsd,
            RemainingBudgetUsd: Math.Max(0m, request.Budget.BudgetUsd - script.CostUsd),
            AttemptCount: observed.Count,
            MaxImprovementCandidates: request.Budget.MaxImprovementCandidates,
            StopReason: stopReason,
            ShouldContinue: shouldContinue,
            RetryMode: retryMode,
            EvidenceRefs:
            [
                DesignConversionVisualQaBoundaryPolicy.TierPolicyMarker,
                $"tier_policy:stop_reason={stopReason}",
                $"tier_policy:retry_mode={retryMode}",
                DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker
            ],
            Metadata: new Dictionary<string, string>
            {
                ["submitted_budget_usd"] = request.Budget.BudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["submitted_max_improvement_candidates"] = request.Budget.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture),
                ["effective_budget_usd"] = request.Budget.BudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["effective_max_improvement_candidates"] = request.Budget.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture),
                ["budget_stop_source"] = stopReason == DesignConversionVisualQaBoundaryPolicy.BudgetCapReached ? "cost_budget" : "none",
                ["retry_mode"] = retryMode,
                ["product_ready"] = bool.FalseString
            });
        var decision = stopReason switch
        {
            DesignConversionVisualQaBoundaryPolicy.QualityReached => DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
            DesignConversionVisualQaBoundaryPolicy.ContinueImprovement => DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete => DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine,
            _ => DesignConversionVisualQaBoundaryPolicy.DecisionBlocked
        };
        var visualDecision = new DesignConversionVisualComparisonDecision(
            Decision: decision,
            QualityScore: script.Score,
            RequiredThreshold: threshold,
            EstimatedCostUsd: script.CostUsd,
            BudgetUsd: request.Budget.BudgetUsd,
            RemainingBudgetUsd: Math.Max(0m, request.Budget.BudgetUsd - script.CostUsd),
            CostBudgetMet: script.CostUsd <= request.Budget.BudgetUsd,
            QualityThresholdMet: script.Score >= threshold,
            CanRetry: shouldContinue,
            CanPersistTrainingCandidate: decision is DesignConversionVisualQaBoundaryPolicy.DecisionAccepted or DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
            ProviderBoundaryStatus: script.ProviderBlockedResult
                ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                : DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            ExternalAiVisualJudgeExecuted: false,
            CorrectionFindings: BuildFindings(coverage, decision, script.RetryFindings),
            EvidenceRefs:
            [
                "visual_comparison_decision",
                $"visual_decision:{decision}",
                DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker
            ],
            Metadata: new Dictionary<string, string>
            {
                ["visual_decision"] = decision,
                ["retry_mode"] = retryMode,
                ["external_ai_visual_judge_executed"] = bool.FalseString
            });

        return new DesignConversionVisualQaBoundaryTrace(
            MultiResolutionRender: coverage,
            VisualJudgeStatus: script.ProviderBlockedResult
                ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                : "SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED",
            VisualJudgeProviderBoundary: script.ProviderBlockedResult
                ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                : DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            ExternalAiVisualJudgeExecuted: false,
            TierPolicy: tierPolicy,
            VisualDecision: visualDecision,
            ProductReady: false,
            TrainedFeatureClaimed: false,
            EvidenceRefs:
            [
                DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker,
                DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker,
                $"scripted_score:{script.Score.ToString("0.####", CultureInfo.InvariantCulture)}"
            ],
            Warnings: coverage.IsComplete
                ? ["PRODUCT_NOT_READY_REAL_PROVIDER_BROWSER_E2E_TRAINED_WEIGHTS_NOT_PROVEN"]
                : ["MULTI_RESOLUTION_RENDER_EVIDENCE_INCOMPLETE", "PRODUCT_NOT_READY_REAL_PROVIDER_BROWSER_E2E_TRAINED_WEIGHTS_NOT_PROVEN"]);
    }

    private static double ResolveQaScore(ScriptedEvaluation script, int index)
    {
        if (script.QaVerdictScores.Count == 0)
            return script.Score;

        return index < script.QaVerdictScores.Count
            ? script.QaVerdictScores[index]
            : script.QaVerdictScores[^1];
    }

    private static bool IsFiniteScore(double score)
        => !double.IsNaN(score) && !double.IsInfinity(score);

    private static IReadOnlyList<DesignConversionVisualCorrectionFinding> BuildFindings(
        DesignConversionViewportCoverage coverage,
        string decision,
        IReadOnlyList<DesignConversionVisualCorrectionFinding> scriptedRetryFindings)
    {
        if (!coverage.IsComplete)
        {
            return coverage.MissingViewportIds
                .Select(id => new DesignConversionVisualCorrectionFinding(
                    FindingId: $"missing:{id}",
                    Category: DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
                    Severity: "blocker",
                    Description: $"Missing viewport {id}.",
                    SuggestedAction: "Render all required viewports.",
                    EvidenceImageIds: [],
                    CanDriveRetry: false))
                .ToArray();
        }

        if (decision == DesignConversionVisualQaBoundaryPolicy.DecisionRetry)
        {
            if (scriptedRetryFindings.Count > 0)
                return scriptedRetryFindings;

            return
            [
                new DesignConversionVisualCorrectionFinding(
                    FindingId: "quality-below-threshold",
                    Category: "quality",
                    Severity: "minor",
                    Description: "Score below threshold.",
                    SuggestedAction: "Improve candidate.",
                    EvidenceImageIds: [],
                    CanDriveRetry: true)
            ];
        }

        return [];
    }

    private static DesignConversionVisualCorrectionFinding UnchangedRuntimeRetryFinding()
        => new(
            FindingId: "unchanged-runtime-retry-spacing",
            Category: "spacing",
            Severity: "minor",
            Description: "Not enough padding; elements are too close.",
            SuggestedAction: "Increase spacing before the next runtime evaluation.",
            EvidenceImageIds: [],
            CanDriveRetry: true);

    private static double ReadThreshold(DesignConversionBudgetMetadata budget)
        => budget.Metadata.TryGetValue("quality_threshold", out var value) &&
           double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0.90;

    private static string NormalizeResolutionId(RenderQaResolution resolution)
        => string.IsNullOrWhiteSpace(resolution.ResolutionId)
            ? $"{resolution.Width.ToString(CultureInfo.InvariantCulture)}x{resolution.Height.ToString(CultureInfo.InvariantCulture)}@{resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)}"
            : resolution.ResolutionId;
}
