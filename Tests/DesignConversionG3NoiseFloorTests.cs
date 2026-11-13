// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using DesignConversionDomain;

namespace LlmFromScratch.Tests;

public sealed class DesignConversionG3NoiseFloorTests
{
    private static readonly DesignConversionG3AcceptanceEvidenceChain CompleteEvidence =
        new(
            TypedProofVerified: true,
            FreshLoadVerified: true,
            ContinuedTrainingVerified: true,
            ImmediateParentLineageVerified: true);

    [Fact]
    public void Evaluate_GainInsideMeasurementNoiseFloorRejectsWithDistinctReason()
    {
        var metrics = Metrics(heldOutCount: 200, baselineWins: 112, absoluteCorrect: 200);
        var passport = PassportForG3(metrics);

        var uncalibrated = DesignConversionG3AcceptancePolicy.Evaluate(metrics, CompleteEvidence);
        var calibrated = DesignConversionG3AcceptancePolicy.Evaluate(
            metrics,
            CompleteEvidence,
            measurementNoisePassport: passport);

        Assert.True(uncalibrated.Eligible);
        Assert.Equal(MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus,
            uncalibrated.MeasurementNoiseStatus);
        Assert.False(calibrated.Eligible);
        Assert.Equal(DesignConversionG3AcceptancePolicy.BaselineWinInsideMeasurementNoiseReason,
            calibrated.Reason);
        Assert.Equal(MeasurementNoisePolicy.IndistinguishableStatus,
            calibrated.MeasurementNoiseStatus);
        Assert.False(calibrated.ComparativeGatePassed);
        Assert.Equal(passport.NoiseFloor, calibrated.MeasurementNoiseFloor);
    }

    [Fact]
    public void Evaluate_GainStrictlyAboveMeasurementNoiseFloorPasses()
    {
        var metrics = Metrics(heldOutCount: 200, baselineWins: 124, absoluteCorrect: 200);
        var passport = PassportForG3(metrics);

        var actual = DesignConversionG3AcceptancePolicy.Evaluate(
            metrics,
            CompleteEvidence,
            measurementNoisePassport: passport);

        Assert.True(
            metrics.BaselineWinRate -
            DesignConversionG3AcceptancePolicy.CanonicalMinimumBaselineWinRate >
            passport.NoiseFloor);
        Assert.True(actual.Eligible);
        Assert.True(actual.ComparativeGatePassed);
        Assert.Equal(DesignConversionG3AcceptancePolicy.EligibleReason, actual.Reason);
        Assert.Equal(MeasurementNoisePolicy.ImprovedStatus, actual.MeasurementNoiseStatus);
    }

    [Fact]
    public void Evaluate_SmallSampleBinomialTailStillRunsBeforeNoiseFloor()
    {
        var metrics = Metrics(heldOutCount: 50, baselineWins: 31, absoluteCorrect: 50);
        var passport = PassportForG3(metrics);

        var actual = DesignConversionG3AcceptancePolicy.Evaluate(
            metrics,
            CompleteEvidence,
            measurementNoisePassport: passport);

        // Шум не заменяет DEC-03 хвост: малый held-out сначала обязан пройти старую статистическую проверку.
        Assert.False(actual.Eligible);
        Assert.NotNull(actual.OneSidedBinomialTailProbability);
        Assert.Equal(DesignConversionG3AcceptancePolicy.SmallSampleSignificanceNotMetReason,
            actual.Reason);
    }

    [Fact]
    public void Evaluate_InconsistentPassportFailsClosed()
    {
        var metrics = Metrics(heldOutCount: 200, baselineWins: 112, absoluteCorrect: 200);
        var passport = MeasurementNoisePolicy.BuildPassport(
            Identity,
            "g3-noise-passport",
            metrics.HeldOutCount,
            [0.57d, 0.57d, 0.57d, 0.57d, 0.57d],
            MeasurementNoisePolicyConfig.Canonical,
            DateTimeOffset.UnixEpoch);

        var actual = DesignConversionG3AcceptancePolicy.Evaluate(
            metrics,
            CompleteEvidence,
            measurementNoisePassport: passport);

        Assert.False(actual.Eligible);
        Assert.Equal(DesignConversionG3AcceptancePolicy.MeasurementNoisePassportInvalidReason,
            actual.Reason);
        Assert.Equal(MeasurementNoisePolicy.InvalidPassportStatus, actual.MeasurementNoiseStatus);
    }

    [Theory]
    [InlineData("scorer-id")]
    [InlineData("scorer-version")]
    [InlineData("normalization-profile")]
    [InlineData("metric-profile")]
    public void Evaluate_ForeignPassportScorerIdentityFailsClosed(string mutation)
    {
        var metrics = Metrics(heldOutCount: 200, baselineWins: 124, absoluteCorrect: 200);
        var passportIdentity = mutation switch
        {
            "scorer-id" => Identity with { ScorerId = "foreign-g3-scorer" },
            "scorer-version" => Identity with { ScorerVersion = "v2" },
            "normalization-profile" => Identity with { NormalizationProfileId = "g3-normalization-v2" },
            _ => Identity with { MetricProfileId = "baseline-win-rate-v2" }
        };
        var passport = MeasurementNoisePolicy.BuildPassport(
            passportIdentity,
            EvaluationSetId,
            metrics.HeldOutCount,
            Enumerable.Repeat(metrics.BaselineWinRate,
                MeasurementNoisePolicyConfig.DefaultRepeatCount).ToArray(),
            MeasurementNoisePolicyConfig.Canonical,
            DateTimeOffset.UnixEpoch);

        var actual = DesignConversionG3AcceptancePolicy.Evaluate(
            metrics,
            CompleteEvidence,
            measurementNoisePassport: passport);

        Assert.False(actual.Eligible);
        Assert.Equal(DesignConversionG3AcceptancePolicy.MeasurementNoisePassportInvalidReason,
            actual.Reason);
        Assert.Equal(MeasurementNoisePolicy.InvalidPassportStatus, actual.MeasurementNoiseStatus);
    }

    private static DesignConversionG3AcceptanceMetrics Metrics(
        int heldOutCount,
        int baselineWins,
        int absoluteCorrect)
        => new(
            HeldOutCount: heldOutCount,
            BaselineWinCount: baselineWins,
            BaselineWinRate: (double)baselineWins / heldOutCount,
            AbsoluteChildPreferenceCorrectCount: absoluteCorrect,
            AbsoluteChildPreferenceAccuracy: (double)absoluteCorrect / heldOutCount)
        {
            MeasurementNoiseScorerIdentity = Identity,
            MeasurementNoiseEvaluationSetId = EvaluationSetId
        };

    private static MeasurementNoisePassport PassportForG3(
        DesignConversionG3AcceptanceMetrics metrics)
        => MeasurementNoisePolicy.BuildPassport(
            Identity,
            EvaluationSetId,
            metrics.HeldOutCount,
            Enumerable.Repeat(metrics.BaselineWinRate, MeasurementNoisePolicyConfig.DefaultRepeatCount).ToArray(),
            MeasurementNoisePolicyConfig.Canonical,
            DateTimeOffset.UnixEpoch);

    private static HeldOutScorerIdentity Identity { get; } = new(
        DomainTag: "design-conversion-g3",
        ScorerId: "g3-held-out-scorer",
        ScorerVersion: "v1",
        NormalizationProfileId: "g3-normalization",
        MetricProfileId: "baseline-win-rate");

    private const string EvaluationSetId = "g3-noise-passport";
}
