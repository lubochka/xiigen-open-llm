// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerStatusReadbackTests
{
    [Fact]
    public async Task TryGetJobStatusAsync_SubmittedJob_ReturnsRefsOnlyStatusEnvelopeWithoutRerun()
    {
        const string privateMarker = "PRIVATE_PASS131_STATUS_SECRET";
        var (adapter, renderProvider, comparisonProvider) = CreateRegisteredAdapter();
        var package = CreatePackage("pass131-status-readback-request", privateMarker);
        var submitEnvelope = await adapter.TrySubmitAsync(package);
        var renderCountAfterSubmit = renderProvider.Requests.Count;
        var comparisonCountAfterSubmit = comparisonProvider.Requests.Count;

        var statusEnvelope = await adapter.TryGetJobStatusAsync(
            submitEnvelope.ParsedJobId,
            package.SourceKind);

        AssertStableStatusSuccessEnvelope(statusEnvelope, submitEnvelope, package, privateMarker);
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
        Assert.False(root.GetProperty("learnedInferenceApplied").GetBoolean());
        Assert.False(root.GetProperty("trainedModelAuthority").GetBoolean());
        Assert.True(root.GetProperty("notTrainedModelAuthority").GetBoolean());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("artifacts").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("evidenceRefs").ValueKind);
        Assert.False(root.TryGetProperty("runtimeStatus", out _));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("trained_model_authority").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("not_trained_model_authority").GetString());
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
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.status_readback_response_refs_only", Required: true });
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
        string privateMarker)
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
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
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
        RecordingComparisonProvider ComparisonProvider) CreateRegisteredAdapter()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass131-status-verdict", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-03T14:00:00Z"));
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

    private static VisualVerdict CreateVerdict(string verdictId, double score, bool passed)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects: passed
                ? []
                :
                [
                    new VisualDefect(
                        DefectId: $"{verdictId}:spacing",
                        Category: "spacing",
                        Severity: "major",
                        Description: "Spacing differs from expected render.",
                        X: 16,
                        Y: 24,
                        Width: 80,
                        Height: 20,
                        EvidenceImageIds: ["expected", "actual"])
                ],
            EvidenceRefs: [$"verdict:{verdictId}"],
            Passed: passed);

    private sealed class RecordingRenderProvider : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass131/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

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
}
