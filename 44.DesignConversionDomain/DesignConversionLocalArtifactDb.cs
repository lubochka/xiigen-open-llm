// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Collections.Concurrent;
using System.Globalization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using AgentInfrastructure;

public sealed record DesignConversionLocalArtifactDbQuery(
    string? JobId = null,
    string? RequestId = null,
    string? SourceKind = null,
    string? UnitId = null,
    double? MinimumUsefulnessScore = null,
    bool ApprovedTrainingOnly = false);

public sealed record DesignConversionConversionResultArtifact(
    string ResultId,
    string HtmlRef,
    string HtmlSha256,
    string CssRef,
    string CssSha256,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionRenderArtifactDbRef(
    string ViewportId,
    string ArtifactId,
    string Kind,
    string UriRef,
    string Sha256,
    long ByteLength,
    int Width,
    int Height,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionVisualJudgeArtifactDbStatus(
    string Status,
    string ProviderBoundary,
    bool ExternalCallExecuted,
    decimal? EstimatedCostUsd,
    string TierDecision,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionLocalArtifactDbDpoCandidate(
    string CandidateId,
    string RecordId,
    string UnitId,
    double UsefulnessScore,
    bool TrainingCandidateOnly,
    bool ProductNotReady,
    bool AllowsTraining,
    bool ContainsUserPrivateData,
    bool IsQuarantined,
    string QuarantineReason,
    IReadOnlyList<string> BlockReasons,
    DesignConversionDpoTrainingRecord TrainingRecord)
{
    public static DesignConversionLocalArtifactDbDpoCandidate FromTrainingRecord(
        DesignConversionDpoTrainingRecord record,
        string? candidateId = null,
        bool trainingCandidateOnly = true)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        return new DesignConversionLocalArtifactDbDpoCandidate(
            CandidateId: string.IsNullOrWhiteSpace(candidateId)
                ? $"{record.RecordId}:dpo_candidate"
                : candidateId,
            RecordId: record.RecordId,
            UnitId: record.UnitId,
            UsefulnessScore: record.UsefulnessScore,
            TrainingCandidateOnly: trainingCandidateOnly,
            ProductNotReady: true,
            AllowsTraining: record.License.AllowsTraining,
            ContainsUserPrivateData: record.License.ContainsUserPrivateData,
            IsQuarantined: record.Quarantine.IsQuarantined,
            QuarantineReason: record.Quarantine.Reason,
            BlockReasons: [],
            TrainingRecord: record);
    }
}

public sealed record DesignConversionLocalArtifactDbRecord(
    string RecordId,
    string DatabaseKind,
    string JobId,
    string RequestId,
    string SourceKind,
    string Tier,
    IReadOnlyDictionary<string, string> SanitizedSourceRefs,
    DesignConversionConversionResultArtifact ConversionResult,
    IReadOnlyList<DesignConversionRenderArtifactDbRef> RenderArtifactRefs,
    DesignConversionVisualJudgeArtifactDbStatus VisualJudgeStatus,
    IReadOnlyList<DesignConversionLocalArtifactDbDpoCandidate> DpoCandidates,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata)
{
    private const string ServerOwnedAccountingReceiptVerifiedKey = "server_owned_accounting_receipt_verified";
    private const string ServerOwnedAccountingReceiptStatusKey = "server_owned_accounting_receipt_status";
    private const string ServerOwnedAccountingReceiptStatusVerifiedPersisted = "verified_persisted";
    private const string ServerOwnedAccountingReceiptStatusNotVerified = "not_verified";

    private static readonly IReadOnlyList<string> ServerOwnedAccountingReceiptKeys =
    [
        ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey
    ];

    public static DesignConversionLocalArtifactDbRecord FromEvaluation(
        DesignConversionInternalEvaluationResult evaluation,
        IReadOnlyList<DesignConversionDpoTrainingRecord> dpoCandidates,
        DateTimeOffset? createdAtUtc = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (evaluation is null)
            throw new ArgumentNullException(nameof(evaluation));

        var request = evaluation.SubmittedRequest;
        var sourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["design_tree_payload_ref"] = request.DesignTreePayload.Sha256,
            ["original_tree_payload_ref"] = request.OriginalTreePayload.Sha256,
            ["source_manifest_ref"] = request.Provenance.TryGetValue("source_manifest_ref", out var sourceManifestRef)
                ? sourceManifestRef
                : request.DesignTreePayload.PayloadId
        };

        foreach (var artifact in request.Screenshots.Concat(request.DesignRenders))
            sourceRefs[$"source_artifact:{artifact.ArtifactId}"] = artifact.Sha256;

        var visualJudge = evaluation.VisualJudgeResults.LastOrDefault();
        var status = visualJudge is null
            ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
            : visualJudge.Provider.Metadata.TryGetValue(
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                out var testProviderOnly)
                && IsTruthy(testProviderOnly)
                    ? DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker
                    : visualJudge.Status;

        return new DesignConversionLocalArtifactDbRecord(
            RecordId: $"{evaluation.JobId}:storage_training_artifact_db",
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: evaluation.JobId,
            RequestId: request.RequestId,
            SourceKind: request.SourceKind.ToString(),
            Tier: request.Budget.Tier,
            SanitizedSourceRefs: sourceRefs,
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: evaluation.RuntimeFlowResult.Output.OutputId,
                HtmlRef: evaluation.ImprovedHtml,
                HtmlSha256: string.Empty,
                CssRef: evaluation.ImprovedCss,
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>
                {
                    ["conversion_result"] = "html_css_result_refs_or_hashes"
                }),
            RenderArtifactRefs: evaluation.RenderResults.Select(ToRenderArtifactRef).ToArray(),
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: status,
                ProviderBoundary: evaluation.VisualQaBoundaryTrace.VisualJudgeProviderBoundary,
                ExternalCallExecuted: evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted,
                EstimatedCostUsd: visualJudge?.EstimatedCostUsd,
                TierDecision: evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason,
                EvidenceRefs: evaluation.VisualQaBoundaryTrace.EvidenceRefs
                    .Concat(visualJudge?.EvidenceRefs ?? [])
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Metadata: BuildVisualJudgeDbMetadata(evaluation, visualJudge, status)),
            DpoCandidates: dpoCandidates.Select(record =>
                DesignConversionLocalArtifactDbDpoCandidate.FromTrainingRecord(record)).ToArray(),
            CreatedAtUtc: createdAtUtc ?? DateTimeOffset.UtcNow,
            EvidenceRefs: evaluation.EvidenceRefs
                .Append(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker)
                .Append(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind)
                .Append("conversion_result")
                .Append("render_artifact_refs")
                .Append("visual_judge_status")
                .Append("dpo_candidate")
                .Append("training_candidate_only")
                .Append("training_grade=false")
                .Append("not_training_grade=true")
                .Append("product_not_ready")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Metadata: metadata ?? new Dictionary<string, string>());
    }

    private static IReadOnlyDictionary<string, string> BuildVisualJudgeDbMetadata(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionVisualJudgeResult? visualJudge,
        string status)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["visual_judge_status"] = status,
            ["visual_decision"] = evaluation.VisualQaBoundaryTrace.VisualDecision.Decision,
            ["visual_decision_correction_count"] = evaluation.VisualQaBoundaryTrace.VisualDecision.CorrectionFindings.Count.ToString(CultureInfo.InvariantCulture),
            ["visual_decision_remaining_budget_usd"] = evaluation.VisualQaBoundaryTrace.VisualDecision.RemainingBudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["product_not_ready"] = bool.TrueString,
            [ServerOwnedAccountingReceiptVerifiedKey] = bool.FalseString,
            [ServerOwnedAccountingReceiptStatusKey] = ServerOwnedAccountingReceiptStatusNotVerified
        };

        if (TryBuildVerifiedServerOwnedAccountingReceipt(visualJudge, out var receipt))
        {
            foreach (var pair in receipt)
                metadata[pair.Key] = pair.Value;

            metadata[ServerOwnedAccountingReceiptVerifiedKey] = bool.TrueString;
            metadata[ServerOwnedAccountingReceiptStatusKey] = ServerOwnedAccountingReceiptStatusVerifiedPersisted;
        }

        return metadata;
    }

    private static bool TryBuildVerifiedServerOwnedAccountingReceipt(
        DesignConversionVisualJudgeResult? visualJudge,
        out IReadOnlyDictionary<string, string> receipt)
    {
        receipt = new Dictionary<string, string>(StringComparer.Ordinal);
        if (visualJudge is null)
            return false;

        var resultMetadata = visualJudge.Metadata ?? new Dictionary<string, string>();
        var providerMetadata = visualJudge.Provider.Metadata ?? new Dictionary<string, string>();
        var verified = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in ServerOwnedAccountingReceiptKeys)
        {
            if (!resultMetadata.TryGetValue(key, out var resultValue) ||
                !providerMetadata.TryGetValue(key, out var providerValue) ||
                !string.Equals(resultValue, providerValue, StringComparison.Ordinal))
            {
                return false;
            }

            verified[key] = resultValue;
        }

        receipt = verified;
        return true;
    }

    private static DesignConversionRenderArtifactDbRef ToRenderArtifactRef(DesignConversionRenderResult render)
        => new(
            ViewportId: render.Image.Resolution.ResolutionId,
            ArtifactId: render.RenderResultId,
            Kind: render.Image.Label,
            UriRef: render.Image.Uri,
            Sha256: render.Image.Sha256,
            ByteLength: 0,
            Width: render.Image.Resolution.Width,
            Height: render.Image.Resolution.Height,
            Metadata: render.Image.Metadata);

    private static bool IsTruthy(string value)
        => string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "1", StringComparison.Ordinal)
           || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
}

public sealed record DesignConversionLocalArtifactDbIndexRecordLocator(
    string RecordId,
    string JobId,
    string RequestId,
    string SourceKind,
    DateTimeOffset CreatedAtUtc,
    long ByteOffset,
    long ByteLength,
    int LineNumber,
    string AuthorityLineSha256,
    string ImmutableAppendFingerprint);

public sealed record DesignConversionLocalArtifactDbQuarantinedAuthorityLine(
    int PhysicalLineNumber,
    long ByteOffset,
    long ByteLength,
    string AuthorityLineSha256);

public sealed record DesignConversionLocalArtifactDbIndex(
    string DatabaseKind,
    string SchemaVersion,
    int RecordCount,
    int ApprovedTrainingCandidateCount,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<string> EvidenceMarkers)
{
    public string? RootDirectoryBindingHash { get; init; }

    public string? TenantBindingHash { get; init; }

    public string? RecordsFileName { get; init; }

    public long RecordsFileLength { get; init; }

    public long RecordsFileLastWriteTimeUtcTicks { get; init; }

    public IReadOnlyList<DesignConversionLocalArtifactDbIndexRecordLocator> RecordLocators { get; init; } = [];

    public IReadOnlyList<DesignConversionLocalArtifactDbQuarantinedAuthorityLine> QuarantinedAuthorityLines { get; init; } = [];
}

public sealed record DesignConversionDpoCandidateApprovalResult(
    string LocalArtifactRecordId,
    string CandidateId,
    string TrainingRecordId,
    string UnitId,
    double UsefulnessScore,
    string ApprovalStatus,
    string ReviewStatus,
    bool ApprovedForTraining,
    string ApprovedByRef,
    string ApprovalReason,
    DateTimeOffset ApprovedAtUtc,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionApprovedDpoHandoffRow(
    string LocalArtifactRecordId,
    string CandidateId,
    string TrainingRecordId,
    string UnitId,
    double UsefulnessScore,
    string ApprovalStatus,
    string ReviewStatus,
    bool ApprovedForTraining,
    bool TrainingHandoffOnly,
    string ApprovedByRef,
    string ApprovalReason,
    string ApprovedAtUtc,
    string SourceManifestRef,
    string RecordHash,
    string InternalUnitStateHash,
    string SourceStructureHash,
    string ChosenPayloadHash,
    string RejectedPayloadHash,
    string TieBreakerHash,
    string ChosenHtmlRef,
    string ChosenCssRef,
    string RejectedHtmlRef,
    string RejectedCssRef,
    string DesignTreeRef,
    string OriginalTreeRef,
    IReadOnlyList<string> SourceArtifactRefs,
    IReadOnlyList<string> RenderArtifactRefs,
    IReadOnlyList<string> ComparisonArtifactRefs,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionApprovedDpoHandoffArtifact(
    string ArtifactId,
    string ArtifactRef,
    string SchemaVersion,
    DateTimeOffset ExportedAtUtc,
    int ApprovedCandidateCount,
    IReadOnlyList<DesignConversionApprovedDpoHandoffRow> Rows,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionTrainingIntakeCandidateRef(
    string LocalArtifactRecordId,
    string CandidateId,
    string TrainingRecordId,
    string UnitId,
    double UsefulnessScore,
    string HandoffRecordHash,
    string SourceManifestRef,
    string SourceManifestHash,
    string SourceIdentityHash,
    string InternalUnitStateHash,
    string SourceStructureHash,
    string ChosenPayloadHash,
    string RejectedPayloadHash,
    string TieBreakerHash,
    string ChosenHtmlRef,
    string ChosenCssRef,
    string RejectedHtmlRef,
    string RejectedCssRef,
    string DesignTreeRef,
    string OriginalTreeRef,
    IReadOnlyList<string> SourceArtifactRefs,
    IReadOnlyList<string> RenderArtifactRefs,
    IReadOnlyList<string> ComparisonArtifactRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionTrainingIntakeRecord(
    string IntakeId,
    string IntakeRef,
    string SourceHandoffArtifactId,
    string SourceHandoffArtifactRef,
    int ApprovedCandidateCount,
    int AcceptedCandidateCount,
    IReadOnlyList<DesignConversionTrainingIntakeCandidateRef> CandidateRefs,
    DateTimeOffset RequestedAtUtc,
    string RequestedByRef,
    string RequestReason,
    bool TrainingIntakeOnly,
    bool TrainingNotStarted,
    bool NotTrained,
    bool WeightsNotProduced,
    bool CheckpointNotMutated,
    bool ModelUploadNotReady,
    bool ProductNotReady,
    bool HeldOutNotProven,
    bool SourceDisjointNotProven,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionTrainingIntakePreflightResult(
    string PreflightId,
    string TrainingIntakeId,
    int AcceptedCandidateCount,
    bool MayStartTraining,
    string SourceManifestStatus,
    string SourceDisjointStatus,
    bool TrainingStarted,
    bool CheckpointMutated,
    bool WeightsProduced,
    bool ModelUploadReady,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionTrainingIntakeRunnerEvidence(
    string BoundaryDecisionKind,
    bool DatasetCountGatePassed,
    int RequiredTrainRecords,
    int RequiredHeldOutRecords,
    int ActualTrainRecords,
    int ActualHeldOutRecords,
    bool SourceDisjoint,
    string SourceManifestStatus,
    string SourceDisjointStatus,
    bool MachineLifecycleHookAttempted,
    bool TrainingStarted,
    string TrainingExecutionStatus,
    string RunnerBlockedReason,
    int LocalLifecycleBatchResultCount,
    string CheckpointCandidateStatus,
    bool CheckpointCandidateCreated,
    string CheckpointMutationStatus,
    bool CheckpointMutationEvidenceProven,
    bool CheckpointContainsRealTrainedWeights,
    string WeightProofStatus,
    string FreshLoadStatus,
    string ContinueTrainingStatus,
    string MeasurementGateStatus,
    string PromotionStatus,
    bool PromotionUploadAllowed,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> FailureReasons,
    IReadOnlyList<string> Warnings);

public sealed record DesignConversionTrainingIntakeRunResult(
    string RunId,
    string TrainingIntakeId,
    string PreflightId,
    int AcceptedCandidateCount,
    int ResolvedTrainingRecordCount,
    bool MayStartTraining,
    string SourceManifestStatus,
    string SourceDisjointStatus,
    bool TrainingRunnerInvoked,
    bool LocalLifecycleHookAttempted,
    bool TrainingStarted,
    bool CheckpointMutated,
    bool CheckpointCandidateCreated,
    bool WeightsProduced,
    bool ModelUploadReady,
    string TrainingExecutionBoundary,
    string TrainingExecutionStatus,
    string RunnerBlockedReason,
    string CheckpointStatus,
    string CheckpointMutationStatus,
    string WeightProofStatus,
    string MeasurementGateStatus,
    string PromotionGateStatus,
    string UploadReadinessStatus,
    IReadOnlyList<string> PromotionBlockedReasons,
    DesignConversionTrainingIntakeRunnerEvidence? RunnerEvidence,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionTrainingIntakeLocalPromotionResult(
    DesignConversionTrainingIntakeRunResult TrainingRun,
    bool LocalPromotionReady,
    bool ExternalCloudUploadAttempted,
    string PromotionOutputDirectory,
    string PromotedCheckpointPath,
    DesignConversionPromotionPublicationResult? Publication,
    DesignConversionPromotedLocalModel? PromotedLocalModel,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionTrainingIntakeLocalPromotionRollbackResult(
    string RollbackId,
    bool RollbackApplied,
    bool PriorStateRestored,
    bool NoPriorStateRecorded,
    string RolledBackLocalModelRef,
    string PreviousActiveLocalModelRef,
    string RestoredActiveLocalModelRef,
    string RestoredActiveCheckpointSha256,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public interface IDesignConversionLocalArtifactDb
{
    Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
        DesignConversionLocalArtifactDbRecord record,
        CancellationToken ct = default);

    Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
        DesignConversionLocalArtifactDbQuery? query = null,
        CancellationToken ct = default);

    Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
        DesignConversionLocalArtifactDbQuery? query = null,
        CancellationToken ct = default);

    Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
        DesignConversionLocalArtifactDbQuery? query = null,
        DateTimeOffset? exportedAtUtc = null,
        CancellationToken ct = default);

    Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
        DesignConversionApprovedDpoHandoffArtifact handoff,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null);

    Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
        CancellationToken ct = default);

    Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
        string intakeId,
        CancellationToken ct = default);

    Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
        string intakeId,
        DesignConversionTrainingPipelineOptions? options = null,
        CancellationToken ct = default,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null);

    Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
        string intakeId,
        string promotionOutputDirectory,
        DesignConversionTrainingPipelineOptions? options = null,
        CancellationToken ct = default);

    Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
        string promotedLocalModelRef,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default);

    Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
        string localArtifactRecordId,
        string candidateOrTrainingRecordId,
        string reviewerRef,
        string reason,
        DateTimeOffset? approvedAtUtc = null,
        CancellationToken ct = default);

    Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
        DesignConversionDpoDisplayQuery? query = null,
        CancellationToken ct = default);
}

public static class DesignConversionTrainingIntakeRouteAdmissionContext
{
    private static readonly System.Threading.AsyncLocal<IReadOnlyDictionary<string, string>?> CurrentMetadata = new();

    public static IReadOnlyDictionary<string, string>? Current => CurrentMetadata.Value;

    public static async Task<T> RunAsync<T>(
        IReadOnlyDictionary<string, string>? metadata,
        Func<Task<T>> action)
    {
        var previous = CurrentMetadata.Value;
        CurrentMetadata.Value = metadata is null
            ? null
            : new Dictionary<string, string>(metadata, StringComparer.Ordinal);

        try
        {
            return await action();
        }
        finally
        {
            CurrentMetadata.Value = previous;
        }
    }
}

public interface IDesignConversionLocalPromotionActivationAuthority
{
    string AllowedArtifactRootDirectory { get; }

    Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>> ListTrainingIntakeLocalPromotionRecordsAsync(
        CancellationToken ct = default);
}

public sealed class JsonlDesignConversionLocalArtifactDb :
    IDesignConversionLocalArtifactDb,
    IDesignConversionLocalPromotionActivationAuthority
{
    public const string StorageTrainingArtifactDbMarker = "storage_training_artifact_db";
    public const string LocalArtifactDbKind = "local_artifact_db";
    public const string ConversionResultMarker = "conversion_result";
    public const string RenderArtifactRefsMarker = "render_artifact_refs";
    public const string VisualJudgeStatusMarker = "visual_judge_status";
    public const string DpoCandidateMarker = "dpo_candidate";
    public const string UsefulnessScoreMarker = "usefulness_score";
    public const string QuarantineMarker = "quarantine";
    public const string TrainingCandidateOnlyMarker = "training_candidate_only";
    public const string ProductNotReadyMarker = "product_not_ready";
    public const string RuntimeCandidateReviewPendingStatus = "runtime-candidate-review-pending";
    public const string RuntimeCandidateReviewApprovedStatus = "runtime-candidate-review-approved";
    public const string ApprovedDpoHandoffSchemaVersion = "design-conversion-approved-dpo-handoff-v1";
    public const string TrainingIntakeSchemaVersion = "design-conversion-training-intake-v1";
    public const string TrainingIntakePreflightSchemaVersion = "design-conversion-training-intake-preflight-v1";
    public const string TrainingIntakeRunSchemaVersion = "design-conversion-training-intake-run-v1";
    public const string TrainingIntakeLocalPromotionSchemaVersion = "design-conversion-training-intake-local-promotion-v1";
    public const string TrainingIntakeLocalPromotionRollbackSchemaVersion = "design-conversion-training-intake-local-promotion-rollback-v1";
    public const string InvalidUtf8ErrorCode = "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_INVALID_UTF8";
    public const string LocalArtifactDbWriteFailedErrorCode = "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_WRITE_FAILED";
    public const string LocalArtifactDbWriteFailedSafeMessage = "DesignConversion local artifact DB record could not be written.";
    public const string TrainingIntakeReadFailedErrorCode = "DESIGN_CONVERSION_TRAINING_INTAKE_READ_FAILED";
    public const string TrainingIntakeReadFailedSafeMessage = "DesignConversion training intake requests could not be read.";
    public const string TrainingIntakeRunReadFailedErrorCode = "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_READ_FAILED";
    public const string TrainingIntakeRunReadFailedSafeMessage = "DesignConversion training intake runs could not be read safely.";
    public const string TrainingIntakeRequestsQuotaExceededErrorCode = "DESIGN_CONVERSION_TRAINING_INTAKE_REQUESTS_QUOTA_EXCEEDED";
    public const string TrainingIntakeRequestsQuotaExceededSafeMessage = "DesignConversion training intake request journal quota is exhausted.";
    public const string TrainingIntakeRunsQuotaExceededErrorCode = "DESIGN_CONVERSION_TRAINING_INTAKE_RUNS_QUOTA_EXCEEDED";
    public const string TrainingIntakeRunsQuotaExceededSafeMessage = "DesignConversion training intake run journal quota is exhausted.";
    public const string TrainingIntakeLocalPromotionsQuotaExceededErrorCode =
        "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTIONS_QUOTA_EXCEEDED";
    public const string TrainingIntakeLocalPromotionsQuotaExceededSafeMessage =
        "DesignConversion training intake local promotion journal quota is exhausted.";
    public const string LocalArtifactDbAuthorityQuotaExceededErrorCode = "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_AUTHORITY_QUOTA_EXCEEDED";
    public const string LocalArtifactDbAuthorityQuotaExceededSafeMessage = "DesignConversion local artifact DB authority storage quota is exhausted.";
    public const string LocalArtifactDbRecordIdConflictErrorCode = "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_RECORD_ID_CONFLICT";
    public const string LocalArtifactDbRecordIdConflictSafeMessage = "DesignConversion local artifact DB record id conflicts with an existing immutable record.";
    public const string ImmutableAppendFingerprintMetadataKey = "local_artifact_db_immutable_append_fingerprint_sha256";
    public const string AuthorityRecordCommittedMarker = "local_artifact_db:authority_record_committed";
    public const string DerivedIndexUpdateDeferredMarker = "local_artifact_db:derived_index_update_deferred";
    public const string DerivedIndexDurableFalseMarker = "local_artifact_db:derived_index_durable:false";
    public const string UncommittedFinalRecordRecoveredMarker = "local_artifact_db:uncommitted_final_record_recovered";
    public const string UncommittedFinalRecordNotCommittedMarker = "local_artifact_db:uncommitted_final_record_not_committed";
    public const string LogicalQuarantinedAuthorityLineMarker = "local_artifact_db:authority_line_logically_quarantined";
    public const string QuarantinedAuthorityBytesPreservedMarker = "local_artifact_db:quarantined_authority_bytes_preserved";
    public const int MaxLogicallyQuarantinedAuthorityLinesPerScan = 64;
    public const string LocalArtifactDbAuthorityLogicalQuarantineLimitExceededErrorCode =
        "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_AUTHORITY_LOGICAL_QUARANTINE_LIMIT_EXCEEDED";

    private const string SchemaVersion = "design-conversion-local-artifact-db-v1";
    private const string TenantBindingNotApplicable = "tenant:not_applicable";
    private const string TrainingDatabaseName = "design_conversion_special_training_artifact_db";
    private const string TrainingDatabaseRetentionClass = "local_refs_only_training_trace";
    private const string TrainingDatabaseRetentionPolicy = "retain_manifest_checksums_and_refs_only_no_raw_payloads";
    private const string TrainingDatabaseRawPayloadRedactionMarker = "raw_html_css_design_tree_provider_credentials_not_persisted";
    private const string RecordsFileName = "conversion-results.jsonl";
    private const string RecordsMutationLockFileName = RecordsFileName + ".mutation.lock";
    private const int RecordsMutationLockRetryDelayMs = 25;
    private const string IndexFileName = "conversion-results.index.json";
    private const int MaxIndexedRecordLineByteLength = 1024 * 1024;
    public const int MaxTrainingIntakeRunJournalRowByteLength = 1024 * 1024;
    public const int MaxTrainingIntakeLocalPromotionJournalRowByteLength = 1024 * 1024;
    public const long DefaultRecordsAuthorityMaxBytes = 1L * 1024 * 1024 * 1024;
    public const long DefaultTrainingIntakeRequestsMaxBytes = 64L * 1024 * 1024;
    // The run journal reserves one established 1 MiB framed JSONL row before work starts;
    // 64 MiB keeps room for at least 64 maximum rows before local rotation is required.
    public const long DefaultTrainingIntakeRunsMaxBytes = 64L * MaxTrainingIntakeRunJournalRowByteLength;
    // The local-promotion journal mirrors the run journal reservation envelope for idempotent appends.
    public const long DefaultTrainingIntakeLocalPromotionsMaxBytes =
        64L * MaxTrainingIntakeLocalPromotionJournalRowByteLength;
    private const string TrainingIntakeFileName = "training-intake-requests.jsonl";
    private const string TrainingIntakeMutationLockFileName = TrainingIntakeFileName + ".mutation.lock";
    private const int TrainingIntakeMutationLockRetryDelayMs = 25;
    private const string TrainingIntakeRunFileName = "training-intake-runs.jsonl";
    private const string TrainingIntakeRunMutationLockFileName = TrainingIntakeRunFileName + ".mutation.lock";
    private const int TrainingIntakeRunMutationLockRetryDelayMs = 25;
    public const string TrainingIntakeLocalPromotionFileName = "training-intake-local-promotions.jsonl";
    private const string TrainingIntakeLocalPromotionMutationLockFileName = TrainingIntakeLocalPromotionFileName + ".mutation.lock";
    private const int TrainingIntakeLocalPromotionMutationLockRetryDelayMs = 25;

    private static readonly JsonSerializerOptions JsonOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions IndexJsonOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = true
    };

    private static readonly Encoding StrictUtf8Encoding = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly ConcurrentDictionary<string, RecordsMutationContext> RecordsMutationContexts = new(GetPathKeyComparer());

    private sealed class RecordsMutationContext
    {
        public SemaphoreSlim Gate { get; } = new(1, 1);

        public SemaphoreSlim CreateTrainingIntakeJournalGate { get; } = new(1, 1);

        public SemaphoreSlim RunTrainingIntakeGate { get; } = new(1, 1);

        public SemaphoreSlim RunTrainingIntakeLocalPromotionGate { get; } = new(1, 1);

        public CachedAuthorityState? CachedAuthorityState { get; set; }
    }

    private sealed record AuthorityRecordIdState(
        int Count,
        string CurrentRecordJson,
        string? ImmutableAppendFingerprint);

    private sealed class AuthorityState
    {
        private readonly GateOwnedRecordLocators _recordLocators;

        public AuthorityState(
            DesignConversionLocalArtifactDbIndex index,
            long committedLength,
            int nextLineNumber,
            RecordsFileFingerprint fingerprint,
            bool hasUncommittedFinalRecord,
            int? uncommittedFinalLineNumber,
            long? uncommittedFinalByteOffset,
            Dictionary<string, AuthorityRecordIdState> recordsById)
        {
            _recordLocators = new GateOwnedRecordLocators(index.RecordLocators);
            Index = index with { RecordLocators = _recordLocators };
            CommittedLength = committedLength;
            NextLineNumber = nextLineNumber;
            Fingerprint = fingerprint;
            HasUncommittedFinalRecord = hasUncommittedFinalRecord;
            UncommittedFinalLineNumber = uncommittedFinalLineNumber;
            UncommittedFinalByteOffset = uncommittedFinalByteOffset;
            RecordsById = recordsById;
        }

        public DesignConversionLocalArtifactDbIndex Index { get; private set; }

        public long CommittedLength { get; private set; }

        public RecordsFileFingerprint Fingerprint { get; private set; }

        public bool HasUncommittedFinalRecord { get; private set; }

        public int? UncommittedFinalLineNumber { get; private set; }

        public long? UncommittedFinalByteOffset { get; private set; }

        public Dictionary<string, AuthorityRecordIdState> RecordsById { get; }

        public int NextLineNumber { get; private set; }

        public void AddCommittedRecord(
            string recordId,
            AuthorityRecordIdState recordState,
            DesignConversionLocalArtifactDbIndexRecordLocator locator,
            int approvedCandidateCount,
            DateTimeOffset updatedAtUtc,
            RecordsFileFingerprint fingerprint,
            long committedLength,
            string rootDirectoryBindingHash)
        {
            var baseIndex = WithoutUncommittedFinalRecordIndexEvidence(Index);
            var nextIndex = baseIndex with
            {
                RecordCount = baseIndex.RecordCount + 1,
                ApprovedTrainingCandidateCount = baseIndex.ApprovedTrainingCandidateCount + approvedCandidateCount,
                UpdatedAtUtc = updatedAtUtc,
                RootDirectoryBindingHash = rootDirectoryBindingHash,
                TenantBindingHash = TenantBindingNotApplicable,
                RecordsFileName = RecordsFileName,
                RecordsFileLength = committedLength,
                RecordsFileLastWriteTimeUtcTicks = fingerprint.Exists ? fingerprint.LastWriteTimeUtc.Ticks : DateTime.MinValue.Ticks,
                RecordLocators = _recordLocators
            };

            _recordLocators.EnsureCapacity(_recordLocators.Count + 1);
            RecordsById.EnsureCapacity(RecordsById.Count + 1);

            RecordsById[recordId] = recordState;
            _recordLocators.Add(locator);
            Index = nextIndex;
            CommittedLength = committedLength;
            NextLineNumber++;
            Fingerprint = fingerprint;
            HasUncommittedFinalRecord = false;
            UncommittedFinalLineNumber = null;
            UncommittedFinalByteOffset = null;
        }
    }

    private sealed class GateOwnedRecordLocators : IReadOnlyList<DesignConversionLocalArtifactDbIndexRecordLocator>
    {
        private readonly List<DesignConversionLocalArtifactDbIndexRecordLocator> _locators;

        // Gate-owned: RecordsMutationContext.Gate serializes warm mutation and index serialization for this view.
        public GateOwnedRecordLocators(IEnumerable<DesignConversionLocalArtifactDbIndexRecordLocator>? locators)
            => _locators = new List<DesignConversionLocalArtifactDbIndexRecordLocator>(locators ?? []);

        public int Count => _locators.Count;

        public int NextLineNumber => _locators.Count == 0 ? 1 : _locators[^1].LineNumber + 1;

        public DesignConversionLocalArtifactDbIndexRecordLocator this[int index] => _locators[index];

        public void Add(DesignConversionLocalArtifactDbIndexRecordLocator locator)
            => _locators.Add(locator);

        public void EnsureCapacity(int capacity)
            => _locators.EnsureCapacity(capacity);

        public IEnumerator<DesignConversionLocalArtifactDbIndexRecordLocator> GetEnumerator()
            => _locators.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed record CachedAuthorityState(
        AuthorityState State,
        RecordsFileFingerprint Fingerprint);

    private sealed record AuthorityRecordLine(
        DesignConversionLocalArtifactDbRecord Record,
        string CurrentRecordJson,
        long ByteOffset,
        long ByteLength,
        int LineNumber,
        string AuthorityLineSha256);

    private sealed record AuthorityLineParseResult(
        AuthorityRecordLine? RecordLine,
        DesignConversionLocalArtifactDbQuarantinedAuthorityLine? QuarantinedLine);

    private sealed record AuthorityRecordScan(
        IReadOnlyList<AuthorityRecordLine> Lines,
        IReadOnlyList<DesignConversionLocalArtifactDbQuarantinedAuthorityLine> QuarantinedLines,
        RecordsFileFingerprint Fingerprint,
        long CommittedLength,
        int NextAppendLineNumber,
        bool HasUncommittedFinalRecord,
        int? UncommittedFinalLineNumber,
        long? UncommittedFinalByteOffset);

    private sealed record DurableIndexLookupResult(
        bool UsedDurableIndex,
        bool RequiresDurableIndexHeal,
        OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>> Result);

    private sealed record IndexedCandidateReadResult(
        bool IsTrusted,
        IReadOnlyList<DesignConversionLocalArtifactDbRecord> Records);

    private sealed record RecordIdReplayResolution(
        bool IsReplay,
        DesignConversionLocalArtifactDbRecord? Record);

    private readonly record struct RecordsFileFingerprint(
        bool Exists,
        long Length,
        DateTime LastWriteTimeUtc);

    private readonly string _rootDirectory;
    private readonly string _recordsPath;
    private readonly string _recordsMutationLockPath;
    private readonly string _indexPath;
    private readonly string _trainingIntakePath;
    private readonly string _trainingIntakeMutationLockPath;
    private readonly string _trainingIntakeRunPath;
    private readonly string _trainingIntakeRunMutationLockPath;
    private readonly string _trainingIntakeLocalPromotionPath;
    private readonly string _trainingIntakeLocalPromotionMutationLockPath;
    private readonly RecordsMutationContext _recordsMutationContext;
    private readonly IDesignConversionPromotionUploader? _promotionUploader;
    private readonly Action<bool>? _createTrainingIntakeJournalWaitTaskObserverForTesting;
    private readonly Func<CancellationToken, Task>? _createTrainingIntakeJournalGateAcquiredCallbackForTesting;
    private readonly Action<bool>? _runTrainingIntakeGateWaitTaskObserverForTesting;
    private readonly Func<CancellationToken, Task>? _runTrainingIntakeGateAcquiredCallbackForTesting;
    private readonly Action? _runTrainingIntakeRunnerBeforeInvokeObserverForTesting;
    private readonly Action? _runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting;
    private readonly Action<bool>? _runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting;
    private readonly Func<CancellationToken, Task>? _runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting;
    private readonly Func<CancellationToken, Task>? _authorityAppendBeforeOpenCallbackForTesting;
    private readonly long _recordsAuthorityMaxBytes;
    private readonly long _trainingIntakeRequestsMaxBytes;
    private readonly long _trainingIntakeRunsMaxBytes;
    private readonly long _trainingIntakeLocalPromotionsMaxBytes;

    internal bool LastSelectiveListRecordsUsedDurableIndex { get; private set; }
    public long RecordsAuthorityMaxBytes => _recordsAuthorityMaxBytes;
    public long TrainingIntakeRequestsMaxBytes => _trainingIntakeRequestsMaxBytes;
    public long TrainingIntakeRunsMaxBytes => _trainingIntakeRunsMaxBytes;
    public long TrainingIntakeLocalPromotionsMaxBytes => _trainingIntakeLocalPromotionsMaxBytes;
    public string AllowedArtifactRootDirectory => _rootDirectory;

    public JsonlDesignConversionLocalArtifactDb(
        string rootDirectory,
        IDesignConversionPromotionUploader? promotionUploader = null,
        long recordsAuthorityMaxBytes = DefaultRecordsAuthorityMaxBytes,
        long trainingIntakeRequestsMaxBytes = DefaultTrainingIntakeRequestsMaxBytes)
        : this(
            rootDirectory,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeGateWaitTaskObserverForTesting: null,
            runTrainingIntakeGateAcquiredCallbackForTesting: null,
            promotionUploader: promotionUploader,
            runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: null,
            runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: null,
            authorityAppendBeforeOpenCallbackForTesting: null,
            recordsAuthorityMaxBytes: recordsAuthorityMaxBytes,
            trainingIntakeRequestsMaxBytes: trainingIntakeRequestsMaxBytes,
            trainingIntakeRunsMaxBytes: DefaultTrainingIntakeRunsMaxBytes,
            runTrainingIntakeRunnerBeforeInvokeObserverForTesting: null,
            trainingIntakeLocalPromotionsMaxBytes: DefaultTrainingIntakeLocalPromotionsMaxBytes,
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting: null)
    {
    }

    public JsonlDesignConversionLocalArtifactDb(
        string rootDirectory,
        IDesignConversionPromotionUploader? promotionUploader,
        long recordsAuthorityMaxBytes,
        long trainingIntakeRequestsMaxBytes,
        long trainingIntakeRunsMaxBytes)
        : this(
            rootDirectory,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeGateWaitTaskObserverForTesting: null,
            runTrainingIntakeGateAcquiredCallbackForTesting: null,
            promotionUploader: promotionUploader,
            runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: null,
            runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: null,
            authorityAppendBeforeOpenCallbackForTesting: null,
            recordsAuthorityMaxBytes: recordsAuthorityMaxBytes,
            trainingIntakeRequestsMaxBytes: trainingIntakeRequestsMaxBytes,
            trainingIntakeRunsMaxBytes: trainingIntakeRunsMaxBytes,
            runTrainingIntakeRunnerBeforeInvokeObserverForTesting: null,
            trainingIntakeLocalPromotionsMaxBytes: DefaultTrainingIntakeLocalPromotionsMaxBytes,
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting: null)
    {
    }

    public JsonlDesignConversionLocalArtifactDb(
        string rootDirectory,
        IDesignConversionPromotionUploader? promotionUploader,
        long recordsAuthorityMaxBytes,
        long trainingIntakeRequestsMaxBytes,
        long trainingIntakeRunsMaxBytes,
        long trainingIntakeLocalPromotionsMaxBytes)
        : this(
            rootDirectory,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeGateWaitTaskObserverForTesting: null,
            runTrainingIntakeGateAcquiredCallbackForTesting: null,
            promotionUploader: promotionUploader,
            runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting: null,
            runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting: null,
            authorityAppendBeforeOpenCallbackForTesting: null,
            recordsAuthorityMaxBytes: recordsAuthorityMaxBytes,
            trainingIntakeRequestsMaxBytes: trainingIntakeRequestsMaxBytes,
            trainingIntakeRunsMaxBytes: trainingIntakeRunsMaxBytes,
            runTrainingIntakeRunnerBeforeInvokeObserverForTesting: null,
            trainingIntakeLocalPromotionsMaxBytes: trainingIntakeLocalPromotionsMaxBytes,
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting: null)
    {
    }

    internal JsonlDesignConversionLocalArtifactDb(
        string rootDirectory,
        Action<bool>? createTrainingIntakeJournalWaitTaskObserverForTesting,
        Func<CancellationToken, Task>? createTrainingIntakeJournalGateAcquiredCallbackForTesting,
        Action<bool>? runTrainingIntakeGateWaitTaskObserverForTesting = null,
        Func<CancellationToken, Task>? runTrainingIntakeGateAcquiredCallbackForTesting = null,
        IDesignConversionPromotionUploader? promotionUploader = null,
        Action<bool>? runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting = null,
        Func<CancellationToken, Task>? runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting = null,
        Func<CancellationToken, Task>? authorityAppendBeforeOpenCallbackForTesting = null,
        long recordsAuthorityMaxBytes = DefaultRecordsAuthorityMaxBytes,
        long trainingIntakeRequestsMaxBytes = DefaultTrainingIntakeRequestsMaxBytes,
        long trainingIntakeRunsMaxBytes = DefaultTrainingIntakeRunsMaxBytes,
        Action? runTrainingIntakeRunnerBeforeInvokeObserverForTesting = null,
        long trainingIntakeLocalPromotionsMaxBytes = DefaultTrainingIntakeLocalPromotionsMaxBytes,
        Action? runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting = null)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A local artifact DB root is required.", nameof(rootDirectory));
        if (recordsAuthorityMaxBytes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(recordsAuthorityMaxBytes),
                recordsAuthorityMaxBytes,
                "The records authority byte limit must be positive.");
        if (trainingIntakeRequestsMaxBytes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(trainingIntakeRequestsMaxBytes),
                trainingIntakeRequestsMaxBytes,
                "The training intake request journal byte limit must be positive.");
        if (trainingIntakeRunsMaxBytes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(trainingIntakeRunsMaxBytes),
                trainingIntakeRunsMaxBytes,
                "The training intake run journal byte limit must be positive.");
        if (trainingIntakeLocalPromotionsMaxBytes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(trainingIntakeLocalPromotionsMaxBytes),
                trainingIntakeLocalPromotionsMaxBytes,
                "The training intake local promotion journal byte limit must be positive.");

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _recordsPath = Path.Combine(_rootDirectory, RecordsFileName);
        _recordsMutationLockPath = Path.Combine(_rootDirectory, RecordsMutationLockFileName);
        _indexPath = Path.Combine(_rootDirectory, IndexFileName);
        _trainingIntakePath = Path.Combine(_rootDirectory, TrainingIntakeFileName);
        _trainingIntakeMutationLockPath = Path.Combine(_rootDirectory, TrainingIntakeMutationLockFileName);
        _trainingIntakeRunPath = Path.Combine(_rootDirectory, TrainingIntakeRunFileName);
        _trainingIntakeRunMutationLockPath = Path.Combine(_rootDirectory, TrainingIntakeRunMutationLockFileName);
        _trainingIntakeLocalPromotionPath = Path.Combine(_rootDirectory, TrainingIntakeLocalPromotionFileName);
        _trainingIntakeLocalPromotionMutationLockPath = Path.Combine(
            _rootDirectory,
            TrainingIntakeLocalPromotionMutationLockFileName);
        _promotionUploader = promotionUploader;
        _createTrainingIntakeJournalWaitTaskObserverForTesting = createTrainingIntakeJournalWaitTaskObserverForTesting;
        _createTrainingIntakeJournalGateAcquiredCallbackForTesting = createTrainingIntakeJournalGateAcquiredCallbackForTesting;
        _runTrainingIntakeGateWaitTaskObserverForTesting = runTrainingIntakeGateWaitTaskObserverForTesting;
        _runTrainingIntakeGateAcquiredCallbackForTesting = runTrainingIntakeGateAcquiredCallbackForTesting;
        _runTrainingIntakeRunnerBeforeInvokeObserverForTesting = runTrainingIntakeRunnerBeforeInvokeObserverForTesting;
        _runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting =
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting;
        _runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting =
            runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting;
        _runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting =
            runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting;
        _authorityAppendBeforeOpenCallbackForTesting = authorityAppendBeforeOpenCallbackForTesting;
        _recordsAuthorityMaxBytes = recordsAuthorityMaxBytes;
        _trainingIntakeRequestsMaxBytes = trainingIntakeRequestsMaxBytes;
        _trainingIntakeRunsMaxBytes = trainingIntakeRunsMaxBytes;
        _trainingIntakeLocalPromotionsMaxBytes = trainingIntakeLocalPromotionsMaxBytes;
        _recordsMutationContext = RecordsMutationContexts.GetOrAdd(
            NormalizeRecordMutationGateKey(_recordsPath),
            _ => new RecordsMutationContext());
    }

    private static StringComparer GetPathKeyComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static string NormalizeRecordMutationGateKey(string recordsPath)
        => Path.GetFullPath(recordsPath);

    private static async Task WaitForObservedGateAsync(
        SemaphoreSlim gate,
        Action<bool>? waitTaskObserver,
        Action onGateAcquired,
        CancellationToken ct)
    {
        if (waitTaskObserver is null)
        {
            await gate.WaitAsync(ct);
            onGateAcquired();
            return;
        }

        using var waitObserverFailureCancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var waitTask = gate.WaitAsync(waitObserverFailureCancellation.Token);
        var waitCompletedSuccessfully = waitTask.IsCompletedSuccessfully;
        try
        {
            waitTaskObserver(waitCompletedSuccessfully);
        }
        catch
        {
            waitObserverFailureCancellation.Cancel();
            try
            {
                await waitTask;
                onGateAcquired();
            }
            catch (OperationCanceledException) when (waitObserverFailureCancellation.IsCancellationRequested)
            {
            }

            throw;
        }

        await waitTask;
        onGateAcquired();
    }

    public async Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
        DesignConversionLocalArtifactDbRecord record,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionLocalArtifactDbRecord>();
        if (record is null)
            return OperationResult<DesignConversionLocalArtifactDbRecord>.Fail(
                "DesignConversion local artifact DB record is required.",
                "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_RECORD_REQUIRED");

        var sanitized = NormalizeRecord(record);
        var line = JsonSerializer.Serialize(sanitized, JsonOptions);

        try
        {
            await _recordsMutationContext.Gate.WaitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionLocalArtifactDbRecord>();
        }

        try
        {
            await using var mutationLease = await AcquireRecordsAuthorityMutationLeaseAsync(ct);
            return await AppendRecordCoreAsync(sanitized, line, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionLocalArtifactDbRecord>();
        }
        catch (IOException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionLocalArtifactDbRecord>();
        }
        catch (UnauthorizedAccessException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionLocalArtifactDbRecord>();
        }
        finally
        {
            _recordsMutationContext.Gate.Release();
        }
    }

    private async Task<FileStream> AcquireRecordsAuthorityMutationLeaseAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(_rootDirectory);

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                return new FileStream(
                    _recordsMutationLockPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.Asynchronous);
            }
            catch (IOException ex) when (IsFileSharingOrLockViolation(ex))
            {
                await Task.Delay(RecordsMutationLockRetryDelayMs, ct);
            }
        }
    }

    private async Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordCoreAsync(
        DesignConversionLocalArtifactDbRecord sanitized,
        string line,
        CancellationToken ct)
    {
        var stateResult = await ResolveCurrentAuthorityStateForAppendAsync(ct);
        if (!stateResult.IsSuccess)
            return stateResult.MapError<DesignConversionLocalArtifactDbRecord>();

        var authorityState = stateResult.Value!;
        var replay = await ResolveRecordIdReplayAsync(sanitized, authorityState, ct);
        if (!replay.IsSuccess)
            return replay.MapError<DesignConversionLocalArtifactDbRecord>();
        if (replay.Value!.IsReplay)
            return OperationResult<DesignConversionLocalArtifactDbRecord>.Ok(replay.Value.Record!);

        var appendOffset = authorityState.CommittedLength;
        var appendedLineBytes = BuildAppendedAuthorityLineBytes(line, appendOffset);
        if (appendedLineBytes.LongLength > MaxIndexedRecordLineByteLength)
            return LocalArtifactDbWriteFailed<DesignConversionLocalArtifactDbRecord>();
        if (!TryProjectCommittedAuthorityLength(
                authorityState.CommittedLength,
                appendedLineBytes.LongLength,
                out var projectedCommittedAuthorityLength)
            || projectedCommittedAuthorityLength > _recordsAuthorityMaxBytes)
        {
            return LocalArtifactDbAuthorityQuotaExceeded<DesignConversionLocalArtifactDbRecord>();
        }

        Directory.CreateDirectory(_rootDirectory);
        if (_authorityAppendBeforeOpenCallbackForTesting is not null)
            await _authorityAppendBeforeOpenCallbackForTesting(ct);
        await CommitAuthorityRecordInPlaceAsync(authorityState, appendedLineBytes, ct);
        var postAppendFingerprint = GetRecordsFileFingerprint();

        var committedState = AddRecordToAuthorityState(
            authorityState,
            sanitized,
            line,
            appendOffset,
            appendedLineBytes,
            postAppendFingerprint);
        PublishCachedAuthorityStateFromCommittedAuthority(committedState);

        return await CompleteCommittedAuthorityAppendAsync(
            sanitized,
            ct,
            indexCt => TryWriteDerivedIndexAsync(committedState.Index, indexCt));
    }

    internal static async Task<OperationResult<DesignConversionLocalArtifactDbRecord>> CompleteCommittedAuthorityAppendAsync(
        DesignConversionLocalArtifactDbRecord sanitized,
        CancellationToken callerCancellationToken,
        Func<CancellationToken, Task<bool>> tryWriteDerivedIndexAsync)
    {
        if (callerCancellationToken.IsCancellationRequested)
        {
            return OperationResult<DesignConversionLocalArtifactDbRecord>.Ok(
                WithDeferredIndexEvidence(sanitized));
        }

        var derivedIndexDurable = await tryWriteDerivedIndexAsync(CancellationToken.None);
        return OperationResult<DesignConversionLocalArtifactDbRecord>.Ok(
            derivedIndexDurable ? sanitized : WithDeferredIndexEvidence(sanitized));
    }

    private async Task CommitAuthorityRecordInPlaceAsync(
        AuthorityState authorityState,
        byte[] appendedLineBytes,
        CancellationToken ct)
    {
        var expectedFingerprint = authorityState.Fingerprint;
        var expectedAppendOffset = authorityState.CommittedLength;
        var expectedSourceLength = expectedFingerprint.Length;
        ValidateAuthorityAppendBoundary(
            expectedAppendOffset,
            expectedSourceLength,
            authorityState.HasUncommittedFinalRecord,
            authorityState.UncommittedFinalByteOffset);

        var access = expectedAppendOffset < expectedSourceLength
            ? FileAccess.ReadWrite
            : FileAccess.Write;
        await using var stream = new FileStream(
            _recordsPath,
            expectedFingerprint.Exists ? FileMode.Open : FileMode.CreateNew,
            access,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.WriteThrough | FileOptions.RandomAccess);

        ValidateOpenAuthorityForAppend(stream, expectedFingerprint, expectedSourceLength);
        var originalTailBytes = await ReadOriginalAuthorityTailAsync(
            stream,
            expectedAppendOffset,
            expectedSourceLength,
            ct);

        ct.ThrowIfCancellationRequested();
        var mutationStarted = false;
        try
        {
            mutationStarted = true;
            stream.Seek(expectedAppendOffset, SeekOrigin.Begin);
            await stream.WriteAsync(appendedLineBytes.AsMemory(0, appendedLineBytes.Length), ct);
            stream.SetLength(expectedAppendOffset + appendedLineBytes.LongLength);
            await stream.FlushAsync(ct);
            stream.Flush(flushToDisk: true);
        }
        catch (OperationCanceledException) when (mutationStarted && ct.IsCancellationRequested)
        {
            await RestoreAuthorityTailBestEffortAsync(
                stream,
                expectedAppendOffset,
                originalTailBytes,
                expectedSourceLength,
                appendedLineBytes);
            throw;
        }
        catch (IOException) when (mutationStarted)
        {
            await RestoreAuthorityTailBestEffortAsync(
                stream,
                expectedAppendOffset,
                originalTailBytes,
                expectedSourceLength,
                appendedLineBytes);
            throw;
        }
        catch (UnauthorizedAccessException) when (mutationStarted)
        {
            await RestoreAuthorityTailBestEffortAsync(
                stream,
                expectedAppendOffset,
                originalTailBytes,
                expectedSourceLength,
                appendedLineBytes);
            throw;
        }
    }

    private void ValidateOpenAuthorityForAppend(
        FileStream stream,
        RecordsFileFingerprint expectedFingerprint,
        long expectedSourceLength)
    {
        if (stream.Length != expectedSourceLength)
            throw new IOException("DesignConversion authority source length changed before commit.");

        var observedFingerprint = GetRecordsFileFingerprint();
        if (expectedFingerprint.Exists)
        {
            if (!observedFingerprint.Equals(expectedFingerprint))
                throw new IOException("DesignConversion authority source fingerprint changed before commit.");
            return;
        }

        if (!observedFingerprint.Exists || observedFingerprint.Length != 0 || expectedSourceLength != 0)
            throw new IOException("DesignConversion authority source existence changed before commit.");
    }

    private static void ValidateAuthorityAppendBoundary(
        long expectedAppendOffset,
        long expectedSourceLength,
        bool hasUncommittedFinalRecord,
        long? uncommittedFinalByteOffset)
    {
        if (expectedAppendOffset < 0
            || expectedSourceLength < 0
            || expectedAppendOffset > expectedSourceLength)
        {
            throw new IOException("DesignConversion authority append boundary is invalid.");
        }

        var tailLength = expectedSourceLength - expectedAppendOffset;
        if (tailLength > MaxIndexedRecordLineByteLength)
            throw new IOException("DesignConversion authority uncommitted tail exceeded the record line bound.");

        if (hasUncommittedFinalRecord)
        {
            if (tailLength <= 0 || uncommittedFinalByteOffset != expectedAppendOffset)
                throw new IOException("DesignConversion authority uncommitted tail boundary changed before commit.");
            return;
        }

        if (tailLength != 0)
            throw new IOException("DesignConversion authority source length changed after committed prefix scan.");
    }

    private static async Task<byte[]> ReadOriginalAuthorityTailAsync(
        FileStream stream,
        long expectedAppendOffset,
        long expectedSourceLength,
        CancellationToken ct)
    {
        var tailLength = expectedSourceLength - expectedAppendOffset;
        if (tailLength == 0)
            return [];

        var tailBytes = new byte[(int)tailLength];
        stream.Seek(expectedAppendOffset, SeekOrigin.Begin);
        await stream.ReadExactlyAsync(tailBytes.AsMemory(0, tailBytes.Length), ct);
        return tailBytes;
    }

    private static async Task RestoreAuthorityTailBestEffortAsync(
        FileStream stream,
        long expectedAppendOffset,
        byte[] originalTailBytes,
        long expectedSourceLength,
        byte[] appendedLineBytes)
    {
        if (await TryTruncateAuthorityToCommittedPrefixAsync(stream, expectedAppendOffset))
        {
            await TryRestoreOriginalAuthorityTailAsync(
                stream,
                expectedAppendOffset,
                originalTailBytes,
                expectedSourceLength);
            return;
        }

        await TryInvalidateAppendedAuthorityLineTerminatorAsync(
            stream,
            expectedAppendOffset,
            appendedLineBytes.LongLength);
    }

    private static async Task<bool> TryTruncateAuthorityToCommittedPrefixAsync(
        FileStream stream,
        long expectedAppendOffset)
    {
        try
        {
            stream.SetLength(expectedAppendOffset);
            await stream.FlushAsync(CancellationToken.None);
            stream.Flush(flushToDisk: true);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (System.Security.SecurityException)
        {
            return false;
        }
    }

    private static async Task TryRestoreOriginalAuthorityTailAsync(
        FileStream stream,
        long expectedAppendOffset,
        byte[] originalTailBytes,
        long expectedSourceLength)
    {
        try
        {
            if (originalTailBytes.Length > 0)
            {
                stream.Seek(expectedAppendOffset, SeekOrigin.Begin);
                await stream.WriteAsync(originalTailBytes.AsMemory(0, originalTailBytes.Length), CancellationToken.None);
            }

            stream.SetLength(expectedSourceLength);
            await stream.FlushAsync(CancellationToken.None);
            stream.Flush(flushToDisk: true);
        }
        catch (IOException)
        {
            // The committed prefix has already been durably restored; any tail loss is recoverable by replay.
        }
        catch (UnauthorizedAccessException)
        {
            // The committed prefix has already been durably restored; any tail loss is recoverable by replay.
        }
        catch (System.Security.SecurityException)
        {
            // The committed prefix has already been durably restored; any tail loss is recoverable by replay.
        }
    }

    private static async Task TryInvalidateAppendedAuthorityLineTerminatorAsync(
        FileStream stream,
        long expectedAppendOffset,
        long appendedLineLength)
    {
        if (appendedLineLength <= 0)
            return;

        try
        {
            stream.Seek(expectedAppendOffset + appendedLineLength - 1, SeekOrigin.Begin);
            await stream.WriteAsync(new byte[] { (byte)' ' }, CancellationToken.None);
            await stream.FlushAsync(CancellationToken.None);
            stream.Flush(flushToDisk: true);
        }
        catch (IOException)
        {
            // Nothing more can be done without risking the committed prefix.
        }
        catch (UnauthorizedAccessException)
        {
            // Nothing more can be done without risking the committed prefix.
        }
        catch (System.Security.SecurityException)
        {
            // Nothing more can be done without risking the committed prefix.
        }
    }

    private async Task<OperationResult<RecordIdReplayResolution>> ResolveRecordIdReplayAsync(
        DesignConversionLocalArtifactDbRecord sanitized,
        AuthorityState authorityState,
        CancellationToken ct)
    {
        if (!authorityState.RecordsById.TryGetValue(sanitized.RecordId, out var existingState))
            return OperationResult<RecordIdReplayResolution>.Ok(new RecordIdReplayResolution(false, null));
        if (existingState.Count != 1)
            return LocalArtifactDbRecordIdConflict<RecordIdReplayResolution>();

        var replayFingerprint = GetImmutableAppendFingerprint(sanitized);
        // Legacy rows have no trustworthy original append identity, so replay fails closed.
        if (existingState.ImmutableAppendFingerprint is null
            || replayFingerprint is null
            || !string.Equals(
                existingState.ImmutableAppendFingerprint,
                replayFingerprint,
                StringComparison.Ordinal))
        {
            return LocalArtifactDbRecordIdConflict<RecordIdReplayResolution>();
        }

        var currentRecord = MaterializeDetachedAuthorityRecord(existingState.CurrentRecordJson);
        var returnedRecord = await TryWriteDerivedIndexAsync(authorityState.Index, ct)
            ? currentRecord
            : WithDeferredIndexEvidence(currentRecord);
        return OperationResult<RecordIdReplayResolution>.Ok(new RecordIdReplayResolution(true, returnedRecord));
    }

    private async Task<bool> TryWriteDerivedIndexAsync(
        DesignConversionLocalArtifactDbIndex index,
        CancellationToken ct)
    {
        var tempPath = Path.Combine(_rootDirectory, $"{IndexFileName}.{Guid.NewGuid():N}.write.tmp");
        try
        {
            Directory.CreateDirectory(_rootDirectory);
            await using (var temp = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                var preamble = Encoding.UTF8.GetPreamble();
                if (preamble.Length > 0)
                    await temp.WriteAsync(preamble, ct);

                await JsonSerializer.SerializeAsync(temp, index, IndexJsonOptions, ct);
                await temp.FlushAsync(ct);
                temp.Flush(flushToDisk: true);
            }

            ct.ThrowIfCancellationRequested();
            File.Move(tempPath, _indexPath, overwrite: true);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            DeleteIndexWriteTempBestEffort(tempPath);
        }
    }

    private static void DeleteIndexWriteTempBestEffort(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch (IOException)
        {
            // Best-effort cleanup must not replace the index write outcome.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup must not replace the index write outcome.
        }
        catch (System.Security.SecurityException)
        {
            // Best-effort cleanup must not replace the index write outcome.
        }
    }

    private async Task<OperationResult<AuthorityState>> ResolveCurrentAuthorityStateForAppendAsync(
        CancellationToken ct)
    {
        var fingerprint = GetRecordsFileFingerprint();
        var cached = _recordsMutationContext.CachedAuthorityState;
        if (cached is not null && cached.Fingerprint.Equals(fingerprint))
            return OperationResult<AuthorityState>.Ok(cached.State);

        var scan = await ReadAuthorityRecordLinesCoreAsync(ct);
        if (!scan.IsSuccess)
            return scan.MapError<AuthorityState>();

        var postReadFingerprint = GetRecordsFileFingerprint();
        if (!postReadFingerprint.Equals(fingerprint))
        {
            _recordsMutationContext.CachedAuthorityState = null;
            return LocalArtifactDbWriteFailed<AuthorityState>();
        }

        var state = BuildAuthorityState(scan.Value!);
        PublishCachedAuthorityState(state, postReadFingerprint);
        return OperationResult<AuthorityState>.Ok(state);
    }

    private RecordsFileFingerprint GetRecordsFileFingerprint()
    {
        var info = new FileInfo(_recordsPath);
        if (!info.Exists)
            return new RecordsFileFingerprint(false, 0, DateTime.MinValue);

        info.Refresh();
        return new RecordsFileFingerprint(
            true,
            info.Length,
            info.LastWriteTimeUtc);
    }

    private void PublishCachedAuthorityState(
        AuthorityState state,
        RecordsFileFingerprint fingerprint)
        => _recordsMutationContext.CachedAuthorityState = new CachedAuthorityState(state, fingerprint);

    private void PublishCachedAuthorityStateFromCommittedAuthority(AuthorityState state)
    {
        RecordsFileFingerprint fingerprint;
        try
        {
            fingerprint = GetRecordsFileFingerprint();
        }
        catch (IOException)
        {
            _recordsMutationContext.CachedAuthorityState = null;
            return;
        }
        catch (UnauthorizedAccessException)
        {
            _recordsMutationContext.CachedAuthorityState = null;
            return;
        }
        catch (System.Security.SecurityException)
        {
            _recordsMutationContext.CachedAuthorityState = null;
            return;
        }

        PublishCachedAuthorityState(state, fingerprint);
    }

    private AuthorityState BuildAuthorityState(AuthorityRecordScan scan)
    {
        var recordsById = new Dictionary<string, AuthorityRecordIdState>(StringComparer.Ordinal);
        foreach (var recordLine in scan.Lines)
        {
            var record = recordLine.Record;
            if (string.IsNullOrEmpty(record.RecordId))
                continue;

            if (recordsById.TryGetValue(record.RecordId, out var existing))
            {
                recordsById[record.RecordId] = existing with { Count = existing.Count + 1 };
                continue;
            }

            recordsById[record.RecordId] = new AuthorityRecordIdState(
                Count: 1,
                CurrentRecordJson: recordLine.CurrentRecordJson,
                ImmutableAppendFingerprint: GetImmutableAppendFingerprint(record));
        }

        return new AuthorityState(
            BuildIndex(scan),
            scan.CommittedLength,
            scan.NextAppendLineNumber,
            scan.Fingerprint,
            scan.HasUncommittedFinalRecord,
            scan.UncommittedFinalLineNumber,
            scan.UncommittedFinalByteOffset,
            recordsById);
    }

    private AuthorityState AddRecordToAuthorityState(
        AuthorityState state,
        DesignConversionLocalArtifactDbRecord record,
        string authorityLine,
        long byteOffset,
        byte[] authorityLineBytes,
        RecordsFileFingerprint fingerprint)
    {
        var recordIdState = new AuthorityRecordIdState(
            Count: 1,
            CurrentRecordJson: authorityLine,
            ImmutableAppendFingerprint: GetImmutableAppendFingerprint(record));
        var authorityLineSha256 = $"sha256:{HashBytes(authorityLineBytes)}";
        var locator = BuildIndexRecordLocator(
            record,
            byteOffset,
            authorityLineBytes.Length,
            state.NextLineNumber,
            authorityLineSha256);
        var approvedCandidateCount = record.DpoCandidates.Count(IsApprovedTrainingCandidate);
        var updatedAtUtc = DateTimeOffset.UtcNow;
        var committedLength = fingerprint.Exists ? fingerprint.Length : byteOffset + authorityLineBytes.Length;
        var rootDirectoryBindingHash = BuildRootDirectoryBindingHash(_rootDirectory);

        state.AddCommittedRecord(
            record.RecordId,
            recordIdState,
            locator,
            approvedCandidateCount,
            updatedAtUtc,
            fingerprint,
            committedLength,
            rootDirectoryBindingHash);
        return state;
    }

    private static DesignConversionLocalArtifactDbRecord MaterializeDetachedAuthorityRecord(string authorityRecordJson)
        => JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(authorityRecordJson, JsonOptions)
           ?? throw new JsonException("Cached DesignConversion authority record snapshot is invalid.");

    private static DesignConversionLocalArtifactDbRecord WithDeferredIndexEvidence(
        DesignConversionLocalArtifactDbRecord record)
    {
        var metadata = new Dictionary<string, string>(record.Metadata, StringComparer.Ordinal)
        {
            ["local_artifact_db_authority_record_committed"] = bool.TrueString,
            ["local_artifact_db_derived_index_update"] = "deferred",
            ["local_artifact_db_derived_index_durable"] = bool.FalseString,
            ["local_artifact_db_cross_file_atomicity_claimed"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString
        };

        var evidenceRefs = record.EvidenceRefs
            .Append(AuthorityRecordCommittedMarker)
            .Append(DerivedIndexUpdateDeferredMarker)
            .Append(DerivedIndexDurableFalseMarker)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return record with
        {
            EvidenceRefs = evidenceRefs,
            Metadata = metadata
        };
    }

    private static DesignConversionDpoCandidateApprovalResult WithDeferredIndexEvidence(
        DesignConversionDpoCandidateApprovalResult result)
    {
        var boundaryMetadata = new Dictionary<string, string>(result.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["local_artifact_db_authority_record_committed"] = bool.TrueString,
            ["local_artifact_db_derived_index_update"] = "deferred",
            ["local_artifact_db_derived_index_durable"] = bool.FalseString,
            ["local_artifact_db_cross_file_atomicity_claimed"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString
        };

        var evidenceRefs = result.EvidenceRefs
            .Append(AuthorityRecordCommittedMarker)
            .Append(DerivedIndexUpdateDeferredMarker)
            .Append(DerivedIndexDurableFalseMarker)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return result with
        {
            EvidenceRefs = evidenceRefs,
            BoundaryMetadata = boundaryMetadata
        };
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
        DesignConversionLocalArtifactDbQuery? query = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

        var effectiveQuery = query ?? new DesignConversionLocalArtifactDbQuery();
        LastSelectiveListRecordsUsedDurableIndex = false;
        var forceDurableIndexHeal = false;
        if (IsSelectiveDurableIndexQuery(effectiveQuery))
        {
            var indexed = await TryListRecordsFromDurableIndexAsync(effectiveQuery, ct);
            if (indexed.UsedDurableIndex)
            {
                LastSelectiveListRecordsUsedDurableIndex = true;
                return indexed.Result;
            }

            forceDurableIndexHeal = indexed.RequiresDurableIndexHeal;
        }

        var records = await ReadRecordsAndHealDerivedIndexAsync(ct, forceDurableIndexHeal);
        if (!records.IsSuccess)
            return records.MapError<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

        var filtered = records.Value!
            .Where(record => MatchesRecord(record, effectiveQuery))
            .OrderBy(record => record.CreatedAtUtc)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(filtered);
    }

    private static bool IsSelectiveDurableIndexQuery(DesignConversionLocalArtifactDbQuery query)
        => !string.IsNullOrWhiteSpace(query.JobId)
           || !string.IsNullOrWhiteSpace(query.RequestId);

    private async Task<DurableIndexLookupResult> TryListRecordsFromDurableIndexAsync(
        DesignConversionLocalArtifactDbQuery query,
        CancellationToken ct)
    {
        var fingerprint = GetRecordsFileFingerprint();
        var index = await TryReadUsableDurableIndexAsync(fingerprint, ct);
        if (index is null)
            return DurableIndexFallback();

        if (!await HasCompleteDurableIndexAuthorityCoverageAsync(index, fingerprint, ct))
            return DurableIndexFallback();

        var locators = index.RecordLocators
            .Where(locator => MatchesIndexLocator(locator, query))
            .OrderBy(locator => locator.ByteOffset)
            .ThenBy(locator => locator.RecordId, StringComparer.Ordinal)
            .ToArray();
        if (locators.Length == 0)
        {
            return new DurableIndexLookupResult(
                true,
                false,
                OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok([]));
        }

        var read = await TryReadIndexedCandidateRecordsAsync(locators, ct);
        if (!read.IsTrusted)
            return DurableIndexFallback();

        if (!GetRecordsFileFingerprint().Equals(fingerprint))
            return DurableIndexFallback();

        var filtered = read.Records
            .Where(record => MatchesRecord(record, query))
            .OrderBy(record => record.CreatedAtUtc)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();

        return new DurableIndexLookupResult(
            true,
            false,
            OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(filtered));
    }

    private static DurableIndexLookupResult DurableIndexFallback()
        => new(
            false,
            true,
            OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok([]));

    private async Task<DesignConversionLocalArtifactDbIndex?> TryReadUsableDurableIndexAsync(
        RecordsFileFingerprint fingerprint,
        CancellationToken ct)
    {
        if (!fingerprint.Exists || !File.Exists(_indexPath))
            return null;

        DesignConversionLocalArtifactDbIndex? index;
        try
        {
            await using var stream = new FileStream(
                _indexPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            index = await JsonSerializer.DeserializeAsync<DesignConversionLocalArtifactDbIndex>(
                stream,
                IndexJsonOptions,
                ct);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        return IsUsableDurableIndex(index, fingerprint) ? index : null;
    }

    private bool IsUsableDurableIndex(
        DesignConversionLocalArtifactDbIndex? index,
        RecordsFileFingerprint fingerprint)
    {
        if (index is null
            || !string.Equals(index.DatabaseKind, LocalArtifactDbKind, StringComparison.Ordinal)
            || !string.Equals(index.SchemaVersion, SchemaVersion, StringComparison.Ordinal)
            || !string.Equals(index.RootDirectoryBindingHash, BuildRootDirectoryBindingHash(_rootDirectory), StringComparison.Ordinal)
            || !string.Equals(index.TenantBindingHash, TenantBindingNotApplicable, StringComparison.Ordinal)
            || !string.Equals(index.RecordsFileName, RecordsFileName, StringComparison.Ordinal)
            || index.RecordsFileLength != fingerprint.Length
            || index.RecordsFileLastWriteTimeUtcTicks != fingerprint.LastWriteTimeUtc.Ticks
            || index.RecordCount < 0
            || index.ApprovedTrainingCandidateCount < 0
            || index.EvidenceMarkers is null
            || index.RecordLocators is null
            || index.RecordLocators.Count != index.RecordCount
            || !index.EvidenceMarkers.Contains(StorageTrainingArtifactDbMarker, StringComparer.Ordinal)
            || !index.EvidenceMarkers.Contains(LocalArtifactDbKind, StringComparer.Ordinal))
        {
            return false;
        }

        var quarantinedLines = index.QuarantinedAuthorityLines ?? [];
        if (quarantinedLines.Count > MaxLogicallyQuarantinedAuthorityLinesPerScan)
        {
            return false;
        }

        if (quarantinedLines.Count > 0
            && (!index.EvidenceMarkers.Contains(LogicalQuarantinedAuthorityLineMarker, StringComparer.Ordinal)
                || !index.EvidenceMarkers.Contains(QuarantinedAuthorityBytesPreservedMarker, StringComparer.Ordinal)
                || !index.EvidenceMarkers.Contains(
                    $"local_artifact_db:quarantined_authority_line_count:{quarantinedLines.Count.ToString(CultureInfo.InvariantCulture)}",
                    StringComparer.Ordinal)))
        {
            return false;
        }

        foreach (var descriptor in quarantinedLines)
        {
            if (descriptor is null
                || descriptor.PhysicalLineNumber <= 0
                || descriptor.ByteOffset < 0
                || descriptor.ByteLength <= 0
                || descriptor.ByteLength > MaxIndexedRecordLineByteLength
                || descriptor.ByteOffset > fingerprint.Length - descriptor.ByteLength
                || !IsSha256Ref(descriptor.AuthorityLineSha256))
            {
                return false;
            }
        }

        var previousEnd = 0L;
        foreach (var locator in index.RecordLocators.OrderBy(locator => locator.ByteOffset))
        {
            if (string.IsNullOrWhiteSpace(locator.RecordId)
                || string.IsNullOrWhiteSpace(locator.JobId)
                || string.IsNullOrWhiteSpace(locator.RequestId)
                || string.IsNullOrWhiteSpace(locator.SourceKind)
                || locator.ByteOffset < 0
                || locator.ByteLength <= 0
                || locator.ByteLength > MaxIndexedRecordLineByteLength
                || locator.ByteOffset < previousEnd
                || locator.ByteOffset > fingerprint.Length - locator.ByteLength
                || locator.LineNumber <= 0
                || !IsSha256Ref(locator.AuthorityLineSha256)
                || !IsSha256Ref(locator.ImmutableAppendFingerprint))
            {
                return false;
            }

            previousEnd = locator.ByteOffset + locator.ByteLength;
        }

        return true;
    }

    private async Task<bool> HasCompleteDurableIndexAuthorityCoverageAsync(
        DesignConversionLocalArtifactDbIndex index,
        RecordsFileFingerprint fingerprint,
        CancellationToken ct)
    {
        var orderedLocators = index.RecordLocators
            .OrderBy(locator => locator.ByteOffset)
            .ToArray();
        var locatorIndex = 0;
        var lineStartOffset = 0L;
        var lineNumber = 0;

        try
        {
            await using var stream = new FileStream(
                _recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var buffer = new byte[8192];
            using var currentLine = new MemoryStream();

            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
                if (read == 0)
                    break;

                for (var bufferIndex = 0; bufferIndex < read; bufferIndex++)
                {
                    if (currentLine.Length >= MaxIndexedRecordLineByteLength)
                        return false;

                    currentLine.WriteByte(buffer[bufferIndex]);
                    if (buffer[bufferIndex] != (byte)'\n')
                        continue;

                    lineNumber++;
                    if (!IsCoveredAuthorityLine(
                            currentLine.ToArray(),
                            lineStartOffset,
                            lineNumber,
                            orderedLocators,
                            ref locatorIndex))
                    {
                        return false;
                    }

                    lineStartOffset += currentLine.Length;
                    currentLine.SetLength(0);
                    currentLine.Position = 0;
                }
            }

            if (currentLine.Length > 0)
            {
                lineNumber++;
                if (!IsCoveredAuthorityLine(
                        currentLine.ToArray(),
                        lineStartOffset,
                        lineNumber,
                        orderedLocators,
                        ref locatorIndex))
                {
                    return false;
                }
            }
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        return locatorIndex == orderedLocators.Length
               && GetRecordsFileFingerprint().Equals(fingerprint);
    }

    private static bool IsCoveredAuthorityLine(
        byte[] lineBytes,
        long byteOffset,
        int lineNumber,
        IReadOnlyList<DesignConversionLocalArtifactDbIndexRecordLocator> orderedLocators,
        ref int locatorIndex)
    {
        var line = DecodeAuthorityJsonLine(lineBytes, byteOffset == 0);
        if (string.IsNullOrWhiteSpace(line))
            return true;
        if (locatorIndex >= orderedLocators.Count)
            return false;

        var locator = orderedLocators[locatorIndex];
        if (locator.ByteOffset != byteOffset
            || locator.ByteLength != lineBytes.LongLength
            || locator.LineNumber != lineNumber
            || !string.Equals(
                locator.AuthorityLineSha256,
                $"sha256:{HashBytes(lineBytes)}",
                StringComparison.Ordinal))
        {
            return false;
        }

        locatorIndex++;
        return true;
    }

    private static bool MatchesIndexLocator(
        DesignConversionLocalArtifactDbIndexRecordLocator locator,
        DesignConversionLocalArtifactDbQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.JobId)
            && !string.Equals(locator.JobId, query.JobId, StringComparison.Ordinal))
            return false;
        if (!string.IsNullOrWhiteSpace(query.RequestId)
            && !string.Equals(locator.RequestId, query.RequestId, StringComparison.Ordinal))
            return false;

        return true;
    }

    private async Task<IndexedCandidateReadResult> TryReadIndexedCandidateRecordsAsync(
        IReadOnlyList<DesignConversionLocalArtifactDbIndexRecordLocator> locators,
        CancellationToken ct)
    {
        var records = new List<DesignConversionLocalArtifactDbRecord>();
        try
        {
            await using var stream = new FileStream(
                _recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.RandomAccess);

            foreach (var locator in locators)
            {
                if (locator.ByteOffset < 0
                    || locator.ByteLength <= 0
                    || locator.ByteLength > MaxIndexedRecordLineByteLength
                    || locator.ByteOffset > stream.Length - locator.ByteLength)
                {
                    return new IndexedCandidateReadResult(false, []);
                }

                var lineBytes = new byte[(int)locator.ByteLength];
                stream.Seek(locator.ByteOffset, SeekOrigin.Begin);
                await stream.ReadExactlyAsync(lineBytes.AsMemory(0, lineBytes.Length), ct);

                if (!string.Equals($"sha256:{HashBytes(lineBytes)}", locator.AuthorityLineSha256, StringComparison.Ordinal))
                    return new IndexedCandidateReadResult(false, []);

                var line = DecodeAuthorityJsonLine(lineBytes, locator.ByteOffset == 0);
                DesignConversionLocalArtifactDbRecord? record;
                try
                {
                    record = JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
                        line,
                        JsonOptions);
                }
                catch (JsonException)
                {
                    return new IndexedCandidateReadResult(false, []);
                }

                if (!IsIndexedRecordLocatorMatch(record, locator))
                    return new IndexedCandidateReadResult(false, []);

                records.Add(record!);
            }
        }
        catch (DecoderFallbackException)
        {
            return new IndexedCandidateReadResult(false, []);
        }
        catch (EndOfStreamException)
        {
            return new IndexedCandidateReadResult(false, []);
        }
        catch (IOException)
        {
            return new IndexedCandidateReadResult(false, []);
        }
        catch (UnauthorizedAccessException)
        {
            return new IndexedCandidateReadResult(false, []);
        }

        return new IndexedCandidateReadResult(true, records);
    }

    private static bool IsIndexedRecordLocatorMatch(
        DesignConversionLocalArtifactDbRecord? record,
        DesignConversionLocalArtifactDbIndexRecordLocator locator)
        => record is not null
           && string.Equals(record.DatabaseKind, LocalArtifactDbKind, StringComparison.Ordinal)
           && string.Equals(record.RecordId, locator.RecordId, StringComparison.Ordinal)
           && string.Equals(record.JobId, locator.JobId, StringComparison.Ordinal)
           && string.Equals(record.RequestId, locator.RequestId, StringComparison.Ordinal)
           && string.Equals(record.SourceKind, locator.SourceKind, StringComparison.Ordinal)
           && record.CreatedAtUtc.Equals(locator.CreatedAtUtc)
           && string.Equals(GetImmutableAppendFingerprint(record), locator.ImmutableAppendFingerprint, StringComparison.Ordinal);

    public async Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
        DesignConversionLocalArtifactDbQuery? query = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<IReadOnlyList<DesignConversionDpoTrainingRecord>>();

        var records = await ListRecordsAsync(query, ct);
        if (!records.IsSuccess)
            return records.MapError<IReadOnlyList<DesignConversionDpoTrainingRecord>>();

        var effectiveQuery = query ?? new DesignConversionLocalArtifactDbQuery();
        var candidates = records.Value!
            .SelectMany(record => record.DpoCandidates)
            .Where(candidate => IsApprovedTrainingCandidate(candidate))
            .Where(candidate => MatchesCandidate(candidate, effectiveQuery))
            .Select(candidate => candidate.TrainingRecord)
            .OrderByDescending(record => record.UsefulnessScore)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>.Ok(candidates);
    }

    public async Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
        DesignConversionLocalArtifactDbQuery? query = null,
        DateTimeOffset? exportedAtUtc = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionApprovedDpoHandoffArtifact>();

        var records = await ListRecordsAsync(query, ct);
        if (!records.IsSuccess)
            return records.MapError<DesignConversionApprovedDpoHandoffArtifact>();

        var effectiveQuery = query ?? new DesignConversionLocalArtifactDbQuery();
        var rows = records.Value!
            .SelectMany(record => record.DpoCandidates.Select(candidate => new { record, candidate }))
            .Where(item => IsApprovedRuntimeHandoffCandidate(item.record, item.candidate))
            .Where(item => MatchesCandidate(item.candidate, effectiveQuery))
            .OrderByDescending(item => item.candidate.UsefulnessScore)
            .ThenBy(item => item.candidate.RecordId, StringComparer.Ordinal)
            .Select(item => BuildHandoffRow(item.record, item.candidate))
            .ToArray();
        var artifactId = BuildHandoffArtifactId(rows);
        var result = new DesignConversionApprovedDpoHandoffArtifact(
            ArtifactId: artifactId,
            ArtifactRef: $"memory://design-conversion/dpo-handoff/{artifactId}",
            SchemaVersion: ApprovedDpoHandoffSchemaVersion,
            ExportedAtUtc: (exportedAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime(),
            ApprovedCandidateCount: rows.Length,
            Rows: rows,
            Warnings: BuildHandoffWarnings(rows.Length),
            BoundaryMetadata: BuildHandoffBoundaryMetadata(rows.Length));

        return OperationResult<DesignConversionApprovedDpoHandoffArtifact>.Ok(result);
    }

    public async Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
        DesignConversionApprovedDpoHandoffArtifact handoff,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionTrainingIntakeRecord>();
        if (handoff is null)
            return OperationResult<DesignConversionTrainingIntakeRecord>.Fail(
                "DesignConversion approved DPO handoff artifact is required for training intake.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_HANDOFF_REQUIRED");
        if (string.IsNullOrWhiteSpace(requestedByRef))
            return OperationResult<DesignConversionTrainingIntakeRecord>.Fail(
                "DesignConversion training intake requires a requester reference.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_REQUESTER_REQUIRED");
        if (string.IsNullOrWhiteSpace(reason))
            return OperationResult<DesignConversionTrainingIntakeRecord>.Fail(
                "DesignConversion training intake requires an evidence-bound request reason.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_REASON_REQUIRED");
        if (ContainsForbiddenTrainingIntakePayload(requestedByRef)
            || ContainsForbiddenTrainingIntakePayload(reason))
        {
            return OperationResult<DesignConversionTrainingIntakeRecord>.Fail(
                "DesignConversion training intake request metadata contains raw or private payload markers.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_REQUEST_UNSANITIZED");
        }

        var validation = ValidateTrainingIntakeHandoff(handoff);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionTrainingIntakeRecord>();

        var requestedAt = (requestedAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();
        var intake = BuildTrainingIntakeRecord(handoff, requestedByRef, reason, requestedAt, admissionBoundaryMetadata);

        var gateAcquired = false;
        try
        {
            await WaitForObservedGateAsync(
                _recordsMutationContext.CreateTrainingIntakeJournalGate,
                _createTrainingIntakeJournalWaitTaskObserverForTesting,
                () => gateAcquired = true,
                ct);

            if (_createTrainingIntakeJournalGateAcquiredCallbackForTesting is not null)
                await _createTrainingIntakeJournalGateAcquiredCallbackForTesting(ct);

            await using var mutationLease = await AcquireTrainingIntakeMutationLeaseAsync(ct);
            return await StoreCreateTrainingIntakeWithIdempotencyReplayAsync(intake, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionTrainingIntakeRecord>();
        }
        catch (IOException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeRecord>();
        }
        catch (UnauthorizedAccessException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeRecord>();
        }
        finally
        {
            if (gateAcquired)
                _recordsMutationContext.CreateTrainingIntakeJournalGate.Release();
        }
    }

    private async Task<FileStream> AcquireTrainingIntakeMutationLeaseAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(_rootDirectory);

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                return new FileStream(
                    _trainingIntakeMutationLockPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.Asynchronous);
            }
            catch (IOException ex) when (IsFileSharingOrLockViolation(ex))
            {
                await Task.Delay(TrainingIntakeMutationLockRetryDelayMs, ct);
            }
        }
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeRecord>> StoreCreateTrainingIntakeWithIdempotencyReplayAsync(
        DesignConversionTrainingIntakeRecord intake,
        CancellationToken ct)
    {
        var replay = await ResolveCreateTrainingIntakeIdempotencyReplayAsync(intake, ct);
        if (!replay.IsSuccess)
            return replay.MapError<DesignConversionTrainingIntakeRecord>();
        if (replay.Value!.HasReplay)
            return OperationResult<DesignConversionTrainingIntakeRecord>.Ok(replay.Value.Record!);

        intake = MarkCreateTrainingIntakeIdempotencyBoundary(
            intake,
            replayHit: false,
            createdNewSideEffect: true);

        Directory.CreateDirectory(_rootDirectory);
        var line = JsonSerializer.Serialize(intake, JsonOptions);
        if (!TryProjectTrainingIntakeRequestsLength(line, out var projectedLength)
            || projectedLength > _trainingIntakeRequestsMaxBytes)
        {
            return TrainingIntakeRequestsQuotaExceeded<DesignConversionTrainingIntakeRecord>();
        }

        await DurableCopyOnWriteAppendJsonLineAsync(_trainingIntakePath, line, ct);

        return OperationResult<DesignConversionTrainingIntakeRecord>.Ok(intake);
    }

    private bool TryProjectTrainingIntakeRequestsLength(
        string line,
        out long projectedLength)
    {
        var existingLength = File.Exists(_trainingIntakePath)
            ? new FileInfo(_trainingIntakePath).Length
            : 0;
        var appendedLineByteLength = StrictUtf8Encoding.GetByteCount(line + Environment.NewLine);

        return TryProjectCommittedAuthorityLength(
            existingLength,
            appendedLineByteLength,
            out projectedLength);
    }

    private static async Task DurableCopyOnWriteAppendJsonLineAsync(
        string destinationPath,
        string line,
        CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (string.IsNullOrWhiteSpace(directory))
            directory = Directory.GetCurrentDirectory();

        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(
            directory,
            $"{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.copyonwrite.tmp");
        var appendedBytes = StrictUtf8Encoding.GetBytes(line + Environment.NewLine);
        FileStream? source = null;
        var committed = false;

        try
        {
            var destinationExists = File.Exists(destinationPath);
            long originalSourceLength = 0;
            if (destinationExists)
            {
                source = new FileStream(
                    destinationPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read | FileShare.Delete,
                    bufferSize: 81920,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                originalSourceLength = source.Length;
            }

            await using (var temp = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.WriteThrough | FileOptions.SequentialScan))
            {
                if (source is not null)
                {
                    await source.CopyToAsync(temp, 81920, ct);
                    if (source.Length != originalSourceLength
                        || temp.Position != originalSourceLength
                        || temp.Length != originalSourceLength)
                    {
                        throw new IOException("DesignConversion journal source length changed during copy-on-write append.");
                    }
                }

                await temp.WriteAsync(appendedBytes.AsMemory(0, appendedBytes.Length), ct);
                await temp.FlushAsync(ct);
                temp.Flush(flushToDisk: true);
            }

            ct.ThrowIfCancellationRequested();

            if (destinationExists)
                File.Replace(tempPath, destinationPath, null);
            else
                File.Move(tempPath, destinationPath);

            committed = true;
        }
        finally
        {
            TryDisposeCopyOnWriteSource(source);
            if (!committed || File.Exists(tempPath))
                TryDeleteCopyOnWriteTemp(tempPath);
        }
    }

    private static void TryDisposeCopyOnWriteSource(FileStream? source)
    {
        if (source is null)
            return;

        try
        {
            source.Dispose();
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
    }

    private static void TryDeleteCopyOnWriteTemp(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();
        if (!File.Exists(_trainingIntakePath))
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>.Ok([]);

        var records = new List<DesignConversionTrainingIntakeRecord>();
        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(_trainingIntakePath, StrictUtf8Encoding, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();
        }
        catch (DecoderFallbackException)
        {
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>.Fail(
                "DesignConversion training intake requests could not be read because the training intake requests file is not valid UTF-8.",
                InvalidUtf8ErrorCode);
        }
        catch (IOException)
        {
            return TrainingIntakeReadFailed<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();
        }
        catch (UnauthorizedAccessException)
        {
            return TrainingIntakeReadFailed<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();
        }
        catch (SecurityException)
        {
            return TrainingIntakeReadFailed<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();
        }

        for (var i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            DesignConversionTrainingIntakeRecord? record;
            try
            {
                record = JsonSerializer.Deserialize<DesignConversionTrainingIntakeRecord>(
                    lines[i],
                    JsonOptions);
            }
            catch (JsonException)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>.Fail(
                    $"DesignConversion training intake line {(i + 1).ToString(CultureInfo.InvariantCulture)} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_LINE_INVALID");
            }
            catch (NotSupportedException)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>.Fail(
                    $"DesignConversion training intake line {(i + 1).ToString(CultureInfo.InvariantCulture)} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_LINE_INVALID");
            }

            if (record is null)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>.Fail(
                    $"DesignConversion training intake line {(i + 1).ToString(CultureInfo.InvariantCulture)} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_LINE_INVALID");
            }

            records.Add(record);
        }

        return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>.Ok(
            records
                .OrderBy(record => record.RequestedAtUtc)
                .ThenBy(record => record.IntakeId, StringComparer.Ordinal)
                .ToArray());
    }

    public async Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
        string intakeId,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionTrainingIntakePreflightResult>();
        if (string.IsNullOrWhiteSpace(intakeId))
            return OperationResult<DesignConversionTrainingIntakePreflightResult>.Fail(
                "DesignConversion training intake preflight requires an intake id.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_ID_REQUIRED");
        if (ContainsForbiddenTrainingIntakePayload(intakeId))
            return OperationResult<DesignConversionTrainingIntakePreflightResult>.Fail(
                "DesignConversion training intake preflight id contains raw or private payload markers.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_ID_UNSANITIZED");

        var intakes = await ListTrainingIntakeRequestsAsync(ct);
        if (!intakes.IsSuccess)
            return intakes.MapError<DesignConversionTrainingIntakePreflightResult>();

        var matches = intakes.Value!
            .Where(record => string.Equals(record.IntakeId, intakeId, StringComparison.Ordinal))
            .ToArray();
        if (matches.Length == 0)
            return OperationResult<DesignConversionTrainingIntakePreflightResult>.Fail(
                "DesignConversion training intake preflight could not find the queued intake record.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_NOT_FOUND");
        if (matches.Length > 1)
            return OperationResult<DesignConversionTrainingIntakePreflightResult>.Fail(
                "DesignConversion training intake preflight found duplicate intake ids.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_AMBIGUOUS");

        return OperationResult<DesignConversionTrainingIntakePreflightResult>.Ok(
            BuildTrainingIntakePreflightResult(matches[0]));
    }

    public async Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
        string intakeId,
        DesignConversionTrainingPipelineOptions? options = null,
        CancellationToken ct = default,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionTrainingIntakeRunResult>();

        var preflight = await RunTrainingIntakePreflightAsync(intakeId, ct);
        if (!preflight.IsSuccess)
            return preflight.MapError<DesignConversionTrainingIntakeRunResult>();

        var intake = await FindQueuedTrainingIntakeAsync(intakeId, ct);
        if (!intake.IsSuccess)
            return intake.MapError<DesignConversionTrainingIntakeRunResult>();

        var runOptions = options ?? BuildDefaultTrainingIntakeRunOptions(intake.Value!);
        var gateAcquired = false;
        try
        {
            await WaitForObservedGateAsync(
                _recordsMutationContext.RunTrainingIntakeGate,
                _runTrainingIntakeGateWaitTaskObserverForTesting,
                () => gateAcquired = true,
                ct);

            if (_runTrainingIntakeGateAcquiredCallbackForTesting is not null)
                await _runTrainingIntakeGateAcquiredCallbackForTesting(ct);

            await using var mutationLease = await AcquireTrainingIntakeRunMutationLeaseAsync(ct);
            return await RunTrainingIntakeCoreAsync(
                intake.Value!,
                preflight.Value!,
                runOptions,
                optionsProvided: options is not null,
                admissionBoundaryMetadata,
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionTrainingIntakeRunResult>();
        }
        catch (IOException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeRunResult>();
        }
        catch (UnauthorizedAccessException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeRunResult>();
        }
        finally
        {
            if (gateAcquired)
                _recordsMutationContext.RunTrainingIntakeGate.Release();
        }
    }

    private async Task<FileStream> AcquireTrainingIntakeRunMutationLeaseAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(_rootDirectory);

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                return new FileStream(
                    _trainingIntakeRunMutationLockPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.Asynchronous);
            }
            catch (IOException ex) when (IsFileSharingOrLockViolation(ex))
            {
                await Task.Delay(TrainingIntakeRunMutationLockRetryDelayMs, ct);
            }
        }
    }

    private static bool IsFileSharingOrLockViolation(IOException ex)
    {
        var nativeErrorCode = ex.HResult & 0xFFFF;
        return nativeErrorCode is 32 or 33;
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeCoreAsync(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionTrainingPipelineOptions runOptions,
        bool optionsProvided,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionTrainingIntakeRunResult>();

        var replay = await ResolveRunTrainingIntakeIdempotencyReplayAsync(
            intake,
            runOptions,
            admissionBoundaryMetadata,
            ct);
        if (!replay.IsSuccess)
            return replay.MapError<DesignConversionTrainingIntakeRunResult>();
        if (replay.Value!.HasReplay)
            return OperationResult<DesignConversionTrainingIntakeRunResult>.Ok(replay.Value.Result!);
        if (!CanReserveMaximumTrainingIntakeRunJournalRow())
            return TrainingIntakeRunsQuotaExceeded<DesignConversionTrainingIntakeRunResult>();

        if (!preflight.MayStartTraining)
        {
            return await StoreRunTrainingIntakeIdempotencyResultAsync(
                BuildTrainingIntakeRunResult(
                    intake,
                    preflight,
                    runOptions,
                    optionsProvided: optionsProvided,
                    resolvedTrainingRecordCount: 0,
                    runnerResult: null,
                    blockReasons: preflight.BlockReasons,
                    trainingExecutionBoundary: "blocked_before_runner_by_training_intake_preflight"),
                intake,
                runOptions,
                admissionBoundaryMetadata,
                ct);
        }

        var resolved = await ResolveTrainingIntakeRecordsAsync(intake, ct);
        if (!resolved.IsSuccess)
            return resolved.MapError<DesignConversionTrainingIntakeRunResult>();

        if (resolved.Value!.BlockReasons.Count > 0)
        {
            return await StoreRunTrainingIntakeIdempotencyResultAsync(
                BuildTrainingIntakeRunResult(
                    intake,
                    preflight,
                    runOptions,
                    optionsProvided: optionsProvided,
                    resolvedTrainingRecordCount: resolved.Value.Records.Count,
                    runnerResult: null,
                    blockReasons: resolved.Value.BlockReasons,
                    trainingExecutionBoundary: "blocked_before_runner_by_training_intake_resolution"),
                intake,
                runOptions,
                admissionBoundaryMetadata,
                ct);
        }

        _runTrainingIntakeRunnerBeforeInvokeObserverForTesting?.Invoke();
        var runner = await new DesignConversionTrainingRunner().RunTrainingRecordsAsync(
            new DesignConversionMachine(),
            resolved.Value.Records,
            runOptions,
            ct);
        if (!runner.IsSuccess)
            return runner.MapError<DesignConversionTrainingIntakeRunResult>();

        return await StoreRunTrainingIntakeIdempotencyResultAsync(
            BuildTrainingIntakeRunResult(
                intake,
                preflight,
                runOptions,
                optionsProvided: optionsProvided,
                resolvedTrainingRecordCount: resolved.Value.Records.Count,
                runnerResult: runner.Value,
                blockReasons: [],
                trainingExecutionBoundary: BuildTrainingExecutionBoundary(runner.Value!)),
            intake,
            runOptions,
            admissionBoundaryMetadata,
            ct);
    }

    public async Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
        string intakeId,
        string promotionOutputDirectory,
        DesignConversionTrainingPipelineOptions? options = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionTrainingIntakeLocalPromotionResult>();
        if (string.IsNullOrWhiteSpace(promotionOutputDirectory))
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Fail(
                "DesignConversion local promotion requires an output directory.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_OUTPUT_DIRECTORY_REQUIRED");

        var preflight = await RunTrainingIntakePreflightAsync(intakeId, ct);
        if (!preflight.IsSuccess)
            return preflight.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        var intake = await FindQueuedTrainingIntakeAsync(intakeId, ct);
        if (!intake.IsSuccess)
            return intake.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        var runOptions = options ?? BuildDefaultTrainingIntakeRunOptions(intake.Value!);
        var admissionBoundaryMetadata = DesignConversionTrainingIntakeRouteAdmissionContext.Current;
        var gateAcquired = false;
        try
        {
            await WaitForObservedGateAsync(
                _recordsMutationContext.RunTrainingIntakeLocalPromotionGate,
                _runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting,
                () => gateAcquired = true,
                ct);

            if (_runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting is not null)
                await _runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting(ct);

            await using var mutationLease = await AcquireTrainingIntakeLocalPromotionMutationLeaseAsync(ct);
            return await RunTrainingIntakeAndPromoteLocalCoreAsync(
                intake.Value!,
                preflight.Value!,
                runOptions,
                optionsProvided: options is not null,
                promotionOutputDirectory,
                admissionBoundaryMetadata,
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionTrainingIntakeLocalPromotionResult>();
        }
        catch (IOException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeLocalPromotionResult>();
        }
        catch (UnauthorizedAccessException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeLocalPromotionResult>();
        }
        finally
        {
            if (gateAcquired)
                _recordsMutationContext.RunTrainingIntakeLocalPromotionGate.Release();
        }
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalCoreAsync(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionTrainingPipelineOptions runOptions,
        bool optionsProvided,
        string promotionOutputDirectory,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        CancellationToken ct)
    {
        var replay = await ResolvePromoteLocalIdempotencyReplayAsync(
            intake,
            runOptions,
            promotionOutputDirectory,
            admissionBoundaryMetadata,
            ct);
        if (!replay.IsSuccess)
            return replay.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();
        if (replay.Value!.HasReplay)
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Ok(replay.Value.Result!);
        if (HasPromoteLocalIdempotencyBoundary(admissionBoundaryMetadata)
            && !CanReserveMaximumTrainingIntakeLocalPromotionJournalRow())
        {
            return TrainingIntakeLocalPromotionsQuotaExceeded<DesignConversionTrainingIntakeLocalPromotionResult>();
        }

        if (!preflight.MayStartTraining)
        {
            var blockedRun = BuildTrainingIntakeRunResult(
                intake,
                preflight,
                runOptions,
                optionsProvided: optionsProvided,
                resolvedTrainingRecordCount: 0,
                runnerResult: null,
                blockReasons: preflight.BlockReasons,
                trainingExecutionBoundary: "blocked_before_runner_by_training_intake_preflight");

            return await StorePromoteLocalIdempotencyResultAsync(
                BuildTrainingIntakeLocalPromotionResult(
                    blockedRun,
                    promotionOutputDirectory,
                    promotedCheckpointPath: string.Empty,
                    publication: null,
                    promotedLocalModel: null,
                    additionalBlockReasons: ["local_promotion_blocked_training_intake_preflight"],
                    additionalWarnings: ["local_promotion_not_attempted"],
                    additionalEvidenceRefs: ["DESIGN_CONVERSION_LOCAL_PROMOTION_BLOCKED_BEFORE_TRAINING_RUNNER"]),
                intake,
                runOptions,
                promotionOutputDirectory,
                admissionBoundaryMetadata,
                ct);
        }

        var resolved = await ResolveTrainingIntakeRecordsAsync(intake, ct);
        if (!resolved.IsSuccess)
            return resolved.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        if (resolved.Value!.BlockReasons.Count > 0)
        {
            var blockedRun = BuildTrainingIntakeRunResult(
                intake,
                preflight,
                runOptions,
                optionsProvided: optionsProvided,
                resolvedTrainingRecordCount: resolved.Value.Records.Count,
                runnerResult: null,
                blockReasons: resolved.Value.BlockReasons,
                trainingExecutionBoundary: "blocked_before_runner_by_training_intake_resolution");

            return await StorePromoteLocalIdempotencyResultAsync(
                BuildTrainingIntakeLocalPromotionResult(
                    blockedRun,
                    promotionOutputDirectory,
                    promotedCheckpointPath: string.Empty,
                    publication: null,
                    promotedLocalModel: null,
                    additionalBlockReasons: ["local_promotion_blocked_training_intake_resolution"],
                    additionalWarnings: ["local_promotion_not_attempted"],
                    additionalEvidenceRefs: ["DESIGN_CONVERSION_LOCAL_PROMOTION_BLOCKED_BY_TRAINING_INTAKE_RESOLUTION"]),
                intake,
                runOptions,
                promotionOutputDirectory,
                admissionBoundaryMetadata,
                ct);
        }

        var machine = new DesignConversionMachine();
        _runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting?.Invoke();
        var runner = await new DesignConversionTrainingRunner().RunTrainingRecordsAsync(
            machine,
            resolved.Value.Records,
            runOptions,
            ct);
        if (!runner.IsSuccess)
            return runner.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        var trainingRun = BuildTrainingIntakeRunResult(
            intake,
            preflight,
            runOptions,
            optionsProvided: optionsProvided,
            resolvedTrainingRecordCount: resolved.Value.Records.Count,
            runnerResult: runner.Value,
            blockReasons: [],
            trainingExecutionBoundary: BuildTrainingExecutionBoundary(runner.Value!));

        if (!trainingRun.ModelUploadReady)
        {
            return await StorePromoteLocalIdempotencyResultAsync(
                BuildTrainingIntakeLocalPromotionResult(
                    trainingRun,
                    promotionOutputDirectory,
                    promotedCheckpointPath: string.Empty,
                    publication: null,
                    promotedLocalModel: null,
                    additionalBlockReasons: ["local_promotion_blocked_model_upload_not_ready"],
                    additionalWarnings: ["local_promotion_not_attempted"],
                    additionalEvidenceRefs: ["DESIGN_CONVERSION_LOCAL_PROMOTION_BLOCKED_BEFORE_PROMOTION_BY_GATE"]),
                intake,
                runOptions,
                promotionOutputDirectory,
                admissionBoundaryMetadata,
                ct);
        }

        var outputDirectory = Path.GetFullPath(promotionOutputDirectory);
        var checkpointPath = Path.Combine(
            outputDirectory,
            $"{ToSafeLocalPromotionFileName(trainingRun.RunId)}-promoted-checkpoint.json");
        Directory.CreateDirectory(outputDirectory);

        var bridge = await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
            resolved.Value.Records,
            outputDirectory,
            trainingRun.RunId,
            checkpointPath,
            ct);
        if (!bridge.IsSuccess)
            return bridge.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        var artifacts = await WriteLocalPromotionArtifactsAsync(outputDirectory, checkpointPath, trainingRun.RunId, ct);
        if (!artifacts.IsSuccess)
            return artifacts.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        var gateReport = BuildLocalPromotionGateReport(runOptions, runner.Value!.PipelineResult, artifacts.Value!);
        var publicationUploader = _promotionUploader ?? new LocalOnlyTrainingIntakePromotionUploader();
        var publication = await new DesignConversionPromotionPublisher(publicationUploader).PublishAsync(
            bridge.Value!.RuntimeMachine,
            runner.Value.PipelineResult,
            $"{ToSafeLocalPromotionFileName(trainingRun.RunId)}-local-promotion-manifest",
            artifacts.Value!,
            gateReport,
            ct);
        if (!publication.IsSuccess)
            return publication.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        if (!publication.Value!.Decision.AllowsUpload)
        {
            return await StorePromoteLocalIdempotencyResultAsync(
                BuildTrainingIntakeLocalPromotionResult(
                    trainingRun,
                    outputDirectory,
                    checkpointPath,
                    publication.Value,
                    promotedLocalModel: null,
                    additionalBlockReasons: publication.Value.Decision.Reasons,
                    additionalWarnings: ["local_promotion_rejected_by_promotion_gate"],
                    additionalEvidenceRefs: publication.Value.Decision.EvidenceRefs),
                intake,
                runOptions,
                promotionOutputDirectory,
                admissionBoundaryMetadata,
                ct);
        }

        if (publication.Value.Manifest.ModelServerUploadAttempted)
        {
            return await StorePromoteLocalIdempotencyResultAsync(
                BuildTrainingIntakeLocalPromotionResult(
                    trainingRun,
                    outputDirectory,
                    checkpointPath,
                    publication.Value,
                    promotedLocalModel: null,
                    additionalBlockReasons: ["local_promotion_not_loaded_after_verified_model_server_upload"],
                    additionalWarnings: ["model_server_upload_verified_local_activation_not_attempted"],
                    additionalEvidenceRefs:
                    [
                        "DESIGN_CONVERSION_MODEL_SERVER_UPLOAD_VERIFIED_BY_CONFIGURED_UPLOADER",
                        "DESIGN_CONVERSION_LOCAL_MODEL_LOAD_NOT_ATTEMPTED_FOR_REMOTE_PROMOTION_RECEIPT"
                    ]),
                intake,
                runOptions,
                promotionOutputDirectory,
                admissionBoundaryMetadata,
                ct);
        }

        var promoted = await DesignConversionPromotedLocalModelLoader.LoadAsync(
            publication.Value,
            checkpointPath,
            ct);
        if (!promoted.IsSuccess)
            return promoted.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        return await StorePromoteLocalIdempotencyResultAsync(
            BuildTrainingIntakeLocalPromotionResult(
                trainingRun,
                outputDirectory,
                checkpointPath,
                publication.Value,
                promoted.Value!.Model,
                additionalBlockReasons: [],
                additionalWarnings: ["local_promotion_ready_no_external_cloud_upload"],
                additionalEvidenceRefs: promoted.Value.Model.EvidenceRefs
                    .Concat(bridge.Value.EvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()),
            intake,
            runOptions,
            promotionOutputDirectory,
            admissionBoundaryMetadata,
            ct);
    }

    public async Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
        string promotedLocalModelRef,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();
        if (string.IsNullOrWhiteSpace(promotedLocalModelRef))
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Fail(
                "DesignConversion local promotion rollback requires a promoted local model reference.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_REF_REQUIRED");
        if (string.IsNullOrWhiteSpace(requestedByRef))
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Fail(
                "DesignConversion local promotion rollback requires requestedByRef.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_REQUESTED_BY_REQUIRED");
        if (string.IsNullOrWhiteSpace(reason))
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Fail(
                "DesignConversion local promotion rollback requires an evidence-bound reason.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_REASON_REQUIRED");

        var admissionBoundaryMetadata = DesignConversionTrainingIntakeRouteAdmissionContext.Current;
        var gateAcquired = false;
        try
        {
            await WaitForObservedGateAsync(
                _recordsMutationContext.RunTrainingIntakeLocalPromotionGate,
                _runTrainingIntakeLocalPromotionGateWaitTaskObserverForTesting,
                () => gateAcquired = true,
                ct);

            if (_runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting is not null)
                await _runTrainingIntakeLocalPromotionGateAcquiredCallbackForTesting(ct);

            await using var mutationLease = await AcquireTrainingIntakeLocalPromotionMutationLeaseAsync(ct);
            var records = await ReadTrainingIntakeLocalPromotionRecordsAsync(ct);
            if (!records.IsSuccess)
                return records.MapError<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();

            var target = records.Value!
                .LastOrDefault(record => MatchesLocalPromotionRef(record, promotedLocalModelRef));
            if (target is null)
                return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Fail(
                    "DesignConversion local promotion rollback could not find the promoted local model reference.",
                    "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_TARGET_NOT_FOUND");
            if (!target.LocalPromotionReady)
                return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Fail(
                    "DesignConversion local promotion rollback target is not an active-ready local promotion.",
                    "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_TARGET_NOT_READY");

            var targetRef = BuildLocalPromotionActiveRef(target);
            var replay = ResolveRollbackLocalPromotionIdempotencyReplay(
                records.Value!,
                targetRef,
                requestedByRef,
                reason,
                requestedAtUtc,
                admissionBoundaryMetadata);
            if (!replay.IsSuccess)
                return replay.MapError<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();
            if (replay.Value!.HasReplay)
                return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Ok(replay.Value.Result!);

            var current = ResolveCurrentLocalPromotionActiveState(records.Value!);
            if (!string.Equals(current.ActiveLocalModelRef, targetRef, StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Fail(
                    "DesignConversion local promotion rollback target is not the current active local promotion.",
                    "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_TARGET_NOT_ACTIVE");
            }

            var priorRef = ReadMetadata(target.BoundaryMetadata, "local_promotion_prior_active_local_model_ref");
            var priorModelId = ReadMetadata(target.BoundaryMetadata, "local_promotion_prior_active_model_id");
            var priorCheckpoint = ReadMetadata(target.BoundaryMetadata, "local_promotion_prior_active_checkpoint_sha256");
            var noPrior = string.Equals(
                ReadMetadata(target.BoundaryMetadata, "local_promotion_prior_state_kind"),
                "none",
                StringComparison.Ordinal);
            if (noPrior)
            {
                return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Fail(
                    "DesignConversion local promotion rollback target has no prior active local promotion to restore.",
                    "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_PRIOR_STATE_NOT_RECORDED");
            }

            if (string.IsNullOrWhiteSpace(priorRef) && !noPrior)
            {
                return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Fail(
                    "DesignConversion local promotion rollback target has no recorded prior active local state.",
                    "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_PRIOR_STATE_MISSING");
            }

            if (!CanReserveMaximumTrainingIntakeLocalPromotionJournalRow())
            {
                return TrainingIntakeLocalPromotionsQuotaExceeded<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();
            }

            var rollbackAt = requestedAtUtc ?? DateTimeOffset.UtcNow;
            var rollbackId = BuildLocalPromotionRollbackId(
                targetRef,
                priorRef,
                requestedByRef,
                reason,
                rollbackAt);
            var rollbackRecord = BuildLocalPromotionRollbackRecord(
                target,
                rollbackId,
                targetRef,
                current.ActiveLocalModelRef,
                priorRef,
                priorModelId,
                priorCheckpoint,
                requestedByRef,
                reason,
                rollbackAt);
            rollbackRecord = MarkRollbackLocalPromotionIdempotencyBoundary(
                rollbackRecord,
                targetRef,
                requestedByRef,
                reason,
                requestedAtUtc,
                admissionBoundaryMetadata,
                replayHit: false,
                createdNewSideEffect: true);

            var line = JsonSerializer.Serialize(rollbackRecord, JsonOptions);
            var framedLineByteLength = StrictUtf8Encoding.GetByteCount(line + Environment.NewLine);
            if (framedLineByteLength > MaxTrainingIntakeLocalPromotionJournalRowByteLength
                || !TryProjectTrainingIntakeLocalPromotionsLength(framedLineByteLength, out var projectedLength)
                || projectedLength > _trainingIntakeLocalPromotionsMaxBytes)
            {
                return TrainingIntakeLocalPromotionsQuotaExceeded<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();
            }

            Directory.CreateDirectory(_rootDirectory);
            await DurableCopyOnWriteAppendJsonLineAsync(_trainingIntakeLocalPromotionPath, line, ct);

            return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>.Ok(
                BuildLocalPromotionRollbackResult(rollbackRecord));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();
        }
        catch (IOException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();
        }
        catch (UnauthorizedAccessException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();
        }
        finally
        {
            if (gateAcquired)
                _recordsMutationContext.RunTrainingIntakeLocalPromotionGate.Release();
        }
    }

    private async Task<FileStream> AcquireTrainingIntakeLocalPromotionMutationLeaseAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(_rootDirectory);

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                return new FileStream(
                    _trainingIntakeLocalPromotionMutationLockPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.Asynchronous);
            }
            catch (IOException ex) when (IsFileSharingOrLockViolation(ex))
            {
                await Task.Delay(TrainingIntakeLocalPromotionMutationLockRetryDelayMs, ct);
            }
        }
    }

    public async Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
        string localArtifactRecordId,
        string candidateOrTrainingRecordId,
        string reviewerRef,
        string reason,
        DateTimeOffset? approvedAtUtc = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionDpoCandidateApprovalResult>();
        if (string.IsNullOrWhiteSpace(localArtifactRecordId))
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion local artifact record id is required for DPO candidate approval.",
                "DESIGN_CONVERSION_LOCAL_ARTIFACT_RECORD_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(candidateOrTrainingRecordId))
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion DPO candidate or training record id is required for approval.",
                "DESIGN_CONVERSION_DPO_CANDIDATE_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(reviewerRef))
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion DPO candidate approval requires a local reviewer reference.",
                "DESIGN_CONVERSION_DPO_APPROVAL_REVIEWER_REQUIRED");
        if (string.IsNullOrWhiteSpace(reason))
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion DPO candidate approval requires an evidence-bound reason.",
                "DESIGN_CONVERSION_DPO_APPROVAL_REASON_REQUIRED");

        try
        {
            await _recordsMutationContext.Gate.WaitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return Cancelled<DesignConversionDpoCandidateApprovalResult>();
        }

        try
        {
            return await ApproveRuntimeCreatedDpoCandidateCoreAsync(
                localArtifactRecordId,
                candidateOrTrainingRecordId,
                reviewerRef,
                reason,
                approvedAtUtc,
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoCandidateApprovalResult>();
        }
        catch (IOException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionDpoCandidateApprovalResult>();
        }
        catch (UnauthorizedAccessException)
        {
            return LocalArtifactDbWriteFailed<DesignConversionDpoCandidateApprovalResult>();
        }
        finally
        {
            _recordsMutationContext.Gate.Release();
        }
    }

    private async Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateCoreAsync(
        string localArtifactRecordId,
        string candidateOrTrainingRecordId,
        string reviewerRef,
        string reason,
        DateTimeOffset? approvedAtUtc,
        CancellationToken ct)
    {
        var recordsResult = await ReadRecordsCoreAsync(ct);
        if (!recordsResult.IsSuccess)
            return recordsResult.MapError<DesignConversionDpoCandidateApprovalResult>();

        var records = recordsResult.Value!.ToArray();
        var matchingRecordIndexes = records
            .Select((record, index) => new { record, index })
            .Where(item => string.Equals(item.record.RecordId, localArtifactRecordId, StringComparison.Ordinal))
            .ToArray();
        if (matchingRecordIndexes.Length == 0)
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion local artifact record was not found for DPO candidate approval.",
                "DESIGN_CONVERSION_LOCAL_ARTIFACT_RECORD_NOT_FOUND");
        if (matchingRecordIndexes.Length > 1)
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion local artifact record id is ambiguous for DPO candidate approval.",
                "DESIGN_CONVERSION_LOCAL_ARTIFACT_RECORD_AMBIGUOUS");

        var recordIndex = matchingRecordIndexes[0].index;
        var record = records[recordIndex];
        if (!string.Equals(record.DatabaseKind, LocalArtifactDbKind, StringComparison.Ordinal))
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion DPO candidate approval is only allowed for local artifact DB records.",
                "DESIGN_CONVERSION_DPO_APPROVAL_NON_LOCAL_RECORD");

        var candidates = record.DpoCandidates.ToArray();
        var matchingCandidateIndexes = candidates
            .Select((candidate, index) => new { candidate, index })
            .Where(item => MatchesCandidateApprovalIdentity(item.candidate, candidateOrTrainingRecordId))
            .ToArray();
        if (matchingCandidateIndexes.Length == 0)
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion runtime-created DPO candidate was not found for approval.",
                "DESIGN_CONVERSION_DPO_CANDIDATE_NOT_FOUND");
        if (matchingCandidateIndexes.Length > 1)
            return OperationResult<DesignConversionDpoCandidateApprovalResult>.Fail(
                "DesignConversion runtime-created DPO candidate id is ambiguous for approval.",
                "DESIGN_CONVERSION_DPO_CANDIDATE_AMBIGUOUS");

        var candidateIndex = matchingCandidateIndexes[0].index;
        var candidate = candidates[candidateIndex];
        var validation = ValidateApprovalCandidate(record, candidate);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionDpoCandidateApprovalResult>();

        var approval = BuildApprovedCandidate(
            record,
            candidate,
            reviewerRef,
            reason,
            approvedAtUtc ?? DateTimeOffset.UtcNow);

        candidates[candidateIndex] = approval.Candidate;
        records[recordIndex] = ApplyApprovalToRecordMetadata(record, candidates, approval.Result);

        var write = await WriteRecordsCoreAsync(records, ct);
        if (!write.IsSuccess)
            return write.MapError<DesignConversionDpoCandidateApprovalResult>();

        return OperationResult<DesignConversionDpoCandidateApprovalResult>.Ok(
            write.Value
                ? approval.Result
                : WithDeferredIndexEvidence(approval.Result));
    }

    public async Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
        DesignConversionDpoDisplayQuery? query = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionDpoDisplayResult>.Fail(
                "DesignConversion local artifact DB query cancelled.",
                "CANCELLED");

        var records = await ListRecordsAsync(null, ct);
        if (!records.IsSuccess)
            return records.MapError<DesignConversionDpoDisplayResult>();

        var trainingRecords = records.Value!
            .SelectMany(record => record.DpoCandidates)
            .Select(candidate => candidate.TrainingRecord)
            .ToArray();
        var candidateMetadataByDisplayId = BuildCandidateMetadataByDisplayId(records.Value!);

        var display = new DesignConversionDpoDisplayBuilder().BuildDisplay(trainingRecords, query);
        if (!display.IsSuccess)
            return display;

        return OperationResult<DesignConversionDpoDisplayResult>.Ok(
            ApplyRuntimeReviewBoundary(display.Value!, candidateMetadataByDisplayId));
    }

    private static DesignConversionDpoDisplayResult ApplyRuntimeReviewBoundary(
        DesignConversionDpoDisplayResult display,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> candidateMetadataByDisplayId)
    {
        var rows = display.Rows
            .Select(row => ApplyRuntimeReviewBoundary(row, candidateMetadataByDisplayId))
            .ToArray();
        var hasRuntimeReviewPending = rows.Any(row =>
            string.Equals(
                ReadMetadata(row.BoundaryMetadata, "review_status"),
                RuntimeCandidateReviewPendingStatus,
                StringComparison.Ordinal));
        var hasRuntimeReviewApproved = rows.Any(row =>
            string.Equals(
                ReadMetadata(row.BoundaryMetadata, "review_status"),
                RuntimeCandidateReviewApprovedStatus,
                StringComparison.Ordinal));

        return display with
        {
            Rows = rows,
            Warnings = display.Warnings
                .Concat(hasRuntimeReviewPending
                    ?
                    [
                        "runtime_created_dpo_candidate_review_pending",
                        "runtime_created_dpo_candidate_approval_required",
                        "approved_for_training=false",
                        "production_dpo=false",
                        "model_readiness_claimed=false"
                    ]
                    : [])
                .Concat(hasRuntimeReviewApproved
                    ?
                    [
                        "runtime_created_dpo_candidate_approved_for_training",
                        "approval_status=approved",
                        "approved_for_training=true",
                        "production_dpo=false",
                        "model_readiness_claimed=false"
                    ]
                    : [])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static DesignConversionDpoDisplayRow ApplyRuntimeReviewBoundary(
        DesignConversionDpoDisplayRow row,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> candidateMetadataByDisplayId)
    {
        if (!IsRuntimeCreatedReviewRow(row.BoundaryMetadata))
            return row;

        var approvalMetadata = candidateMetadataByDisplayId.TryGetValue(row.DisplayId, out var candidateMetadata)
            ? candidateMetadata
            : row.BoundaryMetadata;
        var approved = IsApprovedReviewMetadata(approvalMetadata);
        var approvalStatus = approved ? "approved" : "pending";
        var reviewStatus = approved ? RuntimeCandidateReviewApprovedStatus : RuntimeCandidateReviewPendingStatus;
        var readinessBoundary = approved ? row.ReadinessBoundary : RuntimeCandidateReviewPendingStatus;
        var metadata = new Dictionary<string, string>(row.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["approval_status"] = approvalStatus,
            ["review_status"] = reviewStatus,
            ["approved_for_training"] = approved.ToString(),
            ["training_candidate_only"] = row.TrainingCandidateOnly.ToString(),
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["heldout"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["api_train_ready"] = approved.ToString()
        };
        CopyApprovalMetadata(approvalMetadata, metadata, "approved_by_ref");
        CopyApprovalMetadata(approvalMetadata, metadata, "approval_reason");
        CopyApprovalMetadata(approvalMetadata, metadata, "approved_at_utc");
        CopyApprovalMetadata(approvalMetadata, metadata, "local_reviewer_approval_evidence");

        return row with
        {
            ReadinessBoundary = readinessBoundary,
            UsefulnessFactors = row.UsefulnessFactors
                .Concat(
                [
                    reviewStatus,
                    "review_required=true",
                    $"approval_status={approvalStatus}",
                    $"approved_for_training={approved.ToString().ToLowerInvariant()}",
                    "training_candidate_only=true",
                    "production_dpo=false",
                    "model_readiness_claimed=false",
                    "held_out_not_proven=true",
                    "source_disjoint_not_proven=true"
                ])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            EvidenceRefs = row.EvidenceRefs
                .Append($"design-conversion-dpo-display:{reviewStatus}")
                .Append("design-conversion-dpo-display:approval_required")
                .Append("design-conversion-dpo-display:not_production_dpo")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            BoundaryMetadata = metadata
        };
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> BuildCandidateMetadataByDisplayId(
        IReadOnlyList<DesignConversionLocalArtifactDbRecord> records)
    {
        var builder = new DesignConversionDpoDisplayBuilder();
        var map = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
        foreach (var candidate in records.SelectMany(record => record.DpoCandidates))
        {
            var display = builder.BuildDisplay(
                [candidate.TrainingRecord],
                new DesignConversionDpoDisplayQuery(IncludeBlocked: true));
            if (!display.IsSuccess || display.Value!.Rows.Count != 1)
                continue;

            var row = display.Value.Rows[0];
            map[row.DisplayId] = candidate.TrainingRecord.Provenance.Metadata;
        }

        return map;
    }

    private static void CopyApprovalMetadata(
        IReadOnlyDictionary<string, string> source,
        IDictionary<string, string> target,
        string key)
    {
        if (source.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            target[key] = value;
    }

    private async Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ReadRecordsCoreAsync(
        CancellationToken ct)
    {
        var scan = await ReadAuthorityRecordLinesCoreAsync(ct);
        if (!scan.IsSuccess)
            return scan.MapError<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

        return OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(
            MaterializeRecoveredAuthorityRecords(scan.Value!));
    }

    private async Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ReadRecordsAndHealDerivedIndexAsync(
        CancellationToken ct,
        bool forceDurableIndexHeal = false)
    {
        var scan = await ReadAuthorityRecordLinesCoreAsync(ct);
        if (!scan.IsSuccess)
            return scan.MapError<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

        var authorityScan = scan.Value!;
        if (forceDurableIndexHeal || await TryReadUsableDurableIndexAsync(authorityScan.Fingerprint, ct) is null)
            await TryWriteDerivedIndexAsync(BuildIndex(authorityScan), ct);

        return OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(
            MaterializeRecoveredAuthorityRecords(authorityScan));
    }

    private static IReadOnlyList<DesignConversionLocalArtifactDbRecord> MaterializeRecoveredAuthorityRecords(
        AuthorityRecordScan scan)
    {
        var quarantineEvidenceValue = scan.QuarantinedLines.Count == 0
            ? string.Empty
            : BuildQuarantinedAuthorityLineEvidenceValue(scan.QuarantinedLines);
        var records = scan.Lines
            .Select(line => line.Record)
            .Select(record => WithLogicalQuarantineRecoveryEvidence(
                record,
                scan.QuarantinedLines.Count,
                quarantineEvidenceValue))
            .ToArray();
        if (!scan.HasUncommittedFinalRecord)
            return records;

        return records
            .Select(record => WithUncommittedFinalRecordRecoveryEvidence(record, scan))
            .ToArray();
    }

    private static DesignConversionLocalArtifactDbRecord WithLogicalQuarantineRecoveryEvidence(
        DesignConversionLocalArtifactDbRecord record,
        int quarantinedLineCount,
        string quarantineEvidenceValue)
    {
        if (quarantinedLineCount == 0)
            return record;

        var metadata = record.Metadata is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(record.Metadata, StringComparer.Ordinal);
        metadata["local_artifact_db_logical_quarantine_recovered"] = bool.TrueString;
        metadata["local_artifact_db_quarantined_authority_line_count"] =
            quarantinedLineCount.ToString(CultureInfo.InvariantCulture);
        metadata["local_artifact_db_quarantined_authority_bytes_preserved"] = bool.TrueString;
        metadata["local_artifact_db_quarantined_authority_lines"] = quarantineEvidenceValue;

        var evidenceRefs = (record.EvidenceRefs ?? Array.Empty<string>())
            .Append(LogicalQuarantinedAuthorityLineMarker)
            .Append(QuarantinedAuthorityBytesPreservedMarker)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return record with
        {
            EvidenceRefs = evidenceRefs,
            Metadata = metadata
        };
    }

    private static DesignConversionLocalArtifactDbRecord WithUncommittedFinalRecordRecoveryEvidence(
        DesignConversionLocalArtifactDbRecord record,
        AuthorityRecordScan scan)
    {
        var metadata = record.Metadata is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(record.Metadata, StringComparer.Ordinal);
        metadata["local_artifact_db_uncommitted_final_record_recovered"] = bool.TrueString;
        metadata["local_artifact_db_uncommitted_final_record_committed"] = bool.FalseString;
        metadata["local_artifact_db_uncommitted_final_record_line"] =
            (scan.UncommittedFinalLineNumber ?? 0).ToString(CultureInfo.InvariantCulture);
        metadata["local_artifact_db_uncommitted_final_record_byte_offset"] =
            (scan.UncommittedFinalByteOffset ?? 0).ToString(CultureInfo.InvariantCulture);
        metadata["local_artifact_db_committed_length_bytes"] = scan.CommittedLength.ToString(CultureInfo.InvariantCulture);
        metadata["local_artifact_db_records_file_length_bytes"] = scan.Fingerprint.Length.ToString(CultureInfo.InvariantCulture);

        var evidenceRefs = (record.EvidenceRefs ?? Array.Empty<string>())
            .Append(UncommittedFinalRecordRecoveredMarker)
            .Append(UncommittedFinalRecordNotCommittedMarker)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return record with
        {
            EvidenceRefs = evidenceRefs,
            Metadata = metadata
        };
    }

    private async Task<OperationResult<AuthorityRecordScan>> ReadAuthorityRecordLinesCoreAsync(
        CancellationToken ct)
    {
        var startFingerprint = GetRecordsFileFingerprint();
        if (!startFingerprint.Exists)
            return OperationResult<AuthorityRecordScan>.Ok(
                new AuthorityRecordScan(
                    [],
                    [],
                    startFingerprint,
                    CommittedLength: 0,
                    NextAppendLineNumber: 1,
                    HasUncommittedFinalRecord: false,
                    UncommittedFinalLineNumber: null,
                    UncommittedFinalByteOffset: null));

        var lines = new List<AuthorityRecordLine>();
        var quarantinedLines = new List<DesignConversionLocalArtifactDbQuarantinedAuthorityLine>();
        try
        {
            await using var stream = new FileStream(
                _recordsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var buffer = new byte[8192];
            using var currentLine = new MemoryStream();
            var lineStartOffset = 0L;
            var lineNumber = 0;
            var committedLength = 0L;
            var hasUncommittedFinalRecord = false;
            int? uncommittedFinalLineNumber = null;
            long? uncommittedFinalByteOffset = null;

            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
                if (read == 0)
                    break;

                for (var index = 0; index < read; index++)
                {
                    if (currentLine.Length >= MaxIndexedRecordLineByteLength)
                        return InvalidAuthorityRecordLine(lineNumber + 1).MapError<AuthorityRecordScan>();

                    currentLine.WriteByte(buffer[index]);
                    if (buffer[index] != (byte)'\n')
                        continue;

                    lineNumber++;
                    var parsed = ParseAuthorityRecordLineBytes(
                        currentLine.ToArray(),
                        lineStartOffset,
                        lineNumber,
                        quarantinedLines.Count < MaxLogicallyQuarantinedAuthorityLinesPerScan);
                    if (!parsed.IsSuccess)
                        return parsed.MapError<AuthorityRecordScan>();
                    if (parsed.Value?.RecordLine is not null)
                        lines.Add(parsed.Value.RecordLine);
                    if (parsed.Value?.QuarantinedLine is not null)
                        quarantinedLines.Add(parsed.Value.QuarantinedLine);

                    lineStartOffset += currentLine.Length;
                    committedLength = lineStartOffset;
                    currentLine.SetLength(0);
                    currentLine.Position = 0;
                }
            }

            if (currentLine.Length > 0)
            {
                hasUncommittedFinalRecord = true;
                uncommittedFinalLineNumber = lineNumber + 1;
                uncommittedFinalByteOffset = lineStartOffset;
            }

            var endFingerprint = GetRecordsFileFingerprint();
            if (!endFingerprint.Equals(startFingerprint))
                return LocalArtifactDbWriteFailed<AuthorityRecordScan>();

            return OperationResult<AuthorityRecordScan>.Ok(new AuthorityRecordScan(
                lines,
                quarantinedLines,
                endFingerprint,
                committedLength,
                NextAppendLineNumber: lineNumber + 1,
                hasUncommittedFinalRecord,
                uncommittedFinalLineNumber,
                uncommittedFinalByteOffset));
        }
        catch (DecoderFallbackException)
        {
            return OperationResult<AuthorityRecordScan>.Fail(
                "DesignConversion local artifact DB records could not be read because the records file is not valid UTF-8.",
                InvalidUtf8ErrorCode);
        }
    }

    private static OperationResult<AuthorityLineParseResult?> ParseAuthorityRecordLineBytes(
        byte[] lineBytes,
        long byteOffset,
        int lineNumber,
        bool canQuarantineLine)
    {
        var line = DecodeAuthorityJsonLine(lineBytes, byteOffset == 0);
        if (string.IsNullOrWhiteSpace(line))
            return OperationResult<AuthorityLineParseResult?>.Ok(null);

        DesignConversionLocalArtifactDbRecord? record;
        try
        {
            record = JsonSerializer.Deserialize<DesignConversionLocalArtifactDbRecord>(
                line,
                JsonOptions);
        }
        catch (JsonException)
        {
            return BuildQuarantinedAuthorityLineParseResult(
                lineBytes,
                byteOffset,
                lineNumber,
                canQuarantineLine);
        }

        if (record is null)
        {
            return BuildQuarantinedAuthorityLineParseResult(
                lineBytes,
                byteOffset,
                lineNumber,
                canQuarantineLine);
        }

        return OperationResult<AuthorityLineParseResult?>.Ok(
            new AuthorityLineParseResult(
                RecordLine: new AuthorityRecordLine(
                    Record: record,
                    CurrentRecordJson: line,
                    ByteOffset: byteOffset,
                    ByteLength: lineBytes.LongLength,
                    LineNumber: lineNumber,
                    AuthorityLineSha256: $"sha256:{HashBytes(lineBytes)}"),
                QuarantinedLine: null));
    }

    private static OperationResult<AuthorityLineParseResult?> BuildQuarantinedAuthorityLineParseResult(
        byte[] lineBytes,
        long byteOffset,
        int lineNumber,
        bool canQuarantineLine)
    {
        if (!canQuarantineLine)
            return LogicalAuthorityQuarantineLimitExceeded(lineNumber);

        return OperationResult<AuthorityLineParseResult?>.Ok(
            new AuthorityLineParseResult(
                RecordLine: null,
                QuarantinedLine: BuildQuarantinedAuthorityLine(lineBytes, byteOffset, lineNumber)));
    }

    private static DesignConversionLocalArtifactDbQuarantinedAuthorityLine BuildQuarantinedAuthorityLine(
        byte[] lineBytes,
        long byteOffset,
        int lineNumber)
        => new(
            PhysicalLineNumber: lineNumber,
            ByteOffset: byteOffset,
            ByteLength: lineBytes.LongLength,
            AuthorityLineSha256: $"sha256:{HashBytes(lineBytes)}");

    private static string BuildQuarantinedAuthorityLineEvidenceValue(
        IReadOnlyList<DesignConversionLocalArtifactDbQuarantinedAuthorityLine> quarantinedLines)
        => string.Join(
            "|",
            quarantinedLines
                .OrderBy(line => line.ByteOffset)
                .ThenBy(line => line.PhysicalLineNumber)
                .Select(line =>
                    $"line={line.PhysicalLineNumber.ToString(CultureInfo.InvariantCulture)};" +
                    $"byte_offset={line.ByteOffset.ToString(CultureInfo.InvariantCulture)};" +
                    $"byte_length={line.ByteLength.ToString(CultureInfo.InvariantCulture)};" +
                    $"sha256_ref={line.AuthorityLineSha256}"));

    private static OperationResult<IReadOnlyList<TRecord>> InvalidRecordLine<TRecord>(int lineNumber)
        => OperationResult<IReadOnlyList<TRecord>>.Fail(
            $"DesignConversion local artifact DB line {lineNumber.ToString(CultureInfo.InvariantCulture)} could not be read.",
            "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_LINE_INVALID");

    private static OperationResult<AuthorityRecordLine?> InvalidAuthorityRecordLine(int lineNumber)
        => OperationResult<AuthorityRecordLine?>.Fail(
            $"DesignConversion local artifact DB line {lineNumber.ToString(CultureInfo.InvariantCulture)} could not be read.",
            "DESIGN_CONVERSION_LOCAL_ARTIFACT_DB_LINE_INVALID");

    private static OperationResult<AuthorityLineParseResult?> LogicalAuthorityQuarantineLimitExceeded(int lineNumber)
        => OperationResult<AuthorityLineParseResult?>.Fail(
            $"DesignConversion local artifact DB logical quarantine limit was exceeded at line {lineNumber.ToString(CultureInfo.InvariantCulture)}.",
            LocalArtifactDbAuthorityLogicalQuarantineLimitExceededErrorCode);

    private async Task<OperationResult<bool>> WriteRecordsCoreAsync(
        IReadOnlyList<DesignConversionLocalArtifactDbRecord> records,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<bool>();

        Directory.CreateDirectory(_rootDirectory);
        var rewrite = await RewriteRecordsAuthorityThroughTempAsync(records, ct);
        if (!rewrite.IsSuccess)
            return rewrite;

        var scan = await ReadAuthorityRecordLinesCoreAsync(ct);
        if (!scan.IsSuccess)
            return scan.MapError<bool>();

        var authorityState = BuildAuthorityState(scan.Value!);
        PublishCachedAuthorityStateFromCommittedAuthority(authorityState);

        var derivedIndexDurable = await TryWriteDerivedIndexAsync(authorityState.Index, ct);
        return OperationResult<bool>.Ok(derivedIndexDurable);
    }

    private async Task<OperationResult<bool>> RewriteRecordsAuthorityThroughTempAsync(
        IReadOnlyList<DesignConversionLocalArtifactDbRecord> records,
        CancellationToken ct)
    {
        var tempPath = BuildRecordsRewriteTempPath();
        try
        {
            await using (var stream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            await using (var writer = new StreamWriter(
                stream,
                StrictUtf8Encoding,
                bufferSize: 4096,
                leaveOpen: true))
            {
                foreach (var record in records)
                {
                    ct.ThrowIfCancellationRequested();
                    var line = JsonSerializer.Serialize(record, JsonOptions);
                    await writer.WriteLineAsync(line.AsMemory(), ct);
                }

                ct.ThrowIfCancellationRequested();
                await writer.FlushAsync(ct);
                await stream.FlushAsync(ct);
            }

            ct.ThrowIfCancellationRequested();
            File.Move(tempPath, _recordsPath, overwrite: true);

            return OperationResult<bool>.Ok(true);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            DeleteRecordsRewriteTempBestEffort(tempPath);
            return Cancelled<bool>();
        }
        catch (IOException)
        {
            DeleteRecordsRewriteTempBestEffort(tempPath);
            return LocalArtifactDbWriteFailed<bool>();
        }
        catch (UnauthorizedAccessException)
        {
            DeleteRecordsRewriteTempBestEffort(tempPath);
            return LocalArtifactDbWriteFailed<bool>();
        }
        finally
        {
            DeleteRecordsRewriteTempBestEffort(tempPath);
        }
    }

    private string BuildRecordsRewriteTempPath()
        => Path.Combine(_rootDirectory, $"{RecordsFileName}.{Guid.NewGuid():N}.rewrite.tmp");

    private static void DeleteRecordsRewriteTempBestEffort(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch (IOException)
        {
            // Best-effort cleanup must not replace the write/cancellation outcome.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup must not replace the write/cancellation outcome.
        }
        catch (System.Security.SecurityException)
        {
            // Best-effort cleanup must not replace the write/cancellation outcome.
        }
    }

    private DesignConversionLocalArtifactDbIndex BuildIndex(AuthorityRecordScan scan)
    {
        var index = BuildIndex(scan.Lines, scan.QuarantinedLines, scan.Fingerprint);
        if (!scan.HasUncommittedFinalRecord)
            return WithoutUncommittedFinalRecordIndexEvidence(index);

        return index with
        {
            EvidenceMarkers = (index.EvidenceMarkers ?? [])
                .Concat(
                [
                    UncommittedFinalRecordRecoveredMarker,
                    UncommittedFinalRecordNotCommittedMarker
                ])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static DesignConversionLocalArtifactDbIndex WithoutUncommittedFinalRecordIndexEvidence(
        DesignConversionLocalArtifactDbIndex index)
        => index with
        {
            EvidenceMarkers = (index.EvidenceMarkers ?? [])
                .Where(marker => !string.Equals(marker, UncommittedFinalRecordRecoveredMarker, StringComparison.Ordinal)
                                 && !string.Equals(marker, UncommittedFinalRecordNotCommittedMarker, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };

    private DesignConversionLocalArtifactDbIndex BuildIndex(
        IReadOnlyList<AuthorityRecordLine> recordLines,
        IReadOnlyList<DesignConversionLocalArtifactDbQuarantinedAuthorityLine> quarantinedLines,
        RecordsFileFingerprint fingerprint)
    {
        var records = recordLines.Select(line => line.Record).ToArray();
        var evidenceMarkers = new List<string>
        {
            StorageTrainingArtifactDbMarker,
            LocalArtifactDbKind,
            ConversionResultMarker,
            RenderArtifactRefsMarker,
            VisualJudgeStatusMarker,
            DpoCandidateMarker,
            UsefulnessScoreMarker,
            QuarantineMarker,
            TrainingCandidateOnlyMarker,
            ProductNotReadyMarker,
            "production_db_ready:false"
        };
        if (quarantinedLines.Count > 0)
        {
            evidenceMarkers.Add(LogicalQuarantinedAuthorityLineMarker);
            evidenceMarkers.Add(QuarantinedAuthorityBytesPreservedMarker);
            evidenceMarkers.Add($"local_artifact_db:quarantined_authority_line_count:{quarantinedLines.Count.ToString(CultureInfo.InvariantCulture)}");
        }

        return new DesignConversionLocalArtifactDbIndex(
            DatabaseKind: LocalArtifactDbKind,
            SchemaVersion: SchemaVersion,
            RecordCount: records.Length,
            ApprovedTrainingCandidateCount: records
                .SelectMany(record => record.DpoCandidates)
                .Count(IsApprovedTrainingCandidate),
            UpdatedAtUtc: DateTimeOffset.UtcNow,
            EvidenceMarkers: evidenceMarkers
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray())
        {
            RootDirectoryBindingHash = BuildRootDirectoryBindingHash(_rootDirectory),
            TenantBindingHash = TenantBindingNotApplicable,
            RecordsFileName = RecordsFileName,
            RecordsFileLength = fingerprint.Exists ? fingerprint.Length : 0,
            RecordsFileLastWriteTimeUtcTicks = fingerprint.Exists ? fingerprint.LastWriteTimeUtc.Ticks : DateTime.MinValue.Ticks,
            RecordLocators = recordLines
                .Select(line => BuildIndexRecordLocator(
                    line.Record,
                    line.ByteOffset,
                    line.ByteLength,
                    line.LineNumber,
                    line.AuthorityLineSha256))
                .ToArray(),
            QuarantinedAuthorityLines = quarantinedLines
                .OrderBy(line => line.ByteOffset)
                .ThenBy(line => line.PhysicalLineNumber)
                .ToArray()
        };
    }

    private static DesignConversionLocalArtifactDbRecord NormalizeRecord(
        DesignConversionLocalArtifactDbRecord record)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var visualStatus = NormalizeVisualJudgeStatus(record.VisualJudgeStatus, redactions);
        var candidates = (record.DpoCandidates ?? [])
            .Select(candidate => NormalizeCandidate(candidate, visualStatus, redactions))
            .ToArray();
        var hasRuntimeCandidate = candidates.Any(IsRuntimeCreatedCandidate);
        var hasApprovedRuntimeCandidate = candidates.Any(candidate =>
            IsRuntimeCreatedCandidate(candidate) && IsReviewApprovedForTraining(candidate));
        var hasPendingRuntimeCandidate = candidates.Any(candidate =>
            IsRuntimeCreatedCandidate(candidate) && !IsReviewApprovedForTraining(candidate));

        var callerMetadata = (record.Metadata ?? new Dictionary<string, string>())
            .Where(pair => !string.Equals(
                pair.Key,
                ImmutableAppendFingerprintMetadataKey,
                StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var metadata = new Dictionary<string, string>(
            DesignConversionDpoLeakageGuard.SanitizeMetadata(callerMetadata, "local_artifact_db_metadata", redactions),
            StringComparer.Ordinal)
        {
            ["storage_training_artifact_db"] = bool.TrueString,
            ["local_artifact_db"] = bool.TrueString,
            ["production_db_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["training_candidate_only"] = candidates.Any(candidate => candidate.TrainingCandidateOnly).ToString(),
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "local_artifact_db_training_candidate_not_training_grade",
            ["runtime_created_dpo_candidate"] = hasRuntimeCandidate.ToString(),
            ["review_required"] = hasRuntimeCandidate.ToString(),
            ["approval_status"] = hasRuntimeCandidate
                ? hasPendingRuntimeCandidate ? "pending" : "approved"
                : "not-required",
            ["approved_for_training"] = hasRuntimeCandidate
                ? hasApprovedRuntimeCandidate.ToString()
                : candidates.Any(IsApprovedTrainingCandidate).ToString(),
            ["production_dpo"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString
        };
        AddTrainingDatabaseDurabilityMetadata(
            metadata,
            SchemaVersion,
            "conversion-results",
            Math.Max(1, candidates.Length),
            (record.SanitizedSourceRefs ?? new Dictionary<string, string>())
                .Values
                .Concat(candidates.Select(candidate => candidate.TrainingRecord.Provenance.SourceManifestRef)),
            [
                record.RecordId,
                record.JobId,
                record.RequestId,
                record.SourceKind,
                candidates.Length.ToString(CultureInfo.InvariantCulture),
                string.Join(",", candidates.Select(candidate => candidate.TrainingRecord.RecordId).Order(StringComparer.Ordinal))
            ]);
        if (redactions.Count > 0)
            metadata["redaction_codes"] = string.Join(",", redactions);

        var normalized = record with
        {
            RecordId = SafeScalar(record.RecordId, "record_id", redactions, "artifact-db-record"),
            DatabaseKind = LocalArtifactDbKind,
            JobId = SafeScalar(record.JobId, "job_id", redactions, "job"),
            RequestId = SafeScalar(record.RequestId, "request_id", redactions, "request"),
            SourceKind = SafeScalar(record.SourceKind, "source_kind", redactions, "unknown"),
            Tier = SafeScalar(record.Tier, "tier", redactions, "unspecified"),
            SanitizedSourceRefs = DesignConversionDpoLeakageGuard.SanitizeMetadata(
                record.SanitizedSourceRefs,
                "source_refs",
                redactions),
            ConversionResult = NormalizeConversionResult(record.ConversionResult, redactions),
            RenderArtifactRefs = (record.RenderArtifactRefs ?? [])
                .Select(render => NormalizeRenderArtifactRef(render, redactions))
                .ToArray(),
            VisualJudgeStatus = visualStatus,
            DpoCandidates = candidates,
            CreatedAtUtc = record.CreatedAtUtc == default ? DateTimeOffset.UtcNow : record.CreatedAtUtc,
            EvidenceRefs = (record.EvidenceRefs ?? [])
                .Select(reference => SafeArtifactRef(reference, "evidence_ref", redactions))
                .Append(StorageTrainingArtifactDbMarker)
                .Append(LocalArtifactDbKind)
                .Append(ConversionResultMarker)
                .Append(RenderArtifactRefsMarker)
                .Append(VisualJudgeStatusMarker)
                .Append(DpoCandidateMarker)
                .Append(UsefulnessScoreMarker)
                .Append(QuarantineMarker)
                .Append(TrainingCandidateOnlyMarker)
                .Append(ProductNotReadyMarker)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Metadata = metadata
        };

        var persistedMetadata = new Dictionary<string, string>(normalized.Metadata, StringComparer.Ordinal)
        {
            [ImmutableAppendFingerprintMetadataKey] = BuildImmutableAppendFingerprint(normalized)
        };
        return normalized with { Metadata = persistedMetadata };
    }

    private static DesignConversionConversionResultArtifact NormalizeConversionResult(
        DesignConversionConversionResultArtifact? result,
        SortedSet<string> redactions)
    {
        result ??= new DesignConversionConversionResultArtifact(
            ResultId: "conversion-result-missing",
            HtmlRef: string.Empty,
            HtmlSha256: string.Empty,
            CssRef: string.Empty,
            CssSha256: string.Empty,
            Metadata: new Dictionary<string, string>());

        var htmlHash = NormalizeSha256(result.HtmlSha256, result.HtmlRef);
        var cssHash = NormalizeSha256(result.CssSha256, result.CssRef);
        var metadata = new Dictionary<string, string>(
            DesignConversionDpoLeakageGuard.SanitizeMetadata(result.Metadata, "conversion_result_metadata", redactions),
            StringComparer.Ordinal)
        {
            ["conversion_result"] = "html_css_refs_or_hashes_only",
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString
        };

        return new DesignConversionConversionResultArtifact(
            ResultId: SafeScalar(result.ResultId, "conversion_result_id", redactions, "conversion-result"),
            HtmlRef: PayloadRef("html_result", result.HtmlRef, redactions),
            HtmlSha256: htmlHash,
            CssRef: PayloadRef("css_result", result.CssRef, redactions),
            CssSha256: cssHash,
            Metadata: metadata);
    }

    private static DesignConversionRenderArtifactDbRef NormalizeRenderArtifactRef(
        DesignConversionRenderArtifactDbRef render,
        SortedSet<string> redactions)
        => new(
            ViewportId: SafeScalar(render.ViewportId, "render_viewport_id", redactions, "viewport"),
            ArtifactId: SafeScalar(render.ArtifactId, "render_artifact_id", redactions, "render-artifact"),
            Kind: SafeScalar(render.Kind, "render_artifact_kind", redactions, "render_artifact"),
            UriRef: SafeArtifactRef(render.UriRef, "render_artifact_uri", redactions),
            Sha256: NormalizeSha256(render.Sha256, render.UriRef),
            ByteLength: Math.Max(0, render.ByteLength),
            Width: Math.Max(0, render.Width),
            Height: Math.Max(0, render.Height),
            Metadata: DesignConversionDpoLeakageGuard.SanitizeMetadata(
                render.Metadata,
                "render_artifact_metadata",
                redactions));

    private static DesignConversionVisualJudgeArtifactDbStatus NormalizeVisualJudgeStatus(
        DesignConversionVisualJudgeArtifactDbStatus? status,
        SortedSet<string> redactions)
    {
        status ??= new DesignConversionVisualJudgeArtifactDbStatus(
            Status: DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            ProviderBoundary: DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            ExternalCallExecuted: false,
            EstimatedCostUsd: null,
            TierDecision: DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            EvidenceRefs: [DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker],
            Metadata: new Dictionary<string, string>());

        var metadata = new Dictionary<string, string>(
            DesignConversionDpoLeakageGuard.SanitizeMetadata(status.Metadata, "visual_judge_metadata", redactions),
            StringComparer.Ordinal)
        {
            ["visual_judge_status"] = SafeScalar(status.Status, "visual_judge_status", redactions, "unknown"),
            ["product_not_ready"] = bool.TrueString
        };

        return status with
        {
            Status = SafeScalar(status.Status, "visual_judge_status", redactions, "unknown"),
            ProviderBoundary = SafeScalar(status.ProviderBoundary, "visual_judge_provider_boundary", redactions, "unknown"),
            TierDecision = SafeScalar(status.TierDecision, "visual_judge_tier_decision", redactions, "unknown"),
            EvidenceRefs = (status.EvidenceRefs ?? [])
                .Select(reference => SafeArtifactRef(reference, "visual_judge_evidence_ref", redactions))
                .Append(VisualJudgeStatusMarker)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Metadata = metadata
        };
    }

    private static DesignConversionLocalArtifactDbDpoCandidate NormalizeCandidate(
        DesignConversionLocalArtifactDbDpoCandidate candidate,
        DesignConversionVisualJudgeArtifactDbStatus visualStatus,
        SortedSet<string> redactions)
    {
        var record = AttachVisualJudgeMetadata(
            SanitizeTrainingRecord(candidate.TrainingRecord, redactions),
            visualStatus);
        var blockReasons = BuildBlockReasons(candidate, record, visualStatus)
            .Select(reason => SafeScalar(reason, "dpo_candidate_block_reason", redactions, "blocked"))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var trainingCandidateOnly = candidate.TrainingCandidateOnly
            && blockReasons.Length == 0
            && IsRecordTrainable(record)
            && IsVisualJudgeTrainingAllowed(visualStatus);

        return candidate with
        {
            CandidateId = SafeScalar(candidate.CandidateId, "dpo_candidate_id", redactions, $"{record.RecordId}:dpo-candidate"),
            RecordId = record.RecordId,
            UnitId = record.UnitId,
            UsefulnessScore = Math.Round(record.UsefulnessScore, 6),
            TrainingCandidateOnly = trainingCandidateOnly,
            ProductNotReady = true,
            AllowsTraining = record.License.AllowsTraining,
            ContainsUserPrivateData = record.License.ContainsUserPrivateData,
            IsQuarantined = record.Quarantine.IsQuarantined,
            QuarantineReason = SafeScalar(record.Quarantine.Reason, "quarantine_reason", redactions, "unknown"),
            BlockReasons = blockReasons,
            TrainingRecord = record
        };
    }

    private static DesignConversionDpoTrainingRecord SanitizeTrainingRecord(
        DesignConversionDpoTrainingRecord record,
        SortedSet<string> redactions)
    {
        var metadata = new Dictionary<string, string>(
            DesignConversionDpoLeakageGuard.SanitizeMetadata(record.Provenance.Metadata, "dpo_provenance_metadata", redactions),
            StringComparer.Ordinal)
        {
            ["storage_training_artifact_db"] = bool.TrueString,
            ["local_artifact_db"] = bool.TrueString,
            ["training_candidate_only"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "local_artifact_db_training_candidate_not_training_grade",
            ["product_not_ready"] = bool.TrueString
        };

        if (redactions.Count > 0)
            metadata["source_encoder_redaction_codes"] = string.Join(",", redactions);

        return record with
        {
            RecordId = SafeScalar(record.RecordId, "dpo_record_id", redactions, "dpo-record"),
            UnitId = SafeScalar(record.UnitId, "dpo_unit_id", redactions, "unknown"),
            InternalUnitStatePayload = DesignConversionDpoLeakageGuard.SanitizeJsonPayload(
                record.InternalUnitStatePayload,
                "dpo_internal_unit_state",
                redactions),
            InternalVectorPayload = (record.InternalVectorPayload ?? [])
                .Where(double.IsFinite)
                .Select(value => Math.Round(value, 6))
                .ToArray(),
            SourceStructurePayload = DesignConversionDpoLeakageGuard.SanitizeJsonPayload(
                record.SourceStructurePayload,
                "dpo_source_structure",
                redactions),
            Chosen = SanitizeTrainingOutput(record.Chosen, "chosen", redactions),
            Rejected = SanitizeTrainingOutput(record.Rejected, "rejected", redactions),
            TieBreakerPayload = DesignConversionDpoLeakageGuard.SanitizeJsonPayload(
                record.TieBreakerPayload,
                "dpo_tie_breaker",
                redactions),
            UsefulnessScore = Math.Round(record.UsefulnessScore, 6),
            NegativeQualityReason = SafeScalar(record.NegativeQualityReason, "negative_quality_reason", redactions, "unknown"),
            NegativeReasonTaxonomy = SafeScalar(record.NegativeReasonTaxonomy, "negative_reason_taxonomy", redactions, "unknown"),
            Provenance = record.Provenance with
            {
                SourceRequestId = SafeArtifactRef(record.Provenance.SourceRequestId, "source_request_ref", redactions),
                SourceKind = SafeScalar(record.Provenance.SourceKind, "source_kind", redactions, "unknown"),
                SourceManifestRef = SafeArtifactRef(record.Provenance.SourceManifestRef, "source_manifest_ref", redactions),
                ReviewerRef = SafeArtifactRef(record.Provenance.ReviewerRef, "reviewer_ref", redactions),
                Metadata = metadata
            },
            License = record.License with
            {
                LicenseId = SafeScalar(record.License.LicenseId, "license_id", redactions, "license"),
                RightsScope = SafeScalar(record.License.RightsScope, "rights_scope", redactions, "unknown"),
                Metadata = DesignConversionDpoLeakageGuard.SanitizeMetadata(
                    record.License.Metadata,
                    "license_metadata",
                    redactions)
            },
            Quarantine = record.Quarantine with
            {
                Reason = SafeScalar(record.Quarantine.Reason, "quarantine_reason", redactions, "unknown"),
                EvidenceRefs = (record.Quarantine.EvidenceRefs ?? [])
                    .Select(reference => SafeArtifactRef(reference, "quarantine_evidence_ref", redactions))
                    .ToArray()
            }
        };
    }

    private static DesignConversionDpoTrainingRecord AttachVisualJudgeMetadata(
        DesignConversionDpoTrainingRecord record,
        DesignConversionVisualJudgeArtifactDbStatus visualStatus)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            ["visual_judge_status"] = visualStatus.Status,
            ["visual_judge_provider_boundary"] = visualStatus.ProviderBoundary,
            ["visual_judge_tier_decision"] = visualStatus.TierDecision,
            ["external_ai_visual_judge_executed"] = visualStatus.ExternalCallExecuted.ToString(),
            ["storage_visual_judge_boundary"] = "persisted_for_training_candidate_filter"
        };

        return record with
        {
            Provenance = record.Provenance with
            {
                Metadata = metadata
            }
        };
    }

    private static DesignConversionImprovementOutput SanitizeTrainingOutput(
        DesignConversionImprovementOutput output,
        string label,
        SortedSet<string> redactions)
        => output with
        {
            OutputId = SafeScalar(output.OutputId, $"{label}_output_id", redactions, $"{label}-output"),
            CandidateId = SafeScalar(output.CandidateId, $"{label}_candidate_id", redactions, $"{label}-candidate"),
            Html = PayloadRef($"{label}_html", output.Html, redactions),
            Css = PayloadRef($"{label}_css", output.Css, redactions),
            Verdict = SanitizeVerdict(output.Verdict, label, redactions),
            QaResults = (output.QaResults ?? [])
                .Select(result => SanitizeQaResult(result, label, redactions))
                .ToArray(),
            EvidenceRefs = (output.EvidenceRefs ?? [])
                .Select(reference => SafeArtifactRef(reference, $"{label}_evidence_ref", redactions))
                .ToArray()
        };

    private static RenderQaResultRecord SanitizeQaResult(
        RenderQaResultRecord result,
        string label,
        SortedSet<string> redactions)
        => result with
        {
            ResultId = SafeScalar(result.ResultId, $"{label}_qa_result_id", redactions, "qa-result"),
            RequestId = SafeArtifactRef(result.RequestId, $"{label}_qa_request_id", redactions),
            Expected = SanitizeImage(result.Expected, $"{label}_expected", redactions),
            Actual = SanitizeImage(result.Actual, $"{label}_actual", redactions),
            Verdict = SanitizeVerdict(result.Verdict, $"{label}_qa", redactions),
            Warnings = (result.Warnings ?? [])
                .Select(warning => SafeScalar(warning, $"{label}_qa_warning", redactions, "warning"))
                .ToArray()
        };

    private static RenderQaImageRecord SanitizeImage(
        RenderQaImageRecord image,
        string label,
        SortedSet<string> redactions)
        => image with
        {
            ImageId = SafeScalar(image.ImageId, $"{label}_image_id", redactions, "image"),
            Uri = SafeArtifactRef(image.Uri, $"{label}_image_uri", redactions),
            Metadata = DesignConversionDpoLeakageGuard.SanitizeMetadata(
                image.Metadata,
                $"{label}_image_metadata",
                redactions)
        };

    private static VisualVerdict SanitizeVerdict(
        VisualVerdict verdict,
        string label,
        SortedSet<string> redactions)
        => verdict with
        {
            VerdictId = SafeScalar(verdict.VerdictId, $"{label}_verdict_id", redactions, "verdict"),
            Defects = (verdict.Defects ?? [])
                .Select(defect => defect with
                {
                    DefectId = SafeScalar(defect.DefectId, $"{label}_defect_id", redactions, "defect"),
                    Category = SafeScalar(defect.Category, $"{label}_defect_category", redactions, "visual"),
                    Severity = SafeScalar(defect.Severity, $"{label}_defect_severity", redactions, "unknown"),
                    Description = SafeScalar(defect.Description, $"{label}_defect_description", redactions, string.Empty),
                    EvidenceImageIds = (defect.EvidenceImageIds ?? [])
                        .Select(imageId => SafeArtifactRef(imageId, $"{label}_defect_image_id", redactions))
                        .ToArray()
                })
                .ToArray(),
            EvidenceRefs = (verdict.EvidenceRefs ?? [])
                .Select(reference => SafeArtifactRef(reference, $"{label}_verdict_evidence_ref", redactions))
                .ToArray()
        };

    private static IReadOnlyList<string> BuildBlockReasons(
        DesignConversionLocalArtifactDbDpoCandidate candidate,
        DesignConversionDpoTrainingRecord record,
        DesignConversionVisualJudgeArtifactDbStatus visualStatus)
    {
        var reasons = new List<string>(candidate.BlockReasons ?? []);
        if (record.Quarantine.IsQuarantined)
            reasons.Add(string.IsNullOrWhiteSpace(record.Quarantine.Reason)
                ? "quarantine"
                : record.Quarantine.Reason);
        if (!record.License.AllowsTraining)
            reasons.Add("license_blocks_training");
        if (record.License.ContainsUserPrivateData)
            reasons.Add("private_data_requires_quarantine");
        if (!IsVisualJudgeTrainingAllowed(visualStatus))
            reasons.Add("provider_missing_or_blocked");
        if (!IsRecordTrainable(record))
            reasons.Add("training_record_not_trainable");

        return reasons;
    }

    private static bool MatchesRecord(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionLocalArtifactDbQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.JobId)
            && !string.Equals(record.JobId, query.JobId, StringComparison.Ordinal))
            return false;
        if (!string.IsNullOrWhiteSpace(query.RequestId)
            && !string.Equals(record.RequestId, query.RequestId, StringComparison.Ordinal))
            return false;
        if (!string.IsNullOrWhiteSpace(query.SourceKind)
            && !string.Equals(record.SourceKind, query.SourceKind, StringComparison.OrdinalIgnoreCase))
            return false;
        if (query.ApprovedTrainingOnly && !record.DpoCandidates.Any(IsApprovedTrainingCandidate))
            return false;

        return true;
    }

    private static bool MatchesCandidate(
        DesignConversionLocalArtifactDbDpoCandidate candidate,
        DesignConversionLocalArtifactDbQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.UnitId) && !MatchesUnit(candidate.UnitId, query.UnitId))
            return false;
        if (query.MinimumUsefulnessScore is not null
            && candidate.UsefulnessScore < query.MinimumUsefulnessScore.Value)
            return false;

        return true;
    }

    private static bool MatchesCandidateApprovalIdentity(
        DesignConversionLocalArtifactDbDpoCandidate candidate,
        string candidateOrTrainingRecordId)
        => string.Equals(candidate.CandidateId, candidateOrTrainingRecordId, StringComparison.Ordinal)
           || string.Equals(candidate.RecordId, candidateOrTrainingRecordId, StringComparison.Ordinal)
           || string.Equals(candidate.TrainingRecord.RecordId, candidateOrTrainingRecordId, StringComparison.Ordinal);

    private static OperationResult<bool> ValidateApprovalCandidate(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionLocalArtifactDbDpoCandidate candidate)
    {
        if (!IsRuntimeCreatedCandidate(candidate))
            return OperationResult<bool>.Fail(
                "Only runtime-created local DesignConversion DPO candidates can be approved by this action.",
                "DESIGN_CONVERSION_DPO_APPROVAL_NON_RUNTIME_CANDIDATE");
        if (candidate.IsQuarantined || candidate.TrainingRecord.Quarantine.IsQuarantined)
            return OperationResult<bool>.Fail(
                "Quarantined DesignConversion DPO candidates cannot be approved for local training-candidate export.",
                "DESIGN_CONVERSION_DPO_APPROVAL_QUARANTINED");
        if (!candidate.TrainingCandidateOnly || !candidate.ProductNotReady)
            return OperationResult<bool>.Fail(
                "DesignConversion DPO approval is limited to local training-candidate-only product-not-ready candidates.",
                "DESIGN_CONVERSION_DPO_APPROVAL_NON_LOCAL_CANDIDATE_STATE");
        if (candidate.BlockReasons.Count > 0)
            return OperationResult<bool>.Fail(
                "Blocked DesignConversion DPO candidates cannot be approved for local training-candidate export.",
                "DESIGN_CONVERSION_DPO_APPROVAL_BLOCKED");
        if (!candidate.AllowsTraining || candidate.ContainsUserPrivateData || !IsRecordTrainable(candidate.TrainingRecord))
            return OperationResult<bool>.Fail(
                "DesignConversion DPO candidate cannot be approved because local training prerequisites are not satisfied.",
                "DESIGN_CONVERSION_DPO_APPROVAL_TRAINING_PREREQUISITE_FAILED");
        if (!IsVisualJudgeTrainingAllowed(record.VisualJudgeStatus))
            return OperationResult<bool>.Fail(
                "DesignConversion DPO candidate cannot be approved while the visual judge boundary blocks local training-candidate export.",
                "DESIGN_CONVERSION_DPO_APPROVAL_VISUAL_BOUNDARY_BLOCKED");

        return OperationResult<bool>.Ok(true);
    }

    private static (DesignConversionLocalArtifactDbDpoCandidate Candidate, DesignConversionDpoCandidateApprovalResult Result) BuildApprovedCandidate(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionLocalArtifactDbDpoCandidate candidate,
        string reviewerRef,
        string reason,
        DateTimeOffset approvedAtUtc)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var approvedAt = approvedAtUtc.ToUniversalTime();
        var approvedAtText = approvedAt.ToString("O", CultureInfo.InvariantCulture);
        var safeReviewerRef = SafeScalar(reviewerRef, "dpo_candidate_approval_reviewer_ref", redactions, "reviewer");
        var safeReason = SafeScalar(reason, "dpo_candidate_approval_reason", redactions, "local-reviewer-approved");
        var provenanceMetadata = ApplyApprovalMetadata(
            candidate.TrainingRecord.Provenance.Metadata,
            safeReviewerRef,
            safeReason,
            approvedAtText,
            redactions);
        var licenseMetadata = ApplyApprovalMetadata(
            candidate.TrainingRecord.License.Metadata,
            safeReviewerRef,
            safeReason,
            approvedAtText,
            redactions);
        var trainingRecord = candidate.TrainingRecord with
        {
            Provenance = candidate.TrainingRecord.Provenance with
            {
                ReviewerRef = safeReviewerRef,
                Metadata = provenanceMetadata
            },
            License = candidate.TrainingRecord.License with
            {
                Metadata = licenseMetadata
            }
        };
        var approvedCandidate = candidate with
        {
            TrainingRecord = trainingRecord
        };
        var evidenceRefs = new[]
        {
            "pass106_local_runtime_candidate_approval",
            "runtime_created_dpo_candidate_approved_for_training",
            "approval_status=approved",
            "approved_for_training=true",
            "training_grade=false",
            "not_training_grade=true",
            "product_not_ready=true",
            "model_readiness_claimed=false",
            "public_api_verified=false",
            "production_db_ready=false"
        };
        var boundaryMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["local_artifact_record_id"] = record.RecordId,
            ["candidate_id"] = approvedCandidate.CandidateId,
            ["training_record_id"] = approvedCandidate.TrainingRecord.RecordId,
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["approval_status"] = "approved",
            ["review_status"] = RuntimeCandidateReviewApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "runtime_approved_handoff_not_training_grade_until_real_training_contract",
            ["approved_by_ref"] = safeReviewerRef,
            ["approval_reason"] = safeReason,
            ["approved_at_utc"] = approvedAtText,
            ["training_candidate_only"] = approvedCandidate.TrainingCandidateOnly.ToString(),
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };
        if (redactions.Count > 0)
            boundaryMetadata["approval_redaction_codes"] = string.Join(",", redactions);

        var result = new DesignConversionDpoCandidateApprovalResult(
            LocalArtifactRecordId: record.RecordId,
            CandidateId: approvedCandidate.CandidateId,
            TrainingRecordId: approvedCandidate.TrainingRecord.RecordId,
            UnitId: approvedCandidate.UnitId,
            UsefulnessScore: approvedCandidate.UsefulnessScore,
            ApprovalStatus: "approved",
            ReviewStatus: RuntimeCandidateReviewApprovedStatus,
            ApprovedForTraining: true,
            ApprovedByRef: safeReviewerRef,
            ApprovalReason: safeReason,
            ApprovedAtUtc: approvedAt,
            EvidenceRefs: evidenceRefs,
            BoundaryMetadata: boundaryMetadata);

        return (approvedCandidate, result);
    }

    private static IReadOnlyDictionary<string, string> ApplyApprovalMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string reviewerRef,
        string reason,
        string approvedAtUtc,
        SortedSet<string> redactions)
    {
        return new Dictionary<string, string>(
            DesignConversionDpoLeakageGuard.SanitizeMetadata(metadata, "dpo_candidate_approval_metadata", redactions),
            StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["approval_status"] = "approved",
            ["review_status"] = RuntimeCandidateReviewApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["approved_by_ref"] = reviewerRef,
            ["approval_reason"] = reason,
            ["approved_at_utc"] = approvedAtUtc,
            ["local_reviewer_approval_evidence"] = "pass106_local_runtime_candidate_approval",
            ["training_candidate_only"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString
        };
    }

    private static DesignConversionLocalArtifactDbRecord ApplyApprovalToRecordMetadata(
        DesignConversionLocalArtifactDbRecord record,
        IReadOnlyList<DesignConversionLocalArtifactDbDpoCandidate> candidates,
        DesignConversionDpoCandidateApprovalResult approval)
    {
        var hasPendingRuntimeCandidate = candidates.Any(candidate =>
            IsRuntimeCreatedCandidate(candidate) && !IsReviewApprovedForTraining(candidate));
        var approvedRuntimeCandidateCount = candidates.Count(candidate =>
            IsRuntimeCreatedCandidate(candidate) && IsReviewApprovedForTraining(candidate));
        var metadata = new Dictionary<string, string>(record.Metadata, StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = candidates.Any(IsRuntimeCreatedCandidate).ToString(),
            ["runtime_created_dpo_candidate_approved_count"] = approvedRuntimeCandidateCount.ToString(),
            ["review_required"] = candidates.Any(IsRuntimeCreatedCandidate).ToString(),
            ["approval_required"] = candidates.Any(IsRuntimeCreatedCandidate).ToString(),
            ["approval_status"] = hasPendingRuntimeCandidate ? "pending" : "approved",
            ["review_status"] = hasPendingRuntimeCandidate
                ? RuntimeCandidateReviewPendingStatus
                : RuntimeCandidateReviewApprovedStatus,
            ["approved_for_training"] = (approvedRuntimeCandidateCount > 0).ToString(),
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "local_artifact_db_training_candidate_not_training_grade",
            ["last_approved_dpo_candidate_id"] = approval.CandidateId,
            ["last_approved_dpo_training_record_id"] = approval.TrainingRecordId,
            ["approved_by_ref"] = approval.ApprovedByRef,
            ["approval_reason"] = approval.ApprovalReason,
            ["approved_at_utc"] = approval.ApprovedAtUtc.ToString("O", CultureInfo.InvariantCulture),
            ["production_dpo"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString
        };

        return record with
        {
            DpoCandidates = candidates,
            EvidenceRefs = record.EvidenceRefs
                .Append("pass106_local_runtime_candidate_approval")
                .Append("runtime_created_dpo_candidate_approved_for_training")
                .Append("approval_status=approved")
                .Append("approved_for_training=true")
                .Append("training_grade=false")
                .Append("not_training_grade=true")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Metadata = metadata
        };
    }

    private static bool IsApprovedTrainingCandidate(DesignConversionLocalArtifactDbDpoCandidate candidate)
        => candidate.TrainingCandidateOnly
           && candidate.ProductNotReady
           && !candidate.IsQuarantined
           && candidate.AllowsTraining
           && !candidate.ContainsUserPrivateData
           && candidate.BlockReasons.Count == 0
           && IsRecordTrainable(candidate.TrainingRecord)
           && IsReviewApprovedForTraining(candidate);

    private static bool IsApprovedRuntimeHandoffCandidate(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionLocalArtifactDbDpoCandidate candidate)
    {
        if (!string.Equals(record.DatabaseKind, LocalArtifactDbKind, StringComparison.Ordinal))
            return false;
        if (!IsRuntimeCreatedCandidate(candidate) || !IsApprovedTrainingCandidate(candidate))
            return false;

        var metadata = candidate.TrainingRecord.Provenance.Metadata;
        return IsApprovedReviewMetadata(metadata)
               && !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "approved_by_ref"))
               && !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "approval_reason"))
               && !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "approved_at_utc"))
               && !IsTruthy(ReadMetadata(metadata, "model_readiness_claimed"))
               && !IsTruthy(ReadMetadata(metadata, "trained_model_ready"))
               && !IsTruthy(ReadMetadata(metadata, "production_dpo"));
    }

    private static DesignConversionApprovedDpoHandoffRow BuildHandoffRow(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionLocalArtifactDbDpoCandidate candidate)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var trainingRecord = candidate.TrainingRecord;
        var metadata = DesignConversionDpoLeakageGuard.SanitizeMetadata(
            trainingRecord.Provenance.Metadata,
            "approved_dpo_handoff_metadata",
            redactions);
        var sourceManifestRef = SafeArtifactRef(
            trainingRecord.Provenance.SourceManifestRef,
            "approved_dpo_handoff_source_manifest_ref",
            redactions);
        var approvedByRef = SafeScalar(
            ReadMetadata(metadata, "approved_by_ref"),
            "approved_dpo_handoff_approved_by_ref",
            redactions,
            "local-reviewer");
        var approvalReason = SafeScalar(
            ReadMetadata(metadata, "approval_reason"),
            "approved_dpo_handoff_approval_reason",
            redactions,
            "local-approval");
        var approvedAtUtc = SafeScalar(
            ReadMetadata(metadata, "approved_at_utc"),
            "approved_dpo_handoff_approved_at_utc",
            redactions,
            string.Empty);
        var internalUnitStateHash = HandoffHashRef("internal-unit-state", trainingRecord.InternalUnitStatePayload);
        var internalVectorHash = HandoffHashRef("internal-vector", BuildInternalVectorHashPayload(trainingRecord));
        var sourceStructureHash = HandoffHashRef("source-structure", trainingRecord.SourceStructurePayload);
        var chosenHash = HandoffHashRef(
            "chosen-payload",
            $"{trainingRecord.Chosen.OutputId}|{trainingRecord.Chosen.CandidateId}|{trainingRecord.Chosen.Html}|{trainingRecord.Chosen.Css}");
        var rejectedHash = HandoffHashRef(
            "rejected-payload",
            $"{trainingRecord.Rejected.OutputId}|{trainingRecord.Rejected.CandidateId}|{trainingRecord.Rejected.Html}|{trainingRecord.Rejected.Css}");
        var tieBreakerHash = HandoffHashRef("tie-breaker", trainingRecord.TieBreakerPayload ?? string.Empty);
        var chosenHtmlRef = SafeArtifactRef(
            trainingRecord.Chosen.Html,
            "approved_dpo_handoff_chosen_html_ref",
            redactions);
        var chosenCssRef = SafeArtifactRef(
            trainingRecord.Chosen.Css,
            "approved_dpo_handoff_chosen_css_ref",
            redactions);
        var rejectedHtmlRef = SafeArtifactRef(
            trainingRecord.Rejected.Html,
            "approved_dpo_handoff_rejected_html_ref",
            redactions);
        var rejectedCssRef = SafeArtifactRef(
            trainingRecord.Rejected.Css,
            "approved_dpo_handoff_rejected_css_ref",
            redactions);
        var designTreeRef = BuildHandoffSourceRef(
            record,
            "design_tree_payload_ref",
            "approved_dpo_handoff_design_tree_ref",
            redactions);
        var originalTreeRef = BuildHandoffSourceRef(
            record,
            "original_tree_payload_ref",
            "approved_dpo_handoff_original_tree_ref",
            redactions);
        var sourceArtifactRefs = BuildHandoffSourceArtifactRefs(record, redactions);
        var renderArtifactRefs = BuildHandoffRenderArtifactRefs(record, redactions);
        var comparisonArtifactRefs = BuildHandoffComparisonArtifactRefs(trainingRecord, redactions);
        var evidenceRefs = record.EvidenceRefs
            .Concat(trainingRecord.Quarantine.EvidenceRefs ?? [])
            .Concat(trainingRecord.Chosen.EvidenceRefs ?? [])
            .Concat(trainingRecord.Rejected.EvidenceRefs ?? [])
            .Append("pass107_approved_dpo_handoff_export")
            .Append("training_handoff_only")
            .Append("training_grade=false")
            .Append("not_training_grade=true")
            .Append("not_trained")
            .Select(reference => SafeArtifactRef(reference, "approved_dpo_handoff_evidence_ref", redactions))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var boundaryMetadata = BuildHandoffRowBoundaryMetadata(
            record,
            candidate,
            approvedByRef,
            approvalReason,
            approvedAtUtc,
            internalUnitStateHash,
            internalVectorHash,
            sourceStructureHash,
            chosenHash,
            rejectedHash,
            tieBreakerHash,
            chosenHtmlRef,
            chosenCssRef,
            rejectedHtmlRef,
            rejectedCssRef,
            designTreeRef,
            originalTreeRef,
            sourceArtifactRefs,
            renderArtifactRefs,
            comparisonArtifactRefs,
            redactions);

        return new DesignConversionApprovedDpoHandoffRow(
            LocalArtifactRecordId: record.RecordId,
            CandidateId: candidate.CandidateId,
            TrainingRecordId: trainingRecord.RecordId,
            UnitId: candidate.UnitId,
            UsefulnessScore: candidate.UsefulnessScore,
            ApprovalStatus: "approved",
            ReviewStatus: RuntimeCandidateReviewApprovedStatus,
            ApprovedForTraining: true,
            TrainingHandoffOnly: true,
            ApprovedByRef: approvedByRef,
            ApprovalReason: approvalReason,
            ApprovedAtUtc: approvedAtUtc,
            SourceManifestRef: sourceManifestRef,
            RecordHash: BuildHandoffRecordHash(
                record,
                candidate,
                internalUnitStateHash,
                internalVectorHash,
                sourceStructureHash,
                chosenHash,
                rejectedHash,
                tieBreakerHash),
            InternalUnitStateHash: internalUnitStateHash,
            SourceStructureHash: sourceStructureHash,
            ChosenPayloadHash: chosenHash,
            RejectedPayloadHash: rejectedHash,
            TieBreakerHash: tieBreakerHash,
            ChosenHtmlRef: chosenHtmlRef,
            ChosenCssRef: chosenCssRef,
            RejectedHtmlRef: rejectedHtmlRef,
            RejectedCssRef: rejectedCssRef,
            DesignTreeRef: designTreeRef,
            OriginalTreeRef: originalTreeRef,
            SourceArtifactRefs: sourceArtifactRefs,
            RenderArtifactRefs: renderArtifactRefs,
            ComparisonArtifactRefs: comparisonArtifactRefs,
            EvidenceRefs: evidenceRefs,
            BoundaryMetadata: boundaryMetadata);
    }

    private static string BuildHandoffSourceRef(
        DesignConversionLocalArtifactDbRecord record,
        string key,
        string scope,
        SortedSet<string> redactions)
        => record.SanitizedSourceRefs is not null
           && record.SanitizedSourceRefs.TryGetValue(key, out var value)
            ? SafeArtifactRef(value, scope, redactions)
            : string.Empty;

    private static IReadOnlyList<string> BuildHandoffSourceArtifactRefs(
        DesignConversionLocalArtifactDbRecord record,
        SortedSet<string> redactions)
        => (record.SanitizedSourceRefs ?? new Dictionary<string, string>(StringComparer.Ordinal))
            .Where(pair => pair.Key.StartsWith("source_artifact:", StringComparison.Ordinal))
            .Select(pair => SafeArtifactRef(pair.Value, "approved_dpo_handoff_source_artifact_ref", redactions))
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildHandoffRenderArtifactRefs(
        DesignConversionLocalArtifactDbRecord record,
        SortedSet<string> redactions)
        => (record.RenderArtifactRefs ?? [])
            .Select(render => !string.IsNullOrWhiteSpace(render.Sha256)
                ? render.Sha256
                : !string.IsNullOrWhiteSpace(render.UriRef)
                    ? render.UriRef
                    : render.ArtifactId)
            .Select(reference => SafeArtifactRef(reference, "approved_dpo_handoff_render_artifact_ref", redactions))
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildHandoffComparisonArtifactRefs(
        DesignConversionDpoTrainingRecord trainingRecord,
        SortedSet<string> redactions)
        => (trainingRecord.Chosen.QaResults ?? [])
            .Concat(trainingRecord.Rejected.QaResults ?? [])
            .SelectMany(result => new[]
            {
                result.ResultId,
                result.RequestId,
                result.Expected.Sha256,
                result.Expected.Uri,
                result.Actual.Sha256,
                result.Actual.Uri,
                result.Verdict.VerdictId
            }.Concat(result.Verdict.EvidenceRefs ?? []))
            .Select(reference => SafeArtifactRef(reference, "approved_dpo_handoff_comparison_artifact_ref", redactions))
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildHandoffWarnings(int rowCount)
    {
        var warnings = new List<string>
        {
            "training_handoff_only",
            "training_grade=false",
            "not_training_grade=true",
            "not_trained",
            "weights_not_produced",
            "model_upload_not_ready",
            "held_out_not_proven",
            "source_disjoint_not_proven",
            "product_not_ready",
            "raw_html_css_tree_private_provider_payloads_not_emitted"
        };
        if (rowCount == 0)
            warnings.Add("no_approved_candidates");

        return warnings
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string> BuildHandoffBoundaryMetadata(int rowCount)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["schema_version"] = ApprovedDpoHandoffSchemaVersion,
            ["approved_candidate_count"] = rowCount.ToString(CultureInfo.InvariantCulture),
            ["training_handoff_only"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "approved_handoff_not_training_grade_until_real_training_contract",
            ["not_trained"] = bool.TrueString,
            ["weights_not_produced"] = bool.TrueString,
            ["model_upload_not_ready"] = bool.TrueString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };
        AddTrainingDatabaseDurabilityMetadata(
            metadata,
            ApprovedDpoHandoffSchemaVersion,
            "approved-dpo-handoff",
            rowCount,
            [],
            [rowCount.ToString(CultureInfo.InvariantCulture)]);

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildHandoffRowBoundaryMetadata(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionLocalArtifactDbDpoCandidate candidate,
        string approvedByRef,
        string approvalReason,
        string approvedAtUtc,
        string internalUnitStateHash,
        string internalVectorHash,
        string sourceStructureHash,
        string chosenHash,
        string rejectedHash,
        string tieBreakerHash,
        string chosenHtmlRef,
        string chosenCssRef,
        string rejectedHtmlRef,
        string rejectedCssRef,
        string designTreeRef,
        string originalTreeRef,
        IReadOnlyList<string> sourceArtifactRefs,
        IReadOnlyList<string> renderArtifactRefs,
        IReadOnlyList<string> comparisonArtifactRefs,
        SortedSet<string> redactions)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["candidate_id"] = candidate.CandidateId,
            ["training_record_id"] = candidate.TrainingRecord.RecordId,
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["approval_status"] = "approved",
            ["review_status"] = RuntimeCandidateReviewApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "approved_handoff_row_not_training_grade_until_real_training_contract",
            ["approved_by_ref"] = approvedByRef,
            ["approval_reason"] = approvalReason,
            ["approved_at_utc"] = approvedAtUtc,
            ["training_candidate_only"] = bool.TrueString,
            ["training_handoff_only"] = bool.TrueString,
            ["not_trained"] = bool.TrueString,
            ["weights_not_produced"] = bool.TrueString,
            ["model_upload_not_ready"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["internal_unit_state_hash"] = internalUnitStateHash,
            ["internal_vector_hash"] = internalVectorHash,
            ["internal_vector_dimensions"] = candidate.TrainingRecord.InternalVectorPayload.Count.ToString(CultureInfo.InvariantCulture),
            ["source_structure_hash"] = sourceStructureHash,
            ["chosen_payload_hash"] = chosenHash,
            ["rejected_payload_hash"] = rejectedHash,
            ["tie_breaker_hash"] = tieBreakerHash,
            ["design_conversion_runtime_artifacts_persisted"] = bool.TrueString,
            ["chosen_html_ref"] = chosenHtmlRef,
            ["chosen_css_ref"] = chosenCssRef,
            ["rejected_html_ref"] = rejectedHtmlRef,
            ["rejected_css_ref"] = rejectedCssRef,
            ["design_tree_ref"] = designTreeRef,
            ["original_tree_ref"] = originalTreeRef,
            ["source_artifact_ref_count"] = sourceArtifactRefs.Count.ToString(CultureInfo.InvariantCulture),
            ["render_artifact_ref_count"] = renderArtifactRefs.Count.ToString(CultureInfo.InvariantCulture),
            ["comparison_artifact_ref_count"] = comparisonArtifactRefs.Count.ToString(CultureInfo.InvariantCulture)
        };
        CopyTrainingRecordSourceEncodingMetadata(candidate.TrainingRecord, metadata, redactions);
        CopyRuntimeAttemptMetadata(record.Metadata, metadata, redactions);
        AddTrainingDatabaseDurabilityMetadata(
            metadata,
            ApprovedDpoHandoffSchemaVersion,
            "approved-dpo-handoff-row",
            1,
            (record.SanitizedSourceRefs ?? new Dictionary<string, string>())
                .Values
                .Concat(
                [
                    candidate.TrainingRecord.Provenance.SourceManifestRef,
                    chosenHtmlRef,
                    chosenCssRef,
                    rejectedHtmlRef,
                    rejectedCssRef,
                    designTreeRef,
                    originalTreeRef
                ])
                .Concat(sourceArtifactRefs)
                .Concat(renderArtifactRefs)
                .Concat(comparisonArtifactRefs),
            [
                record.RecordId,
                candidate.CandidateId,
                candidate.TrainingRecord.RecordId,
                internalUnitStateHash,
                sourceStructureHash,
                chosenHash,
                rejectedHash,
                tieBreakerHash
            ]);
        if (redactions.Count > 0)
            metadata["handoff_redaction_codes"] = string.Join(",", redactions);

        return metadata;
    }

    private static void CopyRuntimeAttemptMetadata(
        IReadOnlyDictionary<string, string>? source,
        IDictionary<string, string> target,
        SortedSet<string> redactions)
    {
        if (source is null)
            return;

        CopyBoundaryMetadata(source, target, redactions,
        [
            "attempt_count",
            "stop_reason",
            "quality_reached",
            "final_candidate_from_loop",
            "comparison_diff_metadata_ref",
            "runtime_created_dpo_candidate_count",
            "runtime_created_dpo_candidate",
            "improvement_loop_to_local_training_store",
            "local_training_store_dpo_source",
            "refs_or_hashes_only",
            "product_not_ready",
            "production_db_ready",
            "model_readiness_claimed"
        ]);
    }

    private static string BuildHandoffArtifactId(
        IReadOnlyList<DesignConversionApprovedDpoHandoffRow> rows)
        => $"approved-dpo-handoff-{HashText(string.Join("|", rows.Select(row => row.RecordHash)))}";

    private static string BuildHandoffRecordHash(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionLocalArtifactDbDpoCandidate candidate,
        string internalUnitStateHash,
        string internalVectorHash,
        string sourceStructureHash,
        string chosenHash,
        string rejectedHash,
        string tieBreakerHash)
        => $"sha256:{HashText(string.Join("|",
        [
            ApprovedDpoHandoffSchemaVersion,
            record.RecordId,
            candidate.CandidateId,
            candidate.TrainingRecord.RecordId,
            candidate.UsefulnessScore.ToString("R", CultureInfo.InvariantCulture),
            internalUnitStateHash,
            internalVectorHash,
            sourceStructureHash,
            chosenHash,
            rejectedHash,
            tieBreakerHash
        ]))}";

    private static string BuildInternalVectorHashPayload(DesignConversionDpoTrainingRecord trainingRecord)
        => string.Join(
            "|",
            (trainingRecord.InternalVectorPayload ?? [])
                .Select(value => Math.Round(value, 6).ToString("R", CultureInfo.InvariantCulture)));

    private static void CopyTrainingRecordSourceEncodingMetadata(
        DesignConversionDpoTrainingRecord trainingRecord,
        IDictionary<string, string> metadata,
        SortedSet<string> redactions)
    {
        CopyBoundaryMetadata(trainingRecord.Provenance.Metadata, metadata, redactions,
        [
            "source_encoder_vector_hash",
            "source_encoder_feature_map_hash",
            "source_encoder_feature_names",
            "source_structure_signal_summary",
            "result_signal_summary",
            "source_encoder_shortcut_guard",
            "source_kind_in_vector",
            "source_id_in_vector"
        ]);
    }

    private static void CopyBoundaryMetadata(
        IReadOnlyDictionary<string, string> source,
        IDictionary<string, string> target,
        SortedSet<string> redactions,
        IReadOnlyList<string> keys)
    {
        foreach (var key in keys)
        {
            if (source.TryGetValue(key, out var value))
                target[key] = SafeScalar(value, $"source_encoding_metadata_{key}", redactions, "missing");
        }
    }

    private static OperationResult<bool> ValidateTrainingIntakeHandoff(
        DesignConversionApprovedDpoHandoffArtifact handoff)
    {
        var rows = handoff.Rows ?? [];
        if (!string.Equals(handoff.SchemaVersion, ApprovedDpoHandoffSchemaVersion, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                "DesignConversion training intake only accepts approved DPO handoff schema v1 artifacts.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_HANDOFF_SCHEMA_INVALID");

        var topLevelBoundary = ValidateTrainingIntakeBoundaryMetadata(
            handoff.BoundaryMetadata,
            "approved_dpo_handoff");
        if (!topLevelBoundary.IsSuccess)
            return topLevelBoundary;

        if (ContainsForbiddenTrainingIntakePayload(handoff))
            return OperationResult<bool>.Fail(
                "DesignConversion training intake handoff contains raw or private payload markers.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_HANDOFF_UNSANITIZED");

        if (handoff.ApprovedCandidateCount == 0 && rows.Count == 0)
            return OperationResult<bool>.Ok(true);

        if (handoff.ApprovedCandidateCount <= 0
            || rows.Count == 0
            || handoff.ApprovedCandidateCount != rows.Count)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion training intake handoff approved candidate count does not match approved rows.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_HANDOFF_COUNT_MISMATCH");
        }

        foreach (var row in rows)
        {
            if (row is null)
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion training intake handoff contains a missing row.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_ROW_REQUIRED");
            }

            var rowBoundary = ValidateTrainingIntakeBoundaryMetadata(
                row.BoundaryMetadata,
                "approved_dpo_handoff_row");
            if (!rowBoundary.IsSuccess)
                return rowBoundary;

            if (!row.TrainingHandoffOnly
                || !row.ApprovedForTraining
                || !string.Equals(row.ApprovalStatus, "approved", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(row.ReviewStatus, RuntimeCandidateReviewApprovedStatus, StringComparison.Ordinal))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion training intake requires approved handoff-only rows.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_ROW_NOT_APPROVED");
            }

            if (string.IsNullOrWhiteSpace(row.ApprovedByRef)
                || string.IsNullOrWhiteSpace(row.ApprovalReason)
                || string.IsNullOrWhiteSpace(row.ApprovedAtUtc))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion training intake requires approved reviewer, reason, and timestamp metadata.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_APPROVAL_METADATA_REQUIRED");
            }

            if (string.IsNullOrWhiteSpace(row.CandidateId)
                || string.IsNullOrWhiteSpace(row.TrainingRecordId)
                || string.IsNullOrWhiteSpace(row.RecordHash)
                || string.IsNullOrWhiteSpace(row.InternalUnitStateHash)
                || string.IsNullOrWhiteSpace(row.SourceStructureHash)
                || string.IsNullOrWhiteSpace(row.ChosenPayloadHash)
                || string.IsNullOrWhiteSpace(row.RejectedPayloadHash)
                || string.IsNullOrWhiteSpace(row.TieBreakerHash))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion training intake requires candidate ids and handoff hashes.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_CANDIDATE_HASH_REQUIRED");
            }

            if (string.IsNullOrWhiteSpace(row.ChosenHtmlRef)
                || string.IsNullOrWhiteSpace(row.ChosenCssRef)
                || string.IsNullOrWhiteSpace(row.RejectedHtmlRef)
                || string.IsNullOrWhiteSpace(row.RejectedCssRef)
                || string.IsNullOrWhiteSpace(row.DesignTreeRef)
                || string.IsNullOrWhiteSpace(row.OriginalTreeRef)
                || (row.SourceArtifactRefs?.Count ?? 0) == 0
                || (row.RenderArtifactRefs?.Count ?? 0) == 0
                || (row.ComparisonArtifactRefs?.Count ?? 0) == 0)
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion training intake requires persisted html/css/source/render/comparison refs.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_RUNTIME_ARTIFACT_REFS_REQUIRED");
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateTrainingIntakeBoundaryMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string scope)
    {
        if (metadata is null || metadata.Count == 0)
            return OperationResult<bool>.Fail(
                $"DesignConversion training intake requires {scope} boundary metadata.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_BOUNDARY_METADATA_REQUIRED");

        foreach (var key in RequiredTrainingIntakeTrueBoundaryKeys)
        {
            if (!IsTruthy(ReadMetadata(metadata, key)))
            {
                return OperationResult<bool>.Fail(
                    $"DesignConversion training intake requires {scope} boundary '{key}=true'.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_BOUNDARY_METADATA_REQUIRED");
            }
        }

        foreach (var key in RequiredTrainingIntakeFalseBoundaryKeys)
        {
            if (!string.Equals(ReadMetadata(metadata, key), bool.FalseString, StringComparison.OrdinalIgnoreCase))
            {
                return OperationResult<bool>.Fail(
                    $"DesignConversion training intake requires {scope} boundary '{key}=false'.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_BOUNDARY_METADATA_REQUIRED");
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static DesignConversionTrainingIntakeRecord BuildTrainingIntakeRecord(
        DesignConversionApprovedDpoHandoffArtifact handoff,
        string requestedByRef,
        string reason,
        DateTimeOffset requestedAtUtc,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var requestedBy = SafeScalar(
            requestedByRef,
            "training_intake_requested_by_ref",
            redactions,
            "local-requester");
        var requestReason = SafeScalar(
            reason,
            "training_intake_reason",
            redactions,
            "future-training-intake-request");
        var candidates = (handoff.Rows ?? [])
            .OrderBy(row => row.TrainingRecordId, StringComparer.Ordinal)
            .ThenBy(row => row.CandidateId, StringComparer.Ordinal)
            .Select(row => BuildTrainingIntakeCandidateRef(row, redactions))
            .ToArray();
        var intakeId = BuildTrainingIntakeId(
            handoff,
            requestedBy,
            requestReason,
            requestedAtUtc,
            admissionBoundaryMetadata);
        var sourceProof = BuildTrainingIntakeSourceProof(candidates);
        var warnings = BuildTrainingIntakeWarnings(candidates.Length, sourceProof);

        return new DesignConversionTrainingIntakeRecord(
            IntakeId: intakeId,
            IntakeRef: candidates.Length == 0
                ? string.Empty
                : $"memory://design-conversion/training-intake/{intakeId}",
            SourceHandoffArtifactId: SafeScalar(
                handoff.ArtifactId,
                "training_intake_source_handoff_artifact_id",
                redactions,
                "approved-dpo-handoff"),
            SourceHandoffArtifactRef: SafeArtifactRef(
                handoff.ArtifactRef,
                "training_intake_source_handoff_artifact_ref",
                redactions),
            ApprovedCandidateCount: handoff.ApprovedCandidateCount,
            AcceptedCandidateCount: candidates.Length,
            CandidateRefs: candidates,
            RequestedAtUtc: requestedAtUtc,
            RequestedByRef: requestedBy,
            RequestReason: requestReason,
            TrainingIntakeOnly: true,
            TrainingNotStarted: true,
            NotTrained: true,
            WeightsNotProduced: true,
            CheckpointNotMutated: true,
            ModelUploadNotReady: true,
            ProductNotReady: true,
            HeldOutNotProven: true,
            SourceDisjointNotProven: true,
            Warnings: warnings,
            BoundaryMetadata: BuildTrainingIntakeBoundaryMetadata(
                handoff,
                candidates.Length,
                requestedBy,
                requestReason,
                sourceProof,
                redactions,
                admissionBoundaryMetadata));
    }

    private static DesignConversionTrainingIntakeCandidateRef BuildTrainingIntakeCandidateRef(
        DesignConversionApprovedDpoHandoffRow row,
        SortedSet<string> redactions)
    {
        var sourceManifestRef = SafeArtifactRef(
            row.SourceManifestRef,
            "training_intake_source_manifest_ref",
            redactions);
        var sourceManifestHash = BuildTrainingIntakeSourceManifestHash(sourceManifestRef);
        var sourceIdentityHash = BuildTrainingIntakeSourceIdentityHash(
            sourceManifestRef,
            row.SourceStructureHash,
            row.TrainingRecordId);
        var chosenHtmlRef = SafeArtifactRef(row.ChosenHtmlRef, "training_intake_chosen_html_ref", redactions);
        var chosenCssRef = SafeArtifactRef(row.ChosenCssRef, "training_intake_chosen_css_ref", redactions);
        var rejectedHtmlRef = SafeArtifactRef(row.RejectedHtmlRef, "training_intake_rejected_html_ref", redactions);
        var rejectedCssRef = SafeArtifactRef(row.RejectedCssRef, "training_intake_rejected_css_ref", redactions);
        var designTreeRef = SafeArtifactRef(row.DesignTreeRef, "training_intake_design_tree_ref", redactions);
        var originalTreeRef = SafeArtifactRef(row.OriginalTreeRef, "training_intake_original_tree_ref", redactions);
        var sourceArtifactRefs = SanitizeTrainingIntakeRefList(
            row.SourceArtifactRefs,
            "training_intake_source_artifact_ref",
            redactions);
        var renderArtifactRefs = SanitizeTrainingIntakeRefList(
            row.RenderArtifactRefs,
            "training_intake_render_artifact_ref",
            redactions);
        var comparisonArtifactRefs = SanitizeTrainingIntakeRefList(
            row.ComparisonArtifactRefs,
            "training_intake_comparison_artifact_ref",
            redactions);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["candidate_id"] = row.CandidateId,
            ["training_record_id"] = row.TrainingRecordId,
            ["handoff_record_hash"] = row.RecordHash,
            ["source_manifest_ref"] = sourceManifestRef,
            ["source_manifest_hash"] = sourceManifestHash,
            ["source_identity_hash"] = sourceIdentityHash,
            ["approved_by_ref"] = row.ApprovedByRef,
            ["approval_reason"] = row.ApprovalReason,
            ["approved_at_utc"] = row.ApprovedAtUtc,
            ["training_intake_only"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "training_intake_preflight_not_training_grade",
            ["training_not_started"] = bool.TrueString,
            ["not_trained"] = bool.TrueString,
            ["weights_not_produced"] = bool.TrueString,
            ["checkpoint_not_mutated"] = bool.TrueString,
            ["model_upload_not_ready"] = bool.TrueString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["design_conversion_runtime_artifacts_persisted"] = bool.TrueString,
            ["chosen_html_ref"] = chosenHtmlRef,
            ["chosen_css_ref"] = chosenCssRef,
            ["rejected_html_ref"] = rejectedHtmlRef,
            ["rejected_css_ref"] = rejectedCssRef,
            ["design_tree_ref"] = designTreeRef,
            ["original_tree_ref"] = originalTreeRef,
            ["source_artifact_ref_count"] = sourceArtifactRefs.Count.ToString(CultureInfo.InvariantCulture),
            ["render_artifact_ref_count"] = renderArtifactRefs.Count.ToString(CultureInfo.InvariantCulture),
            ["comparison_artifact_ref_count"] = comparisonArtifactRefs.Count.ToString(CultureInfo.InvariantCulture)
        };
        CopyBoundaryMetadata(row.BoundaryMetadata, metadata, redactions,
        [
            "internal_vector_hash",
            "internal_vector_dimensions",
            "source_encoder_vector_hash",
            "source_encoder_feature_map_hash",
            "source_encoder_feature_names",
            "source_structure_signal_summary",
            "result_signal_summary",
            "source_encoder_shortcut_guard",
            "source_kind_in_vector",
            "source_id_in_vector",
            "attempt_count",
            "stop_reason",
            "quality_reached",
            "final_candidate_from_loop",
            "comparison_diff_metadata_ref",
            "runtime_created_dpo_candidate_count",
            "runtime_created_dpo_candidate",
            "improvement_loop_to_local_training_store",
            "local_training_store_dpo_source",
            "refs_or_hashes_only"
        ]);
        AddTrainingDatabaseDurabilityMetadata(
            metadata,
            TrainingIntakeSchemaVersion,
            "training-intake-candidate",
            1,
            new[]
            {
                sourceManifestRef,
                chosenHtmlRef,
                chosenCssRef,
                rejectedHtmlRef,
                rejectedCssRef,
                designTreeRef,
                originalTreeRef
            }
                .Concat(sourceArtifactRefs)
                .Concat(renderArtifactRefs)
                .Concat(comparisonArtifactRefs),
            [
                row.LocalArtifactRecordId,
                row.CandidateId,
                row.TrainingRecordId,
                row.RecordHash,
                row.InternalUnitStateHash,
                row.SourceStructureHash,
                row.ChosenPayloadHash,
                row.RejectedPayloadHash,
                row.TieBreakerHash
            ]);

        return new DesignConversionTrainingIntakeCandidateRef(
            LocalArtifactRecordId: row.LocalArtifactRecordId,
            CandidateId: row.CandidateId,
            TrainingRecordId: row.TrainingRecordId,
            UnitId: row.UnitId,
            UsefulnessScore: row.UsefulnessScore,
            HandoffRecordHash: row.RecordHash,
            SourceManifestRef: sourceManifestRef,
            SourceManifestHash: sourceManifestHash,
            SourceIdentityHash: sourceIdentityHash,
            InternalUnitStateHash: row.InternalUnitStateHash,
            SourceStructureHash: row.SourceStructureHash,
            ChosenPayloadHash: row.ChosenPayloadHash,
            RejectedPayloadHash: row.RejectedPayloadHash,
            TieBreakerHash: row.TieBreakerHash,
            ChosenHtmlRef: chosenHtmlRef,
            ChosenCssRef: chosenCssRef,
            RejectedHtmlRef: rejectedHtmlRef,
            RejectedCssRef: rejectedCssRef,
            DesignTreeRef: designTreeRef,
            OriginalTreeRef: originalTreeRef,
            SourceArtifactRefs: sourceArtifactRefs,
            RenderArtifactRefs: renderArtifactRefs,
            ComparisonArtifactRefs: comparisonArtifactRefs,
            BoundaryMetadata: metadata);
    }

    private static IReadOnlyList<string> SanitizeTrainingIntakeRefList(
        IReadOnlyList<string>? refs,
        string scope,
        SortedSet<string> redactions)
        => (refs ?? [])
            .Select(reference => SafeArtifactRef(reference, scope, redactions))
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildTrainingIntakeWarnings(
        int acceptedCandidateCount,
        DesignConversionTrainingIntakeSourceProof sourceProof)
    {
        var warnings = new List<string>
        {
            "training_intake_only",
            "future_ilearningmachine_training_intake_request",
            "training_not_started",
            "not_trained",
            "weights_not_produced",
            "checkpoint_not_mutated",
            "model_upload_not_ready",
            "product_not_ready",
            "held_out_not_proven",
            "source_disjoint_not_proven",
            $"source_manifest_status:{sourceProof.SourceManifestStatus}",
            $"source_disjoint_status:{sourceProof.SourceDisjointStatus}",
            "raw_html_css_tree_private_provider_payloads_not_emitted"
        };
        if (acceptedCandidateCount == 0)
        {
            warnings.Add("no_approved_candidates");
            warnings.Add("training_intake_not_enqueued");
        }

        return warnings
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string> BuildTrainingIntakeBoundaryMetadata(
        DesignConversionApprovedDpoHandoffArtifact handoff,
        int acceptedCandidateCount,
        string requestedBy,
        string reason,
        DesignConversionTrainingIntakeSourceProof sourceProof,
        SortedSet<string> redactions,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["schema_version"] = TrainingIntakeSchemaVersion,
            ["source_handoff_schema_version"] = handoff.SchemaVersion,
            ["source_handoff_artifact_id"] = handoff.ArtifactId,
            ["source_handoff_artifact_ref"] = handoff.ArtifactRef,
            ["approved_candidate_count"] = handoff.ApprovedCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["accepted_candidate_count"] = acceptedCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["requested_by_ref"] = requestedBy,
            ["request_reason"] = reason,
            ["training_intake_only"] = bool.TrueString,
            ["training_not_started"] = bool.TrueString,
            ["training_started"] = bool.FalseString,
            ["not_trained"] = bool.TrueString,
            ["weights_not_produced"] = bool.TrueString,
            ["checkpoint_not_mutated"] = bool.TrueString,
            ["model_upload_not_ready"] = bool.TrueString,
            ["model_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["source_manifest_status"] = sourceProof.SourceManifestStatus,
            ["source_disjoint_status"] = sourceProof.SourceDisjointStatus,
            ["source_manifest_proven"] = sourceProof.SourceManifestProven.ToString(),
            ["source_disjoint_proven"] = sourceProof.SourceDisjointProven.ToString(),
            ["source_manifest_candidate_count"] = sourceProof.CandidateCount.ToString(CultureInfo.InvariantCulture),
            ["source_manifest_hash_count"] = sourceProof.SourceManifestHashCount.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_train_source_count"] = sourceProof.TrainSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_heldout_source_count"] = sourceProof.HeldOutSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_overlap_count"] = sourceProof.OverlapSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["target_training_system"] = "future_ILearningMachine",
            ["training_execution_boundary"] = acceptedCandidateCount == 0
                ? "zero_intake_not_enqueued"
                : "local_intake_record_only"
        };
        foreach (var pair in admissionBoundaryMetadata ?? new Dictionary<string, string>(StringComparer.Ordinal))
            metadata[SafeScalar(pair.Key, "training_intake_admission_metadata_key", redactions, "admission_metadata_key")] =
                SafeScalar(pair.Value, $"training_intake_admission_metadata_{pair.Key}", redactions, "admission_metadata_value");

        AddTrainingDatabaseDurabilityMetadata(
            metadata,
            TrainingIntakeSchemaVersion,
            "training-intake",
            acceptedCandidateCount,
            [
                handoff.ArtifactRef,
                handoff.ArtifactId,
                sourceProof.SourceManifestStatus,
                sourceProof.SourceDisjointStatus
            ],
            [
                handoff.ArtifactId,
                handoff.SchemaVersion,
                acceptedCandidateCount.ToString(CultureInfo.InvariantCulture),
                sourceProof.SourceManifestHashCount.ToString(CultureInfo.InvariantCulture),
                sourceProof.SourceManifestStatus,
                sourceProof.SourceDisjointStatus
            ]);

        if (redactions.Count > 0)
            metadata["training_intake_redaction_codes"] = string.Join(",", redactions);

        return metadata;
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeReplayResolution>> ResolveCreateTrainingIntakeIdempotencyReplayAsync(
        DesignConversionTrainingIntakeRecord intake,
        CancellationToken ct)
    {
        if (!HasCreateTrainingIntakeIdempotencyBoundary(intake.BoundaryMetadata))
            return OperationResult<DesignConversionTrainingIntakeReplayResolution>.Ok(
                new DesignConversionTrainingIntakeReplayResolution(false, null));

        var existing = await ListTrainingIntakeRequestsAsync(ct);
        if (!existing.IsSuccess)
        {
            if (string.Equals(existing.ErrorCode, TrainingIntakeReadFailedErrorCode, StringComparison.Ordinal))
                return LocalArtifactDbWriteFailed<DesignConversionTrainingIntakeReplayResolution>();

            return existing.MapError<DesignConversionTrainingIntakeReplayResolution>();
        }

        var matches = existing.Value!
            .Where(record => HasSameCreateTrainingIntakeIdempotencyBoundary(record, intake))
            .OrderBy(record => record.RequestedAtUtc)
            .ThenBy(record => record.IntakeId, StringComparer.Ordinal)
            .ToArray();
        if (matches.Length == 0)
            return OperationResult<DesignConversionTrainingIntakeReplayResolution>.Ok(
                new DesignConversionTrainingIntakeReplayResolution(false, null));

        var samePayload = matches.FirstOrDefault(record =>
            string.Equals(record.IntakeId, intake.IntakeId, StringComparison.Ordinal));
        if (samePayload is null)
        {
            return OperationResult<DesignConversionTrainingIntakeReplayResolution>.Fail(
                "DesignConversion training intake create idempotency key was replayed with a different payload.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_IDEMPOTENCY_REPLAY_CONFLICT");
        }

        return OperationResult<DesignConversionTrainingIntakeReplayResolution>.Ok(
            new DesignConversionTrainingIntakeReplayResolution(
                true,
                MarkCreateTrainingIntakeIdempotencyBoundary(
                    samePayload,
                    replayHit: true,
                    createdNewSideEffect: false)));
    }

    private static bool HasSameCreateTrainingIntakeIdempotencyBoundary(
        DesignConversionTrainingIntakeRecord existing,
        DesignConversionTrainingIntakeRecord candidate)
        => string.Equals(
               ReadMetadata(existing.BoundaryMetadata, "training_intake_tenant_id_ref"),
               ReadMetadata(candidate.BoundaryMetadata, "training_intake_tenant_id_ref"),
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(existing.BoundaryMetadata, "training_intake_idempotency_key_ref"),
               ReadMetadata(candidate.BoundaryMetadata, "training_intake_idempotency_key_ref"),
               StringComparison.Ordinal);

    private static bool HasCreateTrainingIntakeIdempotencyBoundary(
        IReadOnlyDictionary<string, string>? metadata)
        => !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_tenant_id_ref"))
           && !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_idempotency_key_ref"));

    private static DesignConversionTrainingIntakeRecord MarkCreateTrainingIntakeIdempotencyBoundary(
        DesignConversionTrainingIntakeRecord record,
        bool replayHit,
        bool createdNewSideEffect)
    {
        if (!HasCreateTrainingIntakeIdempotencyBoundary(record.BoundaryMetadata))
            return record;

        var metadata = new Dictionary<string, string>(record.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["training_intake_admission_boundary_scope"] =
                "route_admission_plus_local_create_replay_cache_no_auth_token_no_full_tenant_isolation",
            ["training_intake_create_replay_cache_boundary"] =
                "tenant_id_idempotency_key_same_payload_create_replay_cache_v1",
            ["training_intake_replay_cache_checked"] = bool.TrueString,
            ["training_intake_idempotency_replay_hit"] = replayHit.ToString(),
            ["training_intake_idempotency_created_new_side_effect"] = createdNewSideEffect.ToString(),
            ["training_intake_idempotent_replay_result"] = replayHit.ToString(),
            ["training_intake_idempotency_side_effect_boundary"] = "create_training_intake_append_only_once"
        };

        return record with { BoundaryMetadata = metadata };
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeRunReplayResolution>> ResolveRunTrainingIntakeIdempotencyReplayAsync(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        CancellationToken ct)
    {
        if (!HasRunTrainingIntakeIdempotencyBoundary(admissionBoundaryMetadata))
            return OperationResult<DesignConversionTrainingIntakeRunReplayResolution>.Ok(
                new DesignConversionTrainingIntakeRunReplayResolution(false, null));

        var existing = await ReadTrainingIntakeRunRecordsAsync(ct);
        if (!existing.IsSuccess)
            return existing.MapError<DesignConversionTrainingIntakeRunReplayResolution>();

        var matches = existing.Value!
            .Where(result => HasSameRunTrainingIntakeIdempotencyBoundary(result.BoundaryMetadata, admissionBoundaryMetadata))
            .OrderBy(result => ReadMetadata(result.BoundaryMetadata, "training_intake_run_recorded_at_utc"), StringComparer.Ordinal)
            .ThenBy(result => result.RunId, StringComparer.Ordinal)
            .ToArray();
        if (matches.Length == 0)
            return OperationResult<DesignConversionTrainingIntakeRunReplayResolution>.Ok(
                new DesignConversionTrainingIntakeRunReplayResolution(false, null));

        var payloadHash = BuildRunTrainingIntakeIdempotencyPayloadHash(intake, options);
        var samePayload = matches.FirstOrDefault(result =>
            string.Equals(
                ReadMetadata(result.BoundaryMetadata, "training_intake_run_idempotency_payload_hash"),
                payloadHash,
                StringComparison.Ordinal));
        if (samePayload is null)
        {
            return OperationResult<DesignConversionTrainingIntakeRunReplayResolution>.Fail(
                "DesignConversion training intake run idempotency key was replayed with a different intake or options payload.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_IDEMPOTENCY_REPLAY_CONFLICT");
        }

        return OperationResult<DesignConversionTrainingIntakeRunReplayResolution>.Ok(
            new DesignConversionTrainingIntakeRunReplayResolution(
                true,
                MarkRunTrainingIntakeIdempotencyBoundary(
                    samePayload,
                    intake,
                    options,
                    admissionBoundaryMetadata,
                    replayHit: true,
                    createdNewSideEffect: false)));
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeRunResult>> StoreRunTrainingIntakeIdempotencyResultAsync(
        DesignConversionTrainingIntakeRunResult result,
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionTrainingIntakeRunResult>();

        var stored = MarkRunTrainingIntakeIdempotencyBoundary(
            result,
            intake,
            options,
            admissionBoundaryMetadata,
            replayHit: false,
            createdNewSideEffect: true);
        var line = JsonSerializer.Serialize(stored, JsonOptions);
        var framedLineByteLength = StrictUtf8Encoding.GetByteCount(line + Environment.NewLine);
        if (framedLineByteLength > MaxTrainingIntakeRunJournalRowByteLength
            || !TryProjectTrainingIntakeRunsLength(framedLineByteLength, out var projectedLength)
            || projectedLength > _trainingIntakeRunsMaxBytes)
        {
            return TrainingIntakeRunsQuotaExceeded<DesignConversionTrainingIntakeRunResult>();
        }

        Directory.CreateDirectory(_rootDirectory);
        await DurableCopyOnWriteAppendJsonLineAsync(_trainingIntakeRunPath, line, ct);

        return OperationResult<DesignConversionTrainingIntakeRunResult>.Ok(stored);
    }

    private bool CanReserveMaximumTrainingIntakeRunJournalRow()
        => TryProjectTrainingIntakeRunsLength(
                MaxTrainingIntakeRunJournalRowByteLength,
                out var projectedLength)
           && projectedLength <= _trainingIntakeRunsMaxBytes;

    private bool TryProjectTrainingIntakeRunsLength(
        long appendedLineByteLength,
        out long projectedLength)
    {
        var existingLength = File.Exists(_trainingIntakeRunPath)
            ? new FileInfo(_trainingIntakeRunPath).Length
            : 0;

        return TryProjectCommittedAuthorityLength(
            existingLength,
            appendedLineByteLength,
            out projectedLength);
    }

    private async Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>> ReadTrainingIntakeRunRecordsAsync(
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>();
        if (!File.Exists(_trainingIntakeRunPath))
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>.Ok([]);

        var records = new List<DesignConversionTrainingIntakeRunResult>();
        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(_trainingIntakeRunPath, StrictUtf8Encoding, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>();
        }
        catch (DecoderFallbackException)
        {
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>.Fail(
                "DesignConversion training intake runs could not be read because the training intake runs file is not valid UTF-8.",
                InvalidUtf8ErrorCode);
        }
        catch (IOException)
        {
            return TrainingIntakeRunReadFailed<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>();
        }
        catch (UnauthorizedAccessException)
        {
            return TrainingIntakeRunReadFailed<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>();
        }
        catch (SecurityException)
        {
            return TrainingIntakeRunReadFailed<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>();
        }

        for (var i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            DesignConversionTrainingIntakeRunResult? record;
            try
            {
                record = JsonSerializer.Deserialize<DesignConversionTrainingIntakeRunResult>(
                    lines[i],
                    JsonOptions);
            }
            catch (JsonException)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>.Fail(
                    $"DesignConversion training intake run line {(i + 1).ToString(CultureInfo.InvariantCulture)} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_LINE_INVALID");
            }
            catch (NotSupportedException)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>.Fail(
                    $"DesignConversion training intake run line {(i + 1).ToString(CultureInfo.InvariantCulture)} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_LINE_INVALID");
            }

            if (record is null)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>.Fail(
                    $"DesignConversion training intake run line {(i + 1).ToString(CultureInfo.InvariantCulture)} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_LINE_INVALID");
            }

            records.Add(record);
        }

        return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRunResult>>.Ok(records.ToArray());
    }

    private static DesignConversionTrainingIntakeRunResult MarkRunTrainingIntakeIdempotencyBoundary(
        DesignConversionTrainingIntakeRunResult result,
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        bool replayHit,
        bool createdNewSideEffect)
    {
        var metadata = new Dictionary<string, string>(result.BoundaryMetadata, StringComparer.Ordinal);
        foreach (var pair in admissionBoundaryMetadata ?? new Dictionary<string, string>(StringComparer.Ordinal))
            metadata[pair.Key] = pair.Value;

        if (!HasRunTrainingIntakeIdempotencyBoundary(metadata))
            return result with { BoundaryMetadata = metadata };

        metadata["training_intake_admission_boundary_scope"] =
            "route_admission_plus_local_run_replay_cache_no_auth_token_no_full_tenant_isolation";
        metadata["training_intake_run_replay_cache_boundary"] =
            "tenant_id_idempotency_key_same_intake_options_run_replay_cache_v1";
        metadata["training_intake_replay_cache_checked"] = bool.TrueString;
        metadata["training_intake_idempotency_replay_hit"] = replayHit.ToString();
        metadata["training_intake_idempotency_created_new_side_effect"] = createdNewSideEffect.ToString();
        metadata["training_intake_idempotent_replay_result"] = replayHit.ToString();
        metadata["training_intake_idempotency_side_effect_boundary"] = "run_training_intake_append_only_once";
        metadata["training_intake_run_idempotency_payload_hash"] =
            BuildRunTrainingIntakeIdempotencyPayloadHash(intake, options);
        metadata["training_intake_run_recorded_at_utc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        return result with { BoundaryMetadata = metadata };
    }

    private static bool HasSameRunTrainingIntakeIdempotencyBoundary(
        IReadOnlyDictionary<string, string>? existing,
        IReadOnlyDictionary<string, string>? candidate)
        => string.Equals(
               ReadMetadata(existing, "training_intake_tenant_id_ref"),
               ReadMetadata(candidate, "training_intake_tenant_id_ref"),
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(existing, "training_intake_idempotency_key_ref"),
               ReadMetadata(candidate, "training_intake_idempotency_key_ref"),
               StringComparison.Ordinal);

    private static bool HasRunTrainingIntakeIdempotencyBoundary(
        IReadOnlyDictionary<string, string>? metadata)
        => !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_tenant_id_ref"))
           && !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_idempotency_key_ref"));

    private static string BuildRunTrainingIntakeIdempotencyPayloadHash(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options)
        => $"sha256:{HashText(string.Join("|",
        [
            TrainingIntakeRunSchemaVersion,
            intake.IntakeId,
            JsonSerializer.Serialize(options, JsonOptions)
        ]))}";

    private async Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionReplayResolution>> ResolvePromoteLocalIdempotencyReplayAsync(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        string promotionOutputDirectory,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        CancellationToken ct)
    {
        if (!HasPromoteLocalIdempotencyBoundary(admissionBoundaryMetadata))
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionReplayResolution>.Ok(
                new DesignConversionTrainingIntakeLocalPromotionReplayResolution(false, null));

        var existing = await ReadTrainingIntakeLocalPromotionRecordsAsync(ct);
        if (!existing.IsSuccess)
            return existing.MapError<DesignConversionTrainingIntakeLocalPromotionReplayResolution>();

        var matches = existing.Value!
            .Where(result => HasSamePromoteLocalIdempotencyBoundary(result.BoundaryMetadata, admissionBoundaryMetadata))
            .Where(result => !IsRollbackLocalPromotionRecord(result.BoundaryMetadata))
            .OrderBy(result => ReadMetadata(result.BoundaryMetadata, "training_intake_promote_local_recorded_at_utc"), StringComparer.Ordinal)
            .ThenBy(result => result.TrainingRun.RunId, StringComparer.Ordinal)
            .ToArray();
        if (matches.Length == 0)
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionReplayResolution>.Ok(
                new DesignConversionTrainingIntakeLocalPromotionReplayResolution(false, null));

        var payloadHash = BuildPromoteLocalIdempotencyPayloadHash(intake, options, promotionOutputDirectory);
        var samePayload = matches.FirstOrDefault(result =>
            string.Equals(
                ReadMetadata(result.BoundaryMetadata, "training_intake_promote_local_idempotency_payload_hash"),
                payloadHash,
                StringComparison.Ordinal));
        if (samePayload is null)
        {
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionReplayResolution>.Fail(
                "DesignConversion training intake promote-local idempotency key was replayed with a different intake, options, or promotion payload.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_PROMOTE_LOCAL_IDEMPOTENCY_REPLAY_CONFLICT");
        }

        return OperationResult<DesignConversionTrainingIntakeLocalPromotionReplayResolution>.Ok(
            new DesignConversionTrainingIntakeLocalPromotionReplayResolution(
                true,
                MarkPromoteLocalIdempotencyBoundary(
                    samePayload,
                    intake,
                    options,
                    promotionOutputDirectory,
                    admissionBoundaryMetadata,
                    replayHit: true,
                    createdNewSideEffect: false)));
    }

    private static OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution> ResolveRollbackLocalPromotionIdempotencyReplay(
        IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult> records,
        string targetRef,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata)
    {
        if (!HasRollbackLocalPromotionIdempotencyBoundary(admissionBoundaryMetadata))
        {
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution>.Ok(
                new DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution(false, null));
        }

        var matches = (records ?? [])
            .Where(record => IsRollbackLocalPromotionRecord(record.BoundaryMetadata))
            .Where(record => !string.IsNullOrWhiteSpace(ReadMetadata(
                record.BoundaryMetadata,
                "training_intake_rollback_local_idempotency_payload_hash")))
            .Where(record => HasSameRollbackLocalPromotionIdempotencyBoundary(record.BoundaryMetadata, admissionBoundaryMetadata))
            .OrderBy(record => ReadMetadata(record.BoundaryMetadata, "training_intake_rollback_local_recorded_at_utc"), StringComparer.Ordinal)
            .ThenBy(record => ReadMetadata(record.BoundaryMetadata, "local_promotion_rollback_id"), StringComparer.Ordinal)
            .ToArray();
        if (matches.Length == 0)
        {
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution>.Ok(
                new DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution(false, null));
        }

        var payloadHash = BuildRollbackLocalPromotionIdempotencyPayloadHash(
            targetRef,
            requestedByRef,
            reason,
            requestedAtUtc);
        var samePayload = matches.FirstOrDefault(record =>
            string.Equals(
                ReadMetadata(record.BoundaryMetadata, "training_intake_rollback_local_idempotency_payload_hash"),
                payloadHash,
                StringComparison.Ordinal));
        if (samePayload is null)
        {
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution>.Fail(
                "DesignConversion training intake rollback-local-promotion idempotency key was replayed with a different rollback payload.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_ROLLBACK_LOCAL_IDEMPOTENCY_REPLAY_CONFLICT");
        }

        return OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution>.Ok(
            new DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution(
                true,
                MarkRollbackLocalPromotionIdempotencyBoundary(
                    BuildLocalPromotionRollbackResult(samePayload),
                    targetRef,
                    requestedByRef,
                    reason,
                    requestedAtUtc,
                    admissionBoundaryMetadata,
                    replayHit: true,
                    createdNewSideEffect: false)));
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> StorePromoteLocalIdempotencyResultAsync(
        DesignConversionTrainingIntakeLocalPromotionResult result,
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        string promotionOutputDirectory,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<DesignConversionTrainingIntakeLocalPromotionResult>();

        var stored = MarkPromoteLocalIdempotencyBoundary(
            result,
            intake,
            options,
            promotionOutputDirectory,
            admissionBoundaryMetadata,
            replayHit: false,
            createdNewSideEffect: true);
        var activeState = await MarkLocalPromotionActiveStateAsync(stored, ct);
        if (!activeState.IsSuccess)
            return activeState;
        stored = activeState.Value!;

        if (!HasPromoteLocalIdempotencyBoundary(stored.BoundaryMetadata))
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Ok(stored);

        var line = JsonSerializer.Serialize(stored, JsonOptions);
        var framedLineByteLength = StrictUtf8Encoding.GetByteCount(line + Environment.NewLine);
        if (framedLineByteLength > MaxTrainingIntakeLocalPromotionJournalRowByteLength
            || !TryProjectTrainingIntakeLocalPromotionsLength(framedLineByteLength, out var projectedLength)
            || projectedLength > _trainingIntakeLocalPromotionsMaxBytes)
        {
            return TrainingIntakeLocalPromotionsQuotaExceeded<DesignConversionTrainingIntakeLocalPromotionResult>();
        }

        Directory.CreateDirectory(_rootDirectory);
        await DurableCopyOnWriteAppendJsonLineAsync(_trainingIntakeLocalPromotionPath, line, ct);

        return OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Ok(stored);
    }

    private bool CanReserveMaximumTrainingIntakeLocalPromotionJournalRow()
        => TryProjectTrainingIntakeLocalPromotionsLength(
                MaxTrainingIntakeLocalPromotionJournalRowByteLength,
                out var projectedLength)
           && projectedLength <= _trainingIntakeLocalPromotionsMaxBytes;

    private bool TryProjectTrainingIntakeLocalPromotionsLength(
        long appendedLineByteLength,
        out long projectedLength)
    {
        var existingLength = File.Exists(_trainingIntakeLocalPromotionPath)
            ? new FileInfo(_trainingIntakeLocalPromotionPath).Length
            : 0;

        return TryProjectCommittedAuthorityLength(
            existingLength,
            appendedLineByteLength,
            out projectedLength);
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> MarkLocalPromotionActiveStateAsync(
        DesignConversionTrainingIntakeLocalPromotionResult result,
        CancellationToken ct)
    {
        if (!result.LocalPromotionReady || result.PromotedLocalModel is null)
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Ok(result);

        var existing = await ReadTrainingIntakeLocalPromotionRecordsAsync(ct);
        if (!existing.IsSuccess)
            return existing.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();

        return OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Ok(
            MarkLocalPromotionActiveState(result, ResolveCurrentLocalPromotionActiveState(existing.Value!)));
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult MarkLocalPromotionActiveState(
        DesignConversionTrainingIntakeLocalPromotionResult result,
        DesignConversionLocalPromotionActiveState prior)
    {
        var activeRef = BuildLocalPromotionActiveRef(result);
        var metadata = new Dictionary<string, string>(result.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["local_promotion_record_kind"] = "promotion",
            ["local_promotion_active_local_model_ref"] = activeRef,
            ["local_promotion_active_model_id"] = result.PromotedLocalModel?.ModelId ?? string.Empty,
            ["local_promotion_active_checkpoint_sha256"] = result.PromotedLocalModel?.CheckpointSha256 ?? string.Empty,
            ["local_promotion_prior_state_recorded"] = bool.TrueString,
            ["local_promotion_prior_state_kind"] = prior.HasActive ? "local_promotion_ref" : "none",
            ["local_promotion_prior_active_local_model_ref"] = prior.ActiveLocalModelRef,
            ["local_promotion_prior_active_model_id"] = prior.ActiveModelId,
            ["local_promotion_prior_active_checkpoint_sha256"] = prior.ActiveCheckpointSha256,
            ["local_promotion_rollback_supported"] = prior.HasActive.ToString(),
            ["local_promotion_rollback_boundary"] = prior.HasActive
                ? "actual_local_rollback_via_local_artifact_db_metadata"
                : "explicit_unsupported_nonready_boundary",
            ["local_promotion_rollback_status"] = prior.HasActive
                ? "ready_restore_prior_active_local_model"
                : "unsupported_no_prior_active_local_model_checkpoint_state",
            ["local_promotion_previous_model_checkpoint_ref_available"] = prior.HasActive.ToString(),
            ["local_promotion_restore_previous_state_supported"] = prior.HasActive.ToString(),
            ["product_ready_blocked_by_local_rollback_boundary"] = (!prior.HasActive).ToString()
        };
        var evidenceRefs = (result.EvidenceRefs ?? [])
            .Append("DESIGN_CONVERSION_LOCAL_PROMOTION_ACTIVE_STATE_RECORDED")
            .Append(prior.HasActive
                ? "DESIGN_CONVERSION_LOCAL_PROMOTION_PRIOR_ACTIVE_STATE_RECORDED"
                : "DESIGN_CONVERSION_LOCAL_PROMOTION_PRIOR_ACTIVE_STATE_NONE_RECORDED")
            .Concat(prior.HasActive
                ? ["DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_SUPPORTED_BY_PRIOR_ACTIVE_STATE"]
                : ["DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_UNSUPPORTED_NO_PRIOR_STATE"])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var warnings = (result.Warnings ?? [])
            .Append(prior.HasActive
                ? "local_promotion_rollback_metadata_recorded"
                : "local_promotion_rollback_unsupported_no_prior_state")
            .Append(prior.HasActive
                ? "local_promotion_prior_active_state_recorded"
                : "local_promotion_prior_active_state_none")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return result with
        {
            EvidenceRefs = evidenceRefs,
            Warnings = warnings,
            BoundaryMetadata = metadata
        };
    }

    private static DesignConversionLocalPromotionActiveState ResolveCurrentLocalPromotionActiveState(
        IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult> records)
    {
        var state = DesignConversionLocalPromotionActiveState.Empty;
        foreach (var record in records ?? [])
        {
            if (string.Equals(
                    ReadMetadata(record.BoundaryMetadata, "local_promotion_record_kind"),
                    "rollback",
                    StringComparison.Ordinal))
            {
                var restoredRef = ReadMetadata(
                    record.BoundaryMetadata,
                    "local_promotion_rollback_restored_active_local_model_ref");
                state = string.IsNullOrWhiteSpace(restoredRef)
                    ? DesignConversionLocalPromotionActiveState.Empty
                    : new DesignConversionLocalPromotionActiveState(
                        HasActive: true,
                        ActiveLocalModelRef: restoredRef,
                        ActiveModelId: ReadMetadata(
                            record.BoundaryMetadata,
                            "local_promotion_rollback_restored_active_model_id"),
                        ActiveCheckpointSha256: ReadMetadata(
                            record.BoundaryMetadata,
                            "local_promotion_rollback_restored_active_checkpoint_sha256"));
                continue;
            }

            if (!record.LocalPromotionReady || record.PromotedLocalModel is null)
                continue;

            state = new DesignConversionLocalPromotionActiveState(
                HasActive: true,
                ActiveLocalModelRef: BuildLocalPromotionActiveRef(record),
                ActiveModelId: record.PromotedLocalModel.ModelId,
                ActiveCheckpointSha256: record.PromotedLocalModel.CheckpointSha256);
        }

        return state;
    }

    private static string BuildLocalPromotionActiveRef(DesignConversionTrainingIntakeLocalPromotionResult result)
    {
        var metadataRef = ReadMetadata(result.BoundaryMetadata, "local_promotion_active_local_model_ref");
        if (!string.IsNullOrWhiteSpace(metadataRef))
            return metadataRef;

        var modelId = result.PromotedLocalModel?.ModelId;
        if (!string.IsNullOrWhiteSpace(modelId))
            return $"local://design-conversion/promoted-local-model/{Uri.EscapeDataString(modelId)}";

        var uploadRef = result.Publication?.Manifest.PromotionUploadRef;
        return string.IsNullOrWhiteSpace(uploadRef) ? string.Empty : uploadRef;
    }

    private static bool MatchesLocalPromotionRef(
        DesignConversionTrainingIntakeLocalPromotionResult record,
        string promotedLocalModelRef)
    {
        if (record is null
            || !record.LocalPromotionReady
            || string.Equals(
                ReadMetadata(record.BoundaryMetadata, "local_promotion_record_kind"),
                "rollback",
                StringComparison.Ordinal))
        {
            return false;
        }

        var requested = promotedLocalModelRef?.Trim();
        if (string.IsNullOrWhiteSpace(requested))
            return false;

        var candidates = new[]
        {
            BuildLocalPromotionActiveRef(record),
            ReadMetadata(record.BoundaryMetadata, "local_promotion_active_local_model_ref"),
            ReadMetadata(record.BoundaryMetadata, "promotion_upload_ref"),
            ReadMetadata(record.BoundaryMetadata, "local_promoted_model_id"),
            ReadMetadata(record.BoundaryMetadata, "local_promoted_checkpoint_sha256"),
            record.PromotedLocalModel?.ModelId,
            record.PromotedLocalModel?.ManifestId,
            record.PromotedLocalModel?.ManifestRef,
            record.PromotedLocalModel?.CheckpointSha256,
            record.Publication?.Manifest.PromotionUploadRef
        };

        return candidates.Any(candidate =>
            !string.IsNullOrWhiteSpace(candidate)
            && string.Equals(candidate, requested, StringComparison.Ordinal));
    }

    private static string BuildLocalPromotionRollbackId(
        string targetRef,
        string priorRef,
        string requestedByRef,
        string reason,
        DateTimeOffset rollbackAt)
        => $"local-promotion-rollback-{HashText(string.Join("|",
        [
            TrainingIntakeLocalPromotionRollbackSchemaVersion,
            targetRef,
            priorRef,
            requestedByRef,
            reason,
            rollbackAt.ToString("O", CultureInfo.InvariantCulture)
        ]))}";

    private static DesignConversionTrainingIntakeLocalPromotionResult BuildLocalPromotionRollbackRecord(
        DesignConversionTrainingIntakeLocalPromotionResult target,
        string rollbackId,
        string targetRef,
        string previousActiveRef,
        string restoredRef,
        string restoredModelId,
        string restoredCheckpointSha256,
        string requestedByRef,
        string reason,
        DateTimeOffset rollbackAt)
    {
        var metadata = new Dictionary<string, string>(target.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["schema_version"] = TrainingIntakeLocalPromotionRollbackSchemaVersion,
            ["local_promotion_record_kind"] = "rollback",
            ["local_promotion_rollback_id"] = rollbackId,
            ["local_promotion_rollback_applied"] = bool.TrueString,
            ["local_promotion_prior_state_restored"] = bool.TrueString,
            ["local_promotion_no_prior_state_recorded"] = bool.FalseString,
            ["local_promotion_rollback_target_local_model_ref"] = targetRef,
            ["local_promotion_rollback_previous_active_local_model_ref"] = previousActiveRef,
            ["local_promotion_rollback_restored_active_local_model_ref"] = restoredRef,
            ["local_promotion_rollback_restored_active_model_id"] = restoredModelId,
            ["local_promotion_rollback_restored_active_checkpoint_sha256"] = restoredCheckpointSha256,
            ["local_promotion_active_local_model_ref"] = restoredRef,
            ["local_promotion_active_model_id"] = restoredModelId,
            ["local_promotion_active_checkpoint_sha256"] = restoredCheckpointSha256,
            ["local_promotion_rollback_requested_by_ref"] = requestedByRef,
            ["local_promotion_rollback_reason_hash"] = $"sha256:{HashText(reason)}",
            ["local_promotion_rollback_requested_at_utc"] = rollbackAt.ToString("O", CultureInfo.InvariantCulture),
            ["local_promotion_rollback_boundary"] = "actual_local_rollback_via_local_artifact_db_metadata",
            ["local_promotion_rollback_status"] = "restored_prior_active_local_model",
            ["local_promotion_rollback_supported"] = bool.TrueString,
            ["local_promotion_restore_previous_state_supported"] = bool.TrueString,
            ["local_promotion_previous_model_checkpoint_ref_available"] = bool.TrueString,
            ["local_promotion_ready"] = bool.FalseString,
            ["local_promotion_not_ready"] = bool.TrueString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["product_ready_blocked_by_local_rollback_boundary"] = bool.FalseString
        };
        var evidenceRefs = (target.EvidenceRefs ?? [])
            .Where(reference => !string.Equals(
                reference,
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_UNSUPPORTED_NONREADY_BOUNDARY",
                StringComparison.Ordinal))
            .Append("DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_RECORD_WRITTEN")
            .Append("DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_APPLIED")
            .Append("DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_PRIOR_ACTIVE_STATE_RESTORED")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var warnings = (target.Warnings ?? [])
            .Where(warning => !string.Equals(warning, "local_promotion_rollback_unsupported", StringComparison.Ordinal))
            .Append("local_promotion_rollback_applied")
            .Append("local_promotion_prior_active_state_restored")
            .Append("product_ready=false")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return target with
        {
            LocalPromotionReady = false,
            PromotedLocalModel = null,
            EvidenceRefs = evidenceRefs,
            Warnings = warnings,
            BoundaryMetadata = metadata
        };
    }

    private static DesignConversionTrainingIntakeLocalPromotionRollbackResult BuildLocalPromotionRollbackResult(
        DesignConversionTrainingIntakeLocalPromotionResult record)
    {
        var metadata = record.BoundaryMetadata;
        return new DesignConversionTrainingIntakeLocalPromotionRollbackResult(
            RollbackId: ReadMetadata(metadata, "local_promotion_rollback_id"),
            RollbackApplied: IsTruthy(ReadMetadata(metadata, "local_promotion_rollback_applied")),
            PriorStateRestored: IsTruthy(ReadMetadata(metadata, "local_promotion_prior_state_restored")),
            NoPriorStateRecorded: IsTruthy(ReadMetadata(metadata, "local_promotion_no_prior_state_recorded")),
            RolledBackLocalModelRef: ReadMetadata(metadata, "local_promotion_rollback_target_local_model_ref"),
            PreviousActiveLocalModelRef: ReadMetadata(metadata, "local_promotion_rollback_previous_active_local_model_ref"),
            RestoredActiveLocalModelRef: ReadMetadata(metadata, "local_promotion_rollback_restored_active_local_model_ref"),
            RestoredActiveCheckpointSha256: ReadMetadata(metadata, "local_promotion_rollback_restored_active_checkpoint_sha256"),
            EvidenceRefs: record.EvidenceRefs,
            Warnings: record.Warnings,
            BlockReasons: record.BlockReasons,
            BoundaryMetadata: record.BoundaryMetadata);
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult MarkRollbackLocalPromotionIdempotencyBoundary(
        DesignConversionTrainingIntakeLocalPromotionResult result,
        string targetRef,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        bool replayHit,
        bool createdNewSideEffect)
        => result with
        {
            BoundaryMetadata = BuildRollbackLocalPromotionIdempotencyMetadata(
                result.BoundaryMetadata,
                targetRef,
                requestedByRef,
                reason,
                requestedAtUtc,
                admissionBoundaryMetadata,
                replayHit,
                createdNewSideEffect)
        };

    private static DesignConversionTrainingIntakeLocalPromotionRollbackResult MarkRollbackLocalPromotionIdempotencyBoundary(
        DesignConversionTrainingIntakeLocalPromotionRollbackResult result,
        string targetRef,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        bool replayHit,
        bool createdNewSideEffect)
        => result with
        {
            BoundaryMetadata = BuildRollbackLocalPromotionIdempotencyMetadata(
                result.BoundaryMetadata,
                targetRef,
                requestedByRef,
                reason,
                requestedAtUtc,
                admissionBoundaryMetadata,
                replayHit,
                createdNewSideEffect)
        };

    private static IReadOnlyDictionary<string, string> BuildRollbackLocalPromotionIdempotencyMetadata(
        IReadOnlyDictionary<string, string>? source,
        string targetRef,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        bool replayHit,
        bool createdNewSideEffect)
    {
        var metadata = new Dictionary<string, string>(source ?? new Dictionary<string, string>(StringComparer.Ordinal), StringComparer.Ordinal);
        foreach (var pair in admissionBoundaryMetadata ?? new Dictionary<string, string>(StringComparer.Ordinal))
            metadata[pair.Key] = pair.Value;

        if (!HasRollbackLocalPromotionIdempotencyBoundary(admissionBoundaryMetadata))
            return metadata;

        metadata["training_intake_admission_boundary_scope"] =
            "route_admission_plus_local_rollback_replay_cache_no_auth_token_no_full_tenant_isolation";
        metadata["training_intake_rollback_local_replay_cache_boundary"] =
            "tenant_id_idempotency_key_same_target_requester_reason_timestamp_rollback_local_replay_cache_v1";
        metadata["training_intake_replay_cache_checked"] = bool.TrueString;
        metadata["training_intake_idempotency_replay_hit"] = replayHit.ToString();
        metadata["training_intake_idempotency_created_new_side_effect"] = createdNewSideEffect.ToString();
        metadata["training_intake_idempotent_replay_result"] = replayHit.ToString();
        metadata["training_intake_idempotency_side_effect_boundary"] = "rollback_local_promotion_append_only_once";
        metadata["training_intake_rollback_local_idempotency_payload_hash"] =
            BuildRollbackLocalPromotionIdempotencyPayloadHash(targetRef, requestedByRef, reason, requestedAtUtc);
        if (string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_rollback_local_recorded_at_utc")))
            metadata["training_intake_rollback_local_recorded_at_utc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        metadata["training_intake_local_promotion_record_file"] = TrainingIntakeLocalPromotionFileName;

        return metadata;
    }

    private static bool IsRollbackLocalPromotionRecord(
        IReadOnlyDictionary<string, string>? metadata)
        => string.Equals(
            ReadMetadata(metadata, "local_promotion_record_kind"),
            "rollback",
            StringComparison.Ordinal);

    private static bool HasSameRollbackLocalPromotionIdempotencyBoundary(
        IReadOnlyDictionary<string, string>? existing,
        IReadOnlyDictionary<string, string>? candidate)
        => string.Equals(
               ReadMetadata(existing, "training_intake_tenant_id_ref"),
               ReadMetadata(candidate, "training_intake_tenant_id_ref"),
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(existing, "training_intake_idempotency_key_ref"),
               ReadMetadata(candidate, "training_intake_idempotency_key_ref"),
               StringComparison.Ordinal);

    private static bool HasRollbackLocalPromotionIdempotencyBoundary(
        IReadOnlyDictionary<string, string>? metadata)
        => !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_tenant_id_ref"))
           && !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_idempotency_key_ref"));

    private static string BuildRollbackLocalPromotionIdempotencyPayloadHash(
        string targetRef,
        string requestedByRef,
        string reason,
        DateTimeOffset? requestedAtUtc)
        => $"sha256:{HashText(string.Join("|",
        [
            TrainingIntakeLocalPromotionRollbackSchemaVersion,
            targetRef,
            requestedByRef,
            $"sha256:{HashText(reason)}",
            BuildRollbackLocalPromotionRequestedAtDiscriminator(requestedAtUtc)
        ]))}";

    private static string BuildRollbackLocalPromotionRequestedAtDiscriminator(
        DateTimeOffset? requestedAtUtc)
        => requestedAtUtc.HasValue
            ? $"requestedAtUtc:explicit:{requestedAtUtc.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)}"
            : "requestedAtUtc:omitted";

    public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>> ListTrainingIntakeLocalPromotionRecordsAsync(
        CancellationToken ct = default)
        => ReadTrainingIntakeLocalPromotionRecordsAsync(ct);

    private async Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>> ReadTrainingIntakeLocalPromotionRecordsAsync(
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Cancelled<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>();
        if (!File.Exists(_trainingIntakeLocalPromotionPath))
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Ok([]);

        var records = new List<DesignConversionTrainingIntakeLocalPromotionResult>();
        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(_trainingIntakeLocalPromotionPath, StrictUtf8Encoding, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Cancelled<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>();
        }
        catch (DecoderFallbackException)
        {
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Fail(
                "DesignConversion training intake local promotions could not be read because the training intake local promotions file is not valid UTF-8.",
                InvalidUtf8ErrorCode);
        }
        catch (IOException)
        {
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Fail(
                "DesignConversion training intake local promotions could not be read safely.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READ_FAILED");
        }
        catch (UnauthorizedAccessException)
        {
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Fail(
                "DesignConversion training intake local promotions could not be read safely.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READ_FAILED");
        }
        catch (SecurityException)
        {
            return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Fail(
                "DesignConversion training intake local promotions could not be read safely.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READ_FAILED");
        }

        for (var i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            DesignConversionTrainingIntakeLocalPromotionResult? record;
            try
            {
                record = JsonSerializer.Deserialize<DesignConversionTrainingIntakeLocalPromotionResult>(
                    lines[i],
                    JsonOptions);
            }
            catch (JsonException)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Fail(
                    $"DesignConversion training intake local promotion line {i + 1} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_LINE_INVALID");
            }
            catch (NotSupportedException)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Fail(
                    $"DesignConversion training intake local promotion line {i + 1} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_LINE_INVALID");
            }

            if (record is null)
            {
                return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Fail(
                    $"DesignConversion training intake local promotion line {i + 1} could not be read.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_LINE_INVALID");
            }

            records.Add(record);
        }

        return OperationResult<IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult>>.Ok(records.ToArray());
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult MarkPromoteLocalIdempotencyBoundary(
        DesignConversionTrainingIntakeLocalPromotionResult result,
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        string promotionOutputDirectory,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata,
        bool replayHit,
        bool createdNewSideEffect)
    {
        var metadata = new Dictionary<string, string>(result.BoundaryMetadata, StringComparer.Ordinal);
        foreach (var pair in admissionBoundaryMetadata ?? new Dictionary<string, string>(StringComparer.Ordinal))
            metadata[pair.Key] = pair.Value;

        if (!HasPromoteLocalIdempotencyBoundary(metadata))
            return result with { BoundaryMetadata = metadata };

        metadata["training_intake_admission_boundary_scope"] =
            "route_admission_plus_local_promote_replay_cache_no_auth_token_no_full_tenant_isolation";
        metadata["training_intake_promote_local_replay_cache_boundary"] =
            "tenant_id_idempotency_key_same_intake_run_options_output_promote_local_replay_cache_v1";
        metadata["training_intake_replay_cache_checked"] = bool.TrueString;
        metadata["training_intake_idempotency_replay_hit"] = replayHit.ToString();
        metadata["training_intake_idempotency_created_new_side_effect"] = createdNewSideEffect.ToString();
        metadata["training_intake_idempotent_replay_result"] = replayHit.ToString();
        metadata["training_intake_idempotency_side_effect_boundary"] = "promote_local_append_and_local_artifacts_once";
        metadata["training_intake_promote_local_idempotency_payload_hash"] =
            BuildPromoteLocalIdempotencyPayloadHash(intake, options, promotionOutputDirectory);
        metadata["training_intake_promote_local_recorded_at_utc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        metadata["training_intake_local_promotion_record_file"] = TrainingIntakeLocalPromotionFileName;

        return result with { BoundaryMetadata = metadata };
    }

    private static bool HasSamePromoteLocalIdempotencyBoundary(
        IReadOnlyDictionary<string, string>? existing,
        IReadOnlyDictionary<string, string>? candidate)
        => string.Equals(
               ReadMetadata(existing, "training_intake_tenant_id_ref"),
               ReadMetadata(candidate, "training_intake_tenant_id_ref"),
               StringComparison.Ordinal)
           && string.Equals(
               ReadMetadata(existing, "training_intake_idempotency_key_ref"),
               ReadMetadata(candidate, "training_intake_idempotency_key_ref"),
               StringComparison.Ordinal);

    private static bool HasPromoteLocalIdempotencyBoundary(
        IReadOnlyDictionary<string, string>? metadata)
        => !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_tenant_id_ref"))
           && !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "training_intake_idempotency_key_ref"));

    private static string BuildPromoteLocalIdempotencyPayloadHash(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        string promotionOutputDirectory)
        => $"sha256:{HashText(string.Join("|",
        [
            TrainingIntakeLocalPromotionSchemaVersion,
            intake.IntakeId,
            string.IsNullOrWhiteSpace(options.RunId) ? BuildTrainingIntakeRunId(intake) : options.RunId,
            JsonSerializer.Serialize(options, JsonOptions),
            NormalizePromotionOutputDirectoryForIdempotency(promotionOutputDirectory)
        ]))}";

    private static string NormalizePromotionOutputDirectoryForIdempotency(string promotionOutputDirectory)
    {
        if (string.IsNullOrWhiteSpace(promotionOutputDirectory))
            return string.Empty;

        try
        {
            return Path.GetFullPath(promotionOutputDirectory);
        }
        catch
        {
            return promotionOutputDirectory.Trim();
        }
    }

    private static string BuildTrainingIntakeId(
        DesignConversionApprovedDpoHandoffArtifact handoff,
        string requestedBy,
        string reason,
        DateTimeOffset requestedAtUtc,
        IReadOnlyDictionary<string, string>? admissionBoundaryMetadata)
    {
        var identityParts = new List<string>
        {
            TrainingIntakeSchemaVersion,
            handoff.ArtifactId,
            requestedBy,
            reason,
            requestedAtUtc.ToString("O", CultureInfo.InvariantCulture),
            string.Join(",", (handoff.Rows ?? []).Select(row => row.RecordHash).Order(StringComparer.Ordinal))
        };

        var tenantRef = ReadMetadata(admissionBoundaryMetadata, "training_intake_tenant_id_ref");
        if (!string.IsNullOrWhiteSpace(tenantRef))
            identityParts.Add($"tenant:{tenantRef}");

        return $"training-intake-{HashText(string.Join("|", identityParts))}";
    }

    private static DesignConversionTrainingIntakeSourceProof BuildTrainingIntakeSourceProof(
        DesignConversionTrainingIntakeRecord intake)
        => BuildTrainingIntakeSourceProof(intake.CandidateRefs);

    private static DesignConversionTrainingIntakeSourceProof BuildTrainingIntakeSourceProof(
        IReadOnlyList<DesignConversionTrainingIntakeCandidateRef>? candidateRefs)
    {
        var candidates = candidateRefs ?? [];
        var blockReasons = new SortedSet<string>(StringComparer.Ordinal);
        var evidenceRefs = new SortedSet<string>(StringComparer.Ordinal)
        {
            "source_manifest:local_candidate_hash_refs_only",
            "source_disjoint:local_candidate_source_identity_hashes_only"
        };

        var missingManifest = candidates.Count == 0
            || candidates.Any(candidate =>
                candidate is null
                || string.IsNullOrWhiteSpace(candidate.SourceManifestRef)
                || string.IsNullOrWhiteSpace(candidate.SourceManifestHash)
                || string.IsNullOrWhiteSpace(candidate.SourceIdentityHash));
        if (missingManifest)
        {
            blockReasons.Add("source_manifest_missing");
            blockReasons.Add("source_disjoint_missing");
            evidenceRefs.Add("source_manifest:missing");
            evidenceRefs.Add("source_disjoint:not_proven");
        }

        var sourceGroups = candidates
            .Where(candidate => candidate is not null
                && !string.IsNullOrWhiteSpace(candidate.SourceIdentityHash))
            .Select(candidate => candidate.SourceIdentityHash)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestHashCount = candidates
            .Where(candidate => candidate is not null
                && !string.IsNullOrWhiteSpace(candidate.SourceManifestHash))
            .Select(candidate => candidate.SourceManifestHash)
            .Distinct(StringComparer.Ordinal)
            .Count();

        string sourceManifestStatus;
        string sourceDisjointStatus;
        IReadOnlyList<string> heldOutSourceIdentityHashes = [];
        IReadOnlyList<string> trainSourceIdentityHashes = [];
        IReadOnlyList<string> overlapSourceIdentityHashes = [];
        if (missingManifest)
        {
            sourceManifestStatus = "missing";
            sourceDisjointStatus = "missing";
        }
        else if (sourceGroups.Length < 2)
        {
            sourceManifestStatus = "proven";
            sourceDisjointStatus = "blocked_same_source";
            blockReasons.Add("source_disjoint_blocked_same_source");
            evidenceRefs.Add("source_manifest:proven");
            evidenceRefs.Add("source_disjoint:blocked_same_source");
            heldOutSourceIdentityHashes = sourceGroups.Take(1).ToArray();
        }
        else
        {
            sourceManifestStatus = "proven";
            heldOutSourceIdentityHashes = sourceGroups.Take(1).ToArray();
            trainSourceIdentityHashes = sourceGroups.Skip(1).ToArray();
            overlapSourceIdentityHashes = trainSourceIdentityHashes
                .Intersect(heldOutSourceIdentityHashes, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (overlapSourceIdentityHashes.Count > 0 || trainSourceIdentityHashes.Count == 0)
            {
                sourceDisjointStatus = "blocked_same_source";
                blockReasons.Add("source_disjoint_blocked_same_source");
                evidenceRefs.Add("source_disjoint:blocked_same_source");
            }
            else
            {
                sourceDisjointStatus = "proven";
                evidenceRefs.Add("source_manifest:proven");
                evidenceRefs.Add("source_disjoint:proven");
            }
        }

        return new DesignConversionTrainingIntakeSourceProof(
            SourceManifestStatus: sourceManifestStatus,
            SourceDisjointStatus: sourceDisjointStatus,
            SourceManifestProven: string.Equals(sourceManifestStatus, "proven", StringComparison.Ordinal),
            SourceDisjointProven: string.Equals(sourceDisjointStatus, "proven", StringComparison.Ordinal),
            CandidateCount: candidates.Count,
            SourceManifestHashCount: manifestHashCount,
            HeldOutSourceIdentityHashes: heldOutSourceIdentityHashes,
            TrainSourceIdentityHashes: trainSourceIdentityHashes,
            OverlapSourceIdentityHashes: overlapSourceIdentityHashes,
            BlockReasons: blockReasons.ToArray(),
            EvidenceRefs: evidenceRefs.ToArray());
    }

    private static string BuildTrainingIntakeSourceManifestHash(string sourceManifestRef)
        => string.IsNullOrWhiteSpace(sourceManifestRef)
            ? string.Empty
            : $"sha256:{HashText($"training-intake-source-manifest|{sourceManifestRef}")}";

    private static string BuildTrainingIntakeSourceIdentityHash(
        string sourceManifestRef,
        string sourceStructureHash,
        string trainingRecordId)
    {
        if (!string.IsNullOrWhiteSpace(sourceManifestRef))
            return $"sha256:{HashText($"source_manifest_ref|{sourceManifestRef}")}";
        if (!string.IsNullOrWhiteSpace(sourceStructureHash))
            return $"sha256:{HashText($"source_structure_hash|{sourceStructureHash}")}";
        if (!string.IsNullOrWhiteSpace(trainingRecordId))
            return $"sha256:{HashText($"training_record_id|{trainingRecordId}")}";

        return string.Empty;
    }

    private static DesignConversionTrainingIntakePreflightResult BuildTrainingIntakePreflightResult(
        DesignConversionTrainingIntakeRecord intake)
    {
        var sourceProof = BuildTrainingIntakeSourceProof(intake);
        IReadOnlyList<string> blockReasons = BuildTrainingIntakePreflightBlockReasons(intake)
            .Concat(sourceProof.BlockReasons)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var mayStartTraining = blockReasons.Count == 0;
        return new DesignConversionTrainingIntakePreflightResult(
            PreflightId: BuildTrainingIntakePreflightId(intake),
            TrainingIntakeId: intake.IntakeId,
            AcceptedCandidateCount: intake.AcceptedCandidateCount,
            MayStartTraining: mayStartTraining,
            SourceManifestStatus: sourceProof.SourceManifestStatus,
            SourceDisjointStatus: sourceProof.SourceDisjointStatus,
            TrainingStarted: false,
            CheckpointMutated: false,
            WeightsProduced: false,
            ModelUploadReady: false,
            Warnings: BuildTrainingIntakePreflightWarnings(intake, mayStartTraining, sourceProof, blockReasons),
            BlockReasons: blockReasons,
            BoundaryMetadata: BuildTrainingIntakePreflightBoundaryMetadata(intake, mayStartTraining, sourceProof, blockReasons));
    }

    private static IReadOnlyList<string> BuildTrainingIntakePreflightBlockReasons(
        DesignConversionTrainingIntakeRecord intake)
    {
        var blockReasons = new SortedSet<string>(StringComparer.Ordinal);
        if (ContainsForbiddenTrainingIntakePayload(intake))
            blockReasons.Add("raw_or_private_marker_detected");

        if (string.IsNullOrWhiteSpace(intake.IntakeId))
            blockReasons.Add("training_intake_id_required");
        if (intake.AcceptedCandidateCount > 0 && string.IsNullOrWhiteSpace(intake.IntakeRef))
            blockReasons.Add("training_intake_ref_required");
        if (string.IsNullOrWhiteSpace(intake.SourceHandoffArtifactId)
            || string.IsNullOrWhiteSpace(intake.SourceHandoffArtifactRef))
        {
            blockReasons.Add("source_handoff_ref_required");
        }

        if (string.IsNullOrWhiteSpace(intake.RequestedByRef))
            blockReasons.Add("safe_requester_required");
        if (string.IsNullOrWhiteSpace(intake.RequestReason))
            blockReasons.Add("safe_reason_required");
        if (intake.AcceptedCandidateCount <= 0 || (intake.CandidateRefs?.Count ?? 0) == 0)
            blockReasons.Add("no_accepted_candidates");
        if (intake.CandidateRefs is not null
            && intake.AcceptedCandidateCount != intake.CandidateRefs.Count)
        {
            blockReasons.Add("accepted_candidate_count_mismatch");
        }

        if (intake.ApprovedCandidateCount < intake.AcceptedCandidateCount)
            blockReasons.Add("approved_candidate_count_less_than_accepted");

        if (!intake.TrainingIntakeOnly)
            blockReasons.Add("training_intake_only_required");
        if (!intake.TrainingNotStarted)
            blockReasons.Add("training_not_started_required");
        if (!intake.NotTrained)
            blockReasons.Add("not_trained_required");
        if (!intake.WeightsNotProduced)
            blockReasons.Add("weights_not_produced_required");
        if (!intake.CheckpointNotMutated)
            blockReasons.Add("checkpoint_not_mutated_required");
        if (!intake.ModelUploadNotReady)
            blockReasons.Add("model_upload_not_ready_required");
        if (!intake.ProductNotReady)
            blockReasons.Add("product_not_ready_required");
        if (!intake.HeldOutNotProven)
            blockReasons.Add("held_out_not_proven_required");
        if (!intake.SourceDisjointNotProven)
            blockReasons.Add("source_disjoint_not_proven_required");

        AddRequiredBoundaryBlocks(
            blockReasons,
            intake.BoundaryMetadata,
            RequiredTrainingIntakePreflightTrueBoundaryKeys,
            mustBeTrue: true);
        AddRequiredBoundaryBlocks(
            blockReasons,
            intake.BoundaryMetadata,
            RequiredTrainingIntakePreflightFalseBoundaryKeys,
            mustBeTrue: false);

        foreach (var candidate in intake.CandidateRefs ?? [])
            AddTrainingIntakeCandidatePreflightBlocks(blockReasons, candidate);

        return blockReasons.ToArray();
    }

    private static void AddTrainingIntakeCandidatePreflightBlocks(
        SortedSet<string> blockReasons,
        DesignConversionTrainingIntakeCandidateRef candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate.LocalArtifactRecordId)
            || string.IsNullOrWhiteSpace(candidate.CandidateId)
            || string.IsNullOrWhiteSpace(candidate.TrainingRecordId)
            || string.IsNullOrWhiteSpace(candidate.UnitId))
        {
            blockReasons.Add("candidate_ref_identity_required");
        }

        if (string.IsNullOrWhiteSpace(candidate.HandoffRecordHash)
            || string.IsNullOrWhiteSpace(candidate.SourceManifestHash)
            || string.IsNullOrWhiteSpace(candidate.SourceIdentityHash)
            || string.IsNullOrWhiteSpace(candidate.InternalUnitStateHash)
            || string.IsNullOrWhiteSpace(candidate.SourceStructureHash)
            || string.IsNullOrWhiteSpace(candidate.ChosenPayloadHash)
            || string.IsNullOrWhiteSpace(candidate.RejectedPayloadHash)
            || string.IsNullOrWhiteSpace(candidate.TieBreakerHash))
        {
            blockReasons.Add("candidate_ref_hash_required");
        }

        if (string.IsNullOrWhiteSpace(candidate.SourceManifestRef))
            blockReasons.Add("candidate_source_manifest_ref_required");

        if (string.IsNullOrWhiteSpace(candidate.ChosenHtmlRef)
            || string.IsNullOrWhiteSpace(candidate.ChosenCssRef)
            || string.IsNullOrWhiteSpace(candidate.RejectedHtmlRef)
            || string.IsNullOrWhiteSpace(candidate.RejectedCssRef)
            || string.IsNullOrWhiteSpace(candidate.DesignTreeRef)
            || string.IsNullOrWhiteSpace(candidate.OriginalTreeRef)
            || (candidate.SourceArtifactRefs?.Count ?? 0) == 0
            || (candidate.RenderArtifactRefs?.Count ?? 0) == 0
            || (candidate.ComparisonArtifactRefs?.Count ?? 0) == 0)
        {
            blockReasons.Add("candidate_runtime_artifact_refs_required");
        }

        AddRequiredBoundaryBlocks(
            blockReasons,
            candidate.BoundaryMetadata,
            RequiredTrainingIntakePreflightTrueBoundaryKeys,
            mustBeTrue: true);
    }

    private static void AddRequiredBoundaryBlocks(
        SortedSet<string> blockReasons,
        IReadOnlyDictionary<string, string>? metadata,
        IReadOnlyList<string> requiredKeys,
        bool mustBeTrue)
    {
        if (metadata is null || metadata.Count == 0)
        {
            blockReasons.Add("boundary_metadata_required");
            return;
        }

        foreach (var key in requiredKeys)
        {
            var value = ReadMetadata(metadata, key);
            if (mustBeTrue && !IsTruthy(value))
                blockReasons.Add($"boundary_metadata_{key}_required");
            if (!mustBeTrue
                && !string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase))
            {
                blockReasons.Add($"boundary_metadata_{key}_must_be_false");
            }
        }
    }

    private static IReadOnlyList<string> BuildTrainingIntakePreflightWarnings(
        DesignConversionTrainingIntakeRecord intake,
        bool mayStartTraining,
        DesignConversionTrainingIntakeSourceProof sourceProof,
        IReadOnlyList<string> blockReasons)
    {
        var warnings = new SortedSet<string>(intake.Warnings ?? [], StringComparer.Ordinal)
        {
            "training_intake_preflight_only",
            "future_training_run_permission_only",
            "training_not_started",
            "not_trained",
            "checkpoint_not_mutated",
            "weights_not_produced",
            "model_upload_not_ready",
            $"source_manifest_status:{sourceProof.SourceManifestStatus}",
            $"source_disjoint_status:{sourceProof.SourceDisjointStatus}",
            "preflight_does_not_invoke_training_runner"
        };
        warnings.Add(mayStartTraining
            ? "may_start_training_future_run_only"
            : "training_intake_preflight_blocked");
        foreach (var blockReason in blockReasons)
            warnings.Add($"block_reason:{blockReason}");

        return warnings.ToArray();
    }

    private static IReadOnlyDictionary<string, string> BuildTrainingIntakePreflightBoundaryMetadata(
        DesignConversionTrainingIntakeRecord intake,
        bool mayStartTraining,
        DesignConversionTrainingIntakeSourceProof sourceProof,
        IReadOnlyList<string> blockReasons)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["schema_version"] = TrainingIntakePreflightSchemaVersion,
            ["source_training_intake_schema_version"] = ReadMetadata(intake.BoundaryMetadata, "schema_version"),
            ["training_intake_id"] = intake.IntakeId,
            ["preflight_only"] = bool.TrueString,
            ["future_training_run_permission_only"] = bool.TrueString,
            ["may_start_training"] = mayStartTraining.ToString(),
            ["accepted_candidate_count"] = intake.AcceptedCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["block_reason_count"] = blockReasons.Count.ToString(CultureInfo.InvariantCulture),
            ["source_manifest_status"] = sourceProof.SourceManifestStatus,
            ["source_disjoint_status"] = sourceProof.SourceDisjointStatus,
            ["source_manifest_proven"] = sourceProof.SourceManifestProven.ToString(),
            ["source_disjoint_proven"] = sourceProof.SourceDisjointProven.ToString(),
            ["source_manifest_candidate_count"] = sourceProof.CandidateCount.ToString(CultureInfo.InvariantCulture),
            ["source_manifest_hash_count"] = sourceProof.SourceManifestHashCount.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_train_source_count"] = sourceProof.TrainSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_heldout_source_count"] = sourceProof.HeldOutSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_overlap_count"] = sourceProof.OverlapSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["training_intake_only"] = bool.TrueString,
            ["training_grade"] = bool.FalseString,
            ["not_training_grade"] = bool.TrueString,
            ["training_grade_boundary"] = "training_intake_preflight_not_training_grade",
            ["training_not_started"] = bool.TrueString,
            ["training_started"] = bool.FalseString,
            ["not_trained"] = bool.TrueString,
            ["checkpoint_mutated"] = bool.FalseString,
            ["checkpoint_not_mutated"] = bool.TrueString,
            ["weights_produced"] = bool.FalseString,
            ["weights_not_produced"] = bool.TrueString,
            ["model_upload_ready"] = bool.FalseString,
            ["model_upload_not_ready"] = bool.TrueString,
            ["model_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
            ["model_readiness_claimed"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["training_execution_boundary"] = "local_preflight_only_no_training_runner",
            ["checkpoint_mutation_boundary"] = "not_mutated",
            ["weight_output_boundary"] = "not_produced",
            ["model_upload_boundary"] = "not_ready"
        };

    private static string BuildTrainingIntakePreflightId(DesignConversionTrainingIntakeRecord intake)
        => $"training-intake-preflight-{HashText(string.Join("|",
        [
            TrainingIntakePreflightSchemaVersion,
            intake.IntakeId,
            intake.AcceptedCandidateCount.ToString(CultureInfo.InvariantCulture),
            string.Join(",", (intake.CandidateRefs ?? [])
                .Select(candidate => candidate.HandoffRecordHash)
                .Order(StringComparer.Ordinal))
        ]))}";

    private async Task<OperationResult<DesignConversionTrainingIntakeRecord>> FindQueuedTrainingIntakeAsync(
        string intakeId,
        CancellationToken ct)
    {
        var intakes = await ListTrainingIntakeRequestsAsync(ct);
        if (!intakes.IsSuccess)
            return intakes.MapError<DesignConversionTrainingIntakeRecord>();

        var matches = intakes.Value!
            .Where(record => string.Equals(record.IntakeId, intakeId, StringComparison.Ordinal))
            .ToArray();
        if (matches.Length == 0)
            return OperationResult<DesignConversionTrainingIntakeRecord>.Fail(
                "DesignConversion training intake run could not find the queued intake record.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_NOT_FOUND");
        if (matches.Length > 1)
            return OperationResult<DesignConversionTrainingIntakeRecord>.Fail(
                "DesignConversion training intake run found duplicate intake ids.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_AMBIGUOUS");

        return OperationResult<DesignConversionTrainingIntakeRecord>.Ok(matches[0]);
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeRecordResolution>> ResolveTrainingIntakeRecordsAsync(
        DesignConversionTrainingIntakeRecord intake,
        CancellationToken ct)
    {
        var records = await ListRecordsAsync(null, ct);
        if (!records.IsSuccess)
            return records.MapError<DesignConversionTrainingIntakeRecordResolution>();

        var resolved = new List<DesignConversionDpoTrainingRecord>();
        var blockReasons = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var candidateRef in intake.CandidateRefs ?? [])
        {
            if (candidateRef is null)
            {
                blockReasons.Add("candidate_ref_required");
                continue;
            }

            var localRecord = records.Value!.FirstOrDefault(record =>
                string.Equals(record.RecordId, candidateRef.LocalArtifactRecordId, StringComparison.Ordinal));
            if (localRecord is null)
            {
                blockReasons.Add($"local_artifact_record_not_found:{candidateRef.TrainingRecordId}");
                continue;
            }

            var candidate = localRecord.DpoCandidates.FirstOrDefault(item =>
                string.Equals(item.CandidateId, candidateRef.CandidateId, StringComparison.Ordinal)
                || string.Equals(item.TrainingRecord.RecordId, candidateRef.TrainingRecordId, StringComparison.Ordinal));
            if (candidate is null)
            {
                blockReasons.Add($"dpo_candidate_not_found:{candidateRef.TrainingRecordId}");
                continue;
            }

            if (!IsApprovedRuntimeHandoffCandidate(localRecord, candidate))
            {
                blockReasons.Add($"approved_runtime_handoff_candidate_required:{candidateRef.TrainingRecordId}");
                continue;
            }

            var blockCountBeforeHashChecks = blockReasons.Count;
            var expectedRow = BuildHandoffRow(localRecord, candidate);
            var expectedSourceManifestRef = expectedRow.SourceManifestRef;
            var expectedSourceManifestHash = BuildTrainingIntakeSourceManifestHash(expectedSourceManifestRef);
            var expectedSourceIdentityHash = BuildTrainingIntakeSourceIdentityHash(
                expectedSourceManifestRef,
                expectedRow.SourceStructureHash,
                expectedRow.TrainingRecordId);
            AddCandidateRefHashBlock(
                blockReasons,
                candidateRef.HandoffRecordHash,
                expectedRow.RecordHash,
                "handoff_record_hash_mismatch",
                candidateRef.TrainingRecordId);
            if (!string.Equals(
                candidateRef.SourceManifestRef,
                expectedSourceManifestRef,
                StringComparison.Ordinal))
            {
                blockReasons.Add($"source_manifest_ref_mismatch:{candidateRef.TrainingRecordId}");
            }

            AddCandidateRefHashBlock(
                blockReasons,
                candidateRef.SourceManifestHash,
                expectedSourceManifestHash,
                "source_manifest_hash_mismatch",
                candidateRef.TrainingRecordId);
            AddCandidateRefHashBlock(
                blockReasons,
                candidateRef.SourceIdentityHash,
                expectedSourceIdentityHash,
                "source_identity_hash_mismatch",
                candidateRef.TrainingRecordId);
            AddCandidateRefHashBlock(
                blockReasons,
                candidateRef.InternalUnitStateHash,
                expectedRow.InternalUnitStateHash,
                "internal_unit_state_hash_mismatch",
                candidateRef.TrainingRecordId);
            AddCandidateRefHashBlock(
                blockReasons,
                candidateRef.SourceStructureHash,
                expectedRow.SourceStructureHash,
                "source_structure_hash_mismatch",
                candidateRef.TrainingRecordId);
            AddCandidateRefHashBlock(
                blockReasons,
                candidateRef.ChosenPayloadHash,
                expectedRow.ChosenPayloadHash,
                "chosen_payload_hash_mismatch",
                candidateRef.TrainingRecordId);
            AddCandidateRefHashBlock(
                blockReasons,
                candidateRef.RejectedPayloadHash,
                expectedRow.RejectedPayloadHash,
                "rejected_payload_hash_mismatch",
                candidateRef.TrainingRecordId);
            AddCandidateRefHashBlock(
                blockReasons,
                candidateRef.TieBreakerHash,
                expectedRow.TieBreakerHash,
                "tie_breaker_hash_mismatch",
                candidateRef.TrainingRecordId);

            if (blockReasons.Count == blockCountBeforeHashChecks)
                resolved.Add(candidate.TrainingRecord);
        }

        if (resolved.Count != intake.AcceptedCandidateCount)
            blockReasons.Add("resolved_training_record_count_mismatch");

        return OperationResult<DesignConversionTrainingIntakeRecordResolution>.Ok(
            new DesignConversionTrainingIntakeRecordResolution(
                Records: resolved.ToArray(),
                BlockReasons: blockReasons.ToArray()));
    }

    private static void AddCandidateRefHashBlock(
        SortedSet<string> blockReasons,
        string actual,
        string expected,
        string reason,
        string trainingRecordId)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            blockReasons.Add($"{reason}:{trainingRecordId}");
    }

    private static DesignConversionTrainingPipelineOptions BuildDefaultTrainingIntakeRunOptions(
        DesignConversionTrainingIntakeRecord intake)
    {
        var runId = BuildTrainingIntakeRunId(intake);
        var sourceProof = BuildTrainingIntakeSourceProof(intake);
        var preflight = sourceProof.SourceManifestProven && sourceProof.SourceDisjointProven
            ? DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof(sourceProof.EvidenceRefs)
            : DesignConversionDpoDatasetPreflight.MissingSourceManifestAfterApproval(
            [
                "training_intake_preflight:blocked_source_manifest_or_disjoint",
                $"source_manifest_status:{sourceProof.SourceManifestStatus}",
                $"source_disjoint_status:{sourceProof.SourceDisjointStatus}"
            ]);
        return new DesignConversionTrainingPipelineOptions(
            RunId: runId,
            BatchSize: Math.Max(1, Math.Min(25, intake.AcceptedCandidateCount)),
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: runId,
                Preflight: preflight));
    }

    private static DesignConversionTrainingIntakeRunResult BuildTrainingIntakeRunResult(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionTrainingPipelineOptions options,
        bool optionsProvided,
        int resolvedTrainingRecordCount,
        DesignConversionTrainingRunResult? runnerResult,
        IReadOnlyList<string> blockReasons,
        string trainingExecutionBoundary)
    {
        var sourceProof = BuildTrainingIntakeSourceProof(intake);
        var runnerEvidence = runnerResult is null ? null : BuildTrainingIntakeRunnerEvidence(runnerResult);
        var localLifecycleHookAttempted = runnerEvidence?.MachineLifecycleHookAttempted == true;
        var trainingStarted = runnerEvidence?.TrainingStarted == true;
        var weightsProduced = runnerEvidence?.CheckpointContainsRealTrainedWeights == true;
        var checkpointMutated = runnerEvidence?.CheckpointMutationEvidenceProven == true;
        var uploadReadiness = BuildTrainingIntakeUploadReadiness(
            runnerResult,
            runnerEvidence,
            weightsProduced);
        var modelUploadReady = uploadReadiness.ModelUploadReady;
        var trainingExecutionStatus = BuildTrainingExecutionStatus(
            preflight,
            runnerResult,
            runnerEvidence,
            trainingStarted);
        var runnerBlockedReason = BuildRunnerBlockedReason(
            preflight,
            runnerResult,
            runnerEvidence,
            blockReasons,
            trainingStarted);
        var checkpointStatus = BuildCheckpointStatus(runnerEvidence);
        var checkpointMutationStatus = BuildCheckpointMutationStatus(runnerEvidence);
        var weightProofStatus = BuildWeightProofStatus(weightsProduced);
        var finalBlockReasons = BuildTrainingIntakeRunBlockReasons(
            preflight,
            runnerResult,
            blockReasons,
            trainingStarted,
            checkpointMutated,
            weightsProduced,
            uploadReadiness.PromotionBlockedReasons,
            modelUploadReady);
        var mayStartTraining = preflight.MayStartTraining && runnerResult is not null;
        var warnings = BuildTrainingIntakeRunWarnings(
            preflight,
            runnerResult,
            runnerEvidence,
            trainingStarted,
            checkpointMutated,
            weightsProduced,
            modelUploadReady,
            trainingExecutionStatus,
            runnerBlockedReason,
            checkpointStatus,
            checkpointMutationStatus,
            weightProofStatus,
            uploadReadiness);
        var runId = string.IsNullOrWhiteSpace(options.RunId)
            ? BuildTrainingIntakeRunId(intake)
            : options.RunId;

        return new DesignConversionTrainingIntakeRunResult(
            RunId: runId,
            TrainingIntakeId: intake.IntakeId,
            PreflightId: preflight.PreflightId,
            AcceptedCandidateCount: intake.AcceptedCandidateCount,
            ResolvedTrainingRecordCount: resolvedTrainingRecordCount,
            MayStartTraining: mayStartTraining,
            SourceManifestStatus: sourceProof.SourceManifestStatus,
            SourceDisjointStatus: sourceProof.SourceDisjointStatus,
            TrainingRunnerInvoked: runnerResult is not null,
            LocalLifecycleHookAttempted: localLifecycleHookAttempted,
            TrainingStarted: trainingStarted,
            CheckpointMutated: checkpointMutated,
            CheckpointCandidateCreated: runnerEvidence?.CheckpointCandidateCreated == true,
            WeightsProduced: weightsProduced,
            ModelUploadReady: modelUploadReady,
            TrainingExecutionBoundary: trainingExecutionBoundary,
            TrainingExecutionStatus: trainingExecutionStatus,
            RunnerBlockedReason: runnerBlockedReason,
            CheckpointStatus: checkpointStatus,
            CheckpointMutationStatus: checkpointMutationStatus,
            WeightProofStatus: weightProofStatus,
            MeasurementGateStatus: uploadReadiness.MeasurementGateStatus,
            PromotionGateStatus: uploadReadiness.PromotionGateStatus,
            UploadReadinessStatus: uploadReadiness.UploadReadinessStatus,
            PromotionBlockedReasons: uploadReadiness.PromotionBlockedReasons,
            RunnerEvidence: runnerEvidence,
            Warnings: warnings,
            BlockReasons: finalBlockReasons,
            BoundaryMetadata: BuildTrainingIntakeRunBoundaryMetadata(
                intake,
                preflight,
                options,
                optionsProvided,
                resolvedTrainingRecordCount,
                sourceProof,
                runnerEvidence,
                trainingExecutionBoundary,
                trainingExecutionStatus,
                runnerBlockedReason,
                checkpointStatus,
                checkpointMutationStatus,
                weightProofStatus,
                uploadReadiness,
                trainingStarted,
                checkpointMutated,
                weightsProduced,
                modelUploadReady,
                mayStartTraining,
                finalBlockReasons));
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult BuildTrainingIntakeLocalPromotionResult(
        DesignConversionTrainingIntakeRunResult trainingRun,
        string promotionOutputDirectory,
        string promotedCheckpointPath,
        DesignConversionPromotionPublicationResult? publication,
        DesignConversionPromotedLocalModel? promotedLocalModel,
        IReadOnlyList<string> additionalBlockReasons,
        IReadOnlyList<string> additionalWarnings,
        IReadOnlyList<string> additionalEvidenceRefs)
    {
        var modelServerUploadAttempted = publication?.Manifest.ModelServerUploadAttempted == true;
        var modelServerPromotionClaimed = publication?.Manifest.ModelServerPromotionClaimed == true;
        var modelServerUploadVerified = string.Equals(
            publication?.Manifest.PromotionReceiptKind,
            "model_server_upload_verified",
            StringComparison.Ordinal);
        var localPromotionReady = trainingRun.ModelUploadReady
                                  && promotedLocalModel is
                                  {
                                      LocalOnly: true,
                                      CheckpointFreshLoadVerified: true,
                                      CheckpointContainsRealTrainedWeights: true,
                                      PromotionManifestClaimed: true
                                  };
        var rollbackBoundary = localPromotionReady
            ? "explicit_unsupported_nonready_boundary"
            : "not_applicable_local_promotion_not_ready";
        var rollbackStatus = localPromotionReady
            ? "unsupported_no_prior_active_local_model_checkpoint_state"
            : "not_applicable_no_promoted_local_model";
        var blockReasons = (trainingRun.BlockReasons ?? [])
            .Concat(additionalBlockReasons ?? [])
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var evidenceRefs = (trainingRun.RunnerEvidence?.EvidenceRefs ?? [])
            .Concat(publication?.Decision.EvidenceRefs ?? [])
            .Concat(publication?.Manifest.EvidenceRefs ?? [])
            .Concat(publication?.UploadReceipt?.EvidenceRefs ?? [])
            .Concat(promotedLocalModel?.EvidenceRefs ?? [])
            .Concat(additionalEvidenceRefs ?? [])
            .Append(localPromotionReady
                ? "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READY"
                : "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_NOT_READY")
            .Concat(localPromotionReady
                ? ["DESIGN_CONVERSION_LOCAL_PROMOTION_ROLLBACK_UNSUPPORTED_NONREADY_BOUNDARY"]
                : [])
            .Append(modelServerUploadAttempted
                ? "DESIGN_CONVERSION_TRAINING_INTAKE_MODEL_SERVER_UPLOAD_ATTEMPTED"
                : "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_NO_EXTERNAL_CLOUD_UPLOAD")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var warnings = (trainingRun.Warnings ?? [])
            .Concat(additionalWarnings ?? [])
            .Append(localPromotionReady
                ? "local_promotion_ready"
                : "local_promotion_not_ready")
            .Concat(localPromotionReady ? ["local_promotion_rollback_unsupported"] : [])
            .Append(modelServerUploadAttempted ? "external_cloud_upload_attempted" : "external_cloud_upload_not_attempted")
            .Append(modelServerUploadAttempted ? "cloud_model_upload_attempted" : "cloud_model_upload_not_attempted")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var metadata = new Dictionary<string, string>(trainingRun.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["local_promotion_ready"] = localPromotionReady.ToString(),
            ["local_promotion_not_ready"] = (!localPromotionReady).ToString(),
            ["local_promotion_rollback_supported"] = bool.FalseString,
            ["local_promotion_rollback_boundary"] = rollbackBoundary,
            ["local_promotion_rollback_status"] = rollbackStatus,
            ["local_promotion_previous_model_checkpoint_ref_available"] = bool.FalseString,
            ["local_promotion_restore_previous_state_supported"] = bool.FalseString,
            ["product_ready_blocked_by_local_rollback_boundary"] = localPromotionReady.ToString(),
            ["external_cloud_upload_attempted"] = modelServerUploadAttempted.ToString(),
            ["external_cloud_upload_not_attempted"] = (!modelServerUploadAttempted).ToString(),
            ["external_cloud_upload_not_attempted_local_only"] = (publication is not null && !modelServerUploadAttempted).ToString(),
            ["cloud_model_upload_attempted"] = modelServerUploadAttempted.ToString(),
            ["cloud_model_upload_not_attempted"] = (!modelServerUploadAttempted).ToString(),
            ["model_server_upload_attempted"] = modelServerUploadAttempted.ToString(),
            ["model_server_upload_verified"] = modelServerUploadVerified.ToString(),
            ["model_server_upload_not_attempted_local_only"] = (!modelServerUploadAttempted).ToString(),
            ["model_server_promotion_claimed"] = modelServerPromotionClaimed.ToString(),
            ["model_server_promotion_not_claimed_local_only"] = (!modelServerPromotionClaimed && !modelServerUploadAttempted).ToString(),
            ["promotion_output_directory_configured"] = (!string.IsNullOrWhiteSpace(promotionOutputDirectory)).ToString(),
            ["promoted_checkpoint_path_configured"] = (!string.IsNullOrWhiteSpace(promotedCheckpointPath)).ToString(),
            ["promotion_publication_created"] = (publication is not null).ToString(),
            ["promotion_decision_kind"] = publication?.Decision.Kind.ToString() ?? "not_attempted",
            ["promotion_upload_ref"] = publication?.Manifest.PromotionUploadRef ?? string.Empty,
            ["promotion_upload_ref_local_only"] = (publication?.Manifest.PromotionUploadRef?.StartsWith(
                DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
                StringComparison.Ordinal) == true).ToString(),
            ["local_promotion_receipt_written"] = (publication?.Manifest.LocalPromotionReceiptWritten == true).ToString(),
            ["promotion_receipt_kind"] = publication?.Manifest.PromotionReceiptKind ?? "none",
            ["model_server_upload_not_attempted_local_receipt"] = (publication?.Manifest.LocalPromotionReceiptWritten == true &&
                                                                   publication.Manifest.ModelServerUploadAttempted == false).ToString(),
            ["model_server_promotion_not_claimed_local_receipt"] = (publication?.Manifest.LocalPromotionReceiptWritten == true &&
                                                                    publication.Manifest.ModelServerPromotionClaimed == false).ToString(),
            ["local_promoted_model_present"] = (promotedLocalModel is not null).ToString(),
            ["local_promoted_checkpoint_loaded"] = (promotedLocalModel?.CheckpointFreshLoadVerified == true).ToString(),
            ["local_promoted_checkpoint_real_weights"] = (promotedLocalModel?.CheckpointContainsRealTrainedWeights == true).ToString(),
            ["local_promoted_model_id"] = promotedLocalModel?.ModelId ?? string.Empty,
            ["local_promoted_model_version"] = promotedLocalModel?.Version ?? string.Empty,
            ["local_promoted_model_local_only"] = (promotedLocalModel?.LocalOnly == true).ToString(),
            ["local_promoted_checkpoint_weights_source"] = promotedLocalModel?.LocalCheckpointWeightsSource ?? string.Empty,
            ["local_promoted_source_provenance_ref"] = promotedLocalModel?.SourceProvenanceRef ?? string.Empty,
            ["local_promoted_dpo_trainer_policy_checkpoint_converted"] =
                (promotedLocalModel?.DpoTrainerPolicyCheckpointConverted == true).ToString(),
            ["local_promoted_dpo_trainer_policy_checkpoint_fresh_loaded"] =
                (promotedLocalModel?.DpoTrainerPolicyCheckpointFreshLoaded == true).ToString(),
            ["local_promoted_dpo_policy_checkpoint_sha256_ref"] =
                promotedLocalModel?.DpoPolicyCheckpointSha256Ref ?? string.Empty,
            ["local_promoted_dpo_policy_checkpoint_file_ref"] =
                promotedLocalModel?.DpoPolicyCheckpointFileRef ?? string.Empty,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };
        AddTrainingDatabaseDurabilityMetadata(
            metadata,
            TrainingIntakeLocalPromotionSchemaVersion,
            "training-intake-local-promotion",
            trainingRun.ResolvedTrainingRecordCount,
            [
                publication?.Manifest.PromotionUploadRef ?? string.Empty,
                promotedLocalModel?.ManifestRef ?? string.Empty,
                promotedLocalModel?.DpoPolicyCheckpointSha256Ref ?? string.Empty,
                trainingRun.SourceManifestStatus,
                trainingRun.SourceDisjointStatus
            ],
            [
                trainingRun.TrainingIntakeId,
                trainingRun.RunId,
                trainingRun.ResolvedTrainingRecordCount.ToString(CultureInfo.InvariantCulture),
                promotionOutputDirectory,
                promotedCheckpointPath,
                localPromotionReady.ToString(),
                publication?.Manifest.ManifestId ?? string.Empty,
                promotedLocalModel?.ModelId ?? string.Empty,
                promotedLocalModel?.Version ?? string.Empty
            ]);

        return new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: trainingRun,
            LocalPromotionReady: localPromotionReady,
            ExternalCloudUploadAttempted: modelServerUploadAttempted,
            PromotionOutputDirectory: string.IsNullOrWhiteSpace(promotionOutputDirectory)
                ? string.Empty
                : Path.GetFullPath(promotionOutputDirectory),
            PromotedCheckpointPath: promotedCheckpointPath,
            Publication: publication,
            PromotedLocalModel: promotedLocalModel,
            EvidenceRefs: evidenceRefs,
            Warnings: warnings,
            BlockReasons: blockReasons,
            BoundaryMetadata: metadata);
    }

    private static async Task<OperationResult<DesignConversionTrainingManifestArtifacts>> WriteLocalPromotionArtifactsAsync(
        string outputDirectory,
        string checkpointPath,
        string runId,
        CancellationToken ct)
    {
        try
        {
            Directory.CreateDirectory(outputDirectory);
            var safeRunId = ToSafeLocalPromotionFileName(runId);
            var tokenizerPath = Path.Combine(outputDirectory, $"{safeRunId}-tokenizer.json");
            var policyPath = Path.Combine(outputDirectory, $"{safeRunId}-policy.json");
            await File.WriteAllTextAsync(
                tokenizerPath,
                "{\"kind\":\"design_conversion_local_training_intake_tokenizer_boundary\"}",
                ct);
            await File.WriteAllTextAsync(
                policyPath,
                "{\"kind\":\"design_conversion_local_training_intake_policy_boundary\",\"external_cloud_upload\":false}",
                ct);

            return OperationResult<DesignConversionTrainingManifestArtifacts>.Ok(
                new DesignConversionTrainingManifestArtifacts(
                    CheckpointPath: checkpointPath,
                    TokenizerPath: tokenizerPath,
                    PolicyPath: policyPath));
        }
        catch (Exception ex)
        {
            return OperationResult<DesignConversionTrainingManifestArtifacts>.Fail(
                ex.Message,
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ARTIFACT_WRITE_FAILED");
        }
    }

    private static DesignConversionPromotionGateReport BuildLocalPromotionGateReport(
        DesignConversionTrainingPipelineOptions options,
        DesignConversionTrainingPipelineResult pipelineResult,
        DesignConversionTrainingManifestArtifacts artifacts)
    {
        var measurement = pipelineResult.LifecycleEvidence.MeasurementGate;
        var heldOutScore = measurement?.HeldOutQualityScore
                           ?? options.MeasurementGateRequest?.HeldOutMetrics?.HeldOutQualityScore
                           ?? 0.0;
        var requiredScore = measurement?.RequiredHeldOutQualityScore
                            ?? options.MeasurementGateRequest?.Thresholds.RequiredHeldOutQualityScore
                            ?? 1.0;
        var sourceDisjoint = measurement?.SourceDisjoint
                             ?? options.MeasurementGateRequest?.IntegrityProof?.SourceDisjoint
                             ?? false;
        var hiddenDeterminism = options.MeasurementGateRequest?.IntegrityProof?.HiddenDeterminismDetected == true;
        var budgetPassed = measurement?.BudgetPassed == true;
        var paths = new[]
        {
            artifacts.CheckpointPath,
            artifacts.TokenizerPath,
            artifacts.PolicyPath
        };

        return new DesignConversionPromotionGateReport(
            HeldOutScore: heldOutScore,
            RequiredHeldOutScore: requiredScore,
            HeldOutSourceDisjoint: sourceDisjoint,
            DeterministicShortcutDetected: hiddenDeterminism,
            HiddenShortcutEvidenceDetected: hiddenDeterminism,
            FreshLoadPassed: string.Equals(
                pipelineResult.LifecycleEvidence.FreshLoadStatus,
                DesignConversionLocalFreshLoadProbe.VerifiedStatus,
                StringComparison.Ordinal)
                             && pipelineResult.LifecycleEvidence.CheckpointContainsRealTrainedWeights,
            BudgetTierPolicyPassed: budgetPassed,
            BudgetTier: "local-training-intake-only",
            DpoQuarantineCount: pipelineResult.QuarantinedRecordCount,
            MaxDpoQuarantineCount: 0,
            RequiredArtifactPaths: paths,
            PresentArtifactPaths: paths.Where(File.Exists).ToArray(),
            EvidenceRefs: pipelineResult.LifecycleEvidence.EvidenceRefs
                .Concat(measurement?.EvidenceRefs ?? [])
                .Append("DESIGN_CONVERSION_LOCAL_PROMOTION_GATE_FROM_TRAINING_INTAKE")
                .Append("DESIGN_CONVERSION_LOCAL_PROMOTION_GATE_NO_EXTERNAL_CLOUD_UPLOAD")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    private static string ToSafeLocalPromotionFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string((string.IsNullOrWhiteSpace(value) ? "design-conversion-local-promotion" : value)
            .Select(ch => invalid.Contains(ch) || char.IsWhiteSpace(ch) ? '-' : ch)
            .ToArray()).Trim('-');
        return string.IsNullOrWhiteSpace(cleaned)
            ? "design-conversion-local-promotion"
            : cleaned;
    }

    private sealed class LocalOnlyTrainingIntakePromotionUploader : IDesignConversionPromotionUploader
    {
        public Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
            DesignConversionTrainingManifest manifest,
            DesignConversionPromotionGateReport gateReport,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<DesignConversionPromotionUploadReceipt>.Ok(
                new DesignConversionPromotionUploadReceipt(
                    UploadId: $"{manifest.ManifestId}:local-only",
                    RemoteRef: DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                               Uri.EscapeDataString(manifest.ManifestId),
                    UploadedAtUtc: DateTimeOffset.UtcNow,
                    EvidenceRefs:
                    [
                        "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_ONLY_PROMOTION_UPLOADER_CALLED",
                        "EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED",
                        "DESIGN_CONVERSION_TRAINING_INTAKE_EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED"
                    ])
                {
                    PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                    UploadAttempted = false,
                    UploadSucceeded = false,
                    VerificationSucceeded = false,
                    PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded
                }));
    }

    private static DesignConversionTrainingIntakeRunnerEvidence BuildTrainingIntakeRunnerEvidence(
        DesignConversionTrainingRunResult runnerResult)
    {
        var pipeline = runnerResult.PipelineResult;
        var lifecycle = pipeline.LifecycleEvidence;
        var countGate = runnerResult.DatasetCountGate;
        var sourceManifestStatus = runnerResult.DatasetManifest.Preflight.SourceManifestPreflightPassed
            ? "proven"
            : "missing";
        var sourceDisjointStatus = !runnerResult.DatasetManifest.Preflight.SourceDisjointDeclared
            ? "missing"
            : countGate.SourceDisjoint
                ? "proven"
                : "blocked_same_source";
        var trainingStarted = pipeline.MachineLifecycleHookAttempted
                              && lifecycle.LocalLifecycleBatchResultCount > 0
                              && countGate.Passed;
        var trainingExecutionStatus = BuildRunnerTrainingExecutionStatus(
            runnerResult,
            trainingStarted);
        var runnerBlockedReason = BuildRunnerBlockedReason(
            runnerResult,
            trainingStarted);
        var checkpointMutationEvidenceProven = HasCheckpointMutationEvidence(lifecycle.LocalContinueTrainingProbe);
        var checkpointMutationStatus = checkpointMutationEvidenceProven
            ? lifecycle.CheckpointContainsRealTrainedWeights
                ? "local_continue_checkpoint_mutated_with_real_learned_weights"
                : "local_continue_checkpoint_mutated_metadata_only"
            : lifecycle.CheckpointCandidateCreated
                ? "checkpoint_candidate_created_mutation_not_proven"
                : "not_proven";
        var weightProofStatus = BuildWeightProofStatus(lifecycle.CheckpointContainsRealTrainedWeights);

        return new DesignConversionTrainingIntakeRunnerEvidence(
            BoundaryDecisionKind: runnerResult.BoundaryDecision.Kind.ToString(),
            DatasetCountGatePassed: countGate.Passed,
            RequiredTrainRecords: countGate.RequiredTrainRecords,
            RequiredHeldOutRecords: countGate.RequiredHeldOutRecords,
            ActualTrainRecords: countGate.ActualTrainRecords,
            ActualHeldOutRecords: countGate.ActualHeldOutRecords,
            SourceDisjoint: countGate.SourceDisjoint,
            SourceManifestStatus: sourceManifestStatus,
            SourceDisjointStatus: sourceDisjointStatus,
            MachineLifecycleHookAttempted: pipeline.MachineLifecycleHookAttempted,
            TrainingStarted: trainingStarted,
            TrainingExecutionStatus: trainingExecutionStatus,
            RunnerBlockedReason: runnerBlockedReason,
            LocalLifecycleBatchResultCount: lifecycle.LocalLifecycleBatchResultCount,
            CheckpointCandidateStatus: lifecycle.CheckpointCandidateStatus,
            CheckpointCandidateCreated: lifecycle.CheckpointCandidateCreated,
            CheckpointMutationStatus: checkpointMutationStatus,
            CheckpointMutationEvidenceProven: checkpointMutationEvidenceProven,
            CheckpointContainsRealTrainedWeights: lifecycle.CheckpointContainsRealTrainedWeights,
            WeightProofStatus: weightProofStatus,
            FreshLoadStatus: lifecycle.FreshLoadStatus,
            ContinueTrainingStatus: lifecycle.ContinueTrainingStatus,
            MeasurementGateStatus: lifecycle.MeasurementGateStatus,
            PromotionStatus: lifecycle.PromotionStatus,
            PromotionUploadAllowed: lifecycle.PromotionUploadAllowed,
            EvidenceRefs: runnerResult.BoundaryDecision.EvidenceRefs
                .Concat(lifecycle.EvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            FailureReasons: lifecycle.FailureReasons
                .Concat(countGate.Passed ? [] : countGate.Reasons)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Warnings: runnerResult.Warnings
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    private static bool HasCheckpointMutationEvidence(
        DesignConversionLocalContinueTrainingProbeReport? continueProbe)
        => continueProbe is
        {
            FreshLoadMetadataContinuityVerified: true,
            DatasetCountGatePassed: true,
            TrainSplitOnly: true,
            SourceCheckpointUnchanged: true,
            ContinuationBatchAppended: true,
            NoReplayBatchIdVerified: true
        }
        && continueProbe.ContinuedTrainingHistoryCount > continueProbe.StartingTrainingHistoryCount;

    private static string BuildTrainingExecutionStatus(
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionTrainingRunResult? runnerResult,
        DesignConversionTrainingIntakeRunnerEvidence? runnerEvidence,
        bool trainingStarted)
    {
        if (runnerResult is null)
            return preflight.MayStartTraining
                ? "blocked_before_runner_by_training_intake_resolution"
                : "blocked_before_runner_by_training_intake_preflight";

        if (!runnerResult.DatasetCountGate.Passed)
            return "runner_invoked_training_blocked_dataset_count_gate";

        if (trainingStarted)
        {
            if (runnerEvidence?.CheckpointContainsRealTrainedWeights == true)
                return "local_lifecycle_training_started_with_real_learned_weights";

            return runnerResult.BoundaryDecision.RealTrainingRunClaimed
                ? "real_training_started_by_runner_boundary"
                : "local_lifecycle_training_started_scaffold_only";
        }

        if (runnerEvidence?.MachineLifecycleHookAttempted != true)
            return "runner_invoked_lifecycle_hook_not_attempted";

        return "runner_invoked_training_not_started";
    }

    private static string BuildRunnerTrainingExecutionStatus(
        DesignConversionTrainingRunResult runnerResult,
        bool trainingStarted)
    {
        if (!runnerResult.DatasetCountGate.Passed)
            return "runner_invoked_training_blocked_dataset_count_gate";

        if (trainingStarted)
        {
            if (runnerResult.PipelineResult.LifecycleEvidence.CheckpointContainsRealTrainedWeights)
                return "local_lifecycle_training_started_with_real_learned_weights";

            return runnerResult.BoundaryDecision.RealTrainingRunClaimed
                ? "real_training_started_by_runner_boundary"
                : "local_lifecycle_training_started_scaffold_only";
        }

        if (!runnerResult.PipelineResult.MachineLifecycleHookAttempted)
            return "runner_invoked_lifecycle_hook_not_attempted";

        return "runner_invoked_training_not_started";
    }

    private static string BuildRunnerBlockedReason(
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionTrainingRunResult? runnerResult,
        DesignConversionTrainingIntakeRunnerEvidence? runnerEvidence,
        IReadOnlyList<string> blockReasons,
        bool trainingStarted)
    {
        if (trainingStarted)
            return "none";

        if (runnerResult is not null)
            return runnerEvidence?.RunnerBlockedReason ?? BuildRunnerBlockedReason(runnerResult, trainingStarted);

        var firstBlock = (blockReasons ?? [])
            .Concat(preflight.BlockReasons ?? [])
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(firstBlock)
            ? "runner_not_invoked"
            : firstBlock;
    }

    private static string BuildRunnerBlockedReason(
        DesignConversionTrainingRunResult runnerResult,
        bool trainingStarted)
    {
        if (trainingStarted)
            return "none";

        if (!runnerResult.DatasetCountGate.Passed)
            return "dataset_count_gate_not_passed";

        if (!runnerResult.PipelineResult.MachineLifecycleHookAttempted)
            return "machine_lifecycle_hook_not_attempted";

        if (runnerResult.PipelineResult.LifecycleEvidence.LocalLifecycleBatchResultCount <= 0)
            return "local_lifecycle_batch_result_missing";

        if (!runnerResult.BoundaryDecision.RealTrainingRunClaimed)
            return "real_training_not_claimed";

        return "training_started_not_proven";
    }

    private static string BuildCheckpointStatus(
        DesignConversionTrainingIntakeRunnerEvidence? runnerEvidence)
    {
        if (runnerEvidence is null)
            return "not_proven";

        return runnerEvidence.CheckpointCandidateCreated
            ? runnerEvidence.CheckpointCandidateStatus
            : "not_proven";
    }

    private static string BuildCheckpointMutationStatus(
        DesignConversionTrainingIntakeRunnerEvidence? runnerEvidence)
        => runnerEvidence?.CheckpointMutationStatus ?? "not_proven";

    private static string BuildWeightProofStatus(bool weightsProduced)
        => weightsProduced
            ? "real_trained_weights_proven"
            : "real_trained_weights_not_proven";

    private static DesignConversionTrainingIntakeUploadReadiness BuildTrainingIntakeUploadReadiness(
        DesignConversionTrainingRunResult? runnerResult,
        DesignConversionTrainingIntakeRunnerEvidence? runnerEvidence,
        bool weightsProduced)
    {
        var lifecycle = runnerResult?.PipelineResult.LifecycleEvidence;
        var measurementGateStatus = lifecycle?.MeasurementGateStatus
            ?? runnerEvidence?.MeasurementGateStatus
            ?? DesignConversionMeasurementGateStatus.NotRun;
        var promotionGateStatus = lifecycle?.PromotionStatus
            ?? runnerEvidence?.PromotionStatus
            ?? "PROMOTION_BLOCKED_MEASUREMENT_GATE_NOT_PASSED";
        var measurementGatePassed = lifecycle?.MeasurementGate?.Passed == true
                                    && string.Equals(
                                        lifecycle.MeasurementGate.Status,
                                        DesignConversionMeasurementGateStatus.Passed,
                                        StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(
                                        measurementGateStatus,
                                        DesignConversionMeasurementGateStatus.Passed,
                                        StringComparison.OrdinalIgnoreCase)
                                    && ContainsMeasurementGatePassEvidence(lifecycle.MeasurementGate.EvidenceRefs)
                                    && ContainsMeasurementGatePassEvidence(lifecycle.EvidenceRefs);
        var promotionPolicyPassed = runnerEvidence?.PromotionUploadAllowed == true
                                    && lifecycle?.PromotionUploadAllowed == true
                                    && string.Equals(
                                        promotionGateStatus,
                                        "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
                                        StringComparison.Ordinal);
        var modelUploadReady = weightsProduced && measurementGatePassed && promotionPolicyPassed;
        var blockedReasons = new SortedSet<string>(StringComparer.Ordinal);
        if (!weightsProduced)
            blockedReasons.Add("real_trained_weights_not_proven");
        if (!measurementGatePassed)
            blockedReasons.Add($"measurement_gate_not_passed:{measurementGateStatus}");
        if (!promotionPolicyPassed)
            blockedReasons.Add($"promotion_policy_not_passed:{promotionGateStatus}");

        return new DesignConversionTrainingIntakeUploadReadiness(
            ModelUploadReady: modelUploadReady,
            MeasurementGateStatus: measurementGateStatus,
            PromotionGateStatus: promotionGateStatus,
            UploadReadinessStatus: BuildUploadReadinessStatus(
                modelUploadReady,
                weightsProduced,
                measurementGatePassed,
                promotionPolicyPassed),
            PromotionBlockedReasons: blockedReasons.ToArray());
    }

    private static string BuildUploadReadinessStatus(
        bool modelUploadReady,
        bool weightsProduced,
        bool measurementGatePassed,
        bool promotionPolicyPassed)
    {
        if (modelUploadReady)
            return "upload_ready_local_promotion_gate_passed_no_external_upload";
        if (!weightsProduced)
            return "upload_blocked_real_trained_weights_not_proven";
        if (!measurementGatePassed)
            return "upload_blocked_measurement_gate_not_passed";
        if (!promotionPolicyPassed)
            return "upload_blocked_promotion_policy_not_passed";

        return "upload_blocked_unknown";
    }

    private static bool ContainsMeasurementGatePassEvidence(IReadOnlyList<string>? evidenceRefs)
        => evidenceRefs?.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED", StringComparer.Ordinal) == true;

    private static IReadOnlyList<string> BuildTrainingIntakeRunBlockReasons(
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionTrainingRunResult? runnerResult,
        IReadOnlyList<string> blockReasons,
        bool trainingStarted,
        bool checkpointMutated,
        bool weightsProduced,
        IReadOnlyList<string> promotionBlockedReasons,
        bool modelUploadReady)
    {
        var reasons = new SortedSet<string>(blockReasons ?? [], StringComparer.Ordinal);
        foreach (var reason in preflight.BlockReasons ?? [])
            reasons.Add(reason);
        if (runnerResult is not null)
        {
            if (!runnerResult.DatasetCountGate.Passed)
            {
                foreach (var reason in runnerResult.DatasetCountGate.Reasons ?? [])
                    reasons.Add(reason);
            }

            foreach (var reason in runnerResult.PipelineResult.LifecycleEvidence.FailureReasons ?? [])
                reasons.Add(reason);
        }

        if (!trainingStarted)
            reasons.Add("training_not_started");
        if (runnerResult?.BoundaryDecision.RealTrainingRunClaimed != true)
            reasons.Add("real_training_not_claimed");
        if (!checkpointMutated)
            reasons.Add("checkpoint_mutation_not_proven");
        if (!weightsProduced)
            reasons.Add("real_trained_weights_not_proven");
        foreach (var reason in promotionBlockedReasons ?? [])
            reasons.Add(reason);
        if (!modelUploadReady)
            reasons.Add("model_upload_not_ready");

        return reasons.ToArray();
    }

    private static IReadOnlyList<string> BuildTrainingIntakeRunWarnings(
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionTrainingRunResult? runnerResult,
        DesignConversionTrainingIntakeRunnerEvidence? runnerEvidence,
        bool trainingStarted,
        bool checkpointMutated,
        bool weightsProduced,
        bool modelUploadReady,
        string trainingExecutionStatus,
        string runnerBlockedReason,
        string checkpointStatus,
        string checkpointMutationStatus,
        string weightProofStatus,
        DesignConversionTrainingIntakeUploadReadiness uploadReadiness)
    {
        var warnings = new SortedSet<string>(preflight.Warnings ?? [], StringComparer.Ordinal)
        {
            "training_intake_to_training_runner_bridge",
            "raw_html_css_tree_private_provider_payloads_not_emitted",
            "product_not_ready",
            "model_readiness_claimed=false",
            "model_server_upload_not_attempted"
        };

        if (runnerResult is not null)
        {
            warnings.Remove("training_intake_preflight_only");
            warnings.Remove("future_training_run_permission_only");
            warnings.Remove("may_start_training_future_run_only");
            warnings.Remove("preflight_does_not_invoke_training_runner");
        }

        if (trainingStarted)
        {
            warnings.Remove("training_not_started");
            warnings.Remove("not_trained");
        }

        if (checkpointMutated)
            warnings.Remove("checkpoint_not_mutated");

        warnings.Add(runnerResult is null
            ? "training_runner_not_invoked"
            : "training_runner_invoked");
        warnings.Add(runnerEvidence?.MachineLifecycleHookAttempted == true
            ? "local_lifecycle_hook_attempted_scaffold_only"
            : "local_lifecycle_hook_not_attempted");
        warnings.Add($"training_execution_status:{trainingExecutionStatus}");
        warnings.Add($"runner_blocked_reason:{runnerBlockedReason}");
        warnings.Add($"checkpoint_status:{checkpointStatus}");
        warnings.Add($"checkpoint_mutation_status:{checkpointMutationStatus}");
        warnings.Add($"weight_proof_status:{weightProofStatus}");
        warnings.Add($"measurement_gate_status:{uploadReadiness.MeasurementGateStatus}");
        warnings.Add($"promotion_gate_status:{uploadReadiness.PromotionGateStatus}");
        warnings.Add($"upload_readiness_status:{uploadReadiness.UploadReadinessStatus}");
        warnings.Add($"source_manifest_status:{preflight.SourceManifestStatus}");
        warnings.Add($"source_disjoint_status:{preflight.SourceDisjointStatus}");
        foreach (var reason in uploadReadiness.PromotionBlockedReasons ?? [])
            warnings.Add($"promotion_blocked_reason:{reason}");
        if (!trainingStarted)
            warnings.Add("not_trained");
        else if (runnerResult?.BoundaryDecision.RealTrainingRunClaimed != true)
            warnings.Add(weightsProduced
                ? "training_started_local_lifecycle_real_weight_artifact_only"
                : "training_started_local_lifecycle_scaffold_only");
        if (!checkpointMutated)
            warnings.Add("checkpoint_mutation_not_proven");
        if (!weightsProduced)
            warnings.Add("weights_not_produced");
        if (!modelUploadReady)
            warnings.Add("model_upload_not_ready");
        if (trainingStarted)
        {
            warnings.Remove("not_trained");
            warnings.Remove("training_not_started");
        }

        if (checkpointMutated)
            warnings.Remove("checkpoint_not_mutated");
        if (weightsProduced)
            warnings.Remove("weights_not_produced");
        if (modelUploadReady)
            warnings.Remove("model_upload_not_ready");

        foreach (var warning in runnerResult?.Warnings ?? [])
            warnings.Add(warning);

        return warnings.ToArray();
    }

    private static IReadOnlyDictionary<string, string> BuildTrainingIntakeRunBoundaryMetadata(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingIntakePreflightResult preflight,
        DesignConversionTrainingPipelineOptions options,
        bool optionsProvided,
        int resolvedTrainingRecordCount,
        DesignConversionTrainingIntakeSourceProof sourceProof,
        DesignConversionTrainingIntakeRunnerEvidence? runnerEvidence,
        string trainingExecutionBoundary,
        string trainingExecutionStatus,
        string runnerBlockedReason,
        string checkpointStatus,
        string checkpointMutationStatus,
        string weightProofStatus,
        DesignConversionTrainingIntakeUploadReadiness uploadReadiness,
        bool trainingStarted,
        bool checkpointMutated,
        bool weightsProduced,
        bool modelUploadReady,
        bool mayStartTraining,
        IReadOnlyList<string> blockReasons)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["schema_version"] = TrainingIntakeRunSchemaVersion,
            ["training_intake_id"] = intake.IntakeId,
            ["preflight_id"] = preflight.PreflightId,
            ["preflight_may_start_training"] = preflight.MayStartTraining.ToString(),
            ["accepted_candidate_count"] = intake.AcceptedCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["resolved_training_record_count"] = resolvedTrainingRecordCount.ToString(CultureInfo.InvariantCulture),
            ["training_options_source"] = optionsProvided ? "caller_supplied" : "default_source_manifest_disjoint_guarded",
            ["run_id"] = string.IsNullOrWhiteSpace(options.RunId) ? BuildTrainingIntakeRunId(intake) : options.RunId,
            ["dataset_id"] = options.DatasetOptions?.DatasetId ?? string.Empty,
            ["may_start_training"] = mayStartTraining.ToString(),
            ["source_manifest_status"] = sourceProof.SourceManifestStatus,
            ["source_disjoint_status"] = sourceProof.SourceDisjointStatus,
            ["source_manifest_proven"] = sourceProof.SourceManifestProven.ToString(),
            ["source_disjoint_proven"] = sourceProof.SourceDisjointProven.ToString(),
            ["source_manifest_candidate_count"] = sourceProof.CandidateCount.ToString(CultureInfo.InvariantCulture),
            ["source_manifest_hash_count"] = sourceProof.SourceManifestHashCount.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_train_source_count"] = sourceProof.TrainSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_heldout_source_count"] = sourceProof.HeldOutSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["source_disjoint_overlap_count"] = sourceProof.OverlapSourceIdentityHashes.Count.ToString(CultureInfo.InvariantCulture),
            ["training_runner_invoked"] = (runnerEvidence is not null).ToString(),
            ["local_lifecycle_hook_attempted"] = (runnerEvidence?.MachineLifecycleHookAttempted == true).ToString(),
            ["training_started"] = trainingStarted.ToString(),
            ["not_trained"] = (!trainingStarted).ToString(),
            ["checkpoint_mutated"] = checkpointMutated.ToString(),
            ["checkpoint_not_mutated"] = (!checkpointMutated).ToString(),
            ["checkpoint_candidate_created"] = (runnerEvidence?.CheckpointCandidateCreated == true).ToString(),
            ["checkpoint_contains_real_trained_weights"] = (runnerEvidence?.CheckpointContainsRealTrainedWeights == true).ToString(),
            ["weights_produced"] = weightsProduced.ToString(),
            ["weights_not_produced"] = (!weightsProduced).ToString(),
            ["model_upload_ready"] = modelUploadReady.ToString(),
            ["model_upload_not_ready"] = (!modelUploadReady).ToString(),
            ["model_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["promotion_upload_attempted"] = bool.FalseString,
            ["promotion_claimed"] = bool.FalseString,
            ["training_execution_boundary"] = trainingExecutionBoundary,
            ["training_execution_status"] = trainingExecutionStatus,
            ["runner_blocked_reason"] = runnerBlockedReason,
            ["checkpoint_status"] = checkpointStatus,
            ["checkpoint_mutation_status"] = checkpointMutationStatus,
            ["weight_proof_status"] = weightProofStatus,
            ["measurement_gate_status"] = uploadReadiness.MeasurementGateStatus,
            ["promotion_gate_status"] = uploadReadiness.PromotionGateStatus,
            ["upload_readiness_status"] = uploadReadiness.UploadReadinessStatus,
            ["promotion_upload_allowed"] = (runnerEvidence?.PromotionUploadAllowed == true).ToString(),
            ["promotion_blocked_reasons"] = string.Join("|", uploadReadiness.PromotionBlockedReasons ?? []),
            ["runner_boundary_decision_kind"] = runnerEvidence?.BoundaryDecisionKind ?? "not_invoked",
            ["dataset_count_gate_passed"] = (runnerEvidence?.DatasetCountGatePassed == true).ToString(),
            ["block_reason_count"] = blockReasons.Count.ToString(CultureInfo.InvariantCulture)
        };
        AddTrainingDatabaseDurabilityMetadata(
            metadata,
            TrainingIntakeRunSchemaVersion,
            "training-intake-run",
            resolvedTrainingRecordCount,
            (intake.CandidateRefs ?? [])
                .Select(candidate => candidate.SourceManifestRef)
                .Concat((runnerEvidence?.EvidenceRefs ?? []).Select(reference => reference)),
            [
                intake.IntakeId,
                preflight.PreflightId,
                string.IsNullOrWhiteSpace(options.RunId) ? BuildTrainingIntakeRunId(intake) : options.RunId,
                resolvedTrainingRecordCount.ToString(CultureInfo.InvariantCulture),
                trainingExecutionBoundary,
                trainingExecutionStatus,
                checkpointStatus,
                checkpointMutationStatus,
                weightProofStatus
            ]);

        return metadata;
    }

    private static string BuildTrainingExecutionBoundary(DesignConversionTrainingRunResult runnerResult)
    {
        if (!runnerResult.DatasetCountGate.Passed)
            return "runner_invoked_count_or_source_manifest_gate_no_training_started";
        if (runnerResult.PipelineResult.MachineLifecycleHookAttempted
            && !runnerResult.BoundaryDecision.RealTrainingRunClaimed)
        {
            if (runnerResult.PipelineResult.LifecycleEvidence.CheckpointContainsRealTrainedWeights)
                return "runner_invoked_local_lifecycle_real_weight_artifact_no_external_training_claim";

            return "runner_invoked_local_lifecycle_scaffold_only_no_real_training_claim";
        }
        if (!runnerResult.BoundaryDecision.RealTrainingRunClaimed)
            return "runner_invoked_no_real_training_claim";

        return "runner_invoked_real_training_claimed_by_boundary_decision";
    }

    private static string BuildTrainingIntakeRunId(DesignConversionTrainingIntakeRecord intake)
        => $"training-intake-run-{HashText(string.Join("|",
        [
            TrainingIntakeRunSchemaVersion,
            intake.IntakeId,
            intake.AcceptedCandidateCount.ToString(CultureInfo.InvariantCulture),
            string.Join(",", (intake.CandidateRefs ?? [])
                .Select(candidate => candidate.HandoffRecordHash)
                .Order(StringComparer.Ordinal))
        ]))}";

    private sealed record DesignConversionTrainingIntakeRecordResolution(
        IReadOnlyList<DesignConversionDpoTrainingRecord> Records,
        IReadOnlyList<string> BlockReasons);

    private sealed record DesignConversionTrainingIntakeReplayResolution(
        bool HasReplay,
        DesignConversionTrainingIntakeRecord? Record);

    private sealed record DesignConversionTrainingIntakeRunReplayResolution(
        bool HasReplay,
        DesignConversionTrainingIntakeRunResult? Result);

    private sealed record DesignConversionTrainingIntakeLocalPromotionReplayResolution(
        bool HasReplay,
        DesignConversionTrainingIntakeLocalPromotionResult? Result);

    private sealed record DesignConversionTrainingIntakeLocalPromotionRollbackReplayResolution(
        bool HasReplay,
        DesignConversionTrainingIntakeLocalPromotionRollbackResult? Result);

    private sealed record DesignConversionLocalPromotionActiveState(
        bool HasActive,
        string ActiveLocalModelRef,
        string ActiveModelId,
        string ActiveCheckpointSha256)
    {
        public static DesignConversionLocalPromotionActiveState Empty { get; } = new(
            HasActive: false,
            ActiveLocalModelRef: string.Empty,
            ActiveModelId: string.Empty,
            ActiveCheckpointSha256: string.Empty);
    }

    private sealed record DesignConversionTrainingIntakeSourceProof(
        string SourceManifestStatus,
        string SourceDisjointStatus,
        bool SourceManifestProven,
        bool SourceDisjointProven,
        int CandidateCount,
        int SourceManifestHashCount,
        IReadOnlyList<string> HeldOutSourceIdentityHashes,
        IReadOnlyList<string> TrainSourceIdentityHashes,
        IReadOnlyList<string> OverlapSourceIdentityHashes,
        IReadOnlyList<string> BlockReasons,
        IReadOnlyList<string> EvidenceRefs);

    private sealed record DesignConversionTrainingIntakeUploadReadiness(
        bool ModelUploadReady,
        string MeasurementGateStatus,
        string PromotionGateStatus,
        string UploadReadinessStatus,
        IReadOnlyList<string> PromotionBlockedReasons);

    private static bool ContainsForbiddenTrainingIntakePayload(
        DesignConversionApprovedDpoHandoffArtifact handoff)
        => EnumerateHandoffScalarValues(handoff).Any(ContainsForbiddenTrainingIntakePayload);

    private static bool ContainsForbiddenTrainingIntakePayload(
        DesignConversionTrainingIntakeRecord intake)
        => EnumerateTrainingIntakeScalarValues(intake).Any(ContainsForbiddenTrainingIntakePayload);

    private static bool ContainsForbiddenTrainingIntakePayload(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && (LooksLikeLocalPath(value)
               || ForbiddenTrainingIntakePayloadMarkers.Any(marker =>
                   value.Contains(marker, StringComparison.OrdinalIgnoreCase)));

    private static IEnumerable<string> EnumerateHandoffScalarValues(
        DesignConversionApprovedDpoHandoffArtifact handoff)
    {
        yield return handoff.ArtifactId;
        yield return handoff.ArtifactRef;
        yield return handoff.SchemaVersion;
        foreach (var warning in handoff.Warnings ?? [])
            yield return warning;
        foreach (var pair in handoff.BoundaryMetadata ?? new Dictionary<string, string>())
        {
            yield return pair.Key;
            yield return pair.Value;
        }

        foreach (var row in handoff.Rows ?? [])
        {
            yield return row.LocalArtifactRecordId;
            yield return row.CandidateId;
            yield return row.TrainingRecordId;
            yield return row.UnitId;
            yield return row.ApprovalStatus;
            yield return row.ReviewStatus;
            yield return row.ApprovedByRef;
            yield return row.ApprovalReason;
            yield return row.ApprovedAtUtc;
            yield return row.SourceManifestRef;
            yield return row.RecordHash;
            yield return row.InternalUnitStateHash;
            yield return row.SourceStructureHash;
            yield return row.ChosenPayloadHash;
            yield return row.RejectedPayloadHash;
            yield return row.TieBreakerHash;
            foreach (var reference in row.EvidenceRefs ?? [])
                yield return reference;
            foreach (var pair in row.BoundaryMetadata ?? new Dictionary<string, string>())
            {
                yield return pair.Key;
                yield return pair.Value;
            }
        }
    }

    private static IEnumerable<string> EnumerateTrainingIntakeScalarValues(
        DesignConversionTrainingIntakeRecord intake)
    {
        yield return intake.IntakeId;
        yield return intake.IntakeRef;
        yield return intake.SourceHandoffArtifactId;
        yield return intake.SourceHandoffArtifactRef;
        yield return intake.RequestedByRef;
        yield return intake.RequestReason;
        foreach (var warning in intake.Warnings ?? [])
            yield return warning;
        foreach (var pair in intake.BoundaryMetadata ?? new Dictionary<string, string>())
        {
            yield return pair.Key;
            yield return pair.Value;
        }

        foreach (var candidate in intake.CandidateRefs ?? [])
        {
            yield return candidate.LocalArtifactRecordId;
            yield return candidate.CandidateId;
            yield return candidate.TrainingRecordId;
            yield return candidate.UnitId;
            yield return candidate.HandoffRecordHash;
            yield return candidate.SourceManifestRef;
            yield return candidate.SourceManifestHash;
            yield return candidate.SourceIdentityHash;
            yield return candidate.InternalUnitStateHash;
            yield return candidate.SourceStructureHash;
            yield return candidate.ChosenPayloadHash;
            yield return candidate.RejectedPayloadHash;
            yield return candidate.TieBreakerHash;
            foreach (var pair in candidate.BoundaryMetadata ?? new Dictionary<string, string>())
            {
                yield return pair.Key;
                yield return pair.Value;
            }
        }
    }

    private static readonly IReadOnlyList<string> RequiredTrainingIntakeTrueBoundaryKeys =
    [
        "training_handoff_only",
        "not_trained",
        "weights_not_produced",
        "model_upload_not_ready",
        "product_not_ready",
        "held_out_not_proven",
        "source_disjoint_not_proven"
    ];

    private static readonly IReadOnlyList<string> RequiredTrainingIntakeFalseBoundaryKeys =
    [
        "model_readiness_claimed",
        "trained_model_ready",
        "raw_html_included",
        "raw_css_included",
        "raw_design_tree_included",
        "raw_user_private_data_included",
        "provider_payload_included"
    ];

    private static readonly IReadOnlyList<string> RequiredTrainingIntakePreflightTrueBoundaryKeys =
    [
        "training_intake_only",
        "training_not_started",
        "not_trained",
        "weights_not_produced",
        "checkpoint_not_mutated",
        "model_upload_not_ready",
        "product_not_ready",
        "held_out_not_proven",
        "source_disjoint_not_proven"
    ];

    private static readonly IReadOnlyList<string> RequiredTrainingIntakePreflightFalseBoundaryKeys =
    [
        "training_started",
        "model_ready",
        "product_ready",
        "trained_model_ready",
        "model_readiness_claimed",
        "raw_html_included",
        "raw_css_included",
        "raw_design_tree_included",
        "raw_user_private_data_included",
        "provider_payload_included"
    ];

    private static readonly IReadOnlyList<string> ForbiddenTrainingIntakePayloadMarkers =
    [
        "<main",
        ".private",
        "raw-tree-private",
        "C:/Users/",
        "exact_prompt",
        "private.fig",
        "external-provider-payload",
        "raw_provider_payload"
    ];

    private static string HandoffHashRef(string scope, string value)
        => DesignConversionDpoLeakageGuard.HashRef($"dpo-handoff-{scope}", value);

    private static bool IsReviewApprovedForTraining(DesignConversionLocalArtifactDbDpoCandidate candidate)
    {
        if (!IsRuntimeCreatedCandidate(candidate))
            return true;

        var metadata = candidate.TrainingRecord.Provenance.Metadata;
        return string.Equals(ReadMetadata(metadata, "approval_status"), "approved", StringComparison.OrdinalIgnoreCase)
               && IsTruthy(ReadMetadata(metadata, "approved_for_training"));
    }

    private static bool IsApprovedReviewMetadata(IReadOnlyDictionary<string, string> metadata)
        => string.Equals(ReadMetadata(metadata, "approval_status"), "approved", StringComparison.OrdinalIgnoreCase)
           && IsTruthy(ReadMetadata(metadata, "approved_for_training"));

    private static bool IsRuntimeCreatedCandidate(DesignConversionLocalArtifactDbDpoCandidate candidate)
        => IsRuntimeCreatedCandidate(candidate.TrainingRecord);

    private static bool IsRuntimeCreatedCandidate(DesignConversionDpoTrainingRecord record)
        => IsTruthy(ReadMetadata(record.Provenance.Metadata, "runtime_created_dpo_candidate"))
           || ReadMetadata(record.Provenance.Metadata, "split")
               .Contains("runtime_training_candidate_review", StringComparison.OrdinalIgnoreCase);

    private static bool IsRuntimeCreatedReviewRow(IReadOnlyDictionary<string, string> metadata)
        => IsTruthy(ReadMetadata(metadata, "runtime_created_dpo_candidate"))
           || ReadMetadata(metadata, "split")
               .Contains("runtime_training_candidate_review", StringComparison.OrdinalIgnoreCase);

    private static string ReadMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value
            : string.Empty;

    private static bool IsTruthy(string? value)
        => string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "1", StringComparison.Ordinal)
           || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);

    private static bool IsRecordTrainable(DesignConversionDpoTrainingRecord record)
        => record is not null
           && !record.Quarantine.IsQuarantined
           && record.License.AllowsTraining
           && !record.License.ContainsUserPrivateData
           && !string.IsNullOrWhiteSpace(record.UnitId)
           && !string.IsNullOrWhiteSpace(record.Provenance.SourceManifestRef)
           && !HasProviderMissingStatus(record);

    private static bool IsVisualJudgeTrainingAllowed(DesignConversionVisualJudgeArtifactDbStatus status)
        => !IsProviderMissingStatus(status);

    private static bool IsProviderMissingStatus(DesignConversionVisualJudgeArtifactDbStatus status)
    {
        var values = new[]
            {
                status.Status,
                status.ProviderBoundary,
                status.TierDecision
            }
            .Concat((status.EvidenceRefs ?? []).Select(reference => reference ?? string.Empty))
            .Concat((status.Metadata ?? new Dictionary<string, string>()).SelectMany(pair => new[] { pair.Key, pair.Value }));

        return values.Any(IsProviderMissingText);
    }

    private static bool HasProviderMissingStatus(DesignConversionDpoTrainingRecord record)
    {
        var values = new[]
            {
                record.Quarantine.Reason
            }
            .Concat((record.Quarantine.EvidenceRefs ?? []).Select(reference => reference ?? string.Empty))
            .Concat((record.Provenance.Metadata ?? new Dictionary<string, string>()).SelectMany(pair => new[] { pair.Key, pair.Value }));

        return values.Any(IsProviderMissingText);
    }

    private static bool IsProviderMissingText(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && (value.Contains(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, StringComparison.OrdinalIgnoreCase)
               || value.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, StringComparison.OrdinalIgnoreCase)
               || (value.Contains("provider", StringComparison.OrdinalIgnoreCase)
                   && (value.Contains("missing", StringComparison.OrdinalIgnoreCase)
                       || value.Contains("required", StringComparison.OrdinalIgnoreCase)
                       || value.Contains("blocked", StringComparison.OrdinalIgnoreCase))));

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

    private static string PayloadRef(string scope, string? value, SortedSet<string> redactions)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (value.StartsWith("artifact://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
        {
            return SafeArtifactRef(value, scope, redactions);
        }

        redactions.Add($"{scope}:payload_hash_ref");
        return DesignConversionDpoLeakageGuard.HashRef($"dcm-{scope}", value);
    }

    private static string SafeArtifactRef(string? value, string scope, SortedSet<string> redactions)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (LooksLikeLocalPath(value) || LooksLikeRawPayload(value))
        {
            redactions.Add($"{scope}:artifact_ref_hash");
            return DesignConversionDpoLeakageGuard.HashRef("dcm-artifact-ref", value);
        }

        return DesignConversionDpoLeakageGuard.SanitizeScalar(value, scope, redactions);
    }

    private static string SafeScalar(
        string? value,
        string scope,
        SortedSet<string> redactions,
        string fallback)
    {
        var sanitized = DesignConversionDpoLeakageGuard.SanitizeScalar(value, scope, redactions);
        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }

    private static string NormalizeSha256(string? candidate, string? fallbackPayload)
    {
        if (!string.IsNullOrWhiteSpace(candidate)
            && candidate.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
        {
            return candidate;
        }

        return $"sha256:{HashText(string.IsNullOrWhiteSpace(candidate) ? fallbackPayload ?? string.Empty : candidate)}";
    }

    private static DesignConversionLocalArtifactDbIndexRecordLocator BuildIndexRecordLocator(
        DesignConversionLocalArtifactDbRecord record,
        long byteOffset,
        long byteLength,
        int lineNumber,
        string authorityLineSha256)
        => new(
            RecordId: record.RecordId,
            JobId: record.JobId,
            RequestId: record.RequestId,
            SourceKind: record.SourceKind,
            CreatedAtUtc: record.CreatedAtUtc,
            ByteOffset: byteOffset,
            ByteLength: byteLength,
            LineNumber: lineNumber,
            AuthorityLineSha256: authorityLineSha256,
            ImmutableAppendFingerprint: GetImmutableAppendFingerprint(record) ?? string.Empty);

    private static string DecodeAuthorityJsonLine(byte[] lineBytes, bool firstLine)
    {
        var offset = 0;
        var length = lineBytes.Length;
        if (length > 0 && lineBytes[length - 1] == (byte)'\n')
            length--;
        if (length > 0 && lineBytes[length - 1] == (byte)'\r')
            length--;
        if (firstLine
            && length >= 3
            && lineBytes[0] == 0xEF
            && lineBytes[1] == 0xBB
            && lineBytes[2] == 0xBF)
        {
            offset = 3;
            length -= 3;
        }

        return StrictUtf8Encoding.GetString(lineBytes, offset, length);
    }

    private static byte[] BuildAppendedAuthorityLineBytes(string line, long appendOffset)
    {
        var lineBytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
        if (appendOffset != 0)
            return lineBytes;

        var preamble = Encoding.UTF8.GetPreamble();
        if (preamble.Length == 0)
            return lineBytes;

        var bytes = new byte[preamble.Length + lineBytes.Length];
        Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
        Buffer.BlockCopy(lineBytes, 0, bytes, preamble.Length, lineBytes.Length);
        return bytes;
    }

    private static bool TryProjectCommittedAuthorityLength(
        long committedLength,
        long appendedLineByteLength,
        out long projectedLength)
    {
        projectedLength = 0;
        if (committedLength < 0 || appendedLineByteLength < 0)
            return false;
        if (committedLength > long.MaxValue - appendedLineByteLength)
            return false;

        projectedLength = committedLength + appendedLineByteLength;
        return true;
    }

    private static string BuildRootDirectoryBindingHash(string rootDirectory)
    {
        var canonical = Path.GetFullPath(rootDirectory);
        if (OperatingSystem.IsWindows())
            canonical = canonical.ToUpperInvariant();

        return $"sha256:{HashText(canonical)}";
    }

    private static bool IsSha256Ref(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length == "sha256:".Length + 64
           && value.StartsWith("sha256:", StringComparison.Ordinal)
           && value.Skip("sha256:".Length).All(character =>
               character is (>= '0' and <= '9') or (>= 'a' and <= 'f'));

    private static string BuildImmutableAppendFingerprint(DesignConversionLocalArtifactDbRecord record)
    {
        var metadata = new Dictionary<string, string>(
            record.Metadata ?? new Dictionary<string, string>(),
            StringComparer.Ordinal);
        metadata.Remove(ImmutableAppendFingerprintMetadataKey);
        var comparable = record with
        {
            CreatedAtUtc = DateTimeOffset.UnixEpoch,
            Metadata = metadata
        };
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(comparable, JsonOptions));
        var builder = new StringBuilder();
        AppendCanonicalJson(document.RootElement, builder);
        return $"sha256:{HashText(builder.ToString())}";
    }

    private static string? GetImmutableAppendFingerprint(DesignConversionLocalArtifactDbRecord record)
    {
        if (record.Metadata is null
            || !record.Metadata.TryGetValue(ImmutableAppendFingerprintMetadataKey, out var fingerprint)
            || fingerprint.Length != "sha256:".Length + 64
            || !fingerprint.StartsWith("sha256:", StringComparison.Ordinal)
            || fingerprint.Skip("sha256:".Length).Any(character =>
                character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
        {
            return null;
        }

        return fingerprint;
    }

    private static void AppendCanonicalJson(JsonElement element, StringBuilder builder)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                builder.Append('{');
                var firstProperty = true;
                foreach (var property in element.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    if (!firstProperty)
                        builder.Append(',');
                    firstProperty = false;
                    builder.Append(JsonSerializer.Serialize(property.Name, JsonOptions));
                    builder.Append(':');
                    AppendCanonicalJson(property.Value, builder);
                }

                builder.Append('}');
                break;

            case JsonValueKind.Array:
                builder.Append('[');
                var firstItem = true;
                foreach (var item in element.EnumerateArray())
                {
                    if (!firstItem)
                        builder.Append(',');
                    firstItem = false;
                    AppendCanonicalJson(item, builder);
                }

                builder.Append(']');
                break;

            case JsonValueKind.String:
                builder.Append(JsonSerializer.Serialize(element.GetString(), JsonOptions));
                break;

            case JsonValueKind.Number:
                builder.Append(element.GetRawText());
                break;

            case JsonValueKind.True:
                builder.Append("true");
                break;

            case JsonValueKind.False:
                builder.Append("false");
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                builder.Append("null");
                break;
        }
    }

    private static string HashText(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty))).ToLowerInvariant();

    private static string HashBytes(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private static void AddTrainingDatabaseDurabilityMetadata(
        IDictionary<string, string> metadata,
        string schemaVersion,
        string manifestScope,
        int recordCount,
        IEnumerable<string?> sourceRefs,
        IEnumerable<string?> checksumParts)
    {
        var normalizedRecordCount = Math.Max(0, recordCount);
        var sourceRefsHash = HashTrainingDatabaseRefs(sourceRefs);
        var checksum = $"sha256:{HashText(string.Join("|",
        [
            LocalArtifactDbKind,
            TrainingDatabaseName,
            SchemaVersion,
            schemaVersion,
            manifestScope,
            normalizedRecordCount.ToString(CultureInfo.InvariantCulture),
            sourceRefsHash,
            string.Join("|", (checksumParts ?? [])
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => HashText(part!.Trim()))
                .Order(StringComparer.Ordinal))
        ]))}";
        var manifestRef = $"manifest://design-conversion-local-artifact-db/{SafeTrainingDatabaseToken(manifestScope)}/{checksum[7..23]}";

        metadata["training_database_kind"] = LocalArtifactDbKind;
        metadata["training_database_name"] = TrainingDatabaseName;
        metadata["training_database_version"] = SchemaVersion;
        metadata["training_database_schema_version"] = schemaVersion;
        metadata["training_database_manifest_ref"] = manifestRef;
        metadata["training_database_checksum_sha256"] = checksum;
        metadata["training_database_manifest_checksum_sha256"] = checksum;
        metadata["training_database_record_count"] = normalizedRecordCount.ToString(CultureInfo.InvariantCulture);
        metadata["training_database_source_refs_sha256"] = sourceRefsHash;
        metadata["training_database_source_refs_policy"] = "source_refs_hashed_or_manifest_refs_only";
        metadata["training_database_retention_class"] = TrainingDatabaseRetentionClass;
        metadata["training_database_retention_policy"] = TrainingDatabaseRetentionPolicy;
        metadata["training_database_refs_only_trace"] = bool.TrueString;
        metadata["training_database_raw_payload_redaction_marker"] = TrainingDatabaseRawPayloadRedactionMarker;
        metadata["raw_payload_redacted"] = bool.TrueString;
    }

    private static string HashTrainingDatabaseRefs(IEnumerable<string?> sourceRefs)
        => $"sha256:{HashText(string.Join("|", (sourceRefs ?? [])
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Select(reference => HashText(reference!.Trim()))
            .Order(StringComparer.Ordinal)))}";

    private static string SafeTrainingDatabaseToken(string value)
    {
        var source = string.IsNullOrWhiteSpace(value) ? "trace" : value.Trim().ToLowerInvariant();
        var builder = new StringBuilder(source.Length);
        foreach (var ch in source)
            builder.Append(char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-');

        var token = builder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(token))
            return "trace";
        return token.Length <= 64 ? token : $"{token[..48]}-{HashText(token)[..12]}";
    }

    private static bool LooksLikeLocalPath(string value)
        => value.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
           || value.StartsWith(@"\\", StringComparison.Ordinal)
           || (value.Length >= 3 && char.IsLetter(value[0]) && value[1] == ':' && (value[2] == '\\' || value[2] == '/'))
           || value.StartsWith("/Users/", StringComparison.OrdinalIgnoreCase)
           || value.StartsWith("/home/", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeRawPayload(string value)
        => value.Contains('<', StringComparison.Ordinal)
           || value.Contains('{', StringComparison.Ordinal)
           || value.Contains('}', StringComparison.Ordinal)
           || value.Contains('\n', StringComparison.Ordinal)
           || value.Contains("exact_prompt", StringComparison.OrdinalIgnoreCase);

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail("DesignConversion local artifact DB operation cancelled.", "CANCELLED");

    private static OperationResult<T> LocalArtifactDbWriteFailed<T>()
        => OperationResult<T>.Fail(LocalArtifactDbWriteFailedSafeMessage, LocalArtifactDbWriteFailedErrorCode);

    private static OperationResult<T> TrainingIntakeReadFailed<T>()
        => OperationResult<T>.Fail(TrainingIntakeReadFailedSafeMessage, TrainingIntakeReadFailedErrorCode);

    private static OperationResult<T> TrainingIntakeRunReadFailed<T>()
        => OperationResult<T>.Fail(TrainingIntakeRunReadFailedSafeMessage, TrainingIntakeRunReadFailedErrorCode);

    private static OperationResult<T> TrainingIntakeRequestsQuotaExceeded<T>()
        => OperationResult<T>.Fail(
            TrainingIntakeRequestsQuotaExceededSafeMessage,
            TrainingIntakeRequestsQuotaExceededErrorCode);

    private static OperationResult<T> TrainingIntakeRunsQuotaExceeded<T>()
        => OperationResult<T>.Fail(
            TrainingIntakeRunsQuotaExceededSafeMessage,
            TrainingIntakeRunsQuotaExceededErrorCode);

    private static OperationResult<T> TrainingIntakeLocalPromotionsQuotaExceeded<T>()
        => OperationResult<T>.Fail(
            TrainingIntakeLocalPromotionsQuotaExceededSafeMessage,
            TrainingIntakeLocalPromotionsQuotaExceededErrorCode);

    private static OperationResult<T> LocalArtifactDbAuthorityQuotaExceeded<T>()
        => OperationResult<T>.Fail(
            LocalArtifactDbAuthorityQuotaExceededSafeMessage,
            LocalArtifactDbAuthorityQuotaExceededErrorCode);

    private static OperationResult<T> LocalArtifactDbRecordIdConflict<T>()
        => OperationResult<T>.Fail(LocalArtifactDbRecordIdConflictSafeMessage, LocalArtifactDbRecordIdConflictErrorCode);
}
