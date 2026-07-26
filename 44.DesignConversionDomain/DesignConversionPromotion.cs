// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;

public enum DesignConversionPromotionDecisionKind
{
    Promote,
    Reject,
    Rollback,
    ContinueTraining,
    Quarantine
}

public sealed record DesignConversionPromotionGateReport(
    double HeldOutScore,
    double RequiredHeldOutScore,
    bool HeldOutSourceDisjoint,
    bool DeterministicShortcutDetected,
    bool HiddenShortcutEvidenceDetected,
    bool FreshLoadPassed,
    bool BudgetTierPolicyPassed,
    string BudgetTier,
    int DpoQuarantineCount,
    int MaxDpoQuarantineCount,
    IReadOnlyList<string> RequiredArtifactPaths,
    IReadOnlyList<string> PresentArtifactPaths,
    IReadOnlyList<string> EvidenceRefs,
    double ContinueTrainingMargin = 0.03);

public sealed record DesignConversionPromotionDecision(
    DesignConversionPromotionDecisionKind Kind,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> EvidenceRefs)
{
    public bool AllowsUpload => Kind == DesignConversionPromotionDecisionKind.Promote;
}

public enum DesignConversionPromotionPublicationKind
{
    LocalOnlyNoRemoteAttempt,
    VerifiedModelServerUpload,
    Rejected
}

public static class DesignConversionPromotionPublicationStatus
{
    public const string NotUploaded = "not_uploaded";
    public const string UploadedVerified = "uploaded_verified";
    public const string Rejected = "rejected";
}

public sealed record DesignConversionPromotionUploadReceipt(
    string UploadId,
    string RemoteRef,
    DateTimeOffset UploadedAtUtc,
    IReadOnlyList<string> EvidenceRefs)
{
    public DesignConversionPromotionPublicationKind PublicationKind { get; init; } =
        DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt;

    public bool UploadAttempted { get; init; }

    public bool UploadSucceeded { get; init; }

    public bool VerificationSucceeded { get; init; }

    public string PublicationStatus { get; init; } = DesignConversionPromotionPublicationStatus.NotUploaded;

    public string ProvenanceRef { get; init; } = string.Empty;
}

public sealed record DesignConversionPromotionPublicationResult(
    DesignConversionPromotionDecision Decision,
    DesignConversionTrainingManifest Manifest,
    DesignConversionPromotionUploadReceipt? UploadReceipt)
{
    public DesignConversionPromotionPublicationKind PublicationKind { get; init; } =
        UploadReceipt?.PublicationKind ?? DesignConversionPromotionPublicationKind.Rejected;

    public string PublicationStatus { get; init; } =
        UploadReceipt?.PublicationStatus ?? DesignConversionPromotionPublicationStatus.Rejected;
}

public interface IDesignConversionPromotionUploader
{
    Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
        DesignConversionTrainingManifest manifest,
        DesignConversionPromotionGateReport gateReport,
        CancellationToken ct = default);
}

public sealed class DesignConversionPromotionGateEvaluator
{
    public DesignConversionPromotionDecision Decide(DesignConversionPromotionGateReport gateReport)
    {
        if (gateReport is null)
            return Reject("DESIGN_CONVERSION_PROMOTION_GATE_REPORT_REQUIRED");

        var hardReasons = new List<string>();
        var evidence = new List<string>(gateReport.EvidenceRefs ?? []);

        if (gateReport.HiddenShortcutEvidenceDetected)
            hardReasons.Add("HIDDEN_SHORTCUT_EVIDENCE_DETECTED");

        if (gateReport.DeterministicShortcutDetected)
            hardReasons.Add("DETERMINISTIC_SHORTCUT_DETECTED");

        if (!gateReport.HeldOutSourceDisjoint)
            hardReasons.Add("HELD_OUT_SOURCE_DISJOINT_CHECK_FAILED");

        if (!gateReport.BudgetTierPolicyPassed)
            hardReasons.Add("BUDGET_TIER_POLICY_FAILED");

        var missingArtifacts = FindMissingArtifacts(gateReport);
        hardReasons.AddRange(missingArtifacts.Select(path => $"REQUIRED_ARTIFACT_MISSING:{path}"));

        if (hardReasons.Count > 0)
            return new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                hardReasons.ToArray(),
                WithGateEvidence(evidence, "DESIGN_CONVERSION_PROMOTION_REJECTED"));

        if (gateReport.DpoQuarantineCount > gateReport.MaxDpoQuarantineCount)
            return new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Quarantine,
                [$"DPO_QUARANTINE_COUNT_EXCEEDED:{gateReport.DpoQuarantineCount}>{gateReport.MaxDpoQuarantineCount}"],
                WithGateEvidence(evidence, "DESIGN_CONVERSION_PROMOTION_QUARANTINED"));

        if (!gateReport.FreshLoadPassed)
            return new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Rollback,
                ["FRESH_LOAD_CHECK_FAILED"],
                WithGateEvidence(evidence, "DESIGN_CONVERSION_PROMOTION_ROLLBACK_REQUIRED"));

        if (gateReport.HeldOutScore < gateReport.RequiredHeldOutScore)
        {
            var gap = gateReport.RequiredHeldOutScore - gateReport.HeldOutScore;
            if (gap <= gateReport.ContinueTrainingMargin)
            {
                return new DesignConversionPromotionDecision(
                    DesignConversionPromotionDecisionKind.ContinueTraining,
                    [$"HELD_OUT_SCORE_NEAR_THRESHOLD:{gateReport.HeldOutScore:0.######}<{gateReport.RequiredHeldOutScore:0.######}"],
                    WithGateEvidence(evidence, "DESIGN_CONVERSION_PROMOTION_CONTINUE_TRAINING"));
            }

            return new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                [$"HELD_OUT_SCORE_BELOW_THRESHOLD:{gateReport.HeldOutScore:0.######}<{gateReport.RequiredHeldOutScore:0.######}"],
                WithGateEvidence(evidence, "DESIGN_CONVERSION_PROMOTION_REJECTED"));
        }

        return new DesignConversionPromotionDecision(
            DesignConversionPromotionDecisionKind.Promote,
            ["ALL_PROMOTION_GATES_PASSED"],
            WithGateEvidence(evidence, "DESIGN_CONVERSION_PROMOTION_GATES_PASSED"));
    }

    private static DesignConversionPromotionDecision Reject(string reason)
        => new(
            DesignConversionPromotionDecisionKind.Reject,
            [reason],
            ["DESIGN_CONVERSION_PROMOTION_REJECTED"]);

    private static IReadOnlyList<string> FindMissingArtifacts(DesignConversionPromotionGateReport gateReport)
    {
        var present = new HashSet<string>(
            (gateReport.PresentArtifactPaths ?? [])
                .Where(path => !string.IsNullOrWhiteSpace(path)),
            StringComparer.Ordinal);

        return (gateReport.RequiredArtifactPaths ?? [])
            .Where(path => string.IsNullOrWhiteSpace(path) || !present.Contains(path))
            .Select(path => string.IsNullOrWhiteSpace(path) ? "<blank>" : path)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> WithGateEvidence(List<string> evidence, string gateEvidence)
    {
        evidence.Add(gateEvidence);
        return evidence.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }
}

public sealed class DesignConversionPromotionPublisher
{
    private readonly IDesignConversionPromotionUploader? _uploader;
    private readonly DesignConversionManifestPublisher _manifestPublisher;
    private readonly DesignConversionPromotionGateEvaluator _gateEvaluator;

    public DesignConversionPromotionPublisher(
        IDesignConversionPromotionUploader? uploader = null,
        DesignConversionManifestPublisher? manifestPublisher = null,
        DesignConversionPromotionGateEvaluator? gateEvaluator = null)
    {
        _uploader = uploader;
        _manifestPublisher = manifestPublisher ?? new DesignConversionManifestPublisher();
        _gateEvaluator = gateEvaluator ?? new DesignConversionPromotionGateEvaluator();
    }

    public async Task<OperationResult<DesignConversionPromotionPublicationResult>> PublishAsync(
        DesignConversionMachine machine,
        DesignConversionTrainingPipelineResult pipelineResult,
        string manifestId,
        DesignConversionTrainingManifestArtifacts artifacts,
        DesignConversionPromotionGateReport gateReport,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                "DesignConversion promotion cancelled.",
                "CANCELLED");

        if (artifacts is null)
            return OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                "DesignConversion promotion artifacts are required.",
                "DESIGN_CONVERSION_PROMOTION_ARTIFACTS_REQUIRED");

        if (pipelineResult is null)
            return OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                "DesignConversion training pipeline result is required.",
                "DESIGN_CONVERSION_PIPELINE_RESULT_REQUIRED");

        var evaluatedDecision = _gateEvaluator.Decide(gateReport);
        var decision = pipelineResult.DatasetCountGate.Passed
            ? evaluatedDecision
            : new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                pipelineResult.DatasetCountGate.Reasons
                    .Prepend("DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_NOT_PASSED")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                evaluatedDecision.EvidenceRefs
                    .Concat(pipelineResult.BoundaryDecision.EvidenceRefs)
                    .Append("DESIGN_CONVERSION_PROMOTION_REJECTED_BY_DATASET_COUNT_GATE")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());

        if (decision.AllowsUpload && !MeasurementGatePassed(pipelineResult))
        {
            var measurementStatus = string.IsNullOrWhiteSpace(pipelineResult.LifecycleEvidence.MeasurementGateStatus)
                ? "missing"
                : pipelineResult.LifecycleEvidence.MeasurementGateStatus;
            decision = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                [$"DESIGN_CONVERSION_MEASUREMENT_GATE_NOT_PASSED:{measurementStatus}"],
                decision.EvidenceRefs
                    .Concat(pipelineResult.LifecycleEvidence.EvidenceRefs)
                    .Append("DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_WITHOUT_MEASUREMENT_GATE")
                    .Append("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());
        }

        if (decision.AllowsUpload && !SourceDisjointHeldOutReadinessGatePassed(pipelineResult))
        {
            decision = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                [DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGateNotProvenReason],
                decision.EvidenceRefs
                    .Concat(pipelineResult.LifecycleEvidence.EvidenceRefs)
                    .Concat(pipelineResult.LifecycleEvidence.MeasurementGate?.EvidenceRefs ?? [])
                    .Append(DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGateBlockedEvidence)
                    .Append("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());
        }

        if (decision.AllowsUpload && !pipelineResult.LifecycleEvidence.CheckpointContainsRealTrainedWeights)
        {
            decision = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                ["DESIGN_CONVERSION_REAL_TRAINED_WEIGHTS_NOT_PROVEN"],
                decision.EvidenceRefs
                    .Concat(pipelineResult.LifecycleEvidence.EvidenceRefs)
                    .Append("DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_REAL_TRAINED_WEIGHTS_NOT_PROVEN")
                    .Append("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());
        }

        if (decision.AllowsUpload && !pipelineResult.LifecycleEvidence.PromotionUploadAllowed)
        {
            decision = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                ["DESIGN_CONVERSION_PROMOTION_UPLOAD_NOT_ALLOWED_BY_LIFECYCLE_EVIDENCE"],
                decision.EvidenceRefs
                    .Concat(pipelineResult.LifecycleEvidence.EvidenceRefs)
                    .Append("DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_BY_LIFECYCLE_EVIDENCE")
                    .Append("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());
        }

        var manifestResult = await _manifestPublisher.PublishManifestMetadataAsync(
            machine,
            pipelineResult,
            manifestId,
            ct);

        if (!manifestResult.IsSuccess)
            return manifestResult.MapError<DesignConversionPromotionPublicationResult>();

        var manifest = manifestResult.Value! with
        {
            PromotionArtifacts = artifacts,
            EvidenceRefs = manifestResult.Value!.EvidenceRefs
                .Concat(decision.EvidenceRefs)
                .Concat(BuildArtifactEvidence(artifacts))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };

        if (!decision.AllowsUpload)
        {
            return OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                new DesignConversionPromotionPublicationResult(decision, manifest, null));
        }

        if (_uploader is null)
        {
            var rejected = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                ["DESIGN_CONVERSION_PROMOTION_UPLOADER_REQUIRED"],
                decision.EvidenceRefs
                    .Append("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray());

            return OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                new DesignConversionPromotionPublicationResult(rejected, manifest, null));
        }

        var upload = await _uploader.UploadAsync(manifest, gateReport, ct);
        if (!upload.IsSuccess)
        {
            return OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                "DesignConversion promotion uploader failed.",
                SafeErrorCode(upload.ErrorCode));
        }

        var receiptValidation = ValidateReceipt(upload.Value);
        if (!receiptValidation.IsAccepted)
        {
            var rejected = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                receiptValidation.Reasons,
                decision.EvidenceRefs
                    .Concat(receiptValidation.EvidenceRefs)
                    .Append("DESIGN_CONVERSION_PROMOTION_RECEIPT_REJECTED")
                    .Append("MODEL_SERVER_UPLOAD_NOT_CLAIMED_REJECTED_RECEIPT")
                    .Append("MODEL_SERVER_PROMOTION_NOT_CLAIMED_REJECTED_RECEIPT")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());

            var rejectedManifest = manifest with
            {
                ModelServerUploadAttempted = false,
                ModelServerPromotionClaimed = false,
                LocalPromotionReceiptWritten = false,
                PromotionReceiptKind = receiptValidation.ManifestReceiptKind,
                PromotionUploadRef = null,
                PromotionUploadedAtUtc = null,
                EvidenceRefs = manifest.EvidenceRefs
                    .Concat(rejected.EvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()
            };

            return OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                new DesignConversionPromotionPublicationResult(rejected, rejectedManifest, null)
                {
                    PublicationKind = DesignConversionPromotionPublicationKind.Rejected,
                    PublicationStatus = DesignConversionPromotionPublicationStatus.Rejected
                });
        }

        var receipt = receiptValidation.Receipt!;
        if (receiptValidation.PublicationKind == DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt)
        {
            var uploadedManifest = manifest with
            {
                ModelServerUploadAttempted = false,
                ModelServerPromotionClaimed = false,
                LocalPromotionReceiptWritten = true,
                PromotionReceiptKind = "local_receipt_only",
                PromotionUploadRef = receipt.RemoteRef,
                PromotionUploadedAtUtc = receipt.UploadedAtUtc,
                EvidenceRefs = manifest.EvidenceRefs
                    .Concat(receipt.EvidenceRefs)
                    .Concat(BuildLocalReceiptEvidenceRefs())
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()
            };

            return OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                new DesignConversionPromotionPublicationResult(decision, uploadedManifest, receipt));
        }

        var remoteManifest = manifest with
        {
            ModelServerUploadAttempted = true,
            ModelServerPromotionClaimed = true,
            LocalPromotionReceiptWritten = false,
            PromotionReceiptKind = "model_server_upload_verified",
            PromotionUploadRef = receipt.RemoteRef,
            PromotionUploadedAtUtc = receipt.UploadedAtUtc,
            EvidenceRefs = manifest.EvidenceRefs
                .Where(reference => !IsStalePreUploadEvidenceForVerifiedRemote(reference))
                .Concat(receipt.EvidenceRefs)
                .Concat(BuildVerifiedRemoteReceiptEvidenceRefs(receipt))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };

        return OperationResult<DesignConversionPromotionPublicationResult>.Ok(
            new DesignConversionPromotionPublicationResult(decision, remoteManifest, receipt)
            {
                PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
                PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified
            });
    }

    private sealed record ReceiptValidationResult(
        bool IsAccepted,
        DesignConversionPromotionPublicationKind PublicationKind,
        string ManifestReceiptKind,
        IReadOnlyList<string> Reasons,
        IReadOnlyList<string> EvidenceRefs,
        DesignConversionPromotionUploadReceipt? Receipt);

    private static ReceiptValidationResult ValidateReceipt(DesignConversionPromotionUploadReceipt? receipt)
    {
        if (receipt is null)
        {
            return RejectReceipt(
                DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                "invalid_receipt",
                ["DESIGN_CONVERSION_PROMOTION_RECEIPT_REQUIRED"],
                ["DESIGN_CONVERSION_PROMOTION_RECEIPT_REJECTED_MISSING"]);
        }

        var safeReceipt = SanitizeReceipt(receipt);
        return receipt.PublicationKind switch
        {
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt =>
                ValidateLocalReceipt(receipt, safeReceipt),
            DesignConversionPromotionPublicationKind.VerifiedModelServerUpload =>
                ValidateVerifiedRemoteReceipt(receipt, safeReceipt),
            _ => RejectReceipt(
                receipt.PublicationKind,
                "invalid_receipt",
                ["DESIGN_CONVERSION_PROMOTION_RECEIPT_PUBLICATION_KIND_UNRECOGNIZED"],
                safeReceipt.EvidenceRefs.Append("DESIGN_CONVERSION_PROMOTION_RECEIPT_REJECTED_PUBLICATION_KIND").ToArray())
        };
    }

    private static ReceiptValidationResult ValidateLocalReceipt(
        DesignConversionPromotionUploadReceipt receipt,
        DesignConversionPromotionUploadReceipt safeReceipt)
    {
        var reasons = new List<string>();
        var canonicalRemoteRef = string.Empty;
        if (string.IsNullOrWhiteSpace(receipt.UploadId))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_UPLOAD_ID_REQUIRED");
        else if (!IsSafePublicReceiptToken(receipt.UploadId))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_UPLOAD_ID_UNSAFE");
        if (!string.IsNullOrWhiteSpace(receipt.ProvenanceRef)
            && !IsSafePublicReceiptToken(receipt.ProvenanceRef))
        {
            reasons.Add("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_PROVENANCE_UNSAFE");
        }
        if (string.IsNullOrWhiteSpace(receipt.RemoteRef))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_REF_REQUIRED");
        else if (!TryGetSafeLocalPromotionReceiptRef(receipt.RemoteRef, out canonicalRemoteRef))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_REF_UNSAFE");
        if (receipt.UploadAttempted || receipt.UploadSucceeded || receipt.VerificationSucceeded)
            reasons.Add("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_UPLOAD_FLAGS_MUST_BE_FALSE");
        if (!string.Equals(
                receipt.PublicationStatus,
                DesignConversionPromotionPublicationStatus.NotUploaded,
                StringComparison.Ordinal))
        {
            reasons.Add("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_STATUS_MUST_BE_NOT_UPLOADED");
        }

        if (reasons.Count > 0)
        {
            return RejectReceipt(
                DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                "invalid_local_receipt",
                reasons,
                safeReceipt.EvidenceRefs.Append("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_REJECTED").ToArray());
        }

        return AcceptReceipt(
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            "local_receipt_only",
            safeReceipt.EvidenceRefs.Append("DESIGN_CONVERSION_PROMOTION_LOCAL_RECEIPT_VALIDATED").ToArray(),
            safeReceipt with { RemoteRef = canonicalRemoteRef });
    }

    private static ReceiptValidationResult ValidateVerifiedRemoteReceipt(
        DesignConversionPromotionUploadReceipt receipt,
        DesignConversionPromotionUploadReceipt safeReceipt)
    {
        var reasons = new List<string>();
        var canonicalRemoteRef = string.Empty;
        if (!receipt.UploadAttempted || !receipt.UploadSucceeded || !receipt.VerificationSucceeded)
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_UPLOAD_VERIFICATION_REQUIRED");
        if (!string.Equals(
                receipt.PublicationStatus,
                DesignConversionPromotionPublicationStatus.UploadedVerified,
                StringComparison.Ordinal))
        {
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_STATUS_MUST_BE_UPLOADED_VERIFIED");
        }

        if (string.IsNullOrWhiteSpace(receipt.UploadId))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_UPLOAD_ID_REQUIRED");
        else if (!IsSafePublicReceiptToken(receipt.UploadId))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_UPLOAD_ID_UNSAFE");
        if (string.IsNullOrWhiteSpace(receipt.ProvenanceRef))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_PROVENANCE_REQUIRED");
        else if (!IsSafePublicReceiptToken(receipt.ProvenanceRef))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_PROVENANCE_UNSAFE");
        if (string.IsNullOrWhiteSpace(receipt.RemoteRef))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_HTTPS_REF_REQUIRED");
        else if (!TryGetSafeAbsoluteHttpsReceiptRef(receipt.RemoteRef, out canonicalRemoteRef))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_HTTPS_REF_UNSAFE");
        if ((receipt.EvidenceRefs ?? []).All(string.IsNullOrWhiteSpace))
            reasons.Add("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_EVIDENCE_REQUIRED");

        if (reasons.Count > 0)
        {
            return RejectReceipt(
                DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
                "invalid_remote_receipt",
                reasons,
                safeReceipt.EvidenceRefs.Append("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_REJECTED").ToArray());
        }

        return AcceptReceipt(
            DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
            "model_server_upload_verified",
            safeReceipt.EvidenceRefs.Append("DESIGN_CONVERSION_PROMOTION_REMOTE_RECEIPT_VALIDATED").ToArray(),
            safeReceipt with { RemoteRef = canonicalRemoteRef });
    }

    private static ReceiptValidationResult AcceptReceipt(
        DesignConversionPromotionPublicationKind publicationKind,
        string manifestReceiptKind,
        IReadOnlyList<string> evidenceRefs,
        DesignConversionPromotionUploadReceipt receipt)
        => new(
            IsAccepted: true,
            PublicationKind: publicationKind,
            ManifestReceiptKind: manifestReceiptKind,
            Reasons: [],
            EvidenceRefs: evidenceRefs
                .Where(reference => !string.IsNullOrWhiteSpace(reference))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Receipt: receipt);

    private static ReceiptValidationResult RejectReceipt(
        DesignConversionPromotionPublicationKind publicationKind,
        string manifestReceiptKind,
        IReadOnlyList<string> reasons,
        IReadOnlyList<string> evidenceRefs)
        => new(
            IsAccepted: false,
            PublicationKind: publicationKind,
            ManifestReceiptKind: manifestReceiptKind,
            Reasons: reasons
                .Where(reason => !string.IsNullOrWhiteSpace(reason))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            EvidenceRefs: evidenceRefs
                .Where(reference => !string.IsNullOrWhiteSpace(reference))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Receipt: null);

    private static bool TryGetSafeLocalPromotionReceiptRef(string remoteRef, out string canonicalRef)
    {
        canonicalRef = string.Empty;
        var prefix = DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix;
        if (!HasSafeRawPromotionReceiptRef(remoteRef)
            || !remoteRef.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var encodedToken = remoteRef[prefix.Length..];
        if (encodedToken.Length == 0)
            return false;

        string decodedToken;
        try
        {
            decodedToken = Uri.UnescapeDataString(encodedToken);
        }
        catch (UriFormatException)
        {
            return false;
        }

        if (decodedToken is "." or ".." || !IsSafePublicReceiptToken(decodedToken))
            return false;

        var canonicalToken = Uri.EscapeDataString(decodedToken);
        if (!string.Equals(encodedToken, canonicalToken, StringComparison.Ordinal))
            return false;

        canonicalRef = prefix + canonicalToken;
        return true;
    }

    private static DesignConversionPromotionUploadReceipt SanitizeReceipt(
        DesignConversionPromotionUploadReceipt receipt)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var evidenceRefs = (receipt.EvidenceRefs ?? [])
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Select(BuildUploadReceiptEvidenceHashRef)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var provenanceRef = DesignConversionDpoLeakageGuard.SanitizeScalar(
            receipt.ProvenanceRef,
            "design_conversion_promotion_receipt_provenance",
            redactions);

        return receipt with
        {
            EvidenceRefs = evidenceRefs,
            ProvenanceRef = provenanceRef
        };
    }

    private static string BuildUploadReceiptEvidenceHashRef(string evidenceRef)
        => $"UPLOAD_RECEIPT_EVIDENCE_SHA256:{BuildSha256Hex(evidenceRef)}";

    private static bool IsSafePublicReceiptToken(string value)
    {
        const int maxLength = 256;
        string[] reservedPublisherFragments =
        [
            "MODEL_SERVER_",
            "PROMOTION_",
            "DESIGN_CONVERSION_",
            "EXTERNAL_CLOUD_",
            "UPLOAD_RECEIPT_"
        ];
        string[] sensitiveFragments =
        [
            "credential",
            "bearer",
            "token",
            "password",
            "secret",
            "api-key",
            "api_key",
            "apikey"
        ];

        if (string.IsNullOrWhiteSpace(value)
            || value.Length > maxLength
            || !string.Equals(value, value.Trim(), StringComparison.Ordinal)
            || value.Any(char.IsControl)
            || value.IndexOfAny(['=', '?', '&', '/', '\\']) >= 0
            || value.Any(ch => !char.IsAsciiLetterOrDigit(ch) && ch is not '-' and not '_' and not '.' and not ':')
            || DesignConversionDpoLeakageGuard.ContainsForbiddenLeakage(value))
        {
            return false;
        }

        return !reservedPublisherFragments.Any(fragment =>
                   value.Contains(fragment, StringComparison.OrdinalIgnoreCase))
               && !sensitiveFragments.Any(fragment =>
                   value.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildSha256Hex(string value)
        => Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static bool HasSafeRawPromotionReceiptRef(string remoteRef)
    {
        const int maxLength = 2048;
        return !string.IsNullOrWhiteSpace(remoteRef)
               && remoteRef.Length <= maxLength
               && string.Equals(remoteRef, remoteRef.Trim(), StringComparison.Ordinal)
               && !remoteRef.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch))
               && !remoteRef.Contains('\\');
    }

    private static bool TryGetSafeAbsoluteHttpsReceiptRef(string remoteRef, out string canonicalRef)
    {
        canonicalRef = string.Empty;
        if (!TryGetSafeRawHttpsReceiptPath(remoteRef, out var rawPath)
            || !HasSafeRemoteReceiptPath(rawPath))
        {
            return false;
        }

        if (!Uri.TryCreate(remoteRef, UriKind.Absolute, out var parsed)
            || !string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !HasSafeReceiptHost(parsed)
            || !string.IsNullOrEmpty(parsed.UserInfo)
            || !string.IsNullOrEmpty(parsed.Query)
            || !string.IsNullOrEmpty(parsed.Fragment)
            || !HasSafeRemoteReceiptPath(parsed.AbsolutePath))
        {
            return false;
        }

        var candidateCanonicalRef = parsed.AbsoluteUri;
        if (!HasSafeRawPromotionReceiptRef(candidateCanonicalRef)
            || !Uri.TryCreate(candidateCanonicalRef, UriKind.Absolute, out var canonicalUri)
            || !HasSafeRemoteReceiptPath(canonicalUri.AbsolutePath))
        {
            return false;
        }

        canonicalRef = canonicalUri.AbsoluteUri;
        return true;
    }

    private static bool TryGetSafeRawHttpsReceiptPath(string remoteRef, out string rawPath)
    {
        const string schemePrefix = "https://";
        rawPath = string.Empty;
        if (!HasSafeRawPromotionReceiptRef(remoteRef)
            || !remoteRef.StartsWith(schemePrefix, StringComparison.OrdinalIgnoreCase)
            || remoteRef.IndexOfAny(['?', '#']) >= 0)
        {
            return false;
        }

        var authorityStart = schemePrefix.Length;
        var pathStart = remoteRef.IndexOf('/', authorityStart);
        var authorityEnd = pathStart < 0 ? remoteRef.Length : pathStart;
        if (authorityEnd == authorityStart)
            return false;

        var rawAuthority = remoteRef[authorityStart..authorityEnd];
        if (rawAuthority.Contains('@'))
            return false;

        rawPath = pathStart < 0 ? "/" : remoteRef[pathStart..];
        return true;
    }

    private static bool HasSafeReceiptHost(Uri uri)
    {
        var host = uri.IdnHost;
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253)
            return false;

        return uri.HostNameType switch
        {
            UriHostNameType.Dns => host
                .Split('.', StringSplitOptions.None)
                .All(label => label.Length is > 0 and <= 63
                              && char.IsAsciiLetterOrDigit(label[0])
                              && char.IsAsciiLetterOrDigit(label[^1])
                              && label.All(ch => char.IsAsciiLetterOrDigit(ch) || ch == '-')),
            UriHostNameType.IPv4 => host.All(ch => char.IsAsciiDigit(ch) || ch == '.'),
            UriHostNameType.IPv6 => host.All(ch => char.IsAsciiHexDigit(ch) || ch is ':' or '.'),
            _ => false
        };
    }

    private static bool HasSafeRemoteReceiptPath(string rawPath)
    {
        const int maxDecodeLayers = 8;
        var currentLayer = rawPath;

        for (var decodeCount = 0; ; decodeCount++)
        {
            if (!TryValidateRemoteReceiptPathLayer(currentLayer, out var hasPercentEscapes))
                return false;
            if (!hasPercentEscapes)
                return true;
            if (decodeCount >= maxDecodeLayers)
                return false;

            string decodedLayer;
            try
            {
                decodedLayer = Uri.UnescapeDataString(currentLayer);
            }
            catch (UriFormatException)
            {
                return false;
            }

            if (string.Equals(decodedLayer, currentLayer, StringComparison.Ordinal))
                return false;

            currentLayer = decodedLayer;
        }
    }

    private static bool TryValidateRemoteReceiptPathLayer(string path, out bool hasPercentEscapes)
    {
        hasPercentEscapes = false;
        if (path.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\')
            || path.Split('/', StringSplitOptions.None).Any(segment => segment is "." or ".."))
        {
            return false;
        }

        for (var index = 0; index < path.Length; index++)
        {
            if (path[index] != '%')
                continue;

            hasPercentEscapes = true;
            if (index > path.Length - 3)
                return false;

            var high = HexDigitValue(path[index + 1]);
            var low = HexDigitValue(path[index + 2]);
            if (high < 0 || low < 0)
                return false;

            var decodedByte = (high << 4) | low;
            if (decodedByte <= 0x20
                || decodedByte == 0x7f
                || decodedByte is 0x2f or 0x5c)
            {
                return false;
            }

            index += 2;
        }

        return true;
    }

    private static int HexDigitValue(char value)
        => value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1
        };

    private static string SafeErrorCode(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return "DESIGN_CONVERSION_PROMOTION_UPLOAD_FAILED";

        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var sanitized = DesignConversionDpoLeakageGuard.SanitizeScalar(
            errorCode,
            "design_conversion_promotion_upload_error_code",
            redactions);

        return redactions.Count == 0
               && string.Equals(sanitized, errorCode, StringComparison.Ordinal)
               && errorCode.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-')
            ? errorCode
            : "DESIGN_CONVERSION_PROMOTION_UPLOAD_FAILED";
    }

    private static IReadOnlyList<string> BuildLocalReceiptEvidenceRefs()
        =>
        [
            "PROMOTION_RECEIPT_KIND:local_receipt_only",
            $"PROMOTION_PUBLICATION_STATUS:{DesignConversionPromotionPublicationStatus.NotUploaded}",
            "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
            "EXTERNAL_CLOUD_UPLOAD_NOT_ATTEMPTED",
            "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED_LOCAL_RECEIPT",
            "MODEL_SERVER_PROMOTION_NOT_CLAIMED_LOCAL_RECEIPT"
        ];

    private static IReadOnlyList<string> BuildVerifiedRemoteReceiptEvidenceRefs(
        DesignConversionPromotionUploadReceipt receipt)
        =>
        [
            "PROMOTION_RECEIPT_KIND:model_server_upload_verified",
            $"PROMOTION_PUBLICATION_STATUS:{DesignConversionPromotionPublicationStatus.UploadedVerified}",
            "MODEL_SERVER_UPLOAD_ATTEMPTED_VERIFIED_REMOTE_RECEIPT",
            "MODEL_SERVER_UPLOAD_SUCCEEDED_VERIFIED_REMOTE_RECEIPT",
            "MODEL_SERVER_UPLOAD_VERIFIED_REMOTE_RECEIPT",
            "MODEL_SERVER_PROMOTION_CLAIMED_VERIFIED_REMOTE_RECEIPT",
            $"MODEL_SERVER_UPLOAD_PROVENANCE_SHA256:{BuildSha256Hex(receipt.ProvenanceRef)}"
        ];

    private static bool IsStalePreUploadEvidenceForVerifiedRemote(string evidenceRef)
        => string.Equals(evidenceRef, "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED", StringComparison.Ordinal)
           || string.Equals(evidenceRef, "MODEL_SERVER_PROMOTION_NOT_CLAIMED", StringComparison.Ordinal);

    private static bool MeasurementGatePassed(DesignConversionTrainingPipelineResult pipelineResult)
    {
        var lifecycle = pipelineResult.LifecycleEvidence;
        var measurementGate = lifecycle.MeasurementGate;

        return measurementGate?.Passed == true &&
               string.Equals(
                   measurementGate.Status,
                   DesignConversionMeasurementGateStatus.Passed,
                   StringComparison.OrdinalIgnoreCase) &&
               string.Equals(
                   lifecycle.MeasurementGateStatus,
                   DesignConversionMeasurementGateStatus.Passed,
                   StringComparison.OrdinalIgnoreCase) &&
               ContainsMeasurementGatePassEvidence(measurementGate.EvidenceRefs) &&
               ContainsMeasurementGatePassEvidence(lifecycle.EvidenceRefs);
    }

    private static bool ContainsMeasurementGatePassEvidence(IReadOnlyList<string>? evidenceRefs)
        => evidenceRefs?.Contains("DESIGN_CONVERSION_MEASUREMENT_GATE_PASSED", StringComparer.Ordinal) == true;

    private static bool SourceDisjointHeldOutReadinessGatePassed(DesignConversionTrainingPipelineResult pipelineResult)
    {
        var lifecycle = pipelineResult.LifecycleEvidence;
        var measurementGate = lifecycle.MeasurementGate;

        return pipelineResult.DatasetCountGate.SourceDisjoint &&
               pipelineResult.DatasetCountGate.ActualHeldOutRecords > 0 &&
               measurementGate?.SourceDisjoint == true &&
               measurementGate.AblationPassed &&
               measurementGate.BudgetPassed &&
               measurementGate.HeldOutQualityScore is not null &&
               measurementGate.HeldOutQualityScore >= measurementGate.RequiredHeldOutQualityScore &&
               lifecycle.LocalContinueTrainingProbe?.HeldOutRecordUsageCount == 0 &&
               ContainsSourceDisjointHeldOutReadinessGateEvidence(measurementGate.EvidenceRefs) &&
               ContainsSourceDisjointHeldOutReadinessGateEvidence(lifecycle.EvidenceRefs);
    }

    private static bool ContainsSourceDisjointHeldOutReadinessGateEvidence(IReadOnlyList<string>? evidenceRefs)
        => evidenceRefs?.Contains(
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
            StringComparer.Ordinal) == true;

    private static IReadOnlyList<string> BuildArtifactEvidence(DesignConversionTrainingManifestArtifacts artifacts)
        =>
        [
            $"CHECKPOINT_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-checkpoint-path", artifacts.CheckpointPath)}",
            $"TOKENIZER_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-tokenizer-path", artifacts.TokenizerPath)}",
            $"POLICY_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-policy-path", artifacts.PolicyPath)}"
        ];
}
