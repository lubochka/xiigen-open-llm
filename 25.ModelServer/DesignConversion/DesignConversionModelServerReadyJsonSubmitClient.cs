// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Net;
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
    public const string ConfiguredFlow47SubmitStatus = "configured_live_local_submit_path_verified";
    public const string ReadyJsonBaseUrlMissingReason = "base_url_missing";
    public const string ReadyJsonBaseUrlInvalidReason = "base_url_invalid";
    public const string ReadyJsonBaseUrlInvalidStatus = "bounded_non_ready_base_url_invalid";
    public const string ReadyJsonSubmitSkippedStatus = "bounded_non_ready_ready_json_submit_skipped_before_http_request";
    public const string Flow47VerificationStateVerifiedByReadyJson = "verified_by_ready_json_loopback_submit_client";
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

        var apiRequest = ToApiRequest(package);
        if (!apiRequest.IsSuccess)
            return apiRequest.MapError<DesignConversionModelServerReadyJsonSubmitResult>();

        var readyRoute = ready.Value!;
        var requestUri = BuildRequestUri(readyRoute.BaseUrl, readyRoute.EndpointPath);
        var bodyJson = JsonSerializer.Serialize(apiRequest.Value!, DesignConversionModelServerHttpHandler.JsonOptions);
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("x-design-conversion-ready-json-submit-client", ClientBoundary);
        request.Headers.TryAddWithoutValidation("x-design-conversion-ready-json-base-url-configured", bool.TrueString);
        request.Headers.TryAddWithoutValidation("x-design-conversion-flow47-submit-status", ConfiguredFlow47SubmitStatus);
        if (!string.IsNullOrWhiteSpace(routeAdmissionToken))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                "Bearer " + routeAdmissionToken.Trim());
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
                idempotencyKey.Trim());
        }

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
            ["default_submit_base_url_configured"] = bool.TrueString,
            ["default_submit_base_url_missing"] = bool.FalseString,
            ["default_submit_base_url_status"] = ConfiguredSubmitStatus,
            ["default_submit_base_url_missing_setting"] = string.Empty,
            ["flow47_verified"] = bool.TrueString,
            ["flow47_verification_authority"] = "ready_json_loopback_submit_client",
            ["flow47_verification_state"] = Flow47VerificationStateVerifiedByReadyJson,
            ["flow47_submit_status"] = ConfiguredFlow47SubmitStatus,
            ["flow47_missing_setting_names"] = string.Empty,
            ["flow47_base_url_source"] = "ready.json",
            ["flow47_endpoint_path"] = ready.EndpointPath,
            ["flow47_submit_request_uri"] = requestUri.ToString(),
            ["flow47_live_local_loopback_verified"] = ready.BaseUrlLoopback.ToString(),
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

            return OperationResult<ReadyJsonRoute>.Ok(
                new ReadyJsonRoute(baseUrl, normalizedEndpointPath, loopback, routeAdvertised));
        }
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
        CancellationToken cancellationToken)
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
        DesignConversionModelServerLocalCallerPackage? package)
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

        var artifactRefs = DesignConversionModelServerArtifactRefGuard.Validate(
            artifacts,
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

    private sealed record ReadyJsonRoute(
        Uri BaseUrl,
        string EndpointPath,
        bool BaseUrlLoopback,
        bool SubmitRouteAdvertised);
}
