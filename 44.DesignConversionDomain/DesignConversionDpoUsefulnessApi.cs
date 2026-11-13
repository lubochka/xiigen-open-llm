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

public sealed record DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult(
    string LocalArtifactRecordId, string CandidateId, string TrainingRecordId, string UnitId,
    string SourceHandoffArtifactRef, string IntakeId, string IntakeRef,
    string StableIntakeIdRef, string IdempotencyKeyRef,
    bool IdempotencyReplayHit, bool IdempotencyCreatedNewSideEffect,
    string PreflightId, string PreflightStatus, bool MayStartTraining,
    int AcceptedCandidateCount, int DpoStepCount,
    string MeasurementGateId, string MeasurementGateUnitId, string MeasurementGateContractRef,
    DateTimeOffset PersistedRequestedAtUtc,
    bool TrainingRunnerInvoked, bool TrainingStarted, bool CheckpointMutated,
    bool WeightsProduced, bool ModelUploadReady,
    IReadOnlyList<string> EvidenceRefs, IReadOnlyList<string> Warnings);

public sealed class DesignConversionDpoUsefulnessApi
{
    public const string ApiShape = "design_conversion_dpo_usefulness_display_api_v1";
    public const string LocalArtifactReadbackApiShape = "design_conversion_local_artifact_readback_api_v1";
    public const string ApprovedRuntimeDpoCandidateTrainingIntakePreflightSchemaVersion = "design-conversion-approved-runtime-dpo-candidate-training-intake-preflight-v1";
    public const string RuntimeSurface = "in_process_design_conversion_dpo_usefulness_api";
    public const string LocalArtifactReadbackRuntimeSurface = "in_process_design_conversion_local_artifact_readback_api";
    public const string ProductNotReadyBoundary = "product_not_ready";
    public const string RuntimeCandidateReviewPendingStatus = "runtime-candidate-review-pending";
    public const string RuntimeCandidateReviewApprovedStatus = "runtime-candidate-review-approved";
    public const string ApprovalPendingStatus = "pending";
    public const string ApprovalApprovedStatus = "approved";
    private const int ApprovedRuntimeDpoCandidatePreflightDpoStepCount = 1;

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

    /// <summary>
    /// Утверждает список runtime-кандидатов через публичную границу полезности, не обходя
    /// право допуска: хранилище проверяет всю пачку и делает одну атомарную перепись журнала.
    /// </summary>
    public Task<OperationResult<DesignConversionDpoCandidateApprovalBatchResult>>
        ApproveRuntimeCreatedDpoCandidatesAsync(
            IReadOnlyList<DesignConversionDpoCandidateApprovalRequest> requests,
            CancellationToken ct = default)
        => _artifactDb.ApproveRuntimeCreatedDpoCandidatesAsync(
            requests,
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

    public async Task<OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>>
        RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync(
        string localArtifactRecordId,
        string candidateOrTrainingRecordId,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>.Fail(
                "DesignConversion approved runtime DPO candidate preflight was cancelled.",
                "CANCELLED");

        if (new[] { localArtifactRecordId, candidateOrTrainingRecordId, requestedByRef, reason }
            .Any(value => string.IsNullOrWhiteSpace(value)
                          || !string.Equals(value, value.Trim(), StringComparison.Ordinal)))
        {
            return OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>.Fail(
                "DesignConversion approved runtime DPO candidate preflight requires canonical nonblank exact inputs.",
                "DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_INPUT_NOT_CANONICAL");
        }

        var requestedAt = (requestedAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();

        var query = new DesignConversionLocalArtifactDbQuery(ApprovedTrainingOnly: true, LocalArtifactRecordId: localArtifactRecordId, CandidateOrTrainingRecordId: candidateOrTrainingRecordId);
        var handoff = await ExportApprovedRuntimeDpoCandidatesAsync(query, requestedAt, ct);
        if (!handoff.IsSuccess)
            return handoff.MapError<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>();

        var handoffValue = handoff.Value!;
        if (handoffValue.ApprovedCandidateCount != 1 || handoffValue.Rows.Count != 1)
            return OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>.Fail(
                "DesignConversion approved runtime DPO candidate preflight requires exactly one already-approved runtime candidate.",
                "DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_EXACT_APPROVED_ROW_REQUIRED");

        var row = handoffValue.Rows[0];
        if (!IsExactApprovedRuntimePreflightRow(row, localArtifactRecordId, candidateOrTrainingRecordId))
            return OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>.Fail(
                "DesignConversion approved runtime DPO candidate preflight handoff row did not match the exact requested record/candidate identity.",
                "DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_EXACT_IDENTITY_MISMATCH");

        var gate = BuildApprovedRuntimeDpoCandidatePreflightMeasurementGate(row);
        var gateContractRef = DesignConversionImprovementLoopCoordinator
            .BuildTrainingIntakePreflightMeasurementGateContractRef(gate);
        var stableIdentity = BuildApprovedRuntimeDpoCandidatePreflightStableIdentity(row);
        var stableIntakeIdRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-approved-runtime-dpo-preflight-stable-intake",
            stableIdentity);
        var idempotencyKeyRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-approved-runtime-dpo-preflight-idempotency",
            stableIdentity);
        var admissionMetadata = BuildApprovedRuntimeDpoCandidatePreflightAdmissionMetadata(
            row,
            handoffValue,
            gate,
            gateContractRef,
            stableIntakeIdRef,
            idempotencyKeyRef);

        var existing = await ListTrainingIntakeRequestsAsync(ct);
        if (!existing.IsSuccess)
            return existing.MapError<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>();
        var replayMatches = existing.Value!
            .Where(intake => MetadataMatches(intake.BoundaryMetadata, admissionMetadata,
                "training_intake_tenant_id_ref", "training_intake_idempotency_key_ref"))
            .ToArray();
        if (replayMatches.Length > 1)
            return OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>.Fail(
                "DesignConversion approved runtime DPO candidate preflight found ambiguous durable idempotency state.",
                "DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_IDEMPOTENCY_AMBIGUOUS");

        var replayHit = replayMatches.Length == 1;
        DesignConversionTrainingIntakeRecord intakeValue;
        if (replayHit)
        {
            intakeValue = replayMatches[0];
        }
        else
        {
            var created = await CreateTrainingIntakeAsync(
                handoffValue, requestedByRef, reason, requestedAt, ct, admissionMetadata);
            if (!created.IsSuccess)
                return created.MapError<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>();
            intakeValue = created.Value!;
            replayHit = IsTruthy(intakeValue.BoundaryMetadata, "training_intake_idempotency_replay_hit");
        }

        if (!ValidateApprovedRuntimeDpoCandidatePreflightIntake(
                intakeValue, row, requestedByRef, reason, admissionMetadata))
            return OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>.Fail(
                "DesignConversion approved runtime DPO candidate preflight intake did not preserve its exact durable contract.",
                "DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_INTAKE_CONTRACT_MISMATCH");

        var preflight = await RunTrainingIntakePreflightAsync(intakeValue.IntakeId, ct);
        if (!preflight.IsSuccess)
            return preflight.MapError<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>();
        var preflightValue = preflight.Value!;
        if (!string.Equals(preflightValue.TrainingIntakeId, intakeValue.IntakeId, StringComparison.Ordinal)
            || preflightValue.TrainingStarted
            || preflightValue.CheckpointMutated
            || preflightValue.WeightsProduced
            || preflightValue.ModelUploadReady)
            return OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>.Fail(
                "DesignConversion approved runtime DPO candidate preflight crossed the preflight-only boundary.",
                "DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_MUTATION_BOUNDARY_VIOLATED");

        return OperationResult<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>.Ok(
            BuildApprovedRuntimeDpoCandidatePreflightResult(
                row, handoffValue, intakeValue, preflightValue, gate, gateContractRef,
                stableIntakeIdRef, idempotencyKeyRef, replayHit));
    }

    public Task<OperationResult<DesignConversionDpoUsefulnessApiResult>> QueryAsync(
        DesignConversionDpoUsefulnessApiRequest? request = null,
        CancellationToken ct = default)
        => QueryCoreAsync(request, requiredTenantIdRef: null, ct);

    public Task<OperationResult<DesignConversionDpoUsefulnessApiResult>> QueryForAdmittedTenantAsync(
        DesignConversionDpoUsefulnessApiRequest? request,
        string admittedTenantIdRef,
        CancellationToken ct = default)
    {
        if (!DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionTenantIdRef(
                admittedTenantIdRef))
        {
            return Task.FromResult(
                OperationResult<DesignConversionDpoUsefulnessApiResult>.Fail(
                    "DesignConversion DPO usefulness admitted tenant authority is invalid.",
                    "DESIGN_CONVERSION_DPO_USEFULNESS_ADMITTED_TENANT_ID_REF_INVALID"));
        }

        return QueryCoreAsync(request, admittedTenantIdRef, ct);
    }

    private async Task<OperationResult<DesignConversionDpoUsefulnessApiResult>> QueryCoreAsync(
        DesignConversionDpoUsefulnessApiRequest? request,
        string? requiredTenantIdRef,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionDpoUsefulnessApiResult>.Fail(
                "DesignConversion DPO usefulness API query cancelled.",
                "CANCELLED");

        var display = await _artifactDb.QueryDisplayRowsAsync(
            ToDisplayQuery(request, requiredTenantIdRef),
            ct);
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
                ["retrain_decision_product_ready"] = bool.FalseString,
                ["tenant_scope_applied"] =
                    (requiredTenantIdRef is not null).ToString(),
                ["tenant_scope_source"] = requiredTenantIdRef is null
                    ? "legacy_unscoped_query"
                    : "server_admission_context"
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
        var snapshot =
            await _artifactDb
                .ListRecordsWithPostEvaluationTrainingEligibilityAsync(
                    ToLocalArtifactDbQuery(effective),
                    ct);
        if (!snapshot.IsSuccess)
        {
            return snapshot.MapError<
                DesignConversionLocalArtifactReadbackApiResult>();
        }

        var eligibleRows = snapshot.Value!.Records
            .Select((record, index) =>
                ToLocalArtifactReadbackRow(
                    record,
                    index < snapshot.Value.Eligibility.Count
                        ? snapshot.Value.Eligibility[index]
                        : DesignConversionPostEvaluationTrainingEligibility
                            .Denied(
                                DesignConversionPostEvaluationTrainingEligibilityPolicy
                                    .EvaluatorUnavailableReason,
                                DesignConversionPostEvaluationTrainingEligibilityPolicy
                                    .EvaluatorUnavailableErrorCode)))
            .ToArray();

        var rows = eligibleRows
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
            IsTruthy(row.BoundaryMetadata, "approved_for_training"));
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
        DesignConversionDpoUsefulnessApiRequest? request,
        string? requiredTenantIdRef = null)
    {
        var effective = request ?? new DesignConversionDpoUsefulnessApiRequest();
        return new DesignConversionDpoDisplayQuery(
            UnitId: effective.UnitId,
            IncludeBlocked: effective.IncludeBlocked,
            MinimumUsefulnessScore: effective.MinimumUsefulnessScore,
            MaximumUsefulnessScore: effective.MaximumUsefulnessScore,
            Limit: effective.Limit,
            RequiredTenantIdRef: requiredTenantIdRef);
    }

    private static DesignConversionLocalArtifactDbQuery ToLocalArtifactDbQuery(
        DesignConversionLocalArtifactReadbackApiRequest request)
        => new(
            JobId: request.JobId,
            RequestId: request.RequestId,
            SourceKind: request.SourceKind);

    private static bool IsExactApprovedRuntimePreflightRow(
        DesignConversionApprovedDpoHandoffRow row,
        string localArtifactRecordId,
        string candidateOrTrainingRecordId)
        => row is not null
           && string.Equals(row.LocalArtifactRecordId, localArtifactRecordId, StringComparison.Ordinal)
           && (string.Equals(row.CandidateId, candidateOrTrainingRecordId, StringComparison.Ordinal)
               || string.Equals(row.TrainingRecordId, candidateOrTrainingRecordId, StringComparison.Ordinal))
           && !string.IsNullOrWhiteSpace(row.CandidateId) && !string.IsNullOrWhiteSpace(row.TrainingRecordId)
           && !string.IsNullOrWhiteSpace(row.UnitId) && row.ApprovedForTraining && row.TrainingHandoffOnly
           && string.Equals(row.ApprovalStatus, ApprovalApprovedStatus, StringComparison.OrdinalIgnoreCase)
           && string.Equals(row.ReviewStatus, RuntimeCandidateReviewApprovedStatus, StringComparison.Ordinal)
           && !string.IsNullOrWhiteSpace(row.ApprovedByRef) && !string.IsNullOrWhiteSpace(row.ApprovalReason)
           && !string.IsNullOrWhiteSpace(row.ApprovedAtUtc)
           && IsTruthy(row.BoundaryMetadata, "approved_for_training")
           && IsTruthy(row.BoundaryMetadata, "runtime_created_dpo_candidate")
           && !IsTruthy(row.BoundaryMetadata, "model_readiness_claimed")
           && !IsTruthy(row.BoundaryMetadata, "trained_model_ready")
           && !IsTruthy(row.BoundaryMetadata, "production_dpo");

    private static DesignConversionMeasurementGateRequest BuildApprovedRuntimeDpoCandidatePreflightMeasurementGate(
        DesignConversionApprovedDpoHandoffRow row)
    {
        var stableIdentity = BuildApprovedRuntimeDpoCandidatePreflightStableIdentity(row);
        var evidenceRefs = BuildApprovedRuntimeDpoCandidatePreflightGateEvidenceRefs(row);
        return new DesignConversionMeasurementGateRequest(
            GateId: DesignConversionDpoLeakageGuard.HashRef(
                "dcm-approved-runtime-dpo-preflight-gate",
                stableIdentity),
            UnitId: row.UnitId,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: null, HeldOutQualityScore: null,
                RuntimeMs: null, CostUsd: null, PeakRamMb: null,
                EvidenceRefs: evidenceRefs),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: false, LeakageDetected: null,
                HiddenDeterminismDetected: null, AblationPassed: null,
                EvidenceRefs: evidenceRefs),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 1d, MaxRuntimeMs: 1,
                MaxCostUsd: 0m, MaxPeakRamMb: 1));
    }

    private static IReadOnlyList<string> BuildApprovedRuntimeDpoCandidatePreflightGateEvidenceRefs(
        DesignConversionApprovedDpoHandoffRow row)
        => BuildApprovedRuntimeDpoCandidatePreflightPublicEvidenceRefs(row)
            .Select(reference => DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                "approved-runtime-dpo-preflight-evidence",
                reference))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildApprovedRuntimeDpoCandidatePreflightPublicEvidenceRefs(
        DesignConversionApprovedDpoHandoffRow row)
        => (row.EvidenceRefs ?? [])
            .Concat(row.BoundaryMetadata.Select(pair => $"{pair.Key}={pair.Value}"))
            .Concat(["approved_runtime_dpo_candidate_preflight:durable_handoff_row",
                "held_out_measurement:not_durably_proven", "source_disjoint:not_durably_proven",
                "training_mutation:not_started"])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string BuildApprovedRuntimeDpoCandidatePreflightStableIdentity(
        DesignConversionApprovedDpoHandoffRow row)
        => string.Join("|",
        [
            ApprovedRuntimeDpoCandidateTrainingIntakePreflightSchemaVersion,
            DesignConversionDpoLeakageGuard.HashRef("dcm-approved-runtime-dpo-preflight-record", row.LocalArtifactRecordId),
            DesignConversionDpoLeakageGuard.HashRef("dcm-approved-runtime-dpo-preflight-candidate", row.CandidateId),
            DesignConversionDpoLeakageGuard.HashRef("dcm-approved-runtime-dpo-preflight-training", row.TrainingRecordId),
            DesignConversionDpoLeakageGuard.HashRef("dcm-approved-runtime-dpo-preflight-unit", row.UnitId),
            DesignConversionDpoLeakageGuard.HashRef("dcm-approved-runtime-dpo-preflight-handoff", row.RecordHash)
        ]);

    private static IReadOnlyDictionary<string, string> BuildApprovedRuntimeDpoCandidatePreflightAdmissionMetadata(
        DesignConversionApprovedDpoHandoffRow row,
        DesignConversionApprovedDpoHandoffArtifact handoff,
        DesignConversionMeasurementGateRequest gate,
        string gateContractRef,
        string stableIntakeIdRef,
        string idempotencyKeyRef)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["approved_runtime_dpo_candidate_preflight_schema_version"] = ApprovedRuntimeDpoCandidateTrainingIntakePreflightSchemaVersion,
            ["training_intake_tenant_id_ref"] = DesignConversionDpoLeakageGuard.HashRef(
                "dcm-approved-runtime-dpo-preflight-tenant", row.LocalArtifactRecordId),
            ["training_intake_idempotency_key_ref"] = idempotencyKeyRef, ["coordinator_stable_intake_id"] = stableIntakeIdRef,
            ["coordinator_dpo_step_count"] = ApprovedRuntimeDpoCandidatePreflightDpoStepCount.ToString(CultureInfo.InvariantCulture),
            ["coordinator_measurement_gate_id"] = gate.GateId, ["coordinator_measurement_gate_unit_id"] = gate.UnitId,
            ["coordinator_measurement_gate_contract_ref"] = gateContractRef,
            ["approved_runtime_dpo_local_artifact_record_id"] = row.LocalArtifactRecordId,
            ["approved_runtime_dpo_candidate_id"] = row.CandidateId, ["approved_runtime_dpo_training_record_id"] = row.TrainingRecordId,
            ["approved_runtime_dpo_handoff_record_hash"] = row.RecordHash,
            ["approved_runtime_dpo_handoff_artifact_ref"] = handoff.ArtifactRef,
            ["approved_runtime_dpo_approved_by_ref"] = row.ApprovedByRef, ["approved_runtime_dpo_approval_reason"] = row.ApprovalReason,
            ["approved_runtime_dpo_approved_at_utc"] = row.ApprovedAtUtc,
            ["training_runner_invoked"] = bool.FalseString, ["training_started"] = bool.FalseString,
            ["checkpoint_mutated"] = bool.FalseString, ["weights_produced"] = bool.FalseString,
            ["model_upload_ready"] = bool.FalseString,
            ["measurement_gate_held_out_metrics_proven"] = bool.FalseString, ["measurement_gate_integrity_proven"] = bool.FalseString
        };

    private static bool ValidateApprovedRuntimeDpoCandidatePreflightIntake(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionApprovedDpoHandoffRow row,
        string requestedByRef,
        string reason,
        IReadOnlyDictionary<string, string> expectedMetadata)
    {
        var candidate = intake.CandidateRefs.Count == 1 ? intake.CandidateRefs[0] : null;
        return intake.AcceptedCandidateCount == 1
               && candidate is not null
               && string.Equals(candidate.LocalArtifactRecordId, row.LocalArtifactRecordId, StringComparison.Ordinal)
               && string.Equals(candidate.CandidateId, row.CandidateId, StringComparison.Ordinal)
               && string.Equals(candidate.TrainingRecordId, row.TrainingRecordId, StringComparison.Ordinal)
               && string.Equals(intake.RequestedByRef, requestedByRef, StringComparison.Ordinal)
               && string.Equals(intake.RequestReason, reason, StringComparison.Ordinal)
               && intake.TrainingIntakeOnly
               && intake.TrainingNotStarted
               && intake.NotTrained
               && intake.WeightsNotProduced
               && intake.CheckpointNotMutated
               && intake.ModelUploadNotReady
               && MetadataMatches(intake.BoundaryMetadata, expectedMetadata, expectedMetadata.Keys.ToArray());
    }

    private static bool MetadataMatches(
        IReadOnlyDictionary<string, string> actual,
        IReadOnlyDictionary<string, string> expected,
        params string[] keys)
        => keys.All(key => actual.TryGetValue(key, out var actualValue)
                           && expected.TryGetValue(key, out var expectedValue)
                           && string.Equals(actualValue, expectedValue, StringComparison.Ordinal));

    private static DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult
        BuildApprovedRuntimeDpoCandidatePreflightResult(
        DesignConversionApprovedDpoHandoffRow row,
        DesignConversionApprovedDpoHandoffArtifact handoff,
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionMeasurementGateRequest gate,
        string gateContractRef,
        string stableIntakeIdRef,
        string idempotencyKeyRef,
        bool replayHit)
    {
        var evidenceRefs = BuildApprovedRuntimeDpoCandidatePreflightPublicEvidenceRefs(row)
            .Concat([handoff.ArtifactRef, intake.IntakeRef, preflight.PreflightId, gateContractRef,
                "approved_runtime_dpo_candidate_training_intake_preflight_only"])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var warnings = (handoff.Warnings ?? [])
            .Concat(intake.Warnings ?? [])
            .Concat(preflight.Warnings ?? [])
            .Concat(["post_approval_preflight_only", "training_runner_invoked=false",
                "training_started=false", "checkpoint_mutated=false", "weights_produced=false",
                "model_upload_ready=false"])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return new DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult(
            LocalArtifactRecordId: row.LocalArtifactRecordId, CandidateId: row.CandidateId,
            TrainingRecordId: row.TrainingRecordId, UnitId: row.UnitId,
            SourceHandoffArtifactRef: handoff.ArtifactRef, IntakeId: intake.IntakeId,
            IntakeRef: intake.IntakeRef, StableIntakeIdRef: stableIntakeIdRef,
            IdempotencyKeyRef: idempotencyKeyRef, IdempotencyReplayHit: replayHit,
            IdempotencyCreatedNewSideEffect: !replayHit, PreflightId: preflight.PreflightId,
            PreflightStatus: preflight.MayStartTraining ? "may_start_training" : "blocked",
            MayStartTraining: preflight.MayStartTraining, AcceptedCandidateCount: preflight.AcceptedCandidateCount,
            DpoStepCount: ApprovedRuntimeDpoCandidatePreflightDpoStepCount, MeasurementGateId: gate.GateId,
            MeasurementGateUnitId: gate.UnitId, MeasurementGateContractRef: gateContractRef,
            PersistedRequestedAtUtc: intake.RequestedAtUtc,
            TrainingRunnerInvoked: false, TrainingStarted: false, CheckpointMutated: false,
            WeightsProduced: false, ModelUploadReady: false,
            EvidenceRefs: evidenceRefs, Warnings: warnings);
    }

    private static DesignConversionLocalArtifactReadbackApiRow ToLocalArtifactReadbackRow(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionPostEvaluationTrainingEligibility eligibility)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var boundaryMetadata = BuildReadbackRowBoundaryMetadata(
            record,
            redactions,
            eligibility);
        var warnings = BuildReadbackWarnings(record, eligibility);
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

    private static IReadOnlyList<string> BuildReadbackWarnings(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionPostEvaluationTrainingEligibility eligibility)
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
        if (!eligibility.IsEligible)
        {
            warnings.Add(eligibility.Reason);
            warnings.Add("post_evaluation_training_eligible=false");
        }

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
        if (eligibility.IsEligible &&
            HasApprovedRuntimeCreatedDpoCandidate(record))
        {
            warnings.Add("runtime_created_dpo_candidate_approved_for_training");
            warnings.Add("approval_status=approved");
            warnings.Add("approved_for_training=true");
            warnings.Add("production_dpo=false");
            warnings.Add("model_readiness_claimed=false");
            warnings.Add("held_out_not_proven=true");
            warnings.Add("source_disjoint_not_proven=true");
        }
        else if (HasApprovedRuntimeCreatedDpoCandidate(record))
        {
            warnings.Add(
                "runtime_created_dpo_candidate_review_approved_but_training_ineligible");
            warnings.Add(
                "approval_status=review-approved-training-ineligible");
            warnings.Add("approved_for_training=false");
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
        SortedSet<string> redactions,
        DesignConversionPostEvaluationTrainingEligibility eligibility)
    {
        var hasRuntimeCreatedDpoCandidate = HasRuntimeCreatedDpoCandidate(record);
        var hasReviewApprovedRuntimeCreatedDpoCandidate =
            HasApprovedRuntimeCreatedDpoCandidate(record);
        var hasApprovedRuntimeCreatedDpoCandidate =
            eligibility.IsEligible &&
            hasReviewApprovedRuntimeCreatedDpoCandidate;
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
                ? hasPendingRuntimeCreatedDpoCandidate
                    ? ApprovalPendingStatus
                    : hasApprovedRuntimeCreatedDpoCandidate
                        ? ApprovalApprovedStatus
                        : "review-approved-training-ineligible"
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
        metadata["post_evaluation_training_eligible"] =
            eligibility.IsEligible.ToString();
        metadata["post_evaluation_eligibility_reason"] = eligibility.Reason;
        metadata["post_evaluation_eligibility_error_code"] =
            eligibility.ErrorCode;
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
        var runtimeReviewApprovedButTrainingIneligibleCount = rows.Count(row =>
            string.Equals(
                row.DpoApprovalStatus,
                "review-approved-training-ineligible",
                StringComparison.Ordinal));

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["api_shape"] = LocalArtifactReadbackApiShape,
            ["runtime_surface"] = LocalArtifactReadbackRuntimeSurface,
            ["backing_store"] = nameof(IDesignConversionLocalArtifactDb),
            ["record_count"] = rows.Count.ToString(),
            ["dpo_candidate_count"] = rows.Sum(row => row.DpoCandidateCount).ToString(),
            ["runtime_review_pending_count"] = runtimeReviewPendingCount.ToString(),
            ["runtime_review_approved_count"] = runtimeReviewApprovedCount.ToString(),
            ["runtime_review_approved_but_training_ineligible_count"] =
                runtimeReviewApprovedButTrainingIneligibleCount.ToString(),
            ["review_required_count"] = rows.Count(row => row.DpoReviewRequired).ToString(),
            ["approval_status"] = runtimeReviewPendingCount > 0
                ? ApprovalPendingStatus
                : runtimeReviewApprovedCount > 0
                    ? ApprovalApprovedStatus
                    : runtimeReviewApprovedButTrainingIneligibleCount > 0
                        ? "review-approved-training-ineligible"
                        : "not-required",
            ["approved_for_training_count"] = runtimeReviewApprovedCount.ToString(),
            ["local_artifact_db"] = bool.TrueString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] =
                (runtimeReviewPendingCount +
                 runtimeReviewApprovedCount +
                 runtimeReviewApprovedButTrainingIneligibleCount > 0)
                .ToString(),
            ["source_disjoint_not_proven"] =
                (runtimeReviewPendingCount +
                 runtimeReviewApprovedCount +
                 runtimeReviewApprovedButTrainingIneligibleCount > 0)
                .ToString(),
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
