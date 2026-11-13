// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerLocalCallerAdapterTests : IDisposable
{
    private readonly List<string> _artifactDbRoots = [];

    [Theory]
    [InlineData(nameof(DesignConversionSourceKind.Figma), "PRIVATE_PASS126_FIGMA_LOCAL_ADAPTER_SECRET")]
    [InlineData(nameof(DesignConversionSourceKind.Canva), "PRIVATE_PASS126_CANVA_LOCAL_ADAPTER_SECRET")]
    public async Task TrySubmitAsync_FigmaAndCanvaPackages_ReturnStableSuccessEnvelope(
        string sourceKind,
        string privateMarker)
    {
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter();
        var package = CreatePackage(sourceKind, privateMarker);

        var envelope = await adapter.TrySubmitAsync(package);
        var (submitAdapter, _, _) = CreateAdapter();
        var submittedPackage = package with { RequestId = $"{package.RequestId}-submit" };
        var submitted = await submitAdapter.SubmitAsync(submittedPackage);

        AssertStableSuccessEnvelope(envelope, package, privateMarker);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        AssertStableSuccessEnvelope(submitted.Value!, submittedPackage, privateMarker);
        Assert.NotEmpty(renderProvider.Requests);
        Assert.NotEmpty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task TrySubmitAsync_DefaultSubmitWithoutBaseUrl_ReturnsActionableBoundedNonReadyMetadata()
    {
        var (adapter, _, _) = CreateAdapter();
        var package = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_PASS_FLOW47_BASEURL_SECRET");

        var envelope = await adapter.TrySubmitAsync(package);

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RuntimeInvoked);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointRoute, envelope.BoundaryMetadata["default_submit_endpoint_route"]);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, envelope.BoundaryMetadata["default_submit_endpoint_path"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["default_submit_base_url_configured"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["default_submit_base_url_missing"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
            envelope.BoundaryMetadata["default_submit_base_url_status"]);
        Assert.Equal("BaseUrl", envelope.BoundaryMetadata["default_submit_base_url_missing_setting"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlNextAction,
            envelope.BoundaryMetadata["default_submit_base_url_next_action"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["local_in_process_submit_route_available"]);
        Assert.Equal("socketless_modelserver_endpoint_registry", envelope.BoundaryMetadata["local_in_process_submit_route_kind"]);
        AssertFlow47NotEvaluated(
            envelope.BoundaryMetadata,
            DesignConversionModelServerCallerContract.Flow47VerificationStateNotEvaluatedInLocalCaller);
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
            envelope.BoundaryMetadata["flow47_submit_status"]);
        Assert.Equal("BaseUrl", envelope.BoundaryMetadata["flow47_missing_setting_names"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlNextAction,
            envelope.BoundaryMetadata["flow47_next_action"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonBaseUrlPayload_PreparesConcreteLoopbackSubmitUriWithoutLiveServer()
    {
        const string requestId = "ready-json-helper-unit-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        var responseJson = JsonSerializer.Serialize(new
        {
            contractStatus = DesignConversionThinClientStatus.Completed,
            requestId,
            boundaryMetadata = new Dictionary<string, string>
            {
                ["runtime_invoked"] = bool.TrueString
            }
        }, DesignConversionModelServerHttpHandler.JsonOptions);
        var handler = new CapturingSubmitHandler(responseJson);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_READY_JSON_BASE_URL_UNIT_SECRET") with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var submitted = result.Value!;
        var expectedBaseUrl = new Uri("http://127.0.0.1:54321");
        var expectedRequestUri = new Uri(expectedBaseUrl, DesignConversionModelServerEndpointRegistry.EndpointPath);
        Assert.Equal(expectedBaseUrl, submitted.BaseUrl);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, submitted.EndpointPath);
        Assert.Equal(expectedRequestUri, submitted.RequestUri);
        Assert.Equal(HttpMethod.Post.Method, handler.CapturedMethod);
        Assert.Equal(expectedRequestUri, handler.CapturedRequestUri);
        Assert.Equal(1, handler.SendCount);
        Assert.True(submitted.ReadyJsonBaseUrlConfigured);
        Assert.False(submitted.ReadyJsonBaseUrlMissing);
        Assert.True(submitted.ReadyJsonBaseUrlLoopback);
        Assert.True(submitted.ReadyJsonAdvertisedSubmitRoute);
        Assert.False(submitted.ProductReady);
        Assert.Equal(requestId, submitted.Envelope.ParsedRequestId);

        using var capturedBody = JsonDocument.Parse(handler.CapturedBodyJson!);
        Assert.Equal(requestId, capturedBody.RootElement.GetProperty("requestId").GetString());

        var metadata = submitted.Envelope.BoundaryMetadata;
        Assert.Equal("ready.json", metadata["ready_json_base_url_source"]);
        Assert.Equal(expectedBaseUrl.GetLeftPart(UriPartial.Authority), metadata["ready_json_base_url"]);
        Assert.Equal(bool.TrueString, metadata["ready_json_base_url_configured"]);
        Assert.Equal(bool.FalseString, metadata["ready_json_base_url_missing"]);
        Assert.Equal(bool.TrueString, metadata["ready_json_base_url_loopback"]);
        Assert.Equal(expectedRequestUri.ToString(), metadata["ready_json_submit_request_uri"]);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.ConfiguredSubmitStatus, metadata["default_submit_base_url_status"]);
        Assert.Equal(bool.FalseString, metadata["flow47_verified"]);
        Assert.Equal(bool.TrueString, metadata["ready_json_submit_http_request_sent"]);
        Assert.Equal(HttpMethod.Post.Method, metadata["ready_json_submit_http_method"]);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.ConfiguredSubmitStatus, metadata["ready_json_submit_status"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.Flow47VerificationAuthority,
            metadata["flow47_verification_authority"]);
        Assert.Equal(
            DesignConversionModelServerReadyJsonSubmitClient.Flow47VerificationStateNotEvaluatedInReadyJson,
            metadata["flow47_verification_state"]);
        Assert.Equal(
            "public_flow47_post_conversion_submit_not_evaluated_at_ready_json_loopback_boundary",
            metadata["flow47_submit_status"]);
        Assert.Equal(string.Empty, metadata["flow47_missing_setting_names"]);
        Assert.Equal("xiigen_platform_post_conversion_submit_live_proof", metadata["flow47_missing_proof_name"]);
        Assert.Equal(
            "verify_xiigen_platform_post_conversion_submit_delegates_to_flow47_and_returns_202_job_status",
            metadata["flow47_next_action"]);
        Assert.Equal(string.Empty, metadata["flow47_base_url_source"]);
        Assert.Equal(string.Empty, metadata["flow47_endpoint_path"]);
        Assert.Equal(string.Empty, metadata["flow47_submit_request_uri"]);
        Assert.Equal(bool.FalseString, metadata["flow47_live_local_loopback_verified"]);
        Assert.False(handler.CapturedHeaders.ContainsKey("x-design-conversion-flow47-submit-status"));
        Assert.True(
            handler.CapturedHeaders.TryGetValue("x-design-conversion-ready-json-submit-status", out var readyJsonSubmitStatus),
            "Expected ready-json-specific submit status header.");
        Assert.Contains(
            DesignConversionModelServerReadyJsonSubmitClient.ConfiguredSubmitStatus,
            readyJsonSubmitStatus);
        Assert.Equal(bool.FalseString, metadata["product_ready"]);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonSendHttpFailure_ReturnsStableSafeFailureWithoutPrivateTransportDetails()
    {
        const string requestId = "ready-json-send-http-failure-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_SEND_FAILURE_SECRET_C:\\tenant\\ready.json";
        var handler = new ThrowingSubmitHandler(
            _ => new HttpRequestException($"connect failed at {privateMarker}"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitHttpFailedErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitHttpFailedMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonSendInvalidOperationFailure_ReturnsStableSafeFailureWithoutPrivateTransportDetails()
    {
        const string requestId = "ready-json-send-invalid-operation-failure-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_SEND_INVALID_OPERATION_SECRET_C:\\tenant\\ready.json";
        var handler = new ThrowingSubmitHandler(
            _ => new InvalidOperationException($"send pipeline failed at {privateMarker}"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitHttpFailedErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitHttpFailedMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonSendNonCallerTaskCanceledFailure_ReturnsStableSafeTimeoutWithoutPrivateTransportDetails()
    {
        const string requestId = "ready-json-send-task-canceled-failure-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_SEND_TASK_CANCELED_SECRET_C:\\tenant\\ready.json";
        var handler = new ThrowingSubmitHandler(
            _ => new TaskCanceledException($"send timed out at {privateMarker}"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitTimeoutErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitTimeoutMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
    }

    [Theory]
    [InlineData("io")]
    [InlineData("http")]
    public async Task TrySubmitAsync_ReadyJsonResponseReadFailure_ReturnsStableSafeFailureWithoutPrivateReadDetails(
        string failureKind)
    {
        const string requestId = "ready-json-response-read-failure-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_READ_FAILURE_SECRET_C:\\tenant\\response.json";
        var content = new ThrowingResponseContent(_ => failureKind == "io"
            ? new IOException($"response stream failed at {privateMarker}")
            : new HttpRequestException($"response read failed at {privateMarker}"));
        var handler = new ReadThrowingSubmitHandler(content);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitResponseReadFailedErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitResponseReadFailedMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
        Assert.True(content.DisposeCount > 0);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonResponseReadNonCallerTaskCanceledFailure_ReturnsStableSafeFailureWithoutPrivateReadDetails()
    {
        const string requestId = "ready-json-response-read-task-canceled-failure-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_READ_TASK_CANCELED_SECRET_C:\\tenant\\response.json";
        var content = new ThrowingResponseContent(
            _ => new TaskCanceledException($"response read timed out at {privateMarker}"));
        var handler = new ReadThrowingSubmitHandler(content);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitResponseReadFailedErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitResponseReadFailedMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
        Assert.True(content.DisposeCount > 0);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonResponseReadObjectDisposedFailure_ReturnsStableSafeFailureWithoutPrivateReadDetails()
    {
        const string requestId = "ready-json-response-read-object-disposed-failure-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_READ_OBJECT_DISPOSED_SECRET_C:\\tenant\\response.json";
        var content = new ThrowingResponseContent(
            _ => new ObjectDisposedException("ready-json-response", $"response stream disposed at {privateMarker}"));
        var handler = new ReadThrowingSubmitHandler(content);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitResponseReadFailedErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitResponseReadFailedMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
        Assert.True(content.DisposeCount > 0);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonOversizedResponseBody_FailsClosedBeforeParsing()
    {
        const string requestId = "ready-json-response-too-large-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_OVERSIZED_RESPONSE_SECRET_C:\\tenant\\response.json";
        var handler = new CapturingSubmitHandler(new string('x', (1024 * 1024) + 1));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal("READY_JSON_SUBMIT_RESPONSE_TOO_LARGE", result.ErrorCode);
        Assert.Equal(
            DesignConversionModelServerReadyJsonSubmitClient.SubmitResponseReadFailedMessage,
            result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonOversizedUnknownLengthResponseBody_FailsClosedWhileStreaming()
    {
        const int oversizedBodySize = (1024 * 1024) + 1;
        const string requestId = "ready-json-streamed-response-too-large-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string packagePrivateMarker = "PRIVATE_READY_JSON_STREAMED_PACKAGE_SECRET_C:\\tenant\\package.json";
        const string responsePrivateMarker = "PRIVATE_READY_JSON_STREAMED_RESPONSE_SECRET_C:\\tenant\\response.json";
        var responseBody = Encoding.UTF8.GetBytes(responsePrivateMarker.PadRight(oversizedBodySize, 'x'));
        var content = new UnknownLengthResponseContent(responseBody);
        var handler = new ReadThrowingSubmitHandler(content);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), packagePrivateMarker) with
        {
            RequestId = requestId
        };

        Assert.Equal(oversizedBodySize, responseBody.Length);
        Assert.Null(content.Headers.ContentLength);

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal("READY_JSON_SUBMIT_RESPONSE_TOO_LARGE", result.ErrorCode);
        Assert.Equal(
            DesignConversionModelServerReadyJsonSubmitClient.SubmitResponseReadFailedMessage,
            result.ErrorMessage);
        Assert.DoesNotContain(packagePrivateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(responsePrivateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("tenant", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(new string('x', 32), result.ErrorMessage, StringComparison.Ordinal);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
        Assert.True(content.DisposeCount > 0);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonSendCallerCancellation_ReturnsCancelledWithoutPrivateTransportDetails()
    {
        const string requestId = "ready-json-send-cancelled-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_SEND_CANCELLED_SECRET_C:\\tenant\\ready.json";
        using var cancellation = new CancellationTokenSource();
        var handler = new ThrowingSubmitHandler(
            token => new OperationCanceledException($"caller cancelled at {privateMarker}", token),
            beforeThrow: cancellation.Cancel);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package, cancellationToken: cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitCancelledErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitCancelledMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonResponseReadCallerCancellation_ReturnsCancelledAndDisposesResponse()
    {
        const string requestId = "ready-json-response-read-cancelled-request";
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        const string privateMarker = "PRIVATE_READY_JSON_READ_CANCELLED_SECRET_C:\\tenant\\response.json";
        using var cancellation = new CancellationTokenSource();
        var content = new ThrowingResponseContent(
            token => new OperationCanceledException($"caller cancelled at {privateMarker}", token),
            beforeThrow: cancellation.Cancel);
        var handler = new ReadThrowingSubmitHandler(content);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), privateMarker) with
        {
            RequestId = requestId
        };

        var result = await client.TrySubmitAsync(readyJson, package, cancellationToken: cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitCancelledErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitCancelledMessage, result.ErrorMessage);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        AssertReadyJsonSubmitRequestCaptured(handler.SendCount, handler.CapturedMethod, handler.CapturedRequestUri, handler.CapturedBodyJson, requestId);
        Assert.True(content.DisposeCount > 0);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonMissingBaseUrl_ReturnsBoundedNonReadyMetadataWithoutHttpRequest()
    {
        const string readyJson = "{ \"status\": \"ready\" }";
        var handler = new CapturingSubmitHandler("{}");
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_READY_JSON_MISSING_BASE_URL_SECRET");

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var submitted = result.Value!;
        Assert.Null(submitted.BaseUrl);
        Assert.Null(submitted.RequestUri);
        Assert.False(submitted.ReadyJsonBaseUrlConfigured);
        Assert.True(submitted.ReadyJsonBaseUrlMissing);
        Assert.False(submitted.ReadyJsonBaseUrlLoopback);
        Assert.False(submitted.ReadyJsonAdvertisedSubmitRoute);
        Assert.False(submitted.ProductReady);
        Assert.Equal(0, handler.SendCount);
        Assert.Null(handler.CapturedRequestUri);
        Assert.Null(handler.CapturedBodyJson);

        var envelope = submitted.Envelope;
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(400, envelope.StatusCode);
        Assert.False(envelope.RegistryInvokeSucceeded);
        Assert.False(envelope.RuntimeInvoked);
        Assert.Equal("READY_JSON_BASE_URL_REQUIRED", envelope.ParsedErrorCode);

        var metadata = envelope.BoundaryMetadata;
        Assert.Equal(bool.FalseString, metadata["ready_json_base_url_configured"]);
        Assert.Equal(bool.TrueString, metadata["ready_json_base_url_missing"]);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonBaseUrlMissingReason, metadata["ready_json_base_url_reason"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
            metadata["ready_json_base_url_status"]);
        Assert.Equal(bool.FalseString, metadata["ready_json_submit_http_request_sent"]);
        Assert.Equal(bool.TrueString, metadata["ready_json_submit_skipped"]);
        Assert.Equal(
            DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonSubmitSkippedStatus,
            metadata["ready_json_submit_status"]);
        Assert.Equal(bool.FalseString, metadata["default_submit_base_url_configured"]);
        Assert.Equal(bool.TrueString, metadata["default_submit_base_url_missing"]);
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
            metadata["default_submit_base_url_status"]);
        AssertFlow47NotEvaluated(
            metadata,
            DesignConversionModelServerReadyJsonSubmitClient.Flow47VerificationStateNotEvaluatedInReadyJson);
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
            metadata["flow47_submit_status"]);
        Assert.Equal(bool.FalseString, metadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, metadata["product_ready"]);
    }

    [Theory]
    [InlineData(
        "{ }",
        "READY_JSON_BASE_URL_REQUIRED",
        DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonBaseUrlMissingReason,
        DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
        true)]
    [InlineData(
        "{ \"baseUrl\": \"\" }",
        "READY_JSON_BASE_URL_REQUIRED",
        DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonBaseUrlMissingReason,
        DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
        true)]
    [InlineData(
        "{ \"baseUrl\": \"not a uri\" }",
        "READY_JSON_BASE_URL_INVALID",
        DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonBaseUrlInvalidReason,
        DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonBaseUrlInvalidStatus,
        false)]
    [InlineData(
        "{ \"baseUrl\": \"ftp://127.0.0.1:54321\" }",
        "READY_JSON_BASE_URL_INVALID",
        DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonBaseUrlInvalidReason,
        DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonBaseUrlInvalidStatus,
        false)]
    public async Task TrySubmitAsync_InvalidReadyJsonBaseUrl_ReturnsBoundedNonReadyMetadataWithoutHttpRequest(
        string readyJson,
        string expectedErrorCode,
        string expectedReason,
        string expectedStatus,
        bool expectedMissing)
    {
        var handler = new CapturingSubmitHandler("{}");
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_READY_JSON_INVALID_BASE_URL_SECRET");

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var submitted = result.Value!;
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.ClientBoundary, submitted.ClientBoundary);
        Assert.Null(submitted.BaseUrl);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, submitted.EndpointPath);
        Assert.Null(submitted.RequestUri);
        Assert.False(submitted.ReadyJsonBaseUrlConfigured);
        Assert.Equal(expectedMissing, submitted.ReadyJsonBaseUrlMissing);
        Assert.False(submitted.ReadyJsonBaseUrlLoopback);
        Assert.False(submitted.ReadyJsonAdvertisedSubmitRoute);
        Assert.False(submitted.ProductReady);
        Assert.Equal(0, handler.SendCount);
        Assert.Null(handler.CapturedRequestUri);
        Assert.Null(handler.CapturedBodyJson);

        var envelope = submitted.Envelope;
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(400, envelope.StatusCode);
        Assert.False(envelope.RegistryInvokeSucceeded);
        Assert.False(envelope.RuntimeInvoked);
        Assert.Equal(expectedErrorCode, envelope.ParsedErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(envelope.ParsedErrorMessage));
        Assert.False(envelope.PublicHttpEndpointVerified);
        Assert.False(envelope.CrossRepoCallerVerified);
        Assert.False(envelope.ExternalCallerRepoTouched);

        var metadata = envelope.BoundaryMetadata;
        Assert.Equal(bool.FalseString, metadata["ready_json_base_url_configured"]);
        Assert.Equal(expectedMissing.ToString(), metadata["ready_json_base_url_missing"]);
        Assert.Equal(bool.FalseString, metadata["ready_json_base_url_loopback"]);
        Assert.Equal(expectedReason, metadata["ready_json_base_url_reason"]);
        Assert.Equal(expectedStatus, metadata["ready_json_base_url_status"]);
        Assert.Equal(expectedErrorCode, metadata["ready_json_error_code"]);
        Assert.Equal(bool.FalseString, metadata["ready_json_submit_http_request_sent"]);
        Assert.Equal(bool.TrueString, metadata["ready_json_submit_skipped"]);
        Assert.Equal(
            DesignConversionModelServerReadyJsonSubmitClient.ReadyJsonSubmitSkippedStatus,
            metadata["ready_json_submit_status"]);
        Assert.Equal(bool.FalseString, metadata["default_submit_base_url_configured"]);
        Assert.Equal(expectedMissing.ToString(), metadata["default_submit_base_url_missing"]);
        Assert.Equal(expectedStatus, metadata["default_submit_base_url_status"]);
        AssertFlow47NotEvaluated(
            metadata,
            DesignConversionModelServerReadyJsonSubmitClient.Flow47VerificationStateNotEvaluatedInReadyJson);
        Assert.Equal(expectedStatus, metadata["flow47_submit_status"]);
        Assert.Equal(bool.FalseString, metadata["runtime_invoked"]);
        Assert.Equal(bool.FalseString, metadata["socket_opened"]);
        Assert.Equal(bool.FalseString, metadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, metadata["product_ready"]);
    }

    [Theory]
    [InlineData("originalTree", "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED")]
    [InlineData("artifactRefs", "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED")]
    [InlineData("unsupportedArtifactRef", "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED")]
    [InlineData("sourceKind", "SOURCE_KIND_UNSUPPORTED")]
    public async Task TrySubmitAsync_InvalidPackage_ReturnsStableLocalValidationEnvelope(
        string missing,
        string expectedErrorCode)
    {
        var descriptor = DesignConversionModelServerCallerContract.Describe();
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter();
        var package = CreatePackage(nameof(DesignConversionSourceKind.Figma), "PRIVATE_PASS126_INVALID_SECRET");
        package = missing switch
        {
            "originalTree" => package with { OriginalTree = null },
            "artifactRefs" => package with { Screenshots = [], DesignRenders = [] },
            "unsupportedArtifactRef" => package with
            {
                Screenshots =
                [
                    package.Screenshots![0] with
                    {
                        ArtifactId = "unsupported-local-caller-screenshot-proof",
                        Uri = "https://client.example.invalid/local-caller-screenshot-proof.png",
                        Sha256 = "sha256:unsupported-local-caller-screenshot-proof",
                        Metadata = new Dictionary<string, string>
                        {
                            ["proof_kind"] = "unsupported_external_uri_only"
                        }
                    }
                ],
                DesignRenders = []
            },
            "sourceKind" => package with { SourceKind = "Sketch" },
            _ => package
        };

        var envelope = await adapter.TrySubmitAsync(package);
        var submitted = await adapter.SubmitAsync(package);

        AssertStableErrorEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false,
            expectedErrorCode);
        Assert.Equal("local_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.False(submitted.IsSuccess);
        Assert.Equal(expectedErrorCode, submitted.ErrorCode);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        if (!string.Equals(expectedErrorCode, "SOURCE_KIND_UNSUPPORTED", StringComparison.Ordinal))
        {
            Assert.Contains(descriptor.ValidationFailures, failure =>
                failure.ErrorCode == expectedErrorCode && failure.StatusCode == 400);
        }
    }

    [Theory]
    [InlineData("html", "html", 'h')]
    [InlineData("designTree.payload", "designTree.payload", 'd')]
    [InlineData("originalTree.payload", "originalTree.payload", 'o')]
    public async Task TrySubmitAsync_OversizedRawPayload_FailsLocalValidationBeforeRuntime(
        string oversizedField,
        string expectedErrorField,
        char fillChar)
    {
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter();
        var basePackage = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_OVERSIZED_LOCAL_CALLER_SECRET");
        var package = basePackage with
        {
            Html = oversizedField == "html"
                ? new string(fillChar, DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1)
                : "<main>bounded local caller html</main>",
            DesignTree = oversizedField == "designTree.payload"
                ? basePackage.DesignTree! with
                    {
                        Payload = new string(fillChar, DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1)
                    }
                : basePackage.DesignTree,
            OriginalTree = oversizedField == "originalTree.payload"
                ? basePackage.OriginalTree! with
                    {
                        Payload = new string(fillChar, DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1)
                    }
                : basePackage.OriginalTree
        };

        var envelope = await adapter.TrySubmitAsync(package);
        var submitted = await adapter.SubmitAsync(package);

        AssertStableErrorEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false,
            DesignConversionModelServerPayloadSizeGuard.ErrorCode);
        Assert.Contains(expectedErrorField, envelope.ParsedErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("local_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.False(submitted.IsSuccess);
        Assert.Equal(DesignConversionModelServerPayloadSizeGuard.ErrorCode, submitted.ErrorCode);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task TrySubmitAsync_ReadyJsonValidBaseUrlRejectsUnsupportedArtifactRefsBeforeHttpRequest()
    {
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        var handler = new CapturingSubmitHandler("{}");
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_READY_JSON_UNSUPPORTED_ARTIFACT_SECRET");
        package = package with
        {
            Screenshots =
            [
                package.Screenshots![0] with
                {
                    ArtifactId = "unsupported-ready-json-screenshot-proof",
                    Uri = "https://client.example.invalid/ready-json-screenshot-proof.png",
                    Sha256 = "sha256:unsupported-ready-json-screenshot-proof"
                }
            ],
            DesignRenders = []
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", result.ErrorCode);
        Assert.Contains("refs-only", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, handler.SendCount);
        Assert.Null(handler.CapturedRequestUri);
        Assert.Null(handler.CapturedBodyJson);
    }

    [Theory]
    [InlineData("css", "css", 'c')]
    [InlineData("designTree.payload", "designTree.payload", 'd')]
    [InlineData("originalTree.payload", "originalTree.payload", 'o')]
    public async Task TrySubmitAsync_ReadyJsonValidBaseUrlRejectsOversizedPackageBeforeHttpRequest(
        string oversizedField,
        string expectedErrorField,
        char fillChar)
    {
        const string readyJson = "{ \"baseUrl\": \"http://127.0.0.1:54321\" }";
        var handler = new CapturingSubmitHandler("{}");
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var basePackage = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_READY_JSON_OVERSIZED_PAYLOAD_SECRET");
        var package = basePackage with
        {
            Css = oversizedField == "css"
                ? new string(fillChar, DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1)
                : ".screen{display:grid;gap:16px;}",
            DesignTree = oversizedField == "designTree.payload"
                ? basePackage.DesignTree! with
                    {
                        Payload = new string(fillChar, DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1)
                    }
                : basePackage.DesignTree,
            OriginalTree = oversizedField == "originalTree.payload"
                ? basePackage.OriginalTree! with
                    {
                        Payload = new string(fillChar, DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1)
                    }
                : basePackage.OriginalTree
        };

        var result = await client.TrySubmitAsync(readyJson, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerPayloadSizeGuard.ErrorCode, result.ErrorCode);
        Assert.Contains(expectedErrorField, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, handler.SendCount);
        Assert.Null(handler.CapturedRequestUri);
        Assert.Null(handler.CapturedBodyJson);
    }

    [Fact]
    public async Task TryInvokeHttpAsync_HandlerValidationFailure_ReturnsSameStableErrorEnvelopeShape()
    {
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter();
        var apiRequest = DesignConversionModelServerCallerContract.CreateFigmaExampleRequest(
            "pass126-handler-validation-envelope") with
        {
            OriginalTree = new DesignConversionModelServerPayloadRef(
                RefId: string.Empty,
                Kind: "figma-json",
                Sha256: null,
                Payload: null,
                Format: "figma-json")
        };
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.EndpointPath,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json");

        var envelope = await adapter.TryInvokeHttpAsync(request, nameof(DesignConversionSourceKind.Figma));

        AssertStableErrorEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: true,
            runtimeInvoked: false,
            expectedErrorCode: "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED");
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Theory]
    [InlineData(
        "GET",
        DesignConversionModelServerEndpointRegistry.EndpointPath,
        405,
        "DESIGN_CONVERSION_ENDPOINT_METHOD_NOT_ALLOWED")]
    [InlineData(
        "POST",
        "/api/design-conversion/missing",
        404,
        "DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND")]
    public async Task TryInvokeHttpAsync_RegistryRouteValidation_ReturnsStableRegistryErrorEnvelope(
        string method,
        string path,
        int expectedStatusCode,
        string expectedErrorCode)
    {
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter();
        var apiRequest = DesignConversionModelServerCallerContract.CreateFigmaExampleRequest(
            "pass126-registry-error-envelope");
        var request = new DesignConversionModelServerHttpRequest(
            Method: method,
            Path: path,
            BodyJson: JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json");

        var envelope = await adapter.TryInvokeHttpAsync(request, nameof(DesignConversionSourceKind.Figma));

        AssertStableErrorEnvelope(
            envelope,
            statusCode: expectedStatusCode,
            registryInvokeSucceeded: false,
            runtimeInvoked: false,
            expectedErrorCode);
        Assert.Equal("endpoint_registry", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Theory]
    [InlineData(
        "GET",
        DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
        405,
        "DESIGN_CONVERSION_ENDPOINT_METHOD_NOT_ALLOWED")]
    [InlineData(
        "POST",
        DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath + "/not-registered",
        404,
        "DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND")]
    public async Task TryInvokeHttpAsync_ImprovementLoopRegistryRouteValidation_ReturnsStableRegistryErrorEnvelopeWithoutRuntimeInvocation(
        string method,
        string path,
        int expectedStatusCode,
        string expectedErrorCode)
    {
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter(registerImprovementLoopRoute: true);
        var request = new DesignConversionModelServerHttpRequest(
            Method: method,
            Path: path,
            BodyJson: "{}",
            ContentType: "application/json");

        var envelope = await adapter.TryInvokeImprovementLoopHttpAsync(request, nameof(DesignConversionSourceKind.Figma));

        AssertStableErrorEnvelope(
            envelope,
            statusCode: expectedStatusCode,
            registryInvokeSucceeded: false,
            runtimeInvoked: false,
            expectedErrorCode);
        Assert.Equal("endpoint_registry", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, envelope.EndpointPath);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            envelope.BoundaryMetadata["local_caller_endpoint_path"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task TrySubmitAsync_EvaluateRouteDescriptorMirrorsCallerContractDescriptor()
    {
        var descriptor = DesignConversionModelServerCallerContract.Describe();
        var expectedRequiredRequestFields = RequiredRequestFieldNames(descriptor.RequestFields);
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter();
        var package = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_PASS126_EVALUATE_DESCRIPTOR_SECRET");

        var envelope = await adapter.TrySubmitAsync(package);

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(200, envelope.StatusCode);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.Equal(descriptor.SchemaVersion, envelope.ContractSchemaVersion);
        Assert.Equal(descriptor.EndpointMethod, envelope.EndpointMethod);
        Assert.Equal(descriptor.EndpointPath, envelope.EndpointPath);
        Assert.Equal(expectedRequiredRequestFields, envelope.RequiredRequestFields);
        Assert.Equal(descriptor.SchemaVersion, envelope.BoundaryMetadata["caller_contract_schema_version"]);
        Assert.Equal(descriptor.Boundary, envelope.BoundaryMetadata["caller_contract_boundary"]);
        Assert.Equal(
            string.Join(",", expectedRequiredRequestFields),
            envelope.BoundaryMetadata["caller_contract_required_fields"]);
        Assert.Equal(descriptor.EndpointMethod, envelope.BoundaryMetadata["local_caller_endpoint_method"]);
        Assert.Equal(descriptor.EndpointPath, envelope.BoundaryMetadata["local_caller_endpoint_path"]);
        Assert.Equal(descriptor.ResponseContentType, envelope.BoundaryMetadata["http_content_type"]);
        Assert.NotEmpty(renderProvider.Requests);
        Assert.NotEmpty(comparisonProvider.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RouteDescriptorMirrorsCallerContractDescriptor()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeImprovementLoopRoute();
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter(registerImprovementLoopRoute: true);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass126-improvement-loop-route-descriptor",
            InitialPackage: CreatePackage(
                nameof(DesignConversionSourceKind.Figma),
                "PRIVATE_PASS126_IMPROVEMENT_LOOP_DESCRIPTOR_SECRET"),
            MaxAttempts: 1);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(503, envelope.StatusCode);
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.CoordinatorNotConfiguredErrorCode,
            envelope.ParsedErrorCode);
        Assert.NotEqual("UNSUPPORTED_MEDIA_TYPE", envelope.ParsedErrorCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.False(envelope.RuntimeInvoked);
        Assert.Equal(descriptor.SchemaVersion, envelope.ContractSchemaVersion);
        Assert.Equal(descriptor.EndpointMethod, envelope.EndpointMethod);
        Assert.Equal(descriptor.EndpointPath, envelope.EndpointPath);
        Assert.Equal(
            RequiredRequestFieldNames(descriptor.RequestFields),
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
    public void CallerContract_ImprovementLoopDescriptorDocumentsTypedIterationOverride()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeImprovementLoopRoute();

        var maxAttempts = Assert.Single(descriptor.RequestFields, field => field.JsonName == "maxAttempts");
        Assert.False(maxAttempts.Required);
        Assert.DoesNotContain("maxAttempts", RequiredRequestFieldNames(descriptor.RequestFields));
        Assert.Contains("legacy", maxAttempts.Requirement, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("total evaluation-attempt cap", maxAttempts.Requirement, StringComparison.Ordinal);
        Assert.Contains(
            $"1..{DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts}",
            maxAttempts.Requirement,
            StringComparison.Ordinal);
        Assert.Contains(
            "initialPackage.budget.maxImprovementIterations is absent",
            maxAttempts.Requirement,
            StringComparison.Ordinal);
        Assert.Contains("post-initial improvement cycles", maxAttempts.Requirement, StringComparison.Ordinal);
        Assert.Contains("overrides this legacy value", maxAttempts.Requirement, StringComparison.OrdinalIgnoreCase);

        var requiredFailure = Assert.Single(
            descriptor.ValidationFailures,
            failure => failure.ErrorCode == "IMPROVEMENT_LOOP_MAX_ATTEMPTS_REQUIRED");
        Assert.Contains(
            "legacy maxAttempts",
            requiredFailure.Trigger,
            StringComparison.Ordinal);
        Assert.Contains(
            "initialPackage.budget.maxImprovementIterations is absent",
            requiredFailure.Trigger,
            StringComparison.Ordinal);

        var exceededFailure = Assert.Single(
            descriptor.ValidationFailures,
            failure => failure.ErrorCode == DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode);
        Assert.Contains(
            "Legacy maxAttempts",
            exceededFailure.Trigger,
            StringComparison.Ordinal);
        Assert.Contains(
            "initialPackage.budget.maxImprovementIterations is absent",
            exceededFailure.Trigger,
            StringComparison.Ordinal);

        var typedInvalidFailure = Assert.Single(
            descriptor.ValidationFailures,
            failure => failure.ErrorCode == DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode);
        Assert.Contains(
            "initialPackage.budget.maxImprovementIterations",
            typedInvalidFailure.Trigger,
            StringComparison.Ordinal);
        Assert.Contains("post-initial improvement cycles", typedInvalidFailure.Trigger, StringComparison.Ordinal);
        Assert.Contains("overrides legacy maxAttempts", typedInvalidFailure.Trigger, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_InvalidMaxImprovementIterationsFailsBeforeRegistryOrRuntime()
    {
        var (adapter, renderProvider, comparisonProvider) = CreateAdapter(registerImprovementLoopRoute: true);
        var initialPackage = CreatePackage(
            nameof(DesignConversionSourceKind.Figma),
            "PRIVATE_PASS126_INVALID_TYPED_ITERATIONS_SECRET");
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass126-invalid-typed-iterations-loop",
            InitialPackage: initialPackage with
            {
                Budget = initialPackage.Budget! with
                {
                    MaxImprovementIterations = -1
                }
            },
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableErrorEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false,
            DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Fact]
    public void CreateEnvelopeFromHttpResponse_UnparseableBody_ReturnsStableParseFailureEnvelope()
    {
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: 502,
            ContentType: "application/json; charset=utf-8",
            BodyJson: "<main data-secret=\"PRIVATE_PASS126_PARSE_SECRET\"></main>",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            nameof(DesignConversionSourceKind.Figma),
            registryInvokeSucceeded: true);

        AssertStableErrorEnvelope(
            envelope,
            statusCode: 502,
            registryInvokeSucceeded: true,
            runtimeInvoked: false,
            expectedErrorCode: DesignConversionModelServerLocalCallerAdapter.ParseFailureErrorCode);
        Assert.False(envelope.ParsedResponseJsonSucceeded);
        Assert.False(string.IsNullOrWhiteSpace(envelope.ResponseJsonParseError));
        Assert.True(envelope.RawHtmlIncluded);
        Assert.True(envelope.ResponseContainsRawPrivateData);
        Assert.True(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_caller_repo_touched"]);
    }

    [Fact]
    public void CreateEnvelopeFromHttpResponse_MetadataWithNotSecretText_DoesNotReportRawPrivateLeakage()
    {
        var bodyJson = JsonSerializer.Serialize(new
        {
            contractStatus = DesignConversionThinClientStatus.Completed,
            requestId = "safe-not-secret-metadata-response",
            boundaryMetadata = new Dictionary<string, string>
            {
                ["runtime_invoked"] = bool.TrueString,
                ["sanitization_mode"] = "pseudonymous_technical_scoped_sha256_not_anonymous_not_secret",
                ["response_contains_raw_private_data"] = bool.FalseString,
                ["raw_user_private_data_included"] = bool.FalseString
            }
        }, DesignConversionModelServerHttpHandler.JsonOptions);
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: 200,
            ContentType: "application/json; charset=utf-8",
            BodyJson: bodyJson,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            nameof(DesignConversionSourceKind.Figma),
            registryInvokeSucceeded: true);

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(
            "pseudonymous_technical_scoped_sha256_not_anonymous_not_secret",
            envelope.BoundaryMetadata["sanitization_mode"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_private_response_leakage_detected"]);
    }

    [Theory]
    [InlineData("secret")]
    [InlineData("clientSecret")]
    [InlineData("api_secret")]
    [InlineData("SecretValue")]
    [InlineData("APISecret")]
    [InlineData("clientSECRETValue")]
    public void CreateEnvelopeFromHttpResponse_ParseableJsonSensitiveSecretPropertyName_ReportsRawPrivateLeakage(
        string propertyName)
    {
        var bodyJson =
            "{ \"contractStatus\": \"" + DesignConversionThinClientStatus.Completed + "\", " +
            "\"requestId\": \"parseable-secret-property-response-" + propertyName + "\", " +
            "\"payload\": { \"" + propertyName + "\" : \"PRIVATE_PASS126_PARSEABLE_JSON_SECRET\" }, " +
            "\"boundaryMetadata\": { \"runtime_invoked\": \"True\" } }";
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: 200,
            ContentType: "application/json; charset=utf-8",
            BodyJson: bodyJson,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            nameof(DesignConversionSourceKind.Figma),
            registryInvokeSucceeded: true);

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.True(envelope.ResponseContainsRawPrivateData);
        Assert.True(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["raw_private_response_leakage_detected"]);
    }

    [Theory]
    [InlineData("secretary", "not_secret")]
    [InlineData("secretariat", "clientSecret")]
    [InlineData("APISecretary", "not_secret")]
    [InlineData("safePayload", "not_secret")]
    [InlineData("safePayload", "clientSecret")]
    public void CreateEnvelopeFromHttpResponse_ParseableJsonNonSensitiveNamesAndValues_DoesNotReportRawPrivateLeakage(
        string propertyName,
        string propertyValue)
    {
        var bodyJson =
            "{ \"contractStatus\": \"" + DesignConversionThinClientStatus.Completed + "\", " +
            "\"requestId\": \"parseable-non-sensitive-secret-text-response\", " +
            "\"payload\": { \"" + propertyName + "\" : \"" + propertyValue + "\" }, " +
            "\"boundaryMetadata\": { \"runtime_invoked\": \"True\" } }";
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: 200,
            ContentType: "application/json; charset=utf-8",
            BodyJson: bodyJson,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            nameof(DesignConversionSourceKind.Figma),
            registryInvokeSucceeded: true);

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_private_response_leakage_detected"]);
    }

    [Theory]
    [InlineData("response_contains_raw_private_data")]
    [InlineData("raw_user_private_data_included")]
    public void CreateEnvelopeFromHttpResponse_PrivateMetadataFlagTrue_ReportsRawPrivateLeakage(string flagName)
    {
        var bodyJson = JsonSerializer.Serialize(new
        {
            contractStatus = DesignConversionThinClientStatus.Completed,
            requestId = $"private-metadata-flag-{flagName}",
            boundaryMetadata = new Dictionary<string, string>
            {
                ["runtime_invoked"] = bool.TrueString,
                [flagName] = bool.TrueString
            }
        }, DesignConversionModelServerHttpHandler.JsonOptions);
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: 200,
            ContentType: "application/json; charset=utf-8",
            BodyJson: bodyJson,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            nameof(DesignConversionSourceKind.Figma),
            registryInvokeSucceeded: true);

        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.True(envelope.ResponseContainsRawPrivateData);
        Assert.True(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["raw_private_response_leakage_detected"]);
    }

    [Theory]
    [InlineData("secret")]
    [InlineData("clientSecret")]
    [InlineData("api_secret")]
    [InlineData("SecretValue")]
    [InlineData("APISecret")]
    [InlineData("clientSECRETValue")]
    public void CreateEnvelopeFromHttpResponse_UnparseableJsonSensitiveSecretPropertyName_ReturnsStableParseFailureAndReportsLeakage(
        string propertyName)
    {
        var bodyJson =
            "{ \"contractStatus\": \"" + DesignConversionThinClientStatus.Completed + "\", " +
            "\"payload\": { \"" + propertyName + "\" : \"PRIVATE_PASS126_MALFORMED_JSON_SECRET\" }";
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: 502,
            ContentType: "application/json; charset=utf-8",
            BodyJson: bodyJson,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            nameof(DesignConversionSourceKind.Figma),
            registryInvokeSucceeded: true);

        AssertStableErrorEnvelope(
            envelope,
            statusCode: 502,
            registryInvokeSucceeded: true,
            runtimeInvoked: false,
            expectedErrorCode: DesignConversionModelServerLocalCallerAdapter.ParseFailureErrorCode);
        Assert.False(envelope.ParsedResponseJsonSucceeded);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.True(envelope.ResponseContainsRawPrivateData);
        Assert.True(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["raw_private_response_leakage_detected"]);
    }

    [Theory]
    [InlineData("secretary", "not_secret")]
    [InlineData("secretariat", "clientSecret")]
    [InlineData("APISecretary", "not_secret")]
    [InlineData("safePayload", "not_secret")]
    [InlineData("safePayload", "clientSecret")]
    public void CreateEnvelopeFromHttpResponse_UnparseableJsonNonSensitiveNamesAndValues_DoesNotReportRawPrivateLeakage(
        string propertyName,
        string propertyValue)
    {
        var bodyJson =
            "{ \"contractStatus\": \"" + DesignConversionThinClientStatus.Completed + "\", " +
            "\"payload\": { \"" + propertyName + "\" : \"" + propertyValue + "\" }";
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: 502,
            ContentType: "application/json; charset=utf-8",
            BodyJson: bodyJson,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            nameof(DesignConversionSourceKind.Figma),
            registryInvokeSucceeded: true);

        AssertStableErrorEnvelope(
            envelope,
            statusCode: 502,
            registryInvokeSucceeded: true,
            runtimeInvoked: false,
            expectedErrorCode: DesignConversionModelServerLocalCallerAdapter.ParseFailureErrorCode);
        Assert.False(envelope.ParsedResponseJsonSucceeded);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_private_response_leakage_detected"]);
    }

    [Fact]
    public void CallerContract_DescribesStableLocalCallerResponseEnvelope()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeLocalCallerResponseEnvelope();

        Assert.Equal(
            DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion,
            descriptor.SchemaVersion);
        Assert.Equal(
            DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary,
            descriptor.Boundary);
        Assert.Contains("success", descriptor.EnvelopeKinds);
        Assert.Contains("error", descriptor.EnvelopeKinds);
        Assert.Contains(descriptor.Fields, field => field.JsonName == "statusCode" && field.Required);
        Assert.Contains(descriptor.Fields, field => field.JsonName == "registryInvokeSucceeded" && field.Required);
        Assert.Contains(descriptor.Fields, field => field.JsonName == "runtimeInvoked" && field.Required);
        Assert.Contains(descriptor.Fields, field => field.JsonName == "parsedErrorCode" && !field.Required);
        Assert.Equal(
            bool.TrueString,
            descriptor.BoundaryMetadata["local_caller_success_error_envelope_stable"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_caller_repo_touched"]);
    }

    private static IReadOnlyList<string> RequiredRequestFieldNames(
        IReadOnlyList<DesignConversionModelServerCallerContractField> requestFields)
        => requestFields
            .Where(field => field.Required)
            .Select(field => field.JsonName)
            .ToArray();

    private static void AssertReadyJsonSubmitRequestCaptured(
        int sendCount,
        string? capturedMethod,
        Uri? capturedRequestUri,
        string? capturedBodyJson,
        string expectedRequestId)
    {
        var expectedBaseUrl = new Uri("http://127.0.0.1:54321");
        var expectedRequestUri = new Uri(expectedBaseUrl, DesignConversionModelServerEndpointRegistry.EndpointPath);

        Assert.Equal(1, sendCount);
        Assert.Equal(HttpMethod.Post.Method, capturedMethod);
        Assert.Equal(expectedRequestUri, capturedRequestUri);
        Assert.False(string.IsNullOrWhiteSpace(capturedBodyJson));

        using var capturedBody = JsonDocument.Parse(capturedBodyJson!);
        Assert.Equal(expectedRequestId, capturedBody.RootElement.GetProperty("requestId").GetString());
    }

    private static void AssertFlow47NotEvaluated(
        IReadOnlyDictionary<string, string> metadata,
        string expectedState)
    {
        Assert.False(metadata.ContainsKey("flow47_verified"));
        Assert.Equal(
            DesignConversionModelServerCallerContract.Flow47VerificationAuthority,
            metadata["flow47_verification_authority"]);
        Assert.Equal(expectedState, metadata["flow47_verification_state"]);
    }

    private static void AssertStableSuccessEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        DesignConversionModelServerLocalCallerPackage package,
        string privateMarker)
    {
        var descriptor = DesignConversionModelServerCallerContract.Describe();

        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion, envelope.EnvelopeSchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary, envelope.EnvelopeBoundary);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionModelServerLocalCallerAdapter.AdapterBoundary, envelope.AdapterBoundary);
        Assert.Equal(DesignConversionModelServerCallerContract.SchemaVersion, envelope.ContractSchemaVersion);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointMethod, envelope.EndpointMethod);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, envelope.EndpointPath);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Null(envelope.ResponseJsonParseError);
        Assert.True(
            string.Equals(
                DesignConversionThinClientStatus.Completed,
                envelope.ParsedContractStatus,
                StringComparison.Ordinal),
            envelope.ResponseBodyJson);
        Assert.Equal(package.RequestId, envelope.ParsedRequestId);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.Equal(RequiredRequestFieldNames(descriptor.RequestFields), envelope.RequiredRequestFields);

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
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["local_caller_success_error_envelope_stable"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["registry_invoke_succeeded"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["runtime_invoked"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_caller_repo_touched"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
    }

    private static void AssertStableErrorEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        int statusCode,
        bool registryInvokeSucceeded,
        bool runtimeInvoked,
        string expectedErrorCode)
    {
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion, envelope.EnvelopeSchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary, envelope.EnvelopeBoundary);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(statusCode, envelope.StatusCode);
        Assert.Equal(registryInvokeSucceeded, envelope.RegistryInvokeSucceeded);
        Assert.Equal(runtimeInvoked, envelope.RuntimeInvoked);
        Assert.Equal(expectedErrorCode, envelope.ParsedErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(envelope.ParsedErrorMessage));
        Assert.False(envelope.ExternalCallerRepoTouched);
        Assert.False(envelope.CrossRepoCallerVerified);
        Assert.False(envelope.PublicHttpEndpointVerified);
        Assert.Equal(registryInvokeSucceeded.ToString(), envelope.BoundaryMetadata["registry_invoke_succeeded"]);
        Assert.Equal(runtimeInvoked.ToString(), envelope.BoundaryMetadata["runtime_invoked"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_caller_repo_touched"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
    }

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string sourceKind,
        string privateMarker)
    {
        var normalized = sourceKind.ToLowerInvariant();
        return new DesignConversionModelServerLocalCallerPackage(
            RequestId: BuildFixtureRequestId(normalized, privateMarker),
            SourceKind: sourceKind,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{normalized}-tree-ref",
                Kind: $"{normalized}-json",
                Sha256: $"sha256:pass126:{normalized}:design-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\"}}",
                Format: $"{normalized}-json",
                Metadata: new Dictionary<string, string>
                {
                    ["tree_private_marker"] = privateMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{normalized}-original-tree-ref",
                Kind: $"{normalized}-json",
                Sha256: $"sha256:pass126:{normalized}:original-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Format: $"{normalized}-json",
                Metadata: new Dictionary<string, string>
                {
                    ["original_tree_private_marker"] = privateMarker
                }),
            Html: $"<main data-secret=\"{privateMarker}\"><h1>Pass126 {sourceKind}</h1></main>",
            Css: $".screen{{background-image:url('{privateMarker}');}}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"pass126:{normalized}:source-screenshot",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: $"artifact://pass126/{normalized}/source-screenshot.png",
                    Sha256: $"sha256:pass126:{normalized}:source-screenshot"),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"pass126:{normalized}:source-screenshot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: $"artifact://pass126/{normalized}/source-screenshot-mobile.png",
                    Sha256: $"sha256:pass126:{normalized}:source-screenshot-mobile")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"pass126:{normalized}:design-render",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: $"artifact://pass126/{normalized}/design-render.png",
                    Sha256: $"sha256:pass126:{normalized}:design-render"),
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: $"pass126:{normalized}:design-render-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: $"artifact://pass126/{normalized}/design-render-mobile.png",
                    Sha256: $"sha256:pass126:{normalized}:design-render-mobile")
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: $"local-adapter-{privateMarker}",
                Tier: "local-pass126-contract-envelope",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.50m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker,
                ["caller_kind"] = sourceKind
            });
    }

    private static string BuildFixtureRequestId(
        string normalizedSourceKind,
        string privateMarker)
    {
        var hashInput = Encoding.UTF8.GetBytes(
            $"DesignConversionModelServerLocalCallerAdapterTests.CreatePackage:v1:{normalizedSourceKind}:{privateMarker}");
        var hash = SHA256.HashData(hashInput);
        var opaqueSuffix = Convert.ToHexString(hash).ToLowerInvariant()[..24];
        return $"pass126-{normalizedSourceKind}-local-caller-envelope-{opaqueSuffix}";
    }

    private (
        DesignConversionModelServerLocalCallerAdapter Adapter,
        RecordingRenderProvider RenderProvider,
        RecordingComparisonProvider ComparisonProvider) CreateAdapter(bool registerImprovementLoopRoute = false)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("pass126-local-adapter-verdict", 0.95, passed: true));
        var service = new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            new RecordingVisualJudge(CreateVerdict("pass126-local-adapter-visual-verdict", 0.95, passed: true)));
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            () => DateTimeOffset.Parse("2026-07-03T12:30:00Z"),
            artifactDb: CreateArtifactDb());
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(new DesignConversionModelServerHttpHandler(api));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        if (registerImprovementLoopRoute)
        {
            var improvementLoopRegistered = registry.RegisterDesignConversionImprovementLoopForLiveHostTesting(
                new DesignConversionModelServerImprovementLoopHandler(coordinator: null));
            Assert.True(improvementLoopRegistered.IsSuccess, improvementLoopRegistered.ErrorMessage);
        }

        return (new DesignConversionModelServerLocalCallerAdapter(registry), renderProvider, comparisonProvider);
    }

    private JsonlDesignConversionLocalArtifactDb CreateArtifactDb()
    {
        var root = Path.Combine(
            ArtifactDbClassRoot(),
            Guid.NewGuid().ToString("N"));
        _artifactDbRoots.Add(root);
        return new JsonlDesignConversionLocalArtifactDb(root);
    }

    public void Dispose()
    {
        foreach (var root in _artifactDbRoots)
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch (IOException)
            {
                // Best-effort cleanup must not hide the caller assertion result.
            }
            catch (UnauthorizedAccessException)
            {
                // Best-effort cleanup must not hide the caller assertion result.
            }
        }

        DeleteDirectoryIfEmpty(ArtifactDbClassRoot());
    }

    private static string ArtifactDbClassRoot()
        => Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core-tests",
            nameof(DesignConversionModelServerLocalCallerAdapterTests));

    private static void DeleteDirectoryIfEmpty(string path)
    {
        try
        {
            if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
                Directory.Delete(path);
        }
        catch (IOException)
        {
            // Best-effort cleanup must not hide the caller assertion result.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup must not hide the caller assertion result.
        }
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

        public Task<AgentInfrastructure.OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass126/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

            return Task.FromResult(AgentInfrastructure.OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["pass126-recording-render-provider"])));
        }
    }

    private sealed class RecordingComparisonProvider(VisualVerdict verdict) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<AgentInfrastructure.OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(AgentInfrastructure.OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://pass126/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["pass126-comparison-provider"])));
        }
    }

    private sealed class RecordingVisualJudge(VisualVerdict verdict) : IDesignConversionVisualJudge
    {
        public Task<AgentInfrastructure.OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(AgentInfrastructure.OperationResult<DesignConversionVisualJudgeResult>.Ok(
                new DesignConversionVisualJudgeResult(
                    JudgeResultId: $"{request.JudgeRequestId}:test-result",
                    JudgeRequestId: request.JudgeRequestId,
                    Status: "TEST_VISUAL_JUDGE_VERDICT_RECEIVED",
                    IsBlocked: false,
                    Verdict: verdict,
                    Provider: new DesignConversionVisualJudgeProviderMetadata(
                        ProviderName: "local-caller-test-visual-judge",
                        EndpointHost: "none",
                        ModelId: "test-double",
                        ExternalCallExecuted: false,
                        Metadata: new Dictionary<string, string>
                        {
                            ["test_double"] = bool.TrueString
                        }),
                    EstimatedCostUsd: 0m,
                    TokenMetadata: new Dictionary<string, double>(),
                    EvidenceRefs: ["local-caller:test-visual-judge", .. verdict.EvidenceRefs],
                    Metadata: new Dictionary<string, string>
                    {
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        ["test_double"] = bool.TrueString
                    })));
    }

    private sealed class ThrowingSubmitHandler(
        Func<CancellationToken, Exception> exceptionFactory,
        Action? beforeThrow = null) : HttpMessageHandler
    {
        public int SendCount { get; private set; }
        public string? CapturedMethod { get; private set; }
        public Uri? CapturedRequestUri { get; private set; }
        public string? CapturedBodyJson { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            CapturedMethod = request.Method.Method;
            CapturedRequestUri = request.RequestUri;
            CapturedBodyJson = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            beforeThrow?.Invoke();
            throw exceptionFactory(cancellationToken);
        }
    }

    private sealed class ReadThrowingSubmitHandler(HttpContent content) : HttpMessageHandler
    {
        public int SendCount { get; private set; }
        public string? CapturedMethod { get; private set; }
        public Uri? CapturedRequestUri { get; private set; }
        public string? CapturedBodyJson { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            CapturedMethod = request.Method.Method;
            CapturedRequestUri = request.RequestUri;
            CapturedBodyJson = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };
        }
    }

    private sealed class ThrowingResponseContent(
        Func<CancellationToken, Exception> exceptionFactory,
        Action? beforeThrow = null) : HttpContent
    {
        public int DisposeCount { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
        {
            beforeThrow?.Invoke();
            return Task.FromException(exceptionFactory(cancellationToken));
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;

            base.Dispose(disposing);
        }
    }

    private sealed class UnknownLengthResponseContent(byte[] body) : HttpContent
    {
        public int DisposeCount { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => stream.WriteAsync(body, 0, body.Length);

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
            => stream.WriteAsync(body.AsMemory(), cancellationToken).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;

            base.Dispose(disposing);
        }
    }

    private sealed class CapturingSubmitHandler(string responseJson) : HttpMessageHandler
    {
        public int SendCount { get; private set; }
        public string? CapturedMethod { get; private set; }
        public Uri? CapturedRequestUri { get; private set; }
        public string? CapturedBodyJson { get; private set; }
        public Dictionary<string, string[]> CapturedHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            CapturedMethod = request.Method.Method;
            CapturedRequestUri = request.RequestUri;
            CapturedHeaders.Clear();
            foreach (var header in request.Headers)
                CapturedHeaders[header.Key] = header.Value.ToArray();

            CapturedBodyJson = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        }
    }
}
