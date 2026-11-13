// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;

namespace LlmFromScratch.Tests;

public sealed class MeasurementNoisePolicyTests
{
    [Fact]
    public void PolicyIdentity_IsStableAndChangesWhenItsContractChanges()
    {
        var canonical = MeasurementNoisePolicyConfig.Canonical;
        var same = new MeasurementNoisePolicyConfig(
            canonical.RepeatCount,
            canonical.StandardDeviationMultiplier);
        var changedRepeatCount = new MeasurementNoisePolicyConfig(
            canonical.RepeatCount + 1,
            canonical.StandardDeviationMultiplier);
        var changedMultiplier = new MeasurementNoisePolicyConfig(
            canonical.RepeatCount,
            canonical.StandardDeviationMultiplier + 0.25d);

        Assert.Equal(canonical.IdentitySha256, same.IdentitySha256);
        Assert.Equal(64, canonical.IdentitySha256.Length);
        Assert.NotEqual(canonical.IdentitySha256, changedRepeatCount.IdentitySha256);
        Assert.NotEqual(canonical.IdentitySha256, changedMultiplier.IdentitySha256);
    }

    [Fact]
    public void BuildPassport_IdenticalRepeatsStillCarryNonZeroBinomialLowerBound()
    {
        var passport = BuildPassport("eval-identical", scoredRows: 100, [0.9d, 0.9d, 0.9d, 0.9d, 0.9d]);

        Assert.Equal(0d, passport.SampleStandardDeviation, 12);
        Assert.Equal(0.06d, passport.BinomialLowerBound, 12);
        Assert.Equal(passport.BinomialLowerBound, passport.NoiseFloor, 12);
    }

    [Fact]
    public void ClassifyDifference_EqualityToNoiseFloorIsIndistinguishable()
    {
        var passport = BuildPassport("eval-equality", scoredRows: 100, [0.9d, 0.9d, 0.9d, 0.9d, 0.9d]);

        var exactFloor = MeasurementNoisePolicy.ClassifyDifference(passport.NoiseFloor, passport);
        var justAbove = MeasurementNoisePolicy.ClassifyDifference(passport.NoiseFloor + 1e-9d, passport);
        var justBelowNegative = MeasurementNoisePolicy.ClassifyDifference(-passport.NoiseFloor - 1e-9d, passport);

        // Граница прибора не является выигрышем: направление появляется только строго за полом шума.
        Assert.Equal(MeasurementNoisePolicy.IndistinguishableStatus, exactFloor.Status);
        Assert.False(exactFloor.DifferenceProven);
        Assert.Equal(MeasurementNoisePolicy.ImprovedStatus, justAbove.Status);
        Assert.True(justAbove.DifferenceProven);
        Assert.Equal(MeasurementNoisePolicy.RegressedStatus, justBelowNegative.Status);
        Assert.True(justBelowNegative.DifferenceProven);
    }

    [Fact]
    public void ClassifyDifference_WithoutCalibrationKeepsDifferenceUnproven()
    {
        var comparison = MeasurementNoisePolicy.ClassifyDifference(0.42d, passport: null);

        Assert.Null(comparison.NoiseFloor);
        Assert.False(comparison.DifferenceProven);
        Assert.Equal(MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus, comparison.Status);
    }

    [Fact]
    public void ValidatePassportForMeasurement_RejectsForeignRowsAndMeanMismatch()
    {
        var passport = BuildPassport("eval-local", scoredRows: 50, [0.8d, 0.8d, 0.8d, 0.8d, 0.8d]);

        Assert.False(MeasurementNoisePolicy.TryValidatePassportForMeasurement(
            passport,
            Identity,
            "eval-foreign",
            expectedScoredRows: 50,
            expectedMeanScore: 0.8d,
            out var foreignReason));
        Assert.Equal("measurement_noise_passport_foreign_evaluation_set", foreignReason);

        Assert.False(MeasurementNoisePolicy.TryValidatePassportForMeasurement(
            passport,
            Identity,
            "eval-local",
            expectedScoredRows: 51,
            expectedMeanScore: 0.8d,
            out var rowReason));
        Assert.Equal("measurement_noise_passport_row_count_mismatch", rowReason);

        Assert.False(MeasurementNoisePolicy.TryValidatePassportForMeasurement(
            passport,
            Identity,
            "eval-local",
            expectedScoredRows: 50,
            expectedMeanScore: 0.81d,
            out var meanReason));
        Assert.Equal("measurement_noise_passport_mean_score_mismatch", meanReason);
    }

    [Theory]
    [InlineData("scorer-id")]
    [InlineData("scorer-version")]
    [InlineData("normalization-profile")]
    [InlineData("metric-profile")]
    public void ValidatePassportForMeasurement_RejectsForeignScorerIdentity(
        string mutation)
    {
        var passport = BuildPassport("eval-local", scoredRows: 50, [0.8d, 0.8d, 0.8d, 0.8d, 0.8d]);
        var expectedIdentity = mutation switch
        {
            "scorer-id" => Identity with { ScorerId = "foreign-scorer" },
            "scorer-version" => Identity with { ScorerVersion = "v2" },
            "normalization-profile" => Identity with { NormalizationProfileId = "normalization-v2" },
            _ => Identity with { MetricProfileId = "accuracy-v2" }
        };

        var accepted = MeasurementNoisePolicy.TryValidatePassportForMeasurement(
            passport,
            expectedIdentity,
            "eval-local",
            expectedScoredRows: 50,
            expectedMeanScore: 0.8d,
            out var reason);

        Assert.False(accepted);
        Assert.Equal("measurement_noise_passport_scorer_identity_mismatch", reason);
    }

    [Fact]
    public void PassportConstruction_RejectsRepeatCountAndInternalInconsistency()
    {
        Assert.Throws<ArgumentException>(() => MeasurementNoisePolicy.BuildPassport(
            Identity,
            "eval-repeat-mismatch",
            scoredRows: 10,
            repeatedScores: [0.5d, 0.5d, 0.5d],
            MeasurementNoisePolicyConfig.Canonical));

        Assert.Throws<ArgumentException>(() => new MeasurementNoisePassport(
            Identity,
            "eval-inconsistent",
            scoredRows: 10,
            repeatCount: 5,
            standardDeviationMultiplier: 2d,
            repeatedScores: [0.5d, 0.5d, 0.5d, 0.5d, 0.5d],
            meanScore: 0.6d,
            sampleStandardDeviation: 0d,
            binomialLowerBound: MeasurementNoisePolicy.ComputeAnalyticBinomialLowerBound(0.5d, 10),
            noiseFloor: MeasurementNoisePolicy.ComputeAnalyticBinomialLowerBound(0.5d, 10),
            DateTimeOffset.UnixEpoch));
    }

    [Fact]
    public void ClassifyDifference_ComputedFloorBoundariesRemainStrict()
    {
        double[] scores = [0.2d, 0.4d, 0.6d, 0.8d, 1d];
        var passport = BuildPassport("strict-computed-floor", 100, scores);
        var mean = scores.Sum() / scores.Length;
        var sampleSd = Math.Sqrt(scores.Sum(score => (score - mean) * (score - mean)) /
                                 (scores.Length - 1));
        var binomial = 2d * Math.Sqrt(mean * (1d - mean) / 100d);
        Assert.Equal(Math.Max(2d * sampleSd, binomial), passport.NoiseFloor, 12);
        Assert.True(passport.NoiseFloor > 0d);

        foreach (var difference in new[]
                 { -passport.NoiseFloor, -passport.NoiseFloor / 2d, 0d, passport.NoiseFloor / 2d, passport.NoiseFloor })
        {
            var actual = MeasurementNoisePolicy.ClassifyDifference(difference, passport);
            Assert.Equal(MeasurementNoisePolicy.IndistinguishableStatus, actual.Status);
            Assert.False(actual.DifferenceProven);
        }

        var above = MeasurementNoisePolicy.ClassifyDifference(
            Math.BitIncrement(passport.NoiseFloor), passport);
        var below = MeasurementNoisePolicy.ClassifyDifference(
            Math.BitDecrement(-passport.NoiseFloor), passport);
        Assert.Equal(MeasurementNoisePolicy.ImprovedStatus, above.Status);
        Assert.True(above.DifferenceProven);
        Assert.Equal(MeasurementNoisePolicy.RegressedStatus, below.Status);
        Assert.True(below.DifferenceProven);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(1d)]
    public void ClassifyDifference_ZeroFloorDoesNotProveZeroDifference(double mean)
    {
        var passport = BuildPassport("zero-floor", 5, Enumerable.Repeat(mean, 5).ToArray());
        Assert.Equal(0d, passport.NoiseFloor);
        var zero = MeasurementNoisePolicy.ClassifyDifference(0d, passport);
        Assert.Equal(MeasurementNoisePolicy.IndistinguishableStatus, zero.Status);
        Assert.False(zero.DifferenceProven);
        var positive = MeasurementNoisePolicy.ClassifyDifference(double.Epsilon, passport);
        var negative = MeasurementNoisePolicy.ClassifyDifference(-double.Epsilon, passport);
        Assert.Equal(MeasurementNoisePolicy.ImprovedStatus, positive.Status);
        Assert.True(positive.DifferenceProven);
        Assert.Equal(MeasurementNoisePolicy.RegressedStatus, negative.Status);
        Assert.True(negative.DifferenceProven);
    }

    [Theory]
    [InlineData("nan")]
    [InlineData("positive-infinity")]
    [InlineData("negative-infinity")]
    public void ClassifyDifference_RejectsNonFiniteDifference(string nonFinite)
    {
        var passport = BuildPassport("nonfinite-difference", 5, [0d, 0d, 0d, 0d, 0d]);
        var difference = NonFiniteValue(nonFinite);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MeasurementNoisePolicy.ClassifyDifference(difference, passport));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MeasurementNoisePolicy.ClassifyDifference(difference, passport: null));
    }

    [Theory]
    [InlineData("nan")]
    [InlineData("positive-infinity")]
    [InlineData("negative-infinity")]
    public void BuildPassport_RejectsNonFiniteScores(string nonFinite)
    {
        double[] scores = [0.4d, 0.4d, 0.4d, 0.4d, 0.4d];
        Assert.True(MeasurementNoisePolicy.TryValidatePassport(
            BuildPassport("finite-control", 5, scores), out _));
        scores[2] = NonFiniteValue(nonFinite);
        Assert.Throws<ArgumentException>(() => BuildPassport("nonfinite-score", 5, scores));
    }

    private static double NonFiniteValue(string name)
        => name switch
        {
            "nan" => double.NaN,
            "positive-infinity" => double.PositiveInfinity,
            "negative-infinity" => double.NegativeInfinity,
            _ => throw new ArgumentException("Unknown nonfinite test row.", nameof(name))
        };

    private static MeasurementNoisePassport BuildPassport(
        string evaluationSetId,
        int scoredRows,
        IReadOnlyList<double> repeatedScores)
        => MeasurementNoisePolicy.BuildPassport(
            Identity,
            evaluationSetId,
            scoredRows,
            repeatedScores,
            MeasurementNoisePolicyConfig.Canonical,
            DateTimeOffset.UnixEpoch);

    private static HeldOutScorerIdentity Identity { get; } = new(
        DomainTag: "measurement-noise-tests",
        ScorerId: "held-out-scorer",
        ScorerVersion: "v1",
        NormalizationProfileId: "normalization-v1",
        MetricProfileId: "accuracy-v1");
}
