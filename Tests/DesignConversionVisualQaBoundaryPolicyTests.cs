// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using DesignConversionDomain;

public sealed class DesignConversionVisualQaBoundaryPolicyTests
{
    [Fact]
    public void Evaluate_MissingViewportFailsMultiResolutionEvidence()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop")],
            [JudgeResult("judge-desktop", score: 0.92, passed: true, blocked: false, cost: 0.01m)]);

        Assert.True(trace.MultiResolutionRender.IsMultiResolution);
        Assert.False(trace.MultiResolutionRender.IsComplete);
        Assert.Equal(["mobile"], trace.MultiResolutionRender.MissingViewportIds);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, trace.TierPolicy.StopReason);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeRetrySameCandidate, trace.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeRetrySameCandidate, trace.TierPolicy.Metadata["retry_mode"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine, trace.VisualDecision.Decision);
        Assert.False(trace.VisualDecision.CanRetry);
        Assert.Contains(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete &&
            finding.Severity == "blocker");
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.MultiResolutionRenderMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void EvaluateViewportCoverage_CompletenessDoesNotRequireMultiResolution()
    {
        var zeroRequired = DesignConversionVisualQaBoundaryPolicy.EvaluateViewportCoverage(
            [],
            []);
        var oneRequiredZeroObserved = DesignConversionVisualQaBoundaryPolicy.EvaluateViewportCoverage(
            [RequiredViewport("desktop")],
            []);
        var oneRequiredOneObserved = DesignConversionVisualQaBoundaryPolicy.EvaluateViewportCoverage(
            [RequiredViewport("desktop")],
            [RenderResult("desktop")]);
        var twoRequiredOneObserved = DesignConversionVisualQaBoundaryPolicy.EvaluateViewportCoverage(
            RequiredViewports(),
            [RenderResult("desktop")]);
        var twoRequiredTwoObserved = DesignConversionVisualQaBoundaryPolicy.EvaluateViewportCoverage(
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")]);

        Assert.False(zeroRequired.IsMultiResolution);
        Assert.False(zeroRequired.IsComplete);
        Assert.Empty(zeroRequired.RequiredViewportIds);
        Assert.Empty(zeroRequired.MissingViewportIds);

        Assert.False(oneRequiredZeroObserved.IsMultiResolution);
        Assert.False(oneRequiredZeroObserved.IsComplete);
        Assert.Equal(["desktop"], oneRequiredZeroObserved.MissingViewportIds);

        Assert.False(oneRequiredOneObserved.IsMultiResolution);
        Assert.True(oneRequiredOneObserved.IsComplete);
        Assert.Empty(oneRequiredOneObserved.MissingViewportIds);

        Assert.True(twoRequiredOneObserved.IsMultiResolution);
        Assert.False(twoRequiredOneObserved.IsComplete);
        Assert.Equal(["mobile"], twoRequiredOneObserved.MissingViewportIds);

        Assert.True(twoRequiredTwoObserved.IsMultiResolution);
        Assert.True(twoRequiredTwoObserved.IsComplete);
        Assert.Empty(twoRequiredTwoObserved.MissingViewportIds);
    }

    [Fact]
    public void EvaluateViewportCoverage_NormalizesGeneratedSourceViewportAliasesByDimensions()
    {
        var required = new RenderQaResolution("source-1440x900", 1440, 900, 1.0);
        var observed = new DesignConversionRenderResult(
            RenderResultId: "render-1440",
            RenderRequestId: "render-request-1440",
            Image: new RenderQaImageRecord(
                ImageId: "actual-1440",
                Label: "actual-render",
                Resolution: new RenderQaResolution("1440x900@1", 1440, 900, 1.0),
                Uri: "artifact://actual-1440.png",
                Sha256: "sha256:actual-1440",
                Metadata: new Dictionary<string, string>()),
            ElapsedMs: 1,
            Warnings: []);

        var coverage = DesignConversionVisualQaBoundaryPolicy.EvaluateViewportCoverage(
            [required],
            [observed]);

        Assert.True(coverage.IsComplete);
        Assert.Equal(["source-1440x900"], coverage.RequiredViewportIds);
        Assert.Equal(["source-1440x900"], coverage.ObservedViewportIds);
        Assert.Empty(coverage.MissingViewportIds);
    }

    [Fact]
    public void Evaluate_OneRequiredViewportCompleteProceedsPastRenderManifestGate()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            [RequiredViewport("desktop")],
            [RenderResult("desktop")],
            [JudgeResult("single-desktop-low-score", score: 0.82, passed: false, blocked: false, cost: 0.02m)]);

        Assert.False(trace.MultiResolutionRender.IsMultiResolution);
        Assert.True(trace.MultiResolutionRender.IsComplete);
        Assert.Empty(trace.MultiResolutionRender.MissingViewportIds);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, trace.TierPolicy.StopReason);
        Assert.NotEqual(DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, trace.TierPolicy.StopReason);
        Assert.True(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeImproveCandidate, trace.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, trace.VisualDecision.Decision);
        Assert.False(trace.ProductReady);
        Assert.False(trace.TrainedFeatureClaimed);
    }

    [Fact]
    public void Evaluate_ProviderUnavailableStopsAsProviderRequiredWithoutAiClaim()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("free", maxAttempts: 1, budgetUsd: 0.01m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("blocked", score: 0.0, passed: false, blocked: true, cost: null)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, trace.VisualJudgeStatus);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, trace.VisualJudgeProviderBoundary);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeNone, trace.TierPolicy.RetryMode);
        Assert.False(trace.ExternalAiVisualJudgeExecuted);
        Assert.False(trace.ProductReady);
        Assert.False(trace.TrainedFeatureClaimed);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, trace.VisualDecision.ProviderBoundaryStatus);
        Assert.False(trace.VisualDecision.QualityThresholdMet);
        Assert.False(trace.VisualDecision.CanRetry);
        Assert.Contains(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker &&
            finding.Severity == "blocker");
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, trace.EvidenceRefs);
        Assert.Contains("VISUAL_JUDGE_PROVIDER_REQUIRED_EXTERNAL_PROVIDER_NOT_CONFIGURED", trace.Warnings);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker, trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedWarning, trace.Warnings);
        Assert.DoesNotContain(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker);
    }

    [Fact]
    public void Evaluate_TerminalHttpBlockedUsesProviderCallFailedTruthWithoutNotConfiguredClaim()
    {
        var httpBlocked = HttpBlockedJudgeResult();

        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("free", maxAttempts: 1, budgetUsd: 0.01m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [httpBlocked]);

        Assert.False(httpBlocked.Verdict.Passed);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked, trace.VisualJudgeStatus);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker,
            trace.VisualJudgeProviderBoundary);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker,
            trace.VisualDecision.ProviderBoundaryStatus);
        Assert.True(trace.ExternalAiVisualJudgeExecuted);
        Assert.True(trace.VisualDecision.ExternalAiVisualJudgeExecuted);
        Assert.False(trace.VisualDecision.QualityThresholdMet);
        Assert.False(trace.VisualDecision.CanRetry);
        Assert.False(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.Equal(0.0084m, trace.TierPolicy.EstimatedCostUsd);
        Assert.True(trace.TierPolicy.ExternalCostMeasurementComplete);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker, trace.EvidenceRefs);
        Assert.Contains("external_ai_visual_judge_executed:true", trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedWarning, trace.Warnings);
        Assert.DoesNotContain("VISUAL_JUDGE_PROVIDER_REQUIRED_EXTERNAL_PROVIDER_NOT_CONFIGURED", trace.Warnings);
        Assert.Contains(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker &&
            finding.Severity == "blocker");
        Assert.DoesNotContain(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker);
    }

    [Fact]
    public void Evaluate_LocalVisualLabelerIsExplicitNonProviderBoundary()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 2, budgetUsd: 0.10m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [LocalJudgeResult("local-judge-high-score", score: 0.91, passed: true)]);

        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, trace.VisualJudgeStatus);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, trace.VisualJudgeProviderBoundary);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, trace.TierPolicy.StopReason);
        Assert.False(trace.ExternalAiVisualJudgeExecuted);
        Assert.False(trace.ProductReady);
        Assert.False(trace.TrainedFeatureClaimed);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.True(trace.VisualDecision.CostBudgetMet);
        Assert.False(trace.VisualDecision.CanRetry);
        Assert.True(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.Equal(0.10m, trace.VisualDecision.RemainingBudgetUsd);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_ExternalHighScoreWithMissingCostBlocksBeforeQualityAcceptance()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [ExternalJudgeResult("external-high-score-missing-cost", score: 0.94, passed: true, cost: null)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.TierPolicy.StopReason);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeNone, trace.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.False(trace.VisualDecision.CostBudgetMet);
        Assert.False(trace.VisualDecision.CanRetry);
        Assert.False(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.Equal("false", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("true", trace.TierPolicy.Metadata["external_visual_cost_measurement_missing"]);
        Assert.Equal("1", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.TierPolicy.Metadata["cost_source"]);
        Assert.Equal("false", trace.VisualDecision.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("1", trace.VisualDecision.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.VisualDecision.Metadata["cost_source"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
        Assert.Contains(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker &&
            finding.Severity == "blocker");
    }

    [Fact]
    public void Evaluate_ExternalHighScoreWithExplicitZeroCostAcceptsWithinBudget()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [ExternalJudgeResult("external-high-score-zero-cost", score: 0.94, passed: true, cost: 0m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, trace.TierPolicy.StopReason);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeNone, trace.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.True(trace.VisualDecision.CostBudgetMet);
        Assert.True(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.Equal(0m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal("true", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("false", trace.TierPolicy.Metadata["external_visual_cost_measurement_missing"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            trace.TierPolicy.Metadata["cost_source"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            trace.VisualDecision.Metadata["cost_source"]);
        Assert.Contains(
            $"cost_source:{DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest}",
            trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_ExternalMultiAttemptServerOwnedCostTrustsMatchingAttemptMetadata()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 20m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [
                ExternalJudgeResult(
                    "external-multi-attempt-server-owned-cost",
                    score: 0.94,
                    passed: true,
                    cost: 15m,
                    providerAttemptCount: "2",
                    resultAttemptCount: "2",
                    providerAttemptSource: ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
                    resultAttemptSource: ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue)
            ]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, trace.VisualDecision.Decision);
        Assert.Equal(15m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal(5m, trace.TierPolicy.RemainingBudgetUsd);
        Assert.Equal("true", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            trace.TierPolicy.Metadata["cost_source"]);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_ExternalMultiAttemptServerOwnedCostRejectsMalformedOrForgedAttemptMetadata()
    {
        var malformed = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 20m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [
                ExternalJudgeResult(
                    "external-malformed-attempt-count",
                    score: 0.94,
                    passed: true,
                    cost: 15m,
                    providerAttemptCount: "2",
                    resultAttemptCount: "two",
                    providerAttemptSource: ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
                    resultAttemptSource: ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue)
            ]);
        var forged = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 20m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [
                ExternalJudgeResult(
                    "external-forged-attempt-source",
                    score: 0.94,
                    passed: true,
                    cost: 15m,
                    providerAttemptCount: "2",
                    resultAttemptCount: "2",
                    providerAttemptSource: "client_supplied",
                    resultAttemptSource: ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue)
            ]);

        AssertMalformedAttemptMetadataBlocksCostCompleteness(malformed);
        AssertMalformedAttemptMetadataBlocksCostCompleteness(forged);
    }

    [Fact]
    public void Evaluate_ExternalHighScoreWithProviderReportedCostSourceBlocksBeforeQualityAcceptance()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [
                ExternalJudgeResult(
                    "external-high-score-provider-reported-cost",
                    score: 0.94,
                    passed: true,
                    cost: 0.04m,
                    providerCostSource: "provider_reported",
                    resultCostSource: "provider_reported")
            ]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.TierPolicy.StopReason);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeNone, trace.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.False(trace.VisualDecision.CostBudgetMet);
        Assert.False(trace.VisualDecision.CanRetry);
        Assert.False(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.False(trace.TrainedFeatureClaimed);
        Assert.Equal(0m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal("false", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("1", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.TierPolicy.Metadata["cost_source"]);
        Assert.Equal("false", trace.VisualDecision.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("1", trace.VisualDecision.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.VisualDecision.Metadata["cost_source"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_ExternalHighScoreWithNegativeTrustedSourceCostBlocksAndExcludesCost()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [ExternalJudgeResult("external-high-score-negative-cost", score: 0.94, passed: true, cost: -0.04m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.False(trace.VisualDecision.CostBudgetMet);
        Assert.False(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.Equal(0m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal(0m, trace.VisualDecision.EstimatedCostUsd);
        Assert.Equal("0", trace.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal("false", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("1", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.TierPolicy.Metadata["cost_source"]);
        Assert.Equal("false", trace.VisualDecision.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("1", trace.VisualDecision.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.VisualDecision.Metadata["cost_source"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_TrustedExternalComparisonAndJudgeCostsAreSummedBeforeBudgetDecision()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 0.03m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [ExternalJudgeResult("external-low-score-trusted-cost", score: 0.70, passed: false, cost: 0.02m)],
            comparisonArtifacts: [TrustedExternalComparisonArtifact("trusted-comparison-cost", cost: 0.02m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, trace.TierPolicy.StopReason);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeNone, trace.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        Assert.False(trace.VisualDecision.QualityThresholdMet);
        Assert.False(trace.VisualDecision.CostBudgetMet);
        Assert.False(trace.VisualDecision.CanRetry);
        Assert.False(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.Equal(0.04m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal(0m, trace.TierPolicy.RemainingBudgetUsd);
        Assert.Equal("0.04", trace.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["remaining_budget_usd"]);
        Assert.Equal("true", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            trace.TierPolicy.Metadata["cost_source"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostCapMarker, trace.EvidenceRefs);
        Assert.Contains(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.BudgetCapReached &&
            finding.Severity == "blocker");
    }

    [Fact]
    public void Evaluate_ForgedLocalComparisonEstimatedCostIsIgnored()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 0.03m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [ExternalJudgeResult("external-low-score-real-cost", score: 0.70, passed: false, cost: 0.02m)],
            comparisonArtifacts: [ForgedLocalComparisonArtifact("forged-local-comparison-cost", cost: 9.99m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, trace.TierPolicy.StopReason);
        Assert.True(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeImproveCandidate, trace.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.CostBudgetMet);
        Assert.True(trace.VisualDecision.CanRetry);
        Assert.Equal(0.02m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal(0.01m, trace.TierPolicy.RemainingBudgetUsd);
        Assert.Equal("0.02", trace.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal("0.01", trace.TierPolicy.Metadata["remaining_budget_usd"]);
        Assert.Equal("true", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_ServerCanonicalizedExternalComparisonNoCallDoesNotRequireOrAddCost()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 0.03m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [LocalJudgeResult("local-high-score-with-no-call-comparison", score: 0.94, passed: true)],
            comparisonArtifacts:
            [
                NoCallCanonicalExternalComparisonArtifact(
                    "canonical-external-provider-not-configured",
                    estimatedCost: 9.99m)
            ]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.True(trace.VisualDecision.CostBudgetMet);
        Assert.True(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.Equal(0m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal(0.03m, trace.TierPolicy.RemainingBudgetUsd);
        Assert.Equal("0", trace.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal("0.03", trace.TierPolicy.Metadata["remaining_budget_usd"]);
        Assert.Equal("true", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("not_applicable_no_external_call", trace.TierPolicy.Metadata["cost_source"]);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_TrustedExternalComparisonArithmeticMismatchFailsClosed()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [LocalJudgeResult("local-high-score-with-mismatched-comparison-cost", score: 0.94, passed: true)],
            comparisonArtifacts:
            [
                TrustedExternalComparisonArtifact(
                    "trusted-comparison-mismatched-cost",
                    cost: 0.02m,
                    mutateMetadata: metadata =>
                    {
                        metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = "2";
                        metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey] = "0.02";
                        metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey] = "0.02";
                    })
            ]);

        AssertMalformedExternalComparisonCostBlocksCostCompleteness(trace);
    }

    [Fact]
    public void Evaluate_TrustedExternalComparisonMalformedPerAttemptCostFailsClosed()
    {
        var malformed = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [LocalJudgeResult("local-high-score-with-malformed-comparison-cost", score: 0.94, passed: true)],
            comparisonArtifacts:
            [
                TrustedExternalComparisonArtifact(
                    "trusted-comparison-malformed-cost",
                    cost: 0.02m,
                    mutateMetadata: metadata =>
                    {
                        metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey] = "not-decimal";
                    })
            ]);
        var negativePerAttempt = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [LocalJudgeResult("local-high-score-with-negative-per-attempt-comparison-cost", score: 0.94, passed: true)],
            comparisonArtifacts:
            [
                TrustedExternalComparisonArtifact(
                    "trusted-comparison-negative-per-attempt-cost",
                    cost: 0.02m,
                    mutateMetadata: metadata =>
                    {
                        metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey] = "-0.01";
                    })
            ]);

        AssertMalformedExternalComparisonCostBlocksCostCompleteness(malformed);
        AssertMalformedExternalComparisonCostBlocksCostCompleteness(negativePerAttempt);
    }

    [Fact]
    public void Evaluate_TrustedExternalComparisonCostAboveMaxServerOwnedCostPerRequestUsdFailsClosed()
    {
        var overMaxCost = DesignConversionExternalVisualProviderConfigIntake.MaxServerOwnedCostPerRequestUsd + 0.01m;
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 100m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [LocalJudgeResult("local-high-score-with-over-max-comparison-cost", score: 0.94, passed: true)],
            comparisonArtifacts:
            [
                TrustedExternalComparisonArtifact(
                    "trusted-comparison-over-max-per-attempt-cost",
                    cost: overMaxCost)
            ]);

        AssertMalformedExternalComparisonCostBlocksCostCompleteness(trace);
    }

    [Fact]
    public void Evaluate_TrustedExternalComparisonMalformedCostFailsClosed()
    {
        var missingCost = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [LocalJudgeResult("local-high-score-with-missing-comparison-cost", score: 0.94, passed: true)],
            comparisonArtifacts: [TrustedExternalComparisonArtifact("trusted-comparison-missing-cost", cost: null)]);
        var negativeCost = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [LocalJudgeResult("local-high-score-with-negative-comparison-cost", score: 0.94, passed: true)],
            comparisonArtifacts:
            [
                TrustedExternalComparisonArtifact(
                    "trusted-comparison-negative-cost",
                    cost: -0.01m)
            ]);

        AssertMalformedExternalComparisonCostBlocksCostCompleteness(missingCost);
        AssertMalformedExternalComparisonCostBlocksCostCompleteness(negativeCost);
    }

    [Fact]
    public void Evaluate_LocalAndTestNullCostDoesNotRequireExternalCostMeasurement()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 2, budgetUsd: 0.10m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [
                LocalJudgeResult("local-null-cost", score: 0.91, passed: true, cost: null),
                JudgeResult("test-null-cost", score: 0.92, passed: true, blocked: false, cost: null)
            ]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, trace.TierPolicy.StopReason);
        Assert.NotEqual(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.CostBudgetMet);
        Assert.True(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.False(trace.ExternalAiVisualJudgeExecuted);
        Assert.Equal("true", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("not_applicable_no_external_call", trace.TierPolicy.Metadata["cost_source"]);
        Assert.Equal("not_applicable_no_external_call", trace.VisualDecision.Metadata["cost_source"]);
        Assert.Contains("cost_source:not_applicable_no_external_call", trace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_TierPolicyStopsAtBudgetOrQualityAndContinuesWhenSafe()
    {
        var freeBudgetCap = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("free", maxAttempts: 3, budgetUsd: 0.01m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("free-low-score", score: 0.74, passed: false, blocked: false, cost: 0.01m)]);
        var proContinue = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("pro-low-score", score: 0.82, passed: false, blocked: false, cost: 0.02m)]);
        var proQualityReached = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("pro-high-score", score: 0.94, passed: true, blocked: false, cost: 0.02m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, freeBudgetCap.TierPolicy.StopReason);
        Assert.False(freeBudgetCap.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeNone, freeBudgetCap.TierPolicy.RetryMode);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostCapMarker, freeBudgetCap.TierPolicy.EvidenceRefs);
        Assert.Equal("0.01", freeBudgetCap.TierPolicy.Metadata["cost_cap"]);
        Assert.Equal("0.75", freeBudgetCap.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, freeBudgetCap.VisualDecision.Decision);
        Assert.Equal(0m, freeBudgetCap.VisualDecision.RemainingBudgetUsd);
        Assert.Contains(freeBudgetCap.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.BudgetCapReached);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, proContinue.TierPolicy.StopReason);
        Assert.True(proContinue.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeImproveCandidate, proContinue.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, proContinue.VisualDecision.Decision);
        Assert.True(proContinue.VisualDecision.CanRetry);
        Assert.Equal(0.98m, proContinue.VisualDecision.RemainingBudgetUsd);
        Assert.Contains(proContinue.VisualDecision.CorrectionFindings, finding =>
            finding.Category == "layout" &&
            finding.CanDriveRetry);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, proQualityReached.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeNone, proQualityReached.TierPolicy.RetryMode);
        Assert.Equal("0.9", proQualityReached.TierPolicy.Metadata["quality_threshold"]);
        Assert.False(proQualityReached.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, proQualityReached.VisualDecision.Decision);
        Assert.True(proQualityReached.VisualDecision.QualityThresholdMet);
        Assert.All(
            [freeBudgetCap, proContinue, proQualityReached],
            trace =>
            {
                Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierPolicyMarker, trace.EvidenceRefs);
                Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker, trace.EvidenceRefs);
                Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, trace.EvidenceRefs);
                Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, trace.EvidenceRefs);
                Assert.False(trace.ProductReady);
            });
    }

    [Fact]
    public void Evaluate_MixedViewportScoresUseWorstScoreForAcceptanceAndRetryWhenBudgetRemains()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 1.0m, qualityThreshold: 0.8),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [
                JudgeResult("desktop-high-score", score: 0.95, passed: true, blocked: false, cost: 0.01m),
                JudgeResult("mobile-low-score", score: 0.30, passed: true, blocked: false, cost: 0.01m)
            ]);

        Assert.Equal(0.95, trace.TierPolicy.BestScore, precision: 6);
        Assert.Equal(0.30, trace.TierPolicy.AcceptanceScore, precision: 6);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, trace.TierPolicy.StopReason);
        Assert.True(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeImproveCandidate, trace.TierPolicy.RetryMode);
        Assert.NotEqual(DesignConversionVisualQaBoundaryPolicy.QualityReached, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, trace.VisualDecision.Decision);
        Assert.NotEqual(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, trace.VisualDecision.Decision);
        Assert.Equal(0.30, trace.VisualDecision.QualityScore, precision: 6);
        Assert.False(trace.VisualDecision.QualityThresholdMet);
        Assert.True(trace.VisualDecision.CanRetry);
        Assert.Contains(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == "quality_threshold_not_met" &&
            finding.CanDriveRetry);
    }

    [Fact]
    public void Evaluate_AllViewportScoresAboveThresholdAcceptsUsingLowerAcceptanceScore()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 1.0m, qualityThreshold: 0.8),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [
                JudgeResult("desktop-high-score", score: 0.95, passed: true, blocked: false, cost: 0.01m),
                JudgeResult("mobile-above-threshold-score", score: 0.84, passed: true, blocked: false, cost: 0.01m)
            ]);

        Assert.Equal(0.95, trace.TierPolicy.BestScore, precision: 6);
        Assert.Equal(0.84, trace.TierPolicy.AcceptanceScore, precision: 6);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, trace.TierPolicy.StopReason);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, trace.VisualDecision.Decision);
        Assert.Equal(0.84, trace.VisualDecision.QualityScore, precision: 6);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.False(trace.VisualDecision.CanRetry);
    }

    [Fact]
    public void Evaluate_MetadataAndEvidenceDocumentMinimumScoreAggregation()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("internal", maxAttempts: 3, budgetUsd: 1.0m, qualityThreshold: 0.8),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [
                JudgeResult("desktop-high-score", score: 0.95, passed: true, blocked: false, cost: 0.01m),
                JudgeResult("mobile-low-score", score: 0.30, passed: true, blocked: false, cost: 0.01m)
            ]);

        Assert.Equal("minimum_all_visual_judge_results", trace.TierPolicy.Metadata["quality_score_aggregation"]);
        Assert.Equal("0.95", trace.TierPolicy.Metadata["best_visual_score"]);
        Assert.Equal("0.3", trace.TierPolicy.Metadata["acceptance_visual_score"]);
        Assert.Equal("0.3", trace.TierPolicy.Metadata["worst_visual_score"]);
        Assert.Equal("0.3", trace.VisualDecision.Metadata["quality_score"]);
        Assert.Equal("minimum_all_visual_judge_results", trace.VisualDecision.Metadata["quality_score_aggregation"]);
        Assert.Equal("0.95", trace.VisualDecision.Metadata["best_visual_score"]);
        Assert.Equal("0.3", trace.VisualDecision.Metadata["acceptance_visual_score"]);
        Assert.Equal("0.3", trace.VisualDecision.Metadata["worst_visual_score"]);
        Assert.Contains("tier_policy:quality_score_aggregation=minimum_all_visual_judge_results", trace.EvidenceRefs);
        Assert.Contains("tier_policy:best_visual_score=0.95", trace.EvidenceRefs);
        Assert.Contains("tier_policy:acceptance_visual_score=0.3", trace.EvidenceRefs);
        Assert.Contains("tier_policy:worst_visual_score=0.3", trace.EvidenceRefs);
        Assert.Contains("visual_decision:quality_score_aggregation=minimum_all_visual_judge_results", trace.EvidenceRefs);
        Assert.Contains("visual_decision:best_visual_score=0.95", trace.EvidenceRefs);
        Assert.Contains("visual_decision:acceptance_visual_score=0.3", trace.EvidenceRefs);
        Assert.Contains("visual_decision:worst_visual_score=0.3", trace.EvidenceRefs);
    }

    [Fact]
    public void Evaluate_SameSubmittedBudgetUsesDifferentTierCapsForLoopContinuation()
    {
        var free = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("free", maxAttempts: 10, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("free-low-score", score: 0.70, passed: false, blocked: false, cost: 0.04m)]);
        var paid = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("paid", maxAttempts: 10, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("paid-low-score", score: 0.70, passed: false, blocked: false, cost: 0.04m)]);
        var pro = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 10, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("pro-low-score", score: 0.70, passed: false, blocked: false, cost: 0.04m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, free.TierPolicy.StopReason);
        Assert.False(free.TierPolicy.ShouldContinue);
        // S4.5 governing plan defaults are provisional pending Luba confirmation:
        // free 0.75, paid 0.85, pro 0.90, dev 0.80.
        Assert.Equal("0.75", free.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal("0.05", free.TierPolicy.Metadata["effective_budget_usd"]);
        Assert.Equal("1", free.TierPolicy.Metadata["effective_max_improvement_candidates"]);
        Assert.Equal("iteration_budget", free.TierPolicy.Metadata["budget_stop_source"]);
        Assert.Equal(bool.TrueString, free.TierPolicy.Metadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.TrueString, free.TierPolicy.Metadata["tier_policy_iteration_cap_applied"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostCapMarker, free.TierPolicy.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierBudgetCapAppliedMarker, free.TierPolicy.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierIterationCapAppliedMarker, free.TierPolicy.EvidenceRefs);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, paid.TierPolicy.StopReason);
        Assert.True(paid.TierPolicy.ShouldContinue);
        Assert.Equal("0.85", paid.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal("0.25", paid.TierPolicy.Metadata["effective_budget_usd"]);
        Assert.Equal("2", paid.TierPolicy.Metadata["effective_max_improvement_candidates"]);
        Assert.Equal(bool.TrueString, paid.TierPolicy.Metadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.TrueString, paid.TierPolicy.Metadata["tier_policy_iteration_cap_applied"]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, pro.TierPolicy.StopReason);
        Assert.True(pro.TierPolicy.ShouldContinue);
        Assert.Equal("0.9", pro.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal("1", pro.TierPolicy.Metadata["effective_budget_usd"]);
        Assert.Equal("4", pro.TierPolicy.Metadata["effective_max_improvement_candidates"]);
        Assert.Equal(bool.FalseString, pro.TierPolicy.Metadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.TrueString, pro.TierPolicy.Metadata["tier_policy_iteration_cap_applied"]);

        Assert.All([free, paid, pro], trace =>
        {
            Assert.Equal("10", trace.TierPolicy.Metadata["submitted_max_improvement_candidates"]);
            Assert.Equal("1", trace.TierPolicy.Metadata["submitted_budget_usd"]);
            Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["product_ready"]);
            Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["trained_feature_claimed"]);
            Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierPolicyMarker, trace.EvidenceRefs);
            Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker, trace.EvidenceRefs);
            Assert.DoesNotContain("PRIVATE", string.Join("|", trace.EvidenceRefs), StringComparison.OrdinalIgnoreCase);
        });
    }

    [Theory]
    [InlineData("free", "0.05", "1", DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, false, "iteration_budget")]
    [InlineData("paid", "0.25", "2", DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, true, "none")]
    [InlineData("pro", "1", "4", DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, true, "none")]
    public void Evaluate_KnownTierZeroSubmissionUsesBudgetAndIterationEntitlementDefaults(
        string tier,
        string expectedBudgetUsd,
        string expectedMaxAttempts,
        string expectedStopReason,
        bool expectedShouldContinue,
        string expectedBudgetStopSource)
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget(tier, maxAttempts: 0, budgetUsd: 0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult($"{tier}-zero-submission-low-score", score: 0.70, passed: false, blocked: false, cost: 0.01m)]);

        Assert.Equal(expectedStopReason, trace.TierPolicy.StopReason);
        Assert.Equal(expectedShouldContinue, trace.TierPolicy.ShouldContinue);
        Assert.Equal(decimal.Parse(expectedBudgetUsd, CultureInfo.InvariantCulture), trace.TierPolicy.BudgetUsd);
        Assert.Equal(expectedMaxAttempts, trace.TierPolicy.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(
            decimal.Parse(expectedBudgetUsd, CultureInfo.InvariantCulture),
            DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierBudgetUsd(0m, tier));
        Assert.Equal(
            expectedMaxAttempts,
            DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierMaxImprovementCandidates(0, tier)
                .ToString(CultureInfo.InvariantCulture));
        Assert.Equal("0", trace.TierPolicy.Metadata["submitted_budget_usd"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["submitted_max_improvement_candidates"]);
        Assert.Equal(expectedBudgetUsd, trace.TierPolicy.Metadata["effective_budget_usd"]);
        Assert.Equal(expectedMaxAttempts, trace.TierPolicy.Metadata["effective_max_improvement_candidates"]);
        Assert.NotEqual("unbounded", trace.TierPolicy.Metadata["tier_budget_cap_usd"]);
        Assert.Equal(expectedBudgetUsd, trace.TierPolicy.Metadata["tier_budget_cap_usd"]);
        Assert.Equal(expectedMaxAttempts, trace.TierPolicy.Metadata["tier_max_improvement_candidates"]);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["tier_policy_iteration_cap_applied"]);
        Assert.Equal(bool.TrueString, trace.TierPolicy.Metadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(bool.TrueString, trace.TierPolicy.Metadata["tier_policy_iteration_entitlement_default_applied"]);
        Assert.Equal(expectedBudgetStopSource, trace.TierPolicy.Metadata["budget_stop_source"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierBudgetEntitlementDefaultAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierIterationEntitlementDefaultAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:submitted_max_improvement_candidates=0", trace.TierPolicy.EvidenceRefs);
        Assert.Contains(
            $"tier_policy:effective_max_improvement_candidates={expectedMaxAttempts}",
            trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:budget_entitlement_default_applied=true", trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:iteration_entitlement_default_applied=true", trace.TierPolicy.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierBudgetCapAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierIterationCapAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["product_ready"]);

        if (expectedStopReason == DesignConversionVisualQaBoundaryPolicy.BudgetCapReached)
        {
            Assert.Contains($"tier_policy:cost_cap={expectedBudgetUsd}", trace.TierPolicy.EvidenceRefs);
            Assert.False(trace.VisualDecision.CanRetry);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        }
        else
        {
            Assert.DoesNotContain($"tier_policy:cost_cap={expectedBudgetUsd}", trace.TierPolicy.EvidenceRefs);
            Assert.True(trace.VisualDecision.CanRetry);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, trace.VisualDecision.Decision);
        }
    }

    [Fact]
    public void Evaluate_TypedImprovementIterationsIncludesBaselineAttemptBeforeTierCap()
    {
        var budget = Budget("paid", maxAttempts: 1, budgetUsd: 1.0m) with
        {
            MaxImprovementIterations = 1
        };

        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            budget,
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("typed-iteration-low-score", score: 0.70, passed: false, blocked: false, cost: 0.01m)]);

        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ContinueImprovement,
            trace.TierPolicy.StopReason);
        Assert.True(trace.TierPolicy.ShouldContinue);
        Assert.Equal(2, trace.TierPolicy.MaxImprovementCandidates);
        Assert.Equal("1", trace.TierPolicy.Metadata["submitted_max_improvement_candidates"]);
        Assert.Equal("1", trace.TierPolicy.Metadata["typed_max_improvement_iterations"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.TypedMaxImprovementIterationsSource,
            trace.TierPolicy.Metadata["max_attempts_source"]);
        Assert.Equal("2", trace.TierPolicy.Metadata["effective_max_improvement_candidates"]);
        Assert.Equal(
            bool.FalseString,
            trace.TierPolicy.Metadata["tier_policy_iteration_entitlement_default_applied"]);
    }

    [Theory]
    [InlineData("free", "0.05", "1", true, true, DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, false)]
    [InlineData("paid", "0.25", "2", true, true, DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, true)]
    [InlineData("pro", "1", "4", false, true, DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, true)]
    public void Evaluate_PositiveSubmittedValuesRemainTierCappedWithoutDefaultMarkers(
        string tier,
        string expectedBudgetUsd,
        string expectedMaxAttempts,
        bool expectedBudgetCapApplied,
        bool expectedIterationCapApplied,
        string expectedStopReason,
        bool expectedShouldContinue)
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget(tier, maxAttempts: 10, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult($"{tier}-positive-submission-low-score", score: 0.70, passed: false, blocked: false, cost: 0.04m)]);

        Assert.Equal(expectedStopReason, trace.TierPolicy.StopReason);
        Assert.Equal(expectedShouldContinue, trace.TierPolicy.ShouldContinue);
        Assert.Equal("1", trace.TierPolicy.Metadata["submitted_budget_usd"]);
        Assert.Equal("10", trace.TierPolicy.Metadata["submitted_max_improvement_candidates"]);
        Assert.Equal(expectedBudgetUsd, trace.TierPolicy.Metadata["effective_budget_usd"]);
        Assert.Equal(expectedMaxAttempts, trace.TierPolicy.Metadata["effective_max_improvement_candidates"]);
        Assert.Equal(expectedBudgetCapApplied.ToString(), trace.TierPolicy.Metadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(expectedIterationCapApplied.ToString(), trace.TierPolicy.Metadata["tier_policy_iteration_cap_applied"]);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["tier_policy_iteration_entitlement_default_applied"]);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierBudgetEntitlementDefaultAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierIterationEntitlementDefaultAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:budget_entitlement_default_applied=false", trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:iteration_entitlement_default_applied=false", trace.TierPolicy.EvidenceRefs);

        if (expectedBudgetCapApplied)
            Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierBudgetCapAppliedMarker, trace.TierPolicy.EvidenceRefs);
        else
            Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierBudgetCapAppliedMarker, trace.TierPolicy.EvidenceRefs);

        if (expectedIterationCapApplied)
            Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierIterationCapAppliedMarker, trace.TierPolicy.EvidenceRefs);
        else
            Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierIterationCapAppliedMarker, trace.TierPolicy.EvidenceRefs);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("")]
    public void Evaluate_UnknownOrUnspecifiedZeroSubmissionFailsClosedWithoutEntitlementDefault(string tier)
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget(tier, maxAttempts: 0, budgetUsd: 0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("unknown-zero-submission-low-score", score: 0.70, passed: false, blocked: false, cost: 0.01m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, trace.TierPolicy.StopReason);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.Equal("0", trace.TierPolicy.Metadata["submitted_budget_usd"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["submitted_max_improvement_candidates"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["effective_budget_usd"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["effective_max_improvement_candidates"]);
        Assert.Equal("unbounded", trace.TierPolicy.Metadata["tier_budget_cap_usd"]);
        Assert.Equal("unbounded", trace.TierPolicy.Metadata["tier_max_improvement_candidates"]);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["tier_policy_iteration_cap_applied"]);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["tier_policy_iteration_entitlement_default_applied"]);
        Assert.Contains("tier_policy:budget_entitlement_default_applied=false", trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:iteration_entitlement_default_applied=false", trace.TierPolicy.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierBudgetEntitlementDefaultAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierIterationEntitlementDefaultAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierBudgetCapAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TierIterationCapAppliedMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
    }

    [Theory]
    [InlineData("free", "0.75", DesignConversionVisualQaBoundaryPolicy.QualityReached, DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, false, true, false)]
    [InlineData("dev", "0.8", DesignConversionVisualQaBoundaryPolicy.QualityReached, DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, false, true, false)]
    [InlineData("paid", "0.85", DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, DesignConversionVisualQaBoundaryPolicy.DecisionRetry, true, false, true)]
    [InlineData("pro", "0.9", DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, DesignConversionVisualQaBoundaryPolicy.DecisionRetry, true, false, true)]
    public void Evaluate_SameVisualScoreUsesCorrectedProvisionalTierThresholdBeforeAccepting(
        string tier,
        string expectedThreshold,
        string expectedStopReason,
        string expectedDecision,
        bool expectedShouldContinue,
        bool expectedQualityThresholdMet,
        bool expectedCanRetry)
    {
        const double sameScore = 0.80;
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget(tier, maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult($"{tier}-same-score", score: sameScore, passed: true, blocked: false, cost: 0.02m)]);

        Assert.Equal(expectedStopReason, trace.TierPolicy.StopReason);
        Assert.Equal(expectedDecision, trace.VisualDecision.Decision);
        Assert.Equal(expectedShouldContinue, trace.TierPolicy.ShouldContinue);
        Assert.Equal(expectedQualityThresholdMet, trace.VisualDecision.QualityThresholdMet);
        Assert.Equal(expectedCanRetry, trace.VisualDecision.CanRetry);
        Assert.Equal(expectedThreshold, trace.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal("0.02", trace.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource,
            trace.TierPolicy.Metadata["quality_threshold_source"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdPolicyStatus,
            trace.TierPolicy.Metadata["quality_threshold_policy_status"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdPolicyMarker,
            trace.TierPolicy.Metadata["quality_threshold_policy_marker"]);
        Assert.Equal("true", trace.TierPolicy.Metadata["quality_threshold_pending_luba_confirmation"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdPolicyMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Contains($"tier_policy:quality_threshold={expectedThreshold}", trace.TierPolicy.EvidenceRefs);
        Assert.Contains(
            $"tier_policy:quality_threshold_source={DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource}",
            trace.TierPolicy.EvidenceRefs);
        Assert.Contains(
            $"tier_policy:quality_threshold_policy_status={DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdPolicyStatus}",
            trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:quality_threshold_pending_luba_confirmation=true", trace.TierPolicy.EvidenceRefs);
        Assert.True(trace.MultiResolutionRender.IsComplete);
        Assert.False(trace.ExternalAiVisualJudgeExecuted);
        Assert.False(trace.ProductReady);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["product_ready"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierPolicyMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdPolicyMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, trace.EvidenceRefs);

        if (expectedCanRetry)
        {
            Assert.Contains(trace.VisualDecision.CorrectionFindings, finding =>
                finding.Category == "quality_threshold_not_met" &&
                finding.CanDriveRetry);
        }
    }

    [Fact]
    public void Evaluate_CanonicalQualityThresholdMetadataOverrideWinsOverProvisionalTierDefault()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("paid", maxAttempts: 3, budgetUsd: 1.0m, qualityThreshold: 0.92),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("paid-override-score", score: 0.90, passed: true, blocked: false, cost: 0.02m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, trace.VisualDecision.Decision);
        Assert.True(trace.TierPolicy.ShouldContinue);
        Assert.False(trace.VisualDecision.QualityThresholdMet);
        Assert.Equal("0.92", trace.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.CanonicalQualityThresholdMetadataOverrideSource,
            trace.TierPolicy.Metadata["quality_threshold_source"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.CanonicalQualityThresholdMetadataOverrideSource,
            trace.TierPolicy.Metadata["quality_threshold_policy_status"]);
        Assert.Equal("false", trace.TierPolicy.Metadata["quality_threshold_pending_luba_confirmation"]);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdPolicyMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Contains(
            $"tier_policy:quality_threshold_source={DesignConversionVisualQaBoundaryPolicy.CanonicalQualityThresholdMetadataOverrideSource}",
            trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:quality_threshold_pending_luba_confirmation=false", trace.TierPolicy.EvidenceRefs);
        Assert.False(trace.ProductReady);
    }

    [Fact]
    public void Evaluate_TypedRenderQaPassThresholdOverrideWinsOverLegacyMetadata()
    {
        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget(
                "paid",
                maxAttempts: 3,
                budgetUsd: 1.0m,
                qualityThreshold: 0.99,
                renderQaPassThreshold: 0.92),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("paid-typed-override-score", score: 0.93, passed: true, blocked: false, cost: 0.02m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, trace.VisualDecision.Decision);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.Equal("0.92", trace.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource,
            trace.TierPolicy.Metadata["quality_threshold_source"]);
        Assert.Equal("typed_render_qa_pass_threshold_override", trace.TierPolicy.Metadata["quality_threshold_policy_status"]);
        Assert.Equal("false", trace.TierPolicy.Metadata["quality_threshold_pending_luba_confirmation"]);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdPolicyMarker, trace.TierPolicy.EvidenceRefs);
        Assert.Contains(
            $"tier_policy:quality_threshold_source={DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource}",
            trace.TierPolicy.EvidenceRefs);
        Assert.Contains("tier_policy:quality_threshold_pending_luba_confirmation=false", trace.TierPolicy.EvidenceRefs);
        Assert.False(trace.ProductReady);
        Assert.False(trace.TrainedFeatureClaimed);
    }

    [Theory]
    [InlineData("NaN", "internal", "0.85", DesignConversionVisualQaBoundaryPolicy.NonTierQualityThresholdFallbackSource)]
    [InlineData("Infinity", "internal", "0.85", DesignConversionVisualQaBoundaryPolicy.NonTierQualityThresholdFallbackSource)]
    [InlineData("2.00", "internal", "0.85", DesignConversionVisualQaBoundaryPolicy.NonTierQualityThresholdFallbackSource)]
    [InlineData("0.00", "internal", "0.85", DesignConversionVisualQaBoundaryPolicy.NonTierQualityThresholdFallbackSource)]
    [InlineData("0.10", "pro", "0.9", DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource)]
    public void Evaluate_InvalidOrLoweringLegacyQualityThresholdCannotWeakenServerBoundary(
        string configuredThreshold,
        string tier,
        string expectedThreshold,
        string expectedSource)
    {
        var budget = BudgetWithQualityThresholdText(tier, configuredThreshold);

        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            budget,
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult($"invalid-legacy-{tier}-{configuredThreshold}", score: 0.86, passed: true, blocked: false, cost: 0.02m)]);

        Assert.Equal(expectedThreshold, trace.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal(expectedSource, trace.TierPolicy.Metadata["quality_threshold_source"]);
        Assert.DoesNotContain(configuredThreshold, trace.TierPolicy.EvidenceRefs);
        if (string.Equals(tier, "pro", StringComparison.Ordinal))
            Assert.False(trace.VisualDecision.QualityThresholdMet);
    }

    [Fact]
    public async Task Evaluate_Pass78LikeRenderManifestInputsKeepProviderRequiredAndDoNotFakeVisualAi()
    {
        var required =
            new[]
            {
                new RenderQaResolution("mobile", 360, 740, 1.0),
                new RenderQaResolution("tablet", 768, 1024, 1.0),
                new RenderQaResolution("desktop", 1440, 900, 1.0)
            };
        var renderResults = required
            .Select(resolution => RenderResultFromPass78Manifest(resolution))
            .ToArray();
        var blockedJudge = await new NoConfiguredVisualJudge().JudgeAsync(new DesignConversionVisualJudgeRequest(
            JudgeRequestId: "pass78-like-visual-judge",
            RequestId: "pass78-like-request",
            ExpectedScreenshot: ExpectedImage(required[0]),
            RenderedImage: renderResults[0].Image,
            DiffArtifactUri: "artifact://pass78-like-diff.png",
            DiffArtifactSha256: "sha256:pass78-like-diff",
            Metadata: new Dictionary<string, string>
            {
                ["render_manifest_ref"] = "artifact://codex-pass78-browser-render-manifest.json",
                ["render_manifest_sha256"] = "sha256:pass78-manifest"
            }));

        var trace = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            required,
            renderResults,
            [blockedJudge.Value!]);

        Assert.True(blockedJudge.IsSuccess, blockedJudge.ErrorMessage);
        Assert.True(trace.MultiResolutionRender.IsComplete);
        Assert.Equal(["mobile", "tablet", "desktop"], trace.MultiResolutionRender.ObservedViewportIds);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, trace.VisualJudgeProviderBoundary);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, trace.TierPolicy.StopReason);
        Assert.False(trace.ExternalAiVisualJudgeExecuted);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker, trace.EvidenceRefs);
    }

    private static DesignConversionBudgetMetadata Budget(
        string tier,
        int maxAttempts,
        decimal budgetUsd,
        double? qualityThreshold = null,
        double? renderQaPassThreshold = null)
    {
        var metadata = new Dictionary<string, string>();
        if (qualityThreshold.HasValue)
            metadata["quality_threshold"] = qualityThreshold.Value.ToString("0.###", CultureInfo.InvariantCulture);

        return new DesignConversionBudgetMetadata(
            UserId: "user-redacted",
            Tier: tier,
            MaxImprovementCandidates: maxAttempts,
            MaxRenderQaRuns: 2,
            BudgetUsd: budgetUsd,
            Metadata: metadata,
            RenderQaPassThreshold: renderQaPassThreshold);
    }

    private static DesignConversionBudgetMetadata BudgetWithQualityThresholdText(
        string tier,
        string configuredThreshold)
        => new(
            UserId: "user-redacted",
            Tier: tier,
            MaxImprovementCandidates: 3,
            MaxRenderQaRuns: 2,
            BudgetUsd: 1.0m,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = configuredThreshold
            });

    private static IReadOnlyList<RenderQaResolution> RequiredViewports()
        =>
        [
            new RenderQaResolution("desktop", 1440, 900, 1.0),
            new RenderQaResolution("mobile", 390, 844, 3.0)
        ];

    private static RenderQaResolution RequiredViewport(string viewportId)
        => viewportId == "mobile"
            ? new RenderQaResolution("mobile", 390, 844, 3.0)
            : new RenderQaResolution("desktop", 1440, 900, 1.0);

    private static DesignConversionRenderResult RenderResult(string viewportId)
    {
        var resolution = RequiredViewport(viewportId);
        return new DesignConversionRenderResult(
            RenderResultId: $"render-{viewportId}",
            RenderRequestId: $"render-request-{viewportId}",
            Image: new RenderQaImageRecord(
                ImageId: $"actual-{viewportId}",
                Label: "actual-render",
                Resolution: resolution,
                Uri: $"artifact://actual-{viewportId}.png",
                Sha256: $"sha256:actual-{viewportId}",
                Metadata: new Dictionary<string, string>()),
            ElapsedMs: 1,
            Warnings: []);
    }

    private static DesignConversionVisualJudgeResult JudgeResult(
        string id,
        double score,
        bool passed,
        bool blocked,
        decimal? cost,
        bool externalCallExecuted = false,
        string? providerCostSource = null,
        string? resultCostSource = null,
        string? providerAttemptCount = null,
        string? resultAttemptCount = null,
        string? providerAttemptSource = null,
        string? resultAttemptSource = null)
    {
        var verdict = new VisualVerdict(
            VerdictId: $"{id}:verdict",
            Score: score,
            Defects: passed
                ? []
                :
                [
                    new VisualDefect(
                        DefectId: $"{id}:defect",
                        Category: "layout",
                        Severity: "major",
                        Description: "Synthetic test verdict only; no external AI claim.",
                        X: 0,
                        Y: 0,
                        Width: 1,
                        Height: 1,
                        EvidenceImageIds: ["expected", "actual"])
                ],
            EvidenceRefs: [$"{id}:evidence"],
            Passed: passed);
        var providerMetadata = new Dictionary<string, string>
        {
            ["external_ai_visual_judge_executed"] = externalCallExecuted.ToString()
        };
        if (!externalCallExecuted)
            providerMetadata[DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = (!blocked).ToString();
        if (externalCallExecuted && providerCostSource is not null)
            providerMetadata["cost_source"] = providerCostSource;
        if (externalCallExecuted && providerAttemptCount is not null)
            providerMetadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = providerAttemptCount;
        if (externalCallExecuted && providerAttemptSource is not null)
            providerMetadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] = providerAttemptSource;
        var resultMetadata = new Dictionary<string, string>
        {
            ["external_ai_visual_judge_executed"] = externalCallExecuted.ToString()
        };
        if (externalCallExecuted && resultCostSource is not null)
            resultMetadata["cost_source"] = resultCostSource;
        if (externalCallExecuted && resultAttemptCount is not null)
            resultMetadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = resultAttemptCount;
        if (externalCallExecuted && resultAttemptSource is not null)
            resultMetadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] = resultAttemptSource;

        return new DesignConversionVisualJudgeResult(
            JudgeResultId: id,
            JudgeRequestId: $"{id}:request",
            Status: blocked ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired : "fake_visual_judge_test_double",
            IsBlocked: blocked,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: blocked ? "none" : externalCallExecuted ? "external-visual-provider" : "fake-local-labeler",
                EndpointHost: externalCallExecuted ? "visual-provider.example.test" : "none",
                ModelId: blocked ? "none" : externalCallExecuted ? "external-visual-provider-model" : "fake-local-labeler",
                ExternalCallExecuted: externalCallExecuted,
                Metadata: providerMetadata),
            EstimatedCostUsd: cost,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: verdict.EvidenceRefs,
            Metadata: resultMetadata);
    }

    private static DesignConversionVisualJudgeResult ExternalJudgeResult(
        string id,
        double score,
        bool passed,
        decimal? cost,
        string? providerCostSource = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
        string? resultCostSource = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
        string? providerAttemptCount = null,
        string? resultAttemptCount = null,
        string? providerAttemptSource = null,
        string? resultAttemptSource = null)
        => JudgeResult(
            id,
            score,
            passed,
            blocked: false,
            cost: cost,
            externalCallExecuted: true,
            providerCostSource: providerCostSource,
            resultCostSource: resultCostSource,
            providerAttemptCount: providerAttemptCount,
            resultAttemptCount: resultAttemptCount,
            providerAttemptSource: providerAttemptSource,
            resultAttemptSource: resultAttemptSource);

    private static void AssertMalformedAttemptMetadataBlocksCostCompleteness(
        DesignConversionVisualQaBoundaryTrace trace)
    {
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        Assert.Equal(0m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal("false", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("1", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.TierPolicy.Metadata["cost_source"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
    }

    private static void AssertMalformedExternalComparisonCostBlocksCostCompleteness(
        DesignConversionVisualQaBoundaryTrace trace)
    {
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.TierPolicy.StopReason);
        Assert.False(trace.TierPolicy.ShouldContinue);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RetryModeNone, trace.TierPolicy.RetryMode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, trace.VisualDecision.Decision);
        Assert.True(trace.VisualDecision.QualityThresholdMet);
        Assert.False(trace.VisualDecision.CostBudgetMet);
        Assert.False(trace.VisualDecision.CanRetry);
        Assert.False(trace.VisualDecision.CanPersistTrainingCandidate);
        Assert.Equal(0m, trace.TierPolicy.EstimatedCostUsd);
        Assert.Equal("false", trace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("1", trace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.TierPolicy.Metadata["cost_source"]);
        Assert.Equal("false", trace.VisualDecision.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("1", trace.VisualDecision.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal("unknown", trace.VisualDecision.Metadata["cost_source"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, trace.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, trace.EvidenceRefs);
        Assert.Contains(trace.VisualDecision.CorrectionFindings, finding =>
            finding.Category == DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker &&
            finding.Severity == "blocker");
    }

    private static DesignConversionVisualJudgeResult HttpBlockedJudgeResult()
    {
        const string id = "terminal-http-blocked";
        var providerMetadata = new Dictionary<string, string>
        {
            ["network_call_executed"] = bool.TrueString,
            ["external_ai_visual_judge_executed"] = bool.TrueString,
            ["provider_response_identity_verified"] = bool.FalseString,
            ["provider_response_schema_verified"] = bool.FalseString,
            ["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = "2",
            [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] =
                ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue
        };
        var resultMetadata = new Dictionary<string, string>(providerMetadata, StringComparer.Ordinal);
        var evidenceRefs = new[]
        {
            ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked,
            ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusMarker,
            "terminal_http_status_code:500",
            "external_ai_visual_judge_executed:true"
        };
        var verdict = new VisualVerdict(
            VerdictId: $"{id}:verdict",
            Score: 0.0,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{id}:defect",
                    Category: "external_visual_judge_http_non_success",
                    Severity: "blocker",
                    Description: "Synthetic terminal HTTP failure for boundary-policy truth testing.",
                    X: 0,
                    Y: 0,
                    Width: 1,
                    Height: 1,
                    EvidenceImageIds: ["expected", "actual"])
            ],
            EvidenceRefs: evidenceRefs,
            Passed: false);

        return new DesignConversionVisualJudgeResult(
            JudgeResultId: id,
            JudgeRequestId: $"{id}:request",
            Status: ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked,
            IsBlocked: true,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: "openai",
                EndpointHost: "visual-provider.example.test",
                ModelId: "vision-model",
                ExternalCallExecuted: true,
                Metadata: providerMetadata),
            EstimatedCostUsd: 0.0084m,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: evidenceRefs,
            Metadata: resultMetadata);
    }

    private static DesignConversionVisualJudgeResult LocalJudgeResult(
        string id,
        double score,
        bool passed,
        decimal? cost = 0m)
    {
        var verdict = new VisualVerdict(
            VerdictId: $"{id}:verdict",
            Score: score,
            Defects: [],
            EvidenceRefs: [DefaultLocalDesignConversionVisualJudge.EvidenceMarker],
            Passed: passed);
        return new DesignConversionVisualJudgeResult(
            JudgeResultId: id,
            JudgeRequestId: $"{id}:request",
            Status: DefaultLocalDesignConversionVisualJudge.StatusOk,
            IsBlocked: false,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: DefaultLocalDesignConversionVisualJudge.ProviderName,
                EndpointHost: "local",
                ModelId: DefaultLocalDesignConversionVisualJudge.ModelId,
                ExternalCallExecuted: false,
                Metadata: new Dictionary<string, string>
                {
                    [DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker] = bool.TrueString,
                    ["external_ai_visual_judge_executed"] = bool.FalseString
                }),
            EstimatedCostUsd: cost,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: verdict.EvidenceRefs,
            Metadata: new Dictionary<string, string>
            {
                ["external_ai_visual_judge_executed"] = bool.FalseString
            });
    }

    private static DesignConversionRenderResult RenderResultFromPass78Manifest(RenderQaResolution resolution)
        => new(
            RenderResultId: $"pass78-{resolution.ResolutionId}-render",
            RenderRequestId: $"pass78-{resolution.ResolutionId}-render-request",
            Image: new RenderQaImageRecord(
                ImageId: $"pass78-{resolution.ResolutionId}-actual",
                Label: "browser-render-screenshot",
                Resolution: resolution,
                Uri: $"artifact://pass78/{resolution.ResolutionId}/screenshot.png",
                Sha256: $"sha256:pass78-{resolution.ResolutionId}",
                Metadata: new Dictionary<string, string>
                {
                    ["provider_kind"] = "browser_process_render_provider",
                    ["render_manifest_ref"] = "artifact://codex-pass78-browser-render-manifest.json",
                    ["png_dimensions"] = $"{resolution.Width}x{resolution.Height}",
                    ["nonblank_pixels"] = bool.TrueString
                }),
            ElapsedMs: 23,
            Warnings: []);

    private static RenderQaImageRecord ExpectedImage(RenderQaResolution resolution)
        => new(
            ImageId: $"expected-{resolution.ResolutionId}",
            Label: "expected-design-render",
            Resolution: resolution,
            Uri: $"artifact://expected/{resolution.ResolutionId}.png",
            Sha256: $"sha256:expected-{resolution.ResolutionId}",
            Metadata: new Dictionary<string, string>());

    private static DesignConversionQaComparisonArtifact TrustedExternalComparisonArtifact(
        string id,
        decimal? cost,
        Action<Dictionary<string, string>>? mutateMetadata = null)
    {
        var resolution = RequiredViewport("desktop");
        var expected = ExpectedImage(resolution);
        var actual = new RenderQaImageRecord(
            ImageId: $"actual-{id}",
            Label: "actual-render",
            Resolution: resolution,
            Uri: $"artifact://actual/{id}.png",
            Sha256: $"sha256:actual-{id}",
            Metadata: new Dictionary<string, string>());
        var jobId = $"job-{id}";
        var requestId = $"request-{id}";
        var comparisonRequestId = $"{jobId}:comparison-request:1";
        var artifactId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
            comparisonRequestId,
            comparisonRequestId);
        var visualResultId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
            comparisonRequestId,
            comparisonRequestId);
        var receiptId = $"{comparisonRequestId}:server-visual-provider-call-receipt";
        var costText = cost?.ToString("0.####", CultureInfo.InvariantCulture);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey] = bool.TrueString,
            [DesignConversionComparisonEvidenceTrustBoundary.TrustBoundaryKey] =
                DesignConversionComparisonEvidenceTrustBoundary.TrustBoundary,
            [DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey] =
                DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification,
            [DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey] = bool.TrueString,
            [DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey] = bool.TrueString,
            ["provider_kind"] = DesignConversionExternalVisualAiComparisonProvider.ProviderKind,
            ["comparison_provider_kind"] = DesignConversionExternalVisualAiComparisonProvider.ProviderKind,
            ["comparison_artifact_kind"] = "server_owned_external_visual_ai_receipt",
            ["hidden_determinism_boundary"] =
                DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification,
            ["visual_comparison_proven"] = bool.TrueString,
            ["server_owned_external_visual_ai_receipt_validated"] = bool.TrueString,
            ["server_owned_external_visual_ai_receipt_validation_reason"] = "valid",
            ["server_owned_external_visual_ai_comparison_trust_verified"] = bool.TrueString,
            ["external_ai_used"] = bool.TrueString,
            ["external_ai_visual_judge_executed"] = bool.TrueString,
            ["network_call_executed"] = bool.TrueString,
            ["real_external_provider_executed"] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey] = bool.TrueString,
            ["external_visual_ai_configured"] = bool.TrueString,
            ["external_visual_ai_executed"] = bool.TrueString,
            ["visual_ai_provider_contract_executed"] = bool.TrueString,
            ["fake_client"] = bool.FalseString,
            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.FalseString,
            ["external_visual_ai_blocked"] = bool.FalseString,
            ["blocked_reason"] = "none",
            ["provider_response_identity_verified"] = bool.TrueString,
            ["provider_response_schema_verified"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["status"] = ExternalHttpDesignConversionVisualJudge.StatusOk,
            ["provider_name"] = "openai",
            ["provider_model"] = "gpt-visual-boundary",
            ["provider_endpoint_host"] = "visual-provider.example.test",
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey] = "openai",
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey] = "executed",
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey] = "external_executed",
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey] = "not_needed",
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey] =
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey] = receiptId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey] = jobId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey] = comparisonRequestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey] = requestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey] = requestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] = comparisonRequestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey] = comparisonRequestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey] = visualResultId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey] = artifactId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey] = "openai",
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey] = "gpt-visual-boundary",
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey] = "visual-provider.example.test",
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey] = bool.TrueString
        };
        if (costText is not null)
        {
            metadata["estimated_cost_usd"] = costText;
            metadata["cost_source"] =
                DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest;
            metadata["server_owned_pricing_valid"] = bool.TrueString;
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = "1";
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] =
                ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue;
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey] = costText;
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey] = costText;
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey] =
                ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt;
        }
        else
        {
            metadata["server_owned_pricing_valid"] = bool.TrueString;
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] = "1";
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] =
                ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue;
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey] =
                ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt;
        }

        mutateMetadata?.Invoke(metadata);
        var evidenceRefs = new[]
        {
            DesignConversionExternalVisualAiComparisonProvider.ProviderKind,
            DesignConversionExternalVisualAiReceiptValidator.ValidatedEvidenceRef,
            "network_call_executed:true",
            "real_external_provider_executed:true",
            "external_ai_visual_judge_executed:true",
            $"{ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey}:true",
            $"visual_provider_call_receipt:{receiptId}"
        };
        var verdict = new VisualVerdict(
            VerdictId: $"{id}:comparison-verdict",
            Score: 0.70,
            Defects: [],
            EvidenceRefs: evidenceRefs,
            Passed: false);

        return new DesignConversionQaComparisonArtifact(
            ArtifactId: artifactId,
            ComparisonRequestId: comparisonRequestId,
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            DiffImageUri: $"artifact://diff/{id}.png",
            DiffImageSha256: $"sha256:diff-{id}",
            Metrics: new Dictionary<string, double>
            {
                ["visual_comparison_proven"] = 1.0,
                ["server_owned_external_visual_ai_receipt_validated"] = 1.0,
                ["estimated_cost_usd"] = cost.HasValue ? (double)cost.Value : 0.0
            },
            EvidenceRefs: evidenceRefs)
        {
            Metadata = metadata
        };
    }

    private static DesignConversionQaComparisonArtifact NoCallCanonicalExternalComparisonArtifact(
        string id,
        decimal estimatedCost)
    {
        var artifact = TrustedExternalComparisonArtifact(id, cost: null);
        var metadata = new Dictionary<string, string>(artifact.Metadata, StringComparer.Ordinal)
        {
            [DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey] =
                DesignConversionComparisonEvidenceTrustBoundary.UntrustedInjectedClassification,
            ["comparison_artifact_kind"] = "external_visual_ai_not_configured",
            ["server_owned_external_visual_ai_receipt_validated"] = bool.FalseString,
            ["server_owned_external_visual_ai_comparison_trust_verified"] = bool.FalseString,
            ["external_ai_used"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["network_call_executed"] = bool.FalseString,
            ["real_external_provider_executed"] = bool.FalseString,
            ["external_visual_ai_configured"] = bool.FalseString,
            ["external_visual_ai_executed"] = bool.FalseString,
            ["visual_ai_provider_contract_executed"] = bool.FalseString,
            ["external_visual_ai_blocked"] = bool.TrueString,
            ["blocked_reason"] = "not_configured",
            ["estimated_cost_usd"] = estimatedCost.ToString("0.####", CultureInfo.InvariantCulture),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey] = bool.FalseString,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey] = bool.FalseString,
            [ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey] = bool.FalseString,
            [ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey] = bool.FalseString,
            [ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey] = bool.FalseString
        };
        metadata.Remove("cost_source");
        metadata.Remove("server_owned_pricing_valid");
        metadata.Remove(ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey);
        metadata.Remove(ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey);
        metadata.Remove(ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey);
        metadata.Remove(ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey);
        metadata.Remove(ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey);

        return artifact with
        {
            EvidenceRefs =
            [
                DesignConversionExternalVisualAiComparisonProvider.ProviderKind,
                DesignConversionExternalVisualAiComparisonProvider.NetworkCallNotExecutedMarker,
                DesignConversionExternalVisualAiComparisonProvider.RealExternalProviderNotExecutedMarker,
                DesignConversionExternalVisualAiComparisonProvider.NoKeyBlockedMarker
            ],
            Metadata = metadata
        };
    }

    private static DesignConversionQaComparisonArtifact ForgedLocalComparisonArtifact(
        string id,
        decimal cost)
    {
        var resolution = RequiredViewport("desktop");
        var expected = ExpectedImage(resolution);
        var actual = new RenderQaImageRecord(
            ImageId: $"forged-actual-{id}",
            Label: "forged-actual-render",
            Resolution: resolution,
            Uri: $"artifact://forged/{id}.png",
            Sha256: $"sha256:forged-{id}",
            Metadata: new Dictionary<string, string>());
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["provider_kind"] = "untrusted-local-test-provider",
            ["comparison_provider_kind"] = "untrusted-local-test-provider",
            ["estimated_cost_usd"] = cost.ToString("0.####", CultureInfo.InvariantCulture),
            ["external_ai_visual_judge_executed"] = bool.TrueString,
            ["network_call_executed"] = bool.TrueString,
            ["real_external_provider_executed"] = bool.TrueString
        };
        var evidenceRefs = new[]
        {
            "untrusted-local-test-provider",
            "network_call_executed:true",
            "real_external_provider_executed:true",
            "external_ai_visual_judge_executed:true"
        };
        var verdict = new VisualVerdict(
            VerdictId: $"{id}:forged-verdict",
            Score: 0.70,
            Defects: [],
            EvidenceRefs: evidenceRefs,
            Passed: false);

        return new DesignConversionQaComparisonArtifact(
            ArtifactId: $"{id}:forged-comparison-artifact",
            ComparisonRequestId: $"{id}:comparison-request",
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            DiffImageUri: $"artifact://forged-diff/{id}.png",
            DiffImageSha256: $"sha256:forged-diff-{id}",
            Metrics: new Dictionary<string, double>
            {
                ["estimated_cost_usd"] = (double)cost
            },
            EvidenceRefs: evidenceRefs)
        {
            Metadata = metadata
        };
    }
}
