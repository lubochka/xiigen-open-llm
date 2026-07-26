// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using AgentInfrastructure;

public sealed record DesignConversionDpoUsefulnessApiRequest(
    string? UnitId = null,
    bool IncludeBlocked = true,
    double? MinimumUsefulnessScore = null,
    double? MaximumUsefulnessScore = null,
    int? Limit = null);

public sealed record DesignConversionDpoUsefulnessMetrics(
    double Score,
    string AcceptanceReason,
    double QualityGain,
    string NegativePairUsefulness,
    string SourceCoverage,
    string SourceDisjointStatus,
    string HeldOutEvaluationStatus,
    string PromotionEligibility,
    IReadOnlyList<string> PolicyCheckpointEvidenceRefs,
    bool ProductReady,
    string ProductReadyBoundary);

public sealed record DesignConversionDpoUsefulnessApiRow(
    string DisplayId,
    string UnitId,
    string SourceKind,
    string SourceRef,
    string ChosenSummary,
    string RejectedSummary,
    double UsefulnessScore,
    string UsefulnessStatus,
    string ReviewStatus,
    bool ReviewRequired,
    string ApprovalStatus,
    bool TrainReady,
    bool TrainingCandidateOnly,
    bool Quarantined,
    bool ProviderMissing,
    bool RejectedNegativeQuality,
    bool HeldOut,
    bool AllowsTraining,
    bool ContainsUserPrivateData,
    bool ProductNotReady,
    bool ProductionDpo,
    bool ModelReadinessClaimed,
    bool HeldOutNotProven,
    bool SourceDisjointNotProven,
    string RetrainDecisionStatus,
    IReadOnlyList<string> RetrainDecisionEvidenceRefs,
    DesignConversionDpoUsefulnessMetrics UsefulnessMetrics,
    string ReadinessBoundary,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyList<string> UsefulnessFactors,
    IReadOnlyList<string> Defects,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionDpoUsefulnessApiResult(
    string ApiShape,
    IReadOnlyList<DesignConversionDpoUsefulnessApiRow> Rows,
    int RowCount,
    int TrainReadyCount,
    int QuarantinedCount,
    int ProviderMissingCount,
    int RejectedNegativeQualityCount,
    int HeldOutCount,
    int BlockedCount,
    double? AverageUsefulnessScore,
    double? MinimumUsefulnessScore,
    double? MaximumUsefulnessScore,
    bool ProductReady,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionLocalArtifactReadbackApiRequest(
    string? JobId = null,
    string? RequestId = null,
    string? SourceKind = null,
    int? Limit = null);

public sealed record DesignConversionLocalArtifactReadbackRenderRef(
    string ViewportId,
    string ArtifactId,
    string Kind,
    string UriRef,
    string Sha256,
    long ByteLength,
    int Width,
    int Height,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionLocalArtifactReadbackApiRow(
    string RecordId,
    string JobId,
    string RequestId,
    string SourceKind,
    string Tier,
    DateTimeOffset CreatedAtUtc,
    string ConversionResultId,
    string HtmlRef,
    string HtmlSha256,
    string CssRef,
    string CssSha256,
    IReadOnlyDictionary<string, string> SourceRefs,
    IReadOnlyList<DesignConversionLocalArtifactReadbackRenderRef> RenderArtifactRefs,
    string VisualJudgeStatus,
    string VisualJudgeProviderBoundary,
    bool ExternalVisualAiCallExecuted,
    decimal? EstimatedCostUsd,
    string TierDecision,
    int DpoCandidateCount,
    string DpoReviewStatus,
    bool DpoReviewRequired,
    string DpoApprovalStatus,
    bool TrainingCandidateOnly,
    bool ProductNotReady,
    bool ProductionDpo,
    bool ModelReadinessClaimed,
    bool HeldOutNotProven,
    bool SourceDisjointNotProven,
    string ReadinessBoundary,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionLocalArtifactReadbackApiResult(
    string ApiShape,
    IReadOnlyList<DesignConversionLocalArtifactReadbackApiRow> Rows,
    int RecordCount,
    int RuntimeCreatedRecordCount,
    int DpoCandidateCount,
    bool ProductReady,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionDpoUsefulnessApi
{
    public const string ApiShape = "design_conversion_dpo_usefulness_display_api_v1";
    public const string LocalArtifactReadbackApiShape = "design_conversion_local_artifact_readback_api_v1";
    public const string RuntimeSurface = "in_process_design_conversion_dpo_usefulness_api";
    public const string LocalArtifactReadbackRuntimeSurface = "in_process_design_conversion_local_artifact_readback_api";
    public const string ProductNotReadyBoundary = "product_not_ready";
    public const string RuntimeCandidateReviewPendingStatus = "runtime-candidate-review-pending";
    public const string RuntimeCandidateReviewApprovedStatus = "runtime-candidate-review-approved";
    public const string ApprovalPendingStatus = "pending";
    public const string ApprovalApprovedStatus = "approved";

    private readonly IDesignConversionLocalArtifactDb _artifactDb;

    public DesignConversionDpoUsefulnessApi(IDesignConversionLocalArtifactDb artifactDb)
    {
        _artifactDb = artifactDb ?? throw new ArgumentNullException(nameof(artifactDb));
    }

    public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
        string localArtifactRecordId,
        string candidateOrTrainingRecordId,
        string reviewerRef,
        string reason,
        DateTimeOffset? approvedAtUtc = null,
        CancellationToken ct = default)
        => _artifactDb.ApproveRuntimeCreatedDpoCandidateAsync(
            localArtifactRecordId,
            candidateOrTrainingRecordId,
            reviewerRef,
            reason,
            approvedAtUtc,
            ct);

    public Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
        DesignConversionLocalArtifactDbQuery? query = null,
        DateTimeOffset? exportedAtUtc = null,
        CancellationToken ct = default)
        => _artifactDb.ExportApprovedRuntimeDpoCandidatesAsync(
            query,
            exportedAtUtc,
            ct);

    public Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
        DesignConversionApprovedDpoHandoffArtifact handoff,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
        => _artifactDb.CreateTrainingIntakeAsync(
            handoff,
            requestedByRef,
            reason,
            requestedAtUtc,
            ct,
            admissionBoundaryMetadata);

    public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
        CancellationToken ct = default)
        => _artifactDb.ListTrainingIntakeRequestsAsync(ct);

    public Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
        string intakeId,
        CancellationToken ct = default)
        => _artifactDb.RunTrainingIntakePreflightAsync(intakeId, ct);

    public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
        string intakeId,
        DesignConversionTrainingPipelineOptions? options = null,
        CancellationToken ct = default,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
        => _artifactDb.RunTrainingIntakeAsync(intakeId, options, ct, admissionBoundaryMetadata);

    public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
        string intakeId,
        string promotionOutputDirectory,
        DesignConversionTrainingPipelineOptions? options = null,
        CancellationToken ct = default)
        => _artifactDb.RunTrainingIntakeAndPromoteLocalAsync(intakeId, promotionOutputDirectory, options, ct);

    public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
        string promotedLocalModelRef,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default)
        => _artifactDb.RollbackTrainingIntakeLocalPromotionAsync(
            promotedLocalModelRef,
            requestedByRef,
            reason,
            requestedAtUtc,
            ct);

    public async Task<OperationResult<DesignConversionDpoUsefulnessApiResult>> QueryAsync(
        DesignConversionDpoUsefulnessApiRequest? request = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionDpoUsefulnessApiResult>.Fail(
                "DesignConversion DPO usefulness API query cancelled.",
                "CANCELLED");

        var display = await _artifactDb.QueryDisplayRowsAsync(ToDisplayQuery(request), ct);
        if (!display.IsSuccess)
            return display.MapError<DesignConversionDpoUsefulnessApiResult>();

        var rows = display.Value!.Rows.Select(ToApiRow).ToArray();
        var runtimeReviewPendingCount = rows.Count(row =>
            string.Equals(row.ReviewStatus, RuntimeCandidateReviewPendingStatus, StringComparison.Ordinal));
        var runtimeReviewApprovedCount = rows.Count(row =>
            row.ReviewRequired
            && string.Equals(row.ApprovalStatus, ApprovalApprovedStatus, StringComparison.Ordinal));
        var rowCount = rows.Length;
        var averageUsefulnessScore = rowCount == 0
            ? (double?)null
            : Math.Round(rows.Average(row => row.UsefulnessScore), 6);
        var minimumUsefulnessScore = rowCount == 0
            ? (double?)null
            : rows.Min(row => row.UsefulnessScore);
        var maximumUsefulnessScore = rowCount == 0
            ? (double?)null
            : rows.Max(row => row.UsefulnessScore);
        var result = new DesignConversionDpoUsefulnessApiResult(
            ApiShape: ApiShape,
            Rows: rows,
            RowCount: rowCount,
            TrainReadyCount: rows.Count(row => row.TrainReady),
            QuarantinedCount: rows.Count(row => row.Quarantined),
            ProviderMissingCount: rows.Count(row => row.ProviderMissing),
            RejectedNegativeQualityCount: rows.Count(row => row.RejectedNegativeQuality),
            HeldOutCount: rows.Count(row => row.HeldOut),
            BlockedCount: rows.Count(row => row.ReviewStatus != "train-ready" && row.ReviewStatus != "heldout"),
            AverageUsefulnessScore: averageUsefulnessScore,
            MinimumUsefulnessScore: minimumUsefulnessScore,
            MaximumUsefulnessScore: maximumUsefulnessScore,
            ProductReady: false,
            Warnings: display.Value.Warnings
                .Concat(
                [
                    RuntimeSurface,
                    "raw_html_css_tree_private_provider_payloads_not_emitted",
                    "usefulness_score_is_review_priority_not_model_readiness",
                    ProductNotReadyBoundary
                ])
                .Concat(runtimeReviewPendingCount > 0
                    ?
                    [
                        "runtime_created_dpo_candidate_review_pending",
                        "runtime_created_dpo_candidate_approval_required",
                        "training_candidate_only_not_approved",
                        "approved_for_training=false",
                        "training_grade=false",
                        "not_training_grade=true",
                        "production_dpo=false",
                        "model_readiness_claimed=false"
                    ]
                    : [])
                .Concat(runtimeReviewApprovedCount > 0
                    ?
                    [
                        "runtime_created_dpo_candidate_approved_for_training",
                        "approval_status=approved",
                        "approved_for_training=true",
                        "training_grade=false",
                        "not_training_grade=true",
                        "production_dpo=false",
                        "model_readiness_claimed=false"
                    ]
                    : [])
                .Append("retrain_decision_surface_refs_only")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["api_shape"] = ApiShape,
                ["runtime_surface"] = RuntimeSurface,
                ["backing_store"] = nameof(IDesignConversionLocalArtifactDb),
                ["backing_display"] = nameof(DesignConversionDpoDisplayBuilder),
                ["row_count"] = rowCount.ToString(CultureInfo.InvariantCulture),
                ["average_usefulness_score"] = FormatUsefulnessScore(averageUsefulnessScore),
                ["minimum_usefulness_score"] = FormatUsefulnessScore(minimumUsefulnessScore),
                ["maximum_usefulness_score"] = FormatUsefulnessScore(maximumUsefulnessScore),
                ["raw_html_included"] = bool.FalseString,
                ["raw_css_included"] = bool.FalseString,
                ["raw_design_tree_included"] = bool.FalseString,
                ["raw_user_private_data_included"] = bool.FalseString,
                ["provider_payload_included"] = bool.FalseString,
                ["product_ready"] = bool.FalseString,
                ["trained_model_ready"] = bool.FalseString,
                ["runtime_review_pending_count"] = runtimeReviewPendingCount.ToString(),
                ["runtime_review_approved_count"] = runtimeReviewApprovedCount.ToString(),
                ["approval_required_count"] = rows.Count(row => row.ReviewRequired).ToString(),
                ["approval_status"] = runtimeReviewPendingCount > 0
                    ? ApprovalPendingStatus
                    : runtimeReviewApprovedCount > 0 ? ApprovalApprovedStatus : "not-required",
                ["approved_for_training_count"] = runtimeReviewApprovedCount.ToString(),
                ["training_grade"] = bool.FalseString,
                ["not_training_grade"] = bool.TrueString,
                ["training_grade_boundary"] = "api_runtime_surface_not_training_grade",
                ["model_readiness_claimed"] = bool.FalseString,
                ["retrain_decision_surface"] = "row_status_plus_refs_only",
                ["retrain_decision_product_ready"] = bool.FalseString
            });

        return OperationResult<DesignConversionDpoUsefulnessApiResult>.Ok(result);
    }

    public async Task<OperationResult<DesignConversionLocalArtifactReadbackApiResult>> QueryLocalArtifactReadbackAsync(
        DesignConversionLocalArtifactReadbackApiRequest? request = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionLocalArtifactReadbackApiResult>.Fail(
                "DesignConversion local artifact readback API query cancelled.",
                "CANCELLED");

        var effective = request ?? new DesignConversionLocalArtifactReadbackApiRequest();
        var records = await _artifactDb.ListRecordsAsync(ToLocalArtifactDbQuery(effective), ct);
        if (!records.IsSuccess)
            return records.MapError<DesignConversionLocalArtifactReadbackApiResult>();

        var rows = records.Value!
            .Select(ToLocalArtifactReadbackRow)
            .OrderByDescending(row => row.CreatedAtUtc)
            .ThenBy(row => row.RecordId, StringComparer.Ordinal)
            .Take(effective.Limit is > 0 ? effective.Limit.Value : int.MaxValue)
            .ToArray();
        var dpoRowsBoundary = rows.Sum(row => row.DpoCandidateCount) > 0
            ? "runtime_created_dpo_candidates_available"
            : "dpo_rows_not_created_without_dpo_candidates";
        var runtimeReviewPendingCount = rows.Count(row =>
            string.Equals(row.DpoReviewStatus, RuntimeCandidateReviewPendingStatus, StringComparison.Ordinal));
        var runtimeReviewApprovedCount = rows.Count(row =>
            string.Equals(row.DpoApprovalStatus, ApprovalApprovedStatus, StringComparison.Ordinal));
        var warnings = rows
            .SelectMany(row => row.Warnings)
            .Concat(
            [
                LocalArtifactReadbackRuntimeSurface,
                "local_artifact_db=true",
                "runtime_created_artifact_record_review_only",
                dpoRowsBoundary,
                "raw_html_css_tree_private_provider_payloads_not_emitted",
                "public_api_verified=false",
                "production_db_ready=false",
                "trained_model_ready=false",
                ProductNotReadyBoundary
            ])
            .Concat(runtimeReviewPendingCount > 0
                ?
                [
                    "runtime_created_dpo_candidate_review_pending",
                    "runtime_created_dpo_candidate_approval_required",
                    "training_candidate_only_not_approved",
                    "approved_for_training=false",
                    "training_grade=false",
                    "not_training_grade=true",
                    "production_dpo=false",
                    "model_readiness_claimed=false"
                    ]
                    : [])
            .Concat(runtimeReviewApprovedCount > 0
                ?
                [
                    "runtime_created_dpo_candidate_approved_for_training",
                    "approval_status=approved",
                    "approved_for_training=true",
                    "training_grade=false",
                    "not_training_grade=true",
                    "production_dpo=false",
                    "model_readiness_claimed=false"
                ]
                : [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var result = new DesignConversionLocalArtifactReadbackApiResult(
            ApiShape: LocalArtifactReadbackApiShape,
            Rows: rows,
            RecordCount: rows.Length,
            RuntimeCreatedRecordCount: rows.Count(row =>
                row.BoundaryMetadata.TryGetValue("artifact_db_record_source", out var source)
                && string.Equals(source, nameof(DesignConversionThinClientJobRuntime), StringComparison.Ordinal)),
            DpoCandidateCount: rows.Sum(row => row.DpoCandidateCount),
            ProductReady: false,
            Warnings: warnings,
            BoundaryMetadata: BuildReadbackResultBoundaryMetadata(rows));

        return OperationResult<DesignConversionLocalArtifactReadbackApiResult>.Ok(result);
    }

    private static DesignConversionDpoDisplayQuery ToDisplayQuery(
        DesignConversionDpoUsefulnessApiRequest? request)
    {
        var effective = request ?? new DesignConversionDpoUsefulnessApiRequest();
        return new DesignConversionDpoDisplayQuery(
            UnitId: effective.UnitId,
            IncludeBlocked: effective.IncludeBlocked,
            MinimumUsefulnessScore: effective.MinimumUsefulnessScore,
            MaximumUsefulnessScore: effective.MaximumUsefulnessScore,
            Limit: effective.Limit);
    }

    private static DesignConversionLocalArtifactDbQuery ToLocalArtifactDbQuery(
        DesignConversionLocalArtifactReadbackApiRequest request)
        => new(
            JobId: request.JobId,
            RequestId: request.RequestId,
            SourceKind: request.SourceKind);

    private static DesignConversionLocalArtifactReadbackApiRow ToLocalArtifactReadbackRow(
        DesignConversionLocalArtifactDbRecord record)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var boundaryMetadata = BuildReadbackRowBoundaryMetadata(record, redactions);
        var warnings = BuildReadbackWarnings(record);
        var reviewStatus = ReadMetadata(boundaryMetadata, "review_status");
        var approvalStatus = ReadMetadata(boundaryMetadata, "approval_status");

        return new DesignConversionLocalArtifactReadbackApiRow(
            RecordId: SanitizeReadbackScalar(record.RecordId, "local_artifact_record_id", redactions, "artifact-record"),
            JobId: SanitizeReadbackScalar(record.JobId, "local_artifact_job_id", redactions, "job"),
            RequestId: SanitizeReadbackScalar(record.RequestId, "local_artifact_request_id", redactions, "request"),
            SourceKind: SanitizeReadbackScalar(record.SourceKind, "local_artifact_source_kind", redactions, "unknown"),
            Tier: SanitizeReadbackScalar(record.Tier, "local_artifact_tier", redactions, "unspecified"),
            CreatedAtUtc: record.CreatedAtUtc,
            ConversionResultId: SanitizeReadbackScalar(record.ConversionResult.ResultId, "conversion_result_id", redactions, "conversion-result"),
            HtmlRef: SanitizeReadbackScalar(record.ConversionResult.HtmlRef, "conversion_html_ref", redactions, string.Empty),
            HtmlSha256: SanitizeReadbackScalar(record.ConversionResult.HtmlSha256, "conversion_html_sha256", redactions, string.Empty),
            CssRef: SanitizeReadbackScalar(record.ConversionResult.CssRef, "conversion_css_ref", redactions, string.Empty),
            CssSha256: SanitizeReadbackScalar(record.ConversionResult.CssSha256, "conversion_css_sha256", redactions, string.Empty),
            SourceRefs: DesignConversionDpoLeakageGuard.SanitizeMetadata(
                record.SanitizedSourceRefs,
                "local_artifact_readback_source_refs",
                redactions),
            RenderArtifactRefs: record.RenderArtifactRefs
                .Select(render => ToReadbackRenderRef(render, redactions))
                .ToArray(),
            VisualJudgeStatus: SanitizeReadbackScalar(record.VisualJudgeStatus.Status, "visual_judge_status", redactions, "unknown"),
            VisualJudgeProviderBoundary: SanitizeReadbackScalar(record.VisualJudgeStatus.ProviderBoundary, "visual_judge_provider_boundary", redactions, "unknown"),
            ExternalVisualAiCallExecuted: record.VisualJudgeStatus.ExternalCallExecuted,
            EstimatedCostUsd: record.VisualJudgeStatus.EstimatedCostUsd,
            TierDecision: SanitizeReadbackScalar(record.VisualJudgeStatus.TierDecision, "visual_judge_tier_decision", redactions, "unknown"),
            DpoCandidateCount: record.DpoCandidates.Count,
            DpoReviewStatus: string.IsNullOrWhiteSpace(reviewStatus) ? "not-required" : reviewStatus,
            DpoReviewRequired: IsTruthy(boundaryMetadata, "review_required"),
            DpoApprovalStatus: string.IsNullOrWhiteSpace(approvalStatus) ? "not-required" : approvalStatus,
            TrainingCandidateOnly: IsTruthy(boundaryMetadata, "training_candidate_only"),
            ProductNotReady: true,
            ProductionDpo: IsTruthy(boundaryMetadata, "production_dpo"),
            ModelReadinessClaimed: IsTruthy(boundaryMetadata, "model_readiness_claimed"),
            HeldOutNotProven: IsTruthy(boundaryMetadata, "held_out_not_proven"),
            SourceDisjointNotProven: IsTruthy(boundaryMetadata, "source_disjoint_not_proven"),
            ReadinessBoundary: ProductNotReadyBoundary,
            EvidenceRefs: record.EvidenceRefs
                .Concat(record.VisualJudgeStatus.EvidenceRefs)
                .Select(reference => SanitizeReadbackScalar(reference, "local_artifact_evidence_ref", redactions, "evidence"))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Warnings: warnings,
            BoundaryMetadata: boundaryMetadata);
    }

    private static DesignConversionLocalArtifactReadbackRenderRef ToReadbackRenderRef(
        DesignConversionRenderArtifactDbRef render,
        SortedSet<string> redactions)
        => new(
            ViewportId: SanitizeReadbackScalar(render.ViewportId, "render_viewport_id", redactions, "viewport"),
            ArtifactId: SanitizeReadbackScalar(render.ArtifactId, "render_artifact_id", redactions, "render-artifact"),
            Kind: SanitizeReadbackScalar(render.Kind, "render_kind", redactions, "render"),
            UriRef: SanitizeReadbackScalar(render.UriRef, "render_uri_ref", redactions, string.Empty),
            Sha256: SanitizeReadbackScalar(render.Sha256, "render_sha256", redactions, string.Empty),
            ByteLength: render.ByteLength,
            Width: render.Width,
            Height: render.Height,
            Metadata: DesignConversionDpoLeakageGuard.SanitizeMetadata(
                render.Metadata,
                "render_metadata",
                redactions));

    private static IReadOnlyList<string> BuildReadbackWarnings(DesignConversionLocalArtifactDbRecord record)
    {
        var warnings = new List<string>
        {
            "local_artifact_readback_review_only",
            "local_artifact_db=true",
            "raw_html_css_tree_private_provider_payloads_not_emitted",
            $"dpo_candidate_count={record.DpoCandidates.Count}",
            "public_api_verified=false",
            "production_db_ready=false",
            "trained_model_ready=false",
            ProductNotReadyBoundary
        };

        if (HasPendingRuntimeCreatedDpoCandidate(record))
        {
            warnings.Add("runtime_created_dpo_candidate_review_pending");
            warnings.Add("runtime_created_dpo_candidate_approval_required");
            warnings.Add("training_candidate_only_not_approved");
            warnings.Add("approved_for_training=false");
            warnings.Add("production_dpo=false");
            warnings.Add("model_readiness_claimed=false");
            warnings.Add("held_out_not_proven=true");
            warnings.Add("source_disjoint_not_proven=true");
        }
        if (HasApprovedRuntimeCreatedDpoCandidate(record))
        {
            warnings.Add("runtime_created_dpo_candidate_approved_for_training");
            warnings.Add("approval_status=approved");
            warnings.Add("approved_for_training=true");
            warnings.Add("production_dpo=false");
            warnings.Add("model_readiness_claimed=false");
            warnings.Add("held_out_not_proven=true");
            warnings.Add("source_disjoint_not_proven=true");
        }

        return warnings
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string> BuildReadbackRowBoundaryMetadata(
        DesignConversionLocalArtifactDbRecord record,
        SortedSet<string> redactions)
    {
        var hasRuntimeCreatedDpoCandidate = HasRuntimeCreatedDpoCandidate(record);
        var hasApprovedRuntimeCreatedDpoCandidate = HasApprovedRuntimeCreatedDpoCandidate(record);
        var hasPendingRuntimeCreatedDpoCandidate = HasPendingRuntimeCreatedDpoCandidate(record);
        var trainingCandidateOnly = record.DpoCandidates.Any(candidate => candidate.TrainingCandidateOnly);
        var metadata = new Dictionary<string, string>(
            DesignConversionDpoLeakageGuard.SanitizeMetadata(
                record.Metadata,
                "local_artifact_readback_metadata",
                redactions),
            StringComparer.Ordinal)
        {
            ["api_shape"] = LocalArtifactReadbackApiShape,
            ["runtime_surface"] = LocalArtifactReadbackRuntimeSurface,
            ["backing_store"] = nameof(IDesignConversionLocalArtifactDb),
            ["local_artifact_db"] = bool.TrueString,
            ["local_artifact_record"] = bool.TrueString,
            ["dpo_candidate_count"] = record.DpoCandidates.Count.ToString(),
            ["runtime_created_dpo_candidate"] = hasRuntimeCreatedDpoCandidate.ToString(),
            ["review_required"] = hasRuntimeCreatedDpoCandidate.ToString(),
            ["approval_required"] = hasRuntimeCreatedDpoCandidate.ToString(),
            ["approval_status"] = hasRuntimeCreatedDpoCandidate
                ? hasPendingRuntimeCreatedDpoCandidate ? ApprovalPendingStatus : ApprovalApprovedStatus
                : "not-required",
            ["review_status"] = hasRuntimeCreatedDpoCandidate
                ? hasPendingRuntimeCreatedDpoCandidate ? RuntimeCandidateReviewPendingStatus : RuntimeCandidateReviewApprovedStatus
                : "not-required",
            ["approved_for_training"] = hasApprovedRuntimeCreatedDpoCandidate.ToString(),
            ["training_candidate_only"] = trainingCandidateOnly.ToString(),
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "local_artifact_readback_not_training_grade",
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] = hasRuntimeCreatedDpoCandidate.ToString(),
            ["source_disjoint_not_proven"] = hasRuntimeCreatedDpoCandidate.ToString(),
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["exact_prompt_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };
        AppendSourceEncodingReadbackMetadata(record, metadata, redactions);
        if (!metadata.ContainsKey("artifact_db_record_source"))
            metadata["artifact_db_record_source"] = "unknown";
        if (redactions.Count > 0)
            metadata["readback_redaction_codes"] = string.Join(",", redactions);

        return metadata;
    }

    private static void AppendSourceEncodingReadbackMetadata(
        DesignConversionLocalArtifactDbRecord record,
        IDictionary<string, string> metadata,
        SortedSet<string> redactions)
    {
        var trainingRecords = record.DpoCandidates
            .Select(candidate => candidate.TrainingRecord)
            .Where(trainingRecord => trainingRecord is not null)
            .ToArray();
        if (trainingRecords.Length == 0)
            return;

        metadata["dpo_internal_vector_dimension_count"] = JoinDistinct(
            trainingRecords.Select(trainingRecord => trainingRecord.InternalVectorPayload.Count.ToString()));
        metadata["dpo_source_encoder_vector_hashes"] = JoinMetadataValues(
            trainingRecords,
            "source_encoder_vector_hash",
            redactions);
        metadata["dpo_source_encoder_feature_map_hashes"] = JoinMetadataValues(
            trainingRecords,
            "source_encoder_feature_map_hash",
            redactions);
        metadata["dpo_source_encoder_feature_names"] = JoinMetadataValues(
            trainingRecords,
            "source_encoder_feature_names",
            redactions);
        metadata["dpo_source_structure_signal_summary"] = JoinMetadataValues(
            trainingRecords,
            "source_structure_signal_summary",
            redactions);
        metadata["dpo_result_signal_summary"] = JoinMetadataValues(
            trainingRecords,
            "result_signal_summary",
            redactions);
        metadata["dpo_source_encoder_shortcut_guard"] = JoinMetadataValues(
            trainingRecords,
            "source_encoder_shortcut_guard",
            redactions);
        metadata["dpo_source_kind_in_vector"] = JoinMetadataValues(
            trainingRecords,
            "source_kind_in_vector",
            redactions);
        metadata["dpo_source_id_in_vector"] = JoinMetadataValues(
            trainingRecords,
            "source_id_in_vector",
            redactions);
    }

    private static string JoinMetadataValues(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        string key,
        SortedSet<string> redactions)
        => JoinDistinct(records
            .Select(record => record.Provenance.Metadata.TryGetValue(key, out var value)
                ? SanitizeReadbackScalar(value, $"source_encoder_readback_{key}", redactions, "missing")
                : "missing"));

    private static string JoinDistinct(IEnumerable<string> values)
    {
        var joined = string.Join(
            "|",
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal));
        return string.IsNullOrWhiteSpace(joined) ? "missing" : joined;
    }

    private static IReadOnlyDictionary<string, string> BuildReadbackResultBoundaryMetadata(
        IReadOnlyList<DesignConversionLocalArtifactReadbackApiRow> rows)
    {
        var runtimeReviewPendingCount = rows.Count(row =>
            string.Equals(row.DpoReviewStatus, RuntimeCandidateReviewPendingStatus, StringComparison.Ordinal));
        var runtimeReviewApprovedCount = rows.Count(row =>
            string.Equals(row.DpoApprovalStatus, ApprovalApprovedStatus, StringComparison.Ordinal));

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["api_shape"] = LocalArtifactReadbackApiShape,
            ["runtime_surface"] = LocalArtifactReadbackRuntimeSurface,
            ["backing_store"] = nameof(IDesignConversionLocalArtifactDb),
            ["record_count"] = rows.Count.ToString(),
            ["dpo_candidate_count"] = rows.Sum(row => row.DpoCandidateCount).ToString(),
            ["runtime_review_pending_count"] = runtimeReviewPendingCount.ToString(),
            ["runtime_review_approved_count"] = runtimeReviewApprovedCount.ToString(),
            ["review_required_count"] = rows.Count(row => row.DpoReviewRequired).ToString(),
            ["approval_status"] = runtimeReviewPendingCount > 0
                ? ApprovalPendingStatus
                : runtimeReviewApprovedCount > 0 ? ApprovalApprovedStatus : "not-required",
            ["approved_for_training_count"] = runtimeReviewApprovedCount.ToString(),
            ["local_artifact_db"] = bool.TrueString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] = (runtimeReviewPendingCount + runtimeReviewApprovedCount > 0).ToString(),
            ["source_disjoint_not_proven"] = (runtimeReviewPendingCount + runtimeReviewApprovedCount > 0).ToString(),
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["exact_prompt_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };
    }

    private static string SanitizeReadbackScalar(
        string? value,
        string scope,
        SortedSet<string> redactions,
        string fallback)
    {
        var sanitized = DesignConversionDpoLeakageGuard.SanitizeScalar(value, scope, redactions);
        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }

    private static DesignConversionDpoUsefulnessApiRow ToApiRow(DesignConversionDpoDisplayRow row)
    {
        var providerMissing = IsTruthy(row.BoundaryMetadata, "provider_missing")
                              || ContainsAny(row.BlockedReason, row.UsefulnessFactors, row.EvidenceRefs, "provider_missing", "provider_required", "provider_missing_or_blocked");
        var negativeRejected = string.Equals(
                                  ReadMetadata(row.BoundaryMetadata, "negative_quality_status"),
                                  "rejected_negative_quality",
                                  StringComparison.Ordinal)
                              || ContainsAny(row.BlockedReason, row.EvidenceRefs, row.UsefulnessFactors, "negative_quality", "negative_reason_taxonomy", "rejected_not_realistic_negative");
        var heldOut = IsTruthy(row.BoundaryMetadata, "heldout")
                      || string.Equals(ReadMetadata(row.BoundaryMetadata, "split"), "held_out", StringComparison.OrdinalIgnoreCase);
        var reviewRequired = IsReviewRequired(row);
        var approvalStatus = BuildApprovalStatus(row, reviewRequired);
        var reviewStatus = BuildReviewStatus(row, providerMissing, negativeRejected, heldOut, reviewRequired, approvalStatus);
        var trainReady = string.Equals(reviewStatus, "train-ready", StringComparison.Ordinal);
        var heldOutNotProven = IsTruthy(row.BoundaryMetadata, "held_out_not_proven")
                                || (!heldOut && row.UsefulnessFactors.Contains("heldout_quality=not_proven", StringComparer.Ordinal));
        var sourceDisjointNotProven = IsTruthy(row.BoundaryMetadata, "source_disjoint_not_proven")
                                      || row.UsefulnessFactors.Contains("source_disjoint=not_proven", StringComparer.Ordinal);
        var retrainDecisionStatus = BuildRetrainDecisionStatus(
            row,
            providerMissing,
            negativeRejected,
            heldOut,
            reviewRequired,
            approvalStatus,
            trainReady);
        var retrainDecisionEvidenceRefs = BuildRetrainDecisionEvidenceRefs(
            row,
            reviewStatus,
            approvalStatus,
            trainReady,
            retrainDecisionStatus,
            heldOutNotProven,
            sourceDisjointNotProven);

        return new DesignConversionDpoUsefulnessApiRow(
            DisplayId: row.DisplayId,
            UnitId: row.UnitId,
            SourceKind: row.SourceKind,
            SourceRef: row.SourceRef,
            ChosenSummary: row.ChosenSummary,
            RejectedSummary: row.RejectedSummary,
            UsefulnessScore: row.UsefulnessScore,
            UsefulnessStatus: row.UsefulnessLabel,
            ReviewStatus: reviewStatus,
            ReviewRequired: reviewRequired,
            ApprovalStatus: approvalStatus,
            TrainReady: trainReady,
            TrainingCandidateOnly: row.TrainingCandidateOnly,
            Quarantined: row.IsQuarantined,
            ProviderMissing: providerMissing,
            RejectedNegativeQuality: negativeRejected,
            HeldOut: heldOut,
            AllowsTraining: row.AllowsTraining,
            ContainsUserPrivateData: row.ContainsUserPrivateData,
            ProductNotReady: true,
            ProductionDpo: IsTruthy(row.BoundaryMetadata, "production_dpo"),
            ModelReadinessClaimed: IsTruthy(row.BoundaryMetadata, "model_readiness_claimed"),
            HeldOutNotProven: heldOutNotProven,
            SourceDisjointNotProven: sourceDisjointNotProven,
            RetrainDecisionStatus: retrainDecisionStatus,
            RetrainDecisionEvidenceRefs: retrainDecisionEvidenceRefs,
            UsefulnessMetrics: BuildUsefulnessMetrics(
                row,
                approvalStatus,
                trainReady,
                retrainDecisionStatus,
                heldOut,
                heldOutNotProven,
                sourceDisjointNotProven),
            ReadinessBoundary: row.ReadinessBoundary,
            BlockReasons: SplitReasons(row.BlockedReason),
            UsefulnessFactors: row.UsefulnessFactors,
            Defects: row.Defects,
            EvidenceRefs: row.EvidenceRefs,
            BoundaryMetadata: new Dictionary<string, string>(row.BoundaryMetadata, StringComparer.Ordinal)
            {
                ["api_review_status"] = reviewStatus,
                ["review_required"] = reviewRequired.ToString(),
                ["approval_status"] = approvalStatus,
                ["api_train_ready"] = trainReady.ToString(),
                ["training_candidate_only"] = row.TrainingCandidateOnly.ToString(),
                ["approved_for_training"] = trainReady.ToString(),
                ["training_grade"] = bool.FalseString,
                ["not_training_grade"] = bool.TrueString,
                ["training_grade_boundary"] = "usefulness_api_row_not_training_grade",
                ["api_retrain_decision_status"] = retrainDecisionStatus,
                ["retrain_decision_evidence_ref_count"] = retrainDecisionEvidenceRefs.Count.ToString(CultureInfo.InvariantCulture),
                ["retrain_decision_surface"] = "row_status_plus_refs_only",
                ["production_dpo"] = bool.FalseString,
                ["api_product_ready"] = bool.FalseString,
                ["model_readiness_claimed"] = bool.FalseString,
                ["held_out_not_proven"] = heldOutNotProven.ToString(),
                ["source_disjoint_not_proven"] = sourceDisjointNotProven.ToString(),
                ["raw_html_included"] = bool.FalseString,
                ["raw_css_included"] = bool.FalseString,
                ["raw_design_tree_included"] = bool.FalseString,
                ["raw_user_private_data_included"] = bool.FalseString,
                ["provider_payload_included"] = bool.FalseString
            });
    }

    private static DesignConversionDpoUsefulnessMetrics BuildUsefulnessMetrics(
        DesignConversionDpoDisplayRow row,
        string approvalStatus,
        bool trainReady,
        string retrainDecisionStatus,
        bool heldOut,
        bool heldOutNotProven,
        bool sourceDisjointNotProven)
        => new(
            Score: row.UsefulnessScore,
            AcceptanceReason: BuildAcceptanceReason(row, approvalStatus, trainReady, retrainDecisionStatus),
            QualityGain: ReadFactorDouble(row.UsefulnessFactors, "score_delta"),
            NegativePairUsefulness: ReadFactorValue(row.UsefulnessFactors, "negative_pair_quality", "unknown"),
            SourceCoverage: BuildSourceCoverage(row, heldOut, heldOutNotProven, sourceDisjointNotProven),
            SourceDisjointStatus: BuildSourceDisjointStatus(row, sourceDisjointNotProven),
            HeldOutEvaluationStatus: BuildHeldOutEvaluationStatus(row, heldOut, heldOutNotProven),
            PromotionEligibility: BuildPromotionEligibility(trainReady, retrainDecisionStatus),
            PolicyCheckpointEvidenceRefs: BuildPolicyCheckpointEvidenceRefs(row),
            ProductReady: false,
            ProductReadyBoundary: "product_ready=false");

    private static string BuildSourceDisjointStatus(
        DesignConversionDpoDisplayRow row,
        bool sourceDisjointNotProven)
    {
        var metadataStatus = ReadMetadata(row.BoundaryMetadata, "source_disjoint_status");
        if (!string.IsNullOrWhiteSpace(metadataStatus))
            return metadataStatus;

        return sourceDisjointNotProven ? "not_proven" : "proven";
    }

    private static string BuildHeldOutEvaluationStatus(
        DesignConversionDpoDisplayRow row,
        bool heldOut,
        bool heldOutNotProven)
    {
        var metadataStatus = ReadMetadata(row.BoundaryMetadata, "held_out_evaluation_status");
        if (!string.IsNullOrWhiteSpace(metadataStatus))
            return metadataStatus;

        if (heldOut)
            return "held_out_row_not_training_input";

        return heldOutNotProven ? "not_proven" : "proven";
    }

    private static string BuildPromotionEligibility(
        bool trainReady,
        string retrainDecisionStatus)
        => trainReady
            ? "retrain_intake_eligible;local_promotion_requires_training_run_measurement_gate_and_policy_checkpoint"
            : $"not_eligible:{retrainDecisionStatus}";

    private static IReadOnlyList<string> BuildPolicyCheckpointEvidenceRefs(
        DesignConversionDpoDisplayRow row)
    {
        var evidenceRefs = row.EvidenceRefs
            .Concat(row.UsefulnessFactors)
            .Concat(row.BoundaryMetadata.SelectMany(pair => new[] { pair.Key, pair.Value }))
            .Where(IsPolicyCheckpointEvidence)
            .Select(reference => DesignConversionDpoLeakageGuard.SanitizeScalar(
                reference,
                "dpo_usefulness_policy_checkpoint_evidence",
                new SortedSet<string>(StringComparer.Ordinal)))
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return evidenceRefs.Length == 0
            ? ["policy_checkpoint_evidence:not_present"]
            : evidenceRefs;
    }

    private static bool IsPolicyCheckpointEvidence(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && (value.Contains("policy_checkpoint", StringComparison.OrdinalIgnoreCase)
               || value.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence, StringComparison.Ordinal)
               || value.Contains(DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence, StringComparison.Ordinal));

    private static string BuildAcceptanceReason(
        DesignConversionDpoDisplayRow row,
        string approvalStatus,
        bool trainReady,
        string retrainDecisionStatus)
    {
        var approvalReason = ReadMetadata(row.BoundaryMetadata, "approval_reason");
        if (!string.IsNullOrWhiteSpace(approvalReason))
            return approvalReason;
        if (trainReady)
            return retrainDecisionStatus;
        if (string.Equals(approvalStatus, ApprovalPendingStatus, StringComparison.OrdinalIgnoreCase))
            return "approval_required_before_training";
        return retrainDecisionStatus;
    }

    private static string BuildSourceCoverage(
        DesignConversionDpoDisplayRow row,
        bool heldOut,
        bool heldOutNotProven,
        bool sourceDisjointNotProven)
    {
        var parts = new List<string>
        {
            string.IsNullOrWhiteSpace(row.SourceKind) ? "source_kind=unknown" : $"source_kind={row.SourceKind}",
            string.IsNullOrWhiteSpace(row.SourceRef) ? "source_ref=missing" : "source_ref=present"
        };

        parts.Add(heldOut ? "heldout_quality=heldout_row" : heldOutNotProven ? "heldout_quality=not_proven" : "heldout_quality=proven");
        parts.Add(sourceDisjointNotProven ? "source_disjoint=not_proven" : "source_disjoint=proven");

        var sourceEncoderDimensions = ReadMetadata(row.BoundaryMetadata, "source_encoder_dimensions");
        if (!string.IsNullOrWhiteSpace(sourceEncoderDimensions))
            parts.Add($"source_encoder_dimensions={sourceEncoderDimensions}");

        return string.Join(";", parts);
    }

    private static double ReadFactorDouble(
        IReadOnlyList<string> factors,
        string key)
        => double.TryParse(
            ReadFactorValue(factors, key, "0"),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : 0;

    private static string ReadFactorValue(
        IReadOnlyList<string> factors,
        string key,
        string fallback)
    {
        var prefix = key + "=";
        var match = factors.FirstOrDefault(factor =>
            factor.StartsWith(prefix, StringComparison.Ordinal));
        return string.IsNullOrWhiteSpace(match) ? fallback : match[prefix.Length..];
    }

    private static string BuildRetrainDecisionStatus(
        DesignConversionDpoDisplayRow row,
        bool providerMissing,
        bool negativeRejected,
        bool heldOut,
        bool reviewRequired,
        string approvalStatus,
        bool trainReady)
    {
        if (trainReady)
            return "ready-for-retrain-intake";
        if (providerMissing)
            return "blocked-provider-evidence-required";
        if (reviewRequired && !string.Equals(approvalStatus, ApprovalApprovedStatus, StringComparison.OrdinalIgnoreCase))
            return "requires-architect-approval-before-retrain";
        if (negativeRejected)
            return "blocked-negative-quality";
        if (row.IsQuarantined)
            return "blocked-quarantined";
        if (heldOut)
            return "holdout-evidence-not-training-input";
        if (row.ContainsUserPrivateData)
            return "blocked-private-data";
        if (!row.AllowsTraining)
            return "blocked-license";
        if (row.IsBlocked)
            return "blocked-not-training-candidate";

        return "review-before-retrain";
    }

    private static IReadOnlyList<string> BuildRetrainDecisionEvidenceRefs(
        DesignConversionDpoDisplayRow row,
        string reviewStatus,
        string approvalStatus,
        bool trainReady,
        string retrainDecisionStatus,
        bool heldOutNotProven,
        bool sourceDisjointNotProven)
    {
        var evidenceRefs = new List<string>
        {
            $"design-conversion-dpo-retrain-decision:status:{retrainDecisionStatus}",
            $"design-conversion-dpo-retrain-decision:review_status:{reviewStatus}",
            $"design-conversion-dpo-retrain-decision:approval_status:{approvalStatus}",
            $"design-conversion-dpo-retrain-decision:usefulness_status:{row.UsefulnessLabel}",
            $"design-conversion-dpo-retrain-decision:usefulness_score:{row.UsefulnessScore.ToString("0.######", CultureInfo.InvariantCulture)}",
            $"design-conversion-dpo-retrain-decision:train_ready:{trainReady.ToString().ToLowerInvariant()}",
            $"design-conversion-dpo-retrain-decision:readiness_boundary:{row.ReadinessBoundary}",
            $"design-conversion-dpo-retrain-decision:held_out_not_proven:{heldOutNotProven.ToString().ToLowerInvariant()}",
            $"design-conversion-dpo-retrain-decision:source_disjoint_not_proven:{sourceDisjointNotProven.ToString().ToLowerInvariant()}"
        };
        evidenceRefs.AddRange(row.EvidenceRefs.Count == 0
            ? ["design-conversion-dpo-retrain-decision:evidence_missing"]
            : row.EvidenceRefs);

        return evidenceRefs
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildReviewStatus(
        DesignConversionDpoDisplayRow row,
        bool providerMissing,
        bool negativeRejected,
        bool heldOut,
        bool reviewRequired,
        string approvalStatus)
    {
        if (providerMissing)
            return "provider-missing";
        if (reviewRequired && !string.Equals(approvalStatus, "approved", StringComparison.OrdinalIgnoreCase))
            return RuntimeCandidateReviewPendingStatus;
        if (reviewRequired
            && string.Equals(approvalStatus, ApprovalApprovedStatus, StringComparison.OrdinalIgnoreCase)
            && row.TrainingCandidateOnly)
        {
            if (row.IsQuarantined)
                return "quarantined";
            if (row.IsBlocked)
                return "blocked";
            return "train-ready";
        }
        if (negativeRejected)
            return "rejected-negative-quality";
        if (row.IsQuarantined)
            return "quarantined";
        if (heldOut)
            return "heldout";
        if (!row.IsBlocked && row.TrainingCandidateOnly)
            return "train-ready";

        return "blocked";
    }

    private static bool IsReviewRequired(DesignConversionDpoDisplayRow row)
        => IsTruthy(row.BoundaryMetadata, "review_required")
           || IsRuntimeCreatedReviewRow(row.BoundaryMetadata);

    private static string BuildApprovalStatus(DesignConversionDpoDisplayRow row, bool reviewRequired)
    {
        var approvalStatus = ReadMetadata(row.BoundaryMetadata, "approval_status");
        if (!string.IsNullOrWhiteSpace(approvalStatus))
            return approvalStatus;

        return reviewRequired ? ApprovalPendingStatus : "not-required";
    }

    private static IReadOnlyList<string> SplitReasons(string reason)
        => string.IsNullOrWhiteSpace(reason) || string.Equals(reason, "not_blocked", StringComparison.Ordinal)
            ? []
            : reason.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value)
           && string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase);

    private static string ReadMetadata(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) ? value : string.Empty;

    private static string FormatUsefulnessScore(double? value)
        => value is null
            ? "none"
            : value.Value.ToString("0.######", CultureInfo.InvariantCulture);

    private static bool HasRuntimeCreatedDpoCandidate(DesignConversionLocalArtifactDbRecord record)
        => IsRuntimeCreatedReviewRow(record.Metadata)
           || record.DpoCandidates.Any(candidate => IsRuntimeCreatedReviewRow(candidate.TrainingRecord.Provenance.Metadata));

    private static bool HasPendingRuntimeCreatedDpoCandidate(DesignConversionLocalArtifactDbRecord record)
        => record.DpoCandidates.Any(candidate =>
            IsRuntimeCreatedReviewRow(candidate.TrainingRecord.Provenance.Metadata)
            && !IsApprovedReviewMetadata(candidate.TrainingRecord.Provenance.Metadata));

    private static bool HasApprovedRuntimeCreatedDpoCandidate(DesignConversionLocalArtifactDbRecord record)
        => record.DpoCandidates.Any(candidate =>
            IsRuntimeCreatedReviewRow(candidate.TrainingRecord.Provenance.Metadata)
            && IsApprovedReviewMetadata(candidate.TrainingRecord.Provenance.Metadata));

    private static bool IsRuntimeCreatedReviewRow(IReadOnlyDictionary<string, string> metadata)
        => IsTruthy(metadata, "runtime_created_dpo_candidate")
           || ReadMetadata(metadata, "split")
                .Contains("runtime_training_candidate_review", StringComparison.OrdinalIgnoreCase);

    private static bool IsApprovedReviewMetadata(IReadOnlyDictionary<string, string> metadata)
        => string.Equals(ReadMetadata(metadata, "approval_status"), ApprovalApprovedStatus, StringComparison.OrdinalIgnoreCase)
           && IsTruthy(metadata, "approved_for_training");

    private static bool ContainsAny(
        string scalar,
        IReadOnlyList<string> first,
        IReadOnlyList<string> second,
        params string[] tokens)
        => TokensContain(scalar, tokens)
           || first.Any(item => TokensContain(item, tokens))
           || second.Any(item => TokensContain(item, tokens));

    private static bool TokensContain(string? value, IReadOnlyList<string> tokens)
        => !string.IsNullOrWhiteSpace(value)
           && tokens.Any(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));
}
