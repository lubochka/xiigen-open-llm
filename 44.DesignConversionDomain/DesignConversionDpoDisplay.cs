// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;

public sealed record DesignConversionDpoDisplayRow(
    string DisplayId,
    string UnitId,
    string SourceKind,
    string SourceRef,
    string ChosenSummary,
    string RejectedSummary,
    double UsefulnessScore,
    string UsefulnessLabel,
    IReadOnlyList<string> UsefulnessFactors,
    IReadOnlyList<string> Defects,
    IReadOnlyList<string> EvidenceRefs,
    bool IsBlocked,
    string BlockedReason,
    bool IsQuarantined,
    bool AllowsTraining,
    bool ContainsUserPrivateData,
    string RightsBoundary,
    bool TrainingCandidateOnly,
    bool ProductNotReady,
    string ReadinessBoundary,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionDpoDisplayResult(
    IReadOnlyList<DesignConversionDpoDisplayRow> Rows,
    int TrainingCandidateCount,
    int BlockedCount,
    int QuarantinedCount,
    int PrivateDataCount,
    IReadOnlyList<string> Warnings);

public sealed record DesignConversionDpoDisplayQuery(
    string? UnitId = null,
    bool IncludeBlocked = true,
    double? MinimumUsefulnessScore = null,
    double? MaximumUsefulnessScore = null,
    int? Limit = null);

public sealed class DesignConversionDpoDisplayService
{
    private readonly IDesignConversionDpoStorage _storage;
    private readonly DesignConversionDpoGenerator _generator;
    private readonly DesignConversionDpoDisplayBuilder _builder;

    public DesignConversionDpoDisplayService(
        IDesignConversionDpoStorage storage,
        DesignConversionDpoGenerator? generator = null,
        DesignConversionDpoDisplayBuilder? builder = null)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _generator = generator ?? new DesignConversionDpoGenerator();
        _builder = builder ?? new DesignConversionDpoDisplayBuilder();
    }

    public async Task<OperationResult<DesignConversionDpoDisplayResult>> GetDisplayAsync(
        DesignConversionDpoDisplayQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionDpoDisplayResult>.Fail(
                "DesignConversion DPO display query cancelled.",
                "CANCELLED");

        var examples = await _storage.ListExamplesAsync(cancellationToken);
        if (!examples.IsSuccess)
            return examples.MapError<DesignConversionDpoDisplayResult>();

        var build = _generator.BuildTrainReadyRecords(examples.Value!);
        if (!build.IsSuccess)
            return build.MapError<DesignConversionDpoDisplayResult>();

        return _builder.BuildDisplay(build.Value!.AllRecords, query);
    }
}

public sealed class DesignConversionDpoDisplayBuilder
{
    private const string UsefulnessRole = "review_priority_only";
    private const string TrainingCandidateOnlyBoundary = "training_candidate_only";
    private const string ProductNotReadyBoundary = "product_not_ready";
    private const string BlockedBoundary = "blocked_not_training_candidate";
    private const string PrivacyRedactionBoundary = "privacy_redaction";
    private const string HeldOutBoundary = "heldout";

    public OperationResult<DesignConversionDpoDisplayResult> BuildDisplay(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionDpoDisplayQuery? query = null)
    {
        if (records is null)
            return OperationResult<DesignConversionDpoDisplayResult>.Fail(
                "DesignConversion DPO records are required.",
                "DESIGN_CONVERSION_DPO_RECORDS_REQUIRED");

        var effectiveQuery = query ?? new DesignConversionDpoDisplayQuery();
        var rows = records
            .Where(record => record is not null)
            .Select(ToRow)
            .Where(row => MatchesQuery(row, effectiveQuery))
            .OrderByDescending(row => row.UsefulnessScore)
            .ThenBy(row => row.DisplayId, StringComparer.Ordinal)
            .Take(effectiveQuery.Limit is > 0 ? effectiveQuery.Limit.Value : int.MaxValue)
            .ToArray();

        return OperationResult<DesignConversionDpoDisplayResult>.Ok(new DesignConversionDpoDisplayResult(
            Rows: rows,
            TrainingCandidateCount: rows.Count(row => row.TrainingCandidateOnly && !row.IsBlocked),
            BlockedCount: rows.Count(row => row.IsBlocked),
            QuarantinedCount: rows.Count(row => row.IsQuarantined),
            PrivateDataCount: rows.Count(row => row.ContainsUserPrivateData),
            Warnings:
            [
                "dpo_usefulness_display_is_review_priority_only",
                "training_candidate_only",
                "product_not_ready",
                "privacy_redaction_applied"
            ]));
    }

    private static DesignConversionDpoDisplayRow ToRow(DesignConversionDpoTrainingRecord record)
    {
        var isBlocked = IsBlocked(record);
        var providerMissing = HasProviderMissingStatus(record);
        var negativePairAccepted = DesignConversionTrainingMeasurementGatePolicy.HasNegativePairQuality(record);
        var heldOut = IsHeldOut(record);
        var trainingGrade = IsTrainingGrade(record);
        var syntheticNegative = IsSyntheticNegative(record);
        var scaffoldNegative = syntheticNegative || IsScaffoldNegative(record);
        var blockedReason = BuildBlockedReason(record);
        var sourceRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-display-source",
            $"{record.Provenance.SourceKind}|{record.Provenance.SourceManifestRef}|{record.Provenance.SourceRequestId}");
        var displayId = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-dpo-display",
            $"{record.RecordId}|{record.UnitId}|{sourceRef}");
        var trainingCandidateOnly = !isBlocked;
        var readinessBoundary = trainingCandidateOnly ? TrainingCandidateOnlyBoundary : BlockedBoundary;
        var boundaryMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["usefulness_score_role"] = UsefulnessRole,
            [TrainingCandidateOnlyBoundary] = trainingCandidateOnly.ToString(),
            [ProductNotReadyBoundary] = bool.TrueString,
            [PrivacyRedactionBoundary] = "raw_source_request_id/raw_candidate_ids/raw_prompts/raw_paths_not_emitted",
            ["raw_source_request_id_included"] = bool.FalseString,
            ["raw_candidate_ids_included"] = bool.FalseString,
            ["raw_prompt_included"] = bool.FalseString,
            ["raw_screenshot_included"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["provider_missing"] = providerMissing.ToString(),
            ["negative_quality_status"] = negativePairAccepted
                ? "accepted_negative_pair"
                : "rejected_negative_quality",
            ["training_grade"] = trainingGrade.ToString(),
            ["not_training_grade"] = (!trainingGrade).ToString(),
            ["training_grade_boundary"] = trainingGrade
                ? "real_training_quality_contract_passed"
                : "training_candidate_or_local_scaffold_not_training_grade",
            ["synthetic_negative"] = syntheticNegative.ToString(),
            ["scaffold_negative"] = scaffoldNegative.ToString(),
            [HeldOutBoundary] = heldOut.ToString(),
            ["split"] = BuildDisplaySplit(record)
        };
        AppendSourceEncodingDisplayMetadata(record, boundaryMetadata);

        return new DesignConversionDpoDisplayRow(
            DisplayId: displayId,
            UnitId: record.UnitId,
            SourceKind: string.IsNullOrWhiteSpace(record.Provenance.SourceKind)
                ? "unknown"
                : record.Provenance.SourceKind,
            SourceRef: sourceRef,
            ChosenSummary: Summarize(record.Chosen, "chosen"),
            RejectedSummary: Summarize(record.Rejected, "rejected"),
            UsefulnessScore: Math.Round(record.UsefulnessScore, 6),
            UsefulnessLabel: UsefulnessLabel(record.UsefulnessScore, isBlocked),
            UsefulnessFactors: BuildUsefulnessFactors(record, readinessBoundary),
            Defects: SummarizeDefects(record),
            EvidenceRefs: BuildEvidenceRefs(record, readinessBoundary),
            IsBlocked: isBlocked,
            BlockedReason: blockedReason,
            IsQuarantined: record.Quarantine.IsQuarantined,
            AllowsTraining: record.License.AllowsTraining,
            ContainsUserPrivateData: record.License.ContainsUserPrivateData,
            RightsBoundary: BuildRightsBoundary(record),
            TrainingCandidateOnly: trainingCandidateOnly,
            ProductNotReady: true,
            ReadinessBoundary: readinessBoundary,
            BoundaryMetadata: boundaryMetadata);
    }

    private static string Summarize(DesignConversionImprovementOutput output, string label)
    {
        var status = output.Accepted ? "accepted" : "rejected";
        var markup = string.IsNullOrWhiteSpace(output.Html) && string.IsNullOrWhiteSpace(output.Css)
            ? "no-markup"
            : "markup";
        var resultRef = DesignConversionDpoLeakageGuard.HashRef($"dcm-display-{label}-result", output.OutputId);
        var defects = output.Verdict.Defects
            .Select(defect => defect.Category)
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(category => category, StringComparer.Ordinal)
            .ToArray();
        var defectText = defects.Length == 0 ? "none" : string.Join(",", defects);

        return $"result_ref={resultRef};status={status};visual_score={output.Verdict.Score:0.###};markup={markup};defects={defectText}";
    }

    private static IReadOnlyList<string> SummarizeDefects(DesignConversionDpoTrainingRecord record)
        => record.Chosen.Verdict.Defects
            .Concat(record.Rejected.Verdict.Defects)
            .Select(defect => $"{defect.Category}:{defect.Severity}")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(defect => defect, StringComparer.Ordinal)
            .ToArray();

    private static string UsefulnessLabel(double score, bool isBlocked)
    {
        if (isBlocked)
            return "blocked";
        if (score >= 0.85)
            return "high_usefulness";
        if (score >= 0.5)
            return "medium_usefulness";
        return "low_usefulness";
    }

    private static IReadOnlyList<string> BuildUsefulnessFactors(
        DesignConversionDpoTrainingRecord record,
        string readinessBoundary)
    {
        var scoreDelta = Math.Round(record.Chosen.Verdict.Score - record.Rejected.Verdict.Score, 6);
        return
        [
            $"usefulness_score_role={UsefulnessRole}",
            $"score_delta={scoreDelta:0.######}",
            $"negative_pair_quality={SafeDisplayToken(record.NegativeReasonTaxonomy, "unknown")}",
            $"training_grade={IsTrainingGrade(record).ToString().ToLowerInvariant()}",
            $"not_training_grade={(!IsTrainingGrade(record)).ToString().ToLowerInvariant()}",
            $"synthetic_negative={IsSyntheticNegative(record).ToString().ToLowerInvariant()}",
            $"scaffold_negative={(IsSyntheticNegative(record) || IsScaffoldNegative(record)).ToString().ToLowerInvariant()}",
            "source_disjoint=not_proven",
            IsHeldOut(record) ? "heldout_quality=heldout_row" : "heldout_quality=not_proven",
            readinessBoundary,
            "privacy_redaction=raw_private_fields_not_emitted"
        ];
    }

    private static void AppendSourceEncodingDisplayMetadata(
        DesignConversionDpoTrainingRecord record,
        IDictionary<string, string> metadata)
    {
        CopyMetadata(record, metadata, "source_encoder_schema", "unknown");
        CopyMetadata(record, metadata, "source_encoder_dimensions", record.InternalVectorPayload.Count.ToString());
        CopyMetadata(record, metadata, "source_encoder_vector_hash", "missing");
        CopyMetadata(record, metadata, "source_encoder_feature_map_hash", "missing");
        CopyMetadata(record, metadata, "source_encoder_feature_names", "missing");
        CopyMetadata(record, metadata, "source_structure_signal_summary", "missing");
        CopyMetadata(record, metadata, "result_signal_summary", "missing");
        CopyMetadata(record, metadata, "source_encoder_shortcut_guard", "missing");
        CopyMetadata(record, metadata, "source_kind_in_vector", bool.FalseString);
        CopyMetadata(record, metadata, "source_id_in_vector", bool.FalseString);
    }

    private static void CopyMetadata(
        DesignConversionDpoTrainingRecord record,
        IDictionary<string, string> target,
        string key,
        string fallback)
    {
        target[key] = record.Provenance.Metadata.TryGetValue(key, out var value)
            ? SafeDisplayToken(value, fallback)
            : fallback;
    }

    private static IReadOnlyList<string> BuildEvidenceRefs(
        DesignConversionDpoTrainingRecord record,
        string readinessBoundary)
    {
        var refs = record.Quarantine.EvidenceRefs
            .Concat(record.Chosen.EvidenceRefs)
            .Concat(record.Rejected.EvidenceRefs)
            .Concat(record.Chosen.Verdict.EvidenceRefs)
            .Concat(record.Rejected.Verdict.EvidenceRefs)
            .Select(ToDisplayEvidenceRef)
            .Append($"design-conversion-dpo-display:{readinessBoundary}")
            .Append("design-conversion-dpo-display:product_not_ready")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();

        return refs.Length == 0
            ? ["design-conversion-dpo-display:evidence_missing"]
            : refs;
    }

    private static string ToDisplayEvidenceRef(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return "design-conversion-dpo-display:evidence_missing";
        if (reference.StartsWith("design-conversion-dpo-validation:", StringComparison.Ordinal))
            return reference;
        return DesignConversionDpoLeakageGuard.HashRef("dcm-display-evidence", reference);
    }

    private static bool MatchesQuery(DesignConversionDpoDisplayRow row, DesignConversionDpoDisplayQuery query)
    {
        if (!query.IncludeBlocked && row.IsBlocked)
            return false;
        if (!string.IsNullOrWhiteSpace(query.UnitId) && !MatchesUnit(row.UnitId, query.UnitId))
            return false;
        if (query.MinimumUsefulnessScore is not null && row.UsefulnessScore < query.MinimumUsefulnessScore.Value)
            return false;
        if (query.MaximumUsefulnessScore is not null && row.UsefulnessScore > query.MaximumUsefulnessScore.Value)
            return false;
        return true;
    }

    private static bool MatchesUnit(string unitId, string requestedUnitId)
    {
        var normalizedStored = DesignConversionDpoUnits.TryNormalize(unitId, out var stored)
            ? stored
            : unitId;
        var normalizedRequested = DesignConversionDpoUnits.TryNormalize(requestedUnitId, out var requested)
            ? requested
            : requestedUnitId;
        return string.Equals(normalizedStored, normalizedRequested, StringComparison.Ordinal);
    }

    private static bool IsBlocked(DesignConversionDpoTrainingRecord record)
        => record.Quarantine.IsQuarantined
            || !record.License.AllowsTraining
            || record.License.ContainsUserPrivateData
            || HasProviderMissingStatus(record)
            || HasNoEvidence(record);

    private static bool HasNoEvidence(DesignConversionDpoTrainingRecord record)
        => record.Quarantine.EvidenceRefs.Count == 0
            && record.Chosen.EvidenceRefs.Count == 0
            && record.Rejected.EvidenceRefs.Count == 0
            && record.Chosen.Verdict.EvidenceRefs.Count == 0
            && record.Rejected.Verdict.EvidenceRefs.Count == 0;

    private static bool HasProviderMissingStatus(DesignConversionDpoTrainingRecord record)
        => ContainsProviderMissing(record.Quarantine.Reason)
            || record.Provenance.Metadata.Values.Any(ContainsProviderMissing);

    private static bool IsHeldOut(DesignConversionDpoTrainingRecord record)
        => record.Provenance.Metadata.Any(pair =>
            ContainsHeldOut(pair.Key) || ContainsHeldOut(pair.Value));

    private static bool IsTrainingGrade(DesignConversionDpoTrainingRecord record)
        => string.Equals(
            ReadMetadata(record.Provenance.Metadata, "training_grade"),
            bool.TrueString,
            StringComparison.OrdinalIgnoreCase);

    private static bool IsSyntheticNegative(DesignConversionDpoTrainingRecord record)
        => string.Equals(record.NegativeReasonTaxonomy, DesignConversionNegativeReasonTaxonomy.SyntheticRuntimeNotTrainReady, StringComparison.Ordinal)
           || string.Equals(ReadMetadata(record.Provenance.Metadata, "synthetic_negative"), bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || (record.NegativeQualityReason ?? string.Empty).Contains("synthetic", StringComparison.OrdinalIgnoreCase);

    private static bool IsScaffoldNegative(DesignConversionDpoTrainingRecord record)
        => string.Equals(ReadMetadata(record.Provenance.Metadata, "scaffold_negative"), bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || string.Equals(ReadMetadata(record.Provenance.Metadata, "local_artifact_db"), bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || (record.NegativeQualityReason ?? string.Empty).Contains("scaffold", StringComparison.OrdinalIgnoreCase);

    private static string ReadMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value) ? value : string.Empty;

    private static bool ContainsHeldOut(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.Contains("held_out", StringComparison.OrdinalIgnoreCase)
               || value.Contains("heldout", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "holdout", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "validation", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsProviderMissing(string? value)
        => !string.IsNullOrWhiteSpace(value)
            && value.Contains("provider", StringComparison.OrdinalIgnoreCase)
            && (value.Contains("missing", StringComparison.OrdinalIgnoreCase)
                || value.Contains("required", StringComparison.OrdinalIgnoreCase)
                || value.Contains("blocked", StringComparison.OrdinalIgnoreCase));

    private static string BuildBlockedReason(DesignConversionDpoTrainingRecord record)
    {
        var reasons = new List<string>();
        if (record.Quarantine.IsQuarantined && !string.IsNullOrWhiteSpace(record.Quarantine.Reason))
            reasons.Add(record.Quarantine.Reason);
        if (!record.License.AllowsTraining)
            reasons.Add("license_blocks_training");
        if (record.License.ContainsUserPrivateData)
            reasons.Add("private_data_requires_quarantine");
        if (HasProviderMissingStatus(record))
            reasons.Add("provider_missing_or_blocked");
        if (HasNoEvidence(record))
            reasons.Add("evidence_missing");

        return reasons.Count == 0
            ? "not_blocked"
            : string.Join(";", reasons.Distinct(StringComparer.Ordinal));
    }

    private static string BuildRightsBoundary(DesignConversionDpoTrainingRecord record)
    {
        if (record.License.ContainsUserPrivateData)
            return "blocked_private_data";
        return record.License.AllowsTraining
            ? "training_allowed_by_record_license_scope"
            : "training_blocked_by_record_license_scope";
    }

    private static string BuildDisplaySplit(DesignConversionDpoTrainingRecord record)
    {
        if (record.Provenance.Metadata.TryGetValue("split", out var split) && !string.IsNullOrWhiteSpace(split))
            return SafeDisplayToken(split, "unknown");
        if (IsHeldOut(record))
            return "held_out";

        return "unknown";
    }

    private static string SafeDisplayToken(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var sanitized = DesignConversionDpoLeakageGuard.SanitizeScalar(value, "dpo_display_token", redactions);
        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }
}
