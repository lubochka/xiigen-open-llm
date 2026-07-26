// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Security.Cryptography;
using System.Security;
using System.Text;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerModelActivationAuthorization(
    string TenantId,
    string RequestedByRef,
    string Reason,
    string IdempotencyKey,
    long ExpectedGeneration,
    string? LocalAuthorizationToken = null);

public sealed record DesignConversionModelServerModelActivationReadAuthorization(
    string TenantId,
    string? LocalAuthorizationToken = null);

public sealed class DesignConversionModelServerModelLifecycleTokenKeyRingOptions
{
    public DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
        string? currentToken = null,
        string? previousToken = null,
        DateTimeOffset? previousTokenExpiresAtUtc = null)
    {
        CurrentToken = currentToken;
        PreviousToken = previousToken;
        PreviousTokenExpiresAtUtc = previousTokenExpiresAtUtc;
    }

    public string? CurrentToken { get; }

    public string? PreviousToken { get; }

    public DateTimeOffset? PreviousTokenExpiresAtUtc { get; }
}

public sealed class DesignConversionModelServerModelLifecycleTokenKeyRing
{
    public const int MinimumTokenLength = 16;
    private const int MaximumTokenLength = 4096;

    private readonly byte[] _currentTokenHash;
    private readonly byte[]? _previousTokenHash;
    private readonly DateTimeOffset? _previousTokenExpiresAtUtc;
    private readonly Func<DateTimeOffset> _utcNow;

    private DesignConversionModelServerModelLifecycleTokenKeyRing(
        byte[] currentTokenHash,
        byte[]? previousTokenHash,
        DateTimeOffset? previousTokenExpiresAtUtc,
        Func<DateTimeOffset>? utcNow)
    {
        _currentTokenHash = currentTokenHash;
        _previousTokenHash = previousTokenHash;
        _previousTokenExpiresAtUtc = previousTokenExpiresAtUtc;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public static OperationResult<DesignConversionModelServerModelLifecycleTokenKeyRing> Create(
        string? currentToken,
        string? previousToken = null,
        DateTimeOffset? previousTokenExpiresAtUtc = null,
        Func<DateTimeOffset>? utcNow = null)
        => Create(
            new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                currentToken,
                previousToken,
                previousTokenExpiresAtUtc),
            utcNow);

    public static OperationResult<DesignConversionModelServerModelLifecycleTokenKeyRing> Create(
        DesignConversionModelServerModelLifecycleTokenKeyRingOptions? options,
        Func<DateTimeOffset>? utcNow = null)
    {
        if (options is null)
        {
            return OperationResult<DesignConversionModelServerModelLifecycleTokenKeyRing>.Fail(
                "DesignConversion model lifecycle token key ring requires a current token.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_REQUIRED");
        }

        var current = BuildConfiguredTokenHash(
            options.CurrentToken,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_REQUIRED",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_INVALID");
        if (!current.IsSuccess)
            return current.MapError<DesignConversionModelServerModelLifecycleTokenKeyRing>();

        byte[]? previousTokenHash = null;
        var hasPreviousToken = !string.IsNullOrWhiteSpace(options.PreviousToken);
        if (!hasPreviousToken && options.PreviousTokenExpiresAtUtc is not null)
        {
            return OperationResult<DesignConversionModelServerModelLifecycleTokenKeyRing>.Fail(
                "DesignConversion model lifecycle previous-token expiration requires a previous token.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_REQUIRED");
        }

        if (hasPreviousToken)
        {
            if (options.PreviousTokenExpiresAtUtc is null)
            {
                return OperationResult<DesignConversionModelServerModelLifecycleTokenKeyRing>.Fail(
                    "DesignConversion model lifecycle previous token requires an explicit expiration.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_EXPIRATION_REQUIRED");
            }

            var previous = BuildConfiguredTokenHash(
                options.PreviousToken,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_REQUIRED",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_PREVIOUS_INVALID");
            if (!previous.IsSuccess)
                return previous.MapError<DesignConversionModelServerModelLifecycleTokenKeyRing>();

            if (CryptographicOperations.FixedTimeEquals(current.Value!, previous.Value!))
            {
                CryptographicOperations.ZeroMemory(previous.Value!);
                return OperationResult<DesignConversionModelServerModelLifecycleTokenKeyRing>.Fail(
                    "DesignConversion model lifecycle current and previous tokens must be distinct.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_CURRENT_PREVIOUS_MATCH");
            }

            previousTokenHash = previous.Value!;
        }

        return OperationResult<DesignConversionModelServerModelLifecycleTokenKeyRing>.Ok(
            new DesignConversionModelServerModelLifecycleTokenKeyRing(
                current.Value!,
                previousTokenHash,
                options.PreviousTokenExpiresAtUtc,
                utcNow));
    }

    public bool IsAccepted(string? suppliedToken)
    {
        if (string.IsNullOrWhiteSpace(suppliedToken))
            return false;

        var normalized = suppliedToken.Trim();
        if (normalized.Length > MaximumTokenLength)
            return false;

        var suppliedHash = HashToken(normalized);
        try
        {
            var currentMatched = CryptographicOperations.FixedTimeEquals(suppliedHash, _currentTokenHash);
            var previousMatched = _previousTokenHash is not null &&
                                  CryptographicOperations.FixedTimeEquals(suppliedHash, _previousTokenHash);
            var previousWithinGrace = _previousTokenHash is not null &&
                                      _previousTokenExpiresAtUtc is not null &&
                                      _utcNow() < _previousTokenExpiresAtUtc.Value;

            return currentMatched | (previousMatched & previousWithinGrace);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(suppliedHash);
        }
    }

    private static OperationResult<byte[]> BuildConfiguredTokenHash(
        string? token,
        string requiredErrorCode,
        string invalidErrorCode)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return OperationResult<byte[]>.Fail(
                "DesignConversion model lifecycle token is required.",
                requiredErrorCode);
        }

        var normalized = token.Trim();
        if (normalized.Length < MinimumTokenLength ||
            normalized.Length > MaximumTokenLength ||
            normalized.Any(char.IsControl))
        {
            return OperationResult<byte[]>.Fail(
                "DesignConversion model lifecycle token configuration is invalid.",
                invalidErrorCode);
        }

        return OperationResult<byte[]>.Ok(HashToken(normalized));
    }

    private static byte[] HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        try
        {
            return SHA256.HashData(bytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
        }
    }
}

public sealed class DesignConversionModelServerModelActivationApi
{
    public const string InternalLifecycleBoundary =
        "internal_modelserver_model_lifecycle_requires_tenant_id_idempotency_key_and_optional_local_token_no_public_anonymous_route";
    public const string LocalAuthorizationHeaderName = "x-design-conversion-model-lifecycle-token";

    private readonly DesignConversionModelActivationRegistry _registry;
    private readonly DesignConversionModelServerModelLifecycleTokenKeyRing? _localAuthorizationTokens;
    private readonly IDesignConversionLocalPromotionActivationAuthority? _localPromotionAuthority;

    public DesignConversionModelServerModelActivationApi(
        DesignConversionModelActivationRegistry registry,
        string? expectedLocalAuthorizationToken = null,
        IDesignConversionLocalPromotionActivationAuthority? localPromotionAuthority = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _localPromotionAuthority = localPromotionAuthority;
        if (!string.IsNullOrWhiteSpace(expectedLocalAuthorizationToken))
        {
            var keyRing = DesignConversionModelServerModelLifecycleTokenKeyRing.Create(
                expectedLocalAuthorizationToken);
            if (!keyRing.IsSuccess)
            {
                throw new ArgumentException(
                    "DesignConversion model lifecycle local authorization token configuration is invalid.",
                    nameof(expectedLocalAuthorizationToken));
            }

            _localAuthorizationTokens = keyRing.Value!;
        }
    }

    public DesignConversionModelServerModelActivationApi(
        DesignConversionModelActivationRegistry registry,
        DesignConversionModelServerModelLifecycleTokenKeyRing localAuthorizationTokens,
        IDesignConversionLocalPromotionActivationAuthority? localPromotionAuthority = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _localPromotionAuthority = localPromotionAuthority;
        _localAuthorizationTokens = localAuthorizationTokens ??
                                    throw new ArgumentNullException(nameof(localAuthorizationTokens));
    }

    public async Task<OperationResult<DesignConversionModelActivationResult>> ActivatePublishedModelAsync(
        DesignConversionModelServerModelActivationAuthorization authorization,
        DesignConversionPromotedModelActivationRecord candidate,
        string? expectedModelVersion = null,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default)
    {
        var auth = ValidateAuthorization(authorization);
        if (!auth.IsSuccess)
            return auth.MapError<DesignConversionModelActivationResult>();

        var durableCandidate = await ResolveDurablePromotionActivationRecordAsync(
            authorization,
            candidate,
            expectedModelVersion,
            ct);
        if (!durableCandidate.IsSuccess)
            return durableCandidate.MapError<DesignConversionModelActivationResult>();

        var result = await _registry.ActivateAsync(
            new DesignConversionModelActivationCommand(
                authorization.TenantId,
                authorization.RequestedByRef,
                authorization.Reason,
                authorization.IdempotencyKey,
                authorization.ExpectedGeneration,
                durableCandidate.Value!,
                expectedModelVersion,
                requestedAtUtc),
            ct);

        return Annotate(result, durablePromotionAuthorityVerified: true);
    }

    public async Task<OperationResult<DesignConversionModelActivationResult>> ReadActiveModelAsync(
        DesignConversionModelServerModelActivationReadAuthorization authorization,
        CancellationToken ct = default)
    {
        if (authorization is null)
        {
            return OperationResult<DesignConversionModelActivationResult>.Fail(
                "DesignConversion model activation read authorization is required.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTHORIZATION_REQUIRED");
        }

        var auth = ValidateLocalAuthorizationToken(authorization.LocalAuthorizationToken);
        if (!auth.IsSuccess)
            return auth.MapError<DesignConversionModelActivationResult>();

        var result = await _registry.ReadActiveAsync(authorization.TenantId, ct);
        return Annotate(result);
    }

    public async Task<OperationResult<DesignConversionModelActivationResult>> RollbackActiveModelAsync(
        DesignConversionModelServerModelActivationAuthorization authorization,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default)
    {
        var auth = ValidateAuthorization(authorization);
        if (!auth.IsSuccess)
            return auth.MapError<DesignConversionModelActivationResult>();

        var result = await _registry.RollbackAsync(
            new DesignConversionModelRollbackCommand(
                authorization.TenantId,
                authorization.RequestedByRef,
                authorization.Reason,
                authorization.IdempotencyKey,
                authorization.ExpectedGeneration,
                requestedAtUtc),
            ct);

        return Annotate(result);
    }

    private OperationResult<bool> ValidateAuthorization(
        DesignConversionModelServerModelActivationAuthorization authorization)
    {
        if (authorization is null)
            return OperationResult<bool>.Fail(
                "DesignConversion model activation authorization is required.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTHORIZATION_REQUIRED");

        return ValidateLocalAuthorizationToken(authorization.LocalAuthorizationToken);
    }

    private OperationResult<bool> ValidateLocalAuthorizationToken(string? localAuthorizationToken)
    {
        if (_localAuthorizationTokens is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model lifecycle local authorization token is not configured.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_NOT_CONFIGURED");
        }

        if (string.IsNullOrWhiteSpace(localAuthorizationToken))
        {
            return OperationResult<bool>.Fail(
                $"DesignConversion model lifecycle operation requires local authorization token {LocalAuthorizationHeaderName}.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_REQUIRED");
        }

        if (!_localAuthorizationTokens.IsAccepted(localAuthorizationToken))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model lifecycle operation rejected by local authorization token boundary.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID");
        }

        return OperationResult<bool>.Ok(true);
    }

    private async Task<OperationResult<DesignConversionPromotedModelActivationRecord>> ResolveDurablePromotionActivationRecordAsync(
        DesignConversionModelServerModelActivationAuthorization authorization,
        DesignConversionPromotedModelActivationRecord candidate,
        string? expectedModelVersion,
        CancellationToken ct)
    {
        if (_localPromotionAuthority is null)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation requires the server-owned local promotion authority.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_NOT_CONFIGURED");
        }

        var tenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(authorization.TenantId);
        if (!tenant.IsSuccess)
            return tenant.MapError<DesignConversionPromotedModelActivationRecord>();
        var tenantIdRef = tenant.Value!.TenantIdRef;

        if (candidate is null)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation requires a promoted model activation record.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_RECORD_REQUIRED");
        }

        if (!string.Equals(candidate.TenantIdRef, tenantIdRef, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation tenant does not match the published activation record tenant.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_SCOPE_MISMATCH");
        }

        var candidateShape = ValidateCandidateShape(candidate);
        if (!candidateShape.IsSuccess)
            return candidateShape.MapError<DesignConversionPromotedModelActivationRecord>();
        if (!string.IsNullOrWhiteSpace(expectedModelVersion) &&
            !string.Equals(expectedModelVersion, candidate.Model.Version, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation expected model version does not match the promoted model version.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_VERSION_SCOPE_MISMATCH");
        }

        var records = await _localPromotionAuthority.ListTrainingIntakeLocalPromotionRecordsAsync(ct);
        if (!records.IsSuccess)
        {
            if (string.Equals(records.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                return records.MapError<DesignConversionPromotedModelActivationRecord>();

            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation could not read the server-owned local promotion authority.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_READ_FAILED");
        }

        var durableCandidates = records.Value!
            .Where(IsReadyPromotionAuthorityRecord)
            .Where(record => MatchesClaimedPromotion(record, candidate))
            .ToArray();
        if (durableCandidates.Length == 0)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation could not find a matching server-owned local promotion record.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");
        }

        foreach (var durableRecord in durableCandidates.Reverse())
        {
            var binding = ValidateDurablePromotionBinding(durableRecord, candidate, tenantIdRef, expectedModelVersion);
            if (!binding.IsSuccess)
                continue;

            return OperationResult<DesignConversionPromotedModelActivationRecord>.Ok(
                new DesignConversionPromotedModelActivationRecord(
                    tenantIdRef,
                    durableRecord.Publication!,
                    durableRecord.PromotedLocalModel!));
        }

        return DurablePromotionMismatch().MapError<DesignConversionPromotedModelActivationRecord>();
    }

    private static bool IsReadyPromotionAuthorityRecord(DesignConversionTrainingIntakeLocalPromotionResult record)
        => record is
           {
               LocalPromotionReady: true,
               Publication: not null,
               PromotedLocalModel: not null
           } &&
           HasActivationSafeDurablePromotionShape(record) &&
           !string.Equals(
               ReadMetadata(record.BoundaryMetadata, "local_promotion_record_kind"),
               "rollback",
               StringComparison.Ordinal);

    private static bool HasActivationSafeDurablePromotionShape(
        DesignConversionTrainingIntakeLocalPromotionResult record)
    {
        var publication = record.Publication;
        var model = record.PromotedLocalModel;
        var artifacts = publication?.Manifest?.PromotionArtifacts;
        if (publication?.Decision?.EvidenceRefs is null ||
            publication.Manifest?.EvidenceRefs is null ||
            publication.UploadReceipt?.EvidenceRefs is null ||
            model?.EvidenceRefs is null ||
            artifacts is null)
        {
            return false;
        }

        if (!model.EvidenceRefs.Contains(
                DesignConversionPromotedLocalModelLoader.LocalPromotionBoundary,
                StringComparer.Ordinal))
        {
            return false;
        }

        return HasUsableLocalPath(model.CheckpointPath) &&
               HasUsableLocalPath(artifacts.CheckpointPath) &&
               HasUsableLocalPath(artifacts.TokenizerPath) &&
               HasUsableLocalPath(artifacts.PolicyPath);
    }

    private static bool MatchesClaimedPromotion(
        DesignConversionTrainingIntakeLocalPromotionResult durableRecord,
        DesignConversionPromotedModelActivationRecord candidate)
    {
        var durable = durableRecord.PromotedLocalModel;
        var publication = durableRecord.Publication;
        if (durable is null ||
            publication?.Manifest?.PromotionArtifacts is null ||
            publication.UploadReceipt is null ||
            candidate.Model is null ||
            candidate.Publication?.Manifest?.PromotionArtifacts is null)
        {
            return false;
        }

        var durableRefs = new HashSet<string>(StringComparer.Ordinal)
        {
            durable.ModelId,
            durable.ManifestId,
            durable.ManifestRef,
            durable.CheckpointSha256,
            publication.Manifest.PromotionUploadRef ?? string.Empty,
            publication.UploadReceipt?.RemoteRef ?? string.Empty
        };
        AddPathRef(durableRefs, durable.CheckpointPath);
        AddPathRef(durableRefs, publication.Manifest.PromotionArtifacts?.CheckpointPath);

        var candidateRefs = new[]
        {
            candidate.Model.ModelId,
            candidate.Model.ManifestId,
            candidate.Model.ManifestRef,
            candidate.Model.CheckpointSha256,
            candidate.Publication.Manifest.PromotionUploadRef,
            candidate.Publication.UploadReceipt?.RemoteRef,
            NormalizePathRef(candidate.Model.CheckpointPath),
            NormalizePathRef(candidate.Publication.Manifest.PromotionArtifacts?.CheckpointPath)
        };

        return candidateRefs.Any(reference =>
            !string.IsNullOrWhiteSpace(reference) &&
            durableRefs.Contains(reference));
    }

    private static OperationResult<bool> ValidateDurablePromotionBinding(
        DesignConversionTrainingIntakeLocalPromotionResult durableRecord,
        DesignConversionPromotedModelActivationRecord candidate,
        string tenantIdRef,
        string? expectedModelVersion)
    {
        var durableTenantRef = ReadMetadata(durableRecord.BoundaryMetadata, "training_intake_tenant_id_ref");
        if (string.IsNullOrWhiteSpace(durableTenantRef))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation durable local promotion record has no tenant binding.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_TENANT_BINDING_REQUIRED");
        }

        if (!string.Equals(durableTenantRef, tenantIdRef, StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation durable local promotion tenant does not match activation tenant.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_TENANT_SCOPE_MISMATCH");
        }

        var durable = durableRecord.PromotedLocalModel!;
        var durablePublication = durableRecord.Publication!;

        if (!Same(candidate.Model.ModelId, durable.ModelId) ||
            !Same(candidate.Model.ManifestId, durable.ManifestId) ||
            !Same(candidate.Model.ManifestRef, durable.ManifestRef) ||
            !Same(candidate.Model.CheckpointSha256, durable.CheckpointSha256) ||
            !Same(candidate.Model.MachineId, durable.MachineId) ||
            !Same(candidate.Model.DomainTag, durable.DomainTag) ||
            !Same(candidate.Model.Version, durable.Version) ||
            candidate.Model.LocalOnly != durable.LocalOnly ||
            candidate.Model.CheckpointFreshLoadVerified != durable.CheckpointFreshLoadVerified ||
            candidate.Model.CheckpointContainsRealTrainedWeights != durable.CheckpointContainsRealTrainedWeights ||
            candidate.Model.PromotionManifestClaimed != durable.PromotionManifestClaimed)
        {
            return DurablePromotionMismatch();
        }

        if (!SamePath(candidate.Model.CheckpointPath, durable.CheckpointPath))
            return DurablePromotionMismatch();

        var candidatePublication = candidate.Publication;
        if (candidatePublication.Decision.Kind != durablePublication.Decision.Kind ||
            candidatePublication.Decision.AllowsUpload != durablePublication.Decision.AllowsUpload)
        {
            return DurablePromotionMismatch();
        }

        var candidateManifest = candidatePublication.Manifest;
        var durableManifest = durablePublication.Manifest;
        if (!Same(candidateManifest.ManifestId, durableManifest.ManifestId) ||
            !Same(candidateManifest.ManifestRef, durableManifest.ManifestRef) ||
            !Same(candidateManifest.MachineId, durableManifest.MachineId) ||
            !Same(candidateManifest.DomainTag, durableManifest.DomainTag) ||
            !Same(candidateManifest.Version, durableManifest.Version) ||
            !Same(candidateManifest.PromotionUploadRef, durableManifest.PromotionUploadRef) ||
            candidateManifest.LocalPromotionReceiptWritten != durableManifest.LocalPromotionReceiptWritten ||
            !Same(candidateManifest.PromotionReceiptKind, durableManifest.PromotionReceiptKind) ||
            candidateManifest.ModelServerUploadAttempted != durableManifest.ModelServerUploadAttempted ||
            candidateManifest.ModelServerPromotionClaimed != durableManifest.ModelServerPromotionClaimed)
        {
            return DurablePromotionMismatch();
        }

        if (!SamePath(candidateManifest.PromotionArtifacts?.CheckpointPath, durableManifest.PromotionArtifacts?.CheckpointPath) ||
            !SamePath(candidateManifest.PromotionArtifacts?.TokenizerPath, durableManifest.PromotionArtifacts?.TokenizerPath) ||
            !SamePath(candidateManifest.PromotionArtifacts?.PolicyPath, durableManifest.PromotionArtifacts?.PolicyPath))
        {
            return DurablePromotionMismatch();
        }

        if (!Same(candidatePublication.UploadReceipt?.UploadId, durablePublication.UploadReceipt?.UploadId) ||
            !Same(candidatePublication.UploadReceipt?.RemoteRef, durablePublication.UploadReceipt?.RemoteRef))
        {
            return DurablePromotionMismatch();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateCandidateShape(DesignConversionPromotedModelActivationRecord candidate)
    {
        if (candidate.Publication is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation requires a promotion publication result.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PUBLICATION_REQUIRED");
        }

        if (candidate.Model is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation requires promoted local model metadata.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTED_MODEL_REQUIRED");
        }

        if (candidate.Publication.Decision is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation requires a promote decision.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTE_DECISION_REQUIRED");
        }

        if (candidate.Publication.Manifest is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation requires a published manifest.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_MANIFEST_REQUIRED");
        }

        if (candidate.Publication.UploadReceipt is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation rejects unverified or non-local promotion refs.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_REF_UNVERIFIED");
        }

        if (candidate.Publication.Manifest.PromotionArtifacts is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation requires promotion artifact paths from the manifest.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACTS_REQUIRED");
        }

        if (candidate.Publication.Decision.EvidenceRefs is null ||
            candidate.Publication.Manifest.EvidenceRefs is null ||
            candidate.Publication.UploadReceipt.EvidenceRefs is null ||
            candidate.Model.EvidenceRefs is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation requires promoted-local-model boundary evidence.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_BOUNDARY_EVIDENCE_REQUIRED");
        }

        if (!candidate.Model.EvidenceRefs.Contains(
                DesignConversionPromotedLocalModelLoader.LocalPromotionBoundary,
                StringComparer.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation requires promoted-local-model boundary evidence.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_BOUNDARY_EVIDENCE_REQUIRED");
        }

        if (!HasUsableLocalPath(candidate.Model.CheckpointPath))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation checkpoint path is invalid.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_INVALID");
        }

        if (!HasUsableLocalPath(candidate.Publication.Manifest.PromotionArtifacts.CheckpointPath) ||
            !HasUsableLocalPath(candidate.Publication.Manifest.PromotionArtifacts.TokenizerPath) ||
            !HasUsableLocalPath(candidate.Publication.Manifest.PromotionArtifacts.PolicyPath))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation requires promotion artifact paths from the manifest.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACTS_REQUIRED");
        }

        if (!IsLocalPromotionRef(candidate.Publication.Manifest.PromotionUploadRef) ||
            !IsLocalPromotionRef(candidate.Publication.UploadReceipt.RemoteRef) ||
            !Same(candidate.Publication.Manifest.PromotionUploadRef, candidate.Publication.UploadReceipt.RemoteRef))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation rejects unverified or non-local promotion refs.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_REF_UNVERIFIED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> DurablePromotionMismatch()
        => OperationResult<bool>.Fail(
            "DesignConversion model activation claimed promotion record does not match the server-owned durable local promotion record.",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_DURABLE_PROMOTION_BINDING_MISMATCH");

    private static bool Same(string? left, string? right)
        => string.Equals(left ?? string.Empty, right ?? string.Empty, StringComparison.Ordinal);

    private static bool IsLocalPromotionRef(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Trim().StartsWith(DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix, StringComparison.Ordinal);

    private static bool SamePath(string? left, string? right)
    {
        var normalizedLeft = NormalizePathRef(left);
        var normalizedRight = NormalizePathRef(right);
        return !string.IsNullOrWhiteSpace(normalizedLeft) &&
               string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasUsableLocalPath(string? path)
        => TryNormalizePathRef(path, out _);

    private static void AddPathRef(HashSet<string> refs, string? path)
    {
        var normalized = NormalizePathRef(path);
        if (!string.IsNullOrWhiteSpace(normalized))
            refs.Add(normalized);
    }

    private static string NormalizePathRef(string? path)
        => TryNormalizePathRef(path, out var normalized)
            ? normalized
            : string.Empty;

    private static bool TryNormalizePathRef(string? path, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            normalized = Path.GetFullPath(path);
            return !string.IsNullOrWhiteSpace(normalized);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
        catch (SecurityException)
        {
            return false;
        }
    }

    private static string ReadMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value
            : string.Empty;

    private static OperationResult<DesignConversionModelActivationResult> Annotate(
        OperationResult<DesignConversionModelActivationResult> result,
        bool durablePromotionAuthorityVerified = false)
    {
        if (!result.IsSuccess)
            return result;

        var value = result.Value!;
        var metadata = new Dictionary<string, string>(value.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["modelserver_model_lifecycle_boundary"] = InternalLifecycleBoundary,
            ["modelserver_model_lifecycle_internal_only"] = bool.TrueString,
            ["modelserver_model_lifecycle_public_http_route_registered"] = bool.FalseString,
            ["modelserver_model_lifecycle_requires_tenant_id"] = bool.TrueString,
            ["modelserver_model_lifecycle_requires_idempotency_key"] = bool.TrueString,
            ["modelserver_model_lifecycle_auth_header_name"] = LocalAuthorizationHeaderName,
            ["product_ready"] = bool.FalseString
        };
        if (durablePromotionAuthorityVerified)
        {
            metadata["modelserver_model_lifecycle_durable_promotion_authority_verified"] = bool.TrueString;
            metadata["modelserver_model_lifecycle_durable_promotion_authority"] =
                nameof(IDesignConversionLocalPromotionActivationAuthority);
        }

        return OperationResult<DesignConversionModelActivationResult>.Ok(value with
        {
            BoundaryMetadata = metadata,
            EvidenceRefs = value.EvidenceRefs
                .Append("DESIGN_CONVERSION_MODEL_SERVER_MODEL_LIFECYCLE_INTERNAL_AUTH_BOUNDARY")
                .Concat(durablePromotionAuthorityVerified
                    ? ["DESIGN_CONVERSION_MODEL_ACTIVATION_DURABLE_LOCAL_PROMOTION_AUTHORITY_VERIFIED"]
                    : [])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        });
    }
}
