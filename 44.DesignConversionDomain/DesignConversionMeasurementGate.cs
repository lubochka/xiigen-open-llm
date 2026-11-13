// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

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
    DesignConversionMeasurementGateThresholds Thresholds)
{
    public HeldOutScorerIdentity? MeasurementNoiseScorerIdentity { get; init; }
    public MeasurementNoisePassport? MeasurementNoisePassport { get; init; }
}

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
    IReadOnlyList<string> EvidenceRefs)
{
    public double? MeasurementNoiseFloor { get; init; }
    public string MeasurementNoiseStatus { get; init; } =
        MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus;
}

/// <summary>
/// Captures caller-supplied measurement used only to admit a local training
/// mutation. It is advisory input, not child-DPO held-out measurement and not
/// learned/model accuracy authority.
/// </summary>
public static class DesignConversionMeasurementAdmission
{
    public const string InvalidErrorCode =
        "DESIGN_CONVERSION_CALLER_ADMISSION_MEASUREMENT_INVALID";
    public const string RequiredErrorCode =
        "DESIGN_CONVERSION_CALLER_ADMISSION_MEASUREMENT_REQUIRED";
    public const int MaxIdentifierChars = 256;
    public const int MaxEvidenceRefsPerSection = 128;
    public const int MaxEvidenceRefChars = 512;
    public const int MaxHeldOutRecordCount = 10_000_000;
    public const long MaxReportedRuntimeMs = 2_592_000_000;
    public const decimal MaxReportedCostUsd = 1_000_000m;
    public const long MaxReportedPeakRamMb = 16_777_216;
    public const string MeasurementNoisePassportInvalidErrorCode =
        "DESIGN_CONVERSION_MEASUREMENT_NOISE_PASSPORT_INVALID";

    public static OperationResult<DesignConversionMeasurementGateRequest> Capture(
        DesignConversionMeasurementGateRequest? request)
    {
        try
        {
            return CaptureCore(request);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Fail(InvalidErrorCode);
        }
    }

    private static OperationResult<DesignConversionMeasurementGateRequest> CaptureCore(
        DesignConversionMeasurementGateRequest? request)
    {
        if (request is null)
            return Fail(RequiredErrorCode);
        if (!IsBoundedIdentifier(request.GateId) ||
            !IsBoundedIdentifier(request.UnitId) ||
            request.Thresholds is null)
        {
            return Fail(InvalidErrorCode);
        }

        var metrics = request.HeldOutMetrics;
        if (metrics is not null &&
            (metrics.HeldOutRecordCount is < 0 or > MaxHeldOutRecordCount ||
             metrics.HeldOutQualityScore is { } qualityScore &&
             !IsFiniteScore(qualityScore) ||
             metrics.RuntimeMs is < 0 or > MaxReportedRuntimeMs ||
             metrics.CostUsd is < 0 or > MaxReportedCostUsd ||
             metrics.PeakRamMb is < 0 or > MaxReportedPeakRamMb ||
             !AreRefsOnly(metrics.EvidenceRefs)))
        {
            return Fail(InvalidErrorCode);
        }

        var proof = request.IntegrityProof;
        if (proof is not null && !AreRefsOnly(proof.EvidenceRefs))
            return Fail(InvalidErrorCode);

        var thresholds = request.Thresholds;
        if (!IsFiniteScore(thresholds.RequiredHeldOutQualityScore) ||
            thresholds.MaxRuntimeMs is <= 0 or > MaxReportedRuntimeMs ||
            thresholds.MaxCostUsd is < 0 or > MaxReportedCostUsd ||
            thresholds.MaxPeakRamMb is <= 0 or > MaxReportedPeakRamMb)
        {
            return Fail(InvalidErrorCode);
        }

        if (request.MeasurementNoiseScorerIdentity is { } expectedScorerIdentity &&
            !MeasurementNoisePolicy.ScorerIdentitiesMatch(
                expectedScorerIdentity,
                expectedScorerIdentity))
        {
            // Самосравнение переиспользует единый валидатор всех пяти полей:
            // identity-only не должен сохранять опасные строки лишь потому,
            // что паспорт калибровки ещё не создан.
            return Fail(InvalidErrorCode);
        }

        if (request.MeasurementNoisePassport is not null)
        {
            if (request.MeasurementNoiseScorerIdentity is null ||
                metrics?.HeldOutRecordCount is not { } scoredRows ||
                metrics.HeldOutQualityScore is not { } heldOutScore ||
                !MeasurementNoisePolicy.TryValidatePassportForMeasurement(
                    request.MeasurementNoisePassport,
                    request.MeasurementNoiseScorerIdentity,
                    request.GateId,
                    scoredRows,
                    heldOutScore,
                    out _))
            {
                // Паспорт без ожидаемой личности того же прибора, той же
                // выборки и среднего легализовал бы чужую погрешность.
                return Fail(MeasurementNoisePassportInvalidErrorCode);
            }
        }

        return OperationResult<DesignConversionMeasurementGateRequest>.Ok(
            request with
            {
                HeldOutMetrics = metrics is null
                    ? null
                    : metrics with
                    {
                        EvidenceRefs = metrics.EvidenceRefs
                            .Order(StringComparer.Ordinal)
                            .ToArray()
                    },
                IntegrityProof = proof is null
                    ? null
                    : proof with
                    {
                        EvidenceRefs = proof.EvidenceRefs
                            .Order(StringComparer.Ordinal)
                            .ToArray()
                    },
                Thresholds = thresholds with { },
                MeasurementNoiseScorerIdentity = request.MeasurementNoiseScorerIdentity,
                MeasurementNoisePassport = request.MeasurementNoisePassport
            });
    }

    public static OperationResult<string> BuildCanonicalSha256(
        DesignConversionMeasurementGateRequest? request)
    {
        var captured = Capture(request);
        if (!captured.IsSuccess || captured.Value is null)
            return captured.MapError<string>();

        var value = captured.Value;
        var hasMeasurementNoiseBinding =
            value.MeasurementNoiseScorerIdentity is not null ||
            value.MeasurementNoisePassport is not null;
        var canonical = new StringBuilder();
        // Старые запросы не знали о паспорте шума. Для них сохраняем прежние
        // байты целиком; наличие хотя бы одного нового поля явно включает v2.
        Append(
            canonical,
            "schema",
            hasMeasurementNoiseBinding
                ? "design-conversion-caller-admission-measurement-v2"
                : "design-conversion-caller-admission-measurement-v1");
        Append(canonical, "gate_id", value.GateId);
        Append(canonical, "unit_id", value.UnitId);
        Append(canonical, "held_out_record_count", Invariant(value.HeldOutMetrics?.HeldOutRecordCount));
        Append(canonical, "held_out_quality_score", value.HeldOutMetrics?.HeldOutQualityScore?.ToString("R", CultureInfo.InvariantCulture));
        if (hasMeasurementNoiseBinding)
        {
            AppendScorerIdentity(canonical, value.MeasurementNoiseScorerIdentity);
            AppendPassport(canonical, value.MeasurementNoisePassport);
        }
        Append(canonical, "runtime_ms", Invariant(value.HeldOutMetrics?.RuntimeMs));
        Append(canonical, "cost_usd", value.HeldOutMetrics?.CostUsd?.ToString("G29", CultureInfo.InvariantCulture));
        Append(canonical, "peak_ram_mb", Invariant(value.HeldOutMetrics?.PeakRamMb));
        AppendRefs(canonical, "held_out_evidence", value.HeldOutMetrics?.EvidenceRefs);
        Append(canonical, "source_disjoint", Invariant(value.IntegrityProof?.SourceDisjoint));
        Append(canonical, "leakage_detected", Invariant(value.IntegrityProof?.LeakageDetected));
        Append(canonical, "hidden_determinism_detected", Invariant(value.IntegrityProof?.HiddenDeterminismDetected));
        Append(canonical, "ablation_passed", Invariant(value.IntegrityProof?.AblationPassed));
        AppendRefs(canonical, "integrity_evidence", value.IntegrityProof?.EvidenceRefs);
        Append(canonical, "required_quality", value.Thresholds.RequiredHeldOutQualityScore.ToString("R", CultureInfo.InvariantCulture));
        Append(canonical, "max_runtime_ms", value.Thresholds.MaxRuntimeMs.ToString(CultureInfo.InvariantCulture));
        Append(canonical, "max_cost_usd", value.Thresholds.MaxCostUsd.ToString("G29", CultureInfo.InvariantCulture));
        Append(canonical, "max_peak_ram_mb", value.Thresholds.MaxPeakRamMb.ToString(CultureInfo.InvariantCulture));

        return OperationResult<string>.Ok(
            "sha256:" + Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())))
                .ToLowerInvariant());
    }

    public static string ProjectIdentifierRef(string kind, string value)
        => "sha256:" + Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(
                    $"design-conversion-caller-admission:{kind}:{value}")))
            .ToLowerInvariant();

    private static bool IsBoundedIdentifier(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaxIdentifierChars &&
           string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
           !HasPrivateOrRawPrefix(value) &&
           value.All(static ch => char.IsAsciiLetterOrDigit(ch) ||
                                  ch is '-' or '_' or '.' or ':');

    private static bool AreRefsOnly(IReadOnlyList<string>? values)
        => values is not null &&
           values.Count <= MaxEvidenceRefsPerSection &&
           values.All(IsRefOnly);

    private static bool IsRefOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > MaxEvidenceRefChars ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Any(static ch => char.IsWhiteSpace(ch) || char.IsControl(ch)) ||
            value.IndexOfAny(['{', '}', '[', ']', '<', '>', '"', '\'', '\\']) >= 0)
        {
            return false;
        }

        if (HasPrivateOrRawPrefix(value))
            return false;

        if (value.StartsWith("sha256:", StringComparison.Ordinal))
            return IsSha256Ref(value);
        if (value.StartsWith("artifact://", StringComparison.Ordinal))
            return IsArtifactUri(value);
        if (value.StartsWith("ref:", StringComparison.Ordinal))
            return IsOpaqueRef(value);
        if (value.StartsWith("urn:sha256:", StringComparison.Ordinal))
            return IsSha256Hex(value.AsSpan("urn:sha256:".Length));
        return false;
    }

    private static bool HasPrivateOrRawPrefix(string value)
        => value.StartsWith("raw:", StringComparison.OrdinalIgnoreCase) ||
           value.StartsWith("private:", StringComparison.OrdinalIgnoreCase) ||
           value.StartsWith("payload:", StringComparison.OrdinalIgnoreCase) ||
           value.StartsWith("data:", StringComparison.OrdinalIgnoreCase);

    private static bool IsSha256Ref(string value)
        => value.Length == "sha256:".Length + 64 &&
           IsSha256Hex(value.AsSpan("sha256:".Length));

    private static bool IsSha256Hex(ReadOnlySpan<char> value)
    {
        if (value.Length != 64)
            return false;
        foreach (var ch in value)
        {
            if (!char.IsAsciiHexDigit(ch))
                return false;
        }
        return true;
    }

    private static bool IsArtifactUri(string value)
        => value.Length > "artifact://".Length &&
           value.IndexOfAny(['?', '#', '@']) < 0 &&
           Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
           string.Equals(uri.Scheme, "artifact", StringComparison.Ordinal) &&
           !string.IsNullOrWhiteSpace(uri.Host) &&
           uri.AbsolutePath.Length > 1;

    private static bool IsOpaqueRef(string value)
    {
        var opaque = value.AsSpan("ref:".Length);
        var separator = opaque.IndexOf(':');
        if (separator <= 0 || separator == opaque.Length - 1)
            return false;
        foreach (var ch in opaque)
        {
            if (!char.IsAsciiLetterOrDigit(ch) &&
                ch != '-' &&
                ch != '_' &&
                ch != '.' &&
                ch != ':' &&
                ch != '/')
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsFiniteScore(double value)
        => double.IsFinite(value) && value is >= 0d and <= 1d;

    private static string? Invariant<T>(T? value) where T : struct
        => value.HasValue
            ? value.Value is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.Value.ToString()
            : null;

    private static string? Invariant(bool? value)
        => value switch { true => "true", false => "false", null => null };

    private static void AppendRefs(
        StringBuilder canonical,
        string name,
        IReadOnlyList<string>? values)
    {
        Append(canonical, name + ".count", values?.Count.ToString(CultureInfo.InvariantCulture));
        if (values is null)
            return;
        for (var index = 0; index < values.Count; index++)
            Append(canonical, name + "." + index.ToString(CultureInfo.InvariantCulture), values[index]);
    }

    private static void AppendPassport(
        StringBuilder canonical,
        MeasurementNoisePassport? passport)
    {
        Append(canonical, "measurement_noise_passport_present", passport is null ? "false" : "true");
        if (passport is null)
            return;

        Append(canonical, "measurement_noise_scorer_domain", passport.ScorerIdentity.DomainTag);
        Append(canonical, "measurement_noise_scorer_id", passport.ScorerIdentity.ScorerId);
        Append(canonical, "measurement_noise_scorer_version", passport.ScorerIdentity.ScorerVersion);
        Append(canonical, "measurement_noise_normalization_profile", passport.ScorerIdentity.NormalizationProfileId);
        Append(canonical, "measurement_noise_metric_profile", passport.ScorerIdentity.MetricProfileId);
        Append(canonical, "measurement_noise_evaluation_set", passport.EvaluationSetId);
        Append(canonical, "measurement_noise_scored_rows", passport.ScoredRows.ToString(CultureInfo.InvariantCulture));
        Append(canonical, "measurement_noise_repeat_count", passport.RepeatCount.ToString(CultureInfo.InvariantCulture));
        Append(canonical, "measurement_noise_multiplier", passport.StandardDeviationMultiplier.ToString("R", CultureInfo.InvariantCulture));
        Append(canonical, "measurement_noise_mean_score", passport.MeanScore.ToString("R", CultureInfo.InvariantCulture));
        Append(canonical, "measurement_noise_sample_stddev", passport.SampleStandardDeviation.ToString("R", CultureInfo.InvariantCulture));
        Append(canonical, "measurement_noise_binomial_lower_bound", passport.BinomialLowerBound.ToString("R", CultureInfo.InvariantCulture));
        Append(canonical, "measurement_noise_floor", passport.NoiseFloor.ToString("R", CultureInfo.InvariantCulture));
        Append(canonical, "measurement_noise_calibrated_at_utc", passport.CalibratedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        Append(canonical, "measurement_noise_repeated_score_count", passport.RepeatedScores.Count.ToString(CultureInfo.InvariantCulture));
        for (var index = 0; index < passport.RepeatedScores.Count; index++)
        {
            Append(
                canonical,
                "measurement_noise_repeated_score." + index.ToString(CultureInfo.InvariantCulture),
                passport.RepeatedScores[index].ToString("R", CultureInfo.InvariantCulture));
        }
    }

    private static void AppendScorerIdentity(
        StringBuilder canonical,
        HeldOutScorerIdentity? scorerIdentity)
    {
        Append(canonical, "measurement_noise_expected_scorer_present", scorerIdentity is null ? "false" : "true");
        if (scorerIdentity is null)
            return;

        Append(canonical, "measurement_noise_expected_scorer_domain", scorerIdentity.DomainTag);
        Append(canonical, "measurement_noise_expected_scorer_id", scorerIdentity.ScorerId);
        Append(canonical, "measurement_noise_expected_scorer_version", scorerIdentity.ScorerVersion);
        Append(canonical, "measurement_noise_expected_normalization_profile", scorerIdentity.NormalizationProfileId);
        Append(canonical, "measurement_noise_expected_metric_profile", scorerIdentity.MetricProfileId);
    }

    private static void Append(StringBuilder canonical, string name, string? value)
    {
        canonical.Append(name.Length.ToString(CultureInfo.InvariantCulture))
            .Append(':').Append(name).Append('=');
        if (value is null)
            canonical.Append("N;");
        else
            canonical.Append('V').Append(value.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(value).Append(';');
    }

    private static OperationResult<DesignConversionMeasurementGateRequest> Fail(
        string errorCode)
        => OperationResult<DesignConversionMeasurementGateRequest>.Fail(
            "Caller admission measurement must be bounded, refs-only, and contain no raw or private evidence.",
            errorCode);
}

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

        var captured = DesignConversionMeasurementAdmission.Capture(request);
        if (!captured.IsSuccess || captured.Value is null)
        {
            return Fail(
                gateId: "invalid-caller-admission-measurement",
                unitId: "unknown",
                requiredHeldOutQualityScore: 0,
                heldOutQualityScore: null,
                sourceDisjoint: false,
                ablationPassed: false,
                budgetPassed: false,
                reasons: [captured.ErrorCode ?? DesignConversionMeasurementAdmission.InvalidErrorCode],
                evidenceRefs: ["DESIGN_CONVERSION_CALLER_ADMISSION_MEASUREMENT_REJECTED"],
                measurementNoiseStatus: request?.MeasurementNoisePassport is null
                    ? MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus
                    : MeasurementNoisePolicy.InvalidPassportStatus);
        }
        request = captured.Value;
        var gateIdRef = DesignConversionMeasurementAdmission.ProjectIdentifierRef(
            "gate-id",
            request.GateId);
        var unitIdRef = DesignConversionMeasurementAdmission.ProjectIdentifierRef(
            "unit-id",
            request.UnitId);

        var reasons = new List<string>();
        var evidenceRefs = new List<string>();
        var metrics = request.HeldOutMetrics;
        var proof = request.IntegrityProof;
        var thresholds = request.Thresholds;
        var thresholdValidity = ValidateThresholds(thresholds, reasons);
        var requiredHeldOutQualityScore = thresholds?.RequiredHeldOutQualityScore ?? 0;
        MeasurementNoiseComparison? noiseComparison = null;
        if (metrics?.HeldOutQualityScore is { } measuredScore &&
            thresholdValidity.RequiredHeldOutQualityScore &&
            request.MeasurementNoisePassport is { } passport)
        {
            noiseComparison = MeasurementNoisePolicy.ClassifyDifference(
                measuredScore - thresholds!.RequiredHeldOutQualityScore,
                passport);
        }

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
                     noiseComparison is not null &&
                     !noiseComparison.DifferenceProven)
            {
                // Точка внутри полосы допуска не становится проходом: она
                // означает, что прибор не различил score и порог.
                reasons.Add(
                    $"HELD_OUT_QUALITY_SCORE_WITHIN_MEASUREMENT_NOISE:{Format(metrics.HeldOutQualityScore.Value)}-{Format(thresholds!.RequiredHeldOutQualityScore)}<={Format(noiseComparison.NoiseFloor!.Value)}");
            }
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
                gateIdRef,
                unitIdRef,
                requiredHeldOutQualityScore,
                metrics?.HeldOutQualityScore,
                proof?.SourceDisjoint == true,
                proof?.AblationPassed == true,
                budgetPassed,
                reasons,
                evidenceRefs,
                noiseComparison?.NoiseFloor,
                noiseComparison?.Status);
        }

        evidenceRefs.Add("DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED");
        return new DesignConversionMeasurementGateResult(
            GateId: gateIdRef,
            UnitId: unitIdRef,
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
                .ToArray())
        {
            MeasurementNoiseFloor = noiseComparison?.NoiseFloor,
            MeasurementNoiseStatus = noiseComparison?.Status ??
                MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus
        };
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
        IReadOnlyList<string> evidenceRefs,
        double? measurementNoiseFloor = null,
        string? measurementNoiseStatus = null)
        => new DesignConversionMeasurementGateResult(
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
                .ToArray())
        {
            MeasurementNoiseFloor = measurementNoiseFloor,
            MeasurementNoiseStatus = measurementNoiseStatus ??
                MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus
        };

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
