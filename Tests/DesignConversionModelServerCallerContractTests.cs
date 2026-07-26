// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerCallerContractTests
{
    [Fact]
    public void Describe_DocumentsEndpointRequestResponseValidationExamplesAndBoundaries()
    {
        var descriptor = DesignConversionModelServerCallerContract.Describe();
        var expectedRequiredRequestFields = string.Join(
            ",",
            descriptor.RequestFields
                .Where(field => field.Required)
                .Select(field => field.JsonName));
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registration = registry.RegisterDesignConversionEvaluate(new DesignConversionModelServerHttpHandler(CreateApi()));

        Assert.Equal(DesignConversionModelServerCallerContract.SchemaVersion, descriptor.SchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.Boundary, descriptor.Boundary);
        Assert.Equal("POST", descriptor.EndpointMethod);
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, descriptor.EndpointPath);
        Assert.Contains(DesignConversionSourceKind.Figma.ToString(), descriptor.SupportedSourceKinds);
        Assert.Contains(DesignConversionSourceKind.Canva.ToString(), descriptor.SupportedSourceKinds);
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "designTree", Required: true, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "originalTree", Required: true, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "html", Required: true, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "css", Required: true, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "budget", Required: false, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(descriptor.ResponseFields, field =>
            field is { JsonName: "boundaryMetadata", Required: true });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.Examples, example =>
            example is { Name: "figma-local-caller-fixture", SourceKind: DesignConversionSourceKind.Figma });
        Assert.Contains(descriptor.Examples, example =>
            example is { Name: "canva-local-caller-fixture", SourceKind: DesignConversionSourceKind.Canva });
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["response_contains_raw_private_data"]);

        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        Assert.Equal(
            DesignConversionModelServerCallerContract.SchemaVersion,
            registration.Value!.BoundaryMetadata["caller_contract_schema_version"]);
        Assert.Equal(
            expectedRequiredRequestFields,
            registration.Value.BoundaryMetadata["caller_contract_required_fields"]);
        Assert.Equal(bool.FalseString, registration.Value.BoundaryMetadata["external_caller_repo_touched"]);
    }

    [Theory]
    [InlineData(nameof(DesignConversionSourceKind.Figma), "PRIVATE_PASS124_FIGMA_CONTRACT_SECRET")]
    [InlineData(nameof(DesignConversionSourceKind.Canva), "PRIVATE_PASS124_CANVA_CONTRACT_SECRET")]
    public async Task CallerContract_FigmaAndCanvaFixtures_InvokeRegistryAndReturnSanitizedResponseShape(
        string sourceKind,
        string privateMarker)
    {
        var (registry, renderProvider, comparisonProvider) = CreateRegisteredRegistry();
        var request = WithPrivateMarker(CreateContractFixture(sourceKind), privateMarker);
        var bodyJson = JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.Contains(privateMarker, bodyJson, StringComparison.Ordinal);

        using (var requestDocument = JsonDocument.Parse(bodyJson))
        {
            var requestRoot = requestDocument.RootElement;
            Assert.Equal(request.RequestId, requestRoot.GetProperty("requestId").GetString());
            Assert.Equal(sourceKind, requestRoot.GetProperty("sourceKind").GetString());
            Assert.Equal(JsonValueKind.Object, requestRoot.GetProperty("designTree").ValueKind);
            Assert.Equal(JsonValueKind.Object, requestRoot.GetProperty("originalTree").ValueKind);
            Assert.Equal(JsonValueKind.String, requestRoot.GetProperty("html").ValueKind);
            Assert.Equal(JsonValueKind.String, requestRoot.GetProperty("css").ValueKind);
            Assert.Equal(JsonValueKind.Array, requestRoot.GetProperty("screenshots").ValueKind);
            Assert.Equal(JsonValueKind.Array, requestRoot.GetProperty("designRenders").ValueKind);
        }

        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.EndpointPath,
            BodyJson: bodyJson,
            ContentType: "application/json"));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        var response = invoked.Value!;
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.NotEmpty(renderProvider.Requests);
        Assert.NotEmpty(comparisonProvider.Requests);
        Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.DesignTree.Payload!, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.OriginalTree.Payload!, response.BodyJson, StringComparison.Ordinal);

        using var responseDocument = JsonDocument.Parse(response.BodyJson);
        var root = responseDocument.RootElement;
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
        Assert.Equal(DesignConversionModelServerApi.ApiShape, root.GetProperty("apiShape").GetString());
        Assert.Equal(DesignConversionModelServerApi.RuntimeSurface, root.GetProperty("runtimeSurface").GetString());
        Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
        Assert.Equal(request.RequestId, root.GetProperty("requestId").GetString());
        Assert.Equal(JsonValueKind.Object, root.GetProperty("runtimeStatus").ValueKind);
        Assert.Equal(JsonValueKind.Object, root.GetProperty("trainingReadiness").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("evidenceRefs").ValueKind);
        Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());
        Assert.False(root.GetProperty("platformIntegrationVerified").GetBoolean());
        Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
        Assert.False(root.GetProperty("productReady").GetBoolean());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(
            DesignConversionModelServerCallerContract.SchemaVersion,
            metadata.GetProperty("caller_contract_schema_version").GetString());
        Assert.Equal(
            DesignConversionModelServerCallerContract.Boundary,
            metadata.GetProperty("caller_contract_boundary").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_http_host_seam_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_caller_repo_touched").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
    }

    [Theory]
    [InlineData("originalTree", "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED")]
    [InlineData("artifactRefs", "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED")]
    [InlineData("unsupportedArtifactRef", "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED")]
    public async Task CallerContract_MissingRequiredInputs_ReturnsDocumentedValidationFailure(
        string missing,
        string expectedErrorCode)
    {
        var descriptor = DesignConversionModelServerCallerContract.Describe();
        var (registry, renderProvider, comparisonProvider) = CreateRegisteredRegistry();
        var request = DesignConversionModelServerCallerContract.CreateFigmaExampleRequest("pass124-invalid-contract-request");
        request = missing switch
        {
            "originalTree" => request with { OriginalTree = null! },
            "artifactRefs" => request with { Screenshots = [], DesignRenders = [] },
            "unsupportedArtifactRef" => request with
            {
                Screenshots =
                [
                    request.Screenshots![0] with
                    {
                        ArtifactId = "unsupported-screenshot-proof",
                        Uri = "https://client.example.invalid/screenshot-proof.png",
                        Sha256 = "sha256:unsupported-screenshot-proof"
                    }
                ],
                DesignRenders = []
            },
            _ => request
        };

        var invoked = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.EndpointPath,
            BodyJson: JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json; charset=utf-8"));

        Assert.True(invoked.IsSuccess, invoked.ErrorMessage);
        Assert.Equal(400, invoked.Value!.StatusCode);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure.ErrorCode == expectedErrorCode && failure.StatusCode == 400);

        using var document = JsonDocument.Parse(invoked.Value.BodyJson);
        var root = document.RootElement;
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, root.GetProperty("endpoint").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(
            DesignConversionModelServerCallerContract.SchemaVersion,
            metadata.GetProperty("caller_contract_schema_version").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_caller_repo_touched").GetString());
    }

    private static DesignConversionModelServerApiRequest CreateContractFixture(string sourceKind)
        => sourceKind switch
        {
            nameof(DesignConversionSourceKind.Figma) =>
                DesignConversionModelServerCallerContract.CreateFigmaExampleRequest(),
            nameof(DesignConversionSourceKind.Canva) =>
                DesignConversionModelServerCallerContract.CreateCanvaExampleRequest(),
            _ => throw new ArgumentOutOfRangeException(nameof(sourceKind), sourceKind, "Unsupported source kind.")
        };

    private static DesignConversionModelServerApiRequest WithPrivateMarker(
        DesignConversionModelServerApiRequest request,
        string privateMarker)
        => request with
        {
            DesignTree = request.DesignTree with
            {
                Payload = $"{{\"secret\":\"{privateMarker}\"}}"
            },
            OriginalTree = request.OriginalTree with
            {
                Payload = $"{{\"originalSecret\":\"{privateMarker}\"}}"
            },
            Html = $"<main data-secret=\"{privateMarker}\">Contract fixture</main>",
            Css = $".screen{{background-image:url('{privateMarker}');}}",
            Budget = request.Budget! with
            {
                Metadata = new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["private_note"] = privateMarker,
                ["caller_contract_schema_version"] = DesignConversionModelServerCallerContract.SchemaVersion
            }
        };

    private static (
        DesignConversionModelServerEndpointRegistry Registry,
        RecordingRenderProvider RenderProvider,
        RecordingComparisonProvider ComparisonProvider) CreateRegisteredRegistry()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass124-caller-contract-verdict", 0.94, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-03T12:00:00Z"));
        var handler = new DesignConversionModelServerHttpHandler(api);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return (registry, renderProvider, comparisonProvider);
    }

    private static DesignConversionModelServerApi CreateApi()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("pass124-descriptor-verdict", 0.90, passed: true)),
            routingFlow: null);
        return new DesignConversionModelServerApi(service, () => DateTimeOffset.Parse("2026-07-03T12:00:00Z"));
    }

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
                Uri: $"artifact://pass124/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["pass124-recording-render-provider"])));
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
                DiffImageUri: $"artifact://pass124/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["pass124-comparison-provider"])));
        }
    }
}
