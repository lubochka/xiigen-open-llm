// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using AgentInfrastructure;

public sealed record DesignConversionExternalVisualProviderConfigInput(
    string? Endpoint,
    string? ProviderName,
    string? ModelId,
    string? ApiKey,
    string ApiKeyHeaderName = "Authorization",
    string ApiKeyScheme = "Bearer",
    IReadOnlyDictionary<string, string>? Metadata = null,
    string? ServerOwnedCostPerRequestUsd = null,
    string? ServerOwnedImageArtifactRoot = null);

public sealed record DesignConversionExternalVisualProviderConfigDescriptor(
    string Status,
    bool IsComplete,
    bool FactoryReady,
    bool RealExternalProviderExecutableWhenNetworkAuthorized,
    bool RealExternalProviderExecuted,
    bool NetworkCallExecuted,
    IReadOnlyList<string> MissingSettingNames,
    IReadOnlyList<string> InvalidSettingNames,
    string EndpointHost,
    string ProviderName,
    string ModelId,
    string ApiKeyHeaderName,
    string ApiKeyScheme,
    IReadOnlyDictionary<string, string> BoundaryMetadata,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings);

internal sealed record DesignConversionExternalVisualProviderApiKeyAuthConfig(
    string ApiKey,
    string ApiKeyHeaderName,
    string ApiKeyScheme,
    bool UsesAuthorizationHeader);

public static class DesignConversionExternalVisualProviderConfigIntake
{
    public const string EndpointSettingName = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_ENDPOINT";
    public const string ProviderSettingName = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_PROVIDER";
    public const string ModelSettingName = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_MODEL";
    public const string ApiKeySettingName = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_API_KEY";
    public const string ApiKeyHeaderSettingName = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_API_KEY_HEADER";
    public const string ApiKeySchemeSettingName = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_API_KEY_SCHEME";
    public const string ServerOwnedCostPerRequestUsdSettingName = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_COST_PER_REQUEST_USD";
    public const string ServerOwnedImageArtifactRootSettingName = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_IMAGE_ARTIFACT_ROOT";
    public const string LegacyEndpointSettingName = "DESIGN_CONVERSION_VISUAL_JUDGE_ENDPOINT";
    public const string LegacyProviderSettingName = "DESIGN_CONVERSION_VISUAL_JUDGE_PROVIDER";
    public const string LegacyModelSettingName = "DESIGN_CONVERSION_VISUAL_JUDGE_MODEL";
    public const string LegacyApiKeySettingName = "DESIGN_CONVERSION_VISUAL_JUDGE_API_KEY";
    public const string DescriptorStatusComplete = "factory_ready_network_not_executed";
    public const string DescriptorStatusIncomplete = "config_incomplete_network_not_executed";
    public const string ServerOwnedCostSourceFixedPerRequest = "server_owned_fixed_per_request";
    public const string ServerOwnedCostSourceUnknownUnpriced = "unknown_unpriced";
    // A visual judge request is expected to cost far below this; larger values are treated as misconfiguration.
    public const decimal MaxServerOwnedCostPerRequestUsd = 10m;
    internal const string EndpointCredentialsForbiddenErrorCode = "EXTERNAL_VISUAL_JUDGE_ENDPOINT_CREDENTIALS_FORBIDDEN";
    internal const string EndpointSchemeForbiddenErrorCode = "EXTERNAL_VISUAL_JUDGE_ENDPOINT_SCHEME_FORBIDDEN";
    internal const string EndpointDestinationForbiddenErrorCode = "EXTERNAL_VISUAL_JUDGE_ENDPOINT_DESTINATION_FORBIDDEN";
    internal const string AuthApiKeyMissingErrorCode = "EXTERNAL_VISUAL_JUDGE_AUTH_API_KEY_MISSING";
    internal const string AuthHeaderInvalidErrorCode = "EXTERNAL_VISUAL_JUDGE_AUTH_HEADER_INVALID";
    internal const string AuthSchemeInvalidErrorCode = "EXTERNAL_VISUAL_JUDGE_AUTH_SCHEME_INVALID";
    internal const string AuthCredentialInvalidErrorCode = "EXTERNAL_VISUAL_JUDGE_AUTH_CREDENTIAL_INVALID";

    public static readonly IReadOnlyList<string> RequiredSettingNames =
    [
        EndpointSettingName,
        ProviderSettingName,
        ModelSettingName,
        ApiKeySettingName,
        ServerOwnedImageArtifactRootSettingName
    ];

    public static DesignConversionExternalVisualProviderConfigInput FromSettings(
        IReadOnlyDictionary<string, string>? settings)
        => new(
            Endpoint: ReadSetting(settings, EndpointSettingName, LegacyEndpointSettingName),
            ProviderName: ReadSetting(settings, ProviderSettingName, LegacyProviderSettingName),
            ModelId: ReadSetting(settings, ModelSettingName, LegacyModelSettingName),
            ApiKey: ReadSetting(settings, ApiKeySettingName, LegacyApiKeySettingName),
            ApiKeyHeaderName: ReadSetting(settings, ApiKeyHeaderSettingName) ?? "Authorization",
            ApiKeyScheme: ReadSetting(settings, ApiKeySchemeSettingName) ?? "Bearer",
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["config_intake_source"] = "settings_dictionary",
                ["primary_endpoint_setting"] = EndpointSettingName,
                ["primary_provider_setting"] = ProviderSettingName,
                ["primary_model_setting"] = ModelSettingName,
                ["primary_api_key_setting"] = ApiKeySettingName,
                ["server_owned_image_artifact_root_setting"] = ServerOwnedImageArtifactRootSettingName
            },
            ServerOwnedCostPerRequestUsd: ReadSetting(settings, ServerOwnedCostPerRequestUsdSettingName),
            ServerOwnedImageArtifactRoot: ReadSetting(settings, ServerOwnedImageArtifactRootSettingName));

    public static DesignConversionExternalVisualProviderConfigDescriptor Describe(
        DesignConversionExternalVisualProviderConfigInput? input)
    {
        var missing = MissingSettingNames(input).ToArray();
        var invalid = InvalidSettingNames(input, out var endpointUri).ToArray();
        var serverOwnedPricing = ResolveServerOwnedPricing(input?.ServerOwnedCostPerRequestUsd);
        var complete = missing.Length == 0 && invalid.Length == 0;
        var imageContentResolverConfigured = !string.IsNullOrWhiteSpace(input?.ServerOwnedImageArtifactRoot);
        var factoryReady = complete && imageContentResolverConfigured;
        var status = factoryReady ? DescriptorStatusComplete : DescriptorStatusIncomplete;
        var endpointInvalid = invalid.Contains(EndpointSettingName, StringComparer.Ordinal);
        var endpointHost = endpointUri is null || endpointInvalid
            ? (missing.Contains(EndpointSettingName, StringComparer.Ordinal) ? "missing" : "invalid")
            : SafeScalar(endpointUri.Host, "external_visual_provider_endpoint_host");
        var providerName = string.IsNullOrWhiteSpace(input?.ProviderName)
            ? "missing"
            : SafeScalar(input!.ProviderName, "external_visual_provider_name");
        var modelId = string.IsNullOrWhiteSpace(input?.ModelId)
            ? "missing"
            : SafeScalar(input!.ModelId, "external_visual_provider_model");
        var headerInvalid = invalid.Contains(ApiKeyHeaderSettingName, StringComparer.Ordinal);
        var headerName = headerInvalid
            ? "invalid"
            : string.IsNullOrWhiteSpace(input?.ApiKeyHeaderName)
            ? "Authorization"
            : SafeScalar(input!.ApiKeyHeaderName, "external_visual_provider_api_key_header");
        var schemeInvalid = invalid.Contains(ApiKeySchemeSettingName, StringComparer.Ordinal);
        var scheme = schemeInvalid
            ? "invalid"
            : string.IsNullOrWhiteSpace(input?.ApiKeyScheme)
            ? "Bearer"
            : SafeScalar(input!.ApiKeyScheme, "external_visual_provider_api_key_scheme");
        var apiKeyConfigured = !string.IsNullOrWhiteSpace(input?.ApiKey);

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["external_visual_provider_config_intake"] = bool.TrueString,
            ["external_visual_provider_config_complete"] = complete.ToString(),
            ["external_visual_provider_factory_ready"] = factoryReady.ToString(),
            ["real_external_provider_executable_when_network_authorized"] = factoryReady.ToString(),
            ["real_external_provider_executed"] = bool.FalseString,
            ["network_call_executed"] = bool.FalseString,
            ["external_visual_provider_image_content_resolver_configured"] = imageContentResolverConfigured.ToString(),
            ["external_visual_provider_image_payload_mode"] = ExternalHttpDesignConversionVisualJudge.PayloadMode,
            ["external_visual_provider_image_artifact_root_setting_name"] = ServerOwnedImageArtifactRootSettingName,
            ["external_visual_provider_image_artifact_root_value_included"] = bool.FalseString,
            ["external_visual_provider_api_key_configured"] = apiKeyConfigured.ToString(),
            ["external_visual_provider_api_key_value_included"] = bool.FalseString,
            ["secret_value_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["server_owned_pricing_configured"] = serverOwnedPricing.Configured.ToString(),
            ["server_owned_pricing_valid"] = serverOwnedPricing.Valid.ToString(),
            ["server_owned_pricing_status"] = serverOwnedPricing.Status,
            ["server_owned_pricing_cost_source"] = serverOwnedPricing.CostSource,
            ["server_owned_pricing_raw_value_included"] = bool.FalseString,
            ["server_owned_pricing_setting_name"] = ServerOwnedCostPerRequestUsdSettingName,
            ["endpoint_host"] = endpointHost,
            ["provider_name"] = providerName,
            ["provider_model"] = modelId,
            ["api_key_header_name"] = headerName,
            ["api_key_scheme"] = scheme,
            ["missing_setting_names"] = missing.Length == 0 ? "none" : string.Join(",", missing),
            ["invalid_setting_names"] = invalid.Length == 0 ? "none" : string.Join(",", invalid),
            ["config_descriptor_status"] = status
        };
        DesignConversionVisualAiProviderSelectionPolicy.CopyPublicMetadata(
            metadata,
            DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
                input?.ProviderName,
                complete));

        return new DesignConversionExternalVisualProviderConfigDescriptor(
            Status: status,
            IsComplete: complete,
            FactoryReady: factoryReady,
            RealExternalProviderExecutableWhenNetworkAuthorized: factoryReady,
            RealExternalProviderExecuted: false,
            NetworkCallExecuted: false,
            MissingSettingNames: missing,
            InvalidSettingNames: invalid,
            EndpointHost: endpointHost,
            ProviderName: providerName,
            ModelId: modelId,
            ApiKeyHeaderName: headerName,
            ApiKeyScheme: scheme,
            BoundaryMetadata: metadata,
            EvidenceRefs: BuildEvidenceRefs(factoryReady, missing, invalid, serverOwnedPricing, imageContentResolverConfigured),
            Warnings: BuildWarnings(factoryReady, missing, invalid, serverOwnedPricing, imageContentResolverConfigured));
    }

    public static OperationResult<DesignConversionExternalVisualJudgeOptions> TryCreateOptions(
        DesignConversionExternalVisualProviderConfigInput? input)
    {
        var descriptor = Describe(input);
        if (!descriptor.IsComplete)
        {
            return OperationResult<DesignConversionExternalVisualJudgeOptions>.Fail(
                "DesignConversion external visual provider config is incomplete; missing/invalid setting names are available in the safe descriptor.",
                "EXTERNAL_VISUAL_PROVIDER_CONFIG_INCOMPLETE");
        }

        var metadata = new Dictionary<string, string>(input!.Metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["external_visual_provider_config_intake"] = bool.TrueString,
            ["external_visual_provider_config_complete"] = bool.TrueString,
            ["external_visual_provider_factory_ready"] = descriptor.FactoryReady.ToString(),
            ["real_external_provider_executable_when_network_authorized"] = descriptor.RealExternalProviderExecutableWhenNetworkAuthorized.ToString(),
            ["real_external_provider_executed"] = bool.FalseString,
            ["network_call_executed"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["external_visual_provider_image_content_resolver_configured"] = descriptor.BoundaryMetadata["external_visual_provider_image_content_resolver_configured"],
            ["external_visual_provider_image_payload_mode"] = ExternalHttpDesignConversionVisualJudge.PayloadMode,
            ["external_visual_provider_image_artifact_root_setting_name"] = ServerOwnedImageArtifactRootSettingName,
            ["external_visual_provider_image_artifact_root_value_included"] = bool.FalseString,
            ["server_owned_pricing_configured"] = descriptor.BoundaryMetadata["server_owned_pricing_configured"],
            ["server_owned_pricing_valid"] = descriptor.BoundaryMetadata["server_owned_pricing_valid"],
            ["server_owned_pricing_status"] = descriptor.BoundaryMetadata["server_owned_pricing_status"],
            ["server_owned_pricing_cost_source"] = descriptor.BoundaryMetadata["server_owned_pricing_cost_source"],
            ["server_owned_pricing_raw_value_included"] = bool.FalseString
        };
        DesignConversionVisualAiProviderSelectionPolicy.CopyPublicMetadata(
            metadata,
            DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
                input.ProviderName,
                configComplete: true));

        return OperationResult<DesignConversionExternalVisualJudgeOptions>.Ok(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri(input.Endpoint!, UriKind.Absolute),
                ProviderName: input.ProviderName!.Trim(),
                ModelId: input.ModelId!.Trim(),
                ApiKey: input.ApiKey,
                ApiKeyHeaderName: string.IsNullOrWhiteSpace(input.ApiKeyHeaderName)
                    ? "Authorization"
                    : input.ApiKeyHeaderName.Trim(),
                ApiKeyScheme: string.IsNullOrWhiteSpace(input.ApiKeyScheme)
                    ? "Bearer"
                    : input.ApiKeyScheme.Trim(),
                Metadata: metadata,
                ServerOwnedCostPerRequestUsd: ResolveServerOwnedPricing(input.ServerOwnedCostPerRequestUsd).CostPerRequestUsd));
    }

    public static OperationResult<DesignConversionExternalVisualAiComparisonProvider> TryCreateComparisonProvider(
        DesignConversionExternalVisualProviderConfigInput? input,
        HttpMessageHandler httpMessageHandler)
    {
        if (httpMessageHandler is null)
        {
            return OperationResult<DesignConversionExternalVisualAiComparisonProvider>.Fail(
                "DesignConversion external visual provider HTTP handler is required.",
                "EXTERNAL_VISUAL_PROVIDER_HTTP_HANDLER_REQUIRED");
        }

        return TryCreateComparisonProvider(
            input,
            CreateHandlerInjectedHttpClient(httpMessageHandler));
    }

    public static OperationResult<DesignConversionExternalVisualAiComparisonProvider> TryCreateComparisonProvider(
        DesignConversionExternalVisualProviderConfigInput? input,
        HttpClient httpClient)
    {
        if (httpClient is null)
        {
            return OperationResult<DesignConversionExternalVisualAiComparisonProvider>.Fail(
                "DesignConversion external visual provider HTTP client is required.",
                "EXTERNAL_VISUAL_PROVIDER_HTTP_CLIENT_REQUIRED");
        }

        var judge = TryCreateVisualJudge(input, httpClient);
        if (!judge.IsSuccess)
            return judge.MapError<DesignConversionExternalVisualAiComparisonProvider>();

        return OperationResult<DesignConversionExternalVisualAiComparisonProvider>.Ok(
            new DesignConversionExternalVisualAiComparisonProvider(judge.Value!));
    }

    public static OperationResult<ExternalHttpDesignConversionVisualJudge> TryCreateVisualJudge(
        DesignConversionExternalVisualProviderConfigInput? input,
        HttpMessageHandler httpMessageHandler)
    {
        if (httpMessageHandler is null)
        {
            return OperationResult<ExternalHttpDesignConversionVisualJudge>.Fail(
                "DesignConversion external visual provider HTTP handler is required.",
                "EXTERNAL_VISUAL_PROVIDER_HTTP_HANDLER_REQUIRED");
        }

        return TryCreateVisualJudge(
            input,
            CreateHandlerInjectedHttpClient(httpMessageHandler));
    }

    public static OperationResult<ExternalHttpDesignConversionVisualJudge> TryCreateSecureProductionVisualJudge(
        DesignConversionExternalVisualProviderConfigInput? input)
    {
        var options = TryCreateOptions(input);
        if (!options.IsSuccess)
            return options.MapError<ExternalHttpDesignConversionVisualJudge>();
        var imageContentResolver = TryCreateImageContentResolver(input);
        if (!imageContentResolver.IsSuccess)
            return imageContentResolver.MapError<ExternalHttpDesignConversionVisualJudge>();

        return OperationResult<ExternalHttpDesignConversionVisualJudge>.Ok(
            new ExternalHttpDesignConversionVisualJudge(
                options.Value!,
                DesignConversionExternalVisualJudgeNetwork.CreateSecureHttpClient(),
                imageContentResolver.Value!));
    }

    public static OperationResult<ExternalHttpDesignConversionVisualJudge> TryCreateVisualJudge(
        DesignConversionExternalVisualProviderConfigInput? input,
        HttpClient httpClient)
    {
        if (httpClient is null)
        {
            return OperationResult<ExternalHttpDesignConversionVisualJudge>.Fail(
                "DesignConversion external visual provider HTTP client is required.",
                "EXTERNAL_VISUAL_PROVIDER_HTTP_CLIENT_REQUIRED");
        }

        var options = TryCreateOptions(input);
        if (!options.IsSuccess)
            return options.MapError<ExternalHttpDesignConversionVisualJudge>();
        var imageContentResolver = TryCreateImageContentResolver(input);
        if (!imageContentResolver.IsSuccess)
            return imageContentResolver.MapError<ExternalHttpDesignConversionVisualJudge>();

        return OperationResult<ExternalHttpDesignConversionVisualJudge>.Ok(
            new ExternalHttpDesignConversionVisualJudge(options.Value!, httpClient, imageContentResolver.Value!));
    }

    private static HttpClient CreateHandlerInjectedHttpClient(HttpMessageHandler httpMessageHandler)
        => new(httpMessageHandler, disposeHandler: false)
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };

    private static OperationResult<IDesignConversionVisualJudgeImageContentResolver> TryCreateImageContentResolver(
        DesignConversionExternalVisualProviderConfigInput? input)
    {
        if (string.IsNullOrWhiteSpace(input?.ServerOwnedImageArtifactRoot))
        {
            return OperationResult<IDesignConversionVisualJudgeImageContentResolver>.Fail(
                "DesignConversion external visual judge requires a server-owned PNG artifact root for inline image content.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_ARTIFACT_ROOT_REQUIRED");
        }

        var root = ValidateImageArtifactRoot(input.ServerOwnedImageArtifactRoot);
        if (!root.IsSuccess)
            return root.MapError<IDesignConversionVisualJudgeImageContentResolver>();

        try
        {
            return OperationResult<IDesignConversionVisualJudgeImageContentResolver>.Ok(
                new ServerOwnedLocalPngComparisonProvider(
                    root.Value!,
                    maxArtifactBytes: ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return OperationResult<IDesignConversionVisualJudgeImageContentResolver>.Fail(
                "DesignConversion external visual judge image artifact root is invalid.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_ARTIFACT_ROOT_INVALID");
        }
    }

    private static OperationResult<string> ValidateImageArtifactRoot(string? root)
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            return OperationResult<string>.Fail(
                "DesignConversion external visual judge requires a server-owned PNG artifact root for inline image content.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_ARTIFACT_ROOT_REQUIRED");
        }

        var trimmed = root.Trim();
        try
        {
            if (trimmed.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ||
                IsNetworkArtifactRoot(trimmed) ||
                !Path.IsPathFullyQualified(trimmed))
            {
                return OperationResult<string>.Fail(
                    "DesignConversion external visual judge image artifact root must be an absolute server-owned local path.",
                    "EXTERNAL_VISUAL_JUDGE_IMAGE_ARTIFACT_ROOT_INVALID");
            }

            return OperationResult<string>.Ok(Path.GetFullPath(trimmed));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return OperationResult<string>.Fail(
                "DesignConversion external visual judge image artifact root is invalid.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_ARTIFACT_ROOT_INVALID");
        }
    }

    private static bool IsNetworkArtifactRoot(string root)
        => root.StartsWith(@"\\", StringComparison.Ordinal) ||
           root.StartsWith("//", StringComparison.Ordinal);

    private static IEnumerable<string> MissingSettingNames(DesignConversionExternalVisualProviderConfigInput? input)
    {
        if (input is null || string.IsNullOrWhiteSpace(input.Endpoint))
            yield return EndpointSettingName;
        if (input is null || string.IsNullOrWhiteSpace(input.ProviderName))
            yield return ProviderSettingName;
        if (input is null || string.IsNullOrWhiteSpace(input.ModelId))
            yield return ModelSettingName;
        if (input is null || string.IsNullOrWhiteSpace(input.ApiKey))
            yield return ApiKeySettingName;
        if (input is null || string.IsNullOrWhiteSpace(input.ServerOwnedImageArtifactRoot))
            yield return ServerOwnedImageArtifactRootSettingName;
    }

    private static IEnumerable<string> InvalidSettingNames(
        DesignConversionExternalVisualProviderConfigInput? input,
        out Uri? endpointUri)
    {
        endpointUri = null;
        var invalid = new List<string>();
        if (input is null)
            return invalid;

        if (!ResolveServerOwnedPricing(input.ServerOwnedCostPerRequestUsd).CanCreateOptions)
            invalid.Add(ServerOwnedCostPerRequestUsdSettingName);
        if (!string.IsNullOrWhiteSpace(input.ServerOwnedImageArtifactRoot) &&
            !ValidateImageArtifactRoot(input.ServerOwnedImageArtifactRoot).IsSuccess)
            invalid.Add(ServerOwnedImageArtifactRootSettingName);
        var authConfig = ValidateApiKeyAuthConfiguration(
            input.ApiKey,
            input.ApiKeyHeaderName,
            input.ApiKeyScheme);
        if (!authConfig.IsSuccess)
        {
            if (string.Equals(authConfig.ErrorCode, AuthHeaderInvalidErrorCode, StringComparison.Ordinal))
                invalid.Add(ApiKeyHeaderSettingName);
            else if (string.Equals(authConfig.ErrorCode, AuthSchemeInvalidErrorCode, StringComparison.Ordinal))
                invalid.Add(ApiKeySchemeSettingName);
            else if (string.Equals(authConfig.ErrorCode, AuthCredentialInvalidErrorCode, StringComparison.Ordinal))
                invalid.Add(ApiKeySettingName);
        }

        if (string.IsNullOrWhiteSpace(input.Endpoint))
            return invalid;

        if (!Uri.TryCreate(input.Endpoint.Trim(), UriKind.Absolute, out endpointUri) ||
            string.IsNullOrWhiteSpace(endpointUri.Host))
        {
            endpointUri = null;
            invalid.Add(EndpointSettingName);
            return invalid;
        }

        var endpointValidation = ValidateEndpoint(endpointUri);
        if (!endpointValidation.IsSuccess)
            invalid.Add(EndpointSettingName);

        return invalid;
    }

    internal static OperationResult<DesignConversionExternalVisualProviderApiKeyAuthConfig> ValidateApiKeyAuthConfiguration(
        string? apiKey,
        string? headerName,
        string? apiKeyScheme)
    {
        var normalizedHeaderName = string.IsNullOrWhiteSpace(headerName)
            ? "Authorization"
            : headerName.Trim();
        if (!IsValidApiKeyRequestHeaderName(normalizedHeaderName))
        {
            return OperationResult<DesignConversionExternalVisualProviderApiKeyAuthConfig>.Fail(
                "External visual judge API key header configuration is invalid.",
                AuthHeaderInvalidErrorCode);
        }

        var usesAuthorizationHeader = UsesAuthorizationApiKeyHeader(normalizedHeaderName);
        var normalizedScheme = string.IsNullOrWhiteSpace(apiKeyScheme)
            ? "Bearer"
            : apiKeyScheme.Trim();
        if (usesAuthorizationHeader && !IsValidAuthorizationApiKeyScheme(normalizedScheme))
        {
            return OperationResult<DesignConversionExternalVisualProviderApiKeyAuthConfig>.Fail(
                "External visual judge API key Authorization scheme configuration is invalid.",
                AuthSchemeInvalidErrorCode);
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return OperationResult<DesignConversionExternalVisualProviderApiKeyAuthConfig>.Fail(
                "External visual judge API key is missing.",
                AuthApiKeyMissingErrorCode);
        }

        if (!IsValidApiKeyCredentialValue(apiKey, normalizedHeaderName, normalizedScheme))
        {
            return OperationResult<DesignConversionExternalVisualProviderApiKeyAuthConfig>.Fail(
                "External visual judge API key credential configuration is invalid.",
                AuthCredentialInvalidErrorCode);
        }

        return OperationResult<DesignConversionExternalVisualProviderApiKeyAuthConfig>.Ok(
            new DesignConversionExternalVisualProviderApiKeyAuthConfig(
                apiKey,
                normalizedHeaderName,
                normalizedScheme,
                usesAuthorizationHeader));
    }

    private static bool IsValidApiKeyRequestHeaderName(string? headerName)
    {
        var candidate = string.IsNullOrWhiteSpace(headerName)
            ? "Authorization"
            : headerName.Trim();
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://visual-provider-config.invalid/");
        try
        {
            return request.Headers.TryAddWithoutValidation(candidate, "redacted-api-key-placeholder");
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool UsesAuthorizationApiKeyHeader(string? headerName)
    {
        var candidate = string.IsNullOrWhiteSpace(headerName)
            ? "Authorization"
            : headerName.Trim();
        return string.Equals(candidate, "Authorization", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidAuthorizationApiKeyScheme(string? apiKeyScheme)
    {
        var candidate = string.IsNullOrWhiteSpace(apiKeyScheme)
            ? "Bearer"
            : apiKeyScheme.Trim();
        try
        {
            _ = new AuthenticationHeaderValue(candidate, "redacted-api-key-placeholder");
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool IsValidApiKeyCredentialValue(
        string apiKey,
        string? headerName,
        string? apiKeyScheme)
    {
        if (ContainsHttpHeaderControlCharacter(apiKey))
            return false;

        return UsesAuthorizationApiKeyHeader(headerName)
            ? IsValidAuthorizationApiKeyCredentialValue(apiKeyScheme, apiKey)
            : IsValidCustomApiKeyHeaderCredentialValue(headerName, apiKey);
    }

    private static bool IsValidAuthorizationApiKeyCredentialValue(
        string? apiKeyScheme,
        string apiKey)
    {
        var scheme = string.IsNullOrWhiteSpace(apiKeyScheme)
            ? "Bearer"
            : apiKeyScheme.Trim();
        try
        {
            _ = new AuthenticationHeaderValue(scheme, apiKey);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool IsValidCustomApiKeyHeaderCredentialValue(
        string? headerName,
        string apiKey)
    {
        var candidate = string.IsNullOrWhiteSpace(headerName)
            ? "Authorization"
            : headerName.Trim();
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://visual-provider-config.invalid/");
        try
        {
            request.Headers.Add(candidate, apiKey);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool ContainsHttpHeaderControlCharacter(string value)
    {
        foreach (var character in value)
        {
            if (char.IsControl(character) || character == '\u007f')
                return true;
        }

        return false;
    }

    internal static OperationResult<bool> ValidateEndpoint(Uri endpoint)
    {
        if (endpoint is null || string.IsNullOrWhiteSpace(endpoint.Host))
        {
            return OperationResult<bool>.Fail(
                "External visual judge endpoint destination is not allowed.",
                EndpointDestinationForbiddenErrorCode);
        }

        if (!string.IsNullOrWhiteSpace(endpoint.UserInfo))
        {
            return OperationResult<bool>.Fail(
                "External visual judge endpoint must not include credentials.",
                EndpointCredentialsForbiddenErrorCode);
        }

        if (!IsAllowedEndpointScheme(endpoint))
        {
            return OperationResult<bool>.Fail(
                "External visual judge endpoint must use HTTPS.",
                EndpointSchemeForbiddenErrorCode);
        }

        if (IsBlockedEndpointDestination(endpoint))
        {
            return OperationResult<bool>.Fail(
                "External visual judge endpoint destination is not allowed.",
                EndpointDestinationForbiddenErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    internal static bool IsAllowedEndpointScheme(Uri endpoint)
        => string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    private static bool IsBlockedEndpointDestination(Uri endpoint)
    {
        var host = NormalizeEndpointHost(endpoint.Host);
        if (string.IsNullOrWhiteSpace(host))
            return true;

        if (IsLocalEndpointName(host))
            return true;

        return TryParseEndpointAddress(host, out var address) &&
               DesignConversionBrowserPngEvidenceInspector.IsBlockedDestinationAddress(address);
    }

    private static bool IsLocalEndpointName(string host)
        => string.Equals(host, "localhost", StringComparison.Ordinal) ||
           host.EndsWith(".localhost", StringComparison.Ordinal) ||
           string.Equals(host, "local", StringComparison.Ordinal) ||
           host.EndsWith(".local", StringComparison.Ordinal);

    private static bool TryParseEndpointAddress(string host, out IPAddress address)
    {
        var candidate = host;
        if (candidate.Length >= 2 && candidate[0] == '[' && candidate[^1] == ']')
            candidate = candidate[1..^1];

        var zoneIndex = candidate.IndexOf('%', StringComparison.Ordinal);
        if (zoneIndex >= 0)
            candidate = candidate[..zoneIndex];

        var parsed = IPAddress.TryParse(candidate, out var parsedAddress);
        address = parsedAddress!;
        return parsed;
    }

    private static string NormalizeEndpointHost(string host)
        => host.Trim().TrimEnd('.').ToLowerInvariant();

    private static IReadOnlyList<string> BuildEvidenceRefs(
        bool factoryReady,
        IReadOnlyList<string> missing,
        IReadOnlyList<string> invalid,
        ServerOwnedPricingState serverOwnedPricing,
        bool imageContentResolverConfigured)
    {
        var refs = new List<string>
        {
            "external_visual_provider_config_intake",
            factoryReady ? DescriptorStatusComplete : DescriptorStatusIncomplete,
            "real_external_provider_executed:false",
            "network_call_executed:false",
            "secret_value_included:false",
            $"image_content_resolver_configured:{imageContentResolverConfigured.ToString().ToLowerInvariant()}",
            $"server_owned_pricing:{serverOwnedPricing.Status}",
            $"cost_source:{serverOwnedPricing.CostSource}"
        };
        refs.AddRange(missing.Select(name => $"missing_setting:{name}"));
        refs.AddRange(invalid.Select(name => $"invalid_setting:{name}"));
        if (!imageContentResolverConfigured)
            refs.Add($"missing_setting:{ServerOwnedImageArtifactRootSettingName}");
        return refs.Distinct(StringComparer.Ordinal).OrderBy(reference => reference, StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> BuildWarnings(
        bool factoryReady,
        IReadOnlyList<string> missing,
        IReadOnlyList<string> invalid,
        ServerOwnedPricingState serverOwnedPricing,
        bool imageContentResolverConfigured)
    {
        if (factoryReady)
        {
            var completeWarnings = new List<string> { "REAL_PROVIDER_NETWORK_EXECUTION_NOT_AUTHORIZED_OR_RUN" };
            if (!serverOwnedPricing.Valid)
                completeWarnings.Add("SERVER_OWNED_PRICING_MAY_BE_UNCONFIGURED_RESULT_COST_NULL");
            return completeWarnings.Distinct(StringComparer.Ordinal).OrderBy(warning => warning, StringComparer.Ordinal).ToArray();
        }

        var warnings = new List<string> { "EXTERNAL_VISUAL_PROVIDER_CONFIG_INCOMPLETE" };
        warnings.AddRange(missing.Select(name => $"MISSING_SETTING:{name}"));
        warnings.AddRange(invalid.Select(name => $"INVALID_SETTING:{name}"));
        if (!imageContentResolverConfigured)
            warnings.Add($"MISSING_SETTING:{ServerOwnedImageArtifactRootSettingName}");
        return warnings.Distinct(StringComparer.Ordinal).OrderBy(warning => warning, StringComparer.Ordinal).ToArray();
    }

    private static string? ReadSetting(IReadOnlyDictionary<string, string>? settings, params string[] names)
    {
        foreach (var name in names)
        {
            if (settings is not null &&
                settings.TryGetValue(name, out var value) &&
                !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string SafeScalar(string? value, string scope)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        return DesignConversionDpoLeakageGuard.SanitizeScalar(value ?? string.Empty, scope, redactions);
    }

    private static ServerOwnedPricingState ResolveServerOwnedPricing(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return new ServerOwnedPricingState(
                Configured: false,
                Valid: false,
                CanCreateOptions: true,
                CostPerRequestUsd: null,
                Status: "unconfigured",
                CostSource: ServerOwnedCostSourceUnknownUnpriced);
        }

        if (!decimal.TryParse(
                rawValue.Trim(),
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var parsed) ||
            parsed < 0m ||
            parsed > MaxServerOwnedCostPerRequestUsd)
        {
            return new ServerOwnedPricingState(
                Configured: true,
                Valid: false,
                CanCreateOptions: false,
                CostPerRequestUsd: null,
                Status: "invalid",
                CostSource: ServerOwnedCostSourceUnknownUnpriced);
        }

        return new ServerOwnedPricingState(
            Configured: true,
            Valid: true,
            CanCreateOptions: true,
            CostPerRequestUsd: parsed,
            Status: "configured",
            CostSource: ServerOwnedCostSourceFixedPerRequest);
    }

    private sealed record ServerOwnedPricingState(
        bool Configured,
        bool Valid,
        bool CanCreateOptions,
        decimal? CostPerRequestUsd,
        string Status,
        string CostSource);
}
