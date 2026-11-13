// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerStatusReadbackTests : IDisposable
{
    private const string TestPostEvaluationStateRootPrefix =
        "design-conversion-status-readback-post-evaluation-";

    private readonly string _testPostEvaluationStateRoot = Path.Combine(
        Path.GetTempPath(),
        TestPostEvaluationStateRootPrefix + Guid.NewGuid().ToString("N"));

    private IDesignConversionPostEvaluationStateStore TestPostEvaluationStateStore =>
        new JsonDesignConversionPostEvaluationStateStore(_testPostEvaluationStateRoot);

    public void Dispose()
    {
        var root = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(_testPostEvaluationStateRoot));
        var tempRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.GetTempPath()));
        if (string.Equals(
                Path.GetDirectoryName(root),
                tempRoot,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal) &&
            Path.GetFileName(root).StartsWith(
                TestPostEvaluationStateRootPrefix,
                StringComparison.Ordinal) &&
            Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TryGetJobStatusAsync_SubmittedJob_ReturnsRefsOnlyStatusEnvelopeWithoutRerun()
    {
        const string privateMarker = "PRIVATE_PASS131_STATUS_SECRET";
        var verdict = CreateVerdict(
            "pass131-status-verdict",
            score: 0.91,
            passed: true,
            privateMarker: privateMarker,
            includeDefect: true);
        var (adapter, renderProvider, comparisonProvider) = CreateRegisteredAdapter(
            verdict,
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var package = CreatePackage("pass131-status-readback-request", privateMarker);
        var submitEnvelope = await adapter.TrySubmitAsync(package);
        Assert.Equal("success", submitEnvelope.EnvelopeKind);
        Assert.Equal(200, submitEnvelope.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(submitEnvelope.ParsedJobId));
        Assert.Null(submitEnvelope.ParsedErrorCode);

        var renderCountAfterSubmit = renderProvider.Requests.Count;
        var comparisonCountAfterSubmit = comparisonProvider.Requests.Count;
        Assert.Equal(0.91, verdict.Score, precision: 6);
        Assert.True(verdict.Passed);
        var injectedDefect = Assert.Single(verdict.Defects);
        Assert.Equal("spacing", injectedDefect.Category);
        Assert.NotEmpty(comparisonProvider.Requests);

        var statusEnvelope = await adapter.TryGetJobStatusAsync(
            submitEnvelope.ParsedJobId,
            package.SourceKind);

        AssertStableStatusSuccessEnvelope(
            statusEnvelope,
            submitEnvelope,
            package,
            privateMarker,
            expectedContractStatus: DesignConversionThinClientStatus.NotReady);
        Assert.Equal(renderCountAfterSubmit, renderProvider.Requests.Count);
        Assert.Equal(comparisonCountAfterSubmit, comparisonProvider.Requests.Count);
        Assert.False(statusEnvelope.RuntimeInvoked);
        Assert.Equal(bool.TrueString, statusEnvelope.BoundaryMetadata["runtime_status_readback_invoked"]);
        Assert.Equal(bool.FalseString, statusEnvelope.BoundaryMetadata["job_rerun_invoked"]);

        using var document = JsonDocument.Parse(statusEnvelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, root.GetProperty("endpoint").GetString());
        Assert.Equal("GET", root.GetProperty("method").GetString());
        Assert.Equal(submitEnvelope.ParsedJobId, root.GetProperty("jobId").GetString());
        Assert.Equal(package.RequestId, root.GetProperty("requestId").GetString());
        Assert.Equal(DesignConversionThinClientStatus.NotReady, root.GetProperty("contractStatus").GetString());
        Assert.False(root.GetProperty("learnedInferenceApplied").GetBoolean());
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.True(root.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("artifacts").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("evidenceRefs").ValueKind);
        Assert.False(root.TryGetProperty("runtimeStatus", out _));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("trained_model_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("not_trained_model_authority").GetString());
        AssertStatusQualityMetricsAvailable(
            root,
            expectedScore: 0.0,
            expectedRequiredThreshold: 0.85,
            expectedDefectCount: 2,
            expectedPassed: false,
            expectedThresholdMet: false);
        AssertStatusDefectsSafe(root, privateMarker);
        Assert.Equal(bool.TrueString, metadata.GetProperty("quality_metrics_available").GetString());
        Assert.Equal("final_visual_verdict", metadata.GetProperty("quality_metrics_source").GetString());
        Assert.Equal("2", metadata.GetProperty("status_defect_count").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("status_defects_refs_only").GetString());
    }

    [Fact]
    public async Task StatusReadback_ServerOwnedDirectDpoAuthorityAgreesWithEvaluateSurface()
    {
        var api = new DesignConversionModelServerApi(
            new FixedThinClientJobEvaluator(CreateServerOwnedDirectDpoAuthorityEvaluation),
            () => DateTimeOffset.Parse("2026-07-03T14:02:00Z"),
            postEvaluationStateStore: TestPostEvaluationStateStore);
        var evaluateHandler = new DesignConversionModelServerHttpHandler(api);
        var statusHandler = new DesignConversionModelServerStatusHandler(api);
        var package = CreatePackage("status-direct-dpo-authority-request", "STATUS_DIRECT_DPO_AUTHORITY");
        var apiRequest = new DesignConversionModelServerApiRequest(
            RequestId: package.RequestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: package.DesignTree!,
            OriginalTree: package.OriginalTree!,
            Html: package.Html!,
            Css: package.Css!,
            Screenshots: package.Screenshots,
            DesignRenders: package.DesignRenders,
            Budget: package.Budget!,
            Metadata: new Dictionary<string, string>
            {
                [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = bool.FalseString,
                [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] = bool.TrueString,
                [DesignConversionModelAuthorityBoundary.BoundarySourceKey] =
                    DesignConversionModelAuthorityBoundary.NoTrustedDirectDpoAuthoritySource
            });

        var submitResponse = await evaluateHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json"));

        Assert.Equal(200, submitResponse.StatusCode);
        using var submitDocument = JsonDocument.Parse(submitResponse.BodyJson);
        var submitRoot = submitDocument.RootElement;
        var submitMetadata = submitRoot.GetProperty("boundaryMetadata");
        Assert.True(submitRoot.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.False(submitRoot.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.False(submitRoot.GetProperty("modelUploadReady").GetBoolean());
        Assert.False(submitRoot.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(submitRoot.GetProperty("productReady").GetBoolean());
        Assert.Equal(bool.TrueString, submitMetadata.GetProperty(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey).GetString());
        Assert.Equal(bool.FalseString, submitMetadata.GetProperty(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey).GetString());

        var jobId = Assert.IsType<string>(submitRoot.GetProperty("jobId").GetString());
        var statusResponse = await statusHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerStatusHandler.EndpointPathPrefix + Uri.EscapeDataString(jobId),
            BodyJson: null));

        Assert.Equal(200, statusResponse.StatusCode);
        using var statusDocument = JsonDocument.Parse(statusResponse.BodyJson);
        var statusRoot = statusDocument.RootElement;
        var statusMetadata = statusRoot.GetProperty("boundaryMetadata");
        Assert.True(statusRoot.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.False(statusRoot.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.False(statusRoot.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(statusRoot.GetProperty("productReady").GetBoolean());
        Assert.Equal(bool.TrueString, statusMetadata.GetProperty(DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey).GetString());
        Assert.Equal(bool.FalseString, statusMetadata.GetProperty(DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey).GetString());
        Assert.Equal(
            DesignConversionModelAuthorityBoundary.TrustedDirectDpoRuntimeFlowAuthoritySource,
            statusMetadata.GetProperty(DesignConversionModelAuthorityBoundary.BoundarySourceKey).GetString());
    }

    [Fact]
    public async Task StatusRoute_EnabledAdmissionHashesPathScopeWithoutRerun()
    {
        const string admissionToken = "current-route-token-0123456789";
        const string admissionTenantId = "tenant-route-admission-alpha";
        const string trustedTier = "paid";
        const string trustedMonthlyCap = "125.75";
        const string rawRouteScope = @"C:\server\scope";
        var budgetStoreRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-status-admission-" + Guid.NewGuid().ToString("N"));

        try
        {
            var admission = DesignConversionModelServerRouteAdmission.Create(
                new DesignConversionModelServerRouteAdmissionOptions(
                    enabled: true,
                    currentToken: admissionToken,
                    tenantId: admissionTenantId,
                    routeScope: rawRouteScope));
            Assert.True(admission.Enabled);
            Assert.True(admission.Ready);

            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(
                CreateVerdict("pass131-status-admission-verdict", score: 0.91, passed: true));
            var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
            var api = new DesignConversionModelServerApi(
                service,
                () => DateTimeOffset.Parse("2026-07-03T14:05:00Z"),
                visualCostBudgetStore: new JsonlDesignConversionVisualCostBudgetStore(budgetStoreRoot));
            var evaluateHandler = new DesignConversionModelServerHttpHandler(api, admission);
            var statusHandler = new DesignConversionModelServerStatusHandler(api, admission);
            var package = CreatePackage("pass131-status-admission-request", "PASS131_STATUS_ADMISSION_MARKER");
            var requestMetadata = new Dictionary<string, string>(
                package.Metadata ?? new Dictionary<string, string>(),
                StringComparer.Ordinal)
            {
                ["tenant_id"] = admissionTenantId,
                [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = trustedTier,
                [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] =
                    trustedMonthlyCap,
                [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                    DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                    DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                    DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
            };
            var apiRequest = new DesignConversionModelServerApiRequest(
                RequestId: package.RequestId,
                SourceKind: DesignConversionSourceKind.Figma,
                DesignTree: package.DesignTree!,
                OriginalTree: package.OriginalTree!,
                Html: package.Html!,
                Css: package.Css!,
                Screenshots: package.Screenshots,
                DesignRenders: package.DesignRenders,
                Budget: package.Budget! with
                {
                    UserId = "status-admission-platform-user",
                    Tier = trustedTier,
                    BudgetUsd = 0.25m,
                    Metadata = new Dictionary<string, string>()
                },
                Metadata: requestMetadata);
            var authorizedHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = $"Bearer {admissionToken}",
                [DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = "pass131-status-admission-idempotency"
            };
            var submitResponse = await evaluateHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
                Method: "POST",
                Path: DesignConversionModelServerHttpHandler.EndpointPath,
                BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
                ContentType: "application/json",
                Headers: authorizedHeaders));
            Assert.Equal(200, submitResponse.StatusCode);

            using var submitDocument = JsonDocument.Parse(submitResponse.BodyJson);
            var jobId = Assert.IsType<string>(submitDocument.RootElement.GetProperty("jobId").GetString());
            var statusRequest = new DesignConversionModelServerHttpRequest(
                Method: "GET",
                Path: DesignConversionModelServerStatusHandler.EndpointPathPrefix + Uri.EscapeDataString(jobId),
                BodyJson: null,
                ContentType: null,
                Headers: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = $"Bearer {admissionToken}"
                });

            var firstStatusResponse = await statusHandler.HandleAsync(statusRequest);
            var secondStatusResponse = await statusHandler.HandleAsync(statusRequest);
            Assert.Equal(200, firstStatusResponse.StatusCode);
            Assert.Equal(200, secondStatusResponse.StatusCode);

            var expectedScopeRef = "sha256:" +
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                    $"status-boundary-metadata:route_admission_scope:{rawRouteScope}")))
                    .ToLowerInvariant();
            using var firstStatusDocument = JsonDocument.Parse(firstStatusResponse.BodyJson);
            using var secondStatusDocument = JsonDocument.Parse(secondStatusResponse.BodyJson);
            var firstRoot = firstStatusDocument.RootElement;
            var secondRoot = secondStatusDocument.RootElement;
            Assert.Equal(jobId, firstRoot.GetProperty("jobId").GetString());
            Assert.Equal(jobId, secondRoot.GetProperty("jobId").GetString());

            var firstMetadata = firstRoot.GetProperty("boundaryMetadata");
            var secondMetadata = secondRoot.GetProperty("boundaryMetadata");
            var firstScopeRef = Assert.IsType<string>(firstMetadata.GetProperty("route_admission_scope").GetString());
            var secondScopeRef = Assert.IsType<string>(secondMetadata.GetProperty("route_admission_scope").GetString());
            Assert.Equal(expectedScopeRef, firstScopeRef);
            Assert.Equal(expectedScopeRef, secondScopeRef);
            Assert.Matches("^sha256:[0-9a-f]{64}$", firstScopeRef);
            Assert.Equal(bool.TrueString, firstMetadata.GetProperty("route_admission_enabled").GetString());
            Assert.Equal(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                firstMetadata.GetProperty("route_admission_header_name").GetString());
            Assert.Equal(bool.FalseString, firstMetadata.GetProperty("job_rerun_invoked").GetString());
            Assert.Equal(bool.FalseString, secondMetadata.GetProperty("job_rerun_invoked").GetString());
            Assert.DoesNotContain(rawRouteScope, firstStatusResponse.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(
                JsonSerializer.Serialize(rawRouteScope),
                firstStatusResponse.BodyJson,
                StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(budgetStoreRoot))
                Directory.Delete(budgetStoreRoot, recursive: true);
        }
    }

    [Fact]
    public async Task StatusRoute_EnabledAdmissionRestoresSameTenantFromArtifactDbAfterRestartWithoutRerun()
    {
        const string admissionToken = "current-route-token-restart-0123456789";
        const string admissionTenantId = "tenant-route-admission-restart-alpha";
        const string rawRouteScope = @"C:\server\scope\restart";
        const string privateMarker = "PASS131_STATUS_RESTART_MARKER";
        var root = CreateRestartTestRoot("same-binding");

        try
        {
            var seed = await SeedAdmissionRestartRecordAsync(
                root,
                "pass131-status-admission-restart",
                admissionToken,
                admissionTenantId,
                rawRouteScope,
                privateMarker);
            var expectedTenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef(admissionTenantId);
            var expectedScopeRef = BuildRouteAdmissionScopeRef(rawRouteScope, expectedTenantIdRef);
            AssertSafePersistedRouteAdmissionBinding(
                seed.Record.Metadata,
                expectedTenantIdRef,
                expectedScopeRef);
            AssertMetadataDoesNotLeak(
                seed.Record.Metadata,
                admissionTenantId,
                rawRouteScope,
                privateMarker);

            var invocation = await InvokeRestartStatusAsync(
                seed.ArtifactDbRoot,
                seed.JobId,
                admissionToken,
                admissionTenantId,
                rawRouteScope);

            Assert.Equal(200, invocation.Response.StatusCode);
            Assert.Empty(invocation.RenderProvider.Requests);
            Assert.Empty(invocation.ComparisonProvider.Requests);
            Assert.DoesNotContain(admissionTenantId, invocation.Response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rawRouteScope, invocation.Response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, invocation.Response.BodyJson, StringComparison.Ordinal);
            Assert.Contains(expectedTenantIdRef, invocation.Response.BodyJson, StringComparison.Ordinal);
            Assert.Contains(expectedScopeRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        }
        finally
        {
            DeleteRestartTestRoot(root);
        }
    }

    [Fact]
    public async Task StatusRoute_EnabledAdmissionRestoreRejectsSameJobFromDifferentTenant()
    {
        var root = CreateRestartTestRoot("foreign-tenant");

        try
        {
            var seed = await SeedAdmissionRestartRecordAsync(
                root,
                "pass131-status-foreign-tenant",
                admissionToken: "current-route-token-foreign-tenant-0123456789",
                admissionTenantId: "tenant-route-admission-restart-owner",
                rawRouteScope: @"C:\server\scope\restart\tenant",
                privateMarker: "PASS131_STATUS_FOREIGN_TENANT_MARKER");
            var invocation = await InvokeRestartStatusAsync(
                seed.ArtifactDbRoot,
                seed.JobId,
                admissionToken: seed.AdmissionToken,
                admissionTenantId: "tenant-route-admission-restart-foreign",
                rawRouteScope: seed.RawRouteScope);

            AssertGenericJobNotFoundWithoutRerunOrLeak(
                invocation,
                seed,
                "tenant-route-admission-restart-foreign",
                seed.RawRouteScope);
        }
        finally
        {
            DeleteRestartTestRoot(root);
        }
    }

    [Fact]
    public async Task StatusRoute_EnabledAdmissionRestoreRejectsSameTenantDifferentRouteScope()
    {
        var root = CreateRestartTestRoot("foreign-scope");

        try
        {
            var seed = await SeedAdmissionRestartRecordAsync(
                root,
                "pass131-status-foreign-scope",
                admissionToken: "current-route-token-foreign-scope-0123456789",
                admissionTenantId: "tenant-route-admission-restart-scope",
                rawRouteScope: "private-customer-scope-owner",
                privateMarker: "PASS131_STATUS_FOREIGN_SCOPE_MARKER");
            const string foreignRawRouteScope = "private-customer-scope-foreign";
            var invocation = await InvokeRestartStatusAsync(
                seed.ArtifactDbRoot,
                seed.JobId,
                admissionToken: seed.AdmissionToken,
                admissionTenantId: seed.AdmissionTenantId,
                rawRouteScope: foreignRawRouteScope);

            AssertGenericJobNotFoundWithoutRerunOrLeak(
                invocation,
                seed,
                seed.AdmissionTenantId,
                foreignRawRouteScope);
        }
        finally
        {
            DeleteRestartTestRoot(root);
        }
    }

    [Theory]
    [InlineData("tenant-route-admission-alpha")]
    [InlineData("sha256:wrong-scope:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("sha256:design-conversion-model-activation-tenant:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("sha256:design-conversion-model-activation-tenant:1234")]
    public async Task GetJobResultAsync_RestoredStatusStripsInvalidExactTenantRefMetadata(
        string invalidTenantIdRef)
    {
        var root = CreateRestartTestRoot("public-invalid-tenant-ref");

        try
        {
            var seed = await SeedAdmissionRestartRecordAsync(
                root,
                "pass131-status-invalid-tenant-ref",
                admissionToken: "current-route-token-invalid-tenant-ref-0123456789",
                admissionTenantId: "tenant-route-admission-alpha",
                rawRouteScope: "private-customer-scope-sanitizer",
                privateMarker: "PASS131_STATUS_INVALID_TENANT_REF_MARKER");
            var tamperedMetadata = new Dictionary<string, string>(seed.Record.Metadata, StringComparer.Ordinal)
            {
                [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                    invalidTenantIdRef
            };
            var tamperedDbRoot = Path.Combine(root, "tampered-db");
            await AppendTamperedRecordAsync(seed.Record, tamperedDbRoot, tamperedMetadata);

            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
                "pass131-status-invalid-tenant-ref-should-not-rerun",
                score: 0.20,
                passed: false,
                privateMarker: "PASS131_STATUS_INVALID_TENANT_REF_SHOULD_NOT_RERUN",
                includeDefect: true));
            var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
            var api = new DesignConversionModelServerApi(
                new DesignConversionInternalServiceThinClientJobEvaluator(service),
                () => DateTimeOffset.Parse("2026-07-03T14:25:00Z"),
                new JsonlDesignConversionLocalArtifactDb(tamperedDbRoot));

            var result = await api.GetJobResultAsync(seed.JobId);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.NotNull(result.Value);
            Assert.Empty(renderProvider.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.False(result.Value!.RuntimeStatus.Provenance.ContainsKey(
                DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey));
            var serialized = JsonSerializer.Serialize(
                result.Value,
                DesignConversionModelServerHttpHandler.JsonOptions);
            Assert.DoesNotContain(invalidTenantIdRef, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(seed.AdmissionTenantId, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(seed.RawRouteScope, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(seed.PrivateMarker, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain("PASS131_STATUS_INVALID_TENANT_REF_SHOULD_NOT_RERUN", serialized, StringComparison.Ordinal);
        }
        finally
        {
            DeleteRestartTestRoot(root);
        }
    }

    [Theory]
    [InlineData("missing_tenant_ref")]
    [InlineData("partial_missing_scope_ref")]
    [InlineData("malformed_tenant_ref")]
    [InlineData("malformed_scope_ref")]
    [InlineData("false_tenant_bound")]
    [InlineData("noncanonical_truthy_tenant_bound")]
    public async Task StatusRoute_EnabledAdmissionRestoreRejectsMissingPartialMalformedOrFalseBinding(
        string caseName)
    {
        var root = CreateRestartTestRoot("tampered-" + caseName);

        try
        {
            var seed = await SeedAdmissionRestartRecordAsync(
                root,
                "pass131-status-tampered-" + caseName,
                admissionToken: "current-route-token-tampered-0123456789",
                admissionTenantId: "tenant-route-admission-restart-tampered",
                rawRouteScope: @"C:\server\scope\restart\tampered",
                privateMarker: "PASS131_STATUS_TAMPERED_BINDING_MARKER");
            var tamperedDbRoot = Path.Combine(root, "tampered-db");
            await AppendTamperedRecordAsync(
                seed.Record,
                tamperedDbRoot,
                TamperRouteAdmissionBinding(seed.Record.Metadata, caseName));

            var invocation = await InvokeRestartStatusAsync(
                tamperedDbRoot,
                seed.JobId,
                admissionToken: seed.AdmissionToken,
                admissionTenantId: seed.AdmissionTenantId,
                rawRouteScope: seed.RawRouteScope);

            AssertGenericJobNotFoundWithoutRerunOrLeak(
                invocation,
                seed,
                seed.AdmissionTenantId,
                seed.RawRouteScope);
        }
        finally
        {
            DeleteRestartTestRoot(root);
        }
    }

    [Theory]
    [InlineData(null, "JOB_ID_REQUIRED", 400)]
    [InlineData("thin-job-pass131-unknown", "JOB_NOT_FOUND", 404)]
    public async Task TryGetJobStatusAsync_MissingOrUnknownJob_ReturnsStableErrorEnvelope(
        string? jobId,
        string expectedErrorCode,
        int expectedStatusCode)
    {
        var (adapter, renderProvider, comparisonProvider) = CreateRegisteredAdapter();

        var envelope = await adapter.TryGetJobStatusAsync(jobId, nameof(DesignConversionSourceKind.Figma));

        AssertStableStatusErrorEnvelope(
            envelope,
            expectedStatusCode,
            registryInvokeSucceeded: true,
            expectedErrorCode);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task TryGetJobStatusAsync_UnregisteredRoute_ReturnsStableRegistryErrorEnvelope()
    {
        var registry = new DesignConversionModelServerEndpointRegistry();
        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);

        var envelope = await adapter.TryGetJobStatusAsync(
            "thin-job-pass131-route-missing",
            nameof(DesignConversionSourceKind.Figma));

        AssertStableStatusErrorEnvelope(
            envelope,
            404,
            registryInvokeSucceeded: false,
            expectedErrorCode: "DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND");
        Assert.Equal("endpoint_registry", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, envelope.EndpointPath);
    }

    [Fact]
    public async Task TryGetJobStatusAsync_RouteDescriptorMirrorsCallerContractDescriptor()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeStatusReadbackRoute();
        var (adapter, renderProvider, comparisonProvider) = CreateRegisteredAdapter();

        var envelope = await adapter.TryGetJobStatusAsync(null, nameof(DesignConversionSourceKind.Figma));

        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(400, envelope.StatusCode);
        Assert.Equal("JOB_ID_REQUIRED", envelope.ParsedErrorCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.False(envelope.RuntimeInvoked);
        Assert.Equal(descriptor.SchemaVersion, envelope.ContractSchemaVersion);
        Assert.Equal(descriptor.EndpointMethod, envelope.EndpointMethod);
        Assert.Equal(descriptor.EndpointPath, envelope.EndpointPath);
        Assert.Equal(
            descriptor.RequestFields
                .Where(field => field.Required)
                .Select(field => field.JsonName)
                .ToArray(),
            envelope.RequiredRequestFields);
        Assert.Equal(descriptor.SchemaVersion, envelope.BoundaryMetadata["caller_contract_schema_version"]);
        Assert.Equal(descriptor.Boundary, envelope.BoundaryMetadata["caller_contract_boundary"]);
        Assert.Equal(descriptor.EndpointMethod, envelope.BoundaryMetadata["local_caller_endpoint_method"]);
        Assert.Equal(descriptor.EndpointPath, envelope.BoundaryMetadata["local_caller_endpoint_path"]);
        Assert.Equal(descriptor.ResponseContentType, envelope.BoundaryMetadata["http_content_type"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task TryGetJobStatusAsync_RestartLocalArtifactDbReadbackPreservesSafeQualitySnapshotWithoutRerun()
    {
        const string privateMarker = "PRIVATE_PASS131_RESTART_STATUS_SECRET";
        const string rawBrowserExecutablePath = @"C:\";
        const int expectedEvidenceRefCountPerDefect = 3;
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-status-quality-restart-" + Guid.NewGuid().ToString("N"));

        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var firstVerdict = CreateVerdict(
                "pass131-status-restart-verdict",
                score: 0.93,
                passed: true,
                privateMarker: privateMarker,
                includeDefect: true,
                evidenceImageIds:
                [
                    "expected",
                    $"actual-{privateMarker}",
                    "detail-safe"
                ]);
            var (firstAdapter, firstRenderProvider, firstComparisonProvider) = CreateRegisteredAdapter(
                firstVerdict,
                artifactDb,
                new Dictionary<string, string>
                {
                    ["browser_executable"] = rawBrowserExecutablePath
                },
                evaluatorDecorator: evaluator =>
                    new ThreeEvidenceCorrectionFindingEvaluator(evaluator));
            var package = CreatePackage("pass131-status-restart-readback-request", privateMarker);
            var submitEnvelope = await firstAdapter.TrySubmitAsync(package);
            Assert.Equal(200, submitEnvelope.StatusCode);
            Assert.NotNull(submitEnvelope.ParsedJobId);
            Assert.NotEmpty(firstRenderProvider.Requests);
            Assert.NotEmpty(firstComparisonProvider.Requests);

            var sameProcessStatusEnvelope = await firstAdapter.TryGetJobStatusAsync(
                submitEnvelope.ParsedJobId,
                package.SourceKind);
            AssertStableStatusSuccessEnvelope(
                sameProcessStatusEnvelope,
                submitEnvelope,
                package,
                privateMarker,
                expectedContractStatus: DesignConversionThinClientStatus.NotReady);
            using var sameProcessDocument = JsonDocument.Parse(sameProcessStatusEnvelope.ResponseBodyJson);
            var sameProcessRoot = sameProcessDocument.RootElement.Clone();
            AssertStatusQualityMetricsAvailable(
                sameProcessRoot,
                expectedScore: 0.0,
                expectedRequiredThreshold: 0.85,
                expectedDefectCount: 2,
                expectedPassed: false,
                expectedThresholdMet: false);
            AssertStatusDefectsSafe(
                sameProcessRoot,
                privateMarker,
                expectedEvidenceRefCountPerDefect);

            var freshRenderProvider = new RecordingRenderProvider();
            var freshComparisonProvider = new RecordingComparisonProvider(CreateVerdict(
                "pass131-status-restart-should-not-run",
                score: 0.20,
                passed: false,
                privateMarker: "PRIVATE_PASS131_SHOULD_NOT_RERUN",
                includeDefect: true));
            var freshService = new DesignConversionInternalService(freshRenderProvider, freshComparisonProvider, routingFlow: null);
            var freshApi = new DesignConversionModelServerApi(
                new DesignConversionInternalServiceThinClientJobEvaluator(freshService),
                () => DateTimeOffset.Parse("2026-07-03T14:10:00Z"),
                artifactDb);
            var freshRegistry = new DesignConversionModelServerEndpointRegistry();
            var statusRegistered = freshRegistry.RegisterDesignConversionStatusReadback(new DesignConversionModelServerStatusHandler(freshApi));
            Assert.True(statusRegistered.IsSuccess, statusRegistered.ErrorMessage);
            var freshAdapter = new DesignConversionModelServerLocalCallerAdapter(freshRegistry);

            var statusEnvelope = await freshAdapter.TryGetJobStatusAsync(
                submitEnvelope.ParsedJobId,
                package.SourceKind);

            AssertStableStatusSuccessEnvelope(
                statusEnvelope,
                submitEnvelope,
                package,
                privateMarker,
                expectedContractStatus: DesignConversionThinClientStatus.NotReady);
            Assert.Empty(freshRenderProvider.Requests);
            Assert.Empty(freshComparisonProvider.Requests);
            Assert.False(statusEnvelope.RuntimeInvoked);
            Assert.Equal(bool.FalseString, statusEnvelope.BoundaryMetadata["job_rerun_invoked"]);

            using var document = JsonDocument.Parse(statusEnvelope.ResponseBodyJson);
            var root = document.RootElement;
            var artifacts = root.GetProperty("artifacts").EnumerateArray().ToArray();
            var renderArtifacts = artifacts
                .Where(artifact => string.Equals(
                    artifact.GetProperty("kind").GetString(),
                    "actual-render",
                    StringComparison.Ordinal))
                .ToArray();
            Assert.NotEmpty(renderArtifacts);
            Assert.All(renderArtifacts, renderArtifact =>
            {
                var renderSha256 = Assert.IsType<string>(renderArtifact.GetProperty("sha256").GetString());
                Assert.Matches("^sha256:[0-9a-f]{64}$", renderSha256);
                var renderMetadata = renderArtifact.GetProperty("metadata");
                Assert.True(renderMetadata.TryGetProperty("browser_executable", out var browserExecutableRef));
                var retainedBrowserExecutableRef = Assert.IsType<string>(browserExecutableRef.GetString());
                Assert.Matches("^sha256:[0-9a-f]{64}$", retainedBrowserExecutableRef);
                Assert.NotEqual(rawBrowserExecutablePath, retainedBrowserExecutableRef);
            });
            Assert.DoesNotContain(rawBrowserExecutablePath, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(
                JsonSerializer.Serialize(rawBrowserExecutablePath),
                statusEnvelope.ResponseBodyJson,
                StringComparison.Ordinal);
            AssertStatusQualityMetricsAvailable(
                root,
                expectedScore: sameProcessRoot.GetProperty("qualityMetrics").GetProperty("score").GetDouble(),
                expectedRequiredThreshold: sameProcessRoot.GetProperty("qualityMetrics").GetProperty("requiredThreshold").GetDouble(),
                expectedDefectCount: sameProcessRoot.GetProperty("qualityMetrics").GetProperty("defectCount").GetInt32(),
                expectedPassed: sameProcessRoot.GetProperty("qualityMetrics").GetProperty("passed").GetBoolean(),
                expectedThresholdMet: sameProcessRoot.GetProperty("qualityMetrics").GetProperty("thresholdMet").GetBoolean());
            Assert.Equal(
                sameProcessRoot.GetProperty("defects").GetRawText(),
                root.GetProperty("defects").GetRawText());
            AssertStatusDefectsSafe(
                root,
                privateMarker,
                expectedEvidenceRefCountPerDefect);
            Assert.Equal(bool.TrueString, root.GetProperty("boundaryMetadata").GetProperty("quality_metrics_available").GetString());
            Assert.Equal("final_visual_verdict", root.GetProperty("boundaryMetadata").GetProperty("quality_metrics_source").GetString());
            Assert.Equal("2", root.GetProperty("boundaryMetadata").GetProperty("status_defect_count").GetString());
            Assert.Equal(bool.TrueString, root.GetProperty("boundaryMetadata").GetProperty("status_defects_refs_only").GetString());
        }
        finally
        {
            if (Directory.Exists(artifactDbRoot))
                Directory.Delete(artifactDbRoot, recursive: true);
        }
    }

    [Fact]
    public async Task StatusRoute_DeniesArtifactStoreRefsWithoutExplicitOwnerMetadata()
    {
        const string jobId = "thin-job-artifact-access-policy-missing-owner";
        const string requestId = "artifact-access-policy-missing-owner-request";
        const string rawStorageRef =
            "artifact-store/status-unbound-tenant/artifact-access-policy-missing-owner/render-safe/blob";
        const string rawMetadataStorageRef =
            "artifact-store/status-unbound-tenant/artifact-access-policy-missing-owner/metadata/blob";
        const string signedHttpsUrl =
            "https://signed.example.test/render.png?X-Amz-Signature=abc123&X-Amz-Credential=tenant-alpha&token=opaque-token";
        const string signedHttpUrl =
            "http://signed.example.test/render.png?sig=abc123&token=legacy-token";
        const string fileMetadataRef = "file:///C:/tenant/status/render.png";
        const string localMetadataPath = @"C:\tenant\status\render.png";
        const string providerPrivateLabel = "provider-private";
        const string rawStorageLabel = "raw-storage-ref";
        var record = CreateArtifactAccessPolicyStatusRecord(
            jobId,
            requestId,
            [
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-missing-owner-render",
                    kind: "actual-render",
                    uriRef: rawStorageRef,
                    sha256: "sha256:" + new string('1', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["route_status_fixture"] = "missing-owner-artifact-store-ref",
                        ["render_storage_ref"] = rawMetadataStorageRef,
                        ["render_signed_url"] = signedHttpsUrl,
                        ["render_http_url"] = signedHttpUrl,
                        ["render_file_ref"] = fileMetadataRef,
                        ["render_local_path"] = localMetadataPath,
                        ["provider_access_label"] = providerPrivateLabel,
                        ["render_storage_label"] = rawStorageLabel
                    })
            ]);

        var invocation = await InvokeStatusReadbackForArtifactAccessRecordAsync(record);

        Assert.Equal(200, invocation.Response.StatusCode);
        Assert.Empty(invocation.RenderProvider.Requests);
        Assert.Empty(invocation.ComparisonProvider.Requests);
        Assert.DoesNotContain(rawStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawMetadataStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(signedHttpsUrl, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(signedHttpUrl, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fileMetadataRef, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(localMetadataPath, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerPrivateLabel, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawStorageLabel, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("X-Amz-Signature", invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact-store/", invocation.Response.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        var root = document.RootElement;
        var artifact = Assert.Single(root.GetProperty("artifacts").EnumerateArray());
        var uri = Assert.IsType<string>(artifact.GetProperty("uri").GetString());
        Assert.StartsWith("artifact-access-denied:sha256:", uri, StringComparison.Ordinal);
        var metadata = artifact.GetProperty("metadata");
        Assert.Equal("deny", metadata.GetProperty("artifact_access_policy_decision").GetString());
        Assert.Equal(ArtifactAccessPolicy.TenantMismatchCode, metadata.GetProperty("artifact_access_policy_reason").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("artifact_access_policy_purpose").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("artifact_access_policy_classification").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("artifact_access_policy_scope_ref").GetString());
        Assert.Matches(
            "^sha256:[0-9a-f]{64}$",
            metadata.GetProperty("artifact_access_policy_redacted_storage_ref_hash").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_access_policy_raw_storage_ref_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_access_policy_raw_uri_exposed").GetString());
        AssertArtifactMetadataValueHashed(metadata, "render_storage_ref", rawMetadataStorageRef);
        AssertArtifactMetadataValueHashed(metadata, "render_signed_url", signedHttpsUrl);
        AssertArtifactMetadataValueHashed(metadata, "render_http_url", signedHttpUrl);
        AssertArtifactMetadataValueHashed(metadata, "render_file_ref", fileMetadataRef);
        AssertArtifactMetadataValueHashed(metadata, "render_local_path", localMetadataPath);
        AssertArtifactMetadataValueHashed(metadata, "provider_access_label", providerPrivateLabel);
        AssertArtifactMetadataValueHashed(metadata, "render_storage_label", rawStorageLabel);

        var boundary = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundary.GetProperty("status_readback_response_refs_only").GetString());
        Assert.Equal("1", boundary.GetProperty("status_readback_artifact_ref_count").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());
    }

    [Fact]
    public async Task StatusRoute_RedactsWindowsStyleMetadataPathsOnAnyRuntimeOs()
    {
        const string jobId = "thin-job-artifact-access-policy-windows-path-metadata";
        const string requestId = "artifact-access-policy-windows-path-metadata-request";
        const string rawStorageRef =
            "artifact-store/status-unbound-tenant/artifact-access-policy-windows-path-metadata/render-safe/blob";
        const string driveBackslashPath = @"C:\tenant\status\drive-backslash.png";
        const string driveSlashPath = "C:/tenant/status/drive-slash.png";
        const string driveBackslashRoot = @"C:\";
        const string driveSlashRoot = "C:/";
        const string uncBackslashPath = @"\\server\share\status\render.png";
        const string uncSlashPath = "//server/share/status/render.png";
        const string safeLabel = "safe-public-render-label";
        var canonicalSha256 = "sha256:" + new string('a', 64);
        // Этот набор ловит именно платформенную дыру: Windows paths остаются чувствительными даже в Linux/WSL testhost.
        var record = CreateArtifactAccessPolicyStatusRecord(
            jobId,
            requestId,
            [
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-windows-path-metadata-render",
                    kind: "actual-render",
                    uriRef: rawStorageRef,
                    sha256: "sha256:" + new string('0', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["drive_backslash_path"] = driveBackslashPath,
                        ["drive_slash_path"] = driveSlashPath,
                        ["drive_backslash_root"] = driveBackslashRoot,
                        ["drive_slash_root"] = driveSlashRoot,
                        ["unc_backslash_path"] = uncBackslashPath,
                        ["unc_slash_path"] = uncSlashPath,
                        ["safe_label"] = safeLabel,
                        ["canonical_sha256"] = canonicalSha256
                    })
            ]);

        var invocation = await InvokeStatusReadbackForArtifactAccessRecordAsync(record);

        Assert.Equal(200, invocation.Response.StatusCode);
        Assert.Empty(invocation.RenderProvider.Requests);
        Assert.Empty(invocation.ComparisonProvider.Requests);
        Assert.DoesNotContain(driveBackslashPath, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(driveSlashPath, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(driveBackslashRoot, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(driveSlashRoot, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(uncBackslashPath, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(uncSlashPath, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        var artifact = Assert.Single(document.RootElement.GetProperty("artifacts").EnumerateArray());
        var metadata = artifact.GetProperty("metadata");
        // Безопасные публичные значения остаются читаемыми: фикс не должен превращать все metadata в непрозрачные хеши.
        AssertArtifactMetadataValueHashed(metadata, "drive_backslash_path", driveBackslashPath);
        AssertArtifactMetadataValueHashed(metadata, "drive_slash_path", driveSlashPath);
        AssertArtifactMetadataValueHashed(metadata, "drive_backslash_root", driveBackslashRoot);
        AssertArtifactMetadataValueHashed(metadata, "drive_slash_root", driveSlashRoot);
        AssertArtifactMetadataValueHashed(metadata, "unc_backslash_path", uncBackslashPath);
        AssertArtifactMetadataValueHashed(metadata, "unc_slash_path", uncSlashPath);
        Assert.Equal(safeLabel, metadata.GetProperty("safe_label").GetString());
        Assert.Equal(canonicalSha256, metadata.GetProperty("canonical_sha256").GetString());
    }

    [Fact]
    public async Task StatusRoute_BrokersArtifactStoreRefsOnlyForTenantBoundExplicitSameOwnerMetadata()
    {
        const string jobId = "thin-job-artifact-access-policy-explicit-owner";
        const string requestId = "artifact-access-policy-explicit-owner-request";
        const string admissionToken = "current-route-token-artifact-owner-0123456789";
        const string admissionTenantId = "tenant-artifact-access-policy-alpha";
        const string rawStorageRef =
            "artifact-store/tenant-artifact-access-policy-alpha/artifact-access-policy-explicit-owner/render-safe/blob";
        const string rawMetadataStorageRef =
            "artifact-store/tenant-artifact-access-policy-alpha/artifact-access-policy-explicit-owner/metadata/blob";
        const string signedHttpsUrl =
            "https://signed.example.test/render.png?X-Amz-Signature=def456&X-Amz-Credential=tenant-alpha&token=opaque-token";
        const string signedHttpUrl =
            "http://signed.example.test/render.png?sig=def456&token=legacy-token";
        const string fileMetadataRef = "file:///C:/tenant/status/explicit-owner-render.png";
        const string localMetadataPath = @"C:\tenant\status\explicit-owner-render.png";
        const string providerPrivateLabel = "provider-private";
        const string rawStorageLabel = "raw-storage-ref";
        var record = CreateArtifactAccessPolicyStatusRecord(
            jobId,
            requestId,
            [
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-explicit-owner-render",
                    kind: "actual-render",
                    uriRef: rawStorageRef,
                    sha256: "sha256:" + new string('5', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["artifact_access_policy_tenant_id"] = admissionTenantId,
                        ["route_status_fixture"] = "explicit-owner-artifact-store-ref",
                        ["render_storage_ref"] = rawMetadataStorageRef,
                        ["render_signed_url"] = signedHttpsUrl,
                        ["render_http_url"] = signedHttpUrl,
                        ["render_file_ref"] = fileMetadataRef,
                        ["render_local_path"] = localMetadataPath,
                        ["provider_access_label"] = providerPrivateLabel,
                        ["render_storage_label"] = rawStorageLabel
                    })
            ]);

        var invocation = await InvokeTenantBoundStatusReadbackForArtifactAccessRecordAsync(
            record,
            admissionToken,
            admissionTenantId);

        Assert.Equal(200, invocation.Response.StatusCode);
        Assert.Empty(invocation.RenderProvider.Requests);
        Assert.Empty(invocation.ComparisonProvider.Requests);
        Assert.DoesNotContain(rawStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawMetadataStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(signedHttpsUrl, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(signedHttpUrl, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fileMetadataRef, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(localMetadataPath, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerPrivateLabel, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawStorageLabel, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(admissionTenantId, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("X-Amz-Signature", invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact-store/", invocation.Response.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        var root = document.RootElement;
        var artifact = Assert.Single(root.GetProperty("artifacts").EnumerateArray());
        var uri = Assert.IsType<string>(artifact.GetProperty("uri").GetString());
        Assert.StartsWith("brokered-artifact-read:", uri, StringComparison.Ordinal);
        var metadata = artifact.GetProperty("metadata");
        Assert.Equal("allow", metadata.GetProperty("artifact_access_policy_decision").GetString());
        Assert.Equal(ArtifactAccessPolicy.AllowReason, metadata.GetProperty("artifact_access_policy_reason").GetString());
        Assert.Equal(ArtifactAccessPolicy.ResultSafePurpose, metadata.GetProperty("artifact_access_policy_purpose").GetString());
        Assert.Equal(ArtifactAccessPolicy.ResultSafeClassification, metadata.GetProperty("artifact_access_policy_classification").GetString());
        Assert.Matches("^sha256:[0-9a-f]{64}$", metadata.GetProperty("artifact_access_policy_scope_ref").GetString());
        Assert.Matches(
            "^sha256:[0-9a-f]{64}$",
            metadata.GetProperty("artifact_access_policy_redacted_storage_ref_hash").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_access_policy_raw_storage_ref_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_access_policy_raw_uri_exposed").GetString());
        AssertArtifactMetadataValueHashed(metadata, "render_storage_ref", rawMetadataStorageRef);
        AssertArtifactMetadataValueHashed(metadata, "render_signed_url", signedHttpsUrl);
        AssertArtifactMetadataValueHashed(metadata, "render_http_url", signedHttpUrl);
        AssertArtifactMetadataValueHashed(metadata, "render_file_ref", fileMetadataRef);
        AssertArtifactMetadataValueHashed(metadata, "render_local_path", localMetadataPath);
        AssertArtifactMetadataValueHashed(metadata, "provider_access_label", providerPrivateLabel);
        AssertArtifactMetadataValueHashed(metadata, "render_storage_label", rawStorageLabel);

        var boundary = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundary.GetProperty("status_readback_response_refs_only").GetString());
        Assert.Equal("1", boundary.GetProperty("status_readback_artifact_ref_count").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());
    }

    [Fact]
    public async Task StatusRoute_BrokersArtifactStoreRefsForTenantBoundExplicitSameOwnerMetadataRef()
    {
        const string jobId = "thin-job-artifact-access-policy-explicit-owner-ref";
        const string requestId = "artifact-access-policy-explicit-owner-ref-request";
        const string admissionToken = "current-route-token-artifact-owner-ref-0123456789";
        const string admissionTenantId = "tenant-artifact-access-policy-ref-owner";
        var admissionTenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef(admissionTenantId);
        Assert.True(DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionTenantIdRef(admissionTenantIdRef));
        const string rawStorageRef =
            "artifact-store/tenant-artifact-access-policy-ref-owner/artifact-access-policy-explicit-owner-ref/render-safe/blob";
        const string rawMetadataStorageRef =
            "artifact-store/tenant-artifact-access-policy-ref-owner/artifact-access-policy-explicit-owner-ref/metadata/blob";
        const string signedHttpsUrl =
            "https://signed.example.test/render.png?X-Amz-Signature=ghi789&X-Amz-Credential=tenant-ref&token=opaque-token";
        const string signedHttpUrl =
            "http://signed.example.test/render.png?sig=ghi789&token=legacy-token";
        const string fileMetadataRef = "file:///C:/tenant/status/explicit-owner-ref-render.png";
        const string localMetadataPath = @"C:\tenant\status\explicit-owner-ref-render.png";
        const string providerPrivateLabel = "provider-private";
        const string rawStorageLabel = "raw-storage-ref";
        var record = CreateArtifactAccessPolicyStatusRecord(
            jobId,
            requestId,
            [
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-explicit-owner-ref-render",
                    kind: "actual-render",
                    uriRef: rawStorageRef,
                    sha256: "sha256:" + new string('6', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["artifact_access_policy_tenant_id_ref"] = admissionTenantIdRef,
                        ["route_status_fixture"] = "explicit-owner-artifact-store-ref-by-tenant-ref",
                        ["render_storage_ref"] = rawMetadataStorageRef,
                        ["render_signed_url"] = signedHttpsUrl,
                        ["render_http_url"] = signedHttpUrl,
                        ["render_file_ref"] = fileMetadataRef,
                        ["render_local_path"] = localMetadataPath,
                        ["provider_access_label"] = providerPrivateLabel,
                        ["render_storage_label"] = rawStorageLabel
                    })
            ]);

        var invocation = await InvokeTenantBoundStatusReadbackForArtifactAccessRecordAsync(
            record,
            admissionToken,
            admissionTenantId);

        Assert.Equal(200, invocation.Response.StatusCode);
        Assert.Empty(invocation.RenderProvider.Requests);
        Assert.Empty(invocation.ComparisonProvider.Requests);
        Assert.DoesNotContain(rawStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawMetadataStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(signedHttpsUrl, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(signedHttpUrl, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fileMetadataRef, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(localMetadataPath, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerPrivateLabel, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawStorageLabel, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(admissionTenantId, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("X-Amz-Signature", invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact-store/", invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact_access_policy_tenant_id_ref", invocation.Response.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        var root = document.RootElement;
        var artifact = Assert.Single(root.GetProperty("artifacts").EnumerateArray());
        var uri = Assert.IsType<string>(artifact.GetProperty("uri").GetString());
        Assert.StartsWith("brokered-artifact-read:", uri, StringComparison.Ordinal);
        var metadata = artifact.GetProperty("metadata");
        Assert.Equal("allow", metadata.GetProperty("artifact_access_policy_decision").GetString());
        Assert.Equal(ArtifactAccessPolicy.AllowReason, metadata.GetProperty("artifact_access_policy_reason").GetString());
        Assert.Equal(ArtifactAccessPolicy.ResultSafePurpose, metadata.GetProperty("artifact_access_policy_purpose").GetString());
        Assert.Equal(ArtifactAccessPolicy.ResultSafeClassification, metadata.GetProperty("artifact_access_policy_classification").GetString());
        Assert.Matches("^sha256:[0-9a-f]{64}$", metadata.GetProperty("artifact_access_policy_scope_ref").GetString());
        Assert.Matches(
            "^sha256:[0-9a-f]{64}$",
            metadata.GetProperty("artifact_access_policy_redacted_storage_ref_hash").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_access_policy_raw_storage_ref_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_access_policy_raw_uri_exposed").GetString());
        AssertArtifactMetadataValueHashed(metadata, "render_storage_ref", rawMetadataStorageRef);
        AssertArtifactMetadataValueHashed(metadata, "render_signed_url", signedHttpsUrl);
        AssertArtifactMetadataValueHashed(metadata, "render_http_url", signedHttpUrl);
        AssertArtifactMetadataValueHashed(metadata, "render_file_ref", fileMetadataRef);
        AssertArtifactMetadataValueHashed(metadata, "render_local_path", localMetadataPath);
        AssertArtifactMetadataValueHashed(metadata, "provider_access_label", providerPrivateLabel);
        AssertArtifactMetadataValueHashed(metadata, "render_storage_label", rawStorageLabel);

        var boundary = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundary.GetProperty("status_readback_response_refs_only").GetString());
        Assert.Equal("1", boundary.GetProperty("status_readback_artifact_ref_count").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());
    }

    [Fact]
    public async Task StatusRoute_BrokersArtifactStoreRefWhenRawAndRefOwnerClaimsMatchSameAdmission()
    {
        const string admissionTenantId = "tenant-artifact-access-policy-dual-owner";
        var admissionTenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef(admissionTenantId);
        const string rawStorageRef =
            "artifact-store/tenant-artifact-access-policy-dual-owner/dual-owner/render-safe/blob";
        var record = CreateArtifactAccessPolicyStatusRecord(
            "thin-job-artifact-access-policy-dual-owner",
            "artifact-access-policy-dual-owner-request",
            [
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-dual-owner-render",
                    kind: "actual-render",
                    uriRef: rawStorageRef,
                    sha256: "sha256:" + new string('7', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["artifact_access_policy_tenant_id"] = admissionTenantId,
                        ["artifact_access_policy_tenant_id_ref"] = admissionTenantIdRef
                    })
            ]);

        var invocation = await InvokeTenantBoundStatusReadbackForArtifactAccessRecordAsync(
            record,
            "current-route-token-artifact-dual-owner-0123456789",
            admissionTenantId);

        Assert.Equal(200, invocation.Response.StatusCode);
        Assert.DoesNotContain(rawStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(admissionTenantId, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "artifact_access_policy_tenant_id_ref",
            invocation.Response.BodyJson,
            StringComparison.Ordinal);
        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        var artifact = Assert.Single(document.RootElement.GetProperty("artifacts").EnumerateArray());
        Assert.StartsWith(
            "brokered-artifact-read:",
            Assert.IsType<string>(artifact.GetProperty("uri").GetString()),
            StringComparison.Ordinal);
        Assert.Equal(
            "allow",
            artifact.GetProperty("metadata").GetProperty("artifact_access_policy_decision").GetString());
    }

    [Fact]
    public async Task StatusRoute_DeniesConflictingRawAndRefOwnerClaimsWithoutLeakingEitherClaim()
    {
        const string admissionTenantId = "tenant-artifact-access-policy-conflicting-owner";
        var conflictingTenantIdRef =
            DesignConversionModelActivationRegistry.BuildTenantIdRef("foreign-conflicting-owner");
        const string rawStorageRef =
            "artifact-store/tenant-artifact-access-policy-conflicting-owner/conflicting-owner/render-safe/blob";
        var record = CreateArtifactAccessPolicyStatusRecord(
            "thin-job-artifact-access-policy-conflicting-owner",
            "artifact-access-policy-conflicting-owner-request",
            [
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-conflicting-owner-render",
                    kind: "actual-render",
                    uriRef: rawStorageRef,
                    sha256: "sha256:" + new string('8', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["artifact_access_policy_tenant_id"] = admissionTenantId,
                        ["artifact_access_policy_tenant_id_ref"] = conflictingTenantIdRef
                    })
            ]);

        var invocation = await InvokeTenantBoundStatusReadbackForArtifactAccessRecordAsync(
            record,
            "current-route-token-artifact-conflicting-owner-0123456789",
            admissionTenantId);

        Assert.Equal(200, invocation.Response.StatusCode);
        Assert.DoesNotContain(rawStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(admissionTenantId, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(conflictingTenantIdRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        var artifact = Assert.Single(document.RootElement.GetProperty("artifacts").EnumerateArray());
        Assert.StartsWith(
            "artifact-access-denied:sha256:",
            Assert.IsType<string>(artifact.GetProperty("uri").GetString()),
            StringComparison.Ordinal);
        Assert.Equal(
            ArtifactAccessPolicy.TenantMismatchCode,
            artifact.GetProperty("metadata").GetProperty("artifact_access_policy_reason").GetString());
    }

    [Theory]
    [InlineData("artifact_access_policy_tenant_id")]
    [InlineData("artifact_access_policy_tenant_id_ref")]
    public async Task StatusRoute_DeniesMalformedDualOwnerClaimEvenWhenOtherClaimMatches(string malformedKey)
    {
        const string admissionTenantId = "tenant-artifact-access-policy-malformed-owner";
        var admissionTenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef(admissionTenantId);
        const string rawStorageRef =
            "artifact-store/tenant-artifact-access-policy-malformed-owner/malformed-owner/render-safe/blob";
        var metadata = new Dictionary<string, string>
        {
            ["artifact_access_policy_tenant_id"] = admissionTenantId,
            ["artifact_access_policy_tenant_id_ref"] = admissionTenantIdRef,
            [malformedKey] = " "
        };
        var caseSuffix = string.Equals(
            malformedKey,
            "artifact_access_policy_tenant_id_ref",
            StringComparison.Ordinal)
            ? "ref"
            : "raw";
        var record = CreateArtifactAccessPolicyStatusRecord(
            "thin-job-artifact-access-policy-malformed-owner-" + caseSuffix,
            "artifact-access-policy-malformed-owner-request-" + caseSuffix,
            [
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-malformed-owner-render",
                    kind: "actual-render",
                    uriRef: rawStorageRef,
                    sha256: "sha256:" + new string('9', 64),
                    metadata: metadata)
            ]);

        var invocation = await InvokeTenantBoundStatusReadbackForArtifactAccessRecordAsync(
            record,
            "current-route-token-artifact-malformed-owner-0123456789",
            admissionTenantId);

        Assert.Equal(200, invocation.Response.StatusCode);
        Assert.DoesNotContain(rawStorageRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(admissionTenantId, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "artifact_access_policy_tenant_id_ref",
            invocation.Response.BodyJson,
            StringComparison.Ordinal);
        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        var artifact = Assert.Single(document.RootElement.GetProperty("artifacts").EnumerateArray());
        Assert.StartsWith(
            "artifact-access-denied:sha256:",
            Assert.IsType<string>(artifact.GetProperty("uri").GetString()),
            StringComparison.Ordinal);
        Assert.Equal(
            ArtifactAccessPolicy.TenantMismatchCode,
            artifact.GetProperty("metadata").GetProperty("artifact_access_policy_reason").GetString());
    }

    [Fact]
    public async Task StatusRoute_RedactsPolicyDeniedArtifactRefsWithoutLeakingRawStorage()
    {
        const string jobId = "thin-job-artifact-access-policy-denied";
        const string requestId = "artifact-access-policy-denied-request";
        const string admissionToken = "current-route-token-artifact-denied-0123456789";
        const string admissionTenantId = "tenant-artifact-access-policy-denied";
        const string crossTenantRef =
            "artifact-store/status-unbound-tenant/artifact-access-policy-denied/cross-tenant/blob";
        const string providerPrivateRef =
            "artifact-store/status-unbound-tenant/artifact-access-policy-denied/provider/blob";
        const string localPrivateRef = "file:///C:/tenant-private/status-output.png";
        var record = CreateArtifactAccessPolicyStatusRecord(
            jobId,
            requestId,
            [
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-cross-tenant",
                    kind: "actual-render",
                    uriRef: crossTenantRef,
                    sha256: "sha256:" + new string('2', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["artifact_access_policy_tenant_id"] = "foreign-tenant"
                    }),
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-provider-private",
                    kind: "actual-render",
                    uriRef: providerPrivateRef,
                    sha256: "sha256:" + new string('3', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["artifact_access_policy_tenant_id"] = admissionTenantId,
                        ["artifact_access_policy_purpose"] = ArtifactAccessPolicy.ProviderPrivatePurpose,
                        ["artifact_access_policy_classification"] = ArtifactAccessPolicy.ProviderPrivateClassification
                    }),
                CreateArtifactAccessPolicyRenderRef(
                    artifactId: "artifact-access-policy-local-path",
                    kind: "actual-render",
                    uriRef: localPrivateRef,
                    sha256: "sha256:" + new string('4', 64),
                    metadata: new Dictionary<string, string>
                    {
                        ["artifact_access_policy_tenant_id"] = admissionTenantId
                    })
            ]);

        var invocation = await InvokeTenantBoundStatusReadbackForArtifactAccessRecordAsync(
            record,
            admissionToken,
            admissionTenantId);

        Assert.Equal(200, invocation.Response.StatusCode);
        Assert.Empty(invocation.RenderProvider.Requests);
        Assert.Empty(invocation.ComparisonProvider.Requests);
        Assert.DoesNotContain(crossTenantRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(providerPrivateRef, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(localPrivateRef, invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("artifact-store/", invocation.Response.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        var root = document.RootElement;
        var artifacts = root.GetProperty("artifacts").EnumerateArray().ToArray();
        Assert.Equal(3, artifacts.Length);
        Assert.All(artifacts, artifact =>
        {
            var uri = Assert.IsType<string>(artifact.GetProperty("uri").GetString());
            Assert.StartsWith("artifact-access-denied:sha256:", uri, StringComparison.Ordinal);
            var metadata = artifact.GetProperty("metadata");
            Assert.Equal("deny", metadata.GetProperty("artifact_access_policy_decision").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_access_policy_raw_storage_ref_exposed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_access_policy_raw_uri_exposed").GetString());
        });
        Assert.Contains(
            artifacts,
            artifact => string.Equals(
                ArtifactAccessPolicy.TenantMismatchCode,
                artifact.GetProperty("metadata").GetProperty("artifact_access_policy_reason").GetString(),
                StringComparison.Ordinal));
        Assert.Contains(
            artifacts,
            artifact => string.Equals(
                ArtifactAccessPolicy.RoleDeniedCode,
                artifact.GetProperty("metadata").GetProperty("artifact_access_policy_reason").GetString(),
                StringComparison.Ordinal));
        Assert.Contains(
            artifacts,
            artifact => string.Equals(
                ArtifactAccessPolicy.StorageRefUnsafeCode,
                artifact.GetProperty("metadata").GetProperty("artifact_access_policy_reason").GetString(),
                StringComparison.Ordinal));

        var boundary = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, boundary.GetProperty("status_readback_response_refs_only").GetString());
        Assert.Equal("3", boundary.GetProperty("status_readback_artifact_ref_count").GetString());
        Assert.False(root.GetProperty("productReady").GetBoolean());
    }

    [Theory]
    [InlineData(
        DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
        DesignConversionThinClientStatus.Completed,
        "accepted")]
    [InlineData(
        DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine,
        DesignConversionThinClientStatus.Quarantined,
        "quarantine")]
    [InlineData(
        DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
        DesignConversionThinClientStatus.NotReady,
        "retry")]
    [InlineData(
        DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
        DesignConversionThinClientStatus.NotReady,
        "blocked")]
    public async Task TryGetJobStatusAsync_RestartKnownVisualDecisionRestoresMappedStatusWithoutRerunOrLeak(
        string visualDecision,
        string expectedStatus,
        string caseName)
    {
        var jobId = $"pass131-status-{caseName}-restore-job";
        var requestId = $"pass131-status-{caseName}-restore-request";
        var privateMarker = $"PRIVATE_PASS131_{caseName.ToUpperInvariant()}_RESTORE_SHOULD_NOT_RERUN";
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-status-{caseName}-restart-" + Guid.NewGuid().ToString("N"));

        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var append = await artifactDb.AppendRecordAsync(CreateStatusRestoreRecord(
                jobId,
                requestId,
                visualDecision));
            Assert.True(append.IsSuccess, append.ErrorMessage);
            var persistedRecord = await ReadSingleRecordAsync(artifactDb, jobId);
            Assert.Equal(visualDecision, persistedRecord.VisualJudgeStatus.Metadata["visual_decision"]);

            var freshArtifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var (freshAdapter, freshRenderProvider, freshComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    $"pass131-status-{caseName}-restore-should-not-run",
                    score: 0.20,
                    passed: false,
                    privateMarker: privateMarker,
                    includeDefect: true),
                freshArtifactDb);

            var statusEnvelope = await freshAdapter.TryGetJobStatusAsync(
                jobId,
                nameof(DesignConversionSourceKind.Figma));

            Assert.Equal("success", statusEnvelope.EnvelopeKind);
            Assert.Equal(200, statusEnvelope.StatusCode);
            Assert.Equal(expectedStatus, statusEnvelope.ParsedContractStatus);
            Assert.Equal(requestId, statusEnvelope.ParsedRequestId);
            Assert.Equal(jobId, statusEnvelope.ParsedJobId);
            Assert.Empty(freshRenderProvider.Requests);
            Assert.Empty(freshComparisonProvider.Requests);
            Assert.False(statusEnvelope.RuntimeInvoked);
            Assert.Equal(bool.FalseString, statusEnvelope.BoundaryMetadata["job_rerun_invoked"]);
            Assert.Equal(expectedStatus, statusEnvelope.BoundaryMetadata["runtime_contract_status"]);
            Assert.Equal(expectedStatus, statusEnvelope.BoundaryMetadata["status_restore_contract_status"]);
            Assert.Equal(visualDecision, statusEnvelope.BoundaryMetadata["status_restore_visual_decision"]);
            Assert.Equal(
                "visual_decision_metadata",
                statusEnvelope.BoundaryMetadata["status_restore_status_source"]);
            Assert.Equal(
                bool.TrueString,
                statusEnvelope.BoundaryMetadata["status_restore_visual_decision_metadata_accepted"]);
            Assert.Equal(
                bool.FalseString,
                statusEnvelope.BoundaryMetadata["status_restore_fail_closed_visual_decision_fallback"]);
            Assert.Equal(
                bool.FalseString,
                statusEnvelope.BoundaryMetadata["status_restore_legacy_completed_fallback"]);

            using var document = JsonDocument.Parse(statusEnvelope.ResponseBodyJson);
            var root = document.RootElement;
            Assert.Equal(expectedStatus, root.GetProperty("contractStatus").GetString());
            Assert.Equal(expectedStatus, root.GetProperty("jobStatus").GetString());
            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(expectedStatus, metadata.GetProperty("status_restore_contract_status").GetString());
            Assert.Equal(visualDecision, metadata.GetProperty("status_restore_visual_decision").GetString());
            Assert.Equal(
                bool.FalseString,
                metadata.GetProperty("status_restore_fail_closed_visual_decision_fallback").GetString());
            Assert.Equal(
                bool.FalseString,
                metadata.GetProperty("status_restore_legacy_completed_fallback").GetString());
            var warnings = root.GetProperty("warnings")
                .EnumerateArray()
                .Select(warning => Assert.IsType<string>(warning.GetString()))
                .ToArray();
            Assert.Contains(
                $"local_artifact_db_status_restore_contract_status={expectedStatus}",
                warnings);
            Assert.Contains(
                $"local_artifact_db_status_restore_visual_decision={visualDecision}",
                warnings);
            Assert.Contains(
                "local_artifact_db_status_restore_fail_closed_visual_decision_fallback=False",
                warnings);
            Assert.DoesNotContain(privateMarker, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(artifactDbRoot))
                Directory.Delete(artifactDbRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData("missing", null, "missing")]
    [InlineData("blank", "   ", "blank")]
    [InlineData("unknown", "PRIVATE_PASS131_UNKNOWN_VISUAL_DECISION_RAW_SHOULD_NOT_LEAK", "unknown")]
    public async Task TryGetJobStatusAsync_RestartMissingBlankOrUnknownVisualDecisionFailsClosedNotReadyWithoutRerunOrLeak(
        string caseName,
        string? rawVisualDecision,
        string expectedRestoreDecision)
    {
        var jobId = $"pass131-status-{caseName}-decision-restore-job";
        var requestId = $"pass131-status-{caseName}-decision-restore-request";
        var privateMarker = $"PRIVATE_PASS131_{caseName.ToUpperInvariant()}_DECISION_SHOULD_NOT_RERUN";
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-status-{caseName}-decision-restart-" + Guid.NewGuid().ToString("N"));

        try
        {
            var seededRecord = CreateStatusRestoreRecord(
                jobId,
                requestId,
                DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine);
            var metadataWithoutDecision = new Dictionary<string, string>(
                seededRecord.VisualJudgeStatus.Metadata,
                StringComparer.Ordinal);
            if (rawVisualDecision is null)
            {
                Assert.True(metadataWithoutDecision.Remove("visual_decision"));
            }
            else
            {
                metadataWithoutDecision["visual_decision"] = rawVisualDecision;
            }

            var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var append = await artifactDb.AppendRecordAsync(seededRecord with
            {
                VisualJudgeStatus = seededRecord.VisualJudgeStatus with
                {
                    EvidenceRefs = [$"visual_decision:{expectedRestoreDecision}"],
                    Metadata = metadataWithoutDecision
                }
            });
            Assert.True(append.IsSuccess, append.ErrorMessage);
            var persistedRecord = await ReadSingleRecordAsync(artifactDb, jobId);
            if (rawVisualDecision is null)
                Assert.False(persistedRecord.VisualJudgeStatus.Metadata.ContainsKey("visual_decision"));
            else
                Assert.True(persistedRecord.VisualJudgeStatus.Metadata.ContainsKey("visual_decision"));

            var freshArtifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var (freshAdapter, freshRenderProvider, freshComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    $"pass131-status-{caseName}-decision-restore-should-not-run",
                    score: 0.20,
                    passed: false,
                    privateMarker: privateMarker,
                    includeDefect: true),
                freshArtifactDb);

            var statusEnvelope = await freshAdapter.TryGetJobStatusAsync(
                jobId,
                nameof(DesignConversionSourceKind.Figma));

            Assert.Equal("success", statusEnvelope.EnvelopeKind);
            Assert.Equal(200, statusEnvelope.StatusCode);
            Assert.Equal(DesignConversionThinClientStatus.NotReady, statusEnvelope.ParsedContractStatus);
            Assert.Empty(freshRenderProvider.Requests);
            Assert.Empty(freshComparisonProvider.Requests);
            Assert.False(statusEnvelope.RuntimeInvoked);
            Assert.Equal(
                DesignConversionThinClientStatus.NotReady,
                statusEnvelope.BoundaryMetadata["status_restore_contract_status"]);
            Assert.Equal(expectedRestoreDecision, statusEnvelope.BoundaryMetadata["status_restore_visual_decision"]);
            Assert.Equal(
                "fail_closed_visual_decision_fallback",
                statusEnvelope.BoundaryMetadata["status_restore_status_source"]);
            Assert.Equal(
                bool.FalseString,
                statusEnvelope.BoundaryMetadata["status_restore_visual_decision_metadata_accepted"]);
            Assert.Equal(
                bool.TrueString,
                statusEnvelope.BoundaryMetadata["status_restore_fail_closed_visual_decision_fallback"]);
            Assert.Equal(bool.FalseString, statusEnvelope.BoundaryMetadata["status_restore_legacy_completed_fallback"]);

            using var document = JsonDocument.Parse(statusEnvelope.ResponseBodyJson);
            var root = document.RootElement;
            Assert.Equal(DesignConversionThinClientStatus.NotReady, root.GetProperty("contractStatus").GetString());
            Assert.Equal(DesignConversionThinClientStatus.NotReady, root.GetProperty("jobStatus").GetString());
            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(
                DesignConversionThinClientStatus.NotReady,
                metadata.GetProperty("status_restore_contract_status").GetString());
            Assert.Equal(
                expectedRestoreDecision,
                metadata.GetProperty("status_restore_visual_decision").GetString());
            Assert.Equal(
                "fail_closed_visual_decision_fallback",
                metadata.GetProperty("status_restore_status_source").GetString());
            Assert.Equal(
                bool.TrueString,
                metadata.GetProperty("status_restore_fail_closed_visual_decision_fallback").GetString());
            Assert.Equal(
                bool.FalseString,
                metadata.GetProperty("status_restore_legacy_completed_fallback").GetString());
            var warnings = root.GetProperty("warnings")
                .EnumerateArray()
                .Select(warning => Assert.IsType<string>(warning.GetString()))
                .ToArray();
            Assert.Contains(
                $"local_artifact_db_status_restore_contract_status={DesignConversionThinClientStatus.NotReady}",
                warnings);
            Assert.Contains(
                $"local_artifact_db_status_restore_visual_decision={expectedRestoreDecision}",
                warnings);
            Assert.Contains(
                "local_artifact_db_status_restore_status_source=fail_closed_visual_decision_fallback",
                warnings);
            Assert.Contains("local_artifact_db_status_restore_visual_decision_metadata_accepted=False", warnings);
            Assert.Contains("local_artifact_db_status_restore_fail_closed_visual_decision_fallback=True", warnings);
            Assert.Contains("local_artifact_db_status_restore_legacy_completed_fallback=False", warnings);
            Assert.Contains("runtime_reexecution_invoked=false", warnings);
            if (!string.IsNullOrWhiteSpace(rawVisualDecision))
                Assert.DoesNotContain(rawVisualDecision, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(artifactDbRoot))
                Directory.Delete(artifactDbRoot, recursive: true);
        }
    }

    [Fact]
    public async Task TryGetJobStatusAsync_RestartLegacyRecordWithoutSnapshotFallsBackWithoutRerunOrLeak()
    {
        const string privateMarker = "PRIVATE_PASS131_LEGACY_RESTART_SECRET";
        const string safeVisualVerdictFixtureValue = "safe-pass131-legacy-visual-verdict-fixture";
        const string rawBrowserExecutablePath =
            @"C:\PRIVATE_PASS131_LEGACY_RESTART_SECRET\browser\chrome.exe";
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-status-legacy-restart-" + Guid.NewGuid().ToString("N"));

        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var (firstAdapter, firstRenderProvider, firstComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    "pass131-status-legacy-restart-verdict",
                    score: 0.93,
                    passed: true,
                    privateMarker: safeVisualVerdictFixtureValue,
                    includeDefect: true),
                artifactDb,
                new Dictionary<string, string>
                {
                    ["browser_executable"] = rawBrowserExecutablePath
                });
            var package = CreatePackage("pass131-status-legacy-restart-request", privateMarker);
            var submitEnvelope = await firstAdapter.TrySubmitAsync(package);
            Assert.Equal(200, submitEnvelope.StatusCode);
            Assert.NotNull(submitEnvelope.ParsedJobId);
            Assert.NotEmpty(firstRenderProvider.Requests);
            Assert.NotEmpty(firstComparisonProvider.Requests);

            var originalRecord = await ReadSingleRecordAsync(artifactDb, submitEnvelope.ParsedJobId);
            Assert.True(originalRecord.TryGetCompletedStatusSnapshot(out var originalSnapshot));
            Assert.NotNull(originalSnapshot);
            var legacyVisualMetadata = new Dictionary<string, string>(
                originalRecord.VisualJudgeStatus.Metadata,
                StringComparer.Ordinal);
            Assert.True(legacyVisualMetadata.Remove(
                DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey));

            var appendedLegacyRecord = await AppendStatusSnapshotVariantAsync(
                artifactDb,
                originalRecord,
                legacyVisualMetadata,
                "legacy-no-completed-status-snapshot");
            Assert.False(appendedLegacyRecord.VisualJudgeStatus.Metadata.ContainsKey(
                DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey));
            Assert.False(appendedLegacyRecord.TryGetCompletedStatusSnapshot(out _));

            var freshArtifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var persistedLegacyRecord = await ReadRecordAsync(
                freshArtifactDb,
                submitEnvelope.ParsedJobId,
                appendedLegacyRecord.RecordId);
            Assert.False(persistedLegacyRecord.TryGetCompletedStatusSnapshot(out _));
            var persistedLegacyJson = JsonSerializer.Serialize(persistedLegacyRecord);
            Assert.DoesNotContain(privateMarker, persistedLegacyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(rawBrowserExecutablePath, persistedLegacyJson, StringComparison.Ordinal);

            var (freshAdapter, freshRenderProvider, freshComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    "pass131-status-legacy-restart-should-not-run",
                    score: 0.20,
                    passed: false,
                    privateMarker: "PRIVATE_PASS131_LEGACY_SHOULD_NOT_RERUN",
                    includeDefect: true),
                freshArtifactDb);
            var statusEnvelope = await freshAdapter.TryGetJobStatusAsync(
                submitEnvelope.ParsedJobId,
                package.SourceKind);

            AssertStableStatusSuccessEnvelope(
                statusEnvelope,
                submitEnvelope,
                package,
                privateMarker,
                expectedContractStatus: DesignConversionThinClientStatus.NotReady);
            Assert.Empty(freshRenderProvider.Requests);
            Assert.Empty(freshComparisonProvider.Requests);
            Assert.False(statusEnvelope.RuntimeInvoked);
            using var document = JsonDocument.Parse(statusEnvelope.ResponseBodyJson);
            var root = document.RootElement;
            AssertStatusQualityMetricsUnavailableAfterRestart(root);
            Assert.Empty(root.GetProperty("defects").EnumerateArray());
            Assert.Equal(
                bool.FalseString,
                root.GetProperty("boundaryMetadata").GetProperty("quality_metrics_available").GetString());
            Assert.Equal(
                "not_persisted_after_restart",
                root.GetProperty("boundaryMetadata").GetProperty("quality_metrics_source").GetString());
            Assert.Equal(
                "0",
                root.GetProperty("boundaryMetadata").GetProperty("status_defect_count").GetString());
            Assert.DoesNotContain(rawBrowserExecutablePath, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(
                JsonSerializer.Serialize(rawBrowserExecutablePath),
                statusEnvelope.ResponseBodyJson,
                StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(artifactDbRoot))
                Directory.Delete(artifactDbRoot, recursive: true);
        }
    }

    [Fact]
    public async Task TryGetJobStatusAsync_RestartOversizedPrivateSnapshotIsDroppedAndFallsBackWithoutRerunOrLeak()
    {
        const string privateMarker = "PRIVATE_PASS131_CORRUPT_RESTART_SECRET";
        const string corruptSnapshotMarker = "PRIVATE_RAW_PASS131_CORRUPT_SNAPSHOT_MARKER";
        const string corruptSnapshotPath =
            @"C:\PRIVATE_RAW_PASS131_CORRUPT_SNAPSHOT_MARKER\raw\provider-payload.json";
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-status-corrupt-restart-" + Guid.NewGuid().ToString("N"));

        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var (firstAdapter, firstRenderProvider, firstComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    "pass131-status-corrupt-restart-verdict",
                    score: 0.93,
                    passed: true,
                    privateMarker: "safe-pass131-corrupt-snapshot-verdict-fixture",
                    includeDefect: true),
                artifactDb);
            var package = CreatePackage("pass131-status-corrupt-restart-request", privateMarker);
            var submitEnvelope = await firstAdapter.TrySubmitAsync(package);
            Assert.Equal(200, submitEnvelope.StatusCode);
            Assert.NotNull(submitEnvelope.ParsedJobId);
            Assert.NotEmpty(firstRenderProvider.Requests);
            Assert.NotEmpty(firstComparisonProvider.Requests);

            var originalRecord = await ReadSingleRecordAsync(artifactDb, submitEnvelope.ParsedJobId);
            Assert.True(originalRecord.TryGetCompletedStatusSnapshot(out var originalSnapshot));
            Assert.NotNull(originalSnapshot);
            var oversizedPrivateSnapshot = JsonSerializer.Serialize(new
            {
                schema = "design-conversion-local-artifact-db-completed-status-snapshot",
                version = 1,
                qualityMetrics = originalSnapshot.QualityMetrics,
                defects = originalSnapshot.Defects,
                privateRawMarker = corruptSnapshotMarker,
                rawPath = corruptSnapshotPath,
                oversizedPadding = new string('x', 600_000)
            });
            Assert.True(Encoding.UTF8.GetByteCount(oversizedPrivateSnapshot) > 512 * 1024);

            var corruptVisualMetadata = new Dictionary<string, string>(
                originalRecord.VisualJudgeStatus.Metadata,
                StringComparer.Ordinal)
            {
                [DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey] =
                    oversizedPrivateSnapshot
            };
            var appendedCorruptRecord = await AppendStatusSnapshotVariantAsync(
                artifactDb,
                originalRecord,
                corruptVisualMetadata,
                "oversized-private-completed-status-snapshot");
            Assert.False(appendedCorruptRecord.VisualJudgeStatus.Metadata.ContainsKey(
                DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey));
            Assert.False(appendedCorruptRecord.TryGetCompletedStatusSnapshot(out _));

            var freshArtifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var persistedCorruptRecord = await ReadRecordAsync(
                freshArtifactDb,
                submitEnvelope.ParsedJobId,
                appendedCorruptRecord.RecordId);
            Assert.False(persistedCorruptRecord.VisualJudgeStatus.Metadata.ContainsKey(
                DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey));
            Assert.False(persistedCorruptRecord.TryGetCompletedStatusSnapshot(out _));
            var persistedCorruptJson = JsonSerializer.Serialize(persistedCorruptRecord);
            Assert.DoesNotContain(corruptSnapshotMarker, persistedCorruptJson, StringComparison.Ordinal);
            Assert.DoesNotContain(corruptSnapshotPath, persistedCorruptJson, StringComparison.Ordinal);

            var (freshAdapter, freshRenderProvider, freshComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    "pass131-status-corrupt-restart-should-not-run",
                    score: 0.20,
                    passed: false,
                    privateMarker: "PRIVATE_PASS131_CORRUPT_SHOULD_NOT_RERUN",
                    includeDefect: true),
                freshArtifactDb);
            var statusEnvelope = await freshAdapter.TryGetJobStatusAsync(
                submitEnvelope.ParsedJobId,
                package.SourceKind);

            AssertStableStatusSuccessEnvelope(
                statusEnvelope,
                submitEnvelope,
                package,
                privateMarker,
                expectedContractStatus: DesignConversionThinClientStatus.NotReady);
            Assert.Empty(freshRenderProvider.Requests);
            Assert.Empty(freshComparisonProvider.Requests);
            Assert.False(statusEnvelope.RuntimeInvoked);
            using var document = JsonDocument.Parse(statusEnvelope.ResponseBodyJson);
            var root = document.RootElement;
            AssertStatusQualityMetricsUnavailableAfterRestart(root);
            Assert.Empty(root.GetProperty("defects").EnumerateArray());
            Assert.Equal(
                bool.FalseString,
                root.GetProperty("boundaryMetadata").GetProperty("quality_metrics_available").GetString());
            Assert.Equal(
                "not_persisted_after_restart",
                root.GetProperty("boundaryMetadata").GetProperty("quality_metrics_source").GetString());
            Assert.Equal(
                "0",
                root.GetProperty("boundaryMetadata").GetProperty("status_defect_count").GetString());
            Assert.DoesNotContain(corruptSnapshotMarker, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(corruptSnapshotPath, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(
                JsonSerializer.Serialize(corruptSnapshotPath),
                statusEnvelope.ResponseBodyJson,
                StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(artifactDbRoot))
                Directory.Delete(artifactDbRoot, recursive: true);
        }
    }

    [Fact]
    public async Task TryGetJobStatusAsync_RestartAmbiguousDuplicateMetricsPropertySnapshotFallsBackWithoutRerunOrLeak()
    {
        const string privateMarker = "PRIVATE_PASS131_AMBIGUOUS_SNAPSHOT_SECRET";
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-status-ambiguous-snapshot-" + Guid.NewGuid().ToString("N"));

        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var (firstAdapter, firstRenderProvider, firstComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    "pass131-status-ambiguous-snapshot-verdict",
                    score: 0.93,
                    passed: true,
                    privateMarker: "safe-pass131-ambiguous-snapshot-verdict-fixture",
                    includeDefect: true),
                artifactDb);
            var package = CreatePackage("pass131-status-ambiguous-snapshot-request", privateMarker);
            var submitEnvelope = await firstAdapter.TrySubmitAsync(package);
            Assert.Equal(200, submitEnvelope.StatusCode);
            Assert.NotNull(submitEnvelope.ParsedJobId);
            Assert.NotEmpty(firstRenderProvider.Requests);
            Assert.NotEmpty(firstComparisonProvider.Requests);

            var originalRecord = await ReadSingleRecordAsync(artifactDb, submitEnvelope.ParsedJobId);
            Assert.True(originalRecord.TryGetCompletedStatusSnapshot(out var originalSnapshot));
            Assert.NotNull(originalSnapshot);
            var ambiguousSnapshot = BuildAmbiguousCompletedStatusSnapshot(originalSnapshot);
            Assert.True(Encoding.UTF8.GetByteCount(ambiguousSnapshot) < 512 * 1024);

            var ambiguousVisualMetadata = new Dictionary<string, string>(
                originalRecord.VisualJudgeStatus.Metadata,
                StringComparer.Ordinal)
            {
                [DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey] =
                    ambiguousSnapshot
            };
            var appendedAmbiguousRecord = await AppendStatusSnapshotVariantAsync(
                artifactDb,
                originalRecord,
                ambiguousVisualMetadata,
                "ambiguous-duplicate-metrics-property-snapshot");

            var freshArtifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var persistedAmbiguousRecord = await ReadRecordAsync(
                freshArtifactDb,
                submitEnvelope.ParsedJobId,
                appendedAmbiguousRecord.RecordId);
            var (freshAdapter, freshRenderProvider, freshComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    "pass131-status-ambiguous-snapshot-should-not-run",
                    score: 0.20,
                    passed: false,
                    privateMarker: "PRIVATE_PASS131_AMBIGUOUS_SHOULD_NOT_RERUN",
                    includeDefect: true),
                freshArtifactDb);

            var statusEnvelope = await freshAdapter.TryGetJobStatusAsync(
                submitEnvelope.ParsedJobId,
                package.SourceKind);

            AssertStableStatusSuccessEnvelope(
                statusEnvelope,
                submitEnvelope,
                package,
                privateMarker,
                expectedContractStatus: DesignConversionThinClientStatus.NotReady);
            Assert.Empty(freshRenderProvider.Requests);
            Assert.Empty(freshComparisonProvider.Requests);
            Assert.False(statusEnvelope.RuntimeInvoked);
            using var document = JsonDocument.Parse(statusEnvelope.ResponseBodyJson);
            var root = document.RootElement;
            AssertStatusQualityMetricsUnavailableAfterRestart(root);
            Assert.Empty(root.GetProperty("defects").EnumerateArray());
            Assert.False(persistedAmbiguousRecord.VisualJudgeStatus.Metadata.ContainsKey(
                DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey));
            Assert.False(persistedAmbiguousRecord.TryGetCompletedStatusSnapshot(out _));
            var persistedAmbiguousJson = JsonSerializer.Serialize(persistedAmbiguousRecord);
            Assert.DoesNotContain(privateMarker, persistedAmbiguousJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateMarker, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(artifactDbRoot))
                Directory.Delete(artifactDbRoot, recursive: true);
        }
    }

    [Fact]
    public async Task TryGetJobStatusAsync_RestartAggregateEvidenceBeyondSnapshotEnvelopeFallsBackWithoutRerunOrLeak()
    {
        const string privateMarker = "PRIVATE_PASS131_AGGREGATE_EVIDENCE_SECRET";
        const int snapshotEnvelopeUtf8Bytes = 512 * 1024;
        const int minimumSerializedCanonicalHashRefUtf8Bytes = 71 + 2;
        const int impossibleAggregateEvidenceRefCount =
            (snapshotEnvelopeUtf8Bytes / minimumSerializedCanonicalHashRefUtf8Bytes) + 1;
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-status-aggregate-evidence-" + Guid.NewGuid().ToString("N"));

        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var (firstAdapter, firstRenderProvider, firstComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    "pass131-status-aggregate-evidence-verdict",
                    score: 0.93,
                    passed: true,
                    privateMarker: "safe-pass131-aggregate-evidence-verdict-fixture",
                    includeDefect: true),
                artifactDb,
                evaluatorDecorator: evaluator =>
                    new AggregateEvidenceCorrectionFindingEvaluator(
                        evaluator,
                        impossibleAggregateEvidenceRefCount));
            var package = CreatePackage("pass131-status-aggregate-evidence-request", privateMarker);
            var submitEnvelope = await firstAdapter.TrySubmitAsync(package);
            Assert.Equal(200, submitEnvelope.StatusCode);
            Assert.NotNull(submitEnvelope.ParsedJobId);
            Assert.NotEmpty(firstRenderProvider.Requests);
            Assert.NotEmpty(firstComparisonProvider.Requests);
            var renderCountAfterSubmit = firstRenderProvider.Requests.Count;
            var comparisonCountAfterSubmit = firstComparisonProvider.Requests.Count;

            var sameProcessStatusEnvelope = await firstAdapter.TryGetJobStatusAsync(
                submitEnvelope.ParsedJobId,
                package.SourceKind);
            AssertStableStatusSuccessEnvelope(
                sameProcessStatusEnvelope,
                submitEnvelope,
                package,
                privateMarker,
                expectedContractStatus: DesignConversionThinClientStatus.NotReady);
            Assert.Equal(renderCountAfterSubmit, firstRenderProvider.Requests.Count);
            Assert.Equal(comparisonCountAfterSubmit, firstComparisonProvider.Requests.Count);
            using var sameProcessDocument = JsonDocument.Parse(
                sameProcessStatusEnvelope.ResponseBodyJson);
            var sameProcessRoot = sameProcessDocument.RootElement;
            AssertStatusQualityMetricsAvailable(
                sameProcessRoot,
                expectedScore: 0.0,
                expectedRequiredThreshold: 0.85,
                expectedDefectCount: 2,
                expectedPassed: false,
                expectedThresholdMet: false);
            var sameProcessEvidenceRefs = sameProcessRoot
                .GetProperty("defects")
                .EnumerateArray()
                .SelectMany(defect => defect
                    .GetProperty("evidenceRefs")
                    .EnumerateArray()
                    .Select(reference => Assert.IsType<string>(reference.GetString())))
                .ToArray();
            Assert.Equal(impossibleAggregateEvidenceRefCount, sameProcessEvidenceRefs.Length);
            Assert.Equal(
                sameProcessEvidenceRefs.Length,
                sameProcessEvidenceRefs.Distinct(StringComparer.Ordinal).Count());
            Assert.All(
                sameProcessEvidenceRefs,
                reference => Assert.Matches("^sha256:[0-9a-f]{64}$", reference));

            var persistedRecord = await ReadSingleRecordAsync(artifactDb, submitEnvelope.ParsedJobId);
            Assert.False(persistedRecord.VisualJudgeStatus.Metadata.ContainsKey(
                DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey));
            Assert.False(persistedRecord.TryGetCompletedStatusSnapshot(out _));

            var freshArtifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
            var freshPersistedRecord = await ReadRecordAsync(
                freshArtifactDb,
                submitEnvelope.ParsedJobId,
                persistedRecord.RecordId);
            Assert.False(freshPersistedRecord.VisualJudgeStatus.Metadata.ContainsKey(
                DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey));
            Assert.False(freshPersistedRecord.TryGetCompletedStatusSnapshot(out _));
            Assert.DoesNotContain(
                privateMarker,
                JsonSerializer.Serialize(freshPersistedRecord),
                StringComparison.Ordinal);

            var (freshAdapter, freshRenderProvider, freshComparisonProvider) = CreateRegisteredAdapter(
                CreateVerdict(
                    "pass131-status-aggregate-evidence-should-not-run",
                    score: 0.20,
                    passed: false,
                    privateMarker: "PRIVATE_PASS131_AGGREGATE_SHOULD_NOT_RERUN",
                    includeDefect: true),
                freshArtifactDb);
            var statusEnvelope = await freshAdapter.TryGetJobStatusAsync(
                submitEnvelope.ParsedJobId,
                package.SourceKind);

            AssertStableStatusSuccessEnvelope(
                statusEnvelope,
                submitEnvelope,
                package,
                privateMarker,
                expectedContractStatus: DesignConversionThinClientStatus.NotReady);
            Assert.Empty(freshRenderProvider.Requests);
            Assert.Empty(freshComparisonProvider.Requests);
            Assert.False(statusEnvelope.RuntimeInvoked);
            using var document = JsonDocument.Parse(statusEnvelope.ResponseBodyJson);
            var root = document.RootElement;
            AssertStatusQualityMetricsUnavailableAfterRestart(root);
            Assert.Empty(root.GetProperty("defects").EnumerateArray());
            Assert.DoesNotContain(privateMarker, statusEnvelope.ResponseBodyJson, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(artifactDbRoot))
                Directory.Delete(artifactDbRoot, recursive: true);
        }
    }

    [Fact]
    public void DescribeStatusReadbackRoute_DocumentsLocalOnlyRefsOnlyContract()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeStatusReadbackRoute();

        Assert.Equal(DesignConversionModelServerCallerContract.StatusReadbackRouteSchemaVersion, descriptor.SchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.StatusReadbackRouteBoundary, descriptor.Boundary);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointMethod, descriptor.EndpointMethod);
        Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, descriptor.EndpointPath);
        Assert.Equal(
            RequiredRequestFieldNames(descriptor.RequestFields),
            DesignConversionModelServerCallerContract.StatusReadbackRequiredRequestFieldNames());
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "jobId", Required: true, MayCarryPrivateInput: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "artifacts", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "qualityMetrics", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "defects", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.status_readback_response_refs_only", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.quality_metrics_available", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.status_defect_count", Required: true });
        Assert.Contains(descriptor.LocalCallerEnvelopeFields, field => field is { JsonName: "parsedJobId", Required: false });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "JOB_NOT_FOUND", StatusCode: 404 });
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["status_readback_route_discoverable_by_contract"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["status_readback_response_refs_only"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["socket_opened"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["product_ready"]);
    }

    private static void AssertStableStatusSuccessEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        DesignConversionModelServerLocalCallerAdapterResponse submitEnvelope,
        DesignConversionModelServerLocalCallerPackage package,
        string privateMarker,
        string expectedContractStatus = DesignConversionThinClientStatus.Completed)
    {
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion, envelope.EnvelopeSchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary, envelope.EnvelopeBoundary);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionModelServerLocalCallerAdapter.AdapterBoundary, envelope.AdapterBoundary);
        Assert.Equal(DesignConversionModelServerCallerContract.StatusReadbackRouteSchemaVersion, envelope.ContractSchemaVersion);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointMethod, envelope.EndpointMethod);
        Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, envelope.EndpointPath);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.False(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Null(envelope.ResponseJsonParseError);
        Assert.Equal(expectedContractStatus, envelope.ParsedContractStatus);
        Assert.Equal(package.RequestId, envelope.ParsedRequestId);
        Assert.Equal(submitEnvelope.ParsedJobId, envelope.ParsedJobId);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.Equal(
            RequiredRequestFieldNames(DesignConversionModelServerCallerContract.DescribeStatusReadbackRoute().RequestFields),
            envelope.RequiredRequestFields);

        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.OriginalTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.RawCssIncluded);
        Assert.False(envelope.RawDesignTreeIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.False(envelope.ExternalCallerRepoTouched);
        Assert.False(envelope.CrossRepoCallerVerified);
        Assert.False(envelope.PublicHttpEndpointVerified);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["status_readback_response_refs_only"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["socket_opened"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["trained_model_authority"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["not_trained_model_authority"]);
    }

    private static void AssertStatusQualityMetricsAvailable(
        JsonElement root,
        double expectedScore,
        double expectedRequiredThreshold,
        int expectedDefectCount,
        bool expectedPassed,
        bool expectedThresholdMet)
    {
        var qualityMetrics = root.GetProperty("qualityMetrics");
        Assert.True(qualityMetrics.GetProperty("available").GetBoolean());
        Assert.Equal(expectedScore, qualityMetrics.GetProperty("score").GetDouble(), precision: 6);
        Assert.Equal(expectedRequiredThreshold, qualityMetrics.GetProperty("requiredThreshold").GetDouble(), precision: 6);
        Assert.Equal(expectedPassed, qualityMetrics.GetProperty("passed").GetBoolean());
        Assert.Equal(expectedThresholdMet, qualityMetrics.GetProperty("thresholdMet").GetBoolean());
        Assert.Equal(expectedDefectCount, qualityMetrics.GetProperty("defectCount").GetInt32());
        Assert.Equal("final_visual_verdict", qualityMetrics.GetProperty("source").GetString());
        Assert.Equal("VisualQaBoundaryTrace.VisualDecision.RequiredThreshold", qualityMetrics.GetProperty("thresholdSource").GetString());
        Assert.Equal(JsonValueKind.Null, qualityMetrics.GetProperty("unavailableReason").ValueKind);
    }

    private static void AssertStatusQualityMetricsUnavailableAfterRestart(JsonElement root)
    {
        var qualityMetrics = root.GetProperty("qualityMetrics");
        Assert.False(qualityMetrics.GetProperty("available").GetBoolean());
        Assert.Equal(JsonValueKind.Null, qualityMetrics.GetProperty("score").ValueKind);
        Assert.Equal(JsonValueKind.Null, qualityMetrics.GetProperty("requiredThreshold").ValueKind);
        Assert.Equal(JsonValueKind.Null, qualityMetrics.GetProperty("passed").ValueKind);
        Assert.Equal(JsonValueKind.Null, qualityMetrics.GetProperty("thresholdMet").ValueKind);
        Assert.Equal(0, qualityMetrics.GetProperty("defectCount").GetInt32());
        Assert.Equal("not_persisted_after_restart", qualityMetrics.GetProperty("source").GetString());
        Assert.Equal("quality_metrics_not_persisted_after_restart", qualityMetrics.GetProperty("unavailableReason").GetString());
    }

    private static void AssertStatusDefectsSafe(
        JsonElement root,
        string privateMarker,
        int? expectedEvidenceRefCountPerDefect = null,
        string expectedSeverity = "blocker")
    {
        var defects = root.GetProperty("defects").EnumerateArray().ToArray();
        Assert.Equal(2, defects.Length);
        var expectedSeverityLabel = string.IsNullOrEmpty(expectedSeverity)
            ? "Other"
            : char.ToUpperInvariant(expectedSeverity[0]) + expectedSeverity[1..];

        var defectRefs = new List<string>(defects.Length);
        for (var index = 0; index < defects.Length; index++)
        {
            var defect = defects[index];
            var defectRef = Assert.IsType<string>(defect.GetProperty("defectRef").GetString());
            Assert.Matches("^sha256:[0-9a-f]{64}$", defectRef);
            defectRefs.Add(defectRef);

            Assert.Equal("other", defect.GetProperty("category").GetString());
            Assert.Equal(expectedSeverity, defect.GetProperty("severity").GetString());
            Assert.Equal($"{expectedSeverityLabel} other issue reported by visual QA.", defect.GetProperty("message").GetString());
            Assert.Equal(index + 1, defect.GetProperty("ordinal").GetInt32());

            var evidenceRefs = defect.GetProperty("evidenceRefs")
                .EnumerateArray()
                .Select(item => Assert.IsType<string>(item.GetString()))
                .ToArray();
            Assert.NotEmpty(evidenceRefs);
            if (expectedEvidenceRefCountPerDefect is not null)
                Assert.Equal(expectedEvidenceRefCountPerDefect.Value, evidenceRefs.Length);
            Assert.All(evidenceRefs, reference => Assert.Matches("^sha256:[0-9a-f]{64}$", reference));
            Assert.DoesNotContain(privateMarker, defect.GetRawText(), StringComparison.Ordinal);
        }

        Assert.Equal(defectRefs.Count, defectRefs.Distinct(StringComparer.Ordinal).Count());
    }

    private static string CreateRestartTestRoot(string suffix)
        => Path.Combine(
            Path.GetTempPath(),
            "design-conversion-status-restart-" + suffix + "-" + Guid.NewGuid().ToString("N"));

    private static void DeleteRestartTestRoot(string root)
    {
        if (!string.IsNullOrWhiteSpace(root) && Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static async Task<AdmissionRestartSeed> SeedAdmissionRestartRecordAsync(
        string root,
        string requestId,
        string admissionToken,
        string admissionTenantId,
        string rawRouteScope,
        string privateMarker)
    {
        var artifactDbRoot = Path.Combine(root, "artifact-db");
        var budgetStoreRoot = Path.Combine(root, "budget-store");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: admissionToken,
                tenantId: admissionTenantId,
                routeScope: rawRouteScope));
        Assert.True(admission.Enabled);
        Assert.True(admission.Ready);

        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(
            CreateVerdict(requestId + "-verdict", score: 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-03T14:15:00Z"),
            artifactDb,
            visualCostBudgetStore: new JsonlDesignConversionVisualCostBudgetStore(budgetStoreRoot));
        var evaluateHandler = new DesignConversionModelServerHttpHandler(api, admission);
        var package = CreatePackage(requestId, privateMarker);
        const string trustedTier = "paid";
        const string trustedMonthlyCap = "125.75";
        var requestMetadata = new Dictionary<string, string>(
            package.Metadata ?? new Dictionary<string, string>(),
            StringComparer.Ordinal)
        {
            ["tenant_id"] = admissionTenantId,
            [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = trustedTier,
            [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] =
                trustedMonthlyCap,
            [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
            [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
        };
        var apiRequest = new DesignConversionModelServerApiRequest(
            RequestId: package.RequestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: package.DesignTree!,
            OriginalTree: package.OriginalTree!,
            Html: package.Html!,
            Css: package.Css!,
            Screenshots: package.Screenshots,
            DesignRenders: package.DesignRenders,
            Budget: package.Budget! with
            {
                UserId = "status-restart-platform-user",
                Tier = trustedTier,
                BudgetUsd = 0.25m,
                Metadata = new Dictionary<string, string>()
            },
            Metadata: requestMetadata);
        var submitResponse = await evaluateHandler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerHttpHandler.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = $"Bearer {admissionToken}",
                [DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = requestId + "-idempotency"
            }));
        Assert.Equal(200, submitResponse.StatusCode);
        Assert.NotEmpty(renderProvider.Requests);
        Assert.NotEmpty(comparisonProvider.Requests);

        using var submitDocument = JsonDocument.Parse(submitResponse.BodyJson);
        var jobId = Assert.IsType<string>(submitDocument.RootElement.GetProperty("jobId").GetString());
        var record = await ReadSingleRecordAsync(artifactDb, jobId);
        return new AdmissionRestartSeed(
            root,
            artifactDbRoot,
            jobId,
            admissionToken,
            admissionTenantId,
            rawRouteScope,
            privateMarker,
            record);
    }

    private static async Task<DesignConversionLocalArtifactDbRecord> ReadSingleRecordAsync(
        IDesignConversionLocalArtifactDb db,
        string jobId)
    {
        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: jobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var record = Assert.Single(listed.Value!);
        Assert.Equal(jobId, record.JobId);
        return record;
    }

    private static async Task<DesignConversionLocalArtifactDbRecord> ReadRecordAsync(
        IDesignConversionLocalArtifactDb db,
        string jobId,
        string recordId)
    {
        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: jobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var record = Assert.Single(listed.Value!, candidate =>
            string.Equals(candidate.RecordId, recordId, StringComparison.Ordinal));
        Assert.Equal(jobId, record.JobId);
        return record;
    }

    private static async Task<RestartStatusInvocation> InvokeStatusReadbackForArtifactAccessRecordAsync(
        DesignConversionLocalArtifactDbRecord record)
    {
        var statusRequest = new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerStatusHandler.EndpointPathPrefix + Uri.EscapeDataString(record.JobId),
            BodyJson: null,
            ContentType: null,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        return await InvokeStatusReadbackForArtifactAccessRecordAsync(record, null, statusRequest);
    }

    private static async Task<RestartStatusInvocation> InvokeTenantBoundStatusReadbackForArtifactAccessRecordAsync(
        DesignConversionLocalArtifactDbRecord record,
        string admissionToken,
        string admissionTenantId)
    {
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: admissionToken,
                tenantId: admissionTenantId,
                routeScope: @"C:\server\artifact-access-policy-status"));
        Assert.True(admission.Enabled);
        Assert.True(admission.Ready);

        var statusRequest = new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerStatusHandler.EndpointPathPrefix + Uri.EscapeDataString(record.JobId),
            BodyJson: null,
            ContentType: null,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = $"Bearer {admissionToken}"
            });
        var acceptedAdmission = admission.AdmitStatus(statusRequest);
        Assert.True(acceptedAdmission.IsAccepted, acceptedAdmission.ErrorMessage);

        var metadata = new Dictionary<string, string>(
            record.Metadata ?? new Dictionary<string, string>(),
            StringComparer.Ordinal)
        {
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                bool.TrueString,
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                acceptedAdmission.AdmissionContext.TenantIdRef,
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                acceptedAdmission.AdmissionContext.ScopeRef
        };

        return await InvokeStatusReadbackForArtifactAccessRecordAsync(
            record with { Metadata = metadata },
            admission,
            statusRequest);
    }

    private static async Task<RestartStatusInvocation> InvokeStatusReadbackForArtifactAccessRecordAsync(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionModelServerRouteAdmission? routeAdmission,
        DesignConversionModelServerHttpRequest statusRequest)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "artifact-access-policy-status-route-should-not-rerun",
            score: 0.20,
            passed: false,
            privateMarker: "PRIVATE_ARTIFACT_ACCESS_POLICY_SHOULD_NOT_RERUN",
            includeDefect: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-03T15:00:00Z"),
            new ExactRecordLocalArtifactDb(record));
        var handler = new DesignConversionModelServerStatusHandler(
            api,
            routeAdmission,
            utcNow: () => DateTimeOffset.Parse("2026-07-03T15:00:00Z"));

        var response = await handler.HandleAsync(statusRequest);

        return new RestartStatusInvocation(response, renderProvider, comparisonProvider);
    }

    private static DesignConversionLocalArtifactDbRecord CreateArtifactAccessPolicyStatusRecord(
        string jobId,
        string requestId,
        IReadOnlyList<DesignConversionRenderArtifactDbRef> renderArtifactRefs)
        => CreateStatusRestoreRecord(
                jobId,
                requestId,
                DesignConversionVisualQaBoundaryPolicy.DecisionAccepted)
            with
            {
                ConversionResult = new DesignConversionConversionResultArtifact(
                    ResultId: jobId + ":conversion-result",
                    HtmlRef: string.Empty,
                    HtmlSha256: string.Empty,
                    CssRef: string.Empty,
                    CssSha256: string.Empty,
                    Metadata: new Dictionary<string, string>()),
                RenderArtifactRefs = renderArtifactRefs
            };

    private static DesignConversionRenderArtifactDbRef CreateArtifactAccessPolicyRenderRef(
        string artifactId,
        string kind,
        string uriRef,
        string sha256,
        IReadOnlyDictionary<string, string> metadata)
        => new(
            ViewportId: artifactId + "-viewport",
            ArtifactId: artifactId,
            Kind: kind,
            UriRef: uriRef,
            Sha256: sha256,
            ByteLength: 128,
            Width: 1440,
            Height: 900,
            Metadata: metadata);

    private static void AssertArtifactMetadataValueHashed(JsonElement metadata, string key, string rawValue)
    {
        Assert.True(metadata.TryGetProperty(key, out var element));
        var redactedValue = Assert.IsType<string>(element.GetString());
        Assert.Equal(BuildStatusArtifactMetadataHash(key, rawValue), redactedValue);
        Assert.Matches("^sha256:[0-9a-f]{64}$", redactedValue);
        Assert.NotEqual(rawValue, redactedValue);
    }

    private static string BuildStatusArtifactMetadataHash(string key, string value)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                $"status-artifact-metadata:{key}:{value}")))
            .ToLowerInvariant();
        return $"sha256:{hash}";
    }

    private static async Task<DesignConversionLocalArtifactDbRecord> AppendStatusSnapshotVariantAsync(
        JsonlDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord originalRecord,
        IReadOnlyDictionary<string, string> visualJudgeMetadata,
        string recordIdSuffix)
    {
        var candidate = originalRecord with
        {
            RecordId = originalRecord.RecordId + ":" + recordIdSuffix,
            CreatedAtUtc = originalRecord.CreatedAtUtc.AddSeconds(1),
            VisualJudgeStatus = originalRecord.VisualJudgeStatus with
            {
                Metadata = visualJudgeMetadata
            }
        };
        var appended = await db.AppendRecordAsync(candidate);
        Assert.True(appended.IsSuccess, appended.ErrorMessage);
        return appended.Value!;
    }

    private static string BuildAmbiguousCompletedStatusSnapshot(
        DesignConversionLocalArtifactDbStatusSnapshot snapshot)
    {
        var metrics = snapshot.QualityMetrics;
        if (metrics.Score is not { } score ||
            metrics.RequiredThreshold is not { } requiredThreshold ||
            metrics.Passed is not { } passed ||
            metrics.ThresholdMet is not { } thresholdMet)
        {
            throw new InvalidOperationException("A complete status snapshot is required by this fixture.");
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", snapshot.Schema);
            writer.WriteNumber("version", snapshot.Version);
            writer.WritePropertyName("qualityMetrics");
            writer.WriteStartObject();
            writer.WriteBoolean("available", metrics.Available);
            writer.WriteNumber("score", score);
            writer.WriteNumber("requiredThreshold", requiredThreshold);
            writer.WriteBoolean("passed", passed);
            writer.WriteBoolean("thresholdMet", thresholdMet);
            writer.WriteNumber("defectCount", metrics.DefectCount);
            writer.WriteString("source", metrics.Source);
            writer.WriteString("source", metrics.Source);
            writer.WriteString("thresholdSource", metrics.ThresholdSource);
            writer.WriteEndObject();
            writer.WritePropertyName("defects");
            writer.WriteStartArray();
            foreach (var defect in snapshot.Defects)
            {
                writer.WriteStartObject();
                writer.WriteString("defectRef", defect.DefectRef);
                writer.WriteString("category", defect.Category);
                writer.WriteString("severity", defect.Severity);
                writer.WriteString("message", defect.Message);
                writer.WriteNumber("ordinal", defect.Ordinal);
                writer.WritePropertyName("evidenceRefs");
                writer.WriteStartArray();
                foreach (var evidenceRef in defect.EvidenceRefs)
                    writer.WriteStringValue(evidenceRef);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string BuildRouteAdmissionScopeRef(
        string rawRouteScope,
        string tenantIdRef)
    {
        var scopedBytes = Encoding.UTF8.GetBytes(rawRouteScope + "\n" + tenantIdRef);
        return "sha256:design-conversion-route-admission-scope:" +
            Convert.ToHexString(SHA256.HashData(scopedBytes)).ToLowerInvariant();
    }

    private static void AssertSafePersistedRouteAdmissionBinding(
        IReadOnlyDictionary<string, string> metadata,
        string expectedTenantIdRef,
        string expectedScopeRef)
    {
        Assert.Equal(
            bool.TrueString,
            metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey]);
        Assert.Equal(
            expectedTenantIdRef,
            metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey]);
        Assert.Equal(
            expectedScopeRef,
            metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey]);
        Assert.True(DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionTenantIdRef(expectedTenantIdRef));
        Assert.True(DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionScopeRef(expectedScopeRef));
    }

    private static void AssertMetadataDoesNotLeak(
        IReadOnlyDictionary<string, string> metadata,
        string rawTenantId,
        string rawRouteScope,
        string privateMarker)
    {
        var serialized = JsonSerializer.Serialize(metadata, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(rawTenantId, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(rawRouteScope, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, serialized, StringComparison.Ordinal);
    }

    private static async Task<RestartStatusInvocation> InvokeRestartStatusAsync(
        string artifactDbRoot,
        string jobId,
        string admissionToken,
        string admissionTenantId,
        string rawRouteScope)
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: admissionToken,
                tenantId: admissionTenantId,
                routeScope: rawRouteScope));
        Assert.True(admission.Enabled);
        Assert.True(admission.Ready);

        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict(
            "pass131-status-restart-should-not-rerun",
            score: 0.20,
            passed: false,
            privateMarker: "PASS131_STATUS_RESTART_SHOULD_NOT_RERUN",
            includeDefect: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-03T14:20:00Z"),
            artifactDb);
        var handler = new DesignConversionModelServerStatusHandler(api, admission);
        var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerStatusHandler.EndpointPathPrefix + Uri.EscapeDataString(jobId),
            BodyJson: null,
            ContentType: null,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = $"Bearer {admissionToken}"
            }));
        return new RestartStatusInvocation(response, renderProvider, comparisonProvider);
    }

    private static void AssertGenericJobNotFoundWithoutRerunOrLeak(
        RestartStatusInvocation invocation,
        AdmissionRestartSeed seed,
        string attemptedTenantId,
        string attemptedRawRouteScope)
    {
        Assert.Equal(404, invocation.Response.StatusCode);
        Assert.Empty(invocation.RenderProvider.Requests);
        Assert.Empty(invocation.ComparisonProvider.Requests);

        using var document = JsonDocument.Parse(invocation.Response.BodyJson);
        Assert.Equal("JOB_NOT_FOUND", document.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(
            "DesignConversion status/readback rejected the job id.",
            document.RootElement.GetProperty("errorMessage").GetString());

        Assert.DoesNotContain(seed.AdmissionTenantId, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(attemptedTenantId, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(seed.RawRouteScope, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(attemptedRawRouteScope, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(seed.PrivateMarker, invocation.Response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant mismatch", invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("scope mismatch", invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("binding mismatch", invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("binding corrupt", invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("corrupt binding", invocation.Response.BodyJson, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task AppendTamperedRecordAsync(
        DesignConversionLocalArtifactDbRecord record,
        string targetRoot,
        IReadOnlyDictionary<string, string> metadata)
    {
        var db = new JsonlDesignConversionLocalArtifactDb(targetRoot);
        var appended = await db.AppendRecordAsync(record with { Metadata = metadata });
        Assert.True(appended.IsSuccess, appended.ErrorMessage);
    }

    private static IReadOnlyDictionary<string, string> TamperRouteAdmissionBinding(
        IReadOnlyDictionary<string, string> metadata,
        string caseName)
    {
        var copy = new Dictionary<string, string>(metadata, StringComparer.Ordinal);
        switch (caseName)
        {
            case "missing_tenant_ref":
                copy.Remove(DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey);
                break;
            case "partial_missing_scope_ref":
                copy.Remove(DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey);
                break;
            case "malformed_tenant_ref":
                copy[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                    "sha256:design-conversion-model-activation-tenant:1234";
                break;
            case "malformed_scope_ref":
                copy[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                    "sha256:design-conversion-route-admission-scope:1234";
                break;
            case "false_tenant_bound":
                copy[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                    bool.FalseString;
                break;
            case "noncanonical_truthy_tenant_bound":
                copy[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                    "yes";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "Unsupported route admission tamper case.");
        }

        return copy;
    }

    private static IReadOnlyList<string> RequiredRequestFieldNames(
        IReadOnlyList<DesignConversionModelServerCallerContractField> requestFields)
        => requestFields
            .Where(field => field.Required)
            .Select(field => field.JsonName)
            .ToArray();

    private static void AssertStableStatusErrorEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        int statusCode,
        bool registryInvokeSucceeded,
        string expectedErrorCode)
    {
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion, envelope.EnvelopeSchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary, envelope.EnvelopeBoundary);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointMethod, envelope.EndpointMethod);
        Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, envelope.EndpointPath);
        Assert.Equal(statusCode, envelope.StatusCode);
        Assert.Equal(registryInvokeSucceeded, envelope.RegistryInvokeSucceeded);
        Assert.False(envelope.RuntimeInvoked);
        Assert.Equal(expectedErrorCode, envelope.ParsedErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(envelope.ParsedErrorMessage));
        Assert.False(envelope.ExternalCallerRepoTouched);
        Assert.False(envelope.CrossRepoCallerVerified);
        Assert.False(envelope.PublicHttpEndpointVerified);
        Assert.Equal(registryInvokeSucceeded.ToString(), envelope.BoundaryMetadata["registry_invoke_succeeded"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["runtime_invoked"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_caller_repo_touched"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
    }

    private static (
        DesignConversionModelServerLocalCallerAdapter Adapter,
        RecordingRenderProvider RenderProvider,
        RecordingComparisonProvider ComparisonProvider) CreateRegisteredAdapter(
        VisualVerdict? verdict = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        IReadOnlyDictionary<string, string>? renderArtifactMetadata = null,
        Func<IDesignConversionThinClientJobEvaluator, IDesignConversionThinClientJobEvaluator>? evaluatorDecorator = null,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null)
    {
        var renderProvider = new RecordingRenderProvider(renderArtifactMetadata);
        var comparisonProvider = new RecordingComparisonProvider(
            verdict ?? CreateVerdict("pass131-status-verdict", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        IDesignConversionThinClientJobEvaluator evaluator =
            new DesignConversionInternalServiceThinClientJobEvaluator(service);
        if (evaluatorDecorator is not null)
            evaluator = evaluatorDecorator(evaluator);

        var api = artifactDb is null && evaluatorDecorator is null
            ? new DesignConversionModelServerApi(
                service,
                () => DateTimeOffset.Parse("2026-07-03T14:00:00Z"),
                postEvaluationStateStore: postEvaluationStateStore)
            : new DesignConversionModelServerApi(
                evaluator,
                () => DateTimeOffset.Parse("2026-07-03T14:00:00Z"),
                artifactDb,
                postEvaluationStateStore: postEvaluationStateStore);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var evaluateRegistered = registry.RegisterDesignConversionEvaluate(new DesignConversionModelServerHttpHandler(api));
        var statusRegistered = registry.RegisterDesignConversionStatusReadback(new DesignConversionModelServerStatusHandler(api));
        Assert.True(evaluateRegistered.IsSuccess, evaluateRegistered.ErrorMessage);
        Assert.True(statusRegistered.IsSuccess, statusRegistered.ErrorMessage);
        return (new DesignConversionModelServerLocalCallerAdapter(registry), renderProvider, comparisonProvider);
    }

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:design-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:original-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            Html: $"<main data-secret=\"{privateMarker}\">Pass131</main>",
            Css: $".pass131{{background-image:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 1440, 900)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:design-desktop", "design-render", 1440, 900)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "pass131-local-status",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.50m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

    private static DesignConversionLocalArtifactDbRecord CreateStatusRestoreRecord(
        string jobId,
        string requestId,
        string visualDecision)
        => new(
            RecordId: jobId + ":status-restore-record",
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: jobId,
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            Tier: "status-readback-local",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["design_tree_payload_ref"] = "sha256:" + new string('a', 64),
                ["original_tree_payload_ref"] = "sha256:" + new string('b', 64)
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: jobId + ":conversion-result",
                HtmlRef: "artifact://pass131/status-restore/html",
                HtmlSha256: "sha256:" + new string('c', 64),
                CssRef: "artifact://pass131/status-restore/css",
                CssSha256: "sha256:" + new string('d', 64),
                Metadata: new Dictionary<string, string>
                {
                    ["conversion_result"] = "html_css_result_refs_or_hashes"
                }),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: jobId + ":desktop-render",
                    Kind: "actual-render",
                    UriRef: "artifact://pass131/status-restore/desktop.png",
                    Sha256: "sha256:" + new string('e', 64),
                    ByteLength: 128,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>
                    {
                        ["render_artifact_refs"] = "refs_or_hashes_only"
                    })
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: "visual_judge_completed",
                ProviderBoundary: DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: 0m,
                TierDecision: DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
                EvidenceRefs: ["visual_decision:" + visualDecision],
                Metadata: new Dictionary<string, string>
                {
                    ["visual_judge_status"] = "visual_judge_completed",
                    ["visual_decision"] = visualDecision,
                    ["product_not_ready"] = bool.TrueString
                }),
            DpoCandidates: [],
            CreatedAtUtc: DateTimeOffset.Parse("2026-07-03T14:12:00Z"),
            EvidenceRefs:
            [
                JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker,
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                JsonlDesignConversionLocalArtifactDb.ConversionResultMarker,
                JsonlDesignConversionLocalArtifactDb.RenderArtifactRefsMarker,
                JsonlDesignConversionLocalArtifactDb.VisualJudgeStatusMarker
            ],
            Metadata: new Dictionary<string, string>
            {
                ["status_restore_seed_record"] = bool.TrueString
            });

    private static DesignConversionModelServerArtifactRef CreateArtifact(
        string id,
        string kind,
        int width,
        int height)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: width,
            Height: height,
            Uri: $"artifact://pass131/{id}.png",
            Sha256: $"sha256:{id}");

    private static VisualVerdict CreateVerdict(
        string verdictId,
        double score,
        bool passed,
        string? privateMarker = null,
        bool includeDefect = false,
        IReadOnlyList<string>? evidenceImageIds = null)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects: includeDefect || !passed
                ?
                [
                    new VisualDefect(
                        DefectId: $"{verdictId}:spacing:{privateMarker ?? "safe"}",
                        Category: "spacing",
                        Severity: "major",
                        Description: $"Spacing differs from expected render. {privateMarker ?? string.Empty}",
                        X: 16,
                        Y: 24,
                        Width: 80,
                        Height: 20,
                        EvidenceImageIds: evidenceImageIds ??
                        [
                            "expected",
                            $"actual-{privateMarker ?? "safe"}"
                        ])
                ]
                : [],
            EvidenceRefs: [$"verdict:{verdictId}"],
            Passed: passed);

    private static DesignConversionInternalEvaluationResult CreateServerOwnedDirectDpoAuthorityEvaluation(
        DesignConversionInternalEvaluateRequest request)
    {
        var evaluation = DesignConversionThinClientJobRuntimeTests.CreateMeasuredRuntimeDpoEvaluation(
            canPersistTrainingCandidate: false,
            jobId: request.JobId,
            requestId: request.RequestId,
            outputId: $"{request.JobId}-status-direct-dpo-authority-output",
            candidateId: $"{request.JobId}-status-direct-dpo-authority-candidate",
            verdictId: $"{request.JobId}-status-direct-dpo-authority-verdict",
            score: 0.97,
            passed: true,
            htmlSuffix: "status-direct-dpo-authority");
        var authorityEvidenceRefs = new[]
        {
            DesignConversionMachine.LearnedInferenceAppliedEvidence,
            DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityScoreEvidence,
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityConfiguredEvidence,
            DesignConversionMachine.DpoRuntimeCheckpointAccuracyFromHeldOutEvaluationEvidence,
            DesignConversionMachine.DpoRuntimeCheckpointAccuracyCurrentCycleHeldOutEvaluationScopeEvidence
        };
        var candidateMetadata = new Dictionary<string, string>(
            evaluation.RuntimeFlowResult.Candidate.Metadata,
            StringComparer.Ordinal)
        {
            ["direct_dpo_policy_inference_applied"] = bool.TrueString,
            ["direct_dpo_policy_score_authority"] = bool.TrueString,
            ["direct_dpo_policy_acceptance_authority"] = bool.TrueString,
            ["baseline_visual_verdict_acceptance_authority"] = bool.FalseString
        };
        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(
            candidateMetadata,
            DesignConversionModelAuthorityBoundary.TrustedDirectDpoRuntimeFlowAuthority);
        var candidate = evaluation.RuntimeFlowResult.Candidate with
        {
            EvidenceRefs = evaluation.RuntimeFlowResult.Candidate.EvidenceRefs
                .Concat(authorityEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Metadata = candidateMetadata
        };
        var output = evaluation.RuntimeFlowResult.Output with
        {
            EvidenceRefs = evaluation.RuntimeFlowResult.Output.EvidenceRefs
                .Concat(authorityEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Verdict = evaluation.RuntimeFlowResult.Output.Verdict with
            {
                EvidenceRefs = evaluation.RuntimeFlowResult.Output.Verdict.EvidenceRefs
                    .Concat(authorityEvidenceRefs)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray()
            }
        };
        var runtimeFlow = evaluation.RuntimeFlowResult with
        {
            Candidate = candidate,
            Output = output,
            ImproverOutput = evaluation.RuntimeFlowResult.ImproverOutput with
            {
                Candidate = candidate,
                Output = output
            },
            EvidenceRefs = evaluation.RuntimeFlowResult.EvidenceRefs
                .Concat(authorityEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };

        return evaluation with
        {
            RuntimeFlowResult = runtimeFlow,
            Verdict = output.Verdict,
            EvidenceRefs = evaluation.EvidenceRefs
                .Concat(authorityEvidenceRefs)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Warnings = evaluation.Warnings
                .Append("DESIGN_CONVERSION_RUNTIME_DIRECT_DPO_POLICY_SCORE_ACCEPTANCE_AUTHORITY_ACTIVE")
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
    }

    private sealed class RecordingRenderProvider(
        IReadOnlyDictionary<string, string>? artifactMetadata = null) : IDesignConversionRenderProvider
    {
        private const string CanonicalRenderSha256 =
            "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in request.Metadata)
                metadata[pair.Key] = pair.Value;

            if (artifactMetadata is not null)
            {
                foreach (var pair in artifactMetadata)
                    metadata[pair.Key] = pair.Value;
            }

            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass131/render/{request.RenderRequestId}.png",
                Sha256: CanonicalRenderSha256,
                Metadata: metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["pass131-recording-render-provider"])));
        }
    }

    private sealed class RecordingComparisonProvider(VisualVerdict verdict) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://pass131/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["pass131-comparison-provider"])));
        }
    }

    private sealed class FixedThinClientJobEvaluator(
        Func<DesignConversionInternalEvaluateRequest, DesignConversionInternalEvaluationResult> evaluationFactory) :
        IDesignConversionThinClientJobEvaluator
    {
        private readonly Func<DesignConversionInternalEvaluateRequest, DesignConversionInternalEvaluationResult>
            _evaluationFactory = evaluationFactory ?? throw new ArgumentNullException(nameof(evaluationFactory));

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                    "Fixed evaluator cancelled.",
                    "CANCELLED"));

            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                _evaluationFactory(request)));
        }
    }

    private sealed class ThreeEvidenceCorrectionFindingEvaluator(
        IDesignConversionThinClientJobEvaluator inner) : IDesignConversionThinClientJobEvaluator
    {
        private readonly IDesignConversionThinClientJobEvaluator _inner = inner;

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            var evaluated = await _inner.EvaluateAsync(request, cancellationToken);
            if (!evaluated.IsSuccess)
                return evaluated;

            var evaluation = evaluated.Value!;
            var trace = evaluation.VisualQaBoundaryTrace;
            var decision = trace.VisualDecision;
            var findings = (decision.CorrectionFindings ?? [])
                .Select(finding => finding with
                {
                    EvidenceImageIds =
                    [
                        finding.FindingId + ":expected",
                        finding.FindingId + ":actual",
                        finding.FindingId + ":detail"
                    ]
                })
                .ToArray();
            return OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                evaluation with
                {
                    VisualQaBoundaryTrace = trace with
                    {
                        VisualDecision = decision with
                        {
                            CorrectionFindings = findings
                        }
                    }
                });
        }
    }

    private sealed class AggregateEvidenceCorrectionFindingEvaluator(
        IDesignConversionThinClientJobEvaluator inner,
        int aggregateEvidenceRefCount) : IDesignConversionThinClientJobEvaluator
    {
        private readonly IDesignConversionThinClientJobEvaluator _inner = inner;
        private readonly int _aggregateEvidenceRefCount = aggregateEvidenceRefCount;

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            var evaluated = await _inner.EvaluateAsync(request, cancellationToken);
            if (!evaluated.IsSuccess)
                return evaluated;

            var evaluation = evaluated.Value!;
            var trace = evaluation.VisualQaBoundaryTrace;
            var decision = trace.VisualDecision;
            var sourceFindings = (decision.CorrectionFindings ?? []).ToArray();
            var findings = new DesignConversionVisualCorrectionFinding[sourceFindings.Length];
            for (var index = 0; index < sourceFindings.Length; index++)
            {
                var evidenceRefCount = index == 0
                    ? _aggregateEvidenceRefCount - Math.Max(0, sourceFindings.Length - 1)
                    : 1;
                var evidenceImageIds = Enumerable
                    .Range(0, Math.Max(1, evidenceRefCount))
                    .Select(evidenceIndex =>
                        $"aggregate-evidence-{index + 1}-{evidenceIndex + 1}")
                    .ToArray();
                findings[index] = sourceFindings[index] with
                {
                    EvidenceImageIds = evidenceImageIds
                };
            }

            return OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                evaluation with
                {
                    VisualQaBoundaryTrace = trace with
                    {
                        VisualDecision = decision with
                        {
                            CorrectionFindings = findings
                        }
                    }
                });
        }
    }

    private sealed class ExactRecordLocalArtifactDb(DesignConversionLocalArtifactDbRecord record)
        : IDesignConversionLocalArtifactDb
    {
        private readonly DesignConversionLocalArtifactDbRecord _record = record;

        public Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
            => Task.FromResult(OperationResult<DesignConversionLocalArtifactDbRecord>.Ok(record));

        public Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
        {
            IReadOnlyList<DesignConversionLocalArtifactDbRecord> records =
                QueryMatches(query) ? [_record] : [];
            return Task.FromResult(OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Ok(records));
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => UnexpectedAsync<IReadOnlyList<DesignConversionDpoTrainingRecord>>();

        public Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            DateTimeOffset? exportedAtUtc = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionApprovedDpoHandoffArtifact>();

        public Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
            DesignConversionApprovedDpoHandoffArtifact handoff,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => UnexpectedAsync<DesignConversionTrainingIntakeRecord>();

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
            CancellationToken ct = default)
            => UnexpectedAsync<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();

        public Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
            string intakeId,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionTrainingIntakePreflightResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => UnexpectedAsync<DesignConversionTrainingIntakeRunResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            string intakeId,
            string promotionOutputDirectory,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionTrainingIntakeLocalPromotionResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
            string promotedLocalModelRef,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();

        public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
            string localArtifactRecordId,
            string candidateOrTrainingRecordId,
            string reviewerRef,
            string reason,
            DateTimeOffset? approvedAtUtc = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionDpoCandidateApprovalResult>();

        public Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
            DesignConversionDpoDisplayQuery? query = null,
            CancellationToken ct = default)
            => UnexpectedAsync<DesignConversionDpoDisplayResult>();

        private bool QueryMatches(DesignConversionLocalArtifactDbQuery? query)
            => query is null ||
               (string.IsNullOrWhiteSpace(query.JobId) ||
                string.Equals(query.JobId, _record.JobId, StringComparison.Ordinal)) &&
               (string.IsNullOrWhiteSpace(query.RequestId) ||
                string.Equals(query.RequestId, _record.RequestId, StringComparison.Ordinal)) &&
               (string.IsNullOrWhiteSpace(query.SourceKind) ||
                string.Equals(query.SourceKind, _record.SourceKind, StringComparison.Ordinal));

        private static Task<OperationResult<T>> UnexpectedAsync<T>()
            => Task.FromResult(OperationResult<T>.Fail(
                "ExactRecordLocalArtifactDb supports status readback records only.",
                "EXACT_RECORD_ARTIFACT_DB_UNEXPECTED_CALL"));
    }

    private sealed record AdmissionRestartSeed(
        string Root,
        string ArtifactDbRoot,
        string JobId,
        string AdmissionToken,
        string AdmissionTenantId,
        string RawRouteScope,
        string PrivateMarker,
        DesignConversionLocalArtifactDbRecord Record);

    private sealed record RestartStatusInvocation(
        DesignConversionModelServerHttpResponse Response,
        RecordingRenderProvider RenderProvider,
        RecordingComparisonProvider ComparisonProvider);
}
