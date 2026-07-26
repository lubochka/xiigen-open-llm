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
        Assert.Equal("0.8", freeBudgetCap.TierPolicy.Metadata["quality_threshold"]);
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
        Assert.Equal("0.8", free.TierPolicy.Metadata["quality_threshold"]);
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
        Assert.Equal("0.86", paid.TierPolicy.Metadata["quality_threshold"]);
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
    public void Evaluate_KnownTierZeroSubmissionUsesEntitlementDefaults(
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
        Assert.Equal(expectedMaxAttempts, trace.TierPolicy.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(
            expectedMaxAttempts,
            DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierMaxImprovementCandidates(0, tier)
                .ToString(CultureInfo.InvariantCulture));
        Assert.Equal("0", trace.TierPolicy.Metadata["submitted_budget_usd"]);
        Assert.Equal("0", trace.TierPolicy.Metadata["submitted_max_improvement_candidates"]);
        Assert.Equal(expectedBudgetUsd, trace.TierPolicy.Metadata["effective_budget_usd"]);
        Assert.Equal(expectedMaxAttempts, trace.TierPolicy.Metadata["effective_max_improvement_candidates"]);
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
        Assert.Equal(expectedShouldContinue, trace.VisualDecision.CanRetry);
        Assert.Equal(bool.FalseString, trace.TierPolicy.Metadata["product_ready"]);
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

    [Fact]
    public void Evaluate_SameVisualScoreUsesTierThresholdBeforeAccepting()
    {
        const double sameScore = 0.84;
        var freeSameScore = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("free", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("free-same-score", score: sameScore, passed: true, blocked: false, cost: 0.02m)]);
        var proSameScore = DesignConversionVisualQaBoundaryPolicy.Evaluate(
            Budget("pro", maxAttempts: 3, budgetUsd: 1.0m),
            RequiredViewports(),
            [RenderResult("desktop"), RenderResult("mobile")],
            [JudgeResult("pro-same-score", score: sameScore, passed: true, blocked: false, cost: 0.02m)]);

        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, freeSameScore.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionAccepted, freeSameScore.VisualDecision.Decision);
        Assert.False(freeSameScore.TierPolicy.ShouldContinue);
        Assert.True(freeSameScore.VisualDecision.QualityThresholdMet);
        Assert.Equal("0.8", freeSameScore.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal("0.02", freeSameScore.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, proSameScore.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, proSameScore.VisualDecision.Decision);
        Assert.True(proSameScore.TierPolicy.ShouldContinue);
        Assert.False(proSameScore.VisualDecision.QualityThresholdMet);
        Assert.True(proSameScore.VisualDecision.CostBudgetMet);
        Assert.Equal("0.9", proSameScore.TierPolicy.Metadata["quality_threshold"]);
        Assert.Equal("0.02", proSameScore.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal("0.98", proSameScore.TierPolicy.Metadata["remaining_budget_usd"]);
        Assert.Contains(proSameScore.VisualDecision.CorrectionFindings, finding =>
            finding.Category == "quality_threshold_not_met" &&
            finding.CanDriveRetry);
        Assert.All(
            [freeSameScore, proSameScore],
            trace =>
            {
                Assert.True(trace.MultiResolutionRender.IsComplete);
                Assert.False(trace.ExternalAiVisualJudgeExecuted);
                Assert.False(trace.ProductReady);
                Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierPolicyMarker, trace.EvidenceRefs);
                Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, trace.EvidenceRefs);
                Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, trace.EvidenceRefs);
            });
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
        double? qualityThreshold = null)
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
            Metadata: metadata);
    }

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
}
