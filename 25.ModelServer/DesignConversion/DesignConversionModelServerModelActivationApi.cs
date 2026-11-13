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
    private const string EvaluatedDpoPromotionEvidence =
        "DESIGN_CONVERSION_PROMOTION_GATE_FROM_VALIDATED_EVALUATED_DPO_CONTRACT";

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

    public async Task<OperationResult<DesignConversionModelActivationResult>> ActivateLocalPromotionAsync(
        DesignConversionModelServerModelActivationAuthorization authorization,
        DesignConversionModelServerLocalPromotionActivationSelector selector,
        string? expectedModelVersion = null,
        DateTimeOffset? requestedAtUtc = null,
        CancellationToken ct = default)
    {
        var auth = ValidateAuthorization(authorization);
        if (!auth.IsSuccess)
            return auth.MapError<DesignConversionModelActivationResult>();

        var durableCandidate = await ResolveDurableLocalPromotionActivationRecordAsync(
            authorization,
            selector,
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
                durableCandidate.Value!.Model.Version,
                requestedAtUtc),
            ct);

        return Annotate(
            result,
            durablePromotionAuthorityVerified: true,
            refsOnlyLocalPromotionSelectorVerified: true);
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
            .Where(record => string.Equals(
                ReadMetadata(record.BoundaryMetadata, "training_intake_tenant_id_ref"),
                tenantIdRef,
                StringComparison.Ordinal))
            .Where(record => MatchesClaimedPromotion(record, candidate))
            .ToArray();
        if (durableCandidates.Length == 0)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation could not find a matching server-owned local promotion record.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");
        }

        var authorityRecords = records.Value!;
        var currentState = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState(authorityRecords);
        if (currentState.Kind ==
            DesignConversionLocalPromotionActiveStateKind.Invalid)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation local promotion authority state is invalid.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_STATE_INVALID");
        }
        var currentRecords = currentState.Kind ==
            DesignConversionLocalPromotionActiveStateKind.Active
            ? authorityRecords
                .Where(IsReadyPromotionAuthorityRecord)
                .Where(record => string.Equals(
                    ReadMetadata(
                        record.BoundaryMetadata,
                        "local_promotion_active_local_model_ref"),
                    currentState.Active!.ActiveLocalModelRef,
                    StringComparison.Ordinal))
                .ToArray()
            : [];
        if (currentRecords.Length > 1)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion model activation local promotion authority state is ambiguous.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_STATE_INVALID");
        }
        var durableRecord = currentRecords.SingleOrDefault();
        if (durableRecord is null ||
            !MatchesClaimedPromotion(durableRecord, candidate))
        {
            var bootstrapRecord = await ResolveBootstrapDurablePromotionRecordAsync(
                authorization,
                tenant.Value.TenantId,
                authorityRecords,
                durableCandidates,
                ct);
            if (!bootstrapRecord.IsSuccess)
                return bootstrapRecord.MapError<DesignConversionPromotedModelActivationRecord>();

            durableRecord = bootstrapRecord.Value!;
        }

        var binding = ValidateDurablePromotionBinding(
            durableRecord,
            candidate,
            tenantIdRef,
            expectedModelVersion);
        if (!binding.IsSuccess)
            return binding.MapError<DesignConversionPromotedModelActivationRecord>();

        return OperationResult<DesignConversionPromotedModelActivationRecord>.Ok(
            new DesignConversionPromotedModelActivationRecord(
                tenantIdRef,
                durableRecord.Publication!,
                durableRecord.PromotedLocalModel!));
    }

    private async Task<OperationResult<DesignConversionPromotedModelActivationRecord>>
        ResolveDurableLocalPromotionActivationRecordAsync(
            DesignConversionModelServerModelActivationAuthorization authorization,
            DesignConversionModelServerLocalPromotionActivationSelector selector,
            string? expectedModelVersion,
            CancellationToken ct)
    {
        if (_localPromotionAuthority is null)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion local-promotion activation requires the server-owned local promotion authority.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_NOT_CONFIGURED");
        }

        var tenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(authorization.TenantId);
        if (!tenant.IsSuccess)
            return tenant.MapError<DesignConversionPromotedModelActivationRecord>();
        var tenantIdRef = tenant.Value!.TenantIdRef;

        var selectorShape = ValidateLocalPromotionActivationSelector(
            selector,
            expectedModelVersion);
        if (!selectorShape.IsSuccess)
            return selectorShape.MapError<DesignConversionPromotedModelActivationRecord>();

        var records = await _localPromotionAuthority.ListTrainingIntakeLocalPromotionRecordsAsync(ct);
        if (!records.IsSuccess)
        {
            if (string.Equals(records.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                return records.MapError<DesignConversionPromotedModelActivationRecord>();

            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion local-promotion activation could not read the server-owned local promotion authority.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_READ_FAILED");
        }

        var authorityRecords = records.Value ??
                               Array.Empty<DesignConversionTrainingIntakeLocalPromotionResult>();
        var tenantAuthorityRecords = authorityRecords
            .Where(record => string.Equals(
                ReadMetadata(record.BoundaryMetadata, "training_intake_tenant_id_ref"),
                tenantIdRef,
                StringComparison.Ordinal))
            .ToArray();

        // Выбор по selector сделан намеренно строгим: одна безопасная ссылка на
        // promotion upload ref плюс model/version/checkpoint SHA должны указать
        // ровно одну durable строку этого tenant-а. Любая неоднозначность закрывает
        // путь до mutation, чтобы caller не смог подобрать чужую или stale запись.
        var selectorMatches = tenantAuthorityRecords
            .Where(IsReadyPromotionAuthorityRecord)
            .Where(record => MatchesLocalPromotionActivationSelector(record, selector))
            .ToArray();
        if (selectorMatches.Length == 0)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion local-promotion activation could not find a matching server-owned local promotion record.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND");
        }

        if (selectorMatches.Length > 1)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion local-promotion activation selector matched multiple server-owned durable records.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_LOCAL_PROMOTION_SELECTOR_AMBIGUOUS");
        }

        var durableRecord = selectorMatches[0];
        var currentState = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState(tenantAuthorityRecords);
        if (currentState.Kind ==
            DesignConversionLocalPromotionActiveStateKind.Invalid)
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion local-promotion activation authority state is invalid.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_STATE_INVALID");
        }

        if (currentState.Kind !=
                DesignConversionLocalPromotionActiveStateKind.Active ||
            currentState.Active is null)
        {
            return PromotionNotCurrent()
                .MapError<DesignConversionPromotedModelActivationRecord>();
        }

        var currentRecords = tenantAuthorityRecords
            .Where(IsReadyPromotionAuthorityRecord)
            .Where(record => string.Equals(
                ReadMetadata(
                    record.BoundaryMetadata,
                    "local_promotion_active_local_model_ref"),
                currentState.Active.ActiveLocalModelRef,
                StringComparison.Ordinal))
            .ToArray();
        if (currentRecords.Length != 1 ||
            !MatchesCurrentLocalPromotionState(currentRecords[0], currentState.Active))
        {
            return OperationResult<DesignConversionPromotedModelActivationRecord>.Fail(
                "DesignConversion local-promotion activation current authority state is ambiguous.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_STATE_INVALID");
        }

        // Current проверяется после exact selector match: private binding всё ещё
        // приходит только из durable строки, но stale/rolled-back ссылка не может
        // открыть старую модель даже при валидном SHA.
        if (!MatchesLocalPromotionActivationSelector(currentRecords[0], selector) ||
            IsRolledBackPromotionTarget(tenantAuthorityRecords, durableRecord))
        {
            return PromotionNotCurrent()
                .MapError<DesignConversionPromotedModelActivationRecord>();
        }

        var activationRecord = new DesignConversionPromotedModelActivationRecord(
            tenantIdRef,
            durableRecord.Publication!,
            durableRecord.PromotedLocalModel!);
        var candidateShape = ValidateCandidateShape(activationRecord);
        if (!candidateShape.IsSuccess)
            return candidateShape.MapError<DesignConversionPromotedModelActivationRecord>();

        return OperationResult<DesignConversionPromotedModelActivationRecord>.Ok(
            activationRecord);
    }

    private async Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>>
        ResolveBootstrapDurablePromotionRecordAsync(
            DesignConversionModelServerModelActivationAuthorization authorization,
            string tenantId,
            IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult> authorityRecords,
            IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult> durableCandidates,
            CancellationToken ct)
    {
        if (authorization.ExpectedGeneration != 0)
        {
            return PromotionNotCurrent();
        }

        var active = await _registry.ReadActiveAsync(tenantId, ct);
        if (active.IsSuccess)
        {
            return PromotionNotCurrent();
        }

        if (!string.Equals(
                active.ErrorCode,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND",
                StringComparison.Ordinal))
        {
            return active.MapError<DesignConversionTrainingIntakeLocalPromotionResult>();
        }

        if (durableCandidates.Count != 1)
        {
            return OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Fail(
                "DesignConversion model activation local promotion authority state is ambiguous.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_STATE_INVALID");
        }

        var durableRecord = durableCandidates[0];
        if (IsRolledBackPromotionTarget(authorityRecords, durableRecord))
        {
            return PromotionNotCurrent();
        }

        return OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Ok(
            durableRecord);
    }

    private static bool IsRolledBackPromotionTarget(
        IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult> authorityRecords,
        DesignConversionTrainingIntakeLocalPromotionResult durableRecord)
    {
        var activeRef = ReadMetadata(
            durableRecord.BoundaryMetadata,
            "local_promotion_active_local_model_ref");
        if (string.IsNullOrWhiteSpace(activeRef))
            return false;

        return (authorityRecords ?? [])
            .Any(record =>
                string.Equals(
                    ReadMetadata(record.BoundaryMetadata, "local_promotion_record_kind"),
                    "rollback",
                    StringComparison.Ordinal) &&
                string.Equals(
                    ReadMetadata(
                        record.BoundaryMetadata,
                        "local_promotion_rollback_target_local_model_ref"),
                    activeRef,
                    StringComparison.Ordinal));
    }

    private static OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>
        PromotionNotCurrent()
        => OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>.Fail(
            "DesignConversion model activation candidate is not the current active local promotion authority state.",
            "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_NOT_CURRENT_ACTIVE");

    private static OperationResult<bool> ValidateLocalPromotionActivationSelector(
        DesignConversionModelServerLocalPromotionActivationSelector? selector,
        string? expectedModelVersion)
    {
        if (selector is null)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion local-promotion activation requires a refs-only selector.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_LOCAL_PROMOTION_SELECTOR_REQUIRED");
        }

        // Security-инвариант selector-а: caller может принести только идентичности
        // already-promoted локальной записи. Даже внутри local:// ref нельзя
        // протащить path-shaped payload, иначе selector снова станет каналом
        // передачи private filesystem authority.
        if (!IsSafeLocalPromotionSelectorRef(selector.PromotionUploadRef) ||
            !IsSafeSelectorValue(selector.PromotedModelId, allowForwardSlash: false) ||
            !IsSafeSelectorValue(selector.Version, allowForwardSlash: false) ||
            !DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(selector.CheckpointSha256))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion local-promotion activation selector must contain only safe promotion refs, model/version refs, and a canonical checkpoint SHA-256.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_LOCAL_PROMOTION_SELECTOR_INVALID");
        }

        if (!string.IsNullOrWhiteSpace(expectedModelVersion) &&
            !Same(expectedModelVersion.Trim(), selector.Version))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion local-promotion activation expected model version does not match the selector version.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_VERSION_SCOPE_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool MatchesLocalPromotionActivationSelector(
        DesignConversionTrainingIntakeLocalPromotionResult durableRecord,
        DesignConversionModelServerLocalPromotionActivationSelector selector)
    {
        var durable = durableRecord.PromotedLocalModel;
        var publication = durableRecord.Publication;
        var manifest = publication?.Manifest;
        return durable is not null &&
               manifest is not null &&
               publication?.UploadReceipt is not null &&
               Same(manifest.PromotionUploadRef, selector.PromotionUploadRef) &&
               Same(publication.UploadReceipt.RemoteRef, selector.PromotionUploadRef) &&
               Same(durable.ModelId, selector.PromotedModelId) &&
               Same(durable.Version, selector.Version) &&
               Same(manifest.Version, selector.Version) &&
               Same(durable.CheckpointSha256, selector.CheckpointSha256);
    }

    private static bool MatchesCurrentLocalPromotionState(
        DesignConversionTrainingIntakeLocalPromotionResult durableRecord,
        DesignConversionLocalPromotionActive current)
    {
        var durable = durableRecord.PromotedLocalModel;
        var manifest = durableRecord.Publication?.Manifest;
        return durable is not null &&
               manifest is not null &&
               Same(
                   ReadMetadata(
                       durableRecord.BoundaryMetadata,
                       "local_promotion_active_local_model_ref"),
                   current.ActiveLocalModelRef) &&
               Same(manifest.PromotionUploadRef, current.ActiveLocalModelRef) &&
               Same(durable.ModelId, current.ActiveModelId) &&
               Same(durable.CheckpointSha256, current.ActiveCheckpointSha256);
    }

    private static bool IsSafeLocalPromotionSelectorRef(string? value)
    {
        if (!IsLocalPromotionRef(value) ||
            !IsSafeSelectorValue(value, allowForwardSlash: true))
        {
            return false;
        }

        var trimmed = value!.Trim();
        var suffix = trimmed[DesignConversionPromotedLocalModelLoader
            .LocalOnlyPromotionRefPrefix.Length..];
        return IsSafeSelectorValue(suffix, allowForwardSlash: false);
    }

    private static bool IsSafeSelectorValue(string? value, bool allowForwardSlash)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return trimmed.Length <= 1024 &&
               !trimmed.Any(char.IsControl) &&
               !ContainsPathLikeSelectorShape(trimmed, allowForwardSlash) &&
               !trimmed.Contains("<", StringComparison.Ordinal) &&
               !trimmed.Contains("{", StringComparison.Ordinal) &&
               !trimmed.Contains("file://", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains("private", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains("secret", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains("token", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains("checkpoint_path:", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains("raw_provider_payload", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains("provider_payload", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsPathLikeSelectorShape(
        string value,
        bool allowForwardSlash)
    {
        if (ContainsRawPathLikeSelectorShape(value, allowForwardSlash))
            return true;

        try
        {
            var decoded = Uri.UnescapeDataString(value);
            return !Same(decoded, value) &&
                   ContainsRawPathLikeSelectorShape(decoded, allowForwardSlash);
        }
        catch (UriFormatException)
        {
            return true;
        }
    }

    private static bool ContainsRawPathLikeSelectorShape(
        string value,
        bool allowForwardSlash)
        => value.Contains("\\", StringComparison.Ordinal) ||
           ContainsDriveLetterPathShape(value) ||
           ContainsTraversalSegment(value) ||
           value.StartsWith("/", StringComparison.Ordinal) ||
           value.StartsWith("~/", StringComparison.Ordinal) ||
           value.StartsWith("./", StringComparison.Ordinal) ||
           (!allowForwardSlash &&
            value.Contains("/", StringComparison.Ordinal));

    private static bool ContainsDriveLetterPathShape(string value)
    {
        for (var index = 0; index + 2 < value.Length; index++)
        {
            if (!char.IsAsciiLetter(value[index]) ||
                value[index + 1] != ':' ||
                value[index + 2] is not ('/' or '\\'))
            {
                continue;
            }

            if (index == 0 ||
                value[index - 1] is '/' or '\\' or ':' ||
                char.IsWhiteSpace(value[index - 1]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsTraversalSegment(string value)
    {
        if (ContainsTraversalSegmentCore(value))
            return true;

        try
        {
            var decoded = Uri.UnescapeDataString(value);
            return !Same(decoded, value) &&
                   ContainsTraversalSegmentCore(decoded);
        }
        catch (UriFormatException)
        {
            return true;
        }
    }

    private static bool ContainsTraversalSegmentCore(string value)
        => value
            .Replace('\\', '/')
            .Split('/')
            .Any(segment => string.Equals(segment, "..", StringComparison.Ordinal));

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
               HasUsableLocalPath(artifacts.PolicyPath) &&
               (!IsEvaluatedDpoPromotion(publication.Manifest) ||
                HasUsableLocalPath(artifacts.SourceEncoderPath));
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

        if ((IsEvaluatedDpoPromotion(candidateManifest) || IsEvaluatedDpoPromotion(durableManifest)) &&
            !SameOptionalPath(
                candidateManifest.PromotionArtifacts?.SourceEncoderPath,
                durableManifest.PromotionArtifacts?.SourceEncoderPath))
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

        if (IsEvaluatedDpoPromotion(candidate.Publication.Manifest) &&
            !HasUsableLocalPath(candidate.Publication.Manifest.PromotionArtifacts.SourceEncoderPath))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion model activation evaluated DPO promotion requires a source-encoder artifact path.",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_SOURCE_ENCODER_ARTIFACT_REQUIRED");
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

    private static bool SameOptionalPath(string? left, string? right)
    {
        var normalizedLeft = NormalizePathRef(left);
        var normalizedRight = NormalizePathRef(right);
        return string.IsNullOrWhiteSpace(normalizedLeft) || string.IsNullOrWhiteSpace(normalizedRight)
            ? string.IsNullOrWhiteSpace(normalizedLeft) && string.IsNullOrWhiteSpace(normalizedRight)
            : string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEvaluatedDpoPromotion(DesignConversionTrainingManifest? manifest)
        => manifest?.EvidenceRefs?.Contains(EvaluatedDpoPromotionEvidence, StringComparer.Ordinal) == true;

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
        bool durablePromotionAuthorityVerified = false,
        bool refsOnlyLocalPromotionSelectorVerified = false)
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
        if (refsOnlyLocalPromotionSelectorVerified)
        {
            metadata["modelserver_model_lifecycle_refs_only_local_promotion_selector_verified"] = bool.TrueString;
            metadata["modelserver_model_lifecycle_activation_record_source"] =
                "server_owned_durable_local_promotion_authority_row";
        }

        return OperationResult<DesignConversionModelActivationResult>.Ok(value with
        {
            BoundaryMetadata = metadata,
            EvidenceRefs = value.EvidenceRefs
                .Append("DESIGN_CONVERSION_MODEL_SERVER_MODEL_LIFECYCLE_INTERNAL_AUTH_BOUNDARY")
                .Concat(durablePromotionAuthorityVerified
                    ? ["DESIGN_CONVERSION_MODEL_ACTIVATION_DURABLE_LOCAL_PROMOTION_AUTHORITY_VERIFIED"]
                    : [])
                .Concat(refsOnlyLocalPromotionSelectorVerified
                    ? ["DESIGN_CONVERSION_MODEL_ACTIVATION_REFS_ONLY_LOCAL_PROMOTION_SELECTOR_VERIFIED"]
                    : [])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        });
    }
}
