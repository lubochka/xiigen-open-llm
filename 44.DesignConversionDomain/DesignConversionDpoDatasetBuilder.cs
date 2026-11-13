// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AgentInfrastructure;

public sealed record DesignConversionDpoDatasetPreflight(
    bool ApprovalPreflightPassed,
    bool SourceManifestPreflightPassed,
    bool SourceDisjointDeclared,
    bool AllowsDatasetRowConstruction,
    bool TrainingCandidateOnly,
    bool ProductReady,
    string RouteClassifier,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> EvidenceRefs)
{
    public const string ApprovalPreflightMissingRoute = "approval_preflight_missing";
    public const string ApprovalPreflightFailedRoute = "approval_preflight_failed";
    public const string SourceManifestPreflightMissingRoute = "source_manifest_preflight_missing";
    public const string SourceDisjointPreflightMissingRoute = "source_disjoint_preflight_missing";
    public const string TrainingCandidateOnlyRoute = "training_candidate_only";

    public static DesignConversionDpoDatasetPreflight MissingApproval()
        => new(
            ApprovalPreflightPassed: false,
            SourceManifestPreflightPassed: false,
            SourceDisjointDeclared: false,
            AllowsDatasetRowConstruction: false,
            TrainingCandidateOnly: false,
            ProductReady: false,
            RouteClassifier: ApprovalPreflightMissingRoute,
            Reasons:
            [
                ApprovalPreflightMissingRoute,
                SourceManifestPreflightMissingRoute,
                SourceDisjointPreflightMissingRoute,
                "product_not_ready"
            ],
            EvidenceRefs:
            [
                "approval_preflight:missing",
                "source_manifest:missing",
                "source_disjoint:not_declared",
                "product_not_ready"
            ]);

    public static DesignConversionDpoDatasetPreflight MissingSourceManifestAfterApproval(
        IReadOnlyList<string>? evidenceRefs = null)
        => new(
            ApprovalPreflightPassed: true,
            SourceManifestPreflightPassed: false,
            SourceDisjointDeclared: false,
            AllowsDatasetRowConstruction: false,
            TrainingCandidateOnly: false,
            ProductReady: false,
            RouteClassifier: SourceManifestPreflightMissingRoute,
            Reasons:
            [
                SourceManifestPreflightMissingRoute,
                SourceDisjointPreflightMissingRoute,
                "product_not_ready"
            ],
            EvidenceRefs: (evidenceRefs ?? [])
                .Concat(
                [
                    "approval_preflight:passed",
                    "source_manifest:missing",
                    "source_disjoint:not_declared",
                    "product_not_ready"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray());

    public static DesignConversionDpoDatasetPreflight LocalFixtureTrainingCandidateOnly(
        string evidenceRef = "local_fixture_structural_preflight")
        => new(
            ApprovalPreflightPassed: true,
            SourceManifestPreflightPassed: true,
            SourceDisjointDeclared: true,
            AllowsDatasetRowConstruction: true,
            TrainingCandidateOnly: true,
            ProductReady: false,
            RouteClassifier: TrainingCandidateOnlyRoute,
            Reasons:
            [
                "approval_preflight:local_fixture_not_real_owner_approval",
                "source_manifest:local_fixture_structural_manifest",
                "source_disjoint:local_fixture_source_grouping_declared",
                TrainingCandidateOnlyRoute,
                "product_not_ready"
            ],
            EvidenceRefs:
            [
                evidenceRef,
                "approval_preflight",
                "source_manifest",
                "source_disjoint",
                TrainingCandidateOnlyRoute,
                "product_not_ready"
            ]);

    public static DesignConversionDpoDatasetPreflight LocalTrainingIntakeSourceManifestDisjointProof(
        IReadOnlyList<string>? evidenceRefs = null)
        => new(
            ApprovalPreflightPassed: true,
            SourceManifestPreflightPassed: true,
            SourceDisjointDeclared: true,
            AllowsDatasetRowConstruction: true,
            TrainingCandidateOnly: true,
            ProductReady: false,
            RouteClassifier: TrainingCandidateOnlyRoute,
            Reasons:
            [
                "approval_preflight:approved_training_intake",
                "source_manifest:local_intake_manifest_hashes",
                "source_disjoint:local_intake_source_hash_split_proven",
                TrainingCandidateOnlyRoute,
                "product_not_ready"
            ],
            EvidenceRefs: (evidenceRefs ?? [])
                .Concat(
                [
                    "approval_preflight:approved_training_intake",
                    "source_manifest:local_intake_manifest_hashes",
                    "source_disjoint:local_intake_source_hash_split_proven",
                    TrainingCandidateOnlyRoute,
                    "product_not_ready"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray());

    public static DesignConversionDpoDatasetPreflight FromRealS3Validation(
        DesignConversionRealS3ApprovalPacketValidationResult? approval,
        DesignConversionRealS3EvidenceBundleValidationResult? bundle,
        bool sourceDisjointDeclared)
    {
        if (approval is null || !approval.CanResumeRealDataArtifactConstruction)
        {
            return new DesignConversionDpoDatasetPreflight(
                ApprovalPreflightPassed: false,
                SourceManifestPreflightPassed: false,
                SourceDisjointDeclared: sourceDisjointDeclared,
                AllowsDatasetRowConstruction: false,
                TrainingCandidateOnly: false,
                ProductReady: false,
                RouteClassifier: ApprovalPreflightFailedRoute,
                Reasons: (approval?.Reasons ?? [ApprovalPreflightFailedRoute])
                    .Append("product_not_ready")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceRefs: (approval?.EvidenceRefs ?? [])
                    .Concat(["approval_preflight:failed", "product_not_ready"])
                    .Distinct(StringComparer.Ordinal)
                    .ToArray());
        }

        if (bundle is null || !bundle.SourceManifestReady)
            return MissingSourceManifestAfterApproval(approval.EvidenceRefs);

        if (!sourceDisjointDeclared)
        {
            return new DesignConversionDpoDatasetPreflight(
                ApprovalPreflightPassed: true,
                SourceManifestPreflightPassed: true,
                SourceDisjointDeclared: false,
                AllowsDatasetRowConstruction: false,
                TrainingCandidateOnly: false,
                ProductReady: false,
                RouteClassifier: SourceDisjointPreflightMissingRoute,
                Reasons: [SourceDisjointPreflightMissingRoute, "product_not_ready"],
                EvidenceRefs: approval.EvidenceRefs
                    .Concat(bundle.EvidenceRefs)
                    .Concat(["approval_preflight", "source_manifest", "source_disjoint:not_declared", "product_not_ready"])
                    .Distinct(StringComparer.Ordinal)
                    .ToArray());
        }

        return new DesignConversionDpoDatasetPreflight(
            ApprovalPreflightPassed: true,
            SourceManifestPreflightPassed: true,
            SourceDisjointDeclared: true,
            AllowsDatasetRowConstruction: true,
            TrainingCandidateOnly: true,
            ProductReady: false,
            RouteClassifier: TrainingCandidateOnlyRoute,
            Reasons:
            [
                "approval_preflight:validated",
                "source_manifest:validated",
                "source_disjoint:declared",
                TrainingCandidateOnlyRoute,
                "product_not_ready"
            ],
            EvidenceRefs: approval.EvidenceRefs
                .Concat(bundle.EvidenceRefs)
                .Concat(["approval_preflight", "source_manifest", "source_disjoint", TrainingCandidateOnlyRoute, "product_not_ready"])
                .Distinct(StringComparer.Ordinal)
                .ToArray());
    }
}

public sealed record DesignConversionDpoDatasetOptions(
    string DatasetId = "dcm-dpo-dataset",
    int MinimumTrainRecords = 50,
    int MinimumHeldOutRecords = 50,
    double HeldOutRecordFraction = 0.2,
    string DomainTag = DesignConversionMachine.DomainTag,
    DesignConversionDpoDatasetPreflight? Preflight = null);

public sealed record DesignConversionDpoDatasetCountGate(
    int RequiredTrainRecords,
    int RequiredHeldOutRecords,
    int ActualTrainRecords,
    int ActualHeldOutRecords,
    bool SourceDisjoint,
    bool Passed,
    IReadOnlyList<string> Reasons);

public sealed record DesignConversionDpoDatasetResultMetadata(
    string ChosenResultHash,
    string RejectedResultHash,
    double ChosenScore,
    double RejectedScore,
    bool ChosenAccepted,
    bool RejectedAccepted,
    IReadOnlyList<string> DefectCategories);

public sealed record DesignConversionDpoDatasetSafeCandidateResult(
    [property: JsonPropertyName("result_ref")] string ResultRef,
    [property: JsonPropertyName("result_hash")] string ResultHash,
    [property: JsonPropertyName("visual_score")] double VisualScore,
    [property: JsonPropertyName("accepted")] bool Accepted);

public sealed record DesignConversionDpoDatasetCandidateResult(
    [property: JsonPropertyName("chosen")] DesignConversionDpoDatasetSafeCandidateResult Chosen,
    [property: JsonPropertyName("rejected")] DesignConversionDpoDatasetSafeCandidateResult Rejected);

public sealed record DesignConversionDpoDatasetTeacherLabels(
    [property: JsonPropertyName("preferred_result_ref")] string PreferredResultRef,
    [property: JsonPropertyName("rejected_result_ref")] string RejectedResultRef,
    [property: JsonPropertyName("preference_label")] string PreferenceLabel,
    [property: JsonPropertyName("score_margin")] double ScoreMargin,
    [property: JsonPropertyName("pair_family")] string PairFamily,
    [property: JsonPropertyName("defect_categories")] IReadOnlyList<string> DefectCategories);

public sealed record DesignConversionDpoDatasetValidationOutcome(
    [property: JsonPropertyName("accepted_for_training")] bool AcceptedForTraining,
    [property: JsonPropertyName("training_grade")] bool TrainingGrade,
    [property: JsonPropertyName("not_training_grade")] bool NotTrainingGrade,
    [property: JsonPropertyName("training_grade_boundary")] string TrainingGradeBoundary,
    [property: JsonPropertyName("quarantined")] bool Quarantined,
    [property: JsonPropertyName("license_allows_training")] bool LicenseAllowsTraining,
    [property: JsonPropertyName("contains_user_private_data")] bool ContainsUserPrivateData,
    [property: JsonPropertyName("unit_id_present")] bool UnitIdPresent,
    [property: JsonPropertyName("outcome")] string Outcome);

public sealed record DesignConversionDpoDatasetRow(
    string Split,
    string RowId,
    string RecordIdentityHash,
    string UnitId,
    string DomainTag,
    string SourceKind,
    string SourceIdentityHash,
    string SourceGroupId,
    string PairFamily,
    double UsefulnessScore,
    string Prompt,
    string Chosen,
    string Rejected,
    DesignConversionDpoDatasetResultMetadata Result,
    DesignConversionDpoDatasetCandidateResult CandidateResult,
    DesignConversionDpoDatasetTeacherLabels TeacherLabels,
    string TeachingPoint,
    string CurriculumTier,
    DesignConversionDpoDatasetValidationOutcome ValidationOutcome,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionDpoDatasetSourceGroup(
    string SourceGroupId,
    string SourceIdentityHash,
    string SourceKind,
    int RecordCount,
    IReadOnlyList<string> RecordIdentityHashes);

public sealed record DesignConversionDpoDatasetExcludedRecord(
    string RecordIdentityHash,
    string UnitId,
    string Reason);

public sealed record DesignConversionDpoDatasetManifest(
    string DatasetId,
    string DomainTag,
    DateTimeOffset CreatedAtUtc,
    int AcceptedRecordCount,
    int QuarantinedRecordCount,
    int ExcludedRecordCount,
    DesignConversionDpoDatasetCountGate CountGate,
    DesignConversionDpoDatasetPreflight Preflight,
    IReadOnlyList<DesignConversionDpoDatasetSourceGroup> TrainSources,
    IReadOnlyList<DesignConversionDpoDatasetSourceGroup> HeldOutSources,
    IReadOnlyList<DesignConversionDpoDatasetExcludedRecord> ExcludedRecords,
    IReadOnlyList<string> EvidenceRefs);

/// <summary>
/// Optimizer-only JSONL schema. Its five-field public constructor is an intentional
/// security boundary; restoring audit fields for constructor compatibility would
/// reintroduce outcome and identity shortcuts into optimizer input.
/// </summary>
public sealed record DesignConversionDpoDatasetJsonlRow(
    string DomainTag,
    string UnitId,
    string Prompt,
    string Chosen,
    string Rejected);

public sealed record DesignConversionDpoDatasetAuditIdentity(
    string DatasetId,
    string RowId,
    string RecordIdentityHash,
    string DomainTag,
    string UnitId,
    string SourceKind,
    string SourceIdentityHash,
    string SourceGroupId);

public sealed record DesignConversionDpoDatasetAuditCustody(
    bool Required,
    string Version,
    string OptimizerPromptSha256,
    string PreferredResponseSha256,
    string NonPreferredResponseSha256,
    string GovernanceSha256,
    string TrainingRecordBindingSha256,
    bool RawPayloadInPublicRecord);

public sealed record DesignConversionDpoDatasetAuditResult(
    string PreferredResultHash,
    string NonPreferredResultHash,
    double PreferredScore,
    double NonPreferredScore,
    bool PreferredAccepted,
    bool NonPreferredAccepted,
    IReadOnlyList<string> DefectCategories);

public sealed record DesignConversionDpoDatasetAuditLabels(
    string PreferredResultRef,
    string NonPreferredResultRef,
    string PreferenceLabel,
    double ScoreMargin,
    string PairFamily,
    IReadOnlyList<string> DefectCategories);

public sealed record DesignConversionDpoDatasetAuditReview(
    string ReviewerIdentityHash,
    string ApprovalStatus,
    string ReviewStatus,
    bool ApprovedForTraining,
    bool RawReviewerReferenceIncluded);

public sealed record DesignConversionDpoDatasetAuditJsonlRow(
    string Split,
    DesignConversionDpoDatasetAuditIdentity Identity,
    DesignConversionDpoDatasetAuditCustody Custody,
    DesignConversionDpoDatasetAuditResult Result,
    DesignConversionDpoDatasetAuditLabels Labels,
    DesignConversionDpoDatasetAuditReview Review,
    DesignConversionDpoDatasetValidationOutcome Validation,
    bool TrainingGrade);

internal static class DesignConversionDpoDatasetAuditProjection
{
    internal const int MaximumDefectCategoryCount = 16;
    internal const int MaximumInputDefectCategoryCount = 32;
    internal const int MaximumDefectCategoryLength = 128;
    internal const int MaximumDefectCategoryUtf8ByteLength = 256;
    internal const string DefectCategoryInputOverflowMarker = "category_input_overflow";
    internal const string DefectCategoryInvalidMarker = "category_invalid_or_oversized";

    internal static string NormalizeSha256Reference(string? value)
    {
        var candidate = value ?? string.Empty;
        var hex = candidate.Length == 64
            ? candidate
            : candidate.Length == "sha256:".Length + 64
              && candidate.StartsWith("sha256:", StringComparison.Ordinal)
                ? candidate["sha256:".Length..]
                : string.Empty;
        return hex.Length == 64 && hex.All(IsAsciiHexDigit)
            ? $"sha256:{hex.ToLowerInvariant()}"
            : string.Empty;
    }

    internal static IReadOnlyList<string> ProjectDefectCategories(IEnumerable<string>? categories)
    {
        var snapshot = new List<string?>(MaximumInputDefectCategoryCount + 1);
        using var enumerator = (categories ?? []).GetEnumerator();
        while (snapshot.Count <= MaximumInputDefectCategoryCount && enumerator.MoveNext())
            snapshot.Add(enumerator.Current);
        if (snapshot.Count > MaximumInputDefectCategoryCount)
            return [DefectCategoryInputOverflowMarker];

        return snapshot
            .Select(ProjectDefectCategory)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Take(MaximumDefectCategoryCount)
            .ToArray();
    }

    private static string ProjectDefectCategory(string? category)
    {
        if (category is null || category.Length == 0 || category.Length > MaximumDefectCategoryLength)
            return DefectCategoryInvalidMarker;
        if (Encoding.UTF8.GetByteCount(category) > MaximumDefectCategoryUtf8ByteLength)
            return DefectCategoryInvalidMarker;

        var bounded = category.Trim();
        if (bounded.Length == 0)
            return DefectCategoryInvalidMarker;
        return DesignConversionNegativeReasonTaxonomy.IsAllowed(bounded)
            ? bounded
            : $"unknown:{DesignConversionDpoGenerator.Sha256Ref(bounded)}";
    }

    private static bool IsAsciiHexDigit(char value)
        => value is >= '0' and <= '9'
           or >= 'a' and <= 'f'
           or >= 'A' and <= 'F';
}

public sealed record DesignConversionDpoDataset(
    string DatasetId,
    DesignConversionDpoDatasetManifest Manifest,
    IReadOnlyList<DesignConversionDpoDatasetRow> TrainRows,
    IReadOnlyList<DesignConversionDpoDatasetRow> HeldOutRows)
{
    internal const string OptimizerJsonlSchemaVersion = "dcm-dpo-optimizer-jsonl-v3";
    internal const string AuditJsonlSchemaVersion = "dcm-dpo-audit-jsonl-v1";

    private static readonly JsonSerializerOptions JsonlOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    public IReadOnlyList<DesignConversionDpoDatasetRow> Rows =>
        TrainRows.Concat(HeldOutRows).ToArray();

    public IReadOnlyList<DesignConversionDpoDatasetJsonlRow> ToJsonlRows()
        => TrainRows.Select(row => new DesignConversionDpoDatasetJsonlRow(
            DomainTag: row.DomainTag,
            UnitId: row.UnitId,
            Prompt: row.Prompt,
            Chosen: row.Chosen,
            Rejected: row.Rejected)).ToArray();

    public IReadOnlyList<string> ToJsonlLines()
        => ToJsonlRows()
            .Select(row => JsonSerializer.Serialize(row, JsonlOptions))
            .ToArray();

    public IReadOnlyList<DesignConversionDpoDatasetAuditJsonlRow> ToAuditJsonlRows()
        => Rows.Select(ToAuditJsonlRow).ToArray();

    private DesignConversionDpoDatasetAuditJsonlRow ToAuditJsonlRow(DesignConversionDpoDatasetRow row)
    {
        var defectCategories = DesignConversionDpoDatasetAuditProjection.ProjectDefectCategories(
            row.Result.DefectCategories.Concat(row.TeacherLabels.DefectCategories));
        return new DesignConversionDpoDatasetAuditJsonlRow(
            Split: row.Split,
            Identity: new DesignConversionDpoDatasetAuditIdentity(
                DatasetId: DatasetId,
                RowId: row.RowId,
                RecordIdentityHash: row.RecordIdentityHash,
                DomainTag: row.DomainTag,
                UnitId: row.UnitId,
                SourceKind: row.SourceKind,
                SourceIdentityHash: row.SourceIdentityHash,
                SourceGroupId: row.SourceGroupId),
            Custody: new DesignConversionDpoDatasetAuditCustody(
                Required: ReadAuditBoolean(row.Metadata, "payload_custody_required"),
                Version: ReadAuditToken(row.Metadata, "payload_custody_version"),
                OptimizerPromptSha256: ReadAuditSha256Reference(row.Metadata, "payload_custody_prompt_sha256"),
                PreferredResponseSha256: ReadAuditSha256Reference(row.Metadata, "payload_custody_chosen_response_sha256"),
                NonPreferredResponseSha256: ReadAuditSha256Reference(row.Metadata, "payload_custody_rejected_response_sha256"),
                GovernanceSha256: ReadAuditSha256Reference(row.Metadata, "payload_custody_governance_sha256"),
                TrainingRecordBindingSha256: ReadAuditSha256Reference(row.Metadata, "payload_custody_training_record_binding_sha256"),
                RawPayloadInPublicRecord: ReadAuditBoolean(row.Metadata, "payload_custody_raw_in_public_record")),
            Result: new DesignConversionDpoDatasetAuditResult(
                PreferredResultHash: row.Result.ChosenResultHash,
                NonPreferredResultHash: row.Result.RejectedResultHash,
                PreferredScore: row.Result.ChosenScore,
                NonPreferredScore: row.Result.RejectedScore,
                PreferredAccepted: row.Result.ChosenAccepted,
                NonPreferredAccepted: row.Result.RejectedAccepted,
                DefectCategories: defectCategories),
            Labels: new DesignConversionDpoDatasetAuditLabels(
                PreferredResultRef: row.TeacherLabels.PreferredResultRef,
                NonPreferredResultRef: row.TeacherLabels.RejectedResultRef,
                PreferenceLabel: row.TeacherLabels.PreferenceLabel,
                ScoreMargin: row.TeacherLabels.ScoreMargin,
                PairFamily: row.TeacherLabels.PairFamily,
                DefectCategories: defectCategories),
            Review: new DesignConversionDpoDatasetAuditReview(
                ReviewerIdentityHash: ReadAuditSha256Reference(row.Metadata, "reviewer_identity_hash"),
                ApprovalStatus: ReadAuditToken(row.Metadata, "approval_status"),
                ReviewStatus: ReadAuditToken(row.Metadata, "review_status"),
                ApprovedForTraining: ReadAuditBoolean(row.Metadata, "approved_for_training"),
                RawReviewerReferenceIncluded: ReadAuditBoolean(row.Metadata, "raw_reviewer_ref_included")),
            Validation: row.ValidationOutcome,
            TrainingGrade: row.ValidationOutcome.TrainingGrade);
    }

    public IReadOnlyList<string> ToAuditJsonlLines()
        => ToAuditJsonlRows()
            .Select(row => JsonSerializer.Serialize(row, JsonlOptions))
            .ToArray();

    private static string ReadAuditSha256Reference(IReadOnlyDictionary<string, string> metadata, string key)
        => DesignConversionDpoDatasetAuditProjection.NormalizeSha256Reference(
            metadata.TryGetValue(key, out var value) ? value : null);

    private static string ReadAuditToken(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value)
           && value.Length is > 0 and <= 96
           && value.All(character => char.IsLetterOrDigit(character) || character is '-' or '_' or '.' or ':')
            ? value
            : string.Empty;

    private static bool ReadAuditBoolean(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value)
           && bool.TryParse(value, out var parsed)
           && parsed;
}

public sealed class DesignConversionDpoDatasetBuilder
{
    private static readonly ConditionalWeakTable<
        DesignConversionDpoDataset,
        CanonicalSourceProof> CanonicalSourceProofs = new();

    private static readonly string[] AllowedSemanticSourceFeatureNames =
    [
        "target_unit_is_critic",
        "target_unit_is_improver",
        "source_json_node_count",
        "source_json_max_depth",
        "source_object_property_count",
        "source_array_item_count",
        "source_frame_like_count",
        "source_text_like_count",
        "source_image_like_count",
        "source_component_like_count",
        "source_layout_hint_count",
        "source_breakpoint_hint_count",
        "source_size_hint_count"
    ];

    private static readonly string[] AllowedStructureSummaryKeys =
    [
        "nodes",
        "depth",
        "properties",
        "arrays",
        "layout_hints",
        "breakpoint_hints",
        "size_hints"
    ];

    private static readonly HashSet<string> ForbiddenPromptIdentityTokens = new(StringComparer.Ordinal)
    {
        "id", "identifier", "identity", "ref", "reference", "hash", "digest", "checksum",
        "sha", "sha256", "path", "filepath", "filename", "file", "uri", "url", "manifest",
        "checkpoint", "artifact", "output", "result", "reviewer", "score", "verdict", "accepted",
        "rejected", "passed", "failed", "approval"
    };

    private const string CanonicalSemanticShortcutGuard =
        "source_kind_source_id_not_vector_features";

    private static readonly JsonSerializerOptions CompactJsonOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    public const string TrainSplit = "train";
    public const string HeldOutSplit = "held_out";

    public OperationResult<DesignConversionDpoDataset> BuildDataset(
        DesignConversionDpoBuildResult build,
        DesignConversionDpoDatasetOptions? options = null)
    {
        if (build is null)
            return OperationResult<DesignConversionDpoDataset>.Fail(
                "DesignConversion DPO build result is required.",
                "DESIGN_CONVERSION_DPO_BUILD_RESULT_REQUIRED");

        return BuildDatasetCore(build.AllRecords, build.QuarantinedRecords.Count, options);
    }

    public OperationResult<DesignConversionDpoDataset> BuildDataset(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionDpoDatasetOptions? options = null)
        => BuildDatasetCore(records, records?.Count(record => record?.Quarantine.IsQuarantined ?? false) ?? 0, options);

    private static OperationResult<DesignConversionDpoDataset> BuildDatasetCore(
        IReadOnlyList<DesignConversionDpoTrainingRecord>? records,
        int quarantinedRecordCount,
        DesignConversionDpoDatasetOptions? options)
    {
        if (records is null)
            return OperationResult<DesignConversionDpoDataset>.Fail(
                "DesignConversion DPO records are required.",
                "DESIGN_CONVERSION_DPO_RECORDS_REQUIRED");

        var effectiveOptions = NormalizeOptions(options);
        if (effectiveOptions.Preflight is not { AllowsDatasetRowConstruction: true })
            return BuildPreflightBlockedDataset(records, quarantinedRecordCount, effectiveOptions);

        var acceptedRecords = records
            .Where(IsAcceptedTrainingRecord)
            .OrderBy(record => record.UnitId, StringComparer.Ordinal)
            .ThenByDescending(record => record.UsefulnessScore)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();
        var excluded = records
            .Where(record => !IsAcceptedTrainingRecord(record))
            .Select(record => ToExcludedRecord(record))
            .ToArray();

        var sourceGroups = acceptedRecords
            .GroupBy(BuildSourceIdentity, StringComparer.Ordinal)
            .Select(group => ToSourceGroup(group.Key, group.ToArray()))
            .OrderBy(group => group.SourceIdentityHash, StringComparer.Ordinal)
            .ThenBy(group => group.SourceKind, StringComparer.Ordinal)
            .ToArray();

        var split = SplitSourceGroups(sourceGroups, effectiveOptions);
        var trainRows = split.TrainGroups
            .SelectMany(group => group.Records.Select(record => ToDatasetRow(record, group, TrainSplit, effectiveOptions)))
            .OrderBy(row => row.SourceIdentityHash, StringComparer.Ordinal)
            .ThenBy(row => row.UnitId, StringComparer.Ordinal)
            .ThenBy(row => row.RowId, StringComparer.Ordinal)
            .ToArray();
        var heldOutRows = split.HeldOutGroups
            .SelectMany(group => group.Records.Select(record => ToDatasetRow(record, group, HeldOutSplit, effectiveOptions)))
            .OrderBy(row => row.SourceIdentityHash, StringComparer.Ordinal)
            .ThenBy(row => row.UnitId, StringComparer.Ordinal)
            .ThenBy(row => row.RowId, StringComparer.Ordinal)
            .ToArray();

        var sourceDisjoint = IsSourceDisjoint(trainRows, heldOutRows);
        var countGate = BuildCountGate(trainRows.Length, heldOutRows.Length, sourceDisjoint, effectiveOptions);
        var evidenceRefs = BuildEvidenceRefs(countGate, effectiveOptions.Preflight!).ToArray();
        var manifest = new DesignConversionDpoDatasetManifest(
            DatasetId: effectiveOptions.DatasetId,
            DomainTag: effectiveOptions.DomainTag,
            CreatedAtUtc: DateTimeOffset.UtcNow,
            AcceptedRecordCount: acceptedRecords.Length,
            QuarantinedRecordCount: quarantinedRecordCount,
            ExcludedRecordCount: excluded.Length,
            CountGate: countGate,
            Preflight: effectiveOptions.Preflight!,
            TrainSources: BuildManifestSources(trainRows),
            HeldOutSources: BuildManifestSources(heldOutRows),
            ExcludedRecords: excluded,
            EvidenceRefs: evidenceRefs);

        var dataset = new DesignConversionDpoDataset(
            DatasetId: effectiveOptions.DatasetId,
            Manifest: manifest,
            TrainRows: trainRows,
            HeldOutRows: heldOutRows);
        RegisterCanonicalSourceProof(dataset);
        return OperationResult<DesignConversionDpoDataset>.Ok(dataset);
    }

    /// <summary>
    /// Converts one server-minted admission into the only dataset projection
    /// that can carry training-grade rows. The source records and handoff rows
    /// remain review-only and custody-identical.
    /// </summary>
    internal OperationResult<DesignConversionDpoAdmittedCorpus>
        BuildAdmittedCorpus(
            DesignConversionDpoCorpusAdmission admission,
            DesignConversionTrainingPipelineOptions? callerSnapshotOptions = null)
    {
        if (admission is null ||
            !TryAuthorizeCanonicalSourceProof(
                admission.SourceProof,
                admission.Originals,
                out var proofSha256) ||
            !string.Equals(
                proofSha256,
                admission.SourceProofSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionDpoAdmittedCorpus>.Fail(
                "A live server-owned corpus admission and its canonical source proof are required.",
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode);
        }

        var admittedRecordHashes = admission.Originals
            .Select(ComputeRecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        var trainRows = admission.SourceProof.TrainRows.Select(row =>
        {
            if (!admittedRecordHashes.Contains(row.RecordIdentityHash))
                return row;

            var metadata = new Dictionary<string, string>(row.Metadata, StringComparer.Ordinal)
            {
                ["runtime_teacher_authored_candidate"] = bool.TrueString,
                ["runtime_teacher_selected_preference"] = bool.FalseString,
                ["training_candidate_only"] = bool.FalseString,
                ["training_grade"] = bool.TrueString,
                ["not_training_grade"] = bool.FalseString,
                ["training_grade_boundary"] = "server_owned_corpus_admission",
                ["corpus_admission_authority"] = "server_owned_one_shot",
                ["corpus_admission_receipt_sha256"] = admission.ReceiptSha256,
                ["corpus_admission_source_proof_sha256"] = admission.SourceProofSha256,
                ["product_not_ready"] = bool.TrueString,
                ["trained_feature_claimed"] = bool.FalseString,
                ["product_ready_verified"] = bool.FalseString
            };
            return row with
            {
                ValidationOutcome = row.ValidationOutcome with
                {
                    TrainingGrade = true,
                    NotTrainingGrade = false,
                    TrainingGradeBoundary = "server_owned_corpus_admission",
                    Outcome = "accepted_training_grade_record"
                },
                Metadata = metadata
            };
        }).ToArray();

        if (trainRows.Length != 2 ||
            trainRows.Any(row => !admittedRecordHashes.Contains(row.RecordIdentityHash) ||
                                 !row.ValidationOutcome.TrainingGrade) ||
            admission.SourceProof.HeldOutRows.Count == 0)
        {
            return OperationResult<DesignConversionDpoAdmittedCorpus>.Fail(
                "The corpus proof must contain exactly the admitted pair in train and a disjoint held-out witness.",
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode);
        }

        var preflight = admission.SourceProof.Manifest.Preflight with
        {
            TrainingCandidateOnly = false,
            ProductReady = false,
            RouteClassifier = "server_owned_corpus_admission",
            Reasons = admission.SourceProof.Manifest.Preflight.Reasons
                .Append("server_owned_corpus_admission")
                .Append("product_not_ready")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            EvidenceRefs = admission.SourceProof.Manifest.Preflight.EvidenceRefs
                .Append(admission.SourceProofSha256)
                .Append(admission.ReceiptSha256)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
        var manifest = admission.SourceProof.Manifest with
        {
            Preflight = preflight,
            EvidenceRefs = admission.SourceProof.Manifest.EvidenceRefs
                .Append("server_owned_corpus_admission")
                .Append(admission.SourceProofSha256)
                .Append(admission.ReceiptSha256)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
        var dataset = admission.SourceProof with
        {
            Manifest = manifest,
            TrainRows = trainRows
        };
        var options = new DesignConversionTrainingPipelineOptions(
            RunId: $"corpus-admission:{admission.ReceiptSha256[7..23]}",
            BatchSize: 2,
            CallMachineLifecycleHook: true,
            RequireDatasetCountGate: true,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: dataset.DatasetId,
                MinimumTrainRecords: dataset.Manifest.CountGate.RequiredTrainRecords,
                MinimumHeldOutRecords: dataset.Manifest.CountGate.RequiredHeldOutRecords,
                HeldOutRecordFraction: 0.2,
                Preflight: preflight),
            MeasurementGateRequest: callerSnapshotOptions?.MeasurementGateRequest,
            DpoPolicyBridgeOptions: callerSnapshotOptions?.DpoPolicyBridgeOptions);
        var corpus = new DesignConversionDpoAdmittedCorpus(admission, dataset, options);
        if (!admission.TryBindCorpus(corpus))
        {
            return OperationResult<DesignConversionDpoAdmittedCorpus>.Fail(
                "The corpus admission was already projected or consumed.",
                DesignConversionDpoCorpusAdmissionAuthority.ReplayErrorCode);
        }

        return OperationResult<DesignConversionDpoAdmittedCorpus>.Ok(corpus);
    }

    internal static bool TryAuthorizeCanonicalSourceProof(
        DesignConversionDpoDataset dataset,
        IReadOnlyList<DesignConversionDpoTrainingRecord> exactTrainRecords,
        out string proofSha256)
    {
        proofSha256 = string.Empty;
        if (dataset is null || exactTrainRecords is null ||
            !CanonicalSourceProofs.TryGetValue(dataset, out var proof) ||
            exactTrainRecords.Count != 2)
            return false;

        if (!TryComputeCanonicalSourceProofDatasetFingerprint(
                dataset,
                out var currentDatasetFingerprintSha256))
            return false;

        if (!string.Equals(
                proof.DatasetFingerprintSha256,
                currentDatasetFingerprintSha256,
                StringComparison.Ordinal))
            return false;

        var exactHashes = exactTrainRecords
            .Select(ComputeRecordIdentityHash)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (!proof.TrainRecordIdentityHashes.SequenceEqual(
                exactHashes,
                StringComparer.Ordinal))
            return false;

        proofSha256 = proof.ProofSha256;
        return true;
    }

    private static void RegisterCanonicalSourceProof(DesignConversionDpoDataset dataset)
    {
        if (!dataset.Manifest.CountGate.Passed ||
            dataset.TrainRows.Count == 0 ||
            dataset.HeldOutRows.Count == 0 ||
            !dataset.Manifest.CountGate.SourceDisjoint ||
            dataset.TrainRows.Any(static row => row.ValidationOutcome.TrainingGrade) ||
            dataset.Manifest.TrainSources.Select(static source => source.SourceIdentityHash)
                .Intersect(
                    dataset.Manifest.HeldOutSources.Select(static source => source.SourceIdentityHash),
                    StringComparer.Ordinal)
                .Any())
            return;

        var trainHashes = dataset.TrainRows
            .Select(static row => row.RecordIdentityHash)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var heldOutHashes = dataset.HeldOutRows
            .Select(static row => row.RecordIdentityHash)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (!TryComputeCanonicalSourceProofDatasetFingerprint(
                dataset,
                out var datasetFingerprintSha256))
            return;

        var proofSha256 = DesignConversionDpoGenerator.Sha256Ref(string.Join(
            "|",
            new[]
            {
                "dcm-canonical-source-disjoint-proof-v2",
                dataset.DatasetId,
                datasetFingerprintSha256,
                string.Join(",", trainHashes),
                string.Join(",", heldOutHashes),
                string.Join(",", dataset.Manifest.TrainSources
                    .Select(static source => source.SourceIdentityHash)
                    .Order(StringComparer.Ordinal)),
                string.Join(",", dataset.Manifest.HeldOutSources
                    .Select(static source => source.SourceIdentityHash)
                    .Order(StringComparer.Ordinal))
            }));
        CanonicalSourceProofs.Add(
            dataset,
            new CanonicalSourceProof(
                proofSha256,
                datasetFingerprintSha256,
                trainHashes));
    }

    private static bool TryComputeCanonicalSourceProofDatasetFingerprint(
        DesignConversionDpoDataset dataset,
        out string fingerprintSha256)
    {
        fingerprintSha256 = string.Empty;
        if (dataset.Manifest is null ||
            dataset.TrainRows is null ||
            dataset.HeldOutRows is null ||
            dataset.TrainRows.Any(static row => !IsCanonicalSourceProofRowWellFormed(row)) ||
            dataset.HeldOutRows.Any(static row => !IsCanonicalSourceProofRowWellFormed(row)))
        {
            return false;
        }

        // Каноническое доказательство происхождения привязано не только к
        // объекту и отпечаткам идентичности записей: повторная проверка
        // пересчитывает полный снимок содержимого, чтобы подмена строки внутри
        // того же массива не получила прежний допуск.
        var payload = new CanonicalSourceProofDatasetFingerprint(
            dataset.DatasetId,
            dataset.Manifest,
            dataset.TrainRows
                .Select(ToCanonicalSourceProofRowFingerprint)
                .ToArray(),
            dataset.HeldOutRows
                .Select(ToCanonicalSourceProofRowFingerprint)
                .ToArray());
        fingerprintSha256 = DesignConversionDpoGenerator.Sha256Ref(
            JsonSerializer.Serialize(payload, CompactJsonOptions));
        return true;
    }

    private static bool IsCanonicalSourceProofRowWellFormed(
        DesignConversionDpoDatasetRow? row)
        => row is not null &&
           row.Metadata is not null &&
           row.Result is not null &&
           row.CandidateResult is not null &&
           row.TeacherLabels is not null &&
           row.ValidationOutcome is not null &&
           double.IsFinite(row.UsefulnessScore) &&
           AllSourceProofStringsPresent(
               row.Split,
               row.RowId,
               row.RecordIdentityHash,
               row.UnitId,
               row.DomainTag,
               row.SourceKind,
               row.SourceIdentityHash,
               row.SourceGroupId,
               row.PairFamily,
               row.Prompt,
               row.Chosen,
               row.Rejected,
               row.TeachingPoint,
               row.CurriculumTier) &&
           row.Metadata.All(static pair => pair.Key is not null &&
                                           pair.Value is not null);

    private static bool AllSourceProofStringsPresent(params string[] values)
        => values.All(static value => value is not null);

    private static CanonicalSourceProofRowFingerprint
        ToCanonicalSourceProofRowFingerprint(DesignConversionDpoDatasetRow row)
        => new(
            row.Split,
            row.RowId,
            row.RecordIdentityHash,
            row.UnitId,
            row.DomainTag,
            row.SourceKind,
            row.SourceIdentityHash,
            row.SourceGroupId,
            row.PairFamily,
            row.UsefulnessScore,
            row.Prompt,
            row.Chosen,
            row.Rejected,
            row.Result,
            row.CandidateResult,
            row.TeacherLabels,
            row.TeachingPoint,
            row.CurriculumTier,
            row.ValidationOutcome,
            row.Metadata
                .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
                .Select(static pair => new CanonicalSourceProofMetadataFingerprint(
                    pair.Key,
                    pair.Value))
                .ToArray());

    private static OperationResult<DesignConversionDpoDataset> BuildPreflightBlockedDataset(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        int quarantinedRecordCount,
        DesignConversionDpoDatasetOptions options)
    {
        var preflight = options.Preflight!;
        var excluded = records
            .Select(record => ToExcludedRecord(record, preflight.RouteClassifier))
            .ToArray();
        var countGate = BuildCountGate(0, 0, sourceDisjoint: false, options);
        var manifest = new DesignConversionDpoDatasetManifest(
            DatasetId: options.DatasetId,
            DomainTag: options.DomainTag,
            CreatedAtUtc: DateTimeOffset.UtcNow,
            AcceptedRecordCount: 0,
            QuarantinedRecordCount: quarantinedRecordCount,
            ExcludedRecordCount: excluded.Length,
            CountGate: countGate,
            Preflight: preflight,
            TrainSources: [],
            HeldOutSources: [],
            ExcludedRecords: excluded,
            EvidenceRefs: BuildEvidenceRefs(countGate, preflight).ToArray());

        return OperationResult<DesignConversionDpoDataset>.Ok(new DesignConversionDpoDataset(
            DatasetId: options.DatasetId,
            Manifest: manifest,
            TrainRows: [],
            HeldOutRows: []));
    }

    private static DesignConversionDpoDatasetOptions NormalizeOptions(DesignConversionDpoDatasetOptions? options)
    {
        var effective = options ?? new DesignConversionDpoDatasetOptions();
        var datasetId = string.IsNullOrWhiteSpace(effective.DatasetId) ? "dcm-dpo-dataset" : effective.DatasetId;
        var domainTag = string.IsNullOrWhiteSpace(effective.DomainTag)
            ? DesignConversionMachine.DomainTag
            : effective.DomainTag;

        return effective with
        {
            DatasetId = datasetId,
            DomainTag = domainTag,
            MinimumTrainRecords = Math.Max(1, effective.MinimumTrainRecords),
            MinimumHeldOutRecords = Math.Max(1, effective.MinimumHeldOutRecords),
            HeldOutRecordFraction = Math.Clamp(effective.HeldOutRecordFraction, 0.05, 0.95),
            Preflight = effective.Preflight ?? DesignConversionDpoDatasetPreflight.MissingApproval()
        };
    }

    private static bool IsAcceptedTrainingRecord(DesignConversionDpoTrainingRecord? record)
        => DesignConversionTrainingMeasurementGatePolicy.IsRecordAllowedForTrainingCandidate(record);

    private static bool HasSourceManifestPreflightFields(DesignConversionDpoTrainingRecord record)
        => !string.IsNullOrWhiteSpace(record.Provenance.SourceManifestRef)
            && NormalizeSourceKind(record.Provenance.SourceKind) is "figma" or "canva" or "other";

    private static bool HasNegativePairQuality(DesignConversionDpoTrainingRecord record)
        => !string.IsNullOrWhiteSpace(record.NegativeQualityReason)
            && !LooksPlaceholder(record.NegativeQualityReason)
            && DesignConversionNegativeReasonTaxonomy.IsAllowed(record.NegativeReasonTaxonomy)
            && !DesignConversionNegativeReasonTaxonomy.IsQuarantineOnly(record.NegativeReasonTaxonomy);

    private static SourceGroup ToSourceGroup(string sourceIdentity, IReadOnlyList<DesignConversionDpoTrainingRecord> records)
    {
        var hash = HashIdentity(sourceIdentity);
        var sourceKind = records
            .Select(record => record.Provenance.SourceKind)
            .Select(NormalizeSourceKind)
            .FirstOrDefault(kind => !string.IsNullOrWhiteSpace(kind)) ?? "unknown";

        return new SourceGroup(
            RawIdentity: sourceIdentity,
            SourceIdentityHash: hash,
            SourceGroupId: $"dcm-source-{hash[..16]}",
            SourceKind: sourceKind,
            Records: records);
    }

    private static SplitResult SplitSourceGroups(
        IReadOnlyList<SourceGroup> sourceGroups,
        DesignConversionDpoDatasetOptions options)
    {
        if (sourceGroups.Count <= 1)
            return new SplitResult(sourceGroups.ToArray(), []);

        var totalRecords = sourceGroups.Sum(group => group.Records.Count);
        var canMeetTargetCounts = totalRecords >= options.MinimumTrainRecords + options.MinimumHeldOutRecords;
        var heldOutTarget = canMeetTargetCounts
            ? options.MinimumHeldOutRecords
            : Math.Max(1, (int)Math.Ceiling(totalRecords * options.HeldOutRecordFraction));
        heldOutTarget = Math.Min(heldOutTarget, Math.Max(1, totalRecords - 1));

        var train = new List<SourceGroup>();
        var heldOut = new List<SourceGroup>();
        var heldOutCount = 0;

        for (var i = 0; i < sourceGroups.Count; i++)
        {
            var group = sourceGroups[i];
            var remainingRecordsAfterGroup = sourceGroups.Skip(i + 1).Sum(candidate => candidate.Records.Count);
            if (heldOutCount < heldOutTarget && remainingRecordsAfterGroup > 0)
            {
                heldOut.Add(group);
                heldOutCount += group.Records.Count;
            }
            else
            {
                train.Add(group);
            }
        }

        return new SplitResult(train.ToArray(), heldOut.ToArray());
    }

    private static DesignConversionDpoDatasetRow ToDatasetRow(
        DesignConversionDpoTrainingRecord record,
        SourceGroup sourceGroup,
        string split,
        DesignConversionDpoDatasetOptions options)
    {
        var recordHash = ComputeRecordIdentityHash(record);
        var rowId = $"dcm-dpo-row-{HashIdentity($"{options.DatasetId}|{split}|{recordHash}")[..16]}";
        var pairFamily = string.IsNullOrWhiteSpace(record.NegativeReasonTaxonomy)
            ? "unknown"
            : record.NegativeReasonTaxonomy;
        var trainingGrade = IsTrainingGradeDatasetRow(record, options);
        var syntheticNegative = IsSyntheticNegative(record);
        var scaffoldNegative = syntheticNegative || IsLocalScaffold(options, record);
        var result = ToResultMetadata(record);
        var candidateResult = ToCandidateResult(result);
        var teacherLabels = ToTeacherLabels(result, pairFamily);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["schema"] = "dcm-dpo-dataset-row-v2",
            ["record_identity_scope"] = "sha256",
            ["source_identity_scope"] = "sha256",
            ["source_structure_scope"] = "sha256",
            ["result_identity_scope"] = "sha256",
            ["candidate_result_scope"] = "safe_hash_ref",
            ["teacher_labels_scope"] = "derived_from_accepted_record",
            ["validation_outcome_scope"] = "accepted_record_booleans",
            ["source_encoder"] = TryGetMetadata(record.Provenance.Metadata, "source_encoder")
                ?? DesignConversionDefaultSourceEncoder.EncoderName,
            ["source_encoder_schema"] = TryGetMetadata(record.Provenance.Metadata, "source_encoder_schema")
                ?? DesignConversionDefaultSourceEncoder.PayloadSchema,
            ["source_encoder_scope"] = "target_source_structure_provenance_only",
            ["result_features_scope"] = "result_outcome_label_aggregate_not_source_encoder",
            ["anti_leakage"] = TryGetMetadata(record.Provenance.Metadata, "anti_leakage")
                ?? "forbidden_source_fields_redacted_before_train_ready_payload",
            ["usefulness_score_role"] = TryGetMetadata(record.Provenance.Metadata, "usefulness_score_role")
                ?? "review_priority_only",
            ["curriculum_tier_role"] = "review_priority_only",
            ["visual_score_margin_role"] = "result_feature_not_source_encoder_feature",
            ["source_encoder_result_features_in_vector"] = TryGetMetadata(record.Provenance.Metadata, "result_features_in_source_vector")
                ?? bool.FalseString,
            ["source_encoder_visual_score_margin_in_vector"] = TryGetMetadata(record.Provenance.Metadata, "visual_score_margin_in_source_vector")
                ?? bool.FalseString,
            ["source_encoder_usefulness_score_in_vector"] = TryGetMetadata(record.Provenance.Metadata, "usefulness_score_in_source_vector")
                ?? bool.FalseString,
            ["source_encoder_answer_payload_in_vector"] = TryGetMetadata(record.Provenance.Metadata, "answer_payload_in_source_vector")
                ?? bool.FalseString,
            ["approval_preflight"] = options.Preflight!.ApprovalPreflightPassed ? "passed" : "failed",
            ["source_manifest"] = options.Preflight.SourceManifestPreflightPassed ? "passed" : "failed",
            ["source_disjoint"] = options.Preflight.SourceDisjointDeclared ? "declared" : "not_declared",
            ["training_candidate_only"] = options.Preflight.TrainingCandidateOnly.ToString(),
            ["training_grade"] = trainingGrade.ToString(),
            ["not_training_grade"] = (!trainingGrade).ToString(),
            ["training_grade_boundary"] = trainingGrade
                ? "real_training_quality_contract_passed"
                : "training_candidate_or_local_scaffold_not_training_grade",
            ["synthetic_negative"] = syntheticNegative.ToString(),
            ["scaffold_negative"] = scaffoldNegative.ToString(),
            ["product_not_ready"] = (!options.Preflight.ProductReady).ToString(),
            ["trained_feature_claimed"] = bool.FalseString,
            ["product_ready_verified"] = bool.FalseString,
            ["raw_source_request_id_included"] = bool.FalseString,
            ["raw_source_manifest_ref_included"] = bool.FalseString,
            ["raw_reviewer_ref_included"] = bool.FalseString,
            ["raw_license_id_included"] = bool.FalseString,
            ["raw_candidate_ids_included"] = bool.FalseString,
            ["local_paths_included"] = bool.FalseString,
            ["reviewer_identity_hash"] = string.IsNullOrWhiteSpace(record.Provenance.ReviewerRef)
                ? string.Empty
                : HashIdentity(record.Provenance.ReviewerRef),
            ["approval_status"] = ToAuditToken(TryGetMetadata(record.Provenance.Metadata, "approval_status")),
            ["review_status"] = ToAuditToken(TryGetMetadata(record.Provenance.Metadata, "review_status")),
            ["approved_for_training"] = ToAuditBooleanString(TryGetMetadata(record.Provenance.Metadata, "approved_for_training")),
            ["payload_custody_required"] = ToAuditBooleanString(TryGetMetadata(record.Provenance.Metadata, "payload_custody_required")),
            ["payload_custody_version"] = ToAuditToken(TryGetMetadata(record.Provenance.Metadata, "payload_custody_version")),
            ["payload_custody_prompt_sha256"] = DesignConversionDpoDatasetAuditProjection.NormalizeSha256Reference(TryGetMetadata(record.Provenance.Metadata, "payload_custody_prompt_sha256")),
            ["payload_custody_chosen_response_sha256"] = DesignConversionDpoDatasetAuditProjection.NormalizeSha256Reference(TryGetMetadata(record.Provenance.Metadata, "payload_custody_chosen_response_sha256")),
            ["payload_custody_rejected_response_sha256"] = DesignConversionDpoDatasetAuditProjection.NormalizeSha256Reference(TryGetMetadata(record.Provenance.Metadata, "payload_custody_rejected_response_sha256")),
            ["payload_custody_governance_sha256"] = DesignConversionDpoDatasetAuditProjection.NormalizeSha256Reference(TryGetMetadata(record.Provenance.Metadata, "payload_custody_governance_sha256")),
            ["payload_custody_training_record_binding_sha256"] = DesignConversionDpoDatasetAuditProjection.NormalizeSha256Reference(TryGetMetadata(record.Provenance.Metadata, "payload_custody_training_record_binding_sha256")),
            ["payload_custody_raw_in_public_record"] = ToAuditBooleanString(TryGetMetadata(record.Provenance.Metadata, "payload_custody_raw_in_public_record"))
        };

        return new DesignConversionDpoDatasetRow(
            Split: split,
            RowId: rowId,
            RecordIdentityHash: recordHash,
            UnitId: record.UnitId,
            DomainTag: options.DomainTag,
            SourceKind: NormalizeSourceKind(record.Provenance.SourceKind),
            SourceIdentityHash: sourceGroup.SourceIdentityHash,
            SourceGroupId: sourceGroup.SourceGroupId,
            PairFamily: pairFamily,
            UsefulnessScore: record.UsefulnessScore,
            Prompt: BuildPromptPayload(record, sourceGroup, options),
            Chosen: BuildOptimizerResponsePayload(record.Chosen),
            Rejected: BuildOptimizerResponsePayload(record.Rejected),
            Result: result,
            CandidateResult: candidateResult,
            TeacherLabels: teacherLabels,
            TeachingPoint: BuildTeachingPoint(pairFamily),
            CurriculumTier: BuildCurriculumTier(record.UsefulnessScore),
            ValidationOutcome: ToValidationOutcome(record, trainingGrade),
            Metadata: metadata);
    }

    private static string ToAuditToken(string? value)
        => value is not null
           && value.Length is > 0 and <= 96
           && value.All(character => char.IsLetterOrDigit(character) || character is '-' or '_' or '.' or ':')
            ? value
            : string.Empty;

    private static string ToAuditBooleanString(string? value)
        => bool.TryParse(value, out var parsed) ? parsed.ToString() : bool.FalseString;

    private static string BuildPromptPayload(
        DesignConversionDpoTrainingRecord record,
        SourceGroup sourceGroup,
        DesignConversionDpoDatasetOptions options)
        => BuildOptimizerPromptPayload(record, options.DomainTag);

    internal static string BuildOptimizerPromptPayload(
        DesignConversionDpoTrainingRecord record,
        string domainTag)
        => JsonSerializer.Serialize(new
        {
            domain_tag = domainTag,
            unit_id = record.UnitId,
            source_kind = NormalizeSourceKind(record.Provenance.SourceKind),
            internal_vector = (record.InternalVectorPayload ?? []).Take(16).ToArray(),
            target = new
            {
                unit_id = record.UnitId,
                machine_part = DesignConversionDefaultSourceEncoder.ToMachinePart(record.UnitId)
            },
            source = new
            {
                kind = NormalizeSourceKind(record.Provenance.SourceKind),
                structure_payload = SanitizeSemanticSourceStructurePayload(record.SourceStructurePayload)
            },
            source_encoder = new
            {
                feature_names = BuildSemanticFeatureNames(
                    TryGetMetadata(record.Provenance.Metadata, "source_encoder_feature_names")),
                structure_summary = SanitizeSemanticStructureSummary(
                    TryGetMetadata(record.Provenance.Metadata, "source_structure_signal_summary")),
                vector_prefix = (record.InternalVectorPayload ?? []).Take(16).ToArray(),
                dimensions = record.InternalVectorPayload?.Count ?? 0,
                shortcut_guard = CanonicalSemanticShortcutGuard,
            }
        }, CompactJsonOptions);

    private static IReadOnlyList<string> BuildSemanticFeatureNames(string? metadataValue)
    {
        var requested = SplitMetadataList(metadataValue)
            .Where(IsSafeSemanticIdentifier)
            .ToHashSet(StringComparer.Ordinal);
        return AllowedSemanticSourceFeatureNames
            .Where(requested.Contains)
            .ToArray();
    }

    private static bool IsSafeSemanticIdentifier(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 64 &&
           value.All(character => char.IsLetterOrDigit(character) || character == '_') &&
           !TokenizeSemanticIdentifier(value).Any(ForbiddenPromptIdentityTokens.Contains);

    private static string SanitizeSemanticStructureSummary(string? metadataValue)
    {
        var values = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var segment in (metadataValue ?? string.Empty).Split(
                     ';',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = segment.IndexOf('=');
            if (separator <= 0 || separator == segment.Length - 1)
                continue;

            var key = segment[..separator].Trim();
            var value = segment[(separator + 1)..].Trim();
            if (!AllowedStructureSummaryKeys.Contains(key, StringComparer.Ordinal) ||
                !int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ||
                parsed < 0)
            {
                continue;
            }

            values[key] = Math.Min(parsed, 1_000_000);
        }

        return string.Join(
            ";",
            AllowedStructureSummaryKeys
                .Where(values.ContainsKey)
                .Select(key => $"{key}={values[key].ToString(CultureInfo.InvariantCulture)}"));
    }

    internal static string SanitizeSemanticSourceStructurePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return "{}";

        try
        {
            var sanitized = SanitizeSemanticSourceNode(JsonNode.Parse(payload));
            return sanitized?.ToJsonString(CompactJsonOptions) ?? "{}";
        }
        catch (JsonException)
        {
            return "{}";
        }
    }

    private static JsonNode? SanitizeSemanticSourceNode(JsonNode? node)
    {
        if (node is null)
            return null;

        if (node is JsonObject obj)
        {
            var sanitized = new JsonObject();
            foreach (var property in obj)
            {
                if (IsOpaqueOrOutcomeSourceKey(property.Key))
                    continue;

                sanitized[property.Key] = SanitizeSemanticSourceNode(property.Value);
            }

            return sanitized;
        }

        if (node is JsonArray array)
        {
            var sanitized = new JsonArray();
            foreach (var item in array)
                sanitized.Add(SanitizeSemanticSourceNode(item));
            return sanitized;
        }

        var element = node.GetValue<JsonElement>();
        if (element.ValueKind != JsonValueKind.String)
            return node.DeepClone();

        var rawValue = element.GetString() ?? string.Empty;
        if (IsOpaqueScalarValue(rawValue))
            return JsonValue.Create("[redacted]");

        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var sanitizedValue = DesignConversionDpoLeakageGuard.SanitizeScalar(
            rawValue,
            "optimizer_prompt_source_structure",
            redactions);
        return JsonValue.Create(
            sanitizedValue.Contains("redacted-local-path-", StringComparison.Ordinal) ||
            sanitizedValue.Contains("redacted-user-", StringComparison.Ordinal) ||
            sanitizedValue.Contains("redacted-binary-", StringComparison.Ordinal)
                ? "[redacted]"
                : sanitizedValue);
    }

    private static bool IsOpaqueOrOutcomeSourceKey(string key)
        => TokenizeSemanticIdentifier(key).Any(ForbiddenPromptIdentityTokens.Contains);

    private static IReadOnlyList<string> TokenizeSemanticIdentifier(string value)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (!char.IsLetterOrDigit(character))
            {
                FlushToken(tokens, current);
                continue;
            }

            if (char.IsUpper(character) && current.Length > 0)
            {
                var previous = value[index - 1];
                var nextIsLower = index + 1 < value.Length && char.IsLower(value[index + 1]);
                if (char.IsLower(previous) || char.IsDigit(previous) ||
                    (char.IsUpper(previous) && nextIsLower))
                {
                    FlushToken(tokens, current);
                }
            }

            current.Append(char.ToLowerInvariant(character));
        }

        FlushToken(tokens, current);
        return tokens;
    }

    private static void FlushToken(ICollection<string> tokens, StringBuilder current)
    {
        if (current.Length == 0)
            return;

        tokens.Add(current.ToString());
        current.Clear();
    }

    private static bool IsOpaqueScalarValue(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
            return false;

        if (trimmed.StartsWith("artifact://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("manifest://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("checkpoint://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("file://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("\\\\", StringComparison.Ordinal) ||
            (trimmed.Length >= 3 && char.IsLetter(trimmed[0]) && trimmed[1] == ':' &&
             (trimmed[2] == '\\' || trimmed[2] == '/')) ||
            trimmed.StartsWith("/Users/", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var digest = trimmed.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? trimmed[7..]
            : trimmed;
        return digest.Length == 64 && digest.All(Uri.IsHexDigit);
    }

    internal static string BuildOptimizerResponsePayload(DesignConversionImprovementOutput output)
        => JsonSerializer.Serialize(new
        {
            html = output.Html,
            css = output.Css
        }, CompactJsonOptions);

    private static DesignConversionDpoDatasetResultMetadata ToResultMetadata(DesignConversionDpoTrainingRecord record)
        => new(
            ChosenResultHash: HashOutput(record.Chosen),
            RejectedResultHash: HashOutput(record.Rejected),
            ChosenScore: record.Chosen.Verdict.Score,
            RejectedScore: record.Rejected.Verdict.Score,
            ChosenAccepted: record.Chosen.Accepted,
            RejectedAccepted: record.Rejected.Accepted,
            DefectCategories: record.Chosen.Verdict.Defects
                .Concat(record.Rejected.Verdict.Defects)
                .Select(defect => defect.Category)
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(category => category, StringComparer.Ordinal)
                .ToArray());

    private static DesignConversionDpoDatasetCandidateResult ToCandidateResult(
        DesignConversionDpoDatasetResultMetadata result)
        => new(
            Chosen: ToSafeCandidateResult(result.ChosenResultHash, result.ChosenScore, result.ChosenAccepted),
            Rejected: ToSafeCandidateResult(result.RejectedResultHash, result.RejectedScore, result.RejectedAccepted));

    private static DesignConversionDpoDatasetSafeCandidateResult ToSafeCandidateResult(
        string resultHash,
        double visualScore,
        bool accepted)
        => new(
            ResultRef: ToResultRef(resultHash),
            ResultHash: resultHash,
            VisualScore: visualScore,
            Accepted: accepted);

    private static DesignConversionDpoDatasetTeacherLabels ToTeacherLabels(
        DesignConversionDpoDatasetResultMetadata result,
        string pairFamily)
        => new(
            PreferredResultRef: ToResultRef(result.ChosenResultHash),
            RejectedResultRef: ToResultRef(result.RejectedResultHash),
            PreferenceLabel: "chosen_over_rejected",
            ScoreMargin: Math.Round(result.ChosenScore - result.RejectedScore, 6),
            PairFamily: pairFamily,
            DefectCategories: result.DefectCategories);

    private static string BuildTeachingPoint(string pairFamily)
        => pairFamily == "unknown"
            ? "prefer_accepted_design_over_rejected_design"
            : $"prefer_accepted_design_over_rejected_design:{pairFamily}";

    private static string BuildCurriculumTier(double usefulnessScore)
    {
        if (usefulnessScore >= 0.85)
            return "tier_1_high_confidence";
        if (usefulnessScore >= 0.7)
            return "tier_2_standard";
        return "tier_3_review";
    }

    private static DesignConversionDpoDatasetValidationOutcome ToValidationOutcome(
        DesignConversionDpoTrainingRecord record,
        bool trainingGrade)
        => new(
            AcceptedForTraining: true,
            TrainingGrade: trainingGrade,
            NotTrainingGrade: !trainingGrade,
            TrainingGradeBoundary: trainingGrade
                ? "real_training_quality_contract_passed"
                : "training_candidate_or_local_scaffold_not_training_grade",
            Quarantined: false,
            LicenseAllowsTraining: record.License.AllowsTraining,
            ContainsUserPrivateData: record.License.ContainsUserPrivateData,
            UnitIdPresent: !string.IsNullOrWhiteSpace(record.UnitId),
            Outcome: trainingGrade ? "accepted_training_grade_record" : "accepted_training_candidate_not_training_grade");

    private static bool IsTrainingGradeDatasetRow(
        DesignConversionDpoTrainingRecord record,
        DesignConversionDpoDatasetOptions options)
        // Mutable record/preflight metadata is never corpus authority.  Only
        // BuildAdmittedCorpus can stamp a derivative dataset row training-grade.
        => false;

    private static bool IsSyntheticNegative(DesignConversionDpoTrainingRecord record)
        => string.Equals(record.NegativeReasonTaxonomy, DesignConversionNegativeReasonTaxonomy.SyntheticRuntimeNotTrainReady, StringComparison.Ordinal)
           || string.Equals(TryGetMetadata(record.Provenance.Metadata, "synthetic_negative"), bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || (record.NegativeQualityReason ?? string.Empty).Contains("synthetic", StringComparison.OrdinalIgnoreCase);

    private static bool IsLocalScaffold(
        DesignConversionDpoDatasetOptions options,
        DesignConversionDpoTrainingRecord record)
        => options.Preflight is { TrainingCandidateOnly: true, ProductReady: false }
           || string.Equals(TryGetMetadata(record.Provenance.Metadata, "scaffold_negative"), bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || string.Equals(TryGetMetadata(record.Provenance.Metadata, "local_artifact_db"), bool.TrueString, StringComparison.OrdinalIgnoreCase);

    private static DesignConversionDpoDatasetExcludedRecord ToExcludedRecord(
        DesignConversionDpoTrainingRecord? record,
        string? preflightReason = null)
    {
        if (record is null)
            return new DesignConversionDpoDatasetExcludedRecord(
                RecordIdentityHash: HashIdentity("record|null"),
                UnitId: "unknown",
                Reason: "record_required");

        var reason = record.Quarantine.IsQuarantined
            ? record.Quarantine.Reason
            : "not_trainable_for_dataset";

        if (!record.License.AllowsTraining)
            reason = AppendReason(reason, "license_blocks_training");
        if (record.License.ContainsUserPrivateData)
            reason = AppendReason(reason, "private_data_requires_quarantine");
        if (string.IsNullOrWhiteSpace(record.UnitId))
            reason = AppendReason(reason, "unit_id_required");
        if (!HasSourceManifestPreflightFields(record))
            reason = AppendReason(reason, "source_manifest_fields_required");
        if (!HasNegativePairQuality(record))
            reason = AppendReason(reason, "negative_pair_quality_missing");
        if (DesignConversionTrainingMeasurementGatePolicy.ContainsProviderMissingStatus(record))
            reason = AppendReason(reason, "provider_missing");
        if (DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(record).Contains("leakage", StringComparer.Ordinal))
            reason = AppendReason(reason, "leakage");
        if (!string.IsNullOrWhiteSpace(preflightReason))
            reason = AppendReason(reason, preflightReason);

        return new DesignConversionDpoDatasetExcludedRecord(
            RecordIdentityHash: ComputeRecordIdentityHash(record),
            UnitId: string.IsNullOrWhiteSpace(record.UnitId) ? "unknown" : record.UnitId,
            Reason: reason);
    }

    private static DesignConversionDpoDatasetCountGate BuildCountGate(
        int trainCount,
        int heldOutCount,
        bool sourceDisjoint,
        DesignConversionDpoDatasetOptions options)
    {
        var reasons = new List<string>();
        var preflight = options.Preflight ?? DesignConversionDpoDatasetPreflight.MissingApproval();
        if (trainCount < options.MinimumTrainRecords)
            reasons.Add($"TRAIN_RECORD_COUNT_BELOW_TARGET:{trainCount}<{options.MinimumTrainRecords}");
        if (heldOutCount < options.MinimumHeldOutRecords)
            reasons.Add($"HELD_OUT_RECORD_COUNT_BELOW_TARGET:{heldOutCount}<{options.MinimumHeldOutRecords}");
        if (!sourceDisjoint)
            reasons.Add("SOURCE_DISJOINT_SPLIT_FAILED");
        if (!preflight.AllowsDatasetRowConstruction)
            reasons.AddRange(preflight.Reasons);

        return new DesignConversionDpoDatasetCountGate(
            RequiredTrainRecords: options.MinimumTrainRecords,
            RequiredHeldOutRecords: options.MinimumHeldOutRecords,
            ActualTrainRecords: trainCount,
            ActualHeldOutRecords: heldOutCount,
            SourceDisjoint: sourceDisjoint,
            Passed: reasons.Count == 0,
            Reasons: reasons.Count == 0 ? ["COUNT_GATE_PASSED"] : reasons.ToArray());
    }

    private static IReadOnlyList<DesignConversionDpoDatasetSourceGroup> BuildManifestSources(
        IReadOnlyList<DesignConversionDpoDatasetRow> rows)
        => rows
            .GroupBy(row => row.SourceIdentityHash, StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.First();
                return new DesignConversionDpoDatasetSourceGroup(
                    SourceGroupId: first.SourceGroupId,
                    SourceIdentityHash: first.SourceIdentityHash,
                    SourceKind: first.SourceKind,
                    RecordCount: group.Count(),
                    RecordIdentityHashes: group
                        .Select(row => row.RecordIdentityHash)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(hash => hash, StringComparer.Ordinal)
                        .ToArray());
            })
            .OrderBy(group => group.SourceIdentityHash, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildEvidenceRefs(
        DesignConversionDpoDatasetCountGate countGate,
        DesignConversionDpoDatasetPreflight preflight)
    {
        var refs = new List<string>
        {
            "DPO_DATASET_ACCEPTED_RECORDS_ONLY",
            "DPO_DATASET_APPROVAL_PREFLIGHT_REQUIRED",
            "DPO_DATASET_SOURCE_MANIFEST_PREFLIGHT_REQUIRED",
            "DPO_DATASET_SOURCE_DISJOINT_SPLIT_BY_SOURCE_IDENTITY",
            "DPO_DATASET_MINIMUM_COUNTS_REPORTED_AS_GATE_METADATA",
            "DPO_DATASET_JSONL_ROWS_USE_HASHED_SOURCE_AND_RESULT_IDENTITIES",
            "DPO_DATASET_JSONL_ROWS_INCLUDE_GUIDE_REQUIRED_LABEL_FIELDS",
            "DPO_DATASET_NO_RAW_SOURCE_REQUEST_REVIEWER_LICENSE_OR_CANDIDATE_IDS",
            "DPO_DATASET_SOURCE_ENCODER_PAYLOAD_INCLUDED",
            "DPO_DATASET_ANTI_LEAKAGE_REDACTION_METADATA_INCLUDED",
            "DPO_DATASET_DOES_NOT_CLAIM_TRAINED_OR_PRODUCT_READY",
            "approval_preflight",
            "source_manifest",
            "source_disjoint",
            "negative_pair_quality",
            "training_candidate_only",
            "product_not_ready"
        };

        refs.Add(countGate.Passed
            ? "DPO_DATASET_COUNT_GATE_PASSED"
            : "DPO_DATASET_COUNT_GATE_NOT_PASSED");
        refs.Add(preflight.ApprovalPreflightPassed
            ? "DPO_DATASET_APPROVAL_PREFLIGHT_PASSED"
            : "DPO_DATASET_APPROVAL_PREFLIGHT_NOT_PASSED");
        refs.Add(preflight.SourceManifestPreflightPassed
            ? "DPO_DATASET_SOURCE_MANIFEST_PREFLIGHT_PASSED"
            : "DPO_DATASET_SOURCE_MANIFEST_PREFLIGHT_NOT_PASSED");
        refs.Add(preflight.SourceDisjointDeclared
            ? "DPO_DATASET_SOURCE_DISJOINT_PREFLIGHT_DECLARED"
            : "DPO_DATASET_SOURCE_DISJOINT_PREFLIGHT_NOT_DECLARED");
        refs.AddRange(preflight.EvidenceRefs);
        refs.AddRange(preflight.Reasons);

        return refs.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static bool IsSourceDisjoint(
        IReadOnlyList<DesignConversionDpoDatasetRow> trainRows,
        IReadOnlyList<DesignConversionDpoDatasetRow> heldOutRows)
    {
        var trainSources = trainRows
            .Select(row => row.SourceIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        return heldOutRows.All(row => !trainSources.Contains(row.SourceIdentityHash));
    }

    private static string BuildSourceIdentity(DesignConversionDpoTrainingRecord record)
    {
        var provenance = record.Provenance;
        return FirstNonEmptyIdentity(
            ("source_design_id", TryGetMetadata(provenance.Metadata, "source_design_id")),
            ("source_example_id", TryGetMetadata(provenance.Metadata, "source_example_id")),
            ("source_manifest_ref", provenance.SourceManifestRef),
            ("source_request_id", provenance.SourceRequestId),
            ("record_id", record.RecordId));
    }

    private static string FirstNonEmptyIdentity(params (string Scope, string? Value)[] identities)
    {
        foreach (var (scope, value) in identities)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return $"{scope}|{value.Trim()}";
        }

        return "unknown_source|unknown-source";
    }

    internal static string ComputeRecordIdentityHash(
        DesignConversionDpoTrainingRecord record)
        => HashIdentity($"record|{record.RecordId}|{record.UnitId}");

    internal static string ComputeSourceIdentityHash(
        DesignConversionDpoTrainingRecord record)
        => HashIdentity(BuildSourceIdentity(record));

    private static string? TryGetMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value) ? value : null;

    private static IReadOnlyList<string> SplitMetadataList(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

    private static IReadOnlyList<string> DefectCategories(DesignConversionImprovementOutput output)
        => output.Verdict.Defects
            .Select(defect => defect.Category)
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(category => category, StringComparer.Ordinal)
            .ToArray();

    private static string HashOutput(DesignConversionImprovementOutput output)
        => HashPayload(string.Join("|", output.Html, output.Css, output.Verdict.Score, output.Accepted));

    private static string ToResultRef(string resultHash)
        => $"dcm-result-{resultHash[..16]}";

    private static string HashPayload(string? payload)
        => HashIdentity(payload ?? string.Empty);

    private static string HashIdentity(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string AppendReason(string reason, string addition)
        => string.IsNullOrWhiteSpace(reason) ? addition : $"{reason};{addition}";

    private static string NormalizeSourceKind(string? sourceKind)
    {
        if (string.Equals(sourceKind, DesignConversionSourceKind.Figma.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceKind, "figma", StringComparison.OrdinalIgnoreCase))
            return "figma";
        if (string.Equals(sourceKind, DesignConversionSourceKind.Canva.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceKind, "canva", StringComparison.OrdinalIgnoreCase))
            return "canva";
        if (string.Equals(sourceKind, DesignConversionSourceKind.Other.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceKind, "other", StringComparison.OrdinalIgnoreCase))
            return "other";

        return "unknown";
    }

    private static bool LooksPlaceholder(string value)
        => value.Equals("unknown", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("placeholder", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("todo", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("n/a", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("todo", StringComparison.OrdinalIgnoreCase);

    private sealed record SourceGroup(
        string RawIdentity,
        string SourceIdentityHash,
        string SourceGroupId,
        string SourceKind,
        IReadOnlyList<DesignConversionDpoTrainingRecord> Records);

    private sealed record SplitResult(
        IReadOnlyList<SourceGroup> TrainGroups,
        IReadOnlyList<SourceGroup> HeldOutGroups);

    private sealed record CanonicalSourceProofDatasetFingerprint(
        string DatasetId,
        DesignConversionDpoDatasetManifest Manifest,
        IReadOnlyList<CanonicalSourceProofRowFingerprint> TrainRows,
        IReadOnlyList<CanonicalSourceProofRowFingerprint> HeldOutRows);

    private sealed record CanonicalSourceProofRowFingerprint(
        string Split,
        string RowId,
        string RecordIdentityHash,
        string UnitId,
        string DomainTag,
        string SourceKind,
        string SourceIdentityHash,
        string SourceGroupId,
        string PairFamily,
        double UsefulnessScore,
        string Prompt,
        string Chosen,
        string Rejected,
        DesignConversionDpoDatasetResultMetadata Result,
        DesignConversionDpoDatasetCandidateResult CandidateResult,
        DesignConversionDpoDatasetTeacherLabels TeacherLabels,
        string TeachingPoint,
        string CurriculumTier,
        DesignConversionDpoDatasetValidationOutcome ValidationOutcome,
        IReadOnlyList<CanonicalSourceProofMetadataFingerprint> Metadata);

    private sealed record CanonicalSourceProofMetadataFingerprint(
        string Key,
        string Value);

    private sealed record CanonicalSourceProof(
        string ProofSha256,
        string DatasetFingerprintSha256,
        IReadOnlyList<string> TrainRecordIdentityHashes);
}
