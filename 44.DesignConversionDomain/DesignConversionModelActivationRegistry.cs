// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public sealed record DesignConversionPromotedModelActivationRecord(
    string TenantIdRef,
    DesignConversionPromotionPublicationResult Publication,
    DesignConversionPromotedLocalModel Model);

public sealed record DesignConversionCanonicalTenantIdentity(
    string TenantId,
    string TenantIdRef);

public sealed record DesignConversionModelActivationCommand(
    string TenantId,
    string RequestedByRef,
    string Reason,
    string IdempotencyKey,
    long ExpectedGeneration,
    DesignConversionPromotedModelActivationRecord Candidate,
    string? ExpectedModelVersion = null,
    DateTimeOffset? RequestedAtUtc = null);

public sealed record DesignConversionModelRollbackCommand(
    string TenantId,
    string RequestedByRef,
    string Reason,
    string IdempotencyKey,
    long ExpectedGeneration,
    DateTimeOffset? RequestedAtUtc = null);

public sealed record DesignConversionActivatedModelVersion(
    string ActivationRef,
    string ModelId,
    string ManifestId,
    string ManifestRef,
    string PromotionUploadRef,
    string Version,
    string MachineId,
    string DomainTag,
    string CheckpointPath,
    string CheckpointPathRef,
    string CheckpointSha256,
    bool CheckpointFreshLoadVerified,
    bool CheckpointContainsRealTrainedWeights,
    bool LocalOnly,
    IReadOnlyList<string> EvidenceRefs)
{
    private string? _localCheckpointWeightsSource;

    public string LocalCheckpointWeightsSource
    {
        get => NormalizeLocalCheckpointWeightsSource(
            _localCheckpointWeightsSource,
            EvidenceRefs,
            CheckpointContainsRealTrainedWeights);
        init => _localCheckpointWeightsSource = NormalizeLocalCheckpointWeightsSource(
            value,
            EvidenceRefs,
            CheckpointContainsRealTrainedWeights);
    }

    private static string NormalizeLocalCheckpointWeightsSource(
        string? value,
        IReadOnlyList<string>? evidenceRefs,
        bool checkpointContainsRealTrainedWeights)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();

        return HasDpoTrainerPolicyCheckpointEvidence(evidenceRefs)
            ? "11.DPO/DpoTrainer:dpo_policy_checkpoint_converted"
            : checkpointContainsRealTrainedWeights ? "local_checkpoint_feature_weights" : "none";
    }

    private static bool HasDpoTrainerPolicyCheckpointEvidence(IReadOnlyList<string>? evidenceRefs)
        => evidenceRefs?.Contains(
               DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
               StringComparer.Ordinal) == true ||
           evidenceRefs?.Contains(
               DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
               StringComparer.Ordinal) == true;
}

public sealed record DesignConversionModelActivationResult(
    string OperationKind,
    string OperationKeyRef,
    bool Applied,
    bool Replay,
    long Generation,
    string TenantIdRef,
    bool ProductReady,
    DesignConversionActivatedModelVersion ActiveVersion,
    DesignConversionActivatedModelVersion? PreviousVersion,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelActivationOperationRetentionLimits(
    int RetainedOperationCount,
    int RetainedOperationCountPerTenant);

public sealed class DesignConversionModelActivationRegistry
{
    public const string StoreSchemaVersion = "design-conversion-model-activation-store-v1";
    public const string MutationLockFileName = "design-conversion-model-activation-state.json.mutation.lock";
    public const string InternalActivationBoundary =
        "internal_tenant_scoped_promoted_model_activation_requires_verified_local_publication_and_generation_cas";
    public const int MaximumCanonicalTenantIdLength = 128;
    public const int DefaultRetainedOperationCount = 1024;
    public const int DefaultRetainedOperationCountPerTenant = 128;
    // Activation stores contain small tenant/version metadata; 16 MiB is a conservative fail-closed ceiling.
    public const long MaximumActivationStoreBytes = 16L * 1024 * 1024;

    private const string TenantIdRefScope = "design-conversion-model-activation-tenant";
    private const string EvaluatedDpoPromotionEvidence =
        "DESIGN_CONVERSION_PROMOTION_GATE_FROM_VALIDATED_EVALUATED_DPO_CONTRACT";
    private const int ActivationStoreReadBufferSize = 81920;
    private const int MaximumStableReadAttempts = 2;
    private static readonly TimeSpan MutationLeaseRetryDelay = TimeSpan.FromMilliseconds(25);
    private static readonly UTF8Encoding ActivationStoreEncoding = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly string _statePath;
    private readonly string _mutationLockPath;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly Func<string, Encoding, CancellationToken, Task<string>> _readAllTextAsync;
    private readonly EffectiveOperationRetentionLimits _operationRetentionLimits;
    private readonly string _allowedArtifactRootDirectory;
    private readonly string _allowedArtifactRootDirectoryWithSeparator;
    private readonly StringComparison _pathComparison;
    private readonly Func<string, FileAttributes> _getAttributes;
    private readonly Action? _beforeStorePublicationCallbackForTesting;
    private readonly SemaphoreSlim _gate;
    private ActivationStoreCacheSnapshot? _cachedSnapshot;

    public DesignConversionModelActivationRegistry(
        string rootDirectory,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionModelActivationOperationRetentionLimits? operationRetentionLimits = null,
        string? allowedArtifactRootDirectory = null)
        : this(
            rootDirectory,
            utcNow,
            ReadActivationStoreTextBoundedAsync,
            operationRetentionLimits,
            allowedArtifactRootDirectory: allowedArtifactRootDirectory)
    {
    }

    internal DesignConversionModelActivationRegistry(
        string rootDirectory,
        Func<DateTimeOffset>? utcNow,
        Func<string, Encoding, CancellationToken, Task<string>> readAllTextAsync,
        DesignConversionModelActivationOperationRetentionLimits? operationRetentionLimits = null,
        SemaphoreSlim? gate = null,
        string? allowedArtifactRootDirectory = null,
        Func<string, FileAttributes>? getAttributes = null,
        Action? beforeStorePublicationCallbackForTesting = null)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Activation registry root directory is required.", nameof(rootDirectory));
        ArgumentNullException.ThrowIfNull(readAllTextAsync);

        var rootPath = Path.GetFullPath(rootDirectory);
        _statePath = Path.Combine(rootPath, "design-conversion-model-activation-state.json");
        _mutationLockPath = Path.Combine(rootPath, MutationLockFileName);
        _pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        _allowedArtifactRootDirectory = NormalizeAllowedArtifactRootDirectory(
            allowedArtifactRootDirectory ?? rootDirectory);
        _allowedArtifactRootDirectoryWithSeparator = EnsureTrailingDirectorySeparator(_allowedArtifactRootDirectory);
        _getAttributes = getAttributes ?? File.GetAttributes;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _readAllTextAsync = readAllTextAsync;
        _operationRetentionLimits = ValidateOperationRetentionLimits(operationRetentionLimits);
        _gate = gate ?? new SemaphoreSlim(1, 1);
        _beforeStorePublicationCallbackForTesting = beforeStorePublicationCallbackForTesting;
    }

    public async Task<OperationResult<DesignConversionModelActivationResult>> ActivateAsync(
        DesignConversionModelActivationCommand command,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionModelActivationResult>.Fail(
                "DesignConversion model activation cancelled.",
                "CANCELLED");

        var validated = await ValidateActivationCommandAsync(command, ct);
        if (!validated.IsSuccess)
            return validated.MapError<DesignConversionModelActivationResult>();

        var tenantIdRef = validated.Value!.Tenant.TenantIdRef;
        var operationKeyRef = ToHashRef("design-conversion-model-activation-idempotency-key", command.IdempotencyKey);
        var payloadHash = BuildActivationPayloadHash(command, tenantIdRef, validated.Value.ActiveVersion);

        var mutationLease = await AcquireStoreMutationLeaseAsync(ct);
        if (!mutationLease.IsSuccess)
            return mutationLease.MapError<DesignConversionModelActivationResult>();

        using (mutationLease.Value!)
        {
            var storeResult = await LoadStoreAsync(ct);
            if (!storeResult.IsSuccess)
                return storeResult.MapError<DesignConversionModelActivationResult>();

            var store = storeResult.Value!;
            var replay = ResolveReplay(store, tenantIdRef, operationKeyRef, payloadHash, "activate");
            if (replay is not null)
                return replay;
            if (HasConflictingOperation(store, tenantIdRef, operationKeyRef, payloadHash, out var conflict))
                return conflict.MapError<DesignConversionModelActivationResult>();

            var current = ResolveTenantState(store, tenantIdRef);
            if (current.Generation != command.ExpectedGeneration)
            {
                return OperationResult<DesignConversionModelActivationResult>.Fail(
                    $"DesignConversion model activation generation conflict: expected {command.ExpectedGeneration}, actual {current.Generation}.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_GENERATION_CONFLICT");
            }

            var operationCreatedAtUtc = _utcNow();
            var nextActive = validated.Value.ActiveVersion;
            var next = current with
            {
                TenantIdRef = tenantIdRef,
                Generation = current.Generation + 1,
                ActiveVersion = nextActive,
                PreviousVersion = current.ActiveVersion,
                UpdatedAtUtc = command.RequestedAtUtc ?? operationCreatedAtUtc
            };
            var result = BuildResult(
                "activate",
                operationKeyRef,
                applied: true,
                replay: false,
                next,
                nextActive,
                next.PreviousVersion,
                [
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_APPLIED",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_VERIFIED_PROMOTED_RECORD",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_GENERATION_CAS_PASSED"
                ],
                command);
            UpsertTenant(store, next);
            UpsertOperation(store, new ActivationOperationRecord(
                TenantIdRef: tenantIdRef,
                OperationKeyRef: operationKeyRef,
                OperationKind: "activate",
                PayloadHash: payloadHash,
                Result: result,
                CreatedAtUtc: operationCreatedAtUtc));
            var saved = await SaveStoreAsync(store, ct);
            if (!saved.IsSuccess)
                return saved.MapError<DesignConversionModelActivationResult>();

            return OperationResult<DesignConversionModelActivationResult>.Ok(result);
        }
    }

    public async Task<OperationResult<DesignConversionModelActivationResult>> RollbackAsync(
        DesignConversionModelRollbackCommand command,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionModelActivationResult>.Fail(
                "DesignConversion model rollback cancelled.",
                "CANCELLED");

        var validation = ValidateRollbackCommand(command);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionModelActivationResult>();

        var tenantIdRef = validation.Value!.TenantIdRef;
        var operationKeyRef = ToHashRef("design-conversion-model-activation-idempotency-key", command.IdempotencyKey);

        var mutationLease = await AcquireStoreMutationLeaseAsync(ct);
        if (!mutationLease.IsSuccess)
            return mutationLease.MapError<DesignConversionModelActivationResult>();

        using (mutationLease.Value!)
        {
            var storeResult = await LoadStoreAsync(ct);
            if (!storeResult.IsSuccess)
                return storeResult.MapError<DesignConversionModelActivationResult>();

            var store = storeResult.Value!;
            var current = ResolveTenantState(store, tenantIdRef);
            var payloadHash = BuildRollbackPayloadHash(command, tenantIdRef);
            var replay = ResolveReplay(store, tenantIdRef, operationKeyRef, payloadHash, "rollback");
            if (replay is not null)
                return replay;
            if (HasConflictingOperation(store, tenantIdRef, operationKeyRef, payloadHash, out var conflict))
                return conflict.MapError<DesignConversionModelActivationResult>();

            if (current.Generation != command.ExpectedGeneration)
            {
                return OperationResult<DesignConversionModelActivationResult>.Fail(
                    $"DesignConversion model rollback generation conflict: expected {command.ExpectedGeneration}, actual {current.Generation}.",
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_GENERATION_CONFLICT");
            }

            if (current.ActiveVersion is null)
            {
                return OperationResult<DesignConversionModelActivationResult>.Fail(
                    "DesignConversion model rollback requires an active model version.",
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_ACTIVE_VERSION_REQUIRED");
            }

            if (current.PreviousVersion is null)
            {
                return OperationResult<DesignConversionModelActivationResult>.Fail(
                    "DesignConversion model rollback requires an immediately previous verified model version.",
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_PREVIOUS_VERSION_REQUIRED");
            }

            var operationCreatedAtUtc = _utcNow();
            var restored = current.PreviousVersion;
            var restoredCheckpoint = await RevalidateRollbackCheckpointAsync(restored, ct);
            if (!restoredCheckpoint.IsSuccess)
                return restoredCheckpoint.MapError<DesignConversionModelActivationResult>();

            var next = current with
            {
                TenantIdRef = tenantIdRef,
                Generation = current.Generation + 1,
                ActiveVersion = restored,
                PreviousVersion = null,
                UpdatedAtUtc = command.RequestedAtUtc ?? operationCreatedAtUtc
            };
            var result = BuildResult(
                "rollback",
                operationKeyRef,
                applied: true,
                replay: false,
                next,
                restored,
                next.PreviousVersion,
                [
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_APPLIED",
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_RESTORED_IMMEDIATE_PREVIOUS_VERSION",
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_GENERATION_CAS_PASSED",
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_RESTORED_CHECKPOINT_REVALIDATED",
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_ONE_STEP_PREVIOUS_VERSION_CONSUMED",
                    "DESIGN_CONVERSION_MODEL_ROLLBACK_PRODUCT_READY_FALSE"
                ],
                command);
            UpsertTenant(store, next);
            UpsertOperation(store, new ActivationOperationRecord(
                TenantIdRef: tenantIdRef,
                OperationKeyRef: operationKeyRef,
                OperationKind: "rollback",
                PayloadHash: payloadHash,
                Result: result,
                CreatedAtUtc: operationCreatedAtUtc));
            var saved = await SaveStoreAsync(store, ct);
            if (!saved.IsSuccess)
                return saved.MapError<DesignConversionModelActivationResult>();

            return OperationResult<DesignConversionModelActivationResult>.Ok(result);
        }
    }

    public async Task<OperationResult<DesignConversionModelActivationResult>> ReadActiveAsync(
        string tenantId,
        CancellationToken ct = default)
    {
        var tenant = TryBuildTenantIdentity(tenantId);
        if (!tenant.IsSuccess)
            return FailActivationTenant<DesignConversionModelActivationResult>(tenant);

        ct.ThrowIfCancellationRequested();
        var tenantIdRef = tenant.Value!.TenantIdRef;
        var cachedStore = TryReadStableCachedStore();
        if (cachedStore is not null)
            return BuildReadActiveResult(cachedStore, tenantIdRef);

        await _gate.WaitAsync(ct);
        try
        {
            var storeResult = await LoadStoreAsync(ct);
            if (!storeResult.IsSuccess)
                return storeResult.MapError<DesignConversionModelActivationResult>();

            return BuildReadActiveResult(storeResult.Value!, tenantIdRef);
        }
        finally
        {
            _gate.Release();
        }
    }

    public static string BuildTenantIdRef(string tenantId)
    {
        var tenant = TryBuildTenantIdentity(tenantId);
        if (!tenant.IsSuccess)
            throw new ArgumentException(tenant.ErrorMessage, nameof(tenantId));

        return tenant.Value!.TenantIdRef;
    }

    public static OperationResult<DesignConversionCanonicalTenantIdentity> TryBuildTenantIdentity(string? tenantId)
    {
        var canonical = TryCanonicalizeTenantId(tenantId);
        if (!canonical.IsSuccess)
            return canonical.MapError<DesignConversionCanonicalTenantIdentity>();

        return OperationResult<DesignConversionCanonicalTenantIdentity>.Ok(new DesignConversionCanonicalTenantIdentity(
            canonical.Value!,
            BuildTenantIdRefFromCanonical(canonical.Value!)));
    }

    public static OperationResult<string> TryCanonicalizeTenantId(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return OperationResult<string>.Fail(
                "DesignConversion tenant id is required.",
                "DESIGN_CONVERSION_TENANT_ID_REQUIRED");

        var trimmed = tenantId.Trim();
        if (trimmed.Length == 0)
            return OperationResult<string>.Fail(
                "DesignConversion tenant id is required.",
                "DESIGN_CONVERSION_TENANT_ID_REQUIRED");

        if (trimmed.Length > MaximumCanonicalTenantIdLength)
            return OperationResult<string>.Fail(
                "DesignConversion tenant id is invalid.",
                "DESIGN_CONVERSION_TENANT_ID_INVALID");

        var canonical = trimmed.ToLowerInvariant();
        var previousSeparator = false;
        for (var index = 0; index < canonical.Length; index++)
        {
            var character = canonical[index];
            var isLetter = character is >= 'a' and <= 'z';
            var isDigit = character is >= '0' and <= '9';
            if (isLetter || isDigit)
            {
                previousSeparator = false;
                continue;
            }

            if (character == '-' &&
                index > 0 &&
                index < canonical.Length - 1 &&
                !previousSeparator)
            {
                previousSeparator = true;
                continue;
            }

            return OperationResult<string>.Fail(
                "DesignConversion tenant id is invalid.",
                "DESIGN_CONVERSION_TENANT_ID_INVALID");
        }

        return OperationResult<string>.Ok(canonical);
    }

    private async Task<OperationResult<ValidatedActivationCommand>> ValidateActivationCommandAsync(
        DesignConversionModelActivationCommand command,
        CancellationToken ct)
    {
        var baseValidation = ValidateCommandFields(command?.TenantId, command?.RequestedByRef, command?.Reason, command?.IdempotencyKey);
        if (!baseValidation.IsSuccess)
            return baseValidation.MapError<ValidatedActivationCommand>();
        if (command!.ExpectedGeneration < 0)
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation expected generation cannot be negative.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_EXPECTED_GENERATION_INVALID");
        if (command.Candidate is null)
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires a promoted model activation record.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_RECORD_REQUIRED");

        var tenant = baseValidation.Value!;
        var tenantIdRef = tenant.TenantIdRef;
        if (!string.Equals(command.Candidate.TenantIdRef, tenantIdRef, StringComparison.Ordinal))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation tenant does not match the published activation record tenant.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_SCOPE_MISMATCH");
        }

        var publication = command.Candidate.Publication;
        var model = command.Candidate.Model;
        if (publication is null)
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires a promotion publication result.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PUBLICATION_REQUIRED");
        if (model is null)
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires promoted local model metadata.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTED_MODEL_REQUIRED");
        if (publication.Decision.Kind != DesignConversionPromotionDecisionKind.Promote || !publication.Decision.AllowsUpload)
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires a promote decision.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTE_DECISION_REQUIRED");
        if (publication.Manifest is null)
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires a published manifest.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_MANIFEST_REQUIRED");

        var manifest = publication.Manifest;
        if (manifest.ModelServerUploadAttempted || manifest.ModelServerPromotionClaimed)
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation only accepts local-only publication records without external model-server promotion claims.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_LOCAL_ONLY_PUBLICATION_REQUIRED");
        if (!manifest.LocalPromotionReceiptWritten ||
            !string.Equals(manifest.PromotionReceiptKind, "local_receipt_only", StringComparison.Ordinal))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires a verified local-only promotion receipt.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_LOCAL_RECEIPT_REQUIRED");
        }

        var uploadReceipt = publication.UploadReceipt;
        if (!IsLocalPromotionRef(manifest.PromotionUploadRef) ||
            uploadReceipt is null ||
            !IsLocalPromotionRef(uploadReceipt.RemoteRef) ||
            !string.Equals(manifest.PromotionUploadRef, uploadReceipt.RemoteRef, StringComparison.Ordinal))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation rejects unverified or non-local promotion refs.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_REF_UNVERIFIED");
        }

        if (!model.LocalOnly ||
            !model.PromotionManifestClaimed ||
            !model.CheckpointFreshLoadVerified ||
            !model.CheckpointContainsRealTrainedWeights)
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires local-only promoted model metadata with fresh-load and real-weight verification.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_MODEL_VERIFICATION_REQUIRED");
        }

        if (!string.Equals(model.ManifestId, manifest.ManifestId, StringComparison.Ordinal) ||
            !string.Equals(model.ManifestRef, manifest.ManifestRef, StringComparison.Ordinal) ||
            !string.Equals(model.MachineId, manifest.MachineId, StringComparison.Ordinal) ||
            !string.Equals(model.DomainTag, manifest.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(model.Version, manifest.Version, StringComparison.Ordinal))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation record metadata does not match the published manifest.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_MANIFEST_MODEL_MISMATCH");
        }

        if (!string.IsNullOrWhiteSpace(command.ExpectedModelVersion) &&
            !string.Equals(command.ExpectedModelVersion, model.Version, StringComparison.Ordinal))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation expected model version does not match the promoted model version.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_VERSION_SCOPE_MISMATCH");
        }

        if (manifest.PromotionArtifacts is null)
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires promotion artifact paths from the manifest.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACTS_REQUIRED");

        var checkpointPath = ValidateLocalCheckpointPath(model.CheckpointPath);
        if (!checkpointPath.IsSuccess)
            return checkpointPath.MapError<ValidatedActivationCommand>();
        var verifiedCheckpointPath = checkpointPath.Value!;
        var manifestCheckpointPath = ValidateLocalCheckpointPath(manifest.PromotionArtifacts.CheckpointPath);
        if (!manifestCheckpointPath.IsSuccess)
            return manifestCheckpointPath.MapError<ValidatedActivationCommand>();
        if (!string.Equals(verifiedCheckpointPath, manifestCheckpointPath.Value, _pathComparison))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation checkpoint path does not match the published manifest artifact.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_ARTIFACT_MISMATCH");
        }
        var tokenizerPath = ValidateLocalSupportingArtifactPath(manifest.PromotionArtifacts.TokenizerPath);
        if (!tokenizerPath.IsSuccess)
            return tokenizerPath.MapError<ValidatedActivationCommand>();
        var policyPath = ValidateLocalSupportingArtifactPath(manifest.PromotionArtifacts.PolicyPath);
        if (!policyPath.IsSuccess)
            return policyPath.MapError<ValidatedActivationCommand>();
        if (IsEvaluatedDpoPromotion(manifest) &&
            string.IsNullOrWhiteSpace(manifest.PromotionArtifacts.SourceEncoderPath))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation evaluated DPO promotion requires a source-encoder artifact path.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_SOURCE_ENCODER_ARTIFACT_REQUIRED");
        }
        if (!string.IsNullOrWhiteSpace(manifest.PromotionArtifacts.SourceEncoderPath))
        {
            var sourceEncoderPath = ValidateLocalSupportingArtifactPath(manifest.PromotionArtifacts.SourceEncoderPath);
            if (!sourceEncoderPath.IsSuccess)
                return sourceEncoderPath.MapError<ValidatedActivationCommand>();
        }

        DesignConversionDpoEvaluatedArtifactUploadLease? evaluatedArtifactLease = null;
        if (IsEvaluatedDpoPromotion(manifest))
        {
            var acquiredArtifacts = DesignConversionDpoPromotionArtifactContractCodec
                .AcquireValidatedEvaluatedArtifactUpload(manifest.PromotionArtifacts);
            if (!acquiredArtifacts.IsSuccess || acquiredArtifacts.Value is null)
            {
                return OperationResult<ValidatedActivationCommand>.Fail(
                    "DesignConversion model activation evaluated artifact authority is invalid.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACT_AUTHORITY_INVALID");
            }
            evaluatedArtifactLease = acquiredArtifacts.Value;
        }
        using var evaluatedArtifactLeaseScope = evaluatedArtifactLease;

        var checkpointSha = await ReadSha256Async(verifiedCheckpointPath, ct);
        if (!checkpointSha.IsSuccess)
            return checkpointSha.MapError<ValidatedActivationCommand>();
        if (!string.Equals(checkpointSha.Value, model.CheckpointSha256, StringComparison.Ordinal))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation checkpoint SHA does not match promoted model metadata.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_SHA_MISMATCH");
        }

        var checkpointLoad = await DesignConversionMachine.LoadCheckpointAsync(verifiedCheckpointPath, ct);
        if (!checkpointLoad.IsSuccess)
        {
            if (string.Equals(checkpointLoad.ErrorCode, "CANCELLED", StringComparison.Ordinal))
            {
                return OperationResult<ValidatedActivationCommand>.Fail(
                    "DesignConversion model activation checkpoint load cancelled.",
                    "CANCELLED");
            }

            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation checkpoint could not be loaded as the promoted model.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_LOAD_FAILED");
        }
        var loadedWeightAuthority = checkpointLoad.Value!.Machine
            .GetLoadedLearnedWeightAuthoritySummary();
        var evaluatedDpoWeightAuthority =
            evaluatedArtifactLease?.MatchesLoadedCheckpoint(checkpointLoad.Value) == true;
        var hasAcceptedWeightAuthority =
            loadedWeightAuthority.EmbeddedLearnedWeightArtifactCount > 0 ||
            evaluatedDpoWeightAuthority;
        if (!string.Equals(checkpointLoad.Value!.Checkpoint.MachineId, model.MachineId, StringComparison.Ordinal) ||
            !string.Equals(checkpointLoad.Value.Checkpoint.DomainTag, model.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(checkpointLoad.Value.Checkpoint.Version, model.Version, StringComparison.Ordinal) ||
            !hasAcceptedWeightAuthority)
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation checkpoint metadata does not match the promoted model.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_MODEL_MISMATCH");
        }
        if (evaluatedArtifactLease is not null &&
            !evaluatedArtifactLease.MatchesLoadedCheckpoint(checkpointLoad.Value))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation evaluated artifacts changed after checkpoint load.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACT_AUTHORITY_INVALID");
        }

        var modelEvidenceRefs = model.EvidenceRefs ?? [];
        if (!modelEvidenceRefs.Contains(DesignConversionPromotedLocalModelLoader.LocalPromotionBoundary, StringComparer.Ordinal))
        {
            return OperationResult<ValidatedActivationCommand>.Fail(
                "DesignConversion model activation requires promoted-local-model boundary evidence.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_BOUNDARY_EVIDENCE_REQUIRED");
        }

        var activationRef = ToHashRef(
            "design-conversion-model-activation-ref",
            string.Join(
                "|",
                tenantIdRef,
                model.ModelId,
                model.ManifestId,
                model.Version,
                model.CheckpointSha256,
                model.LocalCheckpointWeightsSource));
        var activeVersion = new DesignConversionActivatedModelVersion(
            ActivationRef: activationRef,
            ModelId: model.ModelId,
            ManifestId: model.ManifestId,
            ManifestRef: model.ManifestRef,
            PromotionUploadRef: manifest.PromotionUploadRef ?? string.Empty,
            Version: model.Version,
            MachineId: model.MachineId,
            DomainTag: model.DomainTag,
            CheckpointPath: verifiedCheckpointPath,
            CheckpointPathRef: ToHashRef("design-conversion-model-activation-checkpoint-path", verifiedCheckpointPath),
            CheckpointSha256: model.CheckpointSha256,
            CheckpointFreshLoadVerified: model.CheckpointFreshLoadVerified,
            CheckpointContainsRealTrainedWeights: model.CheckpointContainsRealTrainedWeights,
            LocalOnly: model.LocalOnly,
            EvidenceRefs: modelEvidenceRefs
                .Concat(publication.Decision.EvidenceRefs)
                .Concat(manifest.EvidenceRefs)
                .Concat(uploadReceipt.EvidenceRefs)
                .Append("DESIGN_CONVERSION_MODEL_ACTIVATION_RECORD_VERIFIED")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray())
        {
            LocalCheckpointWeightsSource = model.LocalCheckpointWeightsSource
        };
        return OperationResult<ValidatedActivationCommand>.Ok(new ValidatedActivationCommand(tenant, activeVersion));
    }

    private static OperationResult<DesignConversionCanonicalTenantIdentity> ValidateRollbackCommand(DesignConversionModelRollbackCommand command)
    {
        var validation = ValidateCommandFields(command?.TenantId, command?.RequestedByRef, command?.Reason, command?.IdempotencyKey);
        if (!validation.IsSuccess)
            return validation;
        if (command!.ExpectedGeneration < 0)
            return OperationResult<DesignConversionCanonicalTenantIdentity>.Fail(
                "DesignConversion model rollback expected generation cannot be negative.",
                "DESIGN_CONVERSION_MODEL_ROLLBACK_EXPECTED_GENERATION_INVALID");
        return validation;
    }

    private async Task<OperationResult<string>> RevalidateRollbackCheckpointAsync(
        DesignConversionActivatedModelVersion restored,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<string>.Fail(
                "DesignConversion model rollback checkpoint revalidation cancelled.",
                "CANCELLED");

        var checkpointPath = ValidateRollbackCheckpointPath(restored);
        if (!checkpointPath.IsSuccess)
            return checkpointPath;

        var verifiedCheckpointPath = checkpointPath.Value!;
        var checkpointSha = await ReadRollbackCheckpointSha256Async(verifiedCheckpointPath, ct);
        if (!checkpointSha.IsSuccess)
            return checkpointSha;

        if (!string.Equals(checkpointSha.Value, restored.CheckpointSha256, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<string>.Fail(
                "DesignConversion model rollback restored checkpoint SHA-256 does not match the stored previous-version metadata.",
                "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_SHA_MISMATCH");
        }

        var checkpointLoad = await DesignConversionMachine.LoadCheckpointAsync(verifiedCheckpointPath, ct);
        if (!checkpointLoad.IsSuccess)
        {
            if (string.Equals(checkpointLoad.ErrorCode, "CANCELLED", StringComparison.Ordinal))
            {
                return OperationResult<string>.Fail(
                    "DesignConversion model rollback checkpoint revalidation cancelled.",
                    "CANCELLED");
            }

            var failureCode = checkpointLoad.ErrorCode is "CHECKPOINT_NOT_FOUND" or "CHECKPOINT_READ_FAILED"
                ? "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE"
                : "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MODEL_MISMATCH_OR_CORRUPT";
            return OperationResult<string>.Fail(
                "DesignConversion model rollback restored checkpoint could not be loaded as the stored previous model.",
                failureCode);
        }

        var checkpoint = checkpointLoad.Value!.Checkpoint;
        var loadedWeightAuthority = checkpointLoad.Value.Machine
            .GetLoadedLearnedWeightAuthoritySummary();
        if (!string.Equals(checkpoint.MachineId, restored.MachineId, StringComparison.Ordinal) ||
            !string.Equals(checkpoint.DomainTag, restored.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(checkpoint.Version, restored.Version, StringComparison.Ordinal) ||
            !loadedWeightAuthority.HasRealLearnedWeightAuthority)
        {
            return OperationResult<string>.Fail(
                "DesignConversion model rollback restored checkpoint metadata or learned-weight contents do not match the stored previous-version metadata.",
                "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MODEL_MISMATCH_OR_CORRUPT");
        }

        return OperationResult<string>.Ok(verifiedCheckpointPath);
    }

    private OperationResult<string> ValidateRollbackCheckpointPath(DesignConversionActivatedModelVersion restored)
    {
        var checkpointPath = ValidateLocalArtifactFilePath(
            restored.CheckpointPath,
            requiredCode: "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE",
            notFullyQualifiedCode: "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE",
            uriCode: "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE",
            invalidCode: "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE",
            notFoundCode: "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE",
            safeMessage: "DesignConversion model rollback restored checkpoint is missing, unreadable, or not a valid server-owned local checkpoint path.");
        if (!checkpointPath.IsSuccess)
            return checkpointPath;

        try
        {
            var fullPath = checkpointPath.Value!;
            var expectedPathRef = ToHashRef("design-conversion-model-activation-checkpoint-path", fullPath);
            if (!string.Equals(restored.CheckpointPathRef, expectedPathRef, StringComparison.Ordinal))
                return FailRollbackCheckpointMissingOrUnreadable();

            return OperationResult<string>.Ok(fullPath);
        }
        catch
        {
            return FailRollbackCheckpointMissingOrUnreadable();
        }
    }

    private static async Task<OperationResult<string>> ReadRollbackCheckpointSha256Async(
        string checkpointPath,
        CancellationToken ct)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(checkpointPath, ct);
            return OperationResult<string>.Ok(ToSha256Hex(bytes));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<string>.Fail(
                "DesignConversion model rollback checkpoint revalidation cancelled.",
                "CANCELLED");
        }
        catch
        {
            return OperationResult<string>.Fail(
                "DesignConversion model rollback restored checkpoint is missing or unreadable.",
                "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE");
        }
    }

    private static OperationResult<string> FailRollbackCheckpointMissingOrUnreadable()
        => OperationResult<string>.Fail(
            "DesignConversion model rollback restored checkpoint is missing, unreadable, or not a valid server-owned local checkpoint path.",
            "DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE");

    private static OperationResult<DesignConversionCanonicalTenantIdentity> ValidateCommandFields(
        string? tenantId,
        string? requestedByRef,
        string? reason,
        string? idempotencyKey)
    {
        var tenant = TryBuildTenantIdentity(tenantId);
        if (!tenant.IsSuccess)
            return FailActivationTenant<DesignConversionCanonicalTenantIdentity>(tenant);
        if (string.IsNullOrWhiteSpace(requestedByRef))
            return OperationResult<DesignConversionCanonicalTenantIdentity>.Fail(
                "DesignConversion model lifecycle operation requires requestedByRef.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_REQUESTED_BY_REQUIRED");
        if (string.IsNullOrWhiteSpace(reason))
            return OperationResult<DesignConversionCanonicalTenantIdentity>.Fail(
                "DesignConversion model lifecycle operation requires an evidence-bound reason.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_REASON_REQUIRED");
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return OperationResult<DesignConversionCanonicalTenantIdentity>.Fail(
                "DesignConversion model lifecycle operation requires an idempotency key.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_KEY_REQUIRED");
        return tenant;
    }

    private OperationResult<string> ValidateLocalCheckpointPath(string? path)
        => ValidateLocalArtifactFilePath(
            path,
            requiredCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_REQUIRED",
            notFullyQualifiedCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_NOT_FULLY_QUALIFIED",
            uriCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_REF_INJECTION_REJECTED",
            invalidCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_INVALID",
            notFoundCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_NOT_FOUND",
            safeMessage: "DesignConversion model activation checkpoint path is not a valid server-owned local artifact.");

    private OperationResult<string> ValidateLocalSupportingArtifactPath(string? path)
        => ValidateLocalArtifactFilePath(
            path,
            requiredCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_SUPPORTING_ARTIFACTS_NOT_FOUND",
            notFullyQualifiedCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_SUPPORTING_ARTIFACT_PATH_INVALID",
            uriCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_SUPPORTING_ARTIFACT_PATH_INVALID",
            invalidCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_SUPPORTING_ARTIFACT_PATH_INVALID",
            notFoundCode: "DESIGN_CONVERSION_MODEL_ACTIVATION_SUPPORTING_ARTIFACTS_NOT_FOUND",
            safeMessage: "DesignConversion model activation supporting artifact path is not a valid server-owned local artifact.");

    private static bool IsEvaluatedDpoPromotion(DesignConversionTrainingManifest? manifest)
        => manifest?.EvidenceRefs?.Contains(EvaluatedDpoPromotionEvidence, StringComparer.Ordinal) == true;

    private OperationResult<string> ValidateLocalArtifactFilePath(
        string? path,
        string requiredCode,
        string notFullyQualifiedCode,
        string uriCode,
        string invalidCode,
        string notFoundCode,
        string safeMessage)
    {
        if (string.IsNullOrWhiteSpace(path))
            return OperationResult<string>.Fail(safeMessage, requiredCode);
        if (LooksLikeUriPath(path))
            return OperationResult<string>.Fail(safeMessage, uriCode);
        if (!Path.IsPathFullyQualified(path))
            return OperationResult<string>.Fail(safeMessage, notFullyQualifiedCode);
        if (ContainsParentTraversalSegment(path))
            return OperationResult<string>.Fail(safeMessage, invalidCode);

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return OperationResult<string>.Fail(safeMessage, invalidCode);
        }

        if (!Path.IsPathFullyQualified(fullPath) || !IsStrictlyUnderAllowedArtifactRoot(fullPath))
            return OperationResult<string>.Fail(safeMessage, invalidCode);

        var integrity = ValidateArtifactPathIntegrity(fullPath);
        if (!integrity.IsSuccess)
            return OperationResult<string>.Fail(safeMessage, invalidCode);

        if (!File.Exists(fullPath))
            return OperationResult<string>.Fail(safeMessage, notFoundCode);

        return OperationResult<string>.Ok(fullPath);
    }

    private static string NormalizeAllowedArtifactRootDirectory(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Activation registry allowed artifact root directory is required.", nameof(rootDirectory));

        return Path.GetFullPath(rootDirectory);
    }

    private static string EnsureTrailingDirectorySeparator(string value)
        => value.EndsWith(Path.DirectorySeparatorChar) || value.EndsWith(Path.AltDirectorySeparatorChar)
            ? value
            : value + Path.DirectorySeparatorChar;

    private static bool LooksLikeUriPath(string path)
        => path.Contains("://", StringComparison.Ordinal) ||
           path.StartsWith("file:", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsParentTraversalSegment(string path)
        => path
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => string.Equals(segment, "..", StringComparison.Ordinal));

    private bool IsStrictlyUnderAllowedArtifactRoot(string fullPath)
    {
        if (!fullPath.StartsWith(_allowedArtifactRootDirectoryWithSeparator, _pathComparison))
            return false;

        var relativePath = Path.GetRelativePath(_allowedArtifactRootDirectory, fullPath);
        return relativePath.Length > 0 &&
               !string.Equals(relativePath, ".", StringComparison.Ordinal) &&
               !string.Equals(relativePath, "..", StringComparison.Ordinal) &&
               !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
               !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal) &&
               !Path.IsPathRooted(relativePath);
    }

    private OperationResult<bool> ValidateArtifactPathIntegrity(string fullPath)
    {
        try
        {
            if (!Directory.Exists(_allowedArtifactRootDirectory))
                return OperationResult<bool>.Fail(
                    "DesignConversion model activation artifact root is unavailable.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACT_ROOT_INVALID");

            if (HasReparsePoint(_allowedArtifactRootDirectory))
                return OperationResult<bool>.Fail(
                    "DesignConversion model activation artifact root is not a regular directory.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACT_ROOT_INVALID");

            var parentDirectory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(parentDirectory))
                return OperationResult<bool>.Fail(
                    "DesignConversion model activation artifact path has no containing directory.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACT_PATH_INVALID");

            if (!ValidateDirectoryChainHasNoReparsePoints(parentDirectory))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion model activation artifact path crosses a reparse point.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACT_PATH_INVALID");
            }

            if (File.Exists(fullPath) && HasReparsePoint(fullPath))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion model activation artifact file is a reparse point.",
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACT_PATH_INVALID");
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation artifact path could not be inspected safely.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_ARTIFACT_PATH_INVALID");
        }
    }

    private bool ValidateDirectoryChainHasNoReparsePoints(string parentDirectory)
    {
        var relativeDirectory = Path.GetRelativePath(_allowedArtifactRootDirectory, parentDirectory);
        if (string.IsNullOrWhiteSpace(relativeDirectory) ||
            string.Equals(relativeDirectory, ".", StringComparison.Ordinal))
        {
            return true;
        }

        var current = _allowedArtifactRootDirectory;
        foreach (var segment in relativeDirectory.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.Equals(segment, ".", StringComparison.Ordinal))
                continue;
            if (string.Equals(segment, "..", StringComparison.Ordinal))
                return false;

            current = Path.Combine(current, segment);
            if (!Directory.Exists(current))
                return true;
            if (HasReparsePoint(current))
                return false;
        }

        return true;
    }

    private bool HasReparsePoint(string path)
        => (_getAttributes(path) & FileAttributes.ReparsePoint) != 0;

    private OperationResult<DesignConversionModelActivationResult>? ResolveReplay(
        ActivationStore store,
        string tenantIdRef,
        string operationKeyRef,
        string payloadHash,
        string operationKind)
    {
        var operation = store.Operations.FirstOrDefault(record =>
            string.Equals(record.TenantIdRef, tenantIdRef, StringComparison.Ordinal) &&
            string.Equals(record.OperationKeyRef, operationKeyRef, StringComparison.Ordinal));
        if (operation is null)
            return null;

        if (!string.Equals(operation.OperationKind, operationKind, StringComparison.Ordinal) ||
            !string.Equals(operation.PayloadHash, payloadHash, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionModelActivationResult>.Fail(
                "DesignConversion model lifecycle operation idempotency key was reused with a different payload.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_KEY_CONFLICT");
        }

        var metadata = new Dictionary<string, string>(operation.Result.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["model_activation_idempotency_replay"] = bool.TrueString,
            ["model_activation_side_effect_created"] = bool.FalseString
        };
        AddOperationRetentionMetadata(metadata);

        return OperationResult<DesignConversionModelActivationResult>.Ok(operation.Result with
        {
            Applied = false,
            Replay = true,
            EvidenceRefs = operation.Result.EvidenceRefs
                .Append("DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_REPLAY")
                .Append("DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_RETENTION_BOUNDED")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            BoundaryMetadata = metadata
        });
    }

    private static bool HasConflictingOperation(
        ActivationStore store,
        string tenantIdRef,
        string operationKeyRef,
        string payloadHash,
        out OperationResult<bool> conflict)
    {
        var operation = store.Operations.FirstOrDefault(record =>
            string.Equals(record.TenantIdRef, tenantIdRef, StringComparison.Ordinal) &&
            string.Equals(record.OperationKeyRef, operationKeyRef, StringComparison.Ordinal));
        if (operation is not null &&
            !string.Equals(operation.PayloadHash, payloadHash, StringComparison.Ordinal))
        {
            conflict = OperationResult<bool>.Fail(
                "DesignConversion model lifecycle operation idempotency key was reused with a different payload.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_KEY_CONFLICT");
            return true;
        }

        conflict = OperationResult<bool>.Ok(true);
        return false;
    }

    private DesignConversionModelActivationResult BuildResult(
        string operationKind,
        string operationKeyRef,
        bool applied,
        bool replay,
        TenantActivationState state,
        DesignConversionActivatedModelVersion active,
        DesignConversionActivatedModelVersion? previous,
        IReadOnlyList<string> operationEvidenceRefs,
        object? command)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["model_activation_store_schema_version"] = StoreSchemaVersion,
            ["model_activation_boundary"] = InternalActivationBoundary,
            ["model_activation_operation_kind"] = operationKind,
            ["model_activation_tenant_id_ref"] = state.TenantIdRef,
            ["model_activation_operation_key_ref"] = operationKeyRef,
            ["model_activation_generation"] = state.Generation.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["model_activation_previous_version_recorded"] = (previous is not null).ToString(),
            ["model_activation_active_model_id"] = active.ModelId,
            ["model_activation_active_version"] = active.Version,
            ["model_activation_active_checkpoint_sha256"] = active.CheckpointSha256,
            ["model_activation_active_checkpoint_path_ref"] = active.CheckpointPathRef,
            ["model_activation_checkpoint_weights_source"] = active.LocalCheckpointWeightsSource,
            ["model_activation_checkpoint_fresh_load_verified"] = active.CheckpointFreshLoadVerified.ToString(),
            ["model_activation_checkpoint_real_weights_verified"] = active.CheckpointContainsRealTrainedWeights.ToString(),
            ["model_activation_generation_cas_used"] = bool.TrueString,
            ["model_activation_idempotency_key_required"] = bool.TrueString,
            ["model_activation_tenant_id_required"] = bool.TrueString,
            ["model_activation_authorization_boundary"] =
                "internal_modelserver_orchestrated_tenant_id_idempotency_key_required_no_public_anonymous_control",
            ["model_activation_internal_only"] = bool.TrueString,
            ["model_activation_side_effect_created"] = applied.ToString(),
            ["model_activation_idempotency_replay"] = replay.ToString(),
            ["model_activation_checkpoints_deleted"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };
        AddOperationRetentionMetadata(metadata);

        if (previous is not null)
        {
            metadata["model_activation_previous_model_id"] = previous.ModelId;
            metadata["model_activation_previous_version"] = previous.Version;
            metadata["model_activation_previous_checkpoint_sha256"] = previous.CheckpointSha256;
        }

        switch (command)
        {
            case DesignConversionModelActivationCommand activation:
                metadata["model_activation_requested_by_ref"] = activation.RequestedByRef;
                metadata["model_activation_reason_ref"] = ToHashRef("design-conversion-model-activation-reason", activation.Reason);
                metadata["model_activation_expected_generation"] =
                    activation.ExpectedGeneration.ToString(System.Globalization.CultureInfo.InvariantCulture);
                break;
            case DesignConversionModelRollbackCommand rollback:
                metadata["model_activation_requested_by_ref"] = rollback.RequestedByRef;
                metadata["model_activation_reason_ref"] = ToHashRef("design-conversion-model-rollback-reason", rollback.Reason);
                metadata["model_activation_expected_generation"] =
                    rollback.ExpectedGeneration.ToString(System.Globalization.CultureInfo.InvariantCulture);
                break;
        }

        return new DesignConversionModelActivationResult(
            OperationKind: operationKind,
            OperationKeyRef: operationKeyRef,
            Applied: applied,
            Replay: replay,
            Generation: state.Generation,
            TenantIdRef: state.TenantIdRef,
            ProductReady: false,
            ActiveVersion: active,
            PreviousVersion: previous,
            EvidenceRefs: active.EvidenceRefs
                .Concat(operationEvidenceRefs)
                .Append("DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_RETENTION_BOUNDED")
                .Append("DESIGN_CONVERSION_MODEL_ACTIVATION_PRODUCT_READY_FALSE")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Warnings:
            [
                "model_activation_internal_only_no_public_anonymous_control",
                "model_activation_changes_runtime_selection_only_product_ready_false"
            ],
            BoundaryMetadata: metadata);
    }

    private static string BuildActivationPayloadHash(
        DesignConversionModelActivationCommand command,
        string tenantIdRef,
        DesignConversionActivatedModelVersion version)
        => ToSha256Hex(string.Join(
            "|",
            "activate",
            tenantIdRef,
            command.ExpectedGeneration.ToString(System.Globalization.CultureInfo.InvariantCulture),
            command.ExpectedModelVersion ?? string.Empty,
            version.ModelId,
            version.ManifestId,
            version.ManifestRef,
            version.Version,
            version.CheckpointSha256,
            version.LocalCheckpointWeightsSource));

    private static string BuildRollbackPayloadHash(
        DesignConversionModelRollbackCommand command,
        string tenantIdRef)
        => ToSha256Hex(string.Join(
            "|",
            "rollback",
            tenantIdRef,
            command.ExpectedGeneration.ToString(System.Globalization.CultureInfo.InvariantCulture)));

    private async Task<OperationResult<ActivationStore>> LoadStoreAsync(CancellationToken ct)
    {
        try
        {
            for (var attempt = 0; attempt < MaximumStableReadAttempts; attempt++)
            {
                var fingerprint = GetStoreFingerprint();
                if (!fingerprint.Exists)
                {
                    InvalidateStoreCache();
                    return OperationResult<ActivationStore>.Ok(ActivationStore.Empty);
                }
                if (fingerprint.Length > MaximumActivationStoreBytes)
                {
                    InvalidateStoreCache();
                    return FailActivationStoreRead();
                }

                var cachedSnapshot = Volatile.Read(ref _cachedSnapshot);
                if (cachedSnapshot is not null && cachedSnapshot.Fingerprint == fingerprint)
                    return OperationResult<ActivationStore>.Ok(CloneStore(cachedSnapshot.Store));

                var json = await _readAllTextAsync(_statePath, ActivationStoreEncoding, ct);
                if (!IsActivationStoreTextWithinByteLimit(json))
                {
                    InvalidateStoreCache();
                    return FailActivationStoreRead();
                }

                json = TrimOptionalUtf8Bom(json);
                if (!HasSupportedStoreSchema(json))
                {
                    InvalidateStoreCache();
                    return FailActivationStoreRead();
                }

                var parsed = JsonSerializer.Deserialize<ActivationStore>(json, JsonOptions);
                if (parsed is null ||
                    !string.Equals(parsed.SchemaVersion, StoreSchemaVersion, StringComparison.Ordinal) ||
                    parsed.Tenants is null ||
                    parsed.Operations is null)
                {
                    InvalidateStoreCache();
                    return FailActivationStoreRead();
                }

                var loadedFingerprint = GetStoreFingerprint();
                if (loadedFingerprint.Exists && loadedFingerprint == fingerprint)
                {
                    PublishStoreCache(parsed, loadedFingerprint);
                    return OperationResult<ActivationStore>.Ok(CloneStore(parsed));
                }

                InvalidateStoreCache();
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            InvalidateStoreCache();
            return OperationResult<ActivationStore>.Fail(
                "DesignConversion model activation store read cancelled.",
                "CANCELLED");
        }
        catch
        {
            InvalidateStoreCache();
            return FailActivationStoreRead();
        }

        InvalidateStoreCache();
        return FailActivationStoreRead();
    }

    private static OperationResult<ActivationStore> FailActivationStoreRead()
        => OperationResult<ActivationStore>.Fail(
            "DesignConversion model activation durable store could not be read safely.",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_STORE_READ_FAILED");

    private static OperationResult<T> FailActivationStoreMutation<T>()
        => OperationResult<T>.Fail(
            "DesignConversion model activation durable store could not be mutated safely.",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_STORE_MUTATION_FAILED");

    private static OperationResult<T> FailActivationMutationCancelled<T>()
        => OperationResult<T>.Fail(
            "DesignConversion model activation mutation cancelled.",
            "CANCELLED");

    private static bool HasSupportedStoreSchema(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.ValueKind == JsonValueKind.Object &&
               document.RootElement.TryGetProperty("schemaVersion", out var schema) &&
               schema.ValueKind == JsonValueKind.String &&
               string.Equals(schema.GetString(), StoreSchemaVersion, StringComparison.Ordinal);
    }

    private static async Task<string> ReadActivationStoreTextBoundedAsync(
        string path,
        Encoding encoding,
        CancellationToken ct)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: ActivationStoreReadBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        if (!stream.CanSeek)
            throw new InvalidDataException("Activation store length could not be measured safely.");

        var knownLength = stream.Length;
        if (knownLength < 0 ||
            knownLength > MaximumActivationStoreBytes ||
            knownLength > int.MaxValue)
            throw new InvalidDataException("Activation store exceeds the maximum permitted size.");

        var byteCount = checked((int)knownLength);
        var bytes = new byte[byteCount];
        var totalRead = 0;
        while (totalRead < byteCount)
        {
            var read = await stream.ReadAsync(bytes.AsMemory(totalRead, byteCount - totalRead), ct);
            if (read == 0)
                break;

            totalRead += read;
        }

        var growthSentinel = new byte[1];
        var sentinelRead = await stream.ReadAsync(growthSentinel.AsMemory(0, 1), ct);
        if (sentinelRead != 0)
            throw new InvalidDataException("Activation store changed while being read.");

        return TrimOptionalUtf8Bom(encoding.GetString(bytes, 0, totalRead));
    }

    private static bool IsActivationStoreTextWithinByteLimit(string json)
    {
        try
        {
            return ActivationStoreEncoding.GetByteCount(json) <= MaximumActivationStoreBytes;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    private static string TrimOptionalUtf8Bom(string json)
        => json.Length > 0 && json[0] == '\uFEFF'
            ? json[1..]
            : json;

    private async Task<OperationResult<bool>> SaveStoreAsync(ActivationStore store, CancellationToken ct)
    {
        string? tempPath = null;
        try
        {
            var directory = Path.GetDirectoryName(_statePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(store, JsonOptions);
            var bytes = ActivationStoreEncoding.GetBytes(json);
            if (bytes.LongLength > MaximumActivationStoreBytes)
                return FailActivationStoreMutation<bool>();

            tempPath = _statePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            await using (var stream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                ActivationStoreReadBufferSize,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(bytes.AsMemory(), ct);
                await stream.FlushAsync(ct);
                stream.Flush(flushToDisk: true);
            }

            ct.ThrowIfCancellationRequested();
            // Тестовый шов стоит после надёжной записи временного файла, но до публикации:
            // так отказ одинаково проверяется в Windows и Linux и не раскрывает данные реестра.
            _beforeStorePublicationCallbackForTesting?.Invoke();
            File.Move(tempPath, _statePath, overwrite: true);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            TryDeleteTempFile(tempPath);
            return FailActivationMutationCancelled<bool>();
        }
        catch (EncoderFallbackException)
        {
            TryDeleteTempFile(tempPath);
            return FailActivationStoreMutation<bool>();
        }
        catch (IOException)
        {
            TryDeleteTempFile(tempPath);
            return FailActivationStoreMutation<bool>();
        }
        catch (UnauthorizedAccessException)
        {
            TryDeleteTempFile(tempPath);
            return FailActivationStoreMutation<bool>();
        }

        try
        {
            var fingerprint = GetStoreFingerprint();
            if (fingerprint.Exists)
            {
                PublishStoreCache(store, fingerprint);
            }
            else
            {
                InvalidateStoreCache();
            }
        }
        catch
        {
            InvalidateStoreCache();
        }

        return OperationResult<bool>.Ok(true);
    }

    private async Task<OperationResult<ActivationStoreMutationLease>> AcquireStoreMutationLeaseAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return FailActivationMutationCancelled<ActivationStoreMutationLease>();

        try
        {
            await _gate.WaitAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return FailActivationMutationCancelled<ActivationStoreMutationLease>();
        }

        var gateHeld = true;
        try
        {
            var fileLease = await AcquireStoreMutationFileLeaseAsync(ct);
            if (!fileLease.IsSuccess)
            {
                _gate.Release();
                gateHeld = false;
                return fileLease.MapError<ActivationStoreMutationLease>();
            }

            InvalidateStoreCache();
            return OperationResult<ActivationStoreMutationLease>.Ok(new ActivationStoreMutationLease(_gate, fileLease.Value!));
        }
        catch (IOException)
        {
            if (gateHeld)
                _gate.Release();

            return FailActivationStoreMutation<ActivationStoreMutationLease>();
        }
        catch (UnauthorizedAccessException)
        {
            if (gateHeld)
                _gate.Release();

            return FailActivationStoreMutation<ActivationStoreMutationLease>();
        }
    }

    private async Task<OperationResult<DesignConversionDeterministicMutationLease>> AcquireStoreMutationFileLeaseAsync(CancellationToken ct)
    {
        try
        {
            var directory = Path.GetDirectoryName(_mutationLockPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            return OperationResult<DesignConversionDeterministicMutationLease>.Ok(
                await DesignConversionDeterministicMutationLock.WaitUntilAcquiredAsync(
                    _mutationLockPath,
                    MutationLeaseRetryDelay,
                    ct));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return FailActivationMutationCancelled<DesignConversionDeterministicMutationLease>();
        }
        catch (IOException)
        {
            return FailActivationStoreMutation<DesignConversionDeterministicMutationLease>();
        }
        catch (UnauthorizedAccessException)
        {
            return FailActivationStoreMutation<DesignConversionDeterministicMutationLease>();
        }
    }

    private static void TryDeleteTempFile(string? tempPath)
    {
        if (string.IsNullOrWhiteSpace(tempPath))
            return;

        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch
        {
        }
    }

    private ActivationStoreFingerprint GetStoreFingerprint()
    {
        var info = new FileInfo(_statePath);
        return info.Exists
            ? new ActivationStoreFingerprint(true, info.Length, info.LastWriteTimeUtc, info.CreationTimeUtc)
            : new ActivationStoreFingerprint(false, 0, DateTime.MinValue, DateTime.MinValue);
    }

    private void InvalidateStoreCache()
    {
        Volatile.Write(ref _cachedSnapshot, null);
    }

    private ActivationStore? TryReadStableCachedStore()
    {
        var cachedSnapshot = Volatile.Read(ref _cachedSnapshot);
        if (cachedSnapshot is null)
            return null;

        try
        {
            var fingerprint = GetStoreFingerprint();
            return fingerprint.Exists && cachedSnapshot.Fingerprint == fingerprint
                ? CloneStore(cachedSnapshot.Store)
                : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private void PublishStoreCache(ActivationStore store, ActivationStoreFingerprint fingerprint)
        => Volatile.Write(
            ref _cachedSnapshot,
            new ActivationStoreCacheSnapshot(CloneStore(store), fingerprint));

    private static ActivationStore CloneStore(ActivationStore store)
        => new()
        {
            SchemaVersion = store.SchemaVersion,
            Tenants = store.Tenants.ToArray(),
            Operations = store.Operations.ToArray()
        };

    private OperationResult<DesignConversionModelActivationResult> BuildReadActiveResult(
        ActivationStore store,
        string tenantIdRef)
    {
        var current = ResolveTenantState(store, tenantIdRef);
        if (current.ActiveVersion is null)
        {
            return OperationResult<DesignConversionModelActivationResult>.Fail(
                "DesignConversion model activation state has no active model for this tenant.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND");
        }

        return OperationResult<DesignConversionModelActivationResult>.Ok(
            BuildResult(
                "read-active",
                string.Empty,
                applied: false,
                replay: false,
                current,
                current.ActiveVersion,
                current.PreviousVersion,
                ["DESIGN_CONVERSION_MODEL_ACTIVATION_STATE_READ"],
                command: null));
    }

    private static TenantActivationState ResolveTenantState(ActivationStore store, string tenantIdRef)
        => store.Tenants.FirstOrDefault(state => string.Equals(state.TenantIdRef, tenantIdRef, StringComparison.Ordinal))
           ?? new TenantActivationState(tenantIdRef, 0, null, null, DateTimeOffset.UnixEpoch);

    private static void UpsertTenant(ActivationStore store, TenantActivationState state)
    {
        var next = store.Tenants
            .Where(existing => !string.Equals(existing.TenantIdRef, state.TenantIdRef, StringComparison.Ordinal))
            .Append(state)
            .OrderBy(existing => existing.TenantIdRef, StringComparer.Ordinal)
            .ToArray();
        store.Tenants = next;
    }

    private static EffectiveOperationRetentionLimits ValidateOperationRetentionLimits(
        DesignConversionModelActivationOperationRetentionLimits? limits)
    {
        var retainedOperationCount = limits?.RetainedOperationCount ?? DefaultRetainedOperationCount;
        var retainedOperationCountPerTenant =
            limits?.RetainedOperationCountPerTenant ?? DefaultRetainedOperationCountPerTenant;

        if (retainedOperationCount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(limits),
                retainedOperationCount,
                "DesignConversion model activation retained operation count must be positive.");
        if (retainedOperationCountPerTenant <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(limits),
                retainedOperationCountPerTenant,
                "DesignConversion model activation retained operation count per tenant must be positive.");
        if (retainedOperationCountPerTenant > retainedOperationCount)
            throw new ArgumentException(
                "DesignConversion model activation retained operation count per tenant cannot exceed the global retained operation count.",
                nameof(limits));

        return new EffectiveOperationRetentionLimits(
            retainedOperationCount,
            retainedOperationCountPerTenant);
    }

    private void AddOperationRetentionMetadata(IDictionary<string, string> metadata)
    {
        metadata["model_activation_idempotency_retention_bounded"] = bool.TrueString;
        metadata["model_activation_idempotency_retention_unbounded"] = bool.FalseString;
        metadata["model_activation_idempotency_infinite_retention_claimed"] = bool.FalseString;
        metadata["model_activation_idempotency_retained_operation_count"] =
            _operationRetentionLimits.RetainedOperationCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        metadata["model_activation_idempotency_retained_operation_count_per_tenant"] =
            _operationRetentionLimits.RetainedOperationCountPerTenant.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private void UpsertOperation(ActivationStore store, ActivationOperationRecord operation)
    {
        var deduplicated = store.Operations
            .Where(existing => !HasSameOperationIdentity(existing, operation))
            .Append(operation)
            .ToArray();

        var perTenantRetained = deduplicated
            .GroupBy(existing => existing.TenantIdRef, StringComparer.Ordinal)
            .SelectMany(group => OrderOperationsForRetention(group, operation)
                .Take(_operationRetentionLimits.RetainedOperationCountPerTenant))
            .ToArray();

        store.Operations = OrderOperationsForRetention(perTenantRetained, operation)
            .Take(_operationRetentionLimits.RetainedOperationCount)
            .OrderBy(existing => existing.CreatedAtUtc)
            .ThenBy(existing => existing.TenantIdRef, StringComparer.Ordinal)
            .ThenBy(existing => existing.OperationKeyRef, StringComparer.Ordinal)
            .ThenBy(existing => existing.OperationKind, StringComparer.Ordinal)
            .ThenBy(existing => existing.PayloadHash, StringComparer.Ordinal)
            .ToArray();
    }

    private static IOrderedEnumerable<ActivationOperationRecord> OrderOperationsForRetention(
        IEnumerable<ActivationOperationRecord> operations,
        ActivationOperationRecord justApplied)
        => operations
            .OrderByDescending(existing => HasSameOperationIdentity(existing, justApplied))
            .ThenByDescending(existing => existing.CreatedAtUtc)
            .ThenBy(existing => existing.TenantIdRef, StringComparer.Ordinal)
            .ThenBy(existing => existing.OperationKeyRef, StringComparer.Ordinal)
            .ThenBy(existing => existing.OperationKind, StringComparer.Ordinal)
            .ThenBy(existing => existing.PayloadHash, StringComparer.Ordinal);

    private static bool HasSameOperationIdentity(
        ActivationOperationRecord left,
        ActivationOperationRecord right)
        => string.Equals(left.TenantIdRef, right.TenantIdRef, StringComparison.Ordinal) &&
           string.Equals(left.OperationKeyRef, right.OperationKeyRef, StringComparison.Ordinal);

    private static bool IsLocalPromotionRef(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.StartsWith(DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix, StringComparison.Ordinal);

    private static async Task<OperationResult<string>> ReadSha256Async(string path, CancellationToken ct)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(path, ct);
            return OperationResult<string>.Ok(ToSha256Hex(bytes));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<string>.Fail(
                "DesignConversion model activation checkpoint hash cancelled.",
                "CANCELLED");
        }
        catch
        {
            return OperationResult<string>.Fail(
                "DesignConversion model activation checkpoint hash failed.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_HASH_FAILED");
        }
    }

    private static string ToHashRef(string scope, string value)
        => $"sha256:{scope}:{ToSha256Hex(Encoding.UTF8.GetBytes(value))}";

    private static string BuildTenantIdRefFromCanonical(string canonicalTenantId)
        => ToHashRef(TenantIdRefScope, canonicalTenantId);

    private static OperationResult<T> FailActivationTenant<T>(
        OperationResult<DesignConversionCanonicalTenantIdentity> tenant)
        => OperationResult<T>.Fail(
            tenant.ErrorMessage ?? "DesignConversion model activation tenant id is invalid.",
            string.Equals(tenant.ErrorCode, "DESIGN_CONVERSION_TENANT_ID_REQUIRED", StringComparison.Ordinal)
                ? "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_ID_REQUIRED"
                : "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_ID_INVALID");

    private static string ToSha256Hex(string value)
        => ToSha256Hex(Encoding.UTF8.GetBytes(value));

    private static string ToSha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record TenantActivationState(
        string TenantIdRef,
        long Generation,
        DesignConversionActivatedModelVersion? ActiveVersion,
        DesignConversionActivatedModelVersion? PreviousVersion,
        DateTimeOffset UpdatedAtUtc);

    private sealed record ActivationOperationRecord(
        string TenantIdRef,
        string OperationKeyRef,
        string OperationKind,
        string PayloadHash,
        DesignConversionModelActivationResult Result,
        DateTimeOffset CreatedAtUtc);

    private sealed record ValidatedActivationCommand(
        DesignConversionCanonicalTenantIdentity Tenant,
        DesignConversionActivatedModelVersion ActiveVersion);

    private readonly record struct ActivationStoreFingerprint(
        bool Exists,
        long Length,
        DateTime LastWriteTimeUtc,
        DateTime CreationTimeUtc);

    private sealed record ActivationStoreCacheSnapshot(
        ActivationStore Store,
        ActivationStoreFingerprint Fingerprint);

    private sealed class ActivationStoreMutationLease : IDisposable
    {
        private readonly SemaphoreSlim _gate;
        private readonly DesignConversionDeterministicMutationLease _fileLease;
        private bool _disposed;

        public ActivationStoreMutationLease(SemaphoreSlim gate, DesignConversionDeterministicMutationLease fileLease)
        {
            _gate = gate;
            _fileLease = fileLease;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _fileLease.Dispose();
            _gate.Release();
            _disposed = true;
        }
    }

    private readonly record struct EffectiveOperationRetentionLimits(
        int RetainedOperationCount,
        int RetainedOperationCountPerTenant);

    private sealed class ActivationStore
    {
        public string SchemaVersion { get; set; } = StoreSchemaVersion;
        public IReadOnlyList<TenantActivationState> Tenants { get; set; } = [];
        public IReadOnlyList<ActivationOperationRecord> Operations { get; set; } = [];

        public static ActivationStore Empty => new();
    }

    private static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
}
