// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ContinuousLearning;

public sealed record MeasurementNoisePolicyConfig
{
    public const string IdentitySchema =
        "xiigen.measurement-noise-policy.v1";
    public const int DefaultRepeatCount = 5;
    public const double DefaultStandardDeviationMultiplier = 2d;

    public MeasurementNoisePolicyConfig(
        int repeatCount = DefaultRepeatCount,
        double standardDeviationMultiplier = DefaultStandardDeviationMultiplier)
    {
        if (repeatCount < 2)
            throw new ArgumentOutOfRangeException(nameof(repeatCount));
        if (!double.IsFinite(standardDeviationMultiplier) ||
            standardDeviationMultiplier <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(standardDeviationMultiplier));
        }

        RepeatCount = repeatCount;
        StandardDeviationMultiplier = standardDeviationMultiplier;
    }

    public int RepeatCount { get; }
    public double StandardDeviationMultiplier { get; }

    /// <summary>
    /// Stable identity of the server-owned uncertainty policy. The identity
    /// is part of the exam contract; equal scores under a different policy
    /// are not comparable evidence.
    /// </summary>
    public string IdentitySha256 => ComputeIdentitySha256(this);

    public static string ComputeIdentitySha256(
        MeasurementNoisePolicyConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var canonical = string.Join(
            "\n",
            IdentitySchema,
            config.RepeatCount.ToString(CultureInfo.InvariantCulture),
            config.StandardDeviationMultiplier.ToString(
                "R",
                CultureInfo.InvariantCulture));
        return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    public static MeasurementNoisePolicyConfig Canonical { get; } = new();
}

public sealed record MeasurementNoisePassport
{
    public MeasurementNoisePassport(
        HeldOutScorerIdentity scorerIdentity,
        string evaluationSetId,
        int scoredRows,
        int repeatCount,
        double standardDeviationMultiplier,
        IReadOnlyList<double> repeatedScores,
        double meanScore,
        double sampleStandardDeviation,
        double binomialLowerBound,
        double noiseFloor,
        DateTimeOffset calibratedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(scorerIdentity);
        var repeatedScoresSnapshot =
            repeatedScores?.ToArray() ??
            throw new ArgumentNullException(nameof(repeatedScores));
        var calibratedAtUtcValue = calibratedAtUtc.ToUniversalTime();

        if (!MeasurementNoisePolicy.TryValidatePassportValues(
                scorerIdentity,
                evaluationSetId,
                scoredRows,
                repeatCount,
                standardDeviationMultiplier,
                repeatedScoresSnapshot,
                meanScore,
                sampleStandardDeviation,
                binomialLowerBound,
                noiseFloor,
                out var reason))
        {
            throw new ArgumentException(reason, nameof(repeatedScores));
        }

        ScorerIdentity = scorerIdentity;
        EvaluationSetId = evaluationSetId;
        ScoredRows = scoredRows;
        RepeatCount = repeatCount;
        StandardDeviationMultiplier = standardDeviationMultiplier;
        RepeatedScores = repeatedScoresSnapshot;
        MeanScore = meanScore;
        SampleStandardDeviation = sampleStandardDeviation;
        BinomialLowerBound = binomialLowerBound;
        NoiseFloor = noiseFloor;
        CalibratedAtUtc = calibratedAtUtcValue;
    }

    public HeldOutScorerIdentity ScorerIdentity { get; }
    public string EvaluationSetId { get; }
    public int ScoredRows { get; }
    public int RepeatCount { get; }
    public double StandardDeviationMultiplier { get; }
    public IReadOnlyList<double> RepeatedScores { get; }
    public double MeanScore { get; }
    public double SampleStandardDeviation { get; }
    public double BinomialLowerBound { get; }
    public double NoiseFloor { get; }
    public DateTimeOffset CalibratedAtUtc { get; }
}

public sealed record MeasurementNoiseComparison(
    double Difference,
    double? NoiseFloor,
    string Status,
    bool DifferenceProven);

public static class MeasurementNoisePolicy
{
    public const string ImprovedStatus = "improved_beyond_measurement_noise";
    public const string RegressedStatus = "regressed_beyond_measurement_noise";
    public const string IndistinguishableStatus =
        "indistinguishable_within_measurement_noise";
    public const string DifferenceNotProvenNoCalibrationStatus =
        "difference_not_proven_no_calibration";
    public const string InvalidPassportStatus =
        "measurement_noise_passport_invalid";

    private const double NumericTolerance = 1e-12;
    private const int MaximumIdentifierChars = 256;

    public static MeasurementNoisePassport BuildPassport(
        HeldOutScorerIdentity scorerIdentity,
        string evaluationSetId,
        int scoredRows,
        IReadOnlyList<double> repeatedScores,
        MeasurementNoisePolicyConfig? config = null,
        DateTimeOffset? calibratedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(scorerIdentity);
        ArgumentNullException.ThrowIfNull(repeatedScores);
        config ??= MeasurementNoisePolicyConfig.Canonical;

        if (repeatedScores.Count != config.RepeatCount)
        {
            // Паспорт привязан к заявленной политике повторов: иначе нельзя
            // понять, это недомер или чужая калибровка с другим прибором.
            throw new ArgumentException(
                "Repeated score count must match measurement noise policy repeat count.",
                nameof(repeatedScores));
        }

        var mean = repeatedScores.Average();
        var sampleStandardDeviation =
            ComputeSampleStandardDeviation(repeatedScores);
        var binomialLowerBound = ComputeAnalyticBinomialLowerBound(
            mean,
            scoredRows);
        var noiseFloor = Math.Max(
            config.StandardDeviationMultiplier * sampleStandardDeviation,
            binomialLowerBound);

        return new MeasurementNoisePassport(
            scorerIdentity,
            evaluationSetId,
            scoredRows,
            config.RepeatCount,
            config.StandardDeviationMultiplier,
            repeatedScores,
            mean,
            sampleStandardDeviation,
            binomialLowerBound,
            noiseFloor,
            calibratedAtUtc ?? DateTimeOffset.UtcNow);
    }

    public static double ComputeSampleStandardDeviation(
        IReadOnlyList<double> repeatedScores)
    {
        ArgumentNullException.ThrowIfNull(repeatedScores);
        if (repeatedScores.Count < 2 || repeatedScores.Any(static score => !IsProbability(score)))
            throw new ArgumentException(
                "At least two finite probability scores are required.",
                nameof(repeatedScores));

        var mean = repeatedScores.Average();
        var variance = repeatedScores.Sum(score => Math.Pow(score - mean, 2d)) /
                       (repeatedScores.Count - 1);
        return Math.Sqrt(Math.Max(0d, variance));
    }

    public static double ComputeAnalyticBinomialLowerBound(
        double probability,
        int rowCount)
    {
        if (!IsProbability(probability))
            throw new ArgumentOutOfRangeException(nameof(probability));
        if (rowCount < 1)
            throw new ArgumentOutOfRangeException(nameof(rowCount));

        return 2d * Math.Sqrt(probability * (1d - probability) / rowCount);
    }

    public static MeasurementNoiseComparison ClassifyDifference(
        double difference,
        MeasurementNoisePassport? passport)
    {
        if (!double.IsFinite(difference))
            throw new ArgumentOutOfRangeException(nameof(difference));
        if (passport is null)
        {
            return new MeasurementNoiseComparison(
                difference,
                NoiseFloor: null,
                DifferenceNotProvenNoCalibrationStatus,
                DifferenceProven: false);
        }

        if (!TryValidatePassport(passport, out _))
        {
            return new MeasurementNoiseComparison(
                difference,
                passport.NoiseFloor,
                InvalidPassportStatus,
                DifferenceProven: false);
        }

        if (difference > passport.NoiseFloor)
        {
            return new MeasurementNoiseComparison(
                difference,
                passport.NoiseFloor,
                ImprovedStatus,
                DifferenceProven: true);
        }

        if (difference < -passport.NoiseFloor)
        {
            return new MeasurementNoiseComparison(
                difference,
                passport.NoiseFloor,
                RegressedStatus,
                DifferenceProven: true);
        }

        // Равенство полу погрешности не доказывает направление: это граница
        // измерительного прибора, а не выигрыш модели.
        return new MeasurementNoiseComparison(
            difference,
            passport.NoiseFloor,
            IndistinguishableStatus,
            DifferenceProven: false);
    }

    public static bool TryValidatePassport(
        MeasurementNoisePassport? passport,
        out string reason)
    {
        if (passport is null)
        {
            reason = DifferenceNotProvenNoCalibrationStatus;
            return false;
        }

        return TryValidatePassportValues(
            passport.ScorerIdentity,
            passport.EvaluationSetId,
            passport.ScoredRows,
            passport.RepeatCount,
            passport.StandardDeviationMultiplier,
            passport.RepeatedScores,
            passport.MeanScore,
            passport.SampleStandardDeviation,
            passport.BinomialLowerBound,
            passport.NoiseFloor,
            out reason);
    }

    public static bool TryValidatePassportForMeasurement(
        MeasurementNoisePassport? passport,
        HeldOutScorerIdentity? expectedScorerIdentity,
        string expectedEvaluationSetId,
        int expectedScoredRows,
        double expectedMeanScore,
        out string reason)
    {
        if (!TryValidatePassport(passport, out reason))
            return false;
        if (!ScorerIdentitiesMatch(
                passport!.ScorerIdentity,
                expectedScorerIdentity))
        {
            // Паспорт калибрует не просто число, а конкретный прибор:
            // scorer/version/normalization/metric должны совпадать с замером.
            reason = "measurement_noise_passport_scorer_identity_mismatch";
            return false;
        }

        if (!string.Equals(
                passport.EvaluationSetId,
                expectedEvaluationSetId,
                StringComparison.Ordinal))
        {
            // Чужой паспорт нельзя применять к этой выборке: пол шума зависит
            // от набора строк и от того, что именно мерил скорер.
            reason = "measurement_noise_passport_foreign_evaluation_set";
            return false;
        }

        if (passport.ScoredRows != expectedScoredRows)
        {
            reason = "measurement_noise_passport_row_count_mismatch";
            return false;
        }

        if (!AlmostEqual(passport.MeanScore, expectedMeanScore))
        {
            reason = "measurement_noise_passport_mean_score_mismatch";
            return false;
        }

        return true;
    }

    public static bool ScorerIdentitiesMatch(
        HeldOutScorerIdentity? actual,
        HeldOutScorerIdentity? expected)
        => ScorerIdentityIsValid(actual) &&
           ScorerIdentityIsValid(expected) &&
           string.Equals(actual!.DomainTag, expected!.DomainTag, StringComparison.Ordinal) &&
           string.Equals(actual.ScorerId, expected.ScorerId, StringComparison.Ordinal) &&
           string.Equals(actual.ScorerVersion, expected.ScorerVersion, StringComparison.Ordinal) &&
           string.Equals(actual.NormalizationProfileId, expected.NormalizationProfileId, StringComparison.Ordinal) &&
           string.Equals(actual.MetricProfileId, expected.MetricProfileId, StringComparison.Ordinal);

    internal static bool TryValidatePassportValues(
        HeldOutScorerIdentity? scorerIdentity,
        string? evaluationSetId,
        int scoredRows,
        int repeatCount,
        double standardDeviationMultiplier,
        IReadOnlyList<double>? repeatedScores,
        double meanScore,
        double sampleStandardDeviation,
        double binomialLowerBound,
        double noiseFloor,
        out string reason)
    {
        if (!ScorerIdentityIsValid(scorerIdentity) ||
            !IsSafeIdentity(evaluationSetId) ||
            scoredRows < 1 ||
            repeatCount < 2 ||
            repeatedScores is null ||
            repeatedScores.Count != repeatCount ||
            !double.IsFinite(standardDeviationMultiplier) ||
            standardDeviationMultiplier <= 0d ||
            !IsProbability(meanScore) ||
            !double.IsFinite(sampleStandardDeviation) ||
            sampleStandardDeviation < 0d ||
            !double.IsFinite(binomialLowerBound) ||
            binomialLowerBound < 0d ||
            !double.IsFinite(noiseFloor) ||
            noiseFloor < 0d ||
            repeatedScores.Any(static score => !IsProbability(score)))
        {
            reason = InvalidPassportStatus;
            return false;
        }

        var expectedMean = repeatedScores.Average();
        var expectedSampleStdDev =
            ComputeSampleStandardDeviation(repeatedScores);
        var expectedBinomialLowerBound =
            ComputeAnalyticBinomialLowerBound(expectedMean, scoredRows);
        var expectedNoiseFloor = Math.Max(
            standardDeviationMultiplier * expectedSampleStdDev,
            expectedBinomialLowerBound);
        if (!AlmostEqual(meanScore, expectedMean) ||
            !AlmostEqual(sampleStandardDeviation, expectedSampleStdDev) ||
            !AlmostEqual(binomialLowerBound, expectedBinomialLowerBound) ||
            !AlmostEqual(noiseFloor, expectedNoiseFloor))
        {
            reason = "measurement_noise_passport_inconsistent";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool IsProbability(double value)
        => double.IsFinite(value) && value is >= 0d and <= 1d;

    private static bool ScorerIdentityIsValid(HeldOutScorerIdentity? scorerIdentity)
        => scorerIdentity is not null &&
           IsSafeIdentity(scorerIdentity.DomainTag) &&
           IsSafeIdentity(scorerIdentity.ScorerId) &&
           IsSafeIdentity(scorerIdentity.ScorerVersion) &&
           IsSafeIdentity(scorerIdentity.NormalizationProfileId) &&
           IsSafeIdentity(scorerIdentity.MetricProfileId);

    private static bool IsSafeIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumIdentifierChars &&
           string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
           value.All(static ch => char.IsAsciiLetterOrDigit(ch) ||
                                  ch is '-' or '_' or '.' or ':');

    private static bool AlmostEqual(double left, double right)
    {
        var scale = Math.Max(1d, Math.Max(Math.Abs(left), Math.Abs(right)));
        return Math.Abs(left - right) <= NumericTolerance * scale;
    }
}
