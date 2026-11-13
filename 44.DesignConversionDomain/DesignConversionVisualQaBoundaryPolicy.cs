// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;

public static class DesignConversionVisualQaBoundaryPolicy
{
    public const string MultiResolutionRenderMarker = "multi_resolution_render";
    public const string VisualJudgeProviderBoundaryMarker = "visual_judge_provider_boundary";
    public const string VisualJudgeProviderRequiredMarker = "visual_judge_provider_required";
    public const string ExternalProviderNotConfiguredMarker = "external_provider_not_configured";
    public const string ExternalProviderCallFailedMarker = "external_provider_call_failed";
    public const string ExternalProviderCallFailedWarning = "VISUAL_JUDGE_PROVIDER_REQUIRED_EXTERNAL_PROVIDER_CALL_FAILED";
    public const string TierPolicyMarker = "tier_policy";
    public const string ImprovementLoopMarker = "improvement_loop";
    public const string ProductNotReadyMarker = "product_not_ready";
    public const string VisualAiExecutedFalseMarker = "visual_ai_executed_false";
    public const string RedactionMarker = "redaction";
    public const string CostCapMarker = "cost_cap";
    public const string CostMeasurementRequired = "cost_measurement_required";
    public const string ExternalVisualCostMeasurementMissingMarker = "external_visual_cost_measurement_missing";
    public const string TierBudgetCapAppliedMarker = "tier_budget_cap_applied";
    public const string TierIterationCapAppliedMarker = "tier_iteration_cap_applied";
    public const string TierBudgetEntitlementDefaultAppliedMarker = "tier_budget_entitlement_default_applied";
    public const string TierIterationEntitlementDefaultAppliedMarker = "tier_iteration_entitlement_default_applied";
    public const string TestProviderOnlyMarker = "test_provider_only";
    public const string ConfiguredOrLocalLabelerBoundary = "configured_or_local_labeler_test_double";
    public const string LocalVisualLabelerMarker = "local_visual_labeler_default";
    public const string ProvisionalTierQualityThresholdPolicyMarker = "provisional_tier_quality_threshold_policy";
    public const string ProvisionalTierQualityThresholdPolicyStatus = "provisional_pending_luba_confirmation";
    public const string ProvisionalTierQualityThresholdSource = "server_owned_provisional_tier_default";
    public const string CanonicalQualityThresholdMetadataOverrideSource = "canonical_quality_threshold_metadata_override";
    public const string TypedRenderQaPassThresholdSource = "typed_render_qa_pass_threshold_override";
    public const string RenderQaPassThresholdInvalidErrorCode = "DESIGN_CONVERSION_RENDER_QA_PASS_THRESHOLD_INVALID";
    public const string TypedMaxImprovementIterationsSource = "typed_max_improvement_iterations_override";
    public const string LegacyMaxImprovementCandidatesIterationSource = "legacy_max_improvement_candidates_iteration_budget";
    public const string DirectImprovementLoopMaxAttemptsSource = "direct_improvement_loop_max_attempts";
    public const string MaxImprovementIterationsInvalidErrorCode = "DESIGN_CONVERSION_MAX_IMPROVEMENT_ITERATIONS_INVALID";
    public const string MaxTeacherCallsInvalidErrorCode = "DESIGN_CONVERSION_MAX_TEACHER_CALLS_INVALID";
    public const string MaxTeacherCallsContractBoundary =
        "contract_transport_with_dpo_attestation_teacher_calls_request_scoped_cap_v2";
    public const string NonTierQualityThresholdFallbackSource = "server_owned_non_tier_fallback";
    public const double ProvisionalFreeTierQualityThreshold = 0.75;
    public const double ProvisionalPaidTierQualityThreshold = 0.85;
    public const double ProvisionalProTierQualityThreshold = 0.90;
    public const double ProvisionalDevTierQualityThreshold = 0.80;

    public static bool IsValidConfiguredRenderQaPassThreshold(double? threshold, string? tier)
        => !threshold.HasValue || IsValidQualityThresholdForTier(threshold.Value, tier);

    public static bool IsValidConfiguredMaxImprovementIterations(int? maxImprovementIterations)
        => !maxImprovementIterations.HasValue ||
           maxImprovementIterations.Value is >= 0 and < DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts;

    public static string MaxImprovementIterationsInvalidErrorMessage()
        => $"DesignConversion max improvement iterations must be between 0 and {(DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts - 1).ToString(CultureInfo.InvariantCulture)} when supplied.";

    public static bool IsValidConfiguredMaxTeacherCalls(int? maxTeacherCalls)
        => !maxTeacherCalls.HasValue || maxTeacherCalls.Value >= 0;

    public static string MaxTeacherCallsInvalidErrorMessage()
        => "DesignConversion max teacher calls must be greater than or equal to zero when supplied.";

    public static bool IsServerOwnedEntitlementTier(string? tier)
        => NormalizeTier(tier) is "free" or "paid" or "pro";

    internal static int ResolveEffectiveTierMaxImprovementCandidates(
        int configuredMaxAttempts,
        string? tier)
        => ResolveEffectiveMaxImprovementCandidates(
            Math.Max(0, configuredMaxAttempts),
            ResolveTierMaxImprovementCandidatesCap(NormalizeTier(tier)));

    internal static decimal ResolveEffectiveTierBudgetUsd(
        decimal configuredBudgetUsd,
        string? tier)
        => ResolveEffectiveBudgetUsd(
            Math.Max(0m, configuredBudgetUsd),
            ResolveTierBudgetCapUsd(NormalizeTier(tier)));

    public const string ContinueImprovement = "continue_improvement";
    public const string QualityReached = "quality_reached";
    public const string BudgetCapReached = "budget_cap_reached";
    public const string ProviderRequired = "provider_required";
    public const string RenderManifestIncomplete = "render_manifest_incomplete";
    public const string ViewportOperationFailed = "viewport_operation_failed";
    public const string DecisionAccepted = "accepted";
    public const string DecisionRetry = "retry";
    public const string DecisionQuarantine = "quarantine";
    public const string DecisionBlocked = "blocked";
    public const string RetryModeNone = "none";
    public const string RetryModeImproveCandidate = "improve_candidate";
    public const string RetryModeRetrySameCandidate = "retry_same_candidate";
    public const string CoverageRetryMarker = "coverage_retry";

    public static DesignConversionVisualQaBoundaryTrace Evaluate(
        DesignConversionBudgetMetadata budget,
        IReadOnlyList<RenderQaResolution> requiredResolutions,
        IReadOnlyList<DesignConversionRenderResult> renderResults,
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults,
        IReadOnlyList<DesignConversionViewportFailure>? viewportFailures = null,
        int improvementAttemptCount = 1,
        IReadOnlyList<DesignConversionQaComparisonArtifact>? comparisonArtifacts = null,
        bool includeTrustedExternalComparisonCosts = true)
    {
        comparisonArtifacts ??= [];
        var comparisonArtifactsForCost = includeTrustedExternalComparisonCosts
            ? comparisonArtifacts
            : Array.Empty<DesignConversionQaComparisonArtifact>();
        var viewportCoverage = EvaluateViewportCoverage(requiredResolutions, renderResults);
        var viewportOperationFailed = viewportFailures is { Count: > 0 };
        var externalProviderCallFailed = visualJudgeResults.Any(IsExternalProviderCallFailed);
        var externalProviderNotConfigured = HasExternalProviderNotConfiguredResult(visualJudgeResults);
        var providerRequired = externalProviderCallFailed || externalProviderNotConfigured;
        var externalAiExecuted = visualJudgeResults.Any(result => result.Provider.ExternalCallExecuted);
        var testProviderOnly = visualJudgeResults.Any(result => result.Provider.Metadata.TryGetValue(
            TestProviderOnlyMarker,
            out var marker) && IsTruthy(marker));
        var localLabeler = visualJudgeResults.Any(result => result.Provider.Metadata.TryGetValue(
            LocalVisualLabelerMarker,
            out var marker) && IsTruthy(marker));
        var providerBoundary = externalProviderNotConfigured
            ? ExternalProviderNotConfiguredMarker
            : externalProviderCallFailed
                ? ExternalProviderCallFailedMarker
                : localLabeler
                    ? LocalVisualLabelerMarker
                    : ConfiguredOrLocalLabelerBoundary;
        var bestScore = visualJudgeResults.Count == 0
            ? 0.0
            : visualJudgeResults.Max(result => result.Verdict.Score);
        var acceptanceScore = visualJudgeResults.Count == 0
            ? 0.0
            : visualJudgeResults.Min(result => result.Verdict.Score);
        var externalCostMeasurement = EvaluateExternalCostMeasurement(visualJudgeResults, comparisonArtifactsForCost);
        var estimatedCost = visualJudgeResults
            .Where(ShouldIncludeEstimatedCost)
            .Select(result => result.EstimatedCostUsd!.Value)
            .Concat(comparisonArtifactsForCost
                .Select(TryReadTrustedExternalComparisonCost)
                .Where(cost => cost.HasValue)
                .Select(cost => cost!.Value))
            .Sum();
        var threshold = ResolveQualityThresholdResolution(budget);
        var tierDecision = DecideTierPolicy(
            budget,
            viewportCoverage,
            providerRequired,
            viewportOperationFailed,
            bestScore,
            acceptanceScore,
            threshold.Value,
            threshold,
            estimatedCost,
            externalCostMeasurement.IsComplete,
            externalCostMeasurement.MissingCount,
            externalCostMeasurement.CostSource,
            improvementAttemptCount);
        var visualDecision = DecideVisualComparison(
            viewportCoverage,
            visualJudgeResults,
            tierDecision);

        var evidenceRefs = viewportCoverage.EvidenceRefs
            .Concat(tierDecision.EvidenceRefs)
            .Concat(visualDecision.EvidenceRefs)
            .Append(VisualJudgeProviderBoundaryMarker)
            .Append(externalAiExecuted
                ? "external_ai_visual_judge_executed:true"
                : "external_ai_visual_judge_executed:false")
            .Append(externalAiExecuted ? "visual_ai_executed_true" : VisualAiExecutedFalseMarker)
            .Append($"external_cost_measurement_complete:{BoolToken(externalCostMeasurement.IsComplete)}")
            .Append($"missing_external_cost_measurement_count:{externalCostMeasurement.MissingCount.ToString(CultureInfo.InvariantCulture)}")
            .Append($"cost_source:{externalCostMeasurement.CostSource}")
            .Append(ProductNotReadyMarker);
        if (!externalCostMeasurement.IsComplete)
            evidenceRefs = evidenceRefs
                .Append(CostMeasurementRequired)
                .Append(ExternalVisualCostMeasurementMissingMarker);
        if (testProviderOnly)
            evidenceRefs = evidenceRefs.Append(TestProviderOnlyMarker);
        if (localLabeler)
            evidenceRefs = evidenceRefs.Append(LocalVisualLabelerMarker);
        if (providerRequired)
            evidenceRefs = evidenceRefs.Append(VisualJudgeProviderRequiredMarker);
        if (externalProviderNotConfigured)
            evidenceRefs = evidenceRefs.Append(ExternalProviderNotConfiguredMarker);
        if (externalProviderCallFailed)
            evidenceRefs = evidenceRefs.Append(ExternalProviderCallFailedMarker);
        if (viewportOperationFailed)
            evidenceRefs = evidenceRefs.Concat(viewportFailures!.SelectMany(failure => failure.EvidenceRefs));

        var warnings = new List<string>();
        if (!viewportCoverage.IsComplete)
            warnings.Add("MULTI_RESOLUTION_RENDER_EVIDENCE_INCOMPLETE");
        if (externalProviderNotConfigured)
            warnings.Add("VISUAL_JUDGE_PROVIDER_REQUIRED_EXTERNAL_PROVIDER_NOT_CONFIGURED");
        if (externalProviderCallFailed)
            warnings.Add(ExternalProviderCallFailedWarning);
        if (!externalCostMeasurement.IsComplete)
            warnings.Add("EXTERNAL_VISUAL_COST_MEASUREMENT_MISSING");
        if (viewportOperationFailed)
            warnings.Add("VIEWPORT_OPERATION_FAILED");
        warnings.Add("PRODUCT_NOT_READY_REAL_PROVIDER_BROWSER_E2E_TRAINED_WEIGHTS_NOT_PROVEN");

        return new DesignConversionVisualQaBoundaryTrace(
            MultiResolutionRender: viewportCoverage,
            VisualJudgeStatus: visualJudgeResults.LastOrDefault()?.Status ?? ProviderRequired,
            VisualJudgeProviderBoundary: providerRequired && externalProviderNotConfigured
                ? ProviderRequired
                : providerBoundary,
            ExternalAiVisualJudgeExecuted: externalAiExecuted,
            TierPolicy: tierDecision,
            VisualDecision: visualDecision,
            ProductReady: false,
            TrainedFeatureClaimed: false,
            EvidenceRefs: evidenceRefs.Distinct(StringComparer.Ordinal).ToArray(),
            Warnings: warnings.Distinct(StringComparer.Ordinal).ToArray());
    }

    public static DesignConversionViewportCoverage EvaluateViewportCoverage(
        IReadOnlyList<RenderQaResolution> requiredResolutions,
        IReadOnlyList<DesignConversionRenderResult> renderResults)
    {
        var required = NormalizeResolutionIds(requiredResolutions);
        var observed = NormalizeResolutionIds(renderResults
            .Select(result => result.Image.Resolution)
            .ToArray());
        var missing = required
            .Where(id => !observed.Contains(id, StringComparer.Ordinal))
            .ToArray();
        var isMultiResolution = required.Count >= 2;
        var isComplete = required.Count > 0 && missing.Length == 0;

        return new DesignConversionViewportCoverage(
            IsMultiResolution: isMultiResolution,
            IsComplete: isComplete,
            RequiredViewportIds: required,
            ObservedViewportIds: observed,
            MissingViewportIds: missing,
            EvidenceRefs:
            [
                MultiResolutionRenderMarker,
                $"multi_resolution_render:required={required.Count.ToString(CultureInfo.InvariantCulture)}",
                $"multi_resolution_render:observed={observed.Count.ToString(CultureInfo.InvariantCulture)}",
                isComplete ? "multi_resolution_render:complete" : "multi_resolution_render:incomplete",
                ProductNotReadyMarker
            ]);
    }

    private static DesignConversionTierPolicyDecision DecideTierPolicy(
        DesignConversionBudgetMetadata budget,
        DesignConversionViewportCoverage viewportCoverage,
        bool providerRequired,
        bool viewportOperationFailed,
        double bestScore,
        double acceptanceScore,
        double requiredScore,
        QualityThresholdResolution qualityThreshold,
        decimal estimatedCost,
        bool externalCostMeasurementComplete,
        int missingExternalCostMeasurementCount,
        string costSource,
        int attemptCount)
    {
        var tier = string.IsNullOrWhiteSpace(budget.Tier) ? "unspecified" : budget.Tier.Trim();
        var normalizedTier = NormalizeTier(tier);
        var configuredBudgetUsd = Math.Max(0m, budget.BudgetUsd);
        var submittedMaxImprovementCandidates =
            Math.Max(0, budget.MaxImprovementCandidates);
        var configuredMaxAttempts = ResolveConfiguredMaxAttempts(budget);
        var maxAttemptsSource = budget.MaxImprovementIterations.HasValue
            ? TypedMaxImprovementIterationsSource
            : LegacyMaxImprovementCandidatesIterationSource;
        var tierBudgetCapUsd = ResolveTierBudgetCapUsd(normalizedTier);
        var tierMaxAttemptsCap = ResolveTierMaxImprovementCandidatesCap(normalizedTier);
        var budgetUsd = ResolveEffectiveTierBudgetUsd(configuredBudgetUsd, tier);
        var maxAttempts = ResolveEffectiveTierMaxImprovementCandidates(configuredMaxAttempts, tier);
        var tierBudgetCapApplied = budgetUsd < configuredBudgetUsd;
        var tierIterationCapApplied = maxAttempts < configuredMaxAttempts;
        var tierBudgetEntitlementDefaultApplied =
            configuredBudgetUsd <= 0m &&
            tierBudgetCapUsd.HasValue;
        var tierIterationEntitlementDefaultApplied =
            !budget.MaxImprovementIterations.HasValue &&
            submittedMaxImprovementCandidates <= 0 &&
            tierMaxAttemptsCap.HasValue;
        string stopReason;
        bool shouldContinue;
        string retryMode;

        if (providerRequired)
        {
            stopReason = ProviderRequired;
            shouldContinue = false;
            retryMode = RetryModeNone;
        }
        else if (viewportOperationFailed)
        {
            stopReason = ViewportOperationFailed;
            shouldContinue = false;
            retryMode = RetryModeNone;
        }
        else if (!viewportCoverage.IsComplete)
        {
            stopReason = RenderManifestIncomplete;
            shouldContinue = false;
            retryMode = RetryModeRetrySameCandidate;
        }
        else if (!externalCostMeasurementComplete)
        {
            stopReason = CostMeasurementRequired;
            shouldContinue = false;
            retryMode = RetryModeNone;
        }
        else if (acceptanceScore >= requiredScore)
        {
            stopReason = QualityReached;
            shouldContinue = false;
            retryMode = RetryModeNone;
        }
        else if (estimatedCost >= budgetUsd || attemptCount >= maxAttempts)
        {
            stopReason = BudgetCapReached;
            shouldContinue = false;
            retryMode = RetryModeNone;
        }
        else
        {
            stopReason = ContinueImprovement;
            shouldContinue = true;
            retryMode = RetryModeImproveCandidate;
        }

        var evidenceRefs = new List<string>
        {
            TierPolicyMarker,
            ImprovementLoopMarker,
            $"tier_policy:stop_reason={stopReason}",
            $"tier_policy:retry_mode={retryMode}",
            $"tier_policy:tier={tier}",
            $"tier_policy:quality_threshold={requiredScore.ToString("0.####", CultureInfo.InvariantCulture)}",
            $"tier_policy:quality_threshold_source={qualityThreshold.Source}",
            $"tier_policy:quality_threshold_policy_status={qualityThreshold.PolicyStatus}",
            $"tier_policy:quality_threshold_pending_luba_confirmation={BoolToken(qualityThreshold.PendingLubaConfirmation)}",
            "tier_policy:quality_score_aggregation=minimum_all_visual_judge_results",
            $"tier_policy:best_visual_score={bestScore.ToString("0.####", CultureInfo.InvariantCulture)}",
            $"tier_policy:acceptance_visual_score={acceptanceScore.ToString("0.####", CultureInfo.InvariantCulture)}",
            $"tier_policy:worst_visual_score={acceptanceScore.ToString("0.####", CultureInfo.InvariantCulture)}",
            $"tier_policy:external_cost_measurement_complete={BoolToken(externalCostMeasurementComplete)}",
            $"tier_policy:missing_external_cost_measurement_count={missingExternalCostMeasurementCount.ToString(CultureInfo.InvariantCulture)}",
            $"tier_policy:cost_source={costSource}",
            $"tier_policy:submitted_max_improvement_candidates={submittedMaxImprovementCandidates.ToString(CultureInfo.InvariantCulture)}",
            $"tier_policy:max_attempts_source={maxAttemptsSource}",
            $"tier_policy:effective_max_improvement_candidates={maxAttempts.ToString(CultureInfo.InvariantCulture)}",
            $"tier_policy:budget_entitlement_default_applied={BoolToken(tierBudgetEntitlementDefaultApplied)}",
            $"tier_policy:iteration_entitlement_default_applied={BoolToken(tierIterationEntitlementDefaultApplied)}",
            ProductNotReadyMarker
        };
        if (qualityThreshold.PendingLubaConfirmation)
            evidenceRefs.Add(ProvisionalTierQualityThresholdPolicyMarker);
        if (retryMode == RetryModeRetrySameCandidate)
            evidenceRefs.Add(CoverageRetryMarker);
        if (tierBudgetCapApplied)
            evidenceRefs.Add(TierBudgetCapAppliedMarker);
        if (tierIterationCapApplied)
            evidenceRefs.Add(TierIterationCapAppliedMarker);
        if (tierBudgetEntitlementDefaultApplied)
            evidenceRefs.Add(TierBudgetEntitlementDefaultAppliedMarker);
        if (tierIterationEntitlementDefaultApplied)
            evidenceRefs.Add(TierIterationEntitlementDefaultAppliedMarker);
        if (stopReason == BudgetCapReached)
        {
            evidenceRefs.Add(CostCapMarker);
            evidenceRefs.Add($"tier_policy:cost_cap={budgetUsd.ToString("0.####", CultureInfo.InvariantCulture)}");
            if (attemptCount >= maxAttempts)
                evidenceRefs.Add("tier_policy:iteration_cap_reached");
        }
        if (!externalCostMeasurementComplete)
        {
            evidenceRefs.Add(CostMeasurementRequired);
            evidenceRefs.Add(ExternalVisualCostMeasurementMissingMarker);
        }

        return new DesignConversionTierPolicyDecision(
            Tier: tier,
            RequiredScore: requiredScore,
            BestScore: bestScore,
            EstimatedCostUsd: estimatedCost,
            BudgetUsd: budgetUsd,
            RemainingBudgetUsd: Math.Max(0m, budgetUsd - estimatedCost),
            AttemptCount: attemptCount,
            MaxImprovementCandidates: maxAttempts,
            StopReason: stopReason,
            ShouldContinue: shouldContinue,
            EvidenceRefs: evidenceRefs,
            Metadata: new Dictionary<string, string>
            {
                ["tier"] = tier,
                ["quality_threshold"] = requiredScore.ToString("0.###", CultureInfo.InvariantCulture),
                ["quality_threshold_source"] = qualityThreshold.Source,
                ["quality_threshold_policy_status"] = qualityThreshold.PolicyStatus,
                ["quality_threshold_policy_marker"] = qualityThreshold.PolicyMarker,
                ["quality_threshold_pending_luba_confirmation"] = BoolToken(qualityThreshold.PendingLubaConfirmation),
                ["quality_score_aggregation"] = "minimum_all_visual_judge_results",
                ["best_visual_score"] = bestScore.ToString("0.###", CultureInfo.InvariantCulture),
                ["acceptance_visual_score"] = acceptanceScore.ToString("0.###", CultureInfo.InvariantCulture),
                ["worst_visual_score"] = acceptanceScore.ToString("0.###", CultureInfo.InvariantCulture),
                ["estimated_cost_usd"] = estimatedCost.ToString("0.####", CultureInfo.InvariantCulture),
                ["external_cost_measurement_complete"] = BoolToken(externalCostMeasurementComplete),
                ["external_visual_cost_measurement_missing"] = BoolToken(!externalCostMeasurementComplete),
                ["missing_external_cost_measurement_count"] = missingExternalCostMeasurementCount.ToString(CultureInfo.InvariantCulture),
                ["cost_source"] = costSource,
                ["budget_usd"] = budgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["remaining_budget_usd"] = Math.Max(0m, budgetUsd - estimatedCost).ToString("0.####", CultureInfo.InvariantCulture),
                ["cost_cap"] = budgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["attempt_count"] = attemptCount.ToString(CultureInfo.InvariantCulture),
                ["max_improvement_candidates"] = maxAttempts.ToString(CultureInfo.InvariantCulture),
                ["submitted_budget_usd"] = configuredBudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["submitted_max_improvement_candidates"] =
                    submittedMaxImprovementCandidates.ToString(CultureInfo.InvariantCulture),
                ["typed_max_improvement_iterations"] =
                    budget.MaxImprovementIterations?.ToString(CultureInfo.InvariantCulture) ??
                    "not_supplied",
                ["max_attempts_source"] = maxAttemptsSource,
                ["effective_budget_usd"] = budgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["effective_max_improvement_candidates"] = maxAttempts.ToString(CultureInfo.InvariantCulture),
                ["tier_budget_cap_usd"] = tierBudgetCapUsd.HasValue
                    ? tierBudgetCapUsd.Value.ToString("0.####", CultureInfo.InvariantCulture)
                    : "unbounded",
                ["tier_max_improvement_candidates"] = tierMaxAttemptsCap.HasValue
                    ? tierMaxAttemptsCap.Value.ToString(CultureInfo.InvariantCulture)
                    : "unbounded",
                ["tier_policy_budget_cap_applied"] = tierBudgetCapApplied.ToString(),
                ["tier_policy_iteration_cap_applied"] = tierIterationCapApplied.ToString(),
                ["tier_policy_budget_entitlement_default_applied"] = tierBudgetEntitlementDefaultApplied.ToString(),
                ["tier_policy_iteration_entitlement_default_applied"] = tierIterationEntitlementDefaultApplied.ToString(),
                ["budget_stop_source"] = BudgetStopSource(stopReason, estimatedCost, budgetUsd, attemptCount, maxAttempts),
                ["retry_mode"] = retryMode,
                ["product_ready"] = bool.FalseString,
                ["trained_feature_claimed"] = bool.FalseString
            },
            RetryMode: retryMode,
            ExternalCostMeasurementComplete: externalCostMeasurementComplete,
            MissingExternalCostMeasurementCount: missingExternalCostMeasurementCount,
            CostSource: costSource)
        {
            AcceptanceScore = acceptanceScore
        };
    }

    private static DesignConversionVisualComparisonDecision DecideVisualComparison(
        DesignConversionViewportCoverage viewportCoverage,
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults,
        DesignConversionTierPolicyDecision tierDecision)
    {
        var latestJudge = visualJudgeResults.LastOrDefault();
        var externalProviderCallFailed = visualJudgeResults.Any(IsExternalProviderCallFailed);
        var externalProviderNotConfigured = HasExternalProviderNotConfiguredResult(visualJudgeResults);
        var providerBoundary = externalProviderNotConfigured
            ? ProviderRequired
            : externalProviderCallFailed
                ? ExternalProviderCallFailedMarker
                : latestJudge?.Provider.Metadata.TryGetValue(LocalVisualLabelerMarker, out var localLabeler) == true && IsTruthy(localLabeler)
                    ? LocalVisualLabelerMarker
                    : latestJudge?.Provider.Metadata.TryGetValue(TestProviderOnlyMarker, out var testProviderOnly) == true && IsTruthy(testProviderOnly)
                        ? TestProviderOnlyMarker
                        : latestJudge is null
                            ? ProviderRequired
                            : ConfiguredOrLocalLabelerBoundary;
        var decision = tierDecision.StopReason switch
        {
            QualityReached => DecisionAccepted,
            ContinueImprovement => DecisionRetry,
            RenderManifestIncomplete => DecisionQuarantine,
            ViewportOperationFailed => DecisionBlocked,
            ProviderRequired => DecisionBlocked,
            CostMeasurementRequired => DecisionBlocked,
            BudgetCapReached => DecisionBlocked,
            _ => DecisionBlocked
        };
        var findings = BuildCorrectionFindings(viewportCoverage, visualJudgeResults, tierDecision, decision);
        var qualityMet = tierDecision.AcceptanceScore >= tierDecision.RequiredScore;
        var costBudgetMet = tierDecision.ExternalCostMeasurementComplete &&
            tierDecision.EstimatedCostUsd <= tierDecision.BudgetUsd;
        var canRetry = decision == DecisionRetry && tierDecision.ShouldContinue;
        var evidenceRefs = findings
            .Select(finding => $"visual_decision_correction:{finding.Category}")
            .Append("visual_comparison_decision")
            .Append($"visual_decision:{decision}")
            .Append($"visual_provider_boundary:{providerBoundary}")
            .Append("visual_decision:quality_score_aggregation=minimum_all_visual_judge_results")
            .Append($"visual_decision:best_visual_score={tierDecision.BestScore.ToString("0.####", CultureInfo.InvariantCulture)}")
            .Append($"visual_decision:acceptance_visual_score={tierDecision.AcceptanceScore.ToString("0.####", CultureInfo.InvariantCulture)}")
            .Append($"visual_decision:worst_visual_score={tierDecision.AcceptanceScore.ToString("0.####", CultureInfo.InvariantCulture)}")
            .Append($"visual_decision:external_cost_measurement_complete={BoolToken(tierDecision.ExternalCostMeasurementComplete)}")
            .Append($"visual_decision:missing_external_cost_measurement_count={tierDecision.MissingExternalCostMeasurementCount.ToString(CultureInfo.InvariantCulture)}")
            .Append($"visual_decision:cost_source={tierDecision.CostSource}")
            .Concat(tierDecision.ExternalCostMeasurementComplete
                ? Array.Empty<string>()
                :
                [
                    CostMeasurementRequired,
                    ExternalVisualCostMeasurementMissingMarker
                ])
            .Append(ProductNotReadyMarker)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var canPersistTrainingCandidate = tierDecision.ExternalCostMeasurementComplete &&
            (decision == DecisionAccepted || decision == DecisionRetry);

        return new DesignConversionVisualComparisonDecision(
            Decision: decision,
            QualityScore: tierDecision.AcceptanceScore,
            RequiredThreshold: tierDecision.RequiredScore,
            EstimatedCostUsd: tierDecision.EstimatedCostUsd,
            BudgetUsd: tierDecision.BudgetUsd,
            RemainingBudgetUsd: tierDecision.RemainingBudgetUsd,
            CostBudgetMet: costBudgetMet,
            QualityThresholdMet: qualityMet,
            CanRetry: canRetry,
            CanPersistTrainingCandidate: canPersistTrainingCandidate,
            ProviderBoundaryStatus: providerBoundary,
            ExternalAiVisualJudgeExecuted: visualJudgeResults.Any(result => result.Provider.ExternalCallExecuted),
            CorrectionFindings: findings,
            EvidenceRefs: evidenceRefs,
            Metadata: new Dictionary<string, string>
            {
                ["visual_decision"] = decision,
                ["quality_score"] = tierDecision.AcceptanceScore.ToString("0.####", CultureInfo.InvariantCulture),
                ["quality_score_aggregation"] = "minimum_all_visual_judge_results",
                ["best_visual_score"] = tierDecision.BestScore.ToString("0.####", CultureInfo.InvariantCulture),
                ["acceptance_visual_score"] = tierDecision.AcceptanceScore.ToString("0.####", CultureInfo.InvariantCulture),
                ["worst_visual_score"] = tierDecision.AcceptanceScore.ToString("0.####", CultureInfo.InvariantCulture),
                ["required_threshold"] = tierDecision.RequiredScore.ToString("0.####", CultureInfo.InvariantCulture),
                ["estimated_cost_usd"] = tierDecision.EstimatedCostUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["external_cost_measurement_complete"] = BoolToken(tierDecision.ExternalCostMeasurementComplete),
                ["external_visual_cost_measurement_missing"] = BoolToken(!tierDecision.ExternalCostMeasurementComplete),
                ["missing_external_cost_measurement_count"] = tierDecision.MissingExternalCostMeasurementCount.ToString(CultureInfo.InvariantCulture),
                ["cost_source"] = tierDecision.CostSource,
                ["budget_usd"] = tierDecision.BudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["remaining_budget_usd"] = tierDecision.RemainingBudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
                ["cost_budget_met"] = costBudgetMet.ToString(),
                ["quality_threshold_met"] = qualityMet.ToString(),
                ["can_retry"] = canRetry.ToString(),
                ["can_persist_training_candidate"] = canPersistTrainingCandidate.ToString(),
                ["provider_boundary_status"] = providerBoundary,
                ["external_ai_visual_judge_executed"] = visualJudgeResults.Any(result => result.Provider.ExternalCallExecuted).ToString(),
                ["correction_finding_count"] = findings.Count.ToString(CultureInfo.InvariantCulture)
            });
    }

    private static IReadOnlyList<DesignConversionVisualCorrectionFinding> BuildCorrectionFindings(
        DesignConversionViewportCoverage viewportCoverage,
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults,
        DesignConversionTierPolicyDecision tierDecision,
        string decision)
    {
        var findings = new List<DesignConversionVisualCorrectionFinding>();
        foreach (var viewportId in viewportCoverage.MissingViewportIds)
        {
            findings.Add(new DesignConversionVisualCorrectionFinding(
                FindingId: $"render-manifest-missing:{viewportId}",
                Category: RenderManifestIncomplete,
                Severity: "blocker",
                Description: $"Required viewport '{viewportId}' was not rendered for visual comparison.",
                SuggestedAction: "Render the missing viewport before accepting or retrying the visual decision.",
                EvidenceImageIds: [],
                CanDriveRetry: false));
        }

        if (decision == DecisionBlocked &&
            tierDecision.StopReason == ProviderRequired &&
            visualJudgeResults.Any(IsExternalProviderCallFailed))
        {
            findings.Add(new DesignConversionVisualCorrectionFinding(
                FindingId: "visual-provider-call-failed",
                Category: ExternalProviderCallFailedMarker,
                Severity: "blocker",
                Description: "The configured external visual provider call returned a terminal non-success HTTP status; this cannot be accepted as visual AI proof.",
                SuggestedAction: "Inspect provider availability and bounded retry evidence before dispatching another visual judge call.",
                EvidenceImageIds: [],
                CanDriveRetry: false));
        }

        if (decision == DecisionBlocked &&
            tierDecision.StopReason == ProviderRequired &&
            HasExternalProviderNotConfiguredResult(visualJudgeResults))
        {
            findings.Add(new DesignConversionVisualCorrectionFinding(
                FindingId: "visual-provider-missing",
                Category: ExternalProviderNotConfiguredMarker,
                Severity: "blocker",
                Description: "No configured external visual provider result was available; this cannot be accepted as visual AI proof.",
                SuggestedAction: "Configure an approved provider or use an explicit local labeler/test-double path.",
                EvidenceImageIds: [],
                CanDriveRetry: false));
        }

        if (decision == DecisionBlocked && tierDecision.StopReason == BudgetCapReached)
        {
            findings.Add(new DesignConversionVisualCorrectionFinding(
                FindingId: "tier-cost-budget-exhausted",
                Category: BudgetCapReached,
                Severity: "blocker",
                Description: "The tier cost or attempt budget was exhausted before the required visual threshold was met.",
                SuggestedAction: "Stop the local loop or request a higher approved budget before another visual judge call.",
                EvidenceImageIds: [],
                CanDriveRetry: false));
        }

        if (decision == DecisionBlocked && tierDecision.StopReason == CostMeasurementRequired)
        {
            findings.Add(new DesignConversionVisualCorrectionFinding(
                FindingId: "external-visual-cost-measurement-missing",
                Category: ExternalVisualCostMeasurementMissingMarker,
                Severity: "blocker",
                Description: "An executed external visual provider result did not report cost, so the local boundary cannot treat the call as free.",
                SuggestedAction: "Require provider-reported cost metadata before accepting or persisting this visual result.",
                EvidenceImageIds: [],
                CanDriveRetry: false));
        }

        foreach (var defect in visualJudgeResults.SelectMany(result => result.Verdict.Defects))
        {
            findings.Add(new DesignConversionVisualCorrectionFinding(
                FindingId: defect.DefectId,
                Category: defect.Category,
                Severity: defect.Severity,
                Description: defect.Description,
                SuggestedAction: $"Repair visual mismatch category '{defect.Category}' before the next render.",
                EvidenceImageIds: defect.EvidenceImageIds,
                CanDriveRetry: decision == DecisionRetry));
        }

        if (decision == DecisionRetry && findings.Count == 0)
        {
            findings.Add(new DesignConversionVisualCorrectionFinding(
                FindingId: "visual-score-below-threshold",
                Category: "quality_threshold_not_met",
                Severity: "minor",
                Description: "Visual score is below the user tier threshold, but cost and attempt budget allow another local improvement iteration.",
                SuggestedAction: "Use comparison artifacts and generated render refs to propose a corrected candidate.",
                EvidenceImageIds: [],
                CanDriveRetry: true));
        }

        return findings
            .GroupBy(finding => finding.FindingId, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
    }

    internal static double ResolveQualityThreshold(DesignConversionBudgetMetadata budget)
        => ResolveQualityThresholdResolution(budget).Value;

    internal static string ResolveQualityThresholdSource(DesignConversionBudgetMetadata budget)
        => ResolveQualityThresholdResolution(budget).Source;

    private static QualityThresholdResolution ResolveQualityThresholdResolution(DesignConversionBudgetMetadata budget)
    {
        if (budget.RenderQaPassThreshold.HasValue &&
            IsValidQualityThresholdForTier(budget.RenderQaPassThreshold.Value, budget.Tier))
        {
            return new QualityThresholdResolution(
                budget.RenderQaPassThreshold.Value,
                TypedRenderQaPassThresholdSource,
                TypedRenderQaPassThresholdSource,
                TypedRenderQaPassThresholdSource,
                PendingLubaConfirmation: false);
        }

        if (TryResolveLegacyQualityThreshold(budget, out var legacyThreshold))
        {
            return new QualityThresholdResolution(
                legacyThreshold,
                CanonicalQualityThresholdMetadataOverrideSource,
                CanonicalQualityThresholdMetadataOverrideSource,
                CanonicalQualityThresholdMetadataOverrideSource,
                PendingLubaConfirmation: false);
        }

        return (budget.Tier ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "free" => ProvisionalQualityThreshold(ProvisionalFreeTierQualityThreshold),
            "paid" => ProvisionalQualityThreshold(ProvisionalPaidTierQualityThreshold),
            "pro" => ProvisionalQualityThreshold(ProvisionalProTierQualityThreshold),
            "dev" => ProvisionalQualityThreshold(ProvisionalDevTierQualityThreshold),
            "internal" => NonTierFallbackQualityThreshold(),
            "thin-client" => NonTierFallbackQualityThreshold(),
            _ => NonTierFallbackQualityThreshold()
        };
    }

    private static bool TryResolveLegacyQualityThreshold(DesignConversionBudgetMetadata budget, out double threshold)
    {
        threshold = 0.0;
        if (budget.Metadata is null ||
            !budget.Metadata.TryGetValue("quality_threshold", out var configured) ||
            !double.TryParse(configured, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ||
            !IsValidQualityThresholdForTier(parsed, budget.Tier))
        {
            return false;
        }

        threshold = parsed;
        return true;
    }

    private static bool IsValidQualityThresholdForTier(double threshold, string? tier)
        => !double.IsNaN(threshold) &&
           !double.IsInfinity(threshold) &&
           threshold > 0.0 &&
           threshold <= 1.0 &&
           !LowersServerOwnedTierThreshold(tier, threshold);

    private static bool LowersServerOwnedTierThreshold(string? tier, double threshold)
        => NormalizeTier(tier) switch
        {
            "free" => threshold < ProvisionalFreeTierQualityThreshold,
            "paid" => threshold < ProvisionalPaidTierQualityThreshold,
            "pro" => threshold < ProvisionalProTierQualityThreshold,
            _ => false
        };

    private static QualityThresholdResolution ProvisionalQualityThreshold(double threshold)
        => new(
            threshold,
            ProvisionalTierQualityThresholdSource,
            ProvisionalTierQualityThresholdPolicyStatus,
            ProvisionalTierQualityThresholdPolicyMarker,
            PendingLubaConfirmation: true);

    private static QualityThresholdResolution NonTierFallbackQualityThreshold()
        => new(
            0.85,
            NonTierQualityThresholdFallbackSource,
            NonTierQualityThresholdFallbackSource,
            NonTierQualityThresholdFallbackSource,
            PendingLubaConfirmation: false);

    private static string NormalizeTier(string? tier)
        => (tier ?? string.Empty).Trim().ToLowerInvariant();

    private static decimal? ResolveTierBudgetCapUsd(string normalizedTier)
        => normalizedTier switch
        {
            "free" => 0.05m,
            "paid" => 0.25m,
            "pro" => 1.00m,
            _ => null
        };

    private static int? ResolveTierMaxImprovementCandidatesCap(string normalizedTier)
        => normalizedTier switch
        {
            "free" => 1,
            "paid" => 2,
            "pro" => 4,
            _ => null
        };

    private static decimal ResolveEffectiveBudgetUsd(decimal configuredBudgetUsd, decimal? tierBudgetCapUsd)
    {
        if (configuredBudgetUsd <= 0m)
            return tierBudgetCapUsd ?? 0m;

        return tierBudgetCapUsd.HasValue
            ? Math.Min(configuredBudgetUsd, tierBudgetCapUsd.Value)
            : configuredBudgetUsd;
    }

    private static int ResolveEffectiveMaxImprovementCandidates(int configuredMaxAttempts, int? tierMaxAttemptsCap)
    {
        if (configuredMaxAttempts <= 0)
            return tierMaxAttemptsCap ?? 0;

        return tierMaxAttemptsCap.HasValue
            ? Math.Min(configuredMaxAttempts, tierMaxAttemptsCap.Value)
            : configuredMaxAttempts;
    }

    private static int ResolveConfiguredMaxAttempts(
        DesignConversionBudgetMetadata budget)
    {
        if (!budget.MaxImprovementIterations.HasValue)
            return Math.Max(0, budget.MaxImprovementCandidates);

        var configuredIterations = budget.MaxImprovementIterations.Value;
        return IsValidConfiguredMaxImprovementIterations(configuredIterations)
            ? checked(configuredIterations + 1)
            : 0;
    }

    private static ExternalCostMeasurement EvaluateExternalCostMeasurement(
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults,
        IReadOnlyList<DesignConversionQaComparisonArtifact> comparisonArtifacts)
    {
        var externalJudgeCallCount = visualJudgeResults.Count(result =>
            result.Provider.ExternalCallExecuted);
        var externalComparisonCallCount = comparisonArtifacts.Count(IsChargeableExternalComparisonAttempt);
        var missingJudgeCostCount = visualJudgeResults.Count(result =>
            result.Provider.ExternalCallExecuted && !HasTrustedExternalCostMeasurement(result));
        var missingComparisonCostCount = comparisonArtifacts.Count(artifact =>
            IsChargeableExternalComparisonAttempt(artifact) &&
            !TryReadTrustedExternalComparisonCost(artifact).HasValue);
        var externalCallCount = externalJudgeCallCount + externalComparisonCallCount;
        var missingCount = missingJudgeCostCount + missingComparisonCostCount;
        return new ExternalCostMeasurement(
            IsComplete: missingCount == 0,
            MissingCount: missingCount,
            CostSource: missingCount > 0
                ? "unknown"
                : externalCallCount > 0
                    ? DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest
                    : "not_applicable_no_external_call");
    }

    private static bool ShouldIncludeEstimatedCost(DesignConversionVisualJudgeResult result)
    {
        if (!result.EstimatedCostUsd.HasValue)
            return false;

        return !result.Provider.ExternalCallExecuted || HasTrustedExternalCostMeasurement(result);
    }

    private static bool HasTrustedExternalCostMeasurement(DesignConversionVisualJudgeResult result)
    {
        if (!result.EstimatedCostUsd.HasValue)
            return false;

        var cost = result.EstimatedCostUsd.Value;
        if (!HasServerOwnedFixedPerRequestCostSource(result.Metadata) ||
            !HasServerOwnedFixedPerRequestCostSource(result.Provider.Metadata))
        {
            return false;
        }

        var providerHasAttemptMetadata = HasServerOwnedAttemptMetadata(result.Provider.Metadata);
        var resultHasAttemptMetadata = HasServerOwnedAttemptMetadata(result.Metadata);
        if (providerHasAttemptMetadata || resultHasAttemptMetadata)
        {
            return TryReadTrustedServerOwnedAttemptCount(
                    result.Provider.Metadata,
                    result.Metadata,
                    out var attemptCount) &&
                cost >= 0m &&
                cost <= DesignConversionExternalVisualProviderConfigIntake.MaxServerOwnedCostPerRequestUsd * attemptCount;
        }

        return cost >= 0m &&
            cost <= DesignConversionExternalVisualProviderConfigIntake.MaxServerOwnedCostPerRequestUsd &&
            HasServerOwnedFixedPerRequestCostSource(result.Metadata) &&
            HasServerOwnedFixedPerRequestCostSource(result.Provider.Metadata);
    }

    private static decimal? TryReadTrustedExternalComparisonCost(DesignConversionQaComparisonArtifact artifact)
    {
        if (!IsTrustedExternalComparisonArtifact(artifact))
            return null;

        var metadata = artifact.Metadata;
        if (!IsChargeableExternalComparisonAttempt(artifact) ||
            !TryReadDecimal(metadata, "estimated_cost_usd", out var cost) ||
            !TryReadDecimal(metadata, ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey, out var costPerAttempt) ||
            !TryReadDecimal(metadata, ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey, out var totalCost) ||
            !HasServerOwnedFixedPerRequestCostSource(metadata) ||
            !MetadataIsTruthy(metadata, "server_owned_pricing_valid") ||
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey,
                ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt) ||
            cost != totalCost)
        {
            return null;
        }

        if (!TryReadTrustedServerOwnedAttemptCount(
                metadata,
                metadata,
                out var attemptCount))
        {
            return null;
        }

        var expectedTotalCost = costPerAttempt * attemptCount;
        return costPerAttempt >= 0m &&
               costPerAttempt <= DesignConversionExternalVisualProviderConfigIntake.MaxServerOwnedCostPerRequestUsd &&
               totalCost == expectedTotalCost &&
               cost == totalCost
            ? cost
            : null;
    }

    private static bool IsServerCanonicalizedExternalComparisonArtifact(DesignConversionQaComparisonArtifact artifact)
        => artifact.Metadata is not null &&
           MetadataIsTruthy(artifact.Metadata, DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey) &&
           MetadataEquals(
                artifact.Metadata,
                "provider_kind",
                DesignConversionExternalVisualAiComparisonProvider.ProviderKind) &&
           MetadataEquals(
                artifact.Metadata,
                "comparison_provider_kind",
                DesignConversionExternalVisualAiComparisonProvider.ProviderKind);

    private static bool IsChargeableExternalComparisonAttempt(DesignConversionQaComparisonArtifact artifact)
        => IsServerCanonicalizedExternalComparisonArtifact(artifact) &&
           artifact.Metadata is not null &&
           MetadataIsTruthy(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey) &&
           MetadataEquals(
               artifact.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion) &&
           MetadataIsTruthy(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey) &&
           MetadataIsTruthy(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey) &&
           (MetadataIsTruthy(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey) ||
            MetadataIsTruthy(artifact.Metadata, "network_call_executed") ||
            MetadataIsTruthy(artifact.Metadata, "real_external_provider_executed"));

    private static bool IsTrustedExternalComparisonArtifact(DesignConversionQaComparisonArtifact artifact)
        => IsServerCanonicalizedExternalComparisonArtifact(artifact) &&
           MetadataEquals(
               artifact.Metadata,
               DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey,
               DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification) &&
           MetadataEquals(artifact.Metadata, "comparison_artifact_kind", "server_owned_external_visual_ai_receipt") &&
           MetadataIsTruthy(artifact.Metadata, "server_owned_external_visual_ai_receipt_validated") &&
           MetadataIsTruthy(artifact.Metadata, "server_owned_external_visual_ai_comparison_trust_verified") &&
           MetadataIsTruthy(artifact.Metadata, "network_call_executed") &&
           MetadataIsTruthy(artifact.Metadata, "real_external_provider_executed") &&
           MetadataIsTruthy(artifact.Metadata, "external_ai_visual_judge_executed") &&
           artifact.EvidenceRefs.Contains(
               DesignConversionExternalVisualAiReceiptValidator.ValidatedEvidenceRef,
               StringComparer.Ordinal);

    private static bool HasServerOwnedFixedPerRequestCostSource(IReadOnlyDictionary<string, string>? metadata)
        => metadata is not null &&
           metadata.TryGetValue("cost_source", out var source) &&
           string.Equals(
               source,
                DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                StringComparison.Ordinal);

    private static bool MetadataIsTruthy(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           IsTruthy(value);

    private static bool MetadataEquals(IReadOnlyDictionary<string, string>? metadata, string key, string expected)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static bool TryReadDecimal(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        out decimal value)
    {
        value = 0m;
        return metadata is not null &&
               metadata.TryGetValue(key, out var text) &&
               decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static bool HasServerOwnedAttemptMetadata(IReadOnlyDictionary<string, string>? metadata)
        => metadata is not null &&
           (metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey) ||
            metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey));

    private static bool TryReadTrustedServerOwnedAttemptCount(
        IReadOnlyDictionary<string, string>? providerMetadata,
        IReadOnlyDictionary<string, string>? resultMetadata,
        out int attemptCount)
    {
        attemptCount = 0;
        if (providerMetadata is null ||
            resultMetadata is null ||
            !providerMetadata.TryGetValue(ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey, out var providerAttemptCount) ||
            !resultMetadata.TryGetValue(ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey, out var resultAttemptCount) ||
            !providerMetadata.TryGetValue(ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey, out var providerAttemptSource) ||
            !resultMetadata.TryGetValue(ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey, out var resultAttemptSource))
        {
            return false;
        }

        if (!string.Equals(providerAttemptCount, resultAttemptCount, StringComparison.Ordinal) ||
            !string.Equals(providerAttemptSource, resultAttemptSource, StringComparison.Ordinal) ||
            !string.Equals(
                providerAttemptSource,
                ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(providerAttemptCount, NumberStyles.None, CultureInfo.InvariantCulture, out attemptCount))
            return false;

        return attemptCount >= 1 &&
            attemptCount <= ExternalHttpDesignConversionVisualJudge.MaxServerOwnedHttpAttemptCount;
    }

    private static string BudgetStopSource(
        string stopReason,
        decimal estimatedCost,
        decimal budgetUsd,
        int attemptCount,
        int maxAttempts)
    {
        if (!string.Equals(stopReason, BudgetCapReached, StringComparison.Ordinal))
            return "none";

        var costCapReached = estimatedCost >= budgetUsd;
        var iterationCapReached = attemptCount >= maxAttempts;
        if (costCapReached && iterationCapReached)
            return "cost_and_iteration_budget";
        if (costCapReached)
            return "cost_budget";
        if (iterationCapReached)
            return "iteration_budget";
        return "budget";
    }

    private static IReadOnlyList<string> NormalizeResolutionIds(IReadOnlyList<RenderQaResolution> resolutions)
        => resolutions
            .Where(resolution => resolution is not null)
            .Select(CanonicalResolutionId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string CanonicalResolutionId(RenderQaResolution resolution)
    {
        var sourceId = $"source-{resolution.Width.ToString(CultureInfo.InvariantCulture)}x{resolution.Height.ToString(CultureInfo.InvariantCulture)}";
        if (string.IsNullOrWhiteSpace(resolution.ResolutionId))
            return sourceId;

        var trimmedId = resolution.ResolutionId.Trim();
        var dimensionId = $"{resolution.Width.ToString(CultureInfo.InvariantCulture)}x{resolution.Height.ToString(CultureInfo.InvariantCulture)}";
        var dimensionScaleId = $"{dimensionId}@{resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)}";
        if (string.Equals(trimmedId, sourceId, StringComparison.Ordinal) ||
            string.Equals(trimmedId, dimensionId, StringComparison.Ordinal) ||
            string.Equals(trimmedId, dimensionScaleId, StringComparison.Ordinal))
        {
            return sourceId;
        }

        return trimmedId;
    }

    private static bool IsTruthy(string value)
        => string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "1", StringComparison.Ordinal)
           || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);

    private static bool IsExternalProviderCallFailed(DesignConversionVisualJudgeResult result)
        => result.IsBlocked &&
           string.Equals(
               result.Status,
               ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked,
               StringComparison.Ordinal) &&
           result.Provider.ExternalCallExecuted &&
           result.Provider.Metadata.TryGetValue("network_call_executed", out var networkCallExecuted) &&
           IsTruthy(networkCallExecuted);

    private static bool HasExternalProviderNotConfiguredResult(
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults)
        => visualJudgeResults.Count == 0 ||
           visualJudgeResults.Any(result => result.IsBlocked && !IsExternalProviderCallFailed(result));

    private static string BoolToken(bool value)
        => value ? "true" : "false";

    private sealed record ExternalCostMeasurement(
        bool IsComplete,
        int MissingCount,
        string CostSource);

    private sealed record QualityThresholdResolution(
        double Value,
        string Source,
        string PolicyStatus,
        string PolicyMarker,
        bool PendingLubaConfirmation);
}

public sealed record DesignConversionViewportCoverage(
    bool IsMultiResolution,
    bool IsComplete,
    IReadOnlyList<string> RequiredViewportIds,
    IReadOnlyList<string> ObservedViewportIds,
    IReadOnlyList<string> MissingViewportIds,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionTierPolicyDecision(
    string Tier,
    double RequiredScore,
    double BestScore,
    decimal EstimatedCostUsd,
    decimal BudgetUsd,
    decimal RemainingBudgetUsd,
    int AttemptCount,
    int MaxImprovementCandidates,
    string StopReason,
    bool ShouldContinue,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata,
    string RetryMode = DesignConversionVisualQaBoundaryPolicy.RetryModeNone,
    bool ExternalCostMeasurementComplete = true,
    int MissingExternalCostMeasurementCount = 0,
    string CostSource = "unknown")
{
    public double AcceptanceScore { get; init; } = BestScore;
}

public sealed record DesignConversionVisualCorrectionFinding(
    string FindingId,
    string Category,
    string Severity,
    string Description,
    string SuggestedAction,
    IReadOnlyList<string> EvidenceImageIds,
    bool CanDriveRetry);

public sealed record DesignConversionVisualComparisonDecision(
    string Decision,
    double QualityScore,
    double RequiredThreshold,
    decimal EstimatedCostUsd,
    decimal BudgetUsd,
    decimal RemainingBudgetUsd,
    bool CostBudgetMet,
    bool QualityThresholdMet,
    bool CanRetry,
    bool CanPersistTrainingCandidate,
    string ProviderBoundaryStatus,
    bool ExternalAiVisualJudgeExecuted,
    IReadOnlyList<DesignConversionVisualCorrectionFinding> CorrectionFindings,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionVisualQaBoundaryTrace(
    DesignConversionViewportCoverage MultiResolutionRender,
    string VisualJudgeStatus,
    string VisualJudgeProviderBoundary,
    bool ExternalAiVisualJudgeExecuted,
    DesignConversionTierPolicyDecision TierPolicy,
    DesignConversionVisualComparisonDecision VisualDecision,
    bool ProductReady,
    bool TrainedFeatureClaimed,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings);
