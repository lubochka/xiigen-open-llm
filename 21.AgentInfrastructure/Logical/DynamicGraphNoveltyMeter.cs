// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Projects already-produced held-out selection evidence into numeric novelty evidence.
/// It does not select rows, calculate pattern or token rates, run training, or decide
/// promotion, full quality, or lifecycle state.
/// </summary>
public static class DynamicGraphNoveltyMeter
{
    private const string InvalidMeasurementCode = "INVALID_NOVELTY_MEASUREMENT";
    private const string InvalidMeasurementMessage = "Dynamic graph novelty measurement is invalid.";

    public static OperationResult<DynamicGraphNoveltyMeasurement> Measure(
        DynamicGraphNoveltyObservation? observation)
    {
        if (observation is null)
            return InvalidMeasurement();

        var splitValidation = DynamicGraphHeldOutSplitGuard.Validate(observation.SplitProof);
        if (!splitValidation.IsSuccess)
            return splitValidation.MapError<DynamicGraphNoveltyMeasurement>();

        if (!IsCanonicalValue(observation.EvaluationId)
            || observation.RequestedRows <= 0
            || observation.RowsEvaluated < 0
            || observation.RowsEvaluated > observation.RequestedRows
            || observation.SelectionShortfall < 0
            || observation.SelectionShortfall != observation.RequestedRows - observation.RowsEvaluated
            || !IsFiniteRate(observation.NewPatternRate)
            || !IsFiniteRate(observation.SourceTokenSeenRate)
            || !IsFiniteRate(observation.MaxAccuracy))
        {
            return InvalidMeasurement();
        }

        var minimumNewPatternRate = 1d - observation.MaxAccuracy;
        var maximumSeenTokenRate = observation.MaxAccuracy;
        var meetsNoveltyGate = observation.SelectionShortfall == 0
                               && observation.RowsEvaluated == observation.RequestedRows
                               && observation.NewPatternRate > minimumNewPatternRate
                               && observation.SourceTokenSeenRate < maximumSeenTokenRate;

        return OperationResult<DynamicGraphNoveltyMeasurement>.Ok(
            new DynamicGraphNoveltyMeasurement(
                observation.EvaluationId,
                observation.RequestedRows,
                observation.RowsEvaluated,
                observation.SelectionShortfall,
                observation.NewPatternRate,
                observation.SourceTokenSeenRate,
                observation.MaxAccuracy,
                minimumNewPatternRate,
                maximumSeenTokenRate,
                meetsNoveltyGate,
                splitValidation.Value!));
    }

    private static bool IsFiniteRate(double value)
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

    private static OperationResult<DynamicGraphNoveltyMeasurement> InvalidMeasurement()
        => OperationResult<DynamicGraphNoveltyMeasurement>.Fail(
            InvalidMeasurementMessage,
            InvalidMeasurementCode);
}
