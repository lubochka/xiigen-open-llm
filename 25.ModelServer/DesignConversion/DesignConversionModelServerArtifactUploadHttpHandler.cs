// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;

public sealed record DesignConversionModelServerArtifactUploadHttpRequest(
    string Method,
    string Path,
    Stream? Body,
    string? ContentType,
    long? ContentLength,
    IReadOnlyDictionary<string, string>? Headers);

public sealed record DesignConversionModelServerArtifactUploadHttpSuccessResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    int StatusCode,
    string ContractStatus,
    DesignConversionModelServerArtifactRef ArtifactRef,
    long ByteCount,
    string MediaType,
    string Sha256,
    string ArtifactId,
    string Kind,
    int Width,
    int Height,
    bool LiveHttpRouteRegistered,
    bool AspNetEndpointMapped,
    bool DeploymentVerified,
    bool PublicHttpEndpointVerified,
    bool ProductReady,
    bool RawBytesIncluded,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerArtifactUploadHttpHandler
{
    public const string EndpointPath = "/api/design-conversion/artifacts";
    public const string LocalHttpSeam = "transport_neutral_raw_binary_handler_no_socket_no_public_deployment";
    public const string ArtifactIdHeaderName = "x-design-conversion-artifact-id";
    public const string ArtifactKindHeaderName = "x-design-conversion-artifact-kind";
    public const string ArtifactRoleHeaderName = ArtifactKindHeaderName;
    public const string ExpectedSha256HeaderName = "x-design-conversion-expected-sha256";
    public const string WidthHeaderName = "x-design-conversion-artifact-width";
    public const string HeightHeaderName = "x-design-conversion-artifact-height";

    private const string PngMediaType = "image/png";
    private const string Sha256Prefix = "sha256:";
    private const int MaxLogicalHeaderLength = 96;
    private const int MaxHeaderValueLength = 128;

    public DesignConversionModelServerArtifactUploadHttpHandler(
        DesignConversionModelServerArtifactUploadStore uploadStore,
        DesignConversionModelServerRouteAdmission routeAdmission)
    {
        UploadStore = uploadStore ?? throw new ArgumentNullException(nameof(uploadStore));
        RouteAdmission = routeAdmission ?? throw new ArgumentNullException(nameof(routeAdmission));
    }

    public DesignConversionModelServerArtifactUploadStore UploadStore { get; }

    public DesignConversionModelServerRouteAdmission RouteAdmission { get; }

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerArtifactUploadHttpRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Error(
                "UNKNOWN",
                EndpointPath,
                400,
                "REQUEST_REQUIRED",
                "DesignConversion artifact upload HTTP request is required.");
        }

        var method = NormalizeMethod(request.Method);
        var path = NormalizePath(request.Path);
        if (!string.Equals(path, EndpointPath, StringComparison.Ordinal))
        {
            return Error(
                method,
                path,
                404,
                "ROUTE_NOT_FOUND",
                "DesignConversion artifact upload route is not registered.");
        }

        if (!string.Equals(method, "POST", StringComparison.Ordinal))
        {
            return Error(
                method,
                path,
                405,
                "METHOD_NOT_ALLOWED",
                "DesignConversion artifact upload endpoint requires POST.");
        }

        if (!RouteAdmission.Enabled)
        {
            return Error(
                method,
                path,
                503,
                "DESIGN_CONVERSION_ARTIFACT_UPLOAD_ROUTE_ADMISSION_DISABLED",
                "DesignConversion artifact upload route admission must be enabled and ready before accepting uploads.",
                BuildFailClosedAdmissionMetadata(method, path));
        }

        var admissionRequest = new DesignConversionModelServerHttpRequest(
            Method: method,
            Path: path,
            BodyJson: null,
            ContentType: request.ContentType,
            Headers: request.Headers);
        var admission = RouteAdmission.AdmitIdempotentMutation(admissionRequest);
        if (!admission.IsAccepted)
        {
            return Error(
                method,
                path,
                admission.StatusCode,
                admission.ErrorCode,
                admission.ErrorMessage,
                admission.BoundaryMetadata);
        }

        if (!RouteAdmission.Ready)
        {
            return Error(
                method,
                path,
                503,
                "DESIGN_CONVERSION_ARTIFACT_UPLOAD_ROUTE_ADMISSION_NOT_READY",
                "DesignConversion artifact upload route admission must be ready before accepting uploads.",
                admission.BoundaryMetadata);
        }

        if (!IsPngContentType(request.ContentType))
        {
            return Error(
                method,
                path,
                415,
                "UNSUPPORTED_MEDIA_TYPE",
                "DesignConversion artifact upload endpoint requires image/png.",
                admission.BoundaryMetadata);
        }

        var metadata = ParseMetadataHeaders(request.Headers);
        if (!metadata.IsSuccess)
        {
            return Error(
                method,
                path,
                400,
                metadata.ErrorCode,
                metadata.ErrorMessage,
                admission.BoundaryMetadata);
        }

        if (request.ContentLength is <= 0)
        {
            return Error(
                method,
                path,
                400,
                DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode,
                "DesignConversion artifact upload Content-Length must be positive when supplied.",
                admission.BoundaryMetadata);
        }

        if (request.ContentLength > UploadStore.MaxUploadBytes)
        {
            return Error(
                method,
                path,
                413,
                DesignConversionModelServerArtifactUploadStore.OversizeErrorCode,
                "DesignConversion artifact upload Content-Length exceeds the configured size limit.",
                admission.BoundaryMetadata);
        }

        var semanticJson = BuildSemanticJson(metadata.Value!);
        return await RouteAdmission.ExecuteIdempotentMutationOnceAsync(
            admission,
            semanticJson,
            ct => StoreAcceptedAsync(method, path, request.Body, metadata.Value!, admission.BoundaryMetadata, ct),
            conflictAdmission => Error(
                method,
                path,
                409,
                "DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT",
                "DesignConversion artifact upload idempotency key was already used with different canonical metadata.",
                conflictAdmission.BoundaryMetadata),
            invalidJsonMessage => Error(
                method,
                path,
                400,
                "DESIGN_CONVERSION_ARTIFACT_UPLOAD_SEMANTIC_JSON_INVALID",
                invalidJsonMessage,
                admission.BoundaryMetadata),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<DesignConversionModelServerHttpResponse> StoreAcceptedAsync(
        string method,
        string path,
        Stream? body,
        ParsedUploadMetadata metadata,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var storeRequest = new DesignConversionModelServerArtifactUploadRequest(
            ArtifactId: metadata.ArtifactId,
            Kind: metadata.Kind,
            MediaType: PngMediaType,
            ExpectedSha256: metadata.ExpectedSha256,
            Width: metadata.Width,
            Height: metadata.Height);

        var result = await UploadStore.StoreAsync(body!, storeRequest, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return Error(
                method,
                path,
                StoreStatusCode(result.ErrorCode),
                string.IsNullOrWhiteSpace(result.ErrorCode)
                    ? "DESIGN_CONVERSION_ARTIFACT_UPLOAD_FAILED"
                    : result.ErrorCode,
                string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "DesignConversion artifact upload failed."
                    : result.ErrorMessage,
                admissionMetadata,
                uploadStoreInvoked: true);
        }

        var receipt = result.Value!;
        var boundaryMetadata = BuildBoundaryMetadata(
            method,
            path,
            201,
            admissionMetadata,
            uploadStoreInvoked: true);
        var bodyJson = new DesignConversionModelServerArtifactUploadHttpSuccessResponse(
            Endpoint: EndpointPath,
            Method: method,
            HttpSeam: LocalHttpSeam,
            StatusCode: 201,
            ContractStatus: "refs_only_png_upload_receipt_transport_neutral_no_live_route",
            ArtifactRef: receipt.ArtifactRef,
            ByteCount: receipt.ByteCount,
            MediaType: receipt.MediaType,
            Sha256: receipt.Sha256,
            ArtifactId: receipt.ArtifactId,
            Kind: receipt.Kind,
            Width: receipt.Width,
            Height: receipt.Height,
            LiveHttpRouteRegistered: false,
            AspNetEndpointMapped: false,
            DeploymentVerified: false,
            PublicHttpEndpointVerified: false,
            ProductReady: false,
            RawBytesIncluded: false,
            BoundaryMetadata: boundaryMetadata);

        return Json(201, method, path, bodyJson);
    }

    private static OperationResult<ParsedUploadMetadata> ParseMetadataHeaders(
        IReadOnlyDictionary<string, string>? headers)
    {
        var artifactId = ReadSingleHeader(headers, ArtifactIdHeaderName);
        if (!artifactId.IsSuccess)
            return artifactId.MapError<ParsedUploadMetadata>();
        var kind = ReadSingleHeader(headers, ArtifactKindHeaderName);
        if (!kind.IsSuccess)
            return kind.MapError<ParsedUploadMetadata>();
        var expectedSha256 = ReadSingleHeader(headers, ExpectedSha256HeaderName);
        if (!expectedSha256.IsSuccess)
            return expectedSha256.MapError<ParsedUploadMetadata>();
        var width = ReadSingleHeader(headers, WidthHeaderName);
        if (!width.IsSuccess)
            return width.MapError<ParsedUploadMetadata>();
        var height = ReadSingleHeader(headers, HeightHeaderName);
        if (!height.IsSuccess)
            return height.MapError<ParsedUploadMetadata>();

        var normalizedArtifactId = NormalizeLogicalToken(artifactId.Value!, "artifact id");
        if (!normalizedArtifactId.IsSuccess)
            return normalizedArtifactId.MapError<ParsedUploadMetadata>();
        var normalizedKind = NormalizeLogicalToken(kind.Value!, "kind");
        if (!normalizedKind.IsSuccess)
            return normalizedKind.MapError<ParsedUploadMetadata>();
        var normalizedSha = NormalizeExpectedSha256(expectedSha256.Value!);
        if (!normalizedSha.IsSuccess)
            return normalizedSha.MapError<ParsedUploadMetadata>();
        var parsedWidth = ParsePositiveDimension(width.Value!, "width");
        if (!parsedWidth.IsSuccess)
            return parsedWidth.MapError<ParsedUploadMetadata>();
        var parsedHeight = ParsePositiveDimension(height.Value!, "height");
        if (!parsedHeight.IsSuccess)
            return parsedHeight.MapError<ParsedUploadMetadata>();

        return OperationResult<ParsedUploadMetadata>.Ok(
            new ParsedUploadMetadata(
                normalizedArtifactId.Value!,
                normalizedKind.Value!,
                normalizedSha.Value!,
                parsedWidth.Value,
                parsedHeight.Value));
    }

    private static OperationResult<string> ReadSingleHeader(
        IReadOnlyDictionary<string, string>? headers,
        string headerName)
    {
        KeyValuePair<string, string>? match = null;
        var count = 0;
        if (headers is not null)
        {
            foreach (var pair in headers)
            {
                if (!string.Equals(pair.Key, headerName, StringComparison.OrdinalIgnoreCase))
                    continue;

                match = pair;
                count++;
            }
        }

        if (count == 0)
        {
            return OperationResult<string>.Fail(
                "DesignConversion artifact upload required metadata header is missing.",
                DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode);
        }

        if (count != 1)
        {
            return OperationResult<string>.Fail(
                "DesignConversion artifact upload metadata header must not be duplicated.",
                DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode);
        }

        var value = match!.Value.Value;
        if (string.IsNullOrEmpty(value) ||
            value.Length > MaxHeaderValueLength ||
            value.Contains(',', StringComparison.Ordinal) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(
                "DesignConversion artifact upload metadata header value is not canonical.",
                DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode);
        }

        return OperationResult<string>.Ok(value);
    }

    private static OperationResult<string> NormalizeLogicalToken(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > MaxLogicalHeaderLength ||
            !value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_'))
        {
            return OperationResult<string>.Fail(
                $"DesignConversion artifact upload {fieldName} must be a bounded safe logical token.",
                DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode);
        }

        return OperationResult<string>.Ok(value);
    }

    private static OperationResult<string> NormalizeExpectedSha256(string value)
    {
        if (value.Length != Sha256Prefix.Length + 64 ||
            !value.StartsWith(Sha256Prefix, StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(
                "DesignConversion artifact upload expected SHA-256 must use canonical lowercase sha256 form.",
                DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode);
        }

        for (var index = Sha256Prefix.Length; index < value.Length; index++)
        {
            var character = value[index];
            if (character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f'))
            {
                return OperationResult<string>.Fail(
                    "DesignConversion artifact upload expected SHA-256 must use canonical lowercase sha256 form.",
                    DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode);
            }
        }

        return OperationResult<string>.Ok(value);
    }

    private static OperationResult<int> ParsePositiveDimension(string value, string fieldName)
    {
        if (value.Length > 10 ||
            !int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ||
            parsed <= 0)
        {
            return OperationResult<int>.Fail(
                $"DesignConversion artifact upload {fieldName} must be a positive integer.",
                DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode);
        }

        return OperationResult<int>.Ok(parsed);
    }

    private static string BuildSemanticJson(ParsedUploadMetadata metadata)
        => JsonSerializer.Serialize(
            new ArtifactUploadSemanticRequest(
                metadata.ArtifactId,
                metadata.Kind,
                PngMediaType,
                metadata.ExpectedSha256,
                metadata.Width,
                metadata.Height),
            DesignConversionModelServerHttpHandler.JsonOptions);

    private static bool IsPngContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        var mediaType = contentType.Split(';', 2)[0].Trim();
        return string.Equals(mediaType, PngMediaType, StringComparison.OrdinalIgnoreCase);
    }

    private static int StoreStatusCode(string? errorCode)
        => errorCode switch
        {
            DesignConversionModelServerArtifactUploadStore.MetadataInvalidErrorCode => 400,
            DesignConversionModelServerArtifactUploadStore.OversizeErrorCode => 413,
            DesignConversionModelServerArtifactUploadStore.HashMismatchErrorCode => 422,
            DesignConversionModelServerArtifactUploadStore.PngValidationErrorCode => 422,
            DesignConversionModelServerArtifactUploadStore.PublishRaceErrorCode => 409,
            "CANCELLED" => 408,
            DesignConversionModelServerArtifactUploadStore.IoErrorCode => 503,
            _ => 503
        };

    private static DesignConversionModelServerHttpResponse Error(
        string method,
        string path,
        int statusCode,
        string errorCode,
        string errorMessage,
        IReadOnlyDictionary<string, string>? boundaryMetadata = null,
        bool uploadStoreInvoked = false)
    {
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: EndpointPath,
            Method: string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            HttpSeam: LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            BoundaryMetadata: BuildBoundaryMetadata(
                method,
                path,
                statusCode,
                boundaryMetadata,
                uploadStoreInvoked));

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
            ["x-design-conversion-endpoint"] = EndpointPath,
            ["x-design-conversion-http-seam"] = LocalHttpSeam,
            ["x-design-conversion-live-route-registered"] = bool.FalseString,
            ["x-design-conversion-aspnet-endpoint-mapped"] = bool.FalseString,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-product-ready"] = bool.FalseString,
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

    private static IReadOnlyDictionary<string, string> BuildFailClosedAdmissionMetadata(
        string method,
        string path)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["route_admission_enabled"] = bool.FalseString,
            ["route_admission_configuration_status"] = "disabled",
            ["route_admission_authorized"] = bool.FalseString,
            ["route_admission_status_code"] = "503",
            ["route_admission_error_ref"] = "route_admission_disabled",
            ["idempotency_required"] = bool.FalseString,
            ["idempotency_key_accepted"] = bool.FalseString,
            ["route_admission_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["route_admission_path"] = NormalizePath(path),
            ["token_value_exposed"] = bool.FalseString,
            ["token_hash_exposed"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["runtime_invoked"] = bool.FalseString,
            ["persistence_invoked"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(
        string method,
        string path,
        int statusCode,
        IReadOnlyDictionary<string, string>? admissionMetadata,
        bool uploadStoreInvoked)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in admissionMetadata ?? new Dictionary<string, string>())
            metadata[pair.Key] = SanitizeMetadataValue(pair.Value);

        metadata["http_endpoint_shape"] = "POST /api/design-conversion/artifacts";
        metadata["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method;
        metadata["http_path"] = NormalizePath(path);
        metadata["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture);
        metadata["http_content_type"] = "application/json; charset=utf-8";
        metadata["local_http_host_seam"] = LocalHttpSeam;
        metadata["transport_neutral_raw_binary_handler"] = bool.TrueString;
        metadata["live_http_route_registered"] = bool.FalseString;
        metadata["aspnet_endpoint_mapped"] = bool.FalseString;
        metadata["socket_opened"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        metadata["runtime_invoked"] = bool.FalseString;
        metadata["upload_store_invoked"] = uploadStoreInvoked.ToString();
        metadata["persistence_invoked"] = uploadStoreInvoked.ToString();
        metadata["browser_runtime_invoked"] = bool.FalseString;
        metadata["model_provider_invoked"] = bool.FalseString;
        metadata["request_body_base64_encoded"] = bool.FalseString;
        metadata["request_body_buffered"] = bool.FalseString;
        metadata["request_body_read_to_end"] = bool.FalseString;
        metadata["raw_bytes_included"] = bool.FalseString;
        metadata["raw_bytes_logged"] = bool.FalseString;
        metadata["content_length_semantic_idempotency_included"] = bool.FalseString;
        metadata["artifact_sha256_digest_exposed_in_metadata"] = bool.FalseString;
        metadata["artifact_ref_returned_on_success"] = (statusCode is >= 200 and < 300).ToString();
        metadata["public_deployment_product_readiness"] = bool.FalseString;
        return metadata;
    }

    private static string SanitizeMetadataValue(string? value)
        => string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Contains("sha256:", StringComparison.OrdinalIgnoreCase)
                ? "sha256_ref_redacted"
                : value;

    private static string NormalizeMethod(string? method)
        => string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method.Trim().ToUpperInvariant();

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }

    private sealed record ParsedUploadMetadata(
        string ArtifactId,
        string Kind,
        string ExpectedSha256,
        int Width,
        int Height);

    private sealed record ArtifactUploadSemanticRequest(
        string ArtifactId,
        string Kind,
        string MediaType,
        string ExpectedSha256,
        int Width,
        int Height);
}
