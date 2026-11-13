// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;
using ContinuousLearning;
using System.Runtime.CompilerServices;

internal enum DesignConversionCallerEvidenceCategory
{
    PureEvaluationProjection,
    CallerMeasurementAdvisory,
    AuthoritativeNonEvaluation,
    UnknownAuthoritative
}

internal static class DesignConversionCallerEvidenceClassifier
{
    private static readonly HashSet<string> PureEvaluationProjectionCodes = new(
        StringComparer.Ordinal)
    {
        "HELD_OUT_SCORE",
        "HELD_OUT_QUALITY_SCORE",
        "HELDOUT_QUALITY",
        "HELD_OUT_ACCURACY",
        "HELD_OUT_RECORD_COUNT",
        "HELDOUT_RECORD_COUNT",
        "HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD",
        "HELD_OUT_SCORE_BELOW_THRESHOLD",
        "HELD_OUT_SCORE_NEAR_THRESHOLD",
        "HELD_OUT_RECORD_COUNT_BELOW_MINIMUM",
        "HELD_OUT_RECORD_COUNT_BELOW_TARGET",
        "HELD_OUT_RECORD_COUNT_PASSED",
        "HELD_OUT_MEASUREMENT_NOT_RUN",
        "SOURCE_DISJOINT_PROOF",
        "SOURCE_DISJOINT_PROOF_MISSING",
        "SOURCE_DISJOINT_PROOF_PASSED",
        "SOURCE_DISJOINT_SPLIT_FAILED",
        "FRESH_LOAD_CHECK_FAILED",
        "FRESH_LOAD_STATUS",
        "MODEL_UPLOAD_READY",
        "MODEL_UPLOAD_NOT_READY",
        "UPLOAD_READINESS_STATUS",
        "MEASUREMENT_GATE_STATUS",
        "PROMOTION_GATE_STATUS",
        "PROMOTION_POLICY_STATUS",
        "PROMOTION_UPLOAD_ALLOWED",
        "PROMOTION_BLOCKED_REASONS",
        "TRAINING_EXECUTION_STATUS",
        "RUNNER_BLOCKED_REASON",
        "BLOCK_REASON_COUNT",
        "held_out_score",
        "held_out_quality_score",
        "heldout_quality",
        "held_out_accuracy",
        "held_out_record_count",
        "heldout_record_count",
        "source_disjoint_proof",
        "fresh_load_status",
        "model_upload_ready",
        "model_upload_not_ready",
        "upload_readiness_status",
        "measurement_gate_status",
        "promotion_gate_status",
        "promotion_policy_status",
        "promotion_upload_allowed",
        "promotion_blocked_reasons",
        "training_execution_status",
        "runner_blocked_reason",
        "block_reason_count",
        "real_training_not_claimed",
        "measurement_gate_not_passed",
        "promotion_policy_not_passed"
    };

    private static readonly HashSet<string> PureProjectionCodesWithNumericDetail = new(
        StringComparer.Ordinal)
    {
        "HELD_OUT_SCORE",
        "HELD_OUT_QUALITY_SCORE",
        "HELDOUT_QUALITY",
        "HELD_OUT_ACCURACY",
        "HELD_OUT_RECORD_COUNT",
        "HELDOUT_RECORD_COUNT",
        "HELD_OUT_QUALITY_SCORE_BELOW_THRESHOLD",
        "HELD_OUT_SCORE_BELOW_THRESHOLD",
        "HELD_OUT_SCORE_NEAR_THRESHOLD",
        "HELD_OUT_RECORD_COUNT_BELOW_MINIMUM",
        "HELD_OUT_RECORD_COUNT_BELOW_TARGET",
        "HELD_OUT_RECORD_COUNT_PASSED",
        "held_out_score",
        "held_out_quality_score",
        "heldout_quality",
        "held_out_accuracy",
        "held_out_record_count",
        "heldout_record_count"
    };

    private static readonly HashSet<string> PureProjectionCodesWithAnyDetail = new(
        StringComparer.Ordinal)
    {
        "measurement_gate_not_passed",
        "promotion_policy_not_passed"
    };

    private static readonly HashSet<string> CallerMeasurementAdvisoryCodes = new(
        StringComparer.Ordinal)
    {
        "DESIGN_CONVERSION_CHILD_DPO_MEASUREMENT_NOT_AVAILABLE_IN_TRAINING_PIPELINE",
        "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_BUDGET_AUTHORITY_REQUIRED",
        "DESIGN_CONVERSION_SERVER_OWNED_PROMOTION_INTEGRITY_AUTHORITY_REQUIRED"
    };

    private static readonly HashSet<string> KnownAuthoritativeCodes = new(
        StringComparer.Ordinal)
    {
        "DESIGN_CONVERSION_CONTINUE_TRAINING_HELD_OUT_RECORD_USAGE_DETECTED",
        "DESIGN_CONVERSION_CONTINUE_TRAINING_FRESH_LOAD_METADATA_MISMATCH",
        "HELD_OUT_PRIVACY_FAILURE",
        "SOURCE_DISJOINT_LEAKAGE_DETECTED",
        "FRESH_LOAD_CHECKPOINT_INTEGRITY_FAILED",
        "HELD_OUT_TRAINING_FAILURE",
        "FRESH_LOAD_FATAL",
        "MODEL_UPLOAD_FAILED"
    };

    private static readonly string[] KnownAuthoritativePrefixes =
    [
        "DESIGN_CONVERSION_CONTINUE_TRAINING_",
        "DESIGN_CONVERSION_PROMOTION_UPLOAD_",
        "DESIGN_CONVERSION_CONFIGURED_PROMOTION_UPLOAD_",
        "DESIGN_CONVERSION_ARTIFACT_UPLOAD_"
    ];

    private static readonly string[] KnownAuthoritativeSuffixes =
    [
        "_FAILED",
        "_FAILURE",
        "_ERROR",
        "_FATAL",
        "_EXCEPTION",
        "_CANCELLED",
        "_CANCELED",
        "_TIMEOUT",
        "_ABORT",
        "_ABORTED",
        "_MISMATCH",
        "_DETECTED"
    ];

    internal static DesignConversionCallerEvidenceCategory Classify(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DesignConversionCallerEvidenceCategory.UnknownAuthoritative;

        var detailSeparator = value.IndexOf(':');
        var code = detailSeparator < 0 ? value : value[..detailSeparator];
        var detail = detailSeparator < 0 ? null : value[(detailSeparator + 1)..];
        if (CallerMeasurementAdvisoryCodes.Contains(code))
            return DesignConversionCallerEvidenceCategory.CallerMeasurementAdvisory;

        if (PureEvaluationProjectionCodes.Contains(code) &&
            (detail is null ||
             PureProjectionCodesWithAnyDetail.Contains(code) ||
             PureProjectionCodesWithNumericDetail.Contains(code) && IsNumericProjectionDetail(detail)))
        {
            return DesignConversionCallerEvidenceCategory.PureEvaluationProjection;
        }

        if (KnownAuthoritativeCodes.Contains(code) ||
            KnownAuthoritativePrefixes.Any(prefix => code.StartsWith(prefix, StringComparison.Ordinal)) ||
            KnownAuthoritativeSuffixes.Any(suffix => code.EndsWith(suffix, StringComparison.Ordinal)))
        {
            return DesignConversionCallerEvidenceCategory.AuthoritativeNonEvaluation;
        }

        return DesignConversionCallerEvidenceCategory.UnknownAuthoritative;
    }

    internal static bool IsPureEvaluationProjection(string? value)
        => Classify(value) == DesignConversionCallerEvidenceCategory.PureEvaluationProjection;

    internal static bool IsCallerMeasurementAdvisory(string? value)
        => Classify(value) == DesignConversionCallerEvidenceCategory.CallerMeasurementAdvisory;

    private static bool IsNumericProjectionDetail(string detail)
    {
        if (detail.Length == 0 || detail.Any(char.IsWhiteSpace))
            return false;

        var index = 0;
        if (TryReadComparisonOperator(detail, ref index))
            return TryReadNumber(detail, ref index) && index == detail.Length;

        if (!TryReadNumber(detail, ref index))
            return false;
        if (index == detail.Length)
            return true;

        if (TryReadComparisonOperator(detail, ref index))
            return TryReadNumber(detail, ref index) && index == detail.Length;

        if (TryReadRangeSeparator(detail, ref index))
            return TryReadNumber(detail, ref index) && index == detail.Length;

        return false;
    }

    private static bool TryReadNumber(string value, ref int index)
    {
        if (index < value.Length && value[index] is '+' or '-')
            index++;

        var integerDigits = ReadAsciiDigits(value, ref index);
        var fractionalDigits = 0;
        if (index < value.Length && value[index] == '.' &&
            (index + 1 >= value.Length || value[index + 1] != '.'))
        {
            index++;
            fractionalDigits = ReadAsciiDigits(value, ref index);
        }

        if (integerDigits + fractionalDigits == 0)
            return false;

        if (index < value.Length && value[index] is 'e' or 'E')
        {
            index++;
            if (index < value.Length && value[index] is '+' or '-')
                index++;
            if (ReadAsciiDigits(value, ref index) == 0)
                return false;
        }

        if (index < value.Length && value[index] == '%')
            index++;
        return true;
    }

    private static int ReadAsciiDigits(string value, ref int index)
    {
        var start = index;
        while (index < value.Length && char.IsAsciiDigit(value[index]))
            index++;
        return index - start;
    }

    private static bool TryReadComparisonOperator(string value, ref int index)
    {
        if (index >= value.Length || value[index] is not ('<' or '>' or '='))
            return false;

        index++;
        if (index < value.Length && value[index] == '=')
            index++;
        return true;
    }

    private static bool TryReadRangeSeparator(string value, ref int index)
    {
        if (index + 1 < value.Length && value[index] == '.' && value[index + 1] == '.')
        {
            index += 2;
            return true;
        }

        if (index < value.Length && value[index] is '/' or '-')
        {
            index++;
            return true;
        }

        return false;
    }
}

public enum DesignConversionPromotionDecisionKind
{
    Promote,
    Reject,
    Rollback,
    ContinueTraining,
    Quarantine
}

public static class DesignConversionPromotionGateDefaults
{
    public const double ContinueTrainingMargin = 0.03d;
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
    double ContinueTrainingMargin = DesignConversionPromotionGateDefaults.ContinueTrainingMargin);

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

    public bool ExternalCloudUploadAttempted { get; init; }

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

/// <summary>
/// Declares a hard upper bound for the serialized receipt returned by a promotion
/// uploader. Crash-safe promotion rejects uploaders that cannot make this promise
/// before the durable mutation intent is recorded.
/// </summary>
public interface IDesignConversionBoundedPromotionUploader :
    IDesignConversionPromotionUploader
{
    int MaximumSerializedReceiptUtf8Bytes { get; }
}

/// <summary>
/// Required mutation-free configuration preflight for every configured promotion
/// uploader. It runs before durable intent and must not perform network or
/// repository writes.
/// </summary>
public interface IDesignConversionPromotionUploaderPreflight
{
    /// <summary>
    /// Validates configuration without mutation and returns a canonical,
    /// non-secret SHA-256 fingerprint of the exact upload destination,
    /// credentials, implementation contract, and configured bounds.
    /// </summary>
    OperationResult<string> PreflightConfigurationFingerprintSha256();
}

internal enum DesignConversionPromotionUploadAuthorityMode
{
    Unrestricted,
    LocalReceiptOnly,
    ServerOwnedConfiguredRemote
}

internal sealed class DesignConversionLocalReceiptOnlyPromotionUploadAuthority
{
    private const string SchemaVersion =
        "design-conversion-local-receipt-only-promotion-upload-authority-v1";
    private const string Mode = "LocalReceiptOnly";
    private const int MaximumIssuerEvidenceRefChars = 512;
    private const string DefaultIssuerEvidenceRef =
        "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INTERNAL_ISSUER";
    private const string IssuerEvidenceInvalidMessage =
        "DesignConversion local receipt-only upload authority issuer evidence is invalid.";
    private const string IssuerEvidenceInvalidCode =
        "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_ISSUER_EVIDENCE_INVALID";

    private static readonly ConditionalWeakTable<
        DesignConversionLocalReceiptOnlyPromotionUploadAuthority,
        MintRecord> Mints = new();

    private DesignConversionLocalReceiptOnlyPromotionUploadAuthority()
    {
    }

    internal static OperationResult<DesignConversionLocalReceiptOnlyPromotionUploadAuthority>
        Issue(
            IDesignConversionPromotionUploader uploader,
            string? issuerEvidenceRef)
    {
        if (uploader is null)
        {
            return OperationResult<DesignConversionLocalReceiptOnlyPromotionUploadAuthority>
                .Fail(
                    "DesignConversion local receipt-only upload authority requires an uploader.",
                    "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_UPLOADER_REQUIRED");
        }

        if (uploader is not IDesignConversionBoundedPromotionUploader bounded ||
            bounded.MaximumSerializedReceiptUtf8Bytes <= 0 ||
            bounded.MaximumSerializedReceiptUtf8Bytes > 64 * 1024 ||
            uploader is not IDesignConversionPromotionUploaderPreflight preflight)
        {
            return OperationResult<DesignConversionLocalReceiptOnlyPromotionUploadAuthority>
                .Fail(
                    "DesignConversion local receipt-only upload authority requires bounded receipt and mutation-free preflight contracts.",
                    "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_BOUND_REQUIRED");
        }

        var fingerprint = preflight.PreflightConfigurationFingerprintSha256();
        if (!fingerprint.IsSuccess)
        {
            return fingerprint.MapError<
                DesignConversionLocalReceiptOnlyPromotionUploadAuthority>();
        }

        if (!IsSha256Ref(fingerprint.Value))
        {
            return OperationResult<DesignConversionLocalReceiptOnlyPromotionUploadAuthority>
                .Fail(
                    "DesignConversion local receipt-only upload authority preflight fingerprint is invalid.",
                    "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_FINGERPRINT_INVALID");
        }

        var issuerEvidence = ValidateIssuerEvidenceRef(issuerEvidenceRef);
        if (!issuerEvidence.IsSuccess)
        {
            return issuerEvidence.MapError<
                DesignConversionLocalReceiptOnlyPromotionUploadAuthority>();
        }

        var typeIdentity = BuildUploaderTypeIdentity(uploader.GetType());
        var stableFingerprint = BuildStableAuthorityFingerprint(
            typeIdentity,
            bounded.MaximumSerializedReceiptUtf8Bytes,
            fingerprint.Value!,
            issuerEvidence.Value!);
        var authority = new DesignConversionLocalReceiptOnlyPromotionUploadAuthority();
        Mints.Add(
            authority,
            new MintRecord(
                uploader,
                typeIdentity,
                bounded.MaximumSerializedReceiptUtf8Bytes,
                fingerprint.Value!,
                stableFingerprint,
                issuerEvidence.Value!));
        return OperationResult<DesignConversionLocalReceiptOnlyPromotionUploadAuthority>
            .Ok(authority);
    }

    internal OperationResult<DesignConversionLocalReceiptOnlyPromotionUploadAuthorityValidation>
        Validate(
            IDesignConversionPromotionUploader uploader,
            string currentPreflightFingerprintSha256,
            int currentMaximumSerializedReceiptUtf8Bytes)
    {
        if (!Mints.TryGetValue(this, out var mint))
        {
            return Invalid();
        }

        if (!ReferenceEquals(mint.Uploader, uploader) ||
            !string.Equals(
                mint.UploaderTypeIdentity,
                BuildUploaderTypeIdentity(uploader.GetType()),
                StringComparison.Ordinal) ||
            mint.MaximumSerializedReceiptUtf8Bytes !=
            currentMaximumSerializedReceiptUtf8Bytes ||
            !string.Equals(
                mint.PreflightFingerprintSha256,
                currentPreflightFingerprintSha256,
                StringComparison.Ordinal))
        {
            return Invalid();
        }

        return OperationResult<
            DesignConversionLocalReceiptOnlyPromotionUploadAuthorityValidation>
            .Ok(new DesignConversionLocalReceiptOnlyPromotionUploadAuthorityValidation(
                mint.StableAuthorityFingerprintSha256,
                mint.MaximumSerializedReceiptUtf8Bytes,
                mint.IssuerEvidenceRef));

        static OperationResult<
            DesignConversionLocalReceiptOnlyPromotionUploadAuthorityValidation>
            Invalid()
            => OperationResult<
                DesignConversionLocalReceiptOnlyPromotionUploadAuthorityValidation>
                .Fail(
                    "DesignConversion local receipt-only upload authority capability is invalid for the configured uploader.",
                    "DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_UPLOAD_AUTHORITY_INVALID");
    }

    private static string BuildStableAuthorityFingerprint(
        string uploaderTypeIdentity,
        int maximumSerializedReceiptUtf8Bytes,
        string preflightFingerprintSha256,
        string issuerEvidenceRef)
        => $"sha256:{BuildSha256Hex(string.Join("|",
        [
            SchemaVersion,
            Mode,
            uploaderTypeIdentity,
            maximumSerializedReceiptUtf8Bytes.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            preflightFingerprintSha256,
            issuerEvidenceRef
        ]))}";

    private static string BuildUploaderTypeIdentity(Type type)
    {
        var assembly = type.Assembly.GetName();
        return string.Join("|",
        [
            type.FullName ?? type.Name,
            assembly.Name ?? string.Empty,
            assembly.Version?.ToString() ?? string.Empty
        ]);
    }

    private static bool IsSha256Ref(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.Length == "sha256:".Length + 64 &&
           value["sha256:".Length..].All(Uri.IsHexDigit);

    private static OperationResult<string> ValidateIssuerEvidenceRef(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return OperationResult<string>.Ok(DefaultIssuerEvidenceRef);

        if (value.Length > MaximumIssuerEvidenceRefChars ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Any(character =>
                char.IsWhiteSpace(character) || char.IsControl(character)))
        {
            return InvalidIssuerEvidenceRef();
        }

        if (IsSymbolicIssuerEvidenceRef(value) ||
            IsCanonicalSha256Ref(value))
        {
            return OperationResult<string>.Ok(value);
        }

        return InvalidIssuerEvidenceRef();
    }

    private static bool IsSymbolicIssuerEvidenceRef(string value)
        => value.StartsWith("DESIGN_CONVERSION_", StringComparison.Ordinal) &&
           value.Length > "DESIGN_CONVERSION_".Length &&
           value.All(character =>
               character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_');

    private static bool IsCanonicalSha256Ref(string value)
        => value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.Length == "sha256:".Length + 64 &&
           value["sha256:".Length..].All(character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<string> InvalidIssuerEvidenceRef()
        => OperationResult<string>.Fail(
            IssuerEvidenceInvalidMessage,
            IssuerEvidenceInvalidCode);

    private static string BuildSha256Hex(string value)
        => Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private sealed record MintRecord(
        IDesignConversionPromotionUploader Uploader,
        string UploaderTypeIdentity,
        int MaximumSerializedReceiptUtf8Bytes,
        string PreflightFingerprintSha256,
        string StableAuthorityFingerprintSha256,
        string IssuerEvidenceRef);
}

internal sealed record
    DesignConversionLocalReceiptOnlyPromotionUploadAuthorityValidation(
        string StableAuthorityFingerprintSha256,
        int MaximumSerializedReceiptUtf8Bytes,
        string IssuerEvidenceRef);

internal sealed class DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority
{
    private const string SchemaVersion =
        "design-conversion-server-owned-configured-remote-promotion-upload-authority-v1";
    private const string Mode = "ServerOwnedConfiguredRemote";
    private const int MaximumIssuerEvidenceRefChars = 512;
    private const string IssuerEvidenceInvalidMessage =
        "DesignConversion server-owned configured remote upload authority issuer evidence is invalid.";
    private const string IssuerEvidenceInvalidCode =
        "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_ISSUER_EVIDENCE_INVALID";

    private static readonly ConditionalWeakTable<
        DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority,
        MintRecord> Mints = new();

    private DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority()
    {
    }

    internal static OperationResult<DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>
        Issue(
            IDesignConversionPromotionUploader uploader,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority legalTeacherAdmissionAuthority,
            string routeBindingFingerprintSha256,
            string? issuerEvidenceRef)
    {
        if (uploader is null)
        {
            return OperationResult<DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>
                .Fail(
                    "DesignConversion server-owned configured remote upload authority requires an uploader.",
                    "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_UPLOADER_REQUIRED");
        }

        if (legalTeacherAdmissionAuthority is null ||
            !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return OperationResult<DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>
                .Fail(
                    "DesignConversion server-owned configured remote upload authority requires a live server-owned legal teacher authority.",
                    "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_LEGAL_REQUIRED");
        }

        if (!IsSha256Ref(routeBindingFingerprintSha256))
        {
            return OperationResult<DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>
                .Fail(
                    "DesignConversion server-owned configured remote upload authority route binding fingerprint is invalid.",
                    "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_ROUTE_BINDING_INVALID");
        }

        if (uploader is not IDesignConversionBoundedPromotionUploader bounded ||
            bounded.MaximumSerializedReceiptUtf8Bytes <= 0 ||
            bounded.MaximumSerializedReceiptUtf8Bytes > 64 * 1024 ||
            uploader is not IDesignConversionPromotionUploaderPreflight preflight)
        {
            return OperationResult<DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>
                .Fail(
                    "DesignConversion server-owned configured remote upload authority requires bounded receipt and mutation-free preflight contracts.",
                    "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_BOUND_REQUIRED");
        }

        var fingerprint = preflight.PreflightConfigurationFingerprintSha256();
        if (!fingerprint.IsSuccess)
        {
            return fingerprint.MapError<
                DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>();
        }

        if (!IsSha256Ref(fingerprint.Value))
        {
            return OperationResult<DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>
                .Fail(
                    "DesignConversion server-owned configured remote upload authority preflight fingerprint is invalid.",
                    "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_FINGERPRINT_INVALID");
        }

        var issuerEvidence = ValidateIssuerEvidenceRef(issuerEvidenceRef);
        if (!issuerEvidence.IsSuccess)
        {
            return issuerEvidence.MapError<
                DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>();
        }

        var typeIdentity = BuildUploaderTypeIdentity(uploader.GetType());
        var stableFingerprint = BuildStableAuthorityFingerprint(
            typeIdentity,
            bounded.MaximumSerializedReceiptUtf8Bytes,
            fingerprint.Value!,
            routeBindingFingerprintSha256,
            issuerEvidence.Value!);
        var authority = new DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority();
        Mints.Add(
            authority,
            new MintRecord(
                uploader,
                legalTeacherAdmissionAuthority,
                typeIdentity,
                bounded.MaximumSerializedReceiptUtf8Bytes,
                fingerprint.Value!,
                routeBindingFingerprintSha256,
                stableFingerprint,
                issuerEvidence.Value!));
        return OperationResult<DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthority>
            .Ok(authority);
    }

    internal OperationResult<DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthorityValidation>
        Validate(
            IDesignConversionPromotionUploader uploader,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority? legalTeacherAdmissionAuthority,
            string currentPreflightFingerprintSha256,
            int currentMaximumSerializedReceiptUtf8Bytes)
    {
        if (!Mints.TryGetValue(this, out var mint) ||
            legalTeacherAdmissionAuthority is null ||
            !legalTeacherAdmissionAuthority.IsAvailable)
        {
            return Invalid();
        }

        if (!ReferenceEquals(mint.Uploader, uploader) ||
            !ReferenceEquals(mint.LegalTeacherAdmissionAuthority, legalTeacherAdmissionAuthority) ||
            !string.Equals(
                mint.UploaderTypeIdentity,
                BuildUploaderTypeIdentity(uploader.GetType()),
                StringComparison.Ordinal) ||
            mint.MaximumSerializedReceiptUtf8Bytes !=
            currentMaximumSerializedReceiptUtf8Bytes ||
            !string.Equals(
                mint.PreflightFingerprintSha256,
                currentPreflightFingerprintSha256,
                StringComparison.Ordinal))
        {
            return Invalid();
        }

        return OperationResult<
            DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthorityValidation>
            .Ok(new DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthorityValidation(
                mint.StableAuthorityFingerprintSha256,
                mint.MaximumSerializedReceiptUtf8Bytes,
                mint.RouteBindingFingerprintSha256,
                mint.IssuerEvidenceRef));

        static OperationResult<
            DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthorityValidation>
            Invalid()
            => OperationResult<
                DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthorityValidation>
                .Fail(
                    "DesignConversion server-owned configured remote upload authority capability is invalid for the configured uploader.",
                    "DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_UPLOAD_AUTHORITY_INVALID");
    }

    private static string BuildStableAuthorityFingerprint(
        string uploaderTypeIdentity,
        int maximumSerializedReceiptUtf8Bytes,
        string preflightFingerprintSha256,
        string routeBindingFingerprintSha256,
        string issuerEvidenceRef)
        => $"sha256:{BuildSha256Hex(string.Join("|",
        [
            SchemaVersion,
            Mode,
            uploaderTypeIdentity,
            maximumSerializedReceiptUtf8Bytes.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            preflightFingerprintSha256,
            routeBindingFingerprintSha256,
            issuerEvidenceRef
        ]))}";

    private static string BuildUploaderTypeIdentity(Type type)
    {
        var assembly = type.Assembly.GetName();
        return string.Join("|",
        [
            type.FullName ?? type.Name,
            assembly.Name ?? string.Empty,
            assembly.Version?.ToString() ?? string.Empty
        ]);
    }

    private static OperationResult<string> ValidateIssuerEvidenceRef(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > MaximumIssuerEvidenceRefChars ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Any(character =>
                char.IsWhiteSpace(character) || char.IsControl(character)) ||
            !value.StartsWith("DESIGN_CONVERSION_", StringComparison.Ordinal) ||
            !value.All(character =>
                character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_'))
        {
            return OperationResult<string>.Fail(
                IssuerEvidenceInvalidMessage,
                IssuerEvidenceInvalidCode);
        }

        return OperationResult<string>.Ok(value);
    }

    private static bool IsSha256Ref(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.Length == "sha256:".Length + 64 &&
           value["sha256:".Length..].All(Uri.IsHexDigit);

    private static string BuildSha256Hex(string value)
        => Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private sealed record MintRecord(
        IDesignConversionPromotionUploader Uploader,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority LegalTeacherAdmissionAuthority,
        string UploaderTypeIdentity,
        int MaximumSerializedReceiptUtf8Bytes,
        string PreflightFingerprintSha256,
        string RouteBindingFingerprintSha256,
        string StableAuthorityFingerprintSha256,
        string IssuerEvidenceRef);
}

internal sealed record
    DesignConversionServerOwnedConfiguredRemotePromotionUploadAuthorityValidation(
        string StableAuthorityFingerprintSha256,
        int MaximumSerializedReceiptUtf8Bytes,
        string RouteBindingFingerprintSha256,
        string IssuerEvidenceRef);

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
    private enum PublicationAuthority
    {
        Legacy,
        ValidatedEvaluatedDpo
    }

    private readonly IDesignConversionPromotionUploader? _uploader;
    private readonly DesignConversionManifestPublisher _manifestPublisher;
    private readonly DesignConversionPromotionGateEvaluator _gateEvaluator;
    internal Action<DesignConversionPromotionPublicationKind>? BeforeTerminalReturnForTesting { get; init; }

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
        => await PublishCoreAsync(
            machine,
            pipelineResult,
            manifestId,
            artifacts,
            gateReport,
            PublicationAuthority.Legacy,
            authoritativeNonMeasurementFailures: [],
            authoritativeDatasetFailures: null,
            evaluatedPublicationLease: null,
            uploadAuthorityMode: DesignConversionPromotionUploadAuthorityMode.Unrestricted,
            ct);

    internal async Task<OperationResult<DesignConversionPromotionPublicationResult>> PublishEvaluatedDpoAsync(
        DesignConversionMachine machine,
        DesignConversionTrainingRunResult completedRunnerResult,
        string manifestId,
        DesignConversionDpoPromotionArtifactPublication artifactPublication,
        DesignConversionPromotionGateReport nonEvaluationGateReport,
        CancellationToken ct = default)
        => await PublishEvaluatedDpoAsync(
            machine,
            completedRunnerResult,
            manifestId,
            artifactPublication,
            nonEvaluationGateReport,
            DesignConversionPromotionUploadAuthorityMode.Unrestricted,
            ct);

    internal async Task<OperationResult<DesignConversionPromotionPublicationResult>> PublishEvaluatedDpoAsync(
        DesignConversionMachine machine,
        DesignConversionTrainingRunResult completedRunnerResult,
        string manifestId,
        DesignConversionDpoPromotionArtifactPublication artifactPublication,
        DesignConversionPromotionGateReport nonEvaluationGateReport,
        DesignConversionPromotionUploadAuthorityMode uploadAuthorityMode,
        CancellationToken ct = default)
    {
        if (nonEvaluationGateReport is null)
        {
            return OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidSafeMessage,
                DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidErrorCode);
        }

        var leaseResult = DesignConversionDpoPromotionArtifactContractCodec
            .AcquireValidatedEvaluatedArtifactPublication(artifactPublication);
        if (!leaseResult.IsSuccess)
            return leaseResult.MapError<DesignConversionPromotionPublicationResult>();
        using var evaluatedPublicationLease = leaseResult.Value!;
        artifactPublication = evaluatedPublicationLease.Publication;

        if (completedRunnerResult is null || completedRunnerResult.PipelineResult is null)
        {
            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                    "DesignConversion training pipeline result is required.",
                    "DESIGN_CONVERSION_PIPELINE_RESULT_REQUIRED"),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
        }

        var evaluation = artifactPublication.PolicyCheckpointContract.Evaluation!;
        var artifacts = artifactPublication.ManifestArtifacts;
        var artifactPaths = BuildEvaluatedArtifactPaths(artifacts);
        var evaluatedGateReport = nonEvaluationGateReport with
        {
            HeldOutScore = evaluation.Accuracy,
            HeldOutSourceDisjoint = evaluation.SourceDisjoint &&
                                    evaluation.HeldOutNotTrained,
            FreshLoadPassed = evaluation.FreshLoadVerified &&
                              evaluation.ReferenceFrozen,
            RequiredArtifactPaths = artifactPaths,
            PresentArtifactPaths = artifactPaths.Where(File.Exists).ToArray(),
            EvidenceRefs = (nonEvaluationGateReport.EvidenceRefs ?? [])
                .Append("DESIGN_CONVERSION_PROMOTION_GATE_FROM_VALIDATED_EVALUATED_DPO_CONTRACT")
                .Concat(BuildArtifactEvidence(artifacts))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
        var authoritativeNonMeasurementFailures =
            (completedRunnerResult.PipelineResult.LifecycleEvidence.FailureReasons ?? [])
                .Where(reason => !IsCallerEvaluationOwnedDimension(reason))
                .Concat(EvaluatedDpoRealTrainingRunClaimed(completedRunnerResult, evaluation)
                    ? []
                    : ["real_training_not_claimed"])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
        var authoritativeDatasetFailures = BuildEvaluatedDatasetFailures(
            completedRunnerResult.PipelineResult.DatasetCountGate,
            evaluation);
        return await PublishCoreAsync(
            machine,
            completedRunnerResult.PipelineResult,
            manifestId,
            artifacts,
            evaluatedGateReport,
            PublicationAuthority.ValidatedEvaluatedDpo,
            authoritativeNonMeasurementFailures,
            authoritativeDatasetFailures,
            evaluatedPublicationLease,
            uploadAuthorityMode,
            ct);
    }

    private static bool EvaluatedDpoRealTrainingRunClaimed(
        DesignConversionTrainingRunResult completedRunnerResult,
        DesignConversionDpoHeldOutEvaluation evaluation)
    {
        var pipeline = completedRunnerResult.PipelineResult;
        var lifecycle = pipeline.LifecycleEvidence;
        return pipeline.MachineLifecycleHookAttempted &&
               lifecycle.LocalLifecycleBatchResultCount > 0 &&
               lifecycle.CheckpointContainsRealTrainedWeights &&
               evaluation.FreshLoadVerified &&
               evaluation.ReferenceFrozen &&
               evaluation.SourceDisjoint &&
               evaluation.HeldOutNotTrained;
    }

    private async Task<OperationResult<DesignConversionPromotionPublicationResult>> PublishCoreAsync(
        DesignConversionMachine machine,
        DesignConversionTrainingPipelineResult pipelineResult,
        string manifestId,
        DesignConversionTrainingManifestArtifacts artifacts,
        DesignConversionPromotionGateReport gateReport,
        PublicationAuthority authority,
        IReadOnlyList<string> authoritativeNonMeasurementFailures,
        IReadOnlyList<string>? authoritativeDatasetFailures,
        DesignConversionDpoValidatedArtifactPublicationLease? evaluatedPublicationLease,
        DesignConversionPromotionUploadAuthorityMode uploadAuthorityMode,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                    "DesignConversion promotion cancelled.",
                    "CANCELLED"),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);

        if (artifacts is null)
            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                    "DesignConversion promotion artifacts are required.",
                    "DESIGN_CONVERSION_PROMOTION_ARTIFACTS_REQUIRED"),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);

        if (pipelineResult is null)
            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                    "DesignConversion training pipeline result is required.",
                    "DESIGN_CONVERSION_PIPELINE_RESULT_REQUIRED"),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);

        var evaluatedDecision = _gateEvaluator.Decide(gateReport);
        var datasetFailures = authority == PublicationAuthority.ValidatedEvaluatedDpo
            ? authoritativeDatasetFailures ?? ["DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_UNCLASSIFIED"]
            : pipelineResult.DatasetCountGate.Passed
                ? []
                : pipelineResult.DatasetCountGate.Reasons
                    .Prepend("DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_NOT_PASSED")
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
        var decision = datasetFailures.Count == 0
            ? evaluatedDecision
            : new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                datasetFailures,
                evaluatedDecision.EvidenceRefs
                    .Concat(pipelineResult.BoundaryDecision.EvidenceRefs)
                    .Append("DESIGN_CONVERSION_PROMOTION_REJECTED_BY_DATASET_COUNT_GATE")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());

        if (decision.AllowsUpload &&
            authority == PublicationAuthority.ValidatedEvaluatedDpo &&
            authoritativeNonMeasurementFailures.Count > 0)
        {
            decision = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                authoritativeNonMeasurementFailures,
                decision.EvidenceRefs
                    .Concat(pipelineResult.LifecycleEvidence.EvidenceRefs)
                    .Where(reference => !IsCallerEvaluationOwnedDimension(reference))
                    .Append("DESIGN_CONVERSION_EVALUATED_DPO_PROMOTION_BLOCKED_BY_NON_MEASUREMENT_FAILURE")
                    .Append("MODEL_SERVER_UPLOAD_NOT_ATTEMPTED")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());
        }

        if (decision.AllowsUpload && authority == PublicationAuthority.Legacy && !MeasurementGatePassed(pipelineResult))
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

        if (decision.AllowsUpload && authority == PublicationAuthority.Legacy && !SourceDisjointHeldOutReadinessGatePassed(pipelineResult))
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

        if (decision.AllowsUpload && authority == PublicationAuthority.Legacy && !pipelineResult.LifecycleEvidence.PromotionUploadAllowed)
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

        var manifestResult = decision.AllowsUpload
            ? await _manifestPublisher.PublishManifestMetadataAsync(
                machine,
                pipelineResult,
                manifestId,
                ct)
            : _manifestPublisher.BuildNonUploadManifestMetadata(
                pipelineResult,
                manifestId,
                artifacts,
                ["DESIGN_CONVERSION_PROMOTION_UPLOAD_BLOCKED_BEFORE_MACHINE_EXPORT"]);

        if (!manifestResult.IsSuccess)
            return CompleteTerminal(
                manifestResult.MapError<DesignConversionPromotionPublicationResult>(),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);

        var manifestEvidenceRefs = manifestResult.Value!.EvidenceRefs
            .Concat(decision.EvidenceRefs)
            .Concat(BuildArtifactEvidence(artifacts));

        var manifest = manifestResult.Value! with
        {
            PromotionArtifacts = artifacts,
            EvidenceRefs = SanitizePublicationEvidence(authority, manifestEvidenceRefs)
        };

        if (!decision.AllowsUpload)
        {
            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                    new DesignConversionPromotionPublicationResult(decision, manifest, null)
                    {
                        PublicationKind = DesignConversionPromotionPublicationKind.Rejected,
                        PublicationStatus = DesignConversionPromotionPublicationStatus.Rejected
                    }),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
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

            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                    new DesignConversionPromotionPublicationResult(rejected, manifest, null)
                    {
                        PublicationKind = DesignConversionPromotionPublicationKind.Rejected,
                        PublicationStatus = DesignConversionPromotionPublicationStatus.Rejected
                    }),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
        }

        if (evaluatedPublicationLease is not null &&
            !evaluatedPublicationLease.Revalidate())
        {
            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                    DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidSafeMessage,
                    DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidErrorCode),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
        }
        var upload = await _uploader.UploadAsync(manifest, gateReport, ct);
        if (evaluatedPublicationLease is not null &&
            !evaluatedPublicationLease.Revalidate())
        {
            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                    DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidSafeMessage,
                    DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidErrorCode),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
        }
        if (!upload.IsSuccess)
        {
            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                    "DesignConversion promotion uploader failed.",
                    SafeErrorCode(upload.ErrorCode)),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
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
                EvidenceRefs = SanitizePublicationEvidence(
                    authority,
                    manifest.EvidenceRefs.Concat(rejected.EvidenceRefs))
            };

            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                    new DesignConversionPromotionPublicationResult(rejected, rejectedManifest, null)
                    {
                        PublicationKind = DesignConversionPromotionPublicationKind.Rejected,
                        PublicationStatus = DesignConversionPromotionPublicationStatus.Rejected
                    }),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
        }

        var receipt = receiptValidation.Receipt!;
        if (uploadAuthorityMode ==
            DesignConversionPromotionUploadAuthorityMode.LocalReceiptOnly &&
            receiptValidation.PublicationKind !=
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt)
        {
            var rejected = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                ["DESIGN_CONVERSION_LOCAL_RECEIPT_ONLY_AUTHORITY_REJECTED_REMOTE_RECEIPT"],
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
                EvidenceRefs = SanitizePublicationEvidence(
                    authority,
                    manifest.EvidenceRefs.Concat(rejected.EvidenceRefs))
            };

            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                    new DesignConversionPromotionPublicationResult(
                        rejected,
                        rejectedManifest,
                        null)
                    {
                        PublicationKind =
                            DesignConversionPromotionPublicationKind.Rejected,
                        PublicationStatus =
                            DesignConversionPromotionPublicationStatus.Rejected
                    }),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
        }

        if (uploadAuthorityMode ==
            DesignConversionPromotionUploadAuthorityMode.ServerOwnedConfiguredRemote &&
            receiptValidation.PublicationKind ==
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt)
        {
            var rejected = new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Reject,
                ["DESIGN_CONVERSION_SERVER_OWNED_CONFIGURED_REMOTE_AUTHORITY_REJECTED_LOCAL_RECEIPT"],
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
                EvidenceRefs = SanitizePublicationEvidence(
                    authority,
                    manifest.EvidenceRefs.Concat(rejected.EvidenceRefs))
            };

            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                    new DesignConversionPromotionPublicationResult(
                        rejected,
                        rejectedManifest,
                        null)
                    {
                        PublicationKind =
                            DesignConversionPromotionPublicationKind.Rejected,
                        PublicationStatus =
                            DesignConversionPromotionPublicationStatus.Rejected
                    }),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.Rejected);
        }

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
                EvidenceRefs = SanitizePublicationEvidence(
                    authority,
                    manifest.EvidenceRefs
                        .Concat(receipt.EvidenceRefs)
                        .Concat(BuildLocalReceiptEvidenceRefs()))
            };

            return CompleteTerminal(
                OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                    new DesignConversionPromotionPublicationResult(decision, uploadedManifest, receipt)),
                evaluatedPublicationLease,
                DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt);
        }

        var remoteManifest = manifest with
        {
            ModelServerUploadAttempted = true,
            ModelServerPromotionClaimed = true,
            LocalPromotionReceiptWritten = false,
            PromotionReceiptKind = "model_server_upload_verified",
            PromotionUploadRef = receipt.RemoteRef,
            PromotionUploadedAtUtc = receipt.UploadedAtUtc,
            EvidenceRefs = SanitizePublicationEvidence(
                authority,
                manifest.EvidenceRefs
                    .Where(reference => !IsStalePreUploadEvidenceForVerifiedRemote(reference))
                    .Concat(receipt.EvidenceRefs)
                    .Concat(BuildVerifiedRemoteReceiptEvidenceRefs(receipt)))
        };

        return CompleteTerminal(
            OperationResult<DesignConversionPromotionPublicationResult>.Ok(
                new DesignConversionPromotionPublicationResult(decision, remoteManifest, receipt)
                {
                    PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
                    PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified
                }),
            evaluatedPublicationLease,
            DesignConversionPromotionPublicationKind.VerifiedModelServerUpload);
    }

    private OperationResult<DesignConversionPromotionPublicationResult> CompleteTerminal(
        OperationResult<DesignConversionPromotionPublicationResult> result,
        DesignConversionDpoValidatedArtifactPublicationLease? evaluatedPublicationLease,
        DesignConversionPromotionPublicationKind publicationKind)
    {
        if (evaluatedPublicationLease is null)
            return result;

        BeforeTerminalReturnForTesting?.Invoke(publicationKind);
        return evaluatedPublicationLease.Revalidate()
            ? result
            : OperationResult<DesignConversionPromotionPublicationResult>.Fail(
                DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidSafeMessage,
                DesignConversionDpoPromotionArtifactContractCodec.ContractInvalidErrorCode);
    }

    internal static IReadOnlyList<string> BuildEvaluatedDatasetFailures(
        DesignConversionDpoDatasetCountGate gate,
        DesignConversionDpoHeldOutEvaluation evaluation)
    {
        var failures = new List<string>();
        if (evaluation.TrainRecordCount < Math.Max(1, gate.RequiredTrainRecords))
        {
            failures.Add(
                $"TRAIN_RECORD_COUNT_BELOW_TARGET:{evaluation.TrainRecordCount}<{Math.Max(1, gate.RequiredTrainRecords)}");
        }
        if (evaluation.HeldOutRecordCount < Math.Max(1, gate.RequiredHeldOutRecords))
        {
            failures.Add(
                $"EVALUATED_HELD_OUT_RECORD_COUNT_BELOW_TARGET:{evaluation.HeldOutRecordCount}<{Math.Max(1, gate.RequiredHeldOutRecords)}");
        }

        var reasons = (gate.Reasons ?? [])
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .ToArray();
        foreach (var reason in reasons)
        {
            if (string.Equals(reason, "COUNT_GATE_PASSED", StringComparison.Ordinal))
            {
                if (!gate.Passed)
                    failures.Add("DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_INCONSISTENT");
                continue;
            }
            if (!DesignConversionCallerEvidenceClassifier.IsPureEvaluationProjection(reason))
                failures.Add(reason);
        }
        if (!gate.Passed && reasons.Length == 0)
            failures.Add("DESIGN_CONVERSION_DPO_DATASET_COUNT_GATE_UNCLASSIFIED");

        return failures
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsCallerEvaluationOwnedDimension(string value)
        => DesignConversionCallerEvidenceClassifier.IsPureEvaluationProjection(value) ||
           DesignConversionCallerEvidenceClassifier.IsCallerMeasurementAdvisory(value);

    private static IReadOnlyList<string> SanitizePublicationEvidence(
        PublicationAuthority authority,
        IEnumerable<string> evidenceRefs)
    {
        var effective = evidenceRefs
            .Where(reference => !string.IsNullOrWhiteSpace(reference));
        if (authority == PublicationAuthority.ValidatedEvaluatedDpo)
        {
            effective = effective
                .Where(reference => !IsCallerEvaluationOwnedDimension(reference))
                .Append("DESIGN_CONVERSION_CALLER_EVALUATION_HISTORY_ADVISORY_ONLY");
        }

        return effective
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
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
               // MeasurementGate.Passed выше несёт весь контракт качества:
               // повторное сравнение score >= threshold здесь обошло бы шумомер.
               lifecycle.LocalContinueTrainingProbe?.HeldOutRecordUsageCount == 0 &&
               ContainsSourceDisjointHeldOutReadinessGateEvidence(measurementGate.EvidenceRefs) &&
               ContainsSourceDisjointHeldOutReadinessGateEvidence(lifecycle.EvidenceRefs);
    }

    private static bool ContainsSourceDisjointHeldOutReadinessGateEvidence(IReadOnlyList<string>? evidenceRefs)
        => evidenceRefs?.Contains(
            DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGatePassedEvidence,
            StringComparer.Ordinal) == true;

    private static IReadOnlyList<string> BuildArtifactEvidence(DesignConversionTrainingManifestArtifacts artifacts)
    {
        var evidence = new List<string>
        {
            $"CHECKPOINT_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-checkpoint-path", artifacts.CheckpointPath)}",
            $"TOKENIZER_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-tokenizer-path", artifacts.TokenizerPath)}",
            $"POLICY_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-policy-path", artifacts.PolicyPath)}"
        };
        if (!string.IsNullOrWhiteSpace(artifacts.SourceEncoderPath))
        {
            evidence.Add(
                $"SOURCE_ENCODER_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-source-encoder-path", artifacts.SourceEncoderPath)}");
        }
        if (!string.IsNullOrWhiteSpace(artifacts.CanonicalBpeTokenizerPath))
        {
            evidence.Add(
                $"CANONICAL_BPE_TOKENIZER_PATH_REF:{DesignConversionDpoLeakageGuard.HashRef("design-conversion-promotion-canonical-bpe-tokenizer-path", artifacts.CanonicalBpeTokenizerPath)}");
        }

        return evidence;
    }

    private static IReadOnlyList<string> BuildEvaluatedArtifactPaths(
        DesignConversionTrainingManifestArtifacts artifacts)
    {
        var paths = new List<string>
        {
            artifacts.CheckpointPath,
            artifacts.TokenizerPath,
            artifacts.PolicyPath
        };
        if (!string.IsNullOrWhiteSpace(artifacts.SourceEncoderPath))
            paths.Add(artifacts.SourceEncoderPath);
        if (!string.IsNullOrWhiteSpace(artifacts.CanonicalBpeTokenizerPath))
            paths.Add(artifacts.CanonicalBpeTokenizerPath);
        return paths;
    }
}
