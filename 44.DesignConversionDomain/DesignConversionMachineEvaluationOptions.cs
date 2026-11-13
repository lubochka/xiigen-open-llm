// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

public sealed record DesignConversionMachineEvaluationOptions
{
    public const double DefaultHeldOutEvaluationAccuracyGateThreshold = 0.75;

    public static DesignConversionMachineEvaluationOptions Default { get; } =
        new(DefaultHeldOutEvaluationAccuracyGateThreshold);

    public DesignConversionMachineEvaluationOptions(double heldOutEvaluationAccuracyGateThreshold)
    {
        if (!IsValidScore(heldOutEvaluationAccuracyGateThreshold))
        {
            throw new ArgumentOutOfRangeException(
                nameof(heldOutEvaluationAccuracyGateThreshold),
                "Held-out evaluation accuracy gate threshold must be a finite score from 0 through 1.");
        }

        HeldOutEvaluationAccuracyGateThreshold = heldOutEvaluationAccuracyGateThreshold;
    }

    public double HeldOutEvaluationAccuracyGateThreshold { get; }

    public static DesignConversionMachineEvaluationOptions FromMeasurementGateThresholds(
        DesignConversionMeasurementGateThresholds thresholds)
    {
        ArgumentNullException.ThrowIfNull(thresholds);
        return new DesignConversionMachineEvaluationOptions(thresholds.RequiredHeldOutQualityScore);
    }

    private static bool IsValidScore(double value)
        => !double.IsNaN(value) &&
           !double.IsInfinity(value) &&
           value is >= 0 and <= 1;
}
