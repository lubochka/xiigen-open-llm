// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionImprovementLoopCoordinatorTests
{
    [Fact]
    public async Task RunAsync_UpdatesCandidateAndStopsWhenSecondAttemptReachesQuality()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(0.82);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.94, cost: 0.01m)
                : JudgeScript.FromScore(0.82, cost: 0.01m));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-002")));
        var coordinator = CreateCoordinator(renderProvider, comparisonProvider, visualJudge, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-quality-loop",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.True(result.Value.Completed);
        Assert.True(result.Value.QualityReached);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Single(strategy.Requests);
        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, result.Value.Attempts[0].StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value.Attempts[1].StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, result.Value.Attempts[0].PublicMetadata["visual_decision"]);
        Assert.Equal(bool.TrueString, result.Value.Attempts[0].PublicMetadata["visual_decision_can_retry"]);
        Assert.Equal("quality", result.Value.Attempts[0].PublicMetadata["visual_decision_correction_categories"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, result.Value.Attempts[1].PublicMetadata["visual_decision"]);
        var candidateRequest = Assert.Single(strategy.Requests);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
            candidateRequest.CurrentEvaluation.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.Contains(candidateRequest.CurrentEvaluation.VisualQaBoundaryTrace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == "quality" &&
            finding.CanDriveRetry);
        Assert.Equal("<main class=\"improved\">PRIVATE_HTML_MARKER_FIXED</main>", result.Value.FinalEvaluation!.SubmittedRequest.Html);
    }

    [Fact]
    public async Task RunAsync_FreeTierEffectiveAttemptCapStopsBeforeNextCandidateAndPublishesRefsOnlyPolicyMetadata()
    {
        const string privateMarker = "PRIVATE_TIER_POLICY_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0.01m));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-should-not-run")));
        var coordinator = CreateCoordinator(renderProvider, comparisonProvider, visualJudge, strategy);
        var request = CreateRequest(privateMarker, privateMarker, privateMarker) with
        {
            Budget = new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "free",
                MaxImprovementCandidates: 10,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                })
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass-tier-free-budget-loop",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.False(result.Value.QualityReached);
        Assert.True(result.Value.BudgetCapReached);
        Assert.False(result.Value.MaxAttemptsReached);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Empty(strategy.Requests);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);

        var attempt = Assert.Single(result.Value.Attempts);
        Assert.Equal("free", attempt.PublicMetadata["tier"]);
        Assert.Equal("0.8", attempt.PublicMetadata["required_score"]);
        Assert.Equal("1", attempt.PublicMetadata["max_improvement_candidates"]);
        Assert.Equal("10", attempt.PublicMetadata["submitted_max_improvement_candidates"]);
        Assert.Equal("0.05", attempt.PublicMetadata["effective_budget_usd"]);
        Assert.Equal("1", attempt.PublicMetadata["effective_max_improvement_candidates"]);
        Assert.Equal("1", attempt.PublicMetadata["submitted_budget_usd"]);
        Assert.Equal("iteration_budget", attempt.PublicMetadata["budget_stop_source"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["tier_policy_iteration_cap_applied"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["raw_design_tree_included"]);
        Assert.Equal("free", result.Value.PublicMetadata["tier"]);
        Assert.Equal("0.8", result.Value.PublicMetadata["final_required_score"]);
        Assert.Equal("0.05", result.Value.PublicMetadata["final_budget_usd"]);
        Assert.Equal("0.03", result.Value.PublicMetadata["final_remaining_budget_usd"]);
        Assert.Equal("1", result.Value.PublicMetadata["effective_max_improvement_candidates"]);
        Assert.Equal("10", result.Value.PublicMetadata["submitted_max_improvement_candidates"]);
        Assert.Equal("iteration_budget", result.Value.PublicMetadata["budget_stop_source"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["raw_design_tree_included"]);
        Assert.DoesNotContain(privateMarker, string.Join("|", result.Value.EvidenceRefs), StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, string.Join("|", result.Value.PublicMetadata.Values), StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, string.Join("|", attempt.PublicMetadata.Values), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_FreeTierZeroBudgetAndIterationDefaultsPublishEntitlementMetadata()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0.01m));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-should-not-run")));
        var coordinator = CreateCoordinator(renderProvider, comparisonProvider, visualJudge, strategy);
        var request = CreateRequest() with
        {
            Budget = new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "free",
                MaxImprovementCandidates: 0,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0m,
                Metadata: new Dictionary<string, string>())
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass-tier-free-entitlement-default-loop",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, result.Value!.StopReason);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Empty(strategy.Requests);

        var attempt = Assert.Single(result.Value.Attempts);
        Assert.Equal("0", attempt.PublicMetadata["submitted_budget_usd"]);
        Assert.Equal("0", attempt.PublicMetadata["submitted_max_improvement_candidates"]);
        Assert.Equal("0.05", attempt.PublicMetadata["effective_budget_usd"]);
        Assert.Equal("1", attempt.PublicMetadata["effective_max_improvement_candidates"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(bool.TrueString, attempt.PublicMetadata["tier_policy_iteration_entitlement_default_applied"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.FalseString, attempt.PublicMetadata["tier_policy_iteration_cap_applied"]);

        Assert.Equal("0", result.Value.PublicMetadata["submitted_budget_usd"]);
        Assert.Equal("0", result.Value.PublicMetadata["submitted_max_improvement_candidates"]);
        Assert.Equal("0.05", result.Value.PublicMetadata["effective_budget_usd"]);
        Assert.Equal("1", result.Value.PublicMetadata["effective_max_improvement_candidates"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata["tier_policy_iteration_entitlement_default_applied"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["tier_policy_iteration_cap_applied"]);
    }

    [Fact]
    public async Task RunAsync_ReevaluatesEachChangedCandidateAcrossTwoImprovementUpdates()
    {
        const string firstImprovedHtml = "<main class=\"improved-one\">PRIVATE_HTML_MARKER_STEP1</main>";
        const string firstImprovedCss = ".improved-one{display:grid;gap:8px;}";
        const string secondImprovedHtml = "<main class=\"improved-two\">PRIVATE_HTML_MARKER_STEP2</main>";
        const string secondImprovedCss = ".improved-two{display:grid;gap:16px;}";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(0.70);
        var visualJudge = new ScriptedVisualJudge(request =>
        {
            var jobId = request.Metadata["job_id"];
            if (jobId.Contains("loop-attempt:3", StringComparison.Ordinal))
                return JudgeScript.FromScore(0.94, cost: 0m);
            if (jobId.Contains("loop-attempt:2", StringComparison.Ordinal))
                return JudgeScript.FromScore(0.82, cost: 0m);
            return JudgeScript.FromScore(0.70, cost: 0m);
        });
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-002",
                firstImprovedHtml,
                firstImprovedCss)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-003",
                secondImprovedHtml,
                secondImprovedCss)));
        var coordinator = CreateCoordinator(renderProvider, comparisonProvider, visualJudge, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass152-two-update-quality-loop",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.True(result.Value.QualityReached);
        Assert.Equal(3, result.Value.AttemptCount);
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal(6, renderProvider.Requests.Count);
        Assert.Equal(6, comparisonProvider.Requests.Count);
        Assert.Equal(6, visualJudge.Requests.Count);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, result.Value.Attempts[0].StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, result.Value.Attempts[1].StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value.Attempts[2].StopReason);
        Assert.Contains("PRIVATE_HTML_MARKER", strategy.Requests[0].CurrentRequest.Html, StringComparison.Ordinal);
        Assert.Equal(firstImprovedHtml, strategy.Requests[1].CurrentRequest.Html);
        Assert.Equal(firstImprovedCss, strategy.Requests[1].CurrentRequest.Css);
        Assert.Contains("PRIVATE_HTML_MARKER", renderProvider.Requests[0].Html, StringComparison.Ordinal);
        Assert.Contains("PRIVATE_HTML_MARKER", renderProvider.Requests[1].Html, StringComparison.Ordinal);
        Assert.Equal(firstImprovedHtml, renderProvider.Requests[2].Html);
        Assert.Equal(firstImprovedHtml, renderProvider.Requests[3].Html);
        Assert.Equal(secondImprovedHtml, renderProvider.Requests[4].Html);
        Assert.Equal(secondImprovedHtml, renderProvider.Requests[5].Html);
        Assert.Equal(firstImprovedCss, renderProvider.Requests[2].Css);
        Assert.Equal(firstImprovedCss, renderProvider.Requests[3].Css);
        Assert.Equal(secondImprovedCss, renderProvider.Requests[4].Css);
        Assert.Equal(secondImprovedCss, renderProvider.Requests[5].Css);
        Assert.Equal(secondImprovedHtml, result.Value.FinalEvaluation!.SubmittedRequest.Html);
        Assert.Equal(secondImprovedCss, result.Value.FinalEvaluation.SubmittedRequest.Css);
        Assert.Equal("3", result.Value.PublicMetadata["attempt_count"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["external_ai_visual_judge_executed"]);
    }

    [Fact]
    public async Task RunAsync_MaxAttemptsAfterRegressionReturnsBestAcceptedIncumbent()
    {
        const string bestHtml = "<main class=\"best-accepted\">PRIVATE_HTML_MARKER_BEST</main>";
        const string bestCss = ".best-accepted{display:grid;gap:14px;}";
        const string regressedHtml = "<main class=\"regressed\">PRIVATE_HTML_MARKER_REGRESSED</main>";
        const string regressedCss = ".regressed{display:block;}";
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-best", bestHtml, bestCss)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-regressed", regressedHtml, regressedCss)));
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(request =>
            {
                var jobId = request.Metadata["job_id"];
                if (jobId.Contains("loop-attempt:3", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.60, cost: 0m);
                if (jobId.Contains("loop-attempt:2", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.84, cost: 0m);
                return JudgeScript.FromScore(0.70, cost: 0m);
            }),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-5-max-regression-best-incumbent",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        AssertFinalEvaluationFromCandidate(result.Value, bestHtml, bestCss, 0.84, "loop-attempt:2", "loop-attempt:3");
        Assert.Equal(bool.FalseString, result.Value.Attempts[2].PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("quality_regression", result.Value.Attempts[2].PublicMetadata["candidate_rejection_reason"]);
    }

    [Fact]
    public async Task RunAsync_BudgetCapAfterRegressionReturnsBestAcceptedIncumbent()
    {
        const string bestHtml = "<main class=\"budget-best\">PRIVATE_HTML_MARKER_BUDGET_BEST</main>";
        const string bestCss = ".budget-best{display:grid;gap:14px;}";
        const string regressedHtml = "<main class=\"budget-regressed\">PRIVATE_HTML_MARKER_BUDGET_REGRESSED</main>";
        const string regressedCss = ".budget-regressed{display:block;}";
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-budget-best", bestHtml, bestCss)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-budget-regressed", regressedHtml, regressedCss)));
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(request =>
            {
                var jobId = request.Metadata["job_id"];
                if (jobId.Contains("loop-attempt:3", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.60, cost: 0.005m);
                if (jobId.Contains("loop-attempt:2", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.84, cost: 0.005m);
                return JudgeScript.FromScore(0.70, cost: 0.005m);
            }),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-5-budget-regression-best-incumbent",
            InitialEvaluationRequest: CreateRequest() with
            {
                Budget = CreateBudget(maxImprovementCandidates: 10, maxRenderQaRuns: 2, budgetUsd: 0.03m)
            },
            MaxAttempts: 5,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, result.Value!.StopReason);
        Assert.True(result.Value.BudgetCapReached);
        Assert.False(result.Value.Completed);
        AssertFinalEvaluationFromCandidate(result.Value, bestHtml, bestCss, 0.84, "loop-attempt:2", "loop-attempt:3");
    }

    [Fact]
    public async Task RunAsync_NoProgressAfterRegressionReturnsBestAcceptedIncumbent()
    {
        const string bestHtml = "<main class=\"no-progress-best\">PRIVATE_HTML_MARKER_NO_PROGRESS_BEST</main>";
        const string bestCss = ".no-progress-best{display:grid;gap:14px;}";
        const string regressedHtml = "<main class=\"no-progress-regressed\">PRIVATE_HTML_MARKER_NO_PROGRESS_REGRESSED</main>";
        const string regressedCss = ".no-progress-regressed{display:block;}";
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-no-progress-best", bestHtml, bestCss)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-no-progress-regressed", regressedHtml, regressedCss)));
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(request =>
            {
                var jobId = request.Metadata["job_id"];
                if (jobId.Contains("loop-attempt:3", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.60, cost: 0m);
                if (jobId.Contains("loop-attempt:2", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.84, cost: 0m);
                return JudgeScript.FromScore(0.70, cost: 0m);
            }),
            strategy);
        var budget = CreateBudget() with
        {
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.90",
                ["max_consecutive_no_improvement"] = "1"
            }
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-5-no-progress-regression-best-incumbent",
            InitialEvaluationRequest: CreateRequest() with { Budget = budget },
            MaxAttempts: 5,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        AssertFinalEvaluationFromCandidate(result.Value, bestHtml, bestCss, 0.84, "loop-attempt:2", "loop-attempt:3");
    }

    [Fact]
    public async Task RunAsync_EvaluationFailureAfterIncumbentReturnsBestAcceptedIncumbent()
    {
        const string bestHtml = "<main class=\"eval-failure-best\">PRIVATE_HTML_MARKER_EVAL_FAILURE_BEST</main>";
        const string bestCss = ".eval-failure-best{display:grid;gap:14px;}";
        const string unevaluatedHtml = "<main class=\"eval-failure-last\">PRIVATE_HTML_MARKER_EVAL_FAILURE_LAST</main>";
        const string unevaluatedCss = ".eval-failure-last{display:block;}";
        var inner = new DesignConversionInternalServiceThinClientJobEvaluator(new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(request =>
            {
                var jobId = request.Metadata["job_id"];
                if (jobId.Contains("loop-attempt:2", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.84, cost: 0m);
                return JudgeScript.FromScore(0.70, cost: 0m);
            })));
        var evaluator = new FailingAfterCountEvaluator(inner, failOnCall: 3);
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-eval-failure-best", bestHtml, bestCss)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-eval-failure-last", unevaluatedHtml, unevaluatedCss)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-5-evaluation-failure-best-incumbent",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 5,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.EvaluationFailed, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.Equal(3, result.Value.AttemptCount);
        AssertFinalEvaluationFromCandidate(result.Value, bestHtml, bestCss, 0.84, "loop-attempt:2", "loop-attempt:3");
        Assert.Contains("IMPROVEMENT_LOOP_EVALUATION_FAILED:SCRIPTED_EVALUATION_FAILED", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_StrategyFailureAfterIncumbentReturnsBestAcceptedIncumbent()
    {
        const string bestHtml = "<main class=\"strategy-failure-best\">PRIVATE_HTML_MARKER_STRATEGY_FAILURE_BEST</main>";
        const string bestCss = ".strategy-failure-best{display:grid;gap:14px;}";
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-strategy-failure-best", bestHtml, bestCss)),
            OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Scripted strategy failure after incumbent.",
                "SCRIPTED_STRATEGY_FAILED"));
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(request =>
            {
                var jobId = request.Metadata["job_id"];
                if (jobId.Contains("loop-attempt:2", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.84, cost: 0m);
                return JudgeScript.FromScore(0.70, cost: 0m);
            }),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-5-strategy-failure-best-incumbent",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 5,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed, result.Value!.StopReason);
        Assert.True(result.Value.CandidateStrategyFailed);
        Assert.False(result.Value.Completed);
        AssertFinalEvaluationFromCandidate(result.Value, bestHtml, bestCss, 0.84, "loop-attempt:2", null);
        Assert.Contains("IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:SCRIPTED_STRATEGY_FAILED", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_QualityReachedCandidateRejectedByViewportRegressionReturnsIncumbent()
    {
        const string initialHtml = "<main class=\"quality-guard-initial\">PRIVATE_HTML_MARKER_QUALITY_GUARD_INITIAL</main>";
        const string initialCss = ".quality-guard-initial{display:grid;gap:10px;}";
        const string regressedDesktopHtml = "<main class=\"quality-guard-regressed\">PRIVATE_HTML_MARKER_QUALITY_GUARD_REGRESSED</main>";
        const string regressedDesktopCss = ".quality-guard-regressed{display:grid;gap:14px;}";
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-quality-guard-regressed",
                regressedDesktopHtml,
                regressedDesktopCss)));
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.90),
            new ScriptedVisualJudge(request =>
            {
                if (request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.90, cost: 0m);

                return string.Equals(request.RenderedImage.Resolution.ResolutionId, "desktop", StringComparison.Ordinal)
                    ? JudgeScript.FromScore(0.95, cost: 0m)
                    : JudgeScript.FromScore(0.85, cost: 0m);
            }),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-5-quality-reached-non-regression-guard",
            InitialEvaluationRequest: CreateRequest() with
            {
                Html = initialHtml,
                Css = initialCss
            },
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.False(result.Value.QualityReached);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value.Attempts[1].StopReason);
        Assert.Equal(bool.FalseString, result.Value.Attempts[1].PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("viewport_regression", result.Value.Attempts[1].PublicMetadata["candidate_rejection_reason"]);
        Assert.Contains("IMPROVEMENT_LOOP_POLICY_QUALITY_REACHED_REJECTED_BY_INCUMBENT_GUARD", result.Value.Warnings);
        AssertFinalEvaluationFromCandidate(result.Value, initialHtml, initialCss, 0.85, null, "loop-attempt:2");
    }

    [Fact]
    public async Task RunAsync_CoverageRetryBudgetStopReturnsIncumbentOutputAndKeepsCoverageEvidence()
    {
        const string initialHtml = "<main class=\"coverage-incumbent\">PRIVATE_HTML_MARKER_COVERAGE_INCUMBENT</main>";
        const string initialCss = ".coverage-incumbent{display:grid;gap:10px;}";
        const string incompleteHtml = "<main class=\"coverage-incomplete\">PRIVATE_HTML_MARKER_COVERAGE_INCOMPLETE</main>";
        const string incompleteCss = ".coverage-incomplete{display:block;}";
        var evaluator = new ScriptedCoverageStopEvaluator(
            initialScore: 0.70,
            incompleteCandidateScore: 0.60,
            incompleteCandidateCostUsd: 0.01m);
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-coverage-incomplete",
                incompleteHtml,
                incompleteCss)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-5-coverage-stop-retains-incumbent",
            InitialEvaluationRequest: CreateRequest() with
            {
                Html = initialHtml,
                Css = initialCss,
                Budget = CreateBudget(maxImprovementCandidates: 5, maxRenderQaRuns: 2, budgetUsd: 0.01m)
            },
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, result.Value!.StopReason);
        Assert.True(result.Value.BudgetCapReached);
        Assert.True(result.Value.RenderManifestIncomplete);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata["render_manifest_incomplete"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker, result.Value.EvidenceRefs);
        Assert.Contains("improvement_loop:coverage_retry_stop_source=cost_budget", result.Value.EvidenceRefs);
        Assert.Equal(bool.FalseString, result.Value.Attempts[1].PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("required_viewport_coverage_incomplete", result.Value.Attempts[1].PublicMetadata["candidate_rejection_reason"]);
        AssertFinalEvaluationFromCandidate(result.Value, initialHtml, initialCss, 0.70, null, "loop-attempt:2");
    }

    [Fact]
    public async Task RunAsync_CoverageRetryAcceptsRecoveredMetadataOnlyCoverageBeforeImproving()
    {
        const string improvedHtml = "<main class=\"metadata-coverage-improved\">PRIVATE_HTML_MARKER_METADATA_COVERAGE</main>";
        const string improvedCss = ".metadata-coverage-improved{display:grid;gap:20px;}";
        var evaluator = new MetadataOnlyCoverageRecoveryEvaluator();
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-metadata-coverage-improved",
                improvedHtml,
                improvedCss)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-coverage-recovery-metadata-only-incumbent",
            InitialEvaluationRequest: CreateRequest() with
            {
                Budget = CreateBudget(maxImprovementCandidates: 5, maxRenderQaRuns: 2, budgetUsd: 1.00m)
            },
            MaxAttempts: 4,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.True(result.Value.QualityReached);
        Assert.Equal(3, result.Value.AttemptCount);
        Assert.Equal(3, evaluator.Requests.Count);
        var strategyRequest = Assert.Single(strategy.Requests);
        Assert.True(strategyRequest.CurrentEvaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);
        Assert.Equal(2, strategyRequest.CurrentEvaluation.RenderResults.Count);
        Assert.Equal(2, strategyRequest.CurrentEvaluation.QaResults.Count);
        Assert.Equal(bool.TrueString, result.Value.Attempts[1].PublicMetadata["candidate_accepted_as_incumbent"]);
        Assert.Equal("accepted_required_viewport_coverage_completed", result.Value.Attempts[1].PublicMetadata["candidate_acceptance_decision"]);
        Assert.Equal("coverage_recovery_score_guarded", result.Value.Attempts[1].PublicMetadata["quality_non_regression_evidence_mode"]);
        Assert.Equal(bool.TrueString, result.Value.Attempts[1].PublicMetadata["required_viewport_coverage_complete"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["product_ready"]);
        Assert.Equal(improvedHtml, result.Value.FinalEvaluation!.SubmittedRequest.Html);
        Assert.Equal(improvedCss, result.Value.FinalEvaluation.SubmittedRequest.Css);
    }

    [Fact]
    public async Task ThinClientImprovementLoopEvaluatorReturnsCoordinatorFinalEvaluation()
    {
        const string bestHtml = "<main class=\"thin-client-best\">PRIVATE_HTML_MARKER_THIN_CLIENT_BEST</main>";
        const string bestCss = ".thin-client-best{display:grid;gap:14px;}";
        const string regressedHtml = "<main class=\"thin-client-regressed\">PRIVATE_HTML_MARKER_THIN_CLIENT_REGRESSED</main>";
        const string regressedCss = ".thin-client-regressed{display:block;}";
        var inner = new DesignConversionInternalServiceThinClientJobEvaluator(new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(request =>
            {
                var jobId = request.Metadata["job_id"];
                if (jobId.Contains("loop-attempt:3", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.60, cost: 0m);
                if (jobId.Contains("loop-attempt:2", StringComparison.Ordinal))
                    return JudgeScript.FromScore(0.84, cost: 0m);
                return JudgeScript.FromScore(0.70, cost: 0m);
            })));
        var evaluator = new DesignConversionImprovementLoopThinClientJobEvaluator(
            inner,
            new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-thin-client-best", bestHtml, bestCss)),
                OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-thin-client-regressed", regressedHtml, regressedCss))));

        var result = await evaluator.EvaluateAsync(CreateRequest() with
        {
            Budget = CreateBudget(maxImprovementCandidates: 3, maxRenderQaRuns: 2, budgetUsd: 1.00m)
        });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(bestHtml, result.Value!.SubmittedRequest.Html);
        Assert.Equal(bestCss, result.Value.SubmittedRequest.Css);
        Assert.Contains(ExtractPrivateHtmlMarker(bestHtml), result.Value.ImprovedHtml, StringComparison.Ordinal);
        Assert.Contains(bestCss, result.Value.ImprovedCss, StringComparison.Ordinal);
        Assert.DoesNotContain(ExtractPrivateHtmlMarker(regressedHtml), result.Value.ImprovedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(regressedCss, result.Value.ImprovedCss, StringComparison.Ordinal);
        Assert.Equal(0.84, result.Value.QaResults.Select(qa => qa.Verdict.Score).Min(), 4);
        Assert.Contains(DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopFinalEvaluationMarker, result.Value.EvidenceRefs);
        Assert.Contains(
            $"submit_path_improvement_loop_stop_reason={DesignConversionVisualQaBoundaryPolicy.BudgetCapReached}",
            result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidateForDpoSourceWithoutRawPrivatePayload()
    {
        const string originalHtmlMarker = "PRIVATE_HTML_MARKER_ORIGINAL";
        const string originalCssMarker = "PRIVATE_CSS_MARKER_ORIGINAL";
        const string originalTreeMarker = "PRIVATE_TREE_MARKER_ORIGINAL";
        const string firstImprovedHtml = "<main class=\"improved-one\">PRIVATE_HTML_MARKER_STEP1</main>";
        const string firstImprovedCss = ".improved-one{display:grid;gap:8px;}/* PRIVATE_CSS_MARKER_STEP1 */";
        const string secondImprovedHtml = "<main class=\"improved-two\">PRIVATE_HTML_MARKER_STEP2</main>";
        const string secondImprovedCss = ".improved-two{display:grid;gap:16px;}/* PRIVATE_CSS_MARKER_STEP2 */";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass154-loop-local-artifact-db");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(0.70);
        var visualJudge = new ScriptedVisualJudge(request =>
        {
            var jobId = request.Metadata["job_id"];
            if (jobId.Contains("loop-attempt:3", StringComparison.Ordinal))
                return JudgeScript.FromScore(0.94, cost: 0m);
            if (jobId.Contains("loop-attempt:2", StringComparison.Ordinal))
                return JudgeScript.FromScore(0.82, cost: 0m);
            return JudgeScript.FromScore(0.70, cost: 0m);
        });
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-002",
                firstImprovedHtml,
                firstImprovedCss)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-003",
                secondImprovedHtml,
                secondImprovedCss)));
        var coordinator = CreateCoordinator(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            artifactDb,
            () => new DateTimeOffset(2026, 7, 3, 12, 0, 0, TimeSpan.Zero));
        var originalRequest = CreateRequest(originalHtmlMarker, originalCssMarker, originalTreeMarker);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass154-loop-to-training-store",
            InitialEvaluationRequest: originalRequest,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.Equal(3, result.Value.AttemptCount);
        Assert.Equal(secondImprovedHtml, result.Value.FinalEvaluation!.SubmittedRequest.Html);
        Assert.Equal(secondImprovedCss, result.Value.FinalEvaluation.SubmittedRequest.Css);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);

        var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
        var records = await reloaded.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.Equal("pass128-job:loop-attempt:3", stored.JobId);
        Assert.Equal("candidate-003-request", stored.RequestId);
        Assert.Equal(DesignConversionSourceKind.Figma.ToString(), stored.SourceKind);
        Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
        Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);
        Assert.Equal(bool.TrueString, stored.Metadata["final_candidate_from_loop"]);
        Assert.Equal("3", stored.Metadata["attempt_count"]);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, stored.Metadata["review_status"]);
        Assert.Equal(bool.FalseString, stored.Metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
            stored.VisualJudgeStatus.Metadata["visual_decision"]);
        Assert.Equal("0", stored.VisualJudgeStatus.Metadata["visual_decision_correction_count"]);
        Assert.StartsWith("dcm-html_result-", stored.ConversionResult.HtmlRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-css_result-", stored.ConversionResult.CssRef, StringComparison.Ordinal);
        Assert.Equal(Sha256Ref(result.Value.FinalEvaluation.ImprovedHtml), stored.ConversionResult.HtmlSha256);
        Assert.Equal(Sha256Ref(result.Value.FinalEvaluation.ImprovedCss), stored.ConversionResult.CssSha256);
        Assert.NotEqual(Sha256Ref(originalRequest.Html), stored.ConversionResult.HtmlSha256);
        Assert.NotEqual(Sha256Ref(originalRequest.Css), stored.ConversionResult.CssSha256);

        var dpoCandidate = Assert.Single(stored.DpoCandidates);
        Assert.True(dpoCandidate.ProductNotReady);
        Assert.True(dpoCandidate.TrainingCandidateOnly);
        Assert.Empty(dpoCandidate.BlockReasons);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, dpoCandidate.TrainingRecord.Provenance.Metadata["review_status"]);
        Assert.Equal(bool.TrueString, dpoCandidate.TrainingRecord.Provenance.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.FalseString, dpoCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, dpoCandidate.TrainingRecord.Provenance.Metadata["product_not_ready"]);
        Assert.True(dpoCandidate.UsefulnessScore > 0.0);
        Assert.StartsWith("dcm-chosen_html-", dpoCandidate.TrainingRecord.Chosen.Html, StringComparison.Ordinal);
        Assert.StartsWith("dcm-rejected_html-", dpoCandidate.TrainingRecord.Rejected.Html, StringComparison.Ordinal);

        var approved = await reloaded.QueryApprovedTrainingCandidatesAsync();
        Assert.True(approved.IsSuccess, approved.ErrorMessage);
        Assert.Empty(approved.Value!);

        var usefulnessBeforeApproval = await new DesignConversionDpoUsefulnessApi(reloaded).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        Assert.True(usefulnessBeforeApproval.IsSuccess, usefulnessBeforeApproval.ErrorMessage);
        Assert.Equal(0, usefulnessBeforeApproval.Value!.TrainReadyCount);
        Assert.Contains("runtime_created_dpo_candidate_review_pending", usefulnessBeforeApproval.Value.Warnings);
        var usefulnessRowBeforeApproval = Assert.Single(usefulnessBeforeApproval.Value.Rows);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, usefulnessRowBeforeApproval.ReviewStatus);
        Assert.True(usefulnessRowBeforeApproval.ReviewRequired);
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalPendingStatus, usefulnessRowBeforeApproval.ApprovalStatus);
        Assert.False(usefulnessRowBeforeApproval.TrainReady);
        Assert.True(usefulnessRowBeforeApproval.ProductNotReady);
        Assert.False(usefulnessRowBeforeApproval.ProductionDpo);
        Assert.False(usefulnessRowBeforeApproval.ModelReadinessClaimed);
        Assert.Equal(bool.FalseString, usefulnessRowBeforeApproval.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, usefulnessRowBeforeApproval.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, usefulnessRowBeforeApproval.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, usefulnessRowBeforeApproval.BoundaryMetadata["provider_payload_included"]);

        var routeBeforeApproval = await InvokeDpoUsefulnessRouteAsync(reloaded);
        AssertLoopCreatedDpoUsefulnessRoute(
            routeBeforeApproval,
            JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus,
            DesignConversionDpoUsefulnessApi.ApprovalPendingStatus,
            expectedTrainReady: false,
            expectedTrainReadyCount: 0,
            expectedBlockedCount: 1,
            originalHtmlMarker,
            originalCssMarker,
            originalTreeMarker,
            secondImprovedHtml,
            secondImprovedCss);

        var readbackBeforeApproval = await new DesignConversionDpoUsefulnessApi(reloaded).QueryLocalArtifactReadbackAsync(
            new DesignConversionLocalArtifactReadbackApiRequest(JobId: stored.JobId));
        Assert.True(readbackBeforeApproval.IsSuccess, readbackBeforeApproval.ErrorMessage);
        Assert.Equal(1, readbackBeforeApproval.Value!.DpoCandidateCount);
        Assert.Contains("runtime_created_dpo_candidate_review_pending", readbackBeforeApproval.Value.Warnings);
        var readbackRowBeforeApproval = Assert.Single(readbackBeforeApproval.Value.Rows);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, readbackRowBeforeApproval.DpoReviewStatus);
        Assert.True(readbackRowBeforeApproval.DpoReviewRequired);
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalPendingStatus, readbackRowBeforeApproval.DpoApprovalStatus);
        Assert.True(readbackRowBeforeApproval.TrainingCandidateOnly);
        Assert.True(readbackRowBeforeApproval.ProductNotReady);
        Assert.False(readbackRowBeforeApproval.ProductionDpo);
        Assert.False(readbackRowBeforeApproval.ModelReadinessClaimed);
        Assert.Equal(bool.FalseString, readbackRowBeforeApproval.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, readbackRowBeforeApproval.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, readbackRowBeforeApproval.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, readbackRowBeforeApproval.BoundaryMetadata["provider_payload_included"]);

        var handoffBeforeApproval = await new DesignConversionDpoUsefulnessApi(reloaded).ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId),
            DateTimeOffset.UnixEpoch.AddMinutes(155));
        Assert.True(handoffBeforeApproval.IsSuccess, handoffBeforeApproval.ErrorMessage);
        Assert.Equal(0, handoffBeforeApproval.Value!.ApprovedCandidateCount);
        Assert.Empty(handoffBeforeApproval.Value.Rows);

        var approval = await new DesignConversionDpoUsefulnessApi(reloaded).ApproveRuntimeCreatedDpoCandidateAsync(
            stored.RecordId,
            dpoCandidate.CandidateId,
            "reviewer:pass155-local",
            "loop-created DPO candidate usefulness reviewed for local training-intake handoff only",
            DateTimeOffset.UnixEpoch.AddMinutes(155));
        Assert.True(approval.IsSuccess, approval.ErrorMessage);
        Assert.Equal("approved", approval.Value!.ApprovalStatus);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewApprovedStatus, approval.Value.ReviewStatus);
        Assert.True(approval.Value.ApprovedForTraining);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["provider_payload_included"]);

        var approvedAfterApproval = await reloaded.QueryApprovedTrainingCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId));
        Assert.True(approvedAfterApproval.IsSuccess, approvedAfterApproval.ErrorMessage);
        var approvedRecord = Assert.Single(approvedAfterApproval.Value!);
        Assert.Equal(dpoCandidate.TrainingRecord.RecordId, approvedRecord.RecordId);
        Assert.Equal("approved", approvedRecord.Provenance.Metadata["approval_status"]);
        Assert.Equal(bool.TrueString, approvedRecord.Provenance.Metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, approvedRecord.Provenance.Metadata["product_not_ready"]);

        var usefulnessAfterApproval = await new DesignConversionDpoUsefulnessApi(reloaded).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        Assert.True(usefulnessAfterApproval.IsSuccess, usefulnessAfterApproval.ErrorMessage);
        Assert.Equal(1, usefulnessAfterApproval.Value!.TrainReadyCount);
        var usefulnessRowAfterApproval = Assert.Single(usefulnessAfterApproval.Value.Rows);
        Assert.Equal("train-ready", usefulnessRowAfterApproval.ReviewStatus);
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus, usefulnessRowAfterApproval.ApprovalStatus);
        Assert.True(usefulnessRowAfterApproval.TrainReady);
        Assert.True(usefulnessRowAfterApproval.ProductNotReady);
        Assert.False(usefulnessRowAfterApproval.ProductionDpo);
        Assert.False(usefulnessRowAfterApproval.ModelReadinessClaimed);

        var routeAfterApproval = await InvokeDpoUsefulnessRouteAsync(reloaded);
        AssertLoopCreatedDpoUsefulnessRoute(
            routeAfterApproval,
            "train-ready",
            DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus,
            expectedTrainReady: true,
            expectedTrainReadyCount: 1,
            expectedBlockedCount: 0,
            originalHtmlMarker,
            originalCssMarker,
            originalTreeMarker,
            secondImprovedHtml,
            secondImprovedCss);

        var handoffAfterApproval = await new DesignConversionDpoUsefulnessApi(reloaded).ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId),
            DateTimeOffset.UnixEpoch.AddMinutes(156));
        Assert.True(handoffAfterApproval.IsSuccess, handoffAfterApproval.ErrorMessage);
        var handoffRow = Assert.Single(handoffAfterApproval.Value!.Rows);
        Assert.Equal(1, handoffAfterApproval.Value.ApprovedCandidateCount);
        Assert.Equal("approved", handoffRow.ApprovalStatus);
        Assert.True(handoffRow.TrainingHandoffOnly);
        Assert.Equal(bool.FalseString, handoffRow.BoundaryMetadata["model_readiness_claimed"]);
        Assert.Equal(bool.FalseString, handoffRow.BoundaryMetadata["provider_payload_included"]);

        var intake = await new DesignConversionDpoUsefulnessApi(reloaded).CreateTrainingIntakeAsync(
            handoffAfterApproval.Value,
            "requester:pass155-local",
            "local loop-created candidate intake proof; no upload readiness without source-disjoint and measurement gates",
            DateTimeOffset.UnixEpoch.AddMinutes(157));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        Assert.Equal(1, intake.Value!.AcceptedCandidateCount);
        Assert.True(intake.Value.TrainingIntakeOnly);
        Assert.True(intake.Value.TrainingNotStarted);
        Assert.True(intake.Value.ModelUploadNotReady);
        Assert.True(intake.Value.ProductNotReady);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["trained_model_ready"]);
        Assert.Equal(bool.FalseString, intake.Value.BoundaryMetadata["model_readiness_claimed"]);

        var preflight = await reloaded.RunTrainingIntakePreflightAsync(intake.Value.IntakeId);
        Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
        Assert.False(preflight.Value!.MayStartTraining);
        Assert.False(preflight.Value.ModelUploadReady);
        Assert.Equal("blocked_same_source", preflight.Value.SourceDisjointStatus);
        Assert.Contains("source_disjoint_blocked_same_source", preflight.Value.BlockReasons);

        var run = await reloaded.RunTrainingIntakeAsync(intake.Value.IntakeId);
        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.False(run.Value!.TrainingRunnerInvoked);
        Assert.False(run.Value.TrainingStarted);
        Assert.False(run.Value.CheckpointMutated);
        Assert.False(run.Value.WeightsProduced);
        Assert.False(run.Value.ModelUploadReady);
        Assert.Equal("blocked_before_runner_by_training_intake_preflight", run.Value.TrainingExecutionBoundary);
        Assert.Contains("source_disjoint_blocked_same_source", run.Value.BlockReasons);

        var persistedJsonl = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
        var serializedUsefulnessBeforeApproval = JsonSerializer.Serialize(usefulnessBeforeApproval.Value);
        var serializedReadbackBeforeApproval = JsonSerializer.Serialize(readbackBeforeApproval.Value);
        var serializedRouteBeforeApproval = routeBeforeApproval.BodyJson;
        var serializedApproval = JsonSerializer.Serialize(approval.Value);
        var serializedRouteAfterApproval = routeAfterApproval.BodyJson;
        var serializedHandoffAfterApproval = JsonSerializer.Serialize(handoffAfterApproval.Value);
        var serializedIntake = JsonSerializer.Serialize(intake.Value);
        var serializedRun = JsonSerializer.Serialize(run.Value);
        Assert.DoesNotContain(originalHtmlMarker, persistedJsonl, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, persistedJsonl, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, persistedJsonl, StringComparison.Ordinal);
        Assert.DoesNotContain(firstImprovedHtml, persistedJsonl, StringComparison.Ordinal);
        Assert.DoesNotContain(firstImprovedCss, persistedJsonl, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, persistedJsonl, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, persistedJsonl, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", persistedJsonl, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", persistedJsonl, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(originalHtmlMarker, serializedUsefulnessBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, serializedUsefulnessBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, serializedUsefulnessBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, serializedUsefulnessBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, serializedUsefulnessBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", serializedUsefulnessBeforeApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", serializedUsefulnessBeforeApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(originalHtmlMarker, serializedReadbackBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, serializedReadbackBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, serializedReadbackBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, serializedReadbackBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, serializedReadbackBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", serializedReadbackBeforeApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", serializedReadbackBeforeApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(originalHtmlMarker, serializedRouteBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, serializedRouteBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, serializedRouteBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, serializedRouteBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, serializedRouteBeforeApproval, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", serializedRouteBeforeApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", serializedRouteBeforeApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(originalHtmlMarker, serializedApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, serializedApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, serializedApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, serializedApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, serializedApproval, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", serializedApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(originalHtmlMarker, serializedRouteAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, serializedRouteAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, serializedRouteAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, serializedRouteAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, serializedRouteAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", serializedRouteAfterApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", serializedRouteAfterApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(originalHtmlMarker, serializedHandoffAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, serializedHandoffAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, serializedHandoffAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, serializedHandoffAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, serializedHandoffAfterApproval, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", serializedHandoffAfterApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", serializedHandoffAfterApproval, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(originalHtmlMarker, serializedIntake, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, serializedIntake, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, serializedIntake, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, serializedIntake, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, serializedIntake, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", serializedIntake, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(originalHtmlMarker, serializedRun, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, serializedRun, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, serializedRun, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedHtml, serializedRun, StringComparison.Ordinal);
        Assert.DoesNotContain(secondImprovedCss, serializedRun, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", serializedRun, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", serializedRun, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_StopsOnBudgetCapWithoutRequestingAnotherCandidate()
    {
        var strategy = new ScriptedCandidateStrategy();
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.62),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.62, cost: 0.01m)),
            strategy);
        var request = CreateRequest() with
        {
            Budget = CreateBudget(budgetUsd: 0.02m)
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-budget-cap",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, result.Value!.StopReason);
        Assert.True(result.Value.BudgetCapReached);
        Assert.False(result.Value.Completed);
        Assert.Empty(strategy.Requests);
        Assert.Single(result.Value.Attempts);
        Assert.Equal("0.02", result.Value.PublicMetadata["final_estimated_cost_usd"]);
    }

    [Fact]
    public async Task RunAsync_StopsOnProviderRequiredWithSanitizedBlockedTelemetry()
    {
        var strategy = new ScriptedCandidateStrategy();
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.0),
            new ScriptedVisualJudge(_ => JudgeScript.FromBlockedProvider()),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-provider-blocked",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value!.StopReason);
        Assert.True(result.Value.ProviderRequiredOrBlocked);
        Assert.False(result.Value.Completed);
        Assert.Empty(strategy.Requests);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["external_ai_visual_judge_executed"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value.Attempts[0].PublicMetadata["visual_judge_provider_boundary"]);
    }

    [Fact]
    public async Task RunAsync_StopsOnRenderManifestIncomplete()
    {
        var strategy = new ScriptedCandidateStrategy();
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.92),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.92, cost: 0m)),
            strategy);
        var request = CreateRequest() with
        {
            Budget = CreateBudget(maxRenderQaRuns: 1, budgetUsd: 1.00m)
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-render-incomplete",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, result.Value!.StopReason);
        Assert.True(result.Value.RenderManifestIncomplete);
        Assert.False(result.Value.Completed);
        Assert.Empty(strategy.Requests);
        Assert.Single(result.Value.FinalEvaluation!.RenderResults);
    }

    [Fact]
    public async Task RunAsync_StopsAtCoordinatorMaxAttemptsEvenWhenPolicyWouldContinue()
    {
        var strategy = new ScriptedCandidateStrategy();
        var renderProvider = new RecordingRenderProvider();
        var coordinator = CreateCoordinator(
            renderProvider,
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0m)),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-max-attempts",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.True(result.Value.MaxAttemptsReached);
        Assert.False(result.Value.Completed);
        Assert.Empty(strategy.Requests);
        Assert.Single(result.Value.Attempts);
        Assert.Equal(2, renderProvider.Requests.Count);
    }

    [Fact]
    public async Task RunAsync_RecordsAttemptHistoryAndMaxAttemptStopWithoutProductReadyClaim()
    {
        var renderProvider = new RecordingRenderProvider();
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-002",
                "<main class=\"attempt-two\">PRIVATE_HTML_MARKER_STEP2</main>",
                ".attempt-two{display:grid;gap:8px;}")),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-003",
                "<main class=\"attempt-three\">PRIVATE_HTML_MARKER_STEP3</main>",
                ".attempt-three{display:grid;gap:16px;}")));
        var coordinator = CreateCoordinator(
            renderProvider,
            new RecordingComparisonProvider(0.72),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, cost: 0.01m)),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass-budget-history-max-attempts",
            InitialEvaluationRequest: CreateRequest() with
            {
                Budget = CreateBudget(maxImprovementCandidates: 10, maxRenderQaRuns: 2, budgetUsd: 1.00m)
            },
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.True(result.Value.MaxAttemptsReached);
        Assert.False(result.Value.Completed);
        Assert.False(result.Value.QualityReached);
        Assert.Equal(3, result.Value.AttemptCount);
        Assert.Equal("3", result.Value.PublicMetadata["attempt_count"]);
        Assert.Equal("3", result.Value.PublicMetadata["configured_max_attempts"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["product_ready"]);
        Assert.Equal(6, renderProvider.Requests.Count);
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Single(strategy.Requests[0].AttemptHistory);
        Assert.Equal(2, strategy.Requests[1].AttemptHistory.Count);
        Assert.All(result.Value.Attempts, attempt =>
        {
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, attempt.StopReason);
            Assert.True(attempt.ShouldContinue);
            Assert.Equal(bool.FalseString, attempt.PublicMetadata["product_ready"]);
            Assert.Equal(attempt.AttemptNumber, attempt.PolicyAttemptCount);
            Assert.Equal(
                attempt.AttemptNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
                attempt.PublicMetadata["policy_attempt_count"]);
            Assert.Equal("4", attempt.PublicMetadata["max_improvement_candidates"]);
            Assert.Equal("10", attempt.PublicMetadata["submitted_max_improvement_candidates"]);
        });
        Assert.Contains("IMPROVEMENT_LOOP_MAX_ATTEMPTS_REACHED_WITH_POLICY_STILL_CONTINUING", result.Value.Warnings);
        Assert.Contains(result.Value.EvidenceRefs, reference =>
            string.Equals(reference, "improvement_loop:stop_reason=max_attempts_reached", StringComparison.Ordinal));
        Assert.Contains(result.Value.EvidenceRefs, reference =>
            string.Equals(reference, "improvement_loop:not_completed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_BuildsLoopAttemptJobIdsFromRootWithoutCumulativeSuffixes()
    {
        var evaluator = new RecordingScriptedLoopEvaluator(attemptNumber => 0.40 + (attemptNumber * 0.01));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(2)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(3)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, strategy);
        var request = CreateRequest() with
        {
            JobId = "base",
            Budget = CreateBudget(maxImprovementCandidates: 10, maxRenderQaRuns: 2, budgetUsd: 10m) with
            {
                Tier = "enterprise"
            }
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-root-job-id",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.Equal(["base", "base:loop-attempt:2", "base:loop-attempt:3"], evaluator.Requests.Select(evaluationRequest => evaluationRequest.JobId).ToArray());
        Assert.Equal("base:loop-attempt:3", result.Value.FinalEvaluation!.JobId);
        Assert.DoesNotContain(":loop-attempt:2:loop-attempt:3", result.Value.FinalEvaluation.JobId, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_TreatsCallerLoopAttemptLikeJobIdAsRootWithoutStripping()
    {
        var evaluator = new RecordingScriptedLoopEvaluator(attemptNumber => 0.40 + (attemptNumber * 0.01));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(2)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(3)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, strategy);
        var request = CreateRequest() with
        {
            JobId = "caller-owned:loop-attempt:seed",
            Budget = CreateBudget(maxImprovementCandidates: 10, maxRenderQaRuns: 2, budgetUsd: 10m) with
            {
                Tier = "enterprise"
            }
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-root-job-id-caller-suffix",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal([
            "caller-owned:loop-attempt:seed",
            "caller-owned:loop-attempt:seed:loop-attempt:2",
            "caller-owned:loop-attempt:seed:loop-attempt:3"
        ], evaluator.Requests.Select(evaluationRequest => evaluationRequest.JobId).ToArray());
        Assert.Equal("caller-owned:loop-attempt:seed:loop-attempt:3", result.Value!.FinalEvaluation!.JobId);
    }

    [Fact]
    public async Task RunAsync_ServerCapJobIdsStayUniqueAndBoundedWithoutPrivatePayloadRefs()
    {
        const string privateHtmlMarker = "PRIVATE_HTML_MARKER_CAP50_SECRET";
        const string privateCssMarker = "PRIVATE_CSS_MARKER_CAP50_SECRET";
        const string privateTreeMarker = "PRIVATE_TREE_MARKER_CAP50_SECRET";
        var evaluator = new RecordingScriptedLoopEvaluator(attemptNumber => 0.40 + (attemptNumber * 0.001));
        var candidates = Enumerable.Range(2, DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts - 1)
            .Select(attemptNumber => OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(attemptNumber)))
            .ToArray();
        var strategy = new ScriptedCandidateStrategy(candidates);
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, strategy);
        var request = CreateRequest(privateHtmlMarker, privateCssMarker, privateTreeMarker) with
        {
            JobId = "base",
            Budget = CreateBudget(
                maxImprovementCandidates: DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts,
                maxRenderQaRuns: 2,
                budgetUsd: 100m) with
            {
                Tier = "enterprise"
            }
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-cap50-job-id-bounds",
            InitialEvaluationRequest: request,
            MaxAttempts: DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var jobIds = evaluator.Requests.Select(evaluationRequest => evaluationRequest.JobId).ToArray();
        var expectedJobIds = new[] { "base" }
            .Concat(Enumerable.Range(2, DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts - 1)
                .Select(attemptNumber => $"base:loop-attempt:{attemptNumber.ToString(System.Globalization.CultureInfo.InvariantCulture)}"))
            .ToArray();
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts, jobIds.Length);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts, jobIds.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(expectedJobIds, jobIds);
        Assert.All(jobIds.Skip(1), jobId =>
        {
            Assert.InRange(jobId.Length, "base:loop-attempt:2".Length, "base:loop-attempt:50".Length);
            Assert.Equal(1, jobId.Split(":loop-attempt:", StringSplitOptions.None).Length - 1);
        });

        var evidenceRefs = result.Value!.EvidenceRefs
            .Concat(result.Value.Attempts.SelectMany(attempt => attempt.EvidenceRefs))
            .Concat(result.Value.FinalEvaluation!.EvidenceRefs)
            .ToArray();
        Assert.All(jobIds.Concat(evidenceRefs), value =>
        {
            Assert.DoesNotContain(privateHtmlMarker, value, StringComparison.Ordinal);
            Assert.DoesNotContain(privateCssMarker, value, StringComparison.Ordinal);
            Assert.DoesNotContain(privateTreeMarker, value, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task RunAsync_ObservesCancellationWithoutCompletionClaim()
    {
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0m)),
            new ScriptedCandidateStrategy());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-cancelled",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()),
            cts.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.Cancelled, result.Value!.StopReason);
        Assert.True(result.Value.Cancelled);
        Assert.False(result.Value.DeadlineExceeded);
        Assert.False(result.Value.Completed);
        Assert.Empty(result.Value.Attempts);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["completed"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.DeadlineExceededMetadataKey]);
    }

    [Fact]
    public async Task RunAsync_BlockingEvaluatorDeadlineReturnsTypedDeadlineWithoutAttemptOrCallerCancel()
    {
        var evaluator = new BlockingEvaluator(throwOnCancellation: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(),
            runDeadline: TimeSpan.FromMilliseconds(50));

        var stopwatch = Stopwatch.StartNew();
        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass-deadline-evaluation-cancelled-result",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));
        stopwatch.Stop();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, result.Value!.StopReason);
        Assert.True(result.Value.DeadlineExceeded);
        Assert.False(result.Value.Cancelled);
        Assert.False(result.Value.Completed);
        Assert.Empty(result.Value.Attempts);
        Assert.Null(result.Value.FinalEvaluation);
        Assert.Single(evaluator.Requests);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.DeadlineExceededMetadataKey]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["cancelled"]);
        Assert.Equal("50", result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.ConfiguredRunDeadlineMsMetadataKey]);
        Assert.Contains(DesignConversionImprovementLoopCoordinator.DeadlineExceededMarker, result.Value.EvidenceRefs);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), $"deadline elapsed {stopwatch.Elapsed}");
    }

    [Fact]
    public async Task RunAsync_BlockingCandidateStrategyDeadlinePreservesCompletedAttemptAndStartsNoNextEvaluation()
    {
        var renderProvider = new RecordingRenderProvider();
        var strategy = new BlockingCandidateStrategy(throwOnCancellation: false);
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "deadline-skips-local-artifact-db");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var coordinator = CreateCoordinator(
            renderProvider,
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0m)),
            strategy,
            artifactDb: artifactDb,
            runDeadline: TimeSpan.FromMilliseconds(500));

        var stopwatch = Stopwatch.StartNew();
        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass-deadline-candidate-cancelled-result",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));
        stopwatch.Stop();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, result.Value!.StopReason);
        Assert.True(result.Value.DeadlineExceeded);
        Assert.False(result.Value.Cancelled);
        Assert.False(result.Value.Completed);
        var attempt = Assert.Single(result.Value.Attempts);
        Assert.Equal(1, attempt.AttemptNumber);
        Assert.NotNull(result.Value.FinalEvaluation);
        Assert.Equal(2, renderProvider.Requests.Count);
        var strategyRequest = Assert.Single(strategy.Requests);
        Assert.Equal(1, strategyRequest.CompletedAttemptNumber);
        Assert.Single(strategyRequest.AttemptHistory);
        Assert.Equal("500", result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.ConfiguredRunDeadlineMsMetadataKey]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbPersistenceSkippedDeadlineExceededMarker]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);
        Assert.Equal("0", result.Value.PublicMetadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["runtime_created_dpo_candidate"]);
        Assert.Contains(DesignConversionImprovementLoopCoordinator.LocalArtifactDbPersistenceSkippedDeadlineExceededMarker, result.Value.EvidenceRefs);
        Assert.Contains("improvement_loop:local_artifact_db_autowrite=false", result.Value.EvidenceRefs);
        Assert.Contains("improvement_loop:local_training_store_dpo_source=false", result.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, result.Value.EvidenceRefs);
        Assert.Contains("IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_DURING_CANDIDATE_STRATEGY", result.Value.Warnings);
        Assert.Contains("improvement_loop_local_artifact_db_persistence_skipped_deadline_exceeded=true", result.Value.Warnings);
        Assert.Contains("improvement_loop_local_artifact_db_autowrite=false", result.Value.Warnings);
        Assert.Contains("improvement_loop_local_training_store_dpo_source=false", result.Value.Warnings);
        Assert.DoesNotContain("improvement_loop_local_artifact_db_autowrite=true", result.Value.Warnings);
        Assert.DoesNotContain("improvement_loop_local_training_store_dpo_source=true", result.Value.Warnings);

        var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
        var records = await reloaded.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3), $"deadline elapsed {stopwatch.Elapsed}");
    }

    [Fact]
    public async Task RunAsync_ThrownOperationCanceledFromDeadlineMapsToTypedDeadline()
    {
        var evaluator = new BlockingEvaluator(throwOnCancellation: true);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(),
            runDeadline: TimeSpan.FromMilliseconds(50));

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass-deadline-evaluation-thrown-oce",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, result.Value!.StopReason);
        Assert.True(result.Value.DeadlineExceeded);
        Assert.False(result.Value.Cancelled);
        Assert.False(result.Value.Completed);
        Assert.Empty(result.Value.Attempts);
        Assert.Single(evaluator.Requests);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.DeadlineExceededMetadataKey]);
    }

    [Fact]
    public async Task RunAsync_StopsCleanlyOnCandidateStrategyFailure()
    {
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Fail(
                "No candidate available from local strategy.",
                "LOCAL_STRATEGY_NO_CANDIDATE"));
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0m)),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-strategy-failure",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed, result.Value!.StopReason);
        Assert.True(result.Value.CandidateStrategyFailed);
        Assert.False(result.Value.Completed);
        Assert.Single(strategy.Requests);
        Assert.Contains("IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:LOCAL_STRATEGY_NO_CANDIDATE", result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_RejectsNoOpCandidateAsNoProgressStop()
    {
        var initialRequest = CreateRequest();
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-no-op",
                initialRequest.Html,
                initialRequest.Css)));
        var renderProvider = new RecordingRenderProvider();
        var coordinator = CreateCoordinator(
            renderProvider,
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0m)),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-no-op-candidate",
            InitialEvaluationRequest: initialRequest,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.CandidateStrategyFailed);
        Assert.False(result.Value.Completed);
        Assert.Single(result.Value.Attempts);
        Assert.Single(strategy.Requests);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Contains("IMPROVEMENT_LOOP_CANDIDATE_VALIDATION_NO_PROGRESS:IMPROVEMENT_CANDIDATE_NO_OP", result.Value.Warnings);
        Assert.DoesNotContain(DesignConversionImprovementLoopCoordinator.CandidateRepeatedHistoryMarker, result.Value.Warnings);
        Assert.DoesNotContain("improvement_loop:candidate_repeated_history", result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task RunAsync_RejectsHistoricallyRepeatedCandidateBeforeEvaluationAndPreservesBestIncumbent()
    {
        const string privateHtmlMarker = "PRIVATE_HTML_MARKER_HISTORY_A";
        const string privateCssMarker = "PRIVATE_CSS_MARKER_HISTORY_A";
        const string privateTreeMarker = "PRIVATE_TREE_MARKER_HISTORY_A";
        const string improvedHtml = "<main class=\"history-b\">PRIVATE_HTML_MARKER_HISTORY_B</main>";
        const string improvedCss = ".history-b{display:grid;gap:18px;}";
        var initialRequest = CreateRequest(privateHtmlMarker, privateCssMarker, privateTreeMarker);
        var repeatedInitialHtml = initialRequest.Html;
        var repeatedInitialCss = initialRequest.Css;
        var evaluator = new RecordingScriptedLoopEvaluator(attemptNumber => attemptNumber switch
        {
            1 => 0.70,
            2 => 0.84,
            _ => 0.99
        });
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-history-b",
                improvedHtml,
                improvedCss)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-history-repeat-a",
                repeatedInitialHtml,
                repeatedInitialCss)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-6-oscillation-history-a-b-a",
            InitialEvaluationRequest: initialRequest,
            MaxAttempts: 5,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.False(result.Value.CandidateStrategyFailed);
        Assert.False(result.Value.Completed);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Equal(2, evaluator.Requests.Count);
        Assert.Equal(2, strategy.Requests.Count);
        AssertFinalEvaluationFromCandidate(result.Value, improvedHtml, improvedCss, 0.84, "loop-attempt:2", "loop-attempt:3");
        Assert.Contains(DesignConversionImprovementLoopCoordinator.CandidateRepeatedHistoryMarker, result.Value.Warnings);
        Assert.Contains(
            $"IMPROVEMENT_LOOP_CANDIDATE_VALIDATION_NO_PROGRESS:{DesignConversionImprovementLoopCoordinator.CandidateRepeatedHistoryMarker}",
            result.Value.Warnings);
        Assert.Contains("improvement_loop:candidate_repeated_history", result.Value.EvidenceRefs);
        Assert.DoesNotContain("loop-attempt:3", result.Value.FinalEvaluation!.JobId, StringComparison.Ordinal);

        var publicTelemetryJson = SerializePublicTelemetry(result.Value);

        Assert.DoesNotContain(privateHtmlMarker, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCssMarker, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateTreeMarker, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(improvedHtml, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(improvedCss, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(repeatedInitialHtml, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(repeatedInitialCss, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", publicTelemetryJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data-private", publicTelemetryJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_EvaluatesHistoricallySimilarCandidateWhenCssWhitespaceIsSemantic()
    {
        const string privateHtmlMarker = "PRIVATE_HTML_MARKER_HISTORY_SEMANTIC_A";
        const string privateCssMarker = "PRIVATE_CSS_MARKER_HISTORY_SEMANTIC_A";
        const string privateTreeMarker = "PRIVATE_TREE_MARKER_HISTORY_SEMANTIC_A";
        const string initialHtml = "<main data-private=\"PRIVATE_HTML_MARKER_HISTORY_SEMANTIC_A\"><div><span class=\"x\">Hello</span></div></main>";
        const string initialCss = "div .x{color:red;}";
        const string improvedHtml = "<main class=\"history-semantic-b\">PRIVATE_HTML_MARKER_HISTORY_SEMANTIC_B</main>";
        const string improvedCss = ".history-semantic-b{display:grid;gap:18px;}";
        const string semanticallyDifferentHtml = initialHtml;
        const string semanticallyDifferentCss = "div.x{color:red;}";
        var initialRequest = CreateRequest(privateHtmlMarker, privateCssMarker, privateTreeMarker) with
        {
            Html = initialHtml,
            Css = initialCss
        };
        var evaluator = new RecordingScriptedLoopEvaluator(attemptNumber => attemptNumber switch
        {
            1 => 0.70,
            2 => 0.84,
            3 => 0.82,
            _ => 0.99
        });
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-history-semantic-b",
                improvedHtml,
                improvedCss)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-history-semantic-a-prime",
                semanticallyDifferentHtml,
                semanticallyDifferentCss)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(evaluator, strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "n8-6-oscillation-history-semantic-a-b-a-prime",
            InitialEvaluationRequest: initialRequest,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.False(result.Value.CandidateStrategyFailed);
        Assert.False(result.Value.Completed);
        Assert.Equal(3, result.Value.AttemptCount);
        Assert.Equal(3, evaluator.Requests.Count);
        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal(semanticallyDifferentHtml, evaluator.Requests[2].Html);
        Assert.Equal(semanticallyDifferentCss, evaluator.Requests[2].Css);
        Assert.Contains(result.Value.Attempts, attempt => attempt.AttemptNumber == 3);
        AssertFinalEvaluationFromCandidate(result.Value, improvedHtml, improvedCss, 0.84, "loop-attempt:2", null);
        Assert.DoesNotContain(DesignConversionImprovementLoopCoordinator.CandidateRepeatedHistoryMarker, result.Value.Warnings);
        Assert.DoesNotContain("improvement_loop:candidate_repeated_history", result.Value.EvidenceRefs);

        var publicTelemetryJson = SerializePublicTelemetry(result.Value);
        Assert.DoesNotContain(privateHtmlMarker, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCssMarker, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateTreeMarker, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialHtml, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialCss, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(improvedHtml, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(improvedCss, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(semanticallyDifferentHtml, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(semanticallyDifferentCss, publicTelemetryJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_PublicTelemetryContainsOnlyRefsHashesScoresAndStatus()
    {
        const string rawHtml = "PRIVATE_HTML_MARKER";
        const string rawCss = "PRIVATE_CSS_MARKER";
        const string rawTree = "PRIVATE_TREE_MARKER";
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-telemetry")));
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.80),
            new ScriptedVisualJudge(request =>
                request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal)
                    ? JudgeScript.FromScore(0.95, cost: 0m)
                    : JudgeScript.FromScore(0.80, cost: 0m)),
            strategy);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-telemetry",
            InitialEvaluationRequest: CreateRequest(rawHtml, rawCss, rawTree),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var publicTelemetryJson = JsonSerializer.Serialize(new
        {
            result.Value!.PublicMetadata,
            result.Value.EvidenceRefs,
            Attempts = result.Value.Attempts.Select(attempt => new
            {
                attempt.PublicMetadata,
                attempt.EvidenceRefs,
                attempt.RenderResultRefs,
                attempt.ComparisonArtifactRefs,
                attempt.VisualJudgeResultRefs,
                attempt.CandidateRef,
                attempt.JobRef,
                attempt.RequestRef,
                attempt.BestScore,
                attempt.RequiredScore,
                attempt.EstimatedCostUsd,
                attempt.BudgetUsd,
                attempt.StopReason,
                attempt.ShouldContinue
            })
        });

        Assert.DoesNotContain(rawHtml, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawCss, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawTree, publicTelemetryJson, StringComparison.Ordinal);
        Assert.DoesNotContain("data:image", publicTelemetryJson, StringComparison.OrdinalIgnoreCase);
        Assert.All(result.Value.Attempts, attempt =>
        {
            Assert.Equal(bool.TrueString, attempt.PublicMetadata["refs_or_hashes_only"]);
            Assert.Equal(bool.FalseString, attempt.PublicMetadata["raw_html_included"]);
            Assert.Equal(bool.FalseString, attempt.PublicMetadata["raw_css_included"]);
            Assert.Equal(bool.FalseString, attempt.PublicMetadata["raw_design_tree_included"]);
            Assert.Equal(bool.FalseString, attempt.PublicMetadata["screenshot_bytes_included"]);
        });
    }

    private static DesignConversionImprovementLoopCoordinator CreateCoordinator(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge,
        IDesignConversionImprovementCandidateStrategy strategy,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        Func<DateTimeOffset>? utcNow = null,
        TimeSpan? runDeadline = null)
        => new(
            new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge),
            strategy,
            artifactDb,
            utcNow,
            runDeadline);

    private static async Task<DesignConversionModelServerHttpResponse> InvokeDpoUsefulnessRouteAsync(
        IDesignConversionLocalArtifactDb db)
    {
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionDpoUsefulness(
            new DesignConversionModelServerDpoUsefulnessHandler(new DesignConversionDpoUsefulnessApi(db)));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            BodyJson: null,
            ContentType: null,
            QueryString: "?includeBlocked=true&limit=10"));
        Assert.True(response.IsSuccess, response.ErrorMessage);

        return response.Value!;
    }

    private static void AssertLoopCreatedDpoUsefulnessRoute(
        DesignConversionModelServerHttpResponse response,
        string expectedReviewStatus,
        string expectedApprovalStatus,
        bool expectedTrainReady,
        int expectedTrainReadyCount,
        int expectedBlockedCount,
        string originalHtmlMarker,
        string originalCssMarker,
        string originalTreeMarker,
        string improvedHtml,
        string improvedCss)
    {
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.Equal(
            DesignConversionModelServerDpoUsefulnessHandler.LocalDpoUsefulnessRouteBoundary,
            response.Headers["x-design-conversion-dpo-usefulness-route"]);
        Assert.DoesNotContain(originalHtmlMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(improvedHtml, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(improvedCss, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:image", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", response.BodyJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal(DesignConversionModelServerDpoUsefulnessHandler.ApiShape, root.GetProperty("apiShape").GetString());
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApiShape, root.GetProperty("dpoUsefulnessApiShape").GetString());
        Assert.Equal(expectedTrainReadyCount, root.GetProperty("trainReadyCount").GetInt32());
        Assert.Equal(expectedBlockedCount, root.GetProperty("blockedCount").GetInt32());
        Assert.False(root.GetProperty("productReady").GetBoolean());
        Assert.Contains(root.GetProperty("warnings").EnumerateArray(), warning =>
            warning.GetString() == "raw_html_css_tree_private_provider_payloads_not_emitted");
        Assert.Contains(root.GetProperty("warnings").EnumerateArray(), warning =>
            warning.GetString() == "modelserver_route_delegates_to_domain_dpo_usefulness_api");

        var row = Assert.Single(root.GetProperty("rows").EnumerateArray());
        Assert.True(row.GetProperty("usefulnessScore").GetDouble() > 0);
        Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("usefulnessStatus").GetString()));
        Assert.Equal(expectedReviewStatus, row.GetProperty("reviewStatus").GetString());
        Assert.True(row.GetProperty("reviewRequired").GetBoolean());
        Assert.Equal(expectedApprovalStatus, row.GetProperty("approvalStatus").GetString());
        Assert.Equal(expectedTrainReady, row.GetProperty("trainReady").GetBoolean());
        Assert.True(row.GetProperty("trainingCandidateOnly").GetBoolean());
        Assert.True(row.GetProperty("productNotReady").GetBoolean());
        Assert.False(row.GetProperty("productionDpo").GetBoolean());
        Assert.False(row.GetProperty("modelReadinessClaimed").GetBoolean());
        Assert.True(row.GetProperty("heldOutNotProven").GetBoolean());
        Assert.True(row.GetProperty("sourceDisjointNotProven").GetBoolean());
        Assert.Contains(row.GetProperty("usefulnessFactors").EnumerateArray(), factor =>
            factor.GetString() == $"approval_status={expectedApprovalStatus}");
        var evidenceRefs = row.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(reference => reference.GetString() ?? string.Empty)
            .ToArray();
        Assert.NotEmpty(evidenceRefs);
        Assert.Contains(evidenceRefs, reference =>
            reference.StartsWith("design-conversion-dpo-display:", StringComparison.Ordinal));
        Assert.All(evidenceRefs, reference =>
        {
            Assert.DoesNotContain(originalHtmlMarker, reference, StringComparison.Ordinal);
            Assert.DoesNotContain(originalCssMarker, reference, StringComparison.Ordinal);
            Assert.DoesNotContain(originalTreeMarker, reference, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/Users/", reference, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("raw_provider_payload", reference, StringComparison.OrdinalIgnoreCase);
        });

        var rowMetadata = row.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, rowMetadata.GetProperty("runtime_created_dpo_candidate").GetString());
        Assert.Equal(expectedApprovalStatus, rowMetadata.GetProperty("approval_status").GetString());
        Assert.Equal(expectedReviewStatus, rowMetadata.GetProperty("api_review_status").GetString());
        Assert.Equal(expectedTrainReady.ToString(), rowMetadata.GetProperty("api_train_ready").GetString());
        Assert.Equal(bool.FalseString, rowMetadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, rowMetadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, rowMetadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, rowMetadata.GetProperty("raw_user_private_data_included").GetString());
        Assert.Equal(bool.FalseString, rowMetadata.GetProperty("provider_payload_included").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), metadata.GetProperty("delegates_to_domain_api").GetString());
        Assert.Equal(nameof(IDesignConversionLocalArtifactDb), metadata.GetProperty("backing_store").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("dpo_usefulness_response_refs_only").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_user_private_data_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("provider_payload_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static DesignConversionInternalEvaluateRequest CreateRequest(
        string htmlMarker = "PRIVATE_HTML_MARKER",
        string cssMarker = "PRIVATE_CSS_MARKER",
        string treeMarker = "PRIVATE_TREE_MARKER")
        => new(
            JobId: "pass128-job",
            RequestId: "pass128-request",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: $"<main data-private=\"{htmlMarker}\">Hello</main>",
            Css: $".{cssMarker}{{display:grid;}}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "tree-current",
                Format: "figma-json",
                Content: $"{{\"secret\":\"{treeMarker}\"}}",
                Sha256: "sha256:tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "tree-original",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\",\"version\":\"original\"}",
                Sha256: "sha256:tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots:
            [
                RenderArtifact("source-shot-desktop", "screenshot", 1440, 900),
                RenderArtifact("source-shot-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                RenderArtifact("design-render-desktop", "design-render", 1440, 900),
                RenderArtifact("design-render-mobile", "design-render", 390, 844)
            ],
            Budget: CreateBudget(),
            RenderResolutions:
            [
                new RenderQaResolution("desktop", 1440, 900, 1.0),
                new RenderQaResolution("mobile", 390, 844, 3.0)
            ],
            AiTeacherEvidenceRefs: ["teacher:none"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["fixture"] = "pass128-improvement-loop"
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "unit-test"
            });

    private static DesignConversionBudgetMetadata CreateBudget(
        int maxImprovementCandidates = 10,
        int maxRenderQaRuns = 2,
        decimal budgetUsd = 1.00m)
        => new(
            UserId: "user-redacted",
            Tier: "pro",
            MaxImprovementCandidates: maxImprovementCandidates,
            MaxRenderQaRuns: maxRenderQaRuns,
            BudgetUsd: budgetUsd,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.90"
            });

    private static DesignRenderArtifact RenderArtifact(string id, string kind, int width, int height)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: width,
            Height: height,
            Uri: $"artifact://pass128/{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static DesignConversionImprovementCandidate CreateCandidate(
        string candidateId,
        string? html = null,
        string? css = null)
        => new(
            CandidateId: candidateId,
            RequestId: $"{candidateId}-request",
            Html: html ?? "<main class=\"improved\">PRIVATE_HTML_MARKER_FIXED</main>",
            Css: css ?? ".improved{display:grid;gap:12px;}",
            ChangeReasons: ["scripted local unit-test candidate"],
            EvidenceRefs: [$"candidate:{candidateId}"],
            Metadata: new Dictionary<string, string>
            {
                ["local_strategy"] = "scripted"
            });

    private static DesignConversionImprovementCandidate CreateLoopCandidate(int attemptNumber)
    {
        var suffix = attemptNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return CreateCandidate(
            $"candidate-{attemptNumber.ToString("000", System.Globalization.CultureInfo.InvariantCulture)}",
            $"<main class=\"attempt-{suffix}\">PRIVATE_HTML_MARKER_STEP{suffix}</main>",
            $".attempt-{suffix}{{display:grid;gap:{suffix}px;}}");
    }

    private static string Sha256Ref(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }

    private static void AssertFinalEvaluationFromCandidate(
        DesignConversionImprovementLoopResult result,
        string expectedHtml,
        string expectedCss,
        double expectedScore,
        string? expectedEvidenceToken,
        string? rejectedEvidenceToken)
    {
        var final = result.FinalEvaluation!;
        Assert.Equal(expectedHtml, final.SubmittedRequest.Html);
        Assert.Equal(expectedCss, final.SubmittedRequest.Css);
        Assert.Contains(ExtractPrivateHtmlMarker(expectedHtml), final.ImprovedHtml, StringComparison.Ordinal);
        Assert.Contains(expectedCss, final.ImprovedCss, StringComparison.Ordinal);
        Assert.Equal(expectedScore, final.QaResults.Select(qa => qa.Verdict.Score).Min(), 4);

        if (!string.IsNullOrWhiteSpace(expectedEvidenceToken))
        {
            Assert.Contains(final.EvidenceRefs, reference =>
                reference.Contains(expectedEvidenceToken, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(rejectedEvidenceToken))
        {
            Assert.DoesNotContain(final.EvidenceRefs, reference =>
                reference.Contains(rejectedEvidenceToken, StringComparison.Ordinal));
            Assert.DoesNotContain(rejectedEvidenceToken, final.ImprovedHtml, StringComparison.Ordinal);
            Assert.DoesNotContain(rejectedEvidenceToken, final.ImprovedCss, StringComparison.Ordinal);
        }
    }

    private static string SerializePublicTelemetry(DesignConversionImprovementLoopResult result)
        => JsonSerializer.Serialize(new
        {
            result.PublicMetadata,
            result.EvidenceRefs,
            result.Warnings,
            Attempts = result.Attempts.Select(attempt => new
            {
                attempt.PublicMetadata,
                attempt.EvidenceRefs,
                attempt.RenderResultRefs,
                attempt.ComparisonArtifactRefs,
                attempt.VisualJudgeResultRefs,
                attempt.CandidateRef,
                attempt.JobRef,
                attempt.RequestRef,
                attempt.BestScore,
                attempt.RequiredScore,
                attempt.EstimatedCostUsd,
                attempt.BudgetUsd,
                attempt.StopReason,
                attempt.ShouldContinue
            })
        });

    private static string ExtractPrivateHtmlMarker(string html)
    {
        const string markerPrefix = "PRIVATE_HTML_MARKER";
        var start = html.IndexOf(markerPrefix, StringComparison.Ordinal);
        if (start < 0)
            return html;

        var end = start;
        while (end < html.Length && (char.IsLetterOrDigit(html[end]) || html[end] == '_'))
            end++;

        return html[start..end];
    }

    private static DesignConversionInternalEvaluationResult BuildScriptedEvaluation(
        DesignConversionInternalEvaluateRequest request,
        int attemptNumber,
        IReadOnlyList<double> scores,
        decimal costPerViewport,
        bool qaActualResolutionMissing = false)
    {
        var submitted = ToSubmittedRequest(request);
        var renderResults = new List<DesignConversionRenderResult>();
        var comparisonArtifacts = new List<DesignConversionQaComparisonArtifact>();
        var visualJudgeResults = new List<DesignConversionVisualJudgeResult>();
        var qaResults = new List<RenderQaResultRecord>();

        for (var index = 0; index < scores.Count; index++)
        {
            var resolution = request.RenderResolutions[index];
            var score = scores[index];
            var renderResultId = $"{request.JobId}:scripted-render:{index + 1}";
            var actual = new RenderQaImageRecord(
                ImageId: $"{renderResultId}:actual",
                Label: "actual-render",
                Resolution: resolution,
                Uri: $"artifact://n8-5/{renderResultId}.png",
                Sha256: $"sha256:{renderResultId}",
                Metadata: new Dictionary<string, string>());
            var expected = new RenderQaImageRecord(
                ImageId: $"{renderResultId}:expected",
                Label: "expected-design-render",
                Resolution: resolution,
                Uri: $"artifact://n8-5/{renderResultId}.expected.png",
                Sha256: $"sha256:{renderResultId}:expected",
                Metadata: new Dictionary<string, string>());
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JobId}:scripted-verdict:{index + 1}",
                Score: score,
                Defects: score >= 0.90
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.JobId}:scripted-defect:{index + 1}",
                            Category: "quality",
                            Severity: "minor",
                            Description: "Scripted N8-5 coordinator evaluation.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [expected.ImageId, actual.ImageId])
                    ],
                EvidenceRefs: [$"visual-judge:{request.JobId}:scripted:{index + 1}"],
                Passed: score >= 0.90);
            var renderResult = new DesignConversionRenderResult(
                RenderResultId: renderResultId,
                RenderRequestId: $"{request.JobId}:scripted-render-request:{index + 1}",
                Image: actual,
                ElapsedMs: 1,
                Warnings: []);
            var comparison = new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.JobId}:scripted-comparison:{index + 1}",
                ComparisonRequestId: $"{request.JobId}:scripted-comparison-request:{index + 1}",
                Expected: expected,
                Actual: actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://n8-5/{request.JobId}.diff.{index + 1}.png",
                DiffImageSha256: $"sha256:{request.JobId}:diff:{index + 1}",
                Metrics: new Dictionary<string, double>
                {
                    ["visual_distance"] = 1.0 - score
                },
                EvidenceRefs: [$"comparison:{request.JobId}:scripted:{index + 1}"]);
            var visualJudge = new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JobId}:scripted-judge:{index + 1}",
                JudgeRequestId: $"{request.JobId}:scripted-judge-request:{index + 1}",
                Status: "SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: "scripted-local-judge",
                    EndpointHost: "none",
                    ModelId: "scripted-local-v1",
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString,
                        ["external_ai_visual_judge_executed"] = bool.FalseString
                    }),
                EstimatedCostUsd: costPerViewport,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: verdict.EvidenceRefs,
                Metadata: new Dictionary<string, string>
                {
                    ["external_ai_visual_judge_executed"] = bool.FalseString
                });

            renderResults.Add(renderResult);
            comparisonArtifacts.Add(comparison);
            visualJudgeResults.Add(visualJudge);
            qaResults.Add(new RenderQaResultRecord(
                ResultId: $"{request.JobId}:scripted-qa:{index + 1}",
                RequestId: request.RequestId,
                Expected: expected,
                Actual: qaActualResolutionMissing ? actual with { Resolution = null! } : actual,
                Verdict: verdict,
                Metrics: new Dictionary<string, double>
                {
                    ["visual_distance"] = 1.0 - score
                },
                Warnings: []));
        }

        var boundaryTrace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            request.Budget,
            request.RenderResolutions,
            renderResults,
            visualJudgeResults,
            viewportFailures: null,
            improvementAttemptCount: attemptNumber);
        var criticOutput = new DesignConversionCriticUnitOutput(
            OutputId: $"scripted-critic:{request.JobId}",
            Verdict: qaResults.Select(qa => qa.Verdict).OrderBy(qa => qa.Score).First(),
            QaEvidenceRefs: qaResults.Select(qa => $"qa:{qa.ResultId}").ToArray(),
            AiTeacherEvidenceRefs: request.AiTeacherEvidenceRefs,
            DeterministicLogicClaimedAsLearning: false);
        var candidate = new DesignConversionImprovementCandidate(
            CandidateId: $"scripted-candidate:{request.RequestId}",
            RequestId: request.RequestId,
            Html: request.Html,
            Css: request.Css,
            ChangeReasons: ["scripted coordinator evaluation"],
            EvidenceRefs: [$"candidate:{request.RequestId}"],
            Metadata: new Dictionary<string, string>());
        var output = new DesignConversionImprovementOutput(
            OutputId: $"scripted-output:{request.JobId}",
            CandidateId: candidate.CandidateId,
            Html: request.Html,
            Css: request.Css,
            Verdict: criticOutput.Verdict,
            QaResults: qaResults,
            EvidenceRefs: candidate.EvidenceRefs,
            Accepted: criticOutput.Verdict.Passed);
        var improverOutput = new DesignConversionImproverUnitOutput(
            OutputId: output.OutputId,
            Candidate: candidate,
            Output: output,
            QaEvidenceRefs: criticOutput.QaEvidenceRefs,
            AiTeacherEvidenceRefs: request.AiTeacherEvidenceRefs,
            DeterministicLogicClaimedAsLearning: false);
        var runtimeFlow = new DesignConversionRuntimeFlowResult(
            Request: submitted,
            CriticOutput: criticOutput,
            ImproverOutput: improverOutput,
            Candidate: candidate,
            Output: output,
            UnitExecutionOrder: ["scripted-critic", "scripted-improver"],
            EvidenceRefs: [$"runtime-flow:{request.JobId}"],
            DeterministicLogicClaimedAsLearning: false);
        var evidenceRefs = renderResults
            .Select(render => $"render-result:{render.RenderResultId}")
            .Concat(comparisonArtifacts.Select(comparison => $"comparison-artifact:{comparison.ArtifactId}"))
            .Concat(visualJudgeResults.Select(judge => $"visual-judge-result:{judge.JudgeResultId}"))
            .Concat(visualJudgeResults.SelectMany(judge => judge.EvidenceRefs))
            .Concat(qaResults.Select(qa => $"render-qa-result:{qa.ResultId}"))
            .Concat(boundaryTrace.EvidenceRefs)
            .Concat(runtimeFlow.EvidenceRefs)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new DesignConversionInternalEvaluationResult(
            JobId: request.JobId,
            SubmittedRequest: submitted,
            RenderResults: renderResults,
            ComparisonArtifacts: comparisonArtifacts,
            VisualJudgeResults: visualJudgeResults,
            QaResults: qaResults,
            RuntimeFlowResult: runtimeFlow,
            Verdict: output.Verdict,
            ImprovedHtml: request.Html,
            ImprovedCss: request.Css,
            EvidenceRefs: evidenceRefs,
            Warnings: boundaryTrace.Warnings,
            ScaffoldMarkers: new Dictionary<string, string>(),
            DeterministicLogicClaimedAsLearning: false,
            VisualQaBoundaryTrace: boundaryTrace);
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

    private sealed class FailingAfterCountEvaluator(
        IDesignConversionThinClientJobEvaluator inner,
        int failOnCall) : IDesignConversionThinClientJobEvaluator
    {
        private int _callCount;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            _callCount++;
            if (_callCount == failOnCall)
            {
                return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                    "Scripted evaluation failure after incumbent.",
                    "SCRIPTED_EVALUATION_FAILED"));
            }

            return inner.EvaluateAsync(request, cancellationToken);
        }
    }

    private sealed class RecordingScriptedLoopEvaluator(Func<int, double> scoreForAttempt)
        : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var attemptNumber = Requests.Count;
            var score = scoreForAttempt(attemptNumber);
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(BuildScriptedEvaluation(
                request,
                attemptNumber,
                [score, score],
                costPerViewport: 0m)));
        }
    }

    private sealed class ScriptedCoverageStopEvaluator(
        double initialScore,
        double incompleteCandidateScore,
        decimal incompleteCandidateCostUsd) : IDesignConversionThinClientJobEvaluator
    {
        private int _callCount;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            _callCount++;
            var evaluation = _callCount == 1
                ? BuildScriptedEvaluation(
                    request,
                    attemptNumber: 1,
                    scores: [initialScore, initialScore],
                    costPerViewport: 0m)
                : BuildScriptedEvaluation(
                    request,
                    attemptNumber: _callCount,
                    scores: [incompleteCandidateScore],
                    costPerViewport: incompleteCandidateCostUsd);
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(evaluation));
        }
    }

    private sealed class MetadataOnlyCoverageRecoveryEvaluator : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var attemptNumber = Requests.Count;
            var scores = attemptNumber switch
            {
                1 => new[] { 0.70 },
                2 => new[] { 0.70, 0.70 },
                _ => new[] { 0.94, 0.94 }
            };

            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(BuildScriptedEvaluation(
                request,
                attemptNumber,
                scores,
                costPerViewport: 0m,
                qaActualResolutionMissing: true)));
        }
    }

    private sealed class BlockingEvaluator(bool throwOnCancellation) : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (OperationCanceledException) when (!throwOnCancellation)
            {
                return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                    "Blocking evaluator observed cancellation.",
                    "CANCELLED");
            }

            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "Blocking evaluator completed without cancellation.",
                "BLOCKING_EVALUATOR_COMPLETED");
        }
    }

    private sealed class RecordingRenderProvider : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass128/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 1,
                Warnings: [])));
        }
    }

    private sealed class RecordingComparisonProvider(double score) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var passed = score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:verdict",
                Score: score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.ComparisonRequestId}:defect",
                            Category: "spacing",
                            Severity: "minor",
                            Description: "Scripted comparison provider for local coordinator tests.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [request.Expected.ImageId, request.Actual.ImageId])
                    ],
                EvidenceRefs: [$"comparison:{request.ComparisonRequestId}"],
                Passed: passed);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://pass128/{request.ComparisonRequestId}.diff.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double>
                {
                    ["visual_distance"] = 1.0 - score
                },
                EvidenceRefs: [$"comparison-artifact:{request.ComparisonRequestId}"])));
        }
    }

    private sealed class ScriptedVisualJudge(Func<DesignConversionVisualJudgeRequest, JudgeScript> resolve)
        : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var script = resolve(request);
            var passed = !script.Blocked && script.Score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:scripted-verdict",
                Score: script.Score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.JudgeRequestId}:scripted-defect",
                            Category: script.Blocked ? "provider" : "quality",
                            Severity: script.Blocked ? "blocker" : "minor",
                            Description: "Scripted visual judge result for local coordinator tests.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
                    ],
                EvidenceRefs: [$"visual-judge:{request.JudgeRequestId}"],
                Passed: passed);
            var status = script.Blocked
                ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                : "SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED";

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:scripted-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: status,
                IsBlocked: script.Blocked,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: script.Blocked ? "none" : "scripted-local-judge",
                    EndpointHost: "none",
                    ModelId: script.Blocked ? "none" : "scripted-local-v1",
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = (!script.Blocked).ToString(),
                        ["external_ai_visual_judge_executed"] = bool.FalseString
                    }),
                EstimatedCostUsd: script.Cost,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: verdict.EvidenceRefs,
                Metadata: new Dictionary<string, string>
                {
                    ["external_ai_visual_judge_executed"] = bool.FalseString
                })));
        }
    }

    private sealed record JudgeScript(double Score, decimal? Cost, bool Blocked)
    {
        public static JudgeScript FromScore(double score, decimal? cost)
            => new(score, cost, Blocked: false);

        public static JudgeScript FromBlockedProvider()
            => new(0.0, 0m, Blocked: true);
    }

    private sealed class BlockingCandidateStrategy(bool throwOnCancellation)
        : IDesignConversionImprovementCandidateStrategy
    {
        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (OperationCanceledException) when (!throwOnCancellation)
            {
                return OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "Blocking candidate strategy observed cancellation.",
                    "CANCELLED");
            }

            return OperationResult<DesignConversionImprovementCandidate>.Fail(
                "Blocking candidate strategy completed without cancellation.",
                "BLOCKING_CANDIDATE_COMPLETED");
        }
    }

    private sealed class ScriptedCandidateStrategy(
        params OperationResult<DesignConversionImprovementCandidate>[] results)
        : IDesignConversionImprovementCandidateStrategy
    {
        private readonly Queue<OperationResult<DesignConversionImprovementCandidate>> _results = new(results);

        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (_results.Count == 0)
            {
                return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "No scripted candidate result was configured.",
                    "SCRIPTED_CANDIDATE_MISSING"));
            }

            return Task.FromResult(_results.Dequeue());
        }
    }
}
