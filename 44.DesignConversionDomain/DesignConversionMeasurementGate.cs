// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;

public static class DesignConversionMeasurementGateStatus
{
    public const string NotRun = "not_run";
    public const string Passed = "passed";
    public const string Failed = "failed";
}

public sealed record DesignConversionHeldOutMeasurementMetrics(
    int? HeldOutRecordCount,
    double? HeldOutQualityScore,
    long? RuntimeMs,
    decimal? CostUsd,
    long? PeakRamMb,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionMeasurementIntegrityProof(
    bool? SourceDisjoint,
    bool? LeakageDetected,
    bool? HiddenDeterminismDetected,
    bool? AblationPassed,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionMeasurementGateThresholds(
    double RequiredHeldOutQualityScore,
    long MaxRuntimeMs,
    decimal MaxCostUsd,
    long MaxPeakRamMb);

public sealed record DesignConversionMeasurementGateRequest(
    string GateId,
    string UnitId,
    DesignConversionHeldOutMeasurementMetrics? HeldOutMetrics,
    DesignConversionMeasurementIntegrityProof? IntegrityProof,
    DesignConversionMeasurementGateThresholds Thresholds);

public sealed record DesignConversionMeasurementGateResult(
    string GateId,
    string UnitId,
    string Status,
    bool Passed,
    double RequiredHeldOutQualityScore,
    double? HeldOutQualityScore,
    bool SourceDisjoint,
    bool AblationPassed,
    bool BudgetPassed,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> EvidenceRefs);

public sealed class DesignConversionMeasurementGateEvaluator
{
    public DesignConversionMeasurementGateResult Evaluate(DesignConversionMeasurementGateRequest? request)
    {
        if (request is null)
        {
            return Fail(
                gateId: "missing-measurement-gate-request",
                unitId: "unknown",
                requiredHeldOutQualityScore: 0,
                heldOutQualityScore: null,
                sourceDisjoint: false,
                ablationPassed: false,
                budgetPassed: false,
                reasons: ["MEASUREMENT_GATE_REQUEST_REQUIRED"],
                evidenceRefs: ["DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED"]);
        }

        var reasons = new List<string>();
        var evidenceRefs = new List<string>();
        var metrics = request.HeldOutMetrics;
        var proof = request.IntegrityProof;
        var thresholds = request.Thresholds;
        var thresholdValidity = ValidateThresholds(thresholds, reasons);
        var requiredHeldOutQualityScore = thresholds?.RequiredHeldOutQualityScore ?? 0;

        if (metrics is null)
        {
            reasons.Add("HELD_OUT_MEASUREMENT_METRICS_REQUIRED");
        }
        else
        {
            if (metrics.HeldOutRecordCount is null)
                reasons.Add("HELD_OUT_RECORD_COUNT_MISSING");
            else if (metrics.HeldOutRecordCount <= 0)
                reasons.Add($"HELD_OUT_RECORD_COUNT_INVALID:{metrics.HeldOutRecordCount}");

            if (metrics.HeldOutQualityScore is null)
                reasons.Add("HELD_OUT_QUALITY_SCORE_MISSING");
            else if (!IsValidScore(metrics.HeldOutQualityScore.Value))
                reasons.Add($"HELD_OUT_QUALITY_SCORE_INVALID:{Format(metrics.HeldOutQualityScore.Value)}");
            else if (thresholdValidity.RequiredHeldOutQualityScore &&
                     metrics.HeldOutQualityScore < thresholds!.RequiredHeldOutQualityScore)
                reasons.Add(
                    $"HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD:{Format(metrics.HeldOutQualityScore.Value)}<{Format(thresholds.RequiredHeldOutQualityScore)}");

            if (metrics.RuntimeMs is null)
                reasons.Add("RUNTIME_MS_MISSING");
            else if (metrics.RuntimeMs < 0)
                reasons.Add($"RUNTIME_MS_INVALID:{metrics.RuntimeMs}");
            else if (thresholdValidity.MaxRuntimeMs && metrics.RuntimeMs > thresholds!.MaxRuntimeMs)
                reasons.Add($"RUNTIME_MS_BUDGET_EXCEEDED:{metrics.RuntimeMs}>{thresholds.MaxRuntimeMs}");

            if (metrics.CostUsd is null)
                reasons.Add("COST_USD_MISSING");
            else if (metrics.CostUsd < 0)
                reasons.Add($"COST_USD_INVALID:{Format(metrics.CostUsd.Value)}");
            else if (thresholdValidity.MaxCostUsd && metrics.CostUsd > thresholds!.MaxCostUsd)
                reasons.Add($"COST_USD_BUDGET_EXCEEDED:{metrics.CostUsd}>{thresholds.MaxCostUsd}");

            if (metrics.PeakRamMb is null)
                reasons.Add("PEAK_RAM_MB_MISSING");
            else if (metrics.PeakRamMb < 0)
                reasons.Add($"PEAK_RAM_MB_INVALID:{metrics.PeakRamMb}");
            else if (thresholdValidity.MaxPeakRamMb && metrics.PeakRamMb > thresholds!.MaxPeakRamMb)
                reasons.Add($"PEAK_RAM_MB_BUDGET_EXCEEDED:{metrics.PeakRamMb}>{thresholds.MaxPeakRamMb}");

            if (metrics.EvidenceRefs is null || metrics.EvidenceRefs.Count == 0)
                reasons.Add("HELD_OUT_MEASUREMENT_EVIDENCE_MISSING");
            else
                evidenceRefs.AddRange(metrics.EvidenceRefs);
        }

        if (proof is null)
        {
            reasons.Add("ANTI_LEAKAGE_ABLATION_PROOF_REQUIRED");
        }
        else
        {
            if (proof.SourceDisjoint is null)
                reasons.Add("SOURCE_DISJOINT_PROOF_MISSING");
            else if (!proof.SourceDisjoint.Value)
                reasons.Add("SOURCE_DISJOINT_PROOF_FAILED");

            if (proof.LeakageDetected is null)
                reasons.Add("LEAKAGE_CHECK_MISSING");
            else if (proof.LeakageDetected.Value)
                reasons.Add("LEAKAGE_DETECTED");

            if (proof.HiddenDeterminismDetected is null)
                reasons.Add("HIDDEN_DETERMINISM_CHECK_MISSING");
            else if (proof.HiddenDeterminismDetected.Value)
                reasons.Add("HIDDEN_DETERMINISM_DETECTED");

            if (proof.AblationPassed is null)
                reasons.Add("ABLATION_PROOF_MISSING");
            else if (!proof.AblationPassed.Value)
                reasons.Add("ABLATION_PROOF_FAILED");

            if (proof.EvidenceRefs is null || proof.EvidenceRefs.Count == 0)
                reasons.Add("ANTI_LEAKAGE_ABLATION_EVIDENCE_MISSING");
            else
                evidenceRefs.AddRange(proof.EvidenceRefs);
        }

        var budgetPassed =
            metrics?.RuntimeMs is not null &&
            metrics.CostUsd is not null &&
            metrics.PeakRamMb is not null &&
            metrics.RuntimeMs >= 0 &&
            metrics.CostUsd >= 0 &&
            metrics.PeakRamMb >= 0 &&
            thresholdValidity.AllBudgetThresholdsValid &&
            metrics.RuntimeMs <= thresholds!.MaxRuntimeMs &&
            metrics.CostUsd <= thresholds.MaxCostUsd &&
            metrics.PeakRamMb <= thresholds.MaxPeakRamMb;

        if (reasons.Count > 0)
        {
            return Fail(
                request.GateId,
                request.UnitId,
                requiredHeldOutQualityScore,
                metrics?.HeldOutQualityScore,
                proof?.SourceDisjoint == true,
                proof?.AblationPassed == true,
                budgetPassed,
                reasons,
                evidenceRefs);
        }

        evidenceRefs.Add("DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED");
        return new DesignConversionMeasurementGateResult(
            GateId: request.GateId,
            UnitId: request.UnitId,
            Status: DesignConversionMeasurementGateStatus.Passed,
            Passed: true,
            RequiredHeldOutQualityScore: thresholds!.RequiredHeldOutQualityScore,
            HeldOutQualityScore: metrics!.HeldOutQualityScore,
            SourceDisjoint: proof!.SourceDisjoint == true,
            AblationPassed: proof.AblationPassed == true,
            BudgetPassed: true,
            Reasons: ["ALL_MEASUREMENT_GATES_PASSED"],
            EvidenceRefs: evidenceRefs
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    private static ThresholdValidity ValidateThresholds(
        DesignConversionMeasurementGateThresholds? thresholds,
        List<string> reasons)
    {
        if (thresholds is null)
        {
            reasons.Add("MEASUREMENT_GATE_THRESHOLDS_REQUIRED");
            return new ThresholdValidity(
                RequiredHeldOutQualityScore: false,
                MaxRuntimeMs: false,
                MaxCostUsd: false,
                MaxPeakRamMb: false);
        }

        var requiredHeldOutQualityScore = IsValidScore(thresholds.RequiredHeldOutQualityScore);
        if (!requiredHeldOutQualityScore)
        {
            reasons.Add(
                $"REQUIRED_HELD_OUT_QUALITY_SCORE_THRESHOLD_INVALID:{Format(thresholds.RequiredHeldOutQualityScore)}");
        }

        var maxRuntimeMs = thresholds.MaxRuntimeMs > 0;
        if (!maxRuntimeMs)
            reasons.Add($"MAX_RUNTIME_MS_THRESHOLD_INVALID:{thresholds.MaxRuntimeMs}");

        var maxCostUsd = thresholds.MaxCostUsd >= 0;
        if (!maxCostUsd)
            reasons.Add($"MAX_COST_USD_THRESHOLD_INVALID:{Format(thresholds.MaxCostUsd)}");

        var maxPeakRamMb = thresholds.MaxPeakRamMb > 0;
        if (!maxPeakRamMb)
            reasons.Add($"MAX_PEAK_RAM_MB_THRESHOLD_INVALID:{thresholds.MaxPeakRamMb}");

        return new ThresholdValidity(
            requiredHeldOutQualityScore,
            maxRuntimeMs,
            maxCostUsd,
            maxPeakRamMb);
    }

    private static DesignConversionMeasurementGateResult Fail(
        string gateId,
        string unitId,
        double requiredHeldOutQualityScore,
        double? heldOutQualityScore,
        bool sourceDisjoint,
        bool ablationPassed,
        bool budgetPassed,
        IReadOnlyList<string> reasons,
        IReadOnlyList<string> evidenceRefs)
        => new(
            GateId: gateId,
            UnitId: unitId,
            Status: DesignConversionMeasurementGateStatus.Failed,
            Passed: false,
            RequiredHeldOutQualityScore: requiredHeldOutQualityScore,
            HeldOutQualityScore: heldOutQualityScore,
            SourceDisjoint: sourceDisjoint,
            AblationPassed: ablationPassed,
            BudgetPassed: budgetPassed,
            Reasons: reasons
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            EvidenceRefs: evidenceRefs
                .Append("DESIGN_CONVERSION_MEASUREMENT_GATE_FAILED")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());

    private static string Format(double value)
        => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static string Format(decimal value)
        => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static bool IsValidScore(double value)
        => !double.IsNaN(value) &&
           !double.IsInfinity(value) &&
           value is >= 0 and <= 1;

    private readonly record struct ThresholdValidity(
        bool RequiredHeldOutQualityScore,
        bool MaxRuntimeMs,
        bool MaxCostUsd,
        bool MaxPeakRamMb)
    {
        public bool AllBudgetThresholdsValid => MaxRuntimeMs && MaxCostUsd && MaxPeakRamMb;
    }
}
