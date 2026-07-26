// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

public static class DesignConversionTrainingMeasurementGateStatus
{
    public const string Blocked = "blocked";
    public const string TrainingCandidateOnly = "training_candidate_only";
    public const string ReadyForTraining = "ready_for_training";
}

public static class DesignConversionTrainingMeasurementGatePolicy
{
    public const string TrainingMeasurementGateMarker = "training_measurement_gate";
    public const string SourceDisjointMarker = "source_disjoint";
    public const string HeldOutRequiredMarker = "heldout_required";
    public const string ProviderMissingBlockedMarker = "provider_missing_blocked";
    public const string QuarantineBlockedMarker = "quarantine_blocked";
    public const string NegativeQualityMissingMarker = "negative_quality_missing";
    public const string TrainingCandidateOnlyMarker = "training_candidate_only";
    public const string NotRealModelReadyMarker = "not_real_model_ready";
    public const string PromotionBlockedMarker = "promotion_blocked";
    public const string ProductNotReadyMarker = "product_not_ready";

    public static IReadOnlyList<string> GetRecordBlockReasons(DesignConversionDpoTrainingRecord? record)
    {
        var reasons = new List<string>();
        if (record is null)
            return ["training_record_missing"];

        if (record.Quarantine.IsQuarantined)
        {
            reasons.Add("quarantine_blocked");
            if (ContainsLeakageText(record.Quarantine.Reason) ||
                (record.Quarantine.EvidenceRefs ?? []).Any(ContainsLeakageText))
            {
                reasons.Add("leakage");
            }
        }

        if (!record.License.AllowsTraining)
            reasons.Add("approval_missing");

        if (record.License.ContainsUserPrivateData)
            reasons.Add("quarantine_blocked");

        if (HasPendingOrUnapprovedMetadata(record) ||
            HasFalseMetadata(record, "approved_for_training"))
        {
            reasons.Add("approval_missing");
        }

        var approvedLocalTrainingCandidateOnly = IsApprovedLocalTrainingCandidateOnly(record);
        var approvedRuntimeTrainingCandidateOnly = approvedLocalTrainingCandidateOnly &&
            HasTrueMetadata(record, "runtime_created_dpo_candidate");
        if (!approvedLocalTrainingCandidateOnly &&
            (HasTrueMetadata(record, "not_training_grade") ||
             HasFalseMetadata(record, "training_grade")))
        {
            reasons.Add("not_training_grade");
        }

        if (!approvedRuntimeTrainingCandidateOnly && HasTrueMetadata(record, "synthetic_negative"))
            reasons.Add("synthetic_negative");

        if (!approvedRuntimeTrainingCandidateOnly && HasTrueMetadata(record, "scaffold_negative"))
            reasons.Add("scaffold_negative");

        if (!approvedRuntimeTrainingCandidateOnly && HasTrueMetadata(record, "high_quality_negative_pair_not_proven"))
            reasons.Add("high_quality_negative_pair_not_proven");

        if (ContainsProviderMissingStatus(record))
            reasons.Add("provider_missing");

        if (string.IsNullOrWhiteSpace(record.UnitId))
            reasons.Add("unit_missing");

        if (string.IsNullOrWhiteSpace(record.Provenance.SourceManifestRef) ||
            NormalizeSourceKind(record.Provenance.SourceKind) is not ("figma" or "canva" or "other"))
        {
            reasons.Add("source_manifest_missing");
        }

        if (!HasNegativePairQuality(record))
            reasons.Add("negative_quality_missing");

        if (ContainsFeatureLeakage(record))
            reasons.Add("leakage");

        return reasons
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    public static bool IsRecordAllowedForTrainingCandidate(DesignConversionDpoTrainingRecord? record)
        => GetRecordBlockReasons(record).Count == 0;

    public static bool ContainsProviderMissingStatus(DesignConversionDpoTrainingRecord record)
    {
        var values = new[]
            {
                record.Quarantine.Reason
            }
            .Concat((record.Quarantine.EvidenceRefs ?? []).Select(reference => reference ?? string.Empty))
            .Concat((record.Provenance.Metadata ?? new Dictionary<string, string>())
                .SelectMany(pair => new[] { pair.Key, pair.Value }));

        return values.Any(IsProviderMissingText);
    }

    public static bool HasNegativePairQuality(DesignConversionDpoTrainingRecord record)
        => !string.IsNullOrWhiteSpace(record.NegativeQualityReason)
           && !LooksPlaceholder(record.NegativeQualityReason)
           && DesignConversionNegativeReasonTaxonomy.IsAllowed(record.NegativeReasonTaxonomy)
           && !DesignConversionNegativeReasonTaxonomy.IsQuarantineOnly(record.NegativeReasonTaxonomy);

    public static string ToRecordIdentityHash(DesignConversionDpoTrainingRecord record)
        => HashIdentity($"record|{record.RecordId}|{record.UnitId}");

    private static bool ContainsFeatureLeakage(DesignConversionDpoTrainingRecord record)
        => DesignConversionDpoLeakageGuard.ContainsForbiddenLeakage(
               record.InternalUnitStatePayload,
               record.SourceStructurePayload,
               record.TieBreakerPayload,
               record.NegativeQualityReason,
               record.NegativeReasonTaxonomy,
               record.Chosen.Html,
               record.Chosen.Css,
               record.Rejected.Html,
               record.Rejected.Css)
           || ContainsLeakageText(record.SourceStructurePayload)
           || ContainsLeakageText(record.InternalUnitStatePayload)
           || ContainsLeakageText(record.NegativeQualityReason)
           || ContainsLeakageText(record.NegativeReasonTaxonomy);

    private static bool HasPendingOrUnapprovedMetadata(DesignConversionDpoTrainingRecord record)
        => MetadataValueMatches(record, "approval_status", IsPendingOrUnapprovedText)
           || MetadataValueMatches(record, "review_status", IsPendingOrUnapprovedText)
           || MetadataValueMatches(record, "status", IsPendingOrUnapprovedText);

    private static bool HasTrueMetadata(DesignConversionDpoTrainingRecord record, string key)
        => MetadataValueMatches(record, key, IsTrueText);

    private static bool HasFalseMetadata(DesignConversionDpoTrainingRecord record, string key)
        => MetadataValueMatches(record, key, IsFalseText);

    private static bool IsApprovedLocalTrainingCandidateOnly(DesignConversionDpoTrainingRecord record)
    {
        if (!HasTrueMetadata(record, "training_candidate_only") ||
            !(HasTrueMetadata(record, "product_not_ready") ||
              HasFalseMetadata(record, "model_readiness_claimed") ||
              HasFalseMetadata(record, "trained_model_ready") ||
              HasFalseMetadata(record, "production_dpo")))
        {
            return false;
        }

        if (HasTrueMetadata(record, "runtime_created_dpo_candidate"))
        {
            return HasTrueMetadata(record, "approved_for_training") &&
                   MetadataValueMatches(record, "approval_status", IsApprovedText);
        }

        return !HasPendingOrUnapprovedMetadata(record) &&
               !HasFalseMetadata(record, "approved_for_training");
    }

    private static bool MetadataValueMatches(
        DesignConversionDpoTrainingRecord record,
        string key,
        Func<string?, bool> predicate)
        => ReadMetadataValues(record, key).Any(predicate);

    private static IEnumerable<string?> ReadMetadataValues(DesignConversionDpoTrainingRecord record, string key)
    {
        if (record.Provenance.Metadata is not null &&
            record.Provenance.Metadata.TryGetValue(key, out var provenanceValue))
        {
            yield return provenanceValue;
        }

        if (record.License.Metadata is not null &&
            record.License.Metadata.TryGetValue(key, out var licenseValue))
        {
            yield return licenseValue;
        }
    }

    private static bool IsTrueText(string? value)
        => string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase);

    private static bool IsFalseText(string? value)
        => string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase);

    private static bool IsApprovedText(string? value)
        => string.Equals(value, "approved", StringComparison.OrdinalIgnoreCase);

    private static bool IsPendingOrUnapprovedText(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && (value.Contains("pending", StringComparison.OrdinalIgnoreCase)
               || value.Contains("unapproved", StringComparison.OrdinalIgnoreCase)
               || value.Contains("not_approved", StringComparison.OrdinalIgnoreCase)
               || value.Contains("not-approved", StringComparison.OrdinalIgnoreCase)
               || value.Contains("not approved", StringComparison.OrdinalIgnoreCase));

    private static bool ContainsLeakageText(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && (value.Contains("hidden_deterministic_or_qa_leakage", StringComparison.OrdinalIgnoreCase)
               || value.Contains("leakage_shortcut", StringComparison.OrdinalIgnoreCase)
               || value.Contains("source_request_id_as_feature", StringComparison.OrdinalIgnoreCase)
               || value.Contains("deterministic-input-evidence", StringComparison.OrdinalIgnoreCase)
               || value.Contains("candidate_order", StringComparison.OrdinalIgnoreCase)
               || value.Contains("candidate_index", StringComparison.OrdinalIgnoreCase)
               || value.Contains("exact_prompt", StringComparison.OrdinalIgnoreCase));

    private static bool IsProviderMissingText(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && (value.Contains(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, StringComparison.OrdinalIgnoreCase)
               || value.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, StringComparison.OrdinalIgnoreCase)
               || (value.Contains("provider", StringComparison.OrdinalIgnoreCase)
                   && (value.Contains("missing", StringComparison.OrdinalIgnoreCase)
                       || value.Contains("required", StringComparison.OrdinalIgnoreCase)
                       || value.Contains("blocked", StringComparison.OrdinalIgnoreCase))));

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

    private static string HashIdentity(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

public sealed record DesignConversionTrainingCandidateDecision(
    string RecordId,
    string UnitId,
    bool Accepted,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionTrainingBatchArtifact(
    string ArtifactId,
    bool TrainingCandidateOnly,
    bool NotRealModelReady,
    IReadOnlyList<DesignConversionTrainingBatch> Batches,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionTrainingMeasurementGateResult(
    string Status,
    bool ReadyForTraining,
    bool TrainingCandidateOnly,
    bool NotRealModelReady,
    IReadOnlyList<DesignConversionTrainingCandidateDecision> CandidateDecisions,
    DesignConversionDpoDatasetManifest DatasetManifest,
    DesignConversionDpoDatasetCountGate DatasetCountGate,
    DesignConversionMeasurementGateResult? MeasurementGate,
    bool PassesGate,
    double? HeldOutQualityScore,
    double? RequiredHeldOutQualityScore,
    double? ScoreDelta,
    bool? SourceDisjoint,
    bool? AblationPassed,
    bool? BudgetPassed,
    string GateReason,
    DesignConversionTrainingBatchArtifact? TrainingBatchArtifact,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> EvidenceRefs);

public sealed class DesignConversionTrainingMeasurementGate
{
    private readonly DesignConversionDpoDatasetBuilder _datasetBuilder;
    private readonly DesignConversionTrainingPipeline _trainingPipeline;
    private readonly DesignConversionMeasurementGateEvaluator _measurementGate;
    private readonly DesignConversionDpoGenerator _generator;

    public DesignConversionTrainingMeasurementGate(
        DesignConversionDpoDatasetBuilder? datasetBuilder = null,
        DesignConversionTrainingPipeline? trainingPipeline = null,
        DesignConversionMeasurementGateEvaluator? measurementGate = null,
        DesignConversionDpoGenerator? generator = null)
    {
        _datasetBuilder = datasetBuilder ?? new DesignConversionDpoDatasetBuilder();
        _trainingPipeline = trainingPipeline ?? new DesignConversionTrainingPipeline(_datasetBuilder);
        _measurementGate = measurementGate ?? new DesignConversionMeasurementGateEvaluator();
        _generator = generator ?? new DesignConversionDpoGenerator();
    }

    public async Task<OperationResult<DesignConversionTrainingMeasurementGateResult>> EvaluateDpoStorageAsync(
        IDesignConversionDpoStorage storage,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
    {
        if (storage is null)
            return OperationResult<DesignConversionTrainingMeasurementGateResult>.Fail(
                "DesignConversion DPO storage is required.",
                "DESIGN_CONVERSION_DPO_STORAGE_REQUIRED");

        var examples = await storage.ListExamplesAsync(ct);
        if (!examples.IsSuccess)
            return examples.MapError<DesignConversionTrainingMeasurementGateResult>();

        var build = _generator.BuildTrainReadyRecords(examples.Value!);
        if (!build.IsSuccess)
            return build.MapError<DesignConversionTrainingMeasurementGateResult>();

        return EvaluateRecords(build.Value!.AllRecords, options);
    }

    public async Task<OperationResult<DesignConversionTrainingMeasurementGateResult>> EvaluateLocalArtifactDbAsync(
        IDesignConversionLocalArtifactDb artifactDb,
        DesignConversionLocalArtifactDbQuery? query,
        DesignConversionTrainingPipelineOptions options,
        CancellationToken ct = default)
    {
        if (artifactDb is null)
            return OperationResult<DesignConversionTrainingMeasurementGateResult>.Fail(
                "DesignConversion local artifact DB is required.",
                "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_REQUIRED");

        var records = await artifactDb.ListRecordsAsync(query, ct);
        if (!records.IsSuccess)
            return records.MapError<DesignConversionTrainingMeasurementGateResult>();

        var trainingRecords = records.Value!
            .SelectMany(record => record.DpoCandidates ?? [])
            .Select(candidate => candidate.TrainingRecord)
            .ToArray();

        return EvaluateRecords(trainingRecords, options);
    }

    public OperationResult<DesignConversionTrainingMeasurementGateResult> EvaluateRecords(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        DesignConversionTrainingPipelineOptions options)
    {
        if (records is null)
            return OperationResult<DesignConversionTrainingMeasurementGateResult>.Fail(
                "DesignConversion DPO records are required.",
                "DESIGN_CONVERSION_DPO_RECORDS_REQUIRED");

        if (options is null)
            return OperationResult<DesignConversionTrainingMeasurementGateResult>.Fail(
                "DesignConversion training options are required.",
                "DESIGN_CONVERSION_TRAINING_OPTIONS_REQUIRED");

        var dataset = _datasetBuilder.BuildDataset(records, options.DatasetOptions);
        if (!dataset.IsSuccess)
            return dataset.MapError<DesignConversionTrainingMeasurementGateResult>();

        var duplicateIdentities = records
            .Where(record => record is not null)
            .GroupBy(DesignConversionTrainingMeasurementGatePolicy.ToRecordIdentityHash, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);

        var preflight = dataset.Value!.Manifest.Preflight;
        var decisions = records
            .Select(record => BuildCandidateDecision(record, duplicateIdentities, preflight))
            .ToArray();
        var reasons = new List<string>();
        reasons.AddRange(decisions.SelectMany(decision => decision.Reasons));
        reasons.AddRange(MapDatasetReasons(dataset.Value.Manifest));

        var measurementResult = options.MeasurementGateRequest is null
            ? null
            : _measurementGate.Evaluate(options.MeasurementGateRequest);
        reasons.AddRange(MapMeasurementReasons(options.MeasurementGateRequest, measurementResult));
        var passesGate = measurementResult?.Passed == true;
        var heldOutQualityScore = measurementResult?.HeldOutQualityScore ??
                                  options.MeasurementGateRequest?.HeldOutMetrics?.HeldOutQualityScore;
        var requiredHeldOutQualityScore = measurementResult?.RequiredHeldOutQualityScore ??
                                          options.MeasurementGateRequest?.Thresholds?.RequiredHeldOutQualityScore;
        var scoreDelta = CalculateScoreDelta(heldOutQualityScore, requiredHeldOutQualityScore);
        var gateReason = BuildGateReason(options.MeasurementGateRequest, measurementResult);

        var trainIdentityHashes = dataset.Value.TrainRows
            .Select(row => row.RecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        var trainRecords = records
            .Where(record => DesignConversionTrainingMeasurementGatePolicy.IsRecordAllowedForTrainingCandidate(record))
            .Where(record => trainIdentityHashes.Contains(DesignConversionTrainingMeasurementGatePolicy.ToRecordIdentityHash(record)))
            .ToArray();
        var batches = _trainingPipeline.BuildBatches(trainRecords, options);
        if (!batches.IsSuccess)
            return batches.MapError<DesignConversionTrainingMeasurementGateResult>();

        var hasBatchArtifact = batches.Value!.Count > 0 && preflight.TrainingCandidateOnly && !preflight.ProductReady;
        var measurementPassed = measurementResult?.Passed == true;
        var readyForTraining =
            dataset.Value.Manifest.CountGate.Passed &&
            measurementPassed &&
            preflight.ProductReady &&
            !preflight.TrainingCandidateOnly &&
            decisions.All(decision => decision.Accepted);
        var trainingCandidateOnly = hasBatchArtifact && !readyForTraining;
        var notRealModelReady = !readyForTraining;

        if (notRealModelReady)
        {
            reasons.Add("not_real_model_ready");
            reasons.Add("promotion_blocked");
            reasons.Add("product_not_ready");
        }

        var status = readyForTraining
            ? DesignConversionTrainingMeasurementGateStatus.ReadyForTraining
            : trainingCandidateOnly
                ? DesignConversionTrainingMeasurementGateStatus.TrainingCandidateOnly
                : DesignConversionTrainingMeasurementGateStatus.Blocked;

        var evidenceRefs = BuildEvidenceRefs(
            status,
            reasons,
            dataset.Value.Manifest,
            measurementResult,
            hasBatchArtifact);
        var batchArtifact = hasBatchArtifact
            ? new DesignConversionTrainingBatchArtifact(
                ArtifactId: $"{NormalizeRunId(options.RunId)}:training_candidate_only",
                TrainingCandidateOnly: true,
                NotRealModelReady: true,
                Batches: batches.Value!,
                EvidenceRefs: evidenceRefs
                    .Append("approved_fixture_rows_only")
                    .Append("not_real_model_ready")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray())
            : null;

        return OperationResult<DesignConversionTrainingMeasurementGateResult>.Ok(
            new DesignConversionTrainingMeasurementGateResult(
                Status: status,
                ReadyForTraining: readyForTraining,
                TrainingCandidateOnly: trainingCandidateOnly,
                NotRealModelReady: notRealModelReady,
                CandidateDecisions: decisions,
                DatasetManifest: dataset.Value.Manifest,
                DatasetCountGate: dataset.Value.Manifest.CountGate,
                MeasurementGate: measurementResult,
                PassesGate: passesGate,
                HeldOutQualityScore: heldOutQualityScore,
                RequiredHeldOutQualityScore: requiredHeldOutQualityScore,
                ScoreDelta: scoreDelta,
                SourceDisjoint: measurementResult?.SourceDisjoint ??
                                options.MeasurementGateRequest?.IntegrityProof?.SourceDisjoint,
                AblationPassed: measurementResult?.AblationPassed ??
                                options.MeasurementGateRequest?.IntegrityProof?.AblationPassed,
                BudgetPassed: measurementResult?.BudgetPassed,
                GateReason: gateReason,
                TrainingBatchArtifact: batchArtifact,
                Reasons: reasons
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray(),
                EvidenceRefs: evidenceRefs));
    }

    private static DesignConversionTrainingCandidateDecision BuildCandidateDecision(
        DesignConversionDpoTrainingRecord record,
        IReadOnlySet<string> duplicateIdentities,
        DesignConversionDpoDatasetPreflight preflight)
    {
        var reasons = DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(record).ToList();
        if (!preflight.ApprovalPreflightPassed || !preflight.AllowsDatasetRowConstruction)
            reasons.Add("approval_missing");

        if (duplicateIdentities.Contains(DesignConversionTrainingMeasurementGatePolicy.ToRecordIdentityHash(record)))
            reasons.Add("replay_duplicate");

        var accepted = reasons.Count == 0;
        var evidenceRefs = new List<string>
        {
            "training_measurement_gate",
            accepted ? "training_candidate_record_accepted" : "training_candidate_record_blocked"
        };
        evidenceRefs.AddRange(reasons.Select(ToEvidenceRef));

        return new DesignConversionTrainingCandidateDecision(
            RecordId: record.RecordId,
            UnitId: record.UnitId,
            Accepted: accepted,
            Reasons: reasons
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            EvidenceRefs: evidenceRefs
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    private static IReadOnlyList<string> MapDatasetReasons(DesignConversionDpoDatasetManifest manifest)
    {
        var reasons = new List<string>();
        if (manifest.CountGate.ActualHeldOutRecords <= 0)
            reasons.Add("heldout_missing");
        if (!manifest.CountGate.SourceDisjoint)
            reasons.Add("source_not_disjoint");
        if (!manifest.Preflight.ApprovalPreflightPassed || !manifest.Preflight.AllowsDatasetRowConstruction)
            reasons.Add("approval_missing");

        foreach (var reason in manifest.CountGate.Reasons ?? [])
        {
            if (reason.StartsWith("HELD_OUT_RECORD_COUNT_BELOW_TARGET", StringComparison.Ordinal))
                reasons.Add("heldout_required");
            if (reason.Contains("SOURCE_DISJOINT", StringComparison.OrdinalIgnoreCase))
                reasons.Add("source_not_disjoint");
        }

        return reasons;
    }

    private static IReadOnlyList<string> MapMeasurementReasons(
        DesignConversionMeasurementGateRequest? request,
        DesignConversionMeasurementGateResult? result)
    {
        var reasons = new List<string>();
        if (request is null)
        {
            reasons.Add("heldout_required");
            return reasons;
        }

        if (request.HeldOutMetrics?.HeldOutRecordCount is null or <= 0)
            reasons.Add("heldout_missing");
        if (request.IntegrityProof?.SourceDisjoint != true)
            reasons.Add("source_not_disjoint");
        if (result?.Passed != true)
            reasons.Add("heldout_required");

        foreach (var reason in result?.Reasons ?? [])
        {
            if (reason.Contains("HELD_OUT", StringComparison.OrdinalIgnoreCase))
                reasons.Add("heldout_required");
            if (reason.Contains("SOURCE_DISJOINT", StringComparison.OrdinalIgnoreCase))
                reasons.Add("source_not_disjoint");
            if (reason.Contains("LEAKAGE", StringComparison.OrdinalIgnoreCase) ||
                reason.Contains("HIDDEN_DETERMINISM", StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add("leakage");
            }
        }

        return reasons;
    }

    private static IReadOnlyList<string> BuildEvidenceRefs(
        string status,
        IReadOnlyList<string> reasons,
        DesignConversionDpoDatasetManifest manifest,
        DesignConversionMeasurementGateResult? measurementResult,
        bool hasBatchArtifact)
    {
        var refs = new List<string>
        {
            "training_measurement_gate",
            "source_disjoint",
            "heldout_required",
            "not_real_model_ready",
            "promotion_blocked",
            "product_not_ready",
            $"training_measurement_gate_status:{status}",
            manifest.CountGate.SourceDisjoint ? "source_disjoint:true" : "source_disjoint:false",
            manifest.CountGate.ActualHeldOutRecords > 0 ? "heldout_present" : "heldout_missing",
            measurementResult?.Passed == true ? "passesQualityGate:true" : "passesQualityGate:false"
        };

        if (hasBatchArtifact)
            refs.Add("training_candidate_only");
        if (status != DesignConversionTrainingMeasurementGateStatus.ReadyForTraining)
            refs.Add("ready_for_training:false");
        if (measurementResult is null)
            refs.Add("heldout_measurement_gate:not_run");
        else
            refs.AddRange(measurementResult.EvidenceRefs);
        refs.AddRange(manifest.EvidenceRefs ?? []);
        refs.AddRange(reasons.Select(ToEvidenceRef));

        return refs
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string ToEvidenceRef(string reason)
        => reason switch
        {
            "provider_missing" => "provider_missing_blocked",
            "quarantine_blocked" => "quarantine_blocked",
            "negative_quality_missing" => "negative_quality_missing",
            "source_not_disjoint" => "source_not_disjoint",
            "heldout_missing" => "heldout_missing",
            "heldout_required" => "heldout_required",
            "training_candidate_only" => "training_candidate_only",
            "not_real_model_ready" => "not_real_model_ready",
            "promotion_blocked" => "promotion_blocked",
            "product_not_ready" => "product_not_ready",
            _ => reason
        };

    private static string BuildGateReason(
        DesignConversionMeasurementGateRequest? request,
        DesignConversionMeasurementGateResult? result)
    {
        if (request is null || result is null)
            return "heldout_measurement_gate:not_run";

        if (result.Reasons is null || result.Reasons.Count == 0)
            return result.Passed ? "ALL_MEASUREMENT_GATES_PASSED" : "measurement_gate_failed_without_reason";

        return string.Join("|", result.Reasons);
    }

    private static double? CalculateScoreDelta(double? heldOutQualityScore, double? requiredHeldOutQualityScore)
    {
        if (heldOutQualityScore is null || requiredHeldOutQualityScore is null)
            return null;

        if (!IsFinite(heldOutQualityScore.Value) || !IsFinite(requiredHeldOutQualityScore.Value))
            return null;

        return Math.Round(heldOutQualityScore.Value - requiredHeldOutQualityScore.Value, 6);
    }

    private static bool IsFinite(double value)
        => !double.IsNaN(value) && !double.IsInfinity(value);

    private static string NormalizeRunId(string? runId)
    {
        if (string.IsNullOrWhiteSpace(runId))
            return "design-conversion-training";

        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var safe = new string(runId.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "design-conversion-training" : safe;
    }
}
