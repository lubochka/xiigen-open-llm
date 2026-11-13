// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;
using Xunit;

public sealed class DesignConversionModelServerImprovementLoopVisualProviderTests
{
    private const string RouteAdmissionCurrentToken =
        "current-visual-provider-route-token-0123456789";
    private const string RouteAdmissionTenantId =
        "tenant-visual-provider-route";

    [Fact]
    public void ExternalVisualProviderConfigIntake_MissingSettingsReturnsKeyNamesOnly()
    {
        var secret = "pass135-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.False(descriptor.RealExternalProviderExecuted);
        Assert.False(descriptor.NetworkCallExecuted);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName,
            DesignConversionExternalVisualProviderConfigIntake.RequiredSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName, descriptor.MissingSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName, descriptor.MissingSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ModelSettingName, descriptor.MissingSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName, descriptor.MissingSettingNames);
        Assert.DoesNotContain(DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName, descriptor.MissingSettingNames);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["external_visual_provider_api_key_configured"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_api_key_value_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["secret_value_included"]);
        Assert.Equal(DesignConversionVisualAiProviderSelectionPolicy.PolicyName, descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey]);
        Assert.Equal(DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders, descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey]);
        Assert.Equal("auto", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal("none", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("not_configured", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal("fallback_unavailable", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]);
        Assert.Equal("provider_config_incomplete", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey]);
        Assert.Equal("3", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey]);
        Assert.Equal("claude,openai,gemini", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.False(options.IsSuccess);

        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, options.ErrorMessage ?? string.Empty, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ExternalVisualProviderConfigIntake_BlankApiKeyStaysMissingAndNotInvalid(string blankApiKey)
    {
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = blankApiKey,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "Authorization",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName] = "Bearer",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(input, handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            descriptor.MissingSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            descriptor.InvalidSettingNames);
        Assert.Empty(descriptor.InvalidSettingNames);
        Assert.False(options.IsSuccess);
        Assert.False(comparisonProvider.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            OptionsErrorCode = options.ErrorCode,
            OptionsErrorMessage = options.ErrorMessage,
            ComparisonProviderErrorCode = comparisonProvider.ErrorCode,
            ComparisonProviderErrorMessage = comparisonProvider.ErrorMessage,
            VisualJudgeErrorCode = visualJudge.ErrorCode,
            VisualJudgeErrorMessage = visualJudge.ErrorMessage
        });
        Assert.DoesNotContain(artifactRoot, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_OpenAiSettingsExposeSelectionPolicyWithoutCloudClaim()
    {
        var secret = "pass-selection-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "openai",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "gpt-vision-safe-test",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.True(descriptor.IsComplete);
        Assert.Equal("openai", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey]);
        Assert.Equal("openai", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("configured_not_called", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Equal("external_configured_not_called", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal("not_needed", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]);
        Assert.Equal("none", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey]);
        Assert.Equal("2", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey]);
        Assert.Equal("claude,gemini", descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);
        Assert.True(options.IsSuccess, options.ErrorMessage);
        Assert.Equal("openai", options.Value!.Metadata![DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]);
        Assert.Equal("external_configured_not_called", options.Value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(bool.FalseString, options.Value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey]);

        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
        Assert.DoesNotContain(artifactRoot, serializedDescriptor, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_MissingImageRootIsIncompleteAndFactoryFailsBeforeNetwork()
    {
        var secret = "missing-root-secret-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(input, handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName,
            descriptor.MissingSettingNames);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_config_complete"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_factory_ready"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["real_external_provider_executable_when_network_authorized"]);
        Assert.False(options.IsSuccess);
        Assert.False(comparisonProvider.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            options.ErrorCode,
            options.ErrorMessage,
            ComparisonProviderErrorCode = comparisonProvider.ErrorCode,
            VisualJudgeErrorCode = visualJudge.ErrorCode
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(@"\\server\share\visual-artifacts")]
    [InlineData("//server/share/visual-artifacts")]
    public void ExternalVisualProviderConfigIntake_NetworkImageRootIsInvalidAndFactoryFailsBeforeNetwork(string networkRoot)
    {
        var secret = "network-root-secret-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] = networkRoot
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName,
            descriptor.InvalidSettingNames);
        Assert.False(options.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            OptionsErrorCode = options.ErrorCode,
            OptionsErrorMessage = options.ErrorMessage,
            VisualJudgeErrorCode = visualJudge.ErrorCode,
            VisualJudgeErrorMessage = visualJudge.ErrorMessage
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(networkRoot, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_RequiresHttpsAndRedactsInvalidEndpoints()
    {
        var secret = "pass-endpoint-safety-" + Guid.NewGuid().ToString("N");
        var publicHttps = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "https://visual-judge.example/v1/judge",
            ProviderName: "openai",
            ModelId: "gpt-vision-public-https",
            ApiKey: secret,
            ServerOwnedImageArtifactRoot: CreateServerOwnedImageArtifactRoot());

        var publicHttpsDescriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(publicHttps);
        var publicHttpsOptions = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(publicHttps);

        Assert.True(publicHttpsDescriptor.IsComplete);
        Assert.True(publicHttpsOptions.IsSuccess, publicHttpsOptions.ErrorMessage);
        Assert.Equal("visual-judge.example", publicHttpsDescriptor.EndpointHost);
        var publicHttpsEndpoint = Assert.IsType<Uri>(publicHttpsOptions.Value!.Endpoint);
        Assert.Equal(Uri.UriSchemeHttps, publicHttpsEndpoint.Scheme);
        Assert.Empty(publicHttpsDescriptor.InvalidSettingNames);

        var invalidCases = new[]
        {
            new
            {
                Endpoint = "http://127.0.0.1:43187/v1/judge",
                Forbidden = new[] { "http://127.0.0.1:43187/v1/judge", "127.0.0.1", "43187" }
            },
            new
            {
                Endpoint = "http://visual-judge.example/v1/judge",
                Forbidden = new[] { "http://visual-judge.example/v1/judge", "visual-judge.example", "/v1/judge" }
            },
            new
            {
                Endpoint = "file:///C:/private/visual.png",
                Forbidden = new[] { "file:///C:/private/visual.png", "C:/private", "visual.png" }
            },
            new
            {
                Endpoint = "ftp://visual-judge.example/v1/judge",
                Forbidden = new[] { "ftp://visual-judge.example/v1/judge", "visual-judge.example", "/v1/judge" }
            },
            new
            {
                Endpoint = "https://user:password@visual-judge.example/v1/judge",
                Forbidden = new[] { "https://user:password@visual-judge.example/v1/judge", "user:password", "password", "visual-judge.example" }
            },
            new
            {
                Endpoint = "https://localhost/v1/judge",
                Forbidden = new[] { "https://localhost/v1/judge", "localhost" }
            },
            new
            {
                Endpoint = "https://localhost./v1/judge",
                Forbidden = new[] { "https://localhost./v1/judge", "localhost." }
            },
            new
            {
                Endpoint = "https://api.localhost/v1/judge",
                Forbidden = new[] { "https://api.localhost/v1/judge", "api.localhost", "localhost" }
            },
            new
            {
                Endpoint = "https://visual.local/v1/judge",
                Forbidden = new[] { "https://visual.local/v1/judge", "visual.local" }
            },
            new
            {
                Endpoint = "https://127.0.0.1/v1/judge",
                Forbidden = new[] { "https://127.0.0.1/v1/judge", "127.0.0.1" }
            },
            new
            {
                Endpoint = "https://10.0.0.1/v1/judge",
                Forbidden = new[] { "https://10.0.0.1/v1/judge", "10.0.0.1" }
            },
            new
            {
                Endpoint = "https://169.254.169.254/latest/meta-data",
                Forbidden = new[] { "https://169.254.169.254/latest/meta-data", "169.254.169.254", "meta-data" }
            },
            new
            {
                Endpoint = "https://192.168.1.1/v1/judge",
                Forbidden = new[] { "https://192.168.1.1/v1/judge", "192.168.1.1" }
            },
            new
            {
                Endpoint = "https://[::1]/v1/judge",
                Forbidden = new[] { "https://[::1]/v1/judge", "[::1]", "::1" }
            },
            new
            {
                Endpoint = "https://[fc00::1]/v1/judge",
                Forbidden = new[] { "https://[fc00::1]/v1/judge", "[fc00::1]", "fc00::1" }
            },
            new
            {
                Endpoint = "https://[fe80::1]/v1/judge",
                Forbidden = new[] { "https://[fe80::1]/v1/judge", "[fe80::1]", "fe80::1" }
            }
        };

        foreach (var testCase in invalidCases)
        {
            var input = new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: testCase.Endpoint,
                ProviderName: "openai",
                ModelId: "gpt-vision-invalid-endpoint",
                ApiKey: secret,
                ServerOwnedImageArtifactRoot: CreateServerOwnedImageArtifactRoot());

            var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
            var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

            Assert.False(descriptor.IsComplete);
            Assert.False(descriptor.FactoryReady);
            Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
            Assert.False(options.IsSuccess);
            Assert.Contains(
                DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName,
                descriptor.InvalidSettingNames);
            Assert.Equal("invalid", descriptor.EndpointHost);
            Assert.Equal("invalid", descriptor.BoundaryMetadata["endpoint_host"]);
            Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_config_complete"]);
            Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["real_external_provider_executable_when_network_authorized"]);

            var serialized = JsonSerializer.Serialize(new
            {
                descriptor,
                options.ErrorCode,
                options.ErrorMessage
            });
            Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(testCase.Endpoint, serialized, StringComparison.OrdinalIgnoreCase);
            foreach (var fragment in testCase.Forbidden)
                Assert.DoesNotContain(fragment, serialized, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_InvalidEndpointReturnsInvalidKeyOnly()
    {
        var secret = "pass135-" + Guid.NewGuid().ToString("N");
        var input = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "not-a-valid-absolute-endpoint",
            ProviderName: "safe-provider",
            ModelId: "safe-model",
            ApiKey: secret,
            ServerOwnedImageArtifactRoot: CreateServerOwnedImageArtifactRoot());

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);

        Assert.False(descriptor.IsComplete);
        Assert.Empty(descriptor.MissingSettingNames);
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName, descriptor.InvalidSettingNames);
        Assert.Equal("invalid", descriptor.EndpointHost);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_config_complete"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["real_external_provider_executable_when_network_authorized"]);

        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_CompleteSettingsBuildsFactoryDescriptorWithoutNetwork()
    {
        var secret = "pass135-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X-Test-Key",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    CreateServerOwnedImageArtifactRoot()
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.True(descriptor.IsComplete);
        Assert.True(descriptor.FactoryReady);
        Assert.True(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.False(descriptor.RealExternalProviderExecuted);
        Assert.False(descriptor.NetworkCallExecuted);
        Assert.Empty(descriptor.MissingSettingNames);
        Assert.Empty(descriptor.InvalidSettingNames);
        Assert.Equal("visual-judge.example", descriptor.EndpointHost);
        Assert.Equal("safe-vision-provider", descriptor.ProviderName);
        Assert.Equal("safe-vision-model", descriptor.ModelId);
        Assert.Equal("X-Test-Key", descriptor.ApiKeyHeaderName);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["external_visual_provider_config_complete"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["external_visual_provider_factory_ready"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["real_external_provider_executable_when_network_authorized"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["external_visual_provider_api_key_value_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["secret_value_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_valid"]);
        Assert.Equal("unconfigured", descriptor.BoundaryMetadata["server_owned_pricing_status"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
            descriptor.BoundaryMetadata["server_owned_pricing_cost_source"]);
        Assert.True(options.IsSuccess, options.ErrorMessage);
        Assert.Equal(secret, options.Value!.ApiKey);
        Assert.Equal("safe-vision-provider", options.Value.ProviderName);
        Assert.Equal("safe-vision-model", options.Value.ModelId);
        Assert.Null(options.Value.ServerOwnedCostPerRequestUsd);
        var configuredFactory = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(
            input,
            new ThrowingNoNetworkHandler());
        Assert.True(configuredFactory.IsSuccess, configuredFactory.ErrorMessage);

        var judge = new ExternalHttpDesignConversionVisualJudge(
            options.Value,
            new HttpClient(new ThrowingNoNetworkHandler()));
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(judge);

        Assert.NotNull(comparisonProvider);
        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/judge", serializedDescriptor, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("X Bad Header")]
    [InlineData("X-Injected\r\nHeader")]
    public void ExternalVisualProviderConfigIntake_InvalidApiKeyHeaderFailsClosedBeforeNetwork(string apiKeyHeaderName)
    {
        var secret = "invalid-header-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = apiKeyHeaderName,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(input, handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName,
            descriptor.InvalidSettingNames);
        Assert.False(options.IsSuccess);
        Assert.False(comparisonProvider.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            OptionsErrorCode = options.ErrorCode,
            OptionsErrorMessage = options.ErrorMessage,
            ComparisonProviderErrorCode = comparisonProvider.ErrorCode,
            ComparisonProviderErrorMessage = comparisonProvider.ErrorMessage,
            VisualJudgeErrorCode = visualJudge.ErrorCode,
            VisualJudgeErrorMessage = visualJudge.ErrorMessage
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(artifactRoot, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(apiKeyHeaderName, serialized, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Bad Scheme")]
    [InlineData("Bearer\r\nInjected")]
    public void ExternalVisualProviderConfigIntake_InvalidAuthorizationSchemeFailsClosedBeforeNetwork(string apiKeyScheme)
    {
        var secret = "invalid-scheme-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "Authorization",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName] = apiKeyScheme,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(input, handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName,
            descriptor.InvalidSettingNames);
        Assert.Equal("invalid", descriptor.ApiKeyScheme);
        Assert.Equal("invalid", descriptor.BoundaryMetadata["api_key_scheme"]);
        Assert.False(options.IsSuccess);
        Assert.False(comparisonProvider.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            OptionsErrorCode = options.ErrorCode,
            OptionsErrorMessage = options.ErrorMessage,
            ComparisonProviderErrorCode = comparisonProvider.ErrorCode,
            ComparisonProviderErrorMessage = comparisonProvider.ErrorMessage,
            VisualJudgeErrorCode = visualJudge.ErrorCode,
            VisualJudgeErrorMessage = visualJudge.ErrorMessage
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(artifactRoot, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(apiKeyScheme, serialized, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("authorization-secret\r\nX-Injected: yes")]
    [InlineData("authorization-secret\nX-Injected: yes")]
    [InlineData("authorization-secret\tbad-tab")]
    [InlineData("authorization-secret\u0001bad-control")]
    [InlineData("authorization-secret\u007fbad-del")]
    public void ExternalVisualProviderConfigIntake_InvalidAuthorizationApiKeyFailsClosedBeforeNetwork(
        string apiKey)
    {
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = apiKey,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "Authorization",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName] = "Bearer",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(input, handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            descriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName,
            descriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName,
            descriptor.InvalidSettingNames);
        Assert.Empty(descriptor.MissingSettingNames);
        Assert.False(options.IsSuccess);
        Assert.False(comparisonProvider.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            OptionsErrorCode = options.ErrorCode,
            OptionsErrorMessage = options.ErrorMessage,
            ComparisonProviderErrorCode = comparisonProvider.ErrorCode,
            ComparisonProviderErrorMessage = comparisonProvider.ErrorMessage,
            VisualJudgeErrorCode = visualJudge.ErrorCode,
            VisualJudgeErrorMessage = visualJudge.ErrorMessage
        });
        AssertExternalProviderConfigDiagnosticsDoNotLeak(serialized, apiKey, artifactRoot);
    }

    [Theory]
    [InlineData("custom-secret\r\nX-Injected: yes")]
    [InlineData("custom-secret\nX-Injected: yes")]
    [InlineData("custom-secret\tbad-tab")]
    [InlineData("custom-secret\u0001bad-control")]
    [InlineData("custom-secret\u007fbad-del")]
    public void ExternalVisualProviderConfigIntake_InvalidCustomHeaderApiKeyFailsClosedBeforeNetwork(
        string apiKey)
    {
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = apiKey,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X-Visual-Key",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName] = "Bearer",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });
        var handler = new CountingNoNetworkHandler();

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var comparisonProvider = DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(input, handler);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(input, handler);

        Assert.False(descriptor.IsComplete);
        Assert.False(descriptor.FactoryReady);
        Assert.False(descriptor.RealExternalProviderExecutableWhenNetworkAuthorized);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            descriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName,
            descriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName,
            descriptor.InvalidSettingNames);
        Assert.Empty(descriptor.MissingSettingNames);
        Assert.False(options.IsSuccess);
        Assert.False(comparisonProvider.IsSuccess);
        Assert.False(visualJudge.IsSuccess);
        Assert.Equal(0, handler.CallCount);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            OptionsErrorCode = options.ErrorCode,
            OptionsErrorMessage = options.ErrorMessage,
            ComparisonProviderErrorCode = comparisonProvider.ErrorCode,
            ComparisonProviderErrorMessage = comparisonProvider.ErrorMessage,
            VisualJudgeErrorCode = visualJudge.ErrorCode,
            VisualJudgeErrorMessage = visualJudge.ErrorMessage
        });
        AssertExternalProviderConfigDiagnosticsDoNotLeak(serialized, apiKey, artifactRoot);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_InvalidHeaderOrSchemeSkipsApiKeyValueAttribution()
    {
        const string injectedApiKey = "dependent-secret\r\nX-Injected: yes";
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var invalidHeaderInput = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = injectedApiKey,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X Bad Header",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });
        var invalidSchemeInput = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = injectedApiKey,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "Authorization",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName] = "Bad Scheme",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });

        var invalidHeaderDescriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(invalidHeaderInput);
        var invalidSchemeDescriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(invalidSchemeInput);

        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName,
            invalidHeaderDescriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            invalidHeaderDescriptor.InvalidSettingNames);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName,
            invalidSchemeDescriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            invalidSchemeDescriptor.InvalidSettingNames);

        var serialized = JsonSerializer.Serialize(new
        {
            invalidHeaderDescriptor,
            invalidSchemeDescriptor
        });
        AssertExternalProviderConfigDiagnosticsDoNotLeak(serialized, injectedApiKey, artifactRoot);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_BlankApiKeyWithInvalidHeaderKeepsMissingKeyAndHeaderInvalid()
    {
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = "   ",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X Bad Header",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName] = "Bad Scheme",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);

        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            descriptor.MissingSettingNames);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName,
            descriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName,
            descriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            descriptor.InvalidSettingNames);
        Assert.Equal("invalid", descriptor.ApiKeyHeaderName);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_BlankApiKeyWithInvalidAuthorizationSchemeKeepsMissingKeyAndSchemeInvalid()
    {
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = "   ",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "Authorization",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName] = "Bad Scheme",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);

        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            descriptor.MissingSettingNames);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName,
            descriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName,
            descriptor.InvalidSettingNames);
        Assert.DoesNotContain(
            DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName,
            descriptor.InvalidSettingNames);
        Assert.Equal("invalid", descriptor.ApiKeyScheme);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_CustomHeaderIgnoresInvalidAuthorizationScheme()
    {
        var secret = "custom-header-ignored-scheme-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X-Visual-Key",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySchemeSettingName] = "Bad Scheme",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    artifactRoot
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);
        var visualJudge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
            input,
            new ThrowingNoNetworkHandler());

        Assert.True(descriptor.IsComplete);
        Assert.True(descriptor.FactoryReady);
        Assert.Empty(descriptor.InvalidSettingNames);
        Assert.Equal("X-Visual-Key", descriptor.ApiKeyHeaderName);
        Assert.True(options.IsSuccess, options.ErrorMessage);
        Assert.Equal("X-Visual-Key", options.Value!.ApiKeyHeaderName);
        Assert.Equal("Bad Scheme", options.Value.ApiKeyScheme);
        Assert.True(visualJudge.IsSuccess, visualJudge.ErrorMessage);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_ServerOwnedPriceSettingReachesOptionsWithoutDescriptorLeak()
    {
        var secret = "pass135-price-" + Guid.NewGuid().ToString("N");
        const string rawPrice = "0.0042";
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName] = rawPrice,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    CreateServerOwnedImageArtifactRoot()
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.True(descriptor.IsComplete);
        Assert.Empty(descriptor.InvalidSettingNames);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["server_owned_pricing_valid"]);
        Assert.Equal("configured", descriptor.BoundaryMetadata["server_owned_pricing_status"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            descriptor.BoundaryMetadata["server_owned_pricing_cost_source"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_raw_value_included"]);
        Assert.Contains("server_owned_pricing:configured", descriptor.EvidenceRefs);
        Assert.True(options.IsSuccess, options.ErrorMessage);
        Assert.Equal(0.0042m, options.Value!.ServerOwnedCostPerRequestUsd);
        Assert.Equal(bool.TrueString, options.Value.Metadata!["server_owned_pricing_configured"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            options.Value.Metadata["server_owned_pricing_cost_source"]);

        var serializedDescriptor = JsonSerializer.Serialize(descriptor);
        Assert.DoesNotContain(secret, serializedDescriptor, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrice, serializedDescriptor, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("not-a-decimal-private-price")]
    [InlineData("10.0001")]
    public void ExternalVisualProviderConfigIntake_InvalidServerOwnedPricesFailBySettingNameWithoutLeak(string rawPrice)
    {
        var secret = "pass135-invalid-price-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName] = rawPrice,
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    CreateServerOwnedImageArtifactRoot()
            });

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(input);
        var options = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(input);

        Assert.False(descriptor.IsComplete);
        Assert.Contains(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName,
            descriptor.InvalidSettingNames);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_valid"]);
        Assert.Equal("invalid", descriptor.BoundaryMetadata["server_owned_pricing_status"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["server_owned_pricing_raw_value_included"]);
        Assert.Contains("server_owned_pricing:invalid", descriptor.EvidenceRefs);
        Assert.Contains(
            $"INVALID_SETTING:{DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName}",
            descriptor.Warnings);
        Assert.False(options.IsSuccess);

        var serialized = JsonSerializer.Serialize(new
        {
            descriptor,
            options.ErrorCode,
            options.ErrorMessage
        });
        Assert.Contains(DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostPerRequestUsdSettingName, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPrice, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void ExternalVisualProviderConfigIntake_SecureProductionVisualJudgeConstructionDoesNotRunNetworkAtStartup()
    {
        var secret = "secure-production-construction-" + Guid.NewGuid().ToString("N");
        var input = DesignConversionExternalVisualProviderConfigIntake.FromSettings(
            new Dictionary<string, string>
            {
                [DesignConversionExternalVisualProviderConfigIntake.EndpointSettingName] = "https://visual-judge.example/v1/judge",
                [DesignConversionExternalVisualProviderConfigIntake.ProviderSettingName] = "safe-vision-provider",
                [DesignConversionExternalVisualProviderConfigIntake.ModelSettingName] = "safe-vision-model",
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeySettingName] = secret,
                [DesignConversionExternalVisualProviderConfigIntake.ApiKeyHeaderSettingName] = "X-Test-Key",
                [DesignConversionExternalVisualProviderConfigIntake.ServerOwnedImageArtifactRootSettingName] =
                    CreateServerOwnedImageArtifactRoot()
            });

        var judge = DesignConversionExternalVisualProviderConfigIntake.TryCreateSecureProductionVisualJudge(input);

        Assert.True(judge.IsSuccess, judge.ErrorMessage);
        Assert.NotNull(judge.Value);
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(judge.Value!);
        Assert.NotNull(comparisonProvider);
        var serialized = JsonSerializer.Serialize(new
        {
            judge.ErrorCode,
            judge.ErrorMessage
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_CustomApiKeyHeaderSendsConfiguredHeaderWithoutAuthorization()
    {
        const string apiKeyHeaderName = "X-Direct-Visual-Key";
        const string ignoredInvalidAuthorizationScheme = "Bad Scheme";
        var providerSecret = "direct-custom-header-secret-" + Guid.NewGuid().ToString("N");
        var artifactRoot = CreateServerOwnedImageArtifactRoot();
        var expected = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            "direct-custom-header-request",
            "expected-shot",
            "expected-render",
            64,
            48);
        var actual = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            "direct-custom-header-request",
            "actual-render",
            "actual-render",
            64,
            48);
        var handler = new CountingNoNetworkHandler();
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://direct-custom-header.visual-judge.example/v1/judge"),
                ProviderName: "direct-custom-header-provider",
                ModelId: "direct-custom-header-model",
                ApiKey: providerSecret,
                ApiKeyHeaderName: apiKeyHeaderName,
                ApiKeyScheme: ignoredInvalidAuthorizationScheme,
                ServerOwnedCostPerRequestUsd: 0.0042m),
            new HttpClient(handler),
            new ServerOwnedLocalPngComparisonProvider(
                artifactRoot,
                ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes));

        var result = await judge.JudgeAsync(new DesignConversionVisualJudgeRequest(
            JudgeRequestId: "direct-custom-header-judge-request",
            RequestId: "direct-custom-header-request",
            ExpectedScreenshot: ToVisualJudgeImage(expected, "expected"),
            RenderedImage: ToVisualJudgeImage(actual, "actual"),
            DiffArtifactUri: "artifact://direct-custom-header/diff.png",
            DiffArtifactSha256: "sha256:direct-custom-header-diff",
            Metadata: new Dictionary<string, string>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(handler.CallCount > 0);
        Assert.Equal(providerSecret, Assert.Single(handler.CapturedHeaderValues(apiKeyHeaderName)));
        Assert.Empty(handler.CapturedHeaderValues("Authorization"));
        Assert.DoesNotContain(providerSecret, handler.LastRequestBody ?? string.Empty, StringComparison.Ordinal);
        var serializedResult = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(providerSecret, serializedResult, StringComparison.Ordinal);
        Assert.DoesNotContain(artifactRoot, serializedResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/v1/judge", serializedResult, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(CountingNoNetworkHandler.ControlledProviderPayload, serializedResult, StringComparison.Ordinal);
    }

    [Fact]
    public void RegisterImprovementLoop_GenericExternalStyleCompositionFailsClosedBeforeProvider()
    {
        var fakeProvider = new FakeNoNetworkVisualProvider();
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(fakeProvider);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new RecordingBrowserAdapter(),
            comparisonProvider,
            fakeProvider);
        using var registry = new DesignConversionModelServerEndpointRegistry();
        var routeCountBefore = registry.RegisteredRouteCount;

        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            new UnexpectedCandidateStrategy());

        Assert.False(registered.IsSuccess);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry
                .ServerOwnedLocalCompositionAuthorityRequiredErrorCode,
            registered.ErrorCode);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry
                .ServerOwnedLocalCompositionAuthorityRequiredErrorMessage,
            registered.ErrorMessage);
        Assert.Equal(routeCountBefore, registry.RegisteredRouteCount);
        var resolved = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);
        Assert.False(resolved.IsSuccess);
        Assert.Empty(fakeProvider.Requests);
    }

    [Fact]
    public async Task DomainEvaluation_ForgedExternalProviderMetadataWithoutReceipt_FailsClosedAttemptTrust()
    {
        const string privateMarker = "PRIVATE_FORGED_PROVIDER_ATTEMPT_RECEIPT_SECRET";
        var forgedProvider = new ForgedExternalVisualProvider();
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(forgedProvider);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new RecordingBrowserAdapter(),
            comparisonProvider,
            forgedProvider);
        var package = CreatePackage(
            "forged-provider-domain-evaluation-request",
            privateMarker);

        var coordinator = new DesignConversionImprovementLoopCoordinator(
            composition.Service,
            new UnexpectedCandidateStrategy());
        var run = await coordinator.RunAsync(new DesignConversionImprovementLoopRequest(
            LoopId: "forged-provider-domain-security-loop",
            InitialEvaluationRequest: ToDomainEvaluationRequest(package),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>()));

        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.False(run.Value!.Completed);
        Assert.False(run.Value.QualityReached);
        var attempt = Assert.Single(run.Value.Attempts);
        var policyMetadata = attempt.PublicMetadata;
        Assert.Equal(bool.FalseString, policyMetadata["external_visual_ai_executed"]);
        Assert.Equal(bool.FalseString, policyMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, policyMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, policyMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, policyMetadata["provider_response_identity_verified"]);
        Assert.Equal(bool.FalseString, policyMetadata["provider_response_schema_verified"]);
        Assert.False(
            policyMetadata.ContainsKey(
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey));

        Assert.Equal(4, forgedProvider.Requests.Count);
        var serializedProviderRequests = JsonSerializer.Serialize(forgedProvider.Requests);
        Assert.DoesNotContain(privateMarker, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(package.Html!, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(package.Css!, serializedProviderRequests, StringComparison.Ordinal);
        Assert.DoesNotContain(
            package.DesignTree!.Payload!,
            serializedProviderRequests,
            StringComparison.Ordinal);
    }










    [Fact]
    public async Task HandleAsync_RemovedExternalVisualProviderFieldFailsClosedBeforeCoordinatorOrNetwork()
    {
        const string privateEndpoint = "https://removed-external-judge.example/v1/private";
        var visualProvider = new FakeNoNetworkVisualProvider();
        var handler = new DesignConversionModelServerImprovementLoopHandler(
            CreateCoordinator(visualProvider));
        var request = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "removed-external-visual-provider-loop",
            InitialPackage: CreatePackage("removed-external-visual-provider-request", "PRIVATE_REMOVED_EXTERNAL_SECRET"),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>());
        var supportedBody = JsonSerializer.Serialize(
            request,
            DesignConversionModelServerHttpHandler.JsonOptions);
        var bodyWithRemovedField = supportedBody[..^1] +
            $",\"externalVisualProviderConfig\":{{\"endpoint\":\"{privateEndpoint}\",\"apiKey\":\"PRIVATE_REMOVED_API_KEY\"}}}}";

        var response = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            Method: "POST",
            Path: DesignConversionModelServerImprovementLoopHandler.EndpointPath,
            BodyJson: bodyWithRemovedField,
            ContentType: "application/json"));

        Assert.Equal(400, response.StatusCode);
        Assert.Empty(visualProvider.Requests);
        Assert.DoesNotContain(privateEndpoint, response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_REMOVED_API_KEY", response.BodyJson, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(response.BodyJson);
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.RemovedExternalVisualProviderConfigErrorCode,
            document.RootElement.GetProperty("errorCode").GetString());
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
    }

    private static DesignConversionInternalEvaluateRequest ToDomainEvaluationRequest(
        DesignConversionModelServerLocalCallerPackage package)
    {
        var designTree = package.DesignTree ?? throw new InvalidOperationException("Design tree fixture is required.");
        var originalTree = package.OriginalTree ?? throw new InvalidOperationException("Original tree fixture is required.");
        var budget = package.Budget ?? throw new InvalidOperationException("Budget fixture is required.");
        var screenshots = (package.Screenshots ?? [])
            .Select(artifact => new DesignRenderArtifact(
                artifact.ArtifactId,
                artifact.Kind,
                artifact.Width,
                artifact.Height,
                artifact.Uri,
                artifact.Sha256,
                artifact.Metadata ?? new Dictionary<string, string>()))
            .ToArray();
        var designRenders = (package.DesignRenders ?? [])
            .Select(artifact => new DesignRenderArtifact(
                artifact.ArtifactId,
                artifact.Kind,
                artifact.Width,
                artifact.Height,
                artifact.Uri,
                artifact.Sha256,
                artifact.Metadata ?? new Dictionary<string, string>()))
            .ToArray();
        var resolutions = screenshots
            .Concat(designRenders)
            .GroupBy(artifact => (artifact.Width, artifact.Height))
            .Select(group => new RenderQaResolution(
                $"viewport-{group.Key.Width}x{group.Key.Height}",
                group.Key.Width,
                group.Key.Height,
                1.0))
            .ToArray();

        return new DesignConversionInternalEvaluateRequest(
            JobId: $"{package.RequestId}:domain-job",
            RequestId: package.RequestId,
            SourceKind: Enum.Parse<DesignConversionSourceKind>(package.SourceKind),
            Html: package.Html ?? string.Empty,
            Css: package.Css ?? string.Empty,
            DesignTreePayload: new DesignTreePayload(
                designTree.RefId,
                designTree.Format ?? designTree.Kind,
                designTree.Payload ?? string.Empty,
                designTree.Sha256 ?? $"sha256:{Sha256Text(designTree.RefId)}",
                designTree.Metadata ?? new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                originalTree.RefId,
                originalTree.Format ?? originalTree.Kind,
                originalTree.Payload ?? string.Empty,
                originalTree.Sha256 ?? $"sha256:{Sha256Text(originalTree.RefId)}",
                originalTree.Metadata ?? new Dictionary<string, string>()),
            Screenshots: screenshots,
            DesignRenders: designRenders,
            Budget: new DesignConversionBudgetMetadata(
                budget.UserId,
                budget.Tier,
                budget.MaxImprovementCandidates,
                budget.MaxRenderQaRuns,
                budget.BudgetUsd,
                budget.Metadata ?? new Dictionary<string, string>(),
                budget.RenderQaPassThreshold,
                budget.MaxImprovementIterations,
                budget.MaxTeacherCalls),
            RenderResolutions: resolutions,
            AiTeacherEvidenceRefs: ["domain-security-seam:no-teacher"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["fixture"] = "forged-provider-domain-security-seam"
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "non-authoritative-domain-test"
            });
    }

    private static DesignConversionImprovementLoopCoordinator CreateCoordinator(
        FakeNoNetworkVisualProvider visualProvider)
    {
        var composition = CreateComposition(visualProvider);
        return new DesignConversionImprovementLoopCoordinator(
            composition.Service,
            new UnexpectedCandidateStrategy());
    }

    private static DesignConversionLocalServiceComposition CreateComposition(
        FakeNoNetworkVisualProvider visualProvider)
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(visualProvider);
        return DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new RecordingBrowserAdapter(),
            comparisonProvider,
            visualProvider);
    }

    private static DesignConversionLocalServiceComposition CreateDeterministicPngComposition(
        string artifactRoot,
        FakeNoNetworkVisualProvider visualProvider)
    {
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(visualProvider);
        return DesignConversionLocalServiceComposition.CreateDeterministicLocalPng(
            artifactRoot,
            comparisonProvider,
            visualProvider);
    }

    private static DesignConversionModelServerRouteAdmission CreateEnabledVisualProviderRouteAdmission()
        => DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: RouteAdmissionCurrentToken,
                idempotencyCapacity: 16,
                idempotencyTtl: TimeSpan.FromMinutes(30),
                tenantId: RouteAdmissionTenantId));

    private static IReadOnlyDictionary<string, string> VisualProviderAdmissionHeaders(string testRoute)
        => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] =
                "Bearer " + RouteAdmissionCurrentToken,
            [DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] =
                "visual-provider-" + testRoute,
            ["x-design-conversion-test-route"] = testRoute
        };

    private static Task<OperationResult<DesignConversionModelServerHttpResponse>> InvokeImprovementLoopRouteAsync(
        DesignConversionModelServerEndpointRegistry registry,
        DesignConversionModelServerImprovementLoopRequest routeRequest,
        string testRoute)
        => registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(routeRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: VisualProviderAdmissionHeaders(testRoute)));

    private static async Task<DesignConversionModelServerLocalCallerPackage> CreatePackageWithServerOwnedPngArtifactsAsync(
        string requestId,
        string privateMarker,
        string artifactRoot)
    {
        var package = CreatePackage(requestId, privateMarker);
        var sourceDesktop = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            "source-desktop",
            "screenshot",
            800,
            600);
        var sourceMobile = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            "source-mobile",
            "screenshot",
            390,
            844);
        var designDesktop = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            "design-desktop",
            "design-render",
            800,
            600);
        var designMobile = await CreateServerOwnedPngArtifactAsync(
            artifactRoot,
            requestId,
            "design-mobile",
            "design-render",
            390,
            844);

        return package with
        {
            Screenshots = [sourceDesktop, sourceMobile],
            DesignRenders = [designDesktop, designMobile]
        };
    }

    private static async Task<DesignConversionModelServerArtifactRef> CreateServerOwnedPngArtifactAsync(
        string artifactRoot,
        string requestId,
        string artifactSuffix,
        string kind,
        int width,
        int height)
    {
        var renderProvider = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot);
        var resolution = new RenderQaResolution(
            ResolutionId: $"{artifactSuffix}-{width}x{height}",
            Width: width,
            Height: height,
            DeviceScaleFactor: 1.0);
        var render = await renderProvider.RenderAsync(new DesignConversionRenderRequest(
            RenderRequestId: $"{requestId}:{artifactSuffix}:server-owned-png",
            RequestId: requestId,
            Html: $"<main class=\"server-owned-png-fixture\">{artifactSuffix}</main>",
            Css: ".server-owned-png-fixture{color:#123456;background:#f8fafc}",
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["fixture_role"] = kind,
                ["fixture_artifact_suffix"] = artifactSuffix
            }));
        Assert.True(render.IsSuccess, render.ErrorMessage);

        var image = render.Value!.Image;
        return new DesignConversionModelServerArtifactRef(
            ArtifactId: $"{requestId}:{artifactSuffix}",
            Kind: kind,
            Width: width,
            Height: height,
            Uri: image.Sha256,
            Sha256: image.Sha256,
            Metadata: image.Metadata
                .Where(pair => !DesignConversionRenderResolutionFactory.IsViewportIdentityMetadataKey(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
    }

    private static void AssertNoRequestPayloadLeak(
        string responseBodyJson,
        DesignConversionModelServerLocalCallerPackage package,
        params string[] forbiddenValues)
    {
        Assert.DoesNotContain(package.Html!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.Css!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.DesignTree!.Payload!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.OriginalTree!.Payload!, responseBodyJson, StringComparison.Ordinal);
        foreach (var forbiddenValue in forbiddenValues)
            Assert.DoesNotContain(forbiddenValue, responseBodyJson, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertExternalProviderConfigDiagnosticsDoNotLeak(
        string serialized,
        string apiKey,
        string artifactRoot)
    {
        Assert.DoesNotContain(apiKey, serialized, StringComparison.Ordinal);
        foreach (var fragment in apiKey.Split(
                     ['\r', '\n', '\t', '\u0001', '\u007f'],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.DoesNotContain(fragment, serialized, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("X-Injected", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(artifactRoot, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/v1/judge", serialized, StringComparison.OrdinalIgnoreCase);
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
            Html: $"<main data-secret=\"{privateMarker}\">Pass135</main>",
            Css: $".pass135{{background-image:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 800, 600),
                CreateArtifact($"{requestId}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:design-desktop", "design-render", 800, 600),
                CreateArtifact($"{requestId}:design-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "pass135-local-provider-route",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
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
            Uri: $"artifact://pass135/{id}.png",
            Sha256: $"sha256:{id}");

    private static RenderQaImageRecord ToVisualJudgeImage(
        DesignConversionModelServerArtifactRef artifact,
        string label)
        => new(
            ImageId: artifact.ArtifactId,
            Label: label,
            Resolution: new RenderQaResolution(
                $"{artifact.ArtifactId}-{artifact.Width.ToString()}x{artifact.Height.ToString()}",
                artifact.Width,
                artifact.Height,
                1.0),
            Uri: artifact.Uri,
            Sha256: artifact.Sha256,
            Metadata: artifact.Metadata ?? new Dictionary<string, string>());

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string CreateServerOwnedImageArtifactRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-external-visual-provider-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private sealed class RecordingBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var render = await new DeterministicLocalDesignConversionPngRenderProvider(Path.Combine(
                    Path.GetTempPath(),
                    "llm_mvp_core",
                    "pass135-visual-provider-recording-browser",
                    Guid.NewGuid().ToString("N")))
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:pass135-recording-browser-png",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}.pass135-recording-browser-proof{color:#123456;background:#f8fafc}",
                        Resolution: request.Resolution,
                        Metadata: request.Metadata),
                    cancellationToken);

            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            var sha = render.Value!.Image.Sha256["sha256:".Length..];
            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: render.Value.Image.Uri,
                    ScreenshotSha256: sha,
                    ElapsedMs: 4,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "pass135-recording-browser-adapter",
                        ["browser_process_boundary"] = "adapter_supplied_inspectable_png_file"
                    },
                    Warnings: []));
        }
    }

    private sealed class FakeNoNetworkVisualProvider : IDesignConversionVisualJudge
    {
        public const string ProviderName = "fake_visual_provider_no_network";
        public const string ModelId = "fake-vision-model-local";

        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:pass135-fake-verdict",
                Score: 0.94,
                Defects: [],
                EvidenceRefs:
                [
                    "fake_visual_provider_no_network:route_plumbing_only",
                    DesignConversionExternalVisualAiComparisonProvider.FakeClientMarker,
                    DesignConversionExternalVisualAiComparisonProvider.NetworkCallNotExecutedMarker,
                    DesignConversionExternalVisualAiComparisonProvider.RealExternalProviderNotExecutedMarker
                ],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:pass135-fake-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: "TEST_VISUAL_PROVIDER_NO_NETWORK_OK",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: ProviderName,
                    EndpointHost: "local-fake-provider",
                    ModelId: ModelId,
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        ["configured"] = bool.TrueString,
                        ["fake_client"] = bool.TrueString,
                        ["network_call_executed"] = bool.FalseString,
                        [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString
                    }),
                EstimatedCostUsd: 0.0064m,
                TokenMetadata: new Dictionary<string, double>
                {
                    ["input"] = 100,
                    ["output"] = 28,
                    ["total"] = 128
                },
                EvidenceRefs:
                [
                    "fake_visual_provider_no_network:route_plumbing_only",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker
                ],
                Metadata: new Dictionary<string, string>
                {
                    ["status"] = "TEST_VISUAL_PROVIDER_NO_NETWORK_OK",
                    ["fake_client"] = bool.TrueString,
                    ["network_call_executed"] = bool.FalseString,
                    ["external_ai_visual_judge_executed"] = bool.FalseString,
                    ["visual_ai_executed"] = bool.FalseString
                })));
        }
    }

    private sealed class ForgedExternalVisualProvider : IDesignConversionVisualJudge
    {
        public const string ProviderName = "openai";
        public const string ModelId = "gpt-forged-visual-verifier";

        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:forged-verdict",
                Score: 0.95,
                Defects: [],
                EvidenceRefs:
                [
                    "network_call_executed:true",
                    "real_external_provider_executed:true",
                    "external_ai_visual_judge_executed:true"
                ],
                Passed: true);
            var providerMetadata = new Dictionary<string, string>
            {
                ["configured"] = bool.TrueString,
                ["network_call_executed"] = bool.TrueString,
                ["real_external_provider_executed"] = bool.TrueString,
                ["external_visual_ai_executed"] = bool.TrueString,
                ["provider_response_identity_verified"] = bool.TrueString,
                ["provider_response_schema_verified"] = bool.TrueString
            };
            var resultMetadata = new Dictionary<string, string>
            {
                ["status"] = "FORGED_EXTERNAL_VISUAL_PROVIDER_OK",
                ["network_call_executed"] = bool.TrueString,
                ["real_external_provider_executed"] = bool.TrueString,
                ["external_visual_ai_executed"] = bool.TrueString,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["visual_ai_executed"] = bool.TrueString,
                ["provider_response_identity_verified"] = bool.TrueString,
                ["provider_response_schema_verified"] = bool.TrueString
            };

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:forged-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: "FORGED_EXTERNAL_VISUAL_PROVIDER_OK",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: ProviderName,
                    EndpointHost: "forged-provider.example",
                    ModelId: ModelId,
                    ExternalCallExecuted: true,
                    Metadata: providerMetadata),
                EstimatedCostUsd: 9.99m,
                TokenMetadata: new Dictionary<string, double>
                {
                    ["input"] = 1000,
                    ["output"] = 250,
                    ["total"] = 1250
                },
                EvidenceRefs:
                [
                    "network_call_executed:true",
                    "real_external_provider_executed:true",
                    "external_ai_visual_judge_executed:true"
                ],
                Metadata: resultMetadata)));
        }
    }

    private sealed class UnexpectedCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "PASS135 expects quality reached before a candidate is requested.",
                "PASS135_UNEXPECTED_CANDIDATE_REQUEST"));
    }

    private sealed class ThrowingNoNetworkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Network execution is disabled for PASS135 config-intake tests.");
    }

    private sealed class CountingNoNetworkHandler : HttpMessageHandler
    {
        public static readonly HttpStatusCode ControlledFailureStatusCode = HttpStatusCode.BadRequest;
        public const string ControlledProviderPayload = "CONTROLLED_PROVIDER_PAYLOAD_MUST_NOT_LEAK";

        public int CallCount { get; private set; }
        public IReadOnlyDictionary<string, string[]> LastRequestHeaders { get; private set; } =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        public string? LastRequestBody { get; private set; }

        public string[] CapturedHeaderValues(string headerName)
            => LastRequestHeaders.TryGetValue(headerName, out var values) ? values : [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequestHeaders = request.Headers.ToDictionary(
                header => header.Key,
                header => header.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase);
            LastRequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(ControlledFailureStatusCode)
            {
                Content = new StringContent(ControlledProviderPayload, Encoding.UTF8, "application/json")
            };
        }
    }
}
