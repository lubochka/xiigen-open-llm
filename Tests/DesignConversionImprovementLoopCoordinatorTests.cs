// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionImprovementLoopCoordinatorTests
{
    [Fact]
    public async Task RunAsync_NoArtifactDbHandsOffCoordinatorSelectedNearestLowerMeasuredEvaluation()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var evaluator = new RecordingScriptedLoopEvaluator(attemptNumber => attemptNumber switch
        {
            1 => 0.70,
            2 => 0.82,
            _ => 0.94
        });
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(2)),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(3)));
        var request = CreateRequest() with
        {
            JobId = "pass128-handoff-root",
            RequestId = "pass128-handoff-request"
        };
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            strategy,
            handoff);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-handoff-loop",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var chosen = result.Value!.FinalEvaluation!;
        Assert.Equal("pass128-handoff-root:loop-attempt:3", chosen.JobId);
        Assert.Equal(1, handoff.PendingCount);

        var resolved = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(chosen.JobId));

        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var reference = Assert.IsType<DesignConversionRuntimeRejectedEvaluationReference>(resolved.Value);
        Assert.True(reference.TryGetTrustedHydratedEvaluation(out var rejected));
        Assert.NotNull(rejected);
        Assert.Equal("pass128-handoff-root:loop-attempt:2", rejected!.JobId);
        Assert.NotEqual(chosen.JobId, rejected.JobId);
        Assert.Equal(
            "scripted-candidate:candidate-002-request",
            rejected.RuntimeFlowResult.Output.CandidateId);
        Assert.True(
            rejected.VisualQaBoundaryTrace.VisualDecision.QualityScore <
            chosen.VisualQaBoundaryTrace.VisualDecision.QualityScore);
        Assert.Equal(0, handoff.PendingCount);

        var consumedAgain = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(chosen.JobId));
        Assert.True(consumedAgain.IsSuccess, consumedAgain.ErrorMessage);
        Assert.Null(consumedAgain.Value);
    }

    [Fact]
    public async Task RunAsync_FirstEvaluationFailurePersistsOneHistoryOnlyRecordWithoutTrainingAuthority()
    {
        using var testRoot = TestResultsDirectoryScope.Create(
            "il",
            $"w082-first-failure-{Guid.NewGuid():N}");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            testRoot.DirectoryPath);
        var evaluator = new FailingAfterCountEvaluator(
            new RecordingScriptedLoopEvaluator(_ => 0.7),
            failOnCall: 1);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(),
            artifactDb,
            () => new DateTimeOffset(2026, 8, 15, 12, 0, 0, TimeSpan.Zero));
        var initialRequest = CreateRequest(
            "PRIVATE_W082_FIRST_FAILURE_HTML",
            "PRIVATE_W082_FIRST_FAILURE_CSS",
            "PRIVATE_W082_FIRST_FAILURE_TREE") with
        {
            JobId = "w082-first-failure-job",
            RequestId = "w082-first-failure-request"
        };

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "w082-first-failure-loop",
                InitialEvaluationRequest: initialRequest,
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.EvaluationFailed,
            result.Value!.StopReason);
        Assert.Null(result.Value.FinalEvaluation);
        var failedAttempt = Assert.Single(result.Value.Attempts);
        Assert.Equal(1, failedAttempt.AttemptNumber);
        Assert.Equal(
            DesignConversionLocalArtifactDbRecord
                .ImprovementLoopAttemptHistoryRecordedStatus,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .ImprovementLoopHistoryStatusMetadataKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .ImprovementLoopHistoryOnlyMetadataKey]);
        Assert.False(result.Value.PublicMetadata.ContainsKey(
            DesignConversionImprovementLoopCoordinator
                .LocalTrainingStoreDpoSourceMarker));

        var records = await new JsonlDesignConversionLocalArtifactDb(
                testRoot.DirectoryPath)
            .ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal(
            bool.TrueString,
            stored.Metadata[
                DesignConversionImprovementLoopCoordinator
                    .ImprovementLoopHistoryOnlyMetadataKey]);
        Assert.Equal(bool.FalseString, stored.Metadata["training_grade"]);
        Assert.Equal(bool.FalseString, stored.Metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
        Assert.Equal(
            DesignConversionDpoLeakageGuard.HashRef(
                "dcm-loop",
                "w082-first-failure-loop"),
            stored.Metadata[DesignConversionLocalArtifactDbRecord
                .ImprovementLoopRefMetadataKey]);
        Assert.DoesNotContain(
            stored.Metadata.Keys,
            key => key.Contains(
                "corpus_admission_authority",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            stored.Metadata.Keys,
            key => key.Contains("g3_acceptance", StringComparison.Ordinal));
        Assert.DoesNotContain(
            stored.EvidenceRefs,
            reference => reference.Contains(
                "corpus_admission_authority",
                StringComparison.Ordinal) ||
                reference.Contains("g3_acceptance", StringComparison.Ordinal));
        Assert.True(stored.TryGetImprovementLoopAttemptHistory(out var history));
        Assert.NotNull(history);
        Assert.Equal(1, history!.AttemptCount);
        Assert.Equal(
            stored.Metadata[DesignConversionLocalArtifactDbRecord
                .ImprovementLoopRefMetadataKey],
            history.LoopRef);
        Assert.Single(history.Attempts);

        var rawAuthority = await File.ReadAllTextAsync(
            Path.Combine(testRoot.DirectoryPath, "conversion-results.jsonl"));
        Assert.DoesNotContain(
            "PRIVATE_W082_FIRST_FAILURE_HTML",
            rawAuthority,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "PRIVATE_W082_FIRST_FAILURE_CSS",
            rawAuthority,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "PRIVATE_W082_FIRST_FAILURE_TREE",
            rawAuthority,
            StringComparison.Ordinal);

        var approved = await artifactDb.QueryApprovedTrainingCandidatesAsync();
        Assert.True(approved.IsSuccess, approved.ErrorMessage);
        Assert.Empty(approved.Value!);
    }

    [Fact]
    public async Task RunAsync_ChangedHistoryReadbackFailsBeforeDurabilityMarkersOrTrainingAuthority()
    {
        using var testRoot = TestResultsDirectoryScope.Create(
            "il",
            $"w082-changed-history-readback-{Guid.NewGuid():N}");
        var innerDb = new JsonlDesignConversionLocalArtifactDb(
            testRoot.DirectoryPath);
        var artifactDb = new FaultingLocalArtifactDb(
            innerDb,
            failAppend: false,
            failList: false,
            alterAttemptHistoryOnList: true);
        var evaluator = new FailingAfterCountEvaluator(
            new RecordingScriptedLoopEvaluator(_ => 0.7),
            failOnCall: 1);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(),
            artifactDb,
            () => new DateTimeOffset(2026, 8, 15, 12, 1, 0, TimeSpan.Zero));

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "w082-changed-history-readback-loop",
                InitialEvaluationRequest: CreateRequest() with
                {
                    JobId = "w082-changed-history-readback-job",
                    RequestId = "w082-changed-history-readback-request"
                },
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .LocalArtifactDbReadbackMismatchErrorCode,
            result.ErrorCode);
        var stored = await innerDb.ListRecordsAsync();
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
        var historyOnly = Assert.Single(stored.Value!);
        Assert.Empty(historyOnly.DpoCandidates);
        Assert.Equal(bool.FalseString, historyOnly.Metadata["training_grade"]);
        Assert.Equal(
            bool.FalseString,
            historyOnly.Metadata["approved_for_training"]);
    }

    [Fact]
    public async Task RunAsync_NoArtifactDbWithoutEligiblePairLeavesNoRejectedEvaluationHandoff()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.95),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, cost: 0.01m)),
            new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("unused"))),
            rejectedEvaluationHandoff: handoff);
        var request = CreateRequest() with
        {
            JobId = "pass128-no-handoff-root",
            RequestId = "pass128-no-handoff-request"
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass128-no-handoff-loop",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(
            result.IsSuccess,
            $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(0, handoff.PendingCount);
        var resolved = await handoff.ResolveMeasuredRejectedEvaluationAsync(
            new DesignConversionRuntimeRejectedEvaluationLookup(request.JobId));
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        Assert.Null(resolved.Value);
    }

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

        Assert.True(
            result.IsSuccess,
            $"{result.ErrorCode}: {result.ErrorMessage}");
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
    public async Task RunAsync_MaxTeacherCallsZeroDoesNotBlockDeterministicCandidateStrategy()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0.01m));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate("candidate-should-not-run")));
        var coordinator = CreateCoordinator(renderProvider, comparisonProvider, visualJudge, strategy);
        var request = CreateRequest() with
        {
            Budget = CreateBudget(maxTeacherCalls: 0)
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "max-teacher-calls-zero-loop",
            InitialEvaluationRequest: request,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(
            result.IsSuccess,
            $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.False(result.Value.QualityReached);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Single(strategy.Requests);
        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey]);
        Assert.Equal("0", result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey]);
        Assert.Equal(
            "contract_transport_with_dpo_attestation_teacher_calls_request_scoped_cap_v2",
            result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey]);
        Assert.Equal("0", result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey]);
        Assert.Contains("improvement_loop:max_teacher_calls_reached", result.Value.EvidenceRefs);
        Assert.DoesNotContain(result.Value.EvidenceRefs, reference =>
            reference.Contains("max_teacher_call_observed=1", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RunAsync_MaxTeacherCallsOneDoesNotConsumeDeterministicStrategyCalls()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(0.70);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.76, cost: 0.01m)
                : JudgeScript.FromScore(0.70, cost: 0.01m));
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-allowed-once",
                "<main class=\"teacher-one\">PRIVATE_HTML_MARKER_ONE</main>",
                ".teacher-one{display:grid;gap:10px;}")),
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateCandidate(
                "candidate-must-not-run",
                "<main class=\"teacher-two\">PRIVATE_HTML_MARKER_TWO</main>",
                ".teacher-two{display:grid;gap:20px;}")));
        var coordinator = CreateCoordinator(renderProvider, comparisonProvider, visualJudge, strategy);
        var request = CreateRequest() with
        {
            Budget = CreateBudget(maxTeacherCalls: 1)
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "max-teacher-calls-one-loop",
            InitialEvaluationRequest: request,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.MaxAttemptsReached, result.Value!.StopReason);
        Assert.False(result.Value.Completed);
        Assert.False(result.Value.QualityReached);
        Assert.Equal(2, result.Value.AttemptCount);
        var candidateRequest = Assert.Single(strategy.Requests);
        Assert.Equal(1, candidateRequest.CompletedAttemptNumber);
        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Equal("<main class=\"teacher-one\">PRIVATE_HTML_MARKER_ONE</main>", result.Value.FinalEvaluation!.SubmittedRequest.Html);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey]);
        Assert.Equal("1", result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey]);
        Assert.Equal("0", result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey]);
        Assert.DoesNotContain(result.Value.EvidenceRefs, reference =>
            reference.Contains("max_teacher_call_observed=1", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Value.EvidenceRefs, reference =>
            reference.Contains("max_teacher_call_observed=2", StringComparison.Ordinal));
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
        Assert.Equal(0.75, DesignConversionVisualQaBoundaryPolicy.ProvisionalFreeTierQualityThreshold);
        Assert.Equal("free", attempt.PublicMetadata["tier"]);
        Assert.Equal("0.75", attempt.PublicMetadata["required_score"]);
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
        Assert.Equal("0.75", result.Value.PublicMetadata["final_required_score"]);
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
        const string quarantineReasonPrefix = "runtime_created_dpo_candidate_quarantine_reason=";
        const string generatorQuarantineReason =
            quarantineReasonPrefix + "server_owned_route_admission_not_bound";
        const string fallbackQuarantineReason =
            quarantineReasonPrefix + "runtime_rejected_candidate_independent_evaluation_required";
        const string originalHtmlMarker = "PRIVATE_HTML_MARKER_ORIGINAL";
        const string originalCssMarker = "PRIVATE_CSS_MARKER_ORIGINAL";
        const string originalTreeMarker = "PRIVATE_TREE_MARKER_ORIGINAL";
        const string firstImprovedHtml = "<main class=\"improved-one\">PRIVATE_HTML_MARKER_STEP1</main>";
        const string firstImprovedCss = ".improved-one{display:grid;gap:8px;}/* PRIVATE_CSS_MARKER_STEP1 */";
        const string secondImprovedHtml = "<main class=\"improved-two\">PRIVATE_HTML_MARKER_STEP2</main>";
        const string secondImprovedCss = ".improved-two{display:grid;gap:16px;}/* PRIVATE_CSS_MARKER_STEP2 */";
        using var testRoot = TestResultsDirectoryScope.Create(
            "il",
            $"p154-{Guid.NewGuid():N}");
        var root = testRoot.DirectoryPath;
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        using var teacherAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true);
        var evaluator = new RecordingScriptedLoopEvaluator(attemptNumber =>
        {
            return attemptNumber switch
            {
                1 => 0.70,
                2 => 0.82,
                _ => 0.94
            };
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
        var postEvaluationStateStore =
            new JsonDesignConversionPostEvaluationStateStore(
                artifactDb.PostEvaluationStateRootDirectory);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            strategy,
            artifactDb,
            () => new DateTimeOffset(2026, 7, 3, 12, 0, 0, TimeSpan.Zero),
            postEvaluationStateStore: postEvaluationStateStore);
        var originalRequest = CreateRequest(originalHtmlMarker, originalCssMarker, originalTreeMarker) with
        {
            Provenance = new Dictionary<string, string>
            {
                ["source"] = "unit-test",
                ["scripted_defect_category"] = DesignConversionNegativeReasonTaxonomy.Spacing
            }
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "pass154-loop-to-training-store",
            InitialEvaluationRequest: originalRequest,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(
            result.IsSuccess,
            $"{result.ErrorCode ?? "<null>"}: {result.ErrorMessage}");
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.Equal(3, result.Value.AttemptCount);
        var evaluatedRequests = evaluator.Requests.ToArray();
        Assert.Equal(3, evaluatedRequests.Length);
        var teacherAuthoredEvaluatedRequests = evaluatedRequests
            .Skip(1)
            .Where(request => request.RequestId.StartsWith(
                "teacher-authored-request:",
                StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(2, teacherAuthoredEvaluatedRequests.Length);
        var finalEvaluatedRequest = evaluatedRequests[^1];
        Assert.Equal(finalEvaluatedRequest.RequestId, result.Value.FinalEvaluation!.SubmittedRequest.RequestId);
        Assert.Equal(finalEvaluatedRequest.Html, result.Value.FinalEvaluation.SubmittedRequest.Html);
        Assert.Equal(finalEvaluatedRequest.Css, result.Value.FinalEvaluation.SubmittedRequest.Css);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);
        Assert.Equal(
            DesignConversionLocalArtifactDbRecord
                .ImprovementLoopAttemptHistoryRecordedStatus,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .ImprovementLoopHistoryStatusMetadataKey]);
        Assert.Equal(
            "3",
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .ImprovementLoopHistoryAttemptCountMetadataKey]);
        Assert.StartsWith(
            "sha256:",
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .ImprovementLoopHistorySha256MetadataKey],
            StringComparison.Ordinal);

        var reloaded = new JsonlDesignConversionLocalArtifactDb(root);
        var records = await reloaded.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.Equal(finalEvaluatedRequest.JobId, stored.JobId);
        Assert.Equal(finalEvaluatedRequest.RequestId, stored.RequestId);
        Assert.Equal(DesignConversionSourceKind.Figma.ToString(), stored.SourceKind);
        Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
        Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);
        Assert.Equal(bool.TrueString, stored.Metadata["final_candidate_from_loop"]);
        Assert.Equal("3", stored.Metadata["attempt_count"]);
        Assert.True(stored.TryGetImprovementLoopAttemptHistory(out var durableHistory));
        Assert.NotNull(durableHistory);
        Assert.Equal(3, durableHistory!.AttemptCount);
        var expectedLoopRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-loop",
            "pass154-loop-to-training-store");
        Assert.Equal(
            expectedLoopRef,
            stored.Metadata[DesignConversionLocalArtifactDbRecord
                .ImprovementLoopRefMetadataKey]);
        Assert.Equal(expectedLoopRef, durableHistory.LoopRef);
        Assert.NotEmpty(durableHistory.RedactionCodes);
        Assert.Equal(
            durableHistory.RedactionCodes.Order(StringComparer.Ordinal),
            durableHistory.RedactionCodes);
        Assert.Equal(
            durableHistory.RedactionCodes.Count,
            durableHistory.RedactionCodes.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .ImprovementLoopHistorySha256MetadataKey],
            durableHistory.HistorySha256);
        Assert.Equal([1, 2, 3], durableHistory.Attempts.Select(attempt => attempt.AttemptNumber));
        Assert.Equal(
            result.Value.Attempts.Select(attempt => attempt.CandidateRef),
            durableHistory.Attempts.Select(attempt => attempt.CandidateRef));
        Assert.Equal(
            result.Value.Attempts.Select(attempt => attempt.StopReason),
            durableHistory.Attempts.Select(attempt => attempt.StopReason));
        Assert.Equal(
            result.Value.Attempts.Select(attempt => attempt.VisualCostMeasurementComplete),
            durableHistory.Attempts.Select(attempt => attempt.VisualCostMeasurementComplete));
        Assert.Equal(
            result.Value.Attempts.Select(attempt => attempt.VisualCostMissingMeasurementCount),
            durableHistory.Attempts.Select(attempt => attempt.VisualCostMissingMeasurementCount));
        Assert.Equal(
            result.Value.Attempts.Select(attempt => attempt.VisualCostSource),
            durableHistory.Attempts.Select(attempt => attempt.VisualCostSource));
        var durableHistoryJson = stored.Metadata[
            DesignConversionLocalArtifactDbRecord
                .ImprovementLoopAttemptHistoryMetadataKey];
        using (var durableHistoryDocument = JsonDocument.Parse(durableHistoryJson))
        {
            var rootProperties = durableHistoryDocument.RootElement
                .EnumerateObject()
                .Select(property => property.Name)
                .ToArray();
            Assert.Equal(
                [
                    "schema", "version", "loopRef", "attemptCount",
                    "redactionCodes", "historySha256", "attempts"
                ],
                rootProperties);
            var firstAttemptProperties = durableHistoryDocument.RootElement
                .GetProperty("attempts")[0]
                .EnumerateObject()
                .Select(property => property.Name)
                .ToArray();
            Assert.Equal(
                [
                    "attemptNumber", "candidateRef", "jobRef", "requestRef",
                    "stopReason", "shouldContinue", "bestScore", "requiredScore",
                    "estimatedCostUsd", "budgetUsd", "policyAttemptCount",
                    "maxImprovementCandidates", "renderResultRefs",
                    "comparisonArtifactRefs", "visualJudgeResultRefs", "evidenceRefs",
                    "publicMetadata", "visualCostMeasurementComplete",
                    "visualCostMissingMeasurementCount", "visualCostSource"
                ],
                firstAttemptProperties);
        }
        Assert.False(stored.Metadata.ContainsKey(
            JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey));
        Assert.Equal(
            bool.FalseString,
            stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal("0", stored.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal(
            bool.FalseString,
            stored.Metadata["training_candidate_only"]);
        Assert.Equal(bool.FalseString, stored.Metadata["approved_for_training"]);
        Assert.Equal(bool.FalseString, stored.Metadata["production_dpo"]);
        Assert.Contains(
            "independent_preference_not_proven",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            generatorQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            fallbackQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            stored.Metadata["runtime_created_dpo_candidate_warnings"]
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(warning => warning.StartsWith(
                    quarantineReasonPrefix,
                    StringComparison.Ordinal)));
        Assert.DoesNotContain(fallbackQuarantineReason, result.Value.Warnings);
        Assert.Equal(
            1,
            result.Value.Warnings.Count(warning => warning.StartsWith(
                quarantineReasonPrefix,
                StringComparison.Ordinal)));
        Assert.Contains(generatorQuarantineReason, result.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_independently_evaluated=false",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            "high_quality_negative_pair_not_proven",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "runtime_rejected_independently_evaluated=true",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "high_quality_negative_pair_proven",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);

        var approved = await reloaded.QueryApprovedTrainingCandidatesAsync();
        Assert.True(approved.IsSuccess, approved.ErrorMessage);
        Assert.Empty(approved.Value!);
        var usefulness = await new DesignConversionDpoUsefulnessApi(reloaded)
            .QueryAsync(new DesignConversionDpoUsefulnessApiRequest(
                IncludeBlocked: true));
        Assert.True(usefulness.IsSuccess, usefulness.ErrorMessage);
        Assert.Equal(0, usefulness.Value!.RowCount);
        Assert.Equal(0, usefulness.Value.TrainReadyCount);
        var handoff = await new DesignConversionDpoUsefulnessApi(reloaded)
            .ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId),
                DateTimeOffset.UnixEpoch.AddMinutes(155));
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(0, handoff.Value!.ApprovedCandidateCount);
        Assert.Empty(handoff.Value.Rows);

        var persistedJsonl = await File.ReadAllTextAsync(
            Path.Combine(root, "conversion-results.jsonl"));
        var rawPrivateFragments = evaluatedRequests
            .SelectMany(evaluatedRequest =>
                new[] { evaluatedRequest.Html, evaluatedRequest.Css })
            .Concat(
            [
                originalHtmlMarker,
                originalCssMarker,
                originalTreeMarker,
                firstImprovedHtml,
                firstImprovedCss,
                secondImprovedHtml,
                secondImprovedCss
            ])
            .Where(fragment => !string.IsNullOrWhiteSpace(fragment))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        foreach (var rawPrivateFragment in rawPrivateFragments)
        {
            Assert.DoesNotContain(
                rawPrivateFragment,
                persistedJsonl,
                StringComparison.Ordinal);
        }
        Assert.DoesNotContain(
            "C:/Users/lubao",
            persistedJsonl,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "data:image",
            persistedJsonl,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "candidate_html",
            persistedJsonl,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "candidate_css",
            persistedJsonl,
            StringComparison.OrdinalIgnoreCase);

    }

    [Fact]
    public async Task RunAsync_PostEvaluationStatePrepareFailureDoesNotAppendTrainingArtifact()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "loop-post-evaluation-state-failure");
        var artifactDb =
            new JsonlDesignConversionLocalArtifactDb(root);
        var failingStateStore =
            new FailingPostEvaluationStateStore();
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate(
                    "candidate-002",
                    "<main class=\"improved-intermediate\">intermediate</main>",
                    ".improved-intermediate{display:grid;}")),
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate(
                    "candidate-003",
                    "<main class=\"improved-final\">final</main>",
                    ".improved-final{display:grid;}")));
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            strategy,
            artifactDb,
            () => new DateTimeOffset(
                2026,
                7,
                3,
                13,
                0,
                0,
                TimeSpan.Zero),
            postEvaluationStateStore: failingStateStore);

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "post-evaluation-state-prepare-failure",
                InitialEvaluationRequest: CreateRequest() with
                {
                    Provenance = new Dictionary<string, string>
                    {
                        ["source"] = "unit-test",
                        ["scripted_defect_category"] =
                            DesignConversionNegativeReasonTaxonomy.Spacing
                    }
                },
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));

        AssertDpoFaultFixtureAttemptProgression(
            evaluator,
            expectInitialSyntheticNegative: false,
            expectTeacherAuthoredCandidates: true);
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .PostEvaluationStateWriteFailedErrorCode,
            result.ErrorCode);
        Assert.Equal(
            "Improvement-loop persistence did not complete durably.",
            result.ErrorMessage);
        Assert.Equal(1, failingStateStore.WriteCalls);
        Assert.Null(result.Value);
        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
    }

    [Theory]
    [InlineData(
        "store-unavailable",
        DesignConversionImprovementLoopCoordinator
            .PostEvaluationStateStoreUnavailableErrorCode)]
    [InlineData(
        "append",
        DesignConversionImprovementLoopCoordinator
            .LocalArtifactDbAppendFailedErrorCode)]
    [InlineData(
        "readback",
        DesignConversionImprovementLoopCoordinator
            .LocalArtifactDbReadbackFailedErrorCode)]
    [InlineData(
        "commit",
        DesignConversionImprovementLoopCoordinator
            .PostEvaluationStateCommitFailedErrorCode)]
    [InlineData(
        "quarantine",
        DesignConversionImprovementLoopCoordinator
            .PostEvaluationStateQuarantineFailedErrorCode)]
    public async Task RunAsync_PostEvaluationPersistenceFailuresAreOuterFailures(
        string failureStage,
        string expectedErrorCode)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "loop-post-evaluation-" + failureStage);
        var innerDb = new JsonlDesignConversionLocalArtifactDb(root);
        var artifactDb = new FaultingLocalArtifactDb(
            innerDb,
            failAppend: failureStage is "append" or "quarantine",
            failList: failureStage == "readback");
        var stateStore = failureStage == "store-unavailable"
            ? null
            : new ScriptedPostEvaluationStateStore(
                failCommit: failureStage == "commit",
                failQuarantine: failureStage == "quarantine");
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "candidate-002",
                        "<main class=\"improved-intermediate\">intermediate</main>",
                        ".improved-intermediate{display:grid;}")),
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "candidate-003",
                        "<main class=\"improved-final\">final</main>",
                        ".improved-final{display:grid;}"))),
            artifactDb,
            () => new DateTimeOffset(
                2026,
                7,
                3,
                13,
                30,
                0,
                TimeSpan.Zero),
            postEvaluationStateStore: stateStore);

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "post-evaluation-" + failureStage,
                InitialEvaluationRequest: CreateRequest() with
                {
                    Provenance = new Dictionary<string, string>
                    {
                        ["source"] = "unit-test",
                        ["scripted_defect_category"] =
                            DesignConversionNegativeReasonTaxonomy.Spacing
                    }
                },
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));

        AssertDpoFaultFixtureAttemptProgression(
            evaluator,
            expectInitialSyntheticNegative: false,
            expectTeacherAuthoredCandidates: true);
        if (failureStage == "store-unavailable")
            Assert.Empty(artifactDb.AttemptedRecords);
        else if (failureStage is "append" or "quarantine")
            AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidateBeforePersistence(
                Assert.Single(artifactDb.AttemptedRecords),
                evaluator);
        else
        {
            var persistedRecords = await innerDb.ListRecordsAsync();
            Assert.True(persistedRecords.IsSuccess, persistedRecords.ErrorMessage);
            AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
                Assert.Single(persistedRecords.Value!),
                evaluator,
                root);
        }
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(
            "Improvement-loop persistence did not complete durably.",
            result.ErrorMessage);
        if (failureStage == "quarantine")
            Assert.True(stateStore!.QuarantineWriteCalls > 0);
    }

    [Fact]
    public async Task RunAsync_PostPublicationCommitFailureLeavesRestartedCentralEligibilityDenied()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "loop-post-publication-commit-failure");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var stateRoot = Path.Combine(
            root,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        var stateStore =
            new PostPublicationCommitFailureStateStore(stateRoot);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "candidate-002",
                        "<main class=\"improved-intermediate\">intermediate</main>",
                        ".improved-intermediate{display:grid;}")),
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "candidate-003",
                        "<main class=\"improved-final\">final</main>",
                        ".improved-final{display:grid;}"))),
            artifactDb,
            () => new DateTimeOffset(
                2026,
                7,
                30,
                13,
                30,
                0,
                TimeSpan.Zero),
            postEvaluationStateStore: stateStore);

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "post-publication-commit-failure",
                InitialEvaluationRequest: CreateRequest() with
                {
                    Provenance = new Dictionary<string, string>
                    {
                        ["source"] = "unit-test",
                        ["scripted_defect_category"] =
                            DesignConversionNegativeReasonTaxonomy.Spacing
                    }
                },
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));
        AssertDpoFaultFixtureAttemptProgression(
            evaluator,
            expectInitialSyntheticNegative: false,
            expectTeacherAuthoredCandidates: true);
        var durableRecords = await artifactDb.ListRecordsAsync();
        Assert.True(durableRecords.IsSuccess, durableRecords.ErrorMessage);
        var actualArtifact = Assert.Single(durableRecords.Value!);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            actualArtifact,
            evaluator,
            root);
        var restartedStore =
            new JsonDesignConversionPostEvaluationStateStore(stateRoot);
        var rawStates = await restartedStore.ListAsync();
        var rawState = Assert.Single(rawStates.Value!);
        var eligibility =
            await new DesignConversionPostEvaluationTrainingEligibilityPolicy(
                    restartedStore)
                .EvaluateManyAsync([actualArtifact]);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .PostEvaluationStateCommitFailedErrorCode,
            result.ErrorCode);
        Assert.Equal(1, stateStore.InjectedFailures);
        Assert.True(rawStates.IsSuccess, rawStates.ErrorMessage);
        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .PersistenceCommitted,
            rawState.PersistenceStatus);
        Assert.Equal(actualArtifact.RecordId, rawState.ArtifactRecordId);
        Assert.True(rawState.TrainingEligibilityDenied);
        Assert.False(Assert.Single(eligibility).IsEligible);
    }

    [Fact]
    public async Task RunAsync_DenialCreationFailureReturnsOuterFailAndLeavesStateIneligible()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "loop-denial-create-failure");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var stateStore = new ScriptedPostEvaluationStateStore(
            failCommit: false,
            failQuarantine: false,
            failDeny: true);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "candidate-002",
                        "<main class=\"improved-intermediate\">intermediate</main>",
                        ".improved-intermediate{display:grid;}")),
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "candidate-003",
                        "<main class=\"improved-final\">final</main>",
                        ".improved-final{display:grid;}"))),
            artifactDb,
            postEvaluationStateStore: stateStore);

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "denial-create-failure",
                InitialEvaluationRequest: CreateRequest() with
                {
                    Provenance = new Dictionary<string, string>
                    {
                        ["source"] = "unit-test",
                        ["scripted_defect_category"] =
                            DesignConversionNegativeReasonTaxonomy.Spacing
                    }
                },
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));
        AssertDpoFaultFixtureAttemptProgression(
            evaluator,
            expectInitialSyntheticNegative: false,
            expectTeacherAuthoredCandidates: true);
        var durableRecords = await artifactDb.ListRecordsAsync();
        var states = await stateStore.ListAsync();
        var actualArtifact = Assert.Single(durableRecords.Value!);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            actualArtifact,
            evaluator,
            root);
        var state = Assert.Single(states.Value!);
        var eligibility =
            await new DesignConversionPostEvaluationTrainingEligibilityPolicy(
                    stateStore)
                .EvaluateManyAsync([actualArtifact]);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .PostEvaluationStateCommitFailedErrorCode,
            result.ErrorCode);
        Assert.NotEqual(
            JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
            state.PersistenceStatus);
        Assert.False(Assert.Single(eligibility).IsEligible);
    }

    [Theory]
    [InlineData("before-delete")]
    [InlineData("after-delete")]
    public async Task RunAsync_DenialReleaseFailureReturnsOuterFailAndRestartedPolicyRemainsDenied(
        string failureStage)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "loop-denial-release-" + failureStage);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var stateRoot = Path.Combine(
            root,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        var stateStore =
            new ReleaseFailurePostEvaluationStateStore(
                stateRoot,
                failureStage);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "candidate-002",
                        "<main class=\"improved-intermediate\">intermediate</main>",
                        ".improved-intermediate{display:grid;}")),
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    CreateCandidate(
                        "candidate-003",
                        "<main class=\"improved-final\">final</main>",
                        ".improved-final{display:grid;}"))),
            artifactDb,
            postEvaluationStateStore: stateStore);

        var result = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: "denial-release-" + failureStage,
                InitialEvaluationRequest: CreateRequest() with
                {
                    Provenance = new Dictionary<string, string>
                    {
                        ["source"] = "unit-test",
                        ["scripted_defect_category"] =
                            DesignConversionNegativeReasonTaxonomy.Spacing
                    }
                },
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));
        AssertDpoFaultFixtureAttemptProgression(
            evaluator,
            expectInitialSyntheticNegative: false,
            expectTeacherAuthoredCandidates: true);
        var durableRecords = await artifactDb.ListRecordsAsync();
        Assert.True(durableRecords.IsSuccess, durableRecords.ErrorMessage);
        var actualArtifact = Assert.Single(durableRecords.Value!);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            actualArtifact,
            evaluator,
            root);
        var restartedStore =
            new JsonDesignConversionPostEvaluationStateStore(stateRoot);
        var eligibility =
            await new DesignConversionPostEvaluationTrainingEligibilityPolicy(
                    restartedStore)
                .EvaluateManyAsync([actualArtifact]);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .PostEvaluationStateCommitFailedErrorCode,
            result.ErrorCode);
        Assert.Equal(1, stateStore.InjectedFailures);
        Assert.False(Assert.Single(eligibility).IsEligible);
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
    public async Task RunAsync_WithLocalArtifactDbSingleAttemptAndRejectedMetadataDoesNotCreateDpoCandidate()
    {
        const string quarantineReasonPrefix = "runtime_created_dpo_candidate_quarantine_reason=";
        const string expectedQuarantineReason =
            quarantineReasonPrefix + "server_owned_route_admission_not_bound";
        const string fallbackQuarantineReason =
            quarantineReasonPrefix + "runtime_rejected_candidate_independent_evaluation_required";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "p1-loop-single-attempt-no-dpo");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var strategy = new ScriptedCandidateStrategy();
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.94),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.94, cost: 0m)),
            strategy,
            artifactDb,
            () => new DateTimeOffset(2026, 7, 31, 10, 0, 0, TimeSpan.Zero));
        var request = CreateRequest() with
        {
            Provenance = new Dictionary<string, string>
            {
                ["runtime_rejected_independently_evaluated"] = bool.TrueString,
                ["runtime_rejected_measured_score"] = "0.10",
                ["runtime_rejected_candidate_ref"] = "metadata-only-fake-rejected"
            },
            AiTeacherEvidenceRefs =
            [
                "runtime_rejected_candidate_metadata_bypass_attempt",
                "runtime_rejected_evidence_source=metadata"
            ]
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "p1-single-attempt-no-rejected",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, result.Value!.StopReason);
        Assert.Empty(strategy.Requests);
        Assert.Equal(1, result.Value.AttemptCount);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["runtime_created_dpo_candidate"]);
        Assert.Equal("0", result.Value.PublicMetadata["runtime_created_dpo_candidate_count"]);

        var records = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbDpoNoEligibleRejectedAttemptMarker]);
        Assert.DoesNotContain(
            fallbackQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            expectedQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            stored.Metadata["runtime_created_dpo_candidate_warnings"]
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(warning => warning.StartsWith(
                    quarantineReasonPrefix,
                    StringComparison.Ordinal)));
        Assert.DoesNotContain(fallbackQuarantineReason, result.Value.Warnings);
        Assert.Contains(expectedQuarantineReason, result.Value.Warnings);
        Assert.Equal(
            1,
            result.Value.Warnings.Count(warning => warning.StartsWith(
                quarantineReasonPrefix,
                StringComparison.Ordinal)));
        Assert.Contains(DesignConversionImprovementLoopCoordinator.LocalArtifactDbDpoNoEligibleRejectedAttemptMarker, stored.Metadata["runtime_created_dpo_candidate_warnings"]);
        Assert.Equal(bool.FalseString, stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.FalseString, stored.Metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
        Assert.Equal(bool.FalseString, stored.Metadata["production_dpo"]);
    }

    [Fact]
    public async Task RunAsync_ExplicitLocalEvaluatorRuntimeIssuesOneEvaluationPerTeacherPair()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "local-independent-evaluator-runtime");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        using var teacherAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: false);
        var evaluationClient = new CountingLocalIndependentEvaluationClient();
        var localEvaluator =
            DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
                evaluationClient,
                CountingLocalIndependentEvaluationClient.ModelId,
                CountingLocalIndependentEvaluationClient.ManifestDigest,
                CountingLocalIndependentEvaluationClient.ModelLayerDigest,
                DesignConversionDpoGenerator.Sha256Ref(
                    "coordinator-local-evaluator-runtime"));
        var ownedRuntime =
            DesignConversionLocalIndependentPreferenceEvaluatorRuntime
                .OwnForTesting(artifactDb, localEvaluator);
        Assert.True(ownedRuntime.IsSuccess, ownedRuntime.ErrorMessage);
        using var runtime = ownedRuntime.Value!;
        var scriptedEvaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var evaluator = new PairCapturingDpoFaultFixtureEvaluator(
            scriptedEvaluator);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(
                2026,
                8,
                7,
                9,
                30,
                0,
                TimeSpan.Zero));

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest("local-independent-evaluator-runtime"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, evaluationClient.CallCount);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await artifactDb.ListRecordsAsync()).Value!),
            scriptedEvaluator,
            root,
            "sha256:" +
            CountingLocalIndependentEvaluationClient.ModelLayerDigest);
        Assert.Equal(2, evaluator.RuntimePairs.Count);
        for (var index = 0; index < evaluator.RuntimePairs.Count; index++)
        {
            var pair = evaluator.RuntimePairs[index];
            Assert.Equal(index + 2, pair.AttemptNumber);
            Assert.Equal(
                scriptedEvaluator.Requests[index + 1].Html,
                pair.Generated.Html);
            Assert.Equal(
                scriptedEvaluator.Requests[index + 1].Css,
                pair.Generated.Css);
            Assert.Equal(
                scriptedEvaluator.Requests[index].Html,
                pair.Incumbent.Html);
            Assert.Equal(
                scriptedEvaluator.Requests[index].Css,
                pair.Incumbent.Css);
            Assert.NotEqual(pair.Generated.ContentSha256,
                pair.Incumbent.ContentSha256);
            Assert.NotEqual(pair.Generated.EvaluationIdentity,
                pair.Incumbent.EvaluationIdentity);
            Assert.Equal(
                DesignConversionIndependentPreferenceEvidenceContract
                    .GeneratedSide,
                pair.IndependentEvaluation.PreferredDirection);
        }
        Assert.DoesNotContain(
            typeof(DesignConversionDpoPolicyRuntimePair).GetProperties(),
            property => property.Name.Contains("Preferred",
                StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Chosen",
                            StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            result.Value!.Warnings,
            warning => warning.StartsWith(
                "LOCAL_INDEPENDENT_PREFERENCE_EVALUATION_FAILED:",
                StringComparison.Ordinal));

        runtime.Dispose();
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));
        Assert.True(evaluationClient.Disposed);
    }

    [Fact]
    public async Task RunAsync_UnavailableLocalEvaluatorFailsClosedWithZeroDpoAndNoFallback()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "local-independent-evaluator-unavailable");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        using var teacherAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: false);
        var evaluationClient = new CountingLocalIndependentEvaluationClient(
            "LOCAL_OLLAMA_UNAVAILABLE");
        var localEvaluator =
            DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
                evaluationClient,
                CountingLocalIndependentEvaluationClient.ModelId,
                CountingLocalIndependentEvaluationClient.ManifestDigest,
                CountingLocalIndependentEvaluationClient.ModelLayerDigest,
                DesignConversionDpoGenerator.Sha256Ref(
                    "coordinator-local-evaluator-unavailable"));
        var ownedRuntime =
            DesignConversionLocalIndependentPreferenceEvaluatorRuntime
                .OwnForTesting(artifactDb, localEvaluator);
        Assert.True(ownedRuntime.IsSuccess, ownedRuntime.ErrorMessage);
        using var runtime = ownedRuntime.Value!;
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            new DpoFaultFixtureEvaluator(markInitialSyntheticNegative: false),
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(
                2026,
                8,
                7,
                9,
                45,
                0,
                TimeSpan.Zero));

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest("local-independent-evaluator-unavailable"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, evaluationClient.CallCount);
        var stored = Assert.Single((await artifactDb.ListRecordsAsync()).Value!);
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal("0", stored.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.Equal(bool.FalseString,
            stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Contains(
            "LOCAL_INDEPENDENT_PREFERENCE_EVALUATION_FAILED:LOCAL_OLLAMA_UNAVAILABLE",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "independent_preference_proven=True",
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_ServerDisabledIgnoresCallerMetadataAndDoesNotCallGateway()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-disabled");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var gateway = new RecordingTrainingIntakePreflightGateway(_ =>
            throw new InvalidOperationException("Disabled preflight gateway must not be called."));
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(2026, 8, 7, 10, 0, 0, TimeSpan.Zero),
            trainingIntakePreflightGateway: gateway);

        var result = await coordinator.RunAsync(CreateDpoFaultLoopRequest(
            "a4-1-preflight-disabled",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_preflight_handoff_enabled"] = bool.TrueString,
                ["training_intake_preflight_dpo_step_count"] = "999999"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertDpoFaultFixtureAttemptProgression(
            evaluator,
            expectInitialSyntheticNegative: false,
            expectTeacherAuthoredCandidates: true);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await artifactDb.ListRecordsAsync()).Value!),
            evaluator,
            root);
        Assert.Equal(0, gateway.CallCount);
        Assert.False(result.Value!.PublicMetadata.ContainsKey(
            DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffStatusMetadataKey));
        Assert.False(result.Value.PublicMetadata.ContainsKey(
            DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffAdmittedMetadataKey));
    }

    [Fact]
    public async Task RunAsync_TrainingIntakePreflightHandoffEnabledEligibleCallsGatewayWithCanonicalGateAndStep()
    {
        using var testRoot = TestResultsDirectoryScope.Create(
            "il",
            $"pf-e-{Guid.NewGuid():N}");
        var root = testRoot.DirectoryPath;
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var gateway = new RecordingTrainingIntakePreflightGateway(_ =>
            OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>.Fail(
                "Request capture completed.",
                "TEST_CAPTURE_ONLY"));
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(2026, 8, 7, 10, 5, 0, TimeSpan.Zero),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2),
            trainingIntakePreflightGateway: gateway);

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest(
                "a4-1-preflight-enabled",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["training_intake_preflight_handoff_enabled"] =
                        bool.FalseString,
                    ["training_intake_preflight_dpo_step_count"] = "5",
                    ["coordinator_dpo_step_count"] = "5"
                }));

        Assert.True(
            result.IsSuccess,
            $"code={result.ErrorCode ?? "<null>"}; message={result.ErrorMessage}");
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await artifactDb.ListRecordsAsync()).Value!),
            evaluator,
            root);
        var request = Assert.Single(gateway.Requests);
        Assert.Equal("a4-1-preflight-enabled", request.LoopId);
        Assert.Equal(2, request.DpoStepCount);
        Assert.NotNull(request.MeasurementGateRequest);
        Assert.Equal(
            DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                "gate-id",
                request.MeasurementGateRequest.GateId),
            request.AdmissionBoundaryMetadata["coordinator_measurement_gate_id"]);
        Assert.DoesNotContain(
            request.MeasurementGateRequest.GateId,
            request.AdmissionBoundaryMetadata["coordinator_measurement_gate_id"],
            StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(
            request.AdmissionBoundaryMetadata["coordinator_measurement_gate_contract_ref"]));
        Assert.Equal(
            "2",
            request.AdmissionBoundaryMetadata["coordinator_dpo_step_count"]);
        Assert.Equal(
            request.IdempotencyKeyRef,
            request.AdmissionBoundaryMetadata["training_intake_idempotency_key_ref"]);
        Assert.Equal(
            request.StableIntakeId,
            request.AdmissionBoundaryMetadata["coordinator_stable_intake_id"]);
        Assert.False(string.IsNullOrWhiteSpace(request.StableIntakeId));
        Assert.False(string.IsNullOrWhiteSpace(request.IdempotencyKeyRef));
        Assert.NotEmpty(request.SourceEvidenceRefs);
        Assert.Contains("coordinator_training_intake_preflight_source_evidence", request.SourceEvidenceRefs);
        Assert.Contains(
            "artifact://measurement/a4-1-preflight-enabled/integrity.json",
            request.MeasurementGateRequest.IntegrityProof!.EvidenceRefs);
        Assert.Equal(bool.FalseString, request.AdmissionBoundaryMetadata["training_runner_invoked"]);
        Assert.Equal(bool.FalseString, request.AdmissionBoundaryMetadata["training_started"]);
        Assert.Equal(bool.FalseString, request.AdmissionBoundaryMetadata["checkpoint_mutated"]);
        Assert.Equal(bool.FalseString, request.AdmissionBoundaryMetadata["weights_produced"]);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffGatewayFailed,
            result.Value!.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffStatusMetadataKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffAdmittedMetadataKey]);
        Assert.Equal(
            "2",
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightConfiguredDpoStepCountMetadataKey]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_started"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["checkpoint_mutated"]);
    }

    [Fact]
    public async Task RunAsync_TrainingIntakePreflightHandoffEnabledUsesBridgeDefaultStepWhenCallerDoesNotOverride()
    {
        using var testRoot = TestResultsDirectoryScope.Create(
            "il",
            $"pf-d-{Guid.NewGuid():N}");
        var root = testRoot.DirectoryPath;
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var gateway = new RecordingTrainingIntakePreflightGateway(_ =>
            OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>.Fail(
                "Request capture completed.",
                "TEST_CAPTURE_ONLY"));
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(2026, 8, 10, 11, 0, 0, TimeSpan.Zero),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true),
            trainingIntakePreflightGateway: gateway);

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest("a4-1-preflight-default-step"));

        Assert.True(
            result.IsSuccess,
            $"code={result.ErrorCode ?? "<null>"}; message={result.ErrorMessage}");
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await artifactDb.ListRecordsAsync()).Value!),
            evaluator,
            root);
        var request = Assert.Single(gateway.Requests);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridgeOptions.DefaultDpoStepCount,
            request.DpoStepCount);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridgeOptions.DefaultDpoStepCount.ToString(
                CultureInfo.InvariantCulture),
            request.AdmissionBoundaryMetadata["coordinator_dpo_step_count"]);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridgeOptions.DefaultDpoStepCount.ToString(
                CultureInfo.InvariantCulture),
            result.Value!.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightConfiguredDpoStepCountMetadataKey]);
    }

    [Fact]
    public void TrainingIntakePreflightMeasurementGateContractRef_MatchesIndependentGoldenOracle()
    {
        var gate = CreateIndependentContractOracleGate();

        var independentCanonical =
            BuildIndependentTrainingIntakePreflightMeasurementGateCanonical(gate);
        var independentContractRef =
            BuildIndependentTrainingIntakePreflightMeasurementGateContractRef(gate);

        Assert.NotEmpty(independentCanonical);
        Assert.StartsWith("sha256:", independentContractRef, StringComparison.Ordinal);
        Assert.Equal(71, independentContractRef.Length);
        Assert.Equal(
            independentContractRef,
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(gate));
    }

    [Fact]
    public async Task RunAsync_MissingAdmissionMeasurementFinishesLoopButNeverCallsTrainingGateway()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing-measurement");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        using var teacherAssociation = DesignConversionDpoTeacherTestAssociation.Associate(artifactDb);
        var gateway = new RecordingTrainingIntakePreflightGateway(_ =>
            throw new InvalidOperationException("gateway must not be invoked"));
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            new DpoFaultFixtureEvaluator(),
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2),
            trainingIntakePreflightGateway: gateway);
        var request = CreateDpoFaultLoopRequest("missing-measurement") with
        {
            MeasurementGateRequest = null
        };

        var result = await coordinator.RunAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, gateway.CallCount);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffMeasurementBlocked,
            result.Value!.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffStatusMetadataKey]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_started"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_runner_invoked"]);
    }

    [Theory]
    [InlineData("below_score")]
    [InlineData("runtime")]
    [InlineData("cost")]
    [InlineData("ram")]
    [InlineData("leakage")]
    [InlineData("hidden_determinism")]
    [InlineData("ablation")]
    [InlineData("source_disjoint")]
    public async Task RunAsync_PresentFailingAdmissionNeverCallsTrainingGatewayOrRunner(
        string failure)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            failure);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        using var teacherAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(artifactDb);
        var gateway = new RecordingTrainingIntakePreflightGateway(_ =>
            throw new InvalidOperationException("gateway must not be invoked"));
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            new DpoFaultFixtureEvaluator(),
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2),
            trainingIntakePreflightGateway: gateway);
        var request = CreateDpoFaultLoopRequest(failure) with
        {
            MeasurementGateRequest = BuildFailingCoordinatorAdmissionMeasurement(
                failure)
        };

        var result = await coordinator.RunAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, gateway.CallCount);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .TrainingIntakePreflightHandoffMeasurementBlocked,
            result.Value!.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .TrainingIntakePreflightHandoffStatusMetadataKey]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_started"]);
        Assert.Equal(
            bool.FalseString,
            result.Value.PublicMetadata["training_runner_invoked"]);
    }

    [Fact]
    public async Task RunAsync_CallerMutationAfterInvocationCannotChangeCapturedMeasurement()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "measurement-snapshot");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var gateway = new RecordingTrainingIntakePreflightGateway(_ =>
            OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>.Fail(
                "capture only",
                "CAPTURE_ONLY"));
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2),
            trainingIntakePreflightGateway: gateway);
        var metricRefs = new[] { "artifact://measurement/snapshot/heldout.json" };
        var proofRefs = new[] { "artifact://measurement/snapshot/integrity.json" };
        var request = CreateDpoFaultLoopRequest("measurement-snapshot") with
        {
            MeasurementGateRequest = BuildPassingCoordinatorAdmissionMeasurement(
                "measurement-snapshot") with
            {
                HeldOutMetrics = BuildPassingCoordinatorAdmissionMeasurement(
                    "measurement-snapshot").HeldOutMetrics! with
                {
                    EvidenceRefs = metricRefs
                },
                IntegrityProof = BuildPassingCoordinatorAdmissionMeasurement(
                    "measurement-snapshot").IntegrityProof! with
                {
                    EvidenceRefs = proofRefs
                }
            }
        };

        var pending = coordinator.RunAsync(request);
        metricRefs[0] = "raw:mutated-private-measurement";
        proofRefs[0] = "private:mutated-proof";
        var result = await pending;

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var captured = Assert.Single(gateway.Requests).MeasurementGateRequest;
        Assert.Equal("artifact://measurement/snapshot/heldout.json", captured.HeldOutMetrics!.EvidenceRefs[0]);
        Assert.Equal("artifact://measurement/snapshot/integrity.json", captured.IntegrityProof!.EvidenceRefs[0]);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await artifactDb.ListRecordsAsync()).Value!),
            evaluator,
            root);
        Assert.Empty(gateway.RunAndPromoteRequests);
        Assert.Equal(0, gateway.RunTrainingIntakeAndPromoteLocalCalls);
        Assert.Equal(bool.FalseString, result.Value!.PublicMetadata["training_runner_invoked"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_started"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["checkpoint_mutated"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["weights_produced"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["model_upload_ready"]);
    }

    [Fact]
    public void TrainingIntakePreflightMeasurementGateContractRef_IsMutationSensitiveAndUnambiguous()
    {
        var gate = CreateIndependentContractOracleGate();
        var reorderedEvidenceGate = gate with
        {
            HeldOutMetrics = gate.HeldOutMetrics! with
            {
                EvidenceRefs = gate.HeldOutMetrics.EvidenceRefs.Reverse().ToArray()
            },
            IntegrityProof = gate.IntegrityProof! with
            {
                EvidenceRefs = gate.IntegrityProof.EvidenceRefs.Reverse().ToArray()
            }
        };

        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(gate),
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(
                    reorderedEvidenceGate));

        var changedEvidenceLeft = gate with
        {
            HeldOutMetrics = gate.HeldOutMetrics! with
            {
                EvidenceRefs =
                [
                    "artifact://oracle/heldout/a.json",
                    "artifact://oracle/heldout/b.json"
                ]
            }
        };
        var changedEvidenceRight = gate with
        {
            HeldOutMetrics = gate.HeldOutMetrics! with
            {
                EvidenceRefs =
                [
                    "artifact://oracle/heldout/a.json",
                    "artifact://oracle/heldout/c.json"
                ]
            }
        };
        Assert.NotEqual(
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(
                    changedEvidenceLeft),
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(
                    changedEvidenceRight));

        var nullCollection = gate with
        {
            HeldOutMetrics = gate.HeldOutMetrics! with { EvidenceRefs = null! }
        };
        var emptyCollection = gate with
        {
            HeldOutMetrics = gate.HeldOutMetrics! with { EvidenceRefs = [] }
        };
        Assert.Throws<ArgumentException>(() =>
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(
                    nullCollection));
        Assert.NotEqual(
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(gate),
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(
                    emptyCollection));

        Assert.Throws<ArgumentException>(() =>
            DesignConversionImprovementLoopCoordinator
                .BuildTrainingIntakePreflightMeasurementGateContractRef(
                    gate with { GateId = null! }));
    }

    [Fact]
    public async Task RunAsync_TrainingIntakePreflightOptInAdmittedCallsAtomicRunPromoteOnce()
    {
        const string intakeId = "intake-opt-in-admitted";
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-opt-in-admitted");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var gateway = new RecordingTrainingIntakePreflightGateway(
            request =>
            {
                var persistedBeforeGateway =
                    artifactDb.ListRecordsAsync().GetAwaiter().GetResult();
                Assert.True(
                    persistedBeforeGateway.IsSuccess,
                    persistedBeforeGateway.ErrorMessage);
                AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
                    Assert.Single(persistedBeforeGateway.Value!),
                    evaluator,
                    root);
                return OperationResult<
                    DesignConversionTrainingIntakePreflightHandoffResult>.Ok(
                    CreateAdmittedTrainingIntakePreflightHandoffResult(
                        request,
                        intakeId));
            },
            request =>
                OperationResult<
                    DesignConversionTrainingIntakeLocalPromotionResult>.Ok(
                    CreateSuccessfulTrainingIntakeLocalPromotionResult(
                        request.IntakeId,
                        root)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(2026, 8, 7, 10, 6, 0, TimeSpan.Zero),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2,
                    runTrainingIntakeAndPromoteLocal: true),
            trainingIntakePreflightGateway: gateway);

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest("a4-1-preflight-opt-in-admitted"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, gateway.CallCount);
        var preflightRequest = Assert.Single(gateway.Requests);
        var atomicRequest = Assert.Single(gateway.RunAndPromoteRequests);
        Assert.Equal(intakeId, atomicRequest.IntakeId);
        Assert.Equal(
            2,
            atomicRequest.TrainingOptions.DpoPolicyBridgeOptions!.DpoStepCount);
        Assert.Equal(
            preflightRequest.MeasurementGateRequest,
            atomicRequest.TrainingOptions.MeasurementGateRequest);
        Assert.Equal(
            preflightRequest.IdempotencyKeyRef,
            atomicRequest.AdmissionBoundaryMetadata[
                "training_intake_idempotency_key_ref"]);
        Assert.Equal(bool.TrueString, result.Value!.PublicMetadata[
            "training_intake_atomic_run_promote_invoked"]);
        Assert.Equal("atomic-run", result.Value.PublicMetadata[
            "training_intake_run_id"]);
        Assert.Equal("completed", result.Value.PublicMetadata[
            "training_intake_run_status"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[
            "training_started"]);
        Assert.Equal(bool.TrueString, result.Value.PublicMetadata[
            "training_intake_local_promotion_ready"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata[
            "training_intake_external_cloud_upload_attempted"]);
        Assert.False(result.Value.PublicMetadata.ContainsKey(
            "training_intake_local_promotion_output_directory"));
        Assert.False(result.Value.PublicMetadata.ContainsKey(
            "training_intake_local_promotion_checkpoint_path"));
        Assert.DoesNotContain(
            result.Value.PublicMetadata.Values,
            value => value.Contains(root, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            result.Value.PublicMetadata.Values,
            value => value.Contains(
                "fake-promotion-output",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            result.Value.PublicMetadata.Values,
            value => value.Contains(
                "fake-checkpoint.json",
                StringComparison.Ordinal));
    }

    [Fact]
    public void TrainingIntakePreflightCoordinatorOptions_RetainsBinaryConstructorAndDefaultsOptInFalse()
    {
        var binaryConstructor =
            typeof(DesignConversionTrainingIntakePreflightCoordinatorOptions)
                .GetConstructor([typeof(bool), typeof(int)]);

        Assert.NotNull(binaryConstructor);
        var defaults =
            new DesignConversionTrainingIntakePreflightCoordinatorOptions();
        var legacyArguments =
            new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                enabled: true,
                dpoStepCount: 2);
        var optIn =
            new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                enabled: true,
                dpoStepCount: 2,
                runTrainingIntakeAndPromoteLocal: true);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridgeOptions.DefaultDpoStepCount,
            defaults.DpoStepCount);
        Assert.Equal(2, legacyArguments.DpoStepCount);
        Assert.Equal(2, optIn.DpoStepCount);
        Assert.False(defaults.RunTrainingIntakeAndPromoteLocal);
        Assert.False(legacyArguments.RunTrainingIntakeAndPromoteLocal);
        Assert.True(optIn.RunTrainingIntakeAndPromoteLocal);
    }

    [Fact]
    public async Task RunAsync_TrainingIntakePreflightOptInBlockedPreflightDoesNotCallAtomicRunPromote()
    {
        const string intakeId = "intake-opt-in-blocked";
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-opt-in-blocked");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var gateway = new RecordingTrainingIntakePreflightGateway(
            request =>
                OperationResult<
                    DesignConversionTrainingIntakePreflightHandoffResult>.Ok(
                    CreateBlockedTrainingIntakePreflightHandoffResult(
                        request,
                        intakeId)));
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(2026, 8, 7, 10, 6, 30, TimeSpan.Zero),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2,
                    runTrainingIntakeAndPromoteLocal: true),
            trainingIntakePreflightGateway: gateway);

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest("a4-1-preflight-opt-in-blocked"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await artifactDb.ListRecordsAsync()).Value!),
            evaluator,
            root);
        Assert.Equal(1, gateway.CallCount);
        Assert.Empty(gateway.RunAndPromoteRequests);
        Assert.Equal(0, gateway.RunTrainingIntakeAndPromoteLocalCalls);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffPreflightBlocked,
            result.Value!.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffStatusMetadataKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffAdmittedMetadataKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.PublicMetadata["training_intake_preflight_may_start_training"]);
        Assert.Equal(
            "source_disjoint_blocked_same_source",
            result.Value.PublicMetadata[
                "training_intake_preflight_gateway_blocked_preflight_reason"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata[
            "training_runner_invoked"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata[
            "training_started"]);
        Assert.False(result.Value.PublicMetadata.ContainsKey(
            "training_intake_atomic_run_promote_invoked"));
        Assert.False(result.Value.PublicMetadata.ContainsKey(
            "training_intake_local_promotion_ready"));
        Assert.Contains(
            "training_intake_preflight_handoff_status:preflight_blocked",
            result.Value.Warnings);
        Assert.Contains(
            "source_disjoint_blocked_same_source",
            result.Value.Warnings);
    }

    [Fact]
    public async Task RunAsync_TrainingIntakePreflightOptInAdmittedAtomicFailureReturnsGatewayErrorWithoutPromotionClaim()
    {
        const string intakeId = "intake-opt-in-atomic-failure";
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-opt-in-atomic-failure");
        var inner = new JsonlDesignConversionLocalArtifactDb(root);
        var artifactDb = new FaultingLocalArtifactDb(
            inner,
            failAppend: false,
            failList: false);
        var gateway = new RecordingTrainingIntakePreflightGateway(
            request =>
                OperationResult<
                    DesignConversionTrainingIntakePreflightHandoffResult>.Ok(
                    CreateAdmittedTrainingIntakePreflightHandoffResult(
                        request,
                        intakeId)),
            _ =>
                OperationResult<
                    DesignConversionTrainingIntakeLocalPromotionResult>.Fail(
                    "Injected atomic boundary failure.",
                    "TEST_ATOMIC_BOUNDARY_FAILURE"));
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(2026, 8, 7, 10, 6, 45, TimeSpan.Zero),
            postEvaluationStateStore:
                new JsonDesignConversionPostEvaluationStateStore(Path.Combine(
                    root,
                    JsonDesignConversionPostEvaluationStateStore.DirectoryName)),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2,
                    runTrainingIntakeAndPromoteLocal: true),
            trainingIntakePreflightGateway: gateway);

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest("a4-1-preflight-opt-in-atomic-failure"));

        Assert.False(result.IsSuccess);
        Assert.Equal("TEST_ATOMIC_BOUNDARY_FAILURE", result.ErrorCode);
        Assert.Equal("Injected atomic boundary failure.", result.ErrorMessage);
        Assert.Null(result.Value);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await inner.ListRecordsAsync()).Value!),
            evaluator,
            root);
        Assert.Equal(1, gateway.CallCount);
        Assert.Equal(1, gateway.RunTrainingIntakeAndPromoteLocalCalls);
        var atomicRequest = Assert.Single(gateway.RunAndPromoteRequests);
        Assert.Equal(intakeId, atomicRequest.IntakeId);
        Assert.Equal(0, artifactDb.RunTrainingIntakePreflightCalls);
        Assert.Equal(0, artifactDb.RunTrainingIntakeCalls);
        Assert.Equal(0, artifactDb.RunTrainingIntakeAndPromoteLocalCalls);
    }

    [Fact]
    public async Task RunAsync_TrainingIntakePreflightOptInAdmittedAtomicExceptionReturnsTypedFailClosedResult()
    {
        const string intakeId = "intake-opt-in-atomic-exception";
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-opt-in-atomic-exception");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var gateway = new RecordingTrainingIntakePreflightGateway(
            request =>
                OperationResult<
                    DesignConversionTrainingIntakePreflightHandoffResult>.Ok(
                    CreateAdmittedTrainingIntakePreflightHandoffResult(
                        request,
                        intakeId)),
            _ => throw new InvalidOperationException(
                "private atomic gateway exception detail"));
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(
                2026,
                8,
                7,
                10,
                6,
                50,
                TimeSpan.Zero),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2,
                    runTrainingIntakeAndPromoteLocal: true),
            trainingIntakePreflightGateway: gateway);

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest(
                "a4-1-preflight-opt-in-atomic-exception"));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator
                .TrainingIntakeRunAndPromoteLocalFailedErrorCode,
            result.ErrorCode);
        Assert.Equal(
            "DesignConversion training-intake atomic local run and promotion failed closed.",
            result.ErrorMessage);
        Assert.Null(result.Value);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await artifactDb.ListRecordsAsync()).Value!),
            evaluator,
            root);
        Assert.Equal(1, gateway.CallCount);
        Assert.Equal(1, gateway.RunTrainingIntakeAndPromoteLocalCalls);
        Assert.DoesNotContain("private", result.ErrorMessage);
    }

    [Fact]
    public async Task RunAsync_ServerEnabledUsesRealLocalArtifactDbPreflightWithoutTrainingOrPromotion()
    {
        var oracleSourceRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-real-adapter-oracle-source");
        var oracleSourceDb = new JsonlDesignConversionLocalArtifactDb(
            oracleSourceRoot);
        var oracleSourceGateway = new RecordingTrainingIntakePreflightGateway(_ =>
            OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>.Fail(
                "Independent contract oracle input captured.",
                "TEST_INDEPENDENT_CONTRACT_ORACLE_INPUT_CAPTURED"));
        using var oracleSourceIndependentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                oracleSourceDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var oracleSourceEvaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        {
            var oracleSourceCoordinator =
                new DesignConversionImprovementLoopCoordinator(
                    oracleSourceEvaluator,
                    CreateDpoFaultCandidateStrategy(),
                    rejectedEvaluationHandoff: null,
                    artifactDb: oracleSourceDb,
                    utcNow: () => new DateTimeOffset(
                        2026,
                        8,
                        7,
                        10,
                        7,
                        0,
                        TimeSpan.Zero),
                    postEvaluationStateStore:
                        new JsonDesignConversionPostEvaluationStateStore(
                            Path.Combine(
                                oracleSourceRoot,
                                JsonDesignConversionPostEvaluationStateStore
                                    .DirectoryName)),
                    trainingIntakePreflightOptions:
                        new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                            enabled: true,
                            dpoStepCount: 2),
                    trainingIntakePreflightGateway: oracleSourceGateway);
            var oracleSourceResult = await oracleSourceCoordinator.RunAsync(
                CreateDpoFaultLoopRequest("a4-1-preflight-real-adapter"));
            Assert.True(
                oracleSourceResult.IsSuccess,
                oracleSourceResult.ErrorMessage);
            var oracleSourceRecords = await oracleSourceDb.ListRecordsAsync();
            Assert.True(
                oracleSourceRecords.IsSuccess,
                oracleSourceRecords.ErrorMessage);
            AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
                Assert.Single(oracleSourceRecords.Value!),
                oracleSourceEvaluator,
                oracleSourceRoot);
        }

        var expectedGateContractRef =
            BuildIndependentTrainingIntakePreflightMeasurementGateContractRef(
                Assert.Single(oracleSourceGateway.Requests)
                    .MeasurementGateRequest);
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-real-adapter");
        var inner = new JsonlDesignConversionLocalArtifactDb(root);
        var artifactDb = new FaultingLocalArtifactDb(
            inner,
            failAppend: false,
            failList: false);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            artifactDb,
            () => new DateTimeOffset(2026, 8, 7, 10, 7, 0, TimeSpan.Zero),
            postEvaluationStateStore:
                new JsonDesignConversionPostEvaluationStateStore(Path.Combine(
                    root,
                    JsonDesignConversionPostEvaluationStateStore.DirectoryName)),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2));

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest("a4-1-preflight-real-adapter"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var persistedRecords = await inner.ListRecordsAsync();
        Assert.True(persistedRecords.IsSuccess, persistedRecords.ErrorMessage);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single(persistedRecords.Value!),
            evaluator,
            root);
        Assert.Equal(1, artifactDb.ExportApprovedRuntimeDpoCandidatesCalls);
        Assert.Equal(1, artifactDb.CreateTrainingIntakeCalls);
        Assert.Equal(1, artifactDb.RunTrainingIntakePreflightCalls);
        Assert.Equal(0, artifactDb.RunTrainingIntakeCalls);
        Assert.Equal(0, artifactDb.RunTrainingIntakeAndPromoteLocalCalls);

        var intakes = await inner.ListTrainingIntakeRequestsAsync();
        Assert.True(intakes.IsSuccess, intakes.ErrorMessage);
        var intake = Assert.Single(intakes.Value!);
        var preflight = await inner.RunTrainingIntakePreflightAsync(intake.IntakeId);
        Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
        Assert.Equal(intake.IntakeId, preflight.Value!.TrainingIntakeId);
        Assert.Equal(
            intake.IntakeId,
            result.Value!.PublicMetadata["training_intake_preflight_actual_intake_id"]);
        Assert.Equal(
            preflight.Value.PreflightId,
            result.Value.PublicMetadata["training_intake_preflight_id"]);
        Assert.Equal(
            "2",
            intake.BoundaryMetadata["coordinator_dpo_step_count"]);
        Assert.Equal(
            result.Value.PublicMetadata["training_intake_preflight_stable_intake_id"],
            intake.BoundaryMetadata["coordinator_stable_intake_id"]);
        Assert.Equal(
            result.Value.PublicMetadata["training_intake_preflight_idempotency_key_ref"],
            intake.BoundaryMetadata["training_intake_idempotency_key_ref"]);
        Assert.StartsWith(
            "sha256:",
            intake.BoundaryMetadata["coordinator_measurement_gate_id"],
            StringComparison.Ordinal);
        var expectedMeasurementGateUnitId =
            DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                "unit-id",
                DesignConversionDpoUnits.CriticUnit);
        Assert.StartsWith(
            "sha256:",
            expectedMeasurementGateUnitId,
            StringComparison.Ordinal);
        Assert.NotEqual(
            DesignConversionDpoUnits.CriticUnit,
            expectedMeasurementGateUnitId);
        Assert.Equal(
            expectedMeasurementGateUnitId,
            intake.BoundaryMetadata["coordinator_measurement_gate_unit_id"]);
        var persistedGateContractRef =
            intake.BoundaryMetadata["coordinator_measurement_gate_contract_ref"];
        Assert.Equal(expectedGateContractRef, persistedGateContractRef);
        Assert.Equal(
            persistedGateContractRef,
            result.Value.PublicMetadata[
                "training_intake_preflight_gateway_measurement_gate_contract_ref"]);
        Assert.Equal(
            expectedMeasurementGateUnitId,
            result.Value.PublicMetadata[
                "training_intake_preflight_gateway_measurement_gate_unit_id"]);
        Assert.NotEqual(
            DesignConversionDpoUnits.CriticUnit,
            result.Value.PublicMetadata[
                "training_intake_preflight_gateway_measurement_gate_unit_id"]);
        Assert.Equal(
            "2",
            result.Value.PublicMetadata[
                "training_intake_preflight_gateway_dpo_step_count"]);
        Assert.Equal(
            preflight.Value.MayStartTraining
                ? DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffPreflightAdmitted
                : DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffPreflightBlocked,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffStatusMetadataKey]);
        Assert.Contains(intake.IntakeRef, result.Value.EvidenceRefs);
        Assert.Contains(preflight.Value.PreflightId, result.Value.EvidenceRefs);
        Assert.Contains(
            "coordinator_training_intake_preflight_gateway_evidence",
            result.Value.EvidenceRefs);
        Assert.Contains(persistedGateContractRef, result.Value.EvidenceRefs);
        Assert.Contains(
            "training_intake_preflight_dpo_step_count=2",
            result.Value.EvidenceRefs);
        Assert.False(preflight.Value.TrainingStarted);
        Assert.False(preflight.Value.CheckpointMutated);
        Assert.False(preflight.Value.WeightsProduced);
        Assert.False(preflight.Value.ModelUploadReady);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_runner_invoked"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_started"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["checkpoint_mutated"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["weights_produced"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["model_upload_ready"]);
    }

    [Theory]
    [InlineData("coordinator_measurement_gate_id", true)]
    [InlineData("coordinator_measurement_gate_id", false)]
    [InlineData("coordinator_measurement_gate_unit_id", true)]
    [InlineData("coordinator_measurement_gate_unit_id", false)]
    [InlineData("coordinator_measurement_gate_contract_ref", true)]
    [InlineData("coordinator_measurement_gate_contract_ref", false)]
    [InlineData("coordinator_dpo_step_count", true)]
    [InlineData("coordinator_dpo_step_count", false)]
    [InlineData("coordinator_stable_intake_id", true)]
    [InlineData("coordinator_stable_intake_id", false)]
    [InlineData("training_intake_idempotency_key_ref", true)]
    [InlineData("training_intake_idempotency_key_ref", false)]
    public async Task RunAsync_RealLocalArtifactDbRejectsMissingOrMutatedPersistedPreflightContract(
        string metadataKey,
        bool removeMetadata)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-persisted-contract-rejection");
        var inner = new JsonlDesignConversionLocalArtifactDb(root);
        var artifactDb = new FaultingLocalArtifactDb(
            inner,
            failAppend: false,
            failList: false,
            trainingIntakeMetadataKeyToAlter: metadataKey,
            removeTrainingIntakeMetadata: removeMetadata);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            artifactDb,
            () => new DateTimeOffset(2026, 8, 7, 10, 8, 0, TimeSpan.Zero),
            postEvaluationStateStore:
                new JsonDesignConversionPostEvaluationStateStore(Path.Combine(
                    root,
                    JsonDesignConversionPostEvaluationStateStore.DirectoryName)),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true,
                    dpoStepCount: 2));

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest(
                $"a4-1-preflight-persisted-contract-{metadataKey}-{removeMetadata}"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var persistedRecords = await inner.ListRecordsAsync();
        Assert.True(persistedRecords.IsSuccess, persistedRecords.ErrorMessage);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single(persistedRecords.Value!),
            evaluator,
            root);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffGatewayFailed,
            result.Value!.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffStatusMetadataKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffAdmittedMetadataKey]);
        Assert.Equal(1, artifactDb.ExportApprovedRuntimeDpoCandidatesCalls);
        Assert.Equal(1, artifactDb.CreateTrainingIntakeCalls);
        Assert.Equal(0, artifactDb.RunTrainingIntakePreflightCalls);
        Assert.Equal(0, artifactDb.RunTrainingIntakeCalls);
        Assert.Equal(0, artifactDb.RunTrainingIntakeAndPromoteLocalCalls);
    }

    [Fact]
    public async Task RunAsync_TrainingIntakePreflightHandoffEnabledWithoutEligibleDpoCandidateDoesNotCallGateway()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-ineligible");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var gateway = new RecordingTrainingIntakePreflightGateway(_ =>
            throw new InvalidOperationException("Ineligible preflight gateway must not be called."));
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.94),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.94, cost: 0m)),
            new ScriptedCandidateStrategy(),
            artifactDb,
            () => new DateTimeOffset(2026, 8, 7, 10, 10, 0, TimeSpan.Zero),
            trainingIntakePreflightGateway: gateway,
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true));

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "a4-1-preflight-ineligible",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>())
        {
            MeasurementGateRequest =
                BuildPassingCoordinatorAdmissionMeasurement("a4-1-preflight-ineligible")
        });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, gateway.CallCount);
        Assert.Equal(
            DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffSkippedNotEligible,
            result.Value!.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffStatusMetadataKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffAdmittedMetadataKey]);
    }

    [Theory]
    [InlineData("gateway-failure")]
    [InlineData("evidence-free-success")]
    public async Task RunAsync_TrainingIntakePreflightHandoffCannotLookAdmittedWithoutGatewayEvidence(
        string mode)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "a4-1-preflight-" + mode);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var gateway = new RecordingTrainingIntakePreflightGateway(_ =>
            mode == "gateway-failure"
                ? OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>.Fail(
                    "Injected gateway failure.",
                    "TEST_GATEWAY_FAILURE")
                : OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>.Ok(
                    new DesignConversionTrainingIntakePreflightHandoffResult(
                        Intake: null,
                        Preflight: null,
                        EvidenceRefs: [],
                        Warnings: ["test_evidence_free_success"],
                        BoundaryMetadata:
                            new Dictionary<string, string>(StringComparer.Ordinal))));
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation.Associate(
                artifactDb,
                supportTeacherAuthoring: true,
                supportIndependentPreferenceEvidence: true);
        var evaluator = new DpoFaultFixtureEvaluator(
            markInitialSyntheticNegative: false);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            CreateDpoFaultCandidateStrategy(),
            rejectedEvaluationHandoff: null,
            artifactDb: artifactDb,
            utcNow: () => new DateTimeOffset(2026, 8, 7, 10, 15, 0, TimeSpan.Zero),
            trainingIntakePreflightOptions:
                new DesignConversionTrainingIntakePreflightCoordinatorOptions(
                    enabled: true),
            trainingIntakePreflightGateway: gateway);

        var result = await coordinator.RunAsync(
            CreateDpoFaultLoopRequest("a4-1-preflight-" + mode));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
            Assert.Single((await artifactDb.ListRecordsAsync()).Value!),
            evaluator,
            root);
        Assert.Equal(1, gateway.CallCount);
        Assert.Equal(
            mode == "gateway-failure"
                ? DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffGatewayFailed
                : DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffGatewayEvidenceMissing,
            result.Value!.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffStatusMetadataKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.PublicMetadata[
                DesignConversionImprovementLoopCoordinator.TrainingIntakePreflightHandoffAdmittedMetadataKey]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_runner_invoked"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["training_started"]);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["weights_produced"]);
    }

    [Fact]
    public async Task RunAsync_WithLocalArtifactDbSameScoreMeasuredRejectedCandidateCreatesNoDpoCandidate()
    {
        const string quarantineReasonPrefix = "runtime_created_dpo_candidate_quarantine_reason=";
        const string expectedQuarantineReason =
            quarantineReasonPrefix + "server_owned_route_admission_not_bound";
        const string fallbackQuarantineReason =
            quarantineReasonPrefix + "runtime_rejected_candidate_independent_evaluation_required";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "p1-loop-same-score-no-dpo");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var evaluator = new RecordingScriptedLoopEvaluator(attemptNumber => attemptNumber switch
        {
            1 => 0.82,
            _ => 0.82
        });
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(2)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            strategy,
            artifactDb,
            () => new DateTimeOffset(2026, 7, 31, 10, 5, 0, TimeSpan.Zero));
        var request = CreateRequest() with
        {
            Budget = CreateBudget(maxImprovementCandidates: 10, maxRenderQaRuns: 2, budgetUsd: 10m) with
            {
                Tier = "enterprise"
            }
        };

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "p1-same-score-no-rejected",
            InitialEvaluationRequest: request,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["max_consecutive_no_improvement"] = "1"
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, result.Value!.StopReason);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Single(strategy.Requests);
        Assert.Equal(["pass128-job", "pass128-job:loop-attempt:2"], evaluator.Requests.Select(item => item.JobId).ToArray());
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["runtime_created_dpo_candidate"]);
        Assert.Equal("0", result.Value.PublicMetadata["runtime_created_dpo_candidate_count"]);

        var records = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.Equal("pass128-job", stored.JobId);
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbDpoNoEligibleRejectedAttemptMarker]);
        Assert.DoesNotContain(
            fallbackQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            expectedQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            stored.Metadata["runtime_created_dpo_candidate_warnings"]
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(warning => warning.StartsWith(
                    quarantineReasonPrefix,
                    StringComparison.Ordinal)));
        Assert.DoesNotContain(fallbackQuarantineReason, result.Value.Warnings);
        Assert.Contains(expectedQuarantineReason, result.Value.Warnings);
        Assert.Equal(
            1,
            result.Value.Warnings.Count(warning => warning.StartsWith(
                quarantineReasonPrefix,
                StringComparison.Ordinal)));
        Assert.Equal(bool.FalseString, stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.FalseString, stored.Metadata["production_dpo"]);
        Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
    }

    [Fact]
    public async Task RunAsync_SameRawJobIdAcrossDistinctOutputsDoesNotSelectRejectedEvaluation()
    {
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        var evaluator = new SameRawJobIdLoopEvaluator("same-raw-job-id");
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            new ScriptedCandidateStrategy(
                OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(2)),
                OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(3))),
            handoff);

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "same-raw-job-id-loop",
            InitialEvaluationRequest: CreateRequest() with
            {
                JobId = "same-raw-job-id",
                RequestId = "same-raw-job-id-request"
            },
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.AttemptCount);
        Assert.Equal(0, handoff.PendingCount);
    }

    [Fact]
    public async Task RunAsync_WithLocalArtifactDbUnmeasuredCandidateCreatesNoDpoCandidate()
    {
        const string quarantineReasonPrefix = "runtime_created_dpo_candidate_quarantine_reason=";
        const string expectedQuarantineReason =
            quarantineReasonPrefix + "server_owned_route_admission_not_bound";
        const string fallbackQuarantineReason =
            quarantineReasonPrefix + "runtime_rejected_candidate_independent_evaluation_required";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "p1-loop-unmeasured-candidate-no-dpo");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var evaluator = new FailingAfterCountEvaluator(
            new DesignConversionInternalServiceThinClientJobEvaluator(
                new DesignConversionInternalService(
                    new RecordingRenderProvider(),
                    new RecordingComparisonProvider(0.70),
                    new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0m)))),
            failOnCall: 2);
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(CreateLoopCandidate(2)));
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            evaluator,
            strategy,
            artifactDb,
            () => new DateTimeOffset(2026, 7, 31, 10, 10, 0, TimeSpan.Zero));

        var result = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "p1-unmeasured-candidate-no-rejected",
            InitialEvaluationRequest: CreateRequest(),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.EvaluationFailed, result.Value!.StopReason);
        Assert.Equal(2, result.Value.AttemptCount);
        Assert.Single(strategy.Requests);
        Assert.Equal(bool.FalseString, result.Value.PublicMetadata["runtime_created_dpo_candidate"]);
        Assert.Equal("0", result.Value.PublicMetadata["runtime_created_dpo_candidate_count"]);

        var records = await new JsonlDesignConversionLocalArtifactDb(root).ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        Assert.Equal("pass128-job", stored.JobId);
        Assert.Empty(stored.DpoCandidates);
        Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbDpoNoEligibleRejectedAttemptMarker]);
        Assert.DoesNotContain(
            fallbackQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Contains(
            expectedQuarantineReason,
            stored.Metadata["runtime_created_dpo_candidate_warnings"],
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            stored.Metadata["runtime_created_dpo_candidate_warnings"]
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(warning => warning.StartsWith(
                    quarantineReasonPrefix,
                    StringComparison.Ordinal)));
        Assert.DoesNotContain(fallbackQuarantineReason, result.Value.Warnings);
        Assert.Contains(expectedQuarantineReason, result.Value.Warnings);
        Assert.Equal(
            1,
            result.Value.Warnings.Count(warning => warning.StartsWith(
                quarantineReasonPrefix,
                StringComparison.Ordinal)));
        Assert.Equal(bool.FalseString, stored.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.FalseString, stored.Metadata["production_dpo"]);
        Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
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
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "cancelled-skips-local-artifact-db");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
        var coordinator = CreateCoordinator(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(0.70),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, cost: 0m)),
            new ScriptedCandidateStrategy(),
            artifactDb: artifactDb);
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
        Assert.False(result.Value.PublicMetadata.ContainsKey(
            DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistoryStatusMetadataKey));
        var records = await artifactDb.ListRecordsAsync();
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Empty(records.Value!);
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
        Assert.Equal(1, strategy.Requests.Count);
        Assert.All(strategy.Requests, strategyRequest =>
        {
            Assert.Equal(1, strategyRequest.CompletedAttemptNumber);
            Assert.Equal(1, Assert.Single(strategyRequest.AttemptHistory).AttemptNumber);
        });
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
        TimeSpan? runDeadline = null,
        IDesignConversionPostEvaluationStateStore?
            postEvaluationStateStore = null,
        IDesignConversionRuntimeRejectedEvaluationHandoff?
            rejectedEvaluationHandoff = null,
        IDesignConversionTrainingIntakePreflightHandoffGateway?
            trainingIntakePreflightGateway = null,
        DesignConversionTrainingIntakePreflightCoordinatorOptions?
            trainingIntakePreflightOptions = null)
        => new(
            new DesignConversionInternalServiceThinClientJobEvaluator(
                new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge)),
            strategy,
            rejectedEvaluationHandoff,
            artifactDb,
            utcNow,
            runDeadline,
            postEvaluationStateStore,
            trainingIntakePreflightOptions,
            trainingIntakePreflightGateway);

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
        int expectedRowCount,
        string originalHtmlMarker,
        string originalCssMarker,
        string originalTreeMarker,
        IReadOnlyList<string> privateMarkupFragments)
    {
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.Equal(
            DesignConversionModelServerDpoUsefulnessHandler.LocalDpoUsefulnessRouteBoundary,
            response.Headers["x-design-conversion-dpo-usefulness-route"]);
        Assert.DoesNotContain(originalHtmlMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(originalCssMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(originalTreeMarker, response.BodyJson, StringComparison.Ordinal);
        foreach (var privateMarkupFragment in privateMarkupFragments)
            Assert.DoesNotContain(privateMarkupFragment, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", response.BodyJson, StringComparison.OrdinalIgnoreCase);
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

        var rows = root.GetProperty("rows").EnumerateArray().ToArray();
        Assert.Equal(expectedRowCount, rows.Length);
        foreach (var row in rows)
        {
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
                Assert.DoesNotContain("C:/Users/lubao", reference, StringComparison.OrdinalIgnoreCase);
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
        }

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

    private static ScriptedCandidateStrategy CreateDpoFaultCandidateStrategy()
        => new(
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate(
                    "candidate-002",
                    "<main class=\"improved-intermediate\">intermediate</main>",
                    ".improved-intermediate{display:grid;}")),
            OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate(
                    "candidate-003",
                    "<main class=\"improved-final\">final</main>",
                    ".improved-final{display:grid;}")));

    private static DesignConversionImprovementLoopRequest CreateDpoFaultLoopRequest(
        string loopId,
        IReadOnlyDictionary<string, string>? metadata = null)
        => new DesignConversionImprovementLoopRequest(
                LoopId: loopId,
                InitialEvaluationRequest: CreateRequest() with
                {
                    Provenance = new Dictionary<string, string>
                    {
                        ["source"] = "unit-test",
                        ["scripted_defect_category"] =
                            DesignConversionNegativeReasonTaxonomy.Spacing
                    }
                },
                MaxAttempts: 3,
                Metadata: metadata ??
                    new Dictionary<string, string>(StringComparer.Ordinal))
            {
                MeasurementGateRequest = BuildPassingCoordinatorAdmissionMeasurement(loopId)
            };

    private static DesignConversionMeasurementGateRequest
        BuildPassingCoordinatorAdmissionMeasurement(string scope)
        => new(
            GateId: $"admission:{scope}",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                2,
                0.95,
                10,
                0.01m,
                64,
                [$"artifact://measurement/{scope}/heldout.json"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                true,
                false,
                false,
                true,
                [$"artifact://measurement/{scope}/integrity.json"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                0.90,
                100,
                1m,
                512));

    private static DesignConversionMeasurementGateRequest
        BuildFailingCoordinatorAdmissionMeasurement(string failure)
    {
        var passing = BuildPassingCoordinatorAdmissionMeasurement(failure);
        return failure switch
        {
            "below_score" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with
                {
                    HeldOutQualityScore = 0.5
                }
            },
            "runtime" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { RuntimeMs = 101 }
            },
            "cost" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { CostUsd = 1.01m }
            },
            "ram" => passing with
            {
                HeldOutMetrics = passing.HeldOutMetrics! with { PeakRamMb = 513 }
            },
            "leakage" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { LeakageDetected = true }
            },
            "hidden_determinism" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with
                {
                    HiddenDeterminismDetected = true
                }
            },
            "ablation" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { AblationPassed = false }
            },
            "source_disjoint" => passing with
            {
                IntegrityProof = passing.IntegrityProof! with { SourceDisjoint = false }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(failure))
        };
    }

    private static DesignConversionMeasurementGateRequest
        CreateIndependentContractOracleGate()
        => new(
            GateId: "gate.oracle.1",
            UnitId: "critic:unit",
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 2,
                HeldOutQualityScore: 0.875,
                RuntimeMs: 17,
                CostUsd: 1.25m,
                PeakRamMb: 256,
                EvidenceRefs:
                [
                    "artifact://oracle/heldout/z.json",
                    "artifact://oracle/heldout/a.json",
                    "ref:oracle:heldout-mid"
                ]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: null,
                AblationPassed: true,
                EvidenceRefs:
                [
                    "artifact://oracle/integrity/proof-2.json",
                    "artifact://oracle/integrity/proof-1.json"
                ]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.9,
                MaxRuntimeMs: 30,
                MaxCostUsd: 2.5m,
                MaxPeakRamMb: 512));

    private static string
        BuildIndependentTrainingIntakePreflightMeasurementGateContractRef(
            DesignConversionMeasurementGateRequest gate)
    {
        var canonical =
            BuildIndependentTrainingIntakePreflightMeasurementGateCanonical(
                gate);
        var digest = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
        return "sha256:" + digest;
    }

    // Test-owned contract oracle: field names, ordering, encoding, and SHA-256
    // are specified here without calling production canonicalization helpers.
    private static string
        BuildIndependentTrainingIntakePreflightMeasurementGateCanonical(
            DesignConversionMeasurementGateRequest gate)
    {
        var canonical = new StringBuilder();
        AppendIndependentCanonicalField(
            canonical,
            "schema",
            "design-conversion-caller-admission-measurement-v1");
        AppendIndependentCanonicalField(canonical, "gate_id", gate.GateId);
        AppendIndependentCanonicalField(canonical, "unit_id", gate.UnitId);

        var metrics = gate.HeldOutMetrics;
        AppendIndependentCanonicalField(
            canonical,
            "held_out_record_count",
            metrics?.HeldOutRecordCount?.ToString(
                CultureInfo.InvariantCulture));
        AppendIndependentCanonicalField(
            canonical,
            "held_out_quality_score",
            metrics?.HeldOutQualityScore?.ToString(
                "R",
                CultureInfo.InvariantCulture));
        AppendIndependentCanonicalField(
            canonical,
            "runtime_ms",
            metrics?.RuntimeMs?.ToString(CultureInfo.InvariantCulture));
        AppendIndependentCanonicalField(
            canonical,
            "cost_usd",
            metrics?.CostUsd?.ToString("G29", CultureInfo.InvariantCulture));
        AppendIndependentCanonicalField(
            canonical,
            "peak_ram_mb",
            metrics?.PeakRamMb?.ToString(CultureInfo.InvariantCulture));
        AppendIndependentCanonicalCollection(
            canonical,
            "held_out_evidence",
            metrics?.EvidenceRefs);

        var integrity = gate.IntegrityProof;
        AppendIndependentCanonicalField(
            canonical,
            "source_disjoint",
            FormatIndependentCanonicalBoolean(integrity?.SourceDisjoint));
        AppendIndependentCanonicalField(
            canonical,
            "leakage_detected",
            FormatIndependentCanonicalBoolean(integrity?.LeakageDetected));
        AppendIndependentCanonicalField(
            canonical,
            "hidden_determinism_detected",
            FormatIndependentCanonicalBoolean(
                integrity?.HiddenDeterminismDetected));
        AppendIndependentCanonicalField(
            canonical,
            "ablation_passed",
            FormatIndependentCanonicalBoolean(integrity?.AblationPassed));
        AppendIndependentCanonicalCollection(
            canonical,
            "integrity_evidence",
            integrity?.EvidenceRefs);

        var thresholds = gate.Thresholds;
        AppendIndependentCanonicalField(
            canonical,
            "required_quality",
            thresholds?.RequiredHeldOutQualityScore.ToString(
                "R",
                CultureInfo.InvariantCulture));
        AppendIndependentCanonicalField(
            canonical,
            "max_runtime_ms",
            thresholds?.MaxRuntimeMs.ToString(CultureInfo.InvariantCulture));
        AppendIndependentCanonicalField(
            canonical,
            "max_cost_usd",
            thresholds?.MaxCostUsd.ToString(
                "G29",
                CultureInfo.InvariantCulture));
        AppendIndependentCanonicalField(
            canonical,
            "max_peak_ram_mb",
            thresholds?.MaxPeakRamMb.ToString(CultureInfo.InvariantCulture));
        return canonical.ToString();
    }

    private static DesignConversionTrainingIntakePreflightHandoffResult
        CreateAdmittedTrainingIntakePreflightHandoffResult(
            DesignConversionTrainingIntakePreflightHandoffRequest request,
            string intakeId)
    {
        var intake = new DesignConversionTrainingIntakeRecord(
            IntakeId: intakeId,
            IntakeRef: "intake-ref-opt-in-admitted",
            SourceHandoffArtifactId: "handoff-opt-in-admitted",
            SourceHandoffArtifactRef: "handoff-ref-opt-in-admitted",
            ApprovedCandidateCount: 1,
            AcceptedCandidateCount: 1,
            CandidateRefs: [],
            RequestedAtUtc: request.RequestedAtUtc,
            RequestedByRef: request.RequestedByRef,
            RequestReason: request.Reason,
            TrainingIntakeOnly: true,
            TrainingNotStarted: true,
            NotTrained: true,
            WeightsNotProduced: true,
            CheckpointNotMutated: true,
            ModelUploadNotReady: true,
            ProductNotReady: true,
            HeldOutNotProven: false,
            SourceDisjointNotProven: false,
            Warnings: [],
            BoundaryMetadata: request.AdmissionBoundaryMetadata);
        var preflight = new DesignConversionTrainingIntakePreflightResult(
            PreflightId: "preflight-opt-in-admitted",
            TrainingIntakeId: intakeId,
            AcceptedCandidateCount: 1,
            MayStartTraining: true,
            SourceManifestStatus: "validated",
            SourceDisjointStatus: "validated",
            TrainingStarted: false,
            CheckpointMutated: false,
            WeightsProduced: false,
            ModelUploadReady: false,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata:
                new Dictionary<string, string>(StringComparer.Ordinal));
        return new DesignConversionTrainingIntakePreflightHandoffResult(
            Intake: intake,
            Preflight: preflight,
            EvidenceRefs: ["preflight-opt-in-admitted-evidence"],
            Warnings: [],
            BoundaryMetadata:
                new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private static DesignConversionTrainingIntakePreflightHandoffResult
        CreateBlockedTrainingIntakePreflightHandoffResult(
            DesignConversionTrainingIntakePreflightHandoffRequest request,
            string intakeId)
    {
        var admitted =
            CreateAdmittedTrainingIntakePreflightHandoffResult(
                request,
                intakeId);
        return admitted with
        {
            Preflight = admitted.Preflight! with
            {
                PreflightId = "preflight-opt-in-blocked",
                MayStartTraining = false,
                SourceDisjointStatus = "blocked_same_source",
                BlockReasons = ["source_disjoint_blocked_same_source"]
            },
            EvidenceRefs = ["preflight-opt-in-blocked-evidence"],
            Warnings = ["source_disjoint_blocked_same_source"],
            BoundaryMetadata =
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["blocked_preflight_reason"] =
                        "source_disjoint_blocked_same_source"
                }
        };
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult
        CreateSuccessfulTrainingIntakeLocalPromotionResult(
            string intakeId,
            string testRoot)
    {
        var run = new DesignConversionTrainingIntakeRunResult(
            RunId: "atomic-run",
            TrainingIntakeId: intakeId,
            PreflightId: "preflight-opt-in-admitted",
            AcceptedCandidateCount: 1,
            ResolvedTrainingRecordCount: 1,
            MayStartTraining: true,
            SourceManifestStatus: "validated",
            SourceDisjointStatus: "validated",
            TrainingRunnerInvoked: true,
            LocalLifecycleHookAttempted: true,
            TrainingStarted: true,
            CheckpointMutated: true,
            CheckpointCandidateCreated: true,
            WeightsProduced: true,
            ModelUploadReady: true,
            TrainingExecutionBoundary: "atomic-test-boundary",
            TrainingExecutionStatus: "completed",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: "created",
            CheckpointMutationStatus: "mutated",
            WeightProofStatus: "real_weights_proven",
            MeasurementGateStatus: DesignConversionMeasurementGateStatus.Passed,
            PromotionGateStatus: "passed",
            UploadReadinessStatus: "ready",
            PromotionBlockedReasons: [],
            RunnerEvidence: null,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata:
                new Dictionary<string, string>(StringComparer.Ordinal));
        return new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: run,
            LocalPromotionReady: true,
            ExternalCloudUploadAttempted: false,
            PromotionOutputDirectory:
                Path.Combine(testRoot, "fake-promotion-output"),
            PromotedCheckpointPath:
                Path.Combine(
                    testRoot,
                    "fake-promotion-output",
                    "fake-checkpoint.json"),
            Publication: null,
            PromotedLocalModel: null,
            EvidenceRefs: ["atomic-run-promote-evidence"],
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata:
                new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private static string? FormatIndependentCanonicalBoolean(bool? value)
        => value switch
        {
            true => "true",
            false => "false",
            null => null
        };

    private static void AppendIndependentCanonicalCollection(
        StringBuilder canonical,
        string name,
        IReadOnlyList<string>? values)
    {
        AppendIndependentCanonicalField(
            canonical,
            $"{name}.count",
            values?.Count.ToString(CultureInfo.InvariantCulture));
        if (values is null)
            return;

        var index = 0;
        foreach (var value in values.Order(StringComparer.Ordinal))
        {
            AppendIndependentCanonicalField(
                canonical,
                $"{name}.{index.ToString(CultureInfo.InvariantCulture)}",
                value);
            index++;
        }
    }

    private static void AppendIndependentCanonicalField(
        StringBuilder canonical,
        string name,
        string? value)
    {
        canonical.Append(name.Length.ToString(CultureInfo.InvariantCulture));
        canonical.Append(':');
        canonical.Append(name);
        canonical.Append('=');
        if (value is null)
        {
            canonical.Append("N;");
            return;
        }

        canonical.Append('V');
        canonical.Append(value.Length.ToString(CultureInfo.InvariantCulture));
        canonical.Append(':');
        canonical.Append(value);
        canonical.Append(';');
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
        decimal budgetUsd = 1.00m,
        int? maxTeacherCalls = null)
        => new(
            UserId: "user-redacted",
            Tier: "pro",
            MaxImprovementCandidates: maxImprovementCandidates,
            MaxRenderQaRuns: maxRenderQaRuns,
            BudgetUsd: budgetUsd,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.90"
            },
            MaxTeacherCalls: maxTeacherCalls);

    private static void AssertDpoFaultFixtureAttemptProgression(
        DpoFaultFixtureEvaluator evaluator,
        bool expectInitialSyntheticNegative = true,
        bool expectTeacherAuthoredCandidates = false)
    {
        Assert.Equal(3, evaluator.Requests.Count);
        Assert.Equal(
            [
                "pass128-job",
                "pass128-job:loop-attempt:2",
                "pass128-job:loop-attempt:3"
            ],
            evaluator.Requests.Select(request => request.JobId).ToArray());
        Assert.Equal("pass128-request", evaluator.Requests[0].RequestId);
        if (expectTeacherAuthoredCandidates)
        {
            Assert.StartsWith(
                "teacher-authored-request:",
                evaluator.Requests[1].RequestId,
                StringComparison.Ordinal);
            Assert.StartsWith(
                "teacher-authored-request:",
                evaluator.Requests[2].RequestId,
                StringComparison.Ordinal);
            Assert.StartsWith(
                "scripted-candidate:teacher-authored-request:",
                evaluator.Evaluations[1].RuntimeFlowResult!.Output.CandidateId,
                StringComparison.Ordinal);
            Assert.StartsWith(
                "scripted-candidate:teacher-authored-request:",
                evaluator.Evaluations[2].RuntimeFlowResult!.Output.CandidateId,
                StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(
                [
                    "pass128-request",
                    "candidate-002-request",
                    "candidate-003-request"
                ],
                evaluator.Requests.Select(request => request.RequestId).ToArray());
            Assert.Equal(
                [
                    "scripted-candidate:pass128-request",
                    "scripted-candidate:candidate-002-request",
                    "scripted-candidate:candidate-003-request"
                ],
                evaluator.Evaluations
                    .Select(evaluation => evaluation.RuntimeFlowResult!.Output.CandidateId)
                    .ToArray());
        }

        Assert.Equal(0.70, evaluator.Evaluations[0].QaResults[0].Verdict.Score, 6);
        Assert.Equal(0.82, evaluator.Evaluations[1].QaResults[0].Verdict.Score, 6);
        Assert.Equal(0.94, evaluator.Evaluations[2].QaResults[0].Verdict.Score, 6);
        if (expectInitialSyntheticNegative)
            Assert.True(evaluator.Evaluations[0].ScaffoldMarkers.ContainsKey("synthetic_negative"));
        else
            Assert.Empty(evaluator.Evaluations[0].ScaffoldMarkers);
        Assert.Empty(evaluator.Evaluations[1].ScaffoldMarkers);
        Assert.Empty(evaluator.Evaluations[2].ScaffoldMarkers);
        Assert.NotEqual(
            evaluator.Evaluations[1].RuntimeFlowResult!.Output.OutputId,
            evaluator.Evaluations[2].RuntimeFlowResult!.Output.OutputId);
        Assert.NotEqual(
            evaluator.Evaluations[1].RuntimeFlowResult.Output.Verdict.VerdictId,
            evaluator.Evaluations[2].RuntimeFlowResult.Output.Verdict.VerdictId);
    }

    private static void AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidate(
        DesignConversionLocalArtifactDbRecord artifact,
        DpoFaultFixtureEvaluator evaluator,
        string artifactRoot,
        string? expectedScorerModelSha256 = null)
    {
        var candidates = AssertDpoFaultFixtureIndependentReceiptPairSet(
            artifact,
            evaluator,
            persisted: true,
            expectedScorerModelSha256: expectedScorerModelSha256);
        Assert.Equal(
            "4",
            artifact.Metadata[
                DesignConversionCandidatePayloadCustodyStore.EntryCountMetadataKey]);
        Assert.All(candidates, candidate =>
            Assert.Null(candidate.TrainingRecord.PayloadCustodySecret));
        var transactionId = artifact.Metadata[
            DesignConversionCandidatePayloadCustodyStore.TransactionMetadataKey];
        Assert.Equal(64, transactionId.Length);
        Assert.All(transactionId, character => Assert.True(
            character is >= '0' and <= '9' or >= 'a' and <= 'f'));
        AssertCanonicalSha256Ref(artifact.Metadata[
            DesignConversionCandidatePayloadCustodyStore.ManifestMetadataKey]);
        AssertCanonicalSha256Ref(artifact.Metadata[
            DesignConversionCandidatePayloadCustodyStore.BindingSetMetadataKey]);

        var custodyRoot = Path.Combine(
            artifactRoot,
            DesignConversionCandidatePayloadCustodyStore.DirectoryName,
            "committed");
        Assert.True(Directory.Exists(custodyRoot));
        var committedPath = Path.Combine(
            custodyRoot,
            transactionId + ".custody-committed.json");
        Assert.True(File.Exists(committedPath));
        Assert.False(
            File.GetAttributes(committedPath).HasFlag(FileAttributes.Directory));
    }

    private static void AssertDpoFaultFixtureCreatedIndependentEvaluatedDpoCandidateBeforePersistence(
        DesignConversionLocalArtifactDbRecord artifact,
        DpoFaultFixtureEvaluator evaluator)
    {
        var candidates = AssertDpoFaultFixtureIndependentReceiptPairSet(
            artifact,
            evaluator,
            persisted: false);
        Assert.All(candidates, candidate =>
            Assert.NotNull(candidate.TrainingRecord.PayloadCustodySecret));
        Assert.False(artifact.Metadata.ContainsKey(
            DesignConversionCandidatePayloadCustodyStore.TransactionMetadataKey));
    }

    private static DesignConversionLocalArtifactDbDpoCandidate[]
        AssertDpoFaultFixtureIndependentReceiptPairSet(
            DesignConversionLocalArtifactDbRecord artifact,
            DpoFaultFixtureEvaluator evaluator,
            bool persisted,
            string? expectedScorerModelSha256 = null)
    {
        AssertDpoFaultFixtureAttemptProgression(
            evaluator,
            expectInitialSyntheticNegative: false,
            expectTeacherAuthoredCandidates: true);
        Assert.Equal(bool.TrueString, artifact.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal("4", artifact.Metadata["runtime_created_dpo_candidate_count"]);
        Assert.False(artifact.Metadata.ContainsKey(
            DesignConversionImprovementLoopCoordinator
                .LocalArtifactDbDpoNoEligibleRejectedAttemptMarker));
        Assert.False(artifact.Metadata.ContainsKey(
            DesignConversionImprovementLoopCoordinator
                .LocalArtifactDbDpoSelectedRejectedAttemptMarker));

        var candidates = artifact.DpoCandidates
            .OrderBy(candidate => candidate.TrainingRecord.Chosen.Verdict.Score)
            .ToArray();
        Assert.Equal(4, candidates.Length);
        Assert.Equal(
            [0.82, 0.82, 0.94, 0.94],
            candidates.Select(candidate =>
                candidate.TrainingRecord.Chosen.Verdict.Score).ToArray());
        Assert.Equal(
            [0.70, 0.70, 0.82, 0.82],
            candidates.Select(candidate =>
                candidate.TrainingRecord.Rejected.Verdict.Score).ToArray());

        var teacherRequestIds = new HashSet<string>(StringComparer.Ordinal);
        var sourceRequestRefs = new HashSet<string>(StringComparer.Ordinal);
        var trainingContentSha256 = new HashSet<string>(StringComparer.Ordinal);
        var custodyBindings = new HashSet<string>(StringComparer.Ordinal);
        var custodyGovernance = new HashSet<string>(StringComparer.Ordinal);
        for (var candidateIndex = 0; candidateIndex < candidates.Length;
             candidateIndex++)
        {
            var transitionIndex = candidateIndex / 2;
            var unitOrdinal = candidateIndex % 2;
            var candidate = candidates[candidateIndex];
            var record = candidate.TrainingRecord;
            Assert.Equal(
                unitOrdinal == 0
                    ? DesignConversionDpoUnits.CriticUnit
                    : DesignConversionDpoUnits.ImproverUnit,
                record.UnitId);
            var rejectedRequest = evaluator.Requests[transitionIndex];
            var chosenRequest = evaluator.Requests[transitionIndex + 1];
            var rejectedEvaluation = evaluator.Evaluations[transitionIndex];
            var chosenEvaluation = evaluator.Evaluations[transitionIndex + 1];
            Assert.StartsWith(
                "teacher-authored-request:",
                chosenRequest.RequestId,
                StringComparison.Ordinal);
            Assert.NotEqual(rejectedRequest.RequestId, chosenRequest.RequestId);
            Assert.Equal(
                DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-source-request",
                    chosenRequest.RequestId),
                record.Provenance.SourceRequestId);
            Assert.Equal(
                unitOrdinal == 0,
                sourceRequestRefs.Add(record.Provenance.SourceRequestId));
            Assert.Equal(
                DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-chosen-output",
                    chosenEvaluation.RuntimeFlowResult!.Output.OutputId),
                record.Chosen.OutputId);
            Assert.Equal(
                DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-chosen-candidate",
                    chosenEvaluation.RuntimeFlowResult.Output.CandidateId),
                record.Chosen.CandidateId);
            Assert.Equal(
                DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-rejected-output",
                    rejectedEvaluation.RuntimeFlowResult!.Output.OutputId),
                record.Rejected.OutputId);
            Assert.Equal(
                DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-rejected-candidate",
                    rejectedEvaluation.RuntimeFlowResult.Output.CandidateId),
                record.Rejected.CandidateId);

            Assert.Equal(
                ExpectedIndependentPreferencePayloadRef(
                    "chosen_html",
                    "chosen-html",
                    chosenRequest.Html,
                    persisted),
                record.Chosen.Html);
            Assert.Equal(
                ExpectedIndependentPreferencePayloadRef(
                    "chosen_css",
                    "chosen-css",
                    chosenRequest.Css,
                    persisted),
                record.Chosen.Css);
            Assert.Equal(
                ExpectedIndependentPreferencePayloadRef(
                    "rejected_html",
                    "rejected-html",
                    rejectedRequest.Html,
                    persisted),
                record.Rejected.Html);
            Assert.Equal(
                ExpectedIndependentPreferencePayloadRef(
                    "rejected_css",
                    "rejected-css",
                    rejectedRequest.Css,
                    persisted),
                record.Rejected.Css);

            var provenanceMetadata = record.Provenance.Metadata;
            var expectedIncumbentRef = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-dpo-teacher-incumbent",
                DesignConversionDpoTeacherAttestationRuntime
                    .BuildIncumbentBinding(rejectedRequest));
            var generatedIdentity = chosenRequest.RequestId[
                "teacher-authored-request:".Length..];
            var expectedGeneratedRef = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-dpo-teacher-generated",
                generatedIdentity);
            Assert.Equal(
                expectedIncumbentRef,
                provenanceMetadata["teacher_authoring_incumbent_ref"]);
            Assert.Equal(
                expectedGeneratedRef,
                provenanceMetadata["teacher_authoring_generated_ref"]);
            Assert.Equal(
                bool.TrueString,
                provenanceMetadata["runtime_teacher_authored_candidate"]);
            Assert.Equal(bool.TrueString,
                provenanceMetadata["candidate_authoring_only"]);
            Assert.False(provenanceMetadata.ContainsKey(
                "runtime_teacher_selected_preference"));
            Assert.Equal(bool.TrueString,
                provenanceMetadata["independent_preference_proven"]);
            Assert.Equal(bool.TrueString,
                provenanceMetadata["runtime_rejected_independently_evaluated"]);
            Assert.Equal(bool.TrueString,
                provenanceMetadata["high_quality_negative_pair_proven"]);
            Assert.Equal(bool.FalseString,
                provenanceMetadata["training_grade"]);
            Assert.Equal(
                DesignConversionIndependentPreferenceEvidenceContract
                    .LocalScorerProvenanceKind,
                provenanceMetadata["directionality_source"]);
            Assert.Equal(
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileId,
                provenanceMetadata["independent_scorer_profile_id"]);
            Assert.Equal(
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileVersion,
                provenanceMetadata["independent_scorer_profile_version"]);
            Assert.Equal(
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileConfigurationSha256,
                provenanceMetadata[
                    "independent_scorer_profile_configuration_sha256"]);
            Assert.Equal(
                expectedScorerModelSha256 ??
                "sha256:" + new string('d', 64),
                provenanceMetadata["independent_scorer_model_sha256"]);
            AssertCanonicalSha256Ref(provenanceMetadata[
                "independent_preference_receipt_sha256"]);
            AssertCanonicalSha256Ref(provenanceMetadata[
                "independent_scorer_request_sha256"]);
            AssertCanonicalSha256Ref(provenanceMetadata[
                "independent_scorer_output_sha256"]);
            AssertCanonicalSha256Ref(provenanceMetadata[
                "independent_preference_generated_evaluation_identity"]);
            AssertCanonicalSha256Ref(provenanceMetadata[
                "independent_preference_incumbent_evaluation_identity"]);
            Assert.NotEqual(
                provenanceMetadata[
                    "independent_preference_generated_evaluation_identity"],
                provenanceMetadata[
                    "independent_preference_incumbent_evaluation_identity"]);
            Assert.Equal(
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                provenanceMetadata["teacher_authoring_schema"]);
            Assert.Equal(
                LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
                provenanceMetadata["teacher_inference_profile_id"]);
            Assert.Equal(
                LocalQwenOllamaInferenceProfileAuthority.Authoring.ConfigurationSha256,
                provenanceMetadata[
                    "teacher_inference_profile_configuration_sha256"]);
            Assert.Equal(
                bool.FalseString,
                provenanceMetadata["raw_teacher_prompt_persisted"]);
            Assert.Equal(
                bool.FalseString,
                provenanceMetadata["raw_teacher_output_persisted"]);
            Assert.Equal(
                bool.FalseString,
                provenanceMetadata["raw_teacher_rationale_persisted"]);

            var teacher = Assert.IsType<TrainingTeacherProvenance>(
                record.TeacherProvenance);
            Assert.Equal(
                (
                    TrainingTeacherSourceKind.ExternalTeacherOutput,
                    OpenWeightTeacherFamily.Qwen,
                    TrainingTeacherAccessMode.LocalHostedOpenWeight,
                    TrainingTeacherHostingProvider.Ollama,
                    "qwen-test:9b",
                    new string('c', 64),
                    TrainingTeacherModelLicense.Apache20),
                (
                    teacher.SourceKind,
                    teacher.Family,
                    teacher.AccessMode,
                    teacher.HostingProvider,
                    teacher.ModelId,
                    teacher.ModelVersion,
                    teacher.ModelLicense));
            Assert.False(teacher.ProviderTermsChecked);
            Assert.False(teacher.ProviderTermsAllowTraining);
            Assert.False(teacher.ClosedProprietaryModel);
            Assert.True(teacher.TeacherOutputUsedForTraining);
            Assert.Equal(
                "teacher-test-authoring-" +
                (transitionIndex + 1).ToString(CultureInfo.InvariantCulture),
                teacher.RequestId);
            Assert.Equal(
                unitOrdinal == 0,
                teacherRequestIds.Add(teacher.RequestId));
            Assert.Equal(1, teacher.InvocationCount);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
                teacher.ModelArtifactDigestKind);
            Assert.Equal(new string('d', 64), teacher.ModelArtifactDigest);
            Assert.Equal(64, teacher.TrainingContentSha256.Length);
            Assert.All(teacher.TrainingContentSha256, character => Assert.True(
                character is >= '0' and <= '9' or >= 'a' and <= 'f'));
            Assert.True(trainingContentSha256.Add(
                teacher.TrainingContentSha256));

            var custodyRequired = provenanceMetadata[
                DesignConversionDpoGenerator.PayloadCustodyRequiredMetadataKey];
            Assert.Equal(bool.TrueString, custodyRequired);
            Assert.Equal(
                DesignConversionDpoGenerator.PayloadCustodyVersion,
                provenanceMetadata[
                    DesignConversionDpoGenerator.PayloadCustodyVersionMetadataKey]);
            var promptSha256 = provenanceMetadata[
                DesignConversionDpoGenerator.PayloadCustodyPromptSha256MetadataKey];
            var chosenSha256 = provenanceMetadata[
                DesignConversionDpoGenerator.PayloadCustodyChosenSha256MetadataKey];
            var rejectedSha256 = provenanceMetadata[
                DesignConversionDpoGenerator.PayloadCustodyRejectedSha256MetadataKey];
            var governanceSha256 = provenanceMetadata[
                DesignConversionDpoGenerator.PayloadCustodyGovernanceSha256MetadataKey];
            var bindingSha256 = provenanceMetadata[
                DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey];
            AssertCanonicalSha256Ref(promptSha256);
            AssertCanonicalSha256Ref(chosenSha256);
            AssertCanonicalSha256Ref(rejectedSha256);
            AssertCanonicalSha256Ref(governanceSha256);
            AssertCanonicalSha256Ref(bindingSha256);
            Assert.Equal(
                DesignConversionDpoGenerator.BuildPayloadCustodyGovernanceSha256(
                    record),
                governanceSha256);
            Assert.Equal(
                DesignConversionDpoGenerator.BuildPayloadCustodyBinding(
                    record,
                    promptSha256,
                    chosenSha256,
                    rejectedSha256,
                    governanceSha256),
                bindingSha256);
            Assert.True(custodyGovernance.Add(governanceSha256));
            Assert.True(custodyBindings.Add(bindingSha256));
            if (persisted)
                Assert.Null(record.PayloadCustodySecret);
        }

        Assert.Equal(2, teacherRequestIds.Count);
        Assert.Equal(2, sourceRequestRefs.Count);
        Assert.Equal(4, trainingContentSha256.Count);
        Assert.Equal(4, custodyBindings.Count);
        Assert.Equal(4, custodyGovernance.Count);
        return candidates;
    }

    private static string ExpectedIndependentPreferencePayloadRef(
        string persistedScope,
        string teacherScope,
        string payload,
        bool persisted)
    {
        var scorerRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-runtime-independent-scorer-" + teacherScope,
            payload);
        return persisted
            ? DesignConversionDpoLeakageGuard.HashRef(
                "dcm-" + persistedScope,
                scorerRef)
            : scorerRef;
    }

    private static void AssertCanonicalSha256Ref(string value)
    {
        Assert.StartsWith("sha256:", value, StringComparison.Ordinal);
        Assert.Equal(71, value.Length);
        Assert.All(value["sha256:".Length..], character => Assert.True(
            character is >= '0' and <= '9' or >= 'a' and <= 'f'));
    }

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

    private static string ResolveScriptedDefectCategory(DesignConversionInternalEvaluateRequest request)
        => request.Provenance.TryGetValue("scripted_defect_category", out var category) &&
           !string.IsNullOrWhiteSpace(category)
            ? category
            : "quality";

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
                            Category: ResolveScriptedDefectCategory(request),
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
            Provenance: new Dictionary<string, string>(request.Provenance)
            {
                ["internal_service_job_id"] = request.JobId
            });

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

    private sealed class SameRawJobIdLoopEvaluator(string rawJobId) : IDesignConversionThinClientJobEvaluator
    {
        private int _attemptNumber;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            var score = ++_attemptNumber switch
            {
                1 => 0.70,
                2 => 0.82,
                _ => 0.94
            };
            var evaluation = BuildScriptedEvaluation(
                request,
                _attemptNumber,
                [score, score],
                costPerViewport: 0m);
            var submitted = evaluation.SubmittedRequest with
            {
                Provenance = new Dictionary<string, string>(request.Provenance)
                {
                    ["internal_service_job_id"] = rawJobId
                }
            };
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                evaluation with { JobId = rawJobId, SubmittedRequest = submitted }));
        }
    }

    private sealed class CountingLocalIndependentEvaluationClient(
        string? failureCode = null)
        : ILegalTeacherEvaluationInferenceClient
    {
        internal const string ModelId = "qwen-coordinator-evaluator-test:9b";
        internal static readonly string ManifestDigest = new('c', 64);
        internal static readonly string ModelLayerDigest = new('d', 64);
        private int _callCount;

        internal int CallCount => Volatile.Read(ref _callCount);

        internal bool Disposed { get; private set; }

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var permit = callPermit.TryIssue();
            if (!permit.IsSuccess)
            {
                return Task.FromResult(
                    permit.MapError<TeacherEvaluationInferenceResult>());
            }

            var call = Interlocked.Increment(ref _callCount);
            if (!string.IsNullOrWhiteSpace(failureCode))
            {
                return Task.FromResult(
                    OperationResult<TeacherEvaluationInferenceResult>.Fail(
                        "The local evaluation response is unavailable.",
                        failureCode));
            }

            using var document = JsonDocument.Parse(prompt);
            var root = document.RootElement;
            var output = JsonSerializer.Serialize(new
            {
                schema = DesignConversionLocalIndependentPreferenceEvaluator
                    .ResultSchema,
                scorer_request_sha256 = root
                    .GetProperty("scorer_request_sha256").GetString(),
                generated_evaluation_identity = root
                    .GetProperty("generated_evaluation_identity").GetString(),
                incumbent_evaluation_identity = root
                    .GetProperty("incumbent_evaluation_identity").GetString(),
                preferred_direction =
                    DesignConversionIndependentPreferenceEvidenceContract
                        .GeneratedSide
            });
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                ModelId,
                ManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: false,
                RequestId: "coordinator-evaluation-request-" + call,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = ModelLayerDigest
            };
            return Task.FromResult(
                OperationResult<TeacherEvaluationInferenceResult>.Ok(
                    new TeacherEvaluationInferenceResult(output, provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority.Evaluation
                    }));
        }

        public void Dispose() => Disposed = true;
    }

    private sealed class DpoFaultFixtureEvaluator(
        bool collideFinalRuntimeIdentity = false,
        bool markInitialSyntheticNegative = true)
        : IDesignConversionThinClientJobEvaluator
    {
        private DesignConversionImprovementOutput? _candidateTwoOutput;

        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public List<DesignConversionInternalEvaluationResult> Evaluations { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var attemptNumber = Requests.Count;
            var score = attemptNumber switch
            {
                1 => 0.70,
                2 => 0.82,
                _ => 0.94
            };
            var evaluation = BuildScriptedEvaluation(
                request,
                attemptNumber,
                [score, score],
                costPerViewport: 0m);
            if (attemptNumber == 1 && markInitialSyntheticNegative)
            {
                evaluation = evaluation with
                {
                    ScaffoldMarkers = new Dictionary<string, string>
                    {
                        ["synthetic_negative"] = bool.TrueString
                    }
                };
            }
            else if (attemptNumber == 2)
            {
                _candidateTwoOutput = evaluation.RuntimeFlowResult!.Output;
            }
            else if (collideFinalRuntimeIdentity)
            {
                evaluation = evaluation with
                {
                    RuntimeFlowResult = evaluation.RuntimeFlowResult! with
                    {
                        Output = _candidateTwoOutput!
                    }
                };
            }

            Evaluations.Add(evaluation);
            return Task.FromResult(
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                    evaluation));
        }
    }

    private sealed class PairCapturingDpoFaultFixtureEvaluator(
        DpoFaultFixtureEvaluator inner) :
        IDesignConversionThinClientJobEvaluator,
        IDesignConversionDpoPolicyPairRuntimeEvaluator
    {
        public List<DesignConversionDpoPolicyRuntimePair> RuntimePairs { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            EvaluateAsync(
                DesignConversionInternalEvaluateRequest request,
                CancellationToken cancellationToken = default)
            => inner.EvaluateAsync(request, cancellationToken);

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            ApplyDpoPolicyRuntimePairAsync(
                DesignConversionInternalEvaluationResult evaluation,
                DesignConversionDpoPolicyRuntimePair pair,
                CancellationToken cancellationToken = default)
        {
            if (!DesignConversionDpoPolicyRuntimePairFactory
                    .TryValidateForRuntime(
                        pair,
                        evaluation.SubmittedRequest,
                        pair.ObservedAtUtc,
                        out var reason))
            {
                return Task.FromResult(
                    OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                        "The coordinator runtime pair was invalid.",
                        reason));
            }

            RuntimePairs.Add(pair);
            return Task.FromResult(
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                    evaluation));
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

    private sealed class TestResultsDirectoryScope : IDisposable
    {
        private static readonly string WorkspaceTestResultsRoot =
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(
                Path.Combine(DiscoverRepoRoot(), "TestResults")));

        private TestResultsDirectoryScope(string directoryPath)
        {
            DirectoryPath = RequireSafeChild(directoryPath);
            Directory.CreateDirectory(DirectoryPath);
        }

        public string DirectoryPath { get; }

        public static TestResultsDirectoryScope Create(params string[] segments)
        {
            if (segments.Length == 0)
            {
                throw new InvalidOperationException(
                    "A test results cleanup scope requires at least one segment.");
            }

            var parts = new string[segments.Length + 1];
            parts[0] = WorkspaceTestResultsRoot;
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (string.IsNullOrWhiteSpace(segment) || Path.IsPathRooted(segment))
                {
                    throw new InvalidOperationException(
                        $"Unsafe test results cleanup segment: {segment}");
                }

                parts[i + 1] = segment;
            }

            return new TestResultsDirectoryScope(Path.Combine(parts));
        }

        public void Dispose()
        {
            var safeDirectoryPath = RequireSafeChild(DirectoryPath);
            if (Directory.Exists(safeDirectoryPath))
            {
                Directory.Delete(safeDirectoryPath, recursive: true);
            }
        }

        private static string RequireSafeChild(string directoryPath)
        {
            var fullPath = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(directoryPath));
            var requiredPrefix =
                WorkspaceTestResultsRoot + Path.DirectorySeparatorChar;
            if (string.Equals(
                    fullPath,
                    WorkspaceTestResultsRoot,
                    StringComparison.OrdinalIgnoreCase)
                || !fullPath.StartsWith(
                    requiredPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Refusing to clean a path outside {WorkspaceTestResultsRoot}: {fullPath}");
            }

            return fullPath;
        }

        private static string DiscoverRepoRoot()
        {
            var searchedStarts = new[]
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory()
            };

            foreach (var start in searchedStarts)
            {
                var root = TryDiscoverRepoRoot(start);
                if (root is not null)
                {
                    return root;
                }
            }

            throw new InvalidOperationException(
                "Unable to discover llm_mvp_core repository root from " +
                $"{nameof(AppContext.BaseDirectory)}='{AppContext.BaseDirectory}' " +
                $"or current directory='{Directory.GetCurrentDirectory()}'. " +
                "Expected an ancestor containing both LlmFromScratch.sln and the Tests directory.");
        }

        private static string? TryDiscoverRepoRoot(string startPath)
        {
            if (string.IsNullOrWhiteSpace(startPath))
            {
                return null;
            }

            var fullStartPath = Path.GetFullPath(startPath);
            var directory = Directory.Exists(fullStartPath)
                ? new DirectoryInfo(fullStartPath)
                : Directory.GetParent(fullStartPath);

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "Tests")))
                {
                    return Path.TrimEndingDirectorySeparator(
                        Path.GetFullPath(directory.FullName));
                }

                directory = directory.Parent;
            }

            return null;
        }
    }

    private sealed class FailingPostEvaluationStateStore
        : IDesignConversionPostEvaluationStateStore
    {
        public int WriteCalls { get; private set; }

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            WriteAsync(
                DesignConversionPostEvaluationState state,
                CancellationToken cancellationToken = default)
        {
            WriteCalls++;
            return Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState>.Fail(
                    "Test post-evaluation state write failed.",
                    "TEST_POST_EVALUATION_STATE_WRITE_FAILED"));
        }

        public Task<OperationResult<DesignConversionPostEvaluationState?>>
            ReadAsync(
                string stateRef,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState?>.Ok(
                    null));

        public Task<OperationResult<
            IReadOnlyList<DesignConversionPostEvaluationState>>>
            ListAsync(
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<
                    IReadOnlyList<DesignConversionPostEvaluationState>>.Ok(
                    Array.Empty<DesignConversionPostEvaluationState>()));
    }

    private sealed class ScriptedPostEvaluationStateStore(
        bool failCommit,
        bool failQuarantine,
        bool failDeny = false)
        : IDesignConversionPostEvaluationStateStore
    {
        private DesignConversionPostEvaluationState? _state;
        private bool _trainingEligibilityDenied;

        public int QuarantineWriteCalls { get; private set; }

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            DenyTrainingEligibilityAsync(
                string stateRef,
                string expectedArtifactRecordId,
                string errorCode,
                CancellationToken cancellationToken = default)
        {
            if (failDeny)
            {
                return Task.FromResult(
                    OperationResult<
                        DesignConversionPostEvaluationState>.Fail(
                        "Test denial creation failed.",
                        "TEST_DENIAL_CREATE_FAILED"));
            }

            _trainingEligibilityDenied = true;
            return Task.FromResult(
                _state is null
                    ? OperationResult<
                        DesignConversionPostEvaluationState>.Fail(
                        "Test denial has no state.",
                        "TEST_DENIAL_STATE_MISSING")
                    : OperationResult<
                        DesignConversionPostEvaluationState>.Ok(_state));
        }

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            ReleaseTrainingEligibilityDenialAsync(
                string stateRef,
                string expectedArtifactRecordId,
                CancellationToken cancellationToken = default)
        {
            _trainingEligibilityDenied = false;
            return Task.FromResult(
                _state is null
                    ? OperationResult<
                        DesignConversionPostEvaluationState>.Fail(
                        "Test denial release has no state.",
                        "TEST_DENIAL_RELEASE_STATE_MISSING")
                    : OperationResult<
                        DesignConversionPostEvaluationState>.Ok(_state));
        }

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            WriteAsync(
                DesignConversionPostEvaluationState state,
                CancellationToken cancellationToken = default)
        {
            if (string.Equals(
                    state.PersistenceStatus,
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceCommitted,
                    StringComparison.Ordinal) &&
                failCommit)
            {
                return Task.FromResult(
                    OperationResult<
                        DesignConversionPostEvaluationState>.Fail(
                        "Test commit failed.",
                        "TEST_COMMIT_FAILED"));
            }

            if (string.Equals(
                    state.PersistenceStatus,
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceQuarantined,
                    StringComparison.Ordinal))
            {
                QuarantineWriteCalls++;
                if (failQuarantine)
                {
                    return Task.FromResult(
                        OperationResult<
                            DesignConversionPostEvaluationState>.Fail(
                            "Test quarantine failed.",
                            "TEST_QUARANTINE_FAILED"));
                }
            }

            _state = state with
            {
                Version = _state is null
                    ? 1
                    : checked(_state.Version + 1)
            };
            return Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState>.Ok(
                    _state));
        }

        public Task<OperationResult<DesignConversionPostEvaluationState?>>
            ReadAsync(
                string stateRef,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DesignConversionPostEvaluationState?>.Ok(
                    _state));

        public Task<OperationResult<
            IReadOnlyList<DesignConversionPostEvaluationState>>>
            ListAsync(
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<
                    IReadOnlyList<DesignConversionPostEvaluationState>>.Ok(
                    _state is null
                        ? []
                        : [_state with
                        {
                            TrainingEligibilityDenied =
                                _trainingEligibilityDenied
                        }]));
    }

    private sealed class PostPublicationCommitFailureStateStore
        : IDesignConversionPostEvaluationStateStore
    {
        private readonly JsonDesignConversionPostEvaluationStateStore _normal;
        private readonly JsonDesignConversionPostEvaluationStateStore
            _commitFault;
        private int _writeCalls;
        private int _injectedFailures;

        public PostPublicationCommitFailureStateStore(string root)
        {
            _normal =
                new JsonDesignConversionPostEvaluationStateStore(root);
            var options =
                DesignConversionPostEvaluationStateStoreOptions.Default with
                {
                    BeforeHandleIoForTesting = (stage, _) =>
                    {
                        if (stage ==
                                DesignConversionHandleBoundIoStage
                                    .AfterBackupDeleteBeforeDirectoryFlush &&
                            Interlocked.Exchange(
                                ref _injectedFailures,
                                1) == 0)
                        {
                            throw new IOException(
                                "injected-coordinator-post-publication-cleanup-failure");
                        }
                    }
                };
            _commitFault =
                new JsonDesignConversionPostEvaluationStateStore(
                    root,
                    options);
        }

        public int InjectedFailures =>
            Volatile.Read(ref _injectedFailures);

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            WriteAsync(
                DesignConversionPostEvaluationState state,
                CancellationToken cancellationToken = default)
            => Interlocked.Increment(ref _writeCalls) == 1
                ? _normal.WriteAsync(state, cancellationToken)
                : _commitFault.WriteAsync(state, cancellationToken);

        public Task<OperationResult<DesignConversionPostEvaluationState?>>
            ReadAsync(
                string stateRef,
                CancellationToken cancellationToken = default)
            => _normal.ReadAsync(stateRef, cancellationToken);

        public Task<OperationResult<
            IReadOnlyList<DesignConversionPostEvaluationState>>>
            ListAsync(
                CancellationToken cancellationToken = default)
            => _normal.ListAsync(cancellationToken);

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            DenyTrainingEligibilityAsync(
                string stateRef,
                string expectedArtifactRecordId,
                string errorCode,
                CancellationToken cancellationToken = default)
            => _normal.DenyTrainingEligibilityAsync(
                stateRef,
                expectedArtifactRecordId,
                errorCode,
                cancellationToken);

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            ReleaseTrainingEligibilityDenialAsync(
                string stateRef,
                string expectedArtifactRecordId,
                CancellationToken cancellationToken = default)
            => _normal.ReleaseTrainingEligibilityDenialAsync(
                stateRef,
                expectedArtifactRecordId,
                cancellationToken);
    }

    private sealed class ReleaseFailurePostEvaluationStateStore
        : IDesignConversionPostEvaluationStateStore
    {
        private readonly JsonDesignConversionPostEvaluationStateStore _normal;
        private readonly JsonDesignConversionPostEvaluationStateStore
            _releaseFault;
        private int _injectedFailures;

        public ReleaseFailurePostEvaluationStateStore(
            string root,
            string failureStage)
        {
            _normal =
                new JsonDesignConversionPostEvaluationStateStore(root);
            var options =
                DesignConversionPostEvaluationStateStoreOptions.Default with
                {
                    BeforeTrainingEligibilityDenialDeleteForTesting =
                        failureStage == "before-delete"
                            ? _ =>
                            {
                                Interlocked.Increment(
                                    ref _injectedFailures);
                                throw new IOException(
                                    "injected-release-before-delete");
                            }
                            : null,
                    AfterTrainingEligibilityDenialDeleteForTesting =
                        failureStage == "after-delete"
                            ? _ =>
                            {
                                Interlocked.Increment(
                                    ref _injectedFailures);
                                throw new IOException(
                                    "injected-release-after-delete");
                            }
                            : null
                };
            _releaseFault =
                new JsonDesignConversionPostEvaluationStateStore(
                    root,
                    options);
        }

        public int InjectedFailures =>
            Volatile.Read(ref _injectedFailures);

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            WriteAsync(
                DesignConversionPostEvaluationState state,
                CancellationToken cancellationToken = default)
            => _normal.WriteAsync(state, cancellationToken);

        public Task<OperationResult<DesignConversionPostEvaluationState?>>
            ReadAsync(
                string stateRef,
                CancellationToken cancellationToken = default)
            => _normal.ReadAsync(stateRef, cancellationToken);

        public Task<OperationResult<
            IReadOnlyList<DesignConversionPostEvaluationState>>>
            ListAsync(
                CancellationToken cancellationToken = default)
            => _normal.ListAsync(cancellationToken);

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            DenyTrainingEligibilityAsync(
                string stateRef,
                string expectedArtifactRecordId,
                string errorCode,
                CancellationToken cancellationToken = default)
            => _normal.DenyTrainingEligibilityAsync(
                stateRef,
                expectedArtifactRecordId,
                errorCode,
                cancellationToken);

        public Task<OperationResult<DesignConversionPostEvaluationState>>
            ReleaseTrainingEligibilityDenialAsync(
                string stateRef,
                string expectedArtifactRecordId,
                CancellationToken cancellationToken = default)
            => _releaseFault.ReleaseTrainingEligibilityDenialAsync(
                stateRef,
                expectedArtifactRecordId,
                cancellationToken);
    }

    private sealed class FaultingLocalArtifactDb(
        IDesignConversionLocalArtifactDb inner,
        bool failAppend,
        bool failList,
        string? trainingIntakeMetadataKeyToAlter = null,
        bool removeTrainingIntakeMetadata = false,
        bool alterAttemptHistoryOnList = false)
        : IDesignConversionLocalArtifactDb
    {
        public List<DesignConversionLocalArtifactDbRecord> AttemptedRecords { get; } = [];
        public int ExportApprovedRuntimeDpoCandidatesCalls { get; private set; }
        public int CreateTrainingIntakeCalls { get; private set; }
        public int RunTrainingIntakePreflightCalls { get; private set; }
        public int RunTrainingIntakeCalls { get; private set; }
        public int RunTrainingIntakeAndPromoteLocalCalls { get; private set; }

        public Task<OperationResult<DesignConversionLocalArtifactDbRecord>>
            AppendRecordAsync(
                DesignConversionLocalArtifactDbRecord record,
                CancellationToken ct = default)
        {
            AttemptedRecords.Add(record);
            return failAppend
                ? Task.FromResult(
                    OperationResult<
                        DesignConversionLocalArtifactDbRecord>.Fail(
                        "Test append failed.",
                        "TEST_APPEND_FAILED"))
                : inner.AppendRecordAsync(record, ct);
        }

        public async Task<OperationResult<
            IReadOnlyList<DesignConversionLocalArtifactDbRecord>>>
            ListRecordsAsync(
                DesignConversionLocalArtifactDbQuery? query = null,
                CancellationToken ct = default)
        {
            if (failList)
            {
                return OperationResult<
                    IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Fail(
                    "Test readback failed.",
                    "TEST_READBACK_FAILED");
            }

            var listed = await inner.ListRecordsAsync(query, ct);
            if (!alterAttemptHistoryOnList ||
                !listed.IsSuccess ||
                listed.Value is null)
            {
                return listed;
            }

            var alteredRecords = listed.Value
                .Select(AlterAttemptHistory)
                .ToArray();
            return OperationResult<
                IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(
                alteredRecords);
        }

        private static DesignConversionLocalArtifactDbRecord AlterAttemptHistory(
            DesignConversionLocalArtifactDbRecord record)
        {
            if (!record.TryGetImprovementLoopAttemptHistory(out var history) ||
                history is null)
            {
                return record;
            }

            var attempts = history.Attempts
                .Select((attempt, index) =>
                {
                    var candidateRef = index == 0
                        ? attempt.CandidateRef + ":changed-readback"
                        : attempt.CandidateRef;
                    var metadata = new Dictionary<string, string>(
                        attempt.PublicMetadata,
                        StringComparer.Ordinal)
                    {
                        ["loop_candidate_ref"] = candidateRef
                    };
                    return new DesignConversionImprovementLoopAttemptTelemetry(
                        AttemptNumber: attempt.AttemptNumber,
                        CandidateRef: candidateRef,
                        JobRef: attempt.JobRef,
                        RequestRef: attempt.RequestRef,
                        StopReason: attempt.StopReason,
                        ShouldContinue: attempt.ShouldContinue,
                        BestScore: attempt.BestScore,
                        RequiredScore: attempt.RequiredScore,
                        EstimatedCostUsd: attempt.EstimatedCostUsd,
                        BudgetUsd: attempt.BudgetUsd,
                        PolicyAttemptCount: attempt.PolicyAttemptCount,
                        MaxImprovementCandidates:
                            attempt.MaxImprovementCandidates,
                        RenderResultRefs: attempt.RenderResultRefs,
                        ComparisonArtifactRefs:
                            attempt.ComparisonArtifactRefs,
                        VisualJudgeResultRefs:
                            attempt.VisualJudgeResultRefs,
                        EvidenceRefs: attempt.EvidenceRefs,
                        PublicMetadata: metadata)
                    {
                        VisualCostMeasurementComplete =
                            attempt.VisualCostMeasurementComplete,
                        VisualCostMissingMeasurementCount =
                            attempt.VisualCostMissingMeasurementCount,
                        VisualCostSource = attempt.VisualCostSource
                    };
                })
                .ToArray();
            Assert.True(
                DesignConversionLocalArtifactDbRecord
                    .TryBuildImprovementLoopAttemptHistory(
                        history.LoopRef,
                        attempts,
                        out _,
                        out var alteredJson));
            Assert.NotNull(alteredJson);
            var metadata = new Dictionary<string, string>(
                record.Metadata,
                StringComparer.Ordinal)
            {
                [DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey] = alteredJson!
            };
            return record with
            {
                Metadata = metadata
            };
        }

        public Task<DesignConversionPostEvaluationTrainingEligibility>
            EvaluatePostEvaluationTrainingEligibilityAsync(
                DesignConversionLocalArtifactDbRecord record,
                CancellationToken ct = default)
            => inner.EvaluatePostEvaluationTrainingEligibilityAsync(record, ct);

        public Task<OperationResult<
            IReadOnlyList<DesignConversionDpoTrainingRecord>>>
            QueryApprovedTrainingCandidatesAsync(
                DesignConversionLocalArtifactDbQuery? query = null,
                CancellationToken ct = default)
            => inner.QueryApprovedTrainingCandidatesAsync(query, ct);

        public Task<OperationResult<
            DesignConversionApprovedDpoHandoffArtifact>>
            ExportApprovedRuntimeDpoCandidatesAsync(
                DesignConversionLocalArtifactDbQuery? query = null,
                DateTimeOffset? exportedAtUtc = null,
                CancellationToken ct = default)
        {
            ExportApprovedRuntimeDpoCandidatesCalls++;
            return inner.ExportApprovedRuntimeDpoCandidatesAsync(
                query,
                exportedAtUtc,
                ct);
        }

        public Task<OperationResult<DesignConversionTrainingIntakeRecord>>
            CreateTrainingIntakeAsync(
                DesignConversionApprovedDpoHandoffArtifact handoff,
                string requestedByRef,
                string reason,
                DateTimeOffset? requestedAtUtc = null,
                CancellationToken ct = default,
                IReadOnlyDictionary<string, string>?
                    admissionBoundaryMetadata = null)
        {
            CreateTrainingIntakeCalls++;
            IReadOnlyDictionary<string, string>? effectiveMetadata =
                admissionBoundaryMetadata;
            if (!string.IsNullOrWhiteSpace(trainingIntakeMetadataKeyToAlter))
            {
                var alteredMetadata = new Dictionary<string, string>(
                    admissionBoundaryMetadata ??
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    StringComparer.Ordinal);
                if (removeTrainingIntakeMetadata)
                {
                    alteredMetadata.Remove(trainingIntakeMetadataKeyToAlter);
                }
                else
                {
                    alteredMetadata[trainingIntakeMetadataKeyToAlter] =
                        $"mutated:{trainingIntakeMetadataKeyToAlter}";
                }

                effectiveMetadata = alteredMetadata;
            }

            return inner.CreateTrainingIntakeAsync(
                handoff,
                requestedByRef,
                reason,
                requestedAtUtc,
                ct,
                effectiveMetadata);
        }

        public Task<OperationResult<
            IReadOnlyList<DesignConversionTrainingIntakeRecord>>>
            ListTrainingIntakeRequestsAsync(
                CancellationToken ct = default)
            => inner.ListTrainingIntakeRequestsAsync(ct);

        public Task<OperationResult<
            DesignConversionTrainingIntakePreflightResult>>
            RunTrainingIntakePreflightAsync(
                string intakeId,
                CancellationToken ct = default)
        {
            RunTrainingIntakePreflightCalls++;
            return inner.RunTrainingIntakePreflightAsync(intakeId, ct);
        }

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>>
            RunTrainingIntakeAsync(
                string intakeId,
                DesignConversionTrainingPipelineOptions? options = null,
                CancellationToken ct = default,
                IReadOnlyDictionary<string, string>?
                    admissionBoundaryMetadata = null)
        {
            RunTrainingIntakeCalls++;
            return inner.RunTrainingIntakeAsync(
                intakeId,
                options,
                ct,
                admissionBoundaryMetadata);
        }

        public Task<OperationResult<
            DesignConversionTrainingIntakeLocalPromotionResult>>
            RunTrainingIntakeAndPromoteLocalAsync(
                string intakeId,
                string promotionOutputDirectory,
                DesignConversionTrainingPipelineOptions? options = null,
                CancellationToken ct = default)
        {
            RunTrainingIntakeAndPromoteLocalCalls++;
            return inner.RunTrainingIntakeAndPromoteLocalAsync(
                intakeId,
                promotionOutputDirectory,
                options,
                ct);
        }

        public Task<OperationResult<
            DesignConversionTrainingIntakeLocalPromotionRollbackResult>>
            RollbackTrainingIntakeLocalPromotionAsync(
                string promotedLocalModelRef,
                string requestedByRef,
                string reason,
                DateTimeOffset? requestedAtUtc = null,
                CancellationToken ct = default)
            => inner.RollbackTrainingIntakeLocalPromotionAsync(
                promotedLocalModelRef,
                requestedByRef,
                reason,
                requestedAtUtc,
                ct);

        public Task<OperationResult<
            DesignConversionDpoCandidateApprovalResult>>
            ApproveRuntimeCreatedDpoCandidateAsync(
                string localArtifactRecordId,
                string candidateOrTrainingRecordId,
                string reviewerRef,
                string reason,
                DateTimeOffset? approvedAtUtc = null,
                CancellationToken ct = default)
            => inner.ApproveRuntimeCreatedDpoCandidateAsync(
                localArtifactRecordId,
                candidateOrTrainingRecordId,
                reviewerRef,
                reason,
                approvedAtUtc,
                ct);

        public Task<OperationResult<DesignConversionDpoDisplayResult>>
            QueryDisplayRowsAsync(
                DesignConversionDpoDisplayQuery? query = null,
                CancellationToken ct = default)
            => inner.QueryDisplayRowsAsync(query, ct);
    }

    private sealed class RecordingTrainingIntakePreflightGateway(
        Func<DesignConversionTrainingIntakePreflightHandoffRequest,
            OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>> handler,
        Func<DesignConversionTrainingIntakeRunAndPromoteLocalRequest,
            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>>?
            runAndPromoteHandler = null)
        : IDesignConversionTrainingIntakePreflightHandoffGateway
    {
        public List<DesignConversionTrainingIntakePreflightHandoffRequest>
            Requests { get; } = [];

        public int CallCount => Requests.Count;

        public int RunTrainingIntakeAndPromoteLocalCalls =>
            RunAndPromoteRequests.Count;

        public List<DesignConversionTrainingIntakeRunAndPromoteLocalRequest>
            RunAndPromoteRequests { get; } = [];

        public Task<OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>> SubmitPreflightAsync(
            DesignConversionTrainingIntakePreflightHandoffRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(handler(request));
        }

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            DesignConversionTrainingIntakeRunAndPromoteLocalRequest request,
            CancellationToken cancellationToken = default)
        {
            RunAndPromoteRequests.Add(request);
            return Task.FromResult(
                runAndPromoteHandler?.Invoke(request) ??
                OperationResult<
                    DesignConversionTrainingIntakeLocalPromotionResult>.Fail(
                    "Atomic run-and-promote handler was not configured.",
                    "TEST_ATOMIC_RUN_AND_PROMOTE_HANDLER_NOT_CONFIGURED"));
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
