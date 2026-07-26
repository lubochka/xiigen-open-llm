// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

public sealed record DesignConversionDpoDatasetJsonlRow(
    string DatasetId,
    string Split,
    string RowId,
    string DomainTag,
    string UnitId,
    string SourceKind,
    string SourceIdentityHash,
    string SourceGroupId,
    string PairFamily,
    double UsefulnessScore,
    string Prompt,
    DesignConversionDpoDatasetSafeCandidateResult Chosen,
    DesignConversionDpoDatasetSafeCandidateResult Rejected,
    DesignConversionDpoDatasetResultMetadata Result,
    [property: JsonPropertyName("candidate_result")] DesignConversionDpoDatasetCandidateResult CandidateResult,
    [property: JsonPropertyName("teacher_labels")] DesignConversionDpoDatasetTeacherLabels TeacherLabels,
    [property: JsonPropertyName("teaching_point")] string TeachingPoint,
    [property: JsonPropertyName("curriculum_tier")] string CurriculumTier,
    [property: JsonPropertyName("validation_outcome")] DesignConversionDpoDatasetValidationOutcome ValidationOutcome,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionDpoDataset(
    string DatasetId,
    DesignConversionDpoDatasetManifest Manifest,
    IReadOnlyList<DesignConversionDpoDatasetRow> TrainRows,
    IReadOnlyList<DesignConversionDpoDatasetRow> HeldOutRows)
{
    private static readonly JsonSerializerOptions JsonlOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    public IReadOnlyList<DesignConversionDpoDatasetRow> Rows =>
        TrainRows.Concat(HeldOutRows).ToArray();

    public IReadOnlyList<DesignConversionDpoDatasetJsonlRow> ToJsonlRows()
        => Rows.Select(row => new DesignConversionDpoDatasetJsonlRow(
            DatasetId: DatasetId,
            Split: row.Split,
            RowId: row.RowId,
            DomainTag: row.DomainTag,
            UnitId: row.UnitId,
            SourceKind: row.SourceKind,
            SourceIdentityHash: row.SourceIdentityHash,
            SourceGroupId: row.SourceGroupId,
            PairFamily: row.PairFamily,
            UsefulnessScore: row.UsefulnessScore,
            Prompt: row.Prompt,
            Chosen: row.CandidateResult.Chosen,
            Rejected: row.CandidateResult.Rejected,
            Result: row.Result,
            CandidateResult: row.CandidateResult,
            TeacherLabels: row.TeacherLabels,
            TeachingPoint: row.TeachingPoint,
            CurriculumTier: row.CurriculumTier,
            ValidationOutcome: row.ValidationOutcome,
            Metadata: row.Metadata)).ToArray();

    public IReadOnlyList<string> ToJsonlLines()
        => ToJsonlRows()
            .Select(row => JsonSerializer.Serialize(row, JsonlOptions))
            .ToArray();
}

public sealed class DesignConversionDpoDatasetBuilder
{
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

        return OperationResult<DesignConversionDpoDataset>.Ok(new DesignConversionDpoDataset(
            DatasetId: effectiveOptions.DatasetId,
            Manifest: manifest,
            TrainRows: trainRows,
            HeldOutRows: heldOutRows));
    }

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
        var recordHash = HashIdentity($"record|{record.RecordId}|{record.UnitId}");
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
            ["local_paths_included"] = bool.FalseString
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
            Chosen: BuildResponsePayload(record.Chosen),
            Rejected: BuildResponsePayload(record.Rejected),
            Result: result,
            CandidateResult: candidateResult,
            TeacherLabels: teacherLabels,
            TeachingPoint: BuildTeachingPoint(pairFamily),
            CurriculumTier: BuildCurriculumTier(record.UsefulnessScore),
            ValidationOutcome: ToValidationOutcome(record, trainingGrade),
            Metadata: metadata);
    }

    private static string BuildPromptPayload(
        DesignConversionDpoTrainingRecord record,
        SourceGroup sourceGroup,
        DesignConversionDpoDatasetOptions options)
        => JsonSerializer.Serialize(new
        {
            domain_tag = options.DomainTag,
            unit_id = record.UnitId,
            target = new
            {
                unit_id = record.UnitId,
                machine_part = DesignConversionDefaultSourceEncoder.ToMachinePart(record.UnitId)
            },
            source = new
            {
                kind = NormalizeSourceKind(record.Provenance.SourceKind),
                identity_hash = sourceGroup.SourceIdentityHash,
                structure_hash = HashPayload(record.SourceStructurePayload),
                structure_payload = record.SourceStructurePayload
            },
            internal_unit_state_hash = HashPayload(record.InternalUnitStatePayload),
            internal_vector_dimensions = record.InternalVectorPayload.Count,
            result_features = new
            {
                @namespace = "result_features",
                source_authority = false,
                role = "outcome_label_aggregate_not_source_authority",
                chosen_result_ref = ToResultRef(HashOutput(record.Chosen)),
                rejected_result_ref = ToResultRef(HashOutput(record.Rejected)),
                visual_score_margin = Math.Round(record.Chosen.Verdict.Score - record.Rejected.Verdict.Score, 6),
                defect_categories = DefectCategories(record.Chosen)
                    .Concat(DefectCategories(record.Rejected))
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                negative_quality_reason_hash = HashPayload(record.NegativeQualityReason),
                pair_family = string.IsNullOrWhiteSpace(record.NegativeReasonTaxonomy)
                    ? "unknown"
                    : record.NegativeReasonTaxonomy
            },
            source_encoder = new
            {
                name = TryGetMetadata(record.Provenance.Metadata, "source_encoder")
                    ?? DesignConversionDefaultSourceEncoder.EncoderName,
                schema = TryGetMetadata(record.Provenance.Metadata, "source_encoder_schema")
                    ?? DesignConversionDefaultSourceEncoder.PayloadSchema,
                redaction_codes = SplitMetadataList(TryGetMetadata(record.Provenance.Metadata, "source_encoder_redaction_codes"))
            },
            pair_family = string.IsNullOrWhiteSpace(record.NegativeReasonTaxonomy)
                ? "unknown"
                : record.NegativeReasonTaxonomy
        }, CompactJsonOptions);

    private static string BuildResponsePayload(DesignConversionImprovementOutput output)
        => JsonSerializer.Serialize(new
        {
            result_hash = HashOutput(output),
            visual_score = output.Verdict.Score,
            accepted = output.Accepted,
            defect_categories = DefectCategories(output)
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
        => DesignConversionTrainingMeasurementGatePolicy.IsRecordAllowedForTrainingCandidate(record)
           && options.Preflight is { ProductReady: true, TrainingCandidateOnly: false }
           && string.Equals(
               TryGetMetadata(record.Provenance.Metadata, "training_grade"),
               bool.TrueString,
               StringComparison.OrdinalIgnoreCase);

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
            RecordIdentityHash: HashIdentity($"record|{record.RecordId}|{record.UnitId}"),
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
}
