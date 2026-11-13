// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

public sealed record ArtifactAccessPolicyReadRequest(
    string CorrelationId,
    string TenantId,
    string JobId,
    string ArtifactRefId,
    string CallerRole,
    DateTimeOffset? PresentedBrokeredGrantExpiresAtUtc = null);

public sealed record ArtifactAccessPolicyStoredArtifact(
    string TenantId,
    string JobId,
    string ArtifactRefId,
    string Purpose,
    string Classification,
    string Status,
    DateTimeOffset RetentionExpiresAtUtc,
    string StorageRef,
    string ContentSha256);

public sealed record ArtifactAccessPolicyReadGrant(
    string BrokeredAccessRef,
    DateTimeOffset ExpiresAtUtc,
    string TenantId,
    string JobId,
    string ArtifactRefId,
    string Role,
    string Purpose,
    string Classification,
    string Scope,
    string RedactedStorageRefHash);

public sealed record ArtifactAccessPolicyAuditEvent(
    string CorrelationId,
    string TenantId,
    string JobId,
    string ArtifactRefId,
    string Role,
    string Decision,
    string Reason,
    DateTimeOffset? ExpiresAtUtc,
    string RedactedStorageRefHash);

public sealed class ArtifactAccessPolicy
{
    public const string TenantUserRole = "tenant-user";
    public const string ArchitectRole = "architect";
    public const string AdminRole = "admin";
    public const string LocalTrainingRole = "local-training";
    public const string DiagnosticRole = "diagnostic";

    public const string ResultSafePurpose = "result-safe";
    public const string EvidencePurpose = "evidence";
    public const string TrainingPurpose = "training";
    public const string ProviderPrivatePurpose = "provider-private";
    public const string PrivatePurpose = "private";
    public const string RawPurpose = "raw";

    public const string ResultSafeClassification = "result-safe";
    public const string EvidenceClassification = "evidence";
    public const string TrainingClassification = "training";
    public const string ProviderPrivateClassification = "provider-private";
    public const string PrivateClassification = "private";
    public const string RawClassification = "raw";

    public const string ActiveStatus = "active";
    public const string ReadableStatus = "readable";
    public const string AllowReason = "ALLOW";
    public const string RequestRequiredCode = "ARTIFACT_ACCESS_REQUEST_REQUIRED";
    public const string StoredArtifactRequiredCode = "ARTIFACT_ACCESS_STORED_ARTIFACT_REQUIRED";
    public const string CorrelationIdInvalidCode = "ARTIFACT_ACCESS_CORRELATION_ID_INVALID";
    public const string TenantIdInvalidCode = "ARTIFACT_ACCESS_TENANT_ID_INVALID";
    public const string JobIdInvalidCode = "ARTIFACT_ACCESS_JOB_ID_INVALID";
    public const string ArtifactRefIdInvalidCode = "ARTIFACT_ACCESS_REF_ID_INVALID";
    public const string CallerRoleInvalidCode = "ARTIFACT_ACCESS_CALLER_ROLE_INVALID";
    public const string StoredArtifactRefInvalidCode = "ARTIFACT_ACCESS_STORED_REF_ID_INVALID";
    public const string PurposeInvalidCode = "ARTIFACT_ACCESS_PURPOSE_INVALID";
    public const string ClassificationInvalidCode = "ARTIFACT_ACCESS_CLASSIFICATION_INVALID";
    public const string StatusInvalidCode = "ARTIFACT_ACCESS_STATUS_INVALID";
    public const string TenantMismatchCode = "ARTIFACT_ACCESS_TENANT_MISMATCH";
    public const string JobMismatchCode = "ARTIFACT_ACCESS_JOB_MISMATCH";
    public const string ArtifactRefMismatchCode = "ARTIFACT_ACCESS_REF_MISMATCH";
    public const string RoleDeniedCode = "ARTIFACT_ACCESS_ROLE_DENIED";
    public const string StatusNotReadableCode = "ARTIFACT_ACCESS_STATUS_NOT_READABLE";
    public const string RetentionExpiredCode = "ARTIFACT_ACCESS_RETENTION_EXPIRED";
    public const string BrokeredGrantExpiredCode = "ARTIFACT_ACCESS_BROKERED_GRANT_EXPIRED";
    public const string StorageRefUnsafeCode = "ARTIFACT_ACCESS_STORAGE_REF_UNSAFE";
    public const string ContentHashInvalidCode = "ARTIFACT_ACCESS_CONTENT_HASH_INVALID";

    private const string BrokeredAccessPrefix = "brokered-artifact-read:";
    private const string StorageRefPrefix = "artifact-store/";
    private const string Sha256Prefix = "sha256:";
    private const string RedactedAuditScalarPrefix = "redacted-";
    private const int MaxLogicalTokenLength = 96;
    private const int MaxStorageRefLength = 256;
    private const int BrokeredAccessSigningSecretBytes = 32;
    private const int BrokeredAccessNonceBytes = 16;
    private const int RedactedAuditScalarHashChars = 16;
    private static readonly TimeSpan DefaultBrokeredGrantLifetime = TimeSpan.FromMinutes(15);
    private readonly HashSet<string> _diagnosticReadablePurposes;
    private readonly TimeSpan _brokeredGrantLifetime;
    private readonly byte[] _brokeredAccessSigningSecret;
    private readonly Func<byte[]> _brokeredAccessNonceProvider;

    public ArtifactAccessPolicy(
        TimeSpan? brokeredGrantLifetime = null,
        IEnumerable<string>? diagnosticReadablePurposes = null)
        : this(
            brokeredGrantLifetime,
            diagnosticReadablePurposes,
            CreateBrokeredAccessSigningSecret(),
            CreateBrokeredAccessNonce)
    {
    }

    internal ArtifactAccessPolicy(
        TimeSpan? brokeredGrantLifetime,
        IEnumerable<string>? diagnosticReadablePurposes,
        byte[] brokeredAccessSigningSecret,
        Func<byte[]> brokeredAccessNonceProvider)
    {
        _brokeredGrantLifetime = brokeredGrantLifetime ?? DefaultBrokeredGrantLifetime;
        if (_brokeredGrantLifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(brokeredGrantLifetime), "Artifact access brokered grant lifetime must be positive.");

        _diagnosticReadablePurposes = new HashSet<string>(
            diagnosticReadablePurposes ?? Array.Empty<string>(),
            StringComparer.Ordinal);
        _brokeredAccessSigningSecret = CopyBrokeredAccessSigningSecret(brokeredAccessSigningSecret);
        _brokeredAccessNonceProvider = brokeredAccessNonceProvider ??
                                       throw new ArgumentNullException(nameof(brokeredAccessNonceProvider));
    }

    public OperationResult<ArtifactAccessPolicyReadGrant> TryAuthorizeRead(
        ArtifactAccessPolicyReadRequest? request,
        ArtifactAccessPolicyStoredArtifact? storedArtifact,
        out ArtifactAccessPolicyAuditEvent audit,
        DateTimeOffset? nowUtc = null)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        var redactedStorageHash = RedactedStorageRefHash(storedArtifact?.StorageRef);

        if (request is null)
            return Deny(null, storedArtifact, RequestRequiredCode, redactedStorageHash, out audit);
        if (storedArtifact is null)
            return Deny(request, null, StoredArtifactRequiredCode, redactedStorageHash, out audit);

        if (!IsCanonicalLogicalToken(request.CorrelationId))
            return Deny(request, storedArtifact, CorrelationIdInvalidCode, redactedStorageHash, out audit);
        if (!IsCanonicalLogicalToken(request.TenantId))
            return Deny(request, storedArtifact, TenantIdInvalidCode, redactedStorageHash, out audit);
        if (!IsCanonicalLogicalToken(request.JobId))
            return Deny(request, storedArtifact, JobIdInvalidCode, redactedStorageHash, out audit);
        if (!IsCanonicalLogicalToken(request.ArtifactRefId))
            return Deny(request, storedArtifact, ArtifactRefIdInvalidCode, redactedStorageHash, out audit);
        if (!IsCanonicalRole(request.CallerRole))
            return Deny(request, storedArtifact, CallerRoleInvalidCode, redactedStorageHash, out audit);
        if (!IsCanonicalLogicalToken(storedArtifact.ArtifactRefId))
            return Deny(request, storedArtifact, StoredArtifactRefInvalidCode, redactedStorageHash, out audit);
        if (!IsCanonicalPolicyLabel(storedArtifact.Purpose))
            return Deny(request, storedArtifact, PurposeInvalidCode, redactedStorageHash, out audit);
        if (!IsCanonicalPolicyLabel(storedArtifact.Classification))
            return Deny(request, storedArtifact, ClassificationInvalidCode, redactedStorageHash, out audit);
        if (!IsCanonicalPolicyLabel(storedArtifact.Status))
            return Deny(request, storedArtifact, StatusInvalidCode, redactedStorageHash, out audit);

        if (!string.Equals(request.TenantId, storedArtifact.TenantId, StringComparison.Ordinal))
            return Deny(request, storedArtifact, TenantMismatchCode, redactedStorageHash, out audit);
        if (!string.Equals(request.JobId, storedArtifact.JobId, StringComparison.Ordinal))
            return Deny(request, storedArtifact, JobMismatchCode, redactedStorageHash, out audit);
        if (!string.Equals(request.ArtifactRefId, storedArtifact.ArtifactRefId, StringComparison.Ordinal))
            return Deny(request, storedArtifact, ArtifactRefMismatchCode, redactedStorageHash, out audit);
        if (!IsRoleAllowed(request.CallerRole, storedArtifact.Purpose, storedArtifact.Classification))
            return Deny(request, storedArtifact, RoleDeniedCode, redactedStorageHash, out audit);
        if (!IsReadableStatus(storedArtifact.Status))
            return Deny(request, storedArtifact, StatusNotReadableCode, redactedStorageHash, out audit);
        if (storedArtifact.RetentionExpiresAtUtc <= now)
            return Deny(request, storedArtifact, RetentionExpiredCode, redactedStorageHash, out audit);
        if (request.PresentedBrokeredGrantExpiresAtUtc is not null &&
            request.PresentedBrokeredGrantExpiresAtUtc.Value <= now)
        {
            return Deny(request, storedArtifact, BrokeredGrantExpiredCode, redactedStorageHash, out audit);
        }

        if (!IsSafeStorageRef(storedArtifact.StorageRef))
            return Deny(request, storedArtifact, StorageRefUnsafeCode, redactedStorageHash, out audit);
        if (!IsCanonicalSha256(storedArtifact.ContentSha256))
            return Deny(request, storedArtifact, ContentHashInvalidCode, redactedStorageHash, out audit);

        var expiresAt = Min(now.Add(_brokeredGrantLifetime), storedArtifact.RetentionExpiresAtUtc);
        var grant = new ArtifactAccessPolicyReadGrant(
            BrokeredAccessRef: BuildBrokeredAccessRef(request, storedArtifact, expiresAt),
            ExpiresAtUtc: expiresAt,
            TenantId: request.TenantId,
            JobId: request.JobId,
            ArtifactRefId: request.ArtifactRefId,
            Role: request.CallerRole,
            Purpose: storedArtifact.Purpose,
            Classification: storedArtifact.Classification,
            Scope: $"{request.TenantId}/{request.JobId}/{request.ArtifactRefId}:read",
            RedactedStorageRefHash: redactedStorageHash);

        audit = new ArtifactAccessPolicyAuditEvent(
            CorrelationId: request.CorrelationId,
            TenantId: request.TenantId,
            JobId: request.JobId,
            ArtifactRefId: request.ArtifactRefId,
            Role: request.CallerRole,
            Decision: "allow",
            Reason: AllowReason,
            ExpiresAtUtc: expiresAt,
            RedactedStorageRefHash: redactedStorageHash);

        return OperationResult<ArtifactAccessPolicyReadGrant>.Ok(grant);
    }

    private OperationResult<ArtifactAccessPolicyReadGrant> Deny(
        ArtifactAccessPolicyReadRequest? request,
        ArtifactAccessPolicyStoredArtifact? storedArtifact,
        string reason,
        string redactedStorageHash,
        out ArtifactAccessPolicyAuditEvent audit)
    {
        audit = new ArtifactAccessPolicyAuditEvent(
            CorrelationId: SanitizeAuditLogicalToken(request?.CorrelationId),
            TenantId: SanitizeAuditLogicalToken(request?.TenantId ?? storedArtifact?.TenantId),
            JobId: SanitizeAuditLogicalToken(request?.JobId ?? storedArtifact?.JobId),
            ArtifactRefId: SanitizeAuditLogicalToken(request?.ArtifactRefId ?? storedArtifact?.ArtifactRefId),
            Role: SanitizeAuditRole(request?.CallerRole),
            Decision: "deny",
            Reason: reason,
            ExpiresAtUtc: null,
            RedactedStorageRefHash: redactedStorageHash);

        return OperationResult<ArtifactAccessPolicyReadGrant>.Fail(
            "Artifact read grant denied by server-side access policy.",
            reason);
    }

    private bool IsRoleAllowed(string role, string purpose, string classification)
    {
        if (IsProviderPrivateRaw(purpose) || IsProviderPrivateRaw(classification))
        {
            return string.Equals(role, DiagnosticRole, StringComparison.Ordinal) &&
                   _diagnosticReadablePurposes.Contains(purpose);
        }

        if (string.Equals(role, TenantUserRole, StringComparison.Ordinal))
            return IsResultSafe(purpose, classification);

        if (string.Equals(role, ArchitectRole, StringComparison.Ordinal) ||
            string.Equals(role, AdminRole, StringComparison.Ordinal) ||
            string.Equals(role, LocalTrainingRole, StringComparison.Ordinal))
        {
            return IsResultSafe(purpose, classification) ||
                   IsEvidenceOrTraining(purpose, classification);
        }

        return false;
    }

    private static bool IsResultSafe(string purpose, string classification)
        => string.Equals(purpose, ResultSafePurpose, StringComparison.Ordinal) &&
           string.Equals(classification, ResultSafeClassification, StringComparison.Ordinal);

    private static bool IsEvidenceOrTraining(string purpose, string classification)
        => IsEvidenceOrTrainingLabel(purpose) && IsElevatedReadableClassification(classification);

    private static bool IsEvidenceOrTrainingLabel(string value)
        => string.Equals(value, EvidencePurpose, StringComparison.Ordinal) ||
           string.Equals(value, TrainingPurpose, StringComparison.Ordinal);

    private static bool IsElevatedReadableClassification(string value)
        => string.Equals(value, EvidenceClassification, StringComparison.Ordinal) ||
           string.Equals(value, TrainingClassification, StringComparison.Ordinal) ||
           string.Equals(value, ResultSafeClassification, StringComparison.Ordinal);

    private static bool IsProviderPrivateRaw(string value)
        => string.Equals(value, ProviderPrivatePurpose, StringComparison.Ordinal) ||
           string.Equals(value, ProviderPrivateClassification, StringComparison.Ordinal) ||
           string.Equals(value, PrivatePurpose, StringComparison.Ordinal) ||
           string.Equals(value, PrivateClassification, StringComparison.Ordinal) ||
           string.Equals(value, RawPurpose, StringComparison.Ordinal) ||
           string.Equals(value, RawClassification, StringComparison.Ordinal);

    private static bool IsReadableStatus(string status)
        => string.Equals(status, ActiveStatus, StringComparison.Ordinal) ||
           string.Equals(status, ReadableStatus, StringComparison.Ordinal);

    private static bool IsCanonicalRole(string value)
        => string.Equals(value, TenantUserRole, StringComparison.Ordinal) ||
           string.Equals(value, ArchitectRole, StringComparison.Ordinal) ||
           string.Equals(value, AdminRole, StringComparison.Ordinal) ||
           string.Equals(value, LocalTrainingRole, StringComparison.Ordinal) ||
           string.Equals(value, DiagnosticRole, StringComparison.Ordinal);

    private static bool IsCanonicalPolicyLabel(string value)
        => IsCanonicalLogicalToken(value);

    private static bool IsCanonicalLogicalToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > MaxLogicalTokenLength ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        foreach (var character in value)
        {
            if (character is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-' or '_')
                continue;

            return false;
        }

        return true;
    }

    private static bool IsSafeStorageRef(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > MaxStorageRefLength ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Any(static character => char.IsWhiteSpace(character) || char.IsControl(character)))
        {
            return false;
        }

        var decoded = DecodeRepeatedly(value);
        if (decoded is null)
            return false;

        foreach (var candidate in new[] { value, decoded })
        {
            if (candidate.Contains('\\', StringComparison.Ordinal) ||
                candidate.Contains(':', StringComparison.Ordinal) ||
                candidate.Contains("://", StringComparison.Ordinal) ||
                candidate.StartsWith("/", StringComparison.Ordinal) ||
                candidate.StartsWith("\\", StringComparison.Ordinal) ||
                candidate.StartsWith("//", StringComparison.Ordinal) ||
                ContainsTraversalSegment(candidate) ||
                LooksLikeDrivePath(candidate) ||
                Path.IsPathFullyQualified(candidate))
            {
                return false;
            }
        }

        if (!string.Equals(value, decoded, StringComparison.Ordinal))
            return false;
        if (!value.StartsWith(StorageRefPrefix, StringComparison.Ordinal) || value.EndsWith("/", StringComparison.Ordinal))
            return false;

        var segments = value.Split('/');
        if (segments.Length < 4)
            return false;

        foreach (var segment in segments)
        {
            if (!IsSafeStorageSegment(segment))
                return false;
        }

        return true;
    }

    private static string? DecodeRepeatedly(string value)
    {
        var current = value;
        for (var index = 0; index < 3; index++)
        {
            try
            {
                var decoded = Uri.UnescapeDataString(current);
                if (string.Equals(decoded, current, StringComparison.Ordinal))
                    return decoded;

                current = decoded;
            }
            catch (UriFormatException)
            {
                return null;
            }
        }

        return current;
    }

    private static bool ContainsTraversalSegment(string value)
        => value.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Any(static segment => segment.Equals("..", StringComparison.Ordinal));

    private static bool LooksLikeDrivePath(string value)
        => value.Length >= 2 &&
           value[1] == ':' &&
           value[0] is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');

    private static bool IsSafeStorageSegment(string value)
    {
        if (string.IsNullOrEmpty(value) ||
            value is "." or ".." ||
            value.Length > MaxLogicalTokenLength)
        {
            return false;
        }

        foreach (var character in value)
        {
            if (character is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-' or '_' or '.')
                continue;

            return false;
        }

        return true;
    }

    private static bool IsCanonicalSha256(string? value)
    {
        if (value is null ||
            value.Length != Sha256Prefix.Length + 64 ||
            !value.StartsWith(Sha256Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        for (var index = Sha256Prefix.Length; index < value.Length; index++)
        {
            var character = value[index];
            if (character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f'))
                return false;
        }

        return true;
    }

    private string BuildBrokeredAccessRef(
        ArtifactAccessPolicyReadRequest request,
        ArtifactAccessPolicyStoredArtifact storedArtifact,
        DateTimeOffset expiresAt)
    {
        var nonce = _brokeredAccessNonceProvider() ??
                    throw new InvalidOperationException("Artifact access brokered grant nonce material is required.");
        if (nonce.Length < BrokeredAccessNonceBytes)
            throw new InvalidOperationException("Artifact access brokered grant nonce material is too short.");

        var nonceText = HexText(nonce);
        var material = string.Join(
            "\n",
            "artifact-access-policy-v1",
            nonceText,
            request.CorrelationId,
            request.TenantId,
            request.JobId,
            request.ArtifactRefId,
            request.CallerRole,
            storedArtifact.Purpose,
            storedArtifact.Classification,
            storedArtifact.ContentSha256,
            RedactedStorageRefHash(storedArtifact.StorageRef),
            expiresAt.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture));
        return BrokeredAccessPrefix + nonceText + "." + HmacSha256Text(_brokeredAccessSigningSecret, material);
    }

    private static string RedactedStorageRefHash(string? storageRef)
        => Sha256Prefix + Sha256Text(storageRef ?? string.Empty);

    private static string Sha256Text(string value)
        => HexText(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string HmacSha256Text(byte[] secret, string value)
        => HexText(HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes(value)));

    private static string HexText(byte[] value)
        => Convert.ToHexString(value).ToLowerInvariant();

    private static byte[] CreateBrokeredAccessSigningSecret()
        => RandomNumberGenerator.GetBytes(BrokeredAccessSigningSecretBytes);

    private static byte[] CreateBrokeredAccessNonce()
        => RandomNumberGenerator.GetBytes(BrokeredAccessNonceBytes);

    private static byte[] CopyBrokeredAccessSigningSecret(byte[] brokeredAccessSigningSecret)
    {
        if (brokeredAccessSigningSecret is null)
            throw new ArgumentNullException(nameof(brokeredAccessSigningSecret));
        if (brokeredAccessSigningSecret.Length < BrokeredAccessSigningSecretBytes)
            throw new ArgumentException("Artifact access brokered grant signing secret is too short.", nameof(brokeredAccessSigningSecret));

        return brokeredAccessSigningSecret.ToArray();
    }

    private static string SanitizeAuditLogicalToken(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return IsCanonicalLogicalToken(value) && !ContainsSensitiveAuditMarker(value)
            ? value
            : RedactAuditScalar(value);
    }

    private static string SanitizeAuditRole(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return IsCanonicalRole(value) && !ContainsSensitiveAuditMarker(value)
            ? value
            : RedactAuditScalar(value);
    }

    private static bool ContainsSensitiveAuditMarker(string value)
    {
        var normalized = value.ToLowerInvariant();
        return normalized.Contains("secret", StringComparison.Ordinal) ||
               normalized.Contains("token", StringComparison.Ordinal) ||
               normalized.Contains("password", StringComparison.Ordinal) ||
               normalized.Contains("passwd", StringComparison.Ordinal) ||
               normalized.Contains("credential", StringComparison.Ordinal) ||
               normalized.Contains("api-key", StringComparison.Ordinal) ||
               normalized.Contains("apikey", StringComparison.Ordinal) ||
               normalized.Contains("bearer", StringComparison.Ordinal) ||
               normalized.Contains("private", StringComparison.Ordinal);
    }

    private static string RedactAuditScalar(string value)
        => RedactedAuditScalarPrefix + Sha256Text(value)[..RedactedAuditScalarHashChars];

    private static DateTimeOffset Min(DateTimeOffset left, DateTimeOffset right)
        => left <= right ? left : right;
}
