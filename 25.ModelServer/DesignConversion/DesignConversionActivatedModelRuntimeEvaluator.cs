// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using AgentInfrastructure;
using DesignConversionDomain;

public interface IDesignConversionActivatedModelRuntimeReadback
{
    bool TryGetPromotedLocalModel(string jobId, out DesignConversionPromotedLocalModel? promotedLocalModel);
}

public sealed class DesignConversionActivatedModelRuntimeEvaluator : IDesignConversionThinClientJobEvaluator,
    IDesignConversionActivatedModelRuntimeReadback
{
    public const string RuntimeSelectionBoundary =
        "design_conversion_modelserver_runtime_selection_reads_durable_active_model_and_fresh_loads_checkpoint";
    public const string ActiveSelectionEvidence =
        "DESIGN_CONVERSION_MODEL_SERVER_RUNTIME_ACTIVE_SELECTION_DURABLE_STATE_READ";
    public const string FreshLoadEvidence =
        "DESIGN_CONVERSION_MODEL_SERVER_RUNTIME_ACTIVE_CHECKPOINT_FRESH_LOADED";
    public const string PreviousRuntimePreservedEvidence =
        "DESIGN_CONVERSION_MODEL_SERVER_RUNTIME_ACTIVE_LOAD_FAILED_PREVIOUS_RUNTIME_PRESERVED";
    public const string NoActiveSelectionEvidence =
        "DESIGN_CONVERSION_MODEL_SERVER_RUNTIME_NO_ACTIVE_SELECTION_FALLBACK_USED";
    private const int MaxRuntimeCacheEntriesPerTenant = 2;

    private readonly IDesignConversionThinClientJobEvaluator _fallbackEvaluator;
    private readonly DesignConversionModelActivationRegistry _registry;
    private readonly IDesignConversionRenderProvider _renderProvider;
    private readonly IDesignConversionComparisonProvider _comparisonProvider;
    private readonly IDesignConversionVisualJudge _visualJudge;
    private readonly object _gate = new();
    private readonly Dictionary<string, List<RuntimeCacheEntry>> _runtimeByTenantRef = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DesignConversionPromotedLocalModel?> _modelsByJobId = new(StringComparer.Ordinal);

    public DesignConversionActivatedModelRuntimeEvaluator(
        IDesignConversionThinClientJobEvaluator fallbackEvaluator,
        DesignConversionModelActivationRegistry registry,
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge)
    {
        _fallbackEvaluator = fallbackEvaluator ?? throw new ArgumentNullException(nameof(fallbackEvaluator));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _renderProvider = renderProvider ?? throw new ArgumentNullException(nameof(renderProvider));
        _comparisonProvider = comparisonProvider ?? throw new ArgumentNullException(nameof(comparisonProvider));
        _visualJudge = visualJudge ?? throw new ArgumentNullException(nameof(visualJudge));
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
            return await EvaluateFallbackAsync(request, NoActiveSelectionEvidence, promotedLocalModel: null, cancellationToken);

        if (!string.IsNullOrWhiteSpace(callerTenantClaim.Value) &&
            !string.Equals(callerTenantClaim.Value, trustedTenant.Value.TenantId, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion active model tenant claim does not match the admitted tenant context.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_TENANT_CONTEXT_MISMATCH");
        }

        var active = await _registry.ReadActiveAsync(trustedTenant.Value.TenantId, cancellationToken);
        if (!active.IsSuccess)
        {
            if (string.Equals(active.ErrorCode, "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND", StringComparison.Ordinal))
                return await EvaluateFallbackAsync(request, NoActiveSelectionEvidence, promotedLocalModel: null, cancellationToken);

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
        return evaluated.IsSuccess
            ? OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                Annotate(
                    evaluated.Value!,
                    selected,
                    trustedTenant.Value,
                    runtime.Value.PromotedLocalModel,
                    runtime.Value.LoadedFreshThisCall,
                    runtime.Value.PreservedPreviousRuntime))
            : evaluated;
    }

    public bool TryGetPromotedLocalModel(string jobId, out DesignConversionPromotedLocalModel? promotedLocalModel)
    {
        lock (_gate)
            return _modelsByJobId.TryGetValue(jobId, out promotedLocalModel);
    }

    private async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateFallbackAsync(
        DesignConversionInternalEvaluateRequest request,
        string evidenceRef,
        DesignConversionPromotedLocalModel? promotedLocalModel,
        CancellationToken cancellationToken)
    {
        RememberModel(request.JobId, promotedLocalModel);
        var evaluated = await _fallbackEvaluator.EvaluateAsync(request, cancellationToken);
        if (!evaluated.IsSuccess)
            return evaluated;

        return OperationResult<DesignConversionInternalEvaluationResult>.Ok(evaluated.Value! with
        {
            EvidenceRefs = evaluated.Value.EvidenceRefs
                .Append(evidenceRef)
                .Append(RuntimeSelectionBoundary)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            Warnings = evaluated.Value.Warnings
                .Append("active_model_runtime_selection=fallback")
                .Append("product_ready=false")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray(),
            ScaffoldMarkers = MergeMetadata(
                evaluated.Value.ScaffoldMarkers,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["active_model_runtime_selection"] = "fallback",
                    ["active_model_runtime_selection_boundary"] = RuntimeSelectionBoundary,
                    ["active_model_runtime_trusted_tenant_bound"] = bool.FalseString,
                    ["active_model_runtime_caller_tenant_metadata_authoritative"] = bool.FalseString,
                    ["active_model_runtime_fresh_load_attempted"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString
                })
        });
    }

    private async Task<OperationResult<ResolvedRuntime>> ResolveRuntimeAsync(
        DesignConversionModelActivationResult active,
        CancellationToken cancellationToken)
    {
        var activeVersion = active.ActiveVersion;
        var tenantRef = active.TenantIdRef;
        var loaded = await LoadRuntimeAsync(tenantRef, activeVersion, cancellationToken);
        if (loaded.IsSuccess)
        {
            var fresh = loaded.Value!;
            lock (_gate)
                RememberRuntimeCacheEntryLocked(tenantRef, fresh);

            return OperationResult<ResolvedRuntime>.Ok(new ResolvedRuntime(
                fresh.Evaluator,
                fresh.PromotedLocalModel,
                LoadedFreshThisCall: true,
                PreservedPreviousRuntime: false));
        }

        RuntimeCacheEntry? cached = null;
        lock (_gate)
            cached = TryFindRuntimeCacheEntryLocked(tenantRef, activeVersion);

        if (cached is not null)
        {
            return OperationResult<ResolvedRuntime>.Ok(new ResolvedRuntime(
                cached.Evaluator,
                cached.PromotedLocalModel,
                LoadedFreshThisCall: false,
                PreservedPreviousRuntime: true));
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
                return cached;
        }

        return null;
    }

    private async Task<OperationResult<RuntimeCacheEntry>> LoadRuntimeAsync(
        string tenantRef,
        DesignConversionActivatedModelVersion activeVersion,
        CancellationToken cancellationToken)
    {
        var loaded = await DesignConversionMachine.LoadCheckpointAsync(activeVersion.CheckpointPath, cancellationToken);
        if (!loaded.IsSuccess)
        {
            return OperationResult<RuntimeCacheEntry>.Fail(
                "DesignConversion active model checkpoint could not be loaded.",
                loaded.ErrorCode ?? "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_LOAD_FAILED");
        }

        if (!string.Equals(loaded.Value!.CheckpointSha256, activeVersion.CheckpointSha256, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<RuntimeCacheEntry>.Fail(
                "DesignConversion active model checkpoint hash no longer matches activation metadata.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_CHECKPOINT_SHA_MISMATCH");
        }

        var checkpoint = loaded.Value.Checkpoint;
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
            CheckpointContainsRealTrainedWeights: DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint),
            PromotionManifestClaimed: true,
            LocalOnly: activeVersion.LocalOnly,
            EvidenceRefs: activeVersion.EvidenceRefs
                .Append(ActiveSelectionEvidence)
                .Append(FreshLoadEvidence)
                .Append(RuntimeSelectionBoundary)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray());

        var service = new DesignConversionInternalService(
            _renderProvider,
            _comparisonProvider,
            _visualJudge,
            loaded.Value.Machine);
        return OperationResult<RuntimeCacheEntry>.Ok(new RuntimeCacheEntry(
            RuntimeCacheIdentity.From(tenantRef, activeVersion),
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            model));
    }

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
           model.CheckpointFreshLoadVerified == activeVersion.CheckpointFreshLoadVerified &&
           model.CheckpointContainsRealTrainedWeights == activeVersion.CheckpointContainsRealTrainedWeights &&
           model.LocalOnly == activeVersion.LocalOnly &&
           model.PromotionManifestClaimed;

    private static DesignConversionInternalEvaluationResult Annotate(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionModelActivationResult active,
        TrustedTenantContext trustedTenant,
        DesignConversionPromotedLocalModel model,
        bool loadedFreshThisCall,
        bool preservedPreviousRuntime)
    {
        var evidence = new[]
        {
            ActiveSelectionEvidence,
            RuntimeSelectionBoundary,
            loadedFreshThisCall ? FreshLoadEvidence : PreviousRuntimePreservedEvidence
        };
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["active_model_runtime_selection"] = "durable_registry",
            ["active_model_runtime_selection_boundary"] = RuntimeSelectionBoundary,
            ["active_model_runtime_tenant_context_source"] = "server_owned_route_admission",
            ["active_model_runtime_trusted_tenant_bound"] = bool.TrueString,
            ["active_model_runtime_caller_tenant_metadata_authoritative"] = bool.FalseString,
            ["active_model_runtime_fresh_load_attempted"] = bool.TrueString,
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
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            Warnings = evaluation.Warnings
                .Append($"active_model_runtime_selection=durable_registry")
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
               CheckpointFreshLoadVerified == other.CheckpointFreshLoadVerified &&
               CheckpointContainsRealTrainedWeights == other.CheckpointContainsRealTrainedWeights &&
               LocalOnly == other.LocalOnly;
    }

    private sealed record RuntimeCacheEntry(
        RuntimeCacheIdentity Identity,
        IDesignConversionThinClientJobEvaluator Evaluator,
        DesignConversionPromotedLocalModel PromotedLocalModel);

    private sealed record ResolvedRuntime(
        IDesignConversionThinClientJobEvaluator Evaluator,
        DesignConversionPromotedLocalModel PromotedLocalModel,
        bool LoadedFreshThisCall,
        bool PreservedPreviousRuntime);

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
