// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Compares source-disjoint held-out accuracy for a subject and an ablated counterpart.
/// The result is numeric evidence only; it does not prove the ablated runtime state,
/// novelty, source-id independence, named controls, trained state, promotion, or lifecycle.
/// </summary>
public static class DynamicGraphAblationMeter
{
    public const double MinimumAccuracyDrop = 0.02d;

    private const string InvalidMeasurementCode = "INVALID_ABLATION_MEASUREMENT";
    private const string InvalidMeasurementMessage = "Dynamic graph ablation measurement is invalid.";
    private const string EvaluationMismatchCode = "ABLATION_EVALUATION_MISMATCH";
    private const string EvaluationMismatchMessage = "Dynamic graph ablation measurements do not describe the same held-out evaluation.";

    /// <summary>
    /// Measures the raw held-out accuracy loss caused by the supplied ablation evidence.
    /// Both split proofs are admitted before either measurement shape or identity is compared.
    /// </summary>
    public static OperationResult<DynamicGraphAblationMeasurement> Measure(
        DynamicGraphAccuracyMeasurement? subject,
        DynamicGraphAccuracyMeasurement? ablated)
    {
        if (subject is null || ablated is null)
            return InvalidMeasurement();

        var subjectSplitValidation = DynamicGraphHeldOutSplitGuard.Validate(subject.SplitProof);
        if (!subjectSplitValidation.IsSuccess)
            return subjectSplitValidation.MapError<DynamicGraphAblationMeasurement>();

        var ablatedSplitValidation = DynamicGraphHeldOutSplitGuard.Validate(ablated.SplitProof);
        if (!ablatedSplitValidation.IsSuccess)
            return ablatedSplitValidation.MapError<DynamicGraphAblationMeasurement>();

        if (!IsValidMeasurementShape(subject) || !IsValidMeasurementShape(ablated))
            return InvalidMeasurement();

        var subjectSplit = subjectSplitValidation.Value!;
        var ablatedSplit = ablatedSplitValidation.Value!;
        if (!StringComparer.Ordinal.Equals(subject.EvaluationId, ablated.EvaluationId)
            || subject.RowsEvaluated != ablated.RowsEvaluated
            || !SplitProofsMatch(subjectSplit, ablatedSplit))
        {
            return OperationResult<DynamicGraphAblationMeasurement>.Fail(
                EvaluationMismatchMessage,
                EvaluationMismatchCode);
        }

        var accuracyDrop = subject.Accuracy.HeldOutAccuracy - ablated.Accuracy.HeldOutAccuracy;
        var meetsQ11NumericalGate =
            Math.Round(accuracyDrop, 12, MidpointRounding.AwayFromZero) >= MinimumAccuracyDrop;

        return OperationResult<DynamicGraphAblationMeasurement>.Ok(
            new DynamicGraphAblationMeasurement(
                subject.EvaluationId,
                subject.RowsEvaluated,
                subject.Accuracy.HeldOutAccuracy,
                ablated.Accuracy.HeldOutAccuracy,
                accuracyDrop,
                meetsQ11NumericalGate,
                subjectSplit));
    }

    private static bool IsValidMeasurementShape(DynamicGraphAccuracyMeasurement measurement)
        => IsCanonicalValue(measurement.EvaluationId)
           && measurement.RowsEvaluated > 0
           && measurement.Accuracy is not null
           && IsFiniteAccuracy(measurement.Accuracy.LiveMarginAccuracy)
           && IsFiniteAccuracy(measurement.Accuracy.HeldOutAccuracy)
           && double.IsFinite(measurement.Accuracy.AccuracyDelta)
           && measurement.Accuracy.AccuracyDelta is >= -1d and <= 1d
           && double.IsFinite(measurement.HeldOutRewardMargin);

    private static bool SplitProofsMatch(
        DynamicGraphMeasurementSplitProof subject,
        DynamicGraphMeasurementSplitProof ablated)
        => subject.TrainSourceRoots.SequenceEqual(ablated.TrainSourceRoots, StringComparer.Ordinal)
           && subject.EvalSourceRoots.SequenceEqual(ablated.EvalSourceRoots, StringComparer.Ordinal)
           && subject.SourceHashes.SequenceEqual(ablated.SourceHashes, StringComparer.Ordinal)
           && subject.CrossSplitNearDuplicateCount == ablated.CrossSplitNearDuplicateCount
           && StringComparer.Ordinal.Equals(subject.SimhashOrMinhash, ablated.SimhashOrMinhash)
           && subject.NearDuplicateThreshold.Equals(ablated.NearDuplicateThreshold);

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

    private static OperationResult<DynamicGraphAblationMeasurement> InvalidMeasurement()
        => OperationResult<DynamicGraphAblationMeasurement>.Fail(
            InvalidMeasurementMessage,
            InvalidMeasurementCode);
}
