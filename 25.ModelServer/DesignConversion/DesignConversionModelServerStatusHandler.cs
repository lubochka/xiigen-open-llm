// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerStatusArtifactRef(
    string ArtifactId,
    string Kind,
    string Uri,
    string Sha256,
    string MediaType,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionModelServerStatusReadbackResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    string ApiShape,
    string RuntimeSurface,
    int StatusCode,
    string JobId,
    string RequestId,
    string ContractStatus,
    string JobStatus,
    string StatusUri,
    bool LearnedInferenceApplied,
    bool TrainedModelAuthority,
    bool NotTrainedModelAuthority,
    bool DeterministicLogicClaimedAsLearning,
    bool PublicHttpEndpointVerified,
    bool PlatformIntegrationVerified,
    bool DeploymentVerified,
    bool ProductReady,
    IReadOnlyList<DesignConversionModelServerStatusArtifactRef> Artifacts,
    DesignConversionThinClientStatusQualityMetrics QualityMetrics,
    IReadOnlyList<DesignConversionThinClientStatusDefect> Defects,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerStatusHandler
{
    public const string EndpointBasePath = "/api/design-conversion/status";
    public const string EndpointPathPrefix = EndpointBasePath + "/";
    public const string EndpointTemplatePath = EndpointPathPrefix + "{jobId}";
    public const string LocalStatusReadbackBoundary =
        "local_status_readback_route_refs_only_no_socket_no_public_deployment";

    private readonly DesignConversionModelServerApi _api;
    private readonly ArtifactAccessPolicy _artifactAccessPolicy;
    private readonly Func<DateTimeOffset> _utcNow;
    private const string ArtifactStoreRefPrefix = "artifact-store/";
    private const string DeniedArtifactAccessRefPrefix = "artifact-access-denied:";
    private const string UnboundStatusTenantId = "status-unbound-tenant";
    private const string MissingArtifactOwnerTenantId = "status-artifact-owner-missing";
    private const string ArtifactAccessPolicyTenantIdMetadataKey = "artifact_access_policy_tenant_id";
    private const string ArtifactAccessPolicyTenantIdRefMetadataKey = "artifact_access_policy_tenant_id_ref";
    private const string ArtifactAccessPolicyJobIdMetadataKey = "artifact_access_policy_job_id";
    private const string ArtifactAccessPolicyArtifactRefIdMetadataKey = "artifact_access_policy_ref_id";
    private const string ArtifactAccessPolicyPurposeMetadataKey = "artifact_access_policy_purpose";
    private const string ArtifactAccessPolicyClassificationMetadataKey = "artifact_access_policy_classification";
    private const string ArtifactAccessPolicyStatusMetadataKey = "artifact_access_policy_status";
    private const string ArtifactAccessPolicyRetentionExpiresAtUtcMetadataKey =
        "artifact_access_policy_retention_expires_at_utc";
    private static readonly TimeSpan StatusReadbackArtifactRetention = TimeSpan.FromDays(7);

    public DesignConversionModelServerStatusHandler(
        DesignConversionModelServerApi api,
        DesignConversionModelServerRouteAdmission? routeAdmission = null,
        ArtifactAccessPolicy? artifactAccessPolicy = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _artifactAccessPolicy = artifactAccessPolicy ?? new ArtifactAccessPolicy();
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        RouteAdmission = routeAdmission ?? DesignConversionModelServerRouteAdmission.Create(null);
    }

    public DesignConversionModelServerRouteAdmission RouteAdmission { get; }

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Error("GET", EndpointTemplatePath, 400, "REQUEST_REQUIRED", "DesignConversion status/readback request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!IsStatusReadbackPath(path))
            return Error(method, path, 404, "ROUTE_NOT_FOUND", $"DesignConversion status/readback route is not registered: {path}");

        if (!string.Equals(method, "GET", StringComparison.Ordinal))
            return Error(method, path, 405, "METHOD_NOT_ALLOWED", "DesignConversion status/readback endpoint requires GET.");

        var admission = RouteAdmission.AdmitStatus(request);
        if (!admission.IsAccepted)
        {
            return Error(
                method,
                EndpointTemplatePath,
                admission.StatusCode,
                admission.ErrorCode,
                admission.ErrorMessage,
                SanitizeStatusDenialAdmissionMetadata(admission.BoundaryMetadata));
        }

        var jobId = ExtractJobId(path);
        var statusJobAllowed = RouteAdmission.IsStatusJobAllowed(admission, jobId);
        var result = await _api.GetJobResultAsync(jobId, cancellationToken);
        if (!statusJobAllowed &&
            !CanUseLocalArtifactDbRestoredStatusForAdmission(result, admission))
        {
            return Error(
                method,
                EndpointTemplatePath,
                404,
                "JOB_NOT_FOUND",
                "DesignConversion status/readback rejected the job id.",
                SanitizeStatusDenialAdmissionMetadata(admission.BoundaryMetadata));
        }

        if (!result.IsSuccess)
        {
            var publicErrorPath = IsUnexpectedStatusFailureCode(result.ErrorCode)
                ? EndpointTemplatePath
                : path;
            return Error(
                method,
                publicErrorPath,
                StatusCodeForApiError(result.ErrorCode),
                result.ErrorCode ?? "DESIGN_CONVERSION_STATUS_READBACK_FAILED",
                result.ErrorMessage ?? "DesignConversion status/readback rejected the job id.",
                SanitizeStatusDenialAdmissionMetadata(admission.BoundaryMetadata));
        }

        var value = result.Value!;
        var runtimeStatus = value.RuntimeStatus;
        var defects = (runtimeStatus.Defects ?? [])
            .Select(ToResponseDefect)
            .ToArray();
        var qualityMetrics = ToResponseQualityMetrics(runtimeStatus.QualityMetrics, defects.Length);
        var artifactAccessNow = _utcNow();
        var responseArtifacts = runtimeStatus.Artifacts
            .Select(artifact => ToResponseArtifact(artifact, runtimeStatus.Job, admission, artifactAccessNow))
            .ToArray();
        var metadata = BuildBoundaryMetadata(
            method,
            path,
            200,
            MergeBoundaryMetadata(admission.BoundaryMetadata, value.BoundaryMetadata),
            value.EvidenceRefs,
            responseArtifacts.Length);
        var warnings = runtimeStatus.Warnings
            .Concat([
                "status_readback_route_local_only=true",
                "job_rerun_invoked=false",
                "runtime_status_readback_invoked=true",
                "public_http_endpoint_verified=false",
                "platform_integration_verified=false",
                "deployment_verified=false",
                "product_ready=false"
            ])
            .Distinct(StringComparer.Ordinal)
            .OrderBy(warning => warning, StringComparer.Ordinal)
            .ToArray();
        var body = new DesignConversionModelServerStatusReadbackResponse(
            Endpoint: EndpointTemplatePath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ApiShape: value.ApiShape,
            RuntimeSurface: DesignConversionModelServerApi.RuntimeSurface,
            StatusCode: 200,
            JobId: value.JobId,
            RequestId: runtimeStatus.Job.RequestId,
            ContractStatus: value.ContractStatus,
            JobStatus: runtimeStatus.Job.Status,
            StatusUri: runtimeStatus.Job.StatusUri,
            LearnedInferenceApplied: value.LearnedInferenceApplied,
            TrainedModelAuthority: value.TrainedModelAuthority,
            NotTrainedModelAuthority: value.NotTrainedModelAuthority,
            DeterministicLogicClaimedAsLearning: value.DeterministicLogicClaimedAsLearning,
            PublicHttpEndpointVerified: false,
            PlatformIntegrationVerified: false,
            DeploymentVerified: false,
            ProductReady: false,
            Artifacts: responseArtifacts,
            QualityMetrics: qualityMetrics,
            Defects: defects,
            EvidenceRefs: value.EvidenceRefs,
            Warnings: warnings,
            BoundaryMetadata: metadata);

        return Json(200, method, path, body);
    }

    private DesignConversionModelServerStatusArtifactRef ToResponseArtifact(
        DesignConversionThinClientArtifactRef artifact,
        DesignConversionThinClientJob job,
        DesignConversionModelServerRouteAdmissionDecision admission,
        DateTimeOffset nowUtc)
    {
        var routeTenant = ResolveRoutePolicyTenant(admission);
        var requestJobId = ToPolicyToken("status-artifact-job", job.JobId);
        var requestArtifactRefId = ToPolicyToken("status-artifact-ref", artifact.ArtifactId);
        var storedArtifact = BuildPolicyStoredArtifact(
            artifact,
            routeTenant,
            requestJobId,
            requestArtifactRefId,
            nowUtc);
        var requestTenantId = routeTenant.ResolveRequestTenantId(storedArtifact.TenantId);
        var request = new ArtifactAccessPolicyReadRequest(
            CorrelationId: ToPolicyToken("status-artifact-correlation", job.RequestId),
            TenantId: requestTenantId,
            JobId: requestJobId,
            ArtifactRefId: requestArtifactRefId,
            CallerRole: ArtifactAccessPolicy.TenantUserRole);
        var access = _artifactAccessPolicy.TryAuthorizeRead(request, storedArtifact, out var audit, nowUtc);
        var uri = access.IsSuccess && access.Value is not null
            ? access.Value.BrokeredAccessRef
            : BuildDeniedArtifactAccessRef(audit);

        return new(
            ArtifactId: SanitizeStatusReference(artifact.ArtifactId, "status-artifact-id"),
            Kind: SanitizeStatusReference(artifact.Kind, "status-artifact-kind"),
            Uri: SanitizeStatusReference(uri, "status-artifact-uri"),
            Sha256: SanitizeStatusReference(artifact.Sha256, "status-artifact-sha256"),
            MediaType: SanitizeStatusReference(artifact.MediaType, "status-artifact-media-type"),
            Metadata: BuildArtifactAccessResponseMetadata(artifact.Metadata, access.Value, audit));
    }

    private static ArtifactAccessPolicyStoredArtifact BuildPolicyStoredArtifact(
        DesignConversionThinClientArtifactRef artifact,
        RoutePolicyTenantBinding routeTenant,
        string defaultJobId,
        string defaultArtifactRefId,
        DateTimeOffset nowUtc)
    {
        var metadata = artifact.Metadata ?? new Dictionary<string, string>();
        var artifactStoreRef = IsArtifactStoreRef(artifact.Uri);
        var ownerClaim = ReadStoredOwnerTenantClaim(metadata);
        var hasAcceptedExplicitOwnerTenant = ownerClaim.IsAcceptedBy(routeTenant);
        var tenantId = hasAcceptedExplicitOwnerTenant
            ? ownerClaim.EffectiveTenantId
            : artifactStoreRef
                ? MissingArtifactOwnerTenantId
                : routeTenant.DefaultTenantId;

        var jobId = TryReadMetadata(metadata, ArtifactAccessPolicyJobIdMetadataKey, out var rawJobId)
            ? ToPolicyToken("status-artifact-stored-job", rawJobId)
            : defaultJobId;
        var artifactRefId = TryReadMetadata(metadata, ArtifactAccessPolicyArtifactRefIdMetadataKey, out var rawRefId)
            ? ToPolicyToken("status-artifact-stored-ref", rawRefId)
            : defaultArtifactRefId;
        var purpose = ReadPolicyLabel(
            metadata,
            ArtifactAccessPolicyPurposeMetadataKey,
            ArtifactAccessPolicy.ResultSafePurpose);
        var classification = ReadPolicyLabel(
            metadata,
            ArtifactAccessPolicyClassificationMetadataKey,
            ArtifactAccessPolicy.ResultSafeClassification);
        var status = ReadPolicyLabel(
            metadata,
            ArtifactAccessPolicyStatusMetadataKey,
            ArtifactAccessPolicy.ActiveStatus);
        var retentionExpiresAtUtc = ReadPolicyRetentionExpiresAtUtc(metadata, nowUtc);
        var storageRef = artifactStoreRef &&
                         hasAcceptedExplicitOwnerTenant
            ? artifact.Uri.Trim()
            : string.Empty;

        return new ArtifactAccessPolicyStoredArtifact(
            TenantId: tenantId,
            JobId: jobId,
            ArtifactRefId: artifactRefId,
            Purpose: purpose,
            Classification: classification,
            Status: status,
            RetentionExpiresAtUtc: retentionExpiresAtUtc,
            StorageRef: storageRef,
            ContentSha256: string.IsNullOrWhiteSpace(artifact.Sha256)
                ? string.Empty
                : artifact.Sha256.Trim().ToLowerInvariant());
    }

    private static IReadOnlyDictionary<string, string> BuildArtifactAccessResponseMetadata(
        IReadOnlyDictionary<string, string>? sourceMetadata,
        ArtifactAccessPolicyReadGrant? grant,
        ArtifactAccessPolicyAuditEvent audit)
    {
        var metadata = new Dictionary<string, string>(
            FilterArtifactMetadata(sourceMetadata),
            StringComparer.Ordinal)
        {
            ["artifact_access_policy_decision"] = audit.Decision,
            ["artifact_access_policy_reason"] = audit.Reason,
            ["artifact_access_policy_expires_at_utc"] = audit.ExpiresAtUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            ["artifact_access_policy_purpose"] = grant?.Purpose ?? string.Empty,
            ["artifact_access_policy_classification"] = grant?.Classification ?? string.Empty,
            ["artifact_access_policy_scope_ref"] = grant is null
                ? string.Empty
                : BuildHashReference("artifact-access-policy-scope", grant.Scope),
            ["artifact_access_policy_redacted_storage_ref_hash"] = audit.RedactedStorageRefHash,
            ["artifact_access_policy_raw_storage_ref_exposed"] = bool.FalseString,
            ["artifact_access_policy_raw_uri_exposed"] = bool.FalseString
        };

        return metadata;
    }

    private static StoredOwnerTenantClaim ReadStoredOwnerTenantClaim(
        IReadOnlyDictionary<string, string> metadata)
    {
        var hasRawTenantId = metadata.TryGetValue(
            ArtifactAccessPolicyTenantIdMetadataKey,
            out var rawTenantId);
        var hasTenantIdRef = metadata.TryGetValue(
            ArtifactAccessPolicyTenantIdRefMetadataKey,
            out var rawTenantIdRef);
        var malformed =
            (hasRawTenantId && string.IsNullOrWhiteSpace(rawTenantId)) ||
            (hasTenantIdRef && string.IsNullOrWhiteSpace(rawTenantIdRef));

        return new StoredOwnerTenantClaim(
            HasRawTenantId: hasRawTenantId,
            RawTenantId: hasRawTenantId && !string.IsNullOrWhiteSpace(rawTenantId)
                ? ToPolicyToken("status-route-tenant", rawTenantId)
                : null,
            HasTenantIdRef: hasTenantIdRef,
            TenantIdRef: hasTenantIdRef && !string.IsNullOrWhiteSpace(rawTenantIdRef)
                ? ToPolicyToken("status-route-tenant-ref", rawTenantIdRef)
                : null,
            Malformed: malformed);
    }

    private static RoutePolicyTenantBinding ResolveRoutePolicyTenant(
        DesignConversionModelServerRouteAdmissionDecision admission)
    {
        if (admission.AdmissionContext.TenantBound)
        {
            var rawTenantId = string.IsNullOrWhiteSpace(admission.AdmissionContext.TenantId)
                ? null
                : ToPolicyToken("status-route-tenant", admission.AdmissionContext.TenantId);
            var tenantIdRef = string.IsNullOrWhiteSpace(admission.AdmissionContext.TenantIdRef)
                ? null
                : ToPolicyToken("status-route-tenant-ref", admission.AdmissionContext.TenantIdRef);
            var defaultTenantId = rawTenantId ?? tenantIdRef ?? UnboundStatusTenantId;
            return new RoutePolicyTenantBinding(
                defaultTenantId,
                rawTenantId,
                tenantIdRef,
                rawTenantId is not null || tenantIdRef is not null);
        }

        return new RoutePolicyTenantBinding(
            UnboundStatusTenantId,
            RawTenantId: null,
            TenantIdRef: null,
            TenantBound: false);
    }

    private sealed record RoutePolicyTenantBinding(
        string DefaultTenantId,
        string? RawTenantId,
        string? TenantIdRef,
        bool TenantBound)
    {
        public string ResolveRequestTenantId(string storedOwnerTenantId)
            => AcceptsOwnerTenantId(storedOwnerTenantId)
                ? storedOwnerTenantId
                : DefaultTenantId;

        public bool AcceptsOwnerTenantId(string storedOwnerTenantId)
            => TenantBound &&
               (string.Equals(storedOwnerTenantId, RawTenantId, StringComparison.Ordinal) ||
                 string.Equals(storedOwnerTenantId, TenantIdRef, StringComparison.Ordinal));
    }

    private sealed record StoredOwnerTenantClaim(
        bool HasRawTenantId,
        string? RawTenantId,
        bool HasTenantIdRef,
        string? TenantIdRef,
        bool Malformed)
    {
        public string EffectiveTenantId => RawTenantId ?? TenantIdRef ?? MissingArtifactOwnerTenantId;

        public bool IsAcceptedBy(RoutePolicyTenantBinding routeTenant)
        {
            if (Malformed ||
                (!HasRawTenantId && !HasTenantIdRef) ||
                !routeTenant.TenantBound)
            {
                return false;
            }

            if (HasRawTenantId &&
                !string.Equals(RawTenantId, routeTenant.RawTenantId, StringComparison.Ordinal))
            {
                return false;
            }

            if (HasTenantIdRef &&
                !string.Equals(TenantIdRef, routeTenant.TenantIdRef, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }

    private static string BuildDeniedArtifactAccessRef(ArtifactAccessPolicyAuditEvent audit)
        => DeniedArtifactAccessRefPrefix + BuildHashReference(
            "artifact-access-denied",
            $"{audit.Reason}:{audit.RedactedStorageRefHash}");

    private static DateTimeOffset ReadPolicyRetentionExpiresAtUtc(
        IReadOnlyDictionary<string, string> metadata,
        DateTimeOffset nowUtc)
    {
        var boundedDefault = nowUtc.Add(StatusReadbackArtifactRetention);
        if (!TryReadMetadata(metadata, ArtifactAccessPolicyRetentionExpiresAtUtcMetadataKey, out var rawRetention) ||
            !DateTimeOffset.TryParse(
                rawRetention,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return boundedDefault;
        }

        return parsed <= boundedDefault ? parsed : boundedDefault;
    }

    private static string ReadPolicyLabel(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string fallback)
        => TryReadMetadata(metadata, key, out var value)
            ? value.Trim().ToLowerInvariant()
            : fallback;

    private static bool TryReadMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        out string value)
    {
        if (metadata.TryGetValue(key, out var rawValue) &&
            !string.IsNullOrWhiteSpace(rawValue))
        {
            value = rawValue;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool IsArtifactStoreRef(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Trim().StartsWith(ArtifactStoreRefPrefix, StringComparison.Ordinal);

    private static string ToPolicyToken(string scope, string? value)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        if (IsCanonicalPolicyToken(trimmed))
            return trimmed;

        return "ref-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{scope}:{value ?? string.Empty}")))
            .ToLowerInvariant();
    }

    private static bool IsCanonicalPolicyToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 96 ||
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

    private static DesignConversionThinClientStatusQualityMetrics ToResponseQualityMetrics(
        DesignConversionThinClientStatusQualityMetrics? qualityMetrics,
        int defectCount)
    {
        if (qualityMetrics is null)
        {
            return DesignConversionThinClientStatusQualityMetrics.Unavailable(
                source: "status_quality_metrics_missing",
                unavailableReason: "quality_metrics_not_available_for_status") with
            {
                DefectCount = defectCount
            };
        }

        if (!qualityMetrics.Available)
        {
            return DesignConversionThinClientStatusQualityMetrics.Unavailable(
                source: NormalizeQualityMetricsSource(qualityMetrics.Source),
                unavailableReason: NormalizeQualityMetricsUnavailableReason(qualityMetrics.UnavailableReason)) with
            {
                DefectCount = defectCount
            };
        }

        if (qualityMetrics.Score is not { } score ||
            qualityMetrics.RequiredThreshold is not { } threshold ||
            qualityMetrics.Passed is null ||
            qualityMetrics.ThresholdMet is null ||
            !IsFiniteUnitInterval(score) ||
            !IsFiniteUnitInterval(threshold))
        {
            return DesignConversionThinClientStatusQualityMetrics.Unavailable(
                source: "final_visual_verdict",
                unavailableReason: "invalid_final_visual_quality_metrics") with
            {
                DefectCount = defectCount
            };
        }

        return qualityMetrics with
        {
            DefectCount = defectCount,
            Source = NormalizeQualityMetricsSource(qualityMetrics.Source),
            ThresholdSource = "VisualQaBoundaryTrace.VisualDecision.RequiredThreshold",
            UnavailableReason = null
        };
    }

    private static DesignConversionThinClientStatusDefect ToResponseDefect(
        DesignConversionThinClientStatusDefect defect,
        int index)
    {
        var category = NormalizeDefectCategory(defect.Category);
        var severity = NormalizeDefectSeverity(defect.Severity);
        var defectRef = SanitizeHashReference(defect.DefectRef, $"status-defect-{index + 1}");
        var evidenceRefs = (defect.EvidenceRefs ?? [])
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Select(reference => SanitizeHashReference(reference, $"status-defect-evidence-{index + 1}"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (evidenceRefs.Length == 0)
            evidenceRefs = [defectRef];

        return new DesignConversionThinClientStatusDefect(
            DefectRef: defectRef,
            Category: category,
            Severity: severity,
            Message: $"{ToDisplayLabel(severity)} {category.Replace('_', ' ')} issue reported by visual QA.",
            Ordinal: index + 1,
            EvidenceRefs: evidenceRefs);
    }

    private static bool IsFiniteUnitInterval(double value)
        => double.IsFinite(value) && value >= 0.0 && value <= 1.0;

    private static string NormalizeQualityMetricsSource(string? source)
    {
        var normalized = string.IsNullOrWhiteSpace(source) ? string.Empty : source.Trim();
        return normalized is
            "final_visual_verdict" or
            "not_persisted_after_restart" or
            "status_cancelled" or
            "status_completed" or
            "status_failed" or
            "status_not_completed" or
            "status_running" or
            "status_submitted" or
            "status_quality_metrics_missing"
                ? normalized
                : "status_quality_metrics_missing";
    }

    private static string NormalizeQualityMetricsUnavailableReason(string? reason)
        => reason is
            "invalid_final_visual_quality_metrics" or
            "quality_metrics_not_available_for_status" or
            "quality_metrics_not_persisted_after_restart"
                ? reason
                : "quality_metrics_not_available_for_status";

    private static string NormalizeDefectCategory(string? value)
    {
        var normalized = NormalizeDefectLabel(value);
        return normalized is
            "alignment" or
            "artifact" or
            "color" or
            "content" or
            "layout" or
            "quality_threshold_not_met" or
            "responsive" or
            "size" or
            "spacing" or
            "typography"
                ? normalized
                : "other";
    }

    private static string NormalizeDefectSeverity(string? value)
    {
        var normalized = NormalizeDefectLabel(value);
        return normalized is "blocker" or "critical" or "major" or "minor" or "info"
            ? normalized
            : "other";
    }

    private static string NormalizeDefectLabel(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? "other"
            : value.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');

    private static string ToDisplayLabel(string value)
        => string.IsNullOrEmpty(value)
            ? "Other"
            : char.ToUpperInvariant(value[0]) + value[1..];

    private static string SanitizeStatusReference(string? value, string scope)
    {
        if (string.IsNullOrWhiteSpace(value))
            return BuildHashReference(scope, string.Empty);

        var trimmed = value.Trim();
        return IsSafeStatusMetadataValue(trimmed) && !LooksLikeLocalPath(trimmed)
            ? trimmed
            : BuildHashReference(scope, trimmed);
    }

    private static string SanitizeHashReference(string? value, string scope)
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            IsCanonicalSha256Reference(value) &&
            IsSafeStatusMetadataValue(value))
        {
            return value.ToLowerInvariant();
        }

        return BuildHashReference(scope, value ?? string.Empty);
    }

    private static string BuildHashReference(string scope, string value)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{scope}:{value}")))
            .ToLowerInvariant();
        return $"sha256:{hash}";
    }

    private static bool IsCanonicalSha256Reference(string value)
        => value.Length == "sha256:".Length + 64 &&
           value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) &&
           value["sha256:".Length..].All(Uri.IsHexDigit);

    private static bool LooksLikeLocalPath(string value)
    {
        // Статусный публичный readback не должен зависеть от ОС testhost: WSL не считает `C:\...` native absolute path.
        var trimmed = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        return trimmed.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
               LooksLikeUncPath(trimmed) ||
               LooksLikeWindowsDrivePath(trimmed) ||
               Path.IsPathFullyQualified(trimmed);
    }

    private static bool LooksLikeWindowsDrivePath(string value)
    {
        // Drive-qualified форма чувствительна и в Windows, и в Linux/WSL: `C:\`, `C:/` и дочерние paths хешируются.
        if (value.Length < 3 ||
            value[1] != ':' ||
            value[0] is not ((>= 'A' and <= 'Z') or (>= 'a' and <= 'z')))
        {
            return false;
        }

        return value[2] is '\\' or '/';
    }

    private static bool LooksLikeUncPath(string value)
    {
        // UNC-адреса с любым separator выглядят как локальная инфраструктура и не должны попадать в metadata сырыми.
        return value.Length >= 2 &&
               ((value[0] == '\\' && value[1] == '\\') ||
                (value[0] == '/' && value[1] == '/'));
    }

    private static IReadOnlyDictionary<string, string> FilterArtifactMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        var filtered = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (IsArtifactAccessPolicyInputMetadataKey(pair.Key) ||
                !IsSafeStatusMetadataKey(pair.Key))
            {
                continue;
            }

            var value = pair.Value ?? string.Empty;
            var rawRefLikeValue = LooksLikeRawArtifactMetadataValue(value);
            if (!rawRefLikeValue && !IsSafeStatusMetadataValue(pair.Key, pair.Value))
                continue;

            filtered[pair.Key] = rawRefLikeValue
                ? BuildHashReference($"status-artifact-metadata:{pair.Key}", value)
                : value;
        }

        return filtered;
    }

    private static bool LooksLikeRawArtifactMetadataValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return IsArtifactStoreRef(trimmed) ||
               LooksLikeHttpUrl(trimmed) ||
               LooksLikeLocalPath(trimmed) ||
               LooksLikeProviderPrivateOrRawStorageLabel(trimmed);
    }

    private static bool LooksLikeHttpUrl(string value)
        => value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeProviderPrivateOrRawStorageLabel(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Replace('_', '-').Replace(' ', '-');
        return normalized.Contains("provider-private", StringComparison.Ordinal) ||
               normalized.Contains("raw-storage", StringComparison.Ordinal) ||
               normalized.Contains("storage-ref", StringComparison.Ordinal) ||
               normalized.Contains("provider-storage", StringComparison.Ordinal);
    }

    private static bool IsArtifactAccessPolicyInputMetadataKey(string key)
        => string.Equals(key, ArtifactAccessPolicyTenantIdMetadataKey, StringComparison.Ordinal) ||
           string.Equals(key, ArtifactAccessPolicyTenantIdRefMetadataKey, StringComparison.Ordinal) ||
           string.Equals(key, ArtifactAccessPolicyJobIdMetadataKey, StringComparison.Ordinal) ||
           string.Equals(key, ArtifactAccessPolicyArtifactRefIdMetadataKey, StringComparison.Ordinal) ||
           string.Equals(key, ArtifactAccessPolicyPurposeMetadataKey, StringComparison.Ordinal) ||
           string.Equals(key, ArtifactAccessPolicyClassificationMetadataKey, StringComparison.Ordinal) ||
           string.Equals(key, ArtifactAccessPolicyStatusMetadataKey, StringComparison.Ordinal) ||
           string.Equals(key, ArtifactAccessPolicyRetentionExpiresAtUtcMetadataKey, StringComparison.Ordinal);

    private static DesignConversionModelServerHttpResponse Error(
        string method,
        string path,
        int statusCode,
        string errorCode,
        string errorMessage,
        IReadOnlyDictionary<string, string>? boundaryMetadata = null)
    {
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: EndpointTemplatePath,
            Method: string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            BoundaryMetadata: BuildBoundaryMetadata(
                method,
                path,
                statusCode,
                apiMetadata: boundaryMetadata,
                evidenceRefs: [],
                artifactCount: 0));

        return Json(statusCode, method, path, body);
    }

    private static DesignConversionModelServerHttpResponse Json<T>(
        int statusCode,
        string method,
        string path,
        T body)
    {
        var headers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["x-design-conversion-endpoint"] = EndpointTemplatePath,
            ["x-design-conversion-http-seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["x-design-conversion-status-readback-boundary"] = LocalStatusReadbackBoundary,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-cross-repo-caller-verified"] = bool.FalseString,
            ["x-design-conversion-runtime-surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["x-design-conversion-method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["x-design-conversion-path"] = NormalizePath(path),
            ["x-design-conversion-status-code"] = statusCode.ToString(CultureInfo.InvariantCulture)
        };

        return new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: "application/json; charset=utf-8",
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Headers: headers);
    }

    private static bool CanUseLocalArtifactDbRestoredStatusForAdmission(
        OperationResult<DesignConversionModelServerStatusResponse> result,
        DesignConversionModelServerRouteAdmissionDecision admission)
    {
        if (!result.IsSuccess ||
            result.Value is null ||
            !admission.AdmissionContext.TenantBound)
        {
            return false;
        }

        var provenance = result.Value.RuntimeStatus.Provenance;
        if (!IsExactTrueMetadata(provenance, "local_artifact_db_status_restored") ||
            !IsExactTrueMetadata(
                provenance,
                DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey))
        {
            return false;
        }

        if (!provenance.TryGetValue(
                DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey,
                out var persistedTenantIdRef) ||
            !provenance.TryGetValue(
                DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey,
                out var persistedScopeRef))
        {
            return false;
        }

        return DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionTenantIdRef(persistedTenantIdRef) &&
               DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionScopeRef(persistedScopeRef) &&
               DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionTenantIdRef(admission.AdmissionContext.TenantIdRef) &&
               DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionScopeRef(admission.AdmissionContext.ScopeRef) &&
               string.Equals(persistedTenantIdRef, admission.AdmissionContext.TenantIdRef, StringComparison.Ordinal) &&
               string.Equals(persistedScopeRef, admission.AdmissionContext.ScopeRef, StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(
        string method,
        string path,
        int statusCode,
        IReadOnlyDictionary<string, string>? apiMetadata,
        IReadOnlyList<string> evidenceRefs,
        int artifactCount)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = "GET /api/design-conversion/status/{jobId}",
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = NormalizePath(path),
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["local_http_host_seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["local_status_readback_boundary"] = LocalStatusReadbackBoundary,
            ["local_http_host_seam_verified"] = bool.TrueString,
            ["status_readback_route_local_only"] = bool.TrueString,
            ["runtime_status_readback_invoked"] = (statusCode == 200).ToString(),
            ["job_rerun_invoked"] = bool.FalseString,
            ["runtime_invoked"] = bool.FalseString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
            ["runtime_surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["api_shape"] = DesignConversionModelServerApi.ApiShape,
            ["status_readback_response_refs_only"] = bool.TrueString,
            ["status_readback_artifact_ref_count"] = artifactCount.ToString(CultureInfo.InvariantCulture),
            ["status_readback_evidence_ref_count"] = evidenceRefs.Count.ToString(CultureInfo.InvariantCulture),
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.BoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        foreach (var pair in DesignConversionModelServerCallerContract.StatusReadbackBoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        foreach (var pair in apiMetadata ?? new Dictionary<string, string>())
        {
            if (!IsSafeStatusMetadataKey(pair.Key) || !IsSafeStatusMetadataValue(pair.Key, pair.Value))
                continue;

            var value = pair.Value ?? string.Empty;
            metadata[pair.Key] = LooksLikeLocalPath(value)
                ? BuildHashReference($"status-boundary-metadata:{pair.Key}", value)
                : value;
        }

        var modelAuthority = DesignConversionModelAuthorityBoundary.FromTrustedBoundaryMetadata(metadata);
        metadata["runtime_status_readback_invoked"] = (statusCode == 200).ToString();
        metadata["job_rerun_invoked"] = bool.FalseString;
        metadata["runtime_invoked"] = bool.FalseString;
        metadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
            DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue;
        metadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
            DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue;
        metadata["refs_or_hashes_only"] = bool.TrueString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["raw_user_private_data_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["external_provider_called"] =
            IsTruthyBoundary(metadata, DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey)
                .ToString();
        metadata["asp_net_host_verified"] = bool.FalseString;
        metadata["socket_opened"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["platform_integration_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        PreserveTrustedNetworkBoundaryMetadata(metadata);
        metadata["product_ready"] = bool.FalseString;
        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(metadata, modelAuthority);
        return metadata;
    }

    private static IReadOnlyDictionary<string, string> MergeBoundaryMetadata(
        IReadOnlyDictionary<string, string>? left,
        IReadOnlyDictionary<string, string>? right)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in left ?? new Dictionary<string, string>())
            metadata[pair.Key] = pair.Value;
        foreach (var pair in right ?? new Dictionary<string, string>())
            metadata[pair.Key] = pair.Value;

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> SanitizeStatusDenialAdmissionMetadata(
        IReadOnlyDictionary<string, string> metadata)
    {
        var sanitized = new Dictionary<string, string>(metadata, StringComparer.Ordinal)
        {
            ["route_admission_path"] = EndpointTemplatePath
        };
        sanitized.Remove("route_admission_scope");

        return sanitized;
    }

    private static void PreserveTrustedNetworkBoundaryMetadata(IDictionary<string, string> metadata)
    {
        var jsonMetadata = new JsonObject();
        foreach (var pair in metadata)
            jsonMetadata[pair.Key] = pair.Value;

        DesignConversionModelServerNetworkBoundary.PreserveTrustedNetworkBoundaryMetadata(jsonMetadata);
        CopyJsonMetadataValue(jsonMetadata, metadata, DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey);
        CopyJsonMetadataValue(jsonMetadata, metadata, DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerificationKey);
        CopyJsonMetadataValue(jsonMetadata, metadata, "network_call_executed");
        CopyJsonMetadataValue(jsonMetadata, metadata, "external_network_call_executed");
        CopyJsonMetadataValue(jsonMetadata, metadata, "external_provider_called");
        CopyJsonMetadataValue(jsonMetadata, metadata, "real_external_provider_executed");
    }

    private static void CopyJsonMetadataValue(
        JsonObject source,
        IDictionary<string, string> target,
        string key)
    {
        if (source.TryGetPropertyValue(key, out var value))
            target[key] = value?.GetValue<string>() ?? string.Empty;
    }

    private static int StatusCodeForApiError(string? errorCode)
        => errorCode switch
        {
            DesignConversionModelServerApi.UnexpectedStatusFailureErrorCode => 500,
            "JOB_ID_REQUIRED" => 400,
            "JOB_NOT_FOUND" => 404,
            _ => 400
        };

    private static bool IsUnexpectedStatusFailureCode(string? errorCode)
        => string.Equals(
            errorCode,
            DesignConversionModelServerApi.UnexpectedStatusFailureErrorCode,
            StringComparison.Ordinal);

    private static string ExtractJobId(string path)
    {
        var normalized = NormalizePath(path);
        if (string.Equals(normalized, EndpointBasePath, StringComparison.Ordinal))
            return string.Empty;
        if (!normalized.StartsWith(EndpointPathPrefix, StringComparison.Ordinal))
            return string.Empty;

        var segment = normalized[EndpointPathPrefix.Length..];
        return Uri.UnescapeDataString(segment);
    }

    private static bool IsStatusReadbackPath(string path)
        => string.Equals(path, EndpointBasePath, StringComparison.Ordinal) ||
           path.StartsWith(EndpointPathPrefix, StringComparison.Ordinal);

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "/" : path.Trim();

    private static bool IsSafeStatusMetadataKey(string key)
        => !string.IsNullOrWhiteSpace(key) &&
           (string.Equals(key, "idempotency_payload_digest_exposed", StringComparison.Ordinal) ||
            (!key.Contains("html", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("css", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("tree", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("payload", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("secret", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("private", StringComparison.OrdinalIgnoreCase)));

    private static bool IsSafeStatusMetadataValue(string key, string? value)
        => IsServerOwnedVisualProviderReceiptMetadataKey(key)
            ? IsSafeServerOwnedVisualProviderReceiptValue(value)
            : IsSafeStatusMetadataValue(value);

    private static bool IsSafeStatusMetadataValue(string? value)
        => string.IsNullOrWhiteSpace(value) ||
           (value.Length <= 160 &&
            !value.Contains("<html", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<main", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<div", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("{\"", StringComparison.Ordinal) &&
            !value.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("SECRET", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("data-secret", StringComparison.OrdinalIgnoreCase));

    private static bool IsSafeServerOwnedVisualProviderReceiptValue(string? value)
        => string.IsNullOrWhiteSpace(value) ||
           (value.Length <= 512 &&
            !value.Contains("<html", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<main", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<div", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("{\"", StringComparison.Ordinal) &&
            !value.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("SECRET", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("data-secret", StringComparison.OrdinalIgnoreCase));

    private static bool IsServerOwnedVisualProviderReceiptMetadataKey(string key)
        => string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey, StringComparison.Ordinal);

    private static bool IsTruthyBoundary(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static bool IsExactTrueMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           string.Equals(value, bool.TrueString, StringComparison.Ordinal);
}
