// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;
using System.Text.Json;
using Xunit;

public sealed class DesignConversionG3AcceptancePolicyTests
{
    private static readonly DesignConversionG3AcceptanceEvidenceChain CompleteEvidence =
        new(
            TypedProofVerified: true,
            FreshLoadVerified: true,
            ContinuedTrainingVerified: true,
            ImmediateParentLineageVerified: true);

    [Theory]
    [InlineData(49, 49, 49, false, DesignConversionG3AcceptancePolicy.HeldOutBelowMinimumReason)]
    [InlineData(50, 31, 50, false, DesignConversionG3AcceptancePolicy.SmallSampleSignificanceNotMetReason)]
    [InlineData(50, 32, 50, true, DesignConversionG3AcceptancePolicy.EligibleReason)]
    [InlineData(100, 58, 100, false, DesignConversionG3AcceptancePolicy.SmallSampleSignificanceNotMetReason)]
    [InlineData(100, 59, 100, true, DesignConversionG3AcceptancePolicy.EligibleReason)]
    [InlineData(199, 111, 199, false, DesignConversionG3AcceptancePolicy.SmallSampleSignificanceNotMetReason)]
    [InlineData(199, 112, 199, true, DesignConversionG3AcceptancePolicy.EligibleReason)]
    [InlineData(200, 109, 200, false, DesignConversionG3AcceptancePolicy.BaselineWinRateBelowFloorReason)]
    [InlineData(200, 110, 200, true, DesignConversionG3AcceptancePolicy.EligibleReason)]
    public void Evaluate_CanonicalComparativeBoundaryMatrix(
        int heldOutCount,
        int wins,
        int absoluteCorrect,
        bool eligible,
        string reason)
    {
        var actual = Evaluate(heldOutCount, wins, absoluteCorrect);

        Assert.Equal(eligible, actual.Eligible);
        Assert.Equal(reason, actual.Reason);
        Assert.Equal(
            heldOutCount < DesignConversionG3AcceptancePolicy.CanonicalLargeSampleHeldOutCount &&
            heldOutCount >= DesignConversionG3AcceptancePolicy.CanonicalMinimumHeldOutCount &&
            (double)wins / heldOutCount >= DesignConversionG3AcceptancePolicy.CanonicalMinimumBaselineWinRate,
            actual.OneSidedBinomialTailProbability.HasValue);
    }

    [Fact]
    public void Evaluate_LargeSampleCutoffKeepsTailAt199AndDropsItAt200()
    {
        var lastSmallSample = Evaluate(199, baselineWins: 112, absoluteCorrect: 199);
        var firstLargeSample = Evaluate(200, baselineWins: 110, absoluteCorrect: 200);

        Assert.True(lastSmallSample.Eligible);
        Assert.NotNull(lastSmallSample.OneSidedBinomialTailProbability);
        Assert.True(firstLargeSample.Eligible);
        Assert.Null(firstLargeSample.OneSidedBinomialTailProbability);
    }

    [Fact]
    public void Evaluate_ComparativePassButAbsolutePoint74Fails()
    {
        var actual = Evaluate(100, baselineWins: 100, absoluteCorrect: 74);

        Assert.False(actual.Eligible);
        Assert.True(actual.ComparativeGatePassed);
        Assert.False(actual.AbsoluteGatePassed);
        Assert.Equal(DesignConversionG3AcceptancePolicy.AbsoluteAccuracyBelowFloorReason, actual.Reason);
    }

    [Fact]
    public void Evaluate_ExactAbsolutePoint75IsEligible()
    {
        var actual = Evaluate(100, baselineWins: 100, absoluteCorrect: 75);

        Assert.True(actual.Eligible);
        Assert.True(actual.AbsoluteGatePassed);
        Assert.Equal(DesignConversionG3AcceptancePolicy.EligibleReason, actual.Reason);
    }

    [Theory]
    [InlineData(-1, 0d, 0, 0d)]
    [InlineData(51, 0.5d, 50, 0.5d)]
    [InlineData(50, double.NaN, 50, 1d)]
    [InlineData(50, 1d, 50, double.PositiveInfinity)]
    [InlineData(50, 1d, 51, 1d)]
    public void Evaluate_InvalidOrTamperedMetricsFailClosed(
        int baselineWins,
        double baselineWinRate,
        int absoluteCorrect,
        double absoluteAccuracy)
    {
        var actual = DesignConversionG3AcceptancePolicy.Evaluate(
            new DesignConversionG3AcceptanceMetrics(
                HeldOutCount: 50,
                BaselineWinCount: baselineWins,
                BaselineWinRate: baselineWinRate,
                AbsoluteChildPreferenceCorrectCount: absoluteCorrect,
                AbsoluteChildPreferenceAccuracy: absoluteAccuracy),
            CompleteEvidence);

        Assert.False(actual.Eligible);
        Assert.False(actual.MetricsValid);
        Assert.Equal(DesignConversionG3AcceptancePolicy.InvalidMetricsReason, actual.Reason);
    }

    [Theory]
    [InlineData(false, true, true, true, DesignConversionG3AcceptancePolicy.TypedProofMissingReason)]
    [InlineData(true, false, true, true, DesignConversionG3AcceptancePolicy.FreshLoadMissingReason)]
    [InlineData(true, true, false, true, DesignConversionG3AcceptancePolicy.ContinuedTrainingMissingReason)]
    [InlineData(true, true, true, false, DesignConversionG3AcceptancePolicy.ImmediateParentLineageInvalidReason)]
    public void Evaluate_EvidenceChainFailsClosedWithExactReason(
        bool typedProof,
        bool freshLoad,
        bool continuedTraining,
        bool immediateParent,
        string reason)
    {
        var actual = DesignConversionG3AcceptancePolicy.Evaluate(
            Metrics(100, 100, 100),
            new DesignConversionG3AcceptanceEvidenceChain(
                typedProof,
                freshLoad,
                continuedTraining,
                immediateParent));

        Assert.False(actual.Eligible);
        Assert.Equal(reason, actual.Reason);
    }

    [Fact]
    public void CanonicalConfig_IsIndependentOfMachineMeasurementThresholds()
    {
        var permissive = DesignConversionMachineEvaluationOptions.FromMeasurementGateThresholds(
            new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.01,
                MaxRuntimeMs: 1,
                MaxCostUsd: 0.01m,
                MaxPeakRamMb: 1));
        var strict = DesignConversionMachineEvaluationOptions.FromMeasurementGateThresholds(
            new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.99,
                MaxRuntimeMs: 999,
                MaxCostUsd: 999m,
                MaxPeakRamMb: 999));

        Assert.NotEqual(
            permissive.HeldOutEvaluationAccuracyGateThreshold,
            strict.HeldOutEvaluationAccuracyGateThreshold);
        Assert.Equal(50, DesignConversionG3AcceptancePolicyConfig.Canonical.MinimumHeldOutCount);
        Assert.Equal(0.55d, DesignConversionG3AcceptancePolicyConfig.Canonical.MinimumBaselineWinRate);
        Assert.Equal(0.05d, DesignConversionG3AcceptancePolicyConfig.Canonical.SmallSampleSignificanceLevel);
        Assert.Equal(0.75d, DesignConversionG3AcceptancePolicyConfig.Canonical.AbsoluteAccuracyFloor);
        Assert.True(Evaluate(100, 59, 75).Eligible);
    }

    [Fact]
    public void BridgeMetricSummary_SeparatesComparativeHalfFromAbsoluteOne()
    {
        var actual = DesignConversionDpoPolicyCheckpointBridge
            .ComputeEvaluatedMetricSummaryForTests(
            [
                (ChildChosen: 2f, ChildRejected: 1f, ReferenceChosen: 3f, ReferenceRejected: 1f),
                (ChildChosen: 3f, ChildRejected: 1f, ReferenceChosen: 2f, ReferenceRejected: 1f)
            ]);

        Assert.True(actual.IsSuccess, actual.ErrorMessage);
        Assert.Equal(1, actual.Value!.CorrectCount);
        Assert.Equal(0.5d, actual.Value.Accuracy);
        Assert.Equal(2, actual.Value.AbsoluteChildPreferenceCorrectCount);
        Assert.Equal(1d, actual.Value.AbsoluteChildPreferenceAccuracy);
    }

    [Fact]
    public void BridgeMetricSummary_AbsoluteChildTieFailsStrictly()
    {
        var actual = DesignConversionDpoPolicyCheckpointBridge
            .ComputeEvaluatedMetricSummaryForTests(
            [
                (ChildChosen: 1f, ChildRejected: 1f, ReferenceChosen: 0f, ReferenceRejected: 1f)
            ]);

        Assert.True(actual.IsSuccess, actual.ErrorMessage);
        Assert.Equal(1, actual.Value!.CorrectCount);
        Assert.Equal(1d, actual.Value.Accuracy);
        Assert.Equal(0, actual.Value.AbsoluteChildPreferenceCorrectCount);
        Assert.Equal(0d, actual.Value.AbsoluteChildPreferenceAccuracy);
    }

    [Theory]
    [InlineData(DesignConversionDpoPolicyCheckpointBridge.LegacyEvaluatedSchema, false)]
    [InlineData(DesignConversionDpoPolicyCheckpointBridge.PreviousEvaluatedSchema, true)]
    public void PreviousEvaluationSchemas_JsonRoundTripAsEvidenceOnly(
        string schema,
        bool hasLineage)
    {
        var evaluation = BuildPreviousEvaluation(schema, hasLineage);

        var json = JsonSerializer.SerializeToUtf8Bytes(
            evaluation,
            DesignConversionCheckpointJsonOptions.Standard);
        var loaded = JsonSerializer.Deserialize<DesignConversionDpoHeldOutEvaluation>(
            json,
            DesignConversionCheckpointJsonOptions.Standard);

        Assert.NotNull(loaded);
        Assert.Equal(schema, loaded.Schema);
        Assert.Null(loaded.G3Metrics);
        Assert.Equal(hasLineage, loaded.CycleAccuracyLineage is not null);
    }

    private static DesignConversionG3AcceptanceResult Evaluate(
        int heldOutCount,
        int baselineWins,
        int absoluteCorrect)
        => DesignConversionG3AcceptancePolicy.Evaluate(
            Metrics(heldOutCount, baselineWins, absoluteCorrect),
            CompleteEvidence);

    private static DesignConversionG3AcceptanceMetrics Metrics(
        int heldOutCount,
        int baselineWins,
        int absoluteCorrect)
        => new(
            HeldOutCount: heldOutCount,
            BaselineWinCount: baselineWins,
            BaselineWinRate: (double)baselineWins / heldOutCount,
            AbsoluteChildPreferenceCorrectCount: absoluteCorrect,
            AbsoluteChildPreferenceAccuracy: (double)absoluteCorrect / heldOutCount);

    private static DesignConversionDpoHeldOutEvaluation BuildPreviousEvaluation(
        string schema,
        bool hasLineage)
    {
        const int heldOutCount = 50;
        const int correctCount = 32;
        const double accuracy = (double)correctCount / heldOutCount;
        var evaluation = new DesignConversionDpoHeldOutEvaluation(
            Schema: schema,
            PackingId: DesignConversionDpoPolicyCheckpointBridge.EvaluatedPackingId,
            VocabSize: 256,
            ContextLength: 8,
            DpoStepCount: 1,
            MaxBridgePreferenceCount: 1,
            ChildPolicyCheckpointSha256: new string('1', 64),
            ReferencePolicyCheckpointSha256: new string('2', 64),
            ReferenceMode: "provider_fresh_step_zero",
            DatasetId: "g3-previous-schema",
            PartitionSnapshotSha256: new string('3', 64),
            TrainRecordSetSha256: new string('4', 64),
            HeldOutRecordSetSha256: new string('5', 64),
            TrainSourceSetSha256: new string('6', 64),
            HeldOutSourceSetSha256: new string('7', 64),
            SelectedTrainInputSha256: new string('8', 64),
            HeldOutEvaluationInputSha256: new string('9', 64),
            TrainRecordCount: 1,
            HeldOutRecordCount: heldOutCount,
            EvaluatedHeldOutRecordCount: heldOutCount,
            CorrectCount: correctCount,
            Accuracy: accuracy,
            MeanRelativeMargin: 0.1d,
            MeanChildMargin: 0.2d,
            MeanReferenceMargin: 0.1d,
            FreshLoadVerified: true,
            ReferenceFrozen: true,
            SourceDisjoint: true,
            HeldOutNotTrained: true);
        return !hasLineage
            ? evaluation
            : evaluation with
            {
                CycleAccuracyLineage = new DesignConversionDpoHeldOutCycleAccuracyLineage(
                    CycleOrdinal: 1,
                    CurrentMeasuredAccuracy: accuracy,
                    HasParentMeasurement: false,
                    ParentCycleOrdinal: null,
                    ParentMeasuredAccuracy: null,
                    ParentPolicyCheckpointSha256: null,
                    ParentDatasetId: null,
                    ParentHeldOutRecordSetSha256: null,
                    ParentHeldOutSourceSetSha256: null,
                    ParentHeldOutEvaluationInputSha256: null,
                    ComparableHeldOut: false,
                    AccuracyDelta: null)
            };
    }
}
