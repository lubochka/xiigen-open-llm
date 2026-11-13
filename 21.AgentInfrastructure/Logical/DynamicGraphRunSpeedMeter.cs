// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Projects run timing from an already-completed observation. The meter owns
/// snapshots of both sample collections before it validates or calculates any
/// timing values.
/// </summary>
public static class DynamicGraphRunSpeedMeter
{
    private const string InvalidObservationCode = "INVALID_RUN_SPEED_OBSERVATION";
    private const string InvalidObservationMessage = "Dynamic graph run speed observation is invalid.";
    private const string OverflowCode = "RUN_SPEED_MEASUREMENT_OVERFLOW";
    private const string OverflowMessage = "Dynamic graph run speed measurement overflowed.";

    public static OperationResult<DynamicGraphRunSpeedMeasurement> Measure(
        DynamicGraphRunSpeedObservation? observation)
    {
        if (observation is null
            || observation.CandidateElapsedMsSamples is null
            || observation.BatchElapsedMsSamples is null)
        {
            return InvalidObservation();
        }

        IReadOnlyList<long> candidateSamples;
        IReadOnlyList<long> batchSamples;
        try
        {
            candidateSamples = Array.AsReadOnly(observation.CandidateElapsedMsSamples.ToArray());
            batchSamples = Array.AsReadOnly(observation.BatchElapsedMsSamples.ToArray());
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidObservation();
        }

        if (observation.EvaluatedRowCount <= 0
            || observation.InferenceElapsedMs <= 0
            || observation.RoutingElapsedMs < 0
            || candidateSamples.Any(sample => sample < 0)
            || batchSamples.Any(sample => sample < 0))
        {
            return InvalidObservation();
        }

        long? perCandidateElapsedMs;
        long? perBatchElapsedMs;
        try
        {
            perCandidateElapsedMs = AverageWholeMilliseconds(candidateSamples);
            perBatchElapsedMs = AverageWholeMilliseconds(batchSamples);
        }
        catch (OverflowException)
        {
            return Overflow();
        }

        var throughputRowsPerSec = observation.EvaluatedRowCount * 1000d
                                   / observation.InferenceElapsedMs;
        if (!double.IsFinite(throughputRowsPerSec) || throughputRowsPerSec <= 0d)
            return InvalidObservation();

        var runSpeed = new DynamicGraphRunSpeedAxis(
            observation.InferenceElapsedMs,
            throughputRowsPerSec,
            observation.RoutingElapsedMs,
            perCandidateElapsedMs,
            perBatchElapsedMs);
        var measurement = new DynamicGraphRunSpeedMeasurement(
            observation.EvaluatedRowCount,
            candidateSamples.Count,
            batchSamples.Count,
            runSpeed);

        return OperationResult<DynamicGraphRunSpeedMeasurement>.Ok(measurement);
    }

    private static long? AverageWholeMilliseconds(IReadOnlyList<long> samples)
    {
        if (samples.Count == 0)
            return null;

        long sum = 0;
        checked
        {
            foreach (var sample in samples)
                sum += sample;
        }

        var arithmeticMean = (decimal)sum / samples.Count;
        var roundedMean = Math.Round(arithmeticMean, MidpointRounding.AwayFromZero);
        return checked((long)roundedMean);
    }

    private static OperationResult<DynamicGraphRunSpeedMeasurement> InvalidObservation()
        => OperationResult<DynamicGraphRunSpeedMeasurement>.Fail(
            InvalidObservationMessage,
            InvalidObservationCode);

    private static OperationResult<DynamicGraphRunSpeedMeasurement> Overflow()
        => OperationResult<DynamicGraphRunSpeedMeasurement>.Fail(
            OverflowMessage,
            OverflowCode);
}
