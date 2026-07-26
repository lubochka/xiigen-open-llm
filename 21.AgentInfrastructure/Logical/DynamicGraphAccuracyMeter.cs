// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Projects live and source-disjoint held-out accuracy into numeric Q30 evidence.
/// This component does not interpret the result as a state transition or lifecycle decision.
/// </summary>
public static class DynamicGraphAccuracyMeter
{
    public const double MinimumHeldOutAccuracy = 0.95d;
    public const double MinimumAccuracyDelta = 0.02d;
    public const double MinimumHeldOutRewardMargin = 0.025d;

    private const string InvalidMeasurementCode = "INVALID_ACCURACY_MEASUREMENT";
    private const string InvalidMeasurementMessage = "Dynamic graph accuracy measurement is invalid.";
    private const string EvaluationMismatchCode = "HELDOUT_EVALUATION_MISMATCH";
    private const string EvaluationMismatchMessage = "Held-out evaluation results do not describe the same evaluation set.";

    /// <summary>
    /// Measures one before/after held-out pair after admitting its source-disjoint split proof.
    /// The supplied held-out accuracy and reward margin are compared without rounding.
    /// Only the derived after-minus-before delta is rounded to twelve decimal places,
    /// away from zero, so binary subtraction artifacts cannot move an exact decimal
    /// delta below its locked boundary.
    /// </summary>
    public static OperationResult<DynamicGraphAccuracyMeasurement> Measure(
        double liveMarginAccuracy,
        LearningEvaluationResult? before,
        LearningEvaluationResult? after,
        DynamicGraphMeasurementSplitProof? splitProof)
    {
        var splitValidation = DynamicGraphHeldOutSplitGuard.Validate(splitProof);
        if (!splitValidation.IsSuccess)
            return splitValidation.MapError<DynamicGraphAccuracyMeasurement>();

        if (before is null
            || after is null
            || !IsCanonicalValue(before.EvaluationId)
            || !IsCanonicalValue(after.EvaluationId)
            || before.RowsEvaluated <= 0
            || after.RowsEvaluated <= 0
            || !IsFiniteAccuracy(liveMarginAccuracy)
            || !IsFiniteAccuracy(before.Accuracy)
            || !IsFiniteAccuracy(after.Accuracy)
            || !double.IsFinite(before.RewardMargin)
            || !double.IsFinite(after.RewardMargin))
        {
            return InvalidMeasurement();
        }

        if (!StringComparer.Ordinal.Equals(before.EvaluationId, after.EvaluationId)
            || before.RowsEvaluated != after.RowsEvaluated)
        {
            return OperationResult<DynamicGraphAccuracyMeasurement>.Fail(
                EvaluationMismatchMessage,
                EvaluationMismatchCode);
        }

        var accuracyDelta = after.Accuracy - before.Accuracy;
        var meetsQ30NumericalGate =
            after.Accuracy >= MinimumHeldOutAccuracy
            && NormalizeForThreshold(accuracyDelta) >= MinimumAccuracyDelta
            && after.RewardMargin >= MinimumHeldOutRewardMargin;

        var accuracy = new DynamicGraphAccuracyAxis(
            liveMarginAccuracy,
            after.Accuracy,
            accuracyDelta);
        var measurement = new DynamicGraphAccuracyMeasurement(
            after.EvaluationId,
            after.RowsEvaluated,
            accuracy,
            after.RewardMargin,
            meetsQ30NumericalGate,
            splitValidation.Value!);
        return OperationResult<DynamicGraphAccuracyMeasurement>.Ok(measurement);
    }

    private static double NormalizeForThreshold(double value)
        => Math.Round(value, 12, MidpointRounding.AwayFromZero);

    private static bool IsFiniteAccuracy(double value)
        => double.IsFinite(value) && value is >= 0d and <= 1d;

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && string.Equals(value, value.Trim(), StringComparison.Ordinal)
           && value.All(character =>
               !char.IsControl(character)
               && character is not '\r'
               && character is not '\n'
               && character is not '\u0085'
               && character is not '\u2028'
               && character is not '\u2029');

    private static OperationResult<DynamicGraphAccuracyMeasurement> InvalidMeasurement()
        => OperationResult<DynamicGraphAccuracyMeasurement>.Fail(
            InvalidMeasurementMessage,
            InvalidMeasurementCode);
}
