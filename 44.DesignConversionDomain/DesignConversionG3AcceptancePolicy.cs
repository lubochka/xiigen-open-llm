// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Text.Json.Serialization;
using ContinuousLearning;

/// <summary>
/// Canonical DEC-03 admission configuration. These values govern trained
/// checkpoint authority only; measurement and tariff thresholds are separate
/// policy axes.
/// </summary>
public sealed record DesignConversionG3AcceptancePolicyConfig
{
    public DesignConversionG3AcceptancePolicyConfig(
        int minimumHeldOutCount = DesignConversionG3AcceptancePolicy.CanonicalMinimumHeldOutCount,
        double minimumBaselineWinRate = DesignConversionG3AcceptancePolicy.CanonicalMinimumBaselineWinRate,
        double smallSampleSignificanceLevel = DesignConversionG3AcceptancePolicy.CanonicalSmallSampleSignificanceLevel,
        double absoluteAccuracyFloor = DesignConversionG3AcceptancePolicy.CanonicalAbsoluteAccuracyFloor)
    {
        if (minimumHeldOutCount < 1)
            throw new ArgumentOutOfRangeException(nameof(minimumHeldOutCount));
        if (!DesignConversionG3AcceptancePolicy.IsFiniteProbability(minimumBaselineWinRate))
            throw new ArgumentOutOfRangeException(nameof(minimumBaselineWinRate));
        if (!double.IsFinite(smallSampleSignificanceLevel) ||
            smallSampleSignificanceLevel is <= 0d or >= 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(smallSampleSignificanceLevel));
        }
        if (!DesignConversionG3AcceptancePolicy.IsFiniteProbability(absoluteAccuracyFloor))
            throw new ArgumentOutOfRangeException(nameof(absoluteAccuracyFloor));

        MinimumHeldOutCount = minimumHeldOutCount;
        MinimumBaselineWinRate = minimumBaselineWinRate;
        SmallSampleSignificanceLevel = smallSampleSignificanceLevel;
        AbsoluteAccuracyFloor = absoluteAccuracyFloor;
    }

    public int MinimumHeldOutCount { get; }
    public double MinimumBaselineWinRate { get; }
    public double SmallSampleSignificanceLevel { get; }
    public double AbsoluteAccuracyFloor { get; }

    public static DesignConversionG3AcceptancePolicyConfig Canonical { get; } = new();
}

/// <summary>
/// The two independent held-out measurements required by DEC-03. Baseline
/// wins are comparative (child margin beats reference margin); absolute
/// correctness requires the child's chosen margin itself to be strictly
/// positive. Ties therefore fail both counts.
/// </summary>
public sealed record DesignConversionG3AcceptanceMetrics(
    int HeldOutCount,
    int BaselineWinCount,
    double BaselineWinRate,
    int AbsoluteChildPreferenceCorrectCount,
    double AbsoluteChildPreferenceAccuracy)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HeldOutScorerIdentity? MeasurementNoiseScorerIdentity { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MeasurementNoiseEvaluationSetId { get; init; }
}

public sealed record DesignConversionG3AcceptanceEvidenceChain(
    bool TypedProofVerified,
    bool FreshLoadVerified,
    bool ContinuedTrainingVerified,
    bool ImmediateParentLineageVerified);

public sealed record DesignConversionG3AcceptanceResult(
    bool Eligible,
    string Reason,
    DesignConversionG3AcceptanceMetrics Metrics,
    DesignConversionG3AcceptanceEvidenceChain EvidenceChain,
    DesignConversionG3AcceptancePolicyConfig Config,
    bool MetricsValid,
    bool ComparativeGatePassed,
    bool AbsoluteGatePassed,
    double? OneSidedBinomialTailProbability)
{
    public double? MeasurementNoiseFloor { get; init; }
    public string MeasurementNoiseStatus { get; init; } =
        MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus;
}

/// <summary>
/// Pure DEC-03 G3 admission rule. It owns no mutable/global state and does not
/// consult machine-evaluation, measurement-gate, tariff, or runtime settings.
/// </summary>
public static class DesignConversionG3AcceptancePolicy
{
    public const int CanonicalMinimumHeldOutCount = 50;
    public const int CanonicalLargeSampleHeldOutCount = 200;
    public const double CanonicalMinimumBaselineWinRate = 0.55d;
    public const double CanonicalSmallSampleSignificanceLevel = 0.05d;
    public const double CanonicalAbsoluteAccuracyFloor = 0.75d;

    public const string EligibleReason = "g3_eligible";
    public const string InvalidMetricsReason = "g3_metrics_invalid";
    public const string TypedProofMissingReason = "g3_typed_proof_missing";
    public const string FreshLoadMissingReason = "g3_fresh_load_missing";
    public const string ContinuedTrainingMissingReason = "g3_continued_training_missing";
    public const string ImmediateParentLineageInvalidReason = "g3_immediate_parent_lineage_invalid";
    public const string HeldOutBelowMinimumReason = "g3_heldout_below_minimum";
    public const string BaselineWinRateBelowFloorReason = "g3_baseline_win_rate_below_floor";
    public const string SmallSampleSignificanceNotMetReason = "g3_small_sample_significance_not_met";
    public const string AbsoluteAccuracyBelowFloorReason = "g3_absolute_accuracy_below_floor";
    public const string MeasurementNoisePassportInvalidReason = "g3_measurement_noise_passport_invalid";
    public const string BaselineWinInsideMeasurementNoiseReason = "g3_gain_inside_measurement_noise_floor";

    public static DesignConversionG3AcceptanceResult Evaluate(
        DesignConversionG3AcceptanceMetrics metrics,
        DesignConversionG3AcceptanceEvidenceChain evidenceChain,
        DesignConversionG3AcceptancePolicyConfig? config = null,
        MeasurementNoisePassport? measurementNoisePassport = null)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(evidenceChain);
        config ??= DesignConversionG3AcceptancePolicyConfig.Canonical;

        if (!MetricsAreValid(metrics))
            return Result(false, InvalidMetricsReason, metrics, evidenceChain, config, false, false, false, null);
        var noiseComparison = MeasurementNoisePolicy.ClassifyDifference(
            metrics.BaselineWinRate - config.MinimumBaselineWinRate,
            measurementNoisePassport);
        if (measurementNoisePassport is not null &&
            !NoisePassportMatchesG3Metrics(
                measurementNoisePassport,
                metrics,
                out _))
        {
            return Result(
                false,
                MeasurementNoisePassportInvalidReason,
                metrics,
                evidenceChain,
                config,
                true,
                false,
                false,
                null,
                noiseComparison with
                {
                    Status = MeasurementNoisePolicy.InvalidPassportStatus,
                    DifferenceProven = false
                });
        }
        if (!evidenceChain.TypedProofVerified)
            return Result(false, TypedProofMissingReason, metrics, evidenceChain, config, true, false, false, null);
        if (!evidenceChain.FreshLoadVerified)
            return Result(false, FreshLoadMissingReason, metrics, evidenceChain, config, true, false, false, null);
        if (!evidenceChain.ContinuedTrainingVerified)
            return Result(false, ContinuedTrainingMissingReason, metrics, evidenceChain, config, true, false, false, null);
        if (!evidenceChain.ImmediateParentLineageVerified)
            return Result(false, ImmediateParentLineageInvalidReason, metrics, evidenceChain, config, true, false, false, null);
        if (metrics.HeldOutCount < config.MinimumHeldOutCount)
            return Result(false, HeldOutBelowMinimumReason, metrics, evidenceChain, config, true, false, false, null);
        if (metrics.BaselineWinRate < config.MinimumBaselineWinRate)
            return Result(false, BaselineWinRateBelowFloorReason, metrics, evidenceChain, config, true, false, false, null);

        double? tailProbability = null;
        if (metrics.HeldOutCount < CanonicalLargeSampleHeldOutCount)
        {
            tailProbability = ComputeOneSidedBinomialTailProbability(
                metrics.HeldOutCount,
                metrics.BaselineWinCount);
            if (!(tailProbability < config.SmallSampleSignificanceLevel))
            {
                return Result(
                    false,
                    SmallSampleSignificanceNotMetReason,
                    metrics,
                    evidenceChain,
                    config,
                    true,
                    false,
                    metrics.AbsoluteChildPreferenceAccuracy >= config.AbsoluteAccuracyFloor,
                    tailProbability,
                    noiseComparison);
            }
        }

        if (measurementNoisePassport is not null &&
            !noiseComparison.DifferenceProven)
        {
            // Биномиальный хвост уже остался на своём месте; эта проверка
            // добавляет второй, приборный пол и требует выигрыш строго выше его.
            return Result(
                false,
                BaselineWinInsideMeasurementNoiseReason,
                metrics,
                evidenceChain,
                config,
                true,
                false,
                metrics.AbsoluteChildPreferenceAccuracy >= config.AbsoluteAccuracyFloor,
                tailProbability,
                noiseComparison);
        }

        if (metrics.AbsoluteChildPreferenceAccuracy < config.AbsoluteAccuracyFloor)
        {
            return Result(
                false,
                AbsoluteAccuracyBelowFloorReason,
                metrics,
                evidenceChain,
                config,
                true,
                measurementNoisePassport is null || noiseComparison.DifferenceProven,
                false,
                tailProbability,
                noiseComparison);
        }

        return Result(
            true,
            EligibleReason,
            metrics,
            evidenceChain,
            config,
            true,
            true,
            true,
            tailProbability,
            noiseComparison);
    }

    public static double ComputeOneSidedBinomialTailProbability(
        int trialCount,
        int wins)
    {
        if (trialCount < 1)
            throw new ArgumentOutOfRangeException(nameof(trialCount));
        if (wins < 0 || wins > trialCount)
            throw new ArgumentOutOfRangeException(nameof(wins));

        // DEC-03 applies this exact tail only below 200 observations. Starting
        // at P(X=0)=2^-N and using the adjacent-PMF recurrence avoids factorial
        // overflow and remains stable throughout the canonical range.
        var probability = Math.ScaleB(1d, -trialCount);
        var tail = wins == 0 ? probability : 0d;
        for (var k = 0; k < trialCount; k++)
        {
            probability *= (double)(trialCount - k) / (k + 1);
            if (k + 1 >= wins)
                tail += probability;
        }

        return Math.Clamp(tail, 0d, 1d);
    }

    internal static bool MetricsAreValid(DesignConversionG3AcceptanceMetrics metrics)
        => metrics.HeldOutCount > 0 &&
           metrics.BaselineWinCount >= 0 &&
           metrics.BaselineWinCount <= metrics.HeldOutCount &&
           IsFiniteProbability(metrics.BaselineWinRate) &&
           Math.Abs(metrics.BaselineWinRate -
                    (double)metrics.BaselineWinCount / metrics.HeldOutCount) <= 1e-12 &&
           metrics.AbsoluteChildPreferenceCorrectCount >= 0 &&
           metrics.AbsoluteChildPreferenceCorrectCount <= metrics.HeldOutCount &&
           IsFiniteProbability(metrics.AbsoluteChildPreferenceAccuracy) &&
           Math.Abs(metrics.AbsoluteChildPreferenceAccuracy -
                    (double)metrics.AbsoluteChildPreferenceCorrectCount / metrics.HeldOutCount) <= 1e-12;

    internal static bool MeasurementNoiseScopeMatches(
        DesignConversionG3AcceptanceMetrics? metrics,
        HeldOutScorerIdentity? expectedScorerIdentity,
        string? expectedEvaluationSetId)
        => metrics is not null &&
           MeasurementNoisePolicy.ScorerIdentitiesMatch(
               metrics.MeasurementNoiseScorerIdentity,
               expectedScorerIdentity) &&
           string.Equals(
               metrics.MeasurementNoiseEvaluationSetId,
               expectedEvaluationSetId,
               StringComparison.Ordinal);

    internal static bool IsFiniteProbability(double value)
        => double.IsFinite(value) && value is >= 0d and <= 1d;

    private static DesignConversionG3AcceptanceResult Result(
        bool eligible,
        string reason,
        DesignConversionG3AcceptanceMetrics metrics,
        DesignConversionG3AcceptanceEvidenceChain evidenceChain,
        DesignConversionG3AcceptancePolicyConfig config,
        bool metricsValid,
        bool comparativeGatePassed,
        bool absoluteGatePassed,
        double? tailProbability,
        MeasurementNoiseComparison? noiseComparison = null)
    {
        var result = new DesignConversionG3AcceptanceResult(
            eligible,
            reason,
            metrics,
            evidenceChain,
            config,
            metricsValid,
            comparativeGatePassed,
            absoluteGatePassed,
            tailProbability);
        return noiseComparison is null
            ? result
            : result with
            {
                MeasurementNoiseFloor = noiseComparison.NoiseFloor,
                MeasurementNoiseStatus = noiseComparison.Status
            };
    }

    private static bool NoisePassportMatchesG3Metrics(
        MeasurementNoisePassport passport,
        DesignConversionG3AcceptanceMetrics metrics,
        out string reason)
        => MeasurementNoisePolicy.TryValidatePassportForMeasurement(
            passport,
            metrics.MeasurementNoiseScorerIdentity,
            metrics.MeasurementNoiseEvaluationSetId ?? string.Empty,
            metrics.HeldOutCount,
            metrics.BaselineWinRate,
            out reason);
}
