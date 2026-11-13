// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerReadyJsonSubmitResult(
    string ClientBoundary,
    Uri? BaseUrl,
    string EndpointPath,
    Uri? RequestUri,
    bool ReadyJsonBaseUrlConfigured,
    bool ReadyJsonBaseUrlMissing,
    bool ReadyJsonBaseUrlLoopback,
    bool ReadyJsonAdvertisedSubmitRoute,
    bool ProductReady,
    DesignConversionModelServerLocalCallerAdapterResponse Envelope);

public sealed class DesignConversionModelServerReadyJsonSubmitClient
{
    public const string ClientBoundary = "ready_json_base_url_live_local_submit_client_loopback_only";
    public const string ConfiguredSubmitStatus = "configured_live_local_ready_json_base_url";
    public const string ReadyJsonSubmitStatusHeaderName = "x-design-conversion-ready-json-submit-status";
    public const string Flow47PublicSubmitNotEvaluatedStatus =
        "public_flow47_post_conversion_submit_not_evaluated_at_ready_json_loopback_boundary";
    public const string Flow47MissingProofName = "xiigen_platform_post_conversion_submit_live_proof";
    public const string Flow47PublicSubmitNextAction =
        "verify_xiigen_platform_post_conversion_submit_delegates_to_flow47_and_returns_202_job_status";
    public const string ReadyJsonBaseUrlMissingReason = "base_url_missing";
    public const string ReadyJsonBaseUrlInvalidReason = "base_url_invalid";
    public const string ReadyJsonBaseUrlInvalidStatus = "bounded_non_ready_base_url_invalid";
    public const string ReadyJsonSubmitSkippedStatus = "bounded_non_ready_ready_json_submit_skipped_before_http_request";
    public const string Flow47VerificationStateNotEvaluatedInReadyJson = "not_evaluated_in_ready_json_client";
    public const int MaxReadyJsonFileBytes = (DesignConversionModelServerPayloadSizeGuard.MaxReadyJsonChars * 4) + 3;
    public const string ReadyJsonFileTooLargeErrorCode = "READY_JSON_FILE_TOO_LARGE";
    public const string ReadyJsonFileTooLargeMessage =
        "DesignConversion live-local ready.json file is too large to read.";
    public const string ReadyJsonReadFailedErrorCode = "READY_JSON_READ_FAILED";
    public const string ReadyJsonReadFailedMessage = "DesignConversion live-local ready.json could not be read.";
    public const string SubmitCancelledErrorCode = "CANCELLED";
    public const string SubmitCancelledMessage = "DesignConversion live-local ready.json submit was cancelled by the caller.";
    public const string SubmitHttpFailedErrorCode = "READY_JSON_SUBMIT_HTTP_FAILED";
    public const string SubmitHttpFailedMessage =
        "DesignConversion live-local ready.json submit failed before a response was received.";
    public const string SubmitTimeoutErrorCode = "READY_JSON_SUBMIT_TIMEOUT";
    public const string SubmitTimeoutMessage =
        "DesignConversion live-local ready.json submit timed out before a response was received.";
    public const string SubmitResponseReadFailedErrorCode = "READY_JSON_SUBMIT_RESPONSE_READ_FAILED";
    public const string SubmitResponseReadFailedMessage =
        "DesignConversion live-local ready.json submit response could not be read.";
    public const string SubmitResponseTooLargeErrorCode = "READY_JSON_SUBMIT_RESPONSE_TOO_LARGE";
    public const int MaxSubmitResponseBodyBytes = 1024 * 1024;
    public const long MaxReadyJsonArtifactUploadBytes = DesignConversionServerOwnedLocalArtifactRefResolver.DefaultMaxArtifactBytes;
    public const string ArtifactUploadNotReadyErrorCode = "READY_JSON_ARTIFACT_UPLOAD_NOT_READY";
    public const string ArtifactUploadNotReadyMessage =
        "DesignConversion live-local ready.json artifact upload route is not ready.";
    public const string ArtifactUploadFileInvalidErrorCode = "READY_JSON_ARTIFACT_UPLOAD_FILE_INVALID";
    public const string ArtifactUploadFileInvalidMessage =
        "DesignConversion live-local artifact upload source file is not a safe readable PNG.";
    public const string ArtifactUploadTooLargeErrorCode = "READY_JSON_ARTIFACT_UPLOAD_TOO_LARGE";
    public const string ArtifactUploadTooLargeMessage =
        "DesignConversion live-local artifact upload source file exceeds the ready.json upload size limit.";
    public const string ArtifactUploadSha256MismatchErrorCode = "READY_JSON_ARTIFACT_UPLOAD_SHA256_MISMATCH";
    public const string ArtifactUploadSha256MismatchMessage =
        "DesignConversion live-local artifact upload source file SHA-256 did not match the package artifact proof.";
    public const string ArtifactUploadHttpFailedErrorCode = "READY_JSON_ARTIFACT_UPLOAD_HTTP_FAILED";
    public const string ArtifactUploadHttpFailedMessage =
        "DesignConversion live-local artifact upload failed before a response was received.";
    public const string ArtifactUploadResponseInvalidErrorCode = "READY_JSON_ARTIFACT_UPLOAD_RESPONSE_INVALID";
    public const string ArtifactUploadResponseInvalidMessage =
        "DesignConversion live-local artifact upload response did not contain a valid refs-only receipt.";
    public const int MaxCallerRouteAdmissionTokenChars = 4096;
    public const int MaxCallerIdempotencyKeyChars = 128;
    public const string CallerOutboundHeaderInvalidErrorCode = "READY_JSON_CALLER_OUTBOUND_HEADER_INVALID";
    public const string CallerOutboundHeaderInvalidMessage =
        "DesignConversion live-local ready.json caller outbound headers are invalid.";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly HttpClient _httpClient;

    public DesignConversionModelServerReadyJsonSubmitClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<OperationResult<DesignConversionModelServerReadyJsonSubmitResult>> TrySubmitReadyFileAsync(
        string readyFilePath,
        DesignConversionModelServerLocalCallerPackage? package,
        string endpointPath = DesignConversionModelServerEndpointRegistry.EndpointPath,
        CancellationToken cancellationToken = default,
        string? routeAdmissionToken = null,
        string? idempotencyKey = null)
    {
        if (string.IsNullOrWhiteSpace(readyFilePath))
        {
            return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Fail(
                "DesignConversion live-local ready.json path is required.",
                "READY_JSON_PATH_REQUIRED");
        }

        var readyJson = await ReadBoundedReadyJsonFileAsync(readyFilePath, cancellationToken);
        if (!readyJson.IsSuccess)
        {
            if (string.Equals(readyJson.ErrorCode, ReadyJsonFileTooLargeErrorCode, StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Ok(
                    BuildBoundedReadyJsonFailureResult(
                        ReadyJsonFileTooLargeErrorCode,
                        ReadyJsonFileTooLargeMessage,
                        endpointPath,
                        package?.SourceKind ?? "UNKNOWN"));
            }

            return readyJson.MapError<DesignConversionModelServerReadyJsonSubmitResult>();
        }

        return await TrySubmitAsync(
            readyJson.Value!,
            package,
            endpointPath,
            cancellationToken,
            routeAdmissionToken,
            idempotencyKey);
    }

    public async Task<OperationResult<DesignConversionModelServerReadyJsonSubmitResult>> TrySubmitAsync(
        string? readyJson,
        DesignConversionModelServerLocalCallerPackage? package,
        string endpointPath = DesignConversionModelServerEndpointRegistry.EndpointPath,
        CancellationToken cancellationToken = default,
        string? routeAdmissionToken = null,
        string? idempotencyKey = null)
    {
        var readyJsonPayloadSize = DesignConversionModelServerPayloadSizeGuard.ValidateReadyJson(
            readyJson,
            "DesignConversion live-local ready.json content");
        if (!readyJsonPayloadSize.IsSuccess)
        {
            return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Ok(
                BuildBoundedReadyJsonFailureResult(
                    readyJsonPayloadSize.ErrorCode,
                    readyJsonPayloadSize.ErrorMessage,
                    endpointPath,
                    package?.SourceKind ?? "UNKNOWN"));
        }

        var ready = ParseReadyJson(readyJson, endpointPath);
        if (!ready.IsSuccess)
        {
            return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Ok(
                BuildBoundedReadyJsonFailureResult(
                    ready.ErrorCode,
                    ready.ErrorMessage,
                    endpointPath,
                    package?.SourceKind ?? "UNKNOWN"));
        }

        var readyRoute = ready.Value!;
        var callerHeaders = ValidateCallerOutboundHeaders(routeAdmissionToken, idempotencyKey);
        if (!callerHeaders.IsSuccess)
            return callerHeaders.MapError<DesignConversionModelServerReadyJsonSubmitResult>();

        var localArtifactUploadRequired = package is not null &&
                                          (package.Screenshots ?? [])
                                              .Concat(package.DesignRenders ?? [])
                                              .Any(IsLocalArtifactReferenceCandidate);
        var uploadedPackage = await TryUploadLocalArtifactsForEvaluateAsync(
            package,
            readyRoute,
            callerHeaders.Value!,
            cancellationToken);
        if (!uploadedPackage.IsSuccess)
        {
            return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Ok(
                BuildBoundedArtifactUploadFailureResult(
                    uploadedPackage.ErrorCode,
                    uploadedPackage.ErrorMessage,
                    readyRoute,
                    package?.SourceKind ?? "UNKNOWN"));
        }

        var apiRequest = ToApiRequest(
            uploadedPackage.Value,
            allowValidatedServerOwnedLocalFileRefs: localArtifactUploadRequired);
        if (!apiRequest.IsSuccess)
            return apiRequest.MapError<DesignConversionModelServerReadyJsonSubmitResult>();

        var requestUri = BuildRequestUri(readyRoute.BaseUrl, readyRoute.EndpointPath);
        var bodyJson = JsonSerializer.Serialize(apiRequest.Value!, DesignConversionModelServerHttpHandler.JsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("x-design-conversion-ready-json-submit-client", ClientBoundary);
        request.Headers.TryAddWithoutValidation("x-design-conversion-ready-json-base-url-configured", bool.TrueString);
        request.Headers.TryAddWithoutValidation(ReadyJsonSubmitStatusHeaderName, ConfiguredSubmitStatus);
        ApplyCallerEvaluateHeaders(request, callerHeaders.Value!);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (Exception ex) when (IsCallerCancellation(ex, cancellationToken))
        {
            return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Fail(
                SubmitCancelledMessage,
                SubmitCancelledErrorCode);
        }
        catch (Exception ex) when (IsSubmitTimeoutCancellation(ex, cancellationToken))
        {
            return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Fail(
                SubmitTimeoutMessage,
                SubmitTimeoutErrorCode);
        }
        catch (Exception ex) when (IsExpectedSubmitTransportException(ex))
        {
            return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Fail(
                SubmitHttpFailedMessage,
                SubmitHttpFailedErrorCode);
        }

        using (response)
        {
            var responseBody = await ReadBoundedResponseBodyAsync(response.Content, cancellationToken);
            if (!responseBody.IsSuccess)
            {
                return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Fail(
                    responseBody.ErrorMessage ?? SubmitResponseReadFailedMessage,
                    responseBody.ErrorCode ?? SubmitResponseReadFailedErrorCode);
            }

            var modelServerResponse = new DesignConversionModelServerHttpResponse(
                StatusCode: (int)response.StatusCode,
                ContentType: response.Content.Headers.ContentType?.ToString() ?? "application/json; charset=utf-8",
                BodyJson: responseBody.Value!,
                Headers: ReadHeaders(response));
            var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
                modelServerResponse,
                package?.SourceKind ?? "UNKNOWN",
                registryInvokeSucceeded: true);
            envelope = MarkConfiguredReadyJsonSubmit(envelope, readyRoute, requestUri);

            return OperationResult<DesignConversionModelServerReadyJsonSubmitResult>.Ok(
                new DesignConversionModelServerReadyJsonSubmitResult(
                    ClientBoundary: ClientBoundary,
                    BaseUrl: readyRoute.BaseUrl,
                    EndpointPath: readyRoute.EndpointPath,
                    RequestUri: requestUri,
                    ReadyJsonBaseUrlConfigured: true,
                    ReadyJsonBaseUrlMissing: false,
                    ReadyJsonBaseUrlLoopback: readyRoute.BaseUrlLoopback,
                    ReadyJsonAdvertisedSubmitRoute: readyRoute.SubmitRouteAdvertised,
                    ProductReady: false,
                    Envelope: envelope));
        }
    }

    private static DesignConversionModelServerLocalCallerAdapterResponse MarkConfiguredReadyJsonSubmit(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        ReadyJsonRoute ready,
        Uri requestUri)
    {
        var metadata = new Dictionary<string, string>(envelope.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["ready_json_submit_client_boundary"] = ClientBoundary,
            ["ready_json_base_url_source"] = "ready.json",
            ["ready_json_base_url"] = ready.BaseUrl.GetLeftPart(UriPartial.Authority),
            ["ready_json_base_url_configured"] = bool.TrueString,
            ["ready_json_base_url_missing"] = bool.FalseString,
            ["ready_json_base_url_loopback"] = ready.BaseUrlLoopback.ToString(),
            ["ready_json_submit_route_advertised"] = ready.SubmitRouteAdvertised.ToString(),
            ["ready_json_submit_endpoint_path"] = ready.EndpointPath,
            ["ready_json_submit_request_uri"] = requestUri.ToString(),
            ["ready_json_submit_http_request_sent"] = bool.TrueString,
            ["ready_json_submit_http_method"] = HttpMethod.Post.Method,
            ["ready_json_submit_status"] = ConfiguredSubmitStatus,
            ["default_submit_base_url_configured"] = bool.TrueString,
            ["default_submit_base_url_missing"] = bool.FalseString,
            ["default_submit_base_url_status"] = ConfiguredSubmitStatus,
            ["default_submit_base_url_missing_setting"] = string.Empty,
            ["flow47_verified"] = bool.FalseString,
            ["flow47_verification_authority"] = DesignConversionModelServerCallerContract.Flow47VerificationAuthority,
            ["flow47_verification_state"] = Flow47VerificationStateNotEvaluatedInReadyJson,
            ["flow47_submit_status"] = Flow47PublicSubmitNotEvaluatedStatus,
            ["flow47_missing_setting_names"] = string.Empty,
            ["flow47_missing_proof_name"] = Flow47MissingProofName,
            ["flow47_next_action"] = Flow47PublicSubmitNextAction,
            ["flow47_base_url_source"] = string.Empty,
            ["flow47_endpoint_path"] = string.Empty,
            ["flow47_submit_request_uri"] = string.Empty,
            ["flow47_live_local_loopback_verified"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        return envelope with { BoundaryMetadata = metadata };
    }

    private static OperationResult<ReadyJsonRoute> ParseReadyJson(string? readyJson, string endpointPath)
    {
        if (string.IsNullOrWhiteSpace(readyJson))
        {
            return OperationResult<ReadyJsonRoute>.Fail(
                "DesignConversion live-local ready.json content is required.",
                "READY_JSON_REQUIRED");
        }

        var normalizedEndpointPath = NormalizeEndpointPath(endpointPath);
        if (string.IsNullOrWhiteSpace(normalizedEndpointPath))
        {
            return OperationResult<ReadyJsonRoute>.Fail(
                "DesignConversion live-local submit endpoint path is required.",
                "READY_JSON_SUBMIT_ENDPOINT_PATH_REQUIRED");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(readyJson);
        }
        catch (JsonException)
        {
            return OperationResult<ReadyJsonRoute>.Fail(
                "DesignConversion live-local ready.json content is not valid JSON.",
                "READY_JSON_INVALID");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return OperationResult<ReadyJsonRoute>.Fail(
                    "DesignConversion live-local ready.json root must be an object.",
                    "READY_JSON_OBJECT_REQUIRED");
            }

            if (!root.TryGetProperty("baseUrl", out var baseUrlProperty) ||
                (baseUrlProperty.ValueKind == JsonValueKind.String &&
                 string.IsNullOrWhiteSpace(baseUrlProperty.GetString())))
            {
                return OperationResult<ReadyJsonRoute>.Fail(
                    "DesignConversion live-local ready.json baseUrl is required.",
                    "READY_JSON_BASE_URL_REQUIRED");
            }

            if (baseUrlProperty.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(baseUrlProperty.GetString()))
            {
                return OperationResult<ReadyJsonRoute>.Fail(
                    "DesignConversion live-local ready.json baseUrl must be a string.",
                    "READY_JSON_BASE_URL_INVALID");
            }

            if (!Uri.TryCreate(baseUrlProperty.GetString()!.Trim(), UriKind.Absolute, out var baseUrl) ||
                (baseUrl.Scheme != Uri.UriSchemeHttp && baseUrl.Scheme != Uri.UriSchemeHttps))
            {
                return OperationResult<ReadyJsonRoute>.Fail(
                    "DesignConversion live-local ready.json baseUrl must be an absolute http or https URL.",
                    "READY_JSON_BASE_URL_INVALID");
            }

            var loopback = IsLoopback(baseUrl.Host);
            if (!loopback)
            {
                return OperationResult<ReadyJsonRoute>.Fail(
                    "DesignConversion live-local ready.json baseUrl must target a loopback host.",
                    "READY_JSON_BASE_URL_NOT_LOOPBACK");
            }

            var routePaths = ReadRoutePaths(root);
            var routeAdvertised = routePaths.Count == 0 ||
                                   routePaths.Contains(normalizedEndpointPath, StringComparer.Ordinal);
            if (!routeAdvertised)
            {
                return OperationResult<ReadyJsonRoute>.Fail(
                    $"DesignConversion live-local ready.json routePaths does not advertise {normalizedEndpointPath}.",
                    "READY_JSON_SUBMIT_ROUTE_NOT_ADVERTISED");
            }

            var artifactUpload = ParseArtifactUploadRoute(root, routePaths);
            return OperationResult<ReadyJsonRoute>.Ok(
                new ReadyJsonRoute(
                    baseUrl,
                    normalizedEndpointPath,
                    loopback,
                    routeAdvertised,
                    artifactUpload));
        }
    }

    private static ReadyJsonArtifactUploadRoute ParseArtifactUploadRoute(
        JsonElement root,
        IReadOnlyList<string> routePaths)
    {
        var configured = root.TryGetProperty("artifactUploadConfigured", out var configuredProperty) &&
                         configuredProperty.ValueKind is JsonValueKind.True;
        var routeMapped = root.TryGetProperty("artifactUploadRouteMapped", out var mappedProperty) &&
                          mappedProperty.ValueKind is JsonValueKind.True;
        var endpointPath = root.TryGetProperty("artifactUploadEndpointPath", out var endpointProperty) &&
                           endpointProperty.ValueKind is JsonValueKind.String
            ? NormalizeEndpointPath(endpointProperty.GetString())
            : string.Empty;
        var routeAdvertised = !string.IsNullOrWhiteSpace(endpointPath) &&
                              routePaths.Contains(endpointPath, StringComparer.Ordinal);
        var maxBytes = root.TryGetProperty("artifactUploadMaxBytes", out var maxBytesProperty) &&
                       maxBytesProperty.ValueKind is JsonValueKind.Number &&
                       maxBytesProperty.TryGetInt64(out var parsedMaxBytes)
            ? parsedMaxBytes
            : 0L;
        var ready = configured &&
                    routeMapped &&
                    routeAdvertised &&
                    string.Equals(
                        endpointPath,
                        DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                        StringComparison.Ordinal) &&
                    maxBytes > 0 &&
                    maxBytes <= MaxReadyJsonArtifactUploadBytes;

        return new ReadyJsonArtifactUploadRoute(
            endpointPath,
            configured,
            routeMapped,
            routeAdvertised,
            maxBytes,
            ready);
    }

    private async Task<OperationResult<DesignConversionModelServerLocalCallerPackage?>>
        TryUploadLocalArtifactsForEvaluateAsync(
            DesignConversionModelServerLocalCallerPackage? package,
            ReadyJsonRoute ready,
            ReadyJsonCallerOutboundHeaders callerHeaders,
            CancellationToken cancellationToken)
    {
        if (package is null)
            return OperationResult<DesignConversionModelServerLocalCallerPackage?>.Ok(package);

        var artifacts = (package.Screenshots ?? [])
            .Concat(package.DesignRenders ?? [])
            .ToArray();
        if (!artifacts.Any(IsLocalArtifactReferenceCandidate))
            return OperationResult<DesignConversionModelServerLocalCallerPackage?>.Ok(package);

        if (!ready.ArtifactUpload.Ready)
        {
            return OperationResult<DesignConversionModelServerLocalCallerPackage?>.Fail(
                ArtifactUploadNotReadyMessage,
                ArtifactUploadNotReadyErrorCode);
        }

        var screenshots = new List<DesignConversionModelServerArtifactRef>();
        var designRenders = new List<DesignConversionModelServerArtifactRef>();
        var ordinal = 0;
        foreach (var artifact in package.Screenshots ?? [])
        {
            var uploaded = await TryUploadLocalArtifactAsync(
                artifact,
                ready,
                callerHeaders,
                ordinal++,
                cancellationToken);
            if (!uploaded.IsSuccess)
                return uploaded.MapError<DesignConversionModelServerLocalCallerPackage?>();
            screenshots.Add(uploaded.Value!);
        }

        foreach (var artifact in package.DesignRenders ?? [])
        {
            var uploaded = await TryUploadLocalArtifactAsync(
                artifact,
                ready,
                callerHeaders,
                ordinal++,
                cancellationToken);
            if (!uploaded.IsSuccess)
                return uploaded.MapError<DesignConversionModelServerLocalCallerPackage?>();
            designRenders.Add(uploaded.Value!);
        }

        return OperationResult<DesignConversionModelServerLocalCallerPackage?>.Ok(
            package with
            {
                Screenshots = package.Screenshots is null ? null : screenshots,
                DesignRenders = package.DesignRenders is null ? null : designRenders
            });
    }

    private async Task<OperationResult<DesignConversionModelServerArtifactRef>> TryUploadLocalArtifactAsync(
        DesignConversionModelServerArtifactRef artifact,
        ReadyJsonRoute ready,
        ReadyJsonCallerOutboundHeaders callerHeaders,
        int ordinal,
        CancellationToken cancellationToken)
    {
        var localPath = ResolveLocalArtifactPath(artifact.Uri);
        if (localPath.Kind is LocalArtifactPathKind.NotLocal)
            return OperationResult<DesignConversionModelServerArtifactRef>.Ok(artifact);
        if (localPath.Kind is LocalArtifactPathKind.InvalidLocal)
            return InvalidLocalArtifact<DesignConversionModelServerArtifactRef>();

        var png = await ReadBoundedLocalPngAsync(
            localPath.Path!,
            artifact,
            ready.ArtifactUpload.MaxBytes,
            cancellationToken);
        if (!png.IsSuccess)
            return png.MapError<DesignConversionModelServerArtifactRef>();

        if (!IsSafeLogicalHeaderValue(artifact.ArtifactId) ||
            !IsSafeLogicalHeaderValue(artifact.Kind))
        {
            return InvalidLocalArtifact<DesignConversionModelServerArtifactRef>();
        }

        var uploadUri = BuildRequestUri(ready.BaseUrl, ready.ArtifactUpload.EndpointPath);
        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUri)
        {
            Content = new ByteArrayContent(png.Value!.Bytes)
        };
        request.Content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName,
            artifact.ArtifactId);
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName,
            artifact.Kind);
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName,
            png.Value.Sha256);
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName,
            artifact.Width.ToString(CultureInfo.InvariantCulture));
        request.Headers.TryAddWithoutValidation(
            DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName,
            artifact.Height.ToString(CultureInfo.InvariantCulture));
        request.Headers.Add(
            DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
            BuildArtifactUploadIdempotencyKey(
                callerHeaders.IdempotencyKey,
                artifact,
                png.Value.Sha256,
                ordinal));
        ApplyCallerAuthorizationHeader(request, callerHeaders);

        if (!TryAddOptionalUploadMetadataHeaders(request, artifact.Metadata))
            return InvalidLocalArtifact<DesignConversionModelServerArtifactRef>();

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (Exception ex) when (IsCallerCancellation(ex, cancellationToken))
        {
            return OperationResult<DesignConversionModelServerArtifactRef>.Fail(
                SubmitCancelledMessage,
                SubmitCancelledErrorCode);
        }
        catch (Exception ex) when (IsSubmitTimeoutCancellation(ex, cancellationToken))
        {
            return OperationResult<DesignConversionModelServerArtifactRef>.Fail(
                SubmitTimeoutMessage,
                SubmitTimeoutErrorCode);
        }
        catch (Exception ex) when (IsExpectedSubmitTransportException(ex))
        {
            return OperationResult<DesignConversionModelServerArtifactRef>.Fail(
                ArtifactUploadHttpFailedMessage,
                ArtifactUploadHttpFailedErrorCode);
        }

        using (response)
        {
            var body = await ReadBoundedResponseBodyAsync(
                response.Content,
                cancellationToken,
                classifyTimeout: true);
            if (!body.IsSuccess)
            {
                if (string.Equals(body.ErrorCode, SubmitCancelledErrorCode, StringComparison.Ordinal) ||
                    string.Equals(body.ErrorCode, SubmitTimeoutErrorCode, StringComparison.Ordinal))
                {
                    return body.MapError<DesignConversionModelServerArtifactRef>();
                }

                return InvalidArtifactUploadResponse<DesignConversionModelServerArtifactRef>();
            }

            if (response.StatusCode is not HttpStatusCode.Created ||
                !string.Equals(
                    response.Content.Headers.ContentType?.MediaType,
                    "application/json",
                    StringComparison.OrdinalIgnoreCase))
            {
                return InvalidArtifactUploadResponse<DesignConversionModelServerArtifactRef>();
            }

            DesignConversionModelServerArtifactUploadHttpSuccessResponse? receipt;
            try
            {
                receipt = JsonSerializer.Deserialize<DesignConversionModelServerArtifactUploadHttpSuccessResponse>(
                    body.Value!,
                    DesignConversionModelServerHttpHandler.JsonOptions);
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
                return InvalidArtifactUploadResponse<DesignConversionModelServerArtifactRef>();
            }

            var uploadedArtifact = ValidateArtifactUploadReceipt(receipt, artifact, png.Value);
            if (!uploadedArtifact.IsSuccess)
                return uploadedArtifact;

            return uploadedArtifact;
        }
    }

    private static async Task<OperationResult<LocalPngUploadSource>> ReadBoundedLocalPngAsync(
        string path,
        DesignConversionModelServerArtifactRef artifact,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<LocalPngUploadSource>.Fail(
                SubmitCancelledMessage,
                SubmitCancelledErrorCode);
        }

        try
        {
            var file = new FileInfo(path);
            file.Refresh();
            if (!file.Exists ||
                file.Attributes.HasFlag(FileAttributes.Directory) ||
                file.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                return InvalidLocalArtifact<LocalPngUploadSource>();
            }

            if (file.Length <= 0)
                return InvalidLocalArtifact<LocalPngUploadSource>();
            if (file.Length > maxBytes)
                return LocalArtifactTooLarge<LocalPngUploadSource>();

            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var memory = new MemoryStream();
            var buffer = new byte[8192];
            while (true)
            {
                var remainingBeforeTooLarge = Math.Min(buffer.LongLength, (maxBytes + 1L) - memory.Length);
                if (remainingBeforeTooLarge <= 0)
                    return LocalArtifactTooLarge<LocalPngUploadSource>();

                var read = await stream.ReadAsync(
                    buffer.AsMemory(0, checked((int)remainingBeforeTooLarge)),
                    cancellationToken);
                if (read == 0)
                    break;
                memory.Write(buffer, 0, read);
            }

            if (memory.Length <= 0)
                return InvalidLocalArtifact<LocalPngUploadSource>();
            if (memory.Length > maxBytes)
                return LocalArtifactTooLarge<LocalPngUploadSource>();

            var bytes = memory.ToArray();
            var sha256 = "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            if (!IsCanonicalLowercaseSha256(artifact.Sha256) ||
                !string.Equals(artifact.Sha256, sha256, StringComparison.Ordinal))
            {
                return OperationResult<LocalPngUploadSource>.Fail(
                    ArtifactUploadSha256MismatchMessage,
                    ArtifactUploadSha256MismatchErrorCode);
            }

            if (!TryReadPngDimensions(bytes, out var pngWidth, out var pngHeight) ||
                !TryGetDeclaredPngDimensions(artifact, out var declaredPngWidth, out var declaredPngHeight) ||
                pngWidth != declaredPngWidth ||
                pngHeight != declaredPngHeight)
            {
                return InvalidLocalArtifact<LocalPngUploadSource>();
            }

            return OperationResult<LocalPngUploadSource>.Ok(
                new LocalPngUploadSource(bytes, sha256, pngWidth, pngHeight));
        }
        catch (Exception ex) when (IsCallerCancellation(ex, cancellationToken))
        {
            return OperationResult<LocalPngUploadSource>.Fail(
                SubmitCancelledMessage,
                SubmitCancelledErrorCode);
        }
        catch (Exception ex) when (IsExpectedLocalArtifactReadException(ex))
        {
            return InvalidLocalArtifact<LocalPngUploadSource>();
        }
    }

    private static bool TryGetDeclaredPngDimensions(
        DesignConversionModelServerArtifactRef artifact,
        out int width,
        out int height)
    {
        width = artifact.Width;
        height = artifact.Height;
        if (width <= 0 || height <= 0)
            return false;

        if (!TryReadSingleMetadataValue(
                artifact.Metadata,
                DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey,
                out var widthValue,
                out var hasWidth) ||
            !TryReadSingleMetadataValue(
                artifact.Metadata,
                DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey,
                out var heightValue,
                out var hasHeight) ||
            hasWidth != hasHeight)
        {
            return false;
        }

        if (!hasWidth)
            return true;

        return TryParseCanonicalPositiveInt(widthValue, out width) &&
               TryParseCanonicalPositiveInt(heightValue, out height);
    }

    private static bool TryReadPngDimensions(byte[] bytes, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (bytes.Length < 24 ||
            bytes[0] != 0x89 ||
            bytes[1] != 0x50 ||
            bytes[2] != 0x4E ||
            bytes[3] != 0x47 ||
            bytes[4] != 0x0D ||
            bytes[5] != 0x0A ||
            bytes[6] != 0x1A ||
            bytes[7] != 0x0A ||
            bytes[8] != 0x00 ||
            bytes[9] != 0x00 ||
            bytes[10] != 0x00 ||
            bytes[11] != 0x0D ||
            bytes[12] != 0x49 ||
            bytes[13] != 0x48 ||
            bytes[14] != 0x44 ||
            bytes[15] != 0x52)
        {
            return false;
        }

        var parsedWidth = ReadBigEndianUInt32(bytes, 16);
        var parsedHeight = ReadBigEndianUInt32(bytes, 20);
        if (parsedWidth is 0 or > int.MaxValue || parsedHeight is 0 or > int.MaxValue)
            return false;

        width = (int)parsedWidth;
        height = (int)parsedHeight;
        return true;
    }

    private static uint ReadBigEndianUInt32(byte[] bytes, int offset)
        => ((uint)bytes[offset] << 24) |
           ((uint)bytes[offset + 1] << 16) |
           ((uint)bytes[offset + 2] << 8) |
           bytes[offset + 3];

    private static OperationResult<DesignConversionModelServerArtifactRef> ValidateArtifactUploadReceipt(
        DesignConversionModelServerArtifactUploadHttpSuccessResponse? receipt,
        DesignConversionModelServerArtifactRef requested,
        LocalPngUploadSource source)
    {
        var receiptArtifactRef = receipt?.ArtifactRef;
        var coreReceiptValid = receipt is not null &&
                               receiptArtifactRef is not null &&
                               receipt.StatusCode == 201 &&
                               string.Equals(
                                   receipt.Endpoint,
                                   DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                                   StringComparison.Ordinal) &&
                               string.Equals(receipt.Method, "POST", StringComparison.Ordinal) &&
                               string.Equals(receipt.MediaType, "image/png", StringComparison.OrdinalIgnoreCase) &&
                               receipt.ByteCount == source.Bytes.LongLength &&
                               string.Equals(receipt.ArtifactId, requested.ArtifactId, StringComparison.Ordinal) &&
                               string.Equals(receipt.Kind, requested.Kind, StringComparison.Ordinal) &&
                               string.Equals(receipt.Sha256, source.Sha256, StringComparison.Ordinal) &&
                               receipt.Width == requested.Width &&
                               receipt.Height == requested.Height &&
                               !receipt.RawBytesIncluded &&
                               string.Equals(receiptArtifactRef.ArtifactId, requested.ArtifactId, StringComparison.Ordinal) &&
                               string.Equals(receiptArtifactRef.Kind, requested.Kind, StringComparison.Ordinal) &&
                               string.Equals(receiptArtifactRef.Sha256, source.Sha256, StringComparison.Ordinal) &&
                               receiptArtifactRef.Width == requested.Width &&
                               receiptArtifactRef.Height == requested.Height;
        if (!coreReceiptValid)
        {
            return OperationResult<DesignConversionModelServerArtifactRef>.Fail(
                ArtifactUploadResponseInvalidMessage,
                ArtifactUploadResponseInvalidErrorCode);
        }

        if (!IsCanonicalServerOwnedArtifactUri(receiptArtifactRef!.Uri))
        {
            return OperationResult<DesignConversionModelServerArtifactRef>.Fail(
                ArtifactUploadResponseInvalidMessage,
                ArtifactUploadResponseInvalidErrorCode);
        }

        if (!TryBuildResolverSafeReceiptMetadata(
                receiptArtifactRef.Metadata,
                requested.Metadata,
                requested.Width,
                requested.Height,
                source.Bytes.LongLength,
                out var safeMetadata))
        {
            return OperationResult<DesignConversionModelServerArtifactRef>.Fail(
                ArtifactUploadResponseInvalidMessage,
                ArtifactUploadResponseInvalidErrorCode);
        }

        return OperationResult<DesignConversionModelServerArtifactRef>.Ok(
            receiptArtifactRef with { Metadata = safeMetadata });
    }

    private static bool IsCanonicalServerOwnedArtifactUri(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            if (ContainsTraversalSegment(value) ||
                ContainsTraversalSegment(Uri.UnescapeDataString(value)) ||
                !Uri.TryCreate(value, UriKind.Absolute, out var fileUri) ||
                !fileUri.IsFile ||
                fileUri.IsUnc ||
                !string.IsNullOrEmpty(fileUri.Host) ||
                !string.IsNullOrEmpty(fileUri.UserInfo) ||
                !string.IsNullOrEmpty(fileUri.Query) ||
                !string.IsNullOrEmpty(fileUri.Fragment))
            {
                return false;
            }

            var localPath = fileUri.LocalPath;
            if (IsUncOrDevicePath(localPath) ||
                !Path.IsPathFullyQualified(localPath) ||
                !string.Equals(Path.GetExtension(localPath), ".png", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var fullPath = Path.GetFullPath(localPath);
            return Path.IsPathFullyQualified(fullPath) &&
                   !IsUncOrDevicePath(fullPath) &&
                   string.Equals(new Uri(fullPath).AbsoluteUri, value, StringComparison.Ordinal);
        }
        catch (Exception ex) when (ex is ArgumentException
                                   or IOException
                                   or NotSupportedException
                                   or UnauthorizedAccessException
                                   or UriFormatException)
        {
            return false;
        }
    }

    private static bool TryBuildResolverSafeReceiptMetadata(
        IReadOnlyDictionary<string, string>? receiptMetadata,
        IReadOnlyDictionary<string, string>? requestedMetadata,
        int width,
        int height,
        long byteCount,
        out IReadOnlyDictionary<string, string> safeMetadata)
    {
        safeMetadata = new Dictionary<string, string>();
        if (receiptMetadata is null || receiptMetadata.Count > 9)
            return false;

        var allowedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "media_type",
            "byte_count",
            "ingress_boundary",
            DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey,
            DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey,
            DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey,
            DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey,
            DesignConversionModelServerArtifactUploadStore.DeviceScaleFactorMetadataKey,
            DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey
        };
        var observedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in receiptMetadata)
        {
            if (!allowedKeys.Contains(pair.Key) ||
                !observedKeys.Add(pair.Key) ||
                !IsSafeHeaderValue(pair.Value, maxLength: 128))
            {
                return false;
            }
        }

        if (!TryBuildCanonicalRequestedUploadMetadata(
                requestedMetadata,
                width,
                height,
                out var canonicalRequestedMetadata))
        {
            return false;
        }

        if (!TryReadSingleMetadataValue(receiptMetadata, "media_type", out var mediaType, out var hasMediaType) ||
            !hasMediaType ||
            !string.Equals(mediaType, "image/png", StringComparison.Ordinal))
        {
            return false;
        }

        if (!TryReadSingleMetadataValue(receiptMetadata, "ingress_boundary", out var ingressBoundary, out var hasIngressBoundary) ||
            !hasIngressBoundary ||
            !string.Equals(
                ingressBoundary,
                DesignConversionServerOwnedLocalArtifactRefResolver.Boundary,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (!TryReadSingleMetadataValue(receiptMetadata, "byte_count", out var receiptByteCount, out var hasByteCount) ||
            (hasByteCount && !string.Equals(
                receiptByteCount,
                byteCount.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal)))
        {
            return false;
        }

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["media_type"] = "image/png",
            ["byte_count"] = byteCount.ToString(CultureInfo.InvariantCulture),
            ["ingress_boundary"] = DesignConversionServerOwnedLocalArtifactRefResolver.Boundary
        };
        var resolverMetadataKeys = new[]
        {
            DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey,
            DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey,
            DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey,
            DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey,
            DesignConversionModelServerArtifactUploadStore.DeviceScaleFactorMetadataKey
        };
        foreach (var key in resolverMetadataKeys)
        {
            var requestedReadable = TryReadSingleMetadataValue(
                canonicalRequestedMetadata,
                key,
                out var requestedValue,
                out var requested);
            var receiptReadable = TryReadSingleMetadataValue(receiptMetadata, key, out var receiptValue, out var received);
            if (!requestedReadable ||
                !receiptReadable ||
                (received && (!requested || !string.Equals(requestedValue, receiptValue, StringComparison.Ordinal))))
            {
                return false;
            }

            if (requested)
                metadata[key] = requestedValue;
        }

        var requestedViewportIdReadable = TryReadSingleMetadataValue(
            canonicalRequestedMetadata,
            DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey,
            out var requestedViewportId,
            out var hasRequestedViewportId);
        var receiptViewportIdReadable = TryReadSingleMetadataValue(
            receiptMetadata,
            DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey,
            out var receiptViewportId,
            out var hasReceiptViewportId);
        if (!requestedViewportIdReadable ||
            !receiptViewportIdReadable ||
            (hasReceiptViewportId &&
             (!hasRequestedViewportId ||
              !string.Equals(
                requestedViewportId,
                receiptViewportId,
                StringComparison.Ordinal))))
        {
            return false;
        }

        safeMetadata = metadata;
        return true;
    }

    private static bool TryBuildCanonicalRequestedUploadMetadata(
        IReadOnlyDictionary<string, string>? requestedMetadata,
        int width,
        int height,
        out IReadOnlyDictionary<string, string> canonicalMetadata)
    {
        canonicalMetadata = new Dictionary<string, string>();
        var uploadMetadata = new Dictionary<string, string>(StringComparer.Ordinal);
        var keys = new[]
        {
            DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey,
            DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey,
            DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey,
            DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey,
            DesignConversionModelServerArtifactUploadStore.DeviceScaleFactorMetadataKey,
            DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey
        };

        foreach (var key in keys)
        {
            if (!TryReadSingleMetadataValue(requestedMetadata, key, out var value, out var found))
                return false;
            if (!found)
                continue;
            if (!IsSafeHeaderValue(value, maxLength: 128))
                return false;

            uploadMetadata[key] = value;
        }

        var validated = DesignConversionModelServerArtifactUploadStore.ValidateUploadMetadata(
            uploadMetadata,
            width,
            height);
        if (!validated.IsSuccess)
            return false;

        canonicalMetadata = validated.Value!;
        return true;
    }

    private static bool TryAddOptionalUploadMetadataHeaders(
        HttpRequestMessage request,
        IReadOnlyDictionary<string, string>? metadata)
    {
        return TryAddOptionalUploadMetadataHeader(
                   request,
                   metadata,
                   DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey,
                   DesignConversionModelServerArtifactUploadHttpHandler.ViewportWidthHeaderName) &&
               TryAddOptionalUploadMetadataHeader(
                   request,
                   metadata,
                   DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey,
                   DesignConversionModelServerArtifactUploadHttpHandler.ViewportHeightHeaderName) &&
               TryAddOptionalUploadMetadataHeader(
                   request,
                   metadata,
                   DesignConversionModelServerArtifactUploadStore.PngWidthMetadataKey,
                   DesignConversionModelServerArtifactUploadHttpHandler.PngWidthHeaderName) &&
               TryAddOptionalUploadMetadataHeader(
                   request,
                   metadata,
                   DesignConversionModelServerArtifactUploadStore.PngHeightMetadataKey,
                   DesignConversionModelServerArtifactUploadHttpHandler.PngHeightHeaderName) &&
               TryAddOptionalUploadMetadataHeader(
                   request,
                   metadata,
                   DesignConversionModelServerArtifactUploadStore.DeviceScaleFactorMetadataKey,
                   DesignConversionModelServerArtifactUploadHttpHandler.DeviceScaleFactorHeaderName) &&
               TryAddOptionalUploadMetadataHeader(
                   request,
                   metadata,
                   DesignConversionModelServerArtifactUploadStore.ViewportIdMetadataKey,
                   DesignConversionModelServerArtifactUploadHttpHandler.ViewportIdHeaderName);
    }

    private static bool TryAddOptionalUploadMetadataHeader(
        HttpRequestMessage request,
        IReadOnlyDictionary<string, string>? metadata,
        string metadataKey,
        string headerName)
    {
        if (!TryReadSingleMetadataValue(metadata, metadataKey, out var value, out var found))
            return false;
        if (!found)
            return true;
        if (!IsSafeHeaderValue(value, maxLength: 128))
            return false;

        return request.Headers.TryAddWithoutValidation(headerName, value);
    }

    private static bool TryReadSingleMetadataValue(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        out string value,
        out bool found)
    {
        value = string.Empty;
        found = false;
        if (metadata is null)
            return true;

        foreach (var pair in metadata)
        {
            if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                continue;
            if (found)
                return false;
            found = true;
            value = pair.Value;
        }

        return true;
    }

    private static OperationResult<ReadyJsonCallerOutboundHeaders> ValidateCallerOutboundHeaders(
        string? routeAdmissionToken,
        string? callerIdempotencyKey)
    {
        if (!TryNormalizeCallerRouteAdmissionToken(routeAdmissionToken, out var normalizedToken) ||
            !TryNormalizeCallerIdempotencyKey(callerIdempotencyKey, out var normalizedIdempotencyKey))
        {
            return OperationResult<ReadyJsonCallerOutboundHeaders>.Fail(
                CallerOutboundHeaderInvalidMessage,
                CallerOutboundHeaderInvalidErrorCode);
        }

        return OperationResult<ReadyJsonCallerOutboundHeaders>.Ok(
            new ReadyJsonCallerOutboundHeaders(normalizedToken, normalizedIdempotencyKey));
    }

    private static bool TryNormalizeCallerRouteAdmissionToken(string? value, out string? normalized)
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var trimmed = value.Trim();
        if (trimmed.Length == 0)
            return true;
        if (trimmed.Length > MaxCallerRouteAdmissionTokenChars)
            return false;
        if (!IsBearerToken68(trimmed))
            return false;
        if (!TryCreateBearerAuthorizationHeader(trimmed, out _))
            return false;

        normalized = trimmed;
        return true;
    }

    private static bool TryNormalizeCallerIdempotencyKey(string? value, out string? normalized)
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var trimmed = value.Trim();
        if (trimmed.Length == 0)
            return true;
        if (trimmed.Length > MaxCallerIdempotencyKeyChars ||
            !trimmed.All(IsVisibleAsciiHeaderChar))
        {
            return false;
        }

        normalized = trimmed;
        return true;
    }

    private static void ApplyCallerEvaluateHeaders(
        HttpRequestMessage request,
        ReadyJsonCallerOutboundHeaders callerHeaders)
    {
        ApplyCallerAuthorizationHeader(request, callerHeaders);
        if (callerHeaders.IdempotencyKey is { } idempotencyKey)
        {
            request.Headers.Add(
                DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
                idempotencyKey);
        }
    }

    private static void ApplyCallerAuthorizationHeader(
        HttpRequestMessage request,
        ReadyJsonCallerOutboundHeaders callerHeaders)
    {
        if (callerHeaders.RouteAdmissionToken is { } routeAdmissionToken)
            request.Headers.Authorization = CreateBearerAuthorizationHeader(routeAdmissionToken);
    }

    private static AuthenticationHeaderValue CreateBearerAuthorizationHeader(string token)
    {
        if (!TryCreateBearerAuthorizationHeader(token, out var header) || header is null)
            throw new InvalidOperationException("Validated ready.json route admission token could not be applied.");
        return header;
    }

    private static bool TryCreateBearerAuthorizationHeader(
        string token,
        out AuthenticationHeaderValue? header)
    {
        try
        {
            header = new AuthenticationHeaderValue("Bearer", token);
            return AuthenticationHeaderValue.TryParse(header.ToString(), out var parsed) &&
                   parsed is not null &&
                   string.Equals(parsed.Scheme, "Bearer", StringComparison.Ordinal) &&
                   string.Equals(parsed.Parameter, token, StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            header = null;
            return false;
        }
    }

    private static bool IsBearerToken68(string value)
    {
        var hasTokenCharacter = false;
        var paddingStarted = false;
        foreach (var character in value)
        {
            if (character == '=')
            {
                if (!hasTokenCharacter)
                    return false;

                paddingStarted = true;
                continue;
            }

            if (paddingStarted || !IsBearerToken68Character(character))
                return false;

            hasTokenCharacter = true;
        }

        return hasTokenCharacter;
    }

    private static bool IsBearerToken68Character(char value)
        => (value >= 'A' && value <= 'Z') ||
           (value >= 'a' && value <= 'z') ||
           (value >= '0' && value <= '9') ||
           value is '-' or '.' or '_' or '~' or '+' or '/';

    private static bool IsVisibleAsciiHeaderChar(char value)
        => value is >= '!' and <= '~';

    private static bool TryParseCanonicalPositiveInt(string value, out int parsed)
    {
        parsed = 0;
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out parsed) &&
               parsed > 0 &&
               string.Equals(parsed.ToString(CultureInfo.InvariantCulture), value, StringComparison.Ordinal);
    }

    private static string BuildArtifactUploadIdempotencyKey(
        string? callerIdempotencyKey,
        DesignConversionModelServerArtifactRef artifact,
        string sha256,
        int ordinal)
    {
        var material = string.Join(
            "\n",
            callerIdempotencyKey?.Trim() ?? string.Empty,
            artifact.ArtifactId,
            artifact.Kind,
            sha256,
            ordinal.ToString(CultureInfo.InvariantCulture));
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material))).ToLowerInvariant();
        return "ready-json-artifact-" + digest;
    }

    private static bool IsSafeLogicalHeaderValue(string? value)
        => IsSafeHeaderValue(value, maxLength: 96);

    private static bool IsSafeHeaderValue(string? value, int maxLength)
        => !string.IsNullOrEmpty(value) &&
           value.Length <= maxLength &&
           string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
           !value.Contains(',', StringComparison.Ordinal) &&
           !value.Any(char.IsControl);

    private static bool IsCanonicalLowercaseSha256(string? value)
    {
        if (value is null ||
            value.Length != 71 ||
            !value.StartsWith("sha256:", StringComparison.Ordinal))
        {
            return false;
        }

        for (var index = 7; index < value.Length; index++)
        {
            var character = value[index];
            if (!char.IsAsciiDigit(character) && character is not (>= 'a' and <= 'f'))
                return false;
        }

        return true;
    }

    private static bool IsLocalArtifactReferenceCandidate(DesignConversionModelServerArtifactRef artifact)
    {
        var value = artifact.Uri;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        if (value.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return true;

        try
        {
            return Path.IsPathFullyQualified(value);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            return false;
        }
    }

    private static LocalArtifactPathResolution ResolveLocalArtifactPath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return LocalArtifactPathResolution.NotLocal();

        try
        {
            string path;
            if (value.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                    !uri.IsFile ||
                    uri.IsUnc ||
                    !string.IsNullOrEmpty(uri.Host) ||
                    !string.IsNullOrEmpty(uri.Query) ||
                    !string.IsNullOrEmpty(uri.Fragment) ||
                    ContainsTraversalSegment(Uri.UnescapeDataString(uri.OriginalString)))
                {
                    return LocalArtifactPathResolution.Invalid();
                }

                path = uri.LocalPath;
            }
            else
            {
                if (!Path.IsPathFullyQualified(value))
                    return LocalArtifactPathResolution.NotLocal();
                if (ContainsTraversalSegment(Uri.UnescapeDataString(value)))
                    return LocalArtifactPathResolution.Invalid();
                path = value;
            }

            if (IsUncOrDevicePath(path) ||
                !Path.IsPathFullyQualified(path) ||
                !string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase))
            {
                return LocalArtifactPathResolution.Invalid();
            }

            var fullPath = Path.GetFullPath(path);
            if (IsUncOrDevicePath(fullPath) || !Path.IsPathFullyQualified(fullPath))
                return LocalArtifactPathResolution.Invalid();

            return LocalArtifactPathResolution.Local(fullPath);
        }
        catch (Exception ex) when (ex is ArgumentException
                                   or IOException
                                   or NotSupportedException
                                   or UnauthorizedAccessException
                                   or UriFormatException)
        {
            return LocalArtifactPathResolution.Invalid();
        }
    }

    private static bool ContainsTraversalSegment(string value)
        => value.Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => segment is "." or "..");

    private static bool IsUncOrDevicePath(string path)
        => path.StartsWith(@"\\", StringComparison.Ordinal) ||
           path.StartsWith("//", StringComparison.Ordinal) ||
           path.StartsWith(@"\\?\", StringComparison.Ordinal) ||
           path.StartsWith(@"\\.\", StringComparison.Ordinal);

    private static bool IsExpectedLocalArtifactReadException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or ArgumentException
            or ObjectDisposedException
            or System.Security.SecurityException;

    private static OperationResult<T> InvalidLocalArtifact<T>()
        => OperationResult<T>.Fail(ArtifactUploadFileInvalidMessage, ArtifactUploadFileInvalidErrorCode);

    private static OperationResult<T> LocalArtifactTooLarge<T>()
        => OperationResult<T>.Fail(ArtifactUploadTooLargeMessage, ArtifactUploadTooLargeErrorCode);

    private static OperationResult<T> InvalidArtifactUploadResponse<T>()
        => OperationResult<T>.Fail(
            ArtifactUploadResponseInvalidMessage,
            ArtifactUploadResponseInvalidErrorCode);

    private static DesignConversionModelServerReadyJsonSubmitResult BuildBoundedArtifactUploadFailureResult(
        string? errorCode,
        string? errorMessage,
        ReadyJsonRoute ready,
        string callerSourceKind)
    {
        var code = string.IsNullOrWhiteSpace(errorCode)
            ? ArtifactUploadResponseInvalidErrorCode
            : errorCode;
        var message = string.IsNullOrWhiteSpace(errorMessage)
            ? ArtifactUploadResponseInvalidMessage
            : errorMessage;
        var statusCode = code switch
        {
            ArtifactUploadTooLargeErrorCode => 413,
            SubmitCancelledErrorCode => 499,
            SubmitTimeoutErrorCode => 504,
            ArtifactUploadHttpFailedErrorCode => 502,
            ArtifactUploadResponseInvalidErrorCode => 502,
            _ => 400
        };
        var metadata = new Dictionary<string, string>(
            DesignConversionModelServerCallerContract.BoundaryMetadata(),
            StringComparer.Ordinal)
        {
            ["ready_json_submit_client_boundary"] = ClientBoundary,
            ["ready_json_base_url_source"] = "ready.json",
            ["ready_json_base_url_configured"] = bool.TrueString,
            ["ready_json_base_url_missing"] = bool.FalseString,
            ["ready_json_base_url_loopback"] = ready.BaseUrlLoopback.ToString(),
            ["ready_json_submit_route_advertised"] = ready.SubmitRouteAdvertised.ToString(),
            ["ready_json_submit_endpoint_path"] = ready.EndpointPath,
            ["ready_json_submit_request_uri"] = string.Empty,
            ["ready_json_submit_http_request_sent"] = bool.FalseString,
            ["ready_json_submit_skipped"] = bool.TrueString,
            ["ready_json_submit_status"] = "failed_closed_before_evaluate",
            ["ready_json_artifact_upload_required"] = bool.TrueString,
            ["ready_json_artifact_upload_route_ready"] = ready.ArtifactUpload.Ready.ToString(),
            ["ready_json_artifact_upload_status"] = "failed_closed_before_evaluate",
            ["ready_json_artifact_upload_error_code"] = code,
            ["runtime_invoked"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: ready.EndpointPath,
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            HttpSeam: ClientBoundary,
            StatusCode: statusCode,
            ErrorCode: code,
            ErrorMessage: message,
            BoundaryMetadata: metadata);
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: "application/json; charset=utf-8",
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-ready-json-submit-client"] = ClientBoundary,
                ["x-design-conversion-ready-json-submit-status"] = "failed_closed_before_evaluate",
                ["x-design-conversion-ready-json-http-request-sent"] = bool.FalseString
            });
        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            callerSourceKind,
            registryInvokeSucceeded: false);

        return new DesignConversionModelServerReadyJsonSubmitResult(
            ClientBoundary: ClientBoundary,
            BaseUrl: ready.BaseUrl,
            EndpointPath: ready.EndpointPath,
            RequestUri: null,
            ReadyJsonBaseUrlConfigured: true,
            ReadyJsonBaseUrlMissing: false,
            ReadyJsonBaseUrlLoopback: ready.BaseUrlLoopback,
            ReadyJsonAdvertisedSubmitRoute: ready.SubmitRouteAdvertised,
            ProductReady: false,
            Envelope: envelope);
    }

    private static DesignConversionModelServerReadyJsonSubmitResult BuildBoundedReadyJsonFailureResult(
        string? errorCode,
        string? errorMessage,
        string endpointPath,
        string callerSourceKind)
    {
        var code = string.IsNullOrWhiteSpace(errorCode) ? "READY_JSON_BASE_URL_INVALID" : errorCode;
        var message = string.IsNullOrWhiteSpace(errorMessage)
            ? "DesignConversion live-local ready.json baseUrl is invalid."
            : errorMessage;
        var normalizedEndpointPath = NormalizeEndpointPath(endpointPath);
        var reason = ReadyJsonFailureReason(code);
        var status = ReadyJsonFailureStatus(reason);
        var baseUrlMissing = string.Equals(reason, ReadyJsonBaseUrlMissingReason, StringComparison.Ordinal);
        var metadata = BuildBoundedReadyJsonFailureMetadata(
            code,
            message,
            normalizedEndpointPath,
            reason,
            status,
            baseUrlMissing);
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: normalizedEndpointPath,
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            HttpSeam: ClientBoundary,
            StatusCode: 400,
            ErrorCode: code,
            ErrorMessage: message,
            BoundaryMetadata: metadata);
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: 400,
            ContentType: "application/json; charset=utf-8",
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-ready-json-submit-client"] = ClientBoundary,
                ["x-design-conversion-ready-json-submit-status"] = ReadyJsonSubmitSkippedStatus,
                ["x-design-conversion-ready-json-http-request-sent"] = bool.FalseString
            });
        var envelope = DesignConversionModelServerLocalCallerAdapter.CreateEnvelopeFromHttpResponse(
            response,
            callerSourceKind,
            registryInvokeSucceeded: false);

        return new DesignConversionModelServerReadyJsonSubmitResult(
            ClientBoundary: ClientBoundary,
            BaseUrl: null,
            EndpointPath: normalizedEndpointPath,
            RequestUri: null,
            ReadyJsonBaseUrlConfigured: false,
            ReadyJsonBaseUrlMissing: baseUrlMissing,
            ReadyJsonBaseUrlLoopback: false,
            ReadyJsonAdvertisedSubmitRoute: false,
            ProductReady: false,
            Envelope: envelope);
    }

    private static Dictionary<string, string> BuildBoundedReadyJsonFailureMetadata(
        string errorCode,
        string errorMessage,
        string endpointPath,
        string reason,
        string status,
        bool baseUrlMissing)
    {
        var metadata = new Dictionary<string, string>(
            DesignConversionModelServerCallerContract.BoundaryMetadata(),
            StringComparer.Ordinal)
        {
            ["ready_json_submit_client_boundary"] = ClientBoundary,
            ["ready_json_base_url_source"] = "ready.json",
            ["ready_json_base_url_configured"] = bool.FalseString,
            ["ready_json_base_url_missing"] = baseUrlMissing.ToString(),
            ["ready_json_base_url_loopback"] = bool.FalseString,
            ["ready_json_base_url_reason"] = reason,
            ["ready_json_base_url_status"] = status,
            ["ready_json_error_code"] = errorCode,
            ["ready_json_error_message"] = errorMessage,
            ["ready_json_submit_route_advertised"] = bool.FalseString,
            ["ready_json_submit_endpoint_path"] = endpointPath,
            ["ready_json_submit_request_uri"] = string.Empty,
            ["ready_json_submit_http_request_sent"] = bool.FalseString,
            ["ready_json_submit_skipped"] = bool.TrueString,
            ["ready_json_submit_status"] = ReadyJsonSubmitSkippedStatus,
            ["default_submit_base_url_configured"] = bool.FalseString,
            ["default_submit_base_url_missing"] = baseUrlMissing.ToString(),
            ["default_submit_base_url_status"] = status,
            ["default_submit_base_url_missing_setting"] = baseUrlMissing ? "baseUrl" : string.Empty,
            ["flow47_verification_authority"] = DesignConversionModelServerCallerContract.Flow47VerificationAuthority,
            ["flow47_verification_state"] = Flow47VerificationStateNotEvaluatedInReadyJson,
            ["flow47_submit_status"] = status,
            ["flow47_missing_setting_names"] = baseUrlMissing ? "baseUrl" : string.Empty,
            ["flow47_next_action"] = DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlNextAction,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["runtime_invoked"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString
        };

        return metadata;
    }

    private static bool IsCallerCancellation(Exception ex, CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested && ex is OperationCanceledException;

    private static bool IsSubmitTimeoutCancellation(Exception ex, CancellationToken cancellationToken)
        => !cancellationToken.IsCancellationRequested && ex is OperationCanceledException;

    private static bool IsExpectedSubmitTransportException(Exception ex)
        => ex is HttpRequestException or InvalidOperationException;

    private static bool IsExpectedReadyJsonReadException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or DecoderFallbackException
            or ArgumentException
            or ObjectDisposedException
            or OperationCanceledException;

    private static OperationResult<string> ReadyJsonFileTooLarge()
        => OperationResult<string>.Fail(ReadyJsonFileTooLargeMessage, ReadyJsonFileTooLargeErrorCode);

    private static OperationResult<string> ReadyJsonReadFailed()
        => OperationResult<string>.Fail(ReadyJsonReadFailedMessage, ReadyJsonReadFailedErrorCode);

    private static OperationResult<string> ReadyJsonReadCancelled()
        => OperationResult<string>.Fail(SubmitCancelledMessage, SubmitCancelledErrorCode);

    private static async Task<OperationResult<string>> ReadBoundedReadyJsonFileAsync(
        string readyFilePath,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return ReadyJsonReadCancelled();

        try
        {
            await using var stream = new FileStream(
                readyFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            if (cancellationToken.IsCancellationRequested)
                return ReadyJsonReadCancelled();

            if (stream.CanSeek && stream.Length > MaxReadyJsonFileBytes)
                return ReadyJsonFileTooLarge();

            using var memory = new MemoryStream(capacity: MaxReadyJsonFileBytes + 1);
            var buffer = new byte[8192];

            while (true)
            {
                if (memory.Length > MaxReadyJsonFileBytes)
                    return ReadyJsonFileTooLarge();

                var remainingBeforeTooLarge = (int)Math.Min(
                    buffer.Length,
                    (MaxReadyJsonFileBytes + 1L) - memory.Length);
                if (remainingBeforeTooLarge <= 0)
                    return ReadyJsonFileTooLarge();

                var read = await stream.ReadAsync(
                    buffer.AsMemory(0, remainingBeforeTooLarge),
                    cancellationToken);
                if (read == 0)
                    break;

                memory.Write(buffer, 0, read);
            }

            var length = checked((int)memory.Length);
            var bytes = memory.GetBuffer();
            var offset = HasUtf8Bom(bytes, length) ? 3 : 0;
            return OperationResult<string>.Ok(StrictUtf8.GetString(bytes, offset, length - offset));
        }
        catch (Exception ex) when (IsCallerCancellation(ex, cancellationToken))
        {
            return ReadyJsonReadCancelled();
        }
        catch (Exception ex) when (IsExpectedReadyJsonReadException(ex))
        {
            return ReadyJsonReadFailed();
        }
    }

    private static bool HasUtf8Bom(byte[] bytes, int length)
        => length >= 3 &&
           bytes[0] == 0xEF &&
           bytes[1] == 0xBB &&
           bytes[2] == 0xBF;

    private static bool IsExpectedResponseReadException(Exception ex)
        => ex is HttpRequestException or IOException or ObjectDisposedException or TaskCanceledException;

    private static async Task<OperationResult<string>> ReadBoundedResponseBodyAsync(
        HttpContent content,
        CancellationToken cancellationToken,
        bool classifyTimeout = false)
    {
        if (content.Headers.ContentLength is > MaxSubmitResponseBodyBytes)
        {
            return OperationResult<string>.Fail(
                SubmitResponseReadFailedMessage,
                SubmitResponseTooLargeErrorCode);
        }

        try
        {
            await using var stream = await content.ReadAsStreamAsync(cancellationToken);
            using var memory = new MemoryStream();
            var buffer = new byte[8192];

            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0)
                    break;

                if (memory.Length + read > MaxSubmitResponseBodyBytes)
                {
                    return OperationResult<string>.Fail(
                        SubmitResponseReadFailedMessage,
                        SubmitResponseTooLargeErrorCode);
                }

                memory.Write(buffer, 0, read);
            }

            return OperationResult<string>.Ok(Encoding.UTF8.GetString(memory.ToArray()));
        }
        catch (Exception ex) when (IsCallerCancellation(ex, cancellationToken))
        {
            return OperationResult<string>.Fail(
                SubmitCancelledMessage,
                SubmitCancelledErrorCode);
        }
        catch (Exception ex) when (classifyTimeout && IsSubmitTimeoutCancellation(ex, cancellationToken))
        {
            return OperationResult<string>.Fail(
                SubmitTimeoutMessage,
                SubmitTimeoutErrorCode);
        }
        catch (Exception ex) when (IsExpectedResponseReadException(ex))
        {
            return OperationResult<string>.Fail(
                SubmitResponseReadFailedMessage,
                SubmitResponseReadFailedErrorCode);
        }
    }

    private static string ReadyJsonFailureReason(string errorCode)
        => errorCode switch
        {
            "READY_JSON_BASE_URL_REQUIRED" => ReadyJsonBaseUrlMissingReason,
            "READY_JSON_BASE_URL_INVALID" => ReadyJsonBaseUrlInvalidReason,
            "READY_JSON_BASE_URL_NOT_LOOPBACK" => ReadyJsonBaseUrlInvalidReason,
            _ => "ready_json_invalid"
        };

    private static string ReadyJsonFailureStatus(string reason)
        => reason switch
        {
            ReadyJsonBaseUrlMissingReason => DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
            ReadyJsonBaseUrlInvalidReason => ReadyJsonBaseUrlInvalidStatus,
            _ => ReadyJsonSubmitSkippedStatus
        };

    private static OperationResult<DesignConversionModelServerApiRequest> ToApiRequest(
        DesignConversionModelServerLocalCallerPackage? package,
        bool allowValidatedServerOwnedLocalFileRefs = false)
    {
        if (package is null)
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                "DesignConversion local caller package is required.",
                "LOCAL_CALLER_PACKAGE_REQUIRED");
        }

        if (!Enum.TryParse<DesignConversionSourceKind>(
                package.SourceKind,
                ignoreCase: true,
                out var sourceKind))
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                $"DesignConversion local caller package source kind is not supported: {package.SourceKind}",
                "SOURCE_KIND_UNSUPPORTED");
        }

        if (string.IsNullOrWhiteSpace(package.RequestId))
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                "DesignConversion local caller package request id is required.",
                "REQUEST_ID_REQUIRED");
        }

        if (package.DesignTree is null)
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                "DesignConversion local caller package design tree payload, ref, or hash is required.",
                "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED");
        }

        if (package.OriginalTree is null)
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                "DesignConversion local caller package original tree payload, ref, or hash is required.",
                "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(package.Html))
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                "DesignConversion local caller package generated HTML is required.",
                "DESIGN_CONVERSION_HTML_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(package.Css))
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                "DesignConversion local caller package generated CSS is required.",
                "DESIGN_CONVERSION_CSS_REQUIRED");
        }

        var payloadSize = DesignConversionModelServerPayloadSizeGuard.ValidatePackage(
            package,
            "DesignConversion live-local ready-json package");
        if (!payloadSize.IsSuccess)
            return payloadSize.MapError<DesignConversionModelServerApiRequest>();

        var artifacts = (package.Screenshots ?? [])
            .Concat(package.DesignRenders ?? [])
            .ToArray();
        if (artifacts.Length == 0)
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                "DesignConversion local caller package screenshot or expected render ref is required.",
                "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED");
        }

        if (!artifacts.Any(artifact => artifact.Width > 0 && artifact.Height > 0))
        {
            return OperationResult<DesignConversionModelServerApiRequest>.Fail(
                "DesignConversion local caller package screenshot/render refs must include at least one positive viewport.",
                "DESIGN_CONVERSION_VIEWPORT_REQUIRED");
        }

        var guardedArtifacts = allowValidatedServerOwnedLocalFileRefs
            ? artifacts
                .Where(artifact => !IsCanonicalServerOwnedArtifactUri(artifact.Uri))
                .ToArray()
            : artifacts;
        var artifactRefs = DesignConversionModelServerArtifactRefGuard.Validate(
            guardedArtifacts,
            "DesignConversion local caller package screenshot/render refs");
        if (!artifactRefs.IsSuccess)
            return artifactRefs.MapError<DesignConversionModelServerApiRequest>();

        return OperationResult<DesignConversionModelServerApiRequest>.Ok(
            new DesignConversionModelServerApiRequest(
                RequestId: package.RequestId,
                SourceKind: sourceKind,
                DesignTree: package.DesignTree,
                OriginalTree: package.OriginalTree,
                Html: package.Html,
                Css: package.Css,
                Screenshots: package.Screenshots,
                DesignRenders: package.DesignRenders,
                Budget: package.Budget,
                Metadata: package.Metadata));
    }

    private static Uri BuildRequestUri(Uri baseUrl, string endpointPath)
        => new(new Uri(baseUrl.GetLeftPart(UriPartial.Authority) + "/"), endpointPath.TrimStart('/'));

    private static Dictionary<string, string> ReadHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var header in response.Headers)
            headers[header.Key] = string.Join(",", header.Value);
        foreach (var header in response.Content.Headers)
            headers[header.Key] = string.Join(",", header.Value);
        return headers;
    }

    private static IReadOnlyList<string> ReadRoutePaths(JsonElement root)
    {
        if (!root.TryGetProperty("routePaths", out var routePaths) ||
            routePaths.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return routePaths.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => NormalizeEndpointPath(item.GetString()))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeEndpointPath(string? endpointPath)
    {
        if (string.IsNullOrWhiteSpace(endpointPath))
            return string.Empty;

        var trimmed = endpointPath.Trim();
        return trimmed.StartsWith("/", StringComparison.Ordinal) ? trimmed : "/" + trimmed;
    }

    private static bool IsLoopback(string host)
        => string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
           (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));

    private sealed record ReadyJsonCallerOutboundHeaders(
        string? RouteAdmissionToken,
        string? IdempotencyKey);

    private sealed record ReadyJsonRoute(
        Uri BaseUrl,
        string EndpointPath,
        bool BaseUrlLoopback,
        bool SubmitRouteAdvertised,
        ReadyJsonArtifactUploadRoute ArtifactUpload);

    private sealed record ReadyJsonArtifactUploadRoute(
        string EndpointPath,
        bool Configured,
        bool RouteMapped,
        bool RouteAdvertised,
        long MaxBytes,
        bool Ready);

    private enum LocalArtifactPathKind
    {
        NotLocal,
        InvalidLocal,
        Local
    }

    private sealed record LocalArtifactPathResolution(
        LocalArtifactPathKind Kind,
        string? Path)
    {
        public static LocalArtifactPathResolution NotLocal()
            => new(LocalArtifactPathKind.NotLocal, null);

        public static LocalArtifactPathResolution Invalid()
            => new(LocalArtifactPathKind.InvalidLocal, null);

        public static LocalArtifactPathResolution Local(string path)
            => new(LocalArtifactPathKind.Local, path);
    }

    private sealed record LocalPngUploadSource(
        byte[] Bytes,
        string Sha256,
        int Width,
        int Height);
}
