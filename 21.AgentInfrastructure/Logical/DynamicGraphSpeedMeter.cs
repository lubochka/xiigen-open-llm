// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Projects the resource cost of an already-produced learning batch without
/// sampling the running process or interpreting training quality.
/// </summary>
public static class DynamicGraphSpeedMeter
{
    private const string InvalidMeasurementCode = "INVALID_TRAINING_SPEED_MEASUREMENT";
    private const string InvalidMeasurementMessage = "Dynamic graph training speed measurement is invalid.";

    public static OperationResult<DynamicGraphTrainingSpeedMeasurement> Measure(
        LearningBatchResult? result)
    {
        if (result is null
            || !IsCanonicalValue(result.BatchId)
            || result.RowsTrained <= 0
            || result.CpuMs < 0
            || result.PeakRamMb < 0
            || result.ElapsedMs < 0)
        {
            return InvalidMeasurement();
        }

        var speed = new DynamicGraphTrainingSpeedAxis(
            result.CpuMs,
            (double)result.PeakRamMb,
            result.ElapsedMs,
            result.RowsTrained);
        var measurement = new DynamicGraphTrainingSpeedMeasurement(result.BatchId, speed);
        return OperationResult<DynamicGraphTrainingSpeedMeasurement>.Ok(measurement);
    }

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

    private static OperationResult<DynamicGraphTrainingSpeedMeasurement> InvalidMeasurement()
        => OperationResult<DynamicGraphTrainingSpeedMeasurement>.Fail(
            InvalidMeasurementMessage,
            InvalidMeasurementCode);
}
