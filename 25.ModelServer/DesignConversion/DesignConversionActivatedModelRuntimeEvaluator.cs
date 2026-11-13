// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Runtime.CompilerServices;
using AgentInfrastructure;
using DesignConversionDomain;

public interface IDesignConversionActivatedModelRuntimeReadback
{
    bool TryGetPromotedLocalModel(string jobId, out DesignConversionPromotedLocalModel? promotedLocalModel);
}

internal interface IDesignConversionVerifiedActivatedModelRuntimeReadback
{
    bool TryGetVerifiedPromotedLocalModel(
        string jobId,
        out DesignConversionPromotedLocalModel? promotedLocalModel);
}

public sealed class DesignConversionActivatedModelRuntimeEvaluator : IDesignConversionThinClientJobEvaluator,
    IDesignConversionDpoPolicyPairRuntimeEvaluator,
    IDesignConversionActivatedModelRuntimeReadback,
    IDesignConversionVerifiedActivatedModelRuntimeReadback
{
    public const string RuntimeSelectionBoundary =
        "design_conversion_modelserver_runtime_selection_reads_durable_active_model_and_fresh_loads_checkpoint";
    public const string ActiveSelectionEvidence =
        "DESIGN_CONVERSION_MODEL_SERVER_RUNTIME_ACTIVE_SELECTION_DURABLE_STATE_READ";
    public const string FreshLoadEvidence =
        "DESIGN_CONVERSION_MODEL_SERVER_RUNTIME_ACTIVE_CHECKPOINT_FRESH_LOADED";
    public const string PreviousRuntimePreservedEvidence =
        "DESIGN_CONVERSION_MODEL_SERVER_RUNTIME_ACTIVE_LOAD_FAILED_PREVIOUS_RUNTIME_PRESERVED";
    public const string CachedRuntimeReusedEvidence =
        "DESIGN_CONVERSION_MODEL_SERVER_RUNTIME_ACTIVE_CACHE_REUSED_WITHOUT_CHECKPOINT_RELOAD";
    public const string ActiveModelNotReadyErrorCode =
        "DESIGN_CONVERSION_ACTIVE_MODEL_NOT_READY";
    public const string ActiveModelCheckpointRealWeightsRequiredErrorCode =
        "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_REAL_WEIGHTS_REQUIRED";
    private const string ActiveModelCheckpointLoadFailedErrorCode =
        "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_LOAD_FAILED";
    private const int MaxRuntimeCacheEntriesPerTenant = 2;

    private readonly DesignConversionModelActivationRegistry? _registry;
    private readonly IDesignConversionRenderProvider _renderProvider;
    private readonly IDesignConversionComparisonProvider _comparisonProvider;
    private readonly IDesignConversionVisualJudge _visualJudge;
    private readonly IDesignConversionActivatedModelCheckpointLoader _checkpointLoader;
    private readonly object _gate = new();
    private readonly Dictionary<string, List<RuntimeCacheEntry>> _runtimeByTenantRef = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DesignConversionPromotedLocalModel?> _modelsByJobId = new(StringComparer.Ordinal);
    private readonly DesignConversionActivatedDpoPolicyPairRuntimeAssociation
        _pairRuntimeAssociation = new();

    public DesignConversionActivatedModelRuntimeEvaluator(
        DesignConversionModelActivationRegistry? registry,
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge,
        IDesignConversionActivatedModelCheckpointLoader? checkpointLoader = null)
    {
        _registry = registry;
        _renderProvider = renderProvider ?? throw new ArgumentNullException(nameof(renderProvider));
        _comparisonProvider = comparisonProvider ?? throw new ArgumentNullException(nameof(comparisonProvider));
        _visualJudge = visualJudge ?? throw new ArgumentNullException(nameof(visualJudge));
        _checkpointLoader = checkpointLoader ?? new DesignConversionLocalActivatedModelCheckpointLoader();
    }

    public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion activated runtime request is required.",
                "REQUEST_REQUIRED");
        }

        var trustedTenant = ResolveTrustedTenantContext(request.Provenance);
        if (!trustedTenant.IsSuccess)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                trustedTenant.ErrorMessage ?? "DesignConversion active model trusted tenant context is invalid.",
                trustedTenant.ErrorCode ?? "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        }

        var callerTenantClaim = ResolveCallerTenantIdClaim(request.Provenance);
        if (!callerTenantClaim.IsSuccess)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                callerTenantClaim.ErrorMessage ?? "DesignConversion active model tenant claim is invalid.",
                callerTenantClaim.ErrorCode ?? "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        }

        if (!trustedTenant.Value!.TenantBound)
        {
            RememberModel(request.JobId, null);
            return ActiveModelNotReady<DesignConversionInternalEvaluationResult>();
        }

        if (!string.IsNullOrWhiteSpace(callerTenantClaim.Value) &&
            !string.Equals(callerTenantClaim.Value, trustedTenant.Value.TenantId, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion active model tenant claim does not match the admitted tenant context.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_TENANT_CONTEXT_MISMATCH");
        }

        if (_registry is null)
        {
            RememberModel(request.JobId, null);
            return ActiveModelNotReady<DesignConversionInternalEvaluationResult>();
        }

        var active = await _registry.ReadActiveAsync(trustedTenant.Value.TenantId, cancellationToken);
        if (!active.IsSuccess)
        {
            if (string.Equals(active.ErrorCode, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND", StringComparison.Ordinal))
            {
                RememberModel(request.JobId, null);
                return ActiveModelNotReady<DesignConversionInternalEvaluationResult>();
            }

            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion active model runtime selection failed.",
                active.ErrorCode ?? "DESIGN_CONVERSION_ACTIVE_MODEL_RUNTIME_SELECTION_FAILED");
        }

        var selected = active.Value!;
        var runtime = await ResolveRuntimeAsync(selected, cancellationToken);
        if (!runtime.IsSuccess)
            return runtime.MapError<DesignConversionInternalEvaluationResult>();

        RememberModel(request.JobId, runtime.Value!.PromotedLocalModel);
        var evaluated = await runtime.Value.Evaluator.EvaluateAsync(request, cancellationToken);
        if (!evaluated.IsSuccess)
            return evaluated;

        var annotated = Annotate(
            evaluated.Value!,
            selected,
            trustedTenant.Value,
            runtime.Value.PromotedLocalModel,
            runtime.Value.FreshLoadAttempted,
            runtime.Value.LoadedFreshThisCall,
            runtime.Value.PreservedPreviousRuntime,
            runtime.Value.RepositoryFreshLoadVerifiedThisCall);
        _pairRuntimeAssociation.Register(
            annotated,
            request,
            runtime.Value.Evaluator as
                IDesignConversionDpoPolicyPairRuntimeEvaluator);
        return OperationResult<DesignConversionInternalEvaluationResult>.Ok(
            annotated);
    }

    Task<OperationResult<DesignConversionInternalEvaluationResult>>
        IDesignConversionDpoPolicyPairRuntimeEvaluator.ApplyDpoPolicyRuntimePairAsync(
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionDpoPolicyRuntimePair pair,
            CancellationToken cancellationToken)
        => _pairRuntimeAssociation.ApplyAsync(
            evaluation,
            pair,
            cancellationToken);

    public bool TryGetPromotedLocalModel(string jobId, out DesignConversionPromotedLocalModel? promotedLocalModel)
    {
        lock (_gate)
            return _modelsByJobId.TryGetValue(jobId, out promotedLocalModel);
    }

    bool IDesignConversionVerifiedActivatedModelRuntimeReadback.TryGetVerifiedPromotedLocalModel(
        string jobId,
        out DesignConversionPromotedLocalModel? promotedLocalModel)
        => TryGetPromotedLocalModel(jobId, out promotedLocalModel);

    private static OperationResult<T> ActiveModelNotReady<T>()
        => OperationResult<T>.Fail(
            "DesignConversion active model runtime is not ready.",
            ActiveModelNotReadyErrorCode);

    private async Task<OperationResult<ResolvedRuntime>> ResolveRuntimeAsync(
        DesignConversionModelActivationResult active,
        CancellationToken cancellationToken)
    {
        var activeVersion = active.ActiveVersion;
        var tenantRef = active.TenantIdRef;
        RuntimeCacheEntry? cached = null;
        lock (_gate)
            cached = TryFindRuntimeCacheEntryLocked(tenantRef, activeVersion);

        if (cached is not null)
        {
            return OperationResult<ResolvedRuntime>.Ok(new ResolvedRuntime(
                cached.Evaluator,
                cached.PromotedLocalModel,
                FreshLoadAttempted: false,
                LoadedFreshThisCall: false,
                PreservedPreviousRuntime: true,
                RepositoryFreshLoadVerifiedThisCall: false));
        }

        var loaded = await LoadRuntimeAsync(tenantRef, activeVersion, cancellationToken);
        if (loaded.IsSuccess)
        {
            var fresh = loaded.Value!;
            lock (_gate)
                RememberRuntimeCacheEntryLocked(tenantRef, fresh);

            return OperationResult<ResolvedRuntime>.Ok(new ResolvedRuntime(
                fresh.Evaluator,
                fresh.PromotedLocalModel,
                FreshLoadAttempted: true,
                LoadedFreshThisCall: true,
                PreservedPreviousRuntime: false,
                fresh.RepositoryFreshLoadVerifiedThisCall));
        }

        lock (_gate)
            cached = TryFindRuntimeCacheEntryLocked(tenantRef, activeVersion);

        if (cached is not null)
        {
            return OperationResult<ResolvedRuntime>.Ok(new ResolvedRuntime(
                cached.Evaluator,
                cached.PromotedLocalModel,
                FreshLoadAttempted: true,
                LoadedFreshThisCall: false,
                PreservedPreviousRuntime: true,
                RepositoryFreshLoadVerifiedThisCall: false));
        }

        return OperationResult<ResolvedRuntime>.Fail(
            "DesignConversion active model runtime load failed.",
            loaded.ErrorCode ?? "DESIGN_CONVERSION_ACTIVE_MODEL_RUNTIME_LOAD_FAILED");
    }

    private void RememberRuntimeCacheEntryLocked(string tenantRef, RuntimeCacheEntry fresh)
    {
        if (!_runtimeByTenantRef.TryGetValue(tenantRef, out var entries))
        {
            entries = [];
            _runtimeByTenantRef[tenantRef] = entries;
        }

        entries.RemoveAll(entry => entry.Identity.Matches(fresh.Identity));
        entries.Add(fresh);

        while (entries.Count > MaxRuntimeCacheEntriesPerTenant)
            entries.RemoveAt(0);
    }

    private RuntimeCacheEntry? TryFindRuntimeCacheEntryLocked(
        string tenantRef,
        DesignConversionActivatedModelVersion activeVersion)
    {
        if (!_runtimeByTenantRef.TryGetValue(tenantRef, out var entries))
            return null;

        for (var index = entries.Count - 1; index >= 0; index--)
        {
            var cached = entries[index];
            if (CachedRuntimeMatchesActiveSelection(tenantRef, activeVersion, cached))
            {
                if (index != entries.Count - 1)
                {
                    entries.RemoveAt(index);
                    entries.Add(cached);
                }

                return cached;
            }
        }

        return null;
    }

    private async Task<OperationResult<RuntimeCacheEntry>> LoadRuntimeAsync(
        string tenantRef,
        DesignConversionActivatedModelVersion activeVersion,
        CancellationToken cancellationToken)
    {
        var loaded = await _checkpointLoader.LoadAsync(activeVersion, cancellationToken);
        if (!loaded.IsSuccess)
        {
            return OperationResult<RuntimeCacheEntry>.Fail(
                "DesignConversion active model checkpoint could not be loaded.",
                SanitizeCheckpointLoaderErrorCode(loaded.ErrorCode));
        }

        if (!string.Equals(loaded.Value!.CheckpointSha256, activeVersion.CheckpointSha256, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<RuntimeCacheEntry>.Fail(
                "DesignConversion active model checkpoint hash no longer matches activation metadata.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_SHA_MISMATCH");
        }

        var checkpoint = loaded.Value.Checkpoint;
        if (!string.Equals(checkpoint.MachineId, activeVersion.MachineId, StringComparison.Ordinal) ||
            !string.Equals(checkpoint.DomainTag, activeVersion.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(checkpoint.Version, activeVersion.Version, StringComparison.Ordinal))
        {
            return OperationResult<RuntimeCacheEntry>.Fail(
                "DesignConversion active model checkpoint identity does not match activation metadata.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_IDENTITY_MISMATCH");
        }

        var checkpointHasRealWeights = DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint);
        if (!checkpointHasRealWeights)
        {
            return OperationResult<RuntimeCacheEntry>.Fail(
                "DesignConversion active model checkpoint must contain real learned weights.",
                ActiveModelCheckpointRealWeightsRequiredErrorCode);
        }

        var model = new DesignConversionPromotedLocalModel(
            ModelId: activeVersion.ModelId,
            ManifestId: activeVersion.ManifestId,
            ManifestRef: activeVersion.ManifestRef,
            CheckpointPath: activeVersion.CheckpointPath,
            CheckpointSha256: activeVersion.CheckpointSha256,
            MachineId: activeVersion.MachineId,
            DomainTag: activeVersion.DomainTag,
            Version: activeVersion.Version,
            LearnedWeightArtifactCount: DesignConversionMachine.CountRealLearnedWeightArtifacts(checkpoint),
            LearnedWeightObservationCount: DesignConversionMachine.CountRealLearnedWeightObservations(checkpoint),
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: checkpointHasRealWeights,
            PromotionManifestClaimed: true,
            LocalOnly: activeVersion.LocalOnly,
            EvidenceRefs: activeVersion.EvidenceRefs
                .Where(reference => !string.Equals(
                    reference,
                    DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
                    StringComparison.Ordinal))
                .Append(ActiveSelectionEvidence)
                .Append(FreshLoadEvidence)
                .Append(RuntimeSelectionBoundary)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray());

        if (!string.Equals(
                model.LocalCheckpointWeightsSource,
                activeVersion.LocalCheckpointWeightsSource,
                StringComparison.Ordinal))
        {
            return OperationResult<RuntimeCacheEntry>.Fail(
                "DesignConversion active model checkpoint weights source does not match activation metadata.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_WEIGHTS_SOURCE_MISMATCH");
        }

        var repositoryFreshLoadVerifiedThisCall =
            _checkpointLoader.GetType() == typeof(DesignConversionRepositoryActivatedModelCheckpointLoader);

        var service = new DesignConversionInternalService(
            _renderProvider,
            _comparisonProvider,
            _visualJudge,
            loaded.Value.Machine);
        return OperationResult<RuntimeCacheEntry>.Ok(new RuntimeCacheEntry(
            RuntimeCacheIdentity.From(tenantRef, activeVersion),
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            model,
            repositoryFreshLoadVerifiedThisCall));
    }

    private static string SanitizeCheckpointLoaderErrorCode(string? errorCode)
        => string.IsNullOrWhiteSpace(errorCode) ||
           errorCode.Contains(
               DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
               StringComparison.OrdinalIgnoreCase)
            ? ActiveModelCheckpointLoadFailedErrorCode
            : errorCode;

    private static bool CachedRuntimeMatchesActiveSelection(
        string tenantRef,
        DesignConversionActivatedModelVersion activeVersion,
        RuntimeCacheEntry cached)
        => cached.Identity.Matches(tenantRef, activeVersion) &&
           CachedPromotedModelMatchesActiveSelection(cached.PromotedLocalModel, activeVersion);

    private static bool CachedPromotedModelMatchesActiveSelection(
        DesignConversionPromotedLocalModel model,
        DesignConversionActivatedModelVersion activeVersion)
        => string.Equals(model.ModelId, activeVersion.ModelId, StringComparison.Ordinal) &&
           string.Equals(model.ManifestId, activeVersion.ManifestId, StringComparison.Ordinal) &&
           string.Equals(model.ManifestRef, activeVersion.ManifestRef, StringComparison.Ordinal) &&
           string.Equals(model.CheckpointPath, activeVersion.CheckpointPath, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(model.CheckpointSha256, activeVersion.CheckpointSha256, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(model.MachineId, activeVersion.MachineId, StringComparison.Ordinal) &&
           string.Equals(model.DomainTag, activeVersion.DomainTag, StringComparison.Ordinal) &&
           string.Equals(model.Version, activeVersion.Version, StringComparison.Ordinal) &&
           string.Equals(model.LocalCheckpointWeightsSource, activeVersion.LocalCheckpointWeightsSource, StringComparison.Ordinal) &&
           model.CheckpointFreshLoadVerified == activeVersion.CheckpointFreshLoadVerified &&
           model.CheckpointContainsRealTrainedWeights == activeVersion.CheckpointContainsRealTrainedWeights &&
           model.LocalOnly == activeVersion.LocalOnly &&
           model.PromotionManifestClaimed;

    private static DesignConversionInternalEvaluationResult Annotate(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionModelActivationResult active,
        TrustedTenantContext trustedTenant,
        DesignConversionPromotedLocalModel model,
        bool freshLoadAttempted,
        bool loadedFreshThisCall,
        bool preservedPreviousRuntime,
        bool repositoryFreshLoadVerifiedThisCall)
    {
        var runtimeEvidence = loadedFreshThisCall
            ? FreshLoadEvidence
            : freshLoadAttempted
                ? PreviousRuntimePreservedEvidence
                : CachedRuntimeReusedEvidence;
        var evidence = new[]
        {
            ActiveSelectionEvidence,
            RuntimeSelectionBoundary,
            runtimeEvidence
        };
        var repositoryEvidence = repositoryFreshLoadVerifiedThisCall
            ? new[] { DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence }
            : Array.Empty<string>();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["active_model_runtime_selection"] = "durable_registry",
            ["active_model_runtime_selection_boundary"] = RuntimeSelectionBoundary,
            ["active_model_runtime_tenant_context_source"] = "server_owned_route_admission",
            ["active_model_runtime_trusted_tenant_bound"] = bool.TrueString,
            ["active_model_runtime_caller_tenant_metadata_authoritative"] = bool.FalseString,
            ["active_model_runtime_fresh_load_attempted"] = freshLoadAttempted.ToString(),
            ["active_model_runtime_fresh_load_succeeded"] = loadedFreshThisCall.ToString(),
            ["active_model_runtime_previous_preserved"] = preservedPreviousRuntime.ToString(),
            ["active_model_generation"] = active.Generation.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["active_model_tenant_id_ref"] = active.TenantIdRef,
            ["active_model_admitted_tenant_id_ref"] = trustedTenant.TenantIdRef,
            ["active_model_admitted_scope_ref"] = trustedTenant.ScopeRef,
            ["active_model_id"] = model.ModelId,
            ["active_model_manifest_id"] = model.ManifestId,
            ["active_model_version"] = model.Version,
            ["active_model_checkpoint_sha256"] = model.CheckpointSha256,
            ["active_model_checkpoint_path_ref"] = active.ActiveVersion.CheckpointPathRef,
            ["active_model_checkpoint_weights_source"] = model.LocalCheckpointWeightsSource,
            ["active_model_checkpoint_fresh_load_verified"] = model.CheckpointFreshLoadVerified.ToString(),
            ["active_model_checkpoint_real_weights"] = model.CheckpointContainsRealTrainedWeights.ToString(),
            ["active_model_local_only"] = model.LocalOnly.ToString(),
            ["product_ready"] = bool.FalseString
        };

        return evaluation with
        {
            EvidenceRefs = evaluation.EvidenceRefs
                .Concat(evidence)
                .Concat(model.EvidenceRefs)
                .Where(reference => !string.Equals(
                    reference,
                    DesignConversionRepositoryActivatedModelCheckpointLoader.RepositoryFreshLoadEvidence,
                    StringComparison.Ordinal))
                .Concat(repositoryEvidence)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            Warnings = evaluation.Warnings
                .Append($"active_model_runtime_selection=durable_registry")
                .Append($"active_model_runtime_fresh_load_attempted={freshLoadAttempted}")
                .Append($"active_model_runtime_fresh_load_succeeded={loadedFreshThisCall}")
                .Append($"active_model_runtime_previous_preserved={preservedPreviousRuntime}")
                .Append("product_ready=false")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray(),
            ScaffoldMarkers = MergeMetadata(evaluation.ScaffoldMarkers, metadata)
        };
    }

    private static OperationResult<TrustedTenantContext> ResolveTrustedTenantContext(
        IReadOnlyDictionary<string, string>? provenance)
    {
        if (!IsTruthy(provenance, DesignConversionModelServerApi.ServerOwnedRouteAdmissionTenantBoundProvenanceKey))
            return OperationResult<TrustedTenantContext>.Ok(TrustedTenantContext.Unbound);

        if (!TryReadMetadata(
                provenance,
                DesignConversionModelServerApi.ServerOwnedRouteAdmissionTenantIdProvenanceKey,
                out var tenantId) ||
            !TryReadMetadata(
                provenance,
                DesignConversionModelServerApi.ServerOwnedRouteAdmissionTenantIdRefProvenanceKey,
                out var tenantIdRef))
        {
            return OperationResult<TrustedTenantContext>.Fail(
                "DesignConversion active model trusted tenant context is missing.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        }

        var tenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(tenantId);
        if (!tenant.IsSuccess ||
            !string.Equals(tenant.Value!.TenantIdRef, tenantIdRef, StringComparison.Ordinal))
        {
            return OperationResult<TrustedTenantContext>.Fail(
                "DesignConversion active model trusted tenant context ref does not match.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        }

        TryReadMetadata(
            provenance,
            DesignConversionModelServerApi.ServerOwnedRouteAdmissionScopeRefProvenanceKey,
            out var scopeRef);

        return OperationResult<TrustedTenantContext>.Ok(new TrustedTenantContext(
            TenantBound: true,
            TenantId: tenant.Value.TenantId,
            TenantIdRef: tenant.Value.TenantIdRef,
            ScopeRef: scopeRef));
    }

    private static OperationResult<string> ResolveCallerTenantIdClaim(IReadOnlyDictionary<string, string>? provenance)
    {
        foreach (var key in new[]
        {
            "request_metadata:tenant_id",
            "request_metadata:tenantId",
            "request_metadata:tenant",
            "request_metadata:x-tenant-id",
            "budget_metadata:tenant_id",
            "budget_metadata:tenantId"
        })
        {
            if (TryReadMetadata(provenance, key, out var value))
            {
                var tenant = DesignConversionModelActivationRegistry.TryCanonicalizeTenantId(value);
                if (!tenant.IsSuccess)
                {
                    return OperationResult<string>.Fail(
                        "DesignConversion active model tenant claim is invalid.",
                        "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
                }

                return tenant;
            }
        }

        return OperationResult<string>.Ok(string.Empty);
    }

    private static bool TryReadMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        out string value)
    {
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (string.Equals(pair.Key, key, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(pair.Value))
            {
                value = pair.Value.Trim();
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool IsTruthy(IReadOnlyDictionary<string, string>? metadata, string key)
        => TryReadMetadata(metadata, key, out var value) &&
           bool.TryParse(value, out var parsed) &&
           parsed;

    private void RememberModel(string jobId, DesignConversionPromotedLocalModel? model)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return;

        lock (_gate)
            _modelsByJobId[jobId] = model;
    }

    private static IReadOnlyDictionary<string, string> MergeMetadata(
        IReadOnlyDictionary<string, string>? first,
        IReadOnlyDictionary<string, string> second)
        => (first ?? new Dictionary<string, string>())
            .Concat(second)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    private sealed record RuntimeCacheIdentity(
        string TenantIdRef,
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
        string LocalCheckpointWeightsSource,
        bool CheckpointFreshLoadVerified,
        bool CheckpointContainsRealTrainedWeights,
        bool LocalOnly)
    {
        public static RuntimeCacheIdentity From(
            string tenantRef,
            DesignConversionActivatedModelVersion activeVersion)
            => new(
                TenantIdRef: tenantRef,
                ActivationRef: activeVersion.ActivationRef,
                ModelId: activeVersion.ModelId,
                ManifestId: activeVersion.ManifestId,
                ManifestRef: activeVersion.ManifestRef,
                PromotionUploadRef: activeVersion.PromotionUploadRef,
                Version: activeVersion.Version,
                MachineId: activeVersion.MachineId,
                DomainTag: activeVersion.DomainTag,
                CheckpointPath: activeVersion.CheckpointPath,
                CheckpointPathRef: activeVersion.CheckpointPathRef,
                CheckpointSha256: activeVersion.CheckpointSha256,
                LocalCheckpointWeightsSource: activeVersion.LocalCheckpointWeightsSource,
                CheckpointFreshLoadVerified: activeVersion.CheckpointFreshLoadVerified,
                CheckpointContainsRealTrainedWeights: activeVersion.CheckpointContainsRealTrainedWeights,
                LocalOnly: activeVersion.LocalOnly);

        public bool Matches(
            string tenantRef,
            DesignConversionActivatedModelVersion activeVersion)
            => string.Equals(TenantIdRef, tenantRef, StringComparison.Ordinal) &&
               string.Equals(ActivationRef, activeVersion.ActivationRef, StringComparison.Ordinal) &&
               string.Equals(ModelId, activeVersion.ModelId, StringComparison.Ordinal) &&
               string.Equals(ManifestId, activeVersion.ManifestId, StringComparison.Ordinal) &&
               string.Equals(ManifestRef, activeVersion.ManifestRef, StringComparison.Ordinal) &&
               string.Equals(PromotionUploadRef, activeVersion.PromotionUploadRef, StringComparison.Ordinal) &&
               string.Equals(Version, activeVersion.Version, StringComparison.Ordinal) &&
               string.Equals(MachineId, activeVersion.MachineId, StringComparison.Ordinal) &&
               string.Equals(DomainTag, activeVersion.DomainTag, StringComparison.Ordinal) &&
               string.Equals(CheckpointPath, activeVersion.CheckpointPath, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(CheckpointPathRef, activeVersion.CheckpointPathRef, StringComparison.Ordinal) &&
               string.Equals(CheckpointSha256, activeVersion.CheckpointSha256, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(LocalCheckpointWeightsSource, activeVersion.LocalCheckpointWeightsSource, StringComparison.Ordinal) &&
               CheckpointFreshLoadVerified == activeVersion.CheckpointFreshLoadVerified &&
               CheckpointContainsRealTrainedWeights == activeVersion.CheckpointContainsRealTrainedWeights &&
               LocalOnly == activeVersion.LocalOnly;

        public bool Matches(RuntimeCacheIdentity other)
            => string.Equals(TenantIdRef, other.TenantIdRef, StringComparison.Ordinal) &&
               string.Equals(ActivationRef, other.ActivationRef, StringComparison.Ordinal) &&
               string.Equals(ModelId, other.ModelId, StringComparison.Ordinal) &&
               string.Equals(ManifestId, other.ManifestId, StringComparison.Ordinal) &&
               string.Equals(ManifestRef, other.ManifestRef, StringComparison.Ordinal) &&
               string.Equals(PromotionUploadRef, other.PromotionUploadRef, StringComparison.Ordinal) &&
               string.Equals(Version, other.Version, StringComparison.Ordinal) &&
               string.Equals(MachineId, other.MachineId, StringComparison.Ordinal) &&
               string.Equals(DomainTag, other.DomainTag, StringComparison.Ordinal) &&
               string.Equals(CheckpointPath, other.CheckpointPath, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(CheckpointPathRef, other.CheckpointPathRef, StringComparison.Ordinal) &&
               string.Equals(CheckpointSha256, other.CheckpointSha256, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(LocalCheckpointWeightsSource, other.LocalCheckpointWeightsSource, StringComparison.Ordinal) &&
               CheckpointFreshLoadVerified == other.CheckpointFreshLoadVerified &&
               CheckpointContainsRealTrainedWeights == other.CheckpointContainsRealTrainedWeights &&
               LocalOnly == other.LocalOnly;
    }

    private sealed record RuntimeCacheEntry(
        RuntimeCacheIdentity Identity,
        IDesignConversionThinClientJobEvaluator Evaluator,
        DesignConversionPromotedLocalModel PromotedLocalModel,
        bool RepositoryFreshLoadVerifiedThisCall);

    private sealed record ResolvedRuntime(
        IDesignConversionThinClientJobEvaluator Evaluator,
        DesignConversionPromotedLocalModel PromotedLocalModel,
        bool FreshLoadAttempted,
        bool LoadedFreshThisCall,
        bool PreservedPreviousRuntime,
        bool RepositoryFreshLoadVerifiedThisCall);

    private sealed record TrustedTenantContext(
        bool TenantBound,
        string TenantId,
        string TenantIdRef,
        string ScopeRef)
    {
        public static TrustedTenantContext Unbound { get; } = new(
            TenantBound: false,
            TenantId: string.Empty,
            TenantIdRef: string.Empty,
            ScopeRef: string.Empty);
    }
}

/// <summary>
/// Binds a coordinator pair continuation to the exact annotated evaluation
/// object returned by the selected activated runtime. Keys and evaluators are
/// weak so an unconsumed continuation cannot retain an evicted runtime.
/// </summary>
internal sealed class DesignConversionActivatedDpoPolicyPairRuntimeAssociation
{
    private readonly object _gate = new();
    private readonly ConditionalWeakTable<
        DesignConversionInternalEvaluationResult,
        Entry> _entries = new();

    internal void Register(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionInternalEvaluateRequest request,
        IDesignConversionDpoPolicyPairRuntimeEvaluator? evaluator)
    {
        if (evaluation is null || request is null || evaluator is null ||
            !Entry.TryCreate(evaluation, request, evaluator, out var entry))
        {
            return;
        }

        lock (_gate)
        {
            _entries.Remove(evaluation);
            _entries.Add(evaluation, entry!);
        }
    }

    internal Task<OperationResult<DesignConversionInternalEvaluationResult>>
        ApplyAsync(
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionDpoPolicyRuntimePair pair,
            CancellationToken cancellationToken = default)
    {
        if (evaluation is null || pair is null)
            return PairUnavailable();

        Entry? entry;
        lock (_gate)
        {
            if (!_entries.TryGetValue(evaluation, out entry))
                return PairUnavailable();

            // Possession of the exact result object is a one-shot capability.
            // Burn it before validating/forwarding so a failed or racing claim
            // cannot probe and then replay the selected evaluator.
            _entries.Remove(evaluation);
        }

        if (!entry.Matches(evaluation, pair) ||
            !entry.Evaluator.TryGetTarget(out var evaluator))
        {
            return PairUnavailable();
        }

        return evaluator.ApplyDpoPolicyRuntimePairAsync(
            evaluation,
            pair,
            cancellationToken);
    }

    private static Task<OperationResult<DesignConversionInternalEvaluationResult>>
        PairUnavailable()
        => Task.FromResult(
            OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "The coordinator-owned DPO runtime pair is unavailable.",
                DesignConversionMachine
                    .DpoPolicyRuntimePairContentUnavailableCode));

    private sealed record Entry(
        WeakReference<IDesignConversionDpoPolicyPairRuntimeEvaluator> Evaluator,
        string JobId,
        string RequestId,
        string RootOwnershipBinding)
    {
        internal static bool TryCreate(
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionInternalEvaluateRequest request,
            IDesignConversionDpoPolicyPairRuntimeEvaluator evaluator,
            out Entry? entry)
        {
            entry = null;
            var submitted = evaluation.SubmittedRequest;
            if (submitted is null ||
                string.IsNullOrWhiteSpace(evaluation.JobId) ||
                string.IsNullOrWhiteSpace(submitted.RequestId) ||
                !string.Equals(request.JobId, evaluation.JobId,
                    StringComparison.Ordinal) ||
                !string.Equals(request.RequestId, submitted.RequestId,
                    StringComparison.Ordinal) ||
                !TryReadExactBinding(
                    request.Provenance,
                    DesignConversionDpoPolicyRuntimePairContract
                        .RootOwnershipBindingProvenanceKey,
                    out var requestRootBinding) ||
                !TryReadExactBinding(
                    submitted.Provenance,
                    DesignConversionDpoPolicyRuntimePairContract
                        .RootOwnershipBindingProvenanceKey,
                    out var submittedRootBinding) ||
                !string.Equals(requestRootBinding, submittedRootBinding,
                    StringComparison.Ordinal) ||
                !TryReadExactBinding(
                    submitted.Provenance,
                    DesignConversionDpoPolicyRuntimePairContract
                        .SubmittedRequestJobIdProvenanceKey,
                    out var submittedJobId) ||
                !string.Equals(submittedJobId, evaluation.JobId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            entry = new Entry(
                new WeakReference<
                    IDesignConversionDpoPolicyPairRuntimeEvaluator>(evaluator),
                evaluation.JobId,
                submitted.RequestId,
                requestRootBinding);
            return true;
        }

        internal bool Matches(
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionDpoPolicyRuntimePair pair)
        {
            var generated = pair.Generated;
            var sourcePair = pair.SourcePair;
            var expectedRootBinding =
                DesignConversionDpoLeakageGuard.HashRef(
                    "dcm-runtime-rejected-root-ownership",
                    string.Concat(
                        pair.RootJobId,
                        "|",
                        pair.RootRequestId));
            return evaluation.SubmittedRequest is { } submitted &&
                   generated is not null &&
                   sourcePair is not null &&
                   ReferenceEquals(
                       sourcePair.GeneratedEvaluation,
                       evaluation) &&
                   string.Equals(evaluation.JobId, JobId,
                       StringComparison.Ordinal) &&
                   string.Equals(submitted.RequestId, RequestId,
                       StringComparison.Ordinal) &&
                   string.Equals(generated.EvaluationJobId, JobId,
                       StringComparison.Ordinal) &&
                   string.Equals(generated.EvaluationRequestId, RequestId,
                       StringComparison.Ordinal) &&
                   string.Equals(pair.RootOwnershipBinding,
                       RootOwnershipBinding, StringComparison.Ordinal) &&
                   string.Equals(expectedRootBinding,
                       RootOwnershipBinding, StringComparison.Ordinal) &&
                   TryReadExactBinding(
                       submitted.Provenance,
                       DesignConversionDpoPolicyRuntimePairContract
                           .RootOwnershipBindingProvenanceKey,
                       out var submittedRootBinding) &&
                   string.Equals(submittedRootBinding,
                       RootOwnershipBinding, StringComparison.Ordinal) &&
                   TryReadExactBinding(
                       submitted.Provenance,
                       DesignConversionDpoPolicyRuntimePairContract
                           .SubmittedRequestJobIdProvenanceKey,
                       out var submittedJobId) &&
                   string.Equals(submittedJobId, JobId,
                       StringComparison.Ordinal);
        }

        private static bool TryReadExactBinding(
            IReadOnlyDictionary<string, string>? provenance,
            string key,
            out string value)
        {
            if (provenance is not null &&
                provenance.TryGetValue(key, out var candidate) &&
                !string.IsNullOrWhiteSpace(candidate))
            {
                value = candidate;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
