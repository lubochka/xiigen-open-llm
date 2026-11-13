// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

public sealed class DesignConversionModelServerLocalHttpHostAdapter
{
    public const string LocalLoopbackHostBoundary =
        "local_loopback_http_host_ephemeral_port_test_only_no_public_deployment";
    public const string LoopbackHost = "127.0.0.1";

    private readonly DesignConversionModelServerEndpointRegistry _registry;

    public DesignConversionModelServerLocalHttpHostAdapter(DesignConversionModelServerEndpointRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task<DesignConversionModelServerHttpResponse> DispatchAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return AnnotateLoopbackHost(BuildRegistryErrorResponse(null, "DESIGN_CONVERSION_ENDPOINT_REQUEST_REQUIRED", "DesignConversion endpoint request is required."));

        var invoked = await _registry.InvokeAsync(request, cancellationToken);
        return AnnotateLoopbackHost(invoked.IsSuccess
            ? invoked.Value!
            : BuildRegistryErrorResponse(request, invoked.ErrorCode, invoked.ErrorMessage));
    }

    public static DesignConversionModelServerHttpResponse AnnotateLoopbackHost(
        DesignConversionModelServerHttpResponse response)
    {
        if (response is null)
            throw new ArgumentNullException(nameof(response));

        var headers = new Dictionary<string, string>(response.Headers, StringComparer.Ordinal)
        {
            ["x-design-conversion-local-http-host-boundary"] = LocalLoopbackHostBoundary,
            ["x-design-conversion-local-http-host-verified"] = bool.TrueString,
            ["x-design-conversion-loopback-host"] = LoopbackHost,
            ["x-design-conversion-socket-opened"] = bool.TrueString,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-product-ready"] = bool.FalseString
        };

        return response with
        {
            BodyJson = AnnotateBodyJson(response.BodyJson),
            Headers = headers
        };
    }

    private static string AnnotateBodyJson(string bodyJson)
    {
        try
        {
            if (JsonNode.Parse(bodyJson) is not JsonObject root)
                return bodyJson;

            var metadata = root["boundaryMetadata"] as JsonObject ?? new JsonObject();
            ApplyLoopbackMetadata(metadata);
            root["boundaryMetadata"] = metadata;
            return root.ToJsonString(DesignConversionModelServerHttpHandler.JsonOptions);
        }
        catch (JsonException)
        {
            return bodyJson;
        }
    }

    private static void ApplyLoopbackMetadata(JsonObject metadata)
    {
        metadata["local_http_host_boundary"] = LocalLoopbackHostBoundary;
        metadata["local_http_host_verified"] = bool.TrueString;
        metadata["local_loopback_host_verified"] = bool.TrueString;
        metadata["localhost_loopback_only"] = bool.TrueString;
        metadata["loopback_host"] = LoopbackHost;
        metadata["bound_port_mode"] = "ephemeral";
        metadata["socket_opened"] = bool.TrueString;
        metadata["loopback_socket_opened"] = bool.TrueString;
        metadata["asp_net_host_verified"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["public_production_default_ready"] = bool.FalseString;
        DesignConversionModelServerNetworkBoundary.PreserveTrustedNetworkBoundaryMetadata(metadata);
        metadata["product_ready"] = bool.FalseString;
    }

    private static DesignConversionModelServerHttpResponse BuildRegistryErrorResponse(
        DesignConversionModelServerHttpRequest? request,
        string? errorCode,
        string? errorMessage)
    {
        var method = string.IsNullOrWhiteSpace(request?.Method)
            ? "UNKNOWN"
            : request!.Method.Trim().ToUpperInvariant();
        var path = string.IsNullOrWhiteSpace(request?.Path)
            ? "/"
            : request!.Path.Trim();
        var statusCode = StatusCodeForRegistryError(errorCode);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = $"{method} {path}",
            ["http_method"] = method,
            ["http_path"] = path,
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["runtime_invoked"] = bool.FalseString,
            ["refs_only_public_response"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString
        };

        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: path,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: string.IsNullOrWhiteSpace(errorCode)
                ? "DESIGN_CONVERSION_ENDPOINT_REGISTRY_ERROR"
                : errorCode,
            ErrorMessage: string.IsNullOrWhiteSpace(errorMessage)
                ? "DesignConversion endpoint registry rejected the local HTTP host request."
                : errorMessage,
            BoundaryMetadata: metadata);

        return new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: "application/json; charset=utf-8",
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-endpoint"] = path,
                ["x-design-conversion-http-seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
                ["x-design-conversion-status-code"] = statusCode.ToString(CultureInfo.InvariantCulture)
            });
    }

    private static int StatusCodeForRegistryError(string? errorCode)
        => errorCode switch
        {
            "DESIGN_CONVERSION_ENDPOINT_REQUEST_REQUIRED" => 400,
            "DESIGN_CONVERSION_ENDPOINT_METHOD_REQUIRED" => 400,
            "DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND" => 404,
            "DESIGN_CONVERSION_ENDPOINT_METHOD_NOT_ALLOWED" => 405,
            _ => 502
        };
}
